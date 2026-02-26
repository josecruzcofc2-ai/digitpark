using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;
using DigitPark.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// Generador de Prefabs para CashWallet.
    /// Crea los prefabs necesarios para que funcione la escena.
    /// Menu: DigitPark > Prefab Generators > CashWallet Prefabs
    /// </summary>
    public class WalletPrefabGenerator : EditorWindow
    {
        #region Colors

        private static readonly Color CARD_BG = new Color(0.09f, 0.10f, 0.13f, 1f);
        private static readonly Color GREEN = new Color(0.2f, 0.95f, 0.4f, 1f);
        private static readonly Color CYAN = new Color(0f, 0.85f, 1f, 1f);
        private static readonly Color RED = new Color(1f, 0.35f, 0.35f, 1f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color TEXT_WHITE = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.55f, 0.55f, 0.6f, 1f);

        #endregion

        #region Paths

        private static readonly string PREFABS_PATH = "Assets/_Project/Prefabs/CashBattle/Wallet/";
        private static readonly string ICONS_PATH = "Assets/_Project/Art/Icons/CashBattle/Wallet/";

        #endregion

        [MenuItem("DigitPark/Prefab Generators/CashWallet Prefabs", false, 300)]
        public static void ShowWindow()
        {
            GetWindow<WalletPrefabGenerator>("Wallet Prefabs Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("CashWallet Prefab Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox(
                "Genera los prefabs necesarios para CashWallet:\n\n" +
                "• TransactionItemUI.prefab - Card de transacción\n" +
                "• DepositOptionUI.prefab - Opción de depósito\n\n" +
                "Los prefabs se guardan en:\n" + PREFABS_PATH,
                MessageType.Info);

            EditorGUILayout.Space(15);

            // Ensure directory exists
            if (!Directory.Exists(PREFABS_PATH))
            {
                EditorGUILayout.HelpBox("La carpeta de prefabs no existe. Se creará automáticamente.", MessageType.Warning);
            }

            EditorGUILayout.Space(10);

            GUI.backgroundColor = new Color(0.2f, 0.9f, 0.4f, 1f);
            if (GUILayout.Button("GENERAR TODOS LOS PREFABS", GUILayout.Height(45)))
            {
                GenerateAllPrefabs();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(20);
            GUILayout.Label("Generar Individualmente:", EditorStyles.boldLabel);

            if (GUILayout.Button("TransactionItemUI Prefab", GUILayout.Height(30)))
            {
                GenerateTransactionItemPrefab();
            }

            if (GUILayout.Button("DepositOptionUI Prefab", GUILayout.Height(30)))
            {
                GenerateDepositOptionPrefab();
            }
        }

        private static void GenerateAllPrefabs()
        {
            EnsureDirectoryExists();

            GenerateTransactionItemPrefab();
            GenerateDepositOptionPrefab();

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Prefabs Generados",
                "Todos los prefabs de CashWallet han sido generados en:\n" + PREFABS_PATH,
                "OK");
        }

        private static void EnsureDirectoryExists()
        {
            if (!Directory.Exists(PREFABS_PATH))
            {
                Directory.CreateDirectory(PREFABS_PATH);
                AssetDatabase.Refresh();
            }
        }

        #region Transaction Item Prefab

        [MenuItem("DigitPark/Prefab Generators/Generate TransactionItemUI", false, 301)]
        public static void GenerateTransactionItemPrefab()
        {
            EnsureDirectoryExists();

            // Create root GameObject
            GameObject root = new GameObject("TransactionItemUI");

            RectTransform rootRT = root.AddComponent<RectTransform>();
            rootRT.sizeDelta = new Vector2(0, 150);

            // Background
            Image bg = root.AddComponent<Image>();
            bg.color = CARD_BG;

            // Layout Element
            LayoutElement le = root.AddComponent<LayoutElement>();
            le.preferredHeight = 150;
            le.minHeight = 140;
            le.flexibleWidth = 1;

            // Add the script component
            var itemUI = root.AddComponent<CashBattle.TransactionItemUI>();

            // === Left Color Bar (Status Indicator) ===
            GameObject statusIndicator = CreateChild("StatusIndicator", root.transform);
            RectTransform indicatorRT = statusIndicator.GetComponent<RectTransform>();
            indicatorRT.anchorMin = new Vector2(0, 0);
            indicatorRT.anchorMax = new Vector2(0, 1);
            indicatorRT.pivot = new Vector2(0, 0.5f);
            indicatorRT.sizeDelta = new Vector2(8, 0);
            indicatorRT.anchoredPosition = Vector2.zero;

            Image indicatorImg = statusIndicator.AddComponent<Image>();
            indicatorImg.color = GREEN;

            // === Type Icon ===
            GameObject typeIcon = CreateChild("TypeIcon", root.transform);
            RectTransform iconRT = typeIcon.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0, 0.5f);
            iconRT.anchorMax = new Vector2(0, 0.5f);
            iconRT.pivot = new Vector2(0, 0.5f);
            iconRT.sizeDelta = new Vector2(90, 90);
            iconRT.anchoredPosition = new Vector2(32, 0);

            Image iconImg = typeIcon.AddComponent<Image>();
            iconImg.color = GREEN;
            iconImg.preserveAspect = true;

            // Load icon if exists
            Sprite depositIcon = AssetDatabase.LoadAssetAtPath<Sprite>(ICONS_PATH + "TxDepositIcon.png");
            if (depositIcon != null) iconImg.sprite = depositIcon;

            // === Description ===
            GameObject descObj = CreateChild("Description", root.transform);
            RectTransform descRT = descObj.GetComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0, 0.5f);
            descRT.anchorMax = new Vector2(1, 1);
            descRT.pivot = new Vector2(0, 1);
            descRT.offsetMin = new Vector2(144, 0);
            descRT.offsetMax = new Vector2(-100, -24);

            TextMeshProUGUI descTMP = descObj.AddComponent<TextMeshProUGUI>();
            descTMP.text = "Description";
            descTMP.fontSize = FontSizes.BodyLarge;
            descTMP.color = TEXT_WHITE;
            descTMP.fontStyle = FontStyles.Bold;
            descTMP.alignment = TextAlignmentOptions.Left;
            descTMP.overflowMode = TextOverflowModes.Ellipsis;

            // === Date ===
            GameObject dateObj = CreateChild("Date", root.transform);
            RectTransform dateRT = dateObj.GetComponent<RectTransform>();
            dateRT.anchorMin = new Vector2(0, 0);
            dateRT.anchorMax = new Vector2(1, 0.5f);
            dateRT.pivot = new Vector2(0, 0);
            dateRT.offsetMin = new Vector2(144, 24);
            dateRT.offsetMax = new Vector2(-100, 0);

            TextMeshProUGUI dateTMP = dateObj.AddComponent<TextMeshProUGUI>();
            dateTMP.text = "Date";
            dateTMP.fontSize = FontSizes.Body;
            dateTMP.color = TEXT_SECONDARY;
            dateTMP.fontStyle = FontStyles.Bold;
            dateTMP.alignment = TextAlignmentOptions.Left;

            // === Amount ===
            GameObject amountObj = CreateChild("Amount", root.transform);
            RectTransform amountRT = amountObj.GetComponent<RectTransform>();
            amountRT.anchorMin = new Vector2(1, 0.5f);
            amountRT.anchorMax = new Vector2(1, 1);
            amountRT.pivot = new Vector2(1, 1);
            amountRT.sizeDelta = new Vector2(90, 0);
            amountRT.anchoredPosition = new Vector2(-24, -20);

            TextMeshProUGUI amountTMP = amountObj.AddComponent<TextMeshProUGUI>();
            amountTMP.text = "$0.00";
            amountTMP.fontSize = FontSizes.Button;
            amountTMP.color = GREEN;
            amountTMP.fontStyle = FontStyles.Bold;
            amountTMP.alignment = TextAlignmentOptions.Right;

            // === Status Text ===
            GameObject statusObj = CreateChild("Status", root.transform);
            RectTransform statusRT = statusObj.GetComponent<RectTransform>();
            statusRT.anchorMin = new Vector2(1, 0);
            statusRT.anchorMax = new Vector2(1, 0.5f);
            statusRT.pivot = new Vector2(1, 0);
            statusRT.sizeDelta = new Vector2(90, 0);
            statusRT.anchoredPosition = new Vector2(-24, 20);

            TextMeshProUGUI statusTMP = statusObj.AddComponent<TextMeshProUGUI>();
            statusTMP.text = "";
            statusTMP.fontSize = FontSizes.Body;
            statusTMP.color = TEXT_SECONDARY;
            statusTMP.fontStyle = FontStyles.Bold;
            statusTMP.alignment = TextAlignmentOptions.Right;

            // Connect references to script using SerializedObject
            SerializedObject so = new SerializedObject(itemUI);
            so.FindProperty("_background").objectReferenceValue = bg;
            so.FindProperty("_typeIcon").objectReferenceValue = iconImg;
            so.FindProperty("_descriptionText").objectReferenceValue = descTMP;
            so.FindProperty("_dateText").objectReferenceValue = dateTMP;
            so.FindProperty("_amountText").objectReferenceValue = amountTMP;
            so.FindProperty("_statusText").objectReferenceValue = statusTMP;
            so.FindProperty("_statusIndicator").objectReferenceValue = indicatorImg;
            so.ApplyModifiedProperties();

            // Save as prefab
            string prefabPath = PREFABS_PATH + "TransactionItemUI.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            DestroyImmediate(root);

            Debug.Log($"[WalletPrefabGenerator] TransactionItemUI.prefab creado en {prefabPath}");
        }

        #endregion

        #region Deposit Option Prefab

        [MenuItem("DigitPark/Prefab Generators/Generate DepositOptionUI", false, 302)]
        public static void GenerateDepositOptionPrefab()
        {
            EnsureDirectoryExists();

            // Create root GameObject
            GameObject root = new GameObject("DepositOptionUI");

            RectTransform rootRT = root.AddComponent<RectTransform>();
            rootRT.sizeDelta = new Vector2(0, 90);

            // Background
            Image bg = root.AddComponent<Image>();
            bg.color = CARD_BG;

            // Layout Element
            LayoutElement le = root.AddComponent<LayoutElement>();
            le.preferredHeight = 90;
            le.flexibleWidth = 1;

            // Button
            Button btn = root.AddComponent<Button>();
            btn.targetGraphic = bg;
            ColorBlock colors = btn.colors;
            colors.highlightedColor = new Color(0.12f, 0.14f, 0.18f, 1f);
            colors.pressedColor = new Color(0.06f, 0.08f, 0.1f, 1f);
            btn.colors = colors;

            // Add the script component
            var optionUI = root.AddComponent<CashBattle.DepositOptionUI>();

            // === Outline Image (for selected/popular state) ===
            GameObject outlineObj = CreateChild("Outline", root.transform);
            RectTransform outlineRT = outlineObj.GetComponent<RectTransform>();
            outlineRT.anchorMin = Vector2.zero;
            outlineRT.anchorMax = Vector2.one;
            outlineRT.sizeDelta = new Vector2(4, 4);
            outlineRT.anchoredPosition = Vector2.zero;

            Image outlineImg = outlineObj.AddComponent<Image>();
            outlineImg.color = GOLD;
            outlineImg.type = Image.Type.Sliced;
            outlineImg.fillCenter = false;
            outlineObj.SetActive(false);

            // === Content Layout ===
            HorizontalLayoutGroup hlg = root.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(15, 15, 10, 10);
            hlg.spacing = 8;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            // === Amount Section ===
            GameObject amountSection = CreateChild("AmountSection", root.transform);
            LayoutElement amountLE = amountSection.AddComponent<LayoutElement>();
            amountLE.flexibleWidth = 1;

            VerticalLayoutGroup amountVLG = amountSection.AddComponent<VerticalLayoutGroup>();
            amountVLG.childAlignment = TextAnchor.MiddleLeft;
            amountVLG.childForceExpandHeight = false;
            amountVLG.childForceExpandWidth = true;
            amountVLG.childControlHeight = false;
            amountVLG.childControlWidth = true;
            amountVLG.spacing = 2;

            // Amount text
            GameObject amountObj = CreateChild("AmountText", amountSection.transform);
            LayoutElement amountTextLE = amountObj.AddComponent<LayoutElement>();
            amountTextLE.preferredHeight = 50;

            TextMeshProUGUI amountTMP = amountObj.AddComponent<TextMeshProUGUI>();
            amountTMP.text = "$10.00";
            amountTMP.fontSize = FontSizes.ValueMedium;
            amountTMP.color = TEXT_WHITE;
            amountTMP.fontStyle = FontStyles.Bold;
            amountTMP.alignment = TextAlignmentOptions.Left;
            amountTMP.enableAutoSizing = true;
            amountTMP.fontSizeMin = FontSizes.AutoMinBody;
            amountTMP.fontSizeMax = FontSizes.ValueMedium;
            amountTMP.overflowMode = TextOverflowModes.Ellipsis;

            // Label "Depositar"
            GameObject labelObj = CreateChild("Label", amountSection.transform);
            LayoutElement labelLE = labelObj.AddComponent<LayoutElement>();
            labelLE.preferredHeight = 22;

            TextMeshProUGUI labelTMP = labelObj.AddComponent<TextMeshProUGUI>();
            labelTMP.text = "Deposit";
            labelTMP.fontSize = FontSizes.Body;
            labelTMP.color = TEXT_SECONDARY;
            labelTMP.alignment = TextAlignmentOptions.Left;
            labelTMP.enableAutoSizing = true;
            labelTMP.fontSizeMin = FontSizes.AutoMinTiny;
            labelTMP.fontSizeMax = FontSizes.Body;

            // === Bonus Text ===
            GameObject bonusObj = CreateChild("BonusText", root.transform);
            LayoutElement bonusLE = bonusObj.AddComponent<LayoutElement>();
            bonusLE.preferredWidth = 150;
            bonusLE.flexibleWidth = 0;

            TextMeshProUGUI bonusTMP = bonusObj.AddComponent<TextMeshProUGUI>();
            bonusTMP.text = "+$0.50 BONUS";
            bonusTMP.fontSize = FontSizes.Body;
            bonusTMP.color = GOLD;
            bonusTMP.fontStyle = FontStyles.Bold;
            bonusTMP.alignment = TextAlignmentOptions.Right;
            bonusTMP.enableAutoSizing = true;
            bonusTMP.fontSizeMin = FontSizes.AutoMinTiny;
            bonusTMP.fontSizeMax = FontSizes.Body;
            bonusTMP.overflowMode = TextOverflowModes.Ellipsis;

            // === Popular Badge (hidden by default) ===
            GameObject popularBadge = CreateChild("PopularBadge", root.transform);
            RectTransform badgeRT = popularBadge.GetComponent<RectTransform>();
            badgeRT.anchorMin = new Vector2(1, 1);
            badgeRT.anchorMax = new Vector2(1, 1);
            badgeRT.pivot = new Vector2(1, 1);
            badgeRT.sizeDelta = new Vector2(110, 35);
            badgeRT.anchoredPosition = new Vector2(0, 0);

            Image badgeBg = popularBadge.AddComponent<Image>();
            badgeBg.color = GREEN;

            GameObject badgeText = CreateChild("Text", popularBadge.transform);
            RectTransform badgeTextRT = badgeText.GetComponent<RectTransform>();
            badgeTextRT.anchorMin = Vector2.zero;
            badgeTextRT.anchorMax = Vector2.one;
            badgeTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI badgeTMP = badgeText.AddComponent<TextMeshProUGUI>();
            badgeTMP.text = "POPULAR";
            badgeTMP.fontSize = FontSizes.Body;
            badgeTMP.color = Color.black;
            badgeTMP.fontStyle = FontStyles.Bold;
            badgeTMP.alignment = TextAlignmentOptions.Center;

            popularBadge.SetActive(false);

            // Connect references to script
            SerializedObject so = new SerializedObject(optionUI);
            so.FindProperty("_button").objectReferenceValue = btn;
            so.FindProperty("_background").objectReferenceValue = bg;
            so.FindProperty("_amountText").objectReferenceValue = amountTMP;
            so.FindProperty("_bonusText").objectReferenceValue = bonusTMP;
            so.FindProperty("_popularBadge").objectReferenceValue = popularBadge;
            so.FindProperty("_outline").objectReferenceValue = outlineImg;
            so.ApplyModifiedProperties();

            // Save as prefab
            string prefabPath = PREFABS_PATH + "DepositOptionUI.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            DestroyImmediate(root);

            Debug.Log($"[WalletPrefabGenerator] DepositOptionUI.prefab creado en {prefabPath}");
        }

        #endregion

        #region Helpers

        private static GameObject CreateChild(string name, Transform parent)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            return obj;
        }

        #endregion
    }
}
