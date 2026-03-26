using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;
using DigitPark.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// Settings UI Builder - Professional card-based design
    /// Builds: Account → Audio → Appearance → Premium → Legal → Danger Zone
    ///
    /// Menu: DigitPark/UI Builders/Settings
    /// </summary>
    public static class SettingsUIBuilder
    {
        // ==================== RESOLUTION ====================
        private const float SCREEN_WIDTH = 1080f;
        private const float SCREEN_HEIGHT = 1920f;

        // ==================== COLORS ====================
        private static readonly Color DARK_NAVY = new Color(0.02f, 0.04f, 0.08f, 1f);
        private static readonly Color CARD_BG = new Color(0.08f, 0.10f, 0.22f, 0.95f);
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_BORDER = new Color(0f, 0.8f, 0.8f, 0.25f);
        private static readonly Color DANGER_RED = new Color(1f, 0.3f, 0.3f, 1f);
        private static readonly Color DANGER_BORDER = new Color(1f, 0.3f, 0.3f, 0.3f);
        private static readonly Color TEXT_WHITE = Color.white;
        private static readonly Color TEXT_GRAY = new Color(0.6f, 0.6f, 0.65f, 1f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color BUTTON_BG = new Color(0.10f, 0.12f, 0.25f, 0.6f);
        private static readonly Color SLIDER_TRACK = new Color(0.15f, 0.15f, 0.22f, 1f);
        private static readonly Color SEPARATOR_COLOR = new Color(0.3f, 0.3f, 0.4f, 0.2f);
        private static readonly Color TOGGLE_OFF_BG = new Color(0.25f, 0.25f, 0.3f, 1f);
        private static readonly Color DROPDOWN_BG = new Color(0.12f, 0.14f, 0.28f, 1f);
        private static readonly Color DROPDOWN_ITEM_BG = new Color(0.10f, 0.12f, 0.25f, 1f);
        private static readonly Color DROPDOWN_ITEM_HOVER = new Color(0.15f, 0.18f, 0.35f, 1f);

        // ==================== LAYOUT ====================
        private const float SIDE_PADDING = 30f;
        private const float CARD_INNER_PAD = 24f;
        private const float CARD_SPACING = 16f;
        private const float ROW_HEIGHT = 56f;
        private const float SLIDER_ROW_HEIGHT = 68f;
        private const float HEADER_HEIGHT = 100f;
        private const float SECTION_TITLE_HEIGHT = 36f;
        private const float SEPARATOR_HEIGHT = 1f;

        // ==================== ASSETS ====================
        private const string WHITE_SPRITE_PATH = "Assets/_Project/Art/Icons/UI/WhiteSquare.png";
        private const string FONT_PATH = "Assets/_Project/Art/Fonts/Rajdhani/Rajdhani-Medium SDF.asset";
        private const string BACK_BUTTON_PREFAB = "Assets/_Project/Prefabs/Common/BackButton.prefab";

        private static Sprite WhiteSprite => AssetDatabase.LoadAssetAtPath<Sprite>(WHITE_SPRITE_PATH);
        private static TMP_FontAsset Font => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_PATH);

        // ==================== MAIN ENTRY POINT ====================

        [MenuItem("DigitPark/Scenes/Build Scene/Core/Settings", false, 111)]
        public static void BuildSettingsUI()
        {
            if (WhiteSprite == null || Font == null)
            {
                Debug.LogError("[SettingsUIBuilder] Missing WhiteSquare.png or Font asset.");
                return;
            }

            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("[SettingsUIBuilder] No Canvas found in scene.");
                return;
            }

            Debug.Log("[SettingsUIBuilder] Building Settings UI...");

            CleanExistingUI(canvas);
            BuildBackground(canvas);
            BuildHeader(canvas);
            Transform content = BuildScrollView(canvas);

            // Build all card sections
            BuildAccountCard(content);
            BuildAudioCard(content);
            BuildAppearanceCard(content);
            BuildPremiumCard(content);
            BuildLegalCard(content);

            BuildDangerZoneCard(content);
            BuildVersionFooter(content);

            // Build overlay panels (on top of canvas, not inside ScrollView)
            BuildOverlayPanels(canvas);

            Canvas.ForceUpdateCanvases();
            EditorUtility.SetDirty(canvas.gameObject);

            // Auto-assign references to SettingsManager
            DigitPark.Editor.AutoAssigners.SettingsReferenceAssigner.RunAutoAssign();

            Debug.Log("[SettingsUIBuilder] Settings UI built successfully!");
        }

        // ==================== CLEAN ====================

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

        private static void CleanExistingUI(Canvas canvas)
        {
            var toDestroy = new List<Transform>();
            foreach (Transform child in canvas.transform)
            {
                if (child.name != "EventSystem" && child.name != "---ANIMATION_MANAGERS---")
                    toDestroy.Add(child);
            }
            foreach (var child in toDestroy)
                Object.DestroyImmediate(child.gameObject);

            // Configure Canvas
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(SCREEN_WIDTH, SCREEN_HEIGHT);
            scaler.matchWidthOrHeight = 0.5f;
        }

        // ==================== BACKGROUND ====================

        private static void BuildBackground(Canvas canvas)
        {
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(canvas.transform, false);

            RectTransform rt = bg.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image img = bg.AddComponent<Image>();
            img.sprite = WhiteSprite;
            img.color = Color.white; // ThemeApplier tints at runtime
            img.raycastTarget = false;

            bg.transform.SetAsFirstSibling();
        }

        // ==================== HEADER ====================

        private static void BuildHeader(Canvas canvas)
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(canvas.transform, false);

            RectTransform rt = header.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, -29); // consistent top margin across all scenes
            rt.sizeDelta = new Vector2(0, HEADER_HEIGHT);

            // Back button - Neon Cyan prefab
            GameObject backBtnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BACK_BUTTON_PREFAB);
            GameObject backBtn;
            if (backBtnPrefab != null)
            {
                backBtn = (GameObject)PrefabUtility.InstantiatePrefab(backBtnPrefab, header.transform);
                backBtn.name = "BackButton";
            }
            else
            {
                backBtn = new GameObject("BackButton");
                backBtn.transform.SetParent(header.transform, false);
                backBtn.AddComponent<Image>().color = new Color(0, 0, 0, 0);
                backBtn.AddComponent<Button>();
                Debug.LogWarning("[SettingsUIBuilder] BackButton prefab not found, using fallback");
            }
            RectTransform backRT = backBtn.GetComponent<RectTransform>();
            backRT.anchorMin = new Vector2(0, 0.5f);
            backRT.anchorMax = new Vector2(0, 0.5f);
            backRT.pivot = new Vector2(0, 0.5f);
            backRT.anchoredPosition = new Vector2(20, 0);
            backRT.sizeDelta = new Vector2(50, 50);

            // Title - Left-aligned next to back button (Material Design pattern)
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(header.transform, false);

            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.offsetMin = new Vector2(80, 0);  // After back button (20 + 50 + 10 margin)
            titleRT.offsetMax = new Vector2(-20, 0);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.font = Font;
            titleText.text = "SETTINGS";
            titleText.fontSize = FontSizes.H4;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = CYAN_NEON;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = FontSizes.AutoMinTitle;
            titleText.fontSizeMax = FontSizes.H4;
            titleText.overflowMode = TextOverflowModes.Ellipsis;
            titleText.raycastTarget = false;
        }

        // ==================== SCROLLVIEW ====================

        private static Transform BuildScrollView(Canvas canvas)
        {
            // ScrollView
            GameObject scrollView = new GameObject("ScrollView");
            scrollView.transform.SetParent(canvas.transform, false);

            RectTransform scrollRT = scrollView.AddComponent<RectTransform>();
            scrollRT.anchorMin = Vector2.zero;
            scrollRT.anchorMax = Vector2.one;
            scrollRT.offsetMin = Vector2.zero;
            scrollRT.offsetMax = new Vector2(0, -HEADER_HEIGHT);

            ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.elasticity = 0.1f;
            scroll.inertia = true;
            scroll.decelerationRate = 0.135f;
            scroll.scrollSensitivity = 0.5f;

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);

            RectTransform viewportRT = viewport.AddComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.sizeDelta = Vector2.zero;

            Image viewportImg = viewport.AddComponent<Image>();
            viewportImg.color = Color.clear;
            viewportImg.raycastTarget = true;

            viewport.AddComponent<RectMask2D>();

            scroll.viewport = viewportRT;

            // Content (SettingsPanel)
            GameObject content = new GameObject("SettingsPanel");
            content.transform.SetParent(viewport.transform, false);

            RectTransform contentRT = content.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.sizeDelta = Vector2.zero;

            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = CARD_SPACING;
            vlg.padding = new RectOffset((int)SIDE_PADDING, (int)SIDE_PADDING, 20, 40);

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = contentRT;

            return content.transform;
        }

        // ==================== CARD SECTIONS ====================

        private static void BuildAccountCard(Transform parent)
        {
            Transform card = CreateCard(parent, "AccountCard", "ACCOUNT", CYAN_BORDER);

            CreateChangeNameRow(card);
            CreateSeparator(card);
            CreatePlayerIDRow(card);
        }

        /// <summary>
        /// Creates Change Name row with gem sprite icon instead of unicode diamond
        /// </summary>
        private static void CreateChangeNameRow(Transform parent)
        {
            GameObject row = new GameObject("ChangeNameButton");
            row.transform.SetParent(parent, false);

            LayoutElement layout = row.AddComponent<LayoutElement>();
            layout.preferredHeight = ROW_HEIGHT;

            Image bg = row.AddComponent<Image>();
            bg.sprite = WhiteSprite;
            bg.color = BUTTON_BG;
            bg.raycastTarget = true;

            Button btn = row.AddComponent<Button>();
            btn.targetGraphic = bg;
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1, 1, 1, 1.2f);
            cb.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            btn.colors = cb;

            // Label text (left)
            GameObject labelObj = new GameObject("ChangeNameButtonText");
            labelObj.transform.SetParent(row.transform, false);
            RectTransform labelRT = labelObj.AddComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = new Vector2(16, 0);
            labelRT.offsetMax = new Vector2(-120, 0);
            TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
            label.font = Font;
            label.text = "Change Name";
            label.fontSize = FontSizes.H3;
            label.enableAutoSizing = true;
            label.fontSizeMin = FontSizes.Caption;
            label.fontSizeMax = FontSizes.H3;
            label.fontStyle = FontStyles.Bold;
            label.color = TEXT_WHITE;
            label.alignment = TextAlignmentOptions.Left;
            label.raycastTarget = false;

            // Cost container (right side): "100" text + gem sprite
            GameObject costContainer = new GameObject("CostContainer");
            costContainer.transform.SetParent(row.transform, false);
            RectTransform costContainerRT = costContainer.AddComponent<RectTransform>();
            costContainerRT.anchorMin = new Vector2(0.50f, 0.1f);
            costContainerRT.anchorMax = new Vector2(1.0f, 0.9f);
            costContainerRT.offsetMin = Vector2.zero;
            costContainerRT.offsetMax = new Vector2(-16, 0); // aligned with separator margin

            HorizontalLayoutGroup costHLG = costContainer.AddComponent<HorizontalLayoutGroup>();
            costHLG.spacing = 6;
            costHLG.childAlignment = TextAnchor.MiddleRight;
            costHLG.childControlWidth = false;
            costHLG.childControlHeight = true;
            costHLG.childForceExpandWidth = false;
            costHLG.padding = new RectOffset(0, 0, 0, 0);

            // Cost text "100"
            GameObject costTextObj = new GameObject("ChangeNameCostText");
            costTextObj.transform.SetParent(costContainer.transform, false);
            costTextObj.AddComponent<RectTransform>();
            LayoutElement costTextLE = costTextObj.AddComponent<LayoutElement>();
            costTextLE.preferredWidth = 60;
            TextMeshProUGUI costText = costTextObj.AddComponent<TextMeshProUGUI>();
            costText.font = Font;
            costText.text = "100";
            costText.fontSize = FontSizes.H3;
            costText.fontStyle = FontStyles.Bold;
            costText.color = CYAN_NEON;
            costText.alignment = TextAlignmentOptions.MidlineRight;
            costText.raycastTarget = false;
            costText.enableAutoSizing = true;
            costText.fontSizeMin = FontSizes.AutoMinBody;
            costText.fontSizeMax = FontSizes.H3;
            costText.overflowMode = TextOverflowModes.Ellipsis;

            // Gem icon sprite
            GameObject gemIconObj = new GameObject("GemIcon");
            gemIconObj.transform.SetParent(costContainer.transform, false);
            gemIconObj.AddComponent<RectTransform>();
            LayoutElement gemLE = gemIconObj.AddComponent<LayoutElement>();
            gemLE.preferredWidth = 28;
            gemLE.preferredHeight = 28;
            Image gemImg = gemIconObj.AddComponent<Image>();
            gemImg.preserveAspect = true;
            gemImg.raycastTarget = false;

            Sprite gemSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/Currency/icon_digitgem_single.png");
            if (gemSprite != null)
            {
                gemImg.sprite = gemSprite;
                gemImg.color = Color.white; // Show original gem colors
            }
            else
            {
                gemImg.color = CYAN_NEON; // Fallback color if sprite not found
            }
        }

        private static void BuildAudioCard(Transform parent)
        {
            Transform card = CreateCard(parent, "AudioCard", "SOUND", CYAN_BORDER);

            CreateSliderRow(card, "SoundVolumeSlider", "SoundValueText", "Music", 0.5f);
            CreateSeparator(card);
            CreateSliderRow(card, "EffectsVolumeSlider", "EffectsValueText", "Effects", 0.5f);
            CreateSeparator(card);
            CreateToggleRow(card, "VibrationToggle", "Vibration", true);
        }

        private static void BuildAppearanceCard(Transform parent)
        {
            Transform card = CreateCard(parent, "AppearanceCard", "APPEARANCE", CYAN_BORDER);

            CreateDropdownRow(card, "LanguageDropdown", "ChangeLanguageLabel", "Language",
                new[] { "English", "Español" }, 1);
        }

        private static void BuildPremiumCard(Transform parent)
        {
            Transform card = CreateCard(parent, "PremiumSection", "PREMIUM", new Color(1f, 0.84f, 0f, 0.2f));

            // Shop entry
            CreateSettingsRow(card, "ShopButton", "Shop", ">", CYAN_NEON, true);
            CreateSeparator(card);

            // Restore Purchases
            CreateSettingsRow(card, "RestorePurchasesButton", "Restore Purchases", ">", CYAN_NEON, true);
        }

        private static void BuildLegalCard(Transform parent)
        {
            Transform card = CreateCard(parent, "LegalCard", "LEGAL", CYAN_BORDER);

            CreateSettingsRow(card, "TermsButton", "Terms and Conditions", ">", CYAN_NEON, true);
            CreateSeparator(card);
            CreateSettingsRow(card, "PrivacyButton", "Privacy Policy", ">", CYAN_NEON, true);
        }

        private static void BuildDangerZoneCard(Transform parent)
        {
            Transform card = CreateCard(parent, "DangerCard", "DANGER ZONE", DANGER_BORDER);

            // Logout - styled as a full button
            CreateDangerButton(card, "LogoutButton", "Sign Out", TEXT_GRAY, new Color(0.25f, 0.25f, 0.30f, 0.8f));
            CreateSeparator(card);
            // Delete Account - red button
            CreateDangerButton(card, "DeleteAccountButton", "Delete Account", DANGER_RED, new Color(0.3f, 0.08f, 0.08f, 0.6f));
        }

        private static void BuildOverlayPanels(Canvas canvas)
        {
            // Logout ConfirmPanelUI overlay
            BuildConfirmPanelOverlay(canvas.transform, "LogoutConfirmPanel",
                "Sign Out", "Are you sure you want to sign out?",
                "Confirm", "Cancel");

            // Delete Account ConfirmPanelUI overlay
            BuildConfirmPanelOverlay(canvas.transform, "DeleteConfirmPanel",
                "Delete Account", "Are you sure you want to delete your account? This action cannot be undone.",
                "Delete", "Cancel");

            // Change Name InputPanelUI overlay
            BuildInputPanelOverlay(canvas.transform);
        }

        private static void BuildPremiumPanelOverlay(Transform parent)
        {
            GameObject panelRoot = new GameObject("PremiumPanel");
            panelRoot.transform.SetParent(parent, false);

            RectTransform rt = panelRoot.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Blocker background
            GameObject blocker = new GameObject("BlockerPanel");
            blocker.transform.SetParent(panelRoot.transform, false);
            RectTransform blockerRT = blocker.AddComponent<RectTransform>();
            blockerRT.anchorMin = Vector2.zero;
            blockerRT.anchorMax = Vector2.one;
            blockerRT.offsetMin = Vector2.zero;
            blockerRT.offsetMax = Vector2.zero;
            Image blockerImg = blocker.AddComponent<Image>();
            blockerImg.color = new Color(0, 0, 0, 0.85f);
            blocker.AddComponent<Button>(); // Close on tap outside

            // Panel card
            GameObject card = new GameObject("Panel");
            card.transform.SetParent(panelRoot.transform, false);
            RectTransform cardRT = card.AddComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0.05f, 0.15f);
            cardRT.anchorMax = new Vector2(0.95f, 0.85f);
            cardRT.offsetMin = Vector2.zero;
            cardRT.offsetMax = Vector2.zero;
            Image cardBg = card.AddComponent<Image>();
            cardBg.sprite = WhiteSprite;
            cardBg.color = CARD_BG;

            // Title
            GameObject titleObj = new GameObject("PremiumPanelTitle");
            titleObj.transform.SetParent(card.transform, false);
            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0.9f);
            titleRT.anchorMax = Vector2.one;
            titleRT.offsetMin = new Vector2(20, 0);
            titleRT.offsetMax = new Vector2(-20, -10);
            TextMeshProUGUI titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
            titleTxt.font = Font;
            titleTxt.text = "PREMIUM";
            titleTxt.fontSize = FontSizes.Equation;
            titleTxt.fontStyle = FontStyles.Bold;
            titleTxt.color = GOLD;
            titleTxt.alignment = TextAlignmentOptions.Center;
            titleTxt.enableAutoSizing = true;
            titleTxt.fontSizeMin = FontSizes.AutoMinBody;
            titleTxt.fontSizeMax = FontSizes.Equation;
            titleTxt.overflowMode = TextOverflowModes.Ellipsis;

            // Add PremiumPanelUI component
            var premiumComp = panelRoot.AddComponent(System.Type.GetType("DigitPark.UI.Panels.PremiumPanelUI, Assembly-CSharp"));
            if (premiumComp != null)
            {
                SerializedObject so = new SerializedObject(premiumComp);
                var panelProp = so.FindProperty("panel");
                if (panelProp != null) panelProp.objectReferenceValue = card;
                var blockerProp = so.FindProperty("blockerPanel");
                if (blockerProp != null) blockerProp.objectReferenceValue = blocker;
                var titleProp = so.FindProperty("titleText");
                if (titleProp != null) titleProp.objectReferenceValue = titleTxt;
                so.ApplyModifiedProperties();
            }

            panelRoot.SetActive(false);
        }

        private static void BuildInputPanelOverlay(Transform parent)
        {
            GameObject panelRoot = new GameObject("ChangeNamePanel");
            panelRoot.transform.SetParent(parent, false);

            RectTransform rt = panelRoot.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Blocker
            GameObject blocker = new GameObject("BlockerPanel");
            blocker.transform.SetParent(panelRoot.transform, false);
            RectTransform blockerRT = blocker.AddComponent<RectTransform>();
            blockerRT.anchorMin = Vector2.zero;
            blockerRT.anchorMax = Vector2.one;
            blockerRT.offsetMin = Vector2.zero;
            blockerRT.offsetMax = Vector2.zero;
            Image blockerImg = blocker.AddComponent<Image>();
            blockerImg.color = new Color(0, 0, 0, 0.85f);

            // Card
            GameObject card = new GameObject("Panel");
            card.transform.SetParent(panelRoot.transform, false);
            RectTransform cardRT = card.AddComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0.08f, 0.3f);
            cardRT.anchorMax = new Vector2(0.92f, 0.7f);
            cardRT.offsetMin = Vector2.zero;
            cardRT.offsetMax = Vector2.zero;
            Image cardBg = card.AddComponent<Image>();
            cardBg.sprite = WhiteSprite;
            cardBg.color = CARD_BG;

            // Title
            GameObject titleObj = new GameObject("ChangeNameTitle");
            titleObj.transform.SetParent(card.transform, false);
            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0.78f);
            titleRT.anchorMax = Vector2.one;
            titleRT.offsetMin = new Vector2(20, 0);
            titleRT.offsetMax = new Vector2(-20, -10);
            TextMeshProUGUI titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
            titleTxt.font = Font;
            titleTxt.text = "Change Name";
            titleTxt.fontSize = FontSizes.H4;
            titleTxt.fontStyle = FontStyles.Bold;
            titleTxt.color = CYAN_NEON;
            titleTxt.alignment = TextAlignmentOptions.Center;
            titleTxt.enableAutoSizing = true;
            titleTxt.fontSizeMin = FontSizes.AutoMinBody;
            titleTxt.fontSizeMax = FontSizes.H4;
            titleTxt.overflowMode = TextOverflowModes.Ellipsis;

            // InputField
            GameObject inputObj = new GameObject("InputField");
            inputObj.transform.SetParent(card.transform, false);
            RectTransform inputRT = inputObj.AddComponent<RectTransform>();
            inputRT.anchorMin = new Vector2(0.08f, 0.48f);
            inputRT.anchorMax = new Vector2(0.92f, 0.72f);
            inputRT.offsetMin = Vector2.zero;
            inputRT.offsetMax = Vector2.zero;
            Image inputBg = inputObj.AddComponent<Image>();
            inputBg.sprite = WhiteSprite;
            inputBg.color = DROPDOWN_BG;

            // TMP InputField TextArea
            GameObject textArea = new GameObject("Text Area");
            textArea.transform.SetParent(inputObj.transform, false);
            RectTransform textAreaRT = textArea.AddComponent<RectTransform>();
            textAreaRT.anchorMin = Vector2.zero;
            textAreaRT.anchorMax = Vector2.one;
            textAreaRT.offsetMin = new Vector2(12, 4);
            textAreaRT.offsetMax = new Vector2(-12, -4);
            textArea.AddComponent<RectMask2D>();

            GameObject placeholder = new GameObject("Placeholder");
            placeholder.transform.SetParent(textArea.transform, false);
            RectTransform placeholderRT = placeholder.AddComponent<RectTransform>();
            placeholderRT.anchorMin = Vector2.zero;
            placeholderRT.anchorMax = Vector2.one;
            placeholderRT.offsetMin = Vector2.zero;
            placeholderRT.offsetMax = Vector2.zero;
            TextMeshProUGUI placeholderTxt = placeholder.AddComponent<TextMeshProUGUI>();
            placeholderTxt.font = Font;
            placeholderTxt.text = "New name...";
            placeholderTxt.fontSize = FontSizes.H3;
            placeholderTxt.fontStyle = FontStyles.Bold;
            placeholderTxt.color = TEXT_GRAY;
            placeholderTxt.alignment = TextAlignmentOptions.Left;
            placeholderTxt.enableAutoSizing = true;
            placeholderTxt.fontSizeMin = FontSizes.AutoMinBody;
            placeholderTxt.fontSizeMax = FontSizes.H3;
            placeholderTxt.overflowMode = TextOverflowModes.Ellipsis;

            GameObject inputText = new GameObject("Text");
            inputText.transform.SetParent(textArea.transform, false);
            RectTransform inputTextRT = inputText.AddComponent<RectTransform>();
            inputTextRT.anchorMin = Vector2.zero;
            inputTextRT.anchorMax = Vector2.one;
            inputTextRT.offsetMin = Vector2.zero;
            inputTextRT.offsetMax = Vector2.zero;
            TextMeshProUGUI inputTxtComp = inputText.AddComponent<TextMeshProUGUI>();
            inputTxtComp.font = Font;
            inputTxtComp.fontSize = FontSizes.H3;
            inputTxtComp.enableAutoSizing = true;
            inputTxtComp.fontSizeMin = FontSizes.AutoMinTitle;
            inputTxtComp.fontSizeMax = inputTxtComp.fontSize;
            inputTxtComp.fontStyle = FontStyles.Bold;
            inputTxtComp.color = TEXT_WHITE;
            inputTxtComp.alignment = TextAlignmentOptions.Left;

            TMP_InputField tmpInput = inputObj.AddComponent<TMP_InputField>();
            tmpInput.textViewport = textAreaRT;
            tmpInput.textComponent = inputTxtComp;
            tmpInput.placeholder = placeholderTxt;
            tmpInput.fontAsset = Font;
            tmpInput.pointSize = FontSizes.H3;
            tmpInput.characterLimit = 20;

            // Confirm Button
            GameObject confirmObj = new GameObject("ConfirmButton");
            confirmObj.transform.SetParent(card.transform, false);
            RectTransform confirmBtnRT = confirmObj.AddComponent<RectTransform>();
            confirmBtnRT.anchorMin = new Vector2(0.55f, 0.08f);
            confirmBtnRT.anchorMax = new Vector2(0.92f, 0.35f);
            confirmBtnRT.offsetMin = Vector2.zero;
            confirmBtnRT.offsetMax = Vector2.zero;
            Image confirmBg = confirmObj.AddComponent<Image>();
            confirmBg.sprite = WhiteSprite;
            confirmBg.color = CYAN_NEON;
            Button confirmBtn = confirmObj.AddComponent<Button>();
            confirmBtn.targetGraphic = confirmBg;

            GameObject confirmTxtObj = new GameObject("ConfirmButtonText");
            confirmTxtObj.transform.SetParent(confirmObj.transform, false);
            RectTransform cTxtRT = confirmTxtObj.AddComponent<RectTransform>();
            cTxtRT.anchorMin = Vector2.zero;
            cTxtRT.anchorMax = Vector2.one;
            cTxtRT.offsetMin = Vector2.zero;
            cTxtRT.offsetMax = Vector2.zero;
            TextMeshProUGUI confirmTxtComp = confirmTxtObj.AddComponent<TextMeshProUGUI>();
            confirmTxtComp.font = Font;
            confirmTxtComp.text = "Confirm";
            confirmTxtComp.fontSize = FontSizes.H3;
            confirmTxtComp.fontStyle = FontStyles.Bold;
            confirmTxtComp.color = DARK_NAVY;
            confirmTxtComp.alignment = TextAlignmentOptions.Center;
            confirmTxtComp.enableAutoSizing = true;
            confirmTxtComp.fontSizeMin = FontSizes.AutoMinBody;
            confirmTxtComp.fontSizeMax = FontSizes.H3;
            confirmTxtComp.overflowMode = TextOverflowModes.Ellipsis;

            // Cancel Button
            GameObject cancelObj = new GameObject("CancelButton");
            cancelObj.transform.SetParent(card.transform, false);
            RectTransform cancelBtnRT = cancelObj.AddComponent<RectTransform>();
            cancelBtnRT.anchorMin = new Vector2(0.08f, 0.08f);
            cancelBtnRT.anchorMax = new Vector2(0.45f, 0.35f);
            cancelBtnRT.offsetMin = Vector2.zero;
            cancelBtnRT.offsetMax = Vector2.zero;
            Image cancelBg = cancelObj.AddComponent<Image>();
            cancelBg.sprite = WhiteSprite;
            cancelBg.color = BUTTON_BG;
            Button cancelBtn = cancelObj.AddComponent<Button>();
            cancelBtn.targetGraphic = cancelBg;

            GameObject cancelTxtObj = new GameObject("CancelButtonText");
            cancelTxtObj.transform.SetParent(cancelObj.transform, false);
            RectTransform ccTxtRT = cancelTxtObj.AddComponent<RectTransform>();
            ccTxtRT.anchorMin = Vector2.zero;
            ccTxtRT.anchorMax = Vector2.one;
            ccTxtRT.offsetMin = Vector2.zero;
            ccTxtRT.offsetMax = Vector2.zero;
            TextMeshProUGUI cancelTxtComp = cancelTxtObj.AddComponent<TextMeshProUGUI>();
            cancelTxtComp.font = Font;
            cancelTxtComp.text = "Cancel";
            cancelTxtComp.fontSize = FontSizes.H3;
            cancelTxtComp.fontStyle = FontStyles.Bold;
            cancelTxtComp.color = TEXT_GRAY;
            cancelTxtComp.alignment = TextAlignmentOptions.Center;
            cancelTxtComp.enableAutoSizing = true;
            cancelTxtComp.fontSizeMin = FontSizes.AutoMinBody;
            cancelTxtComp.fontSizeMax = FontSizes.H3;
            cancelTxtComp.overflowMode = TextOverflowModes.Ellipsis;

            // Add InputPanelUI component
            var inputComp = panelRoot.AddComponent(System.Type.GetType("DigitPark.UI.Panels.InputPanelUI, Assembly-CSharp"));
            if (inputComp != null)
            {
                SerializedObject so = new SerializedObject(inputComp);
                var pp = so.FindProperty("panel"); if (pp != null) pp.objectReferenceValue = card;
                var bp = so.FindProperty("blockerPanel"); if (bp != null) bp.objectReferenceValue = blocker;
                var tp = so.FindProperty("titleText"); if (tp != null) tp.objectReferenceValue = titleTxt;
                var ip = so.FindProperty("inputField"); if (ip != null) ip.objectReferenceValue = tmpInput;
                var cb = so.FindProperty("confirmButton"); if (cb != null) cb.objectReferenceValue = confirmBtn;
                var ccb = so.FindProperty("cancelButton"); if (ccb != null) ccb.objectReferenceValue = cancelBtn;
                var ct = so.FindProperty("confirmButtonText"); if (ct != null) ct.objectReferenceValue = confirmTxtComp;
                var cct = so.FindProperty("cancelButtonText"); if (cct != null) cct.objectReferenceValue = cancelTxtComp;
                so.ApplyModifiedProperties();
            }

            panelRoot.SetActive(false);
        }

        private static void BuildConfirmPanelOverlay(Transform parent, string name,
            string title, string message, string confirmText, string cancelText)
        {
            GameObject panelRoot = new GameObject(name);
            panelRoot.transform.SetParent(parent, false);

            RectTransform rt = panelRoot.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Blocker
            GameObject blocker = new GameObject("BlockerPanel");
            blocker.transform.SetParent(panelRoot.transform, false);
            RectTransform blockerRT = blocker.AddComponent<RectTransform>();
            blockerRT.anchorMin = Vector2.zero;
            blockerRT.anchorMax = Vector2.one;
            blockerRT.offsetMin = Vector2.zero;
            blockerRT.offsetMax = Vector2.zero;
            Image blockerImg = blocker.AddComponent<Image>();
            blockerImg.color = new Color(0, 0, 0, 0.85f);

            // Card (compact confirm dialog)
            GameObject card = new GameObject("Panel");
            card.transform.SetParent(panelRoot.transform, false);
            RectTransform cardRT = card.AddComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0.06f, 0.28f);
            cardRT.anchorMax = new Vector2(0.94f, 0.68f);
            cardRT.offsetMin = Vector2.zero;
            cardRT.offsetMax = Vector2.zero;
            Image cardBg = card.AddComponent<Image>();
            cardBg.sprite = WhiteSprite;
            cardBg.color = CARD_BG;

            // Title
            GameObject titleObj = new GameObject("ConfirmTitleLabel");
            titleObj.transform.SetParent(card.transform, false);
            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0.78f);
            titleRT.anchorMax = Vector2.one;
            titleRT.offsetMin = new Vector2(20, 0);
            titleRT.offsetMax = new Vector2(-20, -8);
            TextMeshProUGUI titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
            titleTxt.font = Font;
            titleTxt.text = title;
            titleTxt.fontSize = FontSizes.H3;
            titleTxt.enableAutoSizing = true;
            titleTxt.fontSizeMin = FontSizes.Caption;
            titleTxt.fontSizeMax = FontSizes.H3;
            titleTxt.fontStyle = FontStyles.Bold;
            titleTxt.color = DANGER_RED;
            titleTxt.alignment = TextAlignmentOptions.Center;

            // Message
            GameObject msgObj = new GameObject("ConfirmMessageLabel");
            msgObj.transform.SetParent(card.transform, false);
            RectTransform msgRT = msgObj.AddComponent<RectTransform>();
            msgRT.anchorMin = new Vector2(0, 0.22f);
            msgRT.anchorMax = new Vector2(1, 0.78f);
            msgRT.offsetMin = new Vector2(24, 0);
            msgRT.offsetMax = new Vector2(-24, 0);
            TextMeshProUGUI msgTxt = msgObj.AddComponent<TextMeshProUGUI>();
            msgTxt.font = Font;
            msgTxt.text = message;
            msgTxt.fontSize = FontSizes.H4;
            msgTxt.enableAutoSizing = true;
            msgTxt.fontSizeMin = FontSizes.Body;
            msgTxt.fontSizeMax = FontSizes.H4;
            msgTxt.fontStyle = FontStyles.Bold;
            msgTxt.color = TEXT_WHITE;
            msgTxt.alignment = TextAlignmentOptions.Center;
            msgTxt.enableWordWrapping = true;

            // Confirm Button
            GameObject confirmObj = new GameObject("ConfirmButton");
            confirmObj.transform.SetParent(card.transform, false);
            RectTransform confirmRT = confirmObj.AddComponent<RectTransform>();
            confirmRT.anchorMin = new Vector2(0.55f, 0.05f);
            confirmRT.anchorMax = new Vector2(0.92f, 0.22f);
            confirmRT.offsetMin = Vector2.zero;
            confirmRT.offsetMax = Vector2.zero;
            Image confirmBg = confirmObj.AddComponent<Image>();
            confirmBg.sprite = WhiteSprite;
            confirmBg.color = DANGER_RED;
            Button confirmBtn = confirmObj.AddComponent<Button>();
            confirmBtn.targetGraphic = confirmBg;

            GameObject confirmTxtObj = new GameObject("ConfirmButtonText");
            confirmTxtObj.transform.SetParent(confirmObj.transform, false);
            RectTransform confirmTxtRT = confirmTxtObj.AddComponent<RectTransform>();
            confirmTxtRT.anchorMin = Vector2.zero;
            confirmTxtRT.anchorMax = Vector2.one;
            confirmTxtRT.offsetMin = Vector2.zero;
            confirmTxtRT.offsetMax = Vector2.zero;
            TextMeshProUGUI confirmTxtComp = confirmTxtObj.AddComponent<TextMeshProUGUI>();
            confirmTxtComp.font = Font;
            confirmTxtComp.text = confirmText;
            confirmTxtComp.fontSize = FontSizes.Body;
            confirmTxtComp.enableAutoSizing = true;
            confirmTxtComp.fontSizeMin = FontSizes.Body;
            confirmTxtComp.fontSizeMax = FontSizes.Body;
            confirmTxtComp.fontStyle = FontStyles.Bold;
            confirmTxtComp.color = TEXT_WHITE;
            confirmTxtComp.alignment = TextAlignmentOptions.Center;

            // Cancel Button
            GameObject cancelObj = new GameObject("CancelButton");
            cancelObj.transform.SetParent(card.transform, false);
            RectTransform cancelRT = cancelObj.AddComponent<RectTransform>();
            cancelRT.anchorMin = new Vector2(0.08f, 0.05f);
            cancelRT.anchorMax = new Vector2(0.45f, 0.22f);
            cancelRT.offsetMin = Vector2.zero;
            cancelRT.offsetMax = Vector2.zero;
            Image cancelBg = cancelObj.AddComponent<Image>();
            cancelBg.sprite = WhiteSprite;
            cancelBg.color = BUTTON_BG;
            Button cancelBtn = cancelObj.AddComponent<Button>();
            cancelBtn.targetGraphic = cancelBg;

            GameObject cancelTxtObj = new GameObject("CancelButtonText");
            cancelTxtObj.transform.SetParent(cancelObj.transform, false);
            RectTransform cancelTxtRT = cancelTxtObj.AddComponent<RectTransform>();
            cancelTxtRT.anchorMin = Vector2.zero;
            cancelTxtRT.anchorMax = Vector2.one;
            cancelTxtRT.offsetMin = Vector2.zero;
            cancelTxtRT.offsetMax = Vector2.zero;
            TextMeshProUGUI cancelTxtComp = cancelTxtObj.AddComponent<TextMeshProUGUI>();
            cancelTxtComp.font = Font;
            cancelTxtComp.text = cancelText;
            cancelTxtComp.fontSize = FontSizes.Body;
            cancelTxtComp.enableAutoSizing = true;
            cancelTxtComp.fontSizeMin = FontSizes.AutoMinBody;
            cancelTxtComp.fontSizeMax = FontSizes.Body;
            cancelTxtComp.fontStyle = FontStyles.Bold;
            cancelTxtComp.color = TEXT_GRAY;
            cancelTxtComp.alignment = TextAlignmentOptions.Center;

            // Add ConfirmPanelUI component
            var confirmComp = panelRoot.AddComponent(System.Type.GetType("DigitPark.UI.Panels.ConfirmPanelUI, Assembly-CSharp"));
            if (confirmComp != null)
            {
                SerializedObject so = new SerializedObject(confirmComp);
                var pp = so.FindProperty("panel"); if (pp != null) pp.objectReferenceValue = card;
                var bp = so.FindProperty("blockerPanel"); if (bp != null) bp.objectReferenceValue = blocker;
                var tp = so.FindProperty("titleText"); if (tp != null) tp.objectReferenceValue = titleTxt;
                var mp = so.FindProperty("messageText"); if (mp != null) mp.objectReferenceValue = msgTxt;
                var cb = so.FindProperty("confirmButton"); if (cb != null) cb.objectReferenceValue = confirmBtn;
                var ccb = so.FindProperty("cancelButton"); if (ccb != null) ccb.objectReferenceValue = cancelBtn;
                var ct = so.FindProperty("confirmButtonText"); if (ct != null) ct.objectReferenceValue = confirmTxtComp;
                var cct = so.FindProperty("cancelButtonText"); if (cct != null) cct.objectReferenceValue = cancelTxtComp;
                so.ApplyModifiedProperties();
            }

            panelRoot.SetActive(false);
        }

        private static void BuildVersionFooter(Transform parent)
        {
            GameObject versionObj = new GameObject("VersionText");
            versionObj.transform.SetParent(parent, false);

            LayoutElement layout = versionObj.AddComponent<LayoutElement>();
            layout.preferredHeight = 50;

            TextMeshProUGUI text = versionObj.AddComponent<TextMeshProUGUI>();
            text.font = Font;
            text.text = "v1.0.0 - Digit Park";
            text.fontSize = FontSizes.H4;
            text.fontStyle = FontStyles.Bold;
            text.color = TEXT_GRAY;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            text.enableAutoSizing = true;
            text.fontSizeMin = FontSizes.AutoMinBody;
            text.fontSizeMax = FontSizes.H4;
            text.overflowMode = TextOverflowModes.Ellipsis;
        }

        // ==================== CARD CREATION ====================

        private static Transform CreateCard(Transform parent, string name, string title, Color borderColor)
        {
            // Card wrapper (auto-sizes height)
            GameObject cardObj = new GameObject(name);
            cardObj.transform.SetParent(parent, false);

            ContentSizeFitter csf = cardObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Card background with border effect
            Image cardBg = cardObj.AddComponent<Image>();
            cardBg.sprite = WhiteSprite;
            cardBg.color = CARD_BG;
            cardBg.raycastTarget = false;

            Outline border = cardObj.AddComponent<Outline>();
            border.effectColor = borderColor;
            border.effectDistance = new Vector2(1, -1);

            // Internal layout
            VerticalLayoutGroup vlg = cardObj.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 0;
            vlg.padding = new RectOffset((int)CARD_INNER_PAD, (int)CARD_INNER_PAD, (int)CARD_INNER_PAD, (int)CARD_INNER_PAD);

            // Section title - name derived from card for AutoLocalizer matching
            CreateSectionTitle(cardObj.transform, $"{name}Title", title, borderColor == DANGER_BORDER ? DANGER_RED : CYAN_NEON);

            return cardObj.transform;
        }

        private static void CreateSectionTitle(Transform parent, string objName, string text, Color color)
        {
            GameObject titleObj = new GameObject(objName);
            titleObj.transform.SetParent(parent, false);

            LayoutElement layout = titleObj.AddComponent<LayoutElement>();
            layout.preferredHeight = SECTION_TITLE_HEIGHT;

            TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
            title.font = Font;
            title.text = text;
            title.fontSize = FontSizes.H3;
            title.fontStyle = FontStyles.Bold;
            title.color = color;
            title.alignment = TextAlignmentOptions.Left;
            title.characterSpacing = 4;
            title.raycastTarget = false;
            title.enableAutoSizing = true;
            title.fontSizeMin = FontSizes.AutoMinBody;
            title.fontSizeMax = FontSizes.H3;
            title.overflowMode = TextOverflowModes.Ellipsis;
        }

        // ==================== ROW ELEMENTS ====================

        /// <summary>
        /// Creates a clickable settings row: [Label text ........... rightText >]
        /// </summary>
        private static void CreateSettingsRow(Transform parent, string buttonName, string labelText,
            string rightText, Color rightColor, bool isButton, string rightTextName = null)
        {
            GameObject row = new GameObject(buttonName);
            row.transform.SetParent(parent, false);

            LayoutElement layout = row.AddComponent<LayoutElement>();
            layout.preferredHeight = ROW_HEIGHT;

            // Background (subtle, for button feedback)
            Image bg = row.AddComponent<Image>();
            bg.sprite = WhiteSprite;
            bg.color = BUTTON_BG;
            bg.raycastTarget = true;

            if (isButton)
            {
                Button btn = row.AddComponent<Button>();
                btn.targetGraphic = bg;

                ColorBlock cb = btn.colors;
                cb.normalColor = Color.white;
                cb.highlightedColor = new Color(1, 1, 1, 1.2f);
                cb.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
                btn.colors = cb;
            }

            // Label text (left) - name matches AutoLocalizer TextNameToKeyMap
            GameObject labelObj = new GameObject($"{buttonName}Text");
            labelObj.transform.SetParent(row.transform, false);

            RectTransform labelRT = labelObj.AddComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = new Vector2(16, 0);
            labelRT.offsetMax = new Vector2(-80, 0);

            TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
            label.font = Font;
            label.text = labelText;
            label.fontSize = FontSizes.H3;
            label.fontStyle = FontStyles.Bold;
            label.color = TEXT_WHITE;
            label.alignment = TextAlignmentOptions.Left;
            label.raycastTarget = false;
            label.enableAutoSizing = true;
            label.fontSizeMin = FontSizes.AutoMinBody;
            label.fontSizeMax = FontSizes.H3;
            label.overflowMode = TextOverflowModes.Ellipsis;

            // Right text (value/arrow)
            if (!string.IsNullOrEmpty(rightText))
            {
                string rName = !string.IsNullOrEmpty(rightTextName) ? rightTextName : "RightText";
                GameObject rightObj = new GameObject(rName);
                rightObj.transform.SetParent(row.transform, false);

                RectTransform rightRT = rightObj.AddComponent<RectTransform>();
                rightRT.anchorMin = new Vector2(0.6f, 0);
                rightRT.anchorMax = Vector2.one;
                rightRT.offsetMin = new Vector2(0, 0);
                rightRT.offsetMax = new Vector2(-16, 0);

                TextMeshProUGUI right = rightObj.AddComponent<TextMeshProUGUI>();
                right.font = Font;
                right.text = rightText;
                right.fontSize = FontSizes.H3;
                right.fontStyle = FontStyles.Bold;
                right.color = rightColor;
                right.alignment = TextAlignmentOptions.Right;
                right.raycastTarget = false;
                right.enableAutoSizing = true;
                right.fontSizeMin = FontSizes.AutoMinBody;
                right.fontSizeMax = FontSizes.H3;
                right.overflowMode = TextOverflowModes.Ellipsis;
            }
        }

        /// <summary>
        /// Creates danger zone full-width button
        /// </summary>
        private static void CreateDangerButton(Transform parent, string name, string text, Color textColor, Color bgColor)
        {
            GameObject row = new GameObject(name);
            row.transform.SetParent(parent, false);

            LayoutElement layout = row.AddComponent<LayoutElement>();
            layout.preferredHeight = ROW_HEIGHT;

            Image bg = row.AddComponent<Image>();
            bg.sprite = WhiteSprite;
            bg.color = bgColor;

            Button btn = row.AddComponent<Button>();
            btn.targetGraphic = bg;

            Outline outline = row.AddComponent<Outline>();
            outline.effectColor = textColor * 0.5f;
            outline.effectDistance = new Vector2(1, -1);

            TextMeshProUGUI label = CreateTextChild(row.transform, $"{name}Text", text, FontSizes.H2, textColor, TextAlignmentOptions.Center);
            label.fontStyle = FontStyles.Bold;
        }

        /// <summary>
        /// Creates Premium PRO button row with badge
        /// </summary>
        private static void CreatePremiumProRow(Transform parent)
        {
            GameObject row = new GameObject("PremiumButton");
            row.transform.SetParent(parent, false);

            LayoutElement layout = row.AddComponent<LayoutElement>();
            layout.preferredHeight = ROW_HEIGHT;

            Image bg = row.AddComponent<Image>();
            bg.sprite = WhiteSprite;
            bg.color = new Color(0.15f, 0.13f, 0.05f, 0.6f);

            Button btn = row.AddComponent<Button>();
            btn.targetGraphic = bg;

            Outline outline = row.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.84f, 0f, 0.3f);
            outline.effectDistance = new Vector2(1, -1);

            // Star + PRO text
            TextMeshProUGUI label = CreateTextChild(row.transform, "Text", "PRO", FontSizes.H1, GOLD, TextAlignmentOptions.Left);
            label.fontStyle = FontStyles.Bold;
            RectTransform labelRT = label.GetComponent<RectTransform>();
            labelRT.offsetMin = new Vector2(16, 0);

            // Badge
            GameObject badge = new GameObject("PremiumBadge");
            badge.transform.SetParent(row.transform, false);

            RectTransform badgeRT = badge.AddComponent<RectTransform>();
            badgeRT.anchorMin = new Vector2(1, 0.5f);
            badgeRT.anchorMax = new Vector2(1, 0.5f);
            badgeRT.pivot = new Vector2(1, 0.5f);
            badgeRT.sizeDelta = new Vector2(80, 28);
            badgeRT.anchoredPosition = new Vector2(-16, 0);

            Image badgeBg = badge.AddComponent<Image>();
            badgeBg.sprite = WhiteSprite;
            badgeBg.color = GOLD;

            TextMeshProUGUI badgeText = CreateTextChild(badge.transform, "BadgeText", "ACTIVE", FontSizes.BodyLarge, DARK_NAVY, TextAlignmentOptions.Center);
            badgeText.fontStyle = FontStyles.Bold;

            badge.SetActive(false); // Hidden by default, shown when premium is active
        }

        /// <summary>
        /// Creates Player ID row with copy button
        /// </summary>
        private static void CreatePlayerIDRow(Transform parent)
        {
            GameObject row = new GameObject("PlayerIDContainer");
            row.transform.SetParent(parent, false);

            LayoutElement layout = row.AddComponent<LayoutElement>();
            layout.preferredHeight = ROW_HEIGHT;

            Image bg = row.AddComponent<Image>();
            bg.sprite = WhiteSprite;
            bg.color = BUTTON_BG;
            bg.raycastTarget = false;

            // Label "ID:" - narrow, secondary gray
            GameObject labelObj = new GameObject("IDLabel");
            labelObj.transform.SetParent(row.transform, false);

            RectTransform labelRT = labelObj.AddComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 0);
            labelRT.anchorMax = new Vector2(0.12f, 1);
            labelRT.offsetMin = new Vector2(16, 0);
            labelRT.offsetMax = Vector2.zero;

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.font = Font;
            labelText.text = "ID:";
            labelText.fontSize = 42f;
            labelText.fontStyle = FontStyles.Bold;
            labelText.color = TEXT_GRAY;
            labelText.alignment = TextAlignmentOptions.Left;
            labelText.raycastTarget = false;
            labelText.enableAutoSizing = true;
            labelText.fontSizeMin = FontSizes.AutoMinBody;
            labelText.fontSizeMax = 42f;
            labelText.overflowMode = TextOverflowModes.Ellipsis;

            // ID Value - sits right next to label, muted gray (secondary data)
            GameObject idObj = new GameObject("IDText");
            idObj.transform.SetParent(row.transform, false);

            RectTransform idRT = idObj.AddComponent<RectTransform>();
            idRT.anchorMin = new Vector2(0.12f, 0);
            idRT.anchorMax = new Vector2(0.73f, 1);
            idRT.offsetMin = Vector2.zero;
            idRT.offsetMax = Vector2.zero;

            TextMeshProUGUI idText = idObj.AddComponent<TextMeshProUGUI>();
            idText.font = Font;
            idText.text = "#ABC123XYZ";
            idText.fontSize = 42f;
            idText.fontStyle = FontStyles.Bold;
            idText.enableAutoSizing = true;
            idText.fontSizeMin = FontSizes.AutoMinBody;
            idText.fontSizeMax = idText.fontSize;
            idText.color = TEXT_GRAY;             // muted: ID is secondary info
            idText.alignment = TextAlignmentOptions.Left; // flush with "ID:" label
            idText.raycastTarget = false;

            // Copy button
            GameObject copyBtn = new GameObject("CopyButton");
            copyBtn.transform.SetParent(row.transform, false);

            RectTransform copyRT = copyBtn.AddComponent<RectTransform>();
            copyRT.anchorMin = new Vector2(0.74f, 0.15f);
            copyRT.anchorMax = new Vector2(1.0f, 0.85f);
            copyRT.offsetMin = Vector2.zero;
            copyRT.offsetMax = new Vector2(-16, 0); // aligned with separator margin

            Image copyBg = copyBtn.AddComponent<Image>();
            copyBg.sprite = WhiteSprite;
            copyBg.color = CYAN_NEON;

            Button copyButton = copyBtn.AddComponent<Button>();
            copyButton.targetGraphic = copyBg;

            TextMeshProUGUI copyText = CreateTextChild(copyBtn.transform, "CopyButtonText", "Copy", FontSizes.Subtitle, DARK_NAVY, TextAlignmentOptions.Center);
            copyText.fontStyle = FontStyles.Bold;
        }

        // ==================== SLIDER ====================

        private static void CreateSliderRow(Transform parent, string sliderName, string valueName, string label, float defaultValue)
        {
            GameObject container = new GameObject($"{sliderName}Container");
            container.transform.SetParent(parent, false);

            LayoutElement containerLayout = container.AddComponent<LayoutElement>();
            containerLayout.preferredHeight = SLIDER_ROW_HEIGHT;

            Image containerBg = container.AddComponent<Image>();
            containerBg.sprite = WhiteSprite;
            containerBg.color = BUTTON_BG;
            containerBg.raycastTarget = false;

            // Label (top-left area) - name matches AutoLocalizer TextNameToKeyMap
            GameObject labelObj = new GameObject($"{sliderName}Label");
            labelObj.transform.SetParent(container.transform, false);

            RectTransform labelRT = labelObj.AddComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 0.55f);
            labelRT.anchorMax = new Vector2(0.5f, 1);
            labelRT.offsetMin = new Vector2(16, 0);
            labelRT.offsetMax = Vector2.zero;

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.font = Font;
            labelText.text = label;
            labelText.fontSize = FontSizes.H3;
            labelText.enableAutoSizing = true;
            labelText.fontSizeMin = FontSizes.Caption;
            labelText.fontSizeMax = FontSizes.H3;
            labelText.fontStyle = FontStyles.Bold;
            labelText.color = TEXT_WHITE;
            labelText.alignment = TextAlignmentOptions.Left;
            labelText.raycastTarget = false;

            // Value text (top-right)
            GameObject valueObj = new GameObject(valueName);
            valueObj.transform.SetParent(container.transform, false);

            RectTransform valueRT = valueObj.AddComponent<RectTransform>();
            valueRT.anchorMin = new Vector2(0.5f, 0.55f);
            valueRT.anchorMax = new Vector2(1, 1);
            valueRT.offsetMin = Vector2.zero;
            valueRT.offsetMax = new Vector2(-16, 0);

            TextMeshProUGUI valueText = valueObj.AddComponent<TextMeshProUGUI>();
            valueText.font = Font;
            valueText.text = $"{Mathf.RoundToInt(defaultValue * 100)}%";
            valueText.fontSize = FontSizes.H3;
            valueText.fontStyle = FontStyles.Bold;
            valueText.color = CYAN_NEON;
            valueText.alignment = TextAlignmentOptions.Right;
            valueText.raycastTarget = false;
            valueText.enableAutoSizing = true;
            valueText.fontSizeMin = FontSizes.AutoMinBody;
            valueText.fontSizeMax = FontSizes.H3;
            valueText.overflowMode = TextOverflowModes.Ellipsis;

            // Slider (bottom row)
            GameObject sliderObj = new GameObject(sliderName);
            sliderObj.transform.SetParent(container.transform, false);

            RectTransform sliderRT = sliderObj.AddComponent<RectTransform>();
            sliderRT.anchorMin = new Vector2(0, 0);
            sliderRT.anchorMax = new Vector2(1, 0.50f);
            sliderRT.offsetMin = new Vector2(16, 8);
            sliderRT.offsetMax = new Vector2(-16, -4);

            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = defaultValue;

            // Track background
            GameObject trackBg = new GameObject("Background");
            trackBg.transform.SetParent(sliderObj.transform, false);

            RectTransform trackBgRT = trackBg.AddComponent<RectTransform>();
            trackBgRT.anchorMin = new Vector2(0, 0.5f);
            trackBgRT.anchorMax = new Vector2(1, 0.5f);
            trackBgRT.sizeDelta = new Vector2(0, 8);

            Image trackBgImg = trackBg.AddComponent<Image>();
            trackBgImg.sprite = WhiteSprite;
            trackBgImg.color = SLIDER_TRACK;

            // Fill Area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);

            RectTransform fillAreaRT = fillArea.AddComponent<RectTransform>();
            fillAreaRT.anchorMin = new Vector2(0, 0.5f);
            fillAreaRT.anchorMax = new Vector2(1, 0.5f);
            fillAreaRT.sizeDelta = new Vector2(-20, 8);

            // Fill
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);

            RectTransform fillRT = fill.AddComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.sizeDelta = Vector2.zero;

            Image fillImg = fill.AddComponent<Image>();
            fillImg.sprite = WhiteSprite;
            fillImg.color = CYAN_NEON;

            slider.fillRect = fillRT;

            // Handle Slide Area
            GameObject handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(sliderObj.transform, false);

            RectTransform handleAreaRT = handleArea.AddComponent<RectTransform>();
            handleAreaRT.anchorMin = Vector2.zero;
            handleAreaRT.anchorMax = Vector2.one;
            handleAreaRT.sizeDelta = new Vector2(-16, 0);

            // Handle
            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);

            RectTransform handleRT = handle.AddComponent<RectTransform>();
            handleRT.sizeDelta = new Vector2(24, 24);

            Image handleImg = handle.AddComponent<Image>();
            handleImg.sprite = WhiteSprite;
            handleImg.color = CYAN_NEON;

            slider.handleRect = handleRT;
            slider.targetGraphic = handleImg;
        }

        // ==================== TOGGLE ====================

        private static void CreateToggleRow(Transform parent, string name, string label, bool defaultValue)
        {
            GameObject container = new GameObject($"{name}Container");
            container.transform.SetParent(parent, false);

            LayoutElement containerLayout = container.AddComponent<LayoutElement>();
            containerLayout.preferredHeight = ROW_HEIGHT;

            Image containerBg = container.AddComponent<Image>();
            containerBg.sprite = WhiteSprite;
            containerBg.color = BUTTON_BG;
            containerBg.raycastTarget = false;

            // Label - name matches AutoLocalizer TextNameToKeyMap
            GameObject labelObj = new GameObject($"{name}Label");
            labelObj.transform.SetParent(container.transform, false);

            RectTransform labelRT = labelObj.AddComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = new Vector2(0.65f, 1);
            labelRT.offsetMin = new Vector2(16, 0);
            labelRT.offsetMax = Vector2.zero;

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.font = Font;
            labelText.text = label;
            labelText.fontSize = FontSizes.H3;
            labelText.enableAutoSizing = true;
            labelText.fontSizeMin = FontSizes.Caption;
            labelText.fontSizeMax = FontSizes.H3;
            labelText.fontStyle = FontStyles.Bold;
            labelText.color = TEXT_WHITE;
            labelText.alignment = TextAlignmentOptions.Left;
            labelText.raycastTarget = false;

            // Toggle switch
            GameObject toggleObj = new GameObject(name);
            toggleObj.transform.SetParent(container.transform, false);

            RectTransform toggleRT = toggleObj.AddComponent<RectTransform>();
            toggleRT.anchorMin = new Vector2(1, 0.5f);
            toggleRT.anchorMax = new Vector2(1, 0.5f);
            toggleRT.pivot = new Vector2(1, 0.5f);
            toggleRT.sizeDelta = new Vector2(150, 44); // wide enough for translated text (ACTIVADO/DÉSACTIVÉ/AKTIVIERT)
            toggleRT.anchoredPosition = new Vector2(-16, 0);

            Image toggleBg = toggleObj.AddComponent<Image>();
            toggleBg.sprite = WhiteSprite;
            toggleBg.color = defaultValue ? CYAN_NEON : TOGGLE_OFF_BG;

            Toggle toggle = toggleObj.AddComponent<Toggle>();
            toggle.isOn = defaultValue;
            toggle.targetGraphic = toggleBg;

            // ON/OFF text - named uniquely so auto-assigner finds THIS text, not the row label
            GameObject checkmark = new GameObject($"{name}Text");
            checkmark.transform.SetParent(toggleObj.transform, false);

            RectTransform checkRT = checkmark.AddComponent<RectTransform>();
            checkRT.anchorMin = Vector2.zero;
            checkRT.anchorMax = Vector2.one;
            checkRT.offsetMin = new Vector2(6, 0);
            checkRT.offsetMax = new Vector2(-6, 0);

            TextMeshProUGUI checkText = checkmark.AddComponent<TextMeshProUGUI>();
            checkText.font = Font;
            checkText.text = defaultValue ? "ON" : "OFF";
            checkText.fontSize = FontSizes.Subtitle;
            checkText.enableAutoSizing = true;
            checkText.fontSizeMin = FontSizes.AutoMinBody;
            checkText.fontSizeMax = FontSizes.Subtitle;
            checkText.fontStyle = FontStyles.Bold;
            checkText.color = defaultValue ? DARK_NAVY : TEXT_GRAY;
            checkText.alignment = TextAlignmentOptions.Center;
            checkText.raycastTarget = false;

            // Do NOT set toggle.graphic = text: Unity hides the graphic when isOn=false,
            // causing "OFF" label to disappear. Text is managed by SettingsManager at runtime.
        }

        // ==================== DROPDOWN ====================

        private static void CreateDropdownRow(Transform parent, string dropdownName, string labelName,
            string labelText, string[] options, int defaultIndex)
        {
            GameObject container = new GameObject($"{dropdownName}Container");
            container.transform.SetParent(parent, false);

            LayoutElement containerLayout = container.AddComponent<LayoutElement>();
            containerLayout.preferredHeight = ROW_HEIGHT;

            Image containerBg = container.AddComponent<Image>();
            containerBg.sprite = WhiteSprite;
            containerBg.color = BUTTON_BG;
            containerBg.raycastTarget = false;

            // Label (left)
            GameObject labelObj = new GameObject(labelName);
            labelObj.transform.SetParent(container.transform, false);

            RectTransform labelRT = labelObj.AddComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 0.05f);
            labelRT.anchorMax = new Vector2(0.38f, 0.95f);
            labelRT.offsetMin = new Vector2(16, 0);
            labelRT.offsetMax = Vector2.zero;

            TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
            label.font = Font;
            label.text = labelText;
            label.fontSize = FontSizes.H3;
            label.fontStyle = FontStyles.Bold;
            label.fontSizeMin = FontSizes.AutoMinBody;
            label.fontSizeMax = FontSizes.H3;
            label.enableAutoSizing = true;
            label.color = TEXT_WHITE;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.raycastTarget = false;
            label.overflowMode = TextOverflowModes.Ellipsis;

            // Dropdown (right)
            GameObject dropdownObj = new GameObject(dropdownName);
            dropdownObj.transform.SetParent(container.transform, false);

            RectTransform dropdownRT = dropdownObj.AddComponent<RectTransform>();
            dropdownRT.anchorMin = new Vector2(0.40f, 0.10f);
            dropdownRT.anchorMax = new Vector2(0.98f, 0.90f);
            dropdownRT.offsetMin = Vector2.zero;
            dropdownRT.offsetMax = Vector2.zero;

            Image dropdownBg = dropdownObj.AddComponent<Image>();
            dropdownBg.sprite = WhiteSprite;
            dropdownBg.color = DROPDOWN_BG;

            TMP_Dropdown dropdown = dropdownObj.AddComponent<TMP_Dropdown>();

            // Caption (selected value)
            GameObject captionObj = new GameObject("Label");
            captionObj.transform.SetParent(dropdownObj.transform, false);

            RectTransform captionRT = captionObj.AddComponent<RectTransform>();
            captionRT.anchorMin = Vector2.zero;
            captionRT.anchorMax = Vector2.one;
            captionRT.offsetMin = new Vector2(12, 0);
            captionRT.offsetMax = new Vector2(-30, 0);

            TextMeshProUGUI captionText = captionObj.AddComponent<TextMeshProUGUI>();
            captionText.font = Font;
            captionText.fontSize = FontSizes.H3;
            captionText.fontStyle = FontStyles.Bold;
            captionText.color = CYAN_NEON;
            captionText.alignment = TextAlignmentOptions.Left;
            captionText.raycastTarget = false;
            captionText.enableAutoSizing = true;
            captionText.fontSizeMin = FontSizes.AutoMinBody;
            captionText.fontSizeMax = FontSizes.H3;
            captionText.overflowMode = TextOverflowModes.Ellipsis;

            dropdown.captionText = captionText;

            // Arrow
            GameObject arrowObj = new GameObject("Arrow");
            arrowObj.transform.SetParent(dropdownObj.transform, false);

            RectTransform arrowRT = arrowObj.AddComponent<RectTransform>();
            arrowRT.anchorMin = new Vector2(1, 0);
            arrowRT.anchorMax = new Vector2(1, 1);
            arrowRT.pivot = new Vector2(1, 0.5f);
            arrowRT.sizeDelta = new Vector2(28, 0);
            arrowRT.anchoredPosition = new Vector2(-4, 0);

            TextMeshProUGUI arrowText = arrowObj.AddComponent<TextMeshProUGUI>();
            arrowText.font = Font;
            arrowText.text = "v";
            arrowText.fontSize = FontSizes.H4;
            arrowText.fontStyle = FontStyles.Bold;
            arrowText.color = CYAN_NEON;
            arrowText.alignment = TextAlignmentOptions.Center;
            arrowText.enableAutoSizing = true;
            arrowText.fontSizeMin = Mathf.Min(FontSizes.AutoMinBody, FontSizes.H4);
            arrowText.fontSizeMax = FontSizes.H4;
            arrowText.overflowMode = TextOverflowModes.Ellipsis;
            arrowText.raycastTarget = false;

            // Dropdown Template (required for TMP_Dropdown to function)
            BuildDropdownTemplate(dropdownObj.transform, dropdown);

            // Fix: reparent dropdown list to root Canvas to escape RectMask2D clipping
            dropdownObj.AddComponent<DigitPark.UI.DropdownScrollFix>();

            // Set options
            dropdown.ClearOptions();
            dropdown.AddOptions(new List<string>(options));
            dropdown.value = defaultIndex;
            dropdown.RefreshShownValue();
        }

        private static void BuildDropdownTemplate(Transform dropdownParent, TMP_Dropdown dropdown)
        {
            // Template root
            GameObject template = new GameObject("Template");
            template.transform.SetParent(dropdownParent, false);

            RectTransform templateRT = template.AddComponent<RectTransform>();
            templateRT.anchorMin = new Vector2(0, 0);
            templateRT.anchorMax = new Vector2(1, 0);
            templateRT.pivot = new Vector2(0.5f, 1);
            templateRT.sizeDelta = new Vector2(0, 500);

            Image templateBg = template.AddComponent<Image>();
            templateBg.sprite = WhiteSprite;
            templateBg.color = DROPDOWN_BG;

            ScrollRect scrollRect = template.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 40f;

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(template.transform, false);

            RectTransform viewportRT = viewport.AddComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.sizeDelta = Vector2.zero;
            viewportRT.anchoredPosition = Vector2.zero;

            Mask viewportMask = viewport.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            Image viewportImg = viewport.AddComponent<Image>();
            viewportImg.sprite = WhiteSprite;
            viewportImg.color = Color.white;

            scrollRect.viewport = viewportRT;

            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);

            RectTransform contentRT = content.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.sizeDelta = Vector2.zero;

            scrollRect.content = contentRT;

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 2;
            vlg.padding = new RectOffset(2, 2, 2, 2);
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;

            // Item template
            GameObject item = new GameObject("Item");
            item.transform.SetParent(content.transform, false);

            RectTransform itemRT = item.AddComponent<RectTransform>();
            itemRT.anchorMin = new Vector2(0, 0.5f);
            itemRT.anchorMax = new Vector2(1, 0.5f);
            itemRT.sizeDelta = new Vector2(0, 40);

            LayoutElement itemLE = item.AddComponent<LayoutElement>();
            itemLE.minHeight = 40;
            itemLE.preferredHeight = 40;

            Toggle itemToggle = item.AddComponent<Toggle>();

            // Item Background
            GameObject itemBg = new GameObject("Item Background");
            itemBg.transform.SetParent(item.transform, false);

            RectTransform itemBgRT = itemBg.AddComponent<RectTransform>();
            itemBgRT.anchorMin = Vector2.zero;
            itemBgRT.anchorMax = Vector2.one;
            itemBgRT.sizeDelta = Vector2.zero;

            Image itemBgImg = itemBg.AddComponent<Image>();
            itemBgImg.sprite = WhiteSprite;
            itemBgImg.color = DROPDOWN_ITEM_BG;

            // Item Checkmark
            GameObject itemCheck = new GameObject("Item Checkmark");
            itemCheck.transform.SetParent(item.transform, false);

            RectTransform checkRT = itemCheck.AddComponent<RectTransform>();
            checkRT.anchorMin = new Vector2(0, 0.5f);
            checkRT.anchorMax = new Vector2(0, 0.5f);
            checkRT.pivot = new Vector2(0, 0.5f);
            checkRT.sizeDelta = new Vector2(16, 16);
            checkRT.anchoredPosition = new Vector2(8, 0);

            Image checkImg = itemCheck.AddComponent<Image>();
            checkImg.sprite = WhiteSprite;
            checkImg.color = CYAN_NEON;

            // Item Label
            GameObject itemLabel = new GameObject("Item Label");
            itemLabel.transform.SetParent(item.transform, false);

            RectTransform itemLabelRT = itemLabel.AddComponent<RectTransform>();
            itemLabelRT.anchorMin = Vector2.zero;
            itemLabelRT.anchorMax = Vector2.one;
            itemLabelRT.offsetMin = new Vector2(30, 0);
            itemLabelRT.offsetMax = new Vector2(-8, 0);

            TextMeshProUGUI itemLabelText = itemLabel.AddComponent<TextMeshProUGUI>();
            itemLabelText.font = Font;
            itemLabelText.fontSize = FontSizes.H3;
            itemLabelText.fontStyle = FontStyles.Bold;
            itemLabelText.color = TEXT_WHITE;
            itemLabelText.alignment = TextAlignmentOptions.Left;
            itemLabelText.enableAutoSizing = true;
            itemLabelText.fontSizeMin = FontSizes.AutoMinBody;
            itemLabelText.fontSizeMax = FontSizes.H3;
            itemLabelText.overflowMode = TextOverflowModes.Ellipsis;

            // Wire up toggle
            itemToggle.targetGraphic = itemBgImg;
            itemToggle.graphic = checkImg;
            itemToggle.isOn = true;

            // Wire up dropdown template
            dropdown.template = templateRT;
            dropdown.itemText = itemLabelText;

            // Template hidden by default
            template.SetActive(false);
        }

        // ==================== SEPARATOR ====================

        private static void CreateSeparator(Transform parent)
        {
            GameObject sep = new GameObject("Separator");
            sep.transform.SetParent(parent, false);

            LayoutElement layout = sep.AddComponent<LayoutElement>();
            layout.preferredHeight = SEPARATOR_HEIGHT;

            Image img = sep.AddComponent<Image>();
            img.sprite = WhiteSprite;
            img.color = SEPARATOR_COLOR;
            img.raycastTarget = false;
        }

        // ==================== TEXT HELPER ====================

        private static TextMeshProUGUI CreateTextChild(Transform parent, string name, string text,
            float fontSize, Color color, TextAlignmentOptions alignment)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.font = Font;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.raycastTarget = false;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = Mathf.Min(FontSizes.AutoMinBody, fontSize);
            tmp.fontSizeMax = fontSize;
            tmp.overflowMode = TextOverflowModes.Ellipsis;

            return tmp;
        }
    }
}
