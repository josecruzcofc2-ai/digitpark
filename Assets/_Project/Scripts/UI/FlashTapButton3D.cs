using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DigitPark.Effects;

namespace DigitPark.UI
{
    /// <summary>
    /// Componente para el boton 3D de FlashTap con sprites Up/Down
    /// Maneja estados: Wait (gris), Ready (rojo), Pressed (sprite down)
    /// Mantiene su estilo propio independiente de los temas de la app
    /// </summary>
    public class FlashTapButton3D : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Sprites")]
        [SerializeField] private Sprite buttonUpSprite;
        [SerializeField] private Sprite buttonDownSprite;

        [Header("References")]
        [SerializeField] private Image buttonImage;
        [SerializeField] private Image glowRing;
        [SerializeField] private TMP_Text instructionText;
        [SerializeField] private RectTransform buttonTransform;

        [Header("Colors - Wait State (No tocar)")]
        [SerializeField] private Color waitButtonColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        [SerializeField] private Color waitGlowColor = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        [SerializeField] private Color waitTextColor = new Color(0.6f, 0.6f, 0.6f, 1f);

        [Header("Colors - Ready State (Toca ahora!)")]
        [SerializeField] private Color readyButtonColor = new Color(1f, 0.2f, 0.2f, 1f);
        [SerializeField] private Color readyGlowColor = new Color(1f, 0.2f, 0.2f, 0.6f);
        [SerializeField] private Color readyTextColor = new Color(1f, 1f, 1f, 1f);

        [Header("Colors - Error State (Muy pronto)")]
        [SerializeField] private Color errorButtonColor = new Color(1f, 0.5f, 0f, 1f);
        [SerializeField] private Color errorGlowColor = new Color(1f, 0.5f, 0f, 0.6f);
        [SerializeField] private Color errorTextColor = new Color(1f, 0.5f, 0f, 1f);

        [Header("Animation Settings")]
        [SerializeField] private float breathingSpeed = 1.5f;
        [SerializeField] private float breathingMinScale = 0.98f;
        [SerializeField] private float breathingMaxScale = 1.02f;
        [SerializeField] private float pulseSpeed = 3f;
        [SerializeField] private float pulseMinScale = 0.95f;
        [SerializeField] private float pulseMaxScale = 1.08f;
        [SerializeField] private float pressAnimDuration = 0.08f;
        [SerializeField] private float colorTransitionDuration = 0.15f;

        [Header("Glow Ring Animation")]
        [SerializeField] private float glowPulseSpeed = 2f;
        [SerializeField] private float glowMinAlpha = 0.2f;
        [SerializeField] private float glowMaxAlpha = 0.8f;
        [SerializeField] private float glowMinScale = 1f;
        [SerializeField] private float glowMaxScale = 1.15f;

        public enum ButtonState { Wait, Ready, Error, Pressed }

        private ButtonState currentState = ButtonState.Wait;
        private Vector3 originalScale;
        private Coroutine animationCoroutine;
        private Coroutine colorCoroutine;
        private Coroutine glowCoroutine;
        private bool isPressed;

        // Evento para cuando se presiona el boton
        public System.Action OnButtonPressed;

        private void Awake()
        {
            if (buttonTransform == null)
                buttonTransform = GetComponent<RectTransform>();

            originalScale = buttonTransform != null ? buttonTransform.localScale : Vector3.one;

            AutoFindReferences();
        }

        private void OnEnable()
        {
            SetState(ButtonState.Wait, true);
        }

        private void OnDisable()
        {
            StopAllAnimations();
            if (buttonTransform != null)
                buttonTransform.localScale = originalScale;
        }

        #region Pointer Events

        public void OnPointerDown(PointerEventData eventData)
        {
            if (currentState == ButtonState.Wait || currentState == ButtonState.Ready)
            {
                isPressed = true;
                PlayPressAnimation();
                OnButtonPressed?.Invoke();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isPressed)
            {
                isPressed = false;
                PlayReleaseAnimation();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Cambia el estado visual del boton
        /// </summary>
        public void SetState(ButtonState state, bool instant = false)
        {
            currentState = state;

            StopAllAnimations();

            switch (state)
            {
                case ButtonState.Wait:
                    TransitionToState(waitButtonColor, waitGlowColor, waitTextColor, instant);
                    SetSprite(buttonUpSprite);
                    StartBreathingAnimation();
                    if (instructionText != null) instructionText.text = "ESPERA...";
                    break;

                case ButtonState.Ready:
                    TransitionToState(readyButtonColor, readyGlowColor, readyTextColor, instant);
                    SetSprite(buttonUpSprite);
                    StartPulseAnimation();
                    StartGlowPulse();
                    if (instructionText != null) instructionText.text = "TAP!";
                    // Haptic feedback cuando se activa
                    TriggerReadyHaptic();
                    break;

                case ButtonState.Error:
                    TransitionToState(errorButtonColor, errorGlowColor, errorTextColor, instant);
                    SetSprite(buttonUpSprite);
                    StartShakeAnimation();
                    if (instructionText != null) instructionText.text = "MUY PRONTO!";
                    // Haptic feedback de error
                    TriggerErrorHaptic();
                    break;

                case ButtonState.Pressed:
                    SetSprite(buttonDownSprite);
                    break;
            }
        }

        /// <summary>
        /// Obtiene el estado actual del boton
        /// </summary>
        public ButtonState GetCurrentState() => currentState;

        /// <summary>
        /// Asigna los sprites del boton
        /// </summary>
        public void SetSprites(Sprite upSprite, Sprite downSprite)
        {
            buttonUpSprite = upSprite;
            buttonDownSprite = downSprite;
        }

        #endregion

        #region Animations

        private void StartBreathingAnimation()
        {
            animationCoroutine = StartCoroutine(BreathingLoop());
        }

        private IEnumerator BreathingLoop()
        {
            while (true)
            {
                if (!isPressed)
                {
                    float t = (Mathf.Sin(Time.time * breathingSpeed * Mathf.PI) + 1f) / 2f;
                    float scale = Mathf.Lerp(breathingMinScale, breathingMaxScale, t);
                    buttonTransform.localScale = originalScale * scale;
                }
                yield return null;
            }
        }

        private void StartPulseAnimation()
        {
            animationCoroutine = StartCoroutine(PulseLoop());
        }

        private IEnumerator PulseLoop()
        {
            while (true)
            {
                if (!isPressed)
                {
                    // Pulse mas agresivo para estado Ready
                    float t = (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI) + 1f) / 2f;
                    // Curva de ease para que el pulse sea mas "punchy"
                    t = EaseOutBack(t);
                    float scale = Mathf.Lerp(pulseMinScale, pulseMaxScale, t);
                    buttonTransform.localScale = originalScale * scale;
                }
                yield return null;
            }
        }

        private void StartGlowPulse()
        {
            if (glowRing != null)
            {
                glowCoroutine = StartCoroutine(GlowPulseLoop());
            }
        }

        private IEnumerator GlowPulseLoop()
        {
            RectTransform glowTransform = glowRing.GetComponent<RectTransform>();
            Vector3 glowOriginalScale = glowTransform != null ? glowTransform.localScale : Vector3.one;

            while (true)
            {
                float t = (Mathf.Sin(Time.time * glowPulseSpeed * Mathf.PI) + 1f) / 2f;

                // Pulsar alpha
                Color c = glowRing.color;
                c.a = Mathf.Lerp(glowMinAlpha, glowMaxAlpha, t);
                glowRing.color = c;

                // Pulsar escala del glow
                if (glowTransform != null)
                {
                    float scale = Mathf.Lerp(glowMinScale, glowMaxScale, t);
                    glowTransform.localScale = glowOriginalScale * scale;
                }

                yield return null;
            }
        }

        private void StartShakeAnimation()
        {
            animationCoroutine = StartCoroutine(ShakeEffect());
        }

        private IEnumerator ShakeEffect()
        {
            Vector2 originalPos = buttonTransform.anchoredPosition;
            float shakeDuration = 0.4f;
            float shakeIntensity = 15f;
            float elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;
                float decay = 1f - (elapsed / shakeDuration);
                float x = Random.Range(-1f, 1f) * shakeIntensity * decay;
                float y = Random.Range(-1f, 1f) * shakeIntensity * decay * 0.5f;

                buttonTransform.anchoredPosition = originalPos + new Vector2(x, y);
                yield return null;
            }

            buttonTransform.anchoredPosition = originalPos;
        }

        private void PlayPressAnimation()
        {
            StopCoroutine(animationCoroutine);
            StartCoroutine(PressEffect());
        }

        private IEnumerator PressEffect()
        {
            // Cambiar a sprite presionado
            SetSprite(buttonDownSprite);

            // Animar escala hacia abajo
            Vector3 targetScale = originalScale * 0.92f;
            Vector3 startScale = buttonTransform.localScale;
            float elapsed = 0f;

            while (elapsed < pressAnimDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / pressAnimDuration;
                t = EaseOutQuad(t);
                buttonTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            buttonTransform.localScale = targetScale;
        }

        private void PlayReleaseAnimation()
        {
            StartCoroutine(ReleaseEffect());
        }

        private IEnumerator ReleaseEffect()
        {
            // Volver a sprite normal
            SetSprite(buttonUpSprite);

            // Animar escala con bounce
            Vector3 targetScale = originalScale;
            Vector3 overshootScale = originalScale * 1.05f;
            Vector3 startScale = buttonTransform.localScale;
            float elapsed = 0f;
            float bounceDuration = pressAnimDuration * 1.5f;

            // Fase 1: Overshoot
            while (elapsed < bounceDuration * 0.6f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (bounceDuration * 0.6f);
                t = EaseOutQuad(t);
                buttonTransform.localScale = Vector3.Lerp(startScale, overshootScale, t);
                yield return null;
            }

            // Fase 2: Settle
            elapsed = 0f;
            startScale = buttonTransform.localScale;
            while (elapsed < bounceDuration * 0.4f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (bounceDuration * 0.4f);
                buttonTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            buttonTransform.localScale = targetScale;

            // Reanudar animacion segun estado
            if (currentState == ButtonState.Ready)
            {
                StartPulseAnimation();
            }
            else if (currentState == ButtonState.Wait)
            {
                StartBreathingAnimation();
            }
        }

        #endregion

        #region Color Transitions

        private void TransitionToState(Color buttonColor, Color glowColor, Color textColor, bool instant)
        {
            if (instant)
            {
                ApplyColors(buttonColor, glowColor, textColor);
            }
            else
            {
                colorCoroutine = StartCoroutine(ColorTransition(buttonColor, glowColor, textColor));
            }
        }

        private IEnumerator ColorTransition(Color targetButtonColor, Color targetGlowColor, Color targetTextColor)
        {
            Color startButtonColor = buttonImage != null ? buttonImage.color : targetButtonColor;
            Color startGlowColor = glowRing != null ? glowRing.color : targetGlowColor;
            Color startTextColor = instructionText != null ? instructionText.color : targetTextColor;

            float elapsed = 0f;

            while (elapsed < colorTransitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / colorTransitionDuration;
                t = EaseOutQuad(t);

                if (buttonImage != null)
                    buttonImage.color = Color.Lerp(startButtonColor, targetButtonColor, t);

                if (glowRing != null)
                    glowRing.color = Color.Lerp(startGlowColor, targetGlowColor, t);

                if (instructionText != null)
                    instructionText.color = Color.Lerp(startTextColor, targetTextColor, t);

                yield return null;
            }

            ApplyColors(targetButtonColor, targetGlowColor, targetTextColor);
        }

        private void ApplyColors(Color buttonColor, Color glowColor, Color textColor)
        {
            if (buttonImage != null) buttonImage.color = buttonColor;
            if (glowRing != null) glowRing.color = glowColor;
            if (instructionText != null) instructionText.color = textColor;
        }

        #endregion

        #region Haptics

        private void TriggerReadyHaptic()
        {
            if (FeedbackManager.Instance != null)
            {
                FeedbackManager.Instance.PlayHaptic(FeedbackManager.HapticType.Medium);
            }
        }

        private void TriggerErrorHaptic()
        {
            if (FeedbackManager.Instance != null)
            {
                FeedbackManager.Instance.PlayHaptic(FeedbackManager.HapticType.Heavy);
                FeedbackManager.Instance.PlayErrorFeedback();
            }
        }

        #endregion

        #region Helpers

        private void SetSprite(Sprite sprite)
        {
            if (buttonImage != null && sprite != null)
            {
                buttonImage.sprite = sprite;
            }
        }

        private void StopAllAnimations()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }

            if (colorCoroutine != null)
            {
                StopCoroutine(colorCoroutine);
                colorCoroutine = null;
            }

            if (glowCoroutine != null)
            {
                StopCoroutine(glowCoroutine);
                glowCoroutine = null;
            }
        }

        private void AutoFindReferences()
        {
            if (buttonImage == null)
                buttonImage = GetComponent<Image>();

            if (buttonImage == null)
                buttonImage = transform.Find("ButtonImage")?.GetComponent<Image>();

            if (glowRing == null)
                glowRing = transform.Find("GlowRing")?.GetComponent<Image>();

            if (instructionText == null)
                instructionText = transform.parent?.Find("InstructionText")?.GetComponent<TMP_Text>();
        }

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

        #endregion

#if UNITY_EDITOR
        [ContextMenu("Test Wait State")]
        private void TestWaitState() => SetState(ButtonState.Wait);

        [ContextMenu("Test Ready State")]
        private void TestReadyState() => SetState(ButtonState.Ready);

        [ContextMenu("Test Error State")]
        private void TestErrorState() => SetState(ButtonState.Error);
#endif
    }
}
