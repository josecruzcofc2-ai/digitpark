using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// MainMenu UI Builder - REDISEÑO COMPLETO 2026
    /// Diseño innovador con cards, estilo Neon profesional
    /// Resolución: Portrait 9:16 (1080x1920)
    /// </summary>
    public class MainMenuUIBuilder : EditorWindow
    {
        #region Colors - Neon Theme

        // Primary Colors
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_GLOW = new Color(0f, 0.85f, 1f, 0.8f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.5f, 1f);

        // Gold/Money Colors
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GOLD_DARK = new Color(0.8f, 0.6f, 0.1f, 1f);
        private static readonly Color GOLD_GLOW = new Color(1f, 0.75f, 0f, 0.7f);

        // Backgrounds
        private static readonly Color DARK_BG = new Color(0.02f, 0.04f, 0.08f, 1f);
        private static readonly Color CARD_BG = new Color(0.06f, 0.08f, 0.12f, 1f);
        private static readonly Color CARD_BG_LIGHT = new Color(0.08f, 0.1f, 0.14f, 1f);
        private static readonly Color HEADER_BG = new Color(0.04f, 0.06f, 0.1f, 0.98f);

        // Text
        private static readonly Color TEXT_WHITE = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.6f, 0.65f, 1f);
        private static readonly Color TEXT_DARK = new Color(0.1f, 0.1f, 0.1f, 1f);

        // Accents
        private static readonly Color GREEN_SUCCESS = new Color(0.2f, 0.9f, 0.4f, 1f);
        private static readonly Color PURPLE_ACCENT = new Color(0.6f, 0.3f, 1f, 1f);

        #endregion

        #region Layout Constants

        private const float HEADER_HEIGHT = 100f;
        private const float PLAYER_CARD_HEIGHT = 180f;
        private const float QUICK_ACTION_HEIGHT = 90f;
        private const float MAIN_CARD_HEIGHT = 140f;
        private const float DAILY_REWARDS_HEIGHT = 130f;
        private const float SECTION_SPACING = 20f;
        private const float CARD_BORDER_WIDTH = 2f;

        #endregion

        #region Icon Paths - Neon Icons

        private const string ICONS_BASE = "Assets/_Project/Art/Icons";

        // Header Icons
        private const string ICON_SETTINGS = ICONS_BASE + "/Navigation/Actions/SettingsIconNeon.png";
        private const string ICON_NOTIFICATIONS = ICONS_BASE + "/Navigation/Actions/NotificationsNeon.png";
        private const string ICON_NOTIFICATIONS_ACTIVE = ICONS_BASE + "/Navigation/Actions/NotificationsActiveNeon.png";
        private const string ICON_PROFILE = ICONS_BASE + "/Social/Profile/ProfileIconNeon.png";

        // Player Card Icons
        private const string ICON_AVATAR_DEFAULT = ICONS_BASE + "/Social/Profile/AvatarDefaultNeon.png";
        private const string ICON_TROPHY = ICONS_BASE + "/Tournaments/TrophyIconNeon.png";
        private const string ICON_GEM = ICONS_BASE + "/Currency/GemIconNeon.png";
        private const string ICON_COIN = ICONS_BASE + "/Currency/CoinIconNeon.png";

        // Quick Actions Icons
        private const string ICON_RANKINGS = ICONS_BASE + "/Tournaments/RankingsIconNeon.png";
        private const string ICON_SEARCH = ICONS_BASE + "/Navigation/Buttons/SearchIconNeon.png";
        private const string ICON_MISSIONS = ICONS_BASE + "/Missions/MissionsIconNeon.png";

        // Main Cards Icons
        private const string ICON_PLAY = ICONS_BASE + "/UI/PlayIconNeon.png";
        private const string ICON_CASH_BATTLE = ICONS_BASE + "/CashBattle/UI/CashBattleIconNeon.png";

        // Daily Rewards Icon
        private const string ICON_GIFT = ICONS_BASE + "/DailyRewards/GiftIconNeon.png";

        #endregion

        [MenuItem("DigitPark/UI Builders/Core/MainMenu", false, 150)]
        public static void ShowWindow()
        {
            GetWindow<MainMenuUIBuilder>("MainMenu Builder v2");
        }

        private void OnGUI()
        {
            GUILayout.Label("MainMenu UI Builder", EditorStyles.boldLabel);
            GUILayout.Label("REDISEÑO COMPLETO 2026 - Estilo Neon", EditorStyles.miniLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Nueva estructura:\n\n" +
                "1. Header (Settings, Logo, Notifications, Profile)\n" +
                "2. Player Card (Avatar, Username, Level, Stats)\n" +
                "3. Quick Actions (Rankings, Buscar, Misiones)\n" +
                "4. Main Cards (JUGAR + CASH BATTLE)\n" +
                "5. Daily Rewards Banner\n" +
                "6. Panels (Premium, Notifications)",
                MessageType.Info);

            GUILayout.Space(15);

            GUI.backgroundColor = CYAN_NEON;
            if (GUILayout.Button("RECONSTRUIR MAINMENU COMPLETO", GUILayout.Height(50)))
            {
                RebuildMainMenu();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(15);
            GUILayout.Label("Componentes Individuales:", EditorStyles.boldLabel);

            if (GUILayout.Button("1. Header", GUILayout.Height(28)))
                CreateHeader();
            if (GUILayout.Button("2. Player Card", GUILayout.Height(28)))
                CreatePlayerCard();
            if (GUILayout.Button("3. Quick Actions", GUILayout.Height(28)))
                CreateQuickActions();
            if (GUILayout.Button("4. Main Cards (Jugar + Cash)", GUILayout.Height(28)))
                CreateMainCards();
            if (GUILayout.Button("5. Daily Rewards", GUILayout.Height(28)))
                CreateDailyRewards();
            if (GUILayout.Button("6. Panels (Premium + Notif)", GUILayout.Height(28)))
                CreatePanels();

            GUILayout.Space(15);

            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
            if (GUILayout.Button("ASIGNAR ICONOS NEON", GUILayout.Height(40)))
            {
                AssignNeonIcons();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "Los iconos deben estar en:\n" +
                "Assets/_Project/Art/Icons/[Carpeta]/[NombreIconNeon].png",
                MessageType.Info);
        }

        #region Main Rebuild

        private static void RebuildMainMenu()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[MainMenuUI] No se encontró Canvas en la escena");
                return;
            }

            // Limpiar elementos viejos
            CleanOldElements(canvas.transform);

            // Crear nueva estructura
            CreateBackground(canvas.transform);
            CreateHeader();
            CreatePlayerCard();
            CreateQuickActions();
            CreateMainCards();
            CreateDailyRewards();
            CreatePanels();

            Debug.Log("[MainMenuUI] MainMenu REDISEÑADO exitosamente!");
            EditorUtility.SetDirty(canvas.gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        }

        private static void CleanOldElements(Transform canvasTransform)
        {
            string[] oldElements = {
                "Background", "Header", "TitleSection", "UserSection",
                "MainMenuPanel", "PremiumBanner", "PremiumPanel", "NotificationsPanel",
                "PlayerCard", "QuickActionsPanel", "MainCardsPanel", "DailyRewardsCard"
            };

            foreach (string name in oldElements)
            {
                Transform element = canvasTransform.Find(name);
                if (element != null)
                {
                    DestroyImmediate(element.gameObject);
                }
            }
        }

        #endregion

        #region Background

        private static void CreateBackground(Transform parent)
        {
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(parent, false);
            bg.transform.SetAsFirstSibling();

            RectTransform rt = bg.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image img = bg.AddComponent<Image>();
            img.color = DARK_BG;
        }

        #endregion

        #region Header

        private static void CreateHeader()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            GameObject header = FindOrCreate(canvas.transform, "Header");
            header.transform.SetSiblingIndex(1);

            RectTransform rt = GetOrAdd<RectTransform>(header);
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0, HEADER_HEIGHT);

            Image bg = GetOrAdd<Image>(header);
            bg.color = HEADER_BG;

            // Settings Button (izquierda)
            CreateHeaderIconButton(header.transform, "SettingsButton",
                new Vector2(0, 0.5f), new Vector2(50, 0), "settings");

            // Logo DIGIT PARK (centro)
            GameObject logo = FindOrCreate(header.transform, "LogoText");
            RectTransform logoRT = GetOrAdd<RectTransform>(logo);
            logoRT.anchorMin = new Vector2(0.5f, 0.5f);
            logoRT.anchorMax = new Vector2(0.5f, 0.5f);
            logoRT.sizeDelta = new Vector2(200, 50);

            TextMeshProUGUI logoTMP = GetOrAdd<TextMeshProUGUI>(logo);
            logoTMP.text = "DIGIT PARK";
            logoTMP.fontSize = 32;
            logoTMP.color = CYAN_NEON;
            logoTMP.fontStyle = FontStyles.Bold;
            logoTMP.alignment = TextAlignmentOptions.Center;

            // Glow effect
            Shadow logoShadow = GetOrAdd<Shadow>(logo);
            logoShadow.effectColor = new Color(0, 1, 1, 0.5f);
            logoShadow.effectDistance = new Vector2(2, -2);

            // Notifications Button (derecha)
            GameObject notifBtn = CreateHeaderIconButton(header.transform, "NotificationsButton",
                new Vector2(1, 0.5f), new Vector2(-100, 0), "bell");

            // Notification Badge
            GameObject badge = FindOrCreate(notifBtn.transform, "Badge");
            RectTransform badgeRT = GetOrAdd<RectTransform>(badge);
            badgeRT.anchorMin = new Vector2(1, 1);
            badgeRT.anchorMax = new Vector2(1, 1);
            badgeRT.pivot = new Vector2(0.5f, 0.5f);
            badgeRT.anchoredPosition = new Vector2(-8, -8);
            badgeRT.sizeDelta = new Vector2(22, 22);

            Image badgeImg = GetOrAdd<Image>(badge);
            badgeImg.color = new Color(1f, 0.2f, 0.2f, 1f);

            // Profile Button (extremo derecha)
            CreateHeaderIconButton(header.transform, "ProfileButton",
                new Vector2(1, 0.5f), new Vector2(-40, 0), "profile");

            Debug.Log("[MainMenuUI] Header creado");
        }

        private static GameObject CreateHeaderIconButton(Transform parent, string name, Vector2 anchor, Vector2 position, string iconType)
        {
            GameObject btn = FindOrCreate(parent, name);

            RectTransform rt = GetOrAdd<RectTransform>(btn);
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(55, 55);

            Image bg = GetOrAdd<Image>(btn);
            bg.color = new Color(1, 1, 1, 0.08f);

            Button button = GetOrAdd<Button>(btn);
            button.targetGraphic = bg;

            // Icon placeholder
            GameObject icon = FindOrCreate(btn.transform, "Icon");
            RectTransform iconRT = GetOrAdd<RectTransform>(icon);
            iconRT.anchorMin = new Vector2(0.15f, 0.15f);
            iconRT.anchorMax = new Vector2(0.85f, 0.85f);
            iconRT.sizeDelta = Vector2.zero;

            Image iconImg = GetOrAdd<Image>(icon);
            iconImg.color = TEXT_WHITE;
            iconImg.preserveAspect = true;

            return btn;
        }

        #endregion

        #region Player Card

        private static void CreatePlayerCard()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            GameObject card = FindOrCreate(canvas.transform, "PlayerCard");

            RectTransform rt = GetOrAdd<RectTransform>(card);
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, -HEADER_HEIGHT - 15);
            rt.sizeDelta = new Vector2(-40, PLAYER_CARD_HEIGHT);

            // Card Background
            Image cardBg = GetOrAdd<Image>(card);
            cardBg.color = CARD_BG;

            // Cyan border
            Outline outline = GetOrAdd<Outline>(card);
            outline.effectColor = CYAN_DARK;
            outline.effectDistance = new Vector2(CARD_BORDER_WIDTH, CARD_BORDER_WIDTH);

            // === Avatar Section (izquierda) ===
            GameObject avatarSection = FindOrCreate(card.transform, "AvatarSection");
            RectTransform avatarSecRT = GetOrAdd<RectTransform>(avatarSection);
            avatarSecRT.anchorMin = new Vector2(0, 0);
            avatarSecRT.anchorMax = new Vector2(0, 1);
            avatarSecRT.pivot = new Vector2(0, 0.5f);
            avatarSecRT.anchoredPosition = new Vector2(20, 0);
            avatarSecRT.sizeDelta = new Vector2(100, 0);

            // Avatar Frame (cyan ring)
            GameObject avatarFrame = FindOrCreate(avatarSection.transform, "AvatarFrame");
            RectTransform frameRT = GetOrAdd<RectTransform>(avatarFrame);
            frameRT.anchorMin = new Vector2(0.5f, 0.6f);
            frameRT.anchorMax = new Vector2(0.5f, 0.6f);
            frameRT.sizeDelta = new Vector2(85, 85);

            Image frameImg = GetOrAdd<Image>(avatarFrame);
            frameImg.color = CYAN_NEON;

            Outline frameOutline = GetOrAdd<Outline>(avatarFrame);
            frameOutline.effectColor = CYAN_GLOW;
            frameOutline.effectDistance = new Vector2(3, 3);

            // Avatar Image (interior)
            GameObject avatarImg = FindOrCreate(avatarFrame.transform, "AvatarImage");
            RectTransform avImgRT = GetOrAdd<RectTransform>(avatarImg);
            avImgRT.anchorMin = new Vector2(0.08f, 0.08f);
            avImgRT.anchorMax = new Vector2(0.92f, 0.92f);
            avImgRT.sizeDelta = Vector2.zero;

            Image avImg = GetOrAdd<Image>(avatarImg);
            avImg.color = CARD_BG_LIGHT;
            avImg.preserveAspect = true;

            // Level Badge
            GameObject levelBadge = FindOrCreate(avatarSection.transform, "LevelBadge");
            RectTransform levelRT = GetOrAdd<RectTransform>(levelBadge);
            levelRT.anchorMin = new Vector2(0.5f, 0.2f);
            levelRT.anchorMax = new Vector2(0.5f, 0.2f);
            levelRT.sizeDelta = new Vector2(60, 28);

            Image levelBg = GetOrAdd<Image>(levelBadge);
            levelBg.color = CYAN_NEON;

            GameObject levelText = FindOrCreate(levelBadge.transform, "Text");
            RectTransform lvlTxtRT = GetOrAdd<RectTransform>(levelText);
            lvlTxtRT.anchorMin = Vector2.zero;
            lvlTxtRT.anchorMax = Vector2.one;
            lvlTxtRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI lvlTMP = GetOrAdd<TextMeshProUGUI>(levelText);
            lvlTMP.text = "Nv. 12";
            lvlTMP.fontSize = 16;
            lvlTMP.color = TEXT_DARK;
            lvlTMP.fontStyle = FontStyles.Bold;
            lvlTMP.alignment = TextAlignmentOptions.Center;

            // === Info Section (centro-derecha) ===
            GameObject infoSection = FindOrCreate(card.transform, "InfoSection");
            RectTransform infoRT = GetOrAdd<RectTransform>(infoSection);
            infoRT.anchorMin = new Vector2(0, 0);
            infoRT.anchorMax = new Vector2(1, 1);
            infoRT.offsetMin = new Vector2(130, 15);
            infoRT.offsetMax = new Vector2(-15, -15);

            // Username
            GameObject username = FindOrCreate(infoSection.transform, "Username");
            RectTransform userRT = GetOrAdd<RectTransform>(username);
            userRT.anchorMin = new Vector2(0, 0.7f);
            userRT.anchorMax = new Vector2(1, 1);
            userRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI userTMP = GetOrAdd<TextMeshProUGUI>(username);
            userTMP.text = "@Username";
            userTMP.fontSize = 28;
            userTMP.color = TEXT_WHITE;
            userTMP.fontStyle = FontStyles.Bold;
            userTMP.alignment = TextAlignmentOptions.Left;

            // Stats Row
            GameObject statsRow = FindOrCreate(infoSection.transform, "StatsRow");
            RectTransform statsRT = GetOrAdd<RectTransform>(statsRow);
            statsRT.anchorMin = new Vector2(0, 0.35f);
            statsRT.anchorMax = new Vector2(1, 0.65f);
            statsRT.sizeDelta = Vector2.zero;

            HorizontalLayoutGroup statsHLG = GetOrAdd<HorizontalLayoutGroup>(statsRow);
            statsHLG.spacing = 25;
            statsHLG.childAlignment = TextAnchor.MiddleLeft;
            statsHLG.childControlWidth = false;
            statsHLG.childControlHeight = true;
            statsHLG.childForceExpandWidth = false;

            // Stat Items
            CreateStatItem(statsRow.transform, "Trophies", "1,250", GOLD);
            CreateStatItem(statsRow.transform, "Gems", "500", PURPLE_ACCENT);
            CreateStatItem(statsRow.transform, "Coins", "2,400", CYAN_NEON);

            // Win Rate / Streak
            GameObject streakRow = FindOrCreate(infoSection.transform, "StreakRow");
            RectTransform streakRT = GetOrAdd<RectTransform>(streakRow);
            streakRT.anchorMin = new Vector2(0, 0);
            streakRT.anchorMax = new Vector2(1, 0.3f);
            streakRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI streakTMP = GetOrAdd<TextMeshProUGUI>(streakRow);
            streakTMP.text = "Racha: 5 victorias";
            streakTMP.fontSize = 16;
            streakTMP.color = GREEN_SUCCESS;
            streakTMP.fontStyle = FontStyles.Normal;
            streakTMP.alignment = TextAlignmentOptions.Left;

            Debug.Log("[MainMenuUI] Player Card creado");
        }

        private static void CreateStatItem(Transform parent, string name, string value, Color color)
        {
            GameObject stat = new GameObject(name);
            stat.transform.SetParent(parent, false);

            RectTransform rt = stat.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(75, 40);

            LayoutElement le = stat.AddComponent<LayoutElement>();
            le.preferredWidth = 75;

            VerticalLayoutGroup vlg = stat.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 2;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // Icon placeholder
            GameObject icon = new GameObject("Icon");
            icon.transform.SetParent(stat.transform, false);
            RectTransform iconRT = icon.AddComponent<RectTransform>();
            iconRT.sizeDelta = new Vector2(24, 24);
            LayoutElement iconLE = icon.AddComponent<LayoutElement>();
            iconLE.preferredHeight = 24;

            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = color;
            iconImg.preserveAspect = true;

            // Value
            GameObject valueObj = new GameObject("Value");
            valueObj.transform.SetParent(stat.transform, false);
            LayoutElement valLE = valueObj.AddComponent<LayoutElement>();
            valLE.preferredHeight = 20;

            TextMeshProUGUI valTMP = valueObj.AddComponent<TextMeshProUGUI>();
            valTMP.text = value;
            valTMP.fontSize = 16;
            valTMP.color = color;
            valTMP.fontStyle = FontStyles.Bold;
            valTMP.alignment = TextAlignmentOptions.Center;
        }

        #endregion

        #region Quick Actions

        private static void CreateQuickActions()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            GameObject panel = FindOrCreate(canvas.transform, "QuickActionsPanel");

            float topOffset = HEADER_HEIGHT + PLAYER_CARD_HEIGHT + SECTION_SPACING * 2 + 15;

            RectTransform rt = GetOrAdd<RectTransform>(panel);
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, -topOffset);
            rt.sizeDelta = new Vector2(-40, QUICK_ACTION_HEIGHT);

            HorizontalLayoutGroup hlg = GetOrAdd<HorizontalLayoutGroup>(panel);
            hlg.spacing = 15;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // Quick Action Cards
            CreateQuickActionCard(panel.transform, "RankingsCard", "Rankings", "trophy", GOLD);
            CreateQuickActionCard(panel.transform, "SearchCard", "Buscar", "search", CYAN_NEON);
            CreateQuickActionCard(panel.transform, "MissionsCard", "Misiones", "target", GREEN_SUCCESS);

            Debug.Log("[MainMenuUI] Quick Actions creado");
        }

        private static void CreateQuickActionCard(Transform parent, string name, string label, string iconType, Color accentColor)
        {
            GameObject card = new GameObject(name);
            card.transform.SetParent(parent, false);

            Image bg = card.AddComponent<Image>();
            bg.color = CARD_BG;

            Button btn = card.AddComponent<Button>();
            btn.targetGraphic = bg;

            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = new Color(accentColor.r, accentColor.g, accentColor.b, 0.5f);
            outline.effectDistance = new Vector2(1.5f, 1.5f);

            // Layout
            VerticalLayoutGroup vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 5;
            vlg.padding = new RectOffset(10, 10, 12, 8);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;

            // Icon
            GameObject icon = new GameObject("Icon");
            icon.transform.SetParent(card.transform, false);
            LayoutElement iconLE = icon.AddComponent<LayoutElement>();
            iconLE.preferredHeight = 35;

            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = accentColor;
            iconImg.preserveAspect = true;

            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(card.transform, false);
            LayoutElement labelLE = labelObj.AddComponent<LayoutElement>();
            labelLE.preferredHeight = 22;

            TextMeshProUGUI labelTMP = labelObj.AddComponent<TextMeshProUGUI>();
            labelTMP.text = label;
            labelTMP.fontSize = 16;
            labelTMP.color = TEXT_WHITE;
            labelTMP.fontStyle = FontStyles.Bold;
            labelTMP.alignment = TextAlignmentOptions.Center;
        }

        #endregion

        #region Main Cards

        private static void CreateMainCards()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            GameObject panel = FindOrCreate(canvas.transform, "MainCardsPanel");

            float topOffset = HEADER_HEIGHT + PLAYER_CARD_HEIGHT + QUICK_ACTION_HEIGHT + SECTION_SPACING * 3 + 20;

            RectTransform rt = GetOrAdd<RectTransform>(panel);
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, -topOffset);
            rt.sizeDelta = new Vector2(-40, MAIN_CARD_HEIGHT * 2 + 15);

            VerticalLayoutGroup vlg = GetOrAdd<VerticalLayoutGroup>(panel);
            vlg.spacing = 15;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;

            // === JUGAR Card (Cyan) ===
            CreateMainCard(panel.transform, "PlayCard", "JUGAR",
                "Elige un juego y compite", CYAN_NEON, CYAN_GLOW, TEXT_DARK, true);

            // === CASH BATTLE Card (Gold - Premium) ===
            CreateMainCard(panel.transform, "CashBattleCard", "CASH BATTLE",
                "Compite por dinero real", GOLD, GOLD_GLOW, TEXT_DARK, false);

            Debug.Log("[MainMenuUI] Main Cards creados");
        }

        private static void CreateMainCard(Transform parent, string name, string title, string subtitle,
            Color bgColor, Color glowColor, Color textColor, bool isCyan)
        {
            GameObject card = new GameObject(name);
            card.transform.SetParent(parent, false);

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredHeight = MAIN_CARD_HEIGHT;
            le.minHeight = MAIN_CARD_HEIGHT;

            Image bg = card.AddComponent<Image>();
            bg.color = bgColor;

            Button btn = card.AddComponent<Button>();
            btn.targetGraphic = bg;

            ColorBlock colors = btn.colors;
            colors.highlightedColor = new Color(bgColor.r * 0.9f, bgColor.g * 0.9f, bgColor.b * 0.9f, 1f);
            colors.pressedColor = new Color(bgColor.r * 0.75f, bgColor.g * 0.75f, bgColor.b * 0.75f, 1f);
            btn.colors = colors;

            // Glow effect
            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = glowColor;
            outline.effectDistance = new Vector2(4, 4);

            Shadow shadow = card.AddComponent<Shadow>();
            shadow.effectColor = new Color(glowColor.r * 0.5f, glowColor.g * 0.5f, glowColor.b * 0.5f, 0.5f);
            shadow.effectDistance = new Vector2(3, -3);

            // Icon (izquierda)
            GameObject icon = FindOrCreate(card.transform, "Icon");
            RectTransform iconRT = GetOrAdd<RectTransform>(icon);
            iconRT.anchorMin = new Vector2(0, 0.5f);
            iconRT.anchorMax = new Vector2(0, 0.5f);
            iconRT.pivot = new Vector2(0, 0.5f);
            iconRT.anchoredPosition = new Vector2(25, 0);
            iconRT.sizeDelta = new Vector2(60, 60);

            Image iconImg = GetOrAdd<Image>(icon);
            iconImg.color = textColor;
            iconImg.preserveAspect = true;

            // Text Container
            GameObject textContainer = FindOrCreate(card.transform, "TextContainer");
            RectTransform textRT = GetOrAdd<RectTransform>(textContainer);
            textRT.anchorMin = new Vector2(0, 0);
            textRT.anchorMax = new Vector2(1, 1);
            textRT.offsetMin = new Vector2(100, 20);
            textRT.offsetMax = new Vector2(-20, -20);

            // Title
            GameObject titleObj = FindOrCreate(textContainer.transform, "Title");
            RectTransform titleRT = GetOrAdd<RectTransform>(titleObj);
            titleRT.anchorMin = new Vector2(0, 0.5f);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI titleTMP = GetOrAdd<TextMeshProUGUI>(titleObj);
            titleTMP.text = title;
            titleTMP.fontSize = 36;
            titleTMP.color = textColor;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Left;

            // Subtitle
            GameObject subObj = FindOrCreate(textContainer.transform, "Subtitle");
            RectTransform subRT = GetOrAdd<RectTransform>(subObj);
            subRT.anchorMin = new Vector2(0, 0);
            subRT.anchorMax = new Vector2(1, 0.45f);
            subRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI subTMP = GetOrAdd<TextMeshProUGUI>(subObj);
            subTMP.text = subtitle;
            subTMP.fontSize = 16;
            subTMP.color = new Color(textColor.r, textColor.g, textColor.b, 0.8f);
            subTMP.alignment = TextAlignmentOptions.Left;

            // Arrow indicator (derecha)
            GameObject arrow = FindOrCreate(card.transform, "Arrow");
            RectTransform arrowRT = GetOrAdd<RectTransform>(arrow);
            arrowRT.anchorMin = new Vector2(1, 0.5f);
            arrowRT.anchorMax = new Vector2(1, 0.5f);
            arrowRT.pivot = new Vector2(1, 0.5f);
            arrowRT.anchoredPosition = new Vector2(-20, 0);
            arrowRT.sizeDelta = new Vector2(30, 30);

            TextMeshProUGUI arrowTMP = GetOrAdd<TextMeshProUGUI>(arrow);
            arrowTMP.text = ">";
            arrowTMP.fontSize = 32;
            arrowTMP.color = textColor;
            arrowTMP.fontStyle = FontStyles.Bold;
            arrowTMP.alignment = TextAlignmentOptions.Center;
        }

        #endregion

        #region Daily Rewards

        private static void CreateDailyRewards()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            GameObject card = FindOrCreate(canvas.transform, "DailyRewardsCard");

            RectTransform rt = GetOrAdd<RectTransform>(card);
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, 25);
            rt.sizeDelta = new Vector2(-40, DAILY_REWARDS_HEIGHT);

            // Background with gradient effect
            Image bg = GetOrAdd<Image>(card);
            bg.color = new Color(0.15f, 0.08f, 0.25f, 1f); // Purple-ish dark

            Button btn = GetOrAdd<Button>(card);
            btn.targetGraphic = bg;

            // Colorful border
            Outline outline = GetOrAdd<Outline>(card);
            outline.effectColor = PURPLE_ACCENT;
            outline.effectDistance = new Vector2(2, 2);

            // Gift Icon (izquierda)
            GameObject icon = FindOrCreate(card.transform, "GiftIcon");
            RectTransform iconRT = GetOrAdd<RectTransform>(icon);
            iconRT.anchorMin = new Vector2(0, 0.5f);
            iconRT.anchorMax = new Vector2(0, 0.5f);
            iconRT.pivot = new Vector2(0, 0.5f);
            iconRT.anchoredPosition = new Vector2(20, 0);
            iconRT.sizeDelta = new Vector2(70, 70);

            Image iconImg = GetOrAdd<Image>(icon);
            iconImg.color = GOLD;
            iconImg.preserveAspect = true;

            // Text Container
            GameObject textContainer = FindOrCreate(card.transform, "TextContainer");
            RectTransform textRT = GetOrAdd<RectTransform>(textContainer);
            textRT.anchorMin = new Vector2(0, 0);
            textRT.anchorMax = new Vector2(0.65f, 1);
            textRT.offsetMin = new Vector2(100, 20);
            textRT.offsetMax = new Vector2(0, -20);

            // Title
            GameObject title = FindOrCreate(textContainer.transform, "Title");
            RectTransform titleRT = GetOrAdd<RectTransform>(title);
            titleRT.anchorMin = new Vector2(0, 0.55f);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI titleTMP = GetOrAdd<TextMeshProUGUI>(title);
            titleTMP.text = "RECOMPENSA DIARIA";
            titleTMP.fontSize = 20;
            titleTMP.color = GOLD;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Left;

            // Subtitle (day info)
            GameObject sub = FindOrCreate(textContainer.transform, "Subtitle");
            RectTransform subRT = GetOrAdd<RectTransform>(sub);
            subRT.anchorMin = new Vector2(0, 0);
            subRT.anchorMax = new Vector2(1, 0.5f);
            subRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI subTMP = GetOrAdd<TextMeshProUGUI>(sub);
            subTMP.text = "Dia 3 de 7 - Reclama tu premio!";
            subTMP.fontSize = 15;
            subTMP.color = TEXT_SECONDARY;
            subTMP.alignment = TextAlignmentOptions.Left;

            // Claim Button (derecha)
            GameObject claimBtn = FindOrCreate(card.transform, "ClaimButton");
            RectTransform claimRT = GetOrAdd<RectTransform>(claimBtn);
            claimRT.anchorMin = new Vector2(1, 0.5f);
            claimRT.anchorMax = new Vector2(1, 0.5f);
            claimRT.pivot = new Vector2(1, 0.5f);
            claimRT.anchoredPosition = new Vector2(-20, 0);
            claimRT.sizeDelta = new Vector2(100, 50);

            Image claimBg = GetOrAdd<Image>(claimBtn);
            claimBg.color = GREEN_SUCCESS;

            Button claimButton = GetOrAdd<Button>(claimBtn);
            claimButton.targetGraphic = claimBg;

            GameObject claimText = FindOrCreate(claimBtn.transform, "Text");
            RectTransform claimTextRT = GetOrAdd<RectTransform>(claimText);
            claimTextRT.anchorMin = Vector2.zero;
            claimTextRT.anchorMax = Vector2.one;
            claimTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI claimTMP = GetOrAdd<TextMeshProUGUI>(claimText);
            claimTMP.text = "Reclamar";
            claimTMP.fontSize = 16;
            claimTMP.color = TEXT_DARK;
            claimTMP.fontStyle = FontStyles.Bold;
            claimTMP.alignment = TextAlignmentOptions.Center;

            Debug.Log("[MainMenuUI] Daily Rewards creado");
        }

        #endregion

        #region Panels (Premium + Notifications)

        private static void CreatePanels()
        {
            CreatePremiumPanel();
            CreateNotificationsPanel();
        }

        private static void CreatePremiumPanel()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            GameObject panel = FindOrCreate(canvas.transform, "PremiumPanel");
            panel.SetActive(false);

            RectTransform rt = GetOrAdd<RectTransform>(panel);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            // Overlay
            GameObject overlay = FindOrCreate(panel.transform, "Overlay");
            RectTransform ovRT = GetOrAdd<RectTransform>(overlay);
            ovRT.anchorMin = Vector2.zero;
            ovRT.anchorMax = Vector2.one;
            ovRT.sizeDelta = Vector2.zero;

            Image ovImg = GetOrAdd<Image>(overlay);
            ovImg.color = new Color(0, 0, 0, 0.85f);

            Button ovBtn = GetOrAdd<Button>(overlay);
            ovBtn.targetGraphic = ovImg;

            // Container
            GameObject container = FindOrCreate(panel.transform, "Container");
            RectTransform contRT = GetOrAdd<RectTransform>(container);
            contRT.anchorMin = new Vector2(0.05f, 0.1f);
            contRT.anchorMax = new Vector2(0.95f, 0.9f);
            contRT.sizeDelta = Vector2.zero;

            Image contBg = GetOrAdd<Image>(container);
            contBg.color = CARD_BG;

            Outline contOutline = GetOrAdd<Outline>(container);
            contOutline.effectColor = GOLD;
            contOutline.effectDistance = new Vector2(2, 2);

            Debug.Log("[MainMenuUI] Premium Panel creado");
        }

        private static void CreateNotificationsPanel()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            GameObject panel = FindOrCreate(canvas.transform, "NotificationsPanel");
            panel.SetActive(false);

            RectTransform rt = GetOrAdd<RectTransform>(panel);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            // Overlay
            GameObject overlay = FindOrCreate(panel.transform, "Overlay");
            RectTransform ovRT = GetOrAdd<RectTransform>(overlay);
            ovRT.anchorMin = Vector2.zero;
            ovRT.anchorMax = Vector2.one;
            ovRT.sizeDelta = Vector2.zero;

            Image ovImg = GetOrAdd<Image>(overlay);
            ovImg.color = new Color(0, 0, 0, 0.85f);

            Button ovBtn = GetOrAdd<Button>(overlay);
            ovBtn.targetGraphic = ovImg;

            // Container
            GameObject container = FindOrCreate(panel.transform, "Container");
            RectTransform contRT = GetOrAdd<RectTransform>(container);
            contRT.anchorMin = new Vector2(0.05f, 0.15f);
            contRT.anchorMax = new Vector2(0.95f, 0.85f);
            contRT.sizeDelta = Vector2.zero;

            Image contBg = GetOrAdd<Image>(container);
            contBg.color = CARD_BG;

            Outline contOutline = GetOrAdd<Outline>(container);
            contOutline.effectColor = CYAN_DARK;
            contOutline.effectDistance = new Vector2(2, 2);

            Debug.Log("[MainMenuUI] Notifications Panel creado");
        }

        #endregion

        #region Assign Neon Icons

        private static void AssignNeonIcons()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[MainMenuUI] No se encontró Canvas en la escena");
                return;
            }

            int assigned = 0;

            // Header Icons
            assigned += TryAssignIcon(canvas.transform, "Header/SettingsButton/Icon", ICON_SETTINGS);
            assigned += TryAssignIcon(canvas.transform, "Header/NotificationsButton/Icon", ICON_NOTIFICATIONS);
            assigned += TryAssignIcon(canvas.transform, "Header/ProfileButton/Icon", ICON_PROFILE);

            // Player Card Icons
            assigned += TryAssignIcon(canvas.transform, "PlayerCard/AvatarSection/AvatarFrame/AvatarImage", ICON_AVATAR_DEFAULT);
            assigned += TryAssignIcon(canvas.transform, "PlayerCard/InfoSection/StatsRow/Trophies/Icon", ICON_TROPHY);
            assigned += TryAssignIcon(canvas.transform, "PlayerCard/InfoSection/StatsRow/Gems/Icon", ICON_GEM);
            assigned += TryAssignIcon(canvas.transform, "PlayerCard/InfoSection/StatsRow/Coins/Icon", ICON_COIN);

            // Quick Actions Icons
            assigned += TryAssignIcon(canvas.transform, "QuickActionsPanel/RankingsCard/Icon", ICON_RANKINGS);
            assigned += TryAssignIcon(canvas.transform, "QuickActionsPanel/SearchCard/Icon", ICON_SEARCH);
            assigned += TryAssignIcon(canvas.transform, "QuickActionsPanel/MissionsCard/Icon", ICON_MISSIONS);

            // Main Cards Icons
            assigned += TryAssignIcon(canvas.transform, "MainCardsPanel/PlayCard/Icon", ICON_PLAY);
            assigned += TryAssignIcon(canvas.transform, "MainCardsPanel/CashBattleCard/Icon", ICON_CASH_BATTLE);

            // Daily Rewards Icon
            assigned += TryAssignIcon(canvas.transform, "DailyRewardsCard/GiftIcon", ICON_GIFT);

            // Assign notification icon references to MainMenuManager
            AssignNotificationReferences(canvas.transform);

            Debug.Log($"[MainMenuUI] Iconos asignados: {assigned}/13");

            if (assigned > 0)
            {
                EditorUtility.SetDirty(canvas.gameObject);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
            }

            EditorUtility.DisplayDialog("Asignación de Iconos",
                $"Iconos asignados: {assigned}/13\n\n" +
                "Revisa la Console para ver detalles de iconos faltantes.",
                "OK");
        }

        private static void AssignNotificationReferences(Transform canvasTransform)
        {
            // Buscar MainMenuManager en la escena
            var manager = Object.FindFirstObjectByType<DigitPark.Managers.MainMenuManager>();
            if (manager == null)
            {
                Debug.LogWarning("[MainMenuUI] MainMenuManager no encontrado en la escena");
                return;
            }

            // Cargar sprites de notificación
            Sprite normalSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_NOTIFICATIONS);
            Sprite activeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_NOTIFICATIONS_ACTIVE);

            if (normalSprite != null)
            {
                var serializedObject = new SerializedObject(manager);
                var normalProp = serializedObject.FindProperty("notificationIconNormal");
                if (normalProp != null)
                {
                    normalProp.objectReferenceValue = normalSprite;
                    Debug.Log("[MainMenuUI] Asignado: notificationIconNormal");
                }
                serializedObject.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogWarning($"[MainMenuUI] No se encontró sprite: {ICON_NOTIFICATIONS}");
            }

            if (activeSprite != null)
            {
                var serializedObject = new SerializedObject(manager);
                var activeProp = serializedObject.FindProperty("notificationIconActive");
                if (activeProp != null)
                {
                    activeProp.objectReferenceValue = activeSprite;
                    Debug.Log("[MainMenuUI] Asignado: notificationIconActive");
                }
                serializedObject.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogWarning($"[MainMenuUI] No se encontró sprite: {ICON_NOTIFICATIONS_ACTIVE}");
            }

            // Buscar y asignar NotificationsButton
            Transform notifBtn = canvasTransform.Find("Header/NotificationsButton");
            if (notifBtn != null)
            {
                Button btnComponent = notifBtn.GetComponent<Button>();
                if (btnComponent != null)
                {
                    var serializedObject = new SerializedObject(manager);
                    var btnProp = serializedObject.FindProperty("notificationsButton");
                    if (btnProp != null)
                    {
                        btnProp.objectReferenceValue = btnComponent;
                        Debug.Log("[MainMenuUI] Asignado: notificationsButton");
                    }
                    serializedObject.ApplyModifiedProperties();
                }

                // Buscar y asignar el Image del icono
                Transform iconTransform = notifBtn.Find("Icon");
                if (iconTransform != null)
                {
                    Image iconImg = iconTransform.GetComponent<Image>();
                    if (iconImg != null)
                    {
                        var serializedObject = new SerializedObject(manager);
                        var imgProp = serializedObject.FindProperty("notificationIconImage");
                        if (imgProp != null)
                        {
                            imgProp.objectReferenceValue = iconImg;
                            Debug.Log("[MainMenuUI] Asignado: notificationIconImage");
                        }
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }

            EditorUtility.SetDirty(manager);
            Debug.Log("[MainMenuUI] Referencias de notificación asignadas al MainMenuManager");
        }

        private static int TryAssignIcon(Transform root, string path, string iconPath)
        {
            // Buscar el elemento por path
            Transform target = root;
            string[] parts = path.Split('/');

            foreach (string part in parts)
            {
                target = target.Find(part);
                if (target == null)
                {
                    Debug.LogWarning($"[MainMenuUI] No se encontró: {path}");
                    return 0;
                }
            }

            // Cargar el icono
            Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            if (icon == null)
            {
                Debug.LogWarning($"[MainMenuUI] Icono no encontrado: {iconPath}");
                return 0;
            }

            // Asignar al Image component
            Image img = target.GetComponent<Image>();
            if (img == null)
            {
                Debug.LogWarning($"[MainMenuUI] No tiene Image component: {path}");
                return 0;
            }

            img.sprite = icon;
            img.preserveAspect = true;
            Debug.Log($"[MainMenuUI] Asignado: {path} = {System.IO.Path.GetFileName(iconPath)}");
            return 1;
        }

        #endregion

        #region Helpers

        private static GameObject FindOrCreate(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null) return existing.gameObject;

            GameObject newObj = new GameObject(name);
            newObj.transform.SetParent(parent, false);
            return newObj;
        }

        private static T GetOrAdd<T>(GameObject obj) where T : Component
        {
            T component = obj.GetComponent<T>();
            if (component == null) component = obj.AddComponent<T>();
            return component;
        }

        #endregion
    }
}
