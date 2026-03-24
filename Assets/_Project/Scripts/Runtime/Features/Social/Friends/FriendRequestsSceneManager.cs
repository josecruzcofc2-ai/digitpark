using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using DigitPark.Services;
using DigitPark.Services.Firebase;
using DigitPark.Data;
using DigitPark.UI.Components;
using DigitPark.Localization;
using DG.Tweening;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager para la escena dedicada de Solicitudes de Amistad.
    /// Muestra tabs Recibidas/Enviadas con lista de solicitudes.
    /// Permite aceptar, rechazar o cancelar solicitudes.
    /// </summary>
    public class FriendRequestsSceneManager : MonoBehaviour
    {
        [Header("UI - Header")]
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI pendingCountText;

        [Header("UI - Tabs")]
        [SerializeField] private Button receivedTab;
        [SerializeField] private Button sentTab;
        [SerializeField] private Image receivedTabBg;
        [SerializeField] private TextMeshProUGUI receivedTabText;
        [SerializeField] private Image sentTabBg;
        [SerializeField] private TextMeshProUGUI sentTabText;

        [Header("UI - Content")]
        [SerializeField] private Transform scrollContent;
        [SerializeField] private GameObject requestItemPrefab;
        [SerializeField] private TextMeshProUGUI emptyText;
        [SerializeField] private GameObject loadingIndicator;

        [Header("UI - Sections (for animations)")]
        [SerializeField] private RectTransform headerTransform;
        [SerializeField] private RectTransform tabsBarTransform;
        [SerializeField] private RectTransform scrollViewTransform;

        private List<GameObject> currentItems = new List<GameObject>();
        private bool showingReceived = true;
        private string returnScene = "Friends";


        #region Unity Lifecycle

        private void Start()
        {
            Debug.Log("[FriendRequests] FriendRequestsSceneManager iniciado");

            SetupListeners();

            returnScene = PlayerPrefs.GetString("DP_FriendRequestsReturnScene", "Friends");
            PlayerPrefs.DeleteKey("DP_FriendRequestsReturnScene");

            AnimateEntrance();
            SwitchTab(true);
        }

        private void OnEnable()
        {
            if (FriendService.Instance != null)
            {
                FriendService.Instance.OnFriendRequestReceived += OnRequestReceived;
                FriendService.Instance.OnFriendRequestAccepted += OnRequestResponded;
                FriendService.Instance.OnFriendRequestRejected += OnRequestResponded;
            }
        }

        private void OnDisable()
        {
            if (FriendService.Instance != null)
            {
                FriendService.Instance.OnFriendRequestReceived -= OnRequestReceived;
                FriendService.Instance.OnFriendRequestAccepted -= OnRequestResponded;
                FriendService.Instance.OnFriendRequestRejected -= OnRequestResponded;
            }
        }

        private void SetupListeners()
        {
            // Disable auto-navigation from BackButton prefab to prevent double listener
            var autoNav = backButton?.GetComponent<DigitPark.UI.BackButton>();
            if (autoNav != null) autoNav.DisableAutoNavigation();
            backButton?.onClick.AddListener(OnBackClicked);
            receivedTab?.onClick.AddListener(() => SwitchTab(true));
            sentTab?.onClick.AddListener(() => SwitchTab(false));
        }

        #endregion

        #region Tabs

        private void SwitchTab(bool received)
        {
            showingReceived = received;

            // Update tab visuals
            Color activeColor = Color.cyan;
            Color inactiveColor = Color.gray;
            if (receivedTabBg != null)
                receivedTabBg.color = received ? activeColor : inactiveColor;
            if (receivedTabText != null)
                receivedTabText.color = received ? Color.black : Color.gray;

            if (sentTabBg != null)
                sentTabBg.color = received ? inactiveColor : activeColor;
            if (sentTabText != null)
                sentTabText.color = received ? Color.gray : Color.black;

            LoadRequests();
        }

        #endregion

        #region Load Requests

        private async void LoadRequests()
        {
            try
            {
                ClearItems();

                ShowLoadingIndicator(true);
                if (emptyText != null)
                    emptyText.gameObject.SetActive(false);

                List<FriendRequest> requests;

                if (showingReceived)
                {
                    requests = await FriendService.Instance.GetPendingReceivedRequests();
                }
                else
                {
                    requests = await FriendService.Instance.GetPendingSentRequests();
                }

                if (this == null) return;

                ShowLoadingIndicator(false);

                UpdatePendingCount();

                if (requests == null || requests.Count == 0)
                {
                    if (emptyText != null)
                    {
                        emptyText.text = showingReceived
                            ? AutoLocalizer.Get("requests_no_received")
                            : AutoLocalizer.Get("requests_no_sent");
                        emptyText.gameObject.SetActive(true);
                        AnimateEmptyText();
                    }
                    return;
                }

                foreach (var request in requests)
                {
                    CreateRequestItem(request);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FriendRequestsSceneManager] {ex.Message}");
            }
        }

        private void CreateRequestItem(FriendRequest request)
        {
            if (requestItemPrefab == null || scrollContent == null) return;

            GameObject item = Instantiate(requestItemPrefab, scrollContent);
            currentItems.Add(item);
            SetupRequestItem(item, request);

            // Apply theme to request item
            var usernameGO = item.transform.Find("InfoSection/Username")?.gameObject;
            var timestampGO = item.transform.Find("InfoSection/TimestampText")?.gameObject;
            var acceptGO = item.transform.Find("ButtonsRow/AcceptButton")?.gameObject;
            var rejectGO = item.transform.Find("ButtonsRow/RejectButton")?.gameObject;
            var cancelGO = item.transform.Find("ButtonsRow/CancelButton")?.gameObject;

            // Animacion de entrada staggered
            int index = currentItems.Count - 1;
            item.transform.localScale = Vector3.zero;
            item.transform.DOScale(1f, 0.3f)
                .SetDelay(index * 0.06f)
                .SetEase(Ease.OutBack)
                .SetLink(item);
        }

        private void SetupRequestItem(GameObject item, FriendRequest request)
        {
            string displayUsername = showingReceived ? request.senderUsername : request.receiverUsername;
            string displayId = showingReceived ? request.senderId : request.receiverId;

            // Username
            var usernameText = item.transform.Find("InfoSection/Username")?.GetComponent<TextMeshProUGUI>();
            if (usernameText != null)
                usernameText.text = displayUsername;

            // Timestamp
            var timestampText = item.transform.Find("InfoSection/TimestampText")?.GetComponent<TextMeshProUGUI>();
            if (timestampText != null)
            {
                var date = request.GetCreatedAt();
                var diff = System.DateTime.Now - date;
                if (diff.TotalMinutes < 60)
                    timestampText.text = AutoLocalizer.Get("time_ago_minutes", (int)diff.TotalMinutes);
                else if (diff.TotalHours < 24)
                    timestampText.text = AutoLocalizer.Get("time_ago_hours", (int)diff.TotalHours);
                else if (diff.TotalDays < 7)
                    timestampText.text = AutoLocalizer.Get("time_ago_days", (int)diff.TotalDays);
                else
                    timestampText.text = date.ToString("dd/MM/yyyy");
            }

            // Buttons - Received: Accept + Reject, Sent: Cancel
            string reqId = request.requestId;

            var acceptBtn = item.transform.Find("ButtonsRow/AcceptButton")?.GetComponent<Button>();
            var rejectBtn = item.transform.Find("ButtonsRow/RejectButton")?.GetComponent<Button>();
            var cancelBtn = item.transform.Find("ButtonsRow/CancelButton")?.GetComponent<Button>();

            if (showingReceived)
            {
                if (acceptBtn != null)
                {
                    acceptBtn.gameObject.SetActive(true);
                    acceptBtn.onClick.AddListener(() => OnAcceptClicked(reqId, item));
                }
                if (rejectBtn != null)
                {
                    rejectBtn.gameObject.SetActive(true);
                    rejectBtn.onClick.AddListener(() => OnRejectClicked(reqId, item));
                }
                if (cancelBtn != null)
                    cancelBtn.gameObject.SetActive(false);
            }
            else
            {
                if (acceptBtn != null)
                    acceptBtn.gameObject.SetActive(false);
                if (rejectBtn != null)
                    rejectBtn.gameObject.SetActive(false);
                if (cancelBtn != null)
                {
                    cancelBtn.gameObject.SetActive(true);
                    cancelBtn.onClick.AddListener(() => OnCancelClicked(reqId, item));
                }
            }
        }

        private void ClearItems()
        {
            foreach (var item in currentItems)
            {
                if (item != null) Destroy(item);
            }
            currentItems.Clear();
        }

        private void UpdatePendingCount()
        {
            int count = FriendService.Instance?.GetPendingRequestsCount() ?? 0;

            if (pendingCountText != null)
            {
                pendingCountText.text = count > 0
                    ? AutoLocalizer.Get("requests_pending_count", count)
                    : "";
            }

            if (titleText != null)
            {
                titleText.text = count > 0
                    ? AutoLocalizer.Get("requests_title_count", count)
                    : AutoLocalizer.Get("requests_title");
            }
        }

        #endregion

        #region Button Callbacks

        private void OnBackClicked()
        {
            Debug.Log($"[FriendRequests] Volviendo a: {returnScene}");
            SceneManager.LoadScene(returnScene);
        }

        private async void OnAcceptClicked(string requestId, GameObject item)
        {
            try
            {
                Debug.Log($"[FriendRequests] Aceptando solicitud: {requestId}");

                if (FriendService.Instance == null) { Debug.LogWarning("[FriendRequests] FriendService not available"); return; }
                var result = await FriendService.Instance.AcceptFriendRequest(requestId);

                if (result.Success)
                {
                    RemoveItem(item);
                    UpdatePendingCount();
                    CheckEmptyState();
                }
                else
                {
                    Debug.LogWarning($"[FriendRequests] Error: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FriendRequestsSceneManager] {ex.Message}");
            }
        }

        private async void OnRejectClicked(string requestId, GameObject item)
        {
            try
            {
                Debug.Log($"[FriendRequests] Rechazando solicitud: {requestId}");

                if (FriendService.Instance == null) { Debug.LogWarning("[FriendRequests] FriendService not available"); return; }
                var result = await FriendService.Instance.RejectFriendRequest(requestId);

                if (result.Success)
                {
                    RemoveItem(item);
                    UpdatePendingCount();
                    CheckEmptyState();
                }
                else
                {
                    Debug.LogWarning($"[FriendRequests] Error: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FriendRequestsSceneManager] {ex.Message}");
            }
        }

        private async void OnCancelClicked(string requestId, GameObject item)
        {
            try
            {
                Debug.Log($"[FriendRequests] Cancelando solicitud: {requestId}");

                if (FriendService.Instance == null) { Debug.LogWarning("[FriendRequests] FriendService not available"); return; }
                var result = await FriendService.Instance.CancelFriendRequest(requestId);

                if (result.Success)
                {
                    RemoveItem(item);
                    CheckEmptyState();
                }
                else
                {
                    Debug.LogWarning($"[FriendRequests] Error: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FriendRequestsSceneManager] {ex.Message}");
            }
        }

        private void RemoveItem(GameObject item)
        {
            if (item != null)
            {
                currentItems.Remove(item);
                item.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack)
                    .SetLink(item).OnComplete(() => { if (item != null) Destroy(item); });
            }
        }

        private void CheckEmptyState()
        {
            if (currentItems.Count == 0 && emptyText != null)
            {
                emptyText.text = showingReceived
                    ? AutoLocalizer.Get("requests_no_received")
                    : AutoLocalizer.Get("requests_no_sent");
                emptyText.gameObject.SetActive(true);
                AnimateEmptyText();
            }
        }

        #endregion

        #region Animations

        private void AnimateEmptyText()
        {
            if (emptyText == null) return;
            var cg = emptyText.GetComponent<CanvasGroup>();
            if (cg == null) cg = emptyText.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.DOFade(1f, 0.4f).SetEase(Ease.OutQuad).SetLink(gameObject);
        }

        private void AnimateEntrance()
        {
            // Header slide desde arriba
            if (headerTransform != null)
            {
                Vector2 pos = headerTransform.anchoredPosition;
                headerTransform.anchoredPosition = new Vector2(pos.x, pos.y + 200);
                headerTransform.DOAnchorPos(pos, 0.4f).SetEase(Ease.OutBack).SetLink(gameObject);
            }

            // Tabs fade + slide desde izquierda
            if (tabsBarTransform != null)
            {
                var cg = tabsBarTransform.gameObject.GetComponent<CanvasGroup>();
                if (cg == null) cg = tabsBarTransform.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                Vector2 pos = tabsBarTransform.anchoredPosition;
                tabsBarTransform.anchoredPosition = new Vector2(pos.x - 100, pos.y);
                DOTween.Sequence()
                    .AppendInterval(0.15f)
                    .Append(tabsBarTransform.DOAnchorPos(pos, 0.35f).SetEase(Ease.OutCubic))
                    .Join(cg.DOFade(1f, 0.35f))
                    .SetLink(gameObject);
            }

            // ScrollView fade in
            if (scrollViewTransform != null)
            {
                var cg = scrollViewTransform.gameObject.GetComponent<CanvasGroup>();
                if (cg == null) cg = scrollViewTransform.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                DOTween.Sequence()
                    .AppendInterval(0.25f)
                    .Append(cg.DOFade(1f, 0.4f))
                    .SetLink(gameObject);
            }
        }

        #endregion

        #region Loading Helper

        private void ShowLoadingIndicator(bool show)
        {
            if (loadingIndicator == null) return;

            if (show)
            {
                loadingIndicator.SetActive(true);
                var cg = loadingIndicator.GetComponent<CanvasGroup>();
                if (cg == null) cg = loadingIndicator.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                cg.DOFade(1f, 0.2f).SetUpdate(true).SetLink(loadingIndicator);
            }
            else
            {
                var cg = loadingIndicator.GetComponent<CanvasGroup>();
                if (cg != null)
                    cg.DOFade(0f, 0.2f).SetUpdate(true).SetLink(loadingIndicator).OnComplete(() => loadingIndicator.SetActive(false));
                else
                    loadingIndicator.SetActive(false);
            }
        }

        #endregion

        #region Events

        private void OnRequestReceived(FriendRequest request)
        {
            UpdatePendingCount();
            if (showingReceived)
            {
                CreateRequestItem(request);
            }
        }

        private void OnRequestResponded(FriendRequest request)
        {
            UpdatePendingCount();
        }

        #endregion

        private void OnDestroy()
        {
            transform.DOKill();

            // Kill tweens on child objects
            foreach (var item in currentItems)
            {
                if (item != null) item.transform.DOKill();
            }
            if (headerTransform != null) headerTransform.DOKill();
            if (tabsBarTransform != null) tabsBarTransform.DOKill();
            if (scrollViewTransform != null) scrollViewTransform.DOKill();
        }
    }
}
