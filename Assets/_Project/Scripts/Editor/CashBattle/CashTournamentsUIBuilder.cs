using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;

namespace DigitPark.Editor
{
    /// <summary>
    /// UI Builder PREMIUM para la escena CashTournaments.unity
    /// Construye la lista de torneos con iconos premium y diseño profesional.
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

        #region Colors - Premium Theme

        private static readonly Color BG_DARK = new Color(0.06f, 0.07f, 0.1f, 1f);
        private static readonly Color CARD_BG = new Color(0.1f, 0.12f, 0.16f, 1f);
        private static readonly Color HEADER_BG = new Color(0.08f, 0.09f, 0.12f, 0.95f);

        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GOLD_DARK = new Color(0.85f, 0.65f, 0.13f, 1f);
        private static readonly Color CYAN = new Color(0f, 0.9f, 1f, 1f);
        private static readonly Color GREEN = new Color(0.2f, 0.95f, 0.4f, 1f);

        private static readonly Color TEXT_WHITE = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.5f, 0.5f, 0.55f, 1f);

        #endregion

        #region Paths

        private const string PREFAB_PATH = "Assets/_Project/Prefabs/CashBattle/TournamentCardUI.prefab";
        private const string TOURNAMENT_ICONS_PATH = "Assets/_Project/Art/Icons/CashBattle/Tournaments/";
        private const string NAVIGATION_ICONS_PATH = "Assets/_Project/Art/Icons/Navigation/Buttons/";
        private const string BACK_BUTTON_GOLD_PREFAB = "Assets/_Project/Prefabs/Common/BackButtonGold.prefab";

        #endregion

        [MenuItem("DigitPark/UI Builders/CashBattle/Cash Tournaments (Premium)", false, 252)]
        public static void ShowWindow()
        {
            GetWindow<CashTournamentsUIBuilder>("Cash Tournaments Builder");
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // ========== SECCION 1: UI BUILDER ==========
            GUILayout.Label("Cash Tournaments UI Builder", EditorStyles.boldLabel);
            GUILayout.Label("Torneos Premium con Iconos Profesionales", EditorStyles.miniLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Construye la UI PREMIUM para CashTournaments.unity:\n\n" +
                "- Header con filtros y botón crear torneo\n" +
                "- Cards premium con iconos:\n" +
                "  * Icono de juego\n" +
                "  * Premio (TrophyPrizeIcon)\n" +
                "  * Jugadores (PlayersCountIcon)\n" +
                "  * Timer (TournamentTimerIcon)\n" +
                "  * Badge LIVE (TournamentLiveIcon)\n" +
                "- Usa prefab TournamentCardUI.prefab",
                MessageType.Info);

            EditorGUILayout.Space(10);

            GUI.backgroundColor = GOLD;
            if (GUILayout.Button("CONSTRUIR UI PREMIUM", GUILayout.Height(40)))
            {
                BuildCashTournamentsUI();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Solo Regenerar Prefab", GUILayout.Height(28)))
            {
                CashBattlePrefabBuilder.CreateTournamentCardPrefab();
            }

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Limpiar Escena", GUILayout.Height(25)))
            {
                CleanScene();
            }

            // ========== SEPARADOR ==========
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // ========== SECCION 2: REFERENCE ASSIGNER ==========
            GUILayout.Label("Asignar Referencias", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "CashTournaments")
            {
                EditorGUILayout.HelpBox($"Escena actual: {currentScene}\nAbre CashTournaments primero.", MessageType.Warning);
            }

            MonoBehaviour targetController = FindTournamentController();
            if (targetController != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Controller:", GUILayout.Width(70));
                EditorGUILayout.ObjectField(targetController, typeof(MonoBehaviour), true);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("TournamentListPanel no encontrado.", MessageType.Warning);
            }

            EditorGUILayout.Space(5);

            GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
            if (GUILayout.Button("ASIGNAR TODAS LAS REFERENCIAS", GUILayout.Height(36)))
            {
                ResetAssignState();
                RunAssignAllReferences();
                Repaint();
            }
            GUI.backgroundColor = Color.white;

            DrawAssignResults();

            EditorGUILayout.EndScrollView();
        }

        private static void BuildCashTournamentsUI()
        {
            Canvas canvas = FindMainCanvas();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontró Canvas. Abre la escena CashTournaments primero.", "OK");
                return;
            }

            if (EditorUtility.DisplayDialog("Reconstruir UI Premium?",
                "Esto reconstruirá completamente la UI de Cash Tournaments con diseño premium.\n\n¿Continuar?",
                "Sí, Construir", "Cancelar"))
            {
                BuildAllElements(canvas);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                Debug.Log("[CashTournamentsUIBuilder] UI PREMIUM construida exitosamente!");
            }
        }

        private static void CleanScene()
        {
            Canvas canvas = FindMainCanvas();
            if (canvas == null) return;

            CleanupOldElements(canvas.transform);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[CashTournamentsUIBuilder] Escena limpiada.");
        }

        private static void BuildAllElements(Canvas canvas)
        {
            Transform canvasTransform = canvas.transform;

            CleanupOldElements(canvasTransform);

            // Background
            CreateBackground(canvasTransform);

            // Safe Area Container
            GameObject safeArea = CreateSafeArea(canvasTransform);

            // Header Premium
            CreatePremiumHeader(safeArea.transform);

            // Filter Bar
            CreateFilterBar(safeArea.transform);

            // Tournaments List
            CreateTournamentsList(safeArea.transform);

            // Missing elements expected by TournamentListPanel controller
            CreateMissingElements(safeArea.transform);
        }

        private static void CleanupOldElements(Transform parent)
        {
            string[] toDestroy = { "Background", "SafeArea", "Header", "TournamentsList", "FilterBar",
                "NoTournamentsText", "LoadingIndicator", "RefreshButton",
                "GameFilterDropdown", "FeeFilterDropdown", "CreateTournamentPanel" };
            foreach (string name in toDestroy)
            {
                Transform existing = parent.Find(name);
                if (existing != null) DestroyImmediate(existing.gameObject);
            }
        }

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
        }

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

        private static void CreatePremiumHeader(Transform parent)
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(parent, false);

            RectTransform rt = header.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(0, 100);

            Image bg = header.AddComponent<Image>();
            bg.color = HEADER_BG;

            // Back Button
            CreateBackButton(header.transform);

            // Title con icono
            CreateHeaderTitle(header.transform);

            // Balance Display
            CreateBalanceDisplay(header.transform);

            // Create Tournament Button
            CreateNewTournamentButton(header.transform);
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
                rect.anchoredPosition = new Vector2(15, 0);
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
                rt.anchoredPosition = new Vector2(15, 0);

                Image img = backBtn.AddComponent<Image>();
                img.color = new Color(1, 1, 1, 0.1f);

                Button btn = backBtn.AddComponent<Button>();
                btn.targetGraphic = img;

                // Arrow icon
                Sprite arrowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(NAVIGATION_ICONS_PATH + "arrowWhite.png");

                GameObject arrow = new GameObject("Icon");
                arrow.transform.SetParent(backBtn.transform, false);

                RectTransform arrowRT = arrow.AddComponent<RectTransform>();
                arrowRT.anchorMin = Vector2.zero;
                arrowRT.anchorMax = Vector2.one;
                arrowRT.offsetMin = new Vector2(10, 10);
                arrowRT.offsetMax = new Vector2(-10, -10);

                if (arrowSprite != null)
                {
                    Image arrowImg = arrow.AddComponent<Image>();
                    arrowImg.sprite = arrowSprite;
                    arrowImg.preserveAspect = true;
                    arrowImg.color = TEXT_WHITE;
                }
                else
                {
                    TextMeshProUGUI arrowText = arrow.AddComponent<TextMeshProUGUI>();
                    arrowText.text = "<";
                    arrowText.fontSize = 32;
                    arrowText.color = TEXT_WHITE;
                    arrowText.alignment = TextAlignmentOptions.Center;
                }
            }
        }

        private static void CreateHeaderTitle(Transform parent)
        {
            GameObject titleContainer = new GameObject("TitleContainer");
            titleContainer.transform.SetParent(parent, false);

            RectTransform tcRT = titleContainer.AddComponent<RectTransform>();
            tcRT.anchorMin = new Vector2(0, 0.5f);
            tcRT.anchorMax = new Vector2(0.6f, 0.5f);
            tcRT.pivot = new Vector2(0, 0.5f);
            tcRT.sizeDelta = new Vector2(0, 80);
            tcRT.anchoredPosition = new Vector2(75, 0);

            // Title Text (centered, no icon)
            GameObject title = new GameObject("TitleText");
            title.transform.SetParent(titleContainer.transform, false);

            RectTransform titleRT = title.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.offsetMin = Vector2.zero;
            titleRT.offsetMax = Vector2.zero;

            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "Torneos";
            titleText.fontSize = 78;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = GOLD;
            titleText.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateBalanceDisplay(Transform parent)
        {
            GameObject balance = new GameObject("BalanceDisplay");
            balance.transform.SetParent(parent, false);

            RectTransform rt = balance.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(1, 0.5f);
            rt.sizeDelta = new Vector2(180, 65);
            rt.anchoredPosition = new Vector2(-15, 0);

            Image bg = balance.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.06f, 0.08f, 0.9f);

            // CoinIcon showing "$"
            GameObject coinIcon = new GameObject("CoinIcon");
            coinIcon.transform.SetParent(balance.transform, false);

            RectTransform coinRT = coinIcon.AddComponent<RectTransform>();
            coinRT.anchorMin = new Vector2(0, 0.5f);
            coinRT.anchorMax = new Vector2(0, 0.5f);
            coinRT.pivot = new Vector2(0, 0.5f);
            coinRT.sizeDelta = new Vector2(50, 50);
            coinRT.anchoredPosition = new Vector2(8, 0);

            TextMeshProUGUI coinText = coinIcon.AddComponent<TextMeshProUGUI>();
            coinText.text = "$";
            coinText.fontSize = 52;
            coinText.fontStyle = FontStyles.Bold;
            coinText.color = GREEN;
            coinText.alignment = TextAlignmentOptions.Center;

            // BalanceText
            GameObject balanceText = new GameObject("BalanceText");
            balanceText.transform.SetParent(balance.transform, false);

            RectTransform btRT = balanceText.AddComponent<RectTransform>();
            btRT.anchorMin = new Vector2(0, 0);
            btRT.anchorMax = new Vector2(1, 1);
            btRT.offsetMin = new Vector2(55, 0);
            btRT.offsetMax = new Vector2(-5, 0);

            TextMeshProUGUI btTMP = balanceText.AddComponent<TextMeshProUGUI>();
            btTMP.text = "125.50";
            btTMP.fontSize = 52;
            btTMP.fontStyle = FontStyles.Bold;
            btTMP.color = GREEN;
            btTMP.alignment = TextAlignmentOptions.Left;
            btTMP.enableAutoSizing = true;
            btTMP.fontSizeMin = 28;
            btTMP.fontSizeMax = 52;
        }

        private static void CreateNewTournamentButton(Transform parent)
        {
            GameObject createBtn = new GameObject("CreateTournamentBtn");
            createBtn.transform.SetParent(parent, false);

            RectTransform rt = createBtn.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(1, 0.5f);
            rt.sizeDelta = new Vector2(260, 60);
            rt.anchoredPosition = new Vector2(-200, 0);

            Image bg = createBtn.AddComponent<Image>();
            bg.color = GOLD_DARK;

            Button btn = createBtn.AddComponent<Button>();
            btn.targetGraphic = bg;

            // Text label (no icon)
            GameObject label = new GameObject("Label");
            label.transform.SetParent(createBtn.transform, false);

            RectTransform labelRT = label.AddComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI labelTMP = label.AddComponent<TextMeshProUGUI>();
            labelTMP.text = "Crear Torneo";
            labelTMP.fontSize = 52;
            labelTMP.fontStyle = FontStyles.Bold;
            labelTMP.color = Color.white;
            labelTMP.alignment = TextAlignmentOptions.Center;
            labelTMP.enableAutoSizing = true;
            labelTMP.fontSizeMin = 28;
            labelTMP.fontSizeMax = 52;
        }

        private static void CreateFilterBar(Transform parent)
        {
            GameObject filterBar = new GameObject("FilterBar");
            filterBar.transform.SetParent(parent, false);

            RectTransform rt = filterBar.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(0, 60);
            rt.anchoredPosition = new Vector2(0, -100);

            Image bg = filterBar.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.3f);

            HorizontalLayoutGroup hlg = filterBar.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8;
            hlg.padding = new RectOffset(15, 15, 8, 8);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;  // Expandir para llenar el ancho
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            // Filter Buttons - se distribuirán equitativamente
            CreateFilterButton(filterBar.transform, "Todos", true);
            CreateFilterButton(filterBar.transform, "Activos", false);
            CreateFilterButton(filterBar.transform, "Próximos", false);
            CreateFilterButton(filterBar.transform, "Mis Torneos", false);
        }

        private static void CreateFilterButton(Transform parent, string text, bool isActive)
        {
            GameObject btn = new GameObject("Filter_" + text);
            btn.transform.SetParent(parent, false);

            RectTransform rt = btn.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 60);

            LayoutElement le = btn.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;  // Distribuir espacio equitativamente
            le.minWidth = 70;
            le.preferredHeight = 60;

            Image bg = btn.AddComponent<Image>();
            bg.color = isActive ? CYAN : new Color(1, 1, 1, 0.15f);

            Button button = btn.AddComponent<Button>();
            button.targetGraphic = bg;
            ColorBlock colors = button.colors;
            colors.highlightedColor = isActive ? CYAN * 1.1f : new Color(1, 1, 1, 0.25f);
            colors.pressedColor = isActive ? CYAN * 0.9f : new Color(1, 1, 1, 0.3f);
            button.colors = colors;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btn.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 52;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = isActive ? BG_DARK : TEXT_WHITE;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 20;
            tmp.fontSizeMax = 52;
        }

        private static void CreateTournamentsList(Transform parent)
        {
            GameObject scrollView = new GameObject("TournamentsList");
            scrollView.transform.SetParent(parent, false);

            RectTransform svRT = scrollView.AddComponent<RectTransform>();
            svRT.anchorMin = Vector2.zero;
            svRT.anchorMax = Vector2.one;
            svRT.offsetMin = new Vector2(15, 15);
            svRT.offsetMax = new Vector2(-15, -155);

            ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = 30;

            Image svBg = scrollView.AddComponent<Image>();
            svBg.color = new Color(0, 0, 0, 0);

            // Viewport con RectMask2D
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);

            RectTransform vpRT = viewport.AddComponent<RectTransform>();
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.sizeDelta = Vector2.zero;

            viewport.AddComponent<Image>().color = new Color(0, 0, 0, 0);
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
            vlg.spacing = 24;
            vlg.padding = new RectOffset(10, 10, 10, 40);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = vpRT;
            scroll.content = contentRT;

            // Cargar prefab y crear sample cards
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);

            if (prefab != null)
            {
                CreateTournamentFromPrefab(content.transform, prefab, "QuickMath Championship", "QuickMath", 500, 5, "15/20", "02:45:00", true);
                CreateTournamentFromPrefab(content.transform, prefab, "Flash Tap Masters", "FlashTap", 300, 10, "8/16", "05:30:00", false);
                CreateTournamentFromPrefab(content.transform, prefab, "Cognitive Elite", "CognitiveSprint", 1000, 25, "5/10", "12:00:00", true);
                CreateTournamentFromPrefab(content.transform, prefab, "Memory Pairs Daily", "MemoryPairs", 100, 1, "18/20", "00:30:00", false);
            }
            else
            {
                Debug.LogWarning("[CashTournamentsUIBuilder] Prefab TournamentCardUI.prefab no encontrado. Ejecuta 'Create All Prefabs' primero.");

                // Crear cards básicos como fallback
                CreateFallbackCard(content.transform, "QuickMath Championship", 500, 5, "15/20");
                CreateFallbackCard(content.transform, "Flash Tap Masters", 300, 10, "8/16");
                CreateFallbackCard(content.transform, "Cognitive Elite", 1000, 25, "5/10");
                CreateFallbackCard(content.transform, "Memory Pairs Daily", 100, 1, "18/20");
            }
        }

        private static void CreateTournamentFromPrefab(Transform parent, GameObject prefab, string name, string game, int prize, int entry, string players, string timer, bool isLive)
        {
            GameObject card = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            card.name = "TournamentCard_" + game;

            // Actualizar datos del card
            Transform nameT = card.transform.Find("TournamentName");
            if (nameT != null)
            {
                TextMeshProUGUI tmp = nameT.GetComponent<TextMeshProUGUI>();
                if (tmp != null) tmp.text = name;
            }

            Transform prizeRow = card.transform.Find("PrizeRow");
            if (prizeRow != null)
            {
                Transform prizeText = prizeRow.Find("Text");
                if (prizeText != null)
                {
                    TextMeshProUGUI tmp = prizeText.GetComponent<TextMeshProUGUI>();
                    if (tmp != null) tmp.text = $"${prize}";
                }
            }

            Transform playersRow = card.transform.Find("PlayersRow");
            if (playersRow != null)
            {
                Transform playersText = playersRow.Find("Text");
                if (playersText != null)
                {
                    TextMeshProUGUI tmp = playersText.GetComponent<TextMeshProUGUI>();
                    if (tmp != null) tmp.text = players;
                }
            }

            Transform timerRow = card.transform.Find("TimerRow");
            if (timerRow != null)
            {
                Transform timerText = timerRow.Find("Text");
                if (timerText != null)
                {
                    TextMeshProUGUI tmp = timerText.GetComponent<TextMeshProUGUI>();
                    if (tmp != null) tmp.text = timer;
                }
            }

            Transform entryBadge = card.transform.Find("EntryFeeBadge");
            if (entryBadge != null)
            {
                Transform entryValue = entryBadge.Find("Value");
                if (entryValue != null)
                {
                    TextMeshProUGUI tmp = entryValue.GetComponent<TextMeshProUGUI>();
                    if (tmp != null) tmp.text = $"${entry}";
                }
            }

            Transform liveBadge = card.transform.Find("LiveBadge");
            if (liveBadge != null)
            {
                liveBadge.gameObject.SetActive(isLive);
            }

            // Game Icon - Cargar el icono apropiado (sprite está en child "Sprite")
            Transform gameIcon = card.transform.Find("GameIcon");
            if (gameIcon != null)
            {
                Transform spriteChild = gameIcon.Find("Sprite");
                Image iconImg = spriteChild != null ? spriteChild.GetComponent<Image>() : gameIcon.GetComponent<Image>();
                if (iconImg != null)
                {
                    string iconPath = $"Assets/_Project/Art/Icons/Games/{game}Icon.png";
                    Sprite gameSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
                    if (gameSprite != null)
                    {
                        iconImg.sprite = gameSprite;
                    }
                }
            }
        }

        private static void CreateMissingElements(Transform parent)
        {
            // A. NoTournamentsText - hidden, shown when no tournaments available
            GameObject noTournamentsObj = new GameObject("NoTournamentsText");
            noTournamentsObj.transform.SetParent(parent, false);
            noTournamentsObj.SetActive(false);

            RectTransform ntRT = noTournamentsObj.AddComponent<RectTransform>();
            ntRT.anchorMin = new Vector2(0.1f, 0.3f);
            ntRT.anchorMax = new Vector2(0.9f, 0.7f);
            ntRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI ntTMP = noTournamentsObj.AddComponent<TextMeshProUGUI>();
            ntTMP.text = "No hay torneos disponibles";
            ntTMP.fontSize = 28;
            ntTMP.color = TEXT_SECONDARY;
            ntTMP.alignment = TextAlignmentOptions.Center;

            // B. LoadingIndicator - hidden, shown while loading
            GameObject loadingObj = new GameObject("LoadingIndicator");
            loadingObj.transform.SetParent(parent, false);
            loadingObj.SetActive(false);

            RectTransform liRT = loadingObj.AddComponent<RectTransform>();
            liRT.anchorMin = new Vector2(0.2f, 0.4f);
            liRT.anchorMax = new Vector2(0.8f, 0.6f);
            liRT.sizeDelta = Vector2.zero;

            GameObject loadingTextObj = new GameObject("LoadingText");
            loadingTextObj.transform.SetParent(loadingObj.transform, false);

            RectTransform ltRT = loadingTextObj.AddComponent<RectTransform>();
            ltRT.anchorMin = Vector2.zero;
            ltRT.anchorMax = Vector2.one;
            ltRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI ltTMP = loadingTextObj.AddComponent<TextMeshProUGUI>();
            ltTMP.text = "Cargando...";
            ltTMP.fontSize = 28;
            ltTMP.color = TEXT_WHITE;
            ltTMP.alignment = TextAlignmentOptions.Center;

            // C. RefreshButton - hidden refresh button
            GameObject refreshBtnObj = new GameObject("RefreshButton");
            refreshBtnObj.transform.SetParent(parent, false);

            RectTransform rbRT = refreshBtnObj.AddComponent<RectTransform>();
            rbRT.anchorMin = new Vector2(1, 1);
            rbRT.anchorMax = new Vector2(1, 1);
            rbRT.pivot = new Vector2(1, 1);
            rbRT.sizeDelta = new Vector2(160, 50);
            rbRT.anchoredPosition = new Vector2(-15, -105);

            Image rbBg = refreshBtnObj.AddComponent<Image>();
            rbBg.color = CYAN;

            Button rbBtn = refreshBtnObj.AddComponent<Button>();
            rbBtn.targetGraphic = rbBg;

            GameObject rbTextObj = new GameObject("Text");
            rbTextObj.transform.SetParent(refreshBtnObj.transform, false);

            RectTransform rbtRT = rbTextObj.AddComponent<RectTransform>();
            rbtRT.anchorMin = Vector2.zero;
            rbtRT.anchorMax = Vector2.one;
            rbtRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI rbTMP = rbTextObj.AddComponent<TextMeshProUGUI>();
            rbTMP.text = "Actualizar";
            rbTMP.fontSize = 28;
            rbTMP.color = BG_DARK;
            rbTMP.fontStyle = FontStyles.Bold;
            rbTMP.alignment = TextAlignmentOptions.Center;

            // D. GameFilterDropdown
            CreateTMPDropdown(parent, "GameFilterDropdown",
                new List<string> { "Todos", "QuickMath", "DigitRush", "FlashTap", "MemoryPairs", "OddOneOut" },
                new Vector2(-210, -105), new Vector2(200, 50));

            // D. FeeFilterDropdown
            CreateTMPDropdown(parent, "FeeFilterDropdown",
                new List<string> { "Todas", "$1", "$5", "$10", "$25" },
                new Vector2(-420, -105), new Vector2(200, 50));

            // E. CreateTournamentPanel - hidden overlay
            CreateCreateTournamentPanel(parent);
        }

        private static void CreateTMPDropdown(Transform parent, string name, List<string> options, Vector2 anchoredPos, Vector2 size)
        {
            GameObject ddObj = new GameObject(name);
            ddObj.transform.SetParent(parent, false);

            RectTransform ddRT = ddObj.AddComponent<RectTransform>();
            ddRT.anchorMin = new Vector2(1, 1);
            ddRT.anchorMax = new Vector2(1, 1);
            ddRT.pivot = new Vector2(1, 1);
            ddRT.sizeDelta = size;
            ddRT.anchoredPosition = anchoredPos;

            Image ddBg = ddObj.AddComponent<Image>();
            ddBg.color = new Color(0.15f, 0.17f, 0.22f, 1f);

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
            capTMP.fontSize = 28;
            capTMP.color = TEXT_WHITE;
            capTMP.alignment = TextAlignmentOptions.Left;

            dd.captionText = capTMP;

            // Add options
            dd.ClearOptions();
            dd.AddOptions(options);
        }

        private static void CreateCreateTournamentPanel(Transform parent)
        {
            // Overlay panel - hidden by default
            GameObject panel = new GameObject("CreateTournamentPanel");
            panel.transform.SetParent(parent, false);
            panel.SetActive(false);

            RectTransform pRT = panel.AddComponent<RectTransform>();
            pRT.anchorMin = Vector2.zero;
            pRT.anchorMax = Vector2.one;
            pRT.sizeDelta = Vector2.zero;

            Image pBg = panel.AddComponent<Image>();
            pBg.color = new Color(0f, 0f, 0f, 0.85f);

            // Content container with vertical layout
            GameObject contentContainer = new GameObject("PanelContent");
            contentContainer.transform.SetParent(panel.transform, false);

            RectTransform ccRT = contentContainer.AddComponent<RectTransform>();
            ccRT.anchorMin = new Vector2(0.1f, 0.15f);
            ccRT.anchorMax = new Vector2(0.9f, 0.85f);
            ccRT.sizeDelta = Vector2.zero;

            Image ccBg = contentContainer.AddComponent<Image>();
            ccBg.color = CARD_BG;

            VerticalLayoutGroup vlg = contentContainer.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 20;
            vlg.padding = new RectOffset(20, 20, 20, 20);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            // Panel Title
            GameObject titleObj = new GameObject("PanelTitle");
            titleObj.transform.SetParent(contentContainer.transform, false);

            RectTransform ttRT = titleObj.AddComponent<RectTransform>();
            ttRT.sizeDelta = new Vector2(0, 50);

            LayoutElement ttLE = titleObj.AddComponent<LayoutElement>();
            ttLE.preferredHeight = 50;

            TextMeshProUGUI ttTMP = titleObj.AddComponent<TextMeshProUGUI>();
            ttTMP.text = "Crear Torneo";
            ttTMP.fontSize = 36;
            ttTMP.fontStyle = FontStyles.Bold;
            ttTMP.color = GOLD;
            ttTMP.alignment = TextAlignmentOptions.Center;

            // MaxPlayersSlider
            CreateSliderWithLabel(contentContainer.transform, "MaxPlayersSlider", "MaxPlayersText", "Max Jugadores: 10");

            // EntryFeeSlider
            CreateSliderWithLabel(contentContainer.transform, "EntryFeeSlider", "EntryFeeText", "Entry Fee: $5");

            // DurationSlider
            CreateSliderWithLabel(contentContainer.transform, "DurationSlider", "DurationText", "Duración: 30 min");

            // GameTypeDropdown
            CreateTMPDropdownInLayout(contentContainer.transform, "GameTypeDropdown",
                new List<string> { "QuickMath", "DigitRush", "FlashTap", "MemoryPairs", "OddOneOut" });

            // Buttons row
            GameObject buttonsRow = new GameObject("ButtonsRow");
            buttonsRow.transform.SetParent(contentContainer.transform, false);

            RectTransform brRT = buttonsRow.AddComponent<RectTransform>();
            brRT.sizeDelta = new Vector2(0, 60);

            LayoutElement brLE = buttonsRow.AddComponent<LayoutElement>();
            brLE.preferredHeight = 60;

            HorizontalLayoutGroup hlg = buttonsRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            // ConfirmCreateButton
            CreatePanelButton(buttonsRow.transform, "ConfirmCreateButton", "Crear Torneo", GREEN, BG_DARK);

            // CancelCreateButton
            CreatePanelButton(buttonsRow.transform, "CancelCreateButton", "Cancelar", new Color(0.6f, 0.2f, 0.2f, 1f), TEXT_WHITE);
        }

        private static void CreateSliderWithLabel(Transform parent, string sliderName, string textName, string defaultText)
        {
            // Container for slider row
            GameObject row = new GameObject(sliderName + "Row");
            row.transform.SetParent(parent, false);

            RectTransform rowRT = row.AddComponent<RectTransform>();
            rowRT.sizeDelta = new Vector2(0, 60);

            LayoutElement rowLE = row.AddComponent<LayoutElement>();
            rowLE.preferredHeight = 60;

            // Label text
            GameObject textObj = new GameObject(textName);
            textObj.transform.SetParent(row.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0, 0.5f);
            textRT.anchorMax = new Vector2(0.4f, 1);
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            TextMeshProUGUI textTMP = textObj.AddComponent<TextMeshProUGUI>();
            textTMP.text = defaultText;
            textTMP.fontSize = 24;
            textTMP.color = TEXT_WHITE;
            textTMP.alignment = TextAlignmentOptions.Left;

            // Slider
            GameObject sliderObj = new GameObject(sliderName);
            sliderObj.transform.SetParent(row.transform, false);

            RectTransform sliderRT = sliderObj.AddComponent<RectTransform>();
            sliderRT.anchorMin = new Vector2(0.42f, 0.15f);
            sliderRT.anchorMax = new Vector2(1, 0.85f);
            sliderRT.offsetMin = Vector2.zero;
            sliderRT.offsetMax = Vector2.zero;

            Slider slider = sliderObj.AddComponent<Slider>();

            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(sliderObj.transform, false);

            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.25f, 1f);

            RectTransform bgRT = bg.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;

            // Fill Area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);

            RectTransform faRT = fillArea.AddComponent<RectTransform>();
            faRT.anchorMin = Vector2.zero;
            faRT.anchorMax = new Vector2(1, 1);
            faRT.sizeDelta = Vector2.zero;

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);

            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = CYAN;

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
            handleImg.color = Color.white;

            RectTransform handleRT = handle.GetComponent<RectTransform>();
            handleRT.sizeDelta = new Vector2(30, 30);

            slider.handleRect = handleRT;
        }

        private static void CreateTMPDropdownInLayout(Transform parent, string name, List<string> options)
        {
            GameObject ddObj = new GameObject(name);
            ddObj.transform.SetParent(parent, false);

            RectTransform ddRT = ddObj.AddComponent<RectTransform>();
            ddRT.sizeDelta = new Vector2(0, 60);

            LayoutElement ddLE = ddObj.AddComponent<LayoutElement>();
            ddLE.preferredHeight = 60;

            Image ddBg = ddObj.AddComponent<Image>();
            ddBg.color = new Color(0.15f, 0.17f, 0.22f, 1f);

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
            capTMP.fontSize = 28;
            capTMP.color = TEXT_WHITE;
            capTMP.alignment = TextAlignmentOptions.Left;

            dd.captionText = capTMP;

            // Add options
            dd.ClearOptions();
            dd.AddOptions(options);
        }

        private static void CreatePanelButton(Transform parent, string name, string text, Color bgColor, Color textColor)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            RectTransform btnRT = btnObj.AddComponent<RectTransform>();
            btnRT.sizeDelta = new Vector2(0, 55);

            Image btnBg = btnObj.AddComponent<Image>();
            btnBg.color = bgColor;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnBg;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform tRT = textObj.AddComponent<RectTransform>();
            tRT.anchorMin = Vector2.zero;
            tRT.anchorMax = Vector2.one;
            tRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 28;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        private static Canvas FindMainCanvas()
        {
            foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (c.gameObject.name == "Canvas")
                    return c;
            }
            foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (c.gameObject.name != "TransitionCanvas")
                    return c;
            }
            return Object.FindFirstObjectByType<Canvas>();
        }

        private static void CreateFallbackCard(Transform parent, string name, int prize, int entry, string players)
        {
            GameObject card = new GameObject("TournamentCard_Fallback");
            card.transform.SetParent(parent, false);

            RectTransform rt = card.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 200);

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredHeight = 200;

            Image bg = card.AddComponent<Image>();
            bg.color = CARD_BG;

            // Name
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(card.transform, false);

            RectTransform nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0.5f);
            nameRT.anchorMax = new Vector2(0.6f, 1);
            nameRT.offsetMin = new Vector2(30, 10);
            nameRT.offsetMax = new Vector2(0, -10);

            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = name;
            nameText.fontSize = 36;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = GOLD;
            nameText.alignment = TextAlignmentOptions.Left;

            // Prize
            GameObject prizeObj = new GameObject("Prize");
            prizeObj.transform.SetParent(card.transform, false);

            RectTransform prizeRT = prizeObj.AddComponent<RectTransform>();
            prizeRT.anchorMin = new Vector2(0, 0);
            prizeRT.anchorMax = new Vector2(0.4f, 0.5f);
            prizeRT.offsetMin = new Vector2(30, 10);
            prizeRT.offsetMax = new Vector2(0, 0);

            TextMeshProUGUI prizeText = prizeObj.AddComponent<TextMeshProUGUI>();
            prizeText.text = $"Premio: ${prize}";
            prizeText.fontSize = 32;
            prizeText.fontStyle = FontStyles.Bold;
            prizeText.color = GREEN;
            prizeText.alignment = TextAlignmentOptions.Left;

            // Entry & Players
            GameObject infoObj = new GameObject("Info");
            infoObj.transform.SetParent(card.transform, false);

            RectTransform infoRT = infoObj.AddComponent<RectTransform>();
            infoRT.anchorMin = new Vector2(0.4f, 0);
            infoRT.anchorMax = new Vector2(0.7f, 0.5f);
            infoRT.offsetMin = new Vector2(0, 10);
            infoRT.offsetMax = new Vector2(0, 0);

            TextMeshProUGUI infoText = infoObj.AddComponent<TextMeshProUGUI>();
            infoText.text = $"${entry} | {players}";
            infoText.fontSize = 28;
            infoText.fontStyle = FontStyles.Bold;
            infoText.color = TEXT_SECONDARY;
            infoText.alignment = TextAlignmentOptions.Center;

            // Join Button
            GameObject joinBtn = new GameObject("JoinButton");
            joinBtn.transform.SetParent(card.transform, false);

            RectTransform joinRT = joinBtn.AddComponent<RectTransform>();
            joinRT.anchorMin = new Vector2(0.75f, 0.2f);
            joinRT.anchorMax = new Vector2(0.95f, 0.8f);
            joinRT.offsetMin = Vector2.zero;
            joinRT.offsetMax = Vector2.zero;

            Image joinBg = joinBtn.AddComponent<Image>();
            joinBg.color = GREEN;

            joinBtn.AddComponent<Button>().targetGraphic = joinBg;

            GameObject joinText = new GameObject("Text");
            joinText.transform.SetParent(joinBtn.transform, false);

            RectTransform jtRT = joinText.AddComponent<RectTransform>();
            jtRT.anchorMin = Vector2.zero;
            jtRT.anchorMax = Vector2.one;
            jtRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI jt = joinText.AddComponent<TextMeshProUGUI>();
            jt.text = "Unirse";
            jt.fontSize = 32;
            jt.fontStyle = FontStyles.Bold;
            jt.color = Color.white;
            jt.alignment = TextAlignmentOptions.Center;
        }

        #region Reference Assigner

        private static MonoBehaviour FindTournamentController()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "TournamentListPanel") return mb;
            return null;
        }

        private static void ResetAssignState()
        {
            assignedCount = 0; failedCount = 0; alreadySetCount = 0;
            assignResults.Clear();
        }

        private static void RunAssignAllReferences()
        {
            var panel = FindTournamentController();
            if (panel == null)
            {
                Debug.LogError("[CashTournamentsUIBuilder] TournamentListPanel no encontrado!");
                return;
            }

            SerializedObject so = new SerializedObject(panel);
            so.Update();

            Canvas canvas = FindMainCanvas();
            Transform root = canvas != null ? canvas.transform : panel.transform.root;

            // Header
            AssignRef(so, "titleText", FindTextDeep(root, "TitleText"));

            Transform backBtnT = FindDeep(root, "BackButton");
            AssignRef(so, "backButton", backBtnT != null ? backBtnT.GetComponent<Button>() : null);

            // Tournament List - "Content" is the scroll content container
            AssignRef(so, "tournamentsContainer", FindDeep(root, "Content"));

            // Note: tournamentCardPrefab skipped - prefab requires manual assignment

            // No Tournaments Text
            AssignRef(so, "noTournamentsText", FindTextDeep(root, "NoTournamentsText"));

            // Filters (TMP_Dropdown)
            AssignRef(so, "gameFilterDropdown", FindDropdownDeep(root, "GameFilterDropdown"));
            AssignRef(so, "feeFilterDropdown", FindDropdownDeep(root, "FeeFilterDropdown"));

            // Actions
            AssignRef(so, "refreshButton", FindBtnDeep(root, "RefreshButton"));

            Transform loadingT = FindDeep(root, "LoadingIndicator");
            AssignGORef(so, "loadingIndicator", loadingT);

            // Create Tournament Button
            Transform createBtnT = FindDeep(root, "CreateTournamentBtn");
            if (createBtnT == null) createBtnT = FindDeep(root, "CreateTournamentButton");
            AssignRef(so, "createTournamentButton", createBtnT != null ? createBtnT.GetComponent<Button>() : null);

            // Create Tournament Panel (GameObject)
            Transform createPanelT = FindDeep(root, "CreateTournamentPanel");
            AssignGORef(so, "createTournamentPanel", createPanelT);

            // Sliders (Slider components)
            AssignRef(so, "maxPlayersSlider", FindSliderDeep(root, "MaxPlayersSlider"));
            AssignRef(so, "maxPlayersText", FindTextDeep(root, "MaxPlayersText"));
            AssignRef(so, "entryFeeSlider", FindSliderDeep(root, "EntryFeeSlider"));
            AssignRef(so, "entryFeeText", FindTextDeep(root, "EntryFeeText"));
            AssignRef(so, "durationSlider", FindSliderDeep(root, "DurationSlider"));
            AssignRef(so, "durationText", FindTextDeep(root, "DurationText"));

            // Game Type Dropdown (TMP_Dropdown)
            AssignRef(so, "gameTypeDropdown", FindDropdownDeep(root, "GameTypeDropdown"));

            // Confirm / Cancel Create (Buttons)
            AssignRef(so, "confirmCreateButton", FindBtnDeep(root, "ConfirmCreateButton"));
            AssignRef(so, "cancelCreateButton", FindBtnDeep(root, "CancelCreateButton"));

            // Premium Required Panel (find by type ConfirmPanelUI)
            MonoBehaviour confirmPanel = null;
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
            {
                if (mb.GetType().Name == "ConfirmPanelUI") { confirmPanel = mb; break; }
            }
            AssignRef(so, "premiumRequiredPanel", confirmPanel);

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(panel);
            EditorUtility.SetDirty(panel.gameObject);
            EditorSceneManager.MarkSceneDirty(panel.gameObject.scene);

            Debug.Log($"[CashTournamentsUIBuilder] Referencias: {assignedCount} asignadas, {alreadySetCount} ya puestas, {failedCount} fallidas");
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

        private static Slider FindSliderDeep(Transform root, string name)
        {
            Transform t = FindDeep(root, name);
            return t != null ? t.GetComponent<Slider>() : null;
        }

        private static TMP_Dropdown FindDropdownDeep(Transform root, string name)
        {
            Transform t = FindDeep(root, name);
            return t != null ? t.GetComponent<TMP_Dropdown>() : null;
        }

        private static void AssignRef(SerializedObject so, string prop, Object value)
        {
            var p = so.FindProperty(prop);
            if (p == null) { AddAR(prop, "Propiedad no existe", false, null); failedCount++; return; }
            if (p.objectReferenceValue != null) { AddAR(prop, "Ya asignada", true, p.objectReferenceValue); alreadySetCount++; return; }
            if (value != null) { p.objectReferenceValue = value; AddAR(prop, "Asignada", true, value); assignedCount++; }
            else { AddAR(prop, "No encontrada", false, null); failedCount++; }
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
            GUILayout.Label(rate == 1f ? "TODAS LAS REFERENCIAS ASIGNADAS" : "Algunas referencias faltan", EditorStyles.boldLabel);
            GUI.color = Color.white;

            GUILayout.Label($"Asignadas: {assignedCount} | Ya puestas: {alreadySetCount} | Fallidas: {failedCount}");
            EditorGUILayout.Space(5);

            foreach (var r in assignResults)
            {
                EditorGUILayout.BeginHorizontal();
                GUI.color = r.success ? (r.status == "Ya asignada" ? new Color(0.5f, 0.8f, 1f) : Color.green) : Color.red;
                GUILayout.Label(r.success ? (r.status == "Ya asignada" ? "o" : "+") : "x", GUILayout.Width(16));
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
