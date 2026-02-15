using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using DigitPark.Services;
using DigitPark.Services.Firebase;
using DigitPark.Data;
using DigitPark.UI.Components;
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

        // Tab colors
        private static readonly Color ACTIVE_TAB = new Color(0f, 1f, 1f, 1f);
        private static readonly Color INACTIVE_TAB = new Color(0.15f, 0.17f, 0.22f, 1f);
        private static readonly Color ACTIVE_TEXT = new Color(0.05f, 0.05f, 0.08f, 1f);
        private static readonly Color INACTIVE_TEXT = new Color(0.5f, 0.5f, 0.55f, 1f);

        #region Unity Lifecycle

        private void Start()
        {
            Debug.Log("[FriendRequests] FriendRequestsSceneManager iniciado");

            SetupListeners();

            returnScene = PlayerPrefs.GetString("FriendRequestsReturnScene", "Friends");
            PlayerPrefs.DeleteKey("FriendRequestsReturnScene");

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
            if (receivedTabBg != null)
                receivedTabBg.color = received ? ACTIVE_TAB : INACTIVE_TAB;
            if (receivedTabText != null)
                receivedTabText.color = received ? ACTIVE_TEXT : INACTIVE_TEXT;

            if (sentTabBg != null)
                sentTabBg.color = received ? INACTIVE_TAB : ACTIVE_TAB;
            if (sentTabText != null)
                sentTabText.color = received ? INACTIVE_TEXT : ACTIVE_TEXT;

            LoadRequests();
        }

        #endregion

        #region Load Requests

        private async void LoadRequests()
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

            ShowLoadingIndicator(false);

            UpdatePendingCount();

            if (requests == null || requests.Count == 0)
            {
                if (emptyText != null)
                {
                    emptyText.text = showingReceived
                        ? "No tienes solicitudes pendientes"
                        : "No has enviado solicitudes";
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

        private void CreateRequestItem(FriendRequest request)
        {
            if (requestItemPrefab == null || scrollContent == null) return;

            GameObject item = Instantiate(requestItemPrefab, scrollContent);
            currentItems.Add(item);
            SetupRequestItem(item, request);

            // Animacion de entrada staggered
            int index = currentItems.Count - 1;
            item.transform.localScale = Vector3.zero;
            item.transform.DOScale(1f, 0.3f)
                .SetDelay(index * 0.06f)
                .SetEase(Ease.OutBack);
        }

        private void SetupRequestItem(GameObject item, FriendRequest request)
        {
            string displayUsername = showingReceived ? request.senderUsername : request.receiverUsername;
            string displayId = showingReceived ? request.senderId : request.receiverId;

            // Avatar
            var avatarImage = item.transform.Find("AvatarFrame/AvatarImage")?.GetComponent<Image>();
            if (avatarImage != null)
            {
                LoadRequestAvatar(avatarImage, displayUsername, displayId,
                    showingReceived ? request.senderAvatarUrl : null);
            }

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
                    timestampText.text = $"Hace {(int)diff.TotalMinutes} min";
                else if (diff.TotalHours < 24)
                    timestampText.text = $"Hace {(int)diff.TotalHours} horas";
                else if (diff.TotalDays < 7)
                    timestampText.text = $"Hace {(int)diff.TotalDays} dias";
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

        private async void LoadRequestAvatar(Image avatarImage, string username, string userId, string avatarUrl)
        {
            if (AvatarService.Instance != null && !string.IsNullOrEmpty(avatarUrl))
            {
                try
                {
                    Sprite avatar = await AvatarService.Instance.LoadAvatar(userId, avatarUrl, username);
                    if (avatar != null && avatarImage != null)
                    {
                        avatarImage.sprite = avatar;
                        avatarImage.color = Color.white;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[FriendRequests] Error cargando avatar: {e.Message}");
                    SetInitialAvatar(avatarImage, username, userId);
                }
            }
            else
            {
                SetInitialAvatar(avatarImage, username, userId);
            }
        }

        private void SetInitialAvatar(Image avatarImage, string username, string userId)
        {
            Sprite initial = AvatarInitialGenerator.GenerateAvatar(username, userId);
            if (avatarImage != null)
            {
                avatarImage.sprite = initial;
                avatarImage.color = Color.white;
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
                    ? $"{count} pendiente{(count != 1 ? "s" : "")}"
                    : "";
            }

            if (titleText != null)
            {
                titleText.text = count > 0
                    ? $"SOLICITUDES ({count})"
                    : "SOLICITUDES";
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
            Debug.Log($"[FriendRequests] Aceptando solicitud: {requestId}");

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

        private async void OnRejectClicked(string requestId, GameObject item)
        {
            Debug.Log($"[FriendRequests] Rechazando solicitud: {requestId}");

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

        private async void OnCancelClicked(string requestId, GameObject item)
        {
            Debug.Log($"[FriendRequests] Cancelando solicitud: {requestId}");

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

        private void RemoveItem(GameObject item)
        {
            if (item != null)
            {
                currentItems.Remove(item);
                item.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack)
                    .OnComplete(() => Destroy(item));
            }
        }

        private void CheckEmptyState()
        {
            if (currentItems.Count == 0 && emptyText != null)
            {
                emptyText.text = showingReceived
                    ? "No tienes solicitudes pendientes"
                    : "No has enviado solicitudes";
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
            cg.DOFade(1f, 0.4f).SetEase(Ease.OutQuad);
        }

        private void AnimateEntrance()
        {
            // Header slide desde arriba
            if (headerTransform != null)
            {
                Vector2 pos = headerTransform.anchoredPosition;
                headerTransform.anchoredPosition = new Vector2(pos.x, pos.y + 200);
                headerTransform.DOAnchorPos(pos, 0.4f).SetEase(Ease.OutBack);
            }

            // Tabs fade + slide desde izquierda
            if (tabsBarTransform != null)
            {
                var cg = tabsBarTransform.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                Vector2 pos = tabsBarTransform.anchoredPosition;
                tabsBarTransform.anchoredPosition = new Vector2(pos.x - 100, pos.y);
                DOTween.Sequence()
                    .AppendInterval(0.15f)
                    .Append(tabsBarTransform.DOAnchorPos(pos, 0.35f).SetEase(Ease.OutCubic))
                    .Join(cg.DOFade(1f, 0.35f));
            }

            // ScrollView fade in
            if (scrollViewTransform != null)
            {
                var cg = scrollViewTransform.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                DOTween.Sequence()
                    .AppendInterval(0.25f)
                    .Append(cg.DOFade(1f, 0.4f));
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
                cg.DOFade(1f, 0.2f).SetUpdate(true);
            }
            else
            {
                var cg = loadingIndicator.GetComponent<CanvasGroup>();
                if (cg != null)
                    cg.DOFade(0f, 0.2f).SetUpdate(true).OnComplete(() => loadingIndicator.SetActive(false));
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
    }
}
