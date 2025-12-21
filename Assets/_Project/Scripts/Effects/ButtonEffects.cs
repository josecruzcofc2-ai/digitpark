using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

namespace DigitPark.Effects
{
    /// <summary>
    /// Componente que agrega efectos satisfactorios a cualquier boton
    /// Incluye: animacion de escala, flash de color, particulas, haptic
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ButtonEffects : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Animation Settings")]
        [SerializeField] private bool enableScaleAnimation = true;
        [SerializeField] private float pressedScale = 0.92f;
        [SerializeField] private float hoverScale = 1.05f;
        [SerializeField] private float animationDuration = 0.1f;
        [SerializeField] private AnimationCurve bounceCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Color Flash")]
        [SerializeField] private bool enableColorFlash = true;
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField] private float flashIntensity = 0.3f;

        [Header("Particles")]
        [SerializeField] private bool enableParticles = true;
        [SerializeField] private ParticleType particleType = ParticleType.NeonBurst;

        [Header("Haptic")]
        [SerializeField] private bool enableHaptic = true;
        [SerializeField] private FeedbackManager.HapticType hapticType = FeedbackManager.HapticType.Light;

        [Header("Glow Effect")]
        [SerializeField] private bool enableGlow = true;
        [SerializeField] private float glowPulseSpeed = 2f;

        [Header("Button Type")]
        [SerializeField] private ButtonEffectType effectType = ButtonEffectType.Normal;

        private Button button;
        private RectTransform rectTransform;
        private Image buttonImage;
        private Graphic targetGraphic;
        private Vector3 originalScale;
        private Color originalColor;
        private Coroutine scaleCoroutine;
        private Coroutine colorCoroutine;
        private Coroutine glowCoroutine;
        private bool isPointerOver;
        private bool isPointerDown;

        // Para el efecto de glow
        private Outline outline;
        private Shadow shadow;

        public enum ButtonEffectType
        {
            Normal,      // Botones regulares
            Important,   // Botones de accion principal (Play, Buy, etc)
            Success,     // Botones de confirmacion
            Danger,      // Botones de eliminar/cancelar
            Premium      // Botones PRO/Premium
        }

        private void Awake()
        {
            button = GetComponent<Button>();
            rectTransform = GetComponent<RectTransform>();
            buttonImage = GetComponent<Image>();
            targetGraphic = button.targetGraphic;

            originalScale = rectTransform.localScale;

            if (targetGraphic != null)
            {
                originalColor = targetGraphic.color;
            }

            // Configurar segun el tipo de boton
            ConfigureByType();

            // Agregar listener para el click
            button.onClick.AddListener(OnButtonClick);
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClick);
            }
        }

        private void ConfigureByType()
        {
            switch (effectType)
            {
                case ButtonEffectType.Important:
                    particleType = ParticleType.NeonBurstLarge;
                    hapticType = FeedbackManager.HapticType.Medium;
                    flashIntensity = 0.4f;
                    break;

                case ButtonEffectType.Success:
                    particleType = ParticleType.SuccessBurst;
                    hapticType = FeedbackManager.HapticType.Medium;
                    flashColor = new Color(0.2f, 1f, 0.4f, 1f);
                    break;

                case ButtonEffectType.Danger:
                    particleType = ParticleType.ErrorBurst;
                    hapticType = FeedbackManager.HapticType.Heavy;
                    flashColor = new Color(1f, 0.3f, 0.3f, 1f);
                    break;

                case ButtonEffectType.Premium:
                    particleType = ParticleType.ComboBurst;
                    hapticType = FeedbackManager.HapticType.Medium;
                    flashColor = new Color(1f, 0.84f, 0f, 1f);
                    enableGlow = true;
                    break;
            }
        }

        #region Pointer Events

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!button.interactable) return;

            isPointerDown = true;

            // Animacion de escala - presionado
            if (enableScaleAnimation)
            {
                AnimateScale(pressedScale);
            }

            // Flash de color
            if (enableColorFlash)
            {
                FlashColor();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPointerDown = false;

            if (!button.interactable) return;

            // Bounce back
            if (enableScaleAnimation)
            {
                float targetScale = isPointerOver ? hoverScale : 1f;
                AnimateScale(targetScale, true);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!button.interactable) return;

            isPointerOver = true;

            // Hover scale
            if (enableScaleAnimation && !isPointerDown)
            {
                AnimateScale(hoverScale);
            }

            // Iniciar glow pulsante
            if (enableGlow)
            {
                StartGlowPulse();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerOver = false;

            if (!button.interactable) return;

            // Volver a escala normal
            if (enableScaleAnimation && !isPointerDown)
            {
                AnimateScale(1f);
            }

            // Detener glow
            if (enableGlow)
            {
                StopGlowPulse();
            }
        }

        #endregion

        #region Button Click

        private void OnButtonClick()
        {
            // Particulas
            if (enableParticles && FeedbackManager.Instance != null)
            {
                Vector3 worldPos = rectTransform.position;
                FeedbackManager.Instance.PlayButtonFeedback(worldPos);
            }

            // Haptic
            if (enableHaptic && FeedbackManager.Instance != null)
            {
                FeedbackManager.Instance.PlayHaptic(hapticType);
            }

            // Efecto adicional segun tipo
            PlayTypeSpecificEffect();
        }

        private void PlayTypeSpecificEffect()
        {
            switch (effectType)
            {
                case ButtonEffectType.Success:
                    if (FeedbackManager.Instance != null)
                    {
                        FeedbackManager.Instance.PlaySuccessFeedback(rectTransform.position);
                    }
                    break;

                case ButtonEffectType.Premium:
                    if (FeedbackManager.Instance != null)
                    {
                        FeedbackManager.Instance.PlayImportantFeedback(rectTransform.position);
                    }
                    break;
            }
        }

        #endregion

        #region Scale Animation

        private void AnimateScale(float targetScale, bool bounce = false)
        {
            if (scaleCoroutine != null)
            {
                StopCoroutine(scaleCoroutine);
            }

            scaleCoroutine = StartCoroutine(ScaleAnimation(targetScale, bounce));
        }

        private IEnumerator ScaleAnimation(float targetScale, bool bounce)
        {
            Vector3 startScale = rectTransform.localScale;
            Vector3 endScale = originalScale * targetScale;

            float elapsed = 0f;
            float duration = animationDuration;

            if (bounce)
            {
                // Primero overshoot, luego volver
                Vector3 overshootScale = originalScale * (targetScale + 0.1f);

                // Fase 1: Overshoot
                while (elapsed < duration * 0.5f)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / (duration * 0.5f);
                    t = bounceCurve.Evaluate(t);
                    rectTransform.localScale = Vector3.Lerp(startScale, overshootScale, t);
                    yield return null;
                }

                // Fase 2: Settle
                elapsed = 0f;
                startScale = rectTransform.localScale;

                while (elapsed < duration * 0.5f)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / (duration * 0.5f);
                    rectTransform.localScale = Vector3.Lerp(startScale, endScale, t);
                    yield return null;
                }
            }
            else
            {
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    t = bounceCurve.Evaluate(t);
                    rectTransform.localScale = Vector3.Lerp(startScale, endScale, t);
                    yield return null;
                }
            }

            rectTransform.localScale = endScale;
            scaleCoroutine = null;
        }

        #endregion

        #region Color Flash

        private void FlashColor()
        {
            if (targetGraphic == null) return;

            if (colorCoroutine != null)
            {
                StopCoroutine(colorCoroutine);
            }

            colorCoroutine = StartCoroutine(ColorFlashAnimation());
        }

        private IEnumerator ColorFlashAnimation()
        {
            if (targetGraphic == null) yield break;

            // Flash hacia el color de flash
            Color startColor = targetGraphic.color;
            Color peakColor = Color.Lerp(startColor, flashColor, flashIntensity);

            float flashDuration = 0.05f;
            float returnDuration = 0.15f;

            // Flash in
            float elapsed = 0f;
            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                targetGraphic.color = Color.Lerp(startColor, peakColor, elapsed / flashDuration);
                yield return null;
            }

            // Flash out
            elapsed = 0f;
            while (elapsed < returnDuration)
            {
                elapsed += Time.deltaTime;
                targetGraphic.color = Color.Lerp(peakColor, originalColor, elapsed / returnDuration);
                yield return null;
            }

            targetGraphic.color = originalColor;
            colorCoroutine = null;
        }

        #endregion

        #region Glow Pulse

        private void StartGlowPulse()
        {
            if (glowCoroutine != null)
            {
                StopCoroutine(glowCoroutine);
            }

            // Obtener o crear componente de outline
            outline = GetComponent<Outline>();

            glowCoroutine = StartCoroutine(GlowPulseAnimation());
        }

        private void StopGlowPulse()
        {
            if (glowCoroutine != null)
            {
                StopCoroutine(glowCoroutine);
                glowCoroutine = null;
            }

            // Restaurar outline si existe
            if (outline != null)
            {
                var outlineColor = outline.effectColor;
                outlineColor.a = 1f;
                outline.effectColor = outlineColor;
            }
        }

        private IEnumerator GlowPulseAnimation()
        {
            if (outline == null) yield break;

            Color baseColor = outline.effectColor;

            while (true)
            {
                float pulse = (Mathf.Sin(Time.time * glowPulseSpeed) + 1f) / 2f;
                pulse = Mathf.Lerp(0.5f, 1f, pulse);

                var glowColor = baseColor;
                glowColor.a = pulse;
                outline.effectColor = glowColor;

                // Tambien pulsar el tamaño del outline
                outline.effectDistance = Vector2.one * Mathf.Lerp(2f, 4f, pulse);

                yield return null;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Fuerza la reproduccion de efectos (util para llamar desde codigo)
        /// </summary>
        public void PlayEffects()
        {
            OnButtonClick();
        }

        /// <summary>
        /// Activa un efecto de exito especial
        /// </summary>
        public void PlaySuccessEffect()
        {
            if (FeedbackManager.Instance != null)
            {
                FeedbackManager.Instance.PlaySuccessFeedback(rectTransform.position);
            }

            StartCoroutine(SuccessAnimation());
        }

        private IEnumerator SuccessAnimation()
        {
            // Triple bounce
            for (int i = 0; i < 3; i++)
            {
                AnimateScale(1.1f - i * 0.03f, true);
                yield return new WaitForSeconds(0.15f);
            }
        }

        /// <summary>
        /// Activa un efecto de error
        /// </summary>
        public void PlayErrorEffect()
        {
            if (FeedbackManager.Instance != null)
            {
                FeedbackManager.Instance.PlayErrorFeedback();
            }

            StartCoroutine(ShakeAnimation());
        }

        private IEnumerator ShakeAnimation()
        {
            Vector3 originalPos = rectTransform.anchoredPosition;
            float shakeMagnitude = 10f;
            float shakeDuration = 0.3f;
            float elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                float x = Random.Range(-1f, 1f) * shakeMagnitude * (1f - elapsed / shakeDuration);
                rectTransform.anchoredPosition = originalPos + new Vector3(x, 0f, 0f);

                elapsed += Time.deltaTime;
                yield return null;
            }

            rectTransform.anchoredPosition = originalPos;
        }

        #endregion
    }
}
