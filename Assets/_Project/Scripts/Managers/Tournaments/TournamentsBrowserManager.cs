using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using DigitPark.Monetization;
using DigitPark.Data;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager para la escena de navegación de torneos.
    /// Muestra lista de torneos disponibles con filtros.
    /// </summary>
    public class TournamentsBrowserManager : MonoBehaviour
    {
        [Header("UI - Header")]
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button createTournamentButton;

        [Header("UI - Filters")]
        [SerializeField] private TMP_Dropdown gameTypeFilter;
        [SerializeField] private TMP_Dropdown entryFeeFilter;
        [SerializeField] private TMP_Dropdown statusFilter;
        [SerializeField] private TMP_InputField searchInput;
        [SerializeField] private Button clearFiltersButton;

        [Header("UI - Tabs")]
        [SerializeField] private Button allTournamentsTab;
        [SerializeField] private Button myTournamentsTab;
        [SerializeField] private Button featuredTab;

        [Header("UI - Tournament List")]
        [SerializeField] private Transform tournamentsContainer;
        [SerializeField] private GameObject tournamentItemPrefab;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private TextMeshProUGUI emptyStateText;
        [SerializeField] private Button loadMoreButton;

        [Header("UI - Loading")]
        [SerializeField] private GameObject loadingIndicator;
        [SerializeField] private GameObject refreshIndicator;

        [Header("Configuration")]
        [SerializeField] private int tournamentsPerPage = 20;
        [SerializeField] private float autoRefreshInterval = 30f;

        // State
        private TournamentBrowserTab currentTab = TournamentBrowserTab.All;
        private List<TournamentData> loadedTournaments = new List<TournamentData>();
        private List<GameObject> spawnedItems = new List<GameObject>();
        private int currentPage = 0;
        private bool isLoading = false;

        // Filters
        private string currentGameTypeFilter = "";
        private string currentEntryFeeFilter = "";
        private string currentStatusFilter = "";
        private string currentSearchQuery = "";

        public enum TournamentBrowserTab
        {
            All,
            MyTournaments,
            Featured
        }

        private void Start()
        {
            SetupUI();
            SetupListeners();
            SetupFilters();
            LoadTournaments(reset: true);

            // Auto refresh
            if (autoRefreshInterval > 0)
            {
                InvokeRepeating(nameof(RefreshTournaments), autoRefreshInterval, autoRefreshInterval);
            }
        }

        private void OnDestroy()
        {
            CancelInvoke();
        }

        private void SetupUI()
        {
            if (loadingIndicator) loadingIndicator.SetActive(false);
            if (refreshIndicator) refreshIndicator.SetActive(false);
            if (emptyStateText) emptyStateText.gameObject.SetActive(false);

            UpdateTabVisuals();
        }

        private void SetupListeners()
        {
            if (backButton) backButton.onClick.AddListener(OnBackClicked);
            if (createTournamentButton) createTournamentButton.onClick.AddListener(OnCreateTournamentClicked);
            if (clearFiltersButton) clearFiltersButton.onClick.AddListener(ClearFilters);
            if (loadMoreButton) loadMoreButton.onClick.AddListener(LoadMoreTournaments);

            // Tabs
            if (allTournamentsTab) allTournamentsTab.onClick.AddListener(() => SwitchTab(TournamentBrowserTab.All));
            if (myTournamentsTab) myTournamentsTab.onClick.AddListener(() => SwitchTab(TournamentBrowserTab.MyTournaments));
            if (featuredTab) featuredTab.onClick.AddListener(() => SwitchTab(TournamentBrowserTab.Featured));

            // Filters
            if (gameTypeFilter) gameTypeFilter.onValueChanged.AddListener(_ => OnFilterChanged());
            if (entryFeeFilter) entryFeeFilter.onValueChanged.AddListener(_ => OnFilterChanged());
            if (statusFilter) statusFilter.onValueChanged.AddListener(_ => OnFilterChanged());
            if (searchInput) searchInput.onEndEdit.AddListener(OnSearchChanged);
        }

        private void SetupFilters()
        {
            // Game type filter
            if (gameTypeFilter)
            {
                gameTypeFilter.ClearOptions();
                gameTypeFilter.AddOptions(new List<string> {
                    "Todos los juegos",
                    "Memory Pairs",
                    "Quick Math",
                    "Flash Tap",
                    "Odd One Out",
                    "Digit Rush"
                });
            }

            // Entry fee filter
            if (entryFeeFilter)
            {
                entryFeeFilter.ClearOptions();
                entryFeeFilter.AddOptions(new List<string> {
                    "Cualquier entrada",
                    "Gratis",
                    "$1 - $5",
                    "$5 - $10",
                    "$10 - $25",
                    "$25+"
                });
            }

            // Status filter
            if (statusFilter)
            {
                statusFilter.ClearOptions();
                statusFilter.AddOptions(new List<string> {
                    "Todos",
                    "Registro abierto",
                    "En progreso",
                    "Completados"
                });
            }
        }

        private void SwitchTab(TournamentBrowserTab tab)
        {
            if (currentTab == tab) return;

            currentTab = tab;
            UpdateTabVisuals();
            LoadTournaments(reset: true);
        }

        private void UpdateTabVisuals()
        {
            Color activeColor = new Color(0f, 0.83f, 1f);
            Color inactiveColor = new Color(0.5f, 0.5f, 0.5f);

            UpdateTabButton(allTournamentsTab, currentTab == TournamentBrowserTab.All, activeColor, inactiveColor);
            UpdateTabButton(myTournamentsTab, currentTab == TournamentBrowserTab.MyTournaments, activeColor, inactiveColor);
            UpdateTabButton(featuredTab, currentTab == TournamentBrowserTab.Featured, activeColor, inactiveColor);
        }

        private void UpdateTabButton(Button button, bool isActive, Color activeColor, Color inactiveColor)
        {
            if (button == null) return;

            var image = button.GetComponent<Image>();
            if (image) image.color = isActive ? activeColor : inactiveColor;
        }

        private void OnFilterChanged()
        {
            currentGameTypeFilter = gameTypeFilter != null && gameTypeFilter.value > 0
                ? gameTypeFilter.options[gameTypeFilter.value].text : "";
            currentEntryFeeFilter = entryFeeFilter != null && entryFeeFilter.value > 0
                ? entryFeeFilter.options[entryFeeFilter.value].text : "";
            currentStatusFilter = statusFilter != null && statusFilter.value > 0
                ? statusFilter.options[statusFilter.value].text : "";

            LoadTournaments(reset: true);
        }

        private void OnSearchChanged(string query)
        {
            currentSearchQuery = query;
            LoadTournaments(reset: true);
        }

        private void ClearFilters()
        {
            if (gameTypeFilter) gameTypeFilter.value = 0;
            if (entryFeeFilter) entryFeeFilter.value = 0;
            if (statusFilter) statusFilter.value = 0;
            if (searchInput) searchInput.text = "";

            currentGameTypeFilter = "";
            currentEntryFeeFilter = "";
            currentStatusFilter = "";
            currentSearchQuery = "";

            LoadTournaments(reset: true);
        }

        private void LoadTournaments(bool reset)
        {
            if (isLoading) return;

            if (reset)
            {
                currentPage = 0;
                ClearTournamentItems();
            }

            isLoading = true;
            if (loadingIndicator) loadingIndicator.SetActive(true);

            // Simulate API call
            Invoke(nameof(SimulateLoadTournaments), 0.5f);
        }

        private void SimulateLoadTournaments()
        {
            // Generate mock data
            var tournaments = GenerateMockTournaments();

            loadedTournaments.AddRange(tournaments);
            PopulateTournaments(tournaments);

            isLoading = false;
            if (loadingIndicator) loadingIndicator.SetActive(false);

            // Update empty state
            if (emptyStateText)
            {
                emptyStateText.gameObject.SetActive(loadedTournaments.Count == 0);
                emptyStateText.text = GetEmptyStateMessage();
            }

            // Update load more button
            if (loadMoreButton)
            {
                loadMoreButton.gameObject.SetActive(tournaments.Count >= tournamentsPerPage);
            }
        }

        private List<TournamentData> GenerateMockTournaments()
        {
            var tournaments = new List<TournamentData>();
            string[] categories = { "Memory Pairs", "Quick Math", "Flash Tap", "Odd One Out" };
            TournamentStatus[] statuses = { TournamentStatus.Scheduled, TournamentStatus.Active, TournamentStatus.Completed };

            for (int i = 0; i < tournamentsPerPage; i++)
            {
                var tournament = new TournamentData
                {
                    name = $"Torneo {currentPage * tournamentsPerPage + i + 1}",
                    category = categories[UnityEngine.Random.Range(0, categories.Length)],
                    entryFee = UnityEngine.Random.Range(0, 5) * 5,
                    totalPrizePool = UnityEngine.Random.Range(50, 500),
                    currentParticipants = UnityEngine.Random.Range(5, 50),
                    maxParticipants = 64,
                    status = statuses[UnityEngine.Random.Range(0, statuses.Length)],
                    startTime = DateTime.Now.AddHours(UnityEngine.Random.Range(1, 48)),
                    endTime = DateTime.Now.AddHours(UnityEngine.Random.Range(49, 96))
                };
                tournaments.Add(tournament);
            }

            return tournaments;
        }

        private void PopulateTournaments(List<TournamentData> tournaments)
        {
            if (tournamentsContainer == null) return;

            foreach (var tournament in tournaments)
            {
                GameObject item;

                if (tournamentItemPrefab != null)
                {
                    item = Instantiate(tournamentItemPrefab, tournamentsContainer);
                }
                else
                {
                    item = CreateTournamentItemFallback(tournament);
                }

                spawnedItems.Add(item);

                // Setup item UI (would use TournamentItemUI component)
                var texts = item.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length > 0) texts[0].text = tournament.name;
                if (texts.Length > 1) texts[1].text = tournament.category;
                if (texts.Length > 2) texts[2].text = $"${tournament.entryFee}";

                var button = item.GetComponent<Button>();
                if (button)
                {
                    var t = tournament;
                    button.onClick.AddListener(() => OnTournamentClicked(t));
                }
            }
        }

        private GameObject CreateTournamentItemFallback(TournamentData tournament)
        {
            var item = new GameObject($"Tournament_{tournament.tournamentId.Substring(0, 8)}");
            item.transform.SetParent(tournamentsContainer, false);

            var rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(350, 100);

            var image = item.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

            item.AddComponent<Button>();

            // Add text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(item.transform, false);
            var textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(10, 10);
            textRT.offsetMax = new Vector2(-10, -10);

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = $"{tournament.name}\n{tournament.category} - ${tournament.entryFee}";
            tmp.fontSize = 16;
            tmp.alignment = TextAlignmentOptions.Left;

            return item;
        }

        private void ClearTournamentItems()
        {
            foreach (var item in spawnedItems)
            {
                if (item) Destroy(item);
            }
            spawnedItems.Clear();
            loadedTournaments.Clear();
        }

        private void LoadMoreTournaments()
        {
            currentPage++;
            LoadTournaments(reset: false);
        }

        private void RefreshTournaments()
        {
            if (refreshIndicator) refreshIndicator.SetActive(true);
            LoadTournaments(reset: true);
        }

        private string GetEmptyStateMessage()
        {
            switch (currentTab)
            {
                case TournamentBrowserTab.MyTournaments:
                    return "No estás participando en ningún torneo.\n¡Únete a uno!";
                case TournamentBrowserTab.Featured:
                    return "No hay torneos destacados en este momento.";
                default:
                    return "No se encontraron torneos con los filtros seleccionados.";
            }
        }

        private void OnTournamentClicked(TournamentData tournament)
        {
            Debug.Log($"[TournamentsBrowser] Tournament clicked: {tournament.name}");

            // Navigate to tournament lobby
            SceneNavigator.Instance?.NavigateTo(SceneNavigator.Scenes.TOURNAMENT_LOBBY,
                new SceneNavigator.NavigationParams
                {
                    ItemId = tournament.tournamentId,
                    CustomData = tournament
                });
        }

        private void OnCreateTournamentClicked()
        {
            SceneNavigator.Instance?.NavigateTo(SceneNavigator.Scenes.TOURNAMENT_CREATE);
        }

        private void OnBackClicked()
        {
            SceneNavigator.Instance?.GoBack();
        }
    }
}
