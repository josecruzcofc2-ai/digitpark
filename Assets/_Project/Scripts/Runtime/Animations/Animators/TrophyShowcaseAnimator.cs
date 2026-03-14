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
    /// Handles all animations for the Trophy Showcase achievement system.
    /// Features: Entrance animations, unlock celebrations, card effects,
    /// particle systems, and reward animations.
    /// </summary>
    public class TrophyShowcaseAnimator : MonoBehaviour
    {
        [Header("Trophy Grid")]
        [SerializeField] private Transform trophyContainer;
        [SerializeField] private ScrollRect scrollRect;

        [Header("Category Tabs")]
        [SerializeField] private List<Button> categoryTabs;
        [SerializeField] private Image tabIndicator;

        [Header("Header Elements")]
        [SerializeField] private RectTransform headerTrophyIcon;
        [SerializeField] private Image overallProgressFill;

        [Header("Detail Panel")]
        [SerializeField] private RectTransform detailPanel;
        [SerializeField] private CanvasGroup detailPanelCanvasGroup;
        [SerializeField] private RectTransform detailTrophyIcon;
        [SerializeField] private Button claimButton;

        [Header("Celebration Overlay")]
        [SerializeField] private CanvasGroup celebrationOverlay;
        [SerializeField] private RectTransform celebrationTrophyIcon;
        [SerializeField] private ParticleSystem confettiParticles;
        [SerializeField] private ParticleSystem sparkleParticles;
        [SerializeField] private Image celebrationGlow;

        [Header("Effects")]
        [SerializeField] private Image screenFlash;
        [SerializeField] private ParticleSystem ambientParticles;

        [Header("Animation Settings")]
        [SerializeField] private float cardEntranceDelay = 0.08f;
        [SerializeField] private float cardEntranceDuration = 0.4f;
        [SerializeField] private float tabTransitionDuration = 0.3f;
        [SerializeField] private float unlockAnimDuration = 1.5f;

        [Header("Audio")]
        [SerializeField] private AudioClip cardRevealSound;
        [SerializeField] private AudioClip trophyUnlockSound;
        [SerializeField] private AudioClip rewardClaimSound;
        [SerializeField] private AudioClip tabSwitchSound;
        [SerializeField] private AudioClip celebrationSound;

        // Events
        public event Action<int> OnTabChanged;
        #pragma warning disable 0067
        public event Action<string> OnTrophyClicked;
        #pragma warning restore 0067
        public event Action<string> OnTrophyUnlocked;

        private AudioSource audioSource;
        private int currentTabIndex = 0;
        private List<RectTransform> cardTransforms = new List<RectTransform>();
        private Sequence entranceSequence;
        private Sequence celebrationSequence;
        private Tween _headerFloatTween;
        private Tween _celebrationGlowTween;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            if (detailPanelCanvasGroup == null && detailPanel != null)
                detailPanelCanvasGroup = detailPanel.GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            // Initialize detail panel as hidden
            if (detailPanel != null)
            {
                detailPanel.gameObject.SetActive(false);
            }

            if (celebrationOverlay != null)
            {
                celebrationOverlay.gameObject.SetActive(false);
            }

            // Play header trophy floating animation
            if (headerTrophyIcon != null)
            {
                _headerFloatTween = headerTrophyIcon.DOAnchorPosY(headerTrophyIcon.anchoredPosition.y + 8f, 2f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetLink(headerTrophyIcon.gameObject);
            }
        }

        // ==================== ENTRANCE ANIMATIONS ====================

        /// <summary>
        /// Play entrance animation for all trophy cards
        /// </summary>
        public void PlayEntranceAnimation()
        {
            if (trophyContainer == null) return;

            // Kill any existing sequence
            entranceSequence?.Kill();

            // Collect all cards
            cardTransforms.Clear();
            foreach (Transform child in trophyContainer)
            {
                if (child.gameObject.activeSelf)
                {
                    cardTransforms.Add(child as RectTransform);
                }
            }

            // Create staggered entrance
            entranceSequence = DOTween.Sequence().SetLink(gameObject);

            for (int i = 0; i < cardTransforms.Count; i++)
            {
                RectTransform card = cardTransforms[i];
                CanvasGroup cg = card.GetComponent<CanvasGroup>();
                if (cg == null)
                    cg = card.gameObject.AddComponent<CanvasGroup>();

                // Initial state
                card.localScale = Vector3.one * 0.3f;
                cg.alpha = 0f;

                float delay = i * cardEntranceDelay;

                // Animate in
                entranceSequence.Insert(delay, card.DOScale(1f, cardEntranceDuration).SetEase(Ease.OutBack));
                entranceSequence.Insert(delay, cg.DOFade(1f, cardEntranceDuration * 0.7f));

                // Add slight bounce
                entranceSequence.Insert(delay + cardEntranceDuration,
                    card.DOPunchScale(Vector3.one * 0.05f, 0.15f, 3, 0.5f));
            }

            // Play reveal sound staggered
            if (cardRevealSound != null)
            {
                entranceSequence.InsertCallback(0.1f, () => PlaySound(cardRevealSound, 0.3f));
                entranceSequence.InsertCallback(cardTransforms.Count * cardEntranceDelay * 0.5f,
                    () => PlaySound(cardRevealSound, 0.2f));
            }
        }

        /// <summary>
        /// Animate a single card entrance
        /// </summary>
        public void AnimateCardEntrance(RectTransform card, float delay = 0f)
        {
            if (card == null) return;

            CanvasGroup cg = card.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = card.gameObject.AddComponent<CanvasGroup>();

            card.localScale = Vector3.zero;
            cg.alpha = 0f;

            Sequence seq = DOTween.Sequence().SetLink(card.gameObject);
            seq.SetDelay(delay);
            seq.Append(card.DOScale(1.1f, cardEntranceDuration).SetEase(Ease.OutBack));
            seq.Join(cg.DOFade(1f, cardEntranceDuration * 0.7f));
            seq.Append(card.DOScale(1f, 0.1f));
        }

        // ==================== TAB TRANSITIONS ====================

        /// <summary>
        /// Switch to a category tab with animation
        /// </summary>
        public void SwitchTab(int tabIndex, bool animate = true)
        {
            if (tabIndex == currentTabIndex) return;

            currentTabIndex = tabIndex;

            if (!animate)
            {
                UpdateTabVisuals(tabIndex);
                OnTabChanged?.Invoke(tabIndex);
                return;
            }

            PlaySound(tabSwitchSound, 0.5f);

            // Fade out current cards
            FadeOutCards(() =>
            {
                UpdateTabVisuals(tabIndex);
                OnTabChanged?.Invoke(tabIndex);

                // Fade in new cards
                DOVirtual.DelayedCall(0.1f, () => PlayEntranceAnimation()).SetLink(gameObject);
            });
        }

        private void FadeOutCards(Action onComplete)
        {
            Sequence fadeOut = DOTween.Sequence().SetLink(gameObject);

            for (int i = 0; i < cardTransforms.Count; i++)
            {
                RectTransform card = cardTransforms[i];
                CanvasGroup cg = card.GetComponent<CanvasGroup>();
                if (cg == null)
                    cg = card.gameObject.AddComponent<CanvasGroup>();

                fadeOut.Insert(i * 0.02f, cg.DOFade(0f, 0.15f));
                fadeOut.Insert(i * 0.02f, card.DOScale(0.8f, 0.15f));
            }

            fadeOut.OnComplete(() => onComplete?.Invoke());
        }

        private void UpdateTabVisuals(int tabIndex)
        {
            for (int i = 0; i < categoryTabs.Count; i++)
            {
                Image tabImage = categoryTabs[i].GetComponent<Image>();
                if (tabImage == null) continue;

                // Animate tab scale
                categoryTabs[i].transform.DOScale(i == tabIndex ? 1.1f : 1f, 0.2f).SetLink(categoryTabs[i].gameObject);
            }

            // Move indicator if present
            if (tabIndicator != null && categoryTabs.Count > tabIndex)
            {
                RectTransform tabRT = categoryTabs[tabIndex].GetComponent<RectTransform>();
                tabIndicator.rectTransform.DOAnchorPosX(tabRT.anchoredPosition.x, tabTransitionDuration).SetLink(tabIndicator.gameObject);
            }
        }

        // ==================== TROPHY UNLOCK ANIMATION ====================

        /// <summary>
        /// Play the full trophy unlock celebration
        /// </summary>
        public void PlayUnlockCelebration(string trophyName, Sprite trophyIcon, int pointsEarned, Action onComplete = null)
        {
            StartCoroutine(UnlockCelebrationCoroutine(trophyName, trophyIcon, pointsEarned, onComplete));
        }

        private IEnumerator UnlockCelebrationCoroutine(string trophyName, Sprite icon, int points, Action onComplete)
        {
            // Screen flash
            if (screenFlash != null)
            {
                screenFlash.gameObject.SetActive(true);
                screenFlash.color = new Color(1f, 0.95f, 0.7f, 0f);
                var celebFlashSeq = DOTween.Sequence().SetLink(gameObject);
                celebFlashSeq.Append(screenFlash.DOFade(0.7f, 0.15f))
                             .Append(screenFlash.DOFade(0f, 0.4f))
                             .OnComplete(() => { if (screenFlash != null) screenFlash.gameObject.SetActive(false); });
            }

            yield return new WaitForSeconds(0.1f);

            // Show celebration overlay
            if (celebrationOverlay != null)
            {
                celebrationOverlay.gameObject.SetActive(true);
                celebrationOverlay.alpha = 0f;
                celebrationOverlay.DOFade(1f, 0.3f).SetLink(celebrationOverlay.gameObject);

                // Trophy icon animation
                if (celebrationTrophyIcon != null)
                {
                    celebrationTrophyIcon.localScale = Vector3.zero;

                    celebrationSequence = DOTween.Sequence().SetLink(celebrationTrophyIcon.gameObject);
                    celebrationSequence.Append(celebrationTrophyIcon.DOScale(1.3f, 0.4f).SetEase(Ease.OutBack));
                    celebrationSequence.Append(celebrationTrophyIcon.DOScale(1f, 0.2f));

                    // Continuous golden glow pulse
                    celebrationSequence.Insert(0.5f, celebrationTrophyIcon.DOScale(1.1f, 0.8f)
                        .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo));
                }

                // Glow pulse — stored so it can be killed when panel closes
                if (celebrationGlow != null)
                {
                    celebrationGlow.color = new Color(1f, 0.9f, 0.5f, 0f);
                    _celebrationGlowTween?.Kill();
                    _celebrationGlowTween = celebrationGlow.DOFade(0.5f, 0.4f)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetLink(gameObject);
                }
            }

            // Play sounds
            PlaySound(trophyUnlockSound);
            yield return new WaitForSeconds(0.3f);
            PlaySound(celebrationSound);

            // Particles
            if (confettiParticles != null)
                confettiParticles.Play();

            if (sparkleParticles != null)
                sparkleParticles.Play();

            // Update total points with animation
            yield return AnimatePointsIncrease(points);

            OnTrophyUnlocked?.Invoke(trophyName);

            // Wait for user interaction to close
            yield return new WaitForSeconds(unlockAnimDuration);

            onComplete?.Invoke();
        }

        /// <summary>
        /// Close the celebration overlay
        /// </summary>
        public void CloseCelebration(Action onComplete = null)
        {
            if (celebrationOverlay == null)
            {
                onComplete?.Invoke();
                return;
            }

            celebrationSequence?.Kill();
            _celebrationGlowTween?.Kill();

            // Stop particles
            if (confettiParticles != null)
                confettiParticles.Stop();

            if (sparkleParticles != null)
                sparkleParticles.Stop();

            celebrationOverlay.DOFade(0f, 0.3f)
                .SetLink(celebrationOverlay.gameObject)
                .OnComplete(() =>
                {
                    if (celebrationOverlay != null) celebrationOverlay.gameObject.SetActive(false);
                    onComplete?.Invoke();
                });
        }

        private IEnumerator AnimatePointsIncrease(int pointsToAdd)
        {
            // Points system removed - no animation needed
            yield break;
        }

        // ==================== CARD UNLOCK ANIMATION ====================

        /// <summary>
        /// Play unlock animation on a specific trophy card
        /// </summary>
        public void PlayCardUnlockAnimation(RectTransform card, Action onComplete = null)
        {
            if (card == null)
            {
                onComplete?.Invoke();
                return;
            }

            Sequence seq = DOTween.Sequence().SetLink(card.gameObject);

            // Scale up
            seq.Append(card.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack));

            // Flash white
            Image cardImage = card.GetComponentInChildren<Image>();
            if (cardImage != null)
            {
                seq.InsertCallback(0.15f, () =>
                {
                    cardImage.DOColor(Color.white, 0.1f).OnComplete(() =>
                        cardImage.DOColor(new Color(0.06f, 0.12f, 0.1f, 1f), 0.2f));
                });
            }

            // Shake
            seq.Insert(0.3f, card.DOPunchRotation(new Vector3(0, 0, 5f), 0.3f, 8));

            // Scale back
            seq.Append(card.DOScale(1f, 0.2f).SetEase(Ease.OutBounce));

            // Play sound
            seq.InsertCallback(0.15f, () => PlaySound(trophyUnlockSound, 0.7f));

            seq.OnComplete(() => onComplete?.Invoke());
        }

        // ==================== DETAIL PANEL ====================

        /// <summary>
        /// Open the detail panel with animation
        /// </summary>
        public void OpenDetailPanel(RectTransform fromCard = null)
        {
            if (detailPanel == null) return;

            detailPanel.gameObject.SetActive(true);

            if (detailPanelCanvasGroup != null)
                detailPanelCanvasGroup.alpha = 0f;

            Vector2 startPos = fromCard != null ?
                fromCard.anchoredPosition : Vector2.zero;

            detailPanel.anchoredPosition = startPos;
            detailPanel.localScale = Vector3.one * 0.5f;

            Sequence seq = DOTween.Sequence().SetLink(detailPanel.gameObject);
            seq.Append(detailPanel.DOScale(1f, 0.35f).SetEase(Ease.OutBack));
            seq.Join(detailPanel.DOAnchorPos(Vector2.zero, 0.3f).SetEase(Ease.OutQuad));

            if (detailPanelCanvasGroup != null)
                seq.Join(detailPanelCanvasGroup.DOFade(1f, 0.2f));

            // Trophy icon bounce
            if (detailTrophyIcon != null)
            {
                seq.Insert(0.3f, detailTrophyIcon.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5));
            }
        }

        /// <summary>
        /// Close the detail panel with animation
        /// </summary>
        public void CloseDetailPanel(Action onComplete = null)
        {
            if (detailPanel == null)
            {
                onComplete?.Invoke();
                return;
            }

            Sequence seq = DOTween.Sequence().SetLink(detailPanel.gameObject);
            seq.Append(detailPanel.DOScale(0.8f, 0.2f).SetEase(Ease.InBack));

            if (detailPanelCanvasGroup != null)
                seq.Join(detailPanelCanvasGroup.DOFade(0f, 0.15f));

            seq.OnComplete(() =>
            {
                if (detailPanel != null) detailPanel.gameObject.SetActive(false);
                onComplete?.Invoke();
            });
        }

        // ==================== REWARD CLAIM ====================

        /// <summary>
        /// Play reward claim animation
        /// </summary>
        public void PlayRewardClaimAnimation(RectTransform rewardIcon, RectTransform target, Action onComplete = null)
        {
            if (rewardIcon == null)
            {
                onComplete?.Invoke();
                return;
            }

            PlaySound(rewardClaimSound);

            // Create flying copy
            GameObject flyingReward = Instantiate(rewardIcon.gameObject, rewardIcon.parent);
            RectTransform flyingRT = flyingReward.GetComponent<RectTransform>();
            flyingRT.position = rewardIcon.position;
            flyingRT.SetAsLastSibling();

            Vector3 targetPos = target != null ? target.position : Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.9f, Screen.height * 0.95f, 0));

            // Animate
            Sequence seq = DOTween.Sequence().SetLink(gameObject);

            // Pop up
            seq.Append(flyingRT.DOScale(1.5f, 0.2f).SetEase(Ease.OutBack));

            // Fly to target with arc
            Vector3 midPoint = Vector3.Lerp(flyingRT.position, targetPos, 0.5f);
            midPoint.y += 100f;

            seq.Append(flyingRT.DOPath(
                new Vector3[] { flyingRT.position, midPoint, targetPos },
                0.5f,
                PathType.CatmullRom
            ).SetEase(Ease.InQuad));

            seq.Join(flyingRT.DOScale(0.5f, 0.5f));
            seq.Join(flyingRT.DORotate(new Vector3(0, 0, 360f), 0.5f, RotateMode.FastBeyond360));

            seq.OnComplete(() =>
            {
                // Bump target
                if (target != null)
                    target.DOPunchScale(Vector3.one * 0.3f, 0.2f, 5);

                if (flyingReward != null) Destroy(flyingReward);
                onComplete?.Invoke();
            });
        }

        // ==================== PROGRESS BAR ====================

        /// <summary>
        /// Animate overall progress bar fill
        /// </summary>
        public void AnimateProgressBar(float targetFill, float duration = 0.5f)
        {
            if (overallProgressFill == null) return;

            overallProgressFill.DOFillAmount(targetFill, duration).SetEase(Ease.OutQuad).SetLink(overallProgressFill.gameObject);

            // Flash at current progress point
            if (screenFlash != null && targetFill > overallProgressFill.fillAmount + 0.1f)
            {
                screenFlash.gameObject.SetActive(true);
                screenFlash.color = new Color(1f, 0.95f, 0.7f, 0f);
                var flashSeq = DOTween.Sequence().SetLink(gameObject);
                flashSeq.Append(screenFlash.DOFade(0.2f, 0.1f))
                        .Append(screenFlash.DOFade(0f, 0.2f))
                        .OnComplete(() => { if (screenFlash != null) screenFlash.gameObject.SetActive(false); });
            }
        }

        // ==================== CARD INTERACTION ====================

        /// <summary>
        /// Play card hover effect
        /// </summary>
        public void PlayCardHover(RectTransform card)
        {
            if (card == null) return;
            card.DOScale(1.08f, 0.15f).SetEase(Ease.OutCubic).SetLink(card.gameObject);
        }

        /// <summary>
        /// Reset card from hover
        /// </summary>
        public void ResetCardHover(RectTransform card)
        {
            if (card == null) return;
            card.DOScale(1f, 0.15f).SetEase(Ease.OutCubic).SetLink(card.gameObject);
        }

        /// <summary>
        /// Play card press effect
        /// </summary>
        public void PlayCardPress(RectTransform card)
        {
            if (card == null) return;
            card.DOPunchScale(Vector3.one * 0.05f, 0.2f, 5, 0.5f).SetLink(card.gameObject);
        }

        // ==================== SHINE EFFECT ====================

        /// <summary>
        /// Play shine sweep across a card
        /// </summary>
        public void PlayShineEffect(RectTransform card, Image shineImage)
        {
            if (card == null || shineImage == null) return;

            shineImage.gameObject.SetActive(true);
            RectTransform shineRT = shineImage.rectTransform;

            // Reset position
            shineRT.anchoredPosition = new Vector2(-card.rect.width, 0);

            // Sweep across
            shineRT.DOAnchorPosX(card.rect.width, 0.8f)
                .SetEase(Ease.Linear)
                .SetLink(shineImage.gameObject)
                .OnComplete(() => { if (shineImage != null) shineImage.gameObject.SetActive(false); });
        }

        // ==================== UTILITY ====================

        private void PlaySound(AudioClip clip, float volume = 1f)
        {
            if (clip != null && audioSource != null)
                audioSource.PlayOneShot(clip, volume);
        }

        /// <summary>
        /// Set the trophy container reference
        /// </summary>
        public void SetTrophyContainer(Transform container)
        {
            trophyContainer = container;
        }

        private void OnDestroy()
        {
            entranceSequence?.Kill();
            celebrationSequence?.Kill();
            _headerFloatTween?.Kill();
            _celebrationGlowTween?.Kill();
        }
    }
}
