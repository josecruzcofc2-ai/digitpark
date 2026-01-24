using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para crear los prefabs PREMIUM del sistema CashBattle.
    /// Genera DepositOptionUI, TransactionItemUI, y HistoryEntryItemUI con iconos premium.
    /// </summary>
    public class CashBattlePrefabBuilder : EditorWindow
    {
        private const string PREFAB_PATH = "Assets/_Project/Prefabs/CashBattle";
        private const string WALLET_ICONS_PATH = "Assets/_Project/Art/Icons/CashBattle/Wallet/";
        private const string GAME_ICONS_PATH = "Assets/_Project/Art/Icons/Games/CashBattle/";
        private const string STAT_ICONS_PATH = "Assets/_Project/Art/Icons/CashBattle/Stats/";
        private const string TOURNAMENT_ICONS_PATH = "Assets/_Project/Art/Icons/CashBattle/Tournaments/";

        // Colors - Premium Theme
        private static readonly Color CARD_BG = new Color(0.1f, 0.12f, 0.16f, 1f);
        private static readonly Color CARD_BG_DARK = new Color(0.08f, 0.1f, 0.13f, 1f);
        private static readonly Color CYAN = new Color(0f, 0.9f, 1f, 1f);
        private static readonly Color GREEN = new Color(0.2f, 0.95f, 0.4f, 1f);
        private static readonly Color RED = new Color(1f, 0.4f, 0.4f, 1f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color TEXT_WHITE = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.5f, 0.5f, 0.55f, 1f);

        [MenuItem("DigitPark/Prefabs/CashBattle/Create All Prefabs (Premium)", false, 100)]
        public static void CreateAllPrefabs()
        {
            EnsurePrefabDirectory();
            CreateDepositOptionPrefab();
            CreateTransactionItemPrefab();
            CreateHistoryEntryItemPrefab();
            CreateTournamentCardPrefab();
            AssetDatabase.Refresh();
            Debug.Log("[CashBattlePrefabBuilder] Todos los prefabs PREMIUM creados!");
        }

        [MenuItem("DigitPark/Prefabs/CashBattle/TransactionItemUI Prefab", false, 110)]
        public static void CreateTransactionItemPrefab()
        {
            EnsurePrefabDirectory();

            GameObject root = new GameObject("TransactionItemUI");
            RectTransform rootRT = root.AddComponent<RectTransform>();
            rootRT.sizeDelta = new Vector2(0, 75);

            LayoutElement rootLE = root.AddComponent<LayoutElement>();
            rootLE.preferredHeight = 75;
            rootLE.minHeight = 75;

            Image rootBg = root.AddComponent<Image>();
            rootBg.color = CARD_BG;

            // Añadir el componente script
            root.AddComponent<CashBattle.TransactionItemUI>();

            // === Color Bar (izquierda) ===
            GameObject colorBar = new GameObject("ColorBar");
            colorBar.transform.SetParent(root.transform, false);

            RectTransform barRT = colorBar.AddComponent<RectTransform>();
            barRT.anchorMin = new Vector2(0, 0);
            barRT.anchorMax = new Vector2(0, 1);
            barRT.pivot = new Vector2(0, 0.5f);
            barRT.sizeDelta = new Vector2(5, 0);
            barRT.anchoredPosition = Vector2.zero;

            Image barImg = colorBar.AddComponent<Image>();
            barImg.color = GREEN;

            // === Icon ===
            GameObject iconObj = new GameObject("TypeIcon");
            iconObj.transform.SetParent(root.transform, false);

            RectTransform iconRT = iconObj.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0, 0.5f);
            iconRT.anchorMax = new Vector2(0, 0.5f);
            iconRT.pivot = new Vector2(0, 0.5f);
            iconRT.sizeDelta = new Vector2(50, 50);
            iconRT.anchoredPosition = new Vector2(15, 0);

            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.preserveAspect = true;

            Sprite txIcon = AssetDatabase.LoadAssetAtPath<Sprite>(WALLET_ICONS_PATH + "TxWinIcon.png");
            if (txIcon != null)
            {
                iconImg.sprite = txIcon;
                iconImg.color = Color.white;
            }
            else
            {
                iconImg.color = GREEN;
            }

            // === Description ===
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(root.transform, false);

            RectTransform descRT = descObj.AddComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0, 0.5f);
            descRT.anchorMax = new Vector2(1, 1);
            descRT.pivot = new Vector2(0, 1);
            descRT.offsetMin = new Vector2(75, 0);
            descRT.offsetMax = new Vector2(-100, -10);

            TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = "Descripción transacción";
            descText.fontSize = 15;
            descText.color = TEXT_WHITE;
            descText.alignment = TextAlignmentOptions.Left;
            descText.overflowMode = TextOverflowModes.Ellipsis;

            // === Date ===
            GameObject dateObj = new GameObject("Date");
            dateObj.transform.SetParent(root.transform, false);

            RectTransform dateRT = dateObj.AddComponent<RectTransform>();
            dateRT.anchorMin = new Vector2(0, 0);
            dateRT.anchorMax = new Vector2(1, 0.5f);
            dateRT.pivot = new Vector2(0, 0);
            dateRT.offsetMin = new Vector2(75, 10);
            dateRT.offsetMax = new Vector2(-100, 0);

            TextMeshProUGUI dateText = dateObj.AddComponent<TextMeshProUGUI>();
            dateText.text = "Fecha";
            dateText.fontSize = 13;
            dateText.color = TEXT_SECONDARY;
            dateText.alignment = TextAlignmentOptions.Left;

            // === Amount ===
            GameObject amountObj = new GameObject("Amount");
            amountObj.transform.SetParent(root.transform, false);

            RectTransform amtRT = amountObj.AddComponent<RectTransform>();
            amtRT.anchorMin = new Vector2(1, 0);
            amtRT.anchorMax = new Vector2(1, 1);
            amtRT.pivot = new Vector2(1, 0.5f);
            amtRT.sizeDelta = new Vector2(90, 0);
            amtRT.anchoredPosition = new Vector2(-10, 0);

            TextMeshProUGUI amountText = amountObj.AddComponent<TextMeshProUGUI>();
            amountText.text = "+$0.00";
            amountText.fontSize = 20;
            amountText.fontStyle = FontStyles.Bold;
            amountText.color = GREEN;
            amountText.alignment = TextAlignmentOptions.Right;

            // Guardar prefab
            string path = $"{PREFAB_PATH}/TransactionItemUI.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, path);
            DestroyImmediate(root);

            Debug.Log($"[CashBattlePrefabBuilder] TransactionItemUI.prefab creado: {path}");
        }

        [MenuItem("DigitPark/Prefabs/CashBattle/HistoryEntryItemUI Prefab", false, 111)]
        public static void CreateHistoryEntryItemPrefab()
        {
            EnsurePrefabDirectory();

            GameObject root = new GameObject("HistoryEntryItemUI");
            RectTransform rootRT = root.AddComponent<RectTransform>();
            rootRT.sizeDelta = new Vector2(0, 90);

            LayoutElement rootLE = root.AddComponent<LayoutElement>();
            rootLE.preferredHeight = 90;
            rootLE.minHeight = 90;

            Image rootBg = root.AddComponent<Image>();
            rootBg.color = CARD_BG;

            Button rootBtn = root.AddComponent<Button>();
            rootBtn.targetGraphic = rootBg;
            ColorBlock colors = rootBtn.colors;
            colors.highlightedColor = CARD_BG * 1.3f;
            rootBtn.colors = colors;

            // Añadir componente script
            root.AddComponent<CashBattle.HistoryEntryItemUI>();

            // === Result Bar (izquierda) ===
            GameObject resultBar = new GameObject("ResultIndicator");
            resultBar.transform.SetParent(root.transform, false);

            RectTransform barRT = resultBar.AddComponent<RectTransform>();
            barRT.anchorMin = new Vector2(0, 0.1f);
            barRT.anchorMax = new Vector2(0, 0.9f);
            barRT.pivot = new Vector2(0, 0.5f);
            barRT.sizeDelta = new Vector2(5, 0);
            barRT.anchoredPosition = Vector2.zero;

            Image barImg = resultBar.AddComponent<Image>();
            barImg.color = GREEN;

            // === Game Icon ===
            GameObject gameIcon = new GameObject("GameTypeIcon");
            gameIcon.transform.SetParent(root.transform, false);

            RectTransform gameRT = gameIcon.AddComponent<RectTransform>();
            gameRT.anchorMin = new Vector2(0, 0.5f);
            gameRT.anchorMax = new Vector2(0, 0.5f);
            gameRT.pivot = new Vector2(0, 0.5f);
            gameRT.sizeDelta = new Vector2(55, 55);
            gameRT.anchoredPosition = new Vector2(15, 0);

            Image gameImg = gameIcon.AddComponent<Image>();
            gameImg.preserveAspect = true;

            Sprite defaultGameIcon = AssetDatabase.LoadAssetAtPath<Sprite>(GAME_ICONS_PATH + "QuickMathIcon.png");
            if (defaultGameIcon != null)
            {
                gameImg.sprite = defaultGameIcon;
                gameImg.color = Color.white;
            }
            else
            {
                gameImg.color = CYAN;
            }

            // === Mode Badge ===
            GameObject modeBadge = new GameObject("ModeBadge");
            modeBadge.transform.SetParent(root.transform, false);

            RectTransform modeRT = modeBadge.AddComponent<RectTransform>();
            modeRT.anchorMin = new Vector2(0, 1);
            modeRT.anchorMax = new Vector2(0, 1);
            modeRT.pivot = new Vector2(0, 1);
            modeRT.sizeDelta = new Vector2(45, 20);
            modeRT.anchoredPosition = new Vector2(80, -8);

            Image modeBg = modeBadge.AddComponent<Image>();
            modeBg.color = new Color(CYAN.r, CYAN.g, CYAN.b, 0.3f);

            GameObject modeText = new GameObject("Text");
            modeText.transform.SetParent(modeBadge.transform, false);

            RectTransform modeTextRT = modeText.AddComponent<RectTransform>();
            modeTextRT.anchorMin = Vector2.zero;
            modeTextRT.anchorMax = Vector2.one;
            modeTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI modeTMP = modeText.AddComponent<TextMeshProUGUI>();
            modeTMP.text = "1v1";
            modeTMP.fontSize = 11;
            modeTMP.fontStyle = FontStyles.Bold;
            modeTMP.color = CYAN;
            modeTMP.alignment = TextAlignmentOptions.Center;

            // === Title ===
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(root.transform, false);

            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0.5f);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0, 1);
            titleRT.offsetMin = new Vector2(80, 5);
            titleRT.offsetMax = new Vector2(-100, -28);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "QuickMath vs @Player123";
            titleText.fontSize = 16;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = TEXT_WHITE;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.overflowMode = TextOverflowModes.Ellipsis;

            // === Entry Fee ===
            GameObject feeObj = new GameObject("EntryFeeText");
            feeObj.transform.SetParent(root.transform, false);

            RectTransform feeRT = feeObj.AddComponent<RectTransform>();
            feeRT.anchorMin = new Vector2(0, 0);
            feeRT.anchorMax = new Vector2(0.5f, 0.5f);
            feeRT.pivot = new Vector2(0, 0);
            feeRT.offsetMin = new Vector2(80, 8);
            feeRT.offsetMax = new Vector2(0, -5);

            TextMeshProUGUI feeText = feeObj.AddComponent<TextMeshProUGUI>();
            feeText.text = "Entrada: $5.00";
            feeText.fontSize = 13;
            feeText.color = TEXT_SECONDARY;
            feeText.alignment = TextAlignmentOptions.Left;

            // === Date ===
            GameObject dateObj = new GameObject("DateText");
            dateObj.transform.SetParent(root.transform, false);

            RectTransform dateRT = dateObj.AddComponent<RectTransform>();
            dateRT.anchorMin = new Vector2(0.5f, 0);
            dateRT.anchorMax = new Vector2(1, 0.5f);
            dateRT.pivot = new Vector2(1, 0);
            dateRT.offsetMin = new Vector2(0, 8);
            dateRT.offsetMax = new Vector2(-100, -5);

            TextMeshProUGUI dateText = dateObj.AddComponent<TextMeshProUGUI>();
            dateText.text = "Hace 2h";
            dateText.fontSize = 12;
            dateText.color = TEXT_SECONDARY;
            dateText.alignment = TextAlignmentOptions.Right;

            // === Result Badge ===
            GameObject resultBadge = new GameObject("ResultBadge");
            resultBadge.transform.SetParent(root.transform, false);

            RectTransform rbRT = resultBadge.AddComponent<RectTransform>();
            rbRT.anchorMin = new Vector2(1, 0.5f);
            rbRT.anchorMax = new Vector2(1, 1);
            rbRT.pivot = new Vector2(1, 1);
            rbRT.sizeDelta = new Vector2(85, 25);
            rbRT.anchoredPosition = new Vector2(-10, -10);

            Image rbBg = resultBadge.AddComponent<Image>();
            rbBg.color = new Color(GREEN.r, GREEN.g, GREEN.b, 0.2f);

            GameObject rbText = new GameObject("Text");
            rbText.transform.SetParent(resultBadge.transform, false);

            RectTransform rbTextRT = rbText.AddComponent<RectTransform>();
            rbTextRT.anchorMin = Vector2.zero;
            rbTextRT.anchorMax = Vector2.one;
            rbTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI rbTMP = rbText.AddComponent<TextMeshProUGUI>();
            rbTMP.text = "VICTORIA";
            rbTMP.fontSize = 12;
            rbTMP.fontStyle = FontStyles.Bold;
            rbTMP.color = GREEN;
            rbTMP.alignment = TextAlignmentOptions.Center;

            // === Net Result ===
            GameObject netObj = new GameObject("NetResultText");
            netObj.transform.SetParent(root.transform, false);

            RectTransform netRT = netObj.AddComponent<RectTransform>();
            netRT.anchorMin = new Vector2(1, 0);
            netRT.anchorMax = new Vector2(1, 0.5f);
            netRT.pivot = new Vector2(1, 0);
            netRT.sizeDelta = new Vector2(85, 30);
            netRT.anchoredPosition = new Vector2(-10, 8);

            TextMeshProUGUI netText = netObj.AddComponent<TextMeshProUGUI>();
            netText.text = "+$4.50";
            netText.fontSize = 20;
            netText.fontStyle = FontStyles.Bold;
            netText.color = GREEN;
            netText.alignment = TextAlignmentOptions.Center;

            // Guardar prefab
            string path = $"{PREFAB_PATH}/HistoryEntryItemUI.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, path);
            DestroyImmediate(root);

            Debug.Log($"[CashBattlePrefabBuilder] HistoryEntryItemUI.prefab creado: {path}");
        }

        [MenuItem("DigitPark/Prefabs/CashBattle/DepositOptionUI Prefab", false, 112)]
        public static void CreateDepositOptionPrefab()
        {
            EnsurePrefabDirectory();

            GameObject root = new GameObject("DepositOptionUI");
            RectTransform rootRT = root.AddComponent<RectTransform>();
            rootRT.sizeDelta = new Vector2(0, 90);

            LayoutElement rootLE = root.AddComponent<LayoutElement>();
            rootLE.preferredHeight = 90;
            rootLE.minHeight = 90;

            Image rootBg = root.AddComponent<Image>();
            rootBg.color = CARD_BG;

            Button rootBtn = root.AddComponent<Button>();
            rootBtn.targetGraphic = rootBg;

            // Añadir componente script
            root.AddComponent<CashBattle.DepositOptionUI>();

            // === Border (seleccionado) ===
            Outline outline = root.AddComponent<Outline>();
            outline.effectColor = new Color(GOLD.r, GOLD.g, GOLD.b, 0.5f);
            outline.effectDistance = new Vector2(2, -2);

            // === Deposit Icon ===
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(root.transform, false);

            RectTransform iconRT = iconObj.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0, 0.5f);
            iconRT.anchorMax = new Vector2(0, 0.5f);
            iconRT.pivot = new Vector2(0, 0.5f);
            iconRT.sizeDelta = new Vector2(55, 55);
            iconRT.anchoredPosition = new Vector2(15, 0);

            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.preserveAspect = true;

            Sprite depositIcon = AssetDatabase.LoadAssetAtPath<Sprite>(WALLET_ICONS_PATH + "DepositIcon.png");
            if (depositIcon != null)
            {
                iconImg.sprite = depositIcon;
                iconImg.color = Color.white;
            }
            else
            {
                iconImg.color = GREEN;
            }

            // === Amount ===
            GameObject amountObj = new GameObject("AmountText");
            amountObj.transform.SetParent(root.transform, false);

            RectTransform amtRT = amountObj.AddComponent<RectTransform>();
            amtRT.anchorMin = new Vector2(0, 0.5f);
            amtRT.anchorMax = new Vector2(0.6f, 1);
            amtRT.pivot = new Vector2(0, 0.5f);
            amtRT.offsetMin = new Vector2(80, 0);
            amtRT.offsetMax = new Vector2(0, -10);

            TextMeshProUGUI amtText = amountObj.AddComponent<TextMeshProUGUI>();
            amtText.text = "$10.00";
            amtText.fontSize = 28;
            amtText.fontStyle = FontStyles.Bold;
            amtText.color = TEXT_WHITE;
            amtText.alignment = TextAlignmentOptions.Left;

            // === Bonus ===
            GameObject bonusObj = new GameObject("BonusText");
            bonusObj.transform.SetParent(root.transform, false);

            RectTransform bonusRT = bonusObj.AddComponent<RectTransform>();
            bonusRT.anchorMin = new Vector2(0, 0);
            bonusRT.anchorMax = new Vector2(0.6f, 0.5f);
            bonusRT.pivot = new Vector2(0, 0.5f);
            bonusRT.offsetMin = new Vector2(80, 10);
            bonusRT.offsetMax = new Vector2(0, 0);

            TextMeshProUGUI bonusText = bonusObj.AddComponent<TextMeshProUGUI>();
            bonusText.text = "+$0.50 BONUS";
            bonusText.fontSize = 14;
            bonusText.color = GREEN;
            bonusText.alignment = TextAlignmentOptions.Left;

            // === Popular Badge ===
            GameObject popularBadge = new GameObject("PopularBadge");
            popularBadge.transform.SetParent(root.transform, false);
            popularBadge.SetActive(false);

            RectTransform popRT = popularBadge.AddComponent<RectTransform>();
            popRT.anchorMin = new Vector2(1, 1);
            popRT.anchorMax = new Vector2(1, 1);
            popRT.pivot = new Vector2(1, 1);
            popRT.sizeDelta = new Vector2(70, 22);
            popRT.anchoredPosition = new Vector2(-10, -8);

            Image popBg = popularBadge.AddComponent<Image>();
            popBg.color = CYAN;

            GameObject popText = new GameObject("Text");
            popText.transform.SetParent(popularBadge.transform, false);

            RectTransform popTextRT = popText.AddComponent<RectTransform>();
            popTextRT.anchorMin = Vector2.zero;
            popTextRT.anchorMax = Vector2.one;
            popTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI popTMP = popText.AddComponent<TextMeshProUGUI>();
            popTMP.text = "POPULAR";
            popTMP.fontSize = 11;
            popTMP.fontStyle = FontStyles.Bold;
            popTMP.color = Color.white;
            popTMP.alignment = TextAlignmentOptions.Center;

            // === Select Button ===
            GameObject selectBtn = new GameObject("SelectButton");
            selectBtn.transform.SetParent(root.transform, false);

            RectTransform selRT = selectBtn.AddComponent<RectTransform>();
            selRT.anchorMin = new Vector2(1, 0.5f);
            selRT.anchorMax = new Vector2(1, 0.5f);
            selRT.pivot = new Vector2(1, 0.5f);
            selRT.sizeDelta = new Vector2(90, 40);
            selRT.anchoredPosition = new Vector2(-15, 0);

            Image selBg = selectBtn.AddComponent<Image>();
            selBg.color = GREEN;

            Button selButton = selectBtn.AddComponent<Button>();
            selButton.targetGraphic = selBg;

            GameObject selText = new GameObject("Text");
            selText.transform.SetParent(selectBtn.transform, false);

            RectTransform selTextRT = selText.AddComponent<RectTransform>();
            selTextRT.anchorMin = Vector2.zero;
            selTextRT.anchorMax = Vector2.one;
            selTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI selTMP = selText.AddComponent<TextMeshProUGUI>();
            selTMP.text = "Seleccionar";
            selTMP.fontSize = 13;
            selTMP.fontStyle = FontStyles.Bold;
            selTMP.color = Color.white;
            selTMP.alignment = TextAlignmentOptions.Center;

            // Guardar prefab
            string path = $"{PREFAB_PATH}/DepositOptionUI.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, path);
            DestroyImmediate(root);

            Debug.Log($"[CashBattlePrefabBuilder] DepositOptionUI.prefab creado: {path}");
        }

        [MenuItem("DigitPark/Prefabs/CashBattle/TournamentCardUI Prefab", false, 113)]
        public static void CreateTournamentCardPrefab()
        {
            EnsurePrefabDirectory();

            GameObject root = new GameObject("TournamentCardUI");
            RectTransform rootRT = root.AddComponent<RectTransform>();
            rootRT.sizeDelta = new Vector2(0, 150);

            LayoutElement rootLE = root.AddComponent<LayoutElement>();
            rootLE.preferredHeight = 150;
            rootLE.minHeight = 150;

            Image rootBg = root.AddComponent<Image>();
            rootBg.color = CARD_BG;

            Button rootBtn = root.AddComponent<Button>();
            rootBtn.targetGraphic = rootBg;
            ColorBlock colors = rootBtn.colors;
            colors.highlightedColor = CARD_BG * 1.3f;
            rootBtn.colors = colors;

            // Border dorado
            Outline outline = root.AddComponent<Outline>();
            outline.effectColor = new Color(GOLD.r, GOLD.g, GOLD.b, 0.5f);
            outline.effectDistance = new Vector2(2, -2);

            // === LIVE Badge (arriba derecha) ===
            GameObject liveBadge = new GameObject("LiveBadge");
            liveBadge.transform.SetParent(root.transform, false);
            liveBadge.SetActive(false);

            RectTransform liveRT = liveBadge.AddComponent<RectTransform>();
            liveRT.anchorMin = new Vector2(1, 1);
            liveRT.anchorMax = new Vector2(1, 1);
            liveRT.pivot = new Vector2(1, 1);
            liveRT.sizeDelta = new Vector2(70, 28);
            liveRT.anchoredPosition = new Vector2(-10, -10);

            Image liveBg = liveBadge.AddComponent<Image>();
            liveBg.color = new Color(1f, 0.3f, 0.2f, 1f);

            // LIVE Icon
            GameObject liveIcon = new GameObject("Icon");
            liveIcon.transform.SetParent(liveBadge.transform, false);

            RectTransform liRT = liveIcon.AddComponent<RectTransform>();
            liRT.anchorMin = new Vector2(0, 0.5f);
            liRT.anchorMax = new Vector2(0, 0.5f);
            liRT.pivot = new Vector2(0, 0.5f);
            liRT.sizeDelta = new Vector2(20, 20);
            liRT.anchoredPosition = new Vector2(5, 0);

            Image liImg = liveIcon.AddComponent<Image>();
            liImg.preserveAspect = true;
            Sprite liveSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TOURNAMENT_ICONS_PATH + "TournamentLiveIcon.png");
            if (liveSprite != null) liImg.sprite = liveSprite;
            liImg.color = Color.white;

            GameObject liveText = new GameObject("Text");
            liveText.transform.SetParent(liveBadge.transform, false);

            RectTransform ltRT = liveText.AddComponent<RectTransform>();
            ltRT.anchorMin = new Vector2(0, 0);
            ltRT.anchorMax = new Vector2(1, 1);
            ltRT.offsetMin = new Vector2(25, 0);
            ltRT.offsetMax = new Vector2(-5, 0);

            TextMeshProUGUI ltTMP = liveText.AddComponent<TextMeshProUGUI>();
            ltTMP.text = "LIVE";
            ltTMP.fontSize = 12;
            ltTMP.fontStyle = FontStyles.Bold;
            ltTMP.color = Color.white;
            ltTMP.alignment = TextAlignmentOptions.Center;

            // === Game Icon (izquierda) ===
            GameObject gameIcon = new GameObject("GameIcon");
            gameIcon.transform.SetParent(root.transform, false);

            RectTransform giRT = gameIcon.AddComponent<RectTransform>();
            giRT.anchorMin = new Vector2(0, 0.5f);
            giRT.anchorMax = new Vector2(0, 0.5f);
            giRT.pivot = new Vector2(0, 0.5f);
            giRT.sizeDelta = new Vector2(70, 70);
            giRT.anchoredPosition = new Vector2(15, 0);

            Image giImg = gameIcon.AddComponent<Image>();
            giImg.preserveAspect = true;
            Sprite gameSprite = AssetDatabase.LoadAssetAtPath<Sprite>(GAME_ICONS_PATH + "QuickMathIcon.png");
            if (gameSprite != null) giImg.sprite = gameSprite;
            giImg.color = Color.white;

            // === Tournament Name ===
            GameObject nameObj = new GameObject("TournamentName");
            nameObj.transform.SetParent(root.transform, false);

            RectTransform nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 1);
            nameRT.anchorMax = new Vector2(0.7f, 1);
            nameRT.pivot = new Vector2(0, 1);
            nameRT.anchoredPosition = new Vector2(95, -12);
            nameRT.sizeDelta = new Vector2(0, 28);

            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "Quick Math Championship";
            nameText.fontSize = 18;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = TEXT_WHITE;
            nameText.alignment = TextAlignmentOptions.Left;
            nameText.overflowMode = TextOverflowModes.Ellipsis;

            // === Prize Pool Row (con icono) ===
            GameObject prizeRow = new GameObject("PrizeRow");
            prizeRow.transform.SetParent(root.transform, false);

            RectTransform prRT = prizeRow.AddComponent<RectTransform>();
            prRT.anchorMin = new Vector2(0, 0.5f);
            prRT.anchorMax = new Vector2(0, 0.5f);
            prRT.pivot = new Vector2(0, 0.5f);
            prRT.sizeDelta = new Vector2(150, 30);
            prRT.anchoredPosition = new Vector2(95, 8);

            // Prize Icon
            GameObject prizeIcon = new GameObject("Icon");
            prizeIcon.transform.SetParent(prizeRow.transform, false);

            RectTransform piRT = prizeIcon.AddComponent<RectTransform>();
            piRT.anchorMin = new Vector2(0, 0.5f);
            piRT.anchorMax = new Vector2(0, 0.5f);
            piRT.pivot = new Vector2(0, 0.5f);
            piRT.sizeDelta = new Vector2(26, 26);
            piRT.anchoredPosition = Vector2.zero;

            Image piImg = prizeIcon.AddComponent<Image>();
            piImg.preserveAspect = true;
            Sprite prizeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TOURNAMENT_ICONS_PATH + "TrophyPrizeIcon.png");
            if (prizeSprite != null) piImg.sprite = prizeSprite;
            piImg.color = Color.white;

            GameObject prizeText = new GameObject("Text");
            prizeText.transform.SetParent(prizeRow.transform, false);

            RectTransform ptRT = prizeText.AddComponent<RectTransform>();
            ptRT.anchorMin = new Vector2(0, 0);
            ptRT.anchorMax = new Vector2(1, 1);
            ptRT.offsetMin = new Vector2(30, 0);
            ptRT.offsetMax = Vector2.zero;

            TextMeshProUGUI ptTMP = prizeText.AddComponent<TextMeshProUGUI>();
            ptTMP.text = "$500";
            ptTMP.fontSize = 20;
            ptTMP.fontStyle = FontStyles.Bold;
            ptTMP.color = GREEN;
            ptTMP.alignment = TextAlignmentOptions.Left;

            // === Players Row (con icono) ===
            GameObject playersRow = new GameObject("PlayersRow");
            playersRow.transform.SetParent(root.transform, false);

            RectTransform plRT = playersRow.AddComponent<RectTransform>();
            plRT.anchorMin = new Vector2(0, 0);
            plRT.anchorMax = new Vector2(0, 0);
            plRT.pivot = new Vector2(0, 0);
            plRT.sizeDelta = new Vector2(100, 26);
            plRT.anchoredPosition = new Vector2(95, 15);

            // Players Icon
            GameObject playersIcon = new GameObject("Icon");
            playersIcon.transform.SetParent(playersRow.transform, false);

            RectTransform pliRT = playersIcon.AddComponent<RectTransform>();
            pliRT.anchorMin = new Vector2(0, 0.5f);
            pliRT.anchorMax = new Vector2(0, 0.5f);
            pliRT.pivot = new Vector2(0, 0.5f);
            pliRT.sizeDelta = new Vector2(22, 22);
            pliRT.anchoredPosition = Vector2.zero;

            Image pliImg = playersIcon.AddComponent<Image>();
            pliImg.preserveAspect = true;
            Sprite playersSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TOURNAMENT_ICONS_PATH + "PlayersCountIcon.png");
            if (playersSprite != null) pliImg.sprite = playersSprite;
            pliImg.color = Color.white;

            GameObject playersText = new GameObject("Text");
            playersText.transform.SetParent(playersRow.transform, false);

            RectTransform pltRT = playersText.AddComponent<RectTransform>();
            pltRT.anchorMin = new Vector2(0, 0);
            pltRT.anchorMax = new Vector2(1, 1);
            pltRT.offsetMin = new Vector2(26, 0);
            pltRT.offsetMax = Vector2.zero;

            TextMeshProUGUI pltTMP = playersText.AddComponent<TextMeshProUGUI>();
            pltTMP.text = "15/20";
            pltTMP.fontSize = 14;
            pltTMP.color = TEXT_SECONDARY;
            pltTMP.alignment = TextAlignmentOptions.Left;

            // === Timer Row (con icono) ===
            GameObject timerRow = new GameObject("TimerRow");
            timerRow.transform.SetParent(root.transform, false);

            RectTransform trRT = timerRow.AddComponent<RectTransform>();
            trRT.anchorMin = new Vector2(0.5f, 0);
            trRT.anchorMax = new Vector2(0.5f, 0);
            trRT.pivot = new Vector2(0, 0);
            trRT.sizeDelta = new Vector2(100, 26);
            trRT.anchoredPosition = new Vector2(-30, 15);

            // Timer Icon
            GameObject timerIcon = new GameObject("Icon");
            timerIcon.transform.SetParent(timerRow.transform, false);

            RectTransform tiRT = timerIcon.AddComponent<RectTransform>();
            tiRT.anchorMin = new Vector2(0, 0.5f);
            tiRT.anchorMax = new Vector2(0, 0.5f);
            tiRT.pivot = new Vector2(0, 0.5f);
            tiRT.sizeDelta = new Vector2(22, 22);
            tiRT.anchoredPosition = Vector2.zero;

            Image tiImg = timerIcon.AddComponent<Image>();
            tiImg.preserveAspect = true;
            Sprite timerSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TOURNAMENT_ICONS_PATH + "TournamentTimerIcon.png");
            if (timerSprite != null) tiImg.sprite = timerSprite;
            tiImg.color = Color.white;

            GameObject timerText = new GameObject("Text");
            timerText.transform.SetParent(timerRow.transform, false);

            RectTransform ttRT = timerText.AddComponent<RectTransform>();
            ttRT.anchorMin = new Vector2(0, 0);
            ttRT.anchorMax = new Vector2(1, 1);
            ttRT.offsetMin = new Vector2(26, 0);
            ttRT.offsetMax = Vector2.zero;

            TextMeshProUGUI ttTMP = timerText.AddComponent<TextMeshProUGUI>();
            ttTMP.text = "02:45:00";
            ttTMP.fontSize = 14;
            ttTMP.color = CYAN;
            ttTMP.alignment = TextAlignmentOptions.Left;

            // === Entry Fee Badge ===
            GameObject entryBadge = new GameObject("EntryFeeBadge");
            entryBadge.transform.SetParent(root.transform, false);

            RectTransform ebRT = entryBadge.AddComponent<RectTransform>();
            ebRT.anchorMin = new Vector2(1, 0.5f);
            ebRT.anchorMax = new Vector2(1, 0.5f);
            ebRT.pivot = new Vector2(1, 0.5f);
            ebRT.sizeDelta = new Vector2(85, 55);
            ebRT.anchoredPosition = new Vector2(-15, -5);

            Image ebBg = entryBadge.AddComponent<Image>();
            ebBg.color = new Color(0, 0, 0, 0.4f);

            // Entry Label
            GameObject entryLabel = new GameObject("Label");
            entryLabel.transform.SetParent(entryBadge.transform, false);

            RectTransform elRT = entryLabel.AddComponent<RectTransform>();
            elRT.anchorMin = new Vector2(0, 0.5f);
            elRT.anchorMax = new Vector2(1, 1);
            elRT.sizeDelta = Vector2.zero;
            elRT.offsetMin = new Vector2(5, 0);
            elRT.offsetMax = new Vector2(-5, -5);

            TextMeshProUGUI elTMP = entryLabel.AddComponent<TextMeshProUGUI>();
            elTMP.text = "Entrada";
            elTMP.fontSize = 11;
            elTMP.color = TEXT_SECONDARY;
            elTMP.alignment = TextAlignmentOptions.Center;

            // Entry Value
            GameObject entryValue = new GameObject("Value");
            entryValue.transform.SetParent(entryBadge.transform, false);

            RectTransform evRT = entryValue.AddComponent<RectTransform>();
            evRT.anchorMin = new Vector2(0, 0);
            evRT.anchorMax = new Vector2(1, 0.5f);
            evRT.sizeDelta = Vector2.zero;
            evRT.offsetMin = new Vector2(5, 5);
            evRT.offsetMax = new Vector2(-5, 0);

            TextMeshProUGUI evTMP = entryValue.AddComponent<TextMeshProUGUI>();
            evTMP.text = "$5";
            evTMP.fontSize = 18;
            evTMP.fontStyle = FontStyles.Bold;
            evTMP.color = GOLD;
            evTMP.alignment = TextAlignmentOptions.Center;

            // === Join Button ===
            GameObject joinBtn = new GameObject("JoinButton");
            joinBtn.transform.SetParent(root.transform, false);

            RectTransform jbRT = joinBtn.AddComponent<RectTransform>();
            jbRT.anchorMin = new Vector2(1, 0);
            jbRT.anchorMax = new Vector2(1, 0);
            jbRT.pivot = new Vector2(1, 0);
            jbRT.sizeDelta = new Vector2(85, 35);
            jbRT.anchoredPosition = new Vector2(-15, 12);

            Image jbBg = joinBtn.AddComponent<Image>();
            jbBg.color = GREEN;

            Button jbButton = joinBtn.AddComponent<Button>();
            jbButton.targetGraphic = jbBg;

            GameObject joinText = new GameObject("Text");
            joinText.transform.SetParent(joinBtn.transform, false);

            RectTransform jtRT = joinText.AddComponent<RectTransform>();
            jtRT.anchorMin = Vector2.zero;
            jtRT.anchorMax = Vector2.one;
            jtRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI jtTMP = joinText.AddComponent<TextMeshProUGUI>();
            jtTMP.text = "Unirse";
            jtTMP.fontSize = 15;
            jtTMP.fontStyle = FontStyles.Bold;
            jtTMP.color = Color.white;
            jtTMP.alignment = TextAlignmentOptions.Center;

            // Guardar prefab
            string path = $"{PREFAB_PATH}/TournamentCardUI.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, path);
            DestroyImmediate(root);

            Debug.Log($"[CashBattlePrefabBuilder] TournamentCardUI.prefab creado: {path}");
        }

        private static void EnsurePrefabDirectory()
        {
            if (!Directory.Exists(PREFAB_PATH))
            {
                Directory.CreateDirectory(PREFAB_PATH);
                AssetDatabase.Refresh();
            }
        }
    }
}
