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
        private const string GAME_ICONS_PATH = "Assets/_Project/Art/Icons/Games/";
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

            // === Card 340px - espacio suficiente para icon 160 + contenido sin solaparse ===
            GameObject root = new GameObject("TournamentCardUI");
            RectTransform rootRT = root.AddComponent<RectTransform>();
            rootRT.sizeDelta = new Vector2(0, 340);

            LayoutElement rootLE = root.AddComponent<LayoutElement>();
            rootLE.preferredHeight = 340;
            rootLE.minHeight = 340;

            Image rootBg = root.AddComponent<Image>();
            rootBg.color = CARD_BG;

            Button rootBtn = root.AddComponent<Button>();
            rootBtn.targetGraphic = rootBg;
            ColorBlock colors = rootBtn.colors;
            colors.highlightedColor = CARD_BG * 1.3f;
            rootBtn.colors = colors;

            // Border dorado inferior (igual que CashHistory ColorBar)
            GameObject colorBar = new GameObject("ColorBar");
            colorBar.transform.SetParent(root.transform, false);
            RectTransform barRT = colorBar.AddComponent<RectTransform>();
            barRT.anchorMin = new Vector2(0, 0);
            barRT.anchorMax = new Vector2(1, 0);
            barRT.pivot = new Vector2(0.5f, 0);
            barRT.sizeDelta = new Vector2(0, 8);
            barRT.anchoredPosition = Vector2.zero;
            Image barImg = colorBar.AddComponent<Image>();
            barImg.color = GOLD;

            // === LIVE Badge (arriba derecha) - proporción CashHistory ModeBadge ===
            GameObject liveBadge = new GameObject("LiveBadge");
            liveBadge.transform.SetParent(root.transform, false);
            liveBadge.SetActive(false);

            RectTransform liveRT = liveBadge.AddComponent<RectTransform>();
            liveRT.anchorMin = new Vector2(1, 1);
            liveRT.anchorMax = new Vector2(1, 1);
            liveRT.pivot = new Vector2(1, 1);
            liveRT.sizeDelta = new Vector2(120, 46);
            liveRT.anchoredPosition = new Vector2(-20, -15);

            Image liveBg = liveBadge.AddComponent<Image>();
            liveBg.color = new Color(1f, 0.3f, 0.2f, 1f);

            GameObject liveIcon = new GameObject("Icon");
            liveIcon.transform.SetParent(liveBadge.transform, false);
            RectTransform liRT = liveIcon.AddComponent<RectTransform>();
            liRT.anchorMin = new Vector2(0, 0.5f);
            liRT.anchorMax = new Vector2(0, 0.5f);
            liRT.pivot = new Vector2(0, 0.5f);
            liRT.sizeDelta = new Vector2(30, 30);
            liRT.anchoredPosition = new Vector2(8, 0);
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
            ltRT.offsetMin = new Vector2(38, 0);
            ltRT.offsetMax = new Vector2(-5, 0);
            TextMeshProUGUI ltTMP = liveText.AddComponent<TextMeshProUGUI>();
            ltTMP.text = "LIVE";
            ltTMP.fontSize = 22;
            ltTMP.fontStyle = FontStyles.Bold;
            ltTMP.color = Color.white;
            ltTMP.alignment = TextAlignmentOptions.Center;

            // === Game Icon (izquierda) - 160x160 centrado en card 340px ===
            GameObject gameIcon = new GameObject("GameIcon");
            gameIcon.transform.SetParent(root.transform, false);

            RectTransform giRT = gameIcon.AddComponent<RectTransform>();
            giRT.anchorMin = new Vector2(0, 0.5f);
            giRT.anchorMax = new Vector2(0, 0.5f);
            giRT.pivot = new Vector2(0, 0.5f);
            giRT.sizeDelta = new Vector2(160, 160);
            giRT.anchoredPosition = new Vector2(25, 0);

            Image giBg = gameIcon.AddComponent<Image>();
            giBg.color = new Color(0.05f, 0.05f, 0.08f, 1f);

            // Child sprite para el icono del juego
            GameObject iconSprite = new GameObject("Sprite");
            iconSprite.transform.SetParent(gameIcon.transform, false);
            RectTransform isRT = iconSprite.AddComponent<RectTransform>();
            isRT.anchorMin = Vector2.zero;
            isRT.anchorMax = Vector2.one;
            isRT.sizeDelta = new Vector2(-20, -20);
            isRT.anchoredPosition = Vector2.zero;
            Image isImg = iconSprite.AddComponent<Image>();
            isImg.preserveAspect = true;
            Sprite gameSprite = AssetDatabase.LoadAssetAtPath<Sprite>(GAME_ICONS_PATH + "QuickMathIcon.png");
            if (gameSprite != null) isImg.sprite = gameSprite;
            isImg.color = Color.white;

            // === Tournament Name - fila superior del lado derecho ===
            GameObject nameObj = new GameObject("TournamentName");
            nameObj.transform.SetParent(root.transform, false);

            RectTransform nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 1);
            nameRT.anchorMax = new Vector2(0.58f, 1);
            nameRT.pivot = new Vector2(0, 1);
            nameRT.anchoredPosition = new Vector2(210, -25);
            nameRT.sizeDelta = new Vector2(0, 60);

            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "Quick Math Championship";
            nameText.fontSize = 44;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = TEXT_WHITE;
            nameText.alignment = TextAlignmentOptions.Left;
            nameText.overflowMode = TextOverflowModes.Ellipsis;

            // === Prize Row - segunda fila, debajo del nombre ===
            GameObject prizeRow = new GameObject("PrizeRow");
            prizeRow.transform.SetParent(root.transform, false);

            RectTransform prRT = prizeRow.AddComponent<RectTransform>();
            prRT.anchorMin = new Vector2(0, 1);
            prRT.anchorMax = new Vector2(0, 1);
            prRT.pivot = new Vector2(0, 1);
            prRT.sizeDelta = new Vector2(300, 50);
            prRT.anchoredPosition = new Vector2(210, -95);

            GameObject prizeIcon = new GameObject("Icon");
            prizeIcon.transform.SetParent(prizeRow.transform, false);
            RectTransform piRT = prizeIcon.AddComponent<RectTransform>();
            piRT.anchorMin = new Vector2(0, 0.5f);
            piRT.anchorMax = new Vector2(0, 0.5f);
            piRT.pivot = new Vector2(0, 0.5f);
            piRT.sizeDelta = new Vector2(40, 40);
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
            ptRT.offsetMin = new Vector2(48, 0);
            ptRT.offsetMax = Vector2.zero;
            TextMeshProUGUI ptTMP = prizeText.AddComponent<TextMeshProUGUI>();
            ptTMP.text = "$500";
            ptTMP.fontSize = 36;
            ptTMP.fontStyle = FontStyles.Bold;
            ptTMP.color = GREEN;
            ptTMP.alignment = TextAlignmentOptions.Left;

            // === Players + Timer Row (abajo izquierda del contenido) ===
            GameObject playersRow = new GameObject("PlayersRow");
            playersRow.transform.SetParent(root.transform, false);

            RectTransform plRT = playersRow.AddComponent<RectTransform>();
            plRT.anchorMin = new Vector2(0, 0);
            plRT.anchorMax = new Vector2(0, 0);
            plRT.pivot = new Vector2(0, 0);
            plRT.sizeDelta = new Vector2(180, 48);
            plRT.anchoredPosition = new Vector2(210, 25);

            GameObject playersIcon = new GameObject("Icon");
            playersIcon.transform.SetParent(playersRow.transform, false);
            RectTransform pliRT = playersIcon.AddComponent<RectTransform>();
            pliRT.anchorMin = new Vector2(0, 0.5f);
            pliRT.anchorMax = new Vector2(0, 0.5f);
            pliRT.pivot = new Vector2(0, 0.5f);
            pliRT.sizeDelta = new Vector2(36, 36);
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
            pltRT.offsetMin = new Vector2(42, 0);
            pltRT.offsetMax = Vector2.zero;
            TextMeshProUGUI pltTMP = playersText.AddComponent<TextMeshProUGUI>();
            pltTMP.text = "15/20";
            pltTMP.fontSize = 32;
            pltTMP.color = TEXT_SECONDARY;
            pltTMP.alignment = TextAlignmentOptions.Left;

            // Timer al lado de players
            GameObject timerRow = new GameObject("TimerRow");
            timerRow.transform.SetParent(root.transform, false);

            RectTransform trRT = timerRow.AddComponent<RectTransform>();
            trRT.anchorMin = new Vector2(0, 0);
            trRT.anchorMax = new Vector2(0, 0);
            trRT.pivot = new Vector2(0, 0);
            trRT.sizeDelta = new Vector2(220, 48);
            trRT.anchoredPosition = new Vector2(410, 25);

            GameObject timerIcon = new GameObject("Icon");
            timerIcon.transform.SetParent(timerRow.transform, false);
            RectTransform tiRT = timerIcon.AddComponent<RectTransform>();
            tiRT.anchorMin = new Vector2(0, 0.5f);
            tiRT.anchorMax = new Vector2(0, 0.5f);
            tiRT.pivot = new Vector2(0, 0.5f);
            tiRT.sizeDelta = new Vector2(36, 36);
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
            ttRT.offsetMin = new Vector2(42, 0);
            ttRT.offsetMax = Vector2.zero;
            TextMeshProUGUI ttTMP = timerText.AddComponent<TextMeshProUGUI>();
            ttTMP.text = "02:45:00";
            ttTMP.fontSize = 32;
            ttTMP.color = CYAN;
            ttTMP.alignment = TextAlignmentOptions.Left;

            // === Entry Fee (arriba derecha, debajo del LiveBadge) ===
            GameObject entryBadge = new GameObject("EntryFeeBadge");
            entryBadge.transform.SetParent(root.transform, false);

            RectTransform ebRT = entryBadge.AddComponent<RectTransform>();
            ebRT.anchorMin = new Vector2(1, 1);
            ebRT.anchorMax = new Vector2(1, 1);
            ebRT.pivot = new Vector2(1, 1);
            ebRT.sizeDelta = new Vector2(180, 80);
            ebRT.anchoredPosition = new Vector2(-20, -70);

            Image ebBg = entryBadge.AddComponent<Image>();
            ebBg.color = new Color(0, 0, 0, 0.4f);

            GameObject entryLabel = new GameObject("Label");
            entryLabel.transform.SetParent(entryBadge.transform, false);
            RectTransform elRT = entryLabel.AddComponent<RectTransform>();
            elRT.anchorMin = new Vector2(0, 0.5f);
            elRT.anchorMax = new Vector2(1, 1);
            elRT.sizeDelta = Vector2.zero;
            elRT.offsetMin = new Vector2(8, 0);
            elRT.offsetMax = new Vector2(-8, -4);
            TextMeshProUGUI elTMP = entryLabel.AddComponent<TextMeshProUGUI>();
            elTMP.text = "Entrada";
            elTMP.fontSize = 24;
            elTMP.color = TEXT_SECONDARY;
            elTMP.alignment = TextAlignmentOptions.Center;

            GameObject entryValue = new GameObject("Value");
            entryValue.transform.SetParent(entryBadge.transform, false);
            RectTransform evRT = entryValue.AddComponent<RectTransform>();
            evRT.anchorMin = new Vector2(0, 0);
            evRT.anchorMax = new Vector2(1, 0.5f);
            evRT.sizeDelta = Vector2.zero;
            evRT.offsetMin = new Vector2(8, 4);
            evRT.offsetMax = new Vector2(-8, 0);
            TextMeshProUGUI evTMP = entryValue.AddComponent<TextMeshProUGUI>();
            evTMP.text = "$5";
            evTMP.fontSize = 36;
            evTMP.fontStyle = FontStyles.Bold;
            evTMP.color = GOLD;
            evTMP.alignment = TextAlignmentOptions.Center;

            // === Join Button (abajo derecha) ===
            GameObject joinBtn = new GameObject("JoinButton");
            joinBtn.transform.SetParent(root.transform, false);

            RectTransform jbRT = joinBtn.AddComponent<RectTransform>();
            jbRT.anchorMin = new Vector2(1, 0);
            jbRT.anchorMax = new Vector2(1, 0);
            jbRT.pivot = new Vector2(1, 0);
            jbRT.sizeDelta = new Vector2(180, 58);
            jbRT.anchoredPosition = new Vector2(-20, 20);

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
            jtTMP.fontSize = 30;
            jtTMP.fontStyle = FontStyles.Bold;
            jtTMP.color = Color.white;
            jtTMP.alignment = TextAlignmentOptions.Center;

            // Guardar prefab
            string path = $"{PREFAB_PATH}/Tournaments/TournamentCardUI.prefab";
            EnsureDirectoryExists(path);
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
