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
    /// Handles Battle Pass animations including tier progression,
    /// reward unlocking, and premium upgrade effects.
    /// </summary>
    public class BattlePassAnimator : MonoBehaviour
    {
        [Header("Progress Bar")]
        [SerializeField] private Image progressFill;
        [SerializeField] private RectTransform progressContainer;
        [SerializeField] private ParticleSystem progressParticles;

        [Header("Tier Elements")]
        [SerializeField] private RectTransform tiersScrollContent;
        [SerializeField] private float tierSpacing = 150f;

        [Header("Reward Claim")]
        [SerializeField] private RectTransform claimEffectPrefab;
        [SerializeField] private Transform claimEffectsParent;

        [Header("Premium Upgrade")]
        [SerializeField] private CanvasGroup premiumOverlay;
        [SerializeField] private Image premiumGlow;
        [SerializeField] private ParticleSystem premiumParticles;

        [Header("XP Animation")]
        [SerializeField] private RectTransform xpPopupPrefab;
        [SerializeField] private Transform xpPopupsParent;

        [Header("Animation Settings")]
        [SerializeField] private float progressAnimDuration = 1f;
        [SerializeField] private float tierUnlockDuration = 0.5f;
        [SerializeField] private float scrollToTierDuration = 0.3f;

        [Header("Audio")]
        [SerializeField] private AudioClip progressSound;
        [SerializeField] private AudioClip tierUnlockSound;
        [SerializeField] private AudioClip claimSound;
        [SerializeField] private AudioClip premiumUnlockSound;

        // Events
        public event Action<int> OnTierUnlocked;
        public event Action OnPremiumActivated;

        private AudioSource audioSource;
        private List<RectTransform> tierItems = new List<RectTransform>();
        private int currentTier;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        // ==================== PROGRESS BAR ====================

        /// <summary>
        /// Animate progress bar fill
        /// </summary>
        public Tween AnimateProgress(float fromValue, float toValue, float duration = -1)
        {
            if (duration < 0) duration = progressAnimDuration;

            if (progressFill != null)
            {
                progressFill.fillAmount = fromValue;

                if (progressParticles != null)
                    progressParticles.Play();

                if (progressSound != null && audioSource != null)
                    audioSource.PlayOneShot(progressSound);

                return progressFill.DOFillAmount(toValue, duration)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        if (progressParticles != null)
                            progressParticles.Stop();
                    });
            }

            return null;
        }

        /// <summary>
        /// Pulse progress bar when near completion
        /// </summary>
        public void PulseProgressBar()
        {
            if (progressContainer != null)
            {
                progressContainer.DOScale(1.05f, 0.3f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }
        }

        // ==================== TIER ANIMATIONS ====================

        /// <summary>
        /// Register tier items for animation
        /// </summary>
        public void RegisterTierItems(List<RectTransform> items)
        {
            tierItems = items;
        }

        /// <summary>
        /// Animate tier unlock
        /// </summary>
        public void AnimateTierUnlock(int tierIndex, Action onComplete = null)
        {
            if (tierIndex < 0 || tierIndex >= tierItems.Count) return;

            RectTransform tier = tierItems[tierIndex];

            if (tierUnlockSound != null && audioSource != null)
                audioSource.PlayOneShot(tierUnlockSound);

            Sequence seq = DOTween.Sequence();

            // Scale punch
            seq.Append(tier.DOScale(1.3f, tierUnlockDuration * 0.4f).SetEase(Ease.OutQuad));
            seq.Append(tier.DOScale(1f, tierUnlockDuration * 0.6f).SetEase(Ease.OutBounce));

            // Shake
            seq.Join(tier.DOShakeRotation(tierUnlockDuration, new Vector3(0, 0, 10f), 10));

            // Glow effect
            Image tierImage = tier.GetComponent<Image>();
            if (tierImage != null)
            {
                Color originalColor = tierImage.color;
                seq.Join(tierImage.DOColor(Color.white, tierUnlockDuration * 0.3f)
                    .OnComplete(() => tierImage.DOColor(originalColor, tierUnlockDuration * 0.3f)));
            }

            seq.OnComplete(() =>
            {
                currentTier = tierIndex;
                OnTierUnlocked?.Invoke(tierIndex);
                onComplete?.Invoke();
            });
        }

        /// <summary>
        /// Scroll to specific tier
        /// </summary>
        public Tween ScrollToTier(int tierIndex)
        {
            if (tiersScrollContent == null || tierIndex < 0) return null;

            float targetX = -tierIndex * tierSpacing;
            return tiersScrollContent.DOAnchorPosX(targetX, scrollToTierDuration).SetEase(Ease.OutQuad);
        }

        /// <summary>
        /// Staggered entrance for all tiers
        /// </summary>
        public Sequence AnimateTiersEntrance(float staggerDelay = 0.1f)
        {
            Sequence seq = DOTween.Sequence();

            for (int i = 0; i < tierItems.Count; i++)
            {
                RectTransform tier = tierItems[i];
                tier.localScale = Vector3.zero;

                seq.Insert(i * staggerDelay, tier.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
            }

            return seq;
        }

        // ==================== REWARD CLAIM ====================

        /// <summary>
        /// Play reward claim animation
        /// </summary>
        public void PlayClaimAnimation(RectTransform rewardSource, RectTransform targetPosition, Action onComplete = null)
        {
            if (claimSound != null && audioSource != null)
                audioSource.PlayOneShot(claimSound);

            // Create flying reward clone
            if (claimEffectPrefab != null && claimEffectsParent != null)
            {
                RectTransform effect = Instantiate(claimEffectPrefab, claimEffectsParent);
                effect.position = rewardSource.position;
                effect.gameObject.SetActive(true);

                Sequence seq = DOTween.Sequence();
                seq.Append(effect.DOScale(1.5f, 0.2f).SetEase(Ease.OutQuad));
                seq.Append(effect.DOMove(targetPosition.position, 0.5f).SetEase(Ease.InBack));
                seq.Join(effect.DOScale(0.3f, 0.5f));
                seq.Join(effect.DORotate(new Vector3(0, 0, 360f), 0.5f, RotateMode.FastBeyond360));
                seq.OnComplete(() =>
                {
                    Destroy(effect.gameObject);
                    onComplete?.Invoke();
                });
            }
            else
            {
                // Fallback - just animate source
                rewardSource.DOPunchScale(Vector3.one * 0.3f, 0.3f, 5)
                    .OnComplete(() => onComplete?.Invoke());
            }
        }

        /// <summary>
        /// Play multiple rewards claim with stagger
        /// </summary>
        public void PlayMultipleClaimAnimation(List<RectTransform> rewardSources, RectTransform targetPosition, Action onAllComplete = null)
        {
            StartCoroutine(MultipleClaimCoroutine(rewardSources, targetPosition, onAllComplete));
        }

        private IEnumerator MultipleClaimCoroutine(List<RectTransform> sources, RectTransform target, Action onComplete)
        {
            int completed = 0;

            for (int i = 0; i < sources.Count; i++)
            {
                PlayClaimAnimation(sources[i], target, () => completed++);
                yield return new WaitForSeconds(0.15f);
            }

            yield return new WaitUntil(() => completed >= sources.Count);
            onComplete?.Invoke();
        }

        // ==================== PREMIUM UPGRADE ====================

        /// <summary>
        /// Play premium upgrade celebration
        /// </summary>
        public void PlayPremiumUpgradeAnimation(Action onComplete = null)
        {
            StartCoroutine(PremiumUpgradeCoroutine(onComplete));
        }

        private IEnumerator PremiumUpgradeCoroutine(Action onComplete)
        {
            if (premiumUnlockSound != null && audioSource != null)
                audioSource.PlayOneShot(premiumUnlockSound);

            // Flash overlay
            if (premiumOverlay != null)
            {
                premiumOverlay.gameObject.SetActive(true);
                premiumOverlay.alpha = 0f;
                premiumOverlay.DOFade(1f, 0.3f);
            }

            yield return new WaitForSeconds(0.3f);

            // Particles
            if (premiumParticles != null)
                premiumParticles.Play();

            // Glow pulse
            if (premiumGlow != null)
            {
                premiumGlow.gameObject.SetActive(true);
                premiumGlow.color = new Color(1f, 0.85f, 0f, 0f);

                DOTween.Sequence()
                    .Append(premiumGlow.DOFade(1f, 0.3f))
                    .Append(premiumGlow.DOFade(0.5f, 0.3f))
                    .SetLoops(3);
            }

            yield return new WaitForSeconds(1.5f);

            // Fade out overlay
            if (premiumOverlay != null)
            {
                premiumOverlay.DOFade(0f, 0.5f).OnComplete(() =>
                    premiumOverlay.gameObject.SetActive(false));
            }

            OnPremiumActivated?.Invoke();
            onComplete?.Invoke();
        }

        // ==================== XP ANIMATIONS ====================

        /// <summary>
        /// Show XP gain popup
        /// </summary>
        public void ShowXPGain(Vector3 position, int amount)
        {
            if (xpPopupPrefab == null || xpPopupsParent == null) return;

            RectTransform popup = Instantiate(xpPopupPrefab, xpPopupsParent);
            popup.position = position;

            TextMeshProUGUI text = popup.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.text = $"+{amount} XP";

            popup.gameObject.SetActive(true);
            popup.localScale = Vector3.zero;

            Sequence seq = DOTween.Sequence();
            seq.Append(popup.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
            seq.Join(popup.DOMoveY(popup.position.y + 100f, 1f).SetEase(Ease.OutQuad));
            seq.Insert(0.5f, popup.GetComponent<CanvasGroup>()?.DOFade(0f, 0.5f));
            seq.OnComplete(() => Destroy(popup.gameObject));
        }

        /// <summary>
        /// Animate XP counter
        /// </summary>
        public void AnimateXPCounter(TextMeshProUGUI xpText, int fromValue, int toValue, float duration = 1f)
        {
            int current = fromValue;
            DOTween.To(() => current, x =>
            {
                current = x;
                xpText.text = $"{current} XP";
            }, toValue, duration).SetEase(Ease.OutQuad);

            xpText.transform.DOPunchScale(Vector3.one * 0.2f, duration, 5);
        }

        // ==================== UTILITY ====================

        /// <summary>
        /// Set tier visual state (locked/unlocked)
        /// </summary>
        public void SetTierState(int tierIndex, bool unlocked)
        {
            if (tierIndex < 0 || tierIndex >= tierItems.Count) return;

            RectTransform tier = tierItems[tierIndex];
            CanvasGroup cg = tier.GetComponent<CanvasGroup>();

            if (cg != null)
                cg.alpha = unlocked ? 1f : 0.5f;

            Image tierImage = tier.GetComponent<Image>();
            if (tierImage != null)
            {
                tierImage.color = unlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            }
        }

        private void OnDestroy()
        {
            DOTween.Kill(progressFill);
            DOTween.Kill(progressContainer);
            DOTween.Kill(tiersScrollContent);

            foreach (var tier in tierItems)
            {
                if (tier != null)
                    DOTween.Kill(tier);
            }
        }
    }
}
