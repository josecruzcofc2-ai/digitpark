using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;
using DigitPark.Monetization;

namespace DigitPark.Editor
{
    /// <summary>
    /// MainMenu UI Builder v3 - Rediseño 2026
    /// Layout proporcional que llena toda la pantalla (2.5% top, 2% bottom)
    /// Cash Battle DORADO prominente como card principal
    /// Portrait 9:16 (1080x1920), matchWidthOrHeight=0
    /// </summary>
    public class MainMenuUIBuilder : EditorWindow
    {
        #region Colors

        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_GLOW = new Color(0f, 0.85f, 1f, 0.8f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.5f, 1f);

        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GOLD_DARK = new Color(0.8f, 0.6f, 0.1f, 1f);
        private static readonly Color GOLD_GLOW = new Color(1f, 0.75f, 0f, 0.7f);

        private static readonly Color DARK_BG = new Color(0.02f, 0.04f, 0.08f, 1f);
        private static readonly Color CARD_BG = new Color(0.06f, 0.08f, 0.12f, 1f);
        private static readonly Color CARD_BG_LIGHT = new Color(0.08f, 0.1f, 0.14f, 1f);
        private static readonly Color HEADER_BG = new Color(0.04f, 0.06f, 0.1f, 0.98f);

        private static readonly Color TEXT_WHITE = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.6f, 0.65f, 1f);
        private static readonly Color TEXT_DARK = new Color(0.1f, 0.1f, 0.1f, 1f);

        private static readonly Color GREEN_SUCCESS = new Color(0.2f, 0.9f, 0.4f, 1f);
        private static readonly Color PURPLE_ACCENT = new Color(0.6f, 0.3f, 1f, 1f);
        private static readonly Color ORANGE_ACCENT = new Color(1f, 0.5f, 0f, 1f);

        #endregion

        #region Layout Anchors (Y: 0=bottom, 1=top)

        // Sections fill screen with 1% gaps, 2.5% top padding, 2% bottom padding
        private const float HEADER_TOP = 1.0f;
        private const float HEADER_BOT = 0.928f;

        private const float PROFILE_TOP = 0.920f;
        private const float PROFILE_BOT = 0.735f;

        private const float DAILY_TOP = 0.725f;
        private const float DAILY_BOT = 0.650f;

        private const float QUICK_TOP = 0.640f;
        private const float QUICK_BOT = 0.575f;

        private const float PLAY_TOP = 0.565f;
        private const float PLAY_BOT = 0.410f;

        private const float CASH_TOP = 0.400f;
        private const float CASH_BOT = 0.225f;

        private const float EXTRA_TOP = 0.215f;
        private const float EXTRA_BOT = 0.02f;

        private const float SIDE_PAD = 20f;

        #endregion

        #region Icon Paths

        private const string ICONS_BASE = "Assets/_Project/Art/Icons";
        private const string ICON_SETTINGS = ICONS_BASE + "/Navigation/Actions/SettingsIcon.png";
        private const string ICON_NOTIFICATIONS = ICONS_BASE + "/Navigation/Actions/NotificationsIcon.png";
        private const string ICON_NOTIFICATIONS_ACTIVE = ICONS_BASE + "/Navigation/Actions/NotificationsActiveIcon.png";
        private const string ICON_AVATAR_DEFAULT = ICONS_BASE + "/Social/Profile/AvatarDefault.png";
        private const string ICON_GEM = ICONS_BASE + "/Currency/icon_digitgem_single.png";
        private const string ICON_COIN = ICONS_BASE + "/Currency/icon_digitcoin_single.png";
        private const string ICON_RANKINGS = ICONS_BASE + "/Tournaments/RankingsIcon.png";
        private const string ICON_SEARCH = ICONS_BASE + "/Navigation/Buttons/SearchIcon.png";
        private const string ICON_MISSIONS = ICONS_BASE + "/Missions/MissionsIcon.png";
        private const string ICON_PLAY = ICONS_BASE + "/UI/PlayIcon.png";
        private const string ICON_CASH_BATTLE = ICONS_BASE + "/CashBattle/UI/CashBattleIcon.png";
        private const string ICON_GIFT = ICONS_BASE + "/DailyRewards/GiftIcon.png";
        private const string ICON_ACHIEVEMENTS = ICONS_BASE + "/Monetization/AchievementsIcon.png";
        private const string ICON_SHOP = ICONS_BASE + "/Monetization/ShopIcon.png";
        private const string ICON_PREMIUM = ICONS_BASE + "/Premium/PremiumIcon.png";

        #endregion

        [MenuItem("DigitPark/UI Builders/Core/MainMenu", false, 110)]
        public static void ShowWindow()
        {
            GetWindow<MainMenuUIBuilder>("MainMenu Builder v3");
        }

        private void OnGUI()
        {
            GUILayout.Label("MainMenu UI Builder v3", EditorStyles.boldLabel);
            GUILayout.Label("Rediseño 2026 - Sin espacio muerto, Cash Battle DORADO", EditorStyles.miniLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Layout completo (de arriba a abajo):\n\n" +
                "1. Header (Settings, Logo, Notificaciones)\n" +
                "2. Profile Card (Avatar grande + stats + nivel)\n" +
                "3. Daily Reward (movida arriba, reclamar)\n" +
                "4. Quick Access (Rankings, Buscar, Misiones)\n" +
                "5. JUGAR (Card cyan neon)\n" +
                "6. CASH BATTLE (Card DORADO prominente)\n" +
                "7. Extra Row (Logros, Tienda, Premium)\n" +
                "8. Paneles overlay (Premium, Notificaciones)",
                MessageType.Info);

            GUILayout.Space(15);

            GUI.backgroundColor = CYAN_NEON;
            if (GUILayout.Button("RECONSTRUIR MAINMENU COMPLETO", GUILayout.Height(50)))
                RebuildMainMenu();
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);
            GUILayout.Label("Secciones individuales:", EditorStyles.boldLabel);

            if (GUILayout.Button("1. Header", GUILayout.Height(25))) CreateHeader();
            if (GUILayout.Button("2. Profile Card", GUILayout.Height(25))) CreateProfileCard();
            if (GUILayout.Button("3. Daily Reward", GUILayout.Height(25))) CreateDailyReward();
            if (GUILayout.Button("4. Quick Access", GUILayout.Height(25))) CreateQuickAccess();
            if (GUILayout.Button("5. JUGAR", GUILayout.Height(25))) CreatePlayCard();
            if (GUILayout.Button("6. CASH BATTLE", GUILayout.Height(25))) CreateCashBattleCard();
            if (GUILayout.Button("7. Extra Row", GUILayout.Height(25))) CreateExtraRow();
            if (GUILayout.Button("8. Paneles", GUILayout.Height(25))) CreatePanels();

            GUILayout.Space(15);

            GUI.backgroundColor = GOLD;
            if (GUILayout.Button("ASIGNAR REFERENCIAS AL MANAGER", GUILayout.Height(35)))
                SetupManagerReferences();
            GUI.backgroundColor = Color.white;

            GUILayout.Space(5);

            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
            if (GUILayout.Button("ASIGNAR ICONOS NEON", GUILayout.Height(35)))
                AssignNeonIcons();
            GUI.backgroundColor = Color.white;
        }

        #region Main Rebuild

        private static void RebuildMainMenu()
        {
            CleanupOldUI();

            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("[MainMenuUI] No se encontró Canvas");
                return;
            }

            // CanvasScaler
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0f;
            }

            // Limpiar elementos viejos
            string[] oldNames = {
                "Background", "Header", "PlayerCard", "ProfileCard",
                "QuickActionsPanel", "MainCardsPanel", "DailyRewardsCard", "DailyRewardCard",
                "PlayCard", "CashBattleCard", "ExtraRow", "PremiumPanel",
                "NotificationsPanel", "PremiumBanner", "TitleSection", "UserSection",
                "MainMenuPanel", "SafeArea"
            };
            foreach (var n in oldNames)
            {
                Transform t = canvas.transform.Find(n);
                if (t != null) DestroyImmediate(t.gameObject);
            }

            // Crear estructura completa
            CreateBackground(canvas.transform);
            CreateHeader();
            CreateProfileCard();
            CreateDailyReward();
            CreateQuickAccess();
            CreatePlayCard();
            CreateCashBattleCard();
            CreateExtraRow();
            CreatePanels();
            SetupManagerReferences();

            Debug.Log("[MainMenuUI] ¡MainMenu v3 REDISEÑADO exitosamente!");
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

        #region Header

        private static void CreateHeader()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var header = FindOrCreate(canvas.transform, "Header");
            var rt = GetOrAdd<RectTransform>(header);
            SetAnchors(rt, 0, HEADER_BOT, 1, HEADER_TOP);
            GetOrAdd<Image>(header).color = HEADER_BG;

            // Settings Button (left)
            CreateIconButton(header.transform, "SettingsButton",
                new Vector2(0, 0.5f), new Vector2(50, 0), new Vector2(100, 100));

            // Logo DIGIT PARK (center-left, narrower to make room for currency)
            var logo = FindOrCreate(header.transform, "LogoText");
            var logoRT = GetOrAdd<RectTransform>(logo);
            logoRT.anchorMin = new Vector2(0.12f, 0);
            logoRT.anchorMax = new Vector2(0.52f, 1);
            logoRT.offsetMin = Vector2.zero;
            logoRT.offsetMax = Vector2.zero;

            var logoTMP = GetOrAdd<TextMeshProUGUI>(logo);
            logoTMP.text = "DIGIT PARK";
            logoTMP.fontSize = FontSizes.SceneTitle;
            logoTMP.color = CYAN_NEON;
            logoTMP.fontStyle = FontStyles.Bold;
            logoTMP.alignment = TextAlignmentOptions.Center;
            logoTMP.enableVertexGradient = true;
            logoTMP.colorGradient = new VertexGradient(CYAN_NEON, CYAN_NEON, CYAN_GLOW, CYAN_GLOW);

            // Currency Display (between logo and notifications)
            CreateMainMenuCurrencyDisplay(header.transform);

            // Notifications Button (far right)
            var notifBtn = CreateIconButton(header.transform, "NotificationsButton",
                new Vector2(1, 0.5f), new Vector2(-50, 0), new Vector2(80, 80));

            // Notification Badge
            var badge = FindOrCreate(notifBtn.transform, "Badge");
            var badgeRT = GetOrAdd<RectTransform>(badge);
            badgeRT.anchorMin = new Vector2(1, 1);
            badgeRT.anchorMax = new Vector2(1, 1);
            badgeRT.pivot = new Vector2(0.5f, 0.5f);
            badgeRT.anchoredPosition = new Vector2(-5, -5);
            badgeRT.sizeDelta = new Vector2(44, 44);
            GetOrAdd<Image>(badge).color = new Color(1, 0.2f, 0.2f, 1);

            var badgeText = FindOrCreate(badge.transform, "BadgeText");
            var btRT = GetOrAdd<RectTransform>(badgeText);
            btRT.anchorMin = Vector2.zero;
            btRT.anchorMax = Vector2.one;
            btRT.offsetMin = Vector2.zero;
            btRT.offsetMax = Vector2.zero;
            var btTMP = GetOrAdd<TextMeshProUGUI>(badgeText);
            btTMP.text = "3";
            btTMP.fontSize = FontSizes.Body;
            btTMP.color = TEXT_WHITE;
            btTMP.fontStyle = FontStyles.Bold;
            btTMP.alignment = TextAlignmentOptions.Center;

            Debug.Log("[MainMenuUI] Header creado");
        }

        /// <summary>
        /// Creates the currency display pills in the MainMenu header.
        /// Two pills: GemsDisplay (gem icon + amount) and CoinsDisplay (coin icon + amount).
        /// Tapping each pill navigates to the Shop (corresponding section).
        /// </summary>
        private static void CreateMainMenuCurrencyDisplay(Transform headerTransform)
        {
            // Container anchored right, before notifications button
            var container = FindOrCreate(headerTransform, "CurrencyDisplay");
            var cRT = GetOrAdd<RectTransform>(container);
            cRT.anchorMin = new Vector2(0.52f, 0.1f);
            cRT.anchorMax = new Vector2(0.88f, 0.9f);
            cRT.offsetMin = Vector2.zero;
            cRT.offsetMax = Vector2.zero;

            var hlg = GetOrAdd<HorizontalLayoutGroup>(container);
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleRight;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.reverseArrangement = false;
            hlg.padding = new RectOffset(0, 0, 4, 4);

            // Remove old children if rebuilding
            for (int i = container.transform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(container.transform.GetChild(i).gameObject);

            // Gems pill
            var gemColor = PURPLE_ACCENT;
            CreateCurrencyPill(container, "GemsDisplay", "0", gemColor, CurrencyType.DigitGems);

            // Coins pill
            var coinColor = new Color(1f, 0.85f, 0.3f, 1f); // Gold
            CreateCurrencyPill(container, "CoinsDisplay", "0", coinColor, CurrencyType.DigitCoins);
        }

        /// <summary>
        /// Creates a single currency pill: [icon] [amount] [+]
        /// Attaches CurrencyDisplayUI component for runtime auto-subscription.
        /// </summary>
        private static void CreateCurrencyPill(GameObject parent, string name, string amount, Color color, CurrencyType currencyType)
        {
            var pill = FindOrCreate(parent.transform, name);
            var rt = GetOrAdd<RectTransform>(pill);
            rt.sizeDelta = new Vector2(145, 46);

            // Pill background
            var bg = GetOrAdd<Image>(pill);
            bg.color = new Color(0.08f, 0.12f, 0.18f, 0.95f);
            var outline = GetOrAdd<Outline>(pill);
            outline.effectColor = color * 0.6f;
            outline.effectDistance = new Vector2(1, 1);

            // Button for tap → navigate to shop
            var btn = GetOrAdd<Button>(pill);
            btn.targetGraphic = bg;

            // Layout
            var pillHLG = GetOrAdd<HorizontalLayoutGroup>(pill);
            pillHLG.spacing = 6;
            pillHLG.padding = new RectOffset(8, 8, 4, 4);
            pillHLG.childAlignment = TextAnchor.MiddleCenter;
            pillHLG.childControlWidth = false;
            pillHLG.childControlHeight = true;

            var le = GetOrAdd<LayoutElement>(pill);
            le.minWidth = 145;
            le.preferredWidth = 145;

            // Remove old children if rebuilding
            for (int i = pill.transform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(pill.transform.GetChild(i).gameObject);

            // Icon
            var icon = new GameObject("Icon");
            icon.transform.SetParent(pill.transform, false);
            var iconImg = icon.AddComponent<Image>();
            iconImg.color = color;
            iconImg.preserveAspect = true;
            var iconLE = icon.AddComponent<LayoutElement>();
            iconLE.minWidth = 30;
            iconLE.minHeight = 30;
            iconLE.preferredWidth = 30;
            iconLE.preferredHeight = 30;

            // Load icon sprite
            string iconPath = currencyType == CurrencyType.DigitGems ? ICON_GEM : ICON_COIN;
            Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            if (iconSprite != null)
            {
                iconImg.sprite = iconSprite;
                iconImg.color = Color.white;
            }

            // Amount text
            var amountObj = new GameObject("Amount");
            amountObj.transform.SetParent(pill.transform, false);
            amountObj.AddComponent<RectTransform>();
            var amountText = amountObj.AddComponent<TextMeshProUGUI>();
            amountText.text = amount;
            amountText.fontSize = FontSizes.Body;
            amountText.fontStyle = FontStyles.Bold;
            amountText.color = TEXT_WHITE;
            amountText.alignment = TextAlignmentOptions.MidlineLeft;
            var amountLE = amountObj.AddComponent<LayoutElement>();
            amountLE.flexibleWidth = 1;

            // Plus indicator
            var plus = new GameObject("Plus");
            plus.transform.SetParent(pill.transform, false);
            var plusImg = plus.AddComponent<Image>();
            plusImg.color = color;
            var plusLE = plus.AddComponent<LayoutElement>();
            plusLE.minWidth = 20;
            plusLE.minHeight = 20;
            plusLE.preferredWidth = 20;
            plusLE.preferredHeight = 20;

            // Attach CurrencyDisplayUI component for runtime auto-subscription
            var currencyUI = GetOrAdd<CurrencyDisplayUI>(pill);
            var so = new SerializedObject(currencyUI);
            so.FindProperty("_currencyType").enumValueIndex = (int)currencyType;
            so.FindProperty("_iconImage").objectReferenceValue = iconImg;
            so.FindProperty("_amountText").objectReferenceValue = amountText;
            so.FindProperty("_button").objectReferenceValue = btn;
            so.FindProperty("_plusButton").objectReferenceValue = plus;
            if (iconSprite != null)
            {
                if (currencyType == CurrencyType.DigitGems)
                    so.FindProperty("_gemsIcon").objectReferenceValue = iconSprite;
                else
                    so.FindProperty("_coinsIcon").objectReferenceValue = iconSprite;
            }
            so.ApplyModifiedProperties();
        }

        private static GameObject CreateIconButton(Transform parent, string name,
            Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var btn = FindOrCreate(parent, name);
            var rt = GetOrAdd<RectTransform>(btn);
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var bg = GetOrAdd<Image>(btn);
            bg.color = new Color(1, 1, 1, 0.06f);
            GetOrAdd<Button>(btn).targetGraphic = bg;

            var icon = FindOrCreate(btn.transform, "Icon");
            var iconRT = GetOrAdd<RectTransform>(icon);
            iconRT.anchorMin = new Vector2(0.15f, 0.15f);
            iconRT.anchorMax = new Vector2(0.85f, 0.85f);
            iconRT.offsetMin = Vector2.zero;
            iconRT.offsetMax = Vector2.zero;
            var iconImg = GetOrAdd<Image>(icon);
            iconImg.color = TEXT_WHITE;
            iconImg.preserveAspect = true;

            return btn;
        }

        #endregion

        #region Profile Card

        private static void CreateProfileCard()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var card = FindOrCreate(canvas.transform, "ProfileCard");
            var rt = GetOrAdd<RectTransform>(card);
            SetAnchorsWithPad(rt, PROFILE_BOT, PROFILE_TOP);

            var bg = GetOrAdd<Image>(card);
            bg.color = CARD_BG;
            var outline = GetOrAdd<Outline>(card);
            outline.effectColor = CYAN_DARK;
            outline.effectDistance = new Vector2(2, 2);

            // Whole card is userButton
            var btn = GetOrAdd<Button>(card);
            btn.targetGraphic = bg;

            // === Avatar Section (left 30%) ===
            var avSec = FindOrCreate(card.transform, "AvatarSection");
            var avSecRT = GetOrAdd<RectTransform>(avSec);
            avSecRT.anchorMin = new Vector2(0, 0);
            avSecRT.anchorMax = new Vector2(0.30f, 1);
            avSecRT.offsetMin = new Vector2(10, 10);
            avSecRT.offsetMax = new Vector2(0, -10);

            // Avatar Frame (dark bg + white ring outline)
            var frame = FindOrCreate(avSec.transform, "AvatarFrame");
            var frameRT = GetOrAdd<RectTransform>(frame);
            frameRT.anchorMin = new Vector2(0.5f, 0.55f);
            frameRT.anchorMax = new Vector2(0.5f, 0.55f);
            frameRT.sizeDelta = new Vector2(240, 240);
            GetOrAdd<Image>(frame).color = CARD_BG;
            var fo = GetOrAdd<Outline>(frame);
            fo.effectColor = Color.white;
            fo.effectDistance = new Vector2(4, 4);

            // Avatar Image
            var avImg = FindOrCreate(frame.transform, "AvatarImage");
            var avImgRT = GetOrAdd<RectTransform>(avImg);
            avImgRT.anchorMin = new Vector2(0.06f, 0.06f);
            avImgRT.anchorMax = new Vector2(0.94f, 0.94f);
            avImgRT.offsetMin = Vector2.zero;
            avImgRT.offsetMax = Vector2.zero;
            var avImgComp = GetOrAdd<Image>(avImg);
            avImgComp.color = Color.white;
            avImgComp.preserveAspect = true;

            // AvatarUI component with default sprite
            Sprite defaultAvatar = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_AVATAR_DEFAULT);
            var avatarUI = GetOrAdd<DigitPark.UI.Components.AvatarUI>(avImg);
            var avatarSO = new SerializedObject(avatarUI);
            avatarSO.FindProperty("loadCurrentUserOnStart").boolValue = true;
            avatarSO.FindProperty("isEditable").boolValue = false;
            avatarSO.FindProperty("avatarImage").objectReferenceValue = avImgComp;
            if (defaultAvatar != null)
            {
                avatarSO.FindProperty("defaultAvatarSprite").objectReferenceValue = defaultAvatar;
            }
            avatarSO.ApplyModifiedProperties();

            // Level Badge
            var lvlBadge = FindOrCreate(avSec.transform, "LevelBadge");
            var lvlRT = GetOrAdd<RectTransform>(lvlBadge);
            lvlRT.anchorMin = new Vector2(0.5f, 0.15f);
            lvlRT.anchorMax = new Vector2(0.5f, 0.15f);
            lvlRT.sizeDelta = new Vector2(140, 56);
            GetOrAdd<Image>(lvlBadge).color = CYAN_NEON;

            var lvlText = FindOrCreate(lvlBadge.transform, "Text");
            var ltRT = GetOrAdd<RectTransform>(lvlText);
            ltRT.anchorMin = Vector2.zero;
            ltRT.anchorMax = Vector2.one;
            ltRT.offsetMin = Vector2.zero;
            ltRT.offsetMax = Vector2.zero;
            var ltTMP = GetOrAdd<TextMeshProUGUI>(lvlText);
            ltTMP.text = "Lv. 12";
            ltTMP.fontSize = FontSizes.BodyLarge;
            ltTMP.color = TEXT_DARK;
            ltTMP.fontStyle = FontStyles.Bold;
            ltTMP.alignment = TextAlignmentOptions.Center;

            // === Info Section (right 70%) ===
            var info = FindOrCreate(card.transform, "InfoSection");
            var infoRT = GetOrAdd<RectTransform>(info);
            infoRT.anchorMin = new Vector2(0.30f, 0);
            infoRT.anchorMax = new Vector2(1, 1);
            infoRT.offsetMin = new Vector2(10, 15);
            infoRT.offsetMax = new Vector2(-15, -15);

            // Username
            var user = FindOrCreate(info.transform, "Username");
            var userRT = GetOrAdd<RectTransform>(user);
            userRT.anchorMin = new Vector2(0, 0.72f);
            userRT.anchorMax = new Vector2(1, 1);
            userRT.offsetMin = Vector2.zero;
            userRT.offsetMax = Vector2.zero;
            var userTMP = GetOrAdd<TextMeshProUGUI>(user);
            userTMP.text = "@Username";
            userTMP.fontSize = FontSizes.DisplayMedium;
            userTMP.color = TEXT_WHITE;
            userTMP.fontStyle = FontStyles.Bold;
            userTMP.alignment = TextAlignmentOptions.Left;

            // Stats Row
            var stats = FindOrCreate(info.transform, "StatsRow");
            var statsRT = GetOrAdd<RectTransform>(stats);
            statsRT.anchorMin = new Vector2(0, 0.38f);
            statsRT.anchorMax = new Vector2(1, 0.68f);
            statsRT.offsetMin = Vector2.zero;
            statsRT.offsetMax = Vector2.zero;

            var hlg = GetOrAdd<HorizontalLayoutGroup>(stats);
            hlg.spacing = 40;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            // Recreate stat items
            for (int i = stats.transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(stats.transform.GetChild(i).gameObject);
            CreateStatItem(stats.transform, "DigitGems", "500", PURPLE_ACCENT);
            CreateStatItem(stats.transform, "DigitCoins", "2,400", CYAN_NEON);

            // Streak
            var streak = FindOrCreate(info.transform, "StreakRow");
            var stRT = GetOrAdd<RectTransform>(streak);
            stRT.anchorMin = new Vector2(0, 0.05f);
            stRT.anchorMax = new Vector2(1, 0.35f);
            stRT.offsetMin = Vector2.zero;
            stRT.offsetMax = Vector2.zero;
            var stTMP = GetOrAdd<TextMeshProUGUI>(streak);
            stTMP.text = "Streak: 5 wins";
            stTMP.fontSize = FontSizes.BodyLarge;
            stTMP.color = GREEN_SUCCESS;
            stTMP.alignment = TextAlignmentOptions.Left;

            Debug.Log("[MainMenuUI] Profile Card creado");
        }

        private static void CreateStatItem(Transform parent, string name, string value, Color color)
        {
            var stat = new GameObject(name);
            stat.transform.SetParent(parent, false);
            stat.AddComponent<RectTransform>().sizeDelta = new Vector2(160, 100);
            stat.AddComponent<LayoutElement>().preferredWidth = 160;

            var vlg = stat.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 2;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            var icon = new GameObject("Icon");
            icon.transform.SetParent(stat.transform, false);
            icon.AddComponent<RectTransform>();
            icon.AddComponent<LayoutElement>().preferredHeight = 104;
            var iconImg = icon.AddComponent<Image>();
            iconImg.color = color;
            iconImg.preserveAspect = true;

            var val = new GameObject("Value");
            val.transform.SetParent(stat.transform, false);
            val.AddComponent<RectTransform>();
            val.AddComponent<LayoutElement>().preferredHeight = 40;
            var valTMP = val.AddComponent<TextMeshProUGUI>();
            valTMP.text = value;
            valTMP.fontSize = FontSizes.BodyLarge;
            valTMP.color = color;
            valTMP.fontStyle = FontStyles.Bold;
            valTMP.alignment = TextAlignmentOptions.Center;
        }

        #endregion

        #region Daily Reward

        private static void CreateDailyReward()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var card = FindOrCreate(canvas.transform, "DailyRewardCard");
            var rt = GetOrAdd<RectTransform>(card);
            SetAnchorsWithPad(rt, DAILY_BOT, DAILY_TOP);

            var bg = GetOrAdd<Image>(card);
            bg.color = new Color(0.12f, 0.06f, 0.20f, 1f);
            GetOrAdd<Button>(card).targetGraphic = bg;

            var outline = GetOrAdd<Outline>(card);
            outline.effectColor = PURPLE_ACCENT;
            outline.effectDistance = new Vector2(2, 2);

            // Gift Icon
            var icon = FindOrCreate(card.transform, "GiftIcon");
            var iconRT = GetOrAdd<RectTransform>(icon);
            iconRT.anchorMin = new Vector2(0, 0.5f);
            iconRT.anchorMax = new Vector2(0, 0.5f);
            iconRT.pivot = new Vector2(0, 0.5f);
            iconRT.anchoredPosition = new Vector2(20, 0);
            iconRT.sizeDelta = new Vector2(120, 120);
            var iconImg = GetOrAdd<Image>(icon);
            iconImg.color = GOLD;
            iconImg.preserveAspect = true;

            // Text Container
            var textC = FindOrCreate(card.transform, "TextContainer");
            var tcRT = GetOrAdd<RectTransform>(textC);
            tcRT.anchorMin = new Vector2(0, 0);
            tcRT.anchorMax = new Vector2(0.65f, 1);
            tcRT.offsetMin = new Vector2(155, 15);
            tcRT.offsetMax = new Vector2(0, -15);

            var title = FindOrCreate(textC.transform, "Title");
            var tRT = GetOrAdd<RectTransform>(title);
            tRT.anchorMin = new Vector2(0, 0.55f);
            tRT.anchorMax = new Vector2(1, 1);
            tRT.offsetMin = Vector2.zero;
            tRT.offsetMax = Vector2.zero;
            var tTMP = GetOrAdd<TextMeshProUGUI>(title);
            tTMP.text = "DAILY REWARD";
            tTMP.fontSize = FontSizes.Button;
            tTMP.color = GOLD;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.Left;

            var sub = FindOrCreate(textC.transform, "Subtitle");
            var sRT = GetOrAdd<RectTransform>(sub);
            sRT.anchorMin = new Vector2(0, 0);
            sRT.anchorMax = new Vector2(1, 0.5f);
            sRT.offsetMin = Vector2.zero;
            sRT.offsetMax = Vector2.zero;
            var sTMP = GetOrAdd<TextMeshProUGUI>(sub);
            sTMP.text = "Day 3 of 7 - Claim your reward!";
            sTMP.fontSize = FontSizes.Body;
            sTMP.color = TEXT_SECONDARY;
            sTMP.alignment = TextAlignmentOptions.Left;

            // Claim Button
            var claimBtn = FindOrCreate(card.transform, "ClaimButton");
            var cRT = GetOrAdd<RectTransform>(claimBtn);
            cRT.anchorMin = new Vector2(1, 0.5f);
            cRT.anchorMax = new Vector2(1, 0.5f);
            cRT.pivot = new Vector2(1, 0.5f);
            cRT.anchoredPosition = new Vector2(-15, 0);
            cRT.sizeDelta = new Vector2(200, 84);
            var cBg = GetOrAdd<Image>(claimBtn);
            cBg.color = GREEN_SUCCESS;
            GetOrAdd<Button>(claimBtn).targetGraphic = cBg;

            var cText = FindOrCreate(claimBtn.transform, "Text");
            var ctRT = GetOrAdd<RectTransform>(cText);
            ctRT.anchorMin = Vector2.zero;
            ctRT.anchorMax = Vector2.one;
            ctRT.offsetMin = Vector2.zero;
            ctRT.offsetMax = Vector2.zero;
            var ctTMP = GetOrAdd<TextMeshProUGUI>(cText);
            ctTMP.text = "Claim";
            ctTMP.fontSize = FontSizes.Body;
            ctTMP.color = TEXT_DARK;
            ctTMP.fontStyle = FontStyles.Bold;
            ctTMP.alignment = TextAlignmentOptions.Center;

            Debug.Log("[MainMenuUI] Daily Reward creado");
        }

        #endregion

        #region Quick Access

        private static void CreateQuickAccess()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var panel = FindOrCreate(canvas.transform, "QuickActionsPanel");
            var rt = GetOrAdd<RectTransform>(panel);
            SetAnchorsWithPad(rt, QUICK_BOT, QUICK_TOP);

            var hlg = GetOrAdd<HorizontalLayoutGroup>(panel);
            hlg.spacing = 12;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            for (int i = panel.transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(panel.transform.GetChild(i).gameObject);

            CreateQuickCard(panel.transform, "RankingsCard", "Rankings", GOLD);
            CreateQuickCard(panel.transform, "SearchCard", "Search", CYAN_NEON);
            CreateQuickCard(panel.transform, "MissionsCard", "Missions", GREEN_SUCCESS);

            Debug.Log("[MainMenuUI] Quick Access creado");
        }

        private static void CreateQuickCard(Transform parent, string name, string label, Color accent)
        {
            var card = new GameObject(name);
            card.transform.SetParent(parent, false);

            var bg = card.AddComponent<Image>();
            bg.color = CARD_BG;
            var btn = card.AddComponent<Button>();
            btn.targetGraphic = bg;

            var outline = card.AddComponent<Outline>();
            outline.effectColor = new Color(accent.r, accent.g, accent.b, 0.4f);
            outline.effectDistance = new Vector2(1.5f, 1.5f);

            var vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 4;
            vlg.padding = new RectOffset(8, 8, 10, 6);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;

            var icon = new GameObject("Icon");
            icon.transform.SetParent(card.transform, false);
            icon.AddComponent<RectTransform>();
            icon.AddComponent<LayoutElement>().preferredHeight = 72;
            var iconImg = icon.AddComponent<Image>();
            iconImg.color = Color.white;
            iconImg.preserveAspect = true;

            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(card.transform, false);
            labelGO.AddComponent<RectTransform>();
            labelGO.AddComponent<LayoutElement>().preferredHeight = 44;
            var lTMP = labelGO.AddComponent<TextMeshProUGUI>();
            lTMP.text = label;
            lTMP.fontSize = FontSizes.Body;
            lTMP.color = TEXT_WHITE;
            lTMP.fontStyle = FontStyles.Bold;
            lTMP.alignment = TextAlignmentOptions.Center;
        }

        #endregion

        #region Play Card (JUGAR - Cyan)

        private static void CreatePlayCard()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var card = FindOrCreate(canvas.transform, "PlayCard");
            var rt = GetOrAdd<RectTransform>(card);
            SetAnchorsWithPad(rt, PLAY_BOT, PLAY_TOP);

            var bg = GetOrAdd<Image>(card);
            bg.color = CYAN_NEON;
            var btn = GetOrAdd<Button>(card);
            btn.targetGraphic = bg;
            var colors = btn.colors;
            colors.highlightedColor = new Color(0, 0.9f, 0.9f, 1);
            colors.pressedColor = new Color(0, 0.7f, 0.7f, 1);
            btn.colors = colors;

            var outline = GetOrAdd<Outline>(card);
            outline.effectColor = CYAN_GLOW;
            outline.effectDistance = new Vector2(4, 4);
            var shadow = GetOrAdd<Shadow>(card);
            shadow.effectColor = new Color(0, 0.5f, 0.5f, 0.5f);
            shadow.effectDistance = new Vector2(3, -3);

            // Side (3D depth strip below card)
            var sideObj = FindOrCreate(card.transform, "Side");
            sideObj.transform.SetAsFirstSibling();
            var sideRT = GetOrAdd<RectTransform>(sideObj);
            sideRT.anchorMin = new Vector2(0, 0);
            sideRT.anchorMax = new Vector2(1, 0);
            sideRT.offsetMin = new Vector2(0, -8);
            sideRT.offsetMax = new Vector2(0, 0);
            var sideImg = GetOrAdd<Image>(sideObj);
            sideImg.color = CYAN_DARK;
            sideImg.raycastTarget = false;

            // Icon
            var icon = FindOrCreate(card.transform, "Icon");
            var iconRT = GetOrAdd<RectTransform>(icon);
            iconRT.anchorMin = new Vector2(0, 0.5f);
            iconRT.anchorMax = new Vector2(0, 0.5f);
            iconRT.pivot = new Vector2(0, 0.5f);
            iconRT.anchoredPosition = new Vector2(25, 0);
            iconRT.sizeDelta = new Vector2(210, 210);
            var iconImg = GetOrAdd<Image>(icon);
            iconImg.color = Color.white;
            iconImg.preserveAspect = true;

            // Text Container
            var textC = FindOrCreate(card.transform, "TextContainer");
            var tcRT = GetOrAdd<RectTransform>(textC);
            tcRT.anchorMin = new Vector2(0, 0);
            tcRT.anchorMax = new Vector2(1, 1);
            tcRT.offsetMin = new Vector2(250, 25);
            tcRT.offsetMax = new Vector2(-60, -25);

            var title = FindOrCreate(textC.transform, "Title");
            var tRT = GetOrAdd<RectTransform>(title);
            tRT.anchorMin = new Vector2(0, 0.5f);
            tRT.anchorMax = new Vector2(1, 1);
            tRT.offsetMin = Vector2.zero;
            tRT.offsetMax = Vector2.zero;
            var tTMP = GetOrAdd<TextMeshProUGUI>(title);
            tTMP.text = "PLAY";
            tTMP.fontSize = FontSizes.CardTitle;
            tTMP.color = TEXT_DARK;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.Left;

            var sub = FindOrCreate(textC.transform, "Subtitle");
            var sRT = GetOrAdd<RectTransform>(sub);
            sRT.anchorMin = new Vector2(0, 0);
            sRT.anchorMax = new Vector2(1, 0.45f);
            sRT.offsetMin = Vector2.zero;
            sRT.offsetMax = Vector2.zero;
            var sTMP = GetOrAdd<TextMeshProUGUI>(sub);
            sTMP.text = "Choose a game and compete";
            sTMP.fontSize = FontSizes.SectionHeader;
            sTMP.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            sTMP.alignment = TextAlignmentOptions.Left;

            // Arrow
            var arrow = FindOrCreate(card.transform, "Arrow");
            var aRT = GetOrAdd<RectTransform>(arrow);
            aRT.anchorMin = new Vector2(1, 0.5f);
            aRT.anchorMax = new Vector2(1, 0.5f);
            aRT.pivot = new Vector2(1, 0.5f);
            aRT.anchoredPosition = new Vector2(-20, 0);
            aRT.sizeDelta = new Vector2(90, 90);
            var aTMP = GetOrAdd<TextMeshProUGUI>(arrow);
            aTMP.text = "\u203A";
            aTMP.fontSize = FontSizes.AppBranding;
            aTMP.color = TEXT_DARK;
            aTMP.fontStyle = FontStyles.Bold;
            aTMP.alignment = TextAlignmentOptions.Center;

            Debug.Log("[MainMenuUI] Play Card creado");
        }

        #endregion

        #region Cash Battle Card (DORADO - Card Principal)

        private static void CreateCashBattleCard()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var card = FindOrCreate(canvas.transform, "CashBattleCard");
            var rt = GetOrAdd<RectTransform>(card);
            SetAnchorsWithPad(rt, CASH_BOT, CASH_TOP);

            // ===== FONDO DORADO =====
            var bg = GetOrAdd<Image>(card);
            bg.color = GOLD;

            var btn = GetOrAdd<Button>(card);
            btn.targetGraphic = bg;
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1, 0.95f, 0.85f, 1);
            colors.pressedColor = new Color(0.9f, 0.8f, 0.6f, 1);
            btn.colors = colors;

            // Doble glow dorado
            var outline = GetOrAdd<Outline>(card);
            outline.effectColor = GOLD_GLOW;
            outline.effectDistance = new Vector2(5, 5);

            var shadow = GetOrAdd<Shadow>(card);
            shadow.effectColor = new Color(1f, 0.6f, 0f, 0.6f);
            shadow.effectDistance = new Vector2(4, -4);

            // Side (3D depth strip below card)
            var sideObj = FindOrCreate(card.transform, "Side");
            sideObj.transform.SetAsFirstSibling();
            var sideRT = GetOrAdd<RectTransform>(sideObj);
            sideRT.anchorMin = new Vector2(0, 0);
            sideRT.anchorMax = new Vector2(1, 0);
            sideRT.offsetMin = new Vector2(0, -10);
            sideRT.offsetMax = new Vector2(0, 0);
            var sideImg = GetOrAdd<Image>(sideObj);
            sideImg.color = GOLD_DARK;
            sideImg.raycastTarget = false;

            // Inner glow
            var glow = FindOrCreate(card.transform, "InnerGlow");
            var glowRT = GetOrAdd<RectTransform>(glow);
            glowRT.anchorMin = Vector2.zero;
            glowRT.anchorMax = Vector2.one;
            glowRT.offsetMin = new Vector2(3, 3);
            glowRT.offsetMax = new Vector2(-3, -3);
            var glowImg = GetOrAdd<Image>(glow);
            glowImg.color = new Color(1, 0.9f, 0.6f, 0.3f);
            glowImg.raycastTarget = false;

            // Icon (left, bigger than JUGAR)
            var icon = FindOrCreate(card.transform, "Icon");
            var iconRT = GetOrAdd<RectTransform>(icon);
            iconRT.anchorMin = new Vector2(0, 0.5f);
            iconRT.anchorMax = new Vector2(0, 0.5f);
            iconRT.pivot = new Vector2(0, 0.5f);
            iconRT.anchoredPosition = new Vector2(25, 0);
            iconRT.sizeDelta = new Vector2(240, 240);
            var iconImg = GetOrAdd<Image>(icon);
            iconImg.color = Color.white;
            iconImg.preserveAspect = true;

            // Text Container
            var textC = FindOrCreate(card.transform, "TextContainer");
            var tcRT = GetOrAdd<RectTransform>(textC);
            tcRT.anchorMin = new Vector2(0, 0);
            tcRT.anchorMax = new Vector2(1, 1);
            tcRT.offsetMin = new Vector2(280, 20);
            tcRT.offsetMax = new Vector2(-60, -20);

            // Title
            var title = FindOrCreate(textC.transform, "Title");
            var tRT = GetOrAdd<RectTransform>(title);
            tRT.anchorMin = new Vector2(0, 0.50f);
            tRT.anchorMax = new Vector2(1, 1);
            tRT.offsetMin = Vector2.zero;
            tRT.offsetMax = Vector2.zero;
            var tTMP = GetOrAdd<TextMeshProUGUI>(title);
            tTMP.text = "CASH BATTLE";
            tTMP.fontSize = FontSizes.CardTitle;
            tTMP.color = TEXT_DARK;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.Left;

            // Subtitle
            var sub = FindOrCreate(textC.transform, "Subtitle");
            var sRT = GetOrAdd<RectTransform>(sub);
            sRT.anchorMin = new Vector2(0, 0.18f);
            sRT.anchorMax = new Vector2(1, 0.45f);
            sRT.offsetMin = Vector2.zero;
            sRT.offsetMax = Vector2.zero;
            var sTMP = GetOrAdd<TextMeshProUGUI>(sub);
            sTMP.text = "Compete for real money";
            sTMP.fontSize = FontSizes.SectionHeader;
            sTMP.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            sTMP.alignment = TextAlignmentOptions.Left;

            // 18+ badge
            var age = FindOrCreate(textC.transform, "AgeBadge");
            var ageRT = GetOrAdd<RectTransform>(age);
            ageRT.anchorMin = new Vector2(0, 0);
            ageRT.anchorMax = new Vector2(0.15f, 0.18f);
            ageRT.offsetMin = Vector2.zero;
            ageRT.offsetMax = Vector2.zero;
            var ageTMP = GetOrAdd<TextMeshProUGUI>(age);
            ageTMP.text = "18+";
            ageTMP.fontSize = FontSizes.Button;
            ageTMP.color = new Color(0.3f, 0.2f, 0f, 0.7f);
            ageTMP.fontStyle = FontStyles.Bold;
            ageTMP.alignment = TextAlignmentOptions.Left;

            // Arrow
            var arrow = FindOrCreate(card.transform, "Arrow");
            var aRT = GetOrAdd<RectTransform>(arrow);
            aRT.anchorMin = new Vector2(1, 0.5f);
            aRT.anchorMax = new Vector2(1, 0.5f);
            aRT.pivot = new Vector2(1, 0.5f);
            aRT.anchoredPosition = new Vector2(-20, 0);
            aRT.sizeDelta = new Vector2(90, 90);
            var aTMP = GetOrAdd<TextMeshProUGUI>(arrow);
            aTMP.text = "\u203A";
            aTMP.fontSize = FontSizes.AppBranding;
            aTMP.color = TEXT_DARK;
            aTMP.fontStyle = FontStyles.Bold;
            aTMP.alignment = TextAlignmentOptions.Center;

            Debug.Log("[MainMenuUI] Cash Battle Card DORADO creado");
        }

        #endregion

        #region Extra Row

        private static void CreateExtraRow()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var panel = FindOrCreate(canvas.transform, "ExtraRow");
            var rt = GetOrAdd<RectTransform>(panel);
            SetAnchorsWithPad(rt, EXTRA_BOT, EXTRA_TOP);

            var hlg = GetOrAdd<HorizontalLayoutGroup>(panel);
            hlg.spacing = 12;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            for (int i = panel.transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(panel.transform.GetChild(i).gameObject);

            CreateExtraCard(panel.transform, "AchievementsCard", "Achievements", ORANGE_ACCENT);
            CreateExtraCard(panel.transform, "ShopCard", "Shop", GOLD);
            CreateExtraCard(panel.transform, "PremiumCard", "Premium", GOLD);

            Debug.Log("[MainMenuUI] Extra Row creado");
        }

        private static void CreateExtraCard(Transform parent, string name, string label, Color accent)
        {
            var card = new GameObject(name);
            card.transform.SetParent(parent, false);

            var bg = card.AddComponent<Image>();
            bg.color = CARD_BG;
            card.AddComponent<Button>().targetGraphic = bg;

            var outline = card.AddComponent<Outline>();
            outline.effectColor = new Color(accent.r, accent.g, accent.b, 0.5f);
            outline.effectDistance = new Vector2(1.5f, 1.5f);

            var vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6;
            vlg.padding = new RectOffset(10, 10, 15, 10);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var icon = new GameObject("Icon");
            icon.transform.SetParent(card.transform, false);
            icon.AddComponent<RectTransform>();
            icon.AddComponent<LayoutElement>().preferredHeight = 250;
            var iconImg = icon.AddComponent<Image>();
            iconImg.color = Color.white;
            iconImg.preserveAspect = true;

            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(card.transform, false);
            labelGO.AddComponent<RectTransform>();
            labelGO.AddComponent<LayoutElement>().preferredHeight = 72;
            var lTMP = labelGO.AddComponent<TextMeshProUGUI>();
            lTMP.text = label;
            lTMP.fontSize = FontSizes.SectionHeader;
            lTMP.fontSizeMin = FontSizes.AutoMinBody;
            lTMP.enableAutoSizing = true;
            lTMP.color = TEXT_WHITE;
            lTMP.fontStyle = FontStyles.Bold;
            lTMP.alignment = TextAlignmentOptions.Center;
            lTMP.overflowMode = TextOverflowModes.Ellipsis;
        }

        #endregion

        #region Panels (Premium + Notifications overlay)

        private static void CreatePanels()
        {
            CreateOverlayPanel("PremiumPanel", GOLD);
            CreateOverlayPanel("NotificationsPanel", CYAN_DARK);
        }

        private static void CreateOverlayPanel(string name, Color borderColor)
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var panel = FindOrCreate(canvas.transform, name);
            panel.SetActive(false);
            var rt = GetOrAdd<RectTransform>(panel);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var overlay = FindOrCreate(panel.transform, "Overlay");
            var ovRT = GetOrAdd<RectTransform>(overlay);
            ovRT.anchorMin = Vector2.zero;
            ovRT.anchorMax = Vector2.one;
            ovRT.offsetMin = Vector2.zero;
            ovRT.offsetMax = Vector2.zero;
            var ovImg = GetOrAdd<Image>(overlay);
            ovImg.color = new Color(0, 0, 0, 0.85f);
            GetOrAdd<Button>(overlay).targetGraphic = ovImg;

            var container = FindOrCreate(panel.transform, "Container");
            var cRT = GetOrAdd<RectTransform>(container);
            cRT.anchorMin = new Vector2(0.05f, 0.1f);
            cRT.anchorMax = new Vector2(0.95f, 0.9f);
            cRT.offsetMin = Vector2.zero;
            cRT.offsetMax = Vector2.zero;
            var cBg = GetOrAdd<Image>(container);
            cBg.color = CARD_BG;
            var cOutline = GetOrAdd<Outline>(container);
            cOutline.effectColor = borderColor;
            cOutline.effectDistance = new Vector2(2, 2);
        }

        #endregion

        #region Manager References

        private static void SetupManagerReferences()
        {
            var manager = Object.FindFirstObjectByType<DigitPark.Managers.MainMenuManager>();
            if (manager == null)
            {
                Debug.LogWarning("[MainMenuUI] MainMenuManager no encontrado en la escena");
                return;
            }

            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var so = new SerializedObject(manager);
            Transform r = canvas.transform;

            SetRef(so, "mainMenuPanel", r.gameObject);
            SetRef(so, "titleText", FindInPath<TextMeshProUGUI>(r, "Header/LogoText"));
            SetRef(so, "playButton", FindInPath<Button>(r, "PlayCard"));
            SetRef(so, "scoresButton", FindInPath<Button>(r, "QuickActionsPanel/RankingsCard"));
            SetRef(so, "cashBattleButton", FindInPath<Button>(r, "CashBattleCard"));
            SetRef(so, "settingsButton", FindInPath<Button>(r, "Header/SettingsButton"));
            SetRef(so, "userButton", FindInPath<Button>(r, "ProfileCard"));
            SetRef(so, "userText", FindInPath<TextMeshProUGUI>(r, "ProfileCard/InfoSection/Username"));
            SetRef(so, "searchButton", FindInPath<Button>(r, "QuickActionsPanel/SearchCard"));
            SetRef(so, "notificationsButton", FindInPath<Button>(r, "Header/NotificationsButton"));
            SetRef(so, "notificationIconImage", FindInPath<Image>(r, "Header/NotificationsButton/Icon"));
            Transform badgeContainer = r.Find("Header/NotificationsButton/Badge");
            if (badgeContainer != null)
                SetRef(so, "notificationBadge", badgeContainer.gameObject);
            SetRef(so, "notificationBadgeText", FindInPath<TextMeshProUGUI>(r, "Header/NotificationsButton/Badge/BadgeText"));
            SetRef(so, "premiumButton", FindInPath<Button>(r, "ExtraRow/PremiumCard"));

            Transform premCard = r.Find("ExtraRow/PremiumCard");
            if (premCard != null)
                SetRef(so, "premiumBadge", premCard.gameObject);

            // Monetization buttons
            SetRef(so, "shopButton", FindInPath<Button>(r, "ExtraRow/ShopCard"));
            SetRef(so, "achievementsButton", FindInPath<Button>(r, "ExtraRow/AchievementsCard"));
            SetRef(so, "dailyRewardsButton", FindInPath<Button>(r, "DailyRewardCard"));
            SetRef(so, "missionsCardButton", FindInPath<Button>(r, "QuickActionsPanel/MissionsCard"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            Debug.Log("[MainMenuUI] Referencias del manager asignadas");
        }

        private static void SetRef(SerializedObject so, string propName, Object value)
        {
            var prop = so.FindProperty(propName);
            if (prop == null) { Debug.LogWarning($"[MainMenuUI] Property '{propName}' no encontrada"); return; }
            if (value != null) { prop.objectReferenceValue = value; Debug.Log($"[MainMenuUI] Asignado: {propName}"); }
            else { Debug.LogWarning($"[MainMenuUI] No se encontró valor para: {propName}"); }
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

        #region Icon Assignment

        private static void AssignNeonIcons()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) { Debug.LogError("[MainMenuUI] No Canvas"); return; }

            int a = 0;
            a += TryAssignIcon(canvas.transform, "Header/SettingsButton/Icon", ICON_SETTINGS);
            a += TryAssignIcon(canvas.transform, "Header/NotificationsButton/Icon", ICON_NOTIFICATIONS);
            a += TryAssignIcon(canvas.transform, "ProfileCard/AvatarSection/AvatarFrame/AvatarImage", ICON_AVATAR_DEFAULT);
            // Currency Display icons in header
            a += TryAssignIcon(canvas.transform, "Header/CurrencyDisplay/GemsDisplay/Icon", ICON_GEM);
            a += TryAssignIcon(canvas.transform, "Header/CurrencyDisplay/CoinsDisplay/Icon", ICON_COIN);
            // Profile stats icons
            a += TryAssignIcon(canvas.transform, "ProfileCard/InfoSection/StatsRow/DigitGems/Icon", ICON_GEM);
            a += TryAssignIcon(canvas.transform, "ProfileCard/InfoSection/StatsRow/DigitCoins/Icon", ICON_COIN);
            a += TryAssignIcon(canvas.transform, "QuickActionsPanel/RankingsCard/Icon", ICON_RANKINGS);
            a += TryAssignIcon(canvas.transform, "QuickActionsPanel/SearchCard/Icon", ICON_SEARCH);
            a += TryAssignIcon(canvas.transform, "QuickActionsPanel/MissionsCard/Icon", ICON_MISSIONS);
            a += TryAssignIcon(canvas.transform, "PlayCard/Icon", ICON_PLAY);
            a += TryAssignIcon(canvas.transform, "CashBattleCard/Icon", ICON_CASH_BATTLE);
            a += TryAssignIcon(canvas.transform, "DailyRewardCard/GiftIcon", ICON_GIFT);
            a += TryAssignIcon(canvas.transform, "ExtraRow/AchievementsCard/Icon", ICON_ACHIEVEMENTS);
            a += TryAssignIcon(canvas.transform, "ExtraRow/ShopCard/Icon", ICON_SHOP);
            a += TryAssignIcon(canvas.transform, "ExtraRow/PremiumCard/Icon", ICON_PREMIUM);

            AssignNotificationSprites();

            Debug.Log($"[MainMenuUI] Iconos asignados: {a}/17");
            EditorUtility.SetDirty(canvas.gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
            EditorUtility.DisplayDialog("Iconos", $"Asignados: {a}/17\nVer Console para detalles.", "OK");
        }

        private static void AssignNotificationSprites()
        {
            var manager = Object.FindFirstObjectByType<DigitPark.Managers.MainMenuManager>();
            if (manager == null) return;

            var so = new SerializedObject(manager);

            Sprite normal = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_NOTIFICATIONS);
            if (normal != null)
            {
                var p = so.FindProperty("notificationIconNormal");
                if (p != null) p.objectReferenceValue = normal;
            }

            Sprite active = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_NOTIFICATIONS_ACTIVE);
            if (active != null)
            {
                var p = so.FindProperty("notificationIconActive");
                if (p != null) p.objectReferenceValue = active;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
        }

        private static int TryAssignIcon(Transform root, string path, string iconPath)
        {
            Transform target = root;
            foreach (string part in path.Split('/'))
            {
                target = target.Find(part);
                if (target == null) { Debug.LogWarning($"[MainMenuUI] No encontrado: {path}"); return 0; }
            }

            Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            if (icon == null) { Debug.LogWarning($"[MainMenuUI] Icono no encontrado: {iconPath}"); return 0; }

            Image img = target.GetComponent<Image>();
            if (img == null) return 0;

            img.sprite = icon;
            img.preserveAspect = true;
            return 1;
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

        private static void SetAnchorsWithPad(RectTransform rt, float bot, float top)
        {
            rt.anchorMin = new Vector2(0, bot);
            rt.anchorMax = new Vector2(1, top);
            rt.offsetMin = new Vector2(SIDE_PAD, 0);
            rt.offsetMax = new Vector2(-SIDE_PAD, 0);
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
