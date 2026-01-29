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
        private static readonly Color TAB_INACTIVE = new Color(0.15f, 0.16f, 0.2f, 1f);
        private static readonly Color TAB_ACTIVE = new Color(0.2f, 0.6f, 0.4f, 1f);

        #endregion

        #region Paths

        private static readonly string WALLET_ICONS_PATH = "Assets/_Project/Art/Icons/CashBattle/Wallet/";
        private static readonly string PREFABS_PATH = "Assets/_Project/Prefabs/CashBattle/Wallet/";

        #endregion

        // Layout constants (from top)
        private const float HEADER_HEIGHT = 80f;
        private const float BALANCE_CARD_HEIGHT = 380f;  // Aumentado para incluir botones
        private const float TABS_HEIGHT = 55f;
        private const float SECTION_SPACING = 15f;
        private const float SIDE_PADDING = 20f;

        [MenuItem("DigitPark/UI Builders/CashBattle/Cash Wallet", false, 254)]
        public static void ShowWindow()
        {
            GetWindow<WalletUIBuilder>("Cash Wallet Builder");
        }

        private void OnGUI()
        {
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
        }

        private static void BuildWalletUI()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
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

            // Back Button
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
            arrowTMP.text = "‹";
            arrowTMP.fontSize = 42;
            arrowTMP.color = TEXT_WHITE;
            arrowTMP.alignment = TextAlignmentOptions.Center;

            // Title
            GameObject title = new GameObject("Title");
            title.transform.SetParent(header.transform, false);

            RectTransform titleRT = title.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.offsetMin = new Vector2(85, 0);
            titleRT.offsetMax = new Vector2(-130, 0);

            TextMeshProUGUI titleTMP = title.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "Mi Wallet";
            titleTMP.fontSize = 32;
            titleTMP.color = TEXT_WHITE;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Left;
            titleTMP.verticalAlignment = VerticalAlignmentOptions.Middle;

            // Balance in header
            GameObject balanceHeader = new GameObject("HeaderBalance");
            balanceHeader.transform.SetParent(header.transform, false);

            RectTransform balHRT = balanceHeader.AddComponent<RectTransform>();
            balHRT.anchorMin = new Vector2(1, 0);
            balHRT.anchorMax = new Vector2(1, 1);
            balHRT.pivot = new Vector2(1, 0.5f);
            balHRT.sizeDelta = new Vector2(120, 0);
            balHRT.anchoredPosition = new Vector2(-SIDE_PADDING, 0);

            TextMeshProUGUI balHTMP = balanceHeader.AddComponent<TextMeshProUGUI>();
            balHTMP.text = "$0.00";
            balHTMP.fontSize = 28;
            balHTMP.color = GREEN;
            balHTMP.fontStyle = FontStyles.Bold;
            balHTMP.alignment = TextAlignmentOptions.Right;
            balHTMP.verticalAlignment = VerticalAlignmentOptions.Middle;
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

            // Icon
            GameObject icon = new GameObject("WalletIcon");
            icon.transform.SetParent(card.transform, false);
            RectTransform iconRT = icon.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0, 1);
            iconRT.anchorMax = new Vector2(0, 1);
            iconRT.pivot = new Vector2(0, 1);
            iconRT.sizeDelta = new Vector2(45, 45);
            iconRT.anchoredPosition = new Vector2(25, -20);
            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = GOLD;
            iconImg.preserveAspect = true;
            Sprite walletSprite = AssetDatabase.LoadAssetAtPath<Sprite>(WALLET_ICONS_PATH + "WalletIcon.png");
            if (walletSprite != null) iconImg.sprite = walletSprite;

            // Label "Balance Disponible"
            GameObject label = new GameObject("BalanceLabel");
            label.transform.SetParent(card.transform, false);
            RectTransform labelRT = label.AddComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 1);
            labelRT.anchorMax = new Vector2(1, 1);
            labelRT.pivot = new Vector2(0, 1);
            labelRT.sizeDelta = new Vector2(0, 35);
            labelRT.anchoredPosition = new Vector2(80, -25);
            labelRT.offsetMax = new Vector2(-25, -25);

            TextMeshProUGUI labelTMP = label.AddComponent<TextMeshProUGUI>();
            labelTMP.text = "Balance Disponible";
            labelTMP.fontSize = 22;
            labelTMP.color = TEXT_SECONDARY;
            labelTMP.alignment = TextAlignmentOptions.Left;

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
            labelTMP.fontSize = 20;
            labelTMP.color = TEXT_SECONDARY;
            labelTMP.alignment = TextAlignmentOptions.Left;

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
            valueTMP.fontSize = 20;
            valueTMP.color = CYAN;
            valueTMP.fontStyle = FontStyles.Bold;
            valueTMP.alignment = TextAlignmentOptions.Right;

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
            textTMP.fontSize = 26;
            textTMP.color = accent;
            textTMP.fontStyle = FontStyles.Bold;
            textTMP.alignment = TextAlignmentOptions.Center;
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
            textTMP.fontSize = 20;
            textTMP.color = active ? TEXT_WHITE : TEXT_SECONDARY;
            textTMP.fontStyle = active ? FontStyles.Bold : FontStyles.Normal;
            textTMP.alignment = TextAlignmentOptions.Center;
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
            titleTMP.text = "Historial de Transacciones";
            titleTMP.fontSize = 30;
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
            emptyTMP.fontSize = 22;
            emptyTMP.color = TEXT_SECONDARY;
            emptyTMP.alignment = TextAlignmentOptions.Center;
        }

        #endregion

        #region Connect Controller

        private static void ConnectToController(Canvas canvas, GameObject walletUI)
        {
            var controller = FindFirstObjectByType<CashBattle.CashWalletSceneController>();
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
    }
}
