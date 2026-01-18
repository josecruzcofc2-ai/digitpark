using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace DigitPark.UI.Items
{
    /// <summary>
    /// UI component for daily mission item prefab.
    /// Displays mission icon, description, progress and reward.
    /// </summary>
    public class MissionItemUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image missionIcon;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Slider progressBar;
        [SerializeField] private Image progressFill;

        [Header("UI - Reward")]
        [SerializeField] private Image rewardIcon;
        [SerializeField] private TextMeshProUGUI rewardAmountText;
        [SerializeField] private Button claimButton;
        [SerializeField] private TextMeshProUGUI claimButtonText;

        [Header("UI - State")]
        [SerializeField] private GameObject completedOverlay;
        [SerializeField] private GameObject claimedCheckmark;
        [SerializeField] private Image difficultyIndicator;

        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = new Color(0.18f, 0.18f, 0.22f, 1f);
        [SerializeField] private Color completedColor = new Color(0.1f, 0.35f, 0.15f, 1f);
        [SerializeField] private Color progressColor = new Color(0f, 0.8f, 0.4f, 1f);
        [SerializeField] private Color easyColor = new Color(0.3f, 0.8f, 0.3f);
        [SerializeField] private Color mediumColor = new Color(0.9f, 0.7f, 0.2f);
        [SerializeField] private Color hardColor = new Color(0.9f, 0.3f, 0.3f);

        private string missionId;
        private Action<string> onClaimClicked;

        public void Setup(MissionDisplayData data, Action<string> claimCallback = null)
        {
            missionId = data.id;
            onClaimClicked = claimCallback;

            // Basic info
            if (titleText)
                titleText.text = data.title;

            if (descriptionText)
                descriptionText.text = data.description;

            if (missionIcon && data.icon != null)
                missionIcon.sprite = data.icon;

            // Progress
            if (progressBar)
            {
                progressBar.maxValue = data.targetValue;
                progressBar.value = data.currentValue;
            }

            if (progressText)
                progressText.text = $"{data.currentValue}/{data.targetValue}";

            if (progressFill)
                progressFill.color = progressColor;

            // Reward
            if (rewardIcon && data.rewardIcon != null)
                rewardIcon.sprite = data.rewardIcon;

            if (rewardAmountText)
                rewardAmountText.text = $"+{data.rewardAmount}";

            // Difficulty
            if (difficultyIndicator)
            {
                switch (data.difficulty)
                {
                    case MissionDifficulty.Easy:
                        difficultyIndicator.color = easyColor;
                        break;
                    case MissionDifficulty.Medium:
                        difficultyIndicator.color = mediumColor;
                        break;
                    case MissionDifficulty.Hard:
                        difficultyIndicator.color = hardColor;
                        break;
                }
            }

            // State
            UpdateVisualState(data.state);

            // Claim button
            if (claimButton)
            {
                claimButton.onClick.RemoveAllListeners();
                claimButton.onClick.AddListener(OnClaimButtonClicked);
                claimButton.gameObject.SetActive(data.state == MissionState.Completed);
            }

            if (claimButtonText)
                claimButtonText.text = "Reclamar";
        }

        private void UpdateVisualState(MissionState state)
        {
            if (backgroundImage)
            {
                backgroundImage.color = state == MissionState.Completed || state == MissionState.Claimed
                    ? completedColor
                    : normalColor;
            }

            if (completedOverlay)
                completedOverlay.SetActive(state == MissionState.Claimed);

            if (claimedCheckmark)
                claimedCheckmark.SetActive(state == MissionState.Claimed);

            // Fade progress bar if claimed
            if (progressBar && state == MissionState.Claimed)
            {
                var canvasGroup = progressBar.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = progressBar.gameObject.AddComponent<CanvasGroup>();
                canvasGroup.alpha = 0.5f;
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
                backgroundImage.color = completedColor;
        }
    }

    [Serializable]
    public class MissionDisplayData
    {
        public string id;
        public string title;
        public string description;
        public Sprite icon;
        public int currentValue;
        public int targetValue;
        public int rewardAmount;
        public Sprite rewardIcon;
        public MissionState state;
        public MissionDifficulty difficulty;
        public MissionCategory category;
    }

    public enum MissionState
    {
        InProgress,
        Completed,
        Claimed
    }

    public enum MissionDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    public enum MissionCategory
    {
        Daily,
        Weekly,
        Special
    }
}
