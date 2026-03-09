using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;

namespace DigitPark.Animations
{
    /// <summary>
    /// 3D Button component that provides depth effect and satisfying press animations.
    /// Attach to any UI Button to give it a 3D appearance.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class Button3D : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("3D Structure")]
        [SerializeField] private RectTransform buttonFace;
        [SerializeField] private RectTransform buttonShadow;
        [SerializeField] private Image highlightLine;

        [Header("Press Settings")]
        [SerializeField] private float pressDepth = 6f;
        [SerializeField] private float pressDuration = 0.08f;
        [SerializeField] private float releaseDuration = 0.12f;

        [Header("Hover Settings")]
        [SerializeField] private bool enableHoverEffect = true;
        [SerializeField] private float hoverScale = 1.03f;
        [SerializeField] private float hoverDuration = 0.15f;

        [Header("Glow Settings")]
        [SerializeField] private bool enableGlow = true;
        [SerializeField] private Image glowImage;
        [SerializeField] private float glowPulseSpeed = 1.5f;
        [SerializeField] private float glowMinAlpha = 0.3f;
        [SerializeField] private float glowMaxAlpha = 0.8f;

        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(0f, 1f, 1f);
        [SerializeField] private Color pressedColor = new Color(0f, 0.7f, 0.7f);
        [SerializeField] private Color highlightColor = new Color(1f, 1f, 1f, 0.5f);

        [Header("Audio")]
        [SerializeField] private AudioClip pressSound;
        [SerializeField] private AudioClip releaseSound;

        [Header("Particles")]
        [SerializeField] private ParticleSystem pressParticles;
        [SerializeField] private ParticleSystem constantParticles;

        // Events
        public event Action OnPressed;
        public event Action OnReleased;

        // Private
        private Button button;
        private Vector2 originalFacePosition;
        private Vector2 originalShadowSize;
        private Color originalButtonColor;
        private bool isPressed;
        private bool isHovered;
        private Tween glowTween;
        private Tween hoverTween;
        private AudioSource audioSource;

        private void Awake()
        {
            button = GetComponent<Button>();
            audioSource = GetComponent<AudioSource>();

            if (buttonFace != null)
                originalFacePosition = buttonFace.anchoredPosition;

            if (buttonShadow != null)
                originalShadowSize = buttonShadow.sizeDelta;

            // Capture original color for fallback mode
            Image buttonImage = GetComponent<Image>();
            if (buttonImage != null)
                originalButtonColor = buttonImage.color;

            SetupGlowAnimation();
            SetupConstantParticles();
        }

        private void OnEnable()
        {
            ResetButton();
            if (enableGlow && glowImage != null)
                StartGlowAnimation();
        }

        private void OnDisable()
        {
            StopAllAnimations();
        }

        // ==================== POINTER EVENTS ====================

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!button.interactable) return;

            isPressed = true;
            PlayPressAnimation();
            PlayPressSound();
            PlayPressParticles();
            OnPressed?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isPressed) return;

            isPressed = false;
            PlayReleaseAnimation();
            PlayReleaseSound();
            OnReleased?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!button.interactable || !enableHoverEffect) return;

            isHovered = true;
            PlayHoverEnterAnimation();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            PlayHoverExitAnimation();

            if (isPressed)
            {
                isPressed = false;
                PlayReleaseAnimation();
            }
        }

        // ==================== ANIMATIONS ====================

        private void PlayPressAnimation()
        {
            if (buttonFace != null)
            {
                // Full 3D effect with Face/Shadow structure
                buttonFace.DOKill();
                buttonFace.DOAnchorPosY(originalFacePosition.y - pressDepth, pressDuration)
                    .SetEase(Ease.OutQuad);

                // Change color
                Image faceImage = buttonFace.GetComponent<Image>();
                if (faceImage != null)
                    faceImage.DOColor(pressedColor, pressDuration);
            }
            else
            {
                // Fallback: Darken the button's own image
                Image buttonImage = GetComponent<Image>();
                if (buttonImage != null)
                {
                    if (originalButtonColor == default)
                        originalButtonColor = buttonImage.color;

                    Color darkerColor = new Color(
                        originalButtonColor.r * 0.7f,
                        originalButtonColor.g * 0.7f,
                        originalButtonColor.b * 0.7f,
                        originalButtonColor.a
                    );
                    buttonImage.DOColor(darkerColor, pressDuration);
                }
            }

            if (buttonShadow != null)
            {
                buttonShadow.DOKill();
                buttonShadow.DOSizeDelta(new Vector2(originalShadowSize.x, originalShadowSize.y - pressDepth), pressDuration)
                    .SetEase(Ease.OutQuad);
            }

            if (highlightLine != null)
            {
                highlightLine.DOFade(0f, pressDuration);
            }

            // Scale punch - makes it look "squashed"
            transform.DOKill();
            transform.DOScale(0.92f, pressDuration).SetEase(Ease.OutQuad);
        }

        private void PlayReleaseAnimation()
        {
            if (buttonFace != null)
            {
                buttonFace.DOKill();
                buttonFace.DOAnchorPosY(originalFacePosition.y, releaseDuration)
                    .SetEase(Ease.OutBack);

                // Restore color
                Image faceImage = buttonFace.GetComponent<Image>();
                if (faceImage != null)
                    faceImage.DOColor(normalColor, releaseDuration);
            }
            else
            {
                // Fallback: Restore original color
                Image buttonImage = GetComponent<Image>();
                if (buttonImage != null && originalButtonColor != default)
                {
                    buttonImage.DOColor(originalButtonColor, releaseDuration);
                }
            }

            if (buttonShadow != null)
            {
                buttonShadow.DOKill();
                buttonShadow.DOSizeDelta(originalShadowSize, releaseDuration)
                    .SetEase(Ease.OutBack);
            }

            if (highlightLine != null)
            {
                highlightLine.DOFade(highlightColor.a, releaseDuration);
            }

            // Scale bounce back with satisfying bounce
            transform.DOKill();
            transform.DOScale(isHovered ? hoverScale : 1f, releaseDuration).SetEase(Ease.OutBack);
        }

        private void PlayHoverEnterAnimation()
        {
            hoverTween?.Kill();
            hoverTween = transform.DOScale(hoverScale, hoverDuration).SetEase(Ease.OutQuad);

            if (glowImage != null)
            {
                glowImage.DOFade(glowMaxAlpha, hoverDuration);
            }
        }

        private void PlayHoverExitAnimation()
        {
            if (isPressed) return;

            hoverTween?.Kill();
            hoverTween = transform.DOScale(1f, hoverDuration).SetEase(Ease.OutQuad);

            if (glowImage != null && enableGlow)
            {
                // Return to pulse animation
                StartGlowAnimation();
            }
        }

        // ==================== GLOW ====================

        private void SetupGlowAnimation()
        {
            if (glowImage != null)
            {
                glowImage.color = new Color(glowImage.color.r, glowImage.color.g, glowImage.color.b, glowMinAlpha);
            }
        }

        private void StartGlowAnimation()
        {
            if (glowImage == null || !enableGlow) return;

            glowTween?.Kill();
            glowTween = glowImage.DOFade(glowMaxAlpha, glowPulseSpeed / 2)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void StopGlowAnimation()
        {
            glowTween?.Kill();
        }

        // ==================== PARTICLES ====================

        private void SetupConstantParticles()
        {
            if (constantParticles != null)
            {
                constantParticles.Play();
            }
        }

        private void PlayPressParticles()
        {
            if (pressParticles != null)
            {
                pressParticles.Play();
            }
        }

        // ==================== AUDIO ====================

        private void PlayPressSound()
        {
            if (pressSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(pressSound);
            }
        }

        private void PlayReleaseSound()
        {
            if (releaseSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(releaseSound);
            }
        }

        // ==================== PUBLIC METHODS ====================

        /// <summary>
        /// Programmatically trigger press animation
        /// </summary>
        public void TriggerPress()
        {
            PlayPressAnimation();
            DOVirtual.DelayedCall(pressDuration + 0.05f, () => PlayReleaseAnimation());
        }

        /// <summary>
        /// Set button colors
        /// </summary>
        public void SetColors(Color normal, Color pressed)
        {
            normalColor = normal;
            pressedColor = pressed;

            if (buttonFace != null)
            {
                Image faceImage = buttonFace.GetComponent<Image>();
                if (faceImage != null && !isPressed)
                    faceImage.color = normalColor;
            }
        }

        /// <summary>
        /// Enable/disable glow effect
        /// </summary>
        public void SetGlowEnabled(bool enabled)
        {
            enableGlow = enabled;
            if (enabled)
                StartGlowAnimation();
            else
                StopGlowAnimation();
        }

        /// <summary>
        /// Set glow color
        /// </summary>
        public void SetGlowColor(Color color)
        {
            if (glowImage != null)
            {
                glowImage.color = new Color(color.r, color.g, color.b, glowImage.color.a);
            }
        }

        /// <summary>
        /// Play attention animation (for important buttons)
        /// </summary>
        public void PlayAttentionAnimation()
        {
            DOTween.Sequence()
                .Append(transform.DOScale(1.1f, 0.15f).SetEase(Ease.OutQuad))
                .Append(transform.DOScale(1f, 0.3f).SetEase(Ease.OutBounce))
                .SetLoops(3);
        }

        /// <summary>
        /// Reset button to original state
        /// </summary>
        public void ResetButton()
        {
            isPressed = false;
            isHovered = false;

            transform.DOKill();
            transform.localScale = Vector3.one;

            if (buttonFace != null)
            {
                buttonFace.DOKill();
                buttonFace.anchoredPosition = originalFacePosition;

                Image faceImage = buttonFace.GetComponent<Image>();
                if (faceImage != null)
                    faceImage.color = normalColor;
            }

            if (buttonShadow != null)
            {
                buttonShadow.DOKill();
                buttonShadow.sizeDelta = originalShadowSize;
            }

            if (highlightLine != null)
            {
                highlightLine.color = highlightColor;
            }
        }

        private void StopAllAnimations()
        {
            transform.DOKill();
            buttonFace?.DOKill();
            buttonShadow?.DOKill();
            glowTween?.Kill();
            hoverTween?.Kill();
        }

        private void OnDestroy()
        {
            StopAllAnimations();
        }
    }
}
