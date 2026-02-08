using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Profile UI Builder - Rediseño 2026
    /// Avatar centrado + stats en cards + game selection overlay
    /// Portrait 9:16 (1080x1920), matchWidthOrHeight=0
    ///
    /// Menu: DigitPark/UI Builders/Social/Profile
    /// </summary>
    public class ProfileUIBuilder : EditorWindow
    {
        #region Colors

        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_GLOW = new Color(0f, 0.85f, 1f, 0.8f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.5f, 1f);

        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);

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

        private const float HEADER_TOP = 0.985f;
        private const float HEADER_BOT = 0.945f;

        private const float AVATAR_TOP = 0.935f;
        private const float AVATAR_BOT = 0.70f;

        private const float GENSTATS_TOP = 0.69f;
        private const float GENSTATS_BOT = 0.45f;

        private const float GAMESTATS_TOP = 0.44f;
        private const float GAMESTATS_BOT = 0.19f;

        private const float ACTIONS_TOP = 0.18f;
        private const float ACTIONS_BOT = 0.10f;

        private const float CTA_TOP = 0.09f;
        private const float CTA_BOT = 0.03f;

        private const float SIDE_PAD = 20f;

        #endregion

        [MenuItem("DigitPark/UI Builders/Social/Profile", false, 170)]
        public static void ShowWindow()
        {
            GetWindow<ProfileUIBuilder>("Profile Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Profile UI Builder", EditorStyles.boldLabel);
            GUILayout.Label("Rediseño 2026 - Avatar + Stats Cards + Neon Theme", EditorStyles.miniLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Layout completo (de arriba a abajo):\n\n" +
                "1. Header (Back, PERFIL, Add Friend)\n" +
                "2. Avatar Card (Avatar grande + username + status)\n" +
                "3. General Stats (Partidas, Wins, Rate, Tiempos)\n" +
                "4. Game Stats (stats por juego con colores)\n" +
                "5. Action Buttons (Amigos, Historial)\n" +
                "6. CTA (RETAR - cyan prominente)\n" +
                "7. Game Selection Panel (overlay para retos)",
                MessageType.Info);

            GUILayout.Space(15);

            GUI.backgroundColor = CYAN_NEON;
            if (GUILayout.Button("RECONSTRUIR PROFILE COMPLETO", GUILayout.Height(50)))
                RebuildProfile();
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);
            GUILayout.Label("Secciones individuales:", EditorStyles.boldLabel);

            if (GUILayout.Button("1. Header", GUILayout.Height(25))) CreateHeader();
            if (GUILayout.Button("2. Avatar Card", GUILayout.Height(25))) CreateAvatarCard();
            if (GUILayout.Button("3. General Stats", GUILayout.Height(25))) CreateGeneralStatsCard();
            if (GUILayout.Button("4. Game Stats", GUILayout.Height(25))) CreateGameStatsCard();
            if (GUILayout.Button("5. Action Buttons", GUILayout.Height(25))) CreateActionRow();
            if (GUILayout.Button("6. CTA Button", GUILayout.Height(25))) CreateCTASection();
            if (GUILayout.Button("7. Game Selection Panel", GUILayout.Height(25))) CreateGameSelectionPanel();

            GUILayout.Space(15);

            GUI.backgroundColor = GOLD;
            if (GUILayout.Button("ASIGNAR REFERENCIAS AL MANAGER", GUILayout.Height(35)))
                SetupManagerReferences();
            GUI.backgroundColor = Color.white;
        }

        #region Main Rebuild

        private static void RebuildProfile()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[ProfileUI] No se encontró Canvas");
                return;
            }

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0f;
            }

            string[] oldNames = {
                "Background", "Header", "AvatarCard", "GeneralStatsCard",
                "GameStatsCard", "ActionRow", "CTASection", "GameSelectionPanel",
                "ProfilePanel", "StatsPanel", "ContentPanel"
            };
            foreach (var n in oldNames)
            {
                Transform t = canvas.transform.Find(n);
                if (t != null) DestroyImmediate(t.gameObject);
            }

            CreateBackground(canvas.transform);
            CreateHeader();
            CreateAvatarCard();
            CreateGeneralStatsCard();
            CreateGameStatsCard();
            CreateActionRow();
            CreateCTASection();
            CreateGameSelectionPanel();
            SetupManagerReferences();

            Debug.Log("[ProfileUI] Profile REDISEÑADO exitosamente!");
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
            var img = GetOrAdd<Image>(bg);
            img.color = DARK_BG;
            img.raycastTarget = false;
        }

        #endregion

        #region Header

        private static void CreateHeader()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            var header = FindOrCreate(canvas.transform, "Header");
            var rt = GetOrAdd<RectTransform>(header);
            SetAnchors(rt, 0, HEADER_BOT, 1, HEADER_TOP);
            GetOrAdd<Image>(header).color = HEADER_BG;

            // Back Button (left)
            var backBtn = FindOrCreate(header.transform, "BackButton");
            var bRT = GetOrAdd<RectTransform>(backBtn);
            bRT.anchorMin = new Vector2(0, 0.5f);
            bRT.anchorMax = new Vector2(0, 0.5f);
            bRT.pivot = new Vector2(0, 0.5f);
            bRT.anchoredPosition = new Vector2(15, 0);
            bRT.sizeDelta = new Vector2(50, 50);
            var bBg = GetOrAdd<Image>(backBtn);
            bBg.color = new Color(1, 1, 1, 0.06f);
            GetOrAdd<Button>(backBtn).targetGraphic = bBg;

            var backIcon = FindOrCreate(backBtn.transform, "Icon");
            var biRT = GetOrAdd<RectTransform>(backIcon);
            biRT.anchorMin = Vector2.zero;
            biRT.anchorMax = Vector2.one;
            biRT.offsetMin = new Vector2(8, 8);
            biRT.offsetMax = new Vector2(-8, -8);
            var biTMP = GetOrAdd<TextMeshProUGUI>(backIcon);
            biTMP.text = "\u2039";
            biTMP.fontSize = 36;
            biTMP.color = CYAN_NEON;
            biTMP.fontStyle = FontStyles.Bold;
            biTMP.alignment = TextAlignmentOptions.Center;

            // Title (center)
            var title = FindOrCreate(header.transform, "TitleText");
            var tRT = GetOrAdd<RectTransform>(title);
            tRT.anchorMin = new Vector2(0.2f, 0);
            tRT.anchorMax = new Vector2(0.8f, 1);
            tRT.offsetMin = Vector2.zero;
            tRT.offsetMax = Vector2.zero;
            var tTMP = GetOrAdd<TextMeshProUGUI>(title);
            tTMP.text = "PERFIL";
            tTMP.fontSize = 28;
            tTMP.color = TEXT_WHITE;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.Center;

            // Add Friend Button (right)
            var addBtn = FindOrCreate(header.transform, "AddFriendButton");
            var aRT = GetOrAdd<RectTransform>(addBtn);
            aRT.anchorMin = new Vector2(1, 0.5f);
            aRT.anchorMax = new Vector2(1, 0.5f);
            aRT.pivot = new Vector2(1, 0.5f);
            aRT.anchoredPosition = new Vector2(-15, 0);
            aRT.sizeDelta = new Vector2(50, 50);
            var aBg = GetOrAdd<Image>(addBtn);
            aBg.color = new Color(1, 1, 1, 0.06f);
            GetOrAdd<Button>(addBtn).targetGraphic = aBg;

            var addIcon = FindOrCreate(addBtn.transform, "Icon");
            var aiRT = GetOrAdd<RectTransform>(addIcon);
            aiRT.anchorMin = Vector2.zero;
            aiRT.anchorMax = Vector2.one;
            aiRT.offsetMin = new Vector2(8, 8);
            aiRT.offsetMax = new Vector2(-8, -8);
            var aiTMP = GetOrAdd<TextMeshProUGUI>(addIcon);
            aiTMP.text = "+";
            aiTMP.fontSize = 28;
            aiTMP.color = GREEN_SUCCESS;
            aiTMP.fontStyle = FontStyles.Bold;
            aiTMP.alignment = TextAlignmentOptions.Center;

            Debug.Log("[ProfileUI] Header creado");
        }

        #endregion

        #region Avatar Card

        private static void CreateAvatarCard()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            var card = FindOrCreate(canvas.transform, "AvatarCard");
            var rt = GetOrAdd<RectTransform>(card);
            SetAnchorsWithPad(rt, AVATAR_BOT, AVATAR_TOP);

            var bg = GetOrAdd<Image>(card);
            bg.color = CARD_BG;
            var outline = GetOrAdd<Outline>(card);
            outline.effectColor = CYAN_DARK;
            outline.effectDistance = new Vector2(2, 2);

            // Avatar Frame (centered, upper area)
            var frame = FindOrCreate(card.transform, "AvatarFrame");
            var fRT = GetOrAdd<RectTransform>(frame);
            fRT.anchorMin = new Vector2(0.5f, 0.50f);
            fRT.anchorMax = new Vector2(0.5f, 0.50f);
            fRT.pivot = new Vector2(0.5f, 0f);
            fRT.anchoredPosition = Vector2.zero;
            fRT.sizeDelta = new Vector2(150, 150);

            var frameImg = GetOrAdd<Image>(frame);
            frameImg.color = CYAN_NEON;
            var frameOutline = GetOrAdd<Outline>(frame);
            frameOutline.effectColor = CYAN_GLOW;
            frameOutline.effectDistance = new Vector2(4, 4);

            // Avatar Image (inside frame)
            var avImg = FindOrCreate(frame.transform, "AvatarImage");
            var avRT = GetOrAdd<RectTransform>(avImg);
            avRT.anchorMin = new Vector2(0.06f, 0.06f);
            avRT.anchorMax = new Vector2(0.94f, 0.94f);
            avRT.offsetMin = Vector2.zero;
            avRT.offsetMax = Vector2.zero;
            var avImgComp = GetOrAdd<Image>(avImg);
            avImgComp.color = CARD_BG_LIGHT;
            avImgComp.preserveAspect = true;

            // AvatarUI component
            var avatarUI = GetOrAdd<DigitPark.UI.Components.AvatarUI>(avImg);
            var avatarSO = new SerializedObject(avatarUI);
            avatarSO.FindProperty("loadCurrentUserOnStart").boolValue = true;
            avatarSO.FindProperty("isEditable").boolValue = true;
            avatarSO.FindProperty("avatarImage").objectReferenceValue = avImgComp;
            avatarSO.ApplyModifiedProperties();

            // Edit Avatar Button (bottom-right corner of avatar)
            var editBtn = FindOrCreate(frame.transform, "EditButton");
            var eRT = GetOrAdd<RectTransform>(editBtn);
            eRT.anchorMin = new Vector2(1, 0);
            eRT.anchorMax = new Vector2(1, 0);
            eRT.pivot = new Vector2(0.5f, 0.5f);
            eRT.anchoredPosition = new Vector2(-10, 10);
            eRT.sizeDelta = new Vector2(40, 40);
            var eBg = GetOrAdd<Image>(editBtn);
            eBg.color = CYAN_NEON;
            GetOrAdd<Button>(editBtn).targetGraphic = eBg;

            var editIcon = FindOrCreate(editBtn.transform, "Icon");
            var eiRT = GetOrAdd<RectTransform>(editIcon);
            eiRT.anchorMin = Vector2.zero;
            eiRT.anchorMax = Vector2.one;
            eiRT.offsetMin = new Vector2(6, 6);
            eiRT.offsetMax = new Vector2(-6, -6);
            var eiTMP = GetOrAdd<TextMeshProUGUI>(editIcon);
            eiTMP.text = "Edit";
            eiTMP.fontSize = 20;
            eiTMP.color = TEXT_DARK;
            eiTMP.fontStyle = FontStyles.Bold;
            eiTMP.alignment = TextAlignmentOptions.Center;

            // Username
            var username = FindOrCreate(card.transform, "UsernameText");
            var uRT = GetOrAdd<RectTransform>(username);
            uRT.anchorMin = new Vector2(0.1f, 0.18f);
            uRT.anchorMax = new Vector2(0.9f, 0.42f);
            uRT.offsetMin = Vector2.zero;
            uRT.offsetMax = Vector2.zero;
            var uTMP = GetOrAdd<TextMeshProUGUI>(username);
            uTMP.text = "@Username";
            uTMP.fontSize = 30;
            uTMP.color = TEXT_WHITE;
            uTMP.fontStyle = FontStyles.Bold;
            uTMP.alignment = TextAlignmentOptions.Center;

            // Status Text
            var status = FindOrCreate(card.transform, "StatusText");
            var sRT = GetOrAdd<RectTransform>(status);
            sRT.anchorMin = new Vector2(0.15f, 0.04f);
            sRT.anchorMax = new Vector2(0.85f, 0.18f);
            sRT.offsetMin = Vector2.zero;
            sRT.offsetMax = Vector2.zero;
            var sTMP = GetOrAdd<TextMeshProUGUI>(status);
            sTMP.text = "Tu perfil";
            sTMP.fontSize = 18;
            sTMP.color = CYAN_NEON;
            sTMP.alignment = TextAlignmentOptions.Center;

            Debug.Log("[ProfileUI] Avatar Card creado");
        }

        #endregion

        #region General Stats Card

        private static void CreateGeneralStatsCard()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            var card = FindOrCreate(canvas.transform, "GeneralStatsCard");
            var rt = GetOrAdd<RectTransform>(card);
            SetAnchorsWithPad(rt, GENSTATS_BOT, GENSTATS_TOP);

            var bg = GetOrAdd<Image>(card);
            bg.color = CARD_BG;
            var outline = GetOrAdd<Outline>(card);
            outline.effectColor = CYAN_DARK;
            outline.effectDistance = new Vector2(2, 2);

            // Title
            var title = FindOrCreate(card.transform, "Title");
            var tRT = GetOrAdd<RectTransform>(title);
            tRT.anchorMin = new Vector2(0, 0.88f);
            tRT.anchorMax = new Vector2(1, 1);
            tRT.offsetMin = new Vector2(15, 0);
            tRT.offsetMax = new Vector2(-15, -5);
            var tTMP = GetOrAdd<TextMeshProUGUI>(title);
            tTMP.text = "ESTAD\u00CDSTICAS GENERALES";
            tTMP.fontSize = 16;
            tTMP.color = TEXT_SECONDARY;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.Left;

            // Top Row: Total Games, Wins, Win Rate
            CreateStatBlock(card.transform, "TotalGames", "0", "Partidas", CYAN_NEON,
                0.02f, 0.42f, 0.32f, 0.85f);
            CreateStatBlock(card.transform, "Wins", "0", "Victorias", GREEN_SUCCESS,
                0.34f, 0.42f, 0.64f, 0.85f);
            CreateStatBlock(card.transform, "WinRate", "0%", "Win Rate", GOLD,
                0.66f, 0.42f, 0.98f, 0.85f);

            // Bottom Row: Best Time, Average Time
            CreateStatBlock(card.transform, "BestTime", "--", "Mejor Tiempo", ORANGE_ACCENT,
                0.05f, 0.03f, 0.48f, 0.38f);
            CreateStatBlock(card.transform, "AvgTime", "--", "Prom. Tiempo", PURPLE_ACCENT,
                0.52f, 0.03f, 0.95f, 0.38f);

            Debug.Log("[ProfileUI] General Stats Card creado");
        }

        private static void CreateStatBlock(Transform parent, string name,
            string value, string label, Color accent,
            float xMin, float yMin, float xMax, float yMax)
        {
            var block = FindOrCreate(parent, name);
            var rt = GetOrAdd<RectTransform>(block);
            rt.anchorMin = new Vector2(xMin, yMin);
            rt.anchorMax = new Vector2(xMax, yMax);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var blockBg = GetOrAdd<Image>(block);
            blockBg.color = CARD_BG_LIGHT;
            blockBg.raycastTarget = false;

            // Value (big number)
            var val = FindOrCreate(block.transform, "Value");
            var vRT = GetOrAdd<RectTransform>(val);
            vRT.anchorMin = new Vector2(0, 0.40f);
            vRT.anchorMax = new Vector2(1, 1);
            vRT.offsetMin = new Vector2(5, 0);
            vRT.offsetMax = new Vector2(-5, -5);
            var vTMP = GetOrAdd<TextMeshProUGUI>(val);
            vTMP.text = value;
            vTMP.fontSize = 28;
            vTMP.color = accent;
            vTMP.fontStyle = FontStyles.Bold;
            vTMP.alignment = TextAlignmentOptions.Center;
            vTMP.enableAutoSizing = true;
            vTMP.fontSizeMin = 16;
            vTMP.fontSizeMax = 28;

            // Label
            var lbl = FindOrCreate(block.transform, "Label");
            var lRT = GetOrAdd<RectTransform>(lbl);
            lRT.anchorMin = new Vector2(0, 0);
            lRT.anchorMax = new Vector2(1, 0.38f);
            lRT.offsetMin = new Vector2(5, 3);
            lRT.offsetMax = new Vector2(-5, 0);
            var lTMP = GetOrAdd<TextMeshProUGUI>(lbl);
            lTMP.text = label;
            lTMP.fontSize = 12;
            lTMP.color = TEXT_SECONDARY;
            lTMP.alignment = TextAlignmentOptions.Center;
            lTMP.enableAutoSizing = true;
            lTMP.fontSizeMin = 9;
            lTMP.fontSizeMax = 12;
        }

        #endregion

        #region Game Stats Card

        private static void CreateGameStatsCard()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            var card = FindOrCreate(canvas.transform, "GameStatsCard");
            var rt = GetOrAdd<RectTransform>(card);
            SetAnchorsWithPad(rt, GAMESTATS_BOT, GAMESTATS_TOP);

            var bg = GetOrAdd<Image>(card);
            bg.color = CARD_BG;
            var outline = GetOrAdd<Outline>(card);
            outline.effectColor = CYAN_DARK;
            outline.effectDistance = new Vector2(2, 2);

            // Title
            var title = FindOrCreate(card.transform, "Title");
            var tRT = GetOrAdd<RectTransform>(title);
            tRT.anchorMin = new Vector2(0, 0.88f);
            tRT.anchorMax = new Vector2(1, 1);
            tRT.offsetMin = new Vector2(15, 0);
            tRT.offsetMax = new Vector2(-15, -5);
            var tTMP = GetOrAdd<TextMeshProUGUI>(title);
            tTMP.text = "STATS POR JUEGO";
            tTMP.fontSize = 16;
            tTMP.color = TEXT_SECONDARY;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.Left;

            // 5 game rows
            CreateGameRow(card.transform, "DigitRush", "Digit Rush", "-- | 0%", CYAN_NEON, 0);
            CreateGameRow(card.transform, "MemoryPairs", "Memory Pairs", "-- | 0%", PURPLE_ACCENT, 1);
            CreateGameRow(card.transform, "QuickMath", "Quick Math", "-- | 0%", GREEN_SUCCESS, 2);
            CreateGameRow(card.transform, "FlashTap", "Flash Tap", "-- | 0%", ORANGE_ACCENT, 3);
            CreateGameRow(card.transform, "OddOneOut", "Odd One Out", "-- | 0%", GOLD, 4);

            Debug.Log("[ProfileUI] Game Stats Card creado");
        }

        private static void CreateGameRow(Transform parent, string name,
            string label, string value, Color accent, int index)
        {
            float rowHeight = 0.16f;
            float gap = 0.015f;
            float startY = 0.85f;

            float yTop = startY - index * (rowHeight + gap);
            float yBot = yTop - rowHeight;

            var row = FindOrCreate(parent, name);
            var rt = GetOrAdd<RectTransform>(row);
            rt.anchorMin = new Vector2(0, yBot);
            rt.anchorMax = new Vector2(1, yTop);
            rt.offsetMin = new Vector2(15, 0);
            rt.offsetMax = new Vector2(-15, 0);

            // Accent bar (left edge)
            var bar = FindOrCreate(row.transform, "AccentBar");
            var barRT = GetOrAdd<RectTransform>(bar);
            barRT.anchorMin = new Vector2(0, 0.15f);
            barRT.anchorMax = new Vector2(0, 0.85f);
            barRT.pivot = new Vector2(0, 0.5f);
            barRT.anchoredPosition = Vector2.zero;
            barRT.sizeDelta = new Vector2(4, 0);
            var barImg = GetOrAdd<Image>(bar);
            barImg.color = accent;
            barImg.raycastTarget = false;

            // Game Name
            var lbl = FindOrCreate(row.transform, "Label");
            var lRT = GetOrAdd<RectTransform>(lbl);
            lRT.anchorMin = new Vector2(0, 0);
            lRT.anchorMax = new Vector2(0.50f, 1);
            lRT.offsetMin = new Vector2(14, 0);
            lRT.offsetMax = Vector2.zero;
            var lTMP = GetOrAdd<TextMeshProUGUI>(lbl);
            lTMP.text = label;
            lTMP.fontSize = 17;
            lTMP.color = TEXT_WHITE;
            lTMP.alignment = TextAlignmentOptions.Left;

            // Value
            var val = FindOrCreate(row.transform, "Value");
            var vRT = GetOrAdd<RectTransform>(val);
            vRT.anchorMin = new Vector2(0.50f, 0);
            vRT.anchorMax = new Vector2(1, 1);
            vRT.offsetMin = Vector2.zero;
            vRT.offsetMax = Vector2.zero;
            var vTMP = GetOrAdd<TextMeshProUGUI>(val);
            vTMP.text = value;
            vTMP.fontSize = 17;
            vTMP.color = accent;
            vTMP.fontStyle = FontStyles.Bold;
            vTMP.alignment = TextAlignmentOptions.Right;

            // Separator line
            var sep = FindOrCreate(row.transform, "Separator");
            var sepRT = GetOrAdd<RectTransform>(sep);
            sepRT.anchorMin = new Vector2(0, 0);
            sepRT.anchorMax = new Vector2(1, 0);
            sepRT.pivot = new Vector2(0.5f, 0);
            sepRT.anchoredPosition = Vector2.zero;
            sepRT.sizeDelta = new Vector2(0, 1);
            var sepImg = GetOrAdd<Image>(sep);
            sepImg.color = new Color(1, 1, 1, 0.05f);
            sepImg.raycastTarget = false;
        }

        #endregion

        #region Action Row

        private static void CreateActionRow()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            var row = FindOrCreate(canvas.transform, "ActionRow");
            var rt = GetOrAdd<RectTransform>(row);
            SetAnchorsWithPad(rt, ACTIONS_BOT, ACTIONS_TOP);

            var hlg = GetOrAdd<HorizontalLayoutGroup>(row);
            hlg.spacing = 15;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            for (int i = row.transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(row.transform.GetChild(i).gameObject);

            CreateActionButton(row.transform, "FriendsButton", "Amigos", CYAN_NEON);
            CreateActionButton(row.transform, "HistoryButton", "Historial", PURPLE_ACCENT);

            Debug.Log("[ProfileUI] Action Row creado");
        }

        private static void CreateActionButton(Transform parent, string name, string label, Color accent)
        {
            var btn = new GameObject(name);
            btn.transform.SetParent(parent, false);

            var bg = btn.AddComponent<Image>();
            bg.color = CARD_BG;
            btn.AddComponent<Button>().targetGraphic = bg;

            var outline = btn.AddComponent<Outline>();
            outline.effectColor = new Color(accent.r, accent.g, accent.b, 0.5f);
            outline.effectDistance = new Vector2(2, 2);

            var text = new GameObject("Text");
            text.transform.SetParent(btn.transform, false);
            var tRT = text.AddComponent<RectTransform>();
            tRT.anchorMin = Vector2.zero;
            tRT.anchorMax = Vector2.one;
            tRT.offsetMin = Vector2.zero;
            tRT.offsetMax = Vector2.zero;
            var tTMP = text.AddComponent<TextMeshProUGUI>();
            tTMP.text = label;
            tTMP.fontSize = 20;
            tTMP.color = accent;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.Center;
        }

        #endregion

        #region CTA Section

        private static void CreateCTASection()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            var section = FindOrCreate(canvas.transform, "CTASection");
            var rt = GetOrAdd<RectTransform>(section);
            SetAnchorsWithPad(rt, CTA_BOT, CTA_TOP);

            var btn = FindOrCreate(section.transform, "ChallengeButton");
            var bRT = GetOrAdd<RectTransform>(btn);
            bRT.anchorMin = Vector2.zero;
            bRT.anchorMax = Vector2.one;
            bRT.offsetMin = Vector2.zero;
            bRT.offsetMax = Vector2.zero;

            var bg = GetOrAdd<Image>(btn);
            bg.color = CYAN_NEON;
            var button = GetOrAdd<Button>(btn);
            button.targetGraphic = bg;
            var colors = button.colors;
            colors.highlightedColor = new Color(0, 0.9f, 0.9f, 1);
            colors.pressedColor = new Color(0, 0.7f, 0.7f, 1);
            button.colors = colors;

            var outline = GetOrAdd<Outline>(btn);
            outline.effectColor = CYAN_GLOW;
            outline.effectDistance = new Vector2(3, 3);

            var text = FindOrCreate(btn.transform, "Text");
            var tRT = GetOrAdd<RectTransform>(text);
            tRT.anchorMin = Vector2.zero;
            tRT.anchorMax = Vector2.one;
            tRT.offsetMin = Vector2.zero;
            tRT.offsetMax = Vector2.zero;
            var tTMP = GetOrAdd<TextMeshProUGUI>(text);
            tTMP.text = "RETAR";
            tTMP.fontSize = 26;
            tTMP.color = TEXT_DARK;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.Center;

            Debug.Log("[ProfileUI] CTA Section creado");
        }

        #endregion

        #region Game Selection Panel (Overlay)

        private static void CreateGameSelectionPanel()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            var panel = FindOrCreate(canvas.transform, "GameSelectionPanel");
            panel.SetActive(false);
            var rt = GetOrAdd<RectTransform>(panel);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Dark overlay
            var overlay = FindOrCreate(panel.transform, "DarkOverlay");
            var ovRT = GetOrAdd<RectTransform>(overlay);
            ovRT.anchorMin = Vector2.zero;
            ovRT.anchorMax = Vector2.one;
            ovRT.offsetMin = Vector2.zero;
            ovRT.offsetMax = Vector2.zero;
            var ovImg = GetOrAdd<Image>(overlay);
            ovImg.color = new Color(0, 0, 0, 0.85f);
            GetOrAdd<Button>(overlay).targetGraphic = ovImg;

            // Container card
            var container = FindOrCreate(panel.transform, "Container");
            var cRT = GetOrAdd<RectTransform>(container);
            cRT.anchorMin = new Vector2(0.08f, 0.22f);
            cRT.anchorMax = new Vector2(0.92f, 0.72f);
            cRT.offsetMin = Vector2.zero;
            cRT.offsetMax = Vector2.zero;
            var cBg = GetOrAdd<Image>(container);
            cBg.color = CARD_BG;
            var cOutline = GetOrAdd<Outline>(container);
            cOutline.effectColor = CYAN_DARK;
            cOutline.effectDistance = new Vector2(2, 2);

            // Title
            var title = FindOrCreate(container.transform, "Title");
            var tRT = GetOrAdd<RectTransform>(title);
            tRT.anchorMin = new Vector2(0, 0.88f);
            tRT.anchorMax = new Vector2(1, 1);
            tRT.offsetMin = new Vector2(20, 0);
            tRT.offsetMax = new Vector2(-20, -10);
            var tTMP = GetOrAdd<TextMeshProUGUI>(title);
            tTMP.text = "ELIGE UN JUEGO";
            tTMP.fontSize = 22;
            tTMP.color = CYAN_NEON;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.Center;

            // Games List
            var list = FindOrCreate(container.transform, "GamesList");
            var lRT = GetOrAdd<RectTransform>(list);
            lRT.anchorMin = new Vector2(0, 0.12f);
            lRT.anchorMax = new Vector2(1, 0.85f);
            lRT.offsetMin = new Vector2(20, 5);
            lRT.offsetMax = new Vector2(-20, -5);

            var vlg = GetOrAdd<VerticalLayoutGroup>(list);
            vlg.spacing = 8;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = true;

            for (int i = list.transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(list.transform.GetChild(i).gameObject);

            CreateGameSelectButton(list.transform, "DigitRushBtn", "Digit Rush", CYAN_NEON);
            CreateGameSelectButton(list.transform, "MemoryPairsBtn", "Memory Pairs", PURPLE_ACCENT);
            CreateGameSelectButton(list.transform, "QuickMathBtn", "Quick Math", GREEN_SUCCESS);
            CreateGameSelectButton(list.transform, "FlashTapBtn", "Flash Tap", ORANGE_ACCENT);
            CreateGameSelectButton(list.transform, "OddOneOutBtn", "Odd One Out", GOLD);

            // Cancel Button
            var cancelBtn = FindOrCreate(container.transform, "CancelButton");
            var cnRT = GetOrAdd<RectTransform>(cancelBtn);
            cnRT.anchorMin = new Vector2(0.25f, 0.01f);
            cnRT.anchorMax = new Vector2(0.75f, 0.10f);
            cnRT.offsetMin = Vector2.zero;
            cnRT.offsetMax = Vector2.zero;
            var cnBg = GetOrAdd<Image>(cancelBtn);
            cnBg.color = new Color(1, 1, 1, 0.1f);
            GetOrAdd<Button>(cancelBtn).targetGraphic = cnBg;

            var cnText = FindOrCreate(cancelBtn.transform, "Text");
            var cntRT = GetOrAdd<RectTransform>(cnText);
            cntRT.anchorMin = Vector2.zero;
            cntRT.anchorMax = Vector2.one;
            cntRT.offsetMin = Vector2.zero;
            cntRT.offsetMax = Vector2.zero;
            var cntTMP = GetOrAdd<TextMeshProUGUI>(cnText);
            cntTMP.text = "Cancelar";
            cntTMP.fontSize = 18;
            cntTMP.color = TEXT_SECONDARY;
            cntTMP.alignment = TextAlignmentOptions.Center;

            Debug.Log("[ProfileUI] Game Selection Panel creado");
        }

        private static void CreateGameSelectButton(Transform parent, string name, string label, Color accent)
        {
            var btn = new GameObject(name);
            btn.transform.SetParent(parent, false);

            var bg = btn.AddComponent<Image>();
            bg.color = CARD_BG_LIGHT;
            btn.AddComponent<Button>().targetGraphic = bg;

            var outline = btn.AddComponent<Outline>();
            outline.effectColor = new Color(accent.r, accent.g, accent.b, 0.4f);
            outline.effectDistance = new Vector2(1, 1);

            var text = new GameObject("Text");
            text.transform.SetParent(btn.transform, false);
            var tRT = text.AddComponent<RectTransform>();
            tRT.anchorMin = Vector2.zero;
            tRT.anchorMax = Vector2.one;
            tRT.offsetMin = new Vector2(15, 0);
            tRT.offsetMax = new Vector2(-15, 0);
            var tTMP = text.AddComponent<TextMeshProUGUI>();
            tTMP.text = label;
            tTMP.fontSize = 20;
            tTMP.color = accent;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.Center;
        }

        #endregion

        #region Manager References

        private static void SetupManagerReferences()
        {
            var manager = Object.FindFirstObjectByType<DigitPark.Managers.ProfileManager>();
            if (manager == null)
            {
                Debug.LogWarning("[ProfileUI] ProfileManager no encontrado en la escena");
                return;
            }

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            var so = new SerializedObject(manager);
            Transform r = canvas.transform;

            // Header
            SetRef(so, "backButton", FindInPath<Button>(r, "Header/BackButton"));
            SetRef(so, "addFriendIconButton", FindInPath<Button>(r, "Header/AddFriendButton"));

            // Profile Info
            SetRef(so, "usernameText", FindInPath<TextMeshProUGUI>(r, "AvatarCard/UsernameText"));
            SetRef(so, "avatarImage", FindInPath<Image>(r, "AvatarCard/AvatarFrame/AvatarImage"));
            SetRef(so, "avatarUI", FindInPath<DigitPark.UI.Components.AvatarUI>(r, "AvatarCard/AvatarFrame/AvatarImage"));
            SetRef(so, "editAvatarButton", FindInPath<Button>(r, "AvatarCard/AvatarFrame/EditButton"));
            SetRef(so, "statusText", FindInPath<TextMeshProUGUI>(r, "AvatarCard/StatusText"));

            // General Stats
            SetRef(so, "totalGamesText", FindInPath<TextMeshProUGUI>(r, "GeneralStatsCard/TotalGames/Value"));
            SetRef(so, "winsText", FindInPath<TextMeshProUGUI>(r, "GeneralStatsCard/Wins/Value"));
            SetRef(so, "winRateText", FindInPath<TextMeshProUGUI>(r, "GeneralStatsCard/WinRate/Value"));
            SetRef(so, "bestTimeText", FindInPath<TextMeshProUGUI>(r, "GeneralStatsCard/BestTime/Value"));
            SetRef(so, "averageTimeText", FindInPath<TextMeshProUGUI>(r, "GeneralStatsCard/AvgTime/Value"));

            // Game Stats
            SetRef(so, "digitRushValueText", FindInPath<TextMeshProUGUI>(r, "GameStatsCard/DigitRush/Value"));
            SetRef(so, "memoryPairsValueText", FindInPath<TextMeshProUGUI>(r, "GameStatsCard/MemoryPairs/Value"));
            SetRef(so, "quickMathValueText", FindInPath<TextMeshProUGUI>(r, "GameStatsCard/QuickMath/Value"));
            SetRef(so, "flashTapValueText", FindInPath<TextMeshProUGUI>(r, "GameStatsCard/FlashTap/Value"));
            SetRef(so, "oddOneOutValueText", FindInPath<TextMeshProUGUI>(r, "GameStatsCard/OddOneOut/Value"));

            // Action Buttons
            SetRef(so, "friendsButton", FindInPath<Button>(r, "ActionRow/FriendsButton"));
            SetRef(so, "historyButton", FindInPath<Button>(r, "ActionRow/HistoryButton"));

            // CTA
            SetRef(so, "challengeButton", FindInPath<Button>(r, "CTASection/ChallengeButton"));

            // Game Selection Panel
            Transform gsp = r.Find("GameSelectionPanel");
            if (gsp != null) SetRef(so, "gameSelectionPanel", gsp.gameObject);
            SetRef(so, "darkOverlayButton", FindInPath<Button>(r, "GameSelectionPanel/DarkOverlay"));
            SetRef(so, "cancelButton", FindInPath<Button>(r, "GameSelectionPanel/Container/CancelButton"));
            SetRef(so, "digitRushButton", FindInPath<Button>(r, "GameSelectionPanel/Container/GamesList/DigitRushBtn"));
            SetRef(so, "memoryPairsButton", FindInPath<Button>(r, "GameSelectionPanel/Container/GamesList/MemoryPairsBtn"));
            SetRef(so, "quickMathButton", FindInPath<Button>(r, "GameSelectionPanel/Container/GamesList/QuickMathBtn"));
            SetRef(so, "flashTapButton", FindInPath<Button>(r, "GameSelectionPanel/Container/GamesList/FlashTapBtn"));
            SetRef(so, "oddOneOutButton", FindInPath<Button>(r, "GameSelectionPanel/Container/GamesList/OddOneOutBtn"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            Debug.Log("[ProfileUI] Referencias del manager asignadas (28 campos)");
        }

        private static void SetRef(SerializedObject so, string propName, Object value)
        {
            var prop = so.FindProperty(propName);
            if (prop == null) { Debug.LogWarning($"[ProfileUI] Property '{propName}' no encontrada"); return; }
            if (value != null) { prop.objectReferenceValue = value; Debug.Log($"[ProfileUI] Asignado: {propName}"); }
            else { Debug.LogWarning($"[ProfileUI] No se encontro valor para: {propName}"); }
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

        #region Helpers

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
