using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using DigitPark.Monetization;
using DigitPark.Localization;
using DigitPark.Services.Firebase;
using DigitPark.UI;

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

        [Header("Reward Icons")]
        [SerializeField] private Sprite coinIcon;
        [SerializeField] private Sprite gemIcon;

        [Header("Configuration")]
        [SerializeField] private int dailyMissionsRequired = 3;
        [SerializeField] private int dailyBonusReward = 100;
        [SerializeField] private float refreshCheckInterval = 60f;

        // Neon theme colors
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CARD_BG = new Color(0.06f, 0.08f, 0.12f, 0.95f);
        private static readonly Color CARD_BG_COMPLETED = new Color(0.08f, 0.2f, 0.1f, 0.95f);
        private static readonly Color CARD_BG_CLAIMED = new Color(0.08f, 0.08f, 0.1f, 0.6f);
        private static readonly Color PURPLE_WEEKLY = new Color(0.6f, 0.2f, 1f, 1f);
        private static readonly Color GOLD_SPECIAL = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GREEN_SUCCESS = new Color(0.2f, 0.9f, 0.4f, 1f);
        private static readonly Color COIN_COLOR = new Color(1f, 0.85f, 0.3f, 1f);
        private static readonly Color GEM_COLOR = new Color(0.4f, 0.8f, 1f, 1f);
        private static readonly Color SEPARATOR_BG = new Color(0.04f, 0.05f, 0.08f, 1f);

        // State
        private MissionTab currentTab = MissionTab.Daily;
        private List<Mission> dailyMissions = new List<Mission>();
        private List<Mission> weeklyMissions = new List<Mission>();
        private List<Mission> specialMissions = new List<Mission>();
        private List<GameObject> spawnedItems = new List<GameObject>();
        private Mission selectedMission;
        private DateTime lastRefreshDate;
        private DateTime weekStartDate;

        // Neon icon sprites
        private Sprite coinIconNeon;
        private Sprite gemIconNeon;

        public enum MissionTab
        {
            Daily,
            Weekly,
            Special
        }

        private void Start()
        {
            LoadNeonIcons();
            InitializeMissions();
            SetupUI();
            SetupListeners();
            CheckAndResetMissions();
            LoadMissions();

            // Check refresh timer
            InvokeRepeating(nameof(UpdateRefreshTimer), 1f, refreshCheckInterval);

            // Scroll fade indicator
            SetupScrollFade();

            // Analytics: screen view
            AnalyticsService.Instance?.LogScreenView("DailyMissions");
        }

        private void SetupScrollFade()
        {
            if (scrollRect != null && scrollRect.GetComponent<ScrollFadeIndicator>() == null)
            {
                scrollRect.gameObject.AddComponent<ScrollFadeIndicator>();
            }
        }

        private void OnDestroy()
        {
            CancelInvoke();
        }

        private void LoadNeonIcons()
        {
            coinIconNeon = Resources.Load<Sprite>("Icons/CoinIconNeon");
            gemIconNeon = Resources.Load<Sprite>("Icons/GemIconNeon");
        }

        /// <summary>
        /// Helper de localizacion
        /// </summary>
        private string L(string key, params object[] args)
        {
            if (LocalizationManager.Instance == null) return key;
            return args.Length > 0
                ? LocalizationManager.Instance.GetText(key, args)
                : LocalizationManager.Instance.GetText(key);
        }

        private string GetRewardTypeName(string type)
        {
            return type switch
            {
                "coins" => L("reward_coins"),
                "gems" => L("reward_gems"),
                _ => type
            };
        }

        private Color GetRewardTypeColor(string type)
        {
            return type switch
            {
                "coins" => COIN_COLOR,
                "gems" => GEM_COLOR,
                _ => Color.white
            };
        }

        private Sprite GetRewardTypeIcon(string type)
        {
            return type switch
            {
                "coins" => coinIconNeon != null ? coinIconNeon : coinIcon,
                "gems" => gemIconNeon != null ? gemIconNeon : gemIcon,
                _ => coinIcon
            };
        }

        private void InitializeMissions()
        {
            // Daily Missions - usando claves de localizacion
            dailyMissions = new List<Mission>
            {
                new Mission("daily_play_3", "ms_daily_play_3_title", "ms_daily_play_3_desc", MissionCategory.Daily, 3, 25, "coins"),
                new Mission("daily_win_1", "ms_daily_win_1_title", "ms_daily_win_1_desc", MissionCategory.Daily, 1, 50, "coins"),
                new Mission("daily_score_1000", "ms_daily_score_1000_title", "ms_daily_score_1000_desc", MissionCategory.Daily, 1000, 30, "coins"),
                new Mission("daily_complete_minigame", "ms_daily_complete_minigame_title", "ms_daily_complete_minigame_desc", MissionCategory.Daily, 1, 20, "coins"),
                new Mission("daily_play_memory", "ms_daily_play_memory_title", "ms_daily_play_memory_desc", MissionCategory.Daily, 2, 35, "coins"),
                new Mission("daily_perfect_round", "ms_daily_perfect_round_title", "ms_daily_perfect_round_desc", MissionCategory.Daily, 1, 75, "gems"),
            };

            // Weekly Missions
            weeklyMissions = new List<Mission>
            {
                new Mission("weekly_play_20", "ms_weekly_play_20_title", "ms_weekly_play_20_desc", MissionCategory.Weekly, 20, 200, "coins"),
                new Mission("weekly_win_10", "ms_weekly_win_10_title", "ms_weekly_win_10_desc", MissionCategory.Weekly, 10, 300, "coins"),
                new Mission("weekly_all_games", "ms_weekly_all_games_title", "ms_weekly_all_games_desc", MissionCategory.Weekly, 6, 150, "gems"),
                new Mission("weekly_streak_5", "ms_weekly_streak_5_title", "ms_weekly_streak_5_desc", MissionCategory.Weekly, 5, 250, "coins"),
                new Mission("weekly_tournament", "ms_weekly_tournament_title", "ms_weekly_tournament_desc", MissionCategory.Weekly, 1, 100, "gems"),
            };

            // Special Missions
            specialMissions = new List<Mission>
            {
                new Mission("special_master", "ms_special_master_title", "ms_special_master_desc", MissionCategory.Special, 10, 500, "gems"),
                new Mission("special_social", "ms_special_social_title", "ms_special_social_desc", MissionCategory.Special, 5, 200, "gems"),
                new Mission("special_collector", "ms_special_collector_title", "ms_special_collector_desc", MissionCategory.Special, 10, 300, "gems"),
            };

            LoadProgress();
        }

        private void LoadProgress()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            lastRefreshDate = DateTime.Parse(PlayerPrefs.GetString("MissionsLastRefresh", today));

            foreach (var mission in dailyMissions)
            {
                mission.currentProgress = PlayerPrefs.GetInt($"Mission_{mission.id}_progress", 0);
                mission.isCompleted = PlayerPrefs.GetInt($"Mission_{mission.id}_completed", 0) == 1;
                mission.isClaimed = PlayerPrefs.GetInt($"Mission_{mission.id}_claimed", 0) == 1;
            }

            foreach (var mission in weeklyMissions)
            {
                mission.currentProgress = PlayerPrefs.GetInt($"Mission_{mission.id}_progress", 0);
                mission.isCompleted = PlayerPrefs.GetInt($"Mission_{mission.id}_completed", 0) == 1;
                mission.isClaimed = PlayerPrefs.GetInt($"Mission_{mission.id}_claimed", 0) == 1;
            }

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

            if (lastRefreshDate.Date < DateTime.Now.Date)
            {
                ResetDailyMissions();
                PlayerPrefs.SetString("MissionsLastRefresh", today);
                PlayerPrefs.Save();
            }

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

            if (dailyTab) dailyTab.onClick.AddListener(() => SwitchTab(MissionTab.Daily));
            if (weeklyTab) weeklyTab.onClick.AddListener(() => SwitchTab(MissionTab.Weekly));
            if (specialTab) specialTab.onClick.AddListener(() => SwitchTab(MissionTab.Special));
        }

        private void UpdateHeaderStats()
        {
            int totalPoints = 0;

            foreach (var mission in dailyMissions)
                if (mission.isClaimed) totalPoints += mission.rewardAmount;
            foreach (var mission in weeklyMissions)
                if (mission.isClaimed) totalPoints += mission.rewardAmount;
            foreach (var mission in specialMissions)
                if (mission.isClaimed) totalPoints += mission.rewardAmount;

            if (totalPointsText) totalPointsText.text = L("ms_points_earned", totalPoints);
        }

        private void UpdateRefreshTimer()
        {
            DateTime tomorrow = DateTime.Now.Date.AddDays(1);
            TimeSpan timeUntilReset = tomorrow - DateTime.Now;

            if (refreshTimerText)
            {
                refreshTimerText.text = L("ms_refresh_in", UIPolish.FormatTimerHHMMSS(timeUntilReset.Hours, timeUntilReset.Minutes, timeUntilReset.Seconds));
            }

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
            Color activeColor = CYAN_NEON;
            Color inactiveColor = new Color(0.2f, 0.2f, 0.25f);

            // Weekly tab usa purple cuando esta activo
            Color weeklyActive = currentTab == MissionTab.Weekly ? PURPLE_WEEKLY : inactiveColor;
            Color specialActive = currentTab == MissionTab.Special ? GOLD_SPECIAL : inactiveColor;

            UpdateTabButton(dailyTab, currentTab == MissionTab.Daily, activeColor, inactiveColor);
            UpdateTabButton(weeklyTab, currentTab == MissionTab.Weekly, weeklyActive, inactiveColor);
            UpdateTabButton(specialTab, currentTab == MissionTab.Special, specialActive, inactiveColor);
        }

        private void UpdateTabButton(Button button, bool isActive, Color activeColor, Color inactiveColor)
        {
            if (button == null) return;
            var image = button.GetComponent<Image>();
            if (image) image.color = isActive ? activeColor : inactiveColor;

            var text = button.GetComponentInChildren<TextMeshProUGUI>();
            if (text) text.color = isActive ? Color.white : new Color(0.5f, 0.5f, 0.5f);
        }

        private void UpdateDailyProgress()
        {
            int completedDaily = 0;
            foreach (var mission in dailyMissions)
                if (mission.isCompleted) completedDaily++;

            if (dailyProgressBar)
            {
                dailyProgressBar.maxValue = dailyMissionsRequired;
                dailyProgressBar.value = Mathf.Min(completedDaily, dailyMissionsRequired);
            }

            if (dailyProgressText)
            {
                dailyProgressText.text = L("ms_progress", completedDaily, dailyMissionsRequired);
            }

            if (bonusRewardText)
            {
                bonusRewardText.text = L("ms_bonus", dailyBonusReward);
            }

            bool canClaimBonus = completedDaily >= dailyMissionsRequired &&
                                PlayerPrefs.GetInt("DailyBonusClaimed", 0) == 0;

            if (claimBonusButton)
            {
                claimBonusButton.gameObject.SetActive(canClaimBonus);
            }

            // Actualizar estado visual de milestone markers
            UpdateProgressMilestoneMarkers(completedDaily);
        }

        private void UpdateProgressMilestoneMarkers(int completedCount)
        {
            if (dailyProgressBar == null) return;
            var barTransform = dailyProgressBar.transform.parent;
            if (barTransform == null) barTransform = dailyProgressBar.transform;

            // Buscar markers existentes (creados por UIBuilder: Marker3, Marker5, MarkerAll)
            string[] markerNames = { "Marker3", "Marker5", "MarkerAll" };
            int[] markerThresholds = { 3, 5, dailyMissions.Count };

            for (int i = 0; i < markerNames.Length; i++)
            {
                var marker = barTransform.Find(markerNames[i]);
                if (marker == null) continue;

                bool reached = completedCount >= markerThresholds[i];
                var img = marker.GetComponent<Image>();
                if (img != null)
                {
                    bool isBonus = i == markerNames.Length - 1;
                    Color reachedColor = isBonus
                        ? new Color(1f, 0.84f, 0f, 1f)   // GOLD
                        : CYAN_NEON;
                    Color unreachedColor = isBonus
                        ? new Color(0.3f, 0.25f, 0f, 1f)  // Dark gold
                        : new Color(0f, 0.4f, 0.5f, 1f);  // Dark cyan
                    img.color = reached ? reachedColor : unreachedColor;
                }

                var outline = marker.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.effectColor = reached
                        ? new Color(1f, 1f, 1f, 0.5f)
                        : new Color(0.3f, 0.3f, 0.3f, 0.5f);
                }
            }
        }

        private GameObject _emptyStatePanel;

        private void ShowEmptyState()
        {
            if (_emptyStatePanel != null)
            {
                _emptyStatePanel.SetActive(true);
                return;
            }

            if (emptyStateText) emptyStateText.gameObject.SetActive(false);

            _emptyStatePanel = new GameObject("EmptyState");
            _emptyStatePanel.transform.SetParent(missionsContainer, false);

            var rt = _emptyStatePanel.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(350, 250);

            var vlg = _emptyStatePanel.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 15;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.padding = new RectOffset(30, 30, 40, 30);

            // Icono
            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(_emptyStatePanel.transform, false);
            var iconLE = iconObj.AddComponent<LayoutElement>();
            iconLE.preferredWidth = 64;
            iconLE.preferredHeight = 64;
            var iconImg = iconObj.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.color = new Color(0.3f, 0.3f, 0.35f);

            Sprite lockedSprite = Resources.Load<Sprite>("Icons/MissionLockedIcon");
            if (lockedSprite != null)
            {
                iconImg.sprite = lockedSprite;
                iconImg.color = Color.white;
            }

            // Titulo
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(_emptyStatePanel.transform, false);
            var titleLE = titleObj.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 28;
            var titleText = titleObj.AddComponent<TMPro.TextMeshProUGUI>();
            titleText.text = L("ms_no_missions");
            titleText.fontSize = 18;
            titleText.fontStyle = TMPro.FontStyles.Bold;
            titleText.color = new Color(0.6f, 0.6f, 0.65f);
            titleText.alignment = TMPro.TextAlignmentOptions.Center;

            // Subtitulo
            var subObj = new GameObject("Subtitle");
            subObj.transform.SetParent(_emptyStatePanel.transform, false);
            var subLE = subObj.AddComponent<LayoutElement>();
            subLE.preferredHeight = 22;
            var subText = subObj.AddComponent<TMPro.TextMeshProUGUI>();
            subText.text = L("ms_refresh_in", UIPolish.FormatTimerHHMMSS(0, 0, 0));
            subText.fontSize = 13;
            subText.color = new Color(0.4f, 0.4f, 0.45f);
            subText.alignment = TMPro.TextAlignmentOptions.Center;

            spawnedItems.Add(_emptyStatePanel);
        }

        private void HideEmptyState()
        {
            if (_emptyStatePanel != null)
            {
                _emptyStatePanel.SetActive(false);
            }
        }

        private void LoadMissions()
        {
            ClearItems();

            var missions = GetCurrentMissions();

            if (missions.Count == 0)
            {
                ShowEmptyState();
                return;
            }

            HideEmptyState();
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

            var button = item.GetComponent<Button>() ?? item.AddComponent<Button>();
            var m = mission;
            button.onClick.AddListener(() => ShowDetail(m));
        }

        /// <summary>
        /// Crea un separador visual de seccion (Daily/Weekly/Special)
        /// </summary>
        private void CreateSectionHeader(string locKey, Color accentColor)
        {
            var header = new GameObject("SectionHeader");
            header.transform.SetParent(missionsContainer, false);

            var rt = header.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(350, 40);

            var bg = header.AddComponent<Image>();
            UIPolish.ApplyRoundedCorners(bg, 8);
            bg.color = SEPARATOR_BG;

            // Linea de acento izquierda
            var lineObj = new GameObject("AccentLine");
            lineObj.transform.SetParent(header.transform, false);
            var lineRT = lineObj.AddComponent<RectTransform>();
            lineRT.anchorMin = new Vector2(0, 0.2f);
            lineRT.anchorMax = new Vector2(0.02f, 0.8f);
            lineRT.offsetMin = Vector2.zero;
            lineRT.offsetMax = Vector2.zero;
            var lineImage = lineObj.AddComponent<Image>();
            lineImage.color = accentColor;

            // Texto
            var textObj = new GameObject("HeaderText");
            textObj.transform.SetParent(header.transform, false);
            var textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0.04f, 0);
            textRT.anchorMax = new Vector2(1, 1);
            textRT.offsetMin = new Vector2(5, 0);
            textRT.offsetMax = new Vector2(-10, 0);

            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = L(locKey);
            text.fontSize = 14;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Left;
            text.color = accentColor;

            spawnedItems.Add(header);
        }

        private GameObject CreateMissionItemFallback(Mission mission)
        {
            var item = new GameObject($"Mission_{mission.id}");
            item.transform.SetParent(missionsContainer, false);

            var rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(350, 110);

            var image = item.AddComponent<Image>();
            UIPolish.ApplyRoundedCorners(image);

            // Color basado en estado y categoria
            if (mission.isClaimed)
            {
                image.color = CARD_BG_CLAIMED;
            }
            else if (mission.isCompleted)
            {
                image.color = CARD_BG_COMPLETED;
            }
            else
            {
                // Tint sutil por categoria: weekly=purpura, special=dorado
                image.color = mission.category switch
                {
                    MissionCategory.Weekly => new Color(
                        CARD_BG.r + PURPLE_WEEKLY.r * 0.025f,
                        CARD_BG.g + PURPLE_WEEKLY.g * 0.01f,
                        CARD_BG.b + PURPLE_WEEKLY.b * 0.04f,
                        CARD_BG.a),
                    MissionCategory.Special => new Color(
                        CARD_BG.r + GOLD_SPECIAL.r * 0.03f,
                        CARD_BG.g + GOLD_SPECIAL.g * 0.025f,
                        CARD_BG.b + GOLD_SPECIAL.b * 0.005f,
                        CARD_BG.a),
                    _ => CARD_BG
                };
            }

            // Borde de categoria
            Color categoryColor = mission.category switch
            {
                MissionCategory.Weekly => PURPLE_WEEKLY,
                MissionCategory.Special => GOLD_SPECIAL,
                _ => CYAN_NEON
            };

            if (!mission.isClaimed)
            {
                var borderObj = new GameObject("CategoryBorder");
                borderObj.transform.SetParent(item.transform, false);
                var borderRT = borderObj.AddComponent<RectTransform>();
                borderRT.anchorMin = new Vector2(0, 0);
                borderRT.anchorMax = new Vector2(0.008f, 1);
                borderRT.offsetMin = Vector2.zero;
                borderRT.offsetMax = Vector2.zero;
                var borderImage = borderObj.AddComponent<Image>();
                borderImage.color = categoryColor;
            }

            // Titulo (localizado)
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(item.transform, false);
            var titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.03f, 0.65f);
            titleRT.anchorMax = new Vector2(0.7f, 1);
            titleRT.offsetMin = new Vector2(10, 0);
            titleRT.offsetMax = new Vector2(0, -8);

            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = L(mission.title);
            titleText.fontSize = 15;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = mission.isClaimed ? new Color(0.45f, 0.45f, 0.45f) : Color.white;

            // Descripcion (localizada)
            var descObj = new GameObject("Description");
            descObj.transform.SetParent(item.transform, false);
            var descRT = descObj.AddComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0.03f, 0.38f);
            descRT.anchorMax = new Vector2(0.7f, 0.65f);
            descRT.offsetMin = new Vector2(10, 0);
            descRT.offsetMax = new Vector2(0, 0);

            var descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = L(mission.description);
            descText.fontSize = 11;
            descText.color = new Color(0.6f, 0.6f, 0.65f);

            // Barra de progreso visual
            var progressBarBg = new GameObject("ProgressBarBg");
            progressBarBg.transform.SetParent(item.transform, false);
            var progressBgRT = progressBarBg.AddComponent<RectTransform>();
            progressBgRT.anchorMin = new Vector2(0.03f, 0.12f);
            progressBgRT.anchorMax = new Vector2(0.55f, 0.22f);
            progressBgRT.offsetMin = new Vector2(10, 0);
            progressBgRT.offsetMax = new Vector2(0, 0);
            var progressBgImage = progressBarBg.AddComponent<Image>();
            progressBgImage.color = new Color(0.15f, 0.15f, 0.2f);

            // Barra de progreso fill
            var progressBarFill = new GameObject("ProgressBarFill");
            progressBarFill.transform.SetParent(progressBarBg.transform, false);
            var progressFillRT = progressBarFill.AddComponent<RectTransform>();
            float fillAmount = mission.targetProgress > 0
                ? Mathf.Clamp01((float)mission.currentProgress / mission.targetProgress)
                : 0f;
            progressFillRT.anchorMin = Vector2.zero;
            progressFillRT.anchorMax = new Vector2(fillAmount, 1f);
            progressFillRT.offsetMin = Vector2.zero;
            progressFillRT.offsetMax = Vector2.zero;
            var progressFillImage = progressBarFill.AddComponent<Image>();
            progressFillImage.color = mission.isCompleted ? GREEN_SUCCESS : categoryColor;

            // Texto de progreso numerico sobre la barra
            var progressTextObj = new GameObject("ProgressText");
            progressTextObj.transform.SetParent(item.transform, false);
            var progressTextRT = progressTextObj.AddComponent<RectTransform>();
            progressTextRT.anchorMin = new Vector2(0.56f, 0.08f);
            progressTextRT.anchorMax = new Vector2(0.7f, 0.25f);
            progressTextRT.offsetMin = Vector2.zero;
            progressTextRT.offsetMax = Vector2.zero;

            var progressText = progressTextObj.AddComponent<TextMeshProUGUI>();
            if (mission.isClaimed)
            {
                progressText.text = "✓ " + L("ms_completed");
                progressText.color = GREEN_SUCCESS;
            }
            else if (mission.isCompleted)
            {
                progressText.text = L("ms_ready_claim");
                progressText.color = GREEN_SUCCESS;
            }
            else
            {
                progressText.text = $"{mission.currentProgress}/{mission.targetProgress}";
                progressText.color = new Color(0.55f, 0.55f, 0.6f);
            }
            progressText.fontSize = 10;
            progressText.alignment = TextAlignmentOptions.Left;

            // === REWARD SECTION (icono + cantidad) ===
            // Icono de currency
            var rewardIconObj = new GameObject("RewardIcon");
            rewardIconObj.transform.SetParent(item.transform, false);
            var rewardIconRT = rewardIconObj.AddComponent<RectTransform>();
            rewardIconRT.anchorMin = new Vector2(0.73f, 0.45f);
            rewardIconRT.anchorMax = new Vector2(0.85f, 0.85f);
            rewardIconRT.offsetMin = Vector2.zero;
            rewardIconRT.offsetMax = Vector2.zero;

            var rewardIconImage = rewardIconObj.AddComponent<Image>();
            rewardIconImage.sprite = GetRewardTypeIcon(mission.rewardType);
            rewardIconImage.preserveAspect = true;
            if (mission.isClaimed) rewardIconImage.color = new Color(1f, 1f, 1f, 0.35f);

            // Cantidad de reward
            var rewardAmountObj = new GameObject("RewardAmount");
            rewardAmountObj.transform.SetParent(item.transform, false);
            var rewardAmountRT = rewardAmountObj.AddComponent<RectTransform>();
            rewardAmountRT.anchorMin = new Vector2(0.73f, 0.15f);
            rewardAmountRT.anchorMax = new Vector2(0.98f, 0.48f);
            rewardAmountRT.offsetMin = Vector2.zero;
            rewardAmountRT.offsetMax = Vector2.zero;

            var rewardAmountText = rewardAmountObj.AddComponent<TextMeshProUGUI>();
            rewardAmountText.text = $"+{mission.rewardAmount}";
            rewardAmountText.fontSize = 14;
            rewardAmountText.fontStyle = FontStyles.Bold;
            rewardAmountText.alignment = TextAlignmentOptions.Center;
            rewardAmountText.color = mission.isClaimed
                ? new Color(0.4f, 0.4f, 0.4f)
                : GetRewardTypeColor(mission.rewardType);

            // Boton "Reclamar" inline para misiones completadas no reclamadas
            if (mission.isCompleted && !mission.isClaimed)
            {
                var claimBtnObj = new GameObject("ClaimBtn");
                claimBtnObj.transform.SetParent(item.transform, false);
                var claimBtnRT = claimBtnObj.AddComponent<RectTransform>();
                claimBtnRT.anchorMin = new Vector2(0.78f, 0.18f);
                claimBtnRT.anchorMax = new Vector2(0.98f, 0.82f);
                claimBtnRT.offsetMin = Vector2.zero;
                claimBtnRT.offsetMax = Vector2.zero;

                var claimBtnImage = claimBtnObj.AddComponent<Image>();
                UIPolish.ApplyRoundedCorners(claimBtnImage, 8);
                claimBtnImage.color = GREEN_SUCCESS;

                var claimBtnTextObj = new GameObject("Text");
                claimBtnTextObj.transform.SetParent(claimBtnObj.transform, false);
                var claimBtnTextRT = claimBtnTextObj.AddComponent<RectTransform>();
                claimBtnTextRT.anchorMin = Vector2.zero;
                claimBtnTextRT.anchorMax = Vector2.one;
                claimBtnTextRT.offsetMin = Vector2.zero;
                claimBtnTextRT.offsetMax = Vector2.zero;

                var claimBtnText = claimBtnTextObj.AddComponent<TextMeshProUGUI>();
                claimBtnText.text = "✓";
                claimBtnText.fontSize = 16;
                claimBtnText.fontStyle = FontStyles.Bold;
                claimBtnText.alignment = TextAlignmentOptions.Center;
                claimBtnText.color = Color.white;
            }

            return item;
        }

        private void ShowDetail(Mission mission)
        {
            selectedMission = mission;

            if (missionDetailPanel) missionDetailPanel.SetActive(true);

            if (detailTitleText) detailTitleText.text = L(mission.title);
            if (detailDescriptionText) detailDescriptionText.text = L(mission.description);

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
                detailRewardText.text = $"+{mission.rewardAmount} {GetRewardTypeName(mission.rewardType)}";
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

            // Scale punch feedback
            if (claimRewardButton != null)
                ScalePunch.Play(claimRewardButton.gameObject, 1.15f, 0.3f);

            // Coin fly animation
            if (claimRewardButton != null && rewardPopupIcon != null)
            {
                var originRT = claimRewardButton.GetComponent<RectTransform>();
                var targetRT = rewardPopupIcon.GetComponent<RectTransform>();
                Sprite flyIcon = GetRewardTypeIcon(selectedMission.rewardType);
                CoinFlyAnimation.Play(originRT, targetRT, selectedMission.rewardType, 6, 0.6f, flyIcon);
            }

            // Analytics: track mission reward claimed
            AnalyticsService.Instance?.LogMissionCompleted(
                selectedMission.id,
                selectedMission.rewardType,
                selectedMission.rewardAmount
            );

            // Haptic feedback
#if UNITY_IOS || UNITY_ANDROID
            Handheld.Vibrate();
#endif

            // Update UI
            if (claimRewardButton) claimRewardButton.gameObject.SetActive(false);
            UpdateHeaderStats();
            UpdateDailyProgress();
            LoadMissions();

            Debug.Log($"[DailyMissions] Claimed reward for: {selectedMission.id}");
        }

        private void ClaimDailyBonus()
        {
            if (PlayerPrefs.GetInt("DailyBonusClaimed", 0) == 1) return;

            PlayerPrefs.SetInt("DailyBonusClaimed", 1);
            PlayerPrefs.Save();

            ApplyReward("coins", dailyBonusReward);
            ShowRewardPopup("coins", dailyBonusReward);

            // Analytics
            AnalyticsService.Instance?.LogVirtualCurrencyEarned("coins", dailyBonusReward, "daily_missions_bonus");

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

            // Analytics
            AnalyticsService.Instance?.LogVirtualCurrencyEarned(type, amount, "mission_reward");
        }

        private void ShowRewardPopup(string type, int amount)
        {
            if (rewardPopup)
            {
                rewardPopup.SetActive(true);
                if (rewardPopupText) rewardPopupText.text = $"+{amount} {GetRewardTypeName(type)}";
                if (rewardPopupIcon) rewardPopupIcon.sprite = GetRewardTypeIcon(type);

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

                // Analytics: mission completed
                AnalyticsService.Instance?.LogMissionCompleted(
                    mission.id,
                    "progress",
                    0
                );

                Debug.Log($"[DailyMissions] Mission completed: {mission.id}");
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
