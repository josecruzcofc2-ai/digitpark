using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using DigitPark.Services;
using DigitPark.Services.Firebase;
using DG.Tweening;
using DigitPark.UI;
using DigitPark.Localization;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager para la escena dedicada de Notificaciones.
    /// Muestra lista de notificaciones con filtros por categoría,
    /// acciones inline (aceptar/rechazar), marcar como leída,
    /// y navegación a escenas relevantes.
    ///
    /// Escena: Assets/_Project/Scenes/Social/Notifications.unity
    /// </summary>
    public class NotificationsManager : MonoBehaviour
    {
        [Header("UI - Header")]
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI countText;

        [Header("UI - Tabs")]
        [SerializeField] private Button tabAll;
        [SerializeField] private Button tabSocial;
        [SerializeField] private Button tabGames;
        [SerializeField] private Button tabRewards;
        [SerializeField] private Image tabAllIndicator;
        [SerializeField] private Image tabSocialIndicator;
        [SerializeField] private Image tabGamesIndicator;
        [SerializeField] private Image tabRewardsIndicator;

        [Header("UI - Content")]
        [SerializeField] private Transform scrollContent;
        [SerializeField] private GameObject notificationCardPrefab;
        [SerializeField] private TextMeshProUGUI emptyText;
        [SerializeField] private GameObject loadingIndicator;

        [Header("UI - Footer")]
        [SerializeField] private Button markAllReadButton;
        [SerializeField] private TextMeshProUGUI markAllReadText;

        [Header("UI - Sections (for animations)")]
        [SerializeField] private RectTransform headerTransform;
        [SerializeField] private RectTransform tabsTransform;
        [SerializeField] private RectTransform scrollViewTransform;
        [SerializeField] private RectTransform footerTransform;

        // State
        private List<GameObject> currentCards = new List<GameObject>();
        private List<StoredNotification> currentNotifications = new List<StoredNotification>();
        private NotificationCategory activeCategory = NotificationCategory.All;
        private string returnScene = "MainMenu";

        // Icon sprite cache
        private Dictionary<string, Sprite> _iconCache = new Dictionary<string, Sprite>();

        // Colors
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color TAB_INACTIVE = new Color(0.3f, 0.3f, 0.35f, 1f);
        private static readonly Color SOCIAL_COLOR = new Color(0.4f, 0.6f, 1f, 1f);
        private static readonly Color GAMES_COLOR = new Color(1f, 0.5f, 0f, 1f);
        private static readonly Color REWARDS_COLOR = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color UNREAD_BG = new Color(0.06f, 0.1f, 0.16f, 1f);
        private static readonly Color READ_BG = new Color(0.04f, 0.06f, 0.1f, 1f);

        #region Unity Lifecycle

        private void Start()
        {
            Debug.Log("[Notifications] NotificationsManager iniciado");

            if (titleText != null)
                titleText.text = AutoLocalizer.Get("notifications_title");

            SetupListeners();

            returnScene = PlayerPrefs.GetString("NotificationsReturnScene", "MainMenu");
            PlayerPrefs.DeleteKey("NotificationsReturnScene");

            AnimateEntrance();
            SwitchTab(NotificationCategory.All);
        }

        private void OnEnable()
        {
            if (NotificationStorageService.Instance != null)
            {
                NotificationStorageService.Instance.OnNotificationsChanged += OnNotificationsChanged;
            }
        }

        private void OnDisable()
        {
            if (NotificationStorageService.Instance != null)
            {
                NotificationStorageService.Instance.OnNotificationsChanged -= OnNotificationsChanged;
            }
        }

        private void SetupListeners()
        {
            // Disable auto-navigation from BackButton prefab to prevent double listener
            var autoNav = backButton?.GetComponent<DigitPark.UI.BackButton>();
            if (autoNav != null) autoNav.DisableAutoNavigation();
            backButton?.onClick.AddListener(OnBackClicked);

            tabAll?.onClick.AddListener(() => SwitchTab(NotificationCategory.All));
            tabSocial?.onClick.AddListener(() => SwitchTab(NotificationCategory.Social));
            tabGames?.onClick.AddListener(() => SwitchTab(NotificationCategory.Games));
            tabRewards?.onClick.AddListener(() => SwitchTab(NotificationCategory.Rewards));

            markAllReadButton?.onClick.AddListener(OnMarkAllReadClicked);
        }

        #endregion

        #region Load Notifications

        private void LoadNotifications()
        {
            ClearCards();

            if (NotificationStorageService.Instance == null)
            {
                ShowEmpty(AutoLocalizer.Get("notif_service_unavailable"));
                return;
            }

            currentNotifications = NotificationStorageService.Instance.GetByCategory(activeCategory);
            UpdateCountText();
            UpdateMarkAllReadButton();

            if (currentNotifications == null || currentNotifications.Count == 0)
            {
                ShowEmpty(GetEmptyMessage());
                return;
            }

            if (emptyText != null)
                emptyText.gameObject.SetActive(false);

            // Agrupar por tiempo
            string currentGroup = "";
            for (int i = 0; i < currentNotifications.Count; i++)
            {
                var notification = currentNotifications[i];
                string group = notification.GetTimeGroup();

                // Crear separador de grupo si cambió
                if (group != currentGroup)
                {
                    currentGroup = group;
                    CreateGroupSeparator(group);
                }

                CreateNotificationCard(notification, i);
            }
        }

        private void CreateGroupSeparator(string groupName)
        {
            var separator = new GameObject($"Separator_{groupName}");
            separator.transform.SetParent(scrollContent, false);
            currentCards.Add(separator);

            var rt = separator.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 35);
            separator.AddComponent<LayoutElement>().preferredHeight = 35;

            var tmp = separator.AddComponent<TextMeshProUGUI>();
            tmp.text = $"— {groupName} —";
            tmp.fontSize = FontSizes.Body;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinBody;
            tmp.fontSizeMax = tmp.fontSize;
            tmp.color = new Color(0.5f, 0.5f, 0.55f, 1f);
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        private void CreateNotificationCard(StoredNotification notification, int index)
        {
            if (notificationCardPrefab == null || scrollContent == null) return;

            GameObject card = Instantiate(notificationCardPrefab, scrollContent);
            currentCards.Add(card);
            SetupNotificationCard(card, notification);

            // Animación de entrada staggered
            card.transform.localScale = Vector3.zero;
            card.transform.DOScale(1f, 0.25f)
                .SetDelay(index * 0.04f)
                .SetEase(Ease.OutBack);
        }

        private void SetupNotificationCard(GameObject card, StoredNotification notification)
        {
            // Background color based on read state
            var cardBg = card.GetComponent<Image>();
            if (cardBg != null)
                cardBg.color = notification.isRead ? READ_BG : UNREAD_BG;

            // Unread indicator
            var unreadDot = card.transform.Find("UnreadDot");
            if (unreadDot != null)
            {
                if (!notification.isRead)
                {
                    unreadDot.gameObject.SetActive(true);
                    unreadDot.localScale = Vector3.zero;
                    unreadDot.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
                }
                else
                {
                    unreadDot.gameObject.SetActive(false);
                }
            }

            // Type icon sprite
            var typeIconImg = card.transform.Find("TypeIcon/IconImage")?.GetComponent<Image>();
            if (typeIconImg != null)
            {
                typeIconImg.sprite = GetTypeIconSprite(notification.type);
                typeIconImg.color = GetTypeColor(notification.type);
            }

            // Title
            var titleTmp = card.transform.Find("InfoSection/Title")?.GetComponent<TextMeshProUGUI>();
            if (titleTmp != null)
            {
                titleTmp.text = notification.title;
                titleTmp.color = notification.isRead
                    ? new Color(0.7f, 0.7f, 0.7f, 1f)
                    : new Color(0.95f, 0.95f, 0.95f, 1f);
            }

            // Body
            var bodyTmp = card.transform.Find("InfoSection/Body")?.GetComponent<TextMeshProUGUI>();
            if (bodyTmp != null)
                bodyTmp.text = notification.body;

            // Timestamp
            var timeTmp = card.transform.Find("InfoSection/Timestamp")?.GetComponent<TextMeshProUGUI>();
            if (timeTmp != null)
                timeTmp.text = notification.GetRelativeTime();

            // Sender name (if applicable)
            var senderTmp = card.transform.Find("InfoSection/SenderName")?.GetComponent<TextMeshProUGUI>();
            if (senderTmp != null)
            {
                bool hasSender = !string.IsNullOrEmpty(notification.senderName);
                senderTmp.gameObject.SetActive(hasSender);
                if (hasSender) senderTmp.text = notification.senderName;
            }

            // Action buttons based on type
            SetupCardActions(card, notification);

            // Card tap → mark as read
            var cardButton = card.GetComponent<Button>();
            if (cardButton != null)
            {
                string nId = notification.id;
                cardButton.onClick.AddListener(() => OnNotificationTapped(nId, notification));
            }
        }

        private void SetupCardActions(GameObject card, StoredNotification notification)
        {
            var primaryBtn = card.transform.Find("ActionsRow/PrimaryButton")?.GetComponent<Button>();
            var primaryText = card.transform.Find("ActionsRow/PrimaryButton/Text")?.GetComponent<TextMeshProUGUI>();
            var secondaryBtn = card.transform.Find("ActionsRow/SecondaryButton")?.GetComponent<Button>();
            var secondaryText = card.transform.Find("ActionsRow/SecondaryButton/Text")?.GetComponent<TextMeshProUGUI>();
            var actionsRow = card.transform.Find("ActionsRow");

            if (actionsRow == null) return;

            // Clear previous listeners before assigning new ones to prevent duplicates on list refresh
            primaryBtn?.onClick.RemoveAllListeners();
            secondaryBtn?.onClick.RemoveAllListeners();

            string nId = notification.id;

            switch (notification.type)
            {
                case "friend_request":
                    SetActionButton(primaryBtn, primaryText, AutoLocalizer.Get("notif_accept"), true);
                    SetActionButton(secondaryBtn, secondaryText, AutoLocalizer.Get("notif_reject"), true);
                    primaryBtn?.onClick.AddListener(() => OnAcceptFriendRequest(nId, notification));
                    secondaryBtn?.onClick.AddListener(() => OnRejectFriendRequest(nId, notification));
                    break;

                case "friend_accepted":
                    SetActionButton(primaryBtn, primaryText, AutoLocalizer.Get("notif_view_profile"), true);
                    SetActionButton(secondaryBtn, secondaryText, null, false);
                    primaryBtn?.onClick.AddListener(() => OnViewProfile(notification.senderId));
                    break;

                case "challenge":
                    SetActionButton(primaryBtn, primaryText, AutoLocalizer.Get("notif_accept"), true);
                    SetActionButton(secondaryBtn, secondaryText, AutoLocalizer.Get("notif_reject"), true);
                    primaryBtn?.onClick.AddListener(() => OnAcceptChallenge(nId, notification));
                    secondaryBtn?.onClick.AddListener(() => OnRejectChallenge(nId, notification));
                    break;

                case "tournament_start":
                    SetActionButton(primaryBtn, primaryText, AutoLocalizer.Get("notif_join"), true);
                    SetActionButton(secondaryBtn, secondaryText, null, false);
                    primaryBtn?.onClick.AddListener(() => OnJoinTournament(notification.targetId));
                    break;

                case "tournament_result":
                    SetActionButton(primaryBtn, primaryText, AutoLocalizer.Get("notif_view_results"), true);
                    SetActionButton(secondaryBtn, secondaryText, null, false);
                    primaryBtn?.onClick.AddListener(() => OnViewTournamentResults(notification.targetId));
                    break;

                case "daily_reward":
                    SetActionButton(primaryBtn, primaryText, AutoLocalizer.Get("notif_claim"), true);
                    SetActionButton(secondaryBtn, secondaryText, null, false);
                    primaryBtn?.onClick.AddListener(() => OnClaimDailyReward());
                    break;

                case "promo":
                    SetActionButton(primaryBtn, primaryText, AutoLocalizer.Get("notif_view_offer"), true);
                    SetActionButton(secondaryBtn, secondaryText, null, false);
                    primaryBtn?.onClick.AddListener(() => OnViewPromo(notification.action));
                    break;

                default:
                    actionsRow.gameObject.SetActive(false);
                    break;
            }
        }

        private void SetActionButton(Button btn, TextMeshProUGUI text, string label, bool active)
        {
            if (btn != null) btn.gameObject.SetActive(active);
            if (text != null && label != null) text.text = label;
        }

        private void ClearCards()
        {
            foreach (var card in currentCards)
            {
                if (card != null) Destroy(card);
            }
            currentCards.Clear();
        }

        private void ShowEmpty(string message)
        {
            if (emptyText != null)
            {
                emptyText.text = message;
                emptyText.gameObject.SetActive(true);

                // Animated fade-in for empty state
                var cg = emptyText.GetComponent<CanvasGroup>();
                if (cg == null) cg = emptyText.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                cg.DOFade(1f, 0.4f).SetEase(Ease.OutQuad);
            }
        }

        private string GetEmptyMessage()
        {
            switch (activeCategory)
            {
                case NotificationCategory.Social: return AutoLocalizer.Get("notif_empty_social");
                case NotificationCategory.Games: return AutoLocalizer.Get("notif_empty_games");
                case NotificationCategory.Rewards: return AutoLocalizer.Get("notif_empty_rewards");
                default: return AutoLocalizer.Get("notif_empty_all");
            }
        }

        #endregion

        #region Tabs

        private void SwitchTab(NotificationCategory category)
        {
            activeCategory = category;
            UpdateTabVisuals();
            LoadNotifications();
        }

        private void UpdateTabVisuals()
        {
            // Reset all
            SetTabState(tabAllIndicator, false, CYAN_NEON);
            SetTabState(tabSocialIndicator, false, SOCIAL_COLOR);
            SetTabState(tabGamesIndicator, false, GAMES_COLOR);
            SetTabState(tabRewardsIndicator, false, REWARDS_COLOR);

            // Activate current
            switch (activeCategory)
            {
                case NotificationCategory.All:
                    SetTabState(tabAllIndicator, true, CYAN_NEON);
                    break;
                case NotificationCategory.Social:
                    SetTabState(tabSocialIndicator, true, SOCIAL_COLOR);
                    break;
                case NotificationCategory.Games:
                    SetTabState(tabGamesIndicator, true, GAMES_COLOR);
                    break;
                case NotificationCategory.Rewards:
                    SetTabState(tabRewardsIndicator, true, REWARDS_COLOR);
                    break;
            }
        }

        private void SetTabState(Image indicator, bool active, Color color)
        {
            if (indicator == null) return;
            indicator.DOColor(active ? color : Color.clear, 0.2f);

            // Scale animation for parent tab button
            if (indicator.transform.parent != null)
            {
                indicator.transform.parent.DOScale(active ? 1.05f : 1f, 0.2f).SetEase(Ease.OutCubic);
            }
        }

        #endregion

        #region UI Updates

        private void UpdateCountText()
        {
            if (countText == null) return;

            int unread = NotificationStorageService.Instance?.GetUnreadCount() ?? 0;
            int total = currentNotifications?.Count ?? 0;

            countText.text = unread > 0
                ? AutoLocalizer.Get("notif_unread_count", unread)
                : AutoLocalizer.Get("notif_total_count", total);
        }

        private void UpdateMarkAllReadButton()
        {
            if (markAllReadButton == null) return;

            int unread = NotificationStorageService.Instance?.GetUnreadCount() ?? 0;
            markAllReadButton.interactable = unread > 0;

            if (markAllReadText != null)
                markAllReadText.color = unread > 0 ? CYAN_NEON : TAB_INACTIVE;
        }

        #endregion

        #region Notification Actions

        private void OnNotificationTapped(string notificationId, StoredNotification notification)
        {
            NotificationStorageService.Instance?.MarkAsRead(notificationId);

            // Update card visual
            // La recarga se dispara por el evento OnNotificationsChanged
        }

        private async void OnAcceptFriendRequest(string notificationId, StoredNotification notification)
        {
            try
            {
                Debug.Log($"[Notifications] Aceptar solicitud de: {notification.senderName}");

                if (FriendService.Instance != null && !string.IsNullOrEmpty(notification.senderId))
                {
                    var result = await FriendService.Instance.AcceptFriendRequest(notification.senderId);
                    if (result.Success)
                    {
                        NotificationStorageService.Instance?.Delete(notificationId);
                        Debug.Log("[Notifications] Solicitud aceptada");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NotificationsManager] {ex.Message}");
            }
        }

        private async void OnRejectFriendRequest(string notificationId, StoredNotification notification)
        {
            try
            {
                Debug.Log($"[Notifications] Rechazar solicitud de: {notification.senderName}");

                if (FriendService.Instance != null && !string.IsNullOrEmpty(notification.senderId))
                {
                    var result = await FriendService.Instance.RejectFriendRequest(notification.senderId);
                    if (result.Success)
                    {
                        NotificationStorageService.Instance?.Delete(notificationId);
                        Debug.Log("[Notifications] Solicitud rechazada");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NotificationsManager] {ex.Message}");
            }
        }

        private void OnViewProfile(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return;
            PlayerPrefs.SetString("ViewProfileId", userId);
            PlayerPrefs.SetString("ProfileReturnScene", "Notifications");
            PlayerPrefs.Save();
            SceneManager.LoadScene("Profile");
        }

        private void OnAcceptChallenge(string notificationId, StoredNotification notification)
        {
            Debug.Log($"[Notifications] Aceptar reto: {notification.targetId}");
            NotificationStorageService.Instance?.MarkAsRead(notificationId);

            if (!string.IsNullOrEmpty(notification.targetId))
                PlayerPrefs.SetString("ChallengeId", notification.targetId);

            SceneManager.LoadScene("PlayModeSelection");
        }

        private void OnRejectChallenge(string notificationId, StoredNotification notification)
        {
            Debug.Log($"[Notifications] Rechazar reto: {notification.targetId}");
            NotificationStorageService.Instance?.Delete(notificationId);
        }

        private void OnJoinTournament(string tournamentId)
        {
            if (!string.IsNullOrEmpty(tournamentId))
                PlayerPrefs.SetString("OpenTournamentId", tournamentId);

            SceneManager.LoadScene("CashBattleHub");
        }

        private void OnViewTournamentResults(string tournamentId)
        {
            if (!string.IsNullOrEmpty(tournamentId))
                PlayerPrefs.SetString("OpenTournamentId", tournamentId);

            SceneManager.LoadScene("CashBattleHub");
        }

        private void OnClaimDailyReward()
        {
            PlayerPrefs.SetString("OpenPanel", "DailyRewards");
            SceneManager.LoadScene("MainMenu");
        }

        private void OnViewPromo(string promoAction)
        {
            switch (promoAction)
            {
                case "premium":
                    PlayerPrefs.SetString("OpenPanel", "Premium");
                    SceneManager.LoadScene("MainMenu");
                    break;
                case "deposit":
                    SceneManager.LoadScene("CashWallet");
                    break;
                default:
                    SceneManager.LoadScene("Shop");
                    break;
            }
        }

        #endregion

        #region Navigation

        private void OnBackClicked()
        {
            Debug.Log($"[Notifications] Volviendo a: {returnScene}");
            SceneManager.LoadScene(returnScene);
        }

        private void OnMarkAllReadClicked()
        {
            Debug.Log("[Notifications] Marcando todas como leídas");
            NotificationStorageService.Instance?.MarkAllAsRead();
        }

        #endregion

        #region Events

        private void OnNotificationsChanged()
        {
            // Recargar la lista actual
            LoadNotifications();
        }

        #endregion

        #region Animations

        private void AnimateEntrance()
        {
            // Header slide from top
            if (headerTransform != null)
            {
                Vector2 pos = headerTransform.anchoredPosition;
                headerTransform.anchoredPosition = new Vector2(pos.x, pos.y + 200);
                headerTransform.DOAnchorPos(pos, 0.4f).SetEase(Ease.OutBack);
            }

            // Tabs fade + slide
            if (tabsTransform != null)
            {
                var cg = tabsTransform.gameObject.GetComponent<CanvasGroup>() ?? tabsTransform.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                Vector2 pos = tabsTransform.anchoredPosition;
                tabsTransform.anchoredPosition = new Vector2(pos.x, pos.y - 50);
                DOTween.Sequence()
                    .AppendInterval(0.15f)
                    .Append(tabsTransform.DOAnchorPos(pos, 0.35f).SetEase(Ease.OutCubic))
                    .Join(cg.DOFade(1f, 0.35f))
                    .SetLink(gameObject);
            }

            // ScrollView fade in
            if (scrollViewTransform != null)
            {
                var cg = scrollViewTransform.gameObject.GetComponent<CanvasGroup>() ?? scrollViewTransform.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                DOTween.Sequence()
                    .AppendInterval(0.25f)
                    .Append(cg.DOFade(1f, 0.4f))
                    .SetLink(gameObject);
            }

            // Footer slide from bottom
            if (footerTransform != null)
            {
                var cg = footerTransform.gameObject.GetComponent<CanvasGroup>() ?? footerTransform.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                Vector2 pos = footerTransform.anchoredPosition;
                footerTransform.anchoredPosition = new Vector2(pos.x, pos.y - 100);
                DOTween.Sequence()
                    .AppendInterval(0.3f)
                    .Append(footerTransform.DOAnchorPos(pos, 0.35f).SetEase(Ease.OutCubic))
                    .Join(cg.DOFade(1f, 0.35f))
                    .SetLink(gameObject);
            }
        }

        #endregion

        #region Helpers

        private Sprite GetTypeIconSprite(string type)
        {
            string iconName = type switch
            {
                "friend_request" => "Icons/ProfileIcon",
                "friend_accepted" => "Icons/icon_handshake",
                "challenge" => "Icons/icon_trophy",
                "tournament_start" => "Icons/icon_trophy",
                "tournament_result" => "Icons/icon_medal",
                "daily_reward" => "Icons/icon_gift",
                "promo" => "Icons/stat_earnings",
                _ => "Icons/NotificationsIcon"
            };

            if (_iconCache.TryGetValue(iconName, out Sprite cached))
                return cached;

            Sprite sprite = Resources.Load<Sprite>(iconName);
            _iconCache[iconName] = sprite;
            return sprite;
        }

        private Color GetTypeColor(string type)
        {
            var category = NotificationStorageService.GetCategory(type);
            switch (category)
            {
                case NotificationCategory.Social: return SOCIAL_COLOR;
                case NotificationCategory.Games: return GAMES_COLOR;
                case NotificationCategory.Rewards: return REWARDS_COLOR;
                default: return CYAN_NEON;
            }
        }

        #endregion

        private void OnDestroy()
        {
            transform.DOKill();

            // Kill tweens on child objects
            foreach (var card in currentCards)
            {
                if (card != null) card.transform.DOKill();
            }
            if (headerTransform != null) headerTransform.DOKill();
            if (tabsTransform != null) tabsTransform.DOKill();
            if (scrollViewTransform != null) scrollViewTransform.DOKill();
            if (footerTransform != null) footerTransform.DOKill();
        }
    }
}
