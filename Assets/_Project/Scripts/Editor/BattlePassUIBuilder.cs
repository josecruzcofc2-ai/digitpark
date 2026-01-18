using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    using DigitPark.Animations;
    /// <summary>
    /// Construye la UI completa de Battle Pass (Pase de Batalla)
    /// Incluye: SafeArea, Header, Season Info, Level Progress, Tier ScrollView, Premium Purchase
    /// </summary>
    public class BattlePassUIBuilder : EditorWindow
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
        private static readonly Color GOLD_DARK = new Color(0.7f, 0.55f, 0f, 1f);
        private static readonly Color PURPLE_PREMIUM = new Color(0.6f, 0.3f, 0.9f, 1f);
        private static readonly Color PURPLE_DARK = new Color(0.3f, 0.15f, 0.5f, 1f);

        private static readonly Color GEM_COLOR = new Color(0.4f, 0.8f, 1f, 1f);
        private static readonly Color COIN_COLOR = new Color(1f, 0.85f, 0.3f, 1f);
        private static readonly Color XP_COLOR = new Color(0.5f, 1f, 0.5f, 1f);

        private static readonly Color FREE_TRACK = new Color(0.1f, 0.15f, 0.2f, 1f);
        private static readonly Color PREMIUM_TRACK = new Color(0.2f, 0.12f, 0.35f, 1f);
        private static readonly Color TIER_CLAIMED = new Color(0.08f, 0.15f, 0.08f, 1f);
        private static readonly Color TIER_CURRENT = new Color(0.1f, 0.2f, 0.25f, 1f);
        private static readonly Color TIER_LOCKED = new Color(0.04f, 0.06f, 0.08f, 0.9f);

        private static readonly Color BLOCKER_BG = new Color(0f, 0f, 0f, 0.85f);

        // ==================== DIMENSIONES ====================
        private const float HEADER_HEIGHT = 110f;
        private const float SEASON_INFO_HEIGHT = 65f;
        private const float LEVEL_PROGRESS_HEIGHT = 90f;
        private const float TIER_WIDTH = 150f;
        private const float TIER_HEIGHT = 280f;
        private const float CONTENT_PADDING = 20f;
        private const float BOTTOM_BUTTON_HEIGHT = 80f;

        [MenuItem("DigitPark/UI Builders/Monetization/BattlePass", false, 181)]
        public static void BuildUI()
        {
            if (!EditorUtility.DisplayDialog("Battle Pass UI Builder",
                "Esto construira la UI completa de Battle Pass.\nAsegurate de tener la escena BattlePass abierta.\n\nContinuar?",
                "Si", "No"))
                return;

            BuildCompleteUI();
        }

        private static void BuildCompleteUI()
        {
            Debug.Log("[BattlePassUIBuilder] ========== INICIANDO CONSTRUCCION ==========");

            Canvas canvas = SetupCanvas();
            if (canvas == null) return;

            CreateBackground(canvas);
            GameObject safeArea = CreateSafeArea(canvas);

            CreateHeader(safeArea);
            CreateSeasonInfo(safeArea);
            CreateLevelProgress(safeArea);
            CreateTiersScrollView(safeArea);
            CreatePremiumPurchaseButton(safeArea);

            CreateTierDetailPopup(canvas);
            CreatePurchaseConfirmPopup(canvas);

            MarkSceneDirty();
            Debug.Log("[BattlePassUIBuilder] ========== CONSTRUCCION COMPLETADA ==========");
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

            CreateBottomGlow(header, PURPLE_PREMIUM);

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
            titleRT.sizeDelta = new Vector2(400, 50);

            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "PASE DE BATALLA";
            titleText.fontSize = 30;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = PURPLE_PREMIUM;
            titleText.alignment = TextAlignmentOptions.Center;
            AddOutline(titleObj, new Color(0.6f, 0.3f, 0.9f, 0.4f), 2);

            // Premium Badge
            GameObject premiumBadge = FindOrCreateChild(header, "PremiumBadge");
            RectTransform premiumRT = GetOrAddComponent<RectTransform>(premiumBadge);
            premiumRT.anchorMin = new Vector2(1, 0.5f);
            premiumRT.anchorMax = new Vector2(1, 0.5f);
            premiumRT.pivot = new Vector2(1, 0.5f);
            premiumRT.anchoredPosition = new Vector2(-20, 0);
            premiumRT.sizeDelta = new Vector2(90, 35);

            Image premiumBg = GetOrAddComponent<Image>(premiumBadge);
            premiumBg.color = GOLD;

            GameObject premiumTextObj = FindOrCreateChild(premiumBadge, "Text");
            TextMeshProUGUI premiumText = GetOrAddComponent<TextMeshProUGUI>(premiumTextObj);
            premiumText.text = "PREMIUM";
            premiumText.fontSize = 12;
            premiumText.fontStyle = FontStyles.Bold;
            premiumText.color = TEXT_DARK;
            premiumText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(premiumTextObj);

            Debug.Log("[BattlePassUIBuilder] Header creado");
        }

        // ==================== SEASON INFO ====================

        private static void CreateSeasonInfo(GameObject parent)
        {
            GameObject seasonPanel = FindOrCreateChild(parent, "SeasonInfo");

            RectTransform seasonRT = GetOrAddComponent<RectTransform>(seasonPanel);
            seasonRT.anchorMin = new Vector2(0, 1);
            seasonRT.anchorMax = new Vector2(1, 1);
            seasonRT.pivot = new Vector2(0.5f, 1);
            seasonRT.anchoredPosition = new Vector2(0, -HEADER_HEIGHT - 10);
            seasonRT.sizeDelta = new Vector2(-CONTENT_PADDING * 2, SEASON_INFO_HEIGHT);

            Image seasonBg = GetOrAddComponent<Image>(seasonPanel);
            seasonBg.color = PREMIUM_TRACK;
            AddOutline(seasonPanel, PURPLE_PREMIUM * 0.7f);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(seasonPanel);
            hlg.spacing = 15;
            hlg.padding = new RectOffset(20, 20, 10, 10);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            // Season Icon
            GameObject seasonIcon = FindOrCreateChild(seasonPanel, "SeasonIcon");
            Image seasonIconImage = GetOrAddComponent<Image>(seasonIcon);
            seasonIconImage.color = PURPLE_PREMIUM;
            LayoutElement seasonIconLE = GetOrAddComponent<LayoutElement>(seasonIcon);
            seasonIconLE.minWidth = 45;
            seasonIconLE.minHeight = 45;

            // Season Name
            GameObject seasonName = FindOrCreateChild(seasonPanel, "SeasonName");
            VerticalLayoutGroup nameVlg = GetOrAddComponent<VerticalLayoutGroup>(seasonName);
            nameVlg.spacing = 2;
            nameVlg.childAlignment = TextAnchor.MiddleLeft;
            nameVlg.childControlWidth = true;
            nameVlg.childControlHeight = true;
            nameVlg.childForceExpandHeight = false;

            LayoutElement nameLE = GetOrAddComponent<LayoutElement>(seasonName);
            nameLE.flexibleWidth = 1;

            GameObject seasonLabel = FindOrCreateChild(seasonName, "Label");
            TextMeshProUGUI seasonLabelText = GetOrAddComponent<TextMeshProUGUI>(seasonLabel);
            seasonLabelText.text = "TEMPORADA 1";
            seasonLabelText.fontSize = 22;
            seasonLabelText.fontStyle = FontStyles.Bold;
            seasonLabelText.color = TEXT_PRIMARY;
            seasonLabelText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement labelLE = GetOrAddComponent<LayoutElement>(seasonLabel);
            labelLE.minHeight = 28;

            GameObject seasonTheme = FindOrCreateChild(seasonName, "Theme");
            TextMeshProUGUI seasonThemeText = GetOrAddComponent<TextMeshProUGUI>(seasonTheme);
            seasonThemeText.text = "Neon Dreams";
            seasonThemeText.fontSize = 14;
            seasonThemeText.color = PURPLE_PREMIUM;
            seasonThemeText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement themeLE = GetOrAddComponent<LayoutElement>(seasonTheme);
            themeLE.minHeight = 18;

            // Timer
            GameObject timerPanel = FindOrCreateChild(seasonPanel, "TimerPanel");
            VerticalLayoutGroup timerVlg = GetOrAddComponent<VerticalLayoutGroup>(timerPanel);
            timerVlg.spacing = 2;
            timerVlg.childAlignment = TextAnchor.MiddleRight;
            timerVlg.childControlWidth = true;
            timerVlg.childControlHeight = true;
            timerVlg.childForceExpandHeight = false;

            LayoutElement timerLE = GetOrAddComponent<LayoutElement>(timerPanel);
            timerLE.minWidth = 130;

            GameObject timerLabel = FindOrCreateChild(timerPanel, "Label");
            TextMeshProUGUI timerLabelText = GetOrAddComponent<TextMeshProUGUI>(timerLabel);
            timerLabelText.text = "Termina en:";
            timerLabelText.fontSize = 12;
            timerLabelText.color = TEXT_SECONDARY;
            timerLabelText.alignment = TextAlignmentOptions.MidlineRight;
            LayoutElement timerLabelLE = GetOrAddComponent<LayoutElement>(timerLabel);
            timerLabelLE.minHeight = 16;

            GameObject timerValue = FindOrCreateChild(timerPanel, "Value");
            TextMeshProUGUI timerValueText = GetOrAddComponent<TextMeshProUGUI>(timerValue);
            timerValueText.text = "23d 12h 34m";
            timerValueText.fontSize = 16;
            timerValueText.fontStyle = FontStyles.Bold;
            timerValueText.color = GOLD;
            timerValueText.alignment = TextAlignmentOptions.MidlineRight;
            LayoutElement timerValueLE = GetOrAddComponent<LayoutElement>(timerValue);
            timerValueLE.minHeight = 22;

            Debug.Log("[BattlePassUIBuilder] SeasonInfo creado");
        }

        // ==================== LEVEL PROGRESS ====================

        private static void CreateLevelProgress(GameObject parent)
        {
            float topOffset = HEADER_HEIGHT + SEASON_INFO_HEIGHT + 25;

            GameObject levelPanel = FindOrCreateChild(parent, "LevelProgress");

            RectTransform levelRT = GetOrAddComponent<RectTransform>(levelPanel);
            levelRT.anchorMin = new Vector2(0, 1);
            levelRT.anchorMax = new Vector2(1, 1);
            levelRT.pivot = new Vector2(0.5f, 1);
            levelRT.anchoredPosition = new Vector2(0, -topOffset);
            levelRT.sizeDelta = new Vector2(-CONTENT_PADDING * 2, LEVEL_PROGRESS_HEIGHT);

            Image levelBg = GetOrAddComponent<Image>(levelPanel);
            levelBg.color = PANEL_BG;
            AddOutline(levelPanel, CYAN_DARK);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(levelPanel);
            vlg.spacing = 10;
            vlg.padding = new RectOffset(20, 20, 12, 12);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Top Row (Level info)
            GameObject topRow = FindOrCreateChild(levelPanel, "TopRow");
            HorizontalLayoutGroup topHlg = GetOrAddComponent<HorizontalLayoutGroup>(topRow);
            topHlg.spacing = 0;
            topHlg.childAlignment = TextAnchor.MiddleCenter;
            topHlg.childControlWidth = true;
            topHlg.childControlHeight = true;
            topHlg.childForceExpandWidth = true;

            LayoutElement topLE = GetOrAddComponent<LayoutElement>(topRow);
            topLE.minHeight = 30;

            // Current Level
            GameObject currentLevel = FindOrCreateChild(topRow, "CurrentLevel");
            HorizontalLayoutGroup currentHlg = GetOrAddComponent<HorizontalLayoutGroup>(currentLevel);
            currentHlg.spacing = 8;
            currentHlg.childAlignment = TextAnchor.MiddleLeft;
            currentHlg.childControlWidth = false;
            currentHlg.childControlHeight = true;

            GameObject levelLabel = FindOrCreateChild(currentLevel, "Label");
            TextMeshProUGUI levelLabelText = GetOrAddComponent<TextMeshProUGUI>(levelLabel);
            levelLabelText.text = "NIVEL";
            levelLabelText.fontSize = 14;
            levelLabelText.color = TEXT_SECONDARY;
            levelLabelText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement levelLabelLE = GetOrAddComponent<LayoutElement>(levelLabel);
            levelLabelLE.minWidth = 55;

            GameObject levelNumber = FindOrCreateChild(currentLevel, "Number");
            TextMeshProUGUI levelNumberText = GetOrAddComponent<TextMeshProUGUI>(levelNumber);
            levelNumberText.text = "12";
            levelNumberText.fontSize = 28;
            levelNumberText.fontStyle = FontStyles.Bold;
            levelNumberText.color = CYAN_NEON;
            levelNumberText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement levelNumberLE = GetOrAddComponent<LayoutElement>(levelNumber);
            levelNumberLE.minWidth = 50;

            // XP Info
            GameObject xpInfo = FindOrCreateChild(topRow, "XPInfo");
            TextMeshProUGUI xpInfoText = GetOrAddComponent<TextMeshProUGUI>(xpInfo);
            xpInfoText.text = "750 / 1000 XP";
            xpInfoText.fontSize = 16;
            xpInfoText.color = TEXT_SECONDARY;
            xpInfoText.alignment = TextAlignmentOptions.MidlineRight;

            // Progress Bar
            GameObject progressBar = FindOrCreateChild(levelPanel, "ProgressBar");
            LayoutElement progressLE = GetOrAddComponent<LayoutElement>(progressBar);
            progressLE.minHeight = 25;
            progressLE.preferredHeight = 25;

            Image progressBg = GetOrAddComponent<Image>(progressBar);
            progressBg.color = new Color(0.1f, 0.12f, 0.15f, 1f);
            AddOutline(progressBar, CYAN_DARK * 0.5f);

            // Fill
            GameObject fill = FindOrCreateChild(progressBar, "Fill");
            RectTransform fillRT = GetOrAddComponent<RectTransform>(fill);
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = new Vector2(0.75f, 1); // 75% progress
            fillRT.sizeDelta = Vector2.zero;

            Image fillImage = GetOrAddComponent<Image>(fill);
            fillImage.color = XP_COLOR;

            // Progress Text
            GameObject progressText = FindOrCreateChild(progressBar, "Text");
            TextMeshProUGUI progressTmp = GetOrAddComponent<TextMeshProUGUI>(progressText);
            progressTmp.text = "75%";
            progressTmp.fontSize = 14;
            progressTmp.fontStyle = FontStyles.Bold;
            progressTmp.color = TEXT_PRIMARY;
            progressTmp.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(progressText);

            Debug.Log("[BattlePassUIBuilder] LevelProgress creado");
        }

        // ==================== TIERS SCROLL VIEW ====================

        private static void CreateTiersScrollView(GameObject parent)
        {
            float topOffset = HEADER_HEIGHT + SEASON_INFO_HEIGHT + LEVEL_PROGRESS_HEIGHT + 50;

            GameObject scrollView = FindOrCreateChild(parent, "TiersScrollView");

            RectTransform scrollRT = GetOrAddComponent<RectTransform>(scrollView);
            scrollRT.anchorMin = Vector2.zero;
            scrollRT.anchorMax = Vector2.one;
            scrollRT.offsetMin = new Vector2(0, BOTTOM_BUTTON_HEIGHT + 30);
            scrollRT.offsetMax = new Vector2(0, -topOffset);

            ScrollRect scrollRect = GetOrAddComponent<ScrollRect>(scrollView);
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;

            // Viewport
            GameObject viewport = FindOrCreateChild(scrollView, "Viewport");
            SetRectTransformStretch(viewport);
            RectTransform viewportRT = viewport.GetComponent<RectTransform>();
            GetOrAddComponent<RectMask2D>(viewport);
            scrollRect.viewport = viewportRT;

            // Content
            GameObject content = FindOrCreateChild(viewport, "Content");
            RectTransform contentRT = GetOrAddComponent<RectTransform>(content);
            contentRT.anchorMin = new Vector2(0, 0);
            contentRT.anchorMax = new Vector2(0, 1);
            contentRT.pivot = new Vector2(0, 0.5f);
            scrollRect.content = contentRT;

            ContentSizeFitter csf = GetOrAddComponent<ContentSizeFitter>(content);
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(content);
            hlg.spacing = 0;
            hlg.padding = new RectOffset(20, 20, 10, 10);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            // Create 30 tiers
            for (int i = 1; i <= 30; i++)
            {
                bool isClaimed = i <= 10;
                bool isCurrent = i == 12;
                bool isLocked = i > 12;

                CreateTierCard(content, $"Tier_{i}", i, isClaimed, isCurrent, isLocked);
            }

            // Add BattlePassController for Clash Royale-style animations
            AddBattlePassController(scrollView, scrollRect, contentRT, viewportRT);

            Debug.Log("[BattlePassUIBuilder] TiersScrollView creado con BattlePassController");
        }

        /// <summary>
        /// Adds and configures the BattlePassController for professional animations
        /// </summary>
        private static void AddBattlePassController(GameObject scrollView, ScrollRect scrollRect, RectTransform content, RectTransform viewport)
        {
            BattlePassController controller = scrollView.GetComponent<BattlePassController>();
            if (controller == null)
                controller = scrollView.AddComponent<BattlePassController>();

            // Configure via SerializedObject for editor
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("scrollRect").objectReferenceValue = scrollRect;
            so.FindProperty("content").objectReferenceValue = content;
            so.FindProperty("viewport").objectReferenceValue = viewport;
            so.FindProperty("tierWidth").floatValue = TIER_WIDTH;
            so.FindProperty("tierSpacing").floatValue = 0f;
            so.FindProperty("totalTiers").intValue = 30;
            so.FindProperty("currentTier").intValue = 12;

            // Snap settings for smooth experience
            so.FindProperty("enableSnap").boolValue = true;
            so.FindProperty("snapSpeed").floatValue = 10f;

            // Scale effect for Clash Royale-style depth
            so.FindProperty("selectedScale").floatValue = 1.15f;
            so.FindProperty("nearbyScale").floatValue = 1.0f;
            so.FindProperty("farScale").floatValue = 0.85f;

            // Glow effect
            so.FindProperty("enableGlow").boolValue = true;
            so.FindProperty("glowColor").colorValue = new Color(0.6f, 0.3f, 0.9f, 0.5f); // Purple to match theme

            so.ApplyModifiedProperties();

            Debug.Log("[BattlePassUIBuilder] BattlePassController configurado");
        }

        private static void CreateTierCard(GameObject parent, string name, int tierNumber, bool isClaimed, bool isCurrent, bool isLocked)
        {
            GameObject tier = FindOrCreateChild(parent, name);

            LayoutElement tierLE = GetOrAddComponent<LayoutElement>(tier);
            tierLE.minWidth = TIER_WIDTH;
            tierLE.preferredWidth = TIER_WIDTH;

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(tier);
            vlg.spacing = 8;
            vlg.padding = new RectOffset(5, 5, 0, 0);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;

            // Tier Number
            GameObject tierNumberObj = FindOrCreateChild(tier, "TierNumber");
            Image tierNumberBg = GetOrAddComponent<Image>(tierNumberObj);
            tierNumberBg.color = isCurrent ? CYAN_NEON : (isClaimed ? BUTTON_SUCCESS : BUTTON_SECONDARY);

            LayoutElement numberLE = GetOrAddComponent<LayoutElement>(tierNumberObj);
            numberLE.minHeight = 30;
            numberLE.preferredHeight = 30;

            GameObject numberText = FindOrCreateChild(tierNumberObj, "Text");
            TextMeshProUGUI numberTmp = GetOrAddComponent<TextMeshProUGUI>(numberText);
            numberTmp.text = tierNumber.ToString();
            numberTmp.fontSize = 16;
            numberTmp.fontStyle = FontStyles.Bold;
            numberTmp.color = isCurrent || isClaimed ? TEXT_DARK : TEXT_PRIMARY;
            numberTmp.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(numberText);

            // Free Reward
            CreateRewardSlot(tier, "FreeReward", false, isClaimed, isCurrent, isLocked, tierNumber);

            // Connector Line
            GameObject connector = FindOrCreateChild(tier, "Connector");
            Image connectorImage = GetOrAddComponent<Image>(connector);
            connectorImage.color = isClaimed ? BUTTON_SUCCESS : CYAN_DARK * 0.5f;
            LayoutElement connectorLE = GetOrAddComponent<LayoutElement>(connector);
            connectorLE.minHeight = 15;
            connectorLE.preferredHeight = 15;

            // Premium Reward
            CreateRewardSlot(tier, "PremiumReward", true, isClaimed, isCurrent, isLocked, tierNumber);
        }

        private static void CreateRewardSlot(GameObject parent, string name, bool isPremium, bool isClaimed, bool isCurrent, bool isLocked, int tierNumber)
        {
            GameObject slot = FindOrCreateChild(parent, name);

            Color bgColor;
            if (isClaimed)
                bgColor = TIER_CLAIMED;
            else if (isCurrent)
                bgColor = TIER_CURRENT;
            else
                bgColor = isPremium ? PREMIUM_TRACK : FREE_TRACK;

            Image slotBg = GetOrAddComponent<Image>(slot);
            slotBg.color = bgColor;

            Color outlineColor = isCurrent ? (isPremium ? PURPLE_PREMIUM : CYAN_NEON) :
                                 isClaimed ? BUTTON_SUCCESS : (isPremium ? PURPLE_DARK : CYAN_DARK * 0.5f);
            AddOutline(slot, outlineColor, isCurrent ? 2 : 1);

            LayoutElement slotLE = GetOrAddComponent<LayoutElement>(slot);
            slotLE.minHeight = 95;
            slotLE.preferredHeight = 95;

            Button slotBtn = GetOrAddComponent<Button>(slot);
            SetupButtonColors(slotBtn, bgColor);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(slot);
            vlg.spacing = 5;
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // Premium Lock Icon (if premium and not owned)
            if (isPremium && !isClaimed)
            {
                GameObject lockBadge = FindOrCreateChild(slot, "LockBadge");
                RectTransform lockRT = GetOrAddComponent<RectTransform>(lockBadge);
                lockRT.anchorMin = new Vector2(0, 1);
                lockRT.anchorMax = new Vector2(0, 1);
                lockRT.pivot = new Vector2(0, 1);
                lockRT.anchoredPosition = new Vector2(5, -5);
                lockRT.sizeDelta = new Vector2(22, 22);

                Image lockImage = GetOrAddComponent<Image>(lockBadge);
                lockImage.color = GOLD;

                GameObject lockTextObj = FindOrCreateChild(lockBadge, "Text");
                TextMeshProUGUI lockText = GetOrAddComponent<TextMeshProUGUI>(lockTextObj);
                lockText.text = "★";
                lockText.fontSize = 12;
                lockText.color = TEXT_DARK;
                lockText.alignment = TextAlignmentOptions.Center;
                SetRectTransformStretch(lockTextObj);
            }

            // Reward Icon
            Color rewardColor = GetRewardColor(tierNumber, isPremium);
            GameObject rewardIcon = FindOrCreateChild(slot, "RewardIcon");
            Image rewardIconImage = GetOrAddComponent<Image>(rewardIcon);
            rewardIconImage.color = isClaimed ? rewardColor * 0.5f : rewardColor;
            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(rewardIcon);
            iconLE.minHeight = 40;
            iconLE.preferredHeight = 40;
            iconLE.minWidth = 40;
            iconLE.preferredWidth = 40;

            // Reward Amount
            GameObject rewardAmount = FindOrCreateChild(slot, "RewardAmount");
            TextMeshProUGUI rewardText = GetOrAddComponent<TextMeshProUGUI>(rewardAmount);
            rewardText.text = GetRewardAmount(tierNumber, isPremium);
            rewardText.fontSize = 14;
            rewardText.fontStyle = FontStyles.Bold;
            rewardText.color = isClaimed ? TEXT_SECONDARY : TEXT_PRIMARY;
            rewardText.alignment = TextAlignmentOptions.Center;
            LayoutElement amountLE = GetOrAddComponent<LayoutElement>(rewardAmount);
            amountLE.minHeight = 20;

            // Claimed checkmark
            if (isClaimed)
            {
                GameObject checkmark = FindOrCreateChild(slot, "Checkmark");
                RectTransform checkRT = GetOrAddComponent<RectTransform>(checkmark);
                checkRT.anchorMin = new Vector2(1, 1);
                checkRT.anchorMax = new Vector2(1, 1);
                checkRT.pivot = new Vector2(1, 1);
                checkRT.anchoredPosition = new Vector2(-5, -5);
                checkRT.sizeDelta = new Vector2(22, 22);

                Image checkImage = GetOrAddComponent<Image>(checkmark);
                checkImage.color = BUTTON_SUCCESS;

                GameObject checkText = FindOrCreateChild(checkmark, "Text");
                TextMeshProUGUI checkTmp = GetOrAddComponent<TextMeshProUGUI>(checkText);
                checkTmp.text = "✓";
                checkTmp.fontSize = 14;
                checkTmp.fontStyle = FontStyles.Bold;
                checkTmp.color = TEXT_DARK;
                checkTmp.alignment = TextAlignmentOptions.Center;
                SetRectTransformStretch(checkText);
            }
        }

        private static Color GetRewardColor(int tier, bool isPremium)
        {
            if (tier % 5 == 0) return GOLD; // Special tier
            if (isPremium)
            {
                return tier % 3 == 0 ? GEM_COLOR : PURPLE_PREMIUM;
            }
            else
            {
                return tier % 2 == 0 ? COIN_COLOR : GEM_COLOR;
            }
        }

        private static string GetRewardAmount(int tier, bool isPremium)
        {
            if (tier % 5 == 0)
            {
                return isPremium ? "Skin" : "Cofre";
            }

            int baseAmount = isPremium ? 50 : 25;
            int amount = baseAmount * tier;

            return tier % 3 == 0 ? $"{amount / 2}" : $"{amount}";
        }

        // ==================== PREMIUM PURCHASE BUTTON ====================

        private static void CreatePremiumPurchaseButton(GameObject parent)
        {
            GameObject buttonArea = FindOrCreateChild(parent, "PremiumPurchaseArea");

            RectTransform buttonRT = GetOrAddComponent<RectTransform>(buttonArea);
            buttonRT.anchorMin = new Vector2(0, 0);
            buttonRT.anchorMax = new Vector2(1, 0);
            buttonRT.pivot = new Vector2(0.5f, 0);
            buttonRT.anchoredPosition = new Vector2(0, 15);
            buttonRT.sizeDelta = new Vector2(-CONTENT_PADDING * 2, BOTTOM_BUTTON_HEIGHT);

            Image buttonBg = GetOrAddComponent<Image>(buttonArea);
            buttonBg.color = PURPLE_PREMIUM;
            Button button = GetOrAddComponent<Button>(buttonArea);
            SetupButtonColors(button, PURPLE_PREMIUM);
            AddOutline(buttonArea, GOLD, 2);

            // Glow effect
            Shadow glow = GetOrAddComponent<Shadow>(buttonArea);
            glow.effectColor = new Color(0.6f, 0.3f, 0.9f, 0.4f);
            glow.effectDistance = new Vector2(0, -4);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(buttonArea);
            hlg.spacing = 15;
            hlg.padding = new RectOffset(25, 25, 12, 12);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            // Premium Icon
            GameObject premiumIcon = FindOrCreateChild(buttonArea, "PremiumIcon");
            Image premiumIconImage = GetOrAddComponent<Image>(premiumIcon);
            premiumIconImage.color = GOLD;
            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(premiumIcon);
            iconLE.minWidth = 45;
            iconLE.minHeight = 45;

            // Text Content
            GameObject textContent = FindOrCreateChild(buttonArea, "TextContent");
            VerticalLayoutGroup textVlg = GetOrAddComponent<VerticalLayoutGroup>(textContent);
            textVlg.spacing = 3;
            textVlg.childAlignment = TextAnchor.MiddleLeft;
            textVlg.childControlWidth = true;
            textVlg.childControlHeight = true;
            textVlg.childForceExpandHeight = false;

            LayoutElement textLE = GetOrAddComponent<LayoutElement>(textContent);
            textLE.flexibleWidth = 1;

            GameObject mainText = FindOrCreateChild(textContent, "MainText");
            TextMeshProUGUI mainTmp = GetOrAddComponent<TextMeshProUGUI>(mainText);
            mainTmp.text = "DESBLOQUEAR PASE PREMIUM";
            mainTmp.fontSize = 18;
            mainTmp.fontStyle = FontStyles.Bold;
            mainTmp.color = TEXT_PRIMARY;
            mainTmp.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement mainLE = GetOrAddComponent<LayoutElement>(mainText);
            mainLE.minHeight = 24;

            GameObject subText = FindOrCreateChild(textContent, "SubText");
            TextMeshProUGUI subTmp = GetOrAddComponent<TextMeshProUGUI>(subText);
            subTmp.text = "Accede a recompensas exclusivas";
            subTmp.fontSize = 12;
            subTmp.color = new Color(0.8f, 0.7f, 0.9f, 1f);
            subTmp.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement subLE = GetOrAddComponent<LayoutElement>(subText);
            subLE.minHeight = 18;

            // Price
            GameObject pricePanel = FindOrCreateChild(buttonArea, "PricePanel");
            Image priceBg = GetOrAddComponent<Image>(pricePanel);
            priceBg.color = GOLD;
            LayoutElement priceLE = GetOrAddComponent<LayoutElement>(pricePanel);
            priceLE.minWidth = 100;
            priceLE.minHeight = 45;

            VerticalLayoutGroup priceVlg = GetOrAddComponent<VerticalLayoutGroup>(pricePanel);
            priceVlg.spacing = 0;
            priceVlg.padding = new RectOffset(10, 10, 5, 5);
            priceVlg.childAlignment = TextAnchor.MiddleCenter;
            priceVlg.childControlWidth = true;
            priceVlg.childControlHeight = true;
            priceVlg.childForceExpandHeight = false;

            GameObject priceText = FindOrCreateChild(pricePanel, "Price");
            TextMeshProUGUI priceTmp = GetOrAddComponent<TextMeshProUGUI>(priceText);
            priceTmp.text = "$9.99";
            priceTmp.fontSize = 20;
            priceTmp.fontStyle = FontStyles.Bold;
            priceTmp.color = TEXT_DARK;
            priceTmp.alignment = TextAlignmentOptions.Center;
            LayoutElement priceTextLE = GetOrAddComponent<LayoutElement>(priceText);
            priceTextLE.minHeight = 26;

            Debug.Log("[BattlePassUIBuilder] PremiumPurchaseButton creado");
        }

        // ==================== TIER DETAIL POPUP ====================

        private static void CreateTierDetailPopup(Canvas canvas)
        {
            GameObject blocker = FindOrCreateChild(canvas.gameObject, "TierDetailBlocker");
            blocker.SetActive(false);

            SetRectTransformStretch(blocker);
            Image blockerBg = GetOrAddComponent<Image>(blocker);
            blockerBg.color = BLOCKER_BG;
            Button blockerBtn = GetOrAddComponent<Button>(blocker);
            blockerBtn.transition = Selectable.Transition.None;
            blocker.transform.SetAsLastSibling();

            GameObject popup = FindOrCreateChild(blocker, "TierDetailPopup");
            RectTransform popupRT = GetOrAddComponent<RectTransform>(popup);
            popupRT.anchorMin = new Vector2(0.5f, 0.5f);
            popupRT.anchorMax = new Vector2(0.5f, 0.5f);
            popupRT.sizeDelta = new Vector2(400, 450);

            Image popupBg = GetOrAddComponent<Image>(popup);
            popupBg.color = POPUP_BG;
            AddOutline(popup, PURPLE_PREMIUM, 2);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(popup);
            vlg.spacing = 15;
            vlg.padding = new RectOffset(25, 25, 25, 25);
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

            // Tier Title
            GameObject tierTitle = FindOrCreateChild(popup, "TierTitle");
            TextMeshProUGUI tierTitleText = GetOrAddComponent<TextMeshProUGUI>(tierTitle);
            tierTitleText.text = "NIVEL 12";
            tierTitleText.fontSize = 28;
            tierTitleText.fontStyle = FontStyles.Bold;
            tierTitleText.color = CYAN_NEON;
            tierTitleText.alignment = TextAlignmentOptions.Center;
            LayoutElement tierTitleLE = GetOrAddComponent<LayoutElement>(tierTitle);
            tierTitleLE.minHeight = 40;

            // Free Reward Section
            CreateRewardSection(popup, "FreeRewardSection", "RECOMPENSA GRATIS", false);

            // Premium Reward Section
            CreateRewardSection(popup, "PremiumRewardSection", "RECOMPENSA PREMIUM", true);

            // Claim Button
            GameObject claimBtn = FindOrCreateChild(popup, "ClaimButton");
            Image claimBg = GetOrAddComponent<Image>(claimBtn);
            claimBg.color = BUTTON_SUCCESS;
            Button claimButton = GetOrAddComponent<Button>(claimBtn);
            SetupButtonColors(claimButton, BUTTON_SUCCESS);
            LayoutElement claimLE = GetOrAddComponent<LayoutElement>(claimBtn);
            claimLE.minHeight = 50;

            GameObject claimText = FindOrCreateChild(claimBtn, "Text");
            TextMeshProUGUI claimTmp = GetOrAddComponent<TextMeshProUGUI>(claimText);
            claimTmp.text = "Reclamar";
            claimTmp.fontSize = 20;
            claimTmp.fontStyle = FontStyles.Bold;
            claimTmp.color = TEXT_DARK;
            claimTmp.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(claimText);

            Debug.Log("[BattlePassUIBuilder] TierDetailPopup creado");
        }

        private static void CreateRewardSection(GameObject parent, string name, string title, bool isPremium)
        {
            GameObject section = FindOrCreateChild(parent, name);

            Image sectionBg = GetOrAddComponent<Image>(section);
            sectionBg.color = isPremium ? PREMIUM_TRACK : FREE_TRACK;
            AddOutline(section, isPremium ? PURPLE_PREMIUM * 0.6f : CYAN_DARK);

            LayoutElement sectionLE = GetOrAddComponent<LayoutElement>(section);
            sectionLE.minHeight = 90;

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(section);
            vlg.spacing = 8;
            vlg.padding = new RectOffset(15, 15, 10, 10);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // Section Title
            GameObject sectionTitle = FindOrCreateChild(section, "Title");
            TextMeshProUGUI sectionTitleText = GetOrAddComponent<TextMeshProUGUI>(sectionTitle);
            sectionTitleText.text = title;
            sectionTitleText.fontSize = 12;
            sectionTitleText.fontStyle = FontStyles.Bold;
            sectionTitleText.color = isPremium ? PURPLE_PREMIUM : CYAN_NEON;
            sectionTitleText.alignment = TextAlignmentOptions.Center;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(sectionTitle);
            titleLE.minHeight = 18;

            // Reward Display
            GameObject rewardDisplay = FindOrCreateChild(section, "RewardDisplay");
            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(rewardDisplay);
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            LayoutElement displayLE = GetOrAddComponent<LayoutElement>(rewardDisplay);
            displayLE.minHeight = 45;

            // Icon
            GameObject iconObj = FindOrCreateChild(rewardDisplay, "Icon");
            Image iconImage = GetOrAddComponent<Image>(iconObj);
            iconImage.color = isPremium ? GEM_COLOR : COIN_COLOR;
            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(iconObj);
            iconLE.minWidth = 40;
            iconLE.minHeight = 40;

            // Amount
            GameObject amountObj = FindOrCreateChild(rewardDisplay, "Amount");
            TextMeshProUGUI amountText = GetOrAddComponent<TextMeshProUGUI>(amountObj);
            amountText.text = isPremium ? "150 Gemas" : "500 Monedas";
            amountText.fontSize = 18;
            amountText.fontStyle = FontStyles.Bold;
            amountText.color = TEXT_PRIMARY;
            amountText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement amountLE = GetOrAddComponent<LayoutElement>(amountObj);
            amountLE.minWidth = 150;
        }

        // ==================== PURCHASE CONFIRM POPUP ====================

        private static void CreatePurchaseConfirmPopup(Canvas canvas)
        {
            GameObject blocker = FindOrCreateChild(canvas.gameObject, "PurchaseBlocker");
            blocker.SetActive(false);

            SetRectTransformStretch(blocker);
            Image blockerBg = GetOrAddComponent<Image>(blocker);
            blockerBg.color = BLOCKER_BG;
            Button blockerBtn = GetOrAddComponent<Button>(blocker);
            blockerBtn.transition = Selectable.Transition.None;
            blocker.transform.SetAsLastSibling();

            GameObject popup = FindOrCreateChild(blocker, "PurchasePopup");
            RectTransform popupRT = GetOrAddComponent<RectTransform>(popup);
            popupRT.anchorMin = new Vector2(0.5f, 0.5f);
            popupRT.anchorMax = new Vector2(0.5f, 0.5f);
            popupRT.sizeDelta = new Vector2(420, 350);

            Image popupBg = GetOrAddComponent<Image>(popup);
            popupBg.color = POPUP_BG;
            AddOutline(popup, GOLD, 2);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(popup);
            vlg.spacing = 20;
            vlg.padding = new RectOffset(30, 30, 30, 30);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Icon
            GameObject iconObj = FindOrCreateChild(popup, "Icon");
            Image iconImage = GetOrAddComponent<Image>(iconObj);
            iconImage.color = PURPLE_PREMIUM;
            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(iconObj);
            iconLE.minHeight = 60;
            iconLE.minWidth = 60;
            iconLE.preferredHeight = 60;
            iconLE.preferredWidth = 60;

            // Title
            GameObject titleObj = FindOrCreateChild(popup, "Title");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "Comprar Pase Premium?";
            titleText.fontSize = 24;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = GOLD;
            titleText.alignment = TextAlignmentOptions.Center;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minHeight = 35;

            // Benefits
            GameObject benefitsObj = FindOrCreateChild(popup, "Benefits");
            TextMeshProUGUI benefitsText = GetOrAddComponent<TextMeshProUGUI>(benefitsObj);
            benefitsText.text = "Desbloquea todas las recompensas premium\n+ Recompensas exclusivas\n+ Progreso instantaneo hasta tu nivel";
            benefitsText.fontSize = 14;
            benefitsText.color = TEXT_SECONDARY;
            benefitsText.alignment = TextAlignmentOptions.Center;
            LayoutElement benefitsLE = GetOrAddComponent<LayoutElement>(benefitsObj);
            benefitsLE.minHeight = 60;

            // Buttons
            GameObject buttons = FindOrCreateChild(popup, "Buttons");
            HorizontalLayoutGroup btnHlg = GetOrAddComponent<HorizontalLayoutGroup>(buttons);
            btnHlg.spacing = 20;
            btnHlg.childControlWidth = true;
            btnHlg.childControlHeight = true;
            btnHlg.childForceExpandWidth = true;
            LayoutElement btnLE = GetOrAddComponent<LayoutElement>(buttons);
            btnLE.minHeight = 50;

            // Cancel
            GameObject cancelBtn = FindOrCreateChild(buttons, "CancelButton");
            Image cancelBg = GetOrAddComponent<Image>(cancelBtn);
            cancelBg.color = BUTTON_SECONDARY;
            Button cancelButton = GetOrAddComponent<Button>(cancelBtn);
            SetupButtonColors(cancelButton, BUTTON_SECONDARY);

            GameObject cancelText = FindOrCreateChild(cancelBtn, "Text");
            TextMeshProUGUI cancelTmp = GetOrAddComponent<TextMeshProUGUI>(cancelText);
            cancelTmp.text = "Cancelar";
            cancelTmp.fontSize = 16;
            cancelTmp.fontStyle = FontStyles.Bold;
            cancelTmp.color = TEXT_PRIMARY;
            cancelTmp.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(cancelText);

            // Confirm
            GameObject confirmBtn = FindOrCreateChild(buttons, "ConfirmButton");
            Image confirmBg = GetOrAddComponent<Image>(confirmBtn);
            confirmBg.color = GOLD;
            Button confirmButton = GetOrAddComponent<Button>(confirmBtn);
            SetupButtonColors(confirmButton, GOLD);

            GameObject confirmText = FindOrCreateChild(confirmBtn, "Text");
            TextMeshProUGUI confirmTmp = GetOrAddComponent<TextMeshProUGUI>(confirmText);
            confirmTmp.text = "$9.99";
            confirmTmp.fontSize = 18;
            confirmTmp.fontStyle = FontStyles.Bold;
            confirmTmp.color = TEXT_DARK;
            confirmTmp.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(confirmText);

            Debug.Log("[BattlePassUIBuilder] PurchaseConfirmPopup creado");
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
