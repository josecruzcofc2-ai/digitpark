using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using DigitPark.Data;
using DigitPark.Localization;
using DG.Tweening;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager para la escena dedicada de Historial de Partidas (generales, no cash).
    /// Muestra lista de partidas con filtro por juego.
    /// </summary>
    public class MatchHistorySceneManager : MonoBehaviour
    {
        [Header("UI - Header")]
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI totalCountText;

        [Header("UI - Game Filters")]
        [SerializeField] private Button filterAllButton;
        [SerializeField] private Button filterDigitRushButton;
        [SerializeField] private Button filterMemoryPairsButton;
        [SerializeField] private Button filterQuickMathButton;
        [SerializeField] private Button filterFlashTapButton;
        [SerializeField] private Button filterOddOneOutButton;
        [SerializeField] private Button filterCognitiveSprintButton;

        [Header("UI - Content")]
        [SerializeField] private Transform scrollContent;
        [SerializeField] private GameObject matchEntryPrefab;
        [SerializeField] private TextMeshProUGUI emptyText;
        [SerializeField] private GameObject loadingIndicator;
        [SerializeField] private Button loadMoreButton;

        [Header("UI - Sections (for animations)")]
        [SerializeField] private RectTransform headerTransform;
        [SerializeField] private RectTransform gameFiltersTransform;
        [SerializeField] private RectTransform scrollViewTransform;

        private List<GameObject> currentItems = new List<GameObject>();
        private string currentFilter = null; // null = todos
        private int currentOffset = 0;
        private const int PAGE_SIZE = 20;
        private string returnScene = "Profile";

        // Colores por juego
        private static readonly Dictionary<string, Color> GAME_COLORS = new Dictionary<string, Color>
        {
            { "DigitRush", new Color(0f, 1f, 1f, 1f) },          // Cyan
            { "MemoryPairs", new Color(0.6f, 0.3f, 1f, 1f) },    // Purple
            { "QuickMath", new Color(1f, 0.5f, 0f, 1f) },        // Orange
            { "FlashTap", new Color(0.2f, 0.9f, 0.4f, 1f) },     // Green
            { "OddOneOut", new Color(1f, 0.3f, 0.3f, 1f) },       // Red
            { "CognitiveSprint", new Color(1f, 0.84f, 0f, 1f) }   // Gold
        };

        // Filter buttons y sus game types
        private Dictionary<Button, string> filterButtons;
        private Button activeFilterButton;

        #region Unity Lifecycle

        private void Start()
        {
            Debug.Log("[MatchHistory] MatchHistorySceneManager iniciado");

            returnScene = PlayerPrefs.GetString("MatchHistoryReturnScene", "Profile");
            PlayerPrefs.DeleteKey("MatchHistoryReturnScene");

            SetupFilterButtons();
            SetupListeners();
            AnimateEntrance();
            ApplyFilter(null); // Mostrar todos
        }

        private void SetupFilterButtons()
        {
            filterButtons = new Dictionary<Button, string>();
            if (filterAllButton != null) filterButtons[filterAllButton] = null;
            if (filterDigitRushButton != null) filterButtons[filterDigitRushButton] = "DigitRush";
            if (filterMemoryPairsButton != null) filterButtons[filterMemoryPairsButton] = "MemoryPairs";
            if (filterQuickMathButton != null) filterButtons[filterQuickMathButton] = "QuickMath";
            if (filterFlashTapButton != null) filterButtons[filterFlashTapButton] = "FlashTap";
            if (filterOddOneOutButton != null) filterButtons[filterOddOneOutButton] = "OddOneOut";
            if (filterCognitiveSprintButton != null) filterButtons[filterCognitiveSprintButton] = "CognitiveSprint";
        }

        private void SetupListeners()
        {
            // Disable auto-navigation from BackButton prefab to prevent double listener
            var autoNav = backButton?.GetComponent<DigitPark.UI.BackButton>();
            if (autoNav != null) autoNav.DisableAutoNavigation();
            backButton?.onClick.AddListener(OnBackClicked);
            loadMoreButton?.onClick.AddListener(OnLoadMoreClicked);

            // Filter buttons
            filterAllButton?.onClick.AddListener(() => ApplyFilter(null));
            filterDigitRushButton?.onClick.AddListener(() => ApplyFilter("DigitRush"));
            filterMemoryPairsButton?.onClick.AddListener(() => ApplyFilter("MemoryPairs"));
            filterQuickMathButton?.onClick.AddListener(() => ApplyFilter("QuickMath"));
            filterFlashTapButton?.onClick.AddListener(() => ApplyFilter("FlashTap"));
            filterOddOneOutButton?.onClick.AddListener(() => ApplyFilter("OddOneOut"));
            filterCognitiveSprintButton?.onClick.AddListener(() => ApplyFilter("CognitiveSprint"));
        }

        #endregion

        #region Filters

        private void ApplyFilter(string gameType)
        {
            currentFilter = gameType;
            currentOffset = 0;

            // Update filter button visuals
            foreach (var kvp in filterButtons)
            {
                if (kvp.Key == null) continue;
                bool isActive = kvp.Value == currentFilter;
                UpdateFilterButtonVisual(kvp.Key, isActive, kvp.Value);
            }

            LoadEntries(true);
        }

        private void UpdateFilterButtonVisual(Button btn, bool active, string gameType)
        {
            var img = btn.GetComponent<Image>();
            if (img == null) return;

            if (active)
            {
                // Activo: fondo del color del juego, texto oscuro
                Color gameColor = gameType != null && GAME_COLORS.ContainsKey(gameType)
                    ? GAME_COLORS[gameType]
                    : new Color(0f, 1f, 1f, 1f); // Cyan para "Todos"
                img.color = gameColor;

                var text = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                    text.color = new Color(0.05f, 0.05f, 0.08f, 1f);
            }
            else
            {
                // Inactivo: fondo oscuro, texto gris
                img.color = new Color(0.12f, 0.14f, 0.18f, 1f);

                var text = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                    text.color = new Color(0.5f, 0.5f, 0.55f, 1f);
            }
        }

        #endregion

        #region Load Entries

        private void LoadEntries(bool clearExisting)
        {
            if (clearExisting)
            {
                ClearItems();
                currentOffset = 0;
            }

            ShowLoadingIndicator(true);
            if (emptyText != null)
                emptyText.gameObject.SetActive(false);

            var entries = MatchHistoryStorage.Instance.GetEntries(currentFilter, PAGE_SIZE, currentOffset);
            int totalCount = MatchHistoryStorage.Instance.GetTotalCount(currentFilter);

            ShowLoadingIndicator(false);

            UpdateHeader(totalCount);

            if (currentOffset == 0 && (entries == null || entries.Count == 0))
            {
                if (emptyText != null)
                {
                    emptyText.text = currentFilter != null
                        ? $"No hay partidas de {GetGameDisplayName(currentFilter)}"
                        : "No has jugado partidas aun\nJuega para ver tu historial";
                    emptyText.gameObject.SetActive(true);

                    // Animated fade-in for empty state
                    var cg = emptyText.GetComponent<CanvasGroup>();
                    if (cg == null) cg = emptyText.gameObject.AddComponent<CanvasGroup>();
                    cg.alpha = 0f;
                    cg.DOFade(1f, 0.4f).SetEase(Ease.OutQuad);
                }
                if (loadMoreButton != null)
                    loadMoreButton.gameObject.SetActive(false);
                return;
            }

            foreach (var entry in entries)
            {
                CreateMatchEntryItem(entry);
            }

            currentOffset += entries.Count;

            // Show/hide load more
            if (loadMoreButton != null)
                loadMoreButton.gameObject.SetActive(currentOffset < totalCount);
        }

        private void CreateMatchEntryItem(MatchHistoryEntry entry)
        {
            if (matchEntryPrefab == null || scrollContent == null) return;

            GameObject item = Instantiate(matchEntryPrefab, scrollContent);
            currentItems.Add(item);

            // Mover antes de LoadMore button
            if (loadMoreButton != null && loadMoreButton.transform.parent == scrollContent)
                loadMoreButton.transform.SetAsLastSibling();

            SetupMatchEntryItem(item, entry);

            // Animacion de entrada staggered
            int index = currentItems.Count - 1;
            item.transform.localScale = Vector3.zero;
            item.transform.DOScale(1f, 0.3f)
                .SetDelay(index * 0.05f)
                .SetEase(Ease.OutBack);
        }

        private void SetupMatchEntryItem(GameObject item, MatchHistoryEntry entry)
        {
            // Game color bar (left accent)
            var colorBar = item.transform.Find("ColorBar")?.GetComponent<Image>();
            if (colorBar != null)
            {
                Color barColor = GAME_COLORS.ContainsKey(entry.gameType)
                    ? GAME_COLORS[entry.gameType]
                    : new Color(0.5f, 0.5f, 0.5f, 1f);
                colorBar.color = barColor;
            }

            // Game name
            var gameNameText = item.transform.Find("InfoSection/GameName")?.GetComponent<TextMeshProUGUI>();
            if (gameNameText != null)
                gameNameText.text = GetGameDisplayName(entry.gameType);

            // Result badge
            var resultText = item.transform.Find("InfoSection/ResultBadge")?.GetComponent<TextMeshProUGUI>();
            if (resultText != null)
            {
                resultText.text = entry.GetResultText();
                resultText.color = entry.GetResultColor();
            }

            // Subtitle (opponent or practice)
            var subtitleText = item.transform.Find("InfoSection/Subtitle")?.GetComponent<TextMeshProUGUI>();
            if (subtitleText != null)
                subtitleText.text = entry.GetSubtitle();

            // Detail (time + errors)
            var detailText = item.transform.Find("InfoSection/DetailText")?.GetComponent<TextMeshProUGUI>();
            if (detailText != null)
                detailText.text = entry.GetDetailText();

            // Date
            var dateText = item.transform.Find("DateText")?.GetComponent<TextMeshProUGUI>();
            if (dateText != null)
                dateText.text = entry.GetFormattedDate();
        }

        private void ClearItems()
        {
            foreach (var item in currentItems)
            {
                if (item != null) Destroy(item);
            }
            currentItems.Clear();
        }

        private void UpdateHeader(int totalCount)
        {
            if (totalCountText != null)
                totalCountText.text = AutoLocalizer.Get("history_match_count", totalCount);

            if (titleText != null)
                titleText.text = AutoLocalizer.Get("history_title");
        }

        #endregion

        #region Button Callbacks

        private void OnBackClicked()
        {
            Debug.Log($"[MatchHistory] Volviendo a: {returnScene}");
            SceneManager.LoadScene(returnScene);
        }

        private void OnLoadMoreClicked()
        {
            LoadEntries(false);
        }

        #endregion

        #region Animations

        private void AnimateEntrance()
        {
            // Header slide desde arriba
            if (headerTransform != null)
            {
                Vector2 pos = headerTransform.anchoredPosition;
                headerTransform.anchoredPosition = new Vector2(pos.x, pos.y + 200);
                headerTransform.DOAnchorPos(pos, 0.4f).SetEase(Ease.OutBack);
            }

            // Game Filters fade + slide desde izquierda
            if (gameFiltersTransform != null)
            {
                var cg = gameFiltersTransform.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                Vector2 pos = gameFiltersTransform.anchoredPosition;
                gameFiltersTransform.anchoredPosition = new Vector2(pos.x - 100, pos.y);
                DOTween.Sequence()
                    .AppendInterval(0.15f)
                    .Append(gameFiltersTransform.DOAnchorPos(pos, 0.35f).SetEase(Ease.OutCubic))
                    .Join(cg.DOFade(1f, 0.35f));
            }

            // ScrollView fade in
            if (scrollViewTransform != null)
            {
                var cg = scrollViewTransform.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                DOTween.Sequence()
                    .AppendInterval(0.25f)
                    .Append(cg.DOFade(1f, 0.4f));
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

        #endregion

        #region Helpers

        private string GetGameDisplayName(string gameType)
        {
            return gameType switch
            {
                "DigitRush" => "Digit Rush",
                "MemoryPairs" => "Memory Pairs",
                "QuickMath" => "Quick Math",
                "FlashTap" => "Flash Tap",
                "OddOneOut" => "Odd One Out",
                "CognitiveSprint" => "Cognitive Sprint",
                _ => gameType ?? AutoLocalizer.Get("game_type_unknown")
            };
        }

        #endregion
    }
}
