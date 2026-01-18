using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace DigitPark.UI.Items
{
    /// <summary>
    /// UI component for battle pass reward tier prefab.
    /// Displays tier number, free/premium rewards and claim state.
    /// </summary>
    public class RewardTierItemUI : MonoBehaviour
    {
        [Header("UI - Tier Info")]
        [SerializeField] private TextMeshProUGUI tierNumberText;
        [SerializeField] private Image tierBackground;
        [SerializeField] private GameObject currentTierIndicator;

        [Header("UI - Free Reward")]
        [SerializeField] private Image freeRewardIcon;
        [SerializeField] private TextMeshProUGUI freeRewardText;
        [SerializeField] private Button freeClaimButton;
        [SerializeField] private GameObject freeClaimedCheck;
        [SerializeField] private GameObject freeLockedOverlay;

        [Header("UI - Premium Reward")]
        [SerializeField] private Image premiumRewardIcon;
        [SerializeField] private TextMeshProUGUI premiumRewardText;
        [SerializeField] private Button premiumClaimButton;
        [SerializeField] private GameObject premiumClaimedCheck;
        [SerializeField] private GameObject premiumLockedOverlay;
        [SerializeField] private Image premiumBorder;

        [Header("Visual Settings")]
        [SerializeField] private Color unlockedColor = new Color(0.2f, 0.2f, 0.25f, 1f);
        [SerializeField] private Color currentColor = new Color(0f, 0.5f, 0.8f, 1f);
        [SerializeField] private Color lockedColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        [SerializeField] private Color premiumColor = new Color(0.8f, 0.6f, 0f, 1f);

        private int tierNumber;
        private Action<int, bool> onClaimClicked; // tier, isPremium

        public void Setup(RewardTierDisplayData data, Action<int, bool> claimCallback = null)
        {
            tierNumber = data.tierNumber;
            onClaimClicked = claimCallback;

            // Tier info
            if (tierNumberText)
                tierNumberText.text = data.tierNumber.ToString();

            if (currentTierIndicator)
                currentTierIndicator.SetActive(data.isCurrentTier);

            // Free reward
            SetupReward(
                freeRewardIcon, freeRewardText, freeClaimButton,
                freeClaimedCheck, freeLockedOverlay,
                data.freeReward, data.freeRewardClaimed, data.isUnlocked, false
            );

            // Premium reward
            SetupReward(
                premiumRewardIcon, premiumRewardText, premiumClaimButton,
                premiumClaimedCheck, premiumLockedOverlay,
                data.premiumReward, data.premiumRewardClaimed,
                data.isUnlocked && data.hasPremiumPass, true
            );

            // Premium border glow
            if (premiumBorder)
                premiumBorder.color = data.hasPremiumPass ? premiumColor : lockedColor;

            // Background color
            if (tierBackground)
            {
                if (data.isCurrentTier)
                    tierBackground.color = currentColor;
                else if (data.isUnlocked)
                    tierBackground.color = unlockedColor;
                else
                    tierBackground.color = lockedColor;
            }
        }

        private void SetupReward(Image icon, TextMeshProUGUI text, Button button,
            GameObject claimedCheck, GameObject lockedOverlay,
            RewardInfo reward, bool isClaimed, bool isUnlocked, bool isPremium)
        {
            if (icon && reward?.icon != null)
                icon.sprite = reward.icon;

            if (text)
                text.text = reward != null ? $"x{reward.amount}" : "";

            if (claimedCheck)
                claimedCheck.SetActive(isClaimed);

            if (lockedOverlay)
                lockedOverlay.SetActive(!isUnlocked);

            if (button)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnClaimReward(isPremium));
                button.gameObject.SetActive(isUnlocked && !isClaimed);
                button.interactable = isUnlocked && !isClaimed;
            }
        }

        private void OnClaimReward(bool isPremium)
        {
            onClaimClicked?.Invoke(tierNumber, isPremium);
        }

        public void PlayClaimAnimation(bool isPremium)
        {
            GameObject target = isPremium ? premiumClaimButton?.gameObject : freeClaimButton?.gameObject;
            GameObject check = isPremium ? premiumClaimedCheck : freeClaimedCheck;

            if (target != null)
                target.SetActive(false);
            if (check != null)
                check.SetActive(true);
        }
    }

    [Serializable]
    public class RewardTierDisplayData
    {
        public int tierNumber;
        public bool isUnlocked;
        public bool isCurrentTier;
        public bool hasPremiumPass;
        public RewardInfo freeReward;
        public RewardInfo premiumReward;
        public bool freeRewardClaimed;
        public bool premiumRewardClaimed;
    }

    [Serializable]
    public class RewardInfo
    {
        public Sprite icon;
        public string type;
        public int amount;
    }
}
