using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

namespace DigitPark.Animations
{
    /// <summary>
    /// Animated loading state component with spinner rotation,
    /// shimmer skeleton effect, and content entrance on load complete.
    /// </summary>
    public class AnimatedLoadingState : MonoBehaviour
    {
        [Header("Loading Indicator")]
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private Transform spinner;
        [SerializeField] private float spinSpeed = 360f;

        [Header("Content Panel")]
        [SerializeField] private GameObject contentPanel;

        [Header("Skeleton Shimmer")]
        [SerializeField] private List<Image> skeletonPlaceholders;
        [SerializeField] private float shimmerDuration = 1.2f;
        [SerializeField] private Color shimmerBaseColor = new Color(0.12f, 0.15f, 0.2f, 1f);
        [SerializeField] private Color shimmerHighlightColor = new Color(0.18f, 0.22f, 0.28f, 1f);

        [Header("Transition")]
        [SerializeField] private float transitionDuration = 0.3f;

        private Tween spinnerTween;
        private Sequence shimmerSequence;
        private bool isLoading;

        public bool IsLoading => isLoading;

        private void OnDestroy()
        {
            spinnerTween?.Kill();
            shimmerSequence?.Kill();
        }

        /// <summary>
        /// Show loading state with spinner and optional skeleton shimmer
        /// </summary>
        public void ShowLoading()
        {
            isLoading = true;

            // Show loading panel
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(true);
                var cg = GetOrAddCanvasGroup(loadingPanel);
                cg.alpha = 0f;
                cg.DOFade(1f, 0.2f).SetLink(loadingPanel);
            }

            // Hide content
            if (contentPanel != null)
                contentPanel.SetActive(false);

            // Start spinner
            StartSpinner();

            // Start skeleton shimmer
            StartShimmer();
        }

        /// <summary>
        /// Transition from loading to content with animation
        /// </summary>
        public void ShowContent()
        {
            isLoading = false;
            StopSpinner();
            StopShimmer();

            var seq = DOTween.Sequence().SetLink(gameObject);

            // Fade out loading
            if (loadingPanel != null)
            {
                var loadingCG = GetOrAddCanvasGroup(loadingPanel);
                seq.Append(loadingCG.DOFade(0f, transitionDuration * 0.5f)
                    .OnComplete(() => { if (loadingPanel != null) loadingPanel.SetActive(false); }));
            }

            // Fade in content
            if (contentPanel != null)
            {
                var contentCG = GetOrAddCanvasGroup(contentPanel);
                contentCG.alpha = 0f;
                contentPanel.SetActive(true);

                seq.Append(contentCG.DOFade(1f, transitionDuration).SetEase(Ease.OutQuad));
            }
        }

        /// <summary>
        /// Show error state (hides loading, shows content area for error message)
        /// </summary>
        public void ShowError()
        {
            isLoading = false;
            StopSpinner();
            StopShimmer();

            if (loadingPanel != null)
                loadingPanel.SetActive(false);

            if (contentPanel != null)
            {
                contentPanel.SetActive(true);
                var cg = GetOrAddCanvasGroup(contentPanel);
                cg.alpha = 1f;
            }
        }

        private void StartSpinner()
        {
            if (spinner == null) return;

            spinnerTween?.Kill();
            spinner.localRotation = Quaternion.identity;
            spinnerTween = spinner.DORotate(new Vector3(0, 0, -360f), 360f / spinSpeed, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1);
        }

        private void StopSpinner()
        {
            spinnerTween?.Kill();
        }

        private void StartShimmer()
        {
            if (skeletonPlaceholders == null || skeletonPlaceholders.Count == 0) return;

            shimmerSequence?.Kill();
            shimmerSequence = DOTween.Sequence();

            foreach (var placeholder in skeletonPlaceholders)
            {
                if (placeholder == null) continue;
                placeholder.color = shimmerBaseColor;

                shimmerSequence.Join(
                    placeholder.DOColor(shimmerHighlightColor, shimmerDuration / 2f)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo)
                );
            }
        }

        private void StopShimmer()
        {
            shimmerSequence?.Kill();
        }

        private CanvasGroup GetOrAddCanvasGroup(GameObject go)
        {
            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null) cg = go.AddComponent<CanvasGroup>();
            return cg;
        }
    }
}
