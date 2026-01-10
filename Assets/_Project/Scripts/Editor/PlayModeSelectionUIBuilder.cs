using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para crear la UI de la escena PlayModeSelection
    /// 3 tarjetas grandes estilo neon: Solo, 1v1, Torneos
    /// Compatible con el sistema de temas de la app
    /// </summary>
    public class PlayModeSelectionUIBuilder : EditorWindow
    {
        // Colores del tema neón (estos se sobrescribirán por el ThemeManager en runtime)
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color GREEN_NEON = new Color(0.3f, 1f, 0.5f, 1f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color MAGENTA_NEON = new Color(1f, 0f, 0.8f, 1f);
        private static readonly Color DARK_BG = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color CARD_BG = new Color(0.04f, 0.08f, 0.12f, 0.98f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.65f, 0.7f, 1f);

        [MenuItem("DigitPark/Build PlayModeSelection UI")]
        public static void ShowWindow()
        {
            GetWindow<PlayModeSelectionUIBuilder>("PlayModeSelection UI");
        }

        private void OnGUI()
        {
            GUILayout.Label("PlayModeSelection UI Builder", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Este script creará la UI para la escena PlayModeSelection.\n" +
                "Incluye 3 tarjetas estilo neon: Solo, 1v1, Torneos.\n" +
                "Compatible con el sistema de temas de la app.",
                MessageType.Info);

            GUILayout.Space(10);

            if (GUILayout.Button("Crear PlayModeSelection UI", GUILayout.Height(40)))
            {
                BuildPlayModeSelectionUI();
            }
        }

        private static void BuildPlayModeSelectionUI()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[PlayModeSelectionUIBuilder] No se encontró Canvas en la escena");
                return;
            }

            Transform canvasTransform = canvas.transform;

            // Clean old elements
            CleanCanvas(canvasTransform);

            // Create layout
            CreatePlayModeSelectionLayout(canvasTransform);

            // Assign references
            AssignManagerReferences();

            Debug.Log("[PlayModeSelectionUIBuilder] UI creada exitosamente!");
            EditorUtility.SetDirty(canvas.gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

        private static void CleanCanvas(Transform canvasTransform)
        {
            for (int i = canvasTransform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(canvasTransform.GetChild(i).gameObject);
            }
        }

        private static void CreatePlayModeSelectionLayout(Transform canvasTransform)
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

            // ========== TITLE SECTION ==========
            CreateTitleSection(safeArea.transform);

            // ========== MODE CARDS ==========
            CreateModeCardsSection(safeArea.transform);
        }

        private static void CreateHeader(Transform parent)
        {
            GameObject header = CreateElement(parent, "Header");
            SetupRectTransform(header,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -50), new Vector2(0, 100));

            // Back Button con estilo 3D
            Create3DBackButton(header.transform);
        }

        private static void Create3DBackButton(Transform parent)
        {
            GameObject backBtn = CreateElement(parent, "BackButton");
            SetupRectTransform(backBtn,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(80, 0), new Vector2(120, 50));

            // Shadow
            GameObject shadow = CreateElement(backBtn, "Shadow");
            SetupRectTransform(shadow, Vector2.zero, Vector2.one,
                new Vector2(3, -5), Vector2.zero);
            Image shadowImg = shadow.AddComponent<Image>();
            shadowImg.color = new Color(0f, 0f, 0f, 0.5f);

            // Side (depth)
            GameObject side = CreateElement(backBtn, "Side");
            SetupRectTransform(side,
                new Vector2(0, 0), new Vector2(1, 0),
                Vector2.zero, new Vector2(0, 8));
            Image sideImg = side.AddComponent<Image>();
            sideImg.color = new Color(0f, 0.4f, 0.4f, 1f);

            // Face
            GameObject face = CreateElement(backBtn, "Face");
            SetupRectTransform(face, Vector2.zero, Vector2.one,
                new Vector2(0, 4), new Vector2(0, -4));
            Image faceImg = face.AddComponent<Image>();
            faceImg.color = CARD_BG;

            Outline faceOutline = face.AddComponent<Outline>();
            faceOutline.effectColor = CYAN_NEON;
            faceOutline.effectDistance = new Vector2(2, -2);

            // Arrow - positioned on left inside face
            GameObject arrow = CreateElement(face, "Arrow");
            SetupRectTransform(arrow,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(22, 0), new Vector2(25, 30));
            TextMeshProUGUI arrowTmp = arrow.AddComponent<TextMeshProUGUI>();
            arrowTmp.text = "<";
            arrowTmp.fontSize = 26;
            arrowTmp.color = CYAN_NEON;
            arrowTmp.fontStyle = FontStyles.Bold;
            arrowTmp.alignment = TextAlignmentOptions.Center;

            // Text - positioned after arrow inside face
            GameObject textObj = CreateElement(face, "Text");
            SetupRectTransform(textObj,
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(12, 0), Vector2.zero);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "BACK";
            tmp.fontSize = 18;
            tmp.color = Color.white;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;

            // Button
            Button btn = backBtn.AddComponent<Button>();
            btn.targetGraphic = faceImg;
            SetupButtonColors(btn);
        }

        private static void CreateTitleSection(Transform parent)
        {
            GameObject titleSection = CreateElement(parent, "TitleSection");
            SetupRectTransform(titleSection,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -180), new Vector2(700, 120));

            // Main title
            GameObject title = CreateElement(titleSection, "TitleText");
            SetupRectTransform(title,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -10), new Vector2(700, 60));
            TextMeshProUGUI titleTmp = title.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "SELECT MODE";
            titleTmp.fontSize = 52;
            titleTmp.color = CYAN_NEON;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;

            // Glow effect
            Outline titleOutline = title.AddComponent<Outline>();
            titleOutline.effectColor = new Color(0f, 0.5f, 0.5f, 0.5f);
            titleOutline.effectDistance = new Vector2(2, -2);

            // Subtitle
            GameObject subtitle = CreateElement(titleSection, "SubtitleText");
            SetupRectTransform(subtitle,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 20), new Vector2(500, 35));
            TextMeshProUGUI subtitleTmp = subtitle.AddComponent<TextMeshProUGUI>();
            subtitleTmp.text = "Choose how you want to play";
            subtitleTmp.fontSize = 20;
            subtitleTmp.color = TEXT_SECONDARY;
            subtitleTmp.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateModeCardsSection(Transform parent)
        {
            GameObject cardsSection = CreateElement(parent, "CardsSection");
            SetupRectTransform(cardsSection,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -80), new Vector2(900, 800));

            // Vertical Layout
            VerticalLayoutGroup vlg = cardsSection.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 30;
            vlg.padding = new RectOffset(40, 40, 0, 0);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Create 3 mode cards - ALL with consistent CYAN NEON style
            // All modes are FREE - no real money involved
            CreateNeonModeCard(cardsSection.transform, "SoloCard",
                "SOLO",
                "Train your brain at your own pace.\nNo competition, just practice.",
                CYAN_NEON, 180);

            CreateNeonModeCard(cardsSection.transform, "OneVsOneCard",
                "1 VS 1",
                "Challenge other players in real-time.\nTest your skills head-to-head.",
                CYAN_NEON, 180);

            CreateNeonModeCard(cardsSection.transform, "TournamentsCard",
                "TOURNAMENTS",
                "Join free tournaments for rankings.\nClimb the leaderboard!",
                CYAN_NEON, 180);
        }

        private static void CreateNeonModeCard(Transform parent, string name, string title,
            string description, Color accentColor, float height)
        {
            Color sideColor = new Color(accentColor.r * 0.3f, accentColor.g * 0.3f, accentColor.b * 0.3f, 1f);

            // ========== CARD CONTAINER ==========
            GameObject card = CreateElement(parent, name);
            RectTransform cardRT = card.GetComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(0, height);

            LayoutElement cardLE = card.AddComponent<LayoutElement>();
            cardLE.preferredHeight = height;
            cardLE.minHeight = height;

            // ========== SHADOW ==========
            GameObject shadow = CreateElement(card, "Shadow");
            SetupRectTransform(shadow, Vector2.zero, Vector2.one,
                new Vector2(5, -8), Vector2.zero);
            Image shadowImg = shadow.AddComponent<Image>();
            shadowImg.color = new Color(0f, 0f, 0f, 0.5f);

            // ========== SIDE (3D depth) ==========
            GameObject side = CreateElement(card, "Side");
            SetupRectTransform(side,
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, -6), new Vector2(0, 12));
            Image sideImg = side.AddComponent<Image>();
            sideImg.color = sideColor;

            // ========== FACE ==========
            GameObject face = CreateElement(card, "Face");
            SetupRectTransform(face, Vector2.zero, Vector2.one,
                Vector2.zero, new Vector2(0, -6));
            Image faceImg = face.AddComponent<Image>();
            faceImg.color = CARD_BG;

            // Neon outline
            Outline faceOutline = face.AddComponent<Outline>();
            faceOutline.effectColor = accentColor;
            faceOutline.effectDistance = new Vector2(2, -2);

            // ========== CONTENT - Properly Centered Layout ==========
            // Icon on left
            GameObject iconContainer = CreateElement(face, "IconContainer");
            SetupRectTransform(iconContainer,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(60, 0), new Vector2(70, 70));

            Image iconBg = iconContainer.AddComponent<Image>();
            iconBg.color = new Color(0.05f, 0.08f, 0.12f, 0.95f);

            Outline iconOutline = iconContainer.AddComponent<Outline>();
            iconOutline.effectColor = accentColor;
            iconOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // Icon image inside - WHITE color to preserve original icon colors
            GameObject icon = CreateElement(iconContainer, "Icon");
            SetupRectTransform(icon, Vector2.zero, Vector2.one,
                Vector2.zero, new Vector2(-8, -8));
            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = Color.white; // White to preserve original icon colors

            // Arrow indicator on right
            GameObject arrowObj = CreateElement(face, "Arrow");
            SetupRectTransform(arrowObj,
                new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(-25, 0), new Vector2(30, 30));
            TextMeshProUGUI arrowTmp = arrowObj.AddComponent<TextMeshProUGUI>();
            arrowTmp.text = ">";
            arrowTmp.fontSize = 28;
            arrowTmp.color = accentColor;
            arrowTmp.fontStyle = FontStyles.Bold;
            arrowTmp.alignment = TextAlignmentOptions.Center;

            // Title text - CENTERED between icon and arrow
            // Icon takes ~100px on left, arrow takes ~50px on right
            GameObject titleObj = CreateElement(face, "TitleText");
            SetupRectTransform(titleObj,
                new Vector2(0, 0.55f), new Vector2(1, 1),
                Vector2.zero, new Vector2(-150, -8));
            TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.text = title;
            titleTmp.fontSize = 32;
            titleTmp.color = accentColor;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.enableWordWrapping = false;
            titleTmp.overflowMode = TextOverflowModes.Overflow;

            // Description text - CENTERED below title, 2 lines max
            GameObject descObj = CreateElement(face, "DescText");
            SetupRectTransform(descObj,
                new Vector2(0, 0.05f), new Vector2(1, 0.55f),
                Vector2.zero, new Vector2(-150, 0));
            TextMeshProUGUI descTmp = descObj.AddComponent<TextMeshProUGUI>();
            descTmp.text = description;
            descTmp.fontSize = 13;
            descTmp.color = TEXT_SECONDARY;
            descTmp.alignment = TextAlignmentOptions.Center;
            descTmp.enableWordWrapping = true;
            descTmp.overflowMode = TextOverflowModes.Ellipsis;
            descTmp.maxVisibleLines = 2;

            // ========== BUTTON COMPONENT ==========
            Button btn = card.AddComponent<Button>();
            btn.targetGraphic = faceImg;
            SetupButtonColors(btn);

            // Add GridGlowPulse for animated glow
            face.AddComponent<DigitPark.UI.GridGlowPulse>();
        }

        private static void SetupButtonColors(Button btn)
        {
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            colors.fadeDuration = 0.1f;
            btn.colors = colors;
        }

        private static void AssignManagerReferences()
        {
            var manager = FindFirstObjectByType<DigitPark.Managers.PlayModeSelectionManager>();
            if (manager == null)
            {
                GameObject managerObj = new GameObject("PlayModeSelectionManager");
                manager = managerObj.AddComponent<DigitPark.Managers.PlayModeSelectionManager>();
                Debug.Log("[PlayModeSelectionUIBuilder] PlayModeSelectionManager creado");
            }

            SerializedObject so = new SerializedObject(manager);

            // Header
            AssignTextReference(so, "titleText", "TitleText");
            AssignButtonReference(so, "backButton", "BackButton");

            // Cards
            AssignButtonReference(so, "soloCard", "SoloCard");
            AssignButtonReference(so, "oneVsOneCard", "OneVsOneCard");
            AssignButtonReference(so, "tournamentsCard", "TournamentsCard");

            // Solo card texts
            AssignNestedTextReference(so, "soloTitleText", "SoloCard", "Face/TitleText");
            AssignNestedTextReference(so, "soloDescText", "SoloCard", "Face/DescText");

            // 1v1 card texts
            AssignNestedTextReference(so, "oneVsOneTitleText", "OneVsOneCard", "Face/TitleText");
            AssignNestedTextReference(so, "oneVsOneDescText", "OneVsOneCard", "Face/DescText");

            // Tournaments card texts
            AssignNestedTextReference(so, "tournamentsTitleText", "TournamentsCard", "Face/TitleText");
            AssignNestedTextReference(so, "tournamentsDescText", "TournamentsCard", "Face/DescText");

            // Icons
            AssignNestedImageReference(so, "soloIcon", "SoloCard", "Face/IconContainer/Icon");
            AssignNestedImageReference(so, "oneVsOneIcon", "OneVsOneCard", "Face/IconContainer/Icon");
            AssignNestedImageReference(so, "tournamentsIcon", "TournamentsCard", "Face/IconContainer/Icon");

            so.ApplyModifiedProperties();
            Debug.Log("[PlayModeSelectionUIBuilder] Referencias asignadas al Manager");
        }

        private static void AssignTextReference(SerializedObject so, string propertyName, string objectName)
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

        private static void AssignButtonReference(SerializedObject so, string propertyName, string objectName)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop != null)
            {
                GameObject obj = GameObject.Find(objectName);
                if (obj != null)
                {
                    prop.objectReferenceValue = obj.GetComponent<Button>();
                }
            }
        }

        private static void AssignNestedTextReference(SerializedObject so, string propertyName, string rootName, string path)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop != null)
            {
                GameObject root = GameObject.Find(rootName);
                if (root != null)
                {
                    Transform target = root.transform.Find(path);
                    if (target != null)
                    {
                        prop.objectReferenceValue = target.GetComponent<TextMeshProUGUI>();
                    }
                }
            }
        }

        private static void AssignNestedImageReference(SerializedObject so, string propertyName, string rootName, string path)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop != null)
            {
                GameObject root = GameObject.Find(rootName);
                if (root != null)
                {
                    Transform target = root.transform.Find(path);
                    if (target != null)
                    {
                        prop.objectReferenceValue = target.GetComponent<Image>();
                    }
                }
            }
        }

        #region Helper Methods

        private static GameObject CreateElement(Transform parent, string name)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            return obj;
        }

        private static GameObject CreateElement(GameObject parent, string name)
        {
            return CreateElement(parent.transform, name);
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

        #endregion
    }
}
