using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

namespace DigitPark.Animations
{
    /// <summary>
    /// Handles animated transitions for bottom navigation bar.
    /// Animates content switch + nav icon selection feedback.
    /// </summary>
    public class NavTransitionAnimator : MonoBehaviour
    {
        [Header("Content Transition")]
        [SerializeField] private float contentFadeDuration = 0.2f;
        [SerializeField] private float contentSlideDuration = 0.25f;
        [SerializeField] private float slideDistance = 40f;

        [Header("Nav Icon Animation")]
        [SerializeField] private float iconBounceDuration = 0.3f;
        [SerializeField] private float selectedScale = 1.15f;
        [SerializeField] private float unselectedScale = 1.0f;

        [Header("Nav Icon Colors")]
        [SerializeField] private Color selectedColor = new Color(0f, 0.96f, 1f); // Neon cyan
        [SerializeField] private Color unselectedColor = new Color(0.5f, 0.5f, 0.55f);
        [SerializeField] private float colorTransitionDuration = 0.2f;

        private int currentIndex = -1;
        private GameObject currentContent;
        private Transform currentNavIcon;
        private Sequence contentSequence;
        private bool isTransitioning;

        public int CurrentIndex => currentIndex;
        public bool IsTransitioning => isTransitioning;

        private void OnDestroy()
        {
            contentSequence?.Kill();
        }

        /// <summary>
        /// Navigate to a new section with animated transition
        /// </summary>
        public void NavigateTo(GameObject newContent, int newIndex, Transform newNavIcon,
            Transform[] allNavIcons = null, Action onMidTransition = null)
        {
            if (isTransitioning || newContent == null) return;
            if (currentContent == newContent && currentIndex == newIndex) return;

            bool goingRight = newIndex > currentIndex;
            var oldContent = currentContent;
            var oldNavIcon = currentNavIcon;

            currentContent = newContent;
            currentIndex = newIndex;
            currentNavIcon = newNavIcon;
            isTransitioning = true;

            contentSequence?.Kill();
            contentSequence = DOTween.Sequence().SetLink(gameObject);

            // Animate out old content
            if (oldContent != null)
            {
                var oldCG = GetOrAddCanvasGroup(oldContent);
                float exitDir = goingRight ? -1f : 1f;

                contentSequence.Append(oldCG.DOFade(0f, contentFadeDuration).SetEase(Ease.InQuad));

                var oldRT = oldContent.GetComponent<RectTransform>();
                if (oldRT != null)
                {
                    Vector2 oldOriginal = oldRT.anchoredPosition;
                    contentSequence.Join(
                        oldRT.DOAnchorPosX(oldOriginal.x + slideDistance * exitDir, contentFadeDuration)
                            .SetEase(Ease.InQuad)
                            .OnComplete(() =>
                            {
                                if (oldContent != null) oldContent.SetActive(false);
                                if (oldRT != null) oldRT.anchoredPosition = oldOriginal;
                                if (oldCG != null) oldCG.alpha = 1f;
                            }));
                }
            }

            // Mid-transition callback
            contentSequence.AppendCallback(() => onMidTransition?.Invoke());

            // Animate in new content
            var newCG = GetOrAddCanvasGroup(newContent);
            var newRT = newContent.GetComponent<RectTransform>();
            Vector2 newOriginal = newRT != null ? newRT.anchoredPosition : Vector2.zero;

            float enterDir = goingRight ? 1f : -1f;
            newCG.alpha = 0f;
            newContent.SetActive(true);
            if (newRT != null)
                newRT.anchoredPosition = newOriginal + new Vector2(slideDistance * enterDir, 0);

            contentSequence.Append(newCG.DOFade(1f, contentSlideDuration).SetEase(Ease.OutQuad));
            if (newRT != null)
                contentSequence.Join(newRT.DOAnchorPos(newOriginal, contentSlideDuration).SetEase(Ease.OutQuad));

            // Animate nav icons
            AnimateNavIcons(newNavIcon, allNavIcons);

            contentSequence.OnComplete(() => isTransitioning = false);
        }

        /// <summary>
        /// Set initial navigation state without animation
        /// </summary>
        public void SetInitial(GameObject content, int index, Transform navIcon, Transform[] allNavIcons = null)
        {
            currentContent = content;
            currentIndex = index;
            currentNavIcon = navIcon;

            if (content != null)
            {
                content.SetActive(true);
                var cg = GetOrAddCanvasGroup(content);
                cg.alpha = 1f;
            }

            // Set nav icon colors
            if (allNavIcons != null)
            {
                foreach (var icon in allNavIcons)
                {
                    if (icon == null) continue;
                    var img = icon.GetComponent<Image>();
                    if (img != null)
                        img.color = (icon == navIcon) ? selectedColor : unselectedColor;
                    icon.localScale = (icon == navIcon) ? Vector3.one * selectedScale : Vector3.one * unselectedScale;
                }
            }
        }

        private void AnimateNavIcons(Transform selectedIcon, Transform[] allIcons)
        {
            if (allIcons == null) return;

            foreach (var icon in allIcons)
            {
                if (icon == null) continue;

                bool isSelected = (icon == selectedIcon);
                float targetScale = isSelected ? selectedScale : unselectedScale;
                Color targetColor = isSelected ? selectedColor : unselectedColor;

                // Scale animation
                if (isSelected)
                {
                    // Bounce effect for selected
                    icon.DOKill();
                    icon.DOScale(targetScale, iconBounceDuration).SetEase(Ease.OutBack);
                }
                else
                {
                    icon.DOKill();
                    icon.DOScale(targetScale, iconBounceDuration * 0.6f).SetEase(Ease.OutQuad);
                }

                // Color transition
                var img = icon.GetComponent<Image>();
                if (img != null)
                {
                    img.DOKill();
                    img.DOColor(targetColor, colorTransitionDuration);
                }
            }
        }

        private CanvasGroup GetOrAddCanvasGroup(GameObject go)
        {
            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null) cg = go.AddComponent<CanvasGroup>();
            return cg;
        }
    }
}
