using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using UnityEngine.EventSystems;

namespace DigitPark.Editor
{
    /// <summary>
    /// Cash Battle Onboarding UI Builder - Premium GOLD theme
    /// Emphasizes REAL MONEY gameplay, 18+ verification, and transparent rules
    /// Flow: AgeVerification → Cash Battle Hub → Deposit → Play → Win
    /// </summary>
    public static class CashBattleOnboardingUIBuilder
    {
        private const float SCREEN_WIDTH = 1080f;
        private const float SCREEN_HEIGHT = 1920f;

        // Colors - GOLD Premium Theme (Cash Battle)
        private static readonly Color GoldPremium = new Color(1f, 0.843f, 0f, 1f); // #FFD700
        private static readonly Color DarkBrown = new Color(0.039f, 0.031f, 0.02f, 1f); // #0A0805
        private static readonly Color CardBackground = new Color(0.2f, 0.133f, 0.067f, 0.95f);
        private static readonly Color GreenSuccess = new Color(0f, 1f, 0.5f, 1f); // Neon green for "WIN"
        private static readonly Color TextWhite = Color.white;
        private static readonly Color TextGray = new Color(0.7f, 0.7f, 0.7f, 1f);

        // Slide accent colors (GOLD variations)
        private static readonly Color[] SlideColors = new Color[]
        {
            new Color(1f, 0.843f, 0f, 1f),     // Slide 1: Pure Gold
            new Color(1f, 0.647f, 0f, 1f),     // Slide 2: Orange Gold
            new Color(1f, 0.549f, 0f, 1f),     // Slide 3: Dark Orange Gold
            new Color(0.85f, 0.647f, 0.125f, 1f), // Slide 4: Dark Gold
            new Color(0f, 1f, 0.5f, 1f)        // Slide 5: Neon Green (WIN!)
        };

        // Paths
        private const string WHITE_SPRITE_PATH = "Assets/_Project/Textures/UI/WhiteSquare.png";
        private const string FONT_ASSET_PATH = "Assets/_Project/Art/Fonts/Rajdhani/Rajdhani-Medium SDF.asset";

        // Cash Battle Icons
        private const string VERIFICATION_ICON = "Assets/_Project/Art/Icons/CashBattle/UI/VerificationIcon.png";
        private const string WALLET_ICON = "Assets/_Project/Art/Icons/CashBattle/Wallet/DepositIcon.png";
        private const string TROPHY_ICON = "Assets/_Project/Art/Icons/CashBattle/UI/TrophyIcon.png";
        private const string CASH_ICON = "Assets/_Project/Art/Icons/CashBattle/Wallet/CashIcon.png";

        // Spacing
        private const float PADDING = 30f;
        private const float CARD_PADDING = 40f;
        private const float ELEMENT_SPACING = 20f;
        private const float BUTTON_HEIGHT = 70f;
        private const float NUMBER_SIZE = 150f;

        private static Sprite WhiteSprite => AssetDatabase.LoadAssetAtPath<Sprite>(WHITE_SPRITE_PATH);
        private static TMP_FontAsset DefaultFont => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_ASSET_PATH);

        [MenuItem("DigitPark/UI Builders/Onboarding/Cash Battle Onboarding", false, 302)]
        public static void RebuildCashBattleOnboarding()
        {
            try
            {
                if (WhiteSprite == null || DefaultFont == null)
                {
                    Debug.LogError("❌ Missing prerequisites! Check WhiteSquare.png and Font");
                    return;
                }

                Debug.Log("💰 Starting Cash Battle Onboarding UI Build (GOLD PREMIUM)...");

                // Clean entire scene first
                CleanEntireScene();

                // Create Canvas and EventSystem
                Canvas canvas = CreateCanvas();
                CreateEventSystem();

                // Build UI
                BuildBackground(canvas);
                BuildLogo(canvas);
                GameObject safeArea = BuildSlidesContainer(canvas);

                // Add and configure the Manager component
                SetupManager(canvas, safeArea);

                Canvas.ForceUpdateCanvases();

                Debug.Log("✅ Cash Battle Onboarding UI built successfully!");
                Debug.Log("💡 Use 'DigitPark/UI Builders/Onboarding/Preview Slides' to test");
                EditorUtility.SetDirty(canvas.gameObject);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error in CashBattleOnboardingUIBuilder: {e.Message}\n{e.StackTrace}");
            }
        }

        private static void CleanEntireScene()
        {
            // Find all root GameObjects in the scene
            var rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

            foreach (var obj in rootObjects)
            {
                // Keep only the Main Camera
                if (obj.name != "Main Camera")
                {
                    Object.DestroyImmediate(obj);
                }
            }

            Debug.Log("🧹 Scene cleaned completely");
        }

        private static Canvas CreateCanvas()
        {
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(SCREEN_WIDTH, SCREEN_HEIGHT);
            scaler.matchWidthOrHeight = 0f;

            canvasObj.AddComponent<GraphicRaycaster>();

            Debug.Log("✅ Canvas created");
            return canvas;
        }

        private static void CreateEventSystem()
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            Debug.Log("✅ EventSystem created");
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
            image.color = DarkBrown;

            bg.transform.SetAsFirstSibling();
        }

        private static void BuildLogo(Canvas canvas)
        {
            GameObject logo = new GameObject("Logo");
            logo.transform.SetParent(canvas.transform, false);

            RectTransform rect = logo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1);
            rect.anchorMax = new Vector2(0.5f, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.sizeDelta = new Vector2(600, 150);
            rect.anchoredPosition = new Vector2(0, -60);

            TextMeshProUGUI text = logo.AddComponent<TextMeshProUGUI>();
            text.font = DefaultFont;
            text.text = "CASH BATTLE";
            text.fontSize = 64;
            text.fontStyle = FontStyles.Bold;
            text.color = GoldPremium;
            text.alignment = TextAlignmentOptions.Center;

            // Gold glow effect
            Outline outline = logo.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.647f, 0f, 0.5f);
            outline.effectDistance = new Vector2(2, -2);
        }

        private static GameObject BuildSlidesContainer(Canvas canvas)
        {
            GameObject safeArea = new GameObject("SafeArea");
            safeArea.transform.SetParent(canvas.transform, false);

            RectTransform safeRect = safeArea.AddComponent<RectTransform>();
            safeRect.anchorMin = Vector2.zero;
            safeRect.anchorMax = Vector2.one;
            safeRect.sizeDelta = Vector2.zero;

            GameObject container = new GameObject("SlidesContainer");
            container.transform.SetParent(safeArea.transform, false);

            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.sizeDelta = Vector2.zero;

            // Create 5 slides
            CreateSlide1_Welcome(container.transform);
            CreateSlide2_Verification(container.transform);
            CreateSlide3_Deposit(container.transform);
            CreateSlide4_Play(container.transform);
            CreateSlide5_Win(container.transform);

            // Create navigation panel
            CreateNavigationPanel(safeArea.transform);

            return safeArea;
        }

        private static void SetupManager(Canvas canvas, GameObject safeArea)
        {
            // Remove old manager if exists
            var oldManager = canvas.GetComponent<DigitPark.Managers.CashBattleOnboardingManager>();
            if (oldManager != null)
            {
                Object.DestroyImmediate(oldManager);
            }

            // Add new manager
            var manager = canvas.gameObject.AddComponent<DigitPark.Managers.CashBattleOnboardingManager>();

            // Auto-wire references using SerializedObject (works in edit mode)
            var serializedManager = new UnityEditor.SerializedObject(manager);

            // Find and assign SlidesContainer
            Transform slidesContainer = safeArea.transform.Find("SlidesContainer");
            if (slidesContainer != null)
            {
                serializedManager.FindProperty("slidesContainer").objectReferenceValue = slidesContainer;
            }

            // Find and assign navigation elements
            Transform navPanel = safeArea.transform.Find("NavigationPanel");
            if (navPanel != null)
            {
                Transform buttonsContainer = navPanel.Find("Buttons");
                if (buttonsContainer != null)
                {
                    Transform nextBtn = buttonsContainer.Find("NextButton");
                    if (nextBtn != null)
                    {
                        serializedManager.FindProperty("nextButton").objectReferenceValue = nextBtn.GetComponent<Button>();
                        serializedManager.FindProperty("nextButtonText").objectReferenceValue = nextBtn.GetComponentInChildren<TextMeshProUGUI>();
                    }

                    Transform backBtn = buttonsContainer.Find("BackButton");
                    if (backBtn != null)
                    {
                        serializedManager.FindProperty("backButton").objectReferenceValue = backBtn.GetComponent<Button>();
                    }
                }

                Transform skipBtn = navPanel.Find("SkipButton");
                if (skipBtn != null)
                {
                    serializedManager.FindProperty("skipButton").objectReferenceValue = skipBtn.GetComponent<Button>();
                }

                Transform dotsContainer = navPanel.Find("DotsContainer");
                if (dotsContainer != null)
                {
                    serializedManager.FindProperty("dotsContainer").objectReferenceValue = dotsContainer;
                }
            }

            // Set configuration
            serializedManager.FindProperty("totalSlides").intValue = 5;
            serializedManager.FindProperty("allowSkip").boolValue = true;
            serializedManager.FindProperty("transitionDuration").floatValue = 0.3f;

            serializedManager.ApplyModifiedProperties();

            Debug.Log("✅ CashBattleOnboardingManager added and configured!");
        }

        private static void CreateSlide1_Welcome(Transform parent)
        {
            Transform content = CreateSlideBase(parent, "Slide1", 1, out GameObject slideObj);

            // Create number divider OUTSIDE content (not in VerticalLayoutGroup)
            CreateSlideNumberDivider(slideObj.transform, 1, SlideColors[0]);
            CreateTitle(content, "¡BIENVENIDO A\nCASH BATTLE!", GoldPremium);
            CreateSpacer(content, 20f);
            CreateDescription(content, "La primera plataforma de competencias\ncon DINERO REAL en Digit Park");
            CreateSpacer(content, 10f);
            CreateHighlightText(content, "💰 GANA DINERO REAL JUGANDO 💰", GreenSuccess);
            CreateSpacer(content, 10f);
            CreateDescription(content, "• Competencias 1v1 desde $1 USD");
            CreateDescription(content, "• Torneos con premios garantizados");
            CreateDescription(content, "• Retiros rápidos y seguros");
        }

        private static void CreateSlide2_Verification(Transform parent)
        {
            Transform content = CreateSlideBase(parent, "Slide2", 2, out GameObject slideObj);

            CreateSlideNumberDivider(slideObj.transform, 2, SlideColors[1]);
            CreateTitle(content, "VERIFICA TU EDAD\n(18+ REQUERIDO)", GoldPremium);
            CreateSpacer(content, 20f);

            // Icon
            Sprite verificationIcon = AssetDatabase.LoadAssetAtPath<Sprite>(VERIFICATION_ICON);
            if (verificationIcon != null)
            {
                CreateIcon(content, verificationIcon, 120f);
                CreateSpacer(content, 10f);
            }

            CreateDescription(content, "Para jugar con dinero real, debes:");
            CreateSpacer(content, 10f);
            CreateBulletPoint(content, "✓ Ser mayor de 18 años");
            CreateBulletPoint(content, "✓ Verificar tu identidad con Triump™");
            CreateBulletPoint(content, "✓ Confirmar tu información bancaria");
            CreateSpacer(content, 20f);
            CreateHighlightText(content, "Proceso 100% seguro y confidencial", TextGray);
        }

        private static void CreateSlide3_Deposit(Transform parent)
        {
            Transform content = CreateSlideBase(parent, "Slide3", 3, out GameObject slideObj);

            CreateSlideNumberDivider(slideObj.transform, 3, SlideColors[2]);
            CreateTitle(content, "DEPOSITA FONDOS\nEN TU WALLET", GoldPremium);
            CreateSpacer(content, 20f);

            // Icon
            Sprite walletIcon = AssetDatabase.LoadAssetAtPath<Sprite>(WALLET_ICON);
            if (walletIcon != null)
            {
                CreateIcon(content, walletIcon, 120f);
                CreateSpacer(content, 10f);
            }

            CreateDescription(content, "Añade dinero a tu cuenta fácilmente:");
            CreateSpacer(content, 10f);
            CreateBulletPoint(content, "💳 Tarjeta de crédito/débito");
            CreateBulletPoint(content, "🏦 Transferencia bancaria");
            CreateBulletPoint(content, "📱 Métodos de pago locales");
            CreateSpacer(content, 20f);
            CreateHighlightText(content, "Depósito mínimo: $5 USD", GoldPremium);
            CreateSpacer(content, 5f);
            CreateDescription(content, "🎁 Bonos de bienvenida disponibles");
        }

        private static void CreateSlide4_Play(Transform parent)
        {
            Transform content = CreateSlideBase(parent, "Slide4", 4, out GameObject slideObj);

            CreateSlideNumberDivider(slideObj.transform, 4, SlideColors[3]);
            CreateTitle(content, "ELIGE TU JUEGO\nY APUESTA", GoldPremium);
            CreateSpacer(content, 20f);

            // Icon
            Sprite trophyIcon = AssetDatabase.LoadAssetAtPath<Sprite>(TROPHY_ICON);
            if (trophyIcon != null)
            {
                CreateIcon(content, trophyIcon, 120f);
                CreateSpacer(content, 10f);
            }

            CreateDescription(content, "Dos formas de competir:");
            CreateSpacer(content, 10f);
            CreateHighlightText(content, "⚔️ COMPETENCIAS 1v1", new Color(1f, 0.647f, 0f, 1f));
            CreateBulletPoint(content, "• Matchmaking basado en habilidad (MMR)");
            CreateBulletPoint(content, "• Apuestas desde $1 hasta $250 USD");
            CreateBulletPoint(content, "• El ganador se lleva el 80%");
            CreateSpacer(content, 10f);
            CreateHighlightText(content, "🏆 TORNEOS", new Color(1f, 0.647f, 0f, 1f));
            CreateBulletPoint(content, "• Hasta 256 jugadores");
            CreateBulletPoint(content, "• Premios garantizados");
            CreateBulletPoint(content, "• Sistema de brackets profesional");
        }

        private static void CreateSlide5_Win(Transform parent)
        {
            Transform content = CreateSlideBase(parent, "Slide5", 5, out GameObject slideObj);

            CreateSlideNumberDivider(slideObj.transform, 5, SlideColors[4]);
            CreateTitle(content, "¡GANA Y RETIRA\nTU DINERO!", GreenSuccess);
            CreateSpacer(content, 20f);

            // Icon
            Sprite cashIcon = AssetDatabase.LoadAssetAtPath<Sprite>(CASH_ICON);
            if (cashIcon != null)
            {
                CreateIcon(content, cashIcon, 120f);
                CreateSpacer(content, 10f);
            }

            CreateHighlightText(content, "💸 RETIROS RÁPIDOS Y SEGUROS 💸", GreenSuccess);
            CreateSpacer(content, 20f);
            CreateBulletPoint(content, "✓ Retiro mínimo: $10 USD");
            CreateBulletPoint(content, "✓ Máximo: $500 USD por retiro");
            CreateBulletPoint(content, "✓ Procesamiento en 1-3 días hábiles");
            CreateBulletPoint(content, "✓ Directo a tu cuenta bancaria");
            CreateSpacer(content, 20f);
            CreateDescription(content, "Rastrea tus ganancias en tiempo real\nen el historial de transacciones");
            CreateSpacer(content, 10f);
            CreateHighlightText(content, "🎮 ¡EMPIEZA A GANAR HOY! 🎮", GoldPremium);
        }

        private static Transform CreateSlideBase(Transform parent, string name, int index, out GameObject slideObj)
        {
            GameObject slide = new GameObject(name);
            slide.transform.SetParent(parent, false);
            slideObj = slide; // Return slide for external access

            RectTransform slideRect = slide.AddComponent<RectTransform>();
            slideRect.anchorMin = Vector2.zero;
            slideRect.anchorMax = Vector2.one;
            slideRect.sizeDelta = Vector2.zero;

            // Card container
            GameObject card = new GameObject("Card");
            card.transform.SetParent(slide.transform, false);

            RectTransform cardRect = card.AddComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(SCREEN_WIDTH - (PADDING * 2), 0);
            cardRect.anchoredPosition = new Vector2(0, 0); // Centrado perfectamente

            Image cardBg = card.AddComponent<Image>();
            cardBg.sprite = WhiteSprite;
            cardBg.color = CardBackground;

            // Gold neon border
            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = GoldPremium;
            outline.effectDistance = new Vector2(3, -3);

            // Content container with layout
            GameObject content = new GameObject("Content");
            content.transform.SetParent(card.transform, false);

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

            ContentSizeFitter fitter = card.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Set slide active state
            slide.SetActive(index == 1); // Only first slide active by default

            // Return content transform directly
            return content.transform;
        }

        private static void CreateSlideNumberDivider(Transform slideParent, int number, Color accentColor)
        {
            // Container for divider + number (OUTSIDE VerticalLayoutGroup)
            GameObject dividerContainer = new GameObject("DividerContainer");
            dividerContainer.transform.SetParent(slideParent, false);

            RectTransform containerRect = dividerContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            containerRect.sizeDelta = new Vector2(SCREEN_WIDTH, NUMBER_SIZE);
            containerRect.anchoredPosition = new Vector2(0, 200); // Default position (user can move)

            // Horizontal divider line
            GameObject divider = new GameObject("Divider");
            divider.transform.SetParent(dividerContainer.transform, false);

            RectTransform dividerRect = divider.AddComponent<RectTransform>();
            dividerRect.anchorMin = new Vector2(0.5f, 0.5f);
            dividerRect.anchorMax = new Vector2(0.5f, 0.5f);
            dividerRect.pivot = new Vector2(0.5f, 0.5f);
            dividerRect.sizeDelta = new Vector2(SCREEN_WIDTH - (PADDING * 4), 4f);

            Image dividerImage = divider.AddComponent<Image>();
            dividerImage.sprite = WhiteSprite;
            dividerImage.color = accentColor;

            // Colored square (on top of divider)
            GameObject square = new GameObject("Square");
            square.transform.SetParent(dividerContainer.transform, false);

            RectTransform squareRect = square.AddComponent<RectTransform>();
            squareRect.anchorMin = new Vector2(0.5f, 0.5f);
            squareRect.anchorMax = new Vector2(0.5f, 0.5f);
            squareRect.pivot = new Vector2(0.5f, 0.5f);
            squareRect.sizeDelta = new Vector2(NUMBER_SIZE, NUMBER_SIZE);

            Image squareImage = square.AddComponent<Image>();
            squareImage.sprite = WhiteSprite;
            squareImage.color = accentColor;

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
            text.color = DarkBrown;
            text.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateSlideNumber(Transform parent, int number, Color accentColor)
        {
            GameObject numberContainer = new GameObject("NumberContainer");
            numberContainer.transform.SetParent(parent, false);

            LayoutElement layout = numberContainer.AddComponent<LayoutElement>();
            layout.preferredHeight = NUMBER_SIZE;

            GameObject square = new GameObject("Square");
            square.transform.SetParent(numberContainer.transform, false);

            RectTransform squareRect = square.AddComponent<RectTransform>();
            squareRect.anchorMin = new Vector2(0.5f, 0.5f);
            squareRect.anchorMax = new Vector2(0.5f, 0.5f);
            squareRect.pivot = new Vector2(0.5f, 0.5f);
            squareRect.sizeDelta = new Vector2(NUMBER_SIZE, NUMBER_SIZE);

            Image squareImage = square.AddComponent<Image>();
            squareImage.sprite = WhiteSprite;
            squareImage.color = accentColor;

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
            text.color = DarkBrown;
            text.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateIcon(Transform parent, Sprite icon, float size)
        {
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(parent, false);

            RectTransform rect = iconObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(size, size);

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.sprite = icon;
            iconImage.preserveAspect = true;

            LayoutElement layout = iconObj.AddComponent<LayoutElement>();
            layout.preferredHeight = size;
        }

        private static void CreateTitle(Transform parent, string text, Color color)
        {
            GameObject title = new GameObject("Title");
            title.transform.SetParent(parent, false);

            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.font = DefaultFont;
            titleText.text = text;
            titleText.fontSize = 32;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = color;
            titleText.alignment = TextAlignmentOptions.Center;

            LayoutElement layout = title.AddComponent<LayoutElement>();
            layout.preferredHeight = 80;
        }

        private static void CreateDescription(Transform parent, string text)
        {
            GameObject desc = new GameObject("Description");
            desc.transform.SetParent(parent, false);

            TextMeshProUGUI descText = desc.AddComponent<TextMeshProUGUI>();
            descText.font = DefaultFont;
            descText.text = text;
            descText.fontSize = 18;
            descText.color = TextWhite;
            descText.alignment = TextAlignmentOptions.Center;
            descText.enableWordWrapping = true;

            LayoutElement layout = desc.AddComponent<LayoutElement>();
            layout.preferredHeight = 30;
        }

        private static void CreateBulletPoint(Transform parent, string text)
        {
            GameObject bullet = new GameObject("BulletPoint");
            bullet.transform.SetParent(parent, false);

            TextMeshProUGUI bulletText = bullet.AddComponent<TextMeshProUGUI>();
            bulletText.font = DefaultFont;
            bulletText.text = text;
            bulletText.fontSize = 16;
            bulletText.color = TextWhite;
            bulletText.alignment = TextAlignmentOptions.Left;
            bulletText.enableWordWrapping = true;

            LayoutElement layout = bullet.AddComponent<LayoutElement>();
            layout.preferredHeight = 25;
        }

        private static void CreateHighlightText(Transform parent, string text, Color color)
        {
            GameObject highlight = new GameObject("Highlight");
            highlight.transform.SetParent(parent, false);

            TextMeshProUGUI highlightText = highlight.AddComponent<TextMeshProUGUI>();
            highlightText.font = DefaultFont;
            highlightText.text = text;
            highlightText.fontSize = 20;
            highlightText.fontStyle = FontStyles.Bold;
            highlightText.color = color;
            highlightText.alignment = TextAlignmentOptions.Center;

            LayoutElement layout = highlight.AddComponent<LayoutElement>();
            layout.preferredHeight = 35;
        }

        private static void CreateLegalText(Transform parent, string text)
        {
            GameObject legal = new GameObject("LegalText");
            legal.transform.SetParent(parent, false);

            TextMeshProUGUI legalText = legal.AddComponent<TextMeshProUGUI>();
            legalText.font = DefaultFont;
            legalText.text = text;
            legalText.fontSize = 11;
            legalText.color = TextGray;
            legalText.alignment = TextAlignmentOptions.Center;
            legalText.enableWordWrapping = true;

            LayoutElement layout = legal.AddComponent<LayoutElement>();
            layout.preferredHeight = 25;
        }

        private static void CreateSpacer(Transform parent, float height)
        {
            GameObject spacer = new GameObject("Spacer");
            spacer.transform.SetParent(parent, false);

            LayoutElement layout = spacer.AddComponent<LayoutElement>();
            layout.preferredHeight = height;
        }

        private static void CreateNavigationPanel(Transform parent)
        {
            GameObject navPanel = new GameObject("NavigationPanel");
            navPanel.transform.SetParent(parent, false);

            RectTransform panelRect = navPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0);
            panelRect.anchorMax = new Vector2(0.5f, 0);
            panelRect.pivot = new Vector2(0.5f, 0);
            panelRect.sizeDelta = new Vector2(SCREEN_WIDTH - (PADDING * 2), 220);
            panelRect.anchoredPosition = new Vector2(0, 40);

            // Navigation Dots Container
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

            // Back Button (will be hidden by Manager initially)
            CreateSecondaryButton(buttonsContainer.transform, "BackButton", "ATRÁS");

            // Next/Start Button (Main CTA)
            CreateGoldButton(buttonsContainer.transform, "NextButton", "SIGUIENTE");

            // Skip Button (top-right corner of panel)
            CreateSkipButton(navPanel.transform);

            // Legal Text (centered below buttons) - parent is safeArea
            CreateGlobalLegalText(parent);
        }

        private static void CreateGlobalLegalText(Transform parent)
        {
            GameObject legalContainer = new GameObject("LegalTextContainer");
            legalContainer.transform.SetParent(parent, false);

            RectTransform rect = legalContainer.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0);
            rect.anchorMax = new Vector2(0.5f, 0);
            rect.pivot = new Vector2(0.5f, 0);
            rect.sizeDelta = new Vector2(SCREEN_WIDTH - (PADDING * 2), 40);
            rect.anchoredPosition = new Vector2(0, 10); // Just above bottom

            TextMeshProUGUI legalText = legalContainer.AddComponent<TextMeshProUGUI>();
            legalText.font = DefaultFont;
            legalText.text = "Powered by Triump™ • Juego responsable • Solo mayores de 18 años";
            legalText.fontSize = 11;
            legalText.color = TextGray;
            legalText.alignment = TextAlignmentOptions.Center;
            legalText.enableWordWrapping = true;
        }

        private static void CreateSecondaryButton(Transform parent, string name, string text)
        {
            GameObject btn = new GameObject(name);
            btn.transform.SetParent(parent, false);

            Image bg = btn.AddComponent<Image>();
            bg.sprite = WhiteSprite;
            bg.color = new Color(0.2f, 0.2f, 0.25f, 0.9f); // Dark gray

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

        private static void CreateSkipButton(Transform parent)
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

            // Transparent background
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

        private static void CreateGoldButton(Transform parent, string name, string text)
        {
            GameObject btn = new GameObject(name);
            btn.transform.SetParent(parent, false);

            Image bg = btn.AddComponent<Image>();
            bg.sprite = WhiteSprite;
            bg.color = GoldPremium;

            Button button = btn.AddComponent<Button>();
            button.targetGraphic = bg;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.647f, 0f, 0.9f);
            colors.pressedColor = new Color(1f, 0.549f, 0f, 1f);
            colors.selectedColor = new Color(1f, 0.647f, 0f, 0.9f);
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
            btnText.color = DarkBrown;
            btnText.alignment = TextAlignmentOptions.Center;
        }
    }
}
