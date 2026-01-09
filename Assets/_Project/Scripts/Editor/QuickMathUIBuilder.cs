using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;
using DigitPark.Games;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para reconstruir la UI de QuickMath con diseño IMPACTANTE
    /// Ecuación gigante, botones 3D, combo system, partículas
    /// </summary>
    public class QuickMathUIBuilder : EditorWindow
    {
        // Colores del tema neón
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color MAGENTA_NEON = new Color(1f, 0f, 0.8f, 1f);
        private static readonly Color GREEN_NEON = new Color(0.3f, 1f, 0.5f, 1f);
        private static readonly Color ORANGE_NEON = new Color(1f, 0.6f, 0.2f, 1f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color DARK_BG = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color PANEL_BG = new Color(0.05f, 0.1f, 0.15f, 0.95f);
        private static readonly Color BUTTON_BG = new Color(0.08f, 0.12f, 0.18f, 1f);
        private static readonly Color ERROR_COLOR = new Color(1f, 0.3f, 0.3f, 1f);

        // Tamaños optimizados
        private const float ANSWER_BUTTON_SIZE = 140f;
        private const float EQUATION_FONT_SIZE = 72f;

        [MenuItem("DigitPark/Rebuild QuickMath UI")]
        public static void ShowWindow()
        {
            GetWindow<QuickMathUIBuilder>("QuickMath UI Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("QuickMath UI Builder", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "REDISEÑO IMPACTANTE de QuickMath:\n" +
                "- Ecuación GIGANTE al centro\n" +
                "- Botones 3D de 140px\n" +
                "- Sistema de Combo/Streak\n" +
                "- Partículas y efectos",
                MessageType.Info);

            GUILayout.Space(10);

            if (GUILayout.Button("Reconstruir QuickMath UI", GUILayout.Height(40)))
            {
                RebuildQuickMathUI();
            }
        }

        private static void RebuildQuickMathUI()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[QuickMathUIBuilder] No se encontró Canvas en la escena");
                return;
            }

            Transform canvasTransform = canvas.transform;

            // Limpiar elementos viejos
            CleanOldElements(canvasTransform);

            // Crear nueva estructura
            CreateQuickMathLayout(canvasTransform);

            // Asignar referencias
            AssignControllerReferences();

            Debug.Log("[QuickMathUIBuilder] QuickMath UI reconstruida con diseño IMPACTANTE!");
            EditorUtility.SetDirty(canvas.gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

        private static void CleanOldElements(Transform canvasTransform)
        {
            string[] keepElements = { "Main Camera", "EventSystem", "Background" };

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

                if (!shouldKeep)
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        private static void CreateQuickMathLayout(Transform canvasTransform)
        {
            // ========== SAFE AREA ==========
            GameObject safeArea = CreateElement(canvasTransform, "SafeArea");
            SetupRectTransform(safeArea, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SafeAreaHandler safeHandler = safeArea.AddComponent<SafeAreaHandler>();

            // ========== HEADER COMPACTO ==========
            CreateHeader(safeArea.transform);

            // ========== STATS BAR ==========
            CreateStatsBar(safeArea.transform);

            // ========== COMBO/STREAK DISPLAY ==========
            CreateComboDisplay(safeArea.transform);

            // ========== ECUACIÓN GIGANTE ==========
            CreateEquationPanel(safeArea.transform);

            // ========== BOTONES DE RESPUESTA 3D ==========
            CreateAnswerButtons(safeArea.transform);

            // ========== BARRA DE PROGRESO ==========
            CreateProgressBar(safeArea.transform);

            // ========== WIN PANEL ==========
            CreateWinPanel(safeArea.transform);

            // ========== PARTICLE EFFECTS ==========
            CreateParticleEffects(safeArea.transform);
        }

        private static void CreateHeader(Transform parent)
        {
            GameObject header = CreateElement(parent, "Header");
            SetupRectTransform(header,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -45), new Vector2(0, 90));

            Image headerBg = header.AddComponent<Image>();
            headerBg.color = new Color(0f, 0f, 0f, 0.3f);

            // Title con efecto glow
            GameObject title = CreateElement(header.transform, "TitleText");
            SetupRectTransform(title,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 0), new Vector2(400, 50));
            TextMeshProUGUI titleTmp = SetupText(title, "QUICK MATH", 36, CYAN_NEON, FontStyles.Bold);

            // Glow effect
            Outline titleGlow = title.AddComponent<Outline>();
            titleGlow.effectColor = new Color(0f, 1f, 1f, 0.5f);
            titleGlow.effectDistance = new Vector2(2, -2);
        }

        private static void CreateStatsBar(Transform parent)
        {
            GameObject statsBar = CreateElement(parent, "StatsBar");
            SetupRectTransform(statsBar,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -110), new Vector2(-40, 40));

            Image statsBg = statsBar.AddComponent<Image>();
            statsBg.color = PANEL_BG;

            HorizontalLayoutGroup statsLayout = statsBar.AddComponent<HorizontalLayoutGroup>();
            statsLayout.childAlignment = TextAnchor.MiddleCenter;
            statsLayout.spacing = 40;
            statsLayout.padding = new RectOffset(20, 20, 5, 5);
            statsLayout.childForceExpandWidth = false;
            statsLayout.childForceExpandHeight = true;

            // Timer
            GameObject timerContainer = CreateElement(statsBar.transform, "TimerContainer");
            AddLayoutElement(timerContainer, 110, 30);
            GameObject timerText = CreateElement(timerContainer.transform, "TimerText");
            SetupRectTransform(timerText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI timerTmp = SetupText(timerText, "00:00", 22, Color.white, FontStyles.Bold);
            timerTmp.alignment = TextAlignmentOptions.Center;

            // Round counter
            GameObject roundText = CreateElement(statsBar.transform, "RoundText");
            AddLayoutElement(roundText, 80, 30);
            SetupText(roundText, "1/10", 24, CYAN_NEON, FontStyles.Bold);

            // Errors con icono Image
            GameObject errorsContainer = CreateElement(statsBar.transform, "ErrorsContainer");
            AddLayoutElement(errorsContainer, 70, 30);

            HorizontalLayoutGroup errLayout = errorsContainer.AddComponent<HorizontalLayoutGroup>();
            errLayout.childAlignment = TextAnchor.MiddleCenter;
            errLayout.spacing = 6;

            GameObject errorsIcon = CreateElement(errorsContainer.transform, "ErrorIcon");
            AddLayoutElement(errorsIcon, 20, 20);
            Image errorIconImg = errorsIcon.AddComponent<Image>();
            errorIconImg.color = ERROR_COLOR;
            // Asignar sprite en Inspector

            GameObject errorsText = CreateElement(errorsContainer.transform, "ErrorsText");
            AddLayoutElement(errorsText, 35, 30);
            SetupText(errorsText, "0", 22, ERROR_COLOR, FontStyles.Bold);
        }

        private static void CreateComboDisplay(Transform parent)
        {
            // Combo container - prominente arriba de la ecuación
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
            comboCg.alpha = 0; // Oculto inicialmente

            // Streak icon (fuego)
            GameObject streakIcon = CreateElement(comboContainer.transform, "StreakIcon");
            SetupRectTransform(streakIcon,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(25, 0), new Vector2(40, 40));
            Image streakImg = streakIcon.AddComponent<Image>();
            streakImg.color = ORANGE_NEON;
            // Asignar sprite de fuego en Inspector

            // Combo text
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
            // Panel contenedor de la ecuación - GIGANTE
            GameObject equationPanel = CreateElement(parent, "EquationPanel");
            SetupRectTransform(equationPanel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 120), new Vector2(900, 250));

            Image panelBg = equationPanel.AddComponent<Image>();
            panelBg.color = new Color(0.04f, 0.08f, 0.15f, 0.95f);

            // Borde neón animable
            Outline panelOutline = equationPanel.AddComponent<Outline>();
            panelOutline.effectColor = CYAN_NEON;
            panelOutline.effectDistance = new Vector2(3, -3);

            // Segundo borde para efecto glow
            Shadow panelShadow = equationPanel.AddComponent<Shadow>();
            panelShadow.effectColor = new Color(0f, 1f, 1f, 0.3f);
            panelShadow.effectDistance = new Vector2(0, -5);

            // Container para los números de la ecuación
            GameObject equationContainer = CreateElement(equationPanel.transform, "EquationContainer");
            SetupRectTransform(equationContainer, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            HorizontalLayoutGroup eqLayout = equationContainer.AddComponent<HorizontalLayoutGroup>();
            eqLayout.childAlignment = TextAnchor.MiddleCenter;
            eqLayout.spacing = 20;
            eqLayout.childForceExpandWidth = false;
            eqLayout.childForceExpandHeight = true;

            // Número A (izquierda)
            GameObject numberA = CreateElement(equationContainer.transform, "NumberA");
            AddLayoutElement(numberA, 150, 200);
            TextMeshProUGUI numATmp = SetupText(numberA, "5", EQUATION_FONT_SIZE, Color.white, FontStyles.Bold);
            numATmp.alignment = TextAlignmentOptions.Center;

            // Operador
            GameObject operatorText = CreateElement(equationContainer.transform, "OperatorText");
            AddLayoutElement(operatorText, 80, 200);
            TextMeshProUGUI opTmp = SetupText(operatorText, "+", EQUATION_FONT_SIZE, CYAN_NEON, FontStyles.Bold);
            opTmp.alignment = TextAlignmentOptions.Center;

            // Número B (derecha)
            GameObject numberB = CreateElement(equationContainer.transform, "NumberB");
            AddLayoutElement(numberB, 150, 200);
            TextMeshProUGUI numBTmp = SetupText(numberB, "7", EQUATION_FONT_SIZE, Color.white, FontStyles.Bold);
            numBTmp.alignment = TextAlignmentOptions.Center;

            // Igual
            GameObject equalsText = CreateElement(equationContainer.transform, "EqualsText");
            AddLayoutElement(equalsText, 80, 200);
            TextMeshProUGUI eqTmp = SetupText(equalsText, "=", EQUATION_FONT_SIZE, CYAN_NEON, FontStyles.Bold);
            eqTmp.alignment = TextAlignmentOptions.Center;

            // Signo de interrogación (pulsa)
            GameObject questionMark = CreateElement(equationContainer.transform, "QuestionMark");
            AddLayoutElement(questionMark, 120, 200);
            TextMeshProUGUI qTmp = SetupText(questionMark, "?", EQUATION_FONT_SIZE + 10, GOLD, FontStyles.Bold);
            qTmp.alignment = TextAlignmentOptions.Center;

            // También crear ProblemText oculto para compatibilidad con controller
            GameObject problemText = CreateElement(equationPanel.transform, "ProblemText");
            SetupRectTransform(problemText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI probTmp = SetupText(problemText, "5 + 7 = ?", 1, Color.clear, FontStyles.Normal);
            probTmp.enabled = false; // Solo para datos, no visible
        }

        private static void CreateAnswerButtons(Transform parent)
        {
            // Container para botones de respuesta
            GameObject answersContainer = CreateElement(parent, "AnswersContainer");
            SetupRectTransform(answersContainer,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -120), new Vector2(600, ANSWER_BUTTON_SIZE + 20));

            HorizontalLayoutGroup answersLayout = answersContainer.AddComponent<HorizontalLayoutGroup>();
            answersLayout.childAlignment = TextAnchor.MiddleCenter;
            answersLayout.spacing = 40;
            answersLayout.childForceExpandWidth = false;
            answersLayout.childForceExpandHeight = false;

            // Crear 3 botones de respuesta con efecto 3D
            for (int i = 0; i < 3; i++)
            {
                CreateAnswerButton3D(answersContainer.transform, i);
            }
        }

        private static void CreateAnswerButton3D(Transform parent, int index)
        {
            // Cell container
            GameObject cell = CreateElement(parent, $"AnswerButton_{index}");
            AddLayoutElement(cell, ANSWER_BUTTON_SIZE, ANSWER_BUTTON_SIZE);

            Image cellBase = cell.AddComponent<Image>();
            cellBase.color = Color.clear;

            // Shadow
            GameObject shadow = CreateElement(cell.transform, "Shadow");
            SetupRectTransform(shadow,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(3, -8), new Vector2(ANSWER_BUTTON_SIZE - 8, ANSWER_BUTTON_SIZE - 8));
            Image shadowImg = shadow.AddComponent<Image>();
            shadowImg.color = new Color(0f, 0f, 0f, 0.5f);

            // Side (depth)
            GameObject side = CreateElement(cell.transform, "Side");
            SetupRectTransform(side,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 0), new Vector2(ANSWER_BUTTON_SIZE - 8, 10));
            Image sideImg = side.AddComponent<Image>();
            sideImg.color = new Color(0f, 0.3f, 0.35f, 1f);

            // Face (top)
            GameObject face = CreateElement(cell.transform, "Face");
            SetupRectTransform(face,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 4), new Vector2(ANSWER_BUTTON_SIZE - 8, ANSWER_BUTTON_SIZE - 8));
            Image faceImg = face.AddComponent<Image>();
            faceImg.color = BUTTON_BG;

            Outline faceOutline = face.AddComponent<Outline>();
            faceOutline.effectColor = CYAN_NEON;
            faceOutline.effectDistance = new Vector2(2, -2);

            // Answer text - GRANDE
            GameObject answerText = CreateElement(face.transform, $"AnswerText_{index}");
            SetupRectTransform(answerText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI answerTmp = SetupText(answerText, "12", 52, CYAN_NEON, FontStyles.Bold);
            answerTmp.alignment = TextAlignmentOptions.Center;

            // Button component
            Button button = cell.AddComponent<Button>();
            button.targetGraphic = faceImg;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.2f, 1f);
            colors.pressedColor = new Color(0.8f, 0.9f, 0.85f, 1f);
            colors.fadeDuration = 0.05f;
            button.colors = colors;

            // Add QuickMathCell3D component
            QuickMathCell3D cell3D = cell.AddComponent<QuickMathCell3D>();

            // Assign references via SerializedObject
            SerializedObject so = new SerializedObject(cell3D);
            so.FindProperty("buttonFace").objectReferenceValue = face.GetComponent<RectTransform>();
            so.FindProperty("shadowImage").objectReferenceValue = shadowImg;
            so.FindProperty("sideImage").objectReferenceValue = sideImg;
            so.FindProperty("faceImage").objectReferenceValue = faceImg;
            so.FindProperty("glowOutline").objectReferenceValue = faceOutline;
            so.FindProperty("answerText").objectReferenceValue = answerTmp;
            so.ApplyModifiedProperties();
        }

        private static void CreateProgressBar(Transform parent)
        {
            // Container de progreso
            GameObject progressContainer = CreateElement(parent, "ProgressContainer");
            SetupRectTransform(progressContainer,
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 100), new Vector2(-80, 50));

            // Round indicator text
            GameObject roundIndicator = CreateElement(progressContainer.transform, "RoundIndicator");
            SetupRectTransform(roundIndicator,
                new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(-50, 0), new Vector2(80, 30));
            SetupText(roundIndicator, "8/15", 20, Color.white, FontStyles.Bold);

            // Progress bar background
            GameObject progressBar = CreateElement(progressContainer.transform, "ProgressBar");
            SetupRectTransform(progressBar,
                new Vector2(0, 0.5f), new Vector2(1, 0.5f),
                new Vector2(0, 0), new Vector2(-100, 16));

            Image progressBg = progressBar.AddComponent<Image>();
            progressBg.color = new Color(0f, 0.2f, 0.25f, 0.8f);

            Outline progressOutline = progressBar.AddComponent<Outline>();
            progressOutline.effectColor = CYAN_NEON;
            progressOutline.effectDistance = new Vector2(1, -1);

            // Progress fill
            GameObject progressFill = CreateElement(progressBar.transform, "ProgressFill");
            SetupRectTransform(progressFill,
                new Vector2(0, 0), new Vector2(0.5f, 1), // 50% como ejemplo
                Vector2.zero, Vector2.zero);

            Image fillImg = progressFill.AddComponent<Image>();
            fillImg.color = CYAN_NEON;

            // Glow en el fill
            Shadow fillGlow = progressFill.AddComponent<Shadow>();
            fillGlow.effectColor = new Color(0f, 1f, 1f, 0.5f);
            fillGlow.effectDistance = new Vector2(0, -2);
        }

        private static void CreateWinPanel(Transform parent)
        {
            GameObject winPanel = CreateElement(parent, "WinPanel");
            SetupRectTransform(winPanel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Image overlayImg = winPanel.AddComponent<Image>();
            overlayImg.color = new Color(0, 0, 0, 0.9f);

            CanvasGroup winCg = winPanel.AddComponent<CanvasGroup>();
            winCg.alpha = 0;

            // Content
            GameObject content = CreateElement(winPanel.transform, "Content");
            SetupRectTransform(content,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(550, 420));

            Image contentBg = content.AddComponent<Image>();
            contentBg.color = PANEL_BG;

            Outline contentOutline = content.AddComponent<Outline>();
            contentOutline.effectColor = GREEN_NEON;
            contentOutline.effectDistance = new Vector2(3, -3);

            // Title
            GameObject winTitle = CreateElement(content.transform, "WinTitle");
            SetupRectTransform(winTitle,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -45), new Vector2(0, 70));
            SetupText(winTitle, "¡COMPLETADO!", 44, GREEN_NEON, FontStyles.Bold);

            // Stats
            GameObject statsText = CreateElement(content.transform, "StatsText");
            SetupRectTransform(statsText,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 20), new Vector2(400, 130));
            TextMeshProUGUI statsTmp = SetupText(statsText, "Tiempo: 00:00\nErrores: 0\nMax Streak: x1", 26, Color.white, FontStyles.Normal);
            statsTmp.lineSpacing = 15;

            // Play again button
            GameObject playAgainBtn = CreateElement(content.transform, "PlayAgainButton");
            SetupRectTransform(playAgainBtn,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 60), new Vector2(300, 65));

            Image playBtnImg = playAgainBtn.AddComponent<Image>();
            playBtnImg.color = GREEN_NEON;

            Outline btnOutline = playAgainBtn.AddComponent<Outline>();
            btnOutline.effectColor = new Color(0.1f, 0.4f, 0.2f, 1f);
            btnOutline.effectDistance = new Vector2(2, -2);

            Button playBtn = playAgainBtn.AddComponent<Button>();
            playBtn.targetGraphic = playBtnImg;

            GameObject playBtnText = CreateElement(playAgainBtn.transform, "PlayAgainButtonText");
            SetupRectTransform(playBtnText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetupText(playBtnText, "JUGAR DE NUEVO", 28, DARK_BG, FontStyles.Bold);

            winPanel.SetActive(false);
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
                Debug.LogWarning("[QuickMathUIBuilder] No se encontró QuickMathController en la escena");
                return;
            }

            SerializedObject so = new SerializedObject(controller);

            // Problem text (hidden, for data)
            AssignTMPReference(so, "problemText", "ProblemText");

            // Equation parts
            AssignTMPReference(so, "numberAText", "NumberA");
            AssignTMPReference(so, "numberBText", "NumberB");
            AssignTMPReference(so, "operatorText", "OperatorText");
            AssignTMPReference(so, "questionMarkText", "QuestionMark");

            // Answer buttons
            GameObject answersContainer = GameObject.Find("AnswersContainer");
            if (answersContainer != null)
            {
                SerializedProperty answerButtonsProp = so.FindProperty("answerButtons");
                SerializedProperty answerTextsProp = so.FindProperty("answerTexts");

                if (answerButtonsProp != null) answerButtonsProp.arraySize = 3;
                if (answerTextsProp != null) answerTextsProp.arraySize = 3;

                for (int i = 0; i < 3; i++)
                {
                    Transform btn = answersContainer.transform.Find($"AnswerButton_{i}");
                    if (btn != null)
                    {
                        if (answerButtonsProp != null)
                            answerButtonsProp.GetArrayElementAtIndex(i).objectReferenceValue = btn.GetComponent<Button>();

                        Transform face = btn.Find("Face");
                        if (face != null && answerTextsProp != null)
                        {
                            TextMeshProUGUI txt = face.GetComponentInChildren<TextMeshProUGUI>();
                            if (txt != null)
                                answerTextsProp.GetArrayElementAtIndex(i).objectReferenceValue = txt;
                        }
                    }
                }
            }

            // UI elements
            AssignTMPReference(so, "timerText", "TimerText");
            AssignTMPReference(so, "roundText", "RoundText");
            AssignTMPReference(so, "errorsText", "ErrorsText");
            AssignTMPReference(so, "comboText", "ComboText");
            AssignTMPReference(so, "statsText", "StatsText");
            AssignTMPReference(so, "roundIndicatorText", "RoundIndicator");

            // Win panel
            GameObject winPanel = GameObject.Find("WinPanel");
            if (winPanel != null)
            {
                SerializedProperty winPanelProp = so.FindProperty("winPanel");
                if (winPanelProp != null)
                    winPanelProp.objectReferenceValue = winPanel;

                SerializedProperty winCgProp = so.FindProperty("winPanelCanvasGroup");
                if (winCgProp != null)
                    winCgProp.objectReferenceValue = winPanel.GetComponent<CanvasGroup>();
            }

            // Combo container
            GameObject comboContainer = GameObject.Find("ComboContainer");
            if (comboContainer != null)
            {
                SerializedProperty comboCgProp = so.FindProperty("comboCanvasGroup");
                if (comboCgProp != null)
                    comboCgProp.objectReferenceValue = comboContainer.GetComponent<CanvasGroup>();
            }

            // Progress fill
            GameObject progressFill = GameObject.Find("ProgressFill");
            if (progressFill != null)
            {
                SerializedProperty progressProp = so.FindProperty("progressFill");
                if (progressProp != null)
                    progressProp.objectReferenceValue = progressFill.GetComponent<RectTransform>();
            }

            // Sparkle effect
            GameObject particleEffects = GameObject.Find("ParticleEffects");
            if (particleEffects != null)
            {
                SerializedProperty sparkleProp = so.FindProperty("sparkleEffect");
                if (sparkleProp != null)
                    sparkleProp.objectReferenceValue = particleEffects.GetComponent<UISparkleEffect>();
            }

            // Equation panel for animations
            GameObject equationPanel = GameObject.Find("EquationPanel");
            if (equationPanel != null)
            {
                SerializedProperty eqPanelProp = so.FindProperty("equationPanel");
                if (eqPanelProp != null)
                    eqPanelProp.objectReferenceValue = equationPanel.GetComponent<RectTransform>();
            }

            so.ApplyModifiedProperties();
            Debug.Log("[QuickMathUIBuilder] Referencias asignadas al Controller");
        }

        private static void AssignTMPReference(SerializedObject so, string propertyName, string objectName)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop != null)
            {
                GameObject obj = GameObject.Find(objectName);
                if (obj != null)
                {
                    prop.objectReferenceValue = obj.GetComponent<TextMeshProUGUI>();
                }
            }
        }

        // ========== UTILITIES ==========

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
