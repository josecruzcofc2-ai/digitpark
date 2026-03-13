using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;
using System.Collections;

namespace DigitPark.Animations
{
    /// <summary>
    /// Handles matchmaking animations including searching effects,
    /// opponent found reveal, and VS screen animations.
    /// </summary>
    public class MatchmakingAnimator : MonoBehaviour
    {
        [Header("Player Cards")]
        [SerializeField] private RectTransform playerCard;
        [SerializeField] private RectTransform opponentCard;
        [SerializeField] private Image playerAvatar;
        [SerializeField] private Image opponentAvatar;
        [SerializeField] private TextMeshProUGUI playerName;
        [SerializeField] private TextMeshProUGUI opponentName;

        [Header("VS Element")]
        [SerializeField] private RectTransform vsContainer;
        [SerializeField] private TextMeshProUGUI vsText;
        [SerializeField] private Image vsGlow;
        [SerializeField] private ParticleSystem vsParticles;

        [Header("Search Animation")]
        [SerializeField] private RectTransform searchIndicator;
        [SerializeField] private Image searchRing;
        [SerializeField] private Image[] searchDots;
        [SerializeField] private TextMeshProUGUI searchingText;

        [Header("Opponent Found")]
        [SerializeField] private Image opponentSilhouette;
        [SerializeField] private ParticleSystem revealParticles;

        [Header("Background")]
        [SerializeField] private CanvasGroup backgroundDimmer;
        [SerializeField] private Image screenFlash;

        [Header("Animation Settings")]
        [SerializeField] private float cardSlideDistance = 800f;
        [SerializeField] private float cardSlideDuration = 0.5f;
        [SerializeField] private float vsPopDuration = 0.4f;
        [SerializeField] private float searchRotationSpeed = 2f;

        [Header("Audio")]
        [SerializeField] private AudioClip searchingSound;
        [SerializeField] private AudioClip opponentFoundSound;
        [SerializeField] private AudioClip vsImpactSound;
        [SerializeField] private AudioClip battleStartSound;

        // Events
        public event Action OnMatchFound;
        public event Action OnBattleReady;

        private AudioSource audioSource;
        private Tween searchRotationTween;
        private Sequence searchDotsSequence;
        private Sequence cardSlideSequence;
        private Sequence vsSequence;
        private Sequence quickVSSequence;
        private Tween vsGlowTween;
        private Tween searchScaleTween;
        private bool isSearching;
        private Coroutine searchTextCoroutine;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        // ==================== SEARCH ANIMATION ====================

        /// <summary>
        /// Start searching for opponent animation
        /// </summary>
        public void StartSearchAnimation()
        {
            isSearching = true;

            // Hide opponent card, show search indicator
            if (opponentCard != null)
                opponentCard.gameObject.SetActive(false);

            if (searchIndicator != null)
            {
                searchIndicator.gameObject.SetActive(true);
                searchIndicator.localScale = Vector3.zero;
                searchScaleTween?.Kill();
                searchScaleTween = searchIndicator.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
            }

            // Rotating search ring
            if (searchRing != null)
            {
                searchRotationTween = searchRing.transform
                    .DORotate(new Vector3(0, 0, -360f), searchRotationSpeed, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1);
            }

            // Animate dots
            AnimateSearchDots();

            // Searching text
            if (searchingText != null)
            {
                searchingText.gameObject.SetActive(true);
                if (searchTextCoroutine != null)
                    StopCoroutine(searchTextCoroutine);
                searchTextCoroutine = StartCoroutine(AnimateSearchingText());
            }

            // Play searching sound (looping)
            if (searchingSound != null && audioSource != null)
            {
                audioSource.clip = searchingSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }

        /// <summary>
        /// Stop search animation
        /// </summary>
        public void StopSearchAnimation()
        {
            isSearching = false;

            if (searchTextCoroutine != null)
            {
                StopCoroutine(searchTextCoroutine);
                searchTextCoroutine = null;
            }

            searchRotationTween?.Kill();
            searchDotsSequence?.Kill();

            if (audioSource != null)
                audioSource.Stop();

            if (searchIndicator != null)
                searchIndicator.DOScale(0f, 0.2f).OnComplete(() =>
                    searchIndicator.gameObject.SetActive(false));

            if (searchingText != null)
                searchingText.gameObject.SetActive(false);
        }

        private void AnimateSearchDots()
        {
            if (searchDots == null || searchDots.Length == 0) return;

            searchDotsSequence = DOTween.Sequence();

            for (int i = 0; i < searchDots.Length; i++)
            {
                int index = i;
                if (searchDots[index] == null) continue;
                searchDotsSequence.Insert(i * 0.2f, searchDots[index].DOFade(1f, 0.3f));
                searchDotsSequence.Insert(i * 0.2f + 0.5f, searchDots[index].DOFade(0.3f, 0.3f));
            }

            searchDotsSequence.SetLoops(-1).SetLink(gameObject);
        }

        private IEnumerator AnimateSearchingText()
        {
            string[] states = { "Searching", "Searching.", "Searching..", "Searching..." };
            int index = 0;

            while (isSearching)
            {
                if (searchingText != null)
                    searchingText.text = states[index];

                index = (index + 1) % states.Length;
                yield return new WaitForSeconds(0.3f);
            }
        }

        // ==================== OPPONENT FOUND ====================

        /// <summary>
        /// Play opponent found sequence
        /// </summary>
        public void PlayOpponentFoundSequence(Sprite opponentAvatarSprite, string opponentNameStr, Action onComplete = null)
        {
            StartCoroutine(OpponentFoundCoroutine(opponentAvatarSprite, opponentNameStr, onComplete));
        }

        private IEnumerator OpponentFoundCoroutine(Sprite avatarSprite, string nameStr, Action onComplete)
        {
            // Stop search
            StopSearchAnimation();

            // Play found sound
            if (opponentFoundSound != null && audioSource != null)
                audioSource.PlayOneShot(opponentFoundSound);

            // Flash
            if (screenFlash != null)
            {
                screenFlash.gameObject.SetActive(true);
                screenFlash.color = new Color(1, 1, 1, 0);
                var flashSeq = DOTween.Sequence().SetLink(gameObject);
                flashSeq.Append(screenFlash.DOFade(0.8f, 0.1f))
                        .Append(screenFlash.DOFade(0f, 0.3f))
                        .OnComplete(() => { if (screenFlash != null) screenFlash.gameObject.SetActive(false); });
            }

            yield return new WaitForSeconds(0.2f);

            // Show silhouette first
            if (opponentSilhouette != null)
            {
                opponentSilhouette.gameObject.SetActive(true);
                opponentSilhouette.color = Color.black;
                opponentSilhouette.transform.localScale = Vector3.zero;
                opponentSilhouette.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
            }

            yield return new WaitForSeconds(0.5f);

            // Reveal opponent
            if (revealParticles != null)
                revealParticles.Play();

            if (opponentSilhouette != null)
                opponentSilhouette.DOFade(0f, 0.3f).OnComplete(() =>
                    opponentSilhouette.gameObject.SetActive(false));

            // Show actual opponent card
            if (opponentCard != null)
            {
                if (opponentAvatar != null)
                    opponentAvatar.sprite = avatarSprite;

                if (opponentName != null)
                    opponentName.text = nameStr;

                opponentCard.gameObject.SetActive(true);
                opponentCard.localScale = Vector3.zero;
                opponentCard.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
            }

            OnMatchFound?.Invoke();
            yield return new WaitForSeconds(0.5f);

            onComplete?.Invoke();
        }

        // ==================== VS ANIMATION ====================

        /// <summary>
        /// Play the full VS screen animation
        /// </summary>
        public void PlayVSAnimation(Action onComplete = null)
        {
            StartCoroutine(VSAnimationCoroutine(onComplete));
        }

        private IEnumerator VSAnimationCoroutine(Action onComplete)
        {
            // Setup - cards off screen
            if (playerCard != null)
            {
                playerCard.anchoredPosition = new Vector2(-cardSlideDistance, playerCard.anchoredPosition.y);
                playerCard.gameObject.SetActive(true);
            }

            if (opponentCard != null)
            {
                opponentCard.anchoredPosition = new Vector2(cardSlideDistance, opponentCard.anchoredPosition.y);
                opponentCard.gameObject.SetActive(true);
            }

            if (vsContainer != null)
            {
                vsContainer.localScale = Vector3.zero;
            }

            // Slide cards in
            cardSlideSequence?.Kill();
            cardSlideSequence = DOTween.Sequence();

            if (playerCard != null)
                cardSlideSequence.Append(playerCard.DOAnchorPosX(0, cardSlideDuration).SetEase(Ease.OutBack));

            if (opponentCard != null)
                cardSlideSequence.Join(opponentCard.DOAnchorPosX(0, cardSlideDuration).SetEase(Ease.OutBack));

            yield return cardSlideSequence.WaitForCompletion();

            // VS Impact
            if (vsImpactSound != null && audioSource != null)
                audioSource.PlayOneShot(vsImpactSound);

            if (vsContainer != null)
            {
                vsContainer.gameObject.SetActive(true);

                // Pop in with overshoot
                vsSequence?.Kill();
                vsSequence = DOTween.Sequence();
                vsSequence.Append(vsContainer.DOScale(1.5f, vsPopDuration * 0.4f).SetEase(Ease.OutQuad));
                vsSequence.Append(vsContainer.DOScale(1f, vsPopDuration * 0.6f).SetEase(Ease.OutBounce));
                vsSequence.Join(vsContainer.DOShakeRotation(vsPopDuration, new Vector3(0, 0, 15f), 10));
            }

            // VS particles
            if (vsParticles != null)
                vsParticles.Play();

            // VS glow pulse
            if (vsGlow != null)
            {
                vsGlow.gameObject.SetActive(true);
                vsGlowTween?.Kill();
                // Assign before OnComplete to avoid race condition where 0.3f tween
                // finishes in same frame before the outer assignment runs
                vsGlowTween = vsGlow.DOFade(1f, 0.3f);
                vsGlowTween.OnComplete(() =>
                {
                    if (vsGlow != null)
                        vsGlowTween = vsGlow.DOFade(0.5f, 0.5f).SetLoops(-1, LoopType.Yoyo);
                });
            }

            // Screen shake
            if (UIAnimationManager.Instance != null)
                UIAnimationManager.Instance.ScreenShake(15f, 0.3f);

            yield return new WaitForSeconds(1f);

            OnBattleReady?.Invoke();
            onComplete?.Invoke();
        }

        /// <summary>
        /// Play compact VS pop (for quick matches)
        /// </summary>
        public void PlayQuickVSPop()
        {
            if (vsContainer == null) return;

            vsContainer.gameObject.SetActive(true);
            vsContainer.localScale = Vector3.zero;

            quickVSSequence?.Kill();
            quickVSSequence = DOTween.Sequence()
                .Append(vsContainer.DOScale(1.3f, 0.15f).SetEase(Ease.OutQuad))
                .Append(vsContainer.DOScale(1f, 0.25f).SetEase(Ease.OutBounce));

            if (vsImpactSound != null && audioSource != null)
                audioSource.PlayOneShot(vsImpactSound);
        }

        // ==================== BATTLE START ====================

        /// <summary>
        /// Transition to battle (fly out animation)
        /// </summary>
        public void TransitionToBattle(Action onComplete = null)
        {
            if (battleStartSound != null && audioSource != null)
                audioSource.PlayOneShot(battleStartSound);

            Sequence exitSeq = DOTween.Sequence().SetLink(gameObject);

            // Cards fly out
            if (playerCard != null)
                exitSeq.Append(playerCard.DOAnchorPosX(-cardSlideDistance * 1.5f, 0.3f).SetEase(Ease.InBack));

            if (opponentCard != null)
                exitSeq.Join(opponentCard.DOAnchorPosX(cardSlideDistance * 1.5f, 0.3f).SetEase(Ease.InBack));

            // VS flies up
            if (vsContainer != null)
                exitSeq.Join(vsContainer.DOAnchorPosY(1000f, 0.3f).SetEase(Ease.InBack));

            exitSeq.OnComplete(() => onComplete?.Invoke());
        }

        // ==================== UTILITY ====================

        /// <summary>
        /// Set player info
        /// </summary>
        public void SetPlayerInfo(Sprite avatar, string name)
        {
            if (playerAvatar != null)
                playerAvatar.sprite = avatar;

            if (playerName != null)
                playerName.text = name;
        }

        /// <summary>
        /// Reset all elements
        /// </summary>
        public void Reset()
        {
            StopSearchAnimation();

            if (playerCard != null)
            {
                playerCard.DOKill();
                playerCard.anchoredPosition = Vector2.zero;
                playerCard.localScale = Vector3.one;
            }

            if (opponentCard != null)
            {
                opponentCard.DOKill();
                opponentCard.gameObject.SetActive(false);
            }

            if (vsContainer != null)
            {
                vsContainer.DOKill();
                vsContainer.gameObject.SetActive(false);
            }

            if (searchIndicator != null)
                searchIndicator.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (searchTextCoroutine != null)
                StopCoroutine(searchTextCoroutine);
            searchRotationTween?.Kill();
            searchDotsSequence?.Kill();
            searchScaleTween?.Kill();
            cardSlideSequence?.Kill();
            vsSequence?.Kill();
            quickVSSequence?.Kill();
            vsGlowTween?.Kill();
            DOTween.Kill(playerCard);
            DOTween.Kill(opponentCard);
            DOTween.Kill(vsContainer);
            if (vsGlow != null) vsGlow.DOKill();
            if (screenFlash != null) screenFlash.DOKill();
            if (opponentSilhouette != null) DOTween.Kill(opponentSilhouette);
        }
    }
}
