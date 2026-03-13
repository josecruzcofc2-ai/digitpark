using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using DigitPark.Localization;
using DigitPark.Services.Firebase;

namespace DigitPark.Services
{
    /// <summary>
    /// Servicio de almacenamiento local de notificaciones.
    /// Persiste notificaciones en PlayerPrefs como JSON.
    /// Singleton DontDestroyOnLoad para acceso global.
    ///
    /// Categorías:
    /// - Social: friend_request, friend_accepted
    /// - Games: challenge, tournament_start, tournament_result
    /// - Rewards: daily_reward, promo
    /// </summary>
    public class NotificationStorageService : MonoBehaviour
    {
        public static NotificationStorageService Instance { get; private set; }

        private const string STORAGE_KEY = "NotificationStorage";
        private const int MAX_NOTIFICATIONS = 100;

        private List<StoredNotification> _notifications = new List<StoredNotification>();

        // Eventos
        public event Action OnNotificationsChanged;
        public event Action<int> OnUnreadCountChanged;

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadFromDisk();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Agrega una nueva notificación al almacenamiento
        /// </summary>
        public void AddNotification(string title, string body, string type,
            string senderId = "", string senderName = "",
            string action = "", string targetId = "",
            Dictionary<string, string> extraData = null)
        {
            // SEC-M09: Sanitize user-provided content to strip rich text / HTML / TMP tags
            title = Regex.Replace(title ?? "", "<.*?>", "");
            body = Regex.Replace(body ?? "", "<.*?>", "");
            senderName = Regex.Replace(senderName ?? "", "<.*?>", "");

            var notification = new StoredNotification
            {
                id = Guid.NewGuid().ToString("N"),
                title = title,
                body = body,
                type = type,
                senderId = senderId,
                senderName = senderName,
                action = action,
                targetId = targetId,
                timestamp = DateTime.UtcNow.ToBinary(),
                isRead = false,
                extraData = extraData != null ? new SerializableDictionary(extraData) : null
            };

            _notifications.Insert(0, notification);

            // Limitar cantidad
            if (_notifications.Count > MAX_NOTIFICATIONS)
            {
                _notifications.RemoveRange(MAX_NOTIFICATIONS, _notifications.Count - MAX_NOTIFICATIONS);
            }

            SaveToDisk();
            OnNotificationsChanged?.Invoke();
            OnUnreadCountChanged?.Invoke(GetUnreadCount());

            Debug.Log($"[NotificationStorage] Notificación agregada: {type} - {title}");
        }

        /// <summary>
        /// Obtiene todas las notificaciones ordenadas por fecha (más recientes primero)
        /// </summary>
        public List<StoredNotification> GetAll()
        {
            return new List<StoredNotification>(_notifications);
        }

        /// <summary>
        /// Filtra notificaciones por categoría
        /// </summary>
        public List<StoredNotification> GetByCategory(NotificationCategory category)
        {
            if (category == NotificationCategory.All)
                return GetAll();

            return _notifications.Where(n => GetCategory(n.type) == category).ToList();
        }

        /// <summary>
        /// Marca una notificación como leída
        /// </summary>
        public void MarkAsRead(string notificationId)
        {
            var notification = _notifications.Find(n => n.id == notificationId);
            if (notification != null && !notification.isRead)
            {
                notification.isRead = true;
                SaveToDisk();
                OnNotificationsChanged?.Invoke();
                OnUnreadCountChanged?.Invoke(GetUnreadCount());
            }
        }

        /// <summary>
        /// Marca todas las notificaciones como leídas
        /// </summary>
        public void MarkAllAsRead()
        {
            bool changed = false;
            foreach (var n in _notifications)
            {
                if (!n.isRead)
                {
                    n.isRead = true;
                    changed = true;
                }
            }

            if (changed)
            {
                SaveToDisk();
                OnNotificationsChanged?.Invoke();
                OnUnreadCountChanged?.Invoke(0);
            }
        }

        /// <summary>
        /// Elimina una notificación
        /// </summary>
        public void Delete(string notificationId)
        {
            int removed = _notifications.RemoveAll(n => n.id == notificationId);
            if (removed > 0)
            {
                SaveToDisk();
                OnNotificationsChanged?.Invoke();
                OnUnreadCountChanged?.Invoke(GetUnreadCount());
            }
        }

        /// <summary>
        /// Elimina todas las notificaciones
        /// </summary>
        public void ClearAll()
        {
            _notifications.Clear();
            SaveToDisk();
            OnNotificationsChanged?.Invoke();
            OnUnreadCountChanged?.Invoke(0);
        }

        /// <summary>
        /// Obtiene cantidad de notificaciones no leídas
        /// </summary>
        public int GetUnreadCount()
        {
            return _notifications.Count(n => !n.isRead);
        }

        /// <summary>
        /// Obtiene cantidad total de notificaciones
        /// </summary>
        public int GetTotalCount()
        {
            return _notifications.Count;
        }

        /// <summary>
        /// Determina la categoría de un tipo de notificación
        /// </summary>
        public static NotificationCategory GetCategory(string type)
        {
            switch (type)
            {
                case "friend_request":
                case "friend_accepted":
                    return NotificationCategory.Social;

                case "challenge":
                case "tournament_start":
                case "tournament_result":
                    return NotificationCategory.Games;

                case "daily_reward":
                case "promo":
                    return NotificationCategory.Rewards;

                default:
                    return NotificationCategory.All;
            }
        }

        #endregion

        #region Persistence

        private void SaveToDisk()
        {
            try
            {
                var wrapper = new NotificationListWrapper { notifications = _notifications };
                string json = JsonUtility.ToJson(wrapper);
                PlayerPrefs.SetString(STORAGE_KEY, json);
                PlayerPrefs.Save();

                // Sync to Firebase
                _ = SyncNotificationsToFirebase(json).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                        Debug.LogWarning($"[NotificationStorage] Firebase sync failed: {t.Exception?.GetBaseException().Message}");
                }, System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception e)
            {
                Debug.LogError($"[NotificationStorage] Error guardando: {e.Message}");
            }
        }

        private async Task SyncNotificationsToFirebase(string jsonData)
        {
            var playerData = AuthenticationService.Instance?.GetCurrentPlayerData();
            if (playerData == null) return;

            try
            {
                var updates = new Dictionary<string, object>
                {
                    { "notificationHistory", jsonData }
                };

                if (DatabaseService.Instance != null)
                {
                    await DatabaseService.Instance.UpdatePlayerFields(playerData.userId, updates);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[NotificationStorage] Error syncing to Firebase: {e.Message}");
            }
        }

        private void LoadFromDisk()
        {
            try
            {
                string json = PlayerPrefs.GetString(STORAGE_KEY, "");
                if (!string.IsNullOrEmpty(json))
                {
                    var wrapper = JsonUtility.FromJson<NotificationListWrapper>(json);
                    _notifications = wrapper?.notifications ?? new List<StoredNotification>();

                    // Ordenar por timestamp descendente
                    _notifications.Sort((a, b) => b.timestamp.CompareTo(a.timestamp));

                    Debug.Log($"[NotificationStorage] Cargadas {_notifications.Count} notificaciones");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[NotificationStorage] Error cargando: {e.Message}");
                _notifications = new List<StoredNotification>();
            }
        }

        #endregion
    }

    #region Data Models

    public enum NotificationCategory
    {
        All,
        Social,
        Games,
        Rewards
    }

    [Serializable]
    public class StoredNotification
    {
        public string id;
        public string title;
        public string body;
        public string type;           // friend_request, challenge, tournament_start, etc.
        public string senderId;
        public string senderName;
        public string action;
        public string targetId;
        public long timestamp;        // DateTime.ToBinary()
        public bool isRead;
        public SerializableDictionary extraData;

        public DateTime GetDateTime()
        {
            return DateTime.FromBinary(timestamp);
        }

        /// <summary>
        /// Devuelve texto relativo: "Hace 5 min", "Hace 2h", "Ayer", etc.
        /// </summary>
        public string GetRelativeTime()
        {
            // Use UTC consistently — GetDateTime() returns UTC (stored via UtcNow.ToBinary())
            var now = DateTime.UtcNow;
            var dt = GetDateTime(); // Kind=Utc
            var diff = now - dt;

            if (diff.TotalMinutes < 1) return AutoLocalizer.Get("time_now");
            if (diff.TotalMinutes < 60) return AutoLocalizer.Get("time_ago_minutes", (int)diff.TotalMinutes);
            if (diff.TotalHours < 24) return AutoLocalizer.Get("time_ago_hours", (int)diff.TotalHours);
            if (diff.TotalDays < 2) return AutoLocalizer.Get("time_yesterday");
            if (diff.TotalDays < 7) return AutoLocalizer.Get("time_ago_days", (int)diff.TotalDays);
            return dt.ToLocalTime().ToString("dd/MM/yyyy");
        }

        /// <summary>
        /// Devuelve el grupo temporal para separadores: "Hoy", "Ayer", "Esta semana", "Anteriores"
        /// </summary>
        public string GetTimeGroup()
        {
            // Compare local dates for human-readable groups ("Today", "Yesterday")
            var now = DateTime.Now;
            var dt = GetDateTime().ToLocalTime(); // Convert UTC→Local for date comparison
            var diff = now - dt;

            if (dt.Date == now.Date) return AutoLocalizer.Get("time_today");
            if (dt.Date == now.Date.AddDays(-1)) return AutoLocalizer.Get("time_yesterday");
            if (diff.TotalDays < 7) return AutoLocalizer.Get("time_this_week");
            return AutoLocalizer.Get("time_earlier");
        }
    }

    [Serializable]
    public class SerializableDictionary
    {
        public List<string> keys = new List<string>();
        public List<string> values = new List<string>();

        public SerializableDictionary() { }

        public SerializableDictionary(Dictionary<string, string> dict)
        {
            if (dict == null) return;
            foreach (var kvp in dict)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        public Dictionary<string, string> ToDictionary()
        {
            var dict = new Dictionary<string, string>();
            for (int i = 0; i < Mathf.Min(keys.Count, values.Count); i++)
            {
                dict[keys[i]] = values[i];
            }
            return dict;
        }
    }

    [Serializable]
    public class NotificationListWrapper
    {
        public List<StoredNotification> notifications;
    }

    #endregion
}
