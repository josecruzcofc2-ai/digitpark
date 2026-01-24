using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// UI Builder para la escena CashTournaments.unity
    /// Construye la lista de torneos disponibles con dinero real.
    /// </summary>
    public class CashTournamentsUIBuilder : EditorWindow
    {
        #region Colors

        private static readonly Color GOLD_PRIMARY = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GOLD_DARK = new Color(0.85f, 0.65f, 0.13f, 1f);
        private static readonly Color GOLD_LIGHT = new Color(1f, 0.93f, 0.55f, 1f);

        private static readonly Color BG_DARK = new Color(0.08f, 0.06f, 0.12f, 1f);
        private static readonly Color CARD_BG = new Color(0.12f, 0.1f, 0.15f, 0.95f);
        private static readonly Color CARD_BORDER = new Color(0.85f, 0.65f, 0.13f, 0.6f);

        private static readonly Color TEXT_PRIMARY = new Color(1f, 1f, 1f, 1f);
        private static readonly Color TEXT_GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.7f, 0.7f, 0.7f, 1f);

        private static readonly Color BUTTON_GOLD = new Color(0.85f, 0.65f, 0.13f, 1f);
        private static readonly Color SUCCESS_GREEN = new Color(0.3f, 1f, 0.5f, 1f);

        #endregion

        [MenuItem("DigitPark/UI Builders/CashBattle/Cash Tournaments", false, 252)]
        public static void ShowWindow()
        {
            GetWindow<CashTournamentsUIBuilder>("Cash Tournaments Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Cash Tournaments UI Builder", EditorStyles.boldLabel);
            GUILayout.Label("Lista de torneos con dinero real", EditorStyles.miniLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Construye la UI para CashTournaments.unity:\n\n" +
                "- Header con título y balance\n" +
                "- Lista scrolleable de torneos\n" +
                "- Cards de torneo con:\n" +
                "  * Nombre del juego\n" +
                "  * Premio pool\n" +
                "  * Entrada requerida\n" +
                "  * Jugadores inscritos\n" +
                "  * Botón 'Unirse'",
                MessageType.Info);

            EditorGUILayout.Space(10);

            GUI.backgroundColor = GOLD_PRIMARY;
            if (GUILayout.Button("CONSTRUIR UI COMPLETA", GUILayout.Height(40)))
            {
                BuildCashTournamentsUI();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(15);

            if (GUILayout.Button("Solo Tournament Card Prefab", GUILayout.Height(28)))
            {
                BuildTournamentCardPrefab();
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

            if (EditorUtility.DisplayDialog("Reconstruir UI?",
                "Esto reconstruirá la UI de Cash Tournaments.\n\n¿Continuar?",
                "Sí, Construir", "Cancelar"))
            {
                BuildAllElements(canvas);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                Debug.Log("[CashTournamentsUIBuilder] UI construida exitosamente!");
            }
        }

        private static void BuildTournamentCardPrefab()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            GameObject prefab = CreateTournamentCard(canvas.transform, "Tournament Name", "QuickMath", 500, 5, "12/20");
            Selection.activeGameObject = prefab;
            Debug.Log("[CashTournamentsUIBuilder] Tournament Card creado. Guárdalo como prefab.");
        }

        private static void BuildAllElements(Canvas canvas)
        {
            Transform canvasTransform = canvas.transform;

            // Limpiar
            CleanupOldElements(canvasTransform);

            // Background
            CreateBackground(canvasTransform);

            // Safe Area
            GameObject safeArea = CreateSafeArea(canvasTransform);

            // Header
            CreateHeader(safeArea.transform);

            // Content con ScrollView
            CreateTournamentsList(safeArea.transform);
        }

        private static void CleanupOldElements(Transform parent)
        {
            string[] toDestroy = { "Background", "SafeArea", "Header", "TournamentsList" };
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

        private static void CreateHeader(Transform parent)
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(parent, false);

            RectTransform rt = header.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(0, 120);

            Image bg = header.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.3f);

            // Back button
            GameObject backBtn = new GameObject("BackButton");
            backBtn.transform.SetParent(header.transform, false);

            RectTransform backRT = backBtn.AddComponent<RectTransform>();
            backRT.anchorMin = new Vector2(0, 0.5f);
            backRT.anchorMax = new Vector2(0, 0.5f);
            backRT.sizeDelta = new Vector2(80, 80);
            backRT.anchoredPosition = new Vector2(50, 0);

            backBtn.AddComponent<Button>();
            Image backBg = backBtn.AddComponent<Image>();
            backBg.color = new Color(1, 1, 1, 0);

            GameObject arrow = new GameObject("Arrow");
            arrow.transform.SetParent(backBtn.transform, false);
            RectTransform arrowRT = arrow.AddComponent<RectTransform>();
            arrowRT.anchorMin = Vector2.zero;
            arrowRT.anchorMax = Vector2.one;
            arrowRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI arrowText = arrow.AddComponent<TextMeshProUGUI>();
            arrowText.text = "<";
            arrowText.fontSize = 48;
            arrowText.color = TEXT_PRIMARY;
            arrowText.alignment = TextAlignmentOptions.Center;

            // Title
            GameObject title = new GameObject("Title");
            title.transform.SetParent(header.transform, false);

            RectTransform titleRT = title.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 0.5f);
            titleRT.anchorMax = new Vector2(0.5f, 0.5f);
            titleRT.sizeDelta = new Vector2(400, 60);

            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "Torneos Disponibles";
            titleText.fontSize = 32;
            titleText.color = TEXT_GOLD;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;

            // Balance
            GameObject balance = new GameObject("Balance");
            balance.transform.SetParent(header.transform, false);

            RectTransform balanceRT = balance.AddComponent<RectTransform>();
            balanceRT.anchorMin = new Vector2(1, 0.5f);
            balanceRT.anchorMax = new Vector2(1, 0.5f);
            balanceRT.pivot = new Vector2(1, 0.5f);
            balanceRT.sizeDelta = new Vector2(120, 40);
            balanceRT.anchoredPosition = new Vector2(-20, 0);

            Image balanceBg = balance.AddComponent<Image>();
            balanceBg.color = new Color(0, 0, 0, 0.5f);

            GameObject balanceText = new GameObject("Text");
            balanceText.transform.SetParent(balance.transform, false);
            RectTransform btRT = balanceText.AddComponent<RectTransform>();
            btRT.anchorMin = Vector2.zero;
            btRT.anchorMax = Vector2.one;
            btRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI bt = balanceText.AddComponent<TextMeshProUGUI>();
            bt.text = "$ 0.00";
            bt.fontSize = 22;
            bt.color = SUCCESS_GREEN;
            bt.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateTournamentsList(Transform parent)
        {
            // Scroll View container
            GameObject scrollView = new GameObject("TournamentsList");
            scrollView.transform.SetParent(parent, false);

            RectTransform svRT = scrollView.AddComponent<RectTransform>();
            svRT.anchorMin = Vector2.zero;
            svRT.anchorMax = Vector2.one;
            svRT.offsetMin = new Vector2(20, 20);
            svRT.offsetMax = new Vector2(-20, -130);

            ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;

            Image svBg = scrollView.AddComponent<Image>();
            svBg.color = new Color(0, 0, 0, 0);

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);

            RectTransform vpRT = viewport.AddComponent<RectTransform>();
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.sizeDelta = Vector2.zero;

            viewport.AddComponent<Image>().color = new Color(0, 0, 0, 0);
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);

            RectTransform contentRT = content.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.sizeDelta = new Vector2(0, 0);

            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 15;
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = vpRT;
            scroll.content = contentRT;

            // Sample tournaments
            CreateTournamentCard(content.transform, "Quick Math Championship", "QuickMath", 500, 5, "15/20");
            CreateTournamentCard(content.transform, "Flash Tap Masters", "FlashTap", 300, 10, "8/16");
            CreateTournamentCard(content.transform, "Cognitive Sprint Elite", "CognitiveSprint", 1000, 25, "5/10");
            CreateTournamentCard(content.transform, "Memory Pairs Daily", "MemoryPairs", 100, 1, "18/20");
        }

        private static GameObject CreateTournamentCard(Transform parent, string name, string game, int prizePool, int entryFee, string players)
        {
            GameObject card = new GameObject("TournamentCard_" + game);
            card.transform.SetParent(parent, false);

            RectTransform rt = card.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 120);

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredHeight = 120;
            le.flexibleWidth = 1;

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = CARD_BG;

            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = CARD_BORDER;
            outline.effectDistance = new Vector2(1, -1);

            // Tournament name
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(card.transform, false);

            RectTransform nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0.5f);
            nameRT.anchorMax = new Vector2(0.6f, 1);
            nameRT.offsetMin = new Vector2(15, 10);
            nameRT.offsetMax = new Vector2(0, -10);

            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = name;
            nameText.fontSize = 24;
            nameText.color = TEXT_GOLD;
            nameText.fontStyle = FontStyles.Bold;
            nameText.alignment = TextAlignmentOptions.Left;

            // Prize Pool
            GameObject prizeObj = new GameObject("PrizePool");
            prizeObj.transform.SetParent(card.transform, false);

            RectTransform prizeRT = prizeObj.AddComponent<RectTransform>();
            prizeRT.anchorMin = new Vector2(0, 0);
            prizeRT.anchorMax = new Vector2(0.4f, 0.5f);
            prizeRT.offsetMin = new Vector2(15, 10);
            prizeRT.offsetMax = new Vector2(0, 0);

            TextMeshProUGUI prizeText = prizeObj.AddComponent<TextMeshProUGUI>();
            prizeText.text = $"Premio: ${prizePool}";
            prizeText.fontSize = 20;
            prizeText.color = SUCCESS_GREEN;
            prizeText.alignment = TextAlignmentOptions.Left;

            // Entry Fee
            GameObject entryObj = new GameObject("EntryFee");
            entryObj.transform.SetParent(card.transform, false);

            RectTransform entryRT = entryObj.AddComponent<RectTransform>();
            entryRT.anchorMin = new Vector2(0.4f, 0.5f);
            entryRT.anchorMax = new Vector2(0.7f, 1);
            entryRT.offsetMin = new Vector2(0, 10);
            entryRT.offsetMax = new Vector2(0, -10);

            TextMeshProUGUI entryText = entryObj.AddComponent<TextMeshProUGUI>();
            entryText.text = $"Entrada: ${entryFee}";
            entryText.fontSize = 20;
            entryText.color = TEXT_SECONDARY;
            entryText.alignment = TextAlignmentOptions.Center;

            // Players
            GameObject playersObj = new GameObject("Players");
            playersObj.transform.SetParent(card.transform, false);

            RectTransform playersRT = playersObj.AddComponent<RectTransform>();
            playersRT.anchorMin = new Vector2(0.4f, 0);
            playersRT.anchorMax = new Vector2(0.7f, 0.5f);
            playersRT.offsetMin = new Vector2(0, 10);
            playersRT.offsetMax = new Vector2(0, 0);

            TextMeshProUGUI playersText = playersObj.AddComponent<TextMeshProUGUI>();
            playersText.text = players;
            playersText.fontSize = 18;
            playersText.color = TEXT_SECONDARY;
            playersText.alignment = TextAlignmentOptions.Center;

            // Join Button
            GameObject joinBtn = new GameObject("JoinButton");
            joinBtn.transform.SetParent(card.transform, false);

            RectTransform joinRT = joinBtn.AddComponent<RectTransform>();
            joinRT.anchorMin = new Vector2(0.72f, 0.15f);
            joinRT.anchorMax = new Vector2(0.98f, 0.85f);
            joinRT.offsetMin = Vector2.zero;
            joinRT.offsetMax = Vector2.zero;

            Image joinBg = joinBtn.AddComponent<Image>();
            joinBg.color = BUTTON_GOLD;

            Button btn = joinBtn.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = BUTTON_GOLD;
            colors.highlightedColor = GOLD_LIGHT;
            colors.pressedColor = GOLD_DARK;
            btn.colors = colors;

            GameObject joinText = new GameObject("Text");
            joinText.transform.SetParent(joinBtn.transform, false);

            RectTransform jtRT = joinText.AddComponent<RectTransform>();
            jtRT.anchorMin = Vector2.zero;
            jtRT.anchorMax = Vector2.one;
            jtRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI jt = joinText.AddComponent<TextMeshProUGUI>();
            jt.text = "Unirse";
            jt.fontSize = 22;
            jt.color = BG_DARK;
            jt.alignment = TextAlignmentOptions.Center;
            jt.fontStyle = FontStyles.Bold;

            return card;
        }
    }
}
