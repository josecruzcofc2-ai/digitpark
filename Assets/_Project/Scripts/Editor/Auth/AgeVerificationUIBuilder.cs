using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// Modern Age Verification UI builder with premium GOLD theme
    /// (Cash Battle style for real money verification)
    /// </summary>
    public static class AgeVerificationUIBuilder
    {
        private const float SCREEN_WIDTH = 1080f;
        private const float SCREEN_HEIGHT = 1920f;

        // Colors - GOLD Theme (Cash Battle)
        private static readonly Color GoldPremium = new Color(1f, 0.843f, 0f, 1f); // #FFD700
        private static readonly Color DarkBrown = new Color(0.06f, 0.05f, 0.10f, 1f); // #0F0D1A
        private static readonly Color CardBackground = new Color(0.2f, 0.133f, 0.067f, 0.95f); // Marrón oscuro con alpha
        private static readonly Color InputBackground = new Color(0.133f, 0.086f, 0.043f, 1f); // Marrón más oscuro
        private static readonly Color TextWhite = Color.white;
        private static readonly Color TextGray = new Color(0.7f, 0.7f, 0.7f, 1f);
        private static readonly Color CyanNeon = new Color(0f, 1f, 1f, 1f); // Para el logo

        // Paths
        private const string WHITE_SPRITE_PATH = "Assets/_Project/Art/Icons/UI/WhiteSquare.png";
        private const string FONT_ASSET_PATH = "Assets/_Project/Art/Fonts/Rajdhani/Rajdhani-Medium SDF.asset";
        private const string VERIFICATION_ICON_PATH = "Assets/_Project/Art/Icons/CashBattle/UI/VerificationIcon.png";

        // Prefab paths
        private const string BACK_BUTTON_GOLD_PREFAB = "Assets/_Project/Prefabs/Common/BackButtonGold.prefab";
        private const string BACK_ICON_GOLD_PATH = "Assets/_Project/Art/Icons/Navigation/BackIconGold.png";

        // Spacing
        private const float PADDING = 30f;
        private const float CARD_PADDING = 40f;
        private const float ELEMENT_SPACING = 20f;
        private const float BUTTON_HEIGHT = 120f;
        private const float ICON_SIZE = 300f;

        private static Sprite WhiteSprite => AssetDatabase.LoadAssetAtPath<Sprite>(WHITE_SPRITE_PATH);
        private static TMP_FontAsset DefaultFont => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_ASSET_PATH);
        private static Sprite VerificationIcon => AssetDatabase.LoadAssetAtPath<Sprite>(VERIFICATION_ICON_PATH);

        [MenuItem("DigitPark/Scenes/Build Scene/Auth/AgeVerification", false, 102)]
        public static void RebuildAgeVerificationScene()
        {
            try
            {
                // Verify prerequisites
                if (WhiteSprite == null || DefaultFont == null || VerificationIcon == null)
                {
                    Debug.LogError("❌ Missing prerequisites! Check WhiteSquare.png, Font, and VerificationIcon.png");
                    return;
                }

                Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
                if (canvas == null)
                {
                    Debug.LogError("❌ No Canvas found in scene.");
                    return;
                }

                Debug.Log("🎨 Starting AgeVerification UI Rebuild (GOLD Theme)...");

                // Clean existing UI
                CleanExistingUI(canvas);

                // Build UI
                BuildBackground(canvas);
                BuildBackButton(canvas);
                BuildLogo(canvas);
                BuildVerificationCard(canvas);
                BuildLoadingIndicator(canvas);

                // Force layout update
                Canvas.ForceUpdateCanvases();

                AutoAssigners.AgeVerificationReferenceAssigner.RunAutoAssign();
                Debug.Log("✅ AgeVerification UI rebuilt successfully!");
                EditorUtility.SetDirty(canvas.gameObject);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error in AgeVerificationUIBuilder: {e.Message}\n{e.StackTrace}");
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
            rect.sizeDelta = new Vector2(600, 120);
            rect.anchoredPosition = new Vector2(0, -80);

            TextMeshProUGUI text = logo.AddComponent<TextMeshProUGUI>();
            text.font = DefaultFont;
            text.text = "Digit Park";
            text.fontSize = FontSizes.Branding;
            text.fontStyle = FontStyles.Bold;
            text.color = GoldPremium;
            text.alignment = TextAlignmentOptions.Center;
            text.enableAutoSizing = true;
            text.fontSizeMin = FontSizes.AutoMinTitle;
            text.fontSizeMax = FontSizes.Branding;
        }

        private static void BuildVerificationCard(Canvas canvas)
        {
            // Card container
            GameObject card = new GameObject("VerificationCard");
            card.transform.SetParent(canvas.transform, false);

            RectTransform cardRect = card.AddComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(SCREEN_WIDTH - (PADDING * 2), 0);
            cardRect.anchoredPosition = new Vector2(0, 500);

            // Card background with GOLD neon border
            Image cardBg = card.AddComponent<Image>();
            cardBg.sprite = WhiteSprite;
            cardBg.color = CardBackground;

            // GOLD neon border
            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = GoldPremium;
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
            CreateVerificationIcon(content.transform);
            CreateTitle(content.transform, "AGE VERIFICATION\nREQUIRED");
            CreateSpacer(content.transform, 10f);
            CreateDescription(content.transform, "Real money competitions require you to be over 18 years old.");
            CreateSpacer(content.transform, 30f);
            CreateStatusText(content.transform); // StatusText for verification status feedback
            CreateSpacer(content.transform, 10f);
            CreateGoldButton(content.transform, "VerifyButton", "VERIFY MY AGE");
            CreateSpacer(content.transform, 10f);
            CreateLegalText(content.transform, "Powered by Triumph™ • Secure and confidential verification");
        }

        private static void CreateVerificationIcon(Transform parent)
        {
            GameObject icon = new GameObject("VerificationIcon");
            icon.transform.SetParent(parent, false);

            RectTransform rect = icon.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(ICON_SIZE, ICON_SIZE);

            // Use the premium verification icon (18+ shield)
            Image iconImage = icon.AddComponent<Image>();
            iconImage.sprite = VerificationIcon;
            iconImage.preserveAspect = true;

            LayoutElement layout = icon.AddComponent<LayoutElement>();
            layout.preferredHeight = ICON_SIZE;
        }

        private static void CreateTitle(Transform parent, string text)
        {
            GameObject title = new GameObject("AgeVerificationTitle");
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
            layout.preferredHeight = 160;
        }

        private static void CreateDescription(Transform parent, string text)
        {
            GameObject desc = new GameObject("AgeVerificationDesc");
            desc.transform.SetParent(parent, false);

            TextMeshProUGUI descText = desc.AddComponent<TextMeshProUGUI>();
            descText.font = DefaultFont;
            descText.text = text;
            descText.fontSize = FontSizes.H3;
            descText.fontStyle = FontStyles.Bold;
            descText.color = TextWhite;
            descText.alignment = TextAlignmentOptions.Center;
            descText.enableWordWrapping = true;
            descText.enableAutoSizing = true;
            descText.fontSizeMin = FontSizes.AutoMinBody;
            descText.fontSizeMax = FontSizes.H3;
            descText.overflowMode = TextOverflowModes.Ellipsis;

            LayoutElement layout = desc.AddComponent<LayoutElement>();
            layout.preferredHeight = 200;
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

            // Gold button hover effect
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.647f, 0f, 0.9f); // Lighter gold
            colors.pressedColor = new Color(1f, 0.549f, 0f, 1f); // Orange gold
            colors.selectedColor = new Color(1f, 0.647f, 0f, 0.9f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.15f;
            button.colors = colors;

            LayoutElement layout = btn.AddComponent<LayoutElement>();
            layout.preferredHeight = BUTTON_HEIGHT;

            // Named for AutoLocalizer: "VerifyAgeButtonText"=>"verify_age"
            GameObject textObj = new GameObject("VerifyAgeButtonText");
            textObj.transform.SetParent(btn.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.font = DefaultFont;
            btnText.text = text;
            btnText.fontSize = FontSizes.H2;
            btnText.fontStyle = FontStyles.Bold;
            btnText.color = DarkBrown;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.enableAutoSizing = true;
            btnText.fontSizeMin = FontSizes.AutoMinBody;
            btnText.fontSizeMax = FontSizes.H2;
            btnText.overflowMode = TextOverflowModes.Ellipsis;
        }

        private static void CreateLegalText(Transform parent, string text)
        {
            // Named for AutoLocalizer: "AgeVerificationLegalText"=>"age_verification_powered"
            GameObject legal = new GameObject("AgeVerificationLegalText");
            legal.transform.SetParent(parent, false);

            TextMeshProUGUI legalText = legal.AddComponent<TextMeshProUGUI>();
            legalText.font = DefaultFont;
            legalText.text = text;
            legalText.fontSize = FontSizes.Body;
            legalText.fontStyle = FontStyles.Bold;
            legalText.color = TextGray;
            legalText.alignment = TextAlignmentOptions.Center;
            legalText.enableWordWrapping = true;
            legalText.enableAutoSizing = true;
            legalText.fontSizeMin = FontSizes.AutoMinBody;
            legalText.fontSizeMax = FontSizes.Body;
            legalText.overflowMode = TextOverflowModes.Ellipsis;

            LayoutElement layout = legal.AddComponent<LayoutElement>();
            layout.preferredHeight = 90;
        }

        private static void CreateSpacer(Transform parent, float height)
        {
            GameObject spacer = new GameObject("Spacer");
            spacer.transform.SetParent(parent, false);

            LayoutElement layout = spacer.AddComponent<LayoutElement>();
            layout.preferredHeight = height;
        }

        private static void CreateStatusText(Transform parent)
        {
            // Named for AutoLocalizer: "AgeVerificationStatusText"=>"age_verification_tap"
            GameObject status = new GameObject("AgeVerificationStatusText");
            status.transform.SetParent(parent, false);

            TextMeshProUGUI statusText = status.AddComponent<TextMeshProUGUI>();
            statusText.font = DefaultFont;
            statusText.text = "Tap the button to start verification";
            statusText.fontSize = FontSizes.H3;
            statusText.fontStyle = FontStyles.Bold;
            statusText.color = TextWhite;
            statusText.alignment = TextAlignmentOptions.Center;
            statusText.enableWordWrapping = true;
            statusText.enableAutoSizing = true;
            statusText.fontSizeMin = FontSizes.AutoMinBody;
            statusText.fontSizeMax = FontSizes.H3;
            statusText.overflowMode = TextOverflowModes.Ellipsis;

            LayoutElement layout = status.AddComponent<LayoutElement>();
            layout.preferredHeight = 150;
        }

        #region Additional Elements (BackButton, LoadingIndicator)

        private static void BuildBackButton(Canvas canvas)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BACK_BUTTON_GOLD_PREFAB);
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

                // Assign BackIconGold sprite to Icon child
                Sprite backIcon = AssetDatabase.LoadAssetAtPath<Sprite>(BACK_ICON_GOLD_PATH);
                if (backIcon != null)
                {
                    Transform iconChild = backBtn.transform.Find("Icon");
                    if (iconChild != null)
                    {
                        Image iconImg = iconChild.GetComponent<Image>();
                        if (iconImg != null) iconImg.sprite = backIcon;
                    }
                }

                Debug.Log("✅ BackButtonGold instantiated from prefab");
            }
            else
            {
                // Fallback: create simple back button
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
                bg.color = new Color(0.3f, 0.2f, 0.1f, 0.9f);

                Button btn = backBtn.AddComponent<Button>();
                btn.targetGraphic = bg;

                GameObject arrow = new GameObject("Arrow");
                arrow.transform.SetParent(backBtn.transform, false);

                RectTransform arrowRect = arrow.AddComponent<RectTransform>();
                arrowRect.anchorMin = Vector2.zero;
                arrowRect.anchorMax = Vector2.one;
                arrowRect.sizeDelta = Vector2.zero;

                Image arrowText = arrow.AddComponent<Image>();
                Sprite arrowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/UI/icon_back_arrow.png");
                if (arrowSprite != null) arrowText.sprite = arrowSprite;
                arrowText.color = GoldPremium;
                arrowText.preserveAspect = true;
                arrowText.raycastTarget = false;

                Debug.Log("✅ BackButton created (no prefab found)");
            }
        }

        private static void BuildLoadingIndicator(Canvas canvas)
        {
            // Fullscreen overlay (blocks interaction behind it)
            GameObject loadingIndicator = new GameObject("LoadingIndicator");
            loadingIndicator.transform.SetParent(canvas.transform, false);

            RectTransform rect = loadingIndicator.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Dark semi-transparent background
            Image overlayBg = loadingIndicator.AddComponent<Image>();
            overlayBg.color = new Color(0f, 0f, 0f, 0.75f);
            overlayBg.raycastTarget = true;

            // Centered spinner container
            GameObject spinnerContainer = new GameObject("SpinnerContainer");
            spinnerContainer.transform.SetParent(loadingIndicator.transform, false);
            RectTransform scRT = spinnerContainer.AddComponent<RectTransform>();
            scRT.anchorMin = new Vector2(0.5f, 0.5f);
            scRT.anchorMax = new Vector2(0.5f, 0.5f);
            scRT.sizeDelta = new Vector2(80, 80);
            Image spinnerImage = spinnerContainer.AddComponent<Image>();
            spinnerImage.sprite = WhiteSprite;
            spinnerImage.color = GoldPremium;

            // "Verifying..." text below spinner
            GameObject textObj = new GameObject("VerifyingText");
            textObj.transform.SetParent(loadingIndicator.transform, false);
            RectTransform txtRT = textObj.AddComponent<RectTransform>();
            txtRT.anchorMin = new Vector2(0.5f, 0.5f);
            txtRT.anchorMax = new Vector2(0.5f, 0.5f);
            txtRT.anchoredPosition = new Vector2(0, -70);
            txtRT.sizeDelta = new Vector2(400, 40);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "Verifying...";
            tmp.fontSize = FontSizes.Body;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color(0.95f, 0.95f, 0.95f, 1f);
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinSmall;
            tmp.fontSizeMax = FontSizes.Body;

            // Start hidden
            loadingIndicator.SetActive(false);

            Debug.Log("[AgeVerificationUI] LoadingIndicator rebuilt as fullscreen overlay");
        }

        #endregion
    }
}
