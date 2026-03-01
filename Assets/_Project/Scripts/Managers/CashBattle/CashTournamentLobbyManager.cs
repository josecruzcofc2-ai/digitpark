using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DigitPark.Games;
using DigitPark.Localization;

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
        [SerializeField] private Button joinButton;
        [SerializeField] private Button leaveButton;
        [SerializeField] private Button shareButton;
        [SerializeField] private TextMeshProUGUI joinButtonText;

        [Header("=== STATUS ===")]
        [SerializeField] private GameObject loadingOverlay;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private GameObject startingOverlay;
        [SerializeField] private TextMeshProUGUI startingCountdownText;

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
        private bool hasJoined = false;
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

            if (joinButton != null)
                joinButton.onClick.AddListener(OnJoinClicked);

            if (leaveButton != null)
                leaveButton.onClick.AddListener(OnLeaveClicked);

            if (shareButton != null)
                shareButton.onClick.AddListener(OnShareClicked);

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
            prizePool = (decimal)PlayerPrefs.GetFloat("CashTournament_PrizePool", 80.00f);
            currentPlayers = PlayerPrefs.GetInt("CashTournament_CurrentPlayers", 8);
            maxPlayers = PlayerPrefs.GetInt("CashTournament_MaxPlayers", 16);
            maxAttempts = PlayerPrefs.GetInt("CashTournament_MaxAttempts", 3);
            timeLimitSeconds = PlayerPrefs.GetFloat("CashTournament_TimeLimit", 120f);

            // Start time: mock 45 minutes from now
            startTime = DateTime.Now.AddMinutes(45);

            PopulateTournamentInfo();
            PopulatePrizeDistribution();
            UpdateJoinLeaveState();
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
            // Calculate prize distribution: 50% / 30% / 20%
            decimal first = prizePool * 0.50m;
            decimal second = prizePool * 0.30m;
            decimal third = prizePool * 0.20m;

            if (prizeDistributionContainer != null)
            {
                // Update existing prize row texts if they exist
                for (int i = 0; i < prizeDistributionContainer.childCount; i++)
                {
                    Transform child = prizeDistributionContainer.GetChild(i);
                    Transform amountT = child.Find("Amount");
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

        private void OnJoinClicked()
        {
            if (hasJoined) return;

            // Mock wallet check
            float balance = PlayerPrefs.GetFloat("CashBattle_Balance", 100f);
            if ((decimal)balance < entryFee)
            {
                Debug.Log($"[CashTournamentLobby] Insufficient balance: ${balance:F2} < ${entryFee:F2}");
                if (statusText != null)
                    statusText.text = AutoLocalizer.Get("wallet_insufficient_balance");
                return;
            }

            // Deduct entry fee (mock)
            PlayerPrefs.SetFloat("CashBattle_Balance", balance - (float)entryFee);
            PlayerPrefs.Save();

            hasJoined = true;
            currentPlayers++;
            UpdatePlayersProgress(currentPlayers, maxPlayers);
            UpdateJoinLeaveState();

            Debug.Log($"[CashTournamentLobby] Joined tournament: {tournamentName} for ${entryFee:F2}");

            // Check if tournament is full and should start
            if (currentPlayers >= maxPlayers)
            {
                StartCountdown();
            }
        }

        private void OnLeaveClicked()
        {
            if (!hasJoined) return;

            // Refund entry fee (mock)
            float balance = PlayerPrefs.GetFloat("CashBattle_Balance", 100f);
            PlayerPrefs.SetFloat("CashBattle_Balance", balance + (float)entryFee);
            PlayerPrefs.Save();

            hasJoined = false;
            currentPlayers = Mathf.Max(0, currentPlayers - 1);
            UpdatePlayersProgress(currentPlayers, maxPlayers);
            UpdateJoinLeaveState();

            Debug.Log($"[CashTournamentLobby] Left tournament: {tournamentName}");
        }

        private void UpdateJoinLeaveState()
        {
            if (joinButton != null)
                joinButton.gameObject.SetActive(!hasJoined);

            if (leaveButton != null)
                leaveButton.gameObject.SetActive(hasJoined);

            if (joinButtonText != null)
                joinButtonText.text = AutoLocalizer.Get("tournament_join_fee", entryFee);
        }

        private void OnShareClicked()
        {
            string shareLink = $"digitpark://tournament/{tournamentId}";
            GUIUtility.systemCopyBuffer = shareLink;
            Debug.Log($"[CashTournamentLobby] Tournament link copied: {shareLink}");
        }

        private void OnSendChat()
        {
            if (chatInput == null || string.IsNullOrWhiteSpace(chatInput.text)) return;

            string message = chatInput.text.Trim();
            chatInput.text = "";

            // Add chat message locally (mock)
            AddChatMessage("You", message);

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
            if (hasJoined)
            {
                Debug.Log("[CashTournamentLobby] Warning: Player is leaving while joined in tournament.");
            }

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
