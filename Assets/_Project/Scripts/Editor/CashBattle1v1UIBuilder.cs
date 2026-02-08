using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// UI Builder para la escena CashBattle1v1
    /// Construye la interfaz de selección de juegos y apuestas para batallas 1v1
    /// </summary>
    public class CashBattle1v1UIBuilder : EditorWindow
    {
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
            GUILayout.Label("CashBattle 1v1 UI Builder", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Construye la UI para la escena CashBattle1v1:\n\n" +
                "- Header con balance y jugadores online\n" +
                "- Grid de seleccion de juegos (iconos premium)\n" +
                "- Selector de apuestas ($1-$250)\n" +
                "- Input personalizado con max $250\n" +
                "- Feedback de ganancias potenciales\n" +
                "- Boton 'Buscar Rival' con contador",
                MessageType.Info);

            EditorGUILayout.Space(10);

            GUI.backgroundColor = GOLD_PRIMARY;
            if (GUILayout.Button("CONSTRUIR UI COMPLETA", GUILayout.Height(40)))
            {
                BuildCashBattle1v1UI();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(15);
            GUILayout.Label("Construccion por Secciones:", EditorStyles.boldLabel);

            if (GUILayout.Button("Solo Header + Online Indicator", GUILayout.Height(28)))
            {
                BuildHeaderOnly();
            }

            if (GUILayout.Button("Solo Game Cards Grid", GUILayout.Height(28)))
            {
                BuildGameCardsOnly();
            }

            if (GUILayout.Button("Solo Entry Fee Section", GUILayout.Height(28)))
            {
                BuildEntryFeeSectionOnly();
            }

            if (GUILayout.Button("Solo Find Opponent Button", GUILayout.Height(28)))
            {
                BuildFindOpponentOnly();
            }
        }

        #region Main Build Methods

        private static void BuildCashBattle1v1UI()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
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

            Debug.Log("[CashBattle1v1Builder] UI construida exitosamente!");
        }

        private static void CleanupOldElements(Transform parent)
        {
            string[] toDestroy = {
                "Background", "SafeArea", "MainContentPanel", "Header",
                "GameSelectionPanel", "EntryFeeSection", "FindOpponentContainer"
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

        private static void CreateBackButton(Transform parent)
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
            title.fontSize = 48;
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
            rt.sizeDelta = new Vector2(180, 50);
            rt.anchoredPosition = new Vector2(-20, 0);

            Image bg = balanceWidget.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.08f, 0.05f, 0.8f);
            // Sin Outline - widget usa color de fondo para distinguirse

            // Dollar sign
            GameObject dollarObj = new GameObject("DollarSign");
            dollarObj.transform.SetParent(balanceWidget.transform, false);

            RectTransform dollarRT = dollarObj.AddComponent<RectTransform>();
            dollarRT.anchorMin = new Vector2(0, 0);
            dollarRT.anchorMax = new Vector2(0, 1);
            dollarRT.sizeDelta = new Vector2(40, 0);
            dollarRT.anchoredPosition = new Vector2(8, 0);

            TextMeshProUGUI dollarText = dollarObj.AddComponent<TextMeshProUGUI>();
            dollarText.text = "$";
            dollarText.fontSize = 28;
            dollarText.color = TEXT_GOLD;
            dollarText.alignment = TextAlignmentOptions.Center;
            dollarText.fontStyle = FontStyles.Bold;

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
            balanceText.fontSize = 26;
            balanceText.color = TEXT_PRIMARY;
            balanceText.alignment = TextAlignmentOptions.Left;
            balanceText.fontStyle = FontStyles.Bold;
        }

        private static void BuildHeaderOnly()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
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

            // Online Indicator
            CreateOnlineIndicator(panel.transform);

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
            titleText.fontSize = 36;
            titleText.color = TEXT_GOLD;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
        }

        private static void CreateOnlineIndicator(Transform parent)
        {
            GameObject indicator = new GameObject("OnlineIndicator");
            indicator.transform.SetParent(parent, false);

            RectTransform rt = indicator.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(0, 30);
            rt.anchoredPosition = new Vector2(0, -48);

            Image bg = indicator.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.08f, 0.12f, 0.8f);

            // Green dot
            GameObject dot = new GameObject("GreenDot");
            dot.transform.SetParent(indicator.transform, false);

            RectTransform dotRT = dot.AddComponent<RectTransform>();
            dotRT.anchorMin = new Vector2(0.5f, 0.5f);
            dotRT.anchorMax = new Vector2(0.5f, 0.5f);
            dotRT.sizeDelta = new Vector2(10, 10);
            dotRT.anchoredPosition = new Vector2(-140, 0);

            Image dotImg = dot.AddComponent<Image>();
            dotImg.color = new Color(0.2f, 1f, 0.4f, 1f);
            // Sin Outline - elemento pequeño no necesita glow adicional

            // Text
            GameObject textObj = new GameObject("OnlineText");
            textObj.transform.SetParent(indicator.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0.5f, 0);
            textRT.anchorMax = new Vector2(0.5f, 1);
            textRT.sizeDelta = new Vector2(300, 0);
            textRT.anchoredPosition = new Vector2(15, 0);

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "47 jugadores online | Pool: $2,340";
            text.fontSize = 18;
            text.color = TEXT_SECONDARY;
            text.alignment = TextAlignmentOptions.Left;
        }

        #endregion

        #region Games Grid

        private static void CreateGamesGrid(Transform parent)
        {
            // ========== VIEWPORT/SCROLL AREA ==========
            GameObject scrollView = new GameObject("GamesScrollView");
            scrollView.transform.SetParent(parent, false);

            RectTransform scrollRT = scrollView.AddComponent<RectTransform>();
            // Ocupa desde debajo del online indicator hasta arriba del entry fee section
            scrollRT.anchorMin = new Vector2(0, 0.38f); // Mas espacio para entry fee
            scrollRT.anchorMax = new Vector2(1, 0.95f);
            scrollRT.sizeDelta = Vector2.zero;
            scrollRT.offsetMin = new Vector2(5, 8); // Padding
            scrollRT.offsetMax = new Vector2(-5, -75); // Espacio para titulo + online indicator

            ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.1f;
            scrollRect.scrollSensitivity = 30f;

            // Viewport (con mask)
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);

            RectTransform viewportRT = viewport.AddComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.sizeDelta = Vector2.zero;
            viewportRT.pivot = new Vector2(0.5f, 1f);

            Image viewportImg = viewport.AddComponent<Image>();
            viewportImg.color = new Color(1, 1, 1, 0.01f);

            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            scrollRect.viewport = viewportRT;

            // ========== GAMES CONTAINER (Content) ==========
            GameObject gamesContainer = new GameObject("GamesContainer");
            gamesContainer.transform.SetParent(viewport.transform, false);

            RectTransform gamesRT = gamesContainer.AddComponent<RectTransform>();
            gamesRT.anchorMin = new Vector2(0, 1);
            gamesRT.anchorMax = new Vector2(1, 1);
            gamesRT.pivot = new Vector2(0.5f, 1);
            gamesRT.anchoredPosition = Vector2.zero;
            // 3 filas x (290 + 40 spacing) + padding = ~1050px
            gamesRT.sizeDelta = new Vector2(0, 1100);

            ContentSizeFitter sizeFitter = gamesContainer.AddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = gamesRT;

            // Grid Layout - Cards optimizados para que quepan 6 con separacion visible
            GridLayoutGroup gridLayout = gamesContainer.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(290, 290); // Tamaño que permite ver separacion
            gridLayout.spacing = new Vector2(40, 40); // MUCHO MAS ESPACIADO para separacion visible
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.UpperCenter;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 2;
            gridLayout.padding = new RectOffset(55, 55, 20, 25); // Mas padding

            // ========== CREATE GAME CARDS (6 juegos) ==========
            CreateGameCard(gamesContainer.transform, "DigitRush", "DigitRushIcon");
            CreateGameCard(gamesContainer.transform, "MemoryPairs", "MemoryPairsIcon");
            CreateGameCard(gamesContainer.transform, "QuickMath", "QuickMathIcon");
            CreateGameCard(gamesContainer.transform, "FlashTap", "FlashTapIcon");
            CreateGameCard(gamesContainer.transform, "OddOneOut", "OddOneOutIcon");
            CreateGameCard(gamesContainer.transform, "CognitiveSprint", "CognitiveSprintIcon", true); // PRO badge

            Debug.Log("[CashBattle1v1] Games Grid creado con 6 cards");
        }

        private static void CreateGameCard(Transform parent, string gameId, string iconName, bool isPro = false)
        {
            GameObject card = new GameObject($"GameCard_{gameId}");
            card.transform.SetParent(parent, false);

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = Color.white;
            cardBg.preserveAspect = true;

            // Cargar icono desde la carpeta CashBattle (iconos con glow pre-renderizado)
            // Esto elimina la necesidad de multiples Outline components y mejora el rendimiento
            string iconPath = $"Assets/_Project/Art/Icons/Games/CashBattle/{iconName}.png";
            Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            if (iconSprite != null)
            {
                cardBg.sprite = iconSprite;
            }
            else
            {
                // Fallback a la carpeta original si no existe en CashBattle
                string fallbackPath = $"Assets/_Project/Art/Icons/Games/{iconName}.png";
                iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(fallbackPath);
                if (iconSprite != null)
                {
                    cardBg.sprite = iconSprite;
                    Debug.LogWarning($"[CashBattle1v1] Using fallback icon (no glow): {fallbackPath}");
                }
                else
                {
                    cardBg.color = CARD_BG;
                    Debug.LogWarning($"[CashBattle1v1] Icon not found: {iconPath}");
                }
            }

            // SIN OUTLINE COMPONENTS - El glow dorado ya esta pre-renderizado en los iconos
            // Esto mejora drasticamente el rendimiento del editor y del juego

            // Button con transiciones suaves
            Button btn = card.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            colors.selectedColor = new Color(1.05f, 1.05f, 1.05f, 1f);
            btn.colors = colors;
            btn.targetGraphic = cardBg;

            // Checkmark para indicar seleccion (oculto por defecto)
            GameObject checkmark = new GameObject("Checkmark");
            checkmark.transform.SetParent(card.transform, false);

            RectTransform checkRT = checkmark.AddComponent<RectTransform>();
            checkRT.anchorMin = new Vector2(1, 1);
            checkRT.anchorMax = new Vector2(1, 1);
            checkRT.pivot = new Vector2(1, 1);
            checkRT.sizeDelta = new Vector2(50, 50);
            checkRT.anchoredPosition = new Vector2(-8, -8);

            Image checkBg = checkmark.AddComponent<Image>();
            checkBg.color = new Color(0.2f, 0.95f, 0.4f, 1f); // Verde brillante

            GameObject checkText = new GameObject("CheckText");
            checkText.transform.SetParent(checkmark.transform, false);

            RectTransform checkTextRT = checkText.AddComponent<RectTransform>();
            checkTextRT.anchorMin = Vector2.zero;
            checkTextRT.anchorMax = Vector2.one;
            checkTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI checkTMP = checkText.AddComponent<TextMeshProUGUI>();
            checkTMP.text = "V";
            checkTMP.fontSize = 32;
            checkTMP.color = Color.white;
            checkTMP.alignment = TextAlignmentOptions.Center;
            checkTMP.fontStyle = FontStyles.Bold;
            checkTMP.raycastTarget = false;

            checkmark.SetActive(false);
        }

        private static void BuildGameCardsOnly()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            Transform panel = canvas.transform.Find("SafeArea/MainContentPanel");
            if (panel == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontro MainContentPanel. Construye la UI completa primero.", "OK");
                return;
            }

            Transform oldGrid = panel.Find("GamesScrollView");
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
            titleText.fontSize = 28;
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
            text.fontSize = 28;
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
            text.fontSize = 22;
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
            dollarText.fontSize = 32;
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
            inputText.fontSize = 28;
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
            placeholderText.fontSize = 24;
            placeholderText.color = TEXT_SECONDARY;
            placeholderText.fontStyle = FontStyles.Italic;
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
            maxText.fontSize = 22;
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
            applyText.fontSize = 22;
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
            earningsText.fontSize = 26; // MAS GRANDE
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
            poolText.fontSize = 20; // MAS GRANDE
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
            coinText.fontSize = 32;
            coinText.color = BG_DARK;
            coinText.fontStyle = FontStyles.Bold;
            coinText.alignment = TextAlignmentOptions.Center;
        }

        private static void BuildEntryFeeSectionOnly()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
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
            textRT.anchorMin = new Vector2(0, 0.3f);
            textRT.anchorMax = new Vector2(1, 1);
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "BUSCAR RIVAL";
            text.fontSize = 38; // Mas grande
            text.color = BG_DARK;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;

            // Subtitle con dot verde integrado
            GameObject subtitleObj = new GameObject("OnlinePlayersText");
            subtitleObj.transform.SetParent(btnObj.transform, false);

            RectTransform subtitleRT = subtitleObj.AddComponent<RectTransform>();
            subtitleRT.anchorMin = new Vector2(0, 0);
            subtitleRT.anchorMax = new Vector2(1, 0.38f);
            subtitleRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI subtitleText = subtitleObj.AddComponent<TextMeshProUGUI>();
            subtitleText.text = "<color=#33FF55>●</color> 12 jugadores buscando";
            subtitleText.fontSize = 20;
            subtitleText.color = new Color(0.25f, 0.2f, 0.1f, 1f);
            subtitleText.fontStyle = FontStyles.Bold;
            subtitleText.alignment = TextAlignmentOptions.Center;
            subtitleText.richText = true;

            // Sin decoradores - diseño limpio
        }

        private static void BuildFindOpponentOnly()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
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
    }
}
