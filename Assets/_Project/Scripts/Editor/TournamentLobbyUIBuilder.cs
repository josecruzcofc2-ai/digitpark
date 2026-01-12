using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Construye la UI completa de TournamentLobby
    /// Incluye: SafeArea, Header, TournamentInfo, Leaderboard, ActionButtons, Popups
    /// </summary>
    public class TournamentLobbyUIBuilder : EditorWindow
    {
        // ==================== COLORES DEL TEMA NEON ====================
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.4f, 1f);
        private static readonly Color CYAN_GLOW = new Color(0f, 1f, 1f, 0.3f);

        private static readonly Color DARK_BG = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color PANEL_BG = new Color(0.08f, 0.12f, 0.18f, 0.98f);
        private static readonly Color POPUP_BG = new Color(0.05f, 0.08f, 0.12f, 0.98f);
        private static readonly Color HEADER_BG = new Color(0.03f, 0.06f, 0.1f, 0.95f);

        private static readonly Color TEXT_PRIMARY = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.7f, 0.75f, 1f);
        private static readonly Color TEXT_DARK = new Color(0.02f, 0.05f, 0.1f, 1f);

        private static readonly Color BUTTON_PRIMARY = CYAN_NEON;
        private static readonly Color BUTTON_SECONDARY = new Color(0.15f, 0.2f, 0.25f, 1f);
        private static readonly Color BUTTON_DANGER = new Color(0.9f, 0.2f, 0.2f, 1f);
        private static readonly Color BUTTON_SUCCESS = new Color(0.2f, 0.8f, 0.4f, 1f);

        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color SILVER = new Color(0.75f, 0.75f, 0.75f, 1f);
        private static readonly Color BRONZE = new Color(0.8f, 0.5f, 0.2f, 1f);

        private static readonly Color BLOCKER_BG = new Color(0f, 0f, 0f, 0.85f);

        // ==================== DIMENSIONES ====================
        private const float HEADER_HEIGHT = 100f;
        private const float INFO_PANEL_HEIGHT = 180f;
        private const float MY_POSITION_HEIGHT = 70f;
        private const float ACTION_BUTTONS_HEIGHT = 80f;
        private const float CONTENT_PADDING = 20f;

        [MenuItem("DigitPark/Tournaments/Build TournamentLobby UI", false, 12)]
        public static void BuildUI()
        {
            if (!EditorUtility.DisplayDialog("TournamentLobby UI Builder",
                "Esto construira la UI completa de TournamentLobby.\nAsegurate de tener la escena TournamentLobby abierta.\n\nContinuar?",
                "Si", "No"))
                return;

            BuildCompleteUI();
        }

        private static void BuildCompleteUI()
        {
            Debug.Log("[TournamentLobbyUIBuilder] ========== INICIANDO CONSTRUCCION ==========");

            Canvas canvas = SetupCanvas();
            if (canvas == null) return;

            CreateBackground(canvas);
            GameObject safeArea = CreateSafeArea(canvas);

            CreateHeader(safeArea);
            CreateTournamentInfoPanel(safeArea);
            CreateLeaderboard(safeArea);
            CreateMyPositionPanel(safeArea);
            CreateActionButtons(safeArea);

            CreatePrizesPopup(canvas);
            CreateLeaveConfirmPopup(canvas);

            MarkSceneDirty();
            Debug.Log("[TournamentLobbyUIBuilder] ========== CONSTRUCCION COMPLETADA ==========");
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

            // Tournament Name (dynamic)
            GameObject titleObj = FindOrCreateChild(header, "TournamentNameText");
            RectTransform titleRT = GetOrAddComponent<RectTransform>(titleObj);
            titleRT.anchorMin = new Vector2(0.5f, 0.5f);
            titleRT.anchorMax = new Vector2(0.5f, 0.5f);
            titleRT.sizeDelta = new Vector2(600, 60);

            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "Nombre del Torneo";
            titleText.fontSize = 32;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = CYAN_NEON;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = 18;
            titleText.fontSizeMax = 32;
            AddOutline(titleObj, CYAN_GLOW, 2);

            // Prizes Button
            GameObject prizesBtn = FindOrCreateChild(header, "PrizesButton");
            RectTransform prizesRT = GetOrAddComponent<RectTransform>(prizesBtn);
            prizesRT.anchorMin = new Vector2(1, 0.5f);
            prizesRT.anchorMax = new Vector2(1, 0.5f);
            prizesRT.pivot = new Vector2(1, 0.5f);
            prizesRT.anchoredPosition = new Vector2(-20, 0);
            prizesRT.sizeDelta = new Vector2(50, 50);

            Image prizesBg = GetOrAddComponent<Image>(prizesBtn);
            prizesBg.color = GOLD;

            Button prizesButton = GetOrAddComponent<Button>(prizesBtn);
            SetupButtonColors(prizesButton, GOLD);
            AddOutline(prizesBtn, new Color(1f, 0.84f, 0f, 0.5f));

            GameObject prizesTextObj = FindOrCreateChild(prizesBtn, "Text");
            TextMeshProUGUI prizesText = GetOrAddComponent<TextMeshProUGUI>(prizesTextObj);
            prizesText.text = "$";
            prizesText.fontSize = 28;
            prizesText.fontStyle = FontStyles.Bold;
            prizesText.color = TEXT_DARK;
            prizesText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(prizesTextObj);

            Debug.Log("[TournamentLobbyUIBuilder] Header creado");
        }

        // ==================== TOURNAMENT INFO PANEL ====================

        private static void CreateTournamentInfoPanel(GameObject parent)
        {
            GameObject infoPanel = FindOrCreateChild(parent, "TournamentInfoPanel");

            RectTransform infoRT = GetOrAddComponent<RectTransform>(infoPanel);
            infoRT.anchorMin = new Vector2(0, 1);
            infoRT.anchorMax = new Vector2(1, 1);
            infoRT.pivot = new Vector2(0.5f, 1);
            infoRT.anchoredPosition = new Vector2(0, -HEADER_HEIGHT - 10);
            infoRT.sizeDelta = new Vector2(-CONTENT_PADDING * 2, INFO_PANEL_HEIGHT);

            Image infoBg = GetOrAddComponent<Image>(infoPanel);
            infoBg.color = PANEL_BG;
            AddOutline(infoPanel, CYAN_DARK);

            // Layout
            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(infoPanel);
            vlg.spacing = 15;
            vlg.padding = new RectOffset(25, 25, 20, 20);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Game Info Row
            GameObject gameRow = FindOrCreateChild(infoPanel, "GameRow");
            HorizontalLayoutGroup gameHlg = GetOrAddComponent<HorizontalLayoutGroup>(gameRow);
            gameHlg.spacing = 15;
            gameHlg.childAlignment = TextAnchor.MiddleCenter;
            gameHlg.childControlWidth = false;
            gameHlg.childControlHeight = true;

            LayoutElement gameLE = GetOrAddComponent<LayoutElement>(gameRow);
            gameLE.minHeight = 50;

            // Game Icon
            GameObject iconObj = FindOrCreateChild(gameRow, "GameIcon");
            RectTransform iconRT = GetOrAddComponent<RectTransform>(iconObj);
            iconRT.sizeDelta = new Vector2(50, 50);
            Image iconImage = GetOrAddComponent<Image>(iconObj);
            iconImage.color = CYAN_DARK;
            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(iconObj);
            iconLE.minWidth = 50;
            iconLE.minHeight = 50;

            // Game Name
            GameObject gameNameObj = FindOrCreateChild(gameRow, "GameName");
            TextMeshProUGUI gameNameText = GetOrAddComponent<TextMeshProUGUI>(gameNameObj);
            gameNameText.text = "Digit Rush";
            gameNameText.fontSize = 24;
            gameNameText.fontStyle = FontStyles.Bold;
            gameNameText.color = TEXT_PRIMARY;
            LayoutElement gameNameLE = GetOrAddComponent<LayoutElement>(gameNameObj);
            gameNameLE.flexibleWidth = 1;
            gameNameLE.minHeight = 35;

            // Stats Row
            GameObject statsRow = FindOrCreateChild(infoPanel, "StatsRow");
            HorizontalLayoutGroup statsHlg = GetOrAddComponent<HorizontalLayoutGroup>(statsRow);
            statsHlg.spacing = 10;
            statsHlg.childAlignment = TextAnchor.MiddleCenter;
            statsHlg.childControlWidth = true;
            statsHlg.childControlHeight = true;
            statsHlg.childForceExpandWidth = true;
            statsHlg.childForceExpandHeight = true;

            LayoutElement statsLE = GetOrAddComponent<LayoutElement>(statsRow);
            statsLE.minHeight = 70;

            // Entry Fee Stat
            CreateStatItem(statsRow, "EntryFeeStat", "Entrada", "GRATIS");

            // Prize Pool Stat
            CreateStatItem(statsRow, "PrizePoolStat", "Premio", "$50");

            // Players Stat
            CreateStatItem(statsRow, "PlayersStat", "Jugadores", "12/16");

            // Status Row
            GameObject statusRow = FindOrCreateChild(infoPanel, "StatusRow");
            HorizontalLayoutGroup statusHlg = GetOrAddComponent<HorizontalLayoutGroup>(statusRow);
            statusHlg.spacing = 10;
            statusHlg.childAlignment = TextAnchor.MiddleCenter;
            statusHlg.childControlWidth = true;
            statusHlg.childControlHeight = true;
            statusHlg.childForceExpandWidth = true;

            LayoutElement statusLE = GetOrAddComponent<LayoutElement>(statusRow);
            statusLE.minHeight = 35;

            // Status Text
            GameObject statusObj = FindOrCreateChild(statusRow, "StatusText");
            TextMeshProUGUI statusText = GetOrAddComponent<TextMeshProUGUI>(statusObj);
            statusText.text = "EN PROGRESO";
            statusText.fontSize = 16;
            statusText.fontStyle = FontStyles.Bold;
            statusText.color = BUTTON_SUCCESS;
            statusText.alignment = TextAlignmentOptions.Center;

            // Time Remaining
            GameObject timeObj = FindOrCreateChild(statusRow, "TimeText");
            TextMeshProUGUI timeText = GetOrAddComponent<TextMeshProUGUI>(timeObj);
            timeText.text = "Termina en: 02:45:30";
            timeText.fontSize = 14;
            timeText.color = TEXT_SECONDARY;
            timeText.alignment = TextAlignmentOptions.Center;

            Debug.Log("[TournamentLobbyUIBuilder] TournamentInfoPanel creado");
        }

        private static void CreateStatItem(GameObject parent, string name, string label, string value)
        {
            GameObject item = FindOrCreateChild(parent, name);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(item);
            vlg.spacing = 5;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Label
            GameObject labelObj = FindOrCreateChild(item, "Label");
            TextMeshProUGUI labelText = GetOrAddComponent<TextMeshProUGUI>(labelObj);
            labelText.text = label;
            labelText.fontSize = 12;
            labelText.color = TEXT_SECONDARY;
            labelText.alignment = TextAlignmentOptions.Center;

            LayoutElement labelLE = GetOrAddComponent<LayoutElement>(labelObj);
            labelLE.minHeight = 18;

            // Value
            GameObject valueObj = FindOrCreateChild(item, "Value");
            TextMeshProUGUI valueText = GetOrAddComponent<TextMeshProUGUI>(valueObj);
            valueText.text = value;
            valueText.fontSize = 22;
            valueText.fontStyle = FontStyles.Bold;
            valueText.color = CYAN_NEON;
            valueText.alignment = TextAlignmentOptions.Center;

            LayoutElement valueLE = GetOrAddComponent<LayoutElement>(valueObj);
            valueLE.minHeight = 30;
        }

        // ==================== LEADERBOARD ====================

        private static void CreateLeaderboard(GameObject parent)
        {
            float topOffset = HEADER_HEIGHT + INFO_PANEL_HEIGHT + 30;
            float bottomOffset = MY_POSITION_HEIGHT + ACTION_BUTTONS_HEIGHT + 40;

            GameObject leaderboard = FindOrCreateChild(parent, "LeaderboardPanel");

            RectTransform lbRT = GetOrAddComponent<RectTransform>(leaderboard);
            lbRT.anchorMin = Vector2.zero;
            lbRT.anchorMax = Vector2.one;
            lbRT.offsetMin = new Vector2(CONTENT_PADDING, bottomOffset);
            lbRT.offsetMax = new Vector2(-CONTENT_PADDING, -topOffset);

            Image lbBg = GetOrAddComponent<Image>(leaderboard);
            lbBg.color = PANEL_BG;
            AddOutline(leaderboard, CYAN_DARK);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(leaderboard);
            vlg.spacing = 0;
            vlg.padding = new RectOffset(0, 0, 0, 0);
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Header Row
            CreateLeaderboardHeader(leaderboard);

            // ScrollView
            CreateLeaderboardScrollView(leaderboard);

            Debug.Log("[TournamentLobbyUIBuilder] Leaderboard creado");
        }

        private static void CreateLeaderboardHeader(GameObject parent)
        {
            GameObject header = FindOrCreateChild(parent, "LeaderboardHeader");

            RectTransform headerRT = GetOrAddComponent<RectTransform>(header);

            Image headerBg = GetOrAddComponent<Image>(header);
            headerBg.color = new Color(0.03f, 0.06f, 0.1f, 1f);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(header);
            hlg.spacing = 10;
            hlg.padding = new RectOffset(20, 20, 10, 10);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            LayoutElement headerLE = GetOrAddComponent<LayoutElement>(header);
            headerLE.minHeight = 45;

            // Rank Column
            GameObject rankCol = FindOrCreateChild(header, "RankColumn");
            TextMeshProUGUI rankText = GetOrAddComponent<TextMeshProUGUI>(rankCol);
            rankText.text = "#";
            rankText.fontSize = 16;
            rankText.fontStyle = FontStyles.Bold;
            rankText.color = TEXT_SECONDARY;
            rankText.alignment = TextAlignmentOptions.Center;
            LayoutElement rankLE = GetOrAddComponent<LayoutElement>(rankCol);
            rankLE.minWidth = 50;
            rankLE.preferredWidth = 50;

            // Player Column
            GameObject playerCol = FindOrCreateChild(header, "PlayerColumn");
            TextMeshProUGUI playerText = GetOrAddComponent<TextMeshProUGUI>(playerCol);
            playerText.text = "Jugador";
            playerText.fontSize = 16;
            playerText.fontStyle = FontStyles.Bold;
            playerText.color = TEXT_SECONDARY;
            playerText.alignment = TextAlignmentOptions.Left;
            LayoutElement playerLE = GetOrAddComponent<LayoutElement>(playerCol);
            playerLE.flexibleWidth = 1;

            // Score Column
            GameObject scoreCol = FindOrCreateChild(header, "ScoreColumn");
            TextMeshProUGUI scoreText = GetOrAddComponent<TextMeshProUGUI>(scoreCol);
            scoreText.text = "Puntos";
            scoreText.fontSize = 16;
            scoreText.fontStyle = FontStyles.Bold;
            scoreText.color = TEXT_SECONDARY;
            scoreText.alignment = TextAlignmentOptions.Right;
            LayoutElement scoreLE = GetOrAddComponent<LayoutElement>(scoreCol);
            scoreLE.minWidth = 100;
            scoreLE.preferredWidth = 100;
        }

        private static void CreateLeaderboardScrollView(GameObject parent)
        {
            GameObject scrollView = FindOrCreateChild(parent, "LeaderboardScrollView");

            ScrollRect scrollRect = GetOrAddComponent<ScrollRect>(scrollView);
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            Image scrollBg = GetOrAddComponent<Image>(scrollView);
            scrollBg.color = Color.clear;

            LayoutElement scrollLE = GetOrAddComponent<LayoutElement>(scrollView);
            scrollLE.flexibleHeight = 1;

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
            vlg.spacing = 2;
            vlg.padding = new RectOffset(0, 0, 5, 10);
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Sample Rows
            for (int i = 0; i < 10; i++)
            {
                CreateLeaderboardRow(content, i + 1, $"Jugador{i + 1}", (1000 - i * 85));
            }
        }

        private static void CreateLeaderboardRow(GameObject parent, int rank, string playerName, int score)
        {
            GameObject row = FindOrCreateChild(parent, $"Row_{rank}");

            Color rowBg = rank <= 3 ? new Color(0.06f, 0.1f, 0.15f, 1f) : Color.clear;

            Image rowImage = GetOrAddComponent<Image>(row);
            rowImage.color = rowBg;

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(row);
            hlg.spacing = 10;
            hlg.padding = new RectOffset(20, 20, 8, 8);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            LayoutElement rowLE = GetOrAddComponent<LayoutElement>(row);
            rowLE.minHeight = 55;

            // Rank
            Color rankColor = rank == 1 ? GOLD : rank == 2 ? SILVER : rank == 3 ? BRONZE : TEXT_PRIMARY;

            GameObject rankObj = FindOrCreateChild(row, "Rank");
            TextMeshProUGUI rankText = GetOrAddComponent<TextMeshProUGUI>(rankObj);
            rankText.text = rank.ToString();
            rankText.fontSize = 20;
            rankText.fontStyle = rank <= 3 ? FontStyles.Bold : FontStyles.Normal;
            rankText.color = rankColor;
            rankText.alignment = TextAlignmentOptions.Center;
            LayoutElement rankLE = GetOrAddComponent<LayoutElement>(rankObj);
            rankLE.minWidth = 50;
            rankLE.preferredWidth = 50;

            // Avatar placeholder
            GameObject avatarObj = FindOrCreateChild(row, "Avatar");
            RectTransform avatarRT = GetOrAddComponent<RectTransform>(avatarObj);
            avatarRT.sizeDelta = new Vector2(40, 40);
            Image avatarImage = GetOrAddComponent<Image>(avatarObj);
            avatarImage.color = CYAN_DARK;
            LayoutElement avatarLE = GetOrAddComponent<LayoutElement>(avatarObj);
            avatarLE.minWidth = 40;
            avatarLE.minHeight = 40;

            // Player Name
            GameObject nameObj = FindOrCreateChild(row, "PlayerName");
            TextMeshProUGUI nameText = GetOrAddComponent<TextMeshProUGUI>(nameObj);
            nameText.text = playerName;
            nameText.fontSize = 18;
            nameText.fontStyle = rank <= 3 ? FontStyles.Bold : FontStyles.Normal;
            nameText.color = TEXT_PRIMARY;
            nameText.alignment = TextAlignmentOptions.Left;
            LayoutElement nameLE = GetOrAddComponent<LayoutElement>(nameObj);
            nameLE.flexibleWidth = 1;

            // Score
            GameObject scoreObj = FindOrCreateChild(row, "Score");
            TextMeshProUGUI scoreText = GetOrAddComponent<TextMeshProUGUI>(scoreObj);
            scoreText.text = score.ToString();
            scoreText.fontSize = 20;
            scoreText.fontStyle = FontStyles.Bold;
            scoreText.color = CYAN_NEON;
            scoreText.alignment = TextAlignmentOptions.Right;
            LayoutElement scoreLE = GetOrAddComponent<LayoutElement>(scoreObj);
            scoreLE.minWidth = 100;
            scoreLE.preferredWidth = 100;
        }

        // ==================== MY POSITION PANEL ====================

        private static void CreateMyPositionPanel(GameObject parent)
        {
            GameObject myPos = FindOrCreateChild(parent, "MyPositionPanel");

            RectTransform myPosRT = GetOrAddComponent<RectTransform>(myPos);
            myPosRT.anchorMin = new Vector2(0, 0);
            myPosRT.anchorMax = new Vector2(1, 0);
            myPosRT.pivot = new Vector2(0.5f, 0);
            myPosRT.anchoredPosition = new Vector2(0, ACTION_BUTTONS_HEIGHT + 20);
            myPosRT.sizeDelta = new Vector2(-CONTENT_PADDING * 2, MY_POSITION_HEIGHT);

            Image myPosBg = GetOrAddComponent<Image>(myPos);
            myPosBg.color = CYAN_DARK;
            AddOutline(myPos, CYAN_NEON, 2);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(myPos);
            hlg.spacing = 15;
            hlg.padding = new RectOffset(25, 25, 10, 10);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            // My Rank
            GameObject rankObj = FindOrCreateChild(myPos, "MyRank");
            TextMeshProUGUI rankText = GetOrAddComponent<TextMeshProUGUI>(rankObj);
            rankText.text = "#5";
            rankText.fontSize = 28;
            rankText.fontStyle = FontStyles.Bold;
            rankText.color = CYAN_NEON;
            rankText.alignment = TextAlignmentOptions.Center;
            LayoutElement rankLE = GetOrAddComponent<LayoutElement>(rankObj);
            rankLE.minWidth = 60;

            // My Avatar
            GameObject avatarObj = FindOrCreateChild(myPos, "MyAvatar");
            RectTransform avatarRT = GetOrAddComponent<RectTransform>(avatarObj);
            Image avatarImage = GetOrAddComponent<Image>(avatarObj);
            avatarImage.color = TEXT_PRIMARY;
            LayoutElement avatarLE = GetOrAddComponent<LayoutElement>(avatarObj);
            avatarLE.minWidth = 50;
            avatarLE.minHeight = 50;

            // My Name
            GameObject nameObj = FindOrCreateChild(myPos, "MyName");
            TextMeshProUGUI nameText = GetOrAddComponent<TextMeshProUGUI>(nameObj);
            nameText.text = "Tu";
            nameText.fontSize = 20;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = TEXT_PRIMARY;
            nameText.alignment = TextAlignmentOptions.Left;
            LayoutElement nameLE = GetOrAddComponent<LayoutElement>(nameObj);
            nameLE.flexibleWidth = 1;

            // My Score
            GameObject scoreObj = FindOrCreateChild(myPos, "MyScore");
            TextMeshProUGUI scoreText = GetOrAddComponent<TextMeshProUGUI>(scoreObj);
            scoreText.text = "745";
            scoreText.fontSize = 26;
            scoreText.fontStyle = FontStyles.Bold;
            scoreText.color = TEXT_PRIMARY;
            scoreText.alignment = TextAlignmentOptions.Right;
            LayoutElement scoreLE = GetOrAddComponent<LayoutElement>(scoreObj);
            scoreLE.minWidth = 100;

            Debug.Log("[TournamentLobbyUIBuilder] MyPositionPanel creado");
        }

        // ==================== ACTION BUTTONS ====================

        private static void CreateActionButtons(GameObject parent)
        {
            GameObject actions = FindOrCreateChild(parent, "ActionButtonsPanel");

            RectTransform actionsRT = GetOrAddComponent<RectTransform>(actions);
            actionsRT.anchorMin = new Vector2(0, 0);
            actionsRT.anchorMax = new Vector2(1, 0);
            actionsRT.pivot = new Vector2(0.5f, 0);
            actionsRT.anchoredPosition = new Vector2(0, CONTENT_PADDING);
            actionsRT.sizeDelta = new Vector2(-CONTENT_PADDING * 2, ACTION_BUTTONS_HEIGHT);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(actions);
            hlg.spacing = 15;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // Play Button (primary)
            GameObject playBtn = FindOrCreateChild(actions, "PlayButton");
            Image playBg = GetOrAddComponent<Image>(playBtn);
            playBg.color = BUTTON_PRIMARY;
            Button playButton = GetOrAddComponent<Button>(playBtn);
            SetupButtonColors(playButton, BUTTON_PRIMARY);
            AddOutline(playBtn, CYAN_GLOW, 3);
            LayoutElement playLE = GetOrAddComponent<LayoutElement>(playBtn);
            playLE.flexibleWidth = 2;

            GameObject playTextObj = FindOrCreateChild(playBtn, "Text");
            TextMeshProUGUI playText = GetOrAddComponent<TextMeshProUGUI>(playTextObj);
            playText.text = "JUGAR";
            playText.fontSize = 26;
            playText.fontStyle = FontStyles.Bold;
            playText.color = TEXT_DARK;
            playText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(playTextObj);

            // Share Button
            GameObject shareBtn = FindOrCreateChild(actions, "ShareButton");
            Image shareBg = GetOrAddComponent<Image>(shareBtn);
            shareBg.color = BUTTON_SECONDARY;
            Button shareButton = GetOrAddComponent<Button>(shareBtn);
            SetupButtonColors(shareButton, BUTTON_SECONDARY);
            AddOutline(shareBtn, CYAN_DARK);
            LayoutElement shareLE = GetOrAddComponent<LayoutElement>(shareBtn);
            shareLE.flexibleWidth = 1;

            GameObject shareTextObj = FindOrCreateChild(shareBtn, "Text");
            TextMeshProUGUI shareText = GetOrAddComponent<TextMeshProUGUI>(shareTextObj);
            shareText.text = "Compartir";
            shareText.fontSize = 16;
            shareText.fontStyle = FontStyles.Bold;
            shareText.color = TEXT_PRIMARY;
            shareText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(shareTextObj);

            // Leave Button
            GameObject leaveBtn = FindOrCreateChild(actions, "LeaveButton");
            Image leaveBg = GetOrAddComponent<Image>(leaveBtn);
            leaveBg.color = BUTTON_DANGER;
            Button leaveButton = GetOrAddComponent<Button>(leaveBtn);
            SetupButtonColors(leaveButton, BUTTON_DANGER);
            AddOutline(leaveBtn, new Color(1f, 0.3f, 0.3f, 0.5f));
            LayoutElement leaveLE = GetOrAddComponent<LayoutElement>(leaveBtn);
            leaveLE.flexibleWidth = 1;

            GameObject leaveTextObj = FindOrCreateChild(leaveBtn, "Text");
            TextMeshProUGUI leaveText = GetOrAddComponent<TextMeshProUGUI>(leaveTextObj);
            leaveText.text = "Salir";
            leaveText.fontSize = 16;
            leaveText.fontStyle = FontStyles.Bold;
            leaveText.color = TEXT_PRIMARY;
            leaveText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(leaveTextObj);

            Debug.Log("[TournamentLobbyUIBuilder] ActionButtons creados");
        }

        // ==================== PRIZES POPUP ====================

        private static void CreatePrizesPopup(Canvas canvas)
        {
            GameObject blocker = FindOrCreateChild(canvas.gameObject, "PrizesBlocker");
            blocker.SetActive(false);

            SetRectTransformStretch(blocker);
            Image blockerBg = GetOrAddComponent<Image>(blocker);
            blockerBg.color = BLOCKER_BG;
            Button blockerBtn = GetOrAddComponent<Button>(blocker);
            blockerBtn.transition = Selectable.Transition.None;
            blocker.transform.SetAsLastSibling();

            GameObject popup = FindOrCreateChild(blocker, "PrizesPopup");
            RectTransform popupRT = GetOrAddComponent<RectTransform>(popup);
            popupRT.anchorMin = new Vector2(0.5f, 0.5f);
            popupRT.anchorMax = new Vector2(0.5f, 0.5f);
            popupRT.sizeDelta = new Vector2(500, 450);

            Image popupBg = GetOrAddComponent<Image>(popup);
            popupBg.color = POPUP_BG;
            AddOutline(popup, GOLD, 2);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(popup);
            vlg.spacing = 20;
            vlg.padding = new RectOffset(30, 30, 25, 25);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Title
            GameObject titleObj = FindOrCreateChild(popup, "Title");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "PREMIOS";
            titleText.fontSize = 32;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = GOLD;
            titleText.alignment = TextAlignmentOptions.Center;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minHeight = 45;

            // Prize Rows
            CreatePrizeRow(popup, "1er Lugar", "$25", GOLD);
            CreatePrizeRow(popup, "2do Lugar", "$15", SILVER);
            CreatePrizeRow(popup, "3er Lugar", "$10", BRONZE);

            // Close Button
            GameObject closeBtn = FindOrCreateChild(popup, "CloseButton");
            Image closeBg = GetOrAddComponent<Image>(closeBtn);
            closeBg.color = BUTTON_SECONDARY;
            Button closeButton = GetOrAddComponent<Button>(closeBtn);
            SetupButtonColors(closeButton, BUTTON_SECONDARY);
            AddOutline(closeBtn, CYAN_DARK);
            LayoutElement closeLE = GetOrAddComponent<LayoutElement>(closeBtn);
            closeLE.minHeight = 50;

            GameObject closeTextObj = FindOrCreateChild(closeBtn, "Text");
            TextMeshProUGUI closeText = GetOrAddComponent<TextMeshProUGUI>(closeTextObj);
            closeText.text = "Cerrar";
            closeText.fontSize = 18;
            closeText.fontStyle = FontStyles.Bold;
            closeText.color = TEXT_PRIMARY;
            closeText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(closeTextObj);

            Debug.Log("[TournamentLobbyUIBuilder] PrizesPopup creado");
        }

        private static void CreatePrizeRow(GameObject parent, string place, string amount, Color color)
        {
            string safeName = place.Replace(" ", "").Replace(".", "");
            GameObject row = FindOrCreateChild(parent, $"Prize_{safeName}");

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(row);
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;

            LayoutElement rowLE = GetOrAddComponent<LayoutElement>(row);
            rowLE.minHeight = 50;

            // Place
            GameObject placeObj = FindOrCreateChild(row, "Place");
            TextMeshProUGUI placeText = GetOrAddComponent<TextMeshProUGUI>(placeObj);
            placeText.text = place;
            placeText.fontSize = 22;
            placeText.fontStyle = FontStyles.Bold;
            placeText.color = color;
            placeText.alignment = TextAlignmentOptions.Left;
            LayoutElement placeLE = GetOrAddComponent<LayoutElement>(placeObj);
            placeLE.flexibleWidth = 1;

            // Amount
            GameObject amountObj = FindOrCreateChild(row, "Amount");
            TextMeshProUGUI amountText = GetOrAddComponent<TextMeshProUGUI>(amountObj);
            amountText.text = amount;
            amountText.fontSize = 26;
            amountText.fontStyle = FontStyles.Bold;
            amountText.color = color;
            amountText.alignment = TextAlignmentOptions.Right;
            LayoutElement amountLE = GetOrAddComponent<LayoutElement>(amountObj);
            amountLE.minWidth = 100;
        }

        // ==================== LEAVE CONFIRM POPUP ====================

        private static void CreateLeaveConfirmPopup(Canvas canvas)
        {
            GameObject blocker = FindOrCreateChild(canvas.gameObject, "LeaveBlocker");
            blocker.SetActive(false);

            SetRectTransformStretch(blocker);
            Image blockerBg = GetOrAddComponent<Image>(blocker);
            blockerBg.color = BLOCKER_BG;
            Button blockerBtn = GetOrAddComponent<Button>(blocker);
            blockerBtn.transition = Selectable.Transition.None;
            blocker.transform.SetAsLastSibling();

            GameObject popup = FindOrCreateChild(blocker, "LeavePopup");
            RectTransform popupRT = GetOrAddComponent<RectTransform>(popup);
            popupRT.anchorMin = new Vector2(0.5f, 0.5f);
            popupRT.anchorMax = new Vector2(0.5f, 0.5f);
            popupRT.sizeDelta = new Vector2(450, 280);

            Image popupBg = GetOrAddComponent<Image>(popup);
            popupBg.color = POPUP_BG;
            AddOutline(popup, BUTTON_DANGER, 2);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(popup);
            vlg.spacing = 20;
            vlg.padding = new RectOffset(30, 30, 30, 30);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Title
            GameObject titleObj = FindOrCreateChild(popup, "Title");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "Abandonar Torneo?";
            titleText.fontSize = 26;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = BUTTON_DANGER;
            titleText.alignment = TextAlignmentOptions.Center;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minHeight = 40;

            // Message
            GameObject msgObj = FindOrCreateChild(popup, "Message");
            TextMeshProUGUI msgText = GetOrAddComponent<TextMeshProUGUI>(msgObj);
            msgText.text = "Perderas tu progreso y\nno podras recuperar tu entrada.";
            msgText.fontSize = 18;
            msgText.color = TEXT_PRIMARY;
            msgText.alignment = TextAlignmentOptions.Center;
            LayoutElement msgLE = GetOrAddComponent<LayoutElement>(msgObj);
            msgLE.minHeight = 60;

            // Buttons
            GameObject buttons = FindOrCreateChild(popup, "Buttons");
            HorizontalLayoutGroup btnHlg = GetOrAddComponent<HorizontalLayoutGroup>(buttons);
            btnHlg.spacing = 20;
            btnHlg.childControlWidth = true;
            btnHlg.childControlHeight = true;
            btnHlg.childForceExpandWidth = true;
            LayoutElement btnLE = GetOrAddComponent<LayoutElement>(buttons);
            btnLE.minHeight = 55;

            // Stay Button
            GameObject stayBtn = FindOrCreateChild(buttons, "StayButton");
            Image stayBg = GetOrAddComponent<Image>(stayBtn);
            stayBg.color = BUTTON_PRIMARY;
            Button stayButton = GetOrAddComponent<Button>(stayBtn);
            SetupButtonColors(stayButton, BUTTON_PRIMARY);
            AddOutline(stayBtn, CYAN_GLOW, 2);

            GameObject stayTextObj = FindOrCreateChild(stayBtn, "Text");
            TextMeshProUGUI stayText = GetOrAddComponent<TextMeshProUGUI>(stayTextObj);
            stayText.text = "Quedarme";
            stayText.fontSize = 18;
            stayText.fontStyle = FontStyles.Bold;
            stayText.color = TEXT_DARK;
            stayText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(stayTextObj);

            // Leave Button
            GameObject leaveBtn = FindOrCreateChild(buttons, "ConfirmLeaveButton");
            Image leaveBg = GetOrAddComponent<Image>(leaveBtn);
            leaveBg.color = BUTTON_DANGER;
            Button leaveButton = GetOrAddComponent<Button>(leaveBtn);
            SetupButtonColors(leaveButton, BUTTON_DANGER);

            GameObject leaveTextObj = FindOrCreateChild(leaveBtn, "Text");
            TextMeshProUGUI leaveText = GetOrAddComponent<TextMeshProUGUI>(leaveTextObj);
            leaveText.text = "Abandonar";
            leaveText.fontSize = 18;
            leaveText.fontStyle = FontStyles.Bold;
            leaveText.color = TEXT_PRIMARY;
            leaveText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(leaveTextObj);

            Debug.Log("[TournamentLobbyUIBuilder] LeaveConfirmPopup creado");
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
