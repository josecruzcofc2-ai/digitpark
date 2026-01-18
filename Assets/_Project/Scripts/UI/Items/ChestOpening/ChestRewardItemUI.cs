using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace DigitPark.UI.Items
{
    /// <summary>
    /// UI component for chest reward item prefab.
    /// Displays reward icon, name, amount and rarity.
    /// </summary>
    public class ChestRewardItemUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image rewardIcon;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image borderImage;
        [SerializeField] private Image glowImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private TextMeshProUGUI rarityText;
        [SerializeField] private GameObject newTag;

        [Header("Particle Effects")]
        [SerializeField] private ParticleSystem revealParticles;
        [SerializeField] private ParticleSystem rarityParticles;

        [Header("Rarity Colors")]
        [SerializeField] private Color commonColor = new Color(0.6f, 0.6f, 0.6f);
        [SerializeField] private Color uncommonColor = new Color(0.3f, 0.8f, 0.3f);
        [SerializeField] private Color rareColor = new Color(0.3f, 0.5f, 1f);
        [SerializeField] private Color epicColor = new Color(0.7f, 0.3f, 0.9f);
        [SerializeField] private Color legendaryColor = new Color(1f, 0.7f, 0.2f);

        [Header("Animation Settings")]
        [SerializeField] private float revealDuration = 0.5f;
        [SerializeField] private float glowPulseDuration = 1f;

        private ChestRewardRarity currentRarity;

        public void Setup(ChestRewardDisplayData data)
        {
            currentRarity = data.rarity;

            // Reward info
            if (rewardIcon && data.icon != null)
                rewardIcon.sprite = data.icon;

            if (nameText)
                nameText.text = data.name;

            if (amountText)
                amountText.text = data.amount > 1 ? $"x{data.amount}" : "";

            if (rarityText)
                rarityText.text = GetRarityText(data.rarity);

            // New tag
            if (newTag)
                newTag.SetActive(data.isNew);

            // Apply rarity visuals
            ApplyRarityVisuals(data.rarity);
        }

        private void ApplyRarityVisuals(ChestRewardRarity rarity)
        {
            Color rarityColor = GetRarityColor(rarity);

            if (borderImage)
                borderImage.color = rarityColor;

            if (glowImage)
            {
                glowImage.color = new Color(rarityColor.r, rarityColor.g, rarityColor.b, 0.5f);

                // Pulse animation for rare+ (requires LeanTween or DOTween)
                // if (rarity >= ChestRewardRarity.Rare) { /* Add animation here */ }
            }

            if (rarityText)
                rarityText.color = rarityColor;

            // Background tint
            if (backgroundImage)
            {
                Color bgColor = Color.Lerp(new Color(0.15f, 0.15f, 0.2f), rarityColor, 0.15f);
                backgroundImage.color = bgColor;
            }
        }

        private Color GetRarityColor(ChestRewardRarity rarity)
        {
            switch (rarity)
            {
                case ChestRewardRarity.Common: return commonColor;
                case ChestRewardRarity.Uncommon: return uncommonColor;
                case ChestRewardRarity.Rare: return rareColor;
                case ChestRewardRarity.Epic: return epicColor;
                case ChestRewardRarity.Legendary: return legendaryColor;
                default: return commonColor;
            }
        }

        private string GetRarityText(ChestRewardRarity rarity)
        {
            switch (rarity)
            {
                case ChestRewardRarity.Common: return "Comun";
                case ChestRewardRarity.Uncommon: return "Poco Comun";
                case ChestRewardRarity.Rare: return "Raro";
                case ChestRewardRarity.Epic: return "Epico";
                case ChestRewardRarity.Legendary: return "Legendario";
                default: return "";
            }
        }

        /// <summary>
        /// Plays the reveal animation when chest opens
        /// </summary>
        public void PlayRevealAnimation(float delay = 0f)
        {
            // Simple reveal without tweening library
            transform.localScale = Vector3.one;

            if (glowImage && currentRarity >= ChestRewardRarity.Uncommon)
                glowImage.gameObject.SetActive(true);

            if (revealParticles)
                revealParticles.Play();

            if (rarityParticles && currentRarity >= ChestRewardRarity.Rare)
            {
                var main = rarityParticles.main;
                main.startColor = GetRarityColor(currentRarity);
                rarityParticles.Play();
            }
        }

        /// <summary>
        /// Plays special animation for legendary items
        /// </summary>
        public void PlayLegendaryAnimation()
        {
            if (currentRarity != ChestRewardRarity.Legendary) return;
            // Animation requires LeanTween or DOTween - implement when available
        }
    }

    [Serializable]
    public class ChestRewardDisplayData
    {
        public string id;
        public string name;
        public Sprite icon;
        public int amount;
        public ChestRewardRarity rarity;
        public string rewardType;
        public bool isNew;
    }

    public enum ChestRewardRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
}
