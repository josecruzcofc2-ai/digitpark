using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DigitPark.Data;
using DigitPark.Localization;

namespace DigitPark.UI.Items
{
    /// <summary>
    /// UI component for the MissionCard prefab.
    /// Populated at runtime from MissionDefinitionSO data.
    /// Follows the same pattern as MissionItemUI.
    /// </summary>
    public class MissionCardUI : MonoBehaviour
    {
        [Header("UI - Background")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image categoryBorder;
        [SerializeField] private Image difficultyIndicator;

        [Header("UI - Icon")]
        [SerializeField] private Image missionIcon;
        [SerializeField] private Image iconGlow;

        [Header("UI - Text")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI progressText;

        [Header("UI - Progress")]
        [SerializeField] private Slider progressBar;
        [SerializeField] private Image progressFill;

        [Header("UI - Reward")]
        [SerializeField] private Image rewardIcon;
        [SerializeField] private TextMeshProUGUI rewardAmountText;

        [Header("UI - Actions")]
        [SerializeField] private Button claimButton;
        [SerializeField] private TextMeshProUGUI claimButtonText;

        [Header("UI - State")]
        [SerializeField] private GameObject completedOverlay;
        [SerializeField] private GameObject claimedCheckmark;

        // Neon theme colors
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color PURPLE_WEEKLY = new Color(0.6f, 0.2f, 1f, 1f);
        private static readonly Color GOLD_SPECIAL = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GREEN_SUCCESS = new Color(0.2f, 0.9f, 0.4f, 1f);
        private static readonly Color COIN_COLOR = new Color(1f, 0.85f, 0.3f, 1f);
        private static readonly Color GEM_COLOR = new Color(0.4f, 0.8f, 1f, 1f);
        private static readonly Color CARD_BG = new Color(0.06f, 0.08f, 0.12f, 0.95f);
        private static readonly Color CARD_BG_COMPLETED = new Color(0.08f, 0.2f, 0.1f, 0.95f);
        private static readonly Color CARD_BG_CLAIMED = new Color(0.08f, 0.08f, 0.1f, 0.6f);

        private static readonly Color EASY_COLOR = new Color(0.3f, 0.8f, 0.3f, 1f);
        private static readonly Color MEDIUM_COLOR = new Color(0.9f, 0.7f, 0.2f, 1f);
        private static readonly Color HARD_COLOR = new Color(0.9f, 0.3f, 0.3f, 1f);

        private string missionId;
        private Action<string> onClaimClicked;

        /// <summary>
        /// Popula la card desde un MissionDefinitionSO + estado runtime.
        /// </summary>
        public void Setup(MissionDefinitionSO definition, int currentProgress, MissionState state,
            Sprite coinIcon, Sprite gemIcon, Action<string> onClaim)
        {
            if (definition == null) return;

            missionId = definition.missionId;
            onClaimClicked = onClaim;

            // Title & Description (localized)
            string title = L(definition.titleLocKey);
            string description = L(definition.descriptionLocKey);

            if (titleText) titleText.text = title;
            if (descriptionText) descriptionText.text = description;

            // Icon
            if (missionIcon && definition.icon != null)
                missionIcon.sprite = definition.icon;

            if (iconGlow)
                iconGlow.color = GetCategoryColor(definition.category) * new Color(1f, 1f, 1f, 0.3f);

            // Progress
            int clamped = Mathf.Min(currentProgress, definition.targetAmount);
            if (progressBar)
            {
                progressBar.maxValue = definition.targetAmount;
                progressBar.value = clamped;
            }

            if (progressFill)
            {
                progressFill.color = state == MissionState.Completed || state == MissionState.Claimed
                    ? GREEN_SUCCESS
                    : GetCategoryColor(definition.category);
            }

            if (progressText)
            {
                if (state == MissionState.Claimed)
                    progressText.text = L("ms_completed");
                else if (state == MissionState.Completed)
                    progressText.text = L("ms_ready_claim");
                else
                    progressText.text = $"{clamped}/{definition.targetAmount}";
            }

            // Category border color
            Color categoryColor = GetCategoryColor(definition.category);
            if (categoryBorder)
                categoryBorder.color = categoryColor;

            // Difficulty indicator
            if (difficultyIndicator)
                difficultyIndicator.color = GetDifficultyColor(definition.difficulty);

            // Reward
            if (rewardIcon)
            {
                rewardIcon.sprite = definition.rewardType == MissionRewardType.Gems ? gemIcon : coinIcon;
            }

            if (rewardAmountText)
            {
                rewardAmountText.text = $"+{definition.rewardAmount}";
                rewardAmountText.color = definition.rewardType == MissionRewardType.Gems ? GEM_COLOR : COIN_COLOR;
            }

            // Background state
            UpdateVisualState(state, definition.category);

            // Claim button
            if (claimButton)
            {
                claimButton.onClick.RemoveAllListeners();
                claimButton.onClick.AddListener(OnClaimButtonClicked);
                claimButton.gameObject.SetActive(state == MissionState.Completed);
            }

            if (claimButtonText)
                claimButtonText.text = L("ms_claim_button");

            // Overlays
            if (completedOverlay)
                completedOverlay.SetActive(state == MissionState.Claimed);

            if (claimedCheckmark)
                claimedCheckmark.SetActive(state == MissionState.Claimed);
        }

        private void UpdateVisualState(MissionState state, MissionCategory category)
        {
            if (backgroundImage == null) return;

            switch (state)
            {
                case MissionState.Claimed:
                    backgroundImage.color = CARD_BG_CLAIMED;
                    break;
                case MissionState.Completed:
                    backgroundImage.color = CARD_BG_COMPLETED;
                    break;
                default:
                    // Tint sutil por categoria
                    Color catColor = GetCategoryColor(category);
                    backgroundImage.color = new Color(
                        CARD_BG.r + catColor.r * 0.02f,
                        CARD_BG.g + catColor.g * 0.01f,
                        CARD_BG.b + catColor.b * 0.03f,
                        CARD_BG.a);
                    break;
            }

            // Fade si claimed
            if (state == MissionState.Claimed)
            {
                if (rewardAmountText) rewardAmountText.color = new Color(0.4f, 0.4f, 0.4f, 1f);
                if (rewardIcon) rewardIcon.color = new Color(1f, 1f, 1f, 0.35f);
                if (titleText) titleText.color = new Color(0.45f, 0.45f, 0.45f, 1f);
            }
        }

        private void OnClaimButtonClicked()
        {
            onClaimClicked?.Invoke(missionId);
        }

        public void PlayClaimAnimation()
        {
            if (claimButton)
                claimButton.gameObject.SetActive(false);

            if (claimedCheckmark)
                claimedCheckmark.SetActive(true);

            if (backgroundImage)
                backgroundImage.color = CARD_BG_COMPLETED;
        }

        private Color GetCategoryColor(MissionCategory category)
        {
            return category switch
            {
                MissionCategory.Weekly => PURPLE_WEEKLY,
                MissionCategory.Special => GOLD_SPECIAL,
                _ => CYAN_NEON
            };
        }

        private Color GetDifficultyColor(MissionDifficulty difficulty)
        {
            return difficulty switch
            {
                MissionDifficulty.Easy => EASY_COLOR,
                MissionDifficulty.Medium => MEDIUM_COLOR,
                MissionDifficulty.Hard => HARD_COLOR,
                _ => EASY_COLOR
            };
        }

        private string L(string key)
        {
            if (string.IsNullOrEmpty(key)) return key;
            if (LocalizationManager.Instance == null) return key;
            return LocalizationManager.Instance.GetText(key);
        }
    }
}
