using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using DigitPark.UI.Items;
using DigitPark.Monetization;
using DigitPark.Localization;
using DigitPark.Services;
using DG.Tweening;
using ServiceAchievementData = DigitPark.Services.AchievementData;
using ServiceCategory = DigitPark.Services.AchievementCategory;

namespace DigitPark.Managers
{
    /// <summary>
    /// Trophy Showcase Achievements Manager.
    /// UI layer that reads data from AchievementService.
    /// Displays achievements as collectible trophies in glass display cases.
    /// </summary>
    public class AchievementsManager : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI totalPointsText;
        [SerializeField] private TextMeshProUGUI completionText;
        [SerializeField] private Slider overallProgressBar;

        [Header("Category Tabs (11 categories)")]
        [SerializeField] private Transform tabsContainer;
        [SerializeField] private ScrollRect tabsScrollRect;
        [SerializeField] private Button allTab;
        [SerializeField] private Button beginnerTab;
        [SerializeField] private Button masteryTab;
        [SerializeField] private Button victoriesTab;
        [SerializeField] private Button streaksTab;
        [SerializeField] private Button cashBattleTab;
        [SerializeField] private Button tournamentsTab;
        [SerializeField] private Button socialTab;
        [SerializeField] private Button progressionTab;
        [SerializeField] private Button collectorTab;
        [SerializeField] private Button timeTab;
        [SerializeField] private Button secretTab;

        [Header("Trophy Showcase")]
        [SerializeField] private Transform showcaseContainer;
        [SerializeField] private GameObject trophyCardPrefab;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private GridLayoutGroup gridLayout;

        [Header("Empty State")]
        [SerializeField] private GameObject emptyStateContainer;
        [SerializeField] private TextMeshProUGUI emptyStateText;
        [SerializeField] private Image emptyStateIcon;

        [Header("Detail Panel")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private CanvasGroup detailPanelCanvasGroup;
        [SerializeField] private Image detailBlocker;
        [SerializeField] private RectTransform detailCard;

        [Header("Detail Panel - Content")]
        [SerializeField] private Image detailTrophyIcon;
        [SerializeField] private TextMeshProUGUI detailTitleText;
        [SerializeField] private TextMeshProUGUI detailDescriptionText;
        [SerializeField] private TextMeshProUGUI detailCategoryText;
        [SerializeField] private Slider detailProgressBar;
        [SerializeField] private TextMeshProUGUI detailProgressText;
        [SerializeField] private TextMeshProUGUI detailPointsText;
        [SerializeField] private Button claimRewardButton;
        [SerializeField] private TextMeshProUGUI claimButtonText;
        [SerializeField] private Button closeDetailButton;
        [SerializeField] private ParticleSystem detailParticles;

        [Header("Reward Celebration")]
        [SerializeField] private GameObject rewardCelebration;
        [SerializeField] private TextMeshProUGUI rewardAmountText;
        [SerializeField] private ParticleSystem celebrationParticles;
        [SerializeField] private Image celebrationGlow;

        [Header("Icons")]
        [SerializeField] private Sprite defaultTrophyIcon;
        [SerializeField] private Sprite lockedTrophyIcon;
        [SerializeField] private Sprite secretTrophyIcon;

        [Header("Tab Colors")]
        [SerializeField] private Color tabActiveColor = new Color(0f, 1f, 1f, 1f);
        [SerializeField] private Color tabInactiveColor = new Color(0.2f, 0.25f, 0.3f, 1f);
        [SerializeField] private Color tabActiveTextColor = new Color(0.02f, 0.05f, 0.1f, 1f);
        [SerializeField] private Color tabInactiveTextColor = new Color(0.6f, 0.6f, 0.6f, 1f);

        // State
        private AchievementCategory currentCategory = AchievementCategory.All;
        private List<AchievementDefinition> allAchievements = new List<AchievementDefinition>();
        private List<TrophyCardUI> spawnedCards = new List<TrophyCardUI>();
        private DigitPark.UI.Items.AchievementData selectedAchievement;
        private TrophyCardUI selectedCard;

        public enum AchievementCategory
        {
            All,
            Beginner,       // Onboarding achievements
            Mastery,        // Game mastery achievements
            Victories,      // Win-based achievements
            Streaks,        // Win streak achievements
            CashBattle,     // Cash battle achievements
            Tournaments,    // Tournament achievements
            Social,         // Friend-based achievements
            Progression,    // Level/rank achievements
            Collector,      // Collection achievements (reserved for V2)
            Time,           // Login/dedication achievements
            Secret          // Hidden achievements
        }

        #region Initialization

        private void Start()
        {
            InitializeAchievements();
            SetupUI();
            SetupListeners();
            LoadShowcase();
        }

        private void OnDestroy()
        {
            // Unsubscribe from Service events to avoid memory leaks
            var service = AchievementService.Instance;
            if (service != null)
            {
                service.OnAchievementProgress -= OnServiceProgressUpdated;
                service.OnAchievementUnlocked -= OnServiceAchievementUnlocked;
            }
        }

        private void InitializeAchievements()
        {
            allAchievements.Clear();

            var service = AchievementService.Instance;
            if (service == null)
            {
                Debug.LogWarning("[AchievementsManager] AchievementService not available, using empty list");
                return;
            }

            // Build local definitions from the Service's data
            foreach (var svcAch in service.AllAchievements)
            {
                var category = MapServiceCategory(svcAch.category);
                bool isSecret = svcAch.isHidden || category == AchievementCategory.Secret;

                var def = new AchievementDefinition(
                    svcAch.id,
                    AutoLocalizer.Get(svcAch.titleKey),
                    AutoLocalizer.Get(svcAch.descriptionKey),
                    category,
                    svcAch.points,
                    svcAch.targetValue,
                    svcAch.iconName,
                    isSecret
                );

                // Load progress from Service
                def.currentProgress = service.GetCurrentValue(svcAch.id);
                def.isCompleted = service.IsUnlocked(svcAch.id);
                def.isClaimed = service.IsClaimed(svcAch.id);

                allAchievements.Add(def);
            }

            Debug.Log($"[AchievementsManager] Loaded {allAchievements.Count} achievements from Service");
        }

        /// <summary>
        /// Maps Service AchievementCategory to Manager AchievementCategory
        /// </summary>
        private AchievementCategory MapServiceCategory(ServiceCategory serviceCategory)
        {
            return serviceCategory switch
            {
                ServiceCategory.Beginner => AchievementCategory.Beginner,
                ServiceCategory.Mastery => AchievementCategory.Mastery,
                ServiceCategory.Victories => AchievementCategory.Victories,
                ServiceCategory.Streaks => AchievementCategory.Streaks,
                ServiceCategory.CashBattle => AchievementCategory.CashBattle,
                ServiceCategory.Tournaments => AchievementCategory.Tournaments,
                ServiceCategory.Social => AchievementCategory.Social,
                ServiceCategory.Progression => AchievementCategory.Progression,
                ServiceCategory.Collector => AchievementCategory.Collector,
                ServiceCategory.Time => AchievementCategory.Time,
                ServiceCategory.Secret => AchievementCategory.Secret,
                _ => AchievementCategory.Beginner
            };
        }

        private void SetupUI()
        {
            if (detailPanel) detailPanel.SetActive(false);
            if (rewardCelebration) rewardCelebration.SetActive(false);
            if (emptyStateContainer) emptyStateContainer.SetActive(false);

            UpdateHeaderStats();
            UpdateTabVisuals();
        }

        private void SetupListeners()
        {
            // Back button
            if (backButton) backButton.onClick.AddListener(OnBackClicked);

            // Detail panel
            if (closeDetailButton) closeDetailButton.onClick.AddListener(CloseDetailPanel);
            if (detailBlocker) detailBlocker.GetComponent<Button>()?.onClick.AddListener(CloseDetailPanel);
            if (claimRewardButton) claimRewardButton.onClick.AddListener(ClaimReward);

            // Tabs - 11 categories with scrollable tabs
            if (allTab) allTab.onClick.AddListener(() => SwitchCategory(AchievementCategory.All));
            if (beginnerTab) beginnerTab.onClick.AddListener(() => SwitchCategory(AchievementCategory.Beginner));
            if (masteryTab) masteryTab.onClick.AddListener(() => SwitchCategory(AchievementCategory.Mastery));
            if (victoriesTab) victoriesTab.onClick.AddListener(() => SwitchCategory(AchievementCategory.Victories));
            if (streaksTab) streaksTab.onClick.AddListener(() => SwitchCategory(AchievementCategory.Streaks));
            if (cashBattleTab) cashBattleTab.onClick.AddListener(() => SwitchCategory(AchievementCategory.CashBattle));
            if (tournamentsTab) tournamentsTab.onClick.AddListener(() => SwitchCategory(AchievementCategory.Tournaments));
            if (socialTab) socialTab.onClick.AddListener(() => SwitchCategory(AchievementCategory.Social));
            if (progressionTab) progressionTab.onClick.AddListener(() => SwitchCategory(AchievementCategory.Progression));
            if (collectorTab) collectorTab.onClick.AddListener(() => SwitchCategory(AchievementCategory.Collector));
            if (timeTab) timeTab.onClick.AddListener(() => SwitchCategory(AchievementCategory.Time));
            if (secretTab) secretTab.onClick.AddListener(() => SwitchCategory(AchievementCategory.Secret));

            // Subscribe to Service events
            var service = AchievementService.Instance;
            if (service != null)
            {
                service.OnAchievementProgress += OnServiceProgressUpdated;
                service.OnAchievementUnlocked += OnServiceAchievementUnlocked;
            }
        }

        #endregion

        #region Service Event Handlers

        private void OnServiceProgressUpdated(ServiceAchievementData svcData, float progress)
        {
            // Update local definition
            var def = allAchievements.Find(a => a.id == svcData.id);
            if (def != null)
            {
                def.currentProgress = AchievementService.Instance?.GetCurrentValue(svcData.id) ?? def.currentProgress;
            }

            // Refresh visible card
            var card = spawnedCards.Find(c => c.GetData()?.id == svcData.id);
            if (card != null)
            {
                card.RefreshProgress(def?.currentProgress ?? 0);
            }
        }

        private void OnServiceAchievementUnlocked(ServiceAchievementData svcData)
        {
            // Update local definition
            var def = allAchievements.Find(a => a.id == svcData.id);
            if (def != null)
            {
                def.isCompleted = true;
                def.currentProgress = svcData.targetValue;
            }

            // Animate visible card
            var card = spawnedCards.Find(c => c.GetData()?.id == svcData.id);
            if (card != null)
            {
                card.PlayUnlockAnimation();
            }

            // Update header stats
            UpdateHeaderStats();
        }

        #endregion

        #region Header Stats

        private void UpdateHeaderStats()
        {
            int totalPoints = 0;
            int earnedPoints = 0;
            int completed = 0;
            int total = allAchievements.Count;

            foreach (var achievement in allAchievements)
            {
                totalPoints += achievement.points;
                if (achievement.isCompleted)
                {
                    earnedPoints += achievement.points;
                    completed++;
                }
            }

            if (totalPointsText)
            {
                totalPointsText.text = $"{earnedPoints:N0}";
            }

            if (completionText)
            {
                int percentage = total > 0 ? (completed * 100 / total) : 0;
                completionText.text = $"{completed}/{total} ({percentage}%)";
            }

            if (overallProgressBar)
            {
                float progress = total > 0 ? (float)completed / total : 0f;
                overallProgressBar.DOValue(progress, 0.5f).SetEase(Ease.OutCubic);
            }
        }

        #endregion

        #region Category Tabs

        private void SwitchCategory(AchievementCategory category)
        {
            if (currentCategory == category) return;

            currentCategory = category;
            UpdateTabVisuals();
            LoadShowcase();

            // Scroll to top
            if (scrollRect)
            {
                scrollRect.DOVerticalNormalizedPos(1f, 0.3f);
            }
        }

        private void UpdateTabVisuals()
        {
            UpdateTabButton(allTab, currentCategory == AchievementCategory.All);
            UpdateTabButton(beginnerTab, currentCategory == AchievementCategory.Beginner);
            UpdateTabButton(masteryTab, currentCategory == AchievementCategory.Mastery);
            UpdateTabButton(victoriesTab, currentCategory == AchievementCategory.Victories);
            UpdateTabButton(streaksTab, currentCategory == AchievementCategory.Streaks);
            UpdateTabButton(cashBattleTab, currentCategory == AchievementCategory.CashBattle);
            UpdateTabButton(tournamentsTab, currentCategory == AchievementCategory.Tournaments);
            UpdateTabButton(socialTab, currentCategory == AchievementCategory.Social);
            UpdateTabButton(progressionTab, currentCategory == AchievementCategory.Progression);
            UpdateTabButton(collectorTab, currentCategory == AchievementCategory.Collector);
            UpdateTabButton(timeTab, currentCategory == AchievementCategory.Time);
            UpdateTabButton(secretTab, currentCategory == AchievementCategory.Secret);
        }

        private void UpdateTabButton(Button button, bool isActive)
        {
            if (button == null) return;

            var image = button.GetComponent<Image>();
            var text = button.GetComponentInChildren<TextMeshProUGUI>();

            if (image)
            {
                image.DOColor(isActive ? tabActiveColor : tabInactiveColor, 0.2f);
            }

            if (text)
            {
                text.DOColor(isActive ? tabActiveTextColor : tabInactiveTextColor, 0.2f);
            }

            // Scale animation
            button.transform.DOScale(isActive ? 1.05f : 1f, 0.2f).SetEase(Ease.OutCubic);
        }

        #endregion

        #region Trophy Showcase

        private void LoadShowcase()
        {
            ClearShowcase();

            var filtered = FilterAchievements();

            if (filtered.Count == 0)
            {
                ShowEmptyState();
                return;
            }

            HideEmptyState();

            // Create trophy cards with staggered animation
            for (int i = 0; i < filtered.Count; i++)
            {
                CreateTrophyCard(filtered[i], i * 0.05f);
            }
        }

        private List<AchievementDefinition> FilterAchievements()
        {
            return currentCategory switch
            {
                AchievementCategory.Beginner => allAchievements.FindAll(a => a.category == AchievementCategory.Beginner),
                AchievementCategory.Mastery => allAchievements.FindAll(a => a.category == AchievementCategory.Mastery),
                AchievementCategory.Victories => allAchievements.FindAll(a => a.category == AchievementCategory.Victories),
                AchievementCategory.Streaks => allAchievements.FindAll(a => a.category == AchievementCategory.Streaks),
                AchievementCategory.CashBattle => allAchievements.FindAll(a => a.category == AchievementCategory.CashBattle),
                AchievementCategory.Tournaments => allAchievements.FindAll(a => a.category == AchievementCategory.Tournaments),
                AchievementCategory.Social => allAchievements.FindAll(a => a.category == AchievementCategory.Social),
                AchievementCategory.Progression => allAchievements.FindAll(a => a.category == AchievementCategory.Progression),
                AchievementCategory.Collector => allAchievements.FindAll(a => a.category == AchievementCategory.Collector),
                AchievementCategory.Time => allAchievements.FindAll(a => a.category == AchievementCategory.Time),
                AchievementCategory.Secret => allAchievements.FindAll(a => a.isSecret), // Show all secrets
                _ => allAchievements.FindAll(a => !a.isSecret) // All shows non-secret achievements
            };
        }

        private void CreateTrophyCard(AchievementDefinition definition, float delay)
        {
            GameObject cardObj;

            if (trophyCardPrefab != null)
            {
                cardObj = Instantiate(trophyCardPrefab, showcaseContainer);
            }
            else
            {
                // Create fallback card
                cardObj = CreateFallbackTrophyCard();
                cardObj.transform.SetParent(showcaseContainer, false);
            }

            var card = cardObj.GetComponent<TrophyCardUI>();
            if (card == null)
            {
                card = cardObj.AddComponent<TrophyCardUI>();
            }

            // Convert to UI AchievementData
            var data = new DigitPark.UI.Items.AchievementData
            {
                id = definition.id,
                title = definition.title,
                description = definition.description,
                category = definition.category.ToString(),
                points = definition.points,
                targetProgress = definition.targetProgress,
                currentProgress = definition.currentProgress,
                isCompleted = definition.isCompleted,
                isClaimed = definition.isClaimed,
                isSecret = definition.isSecret,
                icon = LoadAchievementIcon(definition.iconName)
            };

            card.Setup(data);
            card.OnCardClicked += OnTrophyCardClicked;

            spawnedCards.Add(card);

            // Entrance animation
            cardObj.transform.localScale = Vector3.zero;
            cardObj.transform.DOScale(1f, 0.4f)
                .SetDelay(delay)
                .SetEase(Ease.OutBack);
        }

        private Sprite LoadAchievementIcon(string iconName)
        {
            if (string.IsNullOrEmpty(iconName))
            {
                return defaultTrophyIcon;
            }

            // Try to load from Resources
            var sprite = Resources.Load<Sprite>($"Icons/Achievements/{iconName}");
            if (sprite != null) return sprite;

            // Try loading from alternate path
            sprite = Resources.Load<Sprite>($"Achievements/{iconName}");
            if (sprite != null) return sprite;

            return defaultTrophyIcon;
        }

        private GameObject CreateFallbackTrophyCard()
        {
            // Create a basic card structure when no prefab is available
            var card = new GameObject("TrophyCard");
            var rt = card.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300f, 350f);

            // Background
            var bg = card.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.08f, 0.12f, 0.95f);

            return card;
        }

        private void ClearShowcase()
        {
            foreach (var card in spawnedCards)
            {
                if (card != null)
                {
                    card.OnCardClicked -= OnTrophyCardClicked;
                    Destroy(card.gameObject);
                }
            }
            spawnedCards.Clear();
        }

        private void ShowEmptyState()
        {
            if (emptyStateContainer)
            {
                emptyStateContainer.SetActive(true);

                if (emptyStateText)
                {
                    emptyStateText.text = currentCategory switch
                    {
                        AchievementCategory.Secret => AutoLocalizer.Get("ach_secret_empty"),
                        _ => AutoLocalizer.Get("ach_category_empty")
                    };
                }
            }
        }

        private void HideEmptyState()
        {
            if (emptyStateContainer)
            {
                emptyStateContainer.SetActive(false);
            }
        }

        #endregion

        #region Detail Panel

        private void OnTrophyCardClicked(TrophyCardUI card, DigitPark.UI.Items.AchievementData data)
        {
            selectedCard = card;
            selectedAchievement = data;
            ShowDetailPanel(data);
        }

        private void ShowDetailPanel(DigitPark.UI.Items.AchievementData data)
        {
            if (detailPanel == null) return;

            detailPanel.SetActive(true);

            // Update content
            if (detailTrophyIcon)
            {
                detailTrophyIcon.sprite = data.isSecret && !data.isCompleted ? secretTrophyIcon : data.icon ?? defaultTrophyIcon;
                detailTrophyIcon.color = data.isCompleted ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            }

            if (detailTitleText)
            {
                detailTitleText.text = data.isSecret && !data.isCompleted ? "???" : data.title;
            }

            if (detailDescriptionText)
            {
                detailDescriptionText.text = data.isSecret && !data.isCompleted
                    ? AutoLocalizer.Get("ach_secret_description_hidden")
                    : data.description;
            }

            if (detailCategoryText)
            {
                detailCategoryText.text = data.category.ToUpper();
            }

            if (detailProgressBar)
            {
                detailProgressBar.maxValue = data.targetProgress;
                detailProgressBar.value = data.currentProgress;
            }

            if (detailProgressText)
            {
                if (data.isCompleted)
                {
                    detailProgressText.text = AutoLocalizer.Get("completed");
                    detailProgressText.color = new Color(0f, 1f, 0.5f);
                }
                else
                {
                    detailProgressText.text = $"{data.currentProgress} / {data.targetProgress}";
                    detailProgressText.color = new Color(1f, 0.84f, 0f);
                }
            }

            if (detailPointsText)
            {
                detailPointsText.text = $"+{data.points} pts";
            }

            // Claim button
            if (claimRewardButton)
            {
                bool canClaim = data.isCompleted && !data.isClaimed;
                claimRewardButton.gameObject.SetActive(canClaim);
                claimRewardButton.interactable = canClaim;
            }

            // Particles
            if (detailParticles && data.isCompleted)
            {
                detailParticles.Play();
            }

            // Animate in
            AnimateDetailPanelIn();
        }

        private void AnimateDetailPanelIn()
        {
            if (detailBlocker)
            {
                detailBlocker.color = new Color(0, 0, 0, 0);
                detailBlocker.DOColor(new Color(0, 0, 0, 0.85f), 0.3f);
            }

            if (detailCard)
            {
                detailCard.localScale = Vector3.one * 0.8f;
                detailCard.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
            }

            if (detailPanelCanvasGroup)
            {
                detailPanelCanvasGroup.alpha = 0f;
                detailPanelCanvasGroup.DOFade(1f, 0.3f);
            }
        }

        private void CloseDetailPanel()
        {
            if (detailPanel == null) return;

            // Deselect card
            if (selectedCard != null)
            {
                selectedCard.SetSelected(false);
            }

            // Animate out
            Sequence seq = DOTween.Sequence();

            if (detailCard)
            {
                seq.Append(detailCard.DOScale(0.8f, 0.2f).SetEase(Ease.InCubic));
            }

            if (detailBlocker)
            {
                seq.Join(detailBlocker.DOColor(new Color(0, 0, 0, 0), 0.2f));
            }

            if (detailPanelCanvasGroup)
            {
                seq.Join(detailPanelCanvasGroup.DOFade(0f, 0.2f));
            }

            seq.OnComplete(() =>
            {
                detailPanel.SetActive(false);
                selectedAchievement = null;
                selectedCard = null;
            });
        }

        #endregion

        #region Rewards

        private void ClaimReward()
        {
            if (selectedAchievement == null || selectedAchievement.isClaimed) return;

            // Delegate to Service for persistence
            bool claimed = AchievementService.Instance?.ClaimReward(selectedAchievement.id) ?? false;

            if (claimed)
            {
                // Update local definition
                var definition = allAchievements.Find(a => a.id == selectedAchievement.id);
                if (definition != null)
                {
                    definition.isClaimed = true;
                }

                selectedAchievement.isClaimed = true;

                // Hide claim button
                if (claimRewardButton)
                {
                    claimRewardButton.interactable = false;
                    claimRewardButton.transform.DOScale(0f, 0.2f);
                }

                // Show celebration
                ShowRewardCelebration(selectedAchievement.points);

                // Update header
                UpdateHeaderStats();

                Debug.Log($"[Achievements] Claimed reward: {selectedAchievement.title} (+{selectedAchievement.points} pts)");
            }
        }

        private void ShowRewardCelebration(int points)
        {
            if (rewardCelebration == null) return;

            rewardCelebration.SetActive(true);

            if (rewardAmountText)
            {
                rewardAmountText.text = $"+{points}";
                rewardAmountText.transform.localScale = Vector3.zero;
                rewardAmountText.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
            }

            if (celebrationParticles)
            {
                celebrationParticles.Play();
            }

            if (celebrationGlow)
            {
                celebrationGlow.color = new Color(1f, 0.84f, 0f, 0f);
                celebrationGlow.DOColor(new Color(1f, 0.84f, 0f, 0.5f), 0.3f)
                    .SetLoops(4, LoopType.Yoyo);
            }

            // Hide after delay
            DOVirtual.DelayedCall(2.5f, () =>
            {
                if (rewardCelebration)
                {
                    rewardCelebration.SetActive(false);
                }
            });
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get total earned points
        /// </summary>
        public int GetTotalPoints()
        {
            return allAchievements.Where(a => a.isCompleted).Sum(a => a.points);
        }

        #endregion

        #region Navigation

        private void OnBackClicked()
        {
            SceneNavigator.Instance?.GoBack();
        }

        #endregion
    }

    /// <summary>
    /// Internal achievement definition for UI display
    /// </summary>
    [Serializable]
    public class AchievementDefinition
    {
        public string id;
        public string title;
        public string description;
        public AchievementsManager.AchievementCategory category;
        public int points;
        public int targetProgress;
        public int currentProgress;
        public bool isCompleted;
        public bool isClaimed;
        public bool isSecret;
        public string iconName;

        public AchievementDefinition(string id, string title, string description,
            AchievementsManager.AchievementCategory category, int points, int target,
            string iconName = null, bool isSecret = false)
        {
            this.id = id;
            this.title = title;
            this.description = description;
            this.category = category;
            this.points = points;
            this.targetProgress = target;
            this.iconName = iconName;
            this.isSecret = isSecret;
        }
    }
}
