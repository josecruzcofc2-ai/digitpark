using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Construye la UI completa de Daily Missions (Misiones Diarias)
    /// Incluye: SafeArea, Header, Timer, Progress, Lista de misiones, Popups
    /// </summary>
    public class DailyMissionsUIBuilder : EditorWindow
    {
        // ==================== COLORES DEL TEMA NEON ====================
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.4f, 1f);
        private static readonly Color CYAN_GLOW = new Color(0f, 1f, 1f, 0.3f);

        private static readonly Color DARK_BG = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color PANEL_BG = new Color(0.08f, 0.12f, 0.18f, 0.98f);
        private static readonly Color CARD_BG = new Color(0.06f, 0.1f, 0.15f, 1f);
        private static readonly Color HEADER_BG = new Color(0.03f, 0.06f, 0.1f, 0.95f);
        private static readonly Color POPUP_BG = new Color(0.05f, 0.08f, 0.12f, 0.98f);

        private static readonly Color TEXT_PRIMARY = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.7f, 0.75f, 1f);
        private static readonly Color TEXT_DARK = new Color(0.02f, 0.05f, 0.1f, 1f);

        private static readonly Color BUTTON_PRIMARY = CYAN_NEON;
        private static readonly Color BUTTON_SECONDARY = new Color(0.15f, 0.2f, 0.25f, 1f);
        private static readonly Color BUTTON_SUCCESS = new Color(0.2f, 0.8f, 0.4f, 1f);

        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color ORANGE_TIMER = new Color(1f, 0.5f, 0.1f, 1f);
        private static readonly Color PURPLE_WEEKLY = new Color(0.6f, 0.3f, 0.9f, 1f);

        private static readonly Color GEM_COLOR = new Color(0.4f, 0.8f, 1f, 1f);
        private static readonly Color COIN_COLOR = new Color(1f, 0.85f, 0.3f, 1f);
        private static readonly Color XP_COLOR = new Color(0.5f, 1f, 0.5f, 1f);

        private static readonly Color PROGRESS_BG = new Color(0.1f, 0.15f, 0.2f, 1f);
        private static readonly Color PROGRESS_FILL = CYAN_NEON;
        private static readonly Color MISSION_COMPLETE = new Color(0.2f, 0.7f, 0.3f, 1f);

        private static readonly Color BLOCKER_BG = new Color(0f, 0f, 0f, 0.85f);

        // ==================== DIMENSIONES ====================
        private const float HEADER_HEIGHT = 110f;
        private const float TIMER_HEIGHT = 50f;
        private const float OVERALL_PROGRESS_HEIGHT = 80f;
        private const float MISSION_CARD_HEIGHT = 110f;
        private const float CONTENT_PADDING = 20f;

        [MenuItem("DigitPark/UI Builders/Monetization/DailyMissions", false, 184)]
        public static void BuildUI()
        {
            if (!EditorUtility.DisplayDialog("Daily Missions UI Builder",
                "Esto construira la UI completa de Daily Missions.\nAsegurate de tener la escena DailyMissions abierta.\n\nContinuar?",
                "Si", "No"))
                return;

            BuildCompleteUI();
        }

        private static void BuildCompleteUI()
        {
            Debug.Log("[DailyMissionsUIBuilder] ========== INICIANDO CONSTRUCCION ==========");

            Canvas canvas = SetupCanvas();
            if (canvas == null) return;

            CreateBackground(canvas);
            GameObject safeArea = CreateSafeArea(canvas);

            CreateHeader(safeArea);
            CreateResetTimer(safeArea);
            CreateOverallProgress(safeArea);
            CreateMissionsScrollView(safeArea);

            CreateRewardClaimPopup(canvas);

            MarkSceneDirty();
            Debug.Log("[DailyMissionsUIBuilder] ========== CONSTRUCCION COMPLETADA ==========");
        }

        // ==================== CANVAS SETUP ====================

        private static Canvas SetupCanvas()
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();

            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;

                canvasObj.AddComponent<GraphicRaycaster>();
            }

            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            if (Camera.main == null)
            {
                GameObject cameraObj = new GameObject("Main Camera");
                Camera cam = cameraObj.AddComponent<Camera>();
                cam.tag = "MainCamera";
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = DARK_BG;
            }

            return canvas;
        }

        private static void CreateBackground(Canvas canvas)
        {
            GameObject bg = FindOrCreateChild(canvas.gameObject, "Background");

            RectTransform bgRT = GetOrAddComponent<RectTransform>(bg);
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;

            Image bgImage = GetOrAddComponent<Image>(bg);
            bgImage.color = DARK_BG;

            bg.transform.SetAsFirstSibling();
        }

        private static GameObject CreateSafeArea(Canvas canvas)
        {
            GameObject safeArea = FindOrCreateChild(canvas.gameObject, "SafeArea");

            RectTransform safeRT = GetOrAddComponent<RectTransform>(safeArea);
            safeRT.anchorMin = Vector2.zero;
            safeRT.anchorMax = Vector2.one;
            safeRT.sizeDelta = Vector2.zero;

            safeArea.transform.SetSiblingIndex(1);
            return safeArea;
        }

        // ==================== HEADER ====================

        private static void CreateHeader(GameObject parent)
        {
            GameObject header = FindOrCreateChild(parent, "Header");

            RectTransform headerRT = GetOrAddComponent<RectTransform>(header);
            headerRT.anchorMin = new Vector2(0, 1);
            headerRT.anchorMax = new Vector2(1, 1);
            headerRT.pivot = new Vector2(0.5f, 1);
            headerRT.anchoredPosition = Vector2.zero;
            headerRT.sizeDelta = new Vector2(0, HEADER_HEIGHT);

            Image headerBg = GetOrAddComponent<Image>(header);
            headerBg.color = HEADER_BG;

            CreateBottomGlow(header);

            // BackButton
            GameObject backBtn = FindOrCreateChild(header, "BackButton");
            RectTransform backRT = GetOrAddComponent<RectTransform>(backBtn);
            backRT.anchorMin = new Vector2(0, 0.5f);
            backRT.anchorMax = new Vector2(0, 0.5f);
            backRT.pivot = new Vector2(0, 0.5f);
            backRT.anchoredPosition = new Vector2(20, 0);
            backRT.sizeDelta = new Vector2(50, 50);

            Image backBg = GetOrAddComponent<Image>(backBtn);
            backBg.color = BUTTON_SECONDARY;

            Button backButton = GetOrAddComponent<Button>(backBtn);
            SetupButtonColors(backButton, BUTTON_SECONDARY);
            AddOutline(backBtn, CYAN_DARK);

            GameObject backTextObj = FindOrCreateChild(backBtn, "Text");
            TextMeshProUGUI backText = GetOrAddComponent<TextMeshProUGUI>(backTextObj);
            backText.text = "<";
            backText.fontSize = 32;
            backText.fontStyle = FontStyles.Bold;
            backText.color = CYAN_NEON;
            backText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(backTextObj);

            // Title
            GameObject titleObj = FindOrCreateChild(header, "TitleText");
            RectTransform titleRT = GetOrAddComponent<RectTransform>(titleObj);
            titleRT.anchorMin = new Vector2(0, 0.5f);
            titleRT.anchorMax = new Vector2(0, 0.5f);
            titleRT.pivot = new Vector2(0, 0.5f);
            titleRT.anchoredPosition = new Vector2(85, 0);
            titleRT.sizeDelta = new Vector2(300, 50);

            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "MISIONES DIARIAS";
            titleText.fontSize = 32;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = CYAN_NEON;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            AddOutline(titleObj, CYAN_GLOW, 2);

            // Info Button
            GameObject infoBtn = FindOrCreateChild(header, "InfoButton");
            RectTransform infoRT = GetOrAddComponent<RectTransform>(infoBtn);
            infoRT.anchorMin = new Vector2(1, 0.5f);
            infoRT.anchorMax = new Vector2(1, 0.5f);
            infoRT.pivot = new Vector2(1, 0.5f);
            infoRT.anchoredPosition = new Vector2(-20, 0);
            infoRT.sizeDelta = new Vector2(45, 45);

            Image infoBg = GetOrAddComponent<Image>(infoBtn);
            infoBg.color = BUTTON_SECONDARY;

            Button infoButton = GetOrAddComponent<Button>(infoBtn);
            SetupButtonColors(infoButton, BUTTON_SECONDARY);
            AddOutline(infoBtn, CYAN_DARK);

            GameObject infoTextObj = FindOrCreateChild(infoBtn, "Text");
            TextMeshProUGUI infoText = GetOrAddComponent<TextMeshProUGUI>(infoTextObj);
            infoText.text = "?";
            infoText.fontSize = 26;
            infoText.fontStyle = FontStyles.Bold;
            infoText.color = CYAN_NEON;
            infoText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(infoTextObj);

            Debug.Log("[DailyMissionsUIBuilder] Header creado");
        }

        // ==================== RESET TIMER ====================

        private static void CreateResetTimer(GameObject parent)
        {
            GameObject timer = FindOrCreateChild(parent, "ResetTimer");

            RectTransform timerRT = GetOrAddComponent<RectTransform>(timer);
            timerRT.anchorMin = new Vector2(0, 1);
            timerRT.anchorMax = new Vector2(1, 1);
            timerRT.pivot = new Vector2(0.5f, 1);
            timerRT.anchoredPosition = new Vector2(0, -HEADER_HEIGHT - 10);
            timerRT.sizeDelta = new Vector2(-CONTENT_PADDING * 2, TIMER_HEIGHT);

            Image timerBg = GetOrAddComponent<Image>(timer);
            timerBg.color = new Color(0.1f, 0.08f, 0.05f, 0.9f);
            AddOutline(timer, ORANGE_TIMER * 0.6f);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(timer);
            hlg.spacing = 12;
            hlg.padding = new RectOffset(20, 20, 8, 8);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            // Clock Icon
            GameObject iconObj = FindOrCreateChild(timer, "ClockIcon");
            Image iconImage = GetOrAddComponent<Image>(iconObj);
            iconImage.color = ORANGE_TIMER;
            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(iconObj);
            iconLE.minWidth = 28;
            iconLE.minHeight = 28;

            // Label
            GameObject labelObj = FindOrCreateChild(timer, "Label");
            TextMeshProUGUI labelText = GetOrAddComponent<TextMeshProUGUI>(labelObj);
            labelText.text = "Misiones se reinician en:";
            labelText.fontSize = 16;
            labelText.color = TEXT_SECONDARY;
            labelText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement labelLE = GetOrAddComponent<LayoutElement>(labelObj);
            labelLE.minWidth = 240;

            // Time
            GameObject timeObj = FindOrCreateChild(timer, "TimeText");
            TextMeshProUGUI timeText = GetOrAddComponent<TextMeshProUGUI>(timeObj);
            timeText.text = "12:34:56";
            timeText.fontSize = 22;
            timeText.fontStyle = FontStyles.Bold;
            timeText.color = ORANGE_TIMER;
            timeText.alignment = TextAlignmentOptions.MidlineRight;
            LayoutElement timeLE = GetOrAddComponent<LayoutElement>(timeObj);
            timeLE.flexibleWidth = 1;

            Debug.Log("[DailyMissionsUIBuilder] ResetTimer creado");
        }

        // ==================== OVERALL PROGRESS ====================

        private static void CreateOverallProgress(GameObject parent)
        {
            float topOffset = HEADER_HEIGHT + TIMER_HEIGHT + 25;

            GameObject progressPanel = FindOrCreateChild(parent, "OverallProgress");

            RectTransform progressRT = GetOrAddComponent<RectTransform>(progressPanel);
            progressRT.anchorMin = new Vector2(0, 1);
            progressRT.anchorMax = new Vector2(1, 1);
            progressRT.pivot = new Vector2(0.5f, 1);
            progressRT.anchoredPosition = new Vector2(0, -topOffset);
            progressRT.sizeDelta = new Vector2(-CONTENT_PADDING * 2, OVERALL_PROGRESS_HEIGHT);

            Image panelBg = GetOrAddComponent<Image>(progressPanel);
            panelBg.color = PANEL_BG;
            AddOutline(progressPanel, CYAN_DARK);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(progressPanel);
            vlg.spacing = 10;
            vlg.padding = new RectOffset(20, 20, 12, 12);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Title Row
            GameObject titleRow = FindOrCreateChild(progressPanel, "TitleRow");
            HorizontalLayoutGroup titleHlg = GetOrAddComponent<HorizontalLayoutGroup>(titleRow);
            titleHlg.spacing = 0;
            titleHlg.childAlignment = TextAnchor.MiddleCenter;
            titleHlg.childControlWidth = true;
            titleHlg.childControlHeight = true;
            titleHlg.childForceExpandWidth = true;

            LayoutElement titleRowLE = GetOrAddComponent<LayoutElement>(titleRow);
            titleRowLE.minHeight = 25;

            // Title Left
            GameObject titleLeft = FindOrCreateChild(titleRow, "TitleLeft");
            TextMeshProUGUI titleLeftText = GetOrAddComponent<TextMeshProUGUI>(titleLeft);
            titleLeftText.text = "Progreso Diario";
            titleLeftText.fontSize = 18;
            titleLeftText.fontStyle = FontStyles.Bold;
            titleLeftText.color = TEXT_PRIMARY;
            titleLeftText.alignment = TextAlignmentOptions.MidlineLeft;

            // Title Right
            GameObject titleRight = FindOrCreateChild(titleRow, "TitleRight");
            TextMeshProUGUI titleRightText = GetOrAddComponent<TextMeshProUGUI>(titleRight);
            titleRightText.text = "4/6 Misiones";
            titleRightText.fontSize = 16;
            titleRightText.color = TEXT_SECONDARY;
            titleRightText.alignment = TextAlignmentOptions.MidlineRight;

            // Progress Bar
            GameObject progressBar = FindOrCreateChild(progressPanel, "ProgressBar");
            LayoutElement progressBarLE = GetOrAddComponent<LayoutElement>(progressBar);
            progressBarLE.minHeight = 25;
            progressBarLE.preferredHeight = 25;

            Image progressBarBg = GetOrAddComponent<Image>(progressBar);
            progressBarBg.color = PROGRESS_BG;
            AddOutline(progressBar, CYAN_DARK * 0.5f);

            // Fill
            GameObject fill = FindOrCreateChild(progressBar, "Fill");
            RectTransform fillRT = GetOrAddComponent<RectTransform>(fill);
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = new Vector2(0.67f, 1); // 4/6 = 67%
            fillRT.sizeDelta = Vector2.zero;

            Image fillImage = GetOrAddComponent<Image>(fill);
            fillImage.color = PROGRESS_FILL;

            // Reward Markers (at 3, 5, 6 missions)
            CreateProgressMarker(progressBar, "Marker3", 0.5f, "50", false);
            CreateProgressMarker(progressBar, "Marker5", 0.833f, "100", false);
            CreateProgressMarker(progressBar, "Marker6", 1f, "200", true);

            Debug.Log("[DailyMissionsUIBuilder] OverallProgress creado");
        }

        private static void CreateProgressMarker(GameObject parent, string name, float position, string reward, bool isBonus)
        {
            GameObject marker = FindOrCreateChild(parent, name);

            RectTransform markerRT = GetOrAddComponent<RectTransform>(marker);
            markerRT.anchorMin = new Vector2(position, 0);
            markerRT.anchorMax = new Vector2(position, 1);
            markerRT.pivot = new Vector2(0.5f, 0.5f);
            markerRT.sizeDelta = new Vector2(40, 0);

            // Marker circle
            GameObject circle = FindOrCreateChild(marker, "Circle");
            RectTransform circleRT = GetOrAddComponent<RectTransform>(circle);
            circleRT.anchorMin = new Vector2(0.5f, 0.5f);
            circleRT.anchorMax = new Vector2(0.5f, 0.5f);
            circleRT.sizeDelta = new Vector2(32, 32);

            Image circleImage = GetOrAddComponent<Image>(circle);
            circleImage.color = isBonus ? GOLD : CYAN_DARK;
            AddOutline(circle, isBonus ? GOLD : CYAN_NEON);

            // Reward amount
            GameObject rewardObj = FindOrCreateChild(circle, "Reward");
            TextMeshProUGUI rewardText = GetOrAddComponent<TextMeshProUGUI>(rewardObj);
            rewardText.text = reward;
            rewardText.fontSize = 10;
            rewardText.fontStyle = FontStyles.Bold;
            rewardText.color = TEXT_PRIMARY;
            rewardText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(rewardObj);
        }

        // ==================== MISSIONS SCROLL VIEW ====================

        private static void CreateMissionsScrollView(GameObject parent)
        {
            float topOffset = HEADER_HEIGHT + TIMER_HEIGHT + OVERALL_PROGRESS_HEIGHT + 50;

            GameObject scrollView = FindOrCreateChild(parent, "MissionsScrollView");

            RectTransform scrollRT = GetOrAddComponent<RectTransform>(scrollView);
            scrollRT.anchorMin = Vector2.zero;
            scrollRT.anchorMax = Vector2.one;
            scrollRT.offsetMin = new Vector2(CONTENT_PADDING, CONTENT_PADDING);
            scrollRT.offsetMax = new Vector2(-CONTENT_PADDING, -topOffset);

            ScrollRect scrollRect = GetOrAddComponent<ScrollRect>(scrollView);
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;

            Image scrollBg = GetOrAddComponent<Image>(scrollView);
            scrollBg.color = Color.clear;

            // Viewport
            GameObject viewport = FindOrCreateChild(scrollView, "Viewport");
            SetRectTransformStretch(viewport);
            RectTransform viewportRT = viewport.GetComponent<RectTransform>();
            GetOrAddComponent<RectMask2D>(viewport);
            scrollRect.viewport = viewportRT;

            // Content
            GameObject content = FindOrCreateChild(viewport, "Content");
            RectTransform contentRT = GetOrAddComponent<RectTransform>(content);
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.sizeDelta = new Vector2(0, 0);
            scrollRect.content = contentRT;

            ContentSizeFitter csf = GetOrAddComponent<ContentSizeFitter>(content);
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(content);
            vlg.spacing = 15;
            vlg.padding = new RectOffset(0, 0, 10, 30);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Section: Daily Missions
            CreateSectionHeader(content, "DailyHeader", "MISIONES DIARIAS", CYAN_NEON);

            // Daily Mission Cards
            CreateMissionCard(content, "Mission1", "Juega 3 Partidas", "Participa en cualquier modo de juego", "2/3", 66, COIN_COLOR, "50", false, false);
            CreateMissionCard(content, "Mission2", "Gana 1 Partida", "Gana en cualquier modo", "1/1", 100, COIN_COLOR, "100", true, false);
            CreateMissionCard(content, "Mission3", "Obtén 500 Puntos", "Acumula puntos en partidas", "350/500", 70, GEM_COLOR, "25", false, false);
            CreateMissionCard(content, "Mission4", "Juega con un Amigo", "Invita a un amigo a jugar", "0/1", 0, GEM_COLOR, "50", false, false);
            CreateMissionCard(content, "Mission5", "Completa un Torneo", "Termina un torneo completo", "1/1", 100, COIN_COLOR, "200", true, false);
            CreateMissionCard(content, "Mission6", "Usa 3 Power-ups", "Activa power-ups en partidas", "1/3", 33, XP_COLOR, "30 XP", false, false);

            // Section: Weekly Missions
            CreateSectionHeader(content, "WeeklyHeader", "MISIONES SEMANALES", PURPLE_WEEKLY);

            // Weekly Mission Cards
            CreateMissionCard(content, "WeeklyMission1", "Gana 10 Partidas", "Acumula victorias esta semana", "7/10", 70, GEM_COLOR, "150", false, true);
            CreateMissionCard(content, "WeeklyMission2", "Juega 20 Partidas", "Juega partidas en cualquier modo", "15/20", 75, COIN_COLOR, "500", false, true);
            CreateMissionCard(content, "WeeklyMission3", "Alcanza Top 3", "Termina en podio 5 veces", "3/5", 60, GOLD, "Cofre Premium", false, true);

            Debug.Log("[DailyMissionsUIBuilder] MissionsScrollView creado");
        }

        private static void CreateSectionHeader(GameObject parent, string name, string title, Color color)
        {
            GameObject header = FindOrCreateChild(parent, name);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(header);
            hlg.spacing = 12;
            hlg.padding = new RectOffset(5, 5, 10, 5);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;

            LayoutElement headerLE = GetOrAddComponent<LayoutElement>(header);
            headerLE.minHeight = 40;

            // Icon
            GameObject iconObj = FindOrCreateChild(header, "Icon");
            Image iconImage = GetOrAddComponent<Image>(iconObj);
            iconImage.color = color;
            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(iconObj);
            iconLE.minWidth = 24;
            iconLE.minHeight = 24;

            // Title
            GameObject titleObj = FindOrCreateChild(header, "Title");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = title;
            titleText.fontSize = 20;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = color;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minWidth = 300;
        }

        private static void CreateMissionCard(GameObject parent, string name, string title, string description,
            string progressText, int progressPercent, Color rewardColor, string rewardAmount,
            bool isCompleted, bool isWeekly)
        {
            GameObject card = FindOrCreateChild(parent, name);

            Image cardBg = GetOrAddComponent<Image>(card);
            cardBg.color = isCompleted ? new Color(0.08f, 0.15f, 0.1f, 1f) : CARD_BG;
            AddOutline(card, isCompleted ? MISSION_COMPLETE : (isWeekly ? PURPLE_WEEKLY * 0.5f : CYAN_DARK * 0.5f));

            LayoutElement cardLE = GetOrAddComponent<LayoutElement>(card);
            cardLE.minHeight = MISSION_CARD_HEIGHT;
            cardLE.preferredHeight = MISSION_CARD_HEIGHT;

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(card);
            hlg.spacing = 15;
            hlg.padding = new RectOffset(15, 15, 12, 12);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            // Mission Icon
            GameObject iconObj = FindOrCreateChild(card, "Icon");
            Image iconImage = GetOrAddComponent<Image>(iconObj);
            iconImage.color = isWeekly ? PURPLE_WEEKLY : CYAN_NEON;
            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(iconObj);
            iconLE.minWidth = 55;
            iconLE.minHeight = 55;
            iconLE.preferredWidth = 55;
            iconLE.preferredHeight = 55;

            if (isCompleted)
            {
                // Checkmark overlay
                GameObject checkObj = FindOrCreateChild(iconObj, "Check");
                Image checkImage = GetOrAddComponent<Image>(checkObj);
                checkImage.color = MISSION_COMPLETE;
                SetRectTransformStretch(checkObj);

                GameObject checkText = FindOrCreateChild(checkObj, "Text");
                TextMeshProUGUI checkTmp = GetOrAddComponent<TextMeshProUGUI>(checkText);
                checkTmp.text = "✓";
                checkTmp.fontSize = 30;
                checkTmp.fontStyle = FontStyles.Bold;
                checkTmp.color = TEXT_PRIMARY;
                checkTmp.alignment = TextAlignmentOptions.Center;
                SetRectTransformStretch(checkText);
            }

            // Info Panel (Title, Description, Progress)
            GameObject infoPanel = FindOrCreateChild(card, "InfoPanel");
            VerticalLayoutGroup infoVlg = GetOrAddComponent<VerticalLayoutGroup>(infoPanel);
            infoVlg.spacing = 6;
            infoVlg.childAlignment = TextAnchor.MiddleLeft;
            infoVlg.childControlWidth = true;
            infoVlg.childControlHeight = true;
            infoVlg.childForceExpandWidth = true;
            infoVlg.childForceExpandHeight = false;

            LayoutElement infoLE = GetOrAddComponent<LayoutElement>(infoPanel);
            infoLE.flexibleWidth = 1;

            // Title
            GameObject titleObj = FindOrCreateChild(infoPanel, "Title");
            TextMeshProUGUI titleTmp = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleTmp.text = title;
            titleTmp.fontSize = 18;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = isCompleted ? MISSION_COMPLETE : TEXT_PRIMARY;
            titleTmp.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minHeight = 24;

            // Description
            GameObject descObj = FindOrCreateChild(infoPanel, "Description");
            TextMeshProUGUI descTmp = GetOrAddComponent<TextMeshProUGUI>(descObj);
            descTmp.text = description;
            descTmp.fontSize = 13;
            descTmp.color = TEXT_SECONDARY;
            descTmp.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement descLE = GetOrAddComponent<LayoutElement>(descObj);
            descLE.minHeight = 18;

            // Progress Row
            GameObject progressRow = FindOrCreateChild(infoPanel, "ProgressRow");
            HorizontalLayoutGroup progressHlg = GetOrAddComponent<HorizontalLayoutGroup>(progressRow);
            progressHlg.spacing = 10;
            progressHlg.childAlignment = TextAnchor.MiddleLeft;
            progressHlg.childControlWidth = false;
            progressHlg.childControlHeight = true;
            LayoutElement progressRowLE = GetOrAddComponent<LayoutElement>(progressRow);
            progressRowLE.minHeight = 22;

            // Progress Bar Mini
            GameObject progressBar = FindOrCreateChild(progressRow, "ProgressBar");
            Image progressBarBg = GetOrAddComponent<Image>(progressBar);
            progressBarBg.color = PROGRESS_BG;
            LayoutElement progressBarLE = GetOrAddComponent<LayoutElement>(progressBar);
            progressBarLE.minWidth = 120;
            progressBarLE.minHeight = 12;

            GameObject progressFill = FindOrCreateChild(progressBar, "Fill");
            RectTransform fillRT = GetOrAddComponent<RectTransform>(progressFill);
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = new Vector2(progressPercent / 100f, 1);
            fillRT.sizeDelta = Vector2.zero;

            Image fillImage = GetOrAddComponent<Image>(progressFill);
            fillImage.color = isCompleted ? MISSION_COMPLETE : (isWeekly ? PURPLE_WEEKLY : PROGRESS_FILL);

            // Progress Text
            GameObject progressTextObj = FindOrCreateChild(progressRow, "ProgressText");
            TextMeshProUGUI progressTmp = GetOrAddComponent<TextMeshProUGUI>(progressTextObj);
            progressTmp.text = progressText;
            progressTmp.fontSize = 14;
            progressTmp.fontStyle = FontStyles.Bold;
            progressTmp.color = isCompleted ? MISSION_COMPLETE : TEXT_SECONDARY;
            progressTmp.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement progressTextLE = GetOrAddComponent<LayoutElement>(progressTextObj);
            progressTextLE.minWidth = 80;

            // Reward Panel
            GameObject rewardPanel = FindOrCreateChild(card, "RewardPanel");
            VerticalLayoutGroup rewardVlg = GetOrAddComponent<VerticalLayoutGroup>(rewardPanel);
            rewardVlg.spacing = 8;
            rewardVlg.childAlignment = TextAnchor.MiddleCenter;
            rewardVlg.childControlWidth = true;
            rewardVlg.childControlHeight = true;
            rewardVlg.childForceExpandHeight = false;

            LayoutElement rewardLE = GetOrAddComponent<LayoutElement>(rewardPanel);
            rewardLE.minWidth = 85;
            rewardLE.preferredWidth = 85;

            // Reward Display
            GameObject rewardDisplay = FindOrCreateChild(rewardPanel, "RewardDisplay");
            HorizontalLayoutGroup rewardHlg = GetOrAddComponent<HorizontalLayoutGroup>(rewardDisplay);
            rewardHlg.spacing = 5;
            rewardHlg.childAlignment = TextAnchor.MiddleCenter;
            rewardHlg.childControlWidth = false;
            rewardHlg.childControlHeight = true;
            LayoutElement rewardDisplayLE = GetOrAddComponent<LayoutElement>(rewardDisplay);
            rewardDisplayLE.minHeight = 25;

            // Reward Icon
            GameObject rewardIcon = FindOrCreateChild(rewardDisplay, "Icon");
            Image rewardIconImage = GetOrAddComponent<Image>(rewardIcon);
            rewardIconImage.color = rewardColor;
            LayoutElement rewardIconLE = GetOrAddComponent<LayoutElement>(rewardIcon);
            rewardIconLE.minWidth = 22;
            rewardIconLE.minHeight = 22;

            // Reward Amount
            GameObject rewardAmountObj = FindOrCreateChild(rewardDisplay, "Amount");
            TextMeshProUGUI rewardAmountTmp = GetOrAddComponent<TextMeshProUGUI>(rewardAmountObj);
            rewardAmountTmp.text = rewardAmount;
            rewardAmountTmp.fontSize = 16;
            rewardAmountTmp.fontStyle = FontStyles.Bold;
            rewardAmountTmp.color = rewardColor;
            rewardAmountTmp.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement rewardAmountLE = GetOrAddComponent<LayoutElement>(rewardAmountObj);
            rewardAmountLE.minWidth = 55;

            // Claim Button or Status
            GameObject actionBtn = FindOrCreateChild(rewardPanel, "ActionButton");
            Image actionBg = GetOrAddComponent<Image>(actionBtn);
            Button actionButton = GetOrAddComponent<Button>(actionBtn);
            LayoutElement actionLE = GetOrAddComponent<LayoutElement>(actionBtn);
            actionLE.minHeight = 38;
            actionLE.preferredHeight = 38;

            GameObject actionTextObj = FindOrCreateChild(actionBtn, "Text");
            TextMeshProUGUI actionText = GetOrAddComponent<TextMeshProUGUI>(actionTextObj);
            actionText.fontSize = 14;
            actionText.fontStyle = FontStyles.Bold;
            actionText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(actionTextObj);

            if (isCompleted)
            {
                actionBg.color = BUTTON_SUCCESS;
                SetupButtonColors(actionButton, BUTTON_SUCCESS);
                actionText.text = "Reclamar";
                actionText.color = TEXT_DARK;
                AddOutline(actionBtn, new Color(0.3f, 1f, 0.5f, 0.5f));
            }
            else
            {
                actionBg.color = BUTTON_SECONDARY;
                SetupButtonColors(actionButton, BUTTON_SECONDARY);
                actionButton.interactable = false;
                actionText.text = "En Progreso";
                actionText.color = TEXT_SECONDARY;
            }
        }

        // ==================== REWARD CLAIM POPUP ====================

        private static void CreateRewardClaimPopup(Canvas canvas)
        {
            GameObject blocker = FindOrCreateChild(canvas.gameObject, "RewardClaimBlocker");
            blocker.SetActive(false);

            SetRectTransformStretch(blocker);
            Image blockerBg = GetOrAddComponent<Image>(blocker);
            blockerBg.color = BLOCKER_BG;
            Button blockerBtn = GetOrAddComponent<Button>(blocker);
            blockerBtn.transition = Selectable.Transition.None;
            blocker.transform.SetAsLastSibling();

            GameObject popup = FindOrCreateChild(blocker, "RewardPopup");
            RectTransform popupRT = GetOrAddComponent<RectTransform>(popup);
            popupRT.anchorMin = new Vector2(0.5f, 0.5f);
            popupRT.anchorMax = new Vector2(0.5f, 0.5f);
            popupRT.sizeDelta = new Vector2(420, 380);

            Image popupBg = GetOrAddComponent<Image>(popup);
            popupBg.color = POPUP_BG;
            AddOutline(popup, GOLD, 2);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(popup);
            vlg.spacing = 20;
            vlg.padding = new RectOffset(30, 30, 30, 30);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Celebration Icon
            GameObject celebrationObj = FindOrCreateChild(popup, "CelebrationIcon");
            Image celebrationImage = GetOrAddComponent<Image>(celebrationObj);
            celebrationImage.color = GOLD;
            LayoutElement celebrationLE = GetOrAddComponent<LayoutElement>(celebrationObj);
            celebrationLE.minHeight = 70;
            celebrationLE.minWidth = 70;
            celebrationLE.preferredHeight = 70;
            celebrationLE.preferredWidth = 70;

            // Title
            GameObject titleObj = FindOrCreateChild(popup, "Title");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "Mision Completada!";
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = GOLD;
            titleText.alignment = TextAlignmentOptions.Center;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minHeight = 40;

            // Mission Name
            GameObject missionNameObj = FindOrCreateChild(popup, "MissionName");
            TextMeshProUGUI missionNameText = GetOrAddComponent<TextMeshProUGUI>(missionNameObj);
            missionNameText.text = "Gana 1 Partida";
            missionNameText.fontSize = 18;
            missionNameText.color = TEXT_SECONDARY;
            missionNameText.alignment = TextAlignmentOptions.Center;
            LayoutElement missionNameLE = GetOrAddComponent<LayoutElement>(missionNameObj);
            missionNameLE.minHeight = 25;

            // Reward Display
            GameObject rewardDisplay = FindOrCreateChild(popup, "RewardDisplay");
            HorizontalLayoutGroup rewardHlg = GetOrAddComponent<HorizontalLayoutGroup>(rewardDisplay);
            rewardHlg.spacing = 15;
            rewardHlg.childAlignment = TextAnchor.MiddleCenter;
            rewardHlg.childControlWidth = false;
            rewardHlg.childControlHeight = true;
            LayoutElement rewardDisplayLE = GetOrAddComponent<LayoutElement>(rewardDisplay);
            rewardDisplayLE.minHeight = 50;

            // Reward Icon
            GameObject rewardIconObj = FindOrCreateChild(rewardDisplay, "Icon");
            Image rewardIconImage = GetOrAddComponent<Image>(rewardIconObj);
            rewardIconImage.color = COIN_COLOR;
            LayoutElement rewardIconLE = GetOrAddComponent<LayoutElement>(rewardIconObj);
            rewardIconLE.minWidth = 45;
            rewardIconLE.minHeight = 45;

            // Reward Amount
            GameObject rewardAmountObj = FindOrCreateChild(rewardDisplay, "Amount");
            TextMeshProUGUI rewardAmountText = GetOrAddComponent<TextMeshProUGUI>(rewardAmountObj);
            rewardAmountText.text = "+100";
            rewardAmountText.fontSize = 36;
            rewardAmountText.fontStyle = FontStyles.Bold;
            rewardAmountText.color = COIN_COLOR;
            rewardAmountText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement rewardAmountLE = GetOrAddComponent<LayoutElement>(rewardAmountObj);
            rewardAmountLE.minWidth = 100;

            // Collect Button
            GameObject collectBtn = FindOrCreateChild(popup, "CollectButton");
            Image collectBg = GetOrAddComponent<Image>(collectBtn);
            collectBg.color = BUTTON_SUCCESS;
            Button collectButton = GetOrAddComponent<Button>(collectBtn);
            SetupButtonColors(collectButton, BUTTON_SUCCESS);
            AddOutline(collectBtn, new Color(0.3f, 1f, 0.5f, 0.5f), 2);
            LayoutElement collectLE = GetOrAddComponent<LayoutElement>(collectBtn);
            collectLE.minHeight = 55;
            collectLE.preferredHeight = 55;

            GameObject collectTextObj = FindOrCreateChild(collectBtn, "Text");
            TextMeshProUGUI collectText = GetOrAddComponent<TextMeshProUGUI>(collectTextObj);
            collectText.text = "Recoger";
            collectText.fontSize = 22;
            collectText.fontStyle = FontStyles.Bold;
            collectText.color = TEXT_DARK;
            collectText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(collectTextObj);

            Debug.Log("[DailyMissionsUIBuilder] RewardClaimPopup creado");
        }

        // ==================== UTILITY METHODS ====================

        private static void CreateBottomGlow(GameObject obj)
        {
            GameObject glow = FindOrCreateChild(obj, "BottomGlow");
            RectTransform glowRT = GetOrAddComponent<RectTransform>(glow);
            glowRT.anchorMin = new Vector2(0, 0);
            glowRT.anchorMax = new Vector2(1, 0);
            glowRT.pivot = new Vector2(0.5f, 1);
            glowRT.anchoredPosition = Vector2.zero;
            glowRT.sizeDelta = new Vector2(0, 3);

            Image glowImage = GetOrAddComponent<Image>(glow);
            glowImage.color = CYAN_NEON;
        }

        private static void SetRectTransformStretch(GameObject obj)
        {
            RectTransform rt = GetOrAddComponent<RectTransform>(obj);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }

        private static T GetOrAddComponent<T>(GameObject obj) where T : Component
        {
            T component = obj.GetComponent<T>();
            if (component == null)
                component = obj.AddComponent<T>();
            return component;
        }

        private static GameObject FindOrCreateChild(GameObject parent, string childName)
        {
            Transform child = parent.transform.Find(childName);
            if (child != null) return child.gameObject;

            GameObject newChild = new GameObject(childName);
            newChild.transform.SetParent(parent.transform, false);

            if (newChild.GetComponent<RectTransform>() == null)
                newChild.AddComponent<RectTransform>();

            return newChild;
        }

        private static void SetupButtonColors(Button btn, Color baseColor)
        {
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            btn.colors = colors;
        }

        private static void AddOutline(GameObject obj, Color color, float distance = 1)
        {
            Outline outline = obj.GetComponent<Outline>();
            if (outline == null)
                outline = obj.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(distance, distance);
        }

        private static void MarkSceneDirty()
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
    }
}
