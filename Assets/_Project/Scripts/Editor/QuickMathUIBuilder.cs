using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para reconstruir la UI de QuickMath con diseño profesional neón
    /// </summary>
    public class QuickMathUIBuilder : EditorWindow
    {
        // Colores del tema neón
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.4f, 1f);
        private static readonly Color DARK_BG = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color PANEL_BG = new Color(0.05f, 0.1f, 0.15f, 0.95f);
        private static readonly Color BUTTON_BG = new Color(0.08f, 0.15f, 0.2f, 1f);
        private static readonly Color ERROR_COLOR = new Color(1f, 0.3f, 0.3f, 1f);

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
                "Este script reconstruirá la UI de QuickMath.\n" +
                "Asegúrate de tener la escena QuickMath abierta.\n" +
                "Usa el BackButton prefab existente.",
                MessageType.Info);

            GUILayout.Space(10);

            if (GUILayout.Button("Reconstruir QuickMath UI", GUILayout.Height(40)))
            {
                RebuildQuickMathUI();
            }
        }

        private static void RebuildQuickMathUI()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No se encontró Canvas en la escena");
                return;
            }

            Transform canvasTransform = canvas.transform;

            // Verificar Background
            Transform background = canvasTransform.Find("Background");
            if (background == null)
            {
                Debug.LogError("No se encontró Background");
                return;
            }

            // LIMPIAR elementos viejos primero
            CleanOldElements(canvasTransform);

            // Crear nueva estructura
            CreateQuickMathLayout(canvasTransform);

            Debug.Log("QuickMath UI reconstruida exitosamente!");
            EditorUtility.SetDirty(canvas.gameObject);
        }

        private static void CleanOldElements(Transform canvasTransform)
        {
            string[] oldElements = new string[]
            {
                "RoundText",
                "TimerText",
                "ErrorsText",
                "ProblemText",
                "AnswersContainer",
                "AnswerButton_0",
                "AnswerButton_1",
                "AnswerButton_2",
                "ResultText",
                "Panel",
                // Nuevos elementos
                "Header",
                "StatsBar",
                "ProblemPanel",
                "AnswersPanel",
                "ProgressBar",
                "WinPanel"
            };

            foreach (string elementName in oldElements)
            {
                Transform element = canvasTransform.Find(elementName);
                if (element != null)
                {
                    Debug.Log($"Limpiando elemento viejo: {elementName}");
                    DestroyImmediate(element.gameObject);
                }
            }
        }

        private static void CreateQuickMathLayout(Transform canvasTransform)
        {
            // ========== HEADER ==========
            GameObject header = CreateOrFind(canvasTransform, "Header");
            SetupRectTransform(header,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -70), new Vector2(0, 140));

            // Title
            GameObject title = CreateOrFind(header.transform, "TitleText");
            SetupRectTransform(title,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 0), new Vector2(400, 60));
            SetupText(title, "QUICK MATH", 42, CYAN_NEON, FontStyles.Bold);

            // ========== STATS BAR ==========
            GameObject statsBar = CreateOrFind(canvasTransform, "StatsBar");
            SetupRectTransform(statsBar,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -180), new Vector2(-80, 60));

            Image statsBg = statsBar.GetComponent<Image>();
            if (statsBg == null) statsBg = statsBar.AddComponent<Image>();
            statsBg.color = PANEL_BG;

            // Añadir HorizontalLayoutGroup
            HorizontalLayoutGroup statsLayout = statsBar.GetComponent<HorizontalLayoutGroup>();
            if (statsLayout == null) statsLayout = statsBar.AddComponent<HorizontalLayoutGroup>();
            statsLayout.childAlignment = TextAnchor.MiddleCenter;
            statsLayout.spacing = 60;
            statsLayout.padding = new RectOffset(40, 40, 10, 10);
            statsLayout.childForceExpandWidth = false;
            statsLayout.childForceExpandHeight = true;
            statsLayout.childControlWidth = true;
            statsLayout.childControlHeight = true;

            // Timer
            GameObject timerContainer = CreateOrFind(statsBar.transform, "TimerContainer");
            AddLayoutElement(timerContainer, 200, 40);

            GameObject timerIcon = CreateOrFind(timerContainer.transform, "TimerIcon");
            SetupRectTransform(timerIcon, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(20, 0), new Vector2(30, 30));
            SetupText(timerIcon, "⏱", 24, CYAN_NEON, FontStyles.Normal);

            GameObject timerText = CreateOrFind(timerContainer.transform, "TimerText");
            SetupRectTransform(timerText, new Vector2(0, 0), new Vector2(1, 1), new Vector2(20, 0), new Vector2(-30, 0));
            SetupText(timerText, "0.00s", 28, Color.white, FontStyles.Normal);
            TextMeshProUGUI timerTmp = timerText.GetComponent<TextMeshProUGUI>();
            timerTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // Round
            GameObject roundText = CreateOrFind(statsBar.transform, "RoundText");
            AddLayoutElement(roundText, 120, 40);
            SetupText(roundText, "1/10", 28, CYAN_NEON, FontStyles.Bold);

            // Errors
            GameObject errorsContainer = CreateOrFind(statsBar.transform, "ErrorsContainer");
            AddLayoutElement(errorsContainer, 100, 40);

            GameObject errorsIcon = CreateOrFind(errorsContainer.transform, "ErrorsIcon");
            SetupRectTransform(errorsIcon, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(10, 0), new Vector2(30, 30));
            SetupText(errorsIcon, "✕", 24, ERROR_COLOR, FontStyles.Normal);

            GameObject errorsText = CreateOrFind(errorsContainer.transform, "ErrorsText");
            SetupRectTransform(errorsText, new Vector2(0, 0), new Vector2(1, 1), new Vector2(20, 0), new Vector2(-20, 0));
            SetupText(errorsText, "0", 28, ERROR_COLOR, FontStyles.Bold);
            TextMeshProUGUI errorsTmp = errorsText.GetComponent<TextMeshProUGUI>();
            errorsTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // ========== PROBLEM PANEL ==========
            GameObject problemPanel = CreateOrFind(canvasTransform, "ProblemPanel");
            SetupRectTransform(problemPanel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 150), new Vector2(700, 300));

            Image problemBg = problemPanel.GetComponent<Image>();
            if (problemBg == null) problemBg = problemPanel.AddComponent<Image>();
            problemBg.color = PANEL_BG;

            // Border del panel
            Outline problemOutline = problemPanel.GetComponent<Outline>();
            if (problemOutline == null) problemOutline = problemPanel.AddComponent<Outline>();
            problemOutline.effectColor = CYAN_NEON;
            problemOutline.effectDistance = new Vector2(3, 3);

            // Problem text (grande)
            GameObject problemText = CreateOrFind(problemPanel.transform, "ProblemText");
            SetupRectTransform(problemText,
                new Vector2(0, 0), new Vector2(1, 1),
                Vector2.zero, new Vector2(-40, -40));
            SetupText(problemText, "5 + 7 = ?", 96, Color.white, FontStyles.Bold);

            // ========== ANSWERS PANEL ==========
            GameObject answersPanel = CreateOrFind(canvasTransform, "AnswersPanel");
            SetupRectTransform(answersPanel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -150), new Vector2(750, 180));

            // Layout horizontal para los botones
            HorizontalLayoutGroup answersLayout = answersPanel.GetComponent<HorizontalLayoutGroup>();
            if (answersLayout == null) answersLayout = answersPanel.AddComponent<HorizontalLayoutGroup>();
            answersLayout.childAlignment = TextAnchor.MiddleCenter;
            answersLayout.spacing = 30;
            answersLayout.childForceExpandWidth = false;
            answersLayout.childForceExpandHeight = true;
            answersLayout.childControlWidth = false;
            answersLayout.childControlHeight = true;

            // Crear 3 botones de respuesta
            for (int i = 0; i < 3; i++)
            {
                CreateAnswerButton(answersPanel.transform, i);
            }

            // ========== PROGRESS BAR ==========
            GameObject progressBar = CreateOrFind(canvasTransform, "ProgressBar");
            SetupRectTransform(progressBar,
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 120), new Vector2(-120, 12));

            Image progressBg = progressBar.GetComponent<Image>();
            if (progressBg == null) progressBg = progressBar.AddComponent<Image>();
            progressBg.color = CYAN_DARK;

            GameObject progressFill = CreateOrFind(progressBar.transform, "ProgressFill");
            SetupRectTransform(progressFill,
                new Vector2(0, 0), new Vector2(0.1f, 1),  // 10% inicial
                Vector2.zero, Vector2.zero);
            Image progressFillImg = progressFill.GetComponent<Image>();
            if (progressFillImg == null) progressFillImg = progressFill.AddComponent<Image>();
            progressFillImg.color = CYAN_NEON;

            // ========== WIN PANEL ==========
            CreateWinPanel(canvasTransform);
        }

        private static void CreateAnswerButton(Transform parent, int index)
        {
            GameObject btnObj = CreateOrFind(parent, $"AnswerButton_{index}");

            // Layout element
            LayoutElement layout = btnObj.GetComponent<LayoutElement>();
            if (layout == null) layout = btnObj.AddComponent<LayoutElement>();
            layout.preferredWidth = 200;
            layout.preferredHeight = 160;

            // Background
            Image btnBg = btnObj.GetComponent<Image>();
            if (btnBg == null) btnBg = btnObj.AddComponent<Image>();
            btnBg.color = BUTTON_BG;

            // Button
            Button btn = btnObj.GetComponent<Button>();
            if (btn == null) btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnBg;

            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            btn.colors = colors;

            // Border
            GameObject border = CreateOrFind(btnObj.transform, "Border");
            SetupRectTransform(border, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Outline outline = btnObj.GetComponent<Outline>();
            if (outline == null) outline = btnObj.AddComponent<Outline>();
            outline.effectColor = CYAN_NEON;
            outline.effectDistance = new Vector2(2, 2);

            // Inner background
            GameObject innerBg = CreateOrFind(btnObj.transform, "InnerBg");
            SetupRectTransform(innerBg, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-8, -8));
            Image innerImg = innerBg.GetComponent<Image>();
            if (innerImg == null) innerImg = innerBg.AddComponent<Image>();
            innerImg.color = BUTTON_BG;
            innerImg.raycastTarget = false;

            // Answer text
            GameObject answerText = CreateOrFind(btnObj.transform, $"AnswerButtonText_{index}");
            SetupRectTransform(answerText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetupText(answerText, (10 + index).ToString(), 64, CYAN_NEON, FontStyles.Bold);

            // Add GameCardEffect for animations
            GameCardEffect effect = btnObj.GetComponent<GameCardEffect>();
            if (effect == null) effect = btnObj.AddComponent<GameCardEffect>();
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
                Vector2.zero, new Vector2(600, 500));

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
                new Vector2(0, -60), new Vector2(0, 80));
            SetupText(winTitle, "COMPLETADO!", 48, CYAN_NEON, FontStyles.Bold);

            // Stats
            GameObject statsText = CreateOrFind(content.transform, "StatsText");
            SetupRectTransform(statsText,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 20), new Vector2(400, 120));
            SetupText(statsText, "Tiempo: 0.00s\nErrores: 0", 32, Color.white, FontStyles.Normal);

            // Result
            GameObject resultText = CreateOrFind(content.transform, "ResultText");
            SetupRectTransform(resultText,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -60), new Vector2(400, 60));
            SetupText(resultText, "Excelente!", 36, CYAN_NEON, FontStyles.Bold);

            // Play again button
            GameObject playAgainBtn = CreateOrFind(content.transform, "PlayAgainButton");
            SetupRectTransform(playAgainBtn,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 80), new Vector2(300, 70));

            Button playBtn = playAgainBtn.GetComponent<Button>();
            if (playBtn == null) playBtn = playAgainBtn.AddComponent<Button>();

            Image playBtnImg = playAgainBtn.GetComponent<Image>();
            if (playBtnImg == null) playBtnImg = playAgainBtn.AddComponent<Image>();
            playBtnImg.color = CYAN_NEON;

            playBtn.targetGraphic = playBtnImg;

            GameObject playBtnText = CreateOrFind(playAgainBtn.transform, "PlayAgainButtonText");
            SetupRectTransform(playBtnText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetupText(playBtnText, "Jugar de Nuevo", 28, DARK_BG, FontStyles.Bold);

            // Desactivar por defecto
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
            {
                obj.AddComponent<RectTransform>();
            }

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
