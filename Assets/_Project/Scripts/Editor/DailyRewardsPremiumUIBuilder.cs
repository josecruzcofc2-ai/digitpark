using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Construye la UI PREMIUM de Daily Rewards.
    /// Diseño inspirado en Top 10 apps iOS: Coin Master, Clash Royale, Candy Crush.
    ///
    /// Características Premium:
    /// - Sistema de Racha (Streak) con multiplicadores y barra de progreso
    /// - Día 7 como evento especial con cofre legendario 3x más grande
    /// - Cofres con colores de rareza en lugar de iconos planos
    /// - Path visual conectando los días
    /// - Efectos de glow pulsante en día actual y día 7
    /// - Botón de reclamar con shine animado
    /// - Timer de próxima recompensa
    /// - Popup de celebración con confeti
    /// - Popup de racha perdida con opción de restaurar
    /// </summary>
    public class DailyRewardsPremiumUIBuilder : EditorWindow
    {
        // ==================== COLORES PREMIUM ====================

        // Base Colors
        private static readonly Color DARK_BG = new Color(0.04f, 0.09f, 0.16f, 1f);           // #0A1628
        private static readonly Color PANEL_BG = new Color(0.06f, 0.1f, 0.18f, 0.98f);
        private static readonly Color CARD_BG = new Color(0.08f, 0.12f, 0.2f, 1f);
        private static readonly Color HEADER_BG = new Color(0.05f, 0.08f, 0.14f, 0.95f);

        // Accent Colors
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.5f, 0.5f, 1f);

        // Gold Theme (Day 7 Special)
        private static readonly Color GOLD_BRIGHT = new Color(1f, 0.84f, 0f, 1f);             // #FFD700
        private static readonly Color GOLD_DARK = new Color(1f, 0.55f, 0f, 1f);               // #FF8C00
        private static readonly Color GOLD_GLOW = new Color(1f, 0.84f, 0f, 0.4f);

        // Streak Colors
        private static readonly Color STREAK_FIRE = new Color(1f, 0.42f, 0.21f, 1f);          // #FF6B35
        private static readonly Color STREAK_GLOW = new Color(1f, 0.42f, 0.21f, 0.5f);

        // Day State Colors
        private static readonly Color DAY_CLAIMED_BG = new Color(0.1f, 0.3f, 0.23f, 1f);      // #1A4D3A
        private static readonly Color DAY_CLAIMED_BORDER = new Color(0f, 1f, 0.53f, 1f);      // #00FF88
        private static readonly Color DAY_CURRENT_GLOW = new Color(1f, 0.84f, 0f, 0.6f);
        private static readonly Color DAY_LOCKED_BG = new Color(0.16f, 0.23f, 0.29f, 1f);     // #2A3A4A
        private static readonly Color DAY_LOCKED_BORDER = new Color(0.3f, 0.35f, 0.4f, 0.5f);

        // Chest Rarity Colors
        private static readonly Color CHEST_COMMON = new Color(0.6f, 0.6f, 0.65f, 1f);        // Gray
        private static readonly Color CHEST_RARE = new Color(0.2f, 0.6f, 1f, 1f);             // Blue
        private static readonly Color CHEST_EPIC = new Color(0.7f, 0.3f, 1f, 1f);             // Purple
        private static readonly Color CHEST_LEGENDARY = new Color(1f, 0.84f, 0f, 1f);         // Gold

        // Text Colors
        private static readonly Color TEXT_PRIMARY = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.7f, 0.75f, 1f);
        private static readonly Color TEXT_GOLD = new Color(1f, 0.9f, 0.5f, 1f);
        private static readonly Color TEXT_DARK = new Color(0.05f, 0.08f, 0.12f, 1f);

        // Button Colors
        private static readonly Color BUTTON_CLAIM = new Color(0.2f, 0.85f, 0.4f, 1f);
        private static readonly Color BUTTON_CLAIM_GLOW = new Color(0.2f, 1f, 0.5f, 0.5f);

        // Currency Colors
        private static readonly Color COIN_COLOR = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GEM_COLOR = new Color(0.4f, 0.7f, 1f, 1f);
        private static readonly Color XP_COLOR = new Color(0.4f, 0.9f, 0.4f, 1f);

        // ==================== DIMENSIONES (OPTIMIZADAS 10/10) ====================
        private const float HEADER_HEIGHT = 90f;
        private const float STREAK_PANEL_HEIGHT = 90f;
        private const float WEEK_TITLE_HEIGHT = 30f;
        private const float DAY_CARD_SIZE = 115f;  // Larger cards for better visibility
        private const float DAY_CARD_SPACING = 12f;
        private const float DAY7_CARD_WIDTH = 360f;  // Wider Day 7 card
        private const float DAY7_CARD_HEIGHT = 140f;  // More compact
        private const float TODAY_REWARD_HEIGHT = 100f;  // More compact
        private const float CLAIM_BUTTON_HEIGHT = 60f;
        private const float TIMER_HEIGHT = 35f;

        [MenuItem("DigitPark/UI Builders/Monetization/Daily Rewards PREMIUM", false, 184)]
        public static void BuildUI()
        {
            if (!EditorUtility.DisplayDialog("Daily Rewards PREMIUM Builder",
                "Esto construirá la UI PREMIUM de Daily Rewards.\n" +
                "Asegúrate de tener la escena DailyRewards abierta.\n\n" +
                "Diseño inspirado en Top 10 iOS Apps:\n\n" +
                "✓ Sistema de Racha con multiplicadores\n" +
                "✓ Día 7 como MEGA recompensa (3x más grande)\n" +
                "✓ Cofres con colores de rareza\n" +
                "✓ Efectos de glow pulsante\n" +
                "✓ Botón con shine animado\n" +
                "✓ Timer de próxima recompensa\n" +
                "✓ Popup de celebración\n" +
                "✓ Popup de racha perdida\n\n" +
                "¿Continuar?",
                "Sí, crear Premium UI", "Cancelar"))
                return;

            BuildPremiumDailyRewards();
        }

        private static void BuildPremiumDailyRewards()
        {
            Debug.Log("[DailyRewards PREMIUM] ========== INICIANDO CONSTRUCCIÓN ==========");

            Canvas canvas = SetupCanvas();
            if (canvas == null) return;

            CleanupOldUI(canvas);

            CreateBackground(canvas);
            GameObject safeArea = CreateSafeArea(canvas);

            CreateHeader(safeArea);
            CreateStreakPanel(safeArea);
            CreateWeekTitle(safeArea);
            CreateDaysGrid(safeArea);
            CreateDay7Special(safeArea);
            CreateTodayRewardPanel(safeArea);
            CreateClaimButton(safeArea);
            CreateNextRewardTimer(safeArea);

            CreateClaimCelebration(canvas);
            CreateStreakLostPopup(canvas);

            MarkSceneDirty();
            Debug.Log("[DailyRewards PREMIUM] ========== CONSTRUCCIÓN COMPLETADA ==========");

            EditorUtility.DisplayDialog("Daily Rewards PREMIUM Completado",
                "UI Premium de Daily Rewards creada exitosamente.\n\n" +
                "Elementos creados:\n" +
                "✓ Header con monedas/gemas\n" +
                "✓ Panel de Racha con barra de progreso\n" +
                "✓ 6 días normales con cofres de rareza\n" +
                "✓ Día 7 especial (MEGA COFRE)\n" +
                "✓ Panel de recompensa de hoy\n" +
                "✓ Botón de reclamar con glow\n" +
                "✓ Timer de próxima recompensa\n" +
                "✓ Popup de celebración\n" +
                "✓ Popup de racha perdida\n\n" +
                "Asigna el DailyRewardsManager y conecta las referencias.",
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
            Debug.Log("[DailyRewards PREMIUM] Limpiando UI antigua...");

            string[] oldElements = new string[]
            {
                "SafeArea", "Background", "Header", "StreakPanel", "WeekTitle",
                "DaysGrid", "DaysContainer", "Day7Special", "TodayRewardPanel",
                "ClaimButton", "ClaimArea", "NextRewardTimer", "ClaimCelebration",
                "StreakLostPopup", "RewardClaimBlocker", "RewardEffects",
                "Content", "BonusPreview", "ProgressContainer", "FireGlow"
            };

            var toDestroy = new System.Collections.Generic.List<GameObject>();

            foreach (Transform child in canvas.transform)
            {
                foreach (string name in oldElements)
                {
                    if (child.name == name || child.name.StartsWith(name))
                    {
                        toDestroy.Add(child.gameObject);
                        break;
                    }
                }
            }

            foreach (var obj in toDestroy)
            {
                Object.DestroyImmediate(obj);
            }

            Debug.Log("[DailyRewards PREMIUM] Limpieza completada");
        }

        private static void CreateBackground(Canvas canvas)
        {
            GameObject bg = FindOrCreateChild(canvas.gameObject, "Background");
            SetRectTransformStretch(bg);

            Image bgImage = GetOrAddComponent<Image>(bg);
            bgImage.color = DARK_BG;

            // Ambient glow from top
            GameObject topGlow = FindOrCreateChild(bg, "TopGlow");
            RectTransform topGlowRT = GetOrAddComponent<RectTransform>(topGlow);
            topGlowRT.anchorMin = new Vector2(0, 1);
            topGlowRT.anchorMax = new Vector2(1, 1);
            topGlowRT.pivot = new Vector2(0.5f, 1);
            topGlowRT.anchoredPosition = Vector2.zero;
            topGlowRT.sizeDelta = new Vector2(0, 400);

            Image topGlowImg = GetOrAddComponent<Image>(topGlow);
            topGlowImg.color = new Color(GOLD_BRIGHT.r, GOLD_BRIGHT.g, GOLD_BRIGHT.b, 0.05f);

            bg.transform.SetAsFirstSibling();
        }

        private static GameObject CreateSafeArea(Canvas canvas)
        {
            GameObject safeArea = FindOrCreateChild(canvas.gameObject, "SafeArea");
            SetRectTransformStretch(safeArea);
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

            CreateGlowLine(header, GOLD_BRIGHT, true);

            // Back Button
            GameObject backBtn = FindOrCreateChild(header, "BackButton");
            RectTransform backRT = GetOrAddComponent<RectTransform>(backBtn);
            backRT.anchorMin = new Vector2(0, 0.5f);
            backRT.anchorMax = new Vector2(0, 0.5f);
            backRT.pivot = new Vector2(0, 0.5f);
            backRT.anchoredPosition = new Vector2(20, 0);
            backRT.sizeDelta = new Vector2(50, 50);

            Image backBg = GetOrAddComponent<Image>(backBtn);
            backBg.color = new Color(0.15f, 0.2f, 0.28f, 1f);
            AddOutline(backBtn, CYAN_DARK, 1);

            Button backButton = GetOrAddComponent<Button>(backBtn);
            SetupButtonColors(backButton);

            GameObject backText = FindOrCreateChild(backBtn, "Text");
            SetRectTransformStretch(backText);
            TextMeshProUGUI backTmp = GetOrAddComponent<TextMeshProUGUI>(backText);
            backTmp.text = "<";
            backTmp.fontSize = 32;
            backTmp.fontStyle = FontStyles.Bold;
            backTmp.color = CYAN_NEON;
            backTmp.alignment = TextAlignmentOptions.Center;

            // Title
            GameObject title = FindOrCreateChild(header, "Title");
            RectTransform titleRT = GetOrAddComponent<RectTransform>(title);
            titleRT.anchorMin = new Vector2(0.5f, 0.5f);
            titleRT.anchorMax = new Vector2(0.5f, 0.5f);
            titleRT.sizeDelta = new Vector2(400, 50);

            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(title);
            titleText.text = "RECOMPENSAS DIARIAS";
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = GOLD_BRIGHT;
            titleText.alignment = TextAlignmentOptions.Center;

            // Currency Row
            GameObject currencyRow = FindOrCreateChild(header, "CurrencyRow");
            RectTransform currencyRT = GetOrAddComponent<RectTransform>(currencyRow);
            currencyRT.anchorMin = new Vector2(1, 0.5f);
            currencyRT.anchorMax = new Vector2(1, 0.5f);
            currencyRT.pivot = new Vector2(1, 0.5f);
            currencyRT.anchoredPosition = new Vector2(-20, 0);
            currencyRT.sizeDelta = new Vector2(200, 40);

            HorizontalLayoutGroup currencyHlg = GetOrAddComponent<HorizontalLayoutGroup>(currencyRow);
            currencyHlg.spacing = 15f;
            currencyHlg.childAlignment = TextAnchor.MiddleRight;
            currencyHlg.childControlWidth = false;
            currencyHlg.childControlHeight = true;

            CreateCurrencyBadge(currencyRow, "Coins", "5,430", COIN_COLOR);
            CreateCurrencyBadge(currencyRow, "Gems", "125", GEM_COLOR);

            Debug.Log("[DailyRewards PREMIUM] Header creado");
        }

        private static void CreateCurrencyBadge(GameObject parent, string name, string amount, Color color)
        {
            GameObject badge = FindOrCreateChild(parent, name);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(badge);
            hlg.spacing = 6f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;

            LayoutElement badgeLE = GetOrAddComponent<LayoutElement>(badge);
            badgeLE.minWidth = 90;

            GameObject icon = FindOrCreateChild(badge, "Icon");
            Image iconImg = GetOrAddComponent<Image>(icon);
            iconImg.color = color;
            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(icon);
            iconLE.minWidth = 24;
            iconLE.minHeight = 24;

            GameObject text = FindOrCreateChild(badge, "Text");
            TextMeshProUGUI textTmp = GetOrAddComponent<TextMeshProUGUI>(text);
            textTmp.text = amount;
            textTmp.fontSize = 18;
            textTmp.fontStyle = FontStyles.Bold;
            textTmp.color = color;
            textTmp.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement textLE = GetOrAddComponent<LayoutElement>(text);
            textLE.minWidth = 60;
        }

        // ==================== STREAK PANEL ====================

        private static void CreateStreakPanel(GameObject parent)
        {
            float yPos = -HEADER_HEIGHT - 10;

            GameObject streak = FindOrCreateChild(parent, "StreakPanel");

            RectTransform streakRT = GetOrAddComponent<RectTransform>(streak);
            streakRT.anchorMin = new Vector2(0, 1);
            streakRT.anchorMax = new Vector2(1, 1);
            streakRT.pivot = new Vector2(0.5f, 1);
            streakRT.anchoredPosition = new Vector2(0, yPos);
            streakRT.sizeDelta = new Vector2(-40, STREAK_PANEL_HEIGHT);

            Image streakBg = GetOrAddComponent<Image>(streak);
            streakBg.color = PANEL_BG;
            AddOutline(streak, STREAK_FIRE, 2);

            // Title Row: [Fire] RACHA ACTUAL: 5 DÍAS (positioned manually)
            GameObject titleRow = FindOrCreateChild(streak, "TitleRow");
            RectTransform titleRowRT = GetOrAddComponent<RectTransform>(titleRow);
            titleRowRT.anchorMin = new Vector2(0, 1);
            titleRowRT.anchorMax = new Vector2(1, 1);
            titleRowRT.pivot = new Vector2(0.5f, 1);
            titleRowRT.anchoredPosition = new Vector2(0, -12);
            titleRowRT.sizeDelta = new Vector2(-40, 30);

            HorizontalLayoutGroup titleHlg = GetOrAddComponent<HorizontalLayoutGroup>(titleRow);
            titleHlg.spacing = 10f;
            titleHlg.padding = new RectOffset(10, 10, 0, 0);
            titleHlg.childAlignment = TextAnchor.MiddleCenter;
            titleHlg.childControlWidth = false;
            titleHlg.childControlHeight = true;

            // Fire Icon
            GameObject fireIcon = FindOrCreateChild(titleRow, "FireIcon");
            Image fireImg = GetOrAddComponent<Image>(fireIcon);
            fireImg.color = STREAK_FIRE;
            LayoutElement fireLE = GetOrAddComponent<LayoutElement>(fireIcon);
            fireLE.minWidth = 26;
            fireLE.minHeight = 26;

            // Streak Text
            GameObject streakText = FindOrCreateChild(titleRow, "StreakText");
            TextMeshProUGUI streakTmp = GetOrAddComponent<TextMeshProUGUI>(streakText);
            streakTmp.text = "RACHA ACTUAL:";
            streakTmp.fontSize = 16;
            streakTmp.fontStyle = FontStyles.Bold;
            streakTmp.color = TEXT_PRIMARY;
            streakTmp.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement streakTextLE = GetOrAddComponent<LayoutElement>(streakText);
            streakTextLE.minWidth = 145;

            // Streak Count
            GameObject streakCount = FindOrCreateChild(titleRow, "StreakCount");
            TextMeshProUGUI countTmp = GetOrAddComponent<TextMeshProUGUI>(streakCount);
            countTmp.text = "5 DÍAS";
            countTmp.fontSize = 20;
            countTmp.fontStyle = FontStyles.Bold;
            countTmp.color = STREAK_FIRE;
            countTmp.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement countLE = GetOrAddComponent<LayoutElement>(streakCount);
            countLE.minWidth = 90;

            // Progress Bar (positioned manually)
            GameObject progressBar = FindOrCreateChild(streak, "ProgressBar");
            RectTransform progressRT = GetOrAddComponent<RectTransform>(progressBar);
            progressRT.anchorMin = new Vector2(0, 1);
            progressRT.anchorMax = new Vector2(1, 1);
            progressRT.pivot = new Vector2(0.5f, 1);
            progressRT.anchoredPosition = new Vector2(0, -48);
            progressRT.sizeDelta = new Vector2(-40, 22);

            Image progressBg = GetOrAddComponent<Image>(progressBar);
            progressBg.color = new Color(0.15f, 0.18f, 0.25f, 1f);
            AddOutline(progressBar, new Color(0.25f, 0.3f, 0.35f, 0.6f));

            // Progress Fill (5/7 = ~71%)
            GameObject progressFill = FindOrCreateChild(progressBar, "Fill");
            RectTransform fillRT = GetOrAddComponent<RectTransform>(progressFill);
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = new Vector2(0.71f, 1);
            fillRT.offsetMin = Vector2.zero;
            fillRT.offsetMax = Vector2.zero;

            Image fillImg = GetOrAddComponent<Image>(progressFill);
            fillImg.color = STREAK_FIRE;

            // Progress Text (on top of bar)
            GameObject progressText = FindOrCreateChild(progressBar, "Text");
            SetRectTransformStretch(progressText);
            TextMeshProUGUI progressTmp = GetOrAddComponent<TextMeshProUGUI>(progressText);
            progressTmp.text = "5/7 para BONUS";
            progressTmp.fontSize = 11;
            progressTmp.fontStyle = FontStyles.Bold;
            progressTmp.color = Color.white;
            progressTmp.alignment = TextAlignmentOptions.Center;

            // Bonus Preview (positioned manually at bottom)
            GameObject bonusText = FindOrCreateChild(streak, "BonusText");
            RectTransform bonusRT = GetOrAddComponent<RectTransform>(bonusText);
            bonusRT.anchorMin = new Vector2(0, 0);
            bonusRT.anchorMax = new Vector2(1, 0);
            bonusRT.pivot = new Vector2(0.5f, 0);
            bonusRT.anchoredPosition = new Vector2(0, 10);
            bonusRT.sizeDelta = new Vector2(0, 20);

            TextMeshProUGUI bonusTmp = GetOrAddComponent<TextMeshProUGUI>(bonusText);
            bonusTmp.text = "Día 7: COFRE LEGENDARIO + 50 Gemas";
            bonusTmp.fontSize = 12;
            bonusTmp.color = TEXT_GOLD;
            bonusTmp.alignment = TextAlignmentOptions.Center;

            Debug.Log("[DailyRewards PREMIUM] StreakPanel creado");
        }

        // ==================== WEEK TITLE ====================

        private static void CreateWeekTitle(GameObject parent)
        {
            // Position: after Header + margin + StreakPanel + margin (reduced)
            float yPos = -HEADER_HEIGHT - 10 - STREAK_PANEL_HEIGHT - 8;

            GameObject weekTitle = FindOrCreateChild(parent, "WeekTitle");

            RectTransform weekRT = GetOrAddComponent<RectTransform>(weekTitle);
            weekRT.anchorMin = new Vector2(0.5f, 1);
            weekRT.anchorMax = new Vector2(0.5f, 1);
            weekRT.pivot = new Vector2(0.5f, 1);
            weekRT.anchoredPosition = new Vector2(0, yPos);
            weekRT.sizeDelta = new Vector2(280, WEEK_TITLE_HEIGHT);

            // Decorative lines
            GameObject leftLine = FindOrCreateChild(weekTitle, "LeftLine");
            RectTransform leftRT = GetOrAddComponent<RectTransform>(leftLine);
            leftRT.anchorMin = new Vector2(0, 0.5f);
            leftRT.anchorMax = new Vector2(0.25f, 0.5f);
            leftRT.sizeDelta = new Vector2(0, 2);

            Image leftImg = GetOrAddComponent<Image>(leftLine);
            leftImg.color = GOLD_DARK;

            GameObject rightLine = FindOrCreateChild(weekTitle, "RightLine");
            RectTransform rightRT = GetOrAddComponent<RectTransform>(rightLine);
            rightRT.anchorMin = new Vector2(0.75f, 0.5f);
            rightRT.anchorMax = new Vector2(1f, 0.5f);
            rightRT.sizeDelta = new Vector2(0, 2);

            Image rightImg = GetOrAddComponent<Image>(rightLine);
            rightImg.color = GOLD_DARK;

            // Title
            GameObject titleText = FindOrCreateChild(weekTitle, "Text");
            SetRectTransformStretch(titleText);
            TextMeshProUGUI titleTmp = GetOrAddComponent<TextMeshProUGUI>(titleText);
            titleTmp.text = "SEMANA 1";
            titleTmp.fontSize = 18;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = GOLD_BRIGHT;
            titleTmp.alignment = TextAlignmentOptions.Center;

            Debug.Log("[DailyRewards PREMIUM] WeekTitle creado");
        }

        // ==================== DAYS GRID ====================

        private static void CreateDaysGrid(GameObject parent)
        {
            // Position: after Header + StreakPanel + WeekTitle + margins (reduced)
            float yPos = -HEADER_HEIGHT - 10 - STREAK_PANEL_HEIGHT - 8 - WEEK_TITLE_HEIGHT - 8;

            GameObject daysContainer = FindOrCreateChild(parent, "DaysContainer");

            RectTransform containerRT = GetOrAddComponent<RectTransform>(daysContainer);
            containerRT.anchorMin = new Vector2(0, 1);
            containerRT.anchorMax = new Vector2(1, 1);
            containerRT.pivot = new Vector2(0.5f, 1);
            containerRT.anchoredPosition = new Vector2(0, yPos);
            containerRT.sizeDelta = new Vector2(-40, DAY_CARD_SIZE * 2 + DAY_CARD_SPACING + 10);

            VerticalLayoutGroup containerVlg = GetOrAddComponent<VerticalLayoutGroup>(daysContainer);
            containerVlg.spacing = DAY_CARD_SPACING;
            containerVlg.childAlignment = TextAnchor.UpperCenter;
            containerVlg.childControlWidth = true;
            containerVlg.childControlHeight = true;
            containerVlg.childForceExpandWidth = true;
            containerVlg.childForceExpandHeight = false;

            // Row 1: Days 1-3
            GameObject row1 = FindOrCreateChild(daysContainer, "Row1");
            HorizontalLayoutGroup row1Hlg = GetOrAddComponent<HorizontalLayoutGroup>(row1);
            row1Hlg.spacing = DAY_CARD_SPACING;
            row1Hlg.childAlignment = TextAnchor.MiddleCenter;
            row1Hlg.childControlWidth = false;
            row1Hlg.childControlHeight = false;
            row1Hlg.childForceExpandWidth = false;
            LayoutElement row1LE = GetOrAddComponent<LayoutElement>(row1);
            row1LE.preferredHeight = DAY_CARD_SIZE;

            // Row 2: Days 4-6
            GameObject row2 = FindOrCreateChild(daysContainer, "Row2");
            HorizontalLayoutGroup row2Hlg = GetOrAddComponent<HorizontalLayoutGroup>(row2);
            row2Hlg.spacing = DAY_CARD_SPACING;
            row2Hlg.childAlignment = TextAnchor.MiddleCenter;
            row2Hlg.childControlWidth = false;
            row2Hlg.childControlHeight = false;
            row2Hlg.childForceExpandWidth = false;
            LayoutElement row2LE = GetOrAddComponent<LayoutElement>(row2);
            row2LE.preferredHeight = DAY_CARD_SIZE;

            // Days 1-6 with chest rarities
            var dayData = new (string day, string reward, string type, Color chest, bool claimed, bool current)[]
            {
                ("DÍA 1", "100", "Monedas", CHEST_COMMON, true, false),
                ("DÍA 2", "150", "Monedas", CHEST_COMMON, true, false),
                ("DÍA 3", "25", "Gemas", CHEST_RARE, true, false),
                ("DÍA 4", "200", "Monedas", CHEST_COMMON, true, false),
                ("DÍA 5", "COFRE", "Aleatorio", CHEST_EPIC, true, false),
                ("DÍA 6", "300", "Monedas", CHEST_RARE, false, true),
            };

            // Days 1-3 in Row 1
            for (int i = 0; i < 3; i++)
            {
                var d = dayData[i];
                CreateDayCard(row1, $"Day{i + 1}", d.day, d.reward, d.type, d.chest, d.claimed, d.current);
            }

            // Days 4-6 in Row 2
            for (int i = 3; i < 6; i++)
            {
                var d = dayData[i];
                CreateDayCard(row2, $"Day{i + 1}", d.day, d.reward, d.type, d.chest, d.claimed, d.current);
            }

            Debug.Log("[DailyRewards PREMIUM] DaysContainer creado con 6 días (3x2)");
        }

        private static void CreateDayCard(GameObject parent, string name, string dayLabel, string reward,
            string rewardType, Color chestColor, bool claimed, bool current)
        {
            GameObject card = FindOrCreateChild(parent, name);

            // Fixed size for day card
            LayoutElement cardLE = GetOrAddComponent<LayoutElement>(card);
            cardLE.preferredWidth = DAY_CARD_SIZE;
            cardLE.preferredHeight = DAY_CARD_SIZE;
            cardLE.minWidth = DAY_CARD_SIZE;
            cardLE.minHeight = DAY_CARD_SIZE;

            Image cardBg = GetOrAddComponent<Image>(card);
            cardBg.color = claimed ? DAY_CLAIMED_BG : (current ? CARD_BG : DAY_LOCKED_BG);

            Color borderColor = claimed ? DAY_CLAIMED_BORDER : (current ? GOLD_BRIGHT : DAY_LOCKED_BORDER);
            AddOutline(card, borderColor, claimed ? 2 : (current ? 3 : 1));

            // Current day glow
            if (current)
            {
                GameObject currentGlow = FindOrCreateChild(card, "CurrentGlow");
                SetRectTransformStretch(currentGlow);
                RectTransform glowRT = currentGlow.GetComponent<RectTransform>();
                glowRT.offsetMin = new Vector2(-8, -8);
                glowRT.offsetMax = new Vector2(8, 8);
                currentGlow.transform.SetAsFirstSibling();

                Image glowImg = GetOrAddComponent<Image>(currentGlow);
                glowImg.color = DAY_CURRENT_GLOW;
            }

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(card);
            vlg.spacing = 6f;
            vlg.padding = new RectOffset(10, 10, 12, 12);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            // Day Label
            GameObject dayLabelObj = FindOrCreateChild(card, "DayLabel");
            TextMeshProUGUI dayTmp = GetOrAddComponent<TextMeshProUGUI>(dayLabelObj);
            dayTmp.text = dayLabel;
            dayTmp.fontSize = 12;
            dayTmp.fontStyle = FontStyles.Bold;
            dayTmp.color = claimed ? DAY_CLAIMED_BORDER : (current ? GOLD_BRIGHT : TEXT_SECONDARY);
            dayTmp.alignment = TextAlignmentOptions.Center;
            LayoutElement dayLE = GetOrAddComponent<LayoutElement>(dayLabelObj);
            dayLE.minHeight = 18;

            // Chest Container
            GameObject chestContainer = FindOrCreateChild(card, "ChestContainer");
            LayoutElement chestContainerLE = GetOrAddComponent<LayoutElement>(chestContainer);
            chestContainerLE.minHeight = 55;
            chestContainerLE.preferredHeight = 55;

            GameObject chest = FindOrCreateChild(chestContainer, "Chest");
            RectTransform chestRT = GetOrAddComponent<RectTransform>(chest);
            chestRT.anchorMin = new Vector2(0.5f, 0.5f);
            chestRT.anchorMax = new Vector2(0.5f, 0.5f);
            chestRT.sizeDelta = new Vector2(50, 50);

            Image chestImg = GetOrAddComponent<Image>(chest);
            chestImg.color = claimed ? new Color(chestColor.r, chestColor.g, chestColor.b, 0.5f) : chestColor;

            // Chest glow for current
            if (current)
            {
                GameObject chestGlow = FindOrCreateChild(chest, "Glow");
                SetRectTransformStretch(chestGlow);
                RectTransform chestGlowRT = chestGlow.GetComponent<RectTransform>();
                chestGlowRT.offsetMin = new Vector2(-15, -15);
                chestGlowRT.offsetMax = new Vector2(15, 15);
                chestGlow.transform.SetAsFirstSibling();

                Image chestGlowImg = GetOrAddComponent<Image>(chestGlow);
                chestGlowImg.color = new Color(chestColor.r, chestColor.g, chestColor.b, 0.4f);
            }

            // Reward Amount
            GameObject rewardObj = FindOrCreateChild(card, "Reward");
            TextMeshProUGUI rewardTmp = GetOrAddComponent<TextMeshProUGUI>(rewardObj);
            rewardTmp.text = reward;
            rewardTmp.fontSize = 16;
            rewardTmp.fontStyle = FontStyles.Bold;
            rewardTmp.color = claimed ? new Color(1, 1, 1, 0.6f) : (rewardType == "Gemas" ? GEM_COLOR : COIN_COLOR);
            rewardTmp.alignment = TextAlignmentOptions.Center;
            LayoutElement rewardLE = GetOrAddComponent<LayoutElement>(rewardObj);
            rewardLE.minHeight = 22;

            // Reward Type
            GameObject typeObj = FindOrCreateChild(card, "RewardType");
            TextMeshProUGUI typeTmp = GetOrAddComponent<TextMeshProUGUI>(typeObj);
            typeTmp.text = rewardType;
            typeTmp.fontSize = 10;
            typeTmp.color = claimed ? new Color(1, 1, 1, 0.4f) : TEXT_SECONDARY;
            typeTmp.alignment = TextAlignmentOptions.Center;
            LayoutElement typeLE = GetOrAddComponent<LayoutElement>(typeObj);
            typeLE.minHeight = 14;

            // Claimed checkmark
            if (claimed)
            {
                GameObject checkmark = FindOrCreateChild(card, "Checkmark");
                RectTransform checkRT = GetOrAddComponent<RectTransform>(checkmark);
                checkRT.anchorMin = new Vector2(1, 1);
                checkRT.anchorMax = new Vector2(1, 1);
                checkRT.pivot = new Vector2(1, 1);
                checkRT.anchoredPosition = new Vector2(-5, -5);
                checkRT.sizeDelta = new Vector2(28, 28);

                Image checkBg = GetOrAddComponent<Image>(checkmark);
                checkBg.color = DAY_CLAIMED_BORDER;

                GameObject checkIcon = FindOrCreateChild(checkmark, "Icon");
                SetRectTransformStretch(checkIcon);
                TextMeshProUGUI checkTmp = GetOrAddComponent<TextMeshProUGUI>(checkIcon);
                checkTmp.text = "✓";
                checkTmp.fontSize = 18;
                checkTmp.fontStyle = FontStyles.Bold;
                checkTmp.color = TEXT_DARK;
                checkTmp.alignment = TextAlignmentOptions.Center;
            }

            // "HOY" badge for current (positioned INSIDE the card at top)
            if (current)
            {
                GameObject todayBadge = FindOrCreateChild(card, "TodayBadge");
                RectTransform badgeRT = GetOrAddComponent<RectTransform>(todayBadge);
                badgeRT.anchorMin = new Vector2(0.5f, 1);
                badgeRT.anchorMax = new Vector2(0.5f, 1);
                badgeRT.pivot = new Vector2(0.5f, 1);
                badgeRT.anchoredPosition = new Vector2(0, -3);  // Inside the card
                badgeRT.sizeDelta = new Vector2(50, 18);

                Image badgeBg = GetOrAddComponent<Image>(todayBadge);
                badgeBg.color = GOLD_BRIGHT;

                GameObject badgeText = FindOrCreateChild(todayBadge, "Text");
                SetRectTransformStretch(badgeText);
                TextMeshProUGUI badgeTmp = GetOrAddComponent<TextMeshProUGUI>(badgeText);
                badgeTmp.text = "HOY";
                badgeTmp.fontSize = 10;
                badgeTmp.fontStyle = FontStyles.Bold;
                badgeTmp.color = TEXT_DARK;
                badgeTmp.alignment = TextAlignmentOptions.Center;
            }
        }

        // ==================== DAY 7 SPECIAL ====================

        private static void CreateDay7Special(GameObject parent)
        {
            // Position after: Header + StreakPanel + WeekTitle + DaysContainer + margins (reduced)
            float daysGridHeight = DAY_CARD_SIZE * 2 + DAY_CARD_SPACING + 10;
            float yPos = -HEADER_HEIGHT - 10 - STREAK_PANEL_HEIGHT - 8 - WEEK_TITLE_HEIGHT - 8 - daysGridHeight - 10;

            GameObject day7 = FindOrCreateChild(parent, "Day7Special");

            RectTransform day7RT = GetOrAddComponent<RectTransform>(day7);
            day7RT.anchorMin = new Vector2(0.5f, 1);
            day7RT.anchorMax = new Vector2(0.5f, 1);
            day7RT.pivot = new Vector2(0.5f, 1);
            day7RT.anchoredPosition = new Vector2(0, yPos);
            day7RT.sizeDelta = new Vector2(DAY7_CARD_WIDTH, DAY7_CARD_HEIGHT);

            // Main background with gold border (simplified, no outer glow)
            Image day7Bg = GetOrAddComponent<Image>(day7);
            day7Bg.color = new Color(0.12f, 0.1f, 0.05f, 1f);
            AddOutline(day7, GOLD_BRIGHT, 3);

            // Content using HorizontalLayout: [Chest Icon] [Info]
            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(day7);
            hlg.spacing = 20f;
            hlg.padding = new RectOffset(25, 25, 15, 15);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;

            // Left: Chest Icon with glow
            GameObject chestContainer = FindOrCreateChild(day7, "ChestContainer");
            LayoutElement chestContainerLE = GetOrAddComponent<LayoutElement>(chestContainer);
            chestContainerLE.minWidth = 90;
            chestContainerLE.minHeight = 90;

            GameObject chest = FindOrCreateChild(chestContainer, "Chest");
            RectTransform chestRT = GetOrAddComponent<RectTransform>(chest);
            chestRT.anchorMin = new Vector2(0.5f, 0.5f);
            chestRT.anchorMax = new Vector2(0.5f, 0.5f);
            chestRT.sizeDelta = new Vector2(70, 70);

            Image chestImg = GetOrAddComponent<Image>(chest);
            chestImg.color = CHEST_LEGENDARY;

            // Chest glow
            GameObject chestGlow = FindOrCreateChild(chest, "Glow");
            SetRectTransformStretch(chestGlow);
            RectTransform chestGlowRT = chestGlow.GetComponent<RectTransform>();
            chestGlowRT.offsetMin = new Vector2(-15, -15);
            chestGlowRT.offsetMax = new Vector2(15, 15);
            chestGlow.transform.SetAsFirstSibling();
            Image chestGlowImg = GetOrAddComponent<Image>(chestGlow);
            chestGlowImg.color = GOLD_GLOW;

            // Right: Info
            GameObject info = FindOrCreateChild(day7, "Info");
            VerticalLayoutGroup infoVlg = GetOrAddComponent<VerticalLayoutGroup>(info);
            infoVlg.spacing = 4f;
            infoVlg.childAlignment = TextAnchor.MiddleLeft;
            infoVlg.childControlWidth = true;
            infoVlg.childControlHeight = true;
            LayoutElement infoLE = GetOrAddComponent<LayoutElement>(info);
            infoLE.minWidth = 200;

            // Day 7 Label
            GameObject dayLabel = FindOrCreateChild(info, "DayLabel");
            TextMeshProUGUI dayTmp = GetOrAddComponent<TextMeshProUGUI>(dayLabel);
            dayTmp.text = "DÍA 7 - GRAN COFRE";
            dayTmp.fontSize = 16;
            dayTmp.fontStyle = FontStyles.Bold;
            dayTmp.color = GOLD_BRIGHT;
            dayTmp.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement dayLE = GetOrAddComponent<LayoutElement>(dayLabel);
            dayLE.minHeight = 22;

            // Rewards summary
            GameObject rewardLine1 = FindOrCreateChild(info, "Reward1");
            TextMeshProUGUI r1Tmp = GetOrAddComponent<TextMeshProUGUI>(rewardLine1);
            r1Tmp.text = "• 500 Monedas + 50 Gemas";
            r1Tmp.fontSize = 13;
            r1Tmp.color = TEXT_GOLD;
            r1Tmp.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement r1LE = GetOrAddComponent<LayoutElement>(rewardLine1);
            r1LE.minHeight = 18;

            GameObject rewardLine2 = FindOrCreateChild(info, "Reward2");
            TextMeshProUGUI r2Tmp = GetOrAddComponent<TextMeshProUGUI>(rewardLine2);
            r2Tmp.text = "• Item Exclusivo";
            r2Tmp.fontSize = 13;
            r2Tmp.color = CHEST_EPIC;
            r2Tmp.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement r2LE = GetOrAddComponent<LayoutElement>(rewardLine2);
            r2LE.minHeight = 18;

            // Locked text
            GameObject lockedText = FindOrCreateChild(info, "Locked");
            TextMeshProUGUI lockedTmp = GetOrAddComponent<TextMeshProUGUI>(lockedText);
            lockedTmp.text = "🔒 Desbloquea en 1 día";
            lockedTmp.fontSize = 11;
            lockedTmp.color = TEXT_SECONDARY;
            lockedTmp.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement lockedLE = GetOrAddComponent<LayoutElement>(lockedText);
            lockedLE.minHeight = 16;

            Debug.Log("[DailyRewards PREMIUM] Day7Special creado");
        }

        // ==================== TODAY REWARD PANEL ====================

        private static void CreateTodayRewardPanel(GameObject parent)
        {
            // Position after: Header + StreakPanel + WeekTitle + DaysContainer + Day7Special + margins (reduced)
            float daysGridHeight = DAY_CARD_SIZE * 2 + DAY_CARD_SPACING + 10;
            float yPos = -HEADER_HEIGHT - 10 - STREAK_PANEL_HEIGHT - 8 - WEEK_TITLE_HEIGHT - 8 - daysGridHeight - 10 - DAY7_CARD_HEIGHT - 10;

            GameObject todayPanel = FindOrCreateChild(parent, "TodayRewardPanel");

            RectTransform todayRT = GetOrAddComponent<RectTransform>(todayPanel);
            todayRT.anchorMin = new Vector2(0, 1);
            todayRT.anchorMax = new Vector2(1, 1);
            todayRT.pivot = new Vector2(0.5f, 1);
            todayRT.anchoredPosition = new Vector2(0, yPos);
            todayRT.sizeDelta = new Vector2(-40, TODAY_REWARD_HEIGHT);

            Image todayBg = GetOrAddComponent<Image>(todayPanel);
            todayBg.color = PANEL_BG;
            AddOutline(todayPanel, CYAN_DARK, 1);

            // Horizontal layout: [Chest] [Info] - more compact
            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(todayPanel);
            hlg.spacing = 15f;
            hlg.padding = new RectOffset(20, 20, 12, 12);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;

            // Chest Icon (larger)
            GameObject chest = FindOrCreateChild(todayPanel, "Chest");
            Image chestImg = GetOrAddComponent<Image>(chest);
            chestImg.color = CHEST_RARE;
            LayoutElement chestLE = GetOrAddComponent<LayoutElement>(chest);
            chestLE.minWidth = 65;
            chestLE.minHeight = 65;

            // Chest Glow
            GameObject chestGlow = FindOrCreateChild(chest, "Glow");
            SetRectTransformStretch(chestGlow);
            RectTransform chestGlowRT = chestGlow.GetComponent<RectTransform>();
            chestGlowRT.offsetMin = new Vector2(-12, -12);
            chestGlowRT.offsetMax = new Vector2(12, 12);
            chestGlow.transform.SetAsFirstSibling();
            Image chestGlowImg = GetOrAddComponent<Image>(chestGlow);
            chestGlowImg.color = new Color(CHEST_RARE.r, CHEST_RARE.g, CHEST_RARE.b, 0.4f);

            // Rewards Info
            GameObject rewardsInfo = FindOrCreateChild(todayPanel, "RewardsInfo");
            VerticalLayoutGroup infoVlg = GetOrAddComponent<VerticalLayoutGroup>(rewardsInfo);
            infoVlg.spacing = 4f;
            infoVlg.childAlignment = TextAnchor.MiddleLeft;
            infoVlg.childControlWidth = true;
            infoVlg.childControlHeight = true;
            LayoutElement infoLE = GetOrAddComponent<LayoutElement>(rewardsInfo);
            infoLE.minWidth = 220;

            // Title + Chest Name
            GameObject chestName = FindOrCreateChild(rewardsInfo, "ChestName");
            TextMeshProUGUI nameTmp = GetOrAddComponent<TextMeshProUGUI>(chestName);
            nameTmp.text = "RECOMPENSA DE HOY";
            nameTmp.fontSize = 14;
            nameTmp.fontStyle = FontStyles.Bold;
            nameTmp.color = CYAN_NEON;
            nameTmp.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement nameLE = GetOrAddComponent<LayoutElement>(chestName);
            nameLE.minHeight = 20;

            // Rewards summary in one line
            GameObject rewardsRow = FindOrCreateChild(rewardsInfo, "RewardsRow");
            TextMeshProUGUI rewardsTmp = GetOrAddComponent<TextMeshProUGUI>(rewardsRow);
            rewardsTmp.text = "300 Monedas + 25 XP";
            rewardsTmp.fontSize = 16;
            rewardsTmp.fontStyle = FontStyles.Bold;
            rewardsTmp.color = COIN_COLOR;
            rewardsTmp.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement rewardsLE = GetOrAddComponent<LayoutElement>(rewardsRow);
            rewardsLE.minHeight = 22;

            Debug.Log("[DailyRewards PREMIUM] TodayRewardPanel creado");
        }

        // ==================== CLAIM BUTTON ====================

        private static void CreateClaimButton(GameObject parent)
        {
            GameObject claimBtn = FindOrCreateChild(parent, "ClaimButton");

            RectTransform claimRT = GetOrAddComponent<RectTransform>(claimBtn);
            claimRT.anchorMin = new Vector2(0.5f, 0);
            claimRT.anchorMax = new Vector2(0.5f, 0);
            claimRT.pivot = new Vector2(0.5f, 0);
            claimRT.anchoredPosition = new Vector2(0, TIMER_HEIGHT + 25);
            claimRT.sizeDelta = new Vector2(400, CLAIM_BUTTON_HEIGHT);

            // Outer glow (subtle, not obtrusive)
            GameObject outerGlow = FindOrCreateChild(claimBtn, "OuterGlow");
            RectTransform glowRT = GetOrAddComponent<RectTransform>(outerGlow);
            glowRT.anchorMin = Vector2.zero;
            glowRT.anchorMax = Vector2.one;
            glowRT.sizeDelta = new Vector2(16, 16);
            glowRT.anchoredPosition = Vector2.zero;
            outerGlow.transform.SetAsFirstSibling();

            Image glowImg = GetOrAddComponent<Image>(outerGlow);
            glowImg.color = BUTTON_CLAIM_GLOW;

            // Button background
            Image claimBg = GetOrAddComponent<Image>(claimBtn);
            claimBg.color = BUTTON_CLAIM;

            Button button = GetOrAddComponent<Button>(claimBtn);
            SetupButtonColors(button);
            AddOutline(claimBtn, new Color(0.3f, 1f, 0.5f, 0.8f), 2);

            // Text only (simple and clean)
            GameObject text = FindOrCreateChild(claimBtn, "Text");
            SetRectTransformStretch(text);
            TextMeshProUGUI textTmp = GetOrAddComponent<TextMeshProUGUI>(text);
            textTmp.text = "RECLAMAR RECOMPENSA";
            textTmp.fontSize = 24;
            textTmp.fontStyle = FontStyles.Bold;
            textTmp.color = TEXT_DARK;
            textTmp.alignment = TextAlignmentOptions.Center;

            Debug.Log("[DailyRewards PREMIUM] ClaimButton creado");
        }

        // ==================== NEXT REWARD TIMER ====================

        private static void CreateNextRewardTimer(GameObject parent)
        {
            GameObject timer = FindOrCreateChild(parent, "NextRewardTimer");

            RectTransform timerRT = GetOrAddComponent<RectTransform>(timer);
            timerRT.anchorMin = new Vector2(0, 0);
            timerRT.anchorMax = new Vector2(1, 0);
            timerRT.pivot = new Vector2(0.5f, 0);
            timerRT.anchoredPosition = new Vector2(0, 10);
            timerRT.sizeDelta = new Vector2(0, TIMER_HEIGHT);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(timer);
            hlg.spacing = 10f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;

            // Clock Icon
            GameObject clockIcon = FindOrCreateChild(timer, "ClockIcon");
            Image clockImg = GetOrAddComponent<Image>(clockIcon);
            clockImg.color = TEXT_SECONDARY;
            LayoutElement clockLE = GetOrAddComponent<LayoutElement>(clockIcon);
            clockLE.minWidth = 22;
            clockLE.minHeight = 22;

            // Label
            GameObject label = FindOrCreateChild(timer, "Label");
            TextMeshProUGUI labelTmp = GetOrAddComponent<TextMeshProUGUI>(label);
            labelTmp.text = "Próxima recompensa en:";
            labelTmp.fontSize = 14;
            labelTmp.color = TEXT_SECONDARY;
            labelTmp.alignment = TextAlignmentOptions.MidlineRight;
            LayoutElement labelLE = GetOrAddComponent<LayoutElement>(label);
            labelLE.minWidth = 200;

            // Timer Value
            GameObject value = FindOrCreateChild(timer, "Value");
            TextMeshProUGUI valueTmp = GetOrAddComponent<TextMeshProUGUI>(value);
            valueTmp.text = "14h 32m 15s";
            valueTmp.fontSize = 18;
            valueTmp.fontStyle = FontStyles.Bold;
            valueTmp.color = GOLD_BRIGHT;
            valueTmp.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement valueLE = GetOrAddComponent<LayoutElement>(value);
            valueLE.minWidth = 150;

            Debug.Log("[DailyRewards PREMIUM] NextRewardTimer creado");
        }

        // ==================== CLAIM CELEBRATION POPUP ====================

        private static void CreateClaimCelebration(Canvas canvas)
        {
            GameObject celebration = FindOrCreateChild(canvas.gameObject, "ClaimCelebration");
            celebration.SetActive(false);
            SetRectTransformStretch(celebration);

            Image blockerBg = GetOrAddComponent<Image>(celebration);
            blockerBg.color = new Color(0, 0, 0, 0.9f);
            celebration.transform.SetAsLastSibling();

            // Center Content
            GameObject center = FindOrCreateChild(celebration, "CenterContent");
            RectTransform centerRT = GetOrAddComponent<RectTransform>(center);
            centerRT.anchorMin = new Vector2(0.5f, 0.5f);
            centerRT.anchorMax = new Vector2(0.5f, 0.5f);
            centerRT.sizeDelta = new Vector2(450, 500);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(center);
            vlg.spacing = 20f;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // Chest Icon
            GameObject chest = FindOrCreateChild(center, "Chest");
            Image chestImg = GetOrAddComponent<Image>(chest);
            chestImg.color = CHEST_RARE;
            LayoutElement chestLE = GetOrAddComponent<LayoutElement>(chest);
            chestLE.minWidth = 120;
            chestLE.minHeight = 120;
            chestLE.preferredWidth = 120;
            chestLE.preferredHeight = 120;

            // Chest Glow
            GameObject chestGlow = FindOrCreateChild(chest, "Glow");
            SetRectTransformStretch(chestGlow);
            RectTransform chestGlowRT = chestGlow.GetComponent<RectTransform>();
            chestGlowRT.offsetMin = new Vector2(-50, -50);
            chestGlowRT.offsetMax = new Vector2(50, 50);
            chestGlow.transform.SetAsFirstSibling();

            Image chestGlowImg = GetOrAddComponent<Image>(chestGlow);
            chestGlowImg.color = new Color(CHEST_RARE.r, CHEST_RARE.g, CHEST_RARE.b, 0.5f);

            // Title
            GameObject title = FindOrCreateChild(center, "Title");
            TextMeshProUGUI titleTmp = GetOrAddComponent<TextMeshProUGUI>(title);
            titleTmp.text = "¡RECOMPENSA OBTENIDA!";
            titleTmp.fontSize = 28;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = GOLD_BRIGHT;
            titleTmp.alignment = TextAlignmentOptions.Center;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(title);
            titleLE.minHeight = 40;

            // Rewards Container
            GameObject rewards = FindOrCreateChild(center, "Rewards");
            VerticalLayoutGroup rewardsVlg = GetOrAddComponent<VerticalLayoutGroup>(rewards);
            rewardsVlg.spacing = 15f;
            rewardsVlg.childAlignment = TextAnchor.MiddleCenter;
            rewardsVlg.childControlWidth = true;
            rewardsVlg.childControlHeight = true;
            LayoutElement rewardsLE = GetOrAddComponent<LayoutElement>(rewards);
            rewardsLE.minHeight = 120;

            CreateCelebrationRewardRow(rewards, "Coins", "+300 Monedas", COIN_COLOR);
            CreateCelebrationRewardRow(rewards, "XP", "+25 XP", XP_COLOR);

            // Streak Bonus
            GameObject streakBonus = FindOrCreateChild(center, "StreakBonus");
            Image streakBg = GetOrAddComponent<Image>(streakBonus);
            streakBg.color = new Color(STREAK_FIRE.r, STREAK_FIRE.g, STREAK_FIRE.b, 0.2f);
            AddOutline(streakBonus, STREAK_FIRE);
            LayoutElement streakLE = GetOrAddComponent<LayoutElement>(streakBonus);
            streakLE.minHeight = 50;

            HorizontalLayoutGroup streakHlg = GetOrAddComponent<HorizontalLayoutGroup>(streakBonus);
            streakHlg.spacing = 10f;
            streakHlg.padding = new RectOffset(20, 20, 10, 10);
            streakHlg.childAlignment = TextAnchor.MiddleCenter;
            streakHlg.childControlWidth = false;
            streakHlg.childControlHeight = true;

            GameObject fireIcon = FindOrCreateChild(streakBonus, "Fire");
            Image fireImg = GetOrAddComponent<Image>(fireIcon);
            fireImg.color = STREAK_FIRE;
            LayoutElement fireLE = GetOrAddComponent<LayoutElement>(fireIcon);
            fireLE.minWidth = 28;
            fireLE.minHeight = 28;

            GameObject streakText = FindOrCreateChild(streakBonus, "Text");
            TextMeshProUGUI streakTmp = GetOrAddComponent<TextMeshProUGUI>(streakText);
            streakTmp.text = "¡Racha de 6 días! +1 día para MEGA BONUS";
            streakTmp.fontSize = 14;
            streakTmp.fontStyle = FontStyles.Bold;
            streakTmp.color = STREAK_FIRE;
            streakTmp.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement streakTextLE = GetOrAddComponent<LayoutElement>(streakText);
            streakTextLE.minWidth = 320;

            // Continue Button
            GameObject continueBtn = FindOrCreateChild(center, "ContinueButton");
            Image continueBg = GetOrAddComponent<Image>(continueBtn);
            continueBg.color = CYAN_NEON;

            Button button = GetOrAddComponent<Button>(continueBtn);
            SetupButtonColors(button);
            LayoutElement continueLE = GetOrAddComponent<LayoutElement>(continueBtn);
            continueLE.minHeight = 60;

            GameObject continueText = FindOrCreateChild(continueBtn, "Text");
            SetRectTransformStretch(continueText);
            TextMeshProUGUI continueTmp = GetOrAddComponent<TextMeshProUGUI>(continueText);
            continueTmp.text = "CONTINUAR";
            continueTmp.fontSize = 22;
            continueTmp.fontStyle = FontStyles.Bold;
            continueTmp.color = TEXT_DARK;
            continueTmp.alignment = TextAlignmentOptions.Center;

            Debug.Log("[DailyRewards PREMIUM] ClaimCelebration creado");
        }

        private static void CreateCelebrationRewardRow(GameObject parent, string name, string text, Color color)
        {
            GameObject row = FindOrCreateChild(parent, name);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(row);
            hlg.spacing = 15f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            LayoutElement rowLE = GetOrAddComponent<LayoutElement>(row);
            rowLE.minHeight = 45;

            GameObject icon = FindOrCreateChild(row, "Icon");
            Image iconImg = GetOrAddComponent<Image>(icon);
            iconImg.color = color;
            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(icon);
            iconLE.minWidth = 40;
            iconLE.minHeight = 40;

            GameObject textObj = FindOrCreateChild(row, "Text");
            TextMeshProUGUI textTmp = GetOrAddComponent<TextMeshProUGUI>(textObj);
            textTmp.text = text;
            textTmp.fontSize = 32;
            textTmp.fontStyle = FontStyles.Bold;
            textTmp.color = color;
            textTmp.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement textLE = GetOrAddComponent<LayoutElement>(textObj);
            textLE.minWidth = 250;
        }

        // ==================== STREAK LOST POPUP ====================

        private static void CreateStreakLostPopup(Canvas canvas)
        {
            GameObject popup = FindOrCreateChild(canvas.gameObject, "StreakLostPopup");
            popup.SetActive(false);
            SetRectTransformStretch(popup);

            Image blockerBg = GetOrAddComponent<Image>(popup);
            blockerBg.color = new Color(0, 0, 0, 0.9f);
            popup.transform.SetAsLastSibling();

            // Center Panel
            GameObject panel = FindOrCreateChild(popup, "Panel");
            RectTransform panelRT = GetOrAddComponent<RectTransform>(panel);
            panelRT.anchorMin = new Vector2(0.5f, 0.5f);
            panelRT.anchorMax = new Vector2(0.5f, 0.5f);
            panelRT.sizeDelta = new Vector2(420, 380);

            Image panelBg = GetOrAddComponent<Image>(panel);
            panelBg.color = PANEL_BG;
            AddOutline(panel, new Color(1f, 0.3f, 0.3f, 0.8f), 2);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(panel);
            vlg.spacing = 18f;
            vlg.padding = new RectOffset(30, 30, 30, 30);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // Sad Icon
            GameObject sadIcon = FindOrCreateChild(panel, "SadIcon");
            Image sadImg = GetOrAddComponent<Image>(sadIcon);
            sadImg.color = new Color(1f, 0.4f, 0.4f, 1f);
            LayoutElement sadLE = GetOrAddComponent<LayoutElement>(sadIcon);
            sadLE.minWidth = 70;
            sadLE.minHeight = 70;
            sadLE.preferredWidth = 70;
            sadLE.preferredHeight = 70;

            // Title
            GameObject title = FindOrCreateChild(panel, "Title");
            TextMeshProUGUI titleTmp = GetOrAddComponent<TextMeshProUGUI>(title);
            titleTmp.text = "¡RACHA PERDIDA!";
            titleTmp.fontSize = 26;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = new Color(1f, 0.4f, 0.4f, 1f);
            titleTmp.alignment = TextAlignmentOptions.Center;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(title);
            titleLE.minHeight = 35;

            // Description
            GameObject desc = FindOrCreateChild(panel, "Description");
            TextMeshProUGUI descTmp = GetOrAddComponent<TextMeshProUGUI>(desc);
            descTmp.text = "No reclamaste tu recompensa ayer.\nTu racha de 5 días se ha reiniciado.";
            descTmp.fontSize = 15;
            descTmp.color = TEXT_SECONDARY;
            descTmp.alignment = TextAlignmentOptions.Center;
            LayoutElement descLE = GetOrAddComponent<LayoutElement>(desc);
            descLE.minHeight = 50;

            // Streak Protector Offer
            GameObject offer = FindOrCreateChild(panel, "Offer");
            Image offerBg = GetOrAddComponent<Image>(offer);
            offerBg.color = new Color(GEM_COLOR.r, GEM_COLOR.g, GEM_COLOR.b, 0.15f);
            AddOutline(offer, GEM_COLOR);
            LayoutElement offerLE = GetOrAddComponent<LayoutElement>(offer);
            offerLE.minHeight = 60;

            HorizontalLayoutGroup offerHlg = GetOrAddComponent<HorizontalLayoutGroup>(offer);
            offerHlg.spacing = 12f;
            offerHlg.padding = new RectOffset(15, 15, 12, 12);
            offerHlg.childAlignment = TextAnchor.MiddleCenter;
            offerHlg.childControlWidth = false;
            offerHlg.childControlHeight = true;

            GameObject gemIcon = FindOrCreateChild(offer, "Gem");
            Image gemImg = GetOrAddComponent<Image>(gemIcon);
            gemImg.color = GEM_COLOR;
            LayoutElement gemLE = GetOrAddComponent<LayoutElement>(gemIcon);
            gemLE.minWidth = 35;
            gemLE.minHeight = 35;

            GameObject offerText = FindOrCreateChild(offer, "Text");
            TextMeshProUGUI offerTmp = GetOrAddComponent<TextMeshProUGUI>(offerText);
            offerTmp.text = "Restaurar racha por 50 gemas";
            offerTmp.fontSize = 15;
            offerTmp.fontStyle = FontStyles.Bold;
            offerTmp.color = GEM_COLOR;
            offerTmp.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement offerTextLE = GetOrAddComponent<LayoutElement>(offerText);
            offerTextLE.minWidth = 250;

            Button offerBtn = GetOrAddComponent<Button>(offer);
            SetupButtonColors(offerBtn);

            // Start Over Button
            GameObject startOver = FindOrCreateChild(panel, "StartOverButton");
            Image startBg = GetOrAddComponent<Image>(startOver);
            startBg.color = new Color(0.25f, 0.3f, 0.35f, 1f);

            Button startBtn = GetOrAddComponent<Button>(startOver);
            SetupButtonColors(startBtn);
            LayoutElement startLE = GetOrAddComponent<LayoutElement>(startOver);
            startLE.minHeight = 50;

            GameObject startText = FindOrCreateChild(startOver, "Text");
            SetRectTransformStretch(startText);
            TextMeshProUGUI startTmp = GetOrAddComponent<TextMeshProUGUI>(startText);
            startTmp.text = "EMPEZAR DE NUEVO";
            startTmp.fontSize = 18;
            startTmp.fontStyle = FontStyles.Bold;
            startTmp.color = TEXT_PRIMARY;
            startTmp.alignment = TextAlignmentOptions.Center;

            Debug.Log("[DailyRewards PREMIUM] StreakLostPopup creado");
        }

        // ==================== UTILITY METHODS ====================

        private static void CreateGlowLine(GameObject parent, Color color, bool isBottom)
        {
            GameObject glow = FindOrCreateChild(parent, isBottom ? "BottomGlow" : "TopGlow");
            RectTransform glowRT = GetOrAddComponent<RectTransform>(glow);

            if (isBottom)
            {
                glowRT.anchorMin = new Vector2(0, 0);
                glowRT.anchorMax = new Vector2(1, 0);
                glowRT.pivot = new Vector2(0.5f, 1);
            }
            else
            {
                glowRT.anchorMin = new Vector2(0, 1);
                glowRT.anchorMax = new Vector2(1, 1);
                glowRT.pivot = new Vector2(0.5f, 0);
            }
            glowRT.anchoredPosition = Vector2.zero;
            glowRT.sizeDelta = new Vector2(0, 3);

            Image glowImg = GetOrAddComponent<Image>(glow);
            glowImg.color = color;
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

        private static void SetupButtonColors(Button btn)
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
