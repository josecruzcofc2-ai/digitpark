using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

namespace DigitPark.Animations
{
    /// <summary>
    /// Universal animated panel component. Replaces instant SetActive(true/false)
    /// with smooth DOTween animations for any popup, panel, or dialog.
    /// Attach to any panel GameObject or use the static helper methods.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class AnimatedPanel : MonoBehaviour
    {
        public enum AnimationType
        {
            ScaleFade,      // Scale 0.85->1 + Fade (default for popups)
            SlideUp,        // Slide from bottom + Fade
            SlideDown,      // Slide from top + Fade
            SlideLeft,      // Slide from left + Fade
            SlideRight,     // Slide from right + Fade
            FadeOnly        // Simple fade in/out
        }

        [Header("Animation Settings")]
        [SerializeField] private AnimationType showAnimation = AnimationType.ScaleFade;
        [SerializeField] private float showDuration = 0.3f;
        [SerializeField] private float hideDuration = 0.2f;
        [SerializeField] private Ease showEase = Ease.OutBack;
        [SerializeField] private Ease hideEase = Ease.InQuad;

        [Header("Backdrop")]
        [SerializeField] private GameObject backdrop;
        [SerializeField] private float backdropFadeDuration = 0.2f;

        [Header("Options")]
        [SerializeField] private bool deactivateOnHide = true;
        [SerializeField] private bool animateOnEnable = false;

        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;
        private Vector2 originalAnchoredPosition;
        private bool originalPositionCached;
        private Sequence currentSequence;
        private CanvasGroup backdropCanvasGroup;
        private bool isAnimating;

        public bool IsVisible { get; private set; }
        public bool IsAnimating => isAnimating;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            rectTransform = GetComponent<RectTransform>();

            CacheOriginalPosition();
            SetupBackdrop();
        }

        private void OnEnable()
        {
            if (animateOnEnable && !isAnimating)
            {
                // Prepare for animation then show
                PrepareForShow();
                AnimateShow(null);
            }
        }

        private void OnDestroy()
        {
            currentSequence?.Kill();
        }

        private void CacheOriginalPosition()
        {
            if (!originalPositionCached && rectTransform != null)
            {
                originalAnchoredPosition = rectTransform.anchoredPosition;
                originalPositionCached = true;
            }
        }

        private void SetupBackdrop()
        {
            if (backdrop != null)
            {
                backdropCanvasGroup = backdrop.GetComponent<CanvasGroup>();
                if (backdropCanvasGroup == null)
                    backdropCanvasGroup = backdrop.AddComponent<CanvasGroup>();
            }
        }

        // ==================== PUBLIC API ====================

        /// <summary>
        /// Show the panel with animation
        /// </summary>
        public void Show(Action onComplete = null)
        {
            CacheOriginalPosition();
            PrepareForShow();
            AnimateShow(onComplete);
        }

        /// <summary>
        /// Hide the panel with animation
        /// </summary>
        public void Hide(Action onComplete = null)
        {
            if (!IsVisible && !isAnimating) return;
            AnimateHide(onComplete);
        }

        /// <summary>
        /// Show instantly without animation
        /// </summary>
        public void ShowInstant()
        {
            CacheOriginalPosition();
            currentSequence?.Kill();

            gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            transform.localScale = Vector3.one;
            rectTransform.anchoredPosition = originalAnchoredPosition;

            if (backdrop != null)
            {
                backdrop.SetActive(true);
                if (backdropCanvasGroup != null)
                    backdropCanvasGroup.alpha = 1f;
            }

            IsVisible = true;
            isAnimating = false;
        }

        /// <summary>
        /// Hide instantly without animation
        /// </summary>
        public void HideInstant()
        {
            currentSequence?.Kill();

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            if (backdrop != null)
                backdrop.SetActive(false);

            if (deactivateOnHide)
                gameObject.SetActive(false);

            IsVisible = false;
            isAnimating = false;
        }

        // ==================== ANIMATION LOGIC ====================

        private void PrepareForShow()
        {
            currentSequence?.Kill();
            gameObject.SetActive(true);

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            switch (showAnimation)
            {
                case AnimationType.ScaleFade:
                    transform.localScale = Vector3.one * 0.85f;
                    break;
                case AnimationType.SlideUp:
                    rectTransform.anchoredPosition = originalAnchoredPosition + new Vector2(0, -80f);
                    break;
                case AnimationType.SlideDown:
                    rectTransform.anchoredPosition = originalAnchoredPosition + new Vector2(0, 80f);
                    break;
                case AnimationType.SlideLeft:
                    rectTransform.anchoredPosition = originalAnchoredPosition + new Vector2(-80f, 0);
                    break;
                case AnimationType.SlideRight:
                    rectTransform.anchoredPosition = originalAnchoredPosition + new Vector2(80f, 0);
                    break;
                case AnimationType.FadeOnly:
                    break;
            }
        }

        private void AnimateShow(Action onComplete)
        {
            isAnimating = true;
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();

            // Backdrop fade
            if (backdrop != null && backdropCanvasGroup != null)
            {
                backdrop.SetActive(true);
                backdropCanvasGroup.alpha = 0f;
                currentSequence.Join(backdropCanvasGroup.DOFade(1f, backdropFadeDuration));
            }

            // Panel animation
            currentSequence.Join(canvasGroup.DOFade(1f, showDuration).SetEase(Ease.OutQuad));

            switch (showAnimation)
            {
                case AnimationType.ScaleFade:
                    currentSequence.Join(transform.DOScale(1f, showDuration).SetEase(showEase));
                    break;
                case AnimationType.SlideUp:
                case AnimationType.SlideDown:
                case AnimationType.SlideLeft:
                case AnimationType.SlideRight:
                    currentSequence.Join(rectTransform.DOAnchorPos(originalAnchoredPosition, showDuration).SetEase(showEase));
                    break;
                case AnimationType.FadeOnly:
                    break;
            }

            currentSequence.OnComplete(() =>
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                IsVisible = true;
                isAnimating = false;
                onComplete?.Invoke();
            });

            currentSequence.SetUpdate(true); // Ignore timeScale
        }

        private void AnimateHide(Action onComplete)
        {
            isAnimating = true;
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            // Panel animation
            currentSequence.Join(canvasGroup.DOFade(0f, hideDuration).SetEase(hideEase));

            switch (showAnimation)
            {
                case AnimationType.ScaleFade:
                    currentSequence.Join(transform.DOScale(0.9f, hideDuration).SetEase(hideEase));
                    break;
                case AnimationType.SlideUp:
                    currentSequence.Join(rectTransform.DOAnchorPosY(originalAnchoredPosition.y - 60f, hideDuration).SetEase(hideEase));
                    break;
                case AnimationType.SlideDown:
                    currentSequence.Join(rectTransform.DOAnchorPosY(originalAnchoredPosition.y + 60f, hideDuration).SetEase(hideEase));
                    break;
                case AnimationType.SlideLeft:
                    currentSequence.Join(rectTransform.DOAnchorPosX(originalAnchoredPosition.x - 60f, hideDuration).SetEase(hideEase));
                    break;
                case AnimationType.SlideRight:
                    currentSequence.Join(rectTransform.DOAnchorPosX(originalAnchoredPosition.x + 60f, hideDuration).SetEase(hideEase));
                    break;
                case AnimationType.FadeOnly:
                    break;
            }

            // Backdrop fade
            if (backdrop != null && backdropCanvasGroup != null)
            {
                currentSequence.Join(backdropCanvasGroup.DOFade(0f, hideDuration));
            }

            currentSequence.OnComplete(() =>
            {
                if (backdrop != null)
                    backdrop.SetActive(false);

                if (deactivateOnHide)
                    gameObject.SetActive(false);

                transform.localScale = Vector3.one;
                rectTransform.anchoredPosition = originalAnchoredPosition;

                IsVisible = false;
                isAnimating = false;
                onComplete?.Invoke();
            });

            currentSequence.SetUpdate(true);
        }

        // ==================== STATIC HELPERS ====================

        /// <summary>
        /// Get or add AnimatedPanel to a GameObject, then show it.
        /// Use this to animate any panel that doesn't have AnimatedPanel attached.
        /// </summary>
        public static AnimatedPanel ShowAnimated(GameObject panel, AnimationType type = AnimationType.ScaleFade, GameObject backdrop = null)
        {
            if (panel == null) return null;

            var animated = panel.GetComponent<AnimatedPanel>();
            if (animated == null)
            {
                animated = panel.AddComponent<AnimatedPanel>();
                animated.showAnimation = type;
                animated.backdrop = backdrop;
                animated.SetupBackdrop();
            }

            animated.Show();
            return animated;
        }

        /// <summary>
        /// Hide an animated panel. Falls back to SetActive(false) if no AnimatedPanel found.
        /// </summary>
        public static void HideAnimated(GameObject panel, Action onComplete = null)
        {
            if (panel == null) return;

            var animated = panel.GetComponent<AnimatedPanel>();
            if (animated != null)
            {
                animated.Hide(onComplete);
            }
            else
            {
                panel.SetActive(false);
                onComplete?.Invoke();
            }
        }
    }
}
