using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
#if FIREBASE_MESSAGING
using Firebase.Messaging;
#endif
using DigitPark.Data;
using DigitPark.Managers;
using DigitPark.Services;

namespace DigitPark.Services.Firebase
{
    /// <summary>
    /// Servicio de Push Notifications usando Firebase Cloud Messaging (FCM)
    ///
    /// CONFIGURACIÓN REQUERIDA:
    /// 1. Instalar Firebase Messaging desde Package Manager
    /// 2. Descomentar "using Firebase.Messaging;" arriba
    /// 3. Definir FIREBASE_MESSAGING en Player Settings > Scripting Define Symbols
    /// 4. Habilitar Cloud Messaging en Firebase Console
    /// 5. iOS: Configurar APNs en Firebase Console y Xcode
    /// 6. Android: El google-services.json ya debería tener la configuración
    ///
    /// TIPOS DE NOTIFICACIONES:
    /// - friend_request: Nueva solicitud de amistad
    /// - friend_accepted: Solicitud aceptada
    /// - challenge: Reto de un amigo
    /// - tournament_start: Torneo por comenzar
    /// - tournament_result: Resultados del torneo
    /// - daily_reward: Recompensa diaria disponible
    /// - promo: Promociones y ofertas
    /// </summary>
    public class NotificationService : MonoBehaviour
    {
        public static NotificationService Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool enableNotifications = true;
        [SerializeField] private bool debugMode = true;

        // Estado
        private bool _isInitialized = false;
        private string _fcmToken = "";

        // Eventos
        public event Action<string> OnTokenReceived;
        public event Action<NotificationData> OnNotificationReceived;
        public event Action<NotificationData> OnNotificationOpened;

        // Constantes
        private const string FCM_TOKEN_KEY = "FCM_Token";
        private const string NOTIFICATIONS_ENABLED_KEY = "Notifications_Enabled";

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private async void Start()
        {
            if (enableNotifications)
            {
                await Initialize();
            }
            else
            {
                Debug.Log("[Notifications] Notificaciones deshabilitadas en configuración");
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
#if FIREBASE_MESSAGING
                // Desuscribirse de eventos FCM
                FirebaseMessaging.TokenReceived -= OnFCMTokenReceived;
                FirebaseMessaging.MessageReceived -= OnFCMMessageReceived;
#endif
                Instance = null;
            }
        }

        #endregion

        #region Initialization

        private async Task Initialize()
        {
#if FIREBASE_MESSAGING
            Debug.Log("[Notifications] Inicializando Firebase Cloud Messaging...");

            try
            {
                var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

                if (dependencyStatus == DependencyStatus.Available)
                {
                    // Suscribirse a eventos de FCM
                    FirebaseMessaging.TokenReceived += OnFCMTokenReceived;
                    FirebaseMessaging.MessageReceived += OnFCMMessageReceived;

                    // Solicitar permiso (principalmente para iOS)
                    await RequestPermission();

                    // Obtener token
                    await FetchToken();

                    // Suscribirse a topics por defecto
                    await SubscribeToDefaultTopics();

                    _isInitialized = true;
                    Debug.Log("[Notifications] FCM inicializado correctamente");
                }
                else
                {
                    Debug.LogError($"[Notifications] Firebase no disponible: {dependencyStatus}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[Notifications] Error inicializando FCM: {e.Message}");
            }
#else
            Debug.LogWarning("[Notifications] Firebase Messaging no está configurado. Agrega FIREBASE_MESSAGING a Scripting Define Symbols.");
            await Task.CompletedTask;
#endif
        }

        private async Task RequestPermission()
        {
#if UNITY_IOS && FIREBASE_MESSAGING
            Debug.Log("[Notifications] Solicitando permiso de notificaciones (iOS)...");

            // En iOS, FCM maneja el permiso automáticamente
            // Pero podemos verificar el estado
            var settings = await Unity.Notifications.iOS.iOSNotificationCenter.RequestAuthorizationAsync(
                Unity.Notifications.iOS.AuthorizationOption.Alert |
                Unity.Notifications.iOS.AuthorizationOption.Badge |
                Unity.Notifications.iOS.AuthorizationOption.Sound
            );

            Debug.Log($"[Notifications] Permiso iOS: {settings.Granted}");
#else
            // En Android 13+, se requiere permiso POST_NOTIFICATIONS
            // Firebase SDK maneja esto automáticamente
            await Task.CompletedTask;
            Debug.Log("[Notifications] Permisos configurados para Android");
#endif
        }

        private async Task FetchToken()
        {
#if FIREBASE_MESSAGING
            try
            {
                var tokenTask = FirebaseMessaging.GetTokenAsync();
                _fcmToken = await tokenTask;

                if (!string.IsNullOrEmpty(_fcmToken))
                {
                    Debug.Log($"[Notifications] FCM Token obtenido: {_fcmToken.Substring(0, 20)}...");

                    // Guardar localmente
                    PlayerPrefs.SetString(FCM_TOKEN_KEY, _fcmToken);
                    PlayerPrefs.Save();

                    // Guardar en Firebase (para el usuario actual)
                    await SaveTokenToFirebase(_fcmToken);

                    OnTokenReceived?.Invoke(_fcmToken);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[Notifications] Error obteniendo token: {e.Message}");
            }
#else
            await Task.CompletedTask;
#endif
        }

        private async Task SaveTokenToFirebase(string token)
        {
            var playerData = AuthenticationService.Instance?.GetCurrentPlayerData();
            if (playerData == null) return;

            try
            {
                // Guardar token en el perfil del usuario
                // Esto permite enviar notificaciones específicas a este usuario
                var updates = new Dictionary<string, object>
                {
                    { "fcmToken", token },
                    { "fcmTokenUpdated", DateTime.UtcNow.ToString("o") },
                    { "platform", Application.platform.ToString() }
                };

                // TODO: Implementar guardado en Firebase Database
                // await DatabaseService.Instance.UpdatePlayerFields(playerData.userId, updates);

                LogDebug($"Token guardado para usuario: {playerData.userId}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Notifications] Error guardando token: {e.Message}");
            }

            await Task.CompletedTask;
        }

        private async Task SubscribeToDefaultTopics()
        {
#if FIREBASE_MESSAGING
            try
            {
                // Topics globales
                await FirebaseMessaging.SubscribeAsync("all_users");
                await FirebaseMessaging.SubscribeAsync("announcements");

                // Topic por plataforma
                string platformTopic = Application.platform == RuntimePlatform.IPhonePlayer
                    ? "ios_users"
                    : "android_users";
                await FirebaseMessaging.SubscribeAsync(platformTopic);

                LogDebug("Suscrito a topics por defecto");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Notifications] Error suscribiendo a topics: {e.Message}");
            }
#else
            await Task.CompletedTask;
#endif
        }

        #endregion

        #region FCM Callbacks

#if FIREBASE_MESSAGING
        private void OnFCMTokenReceived(object sender, TokenReceivedEventArgs e)
        {
            LogDebug($"Token actualizado: {e.Token.Substring(0, 20)}...");
            _fcmToken = e.Token;

            // Guardar nuevo token
            PlayerPrefs.SetString(FCM_TOKEN_KEY, _fcmToken);
            PlayerPrefs.Save();

            _ = SaveTokenToFirebase(_fcmToken);
            OnTokenReceived?.Invoke(_fcmToken);
        }

        private void OnFCMMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            LogDebug($"Mensaje recibido - From: {e.Message.From}");

            var notification = ParseNotification(e.Message);

            if (notification != null)
            {
                // Verificar si la app está en primer plano
                bool isAppInForeground = Application.isFocused;

                if (isAppInForeground)
                {
                    // App en primer plano: mostrar notificación in-app
                    OnNotificationReceived?.Invoke(notification);
                    HandleForegroundNotification(notification);
                }
                else
                {
                    // App en background: el sistema ya muestra la notificación
                    // Este callback se ejecuta cuando el usuario abre la notificación
                    OnNotificationOpened?.Invoke(notification);
                    HandleNotificationOpen(notification);
                }
            }
        }

        private NotificationData ParseNotification(FirebaseMessage message)
        {
            try
            {
                var notification = new NotificationData
                {
                    Title = message.Notification?.Title ?? "",
                    Body = message.Notification?.Body ?? "",
                    ReceivedAt = DateTime.Now
                };

                // Parsear datos adicionales
                if (message.Data != null)
                {
                    if (message.Data.TryGetValue("type", out string type))
                        notification.Type = type;

                    if (message.Data.TryGetValue("action", out string action))
                        notification.Action = action;

                    if (message.Data.TryGetValue("targetId", out string targetId))
                        notification.TargetId = targetId;

                    if (message.Data.TryGetValue("senderId", out string senderId))
                        notification.SenderId = senderId;

                    if (message.Data.TryGetValue("senderName", out string senderName))
                        notification.SenderName = senderName;

                    // Guardar todos los datos extra
                    notification.ExtraData = new Dictionary<string, string>(message.Data);
                }

                return notification;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Notifications] Error parseando notificación: {e.Message}");
                return null;
            }
        }
#endif

        #endregion

        #region Notification Handling

        private void HandleForegroundNotification(NotificationData notification)
        {
            LogDebug($"Notificación en foreground: {notification.Type} - {notification.Title}");

            // Persistir en almacenamiento local
            StoreNotification(notification);

            // Mostrar UI de notificación in-app
            ShowInAppNotification(notification);
        }

        private void HandleNotificationOpen(NotificationData notification)
        {
            LogDebug($"Notificación abierta: {notification.Type} - {notification.Action}");

            // Persistir en almacenamiento local
            StoreNotification(notification);

            // Navegar según el tipo de notificación
            switch (notification.Type)
            {
                case "friend_request":
                    NavigateToFriendRequests();
                    break;

                case "friend_accepted":
                    NavigateToFriends();
                    break;

                case "challenge":
                    NavigateToChallenge(notification.TargetId);
                    break;

                case "tournament_start":
                case "tournament_result":
                    NavigateToTournament(notification.TargetId);
                    break;

                case "daily_reward":
                    NavigateToDailyRewards();
                    break;

                case "promo":
                    NavigateToPromo(notification.Action);
                    break;

                default:
                    // Ir al menú principal
                    SceneManager.LoadScene("MainMenu");
                    break;
            }
        }

        private void ShowInAppNotification(NotificationData notification)
        {
            Debug.Log($"[Notifications] IN-APP: {notification.Title} - {notification.Body}");

            // Mostrar toast in-app via InAppNotificationManager
            var toastManager = InAppNotificationManager.Instance;
            if (toastManager != null)
            {
                toastManager.ShowFromNotificationData(
                    notification.Title,
                    notification.Body,
                    notification.Type,
                    notification.SenderId,
                    notification.SenderName,
                    notification.Action,
                    notification.TargetId
                );
            }
        }

        /// <summary>
        /// Persiste la notificación en el almacenamiento local
        /// </summary>
        private void StoreNotification(NotificationData notification)
        {
            if (notification == null) return;

            var storage = NotificationStorageService.Instance;
            if (storage == null)
            {
                LogDebug("NotificationStorageService no disponible, notificación no persistida");
                return;
            }

            storage.AddNotification(
                title: notification.Title,
                body: notification.Body,
                type: notification.Type,
                senderId: notification.SenderId,
                senderName: notification.SenderName,
                action: notification.Action,
                targetId: notification.TargetId,
                extraData: notification.ExtraData
            );
        }

        #endregion

        #region Navigation

        private void NavigateToFriendRequests()
        {
            // Navegar a Notifications con filtro social
            PlayerPrefs.SetString("NotificationsReturnScene", "MainMenu");
            SceneManager.LoadScene("Notifications");
        }

        private void NavigateToFriends()
        {
            // Navegar a Notifications con filtro social
            PlayerPrefs.SetString("NotificationsReturnScene", "MainMenu");
            SceneManager.LoadScene("Notifications");
        }

        private void NavigateToChallenge(string challengeId)
        {
            if (!string.IsNullOrEmpty(challengeId))
            {
                PlayerPrefs.SetString("ChallengeId", challengeId);
            }
            SceneManager.LoadScene("PlayModeSelection");
        }

        private void NavigateToTournament(string tournamentId)
        {
            if (!string.IsNullOrEmpty(tournamentId))
            {
                PlayerPrefs.SetString("OpenTournamentId", tournamentId);
            }
            SceneManager.LoadScene("CashBattleHub");
        }

        private void NavigateToDailyRewards()
        {
            PlayerPrefs.SetString("OpenPanel", "DailyRewards");
            SceneManager.LoadScene("MainMenu");
        }

        private void NavigateToPromo(string promoAction)
        {
            switch (promoAction)
            {
                case "premium":
                    PlayerPrefs.SetString("OpenPanel", "Premium");
                    SceneManager.LoadScene("MainMenu");
                    break;

                case "deposit":
                    SceneManager.LoadScene("CashWallet");
                    break;

                default:
                    SceneManager.LoadScene("MainMenu");
                    break;
            }
        }

        #endregion

        #region Topic Subscriptions

        /// <summary>
        /// Suscribirse a un topic específico
        /// </summary>
        public async Task SubscribeToTopic(string topic)
        {
            if (!_isInitialized) return;

#if FIREBASE_MESSAGING
            try
            {
                await FirebaseMessaging.SubscribeAsync(topic);
                LogDebug($"Suscrito a topic: {topic}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Notifications] Error suscribiendo a {topic}: {e.Message}");
            }
#else
            await Task.CompletedTask;
#endif
        }

        /// <summary>
        /// Desuscribirse de un topic
        /// </summary>
        public async Task UnsubscribeFromTopic(string topic)
        {
            if (!_isInitialized) return;

#if FIREBASE_MESSAGING
            try
            {
                await FirebaseMessaging.UnsubscribeAsync(topic);
                LogDebug($"Desuscrito de topic: {topic}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Notifications] Error desuscribiendo de {topic}: {e.Message}");
            }
#else
            await Task.CompletedTask;
#endif
        }

        /// <summary>
        /// Suscribirse a notificaciones de torneos
        /// </summary>
        public async Task SubscribeToTournamentNotifications(string tournamentId)
        {
            await SubscribeToTopic($"tournament_{tournamentId}");
        }

        /// <summary>
        /// Desuscribirse de notificaciones de torneos
        /// </summary>
        public async Task UnsubscribeFromTournamentNotifications(string tournamentId)
        {
            await UnsubscribeFromTopic($"tournament_{tournamentId}");
        }

        #endregion

        #region User Preferences

        /// <summary>
        /// Habilitar/deshabilitar todas las notificaciones
        /// </summary>
        public void SetNotificationsEnabled(bool enabled)
        {
            enableNotifications = enabled;
            PlayerPrefs.SetInt(NOTIFICATIONS_ENABLED_KEY, enabled ? 1 : 0);
            PlayerPrefs.Save();

            if (!enabled && _isInitialized)
            {
                // Desuscribirse de todos los topics
                _ = UnsubscribeFromTopic("all_users");
            }
            else if (enabled && !_isInitialized)
            {
                // Reinicializar
                _ = Initialize();
            }

            LogDebug($"Notificaciones: {(enabled ? "habilitadas" : "deshabilitadas")}");
        }

        /// <summary>
        /// Verificar si las notificaciones están habilitadas
        /// </summary>
        public bool AreNotificationsEnabled()
        {
            return PlayerPrefs.GetInt(NOTIFICATIONS_ENABLED_KEY, 1) == 1;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Obtener el token FCM actual (guardado localmente)
        /// </summary>
        public string GetCurrentToken()
        {
            return _fcmToken;
        }

        /// <summary>
        /// Alias para compatibilidad - Obtener el token FCM actual
        /// </summary>
        public string GetToken()
        {
            return _fcmToken;
        }

        /// <summary>
        /// Verificar si el servicio está inicializado
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// Refrescar el token FCM
        /// </summary>
        public async Task RefreshToken()
        {
#if FIREBASE_MESSAGING
            if (_isInitialized)
            {
                await FirebaseMessaging.DeleteTokenAsync();
                await FetchToken();
            }
#else
            await Task.CompletedTask;
#endif
        }

        #endregion

        #region Helpers

        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[Notifications] {message}");
            }
        }

        #endregion
    }

    /// <summary>
    /// Datos de una notificación recibida
    /// </summary>
    [Serializable]
    public class NotificationData
    {
        public string Title;
        public string Body;
        public string Type;           // friend_request, challenge, tournament_start, etc.
        public string Action;         // Acción a realizar (premium, deposit, etc.)
        public string TargetId;       // ID del objeto relacionado (tournamentId, challengeId, etc.)
        public string SenderId;       // ID del usuario que envió (para friend_request, challenge)
        public string SenderName;     // Nombre del usuario que envió
        public DateTime ReceivedAt;
        public Dictionary<string, string> ExtraData;
    }
}
