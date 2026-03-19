using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;
using DigitPark.Monetization;
using DigitPark.Editor.AutoAssigners;
using DigitPark.Themes;
using ET = DigitPark.Themes.ThemeApplier.ElementType;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para reconstruir la UI de Scores/Rankings con diseño profesional neón
    /// Incluye: Game Selector (5 juegos), Tabs Nacional/Mundial, Cards con avatar
    /// Resolución: Portrait 9:16 (1080x1920)
    /// </summary>
    public class ScoresUIBuilder : EditorWindow
    {
        // Colores del tema neón
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.5f, 1f);
        private static readonly Color CYAN_GLOW = new Color(0f, 0.83f, 1f, 1f);
        private static readonly Color DARK_BG = new Color(0.02f, 0.04f, 0.08f, 1f);
        private static readonly Color PANEL_BG = new Color(0.05f, 0.1f, 0.15f, 0.95f);
        private static readonly Color TAB_INACTIVE = new Color(0.15f, 0.2f, 0.25f, 1f);
        private static readonly Color TAB_ACTIVE = new Color(0f, 0.83f, 1f, 1f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color SILVER = new Color(0.75f, 0.75f, 0.75f, 1f);
        private static readonly Color BRONZE = new Color(0.8f, 0.5f, 0.2f, 1f);
        private static readonly Color TIME_COLOR = new Color(0f, 1f, 0.53f, 1f);
        private static readonly Color GAME_BTN_NORMAL = new Color(0.08f, 0.12f, 0.18f, 0.9f);
        private static readonly Color GAME_BTN_SELECTED = new Color(0f, 0.83f, 1f, 0.3f);

        private const string BACK_BUTTON_PREFAB = "Assets/_Project/Prefabs/Common/BackButton.prefab";

        // 5 juegos (sin CognitiveSprint)
        private static readonly string[] GAME_IDS = { "DigitRush", "FlashTap", "MemoryPairs", "OddOneOut", "QuickMath" };
        private static readonly string[] GAME_LABELS = { "Digit\nRush", "Flash\nTap", "Memory\nPairs", "Odd One\nOut", "Quick\nMath" };

        [MenuItem("DigitPark/Scenes/Build Scene/Social/Scores", false, 157)]
        public static void ShowWindow()
        {
            GetWindow<ScoresUIBuilder>("Scores UI Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Scores/Rankings UI Builder", EditorStyles.boldLabel);
            GUILayout.Label("Resolución: Portrait 9:16 (1080x1920)", EditorStyles.miniLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Este script reconstruirá la UI de Scores/Rankings.\n" +
                "Asegúrate de tener la escena Scores abierta.\n\n" +
                "Elementos que se crearán:\n" +
                "• Header con título RANKINGS\n" +
                "• Game Selector (5 juegos con iconos)\n" +
                "• Tabs Nacional/Mundial\n" +
                "• 5 sample entries con avatar y medallas\n" +
                "• Estado vacío atractivo\n" +
                "• Panel Tu Posición (footer)\n" +
                "• Loading panel",
                MessageType.Info);

            GUILayout.Space(10);

            if (GUILayout.Button("Reconstruir Scores UI", GUILayout.Height(40)))
            {
                RebuildScoresUI();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Solo crear Estado Vacío", GUILayout.Height(30)))
            {
                CreateEmptyStateOnly();
            }

            GUILayout.Space(15);
            GUI.backgroundColor = new Color(0.5f, 0.8f, 1f);
            if (GUILayout.Button("Auto-Asignar Referencias", GUILayout.Height(30)))
            {
                LeaderboardReferenceAssigner.RunAutoAssign();
            }
            GUI.backgroundColor = Color.white;
        }

        private static void RebuildScoresUI()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("[ScoresUI] No se encontró Canvas en la escena");
                return;
            }

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
            }

            Transform canvasTransform = canvas.transform;

            // Buscar o crear Background
            Transform background = canvasTransform.Find("Background");
            if (background == null)
            {
                CreateBackground(canvasTransform);
            }

            // Limpiar elementos viejos
            CleanOldElements(canvasTransform);

            // Crear nueva estructura
            CreateScoresLayout(canvasTransform);

            Debug.Log("[ScoresUI] Scores UI reconstruida exitosamente!");
            EditorUtility.SetDirty(canvas.gameObject);

            // Auto-asignar referencias al Manager
            LeaderboardReferenceAssigner.RunAutoAssign();
        }

        private static void CreateEmptyStateOnly()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("[ScoresUI] No se encontró Canvas en la escena");
                return;
            }

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
            }

            Transform scoresPanel = canvas.transform.Find("ScoresPanel");
            if (scoresPanel == null)
            {
                Debug.LogError("[ScoresUI] No se encontró ScoresPanel. Ejecuta 'Reconstruir Scores UI' primero.");
                return;
            }

            Transform existingEmpty = scoresPanel.Find("EmptyState");
            if (existingEmpty != null)
                DestroyImmediate(existingEmpty.gameObject);

            CreateEmptyState(scoresPanel);
            Debug.Log("[ScoresUI] Estado vacío creado!");
        }

        private static void CleanOldElements(Transform canvasTransform)
        {
            string[] oldElements = new string[]
            {
                "Header", "HeaderPanel", "TitleText", "Title",
                "GameSelectorPanel",
                "TabsPanel", "TabsContainer", "Tabs",
                "ScoresPanel", "LeaderboardPanel", "ContentPanel",
                "EmptyState", "EmptyMessage", "NoDataText",
                "PlayerPositionPanel", "YourPosition",
                "LoadingPanel", "LoadingIndicator"
            };

            foreach (string elementName in oldElements)
            {
                Transform element = canvasTransform.Find(elementName);
                if (element != null)
                {
                    Debug.Log($"[ScoresUI] Limpiando: {elementName}");
                    DestroyImmediate(element.gameObject);
                }
            }
        }

        private static void CreateBackground(Transform parent)
        {
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(parent, false);
            bg.transform.SetAsFirstSibling();

            RectTransform bgRT = bg.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = Color.white; // ThemeApplier tints at runtime
            bgImage.raycastTarget = false;
            ThemeApplierHelper.Apply(bg, ET.PrimaryBackground);
        }

        // Layout: Header(100) -> ScoresPanel(rest) containing GameSelector(120) -> Tabs(70) -> ScrollView -> PlayerPosition(80)
        private static void CreateScoresLayout(Transform canvasTransform)
        {
            // ========== HEADER (100px from top) ==========
            CreateHeader(canvasTransform);

            // ========== SCORES PANEL (contenedor principal, debajo del header) ==========
            GameObject scoresPanel = CreateOrFind(canvasTransform, "ScoresPanel");
            RectTransform scoresPanelRT = SetupRectTransform(scoresPanel,
                new Vector2(0, 0), new Vector2(1, 1),
                Vector2.zero, Vector2.zero);
            scoresPanelRT.offsetMin = new Vector2(0, 0);
            scoresPanelRT.offsetMax = new Vector2(0, -130); // Debajo del header (29px margin + 100px header + 1px gap)

            // ========== GAME SELECTOR (120px from top of ScoresPanel) ==========
            CreateGameSelectorPanel(scoresPanel.transform);

            // ========== TABS (70px, debajo del game selector) ==========
            CreateTabs(scoresPanel.transform);

            // ========== SCROLL VIEW CON CONTENIDO ==========
            CreateScrollView(scoresPanel.transform);

            // ========== EMPTY STATE ==========
            CreateEmptyState(scoresPanel.transform);

            // ========== SAMPLE ENTRIES (for Editor preview) ==========
            Transform leaderboardContainer = scoresPanel.transform.Find("LeaderboardScrollView/Viewport/LeaderboardContainer");
            if (leaderboardContainer != null)
                CreateSampleEntries(leaderboardContainer.gameObject);

            // ========== TU POSICIÓN (footer fijo, 80px) ==========
            CreatePlayerPositionPanel(scoresPanel.transform);

            // ========== LOADING PANEL (overlay) ==========
            CreateLoadingPanel(scoresPanel.transform);
        }

        #region Header

        private static void CreateHeader(Transform parent)
        {
            GameObject header = CreateOrFind(parent, "Header");
            RectTransform headerRT = SetupRectTransform(header,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -79), new Vector2(0, 100));

            Image headerBg = header.GetComponent<Image>();
            if (headerBg == null) headerBg = header.AddComponent<Image>();
            headerBg.color = new Color(0.03f, 0.08f, 0.12f, 0.98f);

            // Back Button - Neon Cyan prefab
            GameObject backBtnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BACK_BUTTON_PREFAB);
            GameObject backBtn;
            if (backBtnPrefab != null)
            {
                Transform oldBtn = header.transform.Find("BackButton");
                if (oldBtn != null) DestroyImmediate(oldBtn.gameObject);
                backBtn = (GameObject)PrefabUtility.InstantiatePrefab(backBtnPrefab, header.transform);
                backBtn.name = "BackButton";
            }
            else
            {
                backBtn = CreateOrFind(header.transform, "BackButton");
                Image fallbackBg = backBtn.GetComponent<Image>();
                if (fallbackBg == null) fallbackBg = backBtn.AddComponent<Image>();
                fallbackBg.color = new Color(0, 0, 0, 0);
                if (backBtn.GetComponent<Button>() == null) backBtn.AddComponent<Button>();
                Debug.LogWarning("[ScoresUI] BackButton prefab not found, using fallback");
            }
            RectTransform backRT = backBtn.GetComponent<RectTransform>();
            if (backRT == null) backRT = backBtn.AddComponent<RectTransform>();
            backRT.anchorMin = new Vector2(0, 0.5f);
            backRT.anchorMax = new Vector2(0, 0.5f);
            backRT.pivot = new Vector2(0, 0.5f);
            backRT.anchoredPosition = new Vector2(20, 0);
            backRT.sizeDelta = new Vector2(50, 50);

            // Título RANKINGS
            GameObject title = CreateOrFind(header.transform, "TitleText");
            RectTransform titleRT = SetupRectTransform(title,
                new Vector2(0.07f, 0f), new Vector2(0.53f, 1f),
                Vector2.zero, Vector2.zero);
            titleRT.pivot = new Vector2(0.5f, 0.5f);
            titleRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI titleTMP = SetupText(title, "RANKINGS", (int)FontSizes.H4, CYAN_NEON, FontStyles.Bold);
            titleTMP.alignment = TextAlignmentOptions.MidlineLeft;
            titleTMP.raycastTarget = false;
            titleTMP.enableAutoSizing = true;
            titleTMP.fontSizeMin = FontSizes.AutoMinTitle;
            titleTMP.fontSizeMax = FontSizes.H4;
            titleTMP.overflowMode = TextOverflowModes.Ellipsis;

            AddTextGlow(title, CYAN_NEON);

            // Currency pills (top-right of header)
            var pills = CurrencyHeaderBarHelper.CreateCurrencyPills(header.transform);
            var pillsRT = pills.GetComponent<RectTransform>();
            pillsRT.anchorMin = new Vector2(0.52f, 0.5f);
            pillsRT.anchorMax = new Vector2(0.95f, 0.5f);
            pillsRT.pivot = new Vector2(0.5f, 0.5f);
            pillsRT.sizeDelta = new Vector2(0, 65);
        }

        #endregion

        #region Game Selector

        private static void CreateGameSelectorPanel(Transform parent)
        {
            GameObject selectorPanel = CreateOrFind(parent, "GameSelectorPanel");
            RectTransform selectorRT = SetupRectTransform(selectorPanel,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -120), new Vector2(-20, 240));

            // HorizontalLayoutGroup para los 5 botones
            HorizontalLayoutGroup hlg = selectorPanel.GetComponent<HorizontalLayoutGroup>();
            if (hlg == null) hlg = selectorPanel.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 24;
            hlg.padding = new RectOffset(15, 15, 5, 5);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // Limpiar hijos existentes
            for (int i = selectorPanel.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(selectorPanel.transform.GetChild(i).gameObject);
            }

            // Crear 5 botones de juego (sin CognitiveSprint)
            for (int i = 0; i < GAME_IDS.Length; i++)
            {
                CreateGameButton(selectorPanel.transform, i);
            }

            Debug.Log("[ScoresUIBuilder] GameSelectorPanel creado con 5 juegos");
        }

        private static void CreateGameButton(Transform parent, int index)
        {
            string gameId = GAME_IDS[index];
            string label = GAME_LABELS[index];
            bool isSelected = index == 0; // DigitRush seleccionado por defecto

            GameObject btn = new GameObject($"GameButton_{gameId}");
            btn.transform.SetParent(parent, false);

            RectTransform btnRT = btn.AddComponent<RectTransform>();

            LayoutElement le = btn.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;
            le.minWidth = 160;
            le.minHeight = 200;

            // Fondo del botón
            Image btnBg = btn.AddComponent<Image>();
            btnBg.color = isSelected ? GAME_BTN_SELECTED : GAME_BTN_NORMAL;

            // Outline neón para seleccionado
            Outline outline = btn.AddComponent<Outline>();
            outline.effectColor = isSelected ? CYAN_NEON : new Color(0.3f, 0.3f, 0.4f, 0.3f);
            outline.effectDistance = new Vector2(1.5f, 1.5f);
            var btnShadow = btn.AddComponent<Shadow>();
            btnShadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            btnShadow.effectDistance = new Vector2(3, -4);

            // Button component
            Button button = btn.AddComponent<Button>();
            button.targetGraphic = btnBg;

            // VerticalLayoutGroup para icono + texto
            VerticalLayoutGroup vlg = btn.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 4;
            vlg.padding = new RectOffset(5, 5, 8, 5);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = false;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;

            // Icono del juego (placeholder - se cargará en runtime con Resources.Load)
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(btn.transform, false);
            RectTransform iconRT = iconObj.AddComponent<RectTransform>();
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.color = isSelected ? Color.white : new Color(0.6f, 0.6f, 0.7f, 1f);
            iconImg.raycastTarget = false;

            // Cargar icono del juego desde Assets
            string iconPath = $"Assets/_Project/Art/Icons/Games/{gameId}Icon.png";
            Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            if (iconSprite != null)
            {
                iconImg.sprite = iconSprite;
                iconImg.preserveAspect = true;
            }

            LayoutElement iconLE = iconObj.AddComponent<LayoutElement>();
            iconLE.minWidth = 100;
            iconLE.minHeight = 100;
            iconLE.preferredWidth = 100;
            iconLE.preferredHeight = 100;

            // Game name label below
            GameObject nameObj = new GameObject("Label");
            nameObj.transform.SetParent(btn.transform, false);
            RectTransform nameRT = nameObj.AddComponent<RectTransform>();
            TextMeshProUGUI nameTMP = nameObj.AddComponent<TextMeshProUGUI>();
            nameTMP.text = label;
            nameTMP.fontSize = FontSizes.Body;
            nameTMP.fontSizeMin = FontSizes.AutoMinBody;
            nameTMP.enableAutoSizing = true;
            nameTMP.fontStyle = FontStyles.Bold;
            nameTMP.color = isSelected ? Color.white : new Color(0.5f, 0.5f, 0.6f, 1f);
            nameTMP.alignment = TextAlignmentOptions.Center;
            nameTMP.enableWordWrapping = true;
            nameTMP.raycastTarget = false;

            LayoutElement nameLE = nameObj.AddComponent<LayoutElement>();
            nameLE.minWidth = 165;
            nameLE.minHeight = 56;
            nameLE.preferredWidth = 165;
            nameLE.preferredHeight = 56;
        }

        #endregion

        #region Tabs

        private static void CreateTabs(Transform parent)
        {
            GameObject tabsContainer = CreateOrFind(parent, "TabsContainer");
            // Positioned below GameSelector: 120 (game selector) + some spacing
            RectTransform tabsRT = SetupRectTransform(tabsContainer,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -335), new Vector2(-40, 55));

            Image tabsBg = tabsContainer.GetComponent<Image>();
            if (tabsBg == null) tabsBg = tabsContainer.AddComponent<Image>();
            tabsBg.color = new Color(0.05f, 0.1f, 0.15f, 0.9f);

            Outline tabsOutline = tabsContainer.GetComponent<Outline>();
            if (tabsOutline == null) tabsOutline = tabsContainer.AddComponent<Outline>();
            tabsOutline.effectColor = CYAN_DARK;
            tabsOutline.effectDistance = new Vector2(1.5f, 1.5f);

            // Limpiar hijos existentes
            for (int i = tabsContainer.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(tabsContainer.transform.GetChild(i).gameObject);
            }

            HorizontalLayoutGroup hlg = tabsContainer.GetComponent<HorizontalLayoutGroup>();
            if (hlg == null) hlg = tabsContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 2;
            hlg.padding = new RectOffset(0, 0, 0, 0);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            CreateTabButton(tabsContainer.transform, "NationalTab", "National", true);
            CreateTabButton(tabsContainer.transform, "GlobalTab", "Global", false);

            // Divisor vertical centrado
            GameObject divider = CreateOrFind(tabsContainer.transform, "TabDivider");
            RectTransform divRT = divider.GetComponent<RectTransform>();
            if (divRT == null) divRT = divider.AddComponent<RectTransform>();
            divRT.anchorMin = new Vector2(0.5f, 0.1f);
            divRT.anchorMax = new Vector2(0.5f, 0.9f);
            divRT.pivot = new Vector2(0.5f, 0.5f);
            divRT.sizeDelta = new Vector2(2, 0);
            divRT.anchoredPosition = Vector2.zero;

            Image divImg = divider.GetComponent<Image>();
            if (divImg == null) divImg = divider.AddComponent<Image>();
            divImg.color = CYAN_NEON;
            divImg.raycastTarget = false;

            LayoutElement divLE = divider.GetComponent<LayoutElement>();
            if (divLE == null) divLE = divider.AddComponent<LayoutElement>();
            divLE.ignoreLayout = true;
        }

        private static void CreateTabButton(Transform parent, string name, string text, bool isActive)
        {
            GameObject tab = new GameObject(name);
            tab.transform.SetParent(parent, false);

            RectTransform tabRT = tab.AddComponent<RectTransform>();

            LayoutElement le = tab.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;
            le.minHeight = 50;

            Image tabBg = tab.AddComponent<Image>();
            tabBg.color = isActive ? TAB_ACTIVE : TAB_INACTIVE;

            Button tabBtn = tab.AddComponent<Button>();
            tabBtn.targetGraphic = tabBg;

            ColorBlock colors = tabBtn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            tabBtn.colors = colors;

            GameObject tabText = new GameObject("Text");
            tabText.transform.SetParent(tab.transform, false);
            RectTransform textRT = tabText.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;
            textRT.anchoredPosition = Vector2.zero;

            Color textColor = isActive ? DARK_BG : Color.white;
            TextMeshProUGUI tabTMP = tabText.AddComponent<TextMeshProUGUI>();
            tabTMP.text = text;
            tabTMP.fontSize = FontSizes.Body;
            tabTMP.color = textColor;
            tabTMP.fontStyle = FontStyles.Bold;
            tabTMP.alignment = TextAlignmentOptions.Center;
            tabTMP.raycastTarget = false;
            tabTMP.enableAutoSizing = true;
            tabTMP.fontSizeMin = FontSizes.AutoMinBody;
            tabTMP.fontSizeMax = FontSizes.Body;
            tabTMP.overflowMode = TextOverflowModes.Ellipsis;
        }

        #endregion

        #region ScrollView

        private static void CreateScrollView(Transform parent)
        {
            GameObject scrollView = CreateOrFind(parent, "LeaderboardScrollView");
            RectTransform scrollRT = SetupRectTransform(scrollView,
                new Vector2(0, 0), new Vector2(1, 1),
                Vector2.zero, Vector2.zero);
            // Top: GameSelector(120) + Tabs(55) + spacing = ~195
            // Bottom: PlayerPosition(80) + spacing = ~100
            scrollRT.offsetMin = new Vector2(10, 180);
            scrollRT.offsetMax = new Vector2(-10, -375);

            Mask scrollMask = scrollView.GetComponent<Mask>();
            if (scrollMask != null) DestroyImmediate(scrollMask);

            ScrollRect scroll = scrollView.GetComponent<ScrollRect>();
            if (scroll == null) scroll = scrollView.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.elasticity = 0.1f;
            scroll.scrollSensitivity = 50f;

            // Viewport
            GameObject viewport = CreateOrFind(scrollView.transform, "Viewport");
            RectTransform viewportRT = SetupRectTransform(viewport, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            RectMask2D rectMask = viewport.GetComponent<RectMask2D>();
            if (rectMask == null) rectMask = viewport.AddComponent<RectMask2D>();

            Mask oldMask = viewport.GetComponent<Mask>();
            if (oldMask != null) DestroyImmediate(oldMask);

            Image oldImg = viewport.GetComponent<Image>();
            if (oldImg != null) DestroyImmediate(oldImg);

            // Content (LeaderboardContainer)
            GameObject content = CreateOrFind(viewport.transform, "LeaderboardContainer");
            RectTransform contentRT = SetupRectTransform(content,
                new Vector2(0, 1), new Vector2(1, 1),
                Vector2.zero, new Vector2(0, 0));
            contentRT.pivot = new Vector2(0.5f, 1);

            VerticalLayoutGroup vlg = content.GetComponent<VerticalLayoutGroup>();
            if (vlg == null) vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6;
            vlg.padding = new RectOffset(0, 0, 8, 8);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            ContentSizeFitter csf = content.GetComponent<ContentSizeFitter>();
            if (csf == null) csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            scroll.viewport = viewportRT;
            scroll.content = contentRT;
        }

        #endregion

        #region Empty State

        private static void CreateEmptyState(Transform parent)
        {
            GameObject emptyState = CreateOrFind(parent, "EmptyState");
            RectTransform emptyRT = SetupRectTransform(emptyState,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 50), new Vector2(600, 500));

            GameObject icon = CreateOrFind(emptyState.transform, "TrophyIcon");
            SetupRectTransform(icon,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -30), new Vector2(150, 150));
            TextMeshProUGUI iconTMP = SetupText(icon, "\u2B50", (int)FontSizes.Symbol, GOLD, FontStyles.Bold);
            iconTMP.alignment = TextAlignmentOptions.Center;

            GameObject title = CreateOrFind(emptyState.transform, "EmptyTitle");
            SetupRectTransform(title,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 30), new Vector2(500, 60));
            TextMeshProUGUI titleTMP = SetupText(title, "No Rankings Yet", (int)FontSizes.Body, Color.white, FontStyles.Bold);
            titleTMP.alignment = TextAlignmentOptions.Center;

            GameObject subtitle = CreateOrFind(emptyState.transform, "EmptySubtitle");
            SetupRectTransform(subtitle,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -30), new Vector2(450, 50));
            TextMeshProUGUI subTMP = SetupText(subtitle, "Play a game to appear on the leaderboard", (int)FontSizes.Body, new Color(0.6f, 0.6f, 0.6f), FontStyles.Bold);
            subTMP.alignment = TextAlignmentOptions.Center;

            GameObject playBtn = CreateOrFind(emptyState.transform, "PlayButton");
            SetupRectTransform(playBtn,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 80), new Vector2(280, 70));

            Image playBg = playBtn.GetComponent<Image>();
            if (playBg == null) playBg = playBtn.AddComponent<Image>();
            playBg.color = CYAN_GLOW;

            Outline playOutline = playBtn.GetComponent<Outline>();
            if (playOutline == null) playOutline = playBtn.AddComponent<Outline>();
            playOutline.effectColor = CYAN_NEON;
            playOutline.effectDistance = new Vector2(2, 2);

            Button playButton = playBtn.GetComponent<Button>();
            if (playButton == null) playButton = playBtn.AddComponent<Button>();
            playButton.targetGraphic = playBg;

            ColorBlock colors = playButton.colors;
            colors.normalColor = CYAN_GLOW;
            colors.highlightedColor = CYAN_NEON;
            colors.pressedColor = CYAN_DARK;
            playButton.colors = colors;

            GameObject playText = CreateOrFind(playBtn.transform, "Text");
            SetupRectTransform(playText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI playTMP = SetupText(playText, "play_now", (int)FontSizes.Body, DARK_BG, FontStyles.Bold);
            playTMP.alignment = TextAlignmentOptions.Center;

            emptyState.SetActive(false);
        }

        #endregion

        #region Sample Entries

        private static void CreateSampleEntries(GameObject container)
        {
            string[] names = { "ProGamer99", "NeonMaster", "SpeedKing", "DigitPro", "QuickMind" };
            string[] times = { "3.450s", "3.812s", "4.103s", "5.287s", "6.420s" };

            for (int i = 0; i < 5; i++)
            {
                GameObject entry = FindOrCreateChild(container, $"SampleEntry_{i + 1}");

                LayoutElement entryLE = GetOrAddComponent<LayoutElement>(entry);
                entryLE.minHeight = 140;
                entryLE.preferredHeight = 140;

                Image entryBg = GetOrAddComponent<Image>(entry);
                entryBg.color = i % 2 == 0
                    ? new Color(0.06f, 0.1f, 0.16f, 0.95f)
                    : new Color(0.04f, 0.08f, 0.13f, 0.95f);
                var entryShadow = entry.AddComponent<Shadow>();
                entryShadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
                entryShadow.effectDistance = new Vector2(3, -4);

                // Top 3: borde sutil de medalla
                if (i < 3)
                {
                    Outline medalOutline = GetOrAddComponent<Outline>(entry);
                    Color medalColor = i == 0 ? GOLD : i == 1 ? SILVER : BRONZE;
                    medalOutline.effectColor = new Color(medalColor.r, medalColor.g, medalColor.b, 0.4f);
                    medalOutline.effectDistance = new Vector2(1, 1);
                }

                HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(entry);
                hlg.spacing = 24;
                hlg.padding = new RectOffset(30, 30, 16, 16);
                hlg.childAlignment = TextAnchor.MiddleLeft;
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth = false;

                // Position (60px)
                GameObject posObj = FindOrCreateChild(entry, "PositionText");
                TextMeshProUGUI posText = GetOrAddComponent<TextMeshProUGUI>(posObj);
                posText.text = i < 3 ? $"{i + 1}" : $"#{i + 1}";
                posText.fontSize = i < 3 ? FontSizes.H3 : FontSizes.Subtitle;
                posText.fontStyle = FontStyles.Bold;
                posText.alignment = TextAlignmentOptions.Center;
                posText.enableAutoSizing = true;
                posText.fontSizeMin = FontSizes.AutoMinBody;
                posText.fontSizeMax = i < 3 ? FontSizes.H3 : FontSizes.Subtitle;
                posText.overflowMode = TextOverflowModes.Ellipsis;
                LayoutElement posLE = GetOrAddComponent<LayoutElement>(posObj);
                posLE.minWidth = 110;
                posLE.preferredWidth = 110;

                if (i == 0) posText.color = GOLD;
                else if (i == 1) posText.color = SILVER;
                else if (i == 2) posText.color = BRONZE;
                else posText.color = new Color(0.5f, 0.55f, 0.65f, 1f);

                // Circular avatar frame
                Sprite circleSprite = GenerateCircleSprite();

                GameObject avatarFrame = FindOrCreateChild(entry, "AvatarFrame");
                Image frameImg = GetOrAddComponent<Image>(avatarFrame);
                frameImg.sprite = circleSprite;
                frameImg.color = new Color(0.15f, 0.2f, 0.3f, 1f);
                frameImg.raycastTarget = false;
                var fr_scores = avatarFrame.AddComponent<DigitPark.Services.FrameRenderer>();
                fr_scores.SetRenderMode(DigitPark.Services.FrameRenderer.RenderMode.Reduced);
                LayoutElement avatarLE = GetOrAddComponent<LayoutElement>(avatarFrame);
                avatarLE.minWidth = 100;
                avatarLE.minHeight = 100;
                avatarLE.preferredWidth = 100;
                avatarLE.preferredHeight = 100;

                // Circular mask
                GameObject avatarMask = FindOrCreateChild(avatarFrame, "AvatarMask");
                RectTransform amRT = GetOrAddComponent<RectTransform>(avatarMask);
                amRT.anchorMin = new Vector2(0.06f, 0.06f);
                amRT.anchorMax = new Vector2(0.94f, 0.94f);
                amRT.offsetMin = Vector2.zero;
                amRT.offsetMax = Vector2.zero;
                Image amImg = GetOrAddComponent<Image>(avatarMask);
                amImg.sprite = circleSprite;
                amImg.color = new Color(0.2f, 0.25f, 0.35f, 1f);
                GetOrAddComponent<Mask>(avatarMask).showMaskGraphic = true;

                // Avatar image (clipped to circle)
                GameObject avatarObj = FindOrCreateChild(avatarMask, "AvatarImage");
                Image avatarImg = GetOrAddComponent<Image>(avatarObj);
                avatarImg.color = Color.white;
                avatarImg.raycastTarget = false;
                avatarImg.preserveAspect = true;
                RectTransform aiRT = GetOrAddComponent<RectTransform>(avatarObj);
                aiRT.anchorMin = Vector2.zero;
                aiRT.anchorMax = Vector2.one;
                aiRT.offsetMin = Vector2.zero;
                aiRT.offsetMax = Vector2.zero;
                Sprite defaultAvatar = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/Social/AvatarDefault.png");
                if (defaultAvatar != null) avatarImg.sprite = defaultAvatar;

                // Username (flex)
                GameObject nameObj = FindOrCreateChild(entry, "UsernameText");
                TextMeshProUGUI nameText = GetOrAddComponent<TextMeshProUGUI>(nameObj);
                nameText.text = names[i];
                nameText.fontSize = FontSizes.BodyLarge;
                nameText.fontStyle = FontStyles.Bold;
                nameText.color = Color.white;
                nameText.alignment = TextAlignmentOptions.MidlineLeft;
                nameText.enableWordWrapping = false;
                nameText.overflowMode = TextOverflowModes.Ellipsis;
                nameText.enableAutoSizing = true;
                nameText.fontSizeMin = FontSizes.AutoMinBody;
                nameText.fontSizeMax = FontSizes.BodyLarge;
                LayoutElement nameLE = GetOrAddComponent<LayoutElement>(nameObj);
                nameLE.flexibleWidth = 1;
                nameLE.minWidth = 240;

                // Time (100px)
                GameObject timeObj = FindOrCreateChild(entry, "TimeText");
                TextMeshProUGUI timeText = GetOrAddComponent<TextMeshProUGUI>(timeObj);
                timeText.text = times[i];
                timeText.fontSize = FontSizes.BodyLarge;
                timeText.fontStyle = FontStyles.Bold;
                timeText.color = TIME_COLOR;
                timeText.alignment = TextAlignmentOptions.MidlineRight;
                timeText.enableAutoSizing = true;
                timeText.fontSizeMin = FontSizes.AutoMinBody;
                timeText.fontSizeMax = FontSizes.BodyLarge;
                timeText.overflowMode = TextOverflowModes.Ellipsis;
                LayoutElement timeLE = GetOrAddComponent<LayoutElement>(timeObj);
                timeLE.minWidth = 200;
                timeLE.preferredWidth = 200;
            }

            Debug.Log("[ScoresUIBuilder] 5 sample entries creadas con avatar");
        }

        #endregion

        #region Player Position Panel

        private static void CreatePlayerPositionPanel(Transform parent)
        {
            GameObject posPanel = CreateOrFind(parent, "PlayerPositionPanel");
            RectTransform posRT = SetupRectTransform(posPanel,
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 50), new Vector2(-20, 160));

            Image posBg = posPanel.GetComponent<Image>();
            if (posBg == null) posBg = posPanel.AddComponent<Image>();
            posBg.color = new Color(0.03f, 0.08f, 0.12f, 0.98f);

            GameObject topLine = CreateOrFind(posPanel.transform, "TopLine");
            SetupRectTransform(topLine,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, 0), new Vector2(0, 2));
            Image topLineImg = topLine.GetComponent<Image>();
            if (topLineImg == null) topLineImg = topLine.AddComponent<Image>();
            topLineImg.color = CYAN_NEON;

            HorizontalLayoutGroup hlg = posPanel.GetComponent<HorizontalLayoutGroup>();
            if (hlg == null) hlg = posPanel.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 30;
            hlg.padding = new RectOffset(40, 40, 20, 20);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            // Etiqueta "TU POSICIÓN"
            GameObject label = CreateOrFind(posPanel.transform, "ScoresPositionLabel");
            TextMeshProUGUI labelTMP = SetupText(label, "YOUR POSITION", (int)FontSizes.Body, new Color(0.6f, 0.6f, 0.6f), FontStyles.Bold);
            labelTMP.alignment = TextAlignmentOptions.Center;
            LayoutElement labelLE = label.GetComponent<LayoutElement>();
            if (labelLE == null) labelLE = label.AddComponent<LayoutElement>();
            labelLE.preferredWidth = 420;

            // Número de posición
            GameObject posNumber = CreateOrFind(posPanel.transform, "PositionNumber");
            TextMeshProUGUI numTMP = SetupText(posNumber, "#--", (int)FontSizes.H1, GOLD, FontStyles.Bold);
            numTMP.alignment = TextAlignmentOptions.Center;
            LayoutElement numLE = posNumber.GetComponent<LayoutElement>();
            if (numLE == null) numLE = posNumber.AddComponent<LayoutElement>();
            numLE.flexibleWidth = 1;
            numLE.preferredWidth = 300;

            AddTextGlow(posNumber, GOLD);

            // Tiempo del jugador
            GameObject posTime = CreateOrFind(posPanel.transform, "PositionTime");
            TextMeshProUGUI timeTMP = SetupText(posTime, "--", (int)FontSizes.Subtitle, TIME_COLOR, FontStyles.Bold);
            timeTMP.alignment = TextAlignmentOptions.Center;
            LayoutElement timeLE = posTime.GetComponent<LayoutElement>();
            if (timeLE == null) timeLE = posTime.AddComponent<LayoutElement>();
            timeLE.preferredWidth = 200;
        }

        #endregion

        #region Loading Panel

        private static void CreateLoadingPanel(Transform parent)
        {
            GameObject loadingPanel = CreateOrFind(parent, "LoadingPanel");
            RectTransform loadingRT = SetupRectTransform(loadingPanel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(300, 150));

            Image loadingBg = loadingPanel.GetComponent<Image>();
            if (loadingBg == null) loadingBg = loadingPanel.AddComponent<Image>();
            loadingBg.color = new Color(0.02f, 0.05f, 0.1f, 0.9f);

            Outline loadingOutline = loadingPanel.GetComponent<Outline>();
            if (loadingOutline == null) loadingOutline = loadingPanel.AddComponent<Outline>();
            loadingOutline.effectColor = CYAN_DARK;
            loadingOutline.effectDistance = new Vector2(1, 1);

            GameObject loadingText = CreateOrFind(loadingPanel.transform, "LoadingText");
            SetupRectTransform(loadingText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI loadingTMP = SetupText(loadingText, "Loading Rankings...", (int)FontSizes.Body, CYAN_NEON, FontStyles.Bold);
            loadingTMP.alignment = TextAlignmentOptions.Center;

            loadingPanel.SetActive(false);
        }

        #endregion

        #region Helpers

        private static void CleanupOldUI()
        {
            string[] toClean = { "Background", "SafeArea" };
            foreach (var canvas in Object.FindObjectsOfType<Canvas>(true))
            {
                if (canvas.transform.parent != null) continue;
                // No tocar TransitionCanvas ni EffectsCanvas
                if (canvas.gameObject.name.Contains("Transition") ||
                    canvas.gameObject.name.Contains("Effects")) continue;
                foreach (string name in toClean)
                {
                    Transform t = canvas.transform.Find(name);
                    if (t != null) Object.DestroyImmediate(t.gameObject);
                }
            }
        }

        private static GameObject CreateOrFind(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject newObj = new GameObject(name);
            newObj.transform.SetParent(parent, false);
            return newObj;
        }

        private static GameObject FindOrCreateChild(GameObject parent, string name)
        {
            Transform existing = parent.transform.Find(name);
            if (existing != null)
                return existing.gameObject;

            GameObject child = new GameObject(name);
            child.transform.SetParent(parent.transform, false);
            child.AddComponent<RectTransform>();
            return child;
        }

        private static T GetOrAddComponent<T>(GameObject obj) where T : Component
        {
            T comp = obj.GetComponent<T>();
            if (comp == null) comp = obj.AddComponent<T>();
            return comp;
        }

        private static RectTransform SetupRectTransform(GameObject obj, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt == null) rt = obj.AddComponent<RectTransform>();

            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;

            return rt;
        }

        private static TextMeshProUGUI SetupText(GameObject obj, string text, int fontSize, Color color, FontStyles style)
        {
            TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = obj.AddComponent<TextMeshProUGUI>();

            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinBody;
            tmp.fontSizeMax = fontSize > 0 ? fontSize : FontSizes.Body;
            tmp.raycastTarget = false;

            return tmp;
        }

        private static void AddTextGlow(GameObject textObj, Color glowColor)
        {
            Shadow shadow = textObj.GetComponent<Shadow>();
            if (shadow == null) shadow = textObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(glowColor.r, glowColor.g, glowColor.b, 0.5f);
            shadow.effectDistance = new Vector2(2, -2);
        }

        private static Sprite GenerateCircleSprite()
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/UI/CircleSprite.png");
            if (s != null) return s;
            // Fallback: generate at runtime (won't survive prefab save)
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size / 2f;
            float radius = center - 1f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dist = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center));
                    if (dist <= radius) tex.SetPixel(x, y, Color.white);
                    else if (dist <= radius + 1f) tex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(radius + 1f - dist)));
                    else tex.SetPixel(x, y, Color.clear);
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        #endregion
    }
}
