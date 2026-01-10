using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;
using DigitPark.Games;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para reconstruir la UI de OddOneOut con diseño profesional neón
    /// Dos grids 4x4 lado a lado - Celdas de 85px optimizadas para móvil
    /// </summary>
    public class OddOneOutUIBuilder : EditorWindow
    {
        // Colores del tema neón
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color MAGENTA_NEON = new Color(1f, 0f, 0.8f, 1f);
        private static readonly Color GREEN_NEON = new Color(0.3f, 1f, 0.5f, 1f);
        private static readonly Color DARK_BG = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color PANEL_BG = new Color(0.05f, 0.1f, 0.15f, 0.95f);
        private static readonly Color BUTTON_BG = new Color(0.08f, 0.12f, 0.18f, 1f);
        private static readonly Color ERROR_COLOR = new Color(1f, 0.3f, 0.3f, 1f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);

        // Tamaño de celda optimizado - MÁS GRANDE para mejor visibilidad
        private const float CELL_SIZE = 100f;
        private const float CELL_SPACING = 4f;
        private const int GRID_COLUMNS = 4;
        private const float GRID_GAP = 15f; // Gap entre grids

        [MenuItem("DigitPark/Rebuild OddOneOut UI")]
        public static void ShowWindow()
        {
            GetWindow<OddOneOutUIBuilder>("OddOneOut UI Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("OddOneOut UI Builder", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Este script reconstruirá la UI de OddOneOut.\n" +
                "Asegúrate de tener la escena OddOneOut abierta.\n" +
                "Celdas de 85px - Layout optimizado sin labels.",
                MessageType.Info);

            GUILayout.Space(10);

            if (GUILayout.Button("Reconstruir OddOneOut UI", GUILayout.Height(40)))
            {
                RebuildOddOneOutUI();
            }
        }

        private static void RebuildOddOneOutUI()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[OddOneOutUIBuilder] No se encontró Canvas en la escena");
                return;
            }

            Transform canvasTransform = canvas.transform;

            // Limpiar elementos viejos
            CleanOldElements(canvasTransform);

            // Crear nueva estructura
            CreateOddOneOutLayout(canvasTransform);

            // Asignar referencias al controller
            AssignControllerReferences();

            Debug.Log("[OddOneOutUIBuilder] OddOneOut UI reconstruida exitosamente!");
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

        private static void CreateOddOneOutLayout(Transform canvasTransform)
        {
            // ========== SAFE AREA ==========
            GameObject safeArea = CreateElement(canvasTransform, "SafeArea");
            SetupRectTransform(safeArea, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SafeAreaHandler safeHandler = safeArea.AddComponent<SafeAreaHandler>();

            // ========== HEADER ==========
            CreateHeader(safeArea.transform);

            // ========== STATS BAR ==========
            CreateStatsBar(safeArea.transform);

            // ========== INSTRUCTION TEXT ==========
            GameObject instructionText = CreateElement(safeArea.transform, "InstructionText");
            SetupRectTransform(instructionText,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -185), new Vector2(600, 40));
            SetupText(instructionText, "¡ENCUENTRA LA DIFERENCIA!", 24, GOLD, FontStyles.Bold);

            // ========== COMBO TEXT ==========
            CreateComboText(safeArea.transform);

            // ========== GRIDS CONTAINER ==========
            CreateGridsContainer(safeArea.transform);

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

            GameObject title = CreateElement(header.transform, "TitleText");
            SetupRectTransform(title,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 0), new Vector2(500, 50));
            SetupText(title, "ODD ONE OUT", 34, CYAN_NEON, FontStyles.Bold);
        }

        private static void CreateStatsBar(Transform parent)
        {
            GameObject statsBar = CreateElement(parent, "StatsBar");
            SetupRectTransform(statsBar,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -112), new Vector2(-40, 42));

            Image statsBg = statsBar.AddComponent<Image>();
            statsBg.color = PANEL_BG;

            HorizontalLayoutGroup statsLayout = statsBar.AddComponent<HorizontalLayoutGroup>();
            statsLayout.childAlignment = TextAnchor.MiddleCenter;
            statsLayout.spacing = 50;
            statsLayout.padding = new RectOffset(25, 25, 5, 5);
            statsLayout.childForceExpandWidth = false;
            statsLayout.childForceExpandHeight = true;

            // Timer
            GameObject timerContainer = CreateElement(statsBar.transform, "TimerContainer");
            AddLayoutElement(timerContainer, 120, 32);
            GameObject timerText = CreateElement(timerContainer.transform, "TimerText");
            SetupRectTransform(timerText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI timerTmp = SetupText(timerText, "00:00", 22, Color.white, FontStyles.Bold);
            timerTmp.alignment = TextAlignmentOptions.Center;

            // Round
            GameObject roundText = CreateElement(statsBar.transform, "RoundText");
            AddLayoutElement(roundText, 70, 32);
            SetupText(roundText, "1/5", 24, CYAN_NEON, FontStyles.Bold);

            // Errors - Ahora con Image en lugar de texto
            GameObject errorsContainer = CreateElement(statsBar.transform, "ErrorsContainer");
            AddLayoutElement(errorsContainer, 70, 32);

            HorizontalLayoutGroup errLayout = errorsContainer.AddComponent<HorizontalLayoutGroup>();
            errLayout.childAlignment = TextAnchor.MiddleCenter;
            errLayout.spacing = 6;

            // Error Icon - Ahora es un Image component
            GameObject errorsIcon = CreateElement(errorsContainer.transform, "ErrorIcon");
            AddLayoutElement(errorsIcon, 22, 22);
            Image errorIconImg = errorsIcon.AddComponent<Image>();
            errorIconImg.color = ERROR_COLOR;
            // NOTA: Asignar sprite de X/close en el Inspector

            GameObject errorsText = CreateElement(errorsContainer.transform, "ErrorsText");
            AddLayoutElement(errorsText, 35, 32);
            SetupText(errorsText, "0", 22, ERROR_COLOR, FontStyles.Bold);
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
            SetupText(comboText, "x2", 26, GOLD, FontStyles.Bold);

            comboContainer.SetActive(false);
        }

        private static void CreateGridsContainer(Transform parent)
        {
            // Calcular tamaño del grid (sin labels)
            float gridSize = (CELL_SIZE * GRID_COLUMNS) + (CELL_SPACING * (GRID_COLUMNS - 1)) + 20; // +20 for padding

            GameObject gridsContainer = CreateElement(parent, "GridsContainer");
            SetupRectTransform(gridsContainer,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 30), new Vector2(gridSize * 2 + GRID_GAP, gridSize + 10));

            // ========== LEFT GRID (sin label) ==========
            GameObject leftGrid = CreateElement(gridsContainer.transform, "LeftGrid");
            SetupRectTransform(leftGrid,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(gridSize / 2, 0), new Vector2(gridSize, gridSize));

            Image leftGridBg = leftGrid.AddComponent<Image>();
            leftGridBg.color = new Color(0.03f, 0.06f, 0.12f, 0.8f);

            Outline leftOutline = leftGrid.AddComponent<Outline>();
            leftOutline.effectColor = CYAN_NEON;
            leftOutline.effectDistance = new Vector2(2.5f, -2.5f);

            // Agregar glow pulsante al borde de la grid
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

            // Create left grid buttons with 3D effect
            for (int i = 0; i < 16; i++)
            {
                CreateGridCell(leftGrid.transform, i, false);
            }

            // ========== RIGHT GRID (sin label) ==========
            GameObject rightGrid = CreateElement(gridsContainer.transform, "RightGrid");
            SetupRectTransform(rightGrid,
                new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(-gridSize / 2, 0), new Vector2(gridSize, gridSize));

            Image rightGridBg = rightGrid.AddComponent<Image>();
            rightGridBg.color = new Color(0.03f, 0.06f, 0.12f, 0.8f);

            Outline rightOutline = rightGrid.AddComponent<Outline>();
            rightOutline.effectColor = MAGENTA_NEON;
            rightOutline.effectDistance = new Vector2(2.5f, -2.5f);

            // Agregar glow pulsante al borde de la grid
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

            // Create right grid buttons with 3D effect
            for (int i = 0; i < 16; i++)
            {
                CreateGridCell(rightGrid.transform, i, true);
            }
        }

        private static void CreateGridCell(Transform parent, int index, bool isRight)
        {
            string prefix = isRight ? "RightButton" : "LeftButton";
            Color borderColor = isRight ? MAGENTA_NEON : CYAN_NEON;

            // Cell container
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

            // Text - más grande para celdas de 100px con glow
            string textName = isRight ? $"RightButtonText_{index}" : $"LeftButtonText_{index}";
            GameObject textObj = CreateElement(face.transform, textName);
            SetupRectTransform(textObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI tmp = SetupText(textObj, "A", 46, Color.white, FontStyles.Bold);
            tmp.alignment = TextAlignmentOptions.Center;

            // Agregar outline/glow al texto para mejor visibilidad
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

            // Assign references via SerializedObject
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

        private static void CreateWinPanel(Transform parent)
        {
            GameObject winPanel = CreateElement(parent, "WinPanel");
            SetupRectTransform(winPanel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Image overlayImg = winPanel.AddComponent<Image>();
            overlayImg.color = new Color(0, 0, 0, 0.85f);

            CanvasGroup winCg = winPanel.AddComponent<CanvasGroup>();
            winCg.alpha = 0;

            // Content
            GameObject content = CreateElement(winPanel.transform, "Content");
            SetupRectTransform(content,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(500, 350));

            Image contentBg = content.AddComponent<Image>();
            contentBg.color = PANEL_BG;

            Outline contentOutline = content.AddComponent<Outline>();
            contentOutline.effectColor = GREEN_NEON;
            contentOutline.effectDistance = new Vector2(3, -3);

            // Title
            GameObject winTitle = CreateElement(content.transform, "WinTitle");
            SetupRectTransform(winTitle,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -40), new Vector2(0, 60));
            SetupText(winTitle, "¡COMPLETADO!", 42, GREEN_NEON, FontStyles.Bold);

            // Stats
            GameObject statsText = CreateElement(content.transform, "StatsText");
            SetupRectTransform(statsText,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 10), new Vector2(350, 100));
            TextMeshProUGUI statsTmp = SetupText(statsText, "Tiempo: 00:00\nErrores: 0\nMax Combo: x1", 24, Color.white, FontStyles.Normal);
            statsTmp.lineSpacing = 15;

            // Play again button
            GameObject playAgainBtn = CreateElement(content.transform, "PlayAgainButton");
            SetupRectTransform(playAgainBtn,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 55), new Vector2(280, 60));

            Image playBtnImg = playAgainBtn.AddComponent<Image>();
            playBtnImg.color = GREEN_NEON;

            Outline btnOutline = playAgainBtn.AddComponent<Outline>();
            btnOutline.effectColor = new Color(0.1f, 0.4f, 0.2f, 1f);
            btnOutline.effectDistance = new Vector2(2, -2);

            Button playBtn = playAgainBtn.AddComponent<Button>();
            playBtn.targetGraphic = playBtnImg;

            GameObject playBtnText = CreateElement(playAgainBtn.transform, "PlayAgainButtonText");
            SetupRectTransform(playBtnText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetupText(playBtnText, "JUGAR DE NUEVO", 26, DARK_BG, FontStyles.Bold);

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
            var controller = FindFirstObjectByType<OddOneOutController>();
            if (controller == null)
            {
                Debug.LogWarning("[OddOneOutUIBuilder] No se encontró OddOneOutController en la escena");
                return;
            }

            SerializedObject so = new SerializedObject(controller);

            // Left grid buttons
            Transform leftGrid = GameObject.Find("LeftGrid")?.transform;
            if (leftGrid != null)
            {
                SerializedProperty leftButtonsProp = so.FindProperty("leftGridButtons");
                SerializedProperty leftTextsProp = so.FindProperty("leftButtonTexts");
                SerializedProperty leftImagesProp = so.FindProperty("leftButtonImages");

                if (leftButtonsProp != null) leftButtonsProp.arraySize = 16;
                if (leftTextsProp != null) leftTextsProp.arraySize = 16;
                if (leftImagesProp != null) leftImagesProp.arraySize = 16;

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
                            if (leftImagesProp != null)
                                leftImagesProp.GetArrayElementAtIndex(i).objectReferenceValue = face.GetComponent<Image>();

                            TextMeshProUGUI txt = face.GetComponentInChildren<TextMeshProUGUI>();
                            if (leftTextsProp != null && txt != null)
                                leftTextsProp.GetArrayElementAtIndex(i).objectReferenceValue = txt;
                        }
                    }
                }
            }

            // Right grid buttons
            Transform rightGrid = GameObject.Find("RightGrid")?.transform;
            if (rightGrid != null)
            {
                SerializedProperty rightButtonsProp = so.FindProperty("rightGridButtons");
                SerializedProperty rightTextsProp = so.FindProperty("rightButtonTexts");
                SerializedProperty rightImagesProp = so.FindProperty("rightButtonImages");

                if (rightButtonsProp != null) rightButtonsProp.arraySize = 16;
                if (rightTextsProp != null) rightTextsProp.arraySize = 16;
                if (rightImagesProp != null) rightImagesProp.arraySize = 16;

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
                            if (rightImagesProp != null)
                                rightImagesProp.GetArrayElementAtIndex(i).objectReferenceValue = face.GetComponent<Image>();

                            TextMeshProUGUI txt = face.GetComponentInChildren<TextMeshProUGUI>();
                            if (rightTextsProp != null && txt != null)
                                rightTextsProp.GetArrayElementAtIndex(i).objectReferenceValue = txt;
                        }
                    }
                }
            }

            // UI elements
            AssignTMPReference(so, "timerText", "TimerText");
            AssignTMPReference(so, "roundText", "RoundText");
            AssignTMPReference(so, "errorsText", "ErrorsText");
            AssignTMPReference(so, "instructionText", "InstructionText");
            AssignTMPReference(so, "comboText", "ComboText");

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

                AssignTMPReference(so, "statsText", "StatsText");
            }

            // Sparkle effect
            GameObject particleEffects = GameObject.Find("ParticleEffects");
            if (particleEffects != null)
            {
                SerializedProperty sparkleProp = so.FindProperty("sparkleEffect");
                if (sparkleProp != null)
                    sparkleProp.objectReferenceValue = particleEffects.GetComponent<UISparkleEffect>();
            }

            // Play again button
            GameObject playAgainBtn = GameObject.Find("PlayAgainButton");
            if (playAgainBtn != null)
            {
                SerializedProperty playBtnProp = so.FindProperty("playAgainButton");
                if (playBtnProp != null)
                    playBtnProp.objectReferenceValue = playAgainBtn.GetComponent<Button>();
            }

            so.ApplyModifiedProperties();
            Debug.Log("[OddOneOutUIBuilder] Referencias asignadas al Controller");
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
