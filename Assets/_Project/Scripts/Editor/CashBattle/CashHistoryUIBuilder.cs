using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;

namespace DigitPark.Editor
{
    /// <summary>
    /// UI Builder para la escena CashHistory.unity
    /// Construye el historial de partidas y estadísticas del jugador.
    /// MEJORADO: Con iconos de juego, filtros, cards rediseñadas.
    /// Incluye Reference Assigner integrado.
    /// </summary>
    public class CashHistoryUIBuilder : EditorWindow
    {
        #region Colors

        private static readonly Color GOLD_PRIMARY = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GOLD_DARK = new Color(0.85f, 0.65f, 0.13f, 1f);
        private static readonly Color GOLD_LIGHT = new Color(1f, 0.93f, 0.55f, 1f);

        private static readonly Color BG_DARK = new Color(0.08f, 0.06f, 0.12f, 1f);
        private static readonly Color CARD_BG = new Color(0.12f, 0.1f, 0.15f, 0.95f);
        private static readonly Color CARD_BG_LIGHT = new Color(0.15f, 0.13f, 0.18f, 0.95f);
        private static readonly Color CARD_BORDER = new Color(0.85f, 0.65f, 0.13f, 0.6f);

        private static readonly Color TEXT_PRIMARY = new Color(1f, 1f, 1f, 1f);
        private static readonly Color TEXT_GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.6f, 0.65f, 1f);

        private static readonly Color SUCCESS_GREEN = new Color(0.3f, 1f, 0.5f, 1f);
        private static readonly Color ERROR_RED = new Color(1f, 0.4f, 0.4f, 1f);
        private static readonly Color CYAN_ACCENT = new Color(0f, 0.9f, 1f, 1f);

        private static readonly Color FILTER_ACTIVE = new Color(0.85f, 0.65f, 0.13f, 1f);
        private static readonly Color FILTER_INACTIVE = new Color(0.2f, 0.2f, 0.25f, 1f);

        // Paths to icons
        private static readonly string GAME_ICONS_PATH = "Assets/_Project/Art/Icons/Games/";
        private static readonly string STAT_ICONS_PATH = "Assets/_Project/Art/Icons/CashBattle/Stats/";

        // BackButton prefab
        private const string BACK_BUTTON_GOLD_PREFAB = "Assets/_Project/Prefabs/Common/BackButtonGold.prefab";

        #endregion

        // Reference Assigner state
        private Vector2 scrollPosition;
        private static int assignedCount = 0;
        private static int failedCount = 0;
        private static int alreadySetCount = 0;
        private static List<AssignResult> assignResults = new List<AssignResult>();

        private struct AssignResult
        {
            public string fieldName;
            public string status;
            public bool success;
            public Object assignedObject;
        }

        [MenuItem("DigitPark/UI Builders/CashBattle/Cash History", false, 253)]
        public static void ShowWindow()
        {
            GetWindow<CashHistoryUIBuilder>("Cash History Builder");
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // ========== BUILD SECTION ==========
            GUILayout.Label("Cash History UI Builder", EditorStyles.boldLabel);
            GUILayout.Label("Historial de partidas con iconos y filtros", EditorStyles.miniLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Construye la UI MEJORADA para CashHistory.unity:\n\n" +
                "- Header con título (sin balance)\n" +
                "- Stats con iconos visuales\n" +
                "- Filtros: Todas | Victorias | Derrotas\n" +
                "- Cards rediseñadas con:\n" +
                "  * Icono del juego\n" +
                "  * Barra de color inferior\n" +
                "  * Info de entrada y ganancia\n" +
                "  * Modo (1v1/Torneo)",
                MessageType.Info);

            EditorGUILayout.Space(10);

            GUI.backgroundColor = GOLD_PRIMARY;
            if (GUILayout.Button("CONSTRUIR UI COMPLETA", GUILayout.Height(40)))
            {
                BuildCashHistoryUI();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(15);
            GUILayout.Label("Construcción por Secciones:", EditorStyles.boldLabel);

            if (GUILayout.Button("Solo Stats Panel", GUILayout.Height(28)))
            {
                BuildStatsPanelOnly();
            }

            if (GUILayout.Button("Solo Filters", GUILayout.Height(28)))
            {
                BuildFiltersOnly();
            }

            if (GUILayout.Button("Solo Match Item Prefab", GUILayout.Height(28)))
            {
                BuildMatchItemPrefab();
            }

            // ========== SEPARADOR ==========
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // ========== REFERENCE ASSIGNER ==========
            GUILayout.Label("Asignar Referencias", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "CashHistory")
            {
                EditorGUILayout.HelpBox($"Escena actual: {currentScene}\nAbre CashHistory primero.", MessageType.Warning);
            }

            MonoBehaviour targetController = FindCashHistoryController();
            if (targetController != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Controller:", GUILayout.Width(70));
                EditorGUILayout.ObjectField(targetController, typeof(MonoBehaviour), true);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("CashHistorySceneController no encontrado.", MessageType.Warning);
            }

            EditorGUILayout.Space(5);

            GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
            if (GUILayout.Button("ASIGNAR TODAS LAS REFERENCIAS", GUILayout.Height(36)))
            {
                ResetAssignState();
                RunAssignAllReferences();
                Repaint();
            }
            GUI.backgroundColor = Color.white;

            DrawAssignResults();

            EditorGUILayout.EndScrollView();
        }

        #region Build Methods

        private static void BuildCashHistoryUI()
        {
            Canvas canvas = FindMainCanvas();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontró Canvas. Abre la escena CashHistory primero.", "OK");
                return;
            }

            if (EditorUtility.DisplayDialog("Reconstruir UI?",
                "Esto reconstruirá la UI de Cash History con el nuevo diseño.\n\n¿Continuar?",
                "Sí, Construir", "Cancelar"))
            {
                BuildAllElements(canvas);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                Debug.Log("[CashHistoryUIBuilder] UI construida exitosamente!");
            }
        }

        private static void BuildStatsPanelOnly()
        {
            Canvas canvas = FindMainCanvas();
            if (canvas == null) return;

            Transform safeArea = canvas.transform.Find("SafeArea");
            if (safeArea == null)
            {
                Debug.LogError("SafeArea no encontrado. Construye la UI completa primero.");
                return;
            }

            Transform old = safeArea.Find("StatsPanel");
            if (old != null) DestroyImmediate(old.gameObject);

            CreateStatsPanel(safeArea);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private static void BuildFiltersOnly()
        {
            Canvas canvas = FindMainCanvas();
            if (canvas == null) return;

            Transform safeArea = canvas.transform.Find("SafeArea");
            if (safeArea == null)
            {
                Debug.LogError("SafeArea no encontrado. Construye la UI completa primero.");
                return;
            }

            Transform old = safeArea.Find("FiltersPanel");
            if (old != null) DestroyImmediate(old.gameObject);

            CreateFiltersPanel(safeArea);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private static void BuildMatchItemPrefab()
        {
            Canvas canvas = FindMainCanvas();
            if (canvas == null) return;

            GameObject prefab = CreateMatchHistoryItem(canvas.transform, "QuickMath", "@Opponent", true, 5f, 10f, "Hoy, 14:30", "5-3", "1v1");
            Selection.activeGameObject = prefab;
            Debug.Log("[CashHistoryUIBuilder] Match Item creado. Guárdalo como prefab.");
        }

        private static void BuildAllElements(Canvas canvas)
        {
            Transform canvasTransform = canvas.transform;

            CleanupOldElements(canvasTransform);

            CreateBackground(canvasTransform);
            GameObject safeArea = CreateSafeArea(canvasTransform);
            CreateHeader(safeArea.transform);
            CreateStatsPanel(safeArea.transform);
            CreateFiltersPanel(safeArea.transform);
            CreateMatchHistoryList(safeArea.transform);

            // Hidden elements needed by controller
            CreateEmptyStateText(safeArea.transform);
            CreateLoadMoreButton(safeArea.transform);
            CreateLoadingIndicator(safeArea.transform);
            CreateDetailPanel(canvasTransform);
        }

        private static void CleanupOldElements(Transform parent)
        {
            string[] toDestroy = { "Background", "SafeArea", "Header", "StatsPanel", "FiltersPanel",
                "MatchHistoryList", "EmptyStateText", "LoadMoreButton", "LoadingIndicator", "DetailPanel" };
            foreach (string name in toDestroy)
            {
                Transform existing = parent.Find(name);
                if (existing != null) DestroyImmediate(existing.gameObject);
            }
            // Also check inside SafeArea for cleanup
            Transform safeArea = parent.Find("SafeArea");
            if (safeArea != null)
            {
                foreach (string name in new[] { "EmptyStateText", "LoadMoreButton", "LoadingIndicator" })
                {
                    Transform existing = safeArea.Find(name);
                    if (existing != null) DestroyImmediate(existing.gameObject);
                }
            }
        }

        #endregion

        #region Background

        private static void CreateBackground(Transform parent)
        {
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(parent, false);
            bg.transform.SetAsFirstSibling();

            RectTransform rt = bg.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image img = bg.AddComponent<Image>();
            img.color = BG_DARK;
        }

        #endregion

        #region Safe Area

        private static GameObject CreateSafeArea(Transform parent)
        {
            GameObject safeArea = new GameObject("SafeArea");
            safeArea.transform.SetParent(parent, false);

            RectTransform rt = safeArea.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            return safeArea;
        }

        #endregion

        #region Header

        private static void CreateHeader(Transform parent)
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(parent, false);

            RectTransform rt = header.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(0, 100);

            Image bg = header.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.4f);

            // Back button - try prefab first, fallback to manual creation
            GameObject backBtnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BACK_BUTTON_GOLD_PREFAB);
            if (backBtnPrefab != null)
            {
                GameObject backBtn = (GameObject)PrefabUtility.InstantiatePrefab(backBtnPrefab, header.transform);
                backBtn.name = "BackButton";
                RectTransform backRT = backBtn.GetComponent<RectTransform>();
                backRT.anchorMin = new Vector2(0, 0.5f);
                backRT.anchorMax = new Vector2(0, 0.5f);
                backRT.sizeDelta = new Vector2(70, 70);
                backRT.anchoredPosition = new Vector2(45, 0);
            }
            else
            {
                // Fallback: manual back button creation
                Debug.LogWarning("[CashHistory] BackButtonGold prefab not found, using fallback");

                GameObject backBtn = new GameObject("BackButton");
                backBtn.transform.SetParent(header.transform, false);

                RectTransform backRT = backBtn.AddComponent<RectTransform>();
                backRT.anchorMin = new Vector2(0, 0.5f);
                backRT.anchorMax = new Vector2(0, 0.5f);
                backRT.sizeDelta = new Vector2(70, 70);
                backRT.anchoredPosition = new Vector2(45, 0);

                backBtn.AddComponent<Button>();
                backBtn.AddComponent<Image>().color = new Color(1, 1, 1, 0);

                GameObject arrow = new GameObject("Arrow");
                arrow.transform.SetParent(backBtn.transform, false);
                RectTransform arrowRT = arrow.AddComponent<RectTransform>();
                arrowRT.anchorMin = Vector2.zero;
                arrowRT.anchorMax = Vector2.one;
                arrowRT.sizeDelta = Vector2.zero;

                TextMeshProUGUI arrowText = arrow.AddComponent<TextMeshProUGUI>();
                arrowText.text = "\u2190";
                arrowText.fontSize = 42;
                arrowText.color = TEXT_PRIMARY;
                arrowText.alignment = TextAlignmentOptions.Center;
                arrowText.fontStyle = FontStyles.Bold;
            }

            // Title
            GameObject title = new GameObject("Title");
            title.transform.SetParent(header.transform, false);

            RectTransform titleRT = title.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 0.5f);
            titleRT.anchorMax = new Vector2(0.5f, 0.5f);
            titleRT.sizeDelta = new Vector2(900, 80);

            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "CashHistory";
            titleText.fontSize = 78;
            titleText.color = TEXT_GOLD;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
        }

        #endregion

        #region Stats Panel

        private static void CreateStatsPanel(Transform parent)
        {
            GameObject panel = new GameObject("StatsPanel");
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(-16, 480);  // Panel para 10 stats (5x2 grid)
            rt.anchoredPosition = new Vector2(0, -105);

            Image bg = panel.AddComponent<Image>();
            bg.color = CARD_BG;

            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = CARD_BORDER;
            outline.effectDistance = new Vector2(1, -1);

            // Grid layout for stats - 5 columns x 2 rows
            GridLayoutGroup glg = panel.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(200, 220);
            glg.spacing = new Vector2(4, 8);
            glg.padding = new RectOffset(10, 10, 10, 10);
            glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
            glg.startAxis = GridLayoutGroup.Axis.Horizontal;
            glg.childAlignment = TextAnchor.MiddleCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 5;

            // Stats con iconos - Row 1
            CreateStatItemWithIcon(panel.transform, "stat_victories", "Victorias", "24", SUCCESS_GREEN);
            CreateStatItemWithIcon(panel.transform, "stat_defeats", "Derrotas", "12", ERROR_RED);
            CreateStatItemWithIcon(panel.transform, "stat_winrate", "Win Rate", "67%", CYAN_ACCENT);
            CreateStatItemWithIcon(panel.transform, "stat_earnings", "Ganado", "$156", SUCCESS_GREEN);
            CreateStatItemWithIcon(panel.transform, "stat_draws", "Empates", "3", TEXT_SECONDARY);

            // Stats con iconos - Row 2
            CreateStatItemWithIcon(panel.transform, "stat_total", "Total Partidas", "39", CYAN_ACCENT);
            CreateStatItemWithIcon(panel.transform, "stat_streak", "Racha Actual", "5", GOLD_PRIMARY);
            CreateStatItemWithIcon(panel.transform, "stat_beststreak", "Mejor Racha", "8", GOLD_LIGHT);
            CreateStatItemWithIcon(panel.transform, "stat_tourneysplayed", "Torneos Jugados", "6", CYAN_ACCENT);
            CreateStatItemWithIcon(panel.transform, "stat_tourneyswins", "Torneos Ganados", "2", SUCCESS_GREEN);
        }

        private static void CreateStatItemWithIcon(Transform parent, string iconName, string label, string value, Color valueColor)
        {
            GameObject item = new GameObject("Stat_" + label);
            item.transform.SetParent(parent, false);

            RectTransform itemRT = item.AddComponent<RectTransform>();

            // Icon - arriba, más grande
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(item.transform, false);

            RectTransform iconRT = iconObj.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 1);
            iconRT.anchorMax = new Vector2(0.5f, 1);
            iconRT.pivot = new Vector2(0.5f, 1);
            iconRT.sizeDelta = new Vector2(100, 100);  // Iconos x2
            iconRT.anchoredPosition = new Vector2(0, -10);

            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.preserveAspect = true;

            string iconPath = STAT_ICONS_PATH + iconName + ".png";
            Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            if (iconSprite != null)
            {
                iconImg.sprite = iconSprite;
                iconImg.color = Color.white;
            }
            else
            {
                iconImg.color = valueColor;
                Debug.LogWarning($"[CashHistoryUIBuilder] Icon not found: {iconPath}");
            }

            // Value - debajo del icono
            GameObject valueObj = new GameObject("Value");
            valueObj.transform.SetParent(item.transform, false);

            RectTransform valueRT = valueObj.AddComponent<RectTransform>();
            valueRT.anchorMin = new Vector2(0, 1);
            valueRT.anchorMax = new Vector2(1, 1);
            valueRT.pivot = new Vector2(0.5f, 1);
            valueRT.sizeDelta = new Vector2(0, 52);
            valueRT.anchoredPosition = new Vector2(0, -116);  // Debajo del icono (10 + 100 + 6)

            TextMeshProUGUI valueText = valueObj.AddComponent<TextMeshProUGUI>();
            valueText.text = value;
            valueText.fontSize = 50;
            valueText.color = valueColor;
            valueText.alignment = TextAlignmentOptions.Center;
            valueText.fontStyle = FontStyles.Bold;

            // Label - debajo del value
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(item.transform, false);

            RectTransform labelRT = labelObj.AddComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 1);
            labelRT.anchorMax = new Vector2(1, 1);
            labelRT.pivot = new Vector2(0.5f, 1);
            labelRT.sizeDelta = new Vector2(0, 44);
            labelRT.anchoredPosition = new Vector2(0, -170);  // Debajo del value

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.fontSize = 40;
            labelText.color = TEXT_SECONDARY;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.fontStyle = FontStyles.Bold;
        }

        #endregion

        #region Filters Panel

        private static void CreateFiltersPanel(Transform parent)
        {
            GameObject panel = new GameObject("FiltersPanel");
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(-30, 60);
            rt.anchoredPosition = new Vector2(0, -590);  // Debajo del stats panel (105 + 480 + 5)

            // Horizontal layout for filters
            HorizontalLayoutGroup hlg = panel.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.padding = new RectOffset(5, 5, 5, 5);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            // Filter buttons
            CreateFilterButton(panel.transform, "FilterAll", "Todas", true);
            CreateFilterButton(panel.transform, "FilterWins", "Victorias", false);
            CreateFilterButton(panel.transform, "FilterLosses", "Derrotas", false);
        }

        private static void CreateFilterButton(Transform parent, string name, string text, bool isActive)
        {
            GameObject btn = new GameObject(name);
            btn.transform.SetParent(parent, false);

            Image bg = btn.AddComponent<Image>();
            bg.color = isActive ? FILTER_ACTIVE : FILTER_INACTIVE;

            Button button = btn.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = isActive ? FILTER_ACTIVE : FILTER_INACTIVE;
            colors.highlightedColor = GOLD_LIGHT;
            colors.pressedColor = GOLD_DARK;
            button.colors = colors;

            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btn.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.text = text;
            btnText.fontSize = 52;
            btnText.color = isActive ? BG_DARK : TEXT_PRIMARY;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.fontStyle = FontStyles.Bold;
        }

        #endregion

        #region Match History List

        private static void CreateMatchHistoryList(Transform parent)
        {
            GameObject scrollView = new GameObject("MatchHistoryList");
            scrollView.transform.SetParent(parent, false);

            RectTransform svRT = scrollView.AddComponent<RectTransform>();
            svRT.anchorMin = new Vector2(0, 0);
            svRT.anchorMax = new Vector2(1, 1);
            svRT.offsetMin = new Vector2(10, 15);  // Bottom padding
            svRT.offsetMax = new Vector2(-10, -660);  // Top offset (header 105 + stats 480 + gap 5 + filters 60 + gap 10)

            ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = 30;

            scrollView.AddComponent<Image>().color = new Color(0, 0, 0, 0);

            // Viewport - IMPORTANTE: RectMask2D en lugar de Mask para mejor rendimiento
            // y no requiere Image con alpha
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);

            RectTransform vpRT = viewport.AddComponent<RectTransform>();
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.sizeDelta = Vector2.zero;
            vpRT.offsetMin = Vector2.zero;
            vpRT.offsetMax = Vector2.zero;

            // Usar RectMask2D en lugar de Mask - funciona mejor para ScrollRect
            viewport.AddComponent<RectMask2D>();

            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);

            RectTransform contentRT = content.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.sizeDelta = new Vector2(0, 1000);  // Initial height, ContentSizeFitter will adjust
            contentRT.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 24;
            vlg.padding = new RectOffset(0, 0, 10, 10);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = vpRT;
            scroll.content = contentRT;

            // Sample matches with new design
            CreateMatchHistoryItem(content.transform, "QuickMath", "@ProGamer99", true, 5f, 8.50f, "Hoy, 14:32", "1250-900", "1v1");
            CreateMatchHistoryItem(content.transform, "FlashTap", "@SpeedKing", false, 10f, -10f, "Hoy, 12:15", "3-5", "1v1");
            CreateMatchHistoryItem(content.transform, "MemoryPairs", "@MemMaster", true, 5f, 15f, "Ayer, 20:45", "6-4", "Torneo");
            CreateMatchHistoryItem(content.transform, "CognitiveSprint", "@BrainStorm", true, 25f, 45f, "Ayer, 18:00", "7-2", "1v1");
            CreateMatchHistoryItem(content.transform, "OddOneOut", "@EagleEye", false, 10f, -10f, "Hace 2 días", "2-5", "1v1");
            CreateMatchHistoryItem(content.transform, "DigitRush", "@NumberKing", true, 15f, 27f, "Hace 2 días", "4500-3200", "1v1");
            CreateMatchHistoryItem(content.transform, "QuickMath", "@MathWiz", true, 50f, 90f, "Hace 3 días", "8-3", "Torneo");
        }

        private static GameObject CreateMatchHistoryItem(Transform parent, string game, string opponent, bool isWin, float entryFee, float netAmount, string date, string score, string mode)
        {
            GameObject item = new GameObject("MatchItem_" + game);
            item.transform.SetParent(parent, false);

            RectTransform rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 260);

            LayoutElement le = item.AddComponent<LayoutElement>();
            le.preferredHeight = 260;
            le.flexibleWidth = 1;

            Image bg = item.AddComponent<Image>();
            bg.color = CARD_BG_LIGHT;

            // Bottom color bar (thicker, more visible)
            GameObject colorBar = new GameObject("ColorBar");
            colorBar.transform.SetParent(item.transform, false);

            RectTransform barRT = colorBar.AddComponent<RectTransform>();
            barRT.anchorMin = new Vector2(0, 0);
            barRT.anchorMax = new Vector2(1, 0);
            barRT.pivot = new Vector2(0.5f, 0);
            barRT.sizeDelta = new Vector2(0, 12);
            barRT.anchoredPosition = Vector2.zero;

            Image barImg = colorBar.AddComponent<Image>();
            barImg.color = isWin ? SUCCESS_GREEN : ERROR_RED;

            // Game Icon container
            GameObject iconContainer = new GameObject("GameIcon");
            iconContainer.transform.SetParent(item.transform, false);

            RectTransform iconRT = iconContainer.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0, 0.5f);
            iconRT.anchorMax = new Vector2(0, 0.5f);
            iconRT.pivot = new Vector2(0, 0.5f);
            iconRT.sizeDelta = new Vector2(180, 180);
            iconRT.anchoredPosition = new Vector2(30, 10);

            Image iconBg = iconContainer.AddComponent<Image>();
            iconBg.color = new Color(0.1f, 0.1f, 0.15f, 1f);

            // Try to load game icon from Assets/_Project/Art/Icons/Games/
            string iconPath = GAME_ICONS_PATH + game + "Icon.png";
            Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            if (iconSprite != null)
            {
                // Create child for sprite (Image with the actual game icon)
                GameObject iconChild = new GameObject("Sprite");
                iconChild.transform.SetParent(iconContainer.transform, false);

                RectTransform iconChildRT = iconChild.AddComponent<RectTransform>();
                iconChildRT.anchorMin = Vector2.zero;
                iconChildRT.anchorMax = Vector2.one;
                iconChildRT.sizeDelta = new Vector2(-20, -20);
                iconChildRT.anchoredPosition = Vector2.zero;

                Image iconChildImg = iconChild.AddComponent<Image>();
                iconChildImg.sprite = iconSprite;
                iconChildImg.preserveAspect = true;

                iconBg.color = new Color(0.05f, 0.05f, 0.08f, 1f);
            }
            else
            {
                // Placeholder text fallback when icon not found
                GameObject placeholder = new GameObject("Placeholder");
                placeholder.transform.SetParent(iconContainer.transform, false);

                RectTransform placeholderRT = placeholder.AddComponent<RectTransform>();
                placeholderRT.anchorMin = Vector2.zero;
                placeholderRT.anchorMax = Vector2.one;
                placeholderRT.sizeDelta = Vector2.zero;

                TextMeshProUGUI placeholderText = placeholder.AddComponent<TextMeshProUGUI>();
                placeholderText.text = game.Substring(0, 2).ToUpper();
                placeholderText.fontSize = 56;
                placeholderText.fontStyle = FontStyles.Bold;
                placeholderText.color = TEXT_GOLD;
                placeholderText.alignment = TextAlignmentOptions.Center;

                Debug.LogWarning($"[CashHistoryUIBuilder] Game icon not found: {iconPath}, using text placeholder");
            }

            // Game name + Mode badge
            GameObject gameNameRow = new GameObject("GameNameRow");
            gameNameRow.transform.SetParent(item.transform, false);

            RectTransform gameNameRowRT = gameNameRow.AddComponent<RectTransform>();
            gameNameRowRT.anchorMin = new Vector2(0, 1);
            gameNameRowRT.anchorMax = new Vector2(0.65f, 1);
            gameNameRowRT.pivot = new Vector2(0, 1);
            gameNameRowRT.sizeDelta = new Vector2(0, 70);
            gameNameRowRT.anchoredPosition = new Vector2(230, -24);

            HorizontalLayoutGroup gameHLG = gameNameRow.AddComponent<HorizontalLayoutGroup>();
            gameHLG.spacing = 20;
            gameHLG.childAlignment = TextAnchor.MiddleLeft;
            gameHLG.childForceExpandWidth = false;
            gameHLG.childForceExpandHeight = true;
            gameHLG.childControlWidth = false;
            gameHLG.childControlHeight = true;

            // Game name
            GameObject gameObj = new GameObject("GameName");
            gameObj.transform.SetParent(gameNameRow.transform, false);

            LayoutElement gameLE = gameObj.AddComponent<LayoutElement>();
            gameLE.preferredWidth = 400;

            TextMeshProUGUI gameText = gameObj.AddComponent<TextMeshProUGUI>();
            gameText.text = game;
            gameText.fontSize = 48;
            gameText.color = TEXT_GOLD;
            gameText.fontStyle = FontStyles.Bold;
            gameText.alignment = TextAlignmentOptions.Left;
            gameText.overflowMode = TextOverflowModes.Ellipsis;

            // Mode badge
            GameObject modeBadge = new GameObject("ModeBadge");
            modeBadge.transform.SetParent(gameNameRow.transform, false);

            LayoutElement modeLE = modeBadge.AddComponent<LayoutElement>();
            modeLE.preferredWidth = 130;
            modeLE.preferredHeight = 52;

            Image modeBg = modeBadge.AddComponent<Image>();
            modeBg.color = mode == "Torneo" ? new Color(0.6f, 0.2f, 0.8f, 1f) : new Color(0.2f, 0.5f, 0.7f, 1f);

            GameObject modeText = new GameObject("Text");
            modeText.transform.SetParent(modeBadge.transform, false);

            RectTransform modeTextRT = modeText.AddComponent<RectTransform>();
            modeTextRT.anchorMin = Vector2.zero;
            modeTextRT.anchorMax = Vector2.one;
            modeTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI mt = modeText.AddComponent<TextMeshProUGUI>();
            mt.text = mode;
            mt.fontSize = 28;
            mt.color = TEXT_PRIMARY;
            mt.alignment = TextAlignmentOptions.Center;
            mt.fontStyle = FontStyles.Bold;

            // Opponent
            GameObject oppObj = new GameObject("Opponent");
            oppObj.transform.SetParent(item.transform, false);

            RectTransform oppRT = oppObj.AddComponent<RectTransform>();
            oppRT.anchorMin = new Vector2(0, 0.5f);
            oppRT.anchorMax = new Vector2(0.6f, 0.5f);
            oppRT.pivot = new Vector2(0, 0.5f);
            oppRT.sizeDelta = new Vector2(0, 50);
            oppRT.anchoredPosition = new Vector2(230, -10);

            TextMeshProUGUI oppText = oppObj.AddComponent<TextMeshProUGUI>();
            oppText.text = $"vs {opponent}";
            oppText.fontSize = 36;
            oppText.fontStyle = FontStyles.Bold;
            oppText.color = CYAN_ACCENT;
            oppText.alignment = TextAlignmentOptions.Left;

            // Date + Score row
            GameObject infoRow = new GameObject("InfoRow");
            infoRow.transform.SetParent(item.transform, false);

            RectTransform infoRowRT = infoRow.AddComponent<RectTransform>();
            infoRowRT.anchorMin = new Vector2(0, 0);
            infoRowRT.anchorMax = new Vector2(0.6f, 0);
            infoRowRT.pivot = new Vector2(0, 0);
            infoRowRT.sizeDelta = new Vector2(0, 60);
            infoRowRT.anchoredPosition = new Vector2(230, 30);

            TextMeshProUGUI infoText = infoRow.AddComponent<TextMeshProUGUI>();
            infoText.text = $"{date}  •  Score: {score}";
            infoText.fontSize = 32;
            infoText.fontStyle = FontStyles.Bold;
            infoText.color = TEXT_SECONDARY;
            infoText.alignment = TextAlignmentOptions.Left;

            // Result label (VICTORIA/DERROTA)
            GameObject resultObj = new GameObject("Result");
            resultObj.transform.SetParent(item.transform, false);

            RectTransform resRT = resultObj.AddComponent<RectTransform>();
            resRT.anchorMin = new Vector2(1, 1);
            resRT.anchorMax = new Vector2(1, 1);
            resRT.pivot = new Vector2(1, 1);
            resRT.sizeDelta = new Vector2(260, 60);
            resRT.anchoredPosition = new Vector2(-30, -30);

            TextMeshProUGUI resText = resultObj.AddComponent<TextMeshProUGUI>();
            resText.text = isWin ? "VICTORIA" : "DERROTA";
            resText.fontSize = 36;
            resText.color = isWin ? SUCCESS_GREEN : ERROR_RED;
            resText.alignment = TextAlignmentOptions.Right;
            resText.fontStyle = FontStyles.Bold;

            // Amount (net gain/loss)
            GameObject amountObj = new GameObject("Amount");
            amountObj.transform.SetParent(item.transform, false);

            RectTransform amtRT = amountObj.AddComponent<RectTransform>();
            amtRT.anchorMin = new Vector2(1, 0.5f);
            amtRT.anchorMax = new Vector2(1, 0.5f);
            amtRT.pivot = new Vector2(1, 0.5f);
            amtRT.sizeDelta = new Vector2(260, 80);
            amtRT.anchoredPosition = new Vector2(-30, 0);

            TextMeshProUGUI amtText = amountObj.AddComponent<TextMeshProUGUI>();
            string amountStr = netAmount >= 0 ? $"+${netAmount:F2}" : $"-${Mathf.Abs(netAmount):F2}";
            amtText.text = amountStr;
            amtText.fontSize = 56;
            amtText.color = isWin ? SUCCESS_GREEN : ERROR_RED;
            amtText.alignment = TextAlignmentOptions.Right;
            amtText.fontStyle = FontStyles.Bold;

            // Entry fee info
            GameObject entryObj = new GameObject("EntryFee");
            entryObj.transform.SetParent(item.transform, false);

            RectTransform entryRT = entryObj.AddComponent<RectTransform>();
            entryRT.anchorMin = new Vector2(1, 0);
            entryRT.anchorMax = new Vector2(1, 0);
            entryRT.pivot = new Vector2(1, 0);
            entryRT.sizeDelta = new Vector2(260, 50);
            entryRT.anchoredPosition = new Vector2(-30, 30);

            TextMeshProUGUI entryText = entryObj.AddComponent<TextMeshProUGUI>();
            entryText.text = $"Entrada: ${entryFee:F0}";
            entryText.fontSize = 28;
            entryText.fontStyle = FontStyles.Bold;
            entryText.color = TEXT_SECONDARY;
            entryText.alignment = TextAlignmentOptions.Right;

            return item;
        }

        #endregion

        #region Hidden Elements

        private static void CreateEmptyStateText(Transform parent)
        {
            // Find the Content inside MatchHistoryList to place empty state text
            Transform scrollView = parent.Find("MatchHistoryList");
            Transform contentParent = parent;
            if (scrollView != null)
            {
                Transform viewport = scrollView.Find("Viewport");
                if (viewport != null)
                {
                    Transform content = viewport.Find("Content");
                    if (content != null) contentParent = content;
                }
            }

            GameObject emptyState = new GameObject("EmptyStateText");
            emptyState.transform.SetParent(contentParent, false);

            RectTransform rt = emptyState.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(0, 80);

            TextMeshProUGUI tmp = emptyState.AddComponent<TextMeshProUGUI>();
            tmp.text = "No hay historial";
            tmp.fontSize = 28;
            tmp.color = new Color(0.5f, 0.5f, 0.55f, 1f);
            tmp.alignment = TextAlignmentOptions.Center;

            emptyState.SetActive(false);
        }

        private static void CreateLoadMoreButton(Transform parent)
        {
            GameObject loadMore = new GameObject("LoadMoreButton");
            loadMore.transform.SetParent(parent, false);

            RectTransform rt = loadMore.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.2f, 0);
            rt.anchorMax = new Vector2(0.8f, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(0, 60);
            rt.anchoredPosition = new Vector2(0, 10);

            Image bg = loadMore.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.25f, 1f);

            loadMore.AddComponent<Button>();

            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(loadMore.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "Cargar M\u00e1s";
            tmp.fontSize = 28;
            tmp.color = GOLD_PRIMARY;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;

            loadMore.SetActive(false);
        }

        private static void CreateLoadingIndicator(Transform parent)
        {
            GameObject loading = new GameObject("LoadingIndicator");
            loading.transform.SetParent(parent, false);

            RectTransform rt = loading.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(80, 80);

            loading.SetActive(false);
        }

        private static void CreateDetailPanel(Transform parent)
        {
            GameObject detail = new GameObject("DetailPanel");
            detail.transform.SetParent(parent, false);

            RectTransform rt = detail.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = detail.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.1f, 0.13f, 0.95f);

            VerticalLayoutGroup vlg = detail.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 12;
            vlg.padding = new RectOffset(40, 40, 80, 40);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            // Close button (top-right, outside layout)
            GameObject closeBtn = new GameObject("CloseDetailButton");
            closeBtn.transform.SetParent(detail.transform, false);

            RectTransform closeBtnRT = closeBtn.AddComponent<RectTransform>();
            closeBtnRT.anchorMin = new Vector2(1, 1);
            closeBtnRT.anchorMax = new Vector2(1, 1);
            closeBtnRT.pivot = new Vector2(1, 1);
            closeBtnRT.sizeDelta = new Vector2(70, 70);
            closeBtnRT.anchoredPosition = new Vector2(-15, -15);

            Image closeBg = closeBtn.AddComponent<Image>();
            closeBg.color = new Color(0.3f, 0.3f, 0.35f, 0.8f);
            closeBtn.AddComponent<Button>();

            LayoutElement closeLE = closeBtn.AddComponent<LayoutElement>();
            closeLE.ignoreLayout = true;

            GameObject closeText = new GameObject("Text");
            closeText.transform.SetParent(closeBtn.transform, false);
            RectTransform closeTextRT = closeText.AddComponent<RectTransform>();
            closeTextRT.anchorMin = Vector2.zero;
            closeTextRT.anchorMax = Vector2.one;
            closeTextRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI closeTMP = closeText.AddComponent<TextMeshProUGUI>();
            closeTMP.text = "X";
            closeTMP.fontSize = 36;
            closeTMP.color = TEXT_PRIMARY;
            closeTMP.alignment = TextAlignmentOptions.Center;
            closeTMP.fontStyle = FontStyles.Bold;

            // Detail texts
            CreateDetailText(detail.transform, "DetailTitleText", "T\u00edtulo", 36, FontStyles.Bold, TEXT_PRIMARY, 60);
            CreateDetailText(detail.transform, "DetailSubtitleText", "Subtitulo", 28, FontStyles.Normal, TEXT_SECONDARY, 45);
            CreateDetailText(detail.transform, "DetailResultText", "VICTORIA", 42, FontStyles.Bold, SUCCESS_GREEN, 65);
            CreateDetailText(detail.transform, "DetailScoreText", "Score: 5 - 3", 28, FontStyles.Normal, TEXT_PRIMARY, 45);
            CreateDetailText(detail.transform, "DetailEntryFeeText", "Entrada: $5.00", 28, FontStyles.Normal, GOLD_PRIMARY, 45);
            CreateDetailText(detail.transform, "DetailPrizeText", "Premio: $8.50", 28, FontStyles.Normal, SUCCESS_GREEN, 45);
            CreateDetailText(detail.transform, "DetailNetText", "+$3.50", 36, FontStyles.Bold, SUCCESS_GREEN, 55);
            CreateDetailText(detail.transform, "DetailDateText", "Hoy, 14:32", 24, FontStyles.Normal, TEXT_SECONDARY, 40);
            CreateDetailText(detail.transform, "DetailDurationText", "Duraci\u00f3n: 2m 15s", 24, FontStyles.Normal, TEXT_SECONDARY, 40);

            detail.SetActive(false);
        }

        private static void CreateDetailText(Transform parent, string name, string text, float fontSize, FontStyles style, Color color, float height)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            LayoutElement le = obj.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.flexibleWidth = 1;

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        #endregion

        #region Canvas Finder

        private static Canvas FindMainCanvas()
        {
            foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (c.gameObject.name == "Canvas")
                    return c;
            }
            foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (c.gameObject.name != "TransitionCanvas")
                    return c;
            }
            return Object.FindFirstObjectByType<Canvas>();
        }

        #endregion

        #region Reference Assigner

        private static MonoBehaviour FindCashHistoryController()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "CashHistorySceneController") return mb;
            return null;
        }

        private static void ResetAssignState()
        {
            assignedCount = 0;
            failedCount = 0;
            alreadySetCount = 0;
            assignResults.Clear();
        }

        private static void RunAssignAllReferences()
        {
            Debug.Log("[CashHistoryUIBuilder] === ASSIGNING CASHHISTORY REFERENCES ===");

            var controller = FindCashHistoryController();
            if (controller == null)
            {
                Debug.LogError("[CashHistoryUIBuilder] CashHistorySceneController not found in scene!");
                AddAR("CashHistorySceneController", "Controller not found", false, null);
                failedCount++;
                return;
            }

            SerializedObject so = new SerializedObject(controller);
            so.Update();

            Canvas canvas = FindMainCanvas();
            Transform root = canvas != null ? canvas.transform : controller.transform.root;

            // ==================== HEADER ====================
            AssignRef(so, "backButton", FindBtnDeep(root, "BackButton"));

            AssignRef(so, "titleText", FindTextDeep(root, "Title"));

            // ==================== STATS PANEL ====================
            Transform statsPanelT = FindDeep(root, "StatsPanel");
            AssignGORef(so, "statsPanel", statsPanelT != null ? statsPanelT.gameObject : null);

            // Stats use parent/child pattern: find "Stat_XXX" then get child "Value"
            AssignRef(so, "winsText", FindStatValueText(root, "Stat_Victorias"));
            AssignRef(so, "lossesText", FindStatValueText(root, "Stat_Derrotas"));
            AssignRef(so, "winRateText", FindStatValueText(root, "Stat_Win Rate"));
            AssignRef(so, "netProfitText", FindStatValueText(root, "Stat_Ganado"));
            AssignRef(so, "drawsText", FindStatValueText(root, "Stat_Empates"));
            AssignRef(so, "totalMatchesText", FindStatValueText(root, "Stat_Total Partidas"));
            AssignRef(so, "currentStreakText", FindStatValueText(root, "Stat_Racha Actual"));
            AssignRef(so, "bestStreakText", FindStatValueText(root, "Stat_Mejor Racha"));
            AssignRef(so, "tournamentsPlayedText", FindStatValueText(root, "Stat_Torneos Jugados"));
            AssignRef(so, "tournamentWinsText", FindStatValueText(root, "Stat_Torneos Ganados"));

            // ==================== FILTER TABS ====================
            AssignRef(so, "allTabButton", FindBtnDeep(root, "FilterAll"));

            AssignRef(so, "matchesTabButton", FindBtnDeep(root, "FilterWins"));

            AssignRef(so, "tournamentsTabButton", FindBtnDeep(root, "FilterLosses"));

            // ==================== MATCH HISTORY LIST ====================
            // entriesContainer: look for EntriesContainer first, then Content inside ScrollView
            Transform entriesContainerT = FindDeep(root, "EntriesContainer");
            if (entriesContainerT == null)
            {
                Transform scrollViewT = FindDeep(root, "MatchHistoryList");
                if (scrollViewT != null)
                {
                    entriesContainerT = FindDeep(scrollViewT, "Content");
                }
            }
            AssignRef(so, "entriesContainer", entriesContainerT);

            // historyEntryPrefab - skip (prefab, can't auto-assign from scene)
            Debug.Log("[CashHistoryUIBuilder] ~ historyEntryPrefab: Skipped (prefab reference, assign manually)");

            // scrollRect
            Transform scrollRectT = FindDeep(root, "MatchHistoryList");
            AssignRef(so, "scrollRect", scrollRectT != null ? scrollRectT.GetComponent<ScrollRect>() : null);

            AssignRef(so, "emptyStateText", FindTextDeep(root, "EmptyStateText"));

            AssignRef(so, "loadMoreButton", FindBtnDeep(root, "LoadMoreButton"));

            // ==================== DETAIL PANEL ====================
            Transform detailPanelT = FindDeep(root, "DetailPanel");
            AssignGORef(so, "detailPanel", detailPanelT != null ? detailPanelT.gameObject : null);

            AssignRef(so, "detailTitleText", FindTextDeep(root, "DetailTitleText"));
            AssignRef(so, "detailSubtitleText", FindTextDeep(root, "DetailSubtitleText"));
            AssignRef(so, "detailResultText", FindTextDeep(root, "DetailResultText"));
            AssignRef(so, "detailScoreText", FindTextDeep(root, "DetailScoreText"));
            AssignRef(so, "detailEntryFeeText", FindTextDeep(root, "DetailEntryFeeText"));
            AssignRef(so, "detailPrizeText", FindTextDeep(root, "DetailPrizeText"));
            AssignRef(so, "detailNetText", FindTextDeep(root, "DetailNetText"));
            AssignRef(so, "detailDateText", FindTextDeep(root, "DetailDateText"));
            AssignRef(so, "detailDurationText", FindTextDeep(root, "DetailDurationText"));

            AssignRef(so, "closeDetailButton", FindBtnDeep(root, "CloseDetailButton"));

            // ==================== LOADING ====================
            Transform loadingT = FindDeep(root, "LoadingIndicator");
            AssignGORef(so, "loadingIndicator", loadingT != null ? loadingT.gameObject : null);

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(controller.gameObject);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            Debug.Log("[CashHistoryUIBuilder] === ASSIGNMENT COMPLETE ===");
        }

        // ---- Deep finders ----

        private static Transform FindDeep(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                Transform result = FindDeep(child, name);
                if (result != null) return result;
            }
            return null;
        }

        private static TextMeshProUGUI FindTextDeep(Transform root, string name)
        {
            Transform t = FindDeep(root, name);
            return t != null ? t.GetComponent<TextMeshProUGUI>() : null;
        }

        private static Button FindBtnDeep(Transform root, string name)
        {
            Transform t = FindDeep(root, name);
            return t != null ? t.GetComponent<Button>() : null;
        }

        private static TextMeshProUGUI FindStatValueText(Transform root, string statParentName)
        {
            Transform statParent = FindDeep(root, statParentName);
            if (statParent != null)
            {
                Transform valueChild = statParent.Find("Value");
                if (valueChild != null)
                {
                    return valueChild.GetComponent<TextMeshProUGUI>();
                }
            }
            return null;
        }

        // ---- Assignment helpers ----

        private static void AssignRef(SerializedObject so, string propertyName, Object value)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { AddAR(propertyName, "Property not found", false, null); failedCount++; return; }
            if (prop.objectReferenceValue != null) { AddAR(propertyName, "Already Set", true, prop.objectReferenceValue); alreadySetCount++; return; }
            if (value != null) { prop.objectReferenceValue = value; AddAR(propertyName, "Assigned", true, value); assignedCount++; }
            else { AddAR(propertyName, "Not found", false, null); failedCount++; }
        }

        private static void AssignGORef(SerializedObject so, string propertyName, GameObject value)
        {
            AssignRef(so, propertyName, value);
        }

        private static void AddAR(string fieldName, string status, bool success, Object assignedObject)
        {
            assignResults.Add(new AssignResult
            {
                fieldName = fieldName,
                status = status,
                success = success,
                assignedObject = assignedObject
            });
        }

        private void DrawAssignResults()
        {
            if (assignResults.Count == 0) return;

            EditorGUILayout.Space(10);

            int total = assignResults.Count;
            int successTotal = assignedCount + alreadySetCount;

            EditorGUILayout.BeginVertical("box");

            float successRate = total > 0 ? (float)successTotal / total : 0f;
            GUI.color = successRate == 1f ? new Color(0.2f, 0.8f, 0.2f) :
                        successRate >= 0.7f ? new Color(1f, 0.8f, 0.2f) : new Color(1f, 0.4f, 0.4f);
            GUILayout.Label(successRate == 1f ? "TODAS LAS REFERENCIAS ASIGNADAS" : "Algunas referencias faltan", EditorStyles.boldLabel);
            GUI.color = Color.white;

            GUILayout.Label($"Asignados: {assignedCount} | Ya estaban: {alreadySetCount} | Fallidos: {failedCount}");

            EditorGUILayout.Space(5);

            foreach (var result in assignResults)
            {
                EditorGUILayout.BeginHorizontal();
                GUI.color = result.success ? (result.status == "Already Set" ? new Color(0.5f, 0.8f, 1f) : Color.green) : Color.red;
                GUILayout.Label(result.success ? (result.status == "Already Set" ? "o" : "+") : "x", GUILayout.Width(20));
                GUI.color = Color.white;
                GUILayout.Label(result.fieldName, GUILayout.Width(180));
                GUILayout.Label(result.status, GUILayout.Width(120));
                if (result.assignedObject != null)
                    EditorGUILayout.ObjectField(result.assignedObject, typeof(Object), true, GUILayout.Width(150));
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        #endregion
    }
}
