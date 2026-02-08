using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using DigitPark.Monetization;
using DigitPark.Data;
using DigitPark.Localization;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager para la escena del lobby de torneo.
    /// Muestra detalles del torneo, participantes y permite unirse/salir.
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
        [SerializeField] private Image gameTypeIcon;
        [SerializeField] private TextMeshProUGUI entryFeeText;
        [SerializeField] private TextMeshProUGUI prizePoolText;
        [SerializeField] private TextMeshProUGUI playersCountText;
        [SerializeField] private Slider playersProgressBar;
        [SerializeField] private TextMeshProUGUI startTimeText;
        [SerializeField] private TextMeshProUGUI countdownText;

        [Header("UI - Rules")]
        [SerializeField] private TextMeshProUGUI roundsText;
        [SerializeField] private TextMeshProUGUI timeLimitText;
        [SerializeField] private TextMeshProUGUI formatText;

        [Header("UI - Prize Distribution")]
        [SerializeField] private Transform prizeDistributionContainer;
        [SerializeField] private GameObject prizeRowPrefab;

        [Header("UI - Participants")]
        [SerializeField] private Transform participantsContainer;
        [SerializeField] private GameObject participantItemPrefab;
        [SerializeField] private TextMeshProUGUI participantsHeaderText;
        [SerializeField] private Button viewAllParticipantsButton;

        [Header("UI - Chat")]
        [SerializeField] private GameObject chatPanel;
        [SerializeField] private Transform chatMessagesContainer;
        [SerializeField] private TMP_InputField chatInput;
        [SerializeField] private Button sendChatButton;
        [SerializeField] private ScrollRect chatScrollRect;

        [Header("UI - Actions")]
        [SerializeField] private Button joinButton;
        [SerializeField] private Button leaveButton;
        [SerializeField] private Button shareButton;
        [SerializeField] private Button readyButton;
        [SerializeField] private TextMeshProUGUI joinButtonText;

        [Header("UI - Status")]
        [SerializeField] private GameObject loadingOverlay;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private GameObject startingOverlay;
        [SerializeField] private TextMeshProUGUI startingCountdownText;

        [Header("Configuration")]
        [SerializeField] private float refreshInterval = 5f;

        // State
        private TournamentData currentTournament;
        private List<ParticipantData> participants = new List<ParticipantData>();
        private bool hasJoined = false;
        private bool isReady = false;
        private bool isLoading = false;

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
            CancelInvoke();
        }

        private void SetupUI()
        {
            if (loadingOverlay) loadingOverlay.SetActive(false);
            if (startingOverlay) startingOverlay.SetActive(false);
            if (leaveButton) leaveButton.gameObject.SetActive(false);
            if (readyButton) readyButton.gameObject.SetActive(false);

            UpdateActionButtons();
        }

        private void SetupListeners()
        {
            if (backButton) backButton.onClick.AddListener(OnBackClicked);
            if (joinButton) joinButton.onClick.AddListener(OnJoinClicked);
            if (leaveButton) leaveButton.onClick.AddListener(OnLeaveClicked);
            if (shareButton) shareButton.onClick.AddListener(OnShareClicked);
            if (readyButton) readyButton.onClick.AddListener(OnReadyClicked);
            if (viewAllParticipantsButton) viewAllParticipantsButton.onClick.AddListener(OnViewAllParticipants);
            if (sendChatButton) sendChatButton.onClick.AddListener(OnSendChat);

            if (chatInput)
            {
                chatInput.onSubmit.AddListener(_ => OnSendChat());
            }
        }

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
        }

        private void LoadTournamentById(string id)
        {
            isLoading = true;
            if (loadingOverlay) loadingOverlay.SetActive(true);

            // Simulate API call
            Invoke(nameof(SimulateLoadTournament), 1f);
        }

        private void SimulateLoadTournament()
        {
            currentTournament = CreateMockTournament();
            isLoading = false;
            if (loadingOverlay) loadingOverlay.SetActive(false);
            UpdateUI();
            LoadParticipants();
        }

        private TournamentData CreateMockTournament()
        {
            return new TournamentData
            {
                name = "Torneo de Prueba",
                category = "Memory Pairs",
                entryFee = 5,
                totalPrizePool = 150,
                currentParticipants = 12,
                maxParticipants = 32,
                status = TournamentStatus.Scheduled,
                startTime = DateTime.Now.AddMinutes(30),
                endTime = DateTime.Now.AddHours(2)
            };
        }

        private void UpdateUI()
        {
            if (currentTournament == null) return;

            // Header
            if (tournamentNameText) tournamentNameText.text = currentTournament.name;
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

            // Players
            if (playersCountText) playersCountText.text = $"{currentTournament.currentParticipants}/{currentTournament.maxParticipants}";
            if (playersProgressBar)
            {
                playersProgressBar.maxValue = currentTournament.maxParticipants;
                playersProgressBar.value = currentTournament.currentParticipants;
            }

            // Time
            UpdateTimeDisplay();

            // Rules
            if (roundsText) roundsText.text = L("tournament_best_of_3");
            if (timeLimitText) timeLimitText.text = L("tournament_60_seconds");
            if (formatText) formatText.text = L("tournament_elimination");

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

            TimeSpan timeUntilStart = currentTournament.startTime - DateTime.Now;

            if (startTimeText)
            {
                startTimeText.text = currentTournament.startTime.ToString("HH:mm dd/MM");
            }

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
            // Update countdown every frame
            UpdateTimeDisplay();

            // Check if tournament is starting
            if (currentTournament != null && hasJoined)
            {
                TimeSpan timeUntilStart = currentTournament.startTime - DateTime.Now;
                if (timeUntilStart.TotalSeconds <= 10 && timeUntilStart.TotalSeconds > 0)
                {
                    ShowStartingOverlay((int)timeUntilStart.TotalSeconds);
                }
                else if (timeUntilStart.TotalSeconds <= 0)
                {
                    StartTournament();
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
                text.fontSize = 14;
            }
        }

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
                    avatarUrl = "",
                    isReady = UnityEngine.Random.value > 0.5f
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
        }

        private void CreateParticipantItem(ParticipantData participant)
        {
            GameObject item;
            if (participantItemPrefab != null)
            {
                item = Instantiate(participantItemPrefab, participantsContainer);
            }
            else
            {
                item = new GameObject(participant.username);
                item.transform.SetParent(participantsContainer, false);
                var rt = item.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(200, 40);

                var text = item.AddComponent<TextMeshProUGUI>();
                text.text = participant.username + (participant.isReady ? " [OK]" : "");
                text.fontSize = 14;
                text.color = participant.isReady ? new Color(0f, 1f, 0.5f) : Color.white;
            }
        }

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
            if (readyButton)
            {
                readyButton.gameObject.SetActive(hasJoined);
                var readyText = readyButton.GetComponentInChildren<TextMeshProUGUI>();
                if (readyText) readyText.text = isReady ? L("tournament_ready") : L("tournament_mark_ready");
            }
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

        private void OnJoinClicked()
        {
            if (isLoading || hasJoined) return;

            isLoading = true;
            if (loadingOverlay) loadingOverlay.SetActive(true);
            ShowStatus(L("tournament_joining"));

            // Simulate join
            Invoke(nameof(ProcessJoin), 1.5f);
        }

        private void ProcessJoin()
        {
            hasJoined = true;
            isLoading = false;
            if (loadingOverlay) loadingOverlay.SetActive(false);
            ShowStatus(L("tournament_joined"));

            currentTournament.currentParticipants++;
            UpdateUI();
            UpdateActionButtons();
        }

        private void OnLeaveClicked()
        {
            if (isLoading || !hasJoined) return;

            hasJoined = false;
            isReady = false;
            currentTournament.currentParticipants--;
            UpdateUI();
            UpdateActionButtons();
            ShowStatus(L("tournament_left"));
        }

        private void OnReadyClicked()
        {
            isReady = !isReady;
            UpdateActionButtons();
            ShowStatus(isReady ? L("tournament_you_ready") : L("tournament_not_ready"));
        }

        private void OnShareClicked()
        {
            string shareText = L("tournament_share_text", currentTournament?.name);
            GUIUtility.systemCopyBuffer = shareText;
            ShowStatus(L("tournament_link_copied"));
        }

        private void OnViewAllParticipants()
        {
            Debug.Log("[TournamentLobby] View all participants");
            // Could open a modal with full list
        }

        private void OnSendChat()
        {
            if (chatInput == null || string.IsNullOrWhiteSpace(chatInput.text)) return;

            string message = chatInput.text;
            chatInput.text = "";

            // Add message to chat (would send to server in production)
            Debug.Log($"[TournamentLobby] Chat: {message}");
        }

        private void ShowStartingOverlay(int seconds)
        {
            if (startingOverlay) startingOverlay.SetActive(true);
            if (startingCountdownText) startingCountdownText.text = seconds.ToString();
        }

        private void StartTournament()
        {
            Debug.Log("[TournamentLobby] Tournament starting!");
            // Would navigate to game scene with tournament context
        }

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

        private string L(string key, params object[] args)
        {
            if (LocalizationManager.Instance == null) return key;
            string text = LocalizationManager.Instance.GetText(key);
            return args.Length > 0 ? string.Format(text, args) : text;
        }
    }

    [Serializable]
    public class ParticipantData
    {
        public string id;
        public string username;
        public string avatarUrl;
        public bool isReady;
        public int rank;
    }
}
