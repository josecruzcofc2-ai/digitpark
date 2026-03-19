using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;
using DigitPark.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// UI Builder para la escena CashBattle1v1
    /// Construye la interfaz de selección de juegos y apuestas para batallas 1v1
    /// </summary>
    public class CashBattle1v1UIBuilder : EditorWindow
    {
        // Reference Assigner state
        private Vector2 scrollPosition;
        private static int assignedCount = 0;
        private static int failedCount = 0;
        private static int alreadySetCount = 0;
        private static List<AssignResult> assignResults = new List<AssignResult>();

        private struct AssignResult
        {
            public string fieldName;
            public string status;
            public bool success;
            public Object assignedObject;
        }

        // Premium Color Palette
        private static readonly Color GOLD_PRIMARY = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GOLD_DARK = new Color(0.85f, 0.65f, 0.13f, 1f);
        private static readonly Color GOLD_LIGHT = new Color(1f, 0.93f, 0.55f, 1f);
        private static readonly Color AMBER = new Color(1f, 0.75f, 0f, 1f);

        private static readonly Color BG_DARK = new Color(0.06f, 0.05f, 0.10f, 1f);
        private static readonly Color CARD_BG = new Color(0.12f, 0.1f, 0.15f, 0.95f);
        private static readonly Color CARD_BORDER = new Color(0.85f, 0.65f, 0.13f, 0.6f);

        private static readonly Color TEXT_PRIMARY = new Color(1f, 1f, 1f, 1f);
        private static readonly Color TEXT_GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.7f, 0.7f, 0.7f, 1f);

        private static readonly Color BUTTON_GOLD = new Color(0.85f, 0.65f, 0.13f, 1f);
        private static readonly Color CYAN_ACCENT = new Color(0f, 0.9f, 1f, 1f);

        [MenuItem("DigitPark/Scenes/Build Scene/CashBattle/1v1", false, 181)]
        public static void ShowWindow()
        {
            GetWindow<CashBattle1v1UIBuilder>("CashBattle 1v1 Builder");
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // ========== SECCION 1: UI BUILDER ==========
            GUILayout.Label("CashBattle 1v1 UI Builder", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox(
                "Construye la UI para la escena CashBattle1v1:\n" +
                "- Header, Grid de juegos, Apuestas, Buscar Rival, Cognitive Sprint",
                MessageType.Info);

            EditorGUILayout.Space(5);

            GUI.backgroundColor = GOLD_PRIMARY;
            if (GUILayout.Button("CONSTRUIR UI COMPLETA", GUILayout.Height(40)))
            {
                BuildCashBattle1v1UI();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);
            GUILayout.Label("Construccion por Secciones:", EditorStyles.boldLabel);

            if (GUILayout.Button("Solo Header + Online Indicator", GUILayout.Height(26)))
                BuildHeaderOnly();
            if (GUILayout.Button("Solo Game Selector", GUILayout.Height(26)))
                BuildGameSelectorOnly();
            if (GUILayout.Button("Solo Entry Fee Section", GUILayout.Height(26)))
                BuildEntryFeeSectionOnly();
            if (GUILayout.Button("Solo Find Opponent Button", GUILayout.Height(26)))
                BuildFindOpponentOnly();

            // ========== SEPARADOR ==========
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // ========== SECCION 2: REFERENCE ASSIGNER ==========
            GUILayout.Label("Asignar Referencias", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "CashBattle1v1")
            {
                EditorGUILayout.HelpBox($"Escena actual: {currentScene}\nAbre CashBattle1v1 primero.", MessageType.Warning);
            }

            MonoBehaviour targetManager = FindCashBattle1v1Manager();
            if (targetManager != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Manager:", GUILayout.Width(60));
                EditorGUILayout.ObjectField(targetManager, typeof(MonoBehaviour), true);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("CashBattle1v1Manager no encontrado en escena.", MessageType.Warning);
            }

            EditorGUILayout.Space(5);

            GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
            if (GUILayout.Button("ASIGNAR TODAS LAS REFERENCIAS", GUILayout.Height(36)))
            {
                ResetAssignState();
                RunAssignAllReferences();
                Repaint();
            }
            GUI.backgroundColor = Color.white;

            // Mostrar resultados
            DrawAssignResults();

            EditorGUILayout.EndScrollView();
        }

        #region Main Build Methods

        private static void BuildCashBattle1v1UI()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontro Canvas. Abre la escena CashBattle1v1 primero.", "OK");
                return;
            }

            // Standardize CanvasScaler
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
            }

            if (EditorUtility.DisplayDialog("Construir UI?",
                "Esto reconstruira la UI de CashBattle1v1.\n\nLos elementos existentes seran reemplazados.\n\nContinuar?",
                "Si, Construir", "Cancelar"))
            {
                BuildAllElements(canvas);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

                EditorUtility.DisplayDialog("Completado",
                    "UI de CashBattle1v1 construida!\n\n" +
                    "Recuerda:\n" +
                    "1. Asignar los iconos de juegos\n" +
                    "2. Configurar el manager\n" +
                    "3. Guardar la escena",
                    "OK");
            }
        }

        /// <summary>
        /// Builds the UI silently without confirmation dialogs. Used by batch builders.
        /// </summary>
        public static void BuildSilent()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("[CashBattle1v1UIBuilder] Canvas not found - cannot build silently");
                return;
            }

            // Standardize CanvasScaler
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
            }

            BuildAllElements(canvas);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("[CashBattle1v1UIBuilder] UI built silently (batch mode)");
        }

        private static void CleanupOldUI()
        {
            string[] toClean = { "Background", "SafeArea", "BackButton", "BackButtonGold" };
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

        private static void BuildAllElements(Canvas canvas)
        {
            Transform canvasTransform = canvas.transform;

            // Limpiar elementos existentes
            CleanupOldElements(canvasTransform);

            // 1. Background
            CreateBackground(canvasTransform);

            // 2. Safe Area
            GameObject safeArea = CreateSafeArea(canvasTransform);

            // 3. Header con balance
            CreateHeader(safeArea.transform);

            // 4. Main Content Panel
            CreateMainContentPanel(safeArea.transform);

            // 5. Cognitive Sprint Section (below title, hidden by default)
            CreateCognitiveSprintSection(safeArea.transform);

            // 6. Game Selection Modal (fullscreen overlay, hidden by default)
            CreateGameSelectionModal(safeArea.transform);

            Debug.Log("[CashBattle1v1Builder] UI construida exitosamente!");
        }

        private static void CleanupOldElements(Transform parent)
        {
            List<GameObject> toDestroy = new List<GameObject>();
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

        #endregion

        #region Background

        private static void CreateBackground(Transform parent)
        {
            GameObject bgContainer = new GameObject("Background");
            bgContainer.transform.SetParent(parent, false);
            bgContainer.transform.SetAsFirstSibling();

            RectTransform bgRT = bgContainer.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;

            // Base dark layer
            Image baseImg = bgContainer.AddComponent<Image>();
            baseImg.color = BG_DARK;
        }

        #endregion

        #region Safe Area

        private static GameObject CreateSafeArea(Transform parent)
        {
            GameObject safeArea = new GameObject("SafeArea");
            safeArea.transform.SetParent(parent, false);

            RectTransform rt = safeArea.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            return safeArea;
        }

        #endregion

        #region Header

        private static void CreateHeader(Transform parent)
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(parent, false);

            RectTransform headerRT = header.AddComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0, 1);
            headerRT.anchorMax = new Vector2(1, 1);
            headerRT.pivot = new Vector2(0.5f, 1);
            headerRT.sizeDelta = new Vector2(0, 120);
            headerRT.anchoredPosition = new Vector2(0, -29);

            Image headerBg = header.AddComponent<Image>();
            headerBg.color = new Color(0, 0, 0, 0.3f);

            // Back Button
            CreateBackButton(header.transform);

            // Title
            CreateHeaderTitle(header.transform);

            // Balance Widget
            CreateBalanceWidget(header.transform);
        }

        private const string BACK_BUTTON_GOLD_PREFAB = "Assets/_Project/Prefabs/Common/BackButtonGold.prefab";
        private const string BACK_ICON_GOLD_PATH = "Assets/_Project/Art/Icons/Navigation/BackIconGold.png";

        private static void CreateBackButton(Transform parent)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BACK_BUTTON_GOLD_PREFAB);
            if (prefab != null)
            {
                GameObject backBtn = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                backBtn.name = "BackButton";

                RectTransform rect = backBtn.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 0.5f);
                rect.anchorMax = new Vector2(0, 0.5f);
                rect.pivot = new Vector2(0, 0.5f);
                rect.anchoredPosition = new Vector2(20, 0);
                rect.sizeDelta = new Vector2(88, 88);

                // Assign BackIconGold sprite to Icon child
                Sprite backIcon = AssetDatabase.LoadAssetAtPath<Sprite>(BACK_ICON_GOLD_PATH);
                if (backIcon != null)
                {
                    Transform iconChild = backBtn.transform.Find("Icon");
                    if (iconChild != null)
                    {
                        Image iconImg = iconChild.GetComponent<Image>();
                        if (iconImg != null) iconImg.sprite = backIcon;
                    }
                }
            }
            else
            {
                GameObject backBtn = new GameObject("BackButton");
                backBtn.transform.SetParent(parent, false);

                RectTransform rt = backBtn.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 0.5f);
                rt.anchorMax = new Vector2(0, 0.5f);
                rt.pivot = new Vector2(0, 0.5f);
                rt.sizeDelta = new Vector2(88, 88);
                rt.anchoredPosition = new Vector2(20, 0);

                Image img = backBtn.AddComponent<Image>();
                img.color = Color.clear;

                Button btn = backBtn.AddComponent<Button>();

                GameObject arrowObj = new GameObject("Arrow");
                arrowObj.transform.SetParent(backBtn.transform, false);

                RectTransform arrowRT = arrowObj.AddComponent<RectTransform>();
                arrowRT.anchorMin = Vector2.zero;
                arrowRT.anchorMax = Vector2.one;
                arrowRT.sizeDelta = Vector2.zero;

                Image arrow = arrowObj.AddComponent<Image>();
                Sprite arrowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/UI/icon_back_arrow.png");
                if (arrowSprite != null) arrow.sprite = arrowSprite;
                arrow.color = TEXT_GOLD;
                arrow.preserveAspect = true;
                arrow.raycastTarget = false;

                Debug.LogWarning("[CashBattle1v1] BackButtonGold prefab not found, using fallback");
            }
        }

        private static void CreateHeaderTitle(Transform parent)
        {
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(parent, false);

            RectTransform rt = titleObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.07f, 0f);
            rt.anchorMax = new Vector2(0.53f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;

            TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
            title.text = "Battles 1v1";
            title.fontSize = FontSizes.H4;
            title.color = TEXT_GOLD;
            title.alignment = TextAlignmentOptions.MidlineLeft;
            title.raycastTarget = false;
            title.fontStyle = FontStyles.Bold;
            title.enableAutoSizing = true;
            title.fontSizeMin = FontSizes.AutoMinTitle;
            title.fontSizeMax = FontSizes.H4;
            title.overflowMode = TextOverflowModes.Ellipsis;
        }

        private static void CreateBalanceWidget(Transform parent)
        {
            GameObject balanceWidget = new GameObject("BalanceWidget");
            balanceWidget.transform.SetParent(parent, false);

            RectTransform rt = balanceWidget.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(1, 0.5f);
            rt.sizeDelta = new Vector2(300, 70);
            rt.anchoredPosition = new Vector2(-15, 0);

            // Background
            Image bg = balanceWidget.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.08f, 0.05f, 0.8f);

            // Gold border
            Outline outline = balanceWidget.AddComponent<Outline>();
            outline.effectColor = CARD_BORDER;
            outline.effectDistance = new Vector2(1, -1);

            // Balance text (includes $ sign via code)
            GameObject balanceObj = new GameObject("BalanceText");
            balanceObj.transform.SetParent(balanceWidget.transform, false);

            RectTransform balanceRT = balanceObj.AddComponent<RectTransform>();
            balanceRT.anchorMin = Vector2.zero;
            balanceRT.anchorMax = Vector2.one;
            balanceRT.sizeDelta = Vector2.zero;
            balanceRT.offsetMin = new Vector2(15, 0);
            balanceRT.offsetMax = new Vector2(-10, 0);

            TextMeshProUGUI balanceText = balanceObj.AddComponent<TextMeshProUGUI>();
            balanceText.text = "$0.00";
            balanceText.fontSize = FontSizes.Subtitle;
            balanceText.color = TEXT_GOLD;
            balanceText.alignment = TextAlignmentOptions.Center;
            balanceText.fontStyle = FontStyles.Bold;
            balanceText.enableAutoSizing = true;
            balanceText.fontSizeMin = FontSizes.AutoMinBody;
            balanceText.fontSizeMax = FontSizes.Subtitle;
        }

        private static void BuildHeaderOnly()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            // Standardize CanvasScaler
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
            }

            Transform safeArea = canvas.transform.Find("SafeArea");
            if (safeArea == null)
            {
                safeArea = CreateSafeArea(canvas.transform).transform;
            }

            Transform oldHeader = safeArea.Find("Header");
            if (oldHeader != null) DestroyImmediate(oldHeader.gameObject);

            CreateHeader(safeArea);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        #endregion

        #region Main Content Panel

        private static void CreateMainContentPanel(Transform parent)
        {
            GameObject panel = new GameObject("MainContentPanel");
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = new Vector2(20, 25);
            rt.offsetMax = new Vector2(-20, -125);

            // Title + Game Selector (compact top)
            CreatePanelTitle(panel.transform);
            CreateGameSelector(panel.transform);

            CreateRoundsSelector(panel.transform);

            CreateSectionSeparator(panel.transform, 0.70f);

            // HERO: Potential Earnings (center)
            CreateHeroEarnings(panel.transform);

            CreateSectionSeparator(panel.transform, 0.48f);

            // Bet Section
            CreateEntryFeeSection(panel.transform);

            CreateSectionSeparator(panel.transform, 0.12f);

            // Find Opponent Button
            CreateFindOpponentButton(panel.transform);
        }

        /// <summary>
        /// Crea una linea separadora sutil entre secciones
        /// </summary>
        private static void CreateSectionSeparator(Transform parent, float yPosition)
        {
            GameObject separator = new GameObject($"Separator_{yPosition}");
            separator.transform.SetParent(parent, false);

            RectTransform rt = separator.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.1f, yPosition);
            rt.anchorMax = new Vector2(0.9f, yPosition);
            rt.sizeDelta = new Vector2(0, 2);

            Image line = separator.AddComponent<Image>();
            line.color = new Color(0.3f, 0.25f, 0.15f, 0.4f); // Linea dorada sutil
        }

        private static void CreatePanelTitle(Transform parent)
        {
            GameObject titleObj = new GameObject("Cash1v1SelectGameTitle");
            titleObj.transform.SetParent(parent, false);

            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 1);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0.5f, 1);
            titleRT.sizeDelta = new Vector2(0, 35);
            titleRT.anchoredPosition = Vector2.zero;

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Select Game";
            titleText.fontSize = FontSizes.Caption;
            titleText.color = new Color(TEXT_GOLD.r, TEXT_GOLD.g, TEXT_GOLD.b, 0.6f);
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.fontStyle = FontStyles.Bold;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = FontSizes.AutoMinSmall;
            titleText.fontSizeMax = FontSizes.Caption;
        }

        #endregion

        #region Game Selector (Dropdown + Details)

        private static void CreateGameSelector(Transform parent)
        {
            GameObject selectorContainer = new GameObject("GameSelectorContainer");
            selectorContainer.transform.SetParent(parent, false);

            RectTransform selectorRT = selectorContainer.AddComponent<RectTransform>();
            selectorRT.anchorMin = new Vector2(0, 0.80f);
            selectorRT.anchorMax = new Vector2(1, 0.95f);
            selectorRT.sizeDelta = Vector2.zero;
            selectorRT.offsetMin = new Vector2(10, 0);
            selectorRT.offsetMax = new Vector2(-10, -40);

            // ========== ROW: Dropdown + View Details Button ==========
            GameObject dropdownRow = new GameObject("DropdownRow");
            dropdownRow.transform.SetParent(selectorContainer.transform, false);

            RectTransform rowRT = dropdownRow.AddComponent<RectTransform>();
            rowRT.anchorMin = new Vector2(0, 0f);
            rowRT.anchorMax = new Vector2(1, 1f);
            rowRT.sizeDelta = Vector2.zero;

            // --- TMP_Dropdown ---
            GameObject dropdownObj = new GameObject("GameDropdown");
            dropdownObj.transform.SetParent(dropdownRow.transform, false);

            RectTransform ddRT = dropdownObj.AddComponent<RectTransform>();
            ddRT.anchorMin = new Vector2(0, 0.1f);
            ddRT.anchorMax = new Vector2(0.62f, 0.9f);
            ddRT.sizeDelta = Vector2.zero;

            Image ddBg = dropdownObj.AddComponent<Image>();
            ddBg.color = CARD_BG;

            Outline ddOutline = dropdownObj.AddComponent<Outline>();
            ddOutline.effectColor = CARD_BORDER;
            ddOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // Dropdown label
            GameObject ddLabelObj = new GameObject("GameDropdownLabel");
            ddLabelObj.transform.SetParent(dropdownObj.transform, false);

            RectTransform ddLabelRT = ddLabelObj.AddComponent<RectTransform>();
            ddLabelRT.anchorMin = Vector2.zero;
            ddLabelRT.anchorMax = Vector2.one;
            ddLabelRT.sizeDelta = Vector2.zero;
            ddLabelRT.offsetMin = new Vector2(15, 0);
            ddLabelRT.offsetMax = new Vector2(-35, 0);

            TextMeshProUGUI ddLabel = ddLabelObj.AddComponent<TextMeshProUGUI>();
            ddLabel.text = "Select a game...";
            ddLabel.fontSize = FontSizes.Body;
            ddLabel.color = TEXT_PRIMARY;
            ddLabel.alignment = TextAlignmentOptions.Left;
            ddLabel.fontStyle = FontStyles.Bold;
            ddLabel.raycastTarget = false;
            ddLabel.enableAutoSizing = true;
            ddLabel.fontSizeMin = FontSizes.AutoMinSmall;
            ddLabel.fontSizeMax = FontSizes.Body;

            // Dropdown arrow
            GameObject arrowObj = new GameObject("Arrow");
            arrowObj.transform.SetParent(dropdownObj.transform, false);

            RectTransform arrowRT = arrowObj.AddComponent<RectTransform>();
            arrowRT.anchorMin = new Vector2(1, 0);
            arrowRT.anchorMax = new Vector2(1, 1);
            arrowRT.pivot = new Vector2(1, 0.5f);
            arrowRT.sizeDelta = new Vector2(35, 0);

            TextMeshProUGUI arrowText = arrowObj.AddComponent<TextMeshProUGUI>();
            arrowText.text = "\u25BC";
            arrowText.fontSize = FontSizes.Body;
            arrowText.color = TEXT_GOLD;
            arrowText.alignment = TextAlignmentOptions.Center;
            arrowText.raycastTarget = false;
            arrowText.fontStyle = FontStyles.Bold;
            arrowText.enableAutoSizing = true;
            arrowText.fontSizeMin = FontSizes.AutoMinBody;
            arrowText.fontSizeMax = FontSizes.Body;
            arrowText.overflowMode = TextOverflowModes.Ellipsis;

            // Dropdown template (required by TMP_Dropdown)
            GameObject templateObj = new GameObject("Template");
            templateObj.transform.SetParent(dropdownObj.transform, false);

            RectTransform templateRT = templateObj.AddComponent<RectTransform>();
            templateRT.anchorMin = new Vector2(0, 0);
            templateRT.anchorMax = new Vector2(1, 0);
            templateRT.pivot = new Vector2(0.5f, 1);
            templateRT.sizeDelta = new Vector2(0, 280); // 5 items × 50px + padding

            Image templateBg = templateObj.AddComponent<Image>();
            templateBg.color = new Color(0.1f, 0.08f, 0.13f, 0.98f);

            ScrollRect scrollRect = templateObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.scrollSensitivity = 30f;

            // Viewport
            GameObject viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(templateObj.transform, false);

            RectTransform viewportRT = viewportObj.AddComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.offsetMin = new Vector2(0, 2);
            viewportRT.offsetMax = new Vector2(0, -2);

            viewportObj.AddComponent<RectMask2D>();
            Image vpImage = viewportObj.AddComponent<Image>();
            vpImage.color = new Color(0, 0, 0, 0); // transparent, needed for RectMask2D

            // Content
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform, false);

            RectTransform contentRT = contentObj.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.sizeDelta = new Vector2(0, 28); // TMP_Dropdown calculates actual height

            // Item template
            GameObject itemObj = new GameObject("Item");
            itemObj.transform.SetParent(contentObj.transform, false);

            RectTransform itemRT = itemObj.AddComponent<RectTransform>();
            itemRT.anchorMin = new Vector2(0, 0.5f);
            itemRT.anchorMax = new Vector2(1, 0.5f);
            itemRT.sizeDelta = new Vector2(0, 50);

            Toggle itemToggle = itemObj.AddComponent<Toggle>();

            // Item background
            Image itemBg = itemObj.AddComponent<Image>();
            itemBg.color = new Color(0.12f, 0.1f, 0.15f, 1f);

            // Item checkmark
            GameObject itemCheckObj = new GameObject("Item Checkmark");
            itemCheckObj.transform.SetParent(itemObj.transform, false);

            RectTransform itemCheckRT = itemCheckObj.AddComponent<RectTransform>();
            itemCheckRT.anchorMin = new Vector2(0, 0);
            itemCheckRT.anchorMax = new Vector2(0, 1);
            itemCheckRT.sizeDelta = new Vector2(30, 0);
            itemCheckRT.anchoredPosition = new Vector2(15, 0);

            Image itemCheckImg = itemCheckObj.AddComponent<Image>();
            itemCheckImg.color = GOLD_PRIMARY;

            // Item label
            GameObject itemLabelObj = new GameObject("Item Label");
            itemLabelObj.transform.SetParent(itemObj.transform, false);

            RectTransform itemLabelRT = itemLabelObj.AddComponent<RectTransform>();
            itemLabelRT.anchorMin = Vector2.zero;
            itemLabelRT.anchorMax = Vector2.one;
            itemLabelRT.sizeDelta = Vector2.zero;
            itemLabelRT.offsetMin = new Vector2(35, 0);
            itemLabelRT.offsetMax = new Vector2(-10, 0);

            TextMeshProUGUI itemLabel = itemLabelObj.AddComponent<TextMeshProUGUI>();
            itemLabel.text = "Option";
            itemLabel.fontSize = FontSizes.Body;
            itemLabel.color = TEXT_PRIMARY;
            itemLabel.alignment = TextAlignmentOptions.Left;
            itemLabel.fontStyle = FontStyles.Bold;
            itemLabel.enableAutoSizing = true;
            itemLabel.fontSizeMin = FontSizes.AutoMinBody;
            itemLabel.fontSizeMax = FontSizes.Body;
            itemLabel.overflowMode = TextOverflowModes.Ellipsis;

            // Wire up toggle
            itemToggle.targetGraphic = itemBg;
            itemToggle.graphic = itemCheckImg;
            itemToggle.isOn = false;

            // Wire up scroll rect
            scrollRect.content = contentRT;
            scrollRect.viewport = viewportRT;

            // TMP_Dropdown component
            TMP_Dropdown dropdown = dropdownObj.AddComponent<TMP_Dropdown>();
            dropdown.targetGraphic = ddBg;
            dropdown.template = templateRT;
            dropdown.captionText = ddLabel;
            dropdown.itemText = itemLabel;

            // Add options
            dropdown.options.Clear();
            dropdown.options.Add(new TMP_Dropdown.OptionData("DigitRush"));
            dropdown.options.Add(new TMP_Dropdown.OptionData("FlashTap"));
            dropdown.options.Add(new TMP_Dropdown.OptionData("MemoryPairs"));
            dropdown.options.Add(new TMP_Dropdown.OptionData("OddOneOut"));
            dropdown.options.Add(new TMP_Dropdown.OptionData("QuickMath"));
            dropdown.value = 0;
            dropdown.RefreshShownValue();

            // DropdownScrollFix: reparents dropdown list to root Canvas to avoid clipping
            dropdownObj.AddComponent<DigitPark.UI.DropdownScrollFix>();

            templateObj.SetActive(false);

            // --- View Details Button ---
            GameObject viewDetailsObj = new GameObject("ViewDetailsButton");
            viewDetailsObj.transform.SetParent(dropdownRow.transform, false);

            RectTransform vdRT = viewDetailsObj.AddComponent<RectTransform>();
            vdRT.anchorMin = new Vector2(0.65f, 0.1f);
            vdRT.anchorMax = new Vector2(1f, 0.9f);
            vdRT.sizeDelta = Vector2.zero;

            Image vdBg = viewDetailsObj.AddComponent<Image>();
            vdBg.color = new Color(0.08f, 0.07f, 0.12f, 0.9f);

            Button vdBtn = viewDetailsObj.AddComponent<Button>();
            ColorBlock vdColors = vdBtn.colors;
            vdColors.normalColor = Color.white;
            vdColors.highlightedColor = new Color(1f, 0.95f, 0.8f, 1f);
            vdColors.pressedColor = new Color(0.9f, 0.8f, 0.5f, 1f);
            vdBtn.colors = vdColors;
            vdBtn.targetGraphic = vdBg;

            Outline vdOutline = viewDetailsObj.AddComponent<Outline>();
            vdOutline.effectColor = CARD_BORDER;
            vdOutline.effectDistance = new Vector2(1.5f, -1.5f);

            GameObject vdTextObj = new GameObject("ViewDetailsButtonText");
            vdTextObj.transform.SetParent(viewDetailsObj.transform, false);

            RectTransform vdTextRT = vdTextObj.AddComponent<RectTransform>();
            vdTextRT.anchorMin = Vector2.zero;
            vdTextRT.anchorMax = Vector2.one;
            vdTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI vdText = vdTextObj.AddComponent<TextMeshProUGUI>();
            vdText.text = "Game Info";
            vdText.fontSize = FontSizes.Body;
            vdText.color = TEXT_GOLD;
            vdText.fontStyle = FontStyles.Bold;
            vdText.alignment = TextAlignmentOptions.Center;
            vdText.enableAutoSizing = true;
            vdText.fontSizeMin = FontSizes.AutoMinBody;
            vdText.fontSizeMax = FontSizes.Body;

            Debug.Log("[CashBattle1v1] Game Selector created");
        }

        private static void BuildGameSelectorOnly()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            // Standardize CanvasScaler
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
            }

            Transform panel = canvas.transform.Find("SafeArea/MainContentPanel");
            if (panel == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontro MainContentPanel. Construye la UI completa primero.", "OK");
                return;
            }

            // Clean up old elements
            Transform oldScroll = panel.Find("GamesScrollView");
            if (oldScroll != null) DestroyImmediate(oldScroll.gameObject);
            Transform oldGrid = panel.Find("GamesContainer");
            if (oldGrid != null) DestroyImmediate(oldGrid.gameObject);
            Transform oldSelector = panel.Find("GameSelectorContainer");
            if (oldSelector != null) DestroyImmediate(oldSelector.gameObject);

            CreateGameSelector(panel);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        #endregion

        #region Rounds Selector

        private static void CreateRoundsSelector(Transform parent)
        {
            GameObject container = new GameObject("RoundsSelectorContainer");
            container.transform.SetParent(parent, false);

            RectTransform rt = container.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.71f);
            rt.anchorMax = new Vector2(1, 0.78f);
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = new Vector2(20, 0);
            rt.offsetMax = new Vector2(-20, 0);

            HorizontalLayoutGroup hlg = container.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.padding = new RectOffset(8, 8, 4, 4);

            // 3 round buttons: 1, 3, 5 — default "1" selected
            CreateRoundButton("Rounds1Button", container.transform, "1", true);
            CreateRoundButton("Rounds3Button", container.transform, "3", false);
            CreateRoundButton("Rounds5Button", container.transform, "5", false);
        }

        private static void CreateRoundButton(string name, Transform parent, string text, bool selected)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();

            Image bg = go.AddComponent<Image>();
            bg.color = selected ? BUTTON_GOLD : CARD_BG;

            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = selected ? new Color(1f, 0.84f, 0f, 0.6f) : new Color(1f, 1f, 1f, 0.1f);
            outline.effectDistance = new Vector2(1, -1);

            Button btn = go.AddComponent<Button>();
            var c = btn.colors;
            c.normalColor = Color.white;
            c.highlightedColor = new Color(1, 1, 1, 0.9f);
            c.pressedColor = new Color(0.75f, 0.75f, 0.75f);
            btn.colors = c;

            GameObject textGO = new GameObject(name + "Text");
            textGO.transform.SetParent(go.transform, false);
            RectTransform textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 28;
            tmp.color = selected ? Color.white : new Color(0.6f, 0.6f, 0.7f);
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 18;
            tmp.fontSizeMax = 28;
        }

        #endregion

        #region Hero Earnings

        private static void CreateHeroEarnings(Transform parent)
        {
            GameObject hero = new GameObject("HeroEarnings");
            hero.transform.SetParent(parent, false);

            RectTransform heroRT = hero.AddComponent<RectTransform>();
            heroRT.anchorMin = new Vector2(0, 0.50f);
            heroRT.anchorMax = new Vector2(1, 0.70f);
            heroRT.sizeDelta = Vector2.zero;
            heroRT.offsetMin = new Vector2(5, 5);
            heroRT.offsetMax = new Vector2(-5, -5);

            Image heroBg = hero.AddComponent<Image>();
            heroBg.color = new Color(0.04f, 0.08f, 0.04f, 0.95f);

            Outline heroOutline = hero.AddComponent<Outline>();
            heroOutline.effectColor = new Color(1f, 0.84f, 0f, 0.4f);
            heroOutline.effectDistance = new Vector2(2, -2);

            // Label: "IF YOU WIN"
            GameObject labelObj = new GameObject("HeroEarningsLabel");
            labelObj.transform.SetParent(hero.transform, false);

            RectTransform labelRT = labelObj.AddComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 0.7f);
            labelRT.anchorMax = new Vector2(1, 1f);
            labelRT.sizeDelta = Vector2.zero;
            labelRT.offsetMin = new Vector2(10, 0);
            labelRT.offsetMax = new Vector2(-10, -5);

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = "If you win you receive";
            labelText.fontSize = FontSizes.Caption;
            labelText.color = new Color(0.5f, 1f, 0.7f, 0.7f);
            labelText.fontStyle = FontStyles.Bold;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.enableAutoSizing = true;
            labelText.fontSizeMin = FontSizes.AutoMinSmall;
            labelText.fontSizeMax = FontSizes.Caption;

            // HERO amount
            GameObject amountObj = new GameObject("PotentialEarningsText");
            amountObj.transform.SetParent(hero.transform, false);

            RectTransform amountRT = amountObj.AddComponent<RectTransform>();
            amountRT.anchorMin = new Vector2(0, 0.2f);
            amountRT.anchorMax = new Vector2(1, 0.72f);
            amountRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI amountText = amountObj.AddComponent<TextMeshProUGUI>();
            amountText.text = "$0.00";
            amountText.fontSize = FontSizes.Symbol;
            amountText.color = GOLD_PRIMARY;
            amountText.fontStyle = FontStyles.Bold;
            amountText.alignment = TextAlignmentOptions.Center;
            amountText.enableAutoSizing = true;
            amountText.fontSizeMin = FontSizes.H1;
            amountText.fontSizeMax = FontSizes.Symbol;

            // Glow effect on amount
            Outline amountGlow = amountObj.AddComponent<Outline>();
            amountGlow.effectColor = new Color(1f, 0.7f, 0f, 0.3f);
            amountGlow.effectDistance = new Vector2(2, -2);

            // Pool info (subtle)
            GameObject poolObj = new GameObject("PoolInfoText");
            poolObj.transform.SetParent(hero.transform, false);

            RectTransform poolRT = poolObj.AddComponent<RectTransform>();
            poolRT.anchorMin = new Vector2(0, 0f);
            poolRT.anchorMax = new Vector2(1, 0.22f);
            poolRT.sizeDelta = Vector2.zero;
            poolRT.offsetMin = new Vector2(10, 3);
            poolRT.offsetMax = new Vector2(-10, 0);

            TextMeshProUGUI poolText = poolObj.AddComponent<TextMeshProUGUI>();
            poolText.text = "Pool: $0.00 | Your bet: $0.00 | Fee: 30%";
            poolText.fontSize = FontSizes.Caption;
            poolText.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
            poolText.fontStyle = FontStyles.Bold;
            poolText.alignment = TextAlignmentOptions.Center;
            poolText.enableAutoSizing = true;
            poolText.fontSizeMin = FontSizes.AutoMinSmall;
            poolText.fontSizeMax = FontSizes.Caption;
        }

        #endregion

        #region Entry Fee Section

        private static void CreateEntryFeeSection(Transform parent)
        {
            GameObject feeSection = new GameObject("EntryFeeSection");
            feeSection.transform.SetParent(parent, false);

            RectTransform rt = feeSection.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.13f);
            rt.anchorMax = new Vector2(1, 0.47f);
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = new Vector2(15, 5); // Padding
            rt.offsetMax = new Vector2(-15, -5);

            Image sectionBg = feeSection.AddComponent<Image>();
            sectionBg.color = new Color(0.06f, 0.05f, 0.09f, 0.95f); // Un poco mas oscuro

            // Borde dorado premium para la seccion de apuestas
            Outline sectionOutline = feeSection.AddComponent<Outline>();
            sectionOutline.effectColor = new Color(0.85f, 0.65f, 0.13f, 0.5f);
            sectionOutline.effectDistance = new Vector2(2, -2);

            // Title
            CreateFeeTitle(feeSection.transform);

            // Selected Fee display
            CreateSelectedFeeText(feeSection.transform);

            // Preset buttons
            CreatePresetButtons(feeSection.transform);

            // Custom input
            CreateCustomInput(feeSection.transform);
        }

        private static void CreateFeeTitle(Transform parent)
        {
            GameObject titleObj = new GameObject("FeeTitleText");
            titleObj.transform.SetParent(parent, false);

            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0.82f);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.sizeDelta = Vector2.zero;
            titleRT.offsetMin = new Vector2(15, 0);
            titleRT.offsetMax = new Vector2(-15, -5);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Choose your bet";
            titleText.fontSize = FontSizes.H3;
            titleText.color = TEXT_GOLD;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = FontSizes.AutoMinSmall;
            titleText.fontSizeMax = FontSizes.H3;
        }

        private static void CreateSelectedFeeText(Transform parent)
        {
            GameObject obj = new GameObject("SelectedFeeText");
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.55f, 0.82f);
            rt.anchorMax = new Vector2(1, 1);
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(-15, -5);

            TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
            text.text = "$5.00";
            text.fontSize = FontSizes.H3;
            text.color = TEXT_PRIMARY;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Right;
            text.enableAutoSizing = true;
            text.fontSizeMin = FontSizes.AutoMinSmall;
            text.fontSizeMax = FontSizes.H3;
        }

        private static void CreatePresetButtons(Transform parent)
        {
            GameObject container = new GameObject("PresetsContainer");
            container.transform.SetParent(parent, false);

            RectTransform containerRT = container.AddComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0, 0.42f);
            containerRT.anchorMax = new Vector2(1, 0.78f);
            containerRT.sizeDelta = Vector2.zero;
            containerRT.offsetMin = new Vector2(10, 0);
            containerRT.offsetMax = new Vector2(-10, 0);

            HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 12;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.padding = new RectOffset(5, 5, 5, 5);

            // Presets: $1, $5, $10, $25, $50, $100
            decimal[] presets = { 1m, 5m, 10m, 25m, 50m, 100m };
            foreach (var preset in presets)
            {
                CreatePresetButton(container.transform, preset);
            }
        }

        private static void CreatePresetButton(Transform parent, decimal amount)
        {
            GameObject btnObj = new GameObject($"Preset_{amount}");
            btnObj.transform.SetParent(parent, false);

            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;
            le.preferredHeight = 55;

            Image bg = btnObj.AddComponent<Image>();
            bg.color = new Color(0.18f, 0.15f, 0.22f, 1f);

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = new Color(0.18f, 0.15f, 0.22f, 1f);
            colors.highlightedColor = new Color(0.85f, 0.65f, 0.13f, 0.6f);
            colors.pressedColor = GOLD_PRIMARY;
            colors.selectedColor = GOLD_PRIMARY;
            btn.colors = colors;
            btn.targetGraphic = bg;
            // Sin Outline - los botones de preset no necesitan borde adicional

            GameObject textObj = new GameObject("PresetAmountText");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = $"${amount}";
            text.fontSize = FontSizes.Body;
            text.color = TEXT_PRIMARY;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.enableAutoSizing = true;
            text.fontSizeMin = FontSizes.AutoMinSmall;
            text.fontSizeMax = FontSizes.Body;

            // Selection indicator
            GameObject indicator = new GameObject("SelectedIndicator");
            indicator.transform.SetParent(btnObj.transform, false);

            RectTransform indRT = indicator.AddComponent<RectTransform>();
            indRT.anchorMin = new Vector2(0.5f, 0);
            indRT.anchorMax = new Vector2(0.5f, 0);
            indRT.pivot = new Vector2(0.5f, 0);
            indRT.sizeDelta = new Vector2(40, 4);
            indRT.anchoredPosition = new Vector2(0, 2);

            Image indImg = indicator.AddComponent<Image>();
            indImg.color = GOLD_PRIMARY;

            indicator.SetActive(false);
        }

        private static void CreateCustomInput(Transform parent)
        {
            GameObject container = new GameObject("CustomInputContainer");
            container.transform.SetParent(parent, false);

            RectTransform containerRT = container.AddComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0, 0.03f);
            containerRT.anchorMax = new Vector2(1, 0.40f);
            containerRT.sizeDelta = Vector2.zero;
            containerRT.offsetMin = new Vector2(15, 0);
            containerRT.offsetMax = new Vector2(-15, 0);

            // Dollar sign
            GameObject dollarSign = new GameObject("DollarSign");
            dollarSign.transform.SetParent(container.transform, false);

            RectTransform dollarRT = dollarSign.AddComponent<RectTransform>();
            dollarRT.anchorMin = new Vector2(0, 0);
            dollarRT.anchorMax = new Vector2(0.08f, 1);
            dollarRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI dollarText = dollarSign.AddComponent<TextMeshProUGUI>();
            dollarText.text = "$";
            dollarText.fontSize = FontSizes.Subtitle;
            dollarText.color = GOLD_PRIMARY;
            dollarText.fontStyle = FontStyles.Bold;
            dollarText.alignment = TextAlignmentOptions.Center;
            dollarText.enableAutoSizing = true;
            dollarText.fontSizeMin = FontSizes.AutoMinBody;
            dollarText.fontSizeMax = FontSizes.Subtitle;
            dollarText.overflowMode = TextOverflowModes.Ellipsis;

            // Input field
            GameObject inputBg = new GameObject("CustomInputField");
            inputBg.transform.SetParent(container.transform, false);

            RectTransform inputBgRT = inputBg.AddComponent<RectTransform>();
            inputBgRT.anchorMin = new Vector2(0.09f, 0.1f);
            inputBgRT.anchorMax = new Vector2(0.55f, 0.9f);
            inputBgRT.sizeDelta = Vector2.zero;

            Image inputBgImg = inputBg.AddComponent<Image>();
            inputBgImg.color = new Color(0.15f, 0.12f, 0.18f, 1f);
            // Sin Outline - campo de input usa color de fondo

            // Input text
            GameObject inputTextArea = new GameObject("InputFieldText");
            inputTextArea.transform.SetParent(inputBg.transform, false);

            RectTransform inputTextRT = inputTextArea.AddComponent<RectTransform>();
            inputTextRT.anchorMin = Vector2.zero;
            inputTextRT.anchorMax = Vector2.one;
            inputTextRT.sizeDelta = Vector2.zero;
            inputTextRT.offsetMin = new Vector2(10, 0);
            inputTextRT.offsetMax = new Vector2(-10, 0);

            TextMeshProUGUI inputText = inputTextArea.AddComponent<TextMeshProUGUI>();
            inputText.text = "";
            inputText.fontSize = FontSizes.H4;
            inputText.color = TEXT_PRIMARY;
            inputText.fontStyle = FontStyles.Bold;
            inputText.alignment = TextAlignmentOptions.Left;
            inputText.enableAutoSizing = true;
            inputText.fontSizeMin = FontSizes.AutoMinBody;
            inputText.fontSizeMax = FontSizes.H4;
            inputText.overflowMode = TextOverflowModes.Ellipsis;

            // Placeholder
            GameObject placeholder = new GameObject("Placeholder");
            placeholder.transform.SetParent(inputBg.transform, false);

            RectTransform placeholderRT = placeholder.AddComponent<RectTransform>();
            placeholderRT.anchorMin = Vector2.zero;
            placeholderRT.anchorMax = Vector2.one;
            placeholderRT.sizeDelta = Vector2.zero;
            placeholderRT.offsetMin = new Vector2(10, 0);
            placeholderRT.offsetMax = new Vector2(-10, 0);

            TextMeshProUGUI placeholderText = placeholder.AddComponent<TextMeshProUGUI>();
            placeholderText.text = "Other amount...";
            placeholderText.fontSize = FontSizes.H4;
            placeholderText.color = TEXT_SECONDARY;
            placeholderText.fontStyle = FontStyles.Bold;
            placeholderText.alignment = TextAlignmentOptions.Left;
            placeholderText.enableAutoSizing = true;
            placeholderText.fontSizeMin = FontSizes.AutoMinBody;
            placeholderText.fontSizeMax = FontSizes.H4;
            placeholderText.overflowMode = TextOverflowModes.Ellipsis;

            // TMP_InputField
            TMP_InputField inputField = inputBg.AddComponent<TMP_InputField>();
            inputField.textViewport = inputTextRT;
            inputField.textComponent = inputText;
            inputField.placeholder = placeholderText;
            inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
            inputField.characterLimit = 6;

            // Max label
            GameObject maxLabel = new GameObject("MaxLabel");
            maxLabel.transform.SetParent(container.transform, false);

            RectTransform maxRT = maxLabel.AddComponent<RectTransform>();
            maxRT.anchorMin = new Vector2(0.57f, 0);
            maxRT.anchorMax = new Vector2(0.78f, 1);
            maxRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI maxText = maxLabel.AddComponent<TextMeshProUGUI>();
            maxText.text = "Max: $250";
            maxText.fontSize = FontSizes.Body;
            maxText.color = TEXT_SECONDARY;
            maxText.fontStyle = FontStyles.Bold;
            maxText.alignment = TextAlignmentOptions.Center;
            maxText.enableAutoSizing = true;
            maxText.fontSizeMin = FontSizes.AutoMinSmall;
            maxText.fontSizeMax = FontSizes.Body;

            // Apply button
            GameObject applyBtn = new GameObject("ApplyButton");
            applyBtn.transform.SetParent(container.transform, false);

            RectTransform applyRT = applyBtn.AddComponent<RectTransform>();
            applyRT.anchorMin = new Vector2(0.8f, 0.1f);
            applyRT.anchorMax = new Vector2(1f, 0.9f);
            applyRT.sizeDelta = Vector2.zero;

            Image applyBg = applyBtn.AddComponent<Image>();
            applyBg.color = CYAN_ACCENT;

            Button applyButton = applyBtn.AddComponent<Button>();
            ColorBlock applyColors = applyButton.colors;
            applyColors.normalColor = CYAN_ACCENT;
            applyColors.highlightedColor = new Color(0.3f, 1f, 1f, 1f);
            applyColors.pressedColor = new Color(0f, 0.7f, 0.8f, 1f);
            applyButton.colors = applyColors;

            GameObject applyTextObj = new GameObject("OkButtonText");
            applyTextObj.transform.SetParent(applyBtn.transform, false);

            RectTransform applyTextRT = applyTextObj.AddComponent<RectTransform>();
            applyTextRT.anchorMin = Vector2.zero;
            applyTextRT.anchorMax = Vector2.one;
            applyTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI applyText = applyTextObj.AddComponent<TextMeshProUGUI>();
            applyText.text = "OK";
            applyText.fontSize = FontSizes.Subtitle;
            applyText.color = BG_DARK;
            applyText.fontStyle = FontStyles.Bold;
            applyText.alignment = TextAlignmentOptions.Center;
            applyText.enableAutoSizing = true;
            applyText.fontSizeMin = FontSizes.AutoMinBody;
            applyText.fontSizeMax = FontSizes.Subtitle;
            applyText.overflowMode = TextOverflowModes.Ellipsis;
        }

        // CreateEarningsFeedback removed - earnings moved to HeroEarnings section

        private static void BuildEntryFeeSectionOnly()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            // Standardize CanvasScaler
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
            }

            Transform panel = canvas.transform.Find("SafeArea/MainContentPanel");
            if (panel == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontro MainContentPanel. Construye la UI completa primero.", "OK");
                return;
            }

            Transform old = panel.Find("EntryFeeSection");
            if (old != null) DestroyImmediate(old.gameObject);

            CreateEntryFeeSection(panel);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        #endregion

        #region Find Opponent Button

        private static void CreateFindOpponentButton(Transform parent)
        {
            GameObject container = new GameObject("FindOpponentContainer");
            container.transform.SetParent(parent, false);

            RectTransform containerRT = container.AddComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0.05f, 0.01f); // Pequeño margen abajo
            containerRT.anchorMax = new Vector2(0.95f, 0.095f); // Un poco mas alto
            containerRT.sizeDelta = Vector2.zero;

            GameObject btnObj = new GameObject("FindOpponentButton");
            btnObj.transform.SetParent(container.transform, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image bg = btnObj.AddComponent<Image>();
            bg.color = BUTTON_GOLD;

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = BUTTON_GOLD;
            colors.highlightedColor = GOLD_LIGHT;
            colors.pressedColor = GOLD_DARK;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            btn.colors = colors;
            btn.targetGraphic = bg;

            // Un solo Outline sutil para el boton (optimizado para rendimiento)
            Outline btnOutline = btnObj.AddComponent<Outline>();
            btnOutline.effectColor = new Color(1f, 0.75f, 0.2f, 0.6f);
            btnOutline.effectDistance = new Vector2(3, -3);

            // Main text - MAS GRANDE
            GameObject textObj = new GameObject("FindOpponentText");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0, 0);
            textRT.anchorMax = new Vector2(1, 1);
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "Find opponent";
            text.fontSize = FontSizes.H1;
            text.color = BG_DARK;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.enableAutoSizing = true;
            text.fontSizeMin = FontSizes.AutoMinSmall;
            text.fontSizeMax = FontSizes.H1;

            // Sin decoradores - diseño limpio
        }

        private static void BuildFindOpponentOnly()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            // Standardize CanvasScaler
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
            }

            Transform panel = canvas.transform.Find("SafeArea/MainContentPanel");
            if (panel == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontro MainContentPanel. Construye la UI completa primero.", "OK");
                return;
            }

            Transform old = panel.Find("FindOpponentContainer");
            if (old != null) DestroyImmediate(old.gameObject);

            CreateFindOpponentButton(panel);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        #endregion

        #region Cognitive Sprint

        private static void CreateCognitiveSprintSection(Transform parent)
        {
            // === Overlay blocker (semi-transparente, se ve la escena detras) ===
            GameObject sprintPanel = new GameObject("CognitiveSprintPanel");
            sprintPanel.transform.SetParent(parent, false);

            RectTransform sprintPanelRT = sprintPanel.AddComponent<RectTransform>();
            sprintPanelRT.anchorMin = Vector2.zero;
            sprintPanelRT.anchorMax = Vector2.one;
            sprintPanelRT.sizeDelta = Vector2.zero;

            // Blocker overlay
            var blocker = new GameObject("BlockerPanel");
            blocker.transform.SetParent(sprintPanel.transform, false);
            blocker.transform.SetAsFirstSibling();
            var blockerRT = blocker.AddComponent<RectTransform>();
            blockerRT.anchorMin = Vector2.zero;
            blockerRT.anchorMax = Vector2.one;
            blockerRT.offsetMin = Vector2.zero;
            blockerRT.offsetMax = Vector2.zero;
            var blockerImg = blocker.AddComponent<Image>();
            blockerImg.color = new Color(0f, 0f, 0f, 0.7f);
            blockerImg.raycastTarget = true;

            // === Card central (popup) ===
            GameObject card = new GameObject("SprintCard");
            card.transform.SetParent(sprintPanel.transform, false);

            RectTransform cardRT = card.AddComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0.05f, 0.5f);
            cardRT.anchorMax = new Vector2(0.95f, 0.5f);
            cardRT.pivot = new Vector2(0.5f, 0.5f);
            cardRT.sizeDelta = new Vector2(0, 0); // alto lo controla ContentSizeFitter

            ContentSizeFitter csf = card.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.08f, 0.06f, 0.11f, 0.97f);

            Outline cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = new Color(0.85f, 0.65f, 0.13f, 0.4f);
            cardOutline.effectDistance = new Vector2(2, -2);

            // VerticalLayout para todo el contenido del popup
            VerticalLayoutGroup cardLayout = card.AddComponent<VerticalLayoutGroup>();
            cardLayout.spacing = 8;
            cardLayout.padding = new RectOffset(20, 20, 25, 20);
            cardLayout.childAlignment = TextAnchor.UpperCenter;
            cardLayout.childForceExpandWidth = true;
            cardLayout.childForceExpandHeight = false;
            cardLayout.childControlWidth = true;
            cardLayout.childControlHeight = false;

            // === Titulo ===
            GameObject titleObj = CreateLayoutText(card.transform, "SprintTitle",
                "COGNITIVE SPRINT", FontSizes.BodyLarge, TEXT_GOLD, FontStyles.Bold, 55);

            // === Subtitulo ===
            GameObject subtitleObj = CreateLayoutText(card.transform, "SprintSubtitle",
                "Select 2 to 5 games", FontSizes.Body, TEXT_SECONDARY, FontStyles.Bold, 30);

            // === Separador ===
            GameObject sep = new GameObject("Separator");
            sep.transform.SetParent(card.transform, false);
            LayoutElement sepLE = sep.AddComponent<LayoutElement>();
            sepLE.preferredHeight = 2;
            Image sepImg = sep.AddComponent<Image>();
            sepImg.color = new Color(0.85f, 0.65f, 0.13f, 0.3f);

            // === 5 Game Cards ===
            string[] sprintGames = { "DigitRush", "MemoryPairs", "QuickMath", "FlashTap", "OddOneOut" };
            string[] sprintIcons = { "DigitRushIcon", "MemoryPairsIcon", "QuickMathIcon", "FlashTapIcon", "OddOneOutIcon" };
            string[] sprintNames = { "Digit Rush", "Memory Pairs", "Quick Math", "Flash Tap", "Odd One Out" };

            for (int i = 0; i < sprintGames.Length; i++)
            {
                CreateSprintGameCard(card.transform, sprintGames[i], sprintIcons[i], sprintNames[i]);
            }

            // === Selection Text (justo debajo de cards) ===
            GameObject selTextObj = CreateLayoutText(card.transform, "SprintSelectionText",
                "Selected: 0/5 (min: 2)", FontSizes.Body, Color.yellow, FontStyles.Bold, 40);

            // === Botones (justo debajo del texto) ===
            GameObject buttonsRow = new GameObject("SprintButtons");
            buttonsRow.transform.SetParent(card.transform, false);

            LayoutElement btnRowLE = buttonsRow.AddComponent<LayoutElement>();
            btnRowLE.preferredHeight = 65;

            HorizontalLayoutGroup btnLayout = buttonsRow.AddComponent<HorizontalLayoutGroup>();
            btnLayout.spacing = 15;
            btnLayout.childAlignment = TextAnchor.MiddleCenter;
            btnLayout.childForceExpandWidth = true;
            btnLayout.childForceExpandHeight = true;
            btnLayout.childControlWidth = true;
            btnLayout.childControlHeight = true;
            btnLayout.padding = new RectOffset(0, 0, 0, 0);

            // Cancelar
            CreateSprintActionButton(buttonsRow.transform, "SprintCancelButton", "CANCEL",
                new Color(0.15f, 0.12f, 0.2f, 1f), new Color(0.5f, 0.45f, 0.6f, 1f),
                TEXT_SECONDARY, 1f);

            // Aceptar
            CreateSprintActionButton(buttonsRow.transform, "SprintAcceptButton", "ACCEPT",
                BUTTON_GOLD, new Color(1f, 0.75f, 0.2f, 0.6f),
                BG_DARK, 1f);

            sprintPanel.SetActive(false);

            Debug.Log("[CashBattle1v1] Cognitive Sprint popup creado");
        }

        private static GameObject CreateLayoutText(Transform parent, string name,
            string content, float fontSize, Color color, FontStyles style, float height)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            LayoutElement le = obj.AddComponent<LayoutElement>();
            le.preferredHeight = height;

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinSmall;
            tmp.fontSizeMax = fontSize;

            return obj;
        }

        private static void CreateSprintGameCard(Transform parent, string gameId, string iconName, string displayName)
        {
            GameObject card = new GameObject($"SprintCard_{gameId}");
            card.transform.SetParent(parent, false);

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredHeight = 110;
            le.flexibleWidth = 1;

            // Fondo de la card
            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.09f, 0.07f, 0.13f, 1f);

            // Borde sutil
            Outline cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = new Color(0.4f, 0.3f, 0.55f, 0.35f);
            cardOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // Button - toda la card es presionable
            Button btn = card.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = new Color(0.09f, 0.07f, 0.13f, 1f);
            colors.highlightedColor = new Color(0.14f, 0.11f, 0.2f, 1f);
            colors.pressedColor = new Color(0.2f, 0.15f, 0.08f, 1f);
            colors.selectedColor = new Color(0.12f, 0.1f, 0.06f, 1f);
            btn.colors = colors;
            btn.targetGraphic = cardBg;

            // HorizontalLayoutGroup para distribuir icon | name | circle
            HorizontalLayoutGroup hlg = card.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.padding = new RectOffset(8, 8, 8, 8);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth = true;
            hlg.childControlHeight = false;

            // === Icono (lado izquierdo) ===
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(card.transform, false);

            LayoutElement iconLE = iconObj.AddComponent<LayoutElement>();
            iconLE.preferredWidth = 85;
            iconLE.preferredHeight = 85;
            iconLE.flexibleWidth = 0;

            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            string iconPath = $"Assets/_Project/Art/Icons/Games/{iconName}.png";
            Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            if (iconSprite != null)
            {
                iconImg.sprite = iconSprite;
                iconImg.color = Color.white;
            }

            // === Nombre del juego (centro, llena el espacio restante) ===
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(card.transform, false);

            LayoutElement nameLE = nameObj.AddComponent<LayoutElement>();
            nameLE.flexibleWidth = 1;
            nameLE.preferredHeight = 85;

            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = displayName;
            nameText.fontSize = FontSizes.Body;
            nameText.color = TEXT_PRIMARY;
            nameText.fontStyle = FontStyles.Bold;
            nameText.alignment = TextAlignmentOptions.Left;
            nameText.enableWordWrapping = false;
            nameText.overflowMode = TextOverflowModes.Ellipsis;
            nameText.raycastTarget = false;
            nameText.enableAutoSizing = true;
            nameText.fontSizeMin = FontSizes.AutoMinSmall;
            nameText.fontSizeMax = FontSizes.Body;

            // === Circulo de seleccion (derecha) ===
            GameObject circleObj = new GameObject("SelectCircle");
            circleObj.transform.SetParent(card.transform, false);

            LayoutElement circleLE = circleObj.AddComponent<LayoutElement>();
            circleLE.preferredWidth = 44;
            circleLE.preferredHeight = 44;
            circleLE.flexibleWidth = 0;

            Image circleBg = circleObj.AddComponent<Image>();
            circleBg.color = new Color(0.2f, 0.17f, 0.28f, 1f);

            // === Checkmark (dentro del circulo, oculto hasta seleccionar) ===
            GameObject checkObj = new GameObject("Checkmark");
            checkObj.transform.SetParent(circleObj.transform, false);

            RectTransform checkRT = checkObj.AddComponent<RectTransform>();
            checkRT.anchorMin = Vector2.zero;
            checkRT.anchorMax = Vector2.one;
            checkRT.sizeDelta = Vector2.zero;

            Image checkBg = checkObj.AddComponent<Image>();
            checkBg.color = new Color(0.2f, 0.9f, 0.4f, 1f);

            GameObject checkIconObj = new GameObject("CheckIcon");
            checkIconObj.transform.SetParent(checkObj.transform, false);

            RectTransform checkIconRT = checkIconObj.AddComponent<RectTransform>();
            checkIconRT.anchorMin = new Vector2(0.15f, 0.15f);
            checkIconRT.anchorMax = new Vector2(0.85f, 0.85f);
            checkIconRT.sizeDelta = Vector2.zero;

            var checkImg = checkIconObj.AddComponent<Image>();
            Sprite checkSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/UI/icon_checkmark.png");
            if (checkSprite != null) checkImg.sprite = checkSprite;
            checkImg.color = Color.white;
            checkImg.preserveAspect = true;
            checkImg.raycastTarget = false;

            checkObj.SetActive(false);
        }

        private static void CreateSprintActionButton(Transform parent, string name, string label,
            Color bgColor, Color outlineColor, Color textColor, float flexWeight)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.flexibleWidth = flexWeight;
            le.preferredHeight = 60;

            Image bg = btnObj.AddComponent<Image>();
            bg.color = bgColor;

            Outline outline = btnObj.AddComponent<Outline>();
            outline.effectColor = outlineColor;
            outline.effectDistance = new Vector2(2, -2);

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = bgColor;
            colors.highlightedColor = new Color(
                Mathf.Min(bgColor.r * 1.2f, 1f),
                Mathf.Min(bgColor.g * 1.2f, 1f),
                Mathf.Min(bgColor.b * 1.2f, 1f), 1f);
            colors.pressedColor = new Color(bgColor.r * 0.8f, bgColor.g * 0.8f, bgColor.b * 0.8f, 1f);
            btn.colors = colors;
            btn.targetGraphic = bg;

            GameObject textObj = new GameObject(name + "Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = FontSizes.Body;
            text.color = textColor;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = false;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.enableAutoSizing = true;
            text.fontSizeMin = FontSizes.AutoMinSmall;
            text.fontSizeMax = FontSizes.Body;
        }

        #endregion

        #region Game Selection Modal

        private static void CreateGameSelectionModal(Transform parent)
        {
            // === Fullscreen overlay (hidden by default) ===
            GameObject modal = new GameObject("GameSelectionModal");
            modal.transform.SetParent(parent, false);

            RectTransform modalRT = modal.AddComponent<RectTransform>();
            modalRT.anchorMin = Vector2.zero;
            modalRT.anchorMax = Vector2.one;
            modalRT.sizeDelta = Vector2.zero;

            Image modalBg = modal.AddComponent<Image>();
            modalBg.color = new Color(0f, 0f, 0f, 0.85f);

            // === Centered panel ===
            GameObject panel = new GameObject("ModalPanel");
            panel.transform.SetParent(modal.transform, false);

            RectTransform panelRT = panel.AddComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.05f, 0.15f);
            panelRT.anchorMax = new Vector2(0.95f, 0.85f);
            panelRT.sizeDelta = Vector2.zero;

            Image panelBg = panel.AddComponent<Image>();
            panelBg.color = CARD_BG;

            Outline panelOutline = panel.AddComponent<Outline>();
            panelOutline.effectColor = CARD_BORDER;
            panelOutline.effectDistance = new Vector2(2, -2);

            // === Close button (X in top-right corner) ===
            GameObject closeObj = new GameObject("CloseModalButton");
            closeObj.transform.SetParent(panel.transform, false);

            RectTransform closeRT = closeObj.AddComponent<RectTransform>();
            closeRT.anchorMin = new Vector2(1, 1);
            closeRT.anchorMax = new Vector2(1, 1);
            closeRT.pivot = new Vector2(1, 1);
            closeRT.sizeDelta = new Vector2(50, 50);
            closeRT.anchoredPosition = new Vector2(-8, -8);

            Image closeBg = closeObj.AddComponent<Image>();
            closeBg.color = new Color(0.2f, 0.15f, 0.25f, 0.9f);

            Button closeBtn = closeObj.AddComponent<Button>();
            ColorBlock closeColors = closeBtn.colors;
            closeColors.normalColor = new Color(0.2f, 0.15f, 0.25f, 0.9f);
            closeColors.highlightedColor = new Color(0.4f, 0.2f, 0.3f, 1f);
            closeColors.pressedColor = new Color(0.6f, 0.2f, 0.2f, 1f);
            closeBtn.colors = closeColors;
            closeBtn.targetGraphic = closeBg;

            GameObject closeTextObj = new GameObject("CloseButtonText");
            closeTextObj.transform.SetParent(closeObj.transform, false);

            RectTransform closeTextRT = closeTextObj.AddComponent<RectTransform>();
            closeTextRT.anchorMin = Vector2.zero;
            closeTextRT.anchorMax = Vector2.one;
            closeTextRT.sizeDelta = Vector2.zero;

            // Sprite close icon
            Image closeImg = closeTextObj.AddComponent<Image>();
            Sprite closeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/UI/icon_close_x.png");
            if (closeSprite != null) closeImg.sprite = closeSprite;
            closeImg.color = TEXT_PRIMARY;
            closeImg.preserveAspect = true;
            closeImg.raycastTarget = false;

            // === Title ===
            GameObject titleObj = new GameObject("ModalTitle");
            titleObj.transform.SetParent(panel.transform, false);

            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0.92f);
            titleRT.anchorMax = new Vector2(1, 1f);
            titleRT.sizeDelta = Vector2.zero;
            titleRT.offsetMin = new Vector2(15, 0);
            titleRT.offsetMax = new Vector2(-60, -5);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Game Info";
            titleText.fontSize = FontSizes.H4;
            titleText.color = TEXT_GOLD;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = FontSizes.AutoMinTitle;
            titleText.fontSizeMax = FontSizes.H4;

            // === Game Icon (large, centered) ===
            GameObject iconObj = new GameObject("GameIcon");
            iconObj.transform.SetParent(panel.transform, false);
            var iconRT = iconObj.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.2f, 0.55f);
            iconRT.anchorMax = new Vector2(0.8f, 0.88f);
            iconRT.offsetMin = Vector2.zero;
            iconRT.offsetMax = Vector2.zero;
            var iconImg = iconObj.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.color = Color.white;
            // Load default game icon
            Sprite defaultIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/Games/DigitRushIcon.png");
            if (defaultIcon != null) iconImg.sprite = defaultIcon;

            // === Game Name ===
            GameObject nameObj = new GameObject("GameName");
            nameObj.transform.SetParent(panel.transform, false);
            var nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0.42f);
            nameRT.anchorMax = new Vector2(1, 0.55f);
            nameRT.offsetMin = new Vector2(20, 0);
            nameRT.offsetMax = new Vector2(-20, 0);
            var nameTMP = nameObj.AddComponent<TextMeshProUGUI>();
            nameTMP.text = "DIGIT RUSH";
            nameTMP.fontSize = FontSizes.H2;
            nameTMP.fontStyle = FontStyles.Bold;
            nameTMP.color = TEXT_GOLD;
            nameTMP.alignment = TextAlignmentOptions.Center;
            nameTMP.enableAutoSizing = true;
            nameTMP.fontSizeMin = FontSizes.AutoMinTitle;
            nameTMP.fontSizeMax = FontSizes.H2;

            // === Game Description ===
            GameObject descObj = new GameObject("GameDescription");
            descObj.transform.SetParent(panel.transform, false);
            var descRT = descObj.AddComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0, 0.18f);
            descRT.anchorMax = new Vector2(1, 0.42f);
            descRT.offsetMin = new Vector2(25, 0);
            descRT.offsetMax = new Vector2(-25, 0);
            var descTMP = descObj.AddComponent<TextMeshProUGUI>();
            descTMP.text = "Type the numbers that appear on screen as fast as you can.\n\n" +
                           "<color=#FFD700>Duration:</color> 60 seconds\n" +
                           "<color=#FFD700>Scoring:</color> Speed + accuracy\n" +
                           "<color=#FFD700>Tip:</color> Focus on accuracy first!";
            descTMP.fontSize = FontSizes.Body;
            descTMP.fontStyle = FontStyles.Normal;
            descTMP.color = TEXT_PRIMARY;
            descTMP.alignment = TextAlignmentOptions.Left;
            descTMP.enableAutoSizing = true;
            descTMP.fontSizeMin = FontSizes.AutoMinSmall;
            descTMP.fontSizeMax = FontSizes.Body;
            descTMP.enableWordWrapping = true;

            // === Close button (bottom, replaces Confirm) ===
            GameObject confirmObj = new GameObject("ConfirmGameButton");
            confirmObj.transform.SetParent(panel.transform, false);

            RectTransform confirmRT = confirmObj.AddComponent<RectTransform>();
            confirmRT.anchorMin = new Vector2(0.15f, 0.03f);
            confirmRT.anchorMax = new Vector2(0.85f, 0.14f);
            confirmRT.sizeDelta = Vector2.zero;

            Image confirmBg = confirmObj.AddComponent<Image>();
            confirmBg.color = BUTTON_GOLD;

            Button confirmBtn = confirmObj.AddComponent<Button>();
            ColorBlock confirmColors = confirmBtn.colors;
            confirmColors.normalColor = BUTTON_GOLD;
            confirmColors.highlightedColor = GOLD_LIGHT;
            confirmColors.pressedColor = GOLD_DARK;
            confirmBtn.colors = confirmColors;
            confirmBtn.targetGraphic = confirmBg;

            Outline confirmOutline = confirmObj.AddComponent<Outline>();
            confirmOutline.effectColor = new Color(1f, 0.75f, 0.2f, 0.5f);
            confirmOutline.effectDistance = new Vector2(2, -2);

            GameObject confirmTextObj = new GameObject("ConfirmButtonText");
            confirmTextObj.transform.SetParent(confirmObj.transform, false);

            RectTransform confirmTextRT = confirmTextObj.AddComponent<RectTransform>();
            confirmTextRT.anchorMin = Vector2.zero;
            confirmTextRT.anchorMax = Vector2.one;
            confirmTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI confirmText = confirmTextObj.AddComponent<TextMeshProUGUI>();
            confirmText.text = "GOT IT";
            confirmText.fontSize = FontSizes.Body;
            confirmText.color = BG_DARK;
            confirmText.fontStyle = FontStyles.Bold;
            confirmText.alignment = TextAlignmentOptions.Center;
            confirmText.enableAutoSizing = true;
            confirmText.fontSizeMin = FontSizes.AutoMinBody;
            confirmText.fontSizeMax = FontSizes.Body;

            // Container for game cards (hidden, used by runtime)
            GameObject cardsContainer = new GameObject("GameCardsContainer");
            cardsContainer.transform.SetParent(panel.transform, false);
            var ccRT = cardsContainer.AddComponent<RectTransform>();
            ccRT.sizeDelta = Vector2.zero;
            cardsContainer.SetActive(false);

            // Hidden by default
            modal.SetActive(false);

            Debug.Log("[CashBattle1v1] Game Selection Modal created (hidden by default)");
        }

        private static void CreateModalGameCard(Transform parent, string gameId, string displayName, string description, string iconName)
        {
            GameObject card = new GameObject($"ModalCard_{gameId}");
            card.transform.SetParent(parent, false);

            // Card background
            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.09f, 0.07f, 0.13f, 1f);

            Outline cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = new Color(0.4f, 0.3f, 0.55f, 0.35f);
            cardOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // Button
            Button btn = card.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = new Color(0.09f, 0.07f, 0.13f, 1f);
            colors.highlightedColor = new Color(0.14f, 0.11f, 0.2f, 1f);
            colors.pressedColor = new Color(0.25f, 0.2f, 0.12f, 1f);
            colors.selectedColor = new Color(0.2f, 0.17f, 0.1f, 1f);
            btn.colors = colors;
            btn.targetGraphic = cardBg;

            // Game icon (80x80)
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(card.transform, false);

            RectTransform iconRT = iconObj.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0, 0.5f);
            iconRT.anchorMax = new Vector2(0, 0.5f);
            iconRT.pivot = new Vector2(0, 0.5f);
            iconRT.sizeDelta = new Vector2(80, 80);
            iconRT.anchoredPosition = new Vector2(10, 0);

            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            string iconPath = $"Assets/_Project/Art/Icons/Games/{iconName}.png";
            Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            if (iconSprite != null)
            {
                iconImg.sprite = iconSprite;
                iconImg.color = Color.white;
            }
            else
            {
                iconImg.color = new Color(0.4f, 0.4f, 0.4f, 0.5f);
                Debug.LogWarning($"[CashBattle1v1] Modal icon not found: {iconPath}");
            }

            // Game name
            GameObject nameObj = new GameObject("GameName");
            nameObj.transform.SetParent(card.transform, false);

            RectTransform nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0.55f);
            nameRT.anchorMax = new Vector2(1, 1f);
            nameRT.sizeDelta = Vector2.zero;
            nameRT.offsetMin = new Vector2(100, 0);
            nameRT.offsetMax = new Vector2(-10, -8);

            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = displayName;
            nameText.fontSize = FontSizes.Body;
            nameText.color = TEXT_PRIMARY;
            nameText.fontStyle = FontStyles.Bold;
            nameText.alignment = TextAlignmentOptions.Left;
            nameText.enableWordWrapping = false;
            nameText.overflowMode = TextOverflowModes.Ellipsis;
            nameText.raycastTarget = false;
            nameText.enableAutoSizing = true;
            nameText.fontSizeMin = FontSizes.AutoMinSmall;
            nameText.fontSizeMax = FontSizes.Body;

            // Game description
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(card.transform, false);

            RectTransform descRT = descObj.AddComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0, 0);
            descRT.anchorMax = new Vector2(1, 0.55f);
            descRT.sizeDelta = Vector2.zero;
            descRT.offsetMin = new Vector2(100, 8);
            descRT.offsetMax = new Vector2(-10, 0);

            TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = description;
            descText.fontSize = FontSizes.AutoMinBody;
            descText.color = TEXT_SECONDARY;
            descText.fontStyle = FontStyles.Bold;
            descText.alignment = TextAlignmentOptions.Left;
            descText.enableWordWrapping = true;
            descText.overflowMode = TextOverflowModes.Ellipsis;
            descText.raycastTarget = false;
            descText.enableAutoSizing = true;
            descText.fontSizeMin = Mathf.Min(FontSizes.AutoMinBody, FontSizes.AutoMinBody);
            descText.fontSizeMax = Mathf.Max(FontSizes.AutoMinBody, FontSizes.AutoMinBody);

            // Checkmark (hidden by default)
            GameObject checkmark = new GameObject("Checkmark");
            checkmark.transform.SetParent(card.transform, false);

            RectTransform checkRT = checkmark.AddComponent<RectTransform>();
            checkRT.anchorMin = new Vector2(1, 1);
            checkRT.anchorMax = new Vector2(1, 1);
            checkRT.pivot = new Vector2(1, 1);
            checkRT.sizeDelta = new Vector2(35, 35);
            checkRT.anchoredPosition = new Vector2(-5, -5);

            Image checkBg = checkmark.AddComponent<Image>();
            checkBg.color = new Color(0.2f, 0.95f, 0.4f, 1f);

            GameObject checkIconObj = new GameObject("CheckIcon");
            checkIconObj.transform.SetParent(checkmark.transform, false);

            RectTransform checkIconRT = checkIconObj.AddComponent<RectTransform>();
            checkIconRT.anchorMin = new Vector2(0.15f, 0.15f);
            checkIconRT.anchorMax = new Vector2(0.85f, 0.85f);
            checkIconRT.sizeDelta = Vector2.zero;

            var checkImg = checkIconObj.AddComponent<Image>();
            Sprite checkSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/UI/icon_checkmark.png");
            if (checkSprite != null) checkImg.sprite = checkSprite;
            checkImg.color = Color.white;
            checkImg.preserveAspect = true;
            checkImg.raycastTarget = false;

            checkmark.SetActive(false);
        }

        private static void CreateModalCognitiveSprintCard(Transform parent)
        {
            GameObject card = new GameObject("ModalCard_CognitiveSprint");
            card.transform.SetParent(parent, false);

            // Card background
            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.09f, 0.07f, 0.13f, 1f);

            Outline cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = new Color(0.6f, 0.3f, 1f, 0.5f);
            cardOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // Button
            Button btn = card.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = new Color(0.09f, 0.07f, 0.13f, 1f);
            colors.highlightedColor = new Color(0.14f, 0.11f, 0.2f, 1f);
            colors.pressedColor = new Color(0.25f, 0.2f, 0.12f, 1f);
            colors.selectedColor = new Color(0.2f, 0.17f, 0.1f, 1f);
            btn.colors = colors;
            btn.targetGraphic = cardBg;

            // Override layout to span full width
            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredWidth = 580;
            le.preferredHeight = 130;

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(card.transform, false);

            RectTransform iconRT = iconObj.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0, 0.5f);
            iconRT.anchorMax = new Vector2(0, 0.5f);
            iconRT.pivot = new Vector2(0, 0.5f);
            iconRT.sizeDelta = new Vector2(80, 80);
            iconRT.anchoredPosition = new Vector2(10, 0);

            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            string iconPath = "Assets/_Project/Art/Icons/Games/CognitiveSprintIcon.png";
            Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            if (iconSprite != null)
            {
                iconImg.sprite = iconSprite;
                iconImg.color = Color.white;
            }
            else
            {
                iconImg.color = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            }

            // PRO badge
            GameObject proBadge = new GameObject("ProBadge");
            proBadge.transform.SetParent(card.transform, false);

            RectTransform proRT = proBadge.AddComponent<RectTransform>();
            proRT.anchorMin = new Vector2(0, 1);
            proRT.anchorMax = new Vector2(0, 1);
            proRT.pivot = new Vector2(0, 1);
            proRT.sizeDelta = new Vector2(70, 30);
            proRT.anchoredPosition = new Vector2(8, -5);

            Image proBg = proBadge.AddComponent<Image>();
            proBg.color = new Color(0.6f, 0.3f, 1f, 0.95f);

            GameObject proTextObj = new GameObject("ProText");
            proTextObj.transform.SetParent(proBadge.transform, false);

            RectTransform proTextRT = proTextObj.AddComponent<RectTransform>();
            proTextRT.anchorMin = Vector2.zero;
            proTextRT.anchorMax = Vector2.one;
            proTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI proText = proTextObj.AddComponent<TextMeshProUGUI>();
            proText.text = "PRO";
            proText.fontSize = FontSizes.AutoMinBody;
            proText.color = Color.white;
            proText.fontStyle = FontStyles.Bold;
            proText.alignment = TextAlignmentOptions.Center;
            proText.raycastTarget = false;
            proText.enableAutoSizing = true;
            proText.fontSizeMin = Mathf.Min(FontSizes.AutoMinBody, FontSizes.AutoMinBody);
            proText.fontSizeMax = Mathf.Max(FontSizes.AutoMinBody, FontSizes.AutoMinBody);

            // Name
            GameObject nameObj = new GameObject("GameName");
            nameObj.transform.SetParent(card.transform, false);

            RectTransform nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0.55f);
            nameRT.anchorMax = new Vector2(1, 1f);
            nameRT.sizeDelta = Vector2.zero;
            nameRT.offsetMin = new Vector2(100, 0);
            nameRT.offsetMax = new Vector2(-10, -8);

            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "Cognitive Sprint";
            nameText.fontSize = FontSizes.Body;
            nameText.color = TEXT_PRIMARY;
            nameText.fontStyle = FontStyles.Bold;
            nameText.alignment = TextAlignmentOptions.Left;
            nameText.raycastTarget = false;
            nameText.enableAutoSizing = true;
            nameText.fontSizeMin = FontSizes.AutoMinBody;
            nameText.fontSizeMax = FontSizes.Body;
            nameText.overflowMode = TextOverflowModes.Ellipsis;

            // Description
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(card.transform, false);

            RectTransform descRT = descObj.AddComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0, 0);
            descRT.anchorMax = new Vector2(1, 0.55f);
            descRT.sizeDelta = Vector2.zero;
            descRT.offsetMin = new Vector2(100, 8);
            descRT.offsetMax = new Vector2(-10, 0);

            TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = "Play 2-5 games in a row!";
            descText.fontSize = FontSizes.AutoMinBody;
            descText.color = TEXT_SECONDARY;
            descText.fontStyle = FontStyles.Bold;
            descText.alignment = TextAlignmentOptions.Left;
            descText.enableWordWrapping = true;
            descText.raycastTarget = false;
            descText.enableAutoSizing = true;
            descText.fontSizeMin = Mathf.Min(FontSizes.AutoMinBody, FontSizes.AutoMinBody);
            descText.fontSizeMax = Mathf.Max(FontSizes.AutoMinBody, FontSizes.AutoMinBody);
        }

        #endregion

        #region Reference Assigner

        private static MonoBehaviour FindCashBattle1v1Manager()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "CashBattle1v1Manager") return mb;
            return null;
        }

        private static void ResetAssignState()
        {
            assignedCount = 0; failedCount = 0; alreadySetCount = 0;
            assignResults.Clear();
        }

        private static void RunAssignAllReferences()
        {
            var manager = FindCashBattle1v1Manager();
            if (manager == null)
            {
                Debug.LogError("[CashBattle1v1] CashBattle1v1Manager no encontrado!");
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            so.Update();

            // Header
            AssignRef(so, "titleText", FindText("titletext"));
            AssignRef(so, "backButton", FindBtn("backbutton"));

            // Game Selection (dropdown + details)
            AssignRef(so, "gameDropdown", FindComp<TMP_Dropdown>("gamedropdown"));
            AssignRef(so, "viewDetailsButton", FindBtn("viewdetails"));
            AssignRef(so, "selectedGameIcon", FindImg("selectedgameicon"));
            AssignRef(so, "selectedGameDescription", FindText("selectedgamedesc"));

            // Game Selection Modal
            AssignGO(so, "gameSelectionModal", "GameSelectionModal");
            AssignRef(so, "gameCardsContainer", FindComp<Transform>("gamecardscontainer"));
            AssignRef(so, "confirmGameButton", FindBtn("confirmgame"));
            AssignRef(so, "closeModalButton", FindBtn("closemodal"));

            // Entry Fee
            AssignRef(so, "selectedFeeText", FindText("selectedfeetext"));

            // Custom Entry
            AssignRef(so, "customAmountInput", FindComp<TMP_InputField>("custominputfield"));
            AssignRef(so, "earningsText", FindText("potentialearningstext"));
            AssignRef(so, "minMaxText", FindText("maxlabel"));

            // Online Players
            AssignRef(so, "onlinePlayersText", FindText("onlineplayerstext"));
            AssignRef(so, "onlineIndicator", FindImg("greendot"));

            // Action Button
            AssignRef(so, "findOpponentButton", FindBtn("findopponentbutton"));
            AssignRef(so, "findOpponentText", FindText("findopponenttext"));

            // Rounds Selection
            AssignRef(so, "rounds1Button", FindBtn("rounds1button"));
            AssignRef(so, "rounds3Button", FindBtn("rounds3button"));
            AssignRef(so, "rounds5Button", FindBtn("rounds5button"));

            // Cognitive Sprint
            AssignRef(so, "cognitiveSprintButton", FindBtn("gamecard_cognitivesprint"));
            AssignGO(so, "cognitiveSprintPanel", "CognitiveSprintPanel");
            AssignRef(so, "sprintSelectionText", FindText("sprintselectiontext"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(manager.gameObject);
            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);

            Debug.Log($"[CashBattle1v1] Referencias: {assignedCount} asignadas, {alreadySetCount} ya puestas, {failedCount} fallidas");
        }

        private static void AssignRef(SerializedObject so, string prop, Object value)
        {
            var p = so.FindProperty(prop);
            if (p == null) { AddAR(prop, "Propiedad no existe", false, null); failedCount++; return; }
            if (p.objectReferenceValue != null) { AddAR(prop, "Ya asignada", true, p.objectReferenceValue); alreadySetCount++; return; }
            if (value != null) { p.objectReferenceValue = value; AddAR(prop, "Asignada", true, value); assignedCount++; }
            else { AddAR(prop, "No encontrada", false, null); failedCount++; }
        }

        private static void AssignGO(SerializedObject so, string prop, params string[] patterns)
        {
            var p = so.FindProperty(prop);
            if (p == null) { AddAR(prop, "Propiedad no existe", false, null); failedCount++; return; }
            if (p.objectReferenceValue != null) { AddAR(prop, "Ya asignada", true, p.objectReferenceValue); alreadySetCount++; return; }
            var all = Object.FindObjectsOfType<Transform>(true);
            foreach (var pat in patterns)
                foreach (var t in all)
                    if (t.gameObject.name.ToLower().Contains(pat.ToLower()))
                    {
                        p.objectReferenceValue = t.gameObject;
                        AddAR(prop, "Asignada", true, t.gameObject);
                        assignedCount++;
                        return;
                    }
            AddAR(prop, "No encontrada", false, null); failedCount++;
        }

        private static T FindComp<T>(params string[] patterns) where T : Component
        {
            var all = Object.FindObjectsOfType<T>(true);
            foreach (var pat in patterns) foreach (var o in all) if (o.gameObject.name.ToLower().Contains(pat.ToLower())) return o;
            return null;
        }

        private static TextMeshProUGUI FindText(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<TextMeshProUGUI>(true);
            foreach (var p in patterns) foreach (var t in all) if (t.gameObject.name.ToLower().Contains(p.ToLower())) return t;
            return null;
        }

        private static Button FindBtn(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<Button>(true);
            foreach (var p in patterns) foreach (var b in all) if (b.gameObject.name.ToLower().Contains(p.ToLower())) return b;
            return null;
        }

        private static Image FindImg(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<Image>(true);
            foreach (var p in patterns) foreach (var i in all) if (i.gameObject.name.ToLower().Contains(p.ToLower())) return i;
            return null;
        }

        private static void AddAR(string f, string s, bool ok, Object o)
        {
            assignResults.Add(new AssignResult { fieldName = f, status = s, success = ok, assignedObject = o });
        }

        private void DrawAssignResults()
        {
            if (assignResults.Count == 0) return;

            EditorGUILayout.Space(10);
            int total = assignResults.Count;
            int successTotal = assignedCount + alreadySetCount;
            float rate = (float)successTotal / total;

            GUI.color = rate == 1f ? new Color(0.2f, 0.8f, 0.2f) :
                        rate >= 0.7f ? new Color(1f, 0.8f, 0.2f) : new Color(1f, 0.4f, 0.4f);
            GUILayout.Label(rate == 1f ? "TODAS LAS REFERENCIAS ASIGNADAS" : "Algunas referencias faltan", EditorStyles.boldLabel);
            GUI.color = Color.white;

            GUILayout.Label($"Asignadas: {assignedCount} | Ya puestas: {alreadySetCount} | Fallidas: {failedCount}");
            EditorGUILayout.Space(5);

            foreach (var r in assignResults)
            {
                EditorGUILayout.BeginHorizontal();
                GUI.color = r.success ? (r.status == "Ya asignada" ? new Color(0.5f, 0.8f, 1f) : Color.green) : Color.red;
                GUILayout.Label(r.success ? (r.status == "Ya asignada" ? "o" : "+") : "x", GUILayout.Width(16));
                GUI.color = Color.white;
                GUILayout.Label(r.fieldName, GUILayout.Width(180));
                GUILayout.Label(r.status, GUILayout.Width(110));
                if (r.assignedObject != null)
                    EditorGUILayout.ObjectField(r.assignedObject, typeof(Object), true, GUILayout.Width(140));
                EditorGUILayout.EndHorizontal();
            }
        }

        #endregion
    }
}
