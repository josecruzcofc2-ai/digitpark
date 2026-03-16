using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DigitPark.Games;
using DigitPark.Localization;
using DigitPark.Services;
using DigitPark.UI;
using DigitPark.UI.Items;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager for CashTournamentLobby scene.
    /// Handles tournament info display, participant list, chat, joining/leaving,
    /// and the pre-start countdown.
    /// </summary>
    public class CashTournamentLobbyManager : MonoBehaviour
    {
        [Header("=== HEADER ===")]
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI tournamentNameText;
        [SerializeField] private TextMeshProUGUI statusBadgeText;
        [SerializeField] private Image statusBadgeImage;

        [Header("=== TOURNAMENT INFO ===")]
        [SerializeField] private TextMeshProUGUI gameTypeText;
        [SerializeField] private TextMeshProUGUI entryFeeText;
        [SerializeField] private TextMeshProUGUI prizePoolText;
        [SerializeField] private Image playersProgressBar;
        [SerializeField] private TextMeshProUGUI playersProgressText;
        [SerializeField] private TextMeshProUGUI countdownText;

        [Header("=== RULES ===")]
        [SerializeField] private TextMeshProUGUI attemptsRuleText;
        [SerializeField] private TextMeshProUGUI timeLimitRuleText;

        [Header("=== PRIZE DISTRIBUTION ===")]
        [SerializeField] private Transform prizeDistributionContainer;

        [Header("=== TABS ===")]
        [SerializeField] private Button participantsTabButton;
        [SerializeField] private Button chatTabButton;
        [SerializeField] private GameObject participantsContent;
        [SerializeField] private GameObject chatContent;
        [SerializeField] private Image participantsTabIndicator;
        [SerializeField] private Image chatTabIndicator;
        [SerializeField] private TextMeshProUGUI chatBadgeText;

        [Header("=== PARTICIPANTS ===")]
        [SerializeField] private Transform participantsContainer;

        [Header("=== CHAT ===")]
        [SerializeField] private ScrollRect chatScrollRect;
        [SerializeField] private Transform chatMessagesContainer;
        [SerializeField] private TMP_InputField chatInput;
        [SerializeField] private Button sendChatButton;

        [Header("=== ACTIONS ===")]
        [SerializeField] private Button shareButton;
        [SerializeField] private Button playButton;
        [SerializeField] private TextMeshProUGUI playButtonText;

        [Header("=== STATUS ===")]
        [SerializeField] private GameObject loadingOverlay;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private GameObject startingOverlay;
        [SerializeField] private TextMeshProUGUI startingCountdownText;

        [Header("=== PREFABS ===")]
        [SerializeField] private GameObject participantItemPrefab;

        [Header("=== SETTINGS ===")]
        [SerializeField] private float refreshInterval = 10f;
        [SerializeField] private Sprite defaultAvatarSprite;

        // Colors
        private static readonly Color GOLD_PRIMARY = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GOLD_DARK = new Color(0.85f, 0.65f, 0.13f, 1f);
        private static readonly Color AMBER = new Color(1f, 0.75f, 0f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.7f, 0.7f, 0.7f, 1f);
        private static readonly Color TEXT_GOLD = new Color(1f, 0.84f, 0f, 1f);

        // State
        private string currentTab = "participants";
        private bool hasJoined = true; // Player already joined from CashTournaments browser
        private int attemptsUsed = 0;
        private bool isPlaying = false;
        private Coroutine refreshCoroutine;
        private Coroutine countdownCoroutine;

        // Mock tournament data
        private string tournamentId;
        private string tournamentName;
        private string gameType;
        private decimal entryFee;
        private decimal prizePool;
        private int currentPlayers;
        private int maxPlayers;
        private int maxAttempts;
        private float timeLimitSeconds;
        private DateTime startTime;

        private void Start()
        {
            SetupListeners();
            LoadTournamentData();
            LoadAttemptsState();
            UpdatePlayButton();
            SwitchToTab("participants");
            refreshCoroutine = StartCoroutine(AutoRefreshCoroutine());
        }

        private void SetupListeners()
        {
            // Disable auto-navigation from BackButtonGold prefab to prevent double listener
            if (backButton != null)
            {
                var autoNav = backButton.GetComponent<DigitPark.UI.BackButtonGold>();
                if (autoNav != null) autoNav.DisableAutoNavigation();
                backButton.onClick.AddListener(OnBackClicked);
            }

            if (shareButton != null)
                shareButton.onClick.AddListener(OnShareClicked);

            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClicked);

            if (participantsTabButton != null)
                participantsTabButton.onClick.AddListener(() => SwitchToTab("participants"));

            if (chatTabButton != null)
                chatTabButton.onClick.AddListener(() => SwitchToTab("chat"));

            if (sendChatButton != null)
                sendChatButton.onClick.AddListener(OnSendChat);

            if (chatInput != null)
                chatInput.onSubmit.AddListener(_ => OnSendChat());
        }

        private void LoadTournamentData()
        {
            // Read from PlayerPrefs / NavigationParams (mock data for now)
            tournamentId = PlayerPrefs.GetString("CashTournament_Id", "tournament_001");
            tournamentName = PlayerPrefs.GetString("CashTournament_Name", "QuickMath Championship");
            gameType = PlayerPrefs.GetString("CashTournament_GameType", "QuickMath");
            entryFee = (decimal)PlayerPrefs.GetFloat("CashTournament_EntryFee", 5.00f);
            // Prize pool = total entries × (1 - platform fee 30%)
            // Example: $5 × 16 players × 0.70 = $56.00
            prizePool = (decimal)PlayerPrefs.GetFloat("CashTournament_PrizePool", 56.00f);
            currentPlayers = PlayerPrefs.GetInt("CashTournament_CurrentPlayers", 8);
            maxPlayers = PlayerPrefs.GetInt("CashTournament_MaxPlayers", 16);
            maxAttempts = PlayerPrefs.GetInt("CashTournament_MaxAttempts", 3);
            timeLimitSeconds = PlayerPrefs.GetFloat("CashTournament_TimeLimit", 120f);

            // Start time: mock 45 minutes from now
            startTime = DateTime.Now.AddMinutes(45);

            PopulateTournamentInfo();
            PopulatePrizeDistribution();
            PopulateMockParticipants();
        }

        private void PopulateTournamentInfo()
        {
            if (tournamentNameText != null)
                tournamentNameText.text = tournamentName;

            if (statusBadgeText != null)
                statusBadgeText.text = AutoLocalizer.Get("tournament_open");

            if (gameTypeText != null)
                gameTypeText.text = gameType;

            if (entryFeeText != null)
                entryFeeText.text = AutoLocalizer.Get("cash_entry_fee", entryFee);

            if (prizePoolText != null)
                prizePoolText.text = $"${prizePool:F2}";

            UpdatePlayersProgress(currentPlayers, maxPlayers);

            if (attemptsRuleText != null)
                attemptsRuleText.text = AutoLocalizer.Get("tournament_rules_attempts", maxAttempts);

            if (timeLimitRuleText != null)
            {
                int minutes = Mathf.FloorToInt(timeLimitSeconds / 60f);
                int seconds = Mathf.FloorToInt(timeLimitSeconds % 60f);
                timeLimitRuleText.text = AutoLocalizer.Get("tournament_time_limit", seconds > 0 ? $"{minutes}:{seconds:D2}" : $"{minutes}:00");
            }

            UpdateCountdownDisplay();
        }

        private void PopulatePrizeDistribution()
        {
            // Prize pool is already post-platform-fee (70% of total entries)
            // Distribution: 50% / 30% / 20% of prize pool
            decimal first = prizePool * 0.50m;
            decimal second = prizePool * 0.30m;
            decimal third = prizePool * 0.20m;

            if (prizeDistributionContainer != null)
            {
                // Update existing prize row texts (supports both prefab and inline layout)
                for (int i = 0; i < prizeDistributionContainer.childCount; i++)
                {
                    Transform child = prizeDistributionContainer.GetChild(i);

                    // Try PrizeRowItem prefab structure first, then inline fallback
                    Transform amountT = child.Find("PrizeAmountText") ?? child.Find("Amount");
                    if (amountT != null)
                    {
                        TextMeshProUGUI amountTMP = amountT.GetComponent<TextMeshProUGUI>();
                        if (amountTMP != null)
                        {
                            switch (i)
                            {
                                case 0: amountTMP.text = $"${first:F2}"; break;
                                case 1: amountTMP.text = $"${second:F2}"; break;
                                case 2: amountTMP.text = $"${third:F2}"; break;
                            }
                        }
                    }
                }
            }
        }

        private void PopulateMockParticipants()
        {
            if (participantsContainer == null) return;

            // Clear existing
            foreach (Transform child in participantsContainer)
                Destroy(child.gameObject);

            var mockParticipants = new[]
            {
                new ParticipantDisplayData
                {
                    userId = "self_001",
                    username = AutoLocalizer.Get("chat_you"),
                    rank = 1,
                    attemptsUsed = 1,
                    maxAttempts = 3,
                    bestTime = 12.34f,
                    isOnline = true,
                    isReady = true,
                    isSelf = true,
                    showRank = true,
                    showAttempts = true,
                    showBestTime = true
                },
                new ParticipantDisplayData
                {
                    userId = "player_002",
                    username = "QuickFingers99",
                    rank = 2,
                    attemptsUsed = 0,
                    maxAttempts = 3,
                    bestTime = 14.87f,
                    isOnline = true,
                    isReady = false,
                    isSelf = false,
                    showRank = true,
                    showAttempts = true,
                    showBestTime = true
                }
            };

            foreach (var data in mockParticipants)
            {
                CreateParticipantItem(data);
            }
        }

        private void CreateParticipantItem(ParticipantDisplayData data)
        {
            if (participantsContainer == null) return;

            GameObject item;
            if (participantItemPrefab != null)
            {
                item = Instantiate(participantItemPrefab, participantsContainer);
            }
            else
            {
                // Fallback: create basic item
                item = new GameObject($"Participant_{data.username}");
                item.transform.SetParent(participantsContainer, false);

                RectTransform rt = item.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(0, 70);

                LayoutElement le = item.AddComponent<LayoutElement>();
                le.preferredHeight = 70;

                Image bg = item.AddComponent<Image>();
                bg.color = new Color(0.06f, 0.08f, 0.12f);

                // Simple text fallback
                GameObject textObj = new GameObject("UsernameText");
                textObj.transform.SetParent(item.transform, false);
                RectTransform trt = textObj.AddComponent<RectTransform>();
                trt.anchorMin = Vector2.zero;
                trt.anchorMax = Vector2.one;
                trt.sizeDelta = Vector2.zero;
                trt.offsetMin = new Vector2(80, 0);
                TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
                tmp.text = $"#{data.rank} {data.username} — {data.attemptsUsed}/{data.maxAttempts}";
                tmp.fontSize = 30f;
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = FontSizes.AutoMinSmall;
                tmp.fontSizeMax = tmp.fontSize;
                tmp.color = Color.white;
                tmp.alignment = TextAlignmentOptions.MidlineLeft;
                tmp.fontStyle = FontStyles.Bold;
                return;
            }

            // Setup via ParticipantItemUI component
            var ui = item.GetComponent<ParticipantItemUI>();
            if (ui != null)
            {
                if (data.avatar == null && defaultAvatarSprite != null)
                    data.avatar = defaultAvatarSprite;

                ui.Setup(data,
                    profileId => Debug.Log($"[CashTournamentLobby] View profile: {profileId}"),
                    challengeId => Debug.Log($"[CashTournamentLobby] Challenge: {challengeId}")
                );
            }
        }

        public void SwitchToTab(string tab)
        {
            currentTab = tab;

            bool isParticipants = tab == "participants";

            if (participantsContent != null)
                participantsContent.SetActive(isParticipants);

            if (chatContent != null)
                chatContent.SetActive(!isParticipants);

            // Update tab indicator colors
            if (participantsTabIndicator != null)
                participantsTabIndicator.color = isParticipants ? GOLD_PRIMARY : Color.clear;

            if (chatTabIndicator != null)
                chatTabIndicator.color = !isParticipants ? GOLD_PRIMARY : Color.clear;

            // Update tab button text colors
            UpdateTabButtonTextColor(participantsTabButton, isParticipants);
            UpdateTabButtonTextColor(chatTabButton, !isParticipants);
        }

        private void UpdateTabButtonTextColor(Button tabButton, bool isActive)
        {
            if (tabButton == null) return;
            TextMeshProUGUI labelText = tabButton.GetComponentInChildren<TextMeshProUGUI>();
            if (labelText != null)
                labelText.color = isActive ? TEXT_GOLD : TEXT_SECONDARY;
        }

        private void OnShareClicked()
        {
            string shareLink = $"digitpark://tournament/{tournamentId}";
            GUIUtility.systemCopyBuffer = shareLink;
            Debug.Log($"[CashTournamentLobby] Tournament link copied: {shareLink}");
        }

        private void OnPlayClicked()
        {
            if (isPlaying) return;

            if (attemptsUsed >= maxAttempts)
            {
                Debug.Log("[CashTournamentLobby] No attempts remaining.");
                return;
            }

            isPlaying = true;
            attemptsUsed++;
            SaveAttemptsState();
            UpdatePlayButton();

            // Configure GameContext for CashTournament mode
            if (GameSessionManager.Instance != null)
            {
                var context = new GameContext
                {
                    Mode = GameMode.CashTournament,
                    TournamentId = tournamentId,
                    EntryFee = entryFee
                };

                // Parse game type to enum
                if (System.Enum.TryParse<GameType>(gameType, out var parsedType))
                    context.Games.Add(parsedType);

                GameSessionManager.Instance.SetContext(context);
            }

            // Store extra data for the game scene
            PlayerPrefs.SetString("CashGame_TournamentId", tournamentId);
            PlayerPrefs.SetInt("CashGame_AttemptNumber", attemptsUsed);
            PlayerPrefs.SetInt("CashGame_MaxAttempts", maxAttempts);
            PlayerPrefs.Save();

            Debug.Log($"[CashTournamentLobby] Starting attempt {attemptsUsed}/{maxAttempts} — Loading {gameType}");

            // Load the minigame scene (same scenes, CashTournament mode distinguishes behavior)
            SceneManager.LoadScene(gameType);
        }

        private void LoadAttemptsState()
        {
            // Restore attempts used from PlayerPrefs (persists across scene loads)
            attemptsUsed = PlayerPrefs.GetInt($"CashTournament_{tournamentId}_AttemptsUsed", 0);
        }

        private void SaveAttemptsState()
        {
            PlayerPrefs.SetInt($"CashTournament_{tournamentId}_AttemptsUsed", attemptsUsed);
            PlayerPrefs.Save();
        }

        private void UpdatePlayButton()
        {
            int remaining = Mathf.Max(0, maxAttempts - attemptsUsed);

            if (playButton != null)
                playButton.interactable = remaining > 0;

            if (playButtonText != null)
                playButtonText.text = remaining > 0 ? AutoLocalizer.Get("play_button") : AutoLocalizer.Get("tournament_no_attempts");
        }

        private void OnSendChat()
        {
            if (chatInput == null || string.IsNullOrWhiteSpace(chatInput.text)) return;

            string message = chatInput.text.Trim();
            chatInput.text = "";

            // Filter profanity
            if (ChatFilterService.Instance != null)
            {
                var result = ChatFilterService.Instance.Filter(message);
                message = result.FilteredMessage;
            }

            // Add chat message locally (mock)
            AddChatMessage(AutoLocalizer.Get("chat_you"), message);

            // Scroll to bottom
            if (chatScrollRect != null)
                StartCoroutine(ScrollToBottomNextFrame());
        }

        private void AddChatMessage(string sender, string message)
        {
            if (chatMessagesContainer == null) return;

            GameObject msgObj = new GameObject("ChatMsg");
            msgObj.transform.SetParent(chatMessagesContainer, false);

            RectTransform rt = msgObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 40);

            UnityEngine.UI.LayoutElement le = msgObj.AddComponent<UnityEngine.UI.LayoutElement>();
            le.preferredHeight = 40;

            TextMeshProUGUI msgText = msgObj.AddComponent<TextMeshProUGUI>();
            msgText.text = $"<color=#FFD700>{sender}:</color> {message}";
            msgText.fontSize = 32f;
            msgText.enableAutoSizing = true;
            msgText.fontSizeMin = FontSizes.AutoMinBody;
            msgText.fontSizeMax = msgText.fontSize;
            msgText.color = Color.white;
            msgText.alignment = TextAlignmentOptions.Left;
        }

        private IEnumerator ScrollToBottomNextFrame()
        {
            yield return null;
            if (chatScrollRect != null)
                chatScrollRect.verticalNormalizedPosition = 0f;
        }

        public void StartCountdown()
        {
            if (countdownCoroutine != null)
                StopCoroutine(countdownCoroutine);

            countdownCoroutine = StartCoroutine(CountdownCoroutine());
        }

        private IEnumerator CountdownCoroutine()
        {
            if (startingOverlay != null)
                startingOverlay.SetActive(true);

            for (int i = 3; i > 0; i--)
            {
                if (startingCountdownText != null)
                    startingCountdownText.text = i.ToString();

                yield return new WaitForSeconds(1f);
            }

            if (startingCountdownText != null)
                startingCountdownText.text = AutoLocalizer.Get("matchmaking_go");

            yield return new WaitForSeconds(0.5f);

            // Navigate to the actual game
            Debug.Log($"[CashTournamentLobby] Tournament started! Navigating to game: {gameType}");
            // SceneManager.LoadScene(gameType);
        }

        public void UpdatePlayersProgress(int current, int max)
        {
            currentPlayers = current;
            maxPlayers = max;

            if (playersProgressText != null)
                playersProgressText.text = AutoLocalizer.Get("tournament_players_count", $"{current}/{max}");

            if (playersProgressBar != null)
            {
                RectTransform fillRT = playersProgressBar.GetComponent<RectTransform>();
                if (fillRT != null)
                {
                    float ratio = max > 0 ? (float)current / max : 0f;
                    fillRT.anchorMax = new Vector2(ratio, 1f);
                }
            }
        }

        private void UpdateCountdownDisplay()
        {
            if (countdownText == null) return;

            TimeSpan remaining = startTime - DateTime.Now;
            if (remaining.TotalSeconds <= 0)
            {
                countdownText.text = AutoLocalizer.Get("tournament_starting_soon");
                countdownText.color = AMBER;
                return;
            }

            int totalMinutes = (int)remaining.TotalMinutes;
            int seconds = remaining.Seconds;

            countdownText.text = AutoLocalizer.Get("tournament_starts_in", $"{totalMinutes}:{seconds:D2}");

            // Use AMBER when < 5 minutes, TEXT_SECONDARY otherwise
            countdownText.color = remaining.TotalMinutes < 5 ? AMBER : TEXT_SECONDARY;
        }

        private IEnumerator AutoRefreshCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(refreshInterval);
                UpdateCountdownDisplay();

                // Simulate player count changes
                int change = UnityEngine.Random.Range(-1, 3);
                int newCount = Mathf.Clamp(currentPlayers + change, 1, maxPlayers);
                if (!hasJoined || newCount != currentPlayers)
                {
                    UpdatePlayersProgress(newCount, maxPlayers);
                }
            }
        }

        private void OnBackClicked()
        {
            // Player cannot leave once joined - back just returns to tournament browser
            // The tournament entry stays active
            SceneManager.LoadScene("CashTournaments");
        }

        private void OnDestroy()
        {
            if (refreshCoroutine != null)
                StopCoroutine(refreshCoroutine);

            if (countdownCoroutine != null)
                StopCoroutine(countdownCoroutine);
        }
    }
}
