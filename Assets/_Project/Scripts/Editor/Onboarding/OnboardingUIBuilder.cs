using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Main Onboarding UI Builder - Rediseño completo
    /// Layout: ProgressBar → TopBar → StepImage → Title → Description → Panels → Navigation
    /// Soporta 8 steps del OnboardingManager:
    ///   welcome, name, avatar, games, cashbattle, tournaments, rewards, complete
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

        private const float IMAGE_TOP = 0.94f;
        private const float IMAGE_BOT = 0.58f;

        private const float TITLE_TOP = 0.56f;
        private const float TITLE_BOT = 0.47f;

        private const float DESC_TOP = 0.46f;
        private const float DESC_BOT = 0.28f;

        private const float DOTS_TOP = 0.095f;
        private const float DOTS_BOT = 0.065f;

        private const float NAV_TOP = 0.055f;
        private const float NAV_BOT = 0.015f;

        private const float SIDE_PAD = 30f;

        #endregion

        #region Paths

        private const string ICONS_PATH = "Assets/_Project/Art/Icons/Onboarding/";
        private const string GAME_ICONS_PATH = "Assets/_Project/Art/Icons/Games/";

        #endregion

        [MenuItem("DigitPark/UI Builders/Onboarding/Main Onboarding", false, 180)]
        public static void ShowWindow()
        {
            GetWindow<OnboardingUIBuilder>("Onboarding Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Main Onboarding UI Builder", EditorStyles.boldLabel);
            GUILayout.Label("Tutorial nuevos usuarios - Neon Cyan - REDISEÑO", EditorStyles.miniLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Nuevo diseño (de arriba a abajo):\n\n" +
                "1. Progress Bar (línea delgada cyan)\n" +
                "2. Top Bar (contador de pasos + saltar)\n" +
                "3. Step Image (icono grande centrado)\n" +
                "4. Title + Description (texto grande legible)\n" +
                "5. Paneles especiales (nombre, avatar, completado)\n" +
                "6. Navigation (dots + prev/next)\n\n" +
                "Soporta 8 steps: welcome, name, avatar, games,\n" +
                "cashbattle, tournaments, rewards, complete",
                MessageType.Info);

            GUILayout.Space(15);

            GUI.backgroundColor = CYAN_NEON;
            if (GUILayout.Button("RECONSTRUIR ONBOARDING COMPLETO", GUILayout.Height(50)))
                RebuildOnboarding();
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);
            GUILayout.Label("Secciones individuales:", EditorStyles.boldLabel);

            if (GUILayout.Button("1. Background + Top Bar", GUILayout.Height(25)))
            {
                Canvas c = UIBuilderCanvasHelper.FindMainCanvas();
                if (c != null) { CreateBackground(c.transform); CreateTopBar(); }
            }
            if (GUILayout.Button("2. Content Area (Image + Text)", GUILayout.Height(25))) CreateContentArea();
            if (GUILayout.Button("3. Name Input Panel", GUILayout.Height(25))) CreateNameInputPanel();
            if (GUILayout.Button("4. Avatar Selection Panel", GUILayout.Height(25))) CreateAvatarSelectionPanel();
            if (GUILayout.Button("5. Completion Panel", GUILayout.Height(25))) CreateCompletionPanel();
            if (GUILayout.Button("6. Navigation (Dots + Buttons)", GUILayout.Height(25))) CreateNavigation();

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

            CleanupOldUI();

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0f;
            }

            // Limpiar elementos anteriores
            string[] oldNames = {
                "Background", "ProgressBar", "TopBar", "StepImage", "IconGlow",
                "TitleText", "DescriptionText", "NameInputPanel", "AvatarSelectionPanel",
                "CompletionPanel", "DotsContainer", "NavigationPanel",
                "SafeArea", "SlidesContainer", "NavigationPanel", "WelcomeGiftBlocker"
            };
            foreach (var n in oldNames)
            {
                Transform t = canvas.transform.Find(n);
                if (t != null) DestroyImmediate(t.gameObject);
            }

            CreateBackground(canvas.transform);
            CreateTopBar();
            CreateContentArea();
            CreateNameInputPanel();
            CreateAvatarSelectionPanel();
            CreateCompletionPanel();
            CreateNavigation();
            SetupManagerReferences();

            Debug.Log("[OnboardingUI] Onboarding RECONSTRUIDO exitosamente!");
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
            GetOrAdd<Image>(bg).color = DARK_BG;
        }

        #endregion

        #region Top Bar (Progress + StepCounter + Skip)

        private static void CreateTopBar()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            // --- Progress Bar (thin cyan line at very top) ---
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

            // Slider Background (dark track)
            var sliderBg = FindOrCreate(progressGO.transform, "Background");
            var sbRT = GetOrAdd<RectTransform>(sliderBg);
            sbRT.anchorMin = Vector2.zero;
            sbRT.anchorMax = Vector2.one;
            sbRT.offsetMin = Vector2.zero;
            sbRT.offsetMax = Vector2.zero;
            GetOrAdd<Image>(sliderBg).color = new Color(0.1f, 0.12f, 0.15f, 1f);

            // Fill Area
            var fillArea = FindOrCreate(progressGO.transform, "Fill Area");
            var faRT = GetOrAdd<RectTransform>(fillArea);
            faRT.anchorMin = Vector2.zero;
            faRT.anchorMax = new Vector2(1, 1);
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

            // --- Top Bar (step counter + skip button) ---
            var topBar = FindOrCreate(canvas.transform, "TopBar");
            var tbRT = GetOrAdd<RectTransform>(topBar);
            SetAnchors(tbRT, 0, TOPBAR_BOT, 1, TOPBAR_TOP);

            // Step Counter (left)
            var counter = FindOrCreate(topBar.transform, "StepCounter");
            var cRT = GetOrAdd<RectTransform>(counter);
            cRT.anchorMin = new Vector2(0, 0);
            cRT.anchorMax = new Vector2(0.3f, 1);
            cRT.offsetMin = new Vector2(SIDE_PAD, 0);
            cRT.offsetMax = Vector2.zero;
            var cTMP = GetOrAdd<TextMeshProUGUI>(counter);
            cTMP.text = "1/8";
            cTMP.fontSize = 18;
            cTMP.color = TEXT_SECONDARY;
            cTMP.alignment = TextAlignmentOptions.Left;

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
            stTMP.text = "SALTAR";
            stTMP.fontSize = 16;
            stTMP.color = new Color(CYAN_NEON.r, CYAN_NEON.g, CYAN_NEON.b, 0.7f);
            stTMP.fontStyle = FontStyles.Bold;
            stTMP.alignment = TextAlignmentOptions.Center;

            Debug.Log("[OnboardingUI] TopBar creado (ProgressBar + StepCounter + Skip)");
        }

        #endregion

        #region Content Area (StepImage + Title + Description)

        private static void CreateContentArea()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            // --- Icon Glow (subtle glow behind step image) ---
            var glow = FindOrCreate(canvas.transform, "IconGlow");
            var glRT = GetOrAdd<RectTransform>(glow);
            glRT.anchorMin = new Vector2(0.10f, IMAGE_BOT - 0.03f);
            glRT.anchorMax = new Vector2(0.90f, IMAGE_TOP + 0.01f);
            glRT.offsetMin = Vector2.zero;
            glRT.offsetMax = Vector2.zero;
            GetOrAdd<Image>(glow).color = CYAN_GLOW;

            // --- Step Image (large centered icon) ---
            var stepImg = FindOrCreate(canvas.transform, "StepImage");
            var siRT = GetOrAdd<RectTransform>(stepImg);
            siRT.anchorMin = new Vector2(0.20f, IMAGE_BOT);
            siRT.anchorMax = new Vector2(0.80f, IMAGE_TOP);
            siRT.offsetMin = Vector2.zero;
            siRT.offsetMax = Vector2.zero;
            var siImg = GetOrAdd<Image>(stepImg);
            siImg.color = Color.white;
            siImg.preserveAspect = true;

            // Try load welcome icon
            Sprite welcomeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ICONS_PATH + "WelcomeIcon.png");
            if (welcomeSprite != null) siImg.sprite = welcomeSprite;

            // --- Title Text ---
            var title = FindOrCreate(canvas.transform, "TitleText");
            var tRT = GetOrAdd<RectTransform>(title);
            tRT.anchorMin = new Vector2(0.05f, TITLE_BOT);
            tRT.anchorMax = new Vector2(0.95f, TITLE_TOP);
            tRT.offsetMin = Vector2.zero;
            tRT.offsetMax = Vector2.zero;
            var tTMP = GetOrAdd<TextMeshProUGUI>(title);
            tTMP.text = "\u00A1Bienvenido a DigitPark!";
            tTMP.fontSize = 34;
            tTMP.color = CYAN_NEON;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.Center;
            tTMP.enableWordWrapping = true;

            // --- Description Text ---
            var desc = FindOrCreate(canvas.transform, "DescriptionText");
            var dRT = GetOrAdd<RectTransform>(desc);
            dRT.anchorMin = new Vector2(0.08f, DESC_BOT);
            dRT.anchorMax = new Vector2(0.92f, DESC_TOP);
            dRT.offsetMin = Vector2.zero;
            dRT.offsetMax = Vector2.zero;
            var dTMP = GetOrAdd<TextMeshProUGUI>(desc);
            dTMP.text = "Tu destino para juegos mentales, competencias y diversi\u00F3n.";
            dTMP.fontSize = 20;
            dTMP.color = TEXT_WHITE;
            dTMP.alignment = TextAlignmentOptions.Center;
            dTMP.enableWordWrapping = true;

            Debug.Log("[OnboardingUI] ContentArea creado (IconGlow + StepImage + Title + Description)");
        }

        #endregion

        #region Name Input Panel

        private static void CreateNameInputPanel()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var panel = FindOrCreate(canvas.transform, "NameInputPanel");
            var pRT = GetOrAdd<RectTransform>(panel);
            pRT.anchorMin = new Vector2(0.08f, 0.22f);
            pRT.anchorMax = new Vector2(0.92f, 0.55f);
            pRT.offsetMin = Vector2.zero;
            pRT.offsetMax = Vector2.zero;
            panel.SetActive(false);

            // Card background
            var pBg = GetOrAdd<Image>(panel);
            pBg.color = CARD_BG;
            var pOutline = GetOrAdd<Outline>(panel);
            pOutline.effectColor = CYAN_DARK;
            pOutline.effectDistance = new Vector2(1.5f, 1.5f);

            // Input container (with VLG)
            var container = FindOrCreate(panel.transform, "InputContainer");
            var cRT = GetOrAdd<RectTransform>(container);
            cRT.anchorMin = new Vector2(0.06f, 0.08f);
            cRT.anchorMax = new Vector2(0.94f, 0.92f);
            cRT.offsetMin = Vector2.zero;
            cRT.offsetMax = Vector2.zero;

            var vlg = GetOrAdd<VerticalLayoutGroup>(container);
            vlg.spacing = 12;
            vlg.padding = new RectOffset(0, 0, 10, 10);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // --- Name Input Field ---
            var inputGO = FindOrCreate(container.transform, "NameInput");
            GetOrAdd<LayoutElement>(inputGO).preferredHeight = 70;
            var inputBg = GetOrAdd<Image>(inputGO);
            inputBg.color = INPUT_BG;
            var inputOutline = GetOrAdd<Outline>(inputGO);
            inputOutline.effectColor = CYAN_DARK;
            inputOutline.effectDistance = new Vector2(1, 1);

            var input = GetOrAdd<TMP_InputField>(inputGO);
            input.contentType = TMP_InputField.ContentType.Standard;
            input.characterLimit = 20;

            // Text Area
            var textArea = FindOrCreate(inputGO.transform, "Text Area");
            var taRT = GetOrAdd<RectTransform>(textArea);
            taRT.anchorMin = Vector2.zero;
            taRT.anchorMax = Vector2.one;
            taRT.offsetMin = new Vector2(15, 5);
            taRT.offsetMax = new Vector2(-15, -5);
            GetOrAdd<RectMask2D>(textArea);

            // Placeholder
            var placeholder = FindOrCreate(textArea.transform, "Placeholder");
            var phRT = GetOrAdd<RectTransform>(placeholder);
            phRT.anchorMin = Vector2.zero;
            phRT.anchorMax = Vector2.one;
            phRT.offsetMin = Vector2.zero;
            phRT.offsetMax = Vector2.zero;
            var phTMP = GetOrAdd<TextMeshProUGUI>(placeholder);
            phTMP.text = "Escribe tu nombre...";
            phTMP.fontSize = 22;
            phTMP.color = new Color(0.4f, 0.4f, 0.45f, 1f);
            phTMP.fontStyle = FontStyles.Italic;
            phTMP.alignment = TextAlignmentOptions.Left;

            // Text
            var text = FindOrCreate(textArea.transform, "Text");
            var txtRT = GetOrAdd<RectTransform>(text);
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.offsetMin = Vector2.zero;
            txtRT.offsetMax = Vector2.zero;
            var txtTMP = GetOrAdd<TextMeshProUGUI>(text);
            txtTMP.fontSize = 22;
            txtTMP.color = TEXT_WHITE;
            txtTMP.alignment = TextAlignmentOptions.Left;

            // Wire TMP_InputField
            input.textViewport = taRT;
            input.textComponent = txtTMP;
            input.placeholder = phTMP;

            // --- Confirm Button ---
            var confirmBtn = FindOrCreate(container.transform, "ConfirmNameButton");
            GetOrAdd<LayoutElement>(confirmBtn).preferredHeight = 55;
            var cbBg = GetOrAdd<Image>(confirmBtn);
            cbBg.color = CYAN_NEON;
            GetOrAdd<Button>(confirmBtn).targetGraphic = cbBg;
            var cbOutline = GetOrAdd<Outline>(confirmBtn);
            cbOutline.effectColor = CYAN_DARK;
            cbOutline.effectDistance = new Vector2(1, 1);

            var confirmText = FindOrCreate(confirmBtn.transform, "Text");
            var ctRT = GetOrAdd<RectTransform>(confirmText);
            ctRT.anchorMin = Vector2.zero;
            ctRT.anchorMax = Vector2.one;
            ctRT.offsetMin = Vector2.zero;
            ctRT.offsetMax = Vector2.zero;
            var ctTMP = GetOrAdd<TextMeshProUGUI>(confirmText);
            ctTMP.text = "CONFIRMAR";
            ctTMP.fontSize = 22;
            ctTMP.color = TEXT_DARK;
            ctTMP.fontStyle = FontStyles.Bold;
            ctTMP.alignment = TextAlignmentOptions.Center;

            // --- Error Text ---
            var errorText = FindOrCreate(container.transform, "NameErrorText");
            GetOrAdd<LayoutElement>(errorText).preferredHeight = 30;
            var eTMP = GetOrAdd<TextMeshProUGUI>(errorText);
            eTMP.text = "";
            eTMP.fontSize = 16;
            eTMP.color = RED_ERROR;
            eTMP.alignment = TextAlignmentOptions.Center;
            errorText.SetActive(false);

            Debug.Log("[OnboardingUI] NameInputPanel creado");
        }

        #endregion

        #region Avatar Selection Panel

        private static void CreateAvatarSelectionPanel()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var panel = FindOrCreate(canvas.transform, "AvatarSelectionPanel");
            var pRT = GetOrAdd<RectTransform>(panel);
            pRT.anchorMin = new Vector2(0.05f, 0.14f);
            pRT.anchorMax = new Vector2(0.95f, 0.55f);
            pRT.offsetMin = Vector2.zero;
            pRT.offsetMax = Vector2.zero;
            panel.SetActive(false);

            // Avatar Grid Container
            var grid = FindOrCreate(panel.transform, "AvatarContainer");
            var gRT = GetOrAdd<RectTransform>(grid);
            gRT.anchorMin = Vector2.zero;
            gRT.anchorMax = Vector2.one;
            gRT.offsetMin = new Vector2(10, 10);
            gRT.offsetMax = new Vector2(-10, -10);

            var gridLayout = GetOrAdd<GridLayoutGroup>(grid);
            gridLayout.cellSize = new Vector2(150, 180);
            gridLayout.spacing = new Vector2(20, 15);
            gridLayout.padding = new RectOffset(10, 10, 5, 5);
            gridLayout.childAlignment = TextAnchor.MiddleCenter;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 3;

            Debug.Log("[OnboardingUI] AvatarSelectionPanel creado (grid 3 columnas)");
        }

        #endregion

        #region Completion Panel

        private static void CreateCompletionPanel()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var panel = FindOrCreate(canvas.transform, "CompletionPanel");
            var pRT = GetOrAdd<RectTransform>(panel);
            pRT.anchorMin = new Vector2(0.05f, 0.12f);
            pRT.anchorMax = new Vector2(0.95f, 0.94f);
            pRT.offsetMin = Vector2.zero;
            pRT.offsetMax = Vector2.zero;
            panel.SetActive(false);

            // VLG for stacked content
            var vlg = GetOrAdd<VerticalLayoutGroup>(panel);
            vlg.spacing = 15;
            vlg.padding = new RectOffset(20, 20, 30, 20);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // --- Completion Icon Placeholder ---
            var iconGO = FindOrCreate(panel.transform, "CompletionIcon");
            GetOrAdd<LayoutElement>(iconGO).preferredHeight = 200;
            var iconImg = GetOrAdd<Image>(iconGO);
            iconImg.color = Color.white;
            iconImg.preserveAspect = true;
            Sprite completeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ICONS_PATH + "CompleteIcon.png");
            if (completeSprite != null) iconImg.sprite = completeSprite;

            // --- Completion Title ---
            var compTitle = FindOrCreate(panel.transform, "CompletionTitle");
            GetOrAdd<LayoutElement>(compTitle).preferredHeight = 50;
            var ctTMP = GetOrAdd<TextMeshProUGUI>(compTitle);
            ctTMP.text = "\u00A1Bien hecho!";
            ctTMP.fontSize = 32;
            ctTMP.color = CYAN_NEON;
            ctTMP.fontStyle = FontStyles.Bold;
            ctTMP.alignment = TextAlignmentOptions.Center;

            // --- Completion Message ---
            var compMsg = FindOrCreate(panel.transform, "CompletionMessage");
            GetOrAdd<LayoutElement>(compMsg).preferredHeight = 60;
            var cmTMP = GetOrAdd<TextMeshProUGUI>(compMsg);
            cmTMP.text = "Has completado el tutorial.\nAqu\u00ED tienes tus recompensas de bienvenida.";
            cmTMP.fontSize = 18;
            cmTMP.color = TEXT_WHITE;
            cmTMP.alignment = TextAlignmentOptions.Center;
            cmTMP.enableWordWrapping = true;

            // --- Rewards Display Card ---
            var rewardsCard = FindOrCreate(panel.transform, "RewardsCard");
            GetOrAdd<LayoutElement>(rewardsCard).preferredHeight = 90;
            var rcBg = GetOrAdd<Image>(rewardsCard);
            rcBg.color = CARD_BG;
            var rcOutline = GetOrAdd<Outline>(rewardsCard);
            rcOutline.effectColor = GOLD;
            rcOutline.effectDistance = new Vector2(2, 2);

            var rewardText = FindOrCreate(rewardsCard.transform, "RewardText");
            var rwRT = GetOrAdd<RectTransform>(rewardText);
            rwRT.anchorMin = Vector2.zero;
            rwRT.anchorMax = Vector2.one;
            rwRT.offsetMin = new Vector2(10, 10);
            rwRT.offsetMax = new Vector2(-10, -10);
            var rwTMP = GetOrAdd<TextMeshProUGUI>(rewardText);
            rwTMP.text = "+500 Monedas  |  +50 Gemas";
            rwTMP.fontSize = 26;
            rwTMP.color = GOLD;
            rwTMP.fontStyle = FontStyles.Bold;
            rwTMP.alignment = TextAlignmentOptions.Center;

            // --- Spacer ---
            var spacer = FindOrCreate(panel.transform, "Spacer");
            GetOrAdd<LayoutElement>(spacer).preferredHeight = 30;

            // --- Start Playing Button ---
            var startBtn = FindOrCreate(panel.transform, "StartPlayingButton");
            GetOrAdd<LayoutElement>(startBtn).preferredHeight = 65;
            var sbBg = GetOrAdd<Image>(startBtn);
            sbBg.color = GREEN_SUCCESS;
            GetOrAdd<Button>(startBtn).targetGraphic = sbBg;
            var sbOutline = GetOrAdd<Outline>(startBtn);
            sbOutline.effectColor = new Color(0.1f, 0.5f, 0.2f, 1f);
            sbOutline.effectDistance = new Vector2(1.5f, 1.5f);

            var startText = FindOrCreate(startBtn.transform, "Text");
            var spRT = GetOrAdd<RectTransform>(startText);
            spRT.anchorMin = Vector2.zero;
            spRT.anchorMax = Vector2.one;
            spRT.offsetMin = Vector2.zero;
            spRT.offsetMax = Vector2.zero;
            var spTMP = GetOrAdd<TextMeshProUGUI>(startText);
            spTMP.text = "\u00A1COMENZAR A JUGAR!";
            spTMP.fontSize = 24;
            spTMP.color = TEXT_DARK;
            spTMP.fontStyle = FontStyles.Bold;
            spTMP.alignment = TextAlignmentOptions.Center;

            Debug.Log("[OnboardingUI] CompletionPanel creado (icon + title + msg + rewards + button)");
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
            ptTMP.text = "ATR\u00C1S";
            ptTMP.fontSize = 20;
            ptTMP.color = TEXT_SECONDARY;
            ptTMP.fontStyle = FontStyles.Bold;
            ptTMP.alignment = TextAlignmentOptions.Center;

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
            ntTMP.text = "SIGUIENTE";
            ntTMP.fontSize = 20;
            ntTMP.color = TEXT_DARK;
            ntTMP.fontStyle = FontStyles.Bold;
            ntTMP.alignment = TextAlignmentOptions.Center;

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

            // UI - Step Display
            SetRef(so, "stepImage", FindInPath<Image>(r, "StepImage"));
            SetRef(so, "titleText", FindInPath<TextMeshProUGUI>(r, "TitleText"));
            SetRef(so, "descriptionText", FindInPath<TextMeshProUGUI>(r, "DescriptionText"));

            // UI - Navigation
            SetRef(so, "nextButton", FindInPath<Button>(r, "NavigationPanel/NextButton"));
            SetRef(so, "prevButton", FindInPath<Button>(r, "NavigationPanel/PrevButton"));
            SetRef(so, "nextButtonText", FindInPath<TextMeshProUGUI>(r, "NavigationPanel/NextButton/Text"));
            Transform dotsT = r.Find("DotsContainer");
            if (dotsT != null) SetRef(so, "dotsContainer", dotsT);

            // UI - Progress
            SetRef(so, "progressBar", FindInPath<Slider>(r, "ProgressBar"));
            SetRef(so, "stepCounterText", FindInPath<TextMeshProUGUI>(r, "TopBar/StepCounter"));

            // UI - Name Input
            Transform namePanel = r.Find("NameInputPanel");
            if (namePanel != null) SetRef(so, "nameInputPanel", namePanel.gameObject);
            SetRef(so, "nameInput", FindInPath<TMP_InputField>(r, "NameInputPanel/InputContainer/NameInput"));
            SetRef(so, "confirmNameButton", FindInPath<Button>(r, "NameInputPanel/InputContainer/ConfirmNameButton"));
            SetRef(so, "nameErrorText", FindInPath<TextMeshProUGUI>(r, "NameInputPanel/InputContainer/NameErrorText"));

            // UI - Avatar Selection
            Transform avatarPanel = r.Find("AvatarSelectionPanel");
            if (avatarPanel != null) SetRef(so, "avatarSelectionPanel", avatarPanel.gameObject);
            Transform avatarGrid = r.Find("AvatarSelectionPanel/AvatarContainer");
            if (avatarGrid != null) SetRef(so, "avatarContainer", avatarGrid);

            // UI - Tutorial Completion
            Transform compPanel = r.Find("CompletionPanel");
            if (compPanel != null) SetRef(so, "completionPanel", compPanel.gameObject);
            SetRef(so, "completionTitleText", FindInPath<TextMeshProUGUI>(r, "CompletionPanel/CompletionTitle"));
            SetRef(so, "completionMessageText", FindInPath<TextMeshProUGUI>(r, "CompletionPanel/CompletionMessage"));
            SetRef(so, "rewardText", FindInPath<TextMeshProUGUI>(r, "CompletionPanel/RewardsCard/RewardText"));
            SetRef(so, "startPlayingButton", FindInPath<Button>(r, "CompletionPanel/StartPlayingButton"));

            // UI - Sections (for animations)
            Transform progressBarT = r.Find("ProgressBar");
            if (progressBarT != null) SetRef(so, "progressBarTransform", progressBarT.GetComponent<RectTransform>());
            Transform topBarT = r.Find("TopBar");
            if (topBarT != null) SetRef(so, "topBarTransform", topBarT.GetComponent<RectTransform>());
            if (dotsT != null) SetRef(so, "dotsTransform", dotsT.GetComponent<RectTransform>());
            Transform navPanelT = r.Find("NavigationPanel");
            if (navPanelT != null) SetRef(so, "navigationTransform", navPanelT.GetComponent<RectTransform>());

            // Step Images
            SetSpriteRef(so, "welcomeImage", ICONS_PATH + "WelcomeIcon.png");
            SetSpriteRef(so, "gamesImage", ICONS_PATH + "GamesIcon.png");
            SetSpriteRef(so, "cashBattleImage", ICONS_PATH + "CashBattleIcon.png");
            SetSpriteRef(so, "tournamentsImage", ICONS_PATH + "TournamentsIcon.png");
            SetSpriteRef(so, "rewardsImage", ICONS_PATH + "RewardsIcon.png");

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            Debug.Log("[OnboardingUI] Referencias del manager asignadas");
        }

        #endregion

        #region Helpers

        private static void CleanupOldUI()
        {
            string[] toClean = { "Background", "SafeArea" };
            foreach (var canvas in Object.FindObjectsOfType<Canvas>(true))
            {
                if (canvas.transform.parent != null) continue;
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

        private static void SetSpriteRef(SerializedObject so, string propName, string assetPath)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite != null)
            {
                var prop = so.FindProperty(propName);
                if (prop != null) prop.objectReferenceValue = sprite;
                else Debug.LogWarning($"[OnboardingUI] Property '{propName}' no encontrada");
            }
            else
            {
                Debug.LogWarning($"[OnboardingUI] Sprite no encontrado: {assetPath}");
            }
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
