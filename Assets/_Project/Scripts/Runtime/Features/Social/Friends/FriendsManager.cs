using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using DigitPark.Services;
using DigitPark.Services.Firebase;
using DigitPark.Data;
using DigitPark.UI;
using DigitPark.UI.Components;
using DigitPark.Localization;
using DG.Tweening;
using DigitPark.Animations;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager para la escena dedicada de Amigos.
    /// Muestra lista de amigos con estado online, nivel, stats.
    /// Permite buscar, ver perfil, retar y eliminar amigos.
    /// Navega a FriendRequests para solicitudes.
    /// </summary>
    public class FriendsManager : MonoBehaviour
    {
        [Header("UI - Header")]
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI friendsCountText;

        [Header("UI - Search")]
        [SerializeField] private TMP_InputField searchInput;

        [Header("UI - Requests Navigation")]
        [SerializeField] private Button requestsButton;
        [SerializeField] private GameObject requestsBadge;
        [SerializeField] private TextMeshProUGUI requestsBadgeText;

        [Header("UI - Content")]
        [SerializeField] private Transform scrollContent;
        [SerializeField] private GameObject friendCardPrefab;
        [SerializeField] private TextMeshProUGUI emptyText;
        [SerializeField] private GameObject loadingIndicator;

        [Header("UI - Sections (for animations)")]
        [SerializeField] private RectTransform headerTransform;
        [SerializeField] private RectTransform searchBarTransform;
        [SerializeField] private RectTransform requestsNavTransform;
        [SerializeField] private RectTransform scrollViewTransform;

        private List<GameObject> currentCards = new List<GameObject>();
        private List<FriendInfo> allFriends = new List<FriendInfo>();
        private string returnScene = "Profile";

        #region Unity Lifecycle

        private void Start()
        {
            Debug.Log("[Friends] FriendsManager iniciado");

            SetupListeners();

            returnScene = PlayerPrefs.GetString("DP_FriendsReturnScene", "Profile");
            PlayerPrefs.DeleteKey("DP_FriendsReturnScene");

            AnimateEntrance();
            LoadFriends();
            UpdateRequestsBadge();
        }

        private void OnEnable()
        {
            if (FriendService.Instance != null)
            {
                FriendService.Instance.OnFriendsListChanged += OnFriendsListChanged;
            }
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
            // Disable auto-navigation from BackButton prefab to prevent double listener
            var autoNav = backButton?.GetComponent<DigitPark.UI.BackButton>();
            if (autoNav != null) autoNav.DisableAutoNavigation();
            backButton?.onClick.AddListener(OnBackClicked);
            requestsButton?.onClick.AddListener(OnRequestsClicked);

            if (searchInput != null)
            {
                searchInput.onValueChanged.AddListener(OnSearchChanged);
            }
        }

        #endregion

        #region Load Friends

        private async void LoadFriends()
        {
            try
            {
                if (FriendService.Instance == null)
                {
                    Debug.LogWarning("[FriendsManager] FriendService not available");
                    return;
                }

                ClearCards();

                ShowLoadingIndicator(true);
                if (emptyText != null)
                    emptyText.gameObject.SetActive(false);

                allFriends = await FriendService.Instance.GetFriendsList();

                // Null check post-await: this MonoBehaviour may have been destroyed during await
                if (this == null) return;

                ShowLoadingIndicator(false);

                UpdateFriendsCount();

                if (allFriends == null || allFriends.Count == 0)
                {
                    if (emptyText != null)
                    {
                        emptyText.text = AutoLocalizer.Get("friends_no_friends");
                        emptyText.gameObject.SetActive(true);
                        AnimateEmptyText();
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
                    CreateFriendCard(friend);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FriendsManager] {ex.Message}");
            }
        }

        private void CreateFriendCard(FriendInfo friend)
        {
            if (friendCardPrefab == null || scrollContent == null) return;

            GameObject card = Instantiate(friendCardPrefab, scrollContent);
            currentCards.Add(card);
            SetupFriendCard(card, friend);

            // Apply theme to card
            var usernameGO = card.transform.Find("InfoSection/Username")?.gameObject;
            var statsGO = card.transform.Find("InfoSection/StatsText")?.gameObject;
            var statusGO = card.transform.Find("InfoSection/StatusText")?.gameObject;

            // Animacion de entrada staggered
            int index = currentCards.Count - 1;
            card.transform.localScale = Vector3.zero;
            card.transform.DOScale(1f, 0.3f)
                .SetDelay(index * 0.06f)
                .SetEase(Ease.OutBack)
                .SetLink(card);
        }

        private void SetupFriendCard(GameObject card, FriendInfo friend)
        {
            // Online indicator
            var onlineIndicator = card.transform.Find("OnlineIndicator")?.GetComponent<Image>();
            if (onlineIndicator != null)
            {
                onlineIndicator.color = friend.isOnline
                    ? new Color(0.2f, 1f, 0.4f, 1f)
                    : new Color(0.4f, 0.4f, 0.4f, 1f);
            }

            // Username
            var usernameText = card.transform.Find("InfoSection/Username")?.GetComponent<TextMeshProUGUI>();
            if (usernameText != null)
                usernameText.text = UICanvasHelper.TmpSafe(friend.username);

            // Stats
            var statsText = card.transform.Find("InfoSection/StatsText")?.GetComponent<TextMeshProUGUI>();
            if (statsText != null)
                statsText.text = $"{friend.winRate:F0}% {AutoLocalizer.Get("win_rate_abbr")} \u00B7 {friend.favoriteGame}";

            // Status (includes level)
            var statusText = card.transform.Find("InfoSection/StatusText")?.GetComponent<TextMeshProUGUI>();
            if (statusText != null)
            {
                string levelTag = friend.level > 0 ? $"Lv. {friend.level} \u00B7 " : "";
                string statusStr = friend.isOnline ? AutoLocalizer.Get("player_online") : friend.GetLastSeenText();
                statusText.text = $"{levelTag}{statusStr}";
                statusText.color = friend.isOnline
                    ? new Color(0.2f, 1f, 0.4f, 1f)
                    : new Color(0.5f, 0.5f, 0.5f, 1f);
            }

            // Buttons
            string odId = friend.odId;

            var viewProfileBtn = card.transform.Find("ButtonsRow/ViewProfileButton")?.GetComponent<Button>();
            if (viewProfileBtn != null)
                viewProfileBtn.onClick.AddListener(() => OnViewProfileClicked(odId));

            var challengeBtn = card.transform.Find("ButtonsRow/ChallengeButton")?.GetComponent<Button>();
            if (challengeBtn != null)
            {
                challengeBtn.onClick.AddListener(() => OnChallengeClicked(odId));
                challengeBtn.gameObject.SetActive(friend.isOnline);
            }

            var removeBtn = card.transform.Find("ButtonsRow/RemoveButton")?.GetComponent<Button>();
            if (removeBtn != null)
                removeBtn.onClick.AddListener(() => OnRemoveFriendClicked(odId, card));
        }

        private void ClearCards()
        {
            foreach (var card in currentCards)
            {
                if (card != null) Destroy(card);
            }
            currentCards.Clear();
        }

        private void UpdateFriendsCount()
        {
            int count = allFriends?.Count ?? 0;
            int onlineCount = allFriends?.FindAll(f => f.isOnline).Count ?? 0;

            if (friendsCountText != null)
                friendsCountText.text = AutoLocalizer.Get("friends_count", count, count != 1 ? AutoLocalizer.Get("friends_count_plural") : "");

            if (titleText != null)
            {
                titleText.text = onlineCount > 0
                    ? AutoLocalizer.Get("friends_online_count", onlineCount)
                    : AutoLocalizer.Get("friends_title");
            }
        }

        #endregion

        #region Search

        private void OnSearchChanged(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                RefreshDisplay(allFriends);
                return;
            }

            var filtered = allFriends.FindAll(f =>
                f.username.ToLower().Contains(query.ToLower()));
            RefreshDisplay(filtered);
        }

        private void RefreshDisplay(List<FriendInfo> friendsToShow)
        {
            ClearCards();

            if (friendsToShow == null || friendsToShow.Count == 0)
            {
                if (emptyText != null)
                {
                    emptyText.text = AutoLocalizer.Get("friends_no_friends");
                    emptyText.gameObject.SetActive(true);
                    AnimateEmptyText();
                }
                return;
            }

            if (emptyText != null)
                emptyText.gameObject.SetActive(false);

            foreach (var friend in friendsToShow)
            {
                CreateFriendCard(friend);
            }
        }

        #endregion

        #region Button Callbacks

        private void OnBackClicked()
        {
            Debug.Log($"[Friends] Volviendo a: {returnScene}");
            SceneManager.LoadScene(returnScene);
        }

        private void OnRequestsClicked()
        {
            Debug.Log("[Friends] Navegando a solicitudes");
            PlayerPrefs.SetString("DP_FriendRequestsReturnScene", "Friends");
            PlayerPrefs.Save();
            SceneManager.LoadScene("FriendRequests");
        }

        private void OnViewProfileClicked(string odId)
        {
            Debug.Log($"[Friends] Ver perfil: {odId}");
            PlayerPrefs.SetString("DP_ViewProfileId", odId);
            PlayerPrefs.SetString("DP_ProfileReturnScene", "Friends");
            PlayerPrefs.Save();
            SceneManager.LoadScene("Profile");
        }

        private void OnChallengeClicked(string odId)
        {
            Debug.Log($"[Friends] Retar a: {odId}");
            PlayerPrefs.SetString("DP_ChallengePlayerId", odId);
            PlayerPrefs.Save();
            InAppNotificationManager.Instance?.Show(AutoLocalizer.Get("feature_coming_soon"), "", "info");
        }

        private void OnRemoveFriendClicked(string odId, GameObject card)
        {
            PopupManager.Instance?.ShowConfirmMessage(
                AutoLocalizer.Get("confirm_remove_friend"),
                () => DoRemoveFriend(odId, card));
        }

        private async void DoRemoveFriend(string odId, GameObject card)
        {
            try
            {
                Debug.Log($"[Friends] Eliminar amigo: {odId}");

                // Disable the remove button to prevent duplicate requests
                var removeBtn = card?.transform.Find("ButtonsRow/RemoveButton")?.GetComponent<Button>();
                if (removeBtn != null) removeBtn.interactable = false;

                var result = await FriendService.Instance.RemoveFriend(odId);

                if (result.Success)
                {
                    if (card != null)
                    {
                        currentCards.Remove(card);
                        card.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack)
                            .SetLink(card).OnComplete(() => { if (card != null) Destroy(card); });
                    }

                    allFriends.RemoveAll(f => f.odId == odId);
                    UpdateFriendsCount();

                    if (allFriends.Count == 0 && emptyText != null)
                    {
                        emptyText.text = AutoLocalizer.Get("friends_no_friends");
                        emptyText.gameObject.SetActive(true);
                        AnimateEmptyText();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FriendsManager] {ex.Message}");
            }
        }

        #endregion

        #region Requests Badge

        private void UpdateRequestsBadge()
        {
            int count = FriendService.Instance?.GetPendingRequestsCount() ?? 0;

            if (requestsBadge != null)
            {
                if (count > 0)
                {
                    requestsBadge.SetActive(true);
                    requestsBadge.transform.localScale = Vector3.zero;
                    requestsBadge.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack).SetLink(requestsBadge);
                }
                else
                {
                    requestsBadge.transform.DOScale(0f, 0.15f).SetEase(Ease.InBack)
                        .SetLink(requestsBadge).OnComplete(() => requestsBadge.SetActive(false));
                }
            }

            if (requestsBadgeText != null)
                requestsBadgeText.text = count > 99 ? "99+" : count.ToString();
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

            // Search bar fade + slide
            if (searchBarTransform != null)
            {
                var cg = searchBarTransform.GetComponent<CanvasGroup>() ?? searchBarTransform.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                Vector2 pos = searchBarTransform.anchoredPosition;
                searchBarTransform.anchoredPosition = new Vector2(pos.x - 100, pos.y);
                DOTween.Sequence()
                    .AppendInterval(0.15f)
                    .Append(searchBarTransform.DOAnchorPos(pos, 0.35f).SetEase(Ease.OutCubic))
                    .Join(cg.DOFade(1f, 0.35f))
                    .SetLink(gameObject);
            }

            // Requests nav fade + slide
            if (requestsNavTransform != null)
            {
                var cg = requestsNavTransform.GetComponent<CanvasGroup>() ?? requestsNavTransform.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                Vector2 pos = requestsNavTransform.anchoredPosition;
                requestsNavTransform.anchoredPosition = new Vector2(pos.x + 100, pos.y);
                DOTween.Sequence()
                    .AppendInterval(0.25f)
                    .Append(requestsNavTransform.DOAnchorPos(pos, 0.35f).SetEase(Ease.OutCubic))
                    .Join(cg.DOFade(1f, 0.35f))
                    .SetLink(gameObject);
            }

            // ScrollView fade in
            if (scrollViewTransform != null)
            {
                var cg = scrollViewTransform.GetComponent<CanvasGroup>() ?? scrollViewTransform.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                DOTween.Sequence()
                    .AppendInterval(0.3f)
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

        private void OnFriendsListChanged()
        {
            LoadFriends();
            UpdateRequestsBadge();
        }

        #endregion

        private void OnDestroy()
        {
            transform.DOKill();

            // Unsubscribe input listener to prevent leak
            searchInput?.onValueChanged.RemoveListener(OnSearchChanged);

            // Kill tweens on child objects (cards, indicators, etc.)
            foreach (var card in currentCards)
            {
                if (card != null) card.transform.DOKill();
            }
            if (headerTransform != null) headerTransform.DOKill();
            if (searchBarTransform != null) searchBarTransform.DOKill();
            if (requestsNavTransform != null) requestsNavTransform.DOKill();
            if (scrollViewTransform != null) scrollViewTransform.DOKill();
            if (requestsBadge != null) requestsBadge.transform.DOKill();
        }
    }
}
