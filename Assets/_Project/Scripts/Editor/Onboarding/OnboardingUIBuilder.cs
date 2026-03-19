using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;
using DigitPark.Themes;
using ET = DigitPark.Themes.ThemeApplier.ElementType;

namespace DigitPark.Editor
{
    /// <summary>
    /// Main Onboarding UI Builder - Slide-Based Architecture
    /// Each step is an independent slide inside SlidesContainer (like CashBattle).
    /// 8 slides: Welcome, Name, Avatar, Games, CashBattle, Tournaments, Rewards, Completion
    /// Portrait 9:16 (1080x1920), matchWidthOrHeight=0
    ///
    /// Menu: DigitPark/UI Builders/Onboarding/Main Onboarding
    /// </summary>
    public class OnboardingUIBuilder : EditorWindow
    {
        #region Colors

        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.5f, 1f);
        private static readonly Color CYAN_GLOW = new Color(0f, 1f, 1f, 0.06f);

        private static readonly Color DARK_BG = new Color(0.02f, 0.04f, 0.08f, 1f);
        private static readonly Color CARD_BG = new Color(0.06f, 0.08f, 0.12f, 1f);
        private static readonly Color CARD_BG_LIGHT = new Color(0.08f, 0.10f, 0.14f, 1f);
        private static readonly Color INPUT_BG = new Color(0.08f, 0.10f, 0.15f, 1f);

        private static readonly Color TEXT_WHITE = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.6f, 0.65f, 1f);
        private static readonly Color TEXT_DARK = new Color(0.05f, 0.05f, 0.08f, 1f);

        private static readonly Color GREEN_SUCCESS = new Color(0.2f, 0.9f, 0.4f, 1f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color RED_ERROR = new Color(1f, 0.3f, 0.3f, 1f);

        #endregion

        #region Layout Anchors (Y: 0=bottom, 1=top)

        private const float PROGRESS_TOP = 0.993f;
        private const float PROGRESS_BOT = 0.990f;

        private const float TOPBAR_TOP = 0.988f;
        private const float TOPBAR_BOT = 0.955f;

        private const float ICON_TOP = 0.92f;
        private const float ICON_BOT = 0.63f;

        private const float TITLE_TOP = 0.61f;
        private const float TITLE_BOT = 0.50f;

        private const float CONTENT_TOP = 0.48f;
        private const float CONTENT_BOT = 0.13f;

        private const float DOTS_TOP = 0.095f;
        private const float DOTS_BOT = 0.065f;

        private const float NAV_TOP = 0.055f;
        private const float NAV_BOT = 0.015f;

        private const float SIDE_PAD = 30f;

        #endregion

        #region Paths

        private static readonly string[] SLIDE_ICONS =
        {
            "Assets/_Project/Art/Icons/Onboarding/WelcomeIcon.png",
            "Assets/_Project/Art/Icons/Social/ProfileIcon.png",
            "Assets/_Project/Art/Icons/Social/AvatarDefault.png",
            "Assets/_Project/Art/Icons/Onboarding/GamesIcon.png",
            "Assets/_Project/Art/Icons/Onboarding/CashBattleIcon.png",
            "Assets/_Project/Art/Icons/Onboarding/TournamentsIcon.png",
            "Assets/_Project/Art/Icons/Onboarding/RewardsIcon.png",
            "Assets/_Project/Art/Icons/Onboarding/CompleteIcon.png"
        };

        #endregion

        [MenuItem("DigitPark/Scenes/Build Scene/Onboarding/Main", false, 170)]
        public static void ShowWindow()
        {
            GetWindow<OnboardingUIBuilder>("Onboarding Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Main Onboarding UI Builder", EditorStyles.boldLabel);
            GUILayout.Label("Slide-Based Architecture (8 slides independientes)", EditorStyles.miniLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Arquitectura slide-based (como CashBattle):\n\n" +
                "Canvas\n" +
                "  Background\n" +
                "  ProgressBar (Slider, maxValue=7)\n" +
                "  TopBar (DIGITPARK 36px + StepCounter 28px + SkipButton 28px)\n" +
                "  SlidesContainer\n" +
                "    Slide1 (Welcome) - Slide8 (Completion)\n" +
                "  DotsContainer\n" +
                "  NavigationPanel (PrevButton 36px + NextButton 36px)",
                MessageType.Info);

            GUILayout.Space(15);

            GUI.backgroundColor = CYAN_NEON;
            if (GUILayout.Button("RECONSTRUIR ONBOARDING COMPLETO", GUILayout.Height(50)))
                RebuildOnboarding();
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);
            GUILayout.Label("Secciones individuales:", EditorStyles.boldLabel);

            if (GUILayout.Button("1. Background + TopBar + Progress", GUILayout.Height(25)))
            {
                Canvas c = UIBuilderCanvasHelper.FindMainCanvas();
                if (c != null) { CreateBackground(c.transform); CreateTopBar(); }
            }
            if (GUILayout.Button("2. SlidesContainer (8 slides)", GUILayout.Height(25)))
                CreateSlidesContainer();
            if (GUILayout.Button("3. Navigation (Dots + Buttons)", GUILayout.Height(25)))
                CreateNavigation();

            GUILayout.Space(15);

            GUI.backgroundColor = GOLD;
            if (GUILayout.Button("ASIGNAR REFERENCIAS AL MANAGER", GUILayout.Height(35)))
                SetupManagerReferences();
            GUI.backgroundColor = Color.white;
        }

        #region Main Rebuild

        private static void RebuildOnboarding()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("[OnboardingUI] No se encontro Canvas");
                return;
            }

            // Full clean of canvas children (keep TransitionCanvas and EventSystem)
            for (int i = canvas.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = canvas.transform.GetChild(i);
                if (child.name == "TransitionCanvas" || child.name == "EventSystem")
                    continue;
                DestroyImmediate(child.gameObject);
            }

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
            }

            CreateBackground(canvas.transform);
            CreateTopBar();
            CreateSlidesContainer();
            CreateNavigation();

            Debug.Log("[OnboardingUI] Onboarding RECONSTRUIDO (slide-based, 8 slides)!");
            EditorUtility.SetDirty(canvas.gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        }

        private static void CreateBackground(Transform parent)
        {
            var bg = FindOrCreate(parent, "Background");
            bg.transform.SetAsFirstSibling();
            var rt = GetOrAdd<RectTransform>(bg);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            GetOrAdd<Image>(bg).color = Color.white;
            ThemeApplierHelper.Apply(bg, ET.PrimaryBackground);
        }

        #endregion

        #region Top Bar (Progress + DIGITPARK Label + StepCounter + Skip)

        private static void CreateTopBar()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            // --- Progress Bar ---
            var progressGO = FindOrCreate(canvas.transform, "ProgressBar");
            var pRT = GetOrAdd<RectTransform>(progressGO);
            SetAnchors(pRT, 0, PROGRESS_BOT, 1, PROGRESS_TOP);

            var slider = GetOrAdd<Slider>(progressGO);
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0;
            slider.maxValue = 7;
            slider.wholeNumbers = true;
            slider.value = 0;
            slider.interactable = false;

            var sliderBg = FindOrCreate(progressGO.transform, "Background");
            var sbRT = GetOrAdd<RectTransform>(sliderBg);
            sbRT.anchorMin = Vector2.zero;
            sbRT.anchorMax = Vector2.one;
            sbRT.offsetMin = Vector2.zero;
            sbRT.offsetMax = Vector2.zero;
            GetOrAdd<Image>(sliderBg).color = new Color(0.1f, 0.12f, 0.15f, 1f);

            var fillArea = FindOrCreate(progressGO.transform, "Fill Area");
            var faRT = GetOrAdd<RectTransform>(fillArea);
            faRT.anchorMin = Vector2.zero;
            faRT.anchorMax = Vector2.one;
            faRT.offsetMin = Vector2.zero;
            faRT.offsetMax = Vector2.zero;

            var fill = FindOrCreate(fillArea.transform, "Fill");
            var fRT = GetOrAdd<RectTransform>(fill);
            fRT.anchorMin = Vector2.zero;
            fRT.anchorMax = Vector2.one;
            fRT.offsetMin = Vector2.zero;
            fRT.offsetMax = Vector2.zero;
            GetOrAdd<Image>(fill).color = CYAN_NEON;

            slider.fillRect = fRT;
            slider.handleRect = null;
            slider.targetGraphic = GetOrAdd<Image>(progressGO);
            GetOrAdd<Image>(progressGO).color = Color.clear;

            // --- Top Bar ---
            var topBar = FindOrCreate(canvas.transform, "TopBar");
            var tbRT = GetOrAdd<RectTransform>(topBar);
            SetAnchors(tbRT, 0, TOPBAR_BOT, 1, TOPBAR_TOP);

            // DIGITPARK label (left)
            var titleLabel = FindOrCreate(topBar.transform, "TitleLabel");
            var tlRT = GetOrAdd<RectTransform>(titleLabel);
            tlRT.anchorMin = new Vector2(0, 0);
            tlRT.anchorMax = new Vector2(0.40f, 1);
            tlRT.offsetMin = new Vector2(SIDE_PAD, 0);
            tlRT.offsetMax = Vector2.zero;
            var tlTMP = GetOrAdd<TextMeshProUGUI>(titleLabel);
            tlTMP.text = "DIGITPARK";
            tlTMP.fontSize = FontSizes.Body;
            tlTMP.color = CYAN_NEON;
            tlTMP.fontStyle = FontStyles.Bold;
            tlTMP.alignment = TextAlignmentOptions.Left;
            tlTMP.enableAutoSizing = true;
            tlTMP.fontSizeMin = FontSizes.AutoMinBody;
            tlTMP.fontSizeMax = FontSizes.Body;

            // Step Counter (center)
            var counter = FindOrCreate(topBar.transform, "StepCounter");
            var cRT = GetOrAdd<RectTransform>(counter);
            cRT.anchorMin = new Vector2(0.35f, 0);
            cRT.anchorMax = new Vector2(0.65f, 1);
            cRT.offsetMin = Vector2.zero;
            cRT.offsetMax = Vector2.zero;
            var cTMP = GetOrAdd<TextMeshProUGUI>(counter);
            cTMP.text = "1/8";
            cTMP.fontSize = FontSizes.Body;
            cTMP.color = TEXT_SECONDARY;
            cTMP.fontStyle = FontStyles.Bold;
            cTMP.alignment = TextAlignmentOptions.Center;
            cTMP.enableAutoSizing = true;
            cTMP.fontSizeMin = FontSizes.AutoMinBody;
            cTMP.fontSizeMax = FontSizes.Body;

            // Skip Button (right)
            var skipBtn = FindOrCreate(topBar.transform, "SkipButton");
            var sRT = GetOrAdd<RectTransform>(skipBtn);
            sRT.anchorMin = new Vector2(0.7f, 0.1f);
            sRT.anchorMax = new Vector2(1, 0.9f);
            sRT.offsetMin = new Vector2(0, 0);
            sRT.offsetMax = new Vector2(-SIDE_PAD, 0);
            var sBg = GetOrAdd<Image>(skipBtn);
            sBg.color = new Color(1, 1, 1, 0.05f);
            GetOrAdd<Button>(skipBtn).targetGraphic = sBg;

            var skipText = FindOrCreate(skipBtn.transform, "Text");
            var stRT = GetOrAdd<RectTransform>(skipText);
            stRT.anchorMin = Vector2.zero;
            stRT.anchorMax = Vector2.one;
            stRT.offsetMin = Vector2.zero;
            stRT.offsetMax = Vector2.zero;
            var stTMP = GetOrAdd<TextMeshProUGUI>(skipText);
            stTMP.text = "SKIP";
            stTMP.fontSize = FontSizes.Body;
            stTMP.color = new Color(CYAN_NEON.r, CYAN_NEON.g, CYAN_NEON.b, 0.7f);
            stTMP.fontStyle = FontStyles.Bold;
            stTMP.enableAutoSizing = true;
            stTMP.fontSizeMin = FontSizes.AutoMinBody;
            stTMP.fontSizeMax = FontSizes.Body;
            stTMP.alignment = TextAlignmentOptions.Center;

            Debug.Log("[OnboardingUI] TopBar creado (ProgressBar + DIGITPARK + StepCounter + Skip)");
        }

        #endregion

        #region Slides Container

        private static void CreateSlidesContainer()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var container = FindOrCreate(canvas.transform, "SlidesContainer");
            var cRT = GetOrAdd<RectTransform>(container);
            cRT.anchorMin = Vector2.zero;
            cRT.anchorMax = Vector2.one;
            cRT.offsetMin = Vector2.zero;
            cRT.offsetMax = Vector2.zero;

            // Clean old slides
            for (int i = container.transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(container.transform.GetChild(i).gameObject);

            CreateSlide1_Welcome(container.transform);
            CreateSlide2_Name(container.transform);
            CreateSlide3_Avatar(container.transform);
            CreateSlide4_Games(container.transform);
            CreateSlide5_CashBattle(container.transform);
            CreateSlide6_Tournaments(container.transform);
            CreateSlide7_Rewards(container.transform);
            CreateSlide8_Completion(container.transform);

            Debug.Log("[OnboardingUI] SlidesContainer creado (8 slides)");
        }

        // --- Slide Base: Icon + Title (shared by info slides) ---
        private static Transform CreateSlideBase(Transform parent, string name, int iconIndex, bool active)
        {
            var slide = FindOrCreate(parent, name);
            var sRT = GetOrAdd<RectTransform>(slide);
            sRT.anchorMin = Vector2.zero;
            sRT.anchorMax = Vector2.one;
            sRT.offsetMin = Vector2.zero;
            sRT.offsetMax = Vector2.zero;
            slide.SetActive(active);

            // Limpiar hijos anteriores del slide
            while (slide.transform.childCount > 0)
                DestroyImmediate(slide.transform.GetChild(0).gameObject);

            // IconGlow
            var glow = new GameObject("IconGlow");
            glow.transform.SetParent(slide.transform, false);
            var glRT = glow.AddComponent<RectTransform>();
            glRT.anchorMin = new Vector2(0.10f, ICON_BOT - 0.03f);
            glRT.anchorMax = new Vector2(0.90f, ICON_TOP + 0.01f);
            glRT.offsetMin = Vector2.zero;
            glRT.offsetMax = Vector2.zero;
            glow.AddComponent<Image>().color = CYAN_GLOW;

            // SlideIcon
            var icon = new GameObject("SlideIcon");
            icon.transform.SetParent(slide.transform, false);
            var iRT = icon.AddComponent<RectTransform>();
            iRT.anchorMin = new Vector2(0.20f, ICON_BOT);
            iRT.anchorMax = new Vector2(0.80f, ICON_TOP);
            iRT.offsetMin = Vector2.zero;
            iRT.offsetMax = Vector2.zero;
            var iImg = icon.AddComponent<Image>();
            iImg.color = Color.white;
            iImg.preserveAspect = true;

            if (iconIndex >= 0 && iconIndex < SLIDE_ICONS.Length)
            {
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SLIDE_ICONS[iconIndex]);
                if (sprite != null) iImg.sprite = sprite;
            }

            return slide.transform;
        }

        private static TextMeshProUGUI CreateSlideTitle(Transform slide, string text, Color color)
        {
            var titleGO = new GameObject("SlideTitle");
            titleGO.transform.SetParent(slide, false);
            var tRT = titleGO.AddComponent<RectTransform>();
            tRT.anchorMin = new Vector2(0.05f, TITLE_BOT);
            tRT.anchorMax = new Vector2(0.95f, TITLE_TOP);
            tRT.offsetMin = Vector2.zero;
            tRT.offsetMax = Vector2.zero;

            var tmp = titleGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = FontSizes.H4;
            tmp.color = color;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = true;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinTitle;
            tmp.fontSizeMax = FontSizes.H4;
            return tmp;
        }

        private static Transform CreateContentCard(Transform slide)
        {
            var card = new GameObject("ContentCard");
            card.transform.SetParent(slide, false);
            var cRT = card.AddComponent<RectTransform>();
            cRT.anchorMin = new Vector2(0.06f, CONTENT_BOT);
            cRT.anchorMax = new Vector2(0.94f, CONTENT_TOP);
            cRT.offsetMin = Vector2.zero;
            cRT.offsetMax = Vector2.zero;

            var bg = card.AddComponent<Image>();
            bg.color = CARD_BG;
            var outline = card.AddComponent<Outline>();
            outline.effectColor = CYAN_DARK;
            outline.effectDistance = new Vector2(1.5f, 1.5f);

            // Prevent content overflow beyond the card
            card.AddComponent<RectMask2D>();

            var content = new GameObject("Content");
            content.transform.SetParent(card.transform, false);
            var coRT = content.AddComponent<RectTransform>();
            coRT.anchorMin = new Vector2(0.04f, 0.03f);
            coRT.anchorMax = new Vector2(0.96f, 0.97f);
            coRT.offsetMin = Vector2.zero;
            coRT.offsetMax = Vector2.zero;

            var vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.padding = new RectOffset(15, 15, 8, 8);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            return content.transform;
        }

        private static void AddContentText(Transform parent, string text, int fontSize, Color color,
            TextAlignmentOptions align, bool bold = false)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            go.AddComponent<LayoutElement>().preferredHeight = fontSize + 14;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontSizeMin = FontSizes.AutoMinBody;
            tmp.enableAutoSizing = true;
            tmp.color = color;
            tmp.alignment = align;
            tmp.enableWordWrapping = true;
            tmp.fontStyle = FontStyles.Bold;
        }

        private static void AddContentSpacer(Transform parent, float height)
        {
            var go = new GameObject("Spacer");
            go.transform.SetParent(parent, false);
            go.AddComponent<LayoutElement>().preferredHeight = height;
        }

        private static void AddBulletText(Transform parent, string text)
        {
            AddContentText(parent, "\u2022 " + text, (int)FontSizes.Body, TEXT_WHITE, TextAlignmentOptions.Left);
        }

        #endregion

        #region Slide 1 - Welcome

        private static void CreateSlide1_Welcome(Transform parent)
        {
            var slide = CreateSlideBase(parent, "Slide1", 0, true); // active by default
            CreateSlideTitle(slide, "Welcome to DigitPark!", CYAN_NEON);

            var content = CreateContentCard(slide);
            AddContentText(content, "Your destination for brain games, competitions, and fun.",
                (int)FontSizes.Body, TEXT_WHITE, TextAlignmentOptions.Center);
            AddContentSpacer(content, 10);
            AddContentText(content, "TRAIN YOUR MIND AND COMPETE",
                (int)FontSizes.Subtitle, CYAN_NEON, TextAlignmentOptions.Center, true);
            AddContentSpacer(content, 14);
            AddBulletText(content, "5 cognitive mini-games");
            AddBulletText(content, "Compete against players from around the world");
            AddBulletText(content, "Daily rewards and achievements");
        }

        #endregion

        #region Slide 2 - Name Input

        private static void CreateSlide2_Name(Transform parent)
        {
            var slide = CreateSlideBase(parent, "Slide2", 1, false);
            CreateSlideTitle(slide, "What's your name?", CYAN_NEON);

            // NameInputPanel
            var panel = new GameObject("NameInputPanel");
            panel.transform.SetParent(slide, false);
            var pRT = panel.AddComponent<RectTransform>();
            pRT.anchorMin = new Vector2(0.08f, 0.18f);
            pRT.anchorMax = new Vector2(0.92f, 0.48f);
            pRT.offsetMin = Vector2.zero;
            pRT.offsetMax = Vector2.zero;

            var pBg = panel.AddComponent<Image>();
            pBg.color = CARD_BG;
            var pOutline = panel.AddComponent<Outline>();
            pOutline.effectColor = CYAN_DARK;
            pOutline.effectDistance = new Vector2(1.5f, 1.5f);

            // InputContainer
            var container = new GameObject("InputContainer");
            container.transform.SetParent(panel.transform, false);
            var cRT = container.AddComponent<RectTransform>();
            cRT.anchorMin = new Vector2(0.06f, 0.08f);
            cRT.anchorMax = new Vector2(0.94f, 0.92f);
            cRT.offsetMin = Vector2.zero;
            cRT.offsetMax = Vector2.zero;

            var vlg = container.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 12;
            vlg.padding = new RectOffset(0, 0, 10, 10);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // NameInput
            var inputGO = new GameObject("NameInput");
            inputGO.transform.SetParent(container.transform, false);
            inputGO.AddComponent<LayoutElement>().preferredHeight = 85;
            var inputBg = inputGO.AddComponent<Image>();
            inputBg.color = INPUT_BG;
            var inputOutline = inputGO.AddComponent<Outline>();
            inputOutline.effectColor = CYAN_DARK;
            inputOutline.effectDistance = new Vector2(1, 1);

            var input = inputGO.AddComponent<TMP_InputField>();
            input.contentType = TMP_InputField.ContentType.Standard;
            input.characterLimit = 20;

            var textArea = new GameObject("Text Area");
            textArea.transform.SetParent(inputGO.transform, false);
            var taRT = textArea.AddComponent<RectTransform>();
            taRT.anchorMin = Vector2.zero;
            taRT.anchorMax = Vector2.one;
            taRT.offsetMin = new Vector2(15, 5);
            taRT.offsetMax = new Vector2(-15, -5);
            textArea.AddComponent<RectMask2D>();

            var placeholder = new GameObject("Placeholder");
            placeholder.transform.SetParent(textArea.transform, false);
            var phRT = placeholder.AddComponent<RectTransform>();
            phRT.anchorMin = Vector2.zero;
            phRT.anchorMax = Vector2.one;
            phRT.offsetMin = Vector2.zero;
            phRT.offsetMax = Vector2.zero;
            var phTMP = placeholder.AddComponent<TextMeshProUGUI>();
            phTMP.text = "Enter your name...";
            phTMP.fontSize = FontSizes.BodyLarge;
            phTMP.color = new Color(0.4f, 0.4f, 0.45f, 1f);
            phTMP.fontStyle = FontStyles.Bold;
            phTMP.alignment = TextAlignmentOptions.Left;
            phTMP.enableAutoSizing = true;
            phTMP.fontSizeMin = FontSizes.AutoMinBody;
            phTMP.fontSizeMax = FontSizes.BodyLarge;

            var text = new GameObject("Text");
            text.transform.SetParent(textArea.transform, false);
            var txtRT = text.AddComponent<RectTransform>();
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.offsetMin = Vector2.zero;
            txtRT.offsetMax = Vector2.zero;
            var txtTMP = text.AddComponent<TextMeshProUGUI>();
            txtTMP.fontSize = FontSizes.BodyLarge;
            txtTMP.color = TEXT_WHITE;
            txtTMP.fontStyle = FontStyles.Bold;
            txtTMP.alignment = TextAlignmentOptions.Left;
            txtTMP.enableAutoSizing = true;
            txtTMP.fontSizeMin = FontSizes.AutoMinBody;
            txtTMP.fontSizeMax = txtTMP.fontSize;

            input.textViewport = taRT;
            input.textComponent = txtTMP;
            input.placeholder = phTMP;

            // ConfirmNameButton
            var confirmBtn = new GameObject("ConfirmNameButton");
            confirmBtn.transform.SetParent(container.transform, false);
            confirmBtn.AddComponent<LayoutElement>().preferredHeight = 70;
            var cbBg = confirmBtn.AddComponent<Image>();
            cbBg.color = CYAN_NEON;
            confirmBtn.AddComponent<Button>().targetGraphic = cbBg;
            var cbOutline = confirmBtn.AddComponent<Outline>();
            cbOutline.effectColor = CYAN_DARK;
            cbOutline.effectDistance = new Vector2(1, 1);

            var confirmText = new GameObject("Text");
            confirmText.transform.SetParent(confirmBtn.transform, false);
            var ctRT = confirmText.AddComponent<RectTransform>();
            ctRT.anchorMin = Vector2.zero;
            ctRT.anchorMax = Vector2.one;
            ctRT.offsetMin = Vector2.zero;
            ctRT.offsetMax = Vector2.zero;
            var ctTMP = confirmText.AddComponent<TextMeshProUGUI>();
            ctTMP.text = "CONFIRM";
            ctTMP.fontSize = FontSizes.Body;
            ctTMP.color = TEXT_DARK;
            ctTMP.fontStyle = FontStyles.Bold;
            ctTMP.alignment = TextAlignmentOptions.Center;
            ctTMP.enableAutoSizing = true;
            ctTMP.fontSizeMin = FontSizes.AutoMinBody;
            ctTMP.fontSizeMax = FontSizes.Body;

            // NameErrorText
            var errorText = new GameObject("NameErrorText");
            errorText.transform.SetParent(container.transform, false);
            errorText.AddComponent<LayoutElement>().preferredHeight = 40;
            var eTMP = errorText.AddComponent<TextMeshProUGUI>();
            eTMP.text = "";
            eTMP.fontSize = FontSizes.Body;
            eTMP.color = RED_ERROR;
            eTMP.fontStyle = FontStyles.Bold;
            eTMP.alignment = TextAlignmentOptions.Center;
            eTMP.enableAutoSizing = true;
            eTMP.fontSizeMin = FontSizes.AutoMinSmall;
            eTMP.fontSizeMax = FontSizes.Body;
            errorText.SetActive(false);
        }

        #endregion

        #region Slide 3 - Avatar Selection

        private static void CreateSlide3_Avatar(Transform parent)
        {
            var slide = CreateSlideBase(parent, "Slide3", 2, false);
            CreateSlideTitle(slide, "Choose your Avatar", CYAN_NEON);

            // AvatarSelectionPanel
            var panel = new GameObject("AvatarSelectionPanel");
            panel.transform.SetParent(slide, false);
            var pRT = panel.AddComponent<RectTransform>();
            pRT.anchorMin = new Vector2(0.05f, 0.14f);
            pRT.anchorMax = new Vector2(0.95f, 0.48f);
            pRT.offsetMin = Vector2.zero;
            pRT.offsetMax = Vector2.zero;

            var grid = new GameObject("AvatarContainer");
            grid.transform.SetParent(panel.transform, false);
            var gRT = grid.AddComponent<RectTransform>();
            gRT.anchorMin = Vector2.zero;
            gRT.anchorMax = Vector2.one;
            gRT.offsetMin = new Vector2(10, 10);
            gRT.offsetMax = new Vector2(-10, -10);

            var gridLayout = grid.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(150, 180);
            gridLayout.spacing = new Vector2(20, 15);
            gridLayout.padding = new RectOffset(10, 10, 5, 5);
            gridLayout.childAlignment = TextAnchor.MiddleCenter;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 3;
        }

        #endregion

        #region Slide 4 - Games

        private static void CreateSlide4_Games(Transform parent)
        {
            var slide = CreateSlideBase(parent, "Slide4", 3, false);
            CreateSlideTitle(slide, "Cognitive Games", CYAN_NEON);

            var content = CreateContentCard(slide);
            AddContentText(content, "Train your mind with mini-games designed to improve your skills.",
                (int)FontSizes.Body, TEXT_WHITE, TextAlignmentOptions.Center);
            AddContentSpacer(content, 10);
            AddContentText(content, "6 UNIQUE MINI-GAMES",
                (int)FontSizes.Subtitle, CYAN_NEON, TextAlignmentOptions.Center, true);
            AddContentSpacer(content, 14);
            AddBulletText(content, "DigitRush: Speed and reflexes");
            AddBulletText(content, "FlashTap: Quick reaction");
            AddBulletText(content, "MemoryPairs: Visual memory");
            AddBulletText(content, "QuickMath: Mental math");
            AddBulletText(content, "OddOneOut: Attention to detail");
        }

        #endregion

        #region Slide 5 - CashBattle

        private static void CreateSlide5_CashBattle(Transform parent)
        {
            var slide = CreateSlideBase(parent, "Slide5", 4, false);
            CreateSlideTitle(slide, "CashBattle", CYAN_NEON);

            var content = CreateContentCard(slide);
            AddContentText(content, "Ready for the challenge? Compete 1v1 against other players for real prizes.",
                (int)FontSizes.Body, TEXT_WHITE, TextAlignmentOptions.Center);
            AddContentSpacer(content, 10);
            AddContentText(content, "COMPETE FOR REAL PRIZES",
                (int)FontSizes.Subtitle, CYAN_NEON, TextAlignmentOptions.Center, true);
            AddContentSpacer(content, 14);
            AddBulletText(content, "Skill-based 1v1 matchmaking");
            AddBulletText(content, "Bets starting at $1 USD");
            AddBulletText(content, "Secure and verified platform");
        }

        #endregion

        #region Slide 6 - Tournaments

        private static void CreateSlide6_Tournaments(Transform parent)
        {
            var slide = CreateSlideBase(parent, "Slide6", 5, false);
            CreateSlideTitle(slide, "Tournaments", CYAN_NEON);

            var content = CreateContentCard(slide);
            AddContentText(content, "Join tournaments with dozens of players.",
                (int)FontSizes.Body, TEXT_WHITE, TextAlignmentOptions.Center);
            AddContentSpacer(content, 10);
            AddContentText(content, "WIN BIG PRIZES",
                (int)FontSizes.Subtitle, CYAN_NEON, TextAlignmentOptions.Center, true);
            AddContentSpacer(content, 14);
            AddBulletText(content, "Up to 256 players per tournament");
            AddBulletText(content, "Guaranteed prizes");
            AddBulletText(content, "Professional bracket system");
        }

        #endregion

        #region Slide 7 - Rewards

        private static void CreateSlide7_Rewards(Transform parent)
        {
            var slide = CreateSlideBase(parent, "Slide7", 6, false);
            CreateSlideTitle(slide, "Daily Rewards", CYAN_NEON);

            var content = CreateContentCard(slide);
            AddContentText(content, "Come back every day for coins, gems, and more rewards.",
                (int)FontSizes.Body, TEXT_WHITE, TextAlignmentOptions.Center);
            AddContentSpacer(content, 10);
            AddContentText(content, "REWARDS EVERY DAY",
                (int)FontSizes.Subtitle, CYAN_NEON, TextAlignmentOptions.Center, true);
            AddContentSpacer(content, 14);
            AddBulletText(content, "Free coins every day");
            AddBulletText(content, "Weekly gem bonuses");
            AddBulletText(content, "Daily missions with prizes");
        }

        #endregion

        #region Slide 8 - Completion

        private static void CreateSlide8_Completion(Transform parent)
        {
            var slide = FindOrCreate(parent, "Slide8");
            var sRT = GetOrAdd<RectTransform>(slide);
            sRT.anchorMin = Vector2.zero;
            sRT.anchorMax = Vector2.one;
            sRT.offsetMin = Vector2.zero;
            sRT.offsetMax = Vector2.zero;
            slide.SetActive(false);

            // CompletionPanel (full content area)
            var panel = new GameObject("CompletionPanel");
            panel.transform.SetParent(slide.transform, false);
            var pRT = panel.AddComponent<RectTransform>();
            pRT.anchorMin = new Vector2(0.05f, 0.12f);
            pRT.anchorMax = new Vector2(0.95f, 0.94f);
            pRT.offsetMin = Vector2.zero;
            pRT.offsetMax = Vector2.zero;

            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 15;
            vlg.padding = new RectOffset(20, 20, 30, 20);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // CompletionIcon
            var iconGO = new GameObject("CompletionIcon");
            iconGO.transform.SetParent(panel.transform, false);
            iconGO.AddComponent<LayoutElement>().preferredHeight = 200;
            var iconImg = iconGO.AddComponent<Image>();
            iconImg.color = Color.white;
            iconImg.preserveAspect = true;
            Sprite completeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SLIDE_ICONS[7]);
            if (completeSprite != null) iconImg.sprite = completeSprite;

            // CompletionTitle
            var compTitle = new GameObject("CompletionTitle");
            compTitle.transform.SetParent(panel.transform, false);
            compTitle.AddComponent<LayoutElement>().preferredHeight = 90;
            var ctTMP = compTitle.AddComponent<TextMeshProUGUI>();
            ctTMP.text = "Well done!";
            ctTMP.fontSize = FontSizes.H1;
            ctTMP.color = CYAN_NEON;
            ctTMP.fontStyle = FontStyles.Bold;
            ctTMP.alignment = TextAlignmentOptions.Center;
            ctTMP.enableAutoSizing = true;
            ctTMP.fontSizeMin = FontSizes.H4;
            ctTMP.fontSizeMax = FontSizes.H1;

            // CompletionMessage
            var compMsg = new GameObject("CompletionMessage");
            compMsg.transform.SetParent(panel.transform, false);
            compMsg.AddComponent<LayoutElement>().preferredHeight = 70;
            var cmTMP = compMsg.AddComponent<TextMeshProUGUI>();
            cmTMP.text = "You've completed the tutorial.\nHere are your welcome rewards.";
            cmTMP.fontSize = FontSizes.Body;
            cmTMP.color = TEXT_WHITE;
            cmTMP.fontStyle = FontStyles.Bold;
            cmTMP.alignment = TextAlignmentOptions.Center;
            cmTMP.enableWordWrapping = true;
            cmTMP.enableAutoSizing = true;
            cmTMP.fontSizeMin = FontSizes.AutoMinSmall;
            cmTMP.fontSizeMax = FontSizes.Body;

            // RewardsCard
            var rewardsCard = new GameObject("RewardsCard");
            rewardsCard.transform.SetParent(panel.transform, false);
            rewardsCard.AddComponent<LayoutElement>().preferredHeight = 100;
            var rcBg = rewardsCard.AddComponent<Image>();
            rcBg.color = CARD_BG;
            var rcOutline = rewardsCard.AddComponent<Outline>();
            rcOutline.effectColor = GOLD;
            rcOutline.effectDistance = new Vector2(2, 2);

            var rewardText = new GameObject("RewardText");
            rewardText.transform.SetParent(rewardsCard.transform, false);
            var rwRT = rewardText.AddComponent<RectTransform>();
            rwRT.anchorMin = Vector2.zero;
            rwRT.anchorMax = Vector2.one;
            rwRT.offsetMin = new Vector2(10, 10);
            rwRT.offsetMax = new Vector2(-10, -10);
            var rwTMP = rewardText.AddComponent<TextMeshProUGUI>();
            rwTMP.text = "+500 DigitCoins  |  +50 DigitGems";
            rwTMP.fontSize = FontSizes.Subtitle;
            rwTMP.color = GOLD;
            rwTMP.fontStyle = FontStyles.Bold;
            rwTMP.alignment = TextAlignmentOptions.Center;
            rwTMP.enableAutoSizing = true;
            rwTMP.fontSizeMin = FontSizes.AutoMinBody;
            rwTMP.fontSizeMax = FontSizes.Subtitle;

            // Spacer
            var spacer = new GameObject("Spacer");
            spacer.transform.SetParent(panel.transform, false);
            spacer.AddComponent<LayoutElement>().preferredHeight = 30;

            // StartPlayingButton
            var startBtn = new GameObject("StartPlayingButton");
            startBtn.transform.SetParent(panel.transform, false);
            startBtn.AddComponent<LayoutElement>().preferredHeight = 80;
            var sbBg = startBtn.AddComponent<Image>();
            sbBg.color = GREEN_SUCCESS;
            startBtn.AddComponent<Button>().targetGraphic = sbBg;
            var sbOutline = startBtn.AddComponent<Outline>();
            sbOutline.effectColor = new Color(0.1f, 0.5f, 0.2f, 1f);
            sbOutline.effectDistance = new Vector2(1.5f, 1.5f);

            var startText = new GameObject("Text");
            startText.transform.SetParent(startBtn.transform, false);
            var spRT = startText.AddComponent<RectTransform>();
            spRT.anchorMin = Vector2.zero;
            spRT.anchorMax = Vector2.one;
            spRT.offsetMin = Vector2.zero;
            spRT.offsetMax = Vector2.zero;
            var spTMP = startText.AddComponent<TextMeshProUGUI>();
            spTMP.text = "START PLAYING!";
            spTMP.fontSize = FontSizes.H4;
            spTMP.color = TEXT_DARK;
            spTMP.fontStyle = FontStyles.Bold;
            spTMP.alignment = TextAlignmentOptions.Center;
            spTMP.enableAutoSizing = true;
            spTMP.fontSizeMin = FontSizes.AutoMinBody;
            spTMP.fontSizeMax = FontSizes.H4;
        }

        #endregion

        #region Navigation (Dots + Buttons)

        private static void CreateNavigation()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            // --- Dots Container ---
            var dots = FindOrCreate(canvas.transform, "DotsContainer");
            var doRT = GetOrAdd<RectTransform>(dots);
            doRT.anchorMin = new Vector2(0.15f, DOTS_BOT);
            doRT.anchorMax = new Vector2(0.85f, DOTS_TOP);
            doRT.offsetMin = Vector2.zero;
            doRT.offsetMax = Vector2.zero;

            var hlg = GetOrAdd<HorizontalLayoutGroup>(dots);
            hlg.spacing = 12;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // --- Navigation Buttons Panel ---
            var navPanel = FindOrCreate(canvas.transform, "NavigationPanel");
            var npRT = GetOrAdd<RectTransform>(navPanel);
            npRT.anchorMin = new Vector2(0, NAV_BOT);
            npRT.anchorMax = new Vector2(1, NAV_TOP);
            npRT.offsetMin = new Vector2(SIDE_PAD, 0);
            npRT.offsetMax = new Vector2(-SIDE_PAD, 0);

            // Prev Button (left)
            var prevBtn = FindOrCreate(navPanel.transform, "PrevButton");
            var pbRT = GetOrAdd<RectTransform>(prevBtn);
            pbRT.anchorMin = new Vector2(0, 0);
            pbRT.anchorMax = new Vector2(0.47f, 1);
            pbRT.offsetMin = Vector2.zero;
            pbRT.offsetMax = Vector2.zero;
            var pbBg = GetOrAdd<Image>(prevBtn);
            pbBg.color = CARD_BG_LIGHT;
            GetOrAdd<Button>(prevBtn).targetGraphic = pbBg;
            var pbOutline = GetOrAdd<Outline>(prevBtn);
            pbOutline.effectColor = new Color(0.3f, 0.3f, 0.35f, 0.5f);
            pbOutline.effectDistance = new Vector2(1, 1);

            var prevText = FindOrCreate(prevBtn.transform, "Text");
            var ptRT = GetOrAdd<RectTransform>(prevText);
            ptRT.anchorMin = Vector2.zero;
            ptRT.anchorMax = Vector2.one;
            ptRT.offsetMin = Vector2.zero;
            ptRT.offsetMax = Vector2.zero;
            var ptTMP = GetOrAdd<TextMeshProUGUI>(prevText);
            ptTMP.text = "BACK";
            ptTMP.fontSize = FontSizes.Body;
            ptTMP.color = TEXT_SECONDARY;
            ptTMP.fontStyle = FontStyles.Bold;
            ptTMP.alignment = TextAlignmentOptions.Center;
            ptTMP.enableAutoSizing = true;
            ptTMP.fontSizeMin = FontSizes.AutoMinBody;
            ptTMP.fontSizeMax = FontSizes.Body;

            // Next Button (right, cyan)
            var nextBtn = FindOrCreate(navPanel.transform, "NextButton");
            var nbRT = GetOrAdd<RectTransform>(nextBtn);
            nbRT.anchorMin = new Vector2(0.53f, 0);
            nbRT.anchorMax = new Vector2(1, 1);
            nbRT.offsetMin = Vector2.zero;
            nbRT.offsetMax = Vector2.zero;
            var nbBg = GetOrAdd<Image>(nextBtn);
            nbBg.color = CYAN_NEON;
            GetOrAdd<Button>(nextBtn).targetGraphic = nbBg;
            var nbOutline = GetOrAdd<Outline>(nextBtn);
            nbOutline.effectColor = CYAN_DARK;
            nbOutline.effectDistance = new Vector2(1.5f, 1.5f);

            var nextText = FindOrCreate(nextBtn.transform, "Text");
            var ntRT = GetOrAdd<RectTransform>(nextText);
            ntRT.anchorMin = Vector2.zero;
            ntRT.anchorMax = Vector2.one;
            ntRT.offsetMin = Vector2.zero;
            ntRT.offsetMax = Vector2.zero;
            var ntTMP = GetOrAdd<TextMeshProUGUI>(nextText);
            ntTMP.text = "NEXT";
            ntTMP.fontSize = FontSizes.Body;
            ntTMP.color = TEXT_DARK;
            ntTMP.fontStyle = FontStyles.Bold;
            ntTMP.alignment = TextAlignmentOptions.Center;
            ntTMP.enableAutoSizing = true;
            ntTMP.fontSizeMin = FontSizes.AutoMinBody;
            ntTMP.fontSizeMax = FontSizes.Body;

            Debug.Log("[OnboardingUI] Navigation creado (DotsContainer + PrevButton + NextButton)");
        }

        #endregion

        #region Manager References

        private static void SetupManagerReferences()
        {
            var manager = Object.FindFirstObjectByType<DigitPark.Managers.OnboardingManager>();
            if (manager == null)
            {
                Debug.LogWarning("[OnboardingUI] OnboardingManager no encontrado. Agrega el componente primero.");
                return;
            }

            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var so = new SerializedObject(manager);
            Transform r = canvas.transform;

            // UI - Main
            SetRef(so, "skipButton", FindInPath<Button>(r, "TopBar/SkipButton"));
            SetRef(so, "skipButtonText", FindInPath<TextMeshProUGUI>(r, "TopBar/SkipButton/Text"));

            // UI - Slides Container
            Transform slidesT = r.Find("SlidesContainer");
            if (slidesT != null) SetRef(so, "slidesContainer", slidesT);

            // UI - Navigation
            SetRef(so, "nextButton", FindInPath<Button>(r, "NavigationPanel/NextButton"));
            SetRef(so, "prevButton", FindInPath<Button>(r, "NavigationPanel/PrevButton"));
            SetRef(so, "nextButtonText", FindInPath<TextMeshProUGUI>(r, "NavigationPanel/NextButton/Text"));
            SetRef(so, "prevButtonText", FindInPath<TextMeshProUGUI>(r, "NavigationPanel/PrevButton/Text"));
            Transform dotsT = r.Find("DotsContainer");
            if (dotsT != null) SetRef(so, "dotsContainer", dotsT);

            // UI - Progress
            SetRef(so, "progressBar", FindInPath<Slider>(r, "ProgressBar"));
            SetRef(so, "stepCounterText", FindInPath<TextMeshProUGUI>(r, "TopBar/StepCounter"));

            // UI - Name Input (inside Slide2)
            Transform namePanel = r.Find("SlidesContainer/Slide2/NameInputPanel");
            if (namePanel != null) SetRef(so, "nameInputPanel", namePanel.gameObject);
            SetRef(so, "nameInput", FindInPath<TMP_InputField>(r, "SlidesContainer/Slide2/NameInputPanel/InputContainer/NameInput"));
            SetRef(so, "confirmNameButton", FindInPath<Button>(r, "SlidesContainer/Slide2/NameInputPanel/InputContainer/ConfirmNameButton"));
            SetRef(so, "nameErrorText", FindInPath<TextMeshProUGUI>(r, "SlidesContainer/Slide2/NameInputPanel/InputContainer/NameErrorText"));

            // UI - Avatar Selection (inside Slide3)
            Transform avatarPanel = r.Find("SlidesContainer/Slide3/AvatarSelectionPanel");
            if (avatarPanel != null) SetRef(so, "avatarSelectionPanel", avatarPanel.gameObject);
            Transform avatarGrid = r.Find("SlidesContainer/Slide3/AvatarSelectionPanel/AvatarContainer");
            if (avatarGrid != null) SetRef(so, "avatarContainer", avatarGrid);

            // UI - Tutorial Completion (inside Slide8)
            Transform compPanel = r.Find("SlidesContainer/Slide8/CompletionPanel");
            if (compPanel != null) SetRef(so, "completionPanel", compPanel.gameObject);
            SetRef(so, "completionTitleText", FindInPath<TextMeshProUGUI>(r, "SlidesContainer/Slide8/CompletionPanel/CompletionTitle"));
            SetRef(so, "completionMessageText", FindInPath<TextMeshProUGUI>(r, "SlidesContainer/Slide8/CompletionPanel/CompletionMessage"));
            SetRef(so, "rewardText", FindInPath<TextMeshProUGUI>(r, "SlidesContainer/Slide8/CompletionPanel/RewardsCard/RewardText"));
            SetRef(so, "startPlayingButton", FindInPath<Button>(r, "SlidesContainer/Slide8/CompletionPanel/StartPlayingButton"));

            // UI - Sections (for animations)
            Transform progressBarT = r.Find("ProgressBar");
            if (progressBarT != null) SetRef(so, "progressBarTransform", progressBarT.GetComponent<RectTransform>());
            Transform topBarT = r.Find("TopBar");
            if (topBarT != null) SetRef(so, "topBarTransform", topBarT.GetComponent<RectTransform>());
            if (dotsT != null) SetRef(so, "dotsTransform", dotsT.GetComponent<RectTransform>());
            Transform navPanelT = r.Find("NavigationPanel");
            if (navPanelT != null) SetRef(so, "navigationTransform", navPanelT.GetComponent<RectTransform>());

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            Debug.Log("[OnboardingUI] Referencias del manager asignadas (slide-based)");
        }

        #endregion

        #region Helpers

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

        private static void SetRef(SerializedObject so, string propName, Object value)
        {
            var prop = so.FindProperty(propName);
            if (prop == null) { Debug.LogWarning($"[OnboardingUI] Property '{propName}' no encontrada"); return; }
            if (value != null) { prop.objectReferenceValue = value; }
            else { Debug.LogWarning($"[OnboardingUI] No se encontro valor para: {propName}"); }
        }

        private static T FindInPath<T>(Transform root, string path) where T : Component
        {
            Transform t = root;
            foreach (string part in path.Split('/'))
            {
                t = t.Find(part);
                if (t == null) return null;
            }
            return t.GetComponent<T>();
        }

        private static GameObject FindOrCreate(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null) return existing.gameObject;
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static T GetOrAdd<T>(GameObject obj) where T : Component
        {
            T c = obj.GetComponent<T>();
            if (c == null) c = obj.AddComponent<T>();
            return c;
        }

        private static void SetAnchors(RectTransform rt, float xMin, float yMin, float xMax, float yMax)
        {
            rt.anchorMin = new Vector2(xMin, yMin);
            rt.anchorMax = new Vector2(xMax, yMax);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        #endregion
    }
}
