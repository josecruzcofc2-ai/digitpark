using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using TMPro;
using DigitPark.UI;
using DigitPark.UI.Common;

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
        private static readonly Color DarkNavy = new Color(0.02f, 0.04f, 0.08f, 1f); // #050A14
        private static readonly Color CardBackground = new Color(0.125f, 0.188f, 0.376f, 0.95f); // #202860 con alpha
        private static readonly Color InputBackground = new Color(0.078f, 0.11f, 0.22f, 1f); // Más oscuro para inputs
        private static readonly Color TextWhite = Color.white;
        private static readonly Color TextGray = new Color(0.7f, 0.7f, 0.7f, 1f);
        private static readonly Color GoogleBorderGray = new Color(0.455f, 0.467f, 0.459f, 1f); // #747775 Google Light theme border
        private static readonly Color NearBlack = new Color(0.122f, 0.122f, 0.122f, 1f); // #1F1F1F Google text color
        private static readonly Color GoogleDark = new Color(0.075f, 0.075f, 0.078f, 1f); // #131314 Google dark theme bg
        private static readonly Color AppleBlack = Color.black; // Apple dark button bg

        // Paths
        private const string WHITE_SPRITE_PATH = "Assets/_Project/Art/Icons/UI/WhiteSquare.png";
        private const string ROUNDED_RECT_PATH = "Assets/_Project/Art/Icons/UI/RoundedRect.png";
        private const string FONT_ASSET_PATH = "Assets/_Project/Art/Fonts/Rajdhani/Rajdhani-Medium SDF.asset";
        private const string EYE_OPEN_PATH = "Assets/_Project/Art/Icons/Navigation/EyeOpen.png";
        private const string EYE_CLOSED_PATH = "Assets/_Project/Art/Icons/Navigation/EyeClosed.png";
        private const string GOOGLE_ICON_PATH = "Assets/_Project/Art/Icons/Auth/Google/google_g_logo.png"; // Clean G on transparent (dark theme)
        private const string GOOGLE_ICON_FALLBACK = "Assets/_Project/Art/Icons/Auth/Google/google_logo_official.png"; // Fallback
        private const string APPLE_ICON_PATH = "Assets/_Project/Art/Icons/Auth/Apple/apple_logo_black.png"; // White logo on transparent (for dark button)

        // Prefab paths
        private const string ERROR_PANEL_PREFAB = "Assets/_Project/Prefabs/Common/ErrorPanel.prefab";
        private const string BACK_BUTTON_PREFAB = "Assets/_Project/Prefabs/Common/BackButton.prefab";

        // Spacing
        private const float PADDING = 30f;
        private const float CARD_PADDING = 40f;
        private const float ELEMENT_SPACING = 20f;
        private const float INPUT_HEIGHT = 90f;
        private const float BUTTON_HEIGHT = 120f;
        private const float SOCIAL_BUTTON_HEIGHT = 100f; // Apple HIG: min 30pt (90px @3x), recommended 44pt
        private const float SOCIAL_ICON_SIZE = 44f; // Icon size per Apple/Google guidelines
        private const float SOCIAL_ICON_PADDING = 28f; // Left padding for icon
        private const float SOCIAL_FONT_SIZE = 44f; // ~43% of button height per Apple HIG (43% of 100 = 43)

        private static Sprite WhiteSprite => AssetDatabase.LoadAssetAtPath<Sprite>(WHITE_SPRITE_PATH);
        private static Sprite RoundedRectSprite => AssetDatabase.LoadAssetAtPath<Sprite>(ROUNDED_RECT_PATH);
        private static TMP_FontAsset DefaultFont => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_ASSET_PATH);
        private static Sprite EyeOpenIcon => AssetDatabase.LoadAssetAtPath<Sprite>(EYE_OPEN_PATH);
        private static Sprite EyeClosedIcon => AssetDatabase.LoadAssetAtPath<Sprite>(EYE_CLOSED_PATH);
        private static Sprite GoogleIcon => AssetDatabase.LoadAssetAtPath<Sprite>(GOOGLE_ICON_PATH)
            ?? AssetDatabase.LoadAssetAtPath<Sprite>(GOOGLE_ICON_FALLBACK);
        private static Sprite AppleIcon => AssetDatabase.LoadAssetAtPath<Sprite>(APPLE_ICON_PATH);

        [MenuItem("DigitPark/Scenes/Build Scene/Auth/Login", false, 100)]
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
            text.fontSize = FontSizes.Branding;
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
            cardRect.anchoredPosition = new Vector2(0, 440); // Adjusted for taller social buttons

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
            CreateTitle(content.transform, "SIGN IN");
            CreateInputField(content.transform, "EmailInput", "Email", false);
            CreateInputField(content.transform, "PasswordInput", "Password", true);
            CreateCenteredLink(content.transform, "ForgotPasswordButton", "Forgot your password?");
            CreateCenteredCheckbox(content.transform);
            CreatePrimaryButton(content.transform, "LoginButton", "Sign In");
            CreateOrDivider(content.transform);
            // Google Sign-In: Dark theme (#131314 bg, white text, colored G — per Google branding guidelines)
            CreateSocialButton(content.transform, "GoogleButton", "Sign in with Google", GoogleIcon, GoogleDark, Color.white, null);
            // Apple Sign-In: Black variant (black bg, white text+logo — per Apple HIG for dark environments)
            CreateSocialButton(content.transform, "AppleButton", "Sign in with Apple", AppleIcon, AppleBlack, Color.white, null);
        }

        private static void BuildFooter(Canvas canvas)
        {
            GameObject footer = new GameObject("NoAccountText");
            footer.transform.SetParent(canvas.transform, false);

            RectTransform rect = footer.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0);
            rect.anchorMax = new Vector2(0.5f, 0);
            rect.pivot = new Vector2(0.5f, 0);
            rect.sizeDelta = new Vector2(600, 60);
            rect.anchoredPosition = new Vector2(0, 190);

            TextMeshProUGUI question = footer.AddComponent<TextMeshProUGUI>();
            question.font = DefaultFont;
            question.text = "Don't have an account?";
            question.fontSize = FontSizes.Body;
            question.fontStyle = FontStyles.Bold;
            question.color = TextGray;
            question.alignment = TextAlignmentOptions.Center;
            question.enableAutoSizing = true;
            question.fontSizeMin = FontSizes.AutoMinBody;
            question.fontSizeMax = FontSizes.Body;
            question.overflowMode = TextOverflowModes.Ellipsis;
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

            // Named for AutoLocalizer: "RegisterButtonText"=>"register_button"
            GameObject textObj = new GameObject("RegisterButtonText");
            textObj.transform.SetParent(registerBtn.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.font = DefaultFont;
            btnText.text = "Create an account";
            btnText.fontSize = FontSizes.H1;
            btnText.fontStyle = FontStyles.Bold;
            btnText.color = DarkNavy;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.enableAutoSizing = true;
            btnText.fontSizeMin = FontSizes.AutoMinBody;
            btnText.fontSizeMax = FontSizes.H1;
            btnText.overflowMode = TextOverflowModes.Ellipsis;

            Debug.Log("✅ RegisterButton creado. Ahora puedes moverlo manualmente en la escena.");
        }

        private static void CreateTitle(Transform parent, string text)
        {
            GameObject title = new GameObject("LoginTitle");
            title.transform.SetParent(parent, false);

            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.font = DefaultFont;
            titleText.text = text;
            titleText.fontSize = FontSizes.H4;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = CyanNeon;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = FontSizes.AutoMinTitle;
            titleText.fontSizeMax = FontSizes.H4;
            titleText.overflowMode = TextOverflowModes.Ellipsis;

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

            // Placeholder — named for AutoLocalizer: "EmailInput"→"EmailPlaceholder", "PasswordInput"→"PasswordPlaceholder"
            string placeholderGoName = name.Replace("Input", "") + "Placeholder";
            GameObject placeholderObj = new GameObject(placeholderGoName);
            placeholderObj.transform.SetParent(textArea.transform, false);

            RectTransform placeholderRect = placeholderObj.AddComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholderText.font = DefaultFont;
            placeholderText.text = placeholder;
            placeholderText.fontSize = FontSizes.H3;
            placeholderText.fontStyle = FontStyles.Bold;
            placeholderText.color = TextGray;
            placeholderText.alignment = TextAlignmentOptions.Left;
            placeholderText.enableAutoSizing = true;
            placeholderText.fontSizeMin = FontSizes.AutoMinBody;
            placeholderText.fontSizeMax = FontSizes.H3;
            placeholderText.overflowMode = TextOverflowModes.Ellipsis;

            // Input text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(textArea.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI inputText = textObj.AddComponent<TextMeshProUGUI>();
            inputText.font = DefaultFont;
            inputText.fontSize = FontSizes.H3;
            inputText.fontStyle = FontStyles.Bold;
            inputText.color = TextWhite;
            inputText.alignment = TextAlignmentOptions.Left;
            inputText.enableAutoSizing = true;
            inputText.fontSizeMin = FontSizes.AutoMinBody;
            inputText.fontSizeMax = FontSizes.H3;
            inputText.overflowMode = TextOverflowModes.Ellipsis;

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

            Image eyeImage = eyeBtn.AddComponent<Image>();
            eyeImage.sprite = EyeOpenIcon;
            eyeImage.color = CyanNeon;

            Button btn = eyeBtn.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;

            // Add PasswordToggle runtime component and wire references
            var toggle = eyeBtn.AddComponent<PasswordToggle>();
            var so = new SerializedObject(toggle);
            so.FindProperty("passwordInput").objectReferenceValue = inputField;
            so.FindProperty("eyeOpenIcon").objectReferenceValue = EyeOpenIcon;
            so.FindProperty("eyeClosedIcon").objectReferenceValue = EyeClosedIcon;
            so.ApplyModifiedProperties();

            // Wire onClick to PasswordToggle.TogglePasswordVisibility
            UnityEventTools.AddPersistentListener(btn.onClick, toggle.TogglePasswordVisibility);
        }

        private static void CreateCenteredLink(Transform parent, string name, string text)
        {
            GameObject linkBtn = new GameObject(name);
            linkBtn.transform.SetParent(parent, false);

            Button btn = linkBtn.AddComponent<Button>();

            TextMeshProUGUI linkText = linkBtn.AddComponent<TextMeshProUGUI>();
            linkText.font = DefaultFont;
            linkText.text = text;
            linkText.fontSize = FontSizes.Body;
            linkText.fontStyle = FontStyles.Bold;
            linkText.color = CyanNeon;
            linkText.alignment = TextAlignmentOptions.Center;
            linkText.enableAutoSizing = true;
            linkText.fontSizeMin = FontSizes.AutoMinBody;
            linkText.fontSizeMax = FontSizes.Body;
            linkText.overflowMode = TextOverflowModes.Ellipsis;
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

            // Label — named for AutoLocalizer: "RememberMeText"=>"remember_me"
            GameObject label = new GameObject("RememberMeText");
            label.transform.SetParent(row.transform, false);

            TextMeshProUGUI labelText = label.AddComponent<TextMeshProUGUI>();
            labelText.font = DefaultFont;
            labelText.text = "Remember me";
            labelText.fontSize = FontSizes.Body;
            labelText.fontStyle = FontStyles.Bold;
            labelText.color = TextWhite;
            labelText.enableWordWrapping = false;
            labelText.enableAutoSizing = true;
            labelText.fontSizeMin = FontSizes.Caption;
            labelText.fontSizeMax = FontSizes.Body;
            labelText.overflowMode = TextOverflowModes.Ellipsis;
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

            // Named for AutoLocalizer: "LoginButton"→"LoginButtonText"=>"login_button"
            GameObject textObj = new GameObject(name + "Text");
            textObj.transform.SetParent(button.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.font = DefaultFont;
            buttonText.text = text;
            buttonText.fontSize = FontSizes.H1;
            buttonText.fontStyle = FontStyles.Bold;
            buttonText.color = DarkNavy;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.enableAutoSizing = true;
            buttonText.fontSizeMin = FontSizes.AutoMinBody;
            buttonText.fontSizeMax = FontSizes.H1;
            buttonText.overflowMode = TextOverflowModes.Ellipsis;
        }


        /// <summary>
        /// Creates an "── or ──" divider between primary login and social buttons (standard App Store pattern)
        /// </summary>
        private static void CreateOrDivider(Transform parent)
        {
            GameObject divider = new GameObject("OrDivider");
            divider.transform.SetParent(parent, false);

            LayoutElement layout = divider.AddComponent<LayoutElement>();
            layout.preferredHeight = 50f;

            // Left line
            GameObject leftLine = new GameObject("LeftLine");
            leftLine.transform.SetParent(divider.transform, false);

            RectTransform leftRect = leftLine.AddComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0, 0.5f);
            leftRect.anchorMax = new Vector2(0.42f, 0.5f);
            leftRect.sizeDelta = new Vector2(0, 2);

            Image leftImg = leftLine.AddComponent<Image>();
            leftImg.sprite = WhiteSprite;
            leftImg.color = TextGray;

            // "or" text
            GameObject orText = new GameObject("OrText");
            orText.transform.SetParent(divider.transform, false);

            RectTransform orRect = orText.AddComponent<RectTransform>();
            orRect.anchorMin = new Vector2(0.42f, 0);
            orRect.anchorMax = new Vector2(0.58f, 1);
            orRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI text = orText.AddComponent<TextMeshProUGUI>();
            text.font = DefaultFont;
            text.text = "or";
            text.fontSize = FontSizes.Body;
            text.fontStyle = FontStyles.Bold;
            text.color = TextGray;
            text.alignment = TextAlignmentOptions.Center;
            text.enableAutoSizing = true;
            text.fontSizeMin = FontSizes.AutoMinBody;
            text.fontSizeMax = FontSizes.Body;
            text.overflowMode = TextOverflowModes.Ellipsis;

            // Right line
            GameObject rightLine = new GameObject("RightLine");
            rightLine.transform.SetParent(divider.transform, false);

            RectTransform rightRect = rightLine.AddComponent<RectTransform>();
            rightRect.anchorMin = new Vector2(0.58f, 0.5f);
            rightRect.anchorMax = new Vector2(1, 0.5f);
            rightRect.sizeDelta = new Vector2(0, 2);

            Image rightImg = rightLine.AddComponent<Image>();
            rightImg.sprite = WhiteSprite;
            rightImg.color = TextGray;
        }

        /// <summary>
        /// Creates a social sign-in button compliant with Apple HIG and Google Branding Guidelines.
        /// - Dark theme: dark bg, white text, colored icon
        /// - Rounded corners via 9-sliced RoundedRect sprite
        /// - Icon: left-aligned at fixed position (not centered)
        /// - Text: centered across full button width
        /// - Height: 100px (~33pt, above Apple's 30pt minimum)
        /// - Font: 43% of height per Apple HIG
        /// - Optional border color (rendered as child Image frame)
        /// </summary>
        private static void CreateSocialButton(Transform parent, string name, string text, Sprite icon, Color bgColor, Color textColor, Color? borderColor)
        {
            GameObject button = new GameObject(name);
            button.transform.SetParent(parent, false);

            // Use rounded rect sprite with 9-slice for proper corner radius
            Sprite bgSprite = RoundedRectSprite ?? WhiteSprite;
            Image bg = button.AddComponent<Image>();
            bg.sprite = bgSprite;
            bg.type = Image.Type.Sliced;
            bg.pixelsPerUnitMultiplier = 1f;
            bg.color = bgColor;

            // Optional border as child frame (clean 2px border, not Unity Outline shadow)
            if (borderColor.HasValue)
            {
                GameObject borderObj = new GameObject("Border");
                borderObj.transform.SetParent(button.transform, false);
                RectTransform borderRect = borderObj.AddComponent<RectTransform>();
                borderRect.anchorMin = Vector2.zero;
                borderRect.anchorMax = Vector2.one;
                borderRect.sizeDelta = Vector2.zero;
                Image borderImg = borderObj.AddComponent<Image>();
                borderImg.sprite = bgSprite;
                borderImg.type = Image.Type.Sliced;
                borderImg.color = borderColor.Value;
                // Place behind content but the bg is already solid, so this acts as outline
                // Actually we need to make bg slightly smaller or border slightly larger
                borderRect.offsetMin = new Vector2(-2, -2);
                borderRect.offsetMax = new Vector2(2, 2);
                borderObj.transform.SetAsFirstSibling();
            }

            Button btn = button.AddComponent<Button>();
            btn.targetGraphic = bg;

            LayoutElement layout = button.AddComponent<LayoutElement>();
            layout.preferredHeight = SOCIAL_BUTTON_HEIGHT;

            // Icon — left-aligned per Apple HIG & Google branding guidelines
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(button.transform, false);

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.sprite = icon;
            iconImage.preserveAspect = true;

            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0, 0.5f);
            iconRect.anchorMax = new Vector2(0, 0.5f);
            iconRect.pivot = new Vector2(0, 0.5f);
            iconRect.anchoredPosition = new Vector2(SOCIAL_ICON_PADDING, 0);
            iconRect.sizeDelta = new Vector2(SOCIAL_ICON_SIZE, SOCIAL_ICON_SIZE);

            // Named for AutoLocalizer: "GoogleButton"→"GoogleSignInText", "AppleButton"→"AppleSignInText"
            string textGoName = name.Replace("Button", "") + "SignInText";
            GameObject textObj = new GameObject(textGoName);
            textObj.transform.SetParent(button.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.font = DefaultFont;
            buttonText.text = text;
            buttonText.fontSize = SOCIAL_FONT_SIZE;
            buttonText.fontStyle = FontStyles.Bold;
            buttonText.color = textColor;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.enableAutoSizing = true;
            buttonText.fontSizeMin = FontSizes.AutoMinBody;
            buttonText.fontSizeMax = SOCIAL_FONT_SIZE;
            buttonText.overflowMode = TextOverflowModes.Ellipsis;
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
                rect.anchoredPosition = new Vector2(20, -20);
                rect.sizeDelta = new Vector2(50, 50);

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
                rect.anchoredPosition = new Vector2(20, -20);

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
                arrowText.fontSize = FontSizes.Body;
                arrowText.fontStyle = FontStyles.Bold;
                arrowText.color = CyanNeon;
                arrowText.alignment = TextAlignmentOptions.Center;
                arrowText.enableAutoSizing = true;
                arrowText.fontSizeMin = FontSizes.AutoMinBody;
                arrowText.fontSizeMax = FontSizes.Body;
                arrowText.overflowMode = TextOverflowModes.Ellipsis;

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
            // Named for AutoLocalizer: "GeneralLoadingText"=>"loading"
            GameObject loadingText = new GameObject("GeneralLoadingText");
            loadingText.transform.SetParent(loadingPanel.transform, false);

            RectTransform textRect = loadingText.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(400, 60);
            textRect.anchoredPosition = new Vector2(0, -80);

            TextMeshProUGUI text = loadingText.AddComponent<TextMeshProUGUI>();
            text.font = DefaultFont;
            text.text = "Loading...";
            text.fontSize = FontSizes.Body;
            text.fontStyle = FontStyles.Bold;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.enableAutoSizing = true;
            text.fontSizeMin = FontSizes.AutoMinBody;
            text.fontSizeMax = FontSizes.Body;
            text.overflowMode = TextOverflowModes.Ellipsis;

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
