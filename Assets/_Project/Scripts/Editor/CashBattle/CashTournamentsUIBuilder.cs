using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// UI Builder PREMIUM para la escena CashTournaments.unity
    /// Construye la lista de torneos con iconos premium y diseño profesional.
    /// </summary>
    public class CashTournamentsUIBuilder : EditorWindow
    {
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

        #endregion

        [MenuItem("DigitPark/UI Builders/CashBattle/Cash Tournaments (Premium)", false, 252)]
        public static void ShowWindow()
        {
            GetWindow<CashTournamentsUIBuilder>("Cash Tournaments Builder");
        }

        private void OnGUI()
        {
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
        }

        private static void BuildCashTournamentsUI()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
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
            Canvas canvas = FindFirstObjectByType<Canvas>();
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
        }

        private static void CleanupOldElements(Transform parent)
        {
            string[] toDestroy = { "Background", "SafeArea", "Header", "TournamentsList", "FilterBar" };
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

        private static void CreateHeaderTitle(Transform parent)
        {
            GameObject titleContainer = new GameObject("TitleContainer");
            titleContainer.transform.SetParent(parent, false);

            RectTransform tcRT = titleContainer.AddComponent<RectTransform>();
            tcRT.anchorMin = new Vector2(0, 0.5f);
            tcRT.anchorMax = new Vector2(0.6f, 0.5f);
            tcRT.pivot = new Vector2(0, 0.5f);
            tcRT.sizeDelta = new Vector2(0, 50);
            tcRT.anchoredPosition = new Vector2(75, 0);

            // Bracket Icon
            GameObject bracketIcon = new GameObject("BracketIcon");
            bracketIcon.transform.SetParent(titleContainer.transform, false);

            RectTransform biRT = bracketIcon.AddComponent<RectTransform>();
            biRT.anchorMin = new Vector2(0, 0.5f);
            biRT.anchorMax = new Vector2(0, 0.5f);
            biRT.pivot = new Vector2(0, 0.5f);
            biRT.sizeDelta = new Vector2(40, 40);
            biRT.anchoredPosition = Vector2.zero;

            Image biImg = bracketIcon.AddComponent<Image>();
            biImg.preserveAspect = true;
            Sprite bracketSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TOURNAMENT_ICONS_PATH + "TournamentBracketIcon.png");
            if (bracketSprite != null)
            {
                biImg.sprite = bracketSprite;
                biImg.color = Color.white;
            }
            else
            {
                biImg.color = GOLD;
            }

            // Title Text
            GameObject title = new GameObject("TitleText");
            title.transform.SetParent(titleContainer.transform, false);

            RectTransform titleRT = title.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.offsetMin = new Vector2(48, 0);
            titleRT.offsetMax = Vector2.zero;

            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "Torneos";
            titleText.fontSize = 26;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = GOLD;
            titleText.alignment = TextAlignmentOptions.Left;
        }

        private static void CreateBalanceDisplay(Transform parent)
        {
            GameObject balance = new GameObject("BalanceDisplay");
            balance.transform.SetParent(parent, false);

            RectTransform rt = balance.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(1, 0.5f);
            rt.sizeDelta = new Vector2(110, 38);
            rt.anchoredPosition = new Vector2(-15, 0);

            Image bg = balance.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.5f);

            GameObject text = new GameObject("Text");
            text.transform.SetParent(balance.transform, false);

            RectTransform textRT = text.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI textTMP = text.AddComponent<TextMeshProUGUI>();
            textTMP.text = "$ 125.50";
            textTMP.fontSize = 18;
            textTMP.fontStyle = FontStyles.Bold;
            textTMP.color = GREEN;
            textTMP.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateNewTournamentButton(Transform parent)
        {
            GameObject createBtn = new GameObject("CreateTournamentBtn");
            createBtn.transform.SetParent(parent, false);

            RectTransform rt = createBtn.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(1, 0.5f);
            rt.sizeDelta = new Vector2(50, 38);
            rt.anchoredPosition = new Vector2(-135, 0);

            Image bg = createBtn.AddComponent<Image>();
            bg.color = GOLD_DARK;

            Button btn = createBtn.AddComponent<Button>();
            btn.targetGraphic = bg;

            // Plus Icon
            GameObject icon = new GameObject("Icon");
            icon.transform.SetParent(createBtn.transform, false);

            RectTransform iconRT = icon.AddComponent<RectTransform>();
            iconRT.anchorMin = Vector2.zero;
            iconRT.anchorMax = Vector2.one;
            iconRT.offsetMin = new Vector2(8, 8);
            iconRT.offsetMax = new Vector2(-8, -8);

            Image iconImg = icon.AddComponent<Image>();
            iconImg.preserveAspect = true;
            Sprite createSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TOURNAMENT_ICONS_PATH + "CreateTournamentIcon.png");
            if (createSprite != null)
            {
                iconImg.sprite = createSprite;
                iconImg.color = Color.white;
            }
            else
            {
                DestroyImmediate(icon);
                GameObject plusText = new GameObject("PlusText");
                plusText.transform.SetParent(createBtn.transform, false);

                RectTransform ptRT = plusText.AddComponent<RectTransform>();
                ptRT.anchorMin = Vector2.zero;
                ptRT.anchorMax = Vector2.one;
                ptRT.sizeDelta = Vector2.zero;

                TextMeshProUGUI pt = plusText.AddComponent<TextMeshProUGUI>();
                pt.text = "+";
                pt.fontSize = 28;
                pt.fontStyle = FontStyles.Bold;
                pt.color = Color.white;
                pt.alignment = TextAlignmentOptions.Center;
            }
        }

        private static void CreateFilterBar(Transform parent)
        {
            GameObject filterBar = new GameObject("FilterBar");
            filterBar.transform.SetParent(parent, false);

            RectTransform rt = filterBar.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(0, 50);
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
            rt.sizeDelta = new Vector2(0, 34);

            LayoutElement le = btn.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;  // Distribuir espacio equitativamente
            le.minWidth = 70;
            le.preferredHeight = 34;

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
            tmp.fontSize = 14;
            tmp.fontStyle = isActive ? FontStyles.Bold : FontStyles.Normal;
            tmp.color = isActive ? BG_DARK : TEXT_WHITE;
            tmp.alignment = TextAlignmentOptions.Center;
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
            vlg.spacing = 12;
            vlg.padding = new RectOffset(5, 5, 5, 20);
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

            // Game Icon - Cargar el icono apropiado
            Transform gameIcon = card.transform.Find("GameIcon");
            if (gameIcon != null)
            {
                Image iconImg = gameIcon.GetComponent<Image>();
                if (iconImg != null)
                {
                    string iconPath = $"Assets/_Project/Art/Icons/Games/CashBattle/{game}Icon.png";
                    Sprite gameSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
                    if (gameSprite != null)
                    {
                        iconImg.sprite = gameSprite;
                    }
                }
            }
        }

        private static void CreateFallbackCard(Transform parent, string name, int prize, int entry, string players)
        {
            GameObject card = new GameObject("TournamentCard_Fallback");
            card.transform.SetParent(parent, false);

            RectTransform rt = card.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 100);

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredHeight = 100;

            Image bg = card.AddComponent<Image>();
            bg.color = CARD_BG;

            // Name
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(card.transform, false);

            RectTransform nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0.5f);
            nameRT.anchorMax = new Vector2(0.6f, 1);
            nameRT.offsetMin = new Vector2(15, 5);
            nameRT.offsetMax = new Vector2(0, -5);

            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = name;
            nameText.fontSize = 18;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = GOLD;
            nameText.alignment = TextAlignmentOptions.Left;

            // Prize
            GameObject prizeObj = new GameObject("Prize");
            prizeObj.transform.SetParent(card.transform, false);

            RectTransform prizeRT = prizeObj.AddComponent<RectTransform>();
            prizeRT.anchorMin = new Vector2(0, 0);
            prizeRT.anchorMax = new Vector2(0.4f, 0.5f);
            prizeRT.offsetMin = new Vector2(15, 5);
            prizeRT.offsetMax = new Vector2(0, 0);

            TextMeshProUGUI prizeText = prizeObj.AddComponent<TextMeshProUGUI>();
            prizeText.text = $"Premio: ${prize}";
            prizeText.fontSize = 16;
            prizeText.color = GREEN;
            prizeText.alignment = TextAlignmentOptions.Left;

            // Entry & Players
            GameObject infoObj = new GameObject("Info");
            infoObj.transform.SetParent(card.transform, false);

            RectTransform infoRT = infoObj.AddComponent<RectTransform>();
            infoRT.anchorMin = new Vector2(0.4f, 0);
            infoRT.anchorMax = new Vector2(0.7f, 0.5f);
            infoRT.offsetMin = new Vector2(0, 5);
            infoRT.offsetMax = new Vector2(0, 0);

            TextMeshProUGUI infoText = infoObj.AddComponent<TextMeshProUGUI>();
            infoText.text = $"${entry} | {players}";
            infoText.fontSize = 14;
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
            jt.fontSize = 16;
            jt.fontStyle = FontStyles.Bold;
            jt.color = Color.white;
            jt.alignment = TextAlignmentOptions.Center;
        }
    }
}
