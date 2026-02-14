using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;

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

        private static readonly Color BG_DARK = new Color(0.08f, 0.06f, 0.12f, 1f);
        private static readonly Color CARD_BG = new Color(0.12f, 0.1f, 0.15f, 0.95f);
        private static readonly Color CARD_BORDER = new Color(0.85f, 0.65f, 0.13f, 0.6f);

        private static readonly Color TEXT_PRIMARY = new Color(1f, 1f, 1f, 1f);
        private static readonly Color TEXT_GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.7f, 0.7f, 0.7f, 1f);

        private static readonly Color BUTTON_GOLD = new Color(0.85f, 0.65f, 0.13f, 1f);
        private static readonly Color CYAN_ACCENT = new Color(0f, 0.9f, 1f, 1f);

        [MenuItem("DigitPark/UI Builders/CashBattle/CashBattle 1v1 (Game Selection)", false, 251)]
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
            if (GUILayout.Button("Solo Game Cards Grid", GUILayout.Height(26)))
                BuildGameCardsOnly();
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
            CleanupOldUI();

            Canvas canvas = FindMainCanvas();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontro Canvas. Abre la escena CashBattle1v1 primero.", "OK");
                return;
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

        private static void CleanupOldUI()
        {
            string[] toClean = { "Background", "SafeArea" };
            foreach (var canvas in Object.FindObjectsOfType<Canvas>(true))
            {
                if (canvas.transform.parent != null) continue;
                foreach (string name in toClean)
                {
                    Transform t = canvas.transform.Find(name);
                    if (t != null) Object.DestroyImmediate(t.gameObject);
                }
            }
        }

        private static Canvas FindMainCanvas()
        {
            // Buscar Canvas principal por nombre, evitando TransitionCanvas
            foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (c.gameObject.name == "Canvas")
                    return c;
            }
            // Fallback: primer Canvas que no sea TransitionCanvas
            foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (c.gameObject.name != "TransitionCanvas")
                    return c;
            }
            return UIBuilderCanvasHelper.FindMainCanvas();
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

            Debug.Log("[CashBattle1v1Builder] UI construida exitosamente!");
        }

        private static void CleanupOldElements(Transform parent)
        {
            string[] toDestroy = {
                "Background", "SafeArea", "MainContentPanel", "Header",
                "GameSelectionPanel", "EntryFeeSection", "FindOpponentContainer",
                "CognitiveSprintPanel", "GamesScrollView", "GamesContainer"
            };

            foreach (string name in toDestroy)
            {
                Transform existing = parent.Find(name);
                if (existing != null)
                {
                    DestroyImmediate(existing.gameObject);
                }
            }
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

            // Gold gradient overlay at top
            GameObject goldGlow = new GameObject("GoldGlow");
            goldGlow.transform.SetParent(bgContainer.transform, false);

            RectTransform glowRT = goldGlow.AddComponent<RectTransform>();
            glowRT.anchorMin = new Vector2(0, 0.7f);
            glowRT.anchorMax = Vector2.one;
            glowRT.sizeDelta = Vector2.zero;

            Image glowImg = goldGlow.AddComponent<Image>();
            glowImg.color = new Color(1f, 0.8f, 0.3f, 0.06f);
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
            headerRT.anchoredPosition = Vector2.zero;

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
            }
            else
            {
                GameObject backBtn = new GameObject("BackButton");
                backBtn.transform.SetParent(parent, false);

                RectTransform rt = backBtn.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 0.5f);
                rt.anchorMax = new Vector2(0, 0.5f);
                rt.pivot = new Vector2(0, 0.5f);
                rt.sizeDelta = new Vector2(100, 80);
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

                TextMeshProUGUI arrow = arrowObj.AddComponent<TextMeshProUGUI>();
                arrow.text = "<";
                arrow.fontSize = 48;
                arrow.color = TEXT_GOLD;
                arrow.alignment = TextAlignmentOptions.Center;
                arrow.fontStyle = FontStyles.Bold;

                Debug.LogWarning("[CashBattle1v1] BackButtonGold prefab not found, using fallback");
            }
        }

        private static void CreateHeaderTitle(Transform parent)
        {
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(parent, false);

            RectTransform rt = titleObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(500, 80);
            rt.anchoredPosition = Vector2.zero;

            TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
            title.text = "Batallas 1v1";
            title.fontSize = 78;
            title.color = TEXT_GOLD;
            title.alignment = TextAlignmentOptions.Center;
            title.fontStyle = FontStyles.Bold;
        }

        private static void CreateBalanceWidget(Transform parent)
        {
            GameObject balanceWidget = new GameObject("BalanceWidget");
            balanceWidget.transform.SetParent(parent, false);

            RectTransform rt = balanceWidget.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(1, 0.5f);
            rt.sizeDelta = new Vector2(180, 65);
            rt.anchoredPosition = new Vector2(-20, 0);

            Image bg = balanceWidget.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.08f, 0.05f, 0.8f);

            Outline outline = balanceWidget.AddComponent<Outline>();
            outline.effectColor = CARD_BORDER;
            outline.effectDistance = new Vector2(1, -1);

            // Coin icon
            GameObject coinIcon = new GameObject("CoinIcon");
            coinIcon.transform.SetParent(balanceWidget.transform, false);

            RectTransform coinRT = coinIcon.AddComponent<RectTransform>();
            coinRT.anchorMin = new Vector2(0, 0);
            coinRT.anchorMax = new Vector2(0, 1);
            coinRT.pivot = new Vector2(0, 0.5f);
            coinRT.sizeDelta = new Vector2(40, 0);
            coinRT.anchoredPosition = new Vector2(8, 0);

            TextMeshProUGUI coinText = coinIcon.AddComponent<TextMeshProUGUI>();
            coinText.text = "$";
            coinText.fontSize = 52;
            coinText.color = TEXT_GOLD;
            coinText.alignment = TextAlignmentOptions.Center;
            coinText.fontStyle = FontStyles.Bold;

            // Balance text
            GameObject balanceObj = new GameObject("BalanceText");
            balanceObj.transform.SetParent(balanceWidget.transform, false);

            RectTransform balanceRT = balanceObj.AddComponent<RectTransform>();
            balanceRT.anchorMin = new Vector2(0, 0);
            balanceRT.anchorMax = new Vector2(1, 1);
            balanceRT.sizeDelta = Vector2.zero;
            balanceRT.offsetMin = new Vector2(45, 0);
            balanceRT.offsetMax = new Vector2(-10, 0);

            TextMeshProUGUI balanceText = balanceObj.AddComponent<TextMeshProUGUI>();
            balanceText.text = "0.00";
            balanceText.fontSize = 52;
            balanceText.color = TEXT_PRIMARY;
            balanceText.alignment = TextAlignmentOptions.Left;
            balanceText.fontStyle = FontStyles.Bold;
        }

        private static void BuildHeaderOnly()
        {
            Canvas canvas = FindMainCanvas();
            if (canvas == null) return;

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
            rt.offsetMin = new Vector2(20, 25); // Mas espacio abajo
            rt.offsetMax = new Vector2(-20, -125);

            // Title
            CreatePanelTitle(panel.transform);

            // Games Grid
            CreateGamesGrid(panel.transform);

            // Separador visual entre games y entry fee
            CreateSectionSeparator(panel.transform, 0.36f);

            // Entry Fee Section
            CreateEntryFeeSection(panel.transform);

            // Separador visual entre entry fee y boton
            CreateSectionSeparator(panel.transform, 0.10f);

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
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(parent, false);

            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 1);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0.5f, 1);
            titleRT.sizeDelta = new Vector2(0, 45);
            titleRT.anchoredPosition = Vector2.zero;

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Selecciona un Juego";
            titleText.fontSize = 42;
            titleText.color = TEXT_GOLD;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
        }

        #endregion

        #region Games Grid

        private static void CreateGamesGrid(Transform parent)
        {
            // ========== GAMES CONTAINER (directo, sin scroll) ==========
            GameObject gamesContainer = new GameObject("GamesContainer");
            gamesContainer.transform.SetParent(parent, false);

            RectTransform gamesRT = gamesContainer.AddComponent<RectTransform>();
            gamesRT.anchorMin = new Vector2(0, 0.38f);
            gamesRT.anchorMax = new Vector2(1, 0.95f);
            gamesRT.sizeDelta = Vector2.zero;
            gamesRT.offsetMin = new Vector2(0, 8);
            gamesRT.offsetMax = new Vector2(0, -75);

            // Grid Layout - Grande y centrado, cards que abarquen la pantalla
            GridLayoutGroup gridLayout = gamesContainer.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(310, 310);
            gridLayout.spacing = new Vector2(25, 25);
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.UpperCenter;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 2;
            gridLayout.padding = new RectOffset(10, 10, 10, 15);

            // ========== CREATE GAME CARDS (6 juegos) ==========
            CreateGameCard(gamesContainer.transform, "DigitRush", "DigitRushIcon");
            CreateGameCard(gamesContainer.transform, "MemoryPairs", "MemoryPairsIcon");
            CreateGameCard(gamesContainer.transform, "QuickMath", "QuickMathIcon");
            CreateGameCard(gamesContainer.transform, "FlashTap", "FlashTapIcon");
            CreateGameCard(gamesContainer.transform, "OddOneOut", "OddOneOutIcon");
            CreateGameCard(gamesContainer.transform, "CognitiveSprint", "CognitiveSprintIcon", true);

            Debug.Log("[CashBattle1v1] Games Grid creado con 6 cards (centrado)");
        }

        private static void CreateGameCard(Transform parent, string gameId, string iconName, bool isPro = false)
        {
            GameObject card = new GameObject($"GameCard_{gameId}");
            card.transform.SetParent(parent, false);

            // Card container con fondo oscuro premium
            Image cardBg = card.AddComponent<Image>();
            cardBg.color = CARD_BG;

            // Borde dorado sutil
            Outline cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = CARD_BORDER;
            cardOutline.effectDistance = new Vector2(2, -2);

            // Icono del juego (llena toda la card)
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(card.transform, false);

            RectTransform iconRT = iconObj.AddComponent<RectTransform>();
            iconRT.anchorMin = Vector2.zero;
            iconRT.anchorMax = Vector2.one;
            iconRT.sizeDelta = Vector2.zero;

            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            // Cargar icono desde Assets/_Project/Art/Icons/Games/
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
                Debug.LogWarning($"[CashBattle1v1] Icon not found: {iconPath}");
            }

            // Button con transiciones suaves
            Button btn = card.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = CARD_BG;
            colors.highlightedColor = new Color(0.18f, 0.15f, 0.22f, 1f);
            colors.pressedColor = new Color(0.25f, 0.2f, 0.12f, 1f);
            colors.selectedColor = new Color(0.2f, 0.17f, 0.1f, 1f);
            btn.colors = colors;
            btn.targetGraphic = cardBg;

            // PRO badge (solo CognitiveSprint)
            if (isPro)
            {
                GameObject proBadge = new GameObject("ProBadge");
                proBadge.transform.SetParent(card.transform, false);

                RectTransform proRT = proBadge.AddComponent<RectTransform>();
                proRT.anchorMin = new Vector2(0, 1);
                proRT.anchorMax = new Vector2(0, 1);
                proRT.pivot = new Vector2(0, 1);
                proRT.sizeDelta = new Vector2(60, 28);
                proRT.anchoredPosition = new Vector2(8, -8);

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
                proText.fontSize = 16;
                proText.color = Color.white;
                proText.fontStyle = FontStyles.Bold;
                proText.alignment = TextAlignmentOptions.Center;
                proText.raycastTarget = false;
            }

            // Checkmark para seleccion (oculto por defecto)
            GameObject checkmark = new GameObject("Checkmark");
            checkmark.transform.SetParent(card.transform, false);

            RectTransform checkRT = checkmark.AddComponent<RectTransform>();
            checkRT.anchorMin = new Vector2(1, 1);
            checkRT.anchorMax = new Vector2(1, 1);
            checkRT.pivot = new Vector2(1, 1);
            checkRT.sizeDelta = new Vector2(45, 45);
            checkRT.anchoredPosition = new Vector2(-8, -8);

            Image checkBg = checkmark.AddComponent<Image>();
            checkBg.color = new Color(0.2f, 0.95f, 0.4f, 1f);

            GameObject checkText = new GameObject("CheckText");
            checkText.transform.SetParent(checkmark.transform, false);

            RectTransform checkTextRT = checkText.AddComponent<RectTransform>();
            checkTextRT.anchorMin = Vector2.zero;
            checkTextRT.anchorMax = Vector2.one;
            checkTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI checkTMP = checkText.AddComponent<TextMeshProUGUI>();
            checkTMP.text = "V";
            checkTMP.fontSize = 28;
            checkTMP.color = Color.white;
            checkTMP.alignment = TextAlignmentOptions.Center;
            checkTMP.fontStyle = FontStyles.Bold;
            checkTMP.raycastTarget = false;

            checkmark.SetActive(false);
        }


        private static void BuildGameCardsOnly()
        {
            Canvas canvas = FindMainCanvas();
            if (canvas == null) return;

            Transform panel = canvas.transform.Find("SafeArea/MainContentPanel");
            if (panel == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontro MainContentPanel. Construye la UI completa primero.", "OK");
                return;
            }

            Transform oldScroll = panel.Find("GamesScrollView");
            if (oldScroll != null) DestroyImmediate(oldScroll.gameObject);
            Transform oldGrid = panel.Find("GamesContainer");
            if (oldGrid != null) DestroyImmediate(oldGrid.gameObject);

            CreateGamesGrid(panel);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        #endregion

        #region Entry Fee Section

        private static void CreateEntryFeeSection(Transform parent)
        {
            GameObject feeSection = new GameObject("EntryFeeSection");
            feeSection.transform.SetParent(parent, false);

            RectTransform rt = feeSection.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.11f); // Mas espacio para el boton
            rt.anchorMax = new Vector2(1, 0.35f); // Mas espacio arriba para separacion
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

            // Earnings feedback
            CreateEarningsFeedback(feeSection.transform);
        }

        private static void CreateFeeTitle(Transform parent)
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(parent, false);

            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0.82f);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.sizeDelta = Vector2.zero;
            titleRT.offsetMin = new Vector2(15, 0);
            titleRT.offsetMax = new Vector2(-15, -5);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Elige tu apuesta";
            titleText.fontSize = 52;
            titleText.color = TEXT_GOLD;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Left;
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
            text.fontSize = 52;
            text.color = TEXT_PRIMARY;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Right;
        }

        private static void CreatePresetButtons(Transform parent)
        {
            GameObject container = new GameObject("PresetsContainer");
            container.transform.SetParent(parent, false);

            RectTransform containerRT = container.AddComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0, 0.52f);
            containerRT.anchorMax = new Vector2(1, 0.8f);
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

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = $"${amount}";
            text.fontSize = 36;
            text.color = TEXT_PRIMARY;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;

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
            containerRT.anchorMin = new Vector2(0, 0.28f);
            containerRT.anchorMax = new Vector2(1, 0.5f);
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
            dollarText.fontSize = 42;
            dollarText.color = GOLD_PRIMARY;
            dollarText.fontStyle = FontStyles.Bold;
            dollarText.alignment = TextAlignmentOptions.Center;

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
            GameObject inputTextArea = new GameObject("Text");
            inputTextArea.transform.SetParent(inputBg.transform, false);

            RectTransform inputTextRT = inputTextArea.AddComponent<RectTransform>();
            inputTextRT.anchorMin = Vector2.zero;
            inputTextRT.anchorMax = Vector2.one;
            inputTextRT.sizeDelta = Vector2.zero;
            inputTextRT.offsetMin = new Vector2(10, 0);
            inputTextRT.offsetMax = new Vector2(-10, 0);

            TextMeshProUGUI inputText = inputTextArea.AddComponent<TextMeshProUGUI>();
            inputText.text = "";
            inputText.fontSize = 48;
            inputText.color = TEXT_PRIMARY;
            inputText.fontStyle = FontStyles.Bold;
            inputText.alignment = TextAlignmentOptions.Left;

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
            placeholderText.text = "Otro monto...";
            placeholderText.fontSize = 48;
            placeholderText.color = TEXT_SECONDARY;
            placeholderText.fontStyle = FontStyles.Bold;
            placeholderText.alignment = TextAlignmentOptions.Left;

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
            maxText.fontSize = 38;
            maxText.color = TEXT_SECONDARY;
            maxText.fontStyle = FontStyles.Bold;
            maxText.alignment = TextAlignmentOptions.Center;

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

            GameObject applyTextObj = new GameObject("Text");
            applyTextObj.transform.SetParent(applyBtn.transform, false);

            RectTransform applyTextRT = applyTextObj.AddComponent<RectTransform>();
            applyTextRT.anchorMin = Vector2.zero;
            applyTextRT.anchorMax = Vector2.one;
            applyTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI applyText = applyTextObj.AddComponent<TextMeshProUGUI>();
            applyText.text = "OK";
            applyText.fontSize = 42;
            applyText.color = BG_DARK;
            applyText.fontStyle = FontStyles.Bold;
            applyText.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateEarningsFeedback(Transform parent)
        {
            GameObject container = new GameObject("EarningsFeedback");
            container.transform.SetParent(parent, false);

            RectTransform containerRT = container.AddComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0, 0);
            containerRT.anchorMax = new Vector2(1, 0.26f);
            containerRT.sizeDelta = Vector2.zero;
            containerRT.offsetMin = new Vector2(15, 8);
            containerRT.offsetMax = new Vector2(-15, -2);

            Image feedbackBg = container.AddComponent<Image>();
            feedbackBg.color = new Color(0.03f, 0.1f, 0.05f, 0.95f);
            // Sin Outline - el color de fondo verde oscuro es suficiente

            // Potential earnings - MAS GRANDE Y VISIBLE
            GameObject earningsObj = new GameObject("PotentialEarningsText");
            earningsObj.transform.SetParent(container.transform, false);

            RectTransform earningsRT = earningsObj.AddComponent<RectTransform>();
            earningsRT.anchorMin = new Vector2(0, 0.45f);
            earningsRT.anchorMax = new Vector2(0.85f, 1);
            earningsRT.sizeDelta = Vector2.zero;
            earningsRT.offsetMin = new Vector2(20, 0);
            earningsRT.offsetMax = new Vector2(0, -5);

            TextMeshProUGUI earningsText = earningsObj.AddComponent<TextMeshProUGUI>();
            earningsText.text = "Si ganas recibes: <color=#FFD700>$0.00</color>";
            earningsText.fontSize = 52; // MAS GRANDE
            earningsText.color = new Color(0.5f, 1f, 0.7f, 1f);
            earningsText.fontStyle = FontStyles.Bold;
            earningsText.alignment = TextAlignmentOptions.Left;
            earningsText.richText = true;

            // Pool info - MAS GRANDE
            GameObject poolObj = new GameObject("PoolInfoText");
            poolObj.transform.SetParent(container.transform, false);

            RectTransform poolRT = poolObj.AddComponent<RectTransform>();
            poolRT.anchorMin = new Vector2(0, 0);
            poolRT.anchorMax = new Vector2(1, 0.5f);
            poolRT.sizeDelta = Vector2.zero;
            poolRT.offsetMin = new Vector2(20, 5);
            poolRT.offsetMax = new Vector2(-20, 0);

            TextMeshProUGUI poolText = poolObj.AddComponent<TextMeshProUGUI>();
            poolText.text = "Pool: $0.00 | Tu apuesta: $0.00 | Fee: 30%";
            poolText.fontSize = 32; // MAS GRANDE
            poolText.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            poolText.fontStyle = FontStyles.Bold;
            poolText.alignment = TextAlignmentOptions.Left;

            // Coin icon - Reemplazado con simbolo de texto
            GameObject coinIcon = new GameObject("CoinIcon");
            coinIcon.transform.SetParent(container.transform, false);

            RectTransform coinRT = coinIcon.AddComponent<RectTransform>();
            coinRT.anchorMin = new Vector2(0.82f, 0.15f);
            coinRT.anchorMax = new Vector2(0.98f, 0.85f);
            coinRT.sizeDelta = Vector2.zero;

            // Fondo circular dorado
            Image coinBg = coinIcon.AddComponent<Image>();
            coinBg.color = new Color(0.8f, 0.65f, 0.1f, 0.9f);
            // Sin Outline - icono pequeño no necesita efecto adicional

            // Texto $ en el icono
            GameObject coinTextObj = new GameObject("CoinText");
            coinTextObj.transform.SetParent(coinIcon.transform, false);

            RectTransform coinTextRT = coinTextObj.AddComponent<RectTransform>();
            coinTextRT.anchorMin = Vector2.zero;
            coinTextRT.anchorMax = Vector2.one;
            coinTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI coinText = coinTextObj.AddComponent<TextMeshProUGUI>();
            coinText.text = "$";
            coinText.fontSize = 42;
            coinText.color = BG_DARK;
            coinText.fontStyle = FontStyles.Bold;
            coinText.alignment = TextAlignmentOptions.Center;
        }

        private static void BuildEntryFeeSectionOnly()
        {
            Canvas canvas = FindMainCanvas();
            if (canvas == null) return;

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
            text.text = "Buscar oponente";
            text.fontSize = 72; // Mas grande
            text.color = BG_DARK;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;

            // Sin decoradores - diseño limpio
        }

        private static void BuildFindOpponentOnly()
        {
            Canvas canvas = FindMainCanvas();
            if (canvas == null) return;

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

            RectTransform blockerRT = sprintPanel.AddComponent<RectTransform>();
            blockerRT.anchorMin = Vector2.zero;
            blockerRT.anchorMax = Vector2.one;
            blockerRT.sizeDelta = Vector2.zero;

            Image blockerBg = sprintPanel.AddComponent<Image>();
            blockerBg.color = new Color(0f, 0f, 0f, 0.75f); // Semi-transparente

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
                "COGNITIVE SPRINT", 40, TEXT_GOLD, FontStyles.Bold, 55);

            // === Subtitulo ===
            GameObject subtitleObj = CreateLayoutText(card.transform, "SprintSubtitle",
                "Selecciona de 2 a 5 juegos", 20, TEXT_SECONDARY, FontStyles.Bold, 30);

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
                "Seleccionados: 0/5 (min: 2)", 24, Color.yellow, FontStyles.Bold, 40);

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
            CreateSprintActionButton(buttonsRow.transform, "SprintCancelButton", "CANCELAR",
                new Color(0.15f, 0.12f, 0.2f, 1f), new Color(0.5f, 0.45f, 0.6f, 1f),
                TEXT_SECONDARY, 1f);

            // Aceptar
            CreateSprintActionButton(buttonsRow.transform, "SprintAcceptButton", "ACEPTAR",
                BUTTON_GOLD, new Color(1f, 0.75f, 0.2f, 0.6f),
                BG_DARK, 1f);

            // Visible para verificar en Editor
            // sprintPanel.SetActive(false);

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
            nameText.fontSize = 28;
            nameText.color = TEXT_PRIMARY;
            nameText.fontStyle = FontStyles.Bold;
            nameText.alignment = TextAlignmentOptions.Left;
            nameText.enableWordWrapping = false;
            nameText.overflowMode = TextOverflowModes.Ellipsis;
            nameText.raycastTarget = false;

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

            GameObject checkTextObj = new GameObject("CheckText");
            checkTextObj.transform.SetParent(checkObj.transform, false);

            RectTransform checkTextRT = checkTextObj.AddComponent<RectTransform>();
            checkTextRT.anchorMin = Vector2.zero;
            checkTextRT.anchorMax = Vector2.one;
            checkTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI checkText = checkTextObj.AddComponent<TextMeshProUGUI>();
            checkText.text = "\u2713";
            checkText.fontSize = 26;
            checkText.color = Color.white;
            checkText.fontStyle = FontStyles.Bold;
            checkText.alignment = TextAlignmentOptions.Center;
            checkText.raycastTarget = false;

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

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 26;
            text.color = textColor;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = false;
            text.overflowMode = TextOverflowModes.Ellipsis;
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

            // Games Grid
            AssignRef(so, "gamesContainer", FindComp<Transform>("gamescontainer"));

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
