using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Settings UI Builder - Modern card-based design for competitive gaming
    /// Order: Account → Audio → Appearance → Danger Zone → Legal/Info
    /// </summary>
    public static class SettingsUIBuilder
    {
        private const float SCREEN_WIDTH = 1080f;
        private const float SCREEN_HEIGHT = 1920f;

        // Colors - CYAN Neon Theme
        private static readonly Color CyanNeon = new Color(0f, 1f, 1f, 1f);
        private static readonly Color DarkNavy = new Color(0.039f, 0.055f, 0.153f, 1f);
        private static readonly Color CardBackground = new Color(0.125f, 0.188f, 0.376f, 0.95f);
        private static readonly Color DangerRed = new Color(1f, 0.3f, 0.3f, 1f);
        private static readonly Color TextWhite = Color.white;
        private static readonly Color TextGray = new Color(0.7f, 0.7f, 0.7f, 1f);

        // Paths
        private const string WHITE_SPRITE_PATH = "Assets/_Project/Textures/UI/WhiteSquare.png";
        private const string FONT_ASSET_PATH = "Assets/_Project/Art/Fonts/Rajdhani/Rajdhani-Medium SDF.asset";

        // Spacing
        private const float PADDING = 30f;
        private const float CARD_PADDING = 30f;
        private const float CARD_SPACING = 20f;
        private const float ELEMENT_SPACING = 15f;
        private const float BUTTON_HEIGHT = 60f;

        private static Sprite WhiteSprite => AssetDatabase.LoadAssetAtPath<Sprite>(WHITE_SPRITE_PATH);
        private static TMP_FontAsset DefaultFont => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_ASSET_PATH);

        [MenuItem("DigitPark/UI Builders/Settings", false, 400)]
        public static void RebuildSettingsScene()
        {
            try
            {
                if (WhiteSprite == null || DefaultFont == null)
                {
                    Debug.LogError("❌ Missing prerequisites! Check WhiteSquare.png and Font");
                    return;
                }

                Canvas canvas = Object.FindFirstObjectByType<Canvas>();
                if (canvas == null)
                {
                    Debug.LogError("❌ No Canvas found in scene.");
                    return;
                }

                Debug.Log("⚙️ Starting Settings UI Build...");

                CleanExistingUI(canvas);
                BuildBackground(canvas);
                BuildHeader(canvas);
                BuildScrollView(canvas);

                Canvas.ForceUpdateCanvases();

                Debug.Log("✅ Settings UI built successfully!");
                EditorUtility.SetDirty(canvas.gameObject);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error in SettingsUIBuilder: {e.Message}\n{e.StackTrace}");
            }
        }

        private static void CleanExistingUI(Canvas canvas)
        {
            var children = new System.Collections.Generic.List<Transform>();
            foreach (Transform child in canvas.transform)
            {
                if (child.name != "EventSystem" && child.name != "---ANIMATION_MANAGERS---")
                {
                    children.Add(child);
                }
            }

            foreach (var child in children)
            {
                Object.DestroyImmediate(child.gameObject);
            }

            // Configure Canvas
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(SCREEN_WIDTH, SCREEN_HEIGHT);
            scaler.matchWidthOrHeight = 0f;

            Debug.Log("🧹 UI Cleaned");
        }

        private static void BuildBackground(Canvas canvas)
        {
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(canvas.transform, false);

            RectTransform rect = bg.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            Image image = bg.AddComponent<Image>();
            image.sprite = WhiteSprite;
            image.color = DarkNavy;

            bg.transform.SetAsFirstSibling();
        }

        private static void BuildHeader(Canvas canvas)
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(canvas.transform, false);

            RectTransform rect = header.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.sizeDelta = new Vector2(0, 120);

            // Back button
            CreateBackButton(header.transform);

            // Title
            GameObject title = new GameObject("Title");
            title.transform.SetParent(header.transform, false);

            RectTransform titleRect = title.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.5f);
            titleRect.anchorMax = new Vector2(0.5f, 0.5f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.sizeDelta = new Vector2(400, 60);

            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.font = DefaultFont;
            titleText.text = "CONFIGURACIÓN";
            titleText.fontSize = 40;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = CyanNeon;
            titleText.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateBackButton(Transform parent)
        {
            GameObject backBtn = new GameObject("BackButton");
            backBtn.transform.SetParent(parent, false);

            RectTransform rect = backBtn.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(0, 0.5f);
            rect.pivot = new Vector2(0, 0.5f);
            rect.sizeDelta = new Vector2(100, 50);
            rect.anchoredPosition = new Vector2(PADDING, 0);

            Button button = backBtn.AddComponent<Button>();

            // Transparent background
            Image bg = backBtn.AddComponent<Image>();
            bg.sprite = WhiteSprite;
            bg.color = new Color(0, 0, 0, 0);

            button.targetGraphic = bg;

            // Back text/icon
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(backBtn.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.font = DefaultFont;
            text.text = "← Atrás";
            text.fontSize = 20;
            text.fontStyle = FontStyles.Bold;
            text.color = CyanNeon;
            text.alignment = TextAlignmentOptions.Left;
        }

        private static void BuildScrollView(Canvas canvas)
        {
            // Scroll View Container
            GameObject scrollView = new GameObject("ScrollView");
            scrollView.transform.SetParent(canvas.transform, false);

            RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0, 0);
            scrollRect.anchorMax = new Vector2(1, 1);
            scrollRect.offsetMin = new Vector2(0, 0);
            scrollRect.offsetMax = new Vector2(0, -120); // Below header

            ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);

            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;

            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            Image maskImage = viewport.AddComponent<Image>();
            maskImage.sprite = WhiteSprite;
            maskImage.color = new Color(1, 1, 1, 0.01f);

            scroll.viewport = viewportRect;

            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);

            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 0);

            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.spacing = CARD_SPACING;
            layout.padding = new RectOffset((int)PADDING, (int)PADDING, 30, 30);

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = contentRect;

            // Build all sections
            BuildAccountSection(content.transform);
            BuildAudioSection(content.transform);
            BuildAppearanceSection(content.transform);
            BuildDangerZoneSection(content.transform);
            BuildLegalInfoSection(content.transform);
        }

        private static void BuildAccountSection(Transform parent)
        {
            GameObject card = CreateCard(parent, "AccountCard", "👤 CUENTA");

            CreateButton(card.transform, "ChangeNameButton", "Cambiar Nombre de Usuario", "💎 100 Gemas", CyanNeon);
            CreateButton(card.transform, "LinkGoogleButton", "Vincular Google", "", CyanNeon);
            CreateButton(card.transform, "LinkAppleButton", "Vincular Apple ID", "", CyanNeon);

            // Player ID (copy button)
            CreatePlayerIDRow(card.transform);
        }

        private static void BuildAudioSection(Transform parent)
        {
            GameObject card = CreateCard(parent, "AudioCard", "🔊 AUDIO");

            CreateSlider(card.transform, "MusicSlider", "Música", 0.7f);
            CreateSlider(card.transform, "SFXSlider", "Efectos", 0.8f);
            CreateToggle(card.transform, "VibrationToggle", "Vibración", true);
        }

        private static void BuildAppearanceSection(Transform parent)
        {
            GameObject card = CreateCard(parent, "AppearanceCard", "🎨 APARIENCIA");

            CreateDropdown(card.transform, "LanguageDropdown", "Idioma", new[] { "English", "Español" }, 1);
            CreateDropdown(card.transform, "ThemeDropdown", "Tema", new[] { "Neon Dark", "Light", "Classic" }, 0);
            CreateDropdown(card.transform, "QualityDropdown", "Calidad Gráfica", new[] { "Baja", "Media", "Alta" }, 2);
        }

        private static void BuildDangerZoneSection(Transform parent)
        {
            GameObject card = CreateCard(parent, "DangerCard", "⚠️ ZONA PELIGROSA");

            CreateButton(card.transform, "LogoutButton", "Cerrar Sesión", "", TextGray);
            CreateButton(card.transform, "DeleteAccountButton", "Eliminar Cuenta", "Permanente", DangerRed);
        }

        private static void BuildLegalInfoSection(Transform parent)
        {
            GameObject card = CreateCard(parent, "LegalCard", "📄 LEGAL E INFORMACIÓN");

            CreateButton(card.transform, "SupportButton", "Centro de Soporte", "", CyanNeon);
            CreateButton(card.transform, "TutorialButton", "Ver Tutorial", "", CyanNeon);
            CreateButton(card.transform, "TermsButton", "Términos y Condiciones", "", TextGray);
            CreateButton(card.transform, "PrivacyButton", "Política de Privacidad", "", TextGray);
            CreateButton(card.transform, "CreditsButton", "Créditos", "", TextGray);

            CreateSpacer(card.transform, 10f);

            // Version text
            CreateVersionText(card.transform, "v1.0.0 - Digit Park");
        }

        // Helper methods continue in next part...
        private static GameObject CreateCard(Transform parent, string name, string title)
        {
            GameObject cardContainer = new GameObject(name);
            cardContainer.transform.SetParent(parent, false);

            LayoutElement cardLayout = cardContainer.AddComponent<LayoutElement>();
            cardLayout.preferredHeight = -1; // Auto height

            // Card background
            GameObject card = new GameObject("Card");
            card.transform.SetParent(cardContainer.transform, false);

            RectTransform cardRect = card.AddComponent<RectTransform>();
            cardRect.anchorMin = Vector2.zero;
            cardRect.anchorMax = Vector2.one;
            cardRect.sizeDelta = Vector2.zero;

            Image cardBg = card.AddComponent<Image>();
            cardBg.sprite = WhiteSprite;
            cardBg.color = CardBackground;

            // Cyan border
            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = CyanNeon;
            outline.effectDistance = new Vector2(2, -2);

            // Content container
            GameObject content = new GameObject("Content");
            content.transform.SetParent(card.transform, false);

            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.sizeDelta = Vector2.zero;

            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.spacing = ELEMENT_SPACING;
            layout.padding = new RectOffset((int)CARD_PADDING, (int)CARD_PADDING, (int)CARD_PADDING, (int)CARD_PADDING);

            ContentSizeFitter fitter = cardContainer.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Card title
            CreateCardTitle(content.transform, title);

            return content;
        }

        private static void CreateCardTitle(Transform parent, string text)
        {
            GameObject titleObj = new GameObject("SectionTitle");
            titleObj.transform.SetParent(parent, false);

            TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
            title.font = DefaultFont;
            title.text = text;
            title.fontSize = 24;
            title.fontStyle = FontStyles.Bold;
            title.color = CyanNeon;
            title.alignment = TextAlignmentOptions.Left;

            LayoutElement layout = titleObj.AddComponent<LayoutElement>();
            layout.preferredHeight = 40;
        }

        private static void CreateButton(Transform parent, string name, string mainText, string subText, Color color)
        {
            GameObject btn = new GameObject(name);
            btn.transform.SetParent(parent, false);

            Image bg = btn.AddComponent<Image>();
            bg.sprite = WhiteSprite;
            bg.color = new Color(0.2f, 0.2f, 0.25f, 0.5f);

            Button button = btn.AddComponent<Button>();
            button.targetGraphic = bg;

            // Button border
            Outline outline = btn.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(2, -2);

            LayoutElement layout = btn.AddComponent<LayoutElement>();
            layout.preferredHeight = BUTTON_HEIGHT;

            // Main text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btn.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.offsetMin = new Vector2(20, 0);
            textRect.offsetMax = new Vector2(-20, 0);

            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.font = DefaultFont;
            btnText.text = mainText;
            btnText.fontSize = 20;
            btnText.fontStyle = FontStyles.Bold;
            btnText.color = TextWhite;
            btnText.alignment = TextAlignmentOptions.Left;

            // Subtext (if provided)
            if (!string.IsNullOrEmpty(subText))
            {
                GameObject subTextObj = new GameObject("SubText");
                subTextObj.transform.SetParent(btn.transform, false);

                RectTransform subRect = subTextObj.AddComponent<RectTransform>();
                subRect.anchorMin = new Vector2(0, 0);
                subRect.anchorMax = new Vector2(1, 1);
                subRect.offsetMin = new Vector2(20, 0);
                subRect.offsetMax = new Vector2(-20, 0);

                TextMeshProUGUI subTextComponent = subTextObj.AddComponent<TextMeshProUGUI>();
                subTextComponent.font = DefaultFont;
                subTextComponent.text = subText;
                subTextComponent.fontSize = 14;
                subTextComponent.color = color;
                subTextComponent.alignment = TextAlignmentOptions.Right;
            }
        }

        private static void CreateSlider(Transform parent, string name, string label, float defaultValue)
        {
            GameObject container = new GameObject(name + "Container");
            container.transform.SetParent(parent, false);

            LayoutElement layout = container.AddComponent<LayoutElement>();
            layout.preferredHeight = 70;

            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(container.transform, false);

            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0.5f);
            labelRect.anchorMax = new Vector2(0.4f, 1);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.font = DefaultFont;
            labelText.text = label;
            labelText.fontSize = 18;
            labelText.color = TextWhite;
            labelText.alignment = TextAlignmentOptions.Left;

            // Slider
            GameObject sliderObj = new GameObject(name);
            sliderObj.transform.SetParent(container.transform, false);

            RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.4f, 0);
            sliderRect.anchorMax = new Vector2(1, 0.5f);
            sliderRect.offsetMin = new Vector2(10, 0);
            sliderRect.offsetMax = Vector2.zero;

            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = defaultValue;

            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(sliderObj.transform, false);

            RectTransform bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = new Vector2(0, 8);

            Image bgImage = bg.AddComponent<Image>();
            bgImage.sprite = WhiteSprite;
            bgImage.color = new Color(0.2f, 0.2f, 0.25f, 1f);

            // Fill Area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);

            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.sizeDelta = new Vector2(0, 8);

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);

            RectTransform fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;

            Image fillImage = fill.AddComponent<Image>();
            fillImage.sprite = WhiteSprite;
            fillImage.color = CyanNeon;

            slider.fillRect = fillRect;

            // Handle
            GameObject handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(sliderObj.transform, false);

            RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.sizeDelta = new Vector2(-10, 0);

            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);

            RectTransform handleRect = handle.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 20);

            Image handleImage = handle.AddComponent<Image>();
            handleImage.sprite = WhiteSprite;
            handleImage.color = CyanNeon;

            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;

            // Percentage text
            GameObject percentObj = new GameObject("Percentage");
            percentObj.transform.SetParent(container.transform, false);

            RectTransform percentRect = percentObj.AddComponent<RectTransform>();
            percentRect.anchorMin = new Vector2(0.4f, 0.5f);
            percentRect.anchorMax = new Vector2(1, 1);
            percentRect.offsetMin = new Vector2(10, 0);
            percentRect.offsetMax = Vector2.zero;

            TextMeshProUGUI percentText = percentObj.AddComponent<TextMeshProUGUI>();
            percentText.font = DefaultFont;
            percentText.text = $"{(int)(defaultValue * 100)}%";
            percentText.fontSize = 16;
            percentText.color = CyanNeon;
            percentText.alignment = TextAlignmentOptions.Right;
        }

        private static void CreateToggle(Transform parent, string name, string label, bool defaultValue)
        {
            GameObject container = new GameObject(name + "Container");
            container.transform.SetParent(parent, false);

            LayoutElement layout = container.AddComponent<LayoutElement>();
            layout.preferredHeight = 50;

            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(container.transform, false);

            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(0.7f, 1);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.font = DefaultFont;
            labelText.text = label;
            labelText.fontSize = 18;
            labelText.color = TextWhite;
            labelText.alignment = TextAlignmentOptions.Left;

            // Toggle
            GameObject toggleObj = new GameObject(name);
            toggleObj.transform.SetParent(container.transform, false);

            RectTransform toggleRect = toggleObj.AddComponent<RectTransform>();
            toggleRect.anchorMin = new Vector2(0.7f, 0.5f);
            toggleRect.anchorMax = new Vector2(1, 0.5f);
            toggleRect.pivot = new Vector2(1, 0.5f);
            toggleRect.sizeDelta = new Vector2(100, 40);

            Toggle toggle = toggleObj.AddComponent<Toggle>();
            toggle.isOn = defaultValue;

            // Background
            Image toggleBg = toggleObj.AddComponent<Image>();
            toggleBg.sprite = WhiteSprite;
            toggleBg.color = defaultValue ? CyanNeon : new Color(0.3f, 0.3f, 0.3f, 1f);

            toggle.targetGraphic = toggleBg;

            // Checkmark (text ON/OFF)
            GameObject checkmark = new GameObject("Checkmark");
            checkmark.transform.SetParent(toggleObj.transform, false);

            RectTransform checkRect = checkmark.AddComponent<RectTransform>();
            checkRect.anchorMin = Vector2.zero;
            checkRect.anchorMax = Vector2.one;
            checkRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI checkText = checkmark.AddComponent<TextMeshProUGUI>();
            checkText.font = DefaultFont;
            checkText.text = defaultValue ? "ON" : "OFF";
            checkText.fontSize = 16;
            checkText.fontStyle = FontStyles.Bold;
            checkText.color = defaultValue ? DarkNavy : TextGray;
            checkText.alignment = TextAlignmentOptions.Center;

            toggle.graphic = checkText;
        }

        private static void CreateDropdown(Transform parent, string name, string label, string[] options, int defaultIndex)
        {
            GameObject container = new GameObject(name + "Container");
            container.transform.SetParent(parent, false);

            LayoutElement layout = container.AddComponent<LayoutElement>();
            layout.preferredHeight = 50;

            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(container.transform, false);

            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(0.4f, 1);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.font = DefaultFont;
            labelText.text = label;
            labelText.fontSize = 18;
            labelText.color = TextWhite;
            labelText.alignment = TextAlignmentOptions.Left;

            // Dropdown
            GameObject dropdownObj = new GameObject(name);
            dropdownObj.transform.SetParent(container.transform, false);

            RectTransform dropdownRect = dropdownObj.AddComponent<RectTransform>();
            dropdownRect.anchorMin = new Vector2(0.4f, 0);
            dropdownRect.anchorMax = new Vector2(1, 1);
            dropdownRect.offsetMin = new Vector2(10, 0);
            dropdownRect.offsetMax = Vector2.zero;

            Image dropdownBg = dropdownObj.AddComponent<Image>();
            dropdownBg.sprite = WhiteSprite;
            dropdownBg.color = new Color(0.2f, 0.2f, 0.25f, 1f);

            TMP_Dropdown dropdown = dropdownObj.AddComponent<TMP_Dropdown>();
            dropdown.ClearOptions();
            dropdown.AddOptions(new System.Collections.Generic.List<string>(options));
            dropdown.value = defaultIndex;

            // Label (selected text)
            GameObject selectedLabel = new GameObject("Label");
            selectedLabel.transform.SetParent(dropdownObj.transform, false);

            RectTransform selectedRect = selectedLabel.AddComponent<RectTransform>();
            selectedRect.anchorMin = Vector2.zero;
            selectedRect.anchorMax = Vector2.one;
            selectedRect.offsetMin = new Vector2(10, 0);
            selectedRect.offsetMax = new Vector2(-30, 0);

            TextMeshProUGUI selectedText = selectedLabel.AddComponent<TextMeshProUGUI>();
            selectedText.font = DefaultFont;
            selectedText.fontSize = 16;
            selectedText.color = CyanNeon;
            selectedText.alignment = TextAlignmentOptions.Left;

            dropdown.captionText = selectedText;

            // Arrow
            GameObject arrow = new GameObject("Arrow");
            arrow.transform.SetParent(dropdownObj.transform, false);

            RectTransform arrowRect = arrow.AddComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(1, 0);
            arrowRect.anchorMax = new Vector2(1, 1);
            arrowRect.pivot = new Vector2(1, 0.5f);
            arrowRect.sizeDelta = new Vector2(20, 20);
            arrowRect.anchoredPosition = new Vector2(-10, 0);

            TextMeshProUGUI arrowText = arrow.AddComponent<TextMeshProUGUI>();
            arrowText.font = DefaultFont;
            arrowText.text = "▼";
            arrowText.fontSize = 14;
            arrowText.color = CyanNeon;
            arrowText.alignment = TextAlignmentOptions.Center;
        }

        private static void CreatePlayerIDRow(Transform parent)
        {
            GameObject container = new GameObject("PlayerIDContainer");
            container.transform.SetParent(parent, false);

            LayoutElement layout = container.AddComponent<LayoutElement>();
            layout.preferredHeight = 50;

            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(container.transform, false);

            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(0.5f, 1);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.font = DefaultFont;
            labelText.text = "ID de Jugador:";
            labelText.fontSize = 16;
            labelText.color = TextGray;
            labelText.alignment = TextAlignmentOptions.Left;

            // ID Text
            GameObject idObj = new GameObject("IDText");
            idObj.transform.SetParent(container.transform, false);

            RectTransform idRect = idObj.AddComponent<RectTransform>();
            idRect.anchorMin = new Vector2(0.5f, 0);
            idRect.anchorMax = new Vector2(0.8f, 1);
            idRect.offsetMin = Vector2.zero;
            idRect.offsetMax = Vector2.zero;

            TextMeshProUGUI idText = idObj.AddComponent<TextMeshProUGUI>();
            idText.font = DefaultFont;
            idText.text = "#ABC123XYZ";
            idText.fontSize = 14;
            idText.color = TextWhite;
            idText.alignment = TextAlignmentOptions.Center;

            // Copy button
            GameObject copyBtn = new GameObject("CopyButton");
            copyBtn.transform.SetParent(container.transform, false);

            RectTransform copyRect = copyBtn.AddComponent<RectTransform>();
            copyRect.anchorMin = new Vector2(0.8f, 0);
            copyRect.anchorMax = new Vector2(1, 1);
            copyRect.offsetMin = Vector2.zero;
            copyRect.offsetMax = Vector2.zero;

            Button button = copyBtn.AddComponent<Button>();

            Image copyBg = copyBtn.AddComponent<Image>();
            copyBg.sprite = WhiteSprite;
            copyBg.color = CyanNeon;

            button.targetGraphic = copyBg;

            GameObject copyText = new GameObject("Text");
            copyText.transform.SetParent(copyBtn.transform, false);

            RectTransform copyTextRect = copyText.AddComponent<RectTransform>();
            copyTextRect.anchorMin = Vector2.zero;
            copyTextRect.anchorMax = Vector2.one;
            copyTextRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI copyTextComponent = copyText.AddComponent<TextMeshProUGUI>();
            copyTextComponent.font = DefaultFont;
            copyTextComponent.text = "Copiar";
            copyTextComponent.fontSize = 14;
            copyTextComponent.fontStyle = FontStyles.Bold;
            copyTextComponent.color = DarkNavy;
            copyTextComponent.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateVersionText(Transform parent, string version)
        {
            GameObject versionObj = new GameObject("Version");
            versionObj.transform.SetParent(parent, false);

            TextMeshProUGUI versionText = versionObj.AddComponent<TextMeshProUGUI>();
            versionText.font = DefaultFont;
            versionText.text = version;
            versionText.fontSize = 14;
            versionText.color = TextGray;
            versionText.alignment = TextAlignmentOptions.Center;

            LayoutElement layout = versionObj.AddComponent<LayoutElement>();
            layout.preferredHeight = 30;
        }

        private static void CreateSpacer(Transform parent, float height)
        {
            GameObject spacer = new GameObject("Spacer");
            spacer.transform.SetParent(parent, false);

            LayoutElement layout = spacer.AddComponent<LayoutElement>();
            layout.preferredHeight = height;
        }
    }
}
