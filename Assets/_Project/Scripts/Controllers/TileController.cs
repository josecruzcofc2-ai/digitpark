using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace DigitPark.Controllers
{
    /// <summary>
    /// Controlador de cada tile individual en el grid
    /// Maneja la visualización, animación e interacción de cada número
    /// </summary>
    public class TileController : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI numberText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image highlightImage;

        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(0.1f, 0.1f, 0.2f);
        [SerializeField] private Color highlightColor = new Color(0f, 0.83f, 1f, 0.3f); // Azul eléctrico
        [SerializeField] private Color correctColor = new Color(0f, 1f, 0.53f); // Verde brillante
        [SerializeField] private Color wrongColor = new Color(1f, 0.42f, 0.42f); // Rojo coral
        [SerializeField] private Color completedColor = new Color(0.3f, 0.3f, 0.4f);

        [Header("Animation Settings")]
        [SerializeField] private float pulseDuration = 0.8f;
        [SerializeField] private float pulseScale = 1.1f;
        [SerializeField] private float shakeDuration = 0.3f;
        [SerializeField] private float shakeMagnitude = 10f;

        // Propiedades
        public int Number { get; private set; }

        // Estado
        private bool isHighlighted = false;
        private bool isCompleted = false;

        // Callback
        private Action<int> onClickCallback;

        // Animation
        private Animator animator;
        private RectTransform rectTransform;
        private Vector3 originalScale;
        private Vector2 originalPosition;

        // Pulse animation
        private bool isPulsing = false;
        private float pulseTimer = 0f;
        private bool hasPlayedEnterAnimation = false;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            animator = GetComponent<Animator>();

            originalScale = transform.localScale;
            originalPosition = rectTransform.anchoredPosition;
        }

        private void OnEnable()
        {
            // Si ya fue inicializado pero no ha jugado la animación, jugarla ahora
            if (Number > 0 && !hasPlayedEnterAnimation && gameObject.activeInHierarchy)
            {
                PlayEnterAnimation();
                hasPlayedEnterAnimation = true;
            }
        }

        private void Update()
        {
            // Animación de pulse cuando está highlighted
            if (isHighlighted && !isCompleted && isPulsing)
            {
                pulseTimer += Time.deltaTime;

                float normalizedTime = (pulseTimer % pulseDuration) / pulseDuration;
                float scale = 1f + (Mathf.Sin(normalizedTime * Mathf.PI * 2) * (pulseScale - 1f) * 0.5f);

                transform.localScale = originalScale * scale;
            }
        }

        #region Initialization

        /// <summary>
        /// Inicializa el tile con un número y callback
        /// </summary>
        public void Initialize(int number, Action<int> clickCallback)
        {
            Number = number;
            onClickCallback = clickCallback;

            // Configurar UI
            if (numberText != null)
            {
                numberText.text = number.ToString();
            }

            // Configurar color inicial
            if (backgroundImage != null)
            {
                backgroundImage.color = normalColor;
            }

            if (highlightImage != null)
            {
                highlightImage.gameObject.SetActive(false);
            }

            // Animación de entrada (solo si el GameObject está activo)
            if (gameObject.activeInHierarchy)
            {
                PlayEnterAnimation();
                hasPlayedEnterAnimation = true;
            }
        }

        #endregion

        #region Pointer Events

        /// <summary>
        /// Cuando se hace click en el tile
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (isCompleted) return;

            Debug.Log($"[Tile] Click en tile {Number}");

            // Invocar callback
            onClickCallback?.Invoke(Number);
        }

        /// <summary>
        /// Cuando se presiona el tile
        /// </summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (isCompleted) return;

            // Animación de presionado
            transform.localScale = originalScale * 0.9f;
        }

        /// <summary>
        /// Cuando se suelta el tile
        /// </summary>
        public void OnPointerUp(PointerEventData eventData)
        {
            if (isCompleted) return;

            // Volver a escala normal
            if (!isHighlighted)
            {
                transform.localScale = originalScale;
            }
        }

        #endregion

        #region State Management

        /// <summary>
        /// Establece si el tile está resaltado (es el siguiente a tocar)
        /// </summary>
        public void SetHighlighted(bool highlighted)
        {
            if (isCompleted) return;

            isHighlighted = highlighted;

            if (highlightImage != null)
            {
                highlightImage.gameObject.SetActive(highlighted);
                highlightImage.color = highlightColor;
            }

            if (highlighted)
            {
                // Iniciar animación de pulse
                isPulsing = true;
                pulseTimer = 0f;
            }
            else
            {
                // Detener animación de pulse
                isPulsing = false;
                transform.localScale = originalScale;
            }
        }

        /// <summary>
        /// Marca el tile como completado
        /// </summary>
        public void SetCompleted(bool completed)
        {
            isCompleted = completed;

            if (completed)
            {
                isPulsing = false;
                isHighlighted = false;

                if (backgroundImage != null)
                {
                    backgroundImage.color = completedColor;
                }

                if (numberText != null)
                {
                    numberText.color = Color.gray;
                }

                if (highlightImage != null)
                {
                    highlightImage.gameObject.SetActive(false);
                }
            }
        }

        #endregion

        #region Feedback Animations

        /// <summary>
        /// Animación cuando se toca correctamente
        /// </summary>
        public void OnCorrectTouch()
        {
            Debug.Log($"[Tile] Toque correcto en {Number}");

            // Marcar como completado
            SetCompleted(true);

            // Flash verde
            StartCoroutine(FlashColor(correctColor, 0.3f));

            // Animación de scale
            StartCoroutine(SuccessScaleAnimation());

            // Si tiene animator
            if (animator != null)
            {
                animator.SetTrigger("Correct");
            }
        }

        /// <summary>
        /// Animación cuando se toca incorrectamente
        /// </summary>
        public void OnWrongTouch()
        {
            Debug.Log($"[Tile] Toque incorrecto en {Number}");

            // Flash rojo
            StartCoroutine(FlashColor(wrongColor, 0.5f));

            // Animación de shake
            StartCoroutine(ShakeAnimation());

            // Si tiene animator
            if (animator != null)
            {
                animator.SetTrigger("Wrong");
            }
        }

        /// <summary>
        /// Flash de color
        /// </summary>
        private System.Collections.IEnumerator FlashColor(Color flashColor, float duration)
        {
            if (backgroundImage == null) yield break;

            Color originalColor = backgroundImage.color;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Ping-pong entre el color original y el flash
                backgroundImage.color = Color.Lerp(flashColor, originalColor, t);

                yield return null;
            }

            backgroundImage.color = originalColor;
        }

        /// <summary>
        /// Animación de scale cuando es correcto
        /// </summary>
        private System.Collections.IEnumerator SuccessScaleAnimation()
        {
            float duration = 0.3f;
            float elapsed = 0f;
            Vector3 targetScale = originalScale * 1.2f;

            // Crecer
            while (elapsed < duration / 2)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (duration / 2);
                transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }

            // Volver a tamaño normal
            elapsed = 0f;
            while (elapsed < duration / 2)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (duration / 2);
                transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }

            transform.localScale = originalScale;
        }

        /// <summary>
        /// Animación de shake cuando es incorrecto
        /// </summary>
        private System.Collections.IEnumerator ShakeAnimation()
        {
            float elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;

                float offsetX = UnityEngine.Random.Range(-shakeMagnitude, shakeMagnitude);
                float offsetY = UnityEngine.Random.Range(-shakeMagnitude, shakeMagnitude);

                rectTransform.anchoredPosition = originalPosition + new Vector2(offsetX, offsetY);

                yield return null;
            }

            rectTransform.anchoredPosition = originalPosition;
        }

        /// <summary>
        /// Animación de entrada cuando se crea el tile
        /// </summary>
        private void PlayEnterAnimation()
        {
            // Iniciar desde escala 0
            transform.localScale = Vector3.zero;

            // Animar a escala original
            StartCoroutine(EnterScaleAnimation());
        }

        /// <summary>
        /// Animación de escala al entrar
        /// </summary>
        private System.Collections.IEnumerator EnterScaleAnimation()
        {
            float duration = 0.3f;
            float elapsed = 0f;

            // Pequeño delay aleatorio para efecto en cascada
            yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 0.1f));

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Ease out elastic
                float easeT = EaseOutElastic(t);
                transform.localScale = originalScale * easeT;

                yield return null;
            }

            transform.localScale = originalScale;
        }

        #endregion

        #region Easing Functions

        /// <summary>
        /// Función de easing elastic
        /// </summary>
        private float EaseOutElastic(float t)
        {
            if (t == 0) return 0;
            if (t == 1) return 1;

            float p = 0.3f;
            return Mathf.Pow(2, -10 * t) * Mathf.Sin((t - p / 4) * (2 * Mathf.PI) / p) + 1;
        }

        #endregion
    }
}
