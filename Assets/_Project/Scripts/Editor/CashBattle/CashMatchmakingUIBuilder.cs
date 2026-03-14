using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using DigitPark.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// Cash Matchmaking UI Builder - Gold Premium Theme
    /// VS screen for Cash Battle matchmaking with:
    /// - Gold premium color palette
    /// - Vertical card layout (Player top, Opponent bottom)
    /// - Horizontal cards: avatar left + info right
    /// - Animated VS badge in center
    /// - Entry fee display
    /// - Search spinner + status section
    /// - Countdown overlay
    /// - Cancel button for exit
    ///
    /// Menu: DigitPark/UI Builders/CashBattle/CashMatchmaking (VS Screen)
    /// </summary>
    public class CashMatchmakingUIBuilder : EditorWindow
    {
        // ═══════════════════════════════════════════════════════════════
        //  GOLD PREMIUM THEME COLORS
        // ═══════════════════════════════════════════════════════════════

        private static readonly Color GOLD_PRIMARY = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GOLD_DARK = new Color(0.85f, 0.65f, 0.13f, 1f);
        private static readonly Color GOLD_LIGHT = new Color(1f, 0.93f, 0.55f, 1f);
        private static readonly Color AMBER = new Color(1f, 0.75f, 0f, 1f);

        private static readonly Color BG_DARK = new Color(0.06f, 0.05f, 0.10f, 1f);
        private static readonly Color CARD_BG = new Color(0.12f, 0.1f, 0.15f, 0.95f);
        private static readonly Color CARD_BORDER = new Color(0.85f, 0.65f, 0.13f, 0.6f);

        private static readonly Color TEXT_PRIMARY = Color.white;
        private static readonly Color TEXT_GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.7f, 0.7f, 0.7f, 1f);

        private static readonly Color BUTTON_GOLD = new Color(0.85f, 0.65f, 0.13f, 1f);
        private static readonly Color CYAN_ACCENT = new Color(0f, 0.9f, 1f, 1f);
        private static readonly Color GREEN_GO = new Color(0.24f, 1f, 0.42f, 1f);
        private static readonly Color RED_CANCEL = new Color(1f, 0.2f, 0.4f, 1f);

        // Asset paths
        // Back button removed — Cancel button handles exit
        private const string ICON_AVATAR_DEFAULT = "Assets/_Project/Art/Icons/Social/AvatarDefault.png";
        private const string ICON_DIGIT_RUSH = "Assets/_Project/Art/Icons/Games/DigitRushIcon.png";
        private const string ICON_MEMORY_PAIRS = "Assets/_Project/Art/Icons/Games/MemoryPairsIcon.png";
        private const string ICON_QUICK_MATH = "Assets/_Project/Art/Icons/Games/QuickMathIcon.png";
        private const string ICON_FLASH_TAP = "Assets/_Project/Art/Icons/Games/FlashTapIcon.png";
        private const string ICON_ODD_ONE_OUT = "Assets/_Project/Art/Icons/Games/OddOneOutIcon.png";
        private const string ICON_COGNITIVE_SPRINT = "Assets/_Project/Art/Icons/Games/CognitiveSprintIcon.png";

        // ═══════════════════════════════════════════════════════════════
        //  MAIN BUILD
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Builds the UI silently without confirmation dialogs. Used by batch builders.
        /// </summary>
        public static void BuildSilent() => BuildUI();

        [MenuItem("DigitPark/Scenes/Build Scene/CashBattle/Matchmaking", false, 182)]
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

            // Clear existing UI
            foreach (Transform child in canvas.transform)
                DestroyImmediate(child.gameObject);

            // Background (behind SafeArea)
            CreateBackground(canvas.transform);

            // SafeArea container
            GameObject safeArea = CreateElement(canvas.transform, "SafeArea");
            SetFullStretch(safeArea.GetComponent<RectTransform>());

            // Build all sections
            CreateHeader(safeArea.transform);
            CreateEntryFeeDisplay(safeArea.transform);
            CreateTitleText(safeArea.transform);
            CreateBattleArea(safeArea.transform);
            CreateSearchSection(safeArea.transform);
            CreateCancelButton(safeArea.transform);
            CreateCountdownPanel(safeArea.transform);
            CreateScreenFlash(safeArea.transform);

            // Wire up manager references
            SetupManagerReferences(canvas.transform);

            Debug.Log("[CashMatchmakingUIBuilder] Gold premium VS screen created successfully!");
        }

        // ═══════════════════════════════════════════════════════════════
        //  BACKGROUND
        // ═══════════════════════════════════════════════════════════════

        private static void CreateBackground(Transform parent)
        {
            GameObject bg = CreateElement(parent, "Background");
            SetFullStretch(bg.GetComponent<RectTransform>());
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = BG_DARK;
            bgImg.raycastTarget = false;
        }

        // Back button removed — Cancel button handles matchmaking exit

        // ═══════════════════════════════════════════════════════════════
        //  HEADER (Game Icon + Game Name)
        // ═══════════════════════════════════════════════════════════════

        private static void CreateHeader(Transform parent)
        {
            // Header container
            GameObject header = CreateElement(parent, "Header");
            RectTransform headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 0.73f);
            headerRect.anchorMax = new Vector2(1f, 0.97f);
            headerRect.offsetMin = Vector2.zero;
            headerRect.offsetMax = Vector2.zero;

            // --- Game Icon Container (centered, 240x240) ---
            GameObject iconContainer = CreateElement(header.transform, "GameIconContainer");
            RectTransform iconContRect = iconContainer.GetComponent<RectTransform>();
            iconContRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconContRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconContRect.pivot = new Vector2(0.5f, 0.5f);
            iconContRect.sizeDelta = new Vector2(240, 240);
            iconContRect.anchoredPosition = new Vector2(0, 30);

            // Gold glow ring around icon
            Outline iconGlow = iconContainer.AddComponent<Outline>();
            iconGlow.effectColor = new Color(GOLD_PRIMARY.r, GOLD_PRIMARY.g, GOLD_PRIMARY.b, 0.5f);
            iconGlow.effectDistance = new Vector2(3, -3);

            // Icon background — Classic Gold #FFD700
            GameObject iconBg = CreateElement(iconContainer.transform, "IconBackground");
            SetFullStretch(iconBg.GetComponent<RectTransform>());
            Image iconBgImg = iconBg.AddComponent<Image>();
            iconBgImg.color = new Color(1f, 0.84f, 0f, 1f); // Classic Gold #FFD700

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
            placeholderTmp.color = GOLD_PRIMARY;
            placeholderTmp.alignment = TextAlignmentOptions.Center;
            placeholderTmp.fontStyle = FontStyles.Bold;
            placeholderTmp.enableAutoSizing = true;
            placeholderTmp.fontSizeMin = FontSizes.AutoMinBody;
            placeholderTmp.fontSizeMax = FontSizes.Symbol;
            placeholderTmp.overflowMode = TextOverflowModes.Ellipsis;
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
            gameNameTmp.color = TEXT_GOLD;
            gameNameTmp.alignment = TextAlignmentOptions.Center;
            gameNameTmp.fontStyle = FontStyles.Bold;
            gameNameTmp.enableAutoSizing = true;
            gameNameTmp.fontSizeMin = FontSizes.AutoMinBody;
            gameNameTmp.fontSizeMax = FontSizes.H3;
            gameNameTmp.overflowMode = TextOverflowModes.Ellipsis;
        }

        // ═══════════════════════════════════════════════════════════════
        //  ENTRY FEE DISPLAY
        // ═══════════════════════════════════════════════════════════════

        private static void CreateEntryFeeDisplay(Transform parent)
        {
            GameObject entryFeeDisplay = CreateElement(parent, "EntryFeeDisplay");
            RectTransform entryRect = entryFeeDisplay.GetComponent<RectTransform>();
            entryRect.anchorMin = new Vector2(0.15f, 0.67f);
            entryRect.anchorMax = new Vector2(0.85f, 0.72f);
            entryRect.offsetMin = Vector2.zero;
            entryRect.offsetMax = Vector2.zero;

            // Entry fee text
            GameObject entryFeeText = CreateElement(entryFeeDisplay.transform, "EntryFeeText");
            SetFullStretch(entryFeeText.GetComponent<RectTransform>());
            TextMeshProUGUI entryTmp = entryFeeText.AddComponent<TextMeshProUGUI>();
            entryTmp.text = "Entry: $0.00";
            entryTmp.fontSize = FontSizes.H4;
            entryTmp.color = TEXT_GOLD;
            entryTmp.alignment = TextAlignmentOptions.Center;
            entryTmp.fontStyle = FontStyles.Bold;
            entryTmp.enableAutoSizing = true;
            entryTmp.fontSizeMin = FontSizes.AutoMinBody;
            entryTmp.fontSizeMax = FontSizes.H4;
            entryTmp.overflowMode = TextOverflowModes.Ellipsis;
        }

        // ═══════════════════════════════════════════════════════════════
        //  TITLE TEXT ("SEARCHING...")
        // ═══════════════════════════════════════════════════════════════

        private static void CreateTitleText(Transform parent)
        {
            GameObject title = CreateElement(parent, "TitleText");
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.62f);
            titleRect.anchorMax = new Vector2(0.9f, 0.67f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            TextMeshProUGUI titleTmp = title.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "SEARCHING...";
            titleTmp.fontSize = FontSizes.H1;
            titleTmp.color = TEXT_GOLD;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.enableAutoSizing = true;
            titleTmp.fontSizeMin = FontSizes.AutoMinBody;
            titleTmp.fontSizeMax = FontSizes.H1;
            titleTmp.overflowMode = TextOverflowModes.Ellipsis;

            // Gold glow outline
            Outline glow = title.AddComponent<Outline>();
            glow.effectColor = new Color(GOLD_PRIMARY.r, GOLD_PRIMARY.g, GOLD_PRIMARY.b, 0.3f);
            glow.effectDistance = new Vector2(2, -2);
        }

        // ═══════════════════════════════════════════════════════════════
        //  BATTLE AREA (Player Card + VS + Opponent Card)
        // ═══════════════════════════════════════════════════════════════

        private static void CreateBattleArea(Transform parent)
        {
            GameObject battleArea = CreateElement(parent, "BattleArea");
            RectTransform battleRect = battleArea.GetComponent<RectTransform>();
            battleRect.anchorMin = new Vector2(0f, 0.24f);
            battleRect.anchorMax = new Vector2(1f, 0.62f);
            battleRect.offsetMin = Vector2.zero;
            battleRect.offsetMax = Vector2.zero;

            // Player Card (top half of battle area)
            CreatePlayerCard(battleArea.transform, true);

            // VS Container (center)
            CreateVSContainer(battleArea.transform);

            // Opponent Card (bottom half of battle area)
            CreatePlayerCard(battleArea.transform, false);
        }

        /// <summary>
        /// Creates a horizontal player/opponent card with Gold theme:
        /// - Avatar section on left (with glow ring)
        /// - Info section on right (name + level pill)
        /// - "YOU" badge for player card (AMBER color)
        /// </summary>
        private static void CreatePlayerCard(Transform parent, bool isPlayer)
        {
            string cardName = isPlayer ? "PlayerCard" : "OpponentCard";
            Color accentColor = isPlayer ? GOLD_PRIMARY : new Color(0.6f, 0.65f, 0.7f, 1f);

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

            // Card border (GOLD_DARK * 0.6 alpha)
            Outline cardBorder = cardBg.AddComponent<Outline>();
            cardBorder.effectColor = new Color(GOLD_DARK.r, GOLD_DARK.g, GOLD_DARK.b, 0.6f);
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
            GameObject avatarGlow = CreateElement(avatarContainer.transform, "GlowRing");
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

            // Circular mask container (clips avatar to circle)
            GameObject maskContainer = CreateElement(avatarContainer.transform, "AvatarMask");
            RectTransform maskRect = maskContainer.GetComponent<RectTransform>();
            maskRect.anchorMin = new Vector2(0.06f, 0.06f);
            maskRect.anchorMax = new Vector2(0.94f, 0.94f);
            maskRect.offsetMin = Vector2.zero;
            maskRect.offsetMax = Vector2.zero;
            Image maskImg = maskContainer.AddComponent<Image>();
            maskImg.sprite = circleSprite;
            maskImg.color = new Color(CARD_BG.r, CARD_BG.g, CARD_BG.b, 1f);
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

            // Set default avatar sprite
            Sprite defaultAvatar = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_AVATAR_DEFAULT);
            if (defaultAvatar != null)
            {
                avatarImg.sprite = defaultAvatar;
            }

            // Add AvatarUI component
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
            nameTmp.color = TEXT_PRIMARY;
            nameTmp.alignment = TextAlignmentOptions.Left;
            nameTmp.fontStyle = FontStyles.Bold;
            nameTmp.enableAutoSizing = true;
            nameTmp.fontSizeMin = FontSizes.AutoMinBody;
            nameTmp.fontSizeMax = FontSizes.H1;
            nameTmp.overflowMode = TextOverflowModes.Ellipsis;

            // Level/Rank pill
            string levelObjName = isPlayer ? "PlayerLevel" : "OpponentLevel";
            GameObject levelObj = CreateElement(infoSection.transform, levelObjName);
            RectTransform levelRect = levelObj.GetComponent<RectTransform>();
            levelRect.anchorMin = new Vector2(0f, 0.10f);
            levelRect.anchorMax = new Vector2(0.5f, 0.45f);
            levelRect.offsetMin = Vector2.zero;
            levelRect.offsetMax = Vector2.zero;

            // Level background pill (GOLD_DARK bg)
            Image levelBg = levelObj.AddComponent<Image>();
            levelBg.color = new Color(GOLD_DARK.r * 0.2f, GOLD_DARK.g * 0.2f, GOLD_DARK.b * 0.2f, 0.8f);

            // Level border
            Outline levelBorder = levelObj.AddComponent<Outline>();
            levelBorder.effectColor = new Color(GOLD_DARK.r, GOLD_DARK.g, GOLD_DARK.b, 0.4f);
            levelBorder.effectDistance = new Vector2(1, -1);

            // Level text
            GameObject levelText = CreateElement(levelObj.transform, "LevelText");
            SetFullStretch(levelText.GetComponent<RectTransform>());
            TextMeshProUGUI levelTmp = levelText.AddComponent<TextMeshProUGUI>();
            levelTmp.text = isPlayer ? "Lv. 1" : "---";
            levelTmp.fontSize = FontSizes.Subtitle;
            levelTmp.color = isPlayer ? GOLD_PRIMARY : TEXT_SECONDARY;
            levelTmp.alignment = TextAlignmentOptions.Center;
            levelTmp.fontStyle = FontStyles.Bold;
            levelTmp.enableAutoSizing = true;
            levelTmp.fontSizeMin = FontSizes.AutoMinBody;
            levelTmp.fontSizeMax = FontSizes.Subtitle;
            levelTmp.overflowMode = TextOverflowModes.Ellipsis;

            // "YOU" badge for player card (in infoSection, top-right)
            if (isPlayer)
            {
                GameObject youBadge = CreateElement(infoSection.transform, "YouBadge");
                RectTransform youRect = youBadge.GetComponent<RectTransform>();
                youRect.anchorMin = new Vector2(0.70f, 0.55f);
                youRect.anchorMax = new Vector2(0.98f, 0.85f);
                youRect.offsetMin = Vector2.zero;
                youRect.offsetMax = Vector2.zero;

                Image youBg = youBadge.AddComponent<Image>();
                youBg.color = AMBER;

                GameObject youText = CreateElement(youBadge.transform, "YouText");
                SetFullStretch(youText.GetComponent<RectTransform>());
                TextMeshProUGUI youTmp = youText.AddComponent<TextMeshProUGUI>();
                youTmp.text = "YOU";
                youTmp.fontSize = FontSizes.Body;
                youTmp.color = BG_DARK;
                youTmp.alignment = TextAlignmentOptions.Center;
                youTmp.fontStyle = FontStyles.Bold;
                youTmp.enableAutoSizing = true;
                youTmp.fontSizeMin = FontSizes.AutoMinBody;
                youTmp.fontSizeMax = FontSizes.Body;
                youTmp.overflowMode = TextOverflowModes.Ellipsis;
            }
        }

        /// <summary>
        /// Creates the VS text label between player cards.
        /// Hidden by default; CashMatchmakingAnimator reveals it when opponent is found.
        /// </summary>
        private static void CreateVSContainer(Transform parent)
        {
            GameObject vsContainer = CreateElement(parent, "VSContainer");
            RectTransform vsRect = vsContainer.GetComponent<RectTransform>();
            vsRect.anchorMin = new Vector2(0.5f, 0.5f);
            vsRect.anchorMax = new Vector2(0.5f, 0.5f);
            vsRect.pivot = new Vector2(0.5f, 0.5f);
            vsRect.sizeDelta = new Vector2(160, 60);
            vsRect.anchoredPosition = Vector2.zero;

            // VS Text — Gold, no icon
            GameObject vsText = CreateElement(vsContainer.transform, "VSText");
            SetFullStretch(vsText.GetComponent<RectTransform>());
            TextMeshProUGUI vsTmp = vsText.AddComponent<TextMeshProUGUI>();
            vsTmp.text = "VS";
            vsTmp.fontSize = 64;
            vsTmp.enableAutoSizing = true;
            vsTmp.fontSizeMin = FontSizes.AutoMinTitle;
            vsTmp.fontSizeMax = vsTmp.fontSize;
            vsTmp.fontStyle = FontStyles.Bold;
            vsTmp.color = GOLD_PRIMARY;
            vsTmp.alignment = TextAlignmentOptions.Center;
            vsTmp.raycastTarget = false;

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
            statusTmp.color = TEXT_SECONDARY;
            statusTmp.alignment = TextAlignmentOptions.Center;
            statusTmp.fontStyle = FontStyles.Bold;
            statusTmp.enableAutoSizing = true;
            statusTmp.fontSizeMin = FontSizes.AutoMinBody;
            statusTmp.fontSizeMax = FontSizes.Subtitle;
            statusTmp.overflowMode = TextOverflowModes.Ellipsis;

            // --- Timer Text (centered, lower half) ---
            GameObject timerText = CreateElement(searchSection.transform, "CashTimerText");
            RectTransform timerRect = timerText.GetComponent<RectTransform>();
            timerRect.anchorMin = new Vector2(0.25f, 0f);
            timerRect.anchorMax = new Vector2(0.75f, 0.45f);
            timerRect.offsetMin = Vector2.zero;
            timerRect.offsetMax = Vector2.zero;
            TextMeshProUGUI timerTmp = timerText.AddComponent<TextMeshProUGUI>();
            timerTmp.text = "0:00";
            timerTmp.fontSize = FontSizes.H3;
            timerTmp.color = TEXT_GOLD;
            timerTmp.alignment = TextAlignmentOptions.Center;
            timerTmp.fontStyle = FontStyles.Bold;
            timerTmp.enableAutoSizing = true;
            timerTmp.fontSizeMin = FontSizes.AutoMinBody;
            timerTmp.fontSizeMax = FontSizes.H3;
            timerTmp.overflowMode = TextOverflowModes.Ellipsis;
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
            btnBg.color = new Color(RED_CANCEL.r * 0.15f, RED_CANCEL.g * 0.08f, RED_CANCEL.b * 0.08f, 0.9f);

            // Red border
            Outline btnBorder = cancelBtn.AddComponent<Outline>();
            btnBorder.effectColor = RED_CANCEL;
            btnBorder.effectDistance = new Vector2(2, -2);

            Button btn = cancelBtn.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1, 0.85f, 0.85f);
            colors.pressedColor = new Color(0.85f, 0.65f, 0.65f);
            btn.colors = colors;

            // Button text
            GameObject btnText = CreateElement(cancelBtn.transform, "CashCancelButtonText");
            SetFullStretch(btnText.GetComponent<RectTransform>());
            TextMeshProUGUI textTmp = btnText.AddComponent<TextMeshProUGUI>();
            textTmp.text = "CANCEL";
            textTmp.fontSize = FontSizes.H3;
            textTmp.color = RED_CANCEL;
            textTmp.alignment = TextAlignmentOptions.Center;
            textTmp.fontStyle = FontStyles.Bold;
            textTmp.enableAutoSizing = true;
            textTmp.fontSizeMin = FontSizes.AutoMinBody;
            textTmp.fontSizeMax = FontSizes.H3;
            textTmp.overflowMode = TextOverflowModes.Ellipsis;
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

            // "GET READY!" text (TEXT_GOLD, 36pt)
            GameObject readyText = CreateElement(panel.transform, "GetReadyText");
            RectTransform readyRect = readyText.GetComponent<RectTransform>();
            readyRect.anchorMin = new Vector2(0.15f, 0.62f);
            readyRect.anchorMax = new Vector2(0.85f, 0.72f);
            readyRect.offsetMin = Vector2.zero;
            readyRect.offsetMax = Vector2.zero;
            TextMeshProUGUI readyTmp = readyText.AddComponent<TextMeshProUGUI>();
            readyTmp.text = "GET READY!";
            readyTmp.fontSize = FontSizes.Equation;
            readyTmp.color = TEXT_GOLD;
            readyTmp.alignment = TextAlignmentOptions.Center;
            readyTmp.fontStyle = FontStyles.Bold;
            readyTmp.enableAutoSizing = true;
            readyTmp.fontSizeMin = FontSizes.AutoMinBody;
            readyTmp.fontSizeMax = FontSizes.Equation;
            readyTmp.overflowMode = TextOverflowModes.Ellipsis;

            // Countdown number (GOLD_PRIMARY, 72pt, with outline glow)
            GameObject countdownText = CreateElement(panel.transform, "CountdownText");
            RectTransform countdownRect = countdownText.GetComponent<RectTransform>();
            countdownRect.anchorMin = new Vector2(0.2f, 0.35f);
            countdownRect.anchorMax = new Vector2(0.8f, 0.62f);
            countdownRect.offsetMin = Vector2.zero;
            countdownRect.offsetMax = Vector2.zero;
            TextMeshProUGUI countdownTmp = countdownText.AddComponent<TextMeshProUGUI>();
            countdownTmp.text = "3";
            countdownTmp.fontSize = FontSizes.H4;
            countdownTmp.color = GOLD_PRIMARY;
            countdownTmp.alignment = TextAlignmentOptions.Center;
            countdownTmp.fontStyle = FontStyles.Bold;
            countdownTmp.enableAutoSizing = true;
            countdownTmp.fontSizeMin = FontSizes.AutoMinBody;
            countdownTmp.fontSizeMax = FontSizes.H4;
            countdownTmp.overflowMode = TextOverflowModes.Ellipsis;

            // Countdown glow
            Outline countdownGlow = countdownText.AddComponent<Outline>();
            countdownGlow.effectColor = new Color(GOLD_PRIMARY.r, GOLD_PRIMARY.g, GOLD_PRIMARY.b, 0.5f);
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
            var manager = FindObjectOfType<DigitPark.Managers.CashMatchmakingManager>();
            if (manager == null)
            {
                Debug.LogWarning("[CashMatchmakingUIBuilder] CashMatchmakingManager not found. Please add it to the scene.");
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            Transform sa = canvasTransform.Find("SafeArea");

            // --- Header ---
            SetProperty(so, "titleText", sa, "TitleText");
            SetProperty(so, "gameIconImage", sa, "Header/GameIconContainer/GameIcon");
            SetProperty(so, "gameTypeText", sa, "Header/GameNameText");

            // --- Entry Fee ---
            SetProperty(so, "entryFeeText", sa, "EntryFeeDisplay/EntryFeeText");

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
            // --- VS Section ---
            SetProperty(so, "vsContainer", sa, "BattleArea/VSContainer");
            SetProperty(so, "vsText", sa, "BattleArea/VSContainer/VSText");

            // --- Search Status ---
            SetProperty(so, "statusText", sa, "SearchSection/StatusText");
            SetProperty(so, "timerText", sa, "SearchSection/CashTimerText");

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
            Debug.Log("[CashMatchmakingUIBuilder] All manager references configured!");
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
                Debug.LogWarning($"[CashMatchmakingUIBuilder] Icon not found: {assetPath}");
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
                Debug.LogWarning($"[CashMatchmakingUIBuilder] Path not found: {path}");
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
                // Don't touch TransitionCanvas or EffectsCanvas
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
