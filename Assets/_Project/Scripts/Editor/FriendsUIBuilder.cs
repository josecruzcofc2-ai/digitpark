using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Friends UI Builder - Escena dedicada de Amigos
    /// Lista de amigos con avatar, online status, stats, acciones
    /// Portrait 9:16 (1080x1920), matchWidthOrHeight=0
    ///
    /// Menu: DigitPark/UI Builders/Social/Friends
    /// </summary>
    public class FriendsUIBuilder : EditorWindow
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
        private static readonly Color RED_BADGE = new Color(1f, 0.2f, 0.2f, 1f);

        private static readonly Color INPUT_BG = new Color(0.08f, 0.10f, 0.15f, 1f);

        #endregion

        #region Layout Anchors (Y: 0=bottom, 1=top)

        private const float HEADER_TOP = 0.985f;
        private const float HEADER_BOT = 0.945f;

        private const float SEARCH_TOP = 0.935f;
        private const float SEARCH_BOT = 0.885f;

        private const float REQUESTS_TOP = 0.875f;
        private const float REQUESTS_BOT = 0.815f;

        private const float CONTENT_TOP = 0.805f;
        private const float CONTENT_BOT = 0.03f;

        private const float SIDE_PAD = 20f;

        #endregion

        #region Prefab

        private const string FRIEND_CARD_PREFAB_PATH = "Assets/_Project/Prefabs/Social/FriendCard.prefab";
        private const string BACK_BUTTON_PREFAB = "Assets/_Project/Prefabs/Common/BackButton.prefab";

        #endregion

        [MenuItem("DigitPark/UI Builders/Social/Friends", false, 171)]
        public static void ShowWindow()
        {
            GetWindow<FriendsUIBuilder>("Friends Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Friends UI Builder", EditorStyles.boldLabel);
            GUILayout.Label("Escena dedicada de Amigos - Neon Theme", EditorStyles.miniLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Layout completo (de arriba a abajo):\n\n" +
                "1. Header (Back, AMIGOS, contador)\n" +
                "2. Search Bar (buscar amigos)\n" +
                "3. Requests Nav (ir a solicitudes + badge)\n" +
                "4. ScrollView (lista de friend cards)\n" +
                "5. Friend Card Prefab (avatar, info, botones)",
                MessageType.Info);

            GUILayout.Space(15);

            GUI.backgroundColor = CYAN_NEON;
            if (GUILayout.Button("RECONSTRUIR FRIENDS COMPLETO", GUILayout.Height(50)))
                RebuildFriends();
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);
            GUILayout.Label("Secciones individuales:", EditorStyles.boldLabel);

            if (GUILayout.Button("1. Header", GUILayout.Height(25))) CreateHeader();
            if (GUILayout.Button("2. Search Bar", GUILayout.Height(25))) CreateSearchBar();
            if (GUILayout.Button("3. Requests Nav", GUILayout.Height(25))) CreateRequestsNav();
            if (GUILayout.Button("4. ScrollView", GUILayout.Height(25))) CreateScrollView();
            if (GUILayout.Button("5. Friend Card Prefab", GUILayout.Height(25))) CreateFriendCardPrefab();

            GUILayout.Space(15);

            GUI.backgroundColor = GOLD;
            if (GUILayout.Button("ASIGNAR REFERENCIAS AL MANAGER", GUILayout.Height(35)))
                SetupManagerReferences();
            GUI.backgroundColor = Color.white;
        }

        #region Main Rebuild

        private static void RebuildFriends()
        {
            CleanupOldUI();
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("[FriendsUI] No se encontro Canvas");
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
                "Background", "Header", "SearchBar", "RequestsNav",
                "ScrollView", "ContentPanel", "FriendsPanel"
            };
            foreach (var n in oldNames)
            {
                Transform t = canvas.transform.Find(n);
                if (t != null) DestroyImmediate(t.gameObject);
            }

            CreateBackground(canvas.transform);
            CreateHeader();
            CreateSearchBar();
            CreateRequestsNav();
            CreateScrollView();
            CreateFriendCardPrefab();
            SetupManagerReferences();

            Debug.Log("[FriendsUI] Friends RECONSTRUIDO exitosamente!");
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
                Debug.LogWarning("[FriendsUI] BackButton prefab not found, using fallback");
            }
            var bRT = GetOrAdd<RectTransform>(backBtnObj);
            bRT.anchorMin = new Vector2(0, 0.5f);
            bRT.anchorMax = new Vector2(0, 0.5f);
            bRT.pivot = new Vector2(0, 0.5f);
            bRT.anchoredPosition = new Vector2(15, 0);
            bRT.sizeDelta = new Vector2(50, 50);

            // Title
            var title = FindOrCreate(header.transform, "TitleText");
            var tRT = GetOrAdd<RectTransform>(title);
            tRT.anchorMin = new Vector2(0.12f, 0);
            tRT.anchorMax = new Vector2(0.65f, 1);
            tRT.offsetMin = Vector2.zero;
            tRT.offsetMax = Vector2.zero;
            var tTMP = GetOrAdd<TextMeshProUGUI>(title);
            tTMP.text = "AMIGOS";
            tTMP.fontSize = 78;
            tTMP.color = TEXT_WHITE;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.Left;

            // Friends Count
            var count = FindOrCreate(header.transform, "FriendsCountText");
            var cRT = GetOrAdd<RectTransform>(count);
            cRT.anchorMin = new Vector2(0.65f, 0);
            cRT.anchorMax = new Vector2(1, 1);
            cRT.offsetMin = Vector2.zero;
            cRT.offsetMax = new Vector2(-15, 0);
            var cTMP = GetOrAdd<TextMeshProUGUI>(count);
            cTMP.text = "0 amigos";
            cTMP.fontSize = 40;
            cTMP.color = TEXT_SECONDARY;
            cTMP.alignment = TextAlignmentOptions.Right;

            Debug.Log("[FriendsUI] Header creado");
        }

        #endregion

        #region Search Bar

        private static void CreateSearchBar()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var bar = FindOrCreate(canvas.transform, "SearchBar");
            var rt = GetOrAdd<RectTransform>(bar);
            SetAnchorsWithPad(rt, SEARCH_BOT, SEARCH_TOP);

            var bg = GetOrAdd<Image>(bar);
            bg.color = INPUT_BG;
            var outline = GetOrAdd<Outline>(bar);
            outline.effectColor = CYAN_DARK;
            outline.effectDistance = new Vector2(1, 1);
            var barShadow = bar.AddComponent<Shadow>();
            barShadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            barShadow.effectDistance = new Vector2(3, -4);

            // Input Field
            var inputGO = FindOrCreate(bar.transform, "SearchInput");
            var iRT = GetOrAdd<RectTransform>(inputGO);
            iRT.anchorMin = Vector2.zero;
            iRT.anchorMax = Vector2.one;
            iRT.offsetMin = new Vector2(15, 5);
            iRT.offsetMax = new Vector2(-15, -5);

            var input = GetOrAdd<TMP_InputField>(inputGO);
            input.contentType = TMP_InputField.ContentType.Standard;
            input.characterLimit = 30;

            // Text Area
            var textArea = FindOrCreate(inputGO.transform, "Text Area");
            var taRT = GetOrAdd<RectTransform>(textArea);
            taRT.anchorMin = Vector2.zero;
            taRT.anchorMax = Vector2.one;
            taRT.offsetMin = Vector2.zero;
            taRT.offsetMax = Vector2.zero;
            GetOrAdd<RectMask2D>(textArea);

            // Placeholder
            var placeholder = FindOrCreate(textArea.transform, "Placeholder");
            var phRT = GetOrAdd<RectTransform>(placeholder);
            phRT.anchorMin = Vector2.zero;
            phRT.anchorMax = Vector2.one;
            phRT.offsetMin = Vector2.zero;
            phRT.offsetMax = Vector2.zero;
            var phTMP = GetOrAdd<TextMeshProUGUI>(placeholder);
            phTMP.text = "Buscar amigos...";
            phTMP.fontSize = 45;
            phTMP.color = new Color(0.4f, 0.4f, 0.45f, 1f);
            phTMP.fontStyle = FontStyles.Bold;
            phTMP.alignment = TextAlignmentOptions.Left;

            // Text
            var text = FindOrCreate(textArea.transform, "Text");
            var txtRT = GetOrAdd<RectTransform>(text);
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.offsetMin = Vector2.zero;
            txtRT.offsetMax = Vector2.zero;
            var txtTMP = GetOrAdd<TextMeshProUGUI>(text);
            txtTMP.fontSize = 45;
            txtTMP.color = TEXT_WHITE;
            txtTMP.alignment = TextAlignmentOptions.Left;

            // Wire input field
            input.textViewport = taRT;
            input.textComponent = txtTMP;
            input.placeholder = phTMP;

            Debug.Log("[FriendsUI] Search Bar creado");
        }

        #endregion

        #region Requests Navigation

        private static void CreateRequestsNav()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var nav = FindOrCreate(canvas.transform, "RequestsNav");
            var rt = GetOrAdd<RectTransform>(nav);
            SetAnchorsWithPad(rt, REQUESTS_BOT, REQUESTS_TOP);

            var bg = GetOrAdd<Image>(nav);
            bg.color = CARD_BG;
            var btn = GetOrAdd<Button>(nav);
            btn.targetGraphic = bg;

            var outline = GetOrAdd<Outline>(nav);
            outline.effectColor = PURPLE_ACCENT;
            outline.effectDistance = new Vector2(1.5f, 1.5f);
            var navShadow = nav.AddComponent<Shadow>();
            navShadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            navShadow.effectDistance = new Vector2(3, -4);

            // Label
            var label = FindOrCreate(nav.transform, "Label");
            var lRT = GetOrAdd<RectTransform>(label);
            lRT.anchorMin = new Vector2(0, 0);
            lRT.anchorMax = new Vector2(0.7f, 1);
            lRT.offsetMin = new Vector2(20, 0);
            lRT.offsetMax = Vector2.zero;
            var lTMP = GetOrAdd<TextMeshProUGUI>(label);
            lTMP.text = "Solicitudes de amistad";
            lTMP.fontSize = 45;
            lTMP.color = PURPLE_ACCENT;
            lTMP.fontStyle = FontStyles.Bold;
            lTMP.alignment = TextAlignmentOptions.Left;

            // Badge
            var badge = FindOrCreate(nav.transform, "RequestsBadge");
            var badgeRT = GetOrAdd<RectTransform>(badge);
            badgeRT.anchorMin = new Vector2(1, 0.5f);
            badgeRT.anchorMax = new Vector2(1, 0.5f);
            badgeRT.pivot = new Vector2(1, 0.5f);
            badgeRT.anchoredPosition = new Vector2(-50, 0);
            badgeRT.sizeDelta = new Vector2(75, 75);
            var badgeBg = GetOrAdd<Image>(badge);
            badgeBg.color = RED_BADGE;

            var badgeText = FindOrCreate(badge.transform, "Text");
            var btRT = GetOrAdd<RectTransform>(badgeText);
            btRT.anchorMin = Vector2.zero;
            btRT.anchorMax = Vector2.one;
            btRT.offsetMin = Vector2.zero;
            btRT.offsetMax = Vector2.zero;
            var btTMP = GetOrAdd<TextMeshProUGUI>(badgeText);
            btTMP.text = "3";
            btTMP.fontSize = 35;
            btTMP.color = TEXT_WHITE;
            btTMP.fontStyle = FontStyles.Bold;
            btTMP.alignment = TextAlignmentOptions.Center;

            // Arrow
            var arrow = FindOrCreate(nav.transform, "Arrow");
            var aRT = GetOrAdd<RectTransform>(arrow);
            aRT.anchorMin = new Vector2(1, 0.5f);
            aRT.anchorMax = new Vector2(1, 0.5f);
            aRT.pivot = new Vector2(1, 0.5f);
            aRT.anchoredPosition = new Vector2(-15, 0);
            aRT.sizeDelta = new Vector2(63, 63);
            var aTMP = GetOrAdd<TextMeshProUGUI>(arrow);
            aTMP.text = "\u203A";
            aTMP.fontSize = 75;
            aTMP.color = PURPLE_ACCENT;
            aTMP.fontStyle = FontStyles.Bold;
            aTMP.alignment = TextAlignmentOptions.Center;

            Debug.Log("[FriendsUI] Requests Nav creado");
        }

        #endregion

        #region ScrollView

        private static void CreateScrollView()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var scrollView = FindOrCreate(canvas.transform, "ScrollView");
            var svRT = GetOrAdd<RectTransform>(scrollView);
            SetAnchorsWithPad(svRT, CONTENT_BOT, CONTENT_TOP);

            var scrollRect = GetOrAdd<ScrollRect>(scrollView);
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.scrollSensitivity = 50;

            var svImg = GetOrAdd<Image>(scrollView);
            svImg.color = Color.clear;
            GetOrAdd<Mask>(scrollView).showMaskGraphic = false;

            // Viewport
            var viewport = FindOrCreate(scrollView.transform, "Viewport");
            var vpRT = GetOrAdd<RectTransform>(viewport);
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.offsetMin = Vector2.zero;
            vpRT.offsetMax = Vector2.zero;
            var vpImg = GetOrAdd<Image>(viewport);
            vpImg.color = Color.clear;
            GetOrAdd<Mask>(viewport).showMaskGraphic = false;
            scrollRect.viewport = vpRT;

            // Content
            var content = FindOrCreate(viewport.transform, "Content");
            var cRT = GetOrAdd<RectTransform>(content);
            cRT.anchorMin = new Vector2(0, 1);
            cRT.anchorMax = new Vector2(1, 1);
            cRT.pivot = new Vector2(0.5f, 1);
            cRT.offsetMin = Vector2.zero;
            cRT.offsetMax = Vector2.zero;
            scrollRect.content = cRT;

            var vlg = GetOrAdd<VerticalLayoutGroup>(content);
            vlg.spacing = 10;
            vlg.padding = new RectOffset(0, 0, 5, 20);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = GetOrAdd<ContentSizeFitter>(content);
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Empty Text
            var emptyText = FindOrCreate(content.transform, "EmptyText");
            var eRT = GetOrAdd<RectTransform>(emptyText);
            eRT.sizeDelta = new Vector2(0, 200);
            GetOrAdd<LayoutElement>(emptyText).preferredHeight = 200;
            var eTMP = GetOrAdd<TextMeshProUGUI>(emptyText);
            eTMP.text = "No tienes amigos aun\nBusca jugadores para agregarlos";
            eTMP.fontSize = 45;
            eTMP.color = TEXT_SECONDARY;
            eTMP.alignment = TextAlignmentOptions.Center;
            eTMP.fontStyle = FontStyles.Bold;

            // Loading Indicator
            var loading = FindOrCreate(content.transform, "LoadingIndicator");
            var ldRT = GetOrAdd<RectTransform>(loading);
            ldRT.sizeDelta = new Vector2(0, 100);
            GetOrAdd<LayoutElement>(loading).preferredHeight = 100;
            var ldTMP = GetOrAdd<TextMeshProUGUI>(loading);
            ldTMP.text = "Cargando...";
            ldTMP.fontSize = 45;
            ldTMP.color = CYAN_NEON;
            ldTMP.alignment = TextAlignmentOptions.Center;
            loading.SetActive(false);

            Debug.Log("[FriendsUI] ScrollView creado");
        }

        #endregion

        #region Friend Card Prefab

        private static void CreateFriendCardPrefab()
        {
            // Create template card in scene for preview
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            // Find scroll content
            Transform content = canvas.transform.Find("ScrollView/Viewport/Content");
            if (content == null)
            {
                Debug.LogWarning("[FriendsUI] ScrollView/Viewport/Content no encontrado. Crea ScrollView primero.");
                return;
            }

            // Create card
            var card = new GameObject("FriendCard_Template");
            card.transform.SetParent(content, false);

            var cardRT = card.AddComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(0, 130);
            card.AddComponent<LayoutElement>().preferredHeight = 130;

            var cardBg = card.AddComponent<Image>();
            cardBg.color = CARD_BG;
            var cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = new Color(CYAN_DARK.r, CYAN_DARK.g, CYAN_DARK.b, 0.4f);
            cardOutline.effectDistance = new Vector2(1, 1);
            var cardShadow = card.AddComponent<Shadow>();
            cardShadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            cardShadow.effectDistance = new Vector2(3, -4);

            // ---- Avatar Frame (left) ----
            var avatarFrame = new GameObject("AvatarFrame");
            avatarFrame.transform.SetParent(card.transform, false);
            var afRT = avatarFrame.AddComponent<RectTransform>();
            afRT.anchorMin = new Vector2(0, 0.5f);
            afRT.anchorMax = new Vector2(0, 0.5f);
            afRT.pivot = new Vector2(0, 0.5f);
            afRT.anchoredPosition = new Vector2(12, 0);
            afRT.sizeDelta = new Vector2(70, 70);
            var afImg = avatarFrame.AddComponent<Image>();
            afImg.color = CYAN_DARK;

            // Avatar Image
            var avatarImg = new GameObject("AvatarImage");
            avatarImg.transform.SetParent(avatarFrame.transform, false);
            var aiRT = avatarImg.AddComponent<RectTransform>();
            aiRT.anchorMin = new Vector2(0.06f, 0.06f);
            aiRT.anchorMax = new Vector2(0.94f, 0.94f);
            aiRT.offsetMin = Vector2.zero;
            aiRT.offsetMax = Vector2.zero;
            var aiImg = avatarImg.AddComponent<Image>();
            aiImg.color = CARD_BG_LIGHT;
            aiImg.preserveAspect = true;

            // ---- Online Indicator ----
            var onlineInd = new GameObject("OnlineIndicator");
            onlineInd.transform.SetParent(avatarFrame.transform, false);
            var oiRT = onlineInd.AddComponent<RectTransform>();
            oiRT.anchorMin = new Vector2(1, 0);
            oiRT.anchorMax = new Vector2(1, 0);
            oiRT.pivot = new Vector2(0.5f, 0.5f);
            oiRT.anchoredPosition = new Vector2(-5, 5);
            oiRT.sizeDelta = new Vector2(16, 16);
            var oiImg = onlineInd.AddComponent<Image>();
            oiImg.color = GREEN_SUCCESS;
            var oiOutline = onlineInd.AddComponent<Outline>();
            oiOutline.effectColor = DARK_BG;
            oiOutline.effectDistance = new Vector2(2, 2);

            // ---- Info Section (center) ----
            var infoSection = new GameObject("InfoSection");
            infoSection.transform.SetParent(card.transform, false);
            var isRT = infoSection.AddComponent<RectTransform>();
            isRT.anchorMin = new Vector2(0, 0);
            isRT.anchorMax = new Vector2(1, 1);
            isRT.offsetMin = new Vector2(95, 10);
            isRT.offsetMax = new Vector2(-15, -10);

            // Username
            var username = new GameObject("Username");
            username.transform.SetParent(infoSection.transform, false);
            var unRT = username.AddComponent<RectTransform>();
            unRT.anchorMin = new Vector2(0, 0.65f);
            unRT.anchorMax = new Vector2(0.7f, 1);
            unRT.offsetMin = Vector2.zero;
            unRT.offsetMax = Vector2.zero;
            var unTMP = username.AddComponent<TextMeshProUGUI>();
            unTMP.text = "Username";
            unTMP.fontSize = 50;
            unTMP.color = TEXT_WHITE;
            unTMP.fontStyle = FontStyles.Bold;
            unTMP.alignment = TextAlignmentOptions.Left;

            // Stats Text
            var stats = new GameObject("StatsText");
            stats.transform.SetParent(infoSection.transform, false);
            var stRT = stats.AddComponent<RectTransform>();
            stRT.anchorMin = new Vector2(0, 0.35f);
            stRT.anchorMax = new Vector2(0.7f, 0.63f);
            stRT.offsetMin = Vector2.zero;
            stRT.offsetMax = Vector2.zero;
            var stTMP = stats.AddComponent<TextMeshProUGUI>();
            stTMP.text = "65% WR \u00B7 Digit Rush";
            stTMP.fontSize = 35;
            stTMP.color = TEXT_SECONDARY;
            stTMP.alignment = TextAlignmentOptions.Left;

            // Status Text
            var status = new GameObject("StatusText");
            status.transform.SetParent(infoSection.transform, false);
            var statusRT = status.AddComponent<RectTransform>();
            statusRT.anchorMin = new Vector2(0, 0);
            statusRT.anchorMax = new Vector2(0.5f, 0.33f);
            statusRT.offsetMin = Vector2.zero;
            statusRT.offsetMax = Vector2.zero;
            var statusTMP = status.AddComponent<TextMeshProUGUI>();
            statusTMP.text = "Online";
            statusTMP.fontSize = 33;
            statusTMP.color = GREEN_SUCCESS;
            statusTMP.alignment = TextAlignmentOptions.Left;

            // ---- Buttons Row (right side) ----
            var buttonsRow = new GameObject("ButtonsRow");
            buttonsRow.transform.SetParent(infoSection.transform, false);
            var brRT = buttonsRow.AddComponent<RectTransform>();
            brRT.anchorMin = new Vector2(0.55f, 0);
            brRT.anchorMax = new Vector2(1, 1);
            brRT.offsetMin = Vector2.zero;
            brRT.offsetMax = Vector2.zero;

            var brVLG = buttonsRow.AddComponent<VerticalLayoutGroup>();
            brVLG.spacing = 5;
            brVLG.padding = new RectOffset(0, 0, 8, 8);
            brVLG.childAlignment = TextAnchor.MiddleRight;
            brVLG.childControlWidth = true;
            brVLG.childControlHeight = true;
            brVLG.childForceExpandWidth = true;
            brVLG.childForceExpandHeight = true;

            // Challenge Button
            CreateCardButton(buttonsRow.transform, "ChallengeButton", "Retar", CYAN_NEON, TEXT_DARK);

            // View Profile Button
            CreateCardButton(buttonsRow.transform, "ViewProfileButton", "Perfil", CARD_BG_LIGHT, CYAN_NEON);

            // Remove Button (hidden)
            var removeBtn = CreateCardButton(buttonsRow.transform, "RemoveButton", "Eliminar", CARD_BG_LIGHT, RED_BADGE);
            removeBtn.SetActive(false);

            // Save as prefab
            string prefabDir = "Assets/_Project/Prefabs/Social";
            if (!AssetDatabase.IsValidFolder(prefabDir))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Social");
            }

            PrefabUtility.SaveAsPrefabAsset(card, FRIEND_CARD_PREFAB_PATH);
            Debug.Log($"[FriendsUI] FriendCard prefab guardado en: {FRIEND_CARD_PREFAB_PATH}");

            // Destroy template from scene
            DestroyImmediate(card);

            Debug.Log("[FriendsUI] Friend Card Prefab creado");
        }

        private static GameObject CreateCardButton(Transform parent, string name, string label, Color bgColor, Color textColor)
        {
            var btn = new GameObject(name);
            btn.transform.SetParent(parent, false);

            var bg = btn.AddComponent<Image>();
            bg.color = bgColor;
            btn.AddComponent<Button>().targetGraphic = bg;

            var outline = btn.AddComponent<Outline>();
            outline.effectColor = new Color(textColor.r, textColor.g, textColor.b, 0.3f);
            outline.effectDistance = new Vector2(1, 1);

            var text = new GameObject("Text");
            text.transform.SetParent(btn.transform, false);
            var tRT = text.AddComponent<RectTransform>();
            tRT.anchorMin = Vector2.zero;
            tRT.anchorMax = Vector2.one;
            tRT.offsetMin = new Vector2(5, 0);
            tRT.offsetMax = new Vector2(-5, 0);
            var tTMP = text.AddComponent<TextMeshProUGUI>();
            tTMP.text = label;
            tTMP.fontSize = 35;
            tTMP.color = textColor;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.Center;

            return btn;
        }

        #endregion

        #region Manager References

        private static void SetupManagerReferences()
        {
            var manager = Object.FindFirstObjectByType<DigitPark.Managers.FriendsManager>();
            if (manager == null)
            {
                Debug.LogWarning("[FriendsUI] FriendsManager no encontrado en la escena");
                return;
            }

            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var so = new SerializedObject(manager);
            Transform r = canvas.transform;

            // Header
            SetRef(so, "backButton", FindInPath<Button>(r, "Header/BackButton"));
            SetRef(so, "titleText", FindInPath<TextMeshProUGUI>(r, "Header/TitleText"));
            SetRef(so, "friendsCountText", FindInPath<TextMeshProUGUI>(r, "Header/FriendsCountText"));

            // Search
            SetRef(so, "searchInput", FindInPath<TMP_InputField>(r, "SearchBar/SearchInput"));

            // Requests Nav
            SetRef(so, "requestsButton", FindInPath<Button>(r, "RequestsNav"));
            Transform badge = r.Find("RequestsNav/RequestsBadge");
            if (badge != null) SetRef(so, "requestsBadge", badge.gameObject);
            SetRef(so, "requestsBadgeText", FindInPath<TextMeshProUGUI>(r, "RequestsNav/RequestsBadge/Text"));

            // Content
            Transform scrollContent = r.Find("ScrollView/Viewport/Content");
            if (scrollContent != null) SetRef(so, "scrollContent", scrollContent);

            // Friend Card Prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FRIEND_CARD_PREFAB_PATH);
            if (prefab != null)
                SetRef(so, "friendCardPrefab", prefab);
            else
                Debug.LogWarning("[FriendsUI] FriendCard prefab no encontrado. Crea el prefab primero.");

            // Empty text and loading
            SetRef(so, "emptyText", FindInPath<TextMeshProUGUI>(r, "ScrollView/Viewport/Content/EmptyText"));
            Transform loading = r.Find("ScrollView/Viewport/Content/LoadingIndicator");
            if (loading != null) SetRef(so, "loadingIndicator", loading.gameObject);

            // Animation sections
            SetRef(so, "headerTransform", FindInPath<RectTransform>(r, "Header"));
            SetRef(so, "searchBarTransform", FindInPath<RectTransform>(r, "SearchBar"));
            SetRef(so, "requestsNavTransform", FindInPath<RectTransform>(r, "RequestsNav"));
            SetRef(so, "scrollViewTransform", FindInPath<RectTransform>(r, "ScrollView"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            Debug.Log("[FriendsUI] Referencias del manager asignadas (16 campos)");
        }

        private static void SetRef(SerializedObject so, string propName, Object value)
        {
            var prop = so.FindProperty(propName);
            if (prop == null) { Debug.LogWarning($"[FriendsUI] Property '{propName}' no encontrada"); return; }
            if (value != null) { prop.objectReferenceValue = value; Debug.Log($"[FriendsUI] Asignado: {propName}"); }
            else { Debug.LogWarning($"[FriendsUI] No se encontro valor para: {propName}"); }
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
