using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.CashBattle;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para construir la UI del Wallet Panel automáticamente.
    /// Genera toda la estructura de UI para depósitos, retiros e historial.
    /// </summary>
    public class WalletUIBuilder : EditorWindow
    {
        // Colores del tema Cash Battle
        private static readonly Color DARK_BG = new Color(0.05f, 0.08f, 0.12f, 0.98f);
        private static readonly Color PANEL_BG = new Color(0.08f, 0.12f, 0.18f, 0.95f);
        private static readonly Color HEADER_BG = new Color(0.1f, 0.15f, 0.2f, 1f);
        private static readonly Color CYAN = new Color(0f, 0.83f, 1f, 1f);
        private static readonly Color GREEN = new Color(0f, 1f, 0.5f, 1f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color RED = new Color(1f, 0.4f, 0.4f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.6f, 0.65f, 1f);

        [MenuItem("DigitPark/UI Builders/CashBattle/Build Wallet Panel", false, 251)]
        public static void BuildWalletPanel()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[WalletUIBuilder] No se encontró Canvas en la escena");
                return;
            }

            // Buscar SafeArea o usar canvas
            Transform parent = canvas.transform.Find("SafeArea") ?? canvas.transform;

            // Verificar si ya existe
            Transform existing = parent.Find("WalletPanel");
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("Wallet Panel Existe",
                    "Ya existe un WalletPanel. ¿Deseas reemplazarlo?",
                    "Sí, Reemplazar", "Cancelar"))
                    return;

                Undo.DestroyObjectImmediate(existing.gameObject);
            }

            // Crear panel
            GameObject walletPanel = CreateWalletPanel(parent);

            // Registrar undo
            Undo.RegisterCreatedObjectUndo(walletPanel, "Create Wallet Panel");

            // Seleccionar
            Selection.activeGameObject = walletPanel;

            Debug.Log("[WalletUIBuilder] Wallet Panel creado exitosamente");
            EditorUtility.DisplayDialog("Wallet Panel Creado",
                "El panel de Wallet ha sido creado.\n\nRecuerda asignar los prefabs de:\n- DepositOption\n- TransactionItem",
                "OK");
        }

        [MenuItem("DigitPark/UI Builders/CashBattle/Build Deposit Option Prefab", false, 252)]
        public static void BuildDepositOptionPrefab()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[WalletUIBuilder] No se encontró Canvas en la escena");
                return;
            }

            GameObject prefab = CreateDepositOptionItem(canvas.transform);
            Selection.activeGameObject = prefab;

            Debug.Log("[WalletUIBuilder] Deposit Option prefab creado. Guárdalo como prefab en la carpeta Prefabs");
        }

        [MenuItem("DigitPark/UI Builders/CashBattle/Build Transaction Item Prefab", false, 253)]
        public static void BuildTransactionItemPrefab()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[WalletUIBuilder] No se encontró Canvas en la escena");
                return;
            }

            GameObject prefab = CreateTransactionItem(canvas.transform);
            Selection.activeGameObject = prefab;

            Debug.Log("[WalletUIBuilder] Transaction Item prefab creado. Guárdalo como prefab en la carpeta Prefabs");
        }

        // ==================== WALLET PANEL ====================

        private static GameObject CreateWalletPanel(Transform parent)
        {
            // Root panel
            GameObject root = CreatePanel("WalletPanel", parent, DARK_BG);
            RectTransform rootRT = root.GetComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.sizeDelta = Vector2.zero;

            // Canvas Group para animaciones
            CanvasGroup cg = root.AddComponent<CanvasGroup>();

            // Add WalletPanelUI component
            WalletPanelUI walletUI = root.AddComponent<WalletPanelUI>();

            // Main container con layout
            GameObject mainContainer = CreatePanel("MainContainer", root.transform, Color.clear);
            RectTransform mainRT = mainContainer.GetComponent<RectTransform>();
            mainRT.anchorMin = Vector2.zero;
            mainRT.anchorMax = Vector2.one;
            mainRT.sizeDelta = Vector2.zero;
            mainRT.offsetMin = new Vector2(20, 20);
            mainRT.offsetMax = new Vector2(-20, -20);

            VerticalLayoutGroup vlg = mainContainer.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 15;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;

            // Header
            CreateWalletHeader(mainContainer.transform);

            // Tabs
            CreateTabsPanel(mainContainer.transform);

            // Content area
            CreateContentArea(mainContainer.transform);

            // Overlays
            CreateOverlays(root.transform);

            return root;
        }

        private static void CreateWalletHeader(Transform parent)
        {
            GameObject header = CreatePanel("Header", parent, HEADER_BG);
            LayoutElement le = header.AddComponent<LayoutElement>();
            le.preferredHeight = 120;

            // Layout
            VerticalLayoutGroup vlg = header.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 15, 15);
            vlg.spacing = 10;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Top row (Title + Close)
            GameObject topRow = new GameObject("TopRow");
            topRow.transform.SetParent(header.transform, false);
            RectTransform topRT = topRow.AddComponent<RectTransform>();
            HorizontalLayoutGroup topHlg = topRow.AddComponent<HorizontalLayoutGroup>();
            topHlg.childAlignment = TextAnchor.MiddleCenter;
            topHlg.childForceExpandWidth = true;
            LayoutElement topLE = topRow.AddComponent<LayoutElement>();
            topLE.preferredHeight = 30;

            // Title
            CreateText("Title", topRow.transform, "MI WALLET", 22, CYAN, TextAlignmentOptions.Center, FontStyles.Bold);

            // Close button
            GameObject closeBtn = CreateButton("CloseButton", header.transform, "X", new Color(1f, 0.3f, 0.3f, 1f));
            RectTransform closeBtnRT = closeBtn.GetComponent<RectTransform>();
            closeBtnRT.anchorMin = new Vector2(1, 1);
            closeBtnRT.anchorMax = new Vector2(1, 1);
            closeBtnRT.pivot = new Vector2(1, 1);
            closeBtnRT.anchoredPosition = new Vector2(-10, -10);
            closeBtnRT.sizeDelta = new Vector2(40, 40);

            // Balance section
            GameObject balanceSection = new GameObject("BalanceSection");
            balanceSection.transform.SetParent(header.transform, false);
            balanceSection.AddComponent<RectTransform>();
            VerticalLayoutGroup balVlg = balanceSection.AddComponent<VerticalLayoutGroup>();
            balVlg.childAlignment = TextAnchor.MiddleCenter;
            balVlg.spacing = 5;
            LayoutElement balLE = balanceSection.AddComponent<LayoutElement>();
            balLE.preferredHeight = 60;

            CreateText("BalanceLabel", balanceSection.transform, "Balance Disponible", 12, TEXT_SECONDARY, TextAlignmentOptions.Center);

            GameObject balanceAmount = new GameObject("Balance");
            balanceAmount.transform.SetParent(balanceSection.transform, false);
            balanceAmount.AddComponent<RectTransform>();
            CreateText("Amount", balanceAmount.transform, "$0.00", 36, GREEN, TextAlignmentOptions.Center, FontStyles.Bold);
        }

        private static void CreateTabsPanel(Transform parent)
        {
            GameObject tabsPanel = CreatePanel("TabsPanel", parent, new Color(0.06f, 0.1f, 0.14f, 1f));
            LayoutElement le = tabsPanel.AddComponent<LayoutElement>();
            le.preferredHeight = 50;

            HorizontalLayoutGroup hlg = tabsPanel.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 0;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.padding = new RectOffset(5, 5, 5, 5);

            // Deposit tab
            CreateTabButton("DepositTab", tabsPanel.transform, "DEPOSITAR", true);

            // Withdraw tab
            CreateTabButton("WithdrawTab", tabsPanel.transform, "RETIRAR", false);

            // History tab
            CreateTabButton("HistoryTab", tabsPanel.transform, "HISTORIAL", false);
        }

        private static void CreateTabButton(string name, Transform parent, string text, bool active)
        {
            GameObject tab = CreateButton(name, parent, text, active ? CYAN : TEXT_SECONDARY);
            Image tabBg = tab.GetComponent<Image>();
            tabBg.color = active ? new Color(0f, 0.2f, 0.3f, 0.8f) : Color.clear;

            // Indicator
            GameObject indicator = CreatePanel("Indicator", tab.transform, active ? CYAN : Color.clear);
            RectTransform indRT = indicator.GetComponent<RectTransform>();
            indRT.anchorMin = new Vector2(0, 0);
            indRT.anchorMax = new Vector2(1, 0);
            indRT.sizeDelta = new Vector2(0, 3);
            indRT.anchoredPosition = Vector2.zero;
        }

        private static void CreateContentArea(Transform parent)
        {
            GameObject content = CreatePanel("Content", parent, Color.clear);
            LayoutElement le = content.AddComponent<LayoutElement>();
            le.flexibleHeight = 1;

            // Deposit Panel
            CreateDepositPanel(content.transform);

            // Withdraw Panel
            CreateWithdrawPanel(content.transform);

            // History Panel
            CreateHistoryPanel(content.transform);
        }

        // ==================== DEPOSIT PANEL ====================

        private static void CreateDepositPanel(Transform parent)
        {
            GameObject panel = CreatePanel("DepositPanel", parent, Color.clear);
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(15, 15, 15, 15);
            vlg.spacing = 15;
            vlg.childForceExpandHeight = false;
            vlg.childControlHeight = false;

            // Title
            CreateText("Title", panel.transform, "Selecciona un monto", 16, Color.white, TextAlignmentOptions.Left);

            // Options container (Grid)
            GameObject optionsContainer = new GameObject("OptionsContainer");
            optionsContainer.transform.SetParent(panel.transform, false);
            RectTransform ocRT = optionsContainer.AddComponent<RectTransform>();

            GridLayoutGroup glg = optionsContainer.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(150, 90);
            glg.spacing = new Vector2(15, 15);
            glg.startAxis = GridLayoutGroup.Axis.Horizontal;
            glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 2;

            LayoutElement ocLE = optionsContainer.AddComponent<LayoutElement>();
            ocLE.preferredHeight = 300;

            // Custom amount section
            GameObject customSection = new GameObject("CustomAmountSection");
            customSection.transform.SetParent(panel.transform, false);
            customSection.AddComponent<RectTransform>();
            HorizontalLayoutGroup csHlg = customSection.AddComponent<HorizontalLayoutGroup>();
            csHlg.spacing = 10;
            csHlg.childForceExpandWidth = false;
            LayoutElement csLE = customSection.AddComponent<LayoutElement>();
            csLE.preferredHeight = 50;

            // Input field
            GameObject inputField = CreateInputField("CustomAmountInput", customSection.transform, "Monto personalizado...");
            LayoutElement inputLE = inputField.AddComponent<LayoutElement>();
            inputLE.flexibleWidth = 1;

            // Button
            GameObject customBtn = CreateButton("CustomAmountButton", customSection.transform, "DEPOSITAR", CYAN);
            LayoutElement btnLE = customBtn.AddComponent<LayoutElement>();
            btnLE.preferredWidth = 120;

            // Payment methods (oculto inicialmente)
            CreatePaymentMethodsPanel(panel.transform);
        }

        private static void CreatePaymentMethodsPanel(Transform parent)
        {
            GameObject panel = CreatePanel("PaymentMethodsPanel", parent, PANEL_BG);
            LayoutElement le = panel.AddComponent<LayoutElement>();
            le.preferredHeight = 180;
            panel.SetActive(false);

            VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(15, 15, 15, 15);
            vlg.spacing = 10;
            vlg.childForceExpandHeight = false;

            CreateText("Title", panel.transform, "Método de pago", 14, TEXT_SECONDARY, TextAlignmentOptions.Left);

            // Buttons grid
            GameObject grid = new GameObject("ButtonsGrid");
            grid.transform.SetParent(panel.transform, false);
            grid.AddComponent<RectTransform>();
            GridLayoutGroup glg = grid.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(140, 50);
            glg.spacing = new Vector2(10, 10);
            glg.constraintCount = 2;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            LayoutElement gridLE = grid.AddComponent<LayoutElement>();
            gridLE.preferredHeight = 120;

            CreateButton("CreditCardButton", grid.transform, "Tarjeta", new Color(0.2f, 0.4f, 0.8f, 1f));
            CreateButton("PayPalButton", grid.transform, "PayPal", new Color(0f, 0.3f, 0.6f, 1f));
            CreateButton("ApplePayButton", grid.transform, "Apple Pay", new Color(0.1f, 0.1f, 0.1f, 1f));
            CreateButton("GooglePayButton", grid.transform, "Google Pay", new Color(0.2f, 0.6f, 0.3f, 1f));
        }

        // ==================== WITHDRAW PANEL ====================

        private static void CreateWithdrawPanel(Transform parent)
        {
            GameObject panel = CreatePanel("WithdrawPanel", parent, Color.clear);
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            panel.SetActive(false);

            VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(15, 15, 15, 15);
            vlg.spacing = 20;
            vlg.childForceExpandHeight = false;
            vlg.childControlHeight = false;

            // Available balance
            GameObject balSection = new GameObject("BalanceSection");
            balSection.transform.SetParent(panel.transform, false);
            balSection.AddComponent<RectTransform>();
            VerticalLayoutGroup balVlg = balSection.AddComponent<VerticalLayoutGroup>();
            balVlg.childAlignment = TextAnchor.MiddleCenter;
            balVlg.spacing = 5;
            LayoutElement balLE = balSection.AddComponent<LayoutElement>();
            balLE.preferredHeight = 70;

            CreateText("Label", balSection.transform, "Disponible para retiro", 14, TEXT_SECONDARY, TextAlignmentOptions.Center);
            CreateText("AvailableBalance", balSection.transform, "$0.00", 28, GREEN, TextAlignmentOptions.Center, FontStyles.Bold);

            // Amount input
            GameObject inputSection = new GameObject("InputSection");
            inputSection.transform.SetParent(panel.transform, false);
            inputSection.AddComponent<RectTransform>();
            VerticalLayoutGroup isVlg = inputSection.AddComponent<VerticalLayoutGroup>();
            isVlg.spacing = 10;
            LayoutElement isLE = inputSection.AddComponent<LayoutElement>();
            isLE.preferredHeight = 80;

            CreateText("Label", inputSection.transform, "Monto a retirar", 14, Color.white, TextAlignmentOptions.Left);
            CreateInputField("AmountInput", inputSection.transform, "$0.00");

            // Info text
            CreateText("WithdrawInfo", panel.transform, "Mínimo: $10.00 | Máximo: $500.00\nProcesamiento: 3-5 días hábiles", 12, TEXT_SECONDARY, TextAlignmentOptions.Center);

            // Withdraw button
            GameObject withdrawBtn = CreateButton("WithdrawButton", panel.transform, "SOLICITAR RETIRO", GREEN);
            LayoutElement wbLE = withdrawBtn.AddComponent<LayoutElement>();
            wbLE.preferredHeight = 50;

            // KYC Required panel
            CreateKYCRequiredPanel(panel.transform);
        }

        private static void CreateKYCRequiredPanel(Transform parent)
        {
            GameObject panel = CreatePanel("KYCRequiredPanel", parent, new Color(0.3f, 0.15f, 0.1f, 0.95f));
            LayoutElement le = panel.AddComponent<LayoutElement>();
            le.preferredHeight = 120;
            panel.SetActive(false);

            VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 15, 15);
            vlg.spacing = 10;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childForceExpandHeight = false;

            CreateText("Title", panel.transform, "Verificación requerida", 16, GOLD, TextAlignmentOptions.Center, FontStyles.Bold);
            CreateText("Description", panel.transform, "Para realizar retiros necesitas verificar tu identidad", 12, Color.white, TextAlignmentOptions.Center);

            GameObject verifyBtn = CreateButton("VerifyButton", panel.transform, "VERIFICAR AHORA", GOLD);
            LayoutElement btnLE = verifyBtn.AddComponent<LayoutElement>();
            btnLE.preferredHeight = 40;
        }

        // ==================== HISTORY PANEL ====================

        private static void CreateHistoryPanel(Transform parent)
        {
            GameObject panel = CreatePanel("HistoryPanel", parent, Color.clear);
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            panel.SetActive(false);

            VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.spacing = 5;

            // Title
            CreateText("Title", panel.transform, "Historial de transacciones", 14, TEXT_SECONDARY, TextAlignmentOptions.Left);

            // Scroll view
            GameObject scrollView = CreateScrollView("ScrollView", panel.transform);
            LayoutElement svLE = scrollView.AddComponent<LayoutElement>();
            svLE.flexibleHeight = 1;

            // No transactions text
            Transform content = scrollView.transform.Find("Viewport/Content");
            CreateText("NoTransactionsText", content, "No hay transacciones", 14, TEXT_SECONDARY, TextAlignmentOptions.Center);
        }

        // ==================== OVERLAYS ====================

        private static void CreateOverlays(Transform parent)
        {
            // Loading overlay
            CreateStatusOverlay("LoadingOverlay", parent, "Procesando...", CYAN, true);

            // Success overlay
            CreateStatusOverlay("SuccessOverlay", parent, "Operación exitosa", GREEN, false);

            // Error overlay
            CreateStatusOverlay("ErrorOverlay", parent, "Error", RED, false);
        }

        private static void CreateStatusOverlay(string name, Transform parent, string defaultText, Color color, bool showSpinner)
        {
            GameObject overlay = CreatePanel(name, parent, new Color(0, 0, 0, 0.8f));
            RectTransform rt = overlay.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            overlay.SetActive(false);

            // Content container
            GameObject content = new GameObject("Content");
            content.transform.SetParent(overlay.transform, false);
            RectTransform contentRT = content.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0.5f, 0.5f);
            contentRT.anchorMax = new Vector2(0.5f, 0.5f);
            contentRT.sizeDelta = new Vector2(200, 100);

            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.spacing = 15;

            // Spinner placeholder (si showSpinner)
            if (showSpinner)
            {
                GameObject spinner = CreatePanel("Spinner", content.transform, color);
                RectTransform spinRT = spinner.GetComponent<RectTransform>();
                LayoutElement spinLE = spinner.AddComponent<LayoutElement>();
                spinLE.preferredWidth = 40;
                spinLE.preferredHeight = 40;
            }

            // Text
            CreateText("Text", content.transform, defaultText, 18, color, TextAlignmentOptions.Center);
        }

        // ==================== PREFABS ====================

        private static GameObject CreateDepositOptionItem(Transform parent)
        {
            GameObject item = CreatePanel("DepositOptionItem", parent, PANEL_BG);
            RectTransform rt = item.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(150, 90);

            // Add component
            DepositOptionUI optionUI = item.AddComponent<DepositOptionUI>();

            // Button
            Button btn = item.AddComponent<Button>();
            btn.targetGraphic = item.GetComponent<Image>();

            // Layout
            VerticalLayoutGroup vlg = item.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.spacing = 5;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;

            // Amount text
            CreateText("AmountText", item.transform, "$25.00", 20, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);

            // Bonus text
            GameObject bonusText = CreateText("BonusText", item.transform, "+$3.00 BONUS", 12, GREEN, TextAlignmentOptions.Center);
            bonusText.SetActive(false);

            // Popular badge
            GameObject badge = CreatePanel("PopularBadge", item.transform, GOLD);
            RectTransform badgeRT = badge.GetComponent<RectTransform>();
            badgeRT.anchorMin = new Vector2(0.5f, 1);
            badgeRT.anchorMax = new Vector2(0.5f, 1);
            badgeRT.pivot = new Vector2(0.5f, 0.5f);
            badgeRT.anchoredPosition = new Vector2(0, 5);
            badgeRT.sizeDelta = new Vector2(60, 18);

            CreateText("BadgeText", badge.transform, "POPULAR", 10, Color.black, TextAlignmentOptions.Center, FontStyles.Bold);
            badge.SetActive(false);

            // Outline
            Outline outline = item.AddComponent<Outline>();
            outline.effectColor = GOLD;
            outline.effectDistance = new Vector2(2, 2);
            outline.enabled = false;

            return item;
        }

        private static GameObject CreateTransactionItem(Transform parent)
        {
            GameObject item = CreatePanel("TransactionItem", parent, PANEL_BG);
            RectTransform rt = item.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 60);

            // Add component
            TransactionItemUI transUI = item.AddComponent<TransactionItemUI>();

            // Layout
            HorizontalLayoutGroup hlg = item.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(15, 15, 10, 10);
            hlg.spacing = 12;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth = false;

            LayoutElement itemLE = item.AddComponent<LayoutElement>();
            itemLE.preferredHeight = 60;
            itemLE.flexibleWidth = 1;

            // Type icon
            GameObject iconObj = CreatePanel("TypeIcon", item.transform, CYAN);
            RectTransform iconRT = iconObj.GetComponent<RectTransform>();
            LayoutElement iconLE = iconObj.AddComponent<LayoutElement>();
            iconLE.preferredWidth = 36;
            iconLE.preferredHeight = 36;

            // Info section
            GameObject infoSection = new GameObject("InfoSection");
            infoSection.transform.SetParent(item.transform, false);
            infoSection.AddComponent<RectTransform>();
            VerticalLayoutGroup infoVlg = infoSection.AddComponent<VerticalLayoutGroup>();
            infoVlg.spacing = 2;
            infoVlg.childForceExpandHeight = false;
            LayoutElement infoLE = infoSection.AddComponent<LayoutElement>();
            infoLE.flexibleWidth = 1;

            CreateText("Description", infoSection.transform, "Depósito vía Tarjeta", 14, Color.white, TextAlignmentOptions.Left);
            CreateText("Date", infoSection.transform, "Hace 2h", 11, TEXT_SECONDARY, TextAlignmentOptions.Left);

            // Amount section
            GameObject amountSection = new GameObject("AmountSection");
            amountSection.transform.SetParent(item.transform, false);
            amountSection.AddComponent<RectTransform>();
            VerticalLayoutGroup amtVlg = amountSection.AddComponent<VerticalLayoutGroup>();
            amtVlg.childAlignment = TextAnchor.MiddleRight;
            amtVlg.spacing = 2;
            LayoutElement amtLE = amountSection.AddComponent<LayoutElement>();
            amtLE.preferredWidth = 80;

            CreateText("Amount", amountSection.transform, "+$25.00", 16, GREEN, TextAlignmentOptions.Right, FontStyles.Bold);
            GameObject statusText = CreateText("Status", amountSection.transform, "", 10, GOLD, TextAlignmentOptions.Right);
            statusText.SetActive(false);

            return item;
        }

        // ==================== UI HELPERS ====================

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image img = panel.AddComponent<Image>();
            img.color = color;

            return panel;
        }

        private static GameObject CreateText(string name, Transform parent, string text, int fontSize, Color color,
            TextAlignmentOptions alignment, FontStyles style = FontStyles.Normal)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);

            RectTransform rt = textObj.AddComponent<RectTransform>();

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.fontStyle = style;

            LayoutElement le = textObj.AddComponent<LayoutElement>();
            le.preferredHeight = fontSize + 8;

            return textObj;
        }

        private static GameObject CreateButton(string name, Transform parent, string text, Color bgColor)
        {
            GameObject btnObj = CreatePanel(name, parent, bgColor);

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnObj.GetComponent<Image>();

            ColorBlock colors = btn.colors;
            colors.highlightedColor = bgColor * 1.2f;
            colors.pressedColor = bgColor * 0.8f;
            btn.colors = colors;

            // Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 14;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;

            return btnObj;
        }

        private static GameObject CreateInputField(string name, Transform parent, string placeholder)
        {
            GameObject inputObj = CreatePanel(name, parent, new Color(0.1f, 0.12f, 0.15f, 1f));
            RectTransform rt = inputObj.GetComponent<RectTransform>();

            // Add outline
            Outline outline = inputObj.AddComponent<Outline>();
            outline.effectColor = CYAN * 0.5f;
            outline.effectDistance = new Vector2(1, 1);

            // Text Area
            GameObject textArea = new GameObject("Text Area");
            textArea.transform.SetParent(inputObj.transform, false);
            RectTransform taRT = textArea.AddComponent<RectTransform>();
            taRT.anchorMin = Vector2.zero;
            taRT.anchorMax = Vector2.one;
            taRT.offsetMin = new Vector2(15, 5);
            taRT.offsetMax = new Vector2(-15, -5);

            // Placeholder
            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(textArea.transform, false);
            RectTransform phRT = placeholderObj.AddComponent<RectTransform>();
            phRT.anchorMin = Vector2.zero;
            phRT.anchorMax = Vector2.one;
            phRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI phTmp = placeholderObj.AddComponent<TextMeshProUGUI>();
            phTmp.text = placeholder;
            phTmp.fontSize = 14;
            phTmp.color = TEXT_SECONDARY;
            phTmp.alignment = TextAlignmentOptions.Left;

            // Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(textArea.transform, false);
            RectTransform txtRT = textObj.AddComponent<RectTransform>();
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI txtTmp = textObj.AddComponent<TextMeshProUGUI>();
            txtTmp.fontSize = 14;
            txtTmp.color = Color.white;
            txtTmp.alignment = TextAlignmentOptions.Left;

            // TMP Input Field
            TMP_InputField input = inputObj.AddComponent<TMP_InputField>();
            input.textViewport = taRT;
            input.textComponent = txtTmp;
            input.placeholder = phTmp;
            input.fontAsset = txtTmp.font;
            input.pointSize = 14;

            LayoutElement le = inputObj.AddComponent<LayoutElement>();
            le.preferredHeight = 45;

            return inputObj;
        }

        private static GameObject CreateScrollView(string name, Transform parent)
        {
            GameObject scrollView = new GameObject(name);
            scrollView.transform.SetParent(parent, false);

            RectTransform svRT = scrollView.AddComponent<RectTransform>();
            svRT.anchorMin = Vector2.zero;
            svRT.anchorMax = Vector2.one;
            svRT.sizeDelta = Vector2.zero;

            ScrollRect sr = scrollView.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Elastic;
            sr.elasticity = 0.1f;

            Image svImg = scrollView.AddComponent<Image>();
            svImg.color = Color.clear;

            // Viewport
            GameObject viewport = CreatePanel("Viewport", scrollView.transform, Color.clear);
            RectTransform vpRT = viewport.GetComponent<RectTransform>();
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.sizeDelta = Vector2.zero;
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            sr.viewport = vpRT;

            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);

            RectTransform cRT = content.AddComponent<RectTransform>();
            cRT.anchorMin = new Vector2(0, 1);
            cRT.anchorMax = new Vector2(1, 1);
            cRT.pivot = new Vector2(0.5f, 1);
            cRT.sizeDelta = new Vector2(0, 0);

            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.content = cRT;

            return scrollView;
        }
    }
}
