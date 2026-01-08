using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para reconstruir la UI de MemoryPairs con diseño profesional neón
    /// </summary>
    public class MemoryPairsUIBuilder : EditorWindow
    {
        // Colores del tema neón
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.4f, 1f);
        private static readonly Color DARK_BG = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color PANEL_BG = new Color(0.05f, 0.1f, 0.15f, 0.95f);
        private static readonly Color CARD_BG = new Color(0.08f, 0.12f, 0.18f, 1f);
        private static readonly Color CARD_FOUND = new Color(0f, 0.5f, 0.3f, 1f);
        private static readonly Color ERROR_COLOR = new Color(1f, 0.3f, 0.3f, 1f);
        private static readonly Color PAIRS_COLOR = new Color(0.3f, 1f, 0.5f, 1f);

        [MenuItem("DigitPark/Rebuild MemoryPairs UI")]
        public static void ShowWindow()
        {
            GetWindow<MemoryPairsUIBuilder>("MemoryPairs UI Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("MemoryPairs UI Builder", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Este script reconstruirá la UI de MemoryPairs.\n" +
                "Asegúrate de tener la escena MemoryPairs abierta.\n" +
                "Crea un grid 4x4 de cartas con estilo neón.",
                MessageType.Info);

            GUILayout.Space(10);

            if (GUILayout.Button("Reconstruir MemoryPairs UI", GUILayout.Height(40)))
            {
                RebuildMemoryPairsUI();
            }
        }

        private static void RebuildMemoryPairsUI()
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
            CreateMemoryPairsLayout(canvasTransform);

            Debug.Log("MemoryPairs UI reconstruida exitosamente!");
            EditorUtility.SetDirty(canvas.gameObject);
        }

        private static void CleanOldElements(Transform canvasTransform)
        {
            string[] oldElements = new string[]
            {
                "TimerText",
                "ErrorsText",
                "PairsFoundText",
                "TopUI",
                "CardsGrid",
                "WinPanel",
                // Cartas individuales viejas
                "Card_0", "Card_1", "Card_2", "Card_3",
                "Card_4", "Card_5", "Card_6", "Card_7",
                "Card_8", "Card_9", "Card_10", "Card_11",
                "Card_12", "Card_13", "Card_14", "Card_15",
                // Nuevos elementos
                "Header",
                "StatsBar"
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
        }

        private static void CreateMemoryPairsLayout(Transform canvasTransform)
        {
            // ========== HEADER ==========
            GameObject header = CreateOrFind(canvasTransform, "Header");
            SetupRectTransform(header,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -70), new Vector2(0, 140));

            GameObject title = CreateOrFind(header.transform, "TitleText");
            SetupRectTransform(title,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 0), new Vector2(500, 60));
            SetupText(title, "MEMORY PAIRS", 42, CYAN_NEON, FontStyles.Bold);

            // ========== STATS BAR ==========
            GameObject statsBar = CreateOrFind(canvasTransform, "StatsBar");
            SetupRectTransform(statsBar,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -180), new Vector2(-80, 60));

            Image statsBg = statsBar.GetComponent<Image>();
            if (statsBg == null) statsBg = statsBar.AddComponent<Image>();
            statsBg.color = PANEL_BG;

            HorizontalLayoutGroup statsLayout = statsBar.GetComponent<HorizontalLayoutGroup>();
            if (statsLayout == null) statsLayout = statsBar.AddComponent<HorizontalLayoutGroup>();
            statsLayout.childAlignment = TextAnchor.MiddleCenter;
            statsLayout.spacing = 50;
            statsLayout.padding = new RectOffset(40, 40, 10, 10);
            statsLayout.childForceExpandWidth = false;
            statsLayout.childForceExpandHeight = true;
            statsLayout.childControlWidth = true;
            statsLayout.childControlHeight = true;

            // Timer
            GameObject timerContainer = CreateOrFind(statsBar.transform, "TimerContainer");
            AddLayoutElement(timerContainer, 180, 40);

            GameObject timerIcon = CreateOrFind(timerContainer.transform, "Icon");
            SetupRectTransform(timerIcon, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(15, 0), new Vector2(30, 30));
            SetupText(timerIcon, "⏱", 22, CYAN_NEON, FontStyles.Normal);

            GameObject timerText = CreateOrFind(timerContainer.transform, "TimerText");
            SetupRectTransform(timerText, new Vector2(0, 0), new Vector2(1, 1), new Vector2(15, 0), new Vector2(-25, 0));
            SetupText(timerText, "00:00", 26, Color.white, FontStyles.Normal);
            timerText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;

            // Pairs Found
            GameObject pairsContainer = CreateOrFind(statsBar.transform, "PairsContainer");
            AddLayoutElement(pairsContainer, 180, 40);

            GameObject pairsIcon = CreateOrFind(pairsContainer.transform, "Icon");
            SetupRectTransform(pairsIcon, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(15, 0), new Vector2(30, 30));
            SetupText(pairsIcon, "🎴", 22, PAIRS_COLOR, FontStyles.Normal);

            GameObject pairsFoundText = CreateOrFind(pairsContainer.transform, "PairsFoundText");
            SetupRectTransform(pairsFoundText, new Vector2(0, 0), new Vector2(1, 1), new Vector2(15, 0), new Vector2(-25, 0));
            SetupText(pairsFoundText, "0/8", 26, PAIRS_COLOR, FontStyles.Bold);
            pairsFoundText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;

            // Errors
            GameObject errorsContainer = CreateOrFind(statsBar.transform, "ErrorsContainer");
            AddLayoutElement(errorsContainer, 120, 40);

            GameObject errorsIcon = CreateOrFind(errorsContainer.transform, "Icon");
            SetupRectTransform(errorsIcon, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(15, 0), new Vector2(30, 30));
            SetupText(errorsIcon, "✕", 22, ERROR_COLOR, FontStyles.Normal);

            GameObject errorsText = CreateOrFind(errorsContainer.transform, "ErrorsText");
            SetupRectTransform(errorsText, new Vector2(0, 0), new Vector2(1, 1), new Vector2(15, 0), new Vector2(-20, 0));
            SetupText(errorsText, "0", 26, ERROR_COLOR, FontStyles.Bold);
            errorsText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;

            // ========== CARDS GRID ==========
            GameObject cardsGrid = CreateOrFind(canvasTransform, "CardsGrid");
            SetupRectTransform(cardsGrid,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -50), new Vector2(720, 720));

            GridLayoutGroup gridLayout = cardsGrid.GetComponent<GridLayoutGroup>();
            if (gridLayout == null) gridLayout = cardsGrid.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(160, 160);
            gridLayout.spacing = new Vector2(15, 15);
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.MiddleCenter;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 4;

            // Crear 16 cartas (4x4)
            for (int i = 0; i < 16; i++)
            {
                CreateCard(cardsGrid.transform, i);
            }

            // ========== WIN PANEL ==========
            CreateWinPanel(canvasTransform);
        }

        private static void CreateCard(Transform parent, int index)
        {
            GameObject card = CreateOrFind(parent, $"Card_{index}");

            // Background
            Image cardBg = card.GetComponent<Image>();
            if (cardBg == null) cardBg = card.AddComponent<Image>();
            cardBg.color = CARD_BG;

            // Button
            Button btn = card.GetComponent<Button>();
            if (btn == null) btn = card.AddComponent<Button>();
            btn.targetGraphic = cardBg;

            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            btn.colors = colors;

            // Border (neón)
            Outline outline = card.GetComponent<Outline>();
            if (outline == null) outline = card.AddComponent<Outline>();
            outline.effectColor = CYAN_NEON;
            outline.effectDistance = new Vector2(2, 2);

            // Inner panel
            GameObject inner = CreateOrFind(card.transform, "Inner");
            SetupRectTransform(inner, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-8, -8));
            Image innerImg = inner.GetComponent<Image>();
            if (innerImg == null) innerImg = inner.AddComponent<Image>();
            innerImg.color = CARD_BG;
            innerImg.raycastTarget = false;

            // Card Image (para el sprite de la carta)
            GameObject cardImageObj = CreateOrFind(card.transform, $"CardImage_{index}");
            SetupRectTransform(cardImageObj, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-16, -16));
            Image cardImage = cardImageObj.GetComponent<Image>();
            if (cardImage == null) cardImage = cardImageObj.AddComponent<Image>();
            cardImage.color = Color.white;
            cardImage.raycastTarget = false;
            // El sprite se asignará en runtime

            // Card text (placeholder, se oculta cuando hay sprite)
            GameObject cardText = CreateOrFind(card.transform, $"CardText_{index}");
            SetupRectTransform(cardText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetupText(cardText, "?", 48, CYAN_DARK, FontStyles.Bold);

            // Add effect
            GameCardEffect effect = card.GetComponent<GameCardEffect>();
            if (effect == null) effect = card.AddComponent<GameCardEffect>();
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
                Vector2.zero, new Vector2(600, 450));

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
                new Vector2(0, -50), new Vector2(0, 70));
            SetupText(winTitle, "COMPLETADO!", 48, CYAN_NEON, FontStyles.Bold);

            // Stats
            GameObject statsText = CreateOrFind(content.transform, "StatsText");
            SetupRectTransform(statsText,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 20), new Vector2(400, 100));
            SetupText(statsText, "Tiempo: 00:00\nErrores: 0", 28, Color.white, FontStyles.Normal);

            // Play again button
            GameObject playAgainBtn = CreateOrFind(content.transform, "PlayAgainButton");
            SetupRectTransform(playAgainBtn,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 70), new Vector2(280, 65));

            Button playBtn = playAgainBtn.GetComponent<Button>();
            if (playBtn == null) playBtn = playAgainBtn.AddComponent<Button>();

            Image playBtnImg = playAgainBtn.GetComponent<Image>();
            if (playBtnImg == null) playBtnImg = playAgainBtn.AddComponent<Image>();
            playBtnImg.color = CYAN_NEON;
            playBtn.targetGraphic = playBtnImg;

            GameObject playBtnText = CreateOrFind(playAgainBtn.transform, "Text");
            SetupRectTransform(playBtnText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetupText(playBtnText, "Jugar de Nuevo", 26, DARK_BG, FontStyles.Bold);

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
