using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DigitPark.Animations
{
    /// <summary>
    /// Handles cinematographic chest opening animations.
    /// Creates excitement and anticipation for reward reveals.
    /// </summary>
    public class ChestOpeningAnimator : MonoBehaviour
    {
        [Header("Chest Elements")]
        [SerializeField] private RectTransform chestContainer;
        [SerializeField] private Image chestImage;
        [SerializeField] private Image chestLid;
        [SerializeField] private Image chestGlow;
        [SerializeField] private Image chestShadow;

        [Header("Particles")]
        [SerializeField] private ParticleSystem anticipationParticles;
        [SerializeField] private ParticleSystem burstParticles;
        [SerializeField] private ParticleSystem continuousSparkles;

        [Header("Light Rays")]
        [SerializeField] private RectTransform lightRaysContainer;
        [SerializeField] private Image[] lightRays;

        [Header("Reward Display")]
        [SerializeField] private RectTransform rewardContainer;
        [SerializeField] private Image rewardIcon;
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private TextMeshProUGUI rewardAmount;

        [Header("Screen Effects")]
        [SerializeField] private CanvasGroup screenDimmer;
        [SerializeField] private Image screenFlash;

        [Header("Animation Settings")]
        [SerializeField] private float shakeDuration = 1.5f;
        [SerializeField] private float shakeIntensity = 10f;
        [SerializeField] private int shakeVibrato = 20;
        [SerializeField] private float openDuration = 0.5f;
        [SerializeField] private float rewardRevealDelay = 0.3f;

        [Header("Audio")]
        [SerializeField] private AudioClip shakeSound;
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip rewardRevealSound;
        [SerializeField] private AudioClip legendarySound;

        // Events
        public event Action OnChestOpened;
        public event Action<int> OnRewardRevealed;

        private AudioSource audioSource;
        private bool isAnimating;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        // ==================== PUBLIC API ====================

        /// <summary>
        /// Play the full chest opening sequence
        /// </summary>
        public void PlayChestOpenSequence(Sprite rewardSprite, string rewardName, int amount, bool isLegendary = false)
        {
            if (isAnimating) return;
            StartCoroutine(ChestOpenSequenceCoroutine(rewardSprite, rewardName, amount, isLegendary));
        }

        /// <summary>
        /// Play quick chest open (skip anticipation)
        /// </summary>
        public void PlayQuickOpen(Sprite rewardSprite, string rewardName, int amount)
        {
            if (isAnimating) return;
            StartCoroutine(QuickOpenCoroutine(rewardSprite, rewardName, amount));
        }

        /// <summary>
        /// Play chest idle animation (breathing, glow)
        /// </summary>
        public void PlayIdleAnimation()
        {
            if (chestContainer != null)
            {
                chestContainer.DOScale(1.02f, 1.5f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }

            if (chestGlow != null)
            {
                chestGlow.DOFade(0.8f, 1f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }

            if (continuousSparkles != null)
                continuousSparkles.Play();
        }

        /// <summary>
        /// Stop all animations
        /// </summary>
        public void StopAllAnimations()
        {
            StopAllCoroutines();
            DOTween.Kill(chestContainer);
            DOTween.Kill(chestGlow);
            isAnimating = false;
        }

        // ==================== ANIMATION SEQUENCES ====================

        private IEnumerator ChestOpenSequenceCoroutine(Sprite rewardSprite, string rewardName, int amount, bool isLegendary)
        {
            isAnimating = true;

            // Phase 1: Anticipation (dimming, focus)
            yield return StartCoroutine(PlayAnticipationPhase());

            // Phase 2: Shake buildup
            yield return StartCoroutine(PlayShakePhase(isLegendary));

            // Phase 3: Open with flash
            yield return StartCoroutine(PlayOpenPhase(isLegendary));

            // Phase 4: Reveal reward
            yield return StartCoroutine(PlayRewardRevealPhase(rewardSprite, rewardName, amount, isLegendary));

            isAnimating = false;
            OnChestOpened?.Invoke();
        }

        private IEnumerator QuickOpenCoroutine(Sprite rewardSprite, string rewardName, int amount)
        {
            isAnimating = true;

            // Quick open without anticipation
            yield return StartCoroutine(PlayOpenPhase(false));
            yield return StartCoroutine(PlayRewardRevealPhase(rewardSprite, rewardName, amount, false));

            isAnimating = false;
            OnChestOpened?.Invoke();
        }

        // ==================== ANIMATION PHASES ====================

        private IEnumerator PlayAnticipationPhase()
        {
            // Dim screen
            if (screenDimmer != null)
            {
                screenDimmer.gameObject.SetActive(true);
                screenDimmer.alpha = 0f;
                screenDimmer.DOFade(0.6f, 0.5f);
            }

            // Zoom to chest
            if (chestContainer != null)
            {
                chestContainer.DOScale(1.3f, 0.5f).SetEase(Ease.OutQuad);
            }

            // Start anticipation particles
            if (anticipationParticles != null)
                anticipationParticles.Play();

            yield return new WaitForSeconds(0.5f);
        }

        private IEnumerator PlayShakePhase(bool isLegendary)
        {
            // Play shake sound
            if (shakeSound != null && audioSource != null)
                audioSource.PlayOneShot(shakeSound);

            float actualShakeDuration = isLegendary ? shakeDuration * 1.5f : shakeDuration;
            float actualIntensity = isLegendary ? shakeIntensity * 1.5f : shakeIntensity;

            // Progressive shake
            if (chestContainer != null)
            {
                // Start with gentle shake
                chestContainer.DOShakePosition(actualShakeDuration * 0.3f, actualIntensity * 0.3f, shakeVibrato / 2);
                yield return new WaitForSeconds(actualShakeDuration * 0.3f);

                // Medium shake
                chestContainer.DOShakePosition(actualShakeDuration * 0.3f, actualIntensity * 0.6f, shakeVibrato);
                yield return new WaitForSeconds(actualShakeDuration * 0.3f);

                // Intense shake
                chestContainer.DOShakePosition(actualShakeDuration * 0.4f, actualIntensity, shakeVibrato * 2);

                // Glow intensifies
                if (chestGlow != null)
                {
                    chestGlow.gameObject.SetActive(true);
                    chestGlow.DOFade(1f, actualShakeDuration * 0.4f);
                }

                yield return new WaitForSeconds(actualShakeDuration * 0.4f);
            }
        }

        private IEnumerator PlayOpenPhase(bool isLegendary)
        {
            // Flash
            if (screenFlash != null)
            {
                screenFlash.gameObject.SetActive(true);
                screenFlash.color = isLegendary ? new Color(1f, 0.9f, 0.3f, 0f) : new Color(1f, 1f, 1f, 0f);
                screenFlash.DOFade(1f, openDuration * 0.3f).OnComplete(() =>
                {
                    screenFlash.DOFade(0f, openDuration * 0.7f).OnComplete(() =>
                        screenFlash.gameObject.SetActive(false));
                });
            }

            // Play open sound
            AudioClip soundToPlay = isLegendary ? legendarySound : openSound;
            if (soundToPlay != null && audioSource != null)
                audioSource.PlayOneShot(soundToPlay);

            // Open lid
            if (chestLid != null)
            {
                chestLid.transform.DOLocalRotate(new Vector3(0, 0, -45f), openDuration).SetEase(Ease.OutBack);
                chestLid.transform.DOLocalMoveY(chestLid.transform.localPosition.y + 50f, openDuration).SetEase(Ease.OutBack);
            }

            // Burst particles
            if (burstParticles != null)
                burstParticles.Play();

            // Light rays appear
            if (lightRaysContainer != null)
            {
                lightRaysContainer.gameObject.SetActive(true);
                lightRaysContainer.localScale = Vector3.zero;
                lightRaysContainer.DOScale(1f, openDuration).SetEase(Ease.OutBack);
                lightRaysContainer.DORotate(new Vector3(0, 0, 360f), 5f, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1);
            }

            yield return new WaitForSeconds(openDuration);
        }

        private IEnumerator PlayRewardRevealPhase(Sprite rewardSprite, string rewardName, int amount, bool isLegendary)
        {
            yield return new WaitForSeconds(rewardRevealDelay);

            // Play reveal sound
            if (rewardRevealSound != null && audioSource != null)
                audioSource.PlayOneShot(rewardRevealSound);

            // Setup reward display
            if (rewardContainer != null)
            {
                rewardContainer.gameObject.SetActive(true);
                rewardContainer.localScale = Vector3.zero;

                if (rewardIcon != null)
                    rewardIcon.sprite = rewardSprite;

                if (rewardText != null)
                    rewardText.text = rewardName;

                if (rewardAmount != null)
                    rewardAmount.text = $"x{amount}";

                // Reveal animation
                Sequence revealSeq = DOTween.Sequence()
                    .Append(rewardContainer.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack))
                    .Append(rewardContainer.DOScale(1f, 0.2f).SetEase(Ease.InOutQuad));

                if (isLegendary)
                {
                    revealSeq.Join(rewardContainer.DOPunchRotation(new Vector3(0, 0, 10f), 0.5f, 10));
                }

                // Continuous float for reward
                DOTween.Sequence()
                    .AppendInterval(0.5f)
                    .Append(rewardContainer.DOMoveY(rewardContainer.position.y + 10f, 1f).SetEase(Ease.InOutSine))
                    .SetLoops(-1, LoopType.Yoyo);
            }

            OnRewardRevealed?.Invoke(amount);
        }

        // ==================== UTILITY ====================

        /// <summary>
        /// Setup chest visuals for different rarity
        /// </summary>
        public void SetChestRarity(ChestRarity rarity)
        {
            Color glowColor = rarity switch
            {
                ChestRarity.Common => new Color(0.7f, 0.7f, 0.7f),
                ChestRarity.Rare => new Color(0.2f, 0.5f, 1f),
                ChestRarity.Epic => new Color(0.6f, 0.2f, 1f),
                ChestRarity.Legendary => new Color(1f, 0.8f, 0.2f),
                _ => Color.white
            };

            if (chestGlow != null)
                chestGlow.color = new Color(glowColor.r, glowColor.g, glowColor.b, chestGlow.color.a);
        }

        /// <summary>
        /// Reset to initial state
        /// </summary>
        public void ResetChest()
        {
            StopAllAnimations();

            if (chestContainer != null)
                chestContainer.localScale = Vector3.one;

            if (chestLid != null)
            {
                chestLid.transform.localRotation = Quaternion.identity;
                chestLid.transform.localPosition = Vector3.zero;
            }

            if (screenDimmer != null)
            {
                screenDimmer.alpha = 0f;
                screenDimmer.gameObject.SetActive(false);
            }

            if (lightRaysContainer != null)
                lightRaysContainer.gameObject.SetActive(false);

            if (rewardContainer != null)
                rewardContainer.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            DOTween.Kill(chestContainer);
            DOTween.Kill(lightRaysContainer);
            DOTween.Kill(rewardContainer);
        }
    }

    public enum ChestRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }
}
