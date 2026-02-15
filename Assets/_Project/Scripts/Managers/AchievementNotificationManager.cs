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
        [SerializeField] private int maxQueuedNotifications = 20;
        [SerializeField] private float delayBetweenToasts = 0.5f;

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
                    // Try loading from AssetDatabase path (for development)
                    #if UNITY_EDITOR
                    toastPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Common/AchievementToast.prefab");
                    #endif
                }

                if (toastPrefab == null)
                {
                    Debug.Log("[AchievementNotification] Toast prefab not found, using code-generated fallback.");
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
                icon = achievement.icon,
                isSecret = achievement.isHidden
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
            if (_notificationCanvas == null)
            {
                CreateNotificationCanvas();
            }

            // Create or reuse toast instance
            if (_currentToast == null)
            {
                if (toastPrefab != null)
                {
                    GameObject toastObj = Instantiate(toastPrefab, _notificationCanvas.transform);
                    _currentToast = toastObj.GetComponent<AchievementToastUI>();

                    if (_currentToast == null)
                    {
                        _currentToast = toastObj.AddComponent<AchievementToastUI>();
                    }
                }
                else
                {
                    // Fallback: create simple toast by code
                    _currentToast = CreateFallbackToast();
                }

                if (_currentToast == null)
                {
                    _isShowingToast = false;
                    return;
                }

                // Subscribe to toast events
                _currentToast.OnToastDismissed += OnToastDismissed;
            }

            // Show the toast
            _currentToast.Show(data);
        }

        /// <summary>
        /// Creates a simple toast UI by code when prefab is not available
        /// </summary>
        private AchievementToastUI CreateFallbackToast()
        {
            GameObject toastRoot = new GameObject("AchievementToast_Fallback");
            toastRoot.transform.SetParent(_notificationCanvas.transform, false);

            RectTransform rootRT = toastRoot.AddComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.sizeDelta = Vector2.zero;

            CanvasGroup cg = toastRoot.AddComponent<CanvasGroup>();
            cg.alpha = 1f;
            cg.blocksRaycasts = false;

            // Toast container
            GameObject container = new GameObject("ToastContainer");
            container.transform.SetParent(toastRoot.transform, false);
            RectTransform containerRT = container.AddComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0.5f, 1f);
            containerRT.anchorMax = new Vector2(0.5f, 1f);
            containerRT.pivot = new Vector2(0.5f, 1f);
            containerRT.anchoredPosition = new Vector2(0f, -60f);
            containerRT.sizeDelta = new Vector2(420f, 120f);

            // Background
            Image bg = container.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.06f, 0.1f, 0.96f);
            Outline outline = container.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 1f, 1f, 1f);
            outline.effectDistance = new Vector2(2f, 2f);

            // Layout
            HorizontalLayoutGroup hlg = container.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12f;
            hlg.padding = new RectOffset(12, 12, 10, 10);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;

            // Icon
            GameObject iconObj = new GameObject("AchievementIcon");
            iconObj.transform.SetParent(container.transform, false);
            RectTransform iconRT = iconObj.AddComponent<RectTransform>();
            iconRT.sizeDelta = new Vector2(72f, 72f);
            LayoutElement iconLE = iconObj.AddComponent<LayoutElement>();
            iconLE.minWidth = 72f;
            iconLE.minHeight = 72f;
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.color = Color.white;

            // Info section
            GameObject infoObj = new GameObject("InfoSection");
            infoObj.transform.SetParent(container.transform, false);
            LayoutElement infoLE = infoObj.AddComponent<LayoutElement>();
            infoLE.flexibleWidth = 1f;
            infoLE.minHeight = 90f;

            VerticalLayoutGroup vlg = infoObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 3f;
            vlg.childAlignment = TextAnchor.MiddleLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;

            // Header
            GameObject headerObj = new GameObject("HeaderText");
            headerObj.transform.SetParent(infoObj.transform, false);
            TMPro.TextMeshProUGUI headerTMP = headerObj.AddComponent<TMPro.TextMeshProUGUI>();
            headerTMP.text = "LOGRO DESBLOQUEADO";
            headerTMP.fontSize = 13f;
            headerTMP.fontStyle = TMPro.FontStyles.Bold;
            headerTMP.color = new Color(0f, 1f, 1f, 1f);
            if (TMPro.TMP_Settings.defaultFontAsset != null) headerTMP.font = TMPro.TMP_Settings.defaultFontAsset;

            // Title
            GameObject titleObj = new GameObject("AchievementTitle");
            titleObj.transform.SetParent(infoObj.transform, false);
            TMPro.TextMeshProUGUI titleTMP = titleObj.AddComponent<TMPro.TextMeshProUGUI>();
            titleTMP.text = "Titulo";
            titleTMP.fontSize = 18f;
            titleTMP.fontStyle = TMPro.FontStyles.Bold;
            titleTMP.color = new Color(1f, 0.84f, 0f, 1f);
            if (TMPro.TMP_Settings.defaultFontAsset != null) titleTMP.font = TMPro.TMP_Settings.defaultFontAsset;

            // Description
            GameObject descObj = new GameObject("AchievementDescription");
            descObj.transform.SetParent(infoObj.transform, false);
            TMPro.TextMeshProUGUI descTMP = descObj.AddComponent<TMPro.TextMeshProUGUI>();
            descTMP.text = "Desc";
            descTMP.fontSize = 12f;
            descTMP.color = new Color(0.7f, 0.75f, 0.8f, 1f);
            if (TMPro.TMP_Settings.defaultFontAsset != null) descTMP.font = TMPro.TMP_Settings.defaultFontAsset;

            // Add AchievementToastUI and wire references via reflection
            AchievementToastUI toastUI = toastRoot.AddComponent<AchievementToastUI>();

            // Wire serialized fields via reflection
            var type = typeof(AchievementToastUI);
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

            type.GetField("toastContainer", flags)?.SetValue(toastUI, containerRT);
            type.GetField("canvasGroup", flags)?.SetValue(toastUI, cg);
            type.GetField("achievementIcon", flags)?.SetValue(toastUI, iconImg);
            type.GetField("headerText", flags)?.SetValue(toastUI, headerTMP);
            type.GetField("titleText", flags)?.SetValue(toastUI, titleTMP);
            type.GetField("descriptionText", flags)?.SetValue(toastUI, descTMP);

            return toastUI;
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
        public void ShowNotification(string title, string description, Sprite icon = null, bool isSecret = false)
        {
            if (!showNotifications) return;

            AchievementToastData data = new AchievementToastData
            {
                achievementId = "",
                title = title,
                description = description,
                icon = icon,
                isSecret = isSecret
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
                icon = icon,
                isSecret = achievement.isSecret
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
            ShowNotification("Primera Victoria", "Gana tu primera partida");
        }

        [ContextMenu("Test Epic Achievement")]
        public void TestEpicAchievement()
        {
            ShowNotification("Leyenda Suprema", "Alcanza el rango máximo");
        }

        [ContextMenu("Test Secret Achievement")]
        public void TestSecretAchievement()
        {
            ShowNotification("Búho Nocturno", "Juega a las 3:00 AM", null, true);
        }

        [ContextMenu("Test Queue (3 achievements)")]
        public void TestQueue()
        {
            ShowNotification("Logro 1", "Descripción del logro 1");
            ShowNotification("Logro 2", "Descripción del logro 2");
            ShowNotification("Logro 3", "Descripción del logro 3");
        }

        #endregion
    }
}
