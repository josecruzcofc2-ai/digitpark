using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Games;
using DigitPark.Managers;
using DigitPark.UI.Panels;
using DigitPark.Localization;

namespace DigitPark.UI.CashBattle
{
    /// <summary>
    /// Panel showing available cash tournaments
    /// </summary>
    public class TournamentListPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button backButton;
        [SerializeField] private Transform tournamentsContainer;
        [SerializeField] private GameObject tournamentCardPrefab;
        [SerializeField] private TextMeshProUGUI noTournamentsText;

        [Header("Filter")]
        [SerializeField] private TMP_Dropdown gameFilterDropdown;
        [SerializeField] private TMP_Dropdown feeFilterDropdown;

        [Header("Refresh")]
        [SerializeField] private Button refreshButton;
        [SerializeField] private GameObject loadingIndicator;

        [Header("Create Tournament (Premium)")]
        [SerializeField] private Button createTournamentButton;
        [SerializeField] private GameObject createTournamentPanel;
        [SerializeField] private Slider maxPlayersSlider;
        [SerializeField] private TextMeshProUGUI maxPlayersText;
        [SerializeField] private Slider entryFeeSlider;
        [SerializeField] private TextMeshProUGUI entryFeeText;
        [SerializeField] private Slider durationSlider;
        [SerializeField] private TextMeshProUGUI durationText;
        [SerializeField] private TMP_Dropdown gameTypeDropdown;
        [SerializeField] private Button confirmCreateButton;
        [SerializeField] private Button cancelCreateButton;

        [Header("Premium Required Panel")]
        [SerializeField] private ConfirmPanelUI premiumRequiredPanel;

        // Events
        public event Action OnBackClicked;
        public event Action<TournamentInfo> OnTournamentSelected;
        public event Action<TournamentInfo> OnTournamentCreated;

        // Data
        private List<TournamentInfo> allTournaments = new List<TournamentInfo>();
        private List<TournamentInfo> filteredTournaments = new List<TournamentInfo>();

        private void Start()
        {
            SetupListeners();
            SetupFilters();
            LoadTournaments();
        }

        private void SetupListeners()
        {
            backButton?.onClick.AddListener(() => OnBackClicked?.Invoke());
            refreshButton?.onClick.AddListener(LoadTournaments);
            gameFilterDropdown?.onValueChanged.AddListener(_ => ApplyFilters());
            feeFilterDropdown?.onValueChanged.AddListener(_ => ApplyFilters());

            // Create tournament (Premium feature)
            createTournamentButton?.onClick.AddListener(OnCreateTournamentClicked);
            confirmCreateButton?.onClick.AddListener(OnConfirmCreateTournament);
            cancelCreateButton?.onClick.AddListener(HideCreatePanel);

            // Sliders
            maxPlayersSlider?.onValueChanged.AddListener(OnMaxPlayersChanged);
            entryFeeSlider?.onValueChanged.AddListener(OnEntryFeeChanged);
            durationSlider?.onValueChanged.AddListener(OnDurationChanged);
        }

        #region Create Tournament (Premium)

        /// <summary>
        /// Called when user clicks "Create Tournament" button
        /// Requires Premium to create paid tournaments
        /// </summary>
        private void OnCreateTournamentClicked()
        {
            Debug.Log("[CashBattle] Create Tournament clicked");

            // Check if user has Premium
            if (PremiumManager.Instance == null || !PremiumManager.Instance.CanCreateTournaments)
            {
                Debug.Log("[CashBattle] Premium required for paid tournaments");
                ShowPremiumRequiredPanel();
                return;
            }

            ShowCreatePanel();
        }

        private void ShowPremiumRequiredPanel()
        {
            if (premiumRequiredPanel != null)
            {
                premiumRequiredPanel.SetButtonTexts(
                    AutoLocalizer.Get("get_premium"),
                    AutoLocalizer.Get("maybe_later")
                );

                premiumRequiredPanel.Show(
                    AutoLocalizer.Get("premium_required_title"),
                    AutoLocalizer.Get("premium_cash_tournaments_message"),
                    OnGetPremiumClicked,
                    () => premiumRequiredPanel.Hide()
                );
            }
        }

        private void OnGetPremiumClicked()
        {
            premiumRequiredPanel?.Hide();
            // Open premium panel
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                PremiumPanelUI.CreateAndShow(canvas.transform);
            }
        }

        private void ShowCreatePanel()
        {
            if (createTournamentPanel != null)
            {
                createTournamentPanel.SetActive(true);

                // Reset sliders to default values
                if (maxPlayersSlider != null)
                {
                    maxPlayersSlider.value = 16;
                    OnMaxPlayersChanged(16);
                }
                if (entryFeeSlider != null)
                {
                    entryFeeSlider.value = 10;
                    OnEntryFeeChanged(10);
                }
                if (durationSlider != null)
                {
                    durationSlider.value = 24;
                    OnDurationChanged(24);
                }

                // Setup game type dropdown
                SetupGameTypeDropdown();
            }
        }

        private void HideCreatePanel()
        {
            if (createTournamentPanel != null)
                createTournamentPanel.SetActive(false);
        }

        private void SetupGameTypeDropdown()
        {
            if (gameTypeDropdown == null) return;

            gameTypeDropdown.ClearOptions();
            var options = new List<string>();

            var gameInfos = CognitiveSprintManager.GetAllGameInfos();
            foreach (var info in gameInfos)
            {
                options.Add(info.Name);
            }
            options.Add("Cognitive Sprint");

            gameTypeDropdown.AddOptions(options);
        }

        private void OnMaxPlayersChanged(float value)
        {
            int players = (int)value;
            if (maxPlayersText != null)
                maxPlayersText.text = $"{players} jugadores";
        }

        private void OnEntryFeeChanged(float value)
        {
            decimal fee = (decimal)value;
            if (entryFeeText != null)
                entryFeeText.text = $"${fee:F2}";
        }

        private void OnDurationChanged(float value)
        {
            int hours = (int)value;
            if (durationText != null)
            {
                if (hours < 24)
                    durationText.text = $"{hours}h";
                else
                    durationText.text = $"{hours / 24}d";
            }
        }

        private void OnConfirmCreateTournament()
        {
            // Get values from sliders
            int maxPlayers = (int)(maxPlayersSlider?.value ?? 16);
            decimal entryFee = (decimal)(entryFeeSlider?.value ?? 10);
            int durationHours = (int)(durationSlider?.value ?? 24);

            // Get game type
            var gameInfos = CognitiveSprintManager.GetAllGameInfos();
            int selectedGame = gameTypeDropdown?.value ?? 0;
            bool isCognitiveSprint = selectedGame >= gameInfos.Length;
            GameType? gameType = isCognitiveSprint ? null : (GameType?)gameInfos[selectedGame].Type;

            // Create tournament info
            var tournament = new TournamentInfo
            {
                Id = Guid.NewGuid().ToString(),
                Name = isCognitiveSprint ? "Cognitive Sprint Tournament" : $"{gameType} Tournament",
                GameType = gameType,
                IsCognitiveSprint = isCognitiveSprint,
                EntryFee = entryFee,
                PrizePool = entryFee * maxPlayers * 0.9m, // 90% pool, 10% commission
                CurrentParticipants = 1, // Creator auto-joins
                MaxParticipants = maxPlayers,
                StartsAt = DateTime.Now.AddHours(durationHours),
                Status = TournamentStatus.Registration
            };

            Debug.Log($"[CashBattle] Creating paid tournament: {tournament.Name}, Entry: ${entryFee}");

            // TODO: Send to Triumph API
            OnTournamentCreated?.Invoke(tournament);

            HideCreatePanel();
            LoadTournaments(); // Refresh list
        }

        #endregion

        private void SetupFilters()
        {
            // Game filter
            if (gameFilterDropdown != null)
            {
                gameFilterDropdown.ClearOptions();
                var gameOptions = new List<string> { "Todos los juegos" };
                var gameInfos = CognitiveSprintManager.GetAllGameInfos();
                foreach (var info in gameInfos)
                {
                    gameOptions.Add(info.Name);
                }
                gameOptions.Add("Cognitive Sprint");
                gameFilterDropdown.AddOptions(gameOptions);
            }

            // Fee filter
            if (feeFilterDropdown != null)
            {
                feeFilterDropdown.ClearOptions();
                var feeOptions = new List<string>
                {
                    "Todas las entradas",
                    "$1 - $10",
                    "$11 - $50",
                    "$51 - $100",
                    "$100+"
                };
                feeFilterDropdown.AddOptions(feeOptions);
            }
        }

        private void LoadTournaments()
        {
            if (loadingIndicator != null) loadingIndicator.SetActive(true);
            if (noTournamentsText != null) noTournamentsText.gameObject.SetActive(false);

            // TODO: Load from Triumph API
            // For now, create mock data
            LoadMockTournaments();

            if (loadingIndicator != null) loadingIndicator.SetActive(false);

            ApplyFilters();
        }

        private void LoadMockTournaments()
        {
            allTournaments.Clear();

            // Mock tournament data
            allTournaments.Add(new TournamentInfo
            {
                Id = "t1",
                Name = "Quick Math Championship",
                GameType = GameType.QuickMath,
                IsCognitiveSprint = false,
                EntryFee = 5m,
                PrizePool = 100m,
                CurrentParticipants = 12,
                MaxParticipants = 16,
                StartsAt = DateTime.Now.AddHours(2),
                Status = TournamentStatus.Registration
            });

            allTournaments.Add(new TournamentInfo
            {
                Id = "t2",
                Name = "Flash Tap Masters",
                GameType = GameType.FlashTap,
                IsCognitiveSprint = false,
                EntryFee = 10m,
                PrizePool = 250m,
                CurrentParticipants = 28,
                MaxParticipants = 32,
                StartsAt = DateTime.Now.AddHours(1),
                Status = TournamentStatus.Registration
            });

            allTournaments.Add(new TournamentInfo
            {
                Id = "t3",
                Name = "Cognitive Sprint Elite",
                GameType = null,
                IsCognitiveSprint = true,
                EntryFee = 25m,
                PrizePool = 500m,
                CurrentParticipants = 8,
                MaxParticipants = 16,
                StartsAt = DateTime.Now.AddHours(3),
                Status = TournamentStatus.Registration
            });

            allTournaments.Add(new TournamentInfo
            {
                Id = "t4",
                Name = "Memory Pairs Daily",
                GameType = GameType.MemoryPairs,
                IsCognitiveSprint = false,
                EntryFee = 1m,
                PrizePool = 20m,
                CurrentParticipants = 18,
                MaxParticipants = 20,
                StartsAt = DateTime.Now.AddMinutes(30),
                Status = TournamentStatus.Registration
            });

            allTournaments.Add(new TournamentInfo
            {
                Id = "t5",
                Name = "Odd One Out High Stakes",
                GameType = GameType.OddOneOut,
                IsCognitiveSprint = false,
                EntryFee = 100m,
                PrizePool = 2000m,
                CurrentParticipants = 4,
                MaxParticipants = 8,
                StartsAt = DateTime.Now.AddHours(5),
                Status = TournamentStatus.Registration
            });
        }

        private void ApplyFilters()
        {
            filteredTournaments.Clear();

            foreach (var tournament in allTournaments)
            {
                if (PassesGameFilter(tournament) && PassesFeeFilter(tournament))
                {
                    filteredTournaments.Add(tournament);
                }
            }

            UpdateTournamentList();
        }

        private bool PassesGameFilter(TournamentInfo tournament)
        {
            if (gameFilterDropdown == null || gameFilterDropdown.value == 0) return true;

            var gameInfos = CognitiveSprintManager.GetAllGameInfos();
            int selectedIndex = gameFilterDropdown.value - 1; // -1 because first option is "All"

            if (selectedIndex >= gameInfos.Length)
            {
                // Cognitive Sprint option
                return tournament.IsCognitiveSprint;
            }

            return tournament.GameType == gameInfos[selectedIndex].Type;
        }

        private bool PassesFeeFilter(TournamentInfo tournament)
        {
            if (feeFilterDropdown == null || feeFilterDropdown.value == 0) return true;

            decimal fee = tournament.EntryFee;

            switch (feeFilterDropdown.value)
            {
                case 1: return fee >= 1 && fee <= 10;
                case 2: return fee >= 11 && fee <= 50;
                case 3: return fee >= 51 && fee <= 100;
                case 4: return fee > 100;
                default: return true;
            }
        }

        private void UpdateTournamentList()
        {
            // Clear existing cards
            if (tournamentsContainer != null)
            {
                foreach (Transform child in tournamentsContainer)
                {
                    Destroy(child.gameObject);
                }
            }

            // Show "no tournaments" message if empty
            if (filteredTournaments.Count == 0)
            {
                if (noTournamentsText != null)
                {
                    noTournamentsText.gameObject.SetActive(true);
                    noTournamentsText.text = "No hay torneos disponibles";
                }
                return;
            }

            if (noTournamentsText != null) noTournamentsText.gameObject.SetActive(false);

            // Create cards
            foreach (var tournament in filteredTournaments)
            {
                CreateTournamentCard(tournament);
            }
        }

        private void CreateTournamentCard(TournamentInfo tournament)
        {
            if (tournamentsContainer == null) return;

            // If prefab not assigned, create a basic card
            GameObject card = tournamentCardPrefab != null
                ? Instantiate(tournamentCardPrefab, tournamentsContainer)
                : CreateDefaultTournamentCard(tournamentsContainer);

            // Setup card
            SetupTournamentCard(card, tournament);
        }

        private GameObject CreateDefaultTournamentCard(Transform parent)
        {
            GameObject card = new GameObject("TournamentCard");
            card.transform.SetParent(parent, false);

            RectTransform rt = card.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 180);

            // Background
            Image bg = card.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.1f, 0.15f, 0.95f);

            // Add outline
            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = new Color(0.85f, 0.65f, 0.13f, 0.6f);
            outline.effectDistance = new Vector2(2, -2);

            // Add layout
            HorizontalLayoutGroup layout = card.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 15, 15);
            layout.spacing = 15;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Info container
            GameObject infoContainer = new GameObject("Info");
            infoContainer.transform.SetParent(card.transform, false);

            RectTransform infoRT = infoContainer.AddComponent<RectTransform>();
            LayoutElement infoLE = infoContainer.AddComponent<LayoutElement>();
            infoLE.flexibleWidth = 1;
            infoLE.preferredHeight = 150;

            VerticalLayoutGroup infoLayout = infoContainer.AddComponent<VerticalLayoutGroup>();
            infoLayout.spacing = 5;
            infoLayout.childForceExpandWidth = true;
            infoLayout.childForceExpandHeight = false;

            // Name
            CreateLabel(infoContainer.transform, "Name", "Tournament Name", 32, true, new Color(1f, 0.84f, 0f));

            // Game type
            CreateLabel(infoContainer.transform, "GameType", "Game Type", 24, true, new Color(0f, 0.9f, 1f));

            // Prize pool
            CreateLabel(infoContainer.transform, "PrizePool", "Prize: $0", 28, true, new Color(0.3f, 1f, 0.5f));

            // Entry fee
            CreateLabel(infoContainer.transform, "EntryFee", "Entry: $0", 22, true, Color.white);

            // Participants
            CreateLabel(infoContainer.transform, "Participants", "0/0 jugadores", 20, false, new Color(0.7f, 0.7f, 0.7f));

            // Join button
            GameObject joinBtn = new GameObject("JoinButton");
            joinBtn.transform.SetParent(card.transform, false);

            RectTransform joinRT = joinBtn.AddComponent<RectTransform>();
            LayoutElement joinLE = joinBtn.AddComponent<LayoutElement>();
            joinLE.preferredWidth = 150;
            joinLE.preferredHeight = 80;

            Image joinBg = joinBtn.AddComponent<Image>();
            joinBg.color = new Color(0.85f, 0.65f, 0.13f, 1f);

            Button button = joinBtn.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(1f, 0.93f, 0.55f, 1f);
            colors.pressedColor = new Color(0.7f, 0.5f, 0.1f, 1f);
            button.colors = colors;

            // Join text
            GameObject joinTextObj = new GameObject("Text");
            joinTextObj.transform.SetParent(joinBtn.transform, false);

            RectTransform joinTextRT = joinTextObj.AddComponent<RectTransform>();
            joinTextRT.anchorMin = Vector2.zero;
            joinTextRT.anchorMax = Vector2.one;
            joinTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI joinText = joinTextObj.AddComponent<TextMeshProUGUI>();
            joinText.text = "Unirse";
            joinText.fontSize = 28;
            joinText.color = new Color(0.08f, 0.06f, 0.12f);
            joinText.alignment = TextAlignmentOptions.Center;
            joinText.fontStyle = FontStyles.Bold;

            return card;
        }

        private void CreateLabel(Transform parent, string name, string defaultText, int fontSize, bool bold, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            LayoutElement le = obj.AddComponent<LayoutElement>();
            le.preferredHeight = fontSize + 8;

            TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
            text.text = defaultText;
            text.fontSize = fontSize;
            text.color = color;
            text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        }

        private void SetupTournamentCard(GameObject card, TournamentInfo tournament)
        {
            // Find and set texts
            var nameText = card.transform.Find("Info/Name")?.GetComponent<TextMeshProUGUI>();
            var gameTypeText = card.transform.Find("Info/GameType")?.GetComponent<TextMeshProUGUI>();
            var prizeText = card.transform.Find("Info/PrizePool")?.GetComponent<TextMeshProUGUI>();
            var entryText = card.transform.Find("Info/EntryFee")?.GetComponent<TextMeshProUGUI>();
            var participantsText = card.transform.Find("Info/Participants")?.GetComponent<TextMeshProUGUI>();
            var joinButton = card.transform.Find("JoinButton")?.GetComponent<Button>();

            if (nameText != null) nameText.text = tournament.Name;

            if (gameTypeText != null)
            {
                gameTypeText.text = tournament.IsCognitiveSprint
                    ? "Cognitive Sprint"
                    : tournament.GameType?.ToString() ?? "Multiple";
            }

            if (prizeText != null) prizeText.text = $"Premio: ${tournament.PrizePool}";
            if (entryText != null) entryText.text = $"Entrada: ${tournament.EntryFee}";

            if (participantsText != null)
            {
                participantsText.text = $"{tournament.CurrentParticipants}/{tournament.MaxParticipants} jugadores";
            }

            // Join button
            if (joinButton != null)
            {
                joinButton.onClick.RemoveAllListeners();
                joinButton.onClick.AddListener(() => OnTournamentSelected?.Invoke(tournament));

                // Disable if full
                joinButton.interactable = tournament.CurrentParticipants < tournament.MaxParticipants;
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
            LoadTournaments();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Tournament information
    /// </summary>
    [Serializable]
    public class TournamentInfo
    {
        public string Id;
        public string Name;
        public GameType? GameType;
        public bool IsCognitiveSprint;
        public decimal EntryFee;
        public decimal PrizePool;
        public int CurrentParticipants;
        public int MaxParticipants;
        public DateTime StartsAt;
        public TournamentStatus Status;
    }

    public enum TournamentStatus
    {
        Registration,
        InProgress,
        Completed,
        Cancelled
    }
}
