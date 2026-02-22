using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Modern Login UI builder with neon card design
    /// </summary>
    public static class LoginUIBuilder
    {
        private const float SCREEN_WIDTH = 1080f;
        private const float SCREEN_HEIGHT = 1920f;

        // Colors
        private static readonly Color CyanNeon = new Color(0f, 1f, 1f, 1f); // #00FFFF
        private static readonly Color DarkNavy = new Color(0.039f, 0.055f, 0.153f, 1f); // #0A0E27
        private static readonly Color CardBackground = new Color(0.125f, 0.188f, 0.376f, 0.95f); // #202860 con alpha
        private static readonly Color InputBackground = new Color(0.078f, 0.11f, 0.22f, 1f); // Más oscuro para inputs
        private static readonly Color TextWhite = Color.white;
        private static readonly Color TextGray = new Color(0.7f, 0.7f, 0.7f, 1f);

        // Paths
        private const string WHITE_SPRITE_PATH = "Assets/_Project/Textures/UI/WhiteSquare.png";
        private const string FONT_ASSET_PATH = "Assets/_Project/Art/Fonts/Rajdhani/Rajdhani-Medium SDF.asset";
        private const string EYE_OPEN_PATH = "Assets/_Project/Art/Icons/Navigation/Actions/EyeOpenNeon.png";
        private const string EYE_CLOSED_PATH = "Assets/_Project/Art/Icons/Navigation/Actions/EyeClosedNeon.png";
        private const string GOOGLE_ICON_PATH = "Assets/_Project/Art/Icons/Auth/Google/google_logo_official.png";
        private const string APPLE_ICON_PATH = "Assets/_Project/Art/Icons/Auth/Apple/apple_logo_black.png";

        // Prefab paths
        private const string ERROR_PANEL_PREFAB = "Assets/_Project/Prefabs/Common/ErrorPanel.prefab";
        private const string BACK_BUTTON_PREFAB = "Assets/_Project/Prefabs/Common/BackButton.prefab";

        // Spacing
        private const float PADDING = 30f;
        private const float CARD_PADDING = 40f;
        private const float ELEMENT_SPACING = 20f;
        private const float INPUT_HEIGHT = 90f;
        private const float BUTTON_HEIGHT = 120f;
        private const float SOCIAL_BUTTON_HEIGHT = 70f; // Más grandes según guidelines

        private static Sprite WhiteSprite => AssetDatabase.LoadAssetAtPath<Sprite>(WHITE_SPRITE_PATH);
        private static TMP_FontAsset DefaultFont => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_ASSET_PATH);
        private static Sprite EyeOpenIcon => AssetDatabase.LoadAssetAtPath<Sprite>(EYE_OPEN_PATH);
        private static Sprite EyeClosedIcon => AssetDatabase.LoadAssetAtPath<Sprite>(EYE_CLOSED_PATH);
        private static Sprite GoogleIcon => AssetDatabase.LoadAssetAtPath<Sprite>(GOOGLE_ICON_PATH);
        private static Sprite AppleIcon => AssetDatabase.LoadAssetAtPath<Sprite>(APPLE_ICON_PATH);

        [MenuItem("DigitPark/UI Builders/Auth/Login", false, 200)]
        public static void RebuildLoginScene()
        {
            try
            {
                // Verify prerequisites
                if (WhiteSprite == null || DefaultFont == null)
                {
                    Debug.LogError("❌ Missing prerequisites! Run 'Create White Sprite' first.");
                    return;
                }

                Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
                if (canvas == null)
                {
                    Debug.LogError("❌ No Canvas found in scene.");
                    return;
                }

                Debug.Log("🎨 Starting Login UI Rebuild...");

                CleanupOldUI();

                // Clean existing UI
                CleanExistingUI(canvas);

                // Build UI
                BuildBackground(canvas);
                BuildBackButton(canvas);
                BuildLogo(canvas);
                BuildLoginCard(canvas);
                BuildFooter(canvas);
                BuildRegisterButton(canvas); // Botón independiente, movible
                BuildLoadingPanel(canvas);
                BuildErrorPanel(canvas);

                // Force layout update
                Canvas.ForceUpdateCanvases();

                AutoAssigners.LoginReferenceAssigner.RunAutoAssign();
                Debug.Log("✅ Login UI rebuilt successfully!");
                EditorUtility.SetDirty(canvas.gameObject);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error in LoginUIBuilder: {e.Message}\n{e.StackTrace}");
            }
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

        private static void BuildLogo(Canvas canvas)
        {
            GameObject logo = new GameObject("Logo");
            logo.transform.SetParent(canvas.transform, false);

            RectTransform rect = logo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1);
            rect.anchorMax = new Vector2(0.5f, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.sizeDelta = new Vector2(600, 120);
            rect.anchoredPosition = new Vector2(0, -80);

            TextMeshProUGUI text = logo.AddComponent<TextMeshProUGUI>();
            text.font = DefaultFont;
            text.text = "Digit Park";
            text.fontSize = 122;
            text.fontStyle = FontStyles.Bold;
            text.color = CyanNeon;
            text.alignment = TextAlignmentOptions.Center;
        }

        private static void BuildLoginCard(Canvas canvas)
        {
            // Card container
            GameObject card = new GameObject("LoginCard");
            card.transform.SetParent(canvas.transform, false);

            RectTransform cardRect = card.AddComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(SCREEN_WIDTH - (PADDING * 2), 0);
            cardRect.anchoredPosition = new Vector2(0, 100); // Subido para dejar espacio al footer

            // Card background with neon border
            Image cardBg = card.AddComponent<Image>();
            cardBg.sprite = WhiteSprite;
            cardBg.color = CardBackground;

            // Neon border
            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = CyanNeon;
            outline.effectDistance = new Vector2(3, -3);

            // Content container
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

            // Build card content
            CreateTitle(content.transform, "INICIAR SESIÓN");
            CreateInputField(content.transform, "EmailInput", "Email", false);
            CreateInputField(content.transform, "PasswordInput", "Contraseña", true);
            CreateCenteredLink(content.transform, "ForgotPasswordButton", "¿Olvidaste tu contraseña?");
            CreateCenteredCheckbox(content.transform);
            CreatePrimaryButton(content.transform, "LoginButton", "Iniciar sesión");
            CreateSpacer(content.transform, 10f); // Espacio extra antes de botones sociales
            CreateSocialButton(content.transform, "GoogleButton", "Sign in with Google", GoogleIcon, Color.white, Color.black);
            CreateSocialButton(content.transform, "AppleButton", "Sign in with Apple", AppleIcon, Color.black, Color.white);
        }

        private static void BuildFooter(Canvas canvas)
        {
            GameObject footer = new GameObject("Footer");
            footer.transform.SetParent(canvas.transform, false);

            RectTransform rect = footer.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0);
            rect.anchorMax = new Vector2(0.5f, 0);
            rect.pivot = new Vector2(0.5f, 0);
            rect.sizeDelta = new Vector2(400, 50);
            rect.anchoredPosition = new Vector2(0, 190); // Arriba del botón "Crear cuenta"

            // "¿No tienes cuenta?"
            TextMeshProUGUI question = footer.AddComponent<TextMeshProUGUI>();
            question.font = DefaultFont;
            question.text = "¿No tienes cuenta?";
            question.fontSize = 32;
            question.fontStyle = FontStyles.Bold;
            question.color = TextGray;
            question.alignment = TextAlignmentOptions.Center;
        }

        private static void BuildRegisterButton(Canvas canvas)
        {
            // "Crear una cuenta" button (independiente, movible manualmente)
            GameObject registerBtn = new GameObject("RegisterButton");
            registerBtn.transform.SetParent(canvas.transform, false);

            RectTransform rect = registerBtn.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0);
            rect.anchorMax = new Vector2(0.5f, 0);
            rect.pivot = new Vector2(0.5f, 0);
            rect.sizeDelta = new Vector2(SCREEN_WIDTH - (PADDING * 2), BUTTON_HEIGHT);
            rect.anchoredPosition = new Vector2(0, 60); // Posición inicial (movible desde Unity)

            Image btnBg = registerBtn.AddComponent<Image>();
            btnBg.sprite = WhiteSprite;
            btnBg.color = CyanNeon;

            Button btn = registerBtn.AddComponent<Button>();
            btn.targetGraphic = btnBg;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(registerBtn.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.font = DefaultFont;
            btnText.text = "Crear una cuenta";
            btnText.fontSize = 72;
            btnText.fontStyle = FontStyles.Bold;
            btnText.color = DarkNavy;
            btnText.alignment = TextAlignmentOptions.Center;

            Debug.Log("✅ RegisterButton creado. Ahora puedes moverlo manualmente en la escena.");
        }

        private static void CreateTitle(Transform parent, string text)
        {
            GameObject title = new GameObject("Title");
            title.transform.SetParent(parent, false);

            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.font = DefaultFont;
            titleText.text = text;
            titleText.fontSize = 72;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = CyanNeon;
            titleText.alignment = TextAlignmentOptions.Center;

            LayoutElement layout = title.AddComponent<LayoutElement>();
            layout.preferredHeight = 120;
        }

        private static void CreateInputField(Transform parent, string name, string placeholder, bool isPassword)
        {
            GameObject inputObj = new GameObject(name);
            inputObj.transform.SetParent(parent, false);

            // Background with neon border
            Image bg = inputObj.AddComponent<Image>();
            bg.sprite = WhiteSprite;
            bg.color = InputBackground;

            Outline outline = inputObj.AddComponent<Outline>();
            outline.effectColor = CyanNeon;
            outline.effectDistance = new Vector2(2, -2);

            LayoutElement inputLayout = inputObj.AddComponent<LayoutElement>();
            inputLayout.preferredHeight = INPUT_HEIGHT;

            // InputField component
            TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();
            inputField.textViewport = inputObj.GetComponent<RectTransform>();

            // Text area
            GameObject textArea = new GameObject("TextArea");
            textArea.transform.SetParent(inputObj.transform, false);

            RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(20, 0);
            textAreaRect.offsetMax = new Vector2(isPassword ? -100 : -20, 0);

            // Placeholder
            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(textArea.transform, false);

            RectTransform placeholderRect = placeholderObj.AddComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholderText.font = DefaultFont;
            placeholderText.text = placeholder;
            placeholderText.fontSize = 52;
            placeholderText.fontStyle = FontStyles.Bold;
            placeholderText.color = TextGray;
            placeholderText.alignment = TextAlignmentOptions.Left;

            // Input text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(textArea.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI inputText = textObj.AddComponent<TextMeshProUGUI>();
            inputText.font = DefaultFont;
            inputText.fontSize = 52;
            inputText.fontStyle = FontStyles.Bold;
            inputText.color = TextWhite;
            inputText.alignment = TextAlignmentOptions.Left;

            inputField.textViewport = textAreaRect;
            inputField.textComponent = inputText;
            inputField.placeholder = placeholderText;

            if (isPassword)
            {
                inputField.contentType = TMP_InputField.ContentType.Password;
                CreateEyeToggle(inputObj.transform, inputField);
            }
        }

        private static void CreateEyeToggle(Transform parent, TMP_InputField inputField)
        {
            GameObject eyeBtn = new GameObject("EyeToggle");
            eyeBtn.transform.SetParent(parent, false);

            RectTransform rect = eyeBtn.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 0.5f);
            rect.anchorMax = new Vector2(1, 0.5f);
            rect.pivot = new Vector2(1, 0.5f);
            rect.sizeDelta = new Vector2(80, 80);
            rect.anchoredPosition = new Vector2(-10, 0);

            Button btn = eyeBtn.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;

            Image eyeImage = eyeBtn.AddComponent<Image>();
            eyeImage.sprite = EyeClosedIcon;
            eyeImage.color = CyanNeon;

            // TODO: Add toggle functionality in runtime script
        }

        private static void CreateCenteredLink(Transform parent, string name, string text)
        {
            GameObject linkBtn = new GameObject(name);
            linkBtn.transform.SetParent(parent, false);

            Button btn = linkBtn.AddComponent<Button>();

            TextMeshProUGUI linkText = linkBtn.AddComponent<TextMeshProUGUI>();
            linkText.font = DefaultFont;
            linkText.text = text;
            linkText.fontSize = 38;
            linkText.fontStyle = FontStyles.Bold;
            linkText.color = CyanNeon;
            linkText.alignment = TextAlignmentOptions.Center;

            btn.targetGraphic = linkText;

            LayoutElement layout = linkBtn.AddComponent<LayoutElement>();
            layout.preferredHeight = 60;
        }

        private static void CreateCenteredCheckbox(Transform parent)
        {
            GameObject row = new GameObject("CheckboxRow");
            row.transform.SetParent(parent, false);

            HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 10f;

            LayoutElement rowLayout = row.AddComponent<LayoutElement>();
            rowLayout.preferredHeight = 70;

            // Checkbox
            GameObject checkbox = new GameObject("RememberCheckbox");
            checkbox.transform.SetParent(row.transform, false);

            Toggle toggle = checkbox.AddComponent<Toggle>();
            toggle.isOn = false;

            RectTransform checkRect = checkbox.GetComponent<RectTransform>();
            checkRect.sizeDelta = new Vector2(50, 50);

            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(checkbox.transform, false);

            RectTransform bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            Image bgImage = bg.AddComponent<Image>();
            bgImage.sprite = WhiteSprite;
            bgImage.color = InputBackground;

            Outline bgOutline = bg.AddComponent<Outline>();
            bgOutline.effectColor = CyanNeon;
            bgOutline.effectDistance = new Vector2(2, -2);

            // Checkmark
            GameObject checkmark = new GameObject("Checkmark");
            checkmark.transform.SetParent(bg.transform, false);

            RectTransform checkRect2 = checkmark.AddComponent<RectTransform>();
            checkRect2.anchorMin = Vector2.zero;
            checkRect2.anchorMax = Vector2.one;
            checkRect2.sizeDelta = new Vector2(-8, -8);

            Image checkImage = checkmark.AddComponent<Image>();
            checkImage.sprite = WhiteSprite;
            checkImage.color = CyanNeon;

            toggle.targetGraphic = bgImage;
            toggle.graphic = checkImage;

            // Label
            GameObject label = new GameObject("Label");
            label.transform.SetParent(row.transform, false);

            TextMeshProUGUI labelText = label.AddComponent<TextMeshProUGUI>();
            labelText.font = DefaultFont;
            labelText.text = "Recordarme";
            labelText.fontSize = 48;
            labelText.fontStyle = FontStyles.Bold;
            labelText.color = TextWhite;

            LayoutElement labelLayout = label.AddComponent<LayoutElement>();
            labelLayout.preferredWidth = 500;
            labelLayout.preferredHeight = 60;
        }

        private static void CreateSpacer(Transform parent, float height)
        {
            GameObject spacer = new GameObject("Spacer");
            spacer.transform.SetParent(parent, false);

            LayoutElement layout = spacer.AddComponent<LayoutElement>();
            layout.preferredHeight = height;
        }

        private static void CreatePrimaryButton(Transform parent, string name, string text)
        {
            GameObject button = new GameObject(name);
            button.transform.SetParent(parent, false);

            Image bg = button.AddComponent<Image>();
            bg.sprite = WhiteSprite;
            bg.color = CyanNeon;

            Button btn = button.AddComponent<Button>();
            btn.targetGraphic = bg;

            LayoutElement layout = button.AddComponent<LayoutElement>();
            layout.preferredHeight = BUTTON_HEIGHT;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(button.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.font = DefaultFont;
            buttonText.text = text;
            buttonText.fontSize = 72;
            buttonText.fontStyle = FontStyles.Bold;
            buttonText.color = DarkNavy;
            buttonText.alignment = TextAlignmentOptions.Center;
        }


        private static void CreateSocialButton(Transform parent, string name, string text, Sprite icon, Color bgColor, Color textColor)
        {
            GameObject button = new GameObject(name);
            button.transform.SetParent(parent, false);

            Image bg = button.AddComponent<Image>();
            bg.sprite = WhiteSprite;
            bg.color = bgColor;

            // NO outline/glow - viola las guidelines de Google/Apple

            Button btn = button.AddComponent<Button>();
            btn.targetGraphic = bg;

            LayoutElement layout = button.AddComponent<LayoutElement>();
            layout.preferredHeight = SOCIAL_BUTTON_HEIGHT;

            // Horizontal layout for icon + text (según guidelines: spacing ~24px)
            HorizontalLayoutGroup hLayout = button.AddComponent<HorizontalLayoutGroup>();
            hLayout.childAlignment = TextAnchor.MiddleCenter;
            hLayout.childControlWidth = false;
            hLayout.childControlHeight = false;
            hLayout.spacing = 24f; // Guidelines oficiales recomiendan 24px
            hLayout.padding = new RectOffset(24, 24, 0, 0); // Padding horizontal adecuado

            // Icon (18x18dp según guidelines = ~24-30px en Unity)
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(button.transform, false);

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.sprite = icon;
            iconImage.preserveAspect = true;

            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(28, 28); // Tamaño apropiado según guidelines

            // Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(button.transform, false);

            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.font = DefaultFont;
            buttonText.text = text;
            buttonText.fontSize = 30;
            buttonText.fontStyle = FontStyles.Bold;
            buttonText.color = textColor;
            buttonText.alignment = TextAlignmentOptions.Left;
        }

        #region Missing Elements (BackButton, LoadingPanel, ErrorPanel)

        private static void BuildBackButton(Canvas canvas)
        {
            // Try to instantiate from prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BACK_BUTTON_PREFAB);
            if (prefab != null)
            {
                GameObject backBtn = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvas.transform);
                backBtn.name = "BackButton";

                RectTransform rect = backBtn.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                rect.anchoredPosition = new Vector2(PADDING, -PADDING);

                Debug.Log("✅ BackButton instantiated from prefab");
            }
            else
            {
                // Create simple back button
                GameObject backBtn = new GameObject("BackButton");
                backBtn.transform.SetParent(canvas.transform, false);

                RectTransform rect = backBtn.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                rect.sizeDelta = new Vector2(50, 50);
                rect.anchoredPosition = new Vector2(PADDING, -PADDING);

                Image bg = backBtn.AddComponent<Image>();
                bg.sprite = WhiteSprite;
                bg.color = new Color(0.2f, 0.2f, 0.3f, 0.8f);

                Button btn = backBtn.AddComponent<Button>();
                btn.targetGraphic = bg;

                // Arrow text
                GameObject arrow = new GameObject("Arrow");
                arrow.transform.SetParent(backBtn.transform, false);

                RectTransform arrowRect = arrow.AddComponent<RectTransform>();
                arrowRect.anchorMin = Vector2.zero;
                arrowRect.anchorMax = Vector2.one;
                arrowRect.sizeDelta = Vector2.zero;

                TextMeshProUGUI arrowText = arrow.AddComponent<TextMeshProUGUI>();
                arrowText.font = DefaultFont;
                arrowText.text = "<";
                arrowText.fontSize = 32;
                arrowText.color = CyanNeon;
                arrowText.alignment = TextAlignmentOptions.Center;

                Debug.Log("✅ BackButton created (no prefab found)");
            }
        }

        private static void BuildLoadingPanel(Canvas canvas)
        {
            GameObject loadingPanel = new GameObject("LoadingPanel");
            loadingPanel.transform.SetParent(canvas.transform, false);

            RectTransform rect = loadingPanel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            // Semi-transparent dark overlay
            Image bg = loadingPanel.AddComponent<Image>();
            bg.sprite = WhiteSprite;
            bg.color = new Color(0, 0, 0, 0.7f);

            // Spinner container
            GameObject spinner = new GameObject("Spinner");
            spinner.transform.SetParent(loadingPanel.transform, false);

            RectTransform spinnerRect = spinner.AddComponent<RectTransform>();
            spinnerRect.anchorMin = new Vector2(0.5f, 0.5f);
            spinnerRect.anchorMax = new Vector2(0.5f, 0.5f);
            spinnerRect.sizeDelta = new Vector2(80, 80);

            Image spinnerImage = spinner.AddComponent<Image>();
            spinnerImage.sprite = WhiteSprite;
            spinnerImage.color = CyanNeon;

            // Loading text
            GameObject loadingText = new GameObject("LoadingText");
            loadingText.transform.SetParent(loadingPanel.transform, false);

            RectTransform textRect = loadingText.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(300, 50);
            textRect.anchoredPosition = new Vector2(0, -80);

            TextMeshProUGUI text = loadingText.AddComponent<TextMeshProUGUI>();
            text.font = DefaultFont;
            text.text = "Cargando...";
            text.fontSize = 30;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;

            // Start hidden
            loadingPanel.SetActive(false);

            Debug.Log("✅ LoadingPanel created");
        }

        private static void BuildErrorPanel(Canvas canvas)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ERROR_PANEL_PREFAB);
            if (prefab != null)
            {
                GameObject errorPanel = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvas.transform);
                errorPanel.name = "ErrorPanel";

                // Position at bottom center
                RectTransform rect = errorPanel.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0.5f, 0);
                    rect.anchorMax = new Vector2(0.5f, 0);
                    rect.pivot = new Vector2(0.5f, 0);
                    rect.anchoredPosition = new Vector2(0, 200);
                }

                Debug.Log("✅ ErrorPanel instantiated from prefab");
            }
            else
            {
                Debug.LogWarning("⚠️ ErrorPanel prefab not found at: " + ERROR_PANEL_PREFAB);
            }
        }

        #endregion
    }
}
