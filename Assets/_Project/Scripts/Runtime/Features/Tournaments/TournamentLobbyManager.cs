using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using DigitPark.Monetization;
using DigitPark.Navigation;
using DigitPark.Data;
using DigitPark.Localization;
using DigitPark.UI;
using DigitPark.UI.Items;
using DigitPark.Themes;
using DigitPark.Games;
using DG.Tweening;
using DigitPark.Animations;
using DigitPark.Services.Firebase;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager para la escena del lobby de torneo.
    /// Muestra detalles del torneo, participantes, chat y permite unirse/salir.
    /// </summary>
    public class TournamentLobbyManager : MonoBehaviour
    {
        [Header("UI - Header")]
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI tournamentNameText;
        [SerializeField] private TextMeshProUGUI statusBadgeText;
        [SerializeField] private Image statusBadgeImage;

        [Header("UI - Tournament Info")]
        [SerializeField] private TextMeshProUGUI gameTypeText;
        [SerializeField] private TextMeshProUGUI entryFeeText;
        [SerializeField] private TextMeshProUGUI prizePoolText;
        [SerializeField] private Image playersProgressBar;
        [SerializeField] private TextMeshProUGUI playersProgressText;
        [SerializeField] private TextMeshProUGUI countdownText;

        [Header("UI - Rules")]
        [SerializeField] private TextMeshProUGUI attemptsRuleText;
        [SerializeField] private TextMeshProUGUI timeLimitRuleText;

        [Header("UI - Prize Distribution")]
        [SerializeField] private Transform prizeDistributionContainer;
        [SerializeField] private GameObject prizeRowPrefab;

        [Header("UI - Tabs")]
        [SerializeField] private Button participantsTabButton;
        [SerializeField] private Button chatTabButton;
        [SerializeField] private GameObject participantsContent;
        [SerializeField] private GameObject chatContent;
        [SerializeField] private Image participantsTabIndicator;
        [SerializeField] private Image chatTabIndicator;
        [SerializeField] private TextMeshProUGUI chatBadgeText;

        [Header("UI - Participants")]
        [SerializeField] private Transform participantsContainer;
        [SerializeField] private GameObject participantItemPrefab;
        [SerializeField] private TextMeshProUGUI participantsHeaderText;

        [Header("UI - Chat")]
        [SerializeField] private ScrollRect chatScrollRect;
        [SerializeField] private Transform chatMessagesContainer;
        [SerializeField] private TMP_InputField chatInput;
        [SerializeField] private Button sendChatButton;

        [Header("UI - Actions")]
        [SerializeField] private Button joinButton;
        [SerializeField] private Button leaveButton;
        [SerializeField] private Button shareButton;
        [SerializeField] private TextMeshProUGUI joinButtonText;

        [Header("UI - Status")]
        [SerializeField] private GameObject loadingOverlay;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private GameObject startingOverlay;
        [SerializeField] private TextMeshProUGUI startingCountdownText;

        [Header("Configuration")]
        [SerializeField] private float refreshInterval = 5f;
        // Avatar removed — participant items no longer display avatars

        // State
        private TournamentData currentTournament;
        private List<ParticipantData> participants = new List<ParticipantData>();
        private bool hasJoined = false;
        private bool _tournamentStarted = false;
        private bool isLoading = false;
        private int currentTab = 0; // 0 = Participants, 1 = Chat
        private int unreadChatCount = 0;
        private float _timeDisplayTimer = 0f;

        // Chat colors
        private static readonly Color CHAT_COLOR_ME = new Color(0f, 1f, 1f);
        private static readonly Color CHAT_COLOR_OTHER = new Color(0.8f, 0.8f, 0.8f);

        private void Start()
        {
            SetupUI();
            SetupListeners();
            LoadTournamentFromParams();

            // Auto refresh
            InvokeRepeating(nameof(RefreshTournament), refreshInterval, refreshInterval);
        }

        private void OnDisable()
        {
            CancelInvoke();
        }

        private void OnDestroy()
        {
            DOTween.Kill(transform);
            CancelInvoke();
        }

        private void SetupUI()
        {
            ShowLoadingOverlay(false);
            if (startingOverlay) startingOverlay.SetActive(false);
            if (leaveButton) leaveButton.gameObject.SetActive(false);

            // Default to participants tab
            SwitchToTab(0);
            UpdateActionButtons();
        }

        private void SetupListeners()
        {
            // Disable auto-navigation from BackButton prefab to prevent double listener
            var autoNav = backButton?.GetComponent<DigitPark.UI.BackButton>();
            if (autoNav != null) autoNav.DisableAutoNavigation();
            if (backButton) backButton.onClick.AddListener(OnBackClicked);
            if (joinButton) joinButton.onClick.AddListener(OnJoinClicked);
            if (leaveButton) leaveButton.onClick.AddListener(OnLeaveClicked);
            if (shareButton) shareButton.onClick.AddListener(OnShareClicked);
            if (sendChatButton) sendChatButton.onClick.AddListener(OnSendChat);
            if (participantsTabButton) participantsTabButton.onClick.AddListener(OnParticipantsTabClicked);
            if (chatTabButton) chatTabButton.onClick.AddListener(OnChatTabClicked);

            if (chatInput)
            {
                chatInput.onSubmit.AddListener(_ => OnSendChat());
            }
        }

        // ── Tab Navigation ──

        private void SwitchToTab(int tabIndex)
        {
            currentTab = tabIndex;

            if (participantsContent) participantsContent.SetActive(tabIndex == 0);
            if (chatContent) chatContent.SetActive(tabIndex == 1);

            var theme = ThemeManager.Instance?.CurrentTheme;
            Color activeColor = theme?.tabActive ?? new Color(0f, 1f, 1f);
            Color inactiveColor = theme?.tabInactive ?? new Color(0.5f, 0.5f, 0.5f, 0.3f);

            if (participantsTabIndicator) participantsTabIndicator.color = tabIndex == 0 ? activeColor : inactiveColor;
            if (chatTabIndicator) chatTabIndicator.color = tabIndex == 1 ? activeColor : inactiveColor;

            if (tabIndex == 1)
            {
                unreadChatCount = 0;
                UpdateChatBadge(0);
                ScrollChatToBottom();
            }
        }

        private void OnParticipantsTabClicked()
        {
            SwitchToTab(0);
        }

        private void OnChatTabClicked()
        {
            SwitchToTab(1);
        }

        // ── Chat ──

        private static string SanitizeChatMessage(string input)
        {
            // B6-C: Strip TMP rich text tags to prevent injection into TextMeshPro
            return System.Text.RegularExpressions.Regex.Replace(input, @"<[^>]*>", "");
        }

        private void OnSendChat()
        {
            if (chatInput == null || string.IsNullOrWhiteSpace(chatInput.text)) return;

            string message = chatInput.text.Trim();
            chatInput.text = "";

            // B6-C: Strip TMP tags before profanity filter
            message = SanitizeChatMessage(message);

            // Filter profanity
            if (DigitPark.Services.ChatFilterService.Instance != null)
            {
                var result = DigitPark.Services.ChatFilterService.Instance.Filter(message);
                message = result.FilteredMessage;
            }

            string displayName = AutoLocalizer.Get("chat_you");
            string senderId = GetCurrentUserId();
            CreateChatMessage(displayName, message, true);
            SaveChatMessage(senderId, message);
            ScrollChatToBottom();

            chatInput.ActivateInputField();
        }

        private void CreateChatMessage(string sender, string message, bool isMe)
        {
            if (chatMessagesContainer == null) return;

            GameObject msgObj = new GameObject($"Msg_{chatMessagesContainer.childCount}");
            msgObj.transform.SetParent(chatMessagesContainer, false);
            var rt = msgObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 50);

            var text = msgObj.AddComponent<TextMeshProUGUI>();
            text.text = $"<b>[{sender}]</b> {message}";
            text.fontSize = FontSizes.Body;
            text.enableAutoSizing = true;
            text.fontSizeMin = FontSizes.AutoMinBody;
            text.fontSizeMax = text.fontSize;
            text.color = isMe ? CHAT_COLOR_ME : CHAT_COLOR_OTHER;
            text.alignment = TextAlignmentOptions.Left;
            text.enableWordWrapping = true;

            var le = msgObj.AddComponent<LayoutElement>();
            le.minHeight = 40;

            // Badge for unread if on participants tab
            if (currentTab != 1 && !isMe)
            {
                unreadChatCount++;
                UpdateChatBadge(unreadChatCount);
            }
        }

        private void UpdateChatBadge(int count)
        {
            if (chatBadgeText == null) return;

            if (count <= 0)
            {
                chatBadgeText.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                chatBadgeText.transform.parent.gameObject.SetActive(true);
                chatBadgeText.text = count > 99 ? "99+" : count.ToString();
            }
        }

        private void ScrollChatToBottom()
        {
            if (chatScrollRect == null) return;
            Canvas.ForceUpdateCanvases();
            chatScrollRect.verticalNormalizedPosition = 0f;
        }

        private void LoadChatHistory()
        {
            if (currentTournament == null) return;

            string key = $"chat_{currentTournament.tournamentId}";
            string json = PlayerPrefs.GetString(key, "");
            if (string.IsNullOrEmpty(json)) return;

            // Simple format: "senderId|message\n"
            string currentUserId = GetCurrentUserId();
            string[] lines = json.Split('\n');
            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line)) continue;
                int sep = line.IndexOf('|');
                if (sep < 0) continue;
                string senderId = line.Substring(0, sep);
                string msg = line.Substring(sep + 1);
                bool isMe = senderId == currentUserId;
                string displayName = isMe ? AutoLocalizer.Get("chat_you") : senderId;
                CreateChatMessage(displayName, msg, isMe);
            }
        }

        private void SaveChatMessage(string sender, string message)
        {
            if (currentTournament == null) return;

            // Sanitize: replace pipe separator to avoid breaking the parse format
            string safeMessage = message.Replace("|", "\u2758");
            string safeSender = sender.Replace("|", "");
            string key = $"chat_{currentTournament.tournamentId}";
            string existing = PlayerPrefs.GetString(key, "");
            // Enforce size limit (~50KB) to avoid PlayerPrefs overflow
            if (existing.Length > 50000) existing = existing.Substring(existing.Length - 40000);
            existing += $"{safeSender}|{safeMessage}\n";
            PlayerPrefs.SetString(key, existing);
            PlayerPrefs.Save();
        }

        // ── Tournament Loading ──

        private void LoadTournamentFromParams()
        {
            var navParams = SceneNavigator.Instance?.PendingParams;

            if (navParams?.CustomData is TournamentData tournament)
            {
                currentTournament = tournament;
            }
            else if (!string.IsNullOrEmpty(navParams?.ItemId))
            {
                // Load from server by ID
                LoadTournamentById(navParams.ItemId);
                return;
            }
            else
            {
                // Create mock tournament for testing
                currentTournament = CreateMockTournament();
            }

            SceneNavigator.Instance?.ClearPendingParams();
            UpdateUI();
            LoadParticipants();
            LoadChatHistory();
        }

        private async void LoadTournamentById(string id)
        {
            isLoading = true;
            ShowLoadingOverlay(true);

            // B1-D: consultar Firebase real por ID
            try
            {
                currentTournament = await DatabaseService.Instance.GetTournament(id);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[TournamentLobby] Error cargando torneo {id}: {e.Message}");
            }

            if (currentTournament == null)
            {
                Debug.LogWarning($"[TournamentLobby] Torneo {id} no encontrado");
                PopupManager.Instance?.ShowErrorMessage(AutoLocalizer.Get("error_tournament_not_found"));
                isLoading = false;
                ShowLoadingOverlay(false);
                return;
            }

            isLoading = false;
            ShowLoadingOverlay(false);
            SceneNavigator.Instance?.ClearPendingParams();
            UpdateUI();
            LoadParticipants();
            LoadChatHistory();
        }

        private TournamentData CreateMockTournament()
        {
            return new TournamentData
            {
                name = AutoLocalizer.Get("mock_tournament_test"),
                category = "Memory Pairs",
                entryFee = 5,
                totalPrizePool = 150,
                currentParticipants = 12,
                maxParticipants = 32,
                status = TournamentStatus.Scheduled,
                startTime = DateTime.UtcNow.AddMinutes(30),
                endTime = DateTime.UtcNow.AddHours(2),
                rules = new TournamentRules { maxAttempts = 3 }
            };
        }

        // ── UI Updates ──

        private void UpdateUI()
        {
            if (currentTournament == null) return;

            // Header
            if (tournamentNameText) tournamentNameText.text = UICanvasHelper.TmpSafe(currentTournament.name);
            if (statusBadgeText) statusBadgeText.text = GetStatusText(currentTournament.status);

            // Update status badge color
            if (statusBadgeImage)
            {
                statusBadgeImage.color = GetStatusColor(currentTournament.status);
            }

            // Info
            if (gameTypeText) gameTypeText.text = currentTournament.category;
            if (entryFeeText) entryFeeText.text = currentTournament.entryFee == 0 ? L("tournament_free") : $"${currentTournament.entryFee}";
            if (prizePoolText) prizePoolText.text = $"${currentTournament.totalPrizePool}";

            // Players progress
            if (playersProgressText)
            {
                playersProgressText.text = L("tournament_players_progress",
                    currentTournament.currentParticipants, currentTournament.maxParticipants);
            }
            if (playersProgressBar)
            {
                float fill = currentTournament.maxParticipants > 0
                    ? (float)currentTournament.currentParticipants / currentTournament.maxParticipants
                    : 0f;
                playersProgressBar.fillAmount = fill;
            }

            // Time
            UpdateTimeDisplay();

            // Rules
            if (attemptsRuleText)
            {
                int attempts = currentTournament.rules?.maxAttempts ?? -1;
                attemptsRuleText.text = attempts < 0
                    ? L("tournament_rules_attempts", "∞")
                    : L("tournament_rules_attempts", attempts);
            }
            if (timeLimitRuleText)
            {
                timeLimitRuleText.text = L("tournament_rules_time_limit", 60);
            }

            // Prize distribution
            UpdatePrizeDistribution();

            // Action buttons
            UpdateActionButtons();
        }

        private string GetStatusText(TournamentStatus status)
        {
            switch (status)
            {
                case TournamentStatus.Scheduled:
                    return L("tournament_status_open");
                case TournamentStatus.Active:
                    return L("tournament_status_active");
                case TournamentStatus.Completed:
                    return L("tournament_status_completed");
                case TournamentStatus.Cancelled:
                    return L("tournament_status_cancelled");
                default:
                    return status.ToString();
            }
        }

        private Color GetStatusColor(TournamentStatus status)
        {
            switch (status)
            {
                case TournamentStatus.Scheduled:
                    return new Color(0f, 1f, 0.5f);
                case TournamentStatus.Active:
                    return new Color(1f, 0.84f, 0f);
                case TournamentStatus.Completed:
                    return new Color(0.5f, 0.5f, 0.5f);
                case TournamentStatus.Cancelled:
                    return new Color(1f, 0.3f, 0.3f);
                default:
                    return new Color(0f, 0.83f, 1f);
            }
        }

        private void UpdateTimeDisplay()
        {
            if (currentTournament == null) return;

            TimeSpan timeUntilStart = currentTournament.startTime - DateTime.UtcNow; // B6-K: use UTC

            if (countdownText)
            {
                if (timeUntilStart.TotalSeconds <= 0)
                {
                    countdownText.text = L("tournament_starting");
                    countdownText.color = new Color(1f, 0.84f, 0f);
                }
                else if (timeUntilStart.TotalMinutes < 60)
                {
                    countdownText.text = L("tournament_starts_in_short", timeUntilStart.Minutes, timeUntilStart.Seconds);
                    countdownText.color = new Color(0f, 1f, 0.5f);
                }
                else
                {
                    countdownText.text = L("tournament_starts_in_long", (int)timeUntilStart.TotalHours, timeUntilStart.Minutes);
                    countdownText.color = Color.white;
                }
            }
        }

        private void Update()
        {
            // Update countdown once per second instead of every frame
            _timeDisplayTimer += Time.deltaTime;
            if (_timeDisplayTimer >= 1f)
            {
                _timeDisplayTimer = 0f;
                UpdateTimeDisplay();
            }

            // Check if tournament is starting
            if (currentTournament != null && hasJoined)
            {
                TimeSpan timeUntilStart = currentTournament.startTime.ToUniversalTime() - DateTime.UtcNow;
                if (timeUntilStart.TotalSeconds <= 10 && timeUntilStart.TotalSeconds > 0)
                {
                    ShowStartingOverlay((int)timeUntilStart.TotalSeconds);
                }
                else if (timeUntilStart.TotalSeconds <= 0 && !_tournamentStarted)
                {
                    _tournamentStarted = true;
                    if (!StartTournament())
                        _tournamentStarted = false; // Reset to allow retry if dependencies not ready
                }
            }
        }

        private void UpdatePrizeDistribution()
        {
            if (prizeDistributionContainer == null) return;

            // Clear existing
            foreach (Transform child in prizeDistributionContainer)
            {
                Destroy(child.gameObject);
            }

            // Prize distribution (example: 50%, 30%, 20% for top 3)
            var distribution = new (string placeKey, float percent)[]
            {
                ("tournament_1st_place", 50f),
                ("tournament_2nd_place", 30f),
                ("tournament_3rd_place", 20f)
            };

            foreach (var (placeKey, percent) in distribution)
            {
                int prize = Mathf.RoundToInt(currentTournament.totalPrizePool * (percent / 100f));
                CreatePrizeRow(L(placeKey), prize, percent);
            }

            // Staggered entrance animation for prize rows
            AnimatePrizeRowsEntrance();
        }

        private void AnimatePrizeRowsEntrance()
        {
            if (prizeDistributionContainer == null || prizeDistributionContainer.childCount == 0) return;

            var seq = DOTween.Sequence().SetLink(gameObject);
            for (int i = 0; i < prizeDistributionContainer.childCount; i++)
            {
                var child = prizeDistributionContainer.GetChild(i);
                var cg = child.GetComponent<CanvasGroup>();
                if (cg == null) cg = child.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                child.localScale = Vector3.one * 0.85f;
                float delay = i * 0.08f;
                seq.Insert(delay, cg.DOFade(1f, 0.3f).SetEase(Ease.OutQuad));
                seq.Insert(delay, child.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
            }
        }

        private void CreatePrizeRow(string place, int prize, float percent)
        {
            GameObject row;
            if (prizeRowPrefab != null)
            {
                row = Instantiate(prizeRowPrefab, prizeDistributionContainer);
            }
            else
            {
                row = new GameObject(place);
                row.transform.SetParent(prizeDistributionContainer, false);
                var rt = row.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(300, 30);

                var text = row.AddComponent<TextMeshProUGUI>();
                text.text = $"{place}: ${prize} ({percent}%)";
                text.fontSize = FontSizes.Body;
                text.enableAutoSizing = true;
                text.fontSizeMin = FontSizes.AutoMinBody;
                text.fontSizeMax = text.fontSize;
            }
        }

        // ── Participants ──

        private void LoadParticipants()
        {
            // Simulate loading participants
            participants.Clear();

            int count = currentTournament?.currentParticipants ?? 10;
            for (int i = 0; i < Mathf.Min(count, 10); i++)
            {
                participants.Add(new ParticipantData
                {
                    id = Guid.NewGuid().ToString(),
                    username = $"Player{i + 1}",
                    rank = i + 1,
                    bestTime = UnityEngine.Random.Range(8f, 45f)
                });
            }

            UpdateParticipantsList();
        }

        private void UpdateParticipantsList()
        {
            if (participantsContainer == null) return;

            // Clear existing
            foreach (Transform child in participantsContainer)
            {
                Destroy(child.gameObject);
            }

            if (participantsHeaderText)
            {
                participantsHeaderText.text = L("tournament_participants_count", participants.Count);
            }

            foreach (var participant in participants)
            {
                CreateParticipantItem(participant);
            }

            // Animate participants list entrance
            AnimateParticipantsListEntrance();
        }

        private void AnimateParticipantsListEntrance()
        {
            if (participantsContainer == null || participantsContainer.childCount == 0) return;

            var seq = DOTween.Sequence().SetLink(gameObject);
            for (int i = 0; i < participantsContainer.childCount; i++)
            {
                var child = participantsContainer.GetChild(i);
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

        private void CreateParticipantItem(ParticipantData participant)
        {
            GameObject item;
            if (participantItemPrefab != null)
            {
                item = Instantiate(participantItemPrefab, participantsContainer);

                var itemUI = item.GetComponent<ParticipantItemUI>();
                if (itemUI != null)
                {
                    var displayData = new ParticipantDisplayData
                    {
                        userId = participant.id,
                        username = participant.username,
                        rank = participant.rank,
                        bestTime = participant.bestTime,
                        showRank = true,
                        showAttempts = false,
                        showBestTime = true,
                        isOnline = true,
                        isSelf = false
                    };
                    itemUI.Setup(displayData);
                }
            }
            else
            {
                // Fallback: create simple text item when prefab is not assigned
                item = new GameObject(participant.username);
                item.transform.SetParent(participantsContainer, false);
                var rt = item.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(0, 70);

                var le = item.AddComponent<LayoutElement>();
                le.minHeight = 70;

                var bg = item.AddComponent<Image>();
                bg.color = participant.rank <= 3
                    ? new Color(0.06f, 0.1f, 0.15f, 1f)
                    : new Color(0.04f, 0.06f, 0.1f, 1f);

                var hlg = item.AddComponent<HorizontalLayoutGroup>();
                hlg.spacing = 8;
                hlg.padding = new RectOffset(16, 16, 6, 6);
                hlg.childAlignment = TextAnchor.MiddleCenter;
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth = false;

                // Rank
                var rankObj = new GameObject("Rank");
                rankObj.transform.SetParent(item.transform, false);
                var rankText = rankObj.AddComponent<TextMeshProUGUI>();
                rankText.text = $"#{participant.rank}";
                rankText.fontSize = FontSizes.BodyLarge;
                rankText.enableAutoSizing = true;
                rankText.fontSizeMin = FontSizes.AutoMinBody;
                rankText.fontSizeMax = rankText.fontSize;
                rankText.fontStyle = FontStyles.Bold;
                rankText.color = new Color(0f, 1f, 1f);
                rankText.alignment = TextAlignmentOptions.Center;
                var rankLE = rankObj.AddComponent<LayoutElement>();
                rankLE.minWidth = 50;
                rankLE.preferredWidth = 50;

                // Name
                var nameObj = new GameObject("PlayerName");
                nameObj.transform.SetParent(item.transform, false);
                var nameText = nameObj.AddComponent<TextMeshProUGUI>();
                nameText.text = UICanvasHelper.TmpSafe(participant.username);
                nameText.fontSize = FontSizes.Body;
                nameText.enableAutoSizing = true;
                nameText.fontSizeMin = FontSizes.AutoMinBody;
                nameText.fontSizeMax = nameText.fontSize;
                nameText.fontStyle = FontStyles.Bold;
                nameText.color = Color.white;
                nameText.alignment = TextAlignmentOptions.Left;
                var nameLE = nameObj.AddComponent<LayoutElement>();
                nameLE.flexibleWidth = 1;

                // Time
                var timeObj = new GameObject("BestTime");
                timeObj.transform.SetParent(item.transform, false);
                var timeText = timeObj.AddComponent<TextMeshProUGUI>();
                timeText.text = participant.bestTime > 0 ? FormatTime(participant.bestTime) : "-";
                timeText.fontSize = FontSizes.BodyLarge;
                timeText.enableAutoSizing = true;
                timeText.fontSizeMin = FontSizes.AutoMinBody;
                timeText.fontSizeMax = timeText.fontSize;
                timeText.fontStyle = FontStyles.Bold;
                timeText.color = new Color(0f, 1f, 1f);
                timeText.alignment = TextAlignmentOptions.Right;
                var timeLE = timeObj.AddComponent<LayoutElement>();
                timeLE.minWidth = 120;
                timeLE.preferredWidth = 120;
            }
        }

        private string FormatTime(float seconds)
        {
            if (seconds >= 60)
            {
                int mins = (int)(seconds / 60);
                float secs = seconds % 60;
                return $"{mins}:{secs:00.00}";
            }
            return $"{seconds:0.00}s";
        }

        // ── Actions ──

        private void UpdateActionButtons()
        {
            bool canJoin = currentTournament?.status == TournamentStatus.Scheduled &&
                          currentTournament?.currentParticipants < currentTournament?.maxParticipants;

            if (joinButton)
            {
                joinButton.gameObject.SetActive(!hasJoined && canJoin);
                if (joinButtonText)
                {
                    joinButtonText.text = currentTournament?.entryFee == 0
                        ? L("tournament_join_free")
                        : L("tournament_join_fee", currentTournament?.entryFee);
                }
            }

            if (leaveButton) leaveButton.gameObject.SetActive(hasJoined);
        }

        private void RefreshTournament()
        {
            if (currentTournament == null || isLoading) return;

            // Simulate refresh - update player count, etc.
            currentTournament.currentParticipants = Mathf.Min(
                currentTournament.currentParticipants + UnityEngine.Random.Range(0, 2),
                currentTournament.maxParticipants
            );

            UpdateUI();
        }

        private async void OnJoinClicked() // B1-E: join real en Firebase
        {
            if (isLoading || hasJoined) return;

            isLoading = true;
            ShowLoadingOverlay(true);
            ShowStatus(L("tournament_joining"));

            bool success = false;
            try
            {
                success = await DatabaseService.Instance.JoinTournament(
                    currentTournament.tournamentId, GetCurrentUserId());
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[TournamentLobby] Error al unirse: {e.Message}");
            }

            isLoading = false;
            ShowLoadingOverlay(false);

            if (success)
            {
                hasJoined = true;
                currentTournament.currentParticipants++;
                ShowStatus(L("tournament_joined"));
                UpdateUI();
                UpdateActionButtons();
            }
            else
            {
                ShowStatus(L("tournament_join_failed"));
            }
        }

        private async void OnLeaveClicked() // B1-E: leave real en Firebase
        {
            if (isLoading || !hasJoined) return;

            isLoading = true;
            ShowLoadingOverlay(true);

            bool success = false;
            try
            {
                success = await DatabaseService.Instance.LeaveTournament(
                    currentTournament.tournamentId, GetCurrentUserId());
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[TournamentLobby] Error al salir: {e.Message}");
            }

            isLoading = false;
            ShowLoadingOverlay(false);

            if (success)
            {
                hasJoined = false;
                currentTournament.currentParticipants--;

                // B6-G: Refund entry fee on leave
                if (currentTournament.entryFee > 0)
                {
                    var currency = DigitPark.Monetization.CurrencyManager.Instance;
                    currency?.AddCoins(currentTournament.entryFee);
                    Debug.Log($"[TournamentLobby] Refunded {currentTournament.entryFee} coins on leave");
                }

                UpdateUI();
                UpdateActionButtons();
                ShowStatus(L("tournament_left"));
            }
            else
            {
                ShowStatus(L("tournament_leave_failed"));
            }
        }

        private void OnShareClicked()
        {
            string shareText = L("tournament_share_text", currentTournament?.name);
            GUIUtility.systemCopyBuffer = shareText;
            ShowStatus(L("tournament_link_copied"));
        }

        // ── Tournament Start ──

        private void ShowStartingOverlay(int seconds)
        {
            if (startingOverlay) startingOverlay.SetActive(true);
            if (startingCountdownText)
                startingCountdownText.text = L("tournament_starting_in", seconds);
        }

        private bool StartTournament()
        {
            if (currentTournament == null) return false;

            if (GameSessionManager.Instance == null)
            {
                Debug.LogWarning("[TournamentLobby] GameSessionManager not ready — will retry");
                return false;
            }

            // Parse game type from category string using enum parsing with fallback
            GameType gameType = GameType.MemoryPairs;
            // Try direct enum parse first (handles "DigitRush", "MemoryPairs", etc.)
            string normalized = currentTournament.category?.Replace(" ", "") ?? "";
            if (!System.Enum.TryParse<GameType>(normalized, true, out gameType))
            {
                gameType = GameType.MemoryPairs; // Default fallback
            }

            GameSessionManager.Instance.StartTournamentSession(
                gameType,
                currentTournament.tournamentId,
                Guid.NewGuid().ToString(),
                "", "" // no opponent in leaderboard tournaments
            );
            return true;
        }

        // ── Status & Overlays ──

        private void ShowStatus(string message)
        {
            if (statusText)
            {
                statusText.text = message;
                CancelInvoke(nameof(ClearStatus));
                Invoke(nameof(ClearStatus), 3f);
            }
        }

        private void ClearStatus()
        {
            if (statusText) statusText.text = "";
        }

        private void OnBackClicked()
        {
            SceneNavigator.Instance?.GoBack();
        }

        private void ShowLoadingOverlay(bool show)
        {
            if (loadingOverlay == null) return;

            if (show)
            {
                loadingOverlay.SetActive(true);
                var cg = loadingOverlay.GetComponent<CanvasGroup>();
                if (cg == null) cg = loadingOverlay.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                cg.DOFade(1f, 0.2f).SetUpdate(true).SetLink(loadingOverlay);
            }
            else
            {
                var cg = loadingOverlay.GetComponent<CanvasGroup>();
                if (cg != null)
                    cg.DOFade(0f, 0.2f).SetUpdate(true).SetLink(loadingOverlay).OnComplete(() => loadingOverlay.SetActive(false));
                else
                    loadingOverlay.SetActive(false);
            }
        }

        private string GetCurrentUserId()
        {
            var authService = DigitPark.Services.Firebase.AuthenticationService.Instance;
            return authService != null ? authService.GetCurrentUserId() ?? "local_user" : "local_user";
        }

        private string L(string key, params object[] args)
        {
            return args.Length > 0 ? AutoLocalizer.Get(key, args) : AutoLocalizer.Get(key);
        }
    }

    [Serializable]
    public class ParticipantData
    {
        public string id;
        public string username;
        public int rank;
        public float bestTime;
    }
}
