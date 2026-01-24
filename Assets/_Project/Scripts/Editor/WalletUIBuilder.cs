using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Builder para la UI de Wallet - Version PREMIUM con iconos.
    /// Incluye: Balance con icono, botones con iconos, badge verificacion, transacciones con iconos.
    /// </summary>
    public class WalletUIBuilder : EditorWindow
    {
        #region Colors

        private static readonly Color DARK_BG = new Color(0.05f, 0.05f, 0.08f, 1f);
        private static readonly Color CARD_BG = new Color(0.1f, 0.12f, 0.16f, 1f);
        private static readonly Color BALANCE_BG = new Color(0.06f, 0.12f, 0.1f, 1f);
        private static readonly Color CYAN = new Color(0f, 0.9f, 1f, 1f);
        private static readonly Color GREEN = new Color(0.2f, 0.95f, 0.4f, 1f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color RED = new Color(1f, 0.4f, 0.4f, 1f);
        private static readonly Color TEXT_WHITE = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.5f, 0.5f, 0.55f, 1f);
        private static readonly Color BUTTON_DEPOSIT = new Color(0.08f, 0.25f, 0.18f, 1f);
        private static readonly Color BUTTON_WITHDRAW = new Color(0.08f, 0.18f, 0.28f, 1f);
        private static readonly Color CARD_BORDER = new Color(0.85f, 0.65f, 0.13f, 0.4f);

        #endregion

        #region Paths

        private static readonly string WALLET_ICONS_PATH = "Assets/_Project/Art/Icons/CashBattle/Wallet/";

        #endregion

        [MenuItem("DigitPark/UI Builders/CashBattle/Cash Wallet", false, 254)]
        public static void ShowWindow()
        {
            GetWindow<WalletUIBuilder>("Cash Wallet Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Cash Wallet UI Builder", EditorStyles.boldLabel);
            GUILayout.Label("Wallet premium con iconos", EditorStyles.miniLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Construye la UI PREMIUM para CashWallet:\n\n" +
                "- Header con título y badge verificado\n" +
                "- Balance card con icono wallet\n" +
                "- Botones DEPOSITAR/RETIRAR con iconos\n" +
                "- Transacciones con iconos por tipo\n" +
                "- Límite semanal con barra de progreso",
                MessageType.Info);

            EditorGUILayout.Space(10);

            GUI.backgroundColor = new Color(0.2f, 0.9f, 0.4f, 1f);
            if (GUILayout.Button("CONSTRUIR UI COMPLETA", GUILayout.Height(40)))
            {
                BuildWalletUI();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(15);
            GUILayout.Label("Construcción por Secciones:", EditorStyles.boldLabel);

            if (GUILayout.Button("Solo Balance Card", GUILayout.Height(28)))
            {
                BuildBalanceCardOnly();
            }

            if (GUILayout.Button("Solo Action Buttons", GUILayout.Height(28)))
            {
                BuildActionButtonsOnly();
            }

        }

        #region Build Methods

        private static void BuildWalletUI()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontró Canvas. Abre la escena CashWallet primero.", "OK");
                return;
            }

            if (EditorUtility.DisplayDialog("Reconstruir UI?",
                "Esto reconstruirá la UI de Cash Wallet con el nuevo diseño premium.\n\n¿Continuar?",
                "Sí, Construir", "Cancelar"))
            {
                BuildAllElements(canvas);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                Debug.Log("[WalletUIBuilder] UI construida exitosamente!");
            }
        }

        private static void BuildBalanceCardOnly()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            Transform container = canvas.transform.Find("WalletUI/Container");
            if (container == null)
            {
                Debug.LogError("Container no encontrado. Construye la UI completa primero.");
                return;
            }

            Transform old = container.Find("BalanceCard");
            if (old != null) DestroyImmediate(old.gameObject);

            CreateBalanceCard(container);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private static void BuildActionButtonsOnly()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            Transform container = canvas.transform.Find("WalletUI/Container");
            if (container == null)
            {
                Debug.LogError("Container no encontrado. Construye la UI completa primero.");
                return;
            }

            Transform old = container.Find("ActionButtons");
            if (old != null) DestroyImmediate(old.gameObject);

            CreateActionButtons(container);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private static void BuildAllElements(Canvas canvas)
        {
            Transform canvasTransform = canvas.transform;

            // Cleanup old
            Transform oldUI = canvasTransform.Find("WalletUI");
            if (oldUI != null) DestroyImmediate(oldUI.gameObject);

            // Root
            GameObject root = new GameObject("WalletUI");
            root.transform.SetParent(canvasTransform, false);

            RectTransform rootRT = root.AddComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.sizeDelta = Vector2.zero;

            Image rootBg = root.AddComponent<Image>();
            rootBg.color = DARK_BG;

            // Container
            GameObject container = new GameObject("Container");
            container.transform.SetParent(root.transform, false);

            RectTransform containerRT = container.AddComponent<RectTransform>();
            containerRT.anchorMin = Vector2.zero;
            containerRT.anchorMax = Vector2.one;
            containerRT.offsetMin = new Vector2(20, 20);
            containerRT.offsetMax = new Vector2(-20, -20);  // Sin espacio extra

            VerticalLayoutGroup vlg = container.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 18;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;

            // Build sections
            CreateHeader(container.transform);
            CreateBalanceCard(container.transform);
            CreateActionButtons(container.transform);
            CreateTransactionsSection(container.transform);

            // Tax alert (hidden)
            CreateTaxAlertPanel(root.transform);
        }

        #endregion

        #region Header

        private static void CreateHeader(Transform parent)
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(parent, false);

            LayoutElement le = header.AddComponent<LayoutElement>();
            le.preferredHeight = 50;

            // Title solamente - badge de verificación se muestra en el balance card
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(header.transform, false);

            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = Vector2.zero;
            titleRT.anchorMax = Vector2.one;
            titleRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
            title.text = "Mi Wallet";
            title.fontSize = 32;
            title.color = GOLD;
            title.fontStyle = FontStyles.Bold;
            title.alignment = TextAlignmentOptions.Left;
        }

        private static void CreateVerificationBadge(Transform parent)
        {
            GameObject badge = new GameObject("VerificationBadge");
            badge.transform.SetParent(parent, false);

            LayoutElement le = badge.AddComponent<LayoutElement>();
            le.preferredWidth = 140;
            le.preferredHeight = 40;

            Image bg = badge.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.25f, 0.18f, 0.95f);

            Outline outline = badge.AddComponent<Outline>();
            outline.effectColor = GREEN;
            outline.effectDistance = new Vector2(1, -1);

            HorizontalLayoutGroup hlg = badge.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(8, 12, 5, 5);
            hlg.spacing = 8;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(badge.transform, false);

            LayoutElement iconLE = iconObj.AddComponent<LayoutElement>();
            iconLE.preferredWidth = 28;
            iconLE.preferredHeight = 28;

            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.preserveAspect = true;

            Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(WALLET_ICONS_PATH + "VerifiedBadgeIcon.png");
            if (iconSprite != null)
            {
                iconImg.sprite = iconSprite;
                iconImg.color = Color.white;
            }
            else
            {
                iconImg.color = GREEN;
            }

            // Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(badge.transform, false);

            LayoutElement textLE = textObj.AddComponent<LayoutElement>();
            textLE.preferredWidth = 80;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "Verificado";
            text.fontSize = 16;
            text.color = GREEN;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Left;
        }

        #endregion

        #region Balance Card

        private static void CreateBalanceCard(Transform parent)
        {
            GameObject card = new GameObject("BalanceCard");
            card.transform.SetParent(parent, false);

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredHeight = 180;

            Image bg = card.AddComponent<Image>();
            bg.color = BALANCE_BG;

            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = new Color(0.2f, 0.6f, 0.4f, 0.6f);
            outline.effectDistance = new Vector2(2, -2);

            // Content container - horizontal para icono + info
            HorizontalLayoutGroup hlg = card.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(25, 25, 20, 20);
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = false;

            // Wallet Icon (left)
            GameObject iconObj = new GameObject("WalletIcon");
            iconObj.transform.SetParent(card.transform, false);

            LayoutElement iconLE = iconObj.AddComponent<LayoutElement>();
            iconLE.preferredWidth = 90;
            iconLE.preferredHeight = 90;

            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.preserveAspect = true;

            Sprite walletSprite = AssetDatabase.LoadAssetAtPath<Sprite>(WALLET_ICONS_PATH + "WalletIcon.png");
            if (walletSprite != null)
            {
                iconImg.sprite = walletSprite;
                iconImg.color = Color.white;
            }
            else
            {
                iconImg.color = GOLD;
            }

            // Info container (right)
            GameObject infoContainer = new GameObject("InfoContainer");
            infoContainer.transform.SetParent(card.transform, false);

            LayoutElement infoLE = infoContainer.AddComponent<LayoutElement>();
            infoLE.flexibleWidth = 1;

            VerticalLayoutGroup infoVLG = infoContainer.AddComponent<VerticalLayoutGroup>();
            infoVLG.spacing = 5;
            infoVLG.childAlignment = TextAnchor.MiddleLeft;
            infoVLG.childForceExpandHeight = false;
            infoVLG.childControlHeight = false;

            // Balance label
            GameObject labelObj = new GameObject("BalanceLabel");
            labelObj.transform.SetParent(infoContainer.transform, false);

            LayoutElement labelLE = labelObj.AddComponent<LayoutElement>();
            labelLE.preferredHeight = 22;

            TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
            label.text = "Balance Disponible";
            label.fontSize = 16;
            label.color = TEXT_SECONDARY;
            label.alignment = TextAlignmentOptions.Left;

            // Balance amount
            GameObject amountObj = new GameObject("BalanceAmount");
            amountObj.transform.SetParent(infoContainer.transform, false);

            LayoutElement amountLE = amountObj.AddComponent<LayoutElement>();
            amountLE.preferredHeight = 60;

            TextMeshProUGUI amount = amountObj.AddComponent<TextMeshProUGUI>();
            amount.text = "$0.00";
            amount.fontSize = 52;
            amount.color = GREEN;
            amount.fontStyle = FontStyles.Bold;
            amount.alignment = TextAlignmentOptions.Left;

            // Weekly limit
            CreateWeeklyLimitBar(infoContainer.transform);
        }

        private static void CreateWeeklyLimitBar(Transform parent)
        {
            GameObject limitContainer = new GameObject("WeeklyLimit");
            limitContainer.transform.SetParent(parent, false);

            LayoutElement le = limitContainer.AddComponent<LayoutElement>();
            le.preferredHeight = 45;

            VerticalLayoutGroup vlg = limitContainer.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 5;
            vlg.childForceExpandHeight = false;
            vlg.childControlHeight = false;

            // Label row
            GameObject labelRow = new GameObject("LabelRow");
            labelRow.transform.SetParent(limitContainer.transform, false);

            LayoutElement labelRowLE = labelRow.AddComponent<LayoutElement>();
            labelRowLE.preferredHeight = 18;

            HorizontalLayoutGroup labelHLG = labelRow.AddComponent<HorizontalLayoutGroup>();
            labelHLG.childForceExpandWidth = false;

            GameObject limitLabel = new GameObject("LimitLabel");
            limitLabel.transform.SetParent(labelRow.transform, false);

            LayoutElement limitLabelLE = limitLabel.AddComponent<LayoutElement>();
            limitLabelLE.flexibleWidth = 1;

            TextMeshProUGUI limitText = limitLabel.AddComponent<TextMeshProUGUI>();
            limitText.text = "Límite semanal";
            limitText.fontSize = 13;
            limitText.color = TEXT_SECONDARY;
            limitText.alignment = TextAlignmentOptions.Left;

            GameObject limitValue = new GameObject("LimitValue");
            limitValue.transform.SetParent(labelRow.transform, false);

            LayoutElement limitValueLE = limitValue.AddComponent<LayoutElement>();
            limitValueLE.preferredWidth = 100;

            TextMeshProUGUI valueText = limitValue.AddComponent<TextMeshProUGUI>();
            valueText.text = "$87 / $150";
            valueText.fontSize = 13;
            valueText.color = CYAN;
            valueText.fontStyle = FontStyles.Bold;
            valueText.alignment = TextAlignmentOptions.Right;

            // Progress bar
            GameObject barBg = new GameObject("ProgressBarBg");
            barBg.transform.SetParent(limitContainer.transform, false);

            LayoutElement barLE = barBg.AddComponent<LayoutElement>();
            barLE.preferredHeight = 8;

            Image barBgImg = barBg.AddComponent<Image>();
            barBgImg.color = new Color(0.15f, 0.15f, 0.2f, 1f);

            // Progress fill
            GameObject barFill = new GameObject("ProgressFill");
            barFill.transform.SetParent(barBg.transform, false);

            RectTransform fillRT = barFill.AddComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = new Vector2(0.58f, 1f);  // 87/150 = 58%
            fillRT.sizeDelta = Vector2.zero;
            fillRT.offsetMin = Vector2.zero;
            fillRT.offsetMax = Vector2.zero;

            Image fillImg = barFill.AddComponent<Image>();
            fillImg.color = CYAN;
        }

        #endregion

        #region Action Buttons

        private static void CreateActionButtons(Transform parent)
        {
            GameObject container = new GameObject("ActionButtons");
            container.transform.SetParent(parent, false);

            LayoutElement le = container.AddComponent<LayoutElement>();
            le.preferredHeight = 85;

            HorizontalLayoutGroup hlg = container.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 15;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // Deposit button
            CreateActionButtonWithIcon(container.transform, "DepositButton", "DEPOSITAR",
                BUTTON_DEPOSIT, GREEN, "DepositIcon.png");

            // Withdraw button
            CreateActionButtonWithIcon(container.transform, "WithdrawButton", "RETIRAR",
                BUTTON_WITHDRAW, CYAN, "WithdrawIcon.png");
        }

        private static void CreateActionButtonWithIcon(Transform parent, string name, string text,
            Color bgColor, Color accentColor, string iconName)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            Image bg = btnObj.AddComponent<Image>();
            bg.color = bgColor;

            Outline outline = btnObj.AddComponent<Outline>();
            outline.effectColor = accentColor;
            outline.effectDistance = new Vector2(2, -2);

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;

            ColorBlock colors = btn.colors;
            colors.highlightedColor = bgColor * 1.4f;
            colors.pressedColor = bgColor * 0.7f;
            btn.colors = colors;

            // Content layout
            HorizontalLayoutGroup hlg = btnObj.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(15, 15, 10, 10);
            hlg.spacing = 12;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childControlWidth = false;

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(btnObj.transform, false);

            LayoutElement iconLE = iconObj.AddComponent<LayoutElement>();
            iconLE.preferredWidth = 50;
            iconLE.preferredHeight = 50;

            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.preserveAspect = true;

            Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(WALLET_ICONS_PATH + iconName);
            if (iconSprite != null)
            {
                iconImg.sprite = iconSprite;
                iconImg.color = Color.white;
            }
            else
            {
                iconImg.color = accentColor;
            }

            // Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            LayoutElement textLE = textObj.AddComponent<LayoutElement>();
            textLE.flexibleWidth = 1;

            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.text = text;
            btnText.fontSize = 20;
            btnText.color = accentColor;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.fontStyle = FontStyles.Bold;
        }

        #endregion

        #region Transactions Section

        private static void CreateTransactionsSection(Transform parent)
        {
            GameObject section = new GameObject("TransactionsSection");
            section.transform.SetParent(parent, false);

            RectTransform sectionRT = section.AddComponent<RectTransform>();

            LayoutElement le = section.AddComponent<LayoutElement>();
            le.flexibleHeight = 1;
            le.minHeight = 250;

            // Title - posicionado manualmente arriba
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(section.transform, false);

            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 1);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0, 1);
            titleRT.sizeDelta = new Vector2(0, 30);
            titleRT.anchoredPosition = Vector2.zero;

            TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
            title.text = "Transacciones Recientes";
            title.fontSize = 20;
            title.color = TEXT_WHITE;
            title.fontStyle = FontStyles.Bold;
            title.alignment = TextAlignmentOptions.Left;

            // ScrollView - debajo del título
            CreateTransactionsScrollView(section.transform);
        }

        private static void CreateTransactionsScrollView(Transform parent)
        {
            GameObject scrollView = new GameObject("ScrollView");
            scrollView.transform.SetParent(parent, false);

            RectTransform svRT = scrollView.AddComponent<RectTransform>();
            svRT.anchorMin = Vector2.zero;
            svRT.anchorMax = Vector2.one;
            svRT.offsetMin = new Vector2(0, 0);
            svRT.offsetMax = new Vector2(0, -35);  // Espacio para el título

            ScrollRect sr = scrollView.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Elastic;
            sr.scrollSensitivity = 30;

            scrollView.AddComponent<Image>().color = Color.clear;

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);

            RectTransform vpRT = viewport.AddComponent<RectTransform>();
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.sizeDelta = Vector2.zero;
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
            cRT.sizeDelta = new Vector2(0, 600);  // Altura inicial
            cRT.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup cVlg = content.AddComponent<VerticalLayoutGroup>();
            cVlg.spacing = 10;
            cVlg.padding = new RectOffset(0, 0, 5, 10);
            cVlg.childForceExpandHeight = false;
            cVlg.childForceExpandWidth = true;
            cVlg.childControlHeight = false;
            cVlg.childControlWidth = true;

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.content = cRT;

            // NO crear transacciones de ejemplo aquí
            // En runtime, el WalletManager instanciará TransactionItemUI.prefab para cada transacción

            // Texto placeholder cuando no hay transacciones
            GameObject emptyText = new GameObject("EmptyText");
            emptyText.transform.SetParent(content.transform, false);

            RectTransform emptyRT = emptyText.AddComponent<RectTransform>();
            emptyRT.sizeDelta = new Vector2(0, 100);

            LayoutElement emptyLE = emptyText.AddComponent<LayoutElement>();
            emptyLE.preferredHeight = 100;

            TextMeshProUGUI empty = emptyText.AddComponent<TextMeshProUGUI>();
            empty.text = "No hay transacciones recientes\n\nLas transacciones aparecerán aquí";
            empty.fontSize = 16;
            empty.color = TEXT_SECONDARY;
            empty.alignment = TextAlignmentOptions.Center;
        }

        private static GameObject CreateTransactionItem(Transform parent, string type, string amount, string description, string date)
        {
            GameObject item = new GameObject("Transaction_" + type);
            item.transform.SetParent(parent, false);

            RectTransform itemRT = item.AddComponent<RectTransform>();
            itemRT.sizeDelta = new Vector2(0, 70);

            LayoutElement le = item.AddComponent<LayoutElement>();
            le.preferredHeight = 70;
            le.minHeight = 70;

            Image bg = item.AddComponent<Image>();
            bg.color = CARD_BG;

            // Color bar izquierda
            GameObject colorBar = new GameObject("ColorBar");
            colorBar.transform.SetParent(item.transform, false);

            RectTransform barRT = colorBar.AddComponent<RectTransform>();
            barRT.anchorMin = new Vector2(0, 0);
            barRT.anchorMax = new Vector2(0, 1);
            barRT.pivot = new Vector2(0, 0.5f);
            barRT.sizeDelta = new Vector2(5, 0);
            barRT.anchoredPosition = Vector2.zero;

            Image barImg = colorBar.AddComponent<Image>();
            barImg.color = type == "loss" ? RED : GREEN;

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(item.transform, false);

            RectTransform iconRT = iconObj.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0, 0.5f);
            iconRT.anchorMax = new Vector2(0, 0.5f);
            iconRT.pivot = new Vector2(0, 0.5f);
            iconRT.sizeDelta = new Vector2(45, 45);
            iconRT.anchoredPosition = new Vector2(15, 0);

            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.preserveAspect = true;

            string iconName = type switch
            {
                "win" => "TxWinIcon.png",
                "loss" => "TxLossIcon.png",
                "deposit" => "TxDepositIcon.png",
                _ => "TxWinIcon.png"
            };

            Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(WALLET_ICONS_PATH + iconName);
            if (iconSprite != null)
            {
                iconImg.sprite = iconSprite;
                iconImg.color = Color.white;
            }
            else
            {
                // Placeholder color si no hay sprite
                iconImg.color = type == "loss" ? RED : GREEN;
            }

            // Description
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(item.transform, false);

            RectTransform descRT = descObj.AddComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0, 0.5f);
            descRT.anchorMax = new Vector2(1, 1);
            descRT.pivot = new Vector2(0, 1);
            descRT.offsetMin = new Vector2(70, 0);
            descRT.offsetMax = new Vector2(-100, -8);

            TextMeshProUGUI desc = descObj.AddComponent<TextMeshProUGUI>();
            desc.text = description;
            desc.fontSize = 15;
            desc.color = TEXT_WHITE;
            desc.alignment = TextAlignmentOptions.Left;
            desc.overflowMode = TextOverflowModes.Ellipsis;

            // Date
            GameObject dateObj = new GameObject("Date");
            dateObj.transform.SetParent(item.transform, false);

            RectTransform dateRT = dateObj.AddComponent<RectTransform>();
            dateRT.anchorMin = new Vector2(0, 0);
            dateRT.anchorMax = new Vector2(1, 0.5f);
            dateRT.pivot = new Vector2(0, 0);
            dateRT.offsetMin = new Vector2(70, 8);
            dateRT.offsetMax = new Vector2(-100, 0);

            TextMeshProUGUI dateText = dateObj.AddComponent<TextMeshProUGUI>();
            dateText.text = date;
            dateText.fontSize = 13;
            dateText.color = TEXT_SECONDARY;
            dateText.alignment = TextAlignmentOptions.Left;

            // Amount
            GameObject amountObj = new GameObject("Amount");
            amountObj.transform.SetParent(item.transform, false);

            RectTransform amtRT = amountObj.AddComponent<RectTransform>();
            amtRT.anchorMin = new Vector2(1, 0);
            amtRT.anchorMax = new Vector2(1, 1);
            amtRT.pivot = new Vector2(1, 0.5f);
            amtRT.sizeDelta = new Vector2(90, 0);
            amtRT.anchoredPosition = new Vector2(-10, 0);

            TextMeshProUGUI amountText = amountObj.AddComponent<TextMeshProUGUI>();
            amountText.text = amount;
            amountText.fontSize = 20;
            amountText.color = type == "loss" ? RED : GREEN;
            amountText.fontStyle = FontStyles.Bold;
            amountText.alignment = TextAlignmentOptions.Right;

            return item;
        }

        #endregion

        #region Tax Alert Panel

        private static void CreateTaxAlertPanel(Transform parent)
        {
            GameObject panel = new GameObject("TaxAlertPanel");
            panel.transform.SetParent(parent, false);
            panel.SetActive(false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.05f, 0.3f);
            rt.anchorMax = new Vector2(0.95f, 0.7f);
            rt.sizeDelta = Vector2.zero;

            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.12f, 0.05f, 0.98f);

            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = GOLD;
            outline.effectDistance = new Vector2(2, -2);

            VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(25, 25, 25, 25);
            vlg.spacing = 15;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childForceExpandHeight = false;

            // Warning icon placeholder
            GameObject iconObj = new GameObject("WarningIcon");
            iconObj.transform.SetParent(panel.transform, false);

            LayoutElement iconLE = iconObj.AddComponent<LayoutElement>();
            iconLE.preferredWidth = 60;
            iconLE.preferredHeight = 60;

            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.color = GOLD;

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panel.transform, false);

            LayoutElement titleLE = titleObj.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 35;

            TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
            title.text = "ACCIÓN REQUERIDA";
            title.fontSize = 24;
            title.color = GOLD;
            title.fontStyle = FontStyles.Bold;
            title.alignment = TextAlignmentOptions.Center;

            // Message
            GameObject msgObj = new GameObject("Message");
            msgObj.transform.SetParent(panel.transform, false);

            LayoutElement msgLE = msgObj.AddComponent<LayoutElement>();
            msgLE.preferredHeight = 70;

            TextMeshProUGUI msg = msgObj.AddComponent<TextMeshProUGUI>();
            msg.text = "Has ganado más de $600 este año.\nCompleta tu información fiscal (W-9)\npara continuar retirando.";
            msg.fontSize = 16;
            msg.color = TEXT_WHITE;
            msg.alignment = TextAlignmentOptions.Center;
            msg.enableWordWrapping = true;

            // Amount
            GameObject amountObj = new GameObject("Amount");
            amountObj.transform.SetParent(panel.transform, false);

            LayoutElement amountLE = amountObj.AddComponent<LayoutElement>();
            amountLE.preferredHeight = 25;

            TextMeshProUGUI amount = amountObj.AddComponent<TextMeshProUGUI>();
            amount.text = "Ganado este año: $623.50";
            amount.fontSize = 16;
            amount.color = RED;
            amount.fontStyle = FontStyles.Bold;
            amount.alignment = TextAlignmentOptions.Center;

            // Button
            GameObject btnObj = new GameObject("CompleteButton");
            btnObj.transform.SetParent(panel.transform, false);

            LayoutElement btnLE = btnObj.AddComponent<LayoutElement>();
            btnLE.preferredHeight = 50;
            btnLE.preferredWidth = 220;

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

        #endregion
    }
}
