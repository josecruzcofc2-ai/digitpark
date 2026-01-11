using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI.Items;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para crear prefabs de items de lista
    /// Resolucion: Portrait 9:16 (1080x1920)
    /// </summary>
    public class ItemPrefabBuilder : EditorWindow
    {
        // Colores del tema neon
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.5f, 1f);
        private static readonly Color PARTICIPATING_BG = new Color(0f, 0.83f, 1f, 0.95f);
        private static readonly Color NORMAL_BG = new Color(0.15f, 0.15f, 0.2f, 0.95f);
        private static readonly Color CARD_BG = new Color(0.08f, 0.12f, 0.18f, 0.98f);
        private static readonly Color HEADER_BG = new Color(0.05f, 0.15f, 0.2f, 1f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color SILVER = new Color(0.75f, 0.75f, 0.75f, 1f);
        private static readonly Color BRONZE = new Color(0.8f, 0.5f, 0.2f, 1f);
        private static readonly Color GREEN_TIME = new Color(0f, 1f, 0.53f, 1f);
        private static readonly Color URGENT_RED = new Color(1f, 0.3f, 0.3f, 1f);
        private static readonly Color DIVIDER_COLOR = new Color(0.5f, 0.5f, 0.6f, 0.8f);
        private static readonly Color H_DIVIDER_COLOR = new Color(0.4f, 0.4f, 0.5f, 0.5f);

        // Dimensiones para 1080x1920
        private const float ITEM_HEIGHT = 70f;
        private const float MY_TOURNAMENT_HEIGHT = 180f; // Card más grande para "Mis Torneos"
        private const float ITEM_WIDTH = 1000f; // Casi todo el ancho con margenes

        [MenuItem("DigitPark/Create Item Prefabs")]
        public static void ShowWindow()
        {
            GetWindow<ItemPrefabBuilder>("Item Prefab Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Item Prefab Builder", EditorStyles.boldLabel);
            GUILayout.Label("Resolucion: Portrait 9:16 (1080x1920)", EditorStyles.miniLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Este script crea los prefabs de items para listas.\n" +
                "Los prefabs se guardaran en Assets/_Project/Prefabs/UI/",
                MessageType.Info);

            // === SCORES ===
            GUILayout.Space(10);
            GUILayout.Label("SCORES / LEADERBOARD", EditorStyles.boldLabel);

            if (GUILayout.Button("Crear LeaderboardEntry Prefab", GUILayout.Height(30)))
            {
                CreateLeaderboardEntryPrefab();
            }

            // === TOURNAMENTS ===
            GUILayout.Space(15);
            GUILayout.Label("TOURNAMENTS", EditorStyles.boldLabel);

            if (GUILayout.Button("Crear TournamentSearchItem (Compacto)", GUILayout.Height(30)))
            {
                CreateTournamentSearchItemPrefab();
            }

            GUILayout.Space(3);

            if (GUILayout.Button("Crear TournamentMyItem (Card Detallada)", GUILayout.Height(30)))
            {
                CreateTournamentMyItemPrefab();
            }

            // === CREAR TODOS ===
            GUILayout.Space(20);
            GUILayout.Label("CREAR TODOS", EditorStyles.boldLabel);

            if (GUILayout.Button("Crear TODOS los Prefabs", GUILayout.Height(45)))
            {
                CreateLeaderboardEntryPrefab();
                CreateTournamentSearchItemPrefab();
                CreateTournamentMyItemPrefab();
                Debug.Log("Todos los prefabs creados!");
            }
        }

        #region Tournament Search Item Prefab (Compacto)

        /// <summary>
        /// Crea prefab compacto para "Buscar Torneos"
        /// Estructura: Participantes | Creador | Tiempo | Flecha
        /// </summary>
        private static void CreateTournamentSearchItemPrefab()
        {
            GameObject itemObj = new GameObject("TournamentSearchItem");

            // RectTransform
            RectTransform itemRT = itemObj.AddComponent<RectTransform>();
            itemRT.anchorMin = new Vector2(0, 1);
            itemRT.anchorMax = new Vector2(1, 1);
            itemRT.pivot = new Vector2(0.5f, 1);
            itemRT.sizeDelta = new Vector2(0, ITEM_HEIGHT);

            LayoutElement layoutElement = itemObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = ITEM_HEIGHT;
            layoutElement.minHeight = ITEM_HEIGHT;
            layoutElement.flexibleWidth = 1;

            // Background
            Image bg = itemObj.AddComponent<Image>();
            bg.color = NORMAL_BG;
            bg.raycastTarget = true;

            // Button
            Button btn = itemObj.AddComponent<Button>();
            btn.targetGraphic = bg;
            ColorBlock colors = btn.colors;
            colors.highlightedColor = new Color(0.2f, 0.4f, 0.5f, 1f);
            colors.pressedColor = new Color(0.1f, 0.3f, 0.4f, 1f);
            btn.colors = colors;

            // === PARTICIPANTES (0% - 15%) ===
            CreateTextElement(itemObj.transform, "ParticipantsText", "12/50",
                0f, 0.15f, 24, GOLD, FontStyles.Bold);

            CreateVerticalDivider(itemObj.transform, "VerticalDivider1", 0.15f);

            // === CREADOR (15% - 55%) ===
            CreateTextElement(itemObj.transform, "CreatorText", "@Username",
                0.15f, 0.55f, 22, Color.white, FontStyles.Normal);

            CreateVerticalDivider(itemObj.transform, "VerticalDivider2", 0.55f);

            // === TIEMPO (55% - 85%) ===
            CreateTextElement(itemObj.transform, "TimeText", "2h 30m",
                0.55f, 0.85f, 22, GREEN_TIME, FontStyles.Normal);

            CreateVerticalDivider(itemObj.transform, "VerticalDivider3", 0.85f);

            // === FLECHA (85% - 100%) ===
            GameObject arrowObj = new GameObject("ArrowIcon");
            arrowObj.transform.SetParent(itemObj.transform, false);
            RectTransform arrowRT = arrowObj.AddComponent<RectTransform>();
            arrowRT.anchorMin = new Vector2(0.85f, 0);
            arrowRT.anchorMax = new Vector2(1f, 1);
            arrowRT.offsetMin = Vector2.zero;
            arrowRT.offsetMax = Vector2.zero;

            TextMeshProUGUI arrowTMP = arrowObj.AddComponent<TextMeshProUGUI>();
            arrowTMP.text = ">";
            arrowTMP.fontSize = 32;
            arrowTMP.color = CYAN_NEON;
            arrowTMP.alignment = TextAlignmentOptions.Center;
            arrowTMP.fontStyle = FontStyles.Bold;

            // Indicadores ocultos por defecto
            CreateIndicator(itemObj.transform, "FullIndicator", "FULL", URGENT_RED);
            CreateIndicator(itemObj.transform, "PrivateIndicator", "PRIV", GOLD);

            // Divisor horizontal
            CreateHorizontalDivider(itemObj.transform, "HorizontalDivider");

            // Componente UI
            TournamentSearchItemUI itemUI = itemObj.AddComponent<TournamentSearchItemUI>();
            itemUI.AutoSetupReferences();

            // Guardar prefab
            string prefabPath = "Assets/_Project/Prefabs/UI/Tournaments/TournamentSearchItem.prefab";
            EnsureDirectoryExists(prefabPath);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(itemObj, prefabPath);
            DestroyImmediate(itemObj);

            Debug.Log($"TournamentSearchItem prefab creado: {prefabPath}");
            Selection.activeObject = prefab;
        }

        #endregion

        #region Tournament My Item Prefab (Card Detallada)

        /// <summary>
        /// Crea prefab card detallada para "Mis Torneos"
        /// Con info del jugador, líder, y sección expandible
        /// </summary>
        private static void CreateTournamentMyItemPrefab()
        {
            GameObject itemObj = new GameObject("TournamentMyItem");

            // RectTransform
            RectTransform itemRT = itemObj.AddComponent<RectTransform>();
            itemRT.anchorMin = new Vector2(0, 1);
            itemRT.anchorMax = new Vector2(1, 1);
            itemRT.pivot = new Vector2(0.5f, 1);
            itemRT.sizeDelta = new Vector2(0, MY_TOURNAMENT_HEIGHT);

            LayoutElement layoutElement = itemObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = MY_TOURNAMENT_HEIGHT;
            layoutElement.minHeight = MY_TOURNAMENT_HEIGHT;
            layoutElement.flexibleWidth = 1;

            // Background
            Image bg = itemObj.AddComponent<Image>();
            bg.color = CARD_BG;
            bg.raycastTarget = true;

            // Outline neón
            Outline outline = itemObj.AddComponent<Outline>();
            outline.effectColor = CYAN_DARK;
            outline.effectDistance = new Vector2(1.5f, 1.5f);

            // === HEADER (arriba - 50px) ===
            GameObject header = CreateSection(itemObj.transform, "Header", 0, 1, 0.72f, 1f, HEADER_BG);

            // Status indicator (círculo)
            GameObject statusObj = new GameObject("StatusIndicator");
            statusObj.transform.SetParent(header.transform, false);
            RectTransform statusRT = statusObj.AddComponent<RectTransform>();
            statusRT.anchorMin = new Vector2(0, 0.5f);
            statusRT.anchorMax = new Vector2(0, 0.5f);
            statusRT.anchoredPosition = new Vector2(25, 0);
            statusRT.sizeDelta = new Vector2(12, 12);
            Image statusImg = statusObj.AddComponent<Image>();
            statusImg.color = GREEN_TIME;

            // Tournament Name
            CreateTextElementAnchored(header.transform, "TournamentName", "Torneo de @Creator",
                new Vector2(0.05f, 0), new Vector2(0.7f, 1), 22, CYAN_NEON, FontStyles.Bold, TextAlignmentOptions.Left);

            // Time Remaining
            CreateTextElementAnchored(header.transform, "TimeRemaining", "2d 5h",
                new Vector2(0.7f, 0), new Vector2(0.98f, 1), 20, GREEN_TIME, FontStyles.Normal, TextAlignmentOptions.Right);

            // === PLAYER STATS (medio izquierda) ===
            GameObject playerStats = CreateSection(itemObj.transform, "PlayerStats", 0, 0.5f, 0.38f, 0.72f, Color.clear);

            CreateTextElementAnchored(playerStats.transform, "LabelMyPosition", "TU POSICIÓN",
                new Vector2(0, 0.5f), new Vector2(0.5f, 1), 14, new Color(0.5f, 0.5f, 0.5f), FontStyles.Normal, TextAlignmentOptions.Center);

            CreateTextElementAnchored(playerStats.transform, "MyPosition", "#3",
                new Vector2(0, 0), new Vector2(0.5f, 0.55f), 32, GOLD, FontStyles.Bold, TextAlignmentOptions.Center);

            CreateTextElementAnchored(playerStats.transform, "LabelMyTime", "TU MEJOR",
                new Vector2(0.5f, 0.5f), new Vector2(1, 1), 14, new Color(0.5f, 0.5f, 0.5f), FontStyles.Normal, TextAlignmentOptions.Center);

            CreateTextElementAnchored(playerStats.transform, "MyBestTime", "2.345s",
                new Vector2(0.5f, 0), new Vector2(1, 0.55f), 24, GREEN_TIME, FontStyles.Bold, TextAlignmentOptions.Center);

            // === CREATOR INFO (medio derecha) ===
            GameObject creatorInfo = CreateSection(itemObj.transform, "CreatorInfo", 0.5f, 1f, 0.38f, 0.72f, Color.clear);

            CreateTextElementAnchored(creatorInfo.transform, "LabelCreator", "CREADOR",
                new Vector2(0, 0.5f), new Vector2(0.5f, 1), 14, new Color(0.5f, 0.5f, 0.5f), FontStyles.Normal, TextAlignmentOptions.Center);

            CreateTextElementAnchored(creatorInfo.transform, "CreatorName", "@Creator",
                new Vector2(0, 0), new Vector2(0.5f, 0.55f), 18, GOLD, FontStyles.Bold, TextAlignmentOptions.Center);

            CreateTextElementAnchored(creatorInfo.transform, "LabelCreatorTime", "TIEMPO",
                new Vector2(0.5f, 0.5f), new Vector2(1, 1), 14, new Color(0.5f, 0.5f, 0.5f), FontStyles.Normal, TextAlignmentOptions.Center);

            CreateTextElementAnchored(creatorInfo.transform, "CreatorTime", "1.987s",
                new Vector2(0.5f, 0), new Vector2(1, 0.55f), 24, GREEN_TIME, FontStyles.Bold, TextAlignmentOptions.Center);

            // === TOURNAMENT INFO (abajo) ===
            GameObject tournamentInfo = CreateSection(itemObj.transform, "TournamentInfo", 0, 0.6f, 0.18f, 0.38f, Color.clear);

            CreateTextElementAnchored(tournamentInfo.transform, "Participants", "45/50 participantes",
                new Vector2(0, 0), new Vector2(0.5f, 1), 16, Color.white, FontStyles.Normal, TextAlignmentOptions.Center);

            CreateTextElementAnchored(tournamentInfo.transform, "MyAttempts", "5 intentos",
                new Vector2(0.5f, 0), new Vector2(1, 1), 16, Color.white, FontStyles.Normal, TextAlignmentOptions.Center);

            // === ACTIONS (abajo derecha) ===
            GameObject actions = CreateSection(itemObj.transform, "Actions", 0.6f, 1f, 0.18f, 0.38f, Color.clear);

            // Botón Jugar
            CreateButton(actions.transform, "PlayButton", "JUGAR", CYAN_NEON, Color.black,
                new Vector2(0.05f, 0.1f), new Vector2(0.95f, 0.9f));

            // === EXPAND BUTTON (muy abajo) ===
            GameObject expandBtn = new GameObject("ExpandButton");
            expandBtn.transform.SetParent(itemObj.transform, false);
            RectTransform expandRT = expandBtn.AddComponent<RectTransform>();
            expandRT.anchorMin = new Vector2(0, 0);
            expandRT.anchorMax = new Vector2(1, 0.18f);
            expandRT.offsetMin = Vector2.zero;
            expandRT.offsetMax = Vector2.zero;

            Image expandBg = expandBtn.AddComponent<Image>();
            expandBg.color = new Color(0.1f, 0.15f, 0.2f, 1f);

            Button expand = expandBtn.AddComponent<Button>();
            expand.targetGraphic = expandBg;

            // Texto "Ver datos del torneo"
            GameObject expandText = new GameObject("Text");
            expandText.transform.SetParent(expandBtn.transform, false);
            RectTransform expandTextRT = expandText.AddComponent<RectTransform>();
            expandTextRT.anchorMin = new Vector2(0, 0.4f);
            expandTextRT.anchorMax = new Vector2(1, 1f);
            expandTextRT.offsetMin = Vector2.zero;
            expandTextRT.offsetMax = Vector2.zero;
            TextMeshProUGUI expandTextTMP = expandText.AddComponent<TextMeshProUGUI>();
            expandTextTMP.text = "Ver datos del torneo";
            expandTextTMP.fontSize = 14;
            expandTextTMP.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            expandTextTMP.alignment = TextAlignmentOptions.Center;
            expandTextTMP.raycastTarget = false;

            // Arrow (debajo del texto)
            GameObject arrow = new GameObject("Arrow");
            arrow.transform.SetParent(expandBtn.transform, false);
            RectTransform arrowRT = arrow.AddComponent<RectTransform>();
            arrowRT.anchorMin = new Vector2(0.5f, 0);
            arrowRT.anchorMax = new Vector2(0.5f, 0.5f);
            arrowRT.anchoredPosition = Vector2.zero;
            arrowRT.sizeDelta = new Vector2(30, 0);
            TextMeshProUGUI arrowTMP = arrow.AddComponent<TextMeshProUGUI>();
            arrowTMP.text = "v";
            arrowTMP.fontSize = 20;
            arrowTMP.color = CYAN_NEON;
            arrowTMP.alignment = TextAlignmentOptions.Center;

            // === EXPANDED SECTION (oculta por defecto) ===
            GameObject expanded = new GameObject("ExpandedSection");
            expanded.transform.SetParent(itemObj.transform, false);
            RectTransform expandedRT = expanded.AddComponent<RectTransform>();
            expandedRT.anchorMin = new Vector2(0, 0);
            expandedRT.anchorMax = new Vector2(1, 0);
            expandedRT.pivot = new Vector2(0.5f, 1);
            expandedRT.anchoredPosition = new Vector2(0, 0);
            expandedRT.sizeDelta = new Vector2(0, 200);
            expanded.SetActive(false);

            // Leaderboard Container
            GameObject leaderboardContainer = new GameObject("LeaderboardContainer");
            leaderboardContainer.transform.SetParent(expanded.transform, false);
            RectTransform lbRT = leaderboardContainer.AddComponent<RectTransform>();
            lbRT.anchorMin = Vector2.zero;
            lbRT.anchorMax = Vector2.one;
            lbRT.offsetMin = new Vector2(10, 10);
            lbRT.offsetMax = new Vector2(-10, -10);

            VerticalLayoutGroup vlg = leaderboardContainer.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 5;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Componente UI
            TournamentMyItemUI itemUI = itemObj.AddComponent<TournamentMyItemUI>();
            itemUI.AutoSetupReferences();

            // Guardar prefab
            string prefabPath = "Assets/_Project/Prefabs/UI/Tournaments/TournamentMyItem.prefab";
            EnsureDirectoryExists(prefabPath);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(itemObj, prefabPath);
            DestroyImmediate(itemObj);

            Debug.Log($"TournamentMyItem prefab creado: {prefabPath}");
            Selection.activeObject = prefab;
        }

        #endregion

        #region Leaderboard Entry Prefab

        private static void CreateLeaderboardEntryPrefab()
        {
            // Crear objeto raiz
            GameObject itemObj = new GameObject("LeaderboardEntry");

            // RectTransform - configurado para LayoutGroup
            RectTransform itemRT = itemObj.AddComponent<RectTransform>();
            itemRT.anchorMin = new Vector2(0, 1);
            itemRT.anchorMax = new Vector2(1, 1);
            itemRT.pivot = new Vector2(0.5f, 1);
            itemRT.sizeDelta = new Vector2(0, ITEM_HEIGHT); // Width 0 para que el LayoutGroup lo controle

            // LayoutElement para scroll
            LayoutElement layoutElement = itemObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = ITEM_HEIGHT;
            layoutElement.minHeight = ITEM_HEIGHT;
            layoutElement.flexibleWidth = 1; // Importante para que se expanda

            // Background - con color visible
            Image bg = itemObj.AddComponent<Image>();
            bg.color = NORMAL_BG;
            bg.raycastTarget = true;

            // Button (opcional para clicks)
            Button btn = itemObj.AddComponent<Button>();
            btn.targetGraphic = bg;
            ColorBlock colors = btn.colors;
            colors.highlightedColor = new Color(0.2f, 0.4f, 0.5f, 1f);
            colors.pressedColor = new Color(0.1f, 0.3f, 0.4f, 1f);
            btn.colors = colors;

            // === POSICION (0% - 15%) ===
            GameObject positionObj = new GameObject("PositionText");
            positionObj.transform.SetParent(itemObj.transform, false);
            RectTransform positionRT = positionObj.AddComponent<RectTransform>();
            positionRT.anchorMin = new Vector2(0, 0);
            positionRT.anchorMax = new Vector2(0.15f, 1);
            positionRT.offsetMin = Vector2.zero;
            positionRT.offsetMax = Vector2.zero;

            TextMeshProUGUI positionTMP = positionObj.AddComponent<TextMeshProUGUI>();
            positionTMP.text = "1";
            positionTMP.fontSize = 28;
            positionTMP.color = GOLD;
            positionTMP.alignment = TextAlignmentOptions.Center;
            positionTMP.fontStyle = FontStyles.Bold;

            // Divisor 1 (15%)
            CreateVerticalDivider(itemObj.transform, "VerticalDivider1", 0.15f);

            // === USERNAME (15% - 70%) ===
            GameObject usernameObj = new GameObject("UsernameText");
            usernameObj.transform.SetParent(itemObj.transform, false);
            RectTransform usernameRT = usernameObj.AddComponent<RectTransform>();
            usernameRT.anchorMin = new Vector2(0.15f, 0);
            usernameRT.anchorMax = new Vector2(0.70f, 1);
            usernameRT.offsetMin = new Vector2(10, 0);
            usernameRT.offsetMax = new Vector2(-10, 0);

            TextMeshProUGUI usernameTMP = usernameObj.AddComponent<TextMeshProUGUI>();
            usernameTMP.text = "Username";
            usernameTMP.fontSize = 22;
            usernameTMP.color = Color.white;
            usernameTMP.alignment = TextAlignmentOptions.Center;
            usernameTMP.enableWordWrapping = false;
            usernameTMP.overflowMode = TextOverflowModes.Ellipsis;

            // Divisor 2 (70%)
            CreateVerticalDivider(itemObj.transform, "VerticalDivider2", 0.70f);

            // === TIEMPO (70% - 100%) ===
            GameObject timeObj = new GameObject("TimeText");
            timeObj.transform.SetParent(itemObj.transform, false);
            RectTransform timeRT = timeObj.AddComponent<RectTransform>();
            timeRT.anchorMin = new Vector2(0.70f, 0);
            timeRT.anchorMax = new Vector2(1f, 1);
            timeRT.offsetMin = new Vector2(10, 0);
            timeRT.offsetMax = new Vector2(-10, 0);

            TextMeshProUGUI timeTMP = timeObj.AddComponent<TextMeshProUGUI>();
            timeTMP.text = "0.000s";
            timeTMP.fontSize = 22;
            timeTMP.color = GREEN_TIME;
            timeTMP.alignment = TextAlignmentOptions.Center;

            // Divisor horizontal
            CreateHorizontalDivider(itemObj.transform, "HorizontalDivider");

            // Agregar componente LeaderboardEntryUI
            LeaderboardEntryUI itemUI = itemObj.AddComponent<LeaderboardEntryUI>();
            itemUI.AutoSetupReferences();

            // Guardar prefab
            string prefabPath = "Assets/_Project/Prefabs/UI/Scores/LeaderboardEntry.prefab";
            EnsureDirectoryExists(prefabPath);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(itemObj, prefabPath);
            DestroyImmediate(itemObj);

            Debug.Log($"LeaderboardEntry prefab creado: {prefabPath}");
            Selection.activeObject = prefab;
        }

        #endregion

        #region Helpers

        private static void CreateVerticalDivider(Transform parent, string name, float anchorX)
        {
            GameObject divider = new GameObject(name);
            divider.transform.SetParent(parent, false);

            RectTransform divRT = divider.AddComponent<RectTransform>();
            divRT.anchorMin = new Vector2(anchorX, 0.1f);
            divRT.anchorMax = new Vector2(anchorX, 0.9f);
            divRT.pivot = new Vector2(0.5f, 0.5f);
            divRT.sizeDelta = new Vector2(2f, 0);

            Image divImage = divider.AddComponent<Image>();
            divImage.color = DIVIDER_COLOR;
            divImage.raycastTarget = false;
        }

        private static void CreateHorizontalDivider(Transform parent, string name)
        {
            GameObject divider = new GameObject(name);
            divider.transform.SetParent(parent, false);

            RectTransform divRT = divider.AddComponent<RectTransform>();
            divRT.anchorMin = new Vector2(0.02f, 0f);
            divRT.anchorMax = new Vector2(0.98f, 0f);
            divRT.pivot = new Vector2(0.5f, 0f);
            divRT.sizeDelta = new Vector2(0, 1f);

            Image divImage = divider.AddComponent<Image>();
            divImage.color = H_DIVIDER_COLOR;
            divImage.raycastTarget = false;
        }

        private static void CreateTextElement(Transform parent, string name, string text,
            float anchorMinX, float anchorMaxX, int fontSize, Color color, FontStyles style)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(anchorMinX, 0);
            rt.anchorMax = new Vector2(anchorMaxX, 1);
            rt.offsetMin = new Vector2(5, 0);
            rt.offsetMax = new Vector2(-5, 0);

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;
        }

        private static void CreateTextElementAnchored(Transform parent, string name, string text,
            Vector2 anchorMin, Vector2 anchorMax, int fontSize, Color color, FontStyles style, TextAlignmentOptions alignment)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = new Vector2(5, 0);
            rt.offsetMax = new Vector2(-5, 0);

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = alignment;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;
        }

        private static void CreateIndicator(Transform parent, string name, string text, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = new Vector2(-5, -5);
            rt.sizeDelta = new Vector2(50, 20);

            Image bg = obj.AddComponent<Image>();
            bg.color = color;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 12;
            tmp.color = Color.black;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;

            obj.SetActive(false); // Oculto por defecto
        }

        private static GameObject CreateSection(Transform parent, string name,
            float anchorMinX, float anchorMaxX, float anchorMinY, float anchorMaxY, Color bgColor)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(anchorMinX, anchorMinY);
            rt.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            if (bgColor.a > 0)
            {
                Image bg = obj.AddComponent<Image>();
                bg.color = bgColor;
                bg.raycastTarget = false;
            }

            return obj;
        }

        private static void CreateButton(Transform parent, string name, string text,
            Color bgColor, Color textColor, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = obj.AddComponent<Image>();
            bg.color = bgColor;

            Button btn = obj.AddComponent<Button>();
            btn.targetGraphic = bg;

            ColorBlock colors = btn.colors;
            colors.highlightedColor = new Color(bgColor.r * 0.9f, bgColor.g * 0.9f, bgColor.b * 0.9f, 1f);
            colors.pressedColor = new Color(bgColor.r * 0.7f, bgColor.g * 0.7f, bgColor.b * 0.7f, 1f);
            btn.colors = colors;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 18;
            tmp.color = textColor;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
        }

        private static void EnsureDirectoryExists(string path)
        {
            string directory = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }
        }

        #endregion
    }
}
