using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DigitPark.Managers;

namespace DigitPark.DevTools
{
    /// <summary>
    /// Runtime debug panel for testing achievements.
    /// Add this to any scene to test unlocking achievements with notifications.
    /// Press F12 to toggle the panel visibility.
    /// </summary>
    public class AchievementDebugPanel : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private KeyCode toggleKey = KeyCode.F12;
        [SerializeField] private bool startHidden = true;

        [Header("Auto-Created UI")]
        private GameObject panelRoot;
        private GameObject floatingToggleBtn;
        private ScrollRect scrollRect;
        private Transform contentParent;
        private TextMeshProUGUI statsText;

        private List<AchievementDebugItem> debugItems = new List<AchievementDebugItem>();
        private bool isVisible;

        // Achievement definitions
        private List<DebugAchievementData> achievements;

        private void Awake()
        {
            if (transform.parent != null)
                transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            InitializeAchievements();
            CreateUI();
            CreateFloatingToggle();

            if (startHidden)
            {
                panelRoot.SetActive(false);
                isVisible = false;
            }
            else
            {
                isVisible = true;
            }

            UpdateFloatingToggle();
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                TogglePanel();
            }
        }

        private void TogglePanel()
        {
            isVisible = !isVisible;
            panelRoot.SetActive(isVisible);
            UpdateFloatingToggle();

            if (isVisible)
            {
                RefreshStates();
            }
        }

        /// <summary>
        /// Creates a small floating button always visible to open/close the panel.
        /// </summary>
        private void CreateFloatingToggle()
        {
            // Canvas independiente para el botón flotante
            GameObject toggleRoot = new GameObject("FloatingToggle");
            toggleRoot.transform.SetParent(transform);

            Canvas toggleCanvas = toggleRoot.AddComponent<Canvas>();
            toggleCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            toggleCanvas.sortingOrder = 10001; // Por encima del panel

            CanvasScaler scaler = toggleRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            toggleRoot.AddComponent<GraphicRaycaster>();

            // Botón flotante
            floatingToggleBtn = new GameObject("ToggleBtn");
            floatingToggleBtn.transform.SetParent(toggleRoot.transform, false);

            RectTransform btnRT = floatingToggleBtn.AddComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0, 0.5f);
            btnRT.anchorMax = new Vector2(0, 0.5f);
            btnRT.pivot = new Vector2(0, 0.5f);
            btnRT.sizeDelta = new Vector2(50, 100);
            btnRT.anchoredPosition = new Vector2(0, 0);

            Image btnBg = floatingToggleBtn.AddComponent<Image>();
            btnBg.color = new Color(0.1f, 0.1f, 0.15f, 0.85f);

            Button btn = floatingToggleBtn.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            btn.colors = colors;
            btn.onClick.AddListener(TogglePanel);

            // Texto del botón
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(floatingToggleBtn.transform, false);
            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "DBG";
            tmp.fontSize = 14;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color(1f, 0.84f, 0f);
            tmp.alignment = TextAlignmentOptions.Center;
        }

        private void UpdateFloatingToggle()
        {
            if (floatingToggleBtn == null) return;

            var text = floatingToggleBtn.GetComponentInChildren<TextMeshProUGUI>();
            var bg = floatingToggleBtn.GetComponent<Image>();

            if (isVisible)
            {
                // Panel abierto: botón dice "X" en rojo
                if (text != null) { text.text = "X"; text.color = new Color(1f, 0.3f, 0.3f); }
                if (bg != null) bg.color = new Color(0.3f, 0.08f, 0.08f, 0.9f);
            }
            else
            {
                // Panel cerrado: botón dice "DBG" en dorado
                if (text != null) { text.text = "DBG"; text.color = new Color(1f, 0.84f, 0f); }
                if (bg != null) bg.color = new Color(0.1f, 0.1f, 0.15f, 0.85f);
            }
        }

        private void InitializeAchievements()
        {
            achievements = new List<DebugAchievementData>
            {
                // BEGINNER
                new DebugAchievementData("first_game", "Primer Paso", 10, "Beginner", "Logro_Primeros_Pasos"),
                new DebugAchievementData("tutorial_complete", "Aprendiz", 10, "Beginner", "Logro_Graduado"),
                new DebugAchievementData("first_win", "Primera Victoria", 15, "Beginner", "Logro_Primera_Victoria"),
                new DebugAchievementData("profile_complete", "Identidad", 10, "Beginner", "Logro_Perfil_Completo"),

                // MASTERY
                new DebugAchievementData("digitrush_master", "Maestro de Dígitos", 50, "Mastery", "Logro_Maestro_Numeros"),
                new DebugAchievementData("flashtap_master", "Reflejos de Luz", 50, "Mastery", "Logro_Reflejos_Rayo"),
                new DebugAchievementData("memorypairs_master", "Memoria Fotográfica", 50, "Mastery", "Logro_Genio"),
                new DebugAchievementData("quickmath_master", "Calculadora Humana", 50, "Mastery", "Logro_Maestro_Matematicas"),
                new DebugAchievementData("oddoneout_master", "Ojo de Águila", 50, "Mastery", "Logro_Ojo_Aguila"),

                // VICTORIES
                new DebugAchievementData("wins_10", "Competidor", 20, "Victories", "Logro_10_Victorias"),
                new DebugAchievementData("wins_50", "Veterano", 40, "Victories", "Logro_50_Victorias"),
                new DebugAchievementData("wins_100", "Centurión", 60, "Victories", "Logro_Centurion"),
                new DebugAchievementData("wins_500", "Leyenda", 100, "Victories", "Logro_500_Victorias"),
                new DebugAchievementData("wins_1000", "Inmortal", 200, "Victories", "Logro_1000_Victorias"),

                // STREAKS
                new DebugAchievementData("streak_3", "En Racha", 25, "Streaks", "Logro_Racha_Fuego"),
                new DebugAchievementData("streak_5", "Imparable", 40, "Streaks", "Logro_Victoria_Racha_7"),
                new DebugAchievementData("streak_10", "Dominación", 75, "Streaks", "Logro_Demoledor"),
                new DebugAchievementData("streak_20", "Invencible", 150, "Streaks", "Logro_Victoria_Racha_30", true),

                // CASH BATTLE
                new DebugAchievementData("cash_first", "Apostador", 25, "CashBattle", "Logro_Ficha_Cash"),
                new DebugAchievementData("cash_first_win", "Ganador Real", 35, "CashBattle", "Logro_Rey_Monedas"),
                new DebugAchievementData("cash_10_wins", "Jugador Serio", 50, "CashBattle", "Logro_VIP_1000"),
                new DebugAchievementData("cash_50_wins", "High Roller", 100, "CashBattle", "Logro_VIP_Dados"),
                new DebugAchievementData("cash_100_wins", "Tiburón", 200, "CashBattle", "Logro_Tiburon_Cash"),
                new DebugAchievementData("cash_earnings_100", "Primeros $100", 75, "CashBattle", "Logro_Bolsa_100"),
                new DebugAchievementData("cash_earnings_1000", "Club de los Mil", 250, "CashBattle", "Logro_Millonario", true),

                // TOURNAMENTS
                new DebugAchievementData("tournament_first", "Participante", 20, "Tournaments", "Logro_Torneo_Bracket"),
                new DebugAchievementData("tournament_top3", "Podio", 50, "Tournaments", "Logro_Coleccion_Trofeos"),
                new DebugAchievementData("tournament_win", "Campeón", 100, "Tournaments", "Logro_Campeon_1"),
                new DebugAchievementData("tournament_5_wins", "Multicampeón", 200, "Tournaments", "Logro_4_Estrellas"),
                new DebugAchievementData("tournament_create", "Organizador", 30, "Tournaments", "Logro_Organizador_Torneo"),

                // SOCIAL
                new DebugAchievementData("friend_first", "Primer Amigo", 15, "Social", "Logro_Primer_Rival"),
                new DebugAchievementData("friends_10", "Popular", 30, "Social", "Logro_Social_10_Amigos"),
                new DebugAchievementData("friends_50", "Influencer", 75, "Social", "Logro_Influencer"),
                new DebugAchievementData("challenge_friend", "Retador", 20, "Social", "Logro_Versus"),
                new DebugAchievementData("beat_friend", "Rival", 25, "Social", "Logro_Amigo_Rival"),

                // PROGRESSION
                new DebugAchievementData("level_10", "Nivel 10", 25, "Progression", "Logro_Nivel_10"),
                new DebugAchievementData("level_25", "Nivel 25", 50, "Progression", "Logro_Nivel_25"),
                new DebugAchievementData("level_50", "Nivel 50", 75, "Progression", "Logro_Nivel50"),
                new DebugAchievementData("level_100", "Nivel 100", 150, "Progression", "Logro_Avance_Epico"),

                // COLLECTOR - Reservado para V2

                // TIME
                new DebugAchievementData("days_7", "Una Semana", 25, "Time", "Logro_Racha_7_Dias"),
                new DebugAchievementData("days_30", "Un Mes", 50, "Time", "Logro_Racha_30_Dias"),
                new DebugAchievementData("days_100", "100 Días", 100, "Time", "Logro_Racha_100_Dias"),
                new DebugAchievementData("days_365", "Un Año", 300, "Time", "Logro_Racha_365_Dias", true),
                new DebugAchievementData("daily_streak_7", "Racha Semanal", 30, "Time", "Logro_Login_Semanal"),
                new DebugAchievementData("daily_streak_30", "Racha Mensual", 75, "Time", "Logro_Login_Mensual"),

                // SECRET
                new DebugAchievementData("night_owl", "Búho Nocturno", 50, "Secret", "Logro_Buho_Nocturno", true),
                new DebugAchievementData("perfect_game", "Perfección", 100, "Secret", "Logro_Perfeccionista", true),
                new DebugAchievementData("comeback_king", "Rey del Comeback", 75, "Secret", "Logro_Ave_Fenix", true),
                new DebugAchievementData("speed_demon", "Demonio de Velocidad", 100, "Secret", "Logro_Demonio_Velocidad", true),
            };
        }

        private void CreateUI()
        {
            // Create canvas
            panelRoot = new GameObject("AchievementDebugPanel");
            panelRoot.transform.SetParent(transform);

            Canvas canvas = panelRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10000;

            CanvasScaler scaler = panelRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            panelRoot.AddComponent<GraphicRaycaster>();

            // Main panel
            GameObject mainPanel = CreatePanel(panelRoot.transform, "MainPanel");
            RectTransform mainRT = mainPanel.GetComponent<RectTransform>();
            mainRT.anchorMin = new Vector2(0, 0);
            mainRT.anchorMax = new Vector2(0.4f, 1);
            mainRT.offsetMin = new Vector2(10, 10);
            mainRT.offsetMax = new Vector2(-10, -10);

            Image mainBg = mainPanel.GetComponent<Image>();
            mainBg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

            // Header
            GameObject header = CreatePanel(mainPanel.transform, "Header");
            RectTransform headerRT = header.GetComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0, 1);
            headerRT.anchorMax = new Vector2(1, 1);
            headerRT.pivot = new Vector2(0.5f, 1);
            headerRT.sizeDelta = new Vector2(0, 80);
            headerRT.anchoredPosition = Vector2.zero;

            Image headerBg = header.GetComponent<Image>();
            headerBg.color = new Color(0.05f, 0.05f, 0.1f, 1f);

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(header.transform, false);
            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0.5f);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Achievement Tester";
            titleText.fontSize = 24;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = new Color(1f, 0.84f, 0f);
            titleText.alignment = TextAlignmentOptions.Center;

            // Stats
            GameObject statsObj = new GameObject("Stats");
            statsObj.transform.SetParent(header.transform, false);
            RectTransform statsRT = statsObj.AddComponent<RectTransform>();
            statsRT.anchorMin = new Vector2(0, 0);
            statsRT.anchorMax = new Vector2(1, 0.5f);
            statsRT.sizeDelta = Vector2.zero;

            statsText = statsObj.AddComponent<TextMeshProUGUI>();
            statsText.text = "0/53 (0%)";
            statsText.fontSize = 16;
            statsText.color = new Color(0f, 1f, 1f);
            statsText.alignment = TextAlignmentOptions.Center;

            // Close button
            GameObject closeBtn = CreateButton(header.transform, "CloseBtn", "X");
            RectTransform closeRT = closeBtn.GetComponent<RectTransform>();
            closeRT.anchorMin = new Vector2(1, 1);
            closeRT.anchorMax = new Vector2(1, 1);
            closeRT.pivot = new Vector2(1, 1);
            closeRT.sizeDelta = new Vector2(40, 40);
            closeRT.anchoredPosition = new Vector2(-5, -5);

            closeBtn.GetComponent<Button>().onClick.AddListener(() => TogglePanel());

            // Scroll view
            GameObject scrollView = new GameObject("ScrollView");
            scrollView.transform.SetParent(mainPanel.transform, false);
            RectTransform scrollRT = scrollView.AddComponent<RectTransform>();
            scrollRT.anchorMin = Vector2.zero;
            scrollRT.anchorMax = Vector2.one;
            scrollRT.offsetMin = new Vector2(5, 60);
            scrollRT.offsetMax = new Vector2(-5, -85);

            scrollRect = scrollView.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;

            // Viewport
            GameObject viewport = CreatePanel(scrollView.transform, "Viewport");
            RectTransform viewportRT = viewport.GetComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.sizeDelta = Vector2.zero;
            viewport.AddComponent<RectMask2D>();
            viewport.GetComponent<Image>().color = Color.clear;
            scrollRect.viewport = viewportRT;

            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRT = content.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.sizeDelta = new Vector2(0, 0);
            scrollRect.content = contentRT;

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 2;
            vlg.padding = new RectOffset(5, 5, 5, 5);
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            contentParent = content.transform;

            // Create achievement items
            string currentCategory = "";
            foreach (var achievement in achievements)
            {
                if (achievement.category != currentCategory)
                {
                    currentCategory = achievement.category;
                    CreateCategoryHeader(currentCategory);
                }
                CreateAchievementItem(achievement);
            }

            // Bottom buttons
            GameObject bottomPanel = CreatePanel(mainPanel.transform, "BottomPanel");
            RectTransform bottomRT = bottomPanel.GetComponent<RectTransform>();
            bottomRT.anchorMin = new Vector2(0, 0);
            bottomRT.anchorMax = new Vector2(1, 0);
            bottomRT.pivot = new Vector2(0.5f, 0);
            bottomRT.sizeDelta = new Vector2(0, 55);
            bottomRT.anchoredPosition = Vector2.zero;

            HorizontalLayoutGroup hlg = bottomPanel.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 5;
            hlg.padding = new RectOffset(5, 5, 5, 5);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;

            bottomPanel.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.1f, 1f);

            // Random button
            GameObject randomBtn = CreateButton(bottomPanel.transform, "RandomBtn", "Random");
            randomBtn.GetComponent<Button>().onClick.AddListener(UnlockRandom);

            // Reset button
            GameObject resetBtn = CreateButton(bottomPanel.transform, "ResetBtn", "Reset");
            resetBtn.GetComponent<Button>().onClick.AddListener(ResetAll);
            resetBtn.GetComponent<Image>().color = new Color(0.6f, 0.2f, 0.2f);

            // Instruction text
            GameObject instructionObj = new GameObject("Instruction");
            instructionObj.transform.SetParent(mainPanel.transform, false);
            RectTransform instrRT = instructionObj.AddComponent<RectTransform>();
            instrRT.anchorMin = new Vector2(0, 1);
            instrRT.anchorMax = new Vector2(1, 1);
            instrRT.pivot = new Vector2(0.5f, 1);
            instrRT.sizeDelta = new Vector2(0, 20);
            instrRT.anchoredPosition = new Vector2(0, -80);

            TextMeshProUGUI instrText = instructionObj.AddComponent<TextMeshProUGUI>();
            instrText.text = $"Presiona {toggleKey} para ocultar";
            instrText.fontSize = 12;
            instrText.color = new Color(0.5f, 0.5f, 0.5f);
            instrText.alignment = TextAlignmentOptions.Center;
        }

        private void CreateCategoryHeader(string category)
        {
            GameObject header = new GameObject($"Header_{category}");
            header.transform.SetParent(contentParent, false);

            RectTransform rt = header.AddComponent<RectTransform>();
            LayoutElement le = header.AddComponent<LayoutElement>();
            le.minHeight = 30;
            le.preferredHeight = 30;

            Image bg = header.AddComponent<Image>();
            bg.color = GetCategoryColor(category);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(header.transform, false);
            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = $"-- {category.ToUpper()} --";
            text.fontSize = 14;
            text.fontStyle = FontStyles.Bold;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
        }

        private void CreateAchievementItem(DebugAchievementData data)
        {
            GameObject item = new GameObject($"Item_{data.id}");
            item.transform.SetParent(contentParent, false);

            RectTransform rt = item.AddComponent<RectTransform>();
            LayoutElement le = item.AddComponent<LayoutElement>();
            le.minHeight = 45;
            le.preferredHeight = 45;

            Image bg = item.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.2f, 0.8f);

            // Toggle button
            GameObject toggleBtn = new GameObject("Toggle");
            toggleBtn.transform.SetParent(item.transform, false);
            RectTransform toggleRT = toggleBtn.AddComponent<RectTransform>();
            toggleRT.anchorMin = new Vector2(0, 0.5f);
            toggleRT.anchorMax = new Vector2(0, 0.5f);
            toggleRT.pivot = new Vector2(0, 0.5f);
            toggleRT.sizeDelta = new Vector2(35, 35);
            toggleRT.anchoredPosition = new Vector2(5, 0);

            Image toggleBg = toggleBtn.AddComponent<Image>();
            toggleBg.color = new Color(0.2f, 0.2f, 0.25f);

            Button toggleButton = toggleBtn.AddComponent<Button>();

            GameObject checkObj = new GameObject("Check");
            checkObj.transform.SetParent(toggleBtn.transform, false);
            RectTransform checkRT = checkObj.AddComponent<RectTransform>();
            checkRT.anchorMin = Vector2.zero;
            checkRT.anchorMax = Vector2.one;
            checkRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI checkText = checkObj.AddComponent<TextMeshProUGUI>();
            checkText.text = "";
            checkText.fontSize = 24;
            checkText.color = new Color(0.2f, 0.8f, 0.4f);
            checkText.alignment = TextAlignmentOptions.Center;

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(item.transform, false);
            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.offsetMin = new Vector2(45, 0);
            titleRT.offsetMax = new Vector2(-50, 0);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            string secretTag = data.isSecret ? " <color=#AA33FF>[S]</color>" : "";
            titleText.text = $"{data.title}{secretTag}";
            titleText.fontSize = 14;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;

            // Points
            GameObject pointsObj = new GameObject("Points");
            pointsObj.transform.SetParent(item.transform, false);
            RectTransform pointsRT = pointsObj.AddComponent<RectTransform>();
            pointsRT.anchorMin = new Vector2(1, 0);
            pointsRT.anchorMax = new Vector2(1, 1);
            pointsRT.pivot = new Vector2(1, 0.5f);
            pointsRT.sizeDelta = new Vector2(45, 0);
            pointsRT.anchoredPosition = new Vector2(-5, 0);

            TextMeshProUGUI pointsText = pointsObj.AddComponent<TextMeshProUGUI>();
            pointsText.text = $"{data.points}";
            pointsText.fontSize = 12;
            pointsText.color = new Color(1f, 0.84f, 0f);
            pointsText.alignment = TextAlignmentOptions.MidlineRight;

            // Store reference
            AchievementDebugItem debugItem = new AchievementDebugItem
            {
                data = data,
                background = bg,
                checkText = checkText,
                button = toggleButton
            };
            debugItems.Add(debugItem);

            // Button click
            toggleButton.onClick.AddListener(() => ToggleAchievement(debugItem));
        }

        private GameObject CreatePanel(Transform parent, string name)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            panel.AddComponent<RectTransform>();
            panel.AddComponent<Image>();
            return panel;
        }

        private GameObject CreateButton(Transform parent, string name, string text)
        {
            GameObject btn = new GameObject(name);
            btn.transform.SetParent(parent, false);
            RectTransform rt = btn.AddComponent<RectTransform>();

            Image bg = btn.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.5f, 0.7f);

            Button button = btn.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
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
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            return btn;
        }

        private void ToggleAchievement(AchievementDebugItem item)
        {
            bool isCompleted = PlayerPrefs.GetInt($"Achievement_{item.data.id}_completed", 0) == 1;

            if (isCompleted)
            {
                // Lock it
                PlayerPrefs.SetInt($"Achievement_{item.data.id}_completed", 0);
                PlayerPrefs.SetInt($"Achievement_{item.data.id}_progress", 0);
                PlayerPrefs.Save();

                item.checkText.text = "";
                item.background.color = new Color(0.15f, 0.15f, 0.2f, 0.8f);
            }
            else
            {
                // Unlock it
                PlayerPrefs.SetInt($"Achievement_{item.data.id}_completed", 1);
                PlayerPrefs.SetInt($"Achievement_{item.data.id}_progress", 100);
                PlayerPrefs.Save();

                item.checkText.text = "V";
                item.background.color = new Color(0.15f, 0.35f, 0.2f, 0.9f);

                // Trigger notification
                TriggerNotification(item.data);
            }

            UpdateStats();
        }

        private void TriggerNotification(DebugAchievementData data)
        {
            if (AchievementNotificationManager.Instance != null)
            {
                Sprite icon = Resources.Load<Sprite>($"Icons/Achievements/{data.iconName}");
                AchievementNotificationManager.Instance.ShowNotification(
                    data.title,
                    $"+{data.points} pts",
                    data.points,
                    icon,
                    data.isSecret
                );
                UnityEngine.Debug.Log($"[DebugPanel] Achievement unlocked: {data.title}");
            }
            else
            {
                UnityEngine.Debug.LogWarning("[DebugPanel] AchievementNotificationManager not found!");
            }
        }

        private void RefreshStates()
        {
            foreach (var item in debugItems)
            {
                bool isCompleted = PlayerPrefs.GetInt($"Achievement_{item.data.id}_completed", 0) == 1;
                item.checkText.text = isCompleted ? "V" : "";
                item.background.color = isCompleted
                    ? new Color(0.15f, 0.35f, 0.2f, 0.9f)
                    : new Color(0.15f, 0.15f, 0.2f, 0.8f);
            }
            UpdateStats();
        }

        private void UpdateStats()
        {
            int unlocked = 0;
            foreach (var item in debugItems)
            {
                if (PlayerPrefs.GetInt($"Achievement_{item.data.id}_completed", 0) == 1)
                    unlocked++;
            }
            int total = debugItems.Count;
            int percent = total > 0 ? (unlocked * 100 / total) : 0;
            statsText.text = $"{unlocked}/{total} ({percent}%)";
        }

        private void UnlockRandom()
        {
            var locked = debugItems.FindAll(i => PlayerPrefs.GetInt($"Achievement_{i.data.id}_completed", 0) == 0);
            if (locked.Count > 0)
            {
                var random = locked[Random.Range(0, locked.Count)];
                ToggleAchievement(random);
            }
        }

        private void ResetAll()
        {
            foreach (var item in debugItems)
            {
                PlayerPrefs.DeleteKey($"Achievement_{item.data.id}_completed");
                PlayerPrefs.DeleteKey($"Achievement_{item.data.id}_progress");
                PlayerPrefs.DeleteKey($"Achievement_{item.data.id}_claimed");
            }
            PlayerPrefs.Save();
            RefreshStates();
        }

        private Color GetCategoryColor(string category)
        {
            return category switch
            {
                "Beginner" => new Color(0.2f, 0.5f, 0.7f, 0.8f),
                "Mastery" => new Color(0.3f, 0.6f, 0.3f, 0.8f),
                "Victories" => new Color(0.2f, 0.6f, 0.3f, 0.8f),
                "Streaks" => new Color(0.7f, 0.4f, 0.2f, 0.8f),
                "CashBattle" => new Color(0.3f, 0.6f, 0.2f, 0.8f),
                "Tournaments" => new Color(0.7f, 0.5f, 0.2f, 0.8f),
                "Social" => new Color(0.3f, 0.4f, 0.7f, 0.8f),
                "Progression" => new Color(0.5f, 0.4f, 0.7f, 0.8f),
                "Collector" => new Color(0.7f, 0.5f, 0.3f, 0.8f),
                "Time" => new Color(0.3f, 0.6f, 0.6f, 0.8f),
                "Secret" => new Color(0.6f, 0.2f, 0.7f, 0.8f),
                _ => Color.gray
            };
        }

        // Helper classes
        private class AchievementDebugItem
        {
            public DebugAchievementData data;
            public Image background;
            public TextMeshProUGUI checkText;
            public Button button;
        }

        private class DebugAchievementData
        {
            public string id;
            public string title;
            public int points;
            public string category;
            public string iconName;
            public bool isSecret;

            public DebugAchievementData(string id, string title, int points, string category, string iconName, bool isSecret = false)
            {
                this.id = id;
                this.title = title;
                this.points = points;
                this.category = category;
                this.iconName = iconName;
                this.isSecret = isSecret;
            }
        }
    }
}
