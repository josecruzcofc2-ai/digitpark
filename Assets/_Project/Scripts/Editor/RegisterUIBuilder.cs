using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Modern Register UI builder with neon card design
    /// </summary>
    public static class RegisterUIBuilder
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

        // Spacing
        private const float PADDING = 30f;
        private const float CARD_PADDING = 40f;
        private const float ELEMENT_SPACING = 20f;
        private const float INPUT_HEIGHT = 60f;
        private const float BUTTON_HEIGHT = 60f;

        private static Sprite WhiteSprite => AssetDatabase.LoadAssetAtPath<Sprite>(WHITE_SPRITE_PATH);
        private static TMP_FontAsset DefaultFont => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_ASSET_PATH);
        private static Sprite EyeOpenIcon => AssetDatabase.LoadAssetAtPath<Sprite>(EYE_OPEN_PATH);
        private static Sprite EyeClosedIcon => AssetDatabase.LoadAssetAtPath<Sprite>(EYE_CLOSED_PATH);

        [MenuItem("DigitPark/UI Builders/Auth/Register", false, 201)]
        public static void RebuildRegisterScene()
        {
            try
            {
                // Verify prerequisites
                if (WhiteSprite == null || DefaultFont == null)
                {
                    Debug.LogError("❌ Missing prerequisites! Run 'Create White Sprite' first.");
                    return;
                }

                Canvas canvas = Object.FindFirstObjectByType<Canvas>();
                if (canvas == null)
                {
                    Debug.LogError("❌ No Canvas found in scene.");
                    return;
                }

                Debug.Log("🎨 Starting Register UI Rebuild...");

                // Clean existing UI
                CleanExistingUI(canvas);

                // Build UI
                BuildBackground(canvas);
                BuildLogo(canvas);
                BuildRegisterCard(canvas);

                // Force layout update
                Canvas.ForceUpdateCanvases();

                Debug.Log("✅ Register UI rebuilt successfully!");
                EditorUtility.SetDirty(canvas.gameObject);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error in RegisterUIBuilder: {e.Message}\n{e.StackTrace}");
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
            rect.sizeDelta = new Vector2(400, 120);
            rect.anchoredPosition = new Vector2(0, -80);

            TextMeshProUGUI text = logo.AddComponent<TextMeshProUGUI>();
            text.font = DefaultFont;
            text.text = "Digit Park";
            text.fontSize = 56;
            text.fontStyle = FontStyles.Bold;
            text.color = CyanNeon;
            text.alignment = TextAlignmentOptions.Center;
        }

        private static void BuildRegisterCard(Canvas canvas)
        {
            // Card container
            GameObject card = new GameObject("RegisterCard");
            card.transform.SetParent(canvas.transform, false);

            RectTransform cardRect = card.AddComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(SCREEN_WIDTH - (PADDING * 2), 0);
            cardRect.anchoredPosition = new Vector2(0, 0); // Centrado verticalmente

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
            CreateTitle(content.transform, "CREA UNA CUENTA");
            CreateInputField(content.transform, "UsernameInput", "Nombre de usuario", false);
            CreateInputField(content.transform, "EmailInput", "Email", false);
            CreateInputField(content.transform, "PasswordInput", "Contraseña", true);
            CreateInputField(content.transform, "ConfirmPasswordInput", "Confirmar Contraseña", true);
            CreatePrimaryButton(content.transform, "RegisterButton", "Crear Cuenta");
        }

        private static void CreateTitle(Transform parent, string text)
        {
            GameObject title = new GameObject("Title");
            title.transform.SetParent(parent, false);

            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.font = DefaultFont;
            titleText.text = text;
            titleText.fontSize = 32;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = CyanNeon;
            titleText.alignment = TextAlignmentOptions.Center;

            LayoutElement layout = title.AddComponent<LayoutElement>();
            layout.preferredHeight = 60;
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
            textAreaRect.offsetMax = new Vector2(isPassword ? -60 : -20, 0);

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
            placeholderText.fontSize = 18;
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
            inputText.fontSize = 20;
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
            rect.sizeDelta = new Vector2(40, 40);
            rect.anchoredPosition = new Vector2(-10, 0);

            Button btn = eyeBtn.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;

            Image eyeImage = eyeBtn.AddComponent<Image>();
            eyeImage.sprite = EyeClosedIcon;
            eyeImage.color = CyanNeon;

            // TODO: Add toggle functionality in runtime script
        }

        private static void CreatePrimaryButton(Transform parent, string name, string text)
        {
            GameObject btn = new GameObject(name);
            btn.transform.SetParent(parent, false);

            Image bg = btn.AddComponent<Image>();
            bg.sprite = WhiteSprite;
            bg.color = CyanNeon;

            Button button = btn.AddComponent<Button>();
            button.targetGraphic = bg;

            LayoutElement layout = btn.AddComponent<LayoutElement>();
            layout.preferredHeight = BUTTON_HEIGHT;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btn.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.font = DefaultFont;
            btnText.text = text;
            btnText.fontSize = 22;
            btnText.fontStyle = FontStyles.Bold;
            btnText.color = DarkNavy;
            btnText.alignment = TextAlignmentOptions.Center;
        }
    }
}
