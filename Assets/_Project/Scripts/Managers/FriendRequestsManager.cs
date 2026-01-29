using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DigitPark.Services;
using DigitPark.Data;

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

            // Actualizar colores de tabs
            if (receivedTab != null)
            {
                var img = receivedTab.GetComponent<Image>();
                if (img != null) img.color = received ? activeTabColor : inactiveTabColor;
            }
            if (sentTab != null)
            {
                var img = sentTab.GetComponent<Image>();
                if (img != null) img.color = received ? inactiveTabColor : activeTabColor;
            }

            LoadRequests();
        }

        #endregion

        #region Load Requests

        private async void LoadRequests()
        {
            ClearItems();

            if (loadingIndicator != null)
                loadingIndicator.SetActive(true);
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

            if (loadingIndicator != null)
                loadingIndicator.SetActive(false);

            if (requests == null || requests.Count == 0)
            {
                if (emptyText != null)
                {
                    emptyText.text = showingReceived
                        ? "No tienes solicitudes pendientes"
                        : "No has enviado solicitudes";
                    emptyText.gameObject.SetActive(true);
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
                pendingBadge.SetActive(count > 0);
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
