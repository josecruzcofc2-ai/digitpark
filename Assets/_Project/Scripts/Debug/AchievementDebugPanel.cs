using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DigitPark.Managers;
using DigitPark.Services;
using DigitPark.UI;

namespace DigitPark.DevTools
{
    /// <summary>
    /// Runtime debug panel for testing achievements.
    /// Add this to any scene to test unlocking achievements with notifications.
    /// Press F12 to toggle the panel visibility.
    /// Uses AchievementService as the single source of truth.
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

        private void Awake()
        {
            if (transform.parent != null)
                transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
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

        private void CreateFloatingToggle()
        {
            GameObject toggleRoot = new GameObject("FloatingToggle");
            toggleRoot.transform.SetParent(transform);

            Canvas toggleCanvas = toggleRoot.AddComponent<Canvas>();
            toggleCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            toggleCanvas.sortingOrder = 10001;

            CanvasScaler scaler = toggleRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            toggleRoot.AddComponent<GraphicRaycaster>();

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

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(floatingToggleBtn.transform, false);
            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "DBG";
            tmp.fontSize = FontSizes.DebugText;
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
                if (text != null) { text.text = "X"; text.color = new Color(1f, 0.3f, 0.3f); }
                if (bg != null) bg.color = new Color(0.3f, 0.08f, 0.08f, 0.9f);
            }
            else
            {
                if (text != null) { text.text = "DBG"; text.color = new Color(1f, 0.84f, 0f); }
                if (bg != null) bg.color = new Color(0.1f, 0.1f, 0.15f, 0.85f);
            }
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
            titleText.fontSize = FontSizes.Body;
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
            statsText.text = "0/0 (0%)";
            statsText.fontSize = FontSizes.DebugText;
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

            // Build items from Service if available, or wait
            PopulateItems();

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
            instrText.fontSize = FontSizes.DebugText;
            instrText.color = new Color(0.5f, 0.5f, 0.5f);
            instrText.alignment = TextAlignmentOptions.Center;
        }

        /// <summary>
        /// Populates items from AchievementService (single source of truth)
        /// </summary>
        private void PopulateItems()
        {
            var service = AchievementService.Instance;
            if (service == null || service.AllAchievements.Count == 0)
            {
                // Service not ready yet - try again later
                Invoke(nameof(PopulateItems), 1f);
                return;
            }

            // Clear existing items
            foreach (var item in debugItems)
            {
                if (item.root != null) Destroy(item.root);
            }
            debugItems.Clear();

            // Build from Service
            string currentCategory = "";
            foreach (var achievement in service.AllAchievements)
            {
                string cat = achievement.category.ToString();
                if (cat != currentCategory)
                {
                    currentCategory = cat;
                    CreateCategoryHeader(currentCategory);
                }
                CreateAchievementItem(achievement);
            }

            UpdateStats();
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
            text.fontSize = FontSizes.DebugText;
            text.fontStyle = FontStyles.Bold;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
        }

        private void CreateAchievementItem(AchievementData data)
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
            checkText.fontSize = FontSizes.Body;
            checkText.color = new Color(0.2f, 0.8f, 0.4f);
            checkText.alignment = TextAlignmentOptions.Center;

            // Title - use localized title from Service
            string displayTitle = data.titleKey;
            if (Localization.LocalizationManager.Instance != null)
                displayTitle = Localization.LocalizationManager.Instance.GetText(data.titleKey);

            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(item.transform, false);
            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.offsetMin = new Vector2(45, 0);
            titleRT.offsetMax = new Vector2(-50, 0);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            string secretTag = data.isHidden ? " <color=#AA33FF>[S]</color>" : "";
            titleText.text = $"{displayTitle}{secretTag}";
            titleText.fontSize = FontSizes.DebugText;
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
            pointsText.fontSize = FontSizes.DebugText;
            pointsText.color = new Color(1f, 0.84f, 0f);
            pointsText.alignment = TextAlignmentOptions.MidlineRight;

            // Store reference
            AchievementDebugItem debugItem = new AchievementDebugItem
            {
                achievementId = data.id,
                root = item,
                background = bg,
                checkText = checkText,
                button = toggleButton
            };
            debugItems.Add(debugItem);

            // Button click - use Service API
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
            tmp.fontSize = FontSizes.DebugText;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            return btn;
        }

        /// <summary>
        /// Toggle achievement via AchievementService (proper unlock/lock)
        /// </summary>
        private void ToggleAchievement(AchievementDebugItem item)
        {
            var service = AchievementService.Instance;
            if (service == null)
            {
                Debug.LogWarning("[DebugPanel] AchievementService not available!");
                return;
            }

            bool isCompleted = service.IsUnlocked(item.achievementId);

            if (isCompleted)
            {
                // Can't "un-unlock" through Service - reset all instead
                Debug.Log($"[DebugPanel] {item.achievementId} is already unlocked. Use Reset to clear all.");
                return;
            }
            else
            {
                // Unlock through Service - this triggers toast automatically via events
                service.UnlockAchievement(item.achievementId);

                item.checkText.text = "V";
                item.background.color = new Color(0.15f, 0.35f, 0.2f, 0.9f);

                Debug.Log($"[DebugPanel] Unlocked via Service: {item.achievementId}");
            }

            UpdateStats();
        }

        private void RefreshStates()
        {
            var service = AchievementService.Instance;
            if (service == null)
            {
                // Service not ready - repopulate when available
                PopulateItems();
                return;
            }

            foreach (var item in debugItems)
            {
                bool isCompleted = service.IsUnlocked(item.achievementId);
                item.checkText.text = isCompleted ? "V" : "";
                item.background.color = isCompleted
                    ? new Color(0.15f, 0.35f, 0.2f, 0.9f)
                    : new Color(0.15f, 0.15f, 0.2f, 0.8f);
            }
            UpdateStats();
        }

        private void UpdateStats()
        {
            var service = AchievementService.Instance;
            if (service == null || statsText == null) return;

            int unlocked = 0;
            int total = service.AllAchievements.Count;
            foreach (var ach in service.AllAchievements)
            {
                if (service.IsUnlocked(ach.id))
                    unlocked++;
            }
            int percent = total > 0 ? (unlocked * 100 / total) : 0;
            statsText.text = $"{unlocked}/{total} ({percent}%)";
        }

        private void UnlockRandom()
        {
            var service = AchievementService.Instance;
            if (service == null) return;

            var locked = new List<AchievementData>();
            foreach (var ach in service.AllAchievements)
            {
                if (!service.IsUnlocked(ach.id))
                    locked.Add(ach);
            }

            if (locked.Count > 0)
            {
                var random = locked[Random.Range(0, locked.Count)];
                service.UnlockAchievement(random.id);
                RefreshStates();
            }
        }

        private void ResetAll()
        {
            var service = AchievementService.Instance;
            if (service != null)
            {
                service.ResetAllAchievements();
            }
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

        // Helper class
        private class AchievementDebugItem
        {
            public string achievementId;
            public GameObject root;
            public Image background;
            public TextMeshProUGUI checkText;
            public Button button;
        }
    }
}
