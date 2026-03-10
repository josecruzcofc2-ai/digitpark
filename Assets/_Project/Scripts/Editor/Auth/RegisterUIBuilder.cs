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
    /// Modern Register UI builder with neon card design
    /// </summary>
    public static class RegisterUIBuilder
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

        // Paths
        private const string WHITE_SPRITE_PATH = "Assets/_Project/Art/Icons/UI/WhiteSquare.png";
        private const string FONT_ASSET_PATH = "Assets/_Project/Art/Fonts/Rajdhani/Rajdhani-Medium SDF.asset";
        private const string EYE_OPEN_PATH = "Assets/_Project/Art/Icons/Navigation/EyeOpen.png";
        private const string EYE_CLOSED_PATH = "Assets/_Project/Art/Icons/Navigation/EyeClosed.png";

        // Prefab paths
        private const string ERROR_PANEL_PREFAB = "Assets/_Project/Prefabs/Common/ErrorPanel.prefab";
        private const string BACK_BUTTON_PREFAB = "Assets/_Project/Prefabs/Common/BackButton.prefab";

        // Spacing
        private const float PADDING = 30f;
        private const float CARD_PADDING = 40f;
        private const float ELEMENT_SPACING = 20f;
        private const float INPUT_HEIGHT = 90f;
        private const float BUTTON_HEIGHT = 120f;

        private static Sprite WhiteSprite => AssetDatabase.LoadAssetAtPath<Sprite>(WHITE_SPRITE_PATH);
        private static TMP_FontAsset DefaultFont => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_ASSET_PATH);
        private static Sprite EyeOpenIcon => AssetDatabase.LoadAssetAtPath<Sprite>(EYE_OPEN_PATH);
        private static Sprite EyeClosedIcon => AssetDatabase.LoadAssetAtPath<Sprite>(EYE_CLOSED_PATH);

        [MenuItem("DigitPark/Scenes/Build Scene/Auth/Register", false, 101)]
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

                Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
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
                BuildBackButton(canvas);
                BuildLogo(canvas);
                BuildRegisterCard(canvas);
                BuildLoadingPanel(canvas);
                BuildErrorPanel(canvas);

                // Force layout update
                Canvas.ForceUpdateCanvases();

                AutoAssigners.RegisterReferenceAssigner.RunAutoAssign();
                Debug.Log("✅ Register UI rebuilt successfully!");
                EditorUtility.SetDirty(canvas.gameObject);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error in RegisterUIBuilder: {e.Message}\n{e.StackTrace}");
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
            scaler.matchWidthOrHeight = 0.5f;

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
            text.enableAutoSizing = true;
            text.fontSizeMin = FontSizes.AutoMinTitle;
            text.fontSizeMax = FontSizes.Branding;
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
            cardRect.anchoredPosition = new Vector2(0, 500);

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
            CreateTitle(content.transform, "CREATE AN ACCOUNT");
            CreateInputField(content.transform, "UsernameInput", "Username", false);
            CreateInputField(content.transform, "EmailInput", "Email", false);
            CreateInputField(content.transform, "PasswordInput", "Password", true);
            CreateInputField(content.transform, "ConfirmPasswordInput", "Confirm Password", true);
            CreatePrimaryButton(content.transform, "RegisterButton", "Create Account");
        }

        private static void CreateTitle(Transform parent, string text)
        {
            GameObject title = new GameObject("RegisterTitleText");
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

            // Named for AutoLocalizer: "UsernameInput"→"UsernameInputPlaceholder", "EmailInput"→"EmailInputPlaceholder", etc.
            string placeholderGoName = name + "Placeholder";
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

            // Named for AutoLocalizer: "RegisterButton"→"RegisterButtonText"=>"register_button"
            GameObject textObj = new GameObject(name + "Text");
            textObj.transform.SetParent(btn.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.font = DefaultFont;
            btnText.text = text;
            btnText.fontSize = FontSizes.H1;
            btnText.fontStyle = FontStyles.Bold;
            btnText.color = DarkNavy;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.enableAutoSizing = true;
            btnText.fontSizeMin = FontSizes.AutoMinBody;
            btnText.fontSizeMax = FontSizes.H1;
            btnText.overflowMode = TextOverflowModes.Ellipsis;
        }

        #region Missing Elements (BackButton, LoadingPanel, ErrorPanel)

        private static void BuildBackButton(Canvas canvas)
        {
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

            Image bg = loadingPanel.AddComponent<Image>();
            bg.sprite = WhiteSprite;
            bg.color = new Color(0, 0, 0, 0.7f);

            GameObject spinner = new GameObject("Spinner");
            spinner.transform.SetParent(loadingPanel.transform, false);

            RectTransform spinnerRect = spinner.AddComponent<RectTransform>();
            spinnerRect.anchorMin = new Vector2(0.5f, 0.5f);
            spinnerRect.anchorMax = new Vector2(0.5f, 0.5f);
            spinnerRect.sizeDelta = new Vector2(80, 80);

            Image spinnerImage = spinner.AddComponent<Image>();
            spinnerImage.sprite = WhiteSprite;
            spinnerImage.color = CyanNeon;

            // Named for AutoLocalizer: "CreatingAccountText"=>"creating_account"
            GameObject loadingText = new GameObject("CreatingAccountText");
            loadingText.transform.SetParent(loadingPanel.transform, false);

            RectTransform textRect = loadingText.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(500, 60);
            textRect.anchoredPosition = new Vector2(0, -80);

            TextMeshProUGUI text = loadingText.AddComponent<TextMeshProUGUI>();
            text.font = DefaultFont;
            text.text = "Creating account...";
            text.fontSize = FontSizes.Body;
            text.fontStyle = FontStyles.Bold;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.enableAutoSizing = true;
            text.fontSizeMin = FontSizes.AutoMinBody;
            text.fontSizeMax = FontSizes.Body;
            text.overflowMode = TextOverflowModes.Ellipsis;

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
