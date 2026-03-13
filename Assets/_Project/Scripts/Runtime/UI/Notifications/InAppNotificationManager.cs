using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using DigitPark.Services;
using DigitPark.UI;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager global de notificaciones in-app (toasts).
    /// Muestra toasts cuando llegan notificaciones con la app en foreground.
    /// Singleton DontDestroyOnLoad con canvas overlay propio.
    ///
    /// Tipos de toast por categoría:
    /// - Social (azul): friend_request, friend_accepted
    /// - Games (naranja): challenge, tournament_start, tournament_result
    /// - Rewards (dorado): daily_reward, promo
    /// - System (cyan): default
    /// </summary>
    public class InAppNotificationManager : MonoBehaviour
    {
        private static InAppNotificationManager _instance;
        public static InAppNotificationManager Instance => _instance;

        [Header("Toast Prefab")]
        [SerializeField] private GameObject toastPrefab;

        [Header("Settings")]
        [SerializeField] private bool showNotifications = true;
        [SerializeField] private int maxQueuedNotifications = 5;
        [SerializeField] private float delayBetweenToasts = 0.8f;

        // Runtime
        private Canvas _toastCanvas;
        private InAppToastUI _currentToast;
        private Queue<InAppToastData> _toastQueue = new Queue<InAppToastData>();
        private bool _isShowingToast;

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            CancelInvoke();
            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion

        #region Initialization

        private void Initialize()
        {
            CreateToastCanvas();

            // Load prefab if not assigned
            if (toastPrefab == null)
            {
                toastPrefab = Resources.Load<GameObject>("Prefabs/Common/InAppToast");

                #if UNITY_EDITOR
                if (toastPrefab == null)
                {
                    toastPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                        "Assets/_Project/Prefabs/Common/InAppToast.prefab");
                }
                #endif
            }

            Debug.Log("[InAppNotification] Manager initialized");
        }

        private void CreateToastCanvas()
        {
            GameObject canvasObj = new GameObject("InAppToastCanvas");
            canvasObj.transform.SetParent(transform);

            _toastCanvas = canvasObj.AddComponent<Canvas>();
            _toastCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _toastCanvas.sortingOrder = 9998; // Just below achievement toasts (9999)

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            Debug.Log("[InAppNotification] Canvas created");
        }

        #endregion

        #region Public API

        /// <summary>
        /// Muestra un toast de notificación in-app
        /// </summary>
        public void Show(string title, string body, string type,
            string senderId = "", string senderName = "",
            string action = "", string targetId = "")
        {
            if (!showNotifications) return;

            var data = new InAppToastData
            {
                title = title,
                body = body,
                type = type,
                senderId = senderId,
                senderName = senderName,
                action = action,
                targetId = targetId,
                category = NotificationStorageService.GetCategory(type)
            };

            QueueToast(data);
        }

        /// <summary>
        /// Muestra un toast desde un NotificationData (del push service)
        /// </summary>
        public void ShowFromNotificationData(string title, string body, string type,
            string senderId, string senderName, string action, string targetId)
        {
            Show(title, body, type, senderId, senderName, action, targetId);
        }

        /// <summary>
        /// Habilita/deshabilita notificaciones in-app
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            showNotifications = enabled;
            if (!enabled)
            {
                DismissCurrent();
                ClearQueue();
            }
        }

        /// <summary>
        /// Descarta el toast actual
        /// </summary>
        public void DismissCurrent()
        {
            if (_currentToast != null && _currentToast.IsShowing)
            {
                _currentToast.Dismiss();
            }
        }

        /// <summary>
        /// Limpia la cola de toasts
        /// </summary>
        public void ClearQueue()
        {
            _toastQueue.Clear();
        }

        #endregion

        #region Queue Management

        private void QueueToast(InAppToastData data)
        {
            if (_toastQueue.Count >= maxQueuedNotifications)
            {
                Debug.Log("[InAppNotification] Queue full, dropping oldest toast");
                _toastQueue.Dequeue();
            }

            _toastQueue.Enqueue(data);

            if (!_isShowingToast)
            {
                ShowNextToast();
            }
        }

        private void ShowNextToast()
        {
            if (_toastQueue.Count == 0)
            {
                _isShowingToast = false;
                return;
            }

            _isShowingToast = true;
            InAppToastData data = _toastQueue.Dequeue();
            ShowToast(data);
        }

        #endregion

        #region Toast Display

        private void ShowToast(InAppToastData data)
        {
            if (_toastCanvas == null)
            {
                CreateToastCanvas();
            }

            if (_currentToast == null)
            {
                if (toastPrefab != null)
                {
                    GameObject toastObj = Instantiate(toastPrefab, _toastCanvas.transform);
                    _currentToast = toastObj.GetComponent<InAppToastUI>();

                    if (_currentToast == null)
                    {
                        _currentToast = toastObj.AddComponent<InAppToastUI>();
                    }
                }
                else
                {
                    // Fallback: crear toast por código
                    _currentToast = CreateFallbackToast();
                }

                if (_currentToast == null)
                {
                    _isShowingToast = false;
                    return;
                }

                _currentToast.OnToastDismissed += OnToastDismissed;
                _currentToast.OnToastAction += OnToastAction;
            }

            _currentToast.Show(data);
        }

        private void OnToastDismissed()
        {
            // Esperar antes de mostrar el siguiente
            if (_toastQueue.Count > 0)
            {
                Invoke(nameof(ShowNextToast), delayBetweenToasts);
            }
            else
            {
                _isShowingToast = false;
            }
        }

        private void OnToastAction(InAppToastData data, string actionType)
        {
            Debug.Log($"[InAppNotification] Action: {actionType} on {data.type}");

            switch (actionType)
            {
                case "primary":
                    HandlePrimaryAction(data);
                    break;
                case "secondary":
                    HandleSecondaryAction(data);
                    break;
                case "tap":
                    HandleTapAction(data);
                    break;
            }
        }

        #endregion

        #region Action Handlers

        private void HandlePrimaryAction(InAppToastData data)
        {
            switch (data.type)
            {
                case "friend_request":
                    AcceptFriendRequest(data.senderId);
                    break;
                case "challenge":
                    AcceptChallenge(data.targetId);
                    break;
                case "tournament_start":
                    JoinTournament(data.targetId);
                    break;
                case "daily_reward":
                    ClaimDailyReward();
                    break;
                case "promo":
                    ViewPromo(data.action);
                    break;
                default:
                    NavigateToNotifications();
                    break;
            }
        }

        private void HandleSecondaryAction(InAppToastData data)
        {
            // Secondary actions just dismiss the toast
            Debug.Log($"[InAppNotification] Secondary action dismissed: {data.type}");
        }

        private void HandleTapAction(InAppToastData data)
        {
            // Tapping the toast navigates to notifications scene
            NavigateToNotifications();
        }

        private async void AcceptFriendRequest(string senderId)
        {
            try
            {
                if (FriendService.Instance != null && !string.IsNullOrEmpty(senderId))
                {
                    var result = await FriendService.Instance.AcceptFriendRequest(senderId);
                    Debug.Log($"[InAppNotification] Friend request accept: {result.Success}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[InAppNotification] AcceptFriendRequest error: {ex.Message}");
            }
        }

        private void AcceptChallenge(string challengeId)
        {
            if (!string.IsNullOrEmpty(challengeId))
                PlayerPrefs.SetString("ChallengeId", challengeId);

            UnityEngine.SceneManagement.SceneManager.LoadScene("PlayModeSelection");
        }

        private void JoinTournament(string tournamentId)
        {
            if (!string.IsNullOrEmpty(tournamentId))
                PlayerPrefs.SetString("OpenTournamentId", tournamentId);

            UnityEngine.SceneManagement.SceneManager.LoadScene("CashBattleHub");
        }

        private void ClaimDailyReward()
        {
            PlayerPrefs.SetString("OpenPanel", "DailyRewards");
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        private void ViewPromo(string promoAction)
        {
            switch (promoAction)
            {
                case "premium":
                    PlayerPrefs.SetString("OpenPanel", "Premium");
                    UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
                    break;
                case "deposit":
                    UnityEngine.SceneManagement.SceneManager.LoadScene("CashWallet");
                    break;
                default:
                    UnityEngine.SceneManagement.SceneManager.LoadScene("Shop");
                    break;
            }
        }

        private void NavigateToNotifications()
        {
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            PlayerPrefs.SetString("NotificationsReturnScene", currentScene);
            UnityEngine.SceneManagement.SceneManager.LoadScene("Notifications");
        }

        #endregion

        #region Fallback Toast (Code-Generated)

        private InAppToastUI CreateFallbackToast()
        {
            var root = new GameObject("InAppToast_Fallback");
            root.transform.SetParent(_toastCanvas.transform, false);

            // Container
            var rootRT = root.AddComponent<RectTransform>();
            rootRT.anchorMin = new Vector2(0.03f, 0.92f);
            rootRT.anchorMax = new Vector2(0.97f, 0.98f);
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            var cg = root.AddComponent<CanvasGroup>();
            cg.alpha = 0f;

            var bg = root.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.08f, 0.14f, 0.95f);

            var outline = root.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 1f, 1f, 0.5f);
            outline.effectDistance = new Vector2(1, 1);

            root.AddComponent<Button>().targetGraphic = bg;

            // Type icon
            var iconGO = new GameObject("TypeIcon");
            iconGO.transform.SetParent(root.transform, false);
            var iconRT = iconGO.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0, 0.15f);
            iconRT.anchorMax = new Vector2(0, 0.85f);
            iconRT.pivot = new Vector2(0, 0.5f);
            iconRT.anchoredPosition = new Vector2(12, 0);
            iconRT.sizeDelta = new Vector2(40, 0);
            var iconBg = iconGO.AddComponent<Image>();
            iconBg.color = new Color(1, 1, 1, 0.06f);

            var iconText = new GameObject("IconText");
            iconText.transform.SetParent(iconGO.transform, false);
            var itRT = iconText.AddComponent<RectTransform>();
            itRT.anchorMin = Vector2.zero;
            itRT.anchorMax = Vector2.one;
            itRT.offsetMin = Vector2.zero;
            itRT.offsetMax = Vector2.zero;
            var itTMP = iconText.AddComponent<TMPro.TextMeshProUGUI>();
            itTMP.fontSize = FontSizes.AutoMinBody;
            itTMP.alignment = TMPro.TextAlignmentOptions.Center;

            // Title
            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(root.transform, false);
            var tRT = titleGO.AddComponent<RectTransform>();
            tRT.anchorMin = new Vector2(0, 0.5f);
            tRT.anchorMax = new Vector2(0.85f, 1);
            tRT.offsetMin = new Vector2(60, 2);
            tRT.offsetMax = new Vector2(-10, -2);
            var tTMP = titleGO.AddComponent<TMPro.TextMeshProUGUI>();
            tTMP.fontSize = FontSizes.AutoMinBody;
            tTMP.fontStyle = TMPro.FontStyles.Bold;
            tTMP.color = new Color(0.95f, 0.95f, 0.95f, 1f);
            tTMP.alignment = TMPro.TextAlignmentOptions.Left;
            tTMP.overflowMode = TMPro.TextOverflowModes.Ellipsis;
            tTMP.maxVisibleLines = 1;

            // Body
            var bodyGO = new GameObject("Body");
            bodyGO.transform.SetParent(root.transform, false);
            var bRT = bodyGO.AddComponent<RectTransform>();
            bRT.anchorMin = new Vector2(0, 0);
            bRT.anchorMax = new Vector2(0.85f, 0.5f);
            bRT.offsetMin = new Vector2(60, 2);
            bRT.offsetMax = new Vector2(-10, -2);
            var bTMP = bodyGO.AddComponent<TMPro.TextMeshProUGUI>();
            bTMP.fontSize = FontSizes.Debug;
            bTMP.color = new Color(0.6f, 0.6f, 0.65f, 1f);
            bTMP.alignment = TMPro.TextAlignmentOptions.Left;
            bTMP.overflowMode = TMPro.TextOverflowModes.Ellipsis;
            bTMP.maxVisibleLines = 1;

            // Dismiss X
            var dismissGO = new GameObject("DismissButton");
            dismissGO.transform.SetParent(root.transform, false);
            var dRT = dismissGO.AddComponent<RectTransform>();
            dRT.anchorMin = new Vector2(1, 0.5f);
            dRT.anchorMax = new Vector2(1, 0.5f);
            dRT.pivot = new Vector2(1, 0.5f);
            dRT.anchoredPosition = new Vector2(-8, 0);
            dRT.sizeDelta = new Vector2(30, 30);
            var dBg = dismissGO.AddComponent<Image>();
            dBg.color = new Color(1, 1, 1, 0.04f);
            dismissGO.AddComponent<Button>().targetGraphic = dBg;

            var xText = new GameObject("X");
            xText.transform.SetParent(dismissGO.transform, false);
            var xRT = xText.AddComponent<RectTransform>();
            xRT.anchorMin = Vector2.zero;
            xRT.anchorMax = Vector2.one;
            xRT.offsetMin = Vector2.zero;
            xRT.offsetMax = Vector2.zero;
            var xTMP = xText.AddComponent<TMPro.TextMeshProUGUI>();
            xTMP.text = "✕";
            xTMP.fontSize = FontSizes.AutoMinBody;
            xTMP.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            xTMP.alignment = TMPro.TextAlignmentOptions.Center;

            // Add InAppToastUI component
            var toastUI = root.AddComponent<InAppToastUI>();

            // Use reflection to assign references
            SetPrivateField(toastUI, "toastContainer", rootRT);
            SetPrivateField(toastUI, "canvasGroup", cg);
            SetPrivateField(toastUI, "background", bg);
            SetPrivateField(toastUI, "borderOutline", outline);
            SetPrivateField(toastUI, "typeIconText", itTMP);
            SetPrivateField(toastUI, "titleText", tTMP);
            SetPrivateField(toastUI, "bodyText", bTMP);
            SetPrivateField(toastUI, "dismissButton", dismissGO.GetComponent<Button>());
            SetPrivateField(toastUI, "cardButton", root.GetComponent<Button>());

            return toastUI;
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(obj, value);
        }

        #endregion
    }

    #region Data Model

    /// <summary>
    /// Datos para un toast in-app
    /// </summary>
    [Serializable]
    public class InAppToastData
    {
        public string title;
        public string body;
        public string type;
        public string senderId;
        public string senderName;
        public string action;
        public string targetId;
        public NotificationCategory category;
    }

    #endregion
}
