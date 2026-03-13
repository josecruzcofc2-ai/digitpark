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
        private int _subscriptionRetries = 0;
        private const int MAX_SUBSCRIPTION_RETRIES = 5;

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
                _subscriptionRetries = 0;
                AchievementService.Instance.OnAchievementUnlocked -= OnAchievementUnlocked;
                AchievementService.Instance.OnAchievementUnlocked += OnAchievementUnlocked;
                Debug.Log("[AchievementNotification] Subscribed to AchievementService (delayed)");
            }
            else if (_subscriptionRetries < MAX_SUBSCRIPTION_RETRIES)
            {
                _subscriptionRetries++;
                Invoke(nameof(TrySubscribeToAchievementService), 2f);
            }
            else
            {
                Debug.LogWarning("[AchievementNotification] AchievementService not found after max retries. Stopping.");
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

            // Load icon: prefer Sprite field, fallback to loading by iconName
            Sprite icon = achievement.icon;
            if (icon == null)
            {
                icon = LoadAchievementIcon(achievement.iconName);
            }

            // Create toast data
            AchievementToastData toastData = new AchievementToastData
            {
                achievementId = achievement.id,
                title = GetLocalizedTitle(achievement.titleKey),
                description = GetLocalizedDescription(achievement.descriptionKey),
                icon = icon,
                isSecret = achievement.isHidden
            };

            // Queue or show immediately
            QueueNotification(toastData);
        }

        private string GetLocalizedTitle(string key)
        {
            return Localization.AutoLocalizer.Get(key);
        }

        private string GetLocalizedDescription(string key)
        {
            return Localization.AutoLocalizer.Get(key);
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
                    FixToastLayout(toastObj);
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
        /// Fixes layout on prefab instances to ensure correct sizing and fonts.
        /// Corrects any stale settings from old prefab builds.
        /// </summary>
        private void FixToastLayout(GameObject toastObj)
        {
            // Fix container size
            Transform containerT = toastObj.transform.Find("ToastContainer");
            if (containerT != null)
            {
                RectTransform containerRT = containerT.GetComponent<RectTransform>();
                if (containerRT != null)
                {
                    containerRT.sizeDelta = new Vector2(950f, 200f);
                    containerRT.anchoredPosition = new Vector2(0f, -80f);
                }
            }

            // Fix Content HorizontalLayoutGroup
            var hlgs = toastObj.GetComponentsInChildren<HorizontalLayoutGroup>(true);
            foreach (var hlg in hlgs)
            {
                if (hlg.gameObject.name == "Content")
                {
                    hlg.childControlWidth = true;
                    hlg.childControlHeight = true;
                    hlg.childForceExpandWidth = false;
                    hlg.childForceExpandHeight = false;
                }
            }

            // Fix icon size
            Transform iconSection = FindChildRecursive(toastObj.transform, "IconSection");
            if (iconSection != null)
            {
                var iconLE = iconSection.GetComponent<LayoutElement>();
                if (iconLE != null)
                {
                    iconLE.minWidth = 120f;
                    iconLE.minHeight = 120f;
                    iconLE.preferredWidth = 120f;
                    iconLE.preferredHeight = 120f;
                }
            }

            // Fix font sizes on TMP components
            var tmps = toastObj.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
            foreach (var tmp in tmps)
            {
                switch (tmp.gameObject.name)
                {
                    case "HeaderText":
                        tmp.fontSize = FontSizes.Body;
                        break;
                    case "AchievementTitle":
                        tmp.fontSize = FontSizes.Body;
                        break;
                    case "AchievementDescription":
                        tmp.fontSize = FontSizes.Body;
                        break;
                    case "CompletionText":
                        tmp.fontSize = FontSizes.Body;
                        break;
                }
            }
        }

        private static Transform FindChildRecursive(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            foreach (Transform child in parent)
            {
                Transform found = FindChildRecursive(child, name);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>
        /// Creates a simple toast UI by code when prefab is not available.
        /// Sized for 1080x1920 mobile reference resolution.
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

            // Toast container - 950x200 for mobile readability
            GameObject container = new GameObject("ToastContainer");
            container.transform.SetParent(toastRoot.transform, false);
            RectTransform containerRT = container.AddComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0.5f, 1f);
            containerRT.anchorMax = new Vector2(0.5f, 1f);
            containerRT.pivot = new Vector2(0.5f, 1f);
            containerRT.anchoredPosition = new Vector2(0f, -80f);
            containerRT.sizeDelta = new Vector2(950f, 200f);

            // Background
            Image bg = container.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.06f, 0.1f, 0.96f);
            Outline outline = container.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 1f, 1f, 1f);
            outline.effectDistance = new Vector2(2f, 2f);

            // Layout
            HorizontalLayoutGroup hlg = container.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 15f;
            hlg.padding = new RectOffset(15, 15, 12, 12);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // Icon frame + icon
            GameObject iconFrame = new GameObject("IconFrame");
            iconFrame.transform.SetParent(container.transform, false);
            Image iconFrameBg = iconFrame.AddComponent<Image>();
            iconFrameBg.color = new Color(1f, 0.84f, 0f, 1f); // Gold frame
            LayoutElement iconFrameLE = iconFrame.AddComponent<LayoutElement>();
            iconFrameLE.minWidth = 116f;
            iconFrameLE.minHeight = 116f;
            iconFrameLE.preferredWidth = 116f;
            iconFrameLE.preferredHeight = 116f;

            GameObject iconObj = new GameObject("AchievementIcon");
            iconObj.transform.SetParent(iconFrame.transform, false);
            RectTransform iconRT = iconObj.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.sizeDelta = new Vector2(106f, 106f);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.color = Color.white;

            // Info section
            GameObject infoObj = new GameObject("InfoSection");
            infoObj.transform.SetParent(container.transform, false);
            LayoutElement infoLE = infoObj.AddComponent<LayoutElement>();
            infoLE.flexibleWidth = 1f;
            infoLE.minHeight = 160f;

            VerticalLayoutGroup vlg = infoObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 4f;
            vlg.childAlignment = TextAnchor.MiddleLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Header - 26pt cyan
            GameObject headerObj = new GameObject("HeaderText");
            headerObj.transform.SetParent(infoObj.transform, false);
            TMPro.TextMeshProUGUI headerTMP = headerObj.AddComponent<TMPro.TextMeshProUGUI>();
            headerTMP.text = Localization.AutoLocalizer.Get("ach_achievement_unlocked");
            headerTMP.fontSize = FontSizes.Body;
            headerTMP.fontStyle = TMPro.FontStyles.Bold;
            headerTMP.color = new Color(0f, 1f, 1f, 1f);
            headerTMP.enableWordWrapping = false;
            headerTMP.overflowMode = TMPro.TextOverflowModes.Ellipsis;
            if (TMPro.TMP_Settings.defaultFontAsset != null) headerTMP.font = TMPro.TMP_Settings.defaultFontAsset;

            // Title - 36pt gold
            GameObject titleObj = new GameObject("AchievementTitle");
            titleObj.transform.SetParent(infoObj.transform, false);
            TMPro.TextMeshProUGUI titleTMP = titleObj.AddComponent<TMPro.TextMeshProUGUI>();
            titleTMP.text = "Title";
            titleTMP.fontSize = FontSizes.Body;
            titleTMP.fontStyle = TMPro.FontStyles.Bold;
            titleTMP.color = new Color(1f, 0.84f, 0f, 1f);
            titleTMP.enableWordWrapping = false;
            titleTMP.overflowMode = TMPro.TextOverflowModes.Ellipsis;
            if (TMPro.TMP_Settings.defaultFontAsset != null) titleTMP.font = TMPro.TMP_Settings.defaultFontAsset;

            // Description - 22pt light gray
            GameObject descObj = new GameObject("AchievementDescription");
            descObj.transform.SetParent(infoObj.transform, false);
            TMPro.TextMeshProUGUI descTMP = descObj.AddComponent<TMPro.TextMeshProUGUI>();
            descTMP.text = "Description";
            descTMP.fontSize = FontSizes.Body;
            descTMP.color = new Color(0.7f, 0.75f, 0.8f, 1f);
            descTMP.enableWordWrapping = false;
            descTMP.overflowMode = TMPro.TextOverflowModes.Ellipsis;
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

#if UNITY_EDITOR
        [ContextMenu("Test Normal Achievement")]
        public void TestNormalAchievement()
        {
            ShowNotification("First Victory", "Win your first match");
        }

        [ContextMenu("Test Epic Achievement")]
        public void TestEpicAchievement()
        {
            ShowNotification("Supreme Legend", "Reach the highest rank");
        }

        [ContextMenu("Test Secret Achievement")]
        public void TestSecretAchievement()
        {
            ShowNotification("Night Owl", "Play at 3:00 AM", null, true);
        }

        [ContextMenu("Test Queue (3 achievements)")]
        public void TestQueue()
        {
            ShowNotification("Achievement 1", "Achievement 1 description");
            ShowNotification("Achievement 2", "Achievement 2 description");
            ShowNotification("Achievement 3", "Achievement 3 description");
        }
#endif

        #endregion
    }
}
