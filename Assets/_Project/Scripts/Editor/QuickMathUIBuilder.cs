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
    /// Editor script para reconstruir la UI de QuickMath
    /// Settings panel, feedback, win/lose panels (WinPanelController)
    /// </summary>
    public class QuickMathUIBuilder : EditorWindow
    {
        // Colores del tema neon
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color MAGENTA_NEON = new Color(1f, 0f, 0.8f, 1f);
        private static readonly Color GREEN_NEON = new Color(0.3f, 1f, 0.5f, 1f);
        private static readonly Color ORANGE_NEON = new Color(1f, 0.6f, 0.2f, 1f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color DARK_BG = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color PANEL_BG = new Color(0.05f, 0.1f, 0.15f, 0.95f);
        private static readonly Color BUTTON_BG = new Color(0.08f, 0.12f, 0.18f, 1f);
        private static readonly Color ERROR_COLOR = new Color(1f, 0.3f, 0.3f, 1f);

        // Stats Bar icon paths
        private const string TIMER_ICON_PATH = "Assets/_Project/Art/Icons/UI/TimerIcon.png";
        private const string ROUND_ICON_PATH = "Assets/_Project/Art/Icons/UI/RoundIcon.png";
        private const string ERROR_ICON_PATH = "Assets/_Project/Art/Icons/UI/ErrorIcon.png";

        // Tamanos optimizados
        private const float ANSWER_BUTTON_WIDTH = 260f;
        private const float ANSWER_BUTTON_HEIGHT = 110f;
        private const float EQUATION_PANEL_WIDTH = 850f;
        private const float EQUATION_FONT_SIZE = 80f;

        [MenuItem("DigitPark/UI Builders/Games/QuickMath", false, 114)]
        public static void ShowWindow()
        {
            GetWindow<QuickMathUIBuilder>("QuickMath UI Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("QuickMath UI Builder", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "QuickMath con:\n" +
                "- Settings panel (operaciones, dificultad, rondas)\n" +
                "- Division support\n" +
                "- Feedback correcto/incorrecto\n" +
                "- Win/Lose panels (WinPanelController)",
                MessageType.Info);

            GUILayout.Space(10);

            if (GUILayout.Button("Reconstruir QuickMath UI", GUILayout.Height(40)))
            {
                RebuildQuickMathUI();
            }

            GUILayout.Space(15);
            GUI.backgroundColor = new Color(0.5f, 0.8f, 1f);
            if (GUILayout.Button("Auto-Asignar Referencias", GUILayout.Height(30)))
            {
                QuickMathReferenceAssigner.RunAutoAssign();
            }
            GUI.backgroundColor = Color.white;
        }

        private static void RebuildQuickMathUI()
        {
            CleanupOldUI();
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("[QuickMathUIBuilder] No se encontro Canvas en la escena");
                return;
            }

            Transform canvasTransform = canvas.transform;

            CleanOldElements(canvasTransform);
            CreateQuickMathLayout(canvasTransform);
            AssignControllerReferences();

            Debug.Log("[QuickMathUIBuilder] QuickMath UI reconstruida!");
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

        private static void CreateQuickMathLayout(Transform canvasTransform)
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

            // ========== COMBO/STREAK DISPLAY ==========
            CreateComboDisplay(safeArea.transform);

            // ========== ECUACION GIGANTE ==========
            CreateEquationPanel(safeArea.transform);

            // ========== BOTONES DE RESPUESTA 3D ==========
            CreateAnswerButtons(safeArea.transform);

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
                new Vector2(0, 0), new Vector2(400, 50));
            TextMeshProUGUI titleTmp = SetupText(title, "QUICK MATH", 78, CYAN_NEON, FontStyles.Bold);

            Outline titleGlow = title.AddComponent<Outline>();
            titleGlow.effectColor = new Color(0f, 1f, 1f, 0.5f);
            titleGlow.effectDistance = new Vector2(2, -2);
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
                "00:00", Color.white, 240, timerIcon, 33);

            // Round counter
            CreateStatItem(statsBar.transform, "RoundContainer", "RoundIcon", "RoundText",
                "1/10", CYAN_NEON, 180, roundIcon, 36);

            // Errors
            CreateStatItem(statsBar.transform, "ErrorsContainer", "ErrorsIcon", "ErrorsText",
                "0", ERROR_COLOR, 120, errorIcon, 33);
        }

        private static void CreateStatItem(Transform parent, string containerName, string iconName,
            string textName, string defaultText, Color color, float width, Sprite iconSprite = null, int fontSize = 22)
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

        private static void CreateComboDisplay(Transform parent)
        {
            GameObject comboContainer = CreateElement(parent, "ComboContainer");
            SetupRectTransform(comboContainer,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -170), new Vector2(250, 60));

            Image comboBg = comboContainer.AddComponent<Image>();
            comboBg.color = new Color(0.1f, 0.05f, 0.15f, 0.9f);

            Outline comboOutline = comboContainer.AddComponent<Outline>();
            comboOutline.effectColor = ORANGE_NEON;
            comboOutline.effectDistance = new Vector2(2, -2);

            CanvasGroup comboCg = comboContainer.AddComponent<CanvasGroup>();
            comboCg.alpha = 0;

            GameObject streakIcon = CreateElement(comboContainer.transform, "StreakIcon");
            SetupRectTransform(streakIcon,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(25, 0), new Vector2(40, 40));
            Image streakImg = streakIcon.AddComponent<Image>();
            streakImg.color = ORANGE_NEON;

            GameObject comboText = CreateElement(comboContainer.transform, "ComboText");
            SetupRectTransform(comboText,
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(20, 0), new Vector2(-20, 0));
            TextMeshProUGUI comboTmp = SetupText(comboText, "x5 STREAK!", 28, ORANGE_NEON, FontStyles.Bold);
            comboTmp.alignment = TextAlignmentOptions.Center;

            comboContainer.SetActive(false);
        }

        private static void CreateEquationPanel(Transform parent)
        {
            GameObject equationPanel = CreateElement(parent, "EquationPanel");
            SetupRectTransform(equationPanel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 100), new Vector2(850, 180));

            // Shadow
            GameObject panelShadow = CreateElement(equationPanel.transform, "PanelShadow");
            SetupRectTransform(panelShadow,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(5, -12), new Vector2(850, 180));
            Image shadowImg = panelShadow.AddComponent<Image>();
            shadowImg.color = new Color(0f, 0f, 0f, 0.5f);
            shadowImg.raycastTarget = false;

            // Side
            GameObject panelSide = CreateElement(equationPanel.transform, "PanelSide");
            SetupRectTransform(panelSide,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, -5), new Vector2(850, 12));
            Image sideImg = panelSide.AddComponent<Image>();
            sideImg.color = new Color(0f, 0.3f, 0.35f, 1f);
            sideImg.raycastTarget = false;

            // Face
            GameObject panelFace = CreateElement(equationPanel.transform, "PanelFace");
            SetupRectTransform(panelFace,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 5), new Vector2(850, 170));
            Image panelBg = panelFace.AddComponent<Image>();
            panelBg.color = new Color(0.04f, 0.08f, 0.15f, 0.98f);

            Outline panelOutline = panelFace.AddComponent<Outline>();
            panelOutline.effectColor = CYAN_NEON;
            panelOutline.effectDistance = new Vector2(3, -3);

            GridGlowPulse panelGlow = panelFace.AddComponent<GridGlowPulse>();

            // Equation container
            GameObject equationContainer = CreateElement(panelFace.transform, "EquationContainer");
            SetupRectTransform(equationContainer, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            HorizontalLayoutGroup eqLayout = equationContainer.AddComponent<HorizontalLayoutGroup>();
            eqLayout.childAlignment = TextAnchor.MiddleCenter;
            eqLayout.spacing = 10;
            eqLayout.padding = new RectOffset(20, 20, 0, 0);
            eqLayout.childForceExpandWidth = false;
            eqLayout.childForceExpandHeight = true;

            // Number A
            GameObject numberA = CreateElement(equationContainer.transform, "NumberA");
            AddLayoutElement(numberA, 120, 150);
            TextMeshProUGUI numATmp = SetupText(numberA, "?", EQUATION_FONT_SIZE, Color.white, FontStyles.Bold);
            numATmp.alignment = TextAlignmentOptions.Center;

            // Operator
            GameObject operatorText = CreateElement(equationContainer.transform, "OperatorText");
            AddLayoutElement(operatorText, 60, 150);
            TextMeshProUGUI opTmp = SetupText(operatorText, "+", EQUATION_FONT_SIZE, CYAN_NEON, FontStyles.Bold);
            opTmp.alignment = TextAlignmentOptions.Center;

            // Number B
            GameObject numberB = CreateElement(equationContainer.transform, "NumberB");
            AddLayoutElement(numberB, 120, 150);
            TextMeshProUGUI numBTmp = SetupText(numberB, "?", EQUATION_FONT_SIZE, Color.white, FontStyles.Bold);
            numBTmp.alignment = TextAlignmentOptions.Center;

            // ProblemText (hidden, for compatibility)
            GameObject problemText = CreateElement(equationPanel.transform, "ProblemText");
            SetupRectTransform(problemText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI probTmp = SetupText(problemText, "? ? ? = ?", 1, Color.clear, FontStyles.Bold);
            probTmp.enabled = false;
        }

        private static void CreateAnswerButtons(Transform parent)
        {
            GameObject answersContainer = CreateElement(parent, "AnswersContainer");
            SetupRectTransform(answersContainer,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -190), new Vector2(EQUATION_PANEL_WIDTH, ANSWER_BUTTON_HEIGHT + 30));

            HorizontalLayoutGroup answersLayout = answersContainer.AddComponent<HorizontalLayoutGroup>();
            answersLayout.childAlignment = TextAnchor.MiddleCenter;
            answersLayout.spacing = 20;
            answersLayout.padding = new RectOffset(10, 10, 0, 0);
            answersLayout.childForceExpandWidth = false;
            answersLayout.childForceExpandHeight = false;
            answersLayout.childControlWidth = true;
            answersLayout.childControlHeight = true;

            for (int i = 0; i < 3; i++)
            {
                CreateAnswerButton3D(answersContainer.transform, i);
            }
        }

        private static void CreateAnswerButton3D(Transform parent, int index)
        {
            GameObject cell = CreateElement(parent, $"AnswerButton_{index}");
            AddLayoutElement(cell, ANSWER_BUTTON_WIDTH, ANSWER_BUTTON_HEIGHT);

            // Shadow
            GameObject shadow = CreateElement(cell.transform, "Shadow");
            SetupRectTransform(shadow,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(3, -8), new Vector2(ANSWER_BUTTON_WIDTH - 8, ANSWER_BUTTON_HEIGHT - 8));
            Image shadowImg = shadow.AddComponent<Image>();
            shadowImg.color = new Color(0f, 0f, 0f, 0.5f);

            // Side
            GameObject side = CreateElement(cell.transform, "Side");
            SetupRectTransform(side,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 0), new Vector2(ANSWER_BUTTON_WIDTH - 8, 10));
            Image sideImg = side.AddComponent<Image>();
            sideImg.color = new Color(0f, 0.3f, 0.35f, 1f);

            // Face
            GameObject face = CreateElement(cell.transform, "Face");
            SetupRectTransform(face,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 4), new Vector2(ANSWER_BUTTON_WIDTH - 8, ANSWER_BUTTON_HEIGHT - 8));
            Image faceImg = face.AddComponent<Image>();
            faceImg.color = BUTTON_BG;

            Outline faceOutline = face.AddComponent<Outline>();
            faceOutline.effectColor = CYAN_NEON;
            faceOutline.effectDistance = new Vector2(3, -3);

            // Answer text
            GameObject answerText = CreateElement(face.transform, $"AnswerText_{index}");
            SetupRectTransform(answerText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI answerTmp = SetupText(answerText, "?", 48, CYAN_NEON, FontStyles.Bold);
            answerTmp.alignment = TextAlignmentOptions.Center;

            Outline textGlow = answerText.AddComponent<Outline>();
            textGlow.effectColor = new Color(0f, 0.5f, 0.5f, 0.5f);
            textGlow.effectDistance = new Vector2(1.5f, -1.5f);

            // Button
            Button button = cell.AddComponent<Button>();
            button.targetGraphic = faceImg;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.2f, 1f);
            colors.pressedColor = new Color(0.8f, 0.9f, 0.85f, 1f);
            colors.fadeDuration = 0.05f;
            button.colors = colors;

            // QuickMathCell3D
            QuickMathCell3D cell3D = cell.AddComponent<QuickMathCell3D>();

            SerializedObject so = new SerializedObject(cell3D);
            so.FindProperty("buttonFace").objectReferenceValue = face.GetComponent<RectTransform>();
            so.FindProperty("shadowImage").objectReferenceValue = shadowImg;
            so.FindProperty("sideImage").objectReferenceValue = sideImg;
            so.FindProperty("faceImage").objectReferenceValue = faceImg;
            so.FindProperty("glowOutline").objectReferenceValue = faceOutline;
            so.FindProperty("answerText").objectReferenceValue = answerTmp;
            so.ApplyModifiedProperties();
        }

        private static void CreateFeedbackPanel(Transform parent)
        {
            GameObject feedbackPanel = CreateElement(parent, "FeedbackPanel");
            SetupRectTransform(feedbackPanel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -320), new Vector2(700, 80));

            Image bg = feedbackPanel.AddComponent<Image>();
            bg.color = PANEL_BG;

            Outline panelOutline = feedbackPanel.AddComponent<Outline>();
            panelOutline.effectColor = CYAN_NEON;
            panelOutline.effectDistance = new Vector2(2, -2);

            GameObject feedbackText = CreateElement(feedbackPanel.transform, "FeedbackText");
            SetupRectTransform(feedbackText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI feedbackTmp = SetupText(feedbackText, "", 42, GREEN_NEON, FontStyles.Bold);
            feedbackTmp.alignment = TextAlignmentOptions.Center;
            feedbackTmp.enableWordWrapping = false;

            feedbackPanel.SetActive(false);
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
            SetupText(roundIndicator, "8/15", 20, Color.white, FontStyles.Bold);

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

        private static void CreateNormalWinPanel(Transform parent)
        {
            GameObject panel = CreateElement(parent, "WinPanel_Normal");
            SetupRectTransform(panel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Image overlay = panel.AddComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0.85f);
            overlay.raycastTarget = true;

            CanvasGroup cg = panel.AddComponent<CanvasGroup>();
            cg.alpha = 0;

            // Content card - taller for more stats
            GameObject content = CreateElement(panel.transform, "Content");
            SetupRectTransform(content,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(700, 620));

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
            TextMeshProUGUI titleTmp = SetupText(titleObj, "COMPLETED!", 44, GREEN_NEON, FontStyles.Bold);

            // Divider after title
            CreateDivider(content.transform, -80);

            // Stats rows
            float statsY = -105f;
            float rowH = 42f;

            TextMeshProUGUI timeTmp = CreateStatRow(content.transform, "Time", "TimeText", "0:00.00", CYAN_NEON, statsY);
            statsY -= rowH;
            TextMeshProUGUI errorsTmp = CreateStatRow(content.transform, "Errors", "PanelErrorsText", "0", ERROR_COLOR, statsY);
            statsY -= rowH;
            CreateStatRow(content.transform, "Max Streak", "MaxStreakValue", "0", ORANGE_NEON, statsY);
            statsY -= rowH;
            CreateStatRow(content.transform, "Penalty", "PenaltyValue", "-", ERROR_COLOR, statsY);
            statsY -= rowH;
            CreateStatRow(content.transform, "Rounds", "RoundsValue", "0/0", CYAN_NEON, statsY);

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

            // Content card - taller for more stats
            GameObject content = CreateElement(panel.transform, "Content");
            SetupRectTransform(content,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(700, 620));

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
            TextMeshProUGUI titleTmp = SetupText(titleObj, "TIME'S UP!", 44, ERROR_COLOR, FontStyles.Bold);

            // Divider after title
            CreateDivider(content.transform, -80);

            // Stats rows
            float statsY = -105f;
            float rowH = 42f;

            TextMeshProUGUI timeTmp = CreateStatRow(content.transform, "Time", "LoseTimeText", "0:00.00", CYAN_NEON, statsY);
            statsY -= rowH;
            TextMeshProUGUI errorsTmp = CreateStatRow(content.transform, "Errors", "LosePanelErrorsText", "0", ERROR_COLOR, statsY);
            statsY -= rowH;
            CreateStatRow(content.transform, "Max Streak", "LoseMaxStreakValue", "0", ORANGE_NEON, statsY);
            statsY -= rowH;
            CreateStatRow(content.transform, "Penalty", "LosePenaltyValue", "-", ERROR_COLOR, statsY);
            statsY -= rowH;
            CreateStatRow(content.transform, "Rounds", "LoseRoundsValue", "0/0", CYAN_NEON, statsY);

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

        private static void CreateSettingsPanel(Transform parent)
        {
            // Full-screen overlay
            GameObject settingsPanel = CreateElement(parent, "SettingsPanel");
            SetupRectTransform(settingsPanel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Image overlay = settingsPanel.AddComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0.9f);
            overlay.raycastTarget = true;

            // Central card - taller for better spacing
            GameObject card = CreateElement(settingsPanel.transform, "SettingsCard");
            SetupRectTransform(card,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 20), new Vector2(700, 820));

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.04f, 0.08f, 0.14f, 0.98f);

            Outline cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = CYAN_NEON;
            cardOutline.effectDistance = new Vector2(3, -3);

            // ====== TITLE AREA ======
            GameObject titleObj = CreateElement(card.transform, "SettingsTitle");
            SetupRectTransform(titleObj,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -32), new Vector2(0, 50));
            TextMeshProUGUI titleTmp = SetupText(titleObj, "QUICK MATH", 44, CYAN_NEON, FontStyles.Bold);

            Outline titleGlow = titleObj.AddComponent<Outline>();
            titleGlow.effectColor = new Color(0f, 0.5f, 0.5f, 0.6f);
            titleGlow.effectDistance = new Vector2(2, -2);

            GameObject subtitleObj = CreateElement(card.transform, "SettingsSubtitle");
            SetupRectTransform(subtitleObj,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -70), new Vector2(0, 24));
            SetupText(subtitleObj, "Configure your game", 18, new Color(0.5f, 0.5f, 0.6f), FontStyles.Bold);

            // Divider after title
            CreateDivider(card.transform, -95);

            // ====== OPERATIONS SECTION ======
            float yPos = -115f;

            GameObject opsHeader = CreateElement(card.transform, "OperationsHeader");
            SetupRectTransform(opsHeader,
                new Vector2(0.05f, 1), new Vector2(0.95f, 1),
                new Vector2(0, yPos), new Vector2(0, 34));
            Image opsHeaderBg = opsHeader.AddComponent<Image>();
            opsHeaderBg.color = new Color(0f, 0.15f, 0.2f, 0.5f);
            GameObject opsHeaderText = CreateElement(opsHeader.transform, "OperationsHeaderText");
            SetupRectTransform(opsHeaderText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetupText(opsHeaderText, "OPERATIONS", 21, new Color(0.7f, 0.9f, 1f), FontStyles.Bold);

            yPos -= 58f;

            GameObject opsContainer = CreateElement(card.transform, "OperationsContainer");
            SetupRectTransform(opsContainer,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, yPos), new Vector2(620, 58));

            HorizontalLayoutGroup opsLayout = opsContainer.AddComponent<HorizontalLayoutGroup>();
            opsLayout.childAlignment = TextAnchor.MiddleCenter;
            opsLayout.spacing = 15;
            opsLayout.childForceExpandWidth = true;
            opsLayout.childForceExpandHeight = true;

            CreateSettingsToggle(opsContainer.transform, "ToggleAddition", "+", true);
            CreateSettingsToggle(opsContainer.transform, "ToggleSubtraction", "-", true);
            CreateSettingsToggle(opsContainer.transform, "ToggleMultiplication", "\u00D7", false);
            CreateSettingsToggle(opsContainer.transform, "ToggleDivision", "\u00F7", false);

            // Divider
            yPos -= 38f;
            CreateDivider(card.transform, yPos);

            // ====== DIFFICULTY SECTION ======
            yPos -= 22f;

            GameObject diffHeader = CreateElement(card.transform, "DifficultyHeader");
            SetupRectTransform(diffHeader,
                new Vector2(0.05f, 1), new Vector2(0.95f, 1),
                new Vector2(0, yPos), new Vector2(0, 34));
            Image diffHeaderBg = diffHeader.AddComponent<Image>();
            diffHeaderBg.color = new Color(0.15f, 0.1f, 0f, 0.5f);
            GameObject diffHeaderText = CreateElement(diffHeader.transform, "DifficultyHeaderText");
            SetupRectTransform(diffHeaderText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetupText(diffHeaderText, "DIFFICULTY", 21, new Color(1f, 0.9f, 0.7f), FontStyles.Bold);

            yPos -= 58f;

            GameObject diffContainer = CreateElement(card.transform, "DifficultyContainer");
            SetupRectTransform(diffContainer,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, yPos), new Vector2(580, 58));

            HorizontalLayoutGroup diffLayout = diffContainer.AddComponent<HorizontalLayoutGroup>();
            diffLayout.childAlignment = TextAnchor.MiddleCenter;
            diffLayout.spacing = 15;
            diffLayout.childForceExpandWidth = true;
            diffLayout.childForceExpandHeight = true;

            ToggleGroup diffGroup = diffContainer.AddComponent<ToggleGroup>();
            diffGroup.allowSwitchOff = false;

            CreateSettingsToggle(diffContainer.transform, "ToggleEasy", "EASY", false, diffGroup);
            CreateSettingsToggle(diffContainer.transform, "ToggleNormal", "NORMAL", true, diffGroup);
            CreateSettingsToggle(diffContainer.transform, "ToggleHard", "HARD", false, diffGroup);

            // Difficulty description
            yPos -= 42f;

            GameObject diffDesc = CreateElement(card.transform, "DifficultyDescText");
            SetupRectTransform(diffDesc,
                new Vector2(0.05f, 1), new Vector2(0.95f, 1),
                new Vector2(0, yPos), new Vector2(0, 32));
            TextMeshProUGUI descTmp = SetupText(diffDesc, "Numbers 1-25, all operations available", 17, new Color(0.5f, 0.5f, 0.6f), FontStyles.Bold);
            descTmp.enableWordWrapping = true;

            // Divider
            yPos -= 28f;
            CreateDivider(card.transform, yPos);

            // ====== ROUNDS SECTION ======
            yPos -= 22f;

            GameObject roundsHeader = CreateElement(card.transform, "RoundsHeader");
            SetupRectTransform(roundsHeader,
                new Vector2(0.05f, 1), new Vector2(0.95f, 1),
                new Vector2(0, yPos), new Vector2(0, 34));
            Image roundsHeaderBg = roundsHeader.AddComponent<Image>();
            roundsHeaderBg.color = new Color(0f, 0.12f, 0.08f, 0.5f);
            GameObject roundsHeaderText = CreateElement(roundsHeader.transform, "RoundsHeaderText");
            SetupRectTransform(roundsHeaderText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetupText(roundsHeaderText, "ROUNDS", 21, new Color(0.7f, 1f, 0.8f), FontStyles.Bold);

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
            CreateSettingsToggle(roundsContainer.transform, "ToggleRounds5", "5", false, roundsGroup);
            CreateSettingsToggle(roundsContainer.transform, "ToggleRounds10", "10", true, roundsGroup);

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
            SetupText(startText, "START", 34, DARK_BG, FontStyles.Bold);

            settingsPanel.SetActive(false);
        }

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
            toggle.graphic = null; // Visual updates handled by controller at runtime

            // Set group BEFORE isOn so ToggleGroup registers properly
            if (group != null)
            {
                toggle.group = group;
                // Also serialize via SerializedObject for scene persistence
                SerializedObject toggleSo = new SerializedObject(toggle);
                toggleSo.FindProperty("m_Group").objectReferenceValue = group;
                toggleSo.ApplyModifiedProperties();
            }

            toggle.isOn = isOn;

            // Label
            GameObject labelObj = CreateElement(toggleObj.transform, "Label");
            SetupRectTransform(labelObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI labelTmp = SetupText(labelObj, label, 28, isOn ? DARK_BG : Color.white, FontStyles.Bold);
            labelTmp.raycastTarget = false;
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
            var controller = FindFirstObjectByType<QuickMathController>();
            if (controller == null)
            {
                Debug.LogWarning("[QuickMathUIBuilder] No se encontro QuickMathController en la escena");
                return;
            }

            SerializedObject so = new SerializedObject(controller);
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            Transform root = canvas != null ? canvas.transform : controller.transform.root;

            // ========== CONFIG ==========
            string[] configGuids = AssetDatabase.FindAssets("t:MinigameConfig");
            MinigameConfig foundConfig = null;

            foreach (string guid in configGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.ToLower().Contains("quickmath"))
                {
                    foundConfig = AssetDatabase.LoadAssetAtPath<MinigameConfig>(path);
                    break;
                }
            }

            if (foundConfig == null)
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Project/Resources/Configs"))
                {
                    if (!AssetDatabase.IsValidFolder("Assets/_Project/Resources"))
                        AssetDatabase.CreateFolder("Assets/_Project", "Resources");
                    AssetDatabase.CreateFolder("Assets/_Project/Resources", "Configs");
                }

                foundConfig = ScriptableObject.CreateInstance<MinigameConfig>();
                foundConfig.gameType = GameType.QuickMath;
                foundConfig.displayName = "Quick Math";
                foundConfig.description = "Resuelve operaciones matematicas rapidamente";
                foundConfig.rounds = 10;
                foundConfig.timeLimit = 0;
                foundConfig.difficultyLevel = 2;

                AssetDatabase.CreateAsset(foundConfig, "Assets/_Project/Resources/Configs/QuickMathConfig.asset");
                AssetDatabase.SaveAssets();
                Debug.Log("[QuickMathUIBuilder] Creado nuevo MinigameConfig: QuickMathConfig.asset");
            }

            SerializedProperty configProp = so.FindProperty("config");
            if (configProp != null && foundConfig != null)
            {
                configProp.objectReferenceValue = foundConfig;
            }

            // ========== EQUATION DISPLAY ==========
            AssignTMPByFindDeep(so, "problemText", root, "ProblemText");
            AssignTMPByFindDeep(so, "numberAText", root, "NumberA");
            AssignTMPByFindDeep(so, "numberBText", root, "NumberB");
            AssignTMPByFindDeep(so, "operatorText", root, "OperatorText");

            Transform eqPanel = FindDeep(root, "EquationPanel");
            if (eqPanel != null)
            {
                SerializedProperty eqProp = so.FindProperty("equationPanel");
                if (eqProp != null) eqProp.objectReferenceValue = eqPanel.GetComponent<RectTransform>();
            }

            // ========== ANSWER BUTTONS ==========
            Transform answersContainer = FindDeep(root, "AnswersContainer");
            if (answersContainer != null)
            {
                SerializedProperty answerButtonsProp = so.FindProperty("answerButtons");
                SerializedProperty answerTextsProp = so.FindProperty("answerTexts");

                if (answerButtonsProp != null) answerButtonsProp.arraySize = 3;
                if (answerTextsProp != null) answerTextsProp.arraySize = 3;

                for (int i = 0; i < 3; i++)
                {
                    Transform btn = FindDeep(answersContainer, $"AnswerButton_{i}");
                    if (btn != null)
                    {
                        if (answerButtonsProp != null)
                            answerButtonsProp.GetArrayElementAtIndex(i).objectReferenceValue = btn.GetComponent<Button>();

                        Transform face = FindDeep(btn, "Face");
                        if (face != null && answerTextsProp != null)
                        {
                            TextMeshProUGUI txt = face.GetComponentInChildren<TextMeshProUGUI>();
                            if (txt != null)
                                answerTextsProp.GetArrayElementAtIndex(i).objectReferenceValue = txt;
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

            // Combo CanvasGroup
            Transform comboContainer = FindDeep(root, "ComboContainer");
            if (comboContainer != null)
            {
                SerializedProperty comboCgProp = so.FindProperty("comboCanvasGroup");
                if (comboCgProp != null)
                    comboCgProp.objectReferenceValue = comboContainer.GetComponent<CanvasGroup>();
            }

            // Progress fill
            Transform progressFill = FindDeep(root, "ProgressFill");
            if (progressFill != null)
            {
                SerializedProperty progressProp = so.FindProperty("progressFill");
                if (progressProp != null)
                    progressProp.objectReferenceValue = progressFill.GetComponent<RectTransform>();
            }

            // ========== SETTINGS PANEL ==========
            Transform settingsPanelT = FindDeep(root, "SettingsPanel");
            if (settingsPanelT != null)
            {
                AssignGameObject(so, "settingsPanel", settingsPanelT.gameObject);

                AssignToggle(so, "toggleAddition", FindDeep(settingsPanelT, "ToggleAddition"));
                AssignToggle(so, "toggleSubtraction", FindDeep(settingsPanelT, "ToggleSubtraction"));
                AssignToggle(so, "toggleMultiplication", FindDeep(settingsPanelT, "ToggleMultiplication"));
                AssignToggle(so, "toggleDivision", FindDeep(settingsPanelT, "ToggleDivision"));
                AssignToggle(so, "toggleEasy", FindDeep(settingsPanelT, "ToggleEasy"));
                AssignToggle(so, "toggleNormal", FindDeep(settingsPanelT, "ToggleNormal"));
                AssignToggle(so, "toggleHard", FindDeep(settingsPanelT, "ToggleHard"));
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

                AssignTMPByFindDeep(so, "difficultyDescText", settingsPanelT, "DifficultyDescText");
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

            so.ApplyModifiedProperties();
            Debug.Log("[QuickMathUIBuilder] Referencias asignadas al Controller");
        }

        // ========== HELPERS ==========

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
            SetupText(textObj, text, 24, DARK_BG, FontStyles.Bold);

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
            TextMeshProUGUI labelTmp = SetupText(labelObj, label, 24, new Color(0.6f, 0.65f, 0.75f), FontStyles.Bold);
            labelTmp.alignment = TextAlignmentOptions.Left;

            // Value (right-aligned)
            GameObject valueObj = CreateElement(parent, valueName);
            SetupRectTransform(valueObj,
                new Vector2(0.5f, 1), new Vector2(0.95f, 1),
                new Vector2(0, yPos), new Vector2(0, 38));
            TextMeshProUGUI valueTmp = SetupText(valueObj, defaultValue, 28, valueColor, FontStyles.Bold);
            valueTmp.alignment = TextAlignmentOptions.Right;

            return valueTmp;
        }

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

        private static TextMeshProUGUI SetupText(GameObject obj, string text, float fontSize, Color color, FontStyles style)
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
