using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using DigitPark.Monetization;
using DigitPark.Localization;
using DigitPark.Services.Firebase;
using DigitPark.UI;
using DigitPark.UI.Items;
using DigitPark.Data;
using DigitPark.Games;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager para la escena de misiones diarias.
    /// Usa MissionDefinitionSO y MissionPoolSO para seleccionar misiones.
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
        [SerializeField] private GameObject missionCardPrefab;
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

        [Header("Mission Pools (SO)")]
        [SerializeField] private MissionPoolSO dailyPool;
        [SerializeField] private MissionPoolSO weeklyPool;

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

        // Persistent state key
        private const string STATE_KEY = "DM_State_v2";

        // State
        private MissionTab currentTab = MissionTab.Daily;
        private List<ActiveMission> activeDailyMissions = new List<ActiveMission>();
        private List<ActiveMission> activeWeeklyMissions = new List<ActiveMission>();
        private List<GameObject> spawnedItems = new List<GameObject>();
        private ActiveMission selectedMission;

        // Neon icon sprites
        private Sprite coinIconNeon;
        private Sprite gemIconNeon;

        public enum MissionTab
        {
            Daily,
            Weekly,
            Special
        }

        [Serializable]
        private class ActiveMission
        {
            public string missionId;
            public int currentProgress;
            public bool isCompleted;
            public bool isClaimed;
            [NonSerialized] public MissionDefinitionSO definition;
        }

        [Serializable]
        private class MissionsState
        {
            public string lastDailyReset;
            public string lastWeeklyReset;
            public List<ActiveMission> daily = new List<ActiveMission>();
            public List<ActiveMission> weekly = new List<ActiveMission>();
        }

        private MissionsState state;

        private void Start()
        {
            LoadNeonIcons();
            LoadState();
            CheckResets();
            ApplyPendingProgress();
            SetupUI();
            SetupListeners();
            PopulateUI();

            InvokeRepeating(nameof(UpdateRefreshTimer), 1f, refreshCheckInterval);
            SetupScrollFade();

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

        private string L(string key, params object[] args)
        {
            if (LocalizationManager.Instance == null) return key;
            return args.Length > 0
                ? LocalizationManager.Instance.GetText(key, args)
                : LocalizationManager.Instance.GetText(key);
        }

        #region State Persistence

        private void LoadState()
        {
            string json = PlayerPrefs.GetString(STATE_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    state = JsonUtility.FromJson<MissionsState>(json);
                }
                catch
                {
                    state = null;
                }
            }

            if (state == null)
            {
                state = new MissionsState
                {
                    lastDailyReset = "",
                    lastWeeklyReset = "",
                    daily = new List<ActiveMission>(),
                    weekly = new List<ActiveMission>()
                };
            }

            // Relink definitions
            RelinkDefinitions(state.daily, dailyPool);
            RelinkDefinitions(state.weekly, weeklyPool);
        }

        private void SaveState()
        {
            string json = JsonUtility.ToJson(state);
            PlayerPrefs.SetString(STATE_KEY, json);
            PlayerPrefs.Save();
        }

        private void RelinkDefinitions(List<ActiveMission> missions, MissionPoolSO pool)
        {
            if (pool == null || pool.missions == null) return;

            foreach (var active in missions)
            {
                foreach (var def in pool.missions)
                {
                    if (def != null && def.missionId == active.missionId)
                    {
                        active.definition = def;
                        break;
                    }
                }
            }
        }

        #endregion

        #region Reset Logic

        private void CheckResets()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");

            // Daily reset
            if (state.lastDailyReset != today)
            {
                SelectNewMissions(dailyPool, state.daily, DateTime.Now);
                state.lastDailyReset = today;
                PlayerPrefs.SetInt("DailyBonusClaimed", 0);
            }

            // Weekly reset
            DayOfWeek startOfWeek = DayOfWeek.Monday;
            DateTime thisWeekStart = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek + (int)startOfWeek);
            if (thisWeekStart > DateTime.Now) thisWeekStart = thisWeekStart.AddDays(-7);
            string weekStartStr = thisWeekStart.ToString("yyyy-MM-dd");

            if (state.lastWeeklyReset != weekStartStr)
            {
                SelectNewMissions(weeklyPool, state.weekly, thisWeekStart);
                state.lastWeeklyReset = weekStartStr;
            }

            // Copy references for convenience
            activeDailyMissions = state.daily;
            activeWeeklyMissions = state.weekly;

            SaveState();
        }

        private void SelectNewMissions(MissionPoolSO pool, List<ActiveMission> target, DateTime date)
        {
            target.Clear();

            if (pool == null) return;

            var selected = pool.SelectMissions(date);
            foreach (var def in selected)
            {
                if (def == null) continue;
                target.Add(new ActiveMission
                {
                    missionId = def.missionId,
                    currentProgress = 0,
                    isCompleted = false,
                    isClaimed = false,
                    definition = def
                });
            }
        }

        #endregion

        #region Progress Reporting

        private void ApplyPendingProgress()
        {
            // Read all pending deltas
            int playDelta = MissionProgressReporter.ConsumeIntDelta(MissionProgressReporter.KeyPlayGames);
            int winDelta = MissionProgressReporter.ConsumeIntDelta(MissionProgressReporter.KeyWinGames);
            int bestScore = MissionProgressReporter.ConsumeIntDelta(MissionProgressReporter.KeyBestScore);
            int totalScore = MissionProgressReporter.ConsumeIntDelta(MissionProgressReporter.KeyTotalScore);
            int precisionCount = MissionProgressReporter.ConsumeIntDelta(MissionProgressReporter.KeyPrecisionCount);
            int winStreak = MissionProgressReporter.PeekInt(MissionProgressReporter.KeyWinStreak);
            int tournamentDelta = MissionProgressReporter.ConsumeIntDelta(MissionProgressReporter.KeyTournamentPlayed);
            int sprintDelta = MissionProgressReporter.ConsumeIntDelta(MissionProgressReporter.KeySprintCompleted);
            int gameTypesMask = MissionProgressReporter.PeekInt(MissionProgressReporter.KeyGameTypesPlayed);

            // Count unique game types played from bitmask
            int gameTypesCount = 0;
            int mask = gameTypesMask;
            while (mask != 0)
            {
                gameTypesCount += mask & 1;
                mask >>= 1;
            }

            // Apply to all active missions
            var allMissions = new List<ActiveMission>();
            allMissions.AddRange(activeDailyMissions);
            allMissions.AddRange(activeWeeklyMissions);

            foreach (var mission in allMissions)
            {
                if (mission.isCompleted || mission.definition == null) continue;

                int progressToAdd = 0;
                switch (mission.definition.actionType)
                {
                    case MissionActionType.PlayGames:
                        progressToAdd = playDelta;
                        break;
                    case MissionActionType.WinGames:
                        progressToAdd = winDelta;
                        break;
                    case MissionActionType.PlaySpecificGame:
                        if (mission.definition.isGameSpecific)
                        {
                            int specDelta = MissionProgressReporter.ConsumeIntDelta(
                                MissionProgressReporter.KeyPlaySpecific(mission.definition.targetGame));
                            progressToAdd = specDelta;
                        }
                        break;
                    case MissionActionType.WinSpecificGame:
                        if (mission.definition.isGameSpecific)
                        {
                            int specWin = MissionProgressReporter.ConsumeIntDelta(
                                MissionProgressReporter.KeyWinSpecific(mission.definition.targetGame));
                            progressToAdd = specWin;
                        }
                        break;
                    case MissionActionType.ReachScore:
                        // For ReachScore, we set progress to the best score achieved
                        mission.currentProgress = Mathf.Max(mission.currentProgress, bestScore);
                        break;
                    case MissionActionType.AccumulateScore:
                        progressToAdd = totalScore;
                        break;
                    case MissionActionType.PrecisionGame:
                        progressToAdd = precisionCount;
                        break;
                    case MissionActionType.PlayAllGameTypes:
                        // Set progress to count of unique types played
                        mission.currentProgress = Mathf.Max(mission.currentProgress, gameTypesCount);
                        break;
                    case MissionActionType.WinStreak:
                        // Set progress to current streak
                        mission.currentProgress = Mathf.Max(mission.currentProgress, winStreak);
                        break;
                    case MissionActionType.CompleteCognitiveSprint:
                        progressToAdd = sprintDelta;
                        break;
                    case MissionActionType.PlayTournament:
                        progressToAdd = tournamentDelta;
                        break;
                }

                if (progressToAdd > 0 &&
                    mission.definition.actionType != MissionActionType.ReachScore &&
                    mission.definition.actionType != MissionActionType.PlayAllGameTypes &&
                    mission.definition.actionType != MissionActionType.WinStreak)
                {
                    mission.currentProgress += progressToAdd;
                }

                // Clamp and check completion
                mission.currentProgress = Mathf.Min(mission.currentProgress, mission.definition.targetAmount);
                if (mission.currentProgress >= mission.definition.targetAmount && !mission.isCompleted)
                {
                    mission.isCompleted = true;
                    AnalyticsService.Instance?.LogMissionCompleted(mission.missionId, "progress", 0);
                    Debug.Log($"[DailyMissions] Mission completed: {mission.missionId}");
                }
            }

            // Clear remaining per-game deltas that weren't consumed
            MissionProgressReporter.ClearAllDeltas();
            SaveState();
        }

        #endregion

        #region UI Setup

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

        #endregion

        #region Header & Tabs

        private void UpdateHeaderStats()
        {
            int totalPoints = 0;

            foreach (var mission in activeDailyMissions)
                if (mission.isClaimed && mission.definition != null)
                    totalPoints += mission.definition.rewardAmount;
            foreach (var mission in activeWeeklyMissions)
                if (mission.isClaimed && mission.definition != null)
                    totalPoints += mission.definition.rewardAmount;

            if (totalPointsText) totalPointsText.text = L("ms_rewards_earned", totalPoints);
        }

        private void UpdateRefreshTimer()
        {
            DateTime tomorrow = DateTime.Now.Date.AddDays(1);
            TimeSpan timeUntilReset = tomorrow - DateTime.Now;

            if (refreshTimerText)
            {
                refreshTimerText.text = L("ms_refresh_in", UIPolish.FormatTimerHHMMSS(timeUntilReset.Hours, timeUntilReset.Minutes, timeUntilReset.Seconds));
            }

            string today = DateTime.Now.ToString("yyyy-MM-dd");
            if (state.lastDailyReset != today)
            {
                CheckResets();
                PopulateUI();
            }
        }

        private void SwitchTab(MissionTab tab)
        {
            currentTab = tab;
            UpdateTabVisuals();
            PopulateUI();
        }

        private void UpdateTabVisuals()
        {
            Color activeColor = CYAN_NEON;
            Color inactiveColor = new Color(0.2f, 0.2f, 0.25f);

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

        #endregion

        #region Daily Progress

        private void UpdateDailyProgress()
        {
            int completedDaily = 0;
            foreach (var mission in activeDailyMissions)
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

            UpdateProgressMilestoneMarkers(completedDaily);
        }

        private void UpdateProgressMilestoneMarkers(int completedCount)
        {
            if (dailyProgressBar == null) return;
            var barTransform = dailyProgressBar.transform.parent;
            if (barTransform == null) barTransform = dailyProgressBar.transform;

            string[] markerNames = { "Marker3", "Marker5", "MarkerAll" };
            int[] markerThresholds = { 3, 5, activeDailyMissions.Count };

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
                        ? new Color(1f, 0.84f, 0f, 1f)
                        : CYAN_NEON;
                    Color unreachedColor = isBonus
                        ? new Color(0.3f, 0.25f, 0f, 1f)
                        : new Color(0f, 0.4f, 0.5f, 1f);
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

        #endregion

        #region Missions List UI

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

            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(_emptyStatePanel.transform, false);
            var titleLE = titleObj.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 28;
            var emptyTitle = titleObj.AddComponent<TMPro.TextMeshProUGUI>();
            emptyTitle.text = L("ms_no_missions");
            emptyTitle.fontSize = 18;
            emptyTitle.fontStyle = TMPro.FontStyles.Bold;
            emptyTitle.color = new Color(0.6f, 0.6f, 0.65f);
            emptyTitle.alignment = TMPro.TextAlignmentOptions.Center;

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

        private void PopulateUI()
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

            // Sort: unclaimed completed first, then in progress, then claimed
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
                CreateMissionCard(mission);
            }
        }

        private List<ActiveMission> GetCurrentMissions()
        {
            return currentTab switch
            {
                MissionTab.Daily => new List<ActiveMission>(activeDailyMissions),
                MissionTab.Weekly => new List<ActiveMission>(activeWeeklyMissions),
                _ => new List<ActiveMission>()
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

        private void CreateMissionCard(ActiveMission mission)
        {
            if (mission.definition == null) return;

            // Determine state
            MissionState missionState;
            if (mission.isClaimed)
                missionState = MissionState.Claimed;
            else if (mission.isCompleted)
                missionState = MissionState.Completed;
            else
                missionState = MissionState.InProgress;

            // Get reward icons
            Sprite cIcon = coinIconNeon != null ? coinIconNeon : coinIcon;
            Sprite gIcon = gemIconNeon != null ? gemIconNeon : gemIcon;

            if (missionCardPrefab != null)
            {
                var item = Instantiate(missionCardPrefab, missionsContainer);
                var cardUI = item.GetComponent<MissionCardUI>();
                if (cardUI != null)
                {
                    cardUI.Setup(mission.definition, mission.currentProgress, missionState, cIcon, gIcon, OnCardClaimClicked);
                }

                // Also add detail click
                var button = item.GetComponent<Button>() ?? item.AddComponent<Button>();
                var m = mission;
                button.onClick.AddListener(() => ShowDetail(m));

                spawnedItems.Add(item);
            }
            else
            {
                // Fallback: create basic item without prefab
                var item = CreateMissionCardFallback(mission, missionState);
                spawnedItems.Add(item);
            }
        }

        private void OnCardClaimClicked(string missionId)
        {
            var mission = FindMission(missionId);
            if (mission == null || mission.isClaimed || !mission.isCompleted) return;

            ClaimReward(mission);
        }

        private GameObject CreateMissionCardFallback(ActiveMission mission, MissionState missionState)
        {
            var item = new GameObject($"Mission_{mission.missionId}");
            item.transform.SetParent(missionsContainer, false);

            var rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(350, 110);

            var image = item.AddComponent<Image>();
            UIPolish.ApplyRoundedCorners(image);

            if (mission.isClaimed)
                image.color = CARD_BG_CLAIMED;
            else if (mission.isCompleted)
                image.color = CARD_BG_COMPLETED;
            else
                image.color = CARD_BG;

            // Category border
            Color categoryColor = mission.definition.category switch
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

            // Title
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(item.transform, false);
            var titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.03f, 0.65f);
            titleRT.anchorMax = new Vector2(0.7f, 1);
            titleRT.offsetMin = new Vector2(10, 0);
            titleRT.offsetMax = new Vector2(0, -8);

            var titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.text = L(mission.definition.titleLocKey);
            titleTmp.fontSize = 15;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = mission.isClaimed ? new Color(0.45f, 0.45f, 0.45f) : Color.white;

            // Description
            var descObj = new GameObject("Description");
            descObj.transform.SetParent(item.transform, false);
            var descRT = descObj.AddComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0.03f, 0.38f);
            descRT.anchorMax = new Vector2(0.7f, 0.65f);
            descRT.offsetMin = new Vector2(10, 0);
            descRT.offsetMax = new Vector2(0, 0);

            var descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = L(mission.definition.descriptionLocKey);
            descText.fontSize = 11;
            descText.color = new Color(0.6f, 0.6f, 0.65f);

            // Progress text
            var progressTextObj = new GameObject("ProgressText");
            progressTextObj.transform.SetParent(item.transform, false);
            var progressTextRT = progressTextObj.AddComponent<RectTransform>();
            progressTextRT.anchorMin = new Vector2(0.03f, 0.08f);
            progressTextRT.anchorMax = new Vector2(0.55f, 0.35f);
            progressTextRT.offsetMin = new Vector2(10, 0);
            progressTextRT.offsetMax = Vector2.zero;

            var progressTmp = progressTextObj.AddComponent<TextMeshProUGUI>();
            if (mission.isClaimed)
            {
                progressTmp.text = "\u2713 " + L("ms_completed");
                progressTmp.color = GREEN_SUCCESS;
            }
            else if (mission.isCompleted)
            {
                progressTmp.text = L("ms_ready_claim");
                progressTmp.color = GREEN_SUCCESS;
            }
            else
            {
                progressTmp.text = $"{mission.currentProgress}/{mission.definition.targetAmount}";
                progressTmp.color = new Color(0.55f, 0.55f, 0.6f);
            }
            progressTmp.fontSize = 11;
            progressTmp.alignment = TextAlignmentOptions.Left;

            // Reward
            var rewardAmountObj = new GameObject("RewardAmount");
            rewardAmountObj.transform.SetParent(item.transform, false);
            var rewardAmountRT = rewardAmountObj.AddComponent<RectTransform>();
            rewardAmountRT.anchorMin = new Vector2(0.73f, 0.15f);
            rewardAmountRT.anchorMax = new Vector2(0.98f, 0.85f);
            rewardAmountRT.offsetMin = Vector2.zero;
            rewardAmountRT.offsetMax = Vector2.zero;

            var rewardAmountTmp = rewardAmountObj.AddComponent<TextMeshProUGUI>();
            rewardAmountTmp.text = $"+{mission.definition.rewardAmount}";
            rewardAmountTmp.fontSize = 14;
            rewardAmountTmp.fontStyle = FontStyles.Bold;
            rewardAmountTmp.alignment = TextAlignmentOptions.Center;
            rewardAmountTmp.color = mission.isClaimed
                ? new Color(0.4f, 0.4f, 0.4f)
                : (mission.definition.rewardType == MissionRewardType.Gems ? GEM_COLOR : COIN_COLOR);

            // Detail click
            var button = item.AddComponent<Button>();
            var m = mission;
            button.onClick.AddListener(() => ShowDetail(m));

            return item;
        }

        #endregion

        #region Detail Panel

        private void ShowDetail(ActiveMission mission)
        {
            if (mission.definition == null) return;
            selectedMission = mission;

            if (missionDetailPanel) missionDetailPanel.SetActive(true);

            if (detailTitleText) detailTitleText.text = L(mission.definition.titleLocKey);
            if (detailDescriptionText) detailDescriptionText.text = L(mission.definition.descriptionLocKey);

            if (detailProgressBar)
            {
                detailProgressBar.maxValue = mission.definition.targetAmount;
                detailProgressBar.value = mission.currentProgress;
            }

            if (detailProgressText)
            {
                detailProgressText.text = $"{mission.currentProgress}/{mission.definition.targetAmount}";
            }

            if (detailRewardText)
            {
                string rewardTypeName = mission.definition.rewardType == MissionRewardType.Gems
                    ? L("reward_gems")
                    : L("reward_coins");
                detailRewardText.text = $"+{mission.definition.rewardAmount} {rewardTypeName}";
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

        #endregion

        #region Claim Rewards

        private void ClaimMissionReward()
        {
            if (selectedMission == null || selectedMission.isClaimed) return;
            ClaimReward(selectedMission);
        }

        private void ClaimReward(ActiveMission mission)
        {
            if (mission == null || mission.isClaimed || mission.definition == null) return;

            mission.isClaimed = true;
            SaveState();

            // Apply reward via CurrencyManager
            int gems = mission.definition.rewardType == MissionRewardType.Gems ? mission.definition.rewardAmount : 0;
            int coins = mission.definition.rewardType == MissionRewardType.Coins ? mission.definition.rewardAmount : 0;

            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.GrantMissionReward(gems, coins);
            }
            else
            {
                // Fallback to PlayerPrefs
                if (coins > 0)
                {
                    int currentCoins = PlayerPrefs.GetInt("PlayerCoins", 0);
                    PlayerPrefs.SetInt("PlayerCoins", currentCoins + coins);
                }
                if (gems > 0)
                {
                    int currentGems = PlayerPrefs.GetInt("PlayerGems", 0);
                    PlayerPrefs.SetInt("PlayerGems", currentGems + gems);
                }
                PlayerPrefs.Save();
            }

            // Show popup
            string rewardTypeStr = mission.definition.rewardType == MissionRewardType.Gems ? "gems" : "coins";
            ShowRewardPopup(rewardTypeStr, mission.definition.rewardAmount);

            // Scale punch feedback
            if (claimRewardButton != null)
                ScalePunch.Play(claimRewardButton.gameObject, 1.15f, 0.3f);

            // Coin fly animation
            if (claimRewardButton != null && rewardPopupIcon != null)
            {
                var originRT = claimRewardButton.GetComponent<RectTransform>();
                var targetRT = rewardPopupIcon.GetComponent<RectTransform>();
                Sprite flyIcon = mission.definition.rewardType == MissionRewardType.Gems
                    ? (gemIconNeon != null ? gemIconNeon : gemIcon)
                    : (coinIconNeon != null ? coinIconNeon : coinIcon);
                CoinFlyAnimation.Play(originRT, targetRT, rewardTypeStr, 6, 0.6f, flyIcon);
            }

            // Analytics
            AnalyticsService.Instance?.LogMissionCompleted(
                mission.missionId,
                rewardTypeStr,
                mission.definition.rewardAmount
            );

            // Haptic feedback
#if UNITY_IOS || UNITY_ANDROID
            Handheld.Vibrate();
#endif

            // Update UI
            if (claimRewardButton) claimRewardButton.gameObject.SetActive(false);
            UpdateHeaderStats();
            UpdateDailyProgress();
            PopulateUI();

            Debug.Log($"[DailyMissions] Claimed reward for: {mission.missionId}");
        }

        private void ClaimDailyBonus()
        {
            if (PlayerPrefs.GetInt("DailyBonusClaimed", 0) == 1) return;

            PlayerPrefs.SetInt("DailyBonusClaimed", 1);
            PlayerPrefs.Save();

            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.GrantMissionReward(0, dailyBonusReward);
            }
            else
            {
                int currentCoins = PlayerPrefs.GetInt("PlayerCoins", 0);
                PlayerPrefs.SetInt("PlayerCoins", currentCoins + dailyBonusReward);
                PlayerPrefs.Save();
            }

            ShowRewardPopup("coins", dailyBonusReward);

            AnalyticsService.Instance?.LogVirtualCurrencyEarned("coins", dailyBonusReward, "daily_missions_bonus");

            UpdateDailyProgress();
            Debug.Log($"[DailyMissions] Claimed daily bonus: {dailyBonusReward} coins");
        }

        #endregion

        #region Reward Popup

        private void ShowRewardPopup(string type, int amount)
        {
            if (rewardPopup)
            {
                rewardPopup.SetActive(true);
                string rewardTypeName = type == "gems" ? L("reward_gems") : L("reward_coins");
                if (rewardPopupText) rewardPopupText.text = $"+{amount} {rewardTypeName}";
                if (rewardPopupIcon)
                {
                    rewardPopupIcon.sprite = type == "gems"
                        ? (gemIconNeon != null ? gemIconNeon : gemIcon)
                        : (coinIconNeon != null ? coinIconNeon : coinIcon);
                }

                Invoke(nameof(HideRewardPopup), 2f);
            }
        }

        private void HideRewardPopup()
        {
            if (rewardPopup) rewardPopup.SetActive(false);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Update mission progress (call from game logic when in scene)
        /// </summary>
        public void UpdateMissionProgress(string missionId, int progress)
        {
            ActiveMission mission = FindMission(missionId);
            if (mission == null || mission.isCompleted || mission.definition == null) return;

            mission.currentProgress = Mathf.Min(progress, mission.definition.targetAmount);

            if (mission.currentProgress >= mission.definition.targetAmount)
            {
                mission.isCompleted = true;
                AnalyticsService.Instance?.LogMissionCompleted(mission.missionId, "progress", 0);
                Debug.Log($"[DailyMissions] Mission completed: {mission.missionId}");
            }

            SaveState();
            UpdateHeaderStats();
            UpdateDailyProgress();
        }

        /// <summary>
        /// Increment mission progress by amount
        /// </summary>
        public void IncrementMissionProgress(string missionId, int amount = 1)
        {
            ActiveMission mission = FindMission(missionId);
            if (mission == null || mission.isCompleted) return;

            UpdateMissionProgress(missionId, mission.currentProgress + amount);
        }

        #endregion

        #region Helpers

        private ActiveMission FindMission(string id)
        {
            foreach (var m in activeDailyMissions) if (m.missionId == id) return m;
            foreach (var m in activeWeeklyMissions) if (m.missionId == id) return m;
            return null;
        }

        private void OnBackClicked()
        {
            SceneNavigator.Instance?.GoBack();
        }

        #endregion
    }
}
