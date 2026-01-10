using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script to build the Matchmaking scene UI with professional neon style
    /// VS screen with player cards, searching animation, and countdown
    /// </summary>
    public class MatchmakingUIBuilder : EditorWindow
    {
        // Neon Colors
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.3f, 0.4f);
        private static readonly Color CYAN_GLOW = new Color(0f, 0.8f, 0.9f, 0.6f);
        private static readonly Color PURPLE_NEON = new Color(0.7f, 0.3f, 1f);
        private static readonly Color GREEN_NEON = new Color(0.2f, 1f, 0.4f);
        private static readonly Color ORANGE_NEON = new Color(1f, 0.6f, 0.2f);
        private static readonly Color DARK_BG = new Color(0.05f, 0.08f, 0.12f);
        private static readonly Color CARD_BG = new Color(0.08f, 0.12f, 0.18f);

        [MenuItem("DigitPark/Build Matchmaking UI")]
        public static void BuildUI()
        {
            // Find or create Canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasGO = new GameObject("Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
                canvasGO.AddComponent<GraphicRaycaster>();
            }

            // Clear existing UI
            foreach (Transform child in canvas.transform)
            {
                DestroyImmediate(child.gameObject);
            }

            // Create main container
            GameObject mainContainer = CreateElement(canvas.transform, "MainContainer");
            RectTransform mainRect = mainContainer.GetComponent<RectTransform>();
            mainRect.anchorMin = Vector2.zero;
            mainRect.anchorMax = Vector2.one;
            mainRect.offsetMin = Vector2.zero;
            mainRect.offsetMax = Vector2.zero;

            // Background
            Image bgImage = mainContainer.AddComponent<Image>();
            bgImage.color = DARK_BG;

            // Create all UI sections
            CreateHeader(mainContainer.transform);
            CreatePlayerSection(mainContainer.transform, true); // Left - Player
            CreateVSSection(mainContainer.transform);
            CreatePlayerSection(mainContainer.transform, false); // Right - Opponent
            CreateStatusSection(mainContainer.transform);
            CreateCountdownPanel(mainContainer.transform);
            CreateCancelButton(mainContainer.transform);

            // Find and setup MatchmakingManager
            SetupMatchmakingManager(canvas.transform);

            Debug.Log("[MatchmakingUIBuilder] UI created successfully!");
        }

        private static GameObject CreateElement(Transform parent, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private static void CreateHeader(Transform parent)
        {
            // Header container
            GameObject header = CreateElement(parent, "Header");
            RectTransform headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 0.88f);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.offsetMin = Vector2.zero;
            headerRect.offsetMax = Vector2.zero;

            // Title
            GameObject title = CreateElement(header.transform, "TitleText");
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.2f, 0.3f);
            titleRect.anchorMax = new Vector2(0.8f, 0.8f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "SEARCHING...";
            titleText.fontSize = 56;
            titleText.color = CYAN_NEON;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;

            // Subtitle / Game type
            GameObject subtitle = CreateElement(header.transform, "GameTypeText");
            RectTransform subtitleRect = subtitle.GetComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0.2f, 0.05f);
            subtitleRect.anchorMax = new Vector2(0.8f, 0.35f);
            subtitleRect.offsetMin = Vector2.zero;
            subtitleRect.offsetMax = Vector2.zero;

            TextMeshProUGUI subtitleText = subtitle.AddComponent<TextMeshProUGUI>();
            subtitleText.text = "DIGIT RUSH";
            subtitleText.fontSize = 32;
            subtitleText.color = new Color(0.7f, 0.7f, 0.7f);
            subtitleText.alignment = TextAlignmentOptions.Center;
        }

        private static void CreatePlayerSection(Transform parent, bool isPlayer)
        {
            string sectionName = isPlayer ? "PlayerSection" : "OpponentSection";
            float anchorMinX = isPlayer ? 0.02f : 0.52f;
            float anchorMaxX = isPlayer ? 0.48f : 0.98f;

            // Player section
            GameObject section = CreateElement(parent, sectionName);
            RectTransform sectionRect = section.GetComponent<RectTransform>();
            sectionRect.anchorMin = new Vector2(anchorMinX, 0.45f);
            sectionRect.anchorMax = new Vector2(anchorMaxX, 0.85f);
            sectionRect.offsetMin = Vector2.zero;
            sectionRect.offsetMax = Vector2.zero;

            // Player card background
            GameObject card = CreateElement(section.transform, isPlayer ? "PlayerCard" : "OpponentCard");
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.05f, 0.1f);
            cardRect.anchorMax = new Vector2(0.95f, 0.95f);
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = CARD_BG;

            // Neon border
            GameObject border = CreateElement(card.transform, "Border");
            RectTransform borderRect = border.GetComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = Vector2.zero;
            borderRect.offsetMax = Vector2.zero;

            Outline outline = border.AddComponent<Outline>();
            outline.effectColor = isPlayer ? CYAN_NEON : PURPLE_NEON;
            outline.effectDistance = new Vector2(3, -3);

            Image borderImg = border.AddComponent<Image>();
            borderImg.color = new Color(0, 0, 0, 0);

            // Avatar placeholder
            GameObject avatar = CreateElement(card.transform, isPlayer ? "PlayerAvatar" : "OpponentAvatar");
            RectTransform avatarRect = avatar.GetComponent<RectTransform>();
            avatarRect.anchorMin = new Vector2(0.2f, 0.45f);
            avatarRect.anchorMax = new Vector2(0.8f, 0.9f);
            avatarRect.offsetMin = Vector2.zero;
            avatarRect.offsetMax = Vector2.zero;

            Image avatarImg = avatar.AddComponent<Image>();
            avatarImg.color = isPlayer ? new Color(CYAN_NEON.r, CYAN_NEON.g, CYAN_NEON.b, 0.3f)
                                       : new Color(PURPLE_NEON.r, PURPLE_NEON.g, PURPLE_NEON.b, 0.3f);

            // Avatar icon (person silhouette)
            GameObject avatarIcon = CreateElement(avatar.transform, "Icon");
            RectTransform avatarIconRect = avatarIcon.GetComponent<RectTransform>();
            avatarIconRect.anchorMin = new Vector2(0.2f, 0.1f);
            avatarIconRect.anchorMax = new Vector2(0.8f, 0.9f);
            avatarIconRect.offsetMin = Vector2.zero;
            avatarIconRect.offsetMax = Vector2.zero;

            TextMeshProUGUI avatarText = avatarIcon.AddComponent<TextMeshProUGUI>();
            avatarText.text = isPlayer ? "YOU" : "?";
            avatarText.fontSize = 48;
            avatarText.color = isPlayer ? CYAN_NEON : PURPLE_NEON;
            avatarText.alignment = TextAlignmentOptions.Center;
            avatarText.fontStyle = FontStyles.Bold;

            // Player name (expanded to fill more space without rank)
            GameObject nameObj = CreateElement(card.transform, isPlayer ? "PlayerNameText" : "OpponentNameText");
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.05f, 0.05f);
            nameRect.anchorMax = new Vector2(0.95f, 0.4f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;

            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = isPlayer ? "Player" : "???";
            nameText.fontSize = 36;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.fontStyle = FontStyles.Bold;

            // Searching indicator for opponent
            if (!isPlayer)
            {
                GameObject searchIndicator = CreateElement(card.transform, "OpponentSearchingIndicator");
                RectTransform searchRect = searchIndicator.GetComponent<RectTransform>();
                searchRect.anchorMin = new Vector2(0.3f, 0.5f);
                searchRect.anchorMax = new Vector2(0.7f, 0.7f);
                searchRect.offsetMin = Vector2.zero;
                searchRect.offsetMax = Vector2.zero;

                // Spinner ring
                GameObject spinner = CreateElement(searchIndicator.transform, "SpinnerRing");
                RectTransform spinnerRect = spinner.GetComponent<RectTransform>();
                spinnerRect.anchorMin = Vector2.zero;
                spinnerRect.anchorMax = Vector2.one;
                spinnerRect.offsetMin = Vector2.zero;
                spinnerRect.offsetMax = Vector2.zero;

                Image spinnerImg = spinner.AddComponent<Image>();
                spinnerImg.color = PURPLE_NEON;
                // This would ideally be a ring sprite
            }
        }

        private static void CreateVSSection(Transform parent)
        {
            // VS container
            GameObject vsContainer = CreateElement(parent, "VSContainer");
            RectTransform vsRect = vsContainer.GetComponent<RectTransform>();
            vsRect.anchorMin = new Vector2(0.35f, 0.55f);
            vsRect.anchorMax = new Vector2(0.65f, 0.75f);
            vsRect.offsetMin = Vector2.zero;
            vsRect.offsetMax = Vector2.zero;

            // VS background circle
            GameObject vsBg = CreateElement(vsContainer.transform, "VSBackground");
            RectTransform vsBgRect = vsBg.GetComponent<RectTransform>();
            vsBgRect.anchorMin = new Vector2(0.15f, 0.1f);
            vsBgRect.anchorMax = new Vector2(0.85f, 0.9f);
            vsBgRect.offsetMin = Vector2.zero;
            vsBgRect.offsetMax = Vector2.zero;

            Image vsBgImg = vsBg.AddComponent<Image>();
            vsBgImg.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            // Glow effect
            Outline vsGlow = vsBg.AddComponent<Outline>();
            vsGlow.effectColor = ORANGE_NEON;
            vsGlow.effectDistance = new Vector2(4, -4);

            // VS text
            GameObject vsText = CreateElement(vsContainer.transform, "VSText");
            RectTransform vsTextRect = vsText.GetComponent<RectTransform>();
            vsTextRect.anchorMin = Vector2.zero;
            vsTextRect.anchorMax = Vector2.one;
            vsTextRect.offsetMin = Vector2.zero;
            vsTextRect.offsetMax = Vector2.zero;

            TextMeshProUGUI vsTextComp = vsText.AddComponent<TextMeshProUGUI>();
            vsTextComp.text = "VS";
            vsTextComp.fontSize = 72;
            vsTextComp.color = ORANGE_NEON;
            vsTextComp.alignment = TextAlignmentOptions.Center;
            vsTextComp.fontStyle = FontStyles.Bold;

            // Initially hide VS container
            vsContainer.SetActive(false);
        }

        private static void CreateStatusSection(Transform parent)
        {
            // Status container
            GameObject statusContainer = CreateElement(parent, "StatusSection");
            RectTransform statusRect = statusContainer.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.1f, 0.25f);
            statusRect.anchorMax = new Vector2(0.9f, 0.45f);
            statusRect.offsetMin = Vector2.zero;
            statusRect.offsetMax = Vector2.zero;

            // Searching spinner
            GameObject spinner = CreateElement(statusContainer.transform, "SearchingSpinner");
            RectTransform spinnerRect = spinner.GetComponent<RectTransform>();
            spinnerRect.anchorMin = new Vector2(0.35f, 0.5f);
            spinnerRect.anchorMax = new Vector2(0.65f, 0.95f);
            spinnerRect.offsetMin = Vector2.zero;
            spinnerRect.offsetMax = Vector2.zero;

            // Spinner ring
            GameObject spinnerRing = CreateElement(spinner.transform, "SearchingRing");
            RectTransform ringRect = spinnerRing.GetComponent<RectTransform>();
            ringRect.anchorMin = Vector2.zero;
            ringRect.anchorMax = Vector2.one;
            ringRect.offsetMin = new Vector2(10, 10);
            ringRect.offsetMax = new Vector2(-10, -10);

            Image ringImg = spinnerRing.AddComponent<Image>();
            ringImg.color = CYAN_NEON;
            // Ideally this would be a ring/circle sprite

            // Status text
            GameObject statusText = CreateElement(statusContainer.transform, "StatusText");
            RectTransform statusTextRect = statusText.GetComponent<RectTransform>();
            statusTextRect.anchorMin = new Vector2(0.1f, 0.15f);
            statusTextRect.anchorMax = new Vector2(0.9f, 0.45f);
            statusTextRect.offsetMin = Vector2.zero;
            statusTextRect.offsetMax = Vector2.zero;

            TextMeshProUGUI statusTextComp = statusText.AddComponent<TextMeshProUGUI>();
            statusTextComp.text = "Looking for opponent...";
            statusTextComp.fontSize = 32;
            statusTextComp.color = new Color(0.8f, 0.8f, 0.8f);
            statusTextComp.alignment = TextAlignmentOptions.Center;

            // Timer text
            GameObject timerText = CreateElement(statusContainer.transform, "TimerText");
            RectTransform timerRect = timerText.GetComponent<RectTransform>();
            timerRect.anchorMin = new Vector2(0.3f, 0.0f);
            timerRect.anchorMax = new Vector2(0.7f, 0.2f);
            timerRect.offsetMin = Vector2.zero;
            timerRect.offsetMax = Vector2.zero;

            TextMeshProUGUI timerTextComp = timerText.AddComponent<TextMeshProUGUI>();
            timerTextComp.text = "0:00";
            timerTextComp.fontSize = 28;
            timerTextComp.color = CYAN_NEON;
            timerTextComp.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateCountdownPanel(Transform parent)
        {
            // Countdown panel (overlay)
            GameObject countdownPanel = CreateElement(parent, "CountdownPanel");
            RectTransform countdownRect = countdownPanel.GetComponent<RectTransform>();
            countdownRect.anchorMin = Vector2.zero;
            countdownRect.anchorMax = Vector2.one;
            countdownRect.offsetMin = Vector2.zero;
            countdownRect.offsetMax = Vector2.zero;

            Image countdownBg = countdownPanel.AddComponent<Image>();
            countdownBg.color = new Color(0, 0, 0, 0.8f);

            // Countdown text
            GameObject countdownText = CreateElement(countdownPanel.transform, "CountdownText");
            RectTransform countdownTextRect = countdownText.GetComponent<RectTransform>();
            countdownTextRect.anchorMin = new Vector2(0.2f, 0.35f);
            countdownTextRect.anchorMax = new Vector2(0.8f, 0.65f);
            countdownTextRect.offsetMin = Vector2.zero;
            countdownTextRect.offsetMax = Vector2.zero;

            TextMeshProUGUI countdownTextComp = countdownText.AddComponent<TextMeshProUGUI>();
            countdownTextComp.text = "3";
            countdownTextComp.fontSize = 200;
            countdownTextComp.color = GREEN_NEON;
            countdownTextComp.alignment = TextAlignmentOptions.Center;
            countdownTextComp.fontStyle = FontStyles.Bold;

            // Glow effect
            Outline textGlow = countdownText.AddComponent<Outline>();
            textGlow.effectColor = new Color(GREEN_NEON.r, GREEN_NEON.g, GREEN_NEON.b, 0.5f);
            textGlow.effectDistance = new Vector2(5, -5);

            // Initially hide countdown
            countdownPanel.SetActive(false);
        }

        private static void CreateCancelButton(Transform parent)
        {
            // Cancel button container
            GameObject cancelContainer = CreateElement(parent, "CancelButtonContainer");
            RectTransform cancelContainerRect = cancelContainer.GetComponent<RectTransform>();
            cancelContainerRect.anchorMin = new Vector2(0.2f, 0.05f);
            cancelContainerRect.anchorMax = new Vector2(0.8f, 0.15f);
            cancelContainerRect.offsetMin = Vector2.zero;
            cancelContainerRect.offsetMax = Vector2.zero;

            // Cancel button with neon style
            GameObject cancelBtn = CreateElement(cancelContainer.transform, "CancelButton");
            RectTransform cancelRect = cancelBtn.GetComponent<RectTransform>();
            cancelRect.anchorMin = new Vector2(0.1f, 0.1f);
            cancelRect.anchorMax = new Vector2(0.9f, 0.9f);
            cancelRect.offsetMin = Vector2.zero;
            cancelRect.offsetMax = Vector2.zero;

            // Button background
            Image cancelBg = cancelBtn.AddComponent<Image>();
            cancelBg.color = new Color(0.15f, 0.1f, 0.1f);

            // Border
            Outline cancelBorder = cancelBtn.AddComponent<Outline>();
            cancelBorder.effectColor = new Color(1f, 0.3f, 0.3f);
            cancelBorder.effectDistance = new Vector2(2, -2);

            Button cancelButton = cancelBtn.AddComponent<Button>();

            // Button text
            GameObject cancelText = CreateElement(cancelBtn.transform, "Text");
            RectTransform cancelTextRect = cancelText.GetComponent<RectTransform>();
            cancelTextRect.anchorMin = Vector2.zero;
            cancelTextRect.anchorMax = Vector2.one;
            cancelTextRect.offsetMin = Vector2.zero;
            cancelTextRect.offsetMax = Vector2.zero;

            TextMeshProUGUI cancelTextComp = cancelText.AddComponent<TextMeshProUGUI>();
            cancelTextComp.text = "CANCEL";
            cancelTextComp.fontSize = 36;
            cancelTextComp.color = new Color(1f, 0.4f, 0.4f);
            cancelTextComp.alignment = TextAlignmentOptions.Center;
            cancelTextComp.fontStyle = FontStyles.Bold;
        }

        private static void SetupMatchmakingManager(Transform canvasTransform)
        {
            // Find MatchmakingManager in the scene
            var manager = FindObjectOfType<DigitPark.Managers.MatchmakingManager>();
            if (manager == null)
            {
                Debug.LogWarning("[MatchmakingUIBuilder] MatchmakingManager not found in scene. Please add it manually.");
                return;
            }

            // Use SerializedObject to set references
            SerializedObject serializedManager = new SerializedObject(manager);

            // Header
            SetSerializedProperty(serializedManager, "titleText", canvasTransform, "MainContainer/Header/TitleText");
            SetSerializedProperty(serializedManager, "gameTypeText", canvasTransform, "MainContainer/Header/GameTypeText");

            // Player info
            SetSerializedProperty(serializedManager, "playerAvatar", canvasTransform, "MainContainer/PlayerSection/PlayerCard/PlayerAvatar");
            SetSerializedProperty(serializedManager, "playerNameText", canvasTransform, "MainContainer/PlayerSection/PlayerCard/PlayerNameText");
            SetSerializedProperty(serializedManager, "playerCard", canvasTransform, "MainContainer/PlayerSection/PlayerCard");

            // Opponent info
            SetSerializedProperty(serializedManager, "opponentAvatar", canvasTransform, "MainContainer/OpponentSection/OpponentCard/OpponentAvatar");
            SetSerializedProperty(serializedManager, "opponentNameText", canvasTransform, "MainContainer/OpponentSection/OpponentCard/OpponentNameText");
            SetSerializedProperty(serializedManager, "opponentCard", canvasTransform, "MainContainer/OpponentSection/OpponentCard");
            SetSerializedProperty(serializedManager, "opponentSearchingIndicator", canvasTransform, "MainContainer/OpponentSection/OpponentCard/OpponentSearchingIndicator");

            // VS section
            SetSerializedProperty(serializedManager, "vsContainer", canvasTransform, "MainContainer/VSContainer");
            SetSerializedProperty(serializedManager, "vsText", canvasTransform, "MainContainer/VSContainer/VSText");

            // Status
            SetSerializedProperty(serializedManager, "statusText", canvasTransform, "MainContainer/StatusSection/StatusText");
            SetSerializedProperty(serializedManager, "timerText", canvasTransform, "MainContainer/StatusSection/TimerText");
            SetSerializedProperty(serializedManager, "searchingSpinner", canvasTransform, "MainContainer/StatusSection/SearchingSpinner");
            SetSerializedProperty(serializedManager, "searchingRing", canvasTransform, "MainContainer/StatusSection/SearchingSpinner/SearchingRing");

            // Countdown
            SetSerializedProperty(serializedManager, "countdownPanel", canvasTransform, "MainContainer/CountdownPanel");
            SetSerializedProperty(serializedManager, "countdownText", canvasTransform, "MainContainer/CountdownPanel/CountdownText");

            // Cancel button
            SetSerializedProperty(serializedManager, "cancelButton", canvasTransform, "MainContainer/CancelButtonContainer/CancelButton");

            serializedManager.ApplyModifiedProperties();

            Debug.Log("[MatchmakingUIBuilder] MatchmakingManager references set successfully!");
        }

        private static void SetSerializedProperty(SerializedObject serializedObject, string propertyName, Transform root, string path)
        {
            SerializedProperty prop = serializedObject.FindProperty(propertyName);
            if (prop == null)
            {
                Debug.LogWarning($"[MatchmakingUIBuilder] Property not found: {propertyName}");
                return;
            }

            Transform target = root.Find(path);
            if (target == null)
            {
                Debug.LogWarning($"[MatchmakingUIBuilder] UI element not found: {path}");
                return;
            }

            // Determine the type and set accordingly
            if (prop.propertyType == SerializedPropertyType.ObjectReference)
            {
                // Check what component type is expected
                System.Type fieldType = GetFieldType(serializedObject.targetObject, propertyName);

                if (fieldType == typeof(Button))
                    prop.objectReferenceValue = target.GetComponent<Button>();
                else if (fieldType == typeof(Image))
                    prop.objectReferenceValue = target.GetComponent<Image>();
                else if (fieldType == typeof(TextMeshProUGUI))
                    prop.objectReferenceValue = target.GetComponent<TextMeshProUGUI>();
                else if (fieldType == typeof(GameObject))
                    prop.objectReferenceValue = target.gameObject;
                else
                    prop.objectReferenceValue = target.gameObject;
            }
        }

        private static System.Type GetFieldType(Object target, string fieldName)
        {
            System.Type type = target.GetType();
            var field = type.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
                return field.FieldType;
            return typeof(GameObject);
        }
    }
}
