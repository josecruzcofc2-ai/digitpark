using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;
using DigitPark.Editor.AutoAssigners;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para reconstruir la UI de DigitRush con diseño profesional neón
    /// Optimizado para formato portrait 9:16 (1080x1920)
    /// </summary>
    public class DigitRushUIBuilder : EditorWindow
    {
        // Colores del tema neón
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color MAGENTA_NEON = new Color(1f, 0f, 0.8f, 1f);
        private static readonly Color GREEN_NEON = new Color(0.3f, 1f, 0.5f, 1f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color DARK_BG = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color PANEL_BG = new Color(0.05f, 0.1f, 0.15f, 0.95f);
        private static readonly Color CELL_BG = new Color(0.08f, 0.12f, 0.2f, 1f);
        private static readonly Color CELL_PRESSED = new Color(0.2f, 0.7f, 0.3f, 0.5f);

        [MenuItem("DigitPark/UI Builders/Games/DigitRush", false, 110)]
        public static void ShowWindow()
        {
            GetWindow<DigitRushUIBuilder>("DigitRush UI Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("DigitRush UI Builder", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Este script reconstruirá la UI de DigitRush.\n" +
                "Asegúrate de tener la escena DigitRush abierta.\n" +
                "Diseño optimizado para portrait 9:16 (1080x1920).",
                MessageType.Info);

            GUILayout.Space(10);

            if (GUILayout.Button("Reconstruir DigitRush UI", GUILayout.Height(40)))
            {
                RebuildDigitRushUI();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Solo Actualizar Estilos", GUILayout.Height(30)))
            {
                UpdateStyles();
            }

            GUILayout.Space(15);
            GUI.backgroundColor = new Color(0.5f, 0.8f, 1f);
            if (GUILayout.Button("Auto-Asignar Referencias", GUILayout.Height(30)))
            {
                DigitRushReferenceAssigner.RunAutoAssign();
            }
            GUI.backgroundColor = Color.white;
        }

        private static void RebuildDigitRushUI()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[DigitRushUIBuilder] No se encontró Canvas en la escena");
                return;
            }

            Transform canvasTransform = canvas.transform;

            // Ensure Canvas has a GraphicRaycaster (required for UI clicks)
            if (canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
            {
                canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                Debug.Log("[DigitRushUIBuilder] GraphicRaycaster añadido al Canvas");
            }

            // Ensure EventSystem exists in the scene
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Debug.Log("[DigitRushUIBuilder] EventSystem creado en la escena");
            }

            // Limpiar elementos viejos (mantener Camera y EventSystem)
            CleanOldElements(canvasTransform);

            // Crear nueva estructura
            CreateDigitRushLayout(canvasTransform);

            // Intentar asignar referencias al GameManager
            AssignDigitRushControllerReferences();

            Debug.Log("[DigitRushUIBuilder] DigitRush UI reconstruida exitosamente!");
            EditorUtility.SetDirty(canvas.gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

        private static void UpdateStyles()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            // Update existing elements with new styles
            UpdateAllTextStyles(canvas.transform);
            UpdateCellStyles(canvas.transform);

            Debug.Log("[DigitRushUIBuilder] Estilos actualizados!");
            EditorUtility.SetDirty(canvas.gameObject);
        }

        private static void CleanOldElements(Transform canvasTransform)
        {
            // Only keep objects the builder does NOT create.
            // Everything else (SafeArea, GridContainer, Cell_, Background, etc.)
            // gets destroyed so the builder can recreate them fresh without duplicates.
            string[] keepElements = {
                "Main Camera", "EventSystem", "DigitRushController",
                "Directional Light", "SceneTransition"
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

                // Never destroy objects with Animator or Animation components
                if (!shouldKeep && (child.GetComponent<Animator>() != null || child.GetComponent<Animation>() != null))
                    shouldKeep = true;

                if (!shouldKeep)
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        private static void CreateDigitRushLayout(Transform canvasTransform)
        {
            // ========== BACKGROUND ==========
            GameObject background = CreateElement(canvasTransform, "Background");
            SetupRectTransform(background, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = DARK_BG;
            background.transform.SetAsFirstSibling();

            // ========== SAFE AREA CONTAINER ==========
            GameObject safeArea = CreateElement(canvasTransform, "SafeArea");
            SetupRectTransform(safeArea, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Add safe area handler (for notch devices)
            SafeAreaHandler safeHandler = safeArea.AddComponent<SafeAreaHandler>();

            // ========== HEADER ==========
            CreateHeader(safeArea.transform);

            // ========== TIMER SECTION ==========
            CreateTimerSection(safeArea.transform);

            // ========== GAME PANEL (GRID) ==========
            CreateGamePanel(safeArea.transform);

            // ========== BEST TIME DISPLAY ==========
            CreateBestTimeSection(safeArea.transform);

            // ========== ACTION BUTTONS ==========
            CreateActionButtons(safeArea.transform);

            // ========== RESULT PANEL (Practice) ==========
            CreateResultPanel(safeArea.transform);

            // ========== REAL MONEY PANELS (Cash Battle) ==========
            CreateRealMoneyPanels(safeArea.transform);

            // ========== COUNTDOWN PANEL ==========
            CreateCountdownPanel(safeArea.transform);

            // ========== COMBO TEXT ==========
            CreateComboText(safeArea.transform);

            // ========== PARTICLE EFFECTS ==========
            CreateParticleEffects(safeArea.transform);

            // Premium Banner removed - will be added in v3+
        }

        private static void CreateHeader(Transform parent)
        {
            GameObject header = CreateElement(parent, "Header");
            SetupRectTransform(header,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -60), new Vector2(0, 120));

            Image headerBg = header.AddComponent<Image>();
            headerBg.color = new Color(0f, 0f, 0f, 0.3f);

            // Back Button placeholder (user will add their own)
            // Note: Back button removed - user will add their own prefab

            // Title
            GameObject title = CreateElement(header.transform, "TitleText");
            SetupRectTransform(title,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 0), new Vector2(600, 80));
            SetupText(title, "DIGIT RUSH", 52, CYAN_NEON, FontStyles.Bold);

            // Subtitle
            GameObject subtitle = CreateElement(header.transform, "SubtitleText");
            SetupRectTransform(subtitle,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 15), new Vector2(400, 30));
            SetupText(subtitle, "Toca 1-9 en orden", 20, Color.white, FontStyles.Italic);
        }

        private static void CreateTimerSection(Transform parent)
        {
            GameObject timerSection = CreateElement(parent, "TimerSection");
            SetupRectTransform(timerSection,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -200), new Vector2(500, 150));

            // Timer background panel
            Image timerBg = timerSection.AddComponent<Image>();
            timerBg.color = PANEL_BG;

            // Add rounded corners outline
            Outline outline = timerSection.AddComponent<Outline>();
            outline.effectColor = CYAN_NEON;
            outline.effectDistance = new Vector2(2, -2);

            // Timer icon - Using Image component (drag your clock icon here)
            GameObject timerIcon = CreateElement(timerSection.transform, "TimerIcon");
            SetupRectTransform(timerIcon,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -18), new Vector2(40, 40));
            Image timerIconImg = timerIcon.AddComponent<Image>();
            timerIconImg.color = CYAN_NEON;
            // Note: Assign clock/stopwatch sprite in Inspector

            // Timer text (large)
            GameObject timerText = CreateElement(timerSection.transform, "TimerText");
            SetupRectTransform(timerText,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -10), new Vector2(400, 80));
            SetupText(timerText, "0.000s", 64, Color.white, FontStyles.Bold);
        }

        private static void CreateGamePanel(Transform parent)
        {
            GameObject gamePanel = CreateElement(parent, "GamePanel");
            SetupRectTransform(gamePanel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 50), new Vector2(900, 900));

            // Background for game panel
            Image panelBg = gamePanel.AddComponent<Image>();
            panelBg.color = new Color(0.03f, 0.06f, 0.12f, 0.8f);

            // Add glow outline
            Outline panelOutline = gamePanel.AddComponent<Outline>();
            panelOutline.effectColor = new Color(0f, 0.8f, 0.8f, 0.4f);
            panelOutline.effectDistance = new Vector2(3, -3);

            // Grid Container
            GameObject gridContainer = CreateElement(gamePanel.transform, "GridContainer");
            SetupRectTransform(gridContainer,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(840, 840));

            // Add GridLayoutGroup
            GridLayoutGroup grid = gridContainer.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(260, 260);
            grid.spacing = new Vector2(20, 20);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.padding = new RectOffset(10, 10, 10, 10);

            // Create 9 cells
            for (int i = 1; i <= 9; i++)
            {
                CreateGridCell(gridContainer.transform, i);
            }
        }

        private static void CreateGridCell(Transform parent, int cellNumber)
        {
            // ========== 3D BUTTON STRUCTURE ==========
            // Cell container (transparent, just for layout)
            GameObject cell = CreateElement(parent, $"Cell_{cellNumber}");

            // Cell base is transparent - children create the 3D effect
            Image cellBase = cell.AddComponent<Image>();
            cellBase.color = Color.clear;

            // 1. SHADOW (bottom layer - darker, offset down)
            GameObject shadow = CreateElement(cell.transform, "Shadow");
            SetupRectTransform(shadow,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(4, -12), new Vector2(250, 250));
            Image shadowImg = shadow.AddComponent<Image>();
            shadowImg.color = new Color(0f, 0f, 0f, 0.4f);

            // 2. SIDE (middle layer - the "depth" of the button)
            GameObject side = CreateElement(cell.transform, "Side");
            SetupRectTransform(side,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 0), new Vector2(250, 12));
            Image sideImg = side.AddComponent<Image>();
            sideImg.color = new Color(0.04f, 0.06f, 0.12f, 1f);

            // 3. FACE (top layer - the visible button surface)
            GameObject face = CreateElement(cell.transform, "Face");
            SetupRectTransform(face,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 4), new Vector2(250, 250));
            Image faceImg = face.AddComponent<Image>();
            faceImg.color = CELL_BG;

            // Neon outline on face
            Outline faceOutline = face.AddComponent<Outline>();
            faceOutline.effectColor = CYAN_NEON;
            faceOutline.effectDistance = new Vector2(2, -2);

            // Inner glow effect
            Shadow innerGlow = face.AddComponent<Shadow>();
            innerGlow.effectColor = new Color(0f, 0.8f, 0.8f, 0.3f);
            innerGlow.effectDistance = new Vector2(0, 0);

            // 4. NUMBER TEXT (on top of face)
            GameObject numberText = CreateElement(face.transform, "Text (TMP)");
            SetupRectTransform(numberText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI tmp = SetupText(numberText, cellNumber.ToString(), 80, Color.white, FontStyles.Bold);
            tmp.alignment = TextAlignmentOptions.Center;

            // Button component on the cell container
            Button button = cell.AddComponent<Button>();
            button.targetGraphic = faceImg;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 1f, 1f, 1f);
            colors.pressedColor = new Color(0.7f, 0.9f, 0.8f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            colors.fadeDuration = 0.05f;
            button.colors = colors;

            // Add 3D Button component for animations
            DigitPark.UI.Cell3DButton cell3D = cell.AddComponent<DigitPark.UI.Cell3DButton>();

            // Assign references via SerializedObject
            SerializedObject so = new SerializedObject(cell3D);
            so.FindProperty("buttonFace").objectReferenceValue = face.GetComponent<RectTransform>();
            so.FindProperty("shadowImage").objectReferenceValue = shadowImg;
            so.FindProperty("sideImage").objectReferenceValue = sideImg;
            so.FindProperty("faceColor").colorValue = CELL_BG;
            so.FindProperty("sideColor").colorValue = new Color(0.04f, 0.06f, 0.12f, 1f);
            so.FindProperty("shadowColor").colorValue = new Color(0f, 0f, 0f, 0.4f);
            so.FindProperty("glowColor").colorValue = CYAN_NEON;
            so.ApplyModifiedProperties();
        }

        private static void CreateBestTimeSection(Transform parent)
        {
            GameObject bestTimeSection = CreateElement(parent, "BestTimeSection");
            SetupRectTransform(bestTimeSection,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -530), new Vector2(500, 60));

            // Background
            Image bg = bestTimeSection.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.08f, 0.15f, 0.8f);

            // Outline
            Outline outline = bestTimeSection.AddComponent<Outline>();
            outline.effectColor = GOLD;
            outline.effectDistance = new Vector2(1, -1);

            // Trophy icon - Using Image component (drag your icon here)
            GameObject trophyIcon = CreateElement(bestTimeSection.transform, "TrophyIcon");
            SetupRectTransform(trophyIcon,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(25, 0), new Vector2(36, 36));
            Image trophyImg = trophyIcon.AddComponent<Image>();
            trophyImg.color = GOLD;
            // Note: Assign trophy sprite in Inspector or via code

            // Best time text
            GameObject bestTimeText = CreateElement(bestTimeSection.transform, "BestTimeText");
            SetupRectTransform(bestTimeText,
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(70, 0), new Vector2(-20, 0));
            TextMeshProUGUI bestTmp = SetupText(bestTimeText, "Mejor: --", 28, GOLD, FontStyles.Bold);
            bestTmp.alignment = TextAlignmentOptions.Left;
        }

        private static void CreateActionButtons(Transform parent)
        {
            // Note: PlayAgainButton removed - game ends and shows WinPanel which handles navigation
            // Note: Back button removed - user will add their own prefab

            // Empty container kept for future action buttons if needed
            GameObject actionButtonsContainer = CreateElement(parent, "ActionButtonsContainer");
            SetupRectTransform(actionButtonsContainer,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 150), new Vector2(600, 120));
        }

        private static void CreateResultPanel(Transform parent)
        {
            GameObject resultPanel = CreateElement(parent, "ResultPanel");
            SetupRectTransform(resultPanel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Full-screen overlay
            Image overlay = resultPanel.AddComponent<Image>();
            overlay.color = new Color(0, 0, 0, 0.85f);
            overlay.raycastTarget = true;

            CanvasGroup cg = resultPanel.AddComponent<CanvasGroup>();
            cg.alpha = 0;

            // Content container
            GameObject content = CreateElement(resultPanel.transform, "Content");
            SetupRectTransform(content,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(700, 500));

            Image contentBg = content.AddComponent<Image>();
            contentBg.color = new Color(0.05f, 0.1f, 0.15f, 0.95f);

            Outline contentOutline = content.AddComponent<Outline>();
            contentOutline.effectColor = GREEN_NEON;
            contentOutline.effectDistance = new Vector2(3, -3);

            // Title
            GameObject titleText = CreateElement(content.transform, "ResultTitleText");
            SetupRectTransform(titleText,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -50), new Vector2(0, 60));
            SetupText(titleText, "COMPLETED!", 46, GREEN_NEON, FontStyles.Bold);

            // Time display
            GameObject timeText = CreateElement(content.transform, "ResultTimeText");
            SetupRectTransform(timeText,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -130), new Vector2(0, 60));
            SetupText(timeText, "Time: 0.000s", 38, CYAN_NEON, FontStyles.Bold);

            // Message
            GameObject messageText = CreateElement(content.transform, "ResultMessageText");
            SetupRectTransform(messageText,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -10), new Vector2(600, 120));
            TextMeshProUGUI msgTmp = SetupText(messageText, "Great job!", 32, Color.white, FontStyles.Normal);
            msgTmp.enableWordWrapping = true;

            // Buttons container
            GameObject buttonsContainer = CreateElement(content.transform, "ButtonsContainer");
            SetupRectTransform(buttonsContainer,
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 60), new Vector2(-40, 100));

            HorizontalLayoutGroup btnLayout = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
            btnLayout.childAlignment = TextAnchor.MiddleCenter;
            btnLayout.spacing = 30;
            btnLayout.childForceExpandWidth = false;
            btnLayout.childControlWidth = false;

            // Play Again button (wider for localized text like "JUGAR DE NUEVO", "JOGAR NOVAMENTE")
            CreateActionButton(buttonsContainer.transform, "ResultPlayAgainButton", "PLAY AGAIN", CYAN_NEON, 320, 80);

            // Exit button (wider for localized text like "QUITTER", "BEENDEN")
            CreateActionButton(buttonsContainer.transform, "ResultExitButton", "EXIT", new Color(0.6f, 0.6f, 0.6f), 220, 80);

            // Hide by default
            resultPanel.SetActive(false);
        }

        private static void CreateActionButton(Transform parent, string name, string text, Color color, float width, float height)
        {
            GameObject btn = CreateElement(parent, name);
            LayoutElement layout = btn.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = height;

            Image faceImg = btn.AddComponent<Image>();
            faceImg.color = color;

            GameObject textObj = CreateElement(btn.transform, "Text");
            SetupRectTransform(textObj, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-10, -6));
            SetupText(textObj, text, 24, DARK_BG, FontStyles.Bold);

            Button button = btn.AddComponent<Button>();
            button.targetGraphic = faceImg;
        }

        private static void CreateRealMoneyPanels(Transform parent)
        {
            WinPanelInlineBuilder.CreateRealMoneyPanels(parent);
        }

        private static void CreateCountdownPanel(Transform parent)
        {
            GameObject countdownPanel = CreateElement(parent, "CountdownPanel");
            SetupRectTransform(countdownPanel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Semi-transparent background overlay
            GameObject overlay = CreateElement(countdownPanel.transform, "Overlay");
            SetupRectTransform(overlay, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image overlayImg = overlay.AddComponent<Image>();
            overlayImg.color = new Color(0f, 0f, 0f, 0.6f);

            // Countdown number container (centered)
            GameObject numberContainer = CreateElement(countdownPanel.transform, "NumberContainer");
            SetupRectTransform(numberContainer,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(400, 400));

            // Countdown text (large number)
            GameObject countdownText = CreateElement(numberContainer.transform, "CountdownText");
            SetupRectTransform(countdownText,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(350, 300));
            TextMeshProUGUI countTmp = SetupText(countdownText, "3", 180, CYAN_NEON, FontStyles.Bold);
            countTmp.alignment = TextAlignmentOptions.Center;
            countTmp.enableWordWrapping = false;

            // Add glow outline to number
            Outline numOutline = countdownText.AddComponent<Outline>();
            numOutline.effectColor = new Color(0f, 0.5f, 0.5f, 0.8f);
            numOutline.effectDistance = new Vector2(4, -4);

            // Add CountdownUI component
            DigitPark.UI.CountdownUI countdownUI = countdownPanel.AddComponent<DigitPark.UI.CountdownUI>();

            // Assign references via SerializedObject
            SerializedObject so = new SerializedObject(countdownUI);
            so.FindProperty("countdownPanel").objectReferenceValue = countdownPanel;
            so.FindProperty("countdownText").objectReferenceValue = countTmp;
            so.FindProperty("backgroundOverlay").objectReferenceValue = overlayImg;
            so.FindProperty("numberColor").colorValue = CYAN_NEON;
            so.FindProperty("goColor").colorValue = GREEN_NEON;
            so.ApplyModifiedProperties();

            // Hide by default
            countdownPanel.SetActive(false);
        }

        // Premium Banner removed - will be added in v3+

        private static void CreateComboText(Transform parent)
        {
            // Combo text positioned below the timer section
            GameObject comboContainer = CreateElement(parent, "ComboContainer");
            SetupRectTransform(comboContainer,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -290), new Vector2(300, 60));

            // Background for combo
            Image comboBg = comboContainer.AddComponent<Image>();
            comboBg.color = new Color(0.1f, 0.08f, 0.15f, 0.8f);

            // Outline that changes with combo level
            Outline comboOutline = comboContainer.AddComponent<Outline>();
            comboOutline.effectColor = GOLD;
            comboOutline.effectDistance = new Vector2(1, -1);

            // CanvasGroup for fade in/out
            CanvasGroup comboCg = comboContainer.AddComponent<CanvasGroup>();
            comboCg.alpha = 0; // Hidden initially

            // Combo text
            GameObject comboText = CreateElement(comboContainer.transform, "ComboText");
            SetupRectTransform(comboText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI comboTmp = SetupText(comboText, "COMBO x1", 32, GOLD, FontStyles.Bold);
            comboTmp.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateParticleEffects(Transform parent)
        {
            // Full-screen container for particle effects
            GameObject particleContainer = CreateElement(parent, "ParticleEffects");
            SetupRectTransform(particleContainer, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Add UISparkleEffect component
            DigitPark.UI.UISparkleEffect sparkleEffect = particleContainer.AddComponent<DigitPark.UI.UISparkleEffect>();

            // Make sure it's at the top of hierarchy (renders on top)
            particleContainer.transform.SetAsLastSibling();
        }

        private static void AssignDigitRushControllerReferences()
        {
            var gameManager = FindFirstObjectByType<DigitPark.Managers.DigitRushController>();
            if (gameManager == null)
            {
                Debug.LogWarning("[DigitRushUIBuilder] No se encontró DigitRushController en la escena");
                return;
            }

            Canvas canvas = FindFirstObjectByType<Canvas>();
            Transform root = canvas != null ? canvas.transform : null;

            SerializedObject serializedManager = new SerializedObject(gameManager);

            // Find and assign grid buttons (search including inactive)
            Transform gridContainer = FindDeep(root, "GridContainer");
            if (gridContainer != null)
            {
                SerializedProperty gridButtonsProp = serializedManager.FindProperty("gridButtons");
                if (gridButtonsProp != null)
                {
                    gridButtonsProp.arraySize = 9;
                    for (int i = 0; i < 9; i++)
                    {
                        Transform cell = gridContainer.Find($"Cell_{i + 1}");
                        if (cell != null)
                        {
                            Button btn = cell.GetComponent<Button>();
                            gridButtonsProp.GetArrayElementAtIndex(i).objectReferenceValue = btn;
                        }
                    }
                }
            }

            // Timer Text
            AssignTMPReference(serializedManager, root, "timerText", "TimerText");

            // Best Time Text
            AssignTMPReference(serializedManager, root, "bestTimeText", "BestTimeText");

            // Result Panel (Practice) - starts inactive!
            Transform resultPanelT = FindDeep(root, "ResultPanel");
            if (resultPanelT != null)
            {
                SerializedProperty rpProp = serializedManager.FindProperty("resultPanel");
                if (rpProp != null) rpProp.objectReferenceValue = resultPanelT.gameObject;

                SerializedProperty cgProp = serializedManager.FindProperty("resultPanelCanvasGroup");
                if (cgProp != null) cgProp.objectReferenceValue = resultPanelT.GetComponent<CanvasGroup>();
            }

            // Result texts
            AssignTMPReference(serializedManager, root, "resultTitleText", "ResultTitleText");
            AssignTMPReference(serializedManager, root, "resultTimeText", "ResultTimeText");
            AssignTMPReference(serializedManager, root, "resultMessageText", "ResultMessageText");

            // Result buttons
            AssignButtonByName(serializedManager, root, "resultPlayAgainButton", "ResultPlayAgainButton");
            AssignButtonByName(serializedManager, root, "resultExitButton", "ResultExitButton");

            // Real Money Panels - start inactive!
            Transform winPanelRM = FindDeep(root, "WinPanel_RealMoney");
            if (winPanelRM != null)
            {
                SerializedProperty winRMProp = serializedManager.FindProperty("winPanelRealMoney");
                if (winRMProp != null)
                    winRMProp.objectReferenceValue = winPanelRM.GetComponent<WinPanelController>();
            }

            Transform losePanelRM = FindDeep(root, "LosePanel_RealMoney");
            if (losePanelRM != null)
            {
                SerializedProperty loseRMProp = serializedManager.FindProperty("losePanelRealMoney");
                if (loseRMProp != null)
                    loseRMProp.objectReferenceValue = losePanelRM.GetComponent<WinPanelController>();
            }

            // Countdown UI - starts inactive!
            Transform countdownPanelT = FindDeep(root, "CountdownPanel");
            if (countdownPanelT != null)
            {
                SerializedProperty countdownProp = serializedManager.FindProperty("countdownUI");
                if (countdownProp != null)
                    countdownProp.objectReferenceValue = countdownPanelT.GetComponent<DigitPark.UI.CountdownUI>();
            }

            // Combo Text
            AssignTMPReference(serializedManager, root, "comboText", "ComboText");

            // Sparkle Effect
            Transform particleEffects = FindDeep(root, "ParticleEffects");
            if (particleEffects != null)
            {
                SerializedProperty sparkleProp = serializedManager.FindProperty("sparkleEffect");
                if (sparkleProp != null)
                    sparkleProp.objectReferenceValue = particleEffects.GetComponent<DigitPark.UI.UISparkleEffect>();
            }

            serializedManager.ApplyModifiedProperties();
            Debug.Log("[DigitRushUIBuilder] Referencias asignadas al DigitRushController");
        }

        /// <summary>
        /// Assigns a TMP text reference by searching the hierarchy (works with inactive objects)
        /// </summary>
        private static void AssignTMPReference(SerializedObject so, Transform root, string propertyName, string objectName)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop == null) return;

            Transform t = FindDeep(root, objectName);
            if (t != null)
                prop.objectReferenceValue = t.GetComponent<TextMeshProUGUI>();
        }

        /// <summary>
        /// Assigns a Button reference by searching the hierarchy (works with inactive objects)
        /// </summary>
        private static void AssignButtonByName(SerializedObject so, Transform root, string propertyName, string objectName)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop == null) return;

            Transform t = FindDeep(root, objectName);
            if (t != null)
                prop.objectReferenceValue = t.GetComponent<Button>();
        }

        // Old GameObject.Find-based methods removed — replaced by FindDeep-based methods above

        private static void UpdateAllTextStyles(Transform root)
        {
            foreach (TextMeshProUGUI tmp in root.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                tmp.fontStyle |= FontStyles.Bold;
            }
        }

        private static void UpdateCellStyles(Transform root)
        {
            for (int i = 1; i <= 9; i++)
            {
                Transform cell = FindDeep(root, $"Cell_{i}");
                if (cell != null)
                {
                    Image img = cell.GetComponent<Image>();
                    if (img != null) img.color = CELL_BG;

                    Outline outline = cell.GetComponent<Outline>();
                    if (outline == null) outline = cell.gameObject.AddComponent<Outline>();
                    outline.effectColor = CYAN_NEON;
                    outline.effectDistance = new Vector2(2, -2);
                }
            }
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

        #region Helper Methods

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

            return tmp;
        }

        #endregion
    }

    /// <summary>
    /// Simple SafeArea handler for notch devices
    /// </summary>
    public class SafeAreaHandler : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Rect lastSafeArea = Rect.zero;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        void Update()
        {
            if (Screen.safeArea != lastSafeArea)
            {
                ApplySafeArea();
            }
        }

        void ApplySafeArea()
        {
            Rect safeArea = Screen.safeArea;
            lastSafeArea = safeArea;

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
        }
    }
}
