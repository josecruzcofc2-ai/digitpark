using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;

namespace DigitPark.Editor
{
    /// <summary>
    /// Builder para la UI de CashWallet - Diseño estilo CashHistory.
    /// Usa posicionamiento explícito para evitar problemas de layout.
    /// </summary>
    public class WalletUIBuilder : EditorWindow
    {
        #region Colors

        private static readonly Color DARK_BG = new Color(0.06f, 0.06f, 0.08f, 1f);
        private static readonly Color CARD_BG = new Color(0.09f, 0.10f, 0.13f, 1f);
        private static readonly Color GREEN = new Color(0.2f, 0.95f, 0.4f, 1f);
        private static readonly Color GREEN_DARK = new Color(0.08f, 0.22f, 0.12f, 1f);
        private static readonly Color CYAN = new Color(0f, 0.85f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0.06f, 0.18f, 0.25f, 1f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color RED = new Color(1f, 0.35f, 0.35f, 1f);
        private static readonly Color TEXT_WHITE = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.55f, 0.55f, 0.6f, 1f);
        private static readonly Color TEXT_GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color TEXT_PRIMARY = new Color(1f, 1f, 1f, 1f);
        private static readonly Color CARD_BORDER = new Color(0.85f, 0.65f, 0.13f, 0.6f);
        private static readonly Color TAB_INACTIVE = new Color(0.15f, 0.16f, 0.2f, 1f);
        private static readonly Color TAB_ACTIVE = new Color(0.2f, 0.6f, 0.4f, 1f);

        #endregion

        #region Paths

        private static readonly string WALLET_ICONS_PATH = "Assets/_Project/Art/Icons/CashBattle/Wallet/";
        private static readonly string PREFABS_PATH = "Assets/_Project/Prefabs/CashBattle/Wallet/";
        private const string BACK_BUTTON_GOLD_PREFAB = "Assets/_Project/Prefabs/Common/BackButtonGold.prefab";

        #endregion

        // Layout constants (from top)
        private const float HEADER_HEIGHT = 120f;
        private const float BALANCE_CARD_HEIGHT = 380f;  // Aumentado para incluir botones
        private const float TABS_HEIGHT = 60f;
        private const float SECTION_SPACING = 15f;
        private const float SIDE_PADDING = 20f;

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

        [MenuItem("DigitPark/UI Builders/CashBattle/Cash Wallet", false, 254)]
        public static void ShowWindow()
        {
            GetWindow<WalletUIBuilder>("Cash Wallet Builder");
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // ========== BUILD SECTION ==========
            GUILayout.Label("Cash Wallet UI Builder", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Construye la UI de CashWallet:\n\n" +
                "• Header con back + título + balance\n" +
                "• Balance Card con botones de acción\n" +
                "• Tabs de filtro\n" +
                "• Lista de transacciones scrollable",
                MessageType.Info);

            EditorGUILayout.Space(10);

            GUI.backgroundColor = GREEN;
            if (GUILayout.Button("CONSTRUIR UI COMPLETA", GUILayout.Height(45)))
            {
                BuildWalletUI();
            }
            GUI.backgroundColor = Color.white;

            // ========== SEPARADOR ==========
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // ========== REFERENCE ASSIGNER ==========
            GUILayout.Label("Asignar Referencias", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "CashWallet")
            {
                EditorGUILayout.HelpBox($"Escena actual: {currentScene}\nAbre CashWallet primero.", MessageType.Warning);
            }

            MonoBehaviour targetController = FindWalletController();
            if (targetController != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Controller:", GUILayout.Width(70));
                EditorGUILayout.ObjectField(targetController, typeof(MonoBehaviour), true);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("CashWalletSceneController no encontrado.", MessageType.Warning);
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

        private static Canvas FindMainCanvas()
        {
            return UIBuilderCanvasHelper.FindMainCanvas();
        }

        private static void BuildWalletUI()
        {
            Canvas canvas = FindMainCanvas();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontró Canvas. Abre la escena CashWallet.", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog("Reconstruir UI?",
                "Esto reconstruirá la UI de CashWallet.\n\n¿Continuar?",
                "Sí", "Cancelar"))
            {
                return;
            }

            CleanupOldUI();

            // Cleanup
            var toDelete = new List<GameObject>();
            foreach (Transform child in canvas.transform)
            {
                if (child.name != "EventSystem")
                    toDelete.Add(child.gameObject);
            }
            foreach (var obj in toDelete)
                DestroyImmediate(obj);

            // Build UI
            GameObject root = CreateRoot(canvas.transform);
            CreateHeader(root.transform);
            CreateBalanceCard(root.transform);
            CreateFilterTabs(root.transform);
            CreateTransactionsList(root.transform);
            CreateMissingPanels(root.transform);

            ConnectToController(canvas, root);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[WalletUIBuilder] UI construida!");
        }

        #region Root

        private static GameObject CreateRoot(Transform parent)
        {
            GameObject root = new GameObject("WalletUI");
            root.transform.SetParent(parent, false);

            RectTransform rt = root.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = root.AddComponent<Image>();
            bg.color = DARK_BG;

            return root;
        }

        #endregion

        #region Header

        private static void CreateHeader(Transform parent)
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(parent, false);

            RectTransform rt = header.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.offsetMin = new Vector2(0, -HEADER_HEIGHT);
            rt.offsetMax = Vector2.zero;

            // Back Button - try prefab first
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BACK_BUTTON_GOLD_PREFAB);
            if (prefab != null)
            {
                GameObject backBtn = (GameObject)PrefabUtility.InstantiatePrefab(prefab, header.transform);
                backBtn.name = "BackButton";
                RectTransform backRT = backBtn.GetComponent<RectTransform>();
                backRT.anchorMin = new Vector2(0, 0.5f);
                backRT.anchorMax = new Vector2(0, 0.5f);
                backRT.pivot = new Vector2(0, 0.5f);
                backRT.sizeDelta = new Vector2(55, 55);
                backRT.anchoredPosition = new Vector2(SIDE_PADDING, 0);
            }
            else
            {
                Debug.LogWarning("[CashWallet] BackButtonGold prefab not found, using fallback");

                GameObject backBtn = new GameObject("BackButton");
                backBtn.transform.SetParent(header.transform, false);

                RectTransform backRT = backBtn.AddComponent<RectTransform>();
                backRT.anchorMin = new Vector2(0, 0.5f);
                backRT.anchorMax = new Vector2(0, 0.5f);
                backRT.pivot = new Vector2(0, 0.5f);
                backRT.sizeDelta = new Vector2(55, 55);
                backRT.anchoredPosition = new Vector2(SIDE_PADDING, 0);

                Image backBg = backBtn.AddComponent<Image>();
                backBg.color = CARD_BG;

                Button backButton = backBtn.AddComponent<Button>();
                backButton.targetGraphic = backBg;

                // Back arrow
                GameObject backArrow = new GameObject("Arrow");
                backArrow.transform.SetParent(backBtn.transform, false);
                RectTransform arrowRT = backArrow.AddComponent<RectTransform>();
                arrowRT.anchorMin = Vector2.zero;
                arrowRT.anchorMax = Vector2.one;
                arrowRT.offsetMin = Vector2.zero;
                arrowRT.offsetMax = Vector2.zero;

                TextMeshProUGUI arrowTMP = backArrow.AddComponent<TextMeshProUGUI>();
                arrowTMP.text = "\u2039";
                arrowTMP.fontSize = 42;
                arrowTMP.color = TEXT_WHITE;
                arrowTMP.fontStyle = FontStyles.Bold;
                arrowTMP.alignment = TextAlignmentOptions.Center;
            }

            // Title - centered, gold color
            GameObject title = new GameObject("TitleText");
            title.transform.SetParent(header.transform, false);

            RectTransform titleRT = title.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 0.5f);
            titleRT.anchorMax = new Vector2(0.5f, 0.5f);
            titleRT.sizeDelta = new Vector2(500, 80);
            titleRT.anchoredPosition = Vector2.zero;

            TextMeshProUGUI titleTMP = title.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "Mi Wallet";
            titleTMP.fontSize = 78;
            titleTMP.color = TEXT_GOLD;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;

            // Gold outline effect
            titleTMP.outlineWidth = 0.2f;
            titleTMP.outlineColor = new Color(0.5f, 0.35f, 0f, 0.6f);

            // BalanceWidget (idéntico a CashBattleHub)
            CreateBalanceWidget(header.transform);
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

        #endregion

        #region Balance Card

        private static void CreateBalanceCard(Transform parent)
        {
            float yPos = HEADER_HEIGHT + SECTION_SPACING;

            GameObject card = new GameObject("BalanceCard");
            card.transform.SetParent(parent, false);

            RectTransform rt = card.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.offsetMin = new Vector2(SIDE_PADDING, -(yPos + BALANCE_CARD_HEIGHT));
            rt.offsetMax = new Vector2(-SIDE_PADDING, -yPos);

            Image bg = card.AddComponent<Image>();
            bg.color = CARD_BG;

            // Left green border
            GameObject border = new GameObject("LeftBorder");
            border.transform.SetParent(card.transform, false);
            RectTransform borderRT = border.AddComponent<RectTransform>();
            borderRT.anchorMin = new Vector2(0, 0);
            borderRT.anchorMax = new Vector2(0, 1);
            borderRT.pivot = new Vector2(0, 0.5f);
            borderRT.sizeDelta = new Vector2(4, 0);
            borderRT.anchoredPosition = Vector2.zero;
            Image borderImg = border.AddComponent<Image>();
            borderImg.color = GREEN;

            // Label "Balance Disponible" (no icon, text fills full width)
            GameObject label = new GameObject("BalanceLabel");
            label.transform.SetParent(card.transform, false);
            RectTransform labelRT = label.AddComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 1);
            labelRT.anchorMax = new Vector2(1, 1);
            labelRT.pivot = new Vector2(0, 1);
            labelRT.sizeDelta = new Vector2(0, 50);
            labelRT.anchoredPosition = new Vector2(25, -20);
            labelRT.offsetMax = new Vector2(-25, -20);

            TextMeshProUGUI labelTMP = label.AddComponent<TextMeshProUGUI>();
            labelTMP.text = "Balance Disponible";
            labelTMP.fontSize = 62;
            labelTMP.color = TEXT_SECONDARY;
            labelTMP.fontStyle = FontStyles.Bold;
            labelTMP.alignment = TextAlignmentOptions.Left;
            labelTMP.enableAutoSizing = true;
            labelTMP.fontSizeMin = 22;
            labelTMP.fontSizeMax = 62;

            // Big Balance Amount
            GameObject amount = new GameObject("BalanceAmount");
            amount.transform.SetParent(card.transform, false);
            RectTransform amountRT = amount.AddComponent<RectTransform>();
            amountRT.anchorMin = new Vector2(0, 1);
            amountRT.anchorMax = new Vector2(1, 1);
            amountRT.pivot = new Vector2(0, 1);
            amountRT.sizeDelta = new Vector2(0, 75);
            amountRT.anchoredPosition = new Vector2(25, -60);
            amountRT.offsetMax = new Vector2(-25, -60);

            TextMeshProUGUI amountTMP = amount.AddComponent<TextMeshProUGUI>();
            amountTMP.text = "$0.00";
            amountTMP.fontSize = 58;
            amountTMP.color = GREEN;
            amountTMP.fontStyle = FontStyles.Bold;
            amountTMP.alignment = TextAlignmentOptions.Left;
            amountTMP.verticalAlignment = VerticalAlignmentOptions.Middle;

            // Weekly limit section
            CreateWeeklyLimit(card.transform);

            // Action buttons
            CreateActionButtons(card.transform);
        }

        private static void CreateWeeklyLimit(Transform card)
        {
            // Container - positioned below balance amount with proper spacing
            GameObject container = new GameObject("WeeklyLimit");
            container.transform.SetParent(card, false);
            RectTransform containerRT = container.AddComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0, 1);
            containerRT.anchorMax = new Vector2(1, 1);
            containerRT.pivot = new Vector2(0, 1);
            containerRT.sizeDelta = new Vector2(0, 55);
            containerRT.anchoredPosition = new Vector2(25, -155);
            containerRT.offsetMax = new Vector2(-25, -155);

            // Label
            GameObject label = new GameObject("Label");
            label.transform.SetParent(container.transform, false);
            RectTransform labelRT = label.AddComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 0.5f);
            labelRT.anchorMax = new Vector2(0.5f, 1);
            labelRT.offsetMin = Vector2.zero;
            labelRT.offsetMax = Vector2.zero;

            TextMeshProUGUI labelTMP = label.AddComponent<TextMeshProUGUI>();
            labelTMP.text = "Límite semanal";
            labelTMP.fontSize = 42;
            labelTMP.color = TEXT_SECONDARY;
            labelTMP.fontStyle = FontStyles.Bold;
            labelTMP.alignment = TextAlignmentOptions.Left;
            labelTMP.enableAutoSizing = true;
            labelTMP.fontSizeMin = 18;
            labelTMP.fontSizeMax = 42;

            // Value
            GameObject value = new GameObject("Value");
            value.transform.SetParent(container.transform, false);
            RectTransform valueRT = value.AddComponent<RectTransform>();
            valueRT.anchorMin = new Vector2(0.5f, 0.5f);
            valueRT.anchorMax = new Vector2(1, 1);
            valueRT.offsetMin = Vector2.zero;
            valueRT.offsetMax = Vector2.zero;

            TextMeshProUGUI valueTMP = value.AddComponent<TextMeshProUGUI>();
            valueTMP.text = "$0 / $150";
            valueTMP.fontSize = 42;
            valueTMP.color = CYAN;
            valueTMP.fontStyle = FontStyles.Bold;
            valueTMP.alignment = TextAlignmentOptions.Right;
            valueTMP.enableAutoSizing = true;
            valueTMP.fontSizeMin = 18;
            valueTMP.fontSizeMax = 42;

            // Progress bar background
            GameObject barBg = new GameObject("ProgressBarBg");
            barBg.transform.SetParent(container.transform, false);
            RectTransform barBgRT = barBg.AddComponent<RectTransform>();
            barBgRT.anchorMin = new Vector2(0, 0);
            barBgRT.anchorMax = new Vector2(1, 0);
            barBgRT.pivot = new Vector2(0, 0);
            barBgRT.sizeDelta = new Vector2(0, 10);
            barBgRT.anchoredPosition = Vector2.zero;

            Image barBgImg = barBg.AddComponent<Image>();
            barBgImg.color = new Color(0.15f, 0.15f, 0.2f, 1f);

            // Progress fill
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(barBg.transform, false);
            RectTransform fillRT = fill.AddComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = new Vector2(0f, 1f); // 0% initial
            fillRT.offsetMin = Vector2.zero;
            fillRT.offsetMax = Vector2.zero;

            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = CYAN;
        }

        private static void CreateActionButtons(Transform card)
        {
            // Container - positioned inside card with proper spacing from bottom
            GameObject container = new GameObject("ActionButtons");
            container.transform.SetParent(card, false);
            RectTransform containerRT = container.AddComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0, 0);
            containerRT.anchorMax = new Vector2(1, 0);
            containerRT.pivot = new Vector2(0.5f, 0);
            containerRT.sizeDelta = new Vector2(-50, 85);
            containerRT.anchoredPosition = new Vector2(0, 25);

            HorizontalLayoutGroup hlg = container.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            // Deposit button
            CreateActionButton(container.transform, "DepositButton", "DEPOSITAR", GREEN, GREEN_DARK, "DepositIcon.png");

            // Withdraw button
            CreateActionButton(container.transform, "WithdrawButton", "RETIRAR", CYAN, CYAN_DARK, "WithdrawIcon.png");
        }

        private static void CreateActionButton(Transform parent, string name, string text, Color accent, Color bgColor, string iconName)
        {
            GameObject btn = new GameObject(name);
            btn.transform.SetParent(parent, false);

            Image bg = btn.AddComponent<Image>();
            bg.color = bgColor;

            LayoutElement btnLE = btn.AddComponent<LayoutElement>();
            btnLE.preferredWidth = 350;

            Button button = btn.AddComponent<Button>();
            button.targetGraphic = bg;

            Outline outline = btn.AddComponent<Outline>();
            outline.effectColor = accent;
            outline.effectDistance = new Vector2(2f, -2f);

            // Content
            HorizontalLayoutGroup hlg = btn.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(15, 15, 10, 10);
            hlg.spacing = 12;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childControlWidth = false;

            // Icon
            GameObject icon = new GameObject("Icon");
            icon.transform.SetParent(btn.transform, false);
            LayoutElement iconLE = icon.AddComponent<LayoutElement>();
            iconLE.preferredWidth = 50;
            iconLE.preferredHeight = 50;

            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = accent;
            iconImg.preserveAspect = true;
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(WALLET_ICONS_PATH + iconName);
            if (sprite != null)
            {
                iconImg.sprite = sprite;
                iconImg.color = Color.white;
            }

            // Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btn.transform, false);
            LayoutElement textLE = textObj.AddComponent<LayoutElement>();
            textLE.flexibleWidth = 1;

            TextMeshProUGUI textTMP = textObj.AddComponent<TextMeshProUGUI>();
            textTMP.text = text;
            textTMP.fontSize = 52;
            textTMP.color = accent;
            textTMP.fontStyle = FontStyles.Bold;
            textTMP.alignment = TextAlignmentOptions.Center;
            textTMP.enableAutoSizing = true;
            textTMP.fontSizeMin = 24;
            textTMP.fontSizeMax = 52;
        }

        #endregion

        #region Filter Tabs

        private static void CreateFilterTabs(Transform parent)
        {
            float yPos = HEADER_HEIGHT + SECTION_SPACING + BALANCE_CARD_HEIGHT + SECTION_SPACING;

            GameObject tabs = new GameObject("FilterTabs");
            tabs.transform.SetParent(parent, false);

            RectTransform rt = tabs.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.offsetMin = new Vector2(SIDE_PADDING, -(yPos + TABS_HEIGHT));
            rt.offsetMax = new Vector2(-SIDE_PADDING, -yPos);

            HorizontalLayoutGroup hlg = tabs.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            CreateTab(tabs.transform, "TabAll", "Todas", true);
            CreateTab(tabs.transform, "TabDeposits", "Depósitos", false);
            CreateTab(tabs.transform, "TabWithdrawals", "Retiros", false);
        }

        private static void CreateTab(Transform parent, string name, string text, bool active)
        {
            GameObject tab = new GameObject(name);
            tab.transform.SetParent(parent, false);

            Image bg = tab.AddComponent<Image>();
            bg.color = active ? TAB_ACTIVE : TAB_INACTIVE;

            Button btn = tab.AddComponent<Button>();
            btn.targetGraphic = bg;

            // Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(tab.transform, false);
            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            TextMeshProUGUI textTMP = textObj.AddComponent<TextMeshProUGUI>();
            textTMP.text = text;
            textTMP.fontSize = 52;
            textTMP.color = active ? TEXT_WHITE : TEXT_SECONDARY;
            textTMP.fontStyle = FontStyles.Bold;
            textTMP.alignment = TextAlignmentOptions.Center;
            textTMP.enableAutoSizing = true;
            textTMP.fontSizeMin = 18;
            textTMP.fontSizeMax = 52;
        }

        #endregion

        #region Transactions List

        private static void CreateTransactionsList(Transform parent)
        {
            float yPos = HEADER_HEIGHT + SECTION_SPACING + BALANCE_CARD_HEIGHT + SECTION_SPACING + TABS_HEIGHT + SECTION_SPACING;

            GameObject section = new GameObject("TransactionsList");
            section.transform.SetParent(parent, false);

            RectTransform rt = section.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.offsetMin = new Vector2(SIDE_PADDING, 20);
            rt.offsetMax = new Vector2(-SIDE_PADDING, -yPos);

            // Title - Centered and bold
            GameObject title = new GameObject("SectionTitle");
            title.transform.SetParent(section.transform, false);
            RectTransform titleRT = title.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 1);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0.5f, 1);
            titleRT.sizeDelta = new Vector2(0, 50);
            titleRT.anchoredPosition = Vector2.zero;

            TextMeshProUGUI titleTMP = title.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "Historial de partidas";
            titleTMP.fontSize = 42;
            titleTMP.color = TEXT_WHITE;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;

            // ScrollView
            CreateScrollView(section.transform);
        }

        private static void CreateScrollView(Transform parent)
        {
            GameObject scrollView = new GameObject("ScrollView");
            scrollView.transform.SetParent(parent, false);

            RectTransform svRT = scrollView.AddComponent<RectTransform>();
            svRT.anchorMin = Vector2.zero;
            svRT.anchorMax = Vector2.one;
            svRT.offsetMin = new Vector2(0, 0);
            svRT.offsetMax = new Vector2(0, -60);

            Image svBg = scrollView.AddComponent<Image>();
            svBg.color = Color.clear;

            ScrollRect sr = scrollView.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Elastic;
            sr.scrollSensitivity = 30;

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);

            RectTransform vpRT = viewport.AddComponent<RectTransform>();
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.offsetMin = Vector2.zero;
            vpRT.offsetMax = Vector2.zero;

            viewport.AddComponent<RectMask2D>();
            sr.viewport = vpRT;

            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);

            RectTransform cRT = content.AddComponent<RectTransform>();
            cRT.anchorMin = new Vector2(0, 1);
            cRT.anchorMax = new Vector2(1, 1);
            cRT.pivot = new Vector2(0.5f, 1);
            cRT.sizeDelta = new Vector2(0, 0);
            cRT.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.padding = new RectOffset(0, 0, 0, 20);
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.content = cRT;

            // Empty state
            GameObject empty = new GameObject("EmptyText");
            empty.transform.SetParent(content.transform, false);

            LayoutElement emptyLE = empty.AddComponent<LayoutElement>();
            emptyLE.preferredHeight = 180;

            TextMeshProUGUI emptyTMP = empty.AddComponent<TextMeshProUGUI>();
            emptyTMP.text = "No hay transacciones\n\nTus depósitos y retiros\naparecerán aquí";
            emptyTMP.fontSize = 38;
            emptyTMP.color = TEXT_SECONDARY;
            emptyTMP.fontStyle = FontStyles.Bold;
            emptyTMP.alignment = TextAlignmentOptions.Center;
        }

        #endregion

        #region Missing Panels

        private static void CreateMissingPanels(Transform parent)
        {
            // A. BonusBalanceText - near balance display, hidden initially
            GameObject bonusText = new GameObject("BonusBalanceText");
            bonusText.transform.SetParent(parent, false);
            RectTransform bonusRT = bonusText.AddComponent<RectTransform>();
            bonusRT.anchorMin = new Vector2(0, 1);
            bonusRT.anchorMax = new Vector2(1, 1);
            bonusRT.pivot = new Vector2(0.5f, 1);
            bonusRT.sizeDelta = new Vector2(0, 40);
            bonusRT.anchoredPosition = new Vector2(0, -(HEADER_HEIGHT + SECTION_SPACING + 140));
            TextMeshProUGUI bonusTMP = bonusText.AddComponent<TextMeshProUGUI>();
            bonusTMP.text = "+$0.00 bonus";
            bonusTMP.fontSize = 24;
            bonusTMP.color = new Color(0.2f, 0.95f, 0.4f, 1f);
            bonusTMP.alignment = TextAlignmentOptions.Center;
            bonusText.SetActive(false);

            // B. HistoryTabButton - third tab button, hidden
            GameObject historyTab = new GameObject("HistoryTabButton");
            historyTab.transform.SetParent(parent, false);
            RectTransform histRT = historyTab.AddComponent<RectTransform>();
            histRT.anchorMin = new Vector2(0, 1);
            histRT.anchorMax = new Vector2(0, 1);
            histRT.pivot = new Vector2(0, 1);
            histRT.sizeDelta = new Vector2(200, 50);
            histRT.anchoredPosition = new Vector2(SIDE_PADDING, -(HEADER_HEIGHT + SECTION_SPACING + BALANCE_CARD_HEIGHT + SECTION_SPACING + TABS_HEIGHT + 10));
            Image histBg = historyTab.AddComponent<Image>();
            histBg.color = TAB_INACTIVE;
            Button histBtn = historyTab.AddComponent<Button>();
            histBtn.targetGraphic = histBg;
            GameObject histText = new GameObject("Text");
            histText.transform.SetParent(historyTab.transform, false);
            RectTransform histTextRT = histText.AddComponent<RectTransform>();
            histTextRT.anchorMin = Vector2.zero;
            histTextRT.anchorMax = Vector2.one;
            histTextRT.offsetMin = Vector2.zero;
            histTextRT.offsetMax = Vector2.zero;
            TextMeshProUGUI histTextTMP = histText.AddComponent<TextMeshProUGUI>();
            histTextTMP.text = "Historial";
            histTextTMP.fontSize = 28;
            histTextTMP.color = TEXT_SECONDARY;
            histTextTMP.fontStyle = FontStyles.Bold;
            histTextTMP.alignment = TextAlignmentOptions.Center;
            historyTab.SetActive(false);

            // C. DepositPanel (hidden overlay)
            GameObject depositPanel = new GameObject("DepositPanel");
            depositPanel.transform.SetParent(parent, false);
            RectTransform dpRT = depositPanel.AddComponent<RectTransform>();
            dpRT.anchorMin = Vector2.zero;
            dpRT.anchorMax = Vector2.one;
            dpRT.offsetMin = Vector2.zero;
            dpRT.offsetMax = Vector2.zero;
            Image dpBg = depositPanel.AddComponent<Image>();
            dpBg.color = DARK_BG;
            VerticalLayoutGroup dpVlg = depositPanel.AddComponent<VerticalLayoutGroup>();
            dpVlg.padding = new RectOffset(20, 20, 20, 20);
            dpVlg.spacing = 10;
            dpVlg.childForceExpandWidth = true;
            dpVlg.childForceExpandHeight = false;
            dpVlg.childControlWidth = true;
            dpVlg.childControlHeight = false;

            GameObject depositOptions = new GameObject("DepositOptionsContainer");
            depositOptions.transform.SetParent(depositPanel.transform, false);
            depositOptions.AddComponent<RectTransform>();

            GameObject paymentMethods = new GameObject("PaymentMethodsContainer");
            paymentMethods.transform.SetParent(depositPanel.transform, false);
            paymentMethods.AddComponent<RectTransform>();

            depositPanel.SetActive(false);

            // D. WithdrawPanel (hidden overlay)
            GameObject withdrawPanel = new GameObject("WithdrawPanel");
            withdrawPanel.transform.SetParent(parent, false);
            RectTransform wpRT = withdrawPanel.AddComponent<RectTransform>();
            wpRT.anchorMin = Vector2.zero;
            wpRT.anchorMax = Vector2.one;
            wpRT.offsetMin = Vector2.zero;
            wpRT.offsetMax = Vector2.zero;
            Image wpBg = withdrawPanel.AddComponent<Image>();
            wpBg.color = DARK_BG;
            VerticalLayoutGroup wpVlg = withdrawPanel.AddComponent<VerticalLayoutGroup>();
            wpVlg.padding = new RectOffset(20, 20, 20, 20);
            wpVlg.spacing = 10;
            wpVlg.childForceExpandWidth = true;
            wpVlg.childForceExpandHeight = false;
            wpVlg.childControlWidth = true;
            wpVlg.childControlHeight = false;

            // WithdrawAmountInput (TMP_InputField)
            GameObject inputObj = new GameObject("WithdrawAmountInput");
            inputObj.transform.SetParent(withdrawPanel.transform, false);
            RectTransform inputRT = inputObj.AddComponent<RectTransform>();
            inputRT.sizeDelta = new Vector2(400, 70);
            Image inputBg = inputObj.AddComponent<Image>();
            inputBg.color = new Color(0.15f, 0.17f, 0.22f, 1f);

            // Text Area
            GameObject textArea = new GameObject("Text Area");
            textArea.transform.SetParent(inputObj.transform, false);
            RectTransform taRT = textArea.AddComponent<RectTransform>();
            taRT.anchorMin = Vector2.zero;
            taRT.anchorMax = Vector2.one;
            taRT.offsetMin = new Vector2(10, 0);
            taRT.offsetMax = new Vector2(-10, 0);

            // Input text
            GameObject inputText = new GameObject("Text");
            inputText.transform.SetParent(textArea.transform, false);
            RectTransform itRT = inputText.AddComponent<RectTransform>();
            itRT.anchorMin = Vector2.zero;
            itRT.anchorMax = Vector2.one;
            itRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI itTMP = inputText.AddComponent<TextMeshProUGUI>();
            itTMP.fontSize = 28;

            // Placeholder
            GameObject placeholder = new GameObject("Placeholder");
            placeholder.transform.SetParent(textArea.transform, false);
            RectTransform phRT = placeholder.AddComponent<RectTransform>();
            phRT.anchorMin = Vector2.zero;
            phRT.anchorMax = Vector2.one;
            phRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI phTMP = placeholder.AddComponent<TextMeshProUGUI>();
            phTMP.text = "Ingrese monto...";
            phTMP.fontSize = 28;
            phTMP.fontStyle = FontStyles.Italic;
            phTMP.color = new Color(0.5f, 0.5f, 0.55f, 0.5f);

            TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();
            inputField.textComponent = itTMP;
            inputField.placeholder = phTMP;
            inputField.contentType = TMP_InputField.ContentType.DecimalNumber;

            // WithdrawableAmountText
            GameObject withdrawableAmt = new GameObject("WithdrawableAmountText");
            withdrawableAmt.transform.SetParent(withdrawPanel.transform, false);
            RectTransform waRT = withdrawableAmt.AddComponent<RectTransform>();
            waRT.sizeDelta = new Vector2(400, 40);
            TextMeshProUGUI waTMP = withdrawableAmt.AddComponent<TextMeshProUGUI>();
            waTMP.text = "$0.00 disponible";
            waTMP.fontSize = 24;
            waTMP.color = TEXT_WHITE;
            waTMP.alignment = TextAlignmentOptions.Left;

            // WithdrawMinText
            GameObject withdrawMin = new GameObject("WithdrawMinText");
            withdrawMin.transform.SetParent(withdrawPanel.transform, false);
            RectTransform wmRT = withdrawMin.AddComponent<RectTransform>();
            wmRT.sizeDelta = new Vector2(400, 35);
            TextMeshProUGUI wmTMP = withdrawMin.AddComponent<TextMeshProUGUI>();
            wmTMP.text = "M\u00ednimo: $10.00";
            wmTMP.fontSize = 20;
            wmTMP.color = TEXT_SECONDARY;
            wmTMP.alignment = TextAlignmentOptions.Left;

            // WithdrawFeeText
            GameObject withdrawFee = new GameObject("WithdrawFeeText");
            withdrawFee.transform.SetParent(withdrawPanel.transform, false);
            RectTransform wfRT = withdrawFee.AddComponent<RectTransform>();
            wfRT.sizeDelta = new Vector2(400, 35);
            TextMeshProUGUI wfTMP = withdrawFee.AddComponent<TextMeshProUGUI>();
            wfTMP.text = "Comisi\u00f3n: $0.00";
            wfTMP.fontSize = 20;
            wfTMP.color = TEXT_SECONDARY;
            wfTMP.alignment = TextAlignmentOptions.Left;

            withdrawPanel.SetActive(false);

            // E. TransactionHistoryPanel (hidden container)
            GameObject txHistPanel = new GameObject("TransactionHistoryPanel");
            txHistPanel.transform.SetParent(parent, false);
            RectTransform thpRT = txHistPanel.AddComponent<RectTransform>();
            thpRT.anchorMin = Vector2.zero;
            thpRT.anchorMax = Vector2.one;
            thpRT.offsetMin = Vector2.zero;
            thpRT.offsetMax = Vector2.zero;
            txHistPanel.SetActive(false);

            // F. KycRequiredPanel (hidden)
            GameObject kycPanel = new GameObject("KycRequiredPanel");
            kycPanel.transform.SetParent(parent, false);
            RectTransform kycRT = kycPanel.AddComponent<RectTransform>();
            kycRT.anchorMin = Vector2.zero;
            kycRT.anchorMax = Vector2.one;
            kycRT.offsetMin = Vector2.zero;
            kycRT.offsetMax = Vector2.zero;
            Image kycBg = kycPanel.AddComponent<Image>();
            kycBg.color = DARK_BG;

            GameObject verifyBtn = new GameObject("VerifyKycButton");
            verifyBtn.transform.SetParent(kycPanel.transform, false);
            RectTransform vkRT = verifyBtn.AddComponent<RectTransform>();
            vkRT.sizeDelta = new Vector2(350, 70);
            Image vkBg = verifyBtn.AddComponent<Image>();
            vkBg.color = GREEN_DARK;
            Button vkButton = verifyBtn.AddComponent<Button>();
            vkButton.targetGraphic = vkBg;
            Outline vkOutline = verifyBtn.AddComponent<Outline>();
            vkOutline.effectColor = GREEN;
            vkOutline.effectDistance = new Vector2(2f, -2f);

            GameObject vkText = new GameObject("Text");
            vkText.transform.SetParent(verifyBtn.transform, false);
            RectTransform vkTextRT = vkText.AddComponent<RectTransform>();
            vkTextRT.anchorMin = Vector2.zero;
            vkTextRT.anchorMax = Vector2.one;
            vkTextRT.offsetMin = Vector2.zero;
            vkTextRT.offsetMax = Vector2.zero;
            TextMeshProUGUI vkTextTMP = vkText.AddComponent<TextMeshProUGUI>();
            vkTextTMP.text = "Verificar Identidad";
            vkTextTMP.fontSize = 28;
            vkTextTMP.color = GREEN;
            vkTextTMP.fontStyle = FontStyles.Bold;
            vkTextTMP.alignment = TextAlignmentOptions.Center;

            kycPanel.SetActive(false);

            // G. Overlay panels (all hidden)
            // LoadingOverlay
            GameObject loadingOverlay = CreateOverlayPanel(parent, "LoadingOverlay");
            GameObject loadingText = new GameObject("LoadingText");
            loadingText.transform.SetParent(loadingOverlay.transform, false);
            RectTransform ltRT = loadingText.AddComponent<RectTransform>();
            ltRT.sizeDelta = new Vector2(400, 60);
            TextMeshProUGUI ltTMP = loadingText.AddComponent<TextMeshProUGUI>();
            ltTMP.text = "Procesando...";
            ltTMP.fontSize = 32;
            ltTMP.color = TEXT_WHITE;
            ltTMP.fontStyle = FontStyles.Bold;
            ltTMP.alignment = TextAlignmentOptions.Center;
            loadingOverlay.SetActive(false);

            // SuccessOverlay
            GameObject successOverlay = CreateOverlayPanel(parent, "SuccessOverlay");
            GameObject successText = new GameObject("SuccessText");
            successText.transform.SetParent(successOverlay.transform, false);
            RectTransform stRT = successText.AddComponent<RectTransform>();
            stRT.sizeDelta = new Vector2(400, 60);
            TextMeshProUGUI stTMP = successText.AddComponent<TextMeshProUGUI>();
            stTMP.text = "\u00a1Operaci\u00f3n exitosa!";
            stTMP.fontSize = 32;
            stTMP.color = GREEN;
            stTMP.fontStyle = FontStyles.Bold;
            stTMP.alignment = TextAlignmentOptions.Center;
            successOverlay.SetActive(false);

            // ErrorOverlay
            GameObject errorOverlay = CreateOverlayPanel(parent, "ErrorOverlay");
            GameObject errorText = new GameObject("ErrorMessageText");
            errorText.transform.SetParent(errorOverlay.transform, false);
            RectTransform etRT = errorText.AddComponent<RectTransform>();
            etRT.sizeDelta = new Vector2(400, 60);
            TextMeshProUGUI etTMP = errorText.AddComponent<TextMeshProUGUI>();
            etTMP.text = "";
            etTMP.fontSize = 28;
            etTMP.color = RED;
            etTMP.fontStyle = FontStyles.Bold;
            etTMP.alignment = TextAlignmentOptions.Center;
            errorOverlay.SetActive(false);

            // H. LoadMoreButton (hidden)
            GameObject loadMoreBtn = new GameObject("LoadMoreButton");
            loadMoreBtn.transform.SetParent(parent, false);
            RectTransform lmRT = loadMoreBtn.AddComponent<RectTransform>();
            lmRT.sizeDelta = new Vector2(300, 60);
            Image lmBg = loadMoreBtn.AddComponent<Image>();
            lmBg.color = TAB_INACTIVE;
            Button lmButton = loadMoreBtn.AddComponent<Button>();
            lmButton.targetGraphic = lmBg;

            GameObject lmText = new GameObject("Text");
            lmText.transform.SetParent(loadMoreBtn.transform, false);
            RectTransform lmTextRT = lmText.AddComponent<RectTransform>();
            lmTextRT.anchorMin = Vector2.zero;
            lmTextRT.anchorMax = Vector2.one;
            lmTextRT.offsetMin = Vector2.zero;
            lmTextRT.offsetMax = Vector2.zero;
            TextMeshProUGUI lmTextTMP = lmText.AddComponent<TextMeshProUGUI>();
            lmTextTMP.text = "Cargar m\u00e1s";
            lmTextTMP.fontSize = 28;
            lmTextTMP.color = TEXT_WHITE;
            lmTextTMP.fontStyle = FontStyles.Bold;
            lmTextTMP.alignment = TextAlignmentOptions.Center;
            loadMoreBtn.SetActive(false);
        }

        private static GameObject CreateOverlayPanel(Transform parent, string name)
        {
            GameObject overlay = new GameObject(name);
            overlay.transform.SetParent(parent, false);
            RectTransform rt = overlay.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            Image bg = overlay.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.85f);
            return overlay;
        }

        #endregion

        #region Connect Controller

        private static void ConnectToController(Canvas canvas, GameObject walletUI)
        {
            var controller = Object.FindFirstObjectByType<CashBattle.CashWalletSceneController>();
            if (controller == null)
            {
                Debug.LogWarning("[WalletUIBuilder] CashWalletSceneController no encontrado.");
                return;
            }

            SerializedObject so = new SerializedObject(controller);

            // Back button
            var backBtn = walletUI.transform.Find("Header/BackButton");
            if (backBtn != null)
            {
                var prop = so.FindProperty("backButton");
                if (prop != null) prop.objectReferenceValue = backBtn.GetComponent<Button>();
            }

            // Balance text
            var balanceText = walletUI.transform.Find("BalanceCard/BalanceAmount");
            if (balanceText != null)
            {
                var prop = so.FindProperty("balanceText");
                if (prop != null) prop.objectReferenceValue = balanceText.GetComponent<TextMeshProUGUI>();
            }

            // Deposit button
            var depositBtn = walletUI.transform.Find("BalanceCard/ActionButtons/DepositButton");
            if (depositBtn != null)
            {
                var prop = so.FindProperty("depositTabButton");
                if (prop != null) prop.objectReferenceValue = depositBtn.GetComponent<Button>();
            }

            // Withdraw button
            var withdrawBtn = walletUI.transform.Find("BalanceCard/ActionButtons/WithdrawButton");
            if (withdrawBtn != null)
            {
                var prop = so.FindProperty("withdrawTabButton");
                if (prop != null) prop.objectReferenceValue = withdrawBtn.GetComponent<Button>();
            }

            // Transactions container
            var txContent = walletUI.transform.Find("TransactionsList/ScrollView/Viewport/Content");
            if (txContent != null)
            {
                var prop = so.FindProperty("transactionsContainer");
                if (prop != null) prop.objectReferenceValue = txContent;
            }

            // Empty text
            var emptyText = walletUI.transform.Find("TransactionsList/ScrollView/Viewport/Content/EmptyText");
            if (emptyText != null)
            {
                var prop = so.FindProperty("emptyHistoryText");
                if (prop != null) prop.objectReferenceValue = emptyText.GetComponent<TextMeshProUGUI>();
            }

            // Prefabs
            var txPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFABS_PATH + "TransactionItemUI.prefab");
            if (txPrefab != null)
            {
                var prop = so.FindProperty("transactionItemPrefab");
                if (prop != null) prop.objectReferenceValue = txPrefab;
            }

            var depositPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFABS_PATH + "DepositOptionUI.prefab");
            if (depositPrefab != null)
            {
                var prop = so.FindProperty("depositOptionPrefab");
                if (prop != null) prop.objectReferenceValue = depositPrefab;
            }

            so.ApplyModifiedProperties();
            Debug.Log("[WalletUIBuilder] Referencias conectadas!");
        }

        #endregion

        #region Reference Assigner

        private static MonoBehaviour FindWalletController()
        {
            MonoBehaviour fallback = null;
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
            {
                if (mb.GetType().Name == "CashWalletSceneController")
                {
                    // Prefer the instance that is NOT on a Canvas object
                    if (mb.GetComponent<Canvas>() == null)
                        return mb;
                    fallback = mb;
                }
            }
            return fallback;
        }

        private static void ResetAssignState()
        {
            assignedCount = 0;
            failedCount = 0;
            alreadySetCount = 0;
            assignResults.Clear();
        }

        private static void RunAssignAllReferences()
        {
            var controller = FindWalletController();
            if (controller == null)
            {
                Debug.LogWarning("[WalletUIBuilder] CashWalletSceneController no encontrado.");
                return;
            }

            SerializedObject so = new SerializedObject(controller);
            so.Update();

            Canvas canvas = FindMainCanvas();
            Transform root = canvas != null ? canvas.transform : controller.transform.root;

            // ==================== HEADER ====================
            AssignRef(so, "backButton", FindBtnDeep(root, "BackButton"));
            AssignRef(so, "balanceText", FindTextDeep(root, "BalanceText"));
            AssignRef(so, "bonusBalanceText", FindTextDeep(root, "BonusBalanceText"));

            // ==================== TAB BUTTONS ====================
            AssignRef(so, "depositTabButton", FindBtnDeep(root, "TabDeposits"));

            AssignRef(so, "withdrawTabButton", FindBtnDeep(root, "TabWithdrawals"));

            AssignRef(so, "historyTabButton", FindBtnDeep(root, "HistoryTabButton"));

            // ==================== TAB PANELS ====================
            AssignGORef(so, "depositPanel", FindDeep(root, "DepositPanel"));
            AssignGORef(so, "withdrawPanel", FindDeep(root, "WithdrawPanel"));

            AssignGORef(so, "transactionHistoryPanel", FindDeep(root, "TransactionHistoryPanel"));

            // ==================== DEPOSIT SECTION ====================
            AssignTransformRef(so, "depositOptionsContainer", FindDeep(root, "DepositOptionsContainer"));
            AssignGORef(so, "paymentMethodsContainer", FindDeep(root, "PaymentMethodsContainer"));

            // ==================== WITHDRAW SECTION ====================
            // Special handling for TMP_InputField
            Transform withdrawInputT = FindDeep(root, "WithdrawAmountInput");
            if (withdrawInputT != null)
            {
                var input = withdrawInputT.GetComponent<TMP_InputField>();
                if (input != null) { AssignRef(so, "withdrawAmountInput", input); }
                else { AddAR("withdrawAmountInput", "InputField component missing", false, null); failedCount++; }
            }
            else { AddAR("withdrawAmountInput", "Not found", false, null); failedCount++; }
            AssignRef(so, "withdrawButton", FindBtnDeep(root, "WithdrawButton"));
            AssignRef(so, "withdrawableAmountText", FindTextDeep(root, "WithdrawableAmountText"));
            AssignRef(so, "withdrawMinText", FindTextDeep(root, "WithdrawMinText"));
            AssignRef(so, "withdrawFeeText", FindTextDeep(root, "WithdrawFeeText"));
            AssignGORef(so, "kycRequiredPanel", FindDeep(root, "KycRequiredPanel"));
            AssignRef(so, "verifyKycButton", FindBtnDeep(root, "VerifyKycButton"));

            // ==================== TRANSACTION HISTORY ====================
            // Content lives inside TransactionsList/ScrollView/Viewport/Content
            Transform txList = FindDeep(root, "TransactionsList");
            Transform txContent = txList != null ? FindDeep(txList, "Content") : FindDeep(root, "Content");
            AssignTransformRef(so, "transactionsContainer", txContent);

            AssignRef(so, "emptyHistoryText", FindTextDeep(root, "EmptyText"));

            AssignRef(so, "loadMoreButton", FindBtnDeep(root, "LoadMoreButton"));

            // ==================== OVERLAYS ====================
            AssignGORef(so, "loadingOverlay", FindDeep(root, "LoadingOverlay"));
            AssignGORef(so, "successOverlay", FindDeep(root, "SuccessOverlay"));
            AssignGORef(so, "errorOverlay", FindDeep(root, "ErrorOverlay"));
            AssignRef(so, "errorMessageText", FindTextDeep(root, "ErrorMessageText"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(controller.gameObject);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            Debug.Log($"[WalletUIBuilder] Referencias asignadas: {assignedCount} | Ya puestas: {alreadySetCount} | Fallidas: {failedCount}");
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

        #region Assigner Finders

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

        private static TMP_InputField FindInputFieldDeep(Transform root, string name)
        {
            Transform t = FindDeep(root, name);
            return t != null ? t.GetComponent<TMP_InputField>() : null;
        }

        #endregion

        #region Assigner Helpers

        private static void AssignRef(SerializedObject so, string propertyName, Object value)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { AddAR(propertyName, "Property not found", false, null); failedCount++; return; }
            if (prop.objectReferenceValue != null) { AddAR(propertyName, "Already Set", true, prop.objectReferenceValue); alreadySetCount++; return; }
            if (value != null) { prop.objectReferenceValue = value; AddAR(propertyName, "Assigned", true, value); assignedCount++; }
            else { AddAR(propertyName, "Not found", false, null); failedCount++; }
        }

        private static void AssignGORef(SerializedObject so, string propertyName, Transform t)
        {
            AssignRef(so, propertyName, t != null ? t.gameObject : null);
        }

        private static void AssignTransformRef(SerializedObject so, string propertyName, Transform t)
        {
            AssignRef(so, propertyName, t);
        }

        private static void AddAR(string fieldName, string status, bool success, Object assignedObject)
        {
            assignResults.Add(new AssignResult
            {
                fieldName = fieldName,
                status = status,
                success = success,
                assignedObject = assignedObject
            });
        }

        private void DrawAssignResults()
        {
            if (assignResults.Count == 0) return;

            EditorGUILayout.Space(10);

            int total = assignResults.Count;
            int successTotal = assignedCount + alreadySetCount;

            EditorGUILayout.BeginVertical("box");

            float successRate = (float)successTotal / total;
            GUI.color = successRate == 1f ? new Color(0.2f, 0.8f, 0.2f) :
                        successRate >= 0.7f ? new Color(1f, 0.8f, 0.2f) : new Color(1f, 0.4f, 0.4f);
            GUILayout.Label(successRate == 1f ? "TODAS LAS REFERENCIAS ASIGNADAS" : "Faltan algunas referencias", EditorStyles.boldLabel);
            GUI.color = Color.white;

            GUILayout.Label($"Asignadas: {assignedCount} | Ya puestas: {alreadySetCount} | Fallidas: {failedCount}");
            EditorGUILayout.Space(5);

            foreach (var result in assignResults)
            {
                EditorGUILayout.BeginHorizontal();
                GUI.color = result.success ? (result.status == "Already Set" ? new Color(0.5f, 0.8f, 1f) : Color.green) : Color.red;
                GUILayout.Label(result.success ? (result.status == "Already Set" ? "o" : "+") : "x", GUILayout.Width(20));
                GUI.color = Color.white;
                GUILayout.Label(result.fieldName, GUILayout.Width(200));
                GUILayout.Label(result.status, GUILayout.Width(120));
                if (result.assignedObject != null)
                    EditorGUILayout.ObjectField(result.assignedObject, typeof(Object), true, GUILayout.Width(150));
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        #endregion
    }
}
