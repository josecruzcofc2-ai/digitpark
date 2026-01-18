using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using DigitPark.Monetization;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager para la escena del Battle Pass.
    /// Sistema de progresión de temporada con recompensas gratuitas y premium.
    /// </summary>
    public class BattlePassManager : MonoBehaviour
    {
        [Header("UI - Header")]
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI seasonNameText;
        [SerializeField] private TextMeshProUGUI timeRemainingText;

        [Header("UI - Progress")]
        [SerializeField] private TextMeshProUGUI currentLevelText;
        [SerializeField] private TextMeshProUGUI currentXPText;
        [SerializeField] private Slider xpProgressBar;
        [SerializeField] private TextMeshProUGUI xpToNextLevelText;

        [Header("UI - Premium Status")]
        [SerializeField] private GameObject premiumBadge;
        [SerializeField] private Button buyPremiumButton;
        [SerializeField] private TextMeshProUGUI premiumPriceText;

        [Header("UI - Rewards Track")]
        [SerializeField] private ScrollRect rewardsScrollRect;
        [SerializeField] private Transform rewardsContainer;
        [SerializeField] private GameObject rewardTierPrefab;

        [Header("UI - Reward Detail")]
        [SerializeField] private GameObject rewardDetailPanel;
        [SerializeField] private Image rewardDetailIcon;
        [SerializeField] private TextMeshProUGUI rewardDetailNameText;
        [SerializeField] private TextMeshProUGUI rewardDetailDescText;
        [SerializeField] private TextMeshProUGUI rewardDetailTypeText;
        [SerializeField] private Button claimRewardButton;
        [SerializeField] private Button closeDetailButton;

        [Header("UI - Buy Premium Modal")]
        [SerializeField] private GameObject buyPremiumModal;
        [SerializeField] private TextMeshProUGUI premiumBenefitsText;
        [SerializeField] private Button confirmBuyButton;
        [SerializeField] private Button cancelBuyButton;

        [Header("UI - Reward Animation")]
        [SerializeField] private GameObject rewardClaimedPopup;
        [SerializeField] private Image rewardClaimedIcon;
        [SerializeField] private TextMeshProUGUI rewardClaimedText;

        [Header("Configuration")]
        [SerializeField] private string seasonName = "Temporada 1";
        [SerializeField] private int maxLevel = 50;
        [SerializeField] private int xpPerLevel = 1000;
        [SerializeField] private decimal premiumPrice = 9.99m;
        [SerializeField] private int seasonDurationDays = 60;

        // State
        private int currentLevel = 1;
        private int currentXP = 0;
        private bool hasPremium = false;
        private DateTime seasonEndDate;
        private List<BattlePassTier> tiers = new List<BattlePassTier>();
        private List<GameObject> spawnedTiers = new List<GameObject>();
        private BattlePassReward selectedReward;

        private void Start()
        {
            InitializeSeason();
            InitializeTiers();
            LoadProgress();
            SetupUI();
            SetupListeners();
            PopulateRewardTrack();
        }

        private void Update()
        {
            UpdateTimeRemaining();
        }

        private void InitializeSeason()
        {
            // Set season end date (in production, this would come from server)
            string savedEndDate = PlayerPrefs.GetString("BattlePass_SeasonEnd", "");
            if (string.IsNullOrEmpty(savedEndDate))
            {
                seasonEndDate = DateTime.Now.AddDays(seasonDurationDays);
                PlayerPrefs.SetString("BattlePass_SeasonEnd", seasonEndDate.ToString("o"));
                PlayerPrefs.Save();
            }
            else
            {
                seasonEndDate = DateTime.Parse(savedEndDate);
            }
        }

        private void InitializeTiers()
        {
            tiers.Clear();

            for (int i = 1; i <= maxLevel; i++)
            {
                var tier = new BattlePassTier
                {
                    level = i,
                    freeReward = GenerateFreeReward(i),
                    premiumReward = GeneratePremiumReward(i)
                };
                tiers.Add(tier);
            }
        }

        private BattlePassReward GenerateFreeReward(int level)
        {
            // Generate rewards based on level
            if (level % 10 == 0)
            {
                return new BattlePassReward
                {
                    id = $"free_{level}",
                    name = "Cofre de Recompensas",
                    type = RewardType.Chest,
                    amount = 1,
                    isPremium = false
                };
            }
            else if (level % 5 == 0)
            {
                return new BattlePassReward
                {
                    id = $"free_{level}",
                    name = $"{50 + level * 5} Monedas",
                    type = RewardType.Coins,
                    amount = 50 + level * 5,
                    isPremium = false
                };
            }
            else
            {
                return new BattlePassReward
                {
                    id = $"free_{level}",
                    name = $"{10 + level} Monedas",
                    type = RewardType.Coins,
                    amount = 10 + level,
                    isPremium = false
                };
            }
        }

        private BattlePassReward GeneratePremiumReward(int level)
        {
            if (level == maxLevel)
            {
                return new BattlePassReward
                {
                    id = $"premium_{level}",
                    name = "Skin Legendaria",
                    type = RewardType.Skin,
                    rarity = "Legendaria",
                    isPremium = true
                };
            }
            else if (level % 10 == 0)
            {
                return new BattlePassReward
                {
                    id = $"premium_{level}",
                    name = "Skin Épica",
                    type = RewardType.Skin,
                    rarity = "Épica",
                    isPremium = true
                };
            }
            else if (level % 5 == 0)
            {
                return new BattlePassReward
                {
                    id = $"premium_{level}",
                    name = $"{20 + level * 2} Gemas",
                    type = RewardType.Gems,
                    amount = 20 + level * 2,
                    isPremium = true
                };
            }
            else
            {
                return new BattlePassReward
                {
                    id = $"premium_{level}",
                    name = $"{25 + level * 3} Monedas",
                    type = RewardType.Coins,
                    amount = 25 + level * 3,
                    isPremium = true
                };
            }
        }

        private void LoadProgress()
        {
            currentLevel = PlayerPrefs.GetInt("BattlePass_Level", 1);
            currentXP = PlayerPrefs.GetInt("BattlePass_XP", 0);
            hasPremium = PlayerPrefs.GetInt("BattlePass_Premium", 0) == 1;

            // Load claimed rewards
            foreach (var tier in tiers)
            {
                tier.freeReward.isClaimed = PlayerPrefs.GetInt($"BattlePass_Claimed_{tier.freeReward.id}", 0) == 1;
                tier.premiumReward.isClaimed = PlayerPrefs.GetInt($"BattlePass_Claimed_{tier.premiumReward.id}", 0) == 1;
            }
        }

        private void SaveProgress()
        {
            PlayerPrefs.SetInt("BattlePass_Level", currentLevel);
            PlayerPrefs.SetInt("BattlePass_XP", currentXP);
            PlayerPrefs.SetInt("BattlePass_Premium", hasPremium ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void SetupUI()
        {
            if (rewardDetailPanel) rewardDetailPanel.SetActive(false);
            if (buyPremiumModal) buyPremiumModal.SetActive(false);
            if (rewardClaimedPopup) rewardClaimedPopup.SetActive(false);

            UpdateUI();
        }

        private void SetupListeners()
        {
            if (backButton) backButton.onClick.AddListener(OnBackClicked);
            if (buyPremiumButton) buyPremiumButton.onClick.AddListener(ShowBuyPremiumModal);
            if (closeDetailButton) closeDetailButton.onClick.AddListener(CloseRewardDetail);
            if (claimRewardButton) claimRewardButton.onClick.AddListener(ClaimSelectedReward);
            if (confirmBuyButton) confirmBuyButton.onClick.AddListener(BuyPremium);
            if (cancelBuyButton) cancelBuyButton.onClick.AddListener(CloseBuyPremiumModal);
        }

        private void UpdateUI()
        {
            // Season
            if (seasonNameText) seasonNameText.text = seasonName;

            // Level & XP
            if (currentLevelText) currentLevelText.text = $"Nivel {currentLevel}";

            int xpInCurrentLevel = currentXP % xpPerLevel;
            if (currentXPText) currentXPText.text = $"{xpInCurrentLevel}/{xpPerLevel} XP";

            if (xpProgressBar)
            {
                xpProgressBar.maxValue = xpPerLevel;
                xpProgressBar.value = xpInCurrentLevel;
            }

            if (xpToNextLevelText)
            {
                int needed = xpPerLevel - xpInCurrentLevel;
                xpToNextLevelText.text = $"{needed} XP para siguiente nivel";
            }

            // Premium status
            if (premiumBadge) premiumBadge.SetActive(hasPremium);
            if (buyPremiumButton) buyPremiumButton.gameObject.SetActive(!hasPremium);
            if (premiumPriceText) premiumPriceText.text = $"${premiumPrice:F2}";
        }

        private void UpdateTimeRemaining()
        {
            if (timeRemainingText == null) return;

            TimeSpan remaining = seasonEndDate - DateTime.Now;

            if (remaining.TotalSeconds <= 0)
            {
                timeRemainingText.text = "¡Temporada terminada!";
                timeRemainingText.color = new Color(1f, 0.4f, 0.4f);
            }
            else if (remaining.TotalDays >= 1)
            {
                timeRemainingText.text = $"{(int)remaining.TotalDays}d {remaining.Hours}h restantes";
            }
            else
            {
                timeRemainingText.text = $"{remaining.Hours}h {remaining.Minutes}m restantes";
                timeRemainingText.color = new Color(1f, 0.84f, 0f);
            }
        }

        private void PopulateRewardTrack()
        {
            ClearTiers();

            foreach (var tier in tiers)
            {
                CreateTierUI(tier);
            }

            // Scroll to current level
            ScrollToLevel(currentLevel);
        }

        private void CreateTierUI(BattlePassTier tier)
        {
            GameObject tierObj;

            if (rewardTierPrefab != null)
            {
                tierObj = Instantiate(rewardTierPrefab, rewardsContainer);
            }
            else
            {
                tierObj = CreateTierFallback(tier);
            }

            spawnedTiers.Add(tierObj);

            // Setup interactions
            SetupTierInteractions(tierObj, tier);
        }

        private GameObject CreateTierFallback(BattlePassTier tier)
        {
            var tierObj = new GameObject($"Tier_{tier.level}");
            tierObj.transform.SetParent(rewardsContainer, false);

            var rt = tierObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(120, 200);

            var layout = tierObj.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 5;
            layout.childAlignment = TextAnchor.MiddleCenter;

            // Level text
            var levelObj = new GameObject("Level");
            levelObj.transform.SetParent(tierObj.transform, false);
            var levelText = levelObj.AddComponent<TextMeshProUGUI>();
            levelText.text = tier.level.ToString();
            levelText.fontSize = 20;
            levelText.fontStyle = FontStyles.Bold;
            levelText.alignment = TextAlignmentOptions.Center;
            levelText.color = tier.level <= currentLevel ? new Color(0f, 1f, 0.5f) : Color.white;

            // Free reward
            CreateRewardButton(tierObj.transform, tier.freeReward, tier.level <= currentLevel, false);

            // Premium reward
            CreateRewardButton(tierObj.transform, tier.premiumReward, tier.level <= currentLevel && hasPremium, true);

            return tierObj;
        }

        private void CreateRewardButton(Transform parent, BattlePassReward reward, bool isUnlocked, bool isPremium)
        {
            var rewardObj = new GameObject(isPremium ? "Premium" : "Free");
            rewardObj.transform.SetParent(parent, false);

            var rt = rewardObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(80, 80);

            var image = rewardObj.AddComponent<Image>();
            if (reward.isClaimed)
            {
                image.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            }
            else if (isUnlocked)
            {
                image.color = isPremium ? new Color(1f, 0.84f, 0f, 0.3f) : new Color(0f, 0.83f, 1f, 0.3f);
            }
            else
            {
                image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            }

            var button = rewardObj.AddComponent<Button>();
            var r = reward;
            button.onClick.AddListener(() => ShowRewardDetail(r));

            // Reward name
            var nameObj = new GameObject("Name");
            nameObj.transform.SetParent(rewardObj.transform, false);
            var nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = Vector2.zero;
            nameRT.anchorMax = Vector2.one;
            nameRT.offsetMin = new Vector2(5, 5);
            nameRT.offsetMax = new Vector2(-5, -5);

            var nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = reward.isClaimed ? "✓" : reward.name;
            nameText.fontSize = reward.isClaimed ? 24 : 10;
            nameText.alignment = TextAlignmentOptions.Center;
        }

        private void SetupTierInteractions(GameObject tierObj, BattlePassTier tier)
        {
            // Find buttons and setup click handlers
            var buttons = tierObj.GetComponentsInChildren<Button>();
            foreach (var button in buttons)
            {
                if (button.gameObject.name == "Free")
                {
                    button.onClick.AddListener(() => ShowRewardDetail(tier.freeReward));
                }
                else if (button.gameObject.name == "Premium")
                {
                    button.onClick.AddListener(() => ShowRewardDetail(tier.premiumReward));
                }
            }
        }

        private void ClearTiers()
        {
            foreach (var tier in spawnedTiers)
            {
                if (tier) Destroy(tier);
            }
            spawnedTiers.Clear();
        }

        private void ScrollToLevel(int level)
        {
            if (rewardsScrollRect == null) return;

            float normalizedPosition = (float)(level - 1) / (maxLevel - 1);
            rewardsScrollRect.horizontalNormalizedPosition = Mathf.Clamp01(normalizedPosition - 0.2f);
        }

        private void ShowRewardDetail(BattlePassReward reward)
        {
            selectedReward = reward;

            if (rewardDetailPanel) rewardDetailPanel.SetActive(true);

            if (rewardDetailNameText) rewardDetailNameText.text = reward.name;
            if (rewardDetailDescText) rewardDetailDescText.text = GetRewardDescription(reward);
            if (rewardDetailTypeText)
            {
                rewardDetailTypeText.text = reward.isPremium ? "PREMIUM" : "GRATIS";
                rewardDetailTypeText.color = reward.isPremium ? new Color(1f, 0.84f, 0f) : new Color(0f, 0.83f, 1f);
            }

            // Check if claimable
            bool canClaim = !reward.isClaimed && IsRewardUnlocked(reward);
            if (claimRewardButton) claimRewardButton.gameObject.SetActive(canClaim);
        }

        private bool IsRewardUnlocked(BattlePassReward reward)
        {
            var tier = tiers.Find(t => t.freeReward.id == reward.id || t.premiumReward.id == reward.id);
            if (tier == null) return false;

            bool levelReached = tier.level <= currentLevel;
            bool premiumOk = !reward.isPremium || hasPremium;

            return levelReached && premiumOk;
        }

        private string GetRewardDescription(BattlePassReward reward)
        {
            return reward.type switch
            {
                RewardType.Coins => $"Obtén {reward.amount} monedas para usar en la tienda",
                RewardType.Gems => $"Obtén {reward.amount} gemas premium",
                RewardType.Chest => "Abre un cofre con recompensas aleatorias",
                RewardType.Skin => $"Skin {reward.rarity} exclusiva del Battle Pass",
                _ => "Recompensa especial"
            };
        }

        private void CloseRewardDetail()
        {
            if (rewardDetailPanel) rewardDetailPanel.SetActive(false);
            selectedReward = null;
        }

        private void ClaimSelectedReward()
        {
            if (selectedReward == null || selectedReward.isClaimed) return;

            selectedReward.isClaimed = true;
            PlayerPrefs.SetInt($"BattlePass_Claimed_{selectedReward.id}", 1);
            PlayerPrefs.Save();

            // Show claimed popup
            ShowRewardClaimedPopup(selectedReward);

            // Grant reward (would call CurrencyManager, etc.)
            GrantReward(selectedReward);

            // Update UI
            CloseRewardDetail();
            PopulateRewardTrack();
        }

        private void GrantReward(BattlePassReward reward)
        {
            switch (reward.type)
            {
                case RewardType.Coins:
                    CurrencyManager.Instance?.AddCoins(reward.amount);
                    break;
                case RewardType.Gems:
                    CurrencyManager.Instance?.AddGems(reward.amount);
                    break;
                default:
                    Debug.Log($"[BattlePass] Granted reward: {reward.name}");
                    break;
            }
        }

        private void ShowRewardClaimedPopup(BattlePassReward reward)
        {
            if (rewardClaimedPopup)
            {
                rewardClaimedPopup.SetActive(true);
                if (rewardClaimedText) rewardClaimedText.text = $"¡{reward.name}!";
                Invoke(nameof(HideRewardClaimedPopup), 2f);
            }
        }

        private void HideRewardClaimedPopup()
        {
            if (rewardClaimedPopup) rewardClaimedPopup.SetActive(false);
        }

        private void ShowBuyPremiumModal()
        {
            if (buyPremiumModal) buyPremiumModal.SetActive(true);
        }

        private void CloseBuyPremiumModal()
        {
            if (buyPremiumModal) buyPremiumModal.SetActive(false);
        }

        private void BuyPremium()
        {
            // In production, would process payment
            hasPremium = true;
            SaveProgress();
            CloseBuyPremiumModal();
            UpdateUI();
            PopulateRewardTrack();

            Debug.Log("[BattlePass] Premium purchased!");
        }

        /// <summary>
        /// Add XP to battle pass (call from game logic)
        /// </summary>
        public void AddXP(int amount)
        {
            currentXP += amount;

            int newLevel = 1 + (currentXP / xpPerLevel);
            if (newLevel > currentLevel && newLevel <= maxLevel)
            {
                currentLevel = newLevel;
                Debug.Log($"[BattlePass] Level up! Now level {currentLevel}");
            }

            currentLevel = Mathf.Min(currentLevel, maxLevel);
            SaveProgress();
            UpdateUI();
        }

        private void OnBackClicked()
        {
            SceneNavigator.Instance?.GoBack();
        }
    }

    [Serializable]
    public class BattlePassTier
    {
        public int level;
        public BattlePassReward freeReward;
        public BattlePassReward premiumReward;
    }

    [Serializable]
    public class BattlePassReward
    {
        public string id;
        public string name;
        public RewardType type;
        public int amount;
        public string rarity;
        public bool isPremium;
        public bool isClaimed;
    }

    public enum RewardType
    {
        Coins,
        Gems,
        Chest,
        Skin,
        Theme,
        Emote,
        Avatar
    }
}
