using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;
using DigitPark.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// UI Builder for CashTournamentCreate scene.
    /// Gold-themed form with cards for each setting section, preview panel, and create action bar.
    /// Includes integrated Reference Assigner for CashTournamentCreateManager.
    /// </summary>
    public class CashTournamentCreateUIBuilder : EditorWindow
    {
        // Reference Assigner state
        private Vector2 scrollPosition;
        private static int assignedCount = 0;
        private static int failedCount = 0;
        private static int alreadySetCount = 0;
        private static List<AssignResult> assignResults = new List<AssignResult>();

        private struct AssignResult
        {
            public string fieldName;
            public string status;
            public bool success;
            public Object assignedObject;
        }

        #region Colors - Gold Theme

        private static readonly Color GOLD_PRIMARY = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GOLD_DARK = new Color(0.85f, 0.65f, 0.13f, 1f);
        private static readonly Color GOLD_LIGHT = new Color(1f, 0.93f, 0.55f, 1f);
        private static readonly Color AMBER = new Color(1f, 0.75f, 0f, 1f);

        private static readonly Color BG_DARK = new Color(0.06f, 0.05f, 0.10f, 1f);
        private static readonly Color CARD_BG = new Color(0.12f, 0.1f, 0.15f, 0.95f);
        private static readonly Color CARD_BORDER = new Color(0.85f, 0.65f, 0.13f, 0.6f);

        private static readonly Color TEXT_PRIMARY = new Color(1f, 1f, 1f, 1f);
        private static readonly Color TEXT_GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.7f, 0.7f, 0.7f, 1f);

        private static readonly Color BUTTON_GOLD = new Color(0.85f, 0.65f, 0.13f, 1f);
        private static readonly Color CYAN_ACCENT = new Color(0f, 0.9f, 1f, 1f);
        private static readonly Color GREEN_GO = new Color(0.24f, 1f, 0.42f, 1f);
        private static readonly Color RED_CANCEL = new Color(1f, 0.2f, 0.4f, 1f);

        #endregion

        #region Paths

        private const string BACK_BUTTON_GOLD_PREFAB = "Assets/_Project/Prefabs/Common/BackButtonGold.prefab";

        #endregion

        [MenuItem("DigitPark/UI Builders/CashBattle/CashTournament Create", false, 184)]
        public static void ShowWindow()
        {
            GetWindow<CashTournamentCreateUIBuilder>("CashTournament Create Builder");
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // ========== SECCION 1: UI BUILDER ==========
            GUILayout.Label("CashTournament Create UI Builder", EditorStyles.boldLabel);
            GUILayout.Label("Gold-themed Tournament Creation Form", EditorStyles.miniLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Builds the Gold-themed UI for CashTournamentCreate:\n\n" +
                "- Header with BackButtonGold + title\n" +
                "- FormScroll with cards: Name, Game, EntryFee, Players, Schedule, Rules\n" +
                "- PreviewPanel (collapsible)\n" +
                "- ActionBar with creation fee + Create button",
                MessageType.Info);

            EditorGUILayout.Space(10);

            GUI.backgroundColor = GOLD_PRIMARY;
            if (GUILayout.Button("BUILD GOLD UI", GUILayout.Height(40)))
            {
                BuildCashTournamentCreateUI();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Clean Scene", GUILayout.Height(25)))
            {
                CleanScene();
            }

            // ========== SEPARADOR ==========
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // ========== SECCION 2: REFERENCE ASSIGNER ==========
            GUILayout.Label("Assign References", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "CashTournamentCreate")
            {
                EditorGUILayout.HelpBox($"Current scene: {currentScene}\nOpen CashTournamentCreate first.", MessageType.Warning);
            }

            MonoBehaviour targetManager = FindCashTournamentCreateManager();
            if (targetManager != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Manager:", GUILayout.Width(60));
                EditorGUILayout.ObjectField(targetManager, typeof(MonoBehaviour), true);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("CashTournamentCreateManager not found in scene.", MessageType.Warning);
            }

            EditorGUILayout.Space(5);

            GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
            if (GUILayout.Button("ASSIGN ALL REFERENCES", GUILayout.Height(36)))
            {
                ResetAssignState();
                SetupManagerReferences();
                Repaint();
            }
            GUI.backgroundColor = Color.white;

            DrawAssignResults();

            EditorGUILayout.EndScrollView();
        }

        #region Main Build Methods

        private static void BuildCashTournamentCreateUI()
        {
            CleanupOldUI();

            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontro Canvas. Abre la escena CashTournamentCreate primero.", "OK");
                return;
            }

            if (EditorUtility.DisplayDialog("Build Gold UI?",
                "This will completely rebuild the CashTournamentCreate UI with Gold theme.\n\nContinue?",
                "Yes, Build", "Cancel"))
            {
                BuildAllElements(canvas);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                Debug.Log("[CashTournamentCreateUIBuilder] Gold UI built successfully!");
            }
        }

        /// <summary>
        /// Builds the UI silently without confirmation dialogs. Used by batch builders.
        /// </summary>
        public static void BuildSilent()
        {
            CleanupOldUI();

            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("[CashTournamentCreateUIBuilder] Canvas not found - cannot build silently");
                return;
            }

            BuildAllElements(canvas);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("[CashTournamentCreateUIBuilder] UI built silently (batch mode)");
        }

        /// <summary>
        /// Alias for BuildSilent. Called by FontSizeBatchRebuilder and AllScenesBatchBuilder via reflection.
        /// </summary>
        public static void BuildGoldUI() => BuildSilent();

        private static void CleanScene()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            CleanupOldElements(canvas.transform);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[CashTournamentCreateUIBuilder] Scene cleaned.");
        }

        private static void CleanupOldUI()
        {
            string[] toClean = { "Background", "SafeArea" };
            foreach (var canvas in Object.FindObjectsOfType<Canvas>(true))
            {
                if (canvas.transform.parent != null) continue;
                if (canvas.gameObject.name.Contains("Transition") ||
                    canvas.gameObject.name.Contains("Effects")) continue;
                foreach (string name in toClean)
                {
                    Transform t = canvas.transform.Find(name);
                    if (t != null) Object.DestroyImmediate(t.gameObject);
                }
            }
        }

        private static void BuildAllElements(Canvas canvas)
        {
            Transform canvasTransform = canvas.transform;

            CleanupOldElements(canvasTransform);

            // Background
            CreateBackground(canvasTransform);

            // Safe Area
            GameObject safeArea = CreateSafeArea(canvasTransform);

            // Header
            CreateHeader(safeArea.transform);

            // FormScroll
            CreateFormScroll(safeArea.transform);

            // ActionBar
            CreateActionBar(safeArea.transform);

            // Loading Overlay (hidden)
            CreateLoadingOverlay(canvasTransform);

            Debug.Log("[CashTournamentCreateUIBuilder] All elements created.");
        }

        private static void CleanupOldElements(Transform parent)
        {
            List<GameObject> toDestroy = new List<GameObject>();
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                string name = child.gameObject.name;
                if (name == "TransitionCanvas" || name == "EventSystem")
                    continue;
                toDestroy.Add(child.gameObject);
            }

            foreach (var go in toDestroy)
            {
                DestroyImmediate(go);
            }
        }

        #endregion

        #region Background

        private static void CreateBackground(Transform parent)
        {
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(parent, false);
            bg.transform.SetAsFirstSibling();

            RectTransform rt = bg.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image img = bg.AddComponent<Image>();
            img.color = BG_DARK;
            img.raycastTarget = false;

            // Gold glow at top
            GameObject goldGlow = new GameObject("GoldGlow");
            goldGlow.transform.SetParent(bg.transform, false);

            RectTransform glowRT = goldGlow.AddComponent<RectTransform>();
            glowRT.anchorMin = new Vector2(0, 0.7f);
            glowRT.anchorMax = Vector2.one;
            glowRT.sizeDelta = Vector2.zero;

            Image glowImg = goldGlow.AddComponent<Image>();
            glowImg.color = new Color(1f, 0.8f, 0.3f, 0.06f);
            glowImg.raycastTarget = false;
        }

        #endregion

        #region Safe Area

        private static GameObject CreateSafeArea(Transform parent)
        {
            GameObject safeArea = new GameObject("SafeArea");
            safeArea.transform.SetParent(parent, false);

            RectTransform rt = safeArea.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            return safeArea;
        }

        #endregion

        #region Header (0-1x, 0.93-1y)

        private static void CreateHeader(Transform parent)
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(parent, false);

            RectTransform rt = header.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.93f);
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image bg = header.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.06f, 0.12f, 0.95f);

            // Back Button
            CreateBackButton(header.transform);

            // Title
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(header.transform, false);

            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.07f, 0f);
            titleRT.anchorMax = new Vector2(0.75f, 1f);
            titleRT.pivot = new Vector2(0.5f, 0.5f);
            titleRT.sizeDelta = Vector2.zero;
            titleRT.anchoredPosition = Vector2.zero;

            TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
            title.text = "CREATE TOURNAMENT";
            title.fontSize = FontSizes.H4;
            title.color = TEXT_GOLD;
            title.alignment = TextAlignmentOptions.MidlineLeft;
            title.fontStyle = FontStyles.Bold;
            title.enableAutoSizing = true;
            title.fontSizeMin = FontSizes.AutoMinTitle;
            title.fontSizeMax = FontSizes.H4;
            title.overflowMode = TextOverflowModes.Ellipsis;
            title.raycastTarget = false;
        }

        private static void CreateBackButton(Transform parent)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BACK_BUTTON_GOLD_PREFAB);
            if (prefab != null)
            {
                GameObject backBtn = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                backBtn.name = "BackButton";

                RectTransform rect = backBtn.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 0.5f);
                rect.anchorMax = new Vector2(0, 0.5f);
                rect.pivot = new Vector2(0, 0.5f);
                rect.sizeDelta = new Vector2(50, 50);
                rect.anchoredPosition = new Vector2(20, 0);
            }
            else
            {
                Debug.LogWarning("[CashTournamentCreate] BackButtonGold prefab not found, using fallback");

                GameObject backBtn = new GameObject("BackButton");
                backBtn.transform.SetParent(parent, false);

                RectTransform rt = backBtn.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 0.5f);
                rt.anchorMax = new Vector2(0, 0.5f);
                rt.pivot = new Vector2(0, 0.5f);
                rt.sizeDelta = new Vector2(50, 50);
                rt.anchoredPosition = new Vector2(20, 0);

                Image img = backBtn.AddComponent<Image>();
                img.color = Color.clear;

                Button btn = backBtn.AddComponent<Button>();
                btn.targetGraphic = img;

                GameObject arrowObj = new GameObject("Arrow");
                arrowObj.transform.SetParent(backBtn.transform, false);

                RectTransform arrowRT = arrowObj.AddComponent<RectTransform>();
                arrowRT.anchorMin = Vector2.zero;
                arrowRT.anchorMax = Vector2.one;
                arrowRT.sizeDelta = Vector2.zero;

                TextMeshProUGUI arrow = arrowObj.AddComponent<TextMeshProUGUI>();
                arrow.text = "<";
                arrow.fontSize = FontSizes.H4;
                arrow.color = TEXT_GOLD;
                arrow.alignment = TextAlignmentOptions.Center;
                arrow.fontStyle = FontStyles.Bold;
            }
        }

        #endregion

        #region FormScroll (0.02-0.98x, 0.12-0.92y)

        private static void CreateFormScroll(Transform parent)
        {
            GameObject scrollView = new GameObject("FormScroll");
            scrollView.transform.SetParent(parent, false);

            RectTransform svRT = scrollView.AddComponent<RectTransform>();
            svRT.anchorMin = new Vector2(0.02f, 0.12f);
            svRT.anchorMax = new Vector2(0.98f, 0.92f);
            svRT.sizeDelta = Vector2.zero;

            ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = 50;

            Image svBg = scrollView.AddComponent<Image>();
            svBg.color = Color.clear;
            svBg.raycastTarget = false;

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);

            RectTransform vpRT = viewport.AddComponent<RectTransform>();
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.sizeDelta = Vector2.zero;

            Image vpImg = viewport.AddComponent<Image>();
            vpImg.color = Color.clear;
            vpImg.raycastTarget = true;
            viewport.AddComponent<RectMask2D>();

            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);

            RectTransform contentRT = content.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.sizeDelta = new Vector2(0, 0);

            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 16;
            vlg.padding = new RectOffset(8, 8, 10, 30);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = vpRT;
            scroll.content = contentRT;

            // Form Cards
            CreateNameCard(content.transform);
            CreateGameCard(content.transform);
            CreateEntryFeeCard(content.transform);
            CreatePlayersCard(content.transform);
            CreateScheduleCard(content.transform);
            CreateRulesCard(content.transform);
            CreatePreviewPanel(content.transform);
        }

        /// <summary>
        /// Creates a FormCard container: CARD_BG bg, CARD_BORDER outline, VerticalLayoutGroup, padding 16.
        /// </summary>
        private static GameObject CreateFormCard(Transform parent, string name, float preferredHeight)
        {
            GameObject card = new GameObject(name);
            card.transform.SetParent(parent, false);

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredHeight = preferredHeight;

            Image bg = card.AddComponent<Image>();
            bg.color = CARD_BG;

            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = CARD_BORDER;
            outline.effectDistance = new Vector2(1, -1);

            VerticalLayoutGroup vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8;
            vlg.padding = new RectOffset(16, 16, 12, 12);
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            return card;
        }

        /// <summary>
        /// Creates a label text inside a form card (TEXT_SECONDARY, 14pt).
        /// </summary>
        private static TextMeshProUGUI CreateCardLabel(Transform parent, string goName, string labelText, float height = 30f)
        {
            GameObject obj = new GameObject(goName);
            obj.transform.SetParent(parent, false);

            LayoutElement le = obj.AddComponent<LayoutElement>();
            le.preferredHeight = height;

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = labelText;
            tmp.fontSize = FontSizes.Body;
            tmp.color = TEXT_SECONDARY;
            tmp.fontStyle = FontStyles.Normal;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.raycastTarget = false;

            return tmp;
        }

        // ==================== NAME CARD ====================
        private static void CreateNameCard(Transform parent)
        {
            GameObject card = CreateFormCard(parent, "NameCard", 130);

            CreateCardLabel(card.transform, "NameLabel", "Tournament Name");

            // TMP_InputField
            GameObject inputBg = new GameObject("TournamentNameInput");
            inputBg.transform.SetParent(card.transform, false);

            LayoutElement inputLE = inputBg.AddComponent<LayoutElement>();
            inputLE.preferredHeight = 55;

            Image bg = inputBg.AddComponent<Image>();
            bg.color = BG_DARK;

            Outline inputOutline = inputBg.AddComponent<Outline>();
            inputOutline.effectColor = GOLD_DARK;
            inputOutline.effectDistance = new Vector2(1, -1);

            // Text area
            GameObject textArea = new GameObject("Text");
            textArea.transform.SetParent(inputBg.transform, false);

            RectTransform textRT = textArea.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(12, 0);
            textRT.offsetMax = new Vector2(-12, 0);

            TextMeshProUGUI inputText = textArea.AddComponent<TextMeshProUGUI>();
            inputText.text = "";
            inputText.fontSize = FontSizes.H3;
            inputText.color = TEXT_PRIMARY;
            inputText.alignment = TextAlignmentOptions.Left;

            // Placeholder
            GameObject placeholder = new GameObject("Placeholder");
            placeholder.transform.SetParent(inputBg.transform, false);

            RectTransform phRT = placeholder.AddComponent<RectTransform>();
            phRT.anchorMin = Vector2.zero;
            phRT.anchorMax = Vector2.one;
            phRT.offsetMin = new Vector2(12, 0);
            phRT.offsetMax = new Vector2(-12, 0);

            TextMeshProUGUI phText = placeholder.AddComponent<TextMeshProUGUI>();
            phText.text = "Enter tournament name...";
            phText.fontSize = FontSizes.H3;
            phText.color = TEXT_SECONDARY;
            phText.alignment = TextAlignmentOptions.Left;

            TMP_InputField inputField = inputBg.AddComponent<TMP_InputField>();
            inputField.textViewport = textRT;
            inputField.textComponent = inputText;
            inputField.placeholder = phText;
            inputField.characterLimit = 40;

            // CharCountText
            GameObject charCount = new GameObject("CharCountText");
            charCount.transform.SetParent(card.transform, false);

            LayoutElement ccLE = charCount.AddComponent<LayoutElement>();
            ccLE.preferredHeight = 22;

            TextMeshProUGUI ccTMP = charCount.AddComponent<TextMeshProUGUI>();
            ccTMP.text = "0/40";
            ccTMP.fontSize = FontSizes.Body;
            ccTMP.color = TEXT_SECONDARY;
            ccTMP.alignment = TextAlignmentOptions.Right;
            ccTMP.raycastTarget = false;
        }

        // ==================== GAME CARD ====================
        private static void CreateGameCard(Transform parent)
        {
            GameObject card = CreateFormCard(parent, "GameCard", 130);

            CreateCardLabel(card.transform, "GameLabel", "Game");

            // Horizontal row: Dropdown + Icon
            GameObject row = new GameObject("GameRow");
            row.transform.SetParent(card.transform, false);

            LayoutElement rowLE = row.AddComponent<LayoutElement>();
            rowLE.preferredHeight = 60;

            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            // Dropdown
            CreateTMPDropdownInLayout(row.transform, "GameTypeDropdown",
                new List<string> { "Select Game", "DigitRush", "MemoryPairs", "QuickMath", "FlashTap", "OddOneOut" },
                flexWidth: 1f);

            // SelectedGameIcon
            GameObject iconObj = new GameObject("SelectedGameIcon");
            iconObj.transform.SetParent(row.transform, false);

            LayoutElement iconLE = iconObj.AddComponent<LayoutElement>();
            iconLE.preferredWidth = 40;
            iconLE.preferredHeight = 40;
            iconLE.flexibleWidth = 0;

            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.color = new Color(0.4f, 0.4f, 0.4f, 0.3f);
            iconImg.preserveAspect = true;
        }

        // ==================== ENTRY FEE CARD ====================
        private static void CreateEntryFeeCard(Transform parent)
        {
            GameObject card = CreateFormCard(parent, "EntryFeeCard", 180);

            CreateCardLabel(card.transform, "EntryFeeLabel", "Entry Fee");

            // Dropdown
            CreateTMPDropdownInLayout(card.transform, "EntryFeeDropdown",
                new List<string> { "$1", "$5", "$10", "$25", "$50", "$100" },
                preferredHeight: 50);

            // Slider row
            GameObject sliderRow = new GameObject("SliderRow");
            sliderRow.transform.SetParent(card.transform, false);

            LayoutElement sliderLE = sliderRow.AddComponent<LayoutElement>();
            sliderLE.preferredHeight = 40;

            // Slider
            CreateSlider(sliderRow.transform, "EntryFeeSlider");

            // Custom Input + Display row
            GameObject customRow = new GameObject("CustomRow");
            customRow.transform.SetParent(card.transform, false);

            LayoutElement customLE = customRow.AddComponent<LayoutElement>();
            customLE.preferredHeight = 40;

            HorizontalLayoutGroup customHLG = customRow.AddComponent<HorizontalLayoutGroup>();
            customHLG.spacing = 10;
            customHLG.childAlignment = TextAnchor.MiddleLeft;
            customHLG.childForceExpandWidth = false;
            customHLG.childForceExpandHeight = true;
            customHLG.childControlWidth = true;
            customHLG.childControlHeight = true;

            // Custom Input
            GameObject customInput = new GameObject("CustomEntryFeeInput");
            customInput.transform.SetParent(customRow.transform, false);

            LayoutElement ciLE = customInput.AddComponent<LayoutElement>();
            ciLE.flexibleWidth = 1;
            ciLE.preferredHeight = 36;

            Image ciBg = customInput.AddComponent<Image>();
            ciBg.color = BG_DARK;

            Outline ciOutline = customInput.AddComponent<Outline>();
            ciOutline.effectColor = GOLD_DARK;
            ciOutline.effectDistance = new Vector2(1, -1);

            GameObject ciTextObj = new GameObject("Text");
            ciTextObj.transform.SetParent(customInput.transform, false);
            RectTransform ciTextRT = ciTextObj.AddComponent<RectTransform>();
            ciTextRT.anchorMin = Vector2.zero;
            ciTextRT.anchorMax = Vector2.one;
            ciTextRT.offsetMin = new Vector2(10, 0);
            ciTextRT.offsetMax = new Vector2(-10, 0);
            TextMeshProUGUI ciText = ciTextObj.AddComponent<TextMeshProUGUI>();
            ciText.text = "";
            ciText.fontSize = FontSizes.Body;
            ciText.color = TEXT_PRIMARY;
            ciText.alignment = TextAlignmentOptions.Left;

            GameObject ciPh = new GameObject("Placeholder");
            ciPh.transform.SetParent(customInput.transform, false);
            RectTransform ciPhRT = ciPh.AddComponent<RectTransform>();
            ciPhRT.anchorMin = Vector2.zero;
            ciPhRT.anchorMax = Vector2.one;
            ciPhRT.offsetMin = new Vector2(10, 0);
            ciPhRT.offsetMax = new Vector2(-10, 0);
            TextMeshProUGUI ciPhText = ciPh.AddComponent<TextMeshProUGUI>();
            ciPhText.text = "Custom...";
            ciPhText.fontSize = FontSizes.Body;
            ciPhText.color = TEXT_SECONDARY;
            ciPhText.alignment = TextAlignmentOptions.Left;

            TMP_InputField ciInput = customInput.AddComponent<TMP_InputField>();
            ciInput.textViewport = ciTextRT;
            ciInput.textComponent = ciText;
            ciInput.placeholder = ciPhText;
            ciInput.contentType = TMP_InputField.ContentType.DecimalNumber;
            ciInput.characterLimit = 6;

            // Display Text
            GameObject displayObj = new GameObject("EntryFeeDisplayText");
            displayObj.transform.SetParent(customRow.transform, false);

            LayoutElement dispLE = displayObj.AddComponent<LayoutElement>();
            dispLE.preferredWidth = 160;
            dispLE.flexibleWidth = 0;

            TextMeshProUGUI dispTMP = displayObj.AddComponent<TextMeshProUGUI>();
            dispTMP.text = "$5.00";
            dispTMP.fontSize = FontSizes.H4;
            dispTMP.color = TEXT_GOLD;
            dispTMP.fontStyle = FontStyles.Bold;
            dispTMP.alignment = TextAlignmentOptions.Right;
        }

        // ==================== PLAYERS CARD ====================
        private static void CreatePlayersCard(Transform parent)
        {
            GameObject card = CreateFormCard(parent, "PlayersCard", 130);

            CreateCardLabel(card.transform, "PlayersLabel", "Max Players");

            // Dropdown
            CreateTMPDropdownInLayout(card.transform, "MaxPlayersDropdown",
                new List<string> { "4", "8", "16", "32" },
                preferredHeight: 50);

            // Estimated Prize
            GameObject prizeObj = new GameObject("EstimatedPrizeText");
            prizeObj.transform.SetParent(card.transform, false);

            LayoutElement prizeLE = prizeObj.AddComponent<LayoutElement>();
            prizeLE.preferredHeight = 28;

            TextMeshProUGUI prizeTMP = prizeObj.AddComponent<TextMeshProUGUI>();
            prizeTMP.text = "Estimated prize: $28.00";
            prizeTMP.fontSize = FontSizes.Body;
            prizeTMP.color = TEXT_GOLD;
            prizeTMP.fontStyle = FontStyles.Bold;
            prizeTMP.alignment = TextAlignmentOptions.Left;
            prizeTMP.raycastTarget = false;
        }

        // ==================== SCHEDULE CARD ====================
        private static void CreateScheduleCard(Transform parent)
        {
            GameObject card = CreateFormCard(parent, "ScheduleCard", 160);

            CreateCardLabel(card.transform, "ScheduleLabel", "Schedule");

            // Start Immediately Toggle
            GameObject toggleRow = new GameObject("ToggleRow");
            toggleRow.transform.SetParent(card.transform, false);

            LayoutElement toggleLE = toggleRow.AddComponent<LayoutElement>();
            toggleLE.preferredHeight = 40;

            HorizontalLayoutGroup toggleHLG = toggleRow.AddComponent<HorizontalLayoutGroup>();
            toggleHLG.spacing = 10;
            toggleHLG.childAlignment = TextAnchor.MiddleLeft;
            toggleHLG.childForceExpandWidth = false;
            toggleHLG.childForceExpandHeight = true;
            toggleHLG.childControlWidth = true;
            toggleHLG.childControlHeight = true;

            CreateToggle(toggleRow.transform, "StartImmediatelyToggle", true);

            GameObject toggleLabel = new GameObject("ToggleLabel");
            toggleLabel.transform.SetParent(toggleRow.transform, false);
            LayoutElement tlLE = toggleLabel.AddComponent<LayoutElement>();
            tlLE.flexibleWidth = 1;
            TextMeshProUGUI tlTMP = toggleLabel.AddComponent<TextMeshProUGUI>();
            tlTMP.text = "Start Immediately";
            tlTMP.fontSize = FontSizes.Body;
            tlTMP.color = TEXT_PRIMARY;
            tlTMP.alignment = TextAlignmentOptions.Left;

            // TimePicker Dropdown
            CreateTMPDropdownInLayout(card.transform, "StartTimeDropdown",
                new List<string> { "Now", "In 30 min", "In 1 hour", "In 2 hours", "In 6 hours", "In 24 hours" },
                preferredHeight: 50);

            // Scheduled time text
            GameObject scheduledObj = new GameObject("ScheduledTimeText");
            scheduledObj.transform.SetParent(card.transform, false);
            LayoutElement stLE = scheduledObj.AddComponent<LayoutElement>();
            stLE.preferredHeight = 22;
            TextMeshProUGUI stTMP = scheduledObj.AddComponent<TextMeshProUGUI>();
            stTMP.text = "Starts: Now";
            stTMP.fontSize = FontSizes.Body;
            stTMP.color = TEXT_SECONDARY;
            stTMP.alignment = TextAlignmentOptions.Left;
            stTMP.raycastTarget = false;
        }

        // ==================== RULES CARD ====================
        private static void CreateRulesCard(Transform parent)
        {
            GameObject card = CreateFormCard(parent, "RulesCard", 320);

            CreateCardLabel(card.transform, "RulesLabel", "Rules");

            // Rounds Dropdown
            CreateCardLabel(card.transform, "RoundsLabel", "Rounds", 22);
            CreateTMPDropdownInLayout(card.transform, "RoundsDropdown",
                new List<string> { "1", "2", "3", "4", "5" },
                preferredHeight: 45);

            // Time Limit Dropdown
            CreateCardLabel(card.transform, "TimeLimitLabel", "Time Limit", 22);
            CreateTMPDropdownInLayout(card.transform, "TimeLimitDropdown",
                new List<string> { "30s", "60s", "90s", "120s", "180s" },
                preferredHeight: 45);

            // Max Attempts Dropdown
            CreateCardLabel(card.transform, "MaxAttemptsLabel", "Max Attempts", 22);
            CreateTMPDropdownInLayout(card.transform, "MaxAttemptsDropdown",
                new List<string> { "1", "2", "3", "5", "Unlimited" },
                preferredHeight: 45);

            // Spectators Toggle Row
            GameObject specRow = new GameObject("SpectatorsRow");
            specRow.transform.SetParent(card.transform, false);
            LayoutElement specLE = specRow.AddComponent<LayoutElement>();
            specLE.preferredHeight = 36;
            HorizontalLayoutGroup specHLG = specRow.AddComponent<HorizontalLayoutGroup>();
            specHLG.spacing = 10;
            specHLG.childAlignment = TextAnchor.MiddleLeft;
            specHLG.childForceExpandWidth = false;
            specHLG.childForceExpandHeight = true;
            specHLG.childControlWidth = true;
            specHLG.childControlHeight = true;

            CreateToggle(specRow.transform, "AllowSpectatorsToggle", true);
            GameObject specLabel = new GameObject("SpectatorsLabel");
            specLabel.transform.SetParent(specRow.transform, false);
            LayoutElement slLE = specLabel.AddComponent<LayoutElement>();
            slLE.flexibleWidth = 1;
            TextMeshProUGUI slTMP = specLabel.AddComponent<TextMeshProUGUI>();
            slTMP.text = "Allow Spectators";
            slTMP.fontSize = FontSizes.Body;
            slTMP.color = TEXT_PRIMARY;
            slTMP.alignment = TextAlignmentOptions.Left;

            // Private Toggle Row
            GameObject privRow = new GameObject("PrivateRow");
            privRow.transform.SetParent(card.transform, false);
            LayoutElement privLE = privRow.AddComponent<LayoutElement>();
            privLE.preferredHeight = 36;
            HorizontalLayoutGroup privHLG = privRow.AddComponent<HorizontalLayoutGroup>();
            privHLG.spacing = 10;
            privHLG.childAlignment = TextAnchor.MiddleLeft;
            privHLG.childForceExpandWidth = false;
            privHLG.childForceExpandHeight = true;
            privHLG.childControlWidth = true;
            privHLG.childControlHeight = true;

            CreateToggle(privRow.transform, "PrivateToggle", false);
            GameObject privLabel = new GameObject("PrivateLabel");
            privLabel.transform.SetParent(privRow.transform, false);
            LayoutElement plLE = privLabel.AddComponent<LayoutElement>();
            plLE.flexibleWidth = 1;
            TextMeshProUGUI plTMP = privLabel.AddComponent<TextMeshProUGUI>();
            plTMP.text = "Private Tournament";
            plTMP.fontSize = FontSizes.Body;
            plTMP.color = TEXT_PRIMARY;
            plTMP.alignment = TextAlignmentOptions.Left;

            // Private Code Input (hidden by default)
            GameObject privateCodeBg = new GameObject("PrivateCodeInput");
            privateCodeBg.transform.SetParent(card.transform, false);
            privateCodeBg.SetActive(false);

            LayoutElement pcLE = privateCodeBg.AddComponent<LayoutElement>();
            pcLE.preferredHeight = 45;

            Image pcBg = privateCodeBg.AddComponent<Image>();
            pcBg.color = BG_DARK;

            Outline pcOutline = privateCodeBg.AddComponent<Outline>();
            pcOutline.effectColor = GOLD_DARK;
            pcOutline.effectDistance = new Vector2(1, -1);

            GameObject pcTextObj = new GameObject("Text");
            pcTextObj.transform.SetParent(privateCodeBg.transform, false);
            RectTransform pcTextRT = pcTextObj.AddComponent<RectTransform>();
            pcTextRT.anchorMin = Vector2.zero;
            pcTextRT.anchorMax = Vector2.one;
            pcTextRT.offsetMin = new Vector2(12, 0);
            pcTextRT.offsetMax = new Vector2(-12, 0);
            TextMeshProUGUI pcText = pcTextObj.AddComponent<TextMeshProUGUI>();
            pcText.text = "";
            pcText.fontSize = FontSizes.Body;
            pcText.color = TEXT_PRIMARY;
            pcText.alignment = TextAlignmentOptions.Left;

            GameObject pcPh = new GameObject("Placeholder");
            pcPh.transform.SetParent(privateCodeBg.transform, false);
            RectTransform pcPhRT = pcPh.AddComponent<RectTransform>();
            pcPhRT.anchorMin = Vector2.zero;
            pcPhRT.anchorMax = Vector2.one;
            pcPhRT.offsetMin = new Vector2(12, 0);
            pcPhRT.offsetMax = new Vector2(-12, 0);
            TextMeshProUGUI pcPhText = pcPh.AddComponent<TextMeshProUGUI>();
            pcPhText.text = "Enter private code...";
            pcPhText.fontSize = FontSizes.Body;
            pcPhText.color = TEXT_SECONDARY;
            pcPhText.alignment = TextAlignmentOptions.Left;

            TMP_InputField pcInput = privateCodeBg.AddComponent<TMP_InputField>();
            pcInput.textViewport = pcTextRT;
            pcInput.textComponent = pcText;
            pcInput.placeholder = pcPhText;
            pcInput.characterLimit = 10;
        }

        // ==================== PREVIEW PANEL ====================
        private static void CreatePreviewPanel(Transform parent)
        {
            GameObject card = CreateFormCard(parent, "PreviewPanel", 200);

            // Title
            GameObject titleObj = new GameObject("PreviewTitle");
            titleObj.transform.SetParent(card.transform, false);
            LayoutElement titleLE = titleObj.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 30;
            TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "Preview";
            titleTMP.fontSize = FontSizes.H3;
            titleTMP.color = TEXT_GOLD;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Left;

            // Separator
            GameObject sep = new GameObject("Separator");
            sep.transform.SetParent(card.transform, false);
            LayoutElement sepLE = sep.AddComponent<LayoutElement>();
            sepLE.preferredHeight = 2;
            Image sepImg = sep.AddComponent<Image>();
            sepImg.color = new Color(0.85f, 0.65f, 0.13f, 0.3f);

            // Preview fields
            CreatePreviewField(card.transform, "PreviewNameText", "Name: --");
            CreatePreviewField(card.transform, "PreviewGameText", "Game: --");
            CreatePreviewField(card.transform, "PreviewEntryText", "Entry Fee: --");
            CreatePreviewField(card.transform, "PreviewPrizeText", "Est. Prize: --");
            CreatePreviewField(card.transform, "PreviewPlayersText", "Max Players: --");
        }

        private static void CreatePreviewField(Transform parent, string goName, string defaultText)
        {
            GameObject obj = new GameObject(goName);
            obj.transform.SetParent(parent, false);

            LayoutElement le = obj.AddComponent<LayoutElement>();
            le.preferredHeight = 26;

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = defaultText;
            tmp.fontSize = FontSizes.Body;
            tmp.color = TEXT_PRIMARY;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.raycastTarget = false;
        }

        #endregion

        #region ActionBar (0-1x, 0-0.11y)

        private static void CreateActionBar(Transform parent)
        {
            GameObject actionBar = new GameObject("ActionBar");
            actionBar.transform.SetParent(parent, false);

            RectTransform rt = actionBar.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = new Vector2(1, 0.11f);
            rt.sizeDelta = Vector2.zero;

            Image bg = actionBar.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.06f, 0.12f, 0.98f);

            // CreationFeeText
            GameObject feeObj = new GameObject("CreationFeeText");
            feeObj.transform.SetParent(actionBar.transform, false);

            RectTransform feeRT = feeObj.AddComponent<RectTransform>();
            feeRT.anchorMin = new Vector2(0, 0.6f);
            feeRT.anchorMax = new Vector2(1, 1);
            feeRT.sizeDelta = Vector2.zero;
            feeRT.offsetMin = new Vector2(20, 0);
            feeRT.offsetMax = new Vector2(-20, -5);

            TextMeshProUGUI feeTMP = feeObj.AddComponent<TextMeshProUGUI>();
            feeTMP.text = "Creation fee: $5.00";
            feeTMP.fontSize = FontSizes.Body;
            feeTMP.color = TEXT_SECONDARY;
            feeTMP.alignment = TextAlignmentOptions.Center;
            feeTMP.raycastTarget = false;

            // CreateButton
            GameObject createBtn = new GameObject("CreateButton");
            createBtn.transform.SetParent(actionBar.transform, false);

            RectTransform btnRT = createBtn.AddComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.05f, 0.08f);
            btnRT.anchorMax = new Vector2(0.95f, 0.58f);
            btnRT.sizeDelta = Vector2.zero;

            Image btnBg = createBtn.AddComponent<Image>();
            btnBg.color = BUTTON_GOLD;

            Button btn = createBtn.AddComponent<Button>();
            btn.targetGraphic = btnBg;
            ColorBlock colors = btn.colors;
            colors.normalColor = BUTTON_GOLD;
            colors.highlightedColor = GOLD_LIGHT;
            colors.pressedColor = GOLD_DARK;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            btn.colors = colors;

            Outline btnOutline = createBtn.AddComponent<Outline>();
            btnOutline.effectColor = new Color(1f, 0.75f, 0.2f, 0.6f);
            btnOutline.effectDistance = new Vector2(2, -2);

            GameObject btnTextObj = new GameObject("CashCreateButtonText");
            btnTextObj.transform.SetParent(createBtn.transform, false);

            RectTransform btnTextRT = btnTextObj.AddComponent<RectTransform>();
            btnTextRT.anchorMin = Vector2.zero;
            btnTextRT.anchorMax = Vector2.one;
            btnTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI btnTMP = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnTMP.text = "Create Tournament";
            btnTMP.fontSize = FontSizes.Body;
            btnTMP.color = BG_DARK;
            btnTMP.fontStyle = FontStyles.Bold;
            btnTMP.alignment = TextAlignmentOptions.Center;
        }

        #endregion

        #region Loading Overlay

        private static void CreateLoadingOverlay(Transform parent)
        {
            GameObject overlay = new GameObject("LoadingOverlay");
            overlay.transform.SetParent(parent, false);
            overlay.SetActive(false);

            RectTransform rt = overlay.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image bg = overlay.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.75f);

            // Status text
            GameObject statusObj = new GameObject("StatusText");
            statusObj.transform.SetParent(overlay.transform, false);

            RectTransform stRT = statusObj.AddComponent<RectTransform>();
            stRT.anchorMin = new Vector2(0.1f, 0.4f);
            stRT.anchorMax = new Vector2(0.9f, 0.6f);
            stRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI stTMP = statusObj.AddComponent<TextMeshProUGUI>();
            stTMP.text = "Creating tournament...";
            stTMP.fontSize = FontSizes.H4;
            stTMP.color = TEXT_GOLD;
            stTMP.fontStyle = FontStyles.Bold;
            stTMP.alignment = TextAlignmentOptions.Center;
        }

        #endregion

        #region Helpers

        private static void CreateSlider(Transform parent, string name)
        {
            GameObject sliderObj = new GameObject(name);
            sliderObj.transform.SetParent(parent, false);

            RectTransform sliderRT = sliderObj.AddComponent<RectTransform>();
            sliderRT.anchorMin = Vector2.zero;
            sliderRT.anchorMax = Vector2.one;
            sliderRT.sizeDelta = Vector2.zero;

            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 1;
            slider.maxValue = 100;
            slider.value = 5;

            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(sliderObj.transform, false);
            RectTransform bgRT = bg.AddComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0, 0.35f);
            bgRT.anchorMax = new Vector2(1, 0.65f);
            bgRT.sizeDelta = Vector2.zero;
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.18f, 0.25f, 1f);

            // Fill Area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform faRT = fillArea.AddComponent<RectTransform>();
            faRT.anchorMin = new Vector2(0, 0.35f);
            faRT.anchorMax = new Vector2(1, 0.65f);
            faRT.sizeDelta = Vector2.zero;

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = GOLD_PRIMARY;
            RectTransform fillRT = fill.GetComponent<RectTransform>();
            fillRT.sizeDelta = Vector2.zero;
            slider.fillRect = fillRT;

            // Handle Slide Area
            GameObject handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(sliderObj.transform, false);
            RectTransform haRT = handleArea.AddComponent<RectTransform>();
            haRT.anchorMin = Vector2.zero;
            haRT.anchorMax = Vector2.one;
            haRT.sizeDelta = Vector2.zero;

            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            Image handleImg = handle.AddComponent<Image>();
            handleImg.color = GOLD_LIGHT;
            RectTransform handleRT = handle.GetComponent<RectTransform>();
            handleRT.sizeDelta = new Vector2(24, 24);
            slider.handleRect = handleRT;
        }

        private static void CreateToggle(Transform parent, string name, bool defaultValue)
        {
            GameObject toggleObj = new GameObject(name);
            toggleObj.transform.SetParent(parent, false);

            LayoutElement le = toggleObj.AddComponent<LayoutElement>();
            le.preferredWidth = 50;
            le.preferredHeight = 30;
            le.flexibleWidth = 0;

            // Background
            Image bg = toggleObj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.18f, 0.25f, 1f);

            // Checkmark
            GameObject checkmark = new GameObject("Checkmark");
            checkmark.transform.SetParent(toggleObj.transform, false);

            RectTransform checkRT = checkmark.AddComponent<RectTransform>();
            checkRT.anchorMin = new Vector2(0.1f, 0.1f);
            checkRT.anchorMax = new Vector2(0.9f, 0.9f);
            checkRT.sizeDelta = Vector2.zero;

            Image checkImg = checkmark.AddComponent<Image>();
            checkImg.color = GOLD_PRIMARY;

            Toggle toggle = toggleObj.AddComponent<Toggle>();
            toggle.targetGraphic = bg;
            toggle.graphic = checkImg;
            toggle.isOn = defaultValue;
        }

        private static void CreateTMPDropdownInLayout(Transform parent, string name, List<string> options,
            float flexWidth = 0, float preferredHeight = 50)
        {
            GameObject ddObj = new GameObject(name);
            ddObj.transform.SetParent(parent, false);

            LayoutElement ddLE = ddObj.AddComponent<LayoutElement>();
            ddLE.preferredHeight = preferredHeight;
            if (flexWidth > 0) ddLE.flexibleWidth = flexWidth;

            Image ddBg = ddObj.AddComponent<Image>();
            ddBg.color = new Color(0.15f, 0.12f, 0.2f, 1f);

            Outline ddOutline = ddObj.AddComponent<Outline>();
            ddOutline.effectColor = CARD_BORDER;
            ddOutline.effectDistance = new Vector2(1, -1);

            TMP_Dropdown dd = ddObj.AddComponent<TMP_Dropdown>();

            // Caption label
            GameObject captionObj = new GameObject("Label");
            captionObj.transform.SetParent(ddObj.transform, false);

            RectTransform capRT = captionObj.AddComponent<RectTransform>();
            capRT.anchorMin = Vector2.zero;
            capRT.anchorMax = Vector2.one;
            capRT.offsetMin = new Vector2(10, 0);
            capRT.offsetMax = new Vector2(-30, 0);

            TextMeshProUGUI capTMP = captionObj.AddComponent<TextMeshProUGUI>();
            capTMP.text = options.Count > 0 ? options[0] : name;
            capTMP.fontSize = FontSizes.Body;
            capTMP.color = TEXT_PRIMARY;
            capTMP.alignment = TextAlignmentOptions.Left;

            dd.captionText = capTMP;

            // Arrow
            GameObject arrow = new GameObject("Arrow");
            arrow.transform.SetParent(ddObj.transform, false);
            RectTransform arrowRT = arrow.AddComponent<RectTransform>();
            arrowRT.anchorMin = new Vector2(1, 0.5f);
            arrowRT.anchorMax = new Vector2(1, 0.5f);
            arrowRT.pivot = new Vector2(1, 0.5f);
            arrowRT.sizeDelta = new Vector2(24, 24);
            arrowRT.anchoredPosition = new Vector2(-6, 0);

            TextMeshProUGUI arrowTMP = arrow.AddComponent<TextMeshProUGUI>();
            arrowTMP.text = "\u25BC";
            arrowTMP.fontSize = FontSizes.Body;
            arrowTMP.color = TEXT_GOLD;
            arrowTMP.alignment = TextAlignmentOptions.Center;

            // Template (hidden dropdown list)
            GameObject template = new GameObject("Template");
            template.transform.SetParent(ddObj.transform, false);
            template.SetActive(false);

            RectTransform tmplRT = template.AddComponent<RectTransform>();
            tmplRT.anchorMin = new Vector2(0, 0);
            tmplRT.anchorMax = new Vector2(1, 0);
            tmplRT.pivot = new Vector2(0.5f, 1);
            tmplRT.sizeDelta = new Vector2(0, 200);

            Image tmplBg = template.AddComponent<Image>();
            tmplBg.color = new Color(0.12f, 0.1f, 0.15f, 1f);

            ScrollRect tmplScroll = template.AddComponent<ScrollRect>();
            tmplScroll.horizontal = false;

            // Viewport
            GameObject tmplViewport = new GameObject("Viewport");
            tmplViewport.transform.SetParent(template.transform, false);
            RectTransform tvRT = tmplViewport.AddComponent<RectTransform>();
            tvRT.anchorMin = Vector2.zero;
            tvRT.anchorMax = Vector2.one;
            tvRT.sizeDelta = Vector2.zero;
            tmplViewport.AddComponent<Image>().color = new Color(0, 0, 0, 0);
            tmplViewport.AddComponent<RectMask2D>();
            tmplScroll.viewport = tvRT;

            // Content
            GameObject tmplContent = new GameObject("Content");
            tmplContent.transform.SetParent(tmplViewport.transform, false);
            RectTransform tcRT = tmplContent.AddComponent<RectTransform>();
            tcRT.anchorMin = new Vector2(0, 1);
            tcRT.anchorMax = new Vector2(1, 1);
            tcRT.pivot = new Vector2(0.5f, 1);
            tcRT.sizeDelta = new Vector2(0, 50);
            tmplScroll.content = tcRT;

            // Item template
            GameObject item = new GameObject("Item");
            item.transform.SetParent(tmplContent.transform, false);
            RectTransform itemRT = item.AddComponent<RectTransform>();
            itemRT.anchorMin = new Vector2(0, 0.5f);
            itemRT.anchorMax = new Vector2(1, 0.5f);
            itemRT.sizeDelta = new Vector2(0, 44);

            Toggle itemToggle = item.AddComponent<Toggle>();

            GameObject itemBg = new GameObject("Item Background");
            itemBg.transform.SetParent(item.transform, false);
            RectTransform ibRT = itemBg.AddComponent<RectTransform>();
            ibRT.anchorMin = Vector2.zero;
            ibRT.anchorMax = Vector2.one;
            ibRT.sizeDelta = Vector2.zero;
            Image ibImg = itemBg.AddComponent<Image>();
            ibImg.color = new Color(0.15f, 0.12f, 0.2f, 1f);

            GameObject itemCheck = new GameObject("Item Checkmark");
            itemCheck.transform.SetParent(item.transform, false);
            RectTransform icRT = itemCheck.AddComponent<RectTransform>();
            icRT.anchorMin = new Vector2(0, 0.5f);
            icRT.anchorMax = new Vector2(0, 0.5f);
            icRT.sizeDelta = new Vector2(20, 20);
            icRT.anchoredPosition = new Vector2(10, 0);
            Image icImg = itemCheck.AddComponent<Image>();
            icImg.color = GOLD_PRIMARY;

            GameObject itemLabel = new GameObject("Item Label");
            itemLabel.transform.SetParent(item.transform, false);
            RectTransform ilRT = itemLabel.AddComponent<RectTransform>();
            ilRT.anchorMin = Vector2.zero;
            ilRT.anchorMax = Vector2.one;
            ilRT.offsetMin = new Vector2(35, 0);
            ilRT.offsetMax = new Vector2(-10, 0);

            TextMeshProUGUI ilTMP = itemLabel.AddComponent<TextMeshProUGUI>();
            ilTMP.text = "Option";
            ilTMP.fontSize = FontSizes.Body;
            ilTMP.color = TEXT_PRIMARY;
            ilTMP.alignment = TextAlignmentOptions.Left;

            itemToggle.targetGraphic = ibImg;
            itemToggle.graphic = icImg;

            dd.template = tmplRT;
            dd.itemText = ilTMP;

            dd.ClearOptions();
            dd.AddOptions(options);
        }

        #endregion

        #region Reference Assigner

        private static MonoBehaviour FindCashTournamentCreateManager()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "CashTournamentCreateManager") return mb;
            return null;
        }

        private static void ResetAssignState()
        {
            assignedCount = 0; failedCount = 0; alreadySetCount = 0;
            assignResults.Clear();
        }

        public static void SetupManagerReferences()
        {
            var manager = FindCashTournamentCreateManager();
            if (manager == null)
            {
                Debug.LogError("[CashTournamentCreateUIBuilder] CashTournamentCreateManager not found!");
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            so.Update();

            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            Transform root = canvas != null ? canvas.transform : manager.transform.root;

            // Header
            AssignRef(so, "backButton", FindBtnDeep(root, "BackButton"));
            AssignRef(so, "titleText", FindTextDeep(root, "TitleText"));

            // Tournament Name
            AssignRef(so, "tournamentNameInput", FindInputDeep(root, "TournamentNameInput"));
            AssignRef(so, "nameCharCountText", FindTextDeep(root, "CharCountText"));

            // Game Selection
            AssignRef(so, "gameTypeDropdown", FindDropdownDeep(root, "GameTypeDropdown"));
            AssignRef(so, "selectedGameIcon", FindImageDeep(root, "SelectedGameIcon"));

            // Entry Fee
            AssignRef(so, "entryFeeDropdown", FindDropdownDeep(root, "EntryFeeDropdown"));
            AssignRef(so, "entryFeeSlider", FindSliderDeep(root, "EntryFeeSlider"));
            AssignRef(so, "customEntryFeeInput", FindInputDeep(root, "CustomEntryFeeInput"));
            AssignRef(so, "entryFeeDisplayText", FindTextDeep(root, "EntryFeeDisplayText"));

            // Players
            AssignRef(so, "maxPlayersDropdown", FindDropdownDeep(root, "MaxPlayersDropdown"));
            AssignRef(so, "estimatedPrizeText", FindTextDeep(root, "EstimatedPrizeText"));

            // Schedule
            AssignRef(so, "startTimeDropdown", FindDropdownDeep(root, "StartTimeDropdown"));
            AssignRef(so, "startImmediatelyToggle", FindToggleDeep(root, "StartImmediatelyToggle"));
            AssignRef(so, "scheduledTimeText", FindTextDeep(root, "ScheduledTimeText"));

            // Rules
            AssignRef(so, "roundsDropdown", FindDropdownDeep(root, "RoundsDropdown"));
            AssignRef(so, "timeLimitDropdown", FindDropdownDeep(root, "TimeLimitDropdown"));
            AssignRef(so, "maxAttemptsDropdown", FindDropdownDeep(root, "MaxAttemptsDropdown"));
            AssignRef(so, "allowSpectatorsToggle", FindToggleDeep(root, "AllowSpectatorsToggle"));
            AssignRef(so, "privateToggle", FindToggleDeep(root, "PrivateToggle"));
            AssignRef(so, "privateCodeInput", FindInputDeep(root, "PrivateCodeInput"));

            // Preview
            AssignGORef(so, "previewPanel", FindDeep(root, "PreviewPanel"));
            AssignRef(so, "previewNameText", FindTextDeep(root, "PreviewNameText"));
            AssignRef(so, "previewGameText", FindTextDeep(root, "PreviewGameText"));
            AssignRef(so, "previewEntryText", FindTextDeep(root, "PreviewEntryText"));
            AssignRef(so, "previewPrizeText", FindTextDeep(root, "PreviewPrizeText"));
            AssignRef(so, "previewPlayersText", FindTextDeep(root, "PreviewPlayersText"));

            // Actions
            AssignRef(so, "createButton", FindBtnDeep(root, "CreateButton"));
            AssignRef(so, "createButtonText", FindTextDeep(root, "CashCreateButtonText"));
            AssignRef(so, "creationFeeText", FindTextDeep(root, "CreationFeeText"));

            // Status
            AssignGORef(so, "loadingOverlay", FindDeep(root, "LoadingOverlay"));
            AssignRef(so, "statusText", FindTextDeep(root, "StatusText"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(manager.gameObject);
            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);

            Debug.Log($"[CashTournamentCreateUIBuilder] References: {assignedCount} assigned, {alreadySetCount} already set, {failedCount} failed");
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

        private static TextMeshProUGUI FindTextDeep(Transform root, string name)
        {
            Transform t = FindDeep(root, name);
            return t != null ? t.GetComponent<TextMeshProUGUI>() : null;
        }

        private static Button FindBtnDeep(Transform root, string name)
        {
            Transform t = FindDeep(root, name);
            return t != null ? t.GetComponent<Button>() : null;
        }

        private static TMP_Dropdown FindDropdownDeep(Transform root, string name)
        {
            Transform t = FindDeep(root, name);
            return t != null ? t.GetComponent<TMP_Dropdown>() : null;
        }

        private static Slider FindSliderDeep(Transform root, string name)
        {
            Transform t = FindDeep(root, name);
            return t != null ? t.GetComponent<Slider>() : null;
        }

        private static TMP_InputField FindInputDeep(Transform root, string name)
        {
            Transform t = FindDeep(root, name);
            return t != null ? t.GetComponent<TMP_InputField>() : null;
        }

        private static Toggle FindToggleDeep(Transform root, string name)
        {
            Transform t = FindDeep(root, name);
            return t != null ? t.GetComponent<Toggle>() : null;
        }

        private static Image FindImageDeep(Transform root, string name)
        {
            Transform t = FindDeep(root, name);
            return t != null ? t.GetComponent<Image>() : null;
        }

        private static void AssignRef(SerializedObject so, string prop, Object value)
        {
            var p = so.FindProperty(prop);
            if (p == null) { AddAR(prop, "Property not found", false, null); failedCount++; return; }
            if (p.objectReferenceValue != null) { AddAR(prop, "Already set", true, p.objectReferenceValue); alreadySetCount++; return; }
            if (value != null) { p.objectReferenceValue = value; AddAR(prop, "Assigned", true, value); assignedCount++; }
            else { AddAR(prop, "Not found", false, null); failedCount++; }
        }

        private static void AssignGORef(SerializedObject so, string prop, Transform t)
        {
            AssignRef(so, prop, t != null ? t.gameObject : null);
        }

        private static void AddAR(string f, string s, bool ok, Object o)
        {
            assignResults.Add(new AssignResult { fieldName = f, status = s, success = ok, assignedObject = o });
        }

        private void DrawAssignResults()
        {
            if (assignResults.Count == 0) return;

            EditorGUILayout.Space(10);
            int total = assignResults.Count;
            int successTotal = assignedCount + alreadySetCount;
            float rate = (float)successTotal / total;

            GUI.color = rate == 1f ? new Color(0.2f, 0.8f, 0.2f) :
                        rate >= 0.7f ? new Color(1f, 0.8f, 0.2f) : new Color(1f, 0.4f, 0.4f);
            GUILayout.Label(rate == 1f ? "ALL REFERENCES ASSIGNED" : "Some references missing", EditorStyles.boldLabel);
            GUI.color = Color.white;

            GUILayout.Label($"Assigned: {assignedCount} | Already set: {alreadySetCount} | Failed: {failedCount}");
            EditorGUILayout.Space(5);

            foreach (var r in assignResults)
            {
                EditorGUILayout.BeginHorizontal();
                GUI.color = r.success ? (r.status == "Already set" ? new Color(0.5f, 0.8f, 1f) : Color.green) : Color.red;
                GUILayout.Label(r.success ? (r.status == "Already set" ? "o" : "+") : "x", GUILayout.Width(16));
                GUI.color = Color.white;
                GUILayout.Label(r.fieldName, GUILayout.Width(200));
                GUILayout.Label(r.status, GUILayout.Width(110));
                if (r.assignedObject != null)
                    EditorGUILayout.ObjectField(r.assignedObject, typeof(Object), true, GUILayout.Width(140));
                EditorGUILayout.EndHorizontal();
            }
        }

        #endregion
    }
}
