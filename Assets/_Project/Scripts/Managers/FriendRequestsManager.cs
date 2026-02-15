using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DigitPark.Services;
using DigitPark.Data;
using DG.Tweening;
using DigitPark.Animations;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager para el panel de solicitudes de amistad
    /// Puede ser usado como popup en cualquier escena o como escena dedicada
    /// </summary>
    public class FriendRequestsManager : MonoBehaviour
    {
        [Header("UI - Panel")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button overlayButton;

        [Header("UI - Tabs")]
        [SerializeField] private Button receivedTab;
        [SerializeField] private Button sentTab;
        [SerializeField] private Color activeTabColor = new Color(0f, 1f, 1f, 1f);
        [SerializeField] private Color inactiveTabColor = new Color(0.3f, 0.3f, 0.3f, 1f);

        [Header("UI - Content")]
        [SerializeField] private Transform contentContainer;
        [SerializeField] private GameObject requestItemPrefab;
        [SerializeField] private TextMeshProUGUI emptyText;
        [SerializeField] private GameObject loadingIndicator;

        [Header("UI - Badge")]
        [SerializeField] private GameObject pendingBadge;
        [SerializeField] private TextMeshProUGUI pendingCountText;

        private List<GameObject> currentItems = new List<GameObject>();
        private bool showingReceived = true;

        private void Start()
        {
            SetupListeners();
            Hide();
        }

        private void OnEnable()
        {
            // Suscribirse a eventos de FriendService
            if (FriendService.Instance != null)
            {
                FriendService.Instance.OnFriendRequestReceived += OnRequestReceived;
                FriendService.Instance.OnFriendRequestAccepted += OnRequestResponded;
                FriendService.Instance.OnFriendRequestRejected += OnRequestResponded;
            }

            UpdatePendingBadge();
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
            closeButton?.onClick.AddListener(Hide);
            overlayButton?.onClick.AddListener(Hide);
            receivedTab?.onClick.AddListener(() => SwitchTab(true));
            sentTab?.onClick.AddListener(() => SwitchTab(false));
        }

        #region Show/Hide

        public void Show()
        {
            if (panelRoot != null)
                panelRoot.SetActive(true);

            SwitchTab(true);
            LoadRequests();
        }

        public void Hide()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        public void Toggle()
        {
            if (panelRoot != null && panelRoot.activeSelf)
                Hide();
            else
                Show();
        }

        public bool IsVisible => panelRoot != null && panelRoot.activeSelf;

        #endregion

        #region Tabs

        private void SwitchTab(bool received)
        {
            showingReceived = received;

            // Actualizar colores de tabs con DOTween
            if (receivedTab != null)
            {
                var img = receivedTab.GetComponent<Image>();
                if (img != null) img.DOColor(received ? activeTabColor : inactiveTabColor, 0.2f);
                var text = receivedTab.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null) text.DOColor(received ? Color.white : new Color(0.6f, 0.6f, 0.6f), 0.2f);
                receivedTab.transform.DOScale(received ? 1.05f : 1f, 0.2f).SetEase(Ease.OutCubic);
            }
            if (sentTab != null)
            {
                var img = sentTab.GetComponent<Image>();
                if (img != null) img.DOColor(received ? inactiveTabColor : activeTabColor, 0.2f);
                var text = sentTab.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null) text.DOColor(received ? new Color(0.6f, 0.6f, 0.6f) : Color.white, 0.2f);
                sentTab.transform.DOScale(received ? 1f : 1.05f, 0.2f).SetEase(Ease.OutCubic);
            }

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

            // Animate requests list entrance
            AnimateRequestsListEntrance();
        }

        /// <summary>
        /// Anima la entrada de los items de solicitudes con efecto staggered
        /// </summary>
        private void AnimateRequestsListEntrance()
        {
            if (contentContainer == null || contentContainer.childCount == 0) return;

            var seq = DOTween.Sequence();
            for (int i = 0; i < contentContainer.childCount; i++)
            {
                var child = contentContainer.GetChild(i);
                if (!child.gameObject.activeSelf) continue;
                var cg = child.GetComponent<CanvasGroup>();
                if (cg == null) cg = child.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                child.localScale = Vector3.one * 0.85f;
                float delay = i * 0.05f;
                seq.Insert(delay, cg.DOFade(1f, 0.3f).SetEase(Ease.OutQuad));
                seq.Insert(delay, child.DOScale(1f, 0.3f).SetEase(Ease.OutQuad));
            }
        }

        private void CreateRequestItem(FriendRequest request)
        {
            if (requestItemPrefab == null || contentContainer == null)
                return;

            GameObject item = Instantiate(requestItemPrefab, contentContainer);
            currentItems.Add(item);

            // Configurar item
            SetupRequestItem(item, request);
        }

        private void SetupRequestItem(GameObject item, FriendRequest request)
        {
            // Username
            var usernameText = item.transform.Find("Username")?.GetComponent<TextMeshProUGUI>();
            if (usernameText != null)
            {
                usernameText.text = showingReceived ? request.senderUsername : request.receiverUsername;
            }

            // Timestamp
            var timestampText = item.transform.Find("Timestamp")?.GetComponent<TextMeshProUGUI>();
            if (timestampText != null)
            {
                var date = request.GetCreatedAt();
                var diff = System.DateTime.Now - date;
                if (diff.TotalMinutes < 60)
                    timestampText.text = $"Hace {(int)diff.TotalMinutes} min";
                else if (diff.TotalHours < 24)
                    timestampText.text = $"Hace {(int)diff.TotalHours} horas";
                else
                    timestampText.text = $"Hace {(int)diff.TotalDays} dias";
            }

            // Botones
            if (showingReceived)
            {
                // Mostrar Accept y Reject
                var acceptBtn = item.transform.Find("AcceptButton")?.GetComponent<Button>();
                var rejectBtn = item.transform.Find("RejectButton")?.GetComponent<Button>();
                var cancelBtn = item.transform.Find("CancelButton")?.GetComponent<Button>();

                if (acceptBtn != null)
                {
                    acceptBtn.gameObject.SetActive(true);
                    acceptBtn.onClick.AddListener(() => OnAcceptClicked(request.requestId, item));
                }
                if (rejectBtn != null)
                {
                    rejectBtn.gameObject.SetActive(true);
                    rejectBtn.onClick.AddListener(() => OnRejectClicked(request.requestId, item));
                }
                if (cancelBtn != null)
                    cancelBtn.gameObject.SetActive(false);
            }
            else
            {
                // Mostrar Cancel
                var acceptBtn = item.transform.Find("AcceptButton")?.GetComponent<Button>();
                var rejectBtn = item.transform.Find("RejectButton")?.GetComponent<Button>();
                var cancelBtn = item.transform.Find("CancelButton")?.GetComponent<Button>();

                if (acceptBtn != null)
                    acceptBtn.gameObject.SetActive(false);
                if (rejectBtn != null)
                    rejectBtn.gameObject.SetActive(false);
                if (cancelBtn != null)
                {
                    cancelBtn.gameObject.SetActive(true);
                    cancelBtn.onClick.AddListener(() => OnCancelClicked(request.requestId, item));
                }
            }
        }

        private void ClearItems()
        {
            foreach (var item in currentItems)
            {
                if (item != null)
                    Destroy(item);
            }
            currentItems.Clear();
        }

        #endregion

        #region Button Callbacks

        private async void OnAcceptClicked(string requestId, GameObject item)
        {
            Debug.Log($"[FriendRequests] Aceptando solicitud: {requestId}");

            var result = await FriendService.Instance.AcceptFriendRequest(requestId);

            if (result.Success)
            {
                // Remover item de la lista
                if (item != null)
                {
                    currentItems.Remove(item);
                    Destroy(item);
                }
                UpdatePendingBadge();
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
                if (item != null)
                {
                    currentItems.Remove(item);
                    Destroy(item);
                }
                UpdatePendingBadge();
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
                if (item != null)
                {
                    currentItems.Remove(item);
                    Destroy(item);
                }
                CheckEmptyState();
            }
            else
            {
                Debug.LogWarning($"[FriendRequests] Error: {result.Message}");
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

        private void AnimateEmptyText()
        {
            if (emptyText == null) return;
            var cg = emptyText.GetComponent<CanvasGroup>();
            if (cg == null) cg = emptyText.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.DOFade(1f, 0.4f).SetEase(Ease.OutQuad);
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
            UpdatePendingBadge();
            if (IsVisible && showingReceived)
            {
                CreateRequestItem(request);
            }
        }

        private void OnRequestResponded(FriendRequest request)
        {
            UpdatePendingBadge();
        }

        #endregion

        #region Badge

        public void UpdatePendingBadge()
        {
            int count = FriendService.Instance?.GetPendingRequestsCount() ?? 0;

            if (pendingBadge != null)
            {
                if (count > 0)
                {
                    pendingBadge.SetActive(true);
                    pendingBadge.transform.localScale = Vector3.zero;
                    pendingBadge.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
                }
                else
                {
                    pendingBadge.transform.DOScale(0f, 0.15f).SetEase(Ease.InBack)
                        .OnComplete(() => pendingBadge.SetActive(false));
                }
            }

            if (pendingCountText != null)
            {
                pendingCountText.text = count > 99 ? "99+" : count.ToString();
            }
        }

        public int GetPendingCount()
        {
            return FriendService.Instance?.GetPendingRequestsCount() ?? 0;
        }

        #endregion
    }
}
