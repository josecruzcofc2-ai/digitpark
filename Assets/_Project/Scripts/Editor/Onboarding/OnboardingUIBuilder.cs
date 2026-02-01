using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Modern Onboarding UI builder with premium neon design
    /// Creates 5 slides: Welcome, Play & Win, Daily Missions, Battle Pass, Ready
    /// </summary>
    public static class OnboardingUIBuilder
    {
        private const float SCREEN_WIDTH = 1080f;
        private const float SCREEN_HEIGHT = 1920f;

        // Colors
        private static readonly Color CyanNeon = new Color(0f, 1f, 1f, 1f); // #00FFFF
        private static readonly Color DarkNavy = new Color(0.039f, 0.055f, 0.153f, 1f); // #0A0E27
        private static readonly Color CardBackground = new Color(0.125f, 0.188f, 0.376f, 0.95f); // #202860
        private static readonly Color TextWhite = Color.white;
        private static readonly Color TextGray = new Color(0.7f, 0.7f, 0.7f, 1f);

        // Theme colors for each slide
        private static readonly Color GoldPremium = new Color(1f, 0.843f, 0f, 1f); // #FFD700
        private static readonly Color PurplePremium = new Color(0.6f, 0.3f, 0.9f, 1f);
        private static readonly Color GreenSuccess = new Color(0.2f, 0.8f, 0.4f, 1f);
        private static readonly Color GemColor = new Color(0.4f, 0.8f, 1f, 1f);

        // Paths
        private const string WHITE_SPRITE_PATH = "Assets/_Project/Textures/UI/WhiteSquare.png";
        private const string FONT_ASSET_PATH = "Assets/_Project/Art/Fonts/Rajdhani/Rajdhani-Medium SDF.asset";

        // Icon paths for each slide
        private const string ICON_WELCOME_PATH = "Assets/_Project/Art/Icons/UI/icon_ui_gift_generic.png";
        private const string ICON_PLAY_WIN_PATH = "Assets/_Project/Art/Icons/CashBattle/Stats/stat_victories.png";
        private const string ICON_MISSIONS_PATH = "Assets/_Project/Art/Icons/DailyRewards/GiftIcon.png";
        private const string ICON_BATTLEPASS_PATH = "Assets/_Project/Art/Icons/BattlePass/icon_battlepass_crown.png";
        private const string ICON_READY_PATH = "Assets/_Project/Art/Icons/UI/PlayIconNeon.png";

        // Welcome gift icons
        private const string ICON_COINS_PATH = "Assets/_Project/Art/Icons/Currency/icon_coins.png";
        private const string ICON_GEMS_PATH = "Assets/_Project/Art/Icons/Currency/GemIcon.png";
        private const string ICON_XP_PATH = "Assets/_Project/Art/Icons/Currency/icon_xp.png";

        // Spacing
        private const float PADDING = 30f;
        private const float CARD_PADDING = 40f;
        private const float ELEMENT_SPACING = 20f;
        private const float BUTTON_HEIGHT = 70f;
        private const float ICON_SIZE = 150f;
        private const float DOT_SIZE = 12f;
        private const float DOT_SPACING = 15f;

        private static Sprite WhiteSprite => AssetDatabase.LoadAssetAtPath<Sprite>(WHITE_SPRITE_PATH);
        private static TMP_FontAsset DefaultFont => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_ASSET_PATH);

        [MenuItem("DigitPark/UI Builders/Onboarding/Main Onboarding", false, 300)]
        public static void RebuildOnboardingScene()
        {
            try
            {
                // Verify prerequisites
                if (WhiteSprite == null || DefaultFont == null)
                {
                    Debug.LogError("❌ Missing prerequisites! Check WhiteSquare.png and Font.");
                    return;
                }

                Canvas canvas = Object.FindFirstObjectByType<Canvas>();
                if (canvas == null)
                {
                    Debug.LogError("❌ No Canvas found in scene.");
                    return;
                }

                Debug.Log("🎨 Starting Onboarding UI Rebuild...");

                // Clean existing UI (except EventSystem and managers)
                CleanExistingUI(canvas);

                // Build UI
                BuildBackground(canvas);
                GameObject safeArea = BuildSafeArea(canvas);
                BuildSlidesContainer(safeArea);
                BuildProgressDots(safeArea);
                BuildNavigationButtons(safeArea); // Now includes Skip button
                BuildWelcomeGiftBlocker(canvas);

                // Force layout update
                Canvas.ForceUpdateCanvases();

                Debug.Log("✅ Onboarding UI rebuilt successfully!");
                EditorUtility.SetDirty(canvas.gameObject);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error in OnboardingUIBuilder: {e.Message}\n{e.StackTrace}");
            }
        }

        private static void CleanExistingUI(Canvas canvas)
        {
            Debug.Log("🧹 Cleaning existing UI elements...");

            // Collect all children except EventSystem and managers
            var children = new System.Collections.Generic.List<Transform>();
            foreach (Transform child in canvas.transform)
            {
                // Keep only EventSystem and animation managers
                if (child.name != "EventSystem" &&
                    child.name != "---ANIMATION_MANAGERS---" &&
                    child.name != "OnboardingManager" &&
                    !child.name.Contains("Manager"))
                {
                    children.Add(child);
                }
            }

            // Destroy all UI elements
            foreach (var child in children)
            {
                Debug.Log($"  Removing: {child.name}");
                Object.DestroyImmediate(child.gameObject);
            }

            Debug.Log($"🧹 Removed {children.Count} UI elements");

            // Configure Canvas fresh
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(SCREEN_WIDTH, SCREEN_HEIGHT);
            scaler.matchWidthOrHeight = 0f;

            // Force canvas update before building
            Canvas.ForceUpdateCanvases();

            Debug.Log("✅ Canvas cleaned and configured");
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

        private static GameObject BuildSafeArea(Canvas canvas)
        {
            GameObject safeArea = new GameObject("SafeArea");
            safeArea.transform.SetParent(canvas.transform, false);

            RectTransform rect = safeArea.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            return safeArea;
        }

        private static void BuildSkipButton(GameObject parent)
        {
            GameObject skipBtn = new GameObject("SkipButton");
            skipBtn.transform.SetParent(parent.transform, false);

            RectTransform rect = skipBtn.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.sizeDelta = new Vector2(100, 50);
            rect.anchoredPosition = new Vector2(-20, -20);

            Button btn = skipBtn.AddComponent<Button>();
            btn.transition = Selectable.Transition.ColorTint;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(skipBtn.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.font = DefaultFont;
            text.text = "Saltar";
            text.fontSize = 18;
            text.color = TextGray;
            text.alignment = TextAlignmentOptions.Center;

            btn.targetGraphic = text;
        }

        private static void BuildSlidesContainer(GameObject parent)
        {
            GameObject slidesContainer = new GameObject("SlidesContainer");
            slidesContainer.transform.SetParent(parent.transform, false);

            RectTransform rect = slidesContainer.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.2f);
            rect.anchorMax = new Vector2(1, 0.9f);
            rect.sizeDelta = Vector2.zero;

            // Create 5 slides with numbers
            CreateSlide(slidesContainer, "Slide1", "¡Bienvenido a DigitPark!",
                "El mejor lugar para jugar, competir y ganar premios reales.",
                CyanNeon, 1, true);

            CreateSlide(slidesContainer, "Slide2", "Juega y Gana",
                "Participa en torneos, desafía a otros jugadores y gana monedas reales.",
                GoldPremium, 2, false);

            CreateSlide(slidesContainer, "Slide3", "Misiones Diarias",
                "Completa misiones cada día para obtener gemas, monedas y recompensas exclusivas.",
                GemColor, 3, false);

            CreateSlide(slidesContainer, "Slide4", "Pase de Batalla",
                "Desbloquea recompensas increíbles mientras subes de nivel cada temporada.",
                PurplePremium, 4, false);

            CreateSlide(slidesContainer, "Slide5", "¡Listo para Comenzar!",
                "Reclama tu regalo de bienvenida y empieza a jugar ahora.",
                GreenSuccess, 5, false);
        }

        private static void CreateSlide(GameObject parent, string name, string title, string description, Color accentColor, int slideNumber, bool isActive)
        {
            GameObject slide = new GameObject(name);
            slide.transform.SetParent(parent.transform, false);
            slide.SetActive(isActive);

            RectTransform slideRect = slide.AddComponent<RectTransform>();
            slideRect.anchorMin = Vector2.zero;
            slideRect.anchorMax = Vector2.one;
            slideRect.sizeDelta = Vector2.zero;

            // Content container
            GameObject content = new GameObject("Content");
            content.transform.SetParent(slide.transform, false);

            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.sizeDelta = Vector2.zero;

            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.spacing = ELEMENT_SPACING;
            layout.padding = new RectOffset((int)CARD_PADDING, (int)CARD_PADDING, (int)CARD_PADDING, (int)CARD_PADDING);

            // Number with divider line (inside VerticalLayoutGroup)
            CreateSlideNumber_OLD(content.transform, slideNumber, accentColor);

            // Title
            CreateSlideTitle(content.transform, title, accentColor);

            // Description
            CreateSlideDescription(content.transform, description);
        }

        private static void CreateSlideNumberDividerNeon(Transform slideParent, int number, Color accentColor)
        {
            // EXACT COPY from CashBattle but with CYAN colors
            // Container for divider + number (OUTSIDE VerticalLayoutGroup)
            GameObject dividerContainer = new GameObject("DividerContainer");
            dividerContainer.transform.SetParent(slideParent, false);

            RectTransform containerRect = dividerContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            containerRect.sizeDelta = new Vector2(SCREEN_WIDTH, ICON_SIZE);
            containerRect.anchoredPosition = new Vector2(0, 200); // Default position (user can move)

            // Horizontal divider line - EXACT same as CashBattle
            GameObject divider = new GameObject("Divider");
            divider.transform.SetParent(dividerContainer.transform, false);

            RectTransform dividerRect = divider.AddComponent<RectTransform>();
            dividerRect.anchorMin = new Vector2(0.5f, 0.5f);
            dividerRect.anchorMax = new Vector2(0.5f, 0.5f);
            dividerRect.pivot = new Vector2(0.5f, 0.5f);
            dividerRect.sizeDelta = new Vector2(SCREEN_WIDTH - (PADDING * 4), 4f);

            Image dividerImage = divider.AddComponent<Image>();
            dividerImage.sprite = WhiteSprite;
            dividerImage.color = accentColor; // CYAN for Onboarding

            // Colored square (on top of divider) - EXACT same as CashBattle
            GameObject square = new GameObject("Square");
            square.transform.SetParent(dividerContainer.transform, false);

            RectTransform squareRect = square.AddComponent<RectTransform>();
            squareRect.anchorMin = new Vector2(0.5f, 0.5f);
            squareRect.anchorMax = new Vector2(0.5f, 0.5f);
            squareRect.pivot = new Vector2(0.5f, 0.5f);
            squareRect.sizeDelta = new Vector2(ICON_SIZE, ICON_SIZE);

            Image squareImage = square.AddComponent<Image>();
            squareImage.sprite = WhiteSprite;
            squareImage.color = accentColor; // CYAN for Onboarding

            // NEON Outline effect on Square (like CashBattle but CYAN)
            Outline squareOutline = square.AddComponent<Outline>();
            squareOutline.effectColor = accentColor; // Same CYAN color for neon effect
            squareOutline.effectDistance = new Vector2(3, -3);

            // Number text - EXACT same as CashBattle
            GameObject numberText = new GameObject("Text");
            numberText.transform.SetParent(square.transform, false);

            RectTransform textRect = numberText.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI text = numberText.AddComponent<TextMeshProUGUI>();
            text.font = DefaultFont;
            text.text = number.ToString();
            text.fontSize = 72;
            text.fontStyle = FontStyles.Bold;
            text.color = DarkNavy;
            text.alignment = TextAlignmentOptions.Center;
        }

        // OLD METHOD - kept for reference, not used anymore
        private static void CreateSlideNumber_OLD(Transform parent, int number, Color accentColor)
        {
            GameObject numberContainer = new GameObject("NumberContainer");
            numberContainer.transform.SetParent(parent, false);

            LayoutElement layout = numberContainer.AddComponent<LayoutElement>();
            layout.preferredHeight = ICON_SIZE;

            // Horizontal divider line (CYAN NEON - thick like CashBattle)
            GameObject divider = new GameObject("Divider");
            divider.transform.SetParent(numberContainer.transform, false);

            RectTransform dividerRect = divider.AddComponent<RectTransform>();
            dividerRect.anchorMin = new Vector2(0.5f, 0.5f);
            dividerRect.anchorMax = new Vector2(0.5f, 0.5f);
            dividerRect.pivot = new Vector2(0.5f, 0.5f);
            dividerRect.sizeDelta = new Vector2(SCREEN_WIDTH - (PADDING * 4), 4f); // Thick line like CashBattle

            Image dividerImage = divider.AddComponent<Image>();
            dividerImage.sprite = WhiteSprite;
            dividerImage.color = accentColor; // CYAN or slide accent color

            // Add neon glow effect with Shadow component
            UnityEngine.UI.Shadow glow = divider.AddComponent<UnityEngine.UI.Shadow>();
            glow.effectColor = new Color(accentColor.r, accentColor.g, accentColor.b, 0.5f);
            glow.effectDistance = new Vector2(0, 0);
            glow.useGraphicAlpha = false;

            // Colored square background (on top of divider)
            GameObject square = new GameObject("Square");
            square.transform.SetParent(numberContainer.transform, false);

            RectTransform squareRect = square.AddComponent<RectTransform>();
            squareRect.anchorMin = new Vector2(0.5f, 0.5f);
            squareRect.anchorMax = new Vector2(0.5f, 0.5f);
            squareRect.pivot = new Vector2(0.5f, 0.5f);
            squareRect.sizeDelta = new Vector2(ICON_SIZE, ICON_SIZE);

            Image squareImage = square.AddComponent<Image>();
            squareImage.sprite = WhiteSprite;
            squareImage.color = accentColor;

            // Optional: Add border/outline
            Outline outline = square.AddComponent<Outline>();
            outline.effectColor = new Color(accentColor.r * 0.5f, accentColor.g * 0.5f, accentColor.b * 0.5f, 1f);
            outline.effectDistance = new Vector2(3, -3);

            // Number text
            GameObject numberText = new GameObject("Text");
            numberText.transform.SetParent(square.transform, false);

            RectTransform textRect = numberText.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI text = numberText.AddComponent<TextMeshProUGUI>();
            text.font = DefaultFont;
            text.text = number.ToString();
            text.fontSize = 72;
            text.fontStyle = FontStyles.Bold;
            text.color = DarkNavy;
            text.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateSlideTitle(Transform parent, string text, Color accentColor)
        {
            GameObject title = new GameObject("Title");
            title.transform.SetParent(parent, false);

            LayoutElement layout = title.AddComponent<LayoutElement>();
            layout.preferredHeight = 80;

            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.font = DefaultFont;
            titleText.text = text;
            titleText.fontSize = 36;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = accentColor;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.enableWordWrapping = true;
        }

        private static void CreateSlideDescription(Transform parent, string text)
        {
            GameObject desc = new GameObject("Description");
            desc.transform.SetParent(parent, false);

            LayoutElement layout = desc.AddComponent<LayoutElement>();
            layout.preferredHeight = 100;

            TextMeshProUGUI descText = desc.AddComponent<TextMeshProUGUI>();
            descText.font = DefaultFont;
            descText.text = text;
            descText.fontSize = 20;
            descText.color = TextWhite;
            descText.alignment = TextAlignmentOptions.Center;
            descText.enableWordWrapping = true;
        }

        private static void BuildProgressDots(GameObject parent)
        {
            GameObject dotsContainer = new GameObject("ProgressDots");
            dotsContainer.transform.SetParent(parent.transform, false);

            RectTransform rect = dotsContainer.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.15f);
            rect.anchorMax = new Vector2(0.5f, 0.15f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(5 * (DOT_SIZE + DOT_SPACING), DOT_SIZE);

            HorizontalLayoutGroup layout = dotsContainer.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = DOT_SPACING;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            // Create 5 dots
            for (int i = 1; i <= 5; i++)
            {
                CreateDot(dotsContainer, $"Dot{i}", i == 1);
            }
        }

        private static void CreateDot(GameObject parent, string name, bool isActive)
        {
            GameObject dot = new GameObject(name);
            dot.transform.SetParent(parent.transform, false);

            RectTransform rect = dot.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(DOT_SIZE, DOT_SIZE);

            Image image = dot.AddComponent<Image>();
            image.sprite = WhiteSprite;
            image.color = isActive ? CyanNeon : new Color(0.3f, 0.35f, 0.4f, 1f);
        }

        private static void BuildNavigationButtons(GameObject parent)
        {
            // Navigation Panel
            GameObject navPanel = new GameObject("NavigationPanel");
            navPanel.transform.SetParent(parent.transform, false);

            RectTransform panelRect = navPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0);
            panelRect.anchorMax = new Vector2(0.5f, 0);
            panelRect.pivot = new Vector2(0.5f, 0);
            panelRect.sizeDelta = new Vector2(SCREEN_WIDTH - (PADDING * 2), 220);
            panelRect.anchoredPosition = new Vector2(0, 40);

            // Dots Container (optional, can be removed if already exists)
            GameObject dotsContainer = new GameObject("DotsContainer");
            dotsContainer.transform.SetParent(navPanel.transform, false);

            RectTransform dotsRect = dotsContainer.AddComponent<RectTransform>();
            dotsRect.anchorMin = new Vector2(0.5f, 1);
            dotsRect.anchorMax = new Vector2(0.5f, 1);
            dotsRect.pivot = new Vector2(0.5f, 1);
            dotsRect.sizeDelta = new Vector2(200, 30);
            dotsRect.anchoredPosition = new Vector2(0, -10);

            HorizontalLayoutGroup dotsLayout = dotsContainer.AddComponent<HorizontalLayoutGroup>();
            dotsLayout.childAlignment = TextAnchor.MiddleCenter;
            dotsLayout.childControlWidth = false;
            dotsLayout.childControlHeight = false;
            dotsLayout.spacing = 10f;

            // Buttons Container
            GameObject buttonsContainer = new GameObject("Buttons");
            buttonsContainer.transform.SetParent(navPanel.transform, false);

            RectTransform buttonsRect = buttonsContainer.AddComponent<RectTransform>();
            buttonsRect.anchorMin = new Vector2(0, 0);
            buttonsRect.anchorMax = new Vector2(1, 0.7f);
            buttonsRect.offsetMin = Vector2.zero;
            buttonsRect.offsetMax = Vector2.zero;

            HorizontalLayoutGroup hLayout = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
            hLayout.childAlignment = TextAnchor.MiddleCenter;
            hLayout.childControlWidth = true;
            hLayout.childControlHeight = true;
            hLayout.spacing = 20f;
            hLayout.padding = new RectOffset(20, 20, 20, 20);

            // Back Button
            CreateSecondaryButton(buttonsContainer.transform, "BackButton", "ATRÁS");

            // Next Button
            CreateCyanButton(buttonsContainer.transform, "NextButton", "SIGUIENTE");

            // Skip Button (top-right corner)
            CreateSkipButtonInPanel(navPanel.transform);
        }

        private static void CreateSecondaryButton(Transform parent, string name, string text)
        {
            GameObject btn = new GameObject(name);
            btn.transform.SetParent(parent, false);

            Image bg = btn.AddComponent<Image>();
            bg.sprite = WhiteSprite;
            bg.color = new Color(0.2f, 0.2f, 0.25f, 0.9f);

            Button button = btn.AddComponent<Button>();
            button.targetGraphic = bg;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.35f, 0.9f);
            colors.pressedColor = new Color(0.15f, 0.15f, 0.2f, 1f);
            colors.selectedColor = new Color(0.3f, 0.3f, 0.35f, 0.9f);
            colors.disabledColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.15f;
            button.colors = colors;

            LayoutElement layout = btn.AddComponent<LayoutElement>();
            layout.preferredHeight = BUTTON_HEIGHT;
            layout.flexibleWidth = 1f;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btn.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.font = DefaultFont;
            btnText.text = text;
            btnText.fontSize = 20;
            btnText.fontStyle = FontStyles.Bold;
            btnText.color = TextWhite;
            btnText.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateCyanButton(Transform parent, string name, string text)
        {
            GameObject btn = new GameObject(name);
            btn.transform.SetParent(parent, false);

            Image bg = btn.AddComponent<Image>();
            bg.sprite = WhiteSprite;
            bg.color = CyanNeon;

            Button button = btn.AddComponent<Button>();
            button.targetGraphic = bg;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0f, 0.8f, 0.8f, 0.9f);
            colors.pressedColor = new Color(0f, 0.6f, 0.6f, 1f);
            colors.selectedColor = new Color(0f, 0.8f, 0.8f, 0.9f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.15f;
            button.colors = colors;

            LayoutElement layout = btn.AddComponent<LayoutElement>();
            layout.preferredHeight = BUTTON_HEIGHT;
            layout.flexibleWidth = 1f;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btn.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.font = DefaultFont;
            btnText.text = text;
            btnText.fontSize = 24;
            btnText.fontStyle = FontStyles.Bold;
            btnText.color = DarkNavy;
            btnText.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateSkipButtonInPanel(Transform parent)
        {
            GameObject skipBtn = new GameObject("SkipButton");
            skipBtn.transform.SetParent(parent, false);

            RectTransform skipRect = skipBtn.AddComponent<RectTransform>();
            skipRect.anchorMin = new Vector2(1, 1);
            skipRect.anchorMax = new Vector2(1, 1);
            skipRect.pivot = new Vector2(1, 1);
            skipRect.sizeDelta = new Vector2(120, 40);
            skipRect.anchoredPosition = new Vector2(-10, -10);

            Button button = skipBtn.AddComponent<Button>();

            Image bg = skipBtn.AddComponent<Image>();
            bg.sprite = WhiteSprite;
            bg.color = new Color(0, 0, 0, 0.3f);

            button.targetGraphic = bg;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.7f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.15f;
            button.colors = colors;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(skipBtn.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.font = DefaultFont;
            btnText.text = "SALTAR";
            btnText.fontSize = 16;
            btnText.fontStyle = FontStyles.Bold;
            btnText.color = TextGray;
            btnText.alignment = TextAlignmentOptions.Center;
        }

        private static void BuildWelcomeGiftBlocker(Canvas canvas)
        {
            // Blocker background
            GameObject blocker = new GameObject("WelcomeGiftBlocker");
            blocker.transform.SetParent(canvas.transform, false);
            blocker.SetActive(false); // Hidden by default

            RectTransform blockerRect = blocker.AddComponent<RectTransform>();
            blockerRect.anchorMin = Vector2.zero;
            blockerRect.anchorMax = Vector2.one;
            blockerRect.sizeDelta = Vector2.zero;

            Image blockerBg = blocker.AddComponent<Image>();
            blockerBg.sprite = WhiteSprite;
            blockerBg.color = new Color(0, 0, 0, 0.85f);

            // Gift card
            GameObject giftCard = new GameObject("GiftCard");
            giftCard.transform.SetParent(blocker.transform, false);

            RectTransform cardRect = giftCard.AddComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(SCREEN_WIDTH - (PADDING * 2), 0);

            Image cardBg = giftCard.AddComponent<Image>();
            cardBg.sprite = WhiteSprite;
            cardBg.color = CardBackground;

            Outline outline = giftCard.AddComponent<Outline>();
            outline.effectColor = GoldPremium;
            outline.effectDistance = new Vector2(3, -3);

            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(giftCard.transform, false);

            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.sizeDelta = Vector2.zero;

            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.spacing = ELEMENT_SPACING;
            layout.padding = new RectOffset((int)CARD_PADDING, (int)CARD_PADDING, (int)CARD_PADDING, (int)CARD_PADDING);

            ContentSizeFitter fitter = giftCard.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Title
            CreateGiftTitle(content.transform);
            CreateSpacer(content.transform, 10f);

            // Rewards
            CreateRewardsRow(content.transform);
            CreateSpacer(content.transform, 10f);

            // Claim button
            CreateClaimButton(content.transform);
        }

        private static void CreateGiftTitle(Transform parent)
        {
            GameObject title = new GameObject("Title");
            title.transform.SetParent(parent, false);

            LayoutElement layout = title.AddComponent<LayoutElement>();
            layout.preferredHeight = 60;

            TextMeshProUGUI text = title.AddComponent<TextMeshProUGUI>();
            text.font = DefaultFont;
            text.text = "¡REGALO DE BIENVENIDA!";
            text.fontSize = 32;
            text.fontStyle = FontStyles.Bold;
            text.color = GoldPremium;
            text.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateRewardsRow(Transform parent)
        {
            GameObject row = new GameObject("RewardsRow");
            row.transform.SetParent(parent, false);

            LayoutElement rowLayout = row.AddComponent<LayoutElement>();
            rowLayout.preferredHeight = 100;

            HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 30;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;

            CreateRewardItem(row.transform, ICON_COINS_PATH, "+100", new Color(1f, 0.85f, 0.3f, 1f));
            CreateRewardItem(row.transform, ICON_GEMS_PATH, "+50", new Color(0.4f, 0.8f, 1f, 1f));
            CreateRewardItem(row.transform, ICON_XP_PATH, "+25", new Color(1f, 0.5f, 0.2f, 1f));
        }

        private static void CreateRewardItem(Transform parent, string iconPath, string amount, Color color)
        {
            GameObject item = new GameObject("RewardItem");
            item.transform.SetParent(parent, false);

            RectTransform itemRect = item.AddComponent<RectTransform>();

            VerticalLayoutGroup layout = item.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 5;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;

            // Icon
            GameObject icon = new GameObject("Icon");
            icon.transform.SetParent(item.transform, false);

            LayoutElement iconLayout = icon.AddComponent<LayoutElement>();
            iconLayout.preferredWidth = 60;
            iconLayout.preferredHeight = 60;

            Image iconImage = icon.AddComponent<Image>();
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            if (sprite != null)
            {
                iconImage.sprite = sprite;
                iconImage.preserveAspect = true;
            }

            // Amount
            GameObject amountObj = new GameObject("Amount");
            amountObj.transform.SetParent(item.transform, false);

            TextMeshProUGUI amountText = amountObj.AddComponent<TextMeshProUGUI>();
            amountText.font = DefaultFont;
            amountText.text = amount;
            amountText.fontSize = 20;
            amountText.fontStyle = FontStyles.Bold;
            amountText.color = color;
            amountText.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateClaimButton(Transform parent)
        {
            GameObject btn = new GameObject("ClaimButton");
            btn.transform.SetParent(parent, false);

            LayoutElement layout = btn.AddComponent<LayoutElement>();
            layout.preferredHeight = BUTTON_HEIGHT;

            Image bg = btn.AddComponent<Image>();
            bg.sprite = WhiteSprite;
            bg.color = GoldPremium;

            Button button = btn.AddComponent<Button>();
            button.targetGraphic = bg;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btn.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.font = DefaultFont;
            text.text = "RECLAMAR Y JUGAR!";
            text.fontSize = 24;
            text.fontStyle = FontStyles.Bold;
            text.color = DarkNavy;
            text.alignment = TextAlignmentOptions.Center;
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
