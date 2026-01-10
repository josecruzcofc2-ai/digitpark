using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;
using DigitPark.Games;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para reconstruir la UI de FlashTap con diseño profesional
    /// Usa el boton 3D con sprites Up/Down y estilo neon para la UI
    /// </summary>
    public class FlashTapUIBuilder : EditorWindow
    {
        // Colores del tema neon para UI
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.3f, 0.3f, 1f);
        private static readonly Color DARK_BG = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color PANEL_BG = new Color(0.05f, 0.1f, 0.15f, 0.95f);

        // Rutas de los sprites del boton 3D
        private const string BUTTON_UP_PATH = "Assets/_Project/Art/Icons/Games/ButtonFlashTap_Up.png";
        private const string BUTTON_DOWN_PATH = "Assets/_Project/Art/Icons/Games/ButtonFlashTap_Down.png";

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
                "Este script reconstruira la UI de FlashTap con:\n" +
                "- Boton 3D con sprites Up/Down\n" +
                "- Estilo neon para UI\n" +
                "- Animaciones mejoradas\n\n" +
                "Asegurate de tener la escena FlashTap abierta.",
                MessageType.Info);

            GUILayout.Space(10);

            // Mostrar estado de los sprites
            Sprite upSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BUTTON_UP_PATH);
            Sprite downSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BUTTON_DOWN_PATH);

            EditorGUILayout.LabelField("Estado de Sprites:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Button Up: {(upSprite != null ? "OK" : "NO ENCONTRADO")}");
            EditorGUILayout.LabelField($"Button Down: {(downSprite != null ? "OK" : "NO ENCONTRADO")}");

            GUILayout.Space(10);

            GUI.enabled = upSprite != null && downSprite != null;
            if (GUILayout.Button("Reconstruir FlashTap UI", GUILayout.Height(40)))
            {
                RebuildFlashTapUI();
            }
            GUI.enabled = true;

            if (upSprite == null || downSprite == null)
            {
                EditorGUILayout.HelpBox(
                    "Faltan sprites del boton 3D.\n" +
                    "Asegurate de que existan:\n" +
                    "- ButtonFlashTap_Up.png\n" +
                    "- ButtonFlashTap_Down.png\n" +
                    "en Assets/_Project/Art/Icons/Games/",
                    MessageType.Warning);
            }
        }

        private static void RebuildFlashTapUI()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No se encontro Canvas en la escena");
                return;
            }

            Transform canvasTransform = canvas.transform;

            // Verificar Background
            Transform background = canvasTransform.Find("Background");
            if (background == null)
            {
                Debug.LogError("No se encontro Background");
                return;
            }

            // LIMPIAR elementos viejos primero
            CleanOldElements(canvasTransform);

            // Crear nueva estructura
            CreateFlashTapLayout(canvasTransform);

            // Configurar el controller
            SetupController(canvasTransform);

            Debug.Log("FlashTap UI reconstruida exitosamente con boton 3D!");
            EditorUtility.SetDirty(canvas.gameObject);
        }

        private static void CleanOldElements(Transform canvasTransform)
        {
            string[] oldElements = new string[]
            {
                "RoundText",
                "ReactionTimeText",
                "AverageText",
                "TitleText",
                "Header",
                "RoundBadge",
                "InstructionText",
                "TapButton",
                "TapButton3D",
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
                new Vector2(0, -220), new Vector2(280, 80));

            Image badgeBg = roundBadge.GetComponent<Image>();
            if (badgeBg == null) badgeBg = roundBadge.AddComponent<Image>();
            badgeBg.color = PANEL_BG;

            Outline badgeOutline = roundBadge.GetComponent<Outline>();
            if (badgeOutline == null) badgeOutline = roundBadge.AddComponent<Outline>();
            badgeOutline.effectColor = CYAN_NEON;
            badgeOutline.effectDistance = new Vector2(2, 2);

            GameObject roundText = CreateOrFind(roundBadge.transform, "RoundText");
            SetupRectTransform(roundText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetupText(roundText, "Ronda 1/5", 38, Color.white, FontStyles.Bold);

            // ========== INSTRUCTION TEXT ==========
            GameObject instructionText = CreateOrFind(canvasTransform, "InstructionText");
            SetupRectTransform(instructionText,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 350), new Vector2(600, 120));
            SetupText(instructionText, "ESPERA...", 82, Color.white, FontStyles.Bold);

            // ========== TAP BUTTON 3D ==========
            Create3DButton(canvasTransform);

            // ========== STATS PANEL ==========
            CreateStatsPanel(canvasTransform);

            // ========== WIN PANEL ==========
            CreateWinPanel(canvasTransform);
        }

        private static void Create3DButton(Transform canvasTransform)
        {
            // Cargar sprites
            Sprite upSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BUTTON_UP_PATH);
            Sprite downSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BUTTON_DOWN_PATH);

            // Container del boton
            GameObject tapButton = CreateOrFind(canvasTransform, "TapButton3D");
            SetupRectTransform(tapButton,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -30), new Vector2(520, 520));

            // Glow Ring (solo como contenedor, sin imagen de fondo)
            GameObject glowRing = CreateOrFind(tapButton.transform, "GlowRing");
            SetupRectTransform(glowRing,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(550, 550));
            // Eliminar Image si existe (no queremos fondo)
            Image glowImg = glowRing.GetComponent<Image>();
            if (glowImg != null) DestroyImmediate(glowImg);
            glowImg = null; // El glow sera manejado programaticamente con efectos

            // Imagen principal del boton 3D
            GameObject buttonImage = CreateOrFind(tapButton.transform, "ButtonImage");
            SetupRectTransform(buttonImage,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(480, 480));
            Image btnImg = buttonImage.GetComponent<Image>();
            if (btnImg == null) btnImg = buttonImage.AddComponent<Image>();
            btnImg.sprite = upSprite;
            btnImg.color = new Color(0.5f, 0.5f, 0.5f, 1f); // Gris inicial (estado Wait)
            btnImg.preserveAspect = true;
            btnImg.raycastTarget = true;

            // Button component en la imagen
            Button btn = buttonImage.GetComponent<Button>();
            if (btn == null) btn = buttonImage.AddComponent<Button>();
            btn.targetGraphic = btnImg;

            // Desactivar transiciones de color del Button (lo manejamos nosotros)
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.pressedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            btn.colors = colors;

            // Agregar componente FlashTapButton3D
            FlashTapButton3D button3D = tapButton.GetComponent<FlashTapButton3D>();
            if (button3D == null) button3D = tapButton.AddComponent<FlashTapButton3D>();

            // Configurar referencias via SerializedObject
            SerializedObject so = new SerializedObject(button3D);
            so.FindProperty("buttonUpSprite").objectReferenceValue = upSprite;
            so.FindProperty("buttonDownSprite").objectReferenceValue = downSprite;
            so.FindProperty("buttonImage").objectReferenceValue = btnImg;
            // glowRing ya no tiene Image, se omite (null check en FlashTapButton3D)
            so.FindProperty("buttonTransform").objectReferenceValue = tapButton.GetComponent<RectTransform>();
            so.ApplyModifiedProperties();

            // Reordenar
            glowRing.transform.SetAsFirstSibling();
            buttonImage.transform.SetAsLastSibling();
        }

        private static void CreateStatsPanel(Transform canvasTransform)
        {
            GameObject statsPanel = CreateOrFind(canvasTransform, "StatsPanel");
            SetupRectTransform(statsPanel,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 320), new Vector2(500, 200));

            Image statsBg = statsPanel.GetComponent<Image>();
            if (statsBg == null) statsBg = statsPanel.AddComponent<Image>();
            statsBg.color = PANEL_BG;

            Outline statsOutline = statsPanel.GetComponent<Outline>();
            if (statsOutline == null) statsOutline = statsPanel.AddComponent<Outline>();
            statsOutline.effectColor = CYAN_DARK;
            statsOutline.effectDistance = new Vector2(2, 2);

            // Reaction time text (ultimo tiempo)
            GameObject reactionTimeText = CreateOrFind(statsPanel.transform, "ReactionTimeText");
            SetupRectTransform(reactionTimeText,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -35), new Vector2(-40, 60));
            SetupText(reactionTimeText, "", 36, CYAN_NEON, FontStyles.Bold);
            TextMeshProUGUI reactionTmp = reactionTimeText.GetComponent<TextMeshProUGUI>();
            if (reactionTmp != null) reactionTmp.alignment = TextAlignmentOptions.Center;

            // Divider line
            GameObject divider = CreateOrFind(statsPanel.transform, "Divider");
            SetupRectTransform(divider,
                new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.5f),
                Vector2.zero, new Vector2(0, 2));
            Image dividerImg = divider.GetComponent<Image>();
            if (dividerImg == null) dividerImg = divider.AddComponent<Image>();
            dividerImg.color = CYAN_DARK;

            // Average text
            GameObject averageText = CreateOrFind(statsPanel.transform, "AverageText");
            SetupRectTransform(averageText,
                new Vector2(0, 0), new Vector2(0.5f, 0.5f),
                new Vector2(20, 10), new Vector2(-20, 50));
            SetupText(averageText, "Promedio: ---", 34, Color.white, FontStyles.Normal);
            TextMeshProUGUI avgTmp = averageText.GetComponent<TextMeshProUGUI>();
            if (avgTmp != null) avgTmp.alignment = TextAlignmentOptions.Left;

            // Best time text
            GameObject bestTimeText = CreateOrFind(statsPanel.transform, "BestTimeText");
            SetupRectTransform(bestTimeText,
                new Vector2(0.5f, 0), new Vector2(1f, 0.5f),
                new Vector2(-20, 10), new Vector2(-20, 50));
            SetupText(bestTimeText, "Mejor: ---", 34, new Color(0f, 1f, 0.5f), FontStyles.Normal);
            TextMeshProUGUI bestTmp = bestTimeText.GetComponent<TextMeshProUGUI>();
            if (bestTmp != null) bestTmp.alignment = TextAlignmentOptions.Right;
        }

        private static void CreateWinPanel(Transform canvasTransform)
        {
            GameObject winPanel = CreateOrFind(canvasTransform, "WinPanel");
            SetupRectTransform(winPanel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Image overlayImg = winPanel.GetComponent<Image>();
            if (overlayImg == null) overlayImg = winPanel.AddComponent<Image>();
            overlayImg.color = new Color(0, 0, 0, 0.9f);

            // Content panel
            GameObject content = CreateOrFind(winPanel.transform, "Content");
            SetupRectTransform(content,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(600, 550));

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
            SetupText(winTitle, "COMPLETADO!", 52, CYAN_NEON, FontStyles.Bold);

            // Final score label
            GameObject finalScoreLabel = CreateOrFind(content.transform, "FinalScoreLabel");
            SetupRectTransform(finalScoreLabel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 80), new Vector2(400, 50));
            SetupText(finalScoreLabel, "Tiempo Promedio", 32, Color.white, FontStyles.Normal);

            // Score value
            GameObject scoreValue = CreateOrFind(content.transform, "ScoreValue");
            SetupRectTransform(scoreValue,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 20), new Vector2(400, 80));
            SetupText(scoreValue, "--- ms", 64, CYAN_NEON, FontStyles.Bold);

            // Rating text
            GameObject ratingText = CreateOrFind(content.transform, "RatingText");
            SetupRectTransform(ratingText,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -50), new Vector2(400, 50));
            SetupText(ratingText, "", 28, new Color(0f, 1f, 0.5f), FontStyles.Italic);

            // Play again button
            GameObject playAgainBtn = CreateOrFind(content.transform, "PlayAgainButton");
            SetupRectTransform(playAgainBtn,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 100), new Vector2(320, 80));

            Button playBtn = playAgainBtn.GetComponent<Button>();
            if (playBtn == null) playBtn = playAgainBtn.AddComponent<Button>();

            Image playBtnImg = playAgainBtn.GetComponent<Image>();
            if (playBtnImg == null) playBtnImg = playAgainBtn.AddComponent<Image>();
            playBtnImg.color = CYAN_NEON;

            playBtn.targetGraphic = playBtnImg;

            GameObject playBtnText = CreateOrFind(playAgainBtn.transform, "Text");
            SetupRectTransform(playBtnText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetupText(playBtnText, "Jugar de Nuevo", 30, DARK_BG, FontStyles.Bold);

            winPanel.SetActive(false);
        }

        private static void SetupController(Transform canvasTransform)
        {
            // Buscar el controller
            FlashTapController controller = FindObjectOfType<FlashTapController>();
            if (controller == null)
            {
                Debug.LogWarning("No se encontro FlashTapController en la escena");
                return;
            }

            // Cargar sprites
            Sprite upSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BUTTON_UP_PATH);
            Sprite downSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BUTTON_DOWN_PATH);

            // Buscar elementos
            Transform tapButton3D = canvasTransform.Find("TapButton3D");
            Transform instructionText = canvasTransform.Find("InstructionText");
            Transform statsPanel = canvasTransform.Find("StatsPanel");
            Transform roundBadge = canvasTransform.Find("RoundBadge");
            Transform winPanel = canvasTransform.Find("WinPanel");

            // Configurar via SerializedObject
            SerializedObject so = new SerializedObject(controller);

            if (tapButton3D != null)
            {
                FlashTapButton3D btn3D = tapButton3D.GetComponent<FlashTapButton3D>();
                so.FindProperty("button3D").objectReferenceValue = btn3D;

                // Tambien configurar el Button como fallback
                Button btn = tapButton3D.GetComponentInChildren<Button>();
                so.FindProperty("tapButton").objectReferenceValue = btn;
            }

            so.FindProperty("buttonUpSprite").objectReferenceValue = upSprite;
            so.FindProperty("buttonDownSprite").objectReferenceValue = downSprite;

            if (instructionText != null)
                so.FindProperty("instructionText").objectReferenceValue = instructionText.GetComponent<TextMeshProUGUI>();

            if (statsPanel != null)
            {
                Transform reactionTime = statsPanel.Find("ReactionTimeText");
                Transform average = statsPanel.Find("AverageText");
                Transform bestTime = statsPanel.Find("BestTimeText");

                if (reactionTime != null)
                    so.FindProperty("reactionTimeText").objectReferenceValue = reactionTime.GetComponent<TextMeshProUGUI>();
                if (average != null)
                    so.FindProperty("averageText").objectReferenceValue = average.GetComponent<TextMeshProUGUI>();
                if (bestTime != null)
                    so.FindProperty("bestTimeText").objectReferenceValue = bestTime.GetComponent<TextMeshProUGUI>();
            }

            if (roundBadge != null)
            {
                Transform roundText = roundBadge.Find("RoundText");
                if (roundText != null)
                    so.FindProperty("roundText").objectReferenceValue = roundText.GetComponent<TextMeshProUGUI>();
            }

            if (winPanel != null)
                so.FindProperty("winPanel").objectReferenceValue = winPanel.gameObject;

            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(controller);
            Debug.Log("FlashTapController configurado correctamente");
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
