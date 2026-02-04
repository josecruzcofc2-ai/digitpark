using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Daily Missions UI Builder - Neon Cyan theme
    /// Layout: ProgressBar → TopBar → TimerBar → OverallProgress → ScrollView (Daily + Weekly)
    /// Portrait 9:16 (1080x1920), matchWidthOrHeight=0
    /// NO SafeArea, NO tab bar, NO dialog confirmation
    ///
    /// Menu: DigitPark/UI Builders/Monetization/Daily Missions
    /// </summary>
    public class DailyMissionsUIBuilder : EditorWindow
    {
        #region Colors

        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.5f, 1f);
        private static readonly Color CYAN_GLOW = new Color(0f, 1f, 1f, 0.06f);

        private static readonly Color DARK_BG = new Color(0.02f, 0.04f, 0.08f, 1f);
        private static readonly Color CARD_BG = new Color(0.06f, 0.08f, 0.12f, 1f);

        private static readonly Color TEXT_WHITE = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.6f, 0.65f, 1f);
        private static readonly Color TEXT_DARK = new Color(0.05f, 0.05f, 0.08f, 1f);

        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color ORANGE_TIMER = new Color(1f, 0.5f, 0.12f, 1f);
        private static readonly Color PURPLE_WEEKLY = new Color(0.6f, 0.2f, 1f, 1f);
        private static readonly Color GREEN_SUCCESS = new Color(0.2f, 0.9f, 0.4f, 1f);

        private static readonly Color COIN_COLOR = new Color(1f, 0.85f, 0.3f, 1f);
        private static readonly Color GEM_COLOR = new Color(0.4f, 0.8f, 1f, 1f);
        private static readonly Color XP_COLOR = new Color(0.5f, 1f, 0.5f, 1f);

        #endregion

        #region Layout Anchors (Y: 0=bottom, 1=top)

        private const float PROGRESS_TOP = 0.993f;
        private const float PROGRESS_BOT = 0.990f;

        private const float TOPBAR_TOP = 0.988f;
        private const float TOPBAR_BOT = 0.955f;

        private const float TIMER_TOP = 0.952f;
        private const float TIMER_BOT = 0.925f;

        private const float OVERALL_TOP = 0.920f;
        private const float OVERALL_BOT = 0.860f;

        private const float SCROLL_TOP = 0.855f;
        private const float SCROLL_BOT = 0.015f;

        private const float SIDE_PAD = 30f;

        #endregion

        #region Paths

        private const string MISSIONS_ICONS_PATH = "Assets/_Project/Art/Icons/Missions/";
        private const string UI_ICONS_PATH = "Assets/_Project/Art/Icons/UI/";
        private const string CURRENCY_ICONS_PATH = "Assets/_Project/Art/Icons/Currency/";

        #endregion

        [MenuItem("DigitPark/UI Builders/Monetization/Daily Missions", false, 184)]
        public static void ShowWindow()
        {
            GetWindow<DailyMissionsUIBuilder>("Daily Missions Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Daily Missions UI Builder", EditorStyles.boldLabel);
            GUILayout.Label("Misiones Diarias + Semanales - Neon Cyan", EditorStyles.miniLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Layout (de arriba a abajo):\n\n" +
                "1. Progress Bar (linea delgada cyan)\n" +
                "2. Top Bar (Back + MISIONES + Info)\n" +
                "3. Timer Bar (countdown reinicio)\n" +
                "4. Overall Progress (barra con milestones)\n" +
                "5. ScrollView (misiones diarias + semanales)\n" +
                "6. Reward Claim Popup (oculto)\n\n" +
                "6 misiones diarias + 3 semanales",
                MessageType.Info);

            GUILayout.Space(15);

            GUI.backgroundColor = CYAN_NEON;
            if (GUILayout.Button("RECONSTRUIR MISIONES COMPLETO", GUILayout.Height(50)))
                RebuildMissions();
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);

            GUI.backgroundColor = GOLD;
            if (GUILayout.Button("ASIGNAR REFERENCIAS AL MANAGER", GUILayout.Height(35)))
                SetupManagerReferences();
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);
            GUILayout.Label("Secciones individuales:", EditorStyles.boldLabel);

            if (GUILayout.Button("1. Background + Progress Bar", GUILayout.Height(25)))
            {
                Canvas c = Object.FindFirstObjectByType<Canvas>();
                if (c != null) { CreateBackground(c.transform); CreateProgressBar(); }
            }
            if (GUILayout.Button("2. Top Bar", GUILayout.Height(25))) CreateTopBar();
            if (GUILayout.Button("3. Timer Bar", GUILayout.Height(25))) CreateTimerBar();
            if (GUILayout.Button("4. Overall Progress", GUILayout.Height(25))) CreateOverallProgress();
            if (GUILayout.Button("5. Missions ScrollView", GUILayout.Height(25))) CreateMissionsScrollView();
            if (GUILayout.Button("6. Reward Claim Popup", GUILayout.Height(25))) CreateRewardClaimPopup();
        }

        #region Main Rebuild

        private static void RebuildMissions()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[DailyMissionsUI] No se encontro Canvas");
                return;
            }

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0f;
            }

            // Limpiar elementos anteriores
            string[] oldNames = {
                "Background", "SafeArea", "Header", "ResetTimer", "OverallProgress",
                "MissionsScrollView", "RewardClaimBlocker", "ProgressBar", "TopBar",
                "TimerBar", "ScrollView"
            };
            foreach (var n in oldNames)
            {
                Transform t = canvas.transform.Find(n);
                if (t != null) Object.DestroyImmediate(t.gameObject);
            }

            CreateBackground(canvas.transform);
            CreateProgressBar();
            CreateTopBar();
            CreateTimerBar();
            CreateOverallProgress();
            CreateMissionsScrollView();
            CreateRewardClaimPopup();
            SetupManagerReferences();

            Debug.Log("[DailyMissionsUI] Misiones RECONSTRUIDAS exitosamente!");
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

        #region Progress Bar

        private static void CreateProgressBar()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            var progressGO = FindOrCreate(canvas.transform, "ProgressBar");
            var pRT = GetOrAdd<RectTransform>(progressGO);
            SetAnchors(pRT, 0, PROGRESS_BOT, 1, PROGRESS_TOP);

            var slider = GetOrAdd<Slider>(progressGO);
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0;
            slider.maxValue = 6;
            slider.wholeNumbers = true;
            slider.value = 4;
            slider.interactable = false;

            // Slider Background
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

            Debug.Log("[DailyMissionsUI] ProgressBar creado");
        }

        #endregion

        #region Top Bar (Back + Title + Info)

        private static void CreateTopBar()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            var topBar = FindOrCreate(canvas.transform, "TopBar");
            var tbRT = GetOrAdd<RectTransform>(topBar);
            SetAnchors(tbRT, 0, TOPBAR_BOT, 1, TOPBAR_TOP);

            // Back Button (left)
            var backBtn = FindOrCreate(topBar.transform, "BackButton");
            var bRT = GetOrAdd<RectTransform>(backBtn);
            bRT.anchorMin = new Vector2(0, 0.5f);
            bRT.anchorMax = new Vector2(0, 0.5f);
            bRT.pivot = new Vector2(0, 0.5f);
            bRT.anchoredPosition = new Vector2(SIDE_PAD, 0);
            bRT.sizeDelta = new Vector2(50, 50);
            var bBg = GetOrAdd<Image>(backBtn);
            bBg.color = CARD_BG;
            GetOrAdd<Button>(backBtn).targetGraphic = bBg;
            var bOutline = GetOrAdd<Outline>(backBtn);
            bOutline.effectColor = CYAN_DARK;
            bOutline.effectDistance = new Vector2(1, 1);

            var backText = FindOrCreate(backBtn.transform, "Text");
            var btRT = GetOrAdd<RectTransform>(backText);
            btRT.anchorMin = Vector2.zero;
            btRT.anchorMax = Vector2.one;
            btRT.offsetMin = Vector2.zero;
            btRT.offsetMax = Vector2.zero;
            var btTMP = GetOrAdd<TextMeshProUGUI>(backText);
            btTMP.text = "<";
            btTMP.fontSize = 28;
            btTMP.fontStyle = FontStyles.Bold;
            btTMP.color = CYAN_NEON;
            btTMP.alignment = TextAlignmentOptions.Center;

            // Title (center)
            var title = FindOrCreate(topBar.transform, "TitleText");
            var tRT = GetOrAdd<RectTransform>(title);
            tRT.anchorMin = new Vector2(0.15f, 0);
            tRT.anchorMax = new Vector2(0.85f, 1);
            tRT.offsetMin = Vector2.zero;
            tRT.offsetMax = Vector2.zero;
            var tTMP = GetOrAdd<TextMeshProUGUI>(title);
            tTMP.text = "MISIONES";
            tTMP.fontSize = 28;
            tTMP.color = CYAN_NEON;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.Center;

            // Info Button (right)
            var infoBtn = FindOrCreate(topBar.transform, "InfoButton");
            var iRT = GetOrAdd<RectTransform>(infoBtn);
            iRT.anchorMin = new Vector2(1, 0.5f);
            iRT.anchorMax = new Vector2(1, 0.5f);
            iRT.pivot = new Vector2(1, 0.5f);
            iRT.anchoredPosition = new Vector2(-SIDE_PAD, 0);
            iRT.sizeDelta = new Vector2(40, 40);
            var iBg = GetOrAdd<Image>(infoBtn);
            iBg.color = CARD_BG;
            GetOrAdd<Button>(infoBtn).targetGraphic = iBg;
            var iOutline = GetOrAdd<Outline>(infoBtn);
            iOutline.effectColor = CYAN_DARK;
            iOutline.effectDistance = new Vector2(1, 1);

            var infoText = FindOrCreate(infoBtn.transform, "Text");
            var itRT = GetOrAdd<RectTransform>(infoText);
            itRT.anchorMin = Vector2.zero;
            itRT.anchorMax = Vector2.one;
            itRT.offsetMin = Vector2.zero;
            itRT.offsetMax = Vector2.zero;
            var itTMP = GetOrAdd<TextMeshProUGUI>(infoText);
            itTMP.text = "?";
            itTMP.fontSize = 22;
            itTMP.fontStyle = FontStyles.Bold;
            itTMP.color = CYAN_NEON;
            itTMP.alignment = TextAlignmentOptions.Center;

            Debug.Log("[DailyMissionsUI] TopBar creado (Back + MISIONES + Info)");
        }

        #endregion

        #region Timer Bar

        private static void CreateTimerBar()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            var timerBar = FindOrCreate(canvas.transform, "TimerBar");
            var tbRT = GetOrAdd<RectTransform>(timerBar);
            SetAnchors(tbRT, 0, TIMER_BOT, 1, TIMER_TOP);
            tbRT.offsetMin = new Vector2(SIDE_PAD, 0);
            tbRT.offsetMax = new Vector2(-SIDE_PAD, 0);

            var tbBg = GetOrAdd<Image>(timerBar);
            tbBg.color = CARD_BG;
            var tbOutline = GetOrAdd<Outline>(timerBar);
            tbOutline.effectColor = new Color(ORANGE_TIMER.r * 0.4f, ORANGE_TIMER.g * 0.4f, ORANGE_TIMER.b * 0.4f, 1f);
            tbOutline.effectDistance = new Vector2(1, 1);

            var hlg = GetOrAdd<HorizontalLayoutGroup>(timerBar);
            hlg.spacing = 10;
            hlg.padding = new RectOffset(15, 15, 0, 0);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // Timer Icon
            var timerIcon = FindOrCreate(timerBar.transform, "TimerIcon");
            var iconImg = GetOrAdd<Image>(timerIcon);
            iconImg.color = ORANGE_TIMER;
            Sprite timerSprite = LoadIcon(UI_ICONS_PATH + "icon_ui_timer.png");
            if (timerSprite != null) { iconImg.sprite = timerSprite; iconImg.preserveAspect = true; }
            var iconLE = GetOrAdd<LayoutElement>(timerIcon);
            iconLE.minWidth = 24;
            iconLE.minHeight = 24;
            iconLE.preferredWidth = 24;
            iconLE.preferredHeight = 24;

            // Label
            var label = FindOrCreate(timerBar.transform, "Label");
            var lTMP = GetOrAdd<TextMeshProUGUI>(label);
            lTMP.text = "Se reinician en:";
            lTMP.fontSize = 15;
            lTMP.color = TEXT_SECONDARY;
            lTMP.alignment = TextAlignmentOptions.MidlineLeft;
            var lLE = GetOrAdd<LayoutElement>(label);
            lLE.flexibleWidth = 1;

            // Countdown
            var countdown = FindOrCreate(timerBar.transform, "CountdownText");
            var cdTMP = GetOrAdd<TextMeshProUGUI>(countdown);
            cdTMP.text = "12:34:56";
            cdTMP.fontSize = 20;
            cdTMP.fontStyle = FontStyles.Bold;
            cdTMP.color = ORANGE_TIMER;
            cdTMP.alignment = TextAlignmentOptions.MidlineRight;
            var cdLE = GetOrAdd<LayoutElement>(countdown);
            cdLE.minWidth = 120;

            Debug.Log("[DailyMissionsUI] TimerBar creado");
        }

        #endregion

        #region Overall Progress

        private static void CreateOverallProgress()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            var panel = FindOrCreate(canvas.transform, "OverallProgress");
            var pRT = GetOrAdd<RectTransform>(panel);
            SetAnchors(pRT, 0, OVERALL_BOT, 1, OVERALL_TOP);
            pRT.offsetMin = new Vector2(SIDE_PAD, 0);
            pRT.offsetMax = new Vector2(-SIDE_PAD, 0);

            var pBg = GetOrAdd<Image>(panel);
            pBg.color = CARD_BG;
            var pOutline = GetOrAdd<Outline>(panel);
            pOutline.effectColor = CYAN_DARK;
            pOutline.effectDistance = new Vector2(1, 1);

            var vlg = GetOrAdd<VerticalLayoutGroup>(panel);
            vlg.spacing = 8;
            vlg.padding = new RectOffset(15, 15, 10, 10);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Row 1: Title row
            var titleRow = FindOrCreate(panel.transform, "TitleRow");
            var trHlg = GetOrAdd<HorizontalLayoutGroup>(titleRow);
            trHlg.spacing = 0;
            trHlg.childAlignment = TextAnchor.MiddleCenter;
            trHlg.childControlWidth = true;
            trHlg.childControlHeight = true;
            trHlg.childForceExpandWidth = true;
            var trLE = GetOrAdd<LayoutElement>(titleRow);
            trLE.preferredHeight = 22;

            var titleLeft = FindOrCreate(titleRow.transform, "TitleLeft");
            var tlTMP = GetOrAdd<TextMeshProUGUI>(titleLeft);
            tlTMP.text = "Progreso Diario";
            tlTMP.fontSize = 16;
            tlTMP.fontStyle = FontStyles.Bold;
            tlTMP.color = TEXT_WHITE;
            tlTMP.alignment = TextAlignmentOptions.MidlineLeft;

            var titleRight = FindOrCreate(titleRow.transform, "TitleRight");
            var trTMP = GetOrAdd<TextMeshProUGUI>(titleRight);
            trTMP.text = "4/6 Misiones";
            trTMP.fontSize = 14;
            trTMP.color = TEXT_SECONDARY;
            trTMP.alignment = TextAlignmentOptions.MidlineRight;

            // Row 2: Progress slider
            var progressContainer = FindOrCreate(panel.transform, "ProgressContainer");
            var pcLE = GetOrAdd<LayoutElement>(progressContainer);
            pcLE.preferredHeight = 28;

            var slider = GetOrAdd<Slider>(progressContainer);
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0;
            slider.maxValue = 6;
            slider.wholeNumbers = true;
            slider.value = 4;
            slider.interactable = false;

            // Slider Background
            var sliderBg = FindOrCreate(progressContainer.transform, "Background");
            var sbRT = GetOrAdd<RectTransform>(sliderBg);
            sbRT.anchorMin = Vector2.zero;
            sbRT.anchorMax = Vector2.one;
            sbRT.offsetMin = Vector2.zero;
            sbRT.offsetMax = Vector2.zero;
            GetOrAdd<Image>(sliderBg).color = new Color(0.1f, 0.12f, 0.15f, 1f);

            // Fill Area
            var fillArea = FindOrCreate(progressContainer.transform, "Fill Area");
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
            slider.targetGraphic = GetOrAdd<Image>(progressContainer);
            GetOrAdd<Image>(progressContainer).color = Color.clear;

            // Reward Markers overlaid on progress bar
            CreateRewardMarker(progressContainer, "Marker3", 0.5f, "50", false);
            CreateRewardMarker(progressContainer, "Marker5", 0.833f, "100", false);
            CreateRewardMarker(progressContainer, "MarkerAll", 1.0f, "200", true);

            Debug.Log("[DailyMissionsUI] OverallProgress creado");
        }

        private static void CreateRewardMarker(GameObject parent, string name, float xPos, string reward, bool isBonus)
        {
            var marker = FindOrCreate(parent.transform, name);
            var mRT = GetOrAdd<RectTransform>(marker);
            mRT.anchorMin = new Vector2(xPos, 0.5f);
            mRT.anchorMax = new Vector2(xPos, 0.5f);
            mRT.pivot = new Vector2(0.5f, 0.5f);
            mRT.sizeDelta = new Vector2(28, 28);
            mRT.anchoredPosition = Vector2.zero;

            var mImg = GetOrAdd<Image>(marker);
            mImg.color = isBonus ? GOLD : CYAN_DARK;
            var mOutline = GetOrAdd<Outline>(marker);
            mOutline.effectColor = isBonus ? GOLD : CYAN_NEON;
            mOutline.effectDistance = new Vector2(1, 1);

            var rewardText = FindOrCreate(marker.transform, "RewardText");
            var rwRT = GetOrAdd<RectTransform>(rewardText);
            rwRT.anchorMin = Vector2.zero;
            rwRT.anchorMax = Vector2.one;
            rwRT.offsetMin = Vector2.zero;
            rwRT.offsetMax = Vector2.zero;
            var rwTMP = GetOrAdd<TextMeshProUGUI>(rewardText);
            rwTMP.text = reward;
            rwTMP.fontSize = 10;
            rwTMP.fontStyle = FontStyles.Bold;
            rwTMP.color = TEXT_WHITE;
            rwTMP.alignment = TextAlignmentOptions.Center;
        }

        #endregion

        #region Missions ScrollView

        private static void CreateMissionsScrollView()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            var scrollView = FindOrCreate(canvas.transform, "ScrollView");
            var svRT = GetOrAdd<RectTransform>(scrollView);
            SetAnchors(svRT, 0, SCROLL_BOT, 1, SCROLL_TOP);

            var scrollRect = GetOrAdd<ScrollRect>(scrollView);
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;

            GetOrAdd<Image>(scrollView).color = Color.clear;

            // Viewport
            var viewport = FindOrCreate(scrollView.transform, "Viewport");
            var vpRT = GetOrAdd<RectTransform>(viewport);
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.offsetMin = Vector2.zero;
            vpRT.offsetMax = Vector2.zero;
            GetOrAdd<RectMask2D>(viewport);
            scrollRect.viewport = vpRT;

            // Content
            var content = FindOrCreate(viewport.transform, "Content");
            var cRT = GetOrAdd<RectTransform>(content);
            cRT.anchorMin = new Vector2(0, 1);
            cRT.anchorMax = new Vector2(1, 1);
            cRT.pivot = new Vector2(0.5f, 1);
            cRT.sizeDelta = Vector2.zero;
            scrollRect.content = cRT;

            var csf = GetOrAdd<ContentSizeFitter>(content);
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var vlg = GetOrAdd<VerticalLayoutGroup>(content);
            vlg.spacing = 12;
            vlg.padding = new RectOffset(5, 5, 10, 25);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // --- Section Header: MISIONES DIARIAS ---
            CreateSectionHeader(content, "DailyHeader", "MISIONES DIARIAS", CYAN_NEON);

            // --- 6 Daily Mission Cards ---
            CreateMissionCard(content, "Mission1",
                "Juega 3 Partidas", "Juega en cualquier modo",
                "play_matches", 2, 3, "50", "coins", false, false);

            CreateMissionCard(content, "Mission2",
                "Gana 1 Partida", "Victoria en cualquier modo",
                "win_matches", 1, 1, "100", "coins", true, false);

            CreateMissionCard(content, "Mission3",
                "Obt\u00E9n 500 Puntos", "Acumula puntos en partidas",
                "earn_points", 350, 500, "25", "gems", false, false);

            CreateMissionCard(content, "Mission4",
                "Juega con un Amigo", "Invita a un amigo a jugar",
                "play_matches", 0, 1, "50", "coins", false, false);

            CreateMissionCard(content, "Mission5",
                "Completa 3 Minijuegos", "Juega 3 tipos diferentes",
                "complete_minigames", 3, 3, "200", "coins", true, false);

            CreateMissionCard(content, "Mission6",
                "Precisi\u00F3n 80%", "Completa con 80%+ precisi\u00F3n",
                "earn_points", 1, 3, "30", "xp", false, false);

            // --- Section Header: MISIONES SEMANALES ---
            CreateSectionHeader(content, "WeeklyHeader", "MISIONES SEMANALES", PURPLE_WEEKLY);

            // --- 3 Weekly Mission Cards ---
            CreateMissionCard(content, "Weekly1",
                "Gana 10 Partidas", "Acumula victorias esta semana",
                "win_matches", 7, 10, "150", "gems", false, true);

            CreateMissionCard(content, "Weekly2",
                "Juega 20 Partidas", "Juega en cualquier modo",
                "play_matches", 15, 20, "500", "coins", false, true);

            CreateMissionCard(content, "Weekly3",
                "Alcanza Top 3", "Termina en podio 5 veces",
                "earn_points", 3, 5, "Gemas Premium", "gems", false, true);

            Debug.Log("[DailyMissionsUI] ScrollView creado (6 diarias + 3 semanales)");
        }

        private static void CreateSectionHeader(GameObject parent, string name, string title, Color color)
        {
            var header = FindOrCreate(parent.transform, name);

            var hlg = GetOrAdd<HorizontalLayoutGroup>(header);
            hlg.spacing = 10;
            hlg.padding = new RectOffset(5, 5, 8, 5);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            var hLE = GetOrAdd<LayoutElement>(header);
            hLE.preferredHeight = 45;

            // Icon
            var iconGO = FindOrCreate(header.transform, "Icon");
            var iconImg = GetOrAdd<Image>(iconGO);
            iconImg.color = color;
            iconImg.preserveAspect = true;
            Sprite iconSprite = LoadIcon(MISSIONS_ICONS_PATH + "MissionsIconNeon.png");
            if (iconSprite != null) iconImg.sprite = iconSprite;
            var iconLE = GetOrAdd<LayoutElement>(iconGO);
            iconLE.minWidth = 24;
            iconLE.minHeight = 24;
            iconLE.preferredWidth = 24;
            iconLE.preferredHeight = 24;

            // Title
            var titleGO = FindOrCreate(header.transform, "Title");
            var tTMP = GetOrAdd<TextMeshProUGUI>(titleGO);
            tTMP.text = title;
            tTMP.fontSize = 18;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.color = color;
            tTMP.alignment = TextAlignmentOptions.MidlineLeft;
            var tLE = GetOrAdd<LayoutElement>(titleGO);
            tLE.minWidth = 300;
        }

        private static void CreateMissionCard(GameObject parent, string name,
            string title, string description, string iconType,
            int current, int target, string rewardAmount, string rewardType,
            bool isCompleted, bool isWeekly)
        {
            Color accentColor = isWeekly ? PURPLE_WEEKLY : CYAN_NEON;
            float progressPercent = target > 0 ? (float)current / target : 0f;

            var card = FindOrCreate(parent.transform, name);
            var cardBg = GetOrAdd<Image>(card);
            cardBg.color = CARD_BG;
            var cardOutline = GetOrAdd<Outline>(card);
            cardOutline.effectColor = isCompleted ? GREEN_SUCCESS : (isWeekly ? new Color(PURPLE_WEEKLY.r * 0.5f, PURPLE_WEEKLY.g * 0.5f, PURPLE_WEEKLY.b * 0.5f, 1f) : CYAN_DARK);
            cardOutline.effectDistance = new Vector2(1, 1);

            var cardLE = GetOrAdd<LayoutElement>(card);
            cardLE.preferredHeight = 100;

            var cardHlg = GetOrAdd<HorizontalLayoutGroup>(card);
            cardHlg.spacing = 12;
            cardHlg.padding = new RectOffset(12, 12, 10, 10);
            cardHlg.childAlignment = TextAnchor.MiddleCenter;
            cardHlg.childControlWidth = false;
            cardHlg.childControlHeight = true;
            cardHlg.childForceExpandWidth = false;
            cardHlg.childForceExpandHeight = false;

            // === Left Section: Mission Icon Area ===
            var iconContainer = FindOrCreate(card.transform, "IconContainer");
            var icLE = GetOrAdd<LayoutElement>(iconContainer);
            icLE.minWidth = 60;
            icLE.minHeight = 60;
            icLE.preferredWidth = 60;
            icLE.preferredHeight = 60;

            // Icon Glow (behind icon)
            var iconGlow = FindOrCreate(iconContainer.transform, "IconGlow");
            var igRT = GetOrAdd<RectTransform>(iconGlow);
            igRT.anchorMin = Vector2.zero;
            igRT.anchorMax = Vector2.one;
            igRT.offsetMin = Vector2.zero;
            igRT.offsetMax = Vector2.zero;
            var igImg = GetOrAdd<Image>(iconGlow);
            igImg.color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.15f);

            // Mission Icon
            var missionIcon = FindOrCreate(iconContainer.transform, "MissionIcon");
            var miRT = GetOrAdd<RectTransform>(missionIcon);
            miRT.anchorMin = new Vector2(0.5f, 0.5f);
            miRT.anchorMax = new Vector2(0.5f, 0.5f);
            miRT.sizeDelta = new Vector2(50, 50);
            miRT.anchoredPosition = Vector2.zero;
            var miImg = GetOrAdd<Image>(missionIcon);
            miImg.preserveAspect = true;
            miImg.color = Color.white;

            string iconPath = MISSIONS_ICONS_PATH + "icon_mission_" + iconType + ".png";
            Sprite missionSprite = LoadIcon(iconPath);
            if (missionSprite != null) miImg.sprite = missionSprite;

            // Completed check overlay
            if (isCompleted)
            {
                var checkOverlay = FindOrCreate(iconContainer.transform, "CheckOverlay");
                var coRT = GetOrAdd<RectTransform>(checkOverlay);
                coRT.anchorMin = new Vector2(0.6f, 0);
                coRT.anchorMax = new Vector2(1, 0.4f);
                coRT.offsetMin = Vector2.zero;
                coRT.offsetMax = Vector2.zero;
                var coTMP = GetOrAdd<TextMeshProUGUI>(checkOverlay);
                coTMP.text = "\u2713";
                coTMP.fontSize = 20;
                coTMP.fontStyle = FontStyles.Bold;
                coTMP.color = GREEN_SUCCESS;
                coTMP.alignment = TextAlignmentOptions.Center;
            }

            // === Center Section: Info ===
            var infoPanel = FindOrCreate(card.transform, "InfoPanel");
            var ipVlg = GetOrAdd<VerticalLayoutGroup>(infoPanel);
            ipVlg.spacing = 4;
            ipVlg.childAlignment = TextAnchor.MiddleLeft;
            ipVlg.childControlWidth = true;
            ipVlg.childControlHeight = false;
            ipVlg.childForceExpandWidth = true;
            ipVlg.childForceExpandHeight = false;
            var ipLE = GetOrAdd<LayoutElement>(infoPanel);
            ipLE.flexibleWidth = 1;

            // Title
            var titleGO = FindOrCreate(infoPanel.transform, "Title");
            var titleTMP = GetOrAdd<TextMeshProUGUI>(titleGO);
            titleTMP.text = title;
            titleTMP.fontSize = 17;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.color = isCompleted ? GREEN_SUCCESS : TEXT_WHITE;
            titleTMP.alignment = TextAlignmentOptions.MidlineLeft;
            var titleLE = GetOrAdd<LayoutElement>(titleGO);
            titleLE.preferredHeight = 22;

            // Description
            var descGO = FindOrCreate(infoPanel.transform, "Description");
            var descTMP = GetOrAdd<TextMeshProUGUI>(descGO);
            descTMP.text = description;
            descTMP.fontSize = 13;
            descTMP.color = TEXT_SECONDARY;
            descTMP.alignment = TextAlignmentOptions.MidlineLeft;
            descTMP.overflowMode = TextOverflowModes.Truncate;
            var descLE = GetOrAdd<LayoutElement>(descGO);
            descLE.preferredHeight = 18;

            // Progress Row
            var progressRow = FindOrCreate(infoPanel.transform, "ProgressRow");
            var prHlg = GetOrAdd<HorizontalLayoutGroup>(progressRow);
            prHlg.spacing = 8;
            prHlg.childAlignment = TextAnchor.MiddleLeft;
            prHlg.childControlWidth = false;
            prHlg.childControlHeight = true;
            prHlg.childForceExpandWidth = false;
            prHlg.childForceExpandHeight = false;
            var prLE = GetOrAdd<LayoutElement>(progressRow);
            prLE.preferredHeight = 20;

            // Progress bar mini
            var progressBar = FindOrCreate(progressRow.transform, "ProgressBar");
            var pbBg = GetOrAdd<Image>(progressBar);
            pbBg.color = new Color(0.1f, 0.12f, 0.15f, 1f);
            var pbLE = GetOrAdd<LayoutElement>(progressBar);
            pbLE.minWidth = 120;
            pbLE.minHeight = 14;
            pbLE.preferredWidth = 120;
            pbLE.preferredHeight = 14;

            var progressFill = FindOrCreate(progressBar.transform, "Fill");
            var pfRT = GetOrAdd<RectTransform>(progressFill);
            pfRT.anchorMin = Vector2.zero;
            pfRT.anchorMax = new Vector2(progressPercent, 1);
            pfRT.offsetMin = Vector2.zero;
            pfRT.offsetMax = Vector2.zero;
            var pfImg = GetOrAdd<Image>(progressFill);
            pfImg.color = isCompleted ? GREEN_SUCCESS : accentColor;

            // Progress text
            var progressText = FindOrCreate(progressRow.transform, "ProgressText");
            var ptTMP = GetOrAdd<TextMeshProUGUI>(progressText);
            ptTMP.text = current + "/" + target;
            ptTMP.fontSize = 13;
            ptTMP.fontStyle = FontStyles.Bold;
            ptTMP.color = TEXT_SECONDARY;
            ptTMP.alignment = TextAlignmentOptions.MidlineLeft;
            var ptLE = GetOrAdd<LayoutElement>(progressText);
            ptLE.minWidth = 60;

            // === Right Section: Reward + Action ===
            var rewardPanel = FindOrCreate(card.transform, "RewardPanel");
            var rpVlg = GetOrAdd<VerticalLayoutGroup>(rewardPanel);
            rpVlg.spacing = 6;
            rpVlg.childAlignment = TextAnchor.MiddleCenter;
            rpVlg.childControlWidth = true;
            rpVlg.childControlHeight = false;
            rpVlg.childForceExpandWidth = true;
            rpVlg.childForceExpandHeight = false;
            var rpLE = GetOrAdd<LayoutElement>(rewardPanel);
            rpLE.minWidth = 80;
            rpLE.preferredWidth = 80;

            // Reward display (HLG)
            var rewardDisplay = FindOrCreate(rewardPanel.transform, "RewardDisplay");
            var rdHlg = GetOrAdd<HorizontalLayoutGroup>(rewardDisplay);
            rdHlg.spacing = 4;
            rdHlg.childAlignment = TextAnchor.MiddleCenter;
            rdHlg.childControlWidth = false;
            rdHlg.childControlHeight = true;
            rdHlg.childForceExpandWidth = false;
            rdHlg.childForceExpandHeight = false;
            var rdLE = GetOrAdd<LayoutElement>(rewardDisplay);
            rdLE.preferredHeight = 24;

            // Currency icon
            Color rewardColor;
            string currencyIconPath;
            switch (rewardType)
            {
                case "gems":
                    rewardColor = GEM_COLOR;
                    currencyIconPath = CURRENCY_ICONS_PATH + "GemIconNeon.png";
                    break;
                case "xp":
                    rewardColor = XP_COLOR;
                    currencyIconPath = UI_ICONS_PATH + "icon_xp.png";
                    break;
                default: // coins
                    rewardColor = COIN_COLOR;
                    currencyIconPath = CURRENCY_ICONS_PATH + "CoinIconNeon.png";
                    break;
            }

            var currencyIcon = FindOrCreate(rewardDisplay.transform, "CurrencyIcon");
            var ciImg = GetOrAdd<Image>(currencyIcon);
            ciImg.color = rewardColor;
            ciImg.preserveAspect = true;
            Sprite currencySprite = LoadIcon(currencyIconPath);
            if (currencySprite != null) ciImg.sprite = currencySprite;
            var ciLE = GetOrAdd<LayoutElement>(currencyIcon);
            ciLE.minWidth = 20;
            ciLE.minHeight = 20;
            ciLE.preferredWidth = 20;
            ciLE.preferredHeight = 20;

            // Amount text
            var amountText = FindOrCreate(rewardDisplay.transform, "Amount");
            var atTMP = GetOrAdd<TextMeshProUGUI>(amountText);
            atTMP.text = rewardAmount;
            atTMP.fontSize = 15;
            atTMP.fontStyle = FontStyles.Bold;
            atTMP.color = rewardColor;
            atTMP.alignment = TextAlignmentOptions.MidlineLeft;
            var atLE = GetOrAdd<LayoutElement>(amountText);
            atLE.minWidth = 50;

            // Action button
            var actionBtn = FindOrCreate(rewardPanel.transform, "ActionButton");
            var abBg = GetOrAdd<Image>(actionBtn);
            var abBtn = GetOrAdd<Button>(actionBtn);
            abBtn.targetGraphic = abBg;
            var abLE = GetOrAdd<LayoutElement>(actionBtn);
            abLE.preferredHeight = 32;

            var actionText = FindOrCreate(actionBtn.transform, "Text");
            var actRT = GetOrAdd<RectTransform>(actionText);
            actRT.anchorMin = Vector2.zero;
            actRT.anchorMax = Vector2.one;
            actRT.offsetMin = Vector2.zero;
            actRT.offsetMax = Vector2.zero;
            var actTMP = GetOrAdd<TextMeshProUGUI>(actionText);

            if (isCompleted)
            {
                abBg.color = GREEN_SUCCESS;
                actTMP.text = "Reclamar";
                actTMP.fontSize = 13;
                actTMP.fontStyle = FontStyles.Bold;
                actTMP.color = TEXT_DARK;
                actTMP.alignment = TextAlignmentOptions.Center;
                var abOutline = GetOrAdd<Outline>(actionBtn);
                abOutline.effectColor = GREEN_SUCCESS;
                abOutline.effectDistance = new Vector2(1, 1);
            }
            else
            {
                abBg.color = CARD_BG;
                abBtn.interactable = false;
                actTMP.text = "En Progreso";
                actTMP.fontSize = 12;
                actTMP.color = TEXT_SECONDARY;
                actTMP.alignment = TextAlignmentOptions.Center;
            }
        }

        #endregion

        #region Reward Claim Popup

        private static void CreateRewardClaimPopup()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            // Blocker
            var blocker = FindOrCreate(canvas.transform, "RewardClaimBlocker");
            blocker.SetActive(false);
            blocker.transform.SetAsLastSibling();

            var bkRT = GetOrAdd<RectTransform>(blocker);
            bkRT.anchorMin = Vector2.zero;
            bkRT.anchorMax = Vector2.one;
            bkRT.offsetMin = Vector2.zero;
            bkRT.offsetMax = Vector2.zero;
            GetOrAdd<Image>(blocker).color = new Color(0f, 0f, 0f, 0.85f);
            var bkBtn = GetOrAdd<Button>(blocker);
            bkBtn.transition = Selectable.Transition.None;

            // Popup
            var popup = FindOrCreate(blocker.transform, "RewardPopup");
            var ppRT = GetOrAdd<RectTransform>(popup);
            ppRT.anchorMin = new Vector2(0.5f, 0.5f);
            ppRT.anchorMax = new Vector2(0.5f, 0.5f);
            ppRT.pivot = new Vector2(0.5f, 0.5f);
            ppRT.sizeDelta = new Vector2(400, 350);
            ppRT.anchoredPosition = Vector2.zero;

            var ppBg = GetOrAdd<Image>(popup);
            ppBg.color = CARD_BG;
            var ppOutline = GetOrAdd<Outline>(popup);
            ppOutline.effectColor = GOLD;
            ppOutline.effectDistance = new Vector2(2, 2);

            var vlg = GetOrAdd<VerticalLayoutGroup>(popup);
            vlg.spacing = 18;
            vlg.padding = new RectOffset(25, 25, 25, 25);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Celebration Icon
            var celebIcon = FindOrCreate(popup.transform, "CelebrationIcon");
            var ceImg = GetOrAdd<Image>(celebIcon);
            ceImg.color = GOLD;
            ceImg.preserveAspect = true;
            Sprite claimSprite = LoadIcon(UI_ICONS_PATH + "icon_ui_claim.png");
            if (claimSprite != null) ceImg.sprite = claimSprite;
            var ceLE = GetOrAdd<LayoutElement>(celebIcon);
            ceLE.preferredWidth = 60;
            ceLE.preferredHeight = 60;

            // Title
            var popupTitle = FindOrCreate(popup.transform, "Title");
            var ptTMP = GetOrAdd<TextMeshProUGUI>(popupTitle);
            ptTMP.text = "\u00A1Misi\u00F3n Completada!";
            ptTMP.fontSize = 26;
            ptTMP.fontStyle = FontStyles.Bold;
            ptTMP.color = GOLD;
            ptTMP.alignment = TextAlignmentOptions.Center;
            var ptLE = GetOrAdd<LayoutElement>(popupTitle);
            ptLE.preferredHeight = 35;

            // Mission Name
            var missionName = FindOrCreate(popup.transform, "MissionName");
            var mnTMP = GetOrAdd<TextMeshProUGUI>(missionName);
            mnTMP.text = "Nombre de la misi\u00F3n";
            mnTMP.fontSize = 17;
            mnTMP.color = TEXT_SECONDARY;
            mnTMP.alignment = TextAlignmentOptions.Center;
            var mnLE = GetOrAdd<LayoutElement>(missionName);
            mnLE.preferredHeight = 25;

            // Reward Display
            var rewardDisplay = FindOrCreate(popup.transform, "RewardDisplay");
            var rdHlg = GetOrAdd<HorizontalLayoutGroup>(rewardDisplay);
            rdHlg.spacing = 10;
            rdHlg.childAlignment = TextAnchor.MiddleCenter;
            rdHlg.childControlWidth = false;
            rdHlg.childControlHeight = true;
            rdHlg.childForceExpandWidth = false;
            rdHlg.childForceExpandHeight = false;
            var rdLE = GetOrAdd<LayoutElement>(rewardDisplay);
            rdLE.preferredHeight = 50;

            // Reward Icon
            var rewardIcon = FindOrCreate(rewardDisplay.transform, "Icon");
            var riImg = GetOrAdd<Image>(rewardIcon);
            riImg.color = COIN_COLOR;
            riImg.preserveAspect = true;
            Sprite coinSprite = LoadIcon(CURRENCY_ICONS_PATH + "CoinIconNeon.png");
            if (coinSprite != null) riImg.sprite = coinSprite;
            var riLE = GetOrAdd<LayoutElement>(rewardIcon);
            riLE.minWidth = 40;
            riLE.minHeight = 40;
            riLE.preferredWidth = 40;
            riLE.preferredHeight = 40;

            // Reward Amount
            var rewardAmount = FindOrCreate(rewardDisplay.transform, "Amount");
            var raTMP = GetOrAdd<TextMeshProUGUI>(rewardAmount);
            raTMP.text = "+100";
            raTMP.fontSize = 32;
            raTMP.fontStyle = FontStyles.Bold;
            raTMP.color = COIN_COLOR;
            raTMP.alignment = TextAlignmentOptions.MidlineLeft;
            var raLE = GetOrAdd<LayoutElement>(rewardAmount);
            raLE.minWidth = 120;

            // Collect Button
            var collectBtn = FindOrCreate(popup.transform, "CollectButton");
            var cbBg = GetOrAdd<Image>(collectBtn);
            cbBg.color = GREEN_SUCCESS;
            GetOrAdd<Button>(collectBtn).targetGraphic = cbBg;
            var cbLE = GetOrAdd<LayoutElement>(collectBtn);
            cbLE.preferredHeight = 50;

            var collectText = FindOrCreate(collectBtn.transform, "Text");
            var ctRT = GetOrAdd<RectTransform>(collectText);
            ctRT.anchorMin = Vector2.zero;
            ctRT.anchorMax = Vector2.one;
            ctRT.offsetMin = Vector2.zero;
            ctRT.offsetMax = Vector2.zero;
            var ctTMP = GetOrAdd<TextMeshProUGUI>(collectText);
            ctTMP.text = "Recoger";
            ctTMP.fontSize = 22;
            ctTMP.fontStyle = FontStyles.Bold;
            ctTMP.color = TEXT_DARK;
            ctTMP.alignment = TextAlignmentOptions.Center;

            Debug.Log("[DailyMissionsUI] RewardClaimPopup creado");
        }

        #endregion

        #region Manager References

        private static void SetupManagerReferences()
        {
            var manager = Object.FindFirstObjectByType<DigitPark.Progression.MissionsManager>();
            if (manager == null)
            {
                Debug.LogWarning("[DailyMissionsUI] MissionsManager no encontrado en la escena. Agrega el componente primero.");
                return;
            }

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            var so = new SerializedObject(manager);

            // Configuracion de misiones (sincronizar con UI)
            var dailyProp = so.FindProperty("dailyMissionsCount");
            if (dailyProp != null) dailyProp.intValue = 6;

            var weeklyProp = so.FindProperty("weeklyMissionsCount");
            if (weeklyProp != null) weeklyProp.intValue = 3;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);

            // Conectar BackButton a SceneNavigator
            Transform r = canvas.transform;
            var backBtn = FindInPath<Button>(r, "TopBar/BackButton");
            if (backBtn != null)
            {
                var navigator = Object.FindFirstObjectByType<DigitPark.Monetization.SceneNavigator>();
                if (navigator != null)
                {
                    backBtn.onClick.RemoveAllListeners();
                    UnityEditor.Events.UnityEventTools.AddStringPersistentListener(
                        backBtn.onClick,
                        navigator.NavigateTo,
                        DigitPark.Monetization.SceneNavigator.Scenes.MAIN_MENU
                    );
                    Debug.Log("[DailyMissionsUI] BackButton conectado a SceneNavigator -> MainMenu");
                }
            }

            Debug.Log("[DailyMissionsUI] Referencias del manager asignadas (dailyMissionsCount=6, weeklyMissionsCount=3)");
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

        #endregion

        #region Helpers

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

        private static Sprite LoadIcon(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        #endregion
    }
}
