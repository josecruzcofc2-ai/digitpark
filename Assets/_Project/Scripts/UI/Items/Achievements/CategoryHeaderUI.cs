using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DigitPark.UI.Items
{
    /// <summary>
    /// UI component for achievement category header prefab.
    /// Displays category name, icon and progress.
    /// </summary>
    public class CategoryHeaderUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI categoryNameText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Image categoryIcon;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Button expandButton;
        [SerializeField] private Image expandArrow;

        [Header("Visual Settings")]
        [SerializeField] private Color headerColor = new Color(0.15f, 0.15f, 0.2f, 1f);
        [SerializeField] private float expandedRotation = 90f;
        [SerializeField] private float collapsedRotation = 0f;

        private bool isExpanded = true;
        private System.Action<string, bool> onExpandToggled;
        private string categoryId;

        public void Setup(string id, string categoryName, Sprite icon, int completed, int total, System.Action<string, bool> expandCallback = null)
        {
            categoryId = id;
            onExpandToggled = expandCallback;

            if (categoryNameText)
                categoryNameText.text = categoryName;

            if (progressText)
                progressText.text = $"{completed}/{total}";

            if (categoryIcon && icon != null)
                categoryIcon.sprite = icon;

            if (backgroundImage)
                backgroundImage.color = headerColor;

            if (expandButton)
            {
                expandButton.onClick.RemoveAllListeners();
                expandButton.onClick.AddListener(ToggleExpand);
            }

            UpdateExpandVisual();
        }

        private void ToggleExpand()
        {
            isExpanded = !isExpanded;
            UpdateExpandVisual();
            onExpandToggled?.Invoke(categoryId, isExpanded);
        }

        private void UpdateExpandVisual()
        {
            if (expandArrow)
            {
                float targetRotation = isExpanded ? expandedRotation : collapsedRotation;
                expandArrow.transform.rotation = Quaternion.Euler(0, 0, targetRotation);
            }
        }

        public void SetExpanded(bool expanded)
        {
            isExpanded = expanded;
            UpdateExpandVisual();
        }
    }
}
