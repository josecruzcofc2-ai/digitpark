using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

namespace DigitPark.UI
{
    /// <summary>
    /// Componente para dar efecto 3D a las celdas de OddOneOut
    /// Con animaciones de acierto, error y celebración
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class OddOneOutCell3D : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("3D Effect Settings")]
        [SerializeField] private float pressDepth = 6f;
        [SerializeField] private float pressDuration = 0.05f;
        [SerializeField] private float releaseDuration = 0.12f;

        [Header("Visual References")]
        [SerializeField] private RectTransform buttonFace;
        [SerializeField] private Image shadowImage;
        [SerializeField] private Image sideImage;
        [SerializeField] private Image faceImage;
        [SerializeField] private Outline glowOutline;
        [SerializeField] private TextMeshProUGUI numberText;

        [Header("Grid Info")]
        [SerializeField] private bool isRightGrid;
        [SerializeField] private Color borderColor = Color.cyan;

        [Header("Colors")]
        [SerializeField] private Color normalFaceColor = new Color(0.08f, 0.12f, 0.18f, 1f);
        [SerializeField] private Color pressedFaceColor = new Color(0.04f, 0.06f, 0.1f, 1f);
        [SerializeField] private Color correctFaceColor = new Color(0.1f, 0.3f, 0.15f, 1f);
        [SerializeField] private Color errorFaceColor = new Color(0.3f, 0.1f, 0.1f, 1f);
        [SerializeField] private Color comboFaceColor = new Color(0.25f, 0.2f, 0.1f, 1f);

        private Button button;
        private Vector2 originalPosition;
        private Coroutine currentAnimation;
        private bool isPressed = false;

        private void Awake()
        {
            button = GetComponent<Button>();

            if (buttonFace != null)
            {
                originalPosition = buttonFace.anchoredPosition;
                if (faceImage == null)
                    faceImage = buttonFace.GetComponent<Image>();
                if (numberText == null)
                    numberText = buttonFace.GetComponentInChildren<TextMeshProUGUI>();
                if (glowOutline == null)
                    glowOutline = buttonFace.GetComponent<Outline>();
            }
        }

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
        /// Animación de acierto (encontró la diferencia)
        /// </summary>
        public void AnimateCorrect(int comboLevel = 1)
        {
            if (currentAnimation != null)
                StopCoroutine(currentAnimation);

            currentAnimation = StartCoroutine(AnimateCorrectSequence(comboLevel));
        }

        /// <summary>
        /// Animación de error
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
        /// Resetea la celda a estado normal
        /// </summary>
        public void ResetToNormal()
        {
            if (currentAnimation != null)
                StopCoroutine(currentAnimation);

            isPressed = false;

            if (buttonFace != null)
                buttonFace.anchoredPosition = originalPosition;

            if (faceImage != null)
                faceImage.color = normalFaceColor;

            if (glowOutline != null)
                glowOutline.effectColor = borderColor;

            if (numberText != null)
                numberText.color = Color.white;

            if (sideImage != null)
            {
                RectTransform sideRT = sideImage.GetComponent<RectTransform>();
                sideRT.sizeDelta = new Vector2(sideRT.sizeDelta.x, pressDepth);
            }

            if (shadowImage != null)
                shadowImage.color = new Color(0f, 0f, 0f, 0.4f);
        }

        /// <summary>
        /// Animación de pop-up al inicio de ronda
        /// </summary>
        public void AnimateRoundStart(float delay)
        {
            StartCoroutine(AnimatePopUp(delay));
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
                t = 1f - Mathf.Pow(1f - t, 3f); // Ease out

                buttonFace.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);

                if (sideImage != null)
                {
                    RectTransform sideRT = sideImage.GetComponent<RectTransform>();
                    float sideHeight = Mathf.Lerp(pressDepth, 1f, t);
                    sideRT.sizeDelta = new Vector2(sideRT.sizeDelta.x, sideHeight);
                }

                if (faceImage != null)
                    faceImage.color = Color.Lerp(normalFaceColor, pressedFaceColor, t);

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
                t = 1f - Mathf.Pow(1f - t, 2f); // Ease out

                buttonFace.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);

                if (sideImage != null)
                {
                    RectTransform sideRT = sideImage.GetComponent<RectTransform>();
                    float sideHeight = Mathf.Lerp(1f, pressDepth, t);
                    sideRT.sizeDelta = new Vector2(sideRT.sizeDelta.x, sideHeight);
                }

                if (faceImage != null)
                    faceImage.color = Color.Lerp(pressedFaceColor, normalFaceColor, t);

                yield return null;
            }

            buttonFace.anchoredPosition = targetPos;
            if (faceImage != null) faceImage.color = normalFaceColor;
        }

        private IEnumerator AnimateCorrectSequence(int comboLevel)
        {
            if (buttonFace == null) yield break;

            // Calcular colores basados en combo
            float comboBlend = Mathf.Clamp01((comboLevel - 1) * 0.2f);
            Color targetFaceColor = Color.Lerp(correctFaceColor, comboFaceColor, comboBlend);
            Color targetGlowColor = Color.Lerp(
                new Color(0.3f, 1f, 0.5f, 1f), // Verde
                new Color(1f, 0.85f, 0.2f, 1f), // Dorado
                comboBlend
            );

            // Flash blanco
            if (faceImage != null) faceImage.color = Color.white;
            if (glowOutline != null) glowOutline.effectColor = Color.white;

            yield return new WaitForSeconds(0.05f);

            // Pulso de escala
            float duration = 0.3f;
            float elapsed = 0f;
            Vector3 originalScale = buttonFace.localScale;
            float pulseScale = 1f + comboLevel * 0.05f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Pulso
                float scale = 1f + Mathf.Sin(t * Mathf.PI) * (pulseScale - 1f);
                buttonFace.localScale = originalScale * scale;

                // Transición de color
                if (faceImage != null)
                    faceImage.color = Color.Lerp(Color.white, targetFaceColor, t);

                if (glowOutline != null)
                    glowOutline.effectColor = Color.Lerp(Color.white, targetGlowColor, t);

                yield return null;
            }

            buttonFace.localScale = originalScale;
            if (faceImage != null) faceImage.color = targetFaceColor;
            if (glowOutline != null) glowOutline.effectColor = targetGlowColor;
        }

        private IEnumerator AnimateErrorSequence()
        {
            if (buttonFace == null) yield break;

            // Flash rojo
            if (faceImage != null) faceImage.color = new Color(1f, 0.3f, 0.3f, 1f);
            if (glowOutline != null) glowOutline.effectColor = new Color(1f, 0.3f, 0.3f, 1f);

            // Shake
            Vector2 startPos = buttonFace.anchoredPosition;
            float shakeDuration = 0.25f;
            float shakeMagnitude = 8f;
            float elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;
                float t = 1f - (elapsed / shakeDuration);

                float x = Random.Range(-1f, 1f) * shakeMagnitude * t;
                float y = Random.Range(-1f, 1f) * shakeMagnitude * t;

                buttonFace.anchoredPosition = startPos + new Vector2(x, y);

                yield return null;
            }

            buttonFace.anchoredPosition = startPos;

            // Volver a color normal
            float fadeDuration = 0.2f;
            elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;

                if (faceImage != null)
                    faceImage.color = Color.Lerp(errorFaceColor, normalFaceColor, t);

                if (glowOutline != null)
                    glowOutline.effectColor = Color.Lerp(new Color(1f, 0.3f, 0.3f, 1f), borderColor, t);

                yield return null;
            }

            if (faceImage != null) faceImage.color = normalFaceColor;
            if (glowOutline != null) glowOutline.effectColor = borderColor;
        }

        private IEnumerator AnimateVictoryCelebration(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (buttonFace == null) yield break;

            Vector2 startPos = buttonFace.anchoredPosition;
            Color startGlow = glowOutline != null ? glowOutline.effectColor : borderColor;

            float jumpDuration = 0.35f;
            float elapsed = 0f;
            float jumpHeight = 15f;

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
                    Color celebrationGlow = Color.Lerp(startGlow, new Color(1f, 0.85f, 0.2f, 1f), jumpT);
                    glowOutline.effectColor = celebrationGlow;
                }

                // Rotación leve
                buttonFace.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * Mathf.PI * 2f) * 3f);

                yield return null;
            }

            buttonFace.anchoredPosition = startPos;
            buttonFace.localRotation = Quaternion.identity;
            if (glowOutline != null) glowOutline.effectColor = startGlow;
        }

        private IEnumerator AnimatePopUp(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (buttonFace == null) yield break;

            // Empezar pequeño
            Vector3 originalScale = buttonFace.localScale;
            buttonFace.localScale = Vector3.zero;

            float duration = 0.25f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Ease out back
                float overshoot = 1.5f;
                float s = t - 1f;
                float scale = 1f + s * s * ((overshoot + 1f) * s + overshoot);

                buttonFace.localScale = originalScale * Mathf.Max(0, scale);

                yield return null;
            }

            buttonFace.localScale = originalScale;
        }
    }
}
