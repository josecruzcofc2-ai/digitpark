using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para reconstruir la UI de Scores/Rankings con diseño profesional neón
    /// Resolución: Portrait 9:16 (1080x1920)
    /// </summary>
    public class ScoresUIBuilder : EditorWindow
    {
        // Colores del tema neón
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.5f, 1f);
        private static readonly Color CYAN_GLOW = new Color(0f, 0.83f, 1f, 1f);
        private static readonly Color DARK_BG = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color PANEL_BG = new Color(0.05f, 0.1f, 0.15f, 0.95f);
        private static readonly Color TAB_INACTIVE = new Color(0.15f, 0.2f, 0.25f, 1f);
        private static readonly Color TAB_ACTIVE = new Color(0f, 0.83f, 1f, 1f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color DIVIDER_COLOR = new Color(0f, 0.6f, 0.7f, 0.5f);

        [MenuItem("DigitPark/Rebuild Scores UI")]
        public static void ShowWindow()
        {
            GetWindow<ScoresUIBuilder>("Scores UI Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Scores/Rankings UI Builder", EditorStyles.boldLabel);
            GUILayout.Label("Resolución: Portrait 9:16 (1080x1920)", EditorStyles.miniLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Este script reconstruirá la UI de Scores/Rankings.\n" +
                "Asegúrate de tener la escena Scores abierta.\n\n" +
                "Elementos que se crearán:\n" +
                "• Header con título RANKINGS\n" +
                "• Tabs Nacional/Mundial mejorados\n" +
                "• Estado vacío atractivo\n" +
                "• Sección Tu Posición",
                MessageType.Info);

            GUILayout.Space(10);

            if (GUILayout.Button("Reconstruir Scores UI", GUILayout.Height(40)))
            {
                RebuildScoresUI();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Solo crear Estado Vacío", GUILayout.Height(30)))
            {
                CreateEmptyStateOnly();
            }
        }

        private static void RebuildScoresUI()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[ScoresUI] No se encontró Canvas en la escena");
                return;
            }

            Transform canvasTransform = canvas.transform;

            // Buscar o crear Background
            Transform background = canvasTransform.Find("Background");
            if (background == null)
            {
                CreateBackground(canvasTransform);
            }

            // Limpiar elementos viejos
            CleanOldElements(canvasTransform);

            // Crear nueva estructura
            CreateScoresLayout(canvasTransform);

            Debug.Log("[ScoresUI] Scores UI reconstruida exitosamente!");
            EditorUtility.SetDirty(canvas.gameObject);
        }

        private static void CreateEmptyStateOnly()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[ScoresUI] No se encontró Canvas en la escena");
                return;
            }

            // Buscar ScoresPanel o crearlo
            Transform scoresPanel = canvas.transform.Find("ScoresPanel");
            if (scoresPanel == null)
            {
                Debug.LogError("[ScoresUI] No se encontró ScoresPanel. Ejecuta 'Reconstruir Scores UI' primero.");
                return;
            }

            // Eliminar EmptyState existente
            Transform existingEmpty = scoresPanel.Find("EmptyState");
            if (existingEmpty != null)
                DestroyImmediate(existingEmpty.gameObject);

            CreateEmptyState(scoresPanel);
            Debug.Log("[ScoresUI] Estado vacío creado!");
        }

        private static void CleanOldElements(Transform canvasTransform)
        {
            string[] oldElements = new string[]
            {
                "Header", "HeaderPanel", "TitleText", "Title",
                "TabsPanel", "TabsContainer", "Tabs",
                "ScoresPanel", "LeaderboardPanel", "ContentPanel",
                "EmptyState", "EmptyMessage", "NoDataText",
                "PlayerPositionPanel", "YourPosition",
                "LoadingPanel", "LoadingIndicator"
            };

            foreach (string elementName in oldElements)
            {
                Transform element = canvasTransform.Find(elementName);
                if (element != null)
                {
                    Debug.Log($"[ScoresUI] Limpiando: {elementName}");
                    DestroyImmediate(element.gameObject);
                }
            }
        }

        private static void CreateBackground(Transform parent)
        {
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(parent, false);
            bg.transform.SetAsFirstSibling();

            RectTransform bgRT = bg.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = DARK_BG;
            bgImage.raycastTarget = true;
        }

        private static void CreateScoresLayout(Transform canvasTransform)
        {
            // ========== HEADER ==========
            CreateHeader(canvasTransform);

            // ========== SCORES PANEL (contenedor principal) ==========
            GameObject scoresPanel = CreateOrFind(canvasTransform, "ScoresPanel");
            RectTransform scoresPanelRT = SetupRectTransform(scoresPanel,
                new Vector2(0, 0), new Vector2(1, 1),
                Vector2.zero, Vector2.zero);
            scoresPanelRT.offsetMin = new Vector2(0, 0);
            scoresPanelRT.offsetMax = new Vector2(0, -140); // Debajo del header

            // ========== TABS ==========
            CreateTabs(scoresPanel.transform);

            // ========== SCROLL VIEW CON CONTENIDO ==========
            CreateScrollView(scoresPanel.transform);

            // ========== EMPTY STATE ==========
            CreateEmptyState(scoresPanel.transform);

            // ========== TU POSICIÓN (footer fijo) ==========
            CreatePlayerPositionPanel(scoresPanel.transform);

            // ========== LOADING PANEL ==========
            CreateLoadingPanel(scoresPanel.transform);
        }

        #region Header

        private static void CreateHeader(Transform parent)
        {
            GameObject header = CreateOrFind(parent, "Header");
            RectTransform headerRT = SetupRectTransform(header,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -70), new Vector2(0, 140));

            // Fondo del header con gradiente sutil
            Image headerBg = header.GetComponent<Image>();
            if (headerBg == null) headerBg = header.AddComponent<Image>();
            headerBg.color = new Color(0.03f, 0.08f, 0.12f, 0.98f);

            // Línea neón inferior
            GameObject headerLine = CreateOrFind(header.transform, "HeaderLine");
            RectTransform lineRT = SetupRectTransform(headerLine,
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 0), new Vector2(0, 3));
            Image lineImg = headerLine.GetComponent<Image>();
            if (lineImg == null) lineImg = headerLine.AddComponent<Image>();
            lineImg.color = CYAN_NEON;

            // Efecto glow de la línea
            Shadow lineShadow = headerLine.GetComponent<Shadow>();
            if (lineShadow == null) lineShadow = headerLine.AddComponent<Shadow>();
            lineShadow.effectColor = new Color(0f, 1f, 1f, 0.5f);
            lineShadow.effectDistance = new Vector2(0, -3);

            // Back Button
            GameObject backBtn = CreateOrFind(header.transform, "BackButton");
            RectTransform backRT = SetupRectTransform(backBtn,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(60, 0), new Vector2(80, 80));

            Image backBg = backBtn.GetComponent<Image>();
            if (backBg == null) backBg = backBtn.AddComponent<Image>();
            backBg.color = new Color(0, 0, 0, 0); // Transparente

            Button backButton = backBtn.GetComponent<Button>();
            if (backButton == null) backButton = backBtn.AddComponent<Button>();
            backButton.targetGraphic = backBg;

            // Texto del back (flecha)
            GameObject backText = CreateOrFind(backBtn.transform, "BackArrow");
            SetupRectTransform(backText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI backTMP = SetupText(backText, "<", 48, CYAN_NEON, FontStyles.Bold);
            backTMP.alignment = TextAlignmentOptions.Center;

            // Título RANKINGS
            GameObject title = CreateOrFind(header.transform, "TitleText");
            RectTransform titleRT = SetupRectTransform(title,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 0), new Vector2(400, 60));
            TextMeshProUGUI titleTMP = SetupText(title, "RANKINGS", 42, CYAN_NEON, FontStyles.Bold);
            titleTMP.alignment = TextAlignmentOptions.Center;

            // Glow del título
            AddTextGlow(title, CYAN_NEON);
        }

        #endregion

        #region Tabs

        private static void CreateTabs(Transform parent)
        {
            GameObject tabsContainer = CreateOrFind(parent, "TabsContainer");
            RectTransform tabsRT = SetupRectTransform(tabsContainer,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -35), new Vector2(-40, 70));

            // Fondo de tabs
            Image tabsBg = tabsContainer.GetComponent<Image>();
            if (tabsBg == null) tabsBg = tabsContainer.AddComponent<Image>();
            tabsBg.color = new Color(0.05f, 0.1f, 0.15f, 0.9f);

            // Borde neón
            Outline tabsOutline = tabsContainer.GetComponent<Outline>();
            if (tabsOutline == null) tabsOutline = tabsContainer.AddComponent<Outline>();
            tabsOutline.effectColor = CYAN_DARK;
            tabsOutline.effectDistance = new Vector2(1.5f, 1.5f);

            // Limpiar hijos existentes (para recrear correctamente)
            for (int i = tabsContainer.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(tabsContainer.transform.GetChild(i).gameObject);
            }

            // Horizontal Layout - 50/50
            HorizontalLayoutGroup hlg = tabsContainer.GetComponent<HorizontalLayoutGroup>();
            if (hlg == null) hlg = tabsContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 2; // Espacio para el divider visual
            hlg.padding = new RectOffset(0, 0, 0, 0);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // Tab Nacional (50%)
            CreateTabButton(tabsContainer.transform, "NacionalTab", "Nacional", true);

            // Tab Mundial (50%)
            CreateTabButton(tabsContainer.transform, "MundialTab", "Mundial", false);

            // Divisor vertical centrado (sobre los tabs, no como hijo del layout)
            GameObject divider = CreateOrFind(tabsContainer.transform, "TabDivider");
            RectTransform divRT = divider.GetComponent<RectTransform>();
            if (divRT == null) divRT = divider.AddComponent<RectTransform>();
            divRT.anchorMin = new Vector2(0.5f, 0.1f);
            divRT.anchorMax = new Vector2(0.5f, 0.9f);
            divRT.pivot = new Vector2(0.5f, 0.5f);
            divRT.sizeDelta = new Vector2(2, 0);
            divRT.anchoredPosition = Vector2.zero;

            Image divImg = divider.GetComponent<Image>();
            if (divImg == null) divImg = divider.AddComponent<Image>();
            divImg.color = CYAN_NEON;
            divImg.raycastTarget = false;

            // Ignorar layout para el divider
            LayoutElement divLE = divider.GetComponent<LayoutElement>();
            if (divLE == null) divLE = divider.AddComponent<LayoutElement>();
            divLE.ignoreLayout = true;
        }

        private static void CreateTabButton(Transform parent, string name, string text, bool isActive)
        {
            GameObject tab = new GameObject(name);
            tab.transform.SetParent(parent, false);

            // RectTransform
            RectTransform tabRT = tab.AddComponent<RectTransform>();

            // LayoutElement - flexibleWidth = 1 para que ambos ocupen 50%
            LayoutElement le = tab.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;
            le.minHeight = 60;

            // Fondo
            Image tabBg = tab.AddComponent<Image>();
            tabBg.color = isActive ? TAB_ACTIVE : TAB_INACTIVE;

            // Button
            Button tabBtn = tab.AddComponent<Button>();
            tabBtn.targetGraphic = tabBg;

            ColorBlock colors = tabBtn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            tabBtn.colors = colors;

            // Texto
            GameObject tabText = new GameObject("Text");
            tabText.transform.SetParent(tab.transform, false);
            RectTransform textRT = tabText.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;
            textRT.anchoredPosition = Vector2.zero;

            Color textColor = isActive ? DARK_BG : Color.white;
            TextMeshProUGUI tabTMP = tabText.AddComponent<TextMeshProUGUI>();
            tabTMP.text = text;
            tabTMP.fontSize = 26;
            tabTMP.color = textColor;
            tabTMP.fontStyle = FontStyles.Bold;
            tabTMP.alignment = TextAlignmentOptions.Center;
            tabTMP.raycastTarget = false;
        }

        #endregion

        #region ScrollView

        private static void CreateScrollView(Transform parent)
        {
            GameObject scrollView = CreateOrFind(parent, "LeaderboardScrollView");
            RectTransform scrollRT = SetupRectTransform(scrollView,
                new Vector2(0, 0), new Vector2(1, 1),
                Vector2.zero, Vector2.zero);
            scrollRT.offsetMin = new Vector2(20, 100); // Espacio para PlayerPosition (abajo)
            scrollRT.offsetMax = new Vector2(-20, -110); // Espacio para Tabs (arriba)

            // Remover cualquier Mask viejo del ScrollView
            Mask scrollMask = scrollView.GetComponent<Mask>();
            if (scrollMask != null) DestroyImmediate(scrollMask);

            ScrollRect scroll = scrollView.GetComponent<ScrollRect>();
            if (scroll == null) scroll = scrollView.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.elasticity = 0.1f;
            scroll.scrollSensitivity = 30f;

            // Viewport con RectMask2D (mejor que Mask regular)
            GameObject viewport = CreateOrFind(scrollView.transform, "Viewport");
            RectTransform viewportRT = SetupRectTransform(viewport, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // RectMask2D no requiere Image
            RectMask2D rectMask = viewport.GetComponent<RectMask2D>();
            if (rectMask == null) rectMask = viewport.AddComponent<RectMask2D>();

            // Remover Mask viejo si existe
            Mask oldMask = viewport.GetComponent<Mask>();
            if (oldMask != null) DestroyImmediate(oldMask);

            // Remover Image si no es necesaria
            Image oldImg = viewport.GetComponent<Image>();
            if (oldImg != null) DestroyImmediate(oldImg);

            // Content (LeaderboardContainer)
            GameObject content = CreateOrFind(viewport.transform, "LeaderboardContainer");
            RectTransform contentRT = SetupRectTransform(content,
                new Vector2(0, 1), new Vector2(1, 1),
                Vector2.zero, new Vector2(0, 0));
            contentRT.pivot = new Vector2(0.5f, 1);

            // VerticalLayoutGroup
            VerticalLayoutGroup vlg = content.GetComponent<VerticalLayoutGroup>();
            if (vlg == null) vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8;
            vlg.padding = new RectOffset(0, 0, 10, 10);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // ContentSizeFitter
            ContentSizeFitter csf = content.GetComponent<ContentSizeFitter>();
            if (csf == null) csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            // Asignar al ScrollRect
            scroll.viewport = viewportRT;
            scroll.content = contentRT;
        }

        #endregion

        #region Empty State

        private static void CreateEmptyState(Transform parent)
        {
            GameObject emptyState = CreateOrFind(parent, "EmptyState");
            RectTransform emptyRT = SetupRectTransform(emptyState,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 50), new Vector2(600, 500));

            // Icono de trofeo (texto emoji o placeholder)
            GameObject icon = CreateOrFind(emptyState.transform, "TrophyIcon");
            RectTransform iconRT = SetupRectTransform(icon,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -30), new Vector2(150, 150));
            TextMeshProUGUI iconTMP = SetupText(icon, "ranking_icon", 100, GOLD, FontStyles.Normal);
            iconTMP.alignment = TextAlignmentOptions.Center;

            // Título
            GameObject title = CreateOrFind(emptyState.transform, "EmptyTitle");
            RectTransform titleRT = SetupRectTransform(title,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 30), new Vector2(500, 60));
            TextMeshProUGUI titleTMP = SetupText(title, "empty_leaderboard_title", 32, Color.white, FontStyles.Bold);
            titleTMP.alignment = TextAlignmentOptions.Center;

            // Subtítulo
            GameObject subtitle = CreateOrFind(emptyState.transform, "EmptySubtitle");
            RectTransform subRT = SetupRectTransform(subtitle,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -30), new Vector2(450, 50));
            TextMeshProUGUI subTMP = SetupText(subtitle, "empty_leaderboard_subtitle", 22, new Color(0.6f, 0.6f, 0.6f), FontStyles.Normal);
            subTMP.alignment = TextAlignmentOptions.Center;

            // Botón JUGAR
            GameObject playBtn = CreateOrFind(emptyState.transform, "PlayButton");
            RectTransform playRT = SetupRectTransform(playBtn,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 80), new Vector2(280, 70));

            Image playBg = playBtn.GetComponent<Image>();
            if (playBg == null) playBg = playBtn.AddComponent<Image>();
            playBg.color = CYAN_GLOW;

            // Borde del botón
            Outline playOutline = playBtn.GetComponent<Outline>();
            if (playOutline == null) playOutline = playBtn.AddComponent<Outline>();
            playOutline.effectColor = CYAN_NEON;
            playOutline.effectDistance = new Vector2(2, 2);

            Button playButton = playBtn.GetComponent<Button>();
            if (playButton == null) playButton = playBtn.AddComponent<Button>();
            playButton.targetGraphic = playBg;

            ColorBlock colors = playButton.colors;
            colors.normalColor = CYAN_GLOW;
            colors.highlightedColor = CYAN_NEON;
            colors.pressedColor = CYAN_DARK;
            playButton.colors = colors;

            // Texto del botón
            GameObject playText = CreateOrFind(playBtn.transform, "Text");
            SetupRectTransform(playText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI playTMP = SetupText(playText, "play_now", 28, DARK_BG, FontStyles.Bold);
            playTMP.alignment = TextAlignmentOptions.Center;

            // Inicialmente oculto (se muestra cuando no hay datos)
            emptyState.SetActive(false);
        }

        #endregion

        #region Player Position Panel

        private static void CreatePlayerPositionPanel(Transform parent)
        {
            GameObject posPanel = CreateOrFind(parent, "PlayerPositionPanel");
            RectTransform posRT = SetupRectTransform(posPanel,
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 50), new Vector2(-40, 80));

            // Fondo oscuro
            Image posBg = posPanel.GetComponent<Image>();
            if (posBg == null) posBg = posPanel.AddComponent<Image>();
            posBg.color = new Color(0.03f, 0.08f, 0.12f, 0.98f);

            // Borde neón superior (línea cyan)
            GameObject topLine = CreateOrFind(posPanel.transform, "TopLine");
            RectTransform topLineRT = SetupRectTransform(topLine,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, 0), new Vector2(0, 2));
            Image topLineImg = topLine.GetComponent<Image>();
            if (topLineImg == null) topLineImg = topLine.AddComponent<Image>();
            topLineImg.color = CYAN_NEON;

            // HorizontalLayoutGroup para distribuir los elementos
            HorizontalLayoutGroup hlg = posPanel.GetComponent<HorizontalLayoutGroup>();
            if (hlg == null) hlg = posPanel.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.padding = new RectOffset(30, 30, 15, 15);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            // Etiqueta "TU POSICIÓN"
            GameObject label = CreateOrFind(posPanel.transform, "PositionLabel");
            TextMeshProUGUI labelTMP = SetupText(label, "TU POSICIÓN", 18, new Color(0.6f, 0.6f, 0.6f), FontStyles.Normal);
            labelTMP.alignment = TextAlignmentOptions.Center;
            LayoutElement labelLE = label.GetComponent<LayoutElement>();
            if (labelLE == null) labelLE = label.AddComponent<LayoutElement>();
            labelLE.preferredWidth = 200;

            // Número de posición (centro)
            GameObject posNumber = CreateOrFind(posPanel.transform, "PositionNumber");
            TextMeshProUGUI numTMP = SetupText(posNumber, "#--", 36, GOLD, FontStyles.Bold);
            numTMP.alignment = TextAlignmentOptions.Center;
            LayoutElement numLE = posNumber.GetComponent<LayoutElement>();
            if (numLE == null) numLE = posNumber.AddComponent<LayoutElement>();
            numLE.flexibleWidth = 1;
            numLE.preferredWidth = 150;

            // Glow del número
            AddTextGlow(posNumber, GOLD);

            // Tiempo del jugador (derecha)
            GameObject posTime = CreateOrFind(posPanel.transform, "PositionTime");
            TextMeshProUGUI timeTMP = SetupText(posTime, "--", 22, new Color(0f, 1f, 0.53f), FontStyles.Normal);
            timeTMP.alignment = TextAlignmentOptions.Center;
            LayoutElement timeLE = posTime.GetComponent<LayoutElement>();
            if (timeLE == null) timeLE = posTime.AddComponent<LayoutElement>();
            timeLE.preferredWidth = 150;
        }

        #endregion

        #region Loading Panel

        private static void CreateLoadingPanel(Transform parent)
        {
            GameObject loadingPanel = CreateOrFind(parent, "LoadingPanel");
            RectTransform loadingRT = SetupRectTransform(loadingPanel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(300, 150));

            // Fondo semi-transparente
            Image loadingBg = loadingPanel.GetComponent<Image>();
            if (loadingBg == null) loadingBg = loadingPanel.AddComponent<Image>();
            loadingBg.color = new Color(0.02f, 0.05f, 0.1f, 0.9f);

            // Borde
            Outline loadingOutline = loadingPanel.GetComponent<Outline>();
            if (loadingOutline == null) loadingOutline = loadingPanel.AddComponent<Outline>();
            loadingOutline.effectColor = CYAN_DARK;
            loadingOutline.effectDistance = new Vector2(1, 1);

            // Texto de carga
            GameObject loadingText = CreateOrFind(loadingPanel.transform, "LoadingText");
            SetupRectTransform(loadingText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI loadingTMP = SetupText(loadingText, "loading_rankings", 24, CYAN_NEON, FontStyles.Normal);
            loadingTMP.alignment = TextAlignmentOptions.Center;

            // Inicialmente oculto
            loadingPanel.SetActive(false);
        }

        #endregion

        #region Helpers

        private static GameObject CreateOrFind(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject newObj = new GameObject(name);
            newObj.transform.SetParent(parent, false);
            return newObj;
        }

        private static RectTransform SetupRectTransform(GameObject obj, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt == null) rt = obj.AddComponent<RectTransform>();

            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;

            return rt;
        }

        private static TextMeshProUGUI SetupText(GameObject obj, string text, int fontSize, Color color, FontStyles style)
        {
            TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = obj.AddComponent<TextMeshProUGUI>();

            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;

            return tmp;
        }

        private static void AddTextGlow(GameObject textObj, Color glowColor)
        {
            Shadow shadow = textObj.GetComponent<Shadow>();
            if (shadow == null) shadow = textObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(glowColor.r, glowColor.g, glowColor.b, 0.5f);
            shadow.effectDistance = new Vector2(2, -2);
        }

        #endregion
    }
}
