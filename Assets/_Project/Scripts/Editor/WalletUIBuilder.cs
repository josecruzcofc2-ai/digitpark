using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Builder para la UI de Wallet - Version simplificada basada en diseño original.
    /// Incluye: Balance, botones grandes, badge verificacion, limite semanal, transacciones.
    /// </summary>
    public class WalletUIBuilder : EditorWindow
    {
        // Colores del tema
        private static readonly Color DARK_BG = new Color(0.05f, 0.05f, 0.08f, 1f);
        private static readonly Color CARD_BG = new Color(0.08f, 0.12f, 0.15f, 1f);
        private static readonly Color BALANCE_BG = new Color(0.06f, 0.15f, 0.12f, 1f);
        private static readonly Color CYAN = new Color(0f, 0.83f, 1f, 1f);
        private static readonly Color GREEN = new Color(0.2f, 0.9f, 0.4f, 1f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color RED = new Color(1f, 0.4f, 0.4f, 1f);
        private static readonly Color TEXT_WHITE = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.55f, 0.55f, 0.6f, 1f);
        private static readonly Color BUTTON_DEPOSIT = new Color(0.1f, 0.35f, 0.25f, 1f);
        private static readonly Color BUTTON_WITHDRAW = new Color(0.1f, 0.25f, 0.35f, 1f);

        [MenuItem("DigitPark/UI Builders/CashBattle/Cash Wallet", false, 254)]
        public static void BuildWalletUI()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[WalletUIBuilder] No se encontro Canvas en la escena");
                return;
            }

            Transform parent = canvas.transform.Find("SafeArea") ?? canvas.transform;

            // Verificar si ya existe
            Transform existing = parent.Find("WalletUI");
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("Wallet UI Existe",
                    "Ya existe WalletUI. Deseas reemplazarlo?",
                    "Si, Reemplazar", "Cancelar"))
                    return;

                Undo.DestroyObjectImmediate(existing.gameObject);
            }

            GameObject walletUI = CreateWalletUI(parent);
            Undo.RegisterCreatedObjectUndo(walletUI, "Create Wallet UI");
            Selection.activeGameObject = walletUI;

            Debug.Log("[WalletUIBuilder] Wallet UI creada exitosamente");
        }

        [MenuItem("DigitPark/UI Builders/CashBattle/Wallet - Transaction Prefab", false, 254)]
        public static void BuildTransactionItemPrefab()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[WalletUIBuilder] No se encontro Canvas");
                return;
            }

            GameObject prefab = CreateTransactionItem(canvas.transform);
            Selection.activeGameObject = prefab;
            Debug.Log("[WalletUIBuilder] Transaction Item creado. Guardalo como prefab.");
        }

        // ==================== MAIN UI ====================

        private static GameObject CreateWalletUI(Transform parent)
        {
            // Root
            GameObject root = new GameObject("WalletUI");
            root.transform.SetParent(parent, false);

            RectTransform rootRT = root.AddComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.sizeDelta = Vector2.zero;

            Image rootBg = root.AddComponent<Image>();
            rootBg.color = DARK_BG;

            // Main container con layout
            GameObject container = new GameObject("Container");
            container.transform.SetParent(root.transform, false);

            RectTransform containerRT = container.AddComponent<RectTransform>();
            containerRT.anchorMin = Vector2.zero;
            containerRT.anchorMax = Vector2.one;
            containerRT.offsetMin = new Vector2(20, 20);
            containerRT.offsetMax = new Vector2(-20, -20);

            VerticalLayoutGroup vlg = container.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 20;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;

            // 1. Header con titulo y badge
            CreateHeader(container.transform);

            // 2. Card de Balance con limite semanal
            CreateBalanceCard(container.transform);

            // 3. Botones DEPOSITAR / RETIRAR (grandes)
            CreateActionButtons(container.transform);

            // 4. Seccion de Transacciones
            CreateTransactionsSection(container.transform);

            // 5. Panel de Alerta de Impuestos (oculto por defecto)
            CreateTaxAlertPanel(root.transform);

            return root;
        }

        // ==================== HEADER ====================

        private static void CreateHeader(Transform parent)
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(parent, false);

            RectTransform rt = header.AddComponent<RectTransform>();
            LayoutElement le = header.AddComponent<LayoutElement>();
            le.preferredHeight = 50;

            HorizontalLayoutGroup hlg = header.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.spacing = 15;
            hlg.childForceExpandWidth = false;

            // Titulo
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(header.transform, false);
            TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
            title.text = "Mi Wallet";
            title.fontSize = 28;
            title.color = TEXT_WHITE;
            title.fontStyle = FontStyles.Bold;
            title.alignment = TextAlignmentOptions.Left;
            LayoutElement titleLE = titleObj.AddComponent<LayoutElement>();
            titleLE.flexibleWidth = 1;

            // Badge de verificacion
            CreateVerificationBadge(header.transform);
        }

        private static void CreateVerificationBadge(Transform parent)
        {
            GameObject badge = new GameObject("VerificationBadge");
            badge.transform.SetParent(parent, false);

            RectTransform rt = badge.AddComponent<RectTransform>();
            LayoutElement le = badge.AddComponent<LayoutElement>();
            le.preferredWidth = 110;
            le.preferredHeight = 30;

            Image bg = badge.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.3f, 0.2f, 0.9f);

            HorizontalLayoutGroup hlg = badge.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(10, 10, 5, 5);
            hlg.spacing = 6;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;

            // Check icon
            GameObject checkObj = new GameObject("Check");
            checkObj.transform.SetParent(badge.transform, false);
            TextMeshProUGUI check = checkObj.AddComponent<TextMeshProUGUI>();
            check.text = "✓";
            check.fontSize = 18;
            check.color = GREEN;
            check.alignment = TextAlignmentOptions.Center;
            LayoutElement checkLE = checkObj.AddComponent<LayoutElement>();
            checkLE.preferredWidth = 22;

            // Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(badge.transform, false);
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "Verificado";
            text.fontSize = 14;
            text.color = GREEN;
            text.alignment = TextAlignmentOptions.Left;
        }

        // ==================== BALANCE CARD ====================

        private static void CreateBalanceCard(Transform parent)
        {
            GameObject card = new GameObject("BalanceCard");
            card.transform.SetParent(parent, false);

            RectTransform rt = card.AddComponent<RectTransform>();
            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredHeight = 140;

            Image bg = card.AddComponent<Image>();
            bg.color = BALANCE_BG;

            // Borde sutil
            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = new Color(0.2f, 0.5f, 0.4f, 0.5f);
            outline.effectDistance = new Vector2(1, -1);

            VerticalLayoutGroup vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 20, 15);
            vlg.spacing = 8;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childForceExpandHeight = false;

            // Balance amount
            GameObject balanceObj = new GameObject("BalanceAmount");
            balanceObj.transform.SetParent(card.transform, false);
            TextMeshProUGUI balance = balanceObj.AddComponent<TextMeshProUGUI>();
            balance.text = "$0.00";
            balance.fontSize = 48;
            balance.color = GREEN;
            balance.fontStyle = FontStyles.Bold;
            balance.alignment = TextAlignmentOptions.Center;
            LayoutElement balanceLE = balanceObj.AddComponent<LayoutElement>();
            balanceLE.preferredHeight = 55;

            // Label "Balance Disponible"
            GameObject labelObj = new GameObject("BalanceLabel");
            labelObj.transform.SetParent(card.transform, false);
            TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
            label.text = "Balance Disponible";
            label.fontSize = 16;
            label.color = CYAN;
            label.alignment = TextAlignmentOptions.Center;
            LayoutElement labelLE = labelObj.AddComponent<LayoutElement>();
            labelLE.preferredHeight = 22;

            // Limite semanal
            CreateWeeklyLimitIndicator(card.transform);
        }

        private static void CreateWeeklyLimitIndicator(Transform parent)
        {
            GameObject limitObj = new GameObject("WeeklyLimit");
            limitObj.transform.SetParent(parent, false);

            RectTransform rt = limitObj.AddComponent<RectTransform>();
            LayoutElement le = limitObj.AddComponent<LayoutElement>();
            le.preferredHeight = 25;

            HorizontalLayoutGroup hlg = limitObj.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 8;
            hlg.childForceExpandWidth = false;

            // Texto limite
            GameObject textObj = new GameObject("LimitText");
            textObj.transform.SetParent(limitObj.transform, false);
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "Limite semanal:";
            text.fontSize = 13;
            text.color = TEXT_SECONDARY;
            text.alignment = TextAlignmentOptions.Right;
            LayoutElement textLE = textObj.AddComponent<LayoutElement>();
            textLE.preferredWidth = 120;

            // Valor
            GameObject valueObj = new GameObject("LimitValue");
            valueObj.transform.SetParent(limitObj.transform, false);
            TextMeshProUGUI value = valueObj.AddComponent<TextMeshProUGUI>();
            value.text = "$87 / $150";
            value.fontSize = 14;
            value.color = CYAN;
            value.fontStyle = FontStyles.Bold;
            value.alignment = TextAlignmentOptions.Left;
            LayoutElement valueLE = valueObj.AddComponent<LayoutElement>();
            valueLE.preferredWidth = 100;
        }

        // ==================== ACTION BUTTONS ====================

        private static void CreateActionButtons(Transform parent)
        {
            GameObject buttonsContainer = new GameObject("ActionButtons");
            buttonsContainer.transform.SetParent(parent, false);

            RectTransform rt = buttonsContainer.AddComponent<RectTransform>();
            LayoutElement le = buttonsContainer.AddComponent<LayoutElement>();
            le.preferredHeight = 70;

            HorizontalLayoutGroup hlg = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // Boton DEPOSITAR
            CreateActionButton(buttonsContainer.transform, "DepositButton", "DEPOSITAR", BUTTON_DEPOSIT, GREEN);

            // Boton RETIRAR
            CreateActionButton(buttonsContainer.transform, "WithdrawButton", "RETIRAR", BUTTON_WITHDRAW, CYAN);
        }

        private static void CreateActionButton(Transform parent, string name, string text, Color bgColor, Color accentColor)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();

            Image bg = btnObj.AddComponent<Image>();
            bg.color = bgColor;

            // Borde de acento
            Outline outline = btnObj.AddComponent<Outline>();
            outline.effectColor = accentColor;
            outline.effectDistance = new Vector2(2, -2);

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;

            ColorBlock colors = btn.colors;
            colors.highlightedColor = bgColor * 1.3f;
            colors.pressedColor = bgColor * 0.7f;
            btn.colors = colors;

            // Texto del boton
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.text = text;
            btnText.fontSize = 22;
            btnText.color = accentColor;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.fontStyle = FontStyles.Bold;
        }

        // ==================== TRANSACTIONS SECTION ====================

        private static void CreateTransactionsSection(Transform parent)
        {
            GameObject section = new GameObject("TransactionsSection");
            section.transform.SetParent(parent, false);

            RectTransform rt = section.AddComponent<RectTransform>();
            LayoutElement le = section.AddComponent<LayoutElement>();
            le.flexibleHeight = 1;
            le.minHeight = 200;

            VerticalLayoutGroup vlg = section.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.childForceExpandHeight = false;
            vlg.childControlHeight = false;

            // Titulo
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(section.transform, false);
            TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
            title.text = "Transacciones Recientes";
            title.fontSize = 18;
            title.color = TEXT_SECONDARY;
            title.alignment = TextAlignmentOptions.Left;
            LayoutElement titleLE = titleObj.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 28;

            // Scroll View para transacciones
            CreateTransactionsScrollView(section.transform);
        }

        private static void CreateTransactionsScrollView(Transform parent)
        {
            GameObject scrollView = new GameObject("ScrollView");
            scrollView.transform.SetParent(parent, false);

            RectTransform svRT = scrollView.AddComponent<RectTransform>();
            LayoutElement svLE = scrollView.AddComponent<LayoutElement>();
            svLE.flexibleHeight = 1;

            ScrollRect sr = scrollView.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Elastic;

            Image svBg = scrollView.AddComponent<Image>();
            svBg.color = Color.clear;

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);

            RectTransform vpRT = viewport.AddComponent<RectTransform>();
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.sizeDelta = Vector2.zero;

            Image vpImg = viewport.AddComponent<Image>();
            vpImg.color = Color.clear;
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

            VerticalLayoutGroup cVlg = content.AddComponent<VerticalLayoutGroup>();
            cVlg.spacing = 8;
            cVlg.padding = new RectOffset(0, 0, 5, 5);
            cVlg.childForceExpandHeight = false;
            cVlg.childForceExpandWidth = true;
            cVlg.childControlHeight = false;

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.content = cRT;

            // Texto vacio
            GameObject emptyText = new GameObject("EmptyText");
            emptyText.transform.SetParent(content.transform, false);
            TextMeshProUGUI empty = emptyText.AddComponent<TextMeshProUGUI>();
            empty.text = "No hay transacciones recientes";
            empty.fontSize = 16;
            empty.color = TEXT_SECONDARY;
            empty.alignment = TextAlignmentOptions.Center;
            LayoutElement emptyLE = emptyText.AddComponent<LayoutElement>();
            emptyLE.preferredHeight = 50;
        }

        // ==================== TAX ALERT PANEL ====================

        private static void CreateTaxAlertPanel(Transform parent)
        {
            GameObject panel = new GameObject("TaxAlertPanel");
            panel.transform.SetParent(parent, false);
            panel.SetActive(false); // Oculto por defecto

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.05f, 0.3f);
            rt.anchorMax = new Vector2(0.95f, 0.7f);
            rt.sizeDelta = Vector2.zero;

            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0.25f, 0.15f, 0.08f, 0.98f);

            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = GOLD;
            outline.effectDistance = new Vector2(2, -2);

            VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(25, 25, 25, 25);
            vlg.spacing = 15;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childForceExpandHeight = false;

            // Titulo
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panel.transform, false);
            TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
            title.text = "ACCION REQUERIDA";
            title.fontSize = 24;
            title.color = GOLD;
            title.fontStyle = FontStyles.Bold;
            title.alignment = TextAlignmentOptions.Center;
            LayoutElement titleLE = titleObj.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 35;

            // Mensaje
            GameObject msgObj = new GameObject("Message");
            msgObj.transform.SetParent(panel.transform, false);
            TextMeshProUGUI msg = msgObj.AddComponent<TextMeshProUGUI>();
            msg.text = "Has ganado mas de $600 este año.\nCompleta tu informacion fiscal (W-9)\npara continuar retirando.";
            msg.fontSize = 17;
            msg.color = TEXT_WHITE;
            msg.alignment = TextAlignmentOptions.Center;
            msg.enableWordWrapping = true;
            LayoutElement msgLE = msgObj.AddComponent<LayoutElement>();
            msgLE.preferredHeight = 80;

            // Cantidad ganada
            GameObject amountObj = new GameObject("Amount");
            amountObj.transform.SetParent(panel.transform, false);
            TextMeshProUGUI amount = amountObj.AddComponent<TextMeshProUGUI>();
            amount.text = "Ganado este año: $623.50";
            amount.fontSize = 16;
            amount.color = RED;
            amount.fontStyle = FontStyles.Bold;
            amount.alignment = TextAlignmentOptions.Center;
            LayoutElement amountLE = amountObj.AddComponent<LayoutElement>();
            amountLE.preferredHeight = 25;

            // Boton
            GameObject btnObj = new GameObject("CompleteButton");
            btnObj.transform.SetParent(panel.transform, false);

            RectTransform btnRT = btnObj.AddComponent<RectTransform>();
            LayoutElement btnLE = btnObj.AddComponent<LayoutElement>();
            btnLE.preferredHeight = 50;
            btnLE.preferredWidth = 250;

            Image btnBg = btnObj.AddComponent<Image>();
            btnBg.color = GOLD;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnBg;

            GameObject btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(btnObj.transform, false);
            RectTransform btnTextRT = btnTextObj.AddComponent<RectTransform>();
            btnTextRT.anchorMin = Vector2.zero;
            btnTextRT.anchorMax = Vector2.one;
            btnTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnText.text = "COMPLETAR W-9";
            btnText.fontSize = 18;
            btnText.color = Color.black;
            btnText.fontStyle = FontStyles.Bold;
            btnText.alignment = TextAlignmentOptions.Center;
        }

        // ==================== TRANSACTION ITEM PREFAB ====================

        private static GameObject CreateTransactionItem(Transform parent)
        {
            GameObject item = new GameObject("TransactionItem");
            item.transform.SetParent(parent, false);

            RectTransform rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 65);

            Image bg = item.AddComponent<Image>();
            bg.color = CARD_BG;

            LayoutElement le = item.AddComponent<LayoutElement>();
            le.preferredHeight = 65;

            HorizontalLayoutGroup hlg = item.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(15, 15, 10, 10);
            hlg.spacing = 15;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // Monto (izquierda)
            GameObject amountObj = new GameObject("Amount");
            amountObj.transform.SetParent(item.transform, false);
            TextMeshProUGUI amount = amountObj.AddComponent<TextMeshProUGUI>();
            amount.text = "+$10.00";
            amount.fontSize = 20;
            amount.color = GREEN;
            amount.fontStyle = FontStyles.Bold;
            amount.alignment = TextAlignmentOptions.Left;
            LayoutElement amountLE = amountObj.AddComponent<LayoutElement>();
            amountLE.preferredWidth = 90;

            // Info (centro)
            GameObject infoContainer = new GameObject("Info");
            infoContainer.transform.SetParent(item.transform, false);
            infoContainer.AddComponent<RectTransform>();

            VerticalLayoutGroup infoVlg = infoContainer.AddComponent<VerticalLayoutGroup>();
            infoVlg.spacing = 3;
            infoVlg.childAlignment = TextAnchor.MiddleLeft;
            infoVlg.childForceExpandHeight = false;

            LayoutElement infoLE = infoContainer.AddComponent<LayoutElement>();
            infoLE.flexibleWidth = 1;

            // Descripcion
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(infoContainer.transform, false);
            TextMeshProUGUI desc = descObj.AddComponent<TextMeshProUGUI>();
            desc.text = "Ganaste - CashBattle vs @Player123";
            desc.fontSize = 15;
            desc.color = TEXT_WHITE;
            desc.alignment = TextAlignmentOptions.Left;
            desc.enableWordWrapping = false;
            desc.overflowMode = TextOverflowModes.Ellipsis;
            LayoutElement descLE = descObj.AddComponent<LayoutElement>();
            descLE.preferredHeight = 22;

            // Fecha
            GameObject dateObj = new GameObject("Date");
            dateObj.transform.SetParent(infoContainer.transform, false);
            TextMeshProUGUI date = dateObj.AddComponent<TextMeshProUGUI>();
            date.text = "hace 2 horas";
            date.fontSize = 13;
            date.color = TEXT_SECONDARY;
            date.alignment = TextAlignmentOptions.Left;
            LayoutElement dateLE = dateObj.AddComponent<LayoutElement>();
            dateLE.preferredHeight = 18;

            // Indicador de color (derecha)
            GameObject indicator = new GameObject("Indicator");
            indicator.transform.SetParent(item.transform, false);

            RectTransform indRT = indicator.AddComponent<RectTransform>();
            LayoutElement indLE = indicator.AddComponent<LayoutElement>();
            indLE.preferredWidth = 8;
            indLE.preferredHeight = 40;

            Image indImg = indicator.AddComponent<Image>();
            indImg.color = GREEN;

            return item;
        }
    }
}
