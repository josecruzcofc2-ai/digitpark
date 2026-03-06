using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// FriendRequests UI Builder - Escena dedicada de Solicitudes de Amistad
    /// Tabs Recibidas/Enviadas, lista de solicitudes con avatar y acciones
    /// Portrait 9:16 (1080x1920), matchWidthOrHeight=0
    ///
    /// Menu: DigitPark/UI Builders/Social/FriendRequests
    /// </summary>
    public class FriendRequestsUIBuilder : EditorWindow
    {
        #region Colors

        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_GLOW = new Color(0f, 0.85f, 1f, 0.8f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.5f, 1f);

        private static readonly Color DARK_BG = new Color(0.02f, 0.04f, 0.08f, 1f);
        private static readonly Color CARD_BG = new Color(0.06f, 0.08f, 0.12f, 1f);
        private static readonly Color CARD_BG_LIGHT = new Color(0.08f, 0.1f, 0.14f, 1f);
        private static readonly Color HEADER_BG = new Color(0.04f, 0.06f, 0.1f, 0.98f);

        private static readonly Color TEXT_WHITE = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.6f, 0.65f, 1f);
        private static readonly Color TEXT_DARK = new Color(0.05f, 0.05f, 0.08f, 1f);

        private static readonly Color GREEN_SUCCESS = new Color(0.2f, 0.9f, 0.4f, 1f);
        private static readonly Color GREEN_DARK = new Color(0.1f, 0.5f, 0.2f, 1f);
        private static readonly Color RED_REJECT = new Color(1f, 0.3f, 0.3f, 1f);
        private static readonly Color RED_DARK = new Color(0.5f, 0.15f, 0.15f, 1f);
        private static readonly Color ORANGE_CANCEL = new Color(1f, 0.5f, 0f, 1f);
        private static readonly Color PURPLE_ACCENT = new Color(0.6f, 0.3f, 1f, 1f);

        private static readonly Color TAB_ACTIVE = new Color(0f, 1f, 1f, 1f);
        private static readonly Color TAB_INACTIVE = new Color(0.15f, 0.17f, 0.22f, 1f);

        #endregion

        #region Layout Anchors (Y: 0=bottom, 1=top)

        private const float HEADER_TOP = 0.985f;
        private const float HEADER_BOT = 0.945f;

        private const float TABS_TOP = 0.935f;
        private const float TABS_BOT = 0.885f;

        private const float CONTENT_TOP = 0.875f;
        private const float CONTENT_BOT = 0.03f;

        private const float SIDE_PAD = 20f;

        #endregion

        #region Prefab

        private const string REQUEST_ITEM_PREFAB_PATH = "Assets/_Project/Prefabs/Social/RequestItem.prefab";
        private const string BACK_BUTTON_PREFAB = "Assets/_Project/Prefabs/Common/BackButton.prefab";

        #endregion

        [MenuItem("DigitPark/UI Builders/Social/FriendRequests", false, 151)]
        public static void ShowWindow()
        {
            GetWindow<FriendRequestsUIBuilder>("FriendRequests Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("FriendRequests UI Builder", EditorStyles.boldLabel);
            GUILayout.Label("Escena dedicada de Solicitudes - Neon Theme", EditorStyles.miniLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Layout completo (de arriba a abajo):\n\n" +
                "1. Header (Back, SOLICITUDES, contador)\n" +
                "2. Tabs (Recibidas | Enviadas)\n" +
                "3. ScrollView (lista de request items)\n" +
                "4. RequestItem Prefab (avatar, info, botones)",
                MessageType.Info);

            GUILayout.Space(15);

            GUI.backgroundColor = CYAN_NEON;
            if (GUILayout.Button("RECONSTRUIR FRIEND REQUESTS COMPLETO", GUILayout.Height(50)))
                RebuildFriendRequests();
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);
            GUILayout.Label("Secciones individuales:", EditorStyles.boldLabel);

            if (GUILayout.Button("1. Header", GUILayout.Height(25))) CreateHeader();
            if (GUILayout.Button("2. Tabs", GUILayout.Height(25))) CreateTabs();
            if (GUILayout.Button("3. ScrollView", GUILayout.Height(25))) CreateScrollView();
            if (GUILayout.Button("4. RequestItem Prefab", GUILayout.Height(25))) CreateRequestItemPrefab();

            GUILayout.Space(15);

            GUI.backgroundColor = new Color(1f, 0.84f, 0f);
            if (GUILayout.Button("ASIGNAR REFERENCIAS AL MANAGER", GUILayout.Height(35)))
                SetupManagerReferences();
            GUI.backgroundColor = Color.white;
        }

        #region Main Rebuild

        private static void RebuildFriendRequests()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("[FriendRequestsUI] No se encontro Canvas");
                return;
            }

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
            CreateTabs();
            CreateScrollView();
            CreateRequestItemPrefab();
            InstantiateSampleCards();
            SetupManagerReferences();

            Debug.Log("[FriendRequestsUI] FriendRequests RECONSTRUIDO exitosamente!");
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
                Debug.LogWarning("[FriendRequestsUI] BackButton prefab not found, using fallback");
            }
            var bRT = GetOrAdd<RectTransform>(backBtnObj);
            bRT.anchorMin = new Vector2(0, 0.5f);
            bRT.anchorMax = new Vector2(0, 0.5f);
            bRT.pivot = new Vector2(0, 0.5f);
            bRT.anchoredPosition = new Vector2(20, 0);
            bRT.sizeDelta = new Vector2(50, 50);

            // Title
            var title = FindOrCreate(header.transform, "TitleText");
            var tRT = GetOrAdd<RectTransform>(title);
            tRT.anchorMin = new Vector2(0.07f, 0f);
            tRT.anchorMax = new Vector2(0.53f, 1f);
            tRT.pivot = new Vector2(0.5f, 0.5f);
            tRT.sizeDelta = Vector2.zero;
            tRT.anchoredPosition = Vector2.zero;
            var tTMP = GetOrAdd<TextMeshProUGUI>(title);
            tTMP.text = "FRIEND REQUESTS";
            tTMP.fontSize = FontSizes.H4;
            tTMP.color = CYAN_NEON;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.MidlineLeft;
            tTMP.enableAutoSizing = true;
            tTMP.fontSizeMin = FontSizes.AutoMinTitle;
            tTMP.fontSizeMax = FontSizes.H4;
            tTMP.overflowMode = TextOverflowModes.Ellipsis;

            // Pending Count
            var count = FindOrCreate(header.transform, "PendingCountText");
            var cRT = GetOrAdd<RectTransform>(count);
            cRT.anchorMin = new Vector2(0.75f, 0);
            cRT.anchorMax = new Vector2(1, 1);
            cRT.offsetMin = Vector2.zero;
            cRT.offsetMax = new Vector2(-15, 0);
            var cTMP = GetOrAdd<TextMeshProUGUI>(count);
            cTMP.text = "2 pending";
            cTMP.fontSize = FontSizes.Body;
            cTMP.fontStyle = FontStyles.Bold;
            cTMP.color = TEXT_SECONDARY;
            cTMP.alignment = TextAlignmentOptions.Right;

            Debug.Log("[FriendRequestsUI] Header creado");
        }

        #endregion

        #region Tabs

        private static void CreateTabs()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var tabsBar = FindOrCreate(canvas.transform, "TabsBar");
            var rt = GetOrAdd<RectTransform>(tabsBar);
            SetAnchorsWithPad(rt, TABS_BOT, TABS_TOP);

            // Clear background
            var barImg = GetOrAdd<Image>(tabsBar);
            barImg.color = Color.clear;

            // HorizontalLayoutGroup
            var hlg = GetOrAdd<HorizontalLayoutGroup>(tabsBar);
            hlg.spacing = 8;
            hlg.padding = new RectOffset(0, 0, 0, 0);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // Received Tab (active by default)
            CreateTabButton(tabsBar.transform, "ReceivedTab", "Received", true);

            // Sent Tab
            CreateTabButton(tabsBar.transform, "SentTab", "Sent", false);

            Debug.Log("[FriendRequestsUI] Tabs creados");
        }

        private static void CreateTabButton(Transform parent, string name, string label, bool active)
        {
            var tab = FindOrCreate(parent, name);

            var bg = GetOrAdd<Image>(tab);
            bg.color = active ? new Color(0f, 1f, 1f, 0.12f) : new Color(0.15f, 0.17f, 0.22f, 0.5f);

            var outline = GetOrAdd<Outline>(tab);
            outline.effectColor = active ? CYAN_DARK : new Color(0.2f, 0.2f, 0.25f, 0.5f);
            outline.effectDistance = new Vector2(1.5f, 1.5f);

            GetOrAdd<Button>(tab).targetGraphic = bg;

            var textGO = FindOrCreate(tab.transform, "Text");
            var tRT = GetOrAdd<RectTransform>(textGO);
            tRT.anchorMin = Vector2.zero;
            tRT.anchorMax = Vector2.one;
            tRT.offsetMin = new Vector2(5, 0);
            tRT.offsetMax = new Vector2(-5, 0);
            var tTMP = GetOrAdd<TextMeshProUGUI>(textGO);
            tTMP.text = label;
            tTMP.fontSize = FontSizes.BodyLarge;
            tTMP.color = active ? Color.white : TEXT_SECONDARY;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.Center;
            tTMP.enableAutoSizing = true;
            tTMP.fontSizeMin = FontSizes.AutoMinBody;
            tTMP.fontSizeMax = FontSizes.BodyLarge;
            tTMP.overflowMode = TextOverflowModes.Ellipsis;

            // Indicator bar (bottom line)
            var indicator = FindOrCreate(tab.transform, "Indicator");
            var iRT = GetOrAdd<RectTransform>(indicator);
            iRT.anchorMin = new Vector2(0.1f, 0);
            iRT.anchorMax = new Vector2(0.9f, 0);
            iRT.pivot = new Vector2(0.5f, 0);
            iRT.anchoredPosition = Vector2.zero;
            iRT.sizeDelta = new Vector2(0, 3);
            var iImg = GetOrAdd<Image>(indicator);
            iImg.color = active ? CYAN_NEON : Color.clear;
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

            // Remove any old Mask on ScrollView (only Viewport should clip)
            var oldSvMask = scrollView.GetComponent<Mask>();
            if (oldSvMask != null) DestroyImmediate(oldSvMask);
            var oldSvImg = scrollView.GetComponent<Image>();
            if (oldSvImg != null) DestroyImmediate(oldSvImg);

            // Viewport (clips scroll content)
            var viewport = FindOrCreate(scrollView.transform, "Viewport");
            var vpRT = GetOrAdd<RectTransform>(viewport);
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.offsetMin = Vector2.zero;
            vpRT.offsetMax = Vector2.zero;
            // Remove old Mask if present, use RectMask2D instead
            var oldVpMask = viewport.GetComponent<Mask>();
            if (oldVpMask != null) DestroyImmediate(oldVpMask);
            var oldVpImg = viewport.GetComponent<Image>();
            if (oldVpImg != null) DestroyImmediate(oldVpImg);
            GetOrAdd<RectMask2D>(viewport);
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
            vlg.childControlHeight = true;
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
            eTMP.text = "You have no pending requests";
            eTMP.fontSize = FontSizes.Subtitle;
            eTMP.color = TEXT_SECONDARY;
            eTMP.alignment = TextAlignmentOptions.Center;
            eTMP.fontStyle = FontStyles.Bold;
            emptyText.SetActive(false);

            // Loading Indicator
            var loading = FindOrCreate(content.transform, "LoadingIndicator");
            var ldRT = GetOrAdd<RectTransform>(loading);
            ldRT.sizeDelta = new Vector2(0, 100);
            GetOrAdd<LayoutElement>(loading).preferredHeight = 100;
            var ldTMP = GetOrAdd<TextMeshProUGUI>(loading);
            ldTMP.text = "Loading...";
            ldTMP.fontSize = FontSizes.Subtitle;
            ldTMP.fontStyle = FontStyles.Bold;
            ldTMP.color = CYAN_NEON;
            ldTMP.alignment = TextAlignmentOptions.Center;
            loading.SetActive(false);

            Debug.Log("[FriendRequestsUI] ScrollView creado");
        }

        #endregion

        #region Sample Cards

        private static void InstantiateSampleCards()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            Transform content = canvas.transform.Find("ScrollView/Viewport/Content");
            if (content == null) { Debug.LogWarning("[FriendRequestsUI] Content no encontrado para sample cards"); return; }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(REQUEST_ITEM_PREFAB_PATH);
            if (prefab == null) { Debug.LogWarning("[FriendRequestsUI] RequestItem prefab no encontrado"); return; }

            // Remove old samples
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                if (content.GetChild(i).name.StartsWith("SampleCard_"))
                    DestroyImmediate(content.GetChild(i).gameObject);
            }

            string[] names = { "CoolPlayer42", "GamerPro", "DigitFan" };
            for (int i = 0; i < 3; i++)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.transform.SetParent(content, false);
                instance.name = $"SampleCard_{i + 1}";

                var userText = instance.GetComponentInChildren<TextMeshProUGUI>();
                if (userText != null) userText.text = names[i];
            }

            Debug.Log("[FriendRequestsUI] 3 sample cards instanciados en Content");
        }

        #endregion

        #region RequestItem Prefab

        private static void CreateRequestItemPrefab()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            Transform content = canvas.transform.Find("ScrollView/Viewport/Content");
            if (content == null)
            {
                Debug.LogWarning("[FriendRequestsUI] ScrollView/Viewport/Content no encontrado. Crea ScrollView primero.");
                return;
            }

            // Create card
            var card = new GameObject("RequestItem_Template");
            card.transform.SetParent(content, false);

            var cardRT = card.AddComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(0, 120);
            card.AddComponent<LayoutElement>().preferredHeight = 120;

            var cardBg = card.AddComponent<Image>();
            cardBg.color = CARD_BG;
            var cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = new Color(CYAN_DARK.r, CYAN_DARK.g, CYAN_DARK.b, 0.35f);
            cardOutline.effectDistance = new Vector2(1.5f, 1.5f);
            var cardShadow = card.AddComponent<Shadow>();
            cardShadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            cardShadow.effectDistance = new Vector2(3, -4);

            // ---- Circular Avatar (left) ----
            Sprite circleSprite = GenerateCircleSprite();

            var avatarFrame = new GameObject("AvatarFrame");
            avatarFrame.transform.SetParent(card.transform, false);
            var afRT = avatarFrame.AddComponent<RectTransform>();
            afRT.anchorMin = new Vector2(0, 0.5f);
            afRT.anchorMax = new Vector2(0, 0.5f);
            afRT.pivot = new Vector2(0, 0.5f);
            afRT.anchoredPosition = new Vector2(12, 0);
            afRT.sizeDelta = new Vector2(60, 60);
            var afImg = avatarFrame.AddComponent<Image>();
            afImg.sprite = circleSprite;
            afImg.color = CYAN_DARK;

            // Circular mask
            var avatarMask = new GameObject("AvatarMask");
            avatarMask.transform.SetParent(avatarFrame.transform, false);
            var amRT = avatarMask.AddComponent<RectTransform>();
            amRT.anchorMin = new Vector2(0.06f, 0.06f);
            amRT.anchorMax = new Vector2(0.94f, 0.94f);
            amRT.offsetMin = Vector2.zero;
            amRT.offsetMax = Vector2.zero;
            var amImg = avatarMask.AddComponent<Image>();
            amImg.sprite = circleSprite;
            amImg.color = CARD_BG_LIGHT;
            avatarMask.AddComponent<Mask>().showMaskGraphic = true;

            // Avatar Image (clipped to circle)
            var avatarImg = new GameObject("AvatarImage");
            avatarImg.transform.SetParent(avatarMask.transform, false);
            var aiRT = avatarImg.AddComponent<RectTransform>();
            aiRT.anchorMin = Vector2.zero;
            aiRT.anchorMax = Vector2.one;
            aiRT.offsetMin = Vector2.zero;
            aiRT.offsetMax = Vector2.zero;
            var aiImg = avatarImg.AddComponent<Image>();
            aiImg.color = Color.white;
            aiImg.preserveAspect = true;
            Sprite defaultAvatar = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/Social/AvatarDefault.png");
            if (defaultAvatar != null) aiImg.sprite = defaultAvatar;

            // ---- Info Section (center) ----
            var infoSection = new GameObject("InfoSection");
            infoSection.transform.SetParent(card.transform, false);
            var isRT = infoSection.AddComponent<RectTransform>();
            isRT.anchorMin = new Vector2(0, 0.5f);
            isRT.anchorMax = new Vector2(0.58f, 0.5f);
            isRT.pivot = new Vector2(0, 0.5f);
            isRT.anchoredPosition = new Vector2(85, 0);
            isRT.sizeDelta = new Vector2(0, 70);

            // Username
            var username = new GameObject("Username");
            username.transform.SetParent(infoSection.transform, false);
            var unRT = username.AddComponent<RectTransform>();
            unRT.anchorMin = new Vector2(0, 0.55f);
            unRT.anchorMax = new Vector2(1, 1);
            unRT.offsetMin = Vector2.zero;
            unRT.offsetMax = Vector2.zero;
            var unTMP = username.AddComponent<TextMeshProUGUI>();
            unTMP.text = "Username";
            unTMP.fontSize = FontSizes.BodySmall;
            unTMP.color = TEXT_WHITE;
            unTMP.fontStyle = FontStyles.Bold;
            unTMP.alignment = TextAlignmentOptions.Left;
            unTMP.enableAutoSizing = true;
            unTMP.fontSizeMin = FontSizes.Caption;
            unTMP.fontSizeMax = FontSizes.BodySmall;
            unTMP.overflowMode = TextOverflowModes.Ellipsis;

            // Timestamp
            var timestamp = new GameObject("TimestampText");
            timestamp.transform.SetParent(infoSection.transform, false);
            var tsRT = timestamp.AddComponent<RectTransform>();
            tsRT.anchorMin = new Vector2(0, 0);
            tsRT.anchorMax = new Vector2(1, 0.50f);
            tsRT.offsetMin = Vector2.zero;
            tsRT.offsetMax = Vector2.zero;
            var tsTMP = timestamp.AddComponent<TextMeshProUGUI>();
            tsTMP.text = "5 min ago";
            tsTMP.fontSize = FontSizes.Caption;
            tsTMP.fontStyle = FontStyles.Bold;
            tsTMP.color = TEXT_SECONDARY;
            tsTMP.alignment = TextAlignmentOptions.Left;

            // ---- Buttons Row (right side) ----
            var buttonsRow = new GameObject("ButtonsRow");
            buttonsRow.transform.SetParent(card.transform, false);
            var brRT = buttonsRow.AddComponent<RectTransform>();
            brRT.anchorMin = new Vector2(0.58f, 0);
            brRT.anchorMax = new Vector2(1, 1);
            brRT.offsetMin = new Vector2(5, 15);
            brRT.offsetMax = new Vector2(-12, -15);

            var brHLG = buttonsRow.AddComponent<HorizontalLayoutGroup>();
            brHLG.spacing = 8;
            brHLG.padding = new RectOffset(0, 0, 0, 0);
            brHLG.childAlignment = TextAnchor.MiddleRight;
            brHLG.childControlWidth = true;
            brHLG.childControlHeight = true;
            brHLG.childForceExpandWidth = true;
            brHLG.childForceExpandHeight = true;

            // Accept Button (for received)
            CreateActionButton(buttonsRow.transform, "AcceptButton", "Accept", GREEN_SUCCESS, TEXT_DARK);

            // Reject Button (for received)
            CreateActionButton(buttonsRow.transform, "RejectButton", "Reject", RED_REJECT, TEXT_WHITE);

            // Cancel Button (for sent - hidden by default)
            var cancelBtn = CreateActionButton(buttonsRow.transform, "CancelButton", "Cancel", CARD_BG_LIGHT, ORANGE_CANCEL);
            cancelBtn.SetActive(false);

            // Save as prefab
            string prefabDir = "Assets/_Project/Prefabs/Social";
            if (!AssetDatabase.IsValidFolder(prefabDir))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Social");
            }

            PrefabUtility.SaveAsPrefabAsset(card, REQUEST_ITEM_PREFAB_PATH);
            Debug.Log($"[FriendRequestsUI] RequestItem prefab guardado en: {REQUEST_ITEM_PREFAB_PATH}");

            DestroyImmediate(card);

            Debug.Log("[FriendRequestsUI] RequestItem Prefab creado");
        }

        private static GameObject CreateActionButton(Transform parent, string name, string label, Color bgColor, Color textColor)
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
            tRT.offsetMin = new Vector2(3, 0);
            tRT.offsetMax = new Vector2(-3, 0);
            var tTMP = text.AddComponent<TextMeshProUGUI>();
            tTMP.text = label;
            tTMP.fontSize = FontSizes.Caption;
            tTMP.color = textColor;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.Center;
            tTMP.enableAutoSizing = true;
            tTMP.fontSizeMin = 20f;
            tTMP.fontSizeMax = FontSizes.Caption;

            return btn;
        }

        #endregion

        #region Manager References

        private static void SetupManagerReferences()
        {
            var manager = Object.FindFirstObjectByType<DigitPark.Managers.FriendRequestsSceneManager>();
            if (manager == null)
            {
                Debug.LogWarning("[FriendRequestsUI] FriendRequestsSceneManager no encontrado en la escena. Agrega el componente primero.");
                return;
            }

            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var so = new SerializedObject(manager);
            Transform r = canvas.transform;

            // Header
            SetRef(so, "backButton", FindInPath<Button>(r, "Header/BackButton"));
            SetRef(so, "titleText", FindInPath<TextMeshProUGUI>(r, "Header/TitleText"));
            SetRef(so, "pendingCountText", FindInPath<TextMeshProUGUI>(r, "Header/PendingCountText"));

            // Tabs
            SetRef(so, "receivedTab", FindInPath<Button>(r, "TabsBar/ReceivedTab"));
            SetRef(so, "sentTab", FindInPath<Button>(r, "TabsBar/SentTab"));
            SetRef(so, "receivedTabBg", FindInPath<Image>(r, "TabsBar/ReceivedTab"));
            SetRef(so, "receivedTabText", FindInPath<TextMeshProUGUI>(r, "TabsBar/ReceivedTab/Text"));
            SetRef(so, "sentTabBg", FindInPath<Image>(r, "TabsBar/SentTab"));
            SetRef(so, "sentTabText", FindInPath<TextMeshProUGUI>(r, "TabsBar/SentTab/Text"));

            // Content
            Transform scrollContent = r.Find("ScrollView/Viewport/Content");
            if (scrollContent != null) SetRef(so, "scrollContent", scrollContent);

            // RequestItem Prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(REQUEST_ITEM_PREFAB_PATH);
            if (prefab != null)
                SetRef(so, "requestItemPrefab", prefab);
            else
                Debug.LogWarning("[FriendRequestsUI] RequestItem prefab no encontrado. Crea el prefab primero.");

            // Empty text and loading
            SetRef(so, "emptyText", FindInPath<TextMeshProUGUI>(r, "ScrollView/Viewport/Content/EmptyText"));
            Transform loading = r.Find("ScrollView/Viewport/Content/LoadingIndicator");
            if (loading != null) SetRef(so, "loadingIndicator", loading.gameObject);

            // Animation sections
            SetRef(so, "headerTransform", FindInPath<RectTransform>(r, "Header"));
            SetRef(so, "tabsBarTransform", FindInPath<RectTransform>(r, "TabsBar"));
            SetRef(so, "scrollViewTransform", FindInPath<RectTransform>(r, "ScrollView"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            Debug.Log("[FriendRequestsUI] Referencias del manager asignadas (17 campos)");
        }

        private static void SetRef(SerializedObject so, string propName, Object value)
        {
            var prop = so.FindProperty(propName);
            if (prop == null) { Debug.LogWarning($"[FriendRequestsUI] Property '{propName}' no encontrada"); return; }
            if (value != null) { prop.objectReferenceValue = value; Debug.Log($"[FriendRequestsUI] Asignado: {propName}"); }
            else { Debug.LogWarning($"[FriendRequestsUI] No se encontro valor para: {propName}"); }
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
    }
}
