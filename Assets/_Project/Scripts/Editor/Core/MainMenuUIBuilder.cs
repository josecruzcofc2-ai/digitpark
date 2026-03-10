using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;
using DigitPark.Animations;
using DigitPark.Monetization;

namespace DigitPark.Editor
{
    /// <summary>
    /// MainMenu UI Builder v4 - Polish 2026
    /// Avatar circular con ring CYAN, sin @ en username, íconos quick access +20%,
    /// bottom safe area 4%, profile card más presencia visual.
    /// Portrait 9:16 (1080x1920), matchWidthOrHeight=0.5
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

        // Uniform 1% (0.010) gap between every section — no overlaps, consistent spacing
        // Heights: Header=100px, Profile=18.5%, Daily=7.0%, Quick=6.0%, Play=15.5%, Cash=17.5%, Extra=18.3%
        private const float HEADER_HEIGHT = 100f;
        private const float HEADER_TOP = 1.000f;
        private const float HEADER_BOT = 0.928f;  // kept for reference, header now uses sizeDelta

        private const float PROFILE_TOP = 0.918f; // gap 1.0%
        private const float PROFILE_BOT = 0.733f; // height 18.5%

        private const float DAILY_TOP = 0.726f;   // gap 1.0%
        private const float DAILY_BOT = 0.656f;   // height 7.0%

        private const float QUICK_TOP = 0.636f;   // gap 1.0%
        private const float QUICK_BOT = 0.576f;   // height 6.0%

        private const float PLAY_TOP = 0.556f;    // gap 1.0%
        private const float PLAY_BOT = 0.401f;    // height 15.5%

        private const float CASH_TOP = 0.391f;    // gap 1.0%
        private const float CASH_BOT = 0.216f;    // height 17.5%

        private const float EXTRA_TOP = 0.206f;   // gap 1.0%
        private const float EXTRA_BOT = 0.023f;   // height 18.3% | iOS home indicator safe area

        private const float SIDE_PAD = 20f;

        #endregion

        #region Icon Paths

        private const string ICONS_BASE = "Assets/_Project/Art/Icons";
        private const string ICON_SETTINGS = ICONS_BASE + "/Navigation/SettingsIcon.png";
        private const string ICON_NOTIFICATIONS = ICONS_BASE + "/Navigation/NotificationsIcon.png";
        private const string ICON_NOTIFICATIONS_ACTIVE = ICONS_BASE + "/Navigation/NotificationsIcon.png";
        private const string ICON_AVATAR_DEFAULT = ICONS_BASE + "/Social/AvatarDefault.png";
        private const string ICON_GEM = ICONS_BASE + "/Currency/icon_digitgem_single.png";
        private const string ICON_COIN = ICONS_BASE + "/Currency/icon_digitcoin_single.png";
        private const string ICON_RANKINGS = ICONS_BASE + "/UI/RankingsIcon.png";
        private const string ICON_SEARCH = ICONS_BASE + "/Navigation/SearchIcon.png";
        private const string ICON_MISSIONS = ICONS_BASE + "/Missions/MissionsIcon.png";
        private const string ICON_PLAY = ICONS_BASE + "/UI/PlayIcon.png";
        private const string ICON_CASH_BATTLE = ICONS_BASE + "/CashBattle/UI/CashBattleIcon.png";
        private const string ICON_DAILY_REWARD = ICONS_BASE + "/DailyRewards/DailyRewardIcon.png";
        private const string ICON_ACHIEVEMENTS = ICONS_BASE + "/UI/AchievementsIcon.png";
        private const string ICON_SHOP = ICONS_BASE + "/UI/ShopIcon.png";
        private const string ICON_PREMIUM = ICONS_BASE + "/UI/PremiumIcon.png";

        #endregion

        [MenuItem("DigitPark/Scenes/Build Scene/Core/MainMenu", false, 110)]
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
                scaler.matchWidthOrHeight = 0.5f;
            }

            // Full clean of canvas children (keep TransitionCanvas and EventSystem)
            CleanupOldElements(canvas.transform);

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
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0, HEADER_HEIGHT);
            GetOrAdd<Image>(header).color = HEADER_BG;

            // Settings Button (left) - left edge aligned with ProfileCard SIDE_PAD (20px)
            // center = 20 + 50 = 70px from left edge
            CreateIconButton(header.transform, "SettingsButton",
                new Vector2(0, 0.5f), new Vector2(70, 0), new Vector2(100, 100));

            // Logo DIGIT PARK - starts after Settings button right edge (120px / 1080 = 0.111)
            var logo = FindOrCreate(header.transform, "LogoText");
            var logoRT = GetOrAdd<RectTransform>(logo);
            logoRT.anchorMin = new Vector2(0.115f, 0);
            logoRT.anchorMax = new Vector2(0.35f, 1);
            logoRT.offsetMin = Vector2.zero;
            logoRT.offsetMax = Vector2.zero;

            var logoTMP = GetOrAdd<TextMeshProUGUI>(logo);
            logoTMP.text = "DIGIT PARK";
            logoTMP.fontSize = FontSizes.H4;
            logoTMP.enableAutoSizing = true;
            logoTMP.fontSizeMin = FontSizes.Caption;
            logoTMP.fontSizeMax = FontSizes.H3;
            logoTMP.color = CYAN_NEON;
            logoTMP.fontStyle = FontStyles.Bold;
            logoTMP.alignment = TextAlignmentOptions.MidlineLeft;
            logoTMP.enableWordWrapping = false;
            logoTMP.overflowMode = TextOverflowModes.Ellipsis;
            logoTMP.enableVertexGradient = true;
            logoTMP.colorGradient = new VertexGradient(CYAN_NEON, CYAN_NEON, CYAN_GLOW, CYAN_GLOW);

            // Currency Display (between logo and notifications)
            CreateMainMenuCurrencyDisplay(header.transform);

            // Notifications Button (far right) - right edge aligned with ProfileCard SIDE_PAD (20px)
            // center = 1080 - 20 - 50 = 1010px from left → -70px from right anchor
            var notifBtn = CreateIconButton(header.transform, "NotificationsButton",
                new Vector2(1, 0.5f), new Vector2(-70, 0), new Vector2(100, 100));

            // Notification Badge
            var badge = FindOrCreate(notifBtn.transform, "Badge");
            var badgeRT = GetOrAdd<RectTransform>(badge);
            badgeRT.anchorMin = new Vector2(1, 1);
            badgeRT.anchorMax = new Vector2(1, 1);
            badgeRT.pivot = new Vector2(0.5f, 0.5f);
            badgeRT.anchoredPosition = new Vector2(-12, -12);
            badgeRT.sizeDelta = new Vector2(36, 36);
            var badgeImg = GetOrAdd<Image>(badge);
            badgeImg.color = new Color(1, 0.2f, 0.2f, 1);
            // Use a circle sprite for round badge appearance
            badgeImg.type = Image.Type.Sliced;
            badgeImg.pixelsPerUnitMultiplier = 2f;

            // BadgeAnimator: pop-in entrance + continuous pulse
            var badgeAnim = GetOrAdd<BadgeAnimator>(badge);
            var badgeSO = new SerializedObject(badgeAnim);
            var autoPulseProp = badgeSO.FindProperty("autoPulse");
            if (autoPulseProp != null) { autoPulseProp.boolValue = true; badgeSO.ApplyModifiedProperties(); }

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
            btTMP.enableAutoSizing = true;
            btTMP.fontSizeMin = FontSizes.AutoMinBody;
            btTMP.fontSizeMax = FontSizes.Body;
            btTMP.overflowMode = TextOverflowModes.Ellipsis;

            Debug.Log("[MainMenuUI] Header creado");
        }

        /// <summary>
        /// Creates the currency display pills in the MainMenu header.
        /// Two pills: GemsDisplay (gem icon + amount) and CoinsDisplay (coin icon + amount).
        /// Tapping each pill navigates to the Shop (corresponding section).
        /// </summary>
        private static void CreateMainMenuCurrencyDisplay(Transform headerTransform)
        {
            var container = CurrencyHeaderBarHelper.CreateCurrencyPills(headerTransform, "CurrencyDisplay");
            var cRT = container.GetComponent<RectTransform>();
            cRT.anchorMin = new Vector2(0.35f, 0.5f);
            cRT.anchorMax = new Vector2(0.87f, 0.5f);
            cRT.pivot = new Vector2(0.5f, 0.5f);
            cRT.sizeDelta = new Vector2(0, 65);
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
            bg.color = CARD_BG_LIGHT;
            var outline = GetOrAdd<Outline>(card);
            outline.effectColor = new Color(CYAN_NEON.r, CYAN_NEON.g, CYAN_NEON.b, 0.45f);
            outline.effectDistance = new Vector2(3, 3);

            // Whole card is userButton
            var btn = GetOrAdd<Button>(card);
            btn.targetGraphic = bg;

            // Remove old children if rebuilding
            for (int i = card.transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(card.transform.GetChild(i).gameObject);

            // === Centered layout ===

            // Avatar Frame container (centered, circular matchmaking style)
            Sprite circleSprite = GenerateCircleSprite();

            var frame = new GameObject("AvatarFrame");
            frame.transform.SetParent(card.transform, false);
            var frameRT = frame.AddComponent<RectTransform>();
            frameRT.anchorMin = new Vector2(0.5f, 0.63f);
            frameRT.anchorMax = new Vector2(0.5f, 0.63f);
            frameRT.pivot = new Vector2(0.5f, 0.5f);
            frameRT.sizeDelta = new Vector2(220, 220);

            // Circular glow ring (outer, slightly larger)
            var glowRing = new GameObject("GlowRing");
            glowRing.transform.SetParent(frame.transform, false);
            var glowRT = glowRing.AddComponent<RectTransform>();
            glowRT.anchorMin = Vector2.zero;
            glowRT.anchorMax = Vector2.one;
            glowRT.offsetMin = new Vector2(-8, -8);
            glowRT.offsetMax = new Vector2(8, 8);
            var glowImg = glowRing.AddComponent<Image>();
            glowImg.sprite = circleSprite;
            glowImg.color = new Color(CYAN_NEON.r, CYAN_NEON.g, CYAN_NEON.b, 0.25f);

            // Circular border ring (solid cyan frame)
            var borderRing = new GameObject("BorderRing");
            borderRing.transform.SetParent(frame.transform, false);
            var borderRT = borderRing.AddComponent<RectTransform>();
            borderRT.anchorMin = Vector2.zero;
            borderRT.anchorMax = Vector2.one;
            borderRT.offsetMin = Vector2.zero;
            borderRT.offsetMax = Vector2.zero;
            var borderImg = borderRing.AddComponent<Image>();
            borderImg.sprite = circleSprite;
            borderImg.color = CYAN_NEON;

            // Circular mask container (clips avatar to circle)
            var avatarMask = new GameObject("AvatarMask");
            avatarMask.transform.SetParent(frame.transform, false);
            var maskRT = avatarMask.AddComponent<RectTransform>();
            maskRT.anchorMin = new Vector2(0.06f, 0.06f);
            maskRT.anchorMax = new Vector2(0.94f, 0.94f);
            maskRT.offsetMin = Vector2.zero;
            maskRT.offsetMax = Vector2.zero;
            var maskImg = avatarMask.AddComponent<Image>();
            maskImg.sprite = circleSprite;
            maskImg.color = CARD_BG_LIGHT;
            avatarMask.AddComponent<Mask>().showMaskGraphic = true;

            // Avatar Image (inside mask, fills circle, clipped circular)
            var avImg = new GameObject("AvatarImage");
            avImg.transform.SetParent(avatarMask.transform, false);
            var avImgRT = avImg.AddComponent<RectTransform>();
            avImgRT.anchorMin = Vector2.zero;
            avImgRT.anchorMax = Vector2.one;
            avImgRT.offsetMin = Vector2.zero;
            avImgRT.offsetMax = Vector2.zero;
            var avImgComp = avImg.AddComponent<Image>();
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

            // Username (centered below avatar)
            var user = new GameObject("Username");
            user.transform.SetParent(card.transform, false);
            var userRT = user.AddComponent<RectTransform>();
            userRT.anchorMin = new Vector2(0.05f, 0.20f);
            userRT.anchorMax = new Vector2(0.95f, 0.35f);
            userRT.offsetMin = Vector2.zero;
            userRT.offsetMax = Vector2.zero;
            var userTMP = user.AddComponent<TextMeshProUGUI>();
            userTMP.text = "Username";
            userTMP.fontSize = FontSizes.H3;
            userTMP.color = TEXT_WHITE;
            userTMP.fontStyle = FontStyles.Bold;
            userTMP.alignment = TextAlignmentOptions.Center;
            userTMP.enableAutoSizing = true;
            userTMP.fontSizeMin = FontSizes.AutoMinBody;
            userTMP.fontSizeMax = FontSizes.H3;

            // Level Badge (centered below username)
            var lvlBadge = new GameObject("LevelBadge");
            lvlBadge.transform.SetParent(card.transform, false);
            var lvlRT = lvlBadge.AddComponent<RectTransform>();
            lvlRT.anchorMin = new Vector2(0.5f, 0.13f);
            lvlRT.anchorMax = new Vector2(0.5f, 0.13f);
            lvlRT.sizeDelta = new Vector2(140, 48);
            lvlBadge.AddComponent<Image>().color = CYAN_NEON;

            var lvlText = new GameObject("LevelText");
            lvlText.transform.SetParent(lvlBadge.transform, false);
            var ltRT = lvlText.AddComponent<RectTransform>();
            ltRT.anchorMin = Vector2.zero;
            ltRT.anchorMax = Vector2.one;
            ltRT.offsetMin = Vector2.zero;
            ltRT.offsetMax = Vector2.zero;
            var ltTMP = lvlText.AddComponent<TextMeshProUGUI>();
            ltTMP.text = "Lv. 12";
            ltTMP.fontSize = FontSizes.Caption;
            ltTMP.color = TEXT_DARK;
            ltTMP.fontStyle = FontStyles.Bold;
            ltTMP.alignment = TextAlignmentOptions.Center;
            ltTMP.enableAutoSizing = true;
            ltTMP.fontSizeMin = FontSizes.AutoMinBody;
            ltTMP.fontSizeMax = FontSizes.Caption;
            ltTMP.overflowMode = TextOverflowModes.Ellipsis;

            Debug.Log("[MainMenuUI] Profile Card creado (centered layout)");
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

            // Daily Reward Icon
            var icon = FindOrCreate(card.transform, "DailyRewardIcon");
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

            var title = FindOrCreate(textC.transform, "DailyRewardCardTitle");
            var tRT = GetOrAdd<RectTransform>(title);
            tRT.anchorMin = new Vector2(0, 0.55f);
            tRT.anchorMax = new Vector2(1, 1);
            tRT.offsetMin = Vector2.zero;
            tRT.offsetMax = Vector2.zero;
            var tTMP = GetOrAdd<TextMeshProUGUI>(title);
            tTMP.text = "DAILY REWARD";
            tTMP.fontSize = FontSizes.Body;
            tTMP.enableAutoSizing = true;
            tTMP.fontSizeMin = FontSizes.Caption;
            tTMP.fontSizeMax = FontSizes.Body;
            tTMP.color = GOLD;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.Left;
            tTMP.enableWordWrapping = false;

            var sub = FindOrCreate(textC.transform, "DailyRewardSubtitle");
            var sRT = GetOrAdd<RectTransform>(sub);
            sRT.anchorMin = new Vector2(0, 0);
            sRT.anchorMax = new Vector2(1, 0.5f);
            sRT.offsetMin = Vector2.zero;
            sRT.offsetMax = Vector2.zero;
            var sTMP = GetOrAdd<TextMeshProUGUI>(sub);
            sTMP.text = "Day 3 of 7 - Claim your reward!";
            sTMP.fontSize = FontSizes.BodySmall;
            sTMP.enableAutoSizing = true;
            sTMP.fontSizeMin = FontSizes.Caption;
            sTMP.fontSizeMax = FontSizes.BodySmall;
            sTMP.color = TEXT_SECONDARY;
            sTMP.fontStyle = FontStyles.Bold;
            sTMP.alignment = TextAlignmentOptions.Left;
            sTMP.enableWordWrapping = true;

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

            var cText = FindOrCreate(claimBtn.transform, "ClaimButtonText");
            var ctRT = GetOrAdd<RectTransform>(cText);
            ctRT.anchorMin = Vector2.zero;
            ctRT.anchorMax = Vector2.one;
            ctRT.offsetMin = Vector2.zero;
            ctRT.offsetMax = Vector2.zero;
            var ctTMP = GetOrAdd<TextMeshProUGUI>(cText);
            ctTMP.text = "Claim";
            ctTMP.fontSize = FontSizes.Body;
            ctTMP.enableAutoSizing = true;
            ctTMP.fontSizeMin = FontSizes.Caption;
            ctTMP.fontSizeMax = FontSizes.Body;
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
            icon.AddComponent<LayoutElement>().preferredHeight = 86; // +20% tap target
            var iconImg = icon.AddComponent<Image>();
            iconImg.color = Color.white;
            iconImg.preserveAspect = true;

            var labelGO = new GameObject(name.Replace("Card", "") + "Label");
            labelGO.transform.SetParent(card.transform, false);
            labelGO.AddComponent<RectTransform>();
            labelGO.AddComponent<LayoutElement>().preferredHeight = 44;
            var lTMP = labelGO.AddComponent<TextMeshProUGUI>();
            lTMP.text = label;
            lTMP.fontSize = FontSizes.Body;
            lTMP.color = TEXT_WHITE;
            lTMP.fontStyle = FontStyles.Bold;
            lTMP.alignment = TextAlignmentOptions.Center;
            lTMP.enableAutoSizing = true;
            lTMP.fontSizeMin = FontSizes.AutoMinBody;
            lTMP.fontSizeMax = FontSizes.Body;
            lTMP.overflowMode = TextOverflowModes.Ellipsis;
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

            var title = FindOrCreate(textC.transform, "PlayCardTitle");
            var tRT = GetOrAdd<RectTransform>(title);
            tRT.anchorMin = new Vector2(0, 0.5f);
            tRT.anchorMax = new Vector2(1, 1);
            tRT.offsetMin = Vector2.zero;
            tRT.offsetMax = Vector2.zero;
            var tTMP = GetOrAdd<TextMeshProUGUI>(title);
            tTMP.text = "PLAY";
            tTMP.fontSize = FontSizes.H3;
            tTMP.color = TEXT_DARK;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.Left;
            tTMP.enableAutoSizing = true;
            tTMP.fontSizeMin = FontSizes.AutoMinTitle;
            tTMP.fontSizeMax = FontSizes.H3;
            tTMP.overflowMode = TextOverflowModes.Ellipsis;

            var sub = FindOrCreate(textC.transform, "PlayCardSubtitle");
            var sRT = GetOrAdd<RectTransform>(sub);
            sRT.anchorMin = new Vector2(0, 0);
            sRT.anchorMax = new Vector2(1, 0.45f);
            sRT.offsetMin = Vector2.zero;
            sRT.offsetMax = Vector2.zero;
            var sTMP = GetOrAdd<TextMeshProUGUI>(sub);
            sTMP.text = "Choose a game and compete";
            sTMP.fontSize = FontSizes.H4;
            sTMP.enableAutoSizing = true;
            sTMP.fontSizeMin = FontSizes.Caption;
            sTMP.fontSizeMax = FontSizes.H4;
            sTMP.fontStyle = FontStyles.Bold;
            sTMP.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            sTMP.alignment = TextAlignmentOptions.Left;
            sTMP.enableWordWrapping = true;

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
            aTMP.fontSize = FontSizes.Branding;
            aTMP.color = TEXT_DARK;
            aTMP.fontStyle = FontStyles.Bold;
            aTMP.alignment = TextAlignmentOptions.Center;
            aTMP.enableAutoSizing = true;
            aTMP.fontSizeMin = FontSizes.AutoMinBody;
            aTMP.fontSizeMax = FontSizes.Branding;
            aTMP.overflowMode = TextOverflowModes.Ellipsis;

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
            var title = FindOrCreate(textC.transform, "CashBattleCardTitle");
            var tRT = GetOrAdd<RectTransform>(title);
            tRT.anchorMin = new Vector2(0, 0.50f);
            tRT.anchorMax = new Vector2(1, 1);
            tRT.offsetMin = Vector2.zero;
            tRT.offsetMax = Vector2.zero;
            var tTMP = GetOrAdd<TextMeshProUGUI>(title);
            tTMP.text = "CASH BATTLE";
            tTMP.fontSize = FontSizes.H3;
            tTMP.color = TEXT_DARK;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.Left;
            tTMP.enableAutoSizing = true;
            tTMP.fontSizeMin = FontSizes.AutoMinTitle;
            tTMP.fontSizeMax = FontSizes.H3;
            tTMP.overflowMode = TextOverflowModes.Ellipsis;

            // Subtitle
            var sub = FindOrCreate(textC.transform, "CashBattleCardSubtitle");
            var sRT = GetOrAdd<RectTransform>(sub);
            sRT.anchorMin = new Vector2(0, 0.18f);
            sRT.anchorMax = new Vector2(1, 0.45f);
            sRT.offsetMin = Vector2.zero;
            sRT.offsetMax = Vector2.zero;
            var sTMP = GetOrAdd<TextMeshProUGUI>(sub);
            sTMP.text = "Compete for real money";
            sTMP.fontSize = FontSizes.H4;
            sTMP.enableAutoSizing = true;
            sTMP.fontSizeMin = FontSizes.Caption;
            sTMP.fontSizeMax = FontSizes.H4;
            sTMP.fontStyle = FontStyles.Bold;
            sTMP.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            sTMP.alignment = TextAlignmentOptions.Left;
            sTMP.enableWordWrapping = true;

            // 18+ badge
            var age = FindOrCreate(textC.transform, "AgeBadge");
            var ageRT = GetOrAdd<RectTransform>(age);
            ageRT.anchorMin = new Vector2(0, 0);
            ageRT.anchorMax = new Vector2(0.15f, 0.18f);
            ageRT.offsetMin = Vector2.zero;
            ageRT.offsetMax = Vector2.zero;
            var ageTMP = GetOrAdd<TextMeshProUGUI>(age);
            ageTMP.text = "18+";
            ageTMP.fontSize = FontSizes.Body;
            ageTMP.color = new Color(0.3f, 0.2f, 0f, 0.7f);
            ageTMP.fontStyle = FontStyles.Bold;
            ageTMP.alignment = TextAlignmentOptions.Left;
            ageTMP.enableAutoSizing = true;
            ageTMP.fontSizeMin = FontSizes.AutoMinBody;
            ageTMP.fontSizeMax = FontSizes.Body;
            ageTMP.overflowMode = TextOverflowModes.Ellipsis;

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
            aTMP.fontSize = FontSizes.Branding;
            aTMP.color = TEXT_DARK;
            aTMP.fontStyle = FontStyles.Bold;
            aTMP.alignment = TextAlignmentOptions.Center;
            aTMP.enableAutoSizing = true;
            aTMP.fontSizeMin = FontSizes.AutoMinBody;
            aTMP.fontSizeMax = FontSizes.Branding;
            aTMP.overflowMode = TextOverflowModes.Ellipsis;

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
            icon.AddComponent<LayoutElement>().preferredHeight = 180;
            var iconImg = icon.AddComponent<Image>();
            iconImg.color = Color.white;
            iconImg.preserveAspect = true;

            var labelGO = new GameObject(name.Replace("Card", "") + "Label");
            labelGO.transform.SetParent(card.transform, false);
            labelGO.AddComponent<RectTransform>();
            labelGO.AddComponent<LayoutElement>().preferredHeight = 60;
            var lTMP = labelGO.AddComponent<TextMeshProUGUI>();
            lTMP.text = label;
            lTMP.fontSize = FontSizes.H4;
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

            // Blocker overlay
            var blocker = FindOrCreate(panel.transform, "BlockerPanel");
            blocker.transform.SetAsFirstSibling();
            var blockerRT = GetOrAdd<RectTransform>(blocker);
            blockerRT.anchorMin = Vector2.zero;
            blockerRT.anchorMax = Vector2.one;
            blockerRT.offsetMin = Vector2.zero;
            blockerRT.offsetMax = Vector2.zero;
            var blockerImg = GetOrAdd<Image>(blocker);
            blockerImg.color = new Color(0f, 0f, 0f, 0.7f);
            blockerImg.raycastTarget = true;

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
            SetRef(so, "userText", FindInPath<TextMeshProUGUI>(r, "ProfileCard/Username"));
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
            a += TryAssignIcon(canvas.transform, "ProfileCard/AvatarFrame/AvatarMask/AvatarImage", ICON_AVATAR_DEFAULT);
            // Currency Display icons in header
            a += TryAssignIcon(canvas.transform, "Header/CurrencyDisplay/GemsDisplay/Icon", ICON_GEM);
            a += TryAssignIcon(canvas.transform, "Header/CurrencyDisplay/CoinsDisplay/Icon", ICON_COIN);
            a += TryAssignIcon(canvas.transform, "QuickActionsPanel/RankingsCard/Icon", ICON_RANKINGS);
            a += TryAssignIcon(canvas.transform, "QuickActionsPanel/SearchCard/Icon", ICON_SEARCH);
            a += TryAssignIcon(canvas.transform, "QuickActionsPanel/MissionsCard/Icon", ICON_MISSIONS);
            a += TryAssignIcon(canvas.transform, "PlayCard/Icon", ICON_PLAY);
            a += TryAssignIcon(canvas.transform, "CashBattleCard/Icon", ICON_CASH_BATTLE);
            a += TryAssignIcon(canvas.transform, "DailyRewardCard/DailyRewardIcon", ICON_DAILY_REWARD);
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

        private static Sprite GenerateCircleSprite()
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/UI/CircleSprite.png");
            if (s != null) return s;
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size / 2f;
            float radius = center - 1f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dist = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center));
                    if (dist <= radius) tex.SetPixel(x, y, Color.white);
                    else if (dist <= radius + 1f) tex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(radius + 1f - dist)));
                    else tex.SetPixel(x, y, Color.clear);
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        #endregion
    }
}
