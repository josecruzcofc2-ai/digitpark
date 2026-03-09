using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// Cash Battle Onboarding UI Builder - Rediseño completo GOLD PREMIUM
    /// Layout: ProgressBar → TopBar → Slides(Icon+Title+ContentCard) → Dots → Navigation → Legal
    /// 5 slides: Bienvenida, Verificación 18+, Depósito, Juega y Apuesta, Gana y Retira
    /// Portrait 9:16 (1080x1920), matchWidthOrHeight=0
    ///
    /// Menu: DigitPark/UI Builders/Onboarding/Cash Battle Onboarding
    /// </summary>
    public class CashBattleOnboardingUIBuilder : EditorWindow
    {
        #region Colors - Gold Premium Theme

        private static readonly Color GOLD = new Color(1f, 0.843f, 0f, 1f);
        private static readonly Color GOLD_DARK = new Color(0.55f, 0.45f, 0f, 1f);
        private static readonly Color GOLD_GLOW = new Color(1f, 0.843f, 0f, 0.06f);
        private static readonly Color ORANGE_GOLD = new Color(1f, 0.647f, 0f, 1f);

        private static readonly Color DARK_BG = new Color(0.06f, 0.05f, 0.10f, 1f);
        private static readonly Color CARD_BG = new Color(0.10f, 0.07f, 0.03f, 0.95f);
        private static readonly Color CARD_BG_LIGHT = new Color(0.14f, 0.10f, 0.05f, 1f);

        private static readonly Color TEXT_WHITE = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.55f, 0.45f, 1f);
        private static readonly Color TEXT_DARK = new Color(0.05f, 0.04f, 0.02f, 1f);

        private static readonly Color GREEN_WIN = new Color(0f, 1f, 0.5f, 1f);

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
        private const float CONTENT_BOT = 0.12f;

        private const float DOTS_TOP = 0.095f;
        private const float DOTS_BOT = 0.065f;

        private const float NAV_TOP = 0.075f;
        private const float NAV_BOT = 0.025f;

        private const float SIDE_PAD = 30f;

        #endregion

        #region Icon Paths

        private const string ICONS_BASE = "Assets/_Project/Art/Icons/CashBattle/";

        private static readonly string[] SLIDE_ICONS =
        {
            ICONS_BASE + "UI/CashBattleIcon.png",          // Slide 1: Welcome (reusa logo)
            ICONS_BASE + "UI/VerificationIcon.png",            // Slide 2: Verificación
            ICONS_BASE + "Wallet/DepositIcon.png",             // Slide 3: Depósito
            ICONS_BASE + "Tournaments/TrophyPrizeIcon.png",    // Slide 4: Juega
            ICONS_BASE + "UI/icon_cash.png"                    // Slide 5: Gana
        };

        #endregion

        [MenuItem("DigitPark/Scenes/Build Scene/Onboarding/CashBattle", false, 171)]
        public static void ShowWindow()
        {
            GetWindow<CashBattleOnboardingUIBuilder>("CashBattle Onboarding Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Cash Battle Onboarding Builder", EditorStyles.boldLabel);
            GUILayout.Label("Onboarding GOLD Premium - Dinero Real - REDISEÑO", EditorStyles.miniLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Nuevo diseño (de arriba a abajo):\n\n" +
                "1. Progress Bar (línea delgada gold)\n" +
                "2. Top Bar (CASH BATTLE + contador + saltar)\n" +
                "3. Slide Icon (grande centrado por slide)\n" +
                "4. Slide Title (título gold por slide)\n" +
                "5. Content Card (descripción + bullets por slide)\n" +
                "6. Navigation dots (gold)\n" +
                "7. Back / Next buttons\n" +
                "8. Legal text\n\n" +
                "5 slides: Bienvenida, Verificación 18+,\n" +
                "Depósito, Juega y Apuesta, Gana y Retira",
                MessageType.Info);

            GUILayout.Space(15);

            GUI.backgroundColor = GOLD;
            if (GUILayout.Button("RECONSTRUIR CASH BATTLE ONBOARDING", GUILayout.Height(50)))
                RebuildCashBattleOnboarding();
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);
            GUILayout.Label("Secciones individuales:", EditorStyles.boldLabel);

            if (GUILayout.Button("1. Background + Top Bar", GUILayout.Height(25)))
            {
                Canvas c = UIBuilderCanvasHelper.FindMainCanvas();
                if (c != null) { CreateBackground(c.transform); CreateTopBar(); }
            }
            if (GUILayout.Button("2. Slides Container (5 slides)", GUILayout.Height(25)))
                CreateSlidesContainer();
            if (GUILayout.Button("3. Navigation (Dots + Buttons)", GUILayout.Height(25)))
                CreateNavigation();
            if (GUILayout.Button("4. Legal Text", GUILayout.Height(25)))
                CreateLegalText();

            GUILayout.Space(15);

            GUI.backgroundColor = ORANGE_GOLD;
            if (GUILayout.Button("ASIGNAR REFERENCIAS AL MANAGER", GUILayout.Height(35)))
                SetupManagerReferences();
            GUI.backgroundColor = Color.white;
        }

        #region Main Rebuild

        private static void RebuildCashBattleOnboarding()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("[CashBattleOnboardingUI] No se encontro Canvas");
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
                scaler.matchWidthOrHeight = 0f;
            }

            CreateBackground(canvas.transform);
            CreateTopBar();
            CreateSlidesContainer();
            CreateNavigation();
            CreateLegalText();
            SetupManagerReferences();

            Debug.Log("[CashBattleOnboardingUI] Onboarding RECONSTRUIDO exitosamente!");
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

        #region Top Bar (Progress + Title + Skip)

        private static void CreateTopBar()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            // --- Progress Bar (thin gold line at very top) ---
            var progressGO = FindOrCreate(canvas.transform, "ProgressBar");
            var pRT = GetOrAdd<RectTransform>(progressGO);
            SetAnchors(pRT, 0, PROGRESS_BOT, 1, PROGRESS_TOP);

            var slider = GetOrAdd<Slider>(progressGO);
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0;
            slider.maxValue = 4;
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
            GetOrAdd<Image>(sliderBg).color = new Color(0.15f, 0.12f, 0.05f, 1f);

            // Fill Area
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
            GetOrAdd<Image>(fill).color = GOLD;

            slider.fillRect = fRT;
            slider.handleRect = null;
            slider.targetGraphic = GetOrAdd<Image>(progressGO);
            GetOrAdd<Image>(progressGO).color = Color.clear;

            // --- Top Bar (CASH BATTLE + progress text + skip) ---
            var topBar = FindOrCreate(canvas.transform, "TopBar");
            var tbRT = GetOrAdd<RectTransform>(topBar);
            SetAnchors(tbRT, 0, TOPBAR_BOT, 1, TOPBAR_TOP);

            // CASH BATTLE title (left)
            var titleLabel = FindOrCreate(topBar.transform, "TitleLabel");
            var tlRT = GetOrAdd<RectTransform>(titleLabel);
            tlRT.anchorMin = new Vector2(0, 0);
            tlRT.anchorMax = new Vector2(0.45f, 1);
            tlRT.offsetMin = new Vector2(SIDE_PAD, 0);
            tlRT.offsetMax = Vector2.zero;
            var tlTMP = GetOrAdd<TextMeshProUGUI>(titleLabel);
            tlTMP.text = "CASH BATTLE";
            tlTMP.fontSize = FontSizes.Body;
            tlTMP.color = GOLD;
            tlTMP.fontStyle = FontStyles.Bold;
            tlTMP.alignment = TextAlignmentOptions.Left;
            tlTMP.enableAutoSizing = true;
            tlTMP.fontSizeMin = FontSizes.AutoMinSmall;
            tlTMP.fontSizeMax = FontSizes.Body;

            // Progress Text (center)
            var progressText = FindOrCreate(topBar.transform, "ProgressText");
            var ptRT = GetOrAdd<RectTransform>(progressText);
            ptRT.anchorMin = new Vector2(0.35f, 0);
            ptRT.anchorMax = new Vector2(0.65f, 1);
            ptRT.offsetMin = Vector2.zero;
            ptRT.offsetMax = Vector2.zero;
            var ptTMP = GetOrAdd<TextMeshProUGUI>(progressText);
            ptTMP.text = "1 / 5";
            ptTMP.fontSize = FontSizes.Body;
            ptTMP.color = TEXT_SECONDARY;
            ptTMP.fontStyle = FontStyles.Bold;
            ptTMP.alignment = TextAlignmentOptions.Center;

            // Skip Button (right)
            var skipBtn = FindOrCreate(topBar.transform, "SkipButton");
            var sRT = GetOrAdd<RectTransform>(skipBtn);
            sRT.anchorMin = new Vector2(0.7f, 0.1f);
            sRT.anchorMax = new Vector2(1, 0.9f);
            sRT.offsetMin = Vector2.zero;
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
            stTMP.color = new Color(GOLD.r, GOLD.g, GOLD.b, 0.7f);
            stTMP.fontStyle = FontStyles.Bold;
            stTMP.alignment = TextAlignmentOptions.Center;
            stTMP.enableAutoSizing = true;
            stTMP.fontSizeMin = FontSizes.AutoMinSmall;
            stTMP.fontSizeMax = FontSizes.Body;

            Debug.Log("[CashBattleOnboardingUI] TopBar creado (ProgressBar + Title + ProgressText + Skip)");
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

            // Limpiar slides anteriores del container
            for (int i = container.transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(container.transform.GetChild(i).gameObject);

            CreateSlide1_Welcome(container.transform);
            CreateSlide2_Verification(container.transform);
            CreateSlide3_Deposit(container.transform);
            CreateSlide4_Play(container.transform);
            CreateSlide5_Win(container.transform);

            Debug.Log("[CashBattleOnboardingUI] SlidesContainer creado con 5 slides");
        }

        private static void CreateSlide1_Welcome(Transform parent)
        {
            var slide = CreateSlideBase(parent, "Slide1", 0, true);

            CreateSlideTitle(slide, "WELCOME TO\nCASH BATTLE!", GOLD);

            var card = CreateContentCard(slide);
            AddContentText(card, "The real money competition\nplatform in Digit Park", (int)FontSizes.Body, TEXT_WHITE, TextAlignmentOptions.Center);
            AddContentSpacer(card, 10);
            AddContentText(card, "WIN REAL MONEY PLAYING", (int)FontSizes.Subtitle, GREEN_WIN, TextAlignmentOptions.Center, true);
            AddContentSpacer(card, 14);
            AddContentText(card, "\u2022  1v1 competitions from $1 USD", (int)FontSizes.Body, TEXT_WHITE);
            AddContentText(card, "\u2022  Tournaments with guaranteed prizes", (int)FontSizes.Body, TEXT_WHITE);
            AddContentText(card, "\u2022  Fast and secure withdrawals", (int)FontSizes.Body, TEXT_WHITE);
        }

        private static void CreateSlide2_Verification(Transform parent)
        {
            var slide = CreateSlideBase(parent, "Slide2", 1, false);

            CreateSlideTitle(slide, "VERIFY YOUR AGE\n(18+ REQUIRED)", GOLD);

            var card = CreateContentCard(slide);
            AddContentText(card, "To play with real money, you must:", (int)FontSizes.Body, TEXT_WHITE, TextAlignmentOptions.Center);
            AddContentSpacer(card, 10);
            AddContentText(card, "\u2713  Be 18 years or older", (int)FontSizes.Body, TEXT_WHITE);
            AddContentText(card, "\u2713  Verify your identity with Triumph\u2122", (int)FontSizes.Body, TEXT_WHITE);
            AddContentText(card, "\u2713  Confirm your banking information", (int)FontSizes.Body, TEXT_WHITE);
            AddContentSpacer(card, 12);
            AddContentText(card, "100% secure and confidential process", (int)FontSizes.Body, TEXT_SECONDARY, TextAlignmentOptions.Center);
        }

        private static void CreateSlide3_Deposit(Transform parent)
        {
            var slide = CreateSlideBase(parent, "Slide3", 2, false);

            CreateSlideTitle(slide, "DEPOSIT FUNDS\nTO YOUR WALLET", GOLD);

            var card = CreateContentCard(slide);
            AddContentText(card, "Add money to your account easily:", (int)FontSizes.Body, TEXT_WHITE, TextAlignmentOptions.Center);
            AddContentSpacer(card, 10);
            AddContentText(card, "\u2022  Credit/debit card", (int)FontSizes.Body, TEXT_WHITE);
            AddContentText(card, "\u2022  Bank transfer", (int)FontSizes.Body, TEXT_WHITE);
            AddContentText(card, "\u2022  Local payment methods", (int)FontSizes.Body, TEXT_WHITE);
            AddContentSpacer(card, 12);
            AddContentText(card, "Minimum deposit: $5 USD", (int)FontSizes.Subtitle, GOLD, TextAlignmentOptions.Center, true);
        }

        private static void CreateSlide4_Play(Transform parent)
        {
            var slide = CreateSlideBase(parent, "Slide4", 3, false);

            CreateSlideTitle(slide, "CHOOSE YOUR GAME\nAND BET", GOLD);

            var card = CreateContentCard(slide);
            AddContentText(card, "1v1 COMPETITIONS", (int)FontSizes.Subtitle, ORANGE_GOLD, TextAlignmentOptions.Center, true);
            AddContentText(card, "\u2022  Skill-based matchmaking (MMR)", (int)FontSizes.Body, TEXT_WHITE);
            AddContentText(card, "\u2022  Bets from $1 to $250 USD", (int)FontSizes.Body, TEXT_WHITE);
            AddContentText(card, "\u2022  Winner takes 80%", (int)FontSizes.Body, TEXT_WHITE);
            AddContentSpacer(card, 10);
            AddContentText(card, "TOURNAMENTS", (int)FontSizes.Subtitle, ORANGE_GOLD, TextAlignmentOptions.Center, true);
            AddContentText(card, "\u2022  Up to 256 players", (int)FontSizes.Body, TEXT_WHITE);
            AddContentText(card, "\u2022  Guaranteed prizes", (int)FontSizes.Body, TEXT_WHITE);
            AddContentText(card, "\u2022  Professional bracket system", (int)FontSizes.Body, TEXT_WHITE);
        }

        private static void CreateSlide5_Win(Transform parent)
        {
            var slide = CreateSlideBase(parent, "Slide5", 4, false);

            CreateSlideTitle(slide, "WIN AND WITHDRAW\nYOUR MONEY!", GREEN_WIN);

            var card = CreateContentCard(slide);
            AddContentText(card, "FAST AND SECURE WITHDRAWALS", (int)FontSizes.Subtitle, GREEN_WIN, TextAlignmentOptions.Center, true);
            AddContentSpacer(card, 10);
            AddContentText(card, "\u2713  Minimum withdrawal: $10 USD", (int)FontSizes.Body, TEXT_WHITE);
            AddContentText(card, "\u2713  Maximum: $500 USD per withdrawal", (int)FontSizes.Body, TEXT_WHITE);
            AddContentText(card, "\u2713  Processed in 1-3 business days", (int)FontSizes.Body, TEXT_WHITE);
            AddContentText(card, "\u2713  Direct to your bank account", (int)FontSizes.Body, TEXT_WHITE);
            AddContentSpacer(card, 14);
            AddContentText(card, "START WINNING TODAY!", (int)FontSizes.Subtitle, GOLD, TextAlignmentOptions.Center, true);
        }

        #endregion

        #region Slide Building Helpers

        private static Transform CreateSlideBase(Transform parent, string name, int iconIndex, bool active)
        {
            var slide = FindOrCreate(parent, name);
            var sRT = GetOrAdd<RectTransform>(slide);
            sRT.anchorMin = Vector2.zero;
            sRT.anchorMax = Vector2.one;
            sRT.offsetMin = Vector2.zero;
            sRT.offsetMax = Vector2.zero;
            slide.SetActive(active);

            // Clear old children for clean rebuild
            while (slide.transform.childCount > 0)
                DestroyImmediate(slide.transform.GetChild(0).gameObject);

            // Icon Glow (subtle gold glow behind icon)
            var glow = new GameObject("IconGlow");
            glow.transform.SetParent(slide.transform, false);
            var glRT = glow.AddComponent<RectTransform>();
            glRT.anchorMin = new Vector2(0.10f, ICON_BOT - 0.03f);
            glRT.anchorMax = new Vector2(0.90f, ICON_TOP + 0.01f);
            glRT.offsetMin = Vector2.zero;
            glRT.offsetMax = Vector2.zero;
            glow.AddComponent<Image>().color = GOLD_GLOW;

            // Slide Icon (large centered)
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

            // Try load icon
            if (iconIndex >= 0 && iconIndex < SLIDE_ICONS.Length)
            {
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SLIDE_ICONS[iconIndex]);
                if (sprite != null) iImg.sprite = sprite;
            }

            return slide.transform;
        }

        private static void CreateSlideTitle(Transform slide, string text, Color color)
        {
            var title = new GameObject("SlideTitle");
            title.transform.SetParent(slide, false);
            var tRT = title.AddComponent<RectTransform>();
            tRT.anchorMin = new Vector2(0.05f, TITLE_BOT);
            tRT.anchorMax = new Vector2(0.95f, TITLE_TOP);
            tRT.offsetMin = Vector2.zero;
            tRT.offsetMax = Vector2.zero;
            var tTMP = title.AddComponent<TextMeshProUGUI>();
            tTMP.text = text;
            tTMP.fontSize = FontSizes.H4;
            tTMP.color = color;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.Center;
            tTMP.enableWordWrapping = true;
            tTMP.enableAutoSizing = true;
            tTMP.fontSizeMin = FontSizes.AutoMinTitle;
            tTMP.fontSizeMax = FontSizes.H4;
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

            card.AddComponent<Image>().color = CARD_BG;
            var outline = card.AddComponent<Outline>();
            outline.effectColor = GOLD_DARK;
            outline.effectDistance = new Vector2(1.5f, 1.5f);

            // Content VLG inside card
            var content = new GameObject("Content");
            content.transform.SetParent(card.transform, false);
            var coRT = content.AddComponent<RectTransform>();
            coRT.anchorMin = Vector2.zero;
            coRT.anchorMax = Vector2.one;
            coRT.offsetMin = new Vector2(25, 15);
            coRT.offsetMax = new Vector2(-25, -15);

            var vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            return content.transform;
        }

        private static void AddContentText(Transform content, string text, int fontSize,
            Color color, TextAlignmentOptions align = TextAlignmentOptions.Left, bool bold = false)
        {
            var obj = new GameObject("Text");
            obj.transform.SetParent(content, false);
            obj.AddComponent<LayoutElement>().preferredHeight = fontSize + 16;
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = align;
            tmp.enableWordWrapping = true;
            tmp.fontStyle = FontStyles.Bold;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinBody;
            tmp.fontSizeMax = fontSize > 0 ? fontSize : FontSizes.Body;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
        }

        private static void AddContentSpacer(Transform content, float height)
        {
            var spacer = new GameObject("Spacer");
            spacer.transform.SetParent(content, false);
            spacer.AddComponent<LayoutElement>().preferredHeight = height;
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

            // --- Navigation Panel ---
            var navPanel = FindOrCreate(canvas.transform, "NavigationPanel");
            var npRT = GetOrAdd<RectTransform>(navPanel);
            npRT.anchorMin = new Vector2(0, NAV_BOT);
            npRT.anchorMax = new Vector2(1, NAV_TOP);
            npRT.offsetMin = new Vector2(SIDE_PAD, 0);
            npRT.offsetMax = new Vector2(-SIDE_PAD, 0);

            // Back Button (left)
            var backBtn = FindOrCreate(navPanel.transform, "BackButton");
            var bbRT = GetOrAdd<RectTransform>(backBtn);
            bbRT.anchorMin = new Vector2(0, 0);
            bbRT.anchorMax = new Vector2(0.47f, 1);
            bbRT.offsetMin = Vector2.zero;
            bbRT.offsetMax = Vector2.zero;
            var bbBg = GetOrAdd<Image>(backBtn);
            bbBg.color = CARD_BG_LIGHT;
            GetOrAdd<Button>(backBtn).targetGraphic = bbBg;
            var bbOutline = GetOrAdd<Outline>(backBtn);
            bbOutline.effectColor = new Color(0.3f, 0.25f, 0.15f, 0.5f);
            bbOutline.effectDistance = new Vector2(1, 1);

            var backText = FindOrCreate(backBtn.transform, "Text");
            var btRT = GetOrAdd<RectTransform>(backText);
            btRT.anchorMin = Vector2.zero;
            btRT.anchorMax = Vector2.one;
            btRT.offsetMin = Vector2.zero;
            btRT.offsetMax = Vector2.zero;
            var btTMP = GetOrAdd<TextMeshProUGUI>(backText);
            btTMP.text = "BACK";
            btTMP.fontSize = FontSizes.Body;
            btTMP.color = TEXT_SECONDARY;
            btTMP.fontStyle = FontStyles.Bold;
            btTMP.alignment = TextAlignmentOptions.Center;
            btTMP.enableAutoSizing = true;
            btTMP.fontSizeMin = FontSizes.AutoMinSmall;
            btTMP.fontSizeMax = FontSizes.Body;

            // Next Button (right, gold)
            var nextBtn = FindOrCreate(navPanel.transform, "NextButton");
            var nbRT = GetOrAdd<RectTransform>(nextBtn);
            nbRT.anchorMin = new Vector2(0.53f, 0);
            nbRT.anchorMax = new Vector2(1, 1);
            nbRT.offsetMin = Vector2.zero;
            nbRT.offsetMax = Vector2.zero;
            var nbBg = GetOrAdd<Image>(nextBtn);
            nbBg.color = GOLD;
            GetOrAdd<Button>(nextBtn).targetGraphic = nbBg;
            var nbOutline = GetOrAdd<Outline>(nextBtn);
            nbOutline.effectColor = GOLD_DARK;
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
            ntTMP.fontSizeMin = FontSizes.AutoMinSmall;
            ntTMP.fontSizeMax = FontSizes.Body;

            Debug.Log("[CashBattleOnboardingUI] Navigation creado (DotsContainer + Back + Next)");
        }

        #endregion

        #region Legal Text

        private static void CreateLegalText()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var legal = FindOrCreate(canvas.transform, "LegalText");
            var lRT = GetOrAdd<RectTransform>(legal);
            lRT.anchorMin = new Vector2(0.05f, 0);
            lRT.anchorMax = new Vector2(0.95f, 0.012f);
            lRT.offsetMin = Vector2.zero;
            lRT.offsetMax = Vector2.zero;

            var lTMP = GetOrAdd<TextMeshProUGUI>(legal);
            lTMP.text = "Powered by Triumph\u2122 \u2022 Responsible gaming \u2022 18+ only";
            lTMP.fontSize = FontSizes.Body;
            lTMP.color = TEXT_SECONDARY;
            lTMP.fontStyle = FontStyles.Bold;
            lTMP.alignment = TextAlignmentOptions.Center;
            lTMP.enableWordWrapping = true;
            lTMP.enableAutoSizing = true;
            lTMP.fontSizeMin = FontSizes.AutoMinSmall;
            lTMP.fontSizeMax = FontSizes.Body;

            Debug.Log("[CashBattleOnboardingUI] LegalText creado");
        }

        #endregion

        #region Manager References

        private static void SetupManagerReferences()
        {
            var manager = Object.FindFirstObjectByType<DigitPark.Managers.CashBattleOnboardingManager>();
            if (manager == null)
            {
                Debug.LogWarning("[CashBattleOnboardingUI] CashBattleOnboardingManager no encontrado. Agrega el componente primero.");
                return;
            }

            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var so = new SerializedObject(manager);
            Transform r = canvas.transform;

            // Slides Container
            Transform slidesT = r.Find("SlidesContainer");
            if (slidesT != null) SetRef(so, "slidesContainer", slidesT);

            // Navigation Buttons
            SetRef(so, "nextButton", FindInPath<Button>(r, "NavigationPanel/NextButton"));
            SetRef(so, "backButton", FindInPath<Button>(r, "NavigationPanel/BackButton"));
            SetRef(so, "skipButton", FindInPath<Button>(r, "TopBar/SkipButton"));
            SetRef(so, "nextButtonText", FindInPath<TextMeshProUGUI>(r, "NavigationPanel/NextButton/Text"));

            // Navigation Dots
            Transform dotsT = r.Find("DotsContainer");
            if (dotsT != null) SetRef(so, "dotsContainer", dotsT);

            // Progress
            SetRef(so, "progressBar", FindInPath<Slider>(r, "ProgressBar"));
            SetRef(so, "progressText", FindInPath<TextMeshProUGUI>(r, "TopBar/ProgressText"));

            // UI - Sections (for animations)
            Transform progressBarT = r.Find("ProgressBar");
            if (progressBarT != null) SetRef(so, "progressBarTransform", progressBarT.GetComponent<RectTransform>());
            Transform topBarT = r.Find("TopBar");
            if (topBarT != null) SetRef(so, "topBarTransform", topBarT.GetComponent<RectTransform>());
            if (dotsT != null) SetRef(so, "dotsRectTransform", dotsT.GetComponent<RectTransform>());
            Transform navPanelT = r.Find("NavigationPanel");
            if (navPanelT != null) SetRef(so, "navigationTransform", navPanelT.GetComponent<RectTransform>());

            // Configuration
            var totalProp = so.FindProperty("totalSlides");
            if (totalProp != null) totalProp.intValue = 5;
            var allowProp = so.FindProperty("allowSkip");
            if (allowProp != null) allowProp.boolValue = true;
            var durProp = so.FindProperty("transitionDuration");
            if (durProp != null) durProp.floatValue = 0.3f;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            Debug.Log("[CashBattleOnboardingUI] Referencias del manager asignadas (12 campos)");
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
            if (prop == null)
            {
                Debug.LogWarning($"[CashBattleOnboardingUI] Property '{propName}' no encontrada");
                return;
            }
            if (value != null) prop.objectReferenceValue = value;
            else Debug.LogWarning($"[CashBattleOnboardingUI] No se encontro valor para: {propName}");
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
