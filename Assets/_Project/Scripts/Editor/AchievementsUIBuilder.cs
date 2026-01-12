using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Construye la UI completa de Achievements (Logros)
    /// Incluye: SafeArea, Header, Stats, Categories, Lista de logros, Detail popup
    /// </summary>
    public class AchievementsUIBuilder : EditorWindow
    {
        // ==================== COLORES DEL TEMA NEON ====================
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.4f, 1f);
        private static readonly Color CYAN_GLOW = new Color(0f, 1f, 1f, 0.3f);

        private static readonly Color DARK_BG = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color PANEL_BG = new Color(0.08f, 0.12f, 0.18f, 0.98f);
        private static readonly Color CARD_BG = new Color(0.06f, 0.1f, 0.15f, 1f);
        private static readonly Color HEADER_BG = new Color(0.03f, 0.06f, 0.1f, 0.95f);
        private static readonly Color POPUP_BG = new Color(0.05f, 0.08f, 0.12f, 0.98f);

        private static readonly Color TEXT_PRIMARY = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.7f, 0.75f, 1f);
        private static readonly Color TEXT_DARK = new Color(0.02f, 0.05f, 0.1f, 1f);

        private static readonly Color BUTTON_PRIMARY = CYAN_NEON;
        private static readonly Color BUTTON_SECONDARY = new Color(0.15f, 0.2f, 0.25f, 1f);
        private static readonly Color BUTTON_SUCCESS = new Color(0.2f, 0.8f, 0.4f, 1f);

        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color SILVER = new Color(0.75f, 0.75f, 0.8f, 1f);
        private static readonly Color BRONZE = new Color(0.8f, 0.5f, 0.2f, 1f);

        private static readonly Color GEM_COLOR = new Color(0.4f, 0.8f, 1f, 1f);
        private static readonly Color COIN_COLOR = new Color(1f, 0.85f, 0.3f, 1f);
        private static readonly Color XP_COLOR = new Color(0.5f, 1f, 0.5f, 1f);

        private static readonly Color ACHIEVEMENT_LOCKED = new Color(0.15f, 0.18f, 0.22f, 1f);
        private static readonly Color ACHIEVEMENT_UNLOCKED = new Color(0.1f, 0.18f, 0.12f, 1f);
        private static readonly Color PROGRESS_BG = new Color(0.1f, 0.12f, 0.15f, 1f);

        private static readonly Color BLOCKER_BG = new Color(0f, 0f, 0f, 0.85f);

        // Category Colors
        private static readonly Color CAT_GAMES = new Color(0.3f, 0.7f, 1f, 1f);
        private static readonly Color CAT_SOCIAL = new Color(1f, 0.5f, 0.7f, 1f);
        private static readonly Color CAT_COLLECTION = new Color(0.6f, 0.3f, 0.9f, 1f);
        private static readonly Color CAT_MASTERY = new Color(1f, 0.6f, 0.2f, 1f);

        // ==================== DIMENSIONES ====================
        private const float HEADER_HEIGHT = 110f;
        private const float STATS_HEIGHT = 100f;
        private const float TABS_HEIGHT = 55f;
        private const float ACHIEVEMENT_CARD_HEIGHT = 120f;
        private const float CONTENT_PADDING = 20f;

        [MenuItem("DigitPark/Monetization/Build Achievements UI", false, 25)]
        public static void BuildUI()
        {
            if (!EditorUtility.DisplayDialog("Achievements UI Builder",
                "Esto construira la UI completa de Achievements.\nAsegurate de tener la escena Achievements abierta.\n\nContinuar?",
                "Si", "No"))
                return;

            BuildCompleteUI();
        }

        private static void BuildCompleteUI()
        {
            Debug.Log("[AchievementsUIBuilder] ========== INICIANDO CONSTRUCCION ==========");

            Canvas canvas = SetupCanvas();
            if (canvas == null) return;

            CreateBackground(canvas);
            GameObject safeArea = CreateSafeArea(canvas);

            CreateHeader(safeArea);
            CreateStatsPanel(safeArea);
            CreateCategoryTabs(safeArea);
            CreateAchievementsScrollView(safeArea);

            CreateAchievementDetailPopup(canvas);
            CreateRewardClaimPopup(canvas);

            MarkSceneDirty();
            Debug.Log("[AchievementsUIBuilder] ========== CONSTRUCCION COMPLETADA ==========");
        }

        // ==================== CANVAS SETUP ====================

        private static Canvas SetupCanvas()
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();

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

        private static void CreateBackground(Canvas canvas)
        {
            GameObject bg = FindOrCreateChild(canvas.gameObject, "Background");

            RectTransform bgRT = GetOrAddComponent<RectTransform>(bg);
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;

            Image bgImage = GetOrAddComponent<Image>(bg);
            bgImage.color = DARK_BG;

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

            CreateBottomGlow(header);

            // BackButton
            GameObject backBtn = FindOrCreateChild(header, "BackButton");
            RectTransform backRT = GetOrAddComponent<RectTransform>(backBtn);
            backRT.anchorMin = new Vector2(0, 0.5f);
            backRT.anchorMax = new Vector2(0, 0.5f);
            backRT.pivot = new Vector2(0, 0.5f);
            backRT.anchoredPosition = new Vector2(20, 0);
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

            // Title
            GameObject titleObj = FindOrCreateChild(header, "TitleText");
            RectTransform titleRT = GetOrAddComponent<RectTransform>(titleObj);
            titleRT.anchorMin = new Vector2(0.5f, 0.5f);
            titleRT.anchorMax = new Vector2(0.5f, 0.5f);
            titleRT.sizeDelta = new Vector2(300, 50);

            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "LOGROS";
            titleText.fontSize = 32;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = CYAN_NEON;
            titleText.alignment = TextAlignmentOptions.Center;
            AddOutline(titleObj, CYAN_GLOW, 2);

            // Trophy Icon
            GameObject trophyIcon = FindOrCreateChild(header, "TrophyIcon");
            RectTransform trophyRT = GetOrAddComponent<RectTransform>(trophyIcon);
            trophyRT.anchorMin = new Vector2(1, 0.5f);
            trophyRT.anchorMax = new Vector2(1, 0.5f);
            trophyRT.pivot = new Vector2(1, 0.5f);
            trophyRT.anchoredPosition = new Vector2(-20, 0);
            trophyRT.sizeDelta = new Vector2(45, 45);

            Image trophyImage = GetOrAddComponent<Image>(trophyIcon);
            trophyImage.color = GOLD;

            Debug.Log("[AchievementsUIBuilder] Header creado");
        }

        // ==================== STATS PANEL ====================

        private static void CreateStatsPanel(GameObject parent)
        {
            GameObject statsPanel = FindOrCreateChild(parent, "StatsPanel");

            RectTransform statsRT = GetOrAddComponent<RectTransform>(statsPanel);
            statsRT.anchorMin = new Vector2(0, 1);
            statsRT.anchorMax = new Vector2(1, 1);
            statsRT.pivot = new Vector2(0.5f, 1);
            statsRT.anchoredPosition = new Vector2(0, -HEADER_HEIGHT - 10);
            statsRT.sizeDelta = new Vector2(-CONTENT_PADDING * 2, STATS_HEIGHT);

            Image statsBg = GetOrAddComponent<Image>(statsPanel);
            statsBg.color = PANEL_BG;
            AddOutline(statsPanel, CYAN_DARK);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(statsPanel);
            hlg.spacing = 0;
            hlg.padding = new RectOffset(15, 15, 15, 15);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;

            // Total Achievements
            CreateStatItem(statsPanel, "TotalAchievements", "27/50", "Logros", CYAN_NEON);

            // Separator
            CreateVerticalSeparator(statsPanel, "Sep1");

            // Points
            CreateStatItem(statsPanel, "TotalPoints", "2,450", "Puntos", GOLD);

            // Separator
            CreateVerticalSeparator(statsPanel, "Sep2");

            // Completion
            CreateStatItem(statsPanel, "Completion", "54%", "Completado", BUTTON_SUCCESS);

            Debug.Log("[AchievementsUIBuilder] StatsPanel creado");
        }

        private static void CreateStatItem(GameObject parent, string name, string value, string label, Color color)
        {
            GameObject item = FindOrCreateChild(parent, name);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(item);
            vlg.spacing = 5;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // Value
            GameObject valueObj = FindOrCreateChild(item, "Value");
            TextMeshProUGUI valueText = GetOrAddComponent<TextMeshProUGUI>(valueObj);
            valueText.text = value;
            valueText.fontSize = 28;
            valueText.fontStyle = FontStyles.Bold;
            valueText.color = color;
            valueText.alignment = TextAlignmentOptions.Center;
            LayoutElement valueLE = GetOrAddComponent<LayoutElement>(valueObj);
            valueLE.minHeight = 35;

            // Label
            GameObject labelObj = FindOrCreateChild(item, "Label");
            TextMeshProUGUI labelText = GetOrAddComponent<TextMeshProUGUI>(labelObj);
            labelText.text = label;
            labelText.fontSize = 13;
            labelText.color = TEXT_SECONDARY;
            labelText.alignment = TextAlignmentOptions.Center;
            LayoutElement labelLE = GetOrAddComponent<LayoutElement>(labelObj);
            labelLE.minHeight = 18;
        }

        private static void CreateVerticalSeparator(GameObject parent, string name)
        {
            GameObject sep = FindOrCreateChild(parent, name);

            Image sepImage = GetOrAddComponent<Image>(sep);
            sepImage.color = CYAN_DARK * 0.5f;

            LayoutElement sepLE = GetOrAddComponent<LayoutElement>(sep);
            sepLE.minWidth = 2;
            sepLE.preferredWidth = 2;
        }

        // ==================== CATEGORY TABS ====================

        private static void CreateCategoryTabs(GameObject parent)
        {
            float topOffset = HEADER_HEIGHT + STATS_HEIGHT + 25;

            GameObject tabsPanel = FindOrCreateChild(parent, "CategoryTabs");

            RectTransform tabsRT = GetOrAddComponent<RectTransform>(tabsPanel);
            tabsRT.anchorMin = new Vector2(0, 1);
            tabsRT.anchorMax = new Vector2(1, 1);
            tabsRT.pivot = new Vector2(0.5f, 1);
            tabsRT.anchoredPosition = new Vector2(0, -topOffset);
            tabsRT.sizeDelta = new Vector2(-CONTENT_PADDING * 2, TABS_HEIGHT);

            Image tabsBg = GetOrAddComponent<Image>(tabsPanel);
            tabsBg.color = new Color(0.04f, 0.07f, 0.11f, 0.9f);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(tabsPanel);
            hlg.spacing = 8f;
            hlg.padding = new RectOffset(8, 8, 5, 5);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // Tabs
            CreateCategoryTab(tabsPanel, "AllTab", "TODOS", true, CYAN_NEON);
            CreateCategoryTab(tabsPanel, "GamesTab", "JUEGOS", false, CAT_GAMES);
            CreateCategoryTab(tabsPanel, "SocialTab", "SOCIAL", false, CAT_SOCIAL);
            CreateCategoryTab(tabsPanel, "CollectionTab", "COLEC.", false, CAT_COLLECTION);

            Debug.Log("[AchievementsUIBuilder] CategoryTabs creado");
        }

        private static void CreateCategoryTab(GameObject parent, string name, string label, bool isActive, Color color)
        {
            GameObject tab = FindOrCreateChild(parent, name);

            Image tabBg = GetOrAddComponent<Image>(tab);
            tabBg.color = isActive ? color : BUTTON_SECONDARY;

            Button tabButton = GetOrAddComponent<Button>(tab);
            SetupButtonColors(tabButton, isActive ? color : BUTTON_SECONDARY);
            AddOutline(tab, isActive ? color : CYAN_DARK * 0.5f);

            GameObject textObj = FindOrCreateChild(tab, "Text");
            TextMeshProUGUI tabText = GetOrAddComponent<TextMeshProUGUI>(textObj);
            tabText.text = label;
            tabText.fontSize = 13;
            tabText.fontStyle = FontStyles.Bold;
            tabText.color = isActive ? TEXT_DARK : TEXT_PRIMARY;
            tabText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(textObj);

            LayoutElement le = GetOrAddComponent<LayoutElement>(tab);
            le.minHeight = 45;
            le.flexibleWidth = 1;
        }

        // ==================== ACHIEVEMENTS SCROLL VIEW ====================

        private static void CreateAchievementsScrollView(GameObject parent)
        {
            float topOffset = HEADER_HEIGHT + STATS_HEIGHT + TABS_HEIGHT + 45;

            GameObject scrollView = FindOrCreateChild(parent, "AchievementsScrollView");

            RectTransform scrollRT = GetOrAddComponent<RectTransform>(scrollView);
            scrollRT.anchorMin = Vector2.zero;
            scrollRT.anchorMax = Vector2.one;
            scrollRT.offsetMin = new Vector2(CONTENT_PADDING, CONTENT_PADDING);
            scrollRT.offsetMax = new Vector2(-CONTENT_PADDING, -topOffset);

            ScrollRect scrollRect = GetOrAddComponent<ScrollRect>(scrollView);
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;

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

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(content);
            vlg.spacing = 12;
            vlg.padding = new RectOffset(0, 0, 10, 30);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Achievement Cards
            CreateAchievementCard(content, "Achievement1", "Primera Victoria", "Gana tu primera partida", 100, GOLD, "50", true, 1);
            CreateAchievementCard(content, "Achievement2", "Jugador Dedicado", "Juega 100 partidas", 75, SILVER, "100", false, 2);
            CreateAchievementCard(content, "Achievement3", "Racha Ganadora", "Gana 5 partidas seguidas", 40, BRONZE, "75", false, 1);
            CreateAchievementCard(content, "Achievement4", "Social", "Agrega 10 amigos", 100, GEM_COLOR, "25", true, 1);
            CreateAchievementCard(content, "Achievement5", "Coleccionista", "Desbloquea 10 avatares", 60, CAT_COLLECTION, "150", false, 2);
            CreateAchievementCard(content, "Achievement6", "Maestro", "Alcanza nivel 50", 24, CAT_MASTERY, "500", false, 3);
            CreateAchievementCard(content, "Achievement7", "Torneo", "Gana un torneo", 0, COIN_COLOR, "200", false, 1);
            CreateAchievementCard(content, "Achievement8", "Veterano", "Juega 30 dias seguidos", 33, GOLD, "300", false, 1);

            Debug.Log("[AchievementsUIBuilder] AchievementsScrollView creado");
        }

        private static void CreateAchievementCard(GameObject parent, string name, string title, string description,
            int progressPercent, Color color, string reward, bool isCompleted, int tier)
        {
            GameObject card = FindOrCreateChild(parent, name);

            Image cardBg = GetOrAddComponent<Image>(card);
            cardBg.color = isCompleted ? ACHIEVEMENT_UNLOCKED : CARD_BG;
            AddOutline(card, isCompleted ? BUTTON_SUCCESS : CYAN_DARK * 0.5f);

            LayoutElement cardLE = GetOrAddComponent<LayoutElement>(card);
            cardLE.minHeight = ACHIEVEMENT_CARD_HEIGHT;
            cardLE.preferredHeight = ACHIEVEMENT_CARD_HEIGHT;

            Button cardBtn = GetOrAddComponent<Button>(card);
            SetupButtonColors(cardBtn, cardBg.color);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(card);
            hlg.spacing = 15;
            hlg.padding = new RectOffset(15, 15, 12, 12);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            // Icon Container
            GameObject iconContainer = FindOrCreateChild(card, "IconContainer");
            LayoutElement iconContainerLE = GetOrAddComponent<LayoutElement>(iconContainer);
            iconContainerLE.minWidth = 70;
            iconContainerLE.minHeight = 70;
            iconContainerLE.preferredWidth = 70;
            iconContainerLE.preferredHeight = 70;

            Image iconBg = GetOrAddComponent<Image>(iconContainer);
            iconBg.color = isCompleted ? color : ACHIEVEMENT_LOCKED;
            AddOutline(iconContainer, isCompleted ? color : CYAN_DARK * 0.3f);

            // Icon
            GameObject iconObj = FindOrCreateChild(iconContainer, "Icon");
            Image iconImage = GetOrAddComponent<Image>(iconObj);
            iconImage.color = isCompleted ? TEXT_DARK : color * 0.5f;
            SetRectTransformStretch(iconObj);
            RectTransform iconRT = iconObj.GetComponent<RectTransform>();
            iconRT.offsetMin = new Vector2(12, 12);
            iconRT.offsetMax = new Vector2(-12, -12);

            // Tier Badge
            if (tier > 1)
            {
                GameObject tierBadge = FindOrCreateChild(iconContainer, "TierBadge");
                RectTransform tierRT = GetOrAddComponent<RectTransform>(tierBadge);
                tierRT.anchorMin = new Vector2(1, 0);
                tierRT.anchorMax = new Vector2(1, 0);
                tierRT.pivot = new Vector2(1, 0);
                tierRT.anchoredPosition = new Vector2(3, -3);
                tierRT.sizeDelta = new Vector2(24, 24);

                Color tierColor = tier == 3 ? GOLD : (tier == 2 ? SILVER : BRONZE);
                Image tierImage = GetOrAddComponent<Image>(tierBadge);
                tierImage.color = tierColor;

                GameObject tierText = FindOrCreateChild(tierBadge, "Text");
                TextMeshProUGUI tierTmp = GetOrAddComponent<TextMeshProUGUI>(tierText);
                tierTmp.text = tier.ToString();
                tierTmp.fontSize = 12;
                tierTmp.fontStyle = FontStyles.Bold;
                tierTmp.color = TEXT_DARK;
                tierTmp.alignment = TextAlignmentOptions.Center;
                SetRectTransformStretch(tierText);
            }

            // Info Panel
            GameObject infoPanel = FindOrCreateChild(card, "InfoPanel");
            VerticalLayoutGroup infoVlg = GetOrAddComponent<VerticalLayoutGroup>(infoPanel);
            infoVlg.spacing = 6;
            infoVlg.childAlignment = TextAnchor.MiddleLeft;
            infoVlg.childControlWidth = true;
            infoVlg.childControlHeight = true;
            infoVlg.childForceExpandWidth = true;
            infoVlg.childForceExpandHeight = false;

            LayoutElement infoLE = GetOrAddComponent<LayoutElement>(infoPanel);
            infoLE.flexibleWidth = 1;

            // Title
            GameObject titleObj = FindOrCreateChild(infoPanel, "Title");
            TextMeshProUGUI titleTmp = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleTmp.text = title;
            titleTmp.fontSize = 18;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = isCompleted ? BUTTON_SUCCESS : TEXT_PRIMARY;
            titleTmp.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minHeight = 24;

            // Description
            GameObject descObj = FindOrCreateChild(infoPanel, "Description");
            TextMeshProUGUI descTmp = GetOrAddComponent<TextMeshProUGUI>(descObj);
            descTmp.text = description;
            descTmp.fontSize = 13;
            descTmp.color = TEXT_SECONDARY;
            descTmp.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement descLE = GetOrAddComponent<LayoutElement>(descObj);
            descLE.minHeight = 18;

            // Progress Row
            if (!isCompleted)
            {
                GameObject progressRow = FindOrCreateChild(infoPanel, "ProgressRow");
                HorizontalLayoutGroup progressHlg = GetOrAddComponent<HorizontalLayoutGroup>(progressRow);
                progressHlg.spacing = 10;
                progressHlg.childAlignment = TextAnchor.MiddleLeft;
                progressHlg.childControlWidth = false;
                progressHlg.childControlHeight = true;
                LayoutElement progressRowLE = GetOrAddComponent<LayoutElement>(progressRow);
                progressRowLE.minHeight = 20;

                // Progress Bar
                GameObject progressBar = FindOrCreateChild(progressRow, "ProgressBar");
                Image progressBarBg = GetOrAddComponent<Image>(progressBar);
                progressBarBg.color = PROGRESS_BG;
                LayoutElement progressBarLE = GetOrAddComponent<LayoutElement>(progressBar);
                progressBarLE.minWidth = 150;
                progressBarLE.minHeight = 10;

                GameObject progressFill = FindOrCreateChild(progressBar, "Fill");
                RectTransform fillRT = GetOrAddComponent<RectTransform>(progressFill);
                fillRT.anchorMin = Vector2.zero;
                fillRT.anchorMax = new Vector2(progressPercent / 100f, 1);
                fillRT.sizeDelta = Vector2.zero;

                Image fillImage = GetOrAddComponent<Image>(progressFill);
                fillImage.color = color;

                // Progress Text
                GameObject progressTextObj = FindOrCreateChild(progressRow, "ProgressText");
                TextMeshProUGUI progressTmp = GetOrAddComponent<TextMeshProUGUI>(progressTextObj);
                progressTmp.text = $"{progressPercent}%";
                progressTmp.fontSize = 12;
                progressTmp.fontStyle = FontStyles.Bold;
                progressTmp.color = TEXT_SECONDARY;
                progressTmp.alignment = TextAlignmentOptions.MidlineLeft;
                LayoutElement progressTextLE = GetOrAddComponent<LayoutElement>(progressTextObj);
                progressTextLE.minWidth = 45;
            }

            // Reward Panel
            GameObject rewardPanel = FindOrCreateChild(card, "RewardPanel");
            VerticalLayoutGroup rewardVlg = GetOrAddComponent<VerticalLayoutGroup>(rewardPanel);
            rewardVlg.spacing = 5;
            rewardVlg.childAlignment = TextAnchor.MiddleCenter;
            rewardVlg.childControlWidth = true;
            rewardVlg.childControlHeight = true;
            rewardVlg.childForceExpandHeight = false;

            LayoutElement rewardLE = GetOrAddComponent<LayoutElement>(rewardPanel);
            rewardLE.minWidth = 70;
            rewardLE.preferredWidth = 70;

            // Reward Icon
            GameObject rewardIcon = FindOrCreateChild(rewardPanel, "RewardIcon");
            Image rewardIconImage = GetOrAddComponent<Image>(rewardIcon);
            rewardIconImage.color = GEM_COLOR;
            LayoutElement rewardIconLE = GetOrAddComponent<LayoutElement>(rewardIcon);
            rewardIconLE.minWidth = 30;
            rewardIconLE.minHeight = 30;
            rewardIconLE.preferredWidth = 30;
            rewardIconLE.preferredHeight = 30;

            // Reward Amount
            GameObject rewardAmount = FindOrCreateChild(rewardPanel, "RewardAmount");
            TextMeshProUGUI rewardTmp = GetOrAddComponent<TextMeshProUGUI>(rewardAmount);
            rewardTmp.text = reward;
            rewardTmp.fontSize = 16;
            rewardTmp.fontStyle = FontStyles.Bold;
            rewardTmp.color = GEM_COLOR;
            rewardTmp.alignment = TextAlignmentOptions.Center;
            LayoutElement rewardAmountLE = GetOrAddComponent<LayoutElement>(rewardAmount);
            rewardAmountLE.minHeight = 22;

            // Status indicator
            if (isCompleted)
            {
                GameObject checkmark = FindOrCreateChild(rewardPanel, "Checkmark");
                Image checkImage = GetOrAddComponent<Image>(checkmark);
                checkImage.color = BUTTON_SUCCESS;
                LayoutElement checkLE = GetOrAddComponent<LayoutElement>(checkmark);
                checkLE.minWidth = 26;
                checkLE.minHeight = 26;

                GameObject checkText = FindOrCreateChild(checkmark, "Text");
                TextMeshProUGUI checkTmp = GetOrAddComponent<TextMeshProUGUI>(checkText);
                checkTmp.text = "✓";
                checkTmp.fontSize = 16;
                checkTmp.fontStyle = FontStyles.Bold;
                checkTmp.color = TEXT_DARK;
                checkTmp.alignment = TextAlignmentOptions.Center;
                SetRectTransformStretch(checkText);
            }
        }

        // ==================== ACHIEVEMENT DETAIL POPUP ====================

        private static void CreateAchievementDetailPopup(Canvas canvas)
        {
            GameObject blocker = FindOrCreateChild(canvas.gameObject, "AchievementDetailBlocker");
            blocker.SetActive(false);

            SetRectTransformStretch(blocker);
            Image blockerBg = GetOrAddComponent<Image>(blocker);
            blockerBg.color = BLOCKER_BG;
            Button blockerBtn = GetOrAddComponent<Button>(blocker);
            blockerBtn.transition = Selectable.Transition.None;
            blocker.transform.SetAsLastSibling();

            GameObject popup = FindOrCreateChild(blocker, "AchievementDetailPopup");
            RectTransform popupRT = GetOrAddComponent<RectTransform>(popup);
            popupRT.anchorMin = new Vector2(0.5f, 0.5f);
            popupRT.anchorMax = new Vector2(0.5f, 0.5f);
            popupRT.sizeDelta = new Vector2(450, 480);

            Image popupBg = GetOrAddComponent<Image>(popup);
            popupBg.color = POPUP_BG;
            AddOutline(popup, CYAN_NEON, 2);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(popup);
            vlg.spacing = 20;
            vlg.padding = new RectOffset(30, 30, 30, 25);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Close Button
            GameObject closeBtn = FindOrCreateChild(popup, "CloseButton");
            RectTransform closeRT = GetOrAddComponent<RectTransform>(closeBtn);
            closeRT.anchorMin = new Vector2(1, 1);
            closeRT.anchorMax = new Vector2(1, 1);
            closeRT.pivot = new Vector2(1, 1);
            closeRT.anchoredPosition = new Vector2(-10, -10);
            closeRT.sizeDelta = new Vector2(35, 35);

            Image closeBg = GetOrAddComponent<Image>(closeBtn);
            closeBg.color = BUTTON_SECONDARY;
            Button closeButton = GetOrAddComponent<Button>(closeBtn);
            SetupButtonColors(closeButton, BUTTON_SECONDARY);

            GameObject closeText = FindOrCreateChild(closeBtn, "Text");
            TextMeshProUGUI closeTmp = GetOrAddComponent<TextMeshProUGUI>(closeText);
            closeTmp.text = "X";
            closeTmp.fontSize = 20;
            closeTmp.fontStyle = FontStyles.Bold;
            closeTmp.color = TEXT_PRIMARY;
            closeTmp.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(closeText);

            // Achievement Icon (large)
            GameObject iconObj = FindOrCreateChild(popup, "AchievementIcon");
            Image iconBg = GetOrAddComponent<Image>(iconObj);
            iconBg.color = GOLD;
            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(iconObj);
            iconLE.minWidth = 100;
            iconLE.minHeight = 100;
            iconLE.preferredWidth = 100;
            iconLE.preferredHeight = 100;

            // Title
            GameObject titleObj = FindOrCreateChild(popup, "Title");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "Primera Victoria";
            titleText.fontSize = 26;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = GOLD;
            titleText.alignment = TextAlignmentOptions.Center;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minHeight = 35;

            // Description
            GameObject descObj = FindOrCreateChild(popup, "Description");
            TextMeshProUGUI descText = GetOrAddComponent<TextMeshProUGUI>(descObj);
            descText.text = "Gana tu primera partida en cualquier modo de juego.";
            descText.fontSize = 16;
            descText.color = TEXT_SECONDARY;
            descText.alignment = TextAlignmentOptions.Center;
            LayoutElement descLE = GetOrAddComponent<LayoutElement>(descObj);
            descLE.minHeight = 45;

            // Progress Section
            GameObject progressSection = FindOrCreateChild(popup, "ProgressSection");
            VerticalLayoutGroup progVlg = GetOrAddComponent<VerticalLayoutGroup>(progressSection);
            progVlg.spacing = 8;
            progVlg.childAlignment = TextAnchor.MiddleCenter;
            progVlg.childControlWidth = true;
            progVlg.childControlHeight = true;
            progVlg.childForceExpandWidth = true;
            progVlg.childForceExpandHeight = false;

            LayoutElement progLE = GetOrAddComponent<LayoutElement>(progressSection);
            progLE.minHeight = 60;

            // Progress Bar
            GameObject progressBar = FindOrCreateChild(progressSection, "ProgressBar");
            Image progressBarBg = GetOrAddComponent<Image>(progressBar);
            progressBarBg.color = PROGRESS_BG;
            LayoutElement progressBarLE = GetOrAddComponent<LayoutElement>(progressBar);
            progressBarLE.minHeight = 20;

            GameObject progressFill = FindOrCreateChild(progressBar, "Fill");
            RectTransform fillRT = GetOrAddComponent<RectTransform>(progressFill);
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = new Vector2(1f, 1); // 100% complete
            fillRT.sizeDelta = Vector2.zero;

            Image fillImage = GetOrAddComponent<Image>(progressFill);
            fillImage.color = BUTTON_SUCCESS;

            // Progress Text
            GameObject progressTextObj = FindOrCreateChild(progressSection, "ProgressText");
            TextMeshProUGUI progressTmp = GetOrAddComponent<TextMeshProUGUI>(progressTextObj);
            progressTmp.text = "1/1 Completado";
            progressTmp.fontSize = 14;
            progressTmp.color = BUTTON_SUCCESS;
            progressTmp.alignment = TextAlignmentOptions.Center;
            LayoutElement progressTextLE = GetOrAddComponent<LayoutElement>(progressTextObj);
            progressTextLE.minHeight = 22;

            // Reward Section
            GameObject rewardSection = FindOrCreateChild(popup, "RewardSection");
            Image rewardBg = GetOrAddComponent<Image>(rewardSection);
            rewardBg.color = CARD_BG;
            LayoutElement rewardSectionLE = GetOrAddComponent<LayoutElement>(rewardSection);
            rewardSectionLE.minHeight = 70;

            HorizontalLayoutGroup rewardHlg = GetOrAddComponent<HorizontalLayoutGroup>(rewardSection);
            rewardHlg.spacing = 15;
            rewardHlg.padding = new RectOffset(20, 20, 15, 15);
            rewardHlg.childAlignment = TextAnchor.MiddleCenter;
            rewardHlg.childControlWidth = false;
            rewardHlg.childControlHeight = true;

            GameObject rewardLabel = FindOrCreateChild(rewardSection, "Label");
            TextMeshProUGUI rewardLabelText = GetOrAddComponent<TextMeshProUGUI>(rewardLabel);
            rewardLabelText.text = "Recompensa:";
            rewardLabelText.fontSize = 16;
            rewardLabelText.color = TEXT_SECONDARY;
            rewardLabelText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement labelLE = GetOrAddComponent<LayoutElement>(rewardLabel);
            labelLE.minWidth = 120;

            GameObject rewardIcon = FindOrCreateChild(rewardSection, "RewardIcon");
            Image rewardIconImage = GetOrAddComponent<Image>(rewardIcon);
            rewardIconImage.color = GEM_COLOR;
            LayoutElement rewardIconLE = GetOrAddComponent<LayoutElement>(rewardIcon);
            rewardIconLE.minWidth = 35;
            rewardIconLE.minHeight = 35;

            GameObject rewardAmount = FindOrCreateChild(rewardSection, "RewardAmount");
            TextMeshProUGUI rewardTmp = GetOrAddComponent<TextMeshProUGUI>(rewardAmount);
            rewardTmp.text = "50 Gemas";
            rewardTmp.fontSize = 20;
            rewardTmp.fontStyle = FontStyles.Bold;
            rewardTmp.color = GEM_COLOR;
            rewardTmp.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement rewardAmountLE = GetOrAddComponent<LayoutElement>(rewardAmount);
            rewardAmountLE.flexibleWidth = 1;

            Debug.Log("[AchievementsUIBuilder] AchievementDetailPopup creado");
        }

        // ==================== REWARD CLAIM POPUP ====================

        private static void CreateRewardClaimPopup(Canvas canvas)
        {
            GameObject blocker = FindOrCreateChild(canvas.gameObject, "RewardClaimBlocker");
            blocker.SetActive(false);

            SetRectTransformStretch(blocker);
            Image blockerBg = GetOrAddComponent<Image>(blocker);
            blockerBg.color = BLOCKER_BG;
            Button blockerBtn = GetOrAddComponent<Button>(blocker);
            blockerBtn.transition = Selectable.Transition.None;
            blocker.transform.SetAsLastSibling();

            GameObject popup = FindOrCreateChild(blocker, "RewardClaimPopup");
            RectTransform popupRT = GetOrAddComponent<RectTransform>(popup);
            popupRT.anchorMin = new Vector2(0.5f, 0.5f);
            popupRT.anchorMax = new Vector2(0.5f, 0.5f);
            popupRT.sizeDelta = new Vector2(400, 350);

            Image popupBg = GetOrAddComponent<Image>(popup);
            popupBg.color = POPUP_BG;
            AddOutline(popup, GOLD, 3);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(popup);
            vlg.spacing = 20;
            vlg.padding = new RectOffset(30, 30, 35, 25);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Trophy Icon
            GameObject trophyObj = FindOrCreateChild(popup, "TrophyIcon");
            Image trophyImage = GetOrAddComponent<Image>(trophyObj);
            trophyImage.color = GOLD;
            LayoutElement trophyLE = GetOrAddComponent<LayoutElement>(trophyObj);
            trophyLE.minWidth = 80;
            trophyLE.minHeight = 80;
            trophyLE.preferredWidth = 80;
            trophyLE.preferredHeight = 80;

            // Title
            GameObject titleObj = FindOrCreateChild(popup, "Title");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "Logro Desbloqueado!";
            titleText.fontSize = 26;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = GOLD;
            titleText.alignment = TextAlignmentOptions.Center;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minHeight = 35;

            // Achievement Name
            GameObject nameObj = FindOrCreateChild(popup, "AchievementName");
            TextMeshProUGUI nameText = GetOrAddComponent<TextMeshProUGUI>(nameObj);
            nameText.text = "Primera Victoria";
            nameText.fontSize = 18;
            nameText.color = TEXT_SECONDARY;
            nameText.alignment = TextAlignmentOptions.Center;
            LayoutElement nameLE = GetOrAddComponent<LayoutElement>(nameObj);
            nameLE.minHeight = 25;

            // Reward Display
            GameObject rewardDisplay = FindOrCreateChild(popup, "RewardDisplay");
            HorizontalLayoutGroup rewardHlg = GetOrAddComponent<HorizontalLayoutGroup>(rewardDisplay);
            rewardHlg.spacing = 10;
            rewardHlg.childAlignment = TextAnchor.MiddleCenter;
            rewardHlg.childControlWidth = false;
            rewardHlg.childControlHeight = true;
            LayoutElement rewardDisplayLE = GetOrAddComponent<LayoutElement>(rewardDisplay);
            rewardDisplayLE.minHeight = 45;

            GameObject rewardIcon = FindOrCreateChild(rewardDisplay, "Icon");
            Image rewardIconImage = GetOrAddComponent<Image>(rewardIcon);
            rewardIconImage.color = GEM_COLOR;
            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(rewardIcon);
            iconLE.minWidth = 40;
            iconLE.minHeight = 40;

            GameObject rewardAmount = FindOrCreateChild(rewardDisplay, "Amount");
            TextMeshProUGUI rewardTmp = GetOrAddComponent<TextMeshProUGUI>(rewardAmount);
            rewardTmp.text = "+50";
            rewardTmp.fontSize = 32;
            rewardTmp.fontStyle = FontStyles.Bold;
            rewardTmp.color = GEM_COLOR;
            rewardTmp.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement amountLE = GetOrAddComponent<LayoutElement>(rewardAmount);
            amountLE.minWidth = 80;

            // Collect Button
            GameObject collectBtn = FindOrCreateChild(popup, "CollectButton");
            Image collectBg = GetOrAddComponent<Image>(collectBtn);
            collectBg.color = BUTTON_SUCCESS;
            Button collectButton = GetOrAddComponent<Button>(collectBtn);
            SetupButtonColors(collectButton, BUTTON_SUCCESS);
            LayoutElement collectLE = GetOrAddComponent<LayoutElement>(collectBtn);
            collectLE.minHeight = 50;

            GameObject collectText = FindOrCreateChild(collectBtn, "Text");
            TextMeshProUGUI collectTmp = GetOrAddComponent<TextMeshProUGUI>(collectText);
            collectTmp.text = "Recoger";
            collectTmp.fontSize = 20;
            collectTmp.fontStyle = FontStyles.Bold;
            collectTmp.color = TEXT_DARK;
            collectTmp.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(collectText);

            Debug.Log("[AchievementsUIBuilder] RewardClaimPopup creado");
        }

        // ==================== UTILITY METHODS ====================

        private static void CreateBottomGlow(GameObject obj)
        {
            GameObject glow = FindOrCreateChild(obj, "BottomGlow");
            RectTransform glowRT = GetOrAddComponent<RectTransform>(glow);
            glowRT.anchorMin = new Vector2(0, 0);
            glowRT.anchorMax = new Vector2(1, 0);
            glowRT.pivot = new Vector2(0.5f, 1);
            glowRT.anchoredPosition = Vector2.zero;
            glowRT.sizeDelta = new Vector2(0, 3);

            Image glowImage = GetOrAddComponent<Image>(glow);
            glowImage.color = CYAN_NEON;
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
