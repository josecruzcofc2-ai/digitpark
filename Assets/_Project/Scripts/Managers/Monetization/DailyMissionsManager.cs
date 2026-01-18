using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using DigitPark.Monetization;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager para la escena de misiones diarias.
    /// Muestra y gestiona misiones diarias, semanales y especiales.
    /// </summary>
    public class DailyMissionsManager : MonoBehaviour
    {
        [Header("UI - Header")]
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI refreshTimerText;
        [SerializeField] private TextMeshProUGUI totalPointsText;

        [Header("UI - Tabs")]
        [SerializeField] private Button dailyTab;
        [SerializeField] private Button weeklyTab;
        [SerializeField] private Button specialTab;

        [Header("UI - Progress Summary")]
        [SerializeField] private Slider dailyProgressBar;
        [SerializeField] private TextMeshProUGUI dailyProgressText;
        [SerializeField] private TextMeshProUGUI bonusRewardText;
        [SerializeField] private Button claimBonusButton;

        [Header("UI - Missions List")]
        [SerializeField] private Transform missionsContainer;
        [SerializeField] private GameObject missionItemPrefab;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private TextMeshProUGUI emptyStateText;

        [Header("UI - Mission Item")]
        [SerializeField] private GameObject missionDetailPanel;
        [SerializeField] private TextMeshProUGUI detailTitleText;
        [SerializeField] private TextMeshProUGUI detailDescriptionText;
        [SerializeField] private Slider detailProgressBar;
        [SerializeField] private TextMeshProUGUI detailProgressText;
        [SerializeField] private TextMeshProUGUI detailRewardText;
        [SerializeField] private Button claimRewardButton;
        [SerializeField] private Button closeDetailButton;

        [Header("UI - Reward Popup")]
        [SerializeField] private GameObject rewardPopup;
        [SerializeField] private TextMeshProUGUI rewardPopupText;
        [SerializeField] private Image rewardPopupIcon;

        [Header("Configuration")]
        [SerializeField] private int dailyMissionsRequired = 3;
        [SerializeField] private int dailyBonusReward = 100;
        [SerializeField] private float refreshCheckInterval = 60f;

        // State
        private MissionTab currentTab = MissionTab.Daily;
        private List<Mission> dailyMissions = new List<Mission>();
        private List<Mission> weeklyMissions = new List<Mission>();
        private List<Mission> specialMissions = new List<Mission>();
        private List<GameObject> spawnedItems = new List<GameObject>();
        private Mission selectedMission;
        private DateTime lastRefreshDate;
        private DateTime weekStartDate;

        public enum MissionTab
        {
            Daily,
            Weekly,
            Special
        }

        private void Start()
        {
            InitializeMissions();
            SetupUI();
            SetupListeners();
            CheckAndResetMissions();
            LoadMissions();

            // Check refresh timer
            InvokeRepeating(nameof(UpdateRefreshTimer), 1f, refreshCheckInterval);
        }

        private void OnDestroy()
        {
            CancelInvoke();
        }

        private void InitializeMissions()
        {
            // Daily Missions
            dailyMissions = new List<Mission>
            {
                new Mission("daily_play_3", "Jugador Activo", "Juega 3 partidas", MissionCategory.Daily, 3, 25, "coins"),
                new Mission("daily_win_1", "Primera Victoria", "Gana 1 partida", MissionCategory.Daily, 1, 50, "coins"),
                new Mission("daily_score_1000", "Cazador de Puntos", "Obtén 1000 puntos totales", MissionCategory.Daily, 1000, 30, "coins"),
                new Mission("daily_complete_minigame", "Explorador", "Completa cualquier minijuego", MissionCategory.Daily, 1, 20, "coins"),
                new Mission("daily_play_memory", "Memoria de Elefante", "Juega 2 partidas de Memory Pairs", MissionCategory.Daily, 2, 35, "coins"),
                new Mission("daily_perfect_round", "Perfeccionista", "Obtén ronda perfecta", MissionCategory.Daily, 1, 75, "gems"),
            };

            // Weekly Missions
            weeklyMissions = new List<Mission>
            {
                new Mission("weekly_play_20", "Maratonista", "Juega 20 partidas esta semana", MissionCategory.Weekly, 20, 200, "coins"),
                new Mission("weekly_win_10", "Campeón Semanal", "Gana 10 partidas", MissionCategory.Weekly, 10, 300, "coins"),
                new Mission("weekly_all_games", "Versátil", "Juega todos los minijuegos", MissionCategory.Weekly, 6, 150, "gems"),
                new Mission("weekly_streak_5", "En Racha", "Mantén racha de 5 victorias", MissionCategory.Weekly, 5, 250, "coins"),
                new Mission("weekly_tournament", "Competidor", "Participa en un torneo", MissionCategory.Weekly, 1, 100, "gems"),
            };

            // Special Missions (event-based, longer term)
            specialMissions = new List<Mission>
            {
                new Mission("special_master", "Gran Maestro", "Alcanza nivel 10", MissionCategory.Special, 10, 500, "gems"),
                new Mission("special_social", "Influencer", "Comparte el juego 5 veces", MissionCategory.Special, 5, 200, "gems"),
                new Mission("special_collector", "Coleccionista", "Desbloquea 10 avatares", MissionCategory.Special, 10, 300, "gems"),
            };

            LoadProgress();
        }

        private void LoadProgress()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            lastRefreshDate = DateTime.Parse(PlayerPrefs.GetString("MissionsLastRefresh", today));

            // Load daily progress
            foreach (var mission in dailyMissions)
            {
                mission.currentProgress = PlayerPrefs.GetInt($"Mission_{mission.id}_progress", 0);
                mission.isCompleted = PlayerPrefs.GetInt($"Mission_{mission.id}_completed", 0) == 1;
                mission.isClaimed = PlayerPrefs.GetInt($"Mission_{mission.id}_claimed", 0) == 1;
            }

            // Load weekly progress
            foreach (var mission in weeklyMissions)
            {
                mission.currentProgress = PlayerPrefs.GetInt($"Mission_{mission.id}_progress", 0);
                mission.isCompleted = PlayerPrefs.GetInt($"Mission_{mission.id}_completed", 0) == 1;
                mission.isClaimed = PlayerPrefs.GetInt($"Mission_{mission.id}_claimed", 0) == 1;
            }

            // Load special progress
            foreach (var mission in specialMissions)
            {
                mission.currentProgress = PlayerPrefs.GetInt($"Mission_{mission.id}_progress", 0);
                mission.isCompleted = PlayerPrefs.GetInt($"Mission_{mission.id}_completed", 0) == 1;
                mission.isClaimed = PlayerPrefs.GetInt($"Mission_{mission.id}_claimed", 0) == 1;
            }
        }

        private void SaveProgress(Mission mission)
        {
            PlayerPrefs.SetInt($"Mission_{mission.id}_progress", mission.currentProgress);
            PlayerPrefs.SetInt($"Mission_{mission.id}_completed", mission.isCompleted ? 1 : 0);
            PlayerPrefs.SetInt($"Mission_{mission.id}_claimed", mission.isClaimed ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void CheckAndResetMissions()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");

            // Check daily reset
            if (lastRefreshDate.Date < DateTime.Now.Date)
            {
                ResetDailyMissions();
                PlayerPrefs.SetString("MissionsLastRefresh", today);
                PlayerPrefs.Save();
            }

            // Check weekly reset (Monday)
            DayOfWeek startOfWeek = DayOfWeek.Monday;
            DateTime thisWeekStart = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek + (int)startOfWeek);
            if (thisWeekStart > DateTime.Now) thisWeekStart = thisWeekStart.AddDays(-7);

            string savedWeekStart = PlayerPrefs.GetString("MissionsWeekStart", "");
            if (string.IsNullOrEmpty(savedWeekStart) || DateTime.Parse(savedWeekStart) < thisWeekStart)
            {
                ResetWeeklyMissions();
                PlayerPrefs.SetString("MissionsWeekStart", thisWeekStart.ToString("yyyy-MM-dd"));
                PlayerPrefs.Save();
            }
        }

        private void ResetDailyMissions()
        {
            foreach (var mission in dailyMissions)
            {
                mission.currentProgress = 0;
                mission.isCompleted = false;
                mission.isClaimed = false;
                SaveProgress(mission);
            }

            // Reset daily bonus
            PlayerPrefs.SetInt("DailyBonusClaimed", 0);
            PlayerPrefs.Save();

            Debug.Log("[DailyMissions] Daily missions reset");
        }

        private void ResetWeeklyMissions()
        {
            foreach (var mission in weeklyMissions)
            {
                mission.currentProgress = 0;
                mission.isCompleted = false;
                mission.isClaimed = false;
                SaveProgress(mission);
            }

            Debug.Log("[DailyMissions] Weekly missions reset");
        }

        private void SetupUI()
        {
            if (missionDetailPanel) missionDetailPanel.SetActive(false);
            if (rewardPopup) rewardPopup.SetActive(false);

            UpdateHeaderStats();
            UpdateTabVisuals();
            UpdateDailyProgress();
        }

        private void SetupListeners()
        {
            if (backButton) backButton.onClick.AddListener(OnBackClicked);
            if (closeDetailButton) closeDetailButton.onClick.AddListener(CloseDetail);
            if (claimRewardButton) claimRewardButton.onClick.AddListener(ClaimMissionReward);
            if (claimBonusButton) claimBonusButton.onClick.AddListener(ClaimDailyBonus);

            // Tabs
            if (dailyTab) dailyTab.onClick.AddListener(() => SwitchTab(MissionTab.Daily));
            if (weeklyTab) weeklyTab.onClick.AddListener(() => SwitchTab(MissionTab.Weekly));
            if (specialTab) specialTab.onClick.AddListener(() => SwitchTab(MissionTab.Special));
        }

        private void UpdateHeaderStats()
        {
            int totalPoints = 0;

            foreach (var mission in dailyMissions)
            {
                if (mission.isClaimed) totalPoints += mission.rewardAmount;
            }
            foreach (var mission in weeklyMissions)
            {
                if (mission.isClaimed) totalPoints += mission.rewardAmount;
            }
            foreach (var mission in specialMissions)
            {
                if (mission.isClaimed) totalPoints += mission.rewardAmount;
            }

            if (totalPointsText) totalPointsText.text = $"{totalPoints} pts ganados";
        }

        private void UpdateRefreshTimer()
        {
            // Calculate time until midnight (daily reset)
            DateTime tomorrow = DateTime.Now.Date.AddDays(1);
            TimeSpan timeUntilReset = tomorrow - DateTime.Now;

            if (refreshTimerText)
            {
                refreshTimerText.text = $"Reinicio en: {timeUntilReset.Hours:D2}:{timeUntilReset.Minutes:D2}:{timeUntilReset.Seconds:D2}";
            }

            // Check if we need to reset
            if (DateTime.Now.Date > lastRefreshDate.Date)
            {
                CheckAndResetMissions();
                LoadMissions();
            }
        }

        private void SwitchTab(MissionTab tab)
        {
            currentTab = tab;
            UpdateTabVisuals();
            LoadMissions();
        }

        private void UpdateTabVisuals()
        {
            Color activeColor = new Color(0f, 0.83f, 1f);
            Color inactiveColor = new Color(0.3f, 0.3f, 0.3f);

            UpdateTabButton(dailyTab, currentTab == MissionTab.Daily, activeColor, inactiveColor);
            UpdateTabButton(weeklyTab, currentTab == MissionTab.Weekly, activeColor, inactiveColor);
            UpdateTabButton(specialTab, currentTab == MissionTab.Special, activeColor, inactiveColor);
        }

        private void UpdateTabButton(Button button, bool isActive, Color activeColor, Color inactiveColor)
        {
            if (button == null) return;
            var image = button.GetComponent<Image>();
            if (image) image.color = isActive ? activeColor : inactiveColor;
        }

        private void UpdateDailyProgress()
        {
            int completedDaily = 0;
            foreach (var mission in dailyMissions)
            {
                if (mission.isCompleted) completedDaily++;
            }

            if (dailyProgressBar)
            {
                dailyProgressBar.maxValue = dailyMissionsRequired;
                dailyProgressBar.value = Mathf.Min(completedDaily, dailyMissionsRequired);
            }

            if (dailyProgressText)
            {
                dailyProgressText.text = $"{completedDaily}/{dailyMissionsRequired} misiones completadas";
            }

            if (bonusRewardText)
            {
                bonusRewardText.text = $"Bonus: +{dailyBonusReward} monedas";
            }

            bool canClaimBonus = completedDaily >= dailyMissionsRequired &&
                                PlayerPrefs.GetInt("DailyBonusClaimed", 0) == 0;

            if (claimBonusButton)
            {
                claimBonusButton.gameObject.SetActive(canClaimBonus);
            }
        }

        private void LoadMissions()
        {
            ClearItems();

            var missions = GetCurrentMissions();

            if (missions.Count == 0)
            {
                if (emptyStateText)
                {
                    emptyStateText.gameObject.SetActive(true);
                    emptyStateText.text = "No hay misiones disponibles";
                }
                return;
            }

            if (emptyStateText) emptyStateText.gameObject.SetActive(false);

            // Sort: unclaimed completed first, then in progress, then not started
            missions.Sort((a, b) =>
            {
                if (a.isCompleted && !a.isClaimed && (!b.isCompleted || b.isClaimed)) return -1;
                if (b.isCompleted && !b.isClaimed && (!a.isCompleted || a.isClaimed)) return 1;
                if (!a.isClaimed && b.isClaimed) return -1;
                if (a.isClaimed && !b.isClaimed) return 1;
                return b.currentProgress.CompareTo(a.currentProgress);
            });

            foreach (var mission in missions)
            {
                CreateMissionItem(mission);
            }
        }

        private List<Mission> GetCurrentMissions()
        {
            return currentTab switch
            {
                MissionTab.Daily => new List<Mission>(dailyMissions),
                MissionTab.Weekly => new List<Mission>(weeklyMissions),
                MissionTab.Special => new List<Mission>(specialMissions),
                _ => new List<Mission>()
            };
        }

        private void ClearItems()
        {
            foreach (var item in spawnedItems)
            {
                if (item) Destroy(item);
            }
            spawnedItems.Clear();
        }

        private void CreateMissionItem(Mission mission)
        {
            GameObject item;

            if (missionItemPrefab != null)
            {
                item = Instantiate(missionItemPrefab, missionsContainer);
            }
            else
            {
                item = CreateMissionItemFallback(mission);
            }

            spawnedItems.Add(item);

            // Setup click
            var button = item.GetComponent<Button>() ?? item.AddComponent<Button>();
            var m = mission;
            button.onClick.AddListener(() => ShowDetail(m));
        }

        private GameObject CreateMissionItemFallback(Mission mission)
        {
            var item = new GameObject($"Mission_{mission.id}");
            item.transform.SetParent(missionsContainer, false);

            var rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(350, 90);

            var image = item.AddComponent<Image>();

            // Color based on state
            if (mission.isClaimed)
                image.color = new Color(0.1f, 0.1f, 0.15f, 0.7f);
            else if (mission.isCompleted)
                image.color = new Color(0.1f, 0.25f, 0.1f, 0.95f);
            else
                image.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

            // Title
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(item.transform, false);
            var titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0.6f);
            titleRT.anchorMax = new Vector2(0.7f, 1);
            titleRT.offsetMin = new Vector2(15, 0);
            titleRT.offsetMax = new Vector2(0, -10);

            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = mission.title;
            titleText.fontSize = 16;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = mission.isClaimed ? new Color(0.5f, 0.5f, 0.5f) : Color.white;

            // Description
            var descObj = new GameObject("Description");
            descObj.transform.SetParent(item.transform, false);
            var descRT = descObj.AddComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0, 0.3f);
            descRT.anchorMax = new Vector2(0.7f, 0.6f);
            descRT.offsetMin = new Vector2(15, 0);
            descRT.offsetMax = new Vector2(0, 0);

            var descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = mission.description;
            descText.fontSize = 12;
            descText.color = new Color(0.7f, 0.7f, 0.7f);

            // Progress
            var progressObj = new GameObject("Progress");
            progressObj.transform.SetParent(item.transform, false);
            var progressRT = progressObj.AddComponent<RectTransform>();
            progressRT.anchorMin = new Vector2(0, 0);
            progressRT.anchorMax = new Vector2(0.7f, 0.3f);
            progressRT.offsetMin = new Vector2(15, 5);
            progressRT.offsetMax = new Vector2(0, 0);

            var progressText = progressObj.AddComponent<TextMeshProUGUI>();
            progressText.text = mission.isClaimed ? "✓ Completada" :
                               mission.isCompleted ? "¡Lista para reclamar!" :
                               $"{mission.currentProgress}/{mission.targetProgress}";
            progressText.fontSize = 12;
            progressText.color = mission.isCompleted ? new Color(0f, 1f, 0.5f) : new Color(0.6f, 0.6f, 0.6f);

            // Reward
            var rewardObj = new GameObject("Reward");
            rewardObj.transform.SetParent(item.transform, false);
            var rewardRT = rewardObj.AddComponent<RectTransform>();
            rewardRT.anchorMin = new Vector2(0.7f, 0);
            rewardRT.anchorMax = new Vector2(1, 1);
            rewardRT.offsetMin = new Vector2(5, 10);
            rewardRT.offsetMax = new Vector2(-10, -10);

            var rewardText = rewardObj.AddComponent<TextMeshProUGUI>();
            string rewardIcon = mission.rewardType == "gems" ? "💎" : "🪙";
            rewardText.text = $"{rewardIcon}\n+{mission.rewardAmount}";
            rewardText.fontSize = 14;
            rewardText.alignment = TextAlignmentOptions.Center;
            rewardText.color = mission.rewardType == "gems" ? new Color(0.5f, 0.8f, 1f) : new Color(1f, 0.84f, 0f);

            return item;
        }

        private void ShowDetail(Mission mission)
        {
            selectedMission = mission;

            if (missionDetailPanel) missionDetailPanel.SetActive(true);

            if (detailTitleText) detailTitleText.text = mission.title;
            if (detailDescriptionText) detailDescriptionText.text = mission.description;

            if (detailProgressBar)
            {
                detailProgressBar.maxValue = mission.targetProgress;
                detailProgressBar.value = mission.currentProgress;
            }

            if (detailProgressText)
            {
                detailProgressText.text = $"{mission.currentProgress}/{mission.targetProgress}";
            }

            if (detailRewardText)
            {
                string rewardName = mission.rewardType == "gems" ? "gemas" : "monedas";
                detailRewardText.text = $"+{mission.rewardAmount} {rewardName}";
            }

            if (claimRewardButton)
            {
                claimRewardButton.gameObject.SetActive(mission.isCompleted && !mission.isClaimed);
            }
        }

        private void CloseDetail()
        {
            if (missionDetailPanel) missionDetailPanel.SetActive(false);
            selectedMission = null;
        }

        private void ClaimMissionReward()
        {
            if (selectedMission == null || selectedMission.isClaimed) return;

            selectedMission.isClaimed = true;
            SaveProgress(selectedMission);

            // Apply reward
            ApplyReward(selectedMission.rewardType, selectedMission.rewardAmount);

            // Show popup
            ShowRewardPopup(selectedMission.rewardType, selectedMission.rewardAmount);

            // Update UI
            if (claimRewardButton) claimRewardButton.gameObject.SetActive(false);
            UpdateHeaderStats();
            UpdateDailyProgress();
            LoadMissions();

            Debug.Log($"[DailyMissions] Claimed reward for: {selectedMission.title}");
        }

        private void ClaimDailyBonus()
        {
            if (PlayerPrefs.GetInt("DailyBonusClaimed", 0) == 1) return;

            PlayerPrefs.SetInt("DailyBonusClaimed", 1);
            PlayerPrefs.Save();

            ApplyReward("coins", dailyBonusReward);
            ShowRewardPopup("coins", dailyBonusReward);

            UpdateDailyProgress();

            Debug.Log($"[DailyMissions] Claimed daily bonus: {dailyBonusReward} coins");
        }

        private void ApplyReward(string type, int amount)
        {
            switch (type)
            {
                case "coins":
                    int currentCoins = PlayerPrefs.GetInt("PlayerCoins", 0);
                    PlayerPrefs.SetInt("PlayerCoins", currentCoins + amount);
                    break;

                case "gems":
                    int currentGems = PlayerPrefs.GetInt("PlayerGems", 0);
                    PlayerPrefs.SetInt("PlayerGems", currentGems + amount);
                    break;
            }

            PlayerPrefs.Save();
        }

        private void ShowRewardPopup(string type, int amount)
        {
            if (rewardPopup)
            {
                rewardPopup.SetActive(true);
                string rewardName = type == "gems" ? "gemas" : "monedas";
                if (rewardPopupText) rewardPopupText.text = $"+{amount} {rewardName}";

                Invoke(nameof(HideRewardPopup), 2f);
            }
        }

        private void HideRewardPopup()
        {
            if (rewardPopup) rewardPopup.SetActive(false);
        }

        /// <summary>
        /// Update mission progress (call from game logic)
        /// </summary>
        public void UpdateMissionProgress(string missionId, int progress)
        {
            Mission mission = FindMission(missionId);
            if (mission == null || mission.isCompleted) return;

            mission.currentProgress = Mathf.Min(progress, mission.targetProgress);

            if (mission.currentProgress >= mission.targetProgress)
            {
                mission.isCompleted = true;
                Debug.Log($"[DailyMissions] Mission completed: {mission.title}");
            }

            SaveProgress(mission);
            UpdateHeaderStats();
            UpdateDailyProgress();
        }

        /// <summary>
        /// Increment mission progress by amount
        /// </summary>
        public void IncrementMissionProgress(string missionId, int amount = 1)
        {
            Mission mission = FindMission(missionId);
            if (mission == null || mission.isCompleted) return;

            UpdateMissionProgress(missionId, mission.currentProgress + amount);
        }

        private Mission FindMission(string id)
        {
            foreach (var m in dailyMissions) if (m.id == id) return m;
            foreach (var m in weeklyMissions) if (m.id == id) return m;
            foreach (var m in specialMissions) if (m.id == id) return m;
            return null;
        }

        private void OnBackClicked()
        {
            SceneNavigator.Instance?.GoBack();
        }
    }

    [Serializable]
    public class Mission
    {
        public string id;
        public string title;
        public string description;
        public MissionCategory category;
        public int targetProgress;
        public int currentProgress;
        public int rewardAmount;
        public string rewardType;
        public bool isCompleted;
        public bool isClaimed;

        public Mission(string id, string title, string description, MissionCategory category, int target, int reward, string rewardType)
        {
            this.id = id;
            this.title = title;
            this.description = description;
            this.category = category;
            this.targetProgress = target;
            this.rewardAmount = reward;
            this.rewardType = rewardType;
        }
    }

    public enum MissionCategory
    {
        Daily,
        Weekly,
        Special
    }
}
