using UnityEngine;
using UnityEngine.UI;

namespace DigitPark.UI.Items
{
    /// <summary>
    /// UI component for onboarding step dot indicator prefab.
    /// Simple dot that shows current step progress.
    /// </summary>
    public class StepDotItemUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image dotImage;
        [SerializeField] private Image pulseImage;

        [Header("Visual Settings")]
        [SerializeField] private Color activeColor = new Color(0f, 0.83f, 1f, 1f);
        [SerializeField] private Color completedColor = new Color(0.3f, 0.8f, 0.4f, 1f);
        [SerializeField] private Color inactiveColor = new Color(0.3f, 0.3f, 0.35f, 1f);
        [SerializeField] private float activeScale = 1.2f;
        [SerializeField] private float inactiveScale = 0.8f;

        private int stepIndex;
        private bool isActive;

        public void Setup(int index)
        {
            stepIndex = index;
            SetState(StepDotState.Inactive);
        }

        public void SetState(StepDotState state)
        {
            isActive = state == StepDotState.Active;

            Color targetColor;
            float targetScale;

            switch (state)
            {
                case StepDotState.Active:
                    targetColor = activeColor;
                    targetScale = activeScale;
                    StartPulseAnimation();
                    break;
                case StepDotState.Completed:
                    targetColor = completedColor;
                    targetScale = 1f;
                    StopPulseAnimation();
                    break;
                case StepDotState.Inactive:
                default:
                    targetColor = inactiveColor;
                    targetScale = inactiveScale;
                    StopPulseAnimation();
                    break;
            }

            // Apply immediately (no tweening)
            if (dotImage)
                dotImage.color = targetColor;

            transform.localScale = Vector3.one * targetScale;
        }

        private void StartPulseAnimation()
        {
            if (pulseImage == null) return;
            pulseImage.gameObject.SetActive(true);
            pulseImage.color = new Color(activeColor.r, activeColor.g, activeColor.b, 0.5f);
        }

        private void StopPulseAnimation()
        {
            if (pulseImage == null) return;
            pulseImage.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            // Cleanup if needed
        }
    }

    public enum StepDotState
    {
        Inactive,
        Active,
        Completed
    }
}
