using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace DigitPark.Animations
{
    /// <summary>
    /// Animated empty state display with floating icon, fade-in text,
    /// and breathing CTA button. For empty lists, no results, etc.
    /// </summary>
    public class EmptyStateAnimator : MonoBehaviour
    {
        [Header("Icon")]
        [SerializeField] private Transform icon;
        [SerializeField] private float floatDistance = 8f;
        [SerializeField] private float floatDuration = 2f;

        [Header("Message Text")]
        [SerializeField] private CanvasGroup messageGroup;
        [SerializeField] private float textFadeDelay = 0.3f;
        [SerializeField] private float textFadeDuration = 0.4f;

        [Header("CTA Button")]
        [SerializeField] private Transform ctaButton;
        #pragma warning disable 0414
        [SerializeField] private float breatheMin = 0.97f;
        #pragma warning restore 0414
        [SerializeField] private float breatheMax = 1.03f;
        [SerializeField] private float breatheDuration = 2f;
        [SerializeField] private float ctaDelay = 0.6f;

        private Tween floatTween;
        private Tween breatheTween;
        private Sequence entranceSequence;

        private void OnEnable()
        {
            PlayEntrance();
        }

        private void OnDisable()
        {
            KillAll();
            ResetState();
        }

        private void OnDestroy()
        {
            KillAll();
        }

        /// <summary>
        /// Play the full entrance animation sequence
        /// </summary>
        public void PlayEntrance()
        {
            KillAll();
            entranceSequence = DOTween.Sequence().SetLink(gameObject);

            // Icon entrance: scale pop + start float
            if (icon != null)
            {
                icon.localScale = Vector3.zero;
                entranceSequence.Append(
                    icon.DOScale(1f, 0.4f).SetEase(Ease.OutBack)
                );
                entranceSequence.AppendCallback(() => StartFloat());
            }

            // Message text fade in
            if (messageGroup != null)
            {
                messageGroup.alpha = 0f;
                entranceSequence.Insert(textFadeDelay,
                    messageGroup.DOFade(1f, textFadeDuration).SetEase(Ease.OutQuad)
                );
            }

            // CTA button entrance + breathe
            if (ctaButton != null)
            {
                ctaButton.localScale = Vector3.zero;
                var ctaCG = ctaButton.GetComponent<CanvasGroup>();
                if (ctaCG != null) ctaCG.alpha = 0f;

                entranceSequence.Insert(ctaDelay,
                    ctaButton.DOScale(1f, 0.35f).SetEase(Ease.OutBack)
                );
                if (ctaCG != null)
                {
                    entranceSequence.Insert(ctaDelay,
                        ctaCG.DOFade(1f, 0.3f)
                    );
                }
                entranceSequence.AppendCallback(() => StartBreathe());
            }
        }

        private void StartFloat()
        {
            if (icon == null) return;

            floatTween?.Kill();
            Vector3 startPos = icon.localPosition;
            floatTween = icon.DOLocalMoveY(startPos.y + floatDistance, floatDuration / 2f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(icon.gameObject);
        }

        private void StartBreathe()
        {
            if (ctaButton == null) return;

            breatheTween?.Kill();
            breatheTween = ctaButton.DOScale(breatheMax, breatheDuration / 2f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(ctaButton.gameObject);
        }

        private void ResetState()
        {
            if (icon != null) icon.localScale = Vector3.one;
            if (messageGroup != null) messageGroup.alpha = 1f;
            if (ctaButton != null) ctaButton.localScale = Vector3.one;
        }

        private void KillAll()
        {
            floatTween?.Kill();
            breatheTween?.Kill();
            entranceSequence?.Kill();
        }
    }
}
