using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using DigitPark.UI;

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
    /// - BackButton integration
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
        private const string BACK_BUTTON_PREFAB = "Assets/_Project/Prefabs/Common/BackButton.prefab";
        private const string ICON_DIGIT_RUSH = "Assets/_Project/Art/Icons/Games/DigitRushIcon.png";
        private const string ICON_MEMORY_PAIRS = "Assets/_Project/Art/Icons/Games/MemoryPairsIcon.png";
        private const string ICON_QUICK_MATH = "Assets/_Project/Art/Icons/Games/QuickMathIcon.png";
        private const string ICON_FLASH_TAP = "Assets/_Project/Art/Icons/Games/FlashTapIcon.png";
        private const string ICON_ODD_ONE_OUT = "Assets/_Project/Art/Icons/Games/OddOneOutIcon.png";
        private const string ICON_COGNITIVE_SPRINT = "Assets/_Project/Art/Icons/Games/CognitiveSprintIcon.png";
        private const string ICON_VS = "Assets/_Project/Art/Icons/Games/VSIcon.png";
        private const string ICON_AVATAR_DEFAULT = "Assets/_Project/Art/Icons/Social/Profile/AvatarDefaultNeon.png";

        // ═══════════════════════════════════════════════════════════════
        //  MAIN BUILD
        // ═══════════════════════════════════════════════════════════════

        [MenuItem("DigitPark/UI Builders/Games/Matchmaking (Premium)", false, 122)]
        public static void BuildUI()
        {
            CleanupOldUI();

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
                scaler.matchWidthOrHeight = 0f;
                canvasGO.AddComponent<GraphicRaycaster>();
            }
            else
            {
                // Fix matchWidthOrHeight if incorrect
                var scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.referenceResolution = new Vector2(1080, 1920);
                    scaler.matchWidthOrHeight = 0f;
                }
            }

            // Clear existing UI
            foreach (Transform child in canvas.transform)
                DestroyImmediate(child.gameObject);

            // SafeArea container
            GameObject safeArea = CreateElement(canvas.transform, "SafeArea");
            SetFullStretch(safeArea.GetComponent<RectTransform>());

            // Build all sections
            CreateBackground(safeArea.transform);
            CreateBackButton(safeArea.transform);
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
            bgImg.color = DARK_NAVY;
            bgImg.raycastTarget = false;

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

        // ═══════════════════════════════════════════════════════════════
        //  BACK BUTTON
        // ═══════════════════════════════════════════════════════════════

        private static void CreateBackButton(Transform parent)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BACK_BUTTON_PREFAB);

            if (prefab != null)
            {
                GameObject backBtn = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                backBtn.name = "BackButton";
                RectTransform rt = backBtn.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                rt.anchoredPosition = new Vector2(30, -40);
                rt.sizeDelta = new Vector2(50, 50);
            }
            else
            {
                // Fallback: simple back button
                GameObject backBtn = CreateElement(parent, "BackButton");
                RectTransform rt = backBtn.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                rt.anchoredPosition = new Vector2(30, -40);
                rt.sizeDelta = new Vector2(50, 50);

                Image btnBg = backBtn.AddComponent<Image>();
                btnBg.color = new Color(CYAN_NEON.r, CYAN_NEON.g, CYAN_NEON.b, 0.2f);
                backBtn.AddComponent<Button>();

                GameObject arrow = CreateElement(backBtn.transform, "Arrow");
                SetFullStretch(arrow.GetComponent<RectTransform>());
                TextMeshProUGUI arrowText = arrow.AddComponent<TextMeshProUGUI>();
                arrowText.text = "<";
                arrowText.fontSize = FontSizes.AuthTitle;
                arrowText.color = CYAN_NEON;
                arrowText.alignment = TextAlignmentOptions.Center;
                arrowText.fontStyle = FontStyles.Bold;
            }
        }

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

            // Game Icon Image
            GameObject gameIcon = CreateElement(iconContainer.transform, "GameIcon");
            RectTransform gameIconRect = gameIcon.GetComponent<RectTransform>();
            gameIconRect.anchorMin = new Vector2(0.05f, 0.05f);
            gameIconRect.anchorMax = new Vector2(0.95f, 0.95f);
            gameIconRect.offsetMin = Vector2.zero;
            gameIconRect.offsetMax = Vector2.zero;
            Image gameIconImg = gameIcon.AddComponent<Image>();
            gameIconImg.color = Color.white;
            gameIconImg.preserveAspect = true;

            // Placeholder text (shown when no icon)
            GameObject placeholder = CreateElement(gameIcon.transform, "Placeholder");
            SetFullStretch(placeholder.GetComponent<RectTransform>());
            TextMeshProUGUI placeholderTmp = placeholder.AddComponent<TextMeshProUGUI>();
            placeholderTmp.text = "?";
            placeholderTmp.fontSize = FontSizes.CardSymbol;
            placeholderTmp.color = CYAN_NEON;
            placeholderTmp.alignment = TextAlignmentOptions.Center;
            placeholderTmp.fontStyle = FontStyles.Bold;

            // --- Game Name Text (below icon) ---
            GameObject gameName = CreateElement(header.transform, "GameNameText");
            RectTransform gameNameRect = gameName.GetComponent<RectTransform>();
            gameNameRect.anchorMin = new Vector2(0.1f, 0f);
            gameNameRect.anchorMax = new Vector2(0.9f, 0.18f);
            gameNameRect.offsetMin = Vector2.zero;
            gameNameRect.offsetMax = Vector2.zero;
            TextMeshProUGUI gameNameTmp = gameName.AddComponent<TextMeshProUGUI>();
            gameNameTmp.text = "DIGIT RUSH";
            gameNameTmp.fontSize = FontSizes.CardTitle;
            gameNameTmp.color = TEXT_SECONDARY;
            gameNameTmp.alignment = TextAlignmentOptions.Center;
            gameNameTmp.fontStyle = FontStyles.Bold;

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
            titleTmp.fontSize = FontSizes.DisplayLarge;
            titleTmp.color = CYAN_NEON;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.fontStyle = FontStyles.Bold;


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
            battleRect.anchorMin = new Vector2(0f, 0.24f);
            battleRect.anchorMax = new Vector2(1f, 0.66f);
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

            // Card position: player on top, opponent on bottom
            float yMin = isPlayer ? 0.55f : 0.05f;
            float yMax = isPlayer ? 0.95f : 0.45f;

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

            // Avatar outer glow ring
            GameObject avatarGlow = CreateElement(avatarContainer.transform, "AvatarGlow");
            RectTransform glowRect = avatarGlow.GetComponent<RectTransform>();
            SetFullStretch(glowRect);
            glowRect.offsetMin = new Vector2(-6, -6);
            glowRect.offsetMax = new Vector2(6, 6);
            Image glowImg = avatarGlow.AddComponent<Image>();
            glowImg.color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.3f);

            // Avatar frame (border)
            GameObject avatarFrame = CreateElement(avatarContainer.transform, "AvatarFrame");
            SetFullStretch(avatarFrame.GetComponent<RectTransform>());
            Image frameImg = avatarFrame.AddComponent<Image>();
            frameImg.color = accentColor;

            // Avatar background (inside frame)
            GameObject avatarBg = CreateElement(avatarContainer.transform, "AvatarBackground");
            RectTransform avatarBgRect = avatarBg.GetComponent<RectTransform>();
            avatarBgRect.anchorMin = new Vector2(0.06f, 0.06f);
            avatarBgRect.anchorMax = new Vector2(0.94f, 0.94f);
            avatarBgRect.offsetMin = Vector2.zero;
            avatarBgRect.offsetMax = Vector2.zero;
            Image avatarBgImg = avatarBg.AddComponent<Image>();
            avatarBgImg.color = CARD_BG_LIGHT;

            // Avatar Image
            string avatarName = isPlayer ? "PlayerAvatar" : "OpponentAvatar";
            GameObject avatar = CreateElement(avatarContainer.transform, avatarName);
            RectTransform avatarRect = avatar.GetComponent<RectTransform>();
            avatarRect.anchorMin = new Vector2(0.1f, 0.1f);
            avatarRect.anchorMax = new Vector2(0.9f, 0.9f);
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

            // Opponent searching indicator (ring animation)
            if (!isPlayer)
            {
                GameObject searchIndicator = CreateElement(avatarContainer.transform, "SearchingIndicator");
                RectTransform searchRect = searchIndicator.GetComponent<RectTransform>();
                SetFullStretch(searchRect);
                searchRect.offsetMin = new Vector2(-10, -10);
                searchRect.offsetMax = new Vector2(10, 10);

                // Rotating search ring
                GameObject ring = CreateElement(searchIndicator.transform, "SearchRing");
                SetFullStretch(ring.GetComponent<RectTransform>());
                Image ringImg = ring.AddComponent<Image>();
                ringImg.color = new Color(0.6f, 0.65f, 0.7f, 0.6f);
                ringImg.fillAmount = 0.7f;
                ringImg.type = Image.Type.Filled;
                ringImg.fillMethod = Image.FillMethod.Radial360;
            }

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
            nameTmp.fontSize = FontSizes.DisplayLarge;
            nameTmp.color = TEXT_WHITE;
            nameTmp.alignment = TextAlignmentOptions.Left;
            nameTmp.fontStyle = FontStyles.Bold;


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
            levelTmp.fontSize = FontSizes.ValueLarge;
            levelTmp.color = isPlayer ? CYAN_NEON : TEXT_MUTED;
            levelTmp.alignment = TextAlignmentOptions.Center;
            levelTmp.fontStyle = FontStyles.Bold;


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
                youTmp.fontSize = FontSizes.BodyLarge;
                youTmp.color = CYAN_NEON;
                youTmp.alignment = TextAlignmentOptions.Center;
                youTmp.fontStyle = FontStyles.Bold;

            }
        }

        /// <summary>
        /// Creates the VS badge between player cards using custom icon
        /// </summary>
        private static void CreateVSBadge(Transform parent)
        {
            GameObject vsContainer = CreateElement(parent, "VSContainer");
            RectTransform vsRect = vsContainer.GetComponent<RectTransform>();
            vsRect.anchorMin = new Vector2(0.5f, 0.5f);
            vsRect.anchorMax = new Vector2(0.5f, 0.5f);
            vsRect.pivot = new Vector2(0.5f, 0.5f);
            vsRect.sizeDelta = new Vector2(200, 200);
            vsRect.anchoredPosition = Vector2.zero;

            // VS Icon Image (custom neon icon)
            GameObject vsIconObj = CreateElement(vsContainer.transform, "VSIcon");
            SetFullStretch(vsIconObj.GetComponent<RectTransform>());
            Image vsIconImg = vsIconObj.AddComponent<Image>();
            vsIconImg.preserveAspect = true;
            vsIconImg.raycastTarget = false;

            // Load VS icon sprite
            Sprite vsSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_VS);
            if (vsSprite != null)
            {
                vsIconImg.sprite = vsSprite;
                vsIconImg.color = Color.white;
            }
            else
            {
                // Fallback: orange tint placeholder
                vsIconImg.color = new Color(ORANGE_NEON.r, ORANGE_NEON.g, ORANGE_NEON.b, 0.3f);
                Debug.LogWarning("[MatchmakingUIBuilder] VSIcon.png not found at: " + ICON_VS);
            }

            // VSText (hidden - kept for manager animation reference)
            GameObject vsText = CreateElement(vsContainer.transform, "VSText");
            SetFullStretch(vsText.GetComponent<RectTransform>());
            TextMeshProUGUI vsTmp = vsText.AddComponent<TextMeshProUGUI>();
            vsTmp.text = "VS";
            vsTmp.fontSize = FontSizes.ValueMedium;
            vsTmp.color = new Color(1, 1, 1, 0); // Invisible
            vsTmp.alignment = TextAlignmentOptions.Center;
            vsTmp.raycastTarget = false;


            // Initially hidden until match found
            vsContainer.SetActive(false);
        }

        // ═══════════════════════════════════════════════════════════════
        //  SEARCH SECTION (Spinner + Status + Timer)
        // ═══════════════════════════════════════════════════════════════

        private static void CreateSearchSection(Transform parent)
        {
            GameObject searchSection = CreateElement(parent, "SearchSection");
            RectTransform searchRect = searchSection.GetComponent<RectTransform>();
            searchRect.anchorMin = new Vector2(0.1f, 0.14f);
            searchRect.anchorMax = new Vector2(0.9f, 0.30f);
            searchRect.offsetMin = Vector2.zero;
            searchRect.offsetMax = Vector2.zero;

            // --- Search Spinner (centered, top part) ---
            GameObject spinner = CreateElement(searchSection.transform, "SearchSpinner");
            RectTransform spinnerRect = spinner.GetComponent<RectTransform>();
            spinnerRect.anchorMin = new Vector2(0.5f, 0.55f);
            spinnerRect.anchorMax = new Vector2(0.5f, 0.55f);
            spinnerRect.pivot = new Vector2(0.5f, 0.5f);
            spinnerRect.sizeDelta = new Vector2(80, 80);
            spinnerRect.anchoredPosition = new Vector2(0, 20);

            // Outer ring (glow)
            GameObject outerRing = CreateElement(spinner.transform, "OuterRing");
            SetFullStretch(outerRing.GetComponent<RectTransform>());
            Image outerImg = outerRing.AddComponent<Image>();
            outerImg.color = new Color(CYAN_NEON.r, CYAN_NEON.g, CYAN_NEON.b, 0.2f);

            // Inner ring (rotating)
            GameObject innerRing = CreateElement(spinner.transform, "InnerRing");
            RectTransform innerRect = innerRing.GetComponent<RectTransform>();
            innerRect.anchorMin = new Vector2(0.08f, 0.08f);
            innerRect.anchorMax = new Vector2(0.92f, 0.92f);
            innerRect.offsetMin = Vector2.zero;
            innerRect.offsetMax = Vector2.zero;
            Image innerImg = innerRing.AddComponent<Image>();
            innerImg.color = CYAN_NEON;
            innerImg.fillAmount = 0.25f;
            innerImg.type = Image.Type.Filled;
            innerImg.fillMethod = Image.FillMethod.Radial360;

            // Center dot
            GameObject centerDot = CreateElement(spinner.transform, "CenterDot");
            RectTransform dotRect = centerDot.GetComponent<RectTransform>();
            dotRect.anchorMin = new Vector2(0.35f, 0.35f);
            dotRect.anchorMax = new Vector2(0.65f, 0.65f);
            dotRect.offsetMin = Vector2.zero;
            dotRect.offsetMax = Vector2.zero;
            Image dotImg = centerDot.AddComponent<Image>();
            dotImg.color = CYAN_NEON;

            // --- Status Text ---
            GameObject statusText = CreateElement(searchSection.transform, "StatusText");
            RectTransform statusRect = statusText.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.05f, 0.18f);
            statusRect.anchorMax = new Vector2(0.95f, 0.48f);
            statusRect.offsetMin = Vector2.zero;
            statusRect.offsetMax = Vector2.zero;
            TextMeshProUGUI statusTmp = statusText.AddComponent<TextMeshProUGUI>();
            statusTmp.text = "Searching for opponent...";
            statusTmp.fontSize = FontSizes.SectionHeader;
            statusTmp.color = TEXT_SECONDARY;
            statusTmp.alignment = TextAlignmentOptions.Center;


            // --- Timer Text ---
            GameObject timerText = CreateElement(searchSection.transform, "TimerText");
            RectTransform timerRect = timerText.GetComponent<RectTransform>();
            timerRect.anchorMin = new Vector2(0.3f, 0f);
            timerRect.anchorMax = new Vector2(0.7f, 0.22f);
            timerRect.offsetMin = Vector2.zero;
            timerRect.offsetMax = Vector2.zero;
            TextMeshProUGUI timerTmp = timerText.AddComponent<TextMeshProUGUI>();
            timerTmp.text = "0:00";
            timerTmp.fontSize = FontSizes.DisplayMedium;
            timerTmp.color = CYAN_NEON;
            timerTmp.alignment = TextAlignmentOptions.Center;
            timerTmp.fontStyle = FontStyles.Bold;

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
            textTmp.fontSize = FontSizes.DisplayMedium;
            textTmp.color = RED_NEON;
            textTmp.alignment = TextAlignmentOptions.Center;
            textTmp.fontStyle = FontStyles.Bold;

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


            // Countdown number (center, big)
            GameObject countdownText = CreateElement(panel.transform, "CountdownText");
            RectTransform countdownRect = countdownText.GetComponent<RectTransform>();
            countdownRect.anchorMin = new Vector2(0.2f, 0.35f);
            countdownRect.anchorMax = new Vector2(0.8f, 0.62f);
            countdownRect.offsetMin = Vector2.zero;
            countdownRect.offsetMax = Vector2.zero;
            TextMeshProUGUI countdownTmp = countdownText.AddComponent<TextMeshProUGUI>();
            countdownTmp.text = "3";
            countdownTmp.fontSize = FontSizes.SceneTitle;
            countdownTmp.color = GREEN_NEON;
            countdownTmp.alignment = TextAlignmentOptions.Center;
            countdownTmp.fontStyle = FontStyles.Bold;


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
            SetProperty(so, "playerAvatar", sa, "BattleArea/PlayerCard/AvatarSection/AvatarContainer/PlayerAvatar");
            SetProperty(so, "playerNameText", sa, "BattleArea/PlayerCard/PlayerInfo/PlayerName");
            SetProperty(so, "playerLevelText", sa, "BattleArea/PlayerCard/PlayerInfo/PlayerLevel/LevelText");

            // --- Opponent Card ---
            SetProperty(so, "opponentCard", sa, "BattleArea/OpponentCard");
            SetProperty(so, "opponentAvatar", sa, "BattleArea/OpponentCard/AvatarSection/AvatarContainer/OpponentAvatar");
            SetProperty(so, "opponentNameText", sa, "BattleArea/OpponentCard/OpponentInfo/OpponentName");
            SetProperty(so, "opponentLevelText", sa, "BattleArea/OpponentCard/OpponentInfo/OpponentLevel/LevelText");
            SetProperty(so, "opponentSearchingIndicator", sa, "BattleArea/OpponentCard/AvatarSection/AvatarContainer/SearchingIndicator");
            SetProperty(so, "opponentSearchRing", sa, "BattleArea/OpponentCard/AvatarSection/AvatarContainer/SearchingIndicator/SearchRing");

            // --- VS Section ---
            SetProperty(so, "vsContainer", sa, "BattleArea/VSContainer");
            SetProperty(so, "vsText", sa, "BattleArea/VSContainer/VSText");

            // --- Search Status ---
            SetProperty(so, "searchingSpinner", sa, "SearchSection/SearchSpinner");
            SetProperty(so, "searchingRing", sa, "SearchSection/SearchSpinner/InnerRing");
            SetProperty(so, "statusText", sa, "SearchSection/StatusText");
            SetProperty(so, "timerText", sa, "SearchSection/TimerText");

            // --- Countdown ---
            SetProperty(so, "countdownPanel", sa, "CountdownPanel");
            SetProperty(so, "countdownText", sa, "CountdownPanel/CountdownText");
            SetProperty(so, "getReadyText", sa, "CountdownPanel/GetReadyText");

            // --- Buttons ---
            SetProperty(so, "cancelButton", sa, "CancelButtonContainer/CancelButton");
            SetProperty(so, "backButton", sa, "BackButton");

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
    }
}
