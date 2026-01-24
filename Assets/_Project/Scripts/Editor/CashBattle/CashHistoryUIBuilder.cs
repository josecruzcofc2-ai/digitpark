using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// UI Builder para la escena CashHistory.unity
    /// Construye el historial de partidas y estadísticas del jugador.
    /// </summary>
    public class CashHistoryUIBuilder : EditorWindow
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

        private static readonly Color SUCCESS_GREEN = new Color(0.3f, 1f, 0.5f, 1f);
        private static readonly Color ERROR_RED = new Color(1f, 0.4f, 0.4f, 1f);
        private static readonly Color CYAN_ACCENT = new Color(0f, 0.9f, 1f, 1f);

        #endregion

        [MenuItem("DigitPark/UI Builders/CashBattle/Cash History", false, 253)]
        public static void ShowWindow()
        {
            GetWindow<CashHistoryUIBuilder>("Cash History Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Cash History UI Builder", EditorStyles.boldLabel);
            GUILayout.Label("Historial de partidas y estadísticas", EditorStyles.miniLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Construye la UI para CashHistory.unity:\n\n" +
                "- Header con título\n" +
                "- Stats resumen (partidas, victorias, winrate, ganancias)\n" +
                "- Lista scrolleable de partidas\n" +
                "- Items con resultado, oponente, monto",
                MessageType.Info);

            EditorGUILayout.Space(10);

            GUI.backgroundColor = GOLD_PRIMARY;
            if (GUILayout.Button("CONSTRUIR UI COMPLETA", GUILayout.Height(40)))
            {
                BuildCashHistoryUI();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(15);

            if (GUILayout.Button("Solo Stats Panel", GUILayout.Height(28)))
            {
                BuildStatsPanelOnly();
            }

            if (GUILayout.Button("Solo Match Item Prefab", GUILayout.Height(28)))
            {
                BuildMatchItemPrefab();
            }
        }

        private static void BuildCashHistoryUI()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontró Canvas. Abre la escena CashHistory primero.", "OK");
                return;
            }

            if (EditorUtility.DisplayDialog("Reconstruir UI?",
                "Esto reconstruirá la UI de Cash History.\n\n¿Continuar?",
                "Sí, Construir", "Cancelar"))
            {
                BuildAllElements(canvas);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                Debug.Log("[CashHistoryUIBuilder] UI construida exitosamente!");
            }
        }

        private static void BuildStatsPanelOnly()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            Transform safeArea = canvas.transform.Find("SafeArea");
            if (safeArea == null)
            {
                Debug.LogError("SafeArea no encontrado. Construye la UI completa primero.");
                return;
            }

            Transform old = safeArea.Find("StatsPanel");
            if (old != null) DestroyImmediate(old.gameObject);

            CreateStatsPanel(safeArea);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private static void BuildMatchItemPrefab()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            GameObject prefab = CreateMatchHistoryItem(canvas.transform, "QuickMath", "@Opponent", true, "+$10.00", "Hoy, 14:30", "5-3");
            Selection.activeGameObject = prefab;
            Debug.Log("[CashHistoryUIBuilder] Match Item creado. Guárdalo como prefab.");
        }

        private static void BuildAllElements(Canvas canvas)
        {
            Transform canvasTransform = canvas.transform;

            CleanupOldElements(canvasTransform);

            CreateBackground(canvasTransform);
            GameObject safeArea = CreateSafeArea(canvasTransform);
            CreateHeader(safeArea.transform);
            CreateStatsPanel(safeArea.transform);
            CreateMatchHistoryList(safeArea.transform);
        }

        private static void CleanupOldElements(Transform parent)
        {
            string[] toDestroy = { "Background", "SafeArea", "Header", "StatsPanel", "MatchHistoryList" };
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
            backBtn.AddComponent<Image>().color = new Color(1, 1, 1, 0);

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
            titleRT.sizeDelta = new Vector2(500, 60);

            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "Historial de Partidas";
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

            balance.AddComponent<Image>().color = new Color(0, 0, 0, 0.5f);

            GameObject bt = new GameObject("Text");
            bt.transform.SetParent(balance.transform, false);
            RectTransform btRT = bt.AddComponent<RectTransform>();
            btRT.anchorMin = Vector2.zero;
            btRT.anchorMax = Vector2.one;
            btRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI balanceText = bt.AddComponent<TextMeshProUGUI>();
            balanceText.text = "$ 0.00";
            balanceText.fontSize = 22;
            balanceText.color = SUCCESS_GREEN;
            balanceText.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateStatsPanel(Transform parent)
        {
            GameObject panel = new GameObject("StatsPanel");
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(-40, 140);
            rt.anchoredPosition = new Vector2(0, -130);

            Image bg = panel.AddComponent<Image>();
            bg.color = CARD_BG;

            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = CARD_BORDER;
            outline.effectDistance = new Vector2(1, -1);

            // Horizontal layout for stats
            HorizontalLayoutGroup hlg = panel.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.padding = new RectOffset(20, 20, 15, 15);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            // Stats
            CreateStatItem(panel.transform, "Partidas", "24", CYAN_ACCENT);
            CreateStatItem(panel.transform, "Victorias", "12", SUCCESS_GREEN);
            CreateStatItem(panel.transform, "Win Rate", "67%", TEXT_GOLD);
            CreateStatItem(panel.transform, "Ganancias", "$156.50", SUCCESS_GREEN);
        }

        private static void CreateStatItem(Transform parent, string label, string value, Color valueColor)
        {
            GameObject item = new GameObject("Stat_" + label);
            item.transform.SetParent(parent, false);

            VerticalLayoutGroup vlg = item.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 5;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            // Value
            GameObject valueObj = new GameObject("Value");
            valueObj.transform.SetParent(item.transform, false);

            LayoutElement valueLE = valueObj.AddComponent<LayoutElement>();
            valueLE.preferredHeight = 50;

            TextMeshProUGUI valueText = valueObj.AddComponent<TextMeshProUGUI>();
            valueText.text = value;
            valueText.fontSize = 36;
            valueText.color = valueColor;
            valueText.alignment = TextAlignmentOptions.Center;
            valueText.fontStyle = FontStyles.Bold;

            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(item.transform, false);

            LayoutElement labelLE = labelObj.AddComponent<LayoutElement>();
            labelLE.preferredHeight = 30;

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.fontSize = 18;
            labelText.color = TEXT_SECONDARY;
            labelText.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateMatchHistoryList(Transform parent)
        {
            GameObject scrollView = new GameObject("MatchHistoryList");
            scrollView.transform.SetParent(parent, false);

            RectTransform svRT = scrollView.AddComponent<RectTransform>();
            svRT.anchorMin = Vector2.zero;
            svRT.anchorMax = Vector2.one;
            svRT.offsetMin = new Vector2(20, 20);
            svRT.offsetMax = new Vector2(-20, -280);

            ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;

            scrollView.AddComponent<Image>().color = new Color(0, 0, 0, 0);

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
            vlg.spacing = 8;
            vlg.padding = new RectOffset(5, 5, 5, 5);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = vpRT;
            scroll.content = contentRT;

            // Sample matches
            CreateMatchHistoryItem(content.transform, "QuickMath", "@ProGamer99", true, "+$10.00", "Hoy, 14:30", "5-3");
            CreateMatchHistoryItem(content.transform, "FlashTap", "@SpeedKing", false, "-$5.00", "Hoy, 12:15", "3-5");
            CreateMatchHistoryItem(content.transform, "MemoryPairs", "@MemMaster", true, "+$15.00", "Ayer, 20:45", "6-4");
            CreateMatchHistoryItem(content.transform, "CognitiveSprint", "@BrainStorm", true, "+$20.00", "Ayer, 18:00", "7-2");
            CreateMatchHistoryItem(content.transform, "OddOneOut", "@EagleEye", false, "-$10.00", "Ayer, 15:30", "2-5");
        }

        private static GameObject CreateMatchHistoryItem(Transform parent, string game, string opponent, bool isWin, string amount, string date, string score)
        {
            GameObject item = new GameObject("MatchItem_" + game);
            item.transform.SetParent(parent, false);

            RectTransform rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 90);

            LayoutElement le = item.AddComponent<LayoutElement>();
            le.preferredHeight = 90;
            le.flexibleWidth = 1;

            Image bg = item.AddComponent<Image>();
            bg.color = CARD_BG;

            // Win/Loss indicator bar
            GameObject indicator = new GameObject("Indicator");
            indicator.transform.SetParent(item.transform, false);

            RectTransform indRT = indicator.AddComponent<RectTransform>();
            indRT.anchorMin = new Vector2(0, 0);
            indRT.anchorMax = new Vector2(0, 1);
            indRT.pivot = new Vector2(0, 0.5f);
            indRT.sizeDelta = new Vector2(6, 0);
            indRT.anchoredPosition = Vector2.zero;

            Image indImg = indicator.AddComponent<Image>();
            indImg.color = isWin ? SUCCESS_GREEN : ERROR_RED;

            // Game name
            GameObject gameObj = new GameObject("Game");
            gameObj.transform.SetParent(item.transform, false);

            RectTransform gameRT = gameObj.AddComponent<RectTransform>();
            gameRT.anchorMin = new Vector2(0, 0.5f);
            gameRT.anchorMax = new Vector2(0.4f, 1);
            gameRT.offsetMin = new Vector2(20, 5);
            gameRT.offsetMax = new Vector2(0, -5);

            TextMeshProUGUI gameText = gameObj.AddComponent<TextMeshProUGUI>();
            gameText.text = game;
            gameText.fontSize = 22;
            gameText.color = TEXT_GOLD;
            gameText.fontStyle = FontStyles.Bold;
            gameText.alignment = TextAlignmentOptions.Left;

            // Opponent
            GameObject oppObj = new GameObject("Opponent");
            oppObj.transform.SetParent(item.transform, false);

            RectTransform oppRT = oppObj.AddComponent<RectTransform>();
            oppRT.anchorMin = new Vector2(0, 0);
            oppRT.anchorMax = new Vector2(0.4f, 0.5f);
            oppRT.offsetMin = new Vector2(20, 5);
            oppRT.offsetMax = new Vector2(0, -5);

            TextMeshProUGUI oppText = oppObj.AddComponent<TextMeshProUGUI>();
            oppText.text = $"vs {opponent}";
            oppText.fontSize = 18;
            oppText.color = TEXT_SECONDARY;
            oppText.alignment = TextAlignmentOptions.Left;

            // Result label
            GameObject resultObj = new GameObject("Result");
            resultObj.transform.SetParent(item.transform, false);

            RectTransform resRT = resultObj.AddComponent<RectTransform>();
            resRT.anchorMin = new Vector2(0.7f, 0.5f);
            resRT.anchorMax = new Vector2(1, 1);
            resRT.offsetMin = new Vector2(0, 5);
            resRT.offsetMax = new Vector2(-15, -5);

            TextMeshProUGUI resText = resultObj.AddComponent<TextMeshProUGUI>();
            resText.text = isWin ? "VICTORIA" : "DERROTA";
            resText.fontSize = 16;
            resText.color = isWin ? SUCCESS_GREEN : ERROR_RED;
            resText.alignment = TextAlignmentOptions.Right;
            resText.fontStyle = FontStyles.Bold;

            // Amount
            GameObject amountObj = new GameObject("Amount");
            amountObj.transform.SetParent(item.transform, false);

            RectTransform amtRT = amountObj.AddComponent<RectTransform>();
            amtRT.anchorMin = new Vector2(0.7f, 0);
            amtRT.anchorMax = new Vector2(1, 0.5f);
            amtRT.offsetMin = new Vector2(0, 5);
            amtRT.offsetMax = new Vector2(-15, -5);

            TextMeshProUGUI amtText = amountObj.AddComponent<TextMeshProUGUI>();
            amtText.text = amount;
            amtText.fontSize = 22;
            amtText.color = isWin ? SUCCESS_GREEN : ERROR_RED;
            amtText.alignment = TextAlignmentOptions.Right;
            amtText.fontStyle = FontStyles.Bold;

            // Score and date
            GameObject scoreObj = new GameObject("ScoreDate");
            scoreObj.transform.SetParent(item.transform, false);

            RectTransform scoreRT = scoreObj.AddComponent<RectTransform>();
            scoreRT.anchorMin = new Vector2(0.4f, 0);
            scoreRT.anchorMax = new Vector2(0.7f, 1);
            scoreRT.offsetMin = new Vector2(10, 10);
            scoreRT.offsetMax = new Vector2(-10, -10);

            TextMeshProUGUI scoreText = scoreObj.AddComponent<TextMeshProUGUI>();
            scoreText.text = $"{score}\n<size=14>{date}</size>";
            scoreText.fontSize = 18;
            scoreText.color = TEXT_SECONDARY;
            scoreText.alignment = TextAlignmentOptions.Center;
            scoreText.richText = true;

            return item;
        }
    }
}
