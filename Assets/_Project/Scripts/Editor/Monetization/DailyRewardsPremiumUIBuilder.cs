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
    /// Layout: TopBar -> RewardsScrollView(7 vertical cards) -> ClaimButton -> Timer
    /// Popups: ClaimAnimationBlocker
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

        // ScrollView fills space between TopBar and ClaimButton (with margin)
        private const float SCROLL_TOP = 0.920f;
        private const float SCROLL_BOT = 0.185f;

        private const float CLAIM_TOP = 0.170f;
        private const float CLAIM_BOT = 0.080f;

        private const float TIMER_TOP = 0.070f;
        private const float TIMER_BOT = 0.015f;

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
                "2. Rewards ScrollView (7 cards verticales)\n" +
                "3. Claim Button (reclamar)\n" +
                "4. Timer (proxima recompensa)\n" +
                "5. Claim Animation Popup (hidden)",
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
            if (GUILayout.Button("2. Rewards ScrollView (7 days)", GUILayout.Height(25))) CreateRewardsScrollView();
            if (GUILayout.Button("3. Claim Button", GUILayout.Height(25))) CreateClaimButton();
            if (GUILayout.Button("4. Timer", GUILayout.Height(25))) CreateTimer();
            if (GUILayout.Button("5. Claim Animation Popup", GUILayout.Height(25))) CreateClaimAnimationPopup();
            // Milestone Popup removed — Model 2

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
            CreateRewardsScrollView();
            CreateClaimButton();
            CreateTimer();
            CreateClaimAnimationPopup();
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
            var bgImg = GetOrAdd<Image>(bg);
            bgImg.color = Color.white; // ThemeApplier tints at runtime
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
            // Safe area handler for Dynamic Island / notch devices
            GetOrAdd<SafeAreaHandler>(topBar);

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

        #region 2. Streak Panel (removed — Model 2)

        private static void CleanupStreakPanel()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var oldStreak = canvas.transform.Find("StreakPanel");
            if (oldStreak != null) Object.DestroyImmediate(oldStreak.gameObject);

            Debug.Log("[DailyRewardsUI] StreakPanel eliminado (Model 2 — no streak)");
        }

        #endregion

        #region 2. Rewards ScrollView (vertical, 7 full-width cards)

        private static void CreateRewardsScrollView()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            // Cleanup old elements
            string[] oldNames = { "WeekLabel", "DaysGrid", "Day7Glow", "Day7Card", "TodayPanel", "StreakPanel", "RewardsScrollView" };
            foreach (string n in oldNames)
            {
                var old = canvas.transform.Find(n);
                if (old != null) Object.DestroyImmediate(old.gameObject);
            }

            // === ScrollView ===
            var scrollView = new GameObject("RewardsScrollView");
            scrollView.transform.SetParent(canvas.transform, false);
            var svRT = scrollView.AddComponent<RectTransform>();
            SetAnchors(svRT, NormX(SIDE_PAD), SCROLL_BOT, NormX(1080 - SIDE_PAD), SCROLL_TOP);

            var sr = scrollView.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Elastic;
            sr.elasticity = 0.1f;
            sr.scrollSensitivity = 30f;
            scrollView.AddComponent<Image>().color = Color.clear; // needed for scroll input

            // Viewport (clips content)
            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);
            var vpRT = viewport.AddComponent<RectTransform>();
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.offsetMin = Vector2.zero;
            vpRT.offsetMax = Vector2.zero;
            viewport.AddComponent<RectMask2D>();
            viewport.AddComponent<Image>().color = Color.clear;

            // Content (grows with cards, scrollable)
            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            var cRT = content.AddComponent<RectTransform>();
            cRT.anchorMin = new Vector2(0, 1);
            cRT.anchorMax = new Vector2(1, 1);
            cRT.pivot = new Vector2(0.5f, 1);
            // Total height: 6×350 + 1×550 + 6×15(spacing) + 10(padding) = 2750
            float totalHeight = (6f * 350f) + 550f + (6f * 15f) + 10f;
            cRT.sizeDelta = new Vector2(0, totalHeight);

            var vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 15;
            vlg.padding = new RectOffset(0, 0, 5, 5);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            sr.content = cRT;
            sr.viewport = vpRT;

            // 7 cards — each ~350px tall, Day7 = 550px (scrollable)
            var dayData = new (int day, int state)[]
            {
                (1, 0), (2, 0), (3, 0), (4, 0),
                (5, 1),
                (6, 2), (7, 2),
            };

            foreach (var d in dayData)
            {
                CreateVerticalDayCard(content.transform, d.day, d.state, d.day == 7);
            }

            Debug.Log("[DailyRewardsUI] RewardsScrollView creado (7 cards verticales con scroll)");
        }

        /// <summary>
        /// Creates a full-width day card for the vertical scroll layout.
        /// Day 7 is taller and more prominent with gold accents.
        /// </summary>
        private static void CreateVerticalDayCard(Transform parent, int day, int state, bool isDay7)
        {
            bool claimed = state == 0;
            bool current = state == 1;
            bool locked  = state == 2;

            float cardHeight = isDay7 ? 550f : 350f;

            var card = new GameObject($"Day{day}");
            card.transform.SetParent(parent, false);
            card.AddComponent<RectTransform>();

            var cardLE = card.AddComponent<LayoutElement>();
            cardLE.preferredHeight = cardHeight;
            cardLE.flexibleWidth = 1;

            // Card background — same for all cards
            var cardBg = card.AddComponent<Image>();
            cardBg.color = CARD_BG;

            // Outline — tier progression: grey → cyan → purple → gold
            var outline = card.AddComponent<Outline>();
            if (current)
            {
                outline.effectColor = GOLD;
                outline.effectDistance = new Vector2(3, 3);
            }
            else if (claimed)
            {
                outline.effectColor = GREEN_SUCCESS;
                outline.effectDistance = new Vector2(1, 1);
            }
            else
            {
                // Locked tier colors
                Color tierColor = day switch
                {
                    1 or 2 => new Color(0.25f, 0.25f, 0.3f, 0.6f),        // grey
                    3 or 4 => new Color(0f, 0.4f, 0.4f, 0.7f),             // teal/cyan dark
                    5 => CYAN_NEON,                                          // cyan bright
                    6 => new Color(0.6f, 0.2f, 1f, 0.8f),                  // purple
                    7 => GOLD,                                               // gold always
                    _ => new Color(0.15f, 0.15f, 0.2f, 0.4f)
                };
                outline.effectColor = tierColor;
                outline.effectDistance = new Vector2(day >= 5 ? 2 : 1, day >= 5 ? 2 : 1);
            }

            // Shadow
            var cardShadow = card.AddComponent<Shadow>();
            cardShadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            cardShadow.effectDistance = new Vector2(3, -4);

            // No VLG — pure anchor-based layout for reliable sizing
            // Layout: DayLabel (top 15%) | GiftIcon (middle 70-85%) | GrandPrize (bottom 12%, Day7 only)

            float labelBot = isDay7 ? 0.88f : 0.85f;
            float iconTop = labelBot;
            float iconBot = isDay7 ? 0.12f : 0.02f;

            // === DAY LABEL (top) ===
            var dayLabel = new GameObject("DayLabel");
            dayLabel.transform.SetParent(card.transform, false);
            var dlRT = dayLabel.AddComponent<RectTransform>();
            dlRT.anchorMin = new Vector2(0, labelBot);
            dlRT.anchorMax = new Vector2(1, 0.98f);
            dlRT.offsetMin = new Vector2(10, 0);
            dlRT.offsetMax = new Vector2(-10, 0);
            var dlTMP = dayLabel.AddComponent<TextMeshProUGUI>();
            dlTMP.text = $"DAY {day}";
            dlTMP.fontSize = isDay7 ? FontSizes.H2 : FontSizes.H3;
            dlTMP.fontStyle = FontStyles.Bold;
            dlTMP.enableAutoSizing = true;
            dlTMP.fontSizeMin = FontSizes.AutoMinBody;
            dlTMP.fontSizeMax = dlTMP.fontSize;
            dlTMP.overflowMode = TextOverflowModes.Ellipsis;
            dlTMP.alignment = TextAlignmentOptions.Center;

            if (current)
                dlTMP.color = GOLD;
            else if (claimed)
                dlTMP.color = GREEN_SUCCESS;
            else if (isDay7)
                dlTMP.color = GOLD;
            else
                dlTMP.color = TEXT_WHITE;

            // === GIFT ICON (center, fills most of the card) ===
            var iconContainer = new GameObject("GiftIcon");
            iconContainer.transform.SetParent(card.transform, false);
            var icRT = iconContainer.AddComponent<RectTransform>();
            icRT.anchorMin = new Vector2(0.15f, iconBot);
            icRT.anchorMax = new Vector2(0.85f, iconTop);
            icRT.offsetMin = Vector2.zero;
            icRT.offsetMax = Vector2.zero;
            var iconImg = iconContainer.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.color = Color.white;

            string giftPath = GetGiftIconForDay(day);
            Sprite giftSprite = AssetDatabase.LoadAssetAtPath<Sprite>(giftPath);
            if (giftSprite != null)
            {
                iconImg.sprite = giftSprite;
                if (claimed) iconImg.color = new Color(1f, 1f, 1f, 0.5f);
                if (locked) iconImg.color = new Color(0.5f, 0.5f, 0.5f, 0.6f);
            }
            else
            {
                Color fallbackColor = day switch
                {
                    1 or 2 => new Color(0.8f, 0.5f, 0.2f, 1f),
                    3 or 4 => new Color(0.75f, 0.75f, 0.8f, 1f),
                    5 or 6 or 7 => GOLD,
                    _ => TEXT_WHITE
                };
                if (claimed) fallbackColor.a = 0.4f;
                if (locked) fallbackColor = new Color(0.2f, 0.2f, 0.25f, 0.6f);
                iconImg.color = fallbackColor;
            }

            // === GRAND PRIZE LABEL (bottom, Day7 only) ===
            if (isDay7)
            {
                var prizeLabel = new GameObject("GrandPrizeLabel");
                prizeLabel.transform.SetParent(card.transform, false);
                var plRT = prizeLabel.AddComponent<RectTransform>();
                plRT.anchorMin = new Vector2(0, 0.01f);
                plRT.anchorMax = new Vector2(1, iconBot);
                plRT.offsetMin = new Vector2(10, 0);
                plRT.offsetMax = new Vector2(-10, 0);
                var plTMP = prizeLabel.AddComponent<TextMeshProUGUI>();
                plTMP.text = "Grand Prize";
                plTMP.fontSize = FontSizes.H2;
                plTMP.fontStyle = FontStyles.Bold;
                plTMP.enableAutoSizing = true;
                plTMP.fontSizeMin = FontSizes.AutoMinTitle;
                plTMP.fontSizeMax = plTMP.fontSize;
                plTMP.color = GOLD;
                plTMP.alignment = TextAlignmentOptions.Center;
                plTMP.overflowMode = TextOverflowModes.Ellipsis;
            }

            // === TODAY BADGE (current day only) ===
            if (current)
            {
                var badge = new GameObject("TodayBadge");
                badge.transform.SetParent(card.transform, false);
                var bdRT = badge.AddComponent<RectTransform>();
                bdRT.anchorMin = new Vector2(1, 1);
                bdRT.anchorMax = new Vector2(1, 1);
                bdRT.pivot = new Vector2(1, 1);
                bdRT.anchoredPosition = new Vector2(-10, -10);
                bdRT.sizeDelta = new Vector2(140, 40);
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
                bttTMP.fontSize = FontSizes.Body;
                bttTMP.fontStyle = FontStyles.Bold;
                bttTMP.enableAutoSizing = true;
                bttTMP.fontSizeMin = FontSizes.AutoMinSmall;
                bttTMP.fontSizeMax = FontSizes.Body;
                bttTMP.color = TEXT_DARK;
                bttTMP.alignment = TextAlignmentOptions.Center;
            }
        }

        #endregion

        #region 3. Claim Button

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

            // Remove old HLG if present
            var oldHLG = timerBar.GetComponent<HorizontalLayoutGroup>();
            if (oldHLG != null) Object.DestroyImmediate(oldHLG);

            // Remove old separate elements
            var oldLabel = timerBar.transform.Find("NextRewardLabel");
            if (oldLabel != null) Object.DestroyImmediate(oldLabel.gameObject);
            var oldIcon = timerBar.transform.Find("TimerIcon");
            if (oldIcon != null) Object.DestroyImmediate(oldIcon.gameObject);

            // Single TimeText centered — runtime writes full localized string here
            var timeText = FindOrCreate(timerBar.transform, "TimeText");
            var ttRT = GetOrAdd<RectTransform>(timeText);
            ttRT.anchorMin = Vector2.zero;
            ttRT.anchorMax = Vector2.one;
            ttRT.offsetMin = new Vector2(20, 0);
            ttRT.offsetMax = new Vector2(-20, 0);
            var ttTMP = GetOrAdd<TextMeshProUGUI>(timeText);
            ttTMP.text = "Next reward in: 14h 32m 15s";
            ttTMP.enableAutoSizing = true;
            ttTMP.fontSizeMin = FontSizes.Caption;
            ttTMP.fontSizeMax = FontSizes.Body;
            ttTMP.fontStyle = FontStyles.Bold;
            ttTMP.color = TEXT_SECONDARY;
            ttTMP.alignment = TextAlignmentOptions.Center;
            ttTMP.enableWordWrapping = false;
            ttTMP.overflowMode = TextOverflowModes.Overflow;

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
            siTMP.enableAutoSizing = true;
            siTMP.fontSizeMin = FontSizes.AutoMinBody;
            siTMP.fontSizeMax = siTMP.fontSize;
            siTMP.fontStyle = FontStyles.Bold;
            siTMP.color = ORANGE_FIRE;
            siTMP.alignment = TextAlignmentOptions.Center;

            // === ContinueButton (FULL SCREEN tap area — tap anywhere to continue) ===
            var continueBtn = FindOrCreate(blocker.transform, "ContinueButton");
            var cbRT = GetOrAdd<RectTransform>(continueBtn);
            cbRT.anchorMin = Vector2.zero;
            cbRT.anchorMax = Vector2.one;
            cbRT.offsetMin = Vector2.zero;
            cbRT.offsetMax = Vector2.zero;
            // Transparent bg — full screen tap target
            var conBg = GetOrAdd<Image>(continueBtn);
            conBg.color = new Color(0, 0, 0, 0);
            var conBtn = GetOrAdd<Button>(continueBtn);
            conBtn.targetGraphic = conBg;
            conBtn.transition = Selectable.Transition.None;

            var conText = FindOrCreate(continueBtn.transform, "TapToContinueText");
            var cnRT = GetOrAdd<RectTransform>(conText);
            cnRT.anchorMin = new Vector2(0, 0.02f);
            cnRT.anchorMax = new Vector2(1, 0.08f);
            cnRT.offsetMin = Vector2.zero;
            cnRT.offsetMax = Vector2.zero;
            var cnTMP = GetOrAdd<TextMeshProUGUI>(conText);
            cnTMP.text = "TAP TO CONTINUE";
            cnTMP.fontSize = FontSizes.Body;
            cnTMP.fontStyle = FontStyles.Bold;
            cnTMP.color = new Color(1f, 1f, 1f, 0.7f);
            cnTMP.alignment = TextAlignmentOptions.Center;
            cnTMP.enableAutoSizing = true;
            cnTMP.fontSizeMin = FontSizes.AutoMinBody;
            cnTMP.fontSizeMax = FontSizes.Body;

            Debug.Log("[DailyRewardsUI] ClaimAnimationPopup (Clash Royale style) creado");
        }

        #endregion

        #region 10. Milestone Popup (hidden)

        // MilestonePopup removed — Model 2 has no streak milestones

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
            SetRef(so, "nextResetText", FindInPath<TextMeshProUGUI>(r, "TimerBar/TimeText"));

            // UI - Current Day (TodayPanel removed — refs cleared)

            // UI - Rewards Grid
            // ScrollView — cards inside Viewport/Content
            Transform scrollContent = FindInPath<Transform>(r, "RewardsScrollView/Viewport/Content");
            if (scrollContent != null) SetRef(so, "rewardsContainer", scrollContent);

            var daysInCycleProp = so.FindProperty("daysInCycle");
            if (daysInCycleProp != null) daysInCycleProp.intValue = 7;

            // UI - Claim Button
            SetRef(so, "claimButton", FindInPath<Button>(r, "ClaimButton"));
            SetRef(so, "claimButtonText", FindInPath<TextMeshProUGUI>(r, "ClaimButton/Text"));
            Transform claimGlow = r.Find("ClaimGlow");
            if (claimGlow != null) SetRef(so, "claimGlow", claimGlow.gameObject);

            // UI - Bonus Info (streak removed)

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
            SetRef(so, "continueButton", FindInPath<Button>(r, "ClaimAnimationBlocker/ContinueButton"));

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
