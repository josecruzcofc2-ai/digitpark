using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Construye la UI del sistema de Misiones.
    /// - Misiones Diarias (4 aleatorias, reset cada 24h)
    /// - Misiones Semanales (5 aleatorias, reset cada lunes)
    /// - Misiones de Temporada (15 fijas, duran 60 días)
    /// </summary>
    public class MissionsUIBuilder : EditorWindow
    {
        // ==================== COLORES DEL TEMA ====================
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.4f, 1f);
        private static readonly Color CYAN_GLOW = new Color(0f, 1f, 1f, 0.3f);

        private static readonly Color DARK_BG = new Color(0.02f, 0.04f, 0.08f, 1f);
        private static readonly Color PANEL_BG = new Color(0.05f, 0.08f, 0.12f, 0.98f);
        private static readonly Color CARD_BG = new Color(0.04f, 0.07f, 0.11f, 1f);
        private static readonly Color HEADER_BG = new Color(0.03f, 0.05f, 0.08f, 0.95f);

        private static readonly Color TEXT_PRIMARY = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.7f, 0.75f, 1f);
        private static readonly Color TEXT_DARK = new Color(0.02f, 0.04f, 0.08f, 1f);

        private static readonly Color BUTTON_SECONDARY = new Color(0.12f, 0.16f, 0.22f, 1f);
        private static readonly Color BUTTON_SUCCESS = new Color(0.2f, 0.8f, 0.4f, 1f);
        private static readonly Color BUTTON_CLAIM = new Color(0.2f, 0.8f, 0.4f, 1f);

        // Mission Period Colors
        private static readonly Color DAILY_COLOR = new Color(0.3f, 0.8f, 1f, 1f);      // Light Blue
        private static readonly Color WEEKLY_COLOR = new Color(0.8f, 0.5f, 1f, 1f);     // Purple
        private static readonly Color SEASON_COLOR = new Color(1f, 0.7f, 0.2f, 1f);     // Gold/Orange

        // Reward Colors
        private static readonly Color XP_COLOR = new Color(0.4f, 0.8f, 0.4f, 1f);       // Green
        private static readonly Color COIN_COLOR = new Color(1f, 0.84f, 0f, 1f);        // Gold
        private static readonly Color GEM_COLOR = new Color(0.4f, 0.7f, 1f, 1f);        // Blue

        private static readonly Color BLOCKER_BG = new Color(0f, 0f, 0f, 0.9f);

        // ==================== DIMENSIONES ====================
        private const float HEADER_HEIGHT = 120f;
        private const float SUMMARY_HEIGHT = 100f;
        private const float TABS_HEIGHT = 55f;
        private const float TIMER_HEIGHT = 40f;
        private const float MISSION_CARD_HEIGHT = 140f;
        private const float CONTENT_PADDING = 15f;

        [MenuItem("DigitPark/UI Builders/Monetization/Missions", false, 186)]
        public static void BuildUI()
        {
            if (!EditorUtility.DisplayDialog("Missions UI Builder",
                "Esto construirá la UI del sistema de Misiones.\n" +
                "Asegúrate de tener la escena DailyMissions abierta.\n\n" +
                "Incluye:\n" +
                "- Panel de resumen con progreso\n" +
                "- Pestañas: Diarias / Semanales / Temporada\n" +
                "- Temporizador de reinicio\n" +
                "- Cards de misiones con recompensas\n" +
                "- Panel de recompensa reclamada\n\n" +
                "¿Continuar?",
                "Sí", "No"))
                return;

            BuildMissionsUI();
        }

        private static void BuildMissionsUI()
        {
            Debug.Log("[MissionsUI] ========== INICIANDO CONSTRUCCIÓN ==========");

            Canvas canvas = SetupCanvas();
            if (canvas == null) return;

            CleanupOldUI(canvas);

            CreateBackground(canvas);
            GameObject safeArea = CreateSafeArea(canvas);

            CreateHeader(safeArea);
            CreateSummaryPanel(safeArea);
            CreatePeriodTabs(safeArea);
            CreateResetTimer(safeArea);
            CreateMissionsScrollView(safeArea);

            CreateClaimAllButton(safeArea);
            CreateRewardPopup(canvas);

            CreateMissionCardPrefab();

            MarkSceneDirty();
            Debug.Log("[MissionsUI] ========== CONSTRUCCIÓN COMPLETADA ==========");

            EditorUtility.DisplayDialog("Misiones UI Completada",
                "UI del sistema de Misiones creada exitosamente.\n\n" +
                "Elementos creados:\n" +
                "- Header con monedas/gemas\n" +
                "- Panel de resumen\n" +
                "- 3 pestañas (Diarias/Semanales/Temporada)\n" +
                "- Temporizador de reinicio\n" +
                "- 4 misiones de ejemplo\n" +
                "- Botón Reclamar Todo\n" +
                "- Popup de recompensa\n" +
                "- Prefab MissionCard\n\n" +
                "Asigna el MissionsUIManager y conecta las referencias.",
                "OK");
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

        private static void CleanupOldUI(Canvas canvas)
        {
            Debug.Log("[MissionsUI] Limpiando UI antigua...");

            string[] oldElementsToRemove = new string[]
            {
                "SafeArea",
                "Background",
                "Header",
                "SummaryPanel",
                "PeriodTabs",
                "ResetTimer",
                "MissionsScrollView",
                "ClaimAllButton",
                "RewardPopup"
            };

            var childrenToDestroy = new System.Collections.Generic.List<GameObject>();

            foreach (Transform child in canvas.transform)
            {
                foreach (string oldName in oldElementsToRemove)
                {
                    if (child.name == oldName || child.name.StartsWith(oldName))
                    {
                        childrenToDestroy.Add(child.gameObject);
                        break;
                    }
                }
            }

            foreach (var child in childrenToDestroy)
            {
                Debug.Log($"[MissionsUI] Eliminando: {child.name}");
                Object.DestroyImmediate(child);
            }

            Debug.Log("[MissionsUI] Limpieza completada");
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

            CreateBottomGlow(header, CYAN_NEON);

            // BackButton
            GameObject backBtn = FindOrCreateChild(header, "BackButton");
            RectTransform backRT = GetOrAddComponent<RectTransform>(backBtn);
            backRT.anchorMin = new Vector2(0, 0.5f);
            backRT.anchorMax = new Vector2(0, 0.5f);
            backRT.pivot = new Vector2(0, 0.5f);
            backRT.anchoredPosition = new Vector2(20, 10);
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
            GameObject titleObj = FindOrCreateChild(header, "Title");
            RectTransform titleRT = GetOrAddComponent<RectTransform>(titleObj);
            titleRT.anchorMin = new Vector2(0.5f, 0.5f);
            titleRT.anchorMax = new Vector2(0.5f, 0.5f);
            titleRT.anchoredPosition = new Vector2(0, 10);
            titleRT.sizeDelta = new Vector2(300, 50);

            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "MISIONES";
            titleText.fontSize = 36;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = CYAN_NEON;
            titleText.alignment = TextAlignmentOptions.Center;

            // Currency Display
            CreateCurrencyDisplay(header);

            Debug.Log("[MissionsUI] Header creado");
        }

        private static void CreateCurrencyDisplay(GameObject header)
        {
            GameObject currencyRow = FindOrCreateChild(header, "CurrencyRow");
            RectTransform currencyRT = GetOrAddComponent<RectTransform>(currencyRow);
            currencyRT.anchorMin = new Vector2(0, 0);
            currencyRT.anchorMax = new Vector2(1, 0);
            currencyRT.pivot = new Vector2(0.5f, 0);
            currencyRT.anchoredPosition = new Vector2(0, 15);
            currencyRT.sizeDelta = new Vector2(-40, 40);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(currencyRow);
            hlg.spacing = 20f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;

            // Coins
            CreateCurrencyItem(currencyRow, "Coins", "5,430", COIN_COLOR);
            // Gems
            CreateCurrencyItem(currencyRow, "Gems", "125", GEM_COLOR);
        }

        private static void CreateCurrencyItem(GameObject parent, string name, string amount, Color color)
        {
            GameObject item = FindOrCreateChild(parent, name);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(item);
            hlg.spacing = 8f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;

            LayoutElement itemLE = GetOrAddComponent<LayoutElement>(item);
            itemLE.minWidth = 120;
            itemLE.minHeight = 40;

            // Icon
            GameObject icon = FindOrCreateChild(item, "Icon");
            Image iconImage = GetOrAddComponent<Image>(icon);
            iconImage.color = color;
            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(icon);
            iconLE.minWidth = 30;
            iconLE.minHeight = 30;

            // Amount
            GameObject amountObj = FindOrCreateChild(item, "Amount");
            TextMeshProUGUI amountText = GetOrAddComponent<TextMeshProUGUI>(amountObj);
            amountText.text = amount;
            amountText.fontSize = 22;
            amountText.fontStyle = FontStyles.Bold;
            amountText.color = color;
            amountText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement amountLE = GetOrAddComponent<LayoutElement>(amountObj);
            amountLE.minWidth = 80;
        }

        // ==================== SUMMARY PANEL ====================

        private static void CreateSummaryPanel(GameObject parent)
        {
            GameObject summary = FindOrCreateChild(parent, "SummaryPanel");

            RectTransform summaryRT = GetOrAddComponent<RectTransform>(summary);
            summaryRT.anchorMin = new Vector2(0, 1);
            summaryRT.anchorMax = new Vector2(1, 1);
            summaryRT.pivot = new Vector2(0.5f, 1);
            summaryRT.anchoredPosition = new Vector2(0, -HEADER_HEIGHT - 10);
            summaryRT.sizeDelta = new Vector2(-30, SUMMARY_HEIGHT);

            Image summaryBg = GetOrAddComponent<Image>(summary);
            summaryBg.color = PANEL_BG;
            AddOutline(summary, CYAN_DARK, 1);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(summary);
            vlg.spacing = 8f;
            vlg.padding = new RectOffset(20, 20, 15, 15);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            // Title Row
            GameObject titleRow = FindOrCreateChild(summary, "TitleRow");
            HorizontalLayoutGroup titleHlg = GetOrAddComponent<HorizontalLayoutGroup>(titleRow);
            titleHlg.childAlignment = TextAnchor.MiddleCenter;
            titleHlg.childControlWidth = true;
            titleHlg.childControlHeight = true;
            titleHlg.childForceExpandWidth = true;
            LayoutElement titleRowLE = GetOrAddComponent<LayoutElement>(titleRow);
            titleRowLE.minHeight = 25;

            GameObject titleLeft = FindOrCreateChild(titleRow, "Left");
            TextMeshProUGUI leftText = GetOrAddComponent<TextMeshProUGUI>(titleLeft);
            leftText.text = "RESUMEN DE HOY";
            leftText.fontSize = 16;
            leftText.fontStyle = FontStyles.Bold;
            leftText.color = CYAN_NEON;
            leftText.alignment = TextAlignmentOptions.MidlineLeft;

            GameObject titleRight = FindOrCreateChild(titleRow, "Right");
            TextMeshProUGUI rightText = GetOrAddComponent<TextMeshProUGUI>(titleRight);
            rightText.text = "Reclamar: +275 XP";
            rightText.fontSize = 14;
            rightText.fontStyle = FontStyles.Bold;
            rightText.color = XP_COLOR;
            rightText.alignment = TextAlignmentOptions.MidlineRight;

            // Progress Stats Row
            GameObject statsRow = FindOrCreateChild(summary, "StatsRow");
            HorizontalLayoutGroup statsHlg = GetOrAddComponent<HorizontalLayoutGroup>(statsRow);
            statsHlg.spacing = 15f;
            statsHlg.childAlignment = TextAnchor.MiddleCenter;
            statsHlg.childControlWidth = true;
            statsHlg.childControlHeight = true;
            statsHlg.childForceExpandWidth = true;
            LayoutElement statsRowLE = GetOrAddComponent<LayoutElement>(statsRow);
            statsRowLE.minHeight = 22;

            CreateStatItem(statsRow, "DailyStat", "Diarias:", "2/4", DAILY_COLOR);
            CreateStatItem(statsRow, "WeeklyStat", "Semanales:", "1/5", WEEKLY_COLOR);
            CreateStatItem(statsRow, "SeasonStat", "Temporada:", "3/15", SEASON_COLOR);

            // Overall Progress Bar
            GameObject progressBar = FindOrCreateChild(summary, "OverallProgressBar");
            LayoutElement progressLE = GetOrAddComponent<LayoutElement>(progressBar);
            progressLE.minHeight = 18;

            Image progressBg = GetOrAddComponent<Image>(progressBar);
            progressBg.color = new Color(0.08f, 0.1f, 0.14f, 1f);

            GameObject progressFill = FindOrCreateChild(progressBar, "Fill");
            RectTransform fillRT = GetOrAddComponent<RectTransform>(progressFill);
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = new Vector2(0.6f, 1);
            fillRT.sizeDelta = Vector2.zero;

            Image fillImage = GetOrAddComponent<Image>(progressFill);
            fillImage.color = CYAN_NEON;

            // Progress percentage text
            GameObject progressText = FindOrCreateChild(progressBar, "Text");
            SetRectTransformStretch(progressText);
            TextMeshProUGUI progressTmp = GetOrAddComponent<TextMeshProUGUI>(progressText);
            progressTmp.text = "60%";
            progressTmp.fontSize = 12;
            progressTmp.fontStyle = FontStyles.Bold;
            progressTmp.color = TEXT_DARK;
            progressTmp.alignment = TextAlignmentOptions.Center;

            Debug.Log("[MissionsUI] SummaryPanel creado");
        }

        private static void CreateStatItem(GameObject parent, string name, string label, string value, Color color)
        {
            GameObject stat = FindOrCreateChild(parent, name);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(stat);
            hlg.spacing = 5f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;

            GameObject labelObj = FindOrCreateChild(stat, "Label");
            TextMeshProUGUI labelText = GetOrAddComponent<TextMeshProUGUI>(labelObj);
            labelText.text = label;
            labelText.fontSize = 13;
            labelText.color = TEXT_SECONDARY;
            labelText.alignment = TextAlignmentOptions.MidlineRight;
            LayoutElement labelLE = GetOrAddComponent<LayoutElement>(labelObj);
            labelLE.minWidth = 85;

            GameObject valueObj = FindOrCreateChild(stat, "Value");
            TextMeshProUGUI valueText = GetOrAddComponent<TextMeshProUGUI>(valueObj);
            valueText.text = value;
            valueText.fontSize = 14;
            valueText.fontStyle = FontStyles.Bold;
            valueText.color = color;
            valueText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement valueLE = GetOrAddComponent<LayoutElement>(valueObj);
            valueLE.minWidth = 40;
        }

        // ==================== PERIOD TABS ====================

        private static void CreatePeriodTabs(GameObject parent)
        {
            float topOffset = HEADER_HEIGHT + SUMMARY_HEIGHT + 25;

            GameObject tabs = FindOrCreateChild(parent, "PeriodTabs");

            RectTransform tabsRT = GetOrAddComponent<RectTransform>(tabs);
            tabsRT.anchorMin = new Vector2(0, 1);
            tabsRT.anchorMax = new Vector2(1, 1);
            tabsRT.pivot = new Vector2(0.5f, 1);
            tabsRT.anchoredPosition = new Vector2(0, -topOffset);
            tabsRT.sizeDelta = new Vector2(-30, TABS_HEIGHT);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(tabs);
            hlg.spacing = 10f;
            hlg.padding = new RectOffset(5, 5, 5, 5);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;

            CreatePeriodTab(tabs, "DailyTab", "DIARIAS", true, DAILY_COLOR);
            CreatePeriodTab(tabs, "WeeklyTab", "SEMANALES", false, WEEKLY_COLOR);
            CreatePeriodTab(tabs, "SeasonTab", "TEMPORADA", false, SEASON_COLOR);

            Debug.Log("[MissionsUI] PeriodTabs creado");
        }

        private static void CreatePeriodTab(GameObject parent, string name, string label, bool isActive, Color color)
        {
            GameObject tab = FindOrCreateChild(parent, name);

            Image tabBg = GetOrAddComponent<Image>(tab);
            tabBg.color = isActive ? color : BUTTON_SECONDARY;

            Button tabButton = GetOrAddComponent<Button>(tab);
            SetupButtonColors(tabButton, isActive ? color : BUTTON_SECONDARY);
            AddOutline(tab, isActive ? color : CYAN_DARK * 0.4f, isActive ? 2 : 1);

            GameObject textObj = FindOrCreateChild(tab, "Text");
            TextMeshProUGUI tabText = GetOrAddComponent<TextMeshProUGUI>(textObj);
            tabText.text = label;
            tabText.fontSize = 16;
            tabText.fontStyle = FontStyles.Bold;
            tabText.color = isActive ? TEXT_DARK : TEXT_PRIMARY;
            tabText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(textObj);

            // Badge for unclaimed count
            GameObject badge = FindOrCreateChild(tab, "Badge");
            RectTransform badgeRT = GetOrAddComponent<RectTransform>(badge);
            badgeRT.anchorMin = new Vector2(1, 1);
            badgeRT.anchorMax = new Vector2(1, 1);
            badgeRT.pivot = new Vector2(1, 1);
            badgeRT.anchoredPosition = new Vector2(5, 5);
            badgeRT.sizeDelta = new Vector2(24, 24);

            Image badgeBg = GetOrAddComponent<Image>(badge);
            badgeBg.color = BUTTON_SUCCESS;

            GameObject badgeText = FindOrCreateChild(badge, "Text");
            SetRectTransformStretch(badgeText);
            TextMeshProUGUI badgeTmp = GetOrAddComponent<TextMeshProUGUI>(badgeText);
            badgeTmp.text = isActive ? "2" : "0";
            badgeTmp.fontSize = 12;
            badgeTmp.fontStyle = FontStyles.Bold;
            badgeTmp.color = TEXT_DARK;
            badgeTmp.alignment = TextAlignmentOptions.Center;

            badge.SetActive(isActive); // Only show badge if there are unclaimed
        }

        // ==================== RESET TIMER ====================

        private static void CreateResetTimer(GameObject parent)
        {
            float topOffset = HEADER_HEIGHT + SUMMARY_HEIGHT + TABS_HEIGHT + 35;

            GameObject timer = FindOrCreateChild(parent, "ResetTimer");

            RectTransform timerRT = GetOrAddComponent<RectTransform>(timer);
            timerRT.anchorMin = new Vector2(0, 1);
            timerRT.anchorMax = new Vector2(1, 1);
            timerRT.pivot = new Vector2(0.5f, 1);
            timerRT.anchoredPosition = new Vector2(0, -topOffset);
            timerRT.sizeDelta = new Vector2(-30, TIMER_HEIGHT);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(timer);
            hlg.spacing = 10f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;

            // Clock Icon
            GameObject clockIcon = FindOrCreateChild(timer, "ClockIcon");
            Image clockImage = GetOrAddComponent<Image>(clockIcon);
            clockImage.color = DAILY_COLOR;
            LayoutElement clockLE = GetOrAddComponent<LayoutElement>(clockIcon);
            clockLE.minWidth = 24;
            clockLE.minHeight = 24;

            // Timer Label
            GameObject labelObj = FindOrCreateChild(timer, "Label");
            TextMeshProUGUI labelText = GetOrAddComponent<TextMeshProUGUI>(labelObj);
            labelText.text = "Reinicio en:";
            labelText.fontSize = 14;
            labelText.color = TEXT_SECONDARY;
            labelText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement labelLE = GetOrAddComponent<LayoutElement>(labelObj);
            labelLE.minWidth = 100;

            // Timer Value
            GameObject valueObj = FindOrCreateChild(timer, "Value");
            TextMeshProUGUI valueText = GetOrAddComponent<TextMeshProUGUI>(valueObj);
            valueText.text = "14h 32m 15s";
            valueText.fontSize = 18;
            valueText.fontStyle = FontStyles.Bold;
            valueText.color = DAILY_COLOR;
            valueText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement valueLE = GetOrAddComponent<LayoutElement>(valueObj);
            valueLE.minWidth = 150;

            Debug.Log("[MissionsUI] ResetTimer creado");
        }

        // ==================== MISSIONS SCROLL VIEW ====================

        private static void CreateMissionsScrollView(GameObject parent)
        {
            float topOffset = HEADER_HEIGHT + SUMMARY_HEIGHT + TABS_HEIGHT + TIMER_HEIGHT + 50;
            float bottomOffset = 100; // Space for Claim All button

            GameObject scrollView = FindOrCreateChild(parent, "MissionsScrollView");

            RectTransform scrollRT = GetOrAddComponent<RectTransform>(scrollView);
            scrollRT.anchorMin = Vector2.zero;
            scrollRT.anchorMax = Vector2.one;
            scrollRT.offsetMin = new Vector2(CONTENT_PADDING, bottomOffset);
            scrollRT.offsetMax = new Vector2(-CONTENT_PADDING, -topOffset);

            ScrollRect scrollRect = GetOrAddComponent<ScrollRect>(scrollView);
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.scrollSensitivity = 30f;

            Image scrollBg = GetOrAddComponent<Image>(scrollView);
            scrollBg.color = Color.clear;

            // Viewport
            GameObject viewport = FindOrCreateChild(scrollView, "Viewport");
            SetRectTransformStretch(viewport);
            GetOrAddComponent<RectMask2D>(viewport);
            scrollRect.viewport = viewport.GetComponent<RectTransform>();

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
            vlg.spacing = 12f;
            vlg.padding = new RectOffset(0, 0, 10, 20);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Create example mission cards
            CreateMissionCard(content, "Mission_1", "Jugador Activo", "Juega 3 partidas",
                3, 3, true, false, 50, 100, 0, DAILY_COLOR);
            CreateMissionCard(content, "Mission_2", "Especialista DigitRush", "Juega 2 partidas de DigitRush",
                1, 2, false, true, 50, 100, 0, DAILY_COLOR);
            CreateMissionCard(content, "Mission_3", "Precisión", "Completa una partida con 80%+ precisión",
                0, 1, false, false, 75, 150, 0, DAILY_COLOR);
            CreateMissionCard(content, "Mission_4", "Triple Victoria", "Gana 3 partidas",
                1, 3, false, true, 100, 200, 0, DAILY_COLOR);

            Debug.Log("[MissionsUI] MissionsScrollView creado con 4 misiones de ejemplo");
        }

        private static void CreateMissionCard(GameObject parent, string name, string title, string description,
            int current, int target, bool isCompleted, bool hasProgress, int xp, int coins, int gems, Color accentColor)
        {
            GameObject card = FindOrCreateChild(parent, name);

            LayoutElement cardLE = GetOrAddComponent<LayoutElement>(card);
            cardLE.minHeight = MISSION_CARD_HEIGHT;
            cardLE.preferredHeight = MISSION_CARD_HEIGHT;

            Image cardBg = GetOrAddComponent<Image>(card);
            cardBg.color = isCompleted ? new Color(0.08f, 0.15f, 0.1f, 1f) : CARD_BG;
            AddOutline(card, isCompleted ? BUTTON_SUCCESS : (hasProgress ? accentColor * 0.5f : CYAN_DARK * 0.3f), isCompleted ? 2 : 1);

            // Main Layout
            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(card);
            hlg.spacing = 15f;
            hlg.padding = new RectOffset(15, 15, 15, 15);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandHeight = true;

            // Left: Icon
            GameObject iconContainer = FindOrCreateChild(card, "IconContainer");
            LayoutElement iconContainerLE = GetOrAddComponent<LayoutElement>(iconContainer);
            iconContainerLE.minWidth = 60;
            iconContainerLE.minHeight = 60;

            Image iconBg = GetOrAddComponent<Image>(iconContainer);
            iconBg.color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.2f);
            AddOutline(iconContainer, accentColor * 0.5f);

            GameObject icon = FindOrCreateChild(iconContainer, "Icon");
            SetRectTransformStretch(icon);
            RectTransform iconRT = icon.GetComponent<RectTransform>();
            iconRT.offsetMin = new Vector2(12, 12);
            iconRT.offsetMax = new Vector2(-12, -12);
            Image iconImage = GetOrAddComponent<Image>(icon);
            iconImage.color = accentColor;

            // Center: Info
            GameObject infoSection = FindOrCreateChild(card, "InfoSection");
            LayoutElement infoLE = GetOrAddComponent<LayoutElement>(infoSection);
            infoLE.flexibleWidth = 1;

            VerticalLayoutGroup infoVlg = GetOrAddComponent<VerticalLayoutGroup>(infoSection);
            infoVlg.spacing = 6f;
            infoVlg.childAlignment = TextAnchor.UpperLeft;
            infoVlg.childControlWidth = true;
            infoVlg.childControlHeight = true;

            // Title
            GameObject titleObj = FindOrCreateChild(infoSection, "Title");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = title;
            titleText.fontSize = 18;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = isCompleted ? BUTTON_SUCCESS : TEXT_PRIMARY;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minHeight = 24;

            // Description
            GameObject descObj = FindOrCreateChild(infoSection, "Description");
            TextMeshProUGUI descText = GetOrAddComponent<TextMeshProUGUI>(descObj);
            descText.text = description;
            descText.fontSize = 13;
            descText.color = TEXT_SECONDARY;
            descText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement descLE = GetOrAddComponent<LayoutElement>(descObj);
            descLE.minHeight = 18;

            // Progress Bar
            GameObject progressContainer = FindOrCreateChild(infoSection, "ProgressContainer");
            LayoutElement progressContainerLE = GetOrAddComponent<LayoutElement>(progressContainer);
            progressContainerLE.minHeight = 20;

            HorizontalLayoutGroup progressHlg = GetOrAddComponent<HorizontalLayoutGroup>(progressContainer);
            progressHlg.spacing = 10f;
            progressHlg.childAlignment = TextAnchor.MiddleLeft;
            progressHlg.childControlWidth = true;
            progressHlg.childControlHeight = true;
            progressHlg.childForceExpandWidth = false;

            GameObject progressBar = FindOrCreateChild(progressContainer, "ProgressBar");
            LayoutElement progressBarLE = GetOrAddComponent<LayoutElement>(progressBar);
            progressBarLE.flexibleWidth = 1;
            progressBarLE.minHeight = 14;

            Image progressBg = GetOrAddComponent<Image>(progressBar);
            progressBg.color = new Color(0.1f, 0.12f, 0.15f, 1f);

            float progressPercent = target > 0 ? (float)current / target : 0;
            GameObject progressFill = FindOrCreateChild(progressBar, "Fill");
            RectTransform fillRT = GetOrAddComponent<RectTransform>(progressFill);
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = new Vector2(progressPercent, 1);
            fillRT.sizeDelta = Vector2.zero;

            Image fillImage = GetOrAddComponent<Image>(progressFill);
            fillImage.color = isCompleted ? BUTTON_SUCCESS : accentColor;

            GameObject progressText = FindOrCreateChild(progressContainer, "ProgressText");
            TextMeshProUGUI progressTmp = GetOrAddComponent<TextMeshProUGUI>(progressText);
            progressTmp.text = $"{current}/{target}";
            progressTmp.fontSize = 13;
            progressTmp.fontStyle = FontStyles.Bold;
            progressTmp.color = isCompleted ? BUTTON_SUCCESS : TEXT_SECONDARY;
            progressTmp.alignment = TextAlignmentOptions.MidlineRight;
            LayoutElement progressTextLE = GetOrAddComponent<LayoutElement>(progressText);
            progressTextLE.minWidth = 50;

            // Rewards Row
            GameObject rewardsRow = FindOrCreateChild(infoSection, "RewardsRow");
            LayoutElement rewardsLE = GetOrAddComponent<LayoutElement>(rewardsRow);
            rewardsLE.minHeight = 22;

            HorizontalLayoutGroup rewardsHlg = GetOrAddComponent<HorizontalLayoutGroup>(rewardsRow);
            rewardsHlg.spacing = 15f;
            rewardsHlg.childAlignment = TextAnchor.MiddleLeft;
            rewardsHlg.childControlWidth = false;
            rewardsHlg.childControlHeight = true;

            if (xp > 0) CreateRewardBadge(rewardsRow, "XP", $"+{xp} XP", XP_COLOR);
            if (coins > 0) CreateRewardBadge(rewardsRow, "Coins", $"+{coins}", COIN_COLOR);
            if (gems > 0) CreateRewardBadge(rewardsRow, "Gems", $"+{gems}", GEM_COLOR);

            // Right: Claim Button or Status
            GameObject rightSection = FindOrCreateChild(card, "RightSection");
            LayoutElement rightLE = GetOrAddComponent<LayoutElement>(rightSection);
            rightLE.minWidth = 100;

            if (isCompleted)
            {
                // Claim Button
                Image claimBg = GetOrAddComponent<Image>(rightSection);
                claimBg.color = BUTTON_CLAIM;

                Button claimBtn = GetOrAddComponent<Button>(rightSection);
                SetupButtonColors(claimBtn, BUTTON_CLAIM);

                GameObject claimText = FindOrCreateChild(rightSection, "Text");
                SetRectTransformStretch(claimText);
                TextMeshProUGUI claimTmp = GetOrAddComponent<TextMeshProUGUI>(claimText);
                claimTmp.text = "RECLAMAR";
                claimTmp.fontSize = 14;
                claimTmp.fontStyle = FontStyles.Bold;
                claimTmp.color = TEXT_DARK;
                claimTmp.alignment = TextAlignmentOptions.Center;
            }
            else
            {
                // Progress percentage
                GameObject percentText = FindOrCreateChild(rightSection, "Percent");
                SetRectTransformStretch(percentText);
                TextMeshProUGUI percentTmp = GetOrAddComponent<TextMeshProUGUI>(percentText);
                int percent = target > 0 ? (int)(100f * current / target) : 0;
                percentTmp.text = $"{percent}%";
                percentTmp.fontSize = 24;
                percentTmp.fontStyle = FontStyles.Bold;
                percentTmp.color = hasProgress ? accentColor : TEXT_SECONDARY;
                percentTmp.alignment = TextAlignmentOptions.Center;
            }

            // Completed checkmark overlay
            if (isCompleted)
            {
                GameObject checkOverlay = FindOrCreateChild(card, "CheckOverlay");
                RectTransform checkRT = GetOrAddComponent<RectTransform>(checkOverlay);
                checkRT.anchorMin = new Vector2(0, 0.5f);
                checkRT.anchorMax = new Vector2(0, 0.5f);
                checkRT.pivot = new Vector2(0, 0.5f);
                checkRT.anchoredPosition = new Vector2(-5, 0);
                checkRT.sizeDelta = new Vector2(30, 30);

                Image checkBg = GetOrAddComponent<Image>(checkOverlay);
                checkBg.color = BUTTON_SUCCESS;

                GameObject checkmark = FindOrCreateChild(checkOverlay, "Checkmark");
                SetRectTransformStretch(checkmark);
                TextMeshProUGUI checkText = GetOrAddComponent<TextMeshProUGUI>(checkmark);
                checkText.text = "✓";
                checkText.fontSize = 18;
                checkText.fontStyle = FontStyles.Bold;
                checkText.color = TEXT_DARK;
                checkText.alignment = TextAlignmentOptions.Center;
            }
        }

        private static void CreateRewardBadge(GameObject parent, string name, string text, Color color)
        {
            GameObject badge = FindOrCreateChild(parent, name);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(badge);
            hlg.spacing = 4f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;

            LayoutElement badgeLE = GetOrAddComponent<LayoutElement>(badge);
            badgeLE.minWidth = 70;

            GameObject icon = FindOrCreateChild(badge, "Icon");
            Image iconImage = GetOrAddComponent<Image>(icon);
            iconImage.color = color;
            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(icon);
            iconLE.minWidth = 16;
            iconLE.minHeight = 16;

            GameObject textObj = FindOrCreateChild(badge, "Text");
            TextMeshProUGUI textTmp = GetOrAddComponent<TextMeshProUGUI>(textObj);
            textTmp.text = text;
            textTmp.fontSize = 12;
            textTmp.fontStyle = FontStyles.Bold;
            textTmp.color = color;
            textTmp.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement textLE = GetOrAddComponent<LayoutElement>(textObj);
            textLE.minWidth = 50;
        }

        // ==================== CLAIM ALL BUTTON ====================

        private static void CreateClaimAllButton(GameObject parent)
        {
            GameObject claimAll = FindOrCreateChild(parent, "ClaimAllButton");

            RectTransform claimRT = GetOrAddComponent<RectTransform>(claimAll);
            claimRT.anchorMin = new Vector2(0, 0);
            claimRT.anchorMax = new Vector2(1, 0);
            claimRT.pivot = new Vector2(0.5f, 0);
            claimRT.anchoredPosition = new Vector2(0, 20);
            claimRT.sizeDelta = new Vector2(-40, 60);

            Image claimBg = GetOrAddComponent<Image>(claimAll);
            claimBg.color = BUTTON_CLAIM;

            Button claimBtn = GetOrAddComponent<Button>(claimAll);
            SetupButtonColors(claimBtn, BUTTON_CLAIM);
            AddOutline(claimAll, new Color(0.3f, 1f, 0.5f, 0.5f), 2);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(claimAll);
            hlg.spacing = 15f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;

            // Icon
            GameObject icon = FindOrCreateChild(claimAll, "Icon");
            Image iconImage = GetOrAddComponent<Image>(icon);
            iconImage.color = TEXT_DARK;
            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(icon);
            iconLE.minWidth = 30;
            iconLE.minHeight = 30;

            // Text
            GameObject textObj = FindOrCreateChild(claimAll, "Text");
            TextMeshProUGUI textTmp = GetOrAddComponent<TextMeshProUGUI>(textObj);
            textTmp.text = "RECLAMAR TODO";
            textTmp.fontSize = 20;
            textTmp.fontStyle = FontStyles.Bold;
            textTmp.color = TEXT_DARK;
            textTmp.alignment = TextAlignmentOptions.Center;
            LayoutElement textLE = GetOrAddComponent<LayoutElement>(textObj);
            textLE.minWidth = 200;

            // Reward preview
            GameObject rewardPreview = FindOrCreateChild(claimAll, "RewardPreview");
            TextMeshProUGUI rewardTmp = GetOrAddComponent<TextMeshProUGUI>(rewardPreview);
            rewardTmp.text = "(+150 XP, +300 🪙)";
            rewardTmp.fontSize = 14;
            rewardTmp.color = new Color(0f, 0.3f, 0.1f, 1f);
            rewardTmp.alignment = TextAlignmentOptions.Center;
            LayoutElement rewardLE = GetOrAddComponent<LayoutElement>(rewardPreview);
            rewardLE.minWidth = 150;

            Debug.Log("[MissionsUI] ClaimAllButton creado");
        }

        // ==================== REWARD POPUP ====================

        private static void CreateRewardPopup(Canvas canvas)
        {
            GameObject popup = FindOrCreateChild(canvas.gameObject, "RewardPopup");
            popup.SetActive(false);

            SetRectTransformStretch(popup);
            Image blockerBg = GetOrAddComponent<Image>(popup);
            blockerBg.color = BLOCKER_BG;
            popup.transform.SetAsLastSibling();

            // Center Panel
            GameObject panel = FindOrCreateChild(popup, "Panel");
            RectTransform panelRT = GetOrAddComponent<RectTransform>(panel);
            panelRT.anchorMin = new Vector2(0.5f, 0.5f);
            panelRT.anchorMax = new Vector2(0.5f, 0.5f);
            panelRT.sizeDelta = new Vector2(450, 380);

            Image panelBg = GetOrAddComponent<Image>(panel);
            panelBg.color = PANEL_BG;
            AddOutline(panel, BUTTON_SUCCESS, 2);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(panel);
            vlg.spacing = 20f;
            vlg.padding = new RectOffset(30, 30, 40, 30);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // Title
            GameObject titleObj = FindOrCreateChild(panel, "Title");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "¡RECOMPENSAS!";
            titleText.fontSize = 32;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = BUTTON_SUCCESS;
            titleText.alignment = TextAlignmentOptions.Center;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minHeight = 45;

            // Rewards Container
            GameObject rewardsContainer = FindOrCreateChild(panel, "RewardsContainer");
            VerticalLayoutGroup rewardsVlg = GetOrAddComponent<VerticalLayoutGroup>(rewardsContainer);
            rewardsVlg.spacing = 15f;
            rewardsVlg.childAlignment = TextAnchor.MiddleCenter;
            rewardsVlg.childControlWidth = true;
            rewardsVlg.childControlHeight = true;
            LayoutElement rewardsLE = GetOrAddComponent<LayoutElement>(rewardsContainer);
            rewardsLE.minHeight = 150;

            // XP Reward
            CreatePopupRewardRow(rewardsContainer, "XPReward", "+275 XP", XP_COLOR);
            // Coins Reward
            CreatePopupRewardRow(rewardsContainer, "CoinsReward", "+500 Monedas", COIN_COLOR);
            // Gems Reward
            CreatePopupRewardRow(rewardsContainer, "GemsReward", "+25 Gemas", GEM_COLOR);

            // Continue Button
            GameObject continueBtn = FindOrCreateChild(panel, "ContinueButton");
            Image continueBg = GetOrAddComponent<Image>(continueBtn);
            continueBg.color = CYAN_NEON;

            Button continueButton = GetOrAddComponent<Button>(continueBtn);
            SetupButtonColors(continueButton, CYAN_NEON);
            LayoutElement continueLE = GetOrAddComponent<LayoutElement>(continueBtn);
            continueLE.minHeight = 55;

            GameObject continueText = FindOrCreateChild(continueBtn, "Text");
            SetRectTransformStretch(continueText);
            TextMeshProUGUI continueTmp = GetOrAddComponent<TextMeshProUGUI>(continueText);
            continueTmp.text = "CONTINUAR";
            continueTmp.fontSize = 20;
            continueTmp.fontStyle = FontStyles.Bold;
            continueTmp.color = TEXT_DARK;
            continueTmp.alignment = TextAlignmentOptions.Center;

            Debug.Log("[MissionsUI] RewardPopup creado");
        }

        private static void CreatePopupRewardRow(GameObject parent, string name, string text, Color color)
        {
            GameObject row = FindOrCreateChild(parent, name);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(row);
            hlg.spacing = 15f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            LayoutElement rowLE = GetOrAddComponent<LayoutElement>(row);
            rowLE.minHeight = 40;

            GameObject icon = FindOrCreateChild(row, "Icon");
            Image iconImage = GetOrAddComponent<Image>(icon);
            iconImage.color = color;
            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(icon);
            iconLE.minWidth = 35;
            iconLE.minHeight = 35;

            GameObject textObj = FindOrCreateChild(row, "Text");
            TextMeshProUGUI textTmp = GetOrAddComponent<TextMeshProUGUI>(textObj);
            textTmp.text = text;
            textTmp.fontSize = 28;
            textTmp.fontStyle = FontStyles.Bold;
            textTmp.color = color;
            textTmp.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement textLE = GetOrAddComponent<LayoutElement>(textObj);
            textLE.minWidth = 200;
        }

        // ==================== PREFAB CREATION ====================

        private static void CreateMissionCardPrefab()
        {
            string prefabPath = "Assets/_Project/Prefabs/Monetization/MissionCard.prefab";

            string directory = System.IO.Path.GetDirectoryName(prefabPath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }

            GameObject prefabRoot = new GameObject("MissionCard");
            RectTransform rootRT = prefabRoot.AddComponent<RectTransform>();
            rootRT.sizeDelta = new Vector2(0, MISSION_CARD_HEIGHT);

            LayoutElement rootLE = prefabRoot.AddComponent<LayoutElement>();
            rootLE.minHeight = MISSION_CARD_HEIGHT;
            rootLE.preferredHeight = MISSION_CARD_HEIGHT;

            Image cardBg = prefabRoot.AddComponent<Image>();
            cardBg.color = CARD_BG;

            Outline outline = prefabRoot.AddComponent<Outline>();
            outline.effectColor = CYAN_DARK * 0.3f;
            outline.effectDistance = new Vector2(1, 1);

            HorizontalLayoutGroup hlg = prefabRoot.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 15f;
            hlg.padding = new RectOffset(15, 15, 15, 15);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandHeight = true;

            // Icon Container
            GameObject iconContainer = new GameObject("IconContainer");
            iconContainer.transform.SetParent(prefabRoot.transform, false);
            RectTransform iconContainerRT = iconContainer.AddComponent<RectTransform>();

            Image iconContainerBg = iconContainer.AddComponent<Image>();
            iconContainerBg.color = new Color(DAILY_COLOR.r, DAILY_COLOR.g, DAILY_COLOR.b, 0.2f);

            LayoutElement iconContainerLE = iconContainer.AddComponent<LayoutElement>();
            iconContainerLE.minWidth = 60;
            iconContainerLE.minHeight = 60;

            GameObject icon = new GameObject("Icon");
            icon.transform.SetParent(iconContainer.transform, false);
            RectTransform iconRT = icon.AddComponent<RectTransform>();
            iconRT.anchorMin = Vector2.zero;
            iconRT.anchorMax = Vector2.one;
            iconRT.offsetMin = new Vector2(12, 12);
            iconRT.offsetMax = new Vector2(-12, -12);

            Image iconImage = icon.AddComponent<Image>();
            iconImage.color = DAILY_COLOR;

            // Info Section
            GameObject infoSection = new GameObject("InfoSection");
            infoSection.transform.SetParent(prefabRoot.transform, false);
            RectTransform infoRT = infoSection.AddComponent<RectTransform>();

            LayoutElement infoLE = infoSection.AddComponent<LayoutElement>();
            infoLE.flexibleWidth = 1;

            VerticalLayoutGroup infoVlg = infoSection.AddComponent<VerticalLayoutGroup>();
            infoVlg.spacing = 6f;
            infoVlg.childAlignment = TextAnchor.UpperLeft;
            infoVlg.childControlWidth = true;
            infoVlg.childControlHeight = true;

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(infoSection.transform, false);
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Misión";
            titleText.fontSize = 18;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = TEXT_PRIMARY;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;

            LayoutElement titleLE = titleObj.AddComponent<LayoutElement>();
            titleLE.minHeight = 24;

            // Description
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(infoSection.transform, false);
            TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = "Descripción de la misión";
            descText.fontSize = 13;
            descText.color = TEXT_SECONDARY;
            descText.alignment = TextAlignmentOptions.MidlineLeft;

            LayoutElement descLE = descObj.AddComponent<LayoutElement>();
            descLE.minHeight = 18;

            // Progress Container
            GameObject progressContainer = new GameObject("ProgressContainer");
            progressContainer.transform.SetParent(infoSection.transform, false);

            HorizontalLayoutGroup progressHlg = progressContainer.AddComponent<HorizontalLayoutGroup>();
            progressHlg.spacing = 10f;
            progressHlg.childAlignment = TextAnchor.MiddleLeft;
            progressHlg.childControlWidth = true;
            progressHlg.childControlHeight = true;
            progressHlg.childForceExpandWidth = false;

            LayoutElement progressContainerLE = progressContainer.AddComponent<LayoutElement>();
            progressContainerLE.minHeight = 20;

            // Progress Bar
            GameObject progressBar = new GameObject("ProgressBar");
            progressBar.transform.SetParent(progressContainer.transform, false);

            Image progressBg = progressBar.AddComponent<Image>();
            progressBg.color = new Color(0.1f, 0.12f, 0.15f, 1f);

            LayoutElement progressBarLE = progressBar.AddComponent<LayoutElement>();
            progressBarLE.flexibleWidth = 1;
            progressBarLE.minHeight = 14;

            GameObject progressFill = new GameObject("Fill");
            progressFill.transform.SetParent(progressBar.transform, false);
            RectTransform fillRT = progressFill.AddComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = new Vector2(0.5f, 1);
            fillRT.sizeDelta = Vector2.zero;

            Image fillImage = progressFill.AddComponent<Image>();
            fillImage.color = DAILY_COLOR;

            // Progress Text
            GameObject progressText = new GameObject("ProgressText");
            progressText.transform.SetParent(progressContainer.transform, false);

            TextMeshProUGUI progressTmp = progressText.AddComponent<TextMeshProUGUI>();
            progressTmp.text = "0/1";
            progressTmp.fontSize = 13;
            progressTmp.fontStyle = FontStyles.Bold;
            progressTmp.color = TEXT_SECONDARY;
            progressTmp.alignment = TextAlignmentOptions.MidlineRight;

            LayoutElement progressTextLE = progressText.AddComponent<LayoutElement>();
            progressTextLE.minWidth = 50;

            // Rewards Row
            GameObject rewardsRow = new GameObject("RewardsRow");
            rewardsRow.transform.SetParent(infoSection.transform, false);

            HorizontalLayoutGroup rewardsHlg = rewardsRow.AddComponent<HorizontalLayoutGroup>();
            rewardsHlg.spacing = 15f;
            rewardsHlg.childAlignment = TextAnchor.MiddleLeft;
            rewardsHlg.childControlWidth = false;
            rewardsHlg.childControlHeight = true;

            LayoutElement rewardsLE = rewardsRow.AddComponent<LayoutElement>();
            rewardsLE.minHeight = 22;

            // Right Section (Claim Button / Percentage)
            GameObject rightSection = new GameObject("RightSection");
            rightSection.transform.SetParent(prefabRoot.transform, false);

            LayoutElement rightLE = rightSection.AddComponent<LayoutElement>();
            rightLE.minWidth = 100;

            GameObject percentText = new GameObject("Percent");
            percentText.transform.SetParent(rightSection.transform, false);
            RectTransform percentRT = percentText.AddComponent<RectTransform>();
            percentRT.anchorMin = Vector2.zero;
            percentRT.anchorMax = Vector2.one;
            percentRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI percentTmp = percentText.AddComponent<TextMeshProUGUI>();
            percentTmp.text = "0%";
            percentTmp.fontSize = 24;
            percentTmp.fontStyle = FontStyles.Bold;
            percentTmp.color = TEXT_SECONDARY;
            percentTmp.alignment = TextAlignmentOptions.Center;

            // Save prefab
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            Object.DestroyImmediate(prefabRoot);

            Debug.Log($"[MissionsUI] MissionCard prefab creado en: {prefabPath}");
        }

        // ==================== UTILITY METHODS ====================

        private static void CreateBottomGlow(GameObject obj, Color color)
        {
            GameObject glow = FindOrCreateChild(obj, "BottomGlow");
            RectTransform glowRT = GetOrAddComponent<RectTransform>(glow);
            glowRT.anchorMin = new Vector2(0, 0);
            glowRT.anchorMax = new Vector2(1, 0);
            glowRT.pivot = new Vector2(0.5f, 1);
            glowRT.anchoredPosition = Vector2.zero;
            glowRT.sizeDelta = new Vector2(0, 3);

            Image glowImage = GetOrAddComponent<Image>(glow);
            glowImage.color = color;
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
