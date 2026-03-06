using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;
using DigitPark.UI;

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
        private const string GAME_ICONS_PATH = "Assets/_Project/Art/Icons/Games/";
        private const string STAT_ICONS_PATH = "Assets/_Project/Art/Icons/CashBattle/Stats/";
        private const string TOURNAMENT_ICONS_PATH = "Assets/_Project/Art/Icons/CashBattle/Tournaments/";

        // Colors - Premium Theme
        private static readonly Color CARD_BG = new Color(0.06f, 0.08f, 0.12f, 1f);
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
            CreateHistoryEntryItemPrefab();
            CreateTournamentCardPrefab();
            AssetDatabase.Refresh();
            Debug.Log("[CashBattlePrefabBuilder] Todos los prefabs PREMIUM creados!");
        }

        [MenuItem("DigitPark/Prefabs/CashBattle/HistoryEntryItemUI Prefab", false, 111)]
        public static void CreateHistoryEntryItemPrefab()
        {
            EnsurePrefabDirectory();

            GameObject root = new GameObject("HistoryEntryItemUI");
            RectTransform rootRT = root.AddComponent<RectTransform>();
            rootRT.sizeDelta = new Vector2(0, 120);

            LayoutElement rootLE = root.AddComponent<LayoutElement>();
            rootLE.preferredHeight = 120;
            rootLE.minHeight = 120;

            Image rootBg = root.AddComponent<Image>();
            rootBg.color = CARD_BG;

            // Gold outline + shadow (unified style)
            Outline rootOutline = root.AddComponent<Outline>();
            rootOutline.effectColor = new Color(0.85f, 0.65f, 0.13f, 0.35f);
            rootOutline.effectDistance = new Vector2(1.5f, 1.5f);
            Shadow rootShadow = root.AddComponent<Shadow>();
            rootShadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            rootShadow.effectDistance = new Vector2(3, -4);

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
            modeTMP.fontSize = FontSizes.Body;
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
            titleText.fontSize = FontSizes.Body;
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
            feeText.text = "Entry: $5.00";
            feeText.fontSize = FontSizes.Body;
            feeText.color = TEXT_SECONDARY;
            feeText.alignment = TextAlignmentOptions.Left;
            feeText.fontStyle = FontStyles.Bold;


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
            dateText.text = "2h ago";
            dateText.fontSize = FontSizes.Body;
            dateText.color = TEXT_SECONDARY;
            dateText.alignment = TextAlignmentOptions.Right;
            dateText.fontStyle = FontStyles.Bold;


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
            rbTMP.text = "VICTORY";
            rbTMP.fontSize = FontSizes.Body;
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
            netText.fontSize = FontSizes.Body;
            netText.fontStyle = FontStyles.Bold;
            netText.color = GREEN;
            netText.alignment = TextAlignmentOptions.Center;


            // Guardar prefab - Renombrado a MatchHistoryItem según la nueva estructura
            string path = $"{PREFAB_PATH}/History/MatchHistoryItem.prefab";
            EnsureDirectoryExists(path);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            DestroyImmediate(root);

            Debug.Log($"[CashBattlePrefabBuilder] MatchHistoryItem.prefab creado: {path}");
        }

        [MenuItem("DigitPark/Prefabs/CashBattle/TournamentCardUI Prefab", false, 113)]
        public static void CreateTournamentCardPrefab()
        {
            EnsurePrefabDirectory();

            // === Clean card (160px) — text-only, no icons ===
            GameObject root = new GameObject("TournamentCardUI");
            RectTransform rootRT = root.AddComponent<RectTransform>();
            rootRT.sizeDelta = new Vector2(0, 160);

            LayoutElement rootLE = root.AddComponent<LayoutElement>();
            rootLE.preferredHeight = 160;
            rootLE.minHeight = 160;

            Image rootBg = root.AddComponent<Image>();
            rootBg.color = CARD_BG;

            Outline tcOutline = root.AddComponent<Outline>();
            tcOutline.effectColor = new Color(0.85f, 0.65f, 0.13f, 0.35f);
            tcOutline.effectDistance = new Vector2(1.5f, 1.5f);

            Button rootBtn = root.AddComponent<Button>();
            rootBtn.targetGraphic = rootBg;
            ColorBlock colors = rootBtn.colors;
            colors.highlightedColor = CARD_BG * 1.3f;
            rootBtn.colors = colors;

            // === Gold bar (bottom accent) ===
            GameObject colorBar = new GameObject("ColorBar");
            colorBar.transform.SetParent(root.transform, false);
            RectTransform barRT = colorBar.AddComponent<RectTransform>();
            barRT.anchorMin = new Vector2(0, 0);
            barRT.anchorMax = new Vector2(1, 0);
            barRT.pivot = new Vector2(0.5f, 0);
            barRT.sizeDelta = new Vector2(0, 4);
            barRT.anchoredPosition = Vector2.zero;
            Image barImg = colorBar.AddComponent<Image>();
            barImg.color = GOLD;
            barImg.raycastTarget = false;

            // === LIVE Badge (top-right, hidden by default) ===
            GameObject liveBadge = new GameObject("LiveBadge");
            liveBadge.transform.SetParent(root.transform, false);
            liveBadge.SetActive(false);

            RectTransform liveRT = liveBadge.AddComponent<RectTransform>();
            liveRT.anchorMin = new Vector2(1, 1);
            liveRT.anchorMax = new Vector2(1, 1);
            liveRT.pivot = new Vector2(1, 1);
            liveRT.sizeDelta = new Vector2(90, 32);
            liveRT.anchoredPosition = new Vector2(-10, -8);

            Image liveBg = liveBadge.AddComponent<Image>();
            liveBg.color = new Color(1f, 0.3f, 0.2f, 1f);

            GameObject liveText = new GameObject("Text");
            liveText.transform.SetParent(liveBadge.transform, false);
            RectTransform ltRT = liveText.AddComponent<RectTransform>();
            ltRT.anchorMin = Vector2.zero;
            ltRT.anchorMax = Vector2.one;
            ltRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI ltTMP = liveText.AddComponent<TextMeshProUGUI>();
            ltTMP.text = "LIVE";
            ltTMP.fontSize = FontSizes.Caption;
            ltTMP.fontStyle = FontStyles.Bold;
            ltTMP.color = Color.white;
            ltTMP.alignment = TextAlignmentOptions.Center;

            // === Tournament Name (top-left, anchor-based) ===
            GameObject nameObj = new GameObject("TournamentName");
            nameObj.transform.SetParent(root.transform, false);

            RectTransform nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0.55f);
            nameRT.anchorMax = new Vector2(0.65f, 1f);
            nameRT.offsetMin = new Vector2(16, 0);
            nameRT.offsetMax = new Vector2(0, -10);

            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "Tournament Name";
            nameText.fontSize = FontSizes.Body;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = TEXT_WHITE;
            nameText.alignment = TextAlignmentOptions.Left;
            nameText.enableAutoSizing = true;
            nameText.fontSizeMin = FontSizes.AutoMinBody;
            nameText.fontSizeMax = FontSizes.Body;
            nameText.overflowMode = TextOverflowModes.Ellipsis;
            nameText.enableWordWrapping = false;

            // === Info Row (bottom-left): Prize · Players · Timer ===
            GameObject infoRow = new GameObject("InfoRow");
            infoRow.transform.SetParent(root.transform, false);

            RectTransform irRT = infoRow.AddComponent<RectTransform>();
            irRT.anchorMin = new Vector2(0, 0);
            irRT.anchorMax = new Vector2(0.65f, 0.55f);
            irRT.offsetMin = new Vector2(16, 12);
            irRT.offsetMax = new Vector2(0, 0);

            HorizontalLayoutGroup irHLG = infoRow.AddComponent<HorizontalLayoutGroup>();
            irHLG.spacing = 6;
            irHLG.padding = new RectOffset(0, 0, 0, 0);
            irHLG.childAlignment = TextAnchor.MiddleLeft;
            irHLG.childForceExpandWidth = false;
            irHLG.childForceExpandHeight = true;
            irHLG.childControlWidth = true;
            irHLG.childControlHeight = true;

            // Prize — direct child of InfoRow
            GameObject prizeText = new GameObject("PrizeText");
            prizeText.transform.SetParent(infoRow.transform, false);
            LayoutElement prLE = prizeText.AddComponent<LayoutElement>();
            prLE.flexibleWidth = 1;
            TextMeshProUGUI ptTMP = prizeText.AddComponent<TextMeshProUGUI>();
            ptTMP.text = "$500";
            ptTMP.fontSize = FontSizes.BodySmall;
            ptTMP.fontStyle = FontStyles.Bold;
            ptTMP.color = GREEN;
            ptTMP.alignment = TextAlignmentOptions.Left;
            ptTMP.enableAutoSizing = true;
            ptTMP.fontSizeMin = FontSizes.AutoMinSmall;
            ptTMP.fontSizeMax = FontSizes.BodySmall;

            // Separator dot 1
            CreateSeparatorDot(infoRow.transform);

            // Players — direct child of InfoRow
            GameObject playersText = new GameObject("PlayersText");
            playersText.transform.SetParent(infoRow.transform, false);
            LayoutElement plLE = playersText.AddComponent<LayoutElement>();
            plLE.flexibleWidth = 1;
            TextMeshProUGUI pltTMP = playersText.AddComponent<TextMeshProUGUI>();
            pltTMP.text = "15/20";
            pltTMP.fontSize = FontSizes.BodySmall;
            pltTMP.color = TEXT_SECONDARY;
            pltTMP.alignment = TextAlignmentOptions.Left;
            pltTMP.fontStyle = FontStyles.Bold;
            pltTMP.enableAutoSizing = true;
            pltTMP.fontSizeMin = FontSizes.AutoMinSmall;
            pltTMP.fontSizeMax = FontSizes.BodySmall;

            // Separator dot 2
            CreateSeparatorDot(infoRow.transform);

            // Timer — direct child of InfoRow
            GameObject timerText = new GameObject("TimerText");
            timerText.transform.SetParent(infoRow.transform, false);
            LayoutElement trLE = timerText.AddComponent<LayoutElement>();
            trLE.flexibleWidth = 1;
            TextMeshProUGUI ttTMP = timerText.AddComponent<TextMeshProUGUI>();
            ttTMP.text = "02:45:00";
            ttTMP.fontSize = FontSizes.BodySmall;
            ttTMP.color = CYAN;
            ttTMP.alignment = TextAlignmentOptions.Left;
            ttTMP.fontStyle = FontStyles.Bold;
            ttTMP.enableAutoSizing = true;
            ttTMP.fontSizeMin = FontSizes.AutoMinSmall;
            ttTMP.fontSizeMax = FontSizes.BodySmall;

            // === Entry Fee Badge (top-right) ===
            GameObject entryBadge = new GameObject("EntryFeeBadge");
            entryBadge.transform.SetParent(root.transform, false);

            RectTransform ebRT = entryBadge.AddComponent<RectTransform>();
            ebRT.anchorMin = new Vector2(0.65f, 0.5f);
            ebRT.anchorMax = new Vector2(0.82f, 1f);
            ebRT.offsetMin = new Vector2(4, 8);
            ebRT.offsetMax = new Vector2(0, -8);

            Image ebBg = entryBadge.AddComponent<Image>();
            ebBg.color = new Color(0, 0, 0, 0.4f);

            GameObject entryLabel = new GameObject("EntryLabel");
            entryLabel.transform.SetParent(entryBadge.transform, false);
            RectTransform elRT = entryLabel.AddComponent<RectTransform>();
            elRT.anchorMin = new Vector2(0, 0.5f);
            elRT.anchorMax = new Vector2(1, 1);
            elRT.sizeDelta = Vector2.zero;
            elRT.offsetMin = new Vector2(4, 0);
            elRT.offsetMax = new Vector2(-4, -2);
            TextMeshProUGUI elTMP = entryLabel.AddComponent<TextMeshProUGUI>();
            elTMP.text = "Entry";
            elTMP.fontSize = FontSizes.Caption;
            elTMP.fontStyle = FontStyles.Bold;
            elTMP.color = TEXT_SECONDARY;
            elTMP.alignment = TextAlignmentOptions.Center;
            elTMP.enableAutoSizing = true;
            elTMP.fontSizeMin = FontSizes.AutoMinSmall;
            elTMP.fontSizeMax = FontSizes.Caption;

            GameObject entryValue = new GameObject("Value");
            entryValue.transform.SetParent(entryBadge.transform, false);
            RectTransform evRT = entryValue.AddComponent<RectTransform>();
            evRT.anchorMin = new Vector2(0, 0);
            evRT.anchorMax = new Vector2(1, 0.5f);
            evRT.sizeDelta = Vector2.zero;
            evRT.offsetMin = new Vector2(4, 2);
            evRT.offsetMax = new Vector2(-4, 0);
            TextMeshProUGUI evTMP = entryValue.AddComponent<TextMeshProUGUI>();
            evTMP.text = "$5";
            evTMP.fontSize = FontSizes.Subtitle;
            evTMP.fontStyle = FontStyles.Bold;
            evTMP.color = GOLD;
            evTMP.alignment = TextAlignmentOptions.Center;
            evTMP.enableAutoSizing = true;
            evTMP.fontSizeMin = FontSizes.AutoMinBody;
            evTMP.fontSizeMax = FontSizes.Subtitle;

            // === Join Button (right side, full height) ===
            GameObject joinBtn = new GameObject("JoinButton");
            joinBtn.transform.SetParent(root.transform, false);

            RectTransform jbRT = joinBtn.AddComponent<RectTransform>();
            jbRT.anchorMin = new Vector2(0.82f, 0);
            jbRT.anchorMax = new Vector2(1, 1);
            jbRT.offsetMin = new Vector2(4, 8);
            jbRT.offsetMax = new Vector2(-10, -8);

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
            jtTMP.text = "Join";
            jtTMP.fontSize = FontSizes.Body;
            jtTMP.fontStyle = FontStyles.Bold;
            jtTMP.color = Color.white;
            jtTMP.alignment = TextAlignmentOptions.Center;
            jtTMP.enableAutoSizing = true;
            jtTMP.fontSizeMin = FontSizes.AutoMinBody;
            jtTMP.fontSizeMax = FontSizes.Body;

            // Guardar prefab
            string path = $"{PREFAB_PATH}/Tournaments/TournamentCardUI.prefab";
            EnsureDirectoryExists(path);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            DestroyImmediate(root);

            Debug.Log($"[CashBattlePrefabBuilder] TournamentCardUI.prefab created: {path}");
        }

        private static void CreateSeparatorDot(Transform parent)
        {
            GameObject dot = new GameObject("Dot");
            dot.transform.SetParent(parent, false);

            LayoutElement dotLE = dot.AddComponent<LayoutElement>();
            dotLE.preferredWidth = 20;

            TextMeshProUGUI dotTMP = dot.AddComponent<TextMeshProUGUI>();
            dotTMP.text = "\u00B7";
            dotTMP.fontSize = FontSizes.BodySmall;
            dotTMP.color = TEXT_SECONDARY;
            dotTMP.alignment = TextAlignmentOptions.Center;
            dotTMP.fontStyle = FontStyles.Bold;
        }

        private static void EnsurePrefabDirectory()
        {
            if (!Directory.Exists(PREFAB_PATH))
            {
                Directory.CreateDirectory(PREFAB_PATH);
                AssetDatabase.Refresh();
            }
        }

        private static void EnsureDirectoryExists(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }
        }
    }
}
