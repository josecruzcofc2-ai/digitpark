using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace DigitPark.UI.Common
{
    /// <summary>
    /// Simple hover effect for the forgot password link
    /// Changes color on hover for visual feedback - neon style
    /// </summary>
    public class ForgotPasswordHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(0f, 0.9f, 1f, 1f);      // Cyan
        [SerializeField] private Color hoverColor = new Color(0.5f, 1f, 1f, 1f);       // Lighter cyan
        [SerializeField] private Color pressedColor = new Color(0f, 0.7f, 0.85f, 1f);  // Darker cyan

        private TextMeshProUGUI textComponent;
        private bool isHovered = false;

        private void Awake()
        {
            textComponent = GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.color = normalColor;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            if (textComponent != null)
            {
                textComponent.color = hoverColor;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            if (textComponent != null)
            {
                textComponent.color = normalColor;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (textComponent != null)
            {
                textComponent.color = pressedColor;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (textComponent != null)
            {
                textComponent.color = isHovered ? hoverColor : normalColor;
            }
        }
    }
}
