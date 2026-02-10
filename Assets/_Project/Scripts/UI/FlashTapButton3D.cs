using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DigitPark.Effects;
using DigitPark.Localization;

namespace DigitPark.UI
{
    /// <summary>
    /// Componente para el boton 3D de FlashTap con sprites por estado
    /// Wait (naranja), Ready (rojo), Error (rojo+shake), Success (verde)
    /// Cada estado tiene su propio sprite Up/Down pre-coloreado
    /// </summary>
    public class FlashTapButton3D : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Sprites - Wait State (Orange)")]
        [SerializeField] private Sprite waitUpSprite;
        [SerializeField] private Sprite waitDownSprite;

        [Header("Sprites - Ready State (Red)")]
        [SerializeField] private Sprite readyUpSprite;
        [SerializeField] private Sprite readyDownSprite;

        [Header("Sprites - Success State (Green)")]
        [SerializeField] private Sprite successUpSprite;

        [Header("References")]
        [SerializeField] private Image buttonImage;
        [SerializeField] private Image glowRing;
        [SerializeField] private TMP_Text instructionText;
        [SerializeField] private RectTransform buttonTransform;

        [Header("Glow Colors")]
        [SerializeField] private Color waitGlowColor = new Color(1f, 0.6f, 0f, 0.3f);
        [SerializeField] private Color readyGlowColor = new Color(1f, 0.2f, 0.2f, 0.6f);
        [SerializeField] private Color successGlowColor = new Color(0.2f, 1f, 0.4f, 0.6f);
        [SerializeField] private Color errorGlowColor = new Color(1f, 0.2f, 0.2f, 0.6f);

        [Header("Text Colors")]
        [SerializeField] private Color waitTextColor = new Color(1f, 0.7f, 0.3f, 1f);
        [SerializeField] private Color readyTextColor = new Color(1f, 1f, 1f, 1f);
        [SerializeField] private Color successTextColor = new Color(0.2f, 1f, 0.4f, 1f);
        [SerializeField] private Color errorTextColor = new Color(1f, 0.3f, 0.3f, 1f);

        [Header("Animation Settings")]
        [SerializeField] private float breathingSpeed = 1.5f;
        [SerializeField] private float breathingMinScale = 0.98f;
        [SerializeField] private float breathingMaxScale = 1.02f;
        [SerializeField] private float pulseSpeed = 3f;
        [SerializeField] private float pulseMinScale = 1.0f;
        [SerializeField] private float pulseMaxScale = 1.08f;
        [SerializeField] private float pressAnimDuration = 0.12f;

        [Header("Glow Ring Animation")]
        [SerializeField] private float glowPulseSpeed = 2f;
        [SerializeField] private float glowMinAlpha = 0.2f;
        [SerializeField] private float glowMaxAlpha = 0.8f;
        [SerializeField] private float glowMinScale = 1f;
        [SerializeField] private float glowMaxScale = 1.15f;

        public enum ButtonState { Wait, Ready, Error, Pressed, Success }

        private ButtonState currentState = ButtonState.Wait;
        private Vector3 originalScale;
        private Coroutine animationCoroutine;
        private Coroutine glowCoroutine;
        private bool isPressed;
        private bool inputEnabled = true;

        public System.Action OnButtonPressed;

        private void Awake()
        {
            if (buttonTransform == null)
                buttonTransform = GetComponent<RectTransform>();

            originalScale = buttonTransform != null ? buttonTransform.localScale : Vector3.one;

            AutoFindReferences();
            EnsureRaycastTarget();
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
            if (!inputEnabled) return;
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

        public void SetState(ButtonState state, bool instant = false)
        {
            currentState = state;
            StopAllAnimations();

            switch (state)
            {
                case ButtonState.Wait:
                    SetSpriteUp(waitUpSprite);
                    ApplyGlowAndText(waitGlowColor, waitTextColor, instant);
                    StartBreathingAnimation();
                    if (instructionText != null) instructionText.text = AutoLocalizer.Get("flashtap_wait");
                    break;

                case ButtonState.Ready:
                    SetSpriteUp(readyUpSprite);
                    ApplyGlowAndText(readyGlowColor, readyTextColor, instant);
                    StartPulseAnimation();
                    StartGlowPulse();
                    if (instructionText != null) instructionText.text = AutoLocalizer.Get("flashtap_tap");
                    TriggerReadyHaptic();
                    break;

                case ButtonState.Error:
                    SetSpriteUp(readyUpSprite); // Red sprite for error
                    ApplyGlowAndText(errorGlowColor, errorTextColor, instant);
                    StartShakeAnimation();
                    if (instructionText != null) instructionText.text = AutoLocalizer.Get("flashtap_too_early");
                    TriggerErrorHaptic();
                    break;

                case ButtonState.Success:
                    SetSpriteUp(successUpSprite);
                    ApplyGlowAndText(successGlowColor, successTextColor, instant);
                    if (instructionText != null) instructionText.text = AutoLocalizer.Get("flashtap_correct");
                    TriggerSuccessHaptic();
                    break;

                case ButtonState.Pressed:
                    // Handled by press/release animations
                    break;
            }
        }

        public ButtonState GetCurrentState() => currentState;

        public void SetInputEnabled(bool enabled)
        {
            inputEnabled = enabled;
        }

        /// <summary>
        /// Legacy compatibility - sets wait sprites only
        /// </summary>
        public void SetSprites(Sprite upSprite, Sprite downSprite)
        {
            waitUpSprite = upSprite;
            waitDownSprite = downSprite;
        }

        /// <summary>
        /// Sets all state sprites at once
        /// </summary>
        public void SetAllStateSprites(Sprite wUp, Sprite wDown, Sprite rUp, Sprite rDown, Sprite sUp)
        {
            waitUpSprite = wUp;
            waitDownSprite = wDown;
            readyUpSprite = rUp;
            readyDownSprite = rDown;
            successUpSprite = sUp;
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
            // Never shrink below original size - pulse only grows
            float effectiveMin = Mathf.Max(pulseMinScale, 1f);
            while (true)
            {
                if (!isPressed)
                {
                    float t = (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI) + 1f) / 2f;
                    t = EaseOutBack(t);
                    float scale = Mathf.Lerp(effectiveMin, pulseMaxScale, t);
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

                Color c = glowRing.color;
                c.a = Mathf.Lerp(glowMinAlpha, glowMaxAlpha, t);
                glowRing.color = c;

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
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }
            StartCoroutine(PressEffect());
        }

        private IEnumerator PressEffect()
        {
            // Use the down sprite for current state
            Sprite downSprite = GetCurrentDownSprite();
            if (downSprite != null && buttonImage != null)
                buttonImage.sprite = downSprite;

            Vector3 targetScale = originalScale * 0.85f;
            Vector3 startScale = buttonTransform.localScale;
            float elapsed = 0f;

            while (elapsed < pressAnimDuration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutQuad(elapsed / pressAnimDuration);
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
            // Restore up sprite
            Sprite upSprite = GetCurrentUpSprite();
            if (upSprite != null && buttonImage != null)
                buttonImage.sprite = upSprite;

            Vector3 targetScale = originalScale;
            Vector3 overshootScale = originalScale * 1.05f;
            Vector3 startScale = buttonTransform.localScale;
            float elapsed = 0f;
            float bounceDuration = pressAnimDuration * 1.5f;

            // Phase 1: Overshoot
            while (elapsed < bounceDuration * 0.6f)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutQuad(elapsed / (bounceDuration * 0.6f));
                buttonTransform.localScale = Vector3.Lerp(startScale, overshootScale, t);
                yield return null;
            }

            // Phase 2: Settle
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

            if (currentState == ButtonState.Ready)
                StartPulseAnimation();
            else if (currentState == ButtonState.Wait)
                StartBreathingAnimation();
        }

        #endregion

        #region Haptics

        private void TriggerReadyHaptic()
        {
            if (FeedbackManager.Instance != null)
                FeedbackManager.Instance.PlayHaptic(FeedbackManager.HapticType.Medium);
        }

        private void TriggerErrorHaptic()
        {
            if (FeedbackManager.Instance != null)
            {
                FeedbackManager.Instance.PlayHaptic(FeedbackManager.HapticType.Heavy);
                FeedbackManager.Instance.PlayErrorFeedback();
            }
        }

        private void TriggerSuccessHaptic()
        {
            if (FeedbackManager.Instance != null)
                FeedbackManager.Instance.PlayHaptic(FeedbackManager.HapticType.Light);
        }

        #endregion

        #region Helpers

        private Sprite GetCurrentUpSprite()
        {
            switch (currentState)
            {
                case ButtonState.Wait: return waitUpSprite;
                case ButtonState.Ready: return readyUpSprite;
                case ButtonState.Error: return readyUpSprite;
                case ButtonState.Success: return successUpSprite;
                default: return waitUpSprite;
            }
        }

        private Sprite GetCurrentDownSprite()
        {
            switch (currentState)
            {
                case ButtonState.Wait: return waitDownSprite;
                case ButtonState.Ready: return readyDownSprite;
                case ButtonState.Error: return readyDownSprite;
                default: return waitDownSprite;
            }
        }

        private void SetSpriteUp(Sprite sprite)
        {
            if (buttonImage != null && sprite != null)
            {
                buttonImage.sprite = sprite;
                buttonImage.color = Color.white; // No tinting - sprites are pre-colored
            }
        }

        private void ApplyGlowAndText(Color glowColor, Color textColor, bool instant)
        {
            if (glowRing != null) glowRing.color = glowColor;
            if (instructionText != null) instructionText.color = textColor;
        }

        private void StopAllAnimations()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
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
                buttonImage = transform.Find("ButtonImage")?.GetComponent<Image>();

            if (buttonImage == null)
                buttonImage = GetComponentInChildren<Image>();

            if (glowRing == null)
                glowRing = transform.Find("GlowRing")?.GetComponent<Image>();

            if (instructionText == null)
                instructionText = transform.parent?.Find("InstructionText")?.GetComponent<TMP_Text>();
        }

        /// <summary>
        /// Asegura que este GameObject tenga un Image transparente para recibir pointer events
        /// y desactiva el raycast del hijo ButtonImage para que no intercepte
        /// </summary>
        private void EnsureRaycastTarget()
        {
            // Add transparent Image if we don't have a Graphic for IPointerDownHandler
            if (GetComponent<Graphic>() == null)
            {
                var img = gameObject.AddComponent<Image>();
                img.color = new Color(0, 0, 0, 0);
                img.raycastTarget = true;
            }

            // Disable child buttonImage's raycast so pointer events reach us
            if (buttonImage != null)
                buttonImage.raycastTarget = false;
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

        [ContextMenu("Test Success State")]
        private void TestSuccessState() => SetState(ButtonState.Success);
#endif
    }
}
