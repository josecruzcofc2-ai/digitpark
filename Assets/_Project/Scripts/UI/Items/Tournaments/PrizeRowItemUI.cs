using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DigitPark.UI.Items
{
    /// <summary>
    /// UI component for tournament prize row prefab.
    /// Displays position, prize amount and percentage.
    /// </summary>
    public class PrizeRowItemUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI positionText;
        [SerializeField] private Image positionIcon;
        [SerializeField] private TextMeshProUGUI prizeAmountText;
        [SerializeField] private TextMeshProUGUI percentageText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image medalImage;

        [Header("Medal Sprites")]
        [SerializeField] private Sprite goldMedal;
        [SerializeField] private Sprite silverMedal;
        [SerializeField] private Sprite bronzeMedal;

        [Header("Visual Settings")]
        [SerializeField] private Color firstPlaceColor = new Color(1f, 0.84f, 0f, 0.3f);
        [SerializeField] private Color secondPlaceColor = new Color(0.75f, 0.75f, 0.8f, 0.3f);
        [SerializeField] private Color thirdPlaceColor = new Color(0.8f, 0.5f, 0.2f, 0.3f);
        [SerializeField] private Color normalColor = new Color(0.15f, 0.15f, 0.2f, 1f);

        public void Setup(PrizeRowData data)
        {
            // Position text
            if (positionText)
                positionText.text = GetPositionText(data.position);

            // Prize amount
            if (prizeAmountText)
                prizeAmountText.text = data.isCash ? $"${data.prizeAmount:F2}" : $"{data.prizeAmount}";

            // Percentage
            if (percentageText)
            {
                percentageText.text = $"({data.percentage}%)";
                percentageText.gameObject.SetActive(data.showPercentage);
            }

            // Medal and background for top 3
            ApplyPositionVisuals(data.position);
        }

        private string GetPositionText(int position)
        {
            switch (position)
            {
                case 1: return "1er Lugar";
                case 2: return "2do Lugar";
                case 3: return "3er Lugar";
                default: return $"{position}to Lugar";
            }
        }

        private void ApplyPositionVisuals(int position)
        {
            Color bgColor = normalColor;
            Sprite medal = null;

            switch (position)
            {
                case 1:
                    bgColor = firstPlaceColor;
                    medal = goldMedal;
                    break;
                case 2:
                    bgColor = secondPlaceColor;
                    medal = silverMedal;
                    break;
                case 3:
                    bgColor = thirdPlaceColor;
                    medal = bronzeMedal;
                    break;
            }

            if (backgroundImage)
                backgroundImage.color = bgColor;

            if (medalImage)
            {
                medalImage.gameObject.SetActive(medal != null);
                if (medal != null)
                    medalImage.sprite = medal;
            }
        }
    }

    [System.Serializable]
    public class PrizeRowData
    {
        public int position;
        public decimal prizeAmount;
        public float percentage;
        public bool isCash;
        public bool showPercentage;
    }
}
