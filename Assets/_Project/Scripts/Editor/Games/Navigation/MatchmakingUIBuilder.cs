using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using DigitPark.UI;
using DigitPark.Themes;
using ET = DigitPark.Themes.ThemeApplier.ElementType;

namespace DigitPark.Editor
{
    /// <summary>
    /// Premium Matchmaking UI Builder - Vertical Layout
    /// Professional VS screen with:
    /// - Vertical card layout (Player top, Opponent bottom)
    /// - Horizontal cards: avatar left + info right
    /// - Animated VS badge in center
    /// - Search spinner + status section
    /// - Countdown overlay
    /// - Cancel button for exit
    ///
    /// Menu: DigitPark/UI Builders/Core/Matchmaking (Premium)
    /// </summary>
    public class MatchmakingUIBuilder : EditorWindow
    {
        // ═══════════════════════════════════════════════════════════════
        //  NEON THEME COLORS
        // ═══════════════════════════════════════════════════════════════

        private static readonly Color DARK_NAVY = new Color(0.02f, 0.04f, 0.08f, 1f);       // #050A14
        private static readonly Color CARD_BG = new Color(0.06f, 0.08f, 0.16f, 1f);          // #101428
        private static readonly Color CARD_BG_LIGHT = new Color(0.08f, 0.10f, 0.20f, 1f);    // #141A33

        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);                 // #00FFFF
        private static readonly Color PURPLE_NEON = new Color(0.62f, 0.29f, 1f, 1f);         // #9D4BFF
        private static readonly Color GREEN_NEON = new Color(0.24f, 1f, 0.42f, 1f);          // #3CFF6B
        private static readonly Color ORANGE_NEON = new Color(1f, 0.55f, 0.15f, 1f);         // #FF8C26
        private static readonly Color RED_NEON = new Color(1f, 0.2f, 0.4f, 1f);              // #FF3366
        private static readonly Color GOLD = new Color(1f, 0.79f, 0.28f, 1f);                // #FFC947

        private static readonly Color TEXT_WHITE = Color.white;
        private static readonly Color TEXT_SECONDARY = new Color(0.77f, 0.80f, 1f, 1f);      // #C4CCFF
        private static readonly Color TEXT_MUTED = new Color(0.44f, 0.45f, 0.60f, 1f);       // #707399

        // Asset paths
        // Back button removed — Cancel button handles exit
        private const string ICON_DIGIT_RUSH = "Assets/_Project/Art/Icons/Games/DigitRushIcon.png";
        private const string ICON_MEMORY_PAIRS = "Assets/_Project/Art/Icons/Games/MemoryPairsIcon.png";
        private const string ICON_QUICK_MATH = "Assets/_Project/Art/Icons/Games/QuickMathIcon.png";
        private const string ICON_FLASH_TAP = "Assets/_Project/Art/Icons/Games/FlashTapIcon.png";
        private const string ICON_ODD_ONE_OUT = "Assets/_Project/Art/Icons/Games/OddOneOutIcon.png";
        private const string ICON_COGNITIVE_SPRINT = "Assets/_Project/Art/Icons/Games/CognitiveSprintIcon.png";
        private const string ICON_AVATAR_DEFAULT = "Assets/_Project/Art/Icons/Social/AvatarDefault.png";

        // ═══════════════════════════════════════════════════════════════
        //  MAIN BUILD
        // ═══════════════════════════════════════════════════════════════

        [MenuItem("DigitPark/Scenes/Build Scene/Games/Matchmaking", false, 122)]
        public static void BuildUI()
        {
            // --- Canvas ---
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                GameObject canvasGO = new GameObject("Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
                canvasGO.AddComponent<GraphicRaycaster>();
            }
            else
            {
                // Fix matchWidthOrHeight if incorrect
                var scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.referenceResolution = new Vector2(1080, 1920);
                    scaler.matchWidthOrHeight = 0.5f;
                }
            }

            // Clear existing UI (skip EventSystem)
            var children = new System.Collections.Generic.List<Transform>();
            foreach (Transform child in canvas.transform)
                children.Add(child);
            foreach (Transform child in children)
            {
                if (child.GetComponent<UnityEngine.EventSystems.EventSystem>() != null) continue;
                DestroyImmediate(child.gameObject);
            }

            // SafeArea container
            GameObject safeArea = CreateElement(canvas.transform, "SafeArea");
            SetFullStretch(safeArea.GetComponent<RectTransform>());

            // Build all sections
            CreateBackground(safeArea.transform);
            CreateHeader(safeArea.transform);
            CreateTitleText(safeArea.transform);
            CreateBattleArea(safeArea.transform);
            CreateSearchSection(safeArea.transform);
            CreateCancelButton(safeArea.transform);
            CreateCountdownPanel(safeArea.transform);
            CreateScreenFlash(safeArea.transform);

            // Wire up manager references
            SetupManagerReferences(canvas.transform);

            Debug.Log("[MatchmakingUIBuilder] Premium vertical UI created successfully!");
        }

        // ═══════════════════════════════════════════════════════════════
        //  BACKGROUND
        // ═══════════════════════════════════════════════════════════════

        private static void CreateBackground(Transform parent)
        {
            GameObject bg = CreateElement(parent, "Background");
            SetFullStretch(bg.GetComponent<RectTransform>());
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = Color.white; // ThemeApplier tints at runtime
            bgImg.raycastTarget = false;
            // Matchmaking: only Background gets ThemeApplier — BattleCardApplier controls the rest
            ThemeApplierHelper.Apply(bg, ET.PrimaryBackground);

            // Ambient spotlight (subtle glow behind battle area)
            GameObject spotlight = CreateElement(bg.transform, "Spotlight");
            RectTransform spotRect = spotlight.GetComponent<RectTransform>();
            spotRect.anchorMin = new Vector2(0.05f, 0.30f);
            spotRect.anchorMax = new Vector2(0.95f, 0.85f);
            spotRect.offsetMin = Vector2.zero;
            spotRect.offsetMax = Vector2.zero;
            Image spotImg = spotlight.AddComponent<Image>();
            spotImg.color = new Color(CYAN_NEON.r, CYAN_NEON.g, CYAN_NEON.b, 0.025f);
            spotImg.raycastTarget = false;

            // Ambient particles placeholder
            GameObject particles = CreateElement(bg.transform, "AmbientParticles");
            SetFullStretch(particles.GetComponent<RectTransform>());
        }

        // Back button removed — Cancel button handles matchmaking exit

        // ═══════════════════════════════════════════════════════════════
        //  HEADER (Game Icon + Game Name)
        // ═══════════════════════════════════════════════════════════════

        private static void CreateHeader(Transform parent)
        {
            // Header container: expanded to fit large game icon
            GameObject header = CreateElement(parent, "Header");
            RectTransform headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 0.73f);
            headerRect.anchorMax = new Vector2(1f, 0.97f);
            headerRect.offsetMin = Vector2.zero;
            headerRect.offsetMax = Vector2.zero;

            // --- Game Icon Container (centered, square, 360x360) ---
            GameObject iconContainer = CreateElement(header.transform, "GameIconContainer");
            RectTransform iconContRect = iconContainer.GetComponent<RectTransform>();
            iconContRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconContRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconContRect.pivot = new Vector2(0.5f, 0.5f);
            iconContRect.sizeDelta = new Vector2(240, 240);
            iconContRect.anchoredPosition = new Vector2(0, 30);

            // Icon background
            GameObject iconBg = CreateElement(iconContainer.transform, "IconBackground");
            SetFullStretch(iconBg.GetComponent<RectTransform>());
            Image iconBgImg = iconBg.AddComponent<Image>();
            iconBgImg.color = CARD_BG;

            // Icon border glow
            Outline iconGlow = iconBg.AddComponent<Outline>();
            iconGlow.effectColor = new Color(CYAN_NEON.r, CYAN_NEON.g, CYAN_NEON.b, 0.5f);
            iconGlow.effectDistance = new Vector2(3, -3);

            // Game Icon Image (pre-assign DigitRush as default; Manager swaps at runtime)
            GameObject gameIcon = CreateElement(iconContainer.transform, "GameIcon");
            RectTransform gameIconRect = gameIcon.GetComponent<RectTransform>();
            gameIconRect.anchorMin = new Vector2(0.05f, 0.05f);
            gameIconRect.anchorMax = new Vector2(0.95f, 0.95f);
            gameIconRect.offsetMin = Vector2.zero;
            gameIconRect.offsetMax = Vector2.zero;
            Image gameIconImg = gameIcon.AddComponent<Image>();
            gameIconImg.color = Color.white;
            gameIconImg.preserveAspect = true;

            // Pre-assign default game icon so it doesn't show "?" in Editor
            Sprite defaultIcon = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_DIGIT_RUSH);
            if (defaultIcon != null)
                gameIconImg.sprite = defaultIcon;

            // Placeholder text (shown when no icon — hidden if default icon assigned)
            GameObject placeholder = CreateElement(gameIcon.transform, "Placeholder");
            SetFullStretch(placeholder.GetComponent<RectTransform>());
            TextMeshProUGUI placeholderTmp = placeholder.AddComponent<TextMeshProUGUI>();
            placeholderTmp.text = "?";
            placeholderTmp.fontSize = FontSizes.Symbol;
            placeholderTmp.color = CYAN_NEON;
            placeholderTmp.alignment = TextAlignmentOptions.Center;
            placeholderTmp.fontStyle = FontStyles.Bold;
            placeholderTmp.enableAutoSizing = true;
            placeholderTmp.fontSizeMin = FontSizes.AutoMinTitle;
            placeholderTmp.fontSizeMax = FontSizes.Symbol;
            if (defaultIcon != null)
                placeholder.SetActive(false);

            // --- Game Name Text (below icon) ---
            GameObject gameName = CreateElement(header.transform, "GameNameText");
            RectTransform gameNameRect = gameName.GetComponent<RectTransform>();
            gameNameRect.anchorMin = new Vector2(0.1f, 0f);
            gameNameRect.anchorMax = new Vector2(0.9f, 0.18f);
            gameNameRect.offsetMin = Vector2.zero;
            gameNameRect.offsetMax = Vector2.zero;
            TextMeshProUGUI gameNameTmp = gameName.AddComponent<TextMeshProUGUI>();
            gameNameTmp.text = "DIGIT RUSH";
            gameNameTmp.fontSize = FontSizes.H3;
            gameNameTmp.color = TEXT_SECONDARY;
            gameNameTmp.alignment = TextAlignmentOptions.Center;
            gameNameTmp.fontStyle = FontStyles.Bold;
            gameNameTmp.enableAutoSizing = true;
            gameNameTmp.fontSizeMin = FontSizes.AutoMinTitle;
            gameNameTmp.fontSizeMax = FontSizes.H3;

        }

        // ═══════════════════════════════════════════════════════════════
        //  TITLE TEXT ("SEARCHING...")
        // ═══════════════════════════════════════════════════════════════

        private static void CreateTitleText(Transform parent)
        {
            GameObject title = CreateElement(parent, "TitleText");
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.67f);
            titleRect.anchorMax = new Vector2(0.9f, 0.72f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            TextMeshProUGUI titleTmp = title.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "SEARCHING...";
            titleTmp.fontSize = FontSizes.H1;
            titleTmp.color = CYAN_NEON;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.enableAutoSizing = true;
            titleTmp.fontSizeMin = FontSizes.AutoMinTitle;
            titleTmp.fontSizeMax = FontSizes.H1;

            // Glow
            Outline glow = title.AddComponent<Outline>();
            glow.effectColor = new Color(CYAN_NEON.r, CYAN_NEON.g, CYAN_NEON.b, 0.35f);
            glow.effectDistance = new Vector2(2, -2);
        }

        // ═══════════════════════════════════════════════════════════════
        //  BATTLE AREA (Player Card + VS + Opponent Card)
        // ═══════════════════════════════════════════════════════════════

        private static void CreateBattleArea(Transform parent)
        {
            GameObject battleArea = CreateElement(parent, "BattleArea");
            RectTransform battleRect = battleArea.GetComponent<RectTransform>();
            battleRect.anchorMin = new Vector2(0f, 0.25f);
            battleRect.anchorMax = new Vector2(1f, 0.67f);
            battleRect.offsetMin = Vector2.zero;
            battleRect.offsetMax = Vector2.zero;

            // Player Card (top half of battle area)
            CreatePlayerCard(battleArea.transform, true);

            // VS Badge (center)
            CreateVSBadge(battleArea.transform);

            // Opponent Card (bottom half of battle area)
            CreatePlayerCard(battleArea.transform, false);
        }

        /// <summary>
        /// Creates a horizontal player card:
        /// ┌──────────────────────────────────────────┐
        /// │  ┌────────┐                              │
        /// │  │ Avatar │  PlayerName     [YOU badge]  │
        /// │  │  (lg)  │  Lv. 12                      │
        /// │  └────────┘                              │
        /// └──────────────────────────────────────────┘
        /// </summary>
        private static void CreatePlayerCard(Transform parent, bool isPlayer)
        {
            string cardName = isPlayer ? "PlayerCard" : "OpponentCard";
            Color accentColor = isPlayer ? CYAN_NEON : new Color(0.6f, 0.65f, 0.7f, 1f);

            // Card position: player on top, opponent on bottom — gap 0.43–0.57 reserved for VS text
            float yMin = isPlayer ? 0.57f : 0.03f;
            float yMax = isPlayer ? 0.97f : 0.43f;

            // --- Card Container ---
            GameObject card = CreateElement(parent, cardName);
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.05f, yMin);
            cardRect.anchorMax = new Vector2(0.95f, yMax);
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;

            // --- Card Background ---
            GameObject cardBg = CreateElement(card.transform, "CardBackground");
            SetFullStretch(cardBg.GetComponent<RectTransform>());
            Image cardBgImg = cardBg.AddComponent<Image>();
            cardBgImg.color = CARD_BG;

            // Card border
            Outline cardBorder = cardBg.AddComponent<Outline>();
            cardBorder.effectColor = new Color(accentColor.r, accentColor.g, accentColor.b, 0.7f);
            cardBorder.effectDistance = new Vector2(2, -2);

            // --- Avatar Section (left 35%) ---
            GameObject avatarSection = CreateElement(card.transform, "AvatarSection");
            RectTransform avatarSectionRect = avatarSection.GetComponent<RectTransform>();
            avatarSectionRect.anchorMin = new Vector2(0.03f, 0.08f);
            avatarSectionRect.anchorMax = new Vector2(0.38f, 0.92f);
            avatarSectionRect.offsetMin = Vector2.zero;
            avatarSectionRect.offsetMax = Vector2.zero;

            // Force square aspect: avatar container centered in section
            GameObject avatarContainer = CreateElement(avatarSection.transform, "AvatarContainer");
            RectTransform avatarContRect = avatarContainer.GetComponent<RectTransform>();
            avatarContRect.anchorMin = new Vector2(0.05f, 0.05f);
            avatarContRect.anchorMax = new Vector2(0.95f, 0.95f);
            avatarContRect.offsetMin = Vector2.zero;
            avatarContRect.offsetMax = Vector2.zero;

            // AspectRatioFitter to guarantee square
            var aspectFitter = avatarContainer.AddComponent<AspectRatioFitter>();
            aspectFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            aspectFitter.aspectRatio = 1f;

            // Generate circle sprite for circular avatar elements
            Sprite circleSprite = GenerateCircleSprite();

            // Circular glow ring (outer, slightly larger)
            GameObject avatarGlow = CreateElement(avatarContainer.transform, "AvatarGlow");
            RectTransform glowRect = avatarGlow.GetComponent<RectTransform>();
            SetFullStretch(glowRect);
            glowRect.offsetMin = new Vector2(-6, -6);
            glowRect.offsetMax = new Vector2(6, 6);
            Image glowImg = avatarGlow.AddComponent<Image>();
            glowImg.sprite = circleSprite;
            glowImg.color = new Color(accentColor.r, accentColor.g, accentColor.b, isPlayer ? 0.25f : 0.1f);

            // Circular border ring
            GameObject avatarFrame = CreateElement(avatarContainer.transform, "AvatarFrame");
            SetFullStretch(avatarFrame.GetComponent<RectTransform>());
            Image frameImg = avatarFrame.AddComponent<Image>();
            frameImg.sprite = circleSprite;
            frameImg.color = accentColor;
            var fr_matchmaking = avatarFrame.AddComponent<DigitPark.Services.FrameRenderer>();
            fr_matchmaking.SetRenderMode(DigitPark.Services.FrameRenderer.RenderMode.Full);

            // Circular mask container (clips avatar to circle)
            GameObject maskContainer = CreateElement(avatarContainer.transform, "AvatarMask");
            RectTransform maskRect = maskContainer.GetComponent<RectTransform>();
            maskRect.anchorMin = new Vector2(0.06f, 0.06f);
            maskRect.anchorMax = new Vector2(0.94f, 0.94f);
            maskRect.offsetMin = Vector2.zero;
            maskRect.offsetMax = Vector2.zero;
            Image maskImg = maskContainer.AddComponent<Image>();
            maskImg.sprite = circleSprite;
            maskImg.color = CARD_BG_LIGHT;
            maskContainer.AddComponent<Mask>().showMaskGraphic = true;

            // Avatar Image (inside mask — clipped to circle)
            string avatarName = isPlayer ? "PlayerAvatar" : "OpponentAvatar";
            GameObject avatar = CreateElement(maskContainer.transform, avatarName);
            RectTransform avatarRect = avatar.GetComponent<RectTransform>();
            avatarRect.anchorMin = Vector2.zero;
            avatarRect.anchorMax = Vector2.one;
            avatarRect.offsetMin = Vector2.zero;
            avatarRect.offsetMax = Vector2.zero;
            Image avatarImg = avatar.AddComponent<Image>();
            avatarImg.color = Color.white;
            avatarImg.preserveAspect = true;

            // Set default avatar sprite on Image
            Sprite defaultAvatar = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_AVATAR_DEFAULT);
            if (defaultAvatar != null)
            {
                avatarImg.sprite = defaultAvatar;
            }

            // Add AvatarUI component with default sprite
            var avatarUI = avatar.AddComponent<DigitPark.UI.Components.AvatarUI>();
            SerializedObject avatarSO = new SerializedObject(avatarUI);
            avatarSO.FindProperty("loadCurrentUserOnStart").boolValue = isPlayer;
            avatarSO.FindProperty("isEditable").boolValue = false;
            avatarSO.FindProperty("avatarImage").objectReferenceValue = avatarImg;
            if (defaultAvatar != null)
            {
                avatarSO.FindProperty("defaultAvatarSprite").objectReferenceValue = defaultAvatar;
            }
            avatarSO.ApplyModifiedProperties();

            // --- Info Section (right 60%) ---
            GameObject infoSection = CreateElement(card.transform, isPlayer ? "PlayerInfo" : "OpponentInfo");
            RectTransform infoRect = infoSection.GetComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0.40f, 0.1f);
            infoRect.anchorMax = new Vector2(0.97f, 0.9f);
            infoRect.offsetMin = Vector2.zero;
            infoRect.offsetMax = Vector2.zero;

            // Player/Opponent Name
            string nameObjName = isPlayer ? "PlayerName" : "OpponentName";
            GameObject nameObj = CreateElement(infoSection.transform, nameObjName);
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 0.50f);
            nameRect.anchorMax = new Vector2(0.85f, 0.90f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            TextMeshProUGUI nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
            nameTmp.text = isPlayer ? "Player" : "???";
            nameTmp.fontSize = FontSizes.H1;
            nameTmp.color = TEXT_WHITE;
            nameTmp.alignment = TextAlignmentOptions.Left;
            nameTmp.fontStyle = FontStyles.Bold;
            nameTmp.enableAutoSizing = true;
            nameTmp.fontSizeMin = FontSizes.AutoMinTitle;
            nameTmp.fontSizeMax = nameTmp.fontSize;


            // Level/Rank
            string levelObjName = isPlayer ? "PlayerLevel" : "OpponentLevel";
            GameObject levelObj = CreateElement(infoSection.transform, levelObjName);
            RectTransform levelRect = levelObj.GetComponent<RectTransform>();
            levelRect.anchorMin = new Vector2(0f, 0.10f);
            levelRect.anchorMax = new Vector2(0.5f, 0.45f);
            levelRect.offsetMin = Vector2.zero;
            levelRect.offsetMax = Vector2.zero;

            // Level background pill
            Image levelBg = levelObj.AddComponent<Image>();
            levelBg.color = new Color(accentColor.r * 0.15f, accentColor.g * 0.15f, accentColor.b * 0.15f, 0.8f);

            // Level border
            Outline levelBorder = levelObj.AddComponent<Outline>();
            levelBorder.effectColor = new Color(accentColor.r, accentColor.g, accentColor.b, 0.4f);
            levelBorder.effectDistance = new Vector2(1, -1);

            // Level text
            GameObject levelText = CreateElement(levelObj.transform, "LevelText");
            SetFullStretch(levelText.GetComponent<RectTransform>());
            TextMeshProUGUI levelTmp = levelText.AddComponent<TextMeshProUGUI>();
            levelTmp.text = isPlayer ? "Lv. 1" : "---";
            levelTmp.fontSize = FontSizes.Subtitle;
            levelTmp.color = isPlayer ? CYAN_NEON : TEXT_MUTED;
            levelTmp.alignment = TextAlignmentOptions.Center;
            levelTmp.fontStyle = FontStyles.Bold;
            levelTmp.enableAutoSizing = true;
            levelTmp.fontSizeMin = FontSizes.AutoMinBody;
            levelTmp.fontSizeMax = levelTmp.fontSize;


            // "YOU" badge for player card
            if (isPlayer)
            {
                GameObject youBadge = CreateElement(infoSection.transform, "YouBadge");
                RectTransform youRect = youBadge.GetComponent<RectTransform>();
                youRect.anchorMin = new Vector2(0.70f, 0.55f);
                youRect.anchorMax = new Vector2(0.98f, 0.85f);
                youRect.offsetMin = Vector2.zero;
                youRect.offsetMax = Vector2.zero;

                Image youBg = youBadge.AddComponent<Image>();
                youBg.color = new Color(CYAN_NEON.r * 0.2f, CYAN_NEON.g * 0.2f, CYAN_NEON.b * 0.2f, 0.8f);

                Outline youBorder = youBadge.AddComponent<Outline>();
                youBorder.effectColor = CYAN_NEON;
                youBorder.effectDistance = new Vector2(1, -1);

                GameObject youText = CreateElement(youBadge.transform, "YouText");
                SetFullStretch(youText.GetComponent<RectTransform>());
                TextMeshProUGUI youTmp = youText.AddComponent<TextMeshProUGUI>();
                youTmp.text = "YOU";
                youTmp.fontSize = FontSizes.Body;
                youTmp.color = CYAN_NEON;
                youTmp.alignment = TextAlignmentOptions.Center;
                youTmp.fontStyle = FontStyles.Bold;
                youTmp.enableAutoSizing = true;
                youTmp.fontSizeMin = FontSizes.AutoMinBody;
                youTmp.fontSizeMax = youTmp.fontSize;

            }

            // --- BattleCardApplier --- wire visual refs for runtime card theming
            var bcApplier = card.AddComponent<DigitPark.Cosmetics.BattleCardApplier>();
            SerializedObject bcSO = new SerializedObject(bcApplier);
            bcSO.FindProperty("cardBackground").objectReferenceValue = cardBgImg;
            bcSO.FindProperty("outlineBorder").objectReferenceValue = cardBorder;
            bcSO.FindProperty("avatarGlow").objectReferenceValue = glowImg;
            bcSO.FindProperty("avatarFrame").objectReferenceValue = frameImg;
            bcSO.FindProperty("levelPillBg").objectReferenceValue = levelBg;
            bcSO.FindProperty("playerNameText").objectReferenceValue = nameTmp;
            bcSO.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// Creates the VS text label between player cards.
        /// Hidden by default; MatchmakingAnimator reveals it when opponent is found.
        /// </summary>
        private static void CreateVSBadge(Transform parent)
        {
            GameObject vsContainer = CreateElement(parent, "VSContainer");
            RectTransform vsRect = vsContainer.GetComponent<RectTransform>();
            vsRect.anchorMin = new Vector2(0.5f, 0.5f);
            vsRect.anchorMax = new Vector2(0.5f, 0.5f);
            vsRect.pivot = new Vector2(0.5f, 0.5f);
            vsRect.sizeDelta = new Vector2(160, 60);
            vsRect.anchoredPosition = Vector2.zero;

            // VS Text — tintable, no icon
            GameObject vsText = CreateElement(vsContainer.transform, "VSText");
            SetFullStretch(vsText.GetComponent<RectTransform>());
            TextMeshProUGUI vsTmp = vsText.AddComponent<TextMeshProUGUI>();
            vsTmp.text = "VS";
            vsTmp.fontSize = 64;
            vsTmp.fontStyle = FontStyles.Bold;
            vsTmp.color = Color.white;
            vsTmp.alignment = TextAlignmentOptions.Center;
            vsTmp.raycastTarget = false;
            vsTmp.enableAutoSizing = false;

            vsContainer.SetActive(true);
        }

        // ═══════════════════════════════════════════════════════════════
        //  SEARCH SECTION (Spinner + Status + Timer)
        // ═══════════════════════════════════════════════════════════════

        private static void CreateSearchSection(Transform parent)
        {
            GameObject searchSection = CreateElement(parent, "SearchSection");
            RectTransform searchRect = searchSection.GetComponent<RectTransform>();
            searchRect.anchorMin = new Vector2(0.1f, 0.12f);
            searchRect.anchorMax = new Vector2(0.9f, 0.25f);
            searchRect.offsetMin = Vector2.zero;
            searchRect.offsetMax = Vector2.zero;

            // --- Status Text (centered, upper half) ---
            GameObject statusText = CreateElement(searchSection.transform, "StatusText");
            RectTransform statusRect = statusText.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.05f, 0.45f);
            statusRect.anchorMax = new Vector2(0.95f, 0.95f);
            statusRect.offsetMin = Vector2.zero;
            statusRect.offsetMax = Vector2.zero;
            TextMeshProUGUI statusTmp = statusText.AddComponent<TextMeshProUGUI>();
            statusTmp.text = "Searching for opponent...";
            statusTmp.fontSize = FontSizes.Subtitle;
            statusTmp.fontStyle = FontStyles.Bold;
            statusTmp.color = TEXT_SECONDARY;
            statusTmp.alignment = TextAlignmentOptions.Center;
            statusTmp.enableAutoSizing = true;
            statusTmp.fontSizeMin = FontSizes.AutoMinBody;
            statusTmp.fontSizeMax = statusTmp.fontSize;

            // --- Timer Text (centered, lower half) ---
            GameObject timerText = CreateElement(searchSection.transform, "TimerText");
            RectTransform timerRect = timerText.GetComponent<RectTransform>();
            timerRect.anchorMin = new Vector2(0.25f, 0f);
            timerRect.anchorMax = new Vector2(0.75f, 0.45f);
            timerRect.offsetMin = Vector2.zero;
            timerRect.offsetMax = Vector2.zero;
            TextMeshProUGUI timerTmp = timerText.AddComponent<TextMeshProUGUI>();
            timerTmp.text = "0:00";
            timerTmp.fontSize = FontSizes.H3;
            timerTmp.color = CYAN_NEON;
            timerTmp.alignment = TextAlignmentOptions.Center;
            timerTmp.fontStyle = FontStyles.Bold;
            timerTmp.enableAutoSizing = true;
            timerTmp.fontSizeMin = FontSizes.AutoMinTitle;
            timerTmp.fontSizeMax = timerTmp.fontSize;
        }

        // ═══════════════════════════════════════════════════════════════
        //  CANCEL BUTTON
        // ═══════════════════════════════════════════════════════════════

        private static void CreateCancelButton(Transform parent)
        {
            GameObject container = CreateElement(parent, "CancelButtonContainer");
            RectTransform containerRect = container.GetComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.15f, 0.04f);
            containerRect.anchorMax = new Vector2(0.85f, 0.11f);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;

            // Button
            GameObject cancelBtn = CreateElement(container.transform, "CancelButton");
            SetFullStretch(cancelBtn.GetComponent<RectTransform>());

            Image btnBg = cancelBtn.AddComponent<Image>();
            btnBg.color = new Color(RED_NEON.r * 0.15f, RED_NEON.g * 0.08f, RED_NEON.b * 0.08f, 0.9f);

            // Red border
            Outline btnBorder = cancelBtn.AddComponent<Outline>();
            btnBorder.effectColor = RED_NEON;
            btnBorder.effectDistance = new Vector2(2, -2);

            Button btn = cancelBtn.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1, 0.85f, 0.85f);
            colors.pressedColor = new Color(0.85f, 0.65f, 0.65f);
            btn.colors = colors;

            // Button text
            GameObject btnText = CreateElement(cancelBtn.transform, "Text");
            SetFullStretch(btnText.GetComponent<RectTransform>());
            TextMeshProUGUI textTmp = btnText.AddComponent<TextMeshProUGUI>();
            textTmp.text = "CANCEL";
            textTmp.fontSize = FontSizes.H3;
            textTmp.color = RED_NEON;
            textTmp.alignment = TextAlignmentOptions.Center;
            textTmp.fontStyle = FontStyles.Bold;
            textTmp.enableAutoSizing = true;
            textTmp.fontSizeMin = FontSizes.AutoMinTitle;
            textTmp.fontSizeMax = textTmp.fontSize;

        }

        // ═══════════════════════════════════════════════════════════════
        //  COUNTDOWN PANEL (Fullscreen overlay)
        // ═══════════════════════════════════════════════════════════════

        private static void CreateCountdownPanel(Transform parent)
        {
            GameObject panel = CreateElement(parent, "CountdownPanel");
            SetFullStretch(panel.GetComponent<RectTransform>());

            // Dark overlay
            Image overlay = panel.AddComponent<Image>();
            overlay.color = new Color(0, 0, 0, 0.88f);

            // "GET READY!" text (top)
            GameObject readyText = CreateElement(panel.transform, "GetReadyText");
            RectTransform readyRect = readyText.GetComponent<RectTransform>();
            readyRect.anchorMin = new Vector2(0.15f, 0.62f);
            readyRect.anchorMax = new Vector2(0.85f, 0.72f);
            readyRect.offsetMin = Vector2.zero;
            readyRect.offsetMax = Vector2.zero;
            TextMeshProUGUI readyTmp = readyText.AddComponent<TextMeshProUGUI>();
            readyTmp.text = "GET READY!";
            readyTmp.fontSize = FontSizes.Equation;
            readyTmp.color = TEXT_WHITE;
            readyTmp.alignment = TextAlignmentOptions.Center;
            readyTmp.fontStyle = FontStyles.Bold;
            readyTmp.enableAutoSizing = true;
            readyTmp.fontSizeMin = FontSizes.AutoMinTitle;
            readyTmp.fontSizeMax = readyTmp.fontSize;


            // Countdown number (center, big)
            GameObject countdownText = CreateElement(panel.transform, "CountdownText");
            RectTransform countdownRect = countdownText.GetComponent<RectTransform>();
            countdownRect.anchorMin = new Vector2(0.2f, 0.35f);
            countdownRect.anchorMax = new Vector2(0.8f, 0.62f);
            countdownRect.offsetMin = Vector2.zero;
            countdownRect.offsetMax = Vector2.zero;
            TextMeshProUGUI countdownTmp = countdownText.AddComponent<TextMeshProUGUI>();
            countdownTmp.text = "3";
            countdownTmp.fontSize = FontSizes.H4;
            countdownTmp.color = GREEN_NEON;
            countdownTmp.alignment = TextAlignmentOptions.Center;
            countdownTmp.fontStyle = FontStyles.Bold;
            countdownTmp.enableAutoSizing = true;
            countdownTmp.fontSizeMin = FontSizes.AutoMinTitle;
            countdownTmp.fontSizeMax = countdownTmp.fontSize;


            // Countdown glow
            Outline countdownGlow = countdownText.AddComponent<Outline>();
            countdownGlow.effectColor = new Color(GREEN_NEON.r, GREEN_NEON.g, GREEN_NEON.b, 0.5f);
            countdownGlow.effectDistance = new Vector2(5, -5);

            // Start hidden
            panel.SetActive(false);
        }

        // ═══════════════════════════════════════════════════════════════
        //  SCREEN FLASH (Effect overlay)
        // ═══════════════════════════════════════════════════════════════

        private static void CreateScreenFlash(Transform parent)
        {
            GameObject flash = CreateElement(parent, "ScreenFlash");
            SetFullStretch(flash.GetComponent<RectTransform>());
            Image flashImg = flash.AddComponent<Image>();
            flashImg.color = new Color(1, 1, 1, 0);
            flashImg.raycastTarget = false;
            flash.SetActive(false);
        }

        // ═══════════════════════════════════════════════════════════════
        //  MANAGER REFERENCES SETUP
        // ═══════════════════════════════════════════════════════════════

        private static void SetupManagerReferences(Transform canvasTransform)
        {
            var manager = FindObjectOfType<DigitPark.Managers.MatchmakingManager>();
            if (manager == null)
            {
                Debug.LogWarning("[MatchmakingUIBuilder] MatchmakingManager not found. Please add it to the scene.");
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            Transform sa = canvasTransform.Find("SafeArea");

            // --- Header ---
            SetProperty(so, "titleText", sa, "TitleText");
            SetProperty(so, "gameIconImage", sa, "Header/GameIconContainer/GameIcon");
            SetProperty(so, "gameTypeText", sa, "Header/GameNameText");

            // --- Player Card ---
            SetProperty(so, "playerCard", sa, "BattleArea/PlayerCard");
            SetProperty(so, "playerAvatar", sa, "BattleArea/PlayerCard/AvatarSection/AvatarContainer/AvatarMask/PlayerAvatar");
            SetProperty(so, "playerNameText", sa, "BattleArea/PlayerCard/PlayerInfo/PlayerName");
            SetProperty(so, "playerLevelText", sa, "BattleArea/PlayerCard/PlayerInfo/PlayerLevel/LevelText");

            // --- Opponent Card ---
            SetProperty(so, "opponentCard", sa, "BattleArea/OpponentCard");
            SetProperty(so, "opponentAvatar", sa, "BattleArea/OpponentCard/AvatarSection/AvatarContainer/AvatarMask/OpponentAvatar");
            SetProperty(so, "opponentNameText", sa, "BattleArea/OpponentCard/OpponentInfo/OpponentName");
            SetProperty(so, "opponentLevelText", sa, "BattleArea/OpponentCard/OpponentInfo/OpponentLevel/LevelText");
            // SearchingIndicator removed — SearchSection handles search feedback

            // --- VS Section ---
            SetProperty(so, "vsContainer", sa, "BattleArea/VSContainer");
            SetProperty(so, "vsText", sa, "BattleArea/VSContainer/VSText");

            // --- Search Status ---
            SetProperty(so, "statusText", sa, "SearchSection/StatusText");
            SetProperty(so, "timerText", sa, "SearchSection/TimerText");

            // --- Countdown ---
            SetProperty(so, "countdownPanel", sa, "CountdownPanel");
            SetProperty(so, "countdownText", sa, "CountdownPanel/CountdownText");
            SetProperty(so, "getReadyText", sa, "CountdownPanel/GetReadyText");

            // --- Buttons ---
            SetProperty(so, "cancelButton", sa, "CancelButtonContainer/CancelButton");

            // --- Effects ---
            SetProperty(so, "screenFlash", sa, "ScreenFlash");

            // --- Game Icons ---
            SetupGameIcons(so);

            so.ApplyModifiedProperties();
            Debug.Log("[MatchmakingUIBuilder] All manager references configured!");
        }

        private static void SetupGameIcons(SerializedObject so)
        {
            SetIconProperty(so, "digitRushIcon", ICON_DIGIT_RUSH);
            SetIconProperty(so, "memoryPairsIcon", ICON_MEMORY_PAIRS);
            SetIconProperty(so, "quickMathIcon", ICON_QUICK_MATH);
            SetIconProperty(so, "flashTapIcon", ICON_FLASH_TAP);
            SetIconProperty(so, "oddOneOutIcon", ICON_ODD_ONE_OUT);
            SetIconProperty(so, "cognitiveSprintIcon", ICON_COGNITIVE_SPRINT);
        }

        private static void SetIconProperty(SerializedObject so, string propName, string assetPath)
        {
            SerializedProperty prop = so.FindProperty(propName);
            if (prop == null) return;

            Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (icon != null)
                prop.objectReferenceValue = icon;
            else
                Debug.LogWarning($"[MatchmakingUIBuilder] Icon not found: {assetPath}");
        }

        // ═══════════════════════════════════════════════════════════════
        //  HELPERS
        // ═══════════════════════════════════════════════════════════════

        private static void SetProperty(SerializedObject so, string propName, Transform root, string path)
        {
            SerializedProperty prop = so.FindProperty(propName);
            if (prop == null) return;

            Transform target = root.Find(path);
            if (target == null)
            {
                Debug.LogWarning($"[MatchmakingUIBuilder] Path not found: {path}");
                return;
            }

            System.Type fieldType = GetFieldType(so.targetObject, propName);

            if (fieldType == typeof(Button))
                prop.objectReferenceValue = target.GetComponent<Button>();
            else if (fieldType == typeof(Image))
                prop.objectReferenceValue = target.GetComponent<Image>();
            else if (fieldType == typeof(TextMeshProUGUI))
                prop.objectReferenceValue = target.GetComponent<TextMeshProUGUI>();
            else if (fieldType == typeof(GameObject))
                prop.objectReferenceValue = target.gameObject;
            else
                prop.objectReferenceValue = target.gameObject;
        }

        private static System.Type GetFieldType(Object target, string fieldName)
        {
            System.Type type = target.GetType();
            var field = type.GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            return field?.FieldType ?? typeof(GameObject);
        }

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

        private static GameObject CreateElement(Transform parent, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private static void SetFullStretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// Generates a white filled circle sprite (128x128) for circular UI masks and frames.
        /// </summary>
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
    }
}
