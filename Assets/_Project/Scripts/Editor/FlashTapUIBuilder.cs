using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para reconstruir la UI de FlashTap con diseño profesional neón
    /// </summary>
    public class FlashTapUIBuilder : EditorWindow
    {
        // Colores del tema neón
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.3f, 0.3f, 1f);
        private static readonly Color DARK_BG = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color PANEL_BG = new Color(0.05f, 0.1f, 0.15f, 0.95f);
        private static readonly Color WAIT_COLOR = new Color(0.3f, 0.3f, 0.3f, 1f);
        private static readonly Color GO_COLOR = new Color(0f, 1f, 0.5f, 1f);
        private static readonly Color ERROR_COLOR = new Color(1f, 0.3f, 0.3f, 1f);

        [MenuItem("DigitPark/Rebuild FlashTap UI")]
        public static void ShowWindow()
        {
            GetWindow<FlashTapUIBuilder>("FlashTap UI Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("FlashTap UI Builder", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Este script reconstruirá la UI de FlashTap.\n" +
                "Asegúrate de tener la escena FlashTap abierta.\n" +
                "Usa el BackButton prefab existente.",
                MessageType.Info);

            GUILayout.Space(10);

            if (GUILayout.Button("Reconstruir FlashTap UI", GUILayout.Height(40)))
            {
                RebuildFlashTapUI();
            }
        }

        private static void RebuildFlashTapUI()
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
            CreateFlashTapLayout(canvasTransform);

            Debug.Log("FlashTap UI reconstruida exitosamente!");
            EditorUtility.SetDirty(canvas.gameObject);
        }

        private static void CleanOldElements(Transform canvasTransform)
        {
            // Elementos viejos que estaban en la raíz y ahora van en otros lugares
            string[] oldElements = new string[]
            {
                "RoundText",           // Ahora va en RoundBadge
                "ReactionTimeText",    // Ahora va en StatsPanel
                "AverageText",         // Ahora va en StatsPanel
                "TitleText",           // Ahora va en Header
                // También limpiar los nuevos por si se ejecuta múltiples veces
                "Header",
                "RoundBadge",
                "InstructionText",
                "TapButton",
                "StatsPanel",
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

        private static void CreateFlashTapLayout(Transform canvasTransform)
        {
            // ========== HEADER ==========
            GameObject header = CreateOrFind(canvasTransform, "Header");
            SetupRectTransform(header,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -80), new Vector2(0, 160));

            // Title
            GameObject title = CreateOrFind(header.transform, "TitleText");
            SetupRectTransform(title,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 0), new Vector2(400, 60));
            SetupText(title, "FLASH TAP", 48, CYAN_NEON, FontStyles.Bold);

            // ========== ROUND BADGE ==========
            GameObject roundBadge = CreateOrFind(canvasTransform, "RoundBadge");
            SetupRectTransform(roundBadge,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -220), new Vector2(200, 60));

            // Badge background
            Image badgeBg = roundBadge.GetComponent<Image>();
            if (badgeBg == null) badgeBg = roundBadge.AddComponent<Image>();
            badgeBg.color = PANEL_BG;

            // Badge border
            GameObject badgeBorder = CreateOrFind(roundBadge.transform, "Border");
            SetupRectTransform(badgeBorder, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Outline badgeOutline = badgeBorder.GetComponent<Outline>();
            if (badgeOutline == null) badgeOutline = roundBadge.AddComponent<Outline>();
            badgeOutline.effectColor = CYAN_NEON;
            badgeOutline.effectDistance = new Vector2(2, 2);

            // Round text
            GameObject roundText = CreateOrFind(roundBadge.transform, "RoundText");
            SetupRectTransform(roundText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetupText(roundText, "Ronda 1/5", 28, Color.white, FontStyles.Normal);

            // ========== INSTRUCTION TEXT ==========
            GameObject instructionText = CreateOrFind(canvasTransform, "InstructionText");
            SetupRectTransform(instructionText,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 280), new Vector2(600, 100));
            SetupText(instructionText, "ESPERA...", 64, Color.white, FontStyles.Bold);

            // ========== TAP BUTTON (Neón Style) ==========
            GameObject tapButton = CreateOrFind(canvasTransform, "TapButton");
            SetupRectTransform(tapButton,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 0), new Vector2(350, 350));

            // Button component
            Button btn = tapButton.GetComponent<Button>();
            if (btn == null) btn = tapButton.AddComponent<Button>();

            // Outer glow ring
            GameObject outerRing = CreateOrFind(tapButton.transform, "OuterRing");
            SetupRectTransform(outerRing, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(40, 40));
            Image outerRingImg = outerRing.GetComponent<Image>();
            if (outerRingImg == null) outerRingImg = outerRing.AddComponent<Image>();
            outerRingImg.color = new Color(CYAN_NEON.r, CYAN_NEON.g, CYAN_NEON.b, 0.3f);
            outerRingImg.raycastTarget = false;

            // Main button background (the actual tap area)
            GameObject buttonBg = CreateOrFind(tapButton.transform, "ButtonBackground");
            SetupRectTransform(buttonBg, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image buttonBgImg = buttonBg.GetComponent<Image>();
            if (buttonBgImg == null) buttonBgImg = buttonBg.AddComponent<Image>();
            buttonBgImg.color = PANEL_BG;

            // Inner border (neon effect)
            GameObject innerBorder = CreateOrFind(tapButton.transform, "InnerBorder");
            SetupRectTransform(innerBorder, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-10, -10));
            Image innerBorderImg = innerBorder.GetComponent<Image>();
            if (innerBorderImg == null) innerBorderImg = innerBorder.AddComponent<Image>();
            innerBorderImg.color = CYAN_NEON;
            innerBorderImg.raycastTarget = false;

            // Inner fill
            GameObject innerFill = CreateOrFind(tapButton.transform, "InnerFill");
            SetupRectTransform(innerFill, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-20, -20));
            Image innerFillImg = innerFill.GetComponent<Image>();
            if (innerFillImg == null) innerFillImg = innerFill.AddComponent<Image>();
            innerFillImg.color = DARK_BG;
            innerFillImg.raycastTarget = false;

            // Tap text inside button
            GameObject tapText = CreateOrFind(tapButton.transform, "TapText");
            SetupRectTransform(tapText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetupText(tapText, "TAP", 72, CYAN_NEON, FontStyles.Bold);

            // Configure button
            btn.targetGraphic = buttonBgImg;
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            btn.colors = colors;

            // Store reference to inner border for color changes (TapButtonImage)
            // The controller uses tapButtonImage to change colors
            GameObject tapButtonImage = CreateOrFind(tapButton.transform, "TapButtonImage");
            SetupRectTransform(tapButtonImage, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-10, -10));
            Image tapBtnImg = tapButtonImage.GetComponent<Image>();
            if (tapBtnImg == null) tapBtnImg = tapButtonImage.AddComponent<Image>();
            tapBtnImg.color = WAIT_COLOR; // Default wait color
            tapBtnImg.raycastTarget = false;

            // Reorder children
            outerRing.transform.SetAsFirstSibling();
            buttonBg.transform.SetSiblingIndex(1);
            tapButtonImage.transform.SetSiblingIndex(2);
            innerFill.transform.SetSiblingIndex(3);
            tapText.transform.SetAsLastSibling();

            // Add effect component
            TapButtonEffect effect = tapButton.GetComponent<TapButtonEffect>();
            if (effect == null) effect = tapButton.AddComponent<TapButtonEffect>();

            // ========== STATS PANEL ==========
            GameObject statsPanel = CreateOrFind(canvasTransform, "StatsPanel");
            SetupRectTransform(statsPanel,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 280), new Vector2(500, 180));

            // Stats background
            Image statsBg = statsPanel.GetComponent<Image>();
            if (statsBg == null) statsBg = statsPanel.AddComponent<Image>();
            statsBg.color = PANEL_BG;

            // Stats border
            Outline statsOutline = statsPanel.GetComponent<Outline>();
            if (statsOutline == null) statsOutline = statsPanel.AddComponent<Outline>();
            statsOutline.effectColor = CYAN_DARK;
            statsOutline.effectDistance = new Vector2(2, 2);

            // Reaction time text (último tiempo)
            GameObject reactionTimeText = CreateOrFind(statsPanel.transform, "ReactionTimeText");
            SetupRectTransform(reactionTimeText,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -30), new Vector2(-40, 50));
            SetupText(reactionTimeText, "Último: --- ms", 32, CYAN_NEON, FontStyles.Normal);
            TextMeshProUGUI reactionTmp = reactionTimeText.GetComponent<TextMeshProUGUI>();
            if (reactionTmp != null) reactionTmp.alignment = TextAlignmentOptions.Center;

            // Average text
            GameObject averageText = CreateOrFind(statsPanel.transform, "AverageText");
            SetupRectTransform(averageText,
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 30), new Vector2(-40, 50));
            SetupText(averageText, "Promedio: --- ms", 32, Color.white, FontStyles.Normal);
            TextMeshProUGUI avgTmp = averageText.GetComponent<TextMeshProUGUI>();
            if (avgTmp != null) avgTmp.alignment = TextAlignmentOptions.Center;

            // Divider line
            GameObject divider = CreateOrFind(statsPanel.transform, "Divider");
            SetupRectTransform(divider,
                new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.5f),
                Vector2.zero, new Vector2(0, 2));
            Image dividerImg = divider.GetComponent<Image>();
            if (dividerImg == null) dividerImg = divider.AddComponent<Image>();
            dividerImg.color = CYAN_DARK;

            // ========== WIN PANEL ==========
            CreateWinPanel(canvasTransform);
        }

        private static void CreateWinPanel(Transform canvasTransform)
        {
            GameObject winPanel = CreateOrFind(canvasTransform, "WinPanel");
            SetupRectTransform(winPanel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Dark overlay
            Image overlayImg = winPanel.GetComponent<Image>();
            if (overlayImg == null) overlayImg = winPanel.AddComponent<Image>();
            overlayImg.color = new Color(0, 0, 0, 0.85f);

            // Content panel
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

            // Final score
            GameObject finalScore = CreateOrFind(content.transform, "FinalScoreText");
            SetupRectTransform(finalScore,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 40), new Vector2(400, 80));
            SetupText(finalScore, "Promedio Final", 36, Color.white, FontStyles.Normal);

            // Score value
            GameObject scoreValue = CreateOrFind(content.transform, "ScoreValue");
            SetupRectTransform(scoreValue,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -40), new Vector2(400, 100));
            SetupText(scoreValue, "--- ms", 64, CYAN_NEON, FontStyles.Bold);

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

            GameObject playBtnText = CreateOrFind(playAgainBtn.transform, "Text");
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
    }
}
