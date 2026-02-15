using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;

namespace DigitPark.Editor
{
    /// <summary>
    /// UI Builder para la escena CashBattleHub (Menu Principal de Cash Battle)
    /// Construye: 4 tarjetas principales (Batallas 1v1, Torneos, Wallet, Historial)
    /// Para la escena CashBattle1v1, usar CashBattle1v1UIBuilder
    /// </summary>
    public class CashBattleUIBuilder : EditorWindow
    {
        private const string BACK_BUTTON_GOLD_PREFAB = "Assets/_Project/Prefabs/Common/BackButtonGold.prefab";

        // Premium Color Palette
        private static readonly Color GOLD_PRIMARY = new Color(1f, 0.84f, 0f, 1f);           // #FFD700 Gold
        private static readonly Color GOLD_DARK = new Color(0.85f, 0.65f, 0.13f, 1f);        // #D4A520 Dark Gold
        private static readonly Color GOLD_LIGHT = new Color(1f, 0.93f, 0.55f, 1f);          // #FFEE8C Light Gold
        private static readonly Color AMBER = new Color(1f, 0.75f, 0f, 1f);                  // #FFBF00 Amber

        private static readonly Color BG_DARK = new Color(0.08f, 0.06f, 0.12f, 1f);          // Very dark purple-black
        private static readonly Color BG_GRADIENT_TOP = new Color(0.15f, 0.1f, 0.05f, 1f);   // Dark brown/gold tint
        private static readonly Color BG_GRADIENT_BOTTOM = new Color(0.05f, 0.03f, 0.08f, 1f); // Almost black

        private static readonly Color CARD_BG = new Color(0.12f, 0.1f, 0.15f, 0.95f);        // Dark card background
        private static readonly Color CARD_BORDER = new Color(0.85f, 0.65f, 0.13f, 0.6f);    // Gold border

        private static readonly Color TEXT_PRIMARY = new Color(1f, 1f, 1f, 1f);              // White
        private static readonly Color TEXT_GOLD = new Color(1f, 0.84f, 0f, 1f);              // Gold
        private static readonly Color TEXT_SECONDARY = new Color(0.7f, 0.7f, 0.7f, 1f);      // Gray

        private static readonly Color BUTTON_GOLD = new Color(0.85f, 0.65f, 0.13f, 1f);      // Gold button
        private static readonly Color BUTTON_DANGER = new Color(0.8f, 0.2f, 0.2f, 1f);       // Red for warnings

        private static readonly Color CYAN_ACCENT = new Color(0f, 0.9f, 1f, 1f);             // Keep some cyan for contrast

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

        [MenuItem("DigitPark/UI Builders/CashBattle/CashBattleHub (Menu Principal)", false, 250)]
        public static void ShowWindow()
        {
            GetWindow<CashBattleUIBuilder>("CashBattleHub Builder");
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // === EXISTING BUILD SECTION ===
            GUILayout.Label("CashBattleHub UI Builder", EditorStyles.boldLabel);
            GUILayout.Label("Menu Principal de Cash Battle", EditorStyles.miniLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Construye la UI para CashBattleHub.unity:\n\n" +
                "- 4 tarjetas principales:\n" +
                "  * Batallas 1v1\n" +
                "  * Torneos Cash\n" +
                "  * Mi Wallet\n" +
                "  * Historial\n" +
                "- Panel de verificacion de edad\n" +
                "- Background premium dorado",
                MessageType.Info);

            EditorGUILayout.Space(10);

            GUI.backgroundColor = GOLD_PRIMARY;
            if (GUILayout.Button("BUILD PREMIUM UI", GUILayout.Height(40)))
            {
                BuildPremiumUI();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Only Rebuild Background", GUILayout.Height(25)))
            {
                RebuildBackground();
            }

            // ========== SEPARADOR ==========
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // ========== SECCION: REFERENCE ASSIGNER ==========
            GUILayout.Label("Asignar Referencias", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "CashBattleHub")
            {
                EditorGUILayout.HelpBox($"Escena actual: {currentScene}\nAbre CashBattleHub primero.", MessageType.Warning);
            }

            MonoBehaviour targetManager = FindCashBattleManager();
            if (targetManager != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Manager:", GUILayout.Width(60));
                EditorGUILayout.ObjectField(targetManager, typeof(MonoBehaviour), true);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("CashBattleManager no encontrado en escena.", MessageType.Warning);
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

            DrawAssignResults();

            EditorGUILayout.EndScrollView();
        }

        private static void BuildPremiumUI()
        {
            CleanupOldUI();

            // Find or create Canvas
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Error", "No Canvas found. Open CashBattle scene first.", "OK");
                return;
            }

            // Clean existing UI (optional - ask user)
            if (EditorUtility.DisplayDialog("Rebuild UI?",
                "This will rebuild the CashBattle UI.\n\nExisting UI elements will be replaced.\n\nContinue?",
                "Yes, Build", "Cancel"))
            {
                BuildAllElements(canvas);

                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

                EditorUtility.DisplayDialog("Success",
                    "CashBattle Premium UI created!\n\n" +
                    "Don't forget to:\n" +
                    "1. Assign references in CashBattleManager\n" +
                    "2. Save the scene",
                    "OK");
            }
        }

        private static void RebuildBackground()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            // Find and destroy old background
            Transform oldBg = canvas.transform.Find("Background");
            if (oldBg != null)
            {
                DestroyImmediate(oldBg.gameObject);
            }

            CreatePremiumBackground(canvas.transform);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private static void BuildAllElements(Canvas canvas)
        {
            Transform canvasTransform = canvas.transform;

            // Remove old elements
            CleanupOldElements(canvasTransform);

            // 1. Premium Background
            CreatePremiumBackground(canvasTransform);

            // 2. Main Container (SafeArea)
            GameObject safeArea = CreateSafeArea(canvasTransform);

            // 3. Header with Back button and Balance
            CreateHeader(safeArea.transform);

            // 4. Main Panel with Cards
            CreateMainPanel(safeArea.transform);

            // 5. Bet Confirmation Panel (overlay, hidden by default)
            CreateBetConfirmationPanel(safeArea.transform);

            // 7. Matchmaking Panel (overlay, hidden by default)
            CreateMatchmakingPanel(safeArea.transform);

            Debug.Log("[CashBattleUIBuilder] CashBattleHub UI built successfully!");
        }

        private static void CleanupOldElements(Transform parent)
        {
            // Destroy specific known elements
            string[] toDestroy = {
                "Background", "SafeArea", "MainPanel", "AgeVerificationPanel", "Header",
                "GameSelectionPanel", "TournamentListPanel", "ConfirmBetPanel", "MatchmakingPanel",
                "WalletPanel", "HistoryPanel"
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

        #region Background

        private static void CreatePremiumBackground(Transform parent)
        {
            // Main background container
            GameObject bgContainer = new GameObject("Background");
            bgContainer.transform.SetParent(parent, false);
            bgContainer.transform.SetAsFirstSibling();

            RectTransform bgRT = bgContainer.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;

            // Base dark layer
            GameObject baseLayer = new GameObject("BaseLayer");
            baseLayer.transform.SetParent(bgContainer.transform, false);

            RectTransform baseRT = baseLayer.AddComponent<RectTransform>();
            baseRT.anchorMin = Vector2.zero;
            baseRT.anchorMax = Vector2.one;
            baseRT.sizeDelta = Vector2.zero;

            Image baseImg = baseLayer.AddComponent<Image>();
            baseImg.color = BG_DARK;

            // Gradient overlay (simulated with multiple layers)
            CreateGradientLayers(bgContainer.transform);

            // Gold particle/glow effects (subtle)
            CreateGoldAccents(bgContainer.transform);

            // Vignette effect
            CreateVignette(bgContainer.transform);
        }

        private static void CreateGradientLayers(Transform parent)
        {
            // Top gradient (gold tint)
            GameObject topGradient = new GameObject("TopGradient");
            topGradient.transform.SetParent(parent, false);

            RectTransform topRT = topGradient.AddComponent<RectTransform>();
            topRT.anchorMin = new Vector2(0, 0.5f);
            topRT.anchorMax = Vector2.one;
            topRT.sizeDelta = Vector2.zero;

            Image topImg = topGradient.AddComponent<Image>();
            topImg.color = new Color(0.2f, 0.15f, 0.05f, 0.3f); // Subtle gold tint at top

            // Bottom darker area
            GameObject bottomGradient = new GameObject("BottomGradient");
            bottomGradient.transform.SetParent(parent, false);

            RectTransform bottomRT = bottomGradient.AddComponent<RectTransform>();
            bottomRT.anchorMin = Vector2.zero;
            bottomRT.anchorMax = new Vector2(1, 0.3f);
            bottomRT.sizeDelta = Vector2.zero;

            Image bottomImg = bottomGradient.AddComponent<Image>();
            bottomImg.color = new Color(0.02f, 0.01f, 0.04f, 0.5f); // Darker at bottom
        }

        private static void CreateGoldAccents(Transform parent)
        {
            // Subtle gold glow at top
            GameObject goldGlow = new GameObject("GoldGlow");
            goldGlow.transform.SetParent(parent, false);

            RectTransform glowRT = goldGlow.AddComponent<RectTransform>();
            glowRT.anchorMin = new Vector2(0.2f, 0.7f);
            glowRT.anchorMax = new Vector2(0.8f, 1f);
            glowRT.sizeDelta = Vector2.zero;

            Image glowImg = goldGlow.AddComponent<Image>();
            glowImg.color = new Color(1f, 0.8f, 0.3f, 0.08f); // Very subtle gold glow

            // Corner accents
            CreateCornerAccent(parent, "TopLeftAccent", new Vector2(0, 1), new Vector2(0.3f, 1), new Vector2(0, 0.7f), new Vector2(0.3f, 0.7f));
            CreateCornerAccent(parent, "TopRightAccent", new Vector2(0.7f, 1), new Vector2(1, 1), new Vector2(0.7f, 0.7f), new Vector2(1, 0.7f));
        }

        private static void CreateCornerAccent(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchorMin2, Vector2 anchorMax2)
        {
            GameObject accent = new GameObject(name);
            accent.transform.SetParent(parent, false);

            RectTransform rt = accent.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(anchorMin.x, anchorMin2.y);
            rt.anchorMax = new Vector2(anchorMax.x, anchorMax.y);
            rt.sizeDelta = Vector2.zero;

            Image img = accent.AddComponent<Image>();
            img.color = new Color(0.85f, 0.65f, 0.13f, 0.03f); // Very subtle gold
        }

        private static void CreateVignette(Transform parent)
        {
            GameObject vignette = new GameObject("Vignette");
            vignette.transform.SetParent(parent, false);

            RectTransform rt = vignette.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image img = vignette.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.4f);

            // Note: For a real vignette, you'd use a radial gradient sprite
            // This is just a simple overlay
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
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 0);

            // Add SafeAreaHandler if exists
            System.Type safeAreaType = System.Type.GetType("DigitPark.UI.SafeAreaHandler, Assembly-CSharp");
            if (safeAreaType != null)
            {
                safeArea.AddComponent(safeAreaType);
            }

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

            // Header background (subtle)
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
                // Fallback: manual back button creation
                GameObject backBtn = new GameObject("BackButton");
                backBtn.transform.SetParent(parent, false);

                RectTransform rt = backBtn.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 0.5f);
                rt.anchorMax = new Vector2(0, 0.5f);
                rt.pivot = new Vector2(0, 0.5f);
                rt.sizeDelta = new Vector2(100, 80);
                rt.anchoredPosition = new Vector2(20, 0);

                Image img = backBtn.AddComponent<Image>();
                img.color = Color.clear; // Transparent

                Button btn = backBtn.AddComponent<Button>();
                ColorBlock colors = btn.colors;
                colors.normalColor = Color.clear;
                colors.highlightedColor = new Color(1, 1, 1, 0.1f);
                colors.pressedColor = new Color(1, 1, 1, 0.2f);
                btn.colors = colors;

                // Arrow text
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

                Debug.LogWarning("[CashBattleHub] BackButtonGold prefab not found, using fallback");
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
            title.text = "Cash Battle";
            title.fontSize = 78; // Bigger
            title.color = TEXT_GOLD;
            title.alignment = TextAlignmentOptions.Center;
            title.fontStyle = FontStyles.Bold;

            // Gold outline effect
            title.outlineWidth = 0.2f;
            title.outlineColor = new Color(0.5f, 0.35f, 0f, 0.6f);
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

            // Background
            Image bg = balanceWidget.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.08f, 0.05f, 0.8f);

            // Gold border
            Outline outline = balanceWidget.AddComponent<Outline>();
            outline.effectColor = CARD_BORDER;
            outline.effectDistance = new Vector2(1, -1);

            // Coin icon (text emoji for now)
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

        #endregion

        #region Main Panel

        private static void CreateMainPanel(Transform parent)
        {
            GameObject panel = new GameObject("MainPanel");
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = new Vector2(20, 20);
            rt.offsetMax = new Vector2(-20, -130);

            // Main cards container
            GameObject cardsContainer = new GameObject("CardsContainer");
            cardsContainer.transform.SetParent(panel.transform, false);

            RectTransform cardsRT = cardsContainer.AddComponent<RectTransform>();
            cardsRT.anchorMin = Vector2.zero;
            cardsRT.anchorMax = Vector2.one;
            cardsRT.sizeDelta = Vector2.zero;

            // NUEVO LAYOUT:
            // - Batallas 1v1: Card GRANDE arriba (45% altura)
            // - Torneos + Wallet: Fila de 2 cards (33% altura)
            // - Historial: Card ancho completo abajo (20% altura)
            CreateBattles1v1Card(cardsContainer.transform);
            CreateTournamentsCashCard(cardsContainer.transform);
            CreateWalletCard(cardsContainer.transform);
            CreateHistoryCard(cardsContainer.transform);

            // MainPanel visible por defecto - el Manager lo ocultara si necesita verificacion
            panel.SetActive(true);
        }

        private static void CreateBattles1v1Card(Transform parent)
        {
            // CARD PRINCIPAL - Arriba, ancho completo, 45% altura
            GameObject card = CreatePremiumCard(parent, "Battles1v1Card",
                "BATALLAS 1v1",
                "Enfrenta a otros jugadores",
                "",
                new Vector2(0, 0.57f),      // anchorMin
                new Vector2(1, 1f));        // anchorMax

            AddCardIconImage(card.transform, "Battles1v1Icon");
        }

        private static void CreateTournamentsCashCard(Transform parent)
        {
            // Card izquierdo medio
            GameObject card = CreatePremiumCard(parent, "CashTournamentsCard",
                "TORNEOS",
                "Grandes premios",
                "",
                new Vector2(0, 0.22f),       // anchorMin
                new Vector2(0.49f, 0.55f));  // anchorMax

            AddCardIconImage(card.transform, "TournamentsCashIcon");
        }

        private static void CreateWalletCard(Transform parent)
        {
            // Card derecho medio
            GameObject card = CreatePremiumCard(parent, "WalletCard",
                "MI WALLET",
                "Deposita y retira",
                "",
                new Vector2(0.51f, 0.22f),   // anchorMin
                new Vector2(1, 0.55f));      // anchorMax

            AddCardIconImage(card.transform, "WalletCashIcon");
        }

        private static void CreateHistoryCard(Transform parent)
        {
            // Card inferior - ancho completo
            GameObject card = CreatePremiumCard(parent, "HistoryCard",
                "HISTORIAL",
                "Tus batallas y estadisticas",
                "",
                new Vector2(0, 0),           // anchorMin
                new Vector2(1, 0.20f));      // anchorMax

            AddCardIconImage(card.transform, "HistoryCashIcon");
        }

        /// <summary>
        /// Crea un card premium con icono de imagen
        /// </summary>
        private static GameObject CreatePremiumCard(Transform parent, string name, string title, string subtitle, string detail,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject card = new GameObject(name);
            card.transform.SetParent(parent, false);

            RectTransform rt = card.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = new Vector2(5, 5);
            rt.offsetMax = new Vector2(-5, -5);

            // Card background con gradiente sutil
            Image bg = card.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.08f, 0.12f, 0.95f);

            // Borde dorado premium
            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = new Color(0.85f, 0.65f, 0.13f, 0.7f);
            outline.effectDistance = new Vector2(2, -2);

            // Button
            Button btn = card.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.1f, 1f, 1f);
            colors.pressedColor = new Color(0.9f, 0.85f, 0.8f, 1f);
            btn.colors = colors;
            btn.targetGraphic = bg;

            // Calcular si es card grande (Batallas 1v1) o pequeño
            bool isLargeCard = (anchorMax.y - anchorMin.y) > 0.35f;
            bool isWideCard = (anchorMax.x - anchorMin.x) > 0.6f;

            // === TITULO ===
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(card.transform, false);

            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            if (isLargeCard)
            {
                // Card grande - titulo arriba a la derecha del icono (150px + margen)
                titleRT.anchorMin = new Vector2(0, 0.55f);
                titleRT.anchorMax = new Vector2(1, 0.95f);
                titleRT.offsetMin = new Vector2(175, 0);  // Ajustado para icono 150px
                titleRT.offsetMax = new Vector2(-160, -10);  // Espacio para flecha x3
            }
            else
            {
                // Card pequeño (icono 150px + margen)
                titleRT.anchorMin = new Vector2(0, 0.5f);
                titleRT.anchorMax = new Vector2(1, 1);
                titleRT.offsetMin = new Vector2(175, 5);  // Ajustado para icono 150px
                titleRT.offsetMax = new Vector2(-115, -8);  // Espacio para flecha x3
            }

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = title;
            titleText.fontSize = 68;
            titleText.color = TEXT_GOLD;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.fontStyle = FontStyles.Bold;

            // === SUBTITULO ===
            GameObject subtitleObj = new GameObject("Subtitle");
            subtitleObj.transform.SetParent(card.transform, false);

            RectTransform subRT = subtitleObj.AddComponent<RectTransform>();
            if (isLargeCard)
            {
                subRT.anchorMin = new Vector2(0, 0.30f);
                subRT.anchorMax = new Vector2(1, 0.55f);
                subRT.offsetMin = new Vector2(175, 0);   // Ajustado para icono 150px
                subRT.offsetMax = new Vector2(-160, 0);   // Espacio para flecha x3
            }
            else
            {
                subRT.anchorMin = new Vector2(0, 0);
                subRT.anchorMax = new Vector2(1, 0.5f);
                subRT.offsetMin = new Vector2(175, 8);  // Ajustado para icono 150px
                subRT.offsetMax = new Vector2(-115, -5);  // Espacio para flecha x3
            }

            TextMeshProUGUI subText = subtitleObj.AddComponent<TextMeshProUGUI>();
            subText.text = subtitle;
            subText.fontSize = 52;
            subText.color = TEXT_SECONDARY;
            subText.alignment = TextAlignmentOptions.Left;
            subText.fontStyle = FontStyles.Bold;

            // === DETALLE (rango de precio) - BADGE PROMINENTE ===
            if (!string.IsNullOrEmpty(detail))
            {
                GameObject detailObj = new GameObject("PriceBadge");
                detailObj.transform.SetParent(card.transform, false);

                RectTransform detailRT = detailObj.AddComponent<RectTransform>();
                detailRT.anchorMin = new Vector2(1, 0);
                detailRT.anchorMax = new Vector2(1, 0);
                detailRT.pivot = new Vector2(1, 0);
                detailRT.sizeDelta = isLargeCard ? new Vector2(160, 45) : new Vector2(130, 38);  // MAS GRANDE
                detailRT.anchoredPosition = new Vector2(-12, 12);

                // Fondo con gradiente cyan más visible
                Image detailBg = detailObj.AddComponent<Image>();
                detailBg.color = new Color(0f, 0.85f, 1f, 0.25f);  // Más opaco

                // Borde cyan brillante para destacar
                Outline badgeOutline = detailObj.AddComponent<Outline>();
                badgeOutline.effectColor = new Color(0f, 0.9f, 1f, 0.8f);
                badgeOutline.effectDistance = new Vector2(1.5f, -1.5f);

                // Texto en objeto hijo separado
                GameObject detailTextObj = new GameObject("DetailText");
                detailTextObj.transform.SetParent(detailObj.transform, false);

                RectTransform detailTextRT = detailTextObj.AddComponent<RectTransform>();
                detailTextRT.anchorMin = Vector2.zero;
                detailTextRT.anchorMax = Vector2.one;
                detailTextRT.sizeDelta = Vector2.zero;

                TextMeshProUGUI detailText = detailTextObj.AddComponent<TextMeshProUGUI>();
                detailText.text = detail;
                detailText.fontSize = isLargeCard ? 26 : 22;  // AUMENTADO: 22→26, 18→22
                detailText.color = CYAN_ACCENT;
                detailText.alignment = TextAlignmentOptions.Center;
                detailText.fontStyle = FontStyles.Bold;
            }

            // === FLECHA INDICADORA (affordance - muestra que es tocable) ===
            CreateTouchIndicator(card.transform, isLargeCard);

            // NOTA: Indicador de jugadores activos se muestra DENTRO de las escenas
            // CashBattle1v1 y CashTournaments, no en el Hub

            return card;
        }

        /// <summary>
        /// Crea flecha indicadora que muestra que el card es tocable (affordance)
        /// </summary>
        private static void CreateTouchIndicator(Transform cardTransform, bool isLargeCard)
        {
            GameObject arrow = new GameObject("TouchArrow");
            arrow.transform.SetParent(cardTransform, false);

            RectTransform rt = arrow.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(1, 0.5f);
            rt.sizeDelta = isLargeCard ? new Vector2(150, 150) : new Vector2(105, 105);
            rt.anchoredPosition = new Vector2(-10, 0);

            TextMeshProUGUI arrowText = arrow.AddComponent<TextMeshProUGUI>();
            arrowText.text = ">";
            arrowText.fontSize = isLargeCard ? 126 : 90;
            arrowText.color = new Color(1f, 0.84f, 0f, 0.6f);  // Dorado semi-transparente
            arrowText.alignment = TextAlignmentOptions.Center;
            arrowText.fontStyle = FontStyles.Bold;
        }

        // NOTA: CreatePlayersCountIndicator eliminado del Hub
        // El indicador de jugadores activos se muestra en CashBattle1v1 y CashTournaments

        /// <summary>
        /// Añade icono de imagen al card (carga desde Assets)
        /// </summary>
        private static void AddCardIconImage(Transform cardTransform, string iconName)
        {
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(cardTransform, false);

            RectTransform rt = iconObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0, 0.5f);

            // Todos los iconos a 150x150
            rt.sizeDelta = new Vector2(150, 150);
            rt.anchoredPosition = new Vector2(12, 0);

            // Imagen del icono
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.color = Color.white;

            // Intentar cargar el sprite desde la carpeta Hub
            string iconPath = $"Assets/_Project/Art/Icons/CashBattle/Hub/{iconName}.png";
            Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);

            if (iconSprite != null)
            {
                iconImg.sprite = iconSprite;
            }
            else
            {
                // Fallback: fondo con color y emoji como placeholder
                iconImg.color = new Color(0.15f, 0.12f, 0.08f, 0.9f);
                Debug.LogWarning($"[CashBattleUIBuilder] Icon not found: {iconPath}. Using placeholder.");

                // Añadir texto placeholder
                GameObject textObj = new GameObject("IconText");
                textObj.transform.SetParent(iconObj.transform, false);

                RectTransform textRT = textObj.AddComponent<RectTransform>();
                textRT.anchorMin = Vector2.zero;
                textRT.anchorMax = Vector2.one;
                textRT.sizeDelta = Vector2.zero;

                TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
                text.text = GetFallbackEmoji(iconName);
                text.fontSize = 60;  // Icono 150x150 para todos
                text.color = TEXT_GOLD;
                text.alignment = TextAlignmentOptions.Center;
            }
        }

        /// <summary>
        /// Retorna emoji de fallback basado en el nombre del icono
        /// </summary>
        private static string GetFallbackEmoji(string iconName)
        {
            if (iconName.Contains("Battles")) return "VS";
            if (iconName.Contains("Tournament")) return "T";
            if (iconName.Contains("Wallet")) return "$";
            if (iconName.Contains("History")) return "H";
            return "?";
        }

        // Mantener metodo legacy para compatibilidad
        private static GameObject CreateOptionCard(Transform parent, string name, string title, string subtitle, string detail,
            Vector2 anchorMinTop, Vector2 anchorMaxTop, Vector2 anchorMinBottom, Vector2 anchorMaxBottom)
        {
            return CreatePremiumCard(parent, name, title, subtitle, detail,
                new Vector2(anchorMinTop.x, anchorMinBottom.y),
                new Vector2(anchorMaxTop.x, anchorMaxTop.y));
        }

        #endregion

        #region Game Selection Panel

        private static void CreateGameSelectionPanel(Transform parent)
        {
            Debug.Log("=== CREANDO GAME SELECTION PANEL CON PREMIUM CARDS v2 ===");

            GameObject panel = new GameObject("GameSelectionPanel");
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = new Vector2(20, 20);
            rt.offsetMax = new Vector2(-20, -130);

            // Add the CashBattle1v1Manager script
            System.Type panelType = System.Type.GetType("DigitPark.UI.CashBattle.CashBattle1v1Manager, Assembly-CSharp");
            if (panelType != null)
            {
                panel.AddComponent(panelType);
            }

            // Panel Title
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(panel.transform, false);

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

            // ========== ONLINE PLAYERS INDICATOR ==========
            GameObject onlineIndicator = new GameObject("OnlinePlayersIndicator");
            onlineIndicator.transform.SetParent(panel.transform, false);

            RectTransform onlineRT = onlineIndicator.AddComponent<RectTransform>();
            onlineRT.anchorMin = new Vector2(0, 1);
            onlineRT.anchorMax = new Vector2(1, 1);
            onlineRT.pivot = new Vector2(0.5f, 1);
            onlineRT.sizeDelta = new Vector2(0, 30);
            onlineRT.anchoredPosition = new Vector2(0, -48);

            // Online indicator background
            Image onlineBg = onlineIndicator.AddComponent<Image>();
            onlineBg.color = new Color(0.05f, 0.08f, 0.12f, 0.8f);

            // Green dot
            GameObject greenDot = new GameObject("GreenDot");
            greenDot.transform.SetParent(onlineIndicator.transform, false);

            RectTransform dotRT = greenDot.AddComponent<RectTransform>();
            dotRT.anchorMin = new Vector2(0.5f, 0.5f);
            dotRT.anchorMax = new Vector2(0.5f, 0.5f);
            dotRT.sizeDelta = new Vector2(10, 10);
            dotRT.anchoredPosition = new Vector2(-140, 0);

            Image dotImg = greenDot.AddComponent<Image>();
            dotImg.color = new Color(0.2f, 1f, 0.4f, 1f);

            Outline dotGlow = greenDot.AddComponent<Outline>();
            dotGlow.effectColor = new Color(0.2f, 1f, 0.4f, 0.5f);
            dotGlow.effectDistance = new Vector2(2, 2);

            // Online text
            GameObject onlineTextObj = new GameObject("OnlineText");
            onlineTextObj.transform.SetParent(onlineIndicator.transform, false);

            RectTransform onlineTextRT = onlineTextObj.AddComponent<RectTransform>();
            onlineTextRT.anchorMin = new Vector2(0.5f, 0);
            onlineTextRT.anchorMax = new Vector2(0.5f, 1);
            onlineTextRT.sizeDelta = new Vector2(300, 0);
            onlineTextRT.anchoredPosition = new Vector2(15, 0);

            TextMeshProUGUI onlineText = onlineTextObj.AddComponent<TextMeshProUGUI>();
            onlineText.text = "47 jugadores online | Pool: $2,340";
            onlineText.fontSize = 18;
            onlineText.color = TEXT_SECONDARY;
            onlineText.alignment = TextAlignmentOptions.Left;
            onlineText.fontStyle = FontStyles.Normal;

            // ========== SCROLL VIEW FOR GAME CARDS ==========
            GameObject scrollView = new GameObject("GamesScrollView");
            scrollView.transform.SetParent(panel.transform, false);

            RectTransform scrollRT = scrollView.AddComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0, 0.33f); // Ajustado para dar espacio al EntryFeeSection
            scrollRT.anchorMax = new Vector2(1, 0.9f);  // Ajustado para el header + online indicator
            scrollRT.sizeDelta = Vector2.zero;
            scrollRT.offsetMin = new Vector2(5, 0);
            scrollRT.offsetMax = new Vector2(-5, -5);

            ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.1f;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.135f;
            scrollRect.scrollSensitivity = 20f;

            // Mask for scroll content
            Image scrollMask = scrollView.AddComponent<Image>();
            scrollMask.color = Color.clear;
            Mask mask = scrollView.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            // Games Container (content)
            GameObject gamesContainer = new GameObject("GamesContainer");
            gamesContainer.transform.SetParent(scrollView.transform, false);

            RectTransform gamesRT = gamesContainer.AddComponent<RectTransform>();
            gamesRT.anchorMin = new Vector2(0, 1);
            gamesRT.anchorMax = new Vector2(1, 1);
            gamesRT.pivot = new Vector2(0.5f, 1);
            gamesRT.sizeDelta = new Vector2(0, 0); // Will be set by ContentSizeFitter

            // Content Size Fitter for dynamic height
            ContentSizeFitter sizeFitter = gamesContainer.AddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = gamesRT;

            // Grid Layout for game cards - Premium visual style optimized for new layout
            GridLayoutGroup gridLayout = gamesContainer.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(320, 320); // Optimizado para el nuevo layout con más elementos
            gridLayout.spacing = new Vector2(20, 20);
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.UpperCenter;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 2;
            gridLayout.padding = new RectOffset(50, 50, 10, 10);

            // Create game cards with visual icons and gold glow
            Debug.Log("=== CREANDO PREMIUM GAME CARDS CON GLOW DORADO ===");
            CreatePremiumGameCard(gamesContainer.transform, "DigitRush", "DigitRushIcon");
            CreatePremiumGameCard(gamesContainer.transform, "MemoryPairs", "MemoryPairsIcon");
            CreatePremiumGameCard(gamesContainer.transform, "QuickMath", "QuickMathIcon");
            CreatePremiumGameCard(gamesContainer.transform, "FlashTap", "FlashTapIcon");
            CreatePremiumGameCard(gamesContainer.transform, "OddOneOut", "OddOneOutIcon");

            // Entry Fee Selection
            CreateEntryFeeSection(panel.transform);

            // Find Opponent Button
            CreateFindOpponentButton(panel.transform);

            // Note: Back button removed - user will add their own prefab

            // Initially hidden
            panel.SetActive(false);
        }

        private static void CreateGameCard(Transform parent, string gameId, string gameName, string description, string icon)
        {
            GameObject card = new GameObject($"GameCard_{gameId}");
            card.transform.SetParent(parent, false);

            // Card background
            Image bg = card.AddComponent<Image>();
            bg.color = CARD_BG;

            // Gold border
            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = CARD_BORDER;
            outline.effectDistance = new Vector2(2, -2);

            // Button
            Button btn = card.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.05f, 1f);
            colors.pressedColor = new Color(0.9f, 0.85f, 0.8f);
            btn.colors = colors;
            btn.targetGraphic = bg;

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(card.transform, false);

            RectTransform iconRT = iconObj.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0, 0.5f);
            iconRT.anchorMax = new Vector2(0, 0.5f);
            iconRT.pivot = new Vector2(0, 0.5f);
            iconRT.sizeDelta = new Vector2(60, 60);
            iconRT.anchoredPosition = new Vector2(15, 0);

            Image iconBg = iconObj.AddComponent<Image>();
            iconBg.color = new Color(0.2f, 0.15f, 0.1f, 0.8f);

            GameObject iconText = new GameObject("IconText");
            iconText.transform.SetParent(iconObj.transform, false);

            RectTransform iconTextRT = iconText.AddComponent<RectTransform>();
            iconTextRT.anchorMin = Vector2.zero;
            iconTextRT.anchorMax = Vector2.one;
            iconTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI iconTMP = iconText.AddComponent<TextMeshProUGUI>();
            iconTMP.text = icon;
            iconTMP.fontSize = 32;
            iconTMP.color = TEXT_GOLD;
            iconTMP.alignment = TextAlignmentOptions.Center;

            // Name
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(card.transform, false);

            RectTransform nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0.5f);
            nameRT.anchorMax = new Vector2(1, 1);
            nameRT.sizeDelta = Vector2.zero;
            nameRT.offsetMin = new Vector2(85, 10);
            nameRT.offsetMax = new Vector2(-50, -10);

            TextMeshProUGUI nameTMP = nameObj.AddComponent<TextMeshProUGUI>();
            nameTMP.text = gameName;
            nameTMP.fontSize = 28;
            nameTMP.color = TEXT_GOLD;
            nameTMP.fontStyle = FontStyles.Bold;
            nameTMP.alignment = TextAlignmentOptions.Left;

            // Description
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(card.transform, false);

            RectTransform descRT = descObj.AddComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0, 0);
            descRT.anchorMax = new Vector2(1, 0.5f);
            descRT.sizeDelta = Vector2.zero;
            descRT.offsetMin = new Vector2(85, 10);
            descRT.offsetMax = new Vector2(-10, -5);

            TextMeshProUGUI descTMP = descObj.AddComponent<TextMeshProUGUI>();
            descTMP.text = description;
            descTMP.fontSize = 20;
            descTMP.color = TEXT_SECONDARY;
            descTMP.fontStyle = FontStyles.Bold;
            descTMP.alignment = TextAlignmentOptions.Left;

            // Checkmark (hidden by default)
            GameObject checkmark = new GameObject("Checkmark");
            checkmark.transform.SetParent(card.transform, false);

            RectTransform checkRT = checkmark.AddComponent<RectTransform>();
            checkRT.anchorMin = new Vector2(1, 1);
            checkRT.anchorMax = new Vector2(1, 1);
            checkRT.pivot = new Vector2(1, 1);
            checkRT.sizeDelta = new Vector2(40, 40);
            checkRT.anchoredPosition = new Vector2(-10, -10);

            Image checkBg = checkmark.AddComponent<Image>();
            checkBg.color = new Color(0.3f, 1f, 0.5f, 1f); // Green

            GameObject checkText = new GameObject("CheckText");
            checkText.transform.SetParent(checkmark.transform, false);

            RectTransform checkTextRT = checkText.AddComponent<RectTransform>();
            checkTextRT.anchorMin = Vector2.zero;
            checkTextRT.anchorMax = Vector2.one;
            checkTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI checkTMP = checkText.AddComponent<TextMeshProUGUI>();
            checkTMP.text = "V";
            checkTMP.fontSize = 28;
            checkTMP.color = BG_DARK;
            checkTMP.alignment = TextAlignmentOptions.Center;
            checkTMP.fontStyle = FontStyles.Bold;

            checkmark.SetActive(false);
        }

        /// <summary>
        /// Creates a premium game card with visual icon and gold glow effect
        /// Similar to GameSelector style but with premium gold accents
        /// </summary>
        private static void CreatePremiumGameCard(Transform parent, string gameId, string iconName)
        {
            GameObject card = new GameObject($"GameCard_{gameId}");
            card.transform.SetParent(parent, false);

            // Card background - will show the game icon image
            Image cardBg = card.AddComponent<Image>();
            cardBg.color = Color.white; // White to show image without tint
            cardBg.preserveAspect = true;

            // Try to load the game icon sprite
            string iconPath = $"Assets/_Project/Art/Icons/Games/{iconName}.png";
            Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            if (iconSprite != null)
            {
                cardBg.sprite = iconSprite;
            }
            else
            {
                // Fallback to dark card if no image found
                cardBg.color = CARD_BG;
                Debug.LogWarning($"[CashBattleUIBuilder] Icon not found: {iconPath}");
            }

            // ========== PREMIUM GOLD GLOW EFFECT ==========
            // Multiple outlines create layered glow effect

            // Inner gold border (sharp)
            Outline innerOutline = card.AddComponent<Outline>();
            innerOutline.effectColor = GOLD_PRIMARY; // Bright gold
            innerOutline.effectDistance = new Vector2(3, 3);

            // Middle gold glow (semi-transparent)
            Outline middleOutline = card.AddComponent<Outline>();
            middleOutline.effectColor = new Color(1f, 0.84f, 0f, 0.6f); // Gold 60% opacity
            middleOutline.effectDistance = new Vector2(6, 6);

            // Outer gold glow (soft)
            Outline outerOutline = card.AddComponent<Outline>();
            outerOutline.effectColor = new Color(1f, 0.75f, 0f, 0.35f); // Amber 35% opacity
            outerOutline.effectDistance = new Vector2(10, 10);

            // Extra outer glow (very soft)
            Outline extraOutline = card.AddComponent<Outline>();
            extraOutline.effectColor = new Color(1f, 0.65f, 0f, 0.15f); // Dark gold 15% opacity
            extraOutline.effectDistance = new Vector2(15, 15);

            // Button component
            Button btn = card.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.1f, 1f, 1f); // Brighter on hover
            colors.pressedColor = new Color(0.9f, 0.85f, 0.8f, 1f);
            colors.selectedColor = new Color(1.1f, 1.05f, 1f, 1f);
            btn.colors = colors;
            btn.targetGraphic = cardBg;

            // ========== SELECTION CHECKMARK ==========
            GameObject checkmark = new GameObject("Checkmark");
            checkmark.transform.SetParent(card.transform, false);

            RectTransform checkRT = checkmark.AddComponent<RectTransform>();
            checkRT.anchorMin = new Vector2(1, 1);
            checkRT.anchorMax = new Vector2(1, 1);
            checkRT.pivot = new Vector2(1, 1);
            checkRT.sizeDelta = new Vector2(60, 60);
            checkRT.anchoredPosition = new Vector2(-15, -15);

            // Checkmark background circle
            Image checkBg = checkmark.AddComponent<Image>();
            checkBg.color = GOLD_PRIMARY; // Gold checkmark background

            // Checkmark border
            Outline checkOutline = checkmark.AddComponent<Outline>();
            checkOutline.effectColor = new Color(1f, 1f, 1f, 0.8f);
            checkOutline.effectDistance = new Vector2(2, 2);

            // Checkmark text
            GameObject checkText = new GameObject("CheckText");
            checkText.transform.SetParent(checkmark.transform, false);

            RectTransform checkTextRT = checkText.AddComponent<RectTransform>();
            checkTextRT.anchorMin = Vector2.zero;
            checkTextRT.anchorMax = Vector2.one;
            checkTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI checkTMP = checkText.AddComponent<TextMeshProUGUI>();
            checkTMP.text = "V";
            checkTMP.fontSize = 36;
            checkTMP.color = BG_DARK; // Dark text on gold background
            checkTMP.alignment = TextAlignmentOptions.Center;
            checkTMP.fontStyle = FontStyles.Bold;

            checkmark.SetActive(false); // Hidden by default until selected
        }

        /// <summary>
        /// Creates a premium entry fee section with:
        /// - 6 preset buttons ($1, $5, $10, $25, $50, $100)
        /// - Custom input field (max $250)
        /// - Real-time earnings feedback
        /// </summary>
        private static void CreateEntryFeeSection(Transform parent)
        {
            GameObject feeSection = new GameObject("EntryFeeSection");
            feeSection.transform.SetParent(parent, false);

            RectTransform rt = feeSection.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.08f);
            rt.anchorMax = new Vector2(1, 0.32f); // Más espacio para el nuevo diseño
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = new Vector2(15, 0);
            rt.offsetMax = new Vector2(-15, 0);

            // Background card for the section
            Image sectionBg = feeSection.AddComponent<Image>();
            sectionBg.color = new Color(0.08f, 0.06f, 0.1f, 0.9f);

            Outline sectionOutline = feeSection.AddComponent<Outline>();
            sectionOutline.effectColor = new Color(0.85f, 0.65f, 0.13f, 0.4f);
            sectionOutline.effectDistance = new Vector2(2, -2);

            // ========== TITLE: "Elige tu apuesta" ==========
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(feeSection.transform, false);

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

            // ========== PRESET BUTTONS ROW ==========
            GameObject presetsContainer = new GameObject("PresetsContainer");
            presetsContainer.transform.SetParent(feeSection.transform, false);

            RectTransform presetsRT = presetsContainer.AddComponent<RectTransform>();
            presetsRT.anchorMin = new Vector2(0, 0.52f);
            presetsRT.anchorMax = new Vector2(1, 0.8f);
            presetsRT.sizeDelta = Vector2.zero;
            presetsRT.offsetMin = new Vector2(10, 0);
            presetsRT.offsetMax = new Vector2(-10, 0);

            HorizontalLayoutGroup presetsLayout = presetsContainer.AddComponent<HorizontalLayoutGroup>();
            presetsLayout.spacing = 12;
            presetsLayout.childAlignment = TextAnchor.MiddleCenter;
            presetsLayout.childForceExpandWidth = true;
            presetsLayout.childForceExpandHeight = true;
            presetsLayout.padding = new RectOffset(5, 5, 5, 5);

            // Create 6 preset buttons
            decimal[] presets = { 1m, 5m, 10m, 25m, 50m, 100m };
            foreach (var preset in presets)
            {
                CreatePresetButton(presetsContainer.transform, preset);
            }

            // ========== CUSTOM INPUT ROW ==========
            GameObject customInputContainer = new GameObject("CustomInputContainer");
            customInputContainer.transform.SetParent(feeSection.transform, false);

            RectTransform customRT = customInputContainer.AddComponent<RectTransform>();
            customRT.anchorMin = new Vector2(0, 0.28f);
            customRT.anchorMax = new Vector2(1, 0.5f);
            customRT.sizeDelta = Vector2.zero;
            customRT.offsetMin = new Vector2(15, 0);
            customRT.offsetMax = new Vector2(-15, 0);

            // Dollar sign
            GameObject dollarSign = new GameObject("DollarSign");
            dollarSign.transform.SetParent(customInputContainer.transform, false);

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

            // Input field background
            GameObject inputBg = new GameObject("CustomInputField");
            inputBg.transform.SetParent(customInputContainer.transform, false);

            RectTransform inputBgRT = inputBg.AddComponent<RectTransform>();
            inputBgRT.anchorMin = new Vector2(0.09f, 0.1f);
            inputBgRT.anchorMax = new Vector2(0.55f, 0.9f);
            inputBgRT.sizeDelta = Vector2.zero;

            Image inputBgImg = inputBg.AddComponent<Image>();
            inputBgImg.color = new Color(0.15f, 0.12f, 0.18f, 1f);

            Outline inputOutline = inputBg.AddComponent<Outline>();
            inputOutline.effectColor = CARD_BORDER;
            inputOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // Input field text area
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
            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(inputBg.transform, false);

            RectTransform placeholderRT = placeholderObj.AddComponent<RectTransform>();
            placeholderRT.anchorMin = Vector2.zero;
            placeholderRT.anchorMax = Vector2.one;
            placeholderRT.sizeDelta = Vector2.zero;
            placeholderRT.offsetMin = new Vector2(10, 0);
            placeholderRT.offsetMax = new Vector2(-10, 0);

            TextMeshProUGUI placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholderText.text = "Otro monto...";
            placeholderText.fontSize = 24;
            placeholderText.color = TEXT_SECONDARY;
            placeholderText.fontStyle = FontStyles.Italic;
            placeholderText.alignment = TextAlignmentOptions.Left;

            // TMP_InputField component
            TMP_InputField inputField = inputBg.AddComponent<TMP_InputField>();
            inputField.textViewport = inputTextRT;
            inputField.textComponent = inputText;
            inputField.placeholder = placeholderText;
            inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
            inputField.characterLimit = 6;

            // Max label
            GameObject maxLabel = new GameObject("MaxLabel");
            maxLabel.transform.SetParent(customInputContainer.transform, false);

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

            // Apply button for custom amount
            GameObject applyBtn = new GameObject("ApplyButton");
            applyBtn.transform.SetParent(customInputContainer.transform, false);

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

            Outline applyOutline = applyBtn.AddComponent<Outline>();
            applyOutline.effectColor = new Color(0f, 1f, 1f, 0.5f);
            applyOutline.effectDistance = new Vector2(2, -2);

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

            // ========== EARNINGS FEEDBACK SECTION ==========
            GameObject feedbackContainer = new GameObject("EarningsFeedback");
            feedbackContainer.transform.SetParent(feeSection.transform, false);

            RectTransform feedbackRT = feedbackContainer.AddComponent<RectTransform>();
            feedbackRT.anchorMin = new Vector2(0, 0);
            feedbackRT.anchorMax = new Vector2(1, 0.26f);
            feedbackRT.sizeDelta = Vector2.zero;
            feedbackRT.offsetMin = new Vector2(15, 8);
            feedbackRT.offsetMax = new Vector2(-15, -2);

            // Feedback background
            Image feedbackBg = feedbackContainer.AddComponent<Image>();
            feedbackBg.color = new Color(0.05f, 0.12f, 0.08f, 0.9f); // Verde oscuro sutil

            Outline feedbackOutline = feedbackContainer.AddComponent<Outline>();
            feedbackOutline.effectColor = new Color(0.3f, 1f, 0.5f, 0.4f);
            feedbackOutline.effectDistance = new Vector2(1, -1);

            // Potential earnings text
            GameObject earningsObj = new GameObject("PotentialEarningsText");
            earningsObj.transform.SetParent(feedbackContainer.transform, false);

            RectTransform earningsRT = earningsObj.AddComponent<RectTransform>();
            earningsRT.anchorMin = new Vector2(0, 0.5f);
            earningsRT.anchorMax = new Vector2(0.6f, 1);
            earningsRT.sizeDelta = Vector2.zero;
            earningsRT.offsetMin = new Vector2(15, 0);
            earningsRT.offsetMax = new Vector2(0, -3);

            TextMeshProUGUI earningsText = earningsObj.AddComponent<TextMeshProUGUI>();
            earningsText.text = "Si ganas recibes: $0.00";
            earningsText.fontSize = 22;
            earningsText.color = new Color(0.4f, 1f, 0.6f, 1f); // Verde brillante
            earningsText.fontStyle = FontStyles.Bold;
            earningsText.alignment = TextAlignmentOptions.Left;

            // Pool info text
            GameObject poolObj = new GameObject("PoolInfoText");
            poolObj.transform.SetParent(feedbackContainer.transform, false);

            RectTransform poolRT = poolObj.AddComponent<RectTransform>();
            poolRT.anchorMin = new Vector2(0, 0);
            poolRT.anchorMax = new Vector2(1, 0.5f);
            poolRT.sizeDelta = Vector2.zero;
            poolRT.offsetMin = new Vector2(15, 3);
            poolRT.offsetMax = new Vector2(-15, 0);

            TextMeshProUGUI poolText = poolObj.AddComponent<TextMeshProUGUI>();
            poolText.text = "Pool: $0.00 | Tu apuesta: $0.00 | Fee: 30%";
            poolText.fontSize = 18;
            poolText.color = TEXT_SECONDARY;
            poolText.fontStyle = FontStyles.Normal;
            poolText.alignment = TextAlignmentOptions.Left;

            // Coin icon for visual appeal
            GameObject coinIcon = new GameObject("CoinIcon");
            coinIcon.transform.SetParent(feedbackContainer.transform, false);

            RectTransform coinRT = coinIcon.AddComponent<RectTransform>();
            coinRT.anchorMin = new Vector2(0.85f, 0.2f);
            coinRT.anchorMax = new Vector2(0.98f, 0.8f);
            coinRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI coinText = coinIcon.AddComponent<TextMeshProUGUI>();
            coinText.text = "$";
            coinText.fontSize = 36;
            coinText.alignment = TextAlignmentOptions.Center;
        }

        /// <summary>
        /// Creates a preset amount button with premium styling
        /// </summary>
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

            // Gold border
            Outline outline = btnObj.AddComponent<Outline>();
            outline.effectColor = CARD_BORDER;
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            // Amount text
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

            // Selection indicator (hidden by default)
            GameObject selectedIndicator = new GameObject("SelectedIndicator");
            selectedIndicator.transform.SetParent(btnObj.transform, false);

            RectTransform indicatorRT = selectedIndicator.AddComponent<RectTransform>();
            indicatorRT.anchorMin = new Vector2(0.5f, 0);
            indicatorRT.anchorMax = new Vector2(0.5f, 0);
            indicatorRT.pivot = new Vector2(0.5f, 0);
            indicatorRT.sizeDelta = new Vector2(40, 4);
            indicatorRT.anchoredPosition = new Vector2(0, 2);

            Image indicatorImg = selectedIndicator.AddComponent<Image>();
            indicatorImg.color = GOLD_PRIMARY;

            selectedIndicator.SetActive(false);
        }

        // Legacy method kept for backwards compatibility
        private static void CreateEntryFeeButton(Transform parent, decimal fee)
        {
            CreatePresetButton(parent, fee);
        }

        /// <summary>
        /// Creates a premium "Find Opponent" button with:
        /// - Eye-catching fire emoji
        /// - Online players counter
        /// - Pulsing glow effect styling
        /// </summary>
        private static void CreateFindOpponentButton(Transform parent)
        {
            // Main button container
            GameObject btnContainer = new GameObject("FindOpponentContainer");
            btnContainer.transform.SetParent(parent, false);

            RectTransform containerRT = btnContainer.AddComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0.08f, 0);
            containerRT.anchorMax = new Vector2(0.92f, 0.07f);
            containerRT.sizeDelta = Vector2.zero;

            // The actual button
            GameObject btnObj = new GameObject("FindOpponentButton");
            btnObj.transform.SetParent(btnContainer.transform, false);

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

            // Multiple outlines for glow effect
            Outline glow1 = btnObj.AddComponent<Outline>();
            glow1.effectColor = new Color(1f, 0.8f, 0.3f, 0.7f);
            glow1.effectDistance = new Vector2(3, -3);

            Outline glow2 = btnObj.AddComponent<Outline>();
            glow2.effectColor = new Color(1f, 0.6f, 0.1f, 0.4f);
            glow2.effectDistance = new Vector2(6, -6);

            Outline glow3 = btnObj.AddComponent<Outline>();
            glow3.effectColor = new Color(1f, 0.5f, 0f, 0.2f);
            glow3.effectDistance = new Vector2(10, -10);

            // Main text with fire emojis
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0, 0.35f);
            textRT.anchorMax = new Vector2(1, 1);
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "BUSCAR RIVAL";
            text.fontSize = 34;
            text.color = BG_DARK;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;

            // Subtitle with online players count
            GameObject subtitleObj = new GameObject("OnlinePlayersText");
            subtitleObj.transform.SetParent(btnObj.transform, false);

            RectTransform subtitleRT = subtitleObj.AddComponent<RectTransform>();
            subtitleRT.anchorMin = new Vector2(0, 0);
            subtitleRT.anchorMax = new Vector2(1, 0.4f);
            subtitleRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI subtitleText = subtitleObj.AddComponent<TextMeshProUGUI>();
            subtitleText.text = "12 jugadores buscando ahora";
            subtitleText.fontSize = 18;
            subtitleText.color = new Color(0.2f, 0.15f, 0.1f, 0.9f);
            subtitleText.fontStyle = FontStyles.Normal;
            subtitleText.alignment = TextAlignmentOptions.Center;

            // Left fire icon
            GameObject leftFire = new GameObject("LeftFireIcon");
            leftFire.transform.SetParent(btnObj.transform, false);

            RectTransform leftFireRT = leftFire.AddComponent<RectTransform>();
            leftFireRT.anchorMin = new Vector2(0, 0.5f);
            leftFireRT.anchorMax = new Vector2(0, 0.5f);
            leftFireRT.pivot = new Vector2(0, 0.5f);
            leftFireRT.sizeDelta = new Vector2(50, 50);
            leftFireRT.anchoredPosition = new Vector2(15, 5);

            TextMeshProUGUI leftFireText = leftFire.AddComponent<TextMeshProUGUI>();
            leftFireText.text = "🔥";
            leftFireText.fontSize = 32;
            leftFireText.alignment = TextAlignmentOptions.Center;

            // Right fire icon
            GameObject rightFire = new GameObject("RightFireIcon");
            rightFire.transform.SetParent(btnObj.transform, false);

            RectTransform rightFireRT = rightFire.AddComponent<RectTransform>();
            rightFireRT.anchorMin = new Vector2(1, 0.5f);
            rightFireRT.anchorMax = new Vector2(1, 0.5f);
            rightFireRT.pivot = new Vector2(1, 0.5f);
            rightFireRT.sizeDelta = new Vector2(50, 50);
            rightFireRT.anchoredPosition = new Vector2(-15, 5);

            TextMeshProUGUI rightFireText = rightFire.AddComponent<TextMeshProUGUI>();
            rightFireText.text = "🔥";
            rightFireText.fontSize = 32;
            rightFireText.alignment = TextAlignmentOptions.Center;

            // Online indicator dot
            GameObject onlineDot = new GameObject("OnlineDot");
            onlineDot.transform.SetParent(btnObj.transform, false);

            RectTransform dotRT = onlineDot.AddComponent<RectTransform>();
            dotRT.anchorMin = new Vector2(0.5f, 0);
            dotRT.anchorMax = new Vector2(0.5f, 0);
            dotRT.pivot = new Vector2(0.5f, 0);
            dotRT.sizeDelta = new Vector2(12, 12);
            dotRT.anchoredPosition = new Vector2(-80, 12);

            Image dotImg = onlineDot.AddComponent<Image>();
            dotImg.color = new Color(0.2f, 0.9f, 0.3f, 1f); // Verde brillante

            // Glow around the dot
            Outline dotGlow = onlineDot.AddComponent<Outline>();
            dotGlow.effectColor = new Color(0.2f, 1f, 0.3f, 0.6f);
            dotGlow.effectDistance = new Vector2(2, 2);
        }

        // Note: CreatePanelBackButton removed - user will add their own prefab

        #endregion

        #region Tournament List Panel

        private static void CreateTournamentListPanel(Transform parent)
        {
            GameObject panel = new GameObject("TournamentListPanel");
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = new Vector2(20, 20);
            rt.offsetMax = new Vector2(-20, -130);

            // Add the TournamentListPanel script
            System.Type panelType = System.Type.GetType("DigitPark.UI.CashBattle.TournamentListPanel, Assembly-CSharp");
            if (panelType != null)
            {
                panel.AddComponent(panelType);
            }

            // Panel Title
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(panel.transform, false);

            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 1);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0.5f, 1);
            titleRT.sizeDelta = new Vector2(0, 60);
            titleRT.anchoredPosition = Vector2.zero;

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Torneos Disponibles";
            titleText.fontSize = 38;
            titleText.color = TEXT_GOLD;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;

            // Tournaments Container (ScrollView)
            GameObject scrollView = CreateScrollView(panel.transform, "TournamentsScrollView",
                new Vector2(0, 0.05f), new Vector2(1, 0.9f));

            // Create sample tournament cards
            Transform content = scrollView.transform.Find("Viewport/Content");
            if (content != null)
            {
                CreateTournamentCard(content, "Quick Math Championship", "QuickMath", 5m, 100m, "12/16");
                CreateTournamentCard(content, "Flash Tap Masters", "FlashTap", 10m, 250m, "28/32");
                CreateTournamentCard(content, "Cognitive Sprint Elite", "Sprint", 25m, 500m, "8/16");
                CreateTournamentCard(content, "Memory Pairs Daily", "MemoryPairs", 1m, 20m, "18/20");
            }

            // Note: Back button removed - user will add their own prefab

            // Initially hidden
            panel.SetActive(false);
        }

        private static GameObject CreateScrollView(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject scrollView = new GameObject(name);
            scrollView.transform.SetParent(parent, false);

            RectTransform scrollRT = scrollView.AddComponent<RectTransform>();
            scrollRT.anchorMin = anchorMin;
            scrollRT.anchorMax = anchorMax;
            scrollRT.sizeDelta = Vector2.zero;
            scrollRT.offsetMin = new Vector2(10, 10);
            scrollRT.offsetMax = new Vector2(-10, -10);

            ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);

            RectTransform viewportRT = viewport.AddComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.sizeDelta = Vector2.zero;

            Image viewportMask = viewport.AddComponent<Image>();
            viewportMask.color = new Color(1, 1, 1, 0.01f);

            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);

            RectTransform contentRT = content.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.sizeDelta = new Vector2(0, 0);

            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 15;
            layout.padding = new RectOffset(0, 0, 10, 10);
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childControlHeight = false;

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewportRT;
            scrollRect.content = contentRT;

            return scrollView;
        }

        private static void CreateTournamentCard(Transform parent, string tournamentName, string gameType, decimal entryFee, decimal prizePool, string participants)
        {
            GameObject card = new GameObject($"Tournament_{tournamentName.Replace(" ", "_")}");
            card.transform.SetParent(parent, false);

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredHeight = 160;

            Image bg = card.AddComponent<Image>();
            bg.color = CARD_BG;

            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = CARD_BORDER;
            outline.effectDistance = new Vector2(2, -2);

            // Info container
            GameObject info = new GameObject("Info");
            info.transform.SetParent(card.transform, false);

            RectTransform infoRT = info.AddComponent<RectTransform>();
            infoRT.anchorMin = new Vector2(0, 0);
            infoRT.anchorMax = new Vector2(0.7f, 1);
            infoRT.sizeDelta = Vector2.zero;
            infoRT.offsetMin = new Vector2(20, 15);
            infoRT.offsetMax = new Vector2(0, -15);

            // Name
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(info.transform, false);

            RectTransform nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0.7f);
            nameRT.anchorMax = new Vector2(1, 1);
            nameRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI nameTMP = nameObj.AddComponent<TextMeshProUGUI>();
            nameTMP.text = tournamentName;
            nameTMP.fontSize = 28;
            nameTMP.color = TEXT_GOLD;
            nameTMP.fontStyle = FontStyles.Bold;

            // Game type
            GameObject gameObj = new GameObject("GameType");
            gameObj.transform.SetParent(info.transform, false);

            RectTransform gameRT = gameObj.AddComponent<RectTransform>();
            gameRT.anchorMin = new Vector2(0, 0.45f);
            gameRT.anchorMax = new Vector2(1, 0.7f);
            gameRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI gameTMP = gameObj.AddComponent<TextMeshProUGUI>();
            gameTMP.text = gameType;
            gameTMP.fontSize = 22;
            gameTMP.color = CYAN_ACCENT;
            gameTMP.fontStyle = FontStyles.Bold;

            // Prize
            GameObject prizeObj = new GameObject("PrizePool");
            prizeObj.transform.SetParent(info.transform, false);

            RectTransform prizeRT = prizeObj.AddComponent<RectTransform>();
            prizeRT.anchorMin = new Vector2(0, 0.2f);
            prizeRT.anchorMax = new Vector2(0.5f, 0.45f);
            prizeRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI prizeTMP = prizeObj.AddComponent<TextMeshProUGUI>();
            prizeTMP.text = $"Premio: ${prizePool}";
            prizeTMP.fontSize = 22;
            prizeTMP.color = new Color(0.3f, 1f, 0.5f);
            prizeTMP.fontStyle = FontStyles.Bold;

            // Entry
            GameObject entryObj = new GameObject("EntryFee");
            entryObj.transform.SetParent(info.transform, false);

            RectTransform entryRT = entryObj.AddComponent<RectTransform>();
            entryRT.anchorMin = new Vector2(0.5f, 0.2f);
            entryRT.anchorMax = new Vector2(1, 0.45f);
            entryRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI entryTMP = entryObj.AddComponent<TextMeshProUGUI>();
            entryTMP.text = $"Entrada: ${entryFee}";
            entryTMP.fontSize = 22;
            entryTMP.color = TEXT_PRIMARY;
            entryTMP.fontStyle = FontStyles.Bold;

            // Participants
            GameObject partObj = new GameObject("Participants");
            partObj.transform.SetParent(info.transform, false);

            RectTransform partRT = partObj.AddComponent<RectTransform>();
            partRT.anchorMin = new Vector2(0, 0);
            partRT.anchorMax = new Vector2(1, 0.2f);
            partRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI partTMP = partObj.AddComponent<TextMeshProUGUI>();
            partTMP.text = $"{participants} jugadores";
            partTMP.fontSize = 20;
            partTMP.color = TEXT_SECONDARY;
            partTMP.fontStyle = FontStyles.Bold;

            // Join button
            GameObject joinBtn = new GameObject("JoinButton");
            joinBtn.transform.SetParent(card.transform, false);

            RectTransform joinRT = joinBtn.AddComponent<RectTransform>();
            joinRT.anchorMin = new Vector2(0.72f, 0.2f);
            joinRT.anchorMax = new Vector2(0.98f, 0.8f);
            joinRT.sizeDelta = Vector2.zero;

            Image joinBg = joinBtn.AddComponent<Image>();
            joinBg.color = BUTTON_GOLD;

            Button btn = joinBtn.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = BUTTON_GOLD;
            colors.highlightedColor = GOLD_LIGHT;
            colors.pressedColor = GOLD_DARK;
            btn.colors = colors;

            GameObject joinText = new GameObject("Text");
            joinText.transform.SetParent(joinBtn.transform, false);

            RectTransform joinTextRT = joinText.AddComponent<RectTransform>();
            joinTextRT.anchorMin = Vector2.zero;
            joinTextRT.anchorMax = Vector2.one;
            joinTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI joinTMP = joinText.AddComponent<TextMeshProUGUI>();
            joinTMP.text = "Unirse";
            joinTMP.fontSize = 26;
            joinTMP.color = BG_DARK;
            joinTMP.fontStyle = FontStyles.Bold;
            joinTMP.alignment = TextAlignmentOptions.Center;
        }

        #endregion

        #region Bet Confirmation Panel

        private static void CreateBetConfirmationPanel(Transform parent)
        {
            GameObject panel = new GameObject("ConfirmBetPanel");
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, -120);

            Image overlay = panel.AddComponent<Image>();
            overlay.color = new Color(0, 0, 0, 0.7f);

            // Content box
            GameObject content = new GameObject("Content");
            content.transform.SetParent(panel.transform, false);

            RectTransform contentRT = content.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0.5f, 0.5f);
            contentRT.anchorMax = new Vector2(0.5f, 0.5f);
            contentRT.sizeDelta = new Vector2(480, 300);

            Image contentBg = content.AddComponent<Image>();
            contentBg.color = CARD_BG;

            Outline contentOutline = content.AddComponent<Outline>();
            contentOutline.effectColor = GOLD_PRIMARY;
            contentOutline.effectDistance = new Vector2(3, -3);

            // Confirm text
            GameObject textObj = new GameObject("ConfirmBetText");
            textObj.transform.SetParent(content.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0.05f, 0.45f);
            textRT.anchorMax = new Vector2(0.95f, 0.9f);
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI textTMP = textObj.AddComponent<TextMeshProUGUI>();
            textTMP.text = "Apostar $0.00 en DigitRush?";
            textTMP.fontSize = 32;
            textTMP.color = TEXT_GOLD;
            textTMP.fontStyle = FontStyles.Bold;
            textTMP.alignment = TextAlignmentOptions.Center;

            // Confirm button
            GameObject confirmBtn = new GameObject("ConfirmBetButton");
            confirmBtn.transform.SetParent(content.transform, false);

            RectTransform confirmRT = confirmBtn.AddComponent<RectTransform>();
            confirmRT.anchorMin = new Vector2(0.55f, 0.08f);
            confirmRT.anchorMax = new Vector2(0.95f, 0.35f);
            confirmRT.sizeDelta = Vector2.zero;

            Image confirmBg = confirmBtn.AddComponent<Image>();
            confirmBg.color = BUTTON_GOLD;

            Button confirmButton = confirmBtn.AddComponent<Button>();
            ColorBlock confirmColors = confirmButton.colors;
            confirmColors.normalColor = BUTTON_GOLD;
            confirmColors.highlightedColor = GOLD_LIGHT;
            confirmColors.pressedColor = GOLD_DARK;
            confirmButton.colors = confirmColors;

            GameObject confirmText = new GameObject("Text");
            confirmText.transform.SetParent(confirmBtn.transform, false);

            RectTransform confirmTextRT = confirmText.AddComponent<RectTransform>();
            confirmTextRT.anchorMin = Vector2.zero;
            confirmTextRT.anchorMax = Vector2.one;
            confirmTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI confirmTMP = confirmText.AddComponent<TextMeshProUGUI>();
            confirmTMP.text = "Confirmar";
            confirmTMP.fontSize = 28;
            confirmTMP.color = BG_DARK;
            confirmTMP.fontStyle = FontStyles.Bold;
            confirmTMP.alignment = TextAlignmentOptions.Center;

            // Cancel button
            GameObject cancelBtn = new GameObject("CancelBetButton");
            cancelBtn.transform.SetParent(content.transform, false);

            RectTransform cancelRT = cancelBtn.AddComponent<RectTransform>();
            cancelRT.anchorMin = new Vector2(0.05f, 0.08f);
            cancelRT.anchorMax = new Vector2(0.45f, 0.35f);
            cancelRT.sizeDelta = Vector2.zero;

            Image cancelBg = cancelBtn.AddComponent<Image>();
            cancelBg.color = BUTTON_DANGER;

            Button cancelButton = cancelBtn.AddComponent<Button>();
            ColorBlock cancelColors = cancelButton.colors;
            cancelColors.normalColor = BUTTON_DANGER;
            cancelColors.highlightedColor = new Color(1f, 0.4f, 0.4f);
            cancelColors.pressedColor = new Color(0.6f, 0.15f, 0.15f);
            cancelButton.colors = cancelColors;

            GameObject cancelText = new GameObject("Text");
            cancelText.transform.SetParent(cancelBtn.transform, false);

            RectTransform cancelTextRT = cancelText.AddComponent<RectTransform>();
            cancelTextRT.anchorMin = Vector2.zero;
            cancelTextRT.anchorMax = Vector2.one;
            cancelTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI cancelTMP = cancelText.AddComponent<TextMeshProUGUI>();
            cancelTMP.text = "Cancelar";
            cancelTMP.fontSize = 28;
            cancelTMP.color = TEXT_PRIMARY;
            cancelTMP.fontStyle = FontStyles.Bold;
            cancelTMP.alignment = TextAlignmentOptions.Center;

            panel.SetActive(false);
        }

        #endregion

        #region Matchmaking Panel

        private static void CreateMatchmakingPanel(Transform parent)
        {
            GameObject panel = new GameObject("MatchmakingPanel");
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, -120);

            // Semi-transparent overlay
            Image overlay = panel.AddComponent<Image>();
            overlay.color = new Color(0, 0, 0, 0.7f);

            // Content container
            GameObject content = new GameObject("Content");
            content.transform.SetParent(panel.transform, false);

            RectTransform contentRT = content.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0.5f, 0.5f);
            contentRT.anchorMax = new Vector2(0.5f, 0.5f);
            contentRT.sizeDelta = new Vector2(500, 400);

            Image contentBg = content.AddComponent<Image>();
            contentBg.color = CARD_BG;

            Outline contentOutline = content.AddComponent<Outline>();
            contentOutline.effectColor = GOLD_PRIMARY;
            contentOutline.effectDistance = new Vector2(3, -3);

            // Searching animation (simple rotating icon)
            GameObject searchIcon = new GameObject("SearchIcon");
            searchIcon.transform.SetParent(content.transform, false);

            RectTransform iconRT = searchIcon.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.6f);
            iconRT.anchorMax = new Vector2(0.5f, 0.6f);
            iconRT.sizeDelta = new Vector2(120, 120);

            Image iconBg = searchIcon.AddComponent<Image>();
            iconBg.color = AMBER;

            Outline iconOutline = searchIcon.AddComponent<Outline>();
            iconOutline.effectColor = new Color(1f, 0.8f, 0.3f, 0.8f);
            iconOutline.effectDistance = new Vector2(4, -4);

            GameObject iconText = new GameObject("Text");
            iconText.transform.SetParent(searchIcon.transform, false);

            RectTransform iconTextRT = iconText.AddComponent<RectTransform>();
            iconTextRT.anchorMin = Vector2.zero;
            iconTextRT.anchorMax = Vector2.one;
            iconTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI iconTMP = iconText.AddComponent<TextMeshProUGUI>();
            iconTMP.text = "⚔";
            iconTMP.fontSize = 60;
            iconTMP.color = BG_DARK;
            iconTMP.alignment = TextAlignmentOptions.Center;

            // Status text
            GameObject statusObj = new GameObject("MatchmakingStatusText");
            statusObj.transform.SetParent(content.transform, false);

            RectTransform statusRT = statusObj.AddComponent<RectTransform>();
            statusRT.anchorMin = new Vector2(0, 0.3f);
            statusRT.anchorMax = new Vector2(1, 0.5f);
            statusRT.sizeDelta = Vector2.zero;
            statusRT.offsetMin = new Vector2(20, 0);
            statusRT.offsetMax = new Vector2(-20, 0);

            TextMeshProUGUI statusTMP = statusObj.AddComponent<TextMeshProUGUI>();
            statusTMP.text = "Buscando oponente...";
            statusTMP.fontSize = 36;
            statusTMP.color = TEXT_GOLD;
            statusTMP.fontStyle = FontStyles.Bold;
            statusTMP.alignment = TextAlignmentOptions.Center;

            // Timer text (shows elapsed matchmaking time)
            GameObject timerObj = new GameObject("MatchmakingTimerText");
            timerObj.transform.SetParent(content.transform, false);

            RectTransform timerRT = timerObj.AddComponent<RectTransform>();
            timerRT.anchorMin = new Vector2(0, 0.22f);
            timerRT.anchorMax = new Vector2(1, 0.32f);
            timerRT.sizeDelta = Vector2.zero;
            timerRT.offsetMin = new Vector2(20, 0);
            timerRT.offsetMax = new Vector2(-20, 0);

            TextMeshProUGUI timerTMP = timerObj.AddComponent<TextMeshProUGUI>();
            timerTMP.text = "00:00";
            timerTMP.fontSize = 42;
            timerTMP.color = CYAN_ACCENT;
            timerTMP.fontStyle = FontStyles.Bold;
            timerTMP.alignment = TextAlignmentOptions.Center;

            // Opponent name text
            GameObject opponentObj = new GameObject("OpponentNameText");
            opponentObj.transform.SetParent(content.transform, false);

            RectTransform opponentRT = opponentObj.AddComponent<RectTransform>();
            opponentRT.anchorMin = new Vector2(0, 0.14f);
            opponentRT.anchorMax = new Vector2(1, 0.24f);
            opponentRT.sizeDelta = Vector2.zero;
            opponentRT.offsetMin = new Vector2(20, 0);
            opponentRT.offsetMax = new Vector2(-20, 0);

            TextMeshProUGUI opponentTMP = opponentObj.AddComponent<TextMeshProUGUI>();
            opponentTMP.text = "Buscando...";
            opponentTMP.fontSize = 36;
            opponentTMP.color = TEXT_PRIMARY;
            opponentTMP.fontStyle = FontStyles.Bold;
            opponentTMP.alignment = TextAlignmentOptions.Center;

            // Cancel button
            GameObject cancelBtn = new GameObject("CancelMatchmakingButton");
            cancelBtn.transform.SetParent(content.transform, false);

            RectTransform cancelRT = cancelBtn.AddComponent<RectTransform>();
            cancelRT.anchorMin = new Vector2(0.2f, 0.08f);
            cancelRT.anchorMax = new Vector2(0.8f, 0.22f);
            cancelRT.sizeDelta = Vector2.zero;

            Image cancelBg = cancelBtn.AddComponent<Image>();
            cancelBg.color = BUTTON_DANGER;

            Button btn = cancelBtn.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = BUTTON_DANGER;
            colors.highlightedColor = new Color(1f, 0.4f, 0.4f);
            colors.pressedColor = new Color(0.6f, 0.15f, 0.15f);
            btn.colors = colors;

            GameObject cancelText = new GameObject("Text");
            cancelText.transform.SetParent(cancelBtn.transform, false);

            RectTransform cancelTextRT = cancelText.AddComponent<RectTransform>();
            cancelTextRT.anchorMin = Vector2.zero;
            cancelTextRT.anchorMax = Vector2.one;
            cancelTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI cancelTMP = cancelText.AddComponent<TextMeshProUGUI>();
            cancelTMP.text = "Cancelar";
            cancelTMP.fontSize = 30;
            cancelTMP.color = TEXT_PRIMARY;
            cancelTMP.fontStyle = FontStyles.Bold;
            cancelTMP.alignment = TextAlignmentOptions.Center;

            // Initially hidden
            panel.SetActive(false);
        }

        #endregion

        #region Wallet Panel

        private static void CreateWalletPanel(Transform parent)
        {
            GameObject panel = new GameObject("WalletPanel");
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = new Vector2(20, 20);
            rt.offsetMax = new Vector2(-20, -130);

            // Panel Title
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(panel.transform, false);

            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 1);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0.5f, 1);
            titleRT.sizeDelta = new Vector2(0, 60);
            titleRT.anchoredPosition = Vector2.zero;

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Mi Wallet";
            titleText.fontSize = 42;
            titleText.color = TEXT_GOLD;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;

            // Balance Card
            CreateBalanceCard(panel.transform);

            // Action Buttons (Deposit/Withdraw)
            CreateWalletActions(panel.transform);

            // Recent Transactions
            CreateRecentTransactions(panel.transform);

            // Note: Back button removed - user will add their own prefab

            // Initially hidden
            panel.SetActive(false);
        }

        private static void CreateBalanceCard(Transform parent)
        {
            GameObject card = new GameObject("BalanceCard");
            card.transform.SetParent(parent, false);

            RectTransform rt = card.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.1f, 0.72f);
            rt.anchorMax = new Vector2(0.9f, 0.92f);
            rt.sizeDelta = Vector2.zero;

            Image bg = card.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.12f, 0.08f, 0.95f);

            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = GOLD_PRIMARY;
            outline.effectDistance = new Vector2(3, -3);

            // Balance Amount
            GameObject amountObj = new GameObject("BalanceAmount");
            amountObj.transform.SetParent(card.transform, false);

            RectTransform amountRT = amountObj.AddComponent<RectTransform>();
            amountRT.anchorMin = new Vector2(0, 0.4f);
            amountRT.anchorMax = new Vector2(1, 0.95f);
            amountRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI amountText = amountObj.AddComponent<TextMeshProUGUI>();
            amountText.text = "$0.00";
            amountText.fontSize = 72;
            amountText.color = GOLD_PRIMARY;
            amountText.alignment = TextAlignmentOptions.Center;
            amountText.fontStyle = FontStyles.Bold;

            // Balance Label
            GameObject labelObj = new GameObject("BalanceLabel");
            labelObj.transform.SetParent(card.transform, false);

            RectTransform labelRT = labelObj.AddComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 0.05f);
            labelRT.anchorMax = new Vector2(1, 0.4f);
            labelRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = "Balance Disponible";
            labelText.fontSize = 28;
            labelText.color = TEXT_SECONDARY;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.fontStyle = FontStyles.Bold;
        }

        private static void CreateWalletActions(Transform parent)
        {
            GameObject actionsContainer = new GameObject("ActionsContainer");
            actionsContainer.transform.SetParent(parent, false);

            RectTransform rt = actionsContainer.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.05f, 0.55f);
            rt.anchorMax = new Vector2(0.95f, 0.7f);
            rt.sizeDelta = Vector2.zero;

            HorizontalLayoutGroup layout = actionsContainer.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 30;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            // Deposit Button
            CreateWalletActionButton(actionsContainer.transform, "DepositButton", "DEPOSITAR",
                new Color(0.2f, 0.7f, 0.3f, 1f), new Color(0.3f, 0.85f, 0.4f, 1f));

            // Withdraw Button
            CreateWalletActionButton(actionsContainer.transform, "WithdrawButton", "RETIRAR",
                BUTTON_GOLD, GOLD_LIGHT);
        }

        private static void CreateWalletActionButton(Transform parent, string name, string text, Color normalColor, Color highlightColor)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;

            Image bg = btnObj.AddComponent<Image>();
            bg.color = normalColor;

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = highlightColor;
            colors.pressedColor = new Color(normalColor.r * 0.7f, normalColor.g * 0.7f, normalColor.b * 0.7f, 1f);
            btn.colors = colors;

            Outline glow = btnObj.AddComponent<Outline>();
            glow.effectColor = new Color(normalColor.r, normalColor.g, normalColor.b, 0.5f);
            glow.effectDistance = new Vector2(3, -3);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.text = text;
            btnText.fontSize = 32;
            btnText.color = Color.white;
            btnText.fontStyle = FontStyles.Bold;
            btnText.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateRecentTransactions(Transform parent)
        {
            // Section Header
            GameObject headerObj = new GameObject("TransactionsHeader");
            headerObj.transform.SetParent(parent, false);

            RectTransform headerRT = headerObj.AddComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0.05f, 0.48f);
            headerRT.anchorMax = new Vector2(0.95f, 0.54f);
            headerRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI headerText = headerObj.AddComponent<TextMeshProUGUI>();
            headerText.text = "Transacciones Recientes";
            headerText.fontSize = 28;
            headerText.color = TEXT_PRIMARY;
            headerText.fontStyle = FontStyles.Bold;
            headerText.alignment = TextAlignmentOptions.Left;

            // Transactions ScrollView
            GameObject scrollView = CreateScrollView(parent, "TransactionsScrollView",
                new Vector2(0.02f, 0.08f), new Vector2(0.98f, 0.47f));

            // Sample transactions
            Transform content = scrollView.transform.Find("Viewport/Content");
            if (content != null)
            {
                CreateTransactionItem(content, "+$10.00", "Ganancia - QuickMath vs @Player123", "Hace 2 horas", true);
                CreateTransactionItem(content, "-$5.00", "Entrada - Torneo Flash Tap", "Hace 5 horas", false);
                CreateTransactionItem(content, "+$50.00", "Deposito", "Ayer", true);
                CreateTransactionItem(content, "+$25.00", "Premio - 2do lugar Torneo", "Ayer", true);
                CreateTransactionItem(content, "-$10.00", "Entrada - Battle 1v1", "Hace 2 dias", false);
            }
        }

        private static void CreateTransactionItem(Transform parent, string amount, string description, string time, bool isPositive)
        {
            GameObject item = new GameObject($"Transaction_{description.GetHashCode()}");
            item.transform.SetParent(parent, false);

            LayoutElement le = item.AddComponent<LayoutElement>();
            le.preferredHeight = 80;

            Image bg = item.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.08f, 0.12f, 0.8f);

            // Amount
            GameObject amountObj = new GameObject("Amount");
            amountObj.transform.SetParent(item.transform, false);

            RectTransform amountRT = amountObj.AddComponent<RectTransform>();
            amountRT.anchorMin = new Vector2(0, 0);
            amountRT.anchorMax = new Vector2(0.25f, 1);
            amountRT.sizeDelta = Vector2.zero;
            amountRT.offsetMin = new Vector2(15, 10);
            amountRT.offsetMax = new Vector2(0, -10);

            TextMeshProUGUI amountText = amountObj.AddComponent<TextMeshProUGUI>();
            amountText.text = amount;
            amountText.fontSize = 28;
            amountText.color = isPositive ? new Color(0.3f, 1f, 0.5f) : new Color(1f, 0.4f, 0.4f);
            amountText.fontStyle = FontStyles.Bold;
            amountText.alignment = TextAlignmentOptions.Left;

            // Description
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(item.transform, false);

            RectTransform descRT = descObj.AddComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0.26f, 0.4f);
            descRT.anchorMax = new Vector2(0.85f, 1);
            descRT.sizeDelta = Vector2.zero;
            descRT.offsetMin = new Vector2(5, 0);
            descRT.offsetMax = new Vector2(-5, -5);

            TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = description;
            descText.fontSize = 20;
            descText.color = TEXT_PRIMARY;
            descText.fontStyle = FontStyles.Bold;
            descText.alignment = TextAlignmentOptions.Left;
            descText.enableWordWrapping = true;
            descText.overflowMode = TextOverflowModes.Ellipsis;

            // Time
            GameObject timeObj = new GameObject("Time");
            timeObj.transform.SetParent(item.transform, false);

            RectTransform timeRT = timeObj.AddComponent<RectTransform>();
            timeRT.anchorMin = new Vector2(0.26f, 0);
            timeRT.anchorMax = new Vector2(0.85f, 0.4f);
            timeRT.sizeDelta = Vector2.zero;
            timeRT.offsetMin = new Vector2(5, 5);
            timeRT.offsetMax = new Vector2(-5, 0);

            TextMeshProUGUI timeText = timeObj.AddComponent<TextMeshProUGUI>();
            timeText.text = time;
            timeText.fontSize = 18;
            timeText.color = TEXT_SECONDARY;
            timeText.alignment = TextAlignmentOptions.Left;

            // Status indicator
            GameObject indicator = new GameObject("Indicator");
            indicator.transform.SetParent(item.transform, false);

            RectTransform indicatorRT = indicator.AddComponent<RectTransform>();
            indicatorRT.anchorMin = new Vector2(0.9f, 0.3f);
            indicatorRT.anchorMax = new Vector2(0.95f, 0.7f);
            indicatorRT.sizeDelta = Vector2.zero;

            Image indicatorImg = indicator.AddComponent<Image>();
            indicatorImg.color = isPositive ? new Color(0.3f, 1f, 0.5f) : new Color(1f, 0.4f, 0.4f);
        }

        #endregion

        #region History Panel

        private static void CreateHistoryPanel(Transform parent)
        {
            GameObject panel = new GameObject("HistoryPanel");
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = new Vector2(20, 20);
            rt.offsetMax = new Vector2(-20, -130);

            // Panel Title
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(panel.transform, false);

            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 1);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0.5f, 1);
            titleRT.sizeDelta = new Vector2(0, 60);
            titleRT.anchoredPosition = Vector2.zero;

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Historial de Partidas";
            titleText.fontSize = 42;
            titleText.color = TEXT_GOLD;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;

            // Stats Summary
            CreateHistoryStats(panel.transform);

            // Match History ScrollView
            GameObject scrollView = CreateScrollView(panel.transform, "HistoryScrollView",
                new Vector2(0, 0.05f), new Vector2(1, 0.75f));

            // Sample match history
            Transform content = scrollView.transform.Find("Viewport/Content");
            if (content != null)
            {
                CreateMatchHistoryItem(content, "QuickMath", "@ProGamer99", true, "+$8.50", "Hoy, 14:32", "1250 vs 980");
                CreateMatchHistoryItem(content, "Torneo Flash Tap", "3er Lugar", true, "+$15.00", "Hoy, 12:15", "8 participantes");
                CreateMatchHistoryItem(content, "MemoryPairs", "@SpeedKing", false, "-$5.00", "Ayer, 22:45", "45s vs 38s");
                CreateMatchHistoryItem(content, "Cognitive Sprint", "@MindMaster", true, "+$20.00", "Ayer, 18:20", "3-2 juegos");
                CreateMatchHistoryItem(content, "OddOneOut", "@EagleEye", false, "-$10.00", "Hace 2 dias", "8/10 vs 10/10");
                CreateMatchHistoryItem(content, "FlashTap", "@Lightning", true, "+$4.50", "Hace 2 dias", "0.21s vs 0.28s");
                CreateMatchHistoryItem(content, "Torneo QuickMath", "1er Lugar", true, "+$50.00", "Hace 3 dias", "16 participantes");
            }

            // Note: Back button removed - user will add their own prefab

            // Initially hidden
            panel.SetActive(false);
        }

        private static void CreateHistoryStats(Transform parent)
        {
            GameObject statsContainer = new GameObject("StatsContainer");
            statsContainer.transform.SetParent(parent, false);

            RectTransform rt = statsContainer.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.78f);
            rt.anchorMax = new Vector2(1, 0.95f);
            rt.sizeDelta = Vector2.zero;

            HorizontalLayoutGroup layout = statsContainer.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 15;
            layout.padding = new RectOffset(10, 10, 5, 5);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            // Wins
            CreateStatCard(statsContainer.transform, "Victorias", "24", new Color(0.3f, 1f, 0.5f));

            // Losses
            CreateStatCard(statsContainer.transform, "Derrotas", "12", new Color(1f, 0.4f, 0.4f));

            // Win Rate
            CreateStatCard(statsContainer.transform, "Win Rate", "67%", GOLD_PRIMARY);

            // Total Earned
            CreateStatCard(statsContainer.transform, "Ganado", "$156.50", CYAN_ACCENT);
        }

        private static void CreateStatCard(Transform parent, string label, string value, Color valueColor)
        {
            GameObject card = new GameObject($"Stat_{label}");
            card.transform.SetParent(parent, false);

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;

            Image bg = card.AddComponent<Image>();
            bg.color = CARD_BG;

            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = CARD_BORDER;
            outline.effectDistance = new Vector2(1, -1);

            // Value
            GameObject valueObj = new GameObject("Value");
            valueObj.transform.SetParent(card.transform, false);

            RectTransform valueRT = valueObj.AddComponent<RectTransform>();
            valueRT.anchorMin = new Vector2(0, 0.4f);
            valueRT.anchorMax = new Vector2(1, 1);
            valueRT.sizeDelta = Vector2.zero;
            valueRT.offsetMin = new Vector2(5, 0);
            valueRT.offsetMax = new Vector2(-5, -5);

            TextMeshProUGUI valueText = valueObj.AddComponent<TextMeshProUGUI>();
            valueText.text = value;
            valueText.fontSize = 32;
            valueText.color = valueColor;
            valueText.fontStyle = FontStyles.Bold;
            valueText.alignment = TextAlignmentOptions.Center;

            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(card.transform, false);

            RectTransform labelRT = labelObj.AddComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 0);
            labelRT.anchorMax = new Vector2(1, 0.4f);
            labelRT.sizeDelta = Vector2.zero;
            labelRT.offsetMin = new Vector2(5, 5);
            labelRT.offsetMax = new Vector2(-5, 0);

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.fontSize = 16;
            labelText.color = TEXT_SECONDARY;
            labelText.fontStyle = FontStyles.Bold;
            labelText.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateMatchHistoryItem(Transform parent, string gameType, string opponent, bool isWin, string amount, string dateTime, string score)
        {
            GameObject item = new GameObject($"Match_{gameType}_{opponent}".Replace(" ", "_").Replace("@", ""));
            item.transform.SetParent(parent, false);

            LayoutElement le = item.AddComponent<LayoutElement>();
            le.preferredHeight = 130;

            Image bg = item.AddComponent<Image>();
            bg.color = CARD_BG;

            Outline outline = item.AddComponent<Outline>();
            outline.effectColor = isWin ? new Color(0.3f, 1f, 0.5f, 0.4f) : new Color(1f, 0.4f, 0.4f, 0.4f);
            outline.effectDistance = new Vector2(2, -2);

            // Result indicator (left side)
            GameObject indicator = new GameObject("ResultIndicator");
            indicator.transform.SetParent(item.transform, false);

            RectTransform indicatorRT = indicator.AddComponent<RectTransform>();
            indicatorRT.anchorMin = new Vector2(0, 0);
            indicatorRT.anchorMax = new Vector2(0.02f, 1);
            indicatorRT.sizeDelta = Vector2.zero;

            Image indicatorImg = indicator.AddComponent<Image>();
            indicatorImg.color = isWin ? new Color(0.3f, 1f, 0.5f) : new Color(1f, 0.4f, 0.4f);

            // Game icon
            GameObject iconObj = new GameObject("GameIcon");
            iconObj.transform.SetParent(item.transform, false);

            RectTransform iconRT = iconObj.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.03f, 0.2f);
            iconRT.anchorMax = new Vector2(0.12f, 0.8f);
            iconRT.sizeDelta = Vector2.zero;

            Image iconBg = iconObj.AddComponent<Image>();
            iconBg.color = new Color(0.2f, 0.15f, 0.1f, 0.9f);

            GameObject iconText = new GameObject("Text");
            iconText.transform.SetParent(iconObj.transform, false);

            RectTransform iconTextRT = iconText.AddComponent<RectTransform>();
            iconTextRT.anchorMin = Vector2.zero;
            iconTextRT.anchorMax = Vector2.one;
            iconTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI iconTMP = iconText.AddComponent<TextMeshProUGUI>();
            iconTMP.text = gameType.Contains("Torneo") ? "T" : "VS";
            iconTMP.fontSize = 28;
            iconTMP.color = TEXT_GOLD;
            iconTMP.alignment = TextAlignmentOptions.Center;

            // Game type
            GameObject gameObj = new GameObject("GameType");
            gameObj.transform.SetParent(item.transform, false);

            RectTransform gameRT = gameObj.AddComponent<RectTransform>();
            gameRT.anchorMin = new Vector2(0.14f, 0.65f);
            gameRT.anchorMax = new Vector2(0.65f, 0.95f);
            gameRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI gameTMP = gameObj.AddComponent<TextMeshProUGUI>();
            gameTMP.text = gameType;
            gameTMP.fontSize = 26;
            gameTMP.color = TEXT_GOLD;
            gameTMP.fontStyle = FontStyles.Bold;
            gameTMP.alignment = TextAlignmentOptions.Left;

            // Opponent
            GameObject oppObj = new GameObject("Opponent");
            oppObj.transform.SetParent(item.transform, false);

            RectTransform oppRT = oppObj.AddComponent<RectTransform>();
            oppRT.anchorMin = new Vector2(0.14f, 0.35f);
            oppRT.anchorMax = new Vector2(0.65f, 0.65f);
            oppRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI oppTMP = oppObj.AddComponent<TextMeshProUGUI>();
            oppTMP.text = opponent.StartsWith("@") ? $"vs {opponent}" : opponent;
            oppTMP.fontSize = 22;
            oppTMP.color = CYAN_ACCENT;
            oppTMP.fontStyle = FontStyles.Bold;
            oppTMP.alignment = TextAlignmentOptions.Left;

            // Date/Time
            GameObject dateObj = new GameObject("DateTime");
            dateObj.transform.SetParent(item.transform, false);

            RectTransform dateRT = dateObj.AddComponent<RectTransform>();
            dateRT.anchorMin = new Vector2(0.14f, 0.08f);
            dateRT.anchorMax = new Vector2(0.65f, 0.35f);
            dateRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI dateTMP = dateObj.AddComponent<TextMeshProUGUI>();
            dateTMP.text = dateTime;
            dateTMP.fontSize = 18;
            dateTMP.color = TEXT_SECONDARY;
            dateTMP.alignment = TextAlignmentOptions.Left;

            // Result & Amount
            GameObject resultObj = new GameObject("Result");
            resultObj.transform.SetParent(item.transform, false);

            RectTransform resultRT = resultObj.AddComponent<RectTransform>();
            resultRT.anchorMin = new Vector2(0.66f, 0.5f);
            resultRT.anchorMax = new Vector2(0.98f, 0.95f);
            resultRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI resultTMP = resultObj.AddComponent<TextMeshProUGUI>();
            resultTMP.text = isWin ? "VICTORIA" : "DERROTA";
            resultTMP.fontSize = 22;
            resultTMP.color = isWin ? new Color(0.3f, 1f, 0.5f) : new Color(1f, 0.4f, 0.4f);
            resultTMP.fontStyle = FontStyles.Bold;
            resultTMP.alignment = TextAlignmentOptions.Right;

            // Amount
            GameObject amountObj = new GameObject("Amount");
            amountObj.transform.SetParent(item.transform, false);

            RectTransform amountRT = amountObj.AddComponent<RectTransform>();
            amountRT.anchorMin = new Vector2(0.66f, 0.25f);
            amountRT.anchorMax = new Vector2(0.98f, 0.55f);
            amountRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI amountTMP = amountObj.AddComponent<TextMeshProUGUI>();
            amountTMP.text = amount;
            amountTMP.fontSize = 28;
            amountTMP.color = isWin ? new Color(0.3f, 1f, 0.5f) : new Color(1f, 0.4f, 0.4f);
            amountTMP.fontStyle = FontStyles.Bold;
            amountTMP.alignment = TextAlignmentOptions.Right;

            // Score
            GameObject scoreObj = new GameObject("Score");
            scoreObj.transform.SetParent(item.transform, false);

            RectTransform scoreRT = scoreObj.AddComponent<RectTransform>();
            scoreRT.anchorMin = new Vector2(0.66f, 0.05f);
            scoreRT.anchorMax = new Vector2(0.98f, 0.28f);
            scoreRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI scoreTMP = scoreObj.AddComponent<TextMeshProUGUI>();
            scoreTMP.text = score;
            scoreTMP.fontSize = 16;
            scoreTMP.color = TEXT_SECONDARY;
            scoreTMP.alignment = TextAlignmentOptions.Right;
        }

        #endregion

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

        #region Reference Assigner

        private static MonoBehaviour FindCashBattleManager()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "CashBattleManager") return mb;
            return null;
        }

        private static void ResetAssignState()
        {
            assignedCount = 0; failedCount = 0; alreadySetCount = 0;
            assignResults.Clear();
        }

        private static void RunAssignAllReferences()
        {
            var manager = FindCashBattleManager();
            if (manager == null)
            {
                Debug.LogError("[CashBattleHub] CashBattleManager no encontrado!");
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            so.Update();

            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            Transform root = canvas != null ? canvas.transform : manager.transform.root;

            // Header
            AssignRef(so, "titleText", FindTextDeep(root, "TitleText"));
            AssignRef(so, "balanceText", FindTextDeep(root, "BalanceText"));
            AssignRef(so, "backButton", FindBtnDeep(root, "BackButton"));

            // Menu Cards
            Transform mainPanelT = FindDeep(root, "MainPanel");
            AssignGORef(so, "mainPanel", mainPanelT);
            AssignRef(so, "battles1v1Card", FindBtnDeep(root, "Battles1v1Card"));
            AssignRef(so, "cashTournamentsCard", FindBtnDeep(root, "CashTournamentsCard"));
            AssignRef(so, "walletCard", FindBtnDeep(root, "WalletCard"));
            AssignRef(so, "historyCard", FindBtnDeep(root, "HistoryCard"));

            // Sub-panels by type
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
            {
                if (mb.GetType().Name == "CashBattle1v1Manager")
                    AssignRef(so, "gameSelectionPanel", mb);
                if (mb.GetType().Name == "TournamentListPanel")
                    AssignRef(so, "tournamentListPanel", mb);
            }

            // Confirm Bet
            AssignGORef(so, "confirmBetPanel", FindDeep(root, "ConfirmBetPanel"));
            AssignRef(so, "confirmBetText", FindTextDeep(root, "ConfirmBetText"));
            AssignRef(so, "confirmBetButton", FindBtnDeep(root, "ConfirmBetButton"));
            AssignRef(so, "cancelBetButton", FindBtnDeep(root, "CancelBetButton"));

            // Matchmaking
            AssignGORef(so, "matchmakingPanel", FindDeep(root, "MatchmakingPanel"));
            AssignRef(so, "matchmakingStatusText", FindTextDeep(root, "MatchmakingStatusText"));
            AssignRef(so, "matchmakingTimerText", FindTextDeep(root, "MatchmakingTimerText"));
            AssignRef(so, "opponentNameText", FindTextDeep(root, "OpponentNameText"));
            AssignRef(so, "cancelMatchmakingButton", FindBtnDeep(root, "CancelMatchmakingButton"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
        }

        // FindDeep - recursive search by exact name
        private static Transform FindDeep(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                Transform result = FindDeep(child, name);
                if (result != null) return result;
            }
            return null;
        }

        private static TextMeshProUGUI FindTextDeep(Transform root, string name)
        {
            Transform t = FindDeep(root, name);
            return t != null ? t.GetComponent<TextMeshProUGUI>() : null;
        }

        private static Button FindBtnDeep(Transform root, string name)
        {
            Transform t = FindDeep(root, name);
            return t != null ? t.GetComponent<Button>() : null;
        }

        private static void AssignRef(SerializedObject so, string prop, Object value)
        {
            var p = so.FindProperty(prop);
            if (p == null) { AddAR(prop, "Propiedad no existe", false, null); failedCount++; return; }
            if (p.objectReferenceValue != null) { AddAR(prop, "Ya asignada", true, p.objectReferenceValue); alreadySetCount++; return; }
            if (value != null) { p.objectReferenceValue = value; AddAR(prop, "Asignada", true, value); assignedCount++; }
            else { AddAR(prop, "No encontrada", false, null); failedCount++; }
        }

        private static void AssignGORef(SerializedObject so, string prop, Transform t)
        {
            AssignRef(so, prop, t != null ? (Object)t.gameObject : null);
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
