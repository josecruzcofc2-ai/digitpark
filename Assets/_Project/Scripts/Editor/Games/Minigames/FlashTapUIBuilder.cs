using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;
using DigitPark.Games;
using DigitPark.Editor.AutoAssigners;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para reconstruir la UI de FlashTap
    /// Settings panel, countdown, feedback, boton 3D naranja/rojo/verde, Win/Lose panels
    /// </summary>
    public class FlashTapUIBuilder : EditorWindow
    {
        // Colores del tema neon
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color GREEN_NEON = new Color(0.3f, 1f, 0.5f, 1f);
        private static readonly Color ORANGE_NEON = new Color(1f, 0.6f, 0.2f, 1f);
        private static readonly Color DARK_BG = new Color(0.02f, 0.04f, 0.08f, 1f);
        private static readonly Color PANEL_BG = new Color(0.05f, 0.1f, 0.15f, 0.95f);
        private static readonly Color BUTTON_BG = new Color(0.08f, 0.12f, 0.18f, 1f);
        private static readonly Color ERROR_COLOR = new Color(1f, 0.3f, 0.3f, 1f);

        // Rutas de sprites
        private const string ORANGE_UP_PATH = "Assets/_Project/Art/Icons/Games/FlashTap/ButtonFlashTap_Orange_Up.png";
        private const string ORANGE_DOWN_PATH = "Assets/_Project/Art/Icons/Games/FlashTap/ButtonFlashTap_Orange_Down.png";
        private const string RED_UP_PATH = "Assets/_Project/Art/Icons/Games/FlashTap/ButtonFlashTap_Red_Up.png";
        private const string RED_DOWN_PATH = "Assets/_Project/Art/Icons/Games/FlashTap/ButtonFlashTap_Red_Down.png";
        private const string GREEN_UP_PATH = "Assets/_Project/Art/Icons/Games/FlashTap/ButtonFlashTap_Green_Up.png";

        // Stats Bar icon paths
        private const string TIMER_ICON_PATH = "Assets/_Project/Art/Icons/UI/TimerIcon.png";
        private const string ROUND_ICON_PATH = "Assets/_Project/Art/Icons/UI/RoundIcon.png";
        private const string ERROR_ICON_PATH = "Assets/_Project/Art/Icons/UI/ErrorIcon.png";

        [MenuItem("DigitPark/Scenes/Build Scene/Games/FlashTap", false, 131)]
        public static void ShowWindow()
        {
            GetWindow<FlashTapUIBuilder>("FlashTap UI Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("FlashTap UI Builder", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "FlashTap con:\n" +
                "- Settings panel (rondas 3/5/10)\n" +
                "- Countdown 3-2-1-GO!\n" +
                "- Boton 3D naranja/rojo/verde\n" +
                "- Feedback correcto/incorrecto\n" +
                "- Win/Lose panels (global prefabs, not created here)\n\n" +
                "Asegurate de tener la escena FlashTap abierta.",
                MessageType.Info);

            GUILayout.Space(10);

            // Verificar sprites
            Sprite orangeUp = AssetDatabase.LoadAssetAtPath<Sprite>(ORANGE_UP_PATH);
            Sprite redUp = AssetDatabase.LoadAssetAtPath<Sprite>(RED_UP_PATH);
            Sprite greenUp = AssetDatabase.LoadAssetAtPath<Sprite>(GREEN_UP_PATH);

            EditorGUILayout.LabelField("Estado de Sprites:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Orange Up: {(orangeUp != null ? "OK" : "NO ENCONTRADO")}");
            EditorGUILayout.LabelField($"Red Up: {(redUp != null ? "OK" : "NO ENCONTRADO")}");
            EditorGUILayout.LabelField($"Green Up: {(greenUp != null ? "OK" : "NO ENCONTRADO")}");

            GUILayout.Space(10);

            GUI.enabled = orangeUp != null;
            if (GUILayout.Button("Reconstruir FlashTap UI", GUILayout.Height(40)))
            {
                RebuildFlashTapUI();
            }
            GUI.enabled = true;

            GUILayout.Space(15);
            GUI.backgroundColor = new Color(0.5f, 0.8f, 1f);
            if (GUILayout.Button("Auto-Asignar Referencias", GUILayout.Height(30)))
            {
                FlashTapReferenceAssigner.RunAutoAssign();
            }
            GUI.backgroundColor = Color.white;
        }

        private static void RebuildFlashTapUI()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) { Debug.LogError("No se encontro Canvas"); return; }

            // Standardize CanvasScaler
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
            }

            Transform root = canvas.transform;
            CleanOldElements(root);
            CreateFlashTapLayout(root);
            AssignControllerReferences(root);

            Debug.Log("FlashTap UI reconstruida con boton 3D naranja/rojo/verde!");
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
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

        private static void CleanOldElements(Transform root)
        {
            string[] keepElements = { "Background", "EventSystem", "FlashTapController" };

            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Transform child = root.GetChild(i);
                bool keep = false;
                foreach (string name in keepElements)
                    if (child.name == name) { keep = true; break; }

                // Keep controller GameObjects
                if (child.GetComponent<FlashTapController>() != null) keep = true;
                if (child.GetComponent<Canvas>() != null) keep = true;

                if (!keep)
                {
                    Debug.Log($"Limpiando: {child.name}");
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        private static void CreateFlashTapLayout(Transform root)
        {
            // ========== BACKGROUND ==========
            if (root.Find("Background") == null)
            {
                GameObject background = CreateElement(root, "Background");
                SetupRectTransform(background, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                Image bgImage = background.AddComponent<Image>();
                bgImage.color = DARK_BG;
                background.transform.SetAsFirstSibling();
            }

            // ========== SAFE AREA (contenedor principal) ==========
            GameObject safeArea = CreateElement(root, "SafeArea");
            SetupRectTransform(safeArea, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // ========== HEADER ==========
            GameObject header = CreateElement(safeArea.transform, "Header");
            SetupRectTransform(header,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -50), new Vector2(0, 100));

            GameObject title = CreateElement(header.transform, "TitleText");
            SetupRectTransform(title,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(400, 60));
            TextMeshProUGUI titleTmp = SetupText(title, "FLASH TAP", (int)FontSizes.H4, CYAN_NEON, FontStyles.Bold);

            Outline titleGlow = title.AddComponent<Outline>();
            titleGlow.effectColor = new Color(0f, 0.4f, 0.4f, 0.6f);
            titleGlow.effectDistance = new Vector2(2, -2);

            // ========== STATS BAR ==========
            CreateStatsBar(safeArea.transform);

            // ========== INSTRUCTION TEXT ==========
            GameObject instrText = CreateElement(safeArea.transform, "InstructionText");
            SetupRectTransform(instrText,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 320), new Vector2(600, 120));
            SetupText(instrText, "WAIT...", (int)FontSizes.H4, new Color(1f, 0.7f, 0.3f), FontStyles.Bold);

            // ========== TAP BUTTON 3D ==========
            Create3DButton(safeArea.transform);

            // ========== FEEDBACK PANEL ==========
            CreateFeedbackPanel(safeArea.transform);

            // ========== WIN/LOSE PANELS removed (now using global prefabs) ==========

            // ========== COUNTDOWN PANEL ==========
            CreateCountdownPanel(safeArea.transform);

            // ========== SETTINGS PANEL (encima de todo) ==========
            CreateSettingsPanel(safeArea.transform);
        }

        #region 3D Button

        private static void Create3DButton(Transform parent)
        {
            Sprite orangeUp = AssetDatabase.LoadAssetAtPath<Sprite>(ORANGE_UP_PATH);

            GameObject tapButton = CreateElement(parent, "TapButton3D");
            SetupRectTransform(tapButton,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -30), new Vector2(520, 520));

            // Transparent raycast target for FlashTapButton3D's IPointerDownHandler
            Image tapButtonImg = tapButton.AddComponent<Image>();
            tapButtonImg.color = new Color(0, 0, 0, 0);
            tapButtonImg.raycastTarget = true;

            // Imagen principal del boton
            GameObject buttonImage = CreateElement(tapButton.transform, "ButtonImage");
            SetupRectTransform(buttonImage,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(480, 480));
            Image btnImg = buttonImage.AddComponent<Image>();
            btnImg.sprite = orangeUp;
            btnImg.color = Color.white; // No tinting
            btnImg.preserveAspect = true;
            btnImg.raycastTarget = false; // FlashTapButton3D on parent handles pointer events

            Button btn = buttonImage.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.pressedColor = Color.white;
            colors.selectedColor = Color.white;
            btn.colors = colors;

            // FlashTapButton3D component
            FlashTapButton3D button3D = tapButton.AddComponent<FlashTapButton3D>();

            // Cargar todos los sprites
            Sprite orangeDown = AssetDatabase.LoadAssetAtPath<Sprite>(ORANGE_DOWN_PATH);
            Sprite redUp = AssetDatabase.LoadAssetAtPath<Sprite>(RED_UP_PATH);
            Sprite redDown = AssetDatabase.LoadAssetAtPath<Sprite>(RED_DOWN_PATH);
            Sprite greenUp = AssetDatabase.LoadAssetAtPath<Sprite>(GREEN_UP_PATH);

            SerializedObject so = new SerializedObject(button3D);
            so.FindProperty("waitUpSprite").objectReferenceValue = orangeUp;
            so.FindProperty("waitDownSprite").objectReferenceValue = orangeDown;
            so.FindProperty("readyUpSprite").objectReferenceValue = redUp;
            so.FindProperty("readyDownSprite").objectReferenceValue = redDown;
            so.FindProperty("successUpSprite").objectReferenceValue = greenUp;
            so.FindProperty("buttonImage").objectReferenceValue = btnImg;
            so.FindProperty("buttonTransform").objectReferenceValue = tapButton.GetComponent<RectTransform>();
            so.ApplyModifiedProperties();

            buttonImage.transform.SetAsLastSibling();
        }

        #endregion

        #region Stats Bar

        private static void CreateStatsBar(Transform parent)
        {
            GameObject statsBar = CreateElement(parent, "StatsBar");
            SetupRectTransform(statsBar,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -160), new Vector2(1020, 130));

            Image statsBg = statsBar.AddComponent<Image>();
            statsBg.color = PANEL_BG;

            Outline statsOutline = statsBar.AddComponent<Outline>();
            statsOutline.effectColor = CYAN_NEON;
            statsOutline.effectDistance = new Vector2(2, -2);

            HorizontalLayoutGroup layout = statsBar.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 20;
            layout.padding = new RectOffset(20, 20, 10, 10);
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            // Load icon sprites
            Sprite timerIcon = AssetDatabase.LoadAssetAtPath<Sprite>(TIMER_ICON_PATH);
            Sprite roundIcon = AssetDatabase.LoadAssetAtPath<Sprite>(ROUND_ICON_PATH);
            Sprite errorIcon = AssetDatabase.LoadAssetAtPath<Sprite>(ERROR_ICON_PATH);

            // Timer (reaction time)
            CreateStatItem(statsBar.transform, "TimerContainer", "TimerIcon", "ReactionTimeText",
                "0ms", Color.white, 380, timerIcon, 54, -130);

            // Round
            CreateStatItem(statsBar.transform, "RoundContainer", "RoundIcon", "RoundText",
                "1/5", CYAN_NEON, 260, roundIcon, 54, 0);

            // Errors
            CreateStatItem(statsBar.transform, "ErrorsContainer", "ErrorsIcon", "ErrorsText",
                "0", ERROR_COLOR, 200, errorIcon, 54, 130);
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

        #endregion

        #region Feedback Panel

        private static void CreateFeedbackPanel(Transform parent)
        {
            // Fullscreen stretch with dark overlay
            GameObject feedbackPanel = CreateElement(parent, "FeedbackPanel");
            SetupRectTransform(feedbackPanel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Image bg = feedbackPanel.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.7f);

            CanvasGroup cg = feedbackPanel.AddComponent<CanvasGroup>();
            cg.alpha = 0;

            // Content card (centered)
            GameObject card = CreateElement(feedbackPanel.transform, "FeedbackCard");
            SetupRectTransform(card, new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.65f), Vector2.zero, Vector2.zero);
            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.06f, 0.09f, 0.14f, 0.95f);
            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = CYAN_NEON;
            outline.effectDistance = new Vector2(2, -2);

            // FeedbackText inside card (stretch fill)
            GameObject feedbackText = CreateElement(card.transform, "FeedbackText");
            SetupRectTransform(feedbackText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI feedbackTmp = SetupText(feedbackText, "", (int)FontSizes.Body, GREEN_NEON, FontStyles.Bold);
            feedbackTmp.alignment = TextAlignmentOptions.Center;

            feedbackPanel.SetActive(false);
        }

        #endregion

        // Win/Lose Panels removed — now using global prefabs

        #region Countdown Panel

        private static void CreateCountdownPanel(Transform parent)
        {
            GameObject countdownPanel = CreateElement(parent, "CountdownPanel");
            SetupRectTransform(countdownPanel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            GameObject overlay = CreateElement(countdownPanel.transform, "Overlay");
            SetupRectTransform(overlay, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image overlayImg = overlay.AddComponent<Image>();
            overlayImg.color = new Color(0f, 0f, 0f, 0.6f);

            GameObject numberContainer = CreateElement(countdownPanel.transform, "NumberContainer");
            SetupRectTransform(numberContainer,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(400, 400));

            GameObject countdownText = CreateElement(numberContainer.transform, "CountdownText");
            SetupRectTransform(countdownText,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(350, 300));
            TextMeshProUGUI countTmp = SetupText(countdownText, "3", 180, CYAN_NEON, FontStyles.Bold);
            countTmp.alignment = TextAlignmentOptions.Center;

            Outline numOutline = countdownText.AddComponent<Outline>();
            numOutline.effectColor = new Color(0f, 0.4f, 0.5f, 0.8f);
            numOutline.effectDistance = new Vector2(4, -4);

            CountdownUI countdownUI = countdownPanel.AddComponent<CountdownUI>();
            SerializedObject so = new SerializedObject(countdownUI);
            so.FindProperty("countdownPanel").objectReferenceValue = countdownPanel;
            so.FindProperty("countdownText").objectReferenceValue = countTmp;
            so.FindProperty("backgroundOverlay").objectReferenceValue = overlayImg;
            so.FindProperty("numberColor").colorValue = CYAN_NEON;
            so.FindProperty("goColor").colorValue = GREEN_NEON;
            so.ApplyModifiedProperties();

            countdownPanel.SetActive(false);
        }

        #endregion

        #region Settings Panel

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
            GameObject titleObj = CreateElement(card.transform, "FlashTapTitle");
            SetupRectTransform(titleObj,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -32), new Vector2(0, 50));
            SetupText(titleObj, "FLASH TAP", (int)FontSizes.Subtitle, CYAN_NEON, FontStyles.Bold);

            Outline titleGlow = titleObj.AddComponent<Outline>();
            titleGlow.effectColor = new Color(0f, 0.5f, 0.5f, 0.6f);
            titleGlow.effectDistance = new Vector2(2, -2);

            // Subtitle
            GameObject subtitleObj = CreateElement(card.transform, "FlashTapSubtitle");
            SetupRectTransform(subtitleObj,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -70), new Vector2(0, 24));
            SetupText(subtitleObj, "Test your reflexes", (int)FontSizes.Body, new Color(0.5f, 0.5f, 0.6f), FontStyles.Bold);

            CreateDivider(card.transform, -95);

            // ====== ROUNDS SECTION ======
            float yPos = -115f;

            GameObject roundsHeader = CreateElement(card.transform, "RoundsHeader");
            SetupRectTransform(roundsHeader,
                new Vector2(0.05f, 1), new Vector2(0.95f, 1),
                new Vector2(0, yPos), new Vector2(0, 34));
            Image roundsHeaderBg = roundsHeader.AddComponent<Image>();
            roundsHeaderBg.color = new Color(0f, 0.12f, 0.08f, 0.5f);
            GameObject roundsHeaderText = CreateElement(roundsHeader.transform, "RoundsHeaderText");
            SetupRectTransform(roundsHeaderText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetupText(roundsHeaderText, "ROUNDS", (int)FontSizes.Body, new Color(0.7f, 1f, 0.8f), FontStyles.Bold);

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

            CreateSettingsToggle(roundsContainer.transform, "ToggleRounds1", "1", false);
            CreateSettingsToggle(roundsContainer.transform, "ToggleRounds3", "3", false);
            CreateSettingsToggle(roundsContainer.transform, "ToggleRounds5", "5", true);
            CreateSettingsToggle(roundsContainer.transform, "ToggleRounds10", "10", false);

            // ====== START BUTTON ======
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
            SetupText(startText, "START", (int)FontSizes.Body, DARK_BG, FontStyles.Bold);

            settingsPanel.SetActive(false);
        }

        #endregion

        #region Assign Controller References

        private static void AssignControllerReferences(Transform root)
        {
            FlashTapController controller = Object.FindObjectOfType<FlashTapController>();
            if (controller == null) { Debug.LogWarning("FlashTapController no encontrado"); return; }

            SerializedObject so = new SerializedObject(controller);

            // TapButton3D
            Transform tapButton3D = FindDeep(root, "TapButton3D");
            if (tapButton3D != null)
            {
                FlashTapButton3D btn3D = tapButton3D.GetComponent<FlashTapButton3D>();
                so.FindProperty("button3D").objectReferenceValue = btn3D;

                Button btn = tapButton3D.GetComponentInChildren<Button>();
                so.FindProperty("tapButton").objectReferenceValue = btn;
            }

            // UI Texts
            AssignTMPByFindDeep(so, "instructionText", root, "InstructionText");
            AssignTMPByFindDeep(so, "reactionTimeText", root, "ReactionTimeText");
            AssignTMPByFindDeep(so, "roundText", root, "RoundText");
            AssignTMPByFindDeep(so, "errorsText", root, "ErrorsText");
            AssignTMPByFindDeep(so, "roundIndicatorText", root, "RoundIndicator");

            // Countdown
            Transform countdownPanel = FindDeep(root, "CountdownPanel");
            if (countdownPanel != null)
                so.FindProperty("countdownUI").objectReferenceValue = countdownPanel.GetComponent<CountdownUI>();

            // Settings Panel
            AssignGameObject(so, "settingsPanel", FindDeep(root, "SettingsPanel")?.gameObject);
            AssignToggle(so, "toggleRounds1", FindDeep(root, "ToggleRounds1"));
            AssignToggle(so, "toggleRounds3", FindDeep(root, "ToggleRounds3"));
            AssignToggle(so, "toggleRounds5", FindDeep(root, "ToggleRounds5"));
            AssignToggle(so, "toggleRounds10", FindDeep(root, "ToggleRounds10"));

            Transform startBtn = FindDeep(root, "StartGameButton");
            if (startBtn != null)
                so.FindProperty("startGameButton").objectReferenceValue = startBtn.GetComponent<Button>();

            // Feedback
            AssignGameObject(so, "feedbackPanel", FindDeep(root, "FeedbackPanel")?.gameObject);
            AssignTMPByFindDeep(so, "feedbackText", root, "FeedbackText");

            // Win/Lose + Real Money panel assigners removed — now using global prefabs

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
            Debug.Log("FlashTapController configurado correctamente");
        }

        #endregion

        #region Helpers

        private static GameObject CreateElement(Transform parent, string name)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
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

        private static void CreateSettingsToggle(Transform parent, string name, string label, bool isOn)
        {
            GameObject toggleObj = CreateElement(parent, name);

            Image toggleBg = toggleObj.AddComponent<Image>();
            toggleBg.color = isOn ? CYAN_NEON : BUTTON_BG;

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

        // CreatePanelButton removed (was only used by Win/Lose panels)

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

        private static TextMeshProUGUI CreateStatRow(Transform parent, string label, string valueName, string defaultValue, Color valueColor, float yPos)
        {
            GameObject labelObj = CreateElement(parent, valueName + "_Label");
            SetupRectTransform(labelObj,
                new Vector2(0.05f, 1), new Vector2(0.5f, 1),
                new Vector2(0, yPos), new Vector2(0, 38));
            TextMeshProUGUI labelTmp = SetupText(labelObj, label, 24, new Color(0.6f, 0.65f, 0.75f), FontStyles.Bold);
            labelTmp.alignment = TextAlignmentOptions.Left;

            GameObject valueObj = CreateElement(parent, valueName);
            SetupRectTransform(valueObj,
                new Vector2(0.5f, 1), new Vector2(0.95f, 1),
                new Vector2(0, yPos), new Vector2(0, 38));
            TextMeshProUGUI valueTmp = SetupText(valueObj, defaultValue, 28, valueColor, FontStyles.Bold);
            valueTmp.alignment = TextAlignmentOptions.Right;

            return valueTmp;
        }

        private static Transform FindDeep(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                Transform result = FindDeep(child, name);
                if (result != null) return result;
            }
            return null;
        }

        private static void AssignTMPByFindDeep(SerializedObject so, string propertyName, Transform root, string objectName)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop == null) return;
            Transform t = FindDeep(root, objectName);
            if (t != null)
                prop.objectReferenceValue = t.GetComponent<TextMeshProUGUI>();
        }

        private static void AssignToggle(SerializedObject so, string propertyName, Transform toggleTransform)
        {
            if (toggleTransform == null) return;
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop != null)
                prop.objectReferenceValue = toggleTransform.GetComponent<Toggle>();
        }

        private static void AssignGameObject(SerializedObject so, string propertyName, GameObject obj)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop != null)
                prop.objectReferenceValue = obj;
        }

        #endregion
    }
}
