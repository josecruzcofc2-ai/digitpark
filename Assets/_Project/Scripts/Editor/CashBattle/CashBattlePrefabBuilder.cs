using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para crear los prefabs del sistema CashBattle.
    /// Genera DepositOptionUI, TransactionItemUI, y HistoryEntryItemUI.
    /// </summary>
    public class CashBattlePrefabBuilder : EditorWindow
    {
        private const string PREFAB_PATH = "Assets/_Project/Prefabs/CashBattle";

        // Colors
        private static readonly Color BG_DARK = new Color(0.1f, 0.12f, 0.15f, 0.95f);
        private static readonly Color CYAN = new Color(0f, 0.83f, 1f, 1f);
        private static readonly Color GREEN = new Color(0f, 1f, 0.5f, 1f);
        private static readonly Color RED = new Color(1f, 0.4f, 0.4f, 1f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color TEXT_WHITE = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_GRAY = new Color(0.6f, 0.6f, 0.6f, 1f);

        [MenuItem("DigitPark/Prefabs/CashBattle/Create All Prefabs")]
        public static void CreateAllPrefabs()
        {
            EnsurePrefabDirectory();
            CreateDepositOptionPrefab();
            CreateTransactionItemPrefab();
            CreateHistoryEntryItemPrefab();
            AssetDatabase.Refresh();
            Debug.Log("[CashBattlePrefabBuilder] Todos los prefabs creados exitosamente!");
        }

        [MenuItem("DigitPark/Prefabs/CashBattle/Create DepositOptionUI Prefab")]
        public static void CreateDepositOptionPrefab()
        {
            EnsurePrefabDirectory();

            // Root
            GameObject root = new GameObject("DepositOptionUI");
            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(300, 100);

            Image rootImage = root.AddComponent<Image>();
            rootImage.color = BG_DARK;

            Button rootButton = root.AddComponent<Button>();
            rootButton.targetGraphic = rootImage;

            // Add DepositOptionUI component
            var depositOptionUI = root.AddComponent<CashBattle.DepositOptionUI>();

            // Background
            GameObject bg = CreateUIElement("Background", root.transform, new Vector2(0, 0), rootRect.sizeDelta);
            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = BG_DARK;
            bgImage.raycastTarget = false;
            StretchToParent(bg.GetComponent<RectTransform>());

            // Border/Outline
            GameObject border = CreateUIElement("BorderImage", root.transform, Vector2.zero, rootRect.sizeDelta);
            Image borderImage = border.AddComponent<Image>();
            borderImage.color = GOLD;
            borderImage.raycastTarget = false;
            Outline borderOutline = border.AddComponent<Outline>();
            borderOutline.effectColor = GOLD;
            borderOutline.effectDistance = new Vector2(2, 2);
            StretchToParent(border.GetComponent<RectTransform>());
            border.SetActive(false); // Hidden by default

            // Amount Text
            GameObject amountObj = CreateUIElement("AmountText", root.transform, new Vector2(-50, 10), new Vector2(150, 50));
            TextMeshProUGUI amountText = amountObj.AddComponent<TextMeshProUGUI>();
            amountText.text = "$10.00";
            amountText.fontSize = 32;
            amountText.fontStyle = FontStyles.Bold;
            amountText.color = TEXT_WHITE;
            amountText.alignment = TextAlignmentOptions.Left;

            // Bonus Container
            GameObject bonusContainer = CreateUIElement("BonusContainer", root.transform, new Vector2(-50, -20), new Vector2(150, 30));

            // Bonus Text
            GameObject bonusTextObj = CreateUIElement("BonusText", bonusContainer.transform, Vector2.zero, new Vector2(150, 30));
            TextMeshProUGUI bonusText = bonusTextObj.AddComponent<TextMeshProUGUI>();
            bonusText.text = "+$0.50 BONUS";
            bonusText.fontSize = 16;
            bonusText.color = GREEN;
            bonusText.alignment = TextAlignmentOptions.Left;
            StretchToParent(bonusTextObj.GetComponent<RectTransform>());

            // Popular Badge
            GameObject popularBadge = CreateUIElement("PopularBadge", root.transform, new Vector2(100, 30), new Vector2(80, 25));
            Image popularBadgeImg = popularBadge.AddComponent<Image>();
            popularBadgeImg.color = CYAN;

            GameObject popularText = CreateUIElement("Text", popularBadge.transform, Vector2.zero, new Vector2(80, 25));
            TextMeshProUGUI popularTMP = popularText.AddComponent<TextMeshProUGUI>();
            popularTMP.text = "POPULAR";
            popularTMP.fontSize = 12;
            popularTMP.fontStyle = FontStyles.Bold;
            popularTMP.color = Color.white;
            popularTMP.alignment = TextAlignmentOptions.Center;
            StretchToParent(popularText.GetComponent<RectTransform>());
            popularBadge.SetActive(false);

            // Select Button
            GameObject selectBtn = CreateUIElement("SelectButton", root.transform, new Vector2(100, -10), new Vector2(100, 40));
            Image selectBtnImg = selectBtn.AddComponent<Image>();
            selectBtnImg.color = CYAN;
            Button selectButton = selectBtn.AddComponent<Button>();
            selectButton.targetGraphic = selectBtnImg;

            GameObject selectBtnText = CreateUIElement("Text", selectBtn.transform, Vector2.zero, new Vector2(100, 40));
            TextMeshProUGUI selectTMP = selectBtnText.AddComponent<TextMeshProUGUI>();
            selectTMP.text = "Depositar";
            selectTMP.fontSize = 14;
            selectTMP.fontStyle = FontStyles.Bold;
            selectTMP.color = Color.white;
            selectTMP.alignment = TextAlignmentOptions.Center;
            StretchToParent(selectBtnText.GetComponent<RectTransform>());

            // Save prefab
            string path = $"{PREFAB_PATH}/DepositOptionUI.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, path);
            DestroyImmediate(root);

            Debug.Log($"[CashBattlePrefabBuilder] DepositOptionUI prefab creado en: {path}");
        }

        [MenuItem("DigitPark/Prefabs/CashBattle/Create TransactionItemUI Prefab")]
        public static void CreateTransactionItemPrefab()
        {
            EnsurePrefabDirectory();

            // Root
            GameObject root = new GameObject("TransactionItemUI");
            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(350, 70);

            Image rootImage = root.AddComponent<Image>();
            rootImage.color = BG_DARK;

            // Add TransactionItemUI component
            var transactionItemUI = root.AddComponent<CashBattle.TransactionItemUI>();

            // Type Icon
            GameObject typeIcon = CreateUIElement("TypeIcon", root.transform, new Vector2(-140, 0), new Vector2(40, 40));
            Image typeIconImg = typeIcon.AddComponent<Image>();
            typeIconImg.color = GREEN;

            // Description
            GameObject descObj = CreateUIElement("Description", root.transform, new Vector2(-30, 10), new Vector2(180, 25));
            TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = "Depósito";
            descText.fontSize = 18;
            descText.fontStyle = FontStyles.Bold;
            descText.color = TEXT_WHITE;
            descText.alignment = TextAlignmentOptions.Left;

            // Date
            GameObject dateObj = CreateUIElement("Date", root.transform, new Vector2(-30, -15), new Vector2(180, 20));
            TextMeshProUGUI dateText = dateObj.AddComponent<TextMeshProUGUI>();
            dateText.text = "Hace 2h";
            dateText.fontSize = 14;
            dateText.color = TEXT_GRAY;
            dateText.alignment = TextAlignmentOptions.Left;

            // Amount
            GameObject amountObj = CreateUIElement("Amount", root.transform, new Vector2(120, 10), new Vector2(100, 25));
            TextMeshProUGUI amountText = amountObj.AddComponent<TextMeshProUGUI>();
            amountText.text = "+$10.00";
            amountText.fontSize = 18;
            amountText.fontStyle = FontStyles.Bold;
            amountText.color = GREEN;
            amountText.alignment = TextAlignmentOptions.Right;

            // Status
            GameObject statusObj = CreateUIElement("Status", root.transform, new Vector2(120, -15), new Vector2(100, 20));
            TextMeshProUGUI statusText = statusObj.AddComponent<TextMeshProUGUI>();
            statusText.text = "Completado";
            statusText.fontSize = 12;
            statusText.color = GREEN;
            statusText.alignment = TextAlignmentOptions.Right;

            // Status Indicator
            GameObject statusIndicator = CreateUIElement("StatusIndicator", root.transform, new Vector2(160, -15), new Vector2(10, 10));
            Image statusImg = statusIndicator.AddComponent<Image>();
            statusImg.color = GREEN;
            statusIndicator.SetActive(false);

            // Save prefab
            string path = $"{PREFAB_PATH}/TransactionItemUI.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, path);
            DestroyImmediate(root);

            Debug.Log($"[CashBattlePrefabBuilder] TransactionItemUI prefab creado en: {path}");
        }

        [MenuItem("DigitPark/Prefabs/CashBattle/Create HistoryEntryItemUI Prefab")]
        public static void CreateHistoryEntryItemPrefab()
        {
            EnsurePrefabDirectory();

            // Root
            GameObject root = new GameObject("HistoryEntryItemUI");
            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(350, 90);

            Image rootImage = root.AddComponent<Image>();
            rootImage.color = BG_DARK;

            Button rootButton = root.AddComponent<Button>();
            rootButton.targetGraphic = rootImage;

            // Add HistoryEntryItemUI component
            var historyEntryUI = root.AddComponent<CashBattle.HistoryEntryItemUI>();

            // Result Indicator (left bar)
            GameObject resultIndicator = CreateUIElement("ResultIndicator", root.transform, new Vector2(-170, 0), new Vector2(6, 80));
            Image resultIndicatorImg = resultIndicator.AddComponent<Image>();
            resultIndicatorImg.color = GREEN;

            // Game Type Icon
            GameObject gameIcon = CreateUIElement("GameTypeIcon", root.transform, new Vector2(-130, 0), new Vector2(40, 40));
            Image gameIconImg = gameIcon.AddComponent<Image>();
            gameIconImg.color = CYAN;

            // Title
            GameObject titleObj = CreateUIElement("TitleText", root.transform, new Vector2(-20, 20), new Vector2(180, 25));
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "1v1 - Memory";
            titleText.fontSize = 18;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = TEXT_WHITE;
            titleText.alignment = TextAlignmentOptions.Left;

            // Subtitle
            GameObject subtitleObj = CreateUIElement("SubtitleText", root.transform, new Vector2(-20, 0), new Vector2(180, 20));
            TextMeshProUGUI subtitleText = subtitleObj.AddComponent<TextMeshProUGUI>();
            subtitleText.text = "vs Player123";
            subtitleText.fontSize = 14;
            subtitleText.color = TEXT_GRAY;
            subtitleText.alignment = TextAlignmentOptions.Left;

            // Date
            GameObject dateObj = CreateUIElement("DateText", root.transform, new Vector2(-20, -22), new Vector2(180, 18));
            TextMeshProUGUI dateText = dateObj.AddComponent<TextMeshProUGUI>();
            dateText.text = "Hace 1h";
            dateText.fontSize = 12;
            dateText.color = TEXT_GRAY;
            dateText.alignment = TextAlignmentOptions.Left;

            // Result Badge
            GameObject resultBadge = CreateUIElement("ResultBadge", root.transform, new Vector2(110, 25), new Vector2(80, 25));
            Image resultBadgeImg = resultBadge.AddComponent<Image>();
            resultBadgeImg.color = new Color(0, 0.3f, 0, 0.5f);

            // Result Text
            GameObject resultTextObj = CreateUIElement("ResultText", resultBadge.transform, Vector2.zero, new Vector2(80, 25));
            TextMeshProUGUI resultText = resultTextObj.AddComponent<TextMeshProUGUI>();
            resultText.text = "VICTORIA";
            resultText.fontSize = 12;
            resultText.fontStyle = FontStyles.Bold;
            resultText.color = GREEN;
            resultText.alignment = TextAlignmentOptions.Center;
            StretchToParent(resultTextObj.GetComponent<RectTransform>());

            // Score
            GameObject scoreObj = CreateUIElement("ScoreText", root.transform, new Vector2(110, 0), new Vector2(80, 20));
            TextMeshProUGUI scoreText = scoreObj.AddComponent<TextMeshProUGUI>();
            scoreText.text = "2500 - 1800";
            scoreText.fontSize = 14;
            scoreText.color = TEXT_WHITE;
            scoreText.alignment = TextAlignmentOptions.Center;

            // Net Result
            GameObject netObj = CreateUIElement("NetResultText", root.transform, new Vector2(110, -22), new Vector2(80, 22));
            TextMeshProUGUI netText = netObj.AddComponent<TextMeshProUGUI>();
            netText.text = "+$4.50";
            netText.fontSize = 16;
            netText.fontStyle = FontStyles.Bold;
            netText.color = GREEN;
            netText.alignment = TextAlignmentOptions.Center;

            // Save prefab
            string path = $"{PREFAB_PATH}/HistoryEntryItemUI.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, path);
            DestroyImmediate(root);

            Debug.Log($"[CashBattlePrefabBuilder] HistoryEntryItemUI prefab creado en: {path}");
        }

        // ==================== HELPER METHODS ====================

        private static void EnsurePrefabDirectory()
        {
            if (!Directory.Exists(PREFAB_PATH))
            {
                Directory.CreateDirectory(PREFAB_PATH);
                AssetDatabase.Refresh();
            }
        }

        private static GameObject CreateUIElement(string name, Transform parent, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            return obj;
        }

        private static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
