using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace DigitPark.UI.Items
{
    /// <summary>
    /// UI component for daily reward day item prefab.
    /// Displays day number, reward icon, amount and claim state.
    /// </summary>
    public class RewardDayItemUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI dayNumberText;
        [SerializeField] private TextMeshProUGUI dayLabelText;
        [SerializeField] private Image rewardIcon;
        [SerializeField] private TextMeshProUGUI rewardAmountText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image borderImage;
        [SerializeField] private Button claimButton;

        [Header("UI - State Indicators")]
        [SerializeField] private GameObject claimedCheckmark;
        [SerializeField] private GameObject currentDayGlow;
        [SerializeField] private GameObject lockedOverlay;
        [SerializeField] private GameObject bonusTag;
        [SerializeField] private TextMeshProUGUI bonusTagText;

        [Header("Visual Settings")]
        [SerializeField] private Color availableColor = new Color(0.15f, 0.4f, 0.15f, 1f);
        [SerializeField] private Color claimedColor = new Color(0.2f, 0.2f, 0.25f, 1f);
        [SerializeField] private Color lockedColor = new Color(0.12f, 0.12f, 0.15f, 0.9f);
        [SerializeField] private Color currentBorderColor = new Color(1f, 0.84f, 0f, 1f);
        [SerializeField] private Color normalBorderColor = new Color(0.3f, 0.3f, 0.35f, 1f);

        private int dayNumber;
        private Action<int> onClaimClicked;

        public void Setup(RewardDayDisplayData data, Action<int> claimCallback = null)
        {
            dayNumber = data.dayNumber;
            onClaimClicked = claimCallback;

            // Day info
            if (dayNumberText)
                dayNumberText.text = data.dayNumber.ToString();

            if (dayLabelText)
                dayLabelText.text = $"Dia {data.dayNumber}";

            // Reward
            if (rewardIcon && data.rewardIcon != null)
                rewardIcon.sprite = data.rewardIcon;

            if (rewardAmountText)
                rewardAmountText.text = data.isBonusDay ? $"+{data.rewardAmount}!" : $"+{data.rewardAmount}";

            // Bonus tag
            if (bonusTag)
            {
                bonusTag.SetActive(data.isBonusDay);
                if (bonusTagText && data.isBonusDay)
                    bonusTagText.text = "BONUS";
            }

            // Visual state
            UpdateVisualState(data.state, data.isCurrentDay);

            // Claim button
            if (claimButton)
            {
                claimButton.onClick.RemoveAllListeners();
                claimButton.onClick.AddListener(OnClaimButtonClicked);
                claimButton.interactable = data.state == RewardDayState.Available;
            }
        }

        private void UpdateVisualState(RewardDayState state, bool isCurrentDay)
        {
            // Background
            if (backgroundImage)
            {
                switch (state)
                {
                    case RewardDayState.Locked:
                        backgroundImage.color = lockedColor;
                        break;
                    case RewardDayState.Available:
                        backgroundImage.color = availableColor;
                        break;
                    case RewardDayState.Claimed:
                        backgroundImage.color = claimedColor;
                        break;
                }
            }

            // Border
            if (borderImage)
                borderImage.color = isCurrentDay ? currentBorderColor : normalBorderColor;

            // Indicators
            if (currentDayGlow)
                currentDayGlow.SetActive(isCurrentDay && state == RewardDayState.Available);

            if (lockedOverlay)
                lockedOverlay.SetActive(state == RewardDayState.Locked);

            if (claimedCheckmark)
                claimedCheckmark.SetActive(state == RewardDayState.Claimed);

            // Dim icon if locked
            if (rewardIcon)
            {
                var color = rewardIcon.color;
                color.a = state == RewardDayState.Locked ? 0.4f : 1f;
                rewardIcon.color = color;
            }
        }

        private void OnClaimButtonClicked()
        {
            onClaimClicked?.Invoke(dayNumber);
        }

        public void PlayClaimAnimation()
        {
            if (claimedCheckmark)
            {
                claimedCheckmark.SetActive(true);
                claimedCheckmark.transform.localScale = Vector3.one;
            }

            if (currentDayGlow)
                currentDayGlow.SetActive(false);

            if (backgroundImage)
                backgroundImage.color = claimedColor;
        }
    }

    [Serializable]
    public class RewardDayDisplayData
    {
        public int dayNumber;
        public Sprite rewardIcon;
        public int rewardAmount;
        public string rewardType;
        public RewardDayState state;
        public bool isCurrentDay;
        public bool isBonusDay;
    }

    public enum RewardDayState
    {
        Locked,
        Available,
        Claimed
    }
}
