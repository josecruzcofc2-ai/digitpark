using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para rediseñar completamente la UI de Tournaments con tema neon profesional
    /// Incluye: header, tabs, searchbar, empty state, popups mejorados y más
    /// </summary>
    public class TournamentsUIBuilder : EditorWindow
    {
        // ==================== COLORES DEL TEMA NEON ====================
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.4f, 1f);
        private static readonly Color CYAN_GLOW = new Color(0f, 1f, 1f, 0.3f);
        private static readonly Color CYAN_SUBTLE = new Color(0f, 0.6f, 0.6f, 1f);

        private static readonly Color DARK_BG = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color PANEL_BG = new Color(0.08f, 0.12f, 0.18f, 0.98f);
        private static readonly Color POPUP_BG = new Color(0.05f, 0.08f, 0.12f, 0.98f);
        private static readonly Color INPUT_BG = new Color(0.04f, 0.08f, 0.12f, 1f);
        private static readonly Color HEADER_BG = new Color(0.03f, 0.06f, 0.1f, 0.95f);

        private static readonly Color TEXT_PRIMARY = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.7f, 0.75f, 1f);
        private static readonly Color TEXT_DARK = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color TEXT_MUTED = new Color(0.4f, 0.5f, 0.55f, 1f);

        private static readonly Color BUTTON_PRIMARY = CYAN_NEON;
        private static readonly Color BUTTON_SECONDARY = new Color(0.15f, 0.2f, 0.25f, 1f);
        private static readonly Color BUTTON_DANGER = new Color(0.9f, 0.2f, 0.2f, 1f);
        private static readonly Color BUTTON_SUCCESS = new Color(0.2f, 0.8f, 0.4f, 1f);

        private static readonly Color TAB_ACTIVE = CYAN_NEON;
        private static readonly Color TAB_INACTIVE = new Color(0.12f, 0.16f, 0.2f, 1f);

        private static readonly Color BLOCKER_BG = new Color(0f, 0f, 0f, 0.85f);

        // ==================== DIMENSIONES ====================
        private const float HEADER_HEIGHT = 80f;
        private const float TABS_HEIGHT = 50f;
        private const float SEARCHBAR_HEIGHT = 60f;
        private const float TAB_SPACING = 8f;
        private const float CONTENT_PADDING = 20f;

        // ==================== MENU ITEMS ====================

        [MenuItem("DigitPark/Tournaments/Rebuild Complete UI", false, 0)]
        public static void RebuildCompleteUI()
        {
            if (!ConfirmAction("Esto reconstruirá TODA la UI de Tournaments. ¿Continuar?"))
                return;

            RedesignAll();
        }

        [MenuItem("DigitPark/Tournaments/Open Builder Window", false, 1)]
        public static void ShowWindow()
        {
            GetWindow<TournamentsUIBuilder>("Tournaments UI Builder");
        }

        [MenuItem("DigitPark/Tournaments/Quick Fix - Colors Only", false, 20)]
        public static void QuickFixColors()
        {
            FixAllColors();
            MarkSceneDirty();
            Debug.Log("[TournamentsUIBuilder] Colores arreglados!");
        }

        // ==================== EDITOR WINDOW ====================

        private Vector2 scrollPos;

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            GUILayout.Label("Tournaments UI Builder", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Constructor completo de UI para la escena Tournaments.\n" +
                "Asegurate de tener la escena Tournaments abierta.\n" +
                "GUARDA LA ESCENA después de cada operación (Ctrl+S)",
                MessageType.Info);

            GUILayout.Space(15);

            // ===== SECCIÓN: RECONSTRUIR TODO =====
            GUI.backgroundColor = CYAN_NEON;
            if (GUILayout.Button("RECONSTRUIR TODO", GUILayout.Height(45)))
            {
                if (ConfirmAction("¿Reconstruir toda la UI?"))
                    RedesignAll();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(20);

            // ===== SECCIÓN: CREAR COMPONENTES =====
            GUILayout.Label("Crear Componentes Nuevos", EditorStyles.boldLabel);

            if (GUILayout.Button("1. Crear Header", GUILayout.Height(28)))
                CreateHeader();

            if (GUILayout.Button("2. Reorganizar Tabs", GUILayout.Height(28)))
                ReorganizeTabs();

            if (GUILayout.Button("3. Crear SearchBar", GUILayout.Height(28)))
                CreateSearchBar();

            if (GUILayout.Button("4. Crear EmptyState", GUILayout.Height(28)))
                CreateEmptyState();

            if (GUILayout.Button("5. Crear LoadingIndicator", GUILayout.Height(28)))
                CreateLoadingIndicator();

            if (GUILayout.Button("6. Crear ModalBlocker Unificado", GUILayout.Height(28)))
                CreateUnifiedModalBlocker();

            GUILayout.Space(15);

            // ===== SECCIÓN: MEJORAR EXISTENTES =====
            GUILayout.Label("Mejorar Componentes Existentes", EditorStyles.boldLabel);

            if (GUILayout.Button("Mejorar Todos los Popups", GUILayout.Height(28)))
                ImproveAllPopups();

            if (GUILayout.Button("Arreglar Todos los Colores", GUILayout.Height(28)))
                FixAllColors();

            if (GUILayout.Button("Arreglar ScrollView", GUILayout.Height(28)))
                FixScrollViews();

            GUILayout.Space(15);

            // ===== SECCIÓN: REORGANIZAR =====
            GUILayout.Label("Reorganizar Hierarchy", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "CUIDADO: Reorganizar puede romper referencias del TournamentManager.\n" +
                "Solo usar si vas a re-asignar referencias manualmente.",
                MessageType.Warning);

            GUI.backgroundColor = new Color(1f, 0.8f, 0.2f);
            if (GUILayout.Button("Reorganizar Hierarchy (Avanzado)", GUILayout.Height(28)))
            {
                if (ConfirmAction("¿Reorganizar hierarchy? Puede romper referencias."))
                    ReorganizeHierarchy();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndScrollView();
        }

        // ==================== REDESIGN ALL ====================

        private static void RedesignAll()
        {
            Debug.Log("[TournamentsUIBuilder] ========== INICIANDO REDISEÑO COMPLETO ==========");

            // 1. Crear/Mejorar estructura
            CreateHeader();
            ReorganizeTabs();
            CreateSearchBar();
            CreateEmptyState();
            CreateLoadingIndicator();
            CreateUnifiedModalBlocker();

            // 2. Mejorar componentes existentes
            ImproveAllPopups();
            FixScrollViews();
            FixAllColors();

            // 3. Arreglar botones específicos
            FixSearchOptionsButton();

            // 4. Limpiar botones redundantes
            CleanupRedundantButtons();

            MarkSceneDirty();
            Debug.Log("[TournamentsUIBuilder] ========== REDISEÑO COMPLETO TERMINADO ==========");
        }

        // Arreglar el botón "Buscar Torneo" que está muy grande
        private static void FixSearchOptionsButton()
        {
            // Buscar el botón SearchOptionsButton o similar
            string[] buttonNames = { "SearchOptionsButton", "BuscarTorneoButton", "SearchButton" };

            foreach (string btnName in buttonNames)
            {
                GameObject btnObj = GameObject.Find(btnName);
                if (btnObj != null)
                {
                    Button btn = btnObj.GetComponent<Button>();
                    if (btn != null)
                    {
                        // Hacer el botón más pequeño
                        RectTransform btnRT = btn.GetComponent<RectTransform>();
                        if (btnRT != null)
                        {
                            // Reducir tamaño
                            btnRT.sizeDelta = new Vector2(
                                Mathf.Min(btnRT.sizeDelta.x, 140),
                                Mathf.Min(btnRT.sizeDelta.y, 45)
                            );
                            EditorUtility.SetDirty(btnRT);
                        }

                        // Estilo secundario (no tan prominente)
                        Image btnBg = btn.GetComponent<Image>();
                        if (btnBg != null)
                        {
                            btnBg.color = BUTTON_SECONDARY;
                            EditorUtility.SetDirty(btnBg);
                        }

                        SetupButtonColors(btn, BUTTON_SECONDARY);
                        AddOutline(btnObj, CYAN_DARK);

                        TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
                        if (btnText != null)
                        {
                            btnText.color = TEXT_PRIMARY;
                            btnText.fontSize = 14;
                            EditorUtility.SetDirty(btnText);
                        }

                        Debug.Log($"[TournamentsUIBuilder] Botón '{btnName}' arreglado");
                    }
                }
            }
        }

        // ==================== 1. CREATE HEADER ====================

        private static void CreateHeader()
        {
            Canvas canvas = GetCanvas();
            if (canvas == null) return;

            // Buscar o crear Header
            Transform existingHeader = canvas.transform.Find("Header");
            GameObject header;

            if (existingHeader != null)
            {
                header = existingHeader.gameObject;
                Debug.Log("[TournamentsUIBuilder] Header existente encontrado, actualizando...");
            }
            else
            {
                header = new GameObject("Header");
                header.transform.SetParent(canvas.transform, false);
                header.transform.SetAsFirstSibling();

                // Mover después del Background si existe
                Transform bg = canvas.transform.Find("Background");
                if (bg != null)
                    header.transform.SetSiblingIndex(bg.GetSiblingIndex() + 1);
            }

            // RectTransform del Header
            RectTransform headerRT = header.GetComponent<RectTransform>();
            if (headerRT == null) headerRT = header.AddComponent<RectTransform>();

            headerRT.anchorMin = new Vector2(0, 1);
            headerRT.anchorMax = new Vector2(1, 1);
            headerRT.pivot = new Vector2(0.5f, 1);
            headerRT.anchoredPosition = Vector2.zero;
            headerRT.sizeDelta = new Vector2(0, HEADER_HEIGHT);

            // Fondo del Header
            Image headerBg = header.GetComponent<Image>();
            if (headerBg == null) headerBg = header.AddComponent<Image>();
            headerBg.color = HEADER_BG;

            // Agregar glow inferior
            AddBottomGlow(header);

            // === BackButton ===
            GameObject backBtn = FindOrCreateChild(header, "BackButton");
            RectTransform backRT = GetOrAddComponent<RectTransform>(backBtn);
            backRT.anchorMin = new Vector2(0, 0.5f);
            backRT.anchorMax = new Vector2(0, 0.5f);
            backRT.pivot = new Vector2(0, 0.5f);
            backRT.anchoredPosition = new Vector2(15, 0);
            backRT.sizeDelta = new Vector2(50, 50);

            Image backBg = GetOrAddComponent<Image>(backBtn);
            backBg.color = BUTTON_SECONDARY;

            Button backButton = GetOrAddComponent<Button>(backBtn);
            SetupButtonColors(backButton, BUTTON_SECONDARY);

            // Texto del BackButton
            GameObject backTextObj = FindOrCreateChild(backBtn, "Text");
            TextMeshProUGUI backText = GetOrAddComponent<TextMeshProUGUI>(backTextObj);
            backText.text = "<";
            backText.fontSize = 28;
            backText.fontStyle = FontStyles.Bold;
            backText.color = CYAN_NEON;
            backText.alignment = TextAlignmentOptions.Center;

            RectTransform backTextRT = backTextObj.GetComponent<RectTransform>();
            backTextRT.anchorMin = Vector2.zero;
            backTextRT.anchorMax = Vector2.one;
            backTextRT.sizeDelta = Vector2.zero;
            backTextRT.anchoredPosition = Vector2.zero;

            AddOutline(backBtn, CYAN_DARK);

            // === Title ===
            GameObject titleObj = FindOrCreateChild(header, "TitleText");
            RectTransform titleRT = GetOrAddComponent<RectTransform>(titleObj);
            titleRT.anchorMin = new Vector2(0.5f, 0.5f);
            titleRT.anchorMax = new Vector2(0.5f, 0.5f);
            titleRT.pivot = new Vector2(0.5f, 0.5f);
            titleRT.anchoredPosition = Vector2.zero;
            titleRT.sizeDelta = new Vector2(300, 50);

            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "TORNEOS";
            titleText.fontSize = 36;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = CYAN_NEON;
            titleText.alignment = TextAlignmentOptions.Center;

            // Glow en el título
            AddOutline(titleObj, CYAN_GLOW, 2);

            // === Info Button (opcional) ===
            GameObject infoBtn = FindOrCreateChild(header, "InfoButton");
            RectTransform infoRT = GetOrAddComponent<RectTransform>(infoBtn);
            infoRT.anchorMin = new Vector2(1, 0.5f);
            infoRT.anchorMax = new Vector2(1, 0.5f);
            infoRT.pivot = new Vector2(1, 0.5f);
            infoRT.anchoredPosition = new Vector2(-15, 0);
            infoRT.sizeDelta = new Vector2(45, 45);

            Image infoBg = GetOrAddComponent<Image>(infoBtn);
            infoBg.color = BUTTON_SECONDARY;

            Button infoButton = GetOrAddComponent<Button>(infoBtn);
            SetupButtonColors(infoButton, BUTTON_SECONDARY);

            GameObject infoTextObj = FindOrCreateChild(infoBtn, "Text");
            TextMeshProUGUI infoText = GetOrAddComponent<TextMeshProUGUI>(infoTextObj);
            infoText.text = "?";
            infoText.fontSize = 24;
            infoText.fontStyle = FontStyles.Bold;
            infoText.color = TEXT_SECONDARY;
            infoText.alignment = TextAlignmentOptions.Center;

            RectTransform infoTextRT = infoTextObj.GetComponent<RectTransform>();
            infoTextRT.anchorMin = Vector2.zero;
            infoTextRT.anchorMax = Vector2.one;
            infoTextRT.sizeDelta = Vector2.zero;

            AddOutline(infoBtn, CYAN_DARK);

            EditorUtility.SetDirty(header);
            MarkSceneDirty();
            Debug.Log("[TournamentsUIBuilder] Header creado/actualizado");
        }

        private static void AddBottomGlow(GameObject obj)
        {
            Transform glowTransform = obj.transform.Find("BottomGlow");
            GameObject glow;

            if (glowTransform != null)
                glow = glowTransform.gameObject;
            else
            {
                glow = new GameObject("BottomGlow");
                glow.transform.SetParent(obj.transform, false);
            }

            RectTransform glowRT = GetOrAddComponent<RectTransform>(glow);
            glowRT.anchorMin = new Vector2(0, 0);
            glowRT.anchorMax = new Vector2(1, 0);
            glowRT.pivot = new Vector2(0.5f, 1);
            glowRT.anchoredPosition = Vector2.zero;
            glowRT.sizeDelta = new Vector2(0, 3);

            Image glowImage = GetOrAddComponent<Image>(glow);
            glowImage.color = CYAN_NEON;

            Outline glowOutline = GetOrAddComponent<Outline>(glow);
            glowOutline.effectColor = CYAN_GLOW;
            glowOutline.effectDistance = new Vector2(0, 2);
        }

        // ==================== 2. REORGANIZE TABS ====================

        private static void ReorganizeTabs()
        {
            GameObject tabsPanel = GameObject.Find("TabsPanel");
            if (tabsPanel == null)
            {
                Debug.LogWarning("[TournamentsUIBuilder] TabsPanel no encontrado");
                return;
            }

            // Ajustar posición del TabsPanel (debajo del header)
            RectTransform tabsRT = tabsPanel.GetComponent<RectTransform>();
            if (tabsRT != null)
            {
                tabsRT.anchorMin = new Vector2(0, 1);
                tabsRT.anchorMax = new Vector2(1, 1);
                tabsRT.pivot = new Vector2(0.5f, 1);
                tabsRT.anchoredPosition = new Vector2(0, -HEADER_HEIGHT);
                tabsRT.sizeDelta = new Vector2(-CONTENT_PADDING * 2, TABS_HEIGHT);
            }

            // Fondo del TabsPanel
            Image tabsBg = tabsPanel.GetComponent<Image>();
            if (tabsBg == null) tabsBg = tabsPanel.AddComponent<Image>();
            tabsBg.color = new Color(0.04f, 0.07f, 0.11f, 0.9f);

            // Agregar HorizontalLayoutGroup para espaciado automático
            HorizontalLayoutGroup hlg = tabsPanel.GetComponent<HorizontalLayoutGroup>();
            if (hlg == null) hlg = tabsPanel.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = TAB_SPACING;
            hlg.padding = new RectOffset(5, 5, 5, 5);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // Eliminar ActiveTabIndicator mal posicionado si existe
            Transform badIndicator = tabsPanel.transform.Find("ActiveTabIndicator");
            if (badIndicator != null)
            {
                Object.DestroyImmediate(badIndicator.gameObject);
            }

            // Estilizar cada tab
            Button[] tabs = tabsPanel.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < tabs.Length; i++)
            {
                Button tab = tabs[i];
                string tabName = tab.name.ToLower();

                // Fondo del tab
                Image tabBg = tab.GetComponent<Image>();
                if (tabBg != null)
                {
                    tabBg.color = TAB_INACTIVE;
                }

                // Estilo especial para "Crear Torneo"
                bool isCreateTab = tabName.Contains("create") || tabName.Contains("crear");

                if (isCreateTab)
                {
                    // Tab de crear es especial - fondo cyan oscuro pero mismo glow que otros
                    if (tabBg != null) tabBg.color = CYAN_DARK;
                    SetupButtonColors(tab, CYAN_DARK);

                    // Agregar icono "+" si no existe
                    TextMeshProUGUI createText = tab.GetComponentInChildren<TextMeshProUGUI>();
                    if (createText != null && !createText.text.Contains("+"))
                    {
                        createText.text = "+ " + createText.text;
                    }

                    // Mismo outline cyan que los otros tabs
                    AddOutline(tab.gameObject, CYAN_DARK);
                }
                else
                {
                    SetupButtonColors(tab, TAB_INACTIVE);
                    // Outline cyan para tabs normales
                    AddOutline(tab.gameObject, CYAN_DARK);
                }

                // Texto del tab
                TextMeshProUGUI tabText = tab.GetComponentInChildren<TextMeshProUGUI>();
                if (tabText != null)
                {
                    tabText.color = TEXT_PRIMARY;
                    tabText.fontStyle = FontStyles.Bold;
                    tabText.fontSize = 14;
                }

                // LayoutElement para control de tamaño
                LayoutElement le = tab.GetComponent<LayoutElement>();
                if (le == null) le = tab.gameObject.AddComponent<LayoutElement>();
                le.minHeight = 40;
                le.flexibleWidth = 1;

                EditorUtility.SetDirty(tab);
            }

            // Crear indicador de tab activo
            CreateTabIndicator(tabsPanel);

            EditorUtility.SetDirty(tabsPanel);
            MarkSceneDirty();
            Debug.Log("[TournamentsUIBuilder] Tabs reorganizados");
        }

        private static void CreateTabIndicator(GameObject tabsPanel)
        {
            Transform existing = tabsPanel.transform.Find("ActiveIndicator");
            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);

            GameObject indicator = new GameObject("ActiveIndicator");
            indicator.transform.SetParent(tabsPanel.transform, false);
            indicator.transform.SetAsLastSibling();

            RectTransform rt = indicator.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0.33f, 0);
            rt.pivot = new Vector2(0, 0);
            rt.anchoredPosition = new Vector2(5, 2);
            rt.sizeDelta = new Vector2(-10, 3);

            Image img = indicator.AddComponent<Image>();
            img.color = CYAN_NEON;

            // El LayoutGroup lo ignorará
            LayoutElement le = indicator.AddComponent<LayoutElement>();
            le.ignoreLayout = true;

            AddOutline(indicator, CYAN_GLOW, 1);

            Debug.Log("[TournamentsUIBuilder] Tab indicator creado");
        }

        // ==================== 3. CREATE SEARCHBAR ====================

        private static void CreateSearchBar()
        {
            Canvas canvas = GetCanvas();
            if (canvas == null) return;

            // Buscar TournamentsPanel o MainContent
            Transform parent = canvas.transform.Find("TournamentsPanel");
            if (parent == null) parent = canvas.transform.Find("MainContent");
            if (parent == null)
            {
                Debug.LogWarning("[TournamentsUIBuilder] No se encontró contenedor para SearchBar");
                return;
            }

            // Buscar o crear SearchBar
            Transform existing = parent.Find("SearchBar");
            GameObject searchBar;

            if (existing != null)
            {
                searchBar = existing.gameObject;
            }
            else
            {
                searchBar = new GameObject("SearchBar");
                searchBar.transform.SetParent(parent, false);

                // Posicionar después de los tabs
                Transform tabs = parent.Find("TabsPanel");
                if (tabs != null)
                    searchBar.transform.SetSiblingIndex(tabs.GetSiblingIndex() + 1);
            }

            // RectTransform
            RectTransform searchRT = GetOrAddComponent<RectTransform>(searchBar);
            searchRT.anchorMin = new Vector2(0, 1);
            searchRT.anchorMax = new Vector2(1, 1);
            searchRT.pivot = new Vector2(0.5f, 1);
            searchRT.anchoredPosition = new Vector2(0, -(HEADER_HEIGHT + TABS_HEIGHT + 10));
            searchRT.sizeDelta = new Vector2(-CONTENT_PADDING * 2, SEARCHBAR_HEIGHT);

            // Fondo
            Image searchBg = GetOrAddComponent<Image>(searchBar);
            searchBg.color = INPUT_BG;
            AddOutline(searchBar, CYAN_DARK);

            // HorizontalLayoutGroup
            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(searchBar);
            hlg.spacing = 10;
            hlg.padding = new RectOffset(15, 15, 10, 10);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            // === Search Icon (Image component para sprite de lupa) ===
            GameObject iconObj = FindOrCreateChild(searchBar, "SearchIcon");
            RectTransform iconRT = GetOrAddComponent<RectTransform>(iconObj);
            iconRT.sizeDelta = new Vector2(35, 35);

            // Remover TextMeshProUGUI si existe
            TextMeshProUGUI oldText = iconObj.GetComponent<TextMeshProUGUI>();
            if (oldText != null) Object.DestroyImmediate(oldText);

            // Usar Image component para el sprite de lupa
            Image iconImage = GetOrAddComponent<Image>(iconObj);
            iconImage.color = TEXT_SECONDARY;
            // El usuario asignará el sprite de lupa manualmente

            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(iconObj);
            iconLE.minWidth = 35;
            iconLE.preferredWidth = 35;

            // === Search Input ===
            GameObject inputObj = FindOrCreateChild(searchBar, "SearchInput");

            // Verificar si ya tiene TMP_InputField
            TMP_InputField inputField = inputObj.GetComponent<TMP_InputField>();
            if (inputField == null)
            {
                // Crear estructura completa del InputField
                inputField = inputObj.AddComponent<TMP_InputField>();

                // Text Area
                GameObject textArea = FindOrCreateChild(inputObj, "Text Area");
                RectTransform textAreaRT = GetOrAddComponent<RectTransform>(textArea);
                textAreaRT.anchorMin = Vector2.zero;
                textAreaRT.anchorMax = Vector2.one;
                textAreaRT.sizeDelta = Vector2.zero;

                RectMask2D mask = GetOrAddComponent<RectMask2D>(textArea);

                // Placeholder
                GameObject placeholder = FindOrCreateChild(textArea, "Placeholder");
                TextMeshProUGUI placeholderText = GetOrAddComponent<TextMeshProUGUI>(placeholder);
                placeholderText.text = "Buscar torneos...";
                placeholderText.fontSize = 16;
                placeholderText.fontStyle = FontStyles.Italic;
                placeholderText.color = TEXT_MUTED;
                placeholderText.alignment = TextAlignmentOptions.MidlineLeft;

                RectTransform placeholderRT = placeholder.GetComponent<RectTransform>();
                placeholderRT.anchorMin = Vector2.zero;
                placeholderRT.anchorMax = Vector2.one;
                placeholderRT.sizeDelta = Vector2.zero;

                // Text
                GameObject text = FindOrCreateChild(textArea, "Text");
                TextMeshProUGUI textComponent = GetOrAddComponent<TextMeshProUGUI>(text);
                textComponent.fontSize = 16;
                textComponent.color = TEXT_PRIMARY;
                textComponent.alignment = TextAlignmentOptions.MidlineLeft;

                RectTransform textRT = text.GetComponent<RectTransform>();
                textRT.anchorMin = Vector2.zero;
                textRT.anchorMax = Vector2.one;
                textRT.sizeDelta = Vector2.zero;

                // Configurar InputField
                inputField.textViewport = textAreaRT;
                inputField.textComponent = textComponent;
                inputField.placeholder = placeholderText;
                inputField.fontAsset = textComponent.font;
            }

            inputField.caretColor = CYAN_NEON;
            inputField.selectionColor = new Color(0f, 1f, 1f, 0.3f);

            Image inputBg = GetOrAddComponent<Image>(inputObj);
            inputBg.color = Color.clear;

            LayoutElement inputLE = GetOrAddComponent<LayoutElement>(inputObj);
            inputLE.flexibleWidth = 1;
            inputLE.minWidth = 250; // Más grande para más espacio
            inputLE.preferredWidth = 400;

            // === Filter Button (más pequeño) ===
            GameObject filterBtn = FindOrCreateChild(searchBar, "FilterButton");
            RectTransform filterRT = GetOrAddComponent<RectTransform>(filterBtn);
            filterRT.sizeDelta = new Vector2(80, 35); // Más pequeño

            Image filterBg = GetOrAddComponent<Image>(filterBtn);
            filterBg.color = BUTTON_SECONDARY;

            Button filterButton = GetOrAddComponent<Button>(filterBtn);
            SetupButtonColors(filterButton, BUTTON_SECONDARY);
            AddOutline(filterBtn, CYAN_DARK);

            GameObject filterTextObj = FindOrCreateChild(filterBtn, "Text");
            TextMeshProUGUI filterText = GetOrAddComponent<TextMeshProUGUI>(filterTextObj);
            filterText.text = "Filtros";
            filterText.fontSize = 14;
            filterText.fontStyle = FontStyles.Bold;
            filterText.color = TEXT_PRIMARY;
            filterText.alignment = TextAlignmentOptions.Center;

            RectTransform filterTextRT = filterTextObj.GetComponent<RectTransform>();
            filterTextRT.anchorMin = Vector2.zero;
            filterTextRT.anchorMax = Vector2.one;
            filterTextRT.sizeDelta = Vector2.zero;

            LayoutElement filterLE = GetOrAddComponent<LayoutElement>(filterBtn);
            filterLE.minWidth = 80;
            filterLE.preferredWidth = 80;

            // === Leaderboard Button (oculto por defecto, visible cuando está en torneo) ===
            GameObject leaderboardBtn = FindOrCreateChild(searchBar, "LeaderboardButton");
            leaderboardBtn.SetActive(false); // Oculto por defecto

            RectTransform leaderboardRT = GetOrAddComponent<RectTransform>(leaderboardBtn);
            leaderboardRT.sizeDelta = new Vector2(110, 35);

            Image leaderboardBg = GetOrAddComponent<Image>(leaderboardBtn);
            leaderboardBg.color = BUTTON_SECONDARY;

            Button leaderboardButton = GetOrAddComponent<Button>(leaderboardBtn);
            SetupButtonColors(leaderboardButton, BUTTON_SECONDARY);
            AddOutline(leaderboardBtn, CYAN_DARK);

            GameObject leaderboardTextObj = FindOrCreateChild(leaderboardBtn, "Text");
            TextMeshProUGUI leaderboardText = GetOrAddComponent<TextMeshProUGUI>(leaderboardTextObj);
            leaderboardText.text = "Ranking";
            leaderboardText.fontSize = 13;
            leaderboardText.fontStyle = FontStyles.Bold;
            leaderboardText.color = TEXT_PRIMARY;
            leaderboardText.alignment = TextAlignmentOptions.Center;

            RectTransform leaderboardTextRT = leaderboardTextObj.GetComponent<RectTransform>();
            leaderboardTextRT.anchorMin = Vector2.zero;
            leaderboardTextRT.anchorMax = Vector2.one;
            leaderboardTextRT.sizeDelta = Vector2.zero;

            LayoutElement leaderboardLE = GetOrAddComponent<LayoutElement>(leaderboardBtn);
            leaderboardLE.minWidth = 100;
            leaderboardLE.preferredWidth = 110;

            // === Exit Tournament Button (oculto por defecto, visible cuando está en torneo) ===
            GameObject exitBtn = FindOrCreateChild(searchBar, "ExitTournamentButton");
            exitBtn.SetActive(false); // Oculto por defecto

            RectTransform exitRT = GetOrAddComponent<RectTransform>(exitBtn);
            exitRT.sizeDelta = new Vector2(120, 35);

            Image exitBg = GetOrAddComponent<Image>(exitBtn);
            exitBg.color = BUTTON_DANGER;

            Button exitButton = GetOrAddComponent<Button>(exitBtn);
            SetupButtonColors(exitButton, BUTTON_DANGER);
            AddOutline(exitBtn, new Color(1f, 0.3f, 0.3f, 0.5f)); // Glow rojo sutil

            GameObject exitTextObj = FindOrCreateChild(exitBtn, "Text");
            TextMeshProUGUI exitText = GetOrAddComponent<TextMeshProUGUI>(exitTextObj);
            exitText.text = "Salir";
            exitText.fontSize = 13;
            exitText.fontStyle = FontStyles.Bold;
            exitText.color = TEXT_PRIMARY;
            exitText.alignment = TextAlignmentOptions.Center;

            RectTransform exitTextRT = exitTextObj.GetComponent<RectTransform>();
            exitTextRT.anchorMin = Vector2.zero;
            exitTextRT.anchorMax = Vector2.one;
            exitTextRT.sizeDelta = Vector2.zero;

            LayoutElement exitLE = GetOrAddComponent<LayoutElement>(exitBtn);
            exitLE.minWidth = 100;
            exitLE.preferredWidth = 120;

            EditorUtility.SetDirty(searchBar);
            MarkSceneDirty();
            Debug.Log("[TournamentsUIBuilder] SearchBar creado con botones contextuales");
        }

        // Limpiar botones redundantes fuera del SearchBar
        private static void CleanupRedundantButtons()
        {
            Canvas canvas = GetCanvas();
            if (canvas == null) return;

            // Lista de botones a ocultar/eliminar
            string[] redundantButtons = {
                "LeaderboardBackButton",  // Ya está en SearchBar
                "ExitTournamentButton",   // Ya está en SearchBar (el viejo fuera del searchbar)
            };

            // Buscar en TournamentsPanel
            Transform tournamentsPanel = canvas.transform.Find("TournamentsPanel");
            if (tournamentsPanel != null)
            {
                foreach (string btnName in redundantButtons)
                {
                    Transform btn = tournamentsPanel.Find(btnName);
                    if (btn != null && btn.parent == tournamentsPanel)
                    {
                        // Ocultar en lugar de eliminar para no romper referencias
                        btn.gameObject.SetActive(false);
                        Debug.Log($"[TournamentsUIBuilder] Botón redundante '{btnName}' ocultado");
                    }
                }
            }

            // Buscar botón "Volver" suelto
            GameObject volverBtn = GameObject.Find("Volver");
            if (volverBtn != null)
            {
                // Verificar que no sea parte del Header
                if (volverBtn.transform.parent != null &&
                    !volverBtn.transform.parent.name.Contains("Header"))
                {
                    volverBtn.SetActive(false);
                    Debug.Log("[TournamentsUIBuilder] Botón 'Volver' redundante ocultado");
                }
            }

            // También buscar SearchOptionsButton y ocultarlo si existe
            GameObject searchOptionsBtn = GameObject.Find("SearchOptionsButton");
            if (searchOptionsBtn != null)
            {
                // Este botón ya no es necesario porque tenemos el SearchBar
                searchOptionsBtn.SetActive(false);
                Debug.Log("[TournamentsUIBuilder] SearchOptionsButton ocultado (reemplazado por SearchBar)");
            }
        }

        // ==================== 4. CREATE EMPTY STATE ====================

        private static void CreateEmptyState()
        {
            Canvas canvas = GetCanvas();
            if (canvas == null) return;

            // Buscar el ScrollView content
            ScrollRect scrollRect = Object.FindObjectOfType<ScrollRect>();
            Transform contentParent = null;

            if (scrollRect != null && scrollRect.content != null)
            {
                contentParent = scrollRect.content;
            }
            else
            {
                // Buscar alternativo
                Transform scrollView = canvas.transform.Find("TournamentsPanel/TournamentsScrollView");
                if (scrollView == null) scrollView = canvas.transform.Find("TournamentsScrollView");

                if (scrollView != null)
                {
                    Transform viewport = scrollView.Find("Viewport");
                    if (viewport != null)
                    {
                        contentParent = viewport.Find("Content");
                    }
                }
            }

            if (contentParent == null)
            {
                Debug.LogWarning("[TournamentsUIBuilder] No se encontró Content para EmptyState");
                return;
            }

            // Buscar o crear EmptyState
            Transform existing = contentParent.Find("EmptyState");
            GameObject emptyState;

            if (existing != null)
            {
                emptyState = existing.gameObject;
            }
            else
            {
                emptyState = new GameObject("EmptyState");
                emptyState.transform.SetParent(contentParent, false);
                emptyState.transform.SetAsFirstSibling();
            }

            // RectTransform
            RectTransform emptyRT = GetOrAddComponent<RectTransform>(emptyState);
            emptyRT.anchorMin = new Vector2(0.5f, 0.5f);
            emptyRT.anchorMax = new Vector2(0.5f, 0.5f);
            emptyRT.pivot = new Vector2(0.5f, 0.5f);
            emptyRT.anchoredPosition = new Vector2(0, 50);
            emptyRT.sizeDelta = new Vector2(350, 300);

            // VerticalLayoutGroup
            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(emptyState);
            vlg.spacing = 20;
            vlg.padding = new RectOffset(20, 20, 30, 30);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // === Trophy Icon ===
            GameObject iconObj = FindOrCreateChild(emptyState, "TrophyIcon");
            TextMeshProUGUI iconText = GetOrAddComponent<TextMeshProUGUI>(iconObj);
            iconText.text = "🏆";
            iconText.fontSize = 72;
            iconText.alignment = TextAlignmentOptions.Center;
            iconText.color = TEXT_MUTED;

            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(iconObj);
            iconLE.minHeight = 80;
            iconLE.preferredHeight = 80;

            // Glow sutil en el icono
            AddOutline(iconObj, CYAN_GLOW, 2);

            // === Title Text ===
            GameObject titleObj = FindOrCreateChild(emptyState, "EmptyTitle");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "No hay torneos disponibles";
            titleText.fontSize = 22;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = TEXT_PRIMARY;

            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minHeight = 30;
            titleLE.preferredHeight = 30;

            // === Subtitle Text ===
            GameObject subtitleObj = FindOrCreateChild(emptyState, "EmptySubtitle");
            TextMeshProUGUI subtitleText = GetOrAddComponent<TextMeshProUGUI>(subtitleObj);
            subtitleText.text = "Sé el primero en crear uno o\nvuelve más tarde";
            subtitleText.fontSize = 16;
            subtitleText.alignment = TextAlignmentOptions.Center;
            subtitleText.color = TEXT_SECONDARY;

            LayoutElement subtitleLE = GetOrAddComponent<LayoutElement>(subtitleObj);
            subtitleLE.minHeight = 50;
            subtitleLE.preferredHeight = 50;

            // === Create Button ===
            GameObject createBtn = FindOrCreateChild(emptyState, "CreateFirstButton");
            Image createBg = GetOrAddComponent<Image>(createBtn);
            createBg.color = BUTTON_PRIMARY;

            Button createButton = GetOrAddComponent<Button>(createBtn);
            SetupButtonColors(createButton, BUTTON_PRIMARY);
            AddOutline(createBtn, CYAN_GLOW, 2);

            LayoutElement createLE = GetOrAddComponent<LayoutElement>(createBtn);
            createLE.minHeight = 50;
            createLE.preferredHeight = 50;
            createLE.minWidth = 200;

            GameObject createTextObj = FindOrCreateChild(createBtn, "Text");
            TextMeshProUGUI createText = GetOrAddComponent<TextMeshProUGUI>(createTextObj);
            createText.text = "Crear Torneo";
            createText.fontSize = 18;
            createText.fontStyle = FontStyles.Bold;
            createText.alignment = TextAlignmentOptions.Center;
            createText.color = TEXT_DARK;

            RectTransform createTextRT = createTextObj.GetComponent<RectTransform>();
            createTextRT.anchorMin = Vector2.zero;
            createTextRT.anchorMax = Vector2.one;
            createTextRT.sizeDelta = Vector2.zero;

            EditorUtility.SetDirty(emptyState);
            MarkSceneDirty();
            Debug.Log("[TournamentsUIBuilder] EmptyState creado");
        }

        // ==================== 5. CREATE LOADING INDICATOR ====================

        private static void CreateLoadingIndicator()
        {
            Canvas canvas = GetCanvas();
            if (canvas == null) return;

            // Buscar contenedor
            Transform parent = canvas.transform.Find("TournamentsPanel");
            if (parent == null) parent = canvas.transform;

            // Buscar o crear
            Transform existing = parent.Find("LoadingIndicator");
            GameObject loading;

            if (existing != null)
            {
                loading = existing.gameObject;
            }
            else
            {
                loading = new GameObject("LoadingIndicator");
                loading.transform.SetParent(parent, false);
            }

            // Inicialmente oculto
            loading.SetActive(false);

            // RectTransform - centrado
            RectTransform loadingRT = GetOrAddComponent<RectTransform>(loading);
            loadingRT.anchorMin = new Vector2(0.5f, 0.5f);
            loadingRT.anchorMax = new Vector2(0.5f, 0.5f);
            loadingRT.pivot = new Vector2(0.5f, 0.5f);
            loadingRT.anchoredPosition = Vector2.zero;
            loadingRT.sizeDelta = new Vector2(150, 150);

            // === Spinner Ring ===
            GameObject ringObj = FindOrCreateChild(loading, "SpinnerRing");
            RectTransform ringRT = GetOrAddComponent<RectTransform>(ringObj);
            ringRT.anchorMin = new Vector2(0.5f, 0.5f);
            ringRT.anchorMax = new Vector2(0.5f, 0.5f);
            ringRT.sizeDelta = new Vector2(80, 80);

            Image ringImage = GetOrAddComponent<Image>(ringObj);
            ringImage.color = CYAN_NEON;
            ringImage.type = Image.Type.Simple;

            // El spinner necesita rotación runtime - agregar componente simple
            // Por ahora solo el visual

            AddOutline(ringObj, CYAN_GLOW, 3);

            // === Loading Text ===
            GameObject textObj = FindOrCreateChild(loading, "LoadingText");
            RectTransform textRT = GetOrAddComponent<RectTransform>(textObj);
            textRT.anchorMin = new Vector2(0.5f, 0);
            textRT.anchorMax = new Vector2(0.5f, 0);
            textRT.pivot = new Vector2(0.5f, 1);
            textRT.anchoredPosition = new Vector2(0, -10);
            textRT.sizeDelta = new Vector2(200, 30);

            TextMeshProUGUI loadingText = GetOrAddComponent<TextMeshProUGUI>(textObj);
            loadingText.text = "Cargando...";
            loadingText.fontSize = 16;
            loadingText.alignment = TextAlignmentOptions.Center;
            loadingText.color = TEXT_SECONDARY;

            EditorUtility.SetDirty(loading);
            MarkSceneDirty();
            Debug.Log("[TournamentsUIBuilder] LoadingIndicator creado");
        }

        // ==================== 6. CREATE UNIFIED MODAL BLOCKER ====================

        private static void CreateUnifiedModalBlocker()
        {
            Canvas canvas = GetCanvas();
            if (canvas == null) return;

            // Buscar o crear ModalBlocker
            Transform existing = canvas.transform.Find("ModalBlocker");
            GameObject blocker;

            if (existing != null)
            {
                blocker = existing.gameObject;
            }
            else
            {
                blocker = new GameObject("ModalBlocker");
                blocker.transform.SetParent(canvas.transform, false);
            }

            // Posicionar antes de los popups pero después del contenido principal
            blocker.transform.SetAsLastSibling();

            // Inicialmente oculto
            blocker.SetActive(false);

            // RectTransform - fullscreen
            RectTransform blockerRT = GetOrAddComponent<RectTransform>(blocker);
            blockerRT.anchorMin = Vector2.zero;
            blockerRT.anchorMax = Vector2.one;
            blockerRT.sizeDelta = Vector2.zero;
            blockerRT.anchoredPosition = Vector2.zero;

            // Fondo oscuro
            Image blockerBg = GetOrAddComponent<Image>(blocker);
            blockerBg.color = BLOCKER_BG;

            // Botón para cerrar al hacer clic fuera (opcional)
            Button blockerButton = GetOrAddComponent<Button>(blocker);
            blockerButton.transition = Selectable.Transition.None;

            EditorUtility.SetDirty(blocker);

            // También arreglar BlockerPanel existente si existe
            GameObject oldBlocker = GameObject.Find("BlockerPanel");
            if (oldBlocker != null)
            {
                Image oldBlockerBg = oldBlocker.GetComponent<Image>();
                if (oldBlockerBg != null)
                {
                    oldBlockerBg.color = BLOCKER_BG;
                    EditorUtility.SetDirty(oldBlockerBg);
                }
            }

            // Arreglar CreateTournamentBlocker
            GameObject createBlocker = GameObject.Find("CreateTournamentBlocker");
            if (createBlocker != null)
            {
                Image createBlockerBg = createBlocker.GetComponent<Image>();
                if (createBlockerBg != null)
                {
                    createBlockerBg.color = BLOCKER_BG;
                    EditorUtility.SetDirty(createBlockerBg);
                }
            }

            MarkSceneDirty();
            Debug.Log("[TournamentsUIBuilder] ModalBlocker unificado creado");
        }

        // ==================== 7. IMPROVE ALL POPUPS ====================

        private static void ImproveAllPopups()
        {
            ImprovePopup("SearchOptionsPanel", "Opciones de Busqueda");
            ImprovePopup("ConfirmPopup", null);
            ImprovePopup("CreateTournamentPanel", "Crear Torneo");
            ImprovePopup("ErrorPanel", "Error", true);
            ImprovePopup("ConfirmPanel", null);

            MarkSceneDirty();
            Debug.Log("[TournamentsUIBuilder] Todos los popups mejorados");
        }

        private static void ImprovePopup(string popupName, string title, bool isError = false)
        {
            GameObject popup = GameObject.Find(popupName);
            if (popup == null) return;

            // Fondo
            Image popupBg = popup.GetComponent<Image>();
            if (popupBg != null)
            {
                popupBg.color = POPUP_BG;
                EditorUtility.SetDirty(popupBg);
            }

            // Agregar glow
            AddPanelGlow(popup, isError);

            // Arreglar título si se especifica
            if (!string.IsNullOrEmpty(title))
            {
                TextMeshProUGUI[] texts = popup.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var text in texts)
                {
                    if (text.name.ToLower().Contains("title"))
                    {
                        text.text = title;
                        text.color = isError ? BUTTON_DANGER : CYAN_NEON;
                        text.fontSize = 28;
                        text.fontStyle = FontStyles.Bold;
                        EditorUtility.SetDirty(text);
                        break;
                    }
                }
            }

            // Mejorar botones del popup
            Button[] buttons = popup.GetComponentsInChildren<Button>(true);
            foreach (Button btn in buttons)
            {
                string btnName = btn.name.ToLower();

                if (btnName.Contains("close") || btnName.Contains("x"))
                {
                    // Botón X de cerrar
                    Image btnBg = btn.GetComponent<Image>();
                    if (btnBg != null) btnBg.color = BUTTON_DANGER;
                    SetupButtonColors(btn, BUTTON_DANGER);

                    // Hacer más pequeño
                    RectTransform btnRT = btn.GetComponent<RectTransform>();
                    if (btnRT != null)
                    {
                        btnRT.sizeDelta = new Vector2(35, 35);
                    }
                }
                else if (btnName.Contains("confirm") || btnName.Contains("apply") ||
                         btnName.Contains("create") || btnName.Contains("join"))
                {
                    // Botón primario
                    Image btnBg = btn.GetComponent<Image>();
                    if (btnBg != null) btnBg.color = BUTTON_PRIMARY;
                    SetupButtonColors(btn, BUTTON_PRIMARY);
                    AddOutline(btn.gameObject, CYAN_GLOW, 2);

                    TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
                    if (btnText != null) btnText.color = TEXT_DARK;
                }
                else if (btnName.Contains("cancel") || btnName.Contains("clear"))
                {
                    // Botón secundario
                    Image btnBg = btn.GetComponent<Image>();
                    if (btnBg != null) btnBg.color = BUTTON_SECONDARY;
                    SetupButtonColors(btn, BUTTON_SECONDARY);
                    AddOutline(btn.gameObject, CYAN_DARK);

                    TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
                    if (btnText != null) btnText.color = TEXT_PRIMARY;
                }

                EditorUtility.SetDirty(btn);
            }

            // Mejorar inputs del popup
            TMP_InputField[] inputs = popup.GetComponentsInChildren<TMP_InputField>(true);
            foreach (var input in inputs)
            {
                Image inputBg = input.GetComponent<Image>();
                if (inputBg != null) inputBg.color = INPUT_BG;

                AddOutline(input.gameObject, CYAN_DARK);

                if (input.textComponent != null)
                    input.textComponent.color = TEXT_PRIMARY;

                input.caretColor = CYAN_NEON;
                input.selectionColor = new Color(0f, 1f, 1f, 0.3f);

                EditorUtility.SetDirty(input);
            }

            // Mejorar sliders
            Slider[] sliders = popup.GetComponentsInChildren<Slider>(true);
            foreach (var slider in sliders)
            {
                if (slider.fillRect != null)
                {
                    Image fill = slider.fillRect.GetComponent<Image>();
                    if (fill != null) fill.color = CYAN_NEON;
                }

                if (slider.handleRect != null)
                {
                    Image handle = slider.handleRect.GetComponent<Image>();
                    if (handle != null) handle.color = CYAN_NEON;
                    AddOutline(slider.handleRect.gameObject, CYAN_GLOW);
                }

                EditorUtility.SetDirty(slider);
            }

            // Mejorar toggles
            Toggle[] toggles = popup.GetComponentsInChildren<Toggle>(true);
            foreach (var toggle in toggles)
            {
                if (toggle.graphic != null)
                    toggle.graphic.color = CYAN_NEON;

                Image toggleBg = toggle.targetGraphic as Image;
                if (toggleBg != null)
                {
                    toggleBg.color = INPUT_BG;
                    AddOutline(toggleBg.gameObject, CYAN_DARK);
                }

                EditorUtility.SetDirty(toggle);
            }

            // Mejorar dropdowns
            TMP_Dropdown[] dropdowns = popup.GetComponentsInChildren<TMP_Dropdown>(true);
            foreach (var dropdown in dropdowns)
            {
                Image dropBg = dropdown.GetComponent<Image>();
                if (dropBg != null) dropBg.color = INPUT_BG;

                AddOutline(dropdown.gameObject, CYAN_DARK);

                if (dropdown.captionText != null)
                    dropdown.captionText.color = TEXT_PRIMARY;

                EditorUtility.SetDirty(dropdown);
            }

            // Mejorar labels
            TextMeshProUGUI[] labels = popup.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var label in labels)
            {
                if (label.GetComponentInParent<Button>() != null) continue;
                if (label.GetComponentInParent<TMP_InputField>() != null) continue;

                string labelName = label.name.ToLower();

                if (labelName.Contains("title"))
                {
                    label.color = isError ? BUTTON_DANGER : CYAN_NEON;
                    label.fontStyle = FontStyles.Bold;
                }
                else if (labelName.Contains("label"))
                {
                    label.color = TEXT_SECONDARY;
                }
                else if (labelName.Contains("value"))
                {
                    label.color = CYAN_NEON;
                    label.fontStyle = FontStyles.Bold;
                }
                else
                {
                    label.color = TEXT_PRIMARY;
                }

                EditorUtility.SetDirty(label);
            }

            EditorUtility.SetDirty(popup);
            Debug.Log($"[TournamentsUIBuilder] Popup '{popupName}' mejorado");
        }

        private static void AddPanelGlow(GameObject panel, bool isError = false)
        {
            Color glowColor = isError ? new Color(1f, 0.3f, 0.3f, 0.4f) : CYAN_GLOW;
            Color borderColor = isError ? BUTTON_DANGER : CYAN_DARK;

            // Limpiar shadows anteriores (no outlines)
            Shadow[] existingShadows = panel.GetComponents<Shadow>();
            foreach (var s in existingShadows)
            {
                if (!(s is Outline))
                    Object.DestroyImmediate(s);
            }

            // Outline principal
            Outline outline = panel.GetComponent<Outline>();
            if (outline == null) outline = panel.AddComponent<Outline>();
            outline.effectColor = borderColor;
            outline.effectDistance = new Vector2(1.5f, 1.5f);

            // Shadows para glow
            Shadow shadow1 = panel.AddComponent<Shadow>();
            shadow1.effectColor = glowColor;
            shadow1.effectDistance = new Vector2(4, -4);

            Shadow shadow2 = panel.AddComponent<Shadow>();
            shadow2.effectColor = new Color(glowColor.r, glowColor.g, glowColor.b, glowColor.a * 0.5f);
            shadow2.effectDistance = new Vector2(-4, 4);
        }

        // ==================== 8. FIX SCROLL VIEWS ====================

        private static void FixScrollViews()
        {
            ScrollRect[] scrollRects = Resources.FindObjectsOfTypeAll<ScrollRect>();

            foreach (ScrollRect scrollRect in scrollRects)
            {
                if (!scrollRect.gameObject.scene.isLoaded) continue;

                // Fondo del ScrollRect
                Image scrollBg = scrollRect.GetComponent<Image>();
                if (scrollBg != null)
                {
                    scrollBg.color = DARK_BG;
                    EditorUtility.SetDirty(scrollBg);
                }

                // Viewport
                if (scrollRect.viewport != null)
                {
                    Image viewportBg = scrollRect.viewport.GetComponent<Image>();
                    if (viewportBg != null)
                    {
                        viewportBg.color = new Color(0.03f, 0.05f, 0.08f, 1f);
                        EditorUtility.SetDirty(viewportBg);
                    }

                    // Margen inferior
                    RectTransform viewportRT = scrollRect.viewport.GetComponent<RectTransform>();
                    if (viewportRT != null)
                    {
                        viewportRT.offsetMin = new Vector2(viewportRT.offsetMin.x, 15);
                        EditorUtility.SetDirty(viewportRT);
                    }
                }

                // Content
                if (scrollRect.content != null)
                {
                    Image contentBg = scrollRect.content.GetComponent<Image>();
                    if (contentBg != null)
                    {
                        contentBg.color = Color.clear;
                        EditorUtility.SetDirty(contentBg);
                    }
                }

                // Scrollbar vertical
                if (scrollRect.verticalScrollbar != null)
                {
                    Scrollbar sb = scrollRect.verticalScrollbar;

                    Image sbBg = sb.GetComponent<Image>();
                    if (sbBg != null) sbBg.color = new Color(0.1f, 0.1f, 0.1f, 0.3f);

                    if (sb.handleRect != null)
                    {
                        Image handleImg = sb.handleRect.GetComponent<Image>();
                        if (handleImg != null) handleImg.color = CYAN_DARK;
                        AddOutline(sb.handleRect.gameObject, CYAN_GLOW, 1);
                    }

                    RectTransform sbRT = sb.GetComponent<RectTransform>();
                    if (sbRT != null) sbRT.sizeDelta = new Vector2(8, sbRT.sizeDelta.y);

                    EditorUtility.SetDirty(sb);
                }

                EditorUtility.SetDirty(scrollRect);
            }

            Debug.Log("[TournamentsUIBuilder] ScrollViews arreglados");
        }

        // ==================== 9. FIX ALL COLORS ====================

        private static void FixAllColors()
        {
            // Buttons
            Button[] buttons = Resources.FindObjectsOfTypeAll<Button>();
            foreach (Button btn in buttons)
            {
                if (!btn.gameObject.scene.isLoaded) continue;

                string name = btn.name.ToLower();
                if (name.Contains("tab")) continue; // Ya procesados

                Image bg = btn.GetComponent<Image>();
                if (bg == null) continue;

                TextMeshProUGUI text = btn.GetComponentInChildren<TextMeshProUGUI>();

                if (name.Contains("confirm") || name.Contains("apply") ||
                    name.Contains("create") || name.Contains("join") || name.Contains("search"))
                {
                    bg.color = BUTTON_PRIMARY;
                    if (text != null) text.color = TEXT_DARK;
                    SetupButtonColors(btn, BUTTON_PRIMARY);
                }
                else if (name.Contains("close") || name.Contains("delete") || name.Contains("exit"))
                {
                    bg.color = BUTTON_DANGER;
                    if (text != null) text.color = TEXT_PRIMARY;
                    SetupButtonColors(btn, BUTTON_DANGER);
                }
                else
                {
                    bg.color = BUTTON_SECONDARY;
                    if (text != null) text.color = TEXT_PRIMARY;
                    SetupButtonColors(btn, BUTTON_SECONDARY);
                }

                EditorUtility.SetDirty(btn);
            }

            // Texts
            TextMeshProUGUI[] texts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
            foreach (TextMeshProUGUI text in texts)
            {
                if (!text.gameObject.scene.isLoaded) continue;
                if (text.GetComponentInParent<Button>() != null) continue;
                if (text.GetComponentInParent<TMP_InputField>() != null) continue;

                string name = text.name.ToLower();

                if (name.Contains("title") || name.Contains("header"))
                {
                    text.color = CYAN_NEON;
                    text.fontStyle = FontStyles.Bold;
                }
                else if (name.Contains("label"))
                {
                    text.color = TEXT_SECONDARY;
                }
                else if (name.Contains("value"))
                {
                    text.color = CYAN_NEON;
                }
                else
                {
                    text.color = TEXT_PRIMARY;
                }

                EditorUtility.SetDirty(text);
            }

            Debug.Log("[TournamentsUIBuilder] Todos los colores arreglados");
        }

        // ==================== 10. REORGANIZE HIERARCHY ====================

        private static void ReorganizeHierarchy()
        {
            Canvas canvas = GetCanvas();
            if (canvas == null) return;

            Debug.Log("[TournamentsUIBuilder] Reorganizando hierarchy...");

            // Esta función es avanzada y puede romper referencias
            // Solo reordena los elementos existentes

            // Orden deseado:
            // 1. Background
            // 2. Header
            // 3. MainContent (TournamentsPanel)
            // 4. BlockerPanel / ModalBlocker
            // 5. Popups
            // 6. BackButton (si no está en Header)

            int index = 0;

            // 1. Background
            Transform bg = canvas.transform.Find("Background");
            if (bg != null) bg.SetSiblingIndex(index++);

            // 2. Header
            Transform header = canvas.transform.Find("Header");
            if (header != null) header.SetSiblingIndex(index++);

            // 3. TournamentsPanel / MainContent
            Transform main = canvas.transform.Find("TournamentsPanel");
            if (main == null) main = canvas.transform.Find("MainContent");
            if (main != null) main.SetSiblingIndex(index++);

            // 4. Blockers
            Transform blocker = canvas.transform.Find("BlockerPanel");
            if (blocker != null) blocker.SetSiblingIndex(index++);

            Transform modalBlocker = canvas.transform.Find("ModalBlocker");
            if (modalBlocker != null) modalBlocker.SetSiblingIndex(index++);

            // 5. Popups individuales (si no están en BlockerPanel)
            string[] popupNames = { "ConfirmPanel", "ErrorPanel", "SearchOptionsPanel", "CreateTournamentPanel" };
            foreach (string popupName in popupNames)
            {
                Transform popup = canvas.transform.Find(popupName);
                if (popup != null) popup.SetSiblingIndex(index++);
            }

            // 6. BackButton al final (si no está en Header)
            Transform backBtn = canvas.transform.Find("BackButton");
            if (backBtn != null && backBtn.parent == canvas.transform)
            {
                backBtn.SetSiblingIndex(index++);
            }

            MarkSceneDirty();
            Debug.Log("[TournamentsUIBuilder] Hierarchy reorganizada");
        }

        // ==================== UTILITY METHODS ====================

        private static Canvas GetCanvas()
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[TournamentsUIBuilder] No se encontró Canvas en la escena");
            }
            return canvas;
        }

        private static void MarkSceneDirty()
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

        private static bool ConfirmAction(string message)
        {
            return EditorUtility.DisplayDialog("Tournaments UI Builder", message, "Sí", "No");
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

            // Asegurar que tiene RectTransform
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
    }
}
