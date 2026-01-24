using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// UI Builder para la escena CashHistory.unity
    /// Construye el historial de partidas y estadísticas del jugador.
    /// MEJORADO: Con iconos de juego, filtros, cards rediseñadas.
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
        private static readonly string GAME_ICONS_PATH = "Assets/_Project/Art/Icons/Games/CashBattle/";
        private static readonly string STAT_ICONS_PATH = "Assets/_Project/Art/Icons/CashBattle/Stats/";

        #endregion

        [MenuItem("DigitPark/UI Builders/CashBattle/Cash History", false, 253)]
        public static void ShowWindow()
        {
            GetWindow<CashHistoryUIBuilder>("Cash History Builder");
        }

        private void OnGUI()
        {
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
        }

        #region Build Methods

        private static void BuildCashHistoryUI()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
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
            Canvas canvas = FindFirstObjectByType<Canvas>();
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
            Canvas canvas = FindFirstObjectByType<Canvas>();
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
            Canvas canvas = FindFirstObjectByType<Canvas>();
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
        }

        private static void CleanupOldElements(Transform parent)
        {
            string[] toDestroy = { "Background", "SafeArea", "Header", "StatsPanel", "FiltersPanel", "MatchHistoryList" };
            foreach (string name in toDestroy)
            {
                Transform existing = parent.Find(name);
                if (existing != null) DestroyImmediate(existing.gameObject);
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

            // Back button
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
            arrowText.text = "←";
            arrowText.fontSize = 42;
            arrowText.color = TEXT_PRIMARY;
            arrowText.alignment = TextAlignmentOptions.Center;

            // Title
            GameObject title = new GameObject("Title");
            title.transform.SetParent(header.transform, false);

            RectTransform titleRT = title.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 0.5f);
            titleRT.anchorMax = new Vector2(0.5f, 0.5f);
            titleRT.sizeDelta = new Vector2(500, 60);

            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "Historial de Partidas";
            titleText.fontSize = 30;
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
            rt.sizeDelta = new Vector2(-16, 130);  // Panel más grande
            rt.anchoredPosition = new Vector2(0, -105);

            Image bg = panel.AddComponent<Image>();
            bg.color = CARD_BG;

            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = CARD_BORDER;
            outline.effectDistance = new Vector2(1, -1);

            // Horizontal layout for stats - distribución uniforme
            HorizontalLayoutGroup hlg = panel.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 0;
            hlg.padding = new RectOffset(10, 10, 10, 10);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            // Stats con iconos de imagen
            CreateStatItemWithIcon(panel.transform, "stat_victories", "Victorias", "24", SUCCESS_GREEN);
            CreateStatItemWithIcon(panel.transform, "stat_defeats", "Derrotas", "12", ERROR_RED);
            CreateStatItemWithIcon(panel.transform, "stat_winrate", "Win Rate", "67%", CYAN_ACCENT);
            CreateStatItemWithIcon(panel.transform, "stat_earnings", "Ganado", "$156", SUCCESS_GREEN);
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
            iconRT.sizeDelta = new Vector2(50, 50);  // Iconos más grandes
            iconRT.anchoredPosition = new Vector2(0, -5);

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
            valueRT.sizeDelta = new Vector2(0, 26);
            valueRT.anchoredPosition = new Vector2(0, -58);  // Debajo del icono (5 + 50 + 3)

            TextMeshProUGUI valueText = valueObj.AddComponent<TextMeshProUGUI>();
            valueText.text = value;
            valueText.fontSize = 20;
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
            labelRT.sizeDelta = new Vector2(0, 20);
            labelRT.anchoredPosition = new Vector2(0, -85);  // Debajo del value

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.fontSize = 13;
            labelText.color = TEXT_SECONDARY;
            labelText.alignment = TextAlignmentOptions.Center;
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
            rt.sizeDelta = new Vector2(-30, 50);
            rt.anchoredPosition = new Vector2(0, -240);  // Debajo del stats panel expandido

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
            btnText.fontSize = 20;
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
            svRT.offsetMax = new Vector2(-10, -295);  // Top offset (header 105 + stats 130 + gap 5 + filters 50 + gap 5)

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
            vlg.spacing = 12;
            vlg.padding = new RectOffset(0, 0, 5, 5);
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
            rt.sizeDelta = new Vector2(0, 130);

            LayoutElement le = item.AddComponent<LayoutElement>();
            le.preferredHeight = 130;
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
            barRT.sizeDelta = new Vector2(0, 6);
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
            iconRT.sizeDelta = new Vector2(90, 90);
            iconRT.anchoredPosition = new Vector2(15, 5);

            Image iconBg = iconContainer.AddComponent<Image>();
            iconBg.color = new Color(0.1f, 0.1f, 0.15f, 1f);

            // Try to load game icon
            Image iconImg = iconContainer.AddComponent<Image>();
            string iconPath = GAME_ICONS_PATH + game + "Icon.png";
            Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            if (iconSprite != null)
            {
                // Create child for sprite
                GameObject iconChild = new GameObject("Sprite");
                iconChild.transform.SetParent(iconContainer.transform, false);

                RectTransform iconChildRT = iconChild.AddComponent<RectTransform>();
                iconChildRT.anchorMin = Vector2.zero;
                iconChildRT.anchorMax = Vector2.one;
                iconChildRT.sizeDelta = new Vector2(-10, -10);
                iconChildRT.anchoredPosition = Vector2.zero;

                Image iconChildImg = iconChild.AddComponent<Image>();
                iconChildImg.sprite = iconSprite;
                iconChildImg.preserveAspect = true;

                iconBg.color = new Color(0.05f, 0.05f, 0.08f, 1f);
            }
            else
            {
                // Placeholder text
                GameObject placeholder = new GameObject("Placeholder");
                placeholder.transform.SetParent(iconContainer.transform, false);

                RectTransform placeholderRT = placeholder.AddComponent<RectTransform>();
                placeholderRT.anchorMin = Vector2.zero;
                placeholderRT.anchorMax = Vector2.one;
                placeholderRT.sizeDelta = Vector2.zero;

                TextMeshProUGUI placeholderText = placeholder.AddComponent<TextMeshProUGUI>();
                placeholderText.text = game.Substring(0, 2).ToUpper();
                placeholderText.fontSize = 28;
                placeholderText.color = TEXT_GOLD;
                placeholderText.alignment = TextAlignmentOptions.Center;
            }

            // Game name + Mode badge
            GameObject gameNameRow = new GameObject("GameNameRow");
            gameNameRow.transform.SetParent(item.transform, false);

            RectTransform gameNameRowRT = gameNameRow.AddComponent<RectTransform>();
            gameNameRowRT.anchorMin = new Vector2(0, 1);
            gameNameRowRT.anchorMax = new Vector2(0.65f, 1);
            gameNameRowRT.pivot = new Vector2(0, 1);
            gameNameRowRT.sizeDelta = new Vector2(0, 35);
            gameNameRowRT.anchoredPosition = new Vector2(115, -12);

            HorizontalLayoutGroup gameHLG = gameNameRow.AddComponent<HorizontalLayoutGroup>();
            gameHLG.spacing = 10;
            gameHLG.childAlignment = TextAnchor.MiddleLeft;
            gameHLG.childForceExpandWidth = false;
            gameHLG.childForceExpandHeight = true;
            gameHLG.childControlWidth = false;
            gameHLG.childControlHeight = true;

            // Game name
            GameObject gameObj = new GameObject("GameName");
            gameObj.transform.SetParent(gameNameRow.transform, false);

            LayoutElement gameLE = gameObj.AddComponent<LayoutElement>();
            gameLE.preferredWidth = 200;

            TextMeshProUGUI gameText = gameObj.AddComponent<TextMeshProUGUI>();
            gameText.text = game;
            gameText.fontSize = 24;
            gameText.color = TEXT_GOLD;
            gameText.fontStyle = FontStyles.Bold;
            gameText.alignment = TextAlignmentOptions.Left;
            gameText.overflowMode = TextOverflowModes.Ellipsis;

            // Mode badge
            GameObject modeBadge = new GameObject("ModeBadge");
            modeBadge.transform.SetParent(gameNameRow.transform, false);

            LayoutElement modeLE = modeBadge.AddComponent<LayoutElement>();
            modeLE.preferredWidth = 65;
            modeLE.preferredHeight = 26;

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
            mt.fontSize = 14;
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
            oppRT.sizeDelta = new Vector2(0, 25);
            oppRT.anchoredPosition = new Vector2(115, -5);

            TextMeshProUGUI oppText = oppObj.AddComponent<TextMeshProUGUI>();
            oppText.text = $"vs {opponent}";
            oppText.fontSize = 18;
            oppText.color = CYAN_ACCENT;
            oppText.alignment = TextAlignmentOptions.Left;

            // Date + Score row
            GameObject infoRow = new GameObject("InfoRow");
            infoRow.transform.SetParent(item.transform, false);

            RectTransform infoRowRT = infoRow.AddComponent<RectTransform>();
            infoRowRT.anchorMin = new Vector2(0, 0);
            infoRowRT.anchorMax = new Vector2(0.6f, 0);
            infoRowRT.pivot = new Vector2(0, 0);
            infoRowRT.sizeDelta = new Vector2(0, 30);
            infoRowRT.anchoredPosition = new Vector2(115, 15);

            TextMeshProUGUI infoText = infoRow.AddComponent<TextMeshProUGUI>();
            infoText.text = $"{date}  •  Score: {score}";
            infoText.fontSize = 16;
            infoText.color = TEXT_SECONDARY;
            infoText.alignment = TextAlignmentOptions.Left;

            // Result label (VICTORIA/DERROTA)
            GameObject resultObj = new GameObject("Result");
            resultObj.transform.SetParent(item.transform, false);

            RectTransform resRT = resultObj.AddComponent<RectTransform>();
            resRT.anchorMin = new Vector2(1, 1);
            resRT.anchorMax = new Vector2(1, 1);
            resRT.pivot = new Vector2(1, 1);
            resRT.sizeDelta = new Vector2(130, 30);
            resRT.anchoredPosition = new Vector2(-15, -15);

            TextMeshProUGUI resText = resultObj.AddComponent<TextMeshProUGUI>();
            resText.text = isWin ? "VICTORIA" : "DERROTA";
            resText.fontSize = 18;
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
            amtRT.sizeDelta = new Vector2(130, 40);
            amtRT.anchoredPosition = new Vector2(-15, 0);

            TextMeshProUGUI amtText = amountObj.AddComponent<TextMeshProUGUI>();
            string amountStr = netAmount >= 0 ? $"+${netAmount:F2}" : $"-${Mathf.Abs(netAmount):F2}";
            amtText.text = amountStr;
            amtText.fontSize = 28;
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
            entryRT.sizeDelta = new Vector2(130, 25);
            entryRT.anchoredPosition = new Vector2(-15, 15);

            TextMeshProUGUI entryText = entryObj.AddComponent<TextMeshProUGUI>();
            entryText.text = $"Entrada: ${entryFee:F0}";
            entryText.fontSize = 14;
            entryText.color = TEXT_SECONDARY;
            entryText.alignment = TextAlignmentOptions.Right;

            return item;
        }

        #endregion
    }
}
