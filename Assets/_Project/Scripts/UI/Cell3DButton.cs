using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

namespace DigitPark.UI
{
    /// <summary>
    /// Componente para dar efecto 3D a los botones del grid
    /// Simula botones físicos que se presionan y levantan
    /// Con efectos de iluminación y oscurecimiento realistas
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class Cell3DButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("3D Effect Settings")]
        [SerializeField] private float pressDepth = 12f;
        [SerializeField] private float pressDuration = 0.06f;
        [SerializeField] private float releaseDuration = 0.15f;

        [Header("Visual References")]
        [SerializeField] private RectTransform buttonFace;
        [SerializeField] private Image shadowImage;
        [SerializeField] private Image sideImage;
        [SerializeField] private Image faceImage;
        [SerializeField] private TextMeshProUGUI numberText;

        [Header("Colors - Normal State")]
        [SerializeField] private Color faceColor = new Color(0.08f, 0.12f, 0.2f, 1f);
        [SerializeField] private Color sideColor = new Color(0.04f, 0.06f, 0.1f, 1f);
        [SerializeField] private Color shadowColor = new Color(0f, 0f, 0f, 0.5f);
        [SerializeField] private Color glowColor = new Color(0f, 1f, 1f, 1f);
        [SerializeField] private Color textColor = Color.white;

        [Header("Colors - Pressed State (Darker)")]
        [SerializeField] private Color pressedFaceColor = new Color(0.04f, 0.06f, 0.1f, 1f);
        [SerializeField] private Color pressedTextColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        [SerializeField] private Color pressedGlowColor = new Color(0f, 0.4f, 0.4f, 0.5f);

        [Header("Colors - Completed State (Green)")]
        [SerializeField] private Color completedFaceColor = new Color(0.1f, 0.3f, 0.15f, 1f);
        [SerializeField] private Color completedTextColor = new Color(0.5f, 0.8f, 0.5f, 1f);
        [SerializeField] private Color completedGlowColor = new Color(0.2f, 0.8f, 0.3f, 0.6f);

        [Header("Colors - Combo (Dorado)")]
        [SerializeField] private Color comboGlowColor = new Color(1f, 0.85f, 0.2f, 1f);
        [SerializeField] private Color comboFaceColor = new Color(0.2f, 0.25f, 0.1f, 1f);

        [Header("State")]
        [SerializeField] private bool isPressed = false;
        [SerializeField] private bool isCompleted = false;
        [SerializeField] private bool isWaitingForGame = true; // Antes del countdown

        private Button button;
        private Vector2 originalPosition;
        private Coroutine currentAnimation;

        // Glow effect
        private Outline glowOutline;
        private Shadow innerShadow;

        private void Awake()
        {
            button = GetComponent<Button>();

            if (buttonFace != null)
            {
                originalPosition = buttonFace.anchoredPosition;
                faceImage = buttonFace.GetComponent<Image>();
                numberText = buttonFace.GetComponentInChildren<TextMeshProUGUI>();
            }

            glowOutline = buttonFace?.GetComponent<Outline>();
            innerShadow = buttonFace?.GetComponent<Shadow>();
        }

        private void Start()
        {
            // Iniciar en estado "esperando" (presionado y oscuro)
            SetWaitingState();
        }

        /// <summary>
        /// Inicializa el botón 3D con los componentes necesarios
        /// </summary>
        public void Initialize()
        {
            // Crear estructura 3D si no existe
            if (buttonFace == null)
            {
                SetupVisualStructure();
            }

            ResetButton();
        }

        private void SetupVisualStructure()
        {
            // La estructura visual se crea desde el UIBuilder
            // Este método es para inicialización en runtime si es necesario
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!button.interactable || isCompleted) return;

            PressButton();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // El botón permanece presionado si fue completado
            if (isCompleted) return;

            // Solo liberar si no fue marcado como completado durante el press
            if (!isCompleted)
            {
                // No liberar automáticamente - el GameManager decidirá
            }
        }

        /// <summary>
        /// Presiona el botón visualmente
        /// </summary>
        public void PressButton()
        {
            if (isPressed) return;
            isPressed = true;

            if (currentAnimation != null)
                StopCoroutine(currentAnimation);

            currentAnimation = StartCoroutine(AnimatePress());
        }

        /// <summary>
        /// Libera el botón (vuelve a su posición original)
        /// </summary>
        public void ReleaseButton()
        {
            if (!isPressed) return;
            isPressed = false;
            isCompleted = false;

            if (currentAnimation != null)
                StopCoroutine(currentAnimation);

            currentAnimation = StartCoroutine(AnimateRelease());
        }

        /// <summary>
        /// Marca el botón como completado (se queda presionado, verde)
        /// </summary>
        public void MarkAsCompleted()
        {
            AnimateToCompleted(1);
        }

        /// <summary>
        /// Marca como completado con nivel de combo (colores más dorados con combo alto)
        /// </summary>
        public void AnimateToCompleted(int comboLevel)
        {
            isCompleted = true;
            isPressed = true;
            isWaitingForGame = false;

            if (currentAnimation != null)
                StopCoroutine(currentAnimation);

            currentAnimation = StartCoroutine(AnimateToCompletedWithCombo(comboLevel));
        }

        /// <summary>
        /// Animación de celebración de victoria
        /// </summary>
        public void PlayVictoryCelebration(float delay)
        {
            StartCoroutine(AnimateVictoryCelebration(delay));
        }

        /// <summary>
        /// Resetea el botón a estado normal (iluminado, no presionado)
        /// </summary>
        public void ResetToNormal()
        {
            ResetButton();
        }

        private IEnumerator AnimateVictoryCelebration(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (buttonFace == null || !isCompleted) yield break;

            // Guardar estado
            Vector2 startPos = buttonFace.anchoredPosition;
            Color startGlow = glowOutline != null ? glowOutline.effectColor : completedGlowColor;

            // Salto de celebración
            float jumpDuration = 0.35f;
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
                    Color celebrationGlow = Color.Lerp(startGlow, comboGlowColor, jumpT);
                    glowOutline.effectColor = celebrationGlow;
                }

                // Rotación leve
                buttonFace.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * Mathf.PI * 2f) * 4f);

                yield return null;
            }

            // Restaurar
            buttonFace.anchoredPosition = startPos;
            buttonFace.localRotation = Quaternion.identity;
            if (glowOutline != null) glowOutline.effectColor = startGlow;
        }

        private IEnumerator AnimateToCompletedWithCombo(int comboLevel)
        {
            if (buttonFace == null) yield break;

            // Calcular colores finales basados en combo
            float comboBlend = Mathf.Clamp01((comboLevel - 1) * 0.15f);
            Color targetFaceColor = Color.Lerp(completedFaceColor, comboFaceColor, comboBlend);
            Color targetGlowColor = Color.Lerp(completedGlowColor, comboGlowColor, comboBlend);
            Color targetTextColor = Color.Lerp(completedTextColor, new Color(1f, 0.95f, 0.6f), comboBlend);

            // Escala de pulso basada en combo
            float pulseScale = 1f + comboLevel * 0.03f;

            // Flash inicial
            if (faceImage != null) faceImage.color = Color.white * 0.8f;
            if (glowOutline != null) glowOutline.effectColor = Color.white;

            yield return new WaitForSeconds(0.03f);

            // Asegurar posición presionada
            Vector2 pressedPos = originalPosition + new Vector2(0, -pressDepth);
            buttonFace.anchoredPosition = pressedPos;

            // Animación con pulso
            float duration = 0.25f;
            float elapsed = 0f;

            Color startFaceColor = faceImage != null ? faceImage.color : faceColor;
            Color startTextColor = numberText != null ? numberText.color : textColor;
            Vector3 startScale = buttonFace.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Ease out back para bounce
                float easeT = 1f + 2.7f * Mathf.Pow(t - 1f, 3f) + 1.7f * Mathf.Pow(t - 1f, 2f);

                // Pulso de escala
                float scalePulse = 1f + Mathf.Sin(t * Mathf.PI) * (pulseScale - 1f);
                buttonFace.localScale = startScale * scalePulse;

                // Transición de colores
                if (faceImage != null)
                    faceImage.color = Color.Lerp(startFaceColor, targetFaceColor, easeT);

                if (numberText != null)
                    numberText.color = Color.Lerp(startTextColor, targetTextColor, easeT);

                if (glowOutline != null)
                    glowOutline.effectColor = Color.Lerp(Color.white, targetGlowColor, easeT);

                yield return null;
            }

            // Estado final
            buttonFace.localScale = startScale;
            if (faceImage != null) faceImage.color = targetFaceColor;
            if (numberText != null) numberText.color = targetTextColor;
            if (glowOutline != null) glowOutline.effectColor = targetGlowColor;

            // Reducir lado
            if (sideImage != null)
            {
                RectTransform sideRT = sideImage.GetComponent<RectTransform>();
                sideRT.sizeDelta = new Vector2(sideRT.sizeDelta.x, 2f);
            }
        }

        /// <summary>
        /// Establece estado de espera (antes del countdown) - oscuro y presionado
        /// </summary>
        public void SetWaitingState()
        {
            isWaitingForGame = true;
            isPressed = true;

            if (buttonFace != null)
            {
                // Posición presionada
                buttonFace.anchoredPosition = originalPosition + new Vector2(0, -pressDepth);
            }

            // Colores oscuros
            if (faceImage != null)
                faceImage.color = pressedFaceColor;

            if (numberText != null)
                numberText.color = pressedTextColor;

            if (glowOutline != null)
                glowOutline.effectColor = pressedGlowColor;

            // Lado reducido
            if (sideImage != null)
            {
                RectTransform sideRT = sideImage.GetComponent<RectTransform>();
                sideRT.sizeDelta = new Vector2(sideRT.sizeDelta.x, 2f);
            }

            // Sombra más pequeña
            if (shadowImage != null)
            {
                shadowImage.color = new Color(0f, 0f, 0f, 0.2f);
            }
        }

        /// <summary>
        /// Efecto de shake para error
        /// </summary>
        public void ShakeError()
        {
            if (currentAnimation != null)
                StopCoroutine(currentAnimation);

            currentAnimation = StartCoroutine(AnimateShake());
        }

        /// <summary>
        /// Resetea el botón a su estado inicial (levantado, iluminado)
        /// </summary>
        public void ResetButton()
        {
            isPressed = false;
            isCompleted = false;
            isWaitingForGame = false;

            if (buttonFace != null)
            {
                buttonFace.anchoredPosition = originalPosition;
            }

            // Colores normales (iluminados)
            if (faceImage != null)
                faceImage.color = faceColor;

            if (numberText != null)
                numberText.color = textColor;

            if (glowOutline != null)
                glowOutline.effectColor = glowColor;

            // Lado completo
            if (sideImage != null)
            {
                sideImage.gameObject.SetActive(true);
                RectTransform sideRT = sideImage.GetComponent<RectTransform>();
                sideRT.sizeDelta = new Vector2(sideRT.sizeDelta.x, pressDepth);
            }

            // Sombra normal
            if (shadowImage != null)
            {
                shadowImage.gameObject.SetActive(true);
                shadowImage.color = shadowColor;
            }

            button.interactable = true;
        }

        /// <summary>
        /// Animación de todos los botones levantándose al inicio del juego
        /// </summary>
        public void AnimateGameStart()
        {
            StartCoroutine(AnimatePopUp());
        }

        private IEnumerator AnimatePress()
        {
            if (buttonFace == null) yield break;

            Vector2 startPos = buttonFace.anchoredPosition;
            Vector2 targetPos = originalPosition + new Vector2(0, -pressDepth);

            // Guardar colores iniciales
            Color startFaceColor = faceImage != null ? faceImage.color : faceColor;
            Color startTextColor = numberText != null ? numberText.color : textColor;
            Color startGlowColor = glowOutline != null ? glowOutline.effectColor : glowColor;

            float elapsed = 0f;

            while (elapsed < pressDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / pressDuration;

                // Ease out for snappy press
                t = 1f - Mathf.Pow(1f - t, 3f);

                // Posición
                buttonFace.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);

                // Reducir altura del lado
                if (sideImage != null)
                {
                    RectTransform sideRT = sideImage.GetComponent<RectTransform>();
                    float sideHeight = Mathf.Lerp(pressDepth, 2f, t);
                    sideRT.sizeDelta = new Vector2(sideRT.sizeDelta.x, sideHeight);
                }

                // Oscurecer colores
                if (faceImage != null)
                    faceImage.color = Color.Lerp(startFaceColor, pressedFaceColor, t);

                if (numberText != null)
                    numberText.color = Color.Lerp(startTextColor, pressedTextColor, t);

                if (glowOutline != null)
                    glowOutline.effectColor = Color.Lerp(startGlowColor, pressedGlowColor, t);

                yield return null;
            }

            // Estado final presionado
            buttonFace.anchoredPosition = targetPos;

            if (sideImage != null)
            {
                RectTransform sideRT = sideImage.GetComponent<RectTransform>();
                sideRT.sizeDelta = new Vector2(sideRT.sizeDelta.x, 2f);
            }

            if (faceImage != null) faceImage.color = pressedFaceColor;
            if (numberText != null) numberText.color = pressedTextColor;
            if (glowOutline != null) glowOutline.effectColor = pressedGlowColor;
        }

        private IEnumerator AnimateRelease()
        {
            if (buttonFace == null) yield break;

            Vector2 startPos = buttonFace.anchoredPosition;
            Vector2 targetPos = originalPosition;

            // Guardar colores iniciales (presionados/oscuros)
            Color startFaceColor = faceImage != null ? faceImage.color : pressedFaceColor;
            Color startTextColor = numberText != null ? numberText.color : pressedTextColor;
            Color startGlowColor = glowOutline != null ? glowOutline.effectColor : pressedGlowColor;

            float elapsed = 0f;

            while (elapsed < releaseDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / releaseDuration;

                // Ease out bounce
                t = 1f - Mathf.Pow(1f - t, 2f);

                // Posición
                buttonFace.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);

                // Restaurar altura del lado
                if (sideImage != null)
                {
                    RectTransform sideRT = sideImage.GetComponent<RectTransform>();
                    float sideHeight = Mathf.Lerp(2f, pressDepth, t);
                    sideRT.sizeDelta = new Vector2(sideRT.sizeDelta.x, sideHeight);
                }

                // Restaurar colores (iluminar)
                if (faceImage != null)
                    faceImage.color = Color.Lerp(startFaceColor, faceColor, t);

                if (numberText != null)
                    numberText.color = Color.Lerp(startTextColor, textColor, t);

                if (glowOutline != null)
                    glowOutline.effectColor = Color.Lerp(startGlowColor, glowColor, t);

                yield return null;
            }

            // Estado final liberado
            buttonFace.anchoredPosition = targetPos;

            if (sideImage != null)
            {
                RectTransform sideRT = sideImage.GetComponent<RectTransform>();
                sideRT.sizeDelta = new Vector2(sideRT.sizeDelta.x, pressDepth);
            }

            if (faceImage != null) faceImage.color = faceColor;
            if (numberText != null) numberText.color = textColor;
            if (glowOutline != null) glowOutline.effectColor = glowColor;
        }

        private IEnumerator AnimateShake()
        {
            if (buttonFace == null) yield break;

            Vector2 startPos = buttonFace.anchoredPosition;
            float shakeDuration = 0.3f;
            float shakeMagnitude = 10f;
            float elapsed = 0f;

            // Flash red
            Image faceImg = buttonFace.GetComponent<Image>();
            Color originalColor = faceImg != null ? faceImg.color : faceColor;

            if (faceImg != null)
            {
                faceImg.color = new Color(0.8f, 0.2f, 0.2f, 1f);
            }

            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;

                float x = Random.Range(-1f, 1f) * shakeMagnitude * (1f - elapsed / shakeDuration);
                float y = Random.Range(-1f, 1f) * shakeMagnitude * (1f - elapsed / shakeDuration);

                buttonFace.anchoredPosition = startPos + new Vector2(x, y);

                yield return null;
            }

            buttonFace.anchoredPosition = startPos;

            if (faceImg != null)
            {
                faceImg.color = originalColor;
            }
        }

        private IEnumerator AnimatePopUp()
        {
            if (buttonFace == null) yield break;

            isWaitingForGame = false;

            // Empezar presionado y oscuro
            Vector2 pressedPos = originalPosition + new Vector2(0, -pressDepth);
            buttonFace.anchoredPosition = pressedPos;

            if (sideImage != null)
            {
                RectTransform sideRT = sideImage.GetComponent<RectTransform>();
                sideRT.sizeDelta = new Vector2(sideRT.sizeDelta.x, 2f);
            }

            // Pequeño delay aleatorio para efecto de ola
            yield return new WaitForSeconds(Random.Range(0f, 0.2f));

            // Pop up con bounce + iluminación
            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Bounce ease
                float bounce = 1f + Mathf.Sin(t * Mathf.PI) * 0.15f;
                float posT = Mathf.Min(1f, t * bounce);

                // Posición
                buttonFace.anchoredPosition = Vector2.Lerp(pressedPos, originalPosition, posT);

                // Lado
                if (sideImage != null)
                {
                    RectTransform sideRT = sideImage.GetComponent<RectTransform>();
                    float sideHeight = Mathf.Lerp(2f, pressDepth, posT);
                    sideRT.sizeDelta = new Vector2(sideRT.sizeDelta.x, sideHeight);
                }

                // Transición de color oscuro a iluminado
                float colorT = Mathf.SmoothStep(0f, 1f, t);

                if (faceImage != null)
                    faceImage.color = Color.Lerp(pressedFaceColor, faceColor, colorT);

                if (numberText != null)
                    numberText.color = Color.Lerp(pressedTextColor, textColor, colorT);

                if (glowOutline != null)
                    glowOutline.effectColor = Color.Lerp(pressedGlowColor, glowColor, colorT);

                if (shadowImage != null)
                    shadowImage.color = Color.Lerp(new Color(0f, 0f, 0f, 0.2f), shadowColor, colorT);

                yield return null;
            }

            // Estado final
            buttonFace.anchoredPosition = originalPosition;
            if (faceImage != null) faceImage.color = faceColor;
            if (numberText != null) numberText.color = textColor;
            if (glowOutline != null) glowOutline.effectColor = glowColor;
            if (shadowImage != null) shadowImage.color = shadowColor;

            isPressed = false;
        }
    }
}
