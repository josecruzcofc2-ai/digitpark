using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace DigitPark.Animations
{
    /// <summary>
    /// Animated badge/indicator component for notification dots, premium badges,
    /// achievement unlocks, and any small UI indicator that needs attention.
    /// </summary>
    public class BadgeAnimator : MonoBehaviour
    {
        [Header("Entrance Animation")]
        [SerializeField] private float entranceDuration = 0.35f;
        [SerializeField] private float entranceOvershoot = 1.2f;

        [Header("Pulse (Attention)")]
        [SerializeField] private bool autoPulse = false;
        [SerializeField] private float pulseMinScale = 1.0f;
        [SerializeField] private float pulseMaxScale = 1.1f;
        [SerializeField] private float pulseDuration = 0.8f;

        [Header("Update Flash")]
        [SerializeField] private float flashDuration = 0.25f;

        private Tween pulseTween;
        private Tween entranceTween;
        private CanvasGroup canvasGroup;
        private bool wasActive;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            PlayEntrance();
        }

        private void OnDisable()
        {
            KillTweens();
            transform.localScale = Vector3.one;
        }

        private void OnDestroy()
        {
            KillTweens();
        }

        /// <summary>
        /// Show the badge with a pop-in animation.
        /// Use instead of SetActive(true).
        /// </summary>
        public void Show()
        {
            if (gameObject.activeSelf) return;
            gameObject.SetActive(true);
            // OnEnable will call PlayEntrance
        }

        /// <summary>
        /// Hide the badge with a shrink animation.
        /// Use instead of SetActive(false).
        /// </summary>
        public void Hide()
        {
            if (!gameObject.activeSelf) return;

            KillTweens();
            transform.DOScale(0f, 0.15f)
                .SetEase(Ease.InBack)
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    transform.localScale = Vector3.one;
                });
        }

        /// <summary>
        /// Set visibility with optional animation
        /// </summary>
        public void SetVisible(bool visible, bool animate = true)
        {
            if (visible == gameObject.activeSelf) return;

            if (animate)
            {
                if (visible) Show();
                else Hide();
            }
            else
            {
                gameObject.SetActive(visible);
            }
        }

        /// <summary>
        /// Play a punch animation when the badge value updates
        /// </summary>
        public void PlayUpdate()
        {
            KillTweens();
            transform.localScale = Vector3.one;

            transform.DOPunchScale(Vector3.one * 0.25f, flashDuration, 8, 0.5f)
                .SetLink(gameObject)
                .OnComplete(() => StartPulseIfEnabled());
        }

        /// <summary>
        /// Play a color flash + punch when value changes
        /// </summary>
        public void PlayUpdate(Image targetImage, Color flashColor)
        {
            PlayUpdate();

            if (targetImage != null)
            {
                Color original = targetImage.color;
                DOTween.Sequence()
                    .SetLink(targetImage.gameObject)
                    .Append(targetImage.DOColor(flashColor, flashDuration * 0.3f))
                    .Append(targetImage.DOColor(original, flashDuration * 0.7f));
            }
        }

        /// <summary>
        /// Start continuous pulse for attention
        /// </summary>
        public void StartPulse()
        {
            pulseTween?.Kill();
            transform.localScale = Vector3.one * pulseMinScale;
            pulseTween = transform.DOScale(pulseMaxScale, pulseDuration / 2f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject);
        }

        /// <summary>
        /// Stop pulsing
        /// </summary>
        public void StopPulse()
        {
            pulseTween?.Kill();
            transform.localScale = Vector3.one;
        }

        private void PlayEntrance()
        {
            KillTweens();

            transform.localScale = Vector3.zero;

            entranceTween = DOTween.Sequence()
                .SetLink(gameObject)
                .Append(transform.DOScale(entranceOvershoot, entranceDuration * 0.6f).SetEase(Ease.OutQuad))
                .Append(transform.DOScale(1f, entranceDuration * 0.4f).SetEase(Ease.OutBack))
                .OnComplete(() => StartPulseIfEnabled());
        }

        private void StartPulseIfEnabled()
        {
            if (autoPulse)
                StartPulse();
        }

        private void KillTweens()
        {
            pulseTween?.Kill();
            entranceTween?.Kill();
        }
    }
}
