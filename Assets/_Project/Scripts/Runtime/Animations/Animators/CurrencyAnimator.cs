using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;
using System.Collections;

namespace DigitPark.Animations
{
    /// <summary>
    /// Handles currency display animations including counters,
    /// purchase feedback, and insufficient funds effects.
    /// </summary>
    public class CurrencyAnimator : MonoBehaviour
    {
        [Header("Display Elements")]
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private Image currencyIcon;
        [SerializeField] private RectTransform container;

        [Header("Effects")]
        [SerializeField] private Image glowEffect;
        [SerializeField] private ParticleSystem gainParticles;
        [SerializeField] private ParticleSystem spendParticles;

        [Header("Plus Button")]
        [SerializeField] private RectTransform plusButton;
        [SerializeField] private Image plusButtonGlow;

        [Header("Animation Settings")]
        [SerializeField] private float countDuration = 0.5f;
        [SerializeField] private float punchScale = 0.2f;
        [SerializeField] private float shakeIntensity = 5f;

        [Header("Colors")]
        [SerializeField] private Color gainColor = new Color(0.3f, 1f, 0.3f);
        [SerializeField] private Color spendColor = new Color(1f, 0.3f, 0.3f);
        [SerializeField] private Color insufficientColor = new Color(1f, 0.2f, 0.2f);
        [SerializeField] private Color normalColor = Color.white;

        [Header("Audio")]
        [SerializeField] private AudioClip countSound;
        [SerializeField] private AudioClip gainSound;
        [SerializeField] private AudioClip spendSound;
        [SerializeField] private AudioClip insufficientSound;

        private AudioSource audioSource;
        private int currentValue;
        private Tween countTween;
        private Tween glowTween;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        // ==================== VALUE CHANGES ====================

        /// <summary>
        /// Set value instantly without animation
        /// </summary>
        public void SetValue(int value)
        {
            currentValue = value;
            UpdateDisplay(value);
        }

        /// <summary>
        /// Animate value change (gain)
        /// </summary>
        public void AnimateGain(int amount, Action onComplete = null)
        {
            int targetValue = currentValue + amount;
            AnimateToValue(targetValue, true, onComplete);
        }

        /// <summary>
        /// Animate value change (spend)
        /// </summary>
        public void AnimateSpend(int amount, Action onComplete = null)
        {
            int targetValue = currentValue - amount;
            AnimateToValue(targetValue, false, onComplete);
        }

        /// <summary>
        /// Animate to specific value
        /// </summary>
        public void AnimateToValue(int targetValue, bool isGain, Action onComplete = null)
        {
            countTween?.Kill();

            // Play appropriate sound
            AudioClip soundToPlay = isGain ? gainSound : spendSound;
            if (soundToPlay != null && audioSource != null)
                audioSource.PlayOneShot(soundToPlay);

            // Particles
            if (isGain && gainParticles != null)
                gainParticles.Play();
            else if (!isGain && spendParticles != null)
                spendParticles.Play();

            // Color flash
            Color flashColor = isGain ? gainColor : spendColor;
            if (amountText != null)
            {
                amountText.DOColor(flashColor, countDuration * 0.3f)
                    .OnComplete(() => { if (amountText != null) amountText.DOColor(normalColor, countDuration * 0.7f); });
            }

            // Scale punch
            if (container != null)
            {
                container.DOKill();
                container.localScale = Vector3.one;
                container.DOPunchScale(Vector3.one * punchScale, countDuration, 5);
            }

            // Icon bounce
            if (currencyIcon != null)
            {
                currencyIcon.transform.DOKill();
                currencyIcon.transform.DOPunchRotation(new Vector3(0, 0, isGain ? 15f : -15f), countDuration, 5);
            }

            // Glow effect
            if (glowEffect != null)
            {
                glowEffect.gameObject.SetActive(true);
                glowEffect.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
                glowEffect.DOFade(0.8f, countDuration * 0.3f)
                    .OnComplete(() => {
                        if (glowEffect != null)
                            glowEffect.DOFade(0f, countDuration * 0.7f)
                                .OnComplete(() => { if (glowEffect != null) glowEffect.gameObject.SetActive(false); });
                    });
            }

            // Count animation
            int startValue = currentValue;
            countTween = DOTween.To(() => startValue, x =>
            {
                startValue = x;
                UpdateDisplay(x);

                // Play tick sound periodically
                if (countSound != null && audioSource != null && UnityEngine.Random.value > 0.7f)
                    audioSource.PlayOneShot(countSound, 0.3f);

            }, targetValue, countDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                currentValue = targetValue;
                onComplete?.Invoke();
            });
        }

        // ==================== FEEDBACK EFFECTS ====================

        /// <summary>
        /// Play insufficient funds feedback
        /// </summary>
        public void PlayInsufficientFunds()
        {
            if (insufficientSound != null && audioSource != null)
                audioSource.PlayOneShot(insufficientSound);

            // Shake
            if (container != null)
            {
                container.DOKill();
                container.DOShakePosition(0.4f, shakeIntensity, 20, 90, false, true);
            }

            // Red flash
            if (amountText != null)
            {
                Sequence flashSeq = DOTween.Sequence();
                flashSeq.Append(amountText.DOColor(insufficientColor, 0.1f));
                flashSeq.Append(amountText.DOColor(normalColor, 0.1f));
                flashSeq.SetLoops(3);
                flashSeq.SetLink(amountText.gameObject);
            }

            // Icon shake
            if (currencyIcon != null)
            {
                currencyIcon.transform.DOShakeRotation(0.4f, new Vector3(0, 0, 20f), 20);
            }

            // Highlight plus button
            HighlightPlusButton();
        }

        /// <summary>
        /// Highlight the plus/add button to prompt purchase
        /// </summary>
        public void HighlightPlusButton()
        {
            if (plusButton == null) return;

            DOTween.Sequence()
                .Append(plusButton.DOScale(1.2f, 0.2f).SetEase(Ease.OutQuad))
                .Append(plusButton.DOScale(1f, 0.3f).SetEase(Ease.OutBounce))
                .SetLoops(3)
                .SetLink(plusButton.gameObject);

            if (plusButtonGlow != null)
            {
                DOTween.Kill(plusButtonGlow);
                plusButtonGlow.gameObject.SetActive(true);
                plusButtonGlow.DOFade(1f, 0.3f)
                    .SetLoops(6, LoopType.Yoyo)
                    .OnComplete(() => plusButtonGlow.gameObject.SetActive(false));
            }
        }

        /// <summary>
        /// Play purchase success feedback
        /// </summary>
        public void PlayPurchaseSuccess()
        {
            if (container != null)
            {
                container.DOPunchScale(Vector3.one * 0.3f, 0.4f, 10);
            }

            if (glowEffect != null)
            {
                glowEffect.gameObject.SetActive(true);
                glowEffect.color = new Color(gainColor.r, gainColor.g, gainColor.b, 0f);

                DOTween.Sequence()
                    .Append(glowEffect.DOFade(1f, 0.2f))
                    .Append(glowEffect.DOFade(0f, 0.4f))
                    .OnComplete(() => glowEffect.gameObject.SetActive(false));
            }
        }

        // ==================== CONTINUOUS EFFECTS ====================

        /// <summary>
        /// Start pulsing glow (for low currency warning)
        /// </summary>
        public void StartLowCurrencyPulse()
        {
            glowTween?.Kill();

            if (glowEffect == null) return;

            glowEffect.gameObject.SetActive(true);
            glowEffect.color = new Color(1f, 0.5f, 0f, 0f);

            glowTween = glowEffect.DOFade(0.5f, 0.8f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        /// <summary>
        /// Stop low currency pulse
        /// </summary>
        public void StopLowCurrencyPulse()
        {
            glowTween?.Kill();

            if (glowEffect != null)
            {
                glowEffect.DOFade(0f, 0.3f)
                    .OnComplete(() => glowEffect.gameObject.SetActive(false));
            }
        }

        /// <summary>
        /// Play attention animation
        /// </summary>
        public void PlayAttention()
        {
            if (container != null)
            {
                container.DOPunchScale(Vector3.one * 0.15f, 0.3f, 5);
            }

            if (currencyIcon != null)
            {
                currencyIcon.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.3f, 5);
            }
        }

        // ==================== FLYING CURRENCY ====================

        /// <summary>
        /// Receive flying currency icon
        /// </summary>
        public void ReceiveFlyingIcon(RectTransform flyingIcon, int amount, Action onComplete = null)
        {
            if (flyingIcon == null || container == null)
            {
                AnimateGain(amount, onComplete);
                return;
            }

            // Animate icon to this position
            Sequence flySeq = DOTween.Sequence();
            flySeq.Append(flyingIcon.DOMove(container.position, 0.4f).SetEase(Ease.InBack));
            flySeq.Join(flyingIcon.DOScale(0.5f, 0.4f));
            flySeq.Join(flyingIcon.DORotate(new Vector3(0, 0, 360f), 0.4f, RotateMode.FastBeyond360));
            flySeq.OnComplete(() =>
            {
                Destroy(flyingIcon.gameObject);
                AnimateGain(amount, onComplete);
            });
        }

        // ==================== MULTIPLIER DISPLAY ====================

        /// <summary>
        /// Show multiplier badge
        /// </summary>
        public void ShowMultiplier(float multiplier, RectTransform multiplierBadge)
        {
            if (multiplierBadge == null) return;

            multiplierBadge.gameObject.SetActive(true);
            multiplierBadge.localScale = Vector3.zero;

            TextMeshProUGUI badgeText = multiplierBadge.GetComponentInChildren<TextMeshProUGUI>();
            if (badgeText != null)
                badgeText.text = $"x{multiplier:F1}";

            DOTween.Sequence()
                .Append(multiplierBadge.DOScale(1.3f, 0.2f).SetEase(Ease.OutBack))
                .Append(multiplierBadge.DOScale(1f, 0.15f))
                .Append(multiplierBadge.DOScale(1.05f, 0.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo))
                .SetLink(multiplierBadge.gameObject);
        }

        // ==================== HELPERS ====================

        private void UpdateDisplay(int value)
        {
            if (amountText != null)
            {
                amountText.text = FormatCurrency(value);
            }
        }

        private string FormatCurrency(int value)
        {
            if (value >= 1000000)
                return $"{value / 1000000f:F1}M";
            else if (value >= 1000)
                return $"{value / 1000f:F1}K";
            else
                return value.ToString();
        }

        /// <summary>
        /// Get current displayed value
        /// </summary>
        public int GetCurrentValue() => currentValue;

        private void OnDestroy()
        {
            countTween?.Kill();
            glowTween?.Kill();
            DOTween.Kill(container);
            DOTween.Kill(currencyIcon?.transform);
            DOTween.Kill(plusButton);
        }
    }
}
