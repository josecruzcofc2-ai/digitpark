using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;
using DigitPark.Monetization;
using DigitPark.Themes;
using ET = DigitPark.Themes.ThemeApplier.ElementType;

namespace DigitPark.Editor
{
    /// <summary>
    /// Construye la UI del Trophy Showcase para Achievements
    /// Diseño premium con vitrina de trofeos, efectos de cristal y animaciones
    /// </summary>
    public class AchievementsUIBuilder : EditorWindow
    {
        // ==================== COLORES DEL TEMA TROPHY SHOWCASE ====================
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.4f, 1f);
        private static readonly Color CYAN_GLOW = new Color(0f, 1f, 1f, 0.3f);

        private static readonly Color DARK_BG = new Color(0.02f, 0.04f, 0.08f, 1f);
        private static readonly Color PANEL_BG = new Color(0.05f, 0.08f, 0.12f, 0.98f);
        internal static readonly Color CARD_BG = new Color(0.04f, 0.07f, 0.11f, 1f);
        private static readonly Color HEADER_BG = new Color(0.03f, 0.05f, 0.08f, 0.95f);
        private static readonly Color POPUP_BG = new Color(0.04f, 0.06f, 0.1f, 0.98f);

        internal static readonly Color TEXT_PRIMARY = new Color(0.95f, 0.95f, 0.95f, 1f);
        internal static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.7f, 0.75f, 1f);
        internal static readonly Color TEXT_DARK = new Color(0.02f, 0.04f, 0.08f, 1f);

        private static readonly Color BUTTON_SECONDARY = new Color(0.12f, 0.16f, 0.22f, 1f);
        internal static readonly Color BUTTON_SUCCESS = new Color(0.2f, 0.8f, 0.4f, 1f);

        // Trophy Colors
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GOLD_DARK = new Color(0.7f, 0.55f, 0f, 1f);
        private static readonly Color SILVER = new Color(0.75f, 0.75f, 0.8f, 1f);
        private static readonly Color BRONZE = new Color(0.8f, 0.5f, 0.2f, 1f);

        // Category Colors
        private static readonly Color CAT_BEGINNER = new Color(0.3f, 0.7f, 1f, 1f);     // Light Blue
        private static readonly Color CAT_GAMES = new Color(0.5f, 1f, 0.5f, 1f);        // Light Green
        private static readonly Color CAT_COMPETITION = new Color(1f, 0.6f, 0.2f, 1f);  // Orange
        internal static readonly Color CAT_SECRET = new Color(0.8f, 0.2f, 1f, 1f);       // Purple

        // Trophy Card States
        internal static readonly Color TROPHY_UNLOCKED_GLOW = new Color(0f, 1f, 1f, 0.6f);
        internal static readonly Color TROPHY_LOCKED_BG = new Color(0.08f, 0.1f, 0.14f, 1f);
        internal static readonly Color TROPHY_PROGRESS_GLOW = new Color(1f, 0.84f, 0f, 0.5f);
        internal static readonly Color TROPHY_SECRET_GLOW = new Color(0.8f, 0.2f, 1f, 0.5f);

        internal static readonly Color GLASS_OVERLAY = new Color(1f, 1f, 1f, 0.08f);
        private static readonly Color BLOCKER_BG = new Color(0f, 0f, 0f, 0.9f);

        // ==================== PREFABS ====================
        private const string BACK_BUTTON_PREFAB = "Assets/_Project/Prefabs/Common/BackButton.prefab";

        // ==================== DIMENSIONES ====================
        private const float HEADER_HEIGHT = 100f;
        private const float TABS_HEIGHT = 65f;
        // 3 columns for mobile (1080px): (1080 - 40 padding - 30 spacing) / 3 = 336
        private const float TROPHY_CARD_WIDTH = 320f;
        private const float TROPHY_CARD_HEIGHT = 380f;
        private const float GRID_SPACING = 15f;
        private const float CONTENT_PADDING = 15f;
        private const float DETAIL_PANEL_HEIGHT = 880f;

        [MenuItem("DigitPark/Scenes/Build Scene/Monetization/Achievements", false, 140)]
        public static void BuildUI()
        {
            if (!EditorUtility.DisplayDialog("Trophy Showcase Builder",
                "Esto construira la UI del Trophy Showcase para Achievements.\n" +
                "Asegurate de tener la escena Achievements abierta.\n\n" +
                "Incluye:\n" +
                "- Vitrina de trofeos con efecto cristal\n" +
                "- Sistema de categorias\n" +
                "- Animaciones premium\n" +
                "- Panel de detalle\n\n" +
                "Continuar?",
                "Si", "No"))
                return;

            BuildTrophyShowcase();
        }

        /// <summary>Called by AllScenesBatchBuilder — no dialogs.</summary>
        public static void BuildSilent()
        {
            BuildTrophyShowcase();
        }

        private static void BuildTrophyShowcase()
        {
            Debug.Log("[TrophyShowcase] ========== INICIANDO CONSTRUCCION ==========");

            Canvas canvas = SetupCanvas();
            if (canvas == null) return;

            // Full clean of canvas children (keep TransitionCanvas and EventSystem)
            CleanupOldElements(canvas.transform);

            CreateBackground(canvas);
            GameObject safeArea = CreateSafeArea(canvas);

            CreateHeader(safeArea);
            CreateCategoryTabs(safeArea);
            CreateProgressSection(safeArea);
            CreateTrophyShowcaseGrid(safeArea);
            // CreateEmptyState removed — achievements always have content once loaded

            CreateDetailPanel(canvas);
            CreateRewardCelebration(canvas);

            CreateTrophyCardPrefab();

            MarkSceneDirty();
            AutoAssigners.AchievementsReferenceAssigner.RunAutoAssign();
            Debug.Log("[TrophyShowcase] ========== CONSTRUCCION COMPLETADA ==========");

            if (!AllScenesBatchBuilder.SilentMode)
                EditorUtility.DisplayDialog("Trophy Showcase Completado",
                    "UI del Trophy Showcase creada exitosamente.\n\n" +
                    "Elementos creados:\n" +
                    "- Header con progreso total\n" +
                    "- 5 tabs de categorias\n" +
                    "- Grid de trofeos (6 ejemplos)\n" +
                    "- Panel de detalle\n" +
                    "- Celebracion de recompensa\n" +
                    "- Prefab TrophyCard\n\n" +
                    "Asigna el AchievementsManager y conecta las referencias.",
                    "OK");
        }

        // ==================== CANVAS SETUP ====================

        private static Canvas SetupCanvas()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();

            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;

                canvasObj.AddComponent<GraphicRaycaster>();
            }

            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            if (Camera.main == null)
            {
                GameObject cameraObj = new GameObject("Main Camera");
                Camera cam = cameraObj.AddComponent<Camera>();
                cam.tag = "MainCamera";
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = DARK_BG;
            }

            return canvas;
        }

        /// <summary>
        /// Remove old UI elements before creating new Trophy Showcase
        /// </summary>
        private static void CleanupOldUI(Canvas canvas)
        {
            // No tocar TransitionCanvas ni EffectsCanvas
            if (canvas.gameObject.name.Contains("Transition") ||
                canvas.gameObject.name.Contains("Effects")) return;

            Debug.Log("[TrophyShowcase] Limpiando UI antigua...");

            // List of old element names to remove
            string[] oldElementsToRemove = new string[]
            {
                "SafeArea",
                "Background",
                "Header",
                "StatsPanel",
                "CategoryTabs",
                "CategoryTabsScroll",
                "TabsPanel",
                "AchievementsScrollView",
                "ScrollView",
                "DetailPanelBlocker",
                "RewardCelebration",
                "PurchaseBlocker",
                "NotEnoughBlocker",
                "TrophyShowcaseScrollView"
            };

            // Collect children to destroy (can't modify collection while iterating)
            var childrenToDestroy = new System.Collections.Generic.List<GameObject>();

            foreach (Transform child in canvas.transform)
            {
                // Check if this is an old element to remove
                foreach (string oldName in oldElementsToRemove)
                {
                    if (child.name == oldName || child.name.StartsWith(oldName))
                    {
                        childrenToDestroy.Add(child.gameObject);
                        break;
                    }
                }
            }

            // Destroy collected children
            foreach (var child in childrenToDestroy)
            {
                Debug.Log($"[TrophyShowcase] Eliminando: {child.name}");
                Object.DestroyImmediate(child);
            }

            // Also check for AchievementsManager and clear its references
            var manager = Object.FindObjectOfType<DigitPark.Managers.AchievementsManager>();
            if (manager != null)
            {
                Debug.Log("[TrophyShowcase] AchievementsManager encontrado - las referencias se deben reconectar");
            }

            Debug.Log("[TrophyShowcase] Limpieza completada");
        }

        private static void CleanupOldElements(Transform parent)
        {
            var toDestroy = new System.Collections.Generic.List<GameObject>();
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                string name = child.gameObject.name;
                if (name == "TransitionCanvas" || name == "EventSystem")
                    continue;
                toDestroy.Add(child.gameObject);
            }
            foreach (var go in toDestroy)
                Object.DestroyImmediate(go);
        }

        private static void CreateBackground(Canvas canvas)
        {
            GameObject bg = FindOrCreateChild(canvas.gameObject, "Background");

            RectTransform bgRT = GetOrAddComponent<RectTransform>(bg);
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;

            Image bgImage = GetOrAddComponent<Image>(bg);
            bgImage.color = Color.white; // ThemeApplier tints at runtime
            bgImage.raycastTarget = false;
            ThemeApplierHelper.Apply(bg, ET.PrimaryBackground);

            // Ambient particles effect (visual layer)
            GameObject particles = FindOrCreateChild(bg, "AmbientParticles");
            SetRectTransformStretch(particles);
            Image particlesImage = GetOrAddComponent<Image>(particles);
            particlesImage.color = new Color(0.1f, 0.2f, 0.3f, 0.1f);
            particlesImage.raycastTarget = false;

            bg.transform.SetAsFirstSibling();
        }

        private static GameObject CreateSafeArea(Canvas canvas)
        {
            GameObject safeArea = FindOrCreateChild(canvas.gameObject, "SafeArea");

            RectTransform safeRT = GetOrAddComponent<RectTransform>(safeArea);
            safeRT.anchorMin = Vector2.zero;
            safeRT.anchorMax = Vector2.one;
            safeRT.sizeDelta = Vector2.zero;

            safeArea.transform.SetSiblingIndex(1);
            return safeArea;
        }

        // ==================== HEADER ====================

        private static void CreateHeader(GameObject parent)
        {
            GameObject header = FindOrCreateChild(parent, "Header");

            RectTransform headerRT = GetOrAddComponent<RectTransform>(header);
            headerRT.anchorMin = new Vector2(0, 1);
            headerRT.anchorMax = new Vector2(1, 1);
            headerRT.pivot = new Vector2(0.5f, 1);
            headerRT.anchoredPosition = new Vector2(0, -29); // consistent top margin across all scenes
            headerRT.sizeDelta = new Vector2(0, HEADER_HEIGHT);

            Image headerBg = GetOrAddComponent<Image>(header);
            headerBg.color = HEADER_BG;

            CreateBottomGlow(header, GOLD);

            // BackButton - Neon Cyan prefab
            Transform oldBackBtn = header.transform.Find("BackButton");
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
                backBtn = FindOrCreateChild(header, "BackButton");
                GetOrAddComponent<Image>(backBtn).color = new Color(0, 0, 0, 0);
                GetOrAddComponent<Button>(backBtn);
                Debug.LogWarning("[TrophyShowcase] BackButton prefab not found, using fallback");
            }
            RectTransform backRT = GetOrAddComponent<RectTransform>(backBtn);
            backRT.anchorMin = new Vector2(0, 0.5f);
            backRT.anchorMax = new Vector2(0, 0.5f);
            backRT.pivot = new Vector2(0, 0.5f);
            backRT.anchoredPosition = new Vector2(20, 0);
            backRT.sizeDelta = new Vector2(50, 50);

            // Title — centered on screen
            GameObject titleObj = FindOrCreateChild(header, "TitleText");
            RectTransform titleRowRT = GetOrAddComponent<RectTransform>(titleObj);
            titleRowRT.anchorMin = new Vector2(0.07f, 0.05f);
            titleRowRT.anchorMax = new Vector2(0.42f, 0.95f);
            titleRowRT.pivot = new Vector2(0.5f, 0.5f);
            titleRowRT.sizeDelta = Vector2.zero;
            titleRowRT.anchoredPosition = Vector2.zero;

            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "ACHIEVEMENTS";
            titleText.fontSize = FontSizes.H4;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = CYAN_NEON;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = FontSizes.AutoMinTitle;
            titleText.fontSizeMax = FontSizes.H4;
            titleText.overflowMode = TextOverflowModes.Ellipsis;
            titleText.raycastTarget = false;

            // Currency pills (top-right of header, above progress section)
            var pills = CurrencyHeaderBarHelper.CreateCurrencyPills(header.transform);
            var pillsRT = pills.GetComponent<RectTransform>();
            pillsRT.anchorMin = new Vector2(0.52f, 0.5f);
            pillsRT.anchorMax = new Vector2(0.95f, 0.5f);
            pillsRT.pivot = new Vector2(0.5f, 0.5f);
            pillsRT.sizeDelta = new Vector2(0, 65);

            Debug.Log("[TrophyShowcase] Header creado");
        }

        // ==================== PROGRESS SECTION (below dropdown) ====================

        private static void CreateProgressSection(GameObject parent)
        {
            float yPos = HEADER_HEIGHT + 60 + 10; // below header + dropdown row + gap

            GameObject progressSection = FindOrCreateChild(parent, "ProgressSection");
            RectTransform progressSectionRT = GetOrAddComponent<RectTransform>(progressSection);
            progressSectionRT.anchorMin = new Vector2(0, 1);
            progressSectionRT.anchorMax = new Vector2(1, 1);
            progressSectionRT.pivot = new Vector2(0.5f, 1);
            progressSectionRT.anchoredPosition = new Vector2(0, -yPos);
            progressSectionRT.sizeDelta = new Vector2(-40, 50);

            VerticalLayoutGroup progressVlg = GetOrAddComponent<VerticalLayoutGroup>(progressSection);
            progressVlg.spacing = 4;
            progressVlg.padding = new RectOffset(0, 0, 0, 0);
            progressVlg.childAlignment = TextAnchor.MiddleCenter;
            progressVlg.childControlWidth = true;
            progressVlg.childControlHeight = true;

            // Progress Label Row
            GameObject progressLabelRow = FindOrCreateChild(progressSection, "LabelRow");
            HorizontalLayoutGroup labelRowHlg = GetOrAddComponent<HorizontalLayoutGroup>(progressLabelRow);
            labelRowHlg.childAlignment = TextAnchor.MiddleCenter;
            labelRowHlg.childControlWidth = true;
            labelRowHlg.childControlHeight = true;
            labelRowHlg.childForceExpandWidth = true;
            LayoutElement labelRowLE = GetOrAddComponent<LayoutElement>(progressLabelRow);
            labelRowLE.minHeight = 18;

            GameObject progressLabelLeft = FindOrCreateChild(progressLabelRow, "AchProgressLeft");
            TextMeshProUGUI progressLeftText = GetOrAddComponent<TextMeshProUGUI>(progressLabelLeft);
            progressLeftText.text = "Total Progress";
            progressLeftText.enableAutoSizing = true;
            progressLeftText.fontSizeMin = FontSizes.AutoMinSmall;
            progressLeftText.fontSizeMax = FontSizes.Body;
            progressLeftText.fontSize = FontSizes.Body;
            progressLeftText.fontStyle = FontStyles.Bold;
            progressLeftText.color = TEXT_SECONDARY;
            progressLeftText.alignment = TextAlignmentOptions.MidlineLeft;

            GameObject progressLabelRight = FindOrCreateChild(progressLabelRow, "AchProgressRight");
            TextMeshProUGUI progressRightText = GetOrAddComponent<TextMeshProUGUI>(progressLabelRight);
            progressRightText.text = "--/52 (--%)" ;
            progressRightText.fontSize = FontSizes.Body;
            progressRightText.fontStyle = FontStyles.Bold;
            progressRightText.color = CYAN_NEON;
            progressRightText.alignment = TextAlignmentOptions.MidlineRight;
            progressRightText.enableAutoSizing = true;
            progressRightText.fontSizeMin = FontSizes.AutoMinSmall;
            progressRightText.fontSizeMax = FontSizes.Body;

            // Overall Progress Bar (Slider)
            GameObject progressBar = FindOrCreateChild(progressSection, "OverallProgressBar");
            LayoutElement progressBarLE = GetOrAddComponent<LayoutElement>(progressBar);
            progressBarLE.minHeight = 14;
            progressBarLE.preferredHeight = 14;

            Image progressBarBg = GetOrAddComponent<Image>(progressBar);
            progressBarBg.color = new Color(0.08f, 0.1f, 0.14f, 1f);
            AddOutline(progressBar, CYAN_DARK * 0.5f);

            Slider progressSlider = GetOrAddComponent<Slider>(progressBar);
            progressSlider.minValue = 0;
            progressSlider.maxValue = 1;
            progressSlider.value = 0f;
            progressSlider.interactable = false;

            // Progress Fill Area
            GameObject fillArea = FindOrCreateChild(progressBar, "Fill Area");
            SetRectTransformStretch(fillArea);

            GameObject progressFill = FindOrCreateChild(fillArea, "Fill");
            RectTransform fillRT = GetOrAddComponent<RectTransform>(progressFill);
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = new Vector2(0f, 1);
            fillRT.sizeDelta = Vector2.zero;

            Image fillImage = GetOrAddComponent<Image>(progressFill);
            fillImage.color = GOLD;

            progressSlider.fillRect = fillRT;
            progressSlider.targetGraphic = progressBarBg;

            Debug.Log("[TrophyShowcase] ProgressSection creado");
        }

        // ==================== CATEGORY DROPDOWN ====================

        private static void CreateCategoryTabs(GameObject parent)
        {
            // Single dropdown replaces 12 scrollable tabs — cleaner and no truncation
            GameObject dropdownRow = FindOrCreateChild(parent, "CategoryDropdownRow");

            RectTransform rowRT = GetOrAddComponent<RectTransform>(dropdownRow);
            rowRT.anchorMin = new Vector2(0, 1);
            rowRT.anchorMax = new Vector2(1, 1);
            rowRT.pivot = new Vector2(0.5f, 1);
            rowRT.anchoredPosition = new Vector2(0, -HEADER_HEIGHT - 10);
            rowRT.sizeDelta = new Vector2(0, 60);

            // Dropdown container with padding
            GameObject dropdownObj = FindOrCreateChild(dropdownRow, "CategoryDropdown");
            RectTransform ddRT = GetOrAddComponent<RectTransform>(dropdownObj);
            ddRT.anchorMin = new Vector2(0.05f, 0.1f);
            ddRT.anchorMax = new Vector2(0.95f, 0.9f);
            ddRT.offsetMin = Vector2.zero;
            ddRT.offsetMax = Vector2.zero;

            // Background
            Image ddBg = GetOrAddComponent<Image>(dropdownObj);
            ddBg.color = PANEL_BG;
            AddOutline(dropdownObj, CYAN_NEON * 0.6f, 2);

            // TMP_Dropdown
            TMP_Dropdown dropdown = GetOrAddComponent<TMP_Dropdown>(dropdownObj);
            dropdown.ClearOptions();
            dropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "All", "Beginner", "Mastery", "Victories", "Streaks",
                "Cash Battle", "Tournaments", "Social", "Progression",
                "Collector", "Time", "Secret"
            });

            // Caption Text
            GameObject captionObj = FindOrCreateChild(dropdownObj, "CategoryDropdownLabel");
            RectTransform captionRT = GetOrAddComponent<RectTransform>(captionObj);
            captionRT.anchorMin = new Vector2(0, 0);
            captionRT.anchorMax = new Vector2(1, 1);
            captionRT.offsetMin = new Vector2(20, 0);
            captionRT.offsetMax = new Vector2(-50, 0);

            TextMeshProUGUI captionText = GetOrAddComponent<TextMeshProUGUI>(captionObj);
            captionText.text = "All";
            captionText.fontSize = FontSizes.BodyLarge;
            captionText.fontStyle = FontStyles.Bold;
            captionText.color = CYAN_NEON;
            captionText.alignment = TextAlignmentOptions.MidlineLeft;
            captionText.enableAutoSizing = true;
            captionText.fontSizeMin = FontSizes.AutoMinBody;
            captionText.fontSizeMax = FontSizes.BodyLarge;
            dropdown.captionText = captionText;

            // Arrow indicator
            GameObject arrowObj = FindOrCreateChild(dropdownObj, "Arrow");
            RectTransform arrowRT = GetOrAddComponent<RectTransform>(arrowObj);
            arrowRT.anchorMin = new Vector2(1, 0);
            arrowRT.anchorMax = new Vector2(1, 1);
            arrowRT.pivot = new Vector2(1, 0.5f);
            arrowRT.sizeDelta = new Vector2(40, 0);
            arrowRT.anchoredPosition = new Vector2(-10, 0);

            TextMeshProUGUI arrowText = GetOrAddComponent<TextMeshProUGUI>(arrowObj);
            arrowText.text = "▼";
            arrowText.fontSize = FontSizes.Body;
            arrowText.enableAutoSizing = true;
            arrowText.fontSizeMin = FontSizes.AutoMinBody;
            arrowText.fontSizeMax = arrowText.fontSize;
            arrowText.fontStyle = FontStyles.Bold;
            arrowText.color = CYAN_NEON;
            arrowText.alignment = TextAlignmentOptions.Center;

            // Template (dropdown list)
            GameObject template = FindOrCreateChild(dropdownObj, "Template");
            RectTransform templateRT = GetOrAddComponent<RectTransform>(template);
            templateRT.anchorMin = new Vector2(0, 0);
            templateRT.anchorMax = new Vector2(1, 0);
            templateRT.pivot = new Vector2(0.5f, 1);
            templateRT.anchoredPosition = Vector2.zero;
            templateRT.sizeDelta = new Vector2(0, 500);
            template.SetActive(false);

            Image templateBg = GetOrAddComponent<Image>(template);
            templateBg.color = new Color(0.04f, 0.06f, 0.1f, 0.98f);
            AddOutline(template, CYAN_NEON * 0.4f, 1);

            ScrollRect templateScroll = GetOrAddComponent<ScrollRect>(template);
            templateScroll.movementType = ScrollRect.MovementType.Clamped;
            templateScroll.scrollSensitivity = 40f;

            // Viewport
            GameObject viewport = FindOrCreateChild(template, "Viewport");
            SetRectTransformStretch(viewport);
            Image vpImg = GetOrAddComponent<Image>(viewport);
            vpImg.color = Color.clear;
            GetOrAddComponent<RectMask2D>(viewport);
            templateScroll.viewport = viewport.GetComponent<RectTransform>();

            // Content
            GameObject content = FindOrCreateChild(viewport, "Content");
            RectTransform contentRT = GetOrAddComponent<RectTransform>(content);
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.sizeDelta = new Vector2(0, 0);
            templateScroll.content = contentRT;

            ContentSizeFitter csf = GetOrAddComponent<ContentSizeFitter>(content);
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(content);
            vlg.spacing = 2;
            vlg.padding = new RectOffset(4, 4, 4, 4);
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;

            // Item template
            GameObject item = FindOrCreateChild(content, "Item");
            RectTransform itemRT = GetOrAddComponent<RectTransform>(item);
            itemRT.sizeDelta = new Vector2(0, 55);

            LayoutElement itemLE = GetOrAddComponent<LayoutElement>(item);
            itemLE.minHeight = 55;
            itemLE.preferredHeight = 55;

            Toggle toggle = GetOrAddComponent<Toggle>(item);

            Image itemBg = GetOrAddComponent<Image>(item);
            itemBg.color = new Color(0.06f, 0.08f, 0.12f, 1f);
            toggle.targetGraphic = itemBg;

            // Item Background (for highlight)
            GameObject itemBgObj = FindOrCreateChild(item, "Item Background");
            SetRectTransformStretch(itemBgObj);
            Image itemHighlight = GetOrAddComponent<Image>(itemBgObj);
            itemHighlight.color = new Color(0f, 1f, 1f, 0.15f);
            toggle.graphic = itemHighlight;

            // Item Checkmark (optional)
            GameObject checkmark = FindOrCreateChild(item, "Item Checkmark");
            RectTransform checkRT = GetOrAddComponent<RectTransform>(checkmark);
            checkRT.anchorMin = new Vector2(1, 0.5f);
            checkRT.anchorMax = new Vector2(1, 0.5f);
            checkRT.sizeDelta = new Vector2(30, 30);
            checkRT.anchoredPosition = new Vector2(-20, 0);
            Image checkImg = GetOrAddComponent<Image>(checkmark);
            checkImg.color = CYAN_NEON;

            // Item Label
            GameObject itemLabel = FindOrCreateChild(item, "Item Label");
            RectTransform labelRT = GetOrAddComponent<RectTransform>(itemLabel);
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = new Vector2(20, 0);
            labelRT.offsetMax = new Vector2(-50, 0);

            TextMeshProUGUI labelText = GetOrAddComponent<TextMeshProUGUI>(itemLabel);
            labelText.text = "All";
            labelText.fontSize = FontSizes.Body;
            labelText.fontStyle = FontStyles.Bold;
            labelText.color = TEXT_PRIMARY;
            labelText.alignment = TextAlignmentOptions.MidlineLeft;
            labelText.enableAutoSizing = true;
            labelText.fontSizeMin = FontSizes.AutoMinBody;
            labelText.fontSizeMax = FontSizes.Body;
            dropdown.itemText = labelText;

            // Configure dropdown template
            dropdown.template = templateRT;

            // Remove DropdownScrollFix if leftover from previous build (not needed here)
            var oldFix = dropdownObj.GetComponent<DigitPark.UI.DropdownScrollFix>();
            if (oldFix != null) DestroyImmediate(oldFix);

            Debug.Log("[TrophyShowcase] CategoryDropdown created with 12 categories");
        }

        // ==================== TROPHY SHOWCASE GRID ====================

        private static void CreateTrophyShowcaseGrid(GameObject parent)
        {
            float topOffset = HEADER_HEIGHT + 60 + 55 + 20; // header + dropdown + progress row + spacing

            GameObject scrollView = FindOrCreateChild(parent, "TrophyShowcaseScrollView");

            RectTransform scrollRT = GetOrAddComponent<RectTransform>(scrollView);
            scrollRT.anchorMin = Vector2.zero;
            scrollRT.anchorMax = Vector2.one;
            scrollRT.offsetMin = new Vector2(CONTENT_PADDING, CONTENT_PADDING);
            scrollRT.offsetMax = new Vector2(-CONTENT_PADDING, -topOffset);

            ScrollRect scrollRect = GetOrAddComponent<ScrollRect>(scrollView);
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.scrollSensitivity = 50f;

            Image scrollBg = GetOrAddComponent<Image>(scrollView);
            scrollBg.color = Color.clear;

            // Viewport
            GameObject viewport = FindOrCreateChild(scrollView, "Viewport");
            SetRectTransformStretch(viewport);
            RectTransform viewportRT = viewport.GetComponent<RectTransform>();
            Image vpImg = GetOrAddComponent<Image>(viewport);
            vpImg.color = Color.clear;
            vpImg.raycastTarget = true;
            GetOrAddComponent<RectMask2D>(viewport);
            scrollRect.viewport = viewportRT;

            // Content
            GameObject content = FindOrCreateChild(viewport, "Content");
            RectTransform contentRT = GetOrAddComponent<RectTransform>(content);
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.sizeDelta = new Vector2(0, 0);
            scrollRect.content = contentRT;

            ContentSizeFitter csf = GetOrAddComponent<ContentSizeFitter>(content);
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Grid Layout - 3 columns
            GridLayoutGroup grid = GetOrAddComponent<GridLayoutGroup>(content);
            grid.cellSize = new Vector2(TROPHY_CARD_WIDTH, TROPHY_CARD_HEIGHT);
            grid.spacing = new Vector2(GRID_SPACING, GRID_SPACING);
            grid.padding = new RectOffset(10, 10, 15, 30);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;

            // ==================== CREATE ALL 53 ACHIEVEMENT CARDS (V1) ====================

            // --- BEGINNER (4) ---
            CreateTrophyCard(content, "Trophy_first_game", "First Steps", 0, false, false, CAT_BEGINNER);
            CreateTrophyCard(content, "Trophy_tutorial_complete", "Apprentice", 0, false, false, CAT_BEGINNER);
            CreateTrophyCard(content, "Trophy_first_win", "First Victory", 100, true, false, GOLD);
            CreateTrophyCard(content, "Trophy_profile_complete", "Identity", 0, false, false, CAT_BEGINNER);

            // --- MASTERY (5) ---
            CreateTrophyCard(content, "Trophy_digitrush_master", "Digit Master", 45, false, true, CAT_GAMES);
            CreateTrophyCard(content, "Trophy_flashtap_master", "Lightning Reflexes", 30, false, true, CAT_GAMES);
            CreateTrophyCard(content, "Trophy_memorypairs_master", "Photographic Memory", 0, false, false, CAT_GAMES);
            CreateTrophyCard(content, "Trophy_quickmath_master", "Human Calculator", 20, false, true, CAT_GAMES);
            CreateTrophyCard(content, "Trophy_oddoneout_master", "Eagle Eye", 65, false, true, CAT_GAMES);

            // --- VICTORIES (5) ---
            CreateTrophyCard(content, "Trophy_wins_10", "Competitor", 80, false, true, new Color(0.2f, 0.8f, 0.4f, 1f));
            CreateTrophyCard(content, "Trophy_wins_50", "Veteran", 40, false, true, new Color(0.2f, 0.8f, 0.4f, 1f));
            CreateTrophyCard(content, "Trophy_wins_100", "Centurion", 15, false, true, new Color(0.2f, 0.8f, 0.4f, 1f));
            CreateTrophyCard(content, "Trophy_wins_500", "Legend", 5, false, true, new Color(0.2f, 0.8f, 0.4f, 1f));
            CreateTrophyCard(content, "Trophy_wins_1000", "Immortal", 0, false, false, new Color(0.2f, 0.8f, 0.4f, 1f));

            // --- STREAKS (4) ---
            CreateTrophyCard(content, "Trophy_streak_3", "On a Roll", 100, true, false, new Color(1f, 0.5f, 0.2f, 1f));
            CreateTrophyCard(content, "Trophy_streak_5", "Unstoppable", 60, false, true, new Color(1f, 0.5f, 0.2f, 1f));
            CreateTrophyCard(content, "Trophy_streak_10", "Domination", 0, false, false, new Color(1f, 0.5f, 0.2f, 1f));
            CreateTrophyCard(content, "Trophy_streak_20", "Invincible", 0, false, false, CAT_SECRET, true); // SECRET

            // --- CASH BATTLE (7) ---
            CreateTrophyCard(content, "Trophy_cash_first", "Bettor", 100, true, false, new Color(0.4f, 0.8f, 0.2f, 1f));
            CreateTrophyCard(content, "Trophy_cash_first_win", "Real Winner", 100, true, false, GOLD);
            CreateTrophyCard(content, "Trophy_cash_10_wins", "Serious Player", 50, false, true, new Color(0.4f, 0.8f, 0.2f, 1f));
            CreateTrophyCard(content, "Trophy_cash_50_wins", "High Roller", 20, false, true, new Color(0.4f, 0.8f, 0.2f, 1f));
            CreateTrophyCard(content, "Trophy_cash_100_wins", "Shark", 5, false, true, new Color(0.4f, 0.8f, 0.2f, 1f));
            CreateTrophyCard(content, "Trophy_cash_earnings_100", "First $100", 35, false, true, GOLD);
            CreateTrophyCard(content, "Trophy_cash_earnings_1000", "Thousand Club", 0, false, false, CAT_SECRET, true); // SECRET

            // --- TOURNAMENTS (5) ---
            CreateTrophyCard(content, "Trophy_tournament_first", "Participant", 100, true, false, CAT_COMPETITION);
            CreateTrophyCard(content, "Trophy_tournament_top3", "Podium", 0, false, false, CAT_COMPETITION);
            CreateTrophyCard(content, "Trophy_tournament_win", "Champion", 0, false, false, GOLD);
            CreateTrophyCard(content, "Trophy_tournament_5_wins", "Multi-Champion", 0, false, false, CAT_COMPETITION);
            CreateTrophyCard(content, "Trophy_tournament_create", "Organizer", 0, false, false, CAT_COMPETITION);

            // --- SOCIAL (5) ---
            CreateTrophyCard(content, "Trophy_friend_first", "First Friend", 100, true, false, new Color(0.4f, 0.6f, 1f, 1f));
            CreateTrophyCard(content, "Trophy_friends_10", "Popular", 70, false, true, new Color(0.4f, 0.6f, 1f, 1f));
            CreateTrophyCard(content, "Trophy_friends_50", "Influencer", 10, false, true, new Color(0.4f, 0.6f, 1f, 1f));
            CreateTrophyCard(content, "Trophy_challenge_friend", "Challenger", 0, false, false, new Color(0.4f, 0.6f, 1f, 1f));
            CreateTrophyCard(content, "Trophy_beat_friend", "Rival", 0, false, false, new Color(0.4f, 0.6f, 1f, 1f));

            // --- PROGRESSION (4) ---
            CreateTrophyCard(content, "Trophy_level_10", "Level 10", 100, true, false, new Color(0.8f, 0.6f, 1f, 1f));
            CreateTrophyCard(content, "Trophy_level_25", "Level 25", 60, false, true, new Color(0.8f, 0.6f, 1f, 1f));
            CreateTrophyCard(content, "Trophy_level_50", "Level 50", 30, false, true, new Color(0.8f, 0.6f, 1f, 1f));
            CreateTrophyCard(content, "Trophy_level_100", "Level 100", 0, false, false, new Color(0.8f, 0.6f, 1f, 1f));

            // --- COLLECTOR --- Reserved for V2

            // --- TIME (6) ---
            CreateTrophyCard(content, "Trophy_days_7", "One Week", 100, true, false, new Color(0.5f, 0.8f, 0.9f, 1f));
            CreateTrophyCard(content, "Trophy_days_30", "One Month", 50, false, true, new Color(0.5f, 0.8f, 0.9f, 1f));
            CreateTrophyCard(content, "Trophy_days_100", "100 Days", 15, false, true, new Color(0.5f, 0.8f, 0.9f, 1f));
            CreateTrophyCard(content, "Trophy_days_365", "One Year", 0, false, false, CAT_SECRET, true); // SECRET
            CreateTrophyCard(content, "Trophy_daily_streak_7", "Weekly Streak", 100, true, false, new Color(0.5f, 0.8f, 0.9f, 1f));
            CreateTrophyCard(content, "Trophy_daily_streak_30", "Monthly Streak", 25, false, true, new Color(0.5f, 0.8f, 0.9f, 1f));

            // --- SECRET (4) ---
            CreateTrophyCard(content, "Trophy_night_owl", "Night Owl", 0, false, false, CAT_SECRET, true);
            CreateTrophyCard(content, "Trophy_perfect_game", "Perfection", 0, false, false, CAT_SECRET, true);
            CreateTrophyCard(content, "Trophy_comeback_king", "Comeback King", 0, false, false, CAT_SECRET, true);
            CreateTrophyCard(content, "Trophy_speed_demon", "Speed Demon", 0, false, false, CAT_SECRET, true);

            Debug.Log("[TrophyShowcase] TrophyShowcaseGrid creado con 53 logros (V1)");
        }

        // CreateEmptyState removed — achievements always have content once loaded

        private static void CreateTrophyCard(GameObject parent, string name, string title, int progressPercent,
            bool isUnlocked, bool hasProgress, Color accentColor, bool isSecret = false)
        {
            GameObject card = FindOrCreateChild(parent, name);

            // Clean up old structure (TrophyArea, InfoSection) if rebuilding
            Transform oldTrophyArea = card.transform.Find("CardContainer/TrophyArea");
            if (oldTrophyArea != null) Object.DestroyImmediate(oldTrophyArea.gameObject);
            Transform oldInfoSection = card.transform.Find("CardContainer/InfoSection");
            if (oldInfoSection != null) Object.DestroyImmediate(oldInfoSection.gameObject);

            // ── CardContainer ──
            GameObject cardContainer = FindOrCreateChild(card, "CardContainer");
            SetRectTransformStretch(cardContainer);

            Image cardBg = GetOrAddComponent<Image>(cardContainer);

            // Determine glow color
            Color glowColor = isSecret ? TROPHY_SECRET_GLOW :
                              isUnlocked ? TROPHY_UNLOCKED_GLOW :
                              hasProgress ? TROPHY_PROGRESS_GLOW :
                              new Color(0.3f, 0.3f, 0.4f, 0.3f);

            // Card BG: tinted when unlocked, flat when locked
            if (isUnlocked)
            {
                cardBg.color = new Color(
                    0.05f + glowColor.r * 0.05f,
                    0.08f + glowColor.g * 0.05f,
                    0.12f + glowColor.b * 0.05f,
                    0.95f);
            }
            else
            {
                cardBg.color = CARD_BG;
            }

            // Outline on CardContainer
            AddOutline(cardContainer, glowColor, isUnlocked ? 2 : 1);

            // ── BorderGlow (child Image, stretch +4px, firstSibling) ──
            GameObject borderGlow = FindOrCreateChild(cardContainer, "BorderGlow");
            RectTransform borderRT = GetOrAddComponent<RectTransform>(borderGlow);
            borderRT.anchorMin = Vector2.zero;
            borderRT.anchorMax = Vector2.one;
            borderRT.sizeDelta = new Vector2(4, 4);
            borderRT.anchoredPosition = Vector2.zero;

            Image borderImage = GetOrAddComponent<Image>(borderGlow);
            borderImage.color = glowColor;
            borderImage.raycastTarget = false;
            borderGlow.transform.SetAsFirstSibling();

            // ── GlassOverlay ──
            GameObject glassOverlay = FindOrCreateChild(cardContainer, "GlassOverlay");
            SetRectTransformStretch(glassOverlay);
            RectTransform glassRT = glassOverlay.GetComponent<RectTransform>();
            glassRT.offsetMax = new Vector2(0, -TROPHY_CARD_HEIGHT * 0.4f);

            Image glassImage = GetOrAddComponent<Image>(glassOverlay);
            glassImage.color = new Color(1f, 1f, 1f, isUnlocked ? 0.1f : 0.05f);
            glassImage.raycastTarget = false;

            // ── TrophyIcon (direct child of CardContainer, 220x220, top-center) ──
            GameObject trophyIcon = FindOrCreateChild(cardContainer, "TrophyIcon");
            RectTransform trophyRT = GetOrAddComponent<RectTransform>(trophyIcon);
            trophyRT.anchorMin = new Vector2(0.5f, 1);
            trophyRT.anchorMax = new Vector2(0.5f, 1);
            trophyRT.pivot = new Vector2(0.5f, 1);
            trophyRT.anchoredPosition = new Vector2(0, -10);
            trophyRT.sizeDelta = new Vector2(220, 220);

            Image trophyImage = GetOrAddComponent<Image>(trophyIcon);
            trophyImage.raycastTarget = false;

            // TrophyIcon visibility & color by state
            if (isSecret)
            {
                trophyIcon.SetActive(false);
                trophyImage.color = new Color(0.4f, 0.4f, 0.5f, 0.5f);
            }
            else
            {
                trophyIcon.SetActive(true);
                trophyImage.color = isUnlocked ? Color.white : new Color(0.4f, 0.4f, 0.4f, 1f);
            }

            // ── TrophyShadow (child of TrophyIcon, 180x25, bottom) ──
            GameObject trophyShadow = FindOrCreateChild(trophyIcon, "TrophyShadow");
            RectTransform shadowRT = GetOrAddComponent<RectTransform>(trophyShadow);
            shadowRT.anchorMin = new Vector2(0.5f, 0);
            shadowRT.anchorMax = new Vector2(0.5f, 0);
            shadowRT.pivot = new Vector2(0.5f, 1);
            shadowRT.anchoredPosition = new Vector2(0, -5);
            shadowRT.sizeDelta = new Vector2(180, 25);

            Image shadowImage = GetOrAddComponent<Image>(trophyShadow);
            shadowImage.color = new Color(0f, 0f, 0f, 0.3f);
            shadowImage.raycastTarget = false;
            trophyShadow.SetActive(isUnlocked && !isSecret);

            // ── LockedOverlay (child of TrophyIcon, stretch) ──
            GameObject lockedOverlay = FindOrCreateChild(trophyIcon, "LockedOverlay");
            SetRectTransformStretch(lockedOverlay);

            Image lockedImage = GetOrAddComponent<Image>(lockedOverlay);
            lockedImage.color = new Color(0f, 0f, 0f, 0.5f);
            lockedImage.raycastTarget = false;
            lockedOverlay.SetActive(!isUnlocked && !isSecret);

            // ── QuestionMark (child of TrophyIcon, stretch) ──
            GameObject questionMark = FindOrCreateChild(trophyIcon, "QuestionMark");
            SetRectTransformStretch(questionMark);

            TextMeshProUGUI questionText = GetOrAddComponent<TextMeshProUGUI>(questionMark);
            questionText.text = "?";
            questionText.fontSize = FontSizes.Subtitle;
            questionText.enableAutoSizing = true;
            questionText.fontSizeMin = FontSizes.AutoMinBody;
            questionText.fontSizeMax = questionText.fontSize;
            questionText.fontStyle = FontStyles.Bold;
            questionText.color = CAT_SECRET;
            questionText.alignment = TextAlignmentOptions.Center;
            questionMark.SetActive(isSecret);

            // ── ProgressContainer (absolute pos, y=70, h=40) ──
            GameObject progressContainer = FindOrCreateChild(cardContainer, "ProgressContainer");
            RectTransform progressContainerRT = GetOrAddComponent<RectTransform>(progressContainer);
            progressContainerRT.anchorMin = new Vector2(0, 0);
            progressContainerRT.anchorMax = new Vector2(1, 0);
            progressContainerRT.pivot = new Vector2(0.5f, 0);
            progressContainerRT.anchoredPosition = new Vector2(0, 70);
            progressContainerRT.sizeDelta = new Vector2(-24, 40);

            VerticalLayoutGroup progressVlg = GetOrAddComponent<VerticalLayoutGroup>(progressContainer);
            progressVlg.spacing = 4;
            progressVlg.padding = new RectOffset(4, 4, 2, 2);
            progressVlg.childAlignment = TextAnchor.MiddleCenter;
            progressVlg.childControlWidth = true;
            progressVlg.childControlHeight = true;

            // Progress Background
            GameObject progressBg = FindOrCreateChild(progressContainer, "ProgressBackground");
            Image progressBgImage = GetOrAddComponent<Image>(progressBg);
            progressBgImage.color = new Color(0.1f, 0.12f, 0.15f, 1f);
            progressBgImage.raycastTarget = false;
            LayoutElement progressBgLE = GetOrAddComponent<LayoutElement>(progressBg);
            progressBgLE.minHeight = 10;
            progressBgLE.preferredHeight = 10;

            // Progress Fill (anchor-based)
            GameObject progressFill = FindOrCreateChild(progressBg, "ProgressFill");
            RectTransform fillRT = GetOrAddComponent<RectTransform>(progressFill);
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = new Vector2(progressPercent / 100f, 1);
            fillRT.sizeDelta = Vector2.zero;

            Image fillImage = GetOrAddComponent<Image>(progressFill);
            fillImage.color = accentColor;
            fillImage.raycastTarget = false;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillAmount = 0f;

            // Progress Text
            GameObject progressTextObj = FindOrCreateChild(progressContainer, "ProgressText");
            TextMeshProUGUI progressTmp = GetOrAddComponent<TextMeshProUGUI>(progressTextObj);
            progressTmp.text = $"{progressPercent}%";
            progressTmp.fontSize = FontSizes.Body;
            progressTmp.fontStyle = FontStyles.Bold;
            progressTmp.color = TEXT_SECONDARY;
            progressTmp.alignment = TextAlignmentOptions.Center;
            progressTmp.enableAutoSizing = true;
            progressTmp.fontSizeMin = FontSizes.AutoMinSmall;
            progressTmp.fontSizeMax = FontSizes.Body;
            LayoutElement progressTextLE = GetOrAddComponent<LayoutElement>(progressTextObj);
            progressTextLE.minHeight = 22;

            // Toggle visibility: active only when in-progress
            progressContainer.SetActive(hasProgress && !isUnlocked);

            // ── TitleText (absolute pos, y=15, h=60) ──
            GameObject titleObj = FindOrCreateChild(cardContainer, "TitleText");
            RectTransform titleRT = GetOrAddComponent<RectTransform>(titleObj);
            titleRT.anchorMin = new Vector2(0, 0);
            titleRT.anchorMax = new Vector2(1, 0);
            titleRT.pivot = new Vector2(0.5f, 0);
            titleRT.anchoredPosition = new Vector2(0, 15);
            titleRT.sizeDelta = new Vector2(-16, 60);

            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = isSecret ? "???" : title;
            titleText.fontSize = FontSizes.Body;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = isUnlocked ? Color.white : (isSecret ? CAT_SECRET : TEXT_SECONDARY);
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = FontSizes.AutoMinSmall;
            titleText.fontSizeMax = FontSizes.Body;
            titleText.enableWordWrapping = true;
            titleText.overflowMode = TextOverflowModes.Ellipsis;

            // ── CompletedBadge (always created, toggled with SetActive) ──
            GameObject completedBadge = FindOrCreateChild(cardContainer, "CompletedBadge");
            RectTransform badgeRT = GetOrAddComponent<RectTransform>(completedBadge);
            badgeRT.anchorMin = new Vector2(1, 1);
            badgeRT.anchorMax = new Vector2(1, 1);
            badgeRT.pivot = new Vector2(1, 1);
            badgeRT.anchoredPosition = new Vector2(-8, -8);
            badgeRT.sizeDelta = new Vector2(26, 26);

            Image badgeImage = GetOrAddComponent<Image>(completedBadge);
            badgeImage.color = BUTTON_SUCCESS;
            badgeImage.raycastTarget = false;

            GameObject checkmark = FindOrCreateChild(completedBadge, "Checkmark");
            RectTransform checkmarkRT = GetOrAddComponent<RectTransform>(checkmark);
            checkmarkRT.anchorMin = new Vector2(0.15f, 0.15f);
            checkmarkRT.anchorMax = new Vector2(0.85f, 0.85f);
            checkmarkRT.sizeDelta = Vector2.zero;
            // Remove any leftover TMP component
            var oldTMP = checkmark.GetComponent<TextMeshProUGUI>();
            if (oldTMP != null) DestroyImmediate(oldTMP);
            Image checkImg = GetOrAddComponent<Image>(checkmark);
            Sprite checkSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/UI/icon_checkmark.png");
            if (checkSprite != null) checkImg.sprite = checkSprite;
            checkImg.color = TEXT_DARK;
            checkImg.preserveAspect = true;
            checkImg.raycastTarget = false;
            completedBadge.SetActive(isUnlocked);

            // ── ShineEffect (inactive, for animations) ──
            GameObject shineEffect = FindOrCreateChild(cardContainer, "ShineEffect");
            RectTransform shineRT = GetOrAddComponent<RectTransform>(shineEffect);
            shineRT.anchorMin = new Vector2(0, 0);
            shineRT.anchorMax = new Vector2(0, 1);
            shineRT.pivot = new Vector2(0.5f, 0.5f);
            shineRT.anchoredPosition = new Vector2(-200, 0);
            shineRT.sizeDelta = new Vector2(60, 0);

            Image shineImage = GetOrAddComponent<Image>(shineEffect);
            shineImage.color = new Color(1f, 1f, 1f, 0.15f);
            shineImage.raycastTarget = false;
            shineEffect.SetActive(false);

            // Make card clickable
            Button cardButton = GetOrAddComponent<Button>(card);
            SetupButtonColors(cardButton, cardBg.color);
            cardButton.transition = Selectable.Transition.None;
        }

        // ==================== DETAIL PANEL ====================

        private static void CreateDetailPanel(Canvas canvas)
        {
            GameObject blocker = FindOrCreateChild(canvas.gameObject, "DetailPanelBlocker");
            blocker.SetActive(false);

            SetRectTransformStretch(blocker);
            Image blockerBg = GetOrAddComponent<Image>(blocker);
            blockerBg.color = BLOCKER_BG;
            Button blockerBtn = GetOrAddComponent<Button>(blocker);
            blockerBtn.transition = Selectable.Transition.None;
            blocker.transform.SetAsLastSibling();

            GameObject panel = FindOrCreateChild(blocker, "DetailPanel");
            RectTransform panelRT = GetOrAddComponent<RectTransform>(panel);
            panelRT.anchorMin = new Vector2(0.5f, 0.5f);
            panelRT.anchorMax = new Vector2(0.5f, 0.5f);
            panelRT.sizeDelta = new Vector2(780, DETAIL_PANEL_HEIGHT);

            Image panelBg = GetOrAddComponent<Image>(panel);
            panelBg.color = POPUP_BG;
            AddOutline(panel, CYAN_NEON, 2);

            // Add CanvasGroup for fade animations
            CanvasGroup panelCanvasGroup = GetOrAddComponent<CanvasGroup>(panel);
            panelCanvasGroup.alpha = 1f;

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(panel);
            vlg.spacing = 14;
            vlg.padding = new RectOffset(40, 40, 50, 30);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // ==================== RED CLOSE BUTTON (top-right corner) ====================
            GameObject closeBtn = FindOrCreateChild(panel, "CloseButton");
            RectTransform closeRT = GetOrAddComponent<RectTransform>(closeBtn);
            closeRT.anchorMin = new Vector2(1, 1);
            closeRT.anchorMax = new Vector2(1, 1);
            closeRT.pivot = new Vector2(1, 1);
            closeRT.anchoredPosition = new Vector2(-8, -8);
            closeRT.sizeDelta = new Vector2(80, 80);

            // Ignore layout so VLG doesn't control position
            LayoutElement closeBtnLE = GetOrAddComponent<LayoutElement>(closeBtn);
            closeBtnLE.ignoreLayout = true;

            Image closeBg = GetOrAddComponent<Image>(closeBtn);
            closeBg.color = new Color(0.85f, 0.15f, 0.15f, 1f); // RED

            Button closeButton = GetOrAddComponent<Button>(closeBtn);
            closeButton.targetGraphic = closeBg;
            ColorBlock closeColors = closeButton.colors;
            closeColors.normalColor = Color.white;
            closeColors.highlightedColor = new Color(1f, 0.85f, 0.85f, 1f);
            closeColors.pressedColor = new Color(0.6f, 0.1f, 0.1f, 1f);
            closeColors.selectedColor = Color.white;
            closeButton.colors = closeColors;

            Outline closeOutline = GetOrAddComponent<Outline>(closeBtn);
            closeOutline.effectColor = new Color(1f, 0.35f, 0.35f, 0.8f);
            closeOutline.effectDistance = new Vector2(2f, 2f);

            // Sprite close icon
            GameObject closeText = FindOrCreateChild(closeBtn, "Text");
            Image closeImg = GetOrAddComponent<Image>(closeText);
            Sprite closeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/UI/icon_close_x.png");
            if (closeSprite != null) closeImg.sprite = closeSprite;
            closeImg.color = TEXT_PRIMARY;
            closeImg.preserveAspect = true;
            closeImg.raycastTarget = false;
            SetRectTransformStretch(closeText);

            // ==================== TROPHY ICON ====================
            GameObject trophyObj = FindOrCreateChild(panel, "DetailTrophyIcon");
            Image trophyBg = GetOrAddComponent<Image>(trophyObj);
            trophyBg.color = GOLD;
            LayoutElement trophyLE = GetOrAddComponent<LayoutElement>(trophyObj);
            trophyLE.minWidth = 160;
            trophyLE.minHeight = 160;
            trophyLE.preferredWidth = 160;
            trophyLE.preferredHeight = 160;

            // ==================== TITLE (auto-sizing to prevent truncation) ====================
            GameObject titleObj = FindOrCreateChild(panel, "DetailTitle");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "First Victory";
            titleText.fontSize = FontSizes.H3;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = GOLD;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = FontSizes.AutoMinTitle;
            titleText.fontSizeMax = FontSizes.H3;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minHeight = 55;

            // ==================== DESCRIPTION (auto-sizing for translations) ====================
            GameObject descObj = FindOrCreateChild(panel, "DetailDescription");
            TextMeshProUGUI descText = GetOrAddComponent<TextMeshProUGUI>(descObj);
            descText.text = "Win your first game in any game mode.";
            descText.fontSize = FontSizes.Body;
            descText.fontStyle = FontStyles.Bold;
            descText.color = TEXT_SECONDARY;
            descText.alignment = TextAlignmentOptions.Center;
            descText.enableAutoSizing = true;
            descText.fontSizeMin = FontSizes.AutoMinBody;
            descText.fontSizeMax = FontSizes.Body;
            descText.enableWordWrapping = true;
            LayoutElement descLE = GetOrAddComponent<LayoutElement>(descObj);
            descLE.minHeight = 70;

            // ==================== CATEGORY TEXT ====================
            GameObject categoryObj = FindOrCreateChild(panel, "DetailCategoryText");
            TextMeshProUGUI categoryText = GetOrAddComponent<TextMeshProUGUI>(categoryObj);
            categoryText.text = "Category: Beginner";
            categoryText.fontSize = FontSizes.BodySmall;
            categoryText.fontStyle = FontStyles.Bold;
            categoryText.color = CYAN_NEON;
            categoryText.alignment = TextAlignmentOptions.Center;
            categoryText.enableAutoSizing = true;
            categoryText.fontSizeMin = FontSizes.AutoMinSmall;
            categoryText.fontSizeMax = FontSizes.BodySmall;
            LayoutElement categoryLE = GetOrAddComponent<LayoutElement>(categoryObj);
            categoryLE.minHeight = 32;

            // ==================== PROGRESS SECTION ====================
            GameObject progressSection = FindOrCreateChild(panel, "DetailProgressSection");
            VerticalLayoutGroup progVlg = GetOrAddComponent<VerticalLayoutGroup>(progressSection);
            progVlg.spacing = 8;
            progVlg.childAlignment = TextAnchor.MiddleCenter;
            progVlg.childControlWidth = true;
            progVlg.childControlHeight = true;
            progVlg.childForceExpandWidth = true;
            LayoutElement progLE = GetOrAddComponent<LayoutElement>(progressSection);
            progLE.minHeight = 65;

            // Progress Bar (Slider)
            GameObject progressBar = FindOrCreateChild(progressSection, "DetailProgressBar");
            Image progressBarBg = GetOrAddComponent<Image>(progressBar);
            progressBarBg.color = new Color(0.1f, 0.12f, 0.15f, 1f);
            AddOutline(progressBar, CYAN_DARK * 0.5f);
            LayoutElement progressBarLE = GetOrAddComponent<LayoutElement>(progressBar);
            progressBarLE.minHeight = 26;

            // Add Slider component
            Slider detailSlider = GetOrAddComponent<Slider>(progressBar);
            detailSlider.minValue = 0;
            detailSlider.maxValue = 1;
            detailSlider.value = 1f;
            detailSlider.interactable = false;

            // Fill Area
            GameObject detailFillArea = FindOrCreateChild(progressBar, "Fill Area");
            SetRectTransformStretch(detailFillArea);

            GameObject progressFill = FindOrCreateChild(detailFillArea, "Fill");
            RectTransform fillRT = GetOrAddComponent<RectTransform>(progressFill);
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = new Vector2(1f, 1);
            fillRT.sizeDelta = Vector2.zero;

            Image fillImage = GetOrAddComponent<Image>(progressFill);
            fillImage.color = BUTTON_SUCCESS;

            // Configure slider
            detailSlider.fillRect = fillRT;
            detailSlider.targetGraphic = progressBarBg;

            // Progress Text
            GameObject progressTextObj = FindOrCreateChild(progressSection, "DetailProgressText");
            TextMeshProUGUI progressTmp = GetOrAddComponent<TextMeshProUGUI>(progressTextObj);
            progressTmp.text = "1/1 Completed";
            progressTmp.fontSize = FontSizes.Body;
            progressTmp.fontStyle = FontStyles.Bold;
            progressTmp.color = BUTTON_SUCCESS;
            progressTmp.alignment = TextAlignmentOptions.Center;
            progressTmp.enableAutoSizing = true;
            progressTmp.fontSizeMin = FontSizes.AutoMinBody;
            progressTmp.fontSizeMax = FontSizes.Body;
            LayoutElement progressTextLE = GetOrAddComponent<LayoutElement>(progressTextObj);
            progressTextLE.minHeight = 30;

            // ==================== REWARD SECTION ====================
            GameObject rewardSection = FindOrCreateChild(panel, "DetailRewardSection");
            Image rewardBg = GetOrAddComponent<Image>(rewardSection);
            rewardBg.color = new Color(0.06f, 0.08f, 0.12f, 1f);
            AddOutline(rewardSection, GOLD * 0.5f);
            LayoutElement rewardSectionLE = GetOrAddComponent<LayoutElement>(rewardSection);
            rewardSectionLE.minHeight = 70;

            HorizontalLayoutGroup rewardHlg = GetOrAddComponent<HorizontalLayoutGroup>(rewardSection);
            rewardHlg.spacing = 12;
            rewardHlg.padding = new RectOffset(25, 25, 14, 14);
            rewardHlg.childAlignment = TextAnchor.MiddleCenter;
            rewardHlg.childControlWidth = false;
            rewardHlg.childControlHeight = true;

            // Reward label "Reward:"
            GameObject rewardLabel = FindOrCreateChild(rewardSection, "AchRewardLabel");
            TextMeshProUGUI rewardLabelTmp = GetOrAddComponent<TextMeshProUGUI>(rewardLabel);
            rewardLabelTmp.text = "Reward:";
            rewardLabelTmp.fontSize = FontSizes.BodySmall;
            rewardLabelTmp.fontStyle = FontStyles.Bold;
            rewardLabelTmp.color = TEXT_SECONDARY;
            rewardLabelTmp.alignment = TextAlignmentOptions.MidlineLeft;
            rewardLabelTmp.enableAutoSizing = true;
            rewardLabelTmp.fontSizeMin = FontSizes.AutoMinSmall;
            rewardLabelTmp.fontSizeMax = FontSizes.BodySmall;
            LayoutElement rewardLabelLE = GetOrAddComponent<LayoutElement>(rewardLabel);
            rewardLabelLE.minWidth = 140;
            rewardLabelLE.minHeight = 40;

            GameObject rewardIcon = FindOrCreateChild(rewardSection, "RewardIcon");
            Image rewardIconImage = GetOrAddComponent<Image>(rewardIcon);
            rewardIconImage.color = new Color(0.4f, 0.8f, 1f, 1f); // Gem blue
            LayoutElement rewardIconLE = GetOrAddComponent<LayoutElement>(rewardIcon);
            rewardIconLE.minWidth = 42;
            rewardIconLE.minHeight = 42;

            GameObject rewardAmount = FindOrCreateChild(rewardSection, "RewardAmount");
            TextMeshProUGUI rewardTmp = GetOrAddComponent<TextMeshProUGUI>(rewardAmount);
            rewardTmp.text = "50 DigitGems";
            rewardTmp.fontSize = FontSizes.Body;
            rewardTmp.fontStyle = FontStyles.Bold;
            rewardTmp.color = new Color(0.4f, 0.8f, 1f, 1f);
            rewardTmp.alignment = TextAlignmentOptions.MidlineLeft;
            rewardTmp.enableAutoSizing = true;
            rewardTmp.fontSizeMin = FontSizes.AutoMinBody;
            rewardTmp.fontSizeMax = FontSizes.Body;
            LayoutElement rewardAmountLE = GetOrAddComponent<LayoutElement>(rewardAmount);
            rewardAmountLE.flexibleWidth = 1;

            // ==================== CLAIM BUTTON ====================
            GameObject claimBtn = FindOrCreateChild(panel, "ClaimRewardButton");
            Image claimBg = GetOrAddComponent<Image>(claimBtn);
            claimBg.color = BUTTON_SUCCESS;
            Button claimButton = GetOrAddComponent<Button>(claimBtn);
            SetupButtonColors(claimButton, BUTTON_SUCCESS);
            LayoutElement claimLE = GetOrAddComponent<LayoutElement>(claimBtn);
            claimLE.minHeight = 70;

            GameObject claimText = FindOrCreateChild(claimBtn, "ClaimButtonText");
            TextMeshProUGUI claimTmp = GetOrAddComponent<TextMeshProUGUI>(claimText);
            claimTmp.text = "CLAIM REWARD";
            claimTmp.fontSize = FontSizes.BodyLarge;
            claimTmp.fontStyle = FontStyles.Bold;
            claimTmp.color = TEXT_DARK;
            claimTmp.alignment = TextAlignmentOptions.Center;
            claimTmp.enableAutoSizing = true;
            claimTmp.fontSizeMin = FontSizes.AutoMinBody;
            claimTmp.fontSizeMax = FontSizes.BodyLarge;
            SetRectTransformStretch(claimText);

            // ==================== CANCEL BUTTON ====================
            GameObject cancelBtn = FindOrCreateChild(panel, "CancelButton");
            Image cancelBg = GetOrAddComponent<Image>(cancelBtn);
            cancelBg.color = BUTTON_SECONDARY;
            Button cancelButton = GetOrAddComponent<Button>(cancelBtn);
            SetupButtonColors(cancelButton, BUTTON_SECONDARY);
            AddOutline(cancelBtn, CYAN_DARK * 0.5f);
            LayoutElement cancelLE = GetOrAddComponent<LayoutElement>(cancelBtn);
            cancelLE.minHeight = 55;

            GameObject cancelText = FindOrCreateChild(cancelBtn, "CancelButtonText");
            TextMeshProUGUI cancelTmp = GetOrAddComponent<TextMeshProUGUI>(cancelText);
            cancelTmp.text = "CANCEL";
            cancelTmp.fontSize = FontSizes.Body;
            cancelTmp.fontStyle = FontStyles.Bold;
            cancelTmp.color = TEXT_SECONDARY;
            cancelTmp.alignment = TextAlignmentOptions.Center;
            cancelTmp.enableAutoSizing = true;
            cancelTmp.fontSizeMin = FontSizes.AutoMinBody;
            cancelTmp.fontSizeMax = FontSizes.Body;
            SetRectTransformStretch(cancelText);

            Debug.Log("[TrophyShowcase] DetailPanel creado");
        }

        // ==================== REWARD CELEBRATION ====================

        private static void CreateRewardCelebration(Canvas canvas)
        {
            GameObject celebration = FindOrCreateChild(canvas.gameObject, "RewardCelebration");
            celebration.SetActive(false);

            SetRectTransformStretch(celebration);
            Image celebrationBg = GetOrAddComponent<Image>(celebration);
            celebrationBg.color = new Color(0f, 0f, 0f, 0.92f);
            celebration.transform.SetAsLastSibling();

            // Center Content (with dark card background)
            GameObject centerContent = FindOrCreateChild(celebration, "CenterContent");
            RectTransform centerRT = GetOrAddComponent<RectTransform>(centerContent);
            centerRT.anchorMin = new Vector2(0.5f, 0.5f);
            centerRT.anchorMax = new Vector2(0.5f, 0.5f);
            centerRT.sizeDelta = new Vector2(420, 480);

            // Background card
            Image cardBg = GetOrAddComponent<Image>(centerContent);
            cardBg.color = new Color(0.06f, 0.09f, 0.14f, 0.95f);
            Outline cardOutline = GetOrAddComponent<Outline>(centerContent);
            cardOutline.effectColor = new Color(GOLD.r, GOLD.g, GOLD.b, 0.4f);
            cardOutline.effectDistance = new Vector2(2, -2);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(centerContent);
            vlg.spacing = 25;
            vlg.padding = new RectOffset(30, 30, 25, 25);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // Trophy Icon (animated)
            GameObject trophyObj = FindOrCreateChild(centerContent, "CelebrationTrophyIcon");
            Image trophyImage = GetOrAddComponent<Image>(trophyObj);
            trophyImage.color = GOLD;
            LayoutElement trophyLE = GetOrAddComponent<LayoutElement>(trophyObj);
            trophyLE.minWidth = 120;
            trophyLE.minHeight = 120;
            trophyLE.preferredWidth = 120;
            trophyLE.preferredHeight = 120;

            // Glow behind trophy (separate child of celebration for manager reference)
            GameObject glowObj = FindOrCreateChild(celebration, "CelebrationGlow");
            RectTransform glowRT = GetOrAddComponent<RectTransform>(glowObj);
            glowRT.anchorMin = new Vector2(0.5f, 0.5f);
            glowRT.anchorMax = new Vector2(0.5f, 0.5f);
            glowRT.sizeDelta = new Vector2(200, 200);
            glowObj.transform.SetSiblingIndex(1); // Behind content

            Image glowImage = GetOrAddComponent<Image>(glowObj);
            glowImage.color = new Color(1f, 0.9f, 0.5f, 0.4f);

            // Title
            GameObject titleObj = FindOrCreateChild(centerContent, "CelebrationTitle");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "Achievement Unlocked!";
            titleText.fontSize = FontSizes.H3;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = GOLD;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = FontSizes.AutoMinTitle;
            titleText.fontSizeMax = FontSizes.H3;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minHeight = 40;

            // Achievement Name
            GameObject nameObj = FindOrCreateChild(centerContent, "CelebrationAchievementName");
            TextMeshProUGUI nameText = GetOrAddComponent<TextMeshProUGUI>(nameObj);
            nameText.text = "First Victory";
            nameText.fontSize = FontSizes.Subtitle;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = TEXT_PRIMARY;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.enableAutoSizing = true;
            nameText.fontSizeMin = FontSizes.AutoMinBody;
            nameText.fontSizeMax = FontSizes.Subtitle;
            LayoutElement nameLE = GetOrAddComponent<LayoutElement>(nameObj);
            nameLE.minHeight = 30;

            // Reward Display
            GameObject rewardDisplay = FindOrCreateChild(centerContent, "CelebrationRewardDisplay");
            HorizontalLayoutGroup rewardHlg = GetOrAddComponent<HorizontalLayoutGroup>(rewardDisplay);
            rewardHlg.spacing = 12;
            rewardHlg.childAlignment = TextAnchor.MiddleCenter;
            rewardHlg.childControlWidth = false;
            rewardHlg.childControlHeight = true;
            LayoutElement rewardDisplayLE = GetOrAddComponent<LayoutElement>(rewardDisplay);
            rewardDisplayLE.minHeight = 55;

            GameObject rewardIcon = FindOrCreateChild(rewardDisplay, "Icon");
            Image rewardIconImage = GetOrAddComponent<Image>(rewardIcon);
            rewardIconImage.color = new Color(0.4f, 0.8f, 1f, 1f);
            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(rewardIcon);
            iconLE.minWidth = 45;
            iconLE.minHeight = 45;

            GameObject rewardAmount = FindOrCreateChild(rewardDisplay, "Amount");
            TextMeshProUGUI rewardTmp = GetOrAddComponent<TextMeshProUGUI>(rewardAmount);
            rewardTmp.text = "+50";
            rewardTmp.fontSize = FontSizes.H3;
            rewardTmp.fontStyle = FontStyles.Bold;
            rewardTmp.color = new Color(0.4f, 0.8f, 1f, 1f);
            rewardTmp.alignment = TextAlignmentOptions.MidlineLeft;
            rewardTmp.enableAutoSizing = true;
            rewardTmp.fontSizeMin = FontSizes.AutoMinBody;
            rewardTmp.fontSizeMax = FontSizes.H3;
            LayoutElement amountLE = GetOrAddComponent<LayoutElement>(rewardAmount);
            amountLE.minWidth = 100;

            // Continue Button
            GameObject continueBtn = FindOrCreateChild(centerContent, "ContinueButton");
            Image continueBg = GetOrAddComponent<Image>(continueBtn);
            continueBg.color = CYAN_NEON;
            Button continueButton = GetOrAddComponent<Button>(continueBtn);
            SetupButtonColors(continueButton, CYAN_NEON);
            LayoutElement continueLE = GetOrAddComponent<LayoutElement>(continueBtn);
            continueLE.minHeight = 55;
            continueLE.minWidth = 250;

            GameObject continueText = FindOrCreateChild(continueBtn, "ContinueButtonText");
            TextMeshProUGUI continueTmp = GetOrAddComponent<TextMeshProUGUI>(continueText);
            continueTmp.text = "CONTINUE";
            continueTmp.fontSize = FontSizes.BodyLarge;
            continueTmp.fontStyle = FontStyles.Bold;
            continueTmp.color = TEXT_DARK;
            continueTmp.alignment = TextAlignmentOptions.Center;
            continueTmp.enableAutoSizing = true;
            continueTmp.fontSizeMin = FontSizes.AutoMinBody;
            continueTmp.fontSizeMax = FontSizes.BodyLarge;
            SetRectTransformStretch(continueText);

            Debug.Log("[TrophyShowcase] RewardCelebration creado");
        }

        // ==================== PREFAB CREATION ====================

        private static void CreateTrophyCardPrefab()
        {
            string prefabPath = "Assets/_Project/Prefabs/Monetization/TrophyCard.prefab";

            // Ensure directory exists
            string directory = System.IO.Path.GetDirectoryName(prefabPath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }

            // Create prefab object
            GameObject prefabRoot = new GameObject("TrophyCard");
            RectTransform rootRT = prefabRoot.AddComponent<RectTransform>();
            rootRT.sizeDelta = new Vector2(TROPHY_CARD_WIDTH, TROPHY_CARD_HEIGHT);

            // Add TrophyCardUI component
            // Note: This will be added when the script compiles
            // prefabRoot.AddComponent<DigitPark.UI.Items.TrophyCardUI>();

            // Card Container
            GameObject cardContainer = new GameObject("CardContainer");
            cardContainer.transform.SetParent(prefabRoot.transform, false);
            RectTransform containerRT = cardContainer.AddComponent<RectTransform>();
            containerRT.anchorMin = Vector2.zero;
            containerRT.anchorMax = Vector2.one;
            containerRT.sizeDelta = Vector2.zero;

            Image cardBg = cardContainer.AddComponent<Image>();
            cardBg.color = CARD_BG;

            Outline outline = cardContainer.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0.4f, 0.5f, 0.35f);
            outline.effectDistance = new Vector2(1.5f, 1.5f);

            // Glass Overlay
            GameObject glassOverlay = new GameObject("GlassOverlay");
            glassOverlay.transform.SetParent(cardContainer.transform, false);
            RectTransform glassRT = glassOverlay.AddComponent<RectTransform>();
            glassRT.anchorMin = Vector2.zero;
            glassRT.anchorMax = Vector2.one;
            glassRT.offsetMax = new Vector2(0, -TROPHY_CARD_HEIGHT * 0.4f);

            Image glassImage = glassOverlay.AddComponent<Image>();
            glassImage.color = GLASS_OVERLAY;

            // Border Glow
            GameObject borderGlow = new GameObject("BorderGlow");
            borderGlow.transform.SetParent(cardContainer.transform, false);
            RectTransform borderRT = borderGlow.AddComponent<RectTransform>();
            borderRT.anchorMin = Vector2.zero;
            borderRT.anchorMax = Vector2.one;
            borderRT.sizeDelta = new Vector2(4, 4);
            borderRT.anchoredPosition = Vector2.zero;

            Image borderImage = borderGlow.AddComponent<Image>();
            borderImage.color = new Color(0.3f, 0.3f, 0.4f, 0.3f);
            borderGlow.transform.SetAsFirstSibling();

            // Trophy Icon - large and prominent (x4 from original 70x70)
            GameObject trophyIcon = new GameObject("TrophyIcon");
            trophyIcon.transform.SetParent(cardContainer.transform, false);
            RectTransform trophyRT = trophyIcon.AddComponent<RectTransform>();
            trophyRT.anchorMin = new Vector2(0.5f, 1);
            trophyRT.anchorMax = new Vector2(0.5f, 1);
            trophyRT.pivot = new Vector2(0.5f, 1);
            trophyRT.anchoredPosition = new Vector2(0, -10);
            trophyRT.sizeDelta = new Vector2(220, 220);

            Image trophyImage = trophyIcon.AddComponent<Image>();
            trophyImage.color = new Color(0.3f, 0.3f, 0.35f, 1f);

            // Trophy Shadow
            GameObject trophyShadow = new GameObject("TrophyShadow");
            trophyShadow.transform.SetParent(trophyIcon.transform, false);
            RectTransform shadowRT = trophyShadow.AddComponent<RectTransform>();
            shadowRT.anchorMin = new Vector2(0.5f, 0);
            shadowRT.anchorMax = new Vector2(0.5f, 0);
            shadowRT.pivot = new Vector2(0.5f, 1);
            shadowRT.anchoredPosition = new Vector2(0, -5);
            shadowRT.sizeDelta = new Vector2(180, 25);

            Image shadowImage = trophyShadow.AddComponent<Image>();
            shadowImage.color = new Color(0f, 0f, 0f, 0.3f);

            // Locked Overlay
            GameObject lockedOverlay = new GameObject("LockedOverlay");
            lockedOverlay.transform.SetParent(trophyIcon.transform, false);
            RectTransform lockedRT = lockedOverlay.AddComponent<RectTransform>();
            lockedRT.anchorMin = Vector2.zero;
            lockedRT.anchorMax = Vector2.one;
            lockedRT.sizeDelta = Vector2.zero;

            Image lockedImage = lockedOverlay.AddComponent<Image>();
            lockedImage.color = new Color(0f, 0f, 0f, 0.5f);
            lockedOverlay.SetActive(false);

            // Question Mark (for secret)
            GameObject questionMark = new GameObject("QuestionMark");
            questionMark.transform.SetParent(trophyIcon.transform, false);
            RectTransform questionRT = questionMark.AddComponent<RectTransform>();
            questionRT.anchorMin = Vector2.zero;
            questionRT.anchorMax = Vector2.one;
            questionRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI questionText = questionMark.AddComponent<TextMeshProUGUI>();
            questionText.text = "?";
            questionText.fontSize = FontSizes.Subtitle;
            questionText.enableAutoSizing = true;
            questionText.fontSizeMin = FontSizes.AutoMinBody;
            questionText.fontSizeMax = questionText.fontSize;
            questionText.fontStyle = FontStyles.Bold;
            questionText.color = CAT_SECRET;
            questionText.alignment = TextAlignmentOptions.Center;
            questionMark.SetActive(false);

            // Progress Container - positioned above title with enough clearance
            GameObject progressContainer = new GameObject("ProgressContainer");
            progressContainer.transform.SetParent(cardContainer.transform, false);
            RectTransform progressContainerRT = progressContainer.AddComponent<RectTransform>();
            progressContainerRT.anchorMin = new Vector2(0, 0);
            progressContainerRT.anchorMax = new Vector2(1, 0);
            progressContainerRT.pivot = new Vector2(0.5f, 0);
            progressContainerRT.anchoredPosition = new Vector2(0, 70);
            progressContainerRT.sizeDelta = new Vector2(-24, 40);

            VerticalLayoutGroup progressVlg = progressContainer.AddComponent<VerticalLayoutGroup>();
            progressVlg.spacing = 4;
            progressVlg.padding = new RectOffset(4, 4, 2, 2);
            progressVlg.childAlignment = TextAnchor.MiddleCenter;
            progressVlg.childControlWidth = true;
            progressVlg.childControlHeight = true;

            // Progress Background
            GameObject progressBg = new GameObject("ProgressBackground");
            progressBg.transform.SetParent(progressContainer.transform, false);
            RectTransform progressBgRT = progressBg.AddComponent<RectTransform>();

            Image progressBgImage = progressBg.AddComponent<Image>();
            progressBgImage.color = new Color(0.1f, 0.12f, 0.15f, 1f);

            LayoutElement progressBgLE = progressBg.AddComponent<LayoutElement>();
            progressBgLE.minHeight = 10;
            progressBgLE.preferredHeight = 10;

            // Progress Fill
            GameObject progressFill = new GameObject("ProgressFill");
            progressFill.transform.SetParent(progressBg.transform, false);
            RectTransform fillRT = progressFill.AddComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = new Vector2(0.5f, 1);
            fillRT.sizeDelta = Vector2.zero;

            Image fillImage = progressFill.AddComponent<Image>();
            fillImage.color = GOLD;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillAmount = 0f;

            // Progress Text - use smaller font that fits the container
            GameObject progressText = new GameObject("ProgressText");
            progressText.transform.SetParent(progressContainer.transform, false);

            TextMeshProUGUI progressTmp = progressText.AddComponent<TextMeshProUGUI>();
            progressTmp.text = "50%";
            progressTmp.fontSize = FontSizes.Body;
            progressTmp.enableAutoSizing = true;
            progressTmp.fontSizeMin = FontSizes.AutoMinBody;
            progressTmp.fontSizeMax = progressTmp.fontSize;
            progressTmp.fontStyle = FontStyles.Bold;
            progressTmp.color = TEXT_SECONDARY;
            progressTmp.alignment = TextAlignmentOptions.Center;

            LayoutElement progressTextLE = progressText.AddComponent<LayoutElement>();
            progressTextLE.minHeight = 22;

            progressContainer.SetActive(false);

            // Title Text - x2 size, auto-sizing for long names
            GameObject titleText = new GameObject("TitleText");
            titleText.transform.SetParent(cardContainer.transform, false);
            RectTransform titleRT = titleText.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0);
            titleRT.anchorMax = new Vector2(1, 0);
            titleRT.pivot = new Vector2(0.5f, 0);
            titleRT.anchoredPosition = new Vector2(0, 15);
            titleRT.sizeDelta = new Vector2(-16, 60);

            TextMeshProUGUI titleTmp = titleText.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "Achievement";
            titleTmp.fontSize = FontSizes.Body;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = TEXT_SECONDARY;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.enableAutoSizing = true;
            titleTmp.fontSizeMin = FontSizes.AutoMinSmall;
            titleTmp.fontSizeMax = FontSizes.Body;
            titleTmp.enableWordWrapping = true;
            titleTmp.overflowMode = TextOverflowModes.Ellipsis;

            // Completed Badge
            GameObject completedBadge = new GameObject("CompletedBadge");
            completedBadge.transform.SetParent(cardContainer.transform, false);
            RectTransform badgeRT = completedBadge.AddComponent<RectTransform>();
            badgeRT.anchorMin = new Vector2(1, 1);
            badgeRT.anchorMax = new Vector2(1, 1);
            badgeRT.pivot = new Vector2(1, 1);
            badgeRT.anchoredPosition = new Vector2(-8, -8);
            badgeRT.sizeDelta = new Vector2(26, 26);

            Image badgeImage = completedBadge.AddComponent<Image>();
            badgeImage.color = BUTTON_SUCCESS;

            GameObject checkmark = new GameObject("Checkmark");
            checkmark.transform.SetParent(completedBadge.transform, false);
            RectTransform checkRT = checkmark.AddComponent<RectTransform>();
            checkRT.anchorMin = Vector2.zero;
            checkRT.anchorMax = Vector2.one;
            checkRT.sizeDelta = Vector2.zero;

            var checkImg = checkmark.AddComponent<Image>();
            Sprite checkSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/UI/icon_checkmark.png");
            if (checkSprite != null) checkImg.sprite = checkSprite;
            checkImg.color = TEXT_DARK;
            checkImg.preserveAspect = true;
            checkImg.raycastTarget = false;

            completedBadge.SetActive(false);

            // Shine Effect
            GameObject shineEffect = new GameObject("ShineEffect");
            shineEffect.transform.SetParent(cardContainer.transform, false);
            RectTransform shineRT = shineEffect.AddComponent<RectTransform>();
            shineRT.anchorMin = new Vector2(0, 0);
            shineRT.anchorMax = new Vector2(0, 1);
            shineRT.pivot = new Vector2(0.5f, 0.5f);
            shineRT.anchoredPosition = new Vector2(-200, 0);
            shineRT.sizeDelta = new Vector2(60, 0);

            Image shineImage = shineEffect.AddComponent<Image>();
            shineImage.color = new Color(1f, 1f, 1f, 0.15f);
            shineEffect.SetActive(false);

            // Save prefab
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            Object.DestroyImmediate(prefabRoot);

            Debug.Log($"[TrophyShowcase] TrophyCard prefab creado en: {prefabPath}");
        }

        // ==================== UTILITY METHODS ====================

        private static void CreateBottomGlow(GameObject obj, Color color)
        {
            GameObject glow = FindOrCreateChild(obj, "BottomGlow");
            RectTransform glowRT = GetOrAddComponent<RectTransform>(glow);
            glowRT.anchorMin = new Vector2(0, 0);
            glowRT.anchorMax = new Vector2(1, 0);
            glowRT.pivot = new Vector2(0.5f, 1);
            glowRT.anchoredPosition = Vector2.zero;
            glowRT.sizeDelta = new Vector2(0, 3);

            Image glowImage = GetOrAddComponent<Image>(glow);
            glowImage.color = color;
        }

        private static void SetRectTransformStretch(GameObject obj)
        {
            RectTransform rt = GetOrAddComponent<RectTransform>(obj);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }

        private static T GetOrAddComponent<T>(GameObject obj) where T : Component
        {
            T component = obj.GetComponent<T>();
            if (component == null)
                component = obj.AddComponent<T>();
            return component;
        }

        private static GameObject FindOrCreateChild(GameObject parent, string childName)
        {
            Transform child = parent.transform.Find(childName);
            if (child != null) return child.gameObject;

            GameObject newChild = new GameObject(childName);
            newChild.transform.SetParent(parent.transform, false);

            if (newChild.GetComponent<RectTransform>() == null)
                newChild.AddComponent<RectTransform>();

            return newChild;
        }

        private static void SetupButtonColors(Button btn, Color baseColor)
        {
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            btn.colors = colors;
        }

        private static void AddOutline(GameObject obj, Color color, float distance = 1.5f)
        {
            Outline outline = obj.GetComponent<Outline>();
            if (outline == null)
                outline = obj.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(distance, distance);
        }

        private static void MarkSceneDirty()
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
    }
}
