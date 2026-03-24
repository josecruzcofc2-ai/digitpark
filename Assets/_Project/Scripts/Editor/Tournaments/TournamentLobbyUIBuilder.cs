using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;
using DigitPark.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// Construye la UI completa de TournamentLobby (rediseno profesional V2).
    /// Layout: Header, InfoCard, TabBar (Participants/Chat), ContentArea, ActionBar,
    /// plus PrizesPopup, LeaveConfirmPopup, LoadingOverlay, StartingOverlay.
    /// </summary>
    public class TournamentLobbyUIBuilder : EditorWindow
    {
        // ==================== COLORES DEL TEMA NEON ====================
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.4f, 1f);
        private static readonly Color CYAN_GLOW = new Color(0f, 1f, 1f, 0.3f);

        private static readonly Color DARK_BG = new Color(0.02f, 0.04f, 0.08f, 1f);
        private static readonly Color PANEL_BG = new Color(0.08f, 0.12f, 0.18f, 0.98f);
        private static readonly Color INFO_CARD_BG = new Color(0.06f, 0.06f, 0.12f, 0.95f);
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
        private static readonly Color TAB_ACTIVE = CYAN_NEON;
        private static readonly Color TAB_INACTIVE = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        private static readonly Color BADGE_RED = new Color(0.9f, 0.2f, 0.2f, 1f);

        private const string BACK_BUTTON_PREFAB = "Assets/_Project/Prefabs/Common/BackButton.prefab";
        private const string TIMER_ICON_PATH = "Assets/_Project/Art/Icons/UI/TimerIcon.png";
        // Avatar icon removed — participants no longer display avatars

        // ==================== DIMENSIONES ====================
        private const float HEADER_HEIGHT = 100f;
        private const float INFO_CARD_HEIGHT = 240f;
        private const float TAB_BAR_HEIGHT = 60f;
        private const float ACTION_BAR_HEIGHT = 80f;
        private const float MY_POSITION_HEIGHT = 80f;
        private const float CHAT_INPUT_HEIGHT = 70f;
        private const float CONTENT_PADDING = 16f;
        private const float TAB_INDICATOR_HEIGHT = 3f;

        [MenuItem("DigitPark/Scenes/Build Scene/Tournaments/Lobby", false, 162)]
        public static void BuildUI()
        {
            if (!EditorUtility.DisplayDialog("TournamentLobby UI Builder",
                "Esto construira la UI completa de TournamentLobby.\nAsegurate de tener la escena TournamentLobby abierta.\n\nContinuar?",
                "Si", "No"))
                return;

            BuildCompleteUI();
        }

        /// <summary>Called by AllScenesBatchBuilder — no dialogs.</summary>
        public static void BuildSilent()
        {
            BuildCompleteUI();
        }

        private static void BuildCompleteUI()
        {
            Debug.Log("[TournamentLobbyUIBuilder] ========== INICIANDO CONSTRUCCION V2 ==========");

            Canvas canvas = SetupCanvas();
            if (canvas == null) return;

            // Full clean of canvas children (keep TransitionCanvas and EventSystem)
            CleanupOldElements(canvas.transform);

            CreateBackground(canvas);
            GameObject safeArea = CreateSafeArea(canvas);

            CreateHeader(safeArea);
            CreateInfoCard(safeArea);
            CreateTabBar(safeArea);
            CreateContentArea(safeArea);
            CreateActionBar(safeArea);

            CreatePrizesPopup(canvas);
            CreateLeaveConfirmPopup(canvas);
            CreateLoadingOverlay(canvas);
            CreateStartingOverlay(canvas);

            MarkSceneDirty();
            AutoAssigners.TournamentLobbyReferenceAssigner.RunAutoAssign();
            Debug.Log("[TournamentLobbyUIBuilder] ========== CONSTRUCCION V2 COMPLETADA ==========");
        }

        // ==================== CANVAS SETUP ====================

        private static Canvas SetupCanvas()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();

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

        /// <summary>
        /// Limpia UI creada por el builder de TODOS los Canvas raiz.
        /// En TransitionCanvas: elimina TODO excepto elementos de transicion.
        /// En Canvas principal: elimina Background, SafeArea, popups y overlays.
        /// </summary>
        private static void CleanupOldUI()
        {
            string[] transitionElements = { "FadeImage", "CircleWipeImage", "SlidePanel" };

            foreach (var canvas in Object.FindObjectsOfType<Canvas>(true))
            {
                if (canvas.transform.parent != null) continue;

                if (canvas.gameObject.name.Contains("Transition"))
                {
                    for (int i = canvas.transform.childCount - 1; i >= 0; i--)
                    {
                        Transform child = canvas.transform.GetChild(i);
                        bool isTransitionElement = false;
                        foreach (string te in transitionElements)
                        {
                            if (child.gameObject.name == te) { isTransitionElement = true; break; }
                        }
                        if (!isTransitionElement)
                        {
                            Debug.Log($"[TournamentLobbyUIBuilder] Limpiando '{child.gameObject.name}' de TransitionCanvas");
                            Object.DestroyImmediate(child.gameObject);
                        }
                    }
                }
                else
                {
                    string[] toClean = {
                        "Background", "SafeArea",
                        "PrizesBlocker", "LeaveBlocker",
                        "LoadingOverlay", "StartingOverlay"
                    };
                    foreach (string name in toClean)
                    {
                        Transform t = canvas.transform.Find(name);
                        if (t != null)
                        {
                            Debug.Log($"[TournamentLobbyUIBuilder] Limpiando '{name}' de {canvas.gameObject.name}");
                            Object.DestroyImmediate(t.gameObject);
                        }
                    }
                }
            }
        }

        private static void CleanupOldElements(Transform parent)
        {
            List<GameObject> toDestroy = new List<GameObject>();
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                string name = child.gameObject.name;
                if (name == "TransitionCanvas" || name == "EventSystem")
                    continue;
                toDestroy.Add(child.gameObject);
            }
            foreach (var go in toDestroy)
                DestroyImmediate(go);
        }

        // ==================== BACKGROUND & SAFE AREA ====================

        private static void CreateBackground(Canvas canvas)
        {
            GameObject bg = FindOrCreateChild(canvas.gameObject, "Background");

            RectTransform bgRT = GetOrAddComponent<RectTransform>(bg);
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;

            Image bgImage = GetOrAddComponent<Image>(bg);
            bgImage.color = Color.white; // ThemeApplier tints at runtime

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

        // ==================== HEADER (100px top) ====================

        private static void CreateHeader(GameObject parent)
        {
            GameObject header = FindOrCreateChild(parent, "Header");

            RectTransform headerRT = GetOrAddComponent<RectTransform>(header);
            headerRT.anchorMin = new Vector2(0, 1);
            headerRT.anchorMax = new Vector2(1, 1);
            headerRT.pivot = new Vector2(0.5f, 1);
            headerRT.anchoredPosition = new Vector2(0, -29); // consistent top margin across all scenes
            headerRT.sizeDelta = new Vector2(0, HEADER_HEIGHT);

            Image headerBg = GetOrAddComponent<Image>(header);
            headerBg.color = HEADER_BG;

            // BottomGlow removed — user request: no separator between header and InfoCard

            // ── BackButton (prefab) ──
            GameObject backBtnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BACK_BUTTON_PREFAB);
            GameObject backBtn;
            if (backBtnPrefab != null)
            {
                Transform oldBtn = header.transform.Find("BackButton");
                if (oldBtn != null) Object.DestroyImmediate(oldBtn.gameObject);
                backBtn = (GameObject)PrefabUtility.InstantiatePrefab(backBtnPrefab, header.transform);
                backBtn.name = "BackButton";
            }
            else
            {
                backBtn = FindOrCreateChild(header, "BackButton");
                Debug.LogWarning("[TournamentLobbyUIBuilder] BackButton prefab not found, using fallback");
            }

            RectTransform backRT = GetOrAddComponent<RectTransform>(backBtn);
            backRT.anchorMin = new Vector2(0, 0.5f);
            backRT.anchorMax = new Vector2(0, 0.5f);
            backRT.pivot = new Vector2(0, 0.5f);
            backRT.anchoredPosition = new Vector2(20, 0);
            backRT.sizeDelta = new Vector2(50, 50);

            // ── TournamentNameText (center, auto-size, cyan) ──
            GameObject titleObj = FindOrCreateChild(header, "TournamentNameText");
            RectTransform titleRT = GetOrAddComponent<RectTransform>(titleObj);
            titleRT.anchorMin = new Vector2(0, 0);
            titleRT.anchorMax = new Vector2(0.42f, 1);
            titleRT.offsetMin = new Vector2(80, 10);
            titleRT.offsetMax = new Vector2(0, -10);

            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "Tournament Name";
            titleText.fontSize = FontSizes.H4;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = CYAN_NEON;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = FontSizes.AutoMinBody;
            titleText.fontSizeMax = FontSizes.H4;
            titleText.overflowMode = TextOverflowModes.Ellipsis;
            AddOutline(titleObj, CYAN_GLOW, 2);

            // Currency pills (right side of header)
            var pills = CurrencyHeaderBarHelper.CreateCurrencyPills(header.transform);
            var pillsRT = pills.GetComponent<RectTransform>();
            pillsRT.anchorMin = new Vector2(0.52f, 0.5f);
            pillsRT.anchorMax = new Vector2(0.95f, 0.5f);
            pillsRT.pivot = new Vector2(0.5f, 0.5f);
            pillsRT.sizeDelta = new Vector2(0, 65);

            // StatusBadge and PrizesButton moved to InfoCard to avoid overlap with CurrencyPills

            Debug.Log("[TournamentLobbyUIBuilder] Header creado");
        }

        // ==================== INFO CARD (160px below header) ====================

        private static void CreateInfoCard(GameObject parent)
        {
            GameObject infoCard = FindOrCreateChild(parent, "InfoCard");

            RectTransform infoRT = GetOrAddComponent<RectTransform>(infoCard);
            infoRT.anchorMin = new Vector2(0, 1);
            infoRT.anchorMax = new Vector2(1, 1);
            infoRT.pivot = new Vector2(0.5f, 1);
            infoRT.anchoredPosition = new Vector2(0, -(HEADER_HEIGHT + 29f) - 6); // +29f accounts for header's own top offset
            infoRT.sizeDelta = new Vector2(-CONTENT_PADDING * 2, INFO_CARD_HEIGHT);

            Image infoBg = GetOrAddComponent<Image>(infoCard);
            infoBg.color = INFO_CARD_BG;
            AddOutline(infoCard, CYAN_DARK);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(infoCard);
            vlg.spacing = 4;
            vlg.padding = new RectOffset(16, 16, 8, 8);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // ── StatusRow: StatusBadge + PrizesButton ──
            CreateInfoCardStatusRow(infoCard);

            // ── TopRow: GameType + Countdown ──
            CreateInfoCardTopRow(infoCard);

            // ── MiddleRow: EntryFee + PrizePool ──
            CreateInfoCardMiddleRow(infoCard);

            // ── ProgressRow: player progress bar ──
            CreateInfoCardProgressRow(infoCard);

            // ── RulesRow: attempts + time limit ──
            CreateInfoCardRulesRow(infoCard);

            Debug.Log("[TournamentLobbyUIBuilder] InfoCard creado");
        }

        private static void CreateInfoCardStatusRow(GameObject parent)
        {
            GameObject statusRow = FindOrCreateChild(parent, "StatusRow");

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(statusRow);
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleRight;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;

            LayoutElement rowLE = GetOrAddComponent<LayoutElement>(statusRow);
            rowLE.minHeight = 36;

            // Spacer to push items right
            GameObject spacer = FindOrCreateChild(statusRow, "Spacer");
            LayoutElement spacerLE = GetOrAddComponent<LayoutElement>(spacer);
            spacerLE.flexibleWidth = 1;

            // StatusBadge
            GameObject statusBadge = FindOrCreateChild(statusRow, "StatusBadge");
            RectTransform statusBadgeRT = GetOrAddComponent<RectTransform>(statusBadge);
            statusBadgeRT.sizeDelta = new Vector2(120, 32);

            Image statusBadgeImage = GetOrAddComponent<Image>(statusBadge);
            statusBadgeImage.color = BUTTON_SUCCESS;

            LayoutElement sbLE = GetOrAddComponent<LayoutElement>(statusBadge);
            sbLE.minWidth = 120;
            sbLE.preferredWidth = 120;
            sbLE.minHeight = 32;
            sbLE.preferredHeight = 32;

            GameObject statusBadgeTextObj = FindOrCreateChild(statusBadge, "StatusBadgeText");
            SetRectTransformStretch(statusBadgeTextObj);
            TextMeshProUGUI statusBadgeTMP = GetOrAddComponent<TextMeshProUGUI>(statusBadgeTextObj);
            statusBadgeTMP.text = "OPEN";
            statusBadgeTMP.fontSize = FontSizes.Body;
            statusBadgeTMP.fontStyle = FontStyles.Bold;
            statusBadgeTMP.color = TEXT_DARK;
            statusBadgeTMP.alignment = TextAlignmentOptions.Center;
            statusBadgeTMP.enableAutoSizing = true;
            statusBadgeTMP.fontSizeMin = FontSizes.AutoMinBody;
            statusBadgeTMP.fontSizeMax = FontSizes.Body;

            // PrizesButton (gold diamond icon)
            GameObject prizesBtn = FindOrCreateChild(statusRow, "PrizesButton");
            RectTransform prizesRT = GetOrAddComponent<RectTransform>(prizesBtn);
            prizesRT.sizeDelta = new Vector2(46, 46);

            Image prizesBg = GetOrAddComponent<Image>(prizesBtn);
            prizesBg.color = GOLD;

            LayoutElement pbLE = GetOrAddComponent<LayoutElement>(prizesBtn);
            pbLE.minWidth = 46;
            pbLE.preferredWidth = 46;
            pbLE.minHeight = 32;
            pbLE.preferredHeight = 32;

            Button prizesButton = GetOrAddComponent<Button>(prizesBtn);
            SetupButtonColors(prizesButton, GOLD);
            AddOutline(prizesBtn, new Color(1f, 0.84f, 0f, 0.5f));

            GameObject prizesIconObj = FindOrCreateChild(prizesBtn, "Text");
            TextMeshProUGUI prizesText = GetOrAddComponent<TextMeshProUGUI>(prizesIconObj);
            prizesText.text = "\u2666"; // diamond symbol
            prizesText.fontSize = FontSizes.Body;
            prizesText.fontStyle = FontStyles.Bold;
            prizesText.color = TEXT_DARK;
            prizesText.alignment = TextAlignmentOptions.Center;
            prizesText.enableAutoSizing = true;
            prizesText.fontSizeMin = FontSizes.AutoMinBody;
            prizesText.fontSizeMax = FontSizes.Body;
            prizesText.overflowMode = TextOverflowModes.Ellipsis;
            SetRectTransformStretch(prizesIconObj);
        }

        private static void CreateInfoCardTopRow(GameObject parent)
        {
            GameObject topRow = FindOrCreateChild(parent, "TopRow");

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(topRow);
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            LayoutElement rowLE = GetOrAddComponent<LayoutElement>(topRow);
            rowLE.minHeight = 34;

            // GameTypeRow (icon + text)
            GameObject gameTypeRow = FindOrCreateChild(topRow, "GameTypeRow");
            HorizontalLayoutGroup gtHlg = GetOrAddComponent<HorizontalLayoutGroup>(gameTypeRow);
            gtHlg.spacing = 8;
            gtHlg.childAlignment = TextAnchor.MiddleLeft;
            gtHlg.childControlWidth = true;
            gtHlg.childControlHeight = true;
            gtHlg.childForceExpandWidth = false;
            LayoutElement gtLE = GetOrAddComponent<LayoutElement>(gameTypeRow);
            gtLE.flexibleWidth = 1;

            // Game icon placeholder
            GameObject gameIcon = FindOrCreateChild(gameTypeRow, "GameIcon");
            Image gameIconImg = GetOrAddComponent<Image>(gameIcon);
            gameIconImg.color = CYAN_DARK;
            LayoutElement giLE = GetOrAddComponent<LayoutElement>(gameIcon);
            giLE.minWidth = 28;
            giLE.preferredWidth = 28;
            giLE.minHeight = 28;
            giLE.preferredHeight = 28;

            // GameTypeText
            GameObject gameTypeTextObj = FindOrCreateChild(gameTypeRow, "GameTypeText");
            TextMeshProUGUI gameTypeTMP = GetOrAddComponent<TextMeshProUGUI>(gameTypeTextObj);
            gameTypeTMP.text = "Digit Rush";
            gameTypeTMP.fontSize = FontSizes.Body;
            gameTypeTMP.fontStyle = FontStyles.Bold;
            gameTypeTMP.color = TEXT_PRIMARY;
            gameTypeTMP.alignment = TextAlignmentOptions.Left;
            gameTypeTMP.enableAutoSizing = true;
            gameTypeTMP.fontSizeMin = FontSizes.AutoMinBody;
            gameTypeTMP.fontSizeMax = FontSizes.Body;
            gameTypeTMP.overflowMode = TextOverflowModes.Ellipsis;
            LayoutElement gtTextLE = GetOrAddComponent<LayoutElement>(gameTypeTextObj);
            gtTextLE.flexibleWidth = 1;
            gtTextLE.minHeight = 30;

            // TimeRow (clock icon + countdown)
            GameObject timeRow = FindOrCreateChild(topRow, "TimeRow");
            HorizontalLayoutGroup trHlg = GetOrAddComponent<HorizontalLayoutGroup>(timeRow);
            trHlg.spacing = 6;
            trHlg.childAlignment = TextAnchor.MiddleRight;
            trHlg.childControlWidth = true;
            trHlg.childControlHeight = true;
            trHlg.childForceExpandWidth = false;
            LayoutElement trLE = GetOrAddComponent<LayoutElement>(timeRow);
            trLE.flexibleWidth = 1;

            // Clock icon placeholder
            GameObject clockIcon = FindOrCreateChild(timeRow, "ClockIcon");
            Image clockIconImg = GetOrAddComponent<Image>(clockIcon);
            clockIconImg.color = TEXT_SECONDARY;
            LayoutElement ciLE = GetOrAddComponent<LayoutElement>(clockIcon);
            ciLE.minWidth = 24;
            ciLE.preferredWidth = 24;
            ciLE.minHeight = 24;
            ciLE.preferredHeight = 24;

            // CountdownText
            GameObject countdownObj = FindOrCreateChild(timeRow, "CountdownText");
            TextMeshProUGUI countdownTMP = GetOrAddComponent<TextMeshProUGUI>(countdownObj);
            countdownTMP.text = "02:45:30";
            countdownTMP.fontSize = FontSizes.Body;
            countdownTMP.fontStyle = FontStyles.Bold;
            countdownTMP.color = CYAN_NEON;
            countdownTMP.alignment = TextAlignmentOptions.Right;
            countdownTMP.enableAutoSizing = true;
            countdownTMP.fontSizeMin = FontSizes.AutoMinBody;
            countdownTMP.fontSizeMax = FontSizes.Body;
            countdownTMP.overflowMode = TextOverflowModes.Ellipsis;
            LayoutElement cdLE = GetOrAddComponent<LayoutElement>(countdownObj);
            cdLE.minWidth = 120;
            cdLE.minHeight = 30;
        }

        private static void CreateInfoCardMiddleRow(GameObject parent)
        {
            GameObject middleRow = FindOrCreateChild(parent, "MiddleRow");

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(middleRow);
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;

            LayoutElement rowLE = GetOrAddComponent<LayoutElement>(middleRow);
            rowLE.minHeight = 36;

            // EntryFeeGroup
            GameObject entryFeeGroup = FindOrCreateChild(middleRow, "EntryFeeGroup");
            HorizontalLayoutGroup efHlg = GetOrAddComponent<HorizontalLayoutGroup>(entryFeeGroup);
            efHlg.spacing = 6;
            efHlg.childAlignment = TextAnchor.MiddleCenter;
            efHlg.childControlWidth = true;
            efHlg.childControlHeight = true;
            efHlg.childForceExpandWidth = false;

            GameObject efIcon = FindOrCreateChild(entryFeeGroup, "Icon");
            Image efIconImg = GetOrAddComponent<Image>(efIcon);
            efIconImg.color = GOLD;
            LayoutElement efIconLE = GetOrAddComponent<LayoutElement>(efIcon);
            efIconLE.minWidth = 24;
            efIconLE.preferredWidth = 24;
            efIconLE.minHeight = 24;
            efIconLE.preferredHeight = 24;

            GameObject entryFeeTextObj = FindOrCreateChild(entryFeeGroup, "EntryFeeText");
            TextMeshProUGUI entryFeeTMP = GetOrAddComponent<TextMeshProUGUI>(entryFeeTextObj);
            entryFeeTMP.text = "FREE";
            entryFeeTMP.fontSize = FontSizes.Body;
            entryFeeTMP.fontStyle = FontStyles.Bold;
            entryFeeTMP.color = BUTTON_SUCCESS;
            entryFeeTMP.alignment = TextAlignmentOptions.Left;
            entryFeeTMP.enableAutoSizing = true;
            entryFeeTMP.fontSizeMin = FontSizes.AutoMinBody;
            entryFeeTMP.fontSizeMax = FontSizes.Body;
            entryFeeTMP.overflowMode = TextOverflowModes.Ellipsis;
            LayoutElement efTextLE = GetOrAddComponent<LayoutElement>(entryFeeTextObj);
            efTextLE.flexibleWidth = 1;
            efTextLE.minHeight = 30;

            // PrizePoolGroup
            GameObject prizePoolGroup = FindOrCreateChild(middleRow, "PrizePoolGroup");
            HorizontalLayoutGroup ppHlg = GetOrAddComponent<HorizontalLayoutGroup>(prizePoolGroup);
            ppHlg.spacing = 6;
            ppHlg.childAlignment = TextAnchor.MiddleCenter;
            ppHlg.childControlWidth = true;
            ppHlg.childControlHeight = true;
            ppHlg.childForceExpandWidth = false;

            GameObject ppIcon = FindOrCreateChild(prizePoolGroup, "Icon");
            Image ppIconImg = GetOrAddComponent<Image>(ppIcon);
            ppIconImg.color = GOLD;
            LayoutElement ppIconLE = GetOrAddComponent<LayoutElement>(ppIcon);
            ppIconLE.minWidth = 24;
            ppIconLE.preferredWidth = 24;
            ppIconLE.minHeight = 24;
            ppIconLE.preferredHeight = 24;

            GameObject prizePoolTextObj = FindOrCreateChild(prizePoolGroup, "PrizePoolText");
            TextMeshProUGUI prizePoolTMP = GetOrAddComponent<TextMeshProUGUI>(prizePoolTextObj);
            prizePoolTMP.text = "$50";
            prizePoolTMP.fontSize = FontSizes.Body;
            prizePoolTMP.fontStyle = FontStyles.Bold;
            prizePoolTMP.color = GOLD;
            prizePoolTMP.alignment = TextAlignmentOptions.Left;
            prizePoolTMP.enableAutoSizing = true;
            prizePoolTMP.fontSizeMin = FontSizes.AutoMinBody;
            prizePoolTMP.fontSizeMax = FontSizes.Body;
            prizePoolTMP.overflowMode = TextOverflowModes.Ellipsis;
            LayoutElement ppTextLE = GetOrAddComponent<LayoutElement>(prizePoolTextObj);
            ppTextLE.flexibleWidth = 1;
            ppTextLE.minHeight = 30;
        }

        private static void CreateInfoCardProgressRow(GameObject parent)
        {
            GameObject progressRow = FindOrCreateChild(parent, "ProgressRow");

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(progressRow);
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            LayoutElement rowLE = GetOrAddComponent<LayoutElement>(progressRow);
            rowLE.minHeight = 28;

            // Progress bar background
            GameObject barBgObj = FindOrCreateChild(progressRow, "PlayersProgressBarBg");
            Image barBgImage = GetOrAddComponent<Image>(barBgObj);
            barBgImage.color = new Color(0.1f, 0.1f, 0.15f, 1f);
            LayoutElement barBgLE = GetOrAddComponent<LayoutElement>(barBgObj);
            barBgLE.flexibleWidth = 1;
            barBgLE.minHeight = 18;
            barBgLE.preferredHeight = 18;

            // Progress bar fill (Image type=Filled) - named PlayersProgressBar
            GameObject barFillObj = FindOrCreateChild(barBgObj, "PlayersProgressBar");
            SetRectTransformStretch(barFillObj);
            Image barFillImage = GetOrAddComponent<Image>(barFillObj);
            barFillImage.color = new Color(0f, 0.9f, 0.8f, 1f); // cyan-green
            barFillImage.type = Image.Type.Filled;
            barFillImage.fillMethod = Image.FillMethod.Horizontal;
            barFillImage.fillAmount = 0.7f;

            // PlayersProgressText
            GameObject progressTextObj = FindOrCreateChild(progressRow, "PlayersProgressText");
            TextMeshProUGUI progressTMP = GetOrAddComponent<TextMeshProUGUI>(progressTextObj);
            progressTMP.text = "7/10";
            progressTMP.fontSize = FontSizes.Body;
            progressTMP.fontStyle = FontStyles.Bold;
            progressTMP.color = TEXT_PRIMARY;
            progressTMP.alignment = TextAlignmentOptions.Right;
            progressTMP.enableAutoSizing = true;
            progressTMP.fontSizeMin = FontSizes.AutoMinBody;
            progressTMP.fontSizeMax = FontSizes.Body;
            progressTMP.overflowMode = TextOverflowModes.Ellipsis;
            LayoutElement ptLE = GetOrAddComponent<LayoutElement>(progressTextObj);
            ptLE.minWidth = 60;
            ptLE.minHeight = 24;
        }

        private static void CreateInfoCardRulesRow(GameObject parent)
        {
            GameObject rulesRow = FindOrCreateChild(parent, "RulesRow");

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(rulesRow);
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;

            LayoutElement rowLE = GetOrAddComponent<LayoutElement>(rulesRow);
            rowLE.minHeight = 26;

            // AttemptsRuleText
            GameObject attemptsObj = FindOrCreateChild(rulesRow, "AttemptsRuleText");
            TextMeshProUGUI attemptsTMP = GetOrAddComponent<TextMeshProUGUI>(attemptsObj);
            attemptsTMP.text = "3 attempts";
            attemptsTMP.fontSize = FontSizes.Body;
            attemptsTMP.fontStyle = FontStyles.Bold;
            attemptsTMP.color = TEXT_SECONDARY;
            attemptsTMP.alignment = TextAlignmentOptions.Center;
            attemptsTMP.enableAutoSizing = true;
            attemptsTMP.fontSizeMin = FontSizes.AutoMinBody;
            attemptsTMP.fontSizeMax = FontSizes.Body;
            attemptsTMP.overflowMode = TextOverflowModes.Ellipsis;

            // TimeLimitRuleText
            GameObject timeLimitObj = FindOrCreateChild(rulesRow, "TimeLimitRuleText");
            TextMeshProUGUI timeLimitTMP = GetOrAddComponent<TextMeshProUGUI>(timeLimitObj);
            timeLimitTMP.text = "60s per round";
            timeLimitTMP.fontSize = FontSizes.Body;
            timeLimitTMP.fontStyle = FontStyles.Bold;
            timeLimitTMP.color = TEXT_SECONDARY;
            timeLimitTMP.alignment = TextAlignmentOptions.Center;
            timeLimitTMP.enableAutoSizing = true;
            timeLimitTMP.fontSizeMin = FontSizes.AutoMinBody;
            timeLimitTMP.fontSizeMax = FontSizes.Body;
            timeLimitTMP.overflowMode = TextOverflowModes.Ellipsis;
        }

        // ==================== TAB BAR (60px) ====================

        private static void CreateTabBar(GameObject parent)
        {
            float topOffset = HEADER_HEIGHT + 29f + INFO_CARD_HEIGHT + 12; // +29f: header top offset

            GameObject tabBar = FindOrCreateChild(parent, "TabBar");

            RectTransform tabBarRT = GetOrAddComponent<RectTransform>(tabBar);
            tabBarRT.anchorMin = new Vector2(0, 1);
            tabBarRT.anchorMax = new Vector2(1, 1);
            tabBarRT.pivot = new Vector2(0.5f, 1);
            tabBarRT.anchoredPosition = new Vector2(0, -topOffset);
            tabBarRT.sizeDelta = new Vector2(-CONTENT_PADDING * 2, TAB_BAR_HEIGHT);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(tabBar);
            hlg.spacing = 0;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // ── ParticipantsTab ──
            CreateTab(tabBar, "ParticipantsTab", "Participants", "ParticipantsTabIndicator", true);

            // ── ChatTab (with badge) ──
            CreateChatTab(tabBar);

            Debug.Log("[TournamentLobbyUIBuilder] TabBar creado");
        }

        private static void CreateTab(GameObject parent, string tabName, string label, string indicatorName, bool isActive)
        {
            GameObject tab = FindOrCreateChild(parent, tabName);

            Image tabBg = GetOrAddComponent<Image>(tab);
            tabBg.color = Color.clear;

            Button tabButton = GetOrAddComponent<Button>(tab);
            tabButton.transition = Selectable.Transition.None;

            // Tab text
            string textName = tabName + "Text";
            GameObject textObj = FindOrCreateChild(tab, textName);
            RectTransform textRT = GetOrAddComponent<RectTransform>(textObj);
            textRT.anchorMin = new Vector2(0, 0);
            textRT.anchorMax = new Vector2(1, 1);
            textRT.offsetMin = new Vector2(0, TAB_INDICATOR_HEIGHT);
            textRT.offsetMax = Vector2.zero;

            TextMeshProUGUI textTMP = GetOrAddComponent<TextMeshProUGUI>(textObj);
            textTMP.text = label;
            textTMP.fontSize = FontSizes.Body;
            textTMP.fontStyle = FontStyles.Bold;
            textTMP.color = isActive ? TEXT_PRIMARY : TEXT_SECONDARY;
            textTMP.alignment = TextAlignmentOptions.Center;
            textTMP.enableAutoSizing = true;
            textTMP.fontSizeMin = FontSizes.AutoMinBody;
            textTMP.fontSizeMax = FontSizes.Body;
            textTMP.overflowMode = TextOverflowModes.Ellipsis;

            // Indicator bar (bottom)
            GameObject indicator = FindOrCreateChild(tab, indicatorName);
            RectTransform indRT = GetOrAddComponent<RectTransform>(indicator);
            indRT.anchorMin = new Vector2(0, 0);
            indRT.anchorMax = new Vector2(1, 0);
            indRT.pivot = new Vector2(0.5f, 0);
            indRT.anchoredPosition = Vector2.zero;
            indRT.sizeDelta = new Vector2(0, TAB_INDICATOR_HEIGHT);

            Image indImage = GetOrAddComponent<Image>(indicator);
            indImage.color = isActive ? TAB_ACTIVE : TAB_INACTIVE;
        }

        private static void CreateChatTab(GameObject parent)
        {
            GameObject tab = FindOrCreateChild(parent, "ChatTab");

            Image tabBg = GetOrAddComponent<Image>(tab);
            tabBg.color = Color.clear;

            Button tabButton = GetOrAddComponent<Button>(tab);
            tabButton.transition = Selectable.Transition.None;

            // Tab text
            GameObject textObj = FindOrCreateChild(tab, "ChatTabText");
            RectTransform textRT = GetOrAddComponent<RectTransform>(textObj);
            textRT.anchorMin = new Vector2(0, 0);
            textRT.anchorMax = new Vector2(1, 1);
            textRT.offsetMin = new Vector2(0, TAB_INDICATOR_HEIGHT);
            textRT.offsetMax = Vector2.zero;

            TextMeshProUGUI textTMP = GetOrAddComponent<TextMeshProUGUI>(textObj);
            textTMP.text = "Chat";
            textTMP.fontSize = FontSizes.Body;
            textTMP.fontStyle = FontStyles.Bold;
            textTMP.color = TEXT_SECONDARY;
            textTMP.alignment = TextAlignmentOptions.Center;
            textTMP.enableAutoSizing = true;
            textTMP.fontSizeMin = FontSizes.AutoMinBody;
            textTMP.fontSizeMax = FontSizes.Body;
            textTMP.overflowMode = TextOverflowModes.Ellipsis;

            // ChatBadge (red circle, inactive by default)
            GameObject badge = FindOrCreateChild(tab, "ChatBadge");
            badge.SetActive(false);

            RectTransform badgeRT = GetOrAddComponent<RectTransform>(badge);
            badgeRT.anchorMin = new Vector2(1, 1);
            badgeRT.anchorMax = new Vector2(1, 1);
            badgeRT.pivot = new Vector2(0.5f, 0.5f);
            badgeRT.anchoredPosition = new Vector2(-30, -14);
            badgeRT.sizeDelta = new Vector2(28, 28);

            Image badgeImg = GetOrAddComponent<Image>(badge);
            badgeImg.color = BADGE_RED;

            GameObject badgeTextObj = FindOrCreateChild(badge, "ChatBadgeText");
            SetRectTransformStretch(badgeTextObj);
            TextMeshProUGUI badgeTMP = GetOrAddComponent<TextMeshProUGUI>(badgeTextObj);
            badgeTMP.text = "0";
            badgeTMP.fontSize = FontSizes.Body;
            badgeTMP.fontStyle = FontStyles.Bold;
            badgeTMP.color = Color.white;
            badgeTMP.alignment = TextAlignmentOptions.Center;
            badgeTMP.enableAutoSizing = true;
            badgeTMP.fontSizeMin = FontSizes.AutoMinBody;
            badgeTMP.fontSizeMax = FontSizes.Body;

            // Indicator bar (bottom)
            GameObject indicator = FindOrCreateChild(tab, "ChatTabIndicator");
            RectTransform indRT = GetOrAddComponent<RectTransform>(indicator);
            indRT.anchorMin = new Vector2(0, 0);
            indRT.anchorMax = new Vector2(1, 0);
            indRT.pivot = new Vector2(0.5f, 0);
            indRT.anchoredPosition = Vector2.zero;
            indRT.sizeDelta = new Vector2(0, TAB_INDICATOR_HEIGHT);

            Image indImage = GetOrAddComponent<Image>(indicator);
            indImage.color = TAB_INACTIVE;
        }

        // ==================== CONTENT AREA (fills remaining space) ====================

        private static void CreateContentArea(GameObject parent)
        {
            float topOffset = HEADER_HEIGHT + 29f + INFO_CARD_HEIGHT + TAB_BAR_HEIGHT + 18; // +29f: header top offset
            float bottomOffset = ACTION_BAR_HEIGHT + CONTENT_PADDING;

            GameObject contentArea = FindOrCreateChild(parent, "ContentArea");

            RectTransform caRT = GetOrAddComponent<RectTransform>(contentArea);
            caRT.anchorMin = Vector2.zero;
            caRT.anchorMax = Vector2.one;
            caRT.offsetMin = new Vector2(CONTENT_PADDING, bottomOffset);
            caRT.offsetMax = new Vector2(-CONTENT_PADDING, -topOffset);

            // ── ParticipantsContent (active by default) ──
            CreateParticipantsContent(contentArea);

            // ── ChatContent (inactive by default) ──
            CreateChatContent(contentArea);

            Debug.Log("[TournamentLobbyUIBuilder] ContentArea creado");
        }

        // ── Participants Content ──

        private static void CreateParticipantsContent(GameObject parent)
        {
            GameObject participantsContent = FindOrCreateChild(parent, "ParticipantsContent");
            participantsContent.SetActive(true);
            SetRectTransformStretch(participantsContent);

            // ── LeaderboardHeader (column titles) ──
            CreateLeaderboardHeader(participantsContent);

            // ── ParticipantsScrollView ──
            CreateParticipantsScrollView(participantsContent);

            // ── MyPositionPanel (fixed bottom, 80px) ──
            CreateMyPositionPanel(participantsContent);
        }

        private static void CreateLeaderboardHeader(GameObject parent)
        {
            GameObject header = FindOrCreateChild(parent, "LeaderboardHeader");

            RectTransform headerRT = GetOrAddComponent<RectTransform>(header);
            headerRT.anchorMin = new Vector2(0, 1);
            headerRT.anchorMax = new Vector2(1, 1);
            headerRT.pivot = new Vector2(0.5f, 1);
            headerRT.anchoredPosition = Vector2.zero;
            headerRT.sizeDelta = new Vector2(0, 40);

            Image headerBg = GetOrAddComponent<Image>(header);
            headerBg.color = new Color(0.03f, 0.06f, 0.1f, 0.8f);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(header);
            hlg.spacing = 8;
            hlg.padding = new RectOffset(16, 16, 4, 4);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            // # column
            GameObject rankCol = FindOrCreateChild(header, "RankCol");
            TextMeshProUGUI rankText = GetOrAddComponent<TextMeshProUGUI>(rankCol);
            rankText.text = "#";
            rankText.fontSize = FontSizes.Body;
            rankText.fontStyle = FontStyles.Bold;
            rankText.color = TEXT_SECONDARY;
            rankText.alignment = TextAlignmentOptions.Center;
            rankText.enableAutoSizing = true;
            rankText.fontSizeMin = FontSizes.AutoMinBody;
            rankText.fontSizeMax = FontSizes.Body;
            rankText.overflowMode = TextOverflowModes.Ellipsis;
            LayoutElement rankLE = GetOrAddComponent<LayoutElement>(rankCol);
            rankLE.minWidth = 50;
            rankLE.preferredWidth = 50;

            // Player column
            GameObject playerCol = FindOrCreateChild(header, "PlayerCol");
            TextMeshProUGUI playerText = GetOrAddComponent<TextMeshProUGUI>(playerCol);
            playerText.text = "Player";
            playerText.fontSize = FontSizes.Body;
            playerText.fontStyle = FontStyles.Bold;
            playerText.color = TEXT_SECONDARY;
            playerText.alignment = TextAlignmentOptions.Left;
            playerText.enableAutoSizing = true;
            playerText.fontSizeMin = FontSizes.AutoMinBody;
            playerText.fontSizeMax = FontSizes.Body;
            playerText.overflowMode = TextOverflowModes.Ellipsis;
            LayoutElement playerLE = GetOrAddComponent<LayoutElement>(playerCol);
            playerLE.flexibleWidth = 1;

            // Time column (icon + text)
            GameObject timeCol = FindOrCreateChild(header, "TimeCol");
            HorizontalLayoutGroup timeHlg = GetOrAddComponent<HorizontalLayoutGroup>(timeCol);
            timeHlg.spacing = 6;
            timeHlg.childAlignment = TextAnchor.MiddleRight;
            timeHlg.childControlWidth = false;
            timeHlg.childControlHeight = false;
            timeHlg.childForceExpandWidth = false;
            LayoutElement timeLE = GetOrAddComponent<LayoutElement>(timeCol);
            timeLE.minWidth = 230;
            timeLE.preferredWidth = 230;

            // Timer icon
            GameObject timerIconObj = FindOrCreateChild(timeCol, "TimerIcon");
            Image timerIcon = GetOrAddComponent<Image>(timerIconObj);
            timerIcon.color = TEXT_SECONDARY;
            timerIcon.preserveAspect = true;
            RectTransform timerIconRT = GetOrAddComponent<RectTransform>(timerIconObj);
            timerIconRT.sizeDelta = new Vector2(96, 96);
            Sprite timerSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TIMER_ICON_PATH);
            if (timerSprite != null) timerIcon.sprite = timerSprite;

            // "Tiempos" text
            GameObject timeLabelObj = FindOrCreateChild(timeCol, "TimeLabel");
            TextMeshProUGUI timeText = GetOrAddComponent<TextMeshProUGUI>(timeLabelObj);
            timeText.text = "Times";
            timeText.fontSize = FontSizes.Body;
            timeText.fontStyle = FontStyles.Bold;
            timeText.color = TEXT_SECONDARY;
            timeText.alignment = TextAlignmentOptions.Right;
            timeText.enableAutoSizing = true;
            timeText.fontSizeMin = FontSizes.AutoMinBody;
            timeText.fontSizeMax = FontSizes.Body;
            timeText.overflowMode = TextOverflowModes.Ellipsis;
            RectTransform timeLabelRT = GetOrAddComponent<RectTransform>(timeLabelObj);
            timeLabelRT.sizeDelta = new Vector2(130, 40);
        }

        private static void CreateParticipantsScrollView(GameObject parent)
        {
            GameObject scrollView = FindOrCreateChild(parent, "ParticipantsScrollView");

            RectTransform svRT = GetOrAddComponent<RectTransform>(scrollView);
            svRT.anchorMin = Vector2.zero;
            svRT.anchorMax = Vector2.one;
            svRT.offsetMin = new Vector2(0, MY_POSITION_HEIGHT + 4);
            svRT.offsetMax = new Vector2(0, -44); // below leaderboard header

            ScrollRect scrollRect = GetOrAddComponent<ScrollRect>(scrollView);
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 50f;

            Image scrollBg = GetOrAddComponent<Image>(scrollView);
            scrollBg.color = Color.clear;

            // Viewport
            GameObject viewport = FindOrCreateChild(scrollView, "Viewport");
            SetRectTransformStretch(viewport);
            GetOrAddComponent<RectMask2D>(viewport);
            RectTransform viewportRT = viewport.GetComponent<RectTransform>();
            scrollRect.viewport = viewportRT;

            // Content (this is participantsContainer)
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
            vlg.padding = new RectOffset(0, 0, 4, 8);
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // No sample rows - participants load at runtime from ParticipantItem prefab
        }

        // CreateLeaderboardRow removed - participants load at runtime from prefab

        private static void CreateMyPositionPanel(GameObject parent)
        {
            GameObject myPos = FindOrCreateChild(parent, "MyPositionPanel");

            RectTransform myPosRT = GetOrAddComponent<RectTransform>(myPos);
            myPosRT.anchorMin = new Vector2(0, 0);
            myPosRT.anchorMax = new Vector2(1, 0);
            myPosRT.pivot = new Vector2(0.5f, 0);
            myPosRT.anchoredPosition = new Vector2(0, 4); // 4px gap prevents Outline bleeding into ActionBar
            myPosRT.sizeDelta = new Vector2(0, MY_POSITION_HEIGHT);

            Image myPosBg = GetOrAddComponent<Image>(myPos);
            myPosBg.color = CYAN_DARK;
            AddOutline(myPos, CYAN_NEON, 2);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(myPos);
            hlg.spacing = 10;
            hlg.padding = new RectOffset(16, 16, 8, 8);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            // My Rank
            GameObject rankObj = FindOrCreateChild(myPos, "MyRank");
            TextMeshProUGUI rankText = GetOrAddComponent<TextMeshProUGUI>(rankObj);
            rankText.text = "#5";
            rankText.fontSize = FontSizes.BodyLarge;
            rankText.fontStyle = FontStyles.Bold;
            rankText.color = CYAN_NEON;
            rankText.alignment = TextAlignmentOptions.Center;
            rankText.enableAutoSizing = true;
            rankText.fontSizeMin = FontSizes.AutoMinBody;
            rankText.fontSizeMax = FontSizes.BodyLarge;
            rankText.overflowMode = TextOverflowModes.Ellipsis;
            LayoutElement rankLE = GetOrAddComponent<LayoutElement>(rankObj);
            rankLE.minWidth = 50;

            // My Name
            GameObject nameObj = FindOrCreateChild(myPos, "MyName");
            TextMeshProUGUI nameText = GetOrAddComponent<TextMeshProUGUI>(nameObj);
            nameText.text = "You";
            nameText.fontSize = FontSizes.Body;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = TEXT_PRIMARY;
            nameText.alignment = TextAlignmentOptions.Left;
            nameText.enableAutoSizing = true;
            nameText.fontSizeMin = FontSizes.AutoMinBody;
            nameText.fontSizeMax = FontSizes.Body;
            nameText.overflowMode = TextOverflowModes.Ellipsis;
            LayoutElement nameLE = GetOrAddComponent<LayoutElement>(nameObj);
            nameLE.flexibleWidth = 1;

            // My Time
            GameObject timeObj = FindOrCreateChild(myPos, "MyTime");
            TextMeshProUGUI myTimeText = GetOrAddComponent<TextMeshProUGUI>(timeObj);
            myTimeText.text = "-";
            myTimeText.fontSize = FontSizes.BodyLarge;
            myTimeText.fontStyle = FontStyles.Bold;
            myTimeText.color = TEXT_PRIMARY;
            myTimeText.alignment = TextAlignmentOptions.Right;
            myTimeText.enableAutoSizing = true;
            myTimeText.fontSizeMin = FontSizes.AutoMinBody;
            myTimeText.fontSizeMax = FontSizes.BodyLarge;
            myTimeText.overflowMode = TextOverflowModes.Ellipsis;
            LayoutElement timeLE = GetOrAddComponent<LayoutElement>(timeObj);
            timeLE.minWidth = 100;
        }

        // ── Chat Content ──

        private static void CreateChatContent(GameObject parent)
        {
            GameObject chatContent = FindOrCreateChild(parent, "ChatContent");
            chatContent.SetActive(false);
            SetRectTransformStretch(chatContent);

            // ── ChatScrollView ──
            CreateChatScrollView(chatContent);

            // ── ChatInputRow (70px bottom) ──
            CreateChatInputRow(chatContent);
        }

        private static void CreateChatScrollView(GameObject parent)
        {
            GameObject scrollView = FindOrCreateChild(parent, "ChatScrollView");

            RectTransform svRT = GetOrAddComponent<RectTransform>(scrollView);
            svRT.anchorMin = Vector2.zero;
            svRT.anchorMax = Vector2.one;
            svRT.offsetMin = new Vector2(0, CHAT_INPUT_HEIGHT + 4);
            svRT.offsetMax = Vector2.zero;

            ScrollRect scrollRect = GetOrAddComponent<ScrollRect>(scrollView);
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 50f;

            Image scrollBg = GetOrAddComponent<Image>(scrollView);
            scrollBg.color = Color.clear;

            // Viewport
            GameObject viewport = FindOrCreateChild(scrollView, "Viewport");
            SetRectTransformStretch(viewport);
            GetOrAddComponent<RectMask2D>(viewport);
            RectTransform viewportRT = viewport.GetComponent<RectTransform>();
            scrollRect.viewport = viewportRT;

            // ChatMessagesContainer
            GameObject messagesContainer = FindOrCreateChild(viewport, "ChatMessagesContainer");
            RectTransform mcRT = GetOrAddComponent<RectTransform>(messagesContainer);
            mcRT.anchorMin = new Vector2(0, 1);
            mcRT.anchorMax = new Vector2(1, 1);
            mcRT.pivot = new Vector2(0.5f, 1);
            mcRT.sizeDelta = new Vector2(0, 0);
            scrollRect.content = mcRT;

            ContentSizeFitter csf = GetOrAddComponent<ContentSizeFitter>(messagesContainer);
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(messagesContainer);
            vlg.spacing = 6;
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
        }

        private static void CreateChatInputRow(GameObject parent)
        {
            GameObject inputRow = FindOrCreateChild(parent, "ChatInputRow");

            RectTransform irRT = GetOrAddComponent<RectTransform>(inputRow);
            irRT.anchorMin = new Vector2(0, 0);
            irRT.anchorMax = new Vector2(1, 0);
            irRT.pivot = new Vector2(0.5f, 0);
            irRT.anchoredPosition = Vector2.zero;
            irRT.sizeDelta = new Vector2(0, CHAT_INPUT_HEIGHT);

            Image irBg = GetOrAddComponent<Image>(inputRow);
            irBg.color = new Color(0.04f, 0.06f, 0.1f, 1f);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(inputRow);
            hlg.spacing = 8;
            hlg.padding = new RectOffset(10, 10, 8, 8);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            // ── ChatInput (TMP_InputField) ──
            GameObject chatInputObj = FindOrCreateChild(inputRow, "ChatInput");
            Image chatInputBg = GetOrAddComponent<Image>(chatInputObj);
            chatInputBg.color = new Color(0.1f, 0.12f, 0.18f, 1f);
            AddOutline(chatInputObj, CYAN_DARK);
            LayoutElement ciLE = GetOrAddComponent<LayoutElement>(chatInputObj);
            ciLE.flexibleWidth = 1;
            ciLE.minHeight = 50;

            TMP_InputField inputField = GetOrAddComponent<TMP_InputField>(chatInputObj);

            // Text Area
            GameObject textArea = FindOrCreateChild(chatInputObj, "Text Area");
            SetRectTransformStretch(textArea);
            RectTransform textAreaRT = textArea.GetComponent<RectTransform>();
            textAreaRT.offsetMin = new Vector2(12, 4);
            textAreaRT.offsetMax = new Vector2(-12, -4);
            GetOrAddComponent<RectMask2D>(textArea);

            // Placeholder
            GameObject placeholderObj = FindOrCreateChild(textArea, "Placeholder");
            SetRectTransformStretch(placeholderObj);
            TextMeshProUGUI phText = GetOrAddComponent<TextMeshProUGUI>(placeholderObj);
            phText.text = "Type a message...";
            phText.fontSize = FontSizes.Body;
            phText.fontStyle = FontStyles.Bold;
            phText.color = TEXT_SECONDARY;
            phText.alignment = TextAlignmentOptions.MidlineLeft;
            phText.enableAutoSizing = true;
            phText.fontSizeMin = FontSizes.AutoMinBody;
            phText.fontSizeMax = FontSizes.Body;
            phText.overflowMode = TextOverflowModes.Ellipsis;

            // Input text
            GameObject textObj = FindOrCreateChild(textArea, "Text");
            SetRectTransformStretch(textObj);
            TextMeshProUGUI inputText = GetOrAddComponent<TextMeshProUGUI>(textObj);
            inputText.text = "";
            inputText.fontSize = FontSizes.Body;
            inputText.fontStyle = FontStyles.Bold;
            inputText.color = TEXT_PRIMARY;
            inputText.alignment = TextAlignmentOptions.MidlineLeft;
            inputText.enableAutoSizing = true;
            inputText.fontSizeMin = FontSizes.AutoMinBody;
            inputText.fontSizeMax = FontSizes.Body;
            inputText.overflowMode = TextOverflowModes.Ellipsis;

            // Wire TMP_InputField references
            inputField.textViewport = textAreaRT;
            inputField.textComponent = inputText;
            inputField.placeholder = phText;
            inputField.caretColor = CYAN_NEON;
            inputField.selectionColor = new Color(0f, 1f, 1f, 0.25f);

            // ── SendChatButton ──
            GameObject sendBtn = FindOrCreateChild(inputRow, "SendChatButton");
            Image sendBg = GetOrAddComponent<Image>(sendBtn);
            sendBg.color = BUTTON_PRIMARY;

            Button sendButton = GetOrAddComponent<Button>(sendBtn);
            SetupButtonColors(sendButton, BUTTON_PRIMARY);

            LayoutElement sendLE = GetOrAddComponent<LayoutElement>(sendBtn);
            sendLE.minWidth = 54;
            sendLE.preferredWidth = 54;
            sendLE.minHeight = 50;

            GameObject sendTextObj = FindOrCreateChild(sendBtn, "Text");
            SetRectTransformStretch(sendTextObj);
            TextMeshProUGUI sendText = GetOrAddComponent<TextMeshProUGUI>(sendTextObj);
            sendText.text = ">";
            sendText.fontSize = FontSizes.Body;
            sendText.fontStyle = FontStyles.Bold;
            sendText.color = TEXT_DARK;
            sendText.alignment = TextAlignmentOptions.Center;
            sendText.enableAutoSizing = true;
            sendText.fontSizeMin = FontSizes.AutoMinBody;
            sendText.fontSizeMax = FontSizes.Body;
            sendText.overflowMode = TextOverflowModes.Ellipsis;
        }

        // ==================== ACTION BAR (80px bottom) ====================

        private static void CreateActionBar(GameObject parent)
        {
            GameObject actionBar = FindOrCreateChild(parent, "ActionBar");

            RectTransform abRT = GetOrAddComponent<RectTransform>(actionBar);
            abRT.anchorMin = new Vector2(0, 0);
            abRT.anchorMax = new Vector2(1, 0);
            abRT.pivot = new Vector2(0.5f, 0);
            abRT.anchoredPosition = new Vector2(0, CONTENT_PADDING);
            abRT.sizeDelta = new Vector2(-CONTENT_PADDING * 2, ACTION_BAR_HEIGHT);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(actionBar);
            hlg.spacing = 10;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // ── JoinButton (primary, biggest share) ──
            GameObject joinBtn = FindOrCreateChild(actionBar, "JoinButton");
            Image joinBg = GetOrAddComponent<Image>(joinBtn);
            joinBg.color = BUTTON_PRIMARY;
            Button joinButton = GetOrAddComponent<Button>(joinBtn);
            SetupButtonColors(joinButton, BUTTON_PRIMARY);
            AddOutline(joinBtn, CYAN_GLOW, 3);
            LayoutElement joinLE = GetOrAddComponent<LayoutElement>(joinBtn);
            joinLE.flexibleWidth = 2;

            GameObject joinTextObj = FindOrCreateChild(joinBtn, "JoinButtonText");
            SetRectTransformStretch(joinTextObj);
            TextMeshProUGUI joinText = GetOrAddComponent<TextMeshProUGUI>(joinTextObj);
            joinText.text = "JOIN";
            joinText.fontSize = FontSizes.BodyLarge;
            joinText.fontStyle = FontStyles.Bold;
            joinText.color = TEXT_DARK;
            joinText.alignment = TextAlignmentOptions.Center;
            joinText.enableAutoSizing = true;
            joinText.fontSizeMin = FontSizes.AutoMinBody;
            joinText.fontSizeMax = FontSizes.BodyLarge;
            joinText.overflowMode = TextOverflowModes.Ellipsis;

            // ── ShareButton (secondary) ──
            GameObject shareBtn = FindOrCreateChild(actionBar, "ShareButton");
            Image shareBg = GetOrAddComponent<Image>(shareBtn);
            shareBg.color = BUTTON_SECONDARY;
            Button shareButton = GetOrAddComponent<Button>(shareBtn);
            SetupButtonColors(shareButton, BUTTON_SECONDARY);
            AddOutline(shareBtn, CYAN_DARK);
            LayoutElement shareLE = GetOrAddComponent<LayoutElement>(shareBtn);
            shareLE.flexibleWidth = 1;

            GameObject shareTextObj = FindOrCreateChild(shareBtn, "ShareButtonText");
            SetRectTransformStretch(shareTextObj);
            TextMeshProUGUI shareText = GetOrAddComponent<TextMeshProUGUI>(shareTextObj);
            shareText.text = "Share";
            shareText.fontSize = FontSizes.Body;
            shareText.fontStyle = FontStyles.Bold;
            shareText.color = TEXT_PRIMARY;
            shareText.alignment = TextAlignmentOptions.Center;
            shareText.enableAutoSizing = true;
            shareText.fontSizeMin = FontSizes.AutoMinBody;
            shareText.fontSizeMax = FontSizes.Body;
            shareText.overflowMode = TextOverflowModes.Ellipsis;

            // ── LeaveButton (danger/secondary) ──
            GameObject leaveBtn = FindOrCreateChild(actionBar, "LeaveButton");
            Image leaveBg = GetOrAddComponent<Image>(leaveBtn);
            leaveBg.color = BUTTON_DANGER;
            Button leaveButton = GetOrAddComponent<Button>(leaveBtn);
            SetupButtonColors(leaveButton, BUTTON_DANGER);
            AddOutline(leaveBtn, new Color(1f, 0.3f, 0.3f, 0.5f));
            LayoutElement leaveLE = GetOrAddComponent<LayoutElement>(leaveBtn);
            leaveLE.flexibleWidth = 1;

            GameObject leaveTextObj = FindOrCreateChild(leaveBtn, "LeaveButtonText");
            SetRectTransformStretch(leaveTextObj);
            TextMeshProUGUI leaveText = GetOrAddComponent<TextMeshProUGUI>(leaveTextObj);
            leaveText.text = "Leave";
            leaveText.fontSize = FontSizes.Body;
            leaveText.fontStyle = FontStyles.Bold;
            leaveText.color = TEXT_PRIMARY;
            leaveText.alignment = TextAlignmentOptions.Center;
            leaveText.enableAutoSizing = true;
            leaveText.fontSizeMin = FontSizes.AutoMinBody;
            leaveText.fontSizeMax = FontSizes.Body;
            leaveText.overflowMode = TextOverflowModes.Ellipsis;

            // ── StatusText (below action bar, informational) ──
            GameObject statusTextObj = FindOrCreateChild(parent, "StatusText");
            RectTransform stRT = GetOrAddComponent<RectTransform>(statusTextObj);
            stRT.anchorMin = new Vector2(0, 0);
            stRT.anchorMax = new Vector2(1, 0);
            stRT.pivot = new Vector2(0.5f, 0);
            stRT.anchoredPosition = new Vector2(0, 2);
            stRT.sizeDelta = new Vector2(-CONTENT_PADDING * 2, CONTENT_PADDING);

            TextMeshProUGUI statusTMP = GetOrAddComponent<TextMeshProUGUI>(statusTextObj);
            statusTMP.text = "";
            statusTMP.fontSize = FontSizes.Body;
            statusTMP.fontStyle = FontStyles.Bold;
            statusTMP.color = TEXT_SECONDARY;
            statusTMP.alignment = TextAlignmentOptions.Center;
            statusTMP.enableAutoSizing = true;
            statusTMP.fontSizeMin = FontSizes.AutoMinBody;
            statusTMP.fontSizeMax = FontSizes.Body;
            statusTMP.overflowMode = TextOverflowModes.Ellipsis;

            Debug.Log("[TournamentLobbyUIBuilder] ActionBar creado");
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
            popupRT.sizeDelta = new Vector2(520, 490);

            Image popupBg = GetOrAddComponent<Image>(popup);
            popupBg.color = POPUP_BG;
            AddOutline(popup, GOLD, 2);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(popup);
            vlg.spacing = 20;
            vlg.padding = new RectOffset(35, 35, 25, 35);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Title
            GameObject titleObj = FindOrCreateChild(popup, "PrizesPopupTitle");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "PRIZES";
            titleText.fontSize = FontSizes.H2;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = GOLD;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = FontSizes.AutoMinTitle;
            titleText.fontSizeMax = FontSizes.H2;
            titleText.overflowMode = TextOverflowModes.Ellipsis;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minHeight = 45;

            // Prize Rows
            CreatePrizeRow(popup, "1st Place", "$25", GOLD, "FirstPlaceLabel");
            CreatePrizeRow(popup, "2nd Place", "$15", SILVER, "SecondPlaceLabel");
            CreatePrizeRow(popup, "3rd Place", "$10", BRONZE, "ThirdPlaceLabel");

            // Close Button
            GameObject closeBtn = FindOrCreateChild(popup, "CloseButton");
            Image closeBg = GetOrAddComponent<Image>(closeBtn);
            closeBg.color = BUTTON_SECONDARY;
            Button closeButton = GetOrAddComponent<Button>(closeBtn);
            SetupButtonColors(closeButton, BUTTON_SECONDARY);
            AddOutline(closeBtn, CYAN_DARK);
            LayoutElement closeLE = GetOrAddComponent<LayoutElement>(closeBtn);
            closeLE.minHeight = 50;

            GameObject closeTextObj = FindOrCreateChild(closeBtn, "PrizesCloseText");
            TextMeshProUGUI closeText = GetOrAddComponent<TextMeshProUGUI>(closeTextObj);
            closeText.text = "Close";
            closeText.fontSize = FontSizes.Subtitle;
            closeText.fontStyle = FontStyles.Bold;
            closeText.color = TEXT_PRIMARY;
            closeText.alignment = TextAlignmentOptions.Center;
            closeText.enableAutoSizing = true;
            closeText.fontSizeMin = FontSizes.AutoMinBody;
            closeText.fontSizeMax = FontSizes.Subtitle;
            closeText.overflowMode = TextOverflowModes.Ellipsis;
            SetRectTransformStretch(closeTextObj);

            Debug.Log("[TournamentLobbyUIBuilder] PrizesPopup creado");
        }

        private static void CreatePrizeRow(GameObject parent, string place, string amount, Color color, string placeLabelName = "Place")
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
            GameObject placeObj = FindOrCreateChild(row, placeLabelName);
            TextMeshProUGUI placeText = GetOrAddComponent<TextMeshProUGUI>(placeObj);
            placeText.text = place;
            placeText.fontSize = FontSizes.H3;
            placeText.fontStyle = FontStyles.Bold;
            placeText.color = color;
            placeText.alignment = TextAlignmentOptions.Left;
            placeText.enableAutoSizing = true;
            placeText.fontSizeMin = FontSizes.AutoMinBody;
            placeText.fontSizeMax = FontSizes.H3;
            placeText.overflowMode = TextOverflowModes.Ellipsis;
            LayoutElement placeLE = GetOrAddComponent<LayoutElement>(placeObj);
            placeLE.flexibleWidth = 1;

            // Amount
            GameObject amountObj = FindOrCreateChild(row, "Amount");
            TextMeshProUGUI amountText = GetOrAddComponent<TextMeshProUGUI>(amountObj);
            amountText.text = amount;
            amountText.fontSize = FontSizes.H3;
            amountText.fontStyle = FontStyles.Bold;
            amountText.color = color;
            amountText.alignment = TextAlignmentOptions.Right;
            amountText.enableAutoSizing = true;
            amountText.fontSizeMin = FontSizes.AutoMinBody;
            amountText.fontSizeMax = FontSizes.H3;
            amountText.overflowMode = TextOverflowModes.Ellipsis;
            LayoutElement amountLE = GetOrAddComponent<LayoutElement>(amountObj);
            amountLE.minWidth = 120;
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
            popupRT.sizeDelta = new Vector2(510, 300);

            Image popupBg = GetOrAddComponent<Image>(popup);
            popupBg.color = POPUP_BG;
            AddOutline(popup, BUTTON_DANGER, 2);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(popup);
            vlg.spacing = 20;
            vlg.padding = new RectOffset(40, 40, 30, 30);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Title
            GameObject titleObj = FindOrCreateChild(popup, "LeavePopupTitle");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "Leave Tournament?";
            titleText.fontSize = FontSizes.H3;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = BUTTON_DANGER;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = FontSizes.AutoMinTitle;
            titleText.fontSizeMax = FontSizes.H3;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minHeight = 40;

            // Message
            GameObject msgObj = FindOrCreateChild(popup, "LeavePopupMessage");
            TextMeshProUGUI msgText = GetOrAddComponent<TextMeshProUGUI>(msgObj);
            msgText.text = "You will lose your progress and\nyour entry fee will not be refunded.";
            msgText.fontSize = FontSizes.Subtitle;
            msgText.fontStyle = FontStyles.Bold;
            msgText.color = TEXT_PRIMARY;
            msgText.alignment = TextAlignmentOptions.Center;
            msgText.enableAutoSizing = true;
            msgText.fontSizeMin = FontSizes.AutoMinBody;
            msgText.fontSizeMax = FontSizes.Subtitle;
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

            GameObject stayTextObj = FindOrCreateChild(stayBtn, "StayButtonText");
            TextMeshProUGUI stayText = GetOrAddComponent<TextMeshProUGUI>(stayTextObj);
            stayText.text = "Stay";
            stayText.fontSize = FontSizes.Subtitle;
            stayText.fontStyle = FontStyles.Bold;
            stayText.color = TEXT_DARK;
            stayText.alignment = TextAlignmentOptions.Center;
            stayText.enableAutoSizing = true;
            stayText.fontSizeMin = FontSizes.AutoMinBody;
            stayText.fontSizeMax = FontSizes.Subtitle;
            stayText.overflowMode = TextOverflowModes.Ellipsis;
            SetRectTransformStretch(stayTextObj);

            // Leave Button
            GameObject leaveBtn = FindOrCreateChild(buttons, "ConfirmLeaveButton");
            Image leaveBg = GetOrAddComponent<Image>(leaveBtn);
            leaveBg.color = BUTTON_DANGER;
            Button leaveButton = GetOrAddComponent<Button>(leaveBtn);
            SetupButtonColors(leaveButton, BUTTON_DANGER);

            GameObject leaveTextObj = FindOrCreateChild(leaveBtn, "LeaveConfirmButtonText");
            TextMeshProUGUI leaveText = GetOrAddComponent<TextMeshProUGUI>(leaveTextObj);
            leaveText.text = "Leave";
            leaveText.fontSize = FontSizes.Subtitle;
            leaveText.fontStyle = FontStyles.Bold;
            leaveText.color = TEXT_PRIMARY;
            leaveText.alignment = TextAlignmentOptions.Center;
            leaveText.enableAutoSizing = true;
            leaveText.fontSizeMin = FontSizes.AutoMinBody;
            leaveText.fontSizeMax = FontSizes.Subtitle;
            leaveText.overflowMode = TextOverflowModes.Ellipsis;
            SetRectTransformStretch(leaveTextObj);

            Debug.Log("[TournamentLobbyUIBuilder] LeaveConfirmPopup creado");
        }

        // ==================== LOADING OVERLAY ====================

        private static void CreateLoadingOverlay(Canvas canvas)
        {
            GameObject overlay = FindOrCreateChild(canvas.gameObject, "LoadingOverlay");
            overlay.SetActive(false);
            SetRectTransformStretch(overlay);
            overlay.transform.SetAsLastSibling();

            Image overlayBg = GetOrAddComponent<Image>(overlay);
            overlayBg.color = new Color(0f, 0f, 0f, 0.75f);

            // Center container
            GameObject center = FindOrCreateChild(overlay, "Center");
            RectTransform centerRT = GetOrAddComponent<RectTransform>(center);
            centerRT.anchorMin = new Vector2(0.2f, 0.4f);
            centerRT.anchorMax = new Vector2(0.8f, 0.6f);
            centerRT.offsetMin = Vector2.zero;
            centerRT.offsetMax = Vector2.zero;

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(center);
            vlg.spacing = 20;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Spinner
            GameObject spinner = FindOrCreateChild(center, "Spinner");
            Image spinnerImg = GetOrAddComponent<Image>(spinner);
            spinnerImg.color = CYAN_NEON;
            AddOutline(spinner, CYAN_GLOW, 3);
            LayoutElement spinLE = GetOrAddComponent<LayoutElement>(spinner);
            spinLE.minWidth = 60;
            spinLE.preferredWidth = 60;
            spinLE.minHeight = 60;
            spinLE.preferredHeight = 60;

            // StatusText inside loading
            GameObject loadingText = FindOrCreateChild(center, "LoadingText");
            TextMeshProUGUI loadingTMP = GetOrAddComponent<TextMeshProUGUI>(loadingText);
            loadingTMP.text = "Loading...";
            loadingTMP.fontSize = FontSizes.Body;
            loadingTMP.fontStyle = FontStyles.Bold;
            loadingTMP.color = TEXT_PRIMARY;
            loadingTMP.alignment = TextAlignmentOptions.Center;
            loadingTMP.enableAutoSizing = true;
            loadingTMP.fontSizeMin = FontSizes.AutoMinBody;
            loadingTMP.fontSizeMax = FontSizes.Body;
            loadingTMP.overflowMode = TextOverflowModes.Ellipsis;
            LayoutElement ltLE = GetOrAddComponent<LayoutElement>(loadingText);
            ltLE.minHeight = 30;

            Debug.Log("[TournamentLobbyUIBuilder] LoadingOverlay creado");
        }

        // ==================== STARTING OVERLAY ====================

        private static void CreateStartingOverlay(Canvas canvas)
        {
            GameObject overlay = FindOrCreateChild(canvas.gameObject, "StartingOverlay");
            overlay.SetActive(false);
            SetRectTransformStretch(overlay);
            overlay.transform.SetAsLastSibling();

            Image overlayBg = GetOrAddComponent<Image>(overlay);
            overlayBg.color = new Color(0f, 0f, 0f, 0.85f);

            // Big countdown number
            GameObject countdownObj = FindOrCreateChild(overlay, "StartingCountdownText");
            RectTransform cdRT = GetOrAddComponent<RectTransform>(countdownObj);
            cdRT.anchorMin = new Vector2(0.5f, 0.5f);
            cdRT.anchorMax = new Vector2(0.5f, 0.5f);
            cdRT.sizeDelta = new Vector2(400, 300);

            TextMeshProUGUI countdownTMP = GetOrAddComponent<TextMeshProUGUI>(countdownObj);
            countdownTMP.text = "3";
            countdownTMP.fontSize = FontSizes.Logo;
            countdownTMP.fontStyle = FontStyles.Bold;
            countdownTMP.color = CYAN_NEON;
            countdownTMP.alignment = TextAlignmentOptions.Center;
            countdownTMP.enableAutoSizing = true;
            countdownTMP.fontSizeMin = FontSizes.H1;
            countdownTMP.fontSizeMax = FontSizes.Logo;
            AddOutline(countdownObj, CYAN_GLOW, 4);

            // Subtitle
            GameObject subtitleObj = FindOrCreateChild(overlay, "StartingSubtitle");
            RectTransform subRT = GetOrAddComponent<RectTransform>(subtitleObj);
            subRT.anchorMin = new Vector2(0.5f, 0.5f);
            subRT.anchorMax = new Vector2(0.5f, 0.5f);
            subRT.anchoredPosition = new Vector2(0, -180);
            subRT.sizeDelta = new Vector2(600, 60);

            TextMeshProUGUI subtitleTMP = GetOrAddComponent<TextMeshProUGUI>(subtitleObj);
            subtitleTMP.text = "Tournament starts in...";
            subtitleTMP.fontSize = FontSizes.H4;
            subtitleTMP.fontStyle = FontStyles.Bold;
            subtitleTMP.color = TEXT_PRIMARY;
            subtitleTMP.alignment = TextAlignmentOptions.Center;
            subtitleTMP.enableAutoSizing = true;
            subtitleTMP.fontSizeMin = FontSizes.AutoMinBody;
            subtitleTMP.fontSizeMax = FontSizes.H4;
            subtitleTMP.overflowMode = TextOverflowModes.Ellipsis;

            Debug.Log("[TournamentLobbyUIBuilder] StartingOverlay creado");
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
