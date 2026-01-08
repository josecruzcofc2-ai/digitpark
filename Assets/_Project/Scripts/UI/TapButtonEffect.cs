using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace DigitPark.UI
{
    /// <summary>
    /// Componente para efectos visuales del botón de FlashTap
    /// Maneja estados: Espera, Activo (TAP!), Error (Muy pronto)
    /// </summary>
    public class TapButtonEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("References")]
        [SerializeField] private Image outerRing;
        [SerializeField] private Image innerBorder;
        [SerializeField] private Image innerFill;
        [SerializeField] private TMP_Text tapText;
        [SerializeField] private RectTransform buttonTransform;

        [Header("Colors - Wait State")]
        [SerializeField] private Color waitRingColor = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        [SerializeField] private Color waitBorderColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color waitTextColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        [Header("Colors - Go State")]
        [SerializeField] private Color goRingColor = new Color(0f, 1f, 0.5f, 0.5f);
        [SerializeField] private Color goBorderColor = new Color(0f, 1f, 0.5f, 1f);
        [SerializeField] private Color goTextColor = new Color(0f, 1f, 0.5f, 1f);

        [Header("Colors - Error State")]
        [SerializeField] private Color errorRingColor = new Color(1f, 0.3f, 0.3f, 0.5f);
        [SerializeField] private Color errorBorderColor = new Color(1f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color errorTextColor = new Color(1f, 0.3f, 0.3f, 1f);

        [Header("Animation")]
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseMinScale = 0.95f;
        [SerializeField] private float pulseMaxScale = 1.05f;
        [SerializeField] private float pressScale = 0.9f;
        [SerializeField] private float transitionDuration = 0.2f;

        public enum ButtonState { Wait, Go, Error }
        private ButtonState currentState = ButtonState.Wait;
        private Coroutine pulseCoroutine;
        private Coroutine transitionCoroutine;
        private Vector3 originalScale;
        private bool isPressed;

        private void Awake()
        {
            if (buttonTransform == null)
                buttonTransform = GetComponent<RectTransform>();

            originalScale = buttonTransform.localScale;

            AutoFindReferences();
        }

        private void OnEnable()
        {
            SetState(ButtonState.Wait);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            buttonTransform.localScale = originalScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isPressed = true;
            StopPulse();
            StartCoroutine(AnimateScale(originalScale * pressScale, 0.1f));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPressed = false;
            StartCoroutine(AnimateScale(originalScale, 0.1f));

            if (currentState == ButtonState.Go)
            {
                StartPulse();
            }
        }

        /// <summary>
        /// Cambia el estado visual del botón
        /// </summary>
        public void SetState(ButtonState state)
        {
            currentState = state;
            StopTransition();

            switch (state)
            {
                case ButtonState.Wait:
                    transitionCoroutine = StartCoroutine(TransitionToColors(
                        waitRingColor, waitBorderColor, waitTextColor));
                    StopPulse();
                    break;

                case ButtonState.Go:
                    transitionCoroutine = StartCoroutine(TransitionToColors(
                        goRingColor, goBorderColor, goTextColor));
                    StartPulse();
                    break;

                case ButtonState.Error:
                    transitionCoroutine = StartCoroutine(TransitionToColors(
                        errorRingColor, errorBorderColor, errorTextColor));
                    StopPulse();
                    StartCoroutine(ShakeEffect());
                    break;
            }
        }

        private IEnumerator TransitionToColors(Color ringColor, Color borderColor, Color textColor)
        {
            Color startRing = outerRing != null ? outerRing.color : ringColor;
            Color startBorder = innerBorder != null ? innerBorder.color : borderColor;
            Color startText = tapText != null ? tapText.color : textColor;

            float elapsed = 0f;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / transitionDuration;
                t = EaseOutQuad(t);

                if (outerRing != null)
                    outerRing.color = Color.Lerp(startRing, ringColor, t);

                if (innerBorder != null)
                    innerBorder.color = Color.Lerp(startBorder, borderColor, t);

                if (tapText != null)
                    tapText.color = Color.Lerp(startText, textColor, t);

                yield return null;
            }

            // Ensure final colors
            if (outerRing != null) outerRing.color = ringColor;
            if (innerBorder != null) innerBorder.color = borderColor;
            if (tapText != null) tapText.color = textColor;
        }

        private void StartPulse()
        {
            StopPulse();
            pulseCoroutine = StartCoroutine(PulseLoop());
        }

        private void StopPulse()
        {
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
                pulseCoroutine = null;
            }
        }

        private void StopTransition()
        {
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
                transitionCoroutine = null;
            }
        }

        private IEnumerator PulseLoop()
        {
            while (true)
            {
                if (!isPressed)
                {
                    // Scale up
                    yield return StartCoroutine(AnimateScale(originalScale * pulseMaxScale, 0.5f / pulseSpeed));
                    // Scale down
                    yield return StartCoroutine(AnimateScale(originalScale * pulseMinScale, 0.5f / pulseSpeed));
                }
                else
                {
                    yield return null;
                }
            }
        }

        private IEnumerator AnimateScale(Vector3 targetScale, float duration)
        {
            Vector3 startScale = buttonTransform.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = EaseInOutSine(t);

                buttonTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            buttonTransform.localScale = targetScale;
        }

        private IEnumerator ShakeEffect()
        {
            Vector3 originalPos = buttonTransform.anchoredPosition;
            float shakeDuration = 0.3f;
            float shakeIntensity = 10f;
            float elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;
                float x = Random.Range(-1f, 1f) * shakeIntensity * (1f - elapsed / shakeDuration);
                float y = Random.Range(-1f, 1f) * shakeIntensity * (1f - elapsed / shakeDuration);

                buttonTransform.anchoredPosition = originalPos + new Vector3(x, y, 0);
                yield return null;
            }

            buttonTransform.anchoredPosition = originalPos;
        }

        private float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }

        private float EaseInOutSine(float t)
        {
            return -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;
        }

        private void AutoFindReferences()
        {
            if (outerRing == null)
                outerRing = transform.Find("OuterRing")?.GetComponent<Image>();

            if (innerBorder == null)
                innerBorder = transform.Find("TapButtonImage")?.GetComponent<Image>();

            if (innerFill == null)
                innerFill = transform.Find("InnerFill")?.GetComponent<Image>();

            if (tapText == null)
                tapText = transform.Find("TapText")?.GetComponent<TMP_Text>();
        }

#if UNITY_EDITOR
        [ContextMenu("Auto Setup References")]
        private void EditorAutoSetup()
        {
            AutoFindReferences();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
