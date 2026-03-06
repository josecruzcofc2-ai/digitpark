using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

namespace DigitPark.Animations
{
    /// <summary>
    /// Collection of reusable UI effect components.
    /// Attach to any UI element for automatic effects.
    /// </summary>
    public class UIEffects : MonoBehaviour
    {
        // ==================== SHINE EFFECT ====================
        [System.Serializable]
        public class ShineEffect
        {
            public bool enabled = false;
            public RectTransform shineTransform;
            public float duration = 2f;
            public float delay = 3f;
            public float startX = -200f;
            public float endX = 200f;
        }

        // ==================== PULSE EFFECT ====================
        [System.Serializable]
        public class PulseEffect
        {
            public bool enabled = false;
            public float minScale = 0.95f;
            public float maxScale = 1.05f;
            public float duration = 1f;
        }

        // ==================== FLOAT EFFECT ====================
        [System.Serializable]
        public class FloatEffect
        {
            public bool enabled = false;
            public float distance = 10f;
            public float duration = 2f;
        }

        // ==================== GLOW PULSE ====================
        [System.Serializable]
        public class GlowPulseEffect
        {
            public bool enabled = false;
            public Image glowImage;
            public float minAlpha = 0.3f;
            public float maxAlpha = 0.8f;
            public float duration = 1.5f;
        }

        // ==================== ROTATION ====================
        [System.Serializable]
        public class RotationEffect
        {
            public bool enabled = false;
            public float speed = 30f;
            public bool continuous = true;
        }

        // ==================== COLOR CYCLE ====================
        [System.Serializable]
        public class ColorCycleEffect
        {
            public bool enabled = false;
            public Image targetImage;
            public Color[] colors;
            public float cycleDuration = 2f;
        }

        [Header("Effects Configuration")]
        [SerializeField] private ShineEffect shine;
        [SerializeField] private PulseEffect pulse;
        [SerializeField] private FloatEffect floatEffect;
        [SerializeField] private GlowPulseEffect glowPulse;
        [SerializeField] private RotationEffect rotation;
        [SerializeField] private ColorCycleEffect colorCycle;

        private Tween shineTween;
        private Tween pulseTween;
        private Tween floatTween;
        private Tween glowTween;
        private Tween rotationTween;
        private Tween colorTween;

        private void OnEnable()
        {
            StartAllEffects();
        }

        private void OnDisable()
        {
            StopAllEffects();
        }

        private void StartAllEffects()
        {
            if (shine.enabled) StartShineEffect();
            if (pulse.enabled) StartPulseEffect();
            if (floatEffect.enabled) StartFloatEffect();
            if (glowPulse.enabled) StartGlowPulseEffect();
            if (rotation.enabled) StartRotationEffect();
            if (colorCycle.enabled) StartColorCycleEffect();
        }

        private void StopAllEffects()
        {
            shineTween?.Kill();
            pulseTween?.Kill();
            floatTween?.Kill();
            glowTween?.Kill();
            rotationTween?.Kill();
            colorTween?.Kill();
        }

        // ==================== SHINE ====================

        private void StartShineEffect()
        {
            if (shine.shineTransform == null) return;

            shine.shineTransform.anchoredPosition = new Vector2(shine.startX, shine.shineTransform.anchoredPosition.y);

            shineTween = DOTween.Sequence()
                .AppendInterval(shine.delay)
                .Append(shine.shineTransform.DOAnchorPosX(shine.endX, shine.duration).SetEase(Ease.InOutQuad))
                .AppendCallback(() => shine.shineTransform.anchoredPosition = new Vector2(shine.startX, shine.shineTransform.anchoredPosition.y))
                .SetLoops(-1);
        }

        public void TriggerShine()
        {
            if (shine.shineTransform == null) return;

            shine.shineTransform.anchoredPosition = new Vector2(shine.startX, shine.shineTransform.anchoredPosition.y);
            shine.shineTransform.DOAnchorPosX(shine.endX, shine.duration).SetEase(Ease.InOutQuad);
        }

        // ==================== PULSE ====================

        private void StartPulseEffect()
        {
            pulseTween = transform.DOScale(pulse.maxScale, pulse.duration / 2)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        // ==================== FLOAT ====================

        private void StartFloatEffect()
        {
            Vector3 startPos = transform.localPosition;
            floatTween = transform.DOLocalMoveY(startPos.y + floatEffect.distance, floatEffect.duration / 2)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        // ==================== GLOW PULSE ====================

        private void StartGlowPulseEffect()
        {
            if (glowPulse.glowImage == null) return;

            glowTween = glowPulse.glowImage.DOFade(glowPulse.maxAlpha, glowPulse.duration / 2)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        // ==================== ROTATION ====================

        private void StartRotationEffect()
        {
            if (rotation.continuous)
            {
                rotationTween = transform.DORotate(new Vector3(0, 0, 360), 360f / rotation.speed, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1);
            }
        }

        // ==================== COLOR CYCLE ====================

        private void StartColorCycleEffect()
        {
            if (colorCycle.targetImage == null || colorCycle.colors == null || colorCycle.colors.Length < 2) return;

            int currentIndex = 0;
            colorTween = DOTween.Sequence()
                .AppendCallback(() =>
                {
                    int nextIndex = (currentIndex + 1) % colorCycle.colors.Length;
                    colorCycle.targetImage.DOColor(colorCycle.colors[nextIndex], colorCycle.cycleDuration / colorCycle.colors.Length);
                    currentIndex = nextIndex;
                })
                .AppendInterval(colorCycle.cycleDuration / colorCycle.colors.Length)
                .SetLoops(-1);
        }

        // ==================== PUBLIC API ====================

        public void SetShineEnabled(bool enabled)
        {
            shine.enabled = enabled;
            if (enabled) StartShineEffect();
            else shineTween?.Kill();
        }

        public void SetPulseEnabled(bool enabled)
        {
            pulse.enabled = enabled;
            if (enabled) StartPulseEffect();
            else
            {
                pulseTween?.Kill();
                transform.localScale = Vector3.one;
            }
        }

        public void SetGlowEnabled(bool enabled)
        {
            glowPulse.enabled = enabled;
            if (enabled) StartGlowPulseEffect();
            else glowTween?.Kill();
        }

        private void OnDestroy()
        {
            StopAllEffects();
        }
    }

    /// <summary>
    /// Simple component for single effects
    /// </summary>
    public class SimplePulse : MonoBehaviour
    {
        #pragma warning disable 0414
        [SerializeField] private float minScale = 0.95f;
        #pragma warning restore 0414
        [SerializeField] private float maxScale = 1.05f;
        [SerializeField] private float duration = 1f;

        private Tween tween;

        private void OnEnable()
        {
            tween = transform.DOScale(maxScale, duration / 2)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void OnDisable()
        {
            tween?.Kill();
            transform.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// Simple float up/down effect
    /// </summary>
    public class SimpleFloat : MonoBehaviour
    {
        [SerializeField] private float distance = 10f;
        [SerializeField] private float duration = 2f;

        private Tween tween;
        private Vector3 startPos;

        private void OnEnable()
        {
            startPos = transform.localPosition;
            tween = transform.DOLocalMoveY(startPos.y + distance, duration / 2)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void OnDisable()
        {
            tween?.Kill();
            transform.localPosition = startPos;
        }
    }

    /// <summary>
    /// Simple rotation effect
    /// </summary>
    public class SimpleRotate : MonoBehaviour
    {
        [SerializeField] private float speed = 30f;
        [SerializeField] private Vector3 axis = Vector3.forward;

        private void Update()
        {
            transform.Rotate(axis * speed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Glow pulse effect for images
    /// </summary>
    public class GlowPulse : MonoBehaviour
    {
        #pragma warning disable 0414
        [SerializeField] private float minAlpha = 0.3f;
        #pragma warning restore 0414
        [SerializeField] private float maxAlpha = 0.8f;
        [SerializeField] private float duration = 1.5f;

        private Image image;
        private Tween tween;

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        private void OnEnable()
        {
            if (image == null) return;

            tween = image.DOFade(maxAlpha, duration / 2)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void OnDisable()
        {
            tween?.Kill();
        }
    }
}
