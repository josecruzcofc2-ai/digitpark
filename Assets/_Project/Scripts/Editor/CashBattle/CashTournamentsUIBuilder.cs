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
    /// UI Builder for CashTournaments scene.
    /// Gold-themed tournament browser with filter tabs, tournament cards, and FAB create button.
    /// Includes integrated Reference Assigner for CashTournamentsManager.
    /// </summary>
    public class CashTournamentsUIBuilder : EditorWindow
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
        private const string BACK_ICON_GOLD_PATH = "Assets/_Project/Art/Icons/Navigation/BackIconGold.png";

        #endregion

        [MenuItem("DigitPark/Scenes/Build Scene/CashBattle/Tournaments", false, 184)]
        public static void ShowWindow()
        {
            GetWindow<CashTournamentsUIBuilder>("Cash Tournaments Builder");
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // ========== SECCION 1: UI BUILDER ==========
            GUILayout.Label("Cash Tournaments UI Builder", EditorStyles.boldLabel);
            GUILayout.Label("Gold-themed Tournament Browser", EditorStyles.miniLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Builds the Gold-themed UI for CashTournaments:\n\n" +
                "- Header with BackButtonGold, title\n" +
                "- FilterBar with All/Active/Completed tabs\n" +
                "- TournamentsList ScrollRect (2 preview cards)\n" +
                "- Create Tournament button (fixed bottom bar)\n" +
                "- EmptyState (text only, hidden)",
                MessageType.Info);

            EditorGUILayout.Space(10);

            GUI.backgroundColor = GOLD_PRIMARY;
            if (GUILayout.Button("BUILD GOLD UI", GUILayout.Height(40)))
            {
                BuildCashTournamentsUI();
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
            if (currentScene != "CashTournaments")
            {
                EditorGUILayout.HelpBox($"Current scene: {currentScene}\nOpen CashTournaments first.", MessageType.Warning);
            }

            MonoBehaviour targetManager = FindCashTournamentsManager();
            if (targetManager != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Manager:", GUILayout.Width(60));
                EditorGUILayout.ObjectField(targetManager, typeof(MonoBehaviour), true);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("CashTournamentsManager not found in scene.", MessageType.Warning);
            }

            EditorGUILayout.Space(5);

            GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
            if (GUILayout.Button("ASSIGN ALL REFERENCES", GUILayout.Height(36)))
            {
                ResetAssignState();
                RunAssignAllReferences();
                Repaint();
            }
            GUI.backgroundColor = Color.white;

            DrawAssignResults();

            EditorGUILayout.EndScrollView();
        }

        #region Main Build Methods

        private static void BuildCashTournamentsUI()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontro Canvas. Abre la escena CashTournaments primero.", "OK");
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

            if (EditorUtility.DisplayDialog("Rebuild Gold UI?",
                "This will completely rebuild the Cash Tournaments UI with Gold theme.\n\nContinue?",
                "Yes, Build", "Cancel"))
            {
                CleanupOldElements(canvas.transform);
                BuildAllElements(canvas);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                Debug.Log("[CashTournamentsUIBuilder] Gold UI built successfully!");
            }
        }

        /// <summary>
        /// Builds the UI silently without confirmation dialogs. Used by batch builders.
        /// </summary>
        public static void BuildSilent()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("[CashTournamentsUIBuilder] Canvas not found - cannot build silently");
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

            CleanupOldElements(canvas.transform);
            BuildAllElements(canvas);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("[CashTournamentsUIBuilder] UI built silently (batch mode)");
        }

        private static void CleanScene()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            // Standardize CanvasScaler
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
            }

            CleanupOldElements(canvas.transform);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[CashTournamentsUIBuilder] Scene cleaned.");
        }

        private static void CleanupOldUI()
        {
            string[] toClean = { "Background", "SafeArea", "BackButton", "BackButtonGold" };
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

            // Safe Area Container
            GameObject safeArea = CreateSafeArea(canvasTransform);

            // Header
            CreateHeader(safeArea.transform);

            // FilterBar
            CreateFilterBar(safeArea.transform);

            // TournamentsList
            CreateTournamentsList(safeArea.transform);

            // Create Tournament Button (fixed at bottom)
            CreateCreateTournamentButton(safeArea.transform);

            // EmptyState (hidden)
            CreateEmptyState(safeArea.transform);

            // Premium Block Panel (hidden by default)
            CreatePremiumBlockPanel(safeArea.transform);

            Debug.Log("[CashTournamentsUIBuilder] All elements created.");
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

            Debug.Log($"[CashTournamentsUIBuilder] Cleaned {toDestroy.Count} old objects from Canvas");
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

        #region Header (0-1x, 0.92-1y)

        private static void CreateHeader(Transform parent)
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(parent, false);

            RectTransform rt = header.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.92f);
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image bg = header.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.06f, 0.12f, 0.95f);

            // Back Button
            CreateBackButton(header.transform);

            // Title
            CreateHeaderTitle(header.transform);
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
                // Assign BackIconGold sprite to Icon child
                Sprite backIcon = AssetDatabase.LoadAssetAtPath<Sprite>(BACK_ICON_GOLD_PATH);
                if (backIcon != null)
                {
                    Transform iconChild = backBtn.transform.Find("Icon");
                    if (iconChild != null)
                    {
                        Image iconImg = iconChild.GetComponent<Image>();
                        if (iconImg != null) iconImg.sprite = backIcon;
                    }
                }
            }
            else
            {
                Debug.LogWarning("[CashTournaments] BackButtonGold prefab not found, using fallback");

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

        private static void CreateHeaderTitle(Transform parent)
        {
            GameObject titleObj = new GameObject("CashTournamentsTitle");
            titleObj.transform.SetParent(parent, false);

            RectTransform rt = titleObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.07f, 0f);
            rt.anchorMax = new Vector2(0.53f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;

            TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
            title.text = "CASH TOURNAMENTS";
            title.fontSize = FontSizes.H4;
            title.color = TEXT_GOLD;
            title.alignment = TextAlignmentOptions.MidlineLeft;
            title.raycastTarget = false;
            title.fontStyle = FontStyles.Bold;
            title.enableAutoSizing = true;
            title.fontSizeMin = FontSizes.AutoMinTitle;
            title.fontSizeMax = FontSizes.H4;
            title.enableWordWrapping = false;
        }

        #endregion

        #region FilterBar (0.02-0.98x, 0.86-0.91y)

        private static void CreateFilterBar(Transform parent)
        {
            GameObject filterBar = new GameObject("FilterBar");
            filterBar.transform.SetParent(parent, false);

            RectTransform rt = filterBar.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.02f, 0.86f);
            rt.anchorMax = new Vector2(0.98f, 0.91f);
            rt.sizeDelta = Vector2.zero;

            Image bg = filterBar.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.3f);

            HorizontalLayoutGroup hlg = filterBar.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6;
            hlg.padding = new RectOffset(8, 8, 4, 4);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            CreateFilterTab(filterBar.transform, "AllFilter", "All", true);
            CreateFilterTab(filterBar.transform, "ActiveFilter", "Active", false);
            CreateFilterTab(filterBar.transform, "CompletedFilter", "Completed", false);
        }

        private static void CreateFilterTab(Transform parent, string goName, string label, bool isActive)
        {
            GameObject tab = new GameObject(goName);
            tab.transform.SetParent(parent, false);

            RectTransform rt = tab.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 50);

            LayoutElement le = tab.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;
            le.minWidth = 80;

            Image bg = tab.AddComponent<Image>();
            bg.color = isActive ? new Color(0.15f, 0.12f, 0.2f, 1f) : new Color(0.1f, 0.08f, 0.14f, 0.8f);

            Button btn = tab.AddComponent<Button>();
            btn.targetGraphic = bg;

            // Tab text
            GameObject textObj = new GameObject(goName + "Text");
            textObj.transform.SetParent(tab.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = FontSizes.Body;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = isActive ? GOLD_PRIMARY : TEXT_SECONDARY;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinSmall;
            tmp.fontSizeMax = FontSizes.Body;

            // Gold accent underline on selected tab
            if (isActive)
            {
                GameObject underline = new GameObject("GoldAccent");
                underline.transform.SetParent(tab.transform, false);

                RectTransform ulRT = underline.AddComponent<RectTransform>();
                ulRT.anchorMin = new Vector2(0.1f, 0);
                ulRT.anchorMax = new Vector2(0.9f, 0);
                ulRT.pivot = new Vector2(0.5f, 0);
                ulRT.sizeDelta = new Vector2(0, 3);
                ulRT.anchoredPosition = new Vector2(0, 2);

                Image ulImg = underline.AddComponent<Image>();
                ulImg.color = GOLD_PRIMARY;
                ulImg.raycastTarget = false;
            }
        }

        #endregion

        #region TournamentsList (0.02-0.98x, 0.10-0.85y)

        private static void CreateTournamentsList(Transform parent)
        {
            GameObject scrollView = new GameObject("TournamentsList");
            scrollView.transform.SetParent(parent, false);

            RectTransform svRT = scrollView.AddComponent<RectTransform>();
            svRT.anchorMin = new Vector2(0.02f, 0.10f);
            svRT.anchorMax = new Vector2(0.98f, 0.85f);
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
            vlg.spacing = 20;
            vlg.padding = new RectOffset(8, 8, 10, 40);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = vpRT;
            scroll.content = contentRT;

            // === Editor Preview: 2 sample TournamentCardUI instances ===
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/CashBattle/Tournaments/TournamentCardUI.prefab");
            if (prefab != null)
            {
                // Card 1
                GameObject card1 = (GameObject)PrefabUtility.InstantiatePrefab(prefab, content.transform);
                SetCardPreviewData(card1, "QuickMath Championship", "$100", "12/16", "02:00:00", "$5");

                // Card 2
                GameObject card2 = (GameObject)PrefabUtility.InstantiatePrefab(prefab, content.transform);
                SetCardPreviewData(card2, "Flash Tap Masters", "$250", "28/32", "01:00:00", "$10");
            }
            else
            {
                Debug.LogWarning("[CashTournamentsUIBuilder] TournamentCardUI.prefab not found — run CashBattle Prefab Builder first");
            }
        }

        private static void SetCardPreviewData(GameObject card, string name, string prize, string players, string timer, string entry)
        {
            var nameText = card.transform.Find("TournamentName")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null) nameText.text = name;
            // Flat structure: InfoRow/PrizeText, InfoRow/PlayersText, InfoRow/TimerText
            var prizeText = card.transform.Find("InfoRow/PrizeText")?.GetComponent<TextMeshProUGUI>();
            if (prizeText != null) prizeText.text = prize;
            var playersText = card.transform.Find("InfoRow/PlayersText")?.GetComponent<TextMeshProUGUI>();
            if (playersText != null) playersText.text = players;
            var timerText = card.transform.Find("InfoRow/TimerText")?.GetComponent<TextMeshProUGUI>();
            if (timerText != null) timerText.text = timer;
            var entryValue = card.transform.Find("EntryFeeBadge/Value")?.GetComponent<TextMeshProUGUI>();
            if (entryValue != null) entryValue.text = entry;
        }

        #endregion

        #region Create Tournament Button (fixed bottom, 0.04-0.96x, 0.02-0.08y)

        private static void CreateCreateTournamentButton(Transform parent)
        {
            GameObject createBtn = new GameObject("CreateButton");
            createBtn.transform.SetParent(parent, false);

            RectTransform btnRT = createBtn.AddComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.04f, 0.02f);
            btnRT.anchorMax = new Vector2(0.96f, 0.08f);
            btnRT.sizeDelta = Vector2.zero;

            Image btnBg = createBtn.AddComponent<Image>();
            btnBg.color = GOLD_DARK;

            Button btn = createBtn.AddComponent<Button>();
            btn.targetGraphic = btnBg;
            ColorBlock colors = btn.colors;
            colors.normalColor = GOLD_DARK;
            colors.highlightedColor = GOLD_PRIMARY;
            colors.pressedColor = new Color(0.65f, 0.45f, 0.08f, 1f);
            btn.colors = colors;

            Outline outline = createBtn.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.75f, 0.2f, 0.4f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            // Text: "Create Tournament"
            GameObject textObj = new GameObject("CreateButtonText");
            textObj.transform.SetParent(createBtn.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI textTMP = textObj.AddComponent<TextMeshProUGUI>();
            textTMP.text = "Create Tournament";
            textTMP.fontSize = FontSizes.Body;
            textTMP.color = BG_DARK;
            textTMP.fontStyle = FontStyles.Bold;
            textTMP.alignment = TextAlignmentOptions.Center;
            textTMP.enableAutoSizing = true;
            textTMP.fontSizeMin = FontSizes.AutoMinBody;
            textTMP.fontSizeMax = FontSizes.Body;
        }

        #endregion

        #region Premium Block Panel (hidden)

        private static void CreatePremiumBlockPanel(Transform parent)
        {
            GameObject panel = new GameObject("PremiumBlockPanel");
            panel.transform.SetParent(parent, false);
            panel.SetActive(false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.03f, 0.22f);
            rt.anchorMax = new Vector2(0.97f, 0.78f);
            rt.sizeDelta = Vector2.zero;

            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.06f, 0.12f, 0.98f);

            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = GOLD_DARK;
            outline.effectDistance = new Vector2(2, -2);

            // Title
            GameObject titleObj = new GameObject("PremiumTitle");
            titleObj.transform.SetParent(panel.transform, false);
            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.05f, 0.7f);
            titleRT.anchorMax = new Vector2(0.95f, 0.9f);
            titleRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "Premium Required";
            titleTMP.fontSize = FontSizes.H3;
            titleTMP.color = GOLD_PRIMARY;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.enableAutoSizing = true;
            titleTMP.fontSizeMin = FontSizes.AutoMinTitle;
            titleTMP.fontSizeMax = FontSizes.H3;

            // Message
            GameObject msgObj = new GameObject("PremiumMessage");
            msgObj.transform.SetParent(panel.transform, false);
            RectTransform msgRT = msgObj.AddComponent<RectTransform>();
            msgRT.anchorMin = new Vector2(0.06f, 0.38f);
            msgRT.anchorMax = new Vector2(0.94f, 0.68f);
            msgRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI msgTMP = msgObj.AddComponent<TextMeshProUGUI>();
            msgTMP.text = "Creating tournaments requires a Premium subscription. Join existing tournaments for free!";
            msgTMP.fontSize = FontSizes.Body;
            msgTMP.color = TEXT_SECONDARY;
            msgTMP.alignment = TextAlignmentOptions.Center;
            msgTMP.enableAutoSizing = true;
            msgTMP.fontSizeMin = FontSizes.AutoMinBody;
            msgTMP.fontSizeMax = FontSizes.Body;

            // Get Premium Button
            GameObject getPremiumBtn = new GameObject("GetPremiumButton");
            getPremiumBtn.transform.SetParent(panel.transform, false);
            RectTransform gpRT = getPremiumBtn.AddComponent<RectTransform>();
            gpRT.anchorMin = new Vector2(0.15f, 0.18f);
            gpRT.anchorMax = new Vector2(0.55f, 0.35f);
            gpRT.sizeDelta = Vector2.zero;

            Image gpBg = getPremiumBtn.AddComponent<Image>();
            gpBg.color = GOLD_PRIMARY;
            Button gpBtn = getPremiumBtn.AddComponent<Button>();
            gpBtn.targetGraphic = gpBg;

            GameObject gpText = new GameObject("GetPremiumButtonText");
            gpText.transform.SetParent(getPremiumBtn.transform, false);
            RectTransform gpTextRT = gpText.AddComponent<RectTransform>();
            gpTextRT.anchorMin = Vector2.zero;
            gpTextRT.anchorMax = Vector2.one;
            gpTextRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI gpTMP = gpText.AddComponent<TextMeshProUGUI>();
            gpTMP.text = "Get Premium";
            gpTMP.fontSize = FontSizes.Body;
            gpTMP.color = BG_DARK;
            gpTMP.fontStyle = FontStyles.Bold;
            gpTMP.alignment = TextAlignmentOptions.Center;
            gpTMP.enableAutoSizing = true;
            gpTMP.fontSizeMin = FontSizes.AutoMinSmall;
            gpTMP.fontSizeMax = FontSizes.Body;

            // Close / Maybe Later Button
            GameObject closeBtn = new GameObject("ClosePremiumButton");
            closeBtn.transform.SetParent(panel.transform, false);
            RectTransform clRT = closeBtn.AddComponent<RectTransform>();
            clRT.anchorMin = new Vector2(0.58f, 0.18f);
            clRT.anchorMax = new Vector2(0.85f, 0.35f);
            clRT.sizeDelta = Vector2.zero;

            Image clBg = closeBtn.AddComponent<Image>();
            clBg.color = Color.clear;
            Button clButton = closeBtn.AddComponent<Button>();
            clButton.targetGraphic = clBg;

            Outline clOutline = closeBtn.AddComponent<Outline>();
            clOutline.effectColor = TEXT_SECONDARY;
            clOutline.effectDistance = new Vector2(1.5f, -1.5f);

            GameObject clText = new GameObject("ClosePremiumButtonText");
            clText.transform.SetParent(closeBtn.transform, false);
            RectTransform clTextRT = clText.AddComponent<RectTransform>();
            clTextRT.anchorMin = Vector2.zero;
            clTextRT.anchorMax = Vector2.one;
            clTextRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI clTMP = clText.AddComponent<TextMeshProUGUI>();
            clTMP.text = "Maybe Later";
            clTMP.fontSize = FontSizes.Body;
            clTMP.color = TEXT_SECONDARY;
            clTMP.fontStyle = FontStyles.Bold;
            clTMP.alignment = TextAlignmentOptions.Center;
            clTMP.enableAutoSizing = true;
            clTMP.fontSizeMin = FontSizes.AutoMinSmall;
            clTMP.fontSizeMax = FontSizes.Body;
        }

        #endregion

        #region EmptyState (hidden)

        private static void CreateEmptyState(Transform parent)
        {
            GameObject emptyState = new GameObject("EmptyState");
            emptyState.transform.SetParent(parent, false);
            emptyState.SetActive(false);

            RectTransform rt = emptyState.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.1f, 0.35f);
            rt.anchorMax = new Vector2(0.9f, 0.65f);
            rt.sizeDelta = Vector2.zero;

            VerticalLayoutGroup vlg = emptyState.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 20;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            // Empty text
            GameObject textObj = new GameObject("CashTournamentsEmptyText");
            textObj.transform.SetParent(emptyState.transform, false);

            LayoutElement textLE = textObj.AddComponent<LayoutElement>();
            textLE.preferredHeight = 60;

            TextMeshProUGUI emptyTMP = textObj.AddComponent<TextMeshProUGUI>();
            emptyTMP.text = "No tournaments available";
            emptyTMP.fontSize = FontSizes.Body;
            emptyTMP.color = TEXT_SECONDARY;
            emptyTMP.fontStyle = FontStyles.Bold;
            emptyTMP.alignment = TextAlignmentOptions.Center;
            emptyTMP.enableAutoSizing = true;
            emptyTMP.fontSizeMin = FontSizes.AutoMinSmall;
            emptyTMP.fontSizeMax = FontSizes.Body;
        }

        #endregion

        #region Reference Assigner

        private static MonoBehaviour FindCashTournamentsManager()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "CashTournamentsManager") return mb;
            return null;
        }

        private static void ResetAssignState()
        {
            assignedCount = 0; failedCount = 0; alreadySetCount = 0;
            assignResults.Clear();
        }

        private static void RunAssignAllReferences()
        {
            var manager = FindCashTournamentsManager();
            if (manager == null)
            {
                Debug.LogError("[CashTournamentsUIBuilder] CashTournamentsManager not found!");
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            so.Update();

            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            Transform root = canvas != null ? canvas.transform : manager.transform.root;

            // Header
            AssignRef(so, "backButton", FindBtnDeep(root, "BackButton"));
            AssignRef(so, "titleText", FindTextDeep(root, "TitleText"));

            // Filters
            AssignRef(so, "allFilterButton", FindBtnDeep(root, "AllFilter"));
            AssignRef(so, "activeFilterButton", FindBtnDeep(root, "ActiveFilter"));
            AssignRef(so, "completedFilterButton", FindBtnDeep(root, "CompletedFilter"));

            // Tournaments List
            AssignRef(so, "tournamentsContainer", FindDeep(root, "Content"));
            AssignGORef(so, "tournamentsList", FindDeep(root, "TournamentsList"));

            // Actions
            AssignRef(so, "createButton", FindBtnDeep(root, "CreateButton"));

            // Empty State
            AssignGORef(so, "emptyState", FindDeep(root, "EmptyState"));
            AssignRef(so, "emptyText", FindTextDeep(root, "CashTournamentsEmptyText"));

            // Premium Block Panel
            AssignGORef(so, "premiumBlockPanel", FindDeep(root, "PremiumBlockPanel"));
            AssignRef(so, "getPremiumButton", FindBtnDeep(root, "GetPremiumButton"));
            AssignRef(so, "closePremiumButton", FindBtnDeep(root, "ClosePremiumButton"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(manager.gameObject);
            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);

            Debug.Log($"[CashTournamentsUIBuilder] References: {assignedCount} assigned, {alreadySetCount} already set, {failedCount} failed");
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
                GUILayout.Label(r.fieldName, GUILayout.Width(180));
                GUILayout.Label(r.status, GUILayout.Width(110));
                if (r.assignedObject != null)
                    EditorGUILayout.ObjectField(r.assignedObject, typeof(Object), true, GUILayout.Width(140));
                EditorGUILayout.EndHorizontal();
            }
        }

        #endregion
    }
}
