using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DigitPark.UI;
using DigitPark.Services;
using ServiceAchievementData = DigitPark.Services.AchievementData;

namespace DigitPark.Managers
{
    /// <summary>
    /// Global manager for achievement toast notifications.
    /// Persists across scenes (DontDestroyOnLoad).
    /// Listens to AchievementService events and displays toasts.
    /// </summary>
    public class AchievementNotificationManager : MonoBehaviour
    {
        private static AchievementNotificationManager _instance;
        public static AchievementNotificationManager Instance => _instance;

        [Header("Toast Prefab")]
        [SerializeField] private GameObject toastPrefab;

        [Header("Settings")]
        [SerializeField] private bool showNotifications = true;
        [SerializeField] private int maxQueuedNotifications = 5;
        [SerializeField] private float delayBetweenToasts = 0.5f;

        [Header("Epic Threshold")]
        [SerializeField] private int epicPointsThreshold = 100;

        // Runtime
        private Canvas _notificationCanvas;
        private AchievementToastUI _currentToast;
        private Queue<AchievementToastData> _notificationQueue = new Queue<AchievementToastData>();
        private bool _isShowingToast;

        private void Awake()
        {
            // Singleton pattern with DontDestroyOnLoad
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Initialize()
        {
            // Create dedicated canvas for notifications
            CreateNotificationCanvas();

            // Load toast prefab if not assigned
            if (toastPrefab == null)
            {
                toastPrefab = Resources.Load<GameObject>("Prefabs/Common/AchievementToast");

                if (toastPrefab == null)
                {
                    Debug.LogWarning("[AchievementNotification] Toast prefab not found. Attempting to load from path...");
                    // Try loading from AssetDatabase path (for development)
                    #if UNITY_EDITOR
                    toastPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Common/AchievementToast.prefab");
                    #endif
                }
            }

            // Subscribe to achievement events
            SubscribeToEvents();

            Debug.Log("[AchievementNotification] Manager initialized");
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void OnEnable()
        {
            // Re-subscribe when enabled
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        #region Canvas Setup

        private void CreateNotificationCanvas()
        {
            // Create canvas as child of this object
            GameObject canvasObj = new GameObject("NotificationCanvas");
            canvasObj.transform.SetParent(transform);

            _notificationCanvas = canvasObj.AddComponent<Canvas>();
            _notificationCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _notificationCanvas.sortingOrder = 9999; // Always on top

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            // Add GraphicRaycaster for interaction
            canvasObj.AddComponent<GraphicRaycaster>();

            Debug.Log("[AchievementNotification] Canvas created");
        }

        #endregion

        #region Event Subscription

        private void SubscribeToEvents()
        {
            // Subscribe to AchievementService
            if (AchievementService.Instance != null)
            {
                AchievementService.Instance.OnAchievementUnlocked -= OnAchievementUnlocked;
                AchievementService.Instance.OnAchievementUnlocked += OnAchievementUnlocked;
                Debug.Log("[AchievementNotification] Subscribed to AchievementService");
            }
            else
            {
                // Try again later when service is available
                Invoke(nameof(TrySubscribeToAchievementService), 1f);
            }
        }

        private void TrySubscribeToAchievementService()
        {
            if (AchievementService.Instance != null)
            {
                AchievementService.Instance.OnAchievementUnlocked -= OnAchievementUnlocked;
                AchievementService.Instance.OnAchievementUnlocked += OnAchievementUnlocked;
                Debug.Log("[AchievementNotification] Subscribed to AchievementService (delayed)");
            }
            else
            {
                // Keep trying
                Invoke(nameof(TrySubscribeToAchievementService), 2f);
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (AchievementService.Instance != null)
            {
                AchievementService.Instance.OnAchievementUnlocked -= OnAchievementUnlocked;
            }
        }

        #endregion

        #region Event Handlers

        private void OnAchievementUnlocked(ServiceAchievementData achievement)
        {
            if (!showNotifications) return;
            if (achievement == null) return;

            Debug.Log($"[AchievementNotification] Achievement unlocked: {achievement.titleKey}");

            // Create toast data
            AchievementToastData toastData = new AchievementToastData
            {
                achievementId = achievement.id,
                title = GetLocalizedTitle(achievement.titleKey),
                description = GetLocalizedDescription(achievement.descriptionKey),
                points = achievement.rewardCoins,
                icon = achievement.icon,
                isSecret = achievement.isHidden,
                isEpic = achievement.rewardCoins >= epicPointsThreshold
            };

            // Queue or show immediately
            QueueNotification(toastData);
        }

        private string GetLocalizedTitle(string key)
        {
            // Try to get localized text
            if (Localization.LocalizationManager.Instance != null)
            {
                return Localization.LocalizationManager.Instance.GetText(key);
            }
            return key;
        }

        private string GetLocalizedDescription(string key)
        {
            if (Localization.LocalizationManager.Instance != null)
            {
                return Localization.LocalizationManager.Instance.GetText(key);
            }
            return key;
        }

        #endregion

        #region Queue Management

        private void QueueNotification(AchievementToastData data)
        {
            // Limit queue size
            if (_notificationQueue.Count >= maxQueuedNotifications)
            {
                Debug.LogWarning("[AchievementNotification] Queue full, dropping oldest notification");
                _notificationQueue.Dequeue();
            }

            _notificationQueue.Enqueue(data);

            // Try to show if not already showing
            if (!_isShowingToast)
            {
                ShowNextNotification();
            }
        }

        private void ShowNextNotification()
        {
            if (_notificationQueue.Count == 0)
            {
                _isShowingToast = false;
                return;
            }

            _isShowingToast = true;
            AchievementToastData data = _notificationQueue.Dequeue();

            ShowToast(data);
        }

        #endregion

        #region Toast Display

        private void ShowToast(AchievementToastData data)
        {
            if (toastPrefab == null)
            {
                Debug.LogError("[AchievementNotification] Toast prefab not assigned!");
                _isShowingToast = false;
                return;
            }

            if (_notificationCanvas == null)
            {
                CreateNotificationCanvas();
            }

            // Create or reuse toast instance
            if (_currentToast == null)
            {
                GameObject toastObj = Instantiate(toastPrefab, _notificationCanvas.transform);
                _currentToast = toastObj.GetComponent<AchievementToastUI>();

                if (_currentToast == null)
                {
                    _currentToast = toastObj.AddComponent<AchievementToastUI>();
                }

                // Subscribe to toast events
                _currentToast.OnToastDismissed += OnToastDismissed;
            }

            // Show the toast
            _currentToast.Show(data);
        }

        private void OnToastDismissed()
        {
            // Wait a bit before showing next notification
            if (_notificationQueue.Count > 0)
            {
                Invoke(nameof(ShowNextNotification), delayBetweenToasts);
            }
            else
            {
                _isShowingToast = false;
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Show a custom achievement notification
        /// </summary>
        public void ShowNotification(string title, string description, int points, Sprite icon = null, bool isSecret = false)
        {
            if (!showNotifications) return;

            AchievementToastData data = new AchievementToastData
            {
                achievementId = "",
                title = title,
                description = description,
                points = points,
                icon = icon,
                isSecret = isSecret,
                isEpic = points >= epicPointsThreshold
            };

            QueueNotification(data);
        }

        /// <summary>
        /// Show notification from AchievementsManager data
        /// </summary>
        public void ShowNotification(AchievementDefinition achievement)
        {
            if (!showNotifications) return;
            if (achievement == null) return;

            // Load icon
            Sprite icon = LoadAchievementIcon(achievement.iconName);

            AchievementToastData data = new AchievementToastData
            {
                achievementId = achievement.id,
                title = achievement.title,
                description = achievement.description,
                points = achievement.points,
                icon = icon,
                isSecret = achievement.isSecret,
                isEpic = achievement.points >= epicPointsThreshold
            };

            QueueNotification(data);
        }

        private Sprite LoadAchievementIcon(string iconName)
        {
            if (string.IsNullOrEmpty(iconName)) return null;

            // Try to load from Resources
            Sprite sprite = Resources.Load<Sprite>($"Icons/Achievements/{iconName}");
            return sprite;
        }

        /// <summary>
        /// Enable/disable notifications
        /// </summary>
        public void SetNotificationsEnabled(bool enabled)
        {
            showNotifications = enabled;
            PlayerPrefs.SetInt("AchievementNotifications", enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Check if notifications are enabled
        /// </summary>
        public bool AreNotificationsEnabled()
        {
            return showNotifications;
        }

        /// <summary>
        /// Clear all queued notifications
        /// </summary>
        public void ClearQueue()
        {
            _notificationQueue.Clear();
        }

        /// <summary>
        /// Dismiss current toast immediately
        /// </summary>
        public void DismissCurrent()
        {
            if (_currentToast != null)
            {
                _currentToast.DismissImmediate();
            }
        }

        #endregion

        #region Debug

        [ContextMenu("Test Normal Achievement")]
        public void TestNormalAchievement()
        {
            ShowNotification("Primera Victoria", "Gana tu primera partida", 50);
        }

        [ContextMenu("Test Epic Achievement")]
        public void TestEpicAchievement()
        {
            ShowNotification("Leyenda Suprema", "Alcanza el rango máximo", 200, null, false);
        }

        [ContextMenu("Test Secret Achievement")]
        public void TestSecretAchievement()
        {
            ShowNotification("Búho Nocturno", "Juega a las 3:00 AM", 75, null, true);
        }

        [ContextMenu("Test Queue (3 achievements)")]
        public void TestQueue()
        {
            ShowNotification("Logro 1", "Descripción del logro 1", 25);
            ShowNotification("Logro 2", "Descripción del logro 2", 50);
            ShowNotification("Logro 3", "Descripción del logro 3", 100);
        }

        #endregion
    }
}
