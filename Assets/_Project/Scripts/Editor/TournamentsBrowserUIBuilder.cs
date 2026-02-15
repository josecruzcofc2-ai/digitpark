using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Construye la UI completa de TournamentsBrowser
    /// Incluye: SafeArea, Header, Tabs, SearchBar, Lista de torneos, EmptyState
    /// </summary>
    public class TournamentsBrowserUIBuilder : EditorWindow
    {
        // ==================== COLORES DEL TEMA NEON ====================
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.4f, 1f);
        private static readonly Color CYAN_GLOW = new Color(0f, 1f, 1f, 0.3f);

        private static readonly Color DARK_BG = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color PANEL_BG = new Color(0.08f, 0.12f, 0.18f, 0.98f);
        private static readonly Color INPUT_BG = new Color(0.04f, 0.08f, 0.12f, 1f);
        private static readonly Color HEADER_BG = new Color(0.03f, 0.06f, 0.1f, 0.95f);

        private static readonly Color TEXT_PRIMARY = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.7f, 0.75f, 1f);
        private static readonly Color TEXT_DARK = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color TEXT_MUTED = new Color(0.4f, 0.5f, 0.55f, 1f);

        private static readonly Color BUTTON_PRIMARY = CYAN_NEON;
        private static readonly Color BUTTON_SECONDARY = new Color(0.15f, 0.2f, 0.25f, 1f);

        private static readonly Color TAB_ACTIVE = CYAN_NEON;
        private static readonly Color TAB_INACTIVE = new Color(0.12f, 0.16f, 0.2f, 1f);

        private const string BACK_BUTTON_PREFAB = "Assets/_Project/Prefabs/Common/BackButton.prefab";

        // ==================== DIMENSIONES ====================
        private const float HEADER_HEIGHT = 100f;
        private const float TABS_HEIGHT = 55f;
        private const float SEARCHBAR_HEIGHT = 80f;
        private const float CONTENT_PADDING = 20f;
        private const float FAB_SIZE = 70f;

        [MenuItem("DigitPark/UI Builders/Tournaments/TournamentsBrowser", false, 235)]
        public static void BuildUI()
        {
            if (!EditorUtility.DisplayDialog("TournamentsBrowser UI Builder",
                "Esto construira la UI completa de TournamentsBrowser.\nAsegurate de tener la escena TournamentsBrowser abierta.\n\nContinuar?",
                "Si", "No"))
                return;

            BuildCompleteUI();
        }

        private static void BuildCompleteUI()
        {
            Debug.Log("[TournamentsBrowserUIBuilder] ========== INICIANDO CONSTRUCCION ==========");

            // Limpiar UI vieja de TODOS los Canvas (especialmente TransitionCanvas)
            CleanupOldUI();

            // 1. Setup base (Canvas, EventSystem, Camera)
            Canvas canvas = SetupCanvas();
            if (canvas == null) return;

            // 2. Background
            CreateBackground(canvas);

            // 3. SafeArea
            GameObject safeArea = CreateSafeArea(canvas);

            // 4. Header
            CreateHeader(safeArea);

            // 5. Tabs
            CreateTabs(safeArea);

            // 6. SearchBar
            CreateSearchBar(safeArea);

            // 7. Tournament List (ScrollView)
            CreateTournamentList(safeArea);

            // 8. Filter Dropdowns Panel
            CreateFilterPanel(safeArea);

            // 9. EmptyState
            CreateEmptyState(safeArea);

            // 10. LoadingIndicator
            CreateLoadingIndicator(safeArea);

            // 11. RefreshIndicator
            CreateRefreshIndicator(safeArea);

            // 12. LoadMore Button
            CreateLoadMoreButton(safeArea);

            // 13. FAB (Create Tournament Button)
            CreateFAB(safeArea);

            MarkSceneDirty();
            Debug.Log("[TournamentsBrowserUIBuilder] ========== CONSTRUCCION COMPLETADA ==========");
        }

        // ==================== CANVAS SETUP ====================

        private static Canvas SetupCanvas()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();

            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;

                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // EventSystem
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Camera
            if (Camera.main == null)
            {
                GameObject cameraObj = new GameObject("Main Camera");
                Camera cam = cameraObj.AddComponent<Camera>();
                cam.tag = "MainCamera";
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = DARK_BG;
            }

            return canvas;
        }

        /// <summary>
        /// Limpia UI creada por el builder de TODOS los Canvas raíz.
        /// En TransitionCanvas: elimina TODO excepto elementos de transición.
        /// En Canvas principal: elimina Background y SafeArea.
        /// </summary>
        private static void CleanupOldUI()
        {
            string[] transitionElements = { "FadeImage", "CircleWipeImage", "SlidePanel" };

            foreach (var canvas in Object.FindObjectsOfType<Canvas>(true))
            {
                if (canvas.transform.parent != null) continue;

                if (canvas.gameObject.name.Contains("Transition"))
                {
                    for (int i = canvas.transform.childCount - 1; i >= 0; i--)
                    {
                        Transform child = canvas.transform.GetChild(i);
                        bool isTransitionElement = false;
                        foreach (string te in transitionElements)
                        {
                            if (child.gameObject.name == te) { isTransitionElement = true; break; }
                        }
                        if (!isTransitionElement)
                        {
                            Debug.Log($"[TournamentsBrowserUIBuilder] Limpiando '{child.gameObject.name}' de TransitionCanvas");
                            Object.DestroyImmediate(child.gameObject);
                        }
                    }
                }
                else
                {
                    string[] toClean = { "Background", "SafeArea" };
                    foreach (string name in toClean)
                    {
                        Transform t = canvas.transform.Find(name);
                        if (t != null)
                        {
                            Debug.Log($"[TournamentsBrowserUIBuilder] Limpiando '{name}' de {canvas.gameObject.name}");
                            Object.DestroyImmediate(t.gameObject);
                        }
                    }
                }
            }
        }

        // ==================== BACKGROUND ====================

        private static void CreateBackground(Canvas canvas)
        {
            GameObject bg = FindOrCreateChild(canvas.gameObject, "Background");

            RectTransform bgRT = GetOrAddComponent<RectTransform>(bg);
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;
            bgRT.anchoredPosition = Vector2.zero;

            Image bgImage = GetOrAddComponent<Image>(bg);
            bgImage.color = DARK_BG;
            bgImage.raycastTarget = false;

            bg.transform.SetAsFirstSibling();
            EditorUtility.SetDirty(bg);
        }

        // ==================== SAFE AREA ====================

        private static GameObject CreateSafeArea(Canvas canvas)
        {
            GameObject safeArea = FindOrCreateChild(canvas.gameObject, "SafeArea");

            RectTransform safeRT = GetOrAddComponent<RectTransform>(safeArea);
            safeRT.anchorMin = Vector2.zero;
            safeRT.anchorMax = Vector2.one;
            safeRT.sizeDelta = Vector2.zero;
            safeRT.anchoredPosition = Vector2.zero;

            safeArea.transform.SetSiblingIndex(1);
            EditorUtility.SetDirty(safeArea);

            return safeArea;
        }

        // ==================== HEADER ====================

        private static void CreateHeader(GameObject parent)
        {
            GameObject header = FindOrCreateChild(parent, "Header");

            RectTransform headerRT = GetOrAddComponent<RectTransform>(header);
            headerRT.anchorMin = new Vector2(0, 1);
            headerRT.anchorMax = new Vector2(1, 1);
            headerRT.pivot = new Vector2(0.5f, 1);
            headerRT.anchoredPosition = Vector2.zero;
            headerRT.sizeDelta = new Vector2(0, HEADER_HEIGHT);

            Image headerBg = GetOrAddComponent<Image>(header);
            headerBg.color = HEADER_BG;

            // Glow inferior
            CreateBottomGlow(header);

            // BackButton
            GameObject backBtnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BACK_BUTTON_PREFAB);
            GameObject backBtn;
            if (backBtnPrefab != null)
            {
                Transform oldBtn = header.transform.Find("BackButton");
                if (oldBtn != null) DestroyImmediate(oldBtn.gameObject);
                backBtn = (GameObject)PrefabUtility.InstantiatePrefab(backBtnPrefab, header.transform);
                backBtn.name = "BackButton";
            }
            else
            {
                backBtn = FindOrCreateChild(header, "BackButton");
                var fallbackBg = backBtn.GetComponent<Image>(); if (fallbackBg == null) fallbackBg = backBtn.AddComponent<Image>();
                fallbackBg.color = new Color(0, 0, 0, 0);
                if (backBtn.GetComponent<Button>() == null) backBtn.AddComponent<Button>();
                Debug.LogWarning("[TournamentsBrowserUI] BackButton prefab not found, using fallback");
            }
            RectTransform backRT = GetOrAddComponent<RectTransform>(backBtn);
            backRT.anchorMin = new Vector2(0, 0.5f);
            backRT.anchorMax = new Vector2(0, 0.5f);
            backRT.pivot = new Vector2(0, 0.5f);
            backRT.anchoredPosition = new Vector2(20, 0);
            backRT.sizeDelta = new Vector2(50, 50);

            // Title
            GameObject titleObj = FindOrCreateChild(header, "TitleText");
            RectTransform titleRT = GetOrAddComponent<RectTransform>(titleObj);
            titleRT.anchorMin = new Vector2(0.1f, 0f);
            titleRT.anchorMax = new Vector2(0.95f, 1f);
            titleRT.offsetMin = Vector2.zero;
            titleRT.offsetMax = Vector2.zero;

            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "TORNEOS";
            titleText.fontSize = 78;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = CYAN_NEON;
            titleText.alignment = TextAlignmentOptions.Center;
            AddOutline(titleObj, CYAN_GLOW, 2);

            EditorUtility.SetDirty(header);
            Debug.Log("[TournamentsBrowserUIBuilder] Header creado");
        }

        // ==================== TABS ====================

        private static void CreateTabs(GameObject parent)
        {
            GameObject tabsPanel = FindOrCreateChild(parent, "TabsPanel");

            RectTransform tabsRT = GetOrAddComponent<RectTransform>(tabsPanel);
            tabsRT.anchorMin = new Vector2(0, 1);
            tabsRT.anchorMax = new Vector2(1, 1);
            tabsRT.pivot = new Vector2(0.5f, 1);
            tabsRT.anchoredPosition = new Vector2(0, -HEADER_HEIGHT);
            tabsRT.sizeDelta = new Vector2(-CONTENT_PADDING * 2, TABS_HEIGHT);

            Image tabsBg = GetOrAddComponent<Image>(tabsPanel);
            tabsBg.color = new Color(0.04f, 0.07f, 0.11f, 0.9f);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(tabsPanel);
            hlg.spacing = 10f;
            hlg.padding = new RectOffset(10, 10, 5, 5);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // Tab: Buscar Torneos
            CreateTab(tabsPanel, "SearchTournamentsTab", "Buscar Torneos", true);

            // Tab: Mis Torneos
            CreateTab(tabsPanel, "MyTournamentsTab", "Mis Torneos", false);

            // Tab: Destacados
            CreateTab(tabsPanel, "FeaturedTab", "Destacados", false);

            EditorUtility.SetDirty(tabsPanel);
            Debug.Log("[TournamentsBrowserUIBuilder] Tabs creados");
        }

        private static void CreateTab(GameObject parent, string name, string label, bool isActive)
        {
            GameObject tab = FindOrCreateChild(parent, name);

            Image tabBg = GetOrAddComponent<Image>(tab);
            tabBg.color = isActive ? TAB_ACTIVE : TAB_INACTIVE;

            Button tabButton = GetOrAddComponent<Button>(tab);
            SetupButtonColors(tabButton, isActive ? TAB_ACTIVE : TAB_INACTIVE);
            AddOutline(tab, CYAN_DARK);

            GameObject textObj = FindOrCreateChild(tab, "Text");
            TextMeshProUGUI tabText = GetOrAddComponent<TextMeshProUGUI>(textObj);
            tabText.text = label;
            tabText.fontSize = 40;
            tabText.fontStyle = FontStyles.Bold;
            tabText.color = isActive ? TEXT_DARK : TEXT_PRIMARY;
            tabText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(textObj);

            LayoutElement le = GetOrAddComponent<LayoutElement>(tab);
            le.minHeight = 45;
            le.flexibleWidth = 1;
        }

        // ==================== SEARCHBAR ====================

        private static void CreateSearchBar(GameObject parent)
        {
            GameObject searchBar = FindOrCreateChild(parent, "SearchBar");

            RectTransform searchRT = GetOrAddComponent<RectTransform>(searchBar);
            searchRT.anchorMin = new Vector2(0, 1);
            searchRT.anchorMax = new Vector2(1, 1);
            searchRT.pivot = new Vector2(0.5f, 1);
            searchRT.anchoredPosition = new Vector2(0, -(HEADER_HEIGHT + TABS_HEIGHT + 10));
            searchRT.sizeDelta = new Vector2(-CONTENT_PADDING * 2, SEARCHBAR_HEIGHT);

            Image searchBg = GetOrAddComponent<Image>(searchBar);
            searchBg.color = INPUT_BG;
            AddOutline(searchBar, CYAN_DARK);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(searchBar);
            hlg.spacing = 10;
            hlg.padding = new RectOffset(15, 15, 10, 10);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            // Search Icon
            GameObject iconObj = FindOrCreateChild(searchBar, "SearchIcon");
            RectTransform iconRT = GetOrAddComponent<RectTransform>(iconObj);
            iconRT.sizeDelta = new Vector2(35, 35);

            Image iconImage = GetOrAddComponent<Image>(iconObj);
            iconImage.color = TEXT_SECONDARY;

            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(iconObj);
            iconLE.minWidth = 35;
            iconLE.preferredWidth = 35;

            // Search Input
            GameObject inputObj = FindOrCreateChild(searchBar, "SearchInput");
            CreateInputField(inputObj, "Buscar torneos...");

            LayoutElement inputLE = GetOrAddComponent<LayoutElement>(inputObj);
            inputLE.flexibleWidth = 1;
            inputLE.minWidth = 200;

            // Filter Button
            GameObject filterBtn = FindOrCreateChild(searchBar, "FilterButton");
            RectTransform filterRT = GetOrAddComponent<RectTransform>(filterBtn);
            filterRT.sizeDelta = new Vector2(160, 50);

            Image filterBg = GetOrAddComponent<Image>(filterBtn);
            filterBg.color = BUTTON_SECONDARY;

            Button filterButton = GetOrAddComponent<Button>(filterBtn);
            SetupButtonColors(filterButton, BUTTON_SECONDARY);
            AddOutline(filterBtn, CYAN_DARK);

            LayoutElement filterLE = GetOrAddComponent<LayoutElement>(filterBtn);
            filterLE.minWidth = 160;
            filterLE.preferredWidth = 160;

            GameObject filterTextObj = FindOrCreateChild(filterBtn, "Text");
            TextMeshProUGUI filterText = GetOrAddComponent<TextMeshProUGUI>(filterTextObj);
            filterText.text = "Filtros";
            filterText.fontSize = 35;
            filterText.fontStyle = FontStyles.Bold;
            filterText.color = TEXT_PRIMARY;
            filterText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(filterTextObj);

            EditorUtility.SetDirty(searchBar);
            Debug.Log("[TournamentsBrowserUIBuilder] SearchBar creado");
        }

        // ==================== TOURNAMENT LIST ====================

        private static void CreateTournamentList(GameObject parent)
        {
            float topOffset = HEADER_HEIGHT + TABS_HEIGHT + SEARCHBAR_HEIGHT + 30;

            GameObject scrollView = FindOrCreateChild(parent, "TournamentsScrollView");

            RectTransform scrollRT = GetOrAddComponent<RectTransform>(scrollView);
            scrollRT.anchorMin = new Vector2(0, 0);
            scrollRT.anchorMax = new Vector2(1, 1);
            scrollRT.pivot = new Vector2(0.5f, 0.5f);
            scrollRT.offsetMin = new Vector2(CONTENT_PADDING, CONTENT_PADDING);
            scrollRT.offsetMax = new Vector2(-CONTENT_PADDING, -topOffset);

            ScrollRect scrollRect = GetOrAddComponent<ScrollRect>(scrollView);
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 50f;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.1f;

            Image scrollBg = GetOrAddComponent<Image>(scrollView);
            scrollBg.color = Color.clear;

            // Viewport
            GameObject viewport = FindOrCreateChild(scrollView, "Viewport");
            RectTransform viewportRT = GetOrAddComponent<RectTransform>(viewport);
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.sizeDelta = Vector2.zero;
            viewportRT.anchoredPosition = Vector2.zero;

            RectMask2D rectMask = GetOrAddComponent<RectMask2D>(viewport);
            scrollRect.viewport = viewportRT;

            // Content
            GameObject content = FindOrCreateChild(viewport, "Content");
            RectTransform contentRT = GetOrAddComponent<RectTransform>(content);
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.anchoredPosition = Vector2.zero;
            contentRT.sizeDelta = new Vector2(0, 0);

            scrollRect.content = contentRT;

            ContentSizeFitter csf = GetOrAddComponent<ContentSizeFitter>(content);
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(content);
            vlg.spacing = 12f;
            vlg.padding = new RectOffset(0, 0, 10, 10);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Sample Tournament Items
            for (int i = 0; i < 3; i++)
            {
                CreateSampleTournamentItem(content, i);
            }

            EditorUtility.SetDirty(scrollView);
            Debug.Log("[TournamentsBrowserUIBuilder] TournamentList creado");
        }

        private static void CreateSampleTournamentItem(GameObject parent, int index)
        {
            GameObject item = FindOrCreateChild(parent, $"TournamentItem_{index}");

            RectTransform itemRT = GetOrAddComponent<RectTransform>(item);
            itemRT.sizeDelta = new Vector2(0, 200);

            Image itemBg = GetOrAddComponent<Image>(item);
            itemBg.color = PANEL_BG;
            AddOutline(item, CYAN_DARK);

            Button itemButton = GetOrAddComponent<Button>(item);
            SetupButtonColors(itemButton, PANEL_BG);

            LayoutElement le = GetOrAddComponent<LayoutElement>(item);
            le.minHeight = 200;
            le.preferredHeight = 200;

            // Tournament Name
            GameObject nameObj = FindOrCreateChild(item, "TournamentName");
            RectTransform nameRT = GetOrAddComponent<RectTransform>(nameObj);
            nameRT.anchorMin = new Vector2(0, 0.6f);
            nameRT.anchorMax = new Vector2(0.7f, 1);
            nameRT.offsetMin = new Vector2(15, 0);
            nameRT.offsetMax = new Vector2(0, -10);

            TextMeshProUGUI nameText = GetOrAddComponent<TextMeshProUGUI>(nameObj);
            nameText.text = $"Torneo de Ejemplo {index + 1}";
            nameText.fontSize = 50;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = TEXT_PRIMARY;
            nameText.alignment = TextAlignmentOptions.MidlineLeft;

            // Game & Players
            GameObject infoObj = FindOrCreateChild(item, "Info");
            RectTransform infoRT = GetOrAddComponent<RectTransform>(infoObj);
            infoRT.anchorMin = new Vector2(0, 0.2f);
            infoRT.anchorMax = new Vector2(0.7f, 0.6f);
            infoRT.offsetMin = new Vector2(15, 0);
            infoRT.offsetMax = Vector2.zero;

            TextMeshProUGUI infoText = GetOrAddComponent<TextMeshProUGUI>(infoObj);
            infoText.text = "Digit Rush  |  8/16 jugadores";
            infoText.fontSize = 35;
            infoText.color = TEXT_SECONDARY;
            infoText.alignment = TextAlignmentOptions.MidlineLeft;

            // Entry Fee
            GameObject feeObj = FindOrCreateChild(item, "EntryFee");
            RectTransform feeRT = GetOrAddComponent<RectTransform>(feeObj);
            feeRT.anchorMin = new Vector2(0.7f, 0.5f);
            feeRT.anchorMax = new Vector2(1, 1);
            feeRT.offsetMin = Vector2.zero;
            feeRT.offsetMax = new Vector2(-15, -10);

            TextMeshProUGUI feeText = GetOrAddComponent<TextMeshProUGUI>(feeObj);
            feeText.text = "GRATIS";
            feeText.fontSize = 45;
            feeText.fontStyle = FontStyles.Bold;
            feeText.color = CYAN_NEON;
            feeText.alignment = TextAlignmentOptions.MidlineRight;

            // Status
            GameObject statusObj = FindOrCreateChild(item, "Status");
            RectTransform statusRT = GetOrAddComponent<RectTransform>(statusObj);
            statusRT.anchorMin = new Vector2(0.7f, 0);
            statusRT.anchorMax = new Vector2(1, 0.5f);
            statusRT.offsetMin = Vector2.zero;
            statusRT.offsetMax = new Vector2(-15, 0);

            TextMeshProUGUI statusText = GetOrAddComponent<TextMeshProUGUI>(statusObj);
            statusText.text = "Esperando...";
            statusText.fontSize = 30;
            statusText.color = TEXT_MUTED;
            statusText.alignment = TextAlignmentOptions.MidlineRight;
        }

        // ==================== EMPTY STATE ====================

        private static void CreateEmptyState(GameObject parent)
        {
            GameObject emptyState = FindOrCreateChild(parent, "EmptyState");
            emptyState.SetActive(false); // Oculto por defecto

            RectTransform emptyRT = GetOrAddComponent<RectTransform>(emptyState);
            emptyRT.anchorMin = new Vector2(0.1f, 0.3f);
            emptyRT.anchorMax = new Vector2(0.9f, 0.7f);
            emptyRT.anchoredPosition = Vector2.zero;
            emptyRT.sizeDelta = Vector2.zero;

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(emptyState);
            vlg.spacing = 25;
            vlg.padding = new RectOffset(20, 20, 30, 30);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Icon
            GameObject iconObj = FindOrCreateChild(emptyState, "Icon");
            Image iconImage = GetOrAddComponent<Image>(iconObj);
            iconImage.color = CYAN_GLOW;
            iconImage.preserveAspect = true;
            AddOutline(iconObj, CYAN_GLOW, 3);

            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(iconObj);
            iconLE.minHeight = 100;
            iconLE.preferredHeight = 100;
            iconLE.minWidth = 100;
            iconLE.preferredWidth = 100;

            // Title (EmptyStateText for AutoAssigner)
            GameObject titleObj = FindOrCreateChild(emptyState, "EmptyStateText");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "No hay torneos disponibles";
            titleText.fontSize = 60;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = TEXT_PRIMARY;
            titleText.alignment = TextAlignmentOptions.Center;

            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minHeight = 35;

            // Subtitle
            GameObject subtitleObj = FindOrCreateChild(emptyState, "Subtitle");
            TextMeshProUGUI subtitleText = GetOrAddComponent<TextMeshProUGUI>(subtitleObj);
            subtitleText.text = "Se el primero en crear uno\no vuelve mas tarde";
            subtitleText.fontSize = 40;
            subtitleText.color = TEXT_SECONDARY;
            subtitleText.alignment = TextAlignmentOptions.Center;

            LayoutElement subtitleLE = GetOrAddComponent<LayoutElement>(subtitleObj);
            subtitleLE.minHeight = 50;

            // Create Button
            GameObject createBtn = FindOrCreateChild(emptyState, "CreateButton");
            Image createBg = GetOrAddComponent<Image>(createBtn);
            createBg.color = BUTTON_PRIMARY;

            Button createButton = GetOrAddComponent<Button>(createBtn);
            SetupButtonColors(createButton, BUTTON_PRIMARY);
            AddOutline(createBtn, CYAN_GLOW, 2);

            LayoutElement createLE = GetOrAddComponent<LayoutElement>(createBtn);
            createLE.minHeight = 55;
            createLE.preferredHeight = 55;
            createLE.minWidth = 220;

            GameObject createTextObj = FindOrCreateChild(createBtn, "Text");
            TextMeshProUGUI createText = GetOrAddComponent<TextMeshProUGUI>(createTextObj);
            createText.text = "Crear Torneo";
            createText.fontSize = 50;
            createText.fontStyle = FontStyles.Bold;
            createText.color = TEXT_DARK;
            createText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(createTextObj);

            EditorUtility.SetDirty(emptyState);
            Debug.Log("[TournamentsBrowserUIBuilder] EmptyState creado");
        }

        // ==================== FILTER PANEL ====================

        private static void CreateFilterPanel(GameObject parent)
        {
            GameObject filterPanel = FindOrCreateChild(parent, "FilterPanel");
            filterPanel.SetActive(false); // Oculto por defecto, se abre con FilterButton

            RectTransform filterRT = GetOrAddComponent<RectTransform>(filterPanel);
            filterRT.anchorMin = new Vector2(0, 1);
            filterRT.anchorMax = new Vector2(1, 1);
            filterRT.pivot = new Vector2(0.5f, 1);
            filterRT.anchoredPosition = new Vector2(0, -(HEADER_HEIGHT + TABS_HEIGHT + SEARCHBAR_HEIGHT + 20));
            filterRT.sizeDelta = new Vector2(-CONTENT_PADDING * 2, 220);

            Image filterBg = GetOrAddComponent<Image>(filterPanel);
            filterBg.color = PANEL_BG;
            AddOutline(filterPanel, CYAN_DARK);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(filterPanel);
            vlg.spacing = 10;
            vlg.padding = new RectOffset(15, 15, 15, 15);
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Game Type Filter Dropdown
            CreateFilterDropdown(filterPanel, "GameTypeFilter", "Tipo de Juego",
                new string[] { "Todos", "DigitRush", "FlashTap", "QuickMath", "MemoryPairs", "OddOneOut" });

            // Entry Fee Filter Dropdown
            CreateFilterDropdown(filterPanel, "EntryFeeFilter", "Entrada",
                new string[] { "Todas", "Gratis", "$1-$5", "$5-$25", "$25+" });

            // Status Filter Dropdown
            CreateFilterDropdown(filterPanel, "StatusFilter", "Estado",
                new string[] { "Todos", "Abierto", "En Progreso", "Finalizado" });

            // Clear Filters Button
            GameObject clearBtn = FindOrCreateChild(filterPanel, "ClearFiltersButton");
            Image clearBg = GetOrAddComponent<Image>(clearBtn);
            clearBg.color = BUTTON_SECONDARY;
            Button clearButton = GetOrAddComponent<Button>(clearBtn);
            SetupButtonColors(clearButton, BUTTON_SECONDARY);

            LayoutElement clearLE = GetOrAddComponent<LayoutElement>(clearBtn);
            clearLE.minHeight = 40;
            clearLE.preferredHeight = 40;

            GameObject clearTextObj = FindOrCreateChild(clearBtn, "Text");
            TextMeshProUGUI clearText = GetOrAddComponent<TextMeshProUGUI>(clearTextObj);
            clearText.text = "Limpiar Filtros";
            clearText.fontSize = 40;
            clearText.color = TEXT_SECONDARY;
            clearText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(clearTextObj);

            EditorUtility.SetDirty(filterPanel);
            Debug.Log("[TournamentsBrowserUIBuilder] FilterPanel creado");
        }

        private static void CreateFilterDropdown(GameObject parent, string name, string label, string[] options)
        {
            GameObject row = FindOrCreateChild(parent, name);

            LayoutElement le = GetOrAddComponent<LayoutElement>(row);
            le.minHeight = 40;
            le.preferredHeight = 40;

            // TMP_Dropdown
            Image rowBg = GetOrAddComponent<Image>(row);
            rowBg.color = INPUT_BG;

            TMP_Dropdown dropdown = GetOrAddComponent<TMP_Dropdown>(row);
            dropdown.ClearOptions();
            var optionsList = new System.Collections.Generic.List<TMP_Dropdown.OptionData>();
            foreach (var opt in options)
                optionsList.Add(new TMP_Dropdown.OptionData(opt));
            dropdown.AddOptions(optionsList);

            // Caption Text
            GameObject captionObj = FindOrCreateChild(row, "Label");
            TextMeshProUGUI captionText = GetOrAddComponent<TextMeshProUGUI>(captionObj);
            captionText.text = label;
            captionText.fontSize = 40;
            captionText.color = TEXT_PRIMARY;
            captionText.alignment = TextAlignmentOptions.Left;
            SetRectTransformStretch(captionObj);
            RectTransform captionRT = captionObj.GetComponent<RectTransform>();
            captionRT.offsetMin = new Vector2(10, 0);
            captionRT.offsetMax = new Vector2(-30, 0);

            dropdown.captionText = captionText;

            // Build dropdown template
            BuildDropdownTemplate(row.transform, dropdown);
        }

        private static void BuildDropdownTemplate(Transform dropdownParent, TMP_Dropdown dropdown)
        {
            GameObject template = FindOrCreateChild(dropdownParent.gameObject, "Template");
            RectTransform templateRT = GetOrAddComponent<RectTransform>(template);
            templateRT.anchorMin = new Vector2(0, 0);
            templateRT.anchorMax = new Vector2(1, 0);
            templateRT.pivot = new Vector2(0.5f, 1);
            templateRT.sizeDelta = new Vector2(0, 200);
            GetOrAddComponent<Image>(template).color = INPUT_BG;
            ScrollRect scrollRect = GetOrAddComponent<ScrollRect>(template);
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            GameObject viewport = FindOrCreateChild(template, "Viewport");
            RectTransform viewportRT = GetOrAddComponent<RectTransform>(viewport);
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.sizeDelta = new Vector2(-2, -2);
            viewportRT.anchoredPosition = Vector2.zero;
            Mask viewportMask = GetOrAddComponent<Mask>(viewport);
            viewportMask.showMaskGraphic = false;
            GetOrAddComponent<Image>(viewport).color = Color.white;
            scrollRect.viewport = viewportRT;

            GameObject content = FindOrCreateChild(viewport, "Content");
            RectTransform contentRT = GetOrAddComponent<RectTransform>(content);
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.sizeDelta = Vector2.zero;
            scrollRect.content = contentRT;

            GameObject item = FindOrCreateChild(content, "Item");
            RectTransform itemRT = GetOrAddComponent<RectTransform>(item);
            itemRT.anchorMin = new Vector2(0, 0.5f);
            itemRT.anchorMax = new Vector2(1, 0.5f);
            itemRT.sizeDelta = new Vector2(0, 40);
            Toggle itemToggle = GetOrAddComponent<Toggle>(item);

            GameObject itemBg = FindOrCreateChild(item, "Item Background");
            RectTransform itemBgRT = GetOrAddComponent<RectTransform>(itemBg);
            itemBgRT.anchorMin = Vector2.zero;
            itemBgRT.anchorMax = Vector2.one;
            itemBgRT.sizeDelta = Vector2.zero;
            Image itemBgImg = GetOrAddComponent<Image>(itemBg);
            itemBgImg.color = PANEL_BG;

            GameObject itemCheck = FindOrCreateChild(item, "Item Checkmark");
            RectTransform checkRT = GetOrAddComponent<RectTransform>(itemCheck);
            checkRT.anchorMin = new Vector2(0, 0.5f);
            checkRT.anchorMax = new Vector2(0, 0.5f);
            checkRT.pivot = new Vector2(0, 0.5f);
            checkRT.sizeDelta = new Vector2(16, 16);
            checkRT.anchoredPosition = new Vector2(8, 0);
            Image checkImg = GetOrAddComponent<Image>(itemCheck);
            checkImg.color = CYAN_NEON;

            GameObject itemLabel = FindOrCreateChild(item, "Item Label");
            RectTransform itemLabelRT = GetOrAddComponent<RectTransform>(itemLabel);
            itemLabelRT.anchorMin = Vector2.zero;
            itemLabelRT.anchorMax = Vector2.one;
            itemLabelRT.offsetMin = new Vector2(30, 0);
            itemLabelRT.offsetMax = new Vector2(-8, 0);
            TextMeshProUGUI itemLabelText = GetOrAddComponent<TextMeshProUGUI>(itemLabel);
            itemLabelText.fontSize = 40;
            itemLabelText.color = TEXT_PRIMARY;
            itemLabelText.alignment = TextAlignmentOptions.Left;

            itemToggle.targetGraphic = itemBgImg;
            itemToggle.graphic = checkImg;
            itemToggle.isOn = true;
            dropdown.template = templateRT;
            dropdown.itemText = itemLabelText;

            template.SetActive(false);
        }

        // ==================== REFRESH INDICATOR ====================

        private static void CreateRefreshIndicator(GameObject parent)
        {
            GameObject refresh = FindOrCreateChild(parent, "RefreshIndicator");
            refresh.SetActive(false);

            RectTransform refreshRT = GetOrAddComponent<RectTransform>(refresh);
            refreshRT.anchorMin = new Vector2(0.3f, 0.5f);
            refreshRT.anchorMax = new Vector2(0.7f, 0.5f);
            refreshRT.sizeDelta = new Vector2(0, 40);
            refreshRT.anchoredPosition = Vector2.zero;

            Image refreshBg = GetOrAddComponent<Image>(refresh);
            refreshBg.color = new Color(0, 0, 0, 0.5f);

            GameObject textObj = FindOrCreateChild(refresh, "Text");
            TextMeshProUGUI refreshText = GetOrAddComponent<TextMeshProUGUI>(textObj);
            refreshText.text = "Actualizando...";
            refreshText.fontSize = 40;
            refreshText.color = CYAN_NEON;
            refreshText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(textObj);

            EditorUtility.SetDirty(refresh);
            Debug.Log("[TournamentsBrowserUIBuilder] RefreshIndicator creado");
        }

        // ==================== LOAD MORE BUTTON ====================

        private static void CreateLoadMoreButton(GameObject parent)
        {
            GameObject loadMore = FindOrCreateChild(parent, "LoadMoreButton");
            loadMore.SetActive(false);

            RectTransform loadMoreRT = GetOrAddComponent<RectTransform>(loadMore);
            loadMoreRT.anchorMin = new Vector2(0.2f, 0);
            loadMoreRT.anchorMax = new Vector2(0.8f, 0);
            loadMoreRT.pivot = new Vector2(0.5f, 0);
            loadMoreRT.anchoredPosition = new Vector2(0, 80);
            loadMoreRT.sizeDelta = new Vector2(0, 50);

            Image loadMoreBg = GetOrAddComponent<Image>(loadMore);
            loadMoreBg.color = BUTTON_SECONDARY;
            Button loadMoreBtn = GetOrAddComponent<Button>(loadMore);
            SetupButtonColors(loadMoreBtn, BUTTON_SECONDARY);
            AddOutline(loadMore, CYAN_DARK);

            GameObject textObj = FindOrCreateChild(loadMore, "Text");
            TextMeshProUGUI loadMoreText = GetOrAddComponent<TextMeshProUGUI>(textObj);
            loadMoreText.text = "Cargar Mas";
            loadMoreText.fontSize = 45;
            loadMoreText.color = TEXT_PRIMARY;
            loadMoreText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(textObj);

            EditorUtility.SetDirty(loadMore);
            Debug.Log("[TournamentsBrowserUIBuilder] LoadMoreButton creado");
        }

        // ==================== LOADING INDICATOR ====================

        private static void CreateLoadingIndicator(GameObject parent)
        {
            GameObject loading = FindOrCreateChild(parent, "LoadingIndicator");
            loading.SetActive(false);

            RectTransform loadingRT = GetOrAddComponent<RectTransform>(loading);
            loadingRT.anchorMin = new Vector2(0.5f, 0.5f);
            loadingRT.anchorMax = new Vector2(0.5f, 0.5f);
            loadingRT.sizeDelta = new Vector2(150, 150);
            loadingRT.anchoredPosition = Vector2.zero;

            // Spinner
            GameObject spinner = FindOrCreateChild(loading, "Spinner");
            RectTransform spinnerRT = GetOrAddComponent<RectTransform>(spinner);
            spinnerRT.anchorMin = new Vector2(0.5f, 0.6f);
            spinnerRT.anchorMax = new Vector2(0.5f, 0.6f);
            spinnerRT.sizeDelta = new Vector2(70, 70);

            Image spinnerImage = GetOrAddComponent<Image>(spinner);
            spinnerImage.color = CYAN_NEON;
            AddOutline(spinner, CYAN_GLOW, 3);

            // Text
            GameObject textObj = FindOrCreateChild(loading, "Text");
            RectTransform textRT = GetOrAddComponent<RectTransform>(textObj);
            textRT.anchorMin = new Vector2(0.5f, 0.2f);
            textRT.anchorMax = new Vector2(0.5f, 0.2f);
            textRT.sizeDelta = new Vector2(200, 30);

            TextMeshProUGUI loadingText = GetOrAddComponent<TextMeshProUGUI>(textObj);
            loadingText.text = "Cargando...";
            loadingText.fontSize = 45;
            loadingText.color = TEXT_SECONDARY;
            loadingText.alignment = TextAlignmentOptions.Center;

            EditorUtility.SetDirty(loading);
            Debug.Log("[TournamentsBrowserUIBuilder] LoadingIndicator creado");
        }

        // ==================== FAB (Floating Action Button) ====================

        private static void CreateFAB(GameObject parent)
        {
            GameObject fab = FindOrCreateChild(parent, "CreateTournamentFAB");

            RectTransform fabRT = GetOrAddComponent<RectTransform>(fab);
            fabRT.anchorMin = new Vector2(1, 0);
            fabRT.anchorMax = new Vector2(1, 0);
            fabRT.pivot = new Vector2(1, 0);
            fabRT.anchoredPosition = new Vector2(-25, 25);
            fabRT.sizeDelta = new Vector2(FAB_SIZE, FAB_SIZE);

            Image fabBg = GetOrAddComponent<Image>(fab);
            fabBg.color = BUTTON_PRIMARY;

            Button fabButton = GetOrAddComponent<Button>(fab);
            SetupButtonColors(fabButton, BUTTON_PRIMARY);
            AddOutline(fab, CYAN_GLOW, 3);

            // Plus Icon
            GameObject plusObj = FindOrCreateChild(fab, "PlusIcon");
            TextMeshProUGUI plusText = GetOrAddComponent<TextMeshProUGUI>(plusObj);
            plusText.text = "+";
            plusText.fontSize = 78;
            plusText.fontStyle = FontStyles.Bold;
            plusText.color = TEXT_DARK;
            plusText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(plusObj);

            EditorUtility.SetDirty(fab);
            Debug.Log("[TournamentsBrowserUIBuilder] FAB creado");
        }

        // ==================== UTILITY METHODS ====================

        private static void CreateBottomGlow(GameObject obj)
        {
            GameObject glow = FindOrCreateChild(obj, "BottomGlow");

            RectTransform glowRT = GetOrAddComponent<RectTransform>(glow);
            glowRT.anchorMin = new Vector2(0, 0);
            glowRT.anchorMax = new Vector2(1, 0);
            glowRT.pivot = new Vector2(0.5f, 1);
            glowRT.anchoredPosition = Vector2.zero;
            glowRT.sizeDelta = new Vector2(0, 3);

            Image glowImage = GetOrAddComponent<Image>(glow);
            glowImage.color = CYAN_NEON;
        }

        private static void CreateInputField(GameObject obj, string placeholder)
        {
            TMP_InputField inputField = obj.GetComponent<TMP_InputField>();

            if (inputField == null)
            {
                inputField = obj.AddComponent<TMP_InputField>();

                GameObject textArea = FindOrCreateChild(obj, "Text Area");
                RectTransform textAreaRT = GetOrAddComponent<RectTransform>(textArea);
                SetRectTransformStretch(textArea);
                GetOrAddComponent<RectMask2D>(textArea);

                GameObject placeholderObj = FindOrCreateChild(textArea, "Placeholder");
                TextMeshProUGUI placeholderText = GetOrAddComponent<TextMeshProUGUI>(placeholderObj);
                placeholderText.text = placeholder;
                placeholderText.fontSize = 40;
                placeholderText.fontStyle = FontStyles.Bold;
                placeholderText.color = TEXT_MUTED;
                placeholderText.alignment = TextAlignmentOptions.MidlineLeft;
                SetRectTransformStretch(placeholderObj);

                GameObject textObj = FindOrCreateChild(textArea, "Text");
                TextMeshProUGUI textComponent = GetOrAddComponent<TextMeshProUGUI>(textObj);
                textComponent.fontSize = 40;
                textComponent.color = TEXT_PRIMARY;
                textComponent.alignment = TextAlignmentOptions.MidlineLeft;
                SetRectTransformStretch(textObj);

                inputField.textViewport = textAreaRT;
                inputField.textComponent = textComponent;
                inputField.placeholder = placeholderText;
            }

            inputField.caretColor = CYAN_NEON;
            inputField.selectionColor = new Color(0f, 1f, 1f, 0.3f);

            Image inputBg = GetOrAddComponent<Image>(obj);
            inputBg.color = Color.clear;
        }

        private static void SetRectTransformStretch(GameObject obj)
        {
            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt == null) rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }

        private static T GetOrAddComponent<T>(GameObject obj) where T : Component
        {
            T component = obj.GetComponent<T>();
            if (component == null)
                component = obj.AddComponent<T>();
            return component;
        }

        private static GameObject FindOrCreateChild(GameObject parent, string childName)
        {
            Transform child = parent.transform.Find(childName);
            if (child != null) return child.gameObject;

            GameObject newChild = new GameObject(childName);
            newChild.transform.SetParent(parent.transform, false);

            if (newChild.GetComponent<RectTransform>() == null)
                newChild.AddComponent<RectTransform>();

            return newChild;
        }

        private static void SetupButtonColors(Button btn, Color baseColor)
        {
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            btn.colors = colors;
        }

        private static void AddOutline(GameObject obj, Color color, float distance = 1)
        {
            Outline outline = obj.GetComponent<Outline>();
            if (outline == null)
                outline = obj.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(distance, distance);
        }

        private static void MarkSceneDirty()
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
    }
}
