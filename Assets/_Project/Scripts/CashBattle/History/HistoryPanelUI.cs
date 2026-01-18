using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DigitPark.CashBattle
{
    /// <summary>
    /// Panel visual del Historial para Cash Battle.
    /// Muestra partidas pasadas, estadísticas y filtros.
    /// Puede usarse como panel inline o como prefab instanciable.
    /// </summary>
    public class HistoryPanelUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Header")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Button _closeButton;

        [Header("Stats Section")]
        [SerializeField] private GameObject _statsPanel;
        [SerializeField] private TextMeshProUGUI _totalMatchesText;
        [SerializeField] private TextMeshProUGUI _winRateText;
        [SerializeField] private TextMeshProUGUI _netProfitText;
        [SerializeField] private TextMeshProUGUI _currentStreakText;

        [Header("Detailed Stats")]
        [SerializeField] private TextMeshProUGUI _winsText;
        [SerializeField] private TextMeshProUGUI _lossesText;
        [SerializeField] private TextMeshProUGUI _drawsText;
        [SerializeField] private TextMeshProUGUI _totalEarningsText;
        [SerializeField] private TextMeshProUGUI _totalSpentText;
        [SerializeField] private TextMeshProUGUI _tournamentsPlayedText;
        [SerializeField] private TextMeshProUGUI _tournamentWinsText;

        [Header("Filter Tabs")]
        [SerializeField] private Button _allTabButton;
        [SerializeField] private Button _matchesTabButton;
        [SerializeField] private Button _tournamentsTabButton;
        [SerializeField] private Image _allTabIndicator;
        [SerializeField] private Image _matchesTabIndicator;
        [SerializeField] private Image _tournamentsTabIndicator;

        [Header("History List")]
        [SerializeField] private ScrollRect _historyScrollRect;
        [SerializeField] private Transform _historyContainer;
        [SerializeField] private GameObject _historyItemPrefab;
        [SerializeField] private TextMeshProUGUI _noHistoryText;

        [Header("Load More")]
        [SerializeField] private Button _loadMoreButton;
        [SerializeField] private TextMeshProUGUI _loadMoreText;

        [Header("Colors")]
        [SerializeField] private Color _activeTabColor = new Color(0f, 0.83f, 1f, 1f);
        [SerializeField] private Color _inactiveTabColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        [SerializeField] private Color _positiveColor = new Color(0f, 1f, 0.5f, 1f);
        [SerializeField] private Color _negativeColor = new Color(1f, 0.4f, 0.4f, 1f);
        [SerializeField] private Color _neutralColor = new Color(1f, 0.84f, 0f, 1f);

        [Header("Animation")]
        [SerializeField] private float _fadeInDuration = 0.3f;

        // Estado
        private HistoryFilterTab _currentTab = HistoryFilterTab.All;
        private List<GameObject> _historyItemInstances = new List<GameObject>();
        private int _currentPage = 0;
        private const int ITEMS_PER_PAGE = 20;

        // Eventos
        public event Action OnCloseRequested;
        public event Action<HistoryEntry> OnEntryClicked;

        private void Awake()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            SetupListeners();
            SubscribeToHistoryEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromHistoryEvents();
        }

        private void OnEnable()
        {
            RefreshAll();
        }

        // ==================== SETUP ====================

        private void SetupListeners()
        {
            // Close
            _closeButton?.onClick.AddListener(OnCloseClicked);

            // Tabs
            _allTabButton?.onClick.AddListener(() => SwitchToTab(HistoryFilterTab.All));
            _matchesTabButton?.onClick.AddListener(() => SwitchToTab(HistoryFilterTab.Matches));
            _tournamentsTabButton?.onClick.AddListener(() => SwitchToTab(HistoryFilterTab.Tournaments));

            // Load more
            _loadMoreButton?.onClick.AddListener(LoadMoreEntries);
        }

        private void SubscribeToHistoryEvents()
        {
            if (HistoryManager.Instance != null)
            {
                HistoryManager.Instance.OnEntryAdded += OnHistoryEntryAdded;
                HistoryManager.Instance.OnEntryUpdated += OnHistoryEntryUpdated;
                HistoryManager.Instance.OnStatsUpdated += OnStatsUpdated;
            }
        }

        private void UnsubscribeFromHistoryEvents()
        {
            if (HistoryManager.Instance != null)
            {
                HistoryManager.Instance.OnEntryAdded -= OnHistoryEntryAdded;
                HistoryManager.Instance.OnEntryUpdated -= OnHistoryEntryUpdated;
                HistoryManager.Instance.OnStatsUpdated -= OnStatsUpdated;
            }
        }

        // ==================== PUBLIC METHODS ====================

        /// <summary>
        /// Muestra el panel con animación
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            RefreshAll();
            StartCoroutine(FadeIn());
            Debug.Log("[HistoryPanelUI] Panel mostrado");
        }

        /// <summary>
        /// Oculta el panel con animación
        /// </summary>
        public void Hide()
        {
            StartCoroutine(FadeOutAndHide());
        }

        /// <summary>
        /// Refresca toda la UI
        /// </summary>
        public void RefreshAll()
        {
            RefreshStats();
            RefreshHistoryList();
        }

        // ==================== STATS ====================

        private void RefreshStats()
        {
            if (HistoryManager.Instance == null) return;

            var stats = HistoryManager.Instance.GetStats();
            var streak = HistoryManager.Instance.GetCurrentStreak();

            // Stats principales
            if (_totalMatchesText != null)
                _totalMatchesText.text = stats.totalMatches.ToString();

            if (_winRateText != null)
            {
                _winRateText.text = $"{stats.winRate:F1}%";
                _winRateText.color = stats.winRate >= 50 ? _positiveColor : _negativeColor;
            }

            if (_netProfitText != null)
            {
                _netProfitText.text = $"${stats.netProfit:F2}";
                _netProfitText.color = stats.netProfit >= 0 ? _positiveColor : _negativeColor;
            }

            if (_currentStreakText != null)
            {
                if (streak.streak > 0)
                {
                    _currentStreakText.text = streak.isWinStreak ? $"{streak.streak}W" : $"{streak.streak}L";
                    _currentStreakText.color = streak.isWinStreak ? _positiveColor : _negativeColor;
                }
                else
                {
                    _currentStreakText.text = "-";
                    _currentStreakText.color = _neutralColor;
                }
            }

            // Stats detallados
            if (_winsText != null)
                _winsText.text = stats.wins.ToString();

            if (_lossesText != null)
                _lossesText.text = stats.losses.ToString();

            if (_drawsText != null)
                _drawsText.text = stats.draws.ToString();

            if (_totalEarningsText != null)
            {
                _totalEarningsText.text = $"${stats.totalEarnings:F2}";
                _totalEarningsText.color = _positiveColor;
            }

            if (_totalSpentText != null)
            {
                _totalSpentText.text = $"${stats.totalSpent:F2}";
                _totalSpentText.color = _negativeColor;
            }

            if (_tournamentsPlayedText != null)
                _tournamentsPlayedText.text = stats.tournamentsPlayed.ToString();

            if (_tournamentWinsText != null)
                _tournamentWinsText.text = stats.tournamentWins.ToString();
        }

        // ==================== TAB NAVIGATION ====================

        private void SwitchToTab(HistoryFilterTab tab)
        {
            _currentTab = tab;
            _currentPage = 0;

            // Actualizar indicadores
            SetTabIndicator(_allTabIndicator, tab == HistoryFilterTab.All);
            SetTabIndicator(_matchesTabIndicator, tab == HistoryFilterTab.Matches);
            SetTabIndicator(_tournamentsTabIndicator, tab == HistoryFilterTab.Tournaments);

            // Refrescar lista
            RefreshHistoryList();

            Debug.Log($"[HistoryPanelUI] Tab cambiado a: {tab}");
        }

        private void SetTabIndicator(Image indicator, bool active)
        {
            if (indicator != null)
            {
                indicator.color = active ? _activeTabColor : _inactiveTabColor;
                indicator.gameObject.SetActive(active);
            }
        }

        // ==================== HISTORY LIST ====================

        private void RefreshHistoryList()
        {
            // Limpiar items existentes
            ClearHistoryItems();

            if (HistoryManager.Instance == null) return;

            // Obtener entradas según filtro
            var filter = GetCurrentFilter();
            var entries = HistoryManager.Instance.GetEntries(filter, ITEMS_PER_PAGE, 0);

            if (entries.Count == 0)
            {
                ShowNoHistoryMessage();
                return;
            }

            HideNoHistoryMessage();

            // Crear items
            foreach (var entry in entries)
            {
                CreateHistoryItem(entry);
            }

            // Mostrar/ocultar botón de cargar más
            UpdateLoadMoreButton(entries.Count);
        }

        private void LoadMoreEntries()
        {
            if (HistoryManager.Instance == null) return;

            _currentPage++;
            int offset = _currentPage * ITEMS_PER_PAGE;

            var filter = GetCurrentFilter();
            var entries = HistoryManager.Instance.GetEntries(filter, ITEMS_PER_PAGE, offset);

            foreach (var entry in entries)
            {
                CreateHistoryItem(entry);
            }

            UpdateLoadMoreButton(entries.Count);
        }

        private HistoryFilter GetCurrentFilter()
        {
            var filter = new HistoryFilter();

            switch (_currentTab)
            {
                case HistoryFilterTab.Matches:
                    // Solo 1v1 y Cognitive Sprint
                    // Como no podemos filtrar múltiples tipos directamente,
                    // excluimos torneos en el código
                    break;
                case HistoryFilterTab.Tournaments:
                    filter.typeFilter = HistoryEntryType.Tournament;
                    break;
            }

            return filter;
        }

        private void CreateHistoryItem(HistoryEntry entry)
        {
            // Filtrar por tab si es necesario
            if (_currentTab == HistoryFilterTab.Matches &&
                (entry.type == HistoryEntryType.Tournament || entry.type == HistoryEntryType.TournamentPrize))
            {
                return;
            }

            GameObject itemObj;

            if (_historyItemPrefab != null)
            {
                itemObj = Instantiate(_historyItemPrefab, _historyContainer);
            }
            else
            {
                itemObj = CreateHistoryItemFallback(entry);
            }

            _historyItemInstances.Add(itemObj);

            var itemUI = itemObj.GetComponent<HistoryEntryItemUI>();
            if (itemUI != null)
            {
                itemUI.Setup(entry, OnHistoryItemClicked);
            }
        }

        private GameObject CreateHistoryItemFallback(HistoryEntry entry)
        {
            GameObject obj = new GameObject($"HistoryItem_{entry.entryId.Substring(0, 8)}");
            obj.transform.SetParent(_historyContainer, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 80);

            LayoutElement le = obj.AddComponent<LayoutElement>();
            le.preferredHeight = 80;
            le.flexibleWidth = 1;

            Image bg = obj.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.12f, 0.15f, 0.95f);

            Button btn = obj.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(() => OnHistoryItemClicked(entry));

            // Layout horizontal
            HorizontalLayoutGroup hlg = obj.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(15, 15, 10, 10);
            hlg.spacing = 15;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;

            // Result indicator
            GameObject resultInd = new GameObject("ResultIndicator");
            resultInd.transform.SetParent(obj.transform, false);
            Image resultImg = resultInd.AddComponent<Image>();
            resultImg.color = entry.GetResultColor();
            LayoutElement resultLE = resultInd.AddComponent<LayoutElement>();
            resultLE.preferredWidth = 5;
            resultLE.preferredHeight = 60;

            // Info section
            GameObject infoSection = new GameObject("Info");
            infoSection.transform.SetParent(obj.transform, false);
            infoSection.AddComponent<RectTransform>();
            VerticalLayoutGroup infoVlg = infoSection.AddComponent<VerticalLayoutGroup>();
            infoVlg.spacing = 3;
            infoVlg.childForceExpandHeight = false;
            LayoutElement infoLE = infoSection.AddComponent<LayoutElement>();
            infoLE.flexibleWidth = 1;

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(infoSection.transform, false);
            TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.text = entry.GetDisplayTitle();
            titleTmp.fontSize = 14;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = Color.white;

            // Subtitle
            GameObject subObj = new GameObject("Subtitle");
            subObj.transform.SetParent(infoSection.transform, false);
            TextMeshProUGUI subTmp = subObj.AddComponent<TextMeshProUGUI>();
            subTmp.text = entry.GetDisplaySubtitle();
            subTmp.fontSize = 12;
            subTmp.color = new Color(0.6f, 0.6f, 0.65f, 1f);

            // Date
            GameObject dateObj = new GameObject("Date");
            dateObj.transform.SetParent(infoSection.transform, false);
            TextMeshProUGUI dateTmp = dateObj.AddComponent<TextMeshProUGUI>();
            dateTmp.text = entry.GetFormattedDate();
            dateTmp.fontSize = 10;
            dateTmp.color = new Color(0.5f, 0.5f, 0.5f, 1f);

            // Result section
            GameObject resultSection = new GameObject("Result");
            resultSection.transform.SetParent(obj.transform, false);
            resultSection.AddComponent<RectTransform>();
            VerticalLayoutGroup resVlg = resultSection.AddComponent<VerticalLayoutGroup>();
            resVlg.spacing = 2;
            resVlg.childAlignment = TextAnchor.MiddleRight;
            LayoutElement resLE = resultSection.AddComponent<LayoutElement>();
            resLE.preferredWidth = 90;

            // Result text
            GameObject resTextObj = new GameObject("ResultText");
            resTextObj.transform.SetParent(resultSection.transform, false);
            TextMeshProUGUI resTmp = resTextObj.AddComponent<TextMeshProUGUI>();
            resTmp.text = entry.GetResultText();
            resTmp.fontSize = 12;
            resTmp.fontStyle = FontStyles.Bold;
            resTmp.color = entry.GetResultColor();
            resTmp.alignment = TextAlignmentOptions.Right;

            // Net result
            GameObject netObj = new GameObject("NetResult");
            netObj.transform.SetParent(resultSection.transform, false);
            TextMeshProUGUI netTmp = netObj.AddComponent<TextMeshProUGUI>();
            netTmp.text = entry.GetFormattedNetResult();
            netTmp.fontSize = 14;
            netTmp.fontStyle = FontStyles.Bold;
            netTmp.color = entry.netResult >= 0 ? _positiveColor : _negativeColor;
            netTmp.alignment = TextAlignmentOptions.Right;

            return obj;
        }

        private void ClearHistoryItems()
        {
            foreach (var instance in _historyItemInstances)
            {
                if (instance != null) Destroy(instance);
            }
            _historyItemInstances.Clear();
        }

        private void ShowNoHistoryMessage()
        {
            if (_noHistoryText != null)
            {
                _noHistoryText.gameObject.SetActive(true);
                _noHistoryText.text = GetNoHistoryMessage();
            }

            if (_loadMoreButton != null)
                _loadMoreButton.gameObject.SetActive(false);
        }

        private void HideNoHistoryMessage()
        {
            if (_noHistoryText != null)
                _noHistoryText.gameObject.SetActive(false);
        }

        private string GetNoHistoryMessage()
        {
            switch (_currentTab)
            {
                case HistoryFilterTab.Matches:
                    return "No hay partidas registradas";
                case HistoryFilterTab.Tournaments:
                    return "No hay torneos registrados";
                default:
                    return "No hay historial";
            }
        }

        private void UpdateLoadMoreButton(int loadedCount)
        {
            if (_loadMoreButton == null) return;

            bool hasMore = loadedCount >= ITEMS_PER_PAGE;
            _loadMoreButton.gameObject.SetActive(hasMore);

            if (_loadMoreText != null)
                _loadMoreText.text = "Cargar más...";
        }

        // ==================== EVENTS ====================

        private void OnHistoryEntryAdded(HistoryEntry entry)
        {
            // Refrescar si el panel está activo
            if (gameObject.activeInHierarchy)
            {
                RefreshAll();
            }
        }

        private void OnHistoryEntryUpdated(HistoryEntry entry)
        {
            if (gameObject.activeInHierarchy)
            {
                RefreshAll();
            }
        }

        private void OnStatsUpdated(HistoryStats stats)
        {
            if (gameObject.activeInHierarchy)
            {
                RefreshStats();
            }
        }

        private void OnHistoryItemClicked(HistoryEntry entry)
        {
            Debug.Log($"[HistoryPanelUI] Entry clicked: {entry.GetDisplayTitle()}");
            OnEntryClicked?.Invoke(entry);

            // TODO: Mostrar detalle de la partida
        }

        // ==================== ANIMATION ====================

        private IEnumerator FadeIn()
        {
            if (_canvasGroup == null) yield break;

            _canvasGroup.alpha = 0f;
            float elapsed = 0f;

            while (elapsed < _fadeInDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / _fadeInDuration);
                yield return null;
            }

            _canvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOutAndHide()
        {
            if (_canvasGroup == null)
            {
                gameObject.SetActive(false);
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < _fadeInDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / _fadeInDuration);
                yield return null;
            }

            _canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        // ==================== CLOSE ====================

        private void OnCloseClicked()
        {
            OnCloseRequested?.Invoke();
            Hide();
        }

        // ==================== EDITOR HELPERS ====================

#if UNITY_EDITOR
        public void AutoSetupReferences()
        {
            _canvasGroup = GetComponent<CanvasGroup>();

            // Header
            _titleText = transform.Find("Header/Title")?.GetComponent<TextMeshProUGUI>();
            _closeButton = transform.Find("Header/CloseButton")?.GetComponent<Button>();

            // Stats
            _statsPanel = transform.Find("StatsPanel")?.gameObject;
            _totalMatchesText = transform.Find("StatsPanel/TotalMatches/Value")?.GetComponent<TextMeshProUGUI>();
            _winRateText = transform.Find("StatsPanel/WinRate/Value")?.GetComponent<TextMeshProUGUI>();
            _netProfitText = transform.Find("StatsPanel/NetProfit/Value")?.GetComponent<TextMeshProUGUI>();

            // Tabs
            Transform tabsPanel = transform.Find("TabsPanel");
            if (tabsPanel != null)
            {
                _allTabButton = tabsPanel.Find("AllTab")?.GetComponent<Button>();
                _matchesTabButton = tabsPanel.Find("MatchesTab")?.GetComponent<Button>();
                _tournamentsTabButton = tabsPanel.Find("TournamentsTab")?.GetComponent<Button>();
            }

            // History list
            _historyScrollRect = transform.Find("HistoryScrollView")?.GetComponent<ScrollRect>();
            _historyContainer = transform.Find("HistoryScrollView/Viewport/Content");
            _noHistoryText = transform.Find("HistoryScrollView/NoHistoryText")?.GetComponent<TextMeshProUGUI>();
            _loadMoreButton = transform.Find("LoadMoreButton")?.GetComponent<Button>();

            Debug.Log("[HistoryPanelUI] Referencias configuradas automáticamente");
        }
#endif
    }

    /// <summary>
    /// Tabs de filtro para el historial
    /// </summary>
    public enum HistoryFilterTab
    {
        All,
        Matches,
        Tournaments
    }
}
