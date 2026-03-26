using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DigitPark.Services.Firebase;
using DigitPark.Data;
using DigitPark.UI;
using DigitPark.Localization;
using DigitPark.UI.Items;
using DG.Tweening;
using DigitPark.Animations;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager de la escena de Scores/Rankings
    /// Muestra tablas de clasificación por juego: Nacional (país) y Mundial (global)
    /// Con selector de juego (5 juegos)
    /// </summary>
    public class LeaderboardManager : MonoBehaviour
    {
        [Header("Game Selector")]
        [SerializeField] public Button[] gameButtons;
        [SerializeField] public Image[] gameButtonBackgrounds;
        [SerializeField] public Image[] gameButtonIcons;

        [Header("Tabs")]
        [SerializeField] public Button nacionalTab;
        [SerializeField] public Button mundialTab;

        [Header("Leaderboard UI")]
        [SerializeField] public Transform leaderboardContainer;
        [SerializeField] public GameObject leaderboardEntryPrefab;
        [SerializeField] public ScrollRect scrollRect;

        [Header("Loading")]
        [SerializeField] public GameObject loadingPanel;
        [SerializeField] public TextMeshProUGUI loadingText;

        [Header("Navigation")]
        [SerializeField] public Button backButton;

        [Header("Empty State")]
        [SerializeField] private GameObject emptyState;
        [SerializeField] private Button playButton;

        [Header("Player Position Panel")]
        [SerializeField] private GameObject playerPositionPanel;
        [SerializeField] private TextMeshProUGUI positionNumberText;
        [SerializeField] private TextMeshProUGUI positionTimeText;

        // Juegos disponibles para ranking
        private static readonly string[] GAME_IDS = {
            "DigitRush", "FlashTap", "MemoryPairs", "OddOneOut", "QuickMath"
        };

        // Estado
        private LeaderboardTab currentTab = LeaderboardTab.Local;
        private string currentGameId = "DigitRush";
        private int currentGameIndex = 0;
        private PlayerData currentPlayer;

        // Datos
        private List<LeaderboardEntry> localScores;
        private List<LeaderboardEntry> globalScores;

        private async void Start()
        {
            try
            {
                Debug.Log("[Leaderboard] LeaderboardManager iniciado");

                EnsureServicesExist();
                SetupListeners();
                await LoadPlayerDataAsync();

                // Seleccionar DigitRush por defecto y cargar
                SelectGame(0);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void EnsureServicesExist()
        {
            if (AuthenticationService.Instance == null)
            {
                Debug.LogError("[Leaderboard] AuthenticationService not found — was BootManager skipped? Leaderboard will use offline data.");
            }

            if (DatabaseService.Instance == null)
            {
                Debug.LogError("[Leaderboard] DatabaseService not found — was BootManager skipped? Leaderboard will use offline data.");
            }
        }

        #region Initialization

        private async System.Threading.Tasks.Task LoadPlayerDataAsync()
        {
            if (AuthenticationService.Instance != null)
            {
                currentPlayer = AuthenticationService.Instance.GetCurrentPlayerData();

                if (currentPlayer == null)
                {
                    Debug.LogError("[Leaderboard] No hay datos del jugador");
                    SceneManager.LoadScene("Login");
                    return;
                }

                Debug.Log($"[Leaderboard] Datos del jugador: {currentPlayer.username}");

                if (DatabaseService.Instance != null)
                {
                    var freshData = await DatabaseService.Instance.LoadPlayerData(currentPlayer.userId);
                    if (freshData != null)
                    {
                        currentPlayer = freshData;
                        AuthenticationService.Instance.UpdateCurrentPlayerData(currentPlayer);
                    }
                }
            }
        }

        private void SetupListeners()
        {
            // Game selector buttons
            if (gameButtons != null)
            {
                for (int i = 0; i < gameButtons.Length && i < GAME_IDS.Length; i++)
                {
                    if (gameButtons[i] != null)
                    {
                        int index = i;
                        gameButtons[i].onClick.RemoveAllListeners();
                        gameButtons[i].onClick.AddListener(() => OnGameSelected(index));
                    }
                }
            }

            // Tabs
            if (nacionalTab != null)
            {
                nacionalTab.onClick.RemoveAllListeners();
                nacionalTab.onClick.AddListener(() => OnTabSelected(LeaderboardTab.Local));
                nacionalTab.interactable = true;
            }

            if (mundialTab != null)
            {
                mundialTab.onClick.RemoveAllListeners();
                mundialTab.onClick.AddListener(() => OnTabSelected(LeaderboardTab.Global));
                mundialTab.interactable = true;
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(OnBackButtonClicked);
                backButton.interactable = true;
            }

            if (playButton != null)
            {
                playButton.onClick.RemoveAllListeners();
                playButton.onClick.AddListener(OnPlayButtonClicked);
            }
        }

        private void OnPlayButtonClicked()
        {
            Debug.Log("[Leaderboard] Navegando a GameSelector");
            SceneManager.LoadScene("GameSelector");
        }

        #endregion

        #region Game Selector

        private void OnGameSelected(int index)
        {
            if (index < 0 || index >= GAME_IDS.Length) return;
            if (index == currentGameIndex) return;

            Debug.Log($"[Leaderboard] Juego seleccionado: {GAME_IDS[index]}");
            SelectGame(index);
        }

        private void SelectGame(int index)
        {
            currentGameIndex = index;
            currentGameId = GAME_IDS[index];
            UpdateGameSelectorVisuals();
            UpdateTabVisuals();
            _ = LoadLeaderboardAsync(currentTab);
        }

        private void UpdateGameSelectorVisuals()
        {
            Color selectedBg = new Color(0f, 0.83f, 1f, 0.3f);
            Color normalBg = new Color(0.08f, 0.12f, 0.18f, 0.9f);
            Color selectedOutline = new Color(0f, 1f, 1f, 1f);
            Color normalOutline = new Color(0.3f, 0.3f, 0.4f, 0.3f);

            if (gameButtonBackgrounds != null)
            {
                for (int i = 0; i < gameButtonBackgrounds.Length && i < GAME_IDS.Length; i++)
                {
                    if (gameButtonBackgrounds[i] != null)
                    {
                        bool selected = i == currentGameIndex;
                        gameButtonBackgrounds[i].color = selected ? selectedBg : normalBg;

                        // Update outline
                        Outline outline = gameButtonBackgrounds[i].GetComponent<Outline>();
                        if (outline != null)
                        {
                            outline.effectColor = selected ? selectedOutline : normalOutline;
                        }
                    }
                }
            }

            // Update icon tints
            if (gameButtonIcons != null)
            {
                for (int i = 0; i < gameButtonIcons.Length && i < GAME_IDS.Length; i++)
                {
                    if (gameButtonIcons[i] != null)
                    {
                        gameButtonIcons[i].color = i == currentGameIndex
                            ? Color.white
                            : new Color(0.6f, 0.6f, 0.7f, 1f);
                    }
                }
            }

            // Update label colors
            if (gameButtons != null)
            {
                for (int i = 0; i < gameButtons.Length && i < GAME_IDS.Length; i++)
                {
                    if (gameButtons[i] == null) continue;
                    Transform labelT = gameButtons[i].transform.Find("Label");
                    if (labelT != null)
                    {
                        var labelTMP = labelT.GetComponent<TextMeshProUGUI>();
                        if (labelTMP != null)
                        {
                            labelTMP.color = i == currentGameIndex
                                ? Color.white
                                : new Color(0.5f, 0.5f, 0.6f, 1f);
                        }
                    }
                }
            }
        }

        #endregion

        #region Tab Navigation

        private void OnTabSelected(LeaderboardTab tab)
        {
            Debug.Log($"[Leaderboard] OnTabSelected - Tab: {tab}");

            if (currentTab == tab) return;

            currentTab = tab;
            UpdateTabVisuals();
            _ = LoadLeaderboardAsync(tab);
        }

        private void UpdateTabVisuals()
        {
            SetTabButtonState(nacionalTab, currentTab == LeaderboardTab.Local);
            SetTabButtonState(mundialTab, currentTab == LeaderboardTab.Global);
        }

        private void SetTabButtonState(Button button, bool isSelected)
        {
            if (button == null) return;

            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.DOColor(isSelected
                    ? new Color(0f, 0.83f, 1f)
                    : new Color(0.15f, 0.2f, 0.25f), 0.2f).SetLink(button.gameObject);
            }

            var text = button.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.DOColor(isSelected
                    ? new Color(0.02f, 0.05f, 0.1f)
                    : Color.white, 0.2f).SetLink(text.gameObject);
            }

            // Scale animation for active tab
            button.transform.DOScale(isSelected ? 1.05f : 1f, 0.2f).SetEase(Ease.OutCubic).SetLink(button.gameObject);
        }

        #endregion

        #region Load Leaderboards

        private async Task LoadLeaderboardAsync(LeaderboardTab tab)
        {
            ShowLoading(true, AutoLocalizer.Get("loading_rankings"));
            ClearLeaderboard();

            List<LeaderboardEntry> entries = null;

            try
            {
                switch (tab)
                {
                    case LeaderboardTab.Local:
                        entries = await LoadLocalScores();
                        break;

                    case LeaderboardTab.Global:
                        entries = await LoadGlobalScores();
                        break;
                }

                if (entries != null && entries.Count > 0)
                {
                    DisplayLeaderboard(entries);
                }
                else
                {
                    ShowEmptyMessage();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Leaderboard] Error al cargar: {ex.Message}");
                ShowErrorMessage(AutoLocalizer.Get("error_loading_rankings"));
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async System.Threading.Tasks.Task<List<LeaderboardEntry>> LoadLocalScores()
        {
            Debug.Log($"[Leaderboard] Cargando puntuaciones nacionales para {currentGameId}...");

            if (currentPlayer == null) return new List<LeaderboardEntry>();

            localScores = await DatabaseService.Instance.GetCountryLeaderboard(
                currentPlayer.countryCode, currentGameId, 100);

            for (int i = 0; i < localScores.Count; i++)
            {
                localScores[i].position = i + 1;
            }

            return localScores;
        }

        private async System.Threading.Tasks.Task<List<LeaderboardEntry>> LoadGlobalScores()
        {
            Debug.Log($"[Leaderboard] Cargando puntuaciones mundiales para {currentGameId}...");

            globalScores = await DatabaseService.Instance.GetGlobalLeaderboard(currentGameId, 200);

            for (int i = 0; i < globalScores.Count; i++)
            {
                globalScores[i].position = i + 1;
            }

            return globalScores;
        }

        #endregion

        #region Display Leaderboard

        private void DisplayLeaderboard(List<LeaderboardEntry> entries)
        {
            Debug.Log($"[Leaderboard] Mostrando {entries.Count} entradas para {currentGameId}");

            HideEmptyState();

            RectTransform containerRT = leaderboardContainer as RectTransform;
            if (containerRT != null)
            {
                containerRT.anchorMin = new Vector2(0, 1);
                containerRT.anchorMax = new Vector2(1, 1);
                containerRT.pivot = new Vector2(0.5f, 1);
                containerRT.anchoredPosition = Vector2.zero;
                containerRT.gameObject.SetActive(true);

                if (containerRT.sizeDelta.x <= 0 && scrollRect != null && scrollRect.viewport != null)
                {
                    float viewportWidth = scrollRect.viewport.rect.width;
                    containerRT.sizeDelta = new Vector2(viewportWidth, containerRT.sizeDelta.y);
                }
            }

            var layoutGroup = leaderboardContainer.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = leaderboardContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            }

            layoutGroup.spacing = 6f;
            layoutGroup.padding = new RectOffset(0, 0, 8, 8);
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;

            foreach (var entry in entries.Take(50))
            {
                CreateLeaderboardEntry(entry);
            }

            AnimateLeaderboardEntrance();
            HighlightPlayerPosition(entries);
            StartCoroutine(ForceLayoutRebuild(containerRT));
        }

        private System.Collections.IEnumerator ForceLayoutRebuild(RectTransform containerRT)
        {
            if (containerRT == null) yield break;

            LayoutRebuilder.ForceRebuildLayoutImmediate(containerRT);
            Canvas.ForceUpdateCanvases();

            yield return null;

            if (containerRT == null) yield break;
            LayoutRebuilder.ForceRebuildLayoutImmediate(containerRT);

            yield return null;

            if (scrollRect != null && scrollRect.content != null)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        private void AnimateLeaderboardEntrance()
        {
            if (leaderboardContainer == null || leaderboardContainer.childCount == 0) return;

            int count = Mathf.Min(leaderboardContainer.childCount, 30);
            Transform[] items = new Transform[count];
            for (int i = 0; i < count; i++)
            {
                items[i] = leaderboardContainer.GetChild(i);
            }

            UIAnimations.StaggeredEntrance(items, 0.04f, UIAnimations.DURATION_NORMAL);
        }

        private void CreateLeaderboardEntry(LeaderboardEntry entry)
        {
            if (leaderboardContainer == null) return;

            bool isCurrentPlayer = currentPlayer != null && entry.userId == currentPlayer.userId;

            if (leaderboardEntryPrefab != null)
            {
                GameObject entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
                entryObj.name = $"Entry_{entry.position}";

                LeaderboardEntryUI entryUI = entryObj.GetComponent<LeaderboardEntryUI>();
                if (entryUI != null)
                {
                    entryUI.Setup(entry, isCurrentPlayer);
                }
                else
                {
                    Debug.LogWarning("[Leaderboard] Prefab no tiene LeaderboardEntryUI, usando fallback");
                    Destroy(entryObj);
                    CreateLeaderboardEntryFallback(entry, isCurrentPlayer);
                }

                entryObj.SetActive(true);
            }
            else
            {
                CreateLeaderboardEntryFallback(entry, isCurrentPlayer);
            }
        }

        /// <summary>
        /// Fallback: Crea entrada por codigo (Pos + Username + Tiempo)
        /// </summary>
        private void CreateLeaderboardEntryFallback(LeaderboardEntry entry, bool isCurrentPlayer)
        {
            GameObject entryObj = new GameObject($"Entry_{entry.position}");
            entryObj.transform.SetParent(leaderboardContainer, false);

            RectTransform entryRT = entryObj.AddComponent<RectTransform>();
            entryRT.anchorMin = new Vector2(0, 1);
            entryRT.anchorMax = new Vector2(1, 1);
            entryRT.pivot = new Vector2(0.5f, 1);
            entryRT.anchoredPosition = Vector2.zero;
            entryRT.sizeDelta = new Vector2(0, 70);

            var layoutElement = entryObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 70;
            layoutElement.minHeight = 70;

            // Fondo alternante, cyan highlight si es jugador actual
            Image bg = entryObj.AddComponent<Image>();
            if (isCurrentPlayer)
            {
                bg.color = new Color(0f, 0.83f, 1f, 0.25f);
            }
            else
            {
                bg.color = entry.position % 2 == 0
                    ? new Color(0.04f, 0.08f, 0.13f, 0.95f)
                    : new Color(0.06f, 0.1f, 0.16f, 0.95f);
            }

            // Top 3 borde de medalla
            if (entry.position <= 3)
            {
                Outline medalOutline = entryObj.AddComponent<Outline>();
                Color medalColor = GetMedalColor(entry.position);
                medalOutline.effectColor = new Color(medalColor.r, medalColor.g, medalColor.b, 0.4f);
                medalOutline.effectDistance = new Vector2(1, 1);
            }

            // HorizontalLayoutGroup
            HorizontalLayoutGroup hlg = entryObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.padding = new RectOffset(15, 15, 8, 8);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            // 1. Posición (55px)
            GameObject posObj = new GameObject("PositionText");
            posObj.transform.SetParent(entryObj.transform, false);
            posObj.AddComponent<RectTransform>();
            TextMeshProUGUI posText = posObj.AddComponent<TextMeshProUGUI>();
            posText.text = entry.position <= 3 ? $"{entry.position}" : $"#{entry.position}";
            posText.fontSize = entry.position <= 3 ? FontSizes.Body : FontSizes.Body;
            posText.enableAutoSizing = true;
            posText.fontSizeMin = FontSizes.AutoMinBody;
            posText.fontSizeMax = posText.fontSize;
            posText.color = entry.position <= 3 ? GetMedalColor(entry.position) : new Color(0.5f, 0.55f, 0.65f);
            posText.fontStyle = FontStyles.Bold;
            posText.alignment = TextAlignmentOptions.Center;
            posText.raycastTarget = false;
            LayoutElement posLE = posObj.AddComponent<LayoutElement>();
            posLE.minWidth = 55;
            posLE.preferredWidth = 55;

            // 2. Username (flex, expanded +50px — avatar removed)
            GameObject nameObj = new GameObject("UsernameText");
            nameObj.transform.SetParent(entryObj.transform, false);
            nameObj.AddComponent<RectTransform>();
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = UICanvasHelper.TmpSafe(entry.username);
            nameText.fontSize = FontSizes.Body;
            nameText.enableAutoSizing = true;
            nameText.fontSizeMin = FontSizes.AutoMinBody;
            nameText.fontSizeMax = nameText.fontSize;
            nameText.color = isCurrentPlayer ? new Color(0f, 1f, 1f) : Color.white;
            nameText.fontStyle = FontStyles.Bold;
            nameText.alignment = TextAlignmentOptions.MidlineLeft;
            nameText.enableWordWrapping = false;
            nameText.overflowMode = TextOverflowModes.Ellipsis;
            nameText.raycastTarget = false;
            LayoutElement nameLE = nameObj.AddComponent<LayoutElement>();
            nameLE.flexibleWidth = 1;
            nameLE.minWidth = 120;

            // 4. Tiempo (100px)
            GameObject timeObj = new GameObject("TimeText");
            timeObj.transform.SetParent(entryObj.transform, false);
            timeObj.AddComponent<RectTransform>();
            TextMeshProUGUI timeText = timeObj.AddComponent<TextMeshProUGUI>();
            timeText.text = $"{entry.time:F3}s";
            timeText.fontSize = FontSizes.Body;
            timeText.enableAutoSizing = true;
            timeText.fontSizeMin = FontSizes.AutoMinBody;
            timeText.fontSizeMax = timeText.fontSize;
            timeText.color = new Color(0f, 1f, 0.53f);
            timeText.fontStyle = FontStyles.Bold;
            timeText.alignment = TextAlignmentOptions.MidlineRight;
            timeText.raycastTarget = false;
            LayoutElement timeLE = timeObj.AddComponent<LayoutElement>();
            timeLE.minWidth = 100;
            timeLE.preferredWidth = 100;

            // Agregar componente UI
            LeaderboardEntryUI entryUI = entryObj.AddComponent<LeaderboardEntryUI>();
            entryUI.AutoSetupReferences();

            entryObj.SetActive(true);
        }

        private Color GetMedalColor(int position)
        {
            switch (position)
            {
                case 1: return new Color(1f, 0.84f, 0f);
                case 2: return new Color(0.75f, 0.75f, 0.75f);
                case 3: return new Color(0.8f, 0.5f, 0.2f);
                default: return Color.white;
            }
        }

        private void HighlightPlayerPosition(List<LeaderboardEntry> entries)
        {
            if (currentPlayer == null) return;

            var playerEntry = entries.Find(e => e.userId == currentPlayer.userId);
            if (playerEntry != null)
            {
                Debug.Log($"[Leaderboard] Jugador encontrado en posición #{playerEntry.position}");
                UpdatePlayerPositionPanel(playerEntry.position, playerEntry.time);
            }
            else
            {
                UpdatePlayerPositionPanel(-1, 0);
            }
        }

        private void UpdatePlayerPositionPanel(int position, float time)
        {
            if (playerPositionPanel == null) return;

            playerPositionPanel.SetActive(true);
            AnimatePanelIn(playerPositionPanel.transform);

            if (positionNumberText != null)
            {
                if (position > 0)
                {
                    positionNumberText.text = $"#{position}";
                    positionNumberText.color = new Color(1f, 0.84f, 0f);
                }
                else
                {
                    positionNumberText.text = AutoLocalizer.Get("not_ranked");
                    positionNumberText.color = new Color(0.5f, 0.5f, 0.5f);
                }
            }

            if (positionTimeText != null)
            {
                if (time > 0)
                {
                    positionTimeText.text = $"{time:F3}s";
                }
                else
                {
                    positionTimeText.text = "--";
                }
            }
        }

        private void ClearLeaderboard()
        {
            if (leaderboardContainer == null) return;

            foreach (Transform child in leaderboardContainer)
            {
                Destroy(child.gameObject);
            }
        }

        #endregion

        #region UI Helpers

        private void ShowLoading(bool show, string message = null)
        {
            if (loadingPanel != null)
            {
                if (show)
                {
                    loadingPanel.SetActive(true);
                    var cg = loadingPanel.GetComponent<CanvasGroup>();
                    if (cg == null) cg = loadingPanel.AddComponent<CanvasGroup>();
                    cg.alpha = 0f;
                    cg.DOFade(1f, 0.25f).SetUpdate(true).SetLink(loadingPanel);
                }
                else
                {
                    var cg = loadingPanel.GetComponent<CanvasGroup>();
                    if (cg != null)
                    {
                        cg.DOFade(0f, 0.2f).SetUpdate(true).SetLink(loadingPanel).OnComplete(() =>
                        {
                            loadingPanel.SetActive(false);
                            cg.alpha = 1f;
                        });
                    }
                    else
                    {
                        loadingPanel.SetActive(false);
                    }
                }
            }

            if (loadingText != null && show)
            {
                loadingText.text = message ?? AutoLocalizer.Get("loading");
            }
        }

        private void ShowEmptyMessage()
        {
            Debug.Log("[Leaderboard] No hay datos para mostrar");

            if (emptyState != null)
            {
                emptyState.SetActive(true);
                AnimatePanelIn(emptyState.transform);
            }

            if (playerPositionPanel != null)
            {
                AnimatePanelOut(playerPositionPanel.transform, () => playerPositionPanel.SetActive(false));
            }
        }

        private void HideEmptyState()
        {
            if (emptyState != null)
            {
                AnimatePanelOut(emptyState.transform, () => emptyState.SetActive(false));
            }

            if (playerPositionPanel != null)
            {
                playerPositionPanel.SetActive(true);
                AnimatePanelIn(playerPositionPanel.transform);
            }
        }

        private void ShowErrorMessage(string message)
        {
            Debug.LogError($"[Leaderboard] {message}");

            GameObject errorMsg = new GameObject("ErrorMessage");
            errorMsg.transform.SetParent(leaderboardContainer, false);

            RectTransform rt = errorMsg.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(800, 100);

            TextMeshProUGUI text = errorMsg.AddComponent<TextMeshProUGUI>();
            if (TMP_Settings.defaultFontAsset != null)
                text.font = TMP_Settings.defaultFontAsset;
            text.text = message;
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = FontSizes.Body;
            text.enableAutoSizing = true;
            text.fontSizeMin = FontSizes.AutoMinBody;
            text.fontSizeMax = text.fontSize;
            text.color = Color.red;
        }

        #endregion

        #region Navigation

        private void OnBackButtonClicked()
        {
            Debug.Log("[Leaderboard] Volviendo al menú principal");
            SceneManager.LoadScene("MainMenu");
        }

        #endregion

        #region Panel Animations

        private void AnimatePanelIn(Transform t)
        {
            t.localScale = Vector3.one * 0.85f;
            var cg = t.GetComponent<CanvasGroup>();
            if (cg == null) cg = t.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            DOTween.Sequence()
                .Join(t.DOScale(1f, 0.3f).SetEase(Ease.OutBack))
                .Join(cg.DOFade(1f, 0.25f))
                .SetUpdate(true)
                .SetLink(t.gameObject);
        }

        private void AnimatePanelOut(Transform t, System.Action onComplete)
        {
            var cg = t.GetComponent<CanvasGroup>();
            if (cg == null) { t.gameObject.SetActive(false); onComplete?.Invoke(); return; }
            DOTween.Sequence()
                .Join(t.DOScale(0.9f, 0.2f).SetEase(Ease.InQuad))
                .Join(cg.DOFade(0f, 0.2f))
                .OnComplete(() => { if (t != null) t.localScale = Vector3.one; if (cg != null) cg.alpha = 1f; onComplete?.Invoke(); })
                .SetUpdate(true)
                .SetLink(t.gameObject);
        }

        #endregion

        private void OnDestroy()
        {
            transform.DOKill();
        }
    }

    /// <summary>
    /// Tipos de tab del leaderboard
    /// </summary>
    public enum LeaderboardTab
    {
        Local,
        Global
    }
}
