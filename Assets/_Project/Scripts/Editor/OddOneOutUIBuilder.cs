using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;
using DigitPark.Games;
using DigitPark.Editor.AutoAssigners;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para reconstruir la UI de OddOneOut con diseño profesional neón
    /// Dos grids 4x4 lado a lado - Celdas de 100px optimizadas para móvil
    /// Settings panel, countdown, feedback, WinPanelController
    /// </summary>
    public class OddOneOutUIBuilder : EditorWindow
    {
        // Colores del tema neón
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color MAGENTA_NEON = new Color(1f, 0f, 0.8f, 1f);
        private static readonly Color GREEN_NEON = new Color(0.3f, 1f, 0.5f, 1f);
        private static readonly Color ORANGE_NEON = new Color(1f, 0.6f, 0.2f, 1f);
        private static readonly Color DARK_BG = new Color(0.02f, 0.04f, 0.08f, 1f);
        private static readonly Color PANEL_BG = new Color(0.05f, 0.1f, 0.15f, 0.95f);
        private static readonly Color BUTTON_BG = new Color(0.08f, 0.12f, 0.18f, 1f);
        private static readonly Color ERROR_COLOR = new Color(1f, 0.3f, 0.3f, 1f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);

        // Stats Bar icon paths
        private const string TIMER_ICON_PATH = "Assets/_Project/Art/Icons/UI/TimerIcon.png";
        private const string ROUND_ICON_PATH = "Assets/_Project/Art/Icons/UI/RoundIcon.png";
        private const string ERROR_ICON_PATH = "Assets/_Project/Art/Icons/UI/ErrorIcon.png";

        // Tamaño de celda optimizado
        private const float CELL_SIZE = 100f;
        private const float CELL_SPACING = 4f;
        private const int GRID_COLUMNS = 4;
        private const float GRID_GAP = 15f;

        [MenuItem("DigitPark/UI Builders/Games/OddOneOut", false, 113)]
        public static void ShowWindow()
        {
            GetWindow<OddOneOutUIBuilder>("OddOneOut UI Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("OddOneOut UI Builder", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "OddOneOut con:\n" +
                "- Settings panel (rondas)\n" +
                "- Countdown 3-2-1-GO!\n" +
                "- Feedback correcto/incorrecto\n" +
                "- Win/Lose panels (WinPanelController)",
                MessageType.Info);

            GUILayout.Space(10);

            if (GUILayout.Button("Reconstruir OddOneOut UI", GUILayout.Height(40)))
            {
                RebuildOddOneOutUI();
            }

            GUILayout.Space(15);
            GUI.backgroundColor = new Color(0.5f, 0.8f, 1f);
            if (GUILayout.Button("Auto-Asignar Referencias", GUILayout.Height(30)))
            {
                OddOneOutReferenceAssigner.RunAutoAssign();
            }
            GUI.backgroundColor = Color.white;
        }

        private static void RebuildOddOneOutUI()
        {
            CleanupOldUI();
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("[OddOneOutUIBuilder] No se encontró Canvas en la escena");
                return;
            }

            Transform canvasTransform = canvas.transform;

            CleanOldElements(canvasTransform);
            CreateOddOneOutLayout(canvasTransform);
            AssignControllerReferences();

            Debug.Log("[OddOneOutUIBuilder] OddOneOut UI reconstruida exitosamente!");
            EditorUtility.SetDirty(canvas.gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

        private static void CleanupOldUI()
        {
            string[] toClean = { "Background", "SafeArea" };
            foreach (var canvas in Object.FindObjectsOfType<Canvas>(true))
            {
                if (canvas.transform.parent != null) continue;
                // No tocar TransitionCanvas ni EffectsCanvas
                if (canvas.gameObject.name.Contains("Transition") ||
                    canvas.gameObject.name.Contains("Effects")) continue;
                foreach (string name in toClean)
                {
                    Transform t = canvas.transform.Find(name);
                    if (t != null) Object.DestroyImmediate(t.gameObject);
                }
            }
        }

        private static void CleanOldElements(Transform canvasTransform)
        {
            string[] keepElements = {
                "Main Camera", "EventSystem",
                "Directional Light", "SceneTransition",
                "Background"
            };

            for (int i = canvasTransform.childCount - 1; i >= 0; i--)
            {
                Transform child = canvasTransform.GetChild(i);
                bool shouldKeep = false;

                foreach (string keep in keepElements)
                {
                    if (child.name.Contains(keep) || child.name == keep)
                    {
                        shouldKeep = true;
                        break;
                    }
                }

                if (!shouldKeep && (child.GetComponent<Animator>() != null || child.GetComponent<Animation>() != null))
                    shouldKeep = true;

                if (!shouldKeep)
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        private static void CreateOddOneOutLayout(Transform canvasTransform)
        {
            // ========== BACKGROUND ==========
            GameObject background = CreateElement(canvasTransform, "Background");
            SetupRectTransform(background, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = DARK_BG;
            background.transform.SetAsFirstSibling();

            // ========== SAFE AREA ==========
            GameObject safeArea = CreateElement(canvasTransform, "SafeArea");
            SetupRectTransform(safeArea, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SafeAreaHandler safeHandler = safeArea.AddComponent<SafeAreaHandler>();

            // ========== HEADER ==========
            CreateHeader(safeArea.transform);

            // ========== STATS BAR ==========
            CreateStatsBar(safeArea.transform);

            // ========== COMBO TEXT ==========
            CreateComboText(safeArea.transform);

            // ========== GRIDS CONTAINER ==========
            CreateGridsContainer(safeArea.transform);

            // ========== FEEDBACK PANEL ==========
            CreateFeedbackPanel(safeArea.transform);

            // ========== BARRA DE PROGRESO ==========
            CreateProgressBar(safeArea.transform);

            // ========== NORMAL WIN/LOSE PANELS ==========
            CreateNormalWinPanel(safeArea.transform);
            CreateNormalLosePanel(safeArea.transform);

            // ========== REAL MONEY PANELS (Cash Battle) ==========
            WinPanelInlineBuilder.CreateRealMoneyPanels(safeArea.transform);

            // ========== PARTICLE EFFECTS ==========
            CreateParticleEffects(safeArea.transform);

            // ========== COUNTDOWN PANEL (on top of game) ==========
            CreateCountdownPanel(safeArea.transform);

            // ========== SETTINGS PANEL (last so it renders on top) ==========
            CreateSettingsPanel(safeArea.transform);
        }

        private static void CreateHeader(Transform parent)
        {
            GameObject header = CreateElement(parent, "Header");
            SetupRectTransform(header,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -50), new Vector2(0, 100));

            Image headerBg = header.AddComponent<Image>();
            headerBg.color = new Color(0f, 0f, 0f, 0.3f);

            GameObject title = CreateElement(header.transform, "TitleText");
            SetupRectTransform(title,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 0), new Vector2(500, 50));
            SetupText(title, "ODD ONE OUT", (int)FontSizes.SceneTitle, CYAN_NEON, FontStyles.Bold);
        }

        private static void CreateStatsBar(Transform parent)
        {
            GameObject statsBar = CreateElement(parent, "StatsBar");
            SetupRectTransform(statsBar,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -160), new Vector2(1020, 105));

            Image statsBg = statsBar.AddComponent<Image>();
            statsBg.color = PANEL_BG;

            Outline statsOutline = statsBar.AddComponent<Outline>();
            statsOutline.effectColor = CYAN_NEON;
            statsOutline.effectDistance = new Vector2(2, -2);

            HorizontalLayoutGroup layout = statsBar.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 90;
            layout.padding = new RectOffset(60, 60, 15, 15);
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            // Load icon sprites
            Sprite timerIcon = AssetDatabase.LoadAssetAtPath<Sprite>(TIMER_ICON_PATH);
            Sprite roundIcon = AssetDatabase.LoadAssetAtPath<Sprite>(ROUND_ICON_PATH);
            Sprite errorIcon = AssetDatabase.LoadAssetAtPath<Sprite>(ERROR_ICON_PATH);

            // Timer
            CreateStatItem(statsBar.transform, "TimerContainer", "TimerIcon", "TimerText",
                "00:00", Color.white, 240, timerIcon, (int)FontSizes.BodyLarge);

            // Round
            CreateStatItem(statsBar.transform, "RoundContainer", "RoundIcon", "RoundText",
                "1/5", CYAN_NEON, 180, roundIcon, (int)FontSizes.Button);

            // Errors
            CreateStatItem(statsBar.transform, "ErrorsContainer", "ErrorsIcon", "ErrorsText",
                "0", ERROR_COLOR, 120, errorIcon, (int)FontSizes.BodyLarge);
        }

        private static void CreateStatItem(Transform parent, string containerName, string iconName,
            string textName, string defaultText, Color color, float width, Sprite iconSprite = null, int fontSize = (int)FontSizes.Body)
        {
            GameObject container = CreateElement(parent, containerName);

            LayoutElement le = container.AddComponent<LayoutElement>();
            le.preferredWidth = width;
            le.preferredHeight = 75;

            // Icon (colored square indicator)
            GameObject icon = CreateElement(container.transform, iconName);
            SetupRectTransform(icon,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(22, 0), new Vector2(54, 54));
            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = color;
            if (iconSprite != null)
            {
                iconImg.sprite = iconSprite;
                iconImg.preserveAspect = true;
            }

            // Text
            GameObject text = CreateElement(container.transform, textName);
            SetupRectTransform(text,
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(45, 0), new Vector2(-10, 0));
            TextMeshProUGUI tmp = SetupText(text, defaultText, fontSize, color, FontStyles.Bold);
            tmp.alignment = TextAlignmentOptions.Left;
        }

        private static void CreateComboText(Transform parent)
        {
            GameObject comboContainer = CreateElement(parent, "ComboContainer");
            SetupRectTransform(comboContainer,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -230), new Vector2(180, 45));

            Image comboBg = comboContainer.AddComponent<Image>();
            comboBg.color = new Color(0.1f, 0.08f, 0.15f, 0.8f);

            Outline comboOutline = comboContainer.AddComponent<Outline>();
            comboOutline.effectColor = GOLD;
            comboOutline.effectDistance = new Vector2(1, -1);

            CanvasGroup comboCg = comboContainer.AddComponent<CanvasGroup>();
            comboCg.alpha = 0;

            GameObject comboText = CreateElement(comboContainer.transform, "ComboText");
            SetupRectTransform(comboText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetupText(comboText, "x2", (int)FontSizes.Body, GOLD, FontStyles.Bold);

            comboContainer.SetActive(false);
        }

        private static void CreateGridsContainer(Transform parent)
        {
            float gridSize = (CELL_SIZE * GRID_COLUMNS) + (CELL_SPACING * (GRID_COLUMNS - 1)) + 20;

            GameObject gridsContainer = CreateElement(parent, "GridsContainer");
            SetupRectTransform(gridsContainer,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 30), new Vector2(gridSize * 2 + GRID_GAP, gridSize + 10));

            // ========== LEFT GRID ==========
            GameObject leftGrid = CreateElement(gridsContainer.transform, "LeftGrid");
            SetupRectTransform(leftGrid,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(gridSize / 2, 0), new Vector2(gridSize, gridSize));

            Image leftGridBg = leftGrid.AddComponent<Image>();
            leftGridBg.color = new Color(0.03f, 0.06f, 0.12f, 0.8f);

            Outline leftOutline = leftGrid.AddComponent<Outline>();
            leftOutline.effectColor = CYAN_NEON;
            leftOutline.effectDistance = new Vector2(2.5f, -2.5f);

            GridGlowPulse leftGlow = leftGrid.AddComponent<GridGlowPulse>();

            GridLayoutGroup leftGridLayout = leftGrid.AddComponent<GridLayoutGroup>();
            leftGridLayout.cellSize = new Vector2(CELL_SIZE, CELL_SIZE);
            leftGridLayout.spacing = new Vector2(CELL_SPACING, CELL_SPACING);
            leftGridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            leftGridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            leftGridLayout.childAlignment = TextAnchor.MiddleCenter;
            leftGridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            leftGridLayout.constraintCount = GRID_COLUMNS;
            leftGridLayout.padding = new RectOffset(10, 10, 10, 10);

            for (int i = 0; i < 16; i++)
            {
                CreateGridCell(leftGrid.transform, i, false);
            }

            // ========== RIGHT GRID ==========
            GameObject rightGrid = CreateElement(gridsContainer.transform, "RightGrid");
            SetupRectTransform(rightGrid,
                new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(-gridSize / 2, 0), new Vector2(gridSize, gridSize));

            Image rightGridBg = rightGrid.AddComponent<Image>();
            rightGridBg.color = new Color(0.03f, 0.06f, 0.12f, 0.8f);

            Outline rightOutline = rightGrid.AddComponent<Outline>();
            rightOutline.effectColor = MAGENTA_NEON;
            rightOutline.effectDistance = new Vector2(2.5f, -2.5f);

            GridGlowPulse rightGlow = rightGrid.AddComponent<GridGlowPulse>();

            GridLayoutGroup rightGridLayout = rightGrid.AddComponent<GridLayoutGroup>();
            rightGridLayout.cellSize = new Vector2(CELL_SIZE, CELL_SIZE);
            rightGridLayout.spacing = new Vector2(CELL_SPACING, CELL_SPACING);
            rightGridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            rightGridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            rightGridLayout.childAlignment = TextAnchor.MiddleCenter;
            rightGridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            rightGridLayout.constraintCount = GRID_COLUMNS;
            rightGridLayout.padding = new RectOffset(10, 10, 10, 10);

            for (int i = 0; i < 16; i++)
            {
                CreateGridCell(rightGrid.transform, i, true);
            }
        }

        private static void CreateGridCell(Transform parent, int index, bool isRight)
        {
            string prefix = isRight ? "RightButton" : "LeftButton";
            Color borderColor = isRight ? MAGENTA_NEON : CYAN_NEON;

            GameObject cell = CreateElement(parent, $"{prefix}_{index}");

            Image cellBase = cell.AddComponent<Image>();
            cellBase.color = Color.clear;

            // Shadow
            GameObject shadow = CreateElement(cell.transform, "Shadow");
            SetupRectTransform(shadow,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(2, -7), new Vector2(CELL_SIZE - 6, CELL_SIZE - 6));
            Image shadowImg = shadow.AddComponent<Image>();
            shadowImg.color = new Color(0f, 0f, 0f, 0.4f);

            // Side (depth)
            GameObject side = CreateElement(cell.transform, "Side");
            SetupRectTransform(side,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 0), new Vector2(CELL_SIZE - 6, 7));
            Image sideImg = side.AddComponent<Image>();
            sideImg.color = new Color(0.04f, 0.06f, 0.1f, 1f);

            // Face (top)
            GameObject face = CreateElement(cell.transform, "Face");
            SetupRectTransform(face,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 3), new Vector2(CELL_SIZE - 6, CELL_SIZE - 6));
            Image faceImg = face.AddComponent<Image>();
            faceImg.color = BUTTON_BG;

            Outline faceOutline = face.AddComponent<Outline>();
            faceOutline.effectColor = borderColor;
            faceOutline.effectDistance = new Vector2(2.5f, -2.5f);

            // Text
            string textName = isRight ? $"RightButtonText_{index}" : $"LeftButtonText_{index}";
            GameObject textObj = CreateElement(face.transform, textName);
            SetupRectTransform(textObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI tmp = SetupText(textObj, "A", (int)FontSizes.ValueLarge, Color.white, FontStyles.Bold);
            tmp.alignment = TextAlignmentOptions.Center;

            Outline textOutline = textObj.AddComponent<Outline>();
            textOutline.effectColor = new Color(borderColor.r * 0.5f, borderColor.g * 0.5f, borderColor.b * 0.5f, 0.8f);
            textOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // Button component
            Button button = cell.AddComponent<Button>();
            button.targetGraphic = faceImg;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.2f, 1f);
            colors.pressedColor = new Color(0.8f, 0.9f, 0.85f, 1f);
            colors.fadeDuration = 0.05f;
            button.colors = colors;

            // Add OddOneOutCell3D component
            OddOneOutCell3D cell3D = cell.AddComponent<OddOneOutCell3D>();

            SerializedObject so = new SerializedObject(cell3D);
            so.FindProperty("buttonFace").objectReferenceValue = face.GetComponent<RectTransform>();
            so.FindProperty("shadowImage").objectReferenceValue = shadowImg;
            so.FindProperty("sideImage").objectReferenceValue = sideImg;
            so.FindProperty("faceImage").objectReferenceValue = faceImg;
            so.FindProperty("glowOutline").objectReferenceValue = faceOutline;
            so.FindProperty("numberText").objectReferenceValue = tmp;
            so.FindProperty("isRightGrid").boolValue = isRight;
            so.FindProperty("borderColor").colorValue = borderColor;
            so.ApplyModifiedProperties();
        }

        private static void CreateFeedbackPanel(Transform parent)
        {
            GameObject feedbackPanel = CreateElement(parent, "FeedbackPanel");
            SetupRectTransform(feedbackPanel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -320), new Vector2(500, 60));

            Image bg = feedbackPanel.AddComponent<Image>();
            bg.color = PANEL_BG;

            Outline panelOutline = feedbackPanel.AddComponent<Outline>();
            panelOutline.effectColor = CYAN_NEON;
            panelOutline.effectDistance = new Vector2(2, -2);

            CanvasGroup cg = feedbackPanel.AddComponent<CanvasGroup>();
            cg.alpha = 0;

            GameObject feedbackText = CreateElement(feedbackPanel.transform, "FeedbackText");
            SetupRectTransform(feedbackText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI feedbackTmp = SetupText(feedbackText, "", (int)FontSizes.Body, GREEN_NEON, FontStyles.Bold);
            feedbackTmp.alignment = TextAlignmentOptions.Center;
            feedbackTmp.enableWordWrapping = false;

            feedbackPanel.SetActive(false);
        }

        private static void CreateNormalWinPanel(Transform parent)
        {
            GameObject panel = CreateElement(parent, "WinPanel_Normal");
            SetupRectTransform(panel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Image overlay = panel.AddComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0.85f);
            overlay.raycastTarget = true;

            CanvasGroup cg = panel.AddComponent<CanvasGroup>();
            cg.alpha = 0;

            // Content card
            GameObject content = CreateElement(panel.transform, "Content");
            SetupRectTransform(content,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(700, 560));

            Image contentBg = content.AddComponent<Image>();
            contentBg.color = new Color(0.04f, 0.08f, 0.14f, 0.98f);

            Outline contentOutline = content.AddComponent<Outline>();
            contentOutline.effectColor = GREEN_NEON;
            contentOutline.effectDistance = new Vector2(3, -3);

            // Title
            GameObject titleObj = CreateElement(content.transform, "TitleText");
            SetupRectTransform(titleObj,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -40), new Vector2(0, 55));
            TextMeshProUGUI titleTmp = SetupText(titleObj, "COMPLETED!", (int)FontSizes.ValueLarge, GREEN_NEON, FontStyles.Bold);

            // Divider after title
            CreateDivider(content.transform, -80);

            // Stats rows
            float statsY = -105f;
            float rowH = 42f;

            TextMeshProUGUI timeTmp = CreateStatRow(content.transform, "Time", "TimeText", "0:00.00", CYAN_NEON, statsY);
            statsY -= rowH;
            TextMeshProUGUI errorsTmp = CreateStatRow(content.transform, "Errors", "PanelErrorsText", "0", ERROR_COLOR, statsY);
            statsY -= rowH;
            CreateStatRow(content.transform, "Max Combo", "MaxComboValue", "0", ORANGE_NEON, statsY);
            statsY -= rowH;
            CreateStatRow(content.transform, "Penalty", "PenaltyValue", "-", ERROR_COLOR, statsY);

            // Divider before buttons
            CreateDivider(content.transform, statsY - 18);

            // Buttons
            GameObject buttonsContainer = CreateElement(content.transform, "ButtonsContainer");
            SetupRectTransform(buttonsContainer,
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 50), new Vector2(-40, 80));

            HorizontalLayoutGroup btnLayout = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
            btnLayout.childAlignment = TextAnchor.MiddleCenter;
            btnLayout.spacing = 30;
            btnLayout.childForceExpandWidth = false;
            btnLayout.childControlWidth = false;

            GameObject acceptBtnObj = CreatePanelButton(buttonsContainer.transform, "AcceptButton", "EXIT", new Color(0.5f, 0.5f, 0.5f), 200, 65);
            GameObject playAgainBtnObj = CreatePanelButton(buttonsContainer.transform, "PlayAgainButton", "PLAY AGAIN", CYAN_NEON, 260, 65);

            // WinPanelController
            WinPanelController wpc = panel.AddComponent<WinPanelController>();
            SerializedObject wpcSo = new SerializedObject(wpc);
            wpcSo.FindProperty("isRealMoneyPanel").boolValue = false;
            wpcSo.FindProperty("canvasGroup").objectReferenceValue = cg;
            wpcSo.FindProperty("content").objectReferenceValue = content;
            wpcSo.FindProperty("titleText").objectReferenceValue = titleTmp;
            wpcSo.FindProperty("timeText").objectReferenceValue = timeTmp;
            wpcSo.FindProperty("errorsText").objectReferenceValue = errorsTmp;
            wpcSo.FindProperty("acceptButton").objectReferenceValue = acceptBtnObj.GetComponent<Button>();
            wpcSo.FindProperty("playAgainButton").objectReferenceValue = playAgainBtnObj.GetComponent<Button>();
            wpcSo.ApplyModifiedProperties();

            panel.SetActive(false);
        }

        private static void CreateNormalLosePanel(Transform parent)
        {
            GameObject panel = CreateElement(parent, "LosePanel_Normal");
            SetupRectTransform(panel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Image overlay = panel.AddComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0.85f);
            overlay.raycastTarget = true;

            CanvasGroup cg = panel.AddComponent<CanvasGroup>();
            cg.alpha = 0;

            // Content card
            GameObject content = CreateElement(panel.transform, "Content");
            SetupRectTransform(content,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(700, 560));

            Image contentBg = content.AddComponent<Image>();
            contentBg.color = new Color(0.04f, 0.08f, 0.14f, 0.98f);

            Outline contentOutline = content.AddComponent<Outline>();
            contentOutline.effectColor = ERROR_COLOR;
            contentOutline.effectDistance = new Vector2(3, -3);

            // Title
            GameObject titleObj = CreateElement(content.transform, "TitleText");
            SetupRectTransform(titleObj,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -40), new Vector2(0, 55));
            TextMeshProUGUI titleTmp = SetupText(titleObj, "TIME'S UP!", (int)FontSizes.ValueLarge, ERROR_COLOR, FontStyles.Bold);

            // Divider after title
            CreateDivider(content.transform, -80);

            // Stats rows
            float statsY = -105f;
            float rowH = 42f;

            TextMeshProUGUI timeTmp = CreateStatRow(content.transform, "Time", "LoseTimeText", "0:00.00", CYAN_NEON, statsY);
            statsY -= rowH;
            TextMeshProUGUI errorsTmp = CreateStatRow(content.transform, "Errors", "LosePanelErrorsText", "0", ERROR_COLOR, statsY);
            statsY -= rowH;
            CreateStatRow(content.transform, "Max Combo", "LoseMaxComboValue", "0", ORANGE_NEON, statsY);
            statsY -= rowH;
            CreateStatRow(content.transform, "Penalty", "LosePenaltyValue", "-", ERROR_COLOR, statsY);

            // Divider before buttons
            CreateDivider(content.transform, statsY - 18);

            // Buttons
            GameObject buttonsContainer = CreateElement(content.transform, "ButtonsContainer");
            SetupRectTransform(buttonsContainer,
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 50), new Vector2(-40, 80));

            HorizontalLayoutGroup btnLayout = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
            btnLayout.childAlignment = TextAnchor.MiddleCenter;
            btnLayout.spacing = 30;
            btnLayout.childForceExpandWidth = false;
            btnLayout.childControlWidth = false;

            GameObject acceptBtnObj = CreatePanelButton(buttonsContainer.transform, "AcceptButton", "EXIT", new Color(0.5f, 0.5f, 0.5f), 200, 65);
            GameObject playAgainBtnObj = CreatePanelButton(buttonsContainer.transform, "PlayAgainButton", "TRY AGAIN", CYAN_NEON, 260, 65);

            // WinPanelController
            WinPanelController wpc = panel.AddComponent<WinPanelController>();
            SerializedObject wpcSo = new SerializedObject(wpc);
            wpcSo.FindProperty("isRealMoneyPanel").boolValue = false;
            wpcSo.FindProperty("canvasGroup").objectReferenceValue = cg;
            wpcSo.FindProperty("content").objectReferenceValue = content;
            wpcSo.FindProperty("titleText").objectReferenceValue = titleTmp;
            wpcSo.FindProperty("timeText").objectReferenceValue = timeTmp;
            wpcSo.FindProperty("errorsText").objectReferenceValue = errorsTmp;
            wpcSo.FindProperty("acceptButton").objectReferenceValue = acceptBtnObj.GetComponent<Button>();
            wpcSo.FindProperty("playAgainButton").objectReferenceValue = playAgainBtnObj.GetComponent<Button>();
            wpcSo.ApplyModifiedProperties();

            panel.SetActive(false);
        }

        private static void CreateCountdownPanel(Transform parent)
        {
            GameObject countdownPanel = CreateElement(parent, "CountdownPanel");
            SetupRectTransform(countdownPanel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Overlay
            GameObject overlay = CreateElement(countdownPanel.transform, "Overlay");
            SetupRectTransform(overlay, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image overlayImg = overlay.AddComponent<Image>();
            overlayImg.color = new Color(0f, 0f, 0f, 0.6f);

            // Number container
            GameObject numberContainer = CreateElement(countdownPanel.transform, "NumberContainer");
            SetupRectTransform(numberContainer,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(400, 400));

            // Countdown text
            GameObject countdownText = CreateElement(numberContainer.transform, "CountdownText");
            SetupRectTransform(countdownText,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(350, 300));
            TextMeshProUGUI countTmp = SetupText(countdownText, "3", 180, CYAN_NEON, FontStyles.Bold);
            countTmp.alignment = TextAlignmentOptions.Center;

            Outline numOutline = countdownText.AddComponent<Outline>();
            numOutline.effectColor = new Color(0f, 0.4f, 0.5f, 0.8f);
            numOutline.effectDistance = new Vector2(4, -4);

            // Add CountdownUI component
            CountdownUI countdownUI = countdownPanel.AddComponent<CountdownUI>();

            SerializedObject so = new SerializedObject(countdownUI);
            so.FindProperty("countdownPanel").objectReferenceValue = countdownPanel;
            so.FindProperty("countdownText").objectReferenceValue = countTmp;
            so.FindProperty("backgroundOverlay").objectReferenceValue = overlayImg;
            so.FindProperty("numberColor").colorValue = CYAN_NEON;
            so.FindProperty("goColor").colorValue = GREEN_NEON;
            so.ApplyModifiedProperties();

            countdownPanel.SetActive(false);
        }

        private static void CreateSettingsPanel(Transform parent)
        {
            // Full-screen overlay
            GameObject settingsPanel = CreateElement(parent, "SettingsPanel");
            SetupRectTransform(settingsPanel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Image overlay = settingsPanel.AddComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0.9f);
            overlay.raycastTarget = true;

            // Central card
            GameObject card = CreateElement(settingsPanel.transform, "SettingsCard");
            SetupRectTransform(card,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 20), new Vector2(600, 450));

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.04f, 0.08f, 0.14f, 0.98f);

            Outline cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = CYAN_NEON;
            cardOutline.effectDistance = new Vector2(3, -3);

            // ====== TITLE ======
            GameObject titleObj = CreateElement(card.transform, "SettingsTitle");
            SetupRectTransform(titleObj,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -32), new Vector2(0, 50));
            TextMeshProUGUI titleTmp = SetupText(titleObj, "ODD ONE OUT", (int)FontSizes.ValueLarge, CYAN_NEON, FontStyles.Bold);

            Outline titleGlow = titleObj.AddComponent<Outline>();
            titleGlow.effectColor = new Color(0f, 0.5f, 0.5f, 0.6f);
            titleGlow.effectDistance = new Vector2(2, -2);

            GameObject subtitleObj = CreateElement(card.transform, "SettingsSubtitle");
            SetupRectTransform(subtitleObj,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -70), new Vector2(0, 24));
            SetupText(subtitleObj, "Find the difference!", (int)FontSizes.Body, new Color(0.5f, 0.5f, 0.6f), FontStyles.Bold);

            // Divider after title
            CreateDivider(card.transform, -95);

            // ====== ROUNDS SECTION ======
            float yPos = -115f;

            GameObject roundsHeader = CreateElement(card.transform, "RoundsHeader");
            SetupRectTransform(roundsHeader,
                new Vector2(0.05f, 1), new Vector2(0.95f, 1),
                new Vector2(0, yPos), new Vector2(0, 34));
            Image roundsHeaderBg = roundsHeader.AddComponent<Image>();
            roundsHeaderBg.color = new Color(0f, 0.12f, 0.08f, 0.5f);
            GameObject roundsHeaderText = CreateElement(roundsHeader.transform, "RoundsHeaderText");
            SetupRectTransform(roundsHeaderText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetupText(roundsHeaderText, "ROUNDS", (int)FontSizes.Body, new Color(0.7f, 1f, 0.8f), FontStyles.Bold);

            yPos -= 58f;

            GameObject roundsContainer = CreateElement(card.transform, "RoundsContainer");
            SetupRectTransform(roundsContainer,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, yPos), new Vector2(450, 58));

            HorizontalLayoutGroup roundsLayout = roundsContainer.AddComponent<HorizontalLayoutGroup>();
            roundsLayout.childAlignment = TextAnchor.MiddleCenter;
            roundsLayout.spacing = 15;
            roundsLayout.childForceExpandWidth = true;
            roundsLayout.childForceExpandHeight = true;

            ToggleGroup roundsGroup = roundsContainer.AddComponent<ToggleGroup>();
            roundsGroup.allowSwitchOff = false;

            CreateSettingsToggle(roundsContainer.transform, "ToggleRounds1", "1", false, roundsGroup);
            CreateSettingsToggle(roundsContainer.transform, "ToggleRounds3", "3", false, roundsGroup);
            CreateSettingsToggle(roundsContainer.transform, "ToggleRounds5", "5", true, roundsGroup);
            CreateSettingsToggle(roundsContainer.transform, "ToggleRounds10", "10", false, roundsGroup);

            // ====== START BUTTON ======
            yPos -= 78f;

            GameObject startBtn = CreateElement(card.transform, "StartGameButton");
            SetupRectTransform(startBtn,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, yPos), new Vector2(500, 68));

            // Button shadow
            GameObject startShadow = CreateElement(startBtn.transform, "Shadow");
            SetupRectTransform(startShadow,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(3, -6), new Vector2(500, 68));
            Image startShadowImg = startShadow.AddComponent<Image>();
            startShadowImg.color = new Color(0f, 0.3f, 0.15f, 0.6f);
            startShadowImg.raycastTarget = false;

            // Button face
            Image startBtnImg = startBtn.AddComponent<Image>();
            startBtnImg.color = GREEN_NEON;

            Outline startOutline = startBtn.AddComponent<Outline>();
            startOutline.effectColor = new Color(0.1f, 0.5f, 0.25f, 1f);
            startOutline.effectDistance = new Vector2(2, -2);

            Button startButton = startBtn.AddComponent<Button>();
            startButton.targetGraphic = startBtnImg;

            ColorBlock startColors = startButton.colors;
            startColors.normalColor = Color.white;
            startColors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            startColors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            startButton.colors = startColors;

            GameObject startText = CreateElement(startBtn.transform, "StartText");
            SetupRectTransform(startText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetupText(startText, "START", (int)FontSizes.BodyLarge, DARK_BG, FontStyles.Bold);

            settingsPanel.SetActive(false);
        }

        private static void CreateProgressBar(Transform parent)
        {
            GameObject progressContainer = CreateElement(parent, "ProgressContainer");
            SetupRectTransform(progressContainer,
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 100), new Vector2(-80, 50));

            // Round indicator
            GameObject roundIndicator = CreateElement(progressContainer.transform, "RoundIndicator");
            SetupRectTransform(roundIndicator,
                new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(-50, 0), new Vector2(80, 30));
            SetupText(roundIndicator, "1/5", (int)FontSizes.Body, Color.white, FontStyles.Bold);

            // Progress bar bg
            GameObject progressBar = CreateElement(progressContainer.transform, "ProgressBar");
            SetupRectTransform(progressBar,
                new Vector2(0, 0.5f), new Vector2(1, 0.5f),
                new Vector2(0, 0), new Vector2(-100, 16));

            Image progressBg = progressBar.AddComponent<Image>();
            progressBg.color = new Color(0f, 0.2f, 0.25f, 0.8f);

            Outline progressOutline = progressBar.AddComponent<Outline>();
            progressOutline.effectColor = CYAN_NEON;
            progressOutline.effectDistance = new Vector2(1, -1);

            // Fill
            GameObject progressFill = CreateElement(progressBar.transform, "ProgressFill");
            SetupRectTransform(progressFill,
                new Vector2(0, 0), new Vector2(0.5f, 1),
                Vector2.zero, Vector2.zero);

            Image fillImg = progressFill.AddComponent<Image>();
            fillImg.color = CYAN_NEON;

            Shadow fillGlow = progressFill.AddComponent<Shadow>();
            fillGlow.effectColor = new Color(0f, 1f, 1f, 0.5f);
            fillGlow.effectDistance = new Vector2(0, -2);
        }

        private static void CreateParticleEffects(Transform parent)
        {
            GameObject particleContainer = CreateElement(parent, "ParticleEffects");
            SetupRectTransform(particleContainer, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            UISparkleEffect sparkleEffect = particleContainer.AddComponent<UISparkleEffect>();

            particleContainer.transform.SetAsLastSibling();
        }

        private static void AssignControllerReferences()
        {
            var controller = FindFirstObjectByType<OddOneOutController>();
            if (controller == null)
            {
                Debug.LogWarning("[OddOneOutUIBuilder] No se encontró OddOneOutController en la escena");
                return;
            }

            SerializedObject so = new SerializedObject(controller);
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            Transform root = canvas != null ? canvas.transform : controller.transform.root;

            // Left grid buttons
            Transform leftGrid = FindDeep(root, "LeftGrid");
            if (leftGrid != null)
            {
                SerializedProperty leftButtonsProp = so.FindProperty("leftGridButtons");
                SerializedProperty leftTextsProp = so.FindProperty("leftButtonTexts");

                if (leftButtonsProp != null) leftButtonsProp.arraySize = 16;
                if (leftTextsProp != null) leftTextsProp.arraySize = 16;

                for (int i = 0; i < 16; i++)
                {
                    Transform cell = leftGrid.Find($"LeftButton_{i}");
                    if (cell != null)
                    {
                        if (leftButtonsProp != null)
                            leftButtonsProp.GetArrayElementAtIndex(i).objectReferenceValue = cell.GetComponent<Button>();

                        Transform face = cell.Find("Face");
                        if (face != null)
                        {
                            TextMeshProUGUI txt = face.GetComponentInChildren<TextMeshProUGUI>();
                            if (leftTextsProp != null && txt != null)
                                leftTextsProp.GetArrayElementAtIndex(i).objectReferenceValue = txt;
                        }
                    }
                }
            }

            // Right grid buttons
            Transform rightGrid = FindDeep(root, "RightGrid");
            if (rightGrid != null)
            {
                SerializedProperty rightButtonsProp = so.FindProperty("rightGridButtons");
                SerializedProperty rightTextsProp = so.FindProperty("rightButtonTexts");

                if (rightButtonsProp != null) rightButtonsProp.arraySize = 16;
                if (rightTextsProp != null) rightTextsProp.arraySize = 16;

                for (int i = 0; i < 16; i++)
                {
                    Transform cell = rightGrid.Find($"RightButton_{i}");
                    if (cell != null)
                    {
                        if (rightButtonsProp != null)
                            rightButtonsProp.GetArrayElementAtIndex(i).objectReferenceValue = cell.GetComponent<Button>();

                        Transform face = cell.Find("Face");
                        if (face != null)
                        {
                            TextMeshProUGUI txt = face.GetComponentInChildren<TextMeshProUGUI>();
                            if (rightTextsProp != null && txt != null)
                                rightTextsProp.GetArrayElementAtIndex(i).objectReferenceValue = txt;
                        }
                    }
                }
            }

            // ========== UI ELEMENTS ==========
            AssignTMPByFindDeep(so, "timerText", root, "TimerText");
            AssignTMPByFindDeep(so, "roundText", root, "RoundText");
            AssignTMPByFindDeep(so, "errorsText", root, "ErrorsText");
            AssignTMPByFindDeep(so, "comboText", root, "ComboText");
            AssignTMPByFindDeep(so, "roundIndicatorText", root, "RoundIndicator");

            // Progress fill
            Transform progressFill = FindDeep(root, "ProgressFill");
            if (progressFill != null)
            {
                SerializedProperty progressProp = so.FindProperty("progressFill");
                if (progressProp != null)
                    progressProp.objectReferenceValue = progressFill.GetComponent<RectTransform>();
            }

            // ========== COUNTDOWN ==========
            Transform countdownPanelT = FindDeep(root, "CountdownPanel");
            if (countdownPanelT != null)
            {
                SerializedProperty countdownProp = so.FindProperty("countdownUI");
                if (countdownProp != null)
                    countdownProp.objectReferenceValue = countdownPanelT.GetComponent<CountdownUI>();
            }

            // ========== SETTINGS PANEL ==========
            Transform settingsPanelT = FindDeep(root, "SettingsPanel");
            if (settingsPanelT != null)
            {
                AssignGameObject(so, "settingsPanel", settingsPanelT.gameObject);

                AssignToggle(so, "toggleRounds1", FindDeep(settingsPanelT, "ToggleRounds1"));
                AssignToggle(so, "toggleRounds3", FindDeep(settingsPanelT, "ToggleRounds3"));
                AssignToggle(so, "toggleRounds5", FindDeep(settingsPanelT, "ToggleRounds5"));
                AssignToggle(so, "toggleRounds10", FindDeep(settingsPanelT, "ToggleRounds10"));

                Transform startBtn = FindDeep(settingsPanelT, "StartGameButton");
                if (startBtn != null)
                {
                    SerializedProperty startProp = so.FindProperty("startGameButton");
                    if (startProp != null)
                        startProp.objectReferenceValue = startBtn.GetComponent<Button>();
                }
            }

            // ========== FEEDBACK ==========
            Transform feedbackPanelT = FindDeep(root, "FeedbackPanel");
            if (feedbackPanelT != null)
            {
                AssignGameObject(so, "feedbackPanel", feedbackPanelT.gameObject);

                Transform feedbackTextT = FindDeep(feedbackPanelT, "FeedbackText");
                if (feedbackTextT != null)
                {
                    SerializedProperty ftProp = so.FindProperty("feedbackText");
                    if (ftProp != null)
                        ftProp.objectReferenceValue = feedbackTextT.GetComponent<TextMeshProUGUI>();
                }
            }

            // ========== NORMAL WIN/LOSE PANELS ==========
            Transform winPanelNormalT = FindDeep(root, "WinPanel_Normal");
            if (winPanelNormalT != null)
            {
                SerializedProperty winNormalProp = so.FindProperty("winPanelNormal");
                if (winNormalProp != null)
                    winNormalProp.objectReferenceValue = winPanelNormalT.GetComponent<WinPanelController>();
            }

            Transform losePanelNormalT = FindDeep(root, "LosePanel_Normal");
            if (losePanelNormalT != null)
            {
                SerializedProperty loseNormalProp = so.FindProperty("losePanelNormal");
                if (loseNormalProp != null)
                    loseNormalProp.objectReferenceValue = losePanelNormalT.GetComponent<WinPanelController>();
            }

            // ========== REAL MONEY PANELS ==========
            Transform winPanelRM = FindDeep(root, "WinPanel_RealMoney");
            Transform losePanelRM = FindDeep(root, "LosePanel_RealMoney");
            if (winPanelRM != null)
            {
                SerializedProperty winRMProp = so.FindProperty("winPanelRealMoney");
                if (winRMProp != null)
                    winRMProp.objectReferenceValue = winPanelRM.GetComponent<WinPanelController>();
            }
            if (losePanelRM != null)
            {
                SerializedProperty loseRMProp = so.FindProperty("losePanelRealMoney");
                if (loseRMProp != null)
                    loseRMProp.objectReferenceValue = losePanelRM.GetComponent<WinPanelController>();
            }

            // ========== SPARKLE ==========
            Transform particleEffects = FindDeep(root, "ParticleEffects");
            if (particleEffects != null)
            {
                SerializedProperty sparkleProp = so.FindProperty("sparkleEffect");
                if (sparkleProp != null)
                    sparkleProp.objectReferenceValue = particleEffects.GetComponent<UISparkleEffect>();
            }

            // ========== NAVIGATION (MinigameBase) ==========
            // backButton is not used in OddOneOut (back via WinPanelController Accept)
            // playAgainButton is not used directly (WinPanelController handles it)

            so.ApplyModifiedProperties();
            Debug.Log("[OddOneOutUIBuilder] Referencias asignadas al Controller");
        }

        // ========== HELPER METHODS ==========

        private static void CreateSettingsToggle(Transform parent, string name, string label, bool isOn, ToggleGroup group = null)
        {
            GameObject toggleObj = CreateElement(parent, name);

            Image toggleBg = toggleObj.AddComponent<Image>();
            toggleBg.color = isOn ? CYAN_NEON : BUTTON_BG;

            Outline toggleOutline = toggleObj.AddComponent<Outline>();
            toggleOutline.effectColor = new Color(0f, 0.7f, 0.7f, 0.5f);
            toggleOutline.effectDistance = new Vector2(1.5f, -1.5f);

            Toggle toggle = toggleObj.AddComponent<Toggle>();
            toggle.targetGraphic = toggleBg;
            toggle.toggleTransition = Toggle.ToggleTransition.None;
            toggle.graphic = null;

            if (group != null)
            {
                toggle.group = group;
                SerializedObject toggleSo = new SerializedObject(toggle);
                toggleSo.FindProperty("m_Group").objectReferenceValue = group;
                toggleSo.ApplyModifiedProperties();
            }

            toggle.isOn = isOn;

            // Label
            GameObject labelObj = CreateElement(toggleObj.transform, "Label");
            SetupRectTransform(labelObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI labelTmp = SetupText(labelObj, label, (int)FontSizes.Body, isOn ? DARK_BG : Color.white, FontStyles.Bold);
            labelTmp.raycastTarget = false;
        }

        private static GameObject CreatePanelButton(Transform parent, string name, string text, Color color, float width, float height)
        {
            GameObject btn = CreateElement(parent, name);

            LayoutElement le = btn.AddComponent<LayoutElement>();
            le.preferredWidth = width;
            le.preferredHeight = height;

            Image faceImg = btn.AddComponent<Image>();
            faceImg.color = color;

            GameObject textObj = CreateElement(btn.transform, "Text");
            SetupRectTransform(textObj, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-10, -6));
            SetupText(textObj, text, (int)FontSizes.Body, DARK_BG, FontStyles.Bold);

            Button button = btn.AddComponent<Button>();
            button.targetGraphic = faceImg;

            return btn;
        }

        private static void CreateDivider(Transform parent, float yPos)
        {
            GameObject divider = CreateElement(parent, "Divider");
            SetupRectTransform(divider,
                new Vector2(0.08f, 1), new Vector2(0.92f, 1),
                new Vector2(0, yPos), new Vector2(0, 2));
            Image divImg = divider.AddComponent<Image>();
            divImg.color = new Color(1f, 1f, 1f, 0.1f);
            divImg.raycastTarget = false;
        }

        private static TextMeshProUGUI CreateStatRow(Transform parent, string label, string valueName, string defaultValue, Color valueColor, float yPos)
        {
            // Label (left-aligned)
            GameObject labelObj = CreateElement(parent, valueName + "_Label");
            SetupRectTransform(labelObj,
                new Vector2(0.05f, 1), new Vector2(0.5f, 1),
                new Vector2(0, yPos), new Vector2(0, 38));
            TextMeshProUGUI labelTmp = SetupText(labelObj, label, (int)FontSizes.Body, new Color(0.6f, 0.65f, 0.75f), FontStyles.Bold);
            labelTmp.alignment = TextAlignmentOptions.Left;

            // Value (right-aligned)
            GameObject valueObj = CreateElement(parent, valueName);
            SetupRectTransform(valueObj,
                new Vector2(0.5f, 1), new Vector2(0.95f, 1),
                new Vector2(0, yPos), new Vector2(0, 38));
            TextMeshProUGUI valueTmp = SetupText(valueObj, defaultValue, (int)FontSizes.Body, valueColor, FontStyles.Bold);
            valueTmp.alignment = TextAlignmentOptions.Right;

            return valueTmp;
        }

        private static Transform FindDeep(Transform root, string name)
        {
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                Transform result = FindDeep(child, name);
                if (result != null) return result;
            }
            return null;
        }

        private static void AssignTMPByFindDeep(SerializedObject so, string propertyName, Transform root, string objectName)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop == null) return;

            Transform t = FindDeep(root, objectName);
            if (t != null)
            {
                prop.objectReferenceValue = t.GetComponent<TextMeshProUGUI>();
            }
        }

        private static void AssignToggle(SerializedObject so, string propertyName, Transform toggleTransform)
        {
            if (toggleTransform == null) return;
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop != null)
                prop.objectReferenceValue = toggleTransform.GetComponent<Toggle>();
        }

        private static void AssignGameObject(SerializedObject so, string propertyName, GameObject obj)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop != null)
                prop.objectReferenceValue = obj;
        }

        // ========== BASE UTILITIES ==========

        private static GameObject CreateElement(Transform parent, string name)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            return obj;
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

        private static TextMeshProUGUI SetupText(GameObject obj, string text, int fontSize, Color color, FontStyles style)
        {
            TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = obj.AddComponent<TextMeshProUGUI>();

            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.raycastTarget = false;

            return tmp;
        }

        private static void AddLayoutElement(GameObject obj, float width, float height)
        {
            LayoutElement layout = obj.GetComponent<LayoutElement>();
            if (layout == null) layout = obj.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = height;
        }
    }
}
