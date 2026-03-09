using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace DigitPark.UI.Items
{
    /// <summary>
    /// UI component for avatar selection option prefab.
    /// Displays avatar image with selection state.
    /// </summary>
    public class AvatarOptionItemUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image avatarImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image borderImage;
        [SerializeField] private Image selectionRing;
        [SerializeField] private Button selectButton;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private GameObject lockedOverlay;
        [SerializeField] private Image lockIcon;
        [SerializeField] private TextMeshProUGUI unlockRequirementText;

        [Header("Visual Settings")]
        [SerializeField] private Color normalBorderColor = new Color(0.3f, 0.3f, 0.35f, 1f);
        [SerializeField] private Color selectedBorderColor = new Color(0f, 0.83f, 1f, 1f);
        [SerializeField] private Color premiumBorderColor = new Color(1f, 0.7f, 0.2f, 1f);
        [SerializeField] private float selectedScale = 1.1f;

        private string avatarId;
        private bool isSelected;
        private bool isLocked;
        private Action<string> onSelectClicked;

        public void Setup(AvatarOptionData data, Action<string> selectCallback = null)
        {
            avatarId = data.id;
            isLocked = data.isLocked;
            onSelectClicked = selectCallback;

            // Avatar image
            if (avatarImage && data.avatarSprite != null)
                avatarImage.sprite = data.avatarSprite;

            // Name
            if (nameText)
            {
                nameText.text = data.name;
                nameText.gameObject.SetActive(!string.IsNullOrEmpty(data.name));
            }

            // Border color for premium
            if (borderImage && data.isPremium && !data.isLocked)
                borderImage.color = premiumBorderColor;

            // Locked state
            if (lockedOverlay)
                lockedOverlay.SetActive(data.isLocked);

            if (unlockRequirementText && data.isLocked)
                unlockRequirementText.text = data.unlockRequirement;

            // Button
            if (selectButton)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(OnSelectButtonClicked);
                selectButton.interactable = !data.isLocked;
            }

            // Initial selection state
            SetSelected(data.isSelected, false);
        }

        public void SetSelected(bool selected, bool animate = true)
        {
            isSelected = selected;

            if (borderImage && !isLocked)
                borderImage.color = selected ? selectedBorderColor : normalBorderColor;

            if (selectionRing)
            {
                selectionRing.gameObject.SetActive(selected);
                selectionRing.transform.localScale = Vector3.one;
            }

            transform.localScale = Vector3.one * (selected ? selectedScale : 1f);
        }

        private void OnSelectButtonClicked()
        {
            if (isLocked) return;
            onSelectClicked?.Invoke(avatarId);
        }

        public void PlaySelectAnimation()
        {
            // Simple selection feedback
            transform.localScale = Vector3.one * selectedScale;
        }

    }

    [Serializable]
    public class AvatarOptionData
    {
        public string id;
        public string name;
        public Sprite avatarSprite;
        public bool isSelected;
        public bool isLocked;
        public bool isPremium;
        public string unlockRequirement;
    }
}
