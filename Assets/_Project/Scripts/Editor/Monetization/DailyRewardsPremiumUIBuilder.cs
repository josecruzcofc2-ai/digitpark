using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;
using DigitPark.Monetization;

namespace DigitPark.Editor
{
    /// <summary>
    /// Daily Rewards Premium UI Builder - Neon Cyan + Gold accents
    /// Layout: TopBar -> StreakPanel -> WeekLabel -> DaysGrid(3x2) -> Day7Card -> TodayPanel -> ClaimButton -> Timer
    /// Popups: ClaimAnimationBlocker, MilestoneBlocker
    /// Portrait 9:16 (1080x1920), matchWidthOrHeight=0
    ///
    /// Menu: DigitPark/UI Builders/Monetization/Daily Rewards (priority 185)
    /// </summary>
    public class DailyRewardsPremiumUIBuilder : EditorWindow
    {
        #region Colors

        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.5f, 1f);
        private static readonly Color CYAN_GLOW = new Color(0f, 1f, 1f, 0.06f);

        private static readonly Color DARK_BG = new Color(0.02f, 0.04f, 0.08f, 1f);
        private static readonly Color CARD_BG = new Color(0.06f, 0.08f, 0.12f, 1f);
        private static readonly Color CARD_BG_LIGHT = new Color(0.08f, 0.10f, 0.14f, 1f);

        private static readonly Color TEXT_WHITE = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.6f, 0.65f, 1f);
        private static readonly Color TEXT_DARK = new Color(0.05f, 0.05f, 0.08f, 1f);

        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GOLD_DARK = new Color(0.6f, 0.5f, 0f, 1f);

        private static readonly Color GREEN_SUCCESS = new Color(0.2f, 0.9f, 0.4f, 1f);
        private static readonly Color GREEN_CLAIMED = new Color(0.15f, 0.5f, 0.25f, 1f);

        private static readonly Color ORANGE_FIRE = new Color(1f, 0.5f, 0.1f, 1f);

        private static readonly Color LOCKED_OVERLAY = new Color(0.03f, 0.04f, 0.07f, 0.85f);

        private static readonly Color COIN_COLOR = new Color(1f, 0.85f, 0.3f, 1f);
        private static readonly Color GEM_COLOR = new Color(0.4f, 0.8f, 1f, 1f);
        private static readonly Color XP_COLOR = new Color(0.5f, 1f, 0.5f, 1f);

        #endregion

        #region Layout Anchors (Y: 0=bottom, 1=top)

        private const float TOPBAR_TOP = 0.990f;
        private const float TOPBAR_BOT = 0.957f;

        private const float STREAK_TOP = 0.950f;
        private const float STREAK_BOT = 0.910f;

        private const float WEEK_TOP = 0.903f;
        private const float WEEK_BOT = 0.880f;

        private const float DAYS_TOP = 0.873f;
        private const float DAYS_BOT = 0.570f;

        private const float DAY7_TOP = 0.558f;
        private const float DAY7_BOT = 0.455f;

        private const float TODAY_TOP = 0.443f;
        private const float TODAY_BOT = 0.370f;

        private const float CLAIM_TOP = 0.350f;
        private const float CLAIM_BOT = 0.290f;

        private const float TIMER_TOP = 0.280f;
        private const float TIMER_BOT = 0.248f;

        private const float SIDE_PAD = 25f;

        #endregion

        #region Icon Paths

        private const string ICONS_BASE = "Assets/_Project/Art/Icons/";
        private const string DAILY_ICONS = ICONS_BASE + "DailyRewards/";
        private const string CURRENCY_ICONS = ICONS_BASE + "Currency/";
        private const string UI_ICONS = ICONS_BASE + "UI/";
        private const string NAV_ICONS = ICONS_BASE + "Navigation/Actions/";
        private const string BACK_BUTTON_PREFAB = "Assets/_Project/Prefabs/Common/BackButton.prefab";

        #endregion

        [MenuItem("DigitPark/UI Builders/Monetization/Daily Rewards", false, 143)]
        public static void ShowWindow()
        {
            GetWindow<DailyRewardsPremiumUIBuilder>("Daily Rewards Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Daily Rewards Premium UI Builder", EditorStyles.boldLabel);
            GUILayout.Label("Neon Cyan + Gold accents - 7-day cycle", EditorStyles.miniLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Layout (de arriba a abajo):\n\n" +
                "1. TopBar (Back + titulo + currency pills)\n" +
                "2. Streak Panel (racha + progress bar)\n" +
                "3. Week Label (SEMANA 1)\n" +
                "4. Days Grid 3x2 (dias 1-6)\n" +
                "5. Day 7 Mega Card (premio especial)\n" +
                "6. Today Panel (recompensa de hoy)\n" +
                "7. Claim Button (reclamar)\n" +
                "8. Timer (proxima recompensa)\n" +
                "9. Claim Animation Popup (hidden)\n" +
                "10. Milestone Popup (hidden)",
                MessageType.Info);

            GUILayout.Space(15);

            GUI.backgroundColor = CYAN_NEON;
            if (GUILayout.Button("RECONSTRUIR RECOMPENSAS COMPLETO", GUILayout.Height(50)))
                RebuildDailyRewards();
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);
            GUILayout.Label("Secciones individuales:", EditorStyles.boldLabel);

            if (GUILayout.Button("1. Background + TopBar", GUILayout.Height(25)))
            {
                Canvas c = UIBuilderCanvasHelper.FindMainCanvas();
                if (c != null) { CreateBackground(c.transform); CreateTopBar(); }
            }
            if (GUILayout.Button("2. Streak Panel", GUILayout.Height(25))) CreateStreakPanel();
            if (GUILayout.Button("3. Week Label", GUILayout.Height(25))) CreateWeekLabel();
            if (GUILayout.Button("4. Days Grid (1-6)", GUILayout.Height(25))) CreateDaysGrid();
            if (GUILayout.Button("5. Day 7 Mega Card", GUILayout.Height(25))) CreateDay7Card();
            if (GUILayout.Button("6. Today Panel", GUILayout.Height(25))) CreateTodayPanel();
            if (GUILayout.Button("7. Claim Button", GUILayout.Height(25))) CreateClaimButton();
            if (GUILayout.Button("8. Timer", GUILayout.Height(25))) CreateTimer();
            if (GUILayout.Button("9. Claim Animation Popup", GUILayout.Height(25))) CreateClaimAnimationPopup();
            if (GUILayout.Button("10. Milestone Popup", GUILayout.Height(25))) CreateMilestonePopup();

            GUILayout.Space(15);

            GUI.backgroundColor = GOLD;
            if (GUILayout.Button("ASIGNAR REFERENCIAS AL MANAGER", GUILayout.Height(35)))
                SetupManagerReferences();
            GUI.backgroundColor = Color.white;
        }

        #region Main Rebuild

        private static void RebuildDailyRewards()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("[DailyRewardsUI] No se encontro Canvas");
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
                "Background", "SafeArea", "StreakPanel", "WeekLabel",
                "DaysGrid", "Day7Card", "TodayPanel", "ClaimButton",
                "ClaimGlow", "TimerBar", "TopBar",
                "ClaimAnimationBlocker", "MilestoneBlocker",
                // Old names from previous version
                "Header", "DaysContainer", "Day7Special", "TodayRewardPanel",
                "NextRewardTimer", "ClaimCelebration", "StreakLostPopup",
                "RewardClaimBlocker", "WeekTitle", "Inner"
            };
            foreach (var n in oldNames)
            {
                Transform t = canvas.transform.Find(n);
                if (t != null) DestroyImmediate(t.gameObject);
            }

            CreateBackground(canvas.transform);
            CreateTopBar();
            CreateStreakPanel();
            CreateWeekLabel();
            CreateDaysGrid();
            CreateDay7Card();
            CreateTodayPanel();
            CreateClaimButton();
            CreateTimer();
            CreateClaimAnimationPopup();
            CreateMilestonePopup();
            SetupManagerReferences();

            Debug.Log("[DailyRewardsUI] Recompensas diarias RECONSTRUIDAS exitosamente!");
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

        #region 1. TopBar (0.960-0.988)

        private static void CreateTopBar()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var topBar = FindOrCreate(canvas.transform, "TopBar");
            var tbRT = GetOrAdd<RectTransform>(topBar);
            SetAnchors(tbRT, 0, TOPBAR_BOT, 1, TOPBAR_TOP);

            // --- BackButton (left, 50x50) - Neon Cyan prefab ---
            // Remove old manual BackButton if exists
            var oldBackBtn = topBar.transform.Find("BackButton");
            if (oldBackBtn != null) Object.DestroyImmediate(oldBackBtn.gameObject);

            GameObject backBtnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BACK_BUTTON_PREFAB);
            GameObject backBtn;
            if (backBtnPrefab != null)
            {
                backBtn = (GameObject)PrefabUtility.InstantiatePrefab(backBtnPrefab, topBar.transform);
                backBtn.name = "BackButton";
            }
            else
            {
                backBtn = FindOrCreate(topBar.transform, "BackButton");
                GetOrAdd<Image>(backBtn).color = CARD_BG;
                GetOrAdd<Button>(backBtn);
                Debug.LogWarning("[DailyRewardsUI] BackButton prefab not found, using fallback");
            }
            var bbRT = GetOrAdd<RectTransform>(backBtn);
            bbRT.anchorMin = new Vector2(0, 0.5f);
            bbRT.anchorMax = new Vector2(0, 0.5f);
            bbRT.pivot = new Vector2(0, 0.5f);
            bbRT.anchoredPosition = new Vector2(20, 0);
            bbRT.sizeDelta = new Vector2(50, 50);

            // --- TitleText (center) ---
            var title = FindOrCreate(topBar.transform, "TitleText");
            var tRT = GetOrAdd<RectTransform>(title);
            tRT.anchorMin = new Vector2(0.07f, 0f);
            tRT.anchorMax = new Vector2(0.53f, 1f);
            tRT.pivot = new Vector2(0.5f, 0.5f);
            tRT.sizeDelta = Vector2.zero;
            tRT.anchoredPosition = Vector2.zero;
            var tTMP = GetOrAdd<TextMeshProUGUI>(title);
            tTMP.text = "DAILY REWARDS";
            tTMP.enableAutoSizing = true;
            tTMP.fontSizeMin = FontSizes.AutoMinTitle;
            tTMP.fontSizeMax = FontSizes.H4;
            tTMP.overflowMode = TextOverflowModes.Ellipsis;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.color = CYAN_NEON;
            tTMP.alignment = TextAlignmentOptions.Center;
            tTMP.raycastTarget = false;

            // --- Currency pills (right) ---
            var currencyRow = CurrencyHeaderBarHelper.CreateCurrencyPills(topBar.transform, "CurrencyRow");
            var crRT = currencyRow.GetComponent<RectTransform>();
            crRT.anchorMin = new Vector2(1, 0.5f);
            crRT.anchorMax = new Vector2(1, 0.5f);
            crRT.pivot = new Vector2(1, 0.5f);
            crRT.anchoredPosition = new Vector2(-SIDE_PAD, 0);
            crRT.sizeDelta = new Vector2(310, 50);

            Debug.Log("[DailyRewardsUI] TopBar creado (BackButton + Title + CurrencyPills)");
        }

        #endregion

        #region 2. Streak Panel (compact)

        private static void CreateStreakPanel()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            // Remove old Inner wrapper if exists
            var streak = FindOrCreate(canvas.transform, "StreakPanel");
            var oldInner = streak.transform.Find("Inner");
            if (oldInner != null) Object.DestroyImmediate(oldInner.gameObject);

            var sRT = GetOrAdd<RectTransform>(streak);
            SetAnchors(sRT, NormX(SIDE_PAD), STREAK_BOT, NormX(1080 - SIDE_PAD), STREAK_TOP);

            var sBg = GetOrAdd<Image>(streak);
            sBg.color = CARD_BG;
            var sOutline = GetOrAdd<Outline>(streak);
            sOutline.effectColor = new Color(ORANGE_FIRE.r, ORANGE_FIRE.g, ORANGE_FIRE.b, 0.3f);
            sOutline.effectDistance = new Vector2(1, 1);

            // Compact VLG: Row1 (fire+text+progress) + Row2 (bonus text)
            var vlg = GetOrAdd<VerticalLayoutGroup>(streak);
            vlg.spacing = 4;
            vlg.padding = new RectOffset(15, 15, 6, 6);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // --- Row 1: [FireIcon] RACHA: [5 DIAS] [===progress===] ---
            var topRow = FindOrCreate(streak.transform, "TopRow");
            GetOrAdd<LayoutElement>(topRow).preferredHeight = 36;
            var trHLG = GetOrAdd<HorizontalLayoutGroup>(topRow);
            trHLG.spacing = 8;
            trHLG.childAlignment = TextAnchor.MiddleCenter;
            trHLG.childControlWidth = false;
            trHLG.childControlHeight = true;

            var fireIcon = FindOrCreate(topRow.transform, "FireIcon");
            var fireLE = GetOrAdd<LayoutElement>(fireIcon);
            fireLE.minWidth = 30;
            fireLE.minHeight = 30;
            Sprite fireSprite = AssetDatabase.LoadAssetAtPath<Sprite>(DAILY_ICONS + "icon_daily_streak.png");
            if (fireSprite != null)
            {
                var fireImg = GetOrAdd<Image>(fireIcon);
                fireImg.preserveAspect = true;
                fireImg.color = Color.white;
                fireImg.sprite = fireSprite;
            }
            else
            {
                // Fallback: text emoji instead of colored rectangle
                var fireTMP = GetOrAdd<TextMeshProUGUI>(fireIcon);
                fireTMP.text = "\U0001F525";
                fireTMP.fontSize = FontSizes.Body;
                fireTMP.alignment = TextAlignmentOptions.Center;
            }

            var streakLabel = FindOrCreate(topRow.transform, "StreakLabel");
            var slTMP = GetOrAdd<TextMeshProUGUI>(streakLabel);
            slTMP.text = "STREAK:";
            slTMP.fontSize = FontSizes.Body;
            slTMP.fontStyle = FontStyles.Bold;
            slTMP.color = TEXT_WHITE;
            slTMP.alignment = TextAlignmentOptions.MidlineLeft;
            var slLE = GetOrAdd<LayoutElement>(streakLabel);
            slLE.minWidth = 120;

            var streakCount = FindOrCreate(topRow.transform, "StreakCount");
            var scTMP = GetOrAdd<TextMeshProUGUI>(streakCount);
            scTMP.text = "5 DAYS";
            scTMP.fontSize = FontSizes.Subtitle;
            scTMP.fontStyle = FontStyles.Bold;
            scTMP.color = ORANGE_FIRE;
            scTMP.alignment = TextAlignmentOptions.MidlineLeft;
            var scLE = GetOrAdd<LayoutElement>(streakCount);
            scLE.minWidth = 120;

            // Inline progress bar
            var progressBar = FindOrCreate(topRow.transform, "StreakProgressBar");
            var pbLE = GetOrAdd<LayoutElement>(progressBar);
            pbLE.flexibleWidth = 1;
            pbLE.minHeight = 14;
            pbLE.preferredHeight = 14;

            var slider = GetOrAdd<Slider>(progressBar);
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0;
            slider.maxValue = 7;
            slider.wholeNumbers = true;
            slider.value = 5;
            slider.interactable = false;

            var sliderBg = FindOrCreate(progressBar.transform, "Background");
            var sbgRT = GetOrAdd<RectTransform>(sliderBg);
            sbgRT.anchorMin = Vector2.zero;
            sbgRT.anchorMax = Vector2.one;
            sbgRT.offsetMin = Vector2.zero;
            sbgRT.offsetMax = Vector2.zero;
            GetOrAdd<Image>(sliderBg).color = new Color(0.1f, 0.12f, 0.15f, 1f);

            var fillArea = FindOrCreate(progressBar.transform, "Fill Area");
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
            GetOrAdd<Image>(fill).color = ORANGE_FIRE;

            slider.fillRect = fRT;
            slider.handleRect = null;
            slider.targetGraphic = GetOrAdd<Image>(progressBar);
            GetOrAdd<Image>(progressBar).color = Color.clear;

            // --- Row 2: Bonus text (compact) ---
            var bonusText = FindOrCreate(streak.transform, "BonusText");
            GetOrAdd<LayoutElement>(bonusText).preferredHeight = 28;
            var btTMP = GetOrAdd<TextMeshProUGUI>(bonusText);
            btTMP.text = "Day 7 bonus: +100 DigitGems";
            btTMP.fontSize = FontSizes.Body;
            btTMP.fontStyle = FontStyles.Bold;
            btTMP.color = GEM_COLOR;
            btTMP.alignment = TextAlignmentOptions.Center;
            btTMP.overflowMode = TextOverflowModes.Ellipsis;

            Debug.Log("[DailyRewardsUI] StreakPanel creado (compacto)");
        }

        #endregion

        #region 3. Week Label (0.865-0.890)

        private static void CreateWeekLabel()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var weekLabel = FindOrCreate(canvas.transform, "WeekLabel");
            var wRT = GetOrAdd<RectTransform>(weekLabel);
            SetAnchors(wRT, 0.05f, WEEK_BOT, 0.95f, WEEK_TOP);

            // Remove any leftover Image component (was causing orange bg)
            var oldImg = weekLabel.GetComponent<Image>();
            if (oldImg != null) Object.DestroyImmediate(oldImg);

            var wTMP = GetOrAdd<TextMeshProUGUI>(weekLabel);
            wTMP.text = "\u2500\u2500\u2500 WEEK 1 \u2500\u2500\u2500";
            wTMP.fontSize = FontSizes.Body;
            wTMP.fontStyle = FontStyles.Bold;
            wTMP.color = GOLD;
            wTMP.alignment = TextAlignmentOptions.Center;

            Debug.Log("[DailyRewardsUI] WeekLabel creado");
        }

        #endregion

        #region 4. Days Grid (0.555-0.860)

        private static void CreateDaysGrid()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var daysGrid = FindOrCreate(canvas.transform, "DaysGrid");
            var dgRT = GetOrAdd<RectTransform>(daysGrid);
            SetAnchors(dgRT, NormX(SIDE_PAD), DAYS_BOT, NormX(1080 - SIDE_PAD), DAYS_TOP);

            var grid = GetOrAdd<GridLayoutGroup>(daysGrid);
            grid.cellSize = new Vector2(310, 200);
            grid.spacing = new Vector2(15, 12);
            grid.padding = new RectOffset(15, 15, 15, 15);
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;

            // Sample data for days 1-6
            var dayData = new (int day, string type, int amount, string name, int state)[]
            {
                (1, "digitcoins", 100, "DigitCoins", 0),   // 0 = CLAIMED
                (2, "digitcoins", 150, "DigitCoins", 0),
                (3, "digitgems",  25,  "DigitGems",   0),
                (4, "digitcoins", 200, "DigitCoins", 0),
                (5, "digitcoins", 300, "DigitCoins", 1),   // 1 = CURRENT
                (6, "xp",    25,  "XP",      2),   // 2 = LOCKED
            };

            // Clear old day cards
            while (daysGrid.transform.childCount > 0)
                DestroyImmediate(daysGrid.transform.GetChild(0).gameObject);

            foreach (var d in dayData)
            {
                CreateDayCard(daysGrid.transform, d.day, d.type, d.amount, d.name, d.state);
            }

            Debug.Log("[DailyRewardsUI] DaysGrid creado con 6 dias (3x2)");
        }

        private static void CreateDayCard(Transform parent, int day, string type, int amount, string typeName, int state)
        {
            // state: 0=CLAIMED, 1=CURRENT, 2=LOCKED
            bool claimed = state == 0;
            bool current = state == 1;
            bool locked  = state == 2;

            var card = new GameObject($"Day{day}");
            card.transform.SetParent(parent, false);
            card.AddComponent<RectTransform>();

            // Card background
            var cardBg = card.AddComponent<Image>();
            cardBg.color = claimed ? GREEN_CLAIMED : CARD_BG;

            // Outline
            var outline = card.AddComponent<Outline>();
            if (claimed)
            {
                outline.effectColor = GREEN_SUCCESS;
                outline.effectDistance = new Vector2(1, 1);
            }
            else if (current)
            {
                outline.effectColor = GOLD;
                outline.effectDistance = new Vector2(2, 2);
            }
            else
            {
                outline.effectColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
                outline.effectDistance = new Vector2(1, 1);
            }

            // 3D depth shadow
            var cardShadow = card.AddComponent<Shadow>();
            cardShadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            cardShadow.effectDistance = new Vector2(3, -4);

            // VLG for card content
            var vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 4;
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Day Label
            var dayLabel = new GameObject("DayLabel");
            dayLabel.transform.SetParent(card.transform, false);
            dayLabel.AddComponent<RectTransform>();
            dayLabel.AddComponent<LayoutElement>().preferredHeight = 38;
            var dlTMP = dayLabel.AddComponent<TextMeshProUGUI>();
            dlTMP.text = $"DAY {day}";
            dlTMP.fontSize = FontSizes.Body;
            dlTMP.fontStyle = FontStyles.Bold;
            dlTMP.color = claimed ? GREEN_SUCCESS : (current ? GOLD : TEXT_WHITE);
            dlTMP.alignment = TextAlignmentOptions.Center;

            // Reward Icon
            var iconContainer = new GameObject("RewardIcon");
            iconContainer.transform.SetParent(card.transform, false);
            iconContainer.AddComponent<RectTransform>();
            var iconLE = iconContainer.AddComponent<LayoutElement>();
            iconLE.preferredHeight = 50;
            iconLE.preferredWidth = 50;
            var iconImg = iconContainer.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.color = Color.white;

            // Load day-specific icon
            Sprite daySprite = AssetDatabase.LoadAssetAtPath<Sprite>(DAILY_ICONS + $"icon_daily_reward_day{day}.png");
            if (daySprite != null) iconImg.sprite = daySprite;
            else
            {
                // Fallback color based on type
                Color fallbackColor = type switch
                {
                    "digitcoins" => COIN_COLOR,
                    "digitgems" => GEM_COLOR,
                    "xp" => XP_COLOR,
                    _ => TEXT_WHITE
                };
                iconImg.color = claimed ? new Color(fallbackColor.r, fallbackColor.g, fallbackColor.b, 0.5f) : fallbackColor;
            }

            // Gold glow behind icon for current day
            if (current)
            {
                var iconGlow = new GameObject("IconGlow");
                iconGlow.transform.SetParent(iconContainer.transform, false);
                iconGlow.transform.SetAsFirstSibling();
                var igRT = iconGlow.AddComponent<RectTransform>();
                igRT.anchorMin = Vector2.zero;
                igRT.anchorMax = Vector2.one;
                igRT.offsetMin = new Vector2(-10, -10);
                igRT.offsetMax = new Vector2(10, 10);
                iconGlow.AddComponent<Image>().color = new Color(GOLD.r, GOLD.g, GOLD.b, 0.15f);
            }

            // Amount Text
            var amountObj = new GameObject("AmountText");
            amountObj.transform.SetParent(card.transform, false);
            amountObj.AddComponent<RectTransform>();
            amountObj.AddComponent<LayoutElement>().preferredHeight = 38;
            var amTMP = amountObj.AddComponent<TextMeshProUGUI>();
            amTMP.text = $"+{amount}";
            amTMP.fontSize = FontSizes.Body;
            amTMP.fontStyle = FontStyles.Bold;
            amTMP.color = claimed ? new Color(1, 1, 1, 0.5f) : TEXT_WHITE;
            amTMP.alignment = TextAlignmentOptions.Center;
            amTMP.overflowMode = TextOverflowModes.Ellipsis;

            // Status overlays
            if (claimed)
            {
                // Green check overlay
                var check = new GameObject("CheckOverlay");
                check.transform.SetParent(card.transform, false);
                var chRT = check.AddComponent<RectTransform>();
                chRT.anchorMin = new Vector2(1, 1);
                chRT.anchorMax = new Vector2(1, 1);
                chRT.pivot = new Vector2(1, 1);
                chRT.anchoredPosition = new Vector2(-4, -4);
                chRT.sizeDelta = new Vector2(26, 26);
                check.AddComponent<Image>().color = GREEN_SUCCESS;

                var checkText = new GameObject("Text");
                checkText.transform.SetParent(check.transform, false);
                var ctRT = checkText.AddComponent<RectTransform>();
                ctRT.anchorMin = Vector2.zero;
                ctRT.anchorMax = Vector2.one;
                ctRT.offsetMin = Vector2.zero;
                ctRT.offsetMax = Vector2.zero;
                var ctTMP = checkText.AddComponent<TextMeshProUGUI>();
                ctTMP.text = "\u2713";
                ctTMP.fontSize = FontSizes.Body;
                ctTMP.fontStyle = FontStyles.Bold;
                ctTMP.color = TEXT_DARK;
                ctTMP.alignment = TextAlignmentOptions.Center;
            }
            else if (current)
            {
                // HOY badge
                var badge = new GameObject("TodayBadge");
                badge.transform.SetParent(card.transform, false);
                var bdRT = badge.AddComponent<RectTransform>();
                bdRT.anchorMin = new Vector2(0.5f, 1);
                bdRT.anchorMax = new Vector2(0.5f, 1);
                bdRT.pivot = new Vector2(0.5f, 1);
                bdRT.anchoredPosition = new Vector2(0, 2);
                bdRT.sizeDelta = new Vector2(80, 22);
                badge.AddComponent<Image>().color = GOLD;

                var badgeText = new GameObject("Text");
                badgeText.transform.SetParent(badge.transform, false);
                var bttRT = badgeText.AddComponent<RectTransform>();
                bttRT.anchorMin = Vector2.zero;
                bttRT.anchorMax = Vector2.one;
                bttRT.offsetMin = Vector2.zero;
                bttRT.offsetMax = Vector2.zero;
                var bttTMP = badgeText.AddComponent<TextMeshProUGUI>();
                bttTMP.text = "TODAY";
                bttTMP.fontSize = FontSizes.Body;
                bttTMP.fontStyle = FontStyles.Bold;
                bttTMP.color = TEXT_DARK;
                bttTMP.alignment = TextAlignmentOptions.Center;
            }
            else if (locked)
            {
                // Locked overlay
                var lockOverlay = new GameObject("LockOverlay");
                lockOverlay.transform.SetParent(card.transform, false);
                var loRT = lockOverlay.AddComponent<RectTransform>();
                loRT.anchorMin = Vector2.zero;
                loRT.anchorMax = Vector2.one;
                loRT.offsetMin = Vector2.zero;
                loRT.offsetMax = Vector2.zero;
                lockOverlay.AddComponent<Image>().color = LOCKED_OVERLAY;

                // Lock icon
                var lockIcon = new GameObject("LockIcon");
                lockIcon.transform.SetParent(lockOverlay.transform, false);
                var liRT = lockIcon.AddComponent<RectTransform>();
                liRT.anchorMin = new Vector2(0.5f, 0.5f);
                liRT.anchorMax = new Vector2(0.5f, 0.5f);
                liRT.sizeDelta = new Vector2(30, 30);
                var liImg = lockIcon.AddComponent<Image>();
                liImg.preserveAspect = true;
                Sprite lockSprite = AssetDatabase.LoadAssetAtPath<Sprite>(NAV_ICONS + "LockIcon.png");
                if (lockSprite != null) { liImg.sprite = lockSprite; liImg.color = Color.white; }
                else liImg.color = TEXT_SECONDARY;
            }
        }

        #endregion

        #region 5. Day 7 Mega Card (0.410-0.545)

        private static void CreateDay7Card()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            // GoldGlow behind card
            var goldGlow = FindOrCreate(canvas.transform, "Day7Glow");
            var ggRT = GetOrAdd<RectTransform>(goldGlow);
            float glowPad = SIDE_PAD - 5;
            SetAnchors(ggRT, NormX(glowPad), DAY7_BOT - 0.005f, NormX(1080 - glowPad), DAY7_TOP + 0.005f);
            GetOrAdd<Image>(goldGlow).color = new Color(GOLD.r, GOLD.g, GOLD.b, 0.08f);

            var day7 = FindOrCreate(canvas.transform, "Day7Card");
            var d7RT = GetOrAdd<RectTransform>(day7);
            SetAnchors(d7RT, NormX(SIDE_PAD), DAY7_BOT, NormX(1080 - SIDE_PAD), DAY7_TOP);

            var d7Bg = GetOrAdd<Image>(day7);
            d7Bg.color = CARD_BG;
            var d7Outline = GetOrAdd<Outline>(day7);
            d7Outline.effectColor = GOLD;
            d7Outline.effectDistance = new Vector2(2, 2);

            // Shadow (3D depth behind card)
            var day7Shadow = FindOrCreate(day7.transform, "Shadow");
            day7Shadow.transform.SetAsFirstSibling();
            var dsShadowRT = GetOrAdd<RectTransform>(day7Shadow);
            dsShadowRT.anchorMin = Vector2.zero;
            dsShadowRT.anchorMax = Vector2.one;
            dsShadowRT.offsetMin = new Vector2(8, -10);
            dsShadowRT.offsetMax = Vector2.zero;
            var dsShadowImg = GetOrAdd<Image>(day7Shadow);
            dsShadowImg.color = new Color(0f, 0f, 0f, 0.5f);
            dsShadowImg.raycastTarget = false;
            var dsShadowLE = GetOrAdd<LayoutElement>(day7Shadow);
            dsShadowLE.ignoreLayout = true;

            // Side (3D depth strip below card)
            var day7Side = FindOrCreate(day7.transform, "Side");
            day7Side.transform.SetSiblingIndex(1);
            var dsSideRT = GetOrAdd<RectTransform>(day7Side);
            dsSideRT.anchorMin = new Vector2(0, 0);
            dsSideRT.anchorMax = new Vector2(1, 0);
            dsSideRT.offsetMin = new Vector2(0, -8);
            dsSideRT.offsetMax = new Vector2(0, 0);
            var dsSideImg = GetOrAdd<Image>(day7Side);
            dsSideImg.color = new Color(GOLD.r * 0.3f, GOLD.g * 0.3f, GOLD.b * 0.3f, 1f);
            dsSideImg.raycastTarget = false;
            var dsSideLE = GetOrAdd<LayoutElement>(day7Side);
            dsSideLE.ignoreLayout = true;

            // HLG content
            var hlg = GetOrAdd<HorizontalLayoutGroup>(day7);
            hlg.spacing = 15;
            hlg.padding = new RectOffset(20, 20, 15, 15);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            // Left: Icon area
            var iconArea = FindOrCreate(day7.transform, "IconArea");
            var iaLE = GetOrAdd<LayoutElement>(iconArea);
            iaLE.minWidth = 80;
            iaLE.preferredWidth = 80;
            iaLE.minHeight = 80;

            // Icon glow
            var iconGlow = FindOrCreate(iconArea.transform, "IconGlow");
            var igRT = GetOrAdd<RectTransform>(iconGlow);
            igRT.anchorMin = new Vector2(0.5f, 0.5f);
            igRT.anchorMax = new Vector2(0.5f, 0.5f);
            igRT.sizeDelta = new Vector2(70, 70);
            GetOrAdd<Image>(iconGlow).color = new Color(GOLD.r, GOLD.g, GOLD.b, 0.15f);

            // Day7 icon
            var day7Icon = FindOrCreate(iconArea.transform, "Day7Icon");
            var d7iRT = GetOrAdd<RectTransform>(day7Icon);
            d7iRT.anchorMin = new Vector2(0.5f, 0.5f);
            d7iRT.anchorMax = new Vector2(0.5f, 0.5f);
            d7iRT.sizeDelta = new Vector2(60, 60);
            var d7iImg = GetOrAdd<Image>(day7Icon);
            d7iImg.preserveAspect = true;
            d7iImg.color = Color.white;
            Sprite d7Sprite = AssetDatabase.LoadAssetAtPath<Sprite>(DAILY_ICONS + "icon_daily_reward_day7.png");
            if (d7Sprite != null) d7iImg.sprite = d7Sprite;
            else d7iImg.color = GOLD;

            // Right: Info VLG
            var info = FindOrCreate(day7.transform, "Info");
            var infoVLG = GetOrAdd<VerticalLayoutGroup>(info);
            infoVLG.spacing = 6;
            infoVLG.childAlignment = TextAnchor.MiddleLeft;
            infoVLG.childControlWidth = true;
            infoVLG.childControlHeight = false;
            infoVLG.childForceExpandWidth = true;
            var infoLE = GetOrAdd<LayoutElement>(info);
            infoLE.flexibleWidth = 1;

            var d7Title = FindOrCreate(info.transform, "DayLabel");
            GetOrAdd<LayoutElement>(d7Title).preferredHeight = 38;
            var d7tTMP = GetOrAdd<TextMeshProUGUI>(d7Title);
            d7tTMP.text = "DAY 7 - GRAND PRIZE";
            d7tTMP.fontSize = FontSizes.Body;
            d7tTMP.fontStyle = FontStyles.Bold;
            d7tTMP.color = GOLD;
            d7tTMP.alignment = TextAlignmentOptions.MidlineLeft;
            d7tTMP.overflowMode = TextOverflowModes.Ellipsis;

            var d7Reward1 = FindOrCreate(info.transform, "Reward1");
            GetOrAdd<LayoutElement>(d7Reward1).preferredHeight = 34;
            var r1TMP = GetOrAdd<TextMeshProUGUI>(d7Reward1);
            r1TMP.text = "500 DigitCoins + 50 DigitGems";
            r1TMP.fontSize = FontSizes.Body;
            r1TMP.fontStyle = FontStyles.Bold;
            r1TMP.color = TEXT_WHITE;
            r1TMP.alignment = TextAlignmentOptions.MidlineLeft;
            r1TMP.overflowMode = TextOverflowModes.Ellipsis;

            var d7Reward2 = FindOrCreate(info.transform, "Reward2");
            GetOrAdd<LayoutElement>(d7Reward2).preferredHeight = 30;
            var r2TMP = GetOrAdd<TextMeshProUGUI>(d7Reward2);
            r2TMP.text = "+ Exclusive Item";
            r2TMP.fontSize = FontSizes.Body;
            r2TMP.fontStyle = FontStyles.Bold;
            r2TMP.color = GOLD;
            r2TMP.alignment = TextAlignmentOptions.MidlineLeft;
            r2TMP.overflowMode = TextOverflowModes.Ellipsis;

            var d7Status = FindOrCreate(info.transform, "StatusText");
            GetOrAdd<LayoutElement>(d7Status).preferredHeight = 28;
            var stTMP = GetOrAdd<TextMeshProUGUI>(d7Status);
            stTMP.text = "Unlocks in 2 days";
            stTMP.fontSize = FontSizes.Body;
            stTMP.fontStyle = FontStyles.Bold;
            stTMP.color = TEXT_SECONDARY;
            stTMP.alignment = TextAlignmentOptions.MidlineLeft;
            stTMP.overflowMode = TextOverflowModes.Ellipsis;

            Debug.Log("[DailyRewardsUI] Day7Card creado");
        }

        #endregion

        #region 6. Today Panel (0.300-0.400)

        private static void CreateTodayPanel()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var today = FindOrCreate(canvas.transform, "TodayPanel");
            var tRT = GetOrAdd<RectTransform>(today);
            SetAnchors(tRT, NormX(SIDE_PAD), TODAY_BOT, NormX(1080 - SIDE_PAD), TODAY_TOP);

            var tBg = GetOrAdd<Image>(today);
            tBg.color = CARD_BG_LIGHT;
            var tOutline = GetOrAdd<Outline>(today);
            tOutline.effectColor = CYAN_DARK;
            tOutline.effectDistance = new Vector2(1, 1);

            // 3D depth shadow
            var todayShadow = today.AddComponent<Shadow>();
            todayShadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            todayShadow.effectDistance = new Vector2(3, -4);

            // HLG content
            var hlg = GetOrAdd<HorizontalLayoutGroup>(today);
            hlg.spacing = 15;
            hlg.padding = new RectOffset(15, 15, 12, 12);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            // Left: HOY badge (wider to fit localized text like "DIA 2", "JOUR 2", etc.)
            var badge = FindOrCreate(today.transform, "TodayBadge");
            var bdLE = GetOrAdd<LayoutElement>(badge);
            bdLE.minWidth = 80;
            bdLE.preferredWidth = 80;
            bdLE.minHeight = 30;
            GetOrAdd<Image>(badge).color = GOLD;

            var badgeText = FindOrCreate(badge.transform, "Text");
            var btRT = GetOrAdd<RectTransform>(badgeText);
            btRT.anchorMin = Vector2.zero;
            btRT.anchorMax = Vector2.one;
            btRT.offsetMin = Vector2.zero;
            btRT.offsetMax = Vector2.zero;
            var btTMP = GetOrAdd<TextMeshProUGUI>(badgeText);
            btTMP.text = "TODAY";
            btTMP.fontSize = FontSizes.Body;
            btTMP.fontStyle = FontStyles.Bold;
            btTMP.color = TEXT_DARK;
            btTMP.alignment = TextAlignmentOptions.Center;

            // Center: TodayRewardIcon
            var rewardIcon = FindOrCreate(today.transform, "TodayRewardIcon");
            var riLE = GetOrAdd<LayoutElement>(rewardIcon);
            riLE.minWidth = 45;
            riLE.preferredWidth = 45;
            riLE.minHeight = 45;
            var riImg = GetOrAdd<Image>(rewardIcon);
            riImg.preserveAspect = true;
            riImg.color = Color.white;
            Sprite todaySprite = AssetDatabase.LoadAssetAtPath<Sprite>(DAILY_ICONS + "icon_daily_reward_day5.png");
            if (todaySprite != null) riImg.sprite = todaySprite;
            else riImg.color = COIN_COLOR;

            // Right: Info VLG
            var infoPanel = FindOrCreate(today.transform, "InfoPanel");
            var ipVLG = GetOrAdd<VerticalLayoutGroup>(infoPanel);
            ipVLG.spacing = 4;
            ipVLG.childAlignment = TextAnchor.MiddleLeft;
            ipVLG.childControlWidth = true;
            ipVLG.childControlHeight = false;
            ipVLG.childForceExpandWidth = true;
            var ipLE = GetOrAdd<LayoutElement>(infoPanel);
            ipLE.flexibleWidth = 1;

            var rewardLabel = FindOrCreate(infoPanel.transform, "RewardLabel");
            GetOrAdd<LayoutElement>(rewardLabel).preferredHeight = 30;
            var rlTMP = GetOrAdd<TextMeshProUGUI>(rewardLabel);
            rlTMP.text = "TODAY'S REWARD";
            rlTMP.fontSize = FontSizes.Body;
            rlTMP.fontStyle = FontStyles.Bold;
            rlTMP.color = TEXT_SECONDARY;
            rlTMP.alignment = TextAlignmentOptions.MidlineLeft;
            rlTMP.overflowMode = TextOverflowModes.Ellipsis;

            var rewardAmount = FindOrCreate(infoPanel.transform, "RewardAmount");
            GetOrAdd<LayoutElement>(rewardAmount).preferredHeight = 38;
            var raTMP = GetOrAdd<TextMeshProUGUI>(rewardAmount);
            raTMP.text = "300 DigitCoins + 25 XP";
            raTMP.fontSize = FontSizes.Body;
            raTMP.fontStyle = FontStyles.Bold;
            raTMP.color = COIN_COLOR;
            raTMP.alignment = TextAlignmentOptions.MidlineLeft;
            raTMP.overflowMode = TextOverflowModes.Ellipsis;

            Debug.Log("[DailyRewardsUI] TodayPanel creado");
        }

        #endregion

        #region 7. Claim Button (0.225-0.290)

        private static void CreateClaimButton()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            float buttonPad = SIDE_PAD + 20;

            // ClaimGlow behind button
            var claimGlow = FindOrCreate(canvas.transform, "ClaimGlow");
            var cgRT = GetOrAdd<RectTransform>(claimGlow);
            SetAnchors(cgRT, NormX(buttonPad - 8), CLAIM_BOT - 0.005f, NormX(1080 - buttonPad + 8), CLAIM_TOP + 0.005f);
            GetOrAdd<Image>(claimGlow).color = new Color(GREEN_SUCCESS.r, GREEN_SUCCESS.g, GREEN_SUCCESS.b, 0.15f);

            var claimBtn = FindOrCreate(canvas.transform, "ClaimButton");
            var cbRT = GetOrAdd<RectTransform>(claimBtn);
            SetAnchors(cbRT, NormX(buttonPad), CLAIM_BOT, NormX(1080 - buttonPad), CLAIM_TOP);

            var cbBg = GetOrAdd<Image>(claimBtn);
            cbBg.color = GREEN_SUCCESS;
            GetOrAdd<Button>(claimBtn).targetGraphic = cbBg;
            var cbOutline = GetOrAdd<Outline>(claimBtn);
            cbOutline.effectColor = new Color(0.1f, 0.5f, 0.2f, 1f);
            cbOutline.effectDistance = new Vector2(1.5f, 1.5f);

            // 3D depth shadow
            var claimShadow = claimBtn.AddComponent<Shadow>();
            claimShadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            claimShadow.effectDistance = new Vector2(3, -4);

            var claimText = FindOrCreate(claimBtn.transform, "Text");
            var ctRT = GetOrAdd<RectTransform>(claimText);
            ctRT.anchorMin = Vector2.zero;
            ctRT.anchorMax = Vector2.one;
            ctRT.offsetMin = Vector2.zero;
            ctRT.offsetMax = Vector2.zero;
            var ctTMP = GetOrAdd<TextMeshProUGUI>(claimText);
            ctTMP.text = "CLAIM REWARD";
            ctTMP.fontSize = FontSizes.H4;
            ctTMP.fontStyle = FontStyles.Bold;
            ctTMP.color = TEXT_DARK;
            ctTMP.alignment = TextAlignmentOptions.Center;

            Debug.Log("[DailyRewardsUI] ClaimButton creado");
        }

        #endregion

        #region 8. Timer (0.185-0.225)

        private static void CreateTimer()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var timerBar = FindOrCreate(canvas.transform, "TimerBar");
            var tbRT = GetOrAdd<RectTransform>(timerBar);
            SetAnchors(tbRT, 0.05f, TIMER_BOT, 0.95f, TIMER_TOP);

            var hlg = GetOrAdd<HorizontalLayoutGroup>(timerBar);
            hlg.spacing = 8;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            // Timer icon
            var timerIcon = FindOrCreate(timerBar.transform, "TimerIcon");
            var tiImg = GetOrAdd<Image>(timerIcon);
            tiImg.preserveAspect = true;
            tiImg.color = Color.white;
            Sprite timerSprite = AssetDatabase.LoadAssetAtPath<Sprite>(UI_ICONS + "TimerIcon.png");
            if (timerSprite != null) tiImg.sprite = timerSprite;
            else tiImg.color = TEXT_SECONDARY;
            var tiLE = GetOrAdd<LayoutElement>(timerIcon);
            tiLE.minWidth = 20;
            tiLE.preferredWidth = 20;
            tiLE.minHeight = 20;

            // Label
            var label = FindOrCreate(timerBar.transform, "Label");
            var lTMP = GetOrAdd<TextMeshProUGUI>(label);
            lTMP.text = "Next reward in:";
            lTMP.fontSize = FontSizes.Body;
            lTMP.fontStyle = FontStyles.Bold;
            lTMP.color = TEXT_SECONDARY;
            lTMP.alignment = TextAlignmentOptions.MidlineRight;
            lTMP.overflowMode = TextOverflowModes.Ellipsis;
            var lLE = GetOrAdd<LayoutElement>(label);
            lLE.flexibleWidth = 1;

            // Time text
            var timeText = FindOrCreate(timerBar.transform, "TimeText");
            var ttTMP = GetOrAdd<TextMeshProUGUI>(timeText);
            ttTMP.text = "14h 32m 15s";
            ttTMP.fontSize = FontSizes.Body;
            ttTMP.fontStyle = FontStyles.Bold;
            ttTMP.color = CYAN_NEON;
            ttTMP.alignment = TextAlignmentOptions.MidlineLeft;
            var ttLE = GetOrAdd<LayoutElement>(timeText);
            ttLE.minWidth = 160;
            ttLE.preferredWidth = 160;

            Debug.Log("[DailyRewardsUI] TimerBar creado");
        }

        #endregion

        #region 9. Claim Animation Popup (hidden)

        private static void CreateClaimAnimationPopup()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            // Blocker (fullscreen)
            var blocker = FindOrCreate(canvas.transform, "ClaimAnimationBlocker");
            blocker.SetActive(false);
            var blRT = GetOrAdd<RectTransform>(blocker);
            blRT.anchorMin = Vector2.zero;
            blRT.anchorMax = Vector2.one;
            blRT.offsetMin = Vector2.zero;
            blRT.offsetMax = Vector2.zero;
            GetOrAdd<Image>(blocker).color = new Color(0, 0, 0, 0.85f);
            var blBtn = GetOrAdd<Button>(blocker);
            var blCB = blBtn.colors;
            blCB.normalColor = Color.white;
            blCB.highlightedColor = Color.white;
            blCB.pressedColor = Color.white;
            blCB.selectedColor = Color.white;
            blBtn.colors = blCB;
            blBtn.transition = Selectable.Transition.None;
            blocker.transform.SetAsLastSibling();

            // ClaimPopup panel (centered)
            var popup = FindOrCreate(blocker.transform, "ClaimPopup");
            var ppRT = GetOrAdd<RectTransform>(popup);
            ppRT.anchorMin = new Vector2(0.5f, 0.5f);
            ppRT.anchorMax = new Vector2(0.5f, 0.5f);
            ppRT.sizeDelta = new Vector2(500, 520);
            GetOrAdd<Image>(popup).color = CARD_BG;
            var ppOutline = GetOrAdd<Outline>(popup);
            ppOutline.effectColor = GOLD;
            ppOutline.effectDistance = new Vector2(2, 2);

            // VLG content
            var vlg = GetOrAdd<VerticalLayoutGroup>(popup);
            vlg.spacing = 15;
            vlg.padding = new RectOffset(25, 25, 25, 25);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Celebration Icon
            var celebIcon = FindOrCreate(popup.transform, "CelebrationIcon");
            var ciLE = GetOrAdd<LayoutElement>(celebIcon);
            ciLE.preferredWidth = 70;
            ciLE.preferredHeight = 70;
            var ciImg = GetOrAdd<Image>(celebIcon);
            ciImg.preserveAspect = true;
            ciImg.color = Color.white;
            // Claim celebration icon
            Sprite claimSprite = AssetDatabase.LoadAssetAtPath<Sprite>(DAILY_ICONS + "icon_daily_claim.png");
            if (claimSprite != null) ciImg.sprite = claimSprite;
            else ciImg.color = GOLD;

            // Title
            var celebTitle = FindOrCreate(popup.transform, "CelebTitle");
            GetOrAdd<LayoutElement>(celebTitle).preferredHeight = 42;
            var ctTMP = GetOrAdd<TextMeshProUGUI>(celebTitle);
            ctTMP.text = "Reward Obtained!";
            ctTMP.fontSize = FontSizes.H3;
            ctTMP.fontStyle = FontStyles.Bold;
            ctTMP.color = GOLD;
            ctTMP.alignment = TextAlignmentOptions.Center;

            // Claim Reward Icon
            var claimRewardIcon = FindOrCreate(popup.transform, "ClaimRewardIcon");
            var criLE = GetOrAdd<LayoutElement>(claimRewardIcon);
            criLE.preferredWidth = 60;
            criLE.preferredHeight = 60;
            var criImg = GetOrAdd<Image>(claimRewardIcon);
            criImg.preserveAspect = true;
            criImg.color = COIN_COLOR;

            // Claim Reward Text
            var claimRewardText = FindOrCreate(popup.transform, "ClaimRewardText");
            GetOrAdd<LayoutElement>(claimRewardText).preferredHeight = 45;
            var crtTMP = GetOrAdd<TextMeshProUGUI>(claimRewardText);
            crtTMP.text = "+300 DigitCoins";
            crtTMP.fontSize = FontSizes.H3;
            crtTMP.fontStyle = FontStyles.Bold;
            crtTMP.color = COIN_COLOR;
            crtTMP.alignment = TextAlignmentOptions.Center;

            // Streak Info
            var streakInfo = FindOrCreate(popup.transform, "StreakInfo");
            GetOrAdd<LayoutElement>(streakInfo).preferredHeight = 30;
            var siTMP = GetOrAdd<TextMeshProUGUI>(streakInfo);
            siTMP.text = "Streak: 6 days";
            siTMP.fontSize = FontSizes.Body;
            siTMP.fontStyle = FontStyles.Bold;
            siTMP.color = TEXT_SECONDARY;
            siTMP.alignment = TextAlignmentOptions.Center;

            // Continue Button
            var continueBtn = FindOrCreate(popup.transform, "ContinueButton");
            GetOrAdd<LayoutElement>(continueBtn).preferredHeight = 55;
            var conBg = GetOrAdd<Image>(continueBtn);
            conBg.color = CYAN_NEON;
            GetOrAdd<Button>(continueBtn).targetGraphic = conBg;

            var conText = FindOrCreate(continueBtn.transform, "Text");
            var cnRT = GetOrAdd<RectTransform>(conText);
            cnRT.anchorMin = Vector2.zero;
            cnRT.anchorMax = Vector2.one;
            cnRT.offsetMin = Vector2.zero;
            cnRT.offsetMax = Vector2.zero;
            var cnTMP = GetOrAdd<TextMeshProUGUI>(conText);
            cnTMP.text = "CONTINUE";
            cnTMP.fontSize = FontSizes.BodyLarge;
            cnTMP.fontStyle = FontStyles.Bold;
            cnTMP.color = TEXT_DARK;
            cnTMP.alignment = TextAlignmentOptions.Center;

            Debug.Log("[DailyRewardsUI] ClaimAnimationPopup creado");
        }

        #endregion

        #region 10. Milestone Popup (hidden)

        private static void CreateMilestonePopup()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            // MilestoneBlocker (fullscreen)
            var blocker = FindOrCreate(canvas.transform, "MilestoneBlocker");
            blocker.SetActive(false);
            var blRT = GetOrAdd<RectTransform>(blocker);
            blRT.anchorMin = Vector2.zero;
            blRT.anchorMax = Vector2.one;
            blRT.offsetMin = Vector2.zero;
            blRT.offsetMax = Vector2.zero;
            GetOrAdd<Image>(blocker).color = new Color(0, 0, 0, 0.85f);
            blocker.transform.SetAsLastSibling();

            // MilestonePopup panel (centered)
            var popup = FindOrCreate(blocker.transform, "MilestonePopup");
            var ppRT = GetOrAdd<RectTransform>(popup);
            ppRT.anchorMin = new Vector2(0.5f, 0.5f);
            ppRT.anchorMax = new Vector2(0.5f, 0.5f);
            ppRT.sizeDelta = new Vector2(480, 440);
            GetOrAdd<Image>(popup).color = CARD_BG;
            var ppOutline = GetOrAdd<Outline>(popup);
            ppOutline.effectColor = GOLD;
            ppOutline.effectDistance = new Vector2(2, 2);

            // VLG content
            var vlg = GetOrAdd<VerticalLayoutGroup>(popup);
            vlg.spacing = 15;
            vlg.padding = new RectOffset(25, 25, 25, 25);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Star icon
            var starIcon = FindOrCreate(popup.transform, "StarIcon");
            var siLE = GetOrAdd<LayoutElement>(starIcon);
            siLE.preferredWidth = 60;
            siLE.preferredHeight = 60;
            GetOrAdd<Image>(starIcon).color = GOLD;

            // Milestone Text
            var milestoneText = FindOrCreate(popup.transform, "MilestoneText");
            GetOrAdd<LayoutElement>(milestoneText).preferredHeight = 42;
            var mtTMP = GetOrAdd<TextMeshProUGUI>(milestoneText);
            mtTMP.text = "7 days in a row!";
            mtTMP.fontSize = FontSizes.H3;
            mtTMP.fontStyle = FontStyles.Bold;
            mtTMP.color = GOLD;
            mtTMP.alignment = TextAlignmentOptions.Center;

            // Milestone Bonus Text
            var milestoneBonusText = FindOrCreate(popup.transform, "MilestoneBonusText");
            GetOrAdd<LayoutElement>(milestoneBonusText).preferredHeight = 36;
            var mbtTMP = GetOrAdd<TextMeshProUGUI>(milestoneBonusText);
            mbtTMP.text = "+100 bonus DigitGems";
            mbtTMP.fontSize = FontSizes.Subtitle;
            mbtTMP.fontStyle = FontStyles.Bold;
            mbtTMP.color = GEM_COLOR;
            mbtTMP.alignment = TextAlignmentOptions.Center;

            // Continue Button
            var continueBtn = FindOrCreate(popup.transform, "ContinueBtn");
            GetOrAdd<LayoutElement>(continueBtn).preferredHeight = 52;
            var conBg = GetOrAdd<Image>(continueBtn);
            conBg.color = CYAN_NEON;
            GetOrAdd<Button>(continueBtn).targetGraphic = conBg;

            var conText = FindOrCreate(continueBtn.transform, "Text");
            var cnRT = GetOrAdd<RectTransform>(conText);
            cnRT.anchorMin = Vector2.zero;
            cnRT.anchorMax = Vector2.one;
            cnRT.offsetMin = Vector2.zero;
            cnRT.offsetMax = Vector2.zero;
            var cnTMP = GetOrAdd<TextMeshProUGUI>(conText);
            cnTMP.text = "CONTINUE";
            cnTMP.fontSize = FontSizes.Body;
            cnTMP.fontStyle = FontStyles.Bold;
            cnTMP.color = TEXT_DARK;
            cnTMP.alignment = TextAlignmentOptions.Center;

            Debug.Log("[DailyRewardsUI] MilestonePopup creado");
        }

        #endregion

        #region Manager References

        private static void SetupManagerReferences()
        {
            var manager = Object.FindFirstObjectByType<DigitPark.Managers.DailyRewardsManager>();
            if (manager == null)
            {
                Debug.LogWarning("[DailyRewardsUI] DailyRewardsManager no encontrado. Agrega el componente primero.");
                return;
            }

            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var so = new SerializedObject(manager);
            Transform r = canvas.transform;

            // UI - Header
            SetRef(so, "backButton", FindInPath<Button>(r, "TopBar/BackButton"));
            SetRef(so, "titleText", FindInPath<TextMeshProUGUI>(r, "TopBar/TitleText"));
            SetRef(so, "streakText", FindInPath<TextMeshProUGUI>(r, "StreakPanel/TopRow/StreakCount"));
            SetRef(so, "nextResetText", FindInPath<TextMeshProUGUI>(r, "TimerBar/TimeText"));

            // UI - Current Day
            Transform todayPanel = r.Find("TodayPanel");
            if (todayPanel != null) SetRef(so, "currentDayHighlight", todayPanel.gameObject);
            SetRef(so, "currentDayText", FindInPath<TextMeshProUGUI>(r, "TodayPanel/TodayBadge/Text"));
            SetRef(so, "currentDayRewardIcon", FindInPath<Image>(r, "TodayPanel/TodayRewardIcon"));
            SetRef(so, "currentDayRewardText", FindInPath<TextMeshProUGUI>(r, "TodayPanel/InfoPanel/RewardAmount"));

            // UI - Rewards Grid
            Transform daysGrid = r.Find("DaysGrid");
            if (daysGrid != null) SetRef(so, "rewardsContainer", daysGrid);

            var daysInCycleProp = so.FindProperty("daysInCycle");
            if (daysInCycleProp != null) daysInCycleProp.intValue = 7;

            // UI - Claim Button
            SetRef(so, "claimButton", FindInPath<Button>(r, "ClaimButton"));
            SetRef(so, "claimButtonText", FindInPath<TextMeshProUGUI>(r, "ClaimButton/Text"));
            Transform claimGlow = r.Find("ClaimGlow");
            if (claimGlow != null) SetRef(so, "claimGlow", claimGlow.gameObject);

            // UI - Bonus Info
            SetRef(so, "streakProgressBar", FindInPath<Slider>(r, "StreakPanel/TopRow/StreakProgressBar"));
            SetRef(so, "streakBonusText", FindInPath<TextMeshProUGUI>(r, "StreakPanel/BonusText"));

            // UI - Claim Animation
            Transform claimBlocker = r.Find("ClaimAnimationBlocker");
            if (claimBlocker != null) SetRef(so, "claimAnimationPanel", claimBlocker.gameObject);
            SetRef(so, "claimRewardText", FindInPath<TextMeshProUGUI>(r, "ClaimAnimationBlocker/ClaimPopup/ClaimRewardText"));
            SetRef(so, "claimRewardIcon", FindInPath<Image>(r, "ClaimAnimationBlocker/ClaimPopup/ClaimRewardIcon"));
            SetRef(so, "continueButton", FindInPath<Button>(r, "ClaimAnimationBlocker/ClaimPopup/ContinueButton"));

            // UI - Milestone
            Transform milestoneBlocker = r.Find("MilestoneBlocker");
            if (milestoneBlocker != null) SetRef(so, "milestonePanel", milestoneBlocker.gameObject);
            SetRef(so, "milestoneText", FindInPath<TextMeshProUGUI>(r, "MilestoneBlocker/MilestonePopup/MilestoneText"));
            SetRef(so, "milestoneBonusText", FindInPath<TextMeshProUGUI>(r, "MilestoneBlocker/MilestonePopup/MilestoneBonusText"));

            // Reward Icons (Sprites loaded from assets)
            SetSpriteRef(so, "coinIcon", CURRENCY_ICONS + "icon_digitcoin_single.png");
            SetSpriteRef(so, "gemIcon", CURRENCY_ICONS + "icon_digitgem_single.png");
            SetSpriteRef(so, "xpIcon", CURRENCY_ICONS + "icon_xp.png");
            SetSpriteRef(so, "mysteryIcon", DAILY_ICONS + "icon_daily_mystery.png");

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            Debug.Log("[DailyRewardsUI] Referencias del manager asignadas (23+ campos)");
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
                Debug.LogWarning($"[DailyRewardsUI] Property '{propName}' no encontrada");
                return;
            }
            if (value != null) prop.objectReferenceValue = value;
            else Debug.LogWarning($"[DailyRewardsUI] No se encontro valor para: {propName}");
        }

        private static void SetSpriteRef(SerializedObject so, string propName, string assetPath)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite != null)
            {
                var prop = so.FindProperty(propName);
                if (prop != null) prop.objectReferenceValue = sprite;
                else Debug.LogWarning($"[DailyRewardsUI] Property '{propName}' no encontrada");
            }
            else
            {
                Debug.LogWarning($"[DailyRewardsUI] Sprite no encontrado: {assetPath}");
            }
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

        /// <summary>
        /// Converts a pixel X position to a normalized anchor value (0-1) for a 1080px wide canvas.
        /// </summary>
        private static float NormX(float pixelX)
        {
            return pixelX / 1080f;
        }

        #endregion
    }
}
