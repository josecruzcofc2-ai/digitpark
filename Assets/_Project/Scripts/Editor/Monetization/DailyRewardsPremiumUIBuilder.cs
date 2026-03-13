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
    /// Portrait 9:16 (1080x1920), matchWidthOrHeight=0.5
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

        #region Layout Anchors (Y: 0=bottom, 1=top) — Full-screen layout, zero dead space

        private const float TOPBAR_HEIGHT = 100f;
        private const float TOPBAR_TOP = 0.985f;
        private const float TOPBAR_BOT = 0.955f;  // kept for reference, topbar now uses sizeDelta

        private const float STREAK_TOP = 0.925f;
        private const float STREAK_BOT = 0.890f;

        private const float WEEK_TOP = 0.885f;
        private const float WEEK_BOT = 0.855f;

        private const float DAYS_TOP = 0.848f;
        private const float DAYS_BOT = 0.460f;

        private const float DAY7_TOP = 0.450f;
        private const float DAY7_BOT = 0.255f;

        private const float CLAIM_TOP = 0.185f;
        private const float CLAIM_BOT = 0.092f;

        private const float TIMER_TOP = 0.080f;
        private const float TIMER_BOT = 0.022f;

        private const float SIDE_PAD = 25f;

        #endregion

        #region Icon Paths

        private const string ICONS_BASE = "Assets/_Project/Art/Icons/";
        private const string DAILY_ICONS = ICONS_BASE + "DailyRewards/";
        private const string CURRENCY_ICONS = ICONS_BASE + "Currency/";
        private const string UI_ICONS = ICONS_BASE + "UI/";
        private const string NAV_ICONS = ICONS_BASE + "Navigation/";
        private const string BACK_BUTTON_PREFAB = "Assets/_Project/Prefabs/Common/BackButton.prefab";

        // Per-day gift box icons (each day has its own unique box)
        // Days 1-4: white box, blue ribbon | Day 5: green+gold | Day 6: purple+gold | Day 7: golden chest
        private static string GetGiftIconForDay(int day) => DAILY_ICONS + $"icon_gift_day{Mathf.Clamp(day, 1, 7)}.png";

        // Opened versions for claim animation (3 tiers + day7 is already open)
        private const string GIFT_OPEN_BASIC = DAILY_ICONS + "icon_gift_open_basic.png";
        private const string GIFT_OPEN_PREMIUM = DAILY_ICONS + "icon_gift_open_premium.png";
        private const string GIFT_OPEN_EPIC = DAILY_ICONS + "icon_gift_open_epic.png";

        /// <summary>Returns the opened gift icon path for claim animation</summary>
        private static string GetGiftOpenIconForDay(int day) => day switch
        {
            1 or 2 or 3 or 4 => GIFT_OPEN_BASIC,
            5 => GIFT_OPEN_PREMIUM,
            6 => GIFT_OPEN_EPIC,
            7 => GetGiftIconForDay(7), // Day 7 chest is already open
            _ => GIFT_OPEN_BASIC
        };

        #endregion

        [MenuItem("DigitPark/Scenes/Build Scene/Monetization/Daily Rewards", false, 143)]
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
            if (GUILayout.Button("6. Cleanup TodayPanel (removed)", GUILayout.Height(25))) CleanupTodayPanel();
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

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
            }

            // Full clean of canvas children (keep TransitionCanvas and EventSystem)
            CleanupOldElements(canvas.transform);

            CreateBackground(canvas.transform);
            CreateTopBar();
            CreateStreakPanel();
            CreateWeekLabel();
            CreateDaysGrid();
            CreateDay7Card();
            CleanupTodayPanel();
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

        #region 1. TopBar (100px height)

        private static void CreateTopBar()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var topBar = FindOrCreate(canvas.transform, "TopBar");
            var tbRT = GetOrAdd<RectTransform>(topBar);
            tbRT.anchorMin = new Vector2(0, 1);
            tbRT.anchorMax = new Vector2(1, 1);
            tbRT.pivot = new Vector2(0.5f, 1);
            tbRT.anchoredPosition = new Vector2(0, -(1920f * (1f - TOPBAR_TOP))); // below top edge
            tbRT.sizeDelta = new Vector2(0, TOPBAR_HEIGHT);

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
            tRT.anchorMax = new Vector2(0.48f, 1f);
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
            tTMP.alignment = TextAlignmentOptions.MidlineLeft;
            tTMP.raycastTarget = false;

            // --- Currency pills (right) ---
            var currencyRow = CurrencyHeaderBarHelper.CreateCurrencyPills(topBar.transform, "CurrencyRow");
            var crRT = currencyRow.GetComponent<RectTransform>();
            crRT.anchorMin = new Vector2(0.52f, 0.5f);
            crRT.anchorMax = new Vector2(0.95f, 0.5f);
            crRT.pivot = new Vector2(0.5f, 0.5f);
            crRT.sizeDelta = new Vector2(0, 65);

            Debug.Log("[DailyRewardsUI] TopBar creado (BackButton + Title + CurrencyPills)");
        }

        #endregion

        #region 2. Streak Panel (compact)

        private static void CreateStreakPanel()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            // Clean slate — destroy and recreate
            var oldStreak = canvas.transform.Find("StreakPanel");
            if (oldStreak != null) Object.DestroyImmediate(oldStreak.gameObject);

            var streak = new GameObject("StreakPanel");
            streak.transform.SetParent(canvas.transform, false);
            var sRT = streak.AddComponent<RectTransform>();
            SetAnchors(sRT, NormX(SIDE_PAD), STREAK_BOT, NormX(1080 - SIDE_PAD), STREAK_TOP);

            var sBg = streak.AddComponent<Image>();
            sBg.color = CARD_BG;
            var sOutline = streak.AddComponent<Outline>();
            sOutline.effectColor = new Color(ORANGE_FIRE.r, ORANGE_FIRE.g, ORANGE_FIRE.b, 0.3f);
            sOutline.effectDistance = new Vector2(1, 1);

            // HLG directly on panel — no VLG/TopRow wrapper
            var hlg = streak.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.padding = new RectOffset(12, 12, 6, 6);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // FireIcon (streak flame)
            var fireIcon = new GameObject("FireIcon");
            fireIcon.transform.SetParent(streak.transform, false);
            fireIcon.AddComponent<RectTransform>();
            var fireLE = fireIcon.AddComponent<LayoutElement>();
            fireLE.minWidth = 40;
            fireLE.preferredWidth = 40;
            fireLE.minHeight = 40;
            fireLE.preferredHeight = 40;
            Sprite fireSprite = AssetDatabase.LoadAssetAtPath<Sprite>(DAILY_ICONS + "icon_daily_streak.png");
            if (fireSprite != null)
            {
                var fireImg = fireIcon.AddComponent<Image>();
                fireImg.preserveAspect = true;
                fireImg.color = Color.white;
                fireImg.sprite = fireSprite;
            }

            // StreakLabel
            var streakLabel = new GameObject("StreakLabel");
            streakLabel.transform.SetParent(streak.transform, false);
            streakLabel.AddComponent<RectTransform>();
            var slTMP = streakLabel.AddComponent<TextMeshProUGUI>();
            slTMP.text = "STREAK:";
            slTMP.fontSize = FontSizes.Caption;
            slTMP.fontStyle = FontStyles.Bold;
            slTMP.color = TEXT_WHITE;
            slTMP.alignment = TextAlignmentOptions.MidlineLeft;
            slTMP.enableWordWrapping = false;
            slTMP.overflowMode = TextOverflowModes.Overflow;
            slTMP.enableAutoSizing = true;
            slTMP.fontSizeMin = FontSizes.AutoMinSmall;
            slTMP.fontSizeMax = FontSizes.Caption;
            var slLE = streakLabel.AddComponent<LayoutElement>();
            slLE.minWidth = 160;
            slLE.preferredWidth = 160;

            // StreakCount
            var streakCount = new GameObject("StreakCount");
            streakCount.transform.SetParent(streak.transform, false);
            streakCount.AddComponent<RectTransform>();
            var scTMP = streakCount.AddComponent<TextMeshProUGUI>();
            scTMP.text = "5 DAYS";
            scTMP.fontSize = FontSizes.Caption;
            scTMP.fontStyle = FontStyles.Bold;
            scTMP.color = ORANGE_FIRE;
            scTMP.alignment = TextAlignmentOptions.MidlineLeft;
            scTMP.enableWordWrapping = false;
            scTMP.overflowMode = TextOverflowModes.Ellipsis;
            scTMP.enableAutoSizing = true;
            scTMP.fontSizeMin = FontSizes.AutoMinSmall;
            scTMP.fontSizeMax = FontSizes.Caption;
            var scLE = streakCount.AddComponent<LayoutElement>();
            scLE.minWidth = 110;
            scLE.preferredWidth = 110;

            // Inline progress bar
            var progressBar = FindOrCreate(streak.transform, "StreakProgressBar");
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
            slider.targetGraphic = GetOrAdd<Image>(progressBar);
            GetOrAdd<Image>(progressBar).color = Color.clear;

            // Ensure no visible slider handle (prevents orange square artifact)
            var handleArea = FindOrCreate(progressBar.transform, "Handle Slide Area");
            var hsaRT = GetOrAdd<RectTransform>(handleArea);
            hsaRT.anchorMin = Vector2.zero;
            hsaRT.anchorMax = Vector2.one;
            hsaRT.offsetMin = Vector2.zero;
            hsaRT.offsetMax = Vector2.zero;
            var handle = FindOrCreate(handleArea.transform, "Handle");
            var hRT = GetOrAdd<RectTransform>(handle);
            hRT.sizeDelta = Vector2.zero;
            var handleImg = GetOrAdd<Image>(handle);
            handleImg.color = Color.clear;
            handleImg.enabled = false;
            slider.handleRect = hRT;

            Debug.Log("[DailyRewardsUI] StreakPanel creado (compacto)");
        }

        #endregion

        #region 3. Week + Bonus Label

        private static void CreateWeekLabel()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var weekLabel = FindOrCreate(canvas.transform, "WeekLabel");
            var wRT = GetOrAdd<RectTransform>(weekLabel);
            SetAnchors(wRT, 0.05f, WEEK_BOT, 0.95f, WEEK_TOP);

            // Remove leftover components from old single-TMP version
            var oldImg = weekLabel.GetComponent<Image>();
            if (oldImg != null) Object.DestroyImmediate(oldImg);
            var oldTmp = weekLabel.GetComponent<TextMeshProUGUI>();
            if (oldTmp != null) Object.DestroyImmediate(oldTmp);

            // VLG: Week title + Bonus text (two centered lines)
            var vlg = GetOrAdd<VerticalLayoutGroup>(weekLabel);
            vlg.spacing = 2;
            vlg.padding = new RectOffset(0, 0, 0, 0);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Line 1: ─── WEEK 1 ───
            var weekText = FindOrCreate(weekLabel.transform, "WeekText");
            GetOrAdd<LayoutElement>(weekText).preferredHeight = 30;
            var wtTMP = GetOrAdd<TextMeshProUGUI>(weekText);
            wtTMP.text = "\u2500\u2500\u2500 WEEK 1 \u2500\u2500\u2500";
            wtTMP.fontSize = FontSizes.Body;
            wtTMP.fontStyle = FontStyles.Bold;
            wtTMP.color = GOLD;
            wtTMP.alignment = TextAlignmentOptions.Center;

            // Destroy leftover BonusText if present from older builds
            var oldBonus = weekLabel.transform.Find("BonusText");
            if (oldBonus != null) Object.DestroyImmediate(oldBonus.gameObject);

            Debug.Log("[DailyRewardsUI] WeekLabel creado");
        }

        #endregion

        #region 4. Days Grid (0.540-0.890)

        private static void CreateDaysGrid()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var daysGrid = FindOrCreate(canvas.transform, "DaysGrid");
            var dgRT = GetOrAdd<RectTransform>(daysGrid);
            SetAnchors(dgRT, NormX(SIDE_PAD), DAYS_BOT, NormX(1080 - SIDE_PAD), DAYS_TOP);

            var grid = GetOrAdd<GridLayoutGroup>(daysGrid);
            grid.cellSize = new Vector2(320, 345);
            grid.spacing = new Vector2(15, 14);
            grid.padding = new RectOffset(10, 10, 6, 6);
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
                (6, "digitgems", 50, "DigitGems", 2),   // 2 = LOCKED
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

            // Card background - darker for claimed, subtle for current
            var cardBg = card.AddComponent<Image>();
            cardBg.color = claimed ? new Color(0.04f, 0.06f, 0.09f, 1f) : CARD_BG;

            // Outline - gold glow for current day, subtle for others
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
                outline.effectColor = new Color(0.15f, 0.15f, 0.2f, 0.4f);
                outline.effectDistance = new Vector2(1, 1);
            }

            // 3D depth shadow
            var cardShadow = card.AddComponent<Shadow>();
            cardShadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            cardShadow.effectDistance = new Vector2(3, -4);

            // Clip overflow content to card boundaries
            card.AddComponent<UnityEngine.UI.RectMask2D>();

            // VLG for card content - gift box centered layout
            var vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 3;
            vlg.padding = new RectOffset(6, 6, 6, 6);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = false;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;

            // Day Label (top, compact)
            var dayLabel = new GameObject("DayLabel");
            dayLabel.transform.SetParent(card.transform, false);
            dayLabel.AddComponent<RectTransform>();
            var dlLE = dayLabel.AddComponent<LayoutElement>();
            dlLE.preferredHeight = 28;
            dlLE.preferredWidth = 300;
            var dlTMP = dayLabel.AddComponent<TextMeshProUGUI>();
            dlTMP.text = $"DAY {day}";
            dlTMP.fontSize = FontSizes.BodySmall;
            dlTMP.fontStyle = FontStyles.Bold;
            dlTMP.color = claimed ? GREEN_SUCCESS : (current ? GOLD : TEXT_SECONDARY);
            dlTMP.alignment = TextAlignmentOptions.Center;

            // === GIFT BOX ICON (protagonist - centered, constrained) ===
            var iconContainer = new GameObject("GiftIcon");
            iconContainer.transform.SetParent(card.transform, false);
            iconContainer.AddComponent<RectTransform>();
            var iconLE = iconContainer.AddComponent<LayoutElement>();
            iconLE.preferredHeight = 210;
            iconLE.preferredWidth = 210;
            iconLE.minWidth = 210;
            iconLE.flexibleWidth = 0;
            var iconImg = iconContainer.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.color = Color.white;

            // Load per-day gift icon (claimed/locked states use same icon with tint)
            string giftPath = GetGiftIconForDay(day);

            Sprite giftSprite = AssetDatabase.LoadAssetAtPath<Sprite>(giftPath);
            if (giftSprite != null)
            {
                iconImg.sprite = giftSprite;
                if (claimed) iconImg.color = new Color(1f, 1f, 1f, 0.7f);
            }
            else
            {
                // Fallback: colored rectangle with tier indication
                Color fallbackColor = day switch
                {
                    1 or 2 => new Color(0.8f, 0.5f, 0.2f, 1f),   // bronze
                    3 or 4 => new Color(0.75f, 0.75f, 0.8f, 1f),  // silver
                    5 or 6 => GOLD,                                  // gold
                    _ => TEXT_WHITE
                };
                if (claimed) fallbackColor = new Color(fallbackColor.r, fallbackColor.g, fallbackColor.b, 0.4f);
                if (locked) fallbackColor = new Color(0.2f, 0.2f, 0.25f, 0.6f);
                iconImg.color = fallbackColor;
            }

            // No glow behind day card icons (clean look)

            // Reward amount + type (compact row)
            var rewardRow = new GameObject("RewardRow");
            rewardRow.transform.SetParent(card.transform, false);
            rewardRow.AddComponent<RectTransform>();
            var rrLE = rewardRow.AddComponent<LayoutElement>();
            rrLE.preferredHeight = 34;
            rrLE.preferredWidth = 300;
            var rrHLG = rewardRow.AddComponent<HorizontalLayoutGroup>();
            rrHLG.spacing = 4;
            rrHLG.childAlignment = TextAnchor.MiddleCenter;
            rrHLG.childControlWidth = false;
            rrHLG.childControlHeight = false;
            rrHLG.childForceExpandWidth = false;
            rrHLG.childForceExpandHeight = false;

            // Small currency icon in reward row
            var rewardTypeIcon = new GameObject("TypeIcon");
            rewardTypeIcon.transform.SetParent(rewardRow.transform, false);
            rewardTypeIcon.AddComponent<RectTransform>();
            var rtiLE = rewardTypeIcon.AddComponent<LayoutElement>();
            rtiLE.minWidth = 22;
            rtiLE.preferredWidth = 22;
            rtiLE.minHeight = 22;
            var rtiImg = rewardTypeIcon.AddComponent<Image>();
            rtiImg.preserveAspect = true;
            string currencyIconPath = type switch
            {
                "digitcoins" => CURRENCY_ICONS + "icon_digitcoin_single.png",
                "digitgems" => CURRENCY_ICONS + "icon_digitgem_single.png",
                "xp" => CURRENCY_ICONS + "icon_xp.png",
                _ => CURRENCY_ICONS + "icon_digitcoin_single.png"
            };
            Sprite currSprite = AssetDatabase.LoadAssetAtPath<Sprite>(currencyIconPath);
            if (currSprite != null) { rtiImg.sprite = currSprite; rtiImg.color = Color.white; }
            else rtiImg.color = COIN_COLOR;

            // Amount text
            var amountObj = new GameObject("AmountText");
            amountObj.transform.SetParent(rewardRow.transform, false);
            amountObj.AddComponent<RectTransform>();
            var amLE = amountObj.AddComponent<LayoutElement>();
            amLE.flexibleWidth = 1;
            var amTMP = amountObj.AddComponent<TextMeshProUGUI>();
            amTMP.text = $"+{amount}";
            amTMP.fontSize = FontSizes.BodySmall;
            amTMP.fontStyle = FontStyles.Bold;
            Color rewardColor = type switch
            {
                "digitcoins" => COIN_COLOR,
                "digitgems" => GEM_COLOR,
                "xp" => XP_COLOR,
                _ => TEXT_WHITE
            };
            amTMP.color = claimed ? new Color(rewardColor.r, rewardColor.g, rewardColor.b, 0.5f) : rewardColor;
            amTMP.alignment = TextAlignmentOptions.MidlineLeft;
            amTMP.overflowMode = TextOverflowModes.Ellipsis;

            // === STATUS OVERLAYS ===
            if (claimed)
            {
                // Green check badge (top-right corner) — ignoreLayout so VLG doesn't stack it
                var check = new GameObject("CheckOverlay");
                check.transform.SetParent(card.transform, false);
                var chRT = check.AddComponent<RectTransform>();
                chRT.anchorMin = new Vector2(1, 1);
                chRT.anchorMax = new Vector2(1, 1);
                chRT.pivot = new Vector2(1, 1);
                chRT.anchoredPosition = new Vector2(-4, -4);
                chRT.sizeDelta = new Vector2(28, 28);
                check.AddComponent<LayoutElement>().ignoreLayout = true;
                check.AddComponent<Image>().color = GREEN_SUCCESS;
                var checkOutline = check.AddComponent<Outline>();
                checkOutline.effectColor = new Color(0.1f, 0.4f, 0.15f, 1f);
                checkOutline.effectDistance = new Vector2(1, 1);

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
                // TODAY badge (positioned above the card) — ignoreLayout so VLG doesn't stack it
                var badge = new GameObject("TodayBadge");
                badge.transform.SetParent(card.transform, false);
                var bdRT = badge.AddComponent<RectTransform>();
                bdRT.anchorMin = new Vector2(0.5f, 1);
                bdRT.anchorMax = new Vector2(0.5f, 1);
                bdRT.pivot = new Vector2(0.5f, 0);
                bdRT.anchoredPosition = new Vector2(0, 2);
                bdRT.sizeDelta = new Vector2(100, 24);
                badge.AddComponent<LayoutElement>().ignoreLayout = true;
                badge.AddComponent<Image>().color = GOLD;
                var badgeOutline = badge.AddComponent<Outline>();
                badgeOutline.effectColor = GOLD_DARK;
                badgeOutline.effectDistance = new Vector2(1, 1);

                var badgeText = new GameObject("Text");
                badgeText.transform.SetParent(badge.transform, false);
                var bttRT = badgeText.AddComponent<RectTransform>();
                bttRT.anchorMin = Vector2.zero;
                bttRT.anchorMax = Vector2.one;
                bttRT.offsetMin = Vector2.zero;
                bttRT.offsetMax = Vector2.zero;
                var bttTMP = badgeText.AddComponent<TextMeshProUGUI>();
                bttTMP.text = "TODAY";
                bttTMP.fontSize = FontSizes.BodySmall;
                bttTMP.fontStyle = FontStyles.Bold;
                bttTMP.color = TEXT_DARK;
                bttTMP.alignment = TextAlignmentOptions.Center;
                bttTMP.enableWordWrapping = false;
                bttTMP.overflowMode = TextOverflowModes.Ellipsis;
            }
            // Locked days use their own day icon with greyed tint (no overlay needed)
        }

        #endregion

        #region 5. Day 7 Mega Card (0.380-0.533)

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

            // Left: Icon area (larger for legendary chest)
            var iconArea = FindOrCreate(day7.transform, "IconArea");
            var iaLE = GetOrAdd<LayoutElement>(iconArea);
            iaLE.minWidth = 160;
            iaLE.preferredWidth = 160;
            iaLE.minHeight = 160;

            // Remove old IconGlow if exists
            var oldGlow = iconArea.transform.Find("IconGlow");
            if (oldGlow != null) Object.DestroyImmediate(oldGlow.gameObject);

            // Day7 legendary chest icon (larger than regular gift boxes)
            var day7Icon = FindOrCreate(iconArea.transform, "Day7Icon");
            var d7iRT = GetOrAdd<RectTransform>(day7Icon);
            d7iRT.anchorMin = new Vector2(0.5f, 0.5f);
            d7iRT.anchorMax = new Vector2(0.5f, 0.5f);
            d7iRT.sizeDelta = new Vector2(140, 140);
            var d7iImg = GetOrAdd<Image>(day7Icon);
            d7iImg.preserveAspect = true;
            d7iImg.color = Color.white;
            Sprite d7Sprite = AssetDatabase.LoadAssetAtPath<Sprite>(GetGiftIconForDay(7));
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

            var d7Title = FindOrCreate(info.transform, "Day7GrandPrizeLabel");
            GetOrAdd<LayoutElement>(d7Title).preferredHeight = 40;
            var d7tTMP = GetOrAdd<TextMeshProUGUI>(d7Title);
            d7tTMP.text = "DAY 7 - GRAND PRIZE";
            d7tTMP.fontSize = FontSizes.Subtitle;
            d7tTMP.fontStyle = FontStyles.Bold;
            d7tTMP.color = GOLD;
            d7tTMP.alignment = TextAlignmentOptions.MidlineLeft;
            d7tTMP.overflowMode = TextOverflowModes.Ellipsis;

            var d7Reward1 = FindOrCreate(info.transform, "Reward1");
            GetOrAdd<LayoutElement>(d7Reward1).preferredHeight = 36;
            var r1TMP = GetOrAdd<TextMeshProUGUI>(d7Reward1);
            r1TMP.text = "500 DigitCoins + 50 DigitGems";
            r1TMP.fontSize = FontSizes.Body;
            r1TMP.fontStyle = FontStyles.Bold;
            r1TMP.color = TEXT_WHITE;
            r1TMP.alignment = TextAlignmentOptions.MidlineLeft;
            r1TMP.overflowMode = TextOverflowModes.Ellipsis;

            var d7Reward2 = FindOrCreate(info.transform, "Reward2");
            GetOrAdd<LayoutElement>(d7Reward2).preferredHeight = 32;
            var r2TMP = GetOrAdd<TextMeshProUGUI>(d7Reward2);
            r2TMP.text = "+ Exclusive Item";
            r2TMP.fontSize = FontSizes.Body;
            r2TMP.fontStyle = FontStyles.Bold;
            r2TMP.color = GOLD;
            r2TMP.alignment = TextAlignmentOptions.MidlineLeft;
            r2TMP.overflowMode = TextOverflowModes.Ellipsis;

            var d7Status = FindOrCreate(info.transform, "StatusText");
            GetOrAdd<LayoutElement>(d7Status).preferredHeight = 30;
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

        #region 6. Cleanup TodayPanel (removed — redundant with day grid + claim animation)

        private static void CleanupTodayPanel()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var oldToday = canvas.transform.Find("TodayPanel");
            if (oldToday != null) Object.DestroyImmediate(oldToday.gameObject);

            Debug.Log("[DailyRewardsUI] TodayPanel eliminado (redundante)");
        }

        #endregion

        #region 7. Claim Button (0.205-0.270)

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

            var claimText = FindOrCreate(claimBtn.transform, "ClaimRewardText");
            var ctRT = GetOrAdd<RectTransform>(claimText);
            ctRT.anchorMin = Vector2.zero;
            ctRT.anchorMax = Vector2.one;
            ctRT.offsetMin = Vector2.zero;
            ctRT.offsetMax = Vector2.zero;
            var ctTMP = GetOrAdd<TextMeshProUGUI>(claimText);
            ctTMP.text = "CLAIM REWARD";
            ctTMP.enableAutoSizing = true;
            ctTMP.fontSizeMin = FontSizes.H4;
            ctTMP.fontSizeMax = FontSizes.H3;
            ctTMP.fontStyle = FontStyles.Bold;
            ctTMP.color = TEXT_DARK;
            ctTMP.alignment = TextAlignmentOptions.Center;

            Debug.Log("[DailyRewardsUI] ClaimButton creado");
        }

        #endregion

        #region 8. Timer (centered, icon 96px)

        private static void CreateTimer()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var timerBar = FindOrCreate(canvas.transform, "TimerBar");
            var tbRT = GetOrAdd<RectTransform>(timerBar);
            SetAnchors(tbRT, 0f, TIMER_BOT, 1f, TIMER_TOP);

            var hlg = GetOrAdd<HorizontalLayoutGroup>(timerBar);
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // Timer icon (96px, left of text)
            var timerIcon = FindOrCreate(timerBar.transform, "TimerIcon");
            var tiLE = GetOrAdd<LayoutElement>(timerIcon);
            tiLE.minWidth = 96;
            tiLE.preferredWidth = 96;
            tiLE.minHeight = 96;
            tiLE.preferredHeight = 96;
            Sprite timerSprite = AssetDatabase.LoadAssetAtPath<Sprite>(UI_ICONS + "TimerIcon.png");
            if (timerSprite != null)
            {
                var tiImg = GetOrAdd<Image>(timerIcon);
                tiImg.preserveAspect = true;
                tiImg.color = Color.white;
                tiImg.sprite = timerSprite;
            }
            else
            {
                var tiImg = timerIcon.GetComponent<Image>();
                if (tiImg != null) Object.DestroyImmediate(tiImg);
                tiLE.minWidth = 0;
                tiLE.preferredWidth = 0;
            }

            // Label "Next reward in:"
            var label = FindOrCreate(timerBar.transform, "NextRewardLabel");
            var lRT = GetOrAdd<RectTransform>(label);
            lRT.sizeDelta = new Vector2(300, 40);
            var lTMP = GetOrAdd<TextMeshProUGUI>(label);
            lTMP.text = "Next reward in:";
            lTMP.enableAutoSizing = true;
            lTMP.fontSizeMin = FontSizes.Caption;
            lTMP.fontSizeMax = FontSizes.Body;
            lTMP.fontStyle = FontStyles.Bold;
            lTMP.color = TEXT_SECONDARY;
            lTMP.alignment = TextAlignmentOptions.MidlineRight;
            lTMP.enableWordWrapping = false;
            lTMP.overflowMode = TextOverflowModes.Ellipsis;
            var lLE = GetOrAdd<LayoutElement>(label);
            lLE.preferredWidth = 300;
            lLE.preferredHeight = 40;
            lLE.flexibleWidth = 0;

            // Time text
            var timeText = FindOrCreate(timerBar.transform, "TimeText");
            var ttRT = GetOrAdd<RectTransform>(timeText);
            ttRT.sizeDelta = new Vector2(250, 40);
            var ttTMP = GetOrAdd<TextMeshProUGUI>(timeText);
            ttTMP.text = "14h 32m 15s";
            ttTMP.enableAutoSizing = true;
            ttTMP.fontSizeMin = FontSizes.Caption;
            ttTMP.fontSizeMax = FontSizes.Body;
            ttTMP.fontStyle = FontStyles.Bold;
            ttTMP.color = CYAN_NEON;
            ttTMP.alignment = TextAlignmentOptions.MidlineLeft;
            ttTMP.enableWordWrapping = false;
            ttTMP.overflowMode = TextOverflowModes.Ellipsis;
            var ttLE = GetOrAdd<LayoutElement>(timeText);
            ttLE.preferredWidth = 250;
            ttLE.preferredHeight = 40;
            ttLE.flexibleWidth = 0;

            Debug.Log("[DailyRewardsUI] TimerBar creado (centrado, icono 96px)");
        }

        #endregion

        #region 9. Claim Animation Popup (hidden)

        private static void CreateClaimAnimationPopup()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            // === Blocker (fullscreen, starts hidden) ===
            var blocker = FindOrCreate(canvas.transform, "ClaimAnimationBlocker");
            blocker.SetActive(false);
            var blRT = GetOrAdd<RectTransform>(blocker);
            blRT.anchorMin = Vector2.zero;
            blRT.anchorMax = Vector2.one;
            blRT.offsetMin = Vector2.zero;
            blRT.offsetMax = Vector2.zero;
            // Blocker itself is transparent — DarkOverlay child handles the dim
            GetOrAdd<Image>(blocker).color = new Color(0, 0, 0, 0f);
            blocker.transform.SetAsLastSibling();

            // === DarkOverlay (fullscreen black dim, animated from 0→0.7) ===
            var darkOverlay = FindOrCreate(blocker.transform, "DarkOverlay");
            var doRT = GetOrAdd<RectTransform>(darkOverlay);
            doRT.anchorMin = Vector2.zero;
            doRT.anchorMax = Vector2.one;
            doRT.offsetMin = Vector2.zero;
            doRT.offsetMax = Vector2.zero;
            GetOrAdd<Image>(darkOverlay).color = new Color(0, 0, 0, 0.7f);
            darkOverlay.AddComponent<CanvasGroup>();

            // === GiftGlow (large radial glow behind the gift, golden) ===
            var giftGlow = FindOrCreate(blocker.transform, "GiftGlow");
            var ggRT = GetOrAdd<RectTransform>(giftGlow);
            ggRT.anchorMin = new Vector2(0.5f, 0.55f);
            ggRT.anchorMax = new Vector2(0.5f, 0.55f);
            ggRT.sizeDelta = new Vector2(500, 500);
            var ggImg = GetOrAdd<Image>(giftGlow);
            ggImg.color = new Color(GOLD.r, GOLD.g, GOLD.b, 0.25f);
            ggImg.raycastTarget = false;

            // === GiftBox (closed gift icon — the protagonist, large 280x280) ===
            var giftBox = FindOrCreate(blocker.transform, "GiftBox");
            var gbRT = GetOrAdd<RectTransform>(giftBox);
            gbRT.anchorMin = new Vector2(0.5f, 0.55f);
            gbRT.anchorMax = new Vector2(0.5f, 0.55f);
            gbRT.sizeDelta = new Vector2(280, 280);
            var gbImg = GetOrAdd<Image>(giftBox);
            gbImg.preserveAspect = true;
            gbImg.color = Color.white;
            gbImg.raycastTarget = false;
            // Use day1 closed gift as placeholder — runtime swaps to correct day
            Sprite giftClosedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(GetGiftIconForDay(1));
            if (giftClosedSprite != null) gbImg.sprite = giftClosedSprite;

            // === LightBurst (fullscreen white flash, alpha 0) ===
            var lightBurst = FindOrCreate(blocker.transform, "LightBurst");
            var lbRT = GetOrAdd<RectTransform>(lightBurst);
            lbRT.anchorMin = Vector2.zero;
            lbRT.anchorMax = Vector2.one;
            lbRT.offsetMin = Vector2.zero;
            lbRT.offsetMax = Vector2.zero;
            var lbImg = GetOrAdd<Image>(lightBurst);
            lbImg.color = new Color(1f, 0.95f, 0.8f, 0f);
            lbImg.raycastTarget = false;

            // === RewardContainer (VLG, centered above gift, holds reward reveal) ===
            var rewardContainer = FindOrCreate(blocker.transform, "RewardContainer");
            var rcRT = GetOrAdd<RectTransform>(rewardContainer);
            rcRT.anchorMin = new Vector2(0.5f, 0.55f);
            rcRT.anchorMax = new Vector2(0.5f, 0.55f);
            rcRT.pivot = new Vector2(0.5f, 0f);
            rcRT.anchoredPosition = new Vector2(0, 160);
            rcRT.sizeDelta = new Vector2(500, 300);
            // Background card for reward container
            var rcBg = GetOrAdd<Image>(rewardContainer);
            rcBg.color = new Color(0.06f, 0.09f, 0.14f, 0.95f);
            var rcOutline = GetOrAdd<Outline>(rewardContainer);
            rcOutline.effectColor = new Color(GOLD.r, GOLD.g, GOLD.b, 0.4f);
            rcOutline.effectDistance = new Vector2(2, -2);
            var rcVLG = GetOrAdd<VerticalLayoutGroup>(rewardContainer);
            rcVLG.spacing = 8;
            rcVLG.padding = new RectOffset(10, 10, 10, 10);
            rcVLG.childAlignment = TextAnchor.MiddleCenter;
            rcVLG.childControlWidth = true;
            rcVLG.childControlHeight = false;
            rcVLG.childForceExpandWidth = true;
            rcVLG.childForceExpandHeight = false;

            // CelebTitle — "Reward Obtained!"
            var celebTitle = FindOrCreate(rewardContainer.transform, "CelebTitle");
            GetOrAdd<LayoutElement>(celebTitle).preferredHeight = 50;
            var ctTMP = GetOrAdd<TextMeshProUGUI>(celebTitle);
            ctTMP.text = "Reward Obtained!";
            ctTMP.fontSize = FontSizes.H2;
            ctTMP.fontStyle = FontStyles.Bold;
            ctTMP.color = GOLD;
            ctTMP.alignment = TextAlignmentOptions.Center;
            ctTMP.enableAutoSizing = true;
            ctTMP.fontSizeMin = FontSizes.H4;
            ctTMP.fontSizeMax = FontSizes.H2;

            // Reward row (HLG: icon + text)
            var rewardRow = FindOrCreate(rewardContainer.transform, "RewardRow");
            GetOrAdd<LayoutElement>(rewardRow).preferredHeight = 60;
            var rrHLG = GetOrAdd<HorizontalLayoutGroup>(rewardRow);
            rrHLG.spacing = 12;
            rrHLG.childAlignment = TextAnchor.MiddleCenter;
            rrHLG.childControlWidth = false;
            rrHLG.childControlHeight = false;
            rrHLG.childForceExpandWidth = false;
            rrHLG.childForceExpandHeight = false;

            // ClaimRewardIcon
            var claimRewardIcon = FindOrCreate(rewardRow.transform, "ClaimRewardIcon");
            var criLE = GetOrAdd<LayoutElement>(claimRewardIcon);
            criLE.preferredWidth = 55;
            criLE.preferredHeight = 55;
            var criImg = GetOrAdd<Image>(claimRewardIcon);
            criImg.preserveAspect = true;
            criImg.color = Color.white;
            Sprite coinSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CURRENCY_ICONS + "icon_digitcoin_single.png");
            if (coinSprite != null) criImg.sprite = coinSprite;

            // ClaimRewardText
            var claimRewardText = FindOrCreate(rewardRow.transform, "ClaimRewardText");
            var crtLE = GetOrAdd<LayoutElement>(claimRewardText);
            crtLE.preferredWidth = 350;
            crtLE.preferredHeight = 55;
            var crtTMP = GetOrAdd<TextMeshProUGUI>(claimRewardText);
            crtTMP.text = "+300 DigitCoins";
            crtTMP.fontSize = FontSizes.H3;
            crtTMP.fontStyle = FontStyles.Bold;
            crtTMP.color = COIN_COLOR;
            crtTMP.alignment = TextAlignmentOptions.Left;
            crtTMP.enableAutoSizing = true;
            crtTMP.fontSizeMin = FontSizes.Body;
            crtTMP.fontSizeMax = FontSizes.H3;

            // StreakInfo
            var streakInfo = FindOrCreate(rewardContainer.transform, "StreakInfo");
            GetOrAdd<LayoutElement>(streakInfo).preferredHeight = 35;
            var siTMP = GetOrAdd<TextMeshProUGUI>(streakInfo);
            siTMP.text = "Streak: 6 days";
            siTMP.fontSize = FontSizes.Body;
            siTMP.fontStyle = FontStyles.Bold;
            siTMP.color = ORANGE_FIRE;
            siTMP.alignment = TextAlignmentOptions.Center;

            // === ContinueButton (bottom area, "TAP TO CONTINUE") ===
            var continueBtn = FindOrCreate(blocker.transform, "ContinueButton");
            var cbRT = GetOrAdd<RectTransform>(continueBtn);
            cbRT.anchorMin = new Vector2(0.5f, 0.08f);
            cbRT.anchorMax = new Vector2(0.5f, 0.08f);
            cbRT.sizeDelta = new Vector2(400, 55);
            // Transparent bg — just text
            var conBg = GetOrAdd<Image>(continueBtn);
            conBg.color = new Color(0, 0, 0, 0);
            var conBtn = GetOrAdd<Button>(continueBtn);
            conBtn.targetGraphic = conBg;
            conBtn.transition = Selectable.Transition.None;

            var conText = FindOrCreate(continueBtn.transform, "TapToContinueText");
            var cnRT = GetOrAdd<RectTransform>(conText);
            cnRT.anchorMin = Vector2.zero;
            cnRT.anchorMax = Vector2.one;
            cnRT.offsetMin = Vector2.zero;
            cnRT.offsetMax = Vector2.zero;
            var cnTMP = GetOrAdd<TextMeshProUGUI>(conText);
            cnTMP.text = "TAP TO CONTINUE";
            cnTMP.fontSize = FontSizes.Body;
            cnTMP.fontStyle = FontStyles.Bold;
            cnTMP.color = new Color(1f, 1f, 1f, 0.7f);
            cnTMP.alignment = TextAlignmentOptions.Center;

            Debug.Log("[DailyRewardsUI] ClaimAnimationPopup (Clash Royale style) creado");
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

            var conText = FindOrCreate(continueBtn.transform, "ContinueButtonText");
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
            SetRef(so, "streakText", FindInPath<TextMeshProUGUI>(r, "StreakPanel/StreakCount"));
            SetRef(so, "nextResetText", FindInPath<TextMeshProUGUI>(r, "TimerBar/TimeText"));

            // UI - Current Day (TodayPanel removed — refs cleared)

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
            SetRef(so, "streakProgressBar", FindInPath<Slider>(r, "StreakPanel/StreakProgressBar"));
            SetRef(so, "streakBonusText", FindInPath<TextMeshProUGUI>(r, "WeekLabel/BonusText"));

            // UI - Claim Animation (Clash Royale style)
            Transform claimBlocker = r.Find("ClaimAnimationBlocker");
            if (claimBlocker != null) SetRef(so, "claimAnimationPanel", claimBlocker.gameObject);
            SetRef(so, "darkOverlayImage", FindInPath<Image>(r, "ClaimAnimationBlocker/DarkOverlay"));
            SetRef(so, "giftBoxImage", FindInPath<Image>(r, "ClaimAnimationBlocker/GiftBox"));
            SetRef(so, "giftGlowImage", FindInPath<Image>(r, "ClaimAnimationBlocker/GiftGlow"));
            SetRef(so, "lightBurstImage", FindInPath<Image>(r, "ClaimAnimationBlocker/LightBurst"));
            SetRef(so, "celebTitleText", FindInPath<TextMeshProUGUI>(r, "ClaimAnimationBlocker/RewardContainer/CelebTitle"));
            SetRef(so, "claimRewardIcon", FindInPath<Image>(r, "ClaimAnimationBlocker/RewardContainer/RewardRow/ClaimRewardIcon"));
            SetRef(so, "claimRewardText", FindInPath<TextMeshProUGUI>(r, "ClaimAnimationBlocker/RewardContainer/RewardRow/ClaimRewardText"));
            SetRef(so, "streakInfoText", FindInPath<TextMeshProUGUI>(r, "ClaimAnimationBlocker/RewardContainer/StreakInfo"));
            SetRef(so, "continueButton", FindInPath<Button>(r, "ClaimAnimationBlocker/ContinueButton"));

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

            // Gift Box Icons (per-day array + opened tiers)
            var dayArrayProp = so.FindProperty("giftDayIcons");
            if (dayArrayProp != null)
            {
                dayArrayProp.arraySize = 7;
                for (int d = 1; d <= 7; d++)
                {
                    var elem = dayArrayProp.GetArrayElementAtIndex(d - 1);
                    Sprite spr = AssetDatabase.LoadAssetAtPath<Sprite>(GetGiftIconForDay(d));
                    if (spr != null) elem.objectReferenceValue = spr;
                }
            }
            SetSpriteRef(so, "giftOpenBasicIcon", GIFT_OPEN_BASIC);
            SetSpriteRef(so, "giftOpenPremiumIcon", GIFT_OPEN_PREMIUM);
            SetSpriteRef(so, "giftOpenEpicIcon", GIFT_OPEN_EPIC);

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            Debug.Log("[DailyRewardsUI] Referencias del manager asignadas (30+ campos)");
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

        private static void CleanupOldElements(Transform parent)
        {
            var toDestroy = new System.Collections.Generic.List<GameObject>();
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
