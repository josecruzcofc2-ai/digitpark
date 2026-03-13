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
    /// UI Builder para la escena CashBattleHub (Menu Principal de Cash Battle)
    /// Construye: 4 tarjetas principales (Batallas 1v1, Torneos, Wallet, Historial)
    /// Para la escena CashBattle1v1, usar CashBattle1v1UIBuilder
    /// </summary>
    public class CashBattleUIBuilder : EditorWindow
    {
        private const string BACK_BUTTON_GOLD_PREFAB = "Assets/_Project/Prefabs/Common/BackButtonGold.prefab";
        private const string BACK_ICON_GOLD_PATH = "Assets/_Project/Art/Icons/Navigation/BackIconGold.png";

        // Premium Color Palette
        private static readonly Color GOLD_PRIMARY = new Color(1f, 0.84f, 0f, 1f);           // #FFD700 Gold
        private static readonly Color GOLD_DARK = new Color(0.85f, 0.65f, 0.13f, 1f);        // #D4A520 Dark Gold
        private static readonly Color GOLD_LIGHT = new Color(1f, 0.93f, 0.55f, 1f);          // #FFEE8C Light Gold
        private static readonly Color AMBER = new Color(1f, 0.75f, 0f, 1f);                  // #FFBF00 Amber

        private static readonly Color BG_DARK = new Color(0.06f, 0.05f, 0.10f, 1f);          // Very dark purple-black
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

        [MenuItem("DigitPark/Scenes/Build Scene/CashBattle/Hub", false, 180)]
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
                "- 4 tarjetas full-width verticales:\n" +
                "  * Batallas 1v1\n" +
                "  * Torneos Cash\n" +
                "  * Mi Wallet\n" +
                "  * Historial\n" +
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
            // Find or create Canvas
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Error", "No Canvas found. Open CashBattle scene first.", "OK");
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

        /// <summary>
        /// Builds the UI silently without confirmation dialogs. Used by batch builders.
        /// </summary>
        public static void BuildSilent()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("[CashBattleUIBuilder] Canvas not found - cannot build silently");
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

            Debug.Log("[CashBattleUIBuilder] UI built silently (batch mode)");
        }

        private static void RebuildBackground()
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

        #region Background

        private static void CreatePremiumBackground(Transform parent)
        {
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(parent, false);
            bg.transform.SetAsFirstSibling();

            RectTransform bgRT = bg.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;

            Image baseImg = bg.AddComponent<Image>();
            baseImg.color = BG_DARK;
            baseImg.raycastTarget = false;
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
            headerRT.anchoredPosition = new Vector2(0, -29);

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
                rect.sizeDelta = new Vector2(50, 50);

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
                // Fallback: manual back button creation
                GameObject backBtn = new GameObject("BackButton");
                backBtn.transform.SetParent(parent, false);

                RectTransform rt = backBtn.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 0.5f);
                rt.anchorMax = new Vector2(0, 0.5f);
                rt.pivot = new Vector2(0, 0.5f);
                rt.sizeDelta = new Vector2(50, 50);
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
                arrow.fontSize = FontSizes.H4;
                arrow.color = TEXT_GOLD;
                arrow.alignment = TextAlignmentOptions.Center;
                arrow.fontStyle = FontStyles.Bold;
                arrow.enableAutoSizing = true;
                arrow.fontSizeMin = FontSizes.AutoMinBody;
                arrow.fontSizeMax = FontSizes.H4;
                arrow.overflowMode = TextOverflowModes.Ellipsis;

                Debug.LogWarning("[CashBattleHub] BackButtonGold prefab not found, using fallback");
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
            title.text = "Cash Battle";
            title.fontSize = FontSizes.H4;
            title.color = TEXT_GOLD;
            title.alignment = TextAlignmentOptions.MidlineLeft;
            title.raycastTarget = false;
            title.enableAutoSizing = true;
            title.fontSizeMin = FontSizes.AutoMinTitle;
            title.fontSizeMax = FontSizes.H4;
            title.fontStyle = FontStyles.Bold;
            title.overflowMode = TextOverflowModes.Ellipsis;

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

            // LAYOUT: 5 cards full-width apilados verticalmente
            // Distribucion uniforme con spacing entre cards
            // Card 0 (top):    0.82 - 1.00 (18%)
            // Card 1:          0.62 - 0.80 (18%)
            // Card 2:          0.42 - 0.60 (18%)
            // Card 3:          0.22 - 0.40 (18%)
            // Card 4 (bottom): 0.02 - 0.20 (18%)
            // Gaps de 2% entre cards
            CreateBattles1v1Card(cardsContainer.transform);
            CreateTournamentsCashCard(cardsContainer.transform);
            CreateWalletCard(cardsContainer.transform);
            CreateCashProfileCard(cardsContainer.transform);
            CreateHistoryCard(cardsContainer.transform);

            panel.SetActive(true);
        }

        private static void CreateBattles1v1Card(Transform parent)
        {
            GameObject card = CreatePremiumCard(parent, "Battles1v1Card",
                "BATTLES 1v1",
                "Challenge other players in real time",
                "",
                new Vector2(0, 0.82f),
                new Vector2(1, 1f));

            AddCardIconImage(card.transform, "Battles1v1Icon");
        }

        private static void CreateTournamentsCashCard(Transform parent)
        {
            GameObject card = CreatePremiumCard(parent, "CashTournamentsCard",
                "CASH TOURNAMENTS",
                "Compete for big prizes",
                "",
                new Vector2(0, 0.62f),
                new Vector2(1, 0.80f));

            AddCardIconImage(card.transform, "TournamentsCashIcon");
        }

        private static void CreateWalletCard(Transform parent)
        {
            GameObject card = CreatePremiumCard(parent, "WalletCard",
                "MY WALLET",
                "Deposit and withdraw funds",
                "",
                new Vector2(0, 0.42f),
                new Vector2(1, 0.60f));

            AddCardIconImage(card.transform, "WalletCashIcon");
        }

        private static void CreateCashProfileCard(Transform parent)
        {
            GameObject card = CreatePremiumCard(parent, "CashProfileCard",
                "MY CASH PROFILE",
                "Your performance and stats",
                "",
                new Vector2(0, 0.22f),
                new Vector2(1, 0.40f));

            AddCardIconImage(card.transform, "CashProfileIcon");
        }

        private static void CreateHistoryCard(Transform parent)
        {
            GameObject card = CreatePremiumCard(parent, "HistoryCard",
                "HISTORY",
                "Your battles and results",
                "",
                new Vector2(0, 0.02f),
                new Vector2(1, 0.20f));

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
            rt.offsetMin = new Vector2(5, 3);
            rt.offsetMax = new Vector2(-5, -3);

            // Shadow (behind card content)
            GameObject shadow = new GameObject("Shadow");
            shadow.transform.SetParent(card.transform, false);
            RectTransform shadowRT = shadow.AddComponent<RectTransform>();
            shadowRT.anchorMin = Vector2.zero;
            shadowRT.anchorMax = Vector2.one;
            shadowRT.offsetMin = new Vector2(8, -12);
            shadowRT.offsetMax = Vector2.zero;
            Image shadowImg = shadow.AddComponent<Image>();
            shadowImg.color = new Color(0f, 0f, 0f, 0.45f);

            // Side (3D depth strip below card)
            GameObject side = new GameObject("Side");
            side.transform.SetParent(card.transform, false);
            RectTransform sideRT = side.AddComponent<RectTransform>();
            sideRT.anchorMin = new Vector2(0, 0);
            sideRT.anchorMax = new Vector2(1, 0);
            sideRT.offsetMin = new Vector2(0, -7);
            sideRT.offsetMax = new Vector2(0, 0);
            Image sideImg = side.AddComponent<Image>();
            sideImg.color = new Color(0.5f, 0.35f, 0.05f, 1f);
            sideImg.raycastTarget = false;

            // Card background
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

            // Layout: Icon (left 200px) | Title + Subtitle (center) | Arrow right
            int iconSize = 200;
            int iconMarginLeft = 15;
            int textMarginLeft = iconMarginLeft + iconSize + 15; // 230
            int arrowWidth = 60;

            // === TITULO ===
            GameObject titleObj = new GameObject(name + "Title");
            titleObj.transform.SetParent(card.transform, false);

            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0.5f);
            titleRT.anchorMax = new Vector2(1, 1f);
            titleRT.offsetMin = new Vector2(textMarginLeft, 2);
            titleRT.offsetMax = new Vector2(-arrowWidth - 10, -5);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = title;
            titleText.fontSize = FontSizes.H3;
            titleText.color = TEXT_GOLD;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.fontStyle = FontStyles.Bold;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = FontSizes.AutoMinBody;
            titleText.fontSizeMax = FontSizes.H3;

            // === SUBTITULO ===
            GameObject subtitleObj = new GameObject(name + "Subtitle");
            subtitleObj.transform.SetParent(card.transform, false);

            RectTransform subRT = subtitleObj.AddComponent<RectTransform>();
            subRT.anchorMin = new Vector2(0, 0);
            subRT.anchorMax = new Vector2(1, 0.5f);
            subRT.offsetMin = new Vector2(textMarginLeft, 5);
            subRT.offsetMax = new Vector2(-arrowWidth - 10, -2);

            TextMeshProUGUI subText = subtitleObj.AddComponent<TextMeshProUGUI>();
            subText.text = subtitle;
            subText.fontSize = FontSizes.Body;
            subText.fontStyle = FontStyles.Bold;
            subText.color = TEXT_SECONDARY;
            subText.alignment = TextAlignmentOptions.Left;
            subText.enableAutoSizing = true;
            subText.fontSizeMin = FontSizes.AutoMinBody;
            subText.fontSizeMax = FontSizes.Body;

            // === DETALLE (badge) ===
            if (!string.IsNullOrEmpty(detail))
            {
                GameObject detailObj = new GameObject("PriceBadge");
                detailObj.transform.SetParent(card.transform, false);

                RectTransform detailRT = detailObj.AddComponent<RectTransform>();
                detailRT.anchorMin = new Vector2(1, 0);
                detailRT.anchorMax = new Vector2(1, 0);
                detailRT.pivot = new Vector2(1, 0);
                detailRT.sizeDelta = new Vector2(130, 38);
                detailRT.anchoredPosition = new Vector2(-12, 8);

                Image detailBg = detailObj.AddComponent<Image>();
                detailBg.color = new Color(0f, 0.85f, 1f, 0.25f);

                Outline badgeOutline = detailObj.AddComponent<Outline>();
                badgeOutline.effectColor = new Color(0f, 0.9f, 1f, 0.8f);
                badgeOutline.effectDistance = new Vector2(1.5f, -1.5f);

                GameObject detailTextObj = new GameObject("DetailText");
                detailTextObj.transform.SetParent(detailObj.transform, false);

                RectTransform detailTextRT = detailTextObj.AddComponent<RectTransform>();
                detailTextRT.anchorMin = Vector2.zero;
                detailTextRT.anchorMax = Vector2.one;
                detailTextRT.sizeDelta = Vector2.zero;

                TextMeshProUGUI detailText = detailTextObj.AddComponent<TextMeshProUGUI>();
                detailText.text = detail;
                detailText.fontSize = FontSizes.Body;
                detailText.color = CYAN_ACCENT;
                detailText.alignment = TextAlignmentOptions.Center;
                detailText.fontStyle = FontStyles.Bold;
                detailText.enableAutoSizing = true;
                detailText.fontSizeMin = FontSizes.AutoMinBody;
                detailText.fontSizeMax = FontSizes.Body;
                detailText.overflowMode = TextOverflowModes.Ellipsis;
            }

            // === ARROW ">" (right side) ===
            GameObject arrowObj = new GameObject("Arrow");
            arrowObj.transform.SetParent(card.transform, false);

            RectTransform arrowRT = arrowObj.AddComponent<RectTransform>();
            arrowRT.anchorMin = new Vector2(1, 0);
            arrowRT.anchorMax = new Vector2(1, 1);
            arrowRT.pivot = new Vector2(1, 0.5f);
            arrowRT.sizeDelta = new Vector2(arrowWidth, 0);
            arrowRT.anchoredPosition = Vector2.zero;

            TextMeshProUGUI arrowText = arrowObj.AddComponent<TextMeshProUGUI>();
            arrowText.text = ">";
            arrowText.fontSize = FontSizes.H1;
            arrowText.color = TEXT_GOLD;
            arrowText.alignment = TextAlignmentOptions.Center;
            arrowText.fontStyle = FontStyles.Bold;
            arrowText.enableAutoSizing = true;
            arrowText.fontSizeMin = FontSizes.AutoMinBody;
            arrowText.fontSizeMax = FontSizes.H1;
            arrowText.overflowMode = TextOverflowModes.Ellipsis;

            return card;
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

            rt.sizeDelta = new Vector2(200, 200);
            rt.anchoredPosition = new Vector2(15, 0);

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
                text.fontSize = FontSizes.H4;
                text.color = TEXT_GOLD;
                text.alignment = TextAlignmentOptions.Center;
                text.fontStyle = FontStyles.Bold;
                text.enableAutoSizing = true;
                text.fontSizeMin = FontSizes.AutoMinBody;
                text.fontSizeMax = FontSizes.H4;
                text.overflowMode = TextOverflowModes.Ellipsis;
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
            GameObject titleObj = new GameObject("CashSelectGameTitle");
            titleObj.transform.SetParent(panel.transform, false);

            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 1);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0.5f, 1);
            titleRT.sizeDelta = new Vector2(0, 45);
            titleRT.anchoredPosition = Vector2.zero;

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Select Game";
            titleText.fontSize = FontSizes.Body;
            titleText.color = TEXT_GOLD;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = FontSizes.AutoMinBody;
            titleText.fontSizeMax = FontSizes.Body;
            titleText.overflowMode = TextOverflowModes.Ellipsis;

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
            onlineText.text = "47 players online | Pool: $2,340";
            onlineText.fontSize = FontSizes.Body;
            onlineText.color = TEXT_SECONDARY;
            onlineText.alignment = TextAlignmentOptions.Left;
            onlineText.fontStyle = FontStyles.Bold;
            onlineText.enableAutoSizing = true;
            onlineText.fontSizeMin = FontSizes.AutoMinBody;
            onlineText.fontSizeMax = FontSizes.Body;
            onlineText.overflowMode = TextOverflowModes.Ellipsis;

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
            scrollRect.scrollSensitivity = 50f;

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
            iconTMP.fontSize = FontSizes.Body;
            iconTMP.color = TEXT_GOLD;
            iconTMP.alignment = TextAlignmentOptions.Center;
            iconTMP.fontStyle = FontStyles.Bold;
            iconTMP.enableAutoSizing = true;
            iconTMP.fontSizeMin = FontSizes.AutoMinBody;
            iconTMP.fontSizeMax = FontSizes.Body;
            iconTMP.overflowMode = TextOverflowModes.Ellipsis;

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
            nameTMP.fontSize = FontSizes.Body;
            nameTMP.color = TEXT_GOLD;
            nameTMP.fontStyle = FontStyles.Bold;
            nameTMP.alignment = TextAlignmentOptions.Left;
            nameTMP.enableAutoSizing = true;
            nameTMP.fontSizeMin = FontSizes.AutoMinBody;
            nameTMP.fontSizeMax = FontSizes.Body;
            nameTMP.overflowMode = TextOverflowModes.Ellipsis;

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
            descTMP.fontSize = FontSizes.Body;
            descTMP.color = TEXT_SECONDARY;
            descTMP.fontStyle = FontStyles.Bold;
            descTMP.alignment = TextAlignmentOptions.Left;
            descTMP.enableAutoSizing = true;
            descTMP.fontSizeMin = FontSizes.AutoMinBody;
            descTMP.fontSizeMax = FontSizes.Body;
            descTMP.overflowMode = TextOverflowModes.Ellipsis;

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
            checkTMP.fontSize = FontSizes.Body;
            checkTMP.color = BG_DARK;
            checkTMP.alignment = TextAlignmentOptions.Center;
            checkTMP.fontStyle = FontStyles.Bold;
            checkTMP.enableAutoSizing = true;
            checkTMP.fontSizeMin = FontSizes.AutoMinBody;
            checkTMP.fontSizeMax = FontSizes.Body;
            checkTMP.overflowMode = TextOverflowModes.Ellipsis;

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
            checkTMP.fontSize = FontSizes.Body;
            checkTMP.color = BG_DARK; // Dark text on gold background
            checkTMP.alignment = TextAlignmentOptions.Center;
            checkTMP.fontStyle = FontStyles.Bold;
            checkTMP.enableAutoSizing = true;
            checkTMP.fontSizeMin = FontSizes.AutoMinBody;
            checkTMP.fontSizeMax = FontSizes.Body;
            checkTMP.overflowMode = TextOverflowModes.Ellipsis;

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

            // ========== TITLE: "Choose your bet" ==========
            GameObject titleObj = new GameObject("FeeTitleText");
            titleObj.transform.SetParent(feeSection.transform, false);

            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0.82f);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.sizeDelta = Vector2.zero;
            titleRT.offsetMin = new Vector2(15, 0);
            titleRT.offsetMax = new Vector2(-15, -5);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Choose your bet";
            titleText.fontSize = FontSizes.Body;
            titleText.color = TEXT_GOLD;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = FontSizes.AutoMinBody;
            titleText.fontSizeMax = FontSizes.Body;
            titleText.overflowMode = TextOverflowModes.Ellipsis;

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
            dollarText.fontSize = FontSizes.Body;
            dollarText.color = GOLD_PRIMARY;
            dollarText.fontStyle = FontStyles.Bold;
            dollarText.alignment = TextAlignmentOptions.Center;
            dollarText.enableAutoSizing = true;
            dollarText.fontSizeMin = FontSizes.AutoMinBody;
            dollarText.fontSizeMax = FontSizes.Body;
            dollarText.overflowMode = TextOverflowModes.Ellipsis;

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
            inputText.fontSize = FontSizes.Body;
            inputText.color = TEXT_PRIMARY;
            inputText.fontStyle = FontStyles.Bold;
            inputText.alignment = TextAlignmentOptions.Left;
            inputText.enableAutoSizing = true;
            inputText.fontSizeMin = FontSizes.AutoMinBody;
            inputText.fontSizeMax = FontSizes.Body;
            inputText.overflowMode = TextOverflowModes.Ellipsis;

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
            placeholderText.text = "Other amount...";
            placeholderText.fontSize = FontSizes.Body;
            placeholderText.color = TEXT_SECONDARY;
            placeholderText.fontStyle = FontStyles.Bold;
            placeholderText.alignment = TextAlignmentOptions.Left;
            placeholderText.enableAutoSizing = true;
            placeholderText.fontSizeMin = FontSizes.AutoMinBody;
            placeholderText.fontSizeMax = FontSizes.Body;
            placeholderText.overflowMode = TextOverflowModes.Ellipsis;

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
            maxText.fontSize = FontSizes.Body;
            maxText.color = TEXT_SECONDARY;
            maxText.fontStyle = FontStyles.Bold;
            maxText.alignment = TextAlignmentOptions.Center;
            maxText.enableAutoSizing = true;
            maxText.fontSizeMin = FontSizes.AutoMinBody;
            maxText.fontSizeMax = FontSizes.Body;
            maxText.overflowMode = TextOverflowModes.Ellipsis;

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

            GameObject applyTextObj = new GameObject("OkButtonText");
            applyTextObj.transform.SetParent(applyBtn.transform, false);

            RectTransform applyTextRT = applyTextObj.AddComponent<RectTransform>();
            applyTextRT.anchorMin = Vector2.zero;
            applyTextRT.anchorMax = Vector2.one;
            applyTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI applyText = applyTextObj.AddComponent<TextMeshProUGUI>();
            applyText.text = "OK";
            applyText.fontSize = FontSizes.Body;
            applyText.color = BG_DARK;
            applyText.fontStyle = FontStyles.Bold;
            applyText.alignment = TextAlignmentOptions.Center;
            applyText.enableAutoSizing = true;
            applyText.fontSizeMin = FontSizes.AutoMinBody;
            applyText.fontSizeMax = FontSizes.Body;
            applyText.overflowMode = TextOverflowModes.Ellipsis;

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
            earningsText.text = "If you win you receive: $0.00";
            earningsText.fontSize = FontSizes.Body;
            earningsText.color = new Color(0.4f, 1f, 0.6f, 1f); // Verde brillante
            earningsText.fontStyle = FontStyles.Bold;
            earningsText.alignment = TextAlignmentOptions.Left;
            earningsText.enableAutoSizing = true;
            earningsText.fontSizeMin = FontSizes.AutoMinBody;
            earningsText.fontSizeMax = FontSizes.Body;
            earningsText.overflowMode = TextOverflowModes.Ellipsis;

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
            poolText.text = "Pool: $0.00 | Your bet: $0.00 | Fee: 30%";
            poolText.fontSize = FontSizes.Body;
            poolText.color = TEXT_SECONDARY;
            poolText.fontStyle = FontStyles.Bold;
            poolText.alignment = TextAlignmentOptions.Left;
            poolText.enableAutoSizing = true;
            poolText.fontSizeMin = FontSizes.AutoMinBody;
            poolText.fontSizeMax = FontSizes.Body;
            poolText.overflowMode = TextOverflowModes.Ellipsis;

            // Coin icon for visual appeal
            GameObject coinIcon = new GameObject("CoinIcon");
            coinIcon.transform.SetParent(feedbackContainer.transform, false);

            RectTransform coinRT = coinIcon.AddComponent<RectTransform>();
            coinRT.anchorMin = new Vector2(0.85f, 0.2f);
            coinRT.anchorMax = new Vector2(0.98f, 0.8f);
            coinRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI coinText = coinIcon.AddComponent<TextMeshProUGUI>();
            coinText.text = "$";
            coinText.fontSize = FontSizes.Body;
            coinText.alignment = TextAlignmentOptions.Center;
            coinText.fontStyle = FontStyles.Bold;
            coinText.enableAutoSizing = true;
            coinText.fontSizeMin = FontSizes.AutoMinBody;
            coinText.fontSizeMax = FontSizes.Body;
            coinText.overflowMode = TextOverflowModes.Ellipsis;
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
            text.fontSizeMin = FontSizes.AutoMinBody;
            text.fontSizeMax = FontSizes.Body;
            text.overflowMode = TextOverflowModes.Ellipsis;

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
            GameObject textObj = new GameObject("FindOpponentButtonText");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0, 0.35f);
            textRT.anchorMax = new Vector2(1, 1);
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "FIND OPPONENT";
            text.fontSize = FontSizes.Body;
            text.color = BG_DARK;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.enableAutoSizing = true;
            text.fontSizeMin = FontSizes.AutoMinBody;
            text.fontSizeMax = FontSizes.Body;
            text.overflowMode = TextOverflowModes.Ellipsis;

            // Subtitle with online players count
            GameObject subtitleObj = new GameObject("OnlinePlayersText");
            subtitleObj.transform.SetParent(btnObj.transform, false);

            RectTransform subtitleRT = subtitleObj.AddComponent<RectTransform>();
            subtitleRT.anchorMin = new Vector2(0, 0);
            subtitleRT.anchorMax = new Vector2(1, 0.4f);
            subtitleRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI subtitleText = subtitleObj.AddComponent<TextMeshProUGUI>();
            subtitleText.text = "12 players searching now";
            subtitleText.fontSize = FontSizes.Body;
            subtitleText.color = new Color(0.2f, 0.15f, 0.1f, 0.9f);
            subtitleText.fontStyle = FontStyles.Bold;
            subtitleText.alignment = TextAlignmentOptions.Center;
            subtitleText.enableAutoSizing = true;
            subtitleText.fontSizeMin = FontSizes.AutoMinBody;
            subtitleText.fontSizeMax = FontSizes.Body;
            subtitleText.overflowMode = TextOverflowModes.Ellipsis;

            // Left fire icon
            GameObject leftFire = new GameObject("LeftFireIcon");
            leftFire.transform.SetParent(btnObj.transform, false);

            RectTransform leftFireRT = leftFire.AddComponent<RectTransform>();
            leftFireRT.anchorMin = new Vector2(0, 0.5f);
            leftFireRT.anchorMax = new Vector2(0, 0.5f);
            leftFireRT.pivot = new Vector2(0, 0.5f);
            leftFireRT.sizeDelta = new Vector2(50, 50);
            leftFireRT.anchoredPosition = new Vector2(15, 5);

            Image leftFireImg = leftFire.AddComponent<Image>();
            leftFireImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/CashBattle/Stats/stat_streak.png");
            leftFireImg.preserveAspect = true;
            leftFireImg.color = Color.white;

            // Right fire icon
            GameObject rightFire = new GameObject("RightFireIcon");
            rightFire.transform.SetParent(btnObj.transform, false);

            RectTransform rightFireRT = rightFire.AddComponent<RectTransform>();
            rightFireRT.anchorMin = new Vector2(1, 0.5f);
            rightFireRT.anchorMax = new Vector2(1, 0.5f);
            rightFireRT.pivot = new Vector2(1, 0.5f);
            rightFireRT.sizeDelta = new Vector2(50, 50);
            rightFireRT.anchoredPosition = new Vector2(-15, 5);

            Image rightFireImg = rightFire.AddComponent<Image>();
            rightFireImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/CashBattle/Stats/stat_streak.png");
            rightFireImg.preserveAspect = true;
            rightFireImg.color = Color.white;

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

        private static void CreateCashTournamentsManager(Transform parent)
        {
            GameObject panel = new GameObject("CashTournamentsManager");
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = new Vector2(20, 20);
            rt.offsetMax = new Vector2(-20, -130);

            // Add the CashTournamentsManager script
            System.Type panelType = System.Type.GetType("DigitPark.UI.CashBattle.CashTournamentsManager, Assembly-CSharp");
            if (panelType != null)
            {
                panel.AddComponent(panelType);
            }

            // Panel Title
            GameObject titleObj = new GameObject("CashAvailTournamentsTitle");
            titleObj.transform.SetParent(panel.transform, false);

            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 1);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0.5f, 1);
            titleRT.sizeDelta = new Vector2(0, 60);
            titleRT.anchoredPosition = Vector2.zero;

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Available Tournaments";
            titleText.fontSize = FontSizes.Body;
            titleText.color = TEXT_GOLD;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = FontSizes.AutoMinBody;
            titleText.fontSizeMax = FontSizes.Body;
            titleText.overflowMode = TextOverflowModes.Ellipsis;

            // Tournaments Container (ScrollView)
            GameObject scrollView = CreateScrollView(panel.transform, "TournamentsScrollView",
                new Vector2(0, 0.05f), new Vector2(1, 0.9f));

            // Items instantiated at runtime from TournamentCardUI prefab

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
            nameTMP.fontSize = FontSizes.Body;
            nameTMP.color = TEXT_GOLD;
            nameTMP.fontStyle = FontStyles.Bold;
            nameTMP.enableAutoSizing = true;
            nameTMP.fontSizeMin = FontSizes.AutoMinBody;
            nameTMP.fontSizeMax = FontSizes.Body;
            nameTMP.overflowMode = TextOverflowModes.Ellipsis;

            // Game type
            GameObject gameObj = new GameObject("GameType");
            gameObj.transform.SetParent(info.transform, false);

            RectTransform gameRT = gameObj.AddComponent<RectTransform>();
            gameRT.anchorMin = new Vector2(0, 0.45f);
            gameRT.anchorMax = new Vector2(1, 0.7f);
            gameRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI gameTMP = gameObj.AddComponent<TextMeshProUGUI>();
            gameTMP.text = gameType;
            gameTMP.fontSize = FontSizes.Body;
            gameTMP.color = CYAN_ACCENT;
            gameTMP.fontStyle = FontStyles.Bold;
            gameTMP.enableAutoSizing = true;
            gameTMP.fontSizeMin = FontSizes.AutoMinBody;
            gameTMP.fontSizeMax = FontSizes.Body;
            gameTMP.overflowMode = TextOverflowModes.Ellipsis;

            // Prize
            GameObject prizeObj = new GameObject("PrizePool");
            prizeObj.transform.SetParent(info.transform, false);

            RectTransform prizeRT = prizeObj.AddComponent<RectTransform>();
            prizeRT.anchorMin = new Vector2(0, 0.2f);
            prizeRT.anchorMax = new Vector2(0.5f, 0.45f);
            prizeRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI prizeTMP = prizeObj.AddComponent<TextMeshProUGUI>();
            prizeTMP.text = $"Prize: ${prizePool}";
            prizeTMP.fontSize = FontSizes.Body;
            prizeTMP.color = new Color(0.3f, 1f, 0.5f);
            prizeTMP.fontStyle = FontStyles.Bold;
            prizeTMP.enableAutoSizing = true;
            prizeTMP.fontSizeMin = FontSizes.AutoMinBody;
            prizeTMP.fontSizeMax = FontSizes.Body;
            prizeTMP.overflowMode = TextOverflowModes.Ellipsis;

            // Entry
            GameObject entryObj = new GameObject("EntryFee");
            entryObj.transform.SetParent(info.transform, false);

            RectTransform entryRT = entryObj.AddComponent<RectTransform>();
            entryRT.anchorMin = new Vector2(0.5f, 0.2f);
            entryRT.anchorMax = new Vector2(1, 0.45f);
            entryRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI entryTMP = entryObj.AddComponent<TextMeshProUGUI>();
            entryTMP.text = $"Entry: ${entryFee}";
            entryTMP.fontSize = FontSizes.Body;
            entryTMP.color = TEXT_PRIMARY;
            entryTMP.fontStyle = FontStyles.Bold;
            entryTMP.enableAutoSizing = true;
            entryTMP.fontSizeMin = FontSizes.AutoMinBody;
            entryTMP.fontSizeMax = FontSizes.Body;
            entryTMP.overflowMode = TextOverflowModes.Ellipsis;

            // Participants
            GameObject partObj = new GameObject("Participants");
            partObj.transform.SetParent(info.transform, false);

            RectTransform partRT = partObj.AddComponent<RectTransform>();
            partRT.anchorMin = new Vector2(0, 0);
            partRT.anchorMax = new Vector2(1, 0.2f);
            partRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI partTMP = partObj.AddComponent<TextMeshProUGUI>();
            partTMP.text = $"{participants} players";
            partTMP.fontSize = FontSizes.Body;
            partTMP.color = TEXT_SECONDARY;
            partTMP.fontStyle = FontStyles.Bold;
            partTMP.enableAutoSizing = true;
            partTMP.fontSizeMin = FontSizes.AutoMinBody;
            partTMP.fontSizeMax = FontSizes.Body;
            partTMP.overflowMode = TextOverflowModes.Ellipsis;

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

            GameObject joinText = new GameObject("JoinButtonText");
            joinText.transform.SetParent(joinBtn.transform, false);

            RectTransform joinTextRT = joinText.AddComponent<RectTransform>();
            joinTextRT.anchorMin = Vector2.zero;
            joinTextRT.anchorMax = Vector2.one;
            joinTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI joinTMP = joinText.AddComponent<TextMeshProUGUI>();
            joinTMP.text = "Join";
            joinTMP.fontSize = FontSizes.Body;
            joinTMP.color = BG_DARK;
            joinTMP.fontStyle = FontStyles.Bold;
            joinTMP.alignment = TextAlignmentOptions.Center;
            joinTMP.enableAutoSizing = true;
            joinTMP.fontSizeMin = FontSizes.AutoMinBody;
            joinTMP.fontSizeMax = FontSizes.Body;
            joinTMP.overflowMode = TextOverflowModes.Ellipsis;
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

            // Blocker overlay
            var blocker = new GameObject("BlockerPanel");
            blocker.transform.SetParent(panel.transform, false);
            blocker.transform.SetAsFirstSibling();
            var blockerRT = blocker.AddComponent<RectTransform>();
            blockerRT.anchorMin = Vector2.zero;
            blockerRT.anchorMax = Vector2.one;
            blockerRT.offsetMin = Vector2.zero;
            blockerRT.offsetMax = Vector2.zero;
            var blockerImg = blocker.AddComponent<Image>();
            blockerImg.color = new Color(0f, 0f, 0f, 0.7f);
            blockerImg.raycastTarget = true;

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
            textTMP.text = "Bet $0.00 on DigitRush?";
            textTMP.fontSize = FontSizes.Body;
            textTMP.color = TEXT_GOLD;
            textTMP.fontStyle = FontStyles.Bold;
            textTMP.alignment = TextAlignmentOptions.Center;
            textTMP.enableAutoSizing = true;
            textTMP.fontSizeMin = FontSizes.AutoMinBody;
            textTMP.fontSizeMax = FontSizes.Body;
            textTMP.overflowMode = TextOverflowModes.Ellipsis;

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

            GameObject confirmText = new GameObject("ConfirmBetButtonText");
            confirmText.transform.SetParent(confirmBtn.transform, false);

            RectTransform confirmTextRT = confirmText.AddComponent<RectTransform>();
            confirmTextRT.anchorMin = Vector2.zero;
            confirmTextRT.anchorMax = Vector2.one;
            confirmTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI confirmTMP = confirmText.AddComponent<TextMeshProUGUI>();
            confirmTMP.text = "Confirm";
            confirmTMP.fontSize = FontSizes.Body;
            confirmTMP.color = BG_DARK;
            confirmTMP.fontStyle = FontStyles.Bold;
            confirmTMP.alignment = TextAlignmentOptions.Center;
            confirmTMP.enableAutoSizing = true;
            confirmTMP.fontSizeMin = FontSizes.AutoMinBody;
            confirmTMP.fontSizeMax = FontSizes.Body;
            confirmTMP.overflowMode = TextOverflowModes.Ellipsis;

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

            GameObject cancelText = new GameObject("CancelBetButtonText");
            cancelText.transform.SetParent(cancelBtn.transform, false);

            RectTransform cancelTextRT = cancelText.AddComponent<RectTransform>();
            cancelTextRT.anchorMin = Vector2.zero;
            cancelTextRT.anchorMax = Vector2.one;
            cancelTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI cancelTMP = cancelText.AddComponent<TextMeshProUGUI>();
            cancelTMP.text = "Cancel";
            cancelTMP.fontSize = FontSizes.Body;
            cancelTMP.color = TEXT_PRIMARY;
            cancelTMP.fontStyle = FontStyles.Bold;
            cancelTMP.alignment = TextAlignmentOptions.Center;
            cancelTMP.enableAutoSizing = true;
            cancelTMP.fontSizeMin = FontSizes.AutoMinBody;
            cancelTMP.fontSizeMax = FontSizes.Body;
            cancelTMP.overflowMode = TextOverflowModes.Ellipsis;

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

            // Blocker overlay
            var blocker = new GameObject("BlockerPanel");
            blocker.transform.SetParent(panel.transform, false);
            blocker.transform.SetAsFirstSibling();
            var blockerRT = blocker.AddComponent<RectTransform>();
            blockerRT.anchorMin = Vector2.zero;
            blockerRT.anchorMax = Vector2.one;
            blockerRT.offsetMin = Vector2.zero;
            blockerRT.offsetMax = Vector2.zero;
            var blockerImg = blocker.AddComponent<Image>();
            blockerImg.color = new Color(0f, 0f, 0f, 0.7f);
            blockerImg.raycastTarget = true;

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

            GameObject iconText = new GameObject("SearchIconText");
            iconText.transform.SetParent(searchIcon.transform, false);

            RectTransform iconTextRT = iconText.AddComponent<RectTransform>();
            iconTextRT.anchorMin = Vector2.zero;
            iconTextRT.anchorMax = Vector2.one;
            iconTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI iconTMP = iconText.AddComponent<TextMeshProUGUI>();
            iconTMP.text = "⚔";
            iconTMP.fontSize = FontSizes.H3;
            iconTMP.color = BG_DARK;
            iconTMP.alignment = TextAlignmentOptions.Center;
            iconTMP.fontStyle = FontStyles.Bold;
            iconTMP.enableAutoSizing = true;
            iconTMP.fontSizeMin = FontSizes.AutoMinBody;
            iconTMP.fontSizeMax = FontSizes.H3;
            iconTMP.overflowMode = TextOverflowModes.Ellipsis;

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
            statusTMP.text = "Searching for opponent...";
            statusTMP.fontSize = FontSizes.Body;
            statusTMP.color = TEXT_GOLD;
            statusTMP.fontStyle = FontStyles.Bold;
            statusTMP.alignment = TextAlignmentOptions.Center;
            statusTMP.enableAutoSizing = true;
            statusTMP.fontSizeMin = FontSizes.AutoMinBody;
            statusTMP.fontSizeMax = FontSizes.Body;
            statusTMP.overflowMode = TextOverflowModes.Ellipsis;

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
            timerTMP.fontSize = FontSizes.Subtitle;
            timerTMP.color = CYAN_ACCENT;
            timerTMP.fontStyle = FontStyles.Bold;
            timerTMP.alignment = TextAlignmentOptions.Center;
            timerTMP.enableAutoSizing = true;
            timerTMP.fontSizeMin = FontSizes.AutoMinBody;
            timerTMP.fontSizeMax = FontSizes.Subtitle;
            timerTMP.overflowMode = TextOverflowModes.Ellipsis;

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
            opponentTMP.text = "Searching...";
            opponentTMP.fontSize = FontSizes.Body;
            opponentTMP.color = TEXT_PRIMARY;
            opponentTMP.fontStyle = FontStyles.Bold;
            opponentTMP.alignment = TextAlignmentOptions.Center;
            opponentTMP.enableAutoSizing = true;
            opponentTMP.fontSizeMin = FontSizes.AutoMinBody;
            opponentTMP.fontSizeMax = FontSizes.Body;
            opponentTMP.overflowMode = TextOverflowModes.Ellipsis;

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

            GameObject cancelText = new GameObject("CancelMatchmakingButtonText");
            cancelText.transform.SetParent(cancelBtn.transform, false);

            RectTransform cancelTextRT = cancelText.AddComponent<RectTransform>();
            cancelTextRT.anchorMin = Vector2.zero;
            cancelTextRT.anchorMax = Vector2.one;
            cancelTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI cancelTMP = cancelText.AddComponent<TextMeshProUGUI>();
            cancelTMP.text = "Cancel";
            cancelTMP.fontSize = FontSizes.Body;
            cancelTMP.color = TEXT_PRIMARY;
            cancelTMP.fontStyle = FontStyles.Bold;
            cancelTMP.alignment = TextAlignmentOptions.Center;
            cancelTMP.enableAutoSizing = true;
            cancelTMP.fontSizeMin = FontSizes.AutoMinBody;
            cancelTMP.fontSizeMax = FontSizes.Body;
            cancelTMP.overflowMode = TextOverflowModes.Ellipsis;

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
            GameObject titleObj = new GameObject("CashWalletTitle");
            titleObj.transform.SetParent(panel.transform, false);

            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 1);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0.5f, 1);
            titleRT.sizeDelta = new Vector2(0, 60);
            titleRT.anchoredPosition = Vector2.zero;

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "My Wallet";
            titleText.fontSize = FontSizes.Subtitle;
            titleText.color = TEXT_GOLD;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = FontSizes.AutoMinBody;
            titleText.fontSizeMax = FontSizes.Subtitle;
            titleText.overflowMode = TextOverflowModes.Ellipsis;

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
            amountText.fontSize = FontSizes.H1;
            amountText.color = GOLD_PRIMARY;
            amountText.alignment = TextAlignmentOptions.Center;
            amountText.fontStyle = FontStyles.Bold;
            amountText.enableAutoSizing = true;
            amountText.fontSizeMin = FontSizes.AutoMinBody;
            amountText.fontSizeMax = FontSizes.H1;
            amountText.overflowMode = TextOverflowModes.Ellipsis;

            // Balance Label
            GameObject labelObj = new GameObject("BalanceLabel");
            labelObj.transform.SetParent(card.transform, false);

            RectTransform labelRT = labelObj.AddComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 0.05f);
            labelRT.anchorMax = new Vector2(1, 0.4f);
            labelRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = "Available Balance";
            labelText.fontSize = FontSizes.Body;
            labelText.color = TEXT_SECONDARY;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.fontStyle = FontStyles.Bold;
            labelText.enableAutoSizing = true;
            labelText.fontSizeMin = FontSizes.AutoMinBody;
            labelText.fontSizeMax = FontSizes.Body;
            labelText.overflowMode = TextOverflowModes.Ellipsis;
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
            CreateWalletActionButton(actionsContainer.transform, "DepositButton", "DEPOSIT",
                new Color(0.2f, 0.7f, 0.3f, 1f), new Color(0.3f, 0.85f, 0.4f, 1f));

            // Withdraw Button
            CreateWalletActionButton(actionsContainer.transform, "WithdrawButton", "WITHDRAW",
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

            GameObject textObj = new GameObject(name + "Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.text = text;
            btnText.fontSize = FontSizes.Body;
            btnText.color = Color.white;
            btnText.fontStyle = FontStyles.Bold;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.enableAutoSizing = true;
            btnText.fontSizeMin = FontSizes.AutoMinBody;
            btnText.fontSizeMax = FontSizes.Body;
            btnText.overflowMode = TextOverflowModes.Ellipsis;
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
            headerText.text = "Recent Transactions";
            headerText.fontSize = FontSizes.Body;
            headerText.color = TEXT_PRIMARY;
            headerText.fontStyle = FontStyles.Bold;
            headerText.alignment = TextAlignmentOptions.Left;
            headerText.enableAutoSizing = true;
            headerText.fontSizeMin = FontSizes.AutoMinBody;
            headerText.fontSizeMax = FontSizes.Body;
            headerText.overflowMode = TextOverflowModes.Ellipsis;

            // Transactions ScrollView
            GameObject scrollView = CreateScrollView(parent, "TransactionsScrollView",
                new Vector2(0.02f, 0.08f), new Vector2(0.98f, 0.47f));

            // Items instantiated at runtime from TransactionItemUI prefab
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
            amountText.fontSize = FontSizes.Body;
            amountText.color = isPositive ? new Color(0.3f, 1f, 0.5f) : new Color(1f, 0.4f, 0.4f);
            amountText.fontStyle = FontStyles.Bold;
            amountText.alignment = TextAlignmentOptions.Left;
            amountText.enableAutoSizing = true;
            amountText.fontSizeMin = FontSizes.AutoMinBody;
            amountText.fontSizeMax = FontSizes.Body;
            amountText.overflowMode = TextOverflowModes.Ellipsis;

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
            descText.fontSize = FontSizes.Body;
            descText.color = TEXT_PRIMARY;
            descText.fontStyle = FontStyles.Bold;
            descText.alignment = TextAlignmentOptions.Left;
            descText.enableWordWrapping = true;
            descText.overflowMode = TextOverflowModes.Ellipsis;
            descText.enableAutoSizing = true;
            descText.fontSizeMin = FontSizes.AutoMinBody;
            descText.fontSizeMax = FontSizes.Body;

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
            timeText.fontSize = FontSizes.Body;
            timeText.color = TEXT_SECONDARY;
            timeText.alignment = TextAlignmentOptions.Left;
            timeText.fontStyle = FontStyles.Bold;
            timeText.enableAutoSizing = true;
            timeText.fontSizeMin = FontSizes.AutoMinBody;
            timeText.fontSizeMax = FontSizes.Body;
            timeText.overflowMode = TextOverflowModes.Ellipsis;

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
            GameObject titleObj = new GameObject("CashMatchHistoryTitle");
            titleObj.transform.SetParent(panel.transform, false);

            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 1);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0.5f, 1);
            titleRT.sizeDelta = new Vector2(0, 60);
            titleRT.anchoredPosition = Vector2.zero;

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Match History";
            titleText.fontSize = FontSizes.Subtitle;
            titleText.color = TEXT_GOLD;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = FontSizes.AutoMinBody;
            titleText.fontSizeMax = FontSizes.Subtitle;
            titleText.overflowMode = TextOverflowModes.Ellipsis;

            // Stats Summary
            CreateHistoryStats(panel.transform);

            // Match History ScrollView
            GameObject scrollView = CreateScrollView(panel.transform, "HistoryScrollView",
                new Vector2(0, 0.05f), new Vector2(1, 0.75f));

            // Items instantiated at runtime from MatchHistoryItem prefab

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
            CreateStatCard(statsContainer.transform, "Wins", "24", new Color(0.3f, 1f, 0.5f));

            // Losses
            CreateStatCard(statsContainer.transform, "Losses", "12", new Color(1f, 0.4f, 0.4f));

            // Win Rate
            CreateStatCard(statsContainer.transform, "Win Rate", "67%", GOLD_PRIMARY);

            // Total Earned
            CreateStatCard(statsContainer.transform, "Earned", "$156.50", CYAN_ACCENT);
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
            valueText.fontSize = FontSizes.Body;
            valueText.color = valueColor;
            valueText.fontStyle = FontStyles.Bold;
            valueText.alignment = TextAlignmentOptions.Center;
            valueText.enableAutoSizing = true;
            valueText.fontSizeMin = FontSizes.AutoMinBody;
            valueText.fontSizeMax = FontSizes.Body;
            valueText.overflowMode = TextOverflowModes.Ellipsis;

            // Label
            GameObject labelObj = new GameObject("StatCardLabel");
            labelObj.transform.SetParent(card.transform, false);

            RectTransform labelRT = labelObj.AddComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 0);
            labelRT.anchorMax = new Vector2(1, 0.4f);
            labelRT.sizeDelta = Vector2.zero;
            labelRT.offsetMin = new Vector2(5, 5);
            labelRT.offsetMax = new Vector2(-5, 0);

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.fontSize = FontSizes.Body;
            labelText.color = TEXT_SECONDARY;
            labelText.fontStyle = FontStyles.Bold;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.enableAutoSizing = true;
            labelText.fontSizeMin = FontSizes.AutoMinBody;
            labelText.fontSizeMax = FontSizes.Body;
            labelText.overflowMode = TextOverflowModes.Ellipsis;
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

            GameObject iconText = new GameObject("TransactionIconText");
            iconText.transform.SetParent(iconObj.transform, false);

            RectTransform iconTextRT = iconText.AddComponent<RectTransform>();
            iconTextRT.anchorMin = Vector2.zero;
            iconTextRT.anchorMax = Vector2.one;
            iconTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI iconTMP = iconText.AddComponent<TextMeshProUGUI>();
            iconTMP.text = gameType.Contains("Tournament") ? "T" : "VS";
            iconTMP.fontSize = FontSizes.Body;
            iconTMP.color = TEXT_GOLD;
            iconTMP.alignment = TextAlignmentOptions.Center;
            iconTMP.fontStyle = FontStyles.Bold;
            iconTMP.enableAutoSizing = true;
            iconTMP.fontSizeMin = FontSizes.AutoMinBody;
            iconTMP.fontSizeMax = FontSizes.Body;
            iconTMP.overflowMode = TextOverflowModes.Ellipsis;

            // Game type
            GameObject gameObj = new GameObject("GameType");
            gameObj.transform.SetParent(item.transform, false);

            RectTransform gameRT = gameObj.AddComponent<RectTransform>();
            gameRT.anchorMin = new Vector2(0.14f, 0.65f);
            gameRT.anchorMax = new Vector2(0.65f, 0.95f);
            gameRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI gameTMP = gameObj.AddComponent<TextMeshProUGUI>();
            gameTMP.text = gameType;
            gameTMP.fontSize = FontSizes.Body;
            gameTMP.color = TEXT_GOLD;
            gameTMP.fontStyle = FontStyles.Bold;
            gameTMP.alignment = TextAlignmentOptions.Left;
            gameTMP.enableAutoSizing = true;
            gameTMP.fontSizeMin = FontSizes.AutoMinBody;
            gameTMP.fontSizeMax = FontSizes.Body;
            gameTMP.overflowMode = TextOverflowModes.Ellipsis;

            // Opponent
            GameObject oppObj = new GameObject("Opponent");
            oppObj.transform.SetParent(item.transform, false);

            RectTransform oppRT = oppObj.AddComponent<RectTransform>();
            oppRT.anchorMin = new Vector2(0.14f, 0.35f);
            oppRT.anchorMax = new Vector2(0.65f, 0.65f);
            oppRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI oppTMP = oppObj.AddComponent<TextMeshProUGUI>();
            oppTMP.text = opponent.StartsWith("@") ? $"vs {opponent}" : opponent;
            oppTMP.fontSize = FontSizes.Body;
            oppTMP.color = CYAN_ACCENT;
            oppTMP.fontStyle = FontStyles.Bold;
            oppTMP.alignment = TextAlignmentOptions.Left;
            oppTMP.enableAutoSizing = true;
            oppTMP.fontSizeMin = FontSizes.AutoMinBody;
            oppTMP.fontSizeMax = FontSizes.Body;
            oppTMP.overflowMode = TextOverflowModes.Ellipsis;

            // Date/Time
            GameObject dateObj = new GameObject("DateTime");
            dateObj.transform.SetParent(item.transform, false);

            RectTransform dateRT = dateObj.AddComponent<RectTransform>();
            dateRT.anchorMin = new Vector2(0.14f, 0.08f);
            dateRT.anchorMax = new Vector2(0.65f, 0.35f);
            dateRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI dateTMP = dateObj.AddComponent<TextMeshProUGUI>();
            dateTMP.text = dateTime;
            dateTMP.fontSize = FontSizes.Body;
            dateTMP.color = TEXT_SECONDARY;
            dateTMP.alignment = TextAlignmentOptions.Left;
            dateTMP.fontStyle = FontStyles.Bold;
            dateTMP.enableAutoSizing = true;
            dateTMP.fontSizeMin = FontSizes.AutoMinBody;
            dateTMP.fontSizeMax = FontSizes.Body;
            dateTMP.overflowMode = TextOverflowModes.Ellipsis;

            // Result & Amount
            GameObject resultObj = new GameObject("Result");
            resultObj.transform.SetParent(item.transform, false);

            RectTransform resultRT = resultObj.AddComponent<RectTransform>();
            resultRT.anchorMin = new Vector2(0.66f, 0.5f);
            resultRT.anchorMax = new Vector2(0.98f, 0.95f);
            resultRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI resultTMP = resultObj.AddComponent<TextMeshProUGUI>();
            resultTMP.text = isWin ? "VICTORY" : "DEFEAT";
            resultTMP.fontSize = FontSizes.Body;
            resultTMP.color = isWin ? new Color(0.3f, 1f, 0.5f) : new Color(1f, 0.4f, 0.4f);
            resultTMP.fontStyle = FontStyles.Bold;
            resultTMP.alignment = TextAlignmentOptions.Right;
            resultTMP.enableAutoSizing = true;
            resultTMP.fontSizeMin = FontSizes.AutoMinBody;
            resultTMP.fontSizeMax = FontSizes.Body;
            resultTMP.overflowMode = TextOverflowModes.Ellipsis;

            // Amount
            GameObject amountObj = new GameObject("Amount");
            amountObj.transform.SetParent(item.transform, false);

            RectTransform amountRT = amountObj.AddComponent<RectTransform>();
            amountRT.anchorMin = new Vector2(0.66f, 0.25f);
            amountRT.anchorMax = new Vector2(0.98f, 0.55f);
            amountRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI amountTMP = amountObj.AddComponent<TextMeshProUGUI>();
            amountTMP.text = amount;
            amountTMP.fontSize = FontSizes.Body;
            amountTMP.color = isWin ? new Color(0.3f, 1f, 0.5f) : new Color(1f, 0.4f, 0.4f);
            amountTMP.fontStyle = FontStyles.Bold;
            amountTMP.alignment = TextAlignmentOptions.Right;
            amountTMP.enableAutoSizing = true;
            amountTMP.fontSizeMin = FontSizes.AutoMinBody;
            amountTMP.fontSizeMax = FontSizes.Body;
            amountTMP.overflowMode = TextOverflowModes.Ellipsis;

            // Score
            GameObject scoreObj = new GameObject("Score");
            scoreObj.transform.SetParent(item.transform, false);

            RectTransform scoreRT = scoreObj.AddComponent<RectTransform>();
            scoreRT.anchorMin = new Vector2(0.66f, 0.05f);
            scoreRT.anchorMax = new Vector2(0.98f, 0.28f);
            scoreRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI scoreTMP = scoreObj.AddComponent<TextMeshProUGUI>();
            scoreTMP.text = score;
            scoreTMP.fontSize = FontSizes.Body;
            scoreTMP.color = TEXT_SECONDARY;
            scoreTMP.alignment = TextAlignmentOptions.Right;
            scoreTMP.fontStyle = FontStyles.Bold;
            scoreTMP.enableAutoSizing = true;
            scoreTMP.fontSizeMin = FontSizes.AutoMinBody;
            scoreTMP.fontSizeMax = FontSizes.Body;
            scoreTMP.overflowMode = TextOverflowModes.Ellipsis;
        }

        #endregion

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
            AssignRef(so, "cashProfileCard", FindBtnDeep(root, "CashProfileCard"));
            AssignRef(so, "historyCard", FindBtnDeep(root, "HistoryCard"));

            // Sub-panels by type
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
            {
                if (mb.GetType().Name == "CashBattle1v1Manager")
                    AssignRef(so, "gameSelectionPanel", mb);
                if (mb.GetType().Name == "CashTournamentsManager")
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
