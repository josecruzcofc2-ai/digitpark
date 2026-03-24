using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;
using DigitPark.Editor.AutoAssigners;

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
        private static readonly Color DARK_BG = new Color(0.02f, 0.04f, 0.08f, 1f);
        private static readonly Color PANEL_BG = new Color(0.05f, 0.1f, 0.15f, 0.95f);
        private static readonly Color CARD_BG = new Color(0.08f, 0.12f, 0.2f, 1f);
        private static readonly Color CARD_PRESSED = new Color(0.04f, 0.06f, 0.1f, 1f);
        private static readonly Color CARD_FOUND = new Color(0.1f, 0.3f, 0.15f, 1f);

        // Rutas de iconos para Stats Bar
        private const string TIMER_ICON_PATH = "Assets/_Project/Art/Icons/UI/TimerIcon.png";
        private const string ROUND_ICON_PATH = "Assets/_Project/Art/Icons/UI/RoundIcon.png";
        private const string PAIRS_ICON_PATH = "Assets/_Project/Art/Icons/UI/PairsIcon.png";
        private const string ERROR_ICON_PATH = "Assets/_Project/Art/Icons/UI/ErrorIcon.png";

        [MenuItem("DigitPark/Scenes/Build Scene/Games/MemoryPairs", false, 132)]
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

            GUILayout.Space(15);
            GUI.backgroundColor = new Color(0.5f, 0.8f, 1f);
            if (GUILayout.Button("Auto-Asignar Referencias", GUILayout.Height(30)))
            {
                MemoryPairsReferenceAssigner.RunAutoAssign();
            }
            GUI.backgroundColor = Color.white;
        }

        private static void RebuildMemoryPairsUI()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("[MemoryPairsUIBuilder] No se encontró Canvas en la escena");
                return;
            }

            // Standardize CanvasScaler
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
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
            string[] keepElements = {
                "Main Camera", "EventSystem",
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

        private static void CreateMemoryPairsLayout(Transform canvasTransform)
        {
            // ========== BACKGROUND ==========
            GameObject background = CreateElement(canvasTransform, "Background");
            SetupRectTransform(background, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = Color.white; // ThemeApplier tints at runtime
            background.transform.SetAsFirstSibling();

            // ========== SAFE AREA ==========
            GameObject safeArea = CreateElement(canvasTransform, "SafeArea");
            SetupRectTransform(safeArea, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            safeArea.AddComponent<SafeAreaHandler>();

            // ========== HEADER ==========
            CreateHeader(safeArea.transform);

            // ========== STATS BAR ==========
            CreateStatsBar(safeArea.transform);

            // ========== COMBO TEXT ==========
            CreateComboText(safeArea.transform);

            // ========== CARDS GRID (Centro - Elemento principal) ==========
            CreateCardsGrid(safeArea.transform);

            // ========== FEEDBACK PANEL (Below grid - Correcto/Incorrecto toast) ==========
            CreateFeedbackPanel(safeArea.transform);

            // ========== ACTION BUTTON + WIN/LOSE PANELS removed (now using global prefabs) ==========

            // ========== COUNTDOWN PANEL ==========
            CreateCountdownPanel(safeArea.transform);

            // ========== SETTINGS PANEL (encima de todo) ==========
            CreateSettingsPanel(safeArea.transform);

            // ========== PARTICLE EFFECTS CONTAINER ==========
            CreateParticleEffects(safeArea.transform);
        }

        private static void CreateComboText(Transform parent)
        {
            GameObject comboObj = CreateElement(parent, "ComboText");
            SetupRectTransform(comboObj,
                new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-80, -240), new Vector2(120, 60));

            TextMeshProUGUI comboTmp = SetupText(comboObj, "x2", (int)FontSizes.Subtitle, GREEN_NEON, FontStyles.Bold);
            comboTmp.alignment = TextAlignmentOptions.Center;

            Outline comboOutline = comboObj.AddComponent<Outline>();
            comboOutline.effectColor = new Color(0f, 0.3f, 0.1f, 1f);
            comboOutline.effectDistance = new Vector2(3, -3);

            comboObj.SetActive(false); // Hidden by default
        }

        private static void CreateFeedbackPanel(Transform parent)
        {
            // Fullscreen stretch with dark overlay
            GameObject feedbackPanel = CreateElement(parent, "FeedbackPanel");
            SetupRectTransform(feedbackPanel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Image bg = feedbackPanel.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.7f);

            // Content card (centered)
            GameObject card = CreateElement(feedbackPanel.transform, "FeedbackCard");
            SetupRectTransform(card, new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.65f), Vector2.zero, Vector2.zero);
            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.06f, 0.09f, 0.14f, 0.95f);
            Outline panelOutline = card.AddComponent<Outline>();
            panelOutline.effectColor = CYAN_NEON;
            panelOutline.effectDistance = new Vector2(2, -2);

            // FeedbackText inside card (stretch fill)
            GameObject feedbackText = CreateElement(card.transform, "FeedbackText");
            SetupRectTransform(feedbackText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI feedbackTmp = SetupText(feedbackText, "", (int)FontSizes.Subtitle, GREEN_NEON, FontStyles.Bold);
            feedbackTmp.alignment = TextAlignmentOptions.Center;
            feedbackTmp.enableWordWrapping = false;

            feedbackPanel.SetActive(false); // Hidden by default, shown by controller
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
                new Vector2(0, -79), new Vector2(0, 100));

            Image headerBg = header.AddComponent<Image>();
            headerBg.color = new Color(0f, 0f, 0f, 0.3f);

            // Title
            GameObject title = CreateElement(header.transform, "TitleText");
            SetupRectTransform(title,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 0), new Vector2(600, 70));
            SetupText(title, "MEMORY PAIRS", (int)FontSizes.H4, CYAN_NEON, FontStyles.Bold);
        }

        private static void CreateStatsBar(Transform parent)
        {
            GameObject statsBar = CreateElement(parent, "StatsBar");
            SetupRectTransform(statsBar,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -190), new Vector2(1020, 130));

            Image statsBg = statsBar.AddComponent<Image>();
            statsBg.color = PANEL_BG;

            Outline statsOutline = statsBar.AddComponent<Outline>();
            statsOutline.effectColor = CYAN_NEON;
            statsOutline.effectDistance = new Vector2(2, -2);

            HorizontalLayoutGroup layout = statsBar.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 15;
            layout.padding = new RectOffset(15, 15, 10, 10);
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            // Cargar iconos
            Sprite timerIcon = AssetDatabase.LoadAssetAtPath<Sprite>(TIMER_ICON_PATH);
            Sprite roundIcon = AssetDatabase.LoadAssetAtPath<Sprite>(ROUND_ICON_PATH);
            Sprite pairsIcon = AssetDatabase.LoadAssetAtPath<Sprite>(PAIRS_ICON_PATH);
            Sprite errorIcon = AssetDatabase.LoadAssetAtPath<Sprite>(ERROR_ICON_PATH);

            // Timer
            CreateStatItem(statsBar.transform, "TimerContainer", "TimerIcon", "TimerText",
                "00:00", Color.white, 280, timerIcon, 54, -130);

            // Round
            CreateStatItem(statsBar.transform, "RoundContainer", "RoundIcon", "RoundText",
                "1/1", CYAN_NEON, 200, roundIcon, 54, -45);

            // Pairs Found
            CreateStatItem(statsBar.transform, "PairsContainer", "PairsIcon", "PairsFoundText",
                "0/8", GREEN_NEON, 210, pairsIcon, 54, 45);

            // Errors
            CreateStatItem(statsBar.transform, "ErrorsContainer", "ErrorsIcon", "ErrorsText",
                "0", ERROR_RED, 170, errorIcon, 54, 130);
        }

        private static void CreateStatItem(Transform parent, string containerName, string iconName,
            string textName, string defaultText, Color color, float width, Sprite iconSprite = null, int fontSize = 54, float extraOffsetX = 0)
        {
            GameObject container = CreateElement(parent, containerName);

            LayoutElement le = container.AddComponent<LayoutElement>();
            le.preferredWidth = width;
            le.preferredHeight = 100;

            // Icon
            GameObject icon = CreateElement(container.transform, iconName);
            SetupRectTransform(icon,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(10 + extraOffsetX, 0), new Vector2(81, 81));
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
                new Vector2(50 + extraOffsetX, 0), new Vector2(-5, 0));
            TextMeshProUGUI tmp = SetupText(text, defaultText, fontSize, color, FontStyles.Bold);
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
            panelOutline.effectColor = new Color(0f, 1f, 1f, 0.4f); // Cyan neon
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
            sideImg.color = new Color(0f, 0.4f, 0.5f, 1f); // Cyan oscuro

            // 3. FACE
            GameObject face = CreateElement(card.transform, "Face");
            SetupRectTransform(face,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 4), new Vector2(200, 200));
            Image faceImg = face.AddComponent<Image>();
            faceImg.color = CARD_BG;

            // Neon outline
            Outline faceOutline = face.AddComponent<Outline>();
            faceOutline.effectColor = CYAN_NEON;
            faceOutline.effectDistance = new Vector2(2, -2);

            // 4. CARD IMAGE (for the card sprite - back/front)
            GameObject cardImageObj = CreateElement(face.transform, $"CardImage_{index}");
            SetupRectTransform(cardImageObj,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(180, 180));
            Image cardImage = cardImageObj.AddComponent<Image>();
            cardImage.color = Color.clear; // Transparente hasta que se asigne sprite
            cardImage.raycastTarget = false;
            // Sprite assigned at runtime

            // 5. DIGIT TEXT (muestra "?" cuando oculto, dígito cuando revelado)
            GameObject digitText = CreateElement(face.transform, $"CardText_{index}");
            SetupRectTransform(digitText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI qText = SetupText(digitText, "?", (int)FontSizes.Symbol, new Color(0f, 1f, 1f, 0.8f), FontStyles.Bold);
            qText.alignment = TextAlignmentOptions.Center;

            // Agregar outline/glow al "?"
            Outline qOutline = digitText.AddComponent<Outline>();
            qOutline.effectColor = new Color(0f, 0.5f, 0.5f, 0.6f);
            qOutline.effectDistance = new Vector2(2, -2);

            // Button on card container
            Button button = card.AddComponent<Button>();
            button.targetGraphic = faceImg;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.7f, 1f, 1f, 1f); // Cyan highlight
            colors.pressedColor = new Color(0f, 0.8f, 0.8f, 1f); // Cyan pressed
            colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
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

            // Explicitly set all color properties to enforce cyan neon theme
            // This prevents old scene-serialized values from overriding script defaults
            so.FindProperty("faceDownColor").colorValue = new Color(0.04f, 0.08f, 0.12f, 1f);
            so.FindProperty("faceDownSideColor").colorValue = new Color(0.02f, 0.05f, 0.08f, 1f);
            so.FindProperty("faceDownGlowColor").colorValue = new Color(0f, 0.6f, 0.6f, 0.3f);
            so.FindProperty("faceDownPatternColor").colorValue = new Color(0f, 0.8f, 0.8f, 0.15f);
            so.FindProperty("faceUpColor").colorValue = new Color(0.06f, 0.12f, 0.18f, 1f);
            so.FindProperty("faceUpSideColor").colorValue = new Color(0.04f, 0.08f, 0.12f, 1f);
            so.FindProperty("faceUpGlowColor").colorValue = new Color(0f, 1f, 1f, 1f); // Cyan neon
            so.FindProperty("symbolColor").colorValue = Color.white;
            so.FindProperty("matchedFaceColor").colorValue = new Color(0.05f, 0.25f, 0.1f, 1f);
            so.FindProperty("matchedSideColor").colorValue = new Color(0.03f, 0.15f, 0.05f, 1f);
            so.FindProperty("matchedGlowColor").colorValue = new Color(0.2f, 1f, 0.4f, 1f);
            so.FindProperty("matchedSymbolColor").colorValue = new Color(0.5f, 1f, 0.6f, 1f);
            so.FindProperty("matchFlashColor").colorValue = new Color(0.4f, 1f, 0.6f, 1f);
            so.FindProperty("comboGlowColor").colorValue = new Color(1f, 0.85f, 0.2f, 1f);
            so.FindProperty("comboFlashColor").colorValue = new Color(1f, 0.95f, 0.5f, 1f);
            so.FindProperty("errorFaceColor").colorValue = new Color(0.35f, 0.05f, 0.05f, 1f);
            so.FindProperty("errorSideColor").colorValue = new Color(0.25f, 0.03f, 0.03f, 1f);
            so.FindProperty("errorGlowColor").colorValue = new Color(1f, 0.2f, 0.2f, 1f);
            so.FindProperty("errorSymbolColor").colorValue = new Color(1f, 0.5f, 0.5f, 1f);
            so.FindProperty("hoverGlowColor").colorValue = new Color(0.4f, 1f, 1f, 0.8f);

            so.ApplyModifiedProperties();
        }

        // CreateActionButton (PlayAgainButton), CreateNormalWinPanel, CreateNormalLosePanel removed — now using global prefabs

        // CreatePanelButton removed (was only used by Win/Lose panels)

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
            TextMeshProUGUI countTmp = SetupText(countdownText, "3", 180, CYAN_NEON, FontStyles.Bold);
            countTmp.alignment = TextAlignmentOptions.Center;

            Outline numOutline = countdownText.AddComponent<Outline>();
            numOutline.effectColor = new Color(0f, 0.4f, 0.5f, 0.8f); // Cyan oscuro
            numOutline.effectDistance = new Vector2(4, -4);

            // Add CountdownUI component
            DigitPark.UI.CountdownUI countdownUI = countdownPanel.AddComponent<DigitPark.UI.CountdownUI>();

            SerializedObject so = new SerializedObject(countdownUI);
            so.FindProperty("countdownPanel").objectReferenceValue = countdownPanel;
            so.FindProperty("countdownText").objectReferenceValue = countTmp;
            so.FindProperty("backgroundOverlay").objectReferenceValue = overlayImg;
            so.FindProperty("numberColor").colorValue = CYAN_NEON;
            so.FindProperty("goColor").colorValue = GREEN_NEON;
            so.ApplyModifiedProperties();

            countdownPanel.SetActive(false);
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
                new Vector2(0, 20), new Vector2(780, 400));

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.04f, 0.08f, 0.14f, 0.98f);

            Outline cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = CYAN_NEON;
            cardOutline.effectDistance = new Vector2(3, -3);

            // Title
            GameObject titleObj = CreateElement(card.transform, "MemoryPairsTitle");
            SetupRectTransform(titleObj,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -32), new Vector2(0, 50));
            SetupText(titleObj, "MEMORY PAIRS", (int)FontSizes.Subtitle, CYAN_NEON, FontStyles.Bold);

            Outline titleGlow = titleObj.AddComponent<Outline>();
            titleGlow.effectColor = new Color(0f, 0.5f, 0.5f, 0.6f);
            titleGlow.effectDistance = new Vector2(2, -2);

            // Subtitle
            GameObject subtitleObj = CreateElement(card.transform, "MemoryPairsSubtitle");
            SetupRectTransform(subtitleObj,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -70), new Vector2(0, 24));
            SetupText(subtitleObj, "Find all matching pairs", (int)FontSizes.Body, new Color(0.5f, 0.5f, 0.6f), FontStyles.Bold);

            // Divider
            CreateDivider(card.transform, -95);

            // ====== ROUNDS SECTION ======
            float yPos = -115f;

            GameObject roundsHeader = CreateElement(card.transform, "RoundsHeader");
            SetupRectTransform(roundsHeader,
                new Vector2(0.05f, 1), new Vector2(0.95f, 1),
                new Vector2(0, yPos), new Vector2(0, 36));
            Image roundsHeaderBg = roundsHeader.AddComponent<Image>();
            roundsHeaderBg.color = new Color(0f, 0.12f, 0.08f, 0.5f);
            GameObject roundsHeaderText = CreateElement(roundsHeader.transform, "RoundsHeaderText");
            SetupRectTransform(roundsHeaderText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetupText(roundsHeaderText, "ROUNDS", (int)FontSizes.Body, new Color(0.7f, 1f, 0.8f), FontStyles.Bold);

            yPos -= 75f;

            GameObject roundsContainer = CreateElement(card.transform, "RoundsContainer");
            SetupRectTransform(roundsContainer,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, yPos), new Vector2(680, 80));

            HorizontalLayoutGroup roundsLayout = roundsContainer.AddComponent<HorizontalLayoutGroup>();
            roundsLayout.childAlignment = TextAnchor.MiddleCenter;
            roundsLayout.spacing = 20;
            roundsLayout.childForceExpandWidth = true;
            roundsLayout.childForceExpandHeight = true;

            CreateSettingsToggle(roundsContainer.transform, "ToggleRounds1", "1", true);
            CreateSettingsToggle(roundsContainer.transform, "ToggleRounds3", "3", false);
            CreateSettingsToggle(roundsContainer.transform, "ToggleRounds5", "5", false);

            // ====== START BUTTON ======
            GameObject startBtn = CreateElement(card.transform, "StartGameButton");
            SetupRectTransform(startBtn,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -330), new Vector2(700, 68));

            // Shadow
            GameObject startShadow = CreateElement(startBtn.transform, "Shadow");
            SetupRectTransform(startShadow,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(3, -6), new Vector2(700, 68));
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
            SetupText(startText, "START", (int)FontSizes.Body, DARK_BG, FontStyles.Bold);

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
            TextMeshProUGUI labelTmp = SetupText(labelObj, label, (int)FontSizes.Body, isOn ? DARK_BG : Color.white, FontStyles.Bold);
            labelTmp.raycastTarget = false;
        }

        private static void CreateDivider(Transform parent, float yPos)
        {
            GameObject divider = CreateElement(parent, "Divider");
            SetupRectTransform(divider,
                new Vector2(0.08f, 1), new Vector2(0.92f, 1),
                new Vector2(0, yPos), new Vector2(0, 2));
            Image divImg = divider.AddComponent<Image>();
            divImg.color = new Color(1f, 1f, 1f, 0.1f);
            divImg.raycastTarget = false;
        }

        private static void AssignControllerReferences()
        {
            var controller = FindFirstObjectByType<DigitPark.Games.MemoryPairsController>();
            if (controller == null)
            {
                Debug.LogWarning("[MemoryPairsUIBuilder] No se encontró MemoryPairsController en la escena");
                return;
            }

            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            Transform root = canvas != null ? canvas.transform : null;
            if (root == null) return;

            SerializedObject so = new SerializedObject(controller);

            // Card buttons, images y Card3DEffects
            Transform cardsGrid = FindDeep(root, "CardsGrid");
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
            AssignTMPReference(so, root, "timerText", "TimerText");

            // Pairs Found Text
            AssignTMPReference(so, root, "pairsFoundText", "PairsFoundText");

            // Errors Text (stats bar)
            AssignTMPReference(so, root, "errorsText", "ErrorsText");

            // Countdown UI (inactive - FindDeep works with inactive objects)
            Transform countdownPanelT = FindDeep(root, "CountdownPanel");
            if (countdownPanelT != null)
            {
                SerializedProperty countdownProp = so.FindProperty("countdownUI");
                if (countdownProp != null)
                    countdownProp.objectReferenceValue = countdownPanelT.GetComponent<DigitPark.UI.CountdownUI>();
            }

            // Combo Text
            AssignTMPReference(so, root, "comboText", "ComboText");

            // Feedback Panel + Text (inactive - FindDeep required)
            Transform feedbackPanelT = FindDeep(root, "FeedbackPanel");
            if (feedbackPanelT != null)
            {
                SerializedProperty feedbackPanelProp = so.FindProperty("feedbackPanel");
                if (feedbackPanelProp != null)
                    feedbackPanelProp.objectReferenceValue = feedbackPanelT.gameObject;

                Transform feedbackTextT = FindDeep(feedbackPanelT, "FeedbackText");
                if (feedbackTextT != null)
                {
                    SerializedProperty feedbackTextProp = so.FindProperty("feedbackText");
                    if (feedbackTextProp != null)
                        feedbackTextProp.objectReferenceValue = feedbackTextT.GetComponent<TextMeshProUGUI>();
                }
            }

            // Round Text
            AssignTMPReference(so, root, "roundText", "RoundText");

            // Round Indicator
            AssignTMPReference(so, root, "roundIndicatorText", "RoundIndicator");

            // Settings Panel
            Transform settingsPanelT = FindDeep(root, "SettingsPanel");
            if (settingsPanelT != null)
            {
                SerializedProperty settingsProp = so.FindProperty("settingsPanel");
                if (settingsProp != null)
                    settingsProp.objectReferenceValue = settingsPanelT.gameObject;

                AssignToggle(so, "toggleRounds1", FindDeep(settingsPanelT, "ToggleRounds1"));
                AssignToggle(so, "toggleRounds3", FindDeep(settingsPanelT, "ToggleRounds3"));
                AssignToggle(so, "toggleRounds5", FindDeep(settingsPanelT, "ToggleRounds5"));

                Transform startBtnT = FindDeep(settingsPanelT, "StartGameButton");
                if (startBtnT != null)
                {
                    SerializedProperty startBtnProp = so.FindProperty("startGameButton");
                    if (startBtnProp != null)
                        startBtnProp.objectReferenceValue = startBtnT.GetComponent<Button>();
                }
            }

            // Sparkle Effect
            Transform particleEffectsT = FindDeep(root, "ParticleEffects");
            if (particleEffectsT != null)
            {
                SerializedProperty sparkleProp = so.FindProperty("sparkleEffect");
                if (sparkleProp != null)
                    sparkleProp.objectReferenceValue = particleEffectsT.GetComponent<DigitPark.UI.UISparkleEffect>();
            }

            // Win/Lose + Real Money panel assigners removed — now using global prefabs

            so.ApplyModifiedProperties();
            Debug.Log("[MemoryPairsUIBuilder] Referencias asignadas al MemoryPairsController");
        }

        private static void AssignToggle(SerializedObject so, string propertyName, Transform toggleTransform)
        {
            if (toggleTransform == null) return;
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop != null)
                prop.objectReferenceValue = toggleTransform.GetComponent<Toggle>();
        }

        /// <summary>
        /// Assigns a TMP text reference by traversing hierarchy (works with inactive objects)
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
        /// Recursively finds a child Transform by name (works with inactive objects)
        /// </summary>
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
            tmp.raycastTarget = false;

            return tmp;
        }

        #endregion
    }
}
