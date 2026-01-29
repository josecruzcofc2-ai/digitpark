using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using DigitPark.Services;
using DigitPark.Data;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager para la lista de amigos
    /// Puede ser usado como popup o como escena dedicada
    /// </summary>
    public class FriendsListManager : MonoBehaviour
    {
        [Header("UI - Panel")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button overlayButton;

        [Header("UI - Header")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI friendsCountText;
        [SerializeField] private Button requestsButton;
        [SerializeField] private GameObject requestsBadge;
        [SerializeField] private TextMeshProUGUI requestsBadgeText;

        [Header("UI - Content")]
        [SerializeField] private Transform contentContainer;
        [SerializeField] private GameObject friendItemPrefab;
        [SerializeField] private TextMeshProUGUI emptyText;
        [SerializeField] private GameObject loadingIndicator;

        [Header("UI - Search")]
        [SerializeField] private TMP_InputField searchInput;

        [Header("References")]
        [SerializeField] private FriendRequestsManager requestsManager;

        private List<GameObject> currentItems = new List<GameObject>();
        private List<FriendInfo> allFriends = new List<FriendInfo>();

        private void Start()
        {
            SetupListeners();
            Hide();
        }

        private void OnEnable()
        {
            if (FriendService.Instance != null)
            {
                FriendService.Instance.OnFriendsListChanged += OnFriendsListChanged;
            }
            UpdateRequestsBadge();
        }

        private void OnDisable()
        {
            if (FriendService.Instance != null)
            {
                FriendService.Instance.OnFriendsListChanged -= OnFriendsListChanged;
            }
        }

        private void SetupListeners()
        {
            closeButton?.onClick.AddListener(Hide);
            overlayButton?.onClick.AddListener(Hide);
            requestsButton?.onClick.AddListener(OnRequestsClicked);

            if (searchInput != null)
            {
                searchInput.onValueChanged.AddListener(OnSearchChanged);
            }
        }

        #region Show/Hide

        public void Show()
        {
            if (panelRoot != null)
                panelRoot.SetActive(true);

            LoadFriends();
            UpdateRequestsBadge();
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

        #region Load Friends

        private async void LoadFriends()
        {
            ClearItems();

            if (loadingIndicator != null)
                loadingIndicator.SetActive(true);
            if (emptyText != null)
                emptyText.gameObject.SetActive(false);

            allFriends = await FriendService.Instance.GetFriendsList();

            if (loadingIndicator != null)
                loadingIndicator.SetActive(false);

            // Actualizar contador
            UpdateFriendsCount();

            if (allFriends == null || allFriends.Count == 0)
            {
                if (emptyText != null)
                {
                    emptyText.text = "No tienes amigos aun\nBusca jugadores para agregarlos";
                    emptyText.gameObject.SetActive(true);
                }
                return;
            }

            // Ordenar: online primero, luego alfabeticamente
            allFriends.Sort((a, b) =>
            {
                if (a.isOnline != b.isOnline)
                    return b.isOnline.CompareTo(a.isOnline);
                return a.username.CompareTo(b.username);
            });

            foreach (var friend in allFriends)
            {
                CreateFriendItem(friend);
            }
        }

        private void CreateFriendItem(FriendInfo friend)
        {
            if (friendItemPrefab == null || contentContainer == null)
                return;

            GameObject item = Instantiate(friendItemPrefab, contentContainer);
            currentItems.Add(item);

            SetupFriendItem(item, friend);
        }

        private void SetupFriendItem(GameObject item, FriendInfo friend)
        {
            // Username
            var usernameText = item.transform.Find("Username")?.GetComponent<TextMeshProUGUI>();
            if (usernameText != null)
            {
                usernameText.text = friend.username;
            }

            // Win Rate
            var winRateText = item.transform.Find("WinRate")?.GetComponent<TextMeshProUGUI>();
            if (winRateText != null)
            {
                winRateText.text = $"{friend.winRate:F0}%";
            }

            // Favorite Game
            var favGameText = item.transform.Find("FavoriteGame")?.GetComponent<TextMeshProUGUI>();
            if (favGameText != null)
            {
                favGameText.text = friend.favoriteGame ?? "";
            }

            // Online Status
            var onlineIndicator = item.transform.Find("OnlineIndicator")?.GetComponent<Image>();
            if (onlineIndicator != null)
            {
                onlineIndicator.color = friend.isOnline
                    ? new Color(0.2f, 1f, 0.4f, 1f)   // Verde
                    : new Color(0.5f, 0.5f, 0.5f, 1f); // Gris
            }

            var statusText = item.transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
            if (statusText != null)
            {
                statusText.text = friend.GetLastSeenText();
                statusText.color = friend.isOnline
                    ? new Color(0.2f, 1f, 0.4f, 1f)
                    : new Color(0.5f, 0.5f, 0.5f, 1f);
            }

            // Botones
            var viewProfileBtn = item.transform.Find("ViewProfileButton")?.GetComponent<Button>();
            if (viewProfileBtn != null)
            {
                string odId = friend.odId;
                viewProfileBtn.onClick.AddListener(() => OnViewProfileClicked(odId));
            }

            var challengeBtn = item.transform.Find("ChallengeButton")?.GetComponent<Button>();
            if (challengeBtn != null)
            {
                string odId = friend.odId;
                challengeBtn.onClick.AddListener(() => OnChallengeClicked(odId));
                // Solo habilitar si esta online
                challengeBtn.interactable = friend.isOnline;
            }

            var removeBtn = item.transform.Find("RemoveButton")?.GetComponent<Button>();
            if (removeBtn != null)
            {
                string odId = friend.odId;
                removeBtn.onClick.AddListener(() => OnRemoveFriendClicked(odId, item));
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

        private void UpdateFriendsCount()
        {
            int count = allFriends?.Count ?? 0;

            if (friendsCountText != null)
            {
                friendsCountText.text = $"{count} amigo{(count != 1 ? "s" : "")}";
            }

            if (titleText != null && count > 0)
            {
                int onlineCount = allFriends.FindAll(f => f.isOnline).Count;
                if (onlineCount > 0)
                {
                    titleText.text = $"Amigos ({onlineCount} en linea)";
                }
                else
                {
                    titleText.text = "Amigos";
                }
            }
        }

        #endregion

        #region Search

        private void OnSearchChanged(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                // Mostrar todos
                RefreshDisplay(allFriends);
                return;
            }

            // Filtrar por nombre
            var filtered = allFriends.FindAll(f =>
                f.username.ToLower().Contains(query.ToLower())
            );

            RefreshDisplay(filtered);
        }

        private void RefreshDisplay(List<FriendInfo> friendsToShow)
        {
            ClearItems();

            if (friendsToShow == null || friendsToShow.Count == 0)
            {
                if (emptyText != null)
                {
                    emptyText.text = "No se encontraron amigos";
                    emptyText.gameObject.SetActive(true);
                }
                return;
            }

            if (emptyText != null)
                emptyText.gameObject.SetActive(false);

            foreach (var friend in friendsToShow)
            {
                CreateFriendItem(friend);
            }
        }

        #endregion

        #region Button Callbacks

        private void OnViewProfileClicked(string odId)
        {
            Debug.Log($"[FriendsList] Ver perfil de: {odId}");

            PlayerPrefs.SetString("ViewProfileId", odId);
            PlayerPrefs.SetString("ProfileReturnScene", SceneManager.GetActiveScene().name);
            PlayerPrefs.Save();

            Hide();
            SceneManager.LoadScene("Profile");
        }

        private void OnChallengeClicked(string odId)
        {
            Debug.Log($"[FriendsList] Retar a: {odId}");

            PlayerPrefs.SetString("ChallengePlayerId", odId);
            PlayerPrefs.Save();

            Hide();
            // TODO: Abrir selector de juego o ir a PlayModeSelection
        }

        private async void OnRemoveFriendClicked(string odId, GameObject item)
        {
            Debug.Log($"[FriendsList] Eliminar amigo: {odId}");

            // TODO: Mostrar confirmacion primero

            var result = await FriendService.Instance.RemoveFriend(odId);

            if (result.Success)
            {
                if (item != null)
                {
                    currentItems.Remove(item);
                    Destroy(item);
                }

                // Actualizar lista local
                allFriends.RemoveAll(f => f.odId == odId);
                UpdateFriendsCount();

                if (allFriends.Count == 0 && emptyText != null)
                {
                    emptyText.text = "No tienes amigos aun\nBusca jugadores para agregarlos";
                    emptyText.gameObject.SetActive(true);
                }
            }
            else
            {
                Debug.LogWarning($"[FriendsList] Error: {result.Message}");
            }
        }

        private void OnRequestsClicked()
        {
            if (requestsManager != null)
            {
                requestsManager.Show();
            }
        }

        #endregion

        #region Events

        private void OnFriendsListChanged()
        {
            if (IsVisible)
            {
                LoadFriends();
            }
        }

        #endregion

        #region Badge

        private void UpdateRequestsBadge()
        {
            int count = FriendService.Instance?.GetPendingRequestsCount() ?? 0;

            if (requestsBadge != null)
            {
                requestsBadge.SetActive(count > 0);
            }

            if (requestsBadgeText != null)
            {
                requestsBadgeText.text = count > 99 ? "99+" : count.ToString();
            }
        }

        #endregion
    }
}
