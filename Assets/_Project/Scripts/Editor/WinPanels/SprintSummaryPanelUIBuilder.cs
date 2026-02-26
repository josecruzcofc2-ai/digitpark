using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using System.IO;
using DigitPark.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// Builder para crear el prefab SprintSummaryPanel
    /// Resumen de Cognitive Sprint con tabla de juegos, totales, VS y money sections
    /// Estilo neón azul/púrpura (gradiente competitivo)
    /// </summary>
    public static class SprintSummaryPanelUIBuilder
    {
        // Colores neón Sprint
        private static readonly Color BLUE_NEON = new Color(0.2f, 0.5f, 1f);
        private static readonly Color PURPLE_NEON = new Color(0.6f, 0.3f, 1f);
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f);
        private static readonly Color WIN_GREEN = new Color(0.2f, 1f, 0.4f);
        private static readonly Color LOSE_RED = new Color(1f, 0.3f, 0.3f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f);
        private static readonly Color DARK_BG = new Color(0.02f, 0.03f, 0.08f, 0.95f);
        private static readonly Color PANEL_BG = new Color(0.04f, 0.06f, 0.14f, 0.95f);
        private static readonly Color ROW_BG_EVEN = new Color(0.06f, 0.08f, 0.16f, 0.8f);
        private static readonly Color ROW_BG_ODD = new Color(0.04f, 0.06f, 0.12f, 0.8f);
        private static readonly Color CARD_BG = new Color(0.05f, 0.07f, 0.15f);
        private static readonly Color SECTION_BORDER = new Color(0.3f, 0.4f, 0.8f, 0.6f);

        private const string PREFAB_PATH = "Assets/_Project/Resources/Prefabs/WinPanels";

        [MenuItem("DigitPark/Prefabs/Games/Sprint Summary Panel", false, 200)]
        public static void BuildSprintSummaryPanel()
        {
            if (!Directory.Exists(PREFAB_PATH))
            {
                Directory.CreateDirectory(PREFAB_PATH);
                AssetDatabase.Refresh();
            }

            GameObject panel = CreateSprintSummaryPanel();

            string prefabPath = $"{PREFAB_PATH}/SprintSummaryPanel.prefab";
            if (File.Exists(prefabPath))
                AssetDatabase.DeleteAsset(prefabPath);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(panel, prefabPath);
            Object.DestroyImmediate(panel);

            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);

            Debug.Log($"[SprintSummaryPanelUIBuilder] Prefab creado: {prefabPath}");
            EditorUtility.DisplayDialog("Prefab Creado",
                $"SprintSummaryPanel prefab guardado en:\n{prefabPath}\n\nAsignalo al ResultPanelManager.",
                "OK");
        }

        private static GameObject CreateSprintSummaryPanel()
        {
            // Root panel (full screen overlay)
            GameObject panel = new GameObject("SprintSummaryPanel");
            RectTransform panelRt = panel.AddComponent<RectTransform>();
            SetFullStretch(panelRt);

            Image overlay = panel.AddComponent<Image>();
            overlay.color = DARK_BG;

            CanvasGroup cg = panel.AddComponent<CanvasGroup>();

            // Content container (700x800)
            GameObject content = CreateChild(panel.transform, "Content");
            SetupRectTransform(content, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(700, 800));

            // Panel 3D effect
            CreatePanel3DEffect(content.transform, PANEL_BG, BLUE_NEON);
            GameObject face = content.transform.Find("Face").gameObject;

            // === HEADER ===
            CreateHeader(face.transform);

            // === GAMES TABLE ===
            CreateGamesTable(face.transform);

            // === TOTALS ===
            CreateTotals(face.transform);

            // === VS SECTION (ocultable) ===
            CreateVSSection(face.transform);

            // === MONEY SECTION (ocultable) ===
            CreateMoneySection(face.transform);

            // === BUTTONS ===
            CreateButtons(face.transform);

            // Add controller
            SprintSummaryPanelController controller = panel.AddComponent<SprintSummaryPanelController>();
            AssignReferences(controller, panel, content);

            return panel;
        }

        private static void CreateHeader(Transform parent)
        {
            // Header icon
            GameObject iconContainer = CreateChild(parent, "HeaderIcon");
            SetupRectTransform(iconContainer, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -50), new Vector2(80, 80));
            Image iconBg = iconContainer.AddComponent<Image>();
            iconBg.color = new Color(0.1f, 0.12f, 0.25f, 0.9f);
            AddGlow(iconContainer, BLUE_NEON, 3);

            GameObject icon = CreateChild(iconContainer.transform, "Icon");
            SetupRectTransform(icon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(55, 55));
            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = CYAN_NEON;
            iconImg.preserveAspect = true;

            // Title
            GameObject title = CreateChild(parent, "Title");
            SetupRectTransform(title, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -120), new Vector2(0, 60));
            TextMeshProUGUI titleTmp = AddText(title, "SPRINT COMPLETE!", 42, CYAN_NEON, FontStyles.Bold);
            AddGlow(title, BLUE_NEON, 3);

            // Subtitle
            GameObject subtitle = CreateChild(parent, "Subtitle");
            SetupRectTransform(subtitle, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -165), new Vector2(0, 35));
            AddText(subtitle, "Sprint Summary", 22, new Color(0.6f, 0.7f, 0.9f), FontStyles.Italic);
        }

        private static void CreateGamesTable(Transform parent)
        {
            // Table container
            GameObject tableContainer = CreateChild(parent, "GamesTable");
            SetupRectTransform(tableContainer, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -280), new Vector2(620, 200));

            Image tableBg = tableContainer.AddComponent<Image>();
            tableBg.color = new Color(0.03f, 0.05f, 0.1f, 0.7f);

            Outline tableBorder = tableContainer.AddComponent<Outline>();
            tableBorder.effectColor = SECTION_BORDER;
            tableBorder.effectDistance = new Vector2(2, -2);

            // Table header row
            GameObject headerRow = CreateChild(tableContainer.transform, "HeaderRow");
            SetupRectTransform(headerRow, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -5), new Vector2(-20, 30));
            headerRow.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -5);

            Image headerBg = headerRow.AddComponent<Image>();
            headerBg.color = new Color(0.08f, 0.1f, 0.2f, 0.9f);

            // Header columns: Game | Time | Errors
            CreateTableHeaderCell(headerRow.transform, "GameHeader", "GAME", 0f, 0.4f);
            CreateTableHeaderCell(headerRow.transform, "TimeHeader", "TIME", 0.4f, 0.7f);
            CreateTableHeaderCell(headerRow.transform, "ErrorsHeader", "ERRORS", 0.7f, 1f);

            // Rows container (populated at runtime)
            GameObject rowsContainer = CreateChild(tableContainer.transform, "GameRowsContainer");
            SetupRectTransform(rowsContainer, new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(0, 0), new Vector2(-20, -40));
            rowsContainer.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            rowsContainer.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            rowsContainer.GetComponent<RectTransform>().offsetMin = new Vector2(10, 10);
            rowsContainer.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -40);

            VerticalLayoutGroup vlg = rowsContainer.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.spacing = 4;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;

            // Template row (hidden by default)
            GameObject templateRow = CreateGameRow(rowsContainer.transform, "GameRowTemplate", true);
            templateRow.SetActive(false);
        }

        private static void CreateTableHeaderCell(Transform parent, string name, string text, float anchorMinX, float anchorMaxX)
        {
            GameObject cell = CreateChild(parent, name);
            RectTransform rt = cell.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(anchorMinX, 0);
            rt.anchorMax = new Vector2(anchorMaxX, 1);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = AddText(cell, text, 16, new Color(0.5f, 0.6f, 0.8f), FontStyles.Bold);
            tmp.alignment = TextAlignmentOptions.Center;
        }

        private static GameObject CreateGameRow(Transform parent, string name, bool isTemplate)
        {
            GameObject row = CreateChild(parent, name);
            LayoutElement le = row.AddComponent<LayoutElement>();
            le.preferredHeight = 32;

            Image rowBg = row.AddComponent<Image>();
            rowBg.color = isTemplate ? ROW_BG_EVEN : ROW_BG_ODD;

            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 5;
            hlg.childForceExpandWidth = true;
            hlg.childControlWidth = true;
            hlg.padding = new RectOffset(10, 10, 2, 2);

            // Game name
            GameObject gameNameObj = CreateChild(row.transform, "GameName");
            LayoutElement gameLE = gameNameObj.AddComponent<LayoutElement>();
            gameLE.flexibleWidth = 2;
            TextMeshProUGUI gameTmp = AddText(gameNameObj, "DigitRush", 18, Color.white, FontStyles.Normal);
            gameTmp.alignment = TextAlignmentOptions.Left;

            // Time
            GameObject timeObj = CreateChild(row.transform, "Time");
            LayoutElement timeLE = timeObj.AddComponent<LayoutElement>();
            timeLE.flexibleWidth = 1.5f;
            TextMeshProUGUI timeTmp = AddText(timeObj, "12.45s", 18, CYAN_NEON, FontStyles.Bold);
            timeTmp.alignment = TextAlignmentOptions.Center;

            // Errors
            GameObject errorsObj = CreateChild(row.transform, "Errors");
            LayoutElement errLE = errorsObj.AddComponent<LayoutElement>();
            errLE.flexibleWidth = 1;
            TextMeshProUGUI errTmp = AddText(errorsObj, "0", 18, new Color(0.7f, 0.7f, 0.7f), FontStyles.Normal);
            errTmp.alignment = TextAlignmentOptions.Center;

            // Opponent Time (hidden for practice)
            GameObject oppTimeObj = CreateChild(row.transform, "OpponentTime");
            LayoutElement oppTimeLE = oppTimeObj.AddComponent<LayoutElement>();
            oppTimeLE.flexibleWidth = 1.5f;
            TextMeshProUGUI oppTimeTmp = AddText(oppTimeObj, "13.20s", 18, PURPLE_NEON, FontStyles.Bold);
            oppTimeTmp.alignment = TextAlignmentOptions.Center;
            oppTimeObj.SetActive(false); // Hidden by default

            // Opponent Errors (hidden for practice)
            GameObject oppErrObj = CreateChild(row.transform, "OpponentErrors");
            LayoutElement oppErrLE = oppErrObj.AddComponent<LayoutElement>();
            oppErrLE.flexibleWidth = 1;
            TextMeshProUGUI oppErrTmp = AddText(oppErrObj, "1", 18, new Color(0.5f, 0.5f, 0.5f), FontStyles.Normal);
            oppErrTmp.alignment = TextAlignmentOptions.Center;
            oppErrObj.SetActive(false); // Hidden by default

            return row;
        }

        private static void CreateTotals(Transform parent)
        {
            GameObject totalsContainer = CreateChild(parent, "TotalsContainer");
            SetupRectTransform(totalsContainer, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -415), new Vector2(620, 80));

            Image totalsBg = totalsContainer.AddComponent<Image>();
            totalsBg.color = new Color(0.06f, 0.08f, 0.18f, 0.9f);
            AddGlow(totalsContainer, BLUE_NEON, 2);

            // Total Time
            CreateStatItem(totalsContainer.transform, "TotalTimeLabel", "TotalTimeValue",
                "TOTAL TIME", "45.23s", CYAN_NEON, 0f, 0.35f);

            // Total Errors
            CreateStatItem(totalsContainer.transform, "TotalErrorsLabel", "TotalErrorsValue",
                "ERRORS", "3", new Color(0.8f, 0.6f, 0.4f), 0.35f, 0.65f);

            // Total Score
            CreateStatItem(totalsContainer.transform, "TotalScoreLabel", "TotalScoreValue",
                "SCORE", "49.23s", GOLD, 0.65f, 1f);
        }

        private static void CreateStatItem(Transform parent, string labelName, string valueName,
            string labelText, string valueText, Color valueColor, float anchorMinX, float anchorMaxX)
        {
            GameObject container = CreateChild(parent, labelName + "Container");
            RectTransform rt = container.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(anchorMinX, 0);
            rt.anchorMax = new Vector2(anchorMaxX, 1);
            rt.offsetMin = new Vector2(5, 5);
            rt.offsetMax = new Vector2(-5, -5);

            // Label
            GameObject label = CreateChild(container.transform, labelName);
            RectTransform labelRt = label.GetComponent<RectTransform>();
            labelRt.anchorMin = new Vector2(0, 0.5f);
            labelRt.anchorMax = new Vector2(1, 1);
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;
            AddText(label, labelText, 14, new Color(0.5f, 0.6f, 0.8f), FontStyles.Normal);

            // Value
            GameObject value = CreateChild(container.transform, valueName);
            RectTransform valueRt = value.GetComponent<RectTransform>();
            valueRt.anchorMin = new Vector2(0, 0);
            valueRt.anchorMax = new Vector2(1, 0.55f);
            valueRt.offsetMin = Vector2.zero;
            valueRt.offsetMax = Vector2.zero;
            AddText(value, valueText, 24, valueColor, FontStyles.Bold);
        }

        private static void CreateVSSection(Transform parent)
        {
            GameObject vsSection = CreateChild(parent, "VSSection");
            SetupRectTransform(vsSection, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -530), new Vector2(620, 90));
            vsSection.SetActive(false); // Hidden by default (shown for online/cash)

            Image vsBg = vsSection.AddComponent<Image>();
            vsBg.color = new Color(0.05f, 0.07f, 0.15f, 0.8f);

            Outline vsBorder = vsSection.AddComponent<Outline>();
            vsBorder.effectColor = SECTION_BORDER;
            vsBorder.effectDistance = new Vector2(2, -2);

            // Player name (left)
            GameObject playerName = CreateChild(vsSection.transform, "PlayerName");
            RectTransform playerRt = playerName.GetComponent<RectTransform>();
            playerRt.anchorMin = new Vector2(0, 0.5f);
            playerRt.anchorMax = new Vector2(0.35f, 1);
            playerRt.offsetMin = new Vector2(15, 5);
            playerRt.offsetMax = new Vector2(0, -5);
            AddText(playerName, "Player", 20, CYAN_NEON, FontStyles.Bold);

            // Games won score (center)
            GameObject gamesWon = CreateChild(vsSection.transform, "GamesWonText");
            RectTransform gwRt = gamesWon.GetComponent<RectTransform>();
            gwRt.anchorMin = new Vector2(0.3f, 0);
            gwRt.anchorMax = new Vector2(0.7f, 0.5f);
            gwRt.offsetMin = Vector2.zero;
            gwRt.offsetMax = Vector2.zero;
            AddText(gamesWon, "Games: 2 - 1", 16, new Color(0.6f, 0.7f, 0.8f), FontStyles.Normal);

            // Overall result (center top)
            GameObject overallResult = CreateChild(vsSection.transform, "OverallResult");
            RectTransform orRt = overallResult.GetComponent<RectTransform>();
            orRt.anchorMin = new Vector2(0.3f, 0.5f);
            orRt.anchorMax = new Vector2(0.7f, 1);
            orRt.offsetMin = Vector2.zero;
            orRt.offsetMax = Vector2.zero;
            AddText(overallResult, "2 - 1", 32, WIN_GREEN, FontStyles.Bold);

            // Opponent name (right)
            GameObject opponentName = CreateChild(vsSection.transform, "OpponentName");
            RectTransform oppRt = opponentName.GetComponent<RectTransform>();
            oppRt.anchorMin = new Vector2(0.65f, 0.5f);
            oppRt.anchorMax = new Vector2(1, 1);
            oppRt.offsetMin = new Vector2(0, 5);
            oppRt.offsetMax = new Vector2(-15, -5);
            AddText(opponentName, "Opponent", 20, PURPLE_NEON, FontStyles.Bold);
        }

        private static void CreateMoneySection(Transform parent)
        {
            GameObject moneySection = CreateChild(parent, "MoneySection");
            SetupRectTransform(moneySection, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -640), new Vector2(400, 80));
            moneySection.SetActive(false); // Hidden by default (shown for cash)

            Image moneyBg = moneySection.AddComponent<Image>();
            moneyBg.color = new Color(0.1f, 0.08f, 0.02f, 0.8f);
            AddGlow(moneySection, GOLD, 3);

            // Money result
            GameObject moneyResult = CreateChild(moneySection.transform, "MoneyResult");
            RectTransform mrRt = moneyResult.GetComponent<RectTransform>();
            mrRt.anchorMin = new Vector2(0, 0.4f);
            mrRt.anchorMax = new Vector2(1, 1);
            mrRt.offsetMin = Vector2.zero;
            mrRt.offsetMax = Vector2.zero;
            AddText(moneyResult, "+$9.00", 36, GOLD, FontStyles.Bold);

            // Entry fee
            GameObject entryFee = CreateChild(moneySection.transform, "EntryFee");
            RectTransform efRt = entryFee.GetComponent<RectTransform>();
            efRt.anchorMin = new Vector2(0, 0);
            efRt.anchorMax = new Vector2(1, 0.45f);
            efRt.offsetMin = Vector2.zero;
            efRt.offsetMax = Vector2.zero;
            AddText(entryFee, "Entry: $5.00", 16, new Color(0.7f, 0.6f, 0.4f), FontStyles.Normal);
        }

        private static void CreateButtons(Transform parent)
        {
            GameObject buttonsContainer = CreateChild(parent, "ButtonsContainer");
            SetupRectTransform(buttonsContainer, new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 50), new Vector2(-40, 90));

            HorizontalLayoutGroup hlg = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 20;
            hlg.childForceExpandWidth = false;
            hlg.childControlWidth = false;

            // Play Again button (practice)
            CreateButton3D(buttonsContainer.transform, "PlayAgainButton", "PLAY AGAIN", BLUE_NEON, 240, 70);

            // Back button (practice)
            CreateButton3D(buttonsContainer.transform, "BackButton", "BACK", new Color(0.4f, 0.4f, 0.5f), 160, 70);

            // Continue button (online/cash)
            GameObject continueBtn = CreateButton3D(buttonsContainer.transform, "ContinueButton", "CONTINUE", WIN_GREEN, 200, 70);
            continueBtn.SetActive(false);

            // Rematch button (online/cash)
            GameObject rematchBtn = CreateButton3D(buttonsContainer.transform, "RematchButton", "REMATCH", CYAN_NEON, 200, 70);
            rematchBtn.SetActive(false);
        }

        private static void AssignReferences(SprintSummaryPanelController controller, GameObject panel, GameObject content)
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
                so.FindProperty("headerIcon").objectReferenceValue =
                    face.Find("HeaderIcon/Icon")?.GetComponent<Image>();

                // Games Table
                so.FindProperty("gameRowsContainer").objectReferenceValue =
                    face.Find("GamesTable/GameRowsContainer");
                so.FindProperty("gameRowPrefab").objectReferenceValue =
                    face.Find("GamesTable/GameRowsContainer/GameRowTemplate")?.gameObject;

                // Totals
                so.FindProperty("totalTimeText").objectReferenceValue =
                    face.Find("TotalsContainer/TotalTimeLabelContainer/TotalTimeValue")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("totalErrorsText").objectReferenceValue =
                    face.Find("TotalsContainer/TotalErrorsLabelContainer/TotalErrorsValue")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("totalScoreText").objectReferenceValue =
                    face.Find("TotalsContainer/TotalScoreLabelContainer/TotalScoreValue")?.GetComponent<TextMeshProUGUI>();

                // VS Section
                so.FindProperty("vsSection").objectReferenceValue =
                    face.Find("VSSection")?.gameObject;
                so.FindProperty("gamesWonText").objectReferenceValue =
                    face.Find("VSSection/GamesWonText")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("overallResultText").objectReferenceValue =
                    face.Find("VSSection/OverallResult")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("playerNameText").objectReferenceValue =
                    face.Find("VSSection/PlayerName")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("opponentNameText").objectReferenceValue =
                    face.Find("VSSection/OpponentName")?.GetComponent<TextMeshProUGUI>();

                // Money Section
                so.FindProperty("moneySection").objectReferenceValue =
                    face.Find("MoneySection")?.gameObject;
                so.FindProperty("moneyResultText").objectReferenceValue =
                    face.Find("MoneySection/MoneyResult")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("entryFeeText").objectReferenceValue =
                    face.Find("MoneySection/EntryFee")?.GetComponent<TextMeshProUGUI>();

                // Buttons
                so.FindProperty("playAgainButton").objectReferenceValue =
                    face.Find("ButtonsContainer/PlayAgainButton")?.GetComponent<Button>();
                so.FindProperty("backButton").objectReferenceValue =
                    face.Find("ButtonsContainer/BackButton")?.GetComponent<Button>();
                so.FindProperty("continueButton").objectReferenceValue =
                    face.Find("ButtonsContainer/ContinueButton")?.GetComponent<Button>();
                so.FindProperty("rematchButton").objectReferenceValue =
                    face.Find("ButtonsContainer/RematchButton")?.GetComponent<Button>();

                // Button texts
                var playAgainBtn = face.Find("ButtonsContainer/PlayAgainButton/Face/Text");
                if (playAgainBtn != null)
                    so.FindProperty("playAgainButtonText").objectReferenceValue = playAgainBtn.GetComponent<TextMeshProUGUI>();

                var backBtn = face.Find("ButtonsContainer/BackButton/Face/Text");
                if (backBtn != null)
                    so.FindProperty("backButtonText").objectReferenceValue = backBtn.GetComponent<TextMeshProUGUI>();

                var continueBtn = face.Find("ButtonsContainer/ContinueButton/Face/Text");
                if (continueBtn != null)
                    so.FindProperty("continueButtonText").objectReferenceValue = continueBtn.GetComponent<TextMeshProUGUI>();

                var rematchBtn = face.Find("ButtonsContainer/RematchButton/Face/Text");
                if (rematchBtn != null)
                    so.FindProperty("rematchButtonText").objectReferenceValue = rematchBtn.GetComponent<TextMeshProUGUI>();
            }

            so.ApplyModifiedProperties();
        }

        // ====================================================================
        // UTILITY METHODS (same pattern as WinPanelUIBuilder)
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
            // Shadow
            GameObject shadow = CreateChild(parent, "Shadow");
            RectTransform shadowRt = shadow.GetComponent<RectTransform>();
            shadowRt.anchorMin = Vector2.zero;
            shadowRt.anchorMax = Vector2.one;
            shadowRt.sizeDelta = Vector2.zero;
            shadowRt.anchoredPosition = new Vector2(6, -12);
            Image shadowImg = shadow.AddComponent<Image>();
            shadowImg.color = new Color(0, 0, 0, 0.5f);

            // Side
            GameObject side = CreateChild(parent, "Side");
            SetupRectTransform(side, new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, -6), new Vector2(0, 12));
            Image sideImg = side.AddComponent<Image>();
            sideImg.color = new Color(glowColor.r * 0.4f, glowColor.g * 0.4f, glowColor.b * 0.4f, 1f);

            // Face
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

            // Shadow
            GameObject shadow = CreateChild(btn.transform, "Shadow");
            SetupRectTransform(shadow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(4, -8), new Vector2(width, height));
            Image shadowImg = shadow.AddComponent<Image>();
            shadowImg.color = new Color(0, 0, 0, 0.4f);

            // Side
            GameObject side = CreateChild(btn.transform, "Side");
            SetupRectTransform(side, new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 0), new Vector2(width, 10));
            Image sideImg = side.AddComponent<Image>();
            sideImg.color = new Color(color.r * 0.5f, color.g * 0.5f, color.b * 0.5f, 1f);

            // Face
            GameObject face = CreateChild(btn.transform, "Face");
            SetupRectTransform(face, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 5), new Vector2(width, height - 10));
            Image faceImg = face.AddComponent<Image>();
            faceImg.color = color;

            // Text
            GameObject textObj = CreateChild(face.transform, "Text");
            SetupRectTransform(textObj, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-10, -6));
            float fontSize = Mathf.Min(height * 0.35f, FontSizes.Button);
            AddText(textObj, text, fontSize, new Color(0.02f, 0.05f, 0.1f), FontStyles.Bold);

            // Button component
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
