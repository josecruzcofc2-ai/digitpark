using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using System.IO;
using DigitPark.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// Builder para crear prefabs CashBattleWin y CashBattleLose
    /// Estilo gold premium (win) / magenta neón (lose)
    /// Consistente con WinPanel_RealMoney y LosePanel_RealMoney existentes
    /// </summary>
    public static class CashBattleResultPanelUIBuilder
    {
        // Colores Win (Gold Premium)
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f);
        private static readonly Color GOLD_DARK = new Color(0.7f, 0.55f, 0f);
        private static readonly Color GOLD_LIGHT = new Color(1f, 0.95f, 0.6f);
        private static readonly Color PREMIUM_BG = new Color(0.1f, 0.08f, 0.02f, 0.95f);

        // Colores Lose (Magenta Neón)
        private static readonly Color MAGENTA = new Color(0.8f, 0.3f, 0.6f);
        private static readonly Color MAGENTA_LIGHT = new Color(1f, 0.5f, 0.8f);
        private static readonly Color LOSE_RED = new Color(1f, 0.4f, 0.4f);
        private static readonly Color LOSE_BG = new Color(0.06f, 0.03f, 0.08f, 0.95f);

        // Colores compartidos
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f);
        private static readonly Color PURPLE_NEON = new Color(0.7f, 0.3f, 1f);
        private static readonly Color WIN_GREEN = new Color(0.2f, 1f, 0.4f);
        private static readonly Color DARK_BG = new Color(0.02f, 0.01f, 0.01f, 0.95f);
        private static readonly Color CARD_BG = new Color(0.05f, 0.07f, 0.12f);

        private const string PREFAB_PATH = "Assets/_Project/Resources/Prefabs/WinPanels";

        [MenuItem("DigitPark/Prefabs/Games/Cash Battle Win Panel", false, 203)]
        public static void BuildWinPanel()
        {
            BuildPanel(true);
        }

        [MenuItem("DigitPark/Prefabs/Games/Cash Battle Lose Panel", false, 204)]
        public static void BuildLosePanel()
        {
            BuildPanel(false);
        }

        private static void BuildPanel(bool isWin)
        {
            if (!Directory.Exists(PREFAB_PATH))
            {
                Directory.CreateDirectory(PREFAB_PATH);
                AssetDatabase.Refresh();
            }

            string panelName = isWin ? "CashBattleWin" : "CashBattleLose";
            GameObject panel = CreateCashBattlePanel(isWin);

            string prefabPath = $"{PREFAB_PATH}/{panelName}.prefab";
            if (File.Exists(prefabPath))
                AssetDatabase.DeleteAsset(prefabPath);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(panel, prefabPath);
            Object.DestroyImmediate(panel);

            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);

            Debug.Log($"[CashBattleResultPanelUIBuilder] Prefab creado: {prefabPath}");
            if (!AllScenesBatchBuilder.SilentMode)
                EditorUtility.DisplayDialog("Prefab Creado",
                    $"{panelName} prefab guardado en:\n{prefabPath}\n\nAsignalo al ResultPanelManager.",
                    "OK");
        }

        private static GameObject CreateCashBattlePanel(bool isWin)
        {
            Color accentColor = isWin ? GOLD : MAGENTA;
            Color panelBg = isWin ? PREMIUM_BG : LOSE_BG;
            Color moneyColor = isWin ? WIN_GREEN : LOSE_RED;

            // Root
            GameObject panel = new GameObject(isWin ? "CashBattleWin" : "CashBattleLose");
            RectTransform panelRt = panel.AddComponent<RectTransform>();
            SetFullStretch(panelRt);

            Image overlay = panel.AddComponent<Image>();
            overlay.color = DARK_BG;

            CanvasGroup cg = panel.AddComponent<CanvasGroup>();

            // Content (650x720)
            GameObject content = CreateChild(panel.transform, "Content");
            SetupRectTransform(content, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(650, 720));

            CreatePanel3DEffect(content.transform, panelBg, accentColor);
            GameObject face = content.transform.Find("Face").gameObject;

            // === HEADER ===
            CreateHeader(face.transform, isWin, accentColor);

            // === MONEY DISPLAY ===
            CreateMoneyDisplay(face.transform, isWin, moneyColor, accentColor);

            // === VS SECTION ===
            CreateVSSection(face.transform, isWin);

            // === FEE/PRIZE INFO ===
            CreateFeeInfo(face.transform, isWin);

            // === BUTTONS ===
            CreateButtons(face.transform, accentColor, isWin);

            // Particles for win
            if (isWin)
            {
                GameObject particles = CreateChild(panel.transform, "ParticleEffects");
                SetFullStretch(particles.GetComponent<RectTransform>());
                particles.AddComponent<UISparkleEffect>();
            }

            // Controller
            CashBattleResultPanelController controller = panel.AddComponent<CashBattleResultPanelController>();
            AssignReferences(controller, panel, content, isWin);

            return panel;
        }

        private static void CreateHeader(Transform parent, bool isWin, Color accentColor)
        {
            // Title
            GameObject title = CreateChild(parent, "Title");
            SetupRectTransform(title, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -55), new Vector2(0, 70));
            string titleStr = isWin ? "YOU WON!" : "YOU LOST";
            TextMeshProUGUI titleTmp = AddText(title, titleStr, 52, accentColor, FontStyles.Bold);
            AddGlow(title, accentColor, 4);

            // Subtitle
            GameObject subtitle = CreateChild(parent, "ResultSubtitleText");
            SetupRectTransform(subtitle, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -110), new Vector2(0, 35));
            string subtitleStr = isWin ? "Cash Battle Victory" : "Next time will be yours";
            AddText(subtitle, subtitleStr, 22, new Color(0.7f, 0.6f, 0.5f), FontStyles.Bold);
        }

        private static void CreateMoneyDisplay(Transform parent, bool isWin, Color moneyColor, Color accentColor)
        {
            GameObject moneyContainer = CreateChild(parent, "MoneyContainer");
            SetupRectTransform(moneyContainer, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -185), new Vector2(400, 90));

            Image moneyBg = moneyContainer.AddComponent<Image>();
            moneyBg.color = isWin
                ? new Color(0.15f, 0.12f, 0.02f, 0.8f)
                : new Color(0.12f, 0.04f, 0.06f, 0.8f);
            AddGlow(moneyContainer, moneyColor, 3);

            // Money result text
            GameObject moneyResult = CreateChild(moneyContainer.transform, "MoneyResult");
            SetupRectTransform(moneyResult, Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero);
            string moneyStr = isWin ? "+$9.00" : "-$5.00";
            AddText(moneyResult, moneyStr, 52, moneyColor, FontStyles.Bold);

            // Entry fee text
            GameObject entryFee = CreateChild(parent, "EntryFee");
            SetupRectTransform(entryFee, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -250), new Vector2(350, 30));
            AddText(entryFee, "Entry: $5.00", 20, new Color(0.6f, 0.55f, 0.45f), FontStyles.Bold);

            // Winner share info
            GameObject winnerShare = CreateChild(parent, "WinnerShare");
            SetupRectTransform(winnerShare, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -278), new Vector2(350, 25));
            AddText(winnerShare, isWin ? "Winner: 90% of the pot" : "", 16,
                new Color(0.5f, 0.5f, 0.4f), FontStyles.Bold);
        }

        private static void CreateVSSection(Transform parent, bool isWin)
        {
            GameObject vsContainer = CreateChild(parent, "VSContainer");
            SetupRectTransform(vsContainer, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -380), new Vector2(580, 140));

            Image vsBg = vsContainer.AddComponent<Image>();
            vsBg.color = new Color(0.04f, 0.04f, 0.06f, 0.7f);

            Outline vsBorder = vsContainer.AddComponent<Outline>();
            vsBorder.effectColor = new Color(isWin ? GOLD.r : MAGENTA.r,
                isWin ? GOLD.g : MAGENTA.g, isWin ? GOLD.b : MAGENTA.b, 0.4f);
            vsBorder.effectDistance = new Vector2(2, -2);

            // Player card (left)
            CreatePlayerCard(vsContainer.transform, true, isWin);

            // VS text
            GameObject vsText = CreateChild(vsContainer.transform, "VSText");
            RectTransform vsRt = vsText.GetComponent<RectTransform>();
            vsRt.anchorMin = new Vector2(0.4f, 0.35f);
            vsRt.anchorMax = new Vector2(0.6f, 0.65f);
            vsRt.offsetMin = Vector2.zero;
            vsRt.offsetMax = Vector2.zero;
            AddText(vsText, "VS", 28, isWin ? GOLD_DARK : MAGENTA, FontStyles.Bold);

            // Opponent card (right)
            CreatePlayerCard(vsContainer.transform, false, isWin);
        }

        private static void CreatePlayerCard(Transform parent, bool isPlayer, bool playerWon)
        {
            string cardName = isPlayer ? "PlayerCard" : "OpponentCard";
            float anchorMinX = isPlayer ? 0.02f : 0.52f;
            float anchorMaxX = isPlayer ? 0.48f : 0.98f;

            Color cardColor = isPlayer ? CYAN_NEON : PURPLE_NEON;
            bool isWinner = (isPlayer && playerWon) || (!isPlayer && !playerWon);

            GameObject card = CreateChild(parent, cardName);
            RectTransform cardRt = card.GetComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(anchorMinX, 0.05f);
            cardRt.anchorMax = new Vector2(anchorMaxX, 0.95f);
            cardRt.offsetMin = Vector2.zero;
            cardRt.offsetMax = Vector2.zero;

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = CARD_BG;

            // Winner highlight
            if (isWinner)
            {
                Outline winnerBorder = card.AddComponent<Outline>();
                winnerBorder.effectColor = WIN_GREEN;
                winnerBorder.effectDistance = new Vector2(3, -3);
            }

            // Highlight overlay
            GameObject highlight = CreateChild(card.transform, isPlayer ? "PlayerHighlight" : "OpponentHighlight");
            SetFullStretch(highlight.GetComponent<RectTransform>());
            Image highlightImg = highlight.AddComponent<Image>();
            highlightImg.color = new Color(WIN_GREEN.r, WIN_GREEN.g, WIN_GREEN.b, 0.08f);
            highlight.SetActive(isWinner);

            // Name
            GameObject nameObj = CreateChild(card.transform, isPlayer ? "PlayerName" : "OpponentName");
            RectTransform nameRt = nameObj.GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0.05f, 0.7f);
            nameRt.anchorMax = new Vector2(0.95f, 0.95f);
            nameRt.offsetMin = Vector2.zero;
            nameRt.offsetMax = Vector2.zero;
            AddText(nameObj, isPlayer ? "YOU" : "OPPONENT", 22, cardColor, FontStyles.Bold);

            // Time
            GameObject timeObj = CreateChild(card.transform, isPlayer ? "PlayerTime" : "OpponentTime");
            RectTransform timeRt = timeObj.GetComponent<RectTransform>();
            timeRt.anchorMin = new Vector2(0.05f, 0.35f);
            timeRt.anchorMax = new Vector2(0.95f, 0.7f);
            timeRt.offsetMin = Vector2.zero;
            timeRt.offsetMax = Vector2.zero;
            AddText(timeObj, "12.45s", 38, Color.white, FontStyles.Bold);

            // Errors
            GameObject errorsObj = CreateChild(card.transform, isPlayer ? "PlayerErrors" : "OpponentErrors");
            RectTransform errRt = errorsObj.GetComponent<RectTransform>();
            errRt.anchorMin = new Vector2(0.1f, 0.05f);
            errRt.anchorMax = new Vector2(0.9f, 0.35f);
            errRt.offsetMin = Vector2.zero;
            errRt.offsetMax = Vector2.zero;
            AddText(errorsObj, "0 errors", 18, new Color(0.6f, 0.6f, 0.6f), FontStyles.Bold);
        }

        private static void CreateFeeInfo(Transform parent, bool isWin)
        {
            // This section already created in MoneyDisplay
            // Additional info can go here if needed
        }

        private static void CreateButtons(Transform parent, Color accentColor, bool isWin)
        {
            GameObject buttonsContainer = CreateChild(parent, "ButtonsContainer");
            SetupRectTransform(buttonsContainer, new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 55), new Vector2(-40, 95));

            HorizontalLayoutGroup hlg = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 25;
            hlg.childForceExpandWidth = false;
            hlg.childControlWidth = false;

            // New Match button
            CreateButton3D(buttonsContainer.transform, "NewMatchButton", "NEW MATCH",
                isWin ? CYAN_NEON : MAGENTA, 230, 70);

            // Continue button
            CreateButton3D(buttonsContainer.transform, "ContinueButton", "CONTINUE",
                isWin ? accentColor : new Color(0.4f, 0.35f, 0.4f), 200, 70);
        }

        private static void AssignReferences(CashBattleResultPanelController controller,
            GameObject panel, GameObject content, bool isWin)
        {
            SerializedObject so = new SerializedObject(controller);

            so.FindProperty("canvasGroup").objectReferenceValue = panel.GetComponent<CanvasGroup>();
            so.FindProperty("content").objectReferenceValue = content;

            Transform face = content.transform.Find("Face");
            if (face != null)
            {
                // Header
                so.FindProperty("titleText").objectReferenceValue =
                    face.Find("Title")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("subtitleText").objectReferenceValue =
                    face.Find("Subtitle")?.GetComponent<TextMeshProUGUI>();

                // Money
                so.FindProperty("moneyResultText").objectReferenceValue =
                    face.Find("MoneyContainer/MoneyResult")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("entryFeeText").objectReferenceValue =
                    face.Find("EntryFee")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("winnerShareText").objectReferenceValue =
                    face.Find("WinnerShare")?.GetComponent<TextMeshProUGUI>();

                // VS Section
                so.FindProperty("playerNameText").objectReferenceValue =
                    face.Find("VSContainer/PlayerCard/PlayerName")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("playerTimeText").objectReferenceValue =
                    face.Find("VSContainer/PlayerCard/PlayerTime")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("playerErrorsText").objectReferenceValue =
                    face.Find("VSContainer/PlayerCard/PlayerErrors")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("playerHighlight").objectReferenceValue =
                    face.Find("VSContainer/PlayerCard/PlayerHighlight")?.gameObject;

                so.FindProperty("opponentNameText").objectReferenceValue =
                    face.Find("VSContainer/OpponentCard/OpponentName")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("opponentTimeText").objectReferenceValue =
                    face.Find("VSContainer/OpponentCard/OpponentTime")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("opponentErrorsText").objectReferenceValue =
                    face.Find("VSContainer/OpponentCard/OpponentErrors")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("opponentHighlight").objectReferenceValue =
                    face.Find("VSContainer/OpponentCard/OpponentHighlight")?.gameObject;

                so.FindProperty("vsText").objectReferenceValue =
                    face.Find("VSContainer/VSText")?.GetComponent<TextMeshProUGUI>();

                // Buttons
                so.FindProperty("continueButton").objectReferenceValue =
                    face.Find("ButtonsContainer/ContinueButton")?.GetComponent<Button>();
                so.FindProperty("newMatchButton").objectReferenceValue =
                    face.Find("ButtonsContainer/NewMatchButton")?.GetComponent<Button>();

                var continueBtn = face.Find("ButtonsContainer/ContinueButton/Face/Text");
                if (continueBtn != null)
                    so.FindProperty("continueButtonText").objectReferenceValue = continueBtn.GetComponent<TextMeshProUGUI>();
                var newMatchBtn = face.Find("ButtonsContainer/NewMatchButton/Face/Text");
                if (newMatchBtn != null)
                    so.FindProperty("newMatchButtonText").objectReferenceValue = newMatchBtn.GetComponent<TextMeshProUGUI>();
            }

            // Sparkle effect
            if (isWin)
            {
                so.FindProperty("sparkleEffect").objectReferenceValue =
                    panel.transform.Find("ParticleEffects")?.GetComponent<UISparkleEffect>();
            }

            so.ApplyModifiedProperties();
        }

        // ====================================================================
        // UTILITY METHODS
        // ====================================================================

        private static GameObject CreateChild(Transform parent, string name)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            return obj;
        }

        private static void SetFullStretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }

        private static void SetupRectTransform(GameObject obj, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt == null) rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;
        }

        private static TextMeshProUGUI AddText(GameObject obj, string text, float size, Color color, FontStyles style)
        {
            TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = false;
            tmp.raycastTarget = false;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinBody;
            tmp.fontSizeMax = size;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            return tmp;
        }

        private static void AddGlow(GameObject obj, Color color, float distance)
        {
            Outline outline = obj.GetComponent<Outline>();
            if (outline == null) outline = obj.AddComponent<Outline>();
            outline.effectColor = new Color(color.r, color.g, color.b, 0.6f);
            outline.effectDistance = new Vector2(distance, -distance);
        }

        private static void CreatePanel3DEffect(Transform parent, Color bgColor, Color glowColor)
        {
            GameObject shadow = CreateChild(parent, "Shadow");
            RectTransform shadowRt = shadow.GetComponent<RectTransform>();
            shadowRt.anchorMin = Vector2.zero;
            shadowRt.anchorMax = Vector2.one;
            shadowRt.sizeDelta = Vector2.zero;
            shadowRt.anchoredPosition = new Vector2(6, -12);
            Image shadowImg = shadow.AddComponent<Image>();
            shadowImg.color = new Color(0, 0, 0, 0.5f);

            GameObject side = CreateChild(parent, "Side");
            SetupRectTransform(side, new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, -6), new Vector2(0, 12));
            Image sideImg = side.AddComponent<Image>();
            sideImg.color = new Color(glowColor.r * 0.4f, glowColor.g * 0.4f, glowColor.b * 0.4f, 1f);

            GameObject face = CreateChild(parent, "Face");
            SetFullStretch(face.GetComponent<RectTransform>());
            face.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 6);
            Image faceImg = face.AddComponent<Image>();
            faceImg.color = bgColor;
            AddGlow(face, glowColor, 3);
        }

        private static GameObject CreateButton3D(Transform parent, string name, string text, Color color, float width, float height)
        {
            GameObject btn = CreateChild(parent, name);
            LayoutElement layout = btn.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = height;

            GameObject shadow = CreateChild(btn.transform, "Shadow");
            SetupRectTransform(shadow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(4, -8), new Vector2(width, height));
            Image shadowImg = shadow.AddComponent<Image>();
            shadowImg.color = new Color(0, 0, 0, 0.4f);

            GameObject side = CreateChild(btn.transform, "Side");
            SetupRectTransform(side, new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 0), new Vector2(width, 10));
            Image sideImg = side.AddComponent<Image>();
            sideImg.color = new Color(color.r * 0.5f, color.g * 0.5f, color.b * 0.5f, 1f);

            GameObject face = CreateChild(btn.transform, "Face");
            SetupRectTransform(face, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 5), new Vector2(width, height - 10));
            Image faceImg = face.AddComponent<Image>();
            faceImg.color = color;

            GameObject textObj = CreateChild(face.transform, "Text");
            SetupRectTransform(textObj, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-10, -6));
            float fontSize = Mathf.Min(height * 0.35f, FontSizes.Body);
            AddText(textObj, text, fontSize, new Color(0.02f, 0.02f, 0.05f), FontStyles.Bold);

            Button button = btn.AddComponent<Button>();
            button.targetGraphic = faceImg;

            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.fadeDuration = 0.1f;
            button.colors = colors;

            return btn;
        }
    }
}
