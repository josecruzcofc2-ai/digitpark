using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Notifications UI Builder - Escena dedicada de Notificaciones
    /// Lista de notificaciones con tabs de categoría, acciones inline, separadores temporales
    /// Portrait 9:16 (1080x1920), matchWidthOrHeight=0
    ///
    /// Menu: DigitPark/UI Builders/Social/Notifications
    /// </summary>
    public class NotificationsUIBuilder : EditorWindow
    {
        #region Colors

        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_GLOW = new Color(0f, 0.85f, 1f, 0.8f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.5f, 1f);

        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);

        private static readonly Color DARK_BG = new Color(0.02f, 0.04f, 0.08f, 1f);
        private static readonly Color CARD_BG = new Color(0.06f, 0.08f, 0.12f, 1f);
        private static readonly Color CARD_BG_UNREAD = new Color(0.06f, 0.1f, 0.16f, 1f);
        private static readonly Color HEADER_BG = new Color(0.04f, 0.06f, 0.1f, 0.98f);
        private static readonly Color TABS_BG = new Color(0.03f, 0.05f, 0.09f, 1f);
        private static readonly Color FOOTER_BG = new Color(0.03f, 0.05f, 0.09f, 0.95f);

        private static readonly Color TEXT_WHITE = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.6f, 0.65f, 1f);
        private static readonly Color TEXT_DARK = new Color(0.1f, 0.1f, 0.1f, 1f);

        private static readonly Color SOCIAL_COLOR = new Color(0.4f, 0.6f, 1f, 1f);
        private static readonly Color GAMES_COLOR = new Color(1f, 0.5f, 0f, 1f);
        private static readonly Color REWARDS_COLOR = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color RED_BADGE = new Color(1f, 0.2f, 0.2f, 1f);
        private static readonly Color GREEN_SUCCESS = new Color(0.2f, 0.9f, 0.4f, 1f);

        #endregion

        #region Layout Anchors (Y: 0=bottom, 1=top)

        private const float HEADER_TOP = 0.985f;
        private const float HEADER_BOT = 0.945f;

        private const float TABS_TOP = 0.940f;
        private const float TABS_BOT = 0.890f;

        private const float CONTENT_TOP = 0.885f;
        private const float CONTENT_BOT = 0.060f;

        private const float FOOTER_TOP = 0.055f;
        private const float FOOTER_BOT = 0.000f;

        private const float SIDE_PAD = 20f;

        #endregion

        #region Prefab

        private const string NOTIFICATION_CARD_PREFAB_PATH = "Assets/_Project/Prefabs/Social/NotificationCard.prefab";
        private const string BACK_BUTTON_PREFAB = "Assets/_Project/Prefabs/Common/BackButton.prefab";

        #endregion

        [MenuItem("DigitPark/UI Builders/Social/Notifications", false, 172)]
        public static void ShowWindow()
        {
            GetWindow<NotificationsUIBuilder>("Notifications Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Notifications UI Builder", EditorStyles.boldLabel);
            GUILayout.Label("Escena dedicada de Notificaciones - Neon Theme", EditorStyles.miniLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Layout completo (de arriba a abajo):\n\n" +
                "1. Header (Back, NOTIFICACIONES, contador)\n" +
                "2. Tabs (Todas | Social | Juegos | Recompensas)\n" +
                "3. ScrollView (lista de notification cards)\n" +
                "4. Footer (Marcar todas como leídas)\n" +
                "5. NotificationCard Prefab",
                MessageType.Info);

            GUILayout.Space(15);

            GUI.backgroundColor = CYAN_NEON;
            if (GUILayout.Button("RECONSTRUIR NOTIFICATIONS COMPLETO", GUILayout.Height(50)))
                RebuildNotifications();
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);
            GUILayout.Label("Secciones individuales:", EditorStyles.boldLabel);

            if (GUILayout.Button("1. Header", GUILayout.Height(25))) CreateHeader();
            if (GUILayout.Button("2. Tabs", GUILayout.Height(25))) CreateTabs();
            if (GUILayout.Button("3. ScrollView", GUILayout.Height(25))) CreateScrollView();
            if (GUILayout.Button("4. Footer", GUILayout.Height(25))) CreateFooter();
            if (GUILayout.Button("5. NotificationCard Prefab", GUILayout.Height(25))) CreateNotificationCardPrefab();

            GUILayout.Space(15);

            GUI.backgroundColor = GOLD;
            if (GUILayout.Button("ASIGNAR REFERENCIAS AL MANAGER", GUILayout.Height(35)))
                SetupManagerReferences();
            GUI.backgroundColor = Color.white;
        }

        #region Main Rebuild

        private static void RebuildNotifications()
        {
            CleanupOldUI();
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("[NotificationsUI] No se encontro Canvas");
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
                "Background", "Header", "Tabs", "ScrollView",
                "Footer", "ContentPanel"
            };
            foreach (var n in oldNames)
            {
                Transform t = canvas.transform.Find(n);
                if (t != null) DestroyImmediate(t.gameObject);
            }

            CreateBackground(canvas.transform);
            CreateHeader();
            CreateTabs();
            CreateScrollView();
            CreateFooter();
            CreateNotificationCardPrefab();
            SetupManagerReferences();

            Debug.Log("[NotificationsUI] Notifications RECONSTRUIDO exitosamente!");
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
                Debug.LogWarning("[NotificationsUI] BackButton prefab not found, using fallback");
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
            tRT.anchorMax = new Vector2(0.70f, 1);
            tRT.offsetMin = Vector2.zero;
            tRT.offsetMax = Vector2.zero;
            var tTMP = GetOrAdd<TextMeshProUGUI>(title);
            tTMP.text = "NOTIFICACIONES";
            tTMP.fontSize = 78;
            tTMP.color = TEXT_WHITE;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.Left;

            // Count Text
            var count = FindOrCreate(header.transform, "CountText");
            var cRT = GetOrAdd<RectTransform>(count);
            cRT.anchorMin = new Vector2(0.65f, 0);
            cRT.anchorMax = new Vector2(1, 1);
            cRT.offsetMin = Vector2.zero;
            cRT.offsetMax = new Vector2(-15, 0);
            var cTMP = GetOrAdd<TextMeshProUGUI>(count);
            cTMP.text = "0 sin leer";
            cTMP.fontSize = 35;
            cTMP.color = TEXT_SECONDARY;
            cTMP.alignment = TextAlignmentOptions.Right;

            Debug.Log("[NotificationsUI] Header creado");
        }

        #endregion

        #region Tabs

        private static void CreateTabs()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var tabs = FindOrCreate(canvas.transform, "Tabs");
            var rt = GetOrAdd<RectTransform>(tabs);
            SetAnchorsWithPad(rt, TABS_BOT, TABS_TOP);
            GetOrAdd<Image>(tabs).color = TABS_BG;

            var outline = GetOrAdd<Outline>(tabs);
            outline.effectColor = new Color(CYAN_DARK.r, CYAN_DARK.g, CYAN_DARK.b, 0.3f);
            outline.effectDistance = new Vector2(0, -1);

            // HorizontalLayoutGroup for tabs
            var hlg = GetOrAdd<HorizontalLayoutGroup>(tabs);
            hlg.spacing = 0;
            hlg.padding = new RectOffset(5, 5, 5, 5);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // Create individual tabs
            CreateTab(tabs.transform, "TabAll", "Todas", CYAN_NEON, true);
            CreateTab(tabs.transform, "TabSocial", "Social", SOCIAL_COLOR, false);
            CreateTab(tabs.transform, "TabGames", "Juegos", GAMES_COLOR, false);
            CreateTab(tabs.transform, "TabRewards", "$", REWARDS_COLOR, false);

            Debug.Log("[NotificationsUI] Tabs creados");
        }

        private static void CreateTab(Transform parent, string name, string label, Color accentColor, bool active)
        {
            var tab = FindOrCreate(parent, name);
            var tabBg = GetOrAdd<Image>(tab);
            tabBg.color = new Color(1, 1, 1, 0.04f);
            var btn = GetOrAdd<Button>(tab);
            btn.targetGraphic = tabBg;

            // Tab label
            var labelGO = FindOrCreate(tab.transform, "Label");
            var lRT = GetOrAdd<RectTransform>(labelGO);
            lRT.anchorMin = new Vector2(0, 0.25f);
            lRT.anchorMax = new Vector2(1, 1);
            lRT.offsetMin = Vector2.zero;
            lRT.offsetMax = Vector2.zero;
            var lTMP = GetOrAdd<TextMeshProUGUI>(labelGO);
            lTMP.text = label;
            lTMP.fontSize = 40;
            lTMP.color = active ? accentColor : TEXT_SECONDARY;
            lTMP.fontStyle = FontStyles.Bold;
            lTMP.alignment = TextAlignmentOptions.Center;

            // Tab indicator bar (bottom line)
            var indicator = FindOrCreate(tab.transform, "Indicator");
            var iRT = GetOrAdd<RectTransform>(indicator);
            iRT.anchorMin = new Vector2(0.15f, 0);
            iRT.anchorMax = new Vector2(0.85f, 0);
            iRT.pivot = new Vector2(0.5f, 0);
            iRT.anchoredPosition = new Vector2(0, 2);
            iRT.sizeDelta = new Vector2(0, 3);
            var iImg = GetOrAdd<Image>(indicator);
            iImg.color = active ? accentColor : Color.clear;
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
            vlg.spacing = 8;
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
            eRT.sizeDelta = new Vector2(0, 300);
            GetOrAdd<LayoutElement>(emptyText).preferredHeight = 300;
            var eTMP = GetOrAdd<TextMeshProUGUI>(emptyText);
            eTMP.text = "No tienes notificaciones\nTe avisaremos cuando algo importante suceda";
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

            Debug.Log("[NotificationsUI] ScrollView creado");
        }

        #endregion

        #region Footer

        private static void CreateFooter()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var footer = FindOrCreate(canvas.transform, "Footer");
            var rt = GetOrAdd<RectTransform>(footer);
            SetAnchors(rt, 0, FOOTER_BOT, 1, FOOTER_TOP);
            GetOrAdd<Image>(footer).color = FOOTER_BG;

            // Top border line
            var topLine = FindOrCreate(footer.transform, "TopLine");
            var tlRT = GetOrAdd<RectTransform>(topLine);
            tlRT.anchorMin = new Vector2(0, 1);
            tlRT.anchorMax = new Vector2(1, 1);
            tlRT.pivot = new Vector2(0.5f, 1);
            tlRT.anchoredPosition = Vector2.zero;
            tlRT.sizeDelta = new Vector2(0, 1);
            GetOrAdd<Image>(topLine).color = new Color(CYAN_DARK.r, CYAN_DARK.g, CYAN_DARK.b, 0.3f);

            // Mark All Read Button
            var markAllBtn = FindOrCreate(footer.transform, "MarkAllReadButton");
            var maRT = GetOrAdd<RectTransform>(markAllBtn);
            maRT.anchorMin = Vector2.zero;
            maRT.anchorMax = Vector2.one;
            maRT.offsetMin = new Vector2(20, 8);
            maRT.offsetMax = new Vector2(-20, -8);
            var maBg = GetOrAdd<Image>(markAllBtn);
            maBg.color = new Color(1, 1, 1, 0.04f);
            var maBtn = GetOrAdd<Button>(markAllBtn);
            maBtn.targetGraphic = maBg;

            var maOutline = GetOrAdd<Outline>(markAllBtn);
            maOutline.effectColor = new Color(CYAN_DARK.r, CYAN_DARK.g, CYAN_DARK.b, 0.4f);
            maOutline.effectDistance = new Vector2(1, 1);

            // Button text
            var maText = FindOrCreate(markAllBtn.transform, "Text");
            var matRT = GetOrAdd<RectTransform>(maText);
            matRT.anchorMin = Vector2.zero;
            matRT.anchorMax = Vector2.one;
            matRT.offsetMin = Vector2.zero;
            matRT.offsetMax = Vector2.zero;
            var matTMP = GetOrAdd<TextMeshProUGUI>(maText);
            matTMP.text = "Marcar todas como leídas";
            matTMP.fontSize = 40;
            matTMP.color = CYAN_NEON;
            matTMP.fontStyle = FontStyles.Bold;
            matTMP.alignment = TextAlignmentOptions.Center;

            Debug.Log("[NotificationsUI] Footer creado");
        }

        #endregion

        #region NotificationCard Prefab

        private static void CreateNotificationCardPrefab()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            // Create card
            var card = new GameObject("NotificationCard_Template");
            card.transform.SetParent(canvas.transform, false);

            var cardRT = card.AddComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(0, 120);
            card.AddComponent<LayoutElement>().preferredHeight = 120;

            var cardBg = card.AddComponent<Image>();
            cardBg.color = CARD_BG_UNREAD;
            var cardBtn = card.AddComponent<Button>();
            cardBtn.targetGraphic = cardBg;

            var cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = new Color(CYAN_DARK.r, CYAN_DARK.g, CYAN_DARK.b, 0.3f);
            cardOutline.effectDistance = new Vector2(1, 1);

            // ---- Unread Dot (left edge) ----
            var unreadDot = new GameObject("UnreadDot");
            unreadDot.transform.SetParent(card.transform, false);
            var udRT = unreadDot.AddComponent<RectTransform>();
            udRT.anchorMin = new Vector2(0, 0.5f);
            udRT.anchorMax = new Vector2(0, 0.5f);
            udRT.pivot = new Vector2(0, 0.5f);
            udRT.anchoredPosition = new Vector2(6, 0);
            udRT.sizeDelta = new Vector2(6, 6);
            var udImg = unreadDot.AddComponent<Image>();
            udImg.color = CYAN_NEON;

            // ---- Type Icon (left) ----
            var typeIcon = new GameObject("TypeIcon");
            typeIcon.transform.SetParent(card.transform, false);
            var tiRT = typeIcon.AddComponent<RectTransform>();
            tiRT.anchorMin = new Vector2(0, 0.5f);
            tiRT.anchorMax = new Vector2(0, 0.5f);
            tiRT.pivot = new Vector2(0, 0.5f);
            tiRT.anchoredPosition = new Vector2(20, 0);
            tiRT.sizeDelta = new Vector2(45, 45);
            // Icon background
            var tiBg = typeIcon.AddComponent<Image>();
            tiBg.color = new Color(1, 1, 1, 0.06f);
            // Icon text (emoji)
            var tiTextGO = new GameObject("IconText");
            tiTextGO.transform.SetParent(typeIcon.transform, false);
            var titRT = tiTextGO.AddComponent<RectTransform>();
            titRT.anchorMin = Vector2.zero;
            titRT.anchorMax = Vector2.one;
            titRT.offsetMin = Vector2.zero;
            titRT.offsetMax = Vector2.zero;
            var tiTMP = tiTextGO.AddComponent<TextMeshProUGUI>();
            tiTMP.text = "🔔";
            tiTMP.fontSize = 55;
            tiTMP.color = CYAN_NEON;
            tiTMP.alignment = TextAlignmentOptions.Center;

            // ---- Info Section (center) ----
            var infoSection = new GameObject("InfoSection");
            infoSection.transform.SetParent(card.transform, false);
            var isRT = infoSection.AddComponent<RectTransform>();
            isRT.anchorMin = new Vector2(0, 0);
            isRT.anchorMax = new Vector2(1, 1);
            isRT.offsetMin = new Vector2(75, 8);
            isRT.offsetMax = new Vector2(-10, -8);

            // Title
            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(infoSection.transform, false);
            var titleRT = titleGO.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0.65f);
            titleRT.anchorMax = new Vector2(0.75f, 1);
            titleRT.offsetMin = Vector2.zero;
            titleRT.offsetMax = Vector2.zero;
            var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "Título de notificación";
            titleTMP.fontSize = 43;
            titleTMP.color = TEXT_WHITE;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Left;
            titleTMP.overflowMode = TextOverflowModes.Ellipsis;
            titleTMP.maxVisibleLines = 1;

            // Timestamp (top right)
            var timestamp = new GameObject("Timestamp");
            timestamp.transform.SetParent(infoSection.transform, false);
            var tsRT = timestamp.AddComponent<RectTransform>();
            tsRT.anchorMin = new Vector2(0.75f, 0.70f);
            tsRT.anchorMax = new Vector2(1, 1);
            tsRT.offsetMin = Vector2.zero;
            tsRT.offsetMax = Vector2.zero;
            var tsTMP = timestamp.AddComponent<TextMeshProUGUI>();
            tsTMP.text = "Hace 5 min";
            tsTMP.fontSize = 30;
            tsTMP.color = TEXT_SECONDARY;
            tsTMP.alignment = TextAlignmentOptions.Right;

            // Body
            var bodyGO = new GameObject("Body");
            bodyGO.transform.SetParent(infoSection.transform, false);
            var bodyRT = bodyGO.AddComponent<RectTransform>();
            bodyRT.anchorMin = new Vector2(0, 0.35f);
            bodyRT.anchorMax = new Vector2(1, 0.68f);
            bodyRT.offsetMin = Vector2.zero;
            bodyRT.offsetMax = Vector2.zero;
            var bodyTMP = bodyGO.AddComponent<TextMeshProUGUI>();
            bodyTMP.text = "Descripción de la notificación";
            bodyTMP.fontSize = 35;
            bodyTMP.color = TEXT_SECONDARY;
            bodyTMP.alignment = TextAlignmentOptions.Left;
            bodyTMP.overflowMode = TextOverflowModes.Ellipsis;
            bodyTMP.maxVisibleLines = 1;

            // Sender Name (below body, subtle)
            var senderGO = new GameObject("SenderName");
            senderGO.transform.SetParent(infoSection.transform, false);
            var snRT = senderGO.AddComponent<RectTransform>();
            snRT.anchorMin = new Vector2(0, 0.15f);
            snRT.anchorMax = new Vector2(0.4f, 0.37f);
            snRT.offsetMin = Vector2.zero;
            snRT.offsetMax = Vector2.zero;
            var snTMP = senderGO.AddComponent<TextMeshProUGUI>();
            snTMP.text = "de: Player123";
            snTMP.fontSize = 30;
            snTMP.color = SOCIAL_COLOR;
            snTMP.fontStyle = FontStyles.Bold;
            snTMP.alignment = TextAlignmentOptions.Left;

            // ---- Actions Row (bottom right) ----
            var actionsRow = new GameObject("ActionsRow");
            actionsRow.transform.SetParent(infoSection.transform, false);
            var arRT = actionsRow.AddComponent<RectTransform>();
            arRT.anchorMin = new Vector2(0.4f, 0);
            arRT.anchorMax = new Vector2(1, 0.37f);
            arRT.offsetMin = Vector2.zero;
            arRT.offsetMax = Vector2.zero;

            var arHLG = actionsRow.AddComponent<HorizontalLayoutGroup>();
            arHLG.spacing = 8;
            arHLG.childAlignment = TextAnchor.MiddleRight;
            arHLG.childControlWidth = true;
            arHLG.childControlHeight = true;
            arHLG.childForceExpandWidth = true;
            arHLG.childForceExpandHeight = true;

            // Primary Action Button
            CreateActionButton(actionsRow.transform, "PrimaryButton", "Aceptar", CYAN_NEON, TEXT_DARK);

            // Secondary Action Button
            CreateActionButton(actionsRow.transform, "SecondaryButton", "Rechazar", CARD_BG, TEXT_SECONDARY);

            // Save as prefab
            string prefabDir = "Assets/_Project/Prefabs/Social";
            if (!AssetDatabase.IsValidFolder(prefabDir))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Social");
            }

            PrefabUtility.SaveAsPrefabAsset(card, NOTIFICATION_CARD_PREFAB_PATH);
            Debug.Log($"[NotificationsUI] NotificationCard prefab guardado en: {NOTIFICATION_CARD_PREFAB_PATH}");

            // Destroy template from scene
            DestroyImmediate(card);

            Debug.Log("[NotificationsUI] NotificationCard Prefab creado");
        }

        private static GameObject CreateActionButton(Transform parent, string name, string label, Color bgColor, Color textColor)
        {
            var btn = new GameObject(name);
            btn.transform.SetParent(parent, false);

            var bg = btn.AddComponent<Image>();
            bg.color = bgColor;
            btn.AddComponent<Button>().targetGraphic = bg;

            var btnOutline = btn.AddComponent<Outline>();
            btnOutline.effectColor = new Color(textColor.r, textColor.g, textColor.b, 0.3f);
            btnOutline.effectDistance = new Vector2(1, 1);

            var text = new GameObject("Text");
            text.transform.SetParent(btn.transform, false);
            var tRT = text.AddComponent<RectTransform>();
            tRT.anchorMin = Vector2.zero;
            tRT.anchorMax = Vector2.one;
            tRT.offsetMin = new Vector2(4, 0);
            tRT.offsetMax = new Vector2(-4, 0);
            var tTMP = text.AddComponent<TextMeshProUGUI>();
            tTMP.text = label;
            tTMP.fontSize = 33;
            tTMP.color = textColor;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.Center;

            return btn;
        }

        #endregion

        #region Manager References

        private static void SetupManagerReferences()
        {
            var manager = Object.FindFirstObjectByType<DigitPark.Managers.NotificationsManager>();
            if (manager == null)
            {
                Debug.LogWarning("[NotificationsUI] NotificationsManager no encontrado en la escena");
                return;
            }

            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var so = new SerializedObject(manager);
            Transform r = canvas.transform;

            // Header
            SetRef(so, "backButton", FindInPath<Button>(r, "Header/BackButton"));
            SetRef(so, "titleText", FindInPath<TextMeshProUGUI>(r, "Header/TitleText"));
            SetRef(so, "countText", FindInPath<TextMeshProUGUI>(r, "Header/CountText"));

            // Tabs
            SetRef(so, "tabAll", FindInPath<Button>(r, "Tabs/TabAll"));
            SetRef(so, "tabSocial", FindInPath<Button>(r, "Tabs/TabSocial"));
            SetRef(so, "tabGames", FindInPath<Button>(r, "Tabs/TabGames"));
            SetRef(so, "tabRewards", FindInPath<Button>(r, "Tabs/TabRewards"));
            SetRef(so, "tabAllIndicator", FindInPath<Image>(r, "Tabs/TabAll/Indicator"));
            SetRef(so, "tabSocialIndicator", FindInPath<Image>(r, "Tabs/TabSocial/Indicator"));
            SetRef(so, "tabGamesIndicator", FindInPath<Image>(r, "Tabs/TabGames/Indicator"));
            SetRef(so, "tabRewardsIndicator", FindInPath<Image>(r, "Tabs/TabRewards/Indicator"));

            // Content
            Transform scrollContent = r.Find("ScrollView/Viewport/Content");
            if (scrollContent != null) SetRef(so, "scrollContent", scrollContent);

            // NotificationCard Prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(NOTIFICATION_CARD_PREFAB_PATH);
            if (prefab != null)
                SetRef(so, "notificationCardPrefab", prefab);
            else
                Debug.LogWarning("[NotificationsUI] NotificationCard prefab no encontrado. Crea el prefab primero.");

            // Empty text and loading
            SetRef(so, "emptyText", FindInPath<TextMeshProUGUI>(r, "ScrollView/Viewport/Content/EmptyText"));
            Transform loading = r.Find("ScrollView/Viewport/Content/LoadingIndicator");
            if (loading != null) SetRef(so, "loadingIndicator", loading.gameObject);

            // Footer
            SetRef(so, "markAllReadButton", FindInPath<Button>(r, "Footer/MarkAllReadButton"));
            SetRef(so, "markAllReadText", FindInPath<TextMeshProUGUI>(r, "Footer/MarkAllReadButton/Text"));

            // Animation sections
            SetRef(so, "headerTransform", FindInPath<RectTransform>(r, "Header"));
            SetRef(so, "tabsTransform", FindInPath<RectTransform>(r, "Tabs"));
            SetRef(so, "scrollViewTransform", FindInPath<RectTransform>(r, "ScrollView"));
            SetRef(so, "footerTransform", FindInPath<RectTransform>(r, "Footer"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            Debug.Log("[NotificationsUI] Referencias del manager asignadas (20 campos)");
        }

        private static void SetRef(SerializedObject so, string propName, Object value)
        {
            var prop = so.FindProperty(propName);
            if (prop == null) { Debug.LogWarning($"[NotificationsUI] Property '{propName}' no encontrada"); return; }
            if (value != null) { prop.objectReferenceValue = value; Debug.Log($"[NotificationsUI] Asignado: {propName}"); }
            else { Debug.LogWarning($"[NotificationsUI] No se encontro valor para: {propName}"); }
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
