using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

namespace DigitPark.UI
{
    /// <summary>
    /// Componente para dar efecto 3D a los botones de respuesta de QuickMath
    /// Con animaciones de correcto, incorrecto, hover, breathing y celebración
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class QuickMathCell3D : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("3D Effect Settings")]
        [SerializeField] private float pressDepth = 10f;
        [SerializeField] private float pressDuration = 0.05f;
        [SerializeField] private float releaseDuration = 0.12f;

        [Header("Visual References")]
        [SerializeField] private RectTransform buttonFace;
        [SerializeField] private Image shadowImage;
        [SerializeField] private Image sideImage;
        [SerializeField] private Image faceImage;
        [SerializeField] private Outline glowOutline;
        [SerializeField] private TextMeshProUGUI answerText;

        [Header("Colors")]
        [SerializeField] private Color normalFaceColor = new Color(0.08f, 0.12f, 0.18f, 1f);
        [SerializeField] private Color normalGlowColor = new Color(0f, 1f, 1f, 1f);
        [SerializeField] private Color correctFaceColor = new Color(0.1f, 0.35f, 0.2f, 1f);
        [SerializeField] private Color correctGlowColor = new Color(0.3f, 1f, 0.5f, 1f);
        [SerializeField] private Color errorFaceColor = new Color(0.35f, 0.1f, 0.1f, 1f);
        [SerializeField] private Color errorGlowColor = new Color(1f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color comboFaceColor = new Color(0.3f, 0.25f, 0.1f, 1f);
        [SerializeField] private Color comboGlowColor = new Color(1f, 0.85f, 0.2f, 1f);

        [Header("Hover Settings")]
        [SerializeField] private Color hoverGlowColor = new Color(1f, 1f, 1f, 1f);
        [SerializeField] private float hoverLiftAmount = 5f;
        [SerializeField] private float hoverScaleMultiplier = 1.08f;

        [Header("Breathing Animation")]
        [SerializeField] private bool enableBreathing = true;
        [SerializeField] private float breathingSpeed = 2.5f;
        [SerializeField] private float breathingScaleMin = 0.97f;
        [SerializeField] private float breathingScaleMax = 1.03f;
        [SerializeField] private float breathingGlowMin = 0.7f;
        [SerializeField] private float breathingGlowMax = 1f;

        private Button button;
        private Vector2 originalPosition;
        private Vector3 originalScale;
        private Coroutine currentAnimation;
        private Coroutine breathingCoroutine;
        private bool isPressed = false;
        private bool isHovering = false;
        private bool isAnimatingResult = false;

        private void Awake()
        {
            button = GetComponent<Button>();

            if (buttonFace != null)
            {
                originalPosition = buttonFace.anchoredPosition;
                originalScale = buttonFace.localScale;
                if (faceImage == null)
                    faceImage = buttonFace.GetComponent<Image>();
                if (answerText == null)
                    answerText = buttonFace.GetComponentInChildren<TextMeshProUGUI>();
                if (glowOutline == null)
                    glowOutline = buttonFace.GetComponent<Outline>();
            }
        }

        private void OnEnable()
        {
            if (enableBreathing && !isAnimatingResult)
            {
                StartBreathing();
            }
        }

        private void OnDisable()
        {
            StopBreathing();
        }

        #region Pointer Events

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!button.interactable) return;
            PressButton();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!button.interactable) return;
            ReleaseButton();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!button.interactable || isAnimatingResult) return;

            isHovering = true;
            StopBreathing();

            if (currentAnimation != null)
                StopCoroutine(currentAnimation);

            currentAnimation = StartCoroutine(AnimateHoverEnter());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!button.interactable || isAnimatingResult) return;

            isHovering = false;

            if (currentAnimation != null)
                StopCoroutine(currentAnimation);

            currentAnimation = StartCoroutine(AnimateHoverExit());
        }

        #endregion

        private void PressButton()
        {
            if (isPressed) return;
            isPressed = true;

            if (currentAnimation != null)
                StopCoroutine(currentAnimation);

            currentAnimation = StartCoroutine(AnimatePress());
        }

        private void ReleaseButton()
        {
            if (!isPressed) return;
            isPressed = false;

            if (currentAnimation != null)
                StopCoroutine(currentAnimation);

            currentAnimation = StartCoroutine(AnimateRelease());
        }

        /// <summary>
        /// Animación de respuesta correcta
        /// </summary>
        public void AnimateCorrect(int streakLevel = 1)
        {
            if (currentAnimation != null)
                StopCoroutine(currentAnimation);

            currentAnimation = StartCoroutine(AnimateCorrectSequence(streakLevel));
        }

        /// <summary>
        /// Animación de respuesta incorrecta
        /// </summary>
        public void AnimateError()
        {
            if (currentAnimation != null)
                StopCoroutine(currentAnimation);

            currentAnimation = StartCoroutine(AnimateErrorSequence());
        }

        /// <summary>
        /// Celebración de victoria
        /// </summary>
        public void PlayVictoryCelebration(float delay)
        {
            StartCoroutine(AnimateVictoryCelebration(delay));
        }

        /// <summary>
        /// Resetea el botón a estado normal
        /// </summary>
        public void ResetToNormal()
        {
            if (currentAnimation != null)
                StopCoroutine(currentAnimation);

            StopBreathing();

            isPressed = false;
            isHovering = false;
            isAnimatingResult = false;

            if (buttonFace != null)
            {
                buttonFace.anchoredPosition = originalPosition;
                buttonFace.localScale = originalScale;
            }

            if (faceImage != null)
                faceImage.color = normalFaceColor;

            if (glowOutline != null)
                glowOutline.effectColor = normalGlowColor;

            if (answerText != null)
                answerText.color = normalGlowColor;

            if (sideImage != null)
            {
                RectTransform sideRT = sideImage.GetComponent<RectTransform>();
                sideRT.sizeDelta = new Vector2(sideRT.sizeDelta.x, pressDepth);
            }

            // Reiniciar breathing
            if (enableBreathing)
            {
                StartBreathing();
            }
        }

        /// <summary>
        /// Establece el texto de la respuesta
        /// </summary>
        public void SetAnswer(string answer)
        {
            if (answerText != null)
                answerText.text = answer;
        }

        private IEnumerator AnimatePress()
        {
            if (buttonFace == null) yield break;

            Vector2 startPos = buttonFace.anchoredPosition;
            Vector2 targetPos = originalPosition + new Vector2(0, -pressDepth);

            float elapsed = 0f;

            while (elapsed < pressDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / pressDuration;
                t = 1f - Mathf.Pow(1f - t, 3f);

                buttonFace.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);

                if (sideImage != null)
                {
                    RectTransform sideRT = sideImage.GetComponent<RectTransform>();
                    float sideHeight = Mathf.Lerp(pressDepth, 2f, t);
                    sideRT.sizeDelta = new Vector2(sideRT.sizeDelta.x, sideHeight);
                }

                yield return null;
            }

            buttonFace.anchoredPosition = targetPos;
        }

        private IEnumerator AnimateRelease()
        {
            if (buttonFace == null) yield break;

            Vector2 startPos = buttonFace.anchoredPosition;
            Vector2 targetPos = originalPosition;

            float elapsed = 0f;

            while (elapsed < releaseDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / releaseDuration;
                t = 1f - Mathf.Pow(1f - t, 2f);

                buttonFace.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);

                if (sideImage != null)
                {
                    RectTransform sideRT = sideImage.GetComponent<RectTransform>();
                    float sideHeight = Mathf.Lerp(2f, pressDepth, t);
                    sideRT.sizeDelta = new Vector2(sideRT.sizeDelta.x, sideHeight);
                }

                yield return null;
            }

            buttonFace.anchoredPosition = targetPos;
        }

        private IEnumerator AnimateCorrectSequence(int streakLevel)
        {
            if (buttonFace == null) yield break;

            isAnimatingResult = true;
            StopBreathing();

            // Calcular colores basados en streak
            float comboBlend = Mathf.Clamp01((streakLevel - 1) * 0.15f);
            Color targetFaceColor = Color.Lerp(correctFaceColor, comboFaceColor, comboBlend);
            Color targetGlowColor = Color.Lerp(correctGlowColor, comboGlowColor, comboBlend);

            // Flash blanco
            if (faceImage != null) faceImage.color = Color.white;
            if (glowOutline != null) glowOutline.effectColor = Color.white;
            if (answerText != null) answerText.color = Color.white;

            yield return new WaitForSeconds(0.05f);

            // Pulso de escala con bounce
            float duration = 0.35f;
            float elapsed = 0f;
            Vector3 originalScale = buttonFace.localScale;
            float pulseScale = 1f + streakLevel * 0.04f;
            pulseScale = Mathf.Min(pulseScale, 1.25f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Pulso con bounce
                float scale = 1f + Mathf.Sin(t * Mathf.PI) * (pulseScale - 1f);
                buttonFace.localScale = originalScale * scale;

                // Transición de color
                if (faceImage != null)
                    faceImage.color = Color.Lerp(Color.white, targetFaceColor, t);

                if (glowOutline != null)
                    glowOutline.effectColor = Color.Lerp(Color.white, targetGlowColor, t);

                if (answerText != null)
                    answerText.color = Color.Lerp(Color.white, targetGlowColor, t);

                yield return null;
            }

            buttonFace.localScale = originalScale;

            // Mantener colores de éxito brevemente
            yield return new WaitForSeconds(0.15f);

            // Volver a normal
            float resetDuration = 0.2f;
            elapsed = 0f;

            while (elapsed < resetDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / resetDuration;

                if (faceImage != null)
                    faceImage.color = Color.Lerp(targetFaceColor, normalFaceColor, t);

                if (glowOutline != null)
                    glowOutline.effectColor = Color.Lerp(targetGlowColor, normalGlowColor, t);

                if (answerText != null)
                    answerText.color = Color.Lerp(targetGlowColor, normalGlowColor, t);

                yield return null;
            }

            if (faceImage != null) faceImage.color = normalFaceColor;
            if (glowOutline != null) glowOutline.effectColor = normalGlowColor;
            if (answerText != null) answerText.color = normalGlowColor;
        }

        private IEnumerator AnimateErrorSequence()
        {
            if (buttonFace == null) yield break;

            isAnimatingResult = true;
            StopBreathing();

            // Flash rojo
            if (faceImage != null) faceImage.color = errorFaceColor;
            if (glowOutline != null) glowOutline.effectColor = errorGlowColor;
            if (answerText != null) answerText.color = errorGlowColor;

            // Shake horizontal
            Vector2 startPos = buttonFace.anchoredPosition;
            float shakeDuration = 0.3f;
            float shakeMagnitude = 12f;
            float elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;
                float t = 1f - (elapsed / shakeDuration);

                float x = Mathf.Sin(elapsed * 50f) * shakeMagnitude * t;

                buttonFace.anchoredPosition = startPos + new Vector2(x, 0);

                yield return null;
            }

            buttonFace.anchoredPosition = startPos;

            // Volver a color normal
            float fadeDuration = 0.25f;
            elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;

                if (faceImage != null)
                    faceImage.color = Color.Lerp(errorFaceColor, normalFaceColor, t);

                if (glowOutline != null)
                    glowOutline.effectColor = Color.Lerp(errorGlowColor, normalGlowColor, t);

                if (answerText != null)
                    answerText.color = Color.Lerp(errorGlowColor, normalGlowColor, t);

                yield return null;
            }

            if (faceImage != null) faceImage.color = normalFaceColor;
            if (glowOutline != null) glowOutline.effectColor = normalGlowColor;
            if (answerText != null) answerText.color = normalGlowColor;
        }

        private IEnumerator AnimateVictoryCelebration(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (buttonFace == null) yield break;

            Vector2 startPos = buttonFace.anchoredPosition;

            float jumpDuration = 0.4f;
            float elapsed = 0f;
            float jumpHeight = 20f;

            while (elapsed < jumpDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / jumpDuration;

                // Salto parabólico
                float jumpT = Mathf.Sin(t * Mathf.PI);
                buttonFace.anchoredPosition = startPos + new Vector2(0, jumpHeight * jumpT);

                // Glow intenso
                if (glowOutline != null)
                {
                    Color celebrationGlow = Color.Lerp(normalGlowColor, comboGlowColor, jumpT);
                    glowOutline.effectColor = celebrationGlow;
                }

                if (answerText != null)
                {
                    answerText.color = Color.Lerp(normalGlowColor, comboGlowColor, jumpT);
                }

                // Escala bounce
                float scale = 1f + jumpT * 0.15f;
                buttonFace.localScale = Vector3.one * scale;

                yield return null;
            }

            buttonFace.anchoredPosition = startPos;
            buttonFace.localScale = Vector3.one;
            if (glowOutline != null) glowOutline.effectColor = normalGlowColor;
            if (answerText != null) answerText.color = normalGlowColor;
        }

        /// <summary>
        /// Animación de entrada para nueva pregunta
        /// </summary>
        public void AnimateNewQuestion(float delay)
        {
            StartCoroutine(AnimatePopIn(delay));
        }

        private IEnumerator AnimatePopIn(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (buttonFace == null) yield break;

            // Empezar pequeño
            Vector3 originalScale = Vector3.one;
            buttonFace.localScale = Vector3.zero;

            float duration = 0.25f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Ease out back (overshoot)
                float overshoot = 1.3f;
                float s = t - 1f;
                float scale = 1f + s * s * ((overshoot + 1f) * s + overshoot);

                buttonFace.localScale = originalScale * Mathf.Max(0, scale);

                yield return null;
            }

            buttonFace.localScale = originalScale;
        }

        #region Hover Animations

        private IEnumerator AnimateHoverEnter()
        {
            if (buttonFace == null) yield break;

            float duration = 0.1f;
            float elapsed = 0f;

            Vector2 startPos = buttonFace.anchoredPosition;
            Vector2 targetPos = originalPosition + new Vector2(0, hoverLiftAmount);
            Vector3 startScale = buttonFace.localScale;
            Vector3 targetScale = originalScale * hoverScaleMultiplier;
            Color startGlow = glowOutline != null ? glowOutline.effectColor : normalGlowColor;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutCubic(elapsed / duration);

                buttonFace.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                buttonFace.localScale = Vector3.Lerp(startScale, targetScale, t);

                if (glowOutline != null)
                    glowOutline.effectColor = Color.Lerp(startGlow, hoverGlowColor, t);

                if (answerText != null)
                    answerText.color = Color.Lerp(startGlow, hoverGlowColor, t);

                yield return null;
            }

            buttonFace.anchoredPosition = targetPos;
            buttonFace.localScale = targetScale;
            if (glowOutline != null) glowOutline.effectColor = hoverGlowColor;
            if (answerText != null) answerText.color = hoverGlowColor;
        }

        private IEnumerator AnimateHoverExit()
        {
            if (buttonFace == null) yield break;

            float duration = 0.12f;
            float elapsed = 0f;

            Vector2 startPos = buttonFace.anchoredPosition;
            Vector3 startScale = buttonFace.localScale;
            Color startGlow = glowOutline != null ? glowOutline.effectColor : hoverGlowColor;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutCubic(elapsed / duration);

                buttonFace.anchoredPosition = Vector2.Lerp(startPos, originalPosition, t);
                buttonFace.localScale = Vector3.Lerp(startScale, originalScale, t);

                if (glowOutline != null)
                    glowOutline.effectColor = Color.Lerp(startGlow, normalGlowColor, t);

                if (answerText != null)
                    answerText.color = Color.Lerp(startGlow, normalGlowColor, t);

                yield return null;
            }

            buttonFace.anchoredPosition = originalPosition;
            buttonFace.localScale = originalScale;
            if (glowOutline != null) glowOutline.effectColor = normalGlowColor;
            if (answerText != null) answerText.color = normalGlowColor;

            // Reiniciar breathing después de hover
            if (enableBreathing && !isAnimatingResult)
            {
                StartBreathing();
            }
        }

        #endregion

        #region Breathing Animation

        private void StartBreathing()
        {
            if (breathingCoroutine != null)
                StopCoroutine(breathingCoroutine);

            breathingCoroutine = StartCoroutine(AnimateBreathing());
        }

        private void StopBreathing()
        {
            if (breathingCoroutine != null)
            {
                StopCoroutine(breathingCoroutine);
                breathingCoroutine = null;
            }
        }

        private IEnumerator AnimateBreathing()
        {
            if (buttonFace == null) yield break;

            float randomOffset = Random.Range(0f, Mathf.PI * 2f);

            while (enableBreathing && !isAnimatingResult && !isHovering)
            {
                float t = (Mathf.Sin(Time.time * breathingSpeed + randomOffset) + 1f) * 0.5f;

                // Escala sutil
                float scale = Mathf.Lerp(breathingScaleMin, breathingScaleMax, t);
                buttonFace.localScale = originalScale * scale;

                // Glow pulsante
                if (glowOutline != null)
                {
                    Color glowColor = normalGlowColor * Mathf.Lerp(breathingGlowMin, breathingGlowMax, t);
                    glowColor.a = normalGlowColor.a;
                    glowOutline.effectColor = glowColor;
                }

                if (answerText != null)
                {
                    Color textColor = normalGlowColor * Mathf.Lerp(breathingGlowMin, breathingGlowMax, t);
                    textColor.a = normalGlowColor.a;
                    answerText.color = textColor;
                }

                yield return null;
            }

            // Volver a estado normal
            if (buttonFace != null)
                buttonFace.localScale = originalScale;
            if (glowOutline != null)
                glowOutline.effectColor = normalGlowColor;
            if (answerText != null)
                answerText.color = normalGlowColor;
        }

        #endregion

        #region Easing

        private float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);

        #endregion
    }
}
