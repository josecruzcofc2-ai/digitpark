using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para reconstruir la UI de OddOneOut con diseño profesional neón
    /// Dos grids 4x4 lado a lado con VS en el centro
    /// </summary>
    public class OddOneOutUIBuilder : EditorWindow
    {
        // Colores del tema neón
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color MAGENTA_NEON = new Color(1f, 0f, 0.8f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.4f, 1f);
        private static readonly Color DARK_BG = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color PANEL_BG = new Color(0.05f, 0.1f, 0.15f, 0.95f);
        private static readonly Color BUTTON_BG = new Color(0.08f, 0.12f, 0.18f, 1f);
        private static readonly Color ERROR_COLOR = new Color(1f, 0.3f, 0.3f, 1f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);

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
                "Crea dos grids 4x4 con VS en el centro.",
                MessageType.Info);

            GUILayout.Space(10);

            if (GUILayout.Button("Reconstruir OddOneOut UI", GUILayout.Height(40)))
            {
                RebuildOddOneOutUI();
            }
        }

        private static void RebuildOddOneOutUI()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No se encontró Canvas en la escena");
                return;
            }

            Transform canvasTransform = canvas.transform;

            Transform background = canvasTransform.Find("Background");
            if (background == null)
            {
                Debug.LogError("No se encontró Background");
                return;
            }

            // Limpiar elementos viejos
            CleanOldElements(canvasTransform);

            // Crear nueva estructura
            CreateOddOneOutLayout(canvasTransform);

            Debug.Log("OddOneOut UI reconstruida exitosamente!");
            EditorUtility.SetDirty(canvas.gameObject);
        }

        private static void CleanOldElements(Transform canvasTransform)
        {
            string[] oldElements = new string[]
            {
                "TimerText", "RoundText", "ErrorsText", "InstructionText",
                "Header", "LeftGrid", "RightGrid", "WinPanel",
                "StatsBar", "GridsContainer", "VSText"
            };

            foreach (string elementName in oldElements)
            {
                Transform element = canvasTransform.Find(elementName);
                if (element != null)
                {
                    Debug.Log($"Limpiando: {elementName}");
                    DestroyImmediate(element.gameObject);
                }
            }

            // Limpiar botones individuales viejos en la raíz
            for (int i = 0; i < 16; i++)
            {
                string[] buttonNames = { $"LeftButton_{i}", $"RightButton_{i}" };
                foreach (string btnName in buttonNames)
                {
                    Transform btn = canvasTransform.Find(btnName);
                    if (btn != null) DestroyImmediate(btn.gameObject);
                }
            }
        }

        private static void CreateOddOneOutLayout(Transform canvasTransform)
        {
            // ========== HEADER ==========
            GameObject header = CreateOrFind(canvasTransform, "Header");
            SetupRectTransform(header,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -60), new Vector2(0, 120));

            GameObject title = CreateOrFind(header.transform, "TitleText");
            SetupRectTransform(title,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 0), new Vector2(500, 50));
            SetupText(title, "ODD ONE OUT", 38, CYAN_NEON, FontStyles.Bold);

            // ========== STATS BAR ==========
            GameObject statsBar = CreateOrFind(canvasTransform, "StatsBar");
            SetupRectTransform(statsBar,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -150), new Vector2(-60, 50));

            Image statsBg = statsBar.GetComponent<Image>();
            if (statsBg == null) statsBg = statsBar.AddComponent<Image>();
            statsBg.color = PANEL_BG;

            HorizontalLayoutGroup statsLayout = statsBar.GetComponent<HorizontalLayoutGroup>();
            if (statsLayout == null) statsLayout = statsBar.AddComponent<HorizontalLayoutGroup>();
            statsLayout.childAlignment = TextAnchor.MiddleCenter;
            statsLayout.spacing = 40;
            statsLayout.padding = new RectOffset(30, 30, 8, 8);
            statsLayout.childForceExpandWidth = false;
            statsLayout.childForceExpandHeight = true;

            // Timer
            GameObject timerContainer = CreateOrFind(statsBar.transform, "TimerContainer");
            AddLayoutElement(timerContainer, 150, 35);
            GameObject timerIcon = CreateOrFind(timerContainer.transform, "Icon");
            SetupRectTransform(timerIcon, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(12, 0), new Vector2(25, 25));
            SetupText(timerIcon, "⏱", 20, CYAN_NEON, FontStyles.Normal);
            GameObject timerText = CreateOrFind(timerContainer.transform, "TimerText");
            SetupRectTransform(timerText, new Vector2(0, 0), new Vector2(1, 1), new Vector2(12, 0), new Vector2(-20, 0));
            SetupText(timerText, "00:00", 22, Color.white, FontStyles.Normal);
            timerText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;

            // Round
            GameObject roundText = CreateOrFind(statsBar.transform, "RoundText");
            AddLayoutElement(roundText, 100, 35);
            SetupText(roundText, "1/5", 24, CYAN_NEON, FontStyles.Bold);

            // Errors
            GameObject errorsContainer = CreateOrFind(statsBar.transform, "ErrorsContainer");
            AddLayoutElement(errorsContainer, 80, 35);
            GameObject errorsIcon = CreateOrFind(errorsContainer.transform, "Icon");
            SetupRectTransform(errorsIcon, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(10, 0), new Vector2(25, 25));
            SetupText(errorsIcon, "✕", 20, ERROR_COLOR, FontStyles.Normal);
            GameObject errorsText = CreateOrFind(errorsContainer.transform, "ErrorsText");
            SetupRectTransform(errorsText, new Vector2(0, 0), new Vector2(1, 1), new Vector2(12, 0), new Vector2(-15, 0));
            SetupText(errorsText, "0", 22, ERROR_COLOR, FontStyles.Bold);
            errorsText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;

            // ========== INSTRUCTION TEXT ==========
            GameObject instructionText = CreateOrFind(canvasTransform, "InstructionText");
            SetupRectTransform(instructionText,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -220), new Vector2(600, 50));
            SetupText(instructionText, "¡ENCUENTRA LA DIFERENCIA!", 28, GOLD, FontStyles.Bold);

            // ========== GRIDS CONTAINER ==========
            GameObject gridsContainer = CreateOrFind(canvasTransform, "GridsContainer");
            SetupRectTransform(gridsContainer,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -30), new Vector2(900, 500));

            // ========== LEFT GRID ==========
            GameObject leftGridContainer = CreateOrFind(gridsContainer.transform, "LeftGridContainer");
            SetupRectTransform(leftGridContainer,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(200, 0), new Vector2(380, 450));

            // Left Grid Label
            GameObject leftLabel = CreateOrFind(leftGridContainer.transform, "Label");
            SetupRectTransform(leftLabel,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, 10), new Vector2(200, 40));
            SetupText(leftLabel, "ORIGINAL", 22, CYAN_NEON, FontStyles.Bold);

            // Left Grid
            GameObject leftGrid = CreateOrFind(leftGridContainer.transform, "LeftGrid");
            SetupRectTransform(leftGrid,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -20), new Vector2(360, 360));

            Image leftGridBg = leftGrid.GetComponent<Image>();
            if (leftGridBg == null) leftGridBg = leftGrid.AddComponent<Image>();
            leftGridBg.color = new Color(0, 0, 0, 0.3f);

            Outline leftOutline = leftGrid.GetComponent<Outline>();
            if (leftOutline == null) leftOutline = leftGrid.AddComponent<Outline>();
            leftOutline.effectColor = CYAN_NEON;
            leftOutline.effectDistance = new Vector2(2, 2);

            GridLayoutGroup leftGridLayout = leftGrid.GetComponent<GridLayoutGroup>();
            if (leftGridLayout == null) leftGridLayout = leftGrid.AddComponent<GridLayoutGroup>();
            leftGridLayout.cellSize = new Vector2(80, 80);
            leftGridLayout.spacing = new Vector2(8, 8);
            leftGridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            leftGridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            leftGridLayout.childAlignment = TextAnchor.MiddleCenter;
            leftGridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            leftGridLayout.constraintCount = 4;
            leftGridLayout.padding = new RectOffset(6, 6, 6, 6);

            // Create left grid buttons
            for (int i = 0; i < 16; i++)
            {
                CreateGridButton(leftGrid.transform, i, false);
            }

            // ========== VS TEXT ==========
            GameObject vsText = CreateOrFind(gridsContainer.transform, "VSText");
            SetupRectTransform(vsText,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 0), new Vector2(80, 80));
            SetupText(vsText, "VS", 36, GOLD, FontStyles.Bold);

            // ========== RIGHT GRID ==========
            GameObject rightGridContainer = CreateOrFind(gridsContainer.transform, "RightGridContainer");
            SetupRectTransform(rightGridContainer,
                new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(-200, 0), new Vector2(380, 450));

            // Right Grid Label
            GameObject rightLabel = CreateOrFind(rightGridContainer.transform, "Label");
            SetupRectTransform(rightLabel,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, 10), new Vector2(200, 40));
            SetupText(rightLabel, "COPIA", 22, MAGENTA_NEON, FontStyles.Bold);

            // Right Grid
            GameObject rightGrid = CreateOrFind(rightGridContainer.transform, "RightGrid");
            SetupRectTransform(rightGrid,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -20), new Vector2(360, 360));

            Image rightGridBg = rightGrid.GetComponent<Image>();
            if (rightGridBg == null) rightGridBg = rightGrid.AddComponent<Image>();
            rightGridBg.color = new Color(0, 0, 0, 0.3f);

            Outline rightOutline = rightGrid.GetComponent<Outline>();
            if (rightOutline == null) rightOutline = rightGrid.AddComponent<Outline>();
            rightOutline.effectColor = MAGENTA_NEON;
            rightOutline.effectDistance = new Vector2(2, 2);

            GridLayoutGroup rightGridLayout = rightGrid.GetComponent<GridLayoutGroup>();
            if (rightGridLayout == null) rightGridLayout = rightGrid.AddComponent<GridLayoutGroup>();
            rightGridLayout.cellSize = new Vector2(80, 80);
            rightGridLayout.spacing = new Vector2(8, 8);
            rightGridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            rightGridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            rightGridLayout.childAlignment = TextAnchor.MiddleCenter;
            rightGridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            rightGridLayout.constraintCount = 4;
            rightGridLayout.padding = new RectOffset(6, 6, 6, 6);

            // Create right grid buttons
            for (int i = 0; i < 16; i++)
            {
                CreateGridButton(rightGrid.transform, i, true);
            }

            // ========== WIN PANEL ==========
            CreateWinPanel(canvasTransform);
        }

        private static void CreateGridButton(Transform parent, int index, bool isRight)
        {
            string prefix = isRight ? "RightButton" : "LeftButton";
            Color borderColor = isRight ? MAGENTA_NEON : CYAN_NEON;

            GameObject btn = CreateOrFind(parent, $"{prefix}_{index}");

            // Background/Image
            Image btnImg = btn.GetComponent<Image>();
            if (btnImg == null) btnImg = btn.AddComponent<Image>();
            btnImg.color = BUTTON_BG;

            // Button
            Button button = btn.GetComponent<Button>();
            if (button == null) button = btn.AddComponent<Button>();
            button.targetGraphic = btnImg;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            button.colors = colors;

            // Border
            Outline outline = btn.GetComponent<Outline>();
            if (outline == null) outline = btn.AddComponent<Outline>();
            outline.effectColor = borderColor;
            outline.effectDistance = new Vector2(1.5f, 1.5f);

            // Text
            string textPrefix = isRight ? "RightButtonText" : "LeftButtonText";
            GameObject textObj = CreateOrFind(btn.transform, $"{textPrefix}_{index}");
            SetupRectTransform(textObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetupText(textObj, "A", 36, Color.white, FontStyles.Bold);
        }

        private static void CreateWinPanel(Transform canvasTransform)
        {
            GameObject winPanel = CreateOrFind(canvasTransform, "WinPanel");
            SetupRectTransform(winPanel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Image overlayImg = winPanel.GetComponent<Image>();
            if (overlayImg == null) overlayImg = winPanel.AddComponent<Image>();
            overlayImg.color = new Color(0, 0, 0, 0.85f);

            // Content
            GameObject content = CreateOrFind(winPanel.transform, "Content");
            SetupRectTransform(content,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(550, 400));

            Image contentBg = content.GetComponent<Image>();
            if (contentBg == null) contentBg = content.AddComponent<Image>();
            contentBg.color = PANEL_BG;

            Outline contentOutline = content.GetComponent<Outline>();
            if (contentOutline == null) contentOutline = content.AddComponent<Outline>();
            contentOutline.effectColor = CYAN_NEON;
            contentOutline.effectDistance = new Vector2(3, 3);

            // Title
            GameObject winTitle = CreateOrFind(content.transform, "WinTitle");
            SetupRectTransform(winTitle,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -45), new Vector2(0, 60));
            SetupText(winTitle, "COMPLETADO!", 44, CYAN_NEON, FontStyles.Bold);

            // Stats
            GameObject statsText = CreateOrFind(content.transform, "StatsText");
            SetupRectTransform(statsText,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 15), new Vector2(350, 90));
            SetupText(statsText, "Tiempo: 00:00\nErrores: 0", 26, Color.white, FontStyles.Normal);

            // Play again button
            GameObject playAgainBtn = CreateOrFind(content.transform, "PlayAgainButton");
            SetupRectTransform(playAgainBtn,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 65), new Vector2(260, 60));

            Button playBtn = playAgainBtn.GetComponent<Button>();
            if (playBtn == null) playBtn = playAgainBtn.AddComponent<Button>();

            Image playBtnImg = playAgainBtn.GetComponent<Image>();
            if (playBtnImg == null) playBtnImg = playAgainBtn.AddComponent<Image>();
            playBtnImg.color = CYAN_NEON;
            playBtn.targetGraphic = playBtnImg;

            GameObject playBtnText = CreateOrFind(playAgainBtn.transform, "PlayAgainButtonText");
            SetupRectTransform(playBtnText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetupText(playBtnText, "Jugar de Nuevo", 24, DARK_BG, FontStyles.Bold);

            winPanel.SetActive(false);
        }

        // ========== UTILIDADES ==========

        private static GameObject CreateOrFind(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null) return existing.gameObject;

            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            if (obj.GetComponent<RectTransform>() == null)
                obj.AddComponent<RectTransform>();

            return obj;
        }

        private static RectTransform SetupRectTransform(GameObject obj, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            RectTransform rect = obj.GetComponent<RectTransform>();
            if (rect == null) rect = obj.AddComponent<RectTransform>();

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = sizeDelta;

            return rect;
        }

        private static void SetupText(GameObject obj, string text, int fontSize, Color color, FontStyles style)
        {
            TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = obj.AddComponent<TextMeshProUGUI>();

            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
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
