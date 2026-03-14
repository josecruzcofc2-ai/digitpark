using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

namespace DigitPark.Animations
{
    /// <summary>
    /// Handles animated transitions between tab content panels.
    /// Attach to the parent that manages tabs. Call SwitchTab() to animate.
    /// </summary>
    public class TabTransitionAnimator : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float transitionDuration = 0.25f;
        [SerializeField] private float slideDistance = 30f;
        [SerializeField] private Ease transitionEase = Ease.OutQuad;

        [Header("Tab Indicator")]
        [SerializeField] private RectTransform tabIndicator;
        [SerializeField] private float indicatorDuration = 0.25f;
        [SerializeField] private Ease indicatorEase = Ease.OutCubic;

        [Header("Tab Buttons Scale")]
        [SerializeField] private float activeTabScale = 1.05f;
        [SerializeField] private float inactiveTabScale = 1.0f;
        [SerializeField] private float tabScaleDuration = 0.2f;

        private GameObject currentContent;
        private int currentIndex = -1;
        private Sequence contentSequence;
        private bool isTransitioning;

        public bool IsTransitioning => isTransitioning;
        public int CurrentIndex => currentIndex;

        private void OnDestroy()
        {
            contentSequence?.Kill();
        }

        /// <summary>
        /// Switch to a new tab with animated transition
        /// </summary>
        /// <param name="newContent">The content panel to show</param>
        /// <param name="tabIndex">Index of the new tab (used for slide direction)</param>
        /// <param name="tabButton">Optional tab button to animate scale</param>
        /// <param name="previousTabButton">Optional previous tab button to de-scale</param>
        public void SwitchTab(GameObject newContent, int tabIndex, Transform tabButton = null, Transform previousTabButton = null)
        {
            if (newContent == null || isTransitioning) return;
            if (currentContent == newContent) return;

            bool slideRight = tabIndex > currentIndex;
            var oldContent = currentContent;
            currentContent = newContent;

            int oldIndex = currentIndex;
            currentIndex = tabIndex;

            isTransitioning = true;
            contentSequence?.Kill();
            contentSequence = DOTween.Sequence().SetLink(gameObject);

            // Animate out old content
            if (oldContent != null)
            {
                var oldCG = GetOrAddCanvasGroup(oldContent);
                var oldRT = oldContent.GetComponent<RectTransform>();
                Vector2 oldOriginalPos = oldRT != null ? oldRT.anchoredPosition : Vector2.zero;

                float exitX = slideRight ? -slideDistance : slideDistance;

                contentSequence.Join(oldCG.DOFade(0f, transitionDuration * 0.6f).SetEase(Ease.InQuad));
                if (oldRT != null)
                {
                    contentSequence.Join(oldRT.DOAnchorPosX(oldOriginalPos.x + exitX, transitionDuration * 0.6f)
                        .SetEase(Ease.InQuad)
                        .OnComplete(() =>
                        {
                            if (oldContent != null) oldContent.SetActive(false);
                            if (oldRT != null) oldRT.anchoredPosition = oldOriginalPos;
                            if (oldCG != null) oldCG.alpha = 1f;
                        }));
                }
            }

            // Animate in new content
            var newCG = GetOrAddCanvasGroup(newContent);
            var newRT = newContent.GetComponent<RectTransform>();
            Vector2 newOriginalPos = newRT != null ? newRT.anchoredPosition : Vector2.zero;

            float enterX = slideRight ? slideDistance : -slideDistance;

            newCG.alpha = 0f;
            newContent.SetActive(true);
            if (newRT != null)
                newRT.anchoredPosition = newOriginalPos + new Vector2(enterX, 0);

            float enterDelay = oldContent != null ? transitionDuration * 0.3f : 0f;

            contentSequence.Insert(enterDelay, newCG.DOFade(1f, transitionDuration).SetEase(Ease.OutQuad));
            if (newRT != null)
                contentSequence.Insert(enterDelay, newRT.DOAnchorPos(newOriginalPos, transitionDuration).SetEase(transitionEase));

            // Tab button animations
            if (tabButton != null)
                contentSequence.Join(tabButton.DOScale(activeTabScale, tabScaleDuration).SetEase(Ease.OutBack));

            if (previousTabButton != null)
                contentSequence.Join(previousTabButton.DOScale(inactiveTabScale, tabScaleDuration).SetEase(Ease.OutQuad));

            // Tab indicator
            if (tabIndicator != null && tabButton != null)
            {
                var tabRT = tabButton.GetComponent<RectTransform>();
                if (tabRT != null)
                {
                    contentSequence.Join(tabIndicator.DOAnchorPosX(tabRT.anchoredPosition.x, indicatorDuration).SetEase(indicatorEase));
                }
            }

            contentSequence.OnComplete(() => isTransitioning = false);
        }

        /// <summary>
        /// Set initial tab without animation
        /// </summary>
        public void SetInitialTab(GameObject content, int tabIndex)
        {
            currentContent = content;
            currentIndex = tabIndex;

            if (content != null)
            {
                content.SetActive(true);
                var cg = GetOrAddCanvasGroup(content);
                cg.alpha = 1f;
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
