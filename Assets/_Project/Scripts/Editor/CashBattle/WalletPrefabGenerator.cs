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

            // === Description ===
            GameObject descObj = CreateChild("Description", root.transform);
            RectTransform descRT = descObj.GetComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0, 0.5f);
            descRT.anchorMax = new Vector2(1, 1);
            descRT.pivot = new Vector2(0, 1);
            descRT.offsetMin = new Vector2(24, 0);
            descRT.offsetMax = new Vector2(-195, -24);

            TextMeshProUGUI descTMP = descObj.AddComponent<TextMeshProUGUI>();
            descTMP.text = "Description";
            descTMP.fontSize = FontSizes.Body;
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
            dateRT.offsetMin = new Vector2(24, 24);
            dateRT.offsetMax = new Vector2(-195, 0);

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
            amountRT.sizeDelta = new Vector2(185, 0);
            amountRT.anchoredPosition = new Vector2(-12, -20);

            TextMeshProUGUI amountTMP = amountObj.AddComponent<TextMeshProUGUI>();
            amountTMP.text = "$0.00";
            amountTMP.fontSize = FontSizes.Body;
            amountTMP.color = GREEN;
            amountTMP.fontStyle = FontStyles.Bold;
            amountTMP.alignment = TextAlignmentOptions.Right;
            amountTMP.enableWordWrapping = false;
            amountTMP.overflowMode = TextOverflowModes.Ellipsis;
            amountTMP.enableAutoSizing = true;
            amountTMP.fontSizeMin = FontSizes.AutoMinBody;
            amountTMP.fontSizeMax = FontSizes.Body;

            // === Status Text ===
            GameObject statusObj = CreateChild("Status", root.transform);
            RectTransform statusRT = statusObj.GetComponent<RectTransform>();
            statusRT.anchorMin = new Vector2(1, 0);
            statusRT.anchorMax = new Vector2(1, 0.5f);
            statusRT.pivot = new Vector2(1, 0);
            statusRT.sizeDelta = new Vector2(185, 0);
            statusRT.anchoredPosition = new Vector2(-12, 20);

            TextMeshProUGUI statusTMP = statusObj.AddComponent<TextMeshProUGUI>();
            statusTMP.text = "";
            statusTMP.fontSize = FontSizes.Body;
            statusTMP.color = TEXT_SECONDARY;
            statusTMP.fontStyle = FontStyles.Bold;
            statusTMP.alignment = TextAlignmentOptions.Right;
            statusTMP.enableWordWrapping = false;
            statusTMP.overflowMode = TextOverflowModes.Ellipsis;
            statusTMP.enableAutoSizing = true;
            statusTMP.fontSizeMin = FontSizes.AutoMinBody;
            statusTMP.fontSizeMax = FontSizes.Body;

            // Connect references to script using SerializedObject
            SerializedObject so = new SerializedObject(itemUI);
            so.FindProperty("_background").objectReferenceValue = bg;
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
            rootRT.sizeDelta = new Vector2(0, 150);

            // Background
            Image bg = root.AddComponent<Image>();
            bg.color = CARD_BG;

            // Layout Element
            LayoutElement le = root.AddComponent<LayoutElement>();
            le.preferredHeight = 150;
            le.minHeight = 140;
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

            // === Status Indicator (green left bar, same as transaction cards) ===
            GameObject statusIndicator = CreateChild("StatusIndicator", root.transform);
            RectTransform indicatorRT = statusIndicator.GetComponent<RectTransform>();
            indicatorRT.anchorMin = new Vector2(0, 0);
            indicatorRT.anchorMax = new Vector2(0, 1);
            indicatorRT.pivot = new Vector2(0, 0.5f);
            indicatorRT.sizeDelta = new Vector2(8, 0);
            indicatorRT.anchoredPosition = Vector2.zero;

            Image indicatorImg = statusIndicator.AddComponent<Image>();
            indicatorImg.color = GREEN;

            // === Amount Text (green, top half) ===
            GameObject amountObj = CreateChild("AmountText", root.transform);
            RectTransform amountRT = amountObj.GetComponent<RectTransform>();
            amountRT.anchorMin = new Vector2(0, 0.5f);
            amountRT.anchorMax = new Vector2(1, 1);
            amountRT.pivot = new Vector2(0, 1);
            amountRT.offsetMin = new Vector2(24, 0);
            amountRT.offsetMax = new Vector2(-16, -20);

            TextMeshProUGUI amountTMP = amountObj.AddComponent<TextMeshProUGUI>();
            amountTMP.text = "$10.00";
            amountTMP.fontSize = FontSizes.Subtitle;
            amountTMP.color = GREEN;
            amountTMP.fontStyle = FontStyles.Bold;
            amountTMP.alignment = TextAlignmentOptions.Left;
            amountTMP.enableAutoSizing = true;
            amountTMP.fontSizeMin = FontSizes.AutoMinBody;
            amountTMP.fontSizeMax = FontSizes.Subtitle;
            amountTMP.overflowMode = TextOverflowModes.Ellipsis;

            // === Label (bottom half) ===
            GameObject labelObj = CreateChild("Label", root.transform);
            RectTransform labelRT = labelObj.GetComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 0);
            labelRT.anchorMax = new Vector2(1, 0.5f);
            labelRT.pivot = new Vector2(0, 0);
            labelRT.offsetMin = new Vector2(24, 20);
            labelRT.offsetMax = new Vector2(-16, 0);

            TextMeshProUGUI labelTMP = labelObj.AddComponent<TextMeshProUGUI>();
            labelTMP.text = "Deposit";
            labelTMP.fontSize = FontSizes.Body;
            labelTMP.color = TEXT_SECONDARY;
            labelTMP.fontStyle = FontStyles.Bold;
            labelTMP.alignment = TextAlignmentOptions.Left;

            // Connect references to script
            SerializedObject so = new SerializedObject(optionUI);
            so.FindProperty("_button").objectReferenceValue = btn;
            so.FindProperty("_background").objectReferenceValue = bg;
            so.FindProperty("_amountText").objectReferenceValue = amountTMP;
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
