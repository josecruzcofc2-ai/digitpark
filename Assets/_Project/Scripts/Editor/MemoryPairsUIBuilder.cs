using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para reconstruir la UI de MemoryPairs con diseño profesional neón 3D
    /// Optimizado para formato portrait 9:16 (1080x1920)
    /// Grid 4x4 con cartas 3D estilo botón presionable
    /// </summary>
    public class MemoryPairsUIBuilder : EditorWindow
    {
        // Colores del tema neón
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color MAGENTA_NEON = new Color(1f, 0f, 0.8f, 1f);
        private static readonly Color GREEN_NEON = new Color(0.3f, 1f, 0.5f, 1f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color ERROR_RED = new Color(1f, 0.3f, 0.3f, 1f);
        private static readonly Color DARK_BG = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color PANEL_BG = new Color(0.05f, 0.1f, 0.15f, 0.95f);
        private static readonly Color CARD_BG = new Color(0.08f, 0.12f, 0.2f, 1f);
        private static readonly Color CARD_PRESSED = new Color(0.04f, 0.06f, 0.1f, 1f);
        private static readonly Color CARD_FOUND = new Color(0.1f, 0.3f, 0.15f, 1f);

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
                "Diseño 3D optimizado para portrait 9:16 (1080x1920).",
                MessageType.Info);

            GUILayout.Space(10);

            if (GUILayout.Button("Reconstruir MemoryPairs UI", GUILayout.Height(40)))
            {
                RebuildMemoryPairsUI();
            }
        }

        private static void RebuildMemoryPairsUI()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[MemoryPairsUIBuilder] No se encontró Canvas en la escena");
                return;
            }

            Transform canvasTransform = canvas.transform;

            // Limpiar elementos viejos
            CleanOldElements(canvasTransform);

            // Crear nueva estructura
            CreateMemoryPairsLayout(canvasTransform);

            // Asignar referencias al controller
            AssignControllerReferences();

            Debug.Log("[MemoryPairsUIBuilder] MemoryPairs UI reconstruida exitosamente!");
            EditorUtility.SetDirty(canvas.gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

        private static void CleanOldElements(Transform canvasTransform)
        {
            string[] keepElements = { "Main Camera", "EventSystem" };

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

        private static void CreateMemoryPairsLayout(Transform canvasTransform)
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

            // ========== HEADER ==========
            CreateHeader(safeArea.transform);

            // ========== STATS BAR ==========
            CreateStatsBar(safeArea.transform);

            // ========== COMBO TEXT ==========
            CreateComboText(safeArea.transform);

            // ========== CARDS GRID (Centro - Elemento principal) ==========
            CreateCardsGrid(safeArea.transform);

            // ========== ACTION BUTTON ==========
            CreateActionButton(safeArea.transform);

            // ========== WIN PANEL ==========
            CreateWinPanel(safeArea.transform);

            // ========== COUNTDOWN PANEL ==========
            CreateCountdownPanel(safeArea.transform);

            // ========== PARTICLE EFFECTS CONTAINER ==========
            CreateParticleEffects(safeArea.transform);
        }

        private static void CreateComboText(Transform parent)
        {
            GameObject comboObj = CreateElement(parent, "ComboText");
            SetupRectTransform(comboObj,
                new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-80, -240), new Vector2(120, 60));

            TextMeshProUGUI comboTmp = SetupText(comboObj, "x2", 42, GREEN_NEON, FontStyles.Bold);
            comboTmp.alignment = TextAlignmentOptions.Center;

            Outline comboOutline = comboObj.AddComponent<Outline>();
            comboOutline.effectColor = new Color(0f, 0.3f, 0.1f, 1f);
            comboOutline.effectDistance = new Vector2(3, -3);

            comboObj.SetActive(false); // Hidden by default
        }

        private static void CreateParticleEffects(Transform parent)
        {
            GameObject particleContainer = CreateElement(parent, "ParticleEffects");
            SetupRectTransform(particleContainer, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Add UISparkleEffect component
            particleContainer.AddComponent<DigitPark.UI.UISparkleEffect>();
        }

        private static void CreateHeader(Transform parent)
        {
            GameObject header = CreateElement(parent, "Header");
            SetupRectTransform(header,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -50), new Vector2(0, 100));

            Image headerBg = header.AddComponent<Image>();
            headerBg.color = new Color(0f, 0f, 0f, 0.3f);

            // Title
            GameObject title = CreateElement(header.transform, "TitleText");
            SetupRectTransform(title,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 0), new Vector2(600, 70));
            SetupText(title, "MEMORY PAIRS", 46, MAGENTA_NEON, FontStyles.Bold);

            // Subtitle
            GameObject subtitle = CreateElement(header.transform, "SubtitleText");
            SetupRectTransform(subtitle,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 10), new Vector2(400, 25));
            SetupText(subtitle, "Encuentra todos los pares", 18, Color.white, FontStyles.Italic);
        }

        private static void CreateStatsBar(Transform parent)
        {
            GameObject statsBar = CreateElement(parent, "StatsBar");
            SetupRectTransform(statsBar,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -160), new Vector2(900, 70));

            Image statsBg = statsBar.AddComponent<Image>();
            statsBg.color = PANEL_BG;

            Outline statsOutline = statsBar.AddComponent<Outline>();
            statsOutline.effectColor = MAGENTA_NEON;
            statsOutline.effectDistance = new Vector2(2, -2);

            HorizontalLayoutGroup layout = statsBar.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 60;
            layout.padding = new RectOffset(40, 40, 10, 10);
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            // Timer
            CreateStatItem(statsBar.transform, "TimerContainer", "TimerIcon", "TimerText",
                "00:00", CYAN_NEON, 180);

            // Pairs Found
            CreateStatItem(statsBar.transform, "PairsContainer", "PairsIcon", "PairsFoundText",
                "0/8", GREEN_NEON, 150);

            // Errors
            CreateStatItem(statsBar.transform, "ErrorsContainer", "ErrorsIcon", "ErrorsText",
                "0", ERROR_RED, 100);
        }

        private static void CreateStatItem(Transform parent, string containerName, string iconName,
            string textName, string defaultText, Color color, float width)
        {
            GameObject container = CreateElement(parent, containerName);

            LayoutElement le = container.AddComponent<LayoutElement>();
            le.preferredWidth = width;
            le.preferredHeight = 50;

            // Icon (Image component for custom icon)
            GameObject icon = CreateElement(container.transform, iconName);
            SetupRectTransform(icon,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(15, 0), new Vector2(32, 32));
            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = color;

            // Text
            GameObject text = CreateElement(container.transform, textName);
            SetupRectTransform(text,
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(25, 0), new Vector2(-30, 0));
            TextMeshProUGUI tmp = SetupText(text, defaultText, 28, color, FontStyles.Bold);
            tmp.alignment = TextAlignmentOptions.Left;
        }

        private static void CreateCardsGrid(Transform parent)
        {
            GameObject gamePanel = CreateElement(parent, "GamePanel");
            SetupRectTransform(gamePanel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 20), new Vector2(950, 950));

            // Background
            Image panelBg = gamePanel.AddComponent<Image>();
            panelBg.color = new Color(0.03f, 0.06f, 0.12f, 0.8f);

            Outline panelOutline = gamePanel.AddComponent<Outline>();
            panelOutline.effectColor = new Color(1f, 0f, 0.8f, 0.4f);
            panelOutline.effectDistance = new Vector2(3, -3);

            // Grid Container
            GameObject cardsGrid = CreateElement(gamePanel.transform, "CardsGrid");
            SetupRectTransform(cardsGrid,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(900, 900));

            GridLayoutGroup grid = cardsGrid.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(210, 210);
            grid.spacing = new Vector2(12, 12);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;
            grid.padding = new RectOffset(8, 8, 8, 8);

            // Create 16 cards (4x4)
            for (int i = 0; i < 16; i++)
            {
                Create3DCard(cardsGrid.transform, i);
            }
        }

        private static void Create3DCard(Transform parent, int index)
        {
            // ========== 3D CARD STRUCTURE ==========
            GameObject card = CreateElement(parent, $"Card_{index}");

            // Card base is transparent
            Image cardBase = card.AddComponent<Image>();
            cardBase.color = Color.clear;

            // 1. SHADOW
            GameObject shadow = CreateElement(card.transform, "Shadow");
            SetupRectTransform(shadow,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(4, -10), new Vector2(200, 200));
            Image shadowImg = shadow.AddComponent<Image>();
            shadowImg.color = new Color(0f, 0f, 0f, 0.4f);

            // 2. SIDE (depth)
            GameObject side = CreateElement(card.transform, "Side");
            SetupRectTransform(side,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 0), new Vector2(200, 10));
            Image sideImg = side.AddComponent<Image>();
            sideImg.color = new Color(0.5f, 0f, 0.4f, 1f); // Magenta oscuro

            // 3. FACE
            GameObject face = CreateElement(card.transform, "Face");
            SetupRectTransform(face,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 4), new Vector2(200, 200));
            Image faceImg = face.AddComponent<Image>();
            faceImg.color = CARD_BG;

            // Neon outline
            Outline faceOutline = face.AddComponent<Outline>();
            faceOutline.effectColor = MAGENTA_NEON;
            faceOutline.effectDistance = new Vector2(2, -2);

            // 4. CARD IMAGE (for the card sprite - back/front)
            GameObject cardImageObj = CreateElement(face.transform, $"CardImage_{index}");
            SetupRectTransform(cardImageObj,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(180, 180));
            Image cardImage = cardImageObj.AddComponent<Image>();
            cardImage.color = Color.white;
            cardImage.raycastTarget = false;
            // Sprite assigned at runtime

            // 5. DIGIT TEXT (muestra "?" cuando oculto, dígito cuando revelado)
            GameObject digitText = CreateElement(face.transform, $"CardText_{index}");
            SetupRectTransform(digitText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI qText = SetupText(digitText, "?", 90, new Color(1f, 0f, 0.8f, 0.3f), FontStyles.Bold);
            qText.alignment = TextAlignmentOptions.Center;

            // Button on card container
            Button button = card.AddComponent<Button>();
            button.targetGraphic = faceImg;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.9f, 1f, 1f);
            colors.pressedColor = new Color(0.8f, 0.7f, 0.9f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            colors.fadeDuration = 0.05f;
            button.colors = colors;

            // Add Card3DEffect component
            DigitPark.UI.Card3DEffect card3D = card.AddComponent<DigitPark.UI.Card3DEffect>();

            // Assign references via SerializedObject
            SerializedObject so = new SerializedObject(card3D);
            so.FindProperty("cardFace").objectReferenceValue = face.GetComponent<RectTransform>();
            so.FindProperty("shadowImage").objectReferenceValue = shadowImg;
            so.FindProperty("sideImage").objectReferenceValue = sideImg;
            so.FindProperty("faceImage").objectReferenceValue = faceImg;
            so.FindProperty("cardImage").objectReferenceValue = cardImage;
            so.FindProperty("symbolText").objectReferenceValue = qText;
            so.ApplyModifiedProperties();
        }

        private static void CreateActionButton(Transform parent)
        {
            GameObject playAgainBtn = CreateElement(parent, "PlayAgainButton");
            SetupRectTransform(playAgainBtn,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 120), new Vector2(500, 100));

            Image btnBg = playAgainBtn.AddComponent<Image>();
            btnBg.color = GREEN_NEON;

            Outline btnOutline = playAgainBtn.AddComponent<Outline>();
            btnOutline.effectColor = new Color(0.1f, 0.5f, 0.2f, 1f);
            btnOutline.effectDistance = new Vector2(3, -3);

            Button btn = playAgainBtn.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.8f, 1f, 0.9f, 1f);
            colors.pressedColor = new Color(0.6f, 0.8f, 0.7f, 1f);
            btn.colors = colors;

            GameObject textObj = CreateElement(playAgainBtn.transform, "Text");
            SetupRectTransform(textObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI tmp = SetupText(textObj, "JUGAR DE NUEVO", 32, DARK_BG, FontStyles.Bold);
            tmp.alignment = TextAlignmentOptions.Center;

            // Hidden by default (shown after win)
            playAgainBtn.SetActive(false);
        }

        private static void CreateWinPanel(Transform parent)
        {
            GameObject winPanel = CreateElement(parent, "WinPanel");
            SetupRectTransform(winPanel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Overlay
            Image overlay = winPanel.AddComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0.85f);

            // Content
            GameObject content = CreateElement(winPanel.transform, "Content");
            SetupRectTransform(content,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(700, 500));

            Image contentBg = content.AddComponent<Image>();
            contentBg.color = PANEL_BG;

            Outline contentOutline = content.AddComponent<Outline>();
            contentOutline.effectColor = GREEN_NEON;
            contentOutline.effectDistance = new Vector2(4, -4);

            // Win Title
            GameObject winTitle = CreateElement(content.transform, "WinTitle");
            SetupRectTransform(winTitle,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -60), new Vector2(500, 80));
            SetupText(winTitle, "COMPLETADO!", 52, GREEN_NEON, FontStyles.Bold);

            // Stats
            GameObject statsText = CreateElement(content.transform, "StatsText");
            SetupRectTransform(statsText,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 20), new Vector2(500, 150));
            TextMeshProUGUI statsTmp = SetupText(statsText, "Tiempo: 00:00\nErrores: 0", 32, Color.white, FontStyles.Normal);
            statsTmp.alignment = TextAlignmentOptions.Center;

            // Play again button in win panel
            GameObject winPlayBtn = CreateElement(content.transform, "WinPlayAgainButton");
            SetupRectTransform(winPlayBtn,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 80), new Vector2(350, 80));

            Image winBtnBg = winPlayBtn.AddComponent<Image>();
            winBtnBg.color = GREEN_NEON;

            Button winBtn = winPlayBtn.AddComponent<Button>();

            GameObject winBtnText = CreateElement(winPlayBtn.transform, "Text");
            SetupRectTransform(winBtnText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetupText(winBtnText, "JUGAR DE NUEVO", 28, DARK_BG, FontStyles.Bold);

            winPanel.SetActive(false);
        }

        private static void CreateCountdownPanel(Transform parent)
        {
            GameObject countdownPanel = CreateElement(parent, "CountdownPanel");
            SetupRectTransform(countdownPanel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Overlay
            GameObject overlay = CreateElement(countdownPanel.transform, "Overlay");
            SetupRectTransform(overlay, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image overlayImg = overlay.AddComponent<Image>();
            overlayImg.color = new Color(0f, 0f, 0f, 0.6f);

            // Number container
            GameObject numberContainer = CreateElement(countdownPanel.transform, "NumberContainer");
            SetupRectTransform(numberContainer,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(400, 400));

            // Countdown text
            GameObject countdownText = CreateElement(numberContainer.transform, "CountdownText");
            SetupRectTransform(countdownText,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(350, 300));
            TextMeshProUGUI countTmp = SetupText(countdownText, "3", 180, MAGENTA_NEON, FontStyles.Bold);
            countTmp.alignment = TextAlignmentOptions.Center;

            Outline numOutline = countdownText.AddComponent<Outline>();
            numOutline.effectColor = new Color(0.5f, 0f, 0.4f, 0.8f);
            numOutline.effectDistance = new Vector2(4, -4);

            // Add CountdownUI component
            DigitPark.UI.CountdownUI countdownUI = countdownPanel.AddComponent<DigitPark.UI.CountdownUI>();

            SerializedObject so = new SerializedObject(countdownUI);
            so.FindProperty("countdownPanel").objectReferenceValue = countdownPanel;
            so.FindProperty("countdownText").objectReferenceValue = countTmp;
            so.FindProperty("backgroundOverlay").objectReferenceValue = overlayImg;
            so.FindProperty("numberColor").colorValue = MAGENTA_NEON;
            so.FindProperty("goColor").colorValue = GREEN_NEON;
            so.ApplyModifiedProperties();

            countdownPanel.SetActive(false);
        }

        private static void AssignControllerReferences()
        {
            var controller = FindFirstObjectByType<DigitPark.Games.MemoryPairsController>();
            if (controller == null)
            {
                Debug.LogWarning("[MemoryPairsUIBuilder] No se encontró MemoryPairsController en la escena");
                return;
            }

            SerializedObject so = new SerializedObject(controller);

            // Card buttons, images y Card3DEffects
            Transform cardsGrid = GameObject.Find("CardsGrid")?.transform;
            if (cardsGrid != null)
            {
                SerializedProperty cardButtonsProp = so.FindProperty("cardButtons");
                SerializedProperty cardImagesProp = so.FindProperty("cardImages");
                SerializedProperty card3DEffectsProp = so.FindProperty("card3DEffects");

                if (cardButtonsProp != null)
                {
                    cardButtonsProp.arraySize = 16;
                    for (int i = 0; i < 16; i++)
                    {
                        Transform card = cardsGrid.Find($"Card_{i}");
                        if (card != null)
                        {
                            cardButtonsProp.GetArrayElementAtIndex(i).objectReferenceValue = card.GetComponent<Button>();
                        }
                    }
                }

                if (cardImagesProp != null)
                {
                    cardImagesProp.arraySize = 16;
                    for (int i = 0; i < 16; i++)
                    {
                        Transform card = cardsGrid.Find($"Card_{i}");
                        if (card != null)
                        {
                            Transform cardImage = card.Find($"Face/CardImage_{i}");
                            if (cardImage != null)
                            {
                                cardImagesProp.GetArrayElementAtIndex(i).objectReferenceValue = cardImage.GetComponent<Image>();
                            }
                        }
                    }
                }

                if (card3DEffectsProp != null)
                {
                    card3DEffectsProp.arraySize = 16;
                    for (int i = 0; i < 16; i++)
                    {
                        Transform card = cardsGrid.Find($"Card_{i}");
                        if (card != null)
                        {
                            card3DEffectsProp.GetArrayElementAtIndex(i).objectReferenceValue =
                                card.GetComponent<DigitPark.UI.Card3DEffect>();
                        }
                    }
                }
            }

            // Timer Text
            GameObject timerText = GameObject.Find("TimerText");
            if (timerText != null)
            {
                SerializedProperty timerProp = so.FindProperty("timerText");
                if (timerProp != null)
                    timerProp.objectReferenceValue = timerText.GetComponent<TextMeshProUGUI>();
            }

            // Pairs Found Text
            GameObject pairsText = GameObject.Find("PairsFoundText");
            if (pairsText != null)
            {
                SerializedProperty pairsProp = so.FindProperty("pairsFoundText");
                if (pairsProp != null)
                    pairsProp.objectReferenceValue = pairsText.GetComponent<TextMeshProUGUI>();
            }

            // Errors Text
            GameObject errorsText = GameObject.Find("ErrorsText");
            if (errorsText != null)
            {
                SerializedProperty errorsProp = so.FindProperty("errorsText");
                if (errorsProp != null)
                    errorsProp.objectReferenceValue = errorsText.GetComponent<TextMeshProUGUI>();
            }

            // Win Panel
            GameObject winPanel = GameObject.Find("WinPanel");
            if (winPanel != null)
            {
                SerializedProperty winProp = so.FindProperty("winPanel");
                if (winProp != null)
                    winProp.objectReferenceValue = winPanel;
            }

            // Countdown UI
            GameObject countdownPanel = GameObject.Find("CountdownPanel");
            if (countdownPanel != null)
            {
                SerializedProperty countdownProp = so.FindProperty("countdownUI");
                if (countdownProp != null)
                    countdownProp.objectReferenceValue = countdownPanel.GetComponent<DigitPark.UI.CountdownUI>();
            }

            // Combo Text
            GameObject comboText = GameObject.Find("ComboText");
            if (comboText != null)
            {
                SerializedProperty comboProp = so.FindProperty("comboText");
                if (comboProp != null)
                    comboProp.objectReferenceValue = comboText.GetComponent<TextMeshProUGUI>();
            }

            // Sparkle Effect
            GameObject particleEffects = GameObject.Find("ParticleEffects");
            if (particleEffects != null)
            {
                SerializedProperty sparkleProp = so.FindProperty("sparkleEffect");
                if (sparkleProp != null)
                    sparkleProp.objectReferenceValue = particleEffects.GetComponent<DigitPark.UI.UISparkleEffect>();
            }

            so.ApplyModifiedProperties();
            Debug.Log("[MemoryPairsUIBuilder] Referencias asignadas al MemoryPairsController");
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
            tmp.raycastTarget = false;

            return tmp;
        }

        #endregion
    }
}
