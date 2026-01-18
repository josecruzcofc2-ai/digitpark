using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using DigitPark.Monetization;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager para la escena de recompensas diarias.
    /// Sistema de login rewards con racha de días consecutivos.
    /// </summary>
    public class DailyRewardsManager : MonoBehaviour
    {
        [Header("UI - Header")]
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI streakText;
        [SerializeField] private TextMeshProUGUI nextResetText;

        [Header("UI - Current Day")]
        [SerializeField] private GameObject currentDayHighlight;
        [SerializeField] private TextMeshProUGUI currentDayText;
        [SerializeField] private Image currentDayRewardIcon;
        [SerializeField] private TextMeshProUGUI currentDayRewardText;

        [Header("UI - Rewards Grid")]
        [SerializeField] private Transform rewardsContainer;
        [SerializeField] private GameObject rewardDayPrefab;
        [SerializeField] private int daysInCycle = 7;

        [Header("UI - Claim Button")]
        [SerializeField] private Button claimButton;
        [SerializeField] private TextMeshProUGUI claimButtonText;
        [SerializeField] private GameObject claimGlow;

        [Header("UI - Bonus Info")]
        [SerializeField] private TextMeshProUGUI bonusInfoText;
        [SerializeField] private Slider streakProgressBar;
        [SerializeField] private TextMeshProUGUI streakBonusText;

        [Header("UI - Claim Animation")]
        [SerializeField] private GameObject claimAnimationPanel;
        [SerializeField] private TextMeshProUGUI claimRewardText;
        [SerializeField] private Image claimRewardIcon;
        [SerializeField] private ParticleSystem claimParticles;
        [SerializeField] private Button continueButton;

        [Header("UI - Streak Milestone")]
        [SerializeField] private GameObject milestonePanel;
        [SerializeField] private TextMeshProUGUI milestoneText;
        [SerializeField] private TextMeshProUGUI milestoneBonusText;

        [Header("Reward Icons")]
        [SerializeField] private Sprite coinIcon;
        [SerializeField] private Sprite gemIcon;
        [SerializeField] private Sprite chestIcon;
        [SerializeField] private Sprite xpIcon;
        [SerializeField] private Sprite mysteryIcon;

        [Header("Configuration")]
        [SerializeField] private List<DailyRewardConfig> rewards = new List<DailyRewardConfig>();
        [SerializeField] private int[] streakMilestones = { 7, 14, 30 };
        [SerializeField] private int[] milestoneBonuses = { 100, 250, 500 };

        // State
        private List<GameObject> spawnedDayItems = new List<GameObject>();
        private int currentStreak = 0;
        private int currentDayInCycle = 0;
        private bool canClaimToday = false;
        private DateTime lastClaimDate;
        private DailyRewardConfig todayReward;

        private void Start()
        {
            InitializeRewards();
            LoadProgress();
            SetupUI();
            SetupListeners();
            CheckClaimStatus();
            PopulateRewardsGrid();
        }

        private void InitializeRewards()
        {
            // Default rewards if not set in inspector
            if (rewards.Count == 0)
            {
                rewards = new List<DailyRewardConfig>
                {
                    new DailyRewardConfig { day = 1, type = "coins", amount = 50, name = "Monedas" },
                    new DailyRewardConfig { day = 2, type = "coins", amount = 75, name = "Monedas" },
                    new DailyRewardConfig { day = 3, type = "gems", amount = 5, name = "Gemas" },
                    new DailyRewardConfig { day = 4, type = "coins", amount = 100, name = "Monedas" },
                    new DailyRewardConfig { day = 5, type = "xp", amount = 200, name = "XP" },
                    new DailyRewardConfig { day = 6, type = "coins", amount = 150, name = "Monedas" },
                    new DailyRewardConfig { day = 7, type = "chest", amount = 1, name = "Cofre Raro", isSpecial = true },
                };

                daysInCycle = rewards.Count;
            }
        }

        private void LoadProgress()
        {
            currentStreak = PlayerPrefs.GetInt("DailyRewards_Streak", 0);
            currentDayInCycle = PlayerPrefs.GetInt("DailyRewards_CurrentDay", 0);

            string lastClaimStr = PlayerPrefs.GetString("DailyRewards_LastClaim", "");
            if (!string.IsNullOrEmpty(lastClaimStr))
            {
                lastClaimDate = DateTime.Parse(lastClaimStr);
            }
            else
            {
                lastClaimDate = DateTime.MinValue;
            }
        }

        private void SaveProgress()
        {
            PlayerPrefs.SetInt("DailyRewards_Streak", currentStreak);
            PlayerPrefs.SetInt("DailyRewards_CurrentDay", currentDayInCycle);
            PlayerPrefs.SetString("DailyRewards_LastClaim", lastClaimDate.ToString("yyyy-MM-dd"));
            PlayerPrefs.Save();
        }

        private void SetupUI()
        {
            if (claimAnimationPanel) claimAnimationPanel.SetActive(false);
            if (milestonePanel) milestonePanel.SetActive(false);

            UpdateStreakDisplay();
            UpdateNextResetTimer();
        }

        private void SetupListeners()
        {
            if (backButton) backButton.onClick.AddListener(OnBackClicked);
            if (claimButton) claimButton.onClick.AddListener(OnClaimClicked);
            if (continueButton) continueButton.onClick.AddListener(OnContinueClicked);
        }

        private void CheckClaimStatus()
        {
            DateTime today = DateTime.Now.Date;
            DateTime lastClaim = lastClaimDate.Date;

            // Check if already claimed today
            if (lastClaim == today)
            {
                canClaimToday = false;
            }
            // Check if missed a day (streak broken)
            else if (lastClaim < today.AddDays(-1) && currentStreak > 0)
            {
                // Streak broken, reset
                currentStreak = 0;
                currentDayInCycle = 0;
                canClaimToday = true;
                Debug.Log("[DailyRewards] Streak broken, resetting");
            }
            else
            {
                canClaimToday = true;
            }

            // Get today's reward
            todayReward = rewards[currentDayInCycle % rewards.Count];

            UpdateClaimButton();
            UpdateCurrentDayDisplay();
        }

        private void UpdateStreakDisplay()
        {
            if (streakText)
            {
                streakText.text = $"Racha: {currentStreak} días";
            }

            if (streakProgressBar)
            {
                int nextMilestone = GetNextMilestone();
                streakProgressBar.maxValue = nextMilestone;
                streakProgressBar.value = currentStreak % nextMilestone;
            }

            if (streakBonusText)
            {
                int nextMilestone = GetNextMilestone();
                int bonusIndex = GetMilestoneIndex(nextMilestone);
                if (bonusIndex >= 0 && bonusIndex < milestoneBonuses.Length)
                {
                    streakBonusText.text = $"Bonus día {nextMilestone}: +{milestoneBonuses[bonusIndex]} gemas";
                }
            }
        }

        private int GetNextMilestone()
        {
            foreach (int milestone in streakMilestones)
            {
                if (currentStreak < milestone) return milestone;
            }
            return streakMilestones[streakMilestones.Length - 1];
        }

        private int GetMilestoneIndex(int milestone)
        {
            for (int i = 0; i < streakMilestones.Length; i++)
            {
                if (streakMilestones[i] == milestone) return i;
            }
            return -1;
        }

        private void UpdateNextResetTimer()
        {
            DateTime tomorrow = DateTime.Now.Date.AddDays(1);
            TimeSpan timeUntilReset = tomorrow - DateTime.Now;

            if (nextResetText)
            {
                nextResetText.text = canClaimToday
                    ? "¡Disponible ahora!"
                    : $"Próximo en: {timeUntilReset.Hours:D2}:{timeUntilReset.Minutes:D2}";
            }
        }

        private void UpdateClaimButton()
        {
            if (claimButton)
            {
                claimButton.interactable = canClaimToday;
            }

            if (claimButtonText)
            {
                claimButtonText.text = canClaimToday ? "¡RECLAMAR!" : "Ya reclamado";
            }

            if (claimGlow)
            {
                claimGlow.SetActive(canClaimToday);
            }
        }

        private void UpdateCurrentDayDisplay()
        {
            int displayDay = currentDayInCycle + 1;

            if (currentDayText)
            {
                currentDayText.text = $"Día {displayDay}";
            }

            if (todayReward != null)
            {
                if (currentDayRewardIcon)
                {
                    currentDayRewardIcon.sprite = GetRewardIcon(todayReward.type);
                }

                if (currentDayRewardText)
                {
                    currentDayRewardText.text = todayReward.type == "chest"
                        ? todayReward.name
                        : $"+{todayReward.amount} {todayReward.name}";
                }
            }
        }

        private void PopulateRewardsGrid()
        {
            // Clear existing
            foreach (var item in spawnedDayItems)
            {
                if (item) Destroy(item);
            }
            spawnedDayItems.Clear();

            // Create day items
            for (int i = 0; i < rewards.Count; i++)
            {
                CreateDayItem(i, rewards[i]);
            }
        }

        private void CreateDayItem(int dayIndex, DailyRewardConfig reward)
        {
            GameObject item;

            if (rewardDayPrefab != null)
            {
                item = Instantiate(rewardDayPrefab, rewardsContainer);
            }
            else
            {
                item = CreateDayItemFallback(dayIndex, reward);
            }

            spawnedDayItems.Add(item);
        }

        private GameObject CreateDayItemFallback(int dayIndex, DailyRewardConfig reward)
        {
            var item = new GameObject($"Day_{dayIndex + 1}");
            item.transform.SetParent(rewardsContainer, false);

            var rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(100, 120);

            var image = item.AddComponent<Image>();

            bool isClaimed = dayIndex < currentDayInCycle;
            bool isToday = dayIndex == currentDayInCycle;
            bool isFuture = dayIndex > currentDayInCycle;

            // Colors
            if (isClaimed)
                image.color = new Color(0.1f, 0.3f, 0.1f, 0.9f);
            else if (isToday)
                image.color = new Color(0f, 0.5f, 0.8f, 0.95f);
            else
                image.color = new Color(0.2f, 0.2f, 0.25f, 0.8f);

            // Day label
            var dayLabelObj = new GameObject("DayLabel");
            dayLabelObj.transform.SetParent(item.transform, false);
            var dayLabelRT = dayLabelObj.AddComponent<RectTransform>();
            dayLabelRT.anchorMin = new Vector2(0, 0.75f);
            dayLabelRT.anchorMax = new Vector2(1, 1);
            dayLabelRT.offsetMin = new Vector2(5, 0);
            dayLabelRT.offsetMax = new Vector2(-5, -5);

            var dayLabelText = dayLabelObj.AddComponent<TextMeshProUGUI>();
            dayLabelText.text = $"Día {dayIndex + 1}";
            dayLabelText.fontSize = 12;
            dayLabelText.fontStyle = FontStyles.Bold;
            dayLabelText.alignment = TextAlignmentOptions.Center;
            dayLabelText.color = isToday ? Color.white : new Color(0.7f, 0.7f, 0.7f);

            // Reward info
            var rewardObj = new GameObject("Reward");
            rewardObj.transform.SetParent(item.transform, false);
            var rewardRT = rewardObj.AddComponent<RectTransform>();
            rewardRT.anchorMin = new Vector2(0, 0.25f);
            rewardRT.anchorMax = new Vector2(1, 0.75f);
            rewardRT.offsetMin = new Vector2(5, 0);
            rewardRT.offsetMax = new Vector2(-5, 0);

            var rewardText = rewardObj.AddComponent<TextMeshProUGUI>();
            if (reward.type == "chest")
            {
                rewardText.text = "🎁\nCofre";
            }
            else
            {
                string icon = reward.type switch
                {
                    "coins" => "🪙",
                    "gems" => "💎",
                    "xp" => "⭐",
                    _ => "🎁"
                };
                rewardText.text = $"{icon}\n+{reward.amount}";
            }
            rewardText.fontSize = 14;
            rewardText.alignment = TextAlignmentOptions.Center;
            rewardText.color = reward.isSpecial ? new Color(1f, 0.84f, 0f) : Color.white;

            // Status indicator
            var statusObj = new GameObject("Status");
            statusObj.transform.SetParent(item.transform, false);
            var statusRT = statusObj.AddComponent<RectTransform>();
            statusRT.anchorMin = new Vector2(0, 0);
            statusRT.anchorMax = new Vector2(1, 0.25f);
            statusRT.offsetMin = new Vector2(5, 5);
            statusRT.offsetMax = new Vector2(-5, 0);

            var statusText = statusObj.AddComponent<TextMeshProUGUI>();
            if (isClaimed)
            {
                statusText.text = "✓";
                statusText.color = new Color(0f, 1f, 0.5f);
            }
            else if (isToday && canClaimToday)
            {
                statusText.text = "¡HOY!";
                statusText.color = new Color(0f, 0.83f, 1f);
            }
            else
            {
                statusText.text = "";
            }
            statusText.fontSize = 12;
            statusText.alignment = TextAlignmentOptions.Center;

            // Special day glow
            if (reward.isSpecial && !isClaimed)
            {
                var glowObj = new GameObject("Glow");
                glowObj.transform.SetParent(item.transform, false);
                glowObj.transform.SetAsFirstSibling();
                var glowRT = glowObj.AddComponent<RectTransform>();
                glowRT.anchorMin = Vector2.zero;
                glowRT.anchorMax = Vector2.one;
                glowRT.sizeDelta = new Vector2(10, 10);

                var glowImage = glowObj.AddComponent<Image>();
                glowImage.color = new Color(1f, 0.84f, 0f, 0.3f);
            }

            return item;
        }

        private Sprite GetRewardIcon(string type)
        {
            return type switch
            {
                "coins" => coinIcon,
                "gems" => gemIcon,
                "chest" => chestIcon,
                "xp" => xpIcon,
                _ => mysteryIcon
            };
        }

        private void OnClaimClicked()
        {
            if (!canClaimToday) return;

            // Claim the reward
            ClaimTodayReward();
        }

        private void ClaimTodayReward()
        {
            if (todayReward == null) return;

            // Apply reward
            ApplyReward(todayReward);

            // Update state
            currentStreak++;
            currentDayInCycle = (currentDayInCycle + 1) % rewards.Count;
            lastClaimDate = DateTime.Now;
            canClaimToday = false;

            SaveProgress();

            // Show claim animation
            ShowClaimAnimation(todayReward);

            // Check for milestone
            CheckMilestone();

            // Update UI
            UpdateStreakDisplay();
            UpdateClaimButton();
            UpdateCurrentDayDisplay();
            PopulateRewardsGrid();

            Debug.Log($"[DailyRewards] Claimed day {currentDayInCycle}, streak: {currentStreak}");
        }

        private void ApplyReward(DailyRewardConfig reward)
        {
            switch (reward.type)
            {
                case "coins":
                    int currentCoins = PlayerPrefs.GetInt("PlayerCoins", 0);
                    PlayerPrefs.SetInt("PlayerCoins", currentCoins + reward.amount);
                    break;

                case "gems":
                    int currentGems = PlayerPrefs.GetInt("PlayerGems", 0);
                    PlayerPrefs.SetInt("PlayerGems", currentGems + reward.amount);
                    break;

                case "xp":
                    int currentXP = PlayerPrefs.GetInt("PlayerXP", 0);
                    PlayerPrefs.SetInt("PlayerXP", currentXP + reward.amount);
                    break;

                case "chest":
                    // Add chest to inventory
                    int chests = PlayerPrefs.GetInt("PlayerChests_Rare", 0);
                    PlayerPrefs.SetInt("PlayerChests_Rare", chests + reward.amount);
                    break;
            }

            PlayerPrefs.Save();
        }

        private void ShowClaimAnimation(DailyRewardConfig reward)
        {
            if (claimAnimationPanel)
            {
                claimAnimationPanel.SetActive(true);

                if (claimRewardText)
                {
                    claimRewardText.text = reward.type == "chest"
                        ? $"¡{reward.name}!"
                        : $"+{reward.amount} {reward.name}";
                }

                if (claimRewardIcon)
                {
                    claimRewardIcon.sprite = GetRewardIcon(reward.type);
                }

                if (claimParticles)
                {
                    claimParticles.Play();
                }
            }
        }

        private void CheckMilestone()
        {
            foreach (int milestone in streakMilestones)
            {
                if (currentStreak == milestone)
                {
                    int bonusIndex = GetMilestoneIndex(milestone);
                    if (bonusIndex >= 0 && bonusIndex < milestoneBonuses.Length)
                    {
                        int bonus = milestoneBonuses[bonusIndex];
                        ApplyMilestoneBonus(bonus);
                        ShowMilestonePopup(milestone, bonus);
                    }
                    break;
                }
            }
        }

        private void ApplyMilestoneBonus(int gemBonus)
        {
            int currentGems = PlayerPrefs.GetInt("PlayerGems", 0);
            PlayerPrefs.SetInt("PlayerGems", currentGems + gemBonus);
            PlayerPrefs.Save();

            Debug.Log($"[DailyRewards] Milestone bonus applied: +{gemBonus} gems");
        }

        private void ShowMilestonePopup(int days, int bonus)
        {
            if (milestonePanel)
            {
                milestonePanel.SetActive(true);

                if (milestoneText)
                {
                    milestoneText.text = $"¡{days} días seguidos!";
                }

                if (milestoneBonusText)
                {
                    milestoneBonusText.text = $"+{bonus} gemas de bonus";
                }
            }
        }

        private void OnContinueClicked()
        {
            if (claimAnimationPanel) claimAnimationPanel.SetActive(false);
            if (milestonePanel) milestonePanel.SetActive(false);
        }

        private void Update()
        {
            // Update timer every second
            UpdateNextResetTimer();
        }

        private void OnBackClicked()
        {
            SceneNavigator.Instance?.GoBack();
        }
    }

    [Serializable]
    public class DailyRewardConfig
    {
        public int day;
        public string type;
        public int amount;
        public string name;
        public bool isSpecial;
    }
}
