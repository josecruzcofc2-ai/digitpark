using UnityEngine;
using UnityEngine.UI;
using DigitPark.Monetization;

namespace DigitPark.UI
{
    /// <summary>
    /// Premium golden back button for Cash Battle scenes
    /// Luxury VIP casino aesthetic with gold theme
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class BackButtonGold : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Button button;
        [SerializeField] private Image iconImage;

        [Header("Settings")]
        [SerializeField] private bool autoNavigateBack = true;
        [SerializeField] private bool playSoundOnClick = true;

        private void Awake()
        {
            if (button == null) button = GetComponent<Button>();

            // Auto-setup if components are missing
            if (iconImage == null)
            {
                iconImage = GetComponentInChildren<Image>();
            }
        }

        private void Start()
        {
            if (autoNavigateBack)
            {
                button.onClick.AddListener(OnBackClicked);
            }
        }

        private void OnBackClicked()
        {
            if (playSoundOnClick)
            {
                // Play gold button click sound (premium sound)
                var audioManager = FindFirstObjectByType<DigitPark.Managers.AudioManager>();
                if (audioManager != null)
                {
                    audioManager.PlaySFX("button_click_gold");
                }
            }

            // Navigate back using SceneNavigator
            SceneNavigator.Instance?.GoBack();
        }

        /// <summary>
        /// Call this to disable auto navigation and add custom onClick behavior
        /// </summary>
        public void DisableAutoNavigation()
        {
            autoNavigateBack = false;
            button.onClick.RemoveListener(OnBackClicked);
        }

        /// <summary>
        /// Get the button component for custom configuration
        /// </summary>
        public Button GetButton() => button;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (button == null) button = GetComponent<Button>();
            if (iconImage == null) iconImage = GetComponentInChildren<Image>();
        }
#endif
    }
}
