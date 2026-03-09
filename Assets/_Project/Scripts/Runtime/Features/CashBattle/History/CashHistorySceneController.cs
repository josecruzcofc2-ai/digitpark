using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Collections.Generic;
using DigitPark.Monetization;
using DG.Tweening;
using DigitPark.Animations;
using DigitPark.Localization;

namespace DigitPark.CashBattle
{
    /// <summary>
    /// Main controller for the CashHistory scene.
    /// Handles all UI and logic for the History screen.
    /// </summary>
    public class CashHistorySceneController : MonoBehaviour
    {
        // ==================== HEADER ====================
        [Header("Header")]
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI titleText;

        // ==================== TABS ====================
        [Header("Tabs")]
        [SerializeField] private Button allTabButton;
        [SerializeField] private Button matchesTabButton;
        [SerializeField] private Button tournamentsTabButton;
        [SerializeField] private Color activeTabColor = new Color(0f, 0.83f, 1f, 1f);
        [SerializeField] private Color inactiveTabColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        // ==================== FILTERS ====================
        [Header("Filters")]
        [SerializeField] private TMP_Dropdown resultFilterDropdown;
        [SerializeField] private TMP_Dropdown dateFilterDropdown;
        [SerializeField] private Button clearFiltersButton;

        // ==================== LIST ====================
        [Header("History List")]
        [SerializeField] private Transform entriesContainer;
        [SerializeField] private GameObject historyEntryPrefab;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private TextMeshProUGUI emptyStateText;
        [SerializeField] private Button loadMoreButton;

        // ==================== DETAIL PANEL ====================
        [Header("Detail Panel")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private TextMeshProUGUI detailTitleText;
        [SerializeField] private TextMeshProUGUI detailSubtitleText;
        [SerializeField] private TextMeshProUGUI detailResultText;
        [SerializeField] private TextMeshProUGUI detailScoreText;
        [SerializeField] private TextMeshProUGUI detailEntryFeeText;
        [SerializeField] private TextMeshProUGUI detailPrizeText;
        [SerializeField] private TextMeshProUGUI detailNetText;
        [SerializeField] private TextMeshProUGUI detailDateText;
        [SerializeField] private TextMeshProUGUI detailDurationText;
        [SerializeField] private Button closeDetailButton;

        // ==================== LOADING ====================
        [Header("Loading")]
        [SerializeField] private GameObject loadingIndicator;

        // ==================== CONFIGURATION ====================
        [Header("Configuration")]
        [SerializeField] private int entriesPerPage = 20;

        // ==================== STATE ====================
        private HistoryTab currentTab = HistoryTab.All;
        private HistoryFilter currentFilter = new HistoryFilter();
        private int currentPage = 0;
        private List<GameObject> spawnedEntries = new List<GameObject>();
        private HistoryEntry selectedEntry;

        private enum HistoryTab
        {
            All,
            Matches,
            Tournaments
        }

        // ==================== LIFECYCLE ====================

        private void Start()
        {
            InitializeUI();
            SetupButtonListeners();
            SetupDropdowns();
            SubscribeToEvents();
            LoadEntries(reset: true);

            // Check navigation params
            CheckNavigationParams();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void InitializeUI()
        {
            // Hide panels
            if (detailPanel) detailPanel.SetActive(false);
            if (loadingIndicator) loadingIndicator.SetActive(false);

            // Set initial tab
            ShowTab(HistoryTab.All);
        }

        private void SetupButtonListeners()
        {
            // Back button - disable auto-navigation from BackButtonGold prefab to prevent double listener
            var autoNav = backButton?.GetComponent<DigitPark.UI.BackButtonGold>();
            if (autoNav != null) autoNav.DisableAutoNavigation();
            if (backButton)
                backButton.onClick.AddListener(OnBackClicked);

            // Tab buttons
            if (allTabButton)
                allTabButton.onClick.AddListener(() => ShowTab(HistoryTab.All));
            if (matchesTabButton)
                matchesTabButton.onClick.AddListener(() => ShowTab(HistoryTab.Matches));
            if (tournamentsTabButton)
                tournamentsTabButton.onClick.AddListener(() => ShowTab(HistoryTab.Tournaments));

            // Filters
            if (clearFiltersButton)
                clearFiltersButton.onClick.AddListener(ClearFilters);

            // Load more
            if (loadMoreButton)
                loadMoreButton.onClick.AddListener(OnLoadMoreClicked);

            // Detail panel
            if (closeDetailButton)
                closeDetailButton.onClick.AddListener(CloseDetailPanel);
        }

        private void SetupDropdowns()
        {
            // Result filter dropdown
            if (resultFilterDropdown)
            {
                resultFilterDropdown.ClearOptions();
                resultFilterDropdown.AddOptions(new List<string>
                {
                    "All Results",
                    "Wins",
                    "Losses",
                    "Draws",
                    "In Progress"
                });
                resultFilterDropdown.onValueChanged.AddListener(OnResultFilterChanged);
            }

            // Date filter dropdown
            if (dateFilterDropdown)
            {
                dateFilterDropdown.ClearOptions();
                dateFilterDropdown.AddOptions(new List<string>
                {
                    "All Time",
                    "Today",
                    "Last 7 Days",
                    "Last 30 Days",
                    "This Month"
                });
                dateFilterDropdown.onValueChanged.AddListener(OnDateFilterChanged);
            }
        }

        private void SubscribeToEvents()
        {
            if (HistoryManager.Instance != null)
            {
                HistoryManager.Instance.OnEntryAdded += OnEntryAdded;
                HistoryManager.Instance.OnEntryUpdated += OnEntryUpdated;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (HistoryManager.Instance != null)
            {
                HistoryManager.Instance.OnEntryAdded -= OnEntryAdded;
                HistoryManager.Instance.OnEntryUpdated -= OnEntryUpdated;
            }
        }

        private void CheckNavigationParams()
        {
            var navParams = SceneNavigator.Instance?.ConsumeParams();
            if (navParams != null)
            {
                // Check if we should show a specific entry
                if (!string.IsNullOrEmpty(navParams.ItemId))
                {
                    var entry = HistoryManager.Instance?.GetEntry(navParams.ItemId);
                    if (entry != null)
                    {
                        ShowEntryDetail(entry);
                    }
                }

                // Check if we should show a specific tab
                if (!string.IsNullOrEmpty(navParams.TargetTab))
                {
                    if (Enum.TryParse<HistoryTab>(navParams.TargetTab, out HistoryTab tab))
                    {
                        ShowTab(tab);
                    }
                }
            }
        }

        // ==================== TAB MANAGEMENT ====================

        private void ShowTab(HistoryTab tab)
        {
            currentTab = tab;

            // Update filter based on tab
            switch (tab)
            {
                case HistoryTab.All:
                    currentFilter.typeFilter = null;
                    break;
                case HistoryTab.Matches:
                    currentFilter.typeFilter = HistoryEntryType.Match1v1;
                    break;
                case HistoryTab.Tournaments:
                    currentFilter.typeFilter = HistoryEntryType.Tournament;
                    break;
            }

            // Update tab visuals
            UpdateTabColors();

            // Reload entries
            LoadEntries(reset: true);
        }

        private void UpdateTabColors()
        {
            UpdateTabButton(allTabButton, currentTab == HistoryTab.All);
            UpdateTabButton(matchesTabButton, currentTab == HistoryTab.Matches);
            UpdateTabButton(tournamentsTabButton, currentTab == HistoryTab.Tournaments);
        }

        private void UpdateTabButton(Button button, bool isActive)
        {
            if (button == null) return;

            var image = button.GetComponent<Image>();
            if (image) image.DOColor(isActive ? activeTabColor : inactiveTabColor, 0.2f);

            var text = button.GetComponentInChildren<TextMeshProUGUI>();
            if (text) text.DOColor(isActive ? Color.white : new Color(0.7f, 0.7f, 0.7f), 0.2f);

            // Scale animation for active tab
            button.transform.DOScale(isActive ? 1.05f : 1f, 0.2f).SetEase(Ease.OutCubic);
        }

        // ==================== FILTERS ====================

        private void OnResultFilterChanged(int index)
        {
            switch (index)
            {
                case 0: // All
                    currentFilter.resultFilter = null;
                    break;
                case 1: // Wins
                    currentFilter.resultFilter = MatchResult.Win;
                    break;
                case 2: // Losses
                    currentFilter.resultFilter = MatchResult.Loss;
                    break;
                case 3: // Draws
                    currentFilter.resultFilter = MatchResult.Draw;
                    break;
                case 4: // Pending
                    currentFilter.resultFilter = MatchResult.Pending;
                    break;
            }

            LoadEntries(reset: true);
        }

        private void OnDateFilterChanged(int index)
        {
            DateTime now = DateTime.UtcNow;

            switch (index)
            {
                case 0: // All time
                    currentFilter.fromDate = null;
                    break;
                case 1: // Today
                    currentFilter.fromDate = now.Date;
                    break;
                case 2: // Last 7 days
                    currentFilter.fromDate = now.AddDays(-7);
                    break;
                case 3: // Last 30 days
                    currentFilter.fromDate = now.AddDays(-30);
                    break;
                case 4: // This month
                    currentFilter.fromDate = new DateTime(now.Year, now.Month, 1);
                    break;
            }

            LoadEntries(reset: true);
        }

        private void ClearFilters()
        {
            currentFilter = new HistoryFilter();

            // Keep tab filter
            if (currentTab == HistoryTab.Matches)
                currentFilter.typeFilter = HistoryEntryType.Match1v1;
            else if (currentTab == HistoryTab.Tournaments)
                currentFilter.typeFilter = HistoryEntryType.Tournament;

            // Reset dropdowns
            if (resultFilterDropdown) resultFilterDropdown.value = 0;
            if (dateFilterDropdown) dateFilterDropdown.value = 0;

            LoadEntries(reset: true);
        }

        // ==================== LOAD ENTRIES ====================

        private void LoadEntries(bool reset)
        {
            if (reset)
            {
                currentPage = 0;
                ClearEntries();
            }

            if (HistoryManager.Instance == null) return;

            ShowLoading(true);

            var entries = HistoryManager.Instance.GetEntries(
                currentFilter,
                entriesPerPage,
                currentPage * entriesPerPage
            );

            ShowLoading(false);

            // Show empty state if needed
            bool isEmpty = entries.Count == 0 && currentPage == 0;
            if (emptyStateText)
            {
                emptyStateText.gameObject.SetActive(isEmpty);
                emptyStateText.text = GetEmptyStateText();

                if (isEmpty)
                {
                    // Animated fade-in for empty state
                    var cg = emptyStateText.GetComponent<CanvasGroup>();
                    if (cg == null) cg = emptyStateText.gameObject.AddComponent<CanvasGroup>();
                    cg.alpha = 0f;
                    cg.DOFade(1f, 0.4f).SetEase(Ease.OutQuad);
                }
            }

            // Populate entries
            PopulateEntries(entries);

            // Show/hide load more button
            if (loadMoreButton)
            {
                loadMoreButton.gameObject.SetActive(entries.Count >= entriesPerPage);
            }
        }

        private string GetEmptyStateText()
        {
            switch (currentTab)
            {
                case HistoryTab.Matches:
                    return "No matches recorded.\nPlay your first 1v1 match!";
                case HistoryTab.Tournaments:
                    return "You haven't joined any tournaments.\nJoin your first tournament!";
                default:
                    return "No history yet.\nStart playing to see your progress!";
            }
        }

        private void PopulateEntries(List<HistoryEntry> entries)
        {
            if (entriesContainer == null || historyEntryPrefab == null) return;

            foreach (var entry in entries)
            {
                var entryObj = Instantiate(historyEntryPrefab, entriesContainer);
                spawnedEntries.Add(entryObj);

                var entryUI = entryObj.GetComponent<HistoryEntryItemUI>();
                if (entryUI)
                {
                    entryUI.Setup(entry, OnEntryClicked, OnOpponentClicked);
                }
            }

            // Animate history entries entrance
            AnimateHistoryEntriesEntrance();
        }

        /// <summary>
        /// Animates the history entries entrance with a staggered effect
        /// </summary>
        private void AnimateHistoryEntriesEntrance()
        {
            if (entriesContainer == null || entriesContainer.childCount == 0) return;

            var seq = DOTween.Sequence();
            for (int i = 0; i < entriesContainer.childCount; i++)
            {
                var child = entriesContainer.GetChild(i);
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

        private void ClearEntries()
        {
            foreach (var obj in spawnedEntries)
            {
                if (obj) Destroy(obj);
            }
            spawnedEntries.Clear();
        }

        private void OnLoadMoreClicked()
        {
            currentPage++;
            LoadEntries(reset: false);
        }

        // ==================== DETAIL PANEL ====================

        private void OnEntryClicked(HistoryEntry entry)
        {
            ShowEntryDetail(entry);
        }

        private void ShowEntryDetail(HistoryEntry entry)
        {
            if (detailPanel == null) return;

            selectedEntry = entry;
            detailPanel.SetActive(true);
            AnimatePanelIn(detailPanel.transform);

            // Populate detail info
            if (detailTitleText)
                detailTitleText.text = entry.GetDisplayTitle();

            if (detailSubtitleText)
                detailSubtitleText.text = entry.GetDisplaySubtitle();

            if (detailResultText)
            {
                detailResultText.text = entry.GetResultText();
                detailResultText.color = entry.GetResultColor();
            }

            if (detailScoreText)
            {
                if (entry.type == HistoryEntryType.Tournament)
                {
                    detailScoreText.text = AutoLocalizer.Get("cash_position", entry.myPosition, entry.totalParticipants);
                }
                else
                {
                    detailScoreText.text = $"{entry.myScore:F0} - {entry.opponentScore:F0}";
                }
            }

            if (detailEntryFeeText)
                detailEntryFeeText.text = AutoLocalizer.Get("cash_entry_fee", entry.entryFee);

            if (detailPrizeText)
                detailPrizeText.text = AutoLocalizer.Get("cash_prize_amount", entry.prize);

            if (detailNetText)
            {
                detailNetText.text = entry.GetFormattedNetResult();
                detailNetText.color = entry.netResult >= 0
                    ? new Color(0f, 1f, 0.5f)
                    : new Color(1f, 0.4f, 0.4f);
            }

            if (detailDateText)
                detailDateText.text = entry.timestamp.ToString("dd/MM/yyyy HH:mm");

            if (detailDurationText)
                detailDurationText.text = entry.GetFormattedDuration();
        }

        private void CloseDetailPanel()
        {
            if (detailPanel)
            {
                AnimatePanelOut(detailPanel.transform, () => detailPanel.SetActive(false));
            }
            selectedEntry = null;
        }

        // ==================== EVENT HANDLERS ====================

        private void OnEntryAdded(HistoryEntry entry)
        {
            // Refresh if matches current filter
            if (currentFilter.Matches(entry))
            {
                LoadEntries(reset: true);
            }
        }

        private void OnEntryUpdated(HistoryEntry entry)
        {
            LoadEntries(reset: true);

            // Update detail panel if showing this entry
            if (selectedEntry != null && selectedEntry.entryId == entry.entryId)
            {
                ShowEntryDetail(entry);
            }
        }

        private void OnOpponentClicked(string opponentId)
        {
            if (string.IsNullOrEmpty(opponentId)) return;

            // Navigate to normal Profile (not CashProfile - privacy rule)
            PlayerPrefs.SetString("ViewProfileId", opponentId);
            PlayerPrefs.SetString("ProfileReturnScene", "CashHistory");
            SceneManager.LoadScene("Profile");
        }

        // ==================== LOADING ====================

        private void ShowLoading(bool show)
        {
            if (loadingIndicator)
            {
                if (show)
                {
                    loadingIndicator.SetActive(true);
                    var cg = loadingIndicator.GetComponent<CanvasGroup>();
                    if (cg == null) cg = loadingIndicator.AddComponent<CanvasGroup>();
                    cg.alpha = 0f;
                    cg.DOFade(1f, 0.2f).SetUpdate(true);
                }
                else
                {
                    var cg = loadingIndicator.GetComponent<CanvasGroup>();
                    if (cg != null)
                        cg.DOFade(0f, 0.2f).SetUpdate(true).OnComplete(() => loadingIndicator.SetActive(false));
                    else
                        loadingIndicator.SetActive(false);
                }
            }
        }

        // ==================== PANEL ANIMATIONS ====================

        private void AnimatePanelIn(Transform t)
        {
            t.localScale = Vector3.one * 0.85f;
            var cg = t.GetComponent<CanvasGroup>();
            if (cg == null) cg = t.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            DOTween.Sequence()
                .Join(t.DOScale(1f, 0.3f).SetEase(Ease.OutBack))
                .Join(cg.DOFade(1f, 0.25f))
                .SetUpdate(true);
        }

        private void AnimatePanelOut(Transform t, System.Action onComplete)
        {
            var cg = t.GetComponent<CanvasGroup>();
            if (cg == null) { t.gameObject.SetActive(false); onComplete?.Invoke(); return; }
            DOTween.Sequence()
                .Join(t.DOScale(0.9f, 0.2f).SetEase(Ease.InQuad))
                .Join(cg.DOFade(0f, 0.2f))
                .OnComplete(() => { t.localScale = Vector3.one; cg.alpha = 1f; onComplete?.Invoke(); })
                .SetUpdate(true);
        }

        // ==================== NAVIGATION ====================

        private void OnBackClicked()
        {
            // Close detail panel first if open
            if (detailPanel != null && detailPanel.activeSelf)
            {
                CloseDetailPanel();
                return;
            }

            SceneNavigator.Instance?.GoBack();
        }

        // ==================== PUBLIC API ====================

        /// <summary>
        /// Opens history to a specific tab
        /// </summary>
        public void OpenTab(string tabName)
        {
            if (Enum.TryParse<HistoryTab>(tabName, out HistoryTab tab))
            {
                ShowTab(tab);
            }
        }

        /// <summary>
        /// Shows detail for a specific entry by ID
        /// </summary>
        public void ShowEntryById(string entryId)
        {
            var entry = HistoryManager.Instance?.GetEntry(entryId);
            if (entry != null)
            {
                ShowEntryDetail(entry);
            }
        }

        /// <summary>
        /// Refreshes all data
        /// </summary>
        public void RefreshAll()
        {
            LoadEntries(reset: true);
        }
    }
}
