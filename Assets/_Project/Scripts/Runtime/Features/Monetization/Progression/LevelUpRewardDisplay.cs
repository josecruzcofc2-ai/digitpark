using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DigitPark.Progression
{
    /// <summary>
    /// Componente hijo del LevelUpPanel para mostrar la reward card
    /// según el tipo de recompensa (Frame, Avatar, Title, DigitCoins, etc.).
    /// </summary>
    public class LevelUpRewardDisplay : MonoBehaviour
    {
        [SerializeField] private Image rewardIcon;
        [SerializeField] private TextMeshProUGUI rewardNameText;
        [SerializeField] private TextMeshProUGUI coinAmountText;
        [SerializeField] private Image framePlaceholder;

        /// <summary>
        /// Configure the display based on the reward type.
        /// Called by LevelUpPanel during Fase 3.
        /// </summary>
        public void Setup(LevelReward reward)
        {
            if (reward == null) return;

            // Hide optional elements
            if (coinAmountText != null) coinAmountText.gameObject.SetActive(false);
            if (framePlaceholder != null) framePlaceholder.gameObject.SetActive(false);

            switch (reward.type)
            {
                case RewardType.DigitCoins:
                case RewardType.DigitGems:
                    SetupCurrencyReward(reward);
                    break;

                case RewardType.Frame:
                    SetupFrameReward(reward);
                    break;

                case RewardType.Avatar:
                    SetupAvatarReward(reward);
                    break;

                case RewardType.Title:
                    SetupTitleReward(reward);
                    break;

                default:
                    SetupGenericReward(reward);
                    break;
            }
        }

        private void SetupCurrencyReward(LevelReward reward)
        {
            // Show coin icon + animated amount
            string iconPath = reward.type == RewardType.DigitCoins
                ? "Icons/Currency/digitcoin"
                : "Icons/Currency/digitgem";

            var sprite = Resources.Load<Sprite>(iconPath);
            if (rewardIcon != null && sprite != null) rewardIcon.sprite = sprite;

            if (coinAmountText != null)
            {
                coinAmountText.gameObject.SetActive(true);
                // Parse amount from rewardId
                if (int.TryParse(reward.rewardId, out int amount))
                    coinAmountText.text = $"+{amount:N0}";
                else
                    coinAmountText.text = reward.name;
            }

            if (rewardNameText != null) rewardNameText.text = reward.name;
        }

        private void SetupFrameReward(LevelReward reward)
        {
            var sprite = Resources.Load<Sprite>($"Icons/Frames/{reward.rewardId}");
            if (rewardIcon != null && sprite != null) rewardIcon.sprite = sprite;

            // Show frame preview placeholder
            if (framePlaceholder != null)
            {
                framePlaceholder.gameObject.SetActive(true);
                if (sprite != null) framePlaceholder.sprite = sprite;
            }

            if (rewardNameText != null) rewardNameText.text = reward.name;
        }

        private void SetupAvatarReward(LevelReward reward)
        {
            var sprite = Resources.Load<Sprite>($"Icons/Avatars/{reward.rewardId}");
            if (rewardIcon != null && sprite != null) rewardIcon.sprite = sprite;
            if (rewardNameText != null) rewardNameText.text = reward.name;
        }

        private void SetupTitleReward(LevelReward reward)
        {
            // TODO: conectar con TitleService cuando esté implementado
            var sprite = Resources.Load<Sprite>($"Icons/Titles/{reward.rewardId}");
            if (rewardIcon != null && sprite != null) rewardIcon.sprite = sprite;
            if (rewardNameText != null) rewardNameText.text = reward.name;
        }

        private void SetupGenericReward(LevelReward reward)
        {
            var sprite = Resources.Load<Sprite>($"Icons/Rewards/{reward.rewardId}");
            if (rewardIcon != null && sprite != null) rewardIcon.sprite = sprite;
            if (rewardNameText != null) rewardNameText.text = reward.name;
        }
    }
}
