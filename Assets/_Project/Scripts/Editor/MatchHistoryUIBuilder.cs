using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// MatchHistory UI Builder - Escena dedicada de Historial de Partidas
    /// Filtros con iconos oficiales de cada minijuego, lista de partidas
    /// Portrait 9:16 (1080x1920), matchWidthOrHeight=0
    ///
    /// Menu: DigitPark/UI Builders/Social/MatchHistory
    /// </summary>
    public class MatchHistoryUIBuilder : EditorWindow
    {
        #region Colors

        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.5f, 1f);

        private static readonly Color DARK_BG = new Color(0.02f, 0.04f, 0.08f, 1f);
        private static readonly Color CARD_BG = new Color(0.06f, 0.08f, 0.12f, 1f);
        private static readonly Color CARD_BG_LIGHT = new Color(0.08f, 0.1f, 0.14f, 1f);
        private static readonly Color HEADER_BG = new Color(0.04f, 0.06f, 0.1f, 0.98f);
        private static readonly Color CHIP_INACTIVE = new Color(0.12f, 0.14f, 0.18f, 1f);

        private static readonly Color TEXT_WHITE = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.6f, 0.65f, 1f);
        private static readonly Color TEXT_DARK = new Color(0.05f, 0.05f, 0.08f, 1f);

        private static readonly Color GREEN_WIN = new Color(0.2f, 0.9f, 0.4f, 1f);
        private static readonly Color RED_LOSS = new Color(1f, 0.3f, 0.3f, 1f);
        private static readonly Color GREY_PRACTICE = new Color(0.5f, 0.5f, 0.55f, 1f);

        // Game colors
        private static readonly Color COLOR_DIGIT_RUSH = new Color(0f, 1f, 1f, 1f);
        private static readonly Color COLOR_MEMORY_PAIRS = new Color(0.6f, 0.3f, 1f, 1f);
        private static readonly Color COLOR_QUICK_MATH = new Color(1f, 0.5f, 0f, 1f);
        private static readonly Color COLOR_FLASH_TAP = new Color(0.2f, 0.9f, 0.4f, 1f);
        private static readonly Color COLOR_ODD_ONE_OUT = new Color(1f, 0.3f, 0.3f, 1f);
        private static readonly Color COLOR_COGNITIVE_SPRINT = new Color(1f, 0.84f, 0f, 1f);

        #endregion

        #region Layout Anchors

        private const float HEADER_TOP = 0.985f;
        private const float HEADER_BOT = 0.945f;

        private const float FILTERS_TOP = 0.935f;
        private const float FILTERS_BOT = 0.87f;

        private const float CONTENT_TOP = 0.86f;
        private const float CONTENT_BOT = 0.03f;

        private const float SIDE_PAD = 20f;

        #endregion

        #region Icon Paths

        private const string ICONS_PATH = "Assets/_Project/Art/Icons/Games/";
        private const string MATCH_ENTRY_PREFAB_PATH = "Assets/_Project/Prefabs/Social/MatchHistoryEntry.prefab";
        private const string BACK_BUTTON_PREFAB = "Assets/_Project/Prefabs/Common/BackButton.prefab";

        #endregion

        [MenuItem("DigitPark/UI Builders/Social/MatchHistory", false, 173)]
        public static void ShowWindow()
        {
            GetWindow<MatchHistoryUIBuilder>("MatchHistory Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("MatchHistory UI Builder", EditorStyles.boldLabel);
            GUILayout.Label("Historial de Partidas Generales - Neon Theme", EditorStyles.miniLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Layout completo (de arriba a abajo):\n\n" +
                "1. Header (Back, HISTORIAL, contador)\n" +
                "2. Game Filters (iconos oficiales de cada juego)\n" +
                "3. ScrollView (lista de match entries)\n" +
                "4. MatchEntry Prefab (color bar, info, resultado)",
                MessageType.Info);

            GUILayout.Space(15);

            GUI.backgroundColor = CYAN_NEON;
            if (GUILayout.Button("RECONSTRUIR MATCH HISTORY COMPLETO", GUILayout.Height(50)))
                RebuildMatchHistory();
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);
            GUILayout.Label("Secciones individuales:", EditorStyles.boldLabel);

            if (GUILayout.Button("1. Header", GUILayout.Height(25))) CreateHeader();
            if (GUILayout.Button("2. Game Filters", GUILayout.Height(25))) CreateGameFilters();
            if (GUILayout.Button("3. ScrollView", GUILayout.Height(25))) CreateScrollView();
            if (GUILayout.Button("4. MatchEntry Prefab", GUILayout.Height(25))) CreateMatchEntryPrefab();

            GUILayout.Space(15);

            GUI.backgroundColor = new Color(1f, 0.84f, 0f);
            if (GUILayout.Button("ASIGNAR REFERENCIAS AL MANAGER", GUILayout.Height(35)))
                SetupManagerReferences();
            GUI.backgroundColor = Color.white;
        }

        #region Main Rebuild

        private static void RebuildMatchHistory()
        {
            CleanupOldUI();
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("[MatchHistoryUI] No se encontro Canvas");
                return;
            }

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0f;
            }

            string[] oldNames = {
                "Background", "Header", "GameFilters", "ScrollView",
                "ContentPanel", "HistoryPanel"
            };
            foreach (var n in oldNames)
            {
                Transform t = canvas.transform.Find(n);
                if (t != null) DestroyImmediate(t.gameObject);
            }

            CreateBackground(canvas.transform);
            CreateHeader();
            CreateGameFilters();
            CreateScrollView();
            CreateMatchEntryPrefab();
            SetupManagerReferences();

            Debug.Log("[MatchHistoryUI] MatchHistory RECONSTRUIDO exitosamente!");
            EditorUtility.SetDirty(canvas.gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        }

        private static void CreateBackground(Transform parent)
        {
            var bg = FindOrCreate(parent, "Background");
            bg.transform.SetAsFirstSibling();
            var rt = GetOrAdd<RectTransform>(bg);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            GetOrAdd<Image>(bg).color = DARK_BG;
        }

        #endregion

        #region Header

        private static void CreateHeader()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var header = FindOrCreate(canvas.transform, "Header");
            var rt = GetOrAdd<RectTransform>(header);
            SetAnchors(rt, 0, HEADER_BOT, 1, HEADER_TOP);
            GetOrAdd<Image>(header).color = HEADER_BG;

            // Back Button - Neon Cyan prefab
            GameObject backBtnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BACK_BUTTON_PREFAB);
            GameObject backBtnObj;
            if (backBtnPrefab != null)
            {
                Transform oldBtn = header.transform.Find("BackButton");
                if (oldBtn != null) DestroyImmediate(oldBtn.gameObject);
                backBtnObj = (GameObject)PrefabUtility.InstantiatePrefab(backBtnPrefab, header.transform);
                backBtnObj.name = "BackButton";
            }
            else
            {
                backBtnObj = FindOrCreate(header.transform, "BackButton");
                GetOrAdd<Image>(backBtnObj).color = new Color(0, 0, 0, 0);
                GetOrAdd<Button>(backBtnObj);
                Debug.LogWarning("[MatchHistoryUI] BackButton prefab not found, using fallback");
            }
            var bRT = GetOrAdd<RectTransform>(backBtnObj);
            bRT.anchorMin = new Vector2(0, 0.5f);
            bRT.anchorMax = new Vector2(0, 0.5f);
            bRT.pivot = new Vector2(0, 0.5f);
            bRT.anchoredPosition = new Vector2(15, 0);
            bRT.sizeDelta = new Vector2(50, 50);

            // Title
            var title = FindOrCreate(header.transform, "TitleText");
            var tRT = GetOrAdd<RectTransform>(title);
            tRT.anchorMin = new Vector2(0.12f, 0);
            tRT.anchorMax = new Vector2(0.65f, 1);
            tRT.offsetMin = Vector2.zero;
            tRT.offsetMax = Vector2.zero;
            var tTMP = GetOrAdd<TextMeshProUGUI>(title);
            tTMP.text = "HISTORIAL";
            tTMP.fontSize = 78;
            tTMP.color = TEXT_WHITE;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.Left;

            // Total Count
            var count = FindOrCreate(header.transform, "TotalCountText");
            var cRT = GetOrAdd<RectTransform>(count);
            cRT.anchorMin = new Vector2(0.55f, 0);
            cRT.anchorMax = new Vector2(1, 1);
            cRT.offsetMin = Vector2.zero;
            cRT.offsetMax = new Vector2(-15, 0);
            var cTMP = GetOrAdd<TextMeshProUGUI>(count);
            cTMP.text = "0 partidas";
            cTMP.fontSize = 38;
            cTMP.color = TEXT_SECONDARY;
            cTMP.alignment = TextAlignmentOptions.Right;

            Debug.Log("[MatchHistoryUI] Header creado");
        }

        #endregion

        #region Game Filters

        private static void CreateGameFilters()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var filters = FindOrCreate(canvas.transform, "GameFilters");
            var rt = GetOrAdd<RectTransform>(filters);
            SetAnchorsWithPad(rt, FILTERS_BOT, FILTERS_TOP);

            var barImg = GetOrAdd<Image>(filters);
            barImg.color = Color.clear;

            var hlg = GetOrAdd<HorizontalLayoutGroup>(filters);
            hlg.spacing = 25;
            hlg.padding = new RectOffset(5, 5, 5, 5);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            // "Todos" chip (text-based, active by default)
            CreateTextFilterChip(filters.transform, "FilterAll", "Todos", CYAN_NEON, true);

            // Game icon chips
            CreateIconFilterChip(filters.transform, "FilterDigitRush", "DigitRushIcon", COLOR_DIGIT_RUSH);
            CreateIconFilterChip(filters.transform, "FilterMemoryPairs", "MemoryPairsIcon", COLOR_MEMORY_PAIRS);
            CreateIconFilterChip(filters.transform, "FilterQuickMath", "QuickMathIcon", COLOR_QUICK_MATH);
            CreateIconFilterChip(filters.transform, "FilterFlashTap", "FlashTapIcon", COLOR_FLASH_TAP);
            CreateIconFilterChip(filters.transform, "FilterOddOneOut", "OddOneOutIcon", COLOR_ODD_ONE_OUT);
            CreateIconFilterChip(filters.transform, "FilterCognitiveSprint", "CognitiveSprintIcon", COLOR_COGNITIVE_SPRINT);

            Debug.Log("[MatchHistoryUI] Game Filters creados con iconos oficiales");
        }

        private static void CreateTextFilterChip(Transform parent, string name, string label, Color accentColor, bool active)
        {
            var chip = FindOrCreate(parent, name);
            var le = GetOrAdd<LayoutElement>(chip);
            le.preferredWidth = 250;

            var bg = GetOrAdd<Image>(chip);
            bg.color = active ? accentColor : CHIP_INACTIVE;

            var outline = GetOrAdd<Outline>(chip);
            outline.effectColor = new Color(accentColor.r, accentColor.g, accentColor.b, active ? 0.6f : 0.15f);
            outline.effectDistance = new Vector2(1.5f, 1.5f);

            GetOrAdd<Button>(chip).targetGraphic = bg;

            var text = FindOrCreate(chip.transform, "Text");
            var tRT = GetOrAdd<RectTransform>(text);
            tRT.anchorMin = Vector2.zero;
            tRT.anchorMax = Vector2.one;
            tRT.offsetMin = new Vector2(5, 0);
            tRT.offsetMax = new Vector2(-5, 0);
            var tTMP = GetOrAdd<TextMeshProUGUI>(text);
            tTMP.text = label;
            tTMP.fontSize = 45;
            tTMP.color = active ? TEXT_DARK : TEXT_SECONDARY;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateIconFilterChip(Transform parent, string name, string iconFileName, Color gameColor)
        {
            var chip = FindOrCreate(parent, name);
            var le = GetOrAdd<LayoutElement>(chip);
            le.preferredWidth = 188;

            var bg = GetOrAdd<Image>(chip);
            bg.color = CHIP_INACTIVE;

            var outline = GetOrAdd<Outline>(chip);
            outline.effectColor = new Color(gameColor.r, gameColor.g, gameColor.b, 0.2f);
            outline.effectDistance = new Vector2(1.5f, 1.5f);

            GetOrAdd<Button>(chip).targetGraphic = bg;

            // Game icon - llena el chip completo con margen minimo
            var iconGO = FindOrCreate(chip.transform, "Icon");
            var iRT = GetOrAdd<RectTransform>(iconGO);
            iRT.anchorMin = new Vector2(0.05f, 0.05f);
            iRT.anchorMax = new Vector2(0.95f, 0.95f);
            iRT.offsetMin = Vector2.zero;
            iRT.offsetMax = Vector2.zero;

            var iconImg = GetOrAdd<Image>(iconGO);
            iconImg.preserveAspect = true;
            iconImg.color = Color.white;

            // Load icon sprite
            Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ICONS_PATH + iconFileName + ".png");
            if (iconSprite != null)
            {
                iconImg.sprite = iconSprite;
                Debug.Log($"[MatchHistoryUI] Icono asignado: {iconFileName}");
            }
            else
            {
                Debug.LogWarning($"[MatchHistoryUI] Icono no encontrado: {ICONS_PATH}{iconFileName}.png");
                // Fallback: poner letra
                var fallback = GetOrAdd<TextMeshProUGUI>(iconGO);
                fallback.text = name.Replace("Filter", "").Substring(0, 2);
                fallback.fontSize = 40;
                fallback.color = gameColor;
                fallback.alignment = TextAlignmentOptions.Center;
            }
        }

        #endregion

        #region ScrollView

        private static void CreateScrollView()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var scrollView = FindOrCreate(canvas.transform, "ScrollView");
            var svRT = GetOrAdd<RectTransform>(scrollView);
            SetAnchorsWithPad(svRT, CONTENT_BOT, CONTENT_TOP);

            var scrollRect = GetOrAdd<ScrollRect>(scrollView);
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.scrollSensitivity = 30;

            var svImg = GetOrAdd<Image>(scrollView);
            svImg.color = Color.clear;
            GetOrAdd<Mask>(scrollView).showMaskGraphic = false;

            // Viewport
            var viewport = FindOrCreate(scrollView.transform, "Viewport");
            var vpRT = GetOrAdd<RectTransform>(viewport);
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.offsetMin = Vector2.zero;
            vpRT.offsetMax = Vector2.zero;
            var vpImg = GetOrAdd<Image>(viewport);
            vpImg.color = Color.clear;
            GetOrAdd<Mask>(viewport).showMaskGraphic = false;
            scrollRect.viewport = vpRT;

            // Content
            var content = FindOrCreate(viewport.transform, "Content");
            var cRT = GetOrAdd<RectTransform>(content);
            cRT.anchorMin = new Vector2(0, 1);
            cRT.anchorMax = new Vector2(1, 1);
            cRT.pivot = new Vector2(0.5f, 1);
            cRT.offsetMin = Vector2.zero;
            cRT.offsetMax = Vector2.zero;
            scrollRect.content = cRT;

            var vlg = GetOrAdd<VerticalLayoutGroup>(content);
            vlg.spacing = 8;
            vlg.padding = new RectOffset(0, 0, 5, 20);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = GetOrAdd<ContentSizeFitter>(content);
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Empty Text
            var emptyText = FindOrCreate(content.transform, "EmptyText");
            var eRT = GetOrAdd<RectTransform>(emptyText);
            eRT.sizeDelta = new Vector2(0, 200);
            GetOrAdd<LayoutElement>(emptyText).preferredHeight = 200;
            var eTMP = GetOrAdd<TextMeshProUGUI>(emptyText);
            eTMP.text = "No has jugado partidas aun\nJuega para ver tu historial";
            eTMP.fontSize = 18;
            eTMP.color = TEXT_SECONDARY;
            eTMP.alignment = TextAlignmentOptions.Center;
            eTMP.fontStyle = FontStyles.Bold;

            // Loading Indicator
            var loading = FindOrCreate(content.transform, "LoadingIndicator");
            var ldRT = GetOrAdd<RectTransform>(loading);
            ldRT.sizeDelta = new Vector2(0, 100);
            GetOrAdd<LayoutElement>(loading).preferredHeight = 100;
            var ldTMP = GetOrAdd<TextMeshProUGUI>(loading);
            ldTMP.text = "Cargando...";
            ldTMP.fontSize = 18;
            ldTMP.color = CYAN_NEON;
            ldTMP.alignment = TextAlignmentOptions.Center;
            loading.SetActive(false);

            // Load More Button
            var loadMore = FindOrCreate(content.transform, "LoadMoreButton");
            var lmRT = GetOrAdd<RectTransform>(loadMore);
            lmRT.sizeDelta = new Vector2(0, 55);
            GetOrAdd<LayoutElement>(loadMore).preferredHeight = 55;

            var lmBg = GetOrAdd<Image>(loadMore);
            lmBg.color = CARD_BG;
            var lmOutline = GetOrAdd<Outline>(loadMore);
            lmOutline.effectColor = new Color(CYAN_DARK.r, CYAN_DARK.g, CYAN_DARK.b, 0.4f);
            lmOutline.effectDistance = new Vector2(1, 1);
            GetOrAdd<Button>(loadMore).targetGraphic = lmBg;

            var lmText = FindOrCreate(loadMore.transform, "Text");
            var lmtRT = GetOrAdd<RectTransform>(lmText);
            lmtRT.anchorMin = Vector2.zero;
            lmtRT.anchorMax = Vector2.one;
            lmtRT.offsetMin = Vector2.zero;
            lmtRT.offsetMax = Vector2.zero;
            var lmtTMP = GetOrAdd<TextMeshProUGUI>(lmText);
            lmtTMP.text = "Cargar mas...";
            lmtTMP.fontSize = 16;
            lmtTMP.color = CYAN_NEON;
            lmtTMP.fontStyle = FontStyles.Bold;
            lmtTMP.alignment = TextAlignmentOptions.Center;

            loadMore.SetActive(false);

            Debug.Log("[MatchHistoryUI] ScrollView creado");
        }

        #endregion

        #region MatchEntry Prefab

        private static void CreateMatchEntryPrefab()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            Transform content = canvas.transform.Find("ScrollView/Viewport/Content");
            if (content == null)
            {
                Debug.LogWarning("[MatchHistoryUI] ScrollView/Viewport/Content no encontrado. Crea ScrollView primero.");
                return;
            }

            // Create card
            var card = new GameObject("MatchEntry_Template");
            card.transform.SetParent(content, false);

            var cardRT = card.AddComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(0, 100);
            card.AddComponent<LayoutElement>().preferredHeight = 100;

            var cardBg = card.AddComponent<Image>();
            cardBg.color = CARD_BG;
            var cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = new Color(CYAN_DARK.r, CYAN_DARK.g, CYAN_DARK.b, 0.3f);
            cardOutline.effectDistance = new Vector2(1, 1);

            // ---- Color Bar (left accent, 5px wide) ----
            var colorBar = new GameObject("ColorBar");
            colorBar.transform.SetParent(card.transform, false);
            var cbRT = colorBar.AddComponent<RectTransform>();
            cbRT.anchorMin = new Vector2(0, 0);
            cbRT.anchorMax = new Vector2(0, 1);
            cbRT.pivot = new Vector2(0, 0.5f);
            cbRT.anchoredPosition = Vector2.zero;
            cbRT.sizeDelta = new Vector2(5, 0);
            var cbImg = colorBar.AddComponent<Image>();
            cbImg.color = CYAN_NEON;

            // ---- Info Section (center) ----
            var infoSection = new GameObject("InfoSection");
            infoSection.transform.SetParent(card.transform, false);
            var isRT = infoSection.AddComponent<RectTransform>();
            isRT.anchorMin = new Vector2(0, 0);
            isRT.anchorMax = new Vector2(0.75f, 1);
            isRT.offsetMin = new Vector2(18, 10);
            isRT.offsetMax = new Vector2(0, -10);

            // Game Name (top left)
            var gameName = new GameObject("GameName");
            gameName.transform.SetParent(infoSection.transform, false);
            var gnRT = gameName.AddComponent<RectTransform>();
            gnRT.anchorMin = new Vector2(0, 0.65f);
            gnRT.anchorMax = new Vector2(0.65f, 1);
            gnRT.offsetMin = Vector2.zero;
            gnRT.offsetMax = Vector2.zero;
            var gnTMP = gameName.AddComponent<TextMeshProUGUI>();
            gnTMP.text = "Digit Rush";
            gnTMP.fontSize = 19;
            gnTMP.color = TEXT_WHITE;
            gnTMP.fontStyle = FontStyles.Bold;
            gnTMP.alignment = TextAlignmentOptions.Left;

            // Result Badge (top right of info)
            var resultBadge = new GameObject("ResultBadge");
            resultBadge.transform.SetParent(infoSection.transform, false);
            var rbRT = resultBadge.AddComponent<RectTransform>();
            rbRT.anchorMin = new Vector2(0.65f, 0.65f);
            rbRT.anchorMax = new Vector2(1, 1);
            rbRT.offsetMin = Vector2.zero;
            rbRT.offsetMax = Vector2.zero;
            var rbTMP = resultBadge.AddComponent<TextMeshProUGUI>();
            rbTMP.text = "WIN";
            rbTMP.fontSize = 17;
            rbTMP.color = GREEN_WIN;
            rbTMP.fontStyle = FontStyles.Bold;
            rbTMP.alignment = TextAlignmentOptions.Right;

            // Subtitle (middle - opponent or practice)
            var subtitle = new GameObject("Subtitle");
            subtitle.transform.SetParent(infoSection.transform, false);
            var stRT = subtitle.AddComponent<RectTransform>();
            stRT.anchorMin = new Vector2(0, 0.33f);
            stRT.anchorMax = new Vector2(1, 0.63f);
            stRT.offsetMin = Vector2.zero;
            stRT.offsetMax = Vector2.zero;
            var stTMP = subtitle.AddComponent<TextMeshProUGUI>();
            stTMP.text = "vs. Player123";
            stTMP.fontSize = 15;
            stTMP.color = TEXT_SECONDARY;
            stTMP.alignment = TextAlignmentOptions.Left;

            // Detail Text (bottom - time + errors)
            var detail = new GameObject("DetailText");
            detail.transform.SetParent(infoSection.transform, false);
            var dtRT = detail.AddComponent<RectTransform>();
            dtRT.anchorMin = new Vector2(0, 0);
            dtRT.anchorMax = new Vector2(1, 0.31f);
            dtRT.offsetMin = Vector2.zero;
            dtRT.offsetMax = Vector2.zero;
            var dtTMP = detail.AddComponent<TextMeshProUGUI>();
            dtTMP.text = "12.5s \u00B7 0 err";
            dtTMP.fontSize = 14;
            dtTMP.color = CYAN_NEON;
            dtTMP.alignment = TextAlignmentOptions.Left;

            // ---- Date Text (right side) ----
            var dateText = new GameObject("DateText");
            dateText.transform.SetParent(card.transform, false);
            var dateRT = dateText.AddComponent<RectTransform>();
            dateRT.anchorMin = new Vector2(0.75f, 0);
            dateRT.anchorMax = new Vector2(1, 1);
            dateRT.offsetMin = new Vector2(0, 0);
            dateRT.offsetMax = new Vector2(-12, 0);
            var dateTMP = dateText.AddComponent<TextMeshProUGUI>();
            dateTMP.text = "Hace 2h";
            dateTMP.fontSize = 13;
            dateTMP.color = TEXT_SECONDARY;
            dateTMP.alignment = TextAlignmentOptions.Right;

            // Save as prefab
            string prefabDir = "Assets/_Project/Prefabs/Social";
            if (!AssetDatabase.IsValidFolder(prefabDir))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Social");
            }

            PrefabUtility.SaveAsPrefabAsset(card, MATCH_ENTRY_PREFAB_PATH);
            Debug.Log($"[MatchHistoryUI] MatchEntry prefab guardado en: {MATCH_ENTRY_PREFAB_PATH}");

            DestroyImmediate(card);

            Debug.Log("[MatchHistoryUI] MatchEntry Prefab creado");
        }

        #endregion

        #region Manager References

        private static void SetupManagerReferences()
        {
            var manager = Object.FindFirstObjectByType<DigitPark.Managers.MatchHistorySceneManager>();
            if (manager == null)
            {
                Debug.LogWarning("[MatchHistoryUI] MatchHistorySceneManager no encontrado en la escena. Agrega el componente primero.");
                return;
            }

            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var so = new SerializedObject(manager);
            Transform r = canvas.transform;

            // Header
            SetRef(so, "backButton", FindInPath<Button>(r, "Header/BackButton"));
            SetRef(so, "titleText", FindInPath<TextMeshProUGUI>(r, "Header/TitleText"));
            SetRef(so, "totalCountText", FindInPath<TextMeshProUGUI>(r, "Header/TotalCountText"));

            // Game Filters
            SetRef(so, "filterAllButton", FindInPath<Button>(r, "GameFilters/FilterAll"));
            SetRef(so, "filterDigitRushButton", FindInPath<Button>(r, "GameFilters/FilterDigitRush"));
            SetRef(so, "filterMemoryPairsButton", FindInPath<Button>(r, "GameFilters/FilterMemoryPairs"));
            SetRef(so, "filterQuickMathButton", FindInPath<Button>(r, "GameFilters/FilterQuickMath"));
            SetRef(so, "filterFlashTapButton", FindInPath<Button>(r, "GameFilters/FilterFlashTap"));
            SetRef(so, "filterOddOneOutButton", FindInPath<Button>(r, "GameFilters/FilterOddOneOut"));
            SetRef(so, "filterCognitiveSprintButton", FindInPath<Button>(r, "GameFilters/FilterCognitiveSprint"));

            // Content
            Transform scrollContent = r.Find("ScrollView/Viewport/Content");
            if (scrollContent != null) SetRef(so, "scrollContent", scrollContent);

            // MatchEntry Prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MATCH_ENTRY_PREFAB_PATH);
            if (prefab != null)
                SetRef(so, "matchEntryPrefab", prefab);
            else
                Debug.LogWarning("[MatchHistoryUI] MatchEntry prefab no encontrado. Crea el prefab primero.");

            // Empty text, loading, load more
            SetRef(so, "emptyText", FindInPath<TextMeshProUGUI>(r, "ScrollView/Viewport/Content/EmptyText"));
            Transform loading = r.Find("ScrollView/Viewport/Content/LoadingIndicator");
            if (loading != null) SetRef(so, "loadingIndicator", loading.gameObject);
            SetRef(so, "loadMoreButton", FindInPath<Button>(r, "ScrollView/Viewport/Content/LoadMoreButton"));

            // Animation sections
            SetRef(so, "headerTransform", FindInPath<RectTransform>(r, "Header"));
            SetRef(so, "gameFiltersTransform", FindInPath<RectTransform>(r, "GameFilters"));
            SetRef(so, "scrollViewTransform", FindInPath<RectTransform>(r, "ScrollView"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            Debug.Log("[MatchHistoryUI] Referencias del manager asignadas (20 campos)");
        }

        private static void SetRef(SerializedObject so, string propName, Object value)
        {
            var prop = so.FindProperty(propName);
            if (prop == null) { Debug.LogWarning($"[MatchHistoryUI] Property '{propName}' no encontrada"); return; }
            if (value != null) { prop.objectReferenceValue = value; Debug.Log($"[MatchHistoryUI] Asignado: {propName}"); }
            else { Debug.LogWarning($"[MatchHistoryUI] No se encontro valor para: {propName}"); }
        }

        private static T FindInPath<T>(Transform root, string path) where T : Component
        {
            Transform t = root;
            foreach (string part in path.Split('/'))
            {
                t = t.Find(part);
                if (t == null) return null;
            }
            return t.GetComponent<T>();
        }

        #endregion

        #region Helpers

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

        private static GameObject FindOrCreate(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null) return existing.gameObject;
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static T GetOrAdd<T>(GameObject obj) where T : Component
        {
            T c = obj.GetComponent<T>();
            if (c == null) c = obj.AddComponent<T>();
            return c;
        }

        private static void SetAnchorsWithPad(RectTransform rt, float bot, float top)
        {
            rt.anchorMin = new Vector2(0, bot);
            rt.anchorMax = new Vector2(1, top);
            rt.offsetMin = new Vector2(SIDE_PAD, 0);
            rt.offsetMax = new Vector2(-SIDE_PAD, 0);
        }

        private static void SetAnchors(RectTransform rt, float xMin, float yMin, float xMax, float yMax)
        {
            rt.anchorMin = new Vector2(xMin, yMin);
            rt.anchorMax = new Vector2(xMax, yMax);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        #endregion
    }
}
