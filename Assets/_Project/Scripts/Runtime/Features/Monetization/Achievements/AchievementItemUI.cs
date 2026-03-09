using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DigitPark.Localization;

namespace DigitPark.UI.Items
{
    /// <summary>
    /// UI component for achievement item prefab.
    /// Displays achievement icon, name, description, progress and rewards.
    /// </summary>
    public class AchievementItemUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private Image rewardIcon;
        [SerializeField] private Button claimButton;
        [SerializeField] private TextMeshProUGUI claimButtonText;
        [SerializeField] private GameObject completedOverlay;
        [SerializeField] private GameObject lockedOverlay;

        [Header("Visual Settings")]
        [SerializeField] private Color unlockedColor = new Color(0.2f, 0.2f, 0.25f, 1f);
        [SerializeField] private Color completedColor = new Color(0.1f, 0.3f, 0.1f, 1f);
        [SerializeField] private Color lockedColor = new Color(0.15f, 0.15f, 0.15f, 0.8f);

        private string achievementId;
        private Action<string> onClaimClicked;

        public void Setup(AchievementDisplayData data, Action<string> claimCallback = null)
        {
            achievementId = data.id;
            onClaimClicked = claimCallback;

            // Set texts
            if (nameText) nameText.text = data.name;
            if (descriptionText) descriptionText.text = data.description;

            // Set icon
            if (iconImage && data.icon != null)
                iconImage.sprite = data.icon;

            // Set progress
            if (progressBar)
            {
                progressBar.maxValue = data.targetValue;
                progressBar.value = data.currentValue;
            }

            if (progressText)
                progressText.text = $"{data.currentValue}/{data.targetValue}";

            // Set reward
            if (rewardText)
                rewardText.text = $"+{data.rewardAmount}";

            if (rewardIcon && data.rewardIcon != null)
                rewardIcon.sprite = data.rewardIcon;

            // Set visual state
            UpdateVisualState(data.state);

            // Setup claim button
            if (claimButton)
            {
                claimButton.onClick.RemoveAllListeners();
                claimButton.onClick.AddListener(OnClaimButtonClicked);
                claimButton.gameObject.SetActive(data.state == AchievementState.Completed);
            }

            if (claimButtonText)
                claimButtonText.text = AutoLocalizer.Get("claim_button");
        }

        private void UpdateVisualState(AchievementState state)
        {
            if (backgroundImage)
            {
                switch (state)
                {
                    case AchievementState.Locked:
                        backgroundImage.color = lockedColor;
                        break;
                    case AchievementState.InProgress:
                        backgroundImage.color = unlockedColor;
                        break;
                    case AchievementState.Completed:
                        backgroundImage.color = completedColor;
                        break;
                    case AchievementState.Claimed:
                        backgroundImage.color = completedColor;
                        break;
                }
            }

            if (lockedOverlay)
                lockedOverlay.SetActive(state == AchievementState.Locked);

            if (completedOverlay)
                completedOverlay.SetActive(state == AchievementState.Claimed);

            // Dim progress for completed
            if (progressBar)
            {
                var colors = progressBar.colors;
                colors.disabledColor = state == AchievementState.Claimed ? Color.gray : Color.white;
                progressBar.colors = colors;
            }
        }

        private void OnClaimButtonClicked()
        {
            onClaimClicked?.Invoke(achievementId);
        }

        /// <summary>
        /// Plays claim animation and updates visual state
        /// </summary>
        public void PlayClaimAnimation()
        {
            if (claimButton)
                claimButton.gameObject.SetActive(false);

            if (completedOverlay)
                completedOverlay.SetActive(true);
        }
    }

    /// <summary>
    /// Data structure for displaying an achievement
    /// </summary>
    [Serializable]
    public class AchievementDisplayData
    {
        public string id;
        public string name;
        public string description;
        public Sprite icon;
        public int currentValue;
        public int targetValue;
        public int rewardAmount;
        public Sprite rewardIcon;
        public AchievementState state;
        public string category;
    }

    public enum AchievementState
    {
        Locked,
        InProgress,
        Completed,
        Claimed
    }
}
