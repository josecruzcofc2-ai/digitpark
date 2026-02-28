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

        // Stats Bar icon paths
        private const string TIMER_ICON_PATH = "Assets/_Project/Art/Icons/UI/TimerIcon.png";
        private const string ROUND_ICON_PATH = "Assets/_Project/Art/Icons/UI/RoundIcon.png";
        private const string ERROR_ICON_PATH = "Assets/_Project/Art/Icons/UI/ErrorIcon.png";

        [MenuItem("DigitPark/UI Builders/Games/DigitRush", false, 130)]
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
            CleanupOldUI();
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
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
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            // Update existing elements with new styles
            UpdateAllTextStyles(canvas.transform);
            UpdateCellStyles(canvas.transform);

            Debug.Log("[DigitRushUIBuilder] Estilos actualizados!");
            EditorUtility.SetDirty(canvas.gameObject);
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

            // ========== STATS BAR ==========
            CreateStatsBar(safeArea.transform);

            // ========== GAME PANEL (GRID) ==========
            CreateGamePanel(safeArea.transform);

            // ========== BARRA DE PROGRESO ==========
            CreateProgressBar(safeArea.transform);

            // ========== ACTION BUTTONS ==========
            CreateActionButtons(safeArea.transform);

            // ========== RESULT PANEL (Practice) ==========
            CreateResultPanel(safeArea.transform);

            // ========== REAL MONEY PANELS (Cash Battle) ==========
            CreateRealMoneyPanels(safeArea.transform);

            // ========== SETTINGS PANEL ==========
            CreateSettingsPanel(safeArea.transform);

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
                new Vector2(0, -50), new Vector2(0, 100));

            Image headerBg = header.AddComponent<Image>();
            headerBg.color = new Color(0f, 0f, 0f, 0.3f);

            // Back Button placeholder (user will add their own)
            // Note: Back button removed - user will add their own prefab

            // Title
            GameObject title = CreateElement(header.transform, "TitleText");
            SetupRectTransform(title,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 0), new Vector2(600, 80));
            SetupText(title, "DIGIT RUSH", (int)FontSizes.H4, CYAN_NEON, FontStyles.Bold);
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
                "0.000s", Color.white, 240, timerIcon, 36);

            // Round
            CreateStatItem(statsBar.transform, "RoundContainer", "RoundIcon", "RoundText",
                "1/1", CYAN_NEON, 180, roundIcon, 36);

            // Errors
            Color ERROR_STAT = new Color(1f, 0.3f, 0.3f, 1f);
            CreateStatItem(statsBar.transform, "ErrorsContainer", "ErrorsIcon", "ErrorsText",
                "0", ERROR_STAT, 120, errorIcon, 36);
        }

        private static void CreateStatItem(Transform parent, string containerName, string iconName,
            string textName, string defaultText, Color color, float width, Sprite iconSprite = null, int fontSize = 36)
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
            so.FindProperty("errorFaceColor").colorValue = new Color(0.4f, 0.08f, 0.08f, 1f);
            so.FindProperty("errorTextColor").colorValue = new Color(1f, 0.4f, 0.4f, 1f);
            so.FindProperty("errorGlowColor").colorValue = new Color(1f, 0.2f, 0.2f, 0.8f);
            so.ApplyModifiedProperties();
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
            SetupText(roundIndicator, "1/1", 20, Color.white, FontStyles.Bold);

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

        private static void CreateSettingsPanel(Transform parent)
        {
            GameObject settingsPanel = CreateElement(parent, "SettingsPanel");
            SetupRectTransform(settingsPanel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Image overlay = settingsPanel.AddComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0.9f);
            overlay.raycastTarget = true;

            // Card
            GameObject card = CreateElement(settingsPanel.transform, "SettingsCard");
            SetupRectTransform(card,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 20), new Vector2(600, 450));

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.04f, 0.08f, 0.14f, 0.98f);

            Outline cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = CYAN_NEON;
            cardOutline.effectDistance = new Vector2(3, -3);

            // Title
            GameObject titleObj = CreateElement(card.transform, "SettingsTitle");
            SetupRectTransform(titleObj,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -32), new Vector2(0, 50));
            SetupText(titleObj, "DIGIT RUSH", 44, CYAN_NEON, FontStyles.Bold);

            Outline titleGlow = titleObj.AddComponent<Outline>();
            titleGlow.effectColor = new Color(0f, 0.5f, 0.5f, 0.6f);
            titleGlow.effectDistance = new Vector2(2, -2);

            // Subtitle
            GameObject subtitleObj = CreateElement(card.transform, "SettingsSubtitle");
            SetupRectTransform(subtitleObj,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -70), new Vector2(0, 24));
            SetupText(subtitleObj, "Tap 1-9 in order", 18, new Color(0.5f, 0.5f, 0.6f), FontStyles.Bold);

            // Divider
            GameObject divider = CreateElement(card.transform, "Divider");
            SetupRectTransform(divider,
                new Vector2(0.08f, 1), new Vector2(0.92f, 1),
                new Vector2(0, -95), new Vector2(0, 2));
            Image divImg = divider.AddComponent<Image>();
            divImg.color = new Color(1f, 1f, 1f, 0.1f);
            divImg.raycastTarget = false;

            // Rounds header
            float yPos = -115f;

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

            CreateSettingsToggle(roundsContainer.transform, "ToggleRounds1", "1", true);
            CreateSettingsToggle(roundsContainer.transform, "ToggleRounds3", "3", false);
            CreateSettingsToggle(roundsContainer.transform, "ToggleRounds5", "5", false);
            CreateSettingsToggle(roundsContainer.transform, "ToggleRounds10", "10", false);

            // Start button
            yPos -= 78f;

            GameObject startBtn = CreateElement(card.transform, "StartGameButton");
            SetupRectTransform(startBtn,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, yPos), new Vector2(500, 68));

            // Shadow
            GameObject startShadow = CreateElement(startBtn.transform, "Shadow");
            SetupRectTransform(startShadow,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(3, -6), new Vector2(500, 68));
            Image shadowImg = startShadow.AddComponent<Image>();
            shadowImg.color = new Color(0f, 0.3f, 0.15f, 0.6f);
            shadowImg.raycastTarget = false;

            Image startBtnImg = startBtn.AddComponent<Image>();
            startBtnImg.color = GREEN_NEON;

            Outline startOutline = startBtn.AddComponent<Outline>();
            startOutline.effectColor = new Color(0.1f, 0.5f, 0.25f, 1f);
            startOutline.effectDistance = new Vector2(2, -2);

            Button startButton = startBtn.AddComponent<Button>();
            startButton.targetGraphic = startBtnImg;

            GameObject startText = CreateElement(startBtn.transform, "StartText");
            SetupRectTransform(startText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetupText(startText, "START", 34, DARK_BG, FontStyles.Bold);

            settingsPanel.SetActive(false);
        }

        private static void CreateSettingsToggle(Transform parent, string name, string label, bool isOn)
        {
            GameObject toggleObj = CreateElement(parent, name);

            Image toggleBg = toggleObj.AddComponent<Image>();
            toggleBg.color = isOn ? CYAN_NEON : new Color(0.08f, 0.12f, 0.18f, 1f);

            Outline toggleOutline = toggleObj.AddComponent<Outline>();
            toggleOutline.effectColor = new Color(0f, 0.7f, 0.7f, 0.5f);
            toggleOutline.effectDistance = new Vector2(1.5f, -1.5f);

            Toggle toggle = toggleObj.AddComponent<Toggle>();
            toggle.targetGraphic = toggleBg;
            toggle.toggleTransition = Toggle.ToggleTransition.None;
            toggle.graphic = null;
            toggle.isOn = isOn;

            GameObject labelObj = CreateElement(toggleObj.transform, "Label");
            SetupRectTransform(labelObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI labelTmp = SetupText(labelObj, label, 28, isOn ? DARK_BG : Color.white, FontStyles.Bold);
            labelTmp.raycastTarget = false;
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
            TextMeshProUGUI msgTmp = SetupText(messageText, "Great job!", 32, Color.white, FontStyles.Bold);
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

            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
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

            // Round + Errors texts
            AssignTMPReference(serializedManager, root, "roundText", "RoundText");
            AssignTMPReference(serializedManager, root, "errorsText", "ErrorsText");
            AssignTMPReference(serializedManager, root, "roundIndicatorText", "RoundIndicator");

            // Progress Fill
            Transform progressFillT = FindDeep(root, "ProgressFill");
            if (progressFillT != null)
            {
                SerializedProperty progressProp = serializedManager.FindProperty("progressFill");
                if (progressProp != null)
                    progressProp.objectReferenceValue = progressFillT.GetComponent<RectTransform>();
            }

            // Settings Panel
            Transform settingsPanelT = FindDeep(root, "SettingsPanel");
            if (settingsPanelT != null)
            {
                SerializedProperty settingsProp = serializedManager.FindProperty("settingsPanel");
                if (settingsProp != null)
                    settingsProp.objectReferenceValue = settingsPanelT.gameObject;

                AssignToggle(serializedManager, "toggleRounds1", FindDeep(settingsPanelT, "ToggleRounds1"));
                AssignToggle(serializedManager, "toggleRounds3", FindDeep(settingsPanelT, "ToggleRounds3"));
                AssignToggle(serializedManager, "toggleRounds5", FindDeep(settingsPanelT, "ToggleRounds5"));
                AssignToggle(serializedManager, "toggleRounds10", FindDeep(settingsPanelT, "ToggleRounds10"));

                Transform startBtnT = FindDeep(settingsPanelT, "StartGameButton");
                if (startBtnT != null)
                {
                    SerializedProperty startBtnProp = serializedManager.FindProperty("startGameButton");
                    if (startBtnProp != null)
                        startBtnProp.objectReferenceValue = startBtnT.GetComponent<Button>();
                }
            }

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

        private static void AssignToggle(SerializedObject so, string propertyName, Transform toggleTransform)
        {
            if (toggleTransform == null) return;
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop != null)
                prop.objectReferenceValue = toggleTransform.GetComponent<Toggle>();
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
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinBody;
            tmp.fontSizeMax = fontSize > 0 ? fontSize : FontSizes.Body;

            return tmp;
        }

        #endregion
    }
}
