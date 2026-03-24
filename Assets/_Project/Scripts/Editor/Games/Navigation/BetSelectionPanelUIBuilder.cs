using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using DigitPark.UI;
using DigitPark.Monetization;

namespace DigitPark.Editor
{
    /// <summary>
    /// UIBuilder for the BetSelection scene.
    /// Creates: ScrollRect with 12 bet cards + custom input + action buttons.
    /// Auto-runs BetSelectionReferenceAssigner after building.
    ///
    /// Menu: DigitPark/UI Builders/Games/BetSelection Scene
    /// </summary>
    public class BetSelectionUIBuilder : EditorWindow
    {
        // ==================== COLORS ====================
        private static readonly Color BG_DARK = new Color(0.02f, 0.04f, 0.08f);
        private static readonly Color CARD_BG = new Color(0.06f, 0.08f, 0.14f);
        private static readonly Color HEADER_BG = new Color(0.04f, 0.06f, 0.12f, 0.9f);
        private static readonly Color CURRENCY_BG = new Color(0.03f, 0.05f, 0.1f, 0.95f);

        // Neon accents
        private static readonly Color NEON_CYAN = new Color(0f, 1f, 1f);
        private static readonly Color CYAN_GLOW = new Color(0f, 1f, 1f, 0.3f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.4f);
        private static readonly Color NEON_GREEN = new Color(0.2f, 1f, 0.4f);
        private static readonly Color GREEN_GLOW = new Color(0.2f, 1f, 0.4f, 0.4f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f);
        private static readonly Color COIN_COLOR = new Color(1f, 0.85f, 0.3f);
        private static readonly Color COIN_GLOW = new Color(1f, 0.85f, 0.3f, 0.4f);
        private static readonly Color GEM_COLOR = new Color(0.4f, 0.7f, 1f);
        private static readonly Color GEM_GLOW = new Color(0.4f, 0.7f, 1f, 0.4f);
        private static readonly Color PURPLE = new Color(0.6f, 0.3f, 1f);
        private static readonly Color PURPLE_GLOW = new Color(0.6f, 0.3f, 1f, 0.4f);
        private static readonly Color CUSTOM_TEAL = new Color(0f, 0.8f, 0.7f);
        private static readonly Color CUSTOM_GLOW = new Color(0f, 0.8f, 0.7f, 0.35f);

        // Buttons
        private static readonly Color BTN_PLAY = new Color(0.15f, 0.75f, 0.35f);
        private static readonly Color BTN_PLAY_GLOW = new Color(0.15f, 0.75f, 0.35f, 0.6f);
        private static readonly Color BTN_CANCEL = new Color(0.55f, 0.12f, 0.12f);

        // Text
        private static readonly Color TEXT_PRIMARY = new Color(0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.65f, 0.75f);
        private static readonly Color TEXT_DIM = new Color(0.4f, 0.45f, 0.55f);

        // Glass, input, toggle
        private static readonly Color GLASS = new Color(1f, 1f, 1f, 0.04f);
        private static readonly Color INPUT_BG = new Color(0.04f, 0.06f, 0.1f);
        private static readonly Color TOGGLE_ON_BG = new Color(0f, 0.8f, 1f, 0.3f);
        private static readonly Color TOGGLE_OFF_BG = new Color(0.08f, 0.1f, 0.16f);
        private static readonly Color CUSTOM_CARD_BG = new Color(0.05f, 0.05f, 0.1f);
        private static readonly Color STEPPER_BG = new Color(0.08f, 0.12f, 0.2f);

        // ==================== LAYOUT ====================
        private const float CARD_H = 128f;
        private const float CARD_PAD_H = 16f;
        private const float CARD_PAD_V = 10f;
        private const float BTN_H = 70f;
        private const float CUSTOM_H = 195f;

        [MenuItem("DigitPark/Scenes/Build Scene/Games/BetSelection", false, 123)]
        public static void ShowWindow()
        {
            var window = GetWindow<BetSelectionUIBuilder>("BetSelection Scene Builder");
            window.minSize = new Vector2(400, 350);
        }

        private void OnGUI()
        {
            GUILayout.Label("BetSelection Scene Builder", EditorStyles.boldLabel);
            GUILayout.Space(10);

            string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (scene != "BetSelection")
            {
                EditorGUILayout.HelpBox(
                    $"Current scene: {scene}\nPlease open the BetSelection scene first!",
                    MessageType.Warning);
            }

            EditorGUILayout.HelpBox(
                "Creates BetSelection scene UI:\n" +
                "- BackButton prefab (top-left)\n" +
                "- Title + Game name header\n" +
                "- Prominent currency display\n" +
                "- 11 preset bet cards (Free + 5 DigitCoins + 5 DigitGems)\n" +
                "- Custom bet section (input + toggles + stepper)\n" +
                "- ScrollRect for smooth scrolling\n" +
                "- Play button (full-width, no Cancel — back button covers it)\n" +
                "- Auto-assigns all references",
                MessageType.Info);

            GUILayout.Space(10);

            GUI.backgroundColor = new Color(0.3f, 1f, 0.6f);
            if (GUILayout.Button("Build BetSelection Scene", GUILayout.Height(45)))
            {
                BuildScene();
            }
            GUI.backgroundColor = Color.white;
        }

        // ==================== MAIN BUILD ====================

        private static void BuildScene()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("[BetSelectionUIBuilder] No Canvas found in scene!");
                return;
            }

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
            }

            // Ensure GraphicRaycaster (required for ALL UI events)
            if (canvas.GetComponent<GraphicRaycaster>() == null)
                canvas.gameObject.AddComponent<GraphicRaycaster>();

            // Ensure EventSystem exists in scene
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            CleanCanvasChildren(canvas.transform);

            // Destroy any orphaned Scrollbar GOs at scene root (may survive across builds)
            foreach (var sb in Object.FindObjectsOfType<Scrollbar>(true))
                DestroyImmediate(sb.gameObject);

            // === BACKGROUND ===
            GameObject bg = CreateUI("Background", canvas.transform);
            SetFullStretch(bg.GetComponent<RectTransform>());
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = Color.white;
            bgImg.raycastTarget = false;

            // === HEADER ===
            CreateHeader(canvas.transform);


            // === BACK BUTTON (after header so it renders on top) ===
            GameObject backPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Common/BackButton.prefab");
            if (backPrefab != null)
            {
                GameObject backBtn = (GameObject)PrefabUtility.InstantiatePrefab(backPrefab, canvas.transform);
                backBtn.name = "BackButton";
                RectTransform brt = backBtn.GetComponent<RectTransform>();
                if (brt != null)
                {
                    brt.anchorMin = new Vector2(0, 0.5f);
                    brt.anchorMax = new Vector2(0, 0.5f);
                    brt.pivot = new Vector2(0, 0.5f);
                    brt.anchoredPosition = new Vector2(20, 0);
                    brt.sizeDelta = new Vector2(50, 50);
                }
            }

            // === SCROLL AREA ===
            GameObject content = CreateScrollArea(canvas.transform);

            // === FREE BET ===
            CreateBetCard("FreeBetOption", content.transform,
                "FREE", "+25 DigitCoins if you win", "FREE",
                "FreeBetCostText", "FreeBetRewardText",
                NEON_GREEN, GREEN_GLOW);

            // === COIN BETS ===
            CreateSectionDivider("CoinBetsHeader", content.transform, "DIGITCOINS", COIN_COLOR, "CoinsSectionText");

            CreateBetCard("Coins50BetOption", content.transform,
                "50 DigitCoins", "Win 100", null,
                "Coins50CostText", "Coins50RewardText",
                COIN_COLOR, COIN_GLOW);

            CreateBetCard("Coins100BetOption", content.transform,
                "100 DigitCoins", "Win 200", null,
                "Coins100CostText", "Coins100RewardText",
                COIN_COLOR, COIN_GLOW);

            CreateBetCard("Coins250BetOption", content.transform,
                "250 DigitCoins", "Win 500", null,
                "Coins250CostText", "Coins250RewardText",
                COIN_COLOR, COIN_GLOW);

            CreateBetCard("Coins500BetOption", content.transform,
                "500 DigitCoins", "Win 1,000", "x2",
                "Coins500CostText", "Coins500RewardText",
                COIN_COLOR, COIN_GLOW);

            CreateBetCard("Coins1000BetOption", content.transform,
                "1,000 DigitCoins", "Win 2,000", "x2",
                "Coins1000CostText", "Coins1000RewardText",
                new Color(1f, 0.7f, 0.1f), new Color(1f, 0.7f, 0.1f, 0.4f));

            CreateBetCard("Coins2500BetOption", content.transform,
                "2,500 DigitCoins", "Win 5,000", "x2",
                "Coins2500CostText", "Coins2500RewardText",
                new Color(1f, 0.5f, 0f), new Color(1f, 0.5f, 0f, 0.4f));

            // === CUSTOM BET ===
            CreateSectionDivider("CustomBetsHeader", content.transform, "CUSTOM", CUSTOM_TEAL, "CustomSectionText");
            CreateCustomBetSection(content.transform);

            // === ROUNDS SELECTION ===
            CreateSpacer(content.transform, 8f);
            CreateRoundsSelector(content.transform);
            CreateSpacer(content.transform, 12f);

            // === FIXED PLAY BUTTON (on Canvas, not in scroll) ===
            CreateFixedPlayButton(canvas.transform);

            // === FORCE LAYOUT REBUILD ===
            // Without this, ContentSizeFitter won't compute the Content height,
            // and ScrollRect thinks the content fits → scroll doesn't work
            Canvas.ForceUpdateCanvases();
            var contentRT = content.GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRT);

            // === FINALIZE ===
            EditorUtility.SetDirty(canvas.gameObject);
            EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
            Debug.Log("[BetSelectionUIBuilder] UI built. Running Reference Assigner...");

            // === AUTO-ASSIGN ===
            AutoAssigners.BetSelectionReferenceAssigner.RunAutoAssign();
            Debug.Log("[BetSelectionUIBuilder] References auto-assigned!");
        }

        // ==================== HEADER ====================

        private static void CreateHeader(Transform parent)
        {
            // Header: 100px standardized height
            GameObject header = CreateUI("HeaderSection", parent);
            RectTransform hrt = header.GetComponent<RectTransform>();
            hrt.anchorMin = new Vector2(0, 1);
            hrt.anchorMax = new Vector2(1, 1);
            hrt.pivot = new Vector2(0.5f, 1);
            hrt.anchoredPosition = new Vector2(0, -29);
            hrt.sizeDelta = new Vector2(0, 100);
            header.AddComponent<Image>().color = HEADER_BG;
            AddOutline(header, CYAN_DARK, 1);

            // Title (left side) — wider area so full text fits
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(header.transform, false);
            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0.15f);
            titleRT.anchorMax = new Vector2(0.5f, 0.85f);
            titleRT.offsetMin = new Vector2(20, 0);
            titleRT.offsetMax = new Vector2(0, 0);
            var titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "CHOOSE YOUR BET";
            titleTMP.fontSize = (int)FontSizes.H4;
            titleTMP.color = NEON_CYAN;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.MidlineLeft;
            titleTMP.enableAutoSizing = true;
            titleTMP.fontSizeMin = FontSizes.AutoMinTitle;
            titleTMP.fontSizeMax = FontSizes.H4;
            titleTMP.enableWordWrapping = false;
            titleTMP.overflowMode = TextOverflowModes.Ellipsis;
            titleTMP.raycastTarget = false;

            // Game name (below title)
            GameObject gameNameObj = new GameObject("GameNameText");
            gameNameObj.transform.SetParent(header.transform, false);
            RectTransform gnRT = gameNameObj.AddComponent<RectTransform>();
            gnRT.anchorMin = new Vector2(0, 0);
            gnRT.anchorMax = new Vector2(0.5f, 0.35f);
            gnRT.offsetMin = new Vector2(20, 0);
            gnRT.offsetMax = new Vector2(0, 0);
            var gnTMP = gameNameObj.AddComponent<TextMeshProUGUI>();
            gnTMP.text = "";
            gnTMP.fontSize = (int)FontSizes.Body;
            gnTMP.color = GOLD;
            gnTMP.fontStyle = FontStyles.Bold;
            gnTMP.alignment = TextAlignmentOptions.MidlineLeft;
            gnTMP.enableAutoSizing = true;
            gnTMP.fontSizeMin = FontSizes.AutoMinBody;
            gnTMP.fontSizeMax = gnTMP.fontSize;
            gnTMP.raycastTarget = false;

            // Currency pills (right side - exact same anchors as MainMenu, offset 120 right)
            var pills = CurrencyHeaderBarHelper.CreateCurrencyPills(header.transform);
            var pillsRT = pills.GetComponent<RectTransform>();
            pillsRT.anchorMin = new Vector2(0.52f, 0.5f);
            pillsRT.anchorMax = new Vector2(0.95f, 0.5f);
            pillsRT.pivot = new Vector2(0.5f, 0.5f);
            pillsRT.sizeDelta = new Vector2(0, 65);
        }

        // ==================== SCROLL AREA ====================

        private static GameObject CreateScrollArea(Transform parent)
        {
            // ScrollRect wrapper (matches ShopPremiumUIBuilder + AchievementsUIBuilder pattern)
            GameObject scrollArea = CreateUI("ScrollArea", parent);
            RectTransform srt = scrollArea.GetComponent<RectTransform>();
            // Leave 90px at bottom for fixed PlayButton, respect header at top
            srt.anchorMin = Vector2.zero;
            srt.anchorMax = new Vector2(1, 0.923f);
            srt.offsetMin = new Vector2(0, 200f); // 200px clearance for fixed PLAY button (175h + 10 margin + 15 padding)
            srt.offsetMax = Vector2.zero;

            ScrollRect scrollRect = scrollArea.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.1f;
            scrollRect.decelerationRate = 0.135f;
            scrollRect.scrollSensitivity = 50f;

            // Transparent Image on ScrollArea for raycast capture
            Image scrollImg = scrollArea.AddComponent<Image>();
            scrollImg.color = Color.clear;
            scrollImg.raycastTarget = true;

            // Viewport with Image + RectMask2D (matching AchievementsUIBuilder)
            GameObject viewport = CreateUI("Viewport", scrollArea.transform);
            RectTransform vpRT = viewport.GetComponent<RectTransform>();
            SetFullStretch(vpRT);
            Image vpImg = viewport.AddComponent<Image>();
            vpImg.color = Color.clear;
            vpImg.raycastTarget = true;
            viewport.AddComponent<RectMask2D>();

            // Content
            GameObject content = CreateUI("Content", viewport.transform);
            RectTransform crt = content.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0, 1);
            crt.anchorMax = new Vector2(1, 1);
            crt.pivot = new Vector2(0.5f, 1);
            crt.sizeDelta = new Vector2(0, 0);

            // Transparent Image on Content catches raycasts in dead zones (spacing/padding)
            // ensuring scroll works when touching ANY part of the content area
            Image contentBg = content.AddComponent<Image>();
            contentBg.color = Color.clear;

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(16, 16, 8, 12);
            vlg.spacing = 6;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            scrollRect.content = crt;
            scrollRect.viewport = vpRT;
            // Explicitly remove any scrollbar (may linger from previous builds)
            scrollRect.verticalScrollbar = null;
            scrollRect.horizontalScrollbar = null;

            return content;
        }

        // ==================== BET CARD ====================

        private static void CreateBetCard(string name, Transform parent,
            string costText, string rewardText, string badgeText,
            string costTextName, string rewardTextName,
            Color accentColor, Color glowColor)
        {
            GameObject card = CreateUI(name, parent);
            var le = card.AddComponent<LayoutElement>();
            le.preferredHeight = CARD_H;

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = CARD_BG;

            AddOutline(card, glowColor, 2);
            AddShadow(card, new Color(accentColor.r, accentColor.g, accentColor.b, 0.2f), new Vector2(0, -2));

            Button btn = card.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            btn.colors = colors;

            var hlg = card.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset((int)CARD_PAD_H, (int)CARD_PAD_H,
                                          (int)CARD_PAD_V, (int)CARD_PAD_V);
            hlg.spacing = 8;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            // Accent bar
            GameObject accentBar = CreateUI("AccentBar", card.transform);
            var abLE = accentBar.AddComponent<LayoutElement>();
            abLE.preferredWidth = 4;
            accentBar.AddComponent<Image>().color = accentColor;
            AddOutline(accentBar, new Color(accentColor.r, accentColor.g, accentColor.b, 0.5f), 1);

            // Cost text
            GameObject costGO = CreateUI(costTextName, card.transform);
            var costLE = costGO.AddComponent<LayoutElement>();
            costLE.flexibleWidth = 1;
            TextMeshProUGUI costTMP = costGO.AddComponent<TextMeshProUGUI>();
            costTMP.text = costText;
            costTMP.fontSize = FontSizes.BodyLarge;
            costTMP.color = accentColor;
            costTMP.fontStyle = FontStyles.Bold;
            costTMP.alignment = TextAlignmentOptions.MidlineLeft;
            costTMP.enableAutoSizing = true;
            costTMP.fontSizeMin = FontSizes.AutoMinBody;
            costTMP.fontSizeMax = FontSizes.BodyLarge;
            costTMP.enableWordWrapping = false;
            costTMP.overflowMode = TextOverflowModes.Ellipsis;
            costTMP.raycastTarget = false;


            // Reward text
            GameObject rewardGO = CreateUI(rewardTextName, card.transform);
            var rwLE = rewardGO.AddComponent<LayoutElement>();
            rwLE.flexibleWidth = 1;
            TextMeshProUGUI rewardTMP = rewardGO.AddComponent<TextMeshProUGUI>();
            rewardTMP.text = rewardText;
            rewardTMP.fontSize = FontSizes.Body;
            rewardTMP.fontStyle = FontStyles.Bold;
            rewardTMP.color = TEXT_SECONDARY;
            rewardTMP.alignment = TextAlignmentOptions.MidlineRight;
            rewardTMP.enableAutoSizing = true;
            rewardTMP.fontSizeMin = FontSizes.AutoMinBody;
            rewardTMP.fontSizeMax = FontSizes.Body;
            rewardTMP.raycastTarget = false;


            // Badge (Image on parent, TMP on child - separate Graphic components)
            if (!string.IsNullOrEmpty(badgeText))
            {
                GameObject badge = CreateUI("Badge", card.transform);
                var badgeLE = badge.AddComponent<LayoutElement>();
                badgeLE.preferredWidth = 110;
                badgeLE.preferredHeight = 38;
                badge.AddComponent<Image>().color =
                    new Color(accentColor.r, accentColor.g, accentColor.b, 0.25f);
                AddOutline(badge, accentColor * 0.6f, 1);

                GameObject badgeTextGO = CreateUI("BadgeText", badge.transform);
                SetFullStretch(badgeTextGO.GetComponent<RectTransform>());
                TextMeshProUGUI badgeTMP = badgeTextGO.AddComponent<TextMeshProUGUI>();
                badgeTMP.text = badgeText;
                badgeTMP.fontSize = FontSizes.Body;
                badgeTMP.color = accentColor;
                badgeTMP.fontStyle = FontStyles.Bold;
                badgeTMP.alignment = TextAlignmentOptions.Center;
                badgeTMP.enableAutoSizing = true;
                badgeTMP.fontSizeMin = FontSizes.AutoMinBody;
                badgeTMP.fontSizeMax = badgeTMP.fontSize;
                badgeTMP.raycastTarget = false;

            }

            // Glass overlay
            GameObject glass = CreateUI("GlassOverlay", card.transform);
            var glLE = glass.AddComponent<LayoutElement>();
            glLE.ignoreLayout = true;
            RectTransform grt = glass.GetComponent<RectTransform>();
            grt.anchorMin = new Vector2(0, 0.5f);
            grt.anchorMax = new Vector2(1, 1);
            grt.offsetMin = Vector2.zero;
            grt.offsetMax = Vector2.zero;
            Image glImg = glass.AddComponent<Image>();
            glImg.color = GLASS;
            glImg.raycastTarget = false;
        }

        // ==================== CUSTOM BET SECTION ====================

        private static void CreateCustomBetSection(Transform parent)
        {
            // Main card container
            GameObject card = CreateUI("CustomBetCard", parent);
            var cardLE = card.AddComponent<LayoutElement>();
            cardLE.preferredHeight = CUSTOM_H;

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = CUSTOM_CARD_BG;
            AddOutline(card, CUSTOM_GLOW, 2);
            AddShadow(card, new Color(0f, 0.8f, 0.7f, 0.15f), new Vector2(0, -2));

            var cardVLG = card.AddComponent<VerticalLayoutGroup>();
            cardVLG.padding = new RectOffset(16, 16, 10, 10);
            cardVLG.spacing = 8;
            cardVLG.childAlignment = TextAnchor.MiddleCenter;
            cardVLG.childForceExpandWidth = true;
            cardVLG.childForceExpandHeight = false;
            cardVLG.childControlWidth = true;
            cardVLG.childControlHeight = false;

            // === ROW 1: Currency toggles ===
            GameObject toggleRow = CreateUI("ToggleRow", card.transform);
            var trLE = toggleRow.AddComponent<LayoutElement>();
            trLE.preferredHeight = 46;
            var trHLG = toggleRow.AddComponent<HorizontalLayoutGroup>();
            trHLG.spacing = 10;
            trHLG.childAlignment = TextAnchor.MiddleCenter;
            trHLG.childForceExpandWidth = true;
            trHLG.childForceExpandHeight = true;
            trHLG.childControlWidth = true;
            trHLG.childControlHeight = true;

            CreateToggleButton("CustomCoinsToggle", toggleRow.transform, "DIGITCOINS", COIN_COLOR, true, "CoinsToggleText");

            // === ROW 2: Stepper (minus, input, plus) ===
            GameObject inputRow = CreateUI("InputRow", card.transform);
            var irLE = inputRow.AddComponent<LayoutElement>();
            irLE.preferredHeight = 56;
            var irHLG = inputRow.AddComponent<HorizontalLayoutGroup>();
            irHLG.spacing = 8;
            irHLG.childAlignment = TextAnchor.MiddleCenter;
            irHLG.childForceExpandWidth = false;
            irHLG.childForceExpandHeight = true;
            irHLG.childControlWidth = true;
            irHLG.childControlHeight = true;

            // Left spacer
            CreateFlexSpacer(inputRow.transform);

            // Minus button
            CreateStepperButton("CustomMinusButton", inputRow.transform, "-50", 65);

            // Input field
            CreateAmountInputField("CustomAmountInput", inputRow.transform);

            // Plus button
            CreateStepperButton("CustomPlusButton", inputRow.transform, "+50", 65);

            // Right spacer
            CreateFlexSpacer(inputRow.transform);

            // === ROW 3: Reward preview ===
            GameObject previewGO = CreateUI("CustomRewardText", card.transform);
            var pvLE = previewGO.AddComponent<LayoutElement>();
            pvLE.preferredHeight = 34;
            TextMeshProUGUI pvTMP = previewGO.AddComponent<TextMeshProUGUI>();
            pvTMP.text = "Win: 100 DigitCoins";
            pvTMP.fontSize = FontSizes.Body;
            pvTMP.color = CUSTOM_TEAL;
            pvTMP.fontStyle = FontStyles.Bold;
            pvTMP.alignment = TextAlignmentOptions.Center;
            pvTMP.enableAutoSizing = true;
            pvTMP.fontSizeMin = FontSizes.AutoMinBody;
            pvTMP.fontSizeMax = FontSizes.Body;

        }

        private static void CreateToggleButton(string name, Transform parent,
            string text, Color textColor, bool initiallyActive, string textGoName = "Text")
        {
            GameObject go = CreateUI(name, parent);
            Image bg = go.AddComponent<Image>();
            bg.color = initiallyActive ? TOGGLE_ON_BG : TOGGLE_OFF_BG;
            AddOutline(go, new Color(textColor.r, textColor.g, textColor.b, 0.3f), 1);

            Button btn = go.AddComponent<Button>();
            var c = btn.colors;
            c.normalColor = Color.white;
            c.highlightedColor = new Color(1, 1, 1, 0.9f);
            c.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            btn.colors = c;

            // Text on child (unique name for AutoLocalizer)
            GameObject textGO = CreateUI(textGoName, go.transform);
            SetFullStretch(textGO.GetComponent<RectTransform>());
            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = FontSizes.Body;
            tmp.color = textColor;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinBody;
            tmp.fontSizeMax = FontSizes.Body;
        }

        private static void CreateStepperButton(string name, Transform parent, string text, float width)
        {
            GameObject go = CreateUI(name, parent);
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = width;
            Image bg = go.AddComponent<Image>();
            bg.color = STEPPER_BG;
            AddOutline(go, CYAN_DARK, 1);

            Button btn = go.AddComponent<Button>();
            var c = btn.colors;
            c.normalColor = Color.white;
            c.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            c.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            btn.colors = c;

            GameObject textGO = CreateUI("Text", go.transform);
            SetFullStretch(textGO.GetComponent<RectTransform>());
            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = FontSizes.Body;
            tmp.color = NEON_CYAN;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinBody;
            tmp.fontSizeMax = tmp.fontSize;
        }

        private static void CreateAmountInputField(string name, Transform parent)
        {
            // Root with background
            GameObject inputObj = CreateUI(name, parent);
            var le = inputObj.AddComponent<LayoutElement>();
            le.preferredWidth = 180;

            Image bg = inputObj.AddComponent<Image>();
            bg.color = INPUT_BG;
            AddOutline(inputObj, CUSTOM_TEAL * 0.7f, 1);

            TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();

            // Text Area
            GameObject textArea = CreateUI("Text Area", inputObj.transform);
            RectTransform taRT = textArea.GetComponent<RectTransform>();
            SetFullStretch(taRT);
            taRT.offsetMin = new Vector2(10, 0);
            taRT.offsetMax = new Vector2(-10, 0);

            // Placeholder (unique name for AutoLocalizer)
            GameObject phGO = CreateUI("AmountPlaceholder", textArea.transform);
            RectTransform phRT = phGO.GetComponent<RectTransform>();
            SetFullStretch(phRT);
            TextMeshProUGUI phTMP = phGO.AddComponent<TextMeshProUGUI>();
            phTMP.text = "Amount...";
            phTMP.fontSize = FontSizes.Body;
            phTMP.fontStyle = FontStyles.Bold;
            phTMP.color = TEXT_DIM;
            phTMP.alignment = TextAlignmentOptions.Center;
            phTMP.enableAutoSizing = true;
            phTMP.fontSizeMin = FontSizes.AutoMinBody;
            phTMP.fontSizeMax = FontSizes.Body;


            // Input text
            GameObject txtGO = CreateUI("Text", textArea.transform);
            RectTransform txtRT = txtGO.GetComponent<RectTransform>();
            SetFullStretch(txtRT);
            TextMeshProUGUI txtTMP = txtGO.AddComponent<TextMeshProUGUI>();
            txtTMP.text = "50";
            txtTMP.fontSize = FontSizes.Body;
            txtTMP.color = TEXT_PRIMARY;
            txtTMP.fontStyle = FontStyles.Bold;
            txtTMP.alignment = TextAlignmentOptions.Center;
            txtTMP.enableAutoSizing = true;
            txtTMP.fontSizeMin = FontSizes.AutoMinBody;
            txtTMP.fontSizeMax = txtTMP.fontSize;


            // Wire up
            inputField.textViewport = taRT;
            inputField.textComponent = txtTMP;
            inputField.placeholder = phTMP;
            inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            inputField.characterLimit = 6;
            inputField.caretColor = NEON_CYAN;
            inputField.selectionColor = new Color(0f, 1f, 1f, 0.2f);
        }

        // ==================== SECTION DIVIDER ====================

        private static void CreateSectionDivider(string name, Transform parent, string text, Color color,
            string textGoName = "SectionText")
        {
            GameObject div = CreateUI(name, parent);
            var le = div.AddComponent<LayoutElement>();
            le.preferredHeight = 48;

            var hlg = div.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(5, 5, 0, 0);
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth = true;
            hlg.childControlHeight = false;

            // Left line
            GameObject lineL = CreateUI("LineLeft", div.transform);
            var llLE = lineL.AddComponent<LayoutElement>();
            llLE.flexibleWidth = 1;
            llLE.preferredHeight = 1;
            lineL.AddComponent<Image>().color = new Color(color.r, color.g, color.b, 0.3f);

            // Section text (unique name for AutoLocalizer mapping)
            GameObject textGO = CreateUI(textGoName, div.transform);
            var tLE = textGO.AddComponent<LayoutElement>();
            tLE.preferredWidth = 260;
            tLE.preferredHeight = 46;
            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = FontSizes.BodyLarge;
            tmp.color = color;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinBody;
            tmp.fontSizeMax = FontSizes.BodyLarge;

            // Right line
            GameObject lineR = CreateUI("LineRight", div.transform);
            var lrLE = lineR.AddComponent<LayoutElement>();
            lrLE.flexibleWidth = 1;
            lrLE.preferredHeight = 1;
            lineR.AddComponent<Image>().color = new Color(color.r, color.g, color.b, 0.3f);
        }

        // ==================== ROUNDS SELECTOR ====================

        private static void CreateRoundsSelector(Transform parent)
        {
            GameObject roundsPanel = CreateUI("RoundsPanel", parent);
            var rpLE = roundsPanel.AddComponent<LayoutElement>();
            rpLE.preferredHeight = 90;

            var rpVLG = roundsPanel.AddComponent<VerticalLayoutGroup>();
            rpVLG.childAlignment = TextAnchor.MiddleCenter;
            rpVLG.spacing = 8;
            rpVLG.childForceExpandWidth = true;
            rpVLG.childForceExpandHeight = false;
            rpVLG.childControlWidth = true;
            rpVLG.childControlHeight = true;
            rpVLG.padding = new RectOffset(12, 12, 6, 6);

            Image rpBg = roundsPanel.AddComponent<Image>();
            rpBg.color = CARD_BG;
            AddOutline(roundsPanel, CYAN_GLOW, 1);

            // Label
            GameObject label = CreateUI("RoundsLabel", roundsPanel.transform);
            var labelLE = label.AddComponent<LayoutElement>();
            labelLE.preferredHeight = 28;
            TextMeshProUGUI labelTmp = label.AddComponent<TextMeshProUGUI>();
            labelTmp.text = "ROUNDS";
            labelTmp.fontSize = FontSizes.Body;
            labelTmp.color = NEON_CYAN;
            labelTmp.fontStyle = FontStyles.Bold;
            labelTmp.alignment = TextAlignmentOptions.Center;
            labelTmp.enableAutoSizing = true;
            labelTmp.fontSizeMin = FontSizes.AutoMinBody;
            labelTmp.fontSizeMax = FontSizes.Body;

            // Buttons row
            GameObject row = CreateUI("RoundsButtonsRow", roundsPanel.transform);
            var rowLE = row.AddComponent<LayoutElement>();
            rowLE.preferredHeight = 44;

            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 14;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            // 3 round buttons: 1, 3, 5 — default "3" selected
            CreateRoundsButton("Rounds1Button", row.transform, "1", false, NEON_CYAN, "Rounds1ButtonText");
            CreateRoundsButton("Rounds3Button", row.transform, "3", true, NEON_CYAN, "Rounds3ButtonText");
            CreateRoundsButton("Rounds5Button", row.transform, "5", false, NEON_CYAN, "Rounds5ButtonText");
        }

        private static void CreateRoundsButton(string name, Transform parent,
            string text, bool selected, Color accentColor, string textGoName)
        {
            GameObject go = CreateUI(name, parent);
            Image bg = go.AddComponent<Image>();
            bg.color = selected ? new Color(accentColor.r, accentColor.g, accentColor.b, 0.3f) : TOGGLE_OFF_BG;
            AddOutline(go, new Color(accentColor.r, accentColor.g, accentColor.b, selected ? 0.6f : 0.2f), 1);

            Button btn = go.AddComponent<Button>();
            var c = btn.colors;
            c.normalColor = Color.white;
            c.highlightedColor = new Color(1, 1, 1, 0.9f);
            c.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            btn.colors = c;

            GameObject textGO = CreateUI(textGoName, go.transform);
            SetFullStretch(textGO.GetComponent<RectTransform>());
            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = FontSizes.Body;
            tmp.color = selected ? accentColor : TEXT_SECONDARY;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinBody;
            tmp.fontSizeMax = FontSizes.Body;
        }

        // ==================== FIXED PLAY BUTTON (anchored to bottom of Canvas) ====================

        private static void CreateFixedPlayButton(Transform canvasParent)
        {
            GameObject go = CreateUI("PlayButton", canvasParent);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, 10f);
            rt.sizeDelta = new Vector2(-32f, 175f);

            go.AddComponent<Image>().color = BTN_PLAY;
            AddOutline(go, BTN_PLAY_GLOW, 2);
            AddShadow(go, BTN_PLAY_GLOW, new Vector2(0, -3));

            Button btn = go.AddComponent<Button>();
            var c = btn.colors;
            c.normalColor = Color.white;
            c.highlightedColor = new Color(1, 1, 1, 0.9f);
            c.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            btn.colors = c;

            GameObject textGO = CreateUI("PlayButtonText", go.transform);
            SetFullStretch(textGO.GetComponent<RectTransform>());
            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = "PLAY";
            tmp.fontSize = FontSizes.H3;
            tmp.color = TEXT_PRIMARY;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinTitle;
            tmp.fontSizeMax = FontSizes.H3;
            tmp.raycastTarget = false;
        }

        // ==================== ACTION BUTTONS (legacy, unused) ====================

        private static void CreateActionButtons(Transform parent)
        {
            GameObject row = CreateUI("ButtonsRow", parent);
            var rowLE = row.AddComponent<LayoutElement>();
            rowLE.preferredHeight = BTN_H;

            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 14;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            CreateActionButton("PlayButton", row.transform, "PLAY", BTN_PLAY, BTN_PLAY_GLOW, true, "PlayButtonText");
            // CancelButton removed — back button covers cancel; matchmaking scene has its own cancel
        }

        private static void CreateActionButton(string name, Transform parent, string text,
            Color bgColor, Color glowColor, bool hasGlow, string textGoName = "Text")
        {
            GameObject go = CreateUI(name, parent);
            go.AddComponent<Image>().color = bgColor;

            if (hasGlow)
            {
                AddOutline(go, glowColor, 2);
                AddShadow(go, glowColor, new Vector2(0, -3));
            }

            Button btn = go.AddComponent<Button>();
            var c = btn.colors;
            c.normalColor = Color.white;
            c.highlightedColor = new Color(1, 1, 1, 0.9f);
            c.pressedColor = new Color(0.75f, 0.75f, 0.75f);
            btn.colors = c;

            GameObject textGO = CreateUI(textGoName, go.transform);
            SetFullStretch(textGO.GetComponent<RectTransform>());
            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = FontSizes.Body;
            tmp.color = Color.white;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinBody;
            tmp.fontSizeMax = FontSizes.Body;
        }

        // ==================== HELPERS ====================

        private static GameObject CreateUI(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private static void SetFullStretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text,
            int fontSize, Color color, FontStyles style, float height)
        {
            GameObject go = CreateUI(name, parent);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = height;

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinBody;
            tmp.fontSizeMax = fontSize;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            return tmp;
        }

        private static void CreateSpacer(Transform parent, float height)
        {
            GameObject spacer = CreateUI("Spacer", parent);
            spacer.AddComponent<LayoutElement>().preferredHeight = height;
        }

        private static void CreateFlexSpacer(Transform parent)
        {
            GameObject spacer = CreateUI("FlexSpacer", parent);
            spacer.AddComponent<LayoutElement>().flexibleWidth = 1;
        }

        private static void AddOutline(GameObject obj, Color color, float distance)
        {
            Outline outline = obj.GetComponent<Outline>();
            if (outline == null) outline = obj.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(distance, distance);
        }

        private static void AddShadow(GameObject obj, Color color, Vector2 distance)
        {
            Shadow shadow = obj.GetComponent<Shadow>();
            if (shadow == null) shadow = obj.AddComponent<Shadow>();
            shadow.effectColor = color;
            shadow.effectDistance = distance;
        }

        private static void CleanCanvasChildren(Transform canvasTransform)
        {
            for (int i = canvasTransform.childCount - 1; i >= 0; i--)
            {
                var child = canvasTransform.GetChild(i);
                if (child.name != "EventSystem" && child.name != "Main Camera")
                    DestroyImmediate(child.gameObject);
            }
        }
    }
}
