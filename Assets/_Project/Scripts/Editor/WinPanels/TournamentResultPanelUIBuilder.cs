using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using System.IO;
using DigitPark.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// Builder para crear prefabs TournamentResultWin y TournamentResultLose
    /// Estilo dorado/bronce neón (competitivo de torneo)
    /// </summary>
    public static class TournamentResultPanelUIBuilder
    {
        // Colores torneo
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f);
        private static readonly Color GOLD_DARK = new Color(0.7f, 0.55f, 0f);
        private static readonly Color GOLD_LIGHT = new Color(1f, 0.95f, 0.6f);
        private static readonly Color SILVER = new Color(0.75f, 0.75f, 0.75f);
        private static readonly Color BRONZE = new Color(0.8f, 0.5f, 0.2f);
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f);
        private static readonly Color WIN_GREEN = new Color(0.2f, 1f, 0.4f);
        private static readonly Color DARK_BG = new Color(0.02f, 0.04f, 0.08f, 0.95f);
        private static readonly Color PANEL_BG_WIN = new Color(0.1f, 0.08f, 0.02f, 0.95f);
        private static readonly Color PANEL_BG_LOSE = new Color(0.06f, 0.06f, 0.08f, 0.95f);
        private static readonly Color DISABLED_COLOR = new Color(0.4f, 0.4f, 0.4f);

        private const string PREFAB_PATH = "Assets/_Project/Resources/Prefabs/WinPanels";

        [MenuItem("DigitPark/Prefabs/Games/Tournament Result Win Panel", false, 201)]
        public static void BuildWinPanel()
        {
            BuildPanel(true);
        }

        [MenuItem("DigitPark/Prefabs/Games/Tournament Result Lose Panel", false, 202)]
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

            string panelName = isWin ? "TournamentResultWin" : "TournamentResultLose";
            GameObject panel = CreateTournamentResultPanel(isWin);

            string prefabPath = $"{PREFAB_PATH}/{panelName}.prefab";
            if (File.Exists(prefabPath))
                AssetDatabase.DeleteAsset(prefabPath);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(panel, prefabPath);
            Object.DestroyImmediate(panel);

            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);

            Debug.Log($"[TournamentResultPanelUIBuilder] Prefab creado: {prefabPath}");
            if (!AllScenesBatchBuilder.SilentMode)
                EditorUtility.DisplayDialog("Prefab Creado",
                    $"{panelName} prefab guardado en:\n{prefabPath}\n\nAsignalo al ResultPanelManager.",
                    "OK");
        }

        private static GameObject CreateTournamentResultPanel(bool isWin)
        {
            Color accentColor = isWin ? GOLD : SILVER;
            Color panelBg = isWin ? PANEL_BG_WIN : PANEL_BG_LOSE;

            // Root panel
            GameObject panel = new GameObject(isWin ? "TournamentResultWin" : "TournamentResultLose");
            RectTransform panelRt = panel.AddComponent<RectTransform>();
            SetFullStretch(panelRt);

            Image overlay = panel.AddComponent<Image>();
            overlay.color = DARK_BG;

            CanvasGroup cg = panel.AddComponent<CanvasGroup>();

            // Content (650x750)
            GameObject content = CreateChild(panel.transform, "Content");
            SetupRectTransform(content, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(650, 750));

            CreatePanel3DEffect(content.transform, panelBg, accentColor);
            GameObject face = content.transform.Find("Face").gameObject;

            // === HEADER (trofeo + título) ===
            CreateHeader(face.transform, isWin, accentColor);

            // === STATS (tiempo/errores/score) ===
            CreateStats(face.transform, accentColor);

            // === TOURNAMENT INFO (posición, intentos, mejor tiempo) ===
            CreateTournamentInfo(face.transform, accentColor);

            // === PRIZE SECTION (cash tournaments) ===
            CreatePrizeSection(face.transform);

            // === BUTTONS (3) ===
            CreateButtons(face.transform, accentColor, isWin);

            // Particles for win
            if (isWin)
            {
                GameObject particles = CreateChild(panel.transform, "ParticleEffects");
                SetFullStretch(particles.GetComponent<RectTransform>());
                particles.AddComponent<UISparkleEffect>();
            }

            // Controller
            TournamentResultPanelController controller = panel.AddComponent<TournamentResultPanelController>();
            AssignReferences(controller, panel, content, isWin);

            return panel;
        }

        private static void CreateHeader(Transform parent, bool isWin, Color accentColor)
        {
            // Trophy icon
            GameObject trophyContainer = CreateChild(parent, "TrophyContainer");
            SetupRectTransform(trophyContainer, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -55), new Vector2(100, 100));
            Image trophyBg = trophyContainer.AddComponent<Image>();
            trophyBg.color = new Color(accentColor.r * 0.2f, accentColor.g * 0.2f, accentColor.b * 0.2f, 0.9f);
            AddGlow(trophyContainer, accentColor, 4);

            GameObject trophyIcon = CreateChild(trophyContainer.transform, "TrophyIcon");
            SetupRectTransform(trophyIcon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(70, 70));
            Image trophyImg = trophyIcon.AddComponent<Image>();
            trophyImg.color = accentColor;
            trophyImg.preserveAspect = true;

            // Title
            GameObject title = CreateChild(parent, "Title");
            SetupRectTransform(title, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -150), new Vector2(0, 60));
            string titleStr = isWin ? "TOURNAMENT RESULT" : "TOURNAMENT RESULT";
            TextMeshProUGUI titleTmp = AddText(title, titleStr, 38, accentColor, FontStyles.Bold);
            AddGlow(title, accentColor, 3);

            // Subtitle (position)
            GameObject subtitle = CreateChild(parent, "TournamentResultSubtitle");
            SetupRectTransform(subtitle, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -195), new Vector2(0, 35));
            AddText(subtitle, "Position #1", 24, new Color(0.7f, 0.7f, 0.7f), FontStyles.Bold);
        }

        private static void CreateStats(Transform parent, Color accentColor)
        {
            GameObject statsContainer = CreateChild(parent, "StatsContainer");
            SetupRectTransform(statsContainer, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -280), new Vector2(550, 100));

            Image statsBg = statsContainer.AddComponent<Image>();
            statsBg.color = new Color(0.05f, 0.05f, 0.08f, 0.7f);

            // Time
            CreateStatItem(statsContainer.transform, "TimeLabel", "TimeValue",
                "TIME", "12.45s", accentColor, 0f, 0.33f);

            // Errors
            CreateStatItem(statsContainer.transform, "ErrorsLabel", "ErrorsValue",
                "ERRORS", "2", new Color(0.8f, 0.6f, 0.4f), 0.33f, 0.66f);

            // Score
            CreateStatItem(statsContainer.transform, "ScoreLabel", "ScoreValue",
                "SCORE", "16.45s", CYAN_NEON, 0.66f, 1f);
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

            GameObject label = CreateChild(container.transform, labelName);
            RectTransform labelRt = label.GetComponent<RectTransform>();
            labelRt.anchorMin = new Vector2(0, 0.55f);
            labelRt.anchorMax = new Vector2(1, 1);
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;
            AddText(label, labelText, 14, new Color(0.5f, 0.5f, 0.6f), FontStyles.Bold);

            GameObject value = CreateChild(container.transform, valueName);
            RectTransform valueRt = value.GetComponent<RectTransform>();
            valueRt.anchorMin = new Vector2(0, 0);
            valueRt.anchorMax = new Vector2(1, 0.6f);
            valueRt.offsetMin = Vector2.zero;
            valueRt.offsetMax = Vector2.zero;
            AddText(value, valueText, 28, valueColor, FontStyles.Bold);
        }

        private static void CreateTournamentInfo(Transform parent, Color accentColor)
        {
            GameObject infoContainer = CreateChild(parent, "TournamentInfoContainer");
            SetupRectTransform(infoContainer, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -420), new Vector2(550, 130));

            Image infoBg = infoContainer.AddComponent<Image>();
            infoBg.color = new Color(0.04f, 0.04f, 0.06f, 0.7f);

            Outline border = infoContainer.AddComponent<Outline>();
            border.effectColor = new Color(accentColor.r, accentColor.g, accentColor.b, 0.4f);
            border.effectDistance = new Vector2(2, -2);

            // Position (big number)
            GameObject positionContainer = CreateChild(infoContainer.transform, "PositionContainer");
            RectTransform posRt = positionContainer.GetComponent<RectTransform>();
            posRt.anchorMin = new Vector2(0, 0);
            posRt.anchorMax = new Vector2(0.3f, 1);
            posRt.offsetMin = new Vector2(10, 10);
            posRt.offsetMax = new Vector2(-5, -10);

            GameObject positionText = CreateChild(positionContainer.transform, "PositionText");
            RectTransform posTxtRt = positionText.GetComponent<RectTransform>();
            posTxtRt.anchorMin = new Vector2(0, 0.3f);
            posTxtRt.anchorMax = new Vector2(1, 1);
            posTxtRt.offsetMin = Vector2.zero;
            posTxtRt.offsetMax = Vector2.zero;
            AddText(positionText, "#1", 48, accentColor, FontStyles.Bold);

            GameObject posLabel = CreateChild(positionContainer.transform, "PositionLabel");
            RectTransform posLblRt = posLabel.GetComponent<RectTransform>();
            posLblRt.anchorMin = new Vector2(0, 0);
            posLblRt.anchorMax = new Vector2(1, 0.35f);
            posLblRt.offsetMin = Vector2.zero;
            posLblRt.offsetMax = Vector2.zero;
            AddText(posLabel, "Position", 14, new Color(0.5f, 0.5f, 0.6f), FontStyles.Bold);

            // Attempts
            GameObject attemptsContainer = CreateChild(infoContainer.transform, "AttemptsContainer");
            RectTransform attRt = attemptsContainer.GetComponent<RectTransform>();
            attRt.anchorMin = new Vector2(0.3f, 0);
            attRt.anchorMax = new Vector2(0.65f, 1);
            attRt.offsetMin = new Vector2(5, 10);
            attRt.offsetMax = new Vector2(-5, -10);

            GameObject attemptsText = CreateChild(attemptsContainer.transform, "AttemptsText");
            SetFullStretch(attemptsText.GetComponent<RectTransform>());
            TextMeshProUGUI attTmp = AddText(attemptsText, "2 of 3 attempts", 18, CYAN_NEON, FontStyles.Bold);
            attTmp.enableWordWrapping = true;

            // Best time
            GameObject bestTimeContainer = CreateChild(infoContainer.transform, "BestTimeContainer");
            RectTransform btRt = bestTimeContainer.GetComponent<RectTransform>();
            btRt.anchorMin = new Vector2(0.65f, 0);
            btRt.anchorMax = new Vector2(1, 1);
            btRt.offsetMin = new Vector2(5, 10);
            btRt.offsetMax = new Vector2(-10, -10);

            GameObject bestTimeText = CreateChild(bestTimeContainer.transform, "BestTimeText");
            SetFullStretch(bestTimeText.GetComponent<RectTransform>());
            TextMeshProUGUI btTmp = AddText(bestTimeText, "Best: 11.20s", 18, WIN_GREEN, FontStyles.Bold);
            btTmp.enableWordWrapping = true;
        }

        private static void CreatePrizeSection(Transform parent)
        {
            GameObject prizeSection = CreateChild(parent, "PrizeSection");
            SetupRectTransform(prizeSection, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -575), new Vector2(350, 70));
            prizeSection.SetActive(false); // Hidden for free tournaments

            Image prizeBg = prizeSection.AddComponent<Image>();
            prizeBg.color = new Color(0.12f, 0.1f, 0.02f, 0.8f);
            AddGlow(prizeSection, GOLD, 3);

            // Prize label
            GameObject prizeLabel = CreateChild(prizeSection.transform, "PrizeLabel");
            RectTransform plRt = prizeLabel.GetComponent<RectTransform>();
            plRt.anchorMin = new Vector2(0, 0.55f);
            plRt.anchorMax = new Vector2(1, 1);
            plRt.offsetMin = Vector2.zero;
            plRt.offsetMax = Vector2.zero;
            AddText(prizeLabel, "POTENTIAL PRIZE", 14, new Color(0.7f, 0.6f, 0.4f), FontStyles.Bold);

            // Prize value
            GameObject prizeValue = CreateChild(prizeSection.transform, "PrizeText");
            RectTransform pvRt = prizeValue.GetComponent<RectTransform>();
            pvRt.anchorMin = new Vector2(0, 0);
            pvRt.anchorMax = new Vector2(1, 0.6f);
            pvRt.offsetMin = Vector2.zero;
            pvRt.offsetMax = Vector2.zero;
            AddText(prizeValue, "$25.00", 32, GOLD_LIGHT, FontStyles.Bold);
        }

        private static void CreateButtons(Transform parent, Color accentColor, bool isWin)
        {
            GameObject buttonsContainer = CreateChild(parent, "ButtonsContainer");
            SetupRectTransform(buttonsContainer, new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 55), new Vector2(-40, 110));

            VerticalLayoutGroup vlg = buttonsContainer.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.spacing = 12;
            vlg.childForceExpandWidth = false;
            vlg.childControlWidth = false;
            vlg.childForceExpandHeight = false;
            vlg.childControlHeight = false;

            // Top row (retry + leaderboard)
            GameObject topRow = CreateChild(buttonsContainer.transform, "TopRow");
            LayoutElement topLE = topRow.AddComponent<LayoutElement>();
            topLE.preferredHeight = 55;
            topLE.preferredWidth = 550;

            HorizontalLayoutGroup topHlg = topRow.AddComponent<HorizontalLayoutGroup>();
            topHlg.childAlignment = TextAnchor.MiddleCenter;
            topHlg.spacing = 20;
            topHlg.childForceExpandWidth = false;
            topHlg.childControlWidth = false;

            // Retry button
            CreateButton3D(topRow.transform, "RetryButton", "RETRY", accentColor, 240, 55);

            // Leaderboard button
            CreateButton3D(topRow.transform, "LeaderboardButton", "LEADERBOARD", CYAN_NEON, 240, 55);

            // Exit button (bottom, full width)
            CreateButton3D(buttonsContainer.transform, "ExitButton", "EXIT TOURNAMENT",
                new Color(0.4f, 0.35f, 0.3f), 350, 50);
        }

        private static void AssignReferences(TournamentResultPanelController controller,
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
                so.FindProperty("trophyIcon").objectReferenceValue =
                    face.Find("TrophyContainer/TrophyIcon")?.GetComponent<Image>();

                // Stats
                so.FindProperty("timeText").objectReferenceValue =
                    face.Find("StatsContainer/TimeLabelContainer/TimeValue")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("errorsText").objectReferenceValue =
                    face.Find("StatsContainer/ErrorsLabelContainer/ErrorsValue")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("scoreText").objectReferenceValue =
                    face.Find("StatsContainer/ScoreLabelContainer/ScoreValue")?.GetComponent<TextMeshProUGUI>();

                // Tournament Info
                so.FindProperty("positionText").objectReferenceValue =
                    face.Find("TournamentInfoContainer/PositionContainer/PositionText")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("positionLabel").objectReferenceValue =
                    face.Find("TournamentInfoContainer/PositionContainer/PositionLabel")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("attemptsText").objectReferenceValue =
                    face.Find("TournamentInfoContainer/AttemptsContainer/AttemptsText")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("bestTimeText").objectReferenceValue =
                    face.Find("TournamentInfoContainer/BestTimeContainer/BestTimeText")?.GetComponent<TextMeshProUGUI>();

                // Prize
                so.FindProperty("prizeSection").objectReferenceValue =
                    face.Find("PrizeSection")?.gameObject;
                so.FindProperty("prizeText").objectReferenceValue =
                    face.Find("PrizeSection/PrizeText")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("prizeLabel").objectReferenceValue =
                    face.Find("PrizeSection/PrizeLabel")?.GetComponent<TextMeshProUGUI>();

                // Buttons
                so.FindProperty("retryButton").objectReferenceValue =
                    face.Find("ButtonsContainer/TopRow/RetryButton")?.GetComponent<Button>();
                so.FindProperty("leaderboardButton").objectReferenceValue =
                    face.Find("ButtonsContainer/TopRow/LeaderboardButton")?.GetComponent<Button>();
                so.FindProperty("exitButton").objectReferenceValue =
                    face.Find("ButtonsContainer/ExitButton")?.GetComponent<Button>();

                // Button texts
                var retryBtn = face.Find("ButtonsContainer/TopRow/RetryButton/Face/Text");
                if (retryBtn != null)
                    so.FindProperty("retryButtonText").objectReferenceValue = retryBtn.GetComponent<TextMeshProUGUI>();
                var lbBtn = face.Find("ButtonsContainer/TopRow/LeaderboardButton/Face/Text");
                if (lbBtn != null)
                    so.FindProperty("leaderboardButtonText").objectReferenceValue = lbBtn.GetComponent<TextMeshProUGUI>();
                var exitBtn = face.Find("ButtonsContainer/ExitButton/Face/Text");
                if (exitBtn != null)
                    so.FindProperty("exitButtonText").objectReferenceValue = exitBtn.GetComponent<TextMeshProUGUI>();
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
            tmp.fontSizeMin = Mathf.Min(FontSizes.AutoMinBody, size);
            tmp.fontSizeMax = Mathf.Max(FontSizes.AutoMinBody, size);
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
                new Vector2(0, 0), new Vector2(width, 8));
            Image sideImg = side.AddComponent<Image>();
            sideImg.color = new Color(color.r * 0.5f, color.g * 0.5f, color.b * 0.5f, 1f);

            GameObject face = CreateChild(btn.transform, "Face");
            SetupRectTransform(face, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 4), new Vector2(width, height - 8));
            Image faceImg = face.AddComponent<Image>();
            faceImg.color = color;

            GameObject textObj = CreateChild(face.transform, "Text");
            SetupRectTransform(textObj, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-10, -6));
            float fontSize = Mathf.Min(height * 0.38f, FontSizes.Body);
            AddText(textObj, text, fontSize, new Color(0.03f, 0.02f, 0.01f), FontStyles.Bold);

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
