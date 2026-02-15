using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Construye la UI del Trophy Showcase para Achievements
    /// Diseño premium con vitrina de trofeos, efectos de cristal y animaciones
    /// </summary>
    public class AchievementsUIBuilder : EditorWindow
    {
        // ==================== COLORES DEL TEMA TROPHY SHOWCASE ====================
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.4f, 1f);
        private static readonly Color CYAN_GLOW = new Color(0f, 1f, 1f, 0.3f);

        private static readonly Color DARK_BG = new Color(0.02f, 0.04f, 0.08f, 1f);
        private static readonly Color PANEL_BG = new Color(0.05f, 0.08f, 0.12f, 0.98f);
        private static readonly Color CARD_BG = new Color(0.04f, 0.07f, 0.11f, 1f);
        private static readonly Color HEADER_BG = new Color(0.03f, 0.05f, 0.08f, 0.95f);
        private static readonly Color POPUP_BG = new Color(0.04f, 0.06f, 0.1f, 0.98f);

        private static readonly Color TEXT_PRIMARY = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.7f, 0.75f, 1f);
        private static readonly Color TEXT_DARK = new Color(0.02f, 0.04f, 0.08f, 1f);

        private static readonly Color BUTTON_SECONDARY = new Color(0.12f, 0.16f, 0.22f, 1f);
        private static readonly Color BUTTON_SUCCESS = new Color(0.2f, 0.8f, 0.4f, 1f);

        // Trophy Colors
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GOLD_DARK = new Color(0.7f, 0.55f, 0f, 1f);
        private static readonly Color SILVER = new Color(0.75f, 0.75f, 0.8f, 1f);
        private static readonly Color BRONZE = new Color(0.8f, 0.5f, 0.2f, 1f);

        // Category Colors
        private static readonly Color CAT_BEGINNER = new Color(0.3f, 0.7f, 1f, 1f);     // Light Blue
        private static readonly Color CAT_GAMES = new Color(0.5f, 1f, 0.5f, 1f);        // Light Green
        private static readonly Color CAT_COMPETITION = new Color(1f, 0.6f, 0.2f, 1f);  // Orange
        private static readonly Color CAT_SECRET = new Color(0.8f, 0.2f, 1f, 1f);       // Purple

        // Trophy Card States
        private static readonly Color TROPHY_UNLOCKED_GLOW = new Color(0f, 1f, 1f, 0.6f);
        private static readonly Color TROPHY_LOCKED_BG = new Color(0.08f, 0.1f, 0.14f, 1f);
        private static readonly Color TROPHY_PROGRESS_GLOW = new Color(1f, 0.84f, 0f, 0.5f);
        private static readonly Color TROPHY_SECRET_GLOW = new Color(0.8f, 0.2f, 1f, 0.5f);

        private static readonly Color GLASS_OVERLAY = new Color(1f, 1f, 1f, 0.08f);
        private static readonly Color BLOCKER_BG = new Color(0f, 0f, 0f, 0.9f);

        // ==================== DIMENSIONES ====================
        private const float HEADER_HEIGHT = 130f;
        private const float TABS_HEIGHT = 60f;
        // 3 columns for mobile (1080px): (1080 - 40 padding - 30 spacing) / 3 = 336
        private const float TROPHY_CARD_WIDTH = 320f;
        private const float TROPHY_CARD_HEIGHT = 380f;
        private const float GRID_SPACING = 15f;
        private const float CONTENT_PADDING = 15f;
        private const float DETAIL_PANEL_HEIGHT = 420f;

        [MenuItem("DigitPark/UI Builders/Monetization/Achievements", false, 185)]
        public static void BuildUI()
        {
            if (!EditorUtility.DisplayDialog("Trophy Showcase Builder",
                "Esto construira la UI del Trophy Showcase para Achievements.\n" +
                "Asegurate de tener la escena Achievements abierta.\n\n" +
                "Incluye:\n" +
                "- Vitrina de trofeos con efecto cristal\n" +
                "- Sistema de categorias\n" +
                "- Animaciones premium\n" +
                "- Panel de detalle\n\n" +
                "Continuar?",
                "Si", "No"))
                return;

            BuildTrophyShowcase();
        }

        private static void BuildTrophyShowcase()
        {
            Debug.Log("[TrophyShowcase] ========== INICIANDO CONSTRUCCION ==========");

            Canvas canvas = SetupCanvas();
            if (canvas == null) return;

            // Clean up old UI before building new one
            CleanupOldUI(canvas);

            CreateBackground(canvas);
            GameObject safeArea = CreateSafeArea(canvas);

            CreateHeader(safeArea);
            CreateCategoryTabs(safeArea);
            CreateTrophyShowcaseGrid(safeArea);
            CreateEmptyState(safeArea);

            CreateDetailPanel(canvas);
            CreateRewardCelebration(canvas);

            CreateTrophyCardPrefab();

            MarkSceneDirty();
            Debug.Log("[TrophyShowcase] ========== CONSTRUCCION COMPLETADA ==========");

            EditorUtility.DisplayDialog("Trophy Showcase Completado",
                "UI del Trophy Showcase creada exitosamente.\n\n" +
                "Elementos creados:\n" +
                "- Header con progreso total\n" +
                "- 5 tabs de categorias\n" +
                "- Grid de trofeos (6 ejemplos)\n" +
                "- Panel de detalle\n" +
                "- Celebracion de recompensa\n" +
                "- Prefab TrophyCard\n\n" +
                "Asigna el AchievementsManager y conecta las referencias.",
                "OK");
        }

        // ==================== CANVAS SETUP ====================

        private static Canvas SetupCanvas()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();

            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;

                canvasObj.AddComponent<GraphicRaycaster>();
            }

            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            if (Camera.main == null)
            {
                GameObject cameraObj = new GameObject("Main Camera");
                Camera cam = cameraObj.AddComponent<Camera>();
                cam.tag = "MainCamera";
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = DARK_BG;
            }

            return canvas;
        }

        /// <summary>
        /// Remove old UI elements before creating new Trophy Showcase
        /// </summary>
        private static void CleanupOldUI(Canvas canvas)
        {
            // No tocar TransitionCanvas ni EffectsCanvas
            if (canvas.gameObject.name.Contains("Transition") ||
                canvas.gameObject.name.Contains("Effects")) return;

            Debug.Log("[TrophyShowcase] Limpiando UI antigua...");

            // List of old element names to remove
            string[] oldElementsToRemove = new string[]
            {
                "SafeArea",
                "Background",
                "Header",
                "StatsPanel",
                "CategoryTabs",
                "CategoryTabsScroll",
                "TabsPanel",
                "AchievementsScrollView",
                "ScrollView",
                "DetailPanelBlocker",
                "RewardCelebration",
                "PurchaseBlocker",
                "NotEnoughBlocker",
                "TrophyShowcaseScrollView"
            };

            // Collect children to destroy (can't modify collection while iterating)
            var childrenToDestroy = new System.Collections.Generic.List<GameObject>();

            foreach (Transform child in canvas.transform)
            {
                // Check if this is an old element to remove
                foreach (string oldName in oldElementsToRemove)
                {
                    if (child.name == oldName || child.name.StartsWith(oldName))
                    {
                        childrenToDestroy.Add(child.gameObject);
                        break;
                    }
                }
            }

            // Destroy collected children
            foreach (var child in childrenToDestroy)
            {
                Debug.Log($"[TrophyShowcase] Eliminando: {child.name}");
                Object.DestroyImmediate(child);
            }

            // Also check for AchievementsManager and clear its references
            var manager = Object.FindObjectOfType<DigitPark.Managers.AchievementsManager>();
            if (manager != null)
            {
                Debug.Log("[TrophyShowcase] AchievementsManager encontrado - las referencias se deben reconectar");
            }

            Debug.Log("[TrophyShowcase] Limpieza completada");
        }

        private static void CreateBackground(Canvas canvas)
        {
            GameObject bg = FindOrCreateChild(canvas.gameObject, "Background");

            RectTransform bgRT = GetOrAddComponent<RectTransform>(bg);
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;

            Image bgImage = GetOrAddComponent<Image>(bg);
            bgImage.color = DARK_BG;

            // Ambient particles effect (visual layer)
            GameObject particles = FindOrCreateChild(bg, "AmbientParticles");
            SetRectTransformStretch(particles);
            Image particlesImage = GetOrAddComponent<Image>(particles);
            particlesImage.color = new Color(0.1f, 0.2f, 0.3f, 0.1f);

            bg.transform.SetAsFirstSibling();
        }

        private static GameObject CreateSafeArea(Canvas canvas)
        {
            GameObject safeArea = FindOrCreateChild(canvas.gameObject, "SafeArea");

            RectTransform safeRT = GetOrAddComponent<RectTransform>(safeArea);
            safeRT.anchorMin = Vector2.zero;
            safeRT.anchorMax = Vector2.one;
            safeRT.sizeDelta = Vector2.zero;

            safeArea.transform.SetSiblingIndex(1);
            return safeArea;
        }

        // ==================== HEADER ====================

        private static void CreateHeader(GameObject parent)
        {
            GameObject header = FindOrCreateChild(parent, "Header");

            RectTransform headerRT = GetOrAddComponent<RectTransform>(header);
            headerRT.anchorMin = new Vector2(0, 1);
            headerRT.anchorMax = new Vector2(1, 1);
            headerRT.pivot = new Vector2(0.5f, 1);
            headerRT.anchoredPosition = Vector2.zero;
            headerRT.sizeDelta = new Vector2(0, HEADER_HEIGHT);

            Image headerBg = GetOrAddComponent<Image>(header);
            headerBg.color = HEADER_BG;

            CreateBottomGlow(header, GOLD);

            // BackButton
            GameObject backBtn = FindOrCreateChild(header, "BackButton");
            RectTransform backRT = GetOrAddComponent<RectTransform>(backBtn);
            backRT.anchorMin = new Vector2(0, 1);
            backRT.anchorMax = new Vector2(0, 1);
            backRT.pivot = new Vector2(0, 1);
            backRT.anchoredPosition = new Vector2(20, -15);
            backRT.sizeDelta = new Vector2(50, 50);

            Image backBg = GetOrAddComponent<Image>(backBtn);
            backBg.color = BUTTON_SECONDARY;

            Button backButton = GetOrAddComponent<Button>(backBtn);
            SetupButtonColors(backButton, BUTTON_SECONDARY);
            AddOutline(backBtn, CYAN_DARK);

            GameObject backTextObj = FindOrCreateChild(backBtn, "Text");
            TextMeshProUGUI backText = GetOrAddComponent<TextMeshProUGUI>(backTextObj);
            backText.text = "<";
            backText.fontSize = 32;
            backText.fontStyle = FontStyles.Bold;
            backText.color = CYAN_NEON;
            backText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(backTextObj);

            // Title Row - Trophy icon AFTER text to not overlap
            GameObject titleRow = FindOrCreateChild(header, "TitleRow");
            RectTransform titleRowRT = GetOrAddComponent<RectTransform>(titleRow);
            titleRowRT.anchorMin = new Vector2(0.5f, 1);
            titleRowRT.anchorMax = new Vector2(0.5f, 1);
            titleRowRT.pivot = new Vector2(0.5f, 1);
            titleRowRT.anchoredPosition = new Vector2(0, -15);
            titleRowRT.sizeDelta = new Vector2(300, 50);

            HorizontalLayoutGroup titleHlg = GetOrAddComponent<HorizontalLayoutGroup>(titleRow);
            titleHlg.spacing = 12;
            titleHlg.childAlignment = TextAnchor.MiddleCenter;
            titleHlg.childControlWidth = false;
            titleHlg.childControlHeight = true;

            // Trophy Icon (BEFORE text in layout)
            GameObject trophyIcon = FindOrCreateChild(titleRow, "TrophyIcon");
            RectTransform trophyRT = GetOrAddComponent<RectTransform>(trophyIcon);
            trophyRT.sizeDelta = new Vector2(40, 40);

            Image trophyImage = GetOrAddComponent<Image>(trophyIcon);
            trophyImage.color = GOLD;

            LayoutElement trophyLE = GetOrAddComponent<LayoutElement>(trophyIcon);
            trophyLE.minWidth = 40;
            trophyLE.minHeight = 40;

            // Title Text (AFTER icon in layout)
            GameObject titleObj = FindOrCreateChild(titleRow, "TitleText");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "LOGROS";
            titleText.fontSize = 78;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = GOLD;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            AddOutline(titleObj, GOLD_DARK, 2);

            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minWidth = 200;
            titleLE.minHeight = 50;

            // Completion Counter (instead of points)
            GameObject completionDisplay = FindOrCreateChild(header, "CompletionDisplay");
            RectTransform completionRT = GetOrAddComponent<RectTransform>(completionDisplay);
            completionRT.anchorMin = new Vector2(1, 1);
            completionRT.anchorMax = new Vector2(1, 1);
            completionRT.pivot = new Vector2(1, 1);
            completionRT.anchoredPosition = new Vector2(-20, -20);
            completionRT.sizeDelta = new Vector2(100, 40);

            TextMeshProUGUI completionText = GetOrAddComponent<TextMeshProUGUI>(completionDisplay);
            completionText.text = "5/17";
            completionText.fontSize = 36;
            completionText.fontStyle = FontStyles.Bold;
            completionText.color = CYAN_NEON;
            completionText.alignment = TextAlignmentOptions.MidlineRight;

            // Progress Section
            GameObject progressSection = FindOrCreateChild(header, "ProgressSection");
            RectTransform progressSectionRT = GetOrAddComponent<RectTransform>(progressSection);
            progressSectionRT.anchorMin = new Vector2(0, 0);
            progressSectionRT.anchorMax = new Vector2(1, 0);
            progressSectionRT.pivot = new Vector2(0.5f, 0);
            progressSectionRT.anchoredPosition = new Vector2(0, 15);
            progressSectionRT.sizeDelta = new Vector2(-40, 45);

            VerticalLayoutGroup progressVlg = GetOrAddComponent<VerticalLayoutGroup>(progressSection);
            progressVlg.spacing = 6;
            progressVlg.padding = new RectOffset(0, 0, 0, 0);
            progressVlg.childAlignment = TextAnchor.MiddleCenter;
            progressVlg.childControlWidth = true;
            progressVlg.childControlHeight = true;

            // Progress Label Row
            GameObject progressLabelRow = FindOrCreateChild(progressSection, "LabelRow");
            HorizontalLayoutGroup labelRowHlg = GetOrAddComponent<HorizontalLayoutGroup>(progressLabelRow);
            labelRowHlg.childAlignment = TextAnchor.MiddleCenter;
            labelRowHlg.childControlWidth = true;
            labelRowHlg.childControlHeight = true;
            labelRowHlg.childForceExpandWidth = true;
            LayoutElement labelRowLE = GetOrAddComponent<LayoutElement>(progressLabelRow);
            labelRowLE.minHeight = 18;

            GameObject progressLabelLeft = FindOrCreateChild(progressLabelRow, "Left");
            TextMeshProUGUI progressLeftText = GetOrAddComponent<TextMeshProUGUI>(progressLabelLeft);
            progressLeftText.text = "Progreso Total";
            progressLeftText.fontSize = 22;
            progressLeftText.fontStyle = FontStyles.Bold;
            progressLeftText.color = TEXT_SECONDARY;
            progressLeftText.alignment = TextAlignmentOptions.MidlineLeft;

            GameObject progressLabelRight = FindOrCreateChild(progressLabelRow, "Right");
            TextMeshProUGUI progressRightText = GetOrAddComponent<TextMeshProUGUI>(progressLabelRight);
            progressRightText.text = "27/50 (54%)";
            progressRightText.fontSize = 22;
            progressRightText.fontStyle = FontStyles.Bold;
            progressRightText.color = CYAN_NEON;
            progressRightText.alignment = TextAlignmentOptions.MidlineRight;

            // Overall Progress Bar (Slider)
            GameObject progressBar = FindOrCreateChild(progressSection, "OverallProgressBar");
            LayoutElement progressBarLE = GetOrAddComponent<LayoutElement>(progressBar);
            progressBarLE.minHeight = 16;
            progressBarLE.preferredHeight = 16;

            Image progressBarBg = GetOrAddComponent<Image>(progressBar);
            progressBarBg.color = new Color(0.08f, 0.1f, 0.14f, 1f);
            AddOutline(progressBar, CYAN_DARK * 0.5f);

            // Add Slider component for auto-assigner
            Slider progressSlider = GetOrAddComponent<Slider>(progressBar);
            progressSlider.minValue = 0;
            progressSlider.maxValue = 1;
            progressSlider.value = 0.54f;
            progressSlider.interactable = false;

            // Progress Fill Area
            GameObject fillArea = FindOrCreateChild(progressBar, "Fill Area");
            SetRectTransformStretch(fillArea);
            RectTransform fillAreaRT = fillArea.GetComponent<RectTransform>();
            fillAreaRT.offsetMin = new Vector2(0, 0);
            fillAreaRT.offsetMax = new Vector2(0, 0);

            // Progress Fill
            GameObject progressFill = FindOrCreateChild(fillArea, "Fill");
            RectTransform fillRT = GetOrAddComponent<RectTransform>(progressFill);
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = new Vector2(0.54f, 1);
            fillRT.sizeDelta = Vector2.zero;

            Image fillImage = GetOrAddComponent<Image>(progressFill);
            fillImage.color = GOLD;

            // Configure slider
            progressSlider.fillRect = fillRT;
            progressSlider.targetGraphic = progressBarBg;

            // Progress Glow
            GameObject progressGlow = FindOrCreateChild(progressBar, "Glow");
            RectTransform glowRT = GetOrAddComponent<RectTransform>(progressGlow);
            glowRT.anchorMin = new Vector2(0.54f, 0);
            glowRT.anchorMax = new Vector2(0.54f, 1);
            glowRT.pivot = new Vector2(0.5f, 0.5f);
            glowRT.sizeDelta = new Vector2(20, 0);

            Image glowImage = GetOrAddComponent<Image>(progressGlow);
            glowImage.color = new Color(1f, 0.9f, 0.5f, 0.6f);

            Debug.Log("[TrophyShowcase] Header creado");
        }

        // ==================== CATEGORY TABS ====================

        private static void CreateCategoryTabs(GameObject parent)
        {
            // Create a scrollable tabs container for 11 categories
            GameObject tabsScrollArea = FindOrCreateChild(parent, "CategoryTabsScroll");

            RectTransform tabsScrollRT = GetOrAddComponent<RectTransform>(tabsScrollArea);
            tabsScrollRT.anchorMin = new Vector2(0, 1);
            tabsScrollRT.anchorMax = new Vector2(1, 1);
            tabsScrollRT.pivot = new Vector2(0.5f, 1);
            tabsScrollRT.anchoredPosition = new Vector2(0, -HEADER_HEIGHT - 10);
            tabsScrollRT.sizeDelta = new Vector2(0, TABS_HEIGHT);

            Image tabsScrollBg = GetOrAddComponent<Image>(tabsScrollArea);
            tabsScrollBg.color = PANEL_BG;

            ScrollRect tabsScroll = GetOrAddComponent<ScrollRect>(tabsScrollArea);
            tabsScroll.horizontal = true;
            tabsScroll.vertical = false;
            tabsScroll.movementType = ScrollRect.MovementType.Elastic;

            // Viewport
            GameObject tabsViewport = FindOrCreateChild(tabsScrollArea, "Viewport");
            SetRectTransformStretch(tabsViewport);
            GetOrAddComponent<RectMask2D>(tabsViewport);
            tabsScroll.viewport = tabsViewport.GetComponent<RectTransform>();

            // Content
            GameObject tabsContent = FindOrCreateChild(tabsViewport, "Content");
            RectTransform tabsContentRT = GetOrAddComponent<RectTransform>(tabsContent);
            tabsContentRT.anchorMin = new Vector2(0, 0);
            tabsContentRT.anchorMax = new Vector2(0, 1);
            tabsContentRT.pivot = new Vector2(0, 0.5f);
            tabsContentRT.sizeDelta = new Vector2(0, 0);
            tabsScroll.content = tabsContentRT;

            ContentSizeFitter tabsCSF = GetOrAddComponent<ContentSizeFitter>(tabsContent);
            tabsCSF.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(tabsContent);
            hlg.spacing = 6f;
            hlg.padding = new RectOffset(10, 10, 8, 8);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            // Create all 11 category tabs
            CreateCategoryTab(tabsContent, "AllTab", "TODOS", true, CYAN_NEON, 70);
            CreateCategoryTab(tabsContent, "BeginnerTab", "INICIO", false, CAT_BEGINNER, 70);
            CreateCategoryTab(tabsContent, "MasteryTab", "MAESTRÍA", false, CAT_GAMES, 85);
            CreateCategoryTab(tabsContent, "VictoriesTab", "VICTORIAS", false, new Color(0.2f, 0.8f, 0.4f, 1f), 90);
            CreateCategoryTab(tabsContent, "StreaksTab", "RACHAS", false, new Color(1f, 0.5f, 0.2f, 1f), 75);
            CreateCategoryTab(tabsContent, "CashBattleTab", "CASH", false, new Color(0.4f, 0.8f, 0.2f, 1f), 60);
            CreateCategoryTab(tabsContent, "TournamentsTab", "TORNEOS", false, CAT_COMPETITION, 80);
            CreateCategoryTab(tabsContent, "SocialTab", "SOCIAL", false, new Color(0.4f, 0.6f, 1f, 1f), 70);
            CreateCategoryTab(tabsContent, "ProgressionTab", "PROGRESO", false, new Color(0.8f, 0.6f, 1f, 1f), 85);
            CreateCategoryTab(tabsContent, "CollectorTab", "COLECCION", false, new Color(0.9f, 0.7f, 0.3f, 1f), 95);
            CreateCategoryTab(tabsContent, "TimeTab", "TIEMPO", false, new Color(0.5f, 0.8f, 0.9f, 1f), 75);
            CreateCategoryTab(tabsContent, "SecretTab", "???", false, CAT_SECRET, 50);

            Debug.Log("[TrophyShowcase] CategoryTabs creado con 10 categorías (V1)");
        }

        private static void CreateCategoryTab(GameObject parent, string name, string label, bool isActive, Color color, float width = 0)
        {
            GameObject tab = FindOrCreateChild(parent, name);

            Image tabBg = GetOrAddComponent<Image>(tab);
            tabBg.color = isActive ? color : BUTTON_SECONDARY;

            Button tabButton = GetOrAddComponent<Button>(tab);
            SetupButtonColors(tabButton, isActive ? color : BUTTON_SECONDARY);
            AddOutline(tab, isActive ? color : CYAN_DARK * 0.4f, isActive ? 2 : 1);

            GameObject textObj = FindOrCreateChild(tab, "Text");
            TextMeshProUGUI tabText = GetOrAddComponent<TextMeshProUGUI>(textObj);
            tabText.text = label;
            tabText.fontSize = 20;
            tabText.fontStyle = FontStyles.Bold;
            tabText.color = isActive ? TEXT_DARK : TEXT_PRIMARY;
            tabText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(textObj);

            LayoutElement le = GetOrAddComponent<LayoutElement>(tab);
            le.minHeight = 44;
            if (width > 0)
            {
                le.minWidth = width;
                le.preferredWidth = width;
            }
            else
            {
                le.flexibleWidth = 1;
            }
        }

        // ==================== TROPHY SHOWCASE GRID ====================

        private static void CreateTrophyShowcaseGrid(GameObject parent)
        {
            float topOffset = HEADER_HEIGHT + TABS_HEIGHT + 30;

            GameObject scrollView = FindOrCreateChild(parent, "TrophyShowcaseScrollView");

            RectTransform scrollRT = GetOrAddComponent<RectTransform>(scrollView);
            scrollRT.anchorMin = Vector2.zero;
            scrollRT.anchorMax = Vector2.one;
            scrollRT.offsetMin = new Vector2(CONTENT_PADDING, CONTENT_PADDING);
            scrollRT.offsetMax = new Vector2(-CONTENT_PADDING, -topOffset);

            ScrollRect scrollRect = GetOrAddComponent<ScrollRect>(scrollView);
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.scrollSensitivity = 30f;

            Image scrollBg = GetOrAddComponent<Image>(scrollView);
            scrollBg.color = Color.clear;

            // Viewport
            GameObject viewport = FindOrCreateChild(scrollView, "Viewport");
            SetRectTransformStretch(viewport);
            RectTransform viewportRT = viewport.GetComponent<RectTransform>();
            GetOrAddComponent<RectMask2D>(viewport);
            scrollRect.viewport = viewportRT;

            // Content
            GameObject content = FindOrCreateChild(viewport, "Content");
            RectTransform contentRT = GetOrAddComponent<RectTransform>(content);
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.sizeDelta = new Vector2(0, 0);
            scrollRect.content = contentRT;

            ContentSizeFitter csf = GetOrAddComponent<ContentSizeFitter>(content);
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Grid Layout - 3 columns
            GridLayoutGroup grid = GetOrAddComponent<GridLayoutGroup>(content);
            grid.cellSize = new Vector2(TROPHY_CARD_WIDTH, TROPHY_CARD_HEIGHT);
            grid.spacing = new Vector2(GRID_SPACING, GRID_SPACING);
            grid.padding = new RectOffset(10, 10, 15, 30);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;

            // ==================== CREATE ALL 53 ACHIEVEMENT CARDS (V1) ====================

            // --- BEGINNER (4) ---
            CreateTrophyCard(content, "Trophy_first_game", "Primer Paso", 0, false, false, CAT_BEGINNER, false, 10);
            CreateTrophyCard(content, "Trophy_tutorial_complete", "Aprendiz", 0, false, false, CAT_BEGINNER, false, 10);
            CreateTrophyCard(content, "Trophy_first_win", "Primera Victoria", 100, true, false, GOLD, false, 15);
            CreateTrophyCard(content, "Trophy_profile_complete", "Identidad", 0, false, false, CAT_BEGINNER, false, 10);

            // --- MASTERY (5) ---
            CreateTrophyCard(content, "Trophy_digitrush_master", "Maestro de Dígitos", 45, false, true, CAT_GAMES, false, 50);
            CreateTrophyCard(content, "Trophy_flashtap_master", "Reflejos de Luz", 30, false, true, CAT_GAMES, false, 50);
            CreateTrophyCard(content, "Trophy_memorypairs_master", "Memoria Fotográfica", 0, false, false, CAT_GAMES, false, 50);
            CreateTrophyCard(content, "Trophy_quickmath_master", "Calculadora Humana", 20, false, true, CAT_GAMES, false, 50);
            CreateTrophyCard(content, "Trophy_oddoneout_master", "Ojo de Águila", 65, false, true, CAT_GAMES, false, 50);

            // --- VICTORIES (5) ---
            CreateTrophyCard(content, "Trophy_wins_10", "Competidor", 80, false, true, new Color(0.2f, 0.8f, 0.4f, 1f), false, 20);
            CreateTrophyCard(content, "Trophy_wins_50", "Veterano", 40, false, true, new Color(0.2f, 0.8f, 0.4f, 1f), false, 40);
            CreateTrophyCard(content, "Trophy_wins_100", "Centurión", 15, false, true, new Color(0.2f, 0.8f, 0.4f, 1f), false, 60);
            CreateTrophyCard(content, "Trophy_wins_500", "Leyenda", 5, false, true, new Color(0.2f, 0.8f, 0.4f, 1f), false, 100);
            CreateTrophyCard(content, "Trophy_wins_1000", "Inmortal", 0, false, false, new Color(0.2f, 0.8f, 0.4f, 1f), false, 200);

            // --- STREAKS (4) ---
            CreateTrophyCard(content, "Trophy_streak_3", "En Racha", 100, true, false, new Color(1f, 0.5f, 0.2f, 1f), false, 25);
            CreateTrophyCard(content, "Trophy_streak_5", "Imparable", 60, false, true, new Color(1f, 0.5f, 0.2f, 1f), false, 40);
            CreateTrophyCard(content, "Trophy_streak_10", "Dominación", 0, false, false, new Color(1f, 0.5f, 0.2f, 1f), false, 75);
            CreateTrophyCard(content, "Trophy_streak_20", "Invencible", 0, false, false, CAT_SECRET, true, 150); // SECRET

            // --- CASH BATTLE (7) ---
            CreateTrophyCard(content, "Trophy_cash_first", "Apostador", 100, true, false, new Color(0.4f, 0.8f, 0.2f, 1f), false, 25);
            CreateTrophyCard(content, "Trophy_cash_first_win", "Ganador Real", 100, true, false, GOLD, false, 35);
            CreateTrophyCard(content, "Trophy_cash_10_wins", "Jugador Serio", 50, false, true, new Color(0.4f, 0.8f, 0.2f, 1f), false, 50);
            CreateTrophyCard(content, "Trophy_cash_50_wins", "High Roller", 20, false, true, new Color(0.4f, 0.8f, 0.2f, 1f), false, 100);
            CreateTrophyCard(content, "Trophy_cash_100_wins", "Tiburón", 5, false, true, new Color(0.4f, 0.8f, 0.2f, 1f), false, 200);
            CreateTrophyCard(content, "Trophy_cash_earnings_100", "Primeros $100", 35, false, true, GOLD, false, 75);
            CreateTrophyCard(content, "Trophy_cash_earnings_1000", "Club de los Mil", 0, false, false, CAT_SECRET, true, 250); // SECRET

            // --- TOURNAMENTS (5) ---
            CreateTrophyCard(content, "Trophy_tournament_first", "Participante", 100, true, false, CAT_COMPETITION, false, 20);
            CreateTrophyCard(content, "Trophy_tournament_top3", "Podio", 0, false, false, CAT_COMPETITION, false, 50);
            CreateTrophyCard(content, "Trophy_tournament_win", "Campeón", 0, false, false, GOLD, false, 100);
            CreateTrophyCard(content, "Trophy_tournament_5_wins", "Multicampeón", 0, false, false, CAT_COMPETITION, false, 200);
            CreateTrophyCard(content, "Trophy_tournament_create", "Organizador", 0, false, false, CAT_COMPETITION, false, 30);

            // --- SOCIAL (5) ---
            CreateTrophyCard(content, "Trophy_friend_first", "Primer Amigo", 100, true, false, new Color(0.4f, 0.6f, 1f, 1f), false, 15);
            CreateTrophyCard(content, "Trophy_friends_10", "Popular", 70, false, true, new Color(0.4f, 0.6f, 1f, 1f), false, 30);
            CreateTrophyCard(content, "Trophy_friends_50", "Influencer", 10, false, true, new Color(0.4f, 0.6f, 1f, 1f), false, 75);
            CreateTrophyCard(content, "Trophy_challenge_friend", "Retador", 0, false, false, new Color(0.4f, 0.6f, 1f, 1f), false, 20);
            CreateTrophyCard(content, "Trophy_beat_friend", "Rival", 0, false, false, new Color(0.4f, 0.6f, 1f, 1f), false, 25);

            // --- PROGRESSION (4) ---
            CreateTrophyCard(content, "Trophy_level_10", "Nivel 10", 100, true, false, new Color(0.8f, 0.6f, 1f, 1f), false, 25);
            CreateTrophyCard(content, "Trophy_level_25", "Nivel 25", 60, false, true, new Color(0.8f, 0.6f, 1f, 1f), false, 50);
            CreateTrophyCard(content, "Trophy_level_50", "Nivel 50", 30, false, true, new Color(0.8f, 0.6f, 1f, 1f), false, 75);
            CreateTrophyCard(content, "Trophy_level_100", "Nivel 100", 0, false, false, new Color(0.8f, 0.6f, 1f, 1f), false, 150);

            // --- COLLECTOR --- Reservado para V2

            // --- TIME (6) ---
            CreateTrophyCard(content, "Trophy_days_7", "Una Semana", 100, true, false, new Color(0.5f, 0.8f, 0.9f, 1f), false, 25);
            CreateTrophyCard(content, "Trophy_days_30", "Un Mes", 50, false, true, new Color(0.5f, 0.8f, 0.9f, 1f), false, 50);
            CreateTrophyCard(content, "Trophy_days_100", "100 Días", 15, false, true, new Color(0.5f, 0.8f, 0.9f, 1f), false, 100);
            CreateTrophyCard(content, "Trophy_days_365", "Un Año", 0, false, false, CAT_SECRET, true, 300); // SECRET
            CreateTrophyCard(content, "Trophy_daily_streak_7", "Racha Semanal", 100, true, false, new Color(0.5f, 0.8f, 0.9f, 1f), false, 30);
            CreateTrophyCard(content, "Trophy_daily_streak_30", "Racha Mensual", 25, false, true, new Color(0.5f, 0.8f, 0.9f, 1f), false, 75);

            // --- SECRET (4) ---
            CreateTrophyCard(content, "Trophy_night_owl", "Búho Nocturno", 0, false, false, CAT_SECRET, true, 50);
            CreateTrophyCard(content, "Trophy_perfect_game", "Perfección", 0, false, false, CAT_SECRET, true, 100);
            CreateTrophyCard(content, "Trophy_comeback_king", "Rey del Comeback", 0, false, false, CAT_SECRET, true, 75);
            CreateTrophyCard(content, "Trophy_speed_demon", "Demonio de Velocidad", 0, false, false, CAT_SECRET, true, 100);

            Debug.Log("[TrophyShowcase] TrophyShowcaseGrid creado con 53 logros (V1)");
        }

        // ==================== EMPTY STATE ====================

        private static void CreateEmptyState(GameObject parent)
        {
            float topOffset = HEADER_HEIGHT + TABS_HEIGHT + 30;

            GameObject emptyState = FindOrCreateChild(parent, "EmptyStateContainer");
            emptyState.SetActive(false); // Hidden by default, shown when no results

            RectTransform emptyRT = GetOrAddComponent<RectTransform>(emptyState);
            emptyRT.anchorMin = Vector2.zero;
            emptyRT.anchorMax = Vector2.one;
            emptyRT.offsetMin = new Vector2(CONTENT_PADDING, CONTENT_PADDING);
            emptyRT.offsetMax = new Vector2(-CONTENT_PADDING, -topOffset);

            // Center content
            GameObject centerContent = FindOrCreateChild(emptyState, "CenterContent");
            RectTransform centerRT = GetOrAddComponent<RectTransform>(centerContent);
            centerRT.anchorMin = new Vector2(0.5f, 0.5f);
            centerRT.anchorMax = new Vector2(0.5f, 0.5f);
            centerRT.sizeDelta = new Vector2(300, 250);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(centerContent);
            vlg.spacing = 20;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // Empty Icon
            GameObject iconObj = FindOrCreateChild(centerContent, "EmptyStateIcon");
            Image iconImage = GetOrAddComponent<Image>(iconObj);
            iconImage.color = TEXT_SECONDARY;
            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(iconObj);
            iconLE.minWidth = 80;
            iconLE.minHeight = 80;
            iconLE.preferredWidth = 80;
            iconLE.preferredHeight = 80;

            // Empty Text
            GameObject textObj = FindOrCreateChild(centerContent, "EmptyStateText");
            TextMeshProUGUI emptyText = GetOrAddComponent<TextMeshProUGUI>(textObj);
            emptyText.text = "No hay logros en esta categoría";
            emptyText.fontSize = 28;
            emptyText.fontStyle = FontStyles.Bold;
            emptyText.color = TEXT_SECONDARY;
            emptyText.alignment = TextAlignmentOptions.Center;
            LayoutElement textLE = GetOrAddComponent<LayoutElement>(textObj);
            textLE.minHeight = 50;

            // Subtitle
            GameObject subtitleObj = FindOrCreateChild(centerContent, "Subtitle");
            TextMeshProUGUI subtitleText = GetOrAddComponent<TextMeshProUGUI>(subtitleObj);
            subtitleText.text = "Sigue jugando para desbloquear más logros";
            subtitleText.fontSize = 22;
            subtitleText.fontStyle = FontStyles.Bold;
            subtitleText.color = new Color(TEXT_SECONDARY.r, TEXT_SECONDARY.g, TEXT_SECONDARY.b, 0.7f);
            subtitleText.alignment = TextAlignmentOptions.Center;
            LayoutElement subtitleLE = GetOrAddComponent<LayoutElement>(subtitleObj);
            subtitleLE.minHeight = 30;

            Debug.Log("[TrophyShowcase] EmptyState creado");
        }

        private static void CreateTrophyCard(GameObject parent, string name, string title, int progressPercent,
            bool isUnlocked, bool hasProgress, Color accentColor, bool isSecret = false, int points = 50)
        {
            GameObject card = FindOrCreateChild(parent, name);

            // Card Container for animations
            GameObject cardContainer = FindOrCreateChild(card, "CardContainer");
            SetRectTransformStretch(cardContainer);

            // Card Background
            Image cardBg = GetOrAddComponent<Image>(cardContainer);
            cardBg.color = isUnlocked ? new Color(0.06f, 0.12f, 0.1f, 1f) : CARD_BG;

            // Border Glow
            Color glowColor = isSecret ? TROPHY_SECRET_GLOW :
                              isUnlocked ? TROPHY_UNLOCKED_GLOW :
                              hasProgress ? TROPHY_PROGRESS_GLOW :
                              new Color(0.2f, 0.25f, 0.3f, 0.3f);
            AddOutline(cardContainer, glowColor, isUnlocked ? 2 : 1);

            // Glass Overlay
            GameObject glassOverlay = FindOrCreateChild(cardContainer, "GlassOverlay");
            SetRectTransformStretch(glassOverlay);
            RectTransform glassRT = glassOverlay.GetComponent<RectTransform>();
            glassRT.offsetMax = new Vector2(0, -TROPHY_CARD_HEIGHT * 0.4f);

            Image glassImage = GetOrAddComponent<Image>(glassOverlay);
            glassImage.color = isUnlocked ? new Color(1f, 1f, 1f, 0.1f) : GLASS_OVERLAY;

            // Trophy Display Area
            GameObject trophyArea = FindOrCreateChild(cardContainer, "TrophyArea");
            RectTransform trophyAreaRT = GetOrAddComponent<RectTransform>(trophyArea);
            trophyAreaRT.anchorMin = new Vector2(0.5f, 1);
            trophyAreaRT.anchorMax = new Vector2(0.5f, 1);
            trophyAreaRT.pivot = new Vector2(0.5f, 1);
            trophyAreaRT.anchoredPosition = new Vector2(0, -20);
            trophyAreaRT.sizeDelta = new Vector2(100, 100);

            // Trophy Icon
            GameObject trophyIcon = FindOrCreateChild(trophyArea, "TrophyIcon");
            SetRectTransformStretch(trophyIcon);
            RectTransform trophyIconRT = trophyIcon.GetComponent<RectTransform>();
            trophyIconRT.offsetMin = new Vector2(15, 15);
            trophyIconRT.offsetMax = new Vector2(-15, -15);

            Image trophyImage = GetOrAddComponent<Image>(trophyIcon);
            trophyImage.color = isSecret ? new Color(0.4f, 0.4f, 0.5f, 0.5f) :
                                isUnlocked ? accentColor : new Color(0.3f, 0.3f, 0.35f, 1f);

            // Question Mark for secret
            if (isSecret)
            {
                GameObject questionMark = FindOrCreateChild(trophyArea, "QuestionMark");
                SetRectTransformStretch(questionMark);
                TextMeshProUGUI questionText = GetOrAddComponent<TextMeshProUGUI>(questionMark);
                questionText.text = "?";
                questionText.fontSize = 48;
                questionText.fontStyle = FontStyles.Bold;
                questionText.color = CAT_SECRET;
                questionText.alignment = TextAlignmentOptions.Center;
            }

            // Locked Overlay
            if (!isUnlocked && !isSecret)
            {
                GameObject lockedOverlay = FindOrCreateChild(trophyArea, "LockedOverlay");
                SetRectTransformStretch(lockedOverlay);

                Image lockedImage = GetOrAddComponent<Image>(lockedOverlay);
                lockedImage.color = new Color(0f, 0f, 0f, 0.5f);

                GameObject lockIcon = FindOrCreateChild(lockedOverlay, "LockIcon");
                RectTransform lockRT = GetOrAddComponent<RectTransform>(lockIcon);
                lockRT.anchorMin = new Vector2(0.5f, 0.5f);
                lockRT.anchorMax = new Vector2(0.5f, 0.5f);
                lockRT.sizeDelta = new Vector2(30, 30);

                Image lockImage = GetOrAddComponent<Image>(lockIcon);
                lockImage.color = TEXT_SECONDARY;
            }

            // Info Section
            GameObject infoSection = FindOrCreateChild(cardContainer, "InfoSection");
            RectTransform infoRT = GetOrAddComponent<RectTransform>(infoSection);
            infoRT.anchorMin = new Vector2(0, 0);
            infoRT.anchorMax = new Vector2(1, 0);
            infoRT.pivot = new Vector2(0.5f, 0);
            infoRT.anchoredPosition = new Vector2(0, 15);
            infoRT.sizeDelta = new Vector2(0, 85);

            VerticalLayoutGroup infoVlg = GetOrAddComponent<VerticalLayoutGroup>(infoSection);
            infoVlg.spacing = 6;
            infoVlg.padding = new RectOffset(12, 12, 8, 8);
            infoVlg.childAlignment = TextAnchor.LowerCenter;
            infoVlg.childControlWidth = true;
            infoVlg.childControlHeight = true;

            // Title
            GameObject titleObj = FindOrCreateChild(infoSection, "Title");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = title;
            titleText.fontSize = 22;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = isUnlocked ? TEXT_PRIMARY : (isSecret ? CAT_SECRET : TEXT_SECONDARY);
            titleText.alignment = TextAlignmentOptions.Center;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minHeight = 20;

            // Progress Bar (if in progress)
            if (hasProgress && !isUnlocked)
            {
                GameObject progressBar = FindOrCreateChild(infoSection, "ProgressBar");
                LayoutElement progressLE = GetOrAddComponent<LayoutElement>(progressBar);
                progressLE.minHeight = 12;

                Image progressBg = GetOrAddComponent<Image>(progressBar);
                progressBg.color = new Color(0.1f, 0.12f, 0.15f, 1f);

                GameObject progressFill = FindOrCreateChild(progressBar, "Fill");
                RectTransform fillRT = GetOrAddComponent<RectTransform>(progressFill);
                fillRT.anchorMin = Vector2.zero;
                fillRT.anchorMax = new Vector2(progressPercent / 100f, 1);
                fillRT.sizeDelta = Vector2.zero;

                Image fillImage = GetOrAddComponent<Image>(progressFill);
                fillImage.color = accentColor;

                // Progress Text
                GameObject progressText = FindOrCreateChild(infoSection, "ProgressText");
                TextMeshProUGUI progressTmp = GetOrAddComponent<TextMeshProUGUI>(progressText);
                progressTmp.text = $"{progressPercent}%";
                progressTmp.fontSize = 16;
                progressTmp.fontStyle = FontStyles.Bold;
                progressTmp.color = TEXT_SECONDARY;
                progressTmp.alignment = TextAlignmentOptions.Center;
                LayoutElement progressTextLE = GetOrAddComponent<LayoutElement>(progressText);
                progressTextLE.minHeight = 15;
            }

            // Completed Badge
            if (isUnlocked)
            {
                GameObject completedBadge = FindOrCreateChild(cardContainer, "CompletedBadge");
                RectTransform badgeRT = GetOrAddComponent<RectTransform>(completedBadge);
                badgeRT.anchorMin = new Vector2(1, 1);
                badgeRT.anchorMax = new Vector2(1, 1);
                badgeRT.pivot = new Vector2(1, 1);
                badgeRT.anchoredPosition = new Vector2(-8, -8);
                badgeRT.sizeDelta = new Vector2(28, 28);

                Image badgeImage = GetOrAddComponent<Image>(completedBadge);
                badgeImage.color = BUTTON_SUCCESS;

                GameObject checkmark = FindOrCreateChild(completedBadge, "Checkmark");
                SetRectTransformStretch(checkmark);
                TextMeshProUGUI checkText = GetOrAddComponent<TextMeshProUGUI>(checkmark);
                checkText.text = "V";
                checkText.fontSize = 22;
                checkText.fontStyle = FontStyles.Bold;
                checkText.color = TEXT_DARK;
                checkText.alignment = TextAlignmentOptions.Center;
            }

            // Points Badge
            if (!isSecret)
            {
                GameObject pointsBadge = FindOrCreateChild(cardContainer, "PointsBadge");
                RectTransform pointsBadgeRT = GetOrAddComponent<RectTransform>(pointsBadge);
                pointsBadgeRT.anchorMin = new Vector2(0, 1);
                pointsBadgeRT.anchorMax = new Vector2(0, 1);
                pointsBadgeRT.pivot = new Vector2(0, 1);
                pointsBadgeRT.anchoredPosition = new Vector2(8, -8);
                pointsBadgeRT.sizeDelta = new Vector2(50, 22);

                Image pointsBadgeBg = GetOrAddComponent<Image>(pointsBadge);
                pointsBadgeBg.color = new Color(0f, 0f, 0f, 0.6f);

                GameObject pointsText = FindOrCreateChild(pointsBadge, "Text");
                SetRectTransformStretch(pointsText);
                TextMeshProUGUI pointsTmp = GetOrAddComponent<TextMeshProUGUI>(pointsText);
                pointsTmp.text = $"{points} pts";
                pointsTmp.fontSize = 16;
                pointsTmp.fontStyle = FontStyles.Bold;
                pointsTmp.color = GOLD;
                pointsTmp.alignment = TextAlignmentOptions.Center;
            }

            // Make card clickable
            Button cardButton = GetOrAddComponent<Button>(card);
            SetupButtonColors(cardButton, cardBg.color);
            cardButton.transition = Selectable.Transition.None;
        }

        // ==================== DETAIL PANEL ====================

        private static void CreateDetailPanel(Canvas canvas)
        {
            GameObject blocker = FindOrCreateChild(canvas.gameObject, "DetailPanelBlocker");
            blocker.SetActive(false);

            SetRectTransformStretch(blocker);
            Image blockerBg = GetOrAddComponent<Image>(blocker);
            blockerBg.color = BLOCKER_BG;
            Button blockerBtn = GetOrAddComponent<Button>(blocker);
            blockerBtn.transition = Selectable.Transition.None;
            blocker.transform.SetAsLastSibling();

            GameObject panel = FindOrCreateChild(blocker, "DetailPanel");
            RectTransform panelRT = GetOrAddComponent<RectTransform>(panel);
            panelRT.anchorMin = new Vector2(0.5f, 0.5f);
            panelRT.anchorMax = new Vector2(0.5f, 0.5f);
            panelRT.sizeDelta = new Vector2(480, DETAIL_PANEL_HEIGHT);

            Image panelBg = GetOrAddComponent<Image>(panel);
            panelBg.color = POPUP_BG;
            AddOutline(panel, CYAN_NEON, 2);

            // Add CanvasGroup for fade animations
            CanvasGroup panelCanvasGroup = GetOrAddComponent<CanvasGroup>(panel);
            panelCanvasGroup.alpha = 1f;

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(panel);
            vlg.spacing = 18;
            vlg.padding = new RectOffset(30, 30, 30, 25);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Close Button
            GameObject closeBtn = FindOrCreateChild(panel, "CloseButton");
            RectTransform closeRT = GetOrAddComponent<RectTransform>(closeBtn);
            closeRT.anchorMin = new Vector2(1, 1);
            closeRT.anchorMax = new Vector2(1, 1);
            closeRT.pivot = new Vector2(1, 1);
            closeRT.anchoredPosition = new Vector2(-12, -12);
            closeRT.sizeDelta = new Vector2(40, 40);

            Image closeBg = GetOrAddComponent<Image>(closeBtn);
            closeBg.color = BUTTON_SECONDARY;
            Button closeButton = GetOrAddComponent<Button>(closeBtn);
            SetupButtonColors(closeButton, BUTTON_SECONDARY);

            GameObject closeText = FindOrCreateChild(closeBtn, "Text");
            TextMeshProUGUI closeTmp = GetOrAddComponent<TextMeshProUGUI>(closeText);
            closeTmp.text = "X";
            closeTmp.fontSize = 28;
            closeTmp.fontStyle = FontStyles.Bold;
            closeTmp.color = TEXT_PRIMARY;
            closeTmp.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(closeText);

            // Trophy Icon (large)
            GameObject trophyObj = FindOrCreateChild(panel, "DetailTrophyIcon");
            Image trophyBg = GetOrAddComponent<Image>(trophyObj);
            trophyBg.color = GOLD;
            LayoutElement trophyLE = GetOrAddComponent<LayoutElement>(trophyObj);
            trophyLE.minWidth = 90;
            trophyLE.minHeight = 90;
            trophyLE.preferredWidth = 90;
            trophyLE.preferredHeight = 90;

            // Title
            GameObject titleObj = FindOrCreateChild(panel, "DetailTitle");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "Primera Victoria";
            titleText.fontSize = 42;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = GOLD;
            titleText.alignment = TextAlignmentOptions.Center;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minHeight = 35;

            // Description
            GameObject descObj = FindOrCreateChild(panel, "DetailDescription");
            TextMeshProUGUI descText = GetOrAddComponent<TextMeshProUGUI>(descObj);
            descText.text = "Gana tu primera partida en cualquier modo de juego.";
            descText.fontSize = 24;
            descText.fontStyle = FontStyles.Bold;
            descText.color = TEXT_SECONDARY;
            descText.alignment = TextAlignmentOptions.Center;
            LayoutElement descLE = GetOrAddComponent<LayoutElement>(descObj);
            descLE.minHeight = 45;

            // Category Text
            GameObject categoryObj = FindOrCreateChild(panel, "DetailCategoryText");
            TextMeshProUGUI categoryText = GetOrAddComponent<TextMeshProUGUI>(categoryObj);
            categoryText.text = "Categoria: Principiante";
            categoryText.fontSize = 22;
            categoryText.fontStyle = FontStyles.Bold;
            categoryText.color = CYAN_NEON;
            categoryText.alignment = TextAlignmentOptions.Center;
            LayoutElement categoryLE = GetOrAddComponent<LayoutElement>(categoryObj);
            categoryLE.minHeight = 22;

            // Progress Section
            GameObject progressSection = FindOrCreateChild(panel, "DetailProgressSection");
            VerticalLayoutGroup progVlg = GetOrAddComponent<VerticalLayoutGroup>(progressSection);
            progVlg.spacing = 8;
            progVlg.childAlignment = TextAnchor.MiddleCenter;
            progVlg.childControlWidth = true;
            progVlg.childControlHeight = true;
            progVlg.childForceExpandWidth = true;
            LayoutElement progLE = GetOrAddComponent<LayoutElement>(progressSection);
            progLE.minHeight = 55;

            // Progress Bar (Slider)
            GameObject progressBar = FindOrCreateChild(progressSection, "DetailProgressBar");
            Image progressBarBg = GetOrAddComponent<Image>(progressBar);
            progressBarBg.color = new Color(0.1f, 0.12f, 0.15f, 1f);
            AddOutline(progressBar, CYAN_DARK * 0.5f);
            LayoutElement progressBarLE = GetOrAddComponent<LayoutElement>(progressBar);
            progressBarLE.minHeight = 22;

            // Add Slider component
            Slider detailSlider = GetOrAddComponent<Slider>(progressBar);
            detailSlider.minValue = 0;
            detailSlider.maxValue = 1;
            detailSlider.value = 1f;
            detailSlider.interactable = false;

            // Fill Area
            GameObject detailFillArea = FindOrCreateChild(progressBar, "Fill Area");
            SetRectTransformStretch(detailFillArea);

            GameObject progressFill = FindOrCreateChild(detailFillArea, "Fill");
            RectTransform fillRT = GetOrAddComponent<RectTransform>(progressFill);
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = new Vector2(1f, 1);
            fillRT.sizeDelta = Vector2.zero;

            Image fillImage = GetOrAddComponent<Image>(progressFill);
            fillImage.color = BUTTON_SUCCESS;

            // Configure slider
            detailSlider.fillRect = fillRT;
            detailSlider.targetGraphic = progressBarBg;

            // Progress Text
            GameObject progressTextObj = FindOrCreateChild(progressSection, "DetailProgressText");
            TextMeshProUGUI progressTmp = GetOrAddComponent<TextMeshProUGUI>(progressTextObj);
            progressTmp.text = "1/1 Completado";
            progressTmp.fontSize = 22;
            progressTmp.fontStyle = FontStyles.Bold;
            progressTmp.color = BUTTON_SUCCESS;
            progressTmp.alignment = TextAlignmentOptions.Center;
            LayoutElement progressTextLE = GetOrAddComponent<LayoutElement>(progressTextObj);
            progressTextLE.minHeight = 20;

            // Points Text
            GameObject pointsObj = FindOrCreateChild(panel, "DetailPointsText");
            TextMeshProUGUI pointsTmp2 = GetOrAddComponent<TextMeshProUGUI>(pointsObj);
            pointsTmp2.text = "+50 pts";
            pointsTmp2.fontSize = 24;
            pointsTmp2.fontStyle = FontStyles.Bold;
            pointsTmp2.color = GOLD;
            pointsTmp2.alignment = TextAlignmentOptions.Center;
            LayoutElement pointsLE2 = GetOrAddComponent<LayoutElement>(pointsObj);
            pointsLE2.minHeight = 22;

            // Reward Section
            GameObject rewardSection = FindOrCreateChild(panel, "DetailRewardSection");
            Image rewardBg = GetOrAddComponent<Image>(rewardSection);
            rewardBg.color = new Color(0.06f, 0.08f, 0.12f, 1f);
            AddOutline(rewardSection, GOLD * 0.5f);
            LayoutElement rewardSectionLE = GetOrAddComponent<LayoutElement>(rewardSection);
            rewardSectionLE.minHeight = 60;

            HorizontalLayoutGroup rewardHlg = GetOrAddComponent<HorizontalLayoutGroup>(rewardSection);
            rewardHlg.spacing = 12;
            rewardHlg.padding = new RectOffset(18, 18, 12, 12);
            rewardHlg.childAlignment = TextAnchor.MiddleCenter;
            rewardHlg.childControlWidth = false;
            rewardHlg.childControlHeight = true;

            GameObject rewardIcon = FindOrCreateChild(rewardSection, "RewardIcon");
            Image rewardIconImage = GetOrAddComponent<Image>(rewardIcon);
            rewardIconImage.color = new Color(0.4f, 0.8f, 1f, 1f); // Gem blue
            LayoutElement rewardIconLE = GetOrAddComponent<LayoutElement>(rewardIcon);
            rewardIconLE.minWidth = 35;
            rewardIconLE.minHeight = 35;

            GameObject rewardAmount = FindOrCreateChild(rewardSection, "RewardAmount");
            TextMeshProUGUI rewardTmp = GetOrAddComponent<TextMeshProUGUI>(rewardAmount);
            rewardTmp.text = "50 Gemas";
            rewardTmp.fontSize = 32;
            rewardTmp.fontStyle = FontStyles.Bold;
            rewardTmp.color = new Color(0.4f, 0.8f, 1f, 1f);
            rewardTmp.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement rewardAmountLE = GetOrAddComponent<LayoutElement>(rewardAmount);
            rewardAmountLE.flexibleWidth = 1;

            // Claim Button
            GameObject claimBtn = FindOrCreateChild(panel, "ClaimRewardButton");
            Image claimBg = GetOrAddComponent<Image>(claimBtn);
            claimBg.color = BUTTON_SUCCESS;
            Button claimButton = GetOrAddComponent<Button>(claimBtn);
            SetupButtonColors(claimButton, BUTTON_SUCCESS);
            LayoutElement claimLE = GetOrAddComponent<LayoutElement>(claimBtn);
            claimLE.minHeight = 55;

            GameObject claimText = FindOrCreateChild(claimBtn, "ClaimButtonText");
            TextMeshProUGUI claimTmp = GetOrAddComponent<TextMeshProUGUI>(claimText);
            claimTmp.text = "RECLAMAR RECOMPENSA";
            claimTmp.fontSize = 30;
            claimTmp.fontStyle = FontStyles.Bold;
            claimTmp.color = TEXT_DARK;
            claimTmp.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(claimText);

            Debug.Log("[TrophyShowcase] DetailPanel creado");
        }

        // ==================== REWARD CELEBRATION ====================

        private static void CreateRewardCelebration(Canvas canvas)
        {
            GameObject celebration = FindOrCreateChild(canvas.gameObject, "RewardCelebration");
            celebration.SetActive(false);

            SetRectTransformStretch(celebration);
            Image celebrationBg = GetOrAddComponent<Image>(celebration);
            celebrationBg.color = new Color(0f, 0f, 0f, 0.92f);
            celebration.transform.SetAsLastSibling();

            // Center Content
            GameObject centerContent = FindOrCreateChild(celebration, "CenterContent");
            RectTransform centerRT = GetOrAddComponent<RectTransform>(centerContent);
            centerRT.anchorMin = new Vector2(0.5f, 0.5f);
            centerRT.anchorMax = new Vector2(0.5f, 0.5f);
            centerRT.sizeDelta = new Vector2(400, 450);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(centerContent);
            vlg.spacing = 25;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // Trophy Icon (animated)
            GameObject trophyObj = FindOrCreateChild(centerContent, "CelebrationTrophyIcon");
            Image trophyImage = GetOrAddComponent<Image>(trophyObj);
            trophyImage.color = GOLD;
            LayoutElement trophyLE = GetOrAddComponent<LayoutElement>(trophyObj);
            trophyLE.minWidth = 120;
            trophyLE.minHeight = 120;
            trophyLE.preferredWidth = 120;
            trophyLE.preferredHeight = 120;

            // Glow behind trophy (separate child of celebration for manager reference)
            GameObject glowObj = FindOrCreateChild(celebration, "CelebrationGlow");
            RectTransform glowRT = GetOrAddComponent<RectTransform>(glowObj);
            glowRT.anchorMin = new Vector2(0.5f, 0.5f);
            glowRT.anchorMax = new Vector2(0.5f, 0.5f);
            glowRT.sizeDelta = new Vector2(200, 200);
            glowObj.transform.SetSiblingIndex(1); // Behind content

            Image glowImage = GetOrAddComponent<Image>(glowObj);
            glowImage.color = new Color(1f, 0.9f, 0.5f, 0.4f);

            // Title
            GameObject titleObj = FindOrCreateChild(centerContent, "CelebrationTitle");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "LOGRO DESBLOQUEADO!";
            titleText.fontSize = 48;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = GOLD;
            titleText.alignment = TextAlignmentOptions.Center;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minHeight = 40;

            // Achievement Name
            GameObject nameObj = FindOrCreateChild(centerContent, "CelebrationAchievementName");
            TextMeshProUGUI nameText = GetOrAddComponent<TextMeshProUGUI>(nameObj);
            nameText.text = "Primera Victoria";
            nameText.fontSize = 36;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = TEXT_PRIMARY;
            nameText.alignment = TextAlignmentOptions.Center;
            LayoutElement nameLE = GetOrAddComponent<LayoutElement>(nameObj);
            nameLE.minHeight = 30;

            // Reward Display
            GameObject rewardDisplay = FindOrCreateChild(centerContent, "CelebrationRewardDisplay");
            HorizontalLayoutGroup rewardHlg = GetOrAddComponent<HorizontalLayoutGroup>(rewardDisplay);
            rewardHlg.spacing = 12;
            rewardHlg.childAlignment = TextAnchor.MiddleCenter;
            rewardHlg.childControlWidth = false;
            rewardHlg.childControlHeight = true;
            LayoutElement rewardDisplayLE = GetOrAddComponent<LayoutElement>(rewardDisplay);
            rewardDisplayLE.minHeight = 55;

            GameObject rewardIcon = FindOrCreateChild(rewardDisplay, "Icon");
            Image rewardIconImage = GetOrAddComponent<Image>(rewardIcon);
            rewardIconImage.color = new Color(0.4f, 0.8f, 1f, 1f);
            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(rewardIcon);
            iconLE.minWidth = 45;
            iconLE.minHeight = 45;

            GameObject rewardAmount = FindOrCreateChild(rewardDisplay, "Amount");
            TextMeshProUGUI rewardTmp = GetOrAddComponent<TextMeshProUGUI>(rewardAmount);
            rewardTmp.text = "+50";
            rewardTmp.fontSize = 52;
            rewardTmp.fontStyle = FontStyles.Bold;
            rewardTmp.color = new Color(0.4f, 0.8f, 1f, 1f);
            rewardTmp.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement amountLE = GetOrAddComponent<LayoutElement>(rewardAmount);
            amountLE.minWidth = 100;

            // Points Earned
            GameObject pointsEarned = FindOrCreateChild(centerContent, "PointsEarned");
            TextMeshProUGUI pointsTmp = GetOrAddComponent<TextMeshProUGUI>(pointsEarned);
            pointsTmp.text = "+50 Puntos";
            pointsTmp.fontSize = 28;
            pointsTmp.fontStyle = FontStyles.Bold;
            pointsTmp.color = GOLD;
            pointsTmp.alignment = TextAlignmentOptions.Center;
            LayoutElement pointsLE = GetOrAddComponent<LayoutElement>(pointsEarned);
            pointsLE.minHeight = 25;

            // Continue Button
            GameObject continueBtn = FindOrCreateChild(centerContent, "ContinueButton");
            Image continueBg = GetOrAddComponent<Image>(continueBtn);
            continueBg.color = CYAN_NEON;
            Button continueButton = GetOrAddComponent<Button>(continueBtn);
            SetupButtonColors(continueButton, CYAN_NEON);
            LayoutElement continueLE = GetOrAddComponent<LayoutElement>(continueBtn);
            continueLE.minHeight = 55;
            continueLE.minWidth = 250;

            GameObject continueText = FindOrCreateChild(continueBtn, "Text");
            TextMeshProUGUI continueTmp = GetOrAddComponent<TextMeshProUGUI>(continueText);
            continueTmp.text = "CONTINUAR";
            continueTmp.fontSize = 34;
            continueTmp.fontStyle = FontStyles.Bold;
            continueTmp.color = TEXT_DARK;
            continueTmp.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(continueText);

            Debug.Log("[TrophyShowcase] RewardCelebration creado");
        }

        // ==================== PREFAB CREATION ====================

        private static void CreateTrophyCardPrefab()
        {
            string prefabPath = "Assets/_Project/Prefabs/Monetization/TrophyCard.prefab";

            // Ensure directory exists
            string directory = System.IO.Path.GetDirectoryName(prefabPath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }

            // Create prefab object
            GameObject prefabRoot = new GameObject("TrophyCard");
            RectTransform rootRT = prefabRoot.AddComponent<RectTransform>();
            rootRT.sizeDelta = new Vector2(TROPHY_CARD_WIDTH, TROPHY_CARD_HEIGHT);

            // Add TrophyCardUI component
            // Note: This will be added when the script compiles
            // prefabRoot.AddComponent<DigitPark.UI.Items.TrophyCardUI>();

            // Card Container
            GameObject cardContainer = new GameObject("CardContainer");
            cardContainer.transform.SetParent(prefabRoot.transform, false);
            RectTransform containerRT = cardContainer.AddComponent<RectTransform>();
            containerRT.anchorMin = Vector2.zero;
            containerRT.anchorMax = Vector2.one;
            containerRT.sizeDelta = Vector2.zero;

            Image cardBg = cardContainer.AddComponent<Image>();
            cardBg.color = CARD_BG;

            Outline outline = cardContainer.AddComponent<Outline>();
            outline.effectColor = new Color(0.3f, 0.3f, 0.4f, 0.3f);
            outline.effectDistance = new Vector2(1, 1);

            // Glass Overlay
            GameObject glassOverlay = new GameObject("GlassOverlay");
            glassOverlay.transform.SetParent(cardContainer.transform, false);
            RectTransform glassRT = glassOverlay.AddComponent<RectTransform>();
            glassRT.anchorMin = Vector2.zero;
            glassRT.anchorMax = Vector2.one;
            glassRT.offsetMax = new Vector2(0, -TROPHY_CARD_HEIGHT * 0.4f);

            Image glassImage = glassOverlay.AddComponent<Image>();
            glassImage.color = GLASS_OVERLAY;

            // Border Glow
            GameObject borderGlow = new GameObject("BorderGlow");
            borderGlow.transform.SetParent(cardContainer.transform, false);
            RectTransform borderRT = borderGlow.AddComponent<RectTransform>();
            borderRT.anchorMin = Vector2.zero;
            borderRT.anchorMax = Vector2.one;
            borderRT.sizeDelta = new Vector2(4, 4);
            borderRT.anchoredPosition = Vector2.zero;

            Image borderImage = borderGlow.AddComponent<Image>();
            borderImage.color = new Color(0.3f, 0.3f, 0.4f, 0.3f);
            borderGlow.transform.SetAsFirstSibling();

            // Trophy Icon
            GameObject trophyIcon = new GameObject("TrophyIcon");
            trophyIcon.transform.SetParent(cardContainer.transform, false);
            RectTransform trophyRT = trophyIcon.AddComponent<RectTransform>();
            trophyRT.anchorMin = new Vector2(0.5f, 1);
            trophyRT.anchorMax = new Vector2(0.5f, 1);
            trophyRT.pivot = new Vector2(0.5f, 1);
            trophyRT.anchoredPosition = new Vector2(0, -30);
            trophyRT.sizeDelta = new Vector2(70, 70);

            Image trophyImage = trophyIcon.AddComponent<Image>();
            trophyImage.color = new Color(0.3f, 0.3f, 0.35f, 1f);

            // Trophy Shadow
            GameObject trophyShadow = new GameObject("TrophyShadow");
            trophyShadow.transform.SetParent(trophyIcon.transform, false);
            RectTransform shadowRT = trophyShadow.AddComponent<RectTransform>();
            shadowRT.anchorMin = new Vector2(0.5f, 0);
            shadowRT.anchorMax = new Vector2(0.5f, 0);
            shadowRT.pivot = new Vector2(0.5f, 1);
            shadowRT.anchoredPosition = new Vector2(0, -5);
            shadowRT.sizeDelta = new Vector2(60, 15);

            Image shadowImage = trophyShadow.AddComponent<Image>();
            shadowImage.color = new Color(0f, 0f, 0f, 0.3f);

            // Locked Overlay
            GameObject lockedOverlay = new GameObject("LockedOverlay");
            lockedOverlay.transform.SetParent(trophyIcon.transform, false);
            RectTransform lockedRT = lockedOverlay.AddComponent<RectTransform>();
            lockedRT.anchorMin = Vector2.zero;
            lockedRT.anchorMax = Vector2.one;
            lockedRT.sizeDelta = Vector2.zero;

            Image lockedImage = lockedOverlay.AddComponent<Image>();
            lockedImage.color = new Color(0f, 0f, 0f, 0.5f);
            lockedOverlay.SetActive(false);

            // Question Mark (for secret)
            GameObject questionMark = new GameObject("QuestionMark");
            questionMark.transform.SetParent(trophyIcon.transform, false);
            RectTransform questionRT = questionMark.AddComponent<RectTransform>();
            questionRT.anchorMin = Vector2.zero;
            questionRT.anchorMax = Vector2.one;
            questionRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI questionText = questionMark.AddComponent<TextMeshProUGUI>();
            questionText.text = "?";
            questionText.fontSize = 42;
            questionText.fontStyle = FontStyles.Bold;
            questionText.color = CAT_SECRET;
            questionText.alignment = TextAlignmentOptions.Center;
            questionMark.SetActive(false);

            // Progress Container
            GameObject progressContainer = new GameObject("ProgressContainer");
            progressContainer.transform.SetParent(cardContainer.transform, false);
            RectTransform progressContainerRT = progressContainer.AddComponent<RectTransform>();
            progressContainerRT.anchorMin = new Vector2(0, 0);
            progressContainerRT.anchorMax = new Vector2(1, 0);
            progressContainerRT.pivot = new Vector2(0.5f, 0);
            progressContainerRT.anchoredPosition = new Vector2(0, 60);
            progressContainerRT.sizeDelta = new Vector2(-24, 30);

            VerticalLayoutGroup progressVlg = progressContainer.AddComponent<VerticalLayoutGroup>();
            progressVlg.spacing = 4;
            progressVlg.childAlignment = TextAnchor.MiddleCenter;
            progressVlg.childControlWidth = true;
            progressVlg.childControlHeight = true;

            // Progress Background
            GameObject progressBg = new GameObject("ProgressBackground");
            progressBg.transform.SetParent(progressContainer.transform, false);
            RectTransform progressBgRT = progressBg.AddComponent<RectTransform>();

            Image progressBgImage = progressBg.AddComponent<Image>();
            progressBgImage.color = new Color(0.1f, 0.12f, 0.15f, 1f);

            LayoutElement progressBgLE = progressBg.AddComponent<LayoutElement>();
            progressBgLE.minHeight = 10;
            progressBgLE.preferredHeight = 10;

            // Progress Fill
            GameObject progressFill = new GameObject("ProgressFill");
            progressFill.transform.SetParent(progressBg.transform, false);
            RectTransform fillRT = progressFill.AddComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = new Vector2(0.5f, 1);
            fillRT.sizeDelta = Vector2.zero;

            Image fillImage = progressFill.AddComponent<Image>();
            fillImage.color = GOLD;

            // Progress Text
            GameObject progressText = new GameObject("ProgressText");
            progressText.transform.SetParent(progressContainer.transform, false);

            TextMeshProUGUI progressTmp = progressText.AddComponent<TextMeshProUGUI>();
            progressTmp.text = "50%";
            progressTmp.fontSize = 16;
            progressTmp.fontStyle = FontStyles.Bold;
            progressTmp.color = TEXT_SECONDARY;
            progressTmp.alignment = TextAlignmentOptions.Center;

            LayoutElement progressTextLE = progressText.AddComponent<LayoutElement>();
            progressTextLE.minHeight = 14;

            progressContainer.SetActive(false);

            // Title Text
            GameObject titleText = new GameObject("TitleText");
            titleText.transform.SetParent(cardContainer.transform, false);
            RectTransform titleRT = titleText.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0);
            titleRT.anchorMax = new Vector2(1, 0);
            titleRT.pivot = new Vector2(0.5f, 0);
            titleRT.anchoredPosition = new Vector2(0, 32);
            titleRT.sizeDelta = new Vector2(-20, 25);

            TextMeshProUGUI titleTmp = titleText.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "Logro";
            titleTmp.fontSize = 22;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = TEXT_SECONDARY;
            titleTmp.alignment = TextAlignmentOptions.Center;

            // Points Text
            GameObject pointsText = new GameObject("PointsText");
            pointsText.transform.SetParent(cardContainer.transform, false);
            RectTransform pointsRT = pointsText.AddComponent<RectTransform>();
            pointsRT.anchorMin = new Vector2(0, 0);
            pointsRT.anchorMax = new Vector2(1, 0);
            pointsRT.pivot = new Vector2(0.5f, 0);
            pointsRT.anchoredPosition = new Vector2(0, 12);
            pointsRT.sizeDelta = new Vector2(-20, 18);

            TextMeshProUGUI pointsTmp = pointsText.AddComponent<TextMeshProUGUI>();
            pointsTmp.text = "50 pts";
            pointsTmp.fontSize = 16;
            pointsTmp.fontStyle = FontStyles.Bold;
            pointsTmp.color = GOLD;
            pointsTmp.alignment = TextAlignmentOptions.Center;

            // Completed Badge
            GameObject completedBadge = new GameObject("CompletedBadge");
            completedBadge.transform.SetParent(cardContainer.transform, false);
            RectTransform badgeRT = completedBadge.AddComponent<RectTransform>();
            badgeRT.anchorMin = new Vector2(1, 1);
            badgeRT.anchorMax = new Vector2(1, 1);
            badgeRT.pivot = new Vector2(1, 1);
            badgeRT.anchoredPosition = new Vector2(-8, -8);
            badgeRT.sizeDelta = new Vector2(26, 26);

            Image badgeImage = completedBadge.AddComponent<Image>();
            badgeImage.color = BUTTON_SUCCESS;

            GameObject checkmark = new GameObject("Checkmark");
            checkmark.transform.SetParent(completedBadge.transform, false);
            RectTransform checkRT = checkmark.AddComponent<RectTransform>();
            checkRT.anchorMin = Vector2.zero;
            checkRT.anchorMax = Vector2.one;
            checkRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI checkTmp = checkmark.AddComponent<TextMeshProUGUI>();
            checkTmp.text = "V";
            checkTmp.fontSize = 22;
            checkTmp.fontStyle = FontStyles.Bold;
            checkTmp.color = TEXT_DARK;
            checkTmp.alignment = TextAlignmentOptions.Center;

            completedBadge.SetActive(false);

            // Shine Effect
            GameObject shineEffect = new GameObject("ShineEffect");
            shineEffect.transform.SetParent(cardContainer.transform, false);
            RectTransform shineRT = shineEffect.AddComponent<RectTransform>();
            shineRT.anchorMin = new Vector2(0, 0);
            shineRT.anchorMax = new Vector2(0, 1);
            shineRT.pivot = new Vector2(0.5f, 0.5f);
            shineRT.anchoredPosition = new Vector2(-200, 0);
            shineRT.sizeDelta = new Vector2(60, 0);

            Image shineImage = shineEffect.AddComponent<Image>();
            shineImage.color = new Color(1f, 1f, 1f, 0.15f);
            shineEffect.SetActive(false);

            // Save prefab
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            Object.DestroyImmediate(prefabRoot);

            Debug.Log($"[TrophyShowcase] TrophyCard prefab creado en: {prefabPath}");
        }

        // ==================== UTILITY METHODS ====================

        private static void CreateBottomGlow(GameObject obj, Color color)
        {
            GameObject glow = FindOrCreateChild(obj, "BottomGlow");
            RectTransform glowRT = GetOrAddComponent<RectTransform>(glow);
            glowRT.anchorMin = new Vector2(0, 0);
            glowRT.anchorMax = new Vector2(1, 0);
            glowRT.pivot = new Vector2(0.5f, 1);
            glowRT.anchoredPosition = Vector2.zero;
            glowRT.sizeDelta = new Vector2(0, 3);

            Image glowImage = GetOrAddComponent<Image>(glow);
            glowImage.color = color;
        }

        private static void SetRectTransformStretch(GameObject obj)
        {
            RectTransform rt = GetOrAddComponent<RectTransform>(obj);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }

        private static T GetOrAddComponent<T>(GameObject obj) where T : Component
        {
            T component = obj.GetComponent<T>();
            if (component == null)
                component = obj.AddComponent<T>();
            return component;
        }

        private static GameObject FindOrCreateChild(GameObject parent, string childName)
        {
            Transform child = parent.transform.Find(childName);
            if (child != null) return child.gameObject;

            GameObject newChild = new GameObject(childName);
            newChild.transform.SetParent(parent.transform, false);

            if (newChild.GetComponent<RectTransform>() == null)
                newChild.AddComponent<RectTransform>();

            return newChild;
        }

        private static void SetupButtonColors(Button btn, Color baseColor)
        {
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            btn.colors = colors;
        }

        private static void AddOutline(GameObject obj, Color color, float distance = 1)
        {
            Outline outline = obj.GetComponent<Outline>();
            if (outline == null)
                outline = obj.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(distance, distance);
        }

        private static void MarkSceneDirty()
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
    }
}
