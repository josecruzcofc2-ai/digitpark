using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;
using DigitPark.Monetization;

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
        private static readonly Color DARK_BG = new Color(0.02f, 0.04f, 0.08f, 1f);
        private static readonly Color CARD_BG = new Color(0.04f, 0.08f, 0.12f, 0.98f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.65f, 0.7f, 1f);

        // Icon paths
        private const string SOLO_ICON_PATH = "Assets/_Project/Art/Icons/PlayModeSelectionIcons/PlayModeSelectionSoloIcon.png";
        private const string ONE_VS_ONE_ICON_PATH = "Assets/_Project/Art/Icons/PlayModeSelectionIcons/PlayModeSelection1v1Icon.png";
        private const string TOURNAMENTS_ICON_PATH = "Assets/_Project/Art/Icons/PlayModeSelectionIcons/PlayModeSelectionTorunamentIcon.png";
        private const string BACK_BUTTON_PREFAB = "Assets/_Project/Prefabs/Common/BackButton.prefab";

        [MenuItem("DigitPark/UI Builders/Games/PlayModeSelection", false, 121)]
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
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("[PlayModeSelectionUIBuilder] No se encontró Canvas en la escena");
                return;
            }
            Debug.Log($"[PlayModeSelectionUIBuilder] Usando Canvas: {canvas.gameObject.name}");

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

            // Back Button - Neon prefab (same as rest of the app)
            var oldBackBtn = header.transform.Find("BackButton");
            if (oldBackBtn != null) Object.DestroyImmediate(oldBackBtn.gameObject);

            GameObject backBtnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BACK_BUTTON_PREFAB);
            GameObject backBtn;
            if (backBtnPrefab != null)
            {
                backBtn = (GameObject)PrefabUtility.InstantiatePrefab(backBtnPrefab, header.transform);
                backBtn.name = "BackButton";
            }
            else
            {
                backBtn = CreateElement(header.transform, "BackButton");
                backBtn.AddComponent<Image>().color = CARD_BG;
                backBtn.AddComponent<Button>();
                Debug.LogWarning("[PlayModeSelectionUI] BackButton prefab not found at: " + BACK_BUTTON_PREFAB);
            }
            RectTransform bbRT = backBtn.GetComponent<RectTransform>();
            if (bbRT == null) bbRT = backBtn.AddComponent<RectTransform>();
            bbRT.anchorMin = new Vector2(0, 0.5f);
            bbRT.anchorMax = new Vector2(0, 0.5f);
            bbRT.pivot = new Vector2(0, 0.5f);
            bbRT.anchoredPosition = new Vector2(20, 0);
            bbRT.sizeDelta = new Vector2(50, 50);

            // Title text (stretch between back button and pills)
            GameObject titleObj = CreateElement(header.transform, "TitleText");
            RectTransform titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.07f, 0f);
            titleRT.anchorMax = new Vector2(0.53f, 1f);
            titleRT.pivot = new Vector2(0.5f, 0.5f);
            titleRT.sizeDelta = Vector2.zero;
            titleRT.anchoredPosition = Vector2.zero;
            TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "SELECT MODE";
            titleTmp.fontSize = FontSizes.H4;
            titleTmp.color = CYAN_NEON;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.enableAutoSizing = true;
            titleTmp.fontSizeMin = FontSizes.AutoMinTitle;
            titleTmp.fontSizeMax = FontSizes.H4;
            titleTmp.overflowMode = TextOverflowModes.Ellipsis;

            // Glow effect
            Outline titleOutline = titleObj.AddComponent<Outline>();
            titleOutline.effectColor = new Color(0f, 0.5f, 0.5f, 0.5f);
            titleOutline.effectDistance = new Vector2(2, -2);

            // Currency pills (right side of header)
            var pills = CurrencyHeaderBarHelper.CreateCurrencyPills(header.transform);
            var pillsRT = pills.GetComponent<RectTransform>();
            pillsRT.anchorMin = new Vector2(0.58f, 0.15f);
            pillsRT.anchorMax = new Vector2(0.98f, 0.85f);
            pillsRT.offsetMin = Vector2.zero;
            pillsRT.offsetMax = Vector2.zero;
        }

        private static void CreateTitleSection(Transform parent)
        {
            GameObject titleSection = CreateElement(parent, "TitleSection");
            SetupRectTransform(titleSection,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -120), new Vector2(700, 50));

            // Subtitle only (title moved to header)
            GameObject subtitle = CreateElement(titleSection, "SubtitleText");
            SetupRectTransform(subtitle,
                new Vector2(0.5f, 0), new Vector2(0.5f, 1),
                Vector2.zero, Vector2.zero);
            TextMeshProUGUI subtitleTmp = subtitle.AddComponent<TextMeshProUGUI>();
            subtitleTmp.text = "Choose how you want to play";
            subtitleTmp.fontSize = FontSizes.Body;
            subtitleTmp.color = TEXT_SECONDARY;
            subtitleTmp.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateModeCardsSection(Transform parent)
        {
            GameObject cardsSection = CreateElement(parent, "CardsSection");
            RectTransform cardsRT = cardsSection.GetComponent<RectTransform>();
            cardsRT.anchorMin = new Vector2(0.02f, 0.05f);
            cardsRT.anchorMax = new Vector2(0.98f, 0.85f);
            cardsRT.offsetMin = Vector2.zero;
            cardsRT.offsetMax = Vector2.zero;

            // Vertical Layout - cards fill available space
            VerticalLayoutGroup vlg = cardsSection.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 30;
            vlg.padding = new RectOffset(15, 15, 10, 10);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = true;

            // Load icon sprites
            Sprite soloIcon = AssetDatabase.LoadAssetAtPath<Sprite>(SOLO_ICON_PATH);
            Sprite oneVsOneIcon = AssetDatabase.LoadAssetAtPath<Sprite>(ONE_VS_ONE_ICON_PATH);
            Sprite tournamentsIcon = AssetDatabase.LoadAssetAtPath<Sprite>(TOURNAMENTS_ICON_PATH);

            // Create 3 mode cards - ALL with consistent CYAN NEON style
            // All modes are FREE - no real money involved
            CreateNeonModeCard(cardsSection.transform, "SoloCard",
                "SOLO",
                "Train your brain at your own pace.\nNo competition, just practice.",
                CYAN_NEON, soloIcon);

            CreateNeonModeCard(cardsSection.transform, "OneVsOneCard",
                "1 VS 1",
                "Challenge other players in real time.\nPut your skills to the test.",
                CYAN_NEON, oneVsOneIcon);

            CreateNeonModeCard(cardsSection.transform, "TournamentsCard",
                "TOURNAMENTS",
                "Join free tournaments.\nClimb the leaderboard!",
                CYAN_NEON, tournamentsIcon);
        }

        private static void CreateNeonModeCard(Transform parent, string name, string title,
            string description, Color accentColor, Sprite iconSprite = null)
        {
            Color sideColor = new Color(accentColor.r * 0.3f, accentColor.g * 0.3f, accentColor.b * 0.3f, 1f);

            // ========== CARD CONTAINER ==========
            GameObject card = CreateElement(parent, name);
            // Height controlled by VerticalLayoutGroup (fills available space)

            // ========== SHADOW ==========
            GameObject shadow = CreateElement(card, "Shadow");
            SetupRectTransform(shadow, Vector2.zero, Vector2.one,
                new Vector2(10, -16), Vector2.zero);
            Image shadowImg = shadow.AddComponent<Image>();
            shadowImg.color = new Color(0f, 0f, 0f, 0.5f);

            // ========== SIDE (3D depth) ==========
            GameObject side = CreateElement(card, "Side");
            SetupRectTransform(side,
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, -12), new Vector2(0, 24));
            Image sideImg = side.AddComponent<Image>();
            sideImg.color = sideColor;

            // ========== FACE ==========
            GameObject face = CreateElement(card, "Face");
            SetupRectTransform(face, Vector2.zero, Vector2.one,
                Vector2.zero, new Vector2(0, -12));
            Image faceImg = face.AddComponent<Image>();
            faceImg.color = CARD_BG;

            // Neon outline
            Outline faceOutline = face.AddComponent<Outline>();
            faceOutline.effectColor = accentColor;
            faceOutline.effectDistance = new Vector2(4, -4);

            // ========== CONTENT - Properly Centered Layout ==========
            // Icon on left
            GameObject iconContainer = CreateElement(face, "IconContainer");
            SetupRectTransform(iconContainer,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(120, 0), new Vector2(140, 140));

            Image iconBg = iconContainer.AddComponent<Image>();
            iconBg.color = new Color(0.05f, 0.08f, 0.12f, 0.95f);

            Outline iconOutline = iconContainer.AddComponent<Outline>();
            iconOutline.effectColor = accentColor;
            iconOutline.effectDistance = new Vector2(3f, -3f);

            // Icon image inside - WHITE color to preserve original icon colors
            GameObject icon = CreateElement(iconContainer, "Icon");
            SetupRectTransform(icon, Vector2.zero, Vector2.one,
                Vector2.zero, new Vector2(-16, -16));
            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = Color.white;
            iconImg.preserveAspect = true;
            if (iconSprite != null)
            {
                iconImg.sprite = iconSprite;
            }

            // Arrow indicator on right
            GameObject arrowObj = CreateElement(face, "Arrow");
            SetupRectTransform(arrowObj,
                new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(-75, 0), new Vector2(90, 90));
            TextMeshProUGUI arrowTmp = arrowObj.AddComponent<TextMeshProUGUI>();
            arrowTmp.text = ">";
            arrowTmp.fontSize = FontSizes.Symbol;
            arrowTmp.color = accentColor;
            arrowTmp.fontStyle = FontStyles.Bold;
            arrowTmp.alignment = TextAlignmentOptions.Center;

            // Title text - CENTERED between icon and arrow
            // Icon takes ~100px on left, arrow takes ~50px on right
            GameObject titleObj = CreateElement(face, "TitleText");
            SetupRectTransform(titleObj,
                new Vector2(0, 0.55f), new Vector2(1, 1),
                Vector2.zero, new Vector2(-300, -16));
            TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.text = title;
            titleTmp.fontSize = FontSizes.H2;
            titleTmp.color = accentColor;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.enableWordWrapping = false;
            titleTmp.overflowMode = TextOverflowModes.Overflow;

            // Description text - CENTERED below title, 2 lines max
            GameObject descObj = CreateElement(face, "DescText");
            SetupRectTransform(descObj,
                new Vector2(0, 0.05f), new Vector2(1, 0.55f),
                Vector2.zero, new Vector2(-300, 0));
            TextMeshProUGUI descTmp = descObj.AddComponent<TextMeshProUGUI>();
            descTmp.text = description;
            descTmp.fontSize = FontSizes.Body;
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
