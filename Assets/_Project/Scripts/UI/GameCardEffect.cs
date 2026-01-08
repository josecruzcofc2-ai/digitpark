using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DigitPark.UI
{
    /// <summary>
    /// Componente para efectos visuales en las cards de juegos
    /// Agrega animaciones de hover, press y glow neón
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class GameCardEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("References")]
        [SerializeField] private Image borderImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private RectTransform cardTransform;

        [Header("Colors")]
        [SerializeField] private Color normalBorderColor = new Color(0f, 1f, 1f, 1f);
        [SerializeField] private Color hoverBorderColor = new Color(0f, 1f, 1f, 1f);
        [SerializeField] private Color pressedBorderColor = new Color(0f, 0.8f, 0.8f, 1f);

        [Header("Animation Settings")]
        [SerializeField] private float hoverScale = 1.05f;
        [SerializeField] private float pressedScale = 0.95f;
        [SerializeField] private float animationDuration = 0.15f;
        [SerializeField] private float glowPulseDuration = 1.5f;

        [Header("Glow Settings")]
        [SerializeField] private bool enableGlowPulse = true;
        [SerializeField] private float minGlowAlpha = 0.6f;
        [SerializeField] private float maxGlowAlpha = 1f;

        private Button button;
        private Vector3 originalScale;
        private bool isHovered;
        private bool isPressed;
        private Coroutine scaleCoroutine;
        private Coroutine colorCoroutine;
        private Coroutine glowCoroutine;

        private void Awake()
        {
            button = GetComponent<Button>();

            if (cardTransform == null)
                cardTransform = GetComponent<RectTransform>();

            originalScale = cardTransform.localScale;

            // Auto-buscar referencias si no están asignadas
            if (borderImage == null)
            {
                Transform border = transform.Find("Border");
                if (border != null)
                    borderImage = border.GetComponent<Image>();
            }

            if (backgroundImage == null)
            {
                Transform innerBg = transform.Find("InnerBackground");
                if (innerBg != null)
                    backgroundImage = innerBg.GetComponent<Image>();
            }
        }

        private void OnEnable()
        {
            if (enableGlowPulse && borderImage != null)
            {
                StartGlowPulse();
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            ResetToOriginal();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!button.interactable) return;

            isHovered = true;
            AnimateHover();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;

            if (!isPressed)
            {
                AnimateNormal();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!button.interactable) return;

            isPressed = true;
            AnimatePress();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPressed = false;

            if (isHovered)
            {
                AnimateHover();
            }
            else
            {
                AnimateNormal();
            }
        }

        private void AnimateHover()
        {
            StopScaleCoroutine();
            scaleCoroutine = StartCoroutine(AnimateScale(originalScale * hoverScale, animationDuration, true));

            if (borderImage != null)
            {
                StopColorCoroutine();
                colorCoroutine = StartCoroutine(AnimateColor(hoverBorderColor, animationDuration));
            }
        }

        private void AnimatePress()
        {
            StopScaleCoroutine();
            scaleCoroutine = StartCoroutine(AnimateScale(originalScale * pressedScale, animationDuration * 0.5f, false));

            if (borderImage != null)
            {
                StopColorCoroutine();
                colorCoroutine = StartCoroutine(AnimateColor(pressedBorderColor, animationDuration * 0.5f));
            }
        }

        private void AnimateNormal()
        {
            StopScaleCoroutine();
            scaleCoroutine = StartCoroutine(AnimateScale(originalScale, animationDuration, false));

            if (borderImage != null)
            {
                StopColorCoroutine();
                colorCoroutine = StartCoroutine(AnimateColor(normalBorderColor, animationDuration));
            }
        }

        private IEnumerator AnimateScale(Vector3 targetScale, float duration, bool overshoot)
        {
            Vector3 startScale = cardTransform.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Ease out con overshoot opcional
                if (overshoot)
                {
                    t = EaseOutBack(t);
                }
                else
                {
                    t = EaseOutQuad(t);
                }

                cardTransform.localScale = Vector3.LerpUnclamped(startScale, targetScale, t);
                yield return null;
            }

            cardTransform.localScale = targetScale;
        }

        private IEnumerator AnimateColor(Color targetColor, float duration)
        {
            Color startColor = borderImage.color;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = EaseOutQuad(t);

                borderImage.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }

            borderImage.color = targetColor;
        }

        private void StartGlowPulse()
        {
            if (borderImage == null) return;

            StopGlowCoroutine();
            glowCoroutine = StartCoroutine(GlowPulseLoop());
        }

        private IEnumerator GlowPulseLoop()
        {
            while (true)
            {
                // Fade out
                yield return StartCoroutine(AnimateAlpha(minGlowAlpha, glowPulseDuration));
                // Fade in
                yield return StartCoroutine(AnimateAlpha(maxGlowAlpha, glowPulseDuration));
            }
        }

        private IEnumerator AnimateAlpha(float targetAlpha, float duration)
        {
            float startAlpha = borderImage.color.a;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = EaseInOutSine(t);

                Color c = borderImage.color;
                c.a = Mathf.Lerp(startAlpha, targetAlpha, t);
                borderImage.color = c;

                yield return null;
            }
        }

        // Easing functions
        private float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }

        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        private float EaseInOutSine(float t)
        {
            return -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;
        }

        private void StopScaleCoroutine()
        {
            if (scaleCoroutine != null)
            {
                StopCoroutine(scaleCoroutine);
                scaleCoroutine = null;
            }
        }

        private void StopColorCoroutine()
        {
            if (colorCoroutine != null)
            {
                StopCoroutine(colorCoroutine);
                colorCoroutine = null;
            }
        }

        private void StopGlowCoroutine()
        {
            if (glowCoroutine != null)
            {
                StopCoroutine(glowCoroutine);
                glowCoroutine = null;
            }
        }

        private void ResetToOriginal()
        {
            if (cardTransform != null)
                cardTransform.localScale = originalScale;

            if (borderImage != null)
                borderImage.color = normalBorderColor;
        }

        /// <summary>
        /// Configura los colores del efecto (útil para cards doradas)
        /// </summary>
        public void SetColors(Color normal, Color hover, Color pressed)
        {
            normalBorderColor = normal;
            hoverBorderColor = hover;
            pressedBorderColor = pressed;

            if (borderImage != null)
                borderImage.color = normalBorderColor;
        }

        /// <summary>
        /// Activa/desactiva el efecto de pulse
        /// </summary>
        public void SetGlowPulse(bool enabled)
        {
            enableGlowPulse = enabled;

            if (enabled && isActiveAndEnabled)
            {
                StartGlowPulse();
            }
            else
            {
                StopGlowCoroutine();
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Auto-configura las referencias en el editor
        /// </summary>
        [ContextMenu("Auto Setup References")]
        private void AutoSetupReferences()
        {
            borderImage = transform.Find("Border")?.GetComponent<Image>();
            backgroundImage = transform.Find("InnerBackground")?.GetComponent<Image>();
            cardTransform = GetComponent<RectTransform>();

            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
