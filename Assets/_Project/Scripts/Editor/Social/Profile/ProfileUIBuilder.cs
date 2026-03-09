using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;
using DigitPark.Monetization;

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

        #region Prefab

        private const string BACK_BUTTON_PREFAB = "Assets/_Project/Prefabs/Common/BackButton.prefab";
        private const string ICON_AVATAR_DEFAULT = "Assets/_Project/Art/Icons/Social/AvatarDefault.png";
        private const string ICON_EDIT = "Assets/_Project/Art/Icons/Social/EditIcon.png";

        #endregion

        [MenuItem("DigitPark/Scenes/Build Scene/Social/Profile", false, 155)]
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
            Canvas canvas = FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("[ProfileUI] No se encontró Canvas principal");
                return;
            }

            Debug.Log($"[ProfileUI] Usando Canvas: {canvas.gameObject.name}");

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0f;
            }

            // Full clean of canvas children (keep TransitionCanvas and EventSystem)
            CleanupOldElements(canvas.transform);

            CreateBackground(canvas.transform);
            CreateHeader();
            CreateAvatarCard();
            CreateGeneralStatsCard();
            CreateGameStatsCard();
            CreateActionRow();
            CreateCTASection();
            CreateGameSelectionPanel();
            BuildChangeNamePanel(canvas.transform);
            BuildErrorPanel(canvas.transform);
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
            Canvas canvas = FindMainCanvas();
            if (canvas == null) return;

            var header = FindOrCreate(canvas.transform, "Header");
            var rt = GetOrAdd<RectTransform>(header);
            SetAnchors(rt, 0, HEADER_BOT, 1, HEADER_TOP);
            GetOrAdd<Image>(header).color = HEADER_BG;

            // Back Button - Neon Cyan prefab
            GameObject backBtnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BACK_BUTTON_PREFAB);
            GameObject backBtnObj;
            if (backBtnPrefab != null)
            {
                Transform oldBtn = header.transform.Find("BackButton");
                if (oldBtn != null) DestroyImmediate(oldBtn.gameObject);
                backBtnObj = (GameObject)PrefabUtility.InstantiatePrefab(backBtnPrefab, header.transform);
                backBtnObj.name = "BackButton";
            }
            else
            {
                backBtnObj = FindOrCreate(header.transform, "BackButton");
                GetOrAdd<Image>(backBtnObj).color = new Color(0, 0, 0, 0);
                GetOrAdd<Button>(backBtnObj);
                Debug.LogWarning("[ProfileUI] BackButton prefab not found, using fallback");
            }
            var bRT = GetOrAdd<RectTransform>(backBtnObj);
            bRT.anchorMin = new Vector2(0, 0.5f);
            bRT.anchorMax = new Vector2(0, 0.5f);
            bRT.pivot = new Vector2(0, 0.5f);
            bRT.anchoredPosition = new Vector2(20, 0);
            bRT.sizeDelta = new Vector2(50, 50);

            // Title (center)
            var title = FindOrCreate(header.transform, "TitleText");
            var tRT = GetOrAdd<RectTransform>(title);
            tRT.anchorMin = new Vector2(0.07f, 0f);
            tRT.anchorMax = new Vector2(0.53f, 1f);
            tRT.pivot = new Vector2(0.5f, 0.5f);
            tRT.sizeDelta = Vector2.zero;
            tRT.anchoredPosition = Vector2.zero;
            var tTMP = GetOrAdd<TextMeshProUGUI>(title);
            tTMP.text = "PROFILE";
            tTMP.fontSize = FontSizes.H4;
            tTMP.color = CYAN_NEON;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.MidlineLeft;
            tTMP.raycastTarget = false;
            tTMP.enableAutoSizing = true;
            tTMP.fontSizeMin = FontSizes.AutoMinTitle;
            tTMP.fontSizeMax = FontSizes.H4;
            tTMP.overflowMode = TextOverflowModes.Ellipsis;

            // Add Friend Button (right edge, before pills)
            var addBtn = FindOrCreate(header.transform, "AddFriendButton");
            var aRT = GetOrAdd<RectTransform>(addBtn);
            aRT.anchorMin = new Vector2(1, 0.5f);
            aRT.anchorMax = new Vector2(1, 0.5f);
            aRT.pivot = new Vector2(1, 0.5f);
            aRT.anchoredPosition = new Vector2(-8, 0);
            aRT.sizeDelta = new Vector2(70, 70);

            // Currency pills (between title and addFriend)
            var pills = CurrencyHeaderBarHelper.CreateCurrencyPills(header.transform);
            var pillsRT = pills.GetComponent<RectTransform>();
            pillsRT.anchorMin = new Vector2(0.52f, 0.15f);
            pillsRT.anchorMax = new Vector2(0.88f, 0.85f);
            pillsRT.offsetMin = Vector2.zero;
            pillsRT.offsetMax = Vector2.zero;
            var aBg = GetOrAdd<Image>(addBtn);
            aBg.color = new Color(1, 1, 1, 0.06f);
            GetOrAdd<Button>(addBtn).targetGraphic = aBg;

            var addIcon = FindOrCreate(addBtn.transform, "Icon");
            var aiRT = GetOrAdd<RectTransform>(addIcon);
            aiRT.anchorMin = Vector2.zero;
            aiRT.anchorMax = Vector2.one;
            aiRT.offsetMin = Vector2.zero;
            aiRT.offsetMax = Vector2.zero;
            // Remove TMP if it exists, use Image with sprite instead
            var oldTMP = addIcon.GetComponent<TextMeshProUGUI>();
            if (oldTMP != null) DestroyImmediate(oldTMP);
            var iconImg = GetOrAdd<Image>(addIcon);
            iconImg.preserveAspect = true;
            iconImg.color = GREEN_SUCCESS;
            var addFriendSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/Social/AddFriendIcon.png");
            if (addFriendSprite != null) iconImg.sprite = addFriendSprite;

            Debug.Log("[ProfileUI] Header creado");
        }

        #endregion

        #region Avatar Card

        private static void CreateAvatarCard()
        {
            Canvas canvas = FindMainCanvas();
            if (canvas == null) return;

            var card = FindOrCreate(canvas.transform, "AvatarCard");
            var rt = GetOrAdd<RectTransform>(card);
            SetAnchorsWithPad(rt, AVATAR_BOT, AVATAR_TOP);

            var bg = GetOrAdd<Image>(card);
            bg.color = CARD_BG;
            var outline = GetOrAdd<Outline>(card);
            outline.effectColor = CYAN_DARK;
            outline.effectDistance = new Vector2(2, 2);

            // Generate circle sprite for circular avatar elements
            Sprite circleSprite = GenerateCircleSprite();

            // Avatar Frame container (centered, upper area)
            var frame = FindOrCreate(card.transform, "AvatarFrame");
            var fRT = GetOrAdd<RectTransform>(frame);
            fRT.anchorMin = new Vector2(0.5f, 0.45f);
            fRT.anchorMax = new Vector2(0.5f, 0.45f);
            fRT.pivot = new Vector2(0.5f, 0f);
            fRT.anchoredPosition = Vector2.zero;
            fRT.sizeDelta = new Vector2(225, 225);

            // Circular glow ring (outer, slightly larger) - matchmaking style
            var glowRing = FindOrCreate(frame.transform, "GlowRing");
            var glowRT = GetOrAdd<RectTransform>(glowRing);
            glowRT.anchorMin = Vector2.zero;
            glowRT.anchorMax = Vector2.one;
            glowRT.offsetMin = new Vector2(-8, -8);
            glowRT.offsetMax = new Vector2(8, 8);
            var glowImg = GetOrAdd<Image>(glowRing);
            glowImg.sprite = circleSprite;
            glowImg.color = new Color(CYAN_NEON.r, CYAN_NEON.g, CYAN_NEON.b, 0.25f);

            // Circular border ring (solid cyan frame)
            var borderRing = FindOrCreate(frame.transform, "BorderRing");
            var borderRT = GetOrAdd<RectTransform>(borderRing);
            borderRT.anchorMin = Vector2.zero;
            borderRT.anchorMax = Vector2.one;
            borderRT.offsetMin = Vector2.zero;
            borderRT.offsetMax = Vector2.zero;
            var borderImg = GetOrAdd<Image>(borderRing);
            borderImg.sprite = circleSprite;
            borderImg.color = CYAN_NEON;

            // Circular mask container (clips avatar to circle)
            var avatarMask = FindOrCreate(frame.transform, "AvatarMask");
            var amRT = GetOrAdd<RectTransform>(avatarMask);
            amRT.anchorMin = new Vector2(0.06f, 0.06f);
            amRT.anchorMax = new Vector2(0.94f, 0.94f);
            amRT.offsetMin = Vector2.zero;
            amRT.offsetMax = Vector2.zero;
            var amImg = GetOrAdd<Image>(avatarMask);
            amImg.sprite = circleSprite;
            amImg.color = CARD_BG_LIGHT;
            GetOrAdd<Mask>(avatarMask).showMaskGraphic = true;

            // Avatar Image (inside mask — clipped to circle)
            var avImg = FindOrCreate(avatarMask.transform, "AvatarImage");
            var avRT = GetOrAdd<RectTransform>(avImg);
            avRT.anchorMin = Vector2.zero;
            avRT.anchorMax = Vector2.one;
            avRT.offsetMin = Vector2.zero;
            avRT.offsetMax = Vector2.zero;
            var avImgComp = GetOrAdd<Image>(avImg);
            avImgComp.color = Color.white;
            avImgComp.preserveAspect = true;

            // Set default avatar sprite on Image
            Sprite defaultAvatar = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_AVATAR_DEFAULT);
            if (defaultAvatar != null)
            {
                avImgComp.sprite = defaultAvatar;
            }

            // AvatarUI component
            var avatarUI = GetOrAdd<DigitPark.UI.Components.AvatarUI>(avImg);
            var avatarSO = new SerializedObject(avatarUI);
            avatarSO.FindProperty("loadCurrentUserOnStart").boolValue = true;
            avatarSO.FindProperty("isEditable").boolValue = true;
            avatarSO.FindProperty("avatarImage").objectReferenceValue = avImgComp;
            if (defaultAvatar != null)
            {
                avatarSO.FindProperty("defaultAvatarSprite").objectReferenceValue = defaultAvatar;
            }
            avatarSO.ApplyModifiedProperties();

            // Edit Avatar Button (bottom-right corner of avatar)
            var editBtn = FindOrCreate(frame.transform, "EditButton");
            var eRT = GetOrAdd<RectTransform>(editBtn);
            eRT.anchorMin = new Vector2(1, 0);
            eRT.anchorMax = new Vector2(1, 0);
            eRT.pivot = new Vector2(0.5f, 0.5f);
            eRT.anchoredPosition = new Vector2(-10, 10);
            eRT.sizeDelta = new Vector2(55, 55);
            var eBg = GetOrAdd<Image>(editBtn);
            eBg.color = CARD_BG;
            GetOrAdd<Button>(editBtn).targetGraphic = eBg;

            var editOutline = GetOrAdd<Outline>(editBtn);
            editOutline.effectColor = CYAN_GLOW;
            editOutline.effectDistance = new Vector2(2, 2);

            // Edit icon sprite (instead of TMP text)
            var editIcon = FindOrCreate(editBtn.transform, "Icon");
            var eiRT = GetOrAdd<RectTransform>(editIcon);
            eiRT.anchorMin = Vector2.zero;
            eiRT.anchorMax = Vector2.one;
            eiRT.offsetMin = new Vector2(6, 6);
            eiRT.offsetMax = new Vector2(-6, -6);
            // Remove old TMP if exists from previous build
            var oldTMP = editIcon.GetComponent<TextMeshProUGUI>();
            if (oldTMP != null) DestroyImmediate(oldTMP);
            var eiImg = GetOrAdd<Image>(editIcon);
            eiImg.preserveAspect = true;
            eiImg.raycastTarget = false;
            Sprite editSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_EDIT);
            if (editSprite != null)
            {
                eiImg.sprite = editSprite;
                eiImg.color = Color.white;
            }
            else
            {
                eiImg.color = CYAN_NEON;
                Debug.LogWarning("[ProfileUI] EditIcon.png not found, using fallback color");
            }

            // Username
            var username = FindOrCreate(card.transform, "UsernameText");
            var uRT = GetOrAdd<RectTransform>(username);
            uRT.anchorMin = new Vector2(0.1f, 0.18f);
            uRT.anchorMax = new Vector2(0.9f, 0.42f);
            uRT.offsetMin = Vector2.zero;
            uRT.offsetMax = Vector2.zero;
            var uTMP = GetOrAdd<TextMeshProUGUI>(username);
            uTMP.text = "@Username";
            uTMP.fontSize = FontSizes.Subtitle;
            uTMP.color = TEXT_WHITE;
            uTMP.fontStyle = FontStyles.Bold;
            uTMP.alignment = TextAlignmentOptions.Center;
            uTMP.enableAutoSizing = true;
            uTMP.fontSizeMin = FontSizes.AutoMinBody;
            uTMP.fontSizeMax = FontSizes.Subtitle;
            uTMP.overflowMode = TextOverflowModes.Ellipsis;

            // EditNameButton removed — name change only available in Settings

            // Status Text
            var status = FindOrCreate(card.transform, "StatusText");
            var sRT = GetOrAdd<RectTransform>(status);
            sRT.anchorMin = new Vector2(0.15f, 0.04f);
            sRT.anchorMax = new Vector2(0.85f, 0.18f);
            sRT.offsetMin = Vector2.zero;
            sRT.offsetMax = Vector2.zero;
            var sTMP = GetOrAdd<TextMeshProUGUI>(status);
            sTMP.text = "Your profile";
            sTMP.fontSize = FontSizes.Body;
            sTMP.fontStyle = FontStyles.Bold;
            sTMP.color = CYAN_NEON;
            sTMP.alignment = TextAlignmentOptions.Center;
            sTMP.enableAutoSizing = true;
            sTMP.fontSizeMin = FontSizes.AutoMinBody;
            sTMP.fontSizeMax = FontSizes.Body;
            sTMP.overflowMode = TextOverflowModes.Ellipsis;

            Debug.Log("[ProfileUI] Avatar Card creado");
        }

        #endregion

        #region General Stats Card

        private static void CreateGeneralStatsCard()
        {
            Canvas canvas = FindMainCanvas();
            if (canvas == null) return;

            var card = FindOrCreate(canvas.transform, "GeneralStatsCard");
            var rt = GetOrAdd<RectTransform>(card);
            SetAnchorsWithPad(rt, GENSTATS_BOT, GENSTATS_TOP);

            var bg = GetOrAdd<Image>(card);
            bg.color = CARD_BG;
            var outline = GetOrAdd<Outline>(card);
            outline.effectColor = CYAN_DARK;
            outline.effectDistance = new Vector2(2, 2);

            // Title divider: ═══ ESTADÍSTICAS GENERALES ═══
            var title = FindOrCreate(card.transform, "Title");
            var tRT = GetOrAdd<RectTransform>(title);
            tRT.anchorMin = new Vector2(0, 0.88f);
            tRT.anchorMax = new Vector2(1, 1);
            tRT.offsetMin = new Vector2(10, 0);
            tRT.offsetMax = new Vector2(-10, -5);
            var tHLG = GetOrAdd<HorizontalLayoutGroup>(title);
            tHLG.spacing = 12;
            tHLG.childAlignment = TextAnchor.MiddleCenter;
            tHLG.childForceExpandWidth = true;
            tHLG.childForceExpandHeight = true;
            tHLG.childControlWidth = true;
            tHLG.childControlHeight = true;

            // Remove old TMP if it existed as direct text
            var oldTMP = title.GetComponent<TextMeshProUGUI>();
            if (oldTMP != null) DestroyImmediate(oldTMP);

            // Clear old children
            for (int c = title.transform.childCount - 1; c >= 0; c--)
                DestroyImmediate(title.transform.GetChild(c).gameObject);

            var genLL = new GameObject("LeftLine");
            genLL.transform.SetParent(title.transform, false);
            var genLLle = genLL.AddComponent<LayoutElement>();
            genLLle.flexibleWidth = 1; genLLle.preferredHeight = 2;
            genLL.AddComponent<Image>().color = CYAN_GLOW;

            var genTxt = new GameObject("TitleText");
            genTxt.transform.SetParent(title.transform, false);
            var genTxtLE = genTxt.AddComponent<LayoutElement>();
            genTxtLE.flexibleWidth = 0; genTxtLE.preferredWidth = 620;
            var genTxtTMP = genTxt.AddComponent<TextMeshProUGUI>();
            genTxtTMP.text = "GENERAL STATISTICS";
            genTxtTMP.fontSize = FontSizes.H4;
            genTxtTMP.color = CYAN_NEON;
            genTxtTMP.fontStyle = FontStyles.Bold;
            genTxtTMP.alignment = TextAlignmentOptions.Center;
            genTxtTMP.characterSpacing = 6;
            genTxtTMP.enableAutoSizing = true;
            genTxtTMP.fontSizeMin = FontSizes.AutoMinBody;
            genTxtTMP.fontSizeMax = FontSizes.H4;

            var genRL = new GameObject("RightLine");
            genRL.transform.SetParent(title.transform, false);
            var genRLle = genRL.AddComponent<LayoutElement>();
            genRLle.flexibleWidth = 1; genRLle.preferredHeight = 2;
            genRL.AddComponent<Image>().color = CYAN_GLOW;

            // Top Row: Total Games, Wins, Win Rate
            CreateStatBlock(card.transform, "TotalGames", "0", "Games", CYAN_NEON,
                0.02f, 0.42f, 0.32f, 0.85f);
            CreateStatBlock(card.transform, "Wins", "0", "Wins", GREEN_SUCCESS,
                0.34f, 0.42f, 0.64f, 0.85f);
            CreateStatBlock(card.transform, "WinRate", "0%", "Win Rate", GOLD,
                0.66f, 0.42f, 0.98f, 0.85f);

            // Bottom Row: Best Time, Average Time
            CreateStatBlock(card.transform, "BestTime", "--", "Best Time", ORANGE_ACCENT,
                0.05f, 0.03f, 0.48f, 0.38f);
            CreateStatBlock(card.transform, "AvgTime", "--", "Avg. Time", PURPLE_ACCENT,
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
            vTMP.fontSize = FontSizes.H3;
            vTMP.color = accent;
            vTMP.fontStyle = FontStyles.Bold;
            vTMP.alignment = TextAlignmentOptions.Center;
            vTMP.enableAutoSizing = true;
            vTMP.fontSizeMin = FontSizes.AutoMinBody;
            vTMP.fontSizeMax = FontSizes.H3;

            // Label
            var lbl = FindOrCreate(block.transform, "Label");
            var lRT = GetOrAdd<RectTransform>(lbl);
            lRT.anchorMin = new Vector2(0, 0);
            lRT.anchorMax = new Vector2(1, 0.38f);
            lRT.offsetMin = new Vector2(5, 3);
            lRT.offsetMax = new Vector2(-5, 0);
            var lTMP = GetOrAdd<TextMeshProUGUI>(lbl);
            lTMP.text = label;
            lTMP.fontSize = FontSizes.Body;
            lTMP.fontStyle = FontStyles.Bold;
            lTMP.color = TEXT_SECONDARY;
            lTMP.alignment = TextAlignmentOptions.Center;
            lTMP.enableAutoSizing = true;
            lTMP.fontSizeMin = FontSizes.AutoMinBody;
            lTMP.fontSizeMax = FontSizes.Body;
        }

        #endregion

        #region Game Stats Card

        private static void CreateGameStatsCard()
        {
            Canvas canvas = FindMainCanvas();
            if (canvas == null) return;

            var card = FindOrCreate(canvas.transform, "GameStatsCard");
            var rt = GetOrAdd<RectTransform>(card);
            SetAnchorsWithPad(rt, GAMESTATS_BOT, GAMESTATS_TOP);

            var bg = GetOrAdd<Image>(card);
            bg.color = CARD_BG;
            var outline = GetOrAdd<Outline>(card);
            outline.effectColor = CYAN_DARK;
            outline.effectDistance = new Vector2(2, 2);

            // Title divider: ═══ STATS POR JUEGO ═══
            var title = FindOrCreate(card.transform, "Title");
            var tRT = GetOrAdd<RectTransform>(title);
            tRT.anchorMin = new Vector2(0, 0.88f);
            tRT.anchorMax = new Vector2(1, 1);
            tRT.offsetMin = new Vector2(10, 0);
            tRT.offsetMax = new Vector2(-10, -5);
            var tHLG = GetOrAdd<HorizontalLayoutGroup>(title);
            tHLG.spacing = 12;
            tHLG.childAlignment = TextAnchor.MiddleCenter;
            tHLG.childForceExpandWidth = true;
            tHLG.childForceExpandHeight = true;
            tHLG.childControlWidth = true;
            tHLG.childControlHeight = true;

            // Remove old TMP if it existed as direct text
            var oldTMP2 = title.GetComponent<TextMeshProUGUI>();
            if (oldTMP2 != null) DestroyImmediate(oldTMP2);

            // Clear old children
            for (int c = title.transform.childCount - 1; c >= 0; c--)
                DestroyImmediate(title.transform.GetChild(c).gameObject);

            var gsLL = new GameObject("LeftLine");
            gsLL.transform.SetParent(title.transform, false);
            var gsLLle = gsLL.AddComponent<LayoutElement>();
            gsLLle.flexibleWidth = 1; gsLLle.preferredHeight = 2;
            gsLL.AddComponent<Image>().color = CYAN_GLOW;

            var gsTxt = new GameObject("TitleText");
            gsTxt.transform.SetParent(title.transform, false);
            var gsTxtLE = gsTxt.AddComponent<LayoutElement>();
            gsTxtLE.flexibleWidth = 0; gsTxtLE.preferredWidth = 500;
            var gsTxtTMP = gsTxt.AddComponent<TextMeshProUGUI>();
            gsTxtTMP.text = "STATS BY GAME";
            gsTxtTMP.fontSize = FontSizes.H4;
            gsTxtTMP.color = CYAN_NEON;
            gsTxtTMP.fontStyle = FontStyles.Bold;
            gsTxtTMP.alignment = TextAlignmentOptions.Center;
            gsTxtTMP.characterSpacing = 6;
            gsTxtTMP.enableAutoSizing = true;
            gsTxtTMP.fontSizeMin = FontSizes.AutoMinBody;
            gsTxtTMP.fontSizeMax = FontSizes.H4;

            var gsRL = new GameObject("RightLine");
            gsRL.transform.SetParent(title.transform, false);
            var gsRLle = gsRL.AddComponent<LayoutElement>();
            gsRLle.flexibleWidth = 1; gsRLle.preferredHeight = 2;
            gsRL.AddComponent<Image>().color = CYAN_GLOW;

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
            lRT.anchorMax = new Vector2(0.30f, 1);
            lRT.offsetMin = new Vector2(14, 0);
            lRT.offsetMax = Vector2.zero;
            var lTMP = GetOrAdd<TextMeshProUGUI>(lbl);
            lTMP.text = label;
            lTMP.fontSize = FontSizes.Body;
            lTMP.fontStyle = FontStyles.Bold;
            lTMP.color = TEXT_WHITE;
            lTMP.alignment = TextAlignmentOptions.Left;
            lTMP.enableAutoSizing = true;
            lTMP.fontSizeMin = FontSizes.AutoMinBody;
            lTMP.fontSizeMax = FontSizes.Body;

            // Progress bar background
            var barBG = FindOrCreate(row.transform, "BarBG");
            var barBGRT = GetOrAdd<RectTransform>(barBG);
            barBGRT.anchorMin = new Vector2(0.32f, 0.25f);
            barBGRT.anchorMax = new Vector2(0.70f, 0.75f);
            barBGRT.offsetMin = Vector2.zero;
            barBGRT.offsetMax = Vector2.zero;
            var barBGImg = GetOrAdd<Image>(barBG);
            barBGImg.color = new Color(0.10f, 0.10f, 0.14f, 1f);
            barBGImg.raycastTarget = false;

            // Progress bar fill
            var barFill = FindOrCreate(barBG.transform, "BarFill");
            var barFillRT = GetOrAdd<RectTransform>(barFill);
            barFillRT.anchorMin = Vector2.zero;
            barFillRT.anchorMax = Vector2.one;
            barFillRT.offsetMin = Vector2.zero;
            barFillRT.offsetMax = Vector2.zero;
            var barFillImg = GetOrAdd<Image>(barFill);
            barFillImg.color = accent;
            barFillImg.type = Image.Type.Filled;
            barFillImg.fillMethod = Image.FillMethod.Horizontal;
            barFillImg.fillOrigin = 0; // Left
            barFillImg.fillAmount = 0f;
            barFillImg.raycastTarget = false;

            // Value
            var val = FindOrCreate(row.transform, "Value");
            var vRT = GetOrAdd<RectTransform>(val);
            vRT.anchorMin = new Vector2(0.72f, 0);
            vRT.anchorMax = new Vector2(1, 1);
            vRT.offsetMin = new Vector2(4, 0);
            vRT.offsetMax = new Vector2(0, 0);
            var vTMP = GetOrAdd<TextMeshProUGUI>(val);
            vTMP.text = value;
            vTMP.fontSize = FontSizes.Body;
            vTMP.color = accent;
            vTMP.fontStyle = FontStyles.Bold;
            vTMP.alignment = TextAlignmentOptions.Right;
            vTMP.enableAutoSizing = true;
            vTMP.fontSizeMin = FontSizes.AutoMinBody;
            vTMP.fontSizeMax = FontSizes.Body;

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
            Canvas canvas = FindMainCanvas();
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

            CreateActionButton(row.transform, "FriendsButton", "Friends", CYAN_NEON);
            CreateActionButton(row.transform, "HistoryButton", "History", PURPLE_ACCENT);

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
            tTMP.fontSize = FontSizes.H4;
            tTMP.color = accent;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.Center;
            tTMP.enableAutoSizing = true;
            tTMP.fontSizeMin = FontSizes.AutoMinBody;
            tTMP.fontSizeMax = FontSizes.H4;
            tTMP.overflowMode = TextOverflowModes.Ellipsis;
        }

        #endregion

        #region CTA Section

        private static void CreateCTASection()
        {
            Canvas canvas = FindMainCanvas();
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
            tTMP.text = "CHALLENGE";
            tTMP.fontSize = FontSizes.H2;
            tTMP.color = TEXT_DARK;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.Center;
            tTMP.enableAutoSizing = true;
            tTMP.fontSizeMin = FontSizes.AutoMinBody;
            tTMP.fontSizeMax = FontSizes.H2;
            tTMP.overflowMode = TextOverflowModes.Ellipsis;

            Debug.Log("[ProfileUI] CTA Section creado");
        }

        #endregion

        #region Game Selection Panel (Overlay)

        private static void CreateGameSelectionPanel()
        {
            Canvas canvas = FindMainCanvas();
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
            tTMP.text = "CHOOSE A GAME";
            tTMP.fontSize = FontSizes.Body;
            tTMP.color = CYAN_NEON;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.Center;
            tTMP.enableAutoSizing = true;
            tTMP.fontSizeMin = FontSizes.AutoMinBody;
            tTMP.fontSizeMax = FontSizes.Body;
            tTMP.overflowMode = TextOverflowModes.Ellipsis;

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
            cntTMP.text = "Cancel";
            cntTMP.fontSize = FontSizes.Body;
            cntTMP.fontStyle = FontStyles.Bold;
            cntTMP.color = TEXT_SECONDARY;
            cntTMP.alignment = TextAlignmentOptions.Center;
            cntTMP.enableAutoSizing = true;
            cntTMP.fontSizeMin = FontSizes.AutoMinBody;
            cntTMP.fontSizeMax = FontSizes.Body;
            cntTMP.overflowMode = TextOverflowModes.Ellipsis;

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
            tTMP.fontSize = FontSizes.Body;
            tTMP.color = accent;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.Center;
            tTMP.enableAutoSizing = true;
            tTMP.fontSizeMin = FontSizes.AutoMinBody;
            tTMP.fontSizeMax = FontSizes.Body;
            tTMP.overflowMode = TextOverflowModes.Ellipsis;
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

            Canvas canvas = FindMainCanvas();
            if (canvas == null) return;

            var so = new SerializedObject(manager);
            Transform r = canvas.transform;

            // Header
            SetRef(so, "backButton", FindInPath<Button>(r, "Header/BackButton"));
            SetRef(so, "addFriendIconButton", FindInPath<Button>(r, "Header/AddFriendButton"));

            // Profile Info
            SetRef(so, "usernameText", FindInPath<TextMeshProUGUI>(r, "AvatarCard/UsernameText"));
            SetRef(so, "avatarImage", FindInPath<Image>(r, "AvatarCard/AvatarFrame/AvatarMask/AvatarImage"));
            SetRef(so, "avatarUI", FindInPath<DigitPark.UI.Components.AvatarUI>(r, "AvatarCard/AvatarFrame/AvatarMask/AvatarImage"));
            SetRef(so, "editAvatarButton", FindInPath<Button>(r, "AvatarCard/AvatarFrame/EditButton"));
            SetRef(so, "statusText", FindInPath<TextMeshProUGUI>(r, "AvatarCard/StatusText"));
            SetRef(so, "errorPanel", FindInPath<DigitPark.UI.Panels.ErrorPanelUI>(r, "ErrorPanel"));

            // General Stats
            SetRef(so, "totalGamesText", FindInPath<TextMeshProUGUI>(r, "GeneralStatsCard/TotalGames/Value"));
            SetRef(so, "winsText", FindInPath<TextMeshProUGUI>(r, "GeneralStatsCard/Wins/Value"));
            SetRef(so, "winRateText", FindInPath<TextMeshProUGUI>(r, "GeneralStatsCard/WinRate/Value"));
            SetRef(so, "bestTimeText", FindInPath<TextMeshProUGUI>(r, "GeneralStatsCard/BestTime/Value"));
            SetRef(so, "averageTimeText", FindInPath<TextMeshProUGUI>(r, "GeneralStatsCard/AvgTime/Value"));

            // Game Stats - Values
            SetRef(so, "digitRushValueText", FindInPath<TextMeshProUGUI>(r, "GameStatsCard/DigitRush/Value"));
            SetRef(so, "memoryPairsValueText", FindInPath<TextMeshProUGUI>(r, "GameStatsCard/MemoryPairs/Value"));
            SetRef(so, "quickMathValueText", FindInPath<TextMeshProUGUI>(r, "GameStatsCard/QuickMath/Value"));
            SetRef(so, "flashTapValueText", FindInPath<TextMeshProUGUI>(r, "GameStatsCard/FlashTap/Value"));
            SetRef(so, "oddOneOutValueText", FindInPath<TextMeshProUGUI>(r, "GameStatsCard/OddOneOut/Value"));

            // Game Stats - Bar Fills
            SetRef(so, "digitRushBarFill", FindInPath<Image>(r, "GameStatsCard/DigitRush/BarBG/BarFill"));
            SetRef(so, "memoryPairsBarFill", FindInPath<Image>(r, "GameStatsCard/MemoryPairs/BarBG/BarFill"));
            SetRef(so, "quickMathBarFill", FindInPath<Image>(r, "GameStatsCard/QuickMath/BarBG/BarFill"));
            SetRef(so, "flashTapBarFill", FindInPath<Image>(r, "GameStatsCard/FlashTap/BarBG/BarFill"));
            SetRef(so, "oddOneOutBarFill", FindInPath<Image>(r, "GameStatsCard/OddOneOut/BarBG/BarFill"));

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

        private static Canvas FindMainCanvas()
        {
            return UIBuilderCanvasHelper.FindMainCanvas();
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
            // Fallback: generate at runtime (won't survive prefab save)
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

        #region Change Name & Error Panels

        private static void BuildChangeNamePanel(Transform parent)
        {
            GameObject panelRoot = new GameObject("ChangeNamePanel");
            panelRoot.transform.SetParent(parent, false);
            RectTransform rt = panelRoot.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            // Blocker
            GameObject blocker = new GameObject("BlockerPanel");
            blocker.transform.SetParent(panelRoot.transform, false);
            RectTransform bRT = blocker.AddComponent<RectTransform>();
            bRT.anchorMin = Vector2.zero; bRT.anchorMax = Vector2.one;
            bRT.offsetMin = Vector2.zero; bRT.offsetMax = Vector2.zero;
            blocker.AddComponent<Image>().color = new Color(0, 0, 0, 0.7f);

            // Card
            GameObject card = new GameObject("Panel");
            card.transform.SetParent(panelRoot.transform, false);
            RectTransform cRT = card.AddComponent<RectTransform>();
            cRT.anchorMin = new Vector2(0.08f, 0.3f); cRT.anchorMax = new Vector2(0.92f, 0.7f);
            cRT.offsetMin = Vector2.zero; cRT.offsetMax = Vector2.zero;
            card.AddComponent<Image>().color = CARD_BG;

            // Title
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(card.transform, false);
            RectTransform tRT = titleObj.AddComponent<RectTransform>();
            tRT.anchorMin = new Vector2(0, 0.78f); tRT.anchorMax = Vector2.one;
            tRT.offsetMin = new Vector2(20, 0); tRT.offsetMax = new Vector2(-20, -10);
            var titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
            titleTxt.text = "Change Name";
            titleTxt.fontSize = FontSizes.H4; titleTxt.fontStyle = FontStyles.Bold;
            titleTxt.color = CYAN_NEON; titleTxt.alignment = TextAlignmentOptions.Center;

            // Input Field
            GameObject inputObj = new GameObject("InputField");
            inputObj.transform.SetParent(card.transform, false);
            RectTransform iRT = inputObj.AddComponent<RectTransform>();
            iRT.anchorMin = new Vector2(0.08f, 0.48f); iRT.anchorMax = new Vector2(0.92f, 0.72f);
            iRT.offsetMin = Vector2.zero; iRT.offsetMax = Vector2.zero;
            inputObj.AddComponent<Image>().color = CARD_BG_LIGHT;

            GameObject textArea = new GameObject("Text Area");
            textArea.transform.SetParent(inputObj.transform, false);
            RectTransform taRT = textArea.AddComponent<RectTransform>();
            taRT.anchorMin = Vector2.zero; taRT.anchorMax = Vector2.one;
            taRT.offsetMin = new Vector2(12, 4); taRT.offsetMax = new Vector2(-12, -4);
            textArea.AddComponent<UnityEngine.UI.RectMask2D>();

            GameObject placeholder = new GameObject("Placeholder");
            placeholder.transform.SetParent(textArea.transform, false);
            RectTransform phRT = placeholder.AddComponent<RectTransform>();
            phRT.anchorMin = Vector2.zero; phRT.anchorMax = Vector2.one;
            phRT.offsetMin = Vector2.zero; phRT.offsetMax = Vector2.zero;
            var phTxt = placeholder.AddComponent<TextMeshProUGUI>();
            phTxt.text = "New name...";
            phTxt.fontSize = FontSizes.Body; phTxt.fontStyle = FontStyles.Bold;
            phTxt.color = TEXT_SECONDARY; phTxt.alignment = TextAlignmentOptions.Left;

            GameObject inputText = new GameObject("Text");
            inputText.transform.SetParent(textArea.transform, false);
            RectTransform itRT = inputText.AddComponent<RectTransform>();
            itRT.anchorMin = Vector2.zero; itRT.anchorMax = Vector2.one;
            itRT.offsetMin = Vector2.zero; itRT.offsetMax = Vector2.zero;
            var iTxt = inputText.AddComponent<TextMeshProUGUI>();
            iTxt.fontSize = FontSizes.Body; iTxt.color = TEXT_WHITE;
            iTxt.alignment = TextAlignmentOptions.Left;

            TMP_InputField tmpInput = inputObj.AddComponent<TMP_InputField>();
            tmpInput.textViewport = taRT;
            tmpInput.textComponent = iTxt;
            tmpInput.placeholder = phTxt;
            tmpInput.pointSize = FontSizes.Body;
            tmpInput.characterLimit = 20;

            // Confirm Button
            GameObject confirmObj = new GameObject("ConfirmButton");
            confirmObj.transform.SetParent(card.transform, false);
            RectTransform cfRT = confirmObj.AddComponent<RectTransform>();
            cfRT.anchorMin = new Vector2(0.55f, 0.08f); cfRT.anchorMax = new Vector2(0.92f, 0.35f);
            cfRT.offsetMin = Vector2.zero; cfRT.offsetMax = Vector2.zero;
            Image cfBg = confirmObj.AddComponent<Image>(); cfBg.color = CYAN_NEON;
            Button confirmBtn = confirmObj.AddComponent<Button>(); confirmBtn.targetGraphic = cfBg;

            GameObject cfTxtObj = new GameObject("Text");
            cfTxtObj.transform.SetParent(confirmObj.transform, false);
            RectTransform cfTxtRT = cfTxtObj.AddComponent<RectTransform>();
            cfTxtRT.anchorMin = Vector2.zero; cfTxtRT.anchorMax = Vector2.one;
            cfTxtRT.offsetMin = Vector2.zero; cfTxtRT.offsetMax = Vector2.zero;
            var cfTxt = cfTxtObj.AddComponent<TextMeshProUGUI>();
            cfTxt.text = "Save"; cfTxt.fontSize = FontSizes.Body;
            cfTxt.fontStyle = FontStyles.Bold; cfTxt.color = TEXT_DARK;
            cfTxt.alignment = TextAlignmentOptions.Center;

            // Cancel Button
            GameObject cancelObj = new GameObject("CancelButton");
            cancelObj.transform.SetParent(card.transform, false);
            RectTransform ccRT = cancelObj.AddComponent<RectTransform>();
            ccRT.anchorMin = new Vector2(0.08f, 0.08f); ccRT.anchorMax = new Vector2(0.45f, 0.35f);
            ccRT.offsetMin = Vector2.zero; ccRT.offsetMax = Vector2.zero;
            Image ccBg = cancelObj.AddComponent<Image>(); ccBg.color = CARD_BG_LIGHT;
            Button cancelBtn = cancelObj.AddComponent<Button>(); cancelBtn.targetGraphic = ccBg;

            GameObject ccTxtObj = new GameObject("Text");
            ccTxtObj.transform.SetParent(cancelObj.transform, false);
            RectTransform ccTxtRT = ccTxtObj.AddComponent<RectTransform>();
            ccTxtRT.anchorMin = Vector2.zero; ccTxtRT.anchorMax = Vector2.one;
            ccTxtRT.offsetMin = Vector2.zero; ccTxtRT.offsetMax = Vector2.zero;
            var ccTxt = ccTxtObj.AddComponent<TextMeshProUGUI>();
            ccTxt.text = "Cancel"; ccTxt.fontSize = FontSizes.Body;
            ccTxt.fontStyle = FontStyles.Bold;
            ccTxt.color = TEXT_SECONDARY; ccTxt.alignment = TextAlignmentOptions.Center;

            // Wire InputPanelUI component
            var inputComp = panelRoot.AddComponent(System.Type.GetType("DigitPark.UI.Panels.InputPanelUI, Assembly-CSharp"));
            if (inputComp != null)
            {
                var so = new SerializedObject(inputComp);
                var pp = so.FindProperty("panel"); if (pp != null) pp.objectReferenceValue = card;
                var bp = so.FindProperty("blockerPanel"); if (bp != null) bp.objectReferenceValue = blocker;
                var tp = so.FindProperty("titleText"); if (tp != null) tp.objectReferenceValue = titleTxt;
                var ip = so.FindProperty("inputField"); if (ip != null) ip.objectReferenceValue = tmpInput;
                var cb = so.FindProperty("confirmButton"); if (cb != null) cb.objectReferenceValue = confirmBtn;
                var ccb = so.FindProperty("cancelButton"); if (ccb != null) ccb.objectReferenceValue = cancelBtn;
                var ct = so.FindProperty("confirmButtonText"); if (ct != null) ct.objectReferenceValue = cfTxt;
                var cct = so.FindProperty("cancelButtonText"); if (cct != null) cct.objectReferenceValue = ccTxt;
                so.ApplyModifiedProperties();
            }

            panelRoot.SetActive(false);
            Debug.Log("[ProfileUI] ChangeNamePanel creado");
        }

        private static void BuildErrorPanel(Transform parent)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Common/ErrorPanel.prefab");
            if (prefab != null)
            {
                GameObject errorPanel = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                errorPanel.name = "ErrorPanel";
                RectTransform rect = errorPanel.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0.5f, 0);
                    rect.anchorMax = new Vector2(0.5f, 0);
                    rect.pivot = new Vector2(0.5f, 0);
                    rect.anchoredPosition = new Vector2(0, 200);
                }
                Debug.Log("[ProfileUI] ErrorPanel instantiated from prefab");
            }
            else
            {
                Debug.LogWarning("[ProfileUI] ErrorPanel prefab not found");
            }
        }

        #endregion
    }
}
