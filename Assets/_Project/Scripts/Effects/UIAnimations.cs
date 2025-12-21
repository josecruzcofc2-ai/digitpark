using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace DigitPark.Effects
{
    /// <summary>
    /// Biblioteca de animaciones de UI reutilizables
    /// Incluye: fade, scale, slide, bounce, shake, pulse
    /// </summary>
    public static class UIAnimations
    {
        #region Fade Animations

        public static IEnumerator FadeIn(CanvasGroup canvasGroup, float duration = 0.3f, System.Action onComplete = null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.gameObject.SetActive(true);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
            onComplete?.Invoke();
        }

        public static IEnumerator FadeOut(CanvasGroup canvasGroup, float duration = 0.3f, bool deactivate = true, System.Action onComplete = null)
        {
            float startAlpha = canvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = 0f;

            if (deactivate)
            {
                canvasGroup.gameObject.SetActive(false);
            }

            onComplete?.Invoke();
        }

        public static IEnumerator FadeInGraphic(Graphic graphic, float duration = 0.3f)
        {
            Color color = graphic.color;
            color.a = 0f;
            graphic.color = color;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                color.a = Mathf.Lerp(0f, 1f, elapsed / duration);
                graphic.color = color;
                yield return null;
            }

            color.a = 1f;
            graphic.color = color;
        }

        #endregion

        #region Scale Animations

        public static IEnumerator ScaleIn(RectTransform rectTransform, float duration = 0.3f, System.Action onComplete = null)
        {
            rectTransform.localScale = Vector3.zero;
            rectTransform.gameObject.SetActive(true);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                // Ease out back - overshoot ligeramente
                t = 1f - Mathf.Pow(1f - t, 3f);
                float scale = Mathf.LerpUnclamped(0f, 1f, t * 1.1f - Mathf.Sin(t * Mathf.PI) * 0.1f);
                rectTransform.localScale = Vector3.one * Mathf.Max(scale, 0f);
                yield return null;
            }

            rectTransform.localScale = Vector3.one;
            onComplete?.Invoke();
        }

        public static IEnumerator ScaleOut(RectTransform rectTransform, float duration = 0.2f, bool deactivate = true, System.Action onComplete = null)
        {
            Vector3 startScale = rectTransform.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = t * t; // Ease in
                rectTransform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                yield return null;
            }

            rectTransform.localScale = Vector3.zero;

            if (deactivate)
            {
                rectTransform.gameObject.SetActive(false);
            }

            onComplete?.Invoke();
        }

        public static IEnumerator PunchScale(RectTransform rectTransform, float punchAmount = 0.2f, float duration = 0.3f)
        {
            Vector3 originalScale = rectTransform.localScale;
            Vector3 punchScale = originalScale * (1f + punchAmount);

            float elapsed = 0f;
            float halfDuration = duration / 2f;

            // Scale up
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                rectTransform.localScale = Vector3.Lerp(originalScale, punchScale, t);
                yield return null;
            }

            // Scale down with bounce
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                // Bounce effect
                float bounce = Mathf.Sin(t * Mathf.PI * 2f) * (1f - t) * 0.1f;
                rectTransform.localScale = Vector3.Lerp(punchScale, originalScale, t) + Vector3.one * bounce;
                yield return null;
            }

            rectTransform.localScale = originalScale;
        }

        public static IEnumerator BounceIn(RectTransform rectTransform, float duration = 0.5f)
        {
            rectTransform.localScale = Vector3.zero;
            rectTransform.gameObject.SetActive(true);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Bounce easing
                float scale;
                if (t < 0.4f)
                {
                    scale = Mathf.Lerp(0f, 1.2f, t / 0.4f);
                }
                else if (t < 0.7f)
                {
                    scale = Mathf.Lerp(1.2f, 0.9f, (t - 0.4f) / 0.3f);
                }
                else
                {
                    scale = Mathf.Lerp(0.9f, 1f, (t - 0.7f) / 0.3f);
                }

                rectTransform.localScale = Vector3.one * scale;
                yield return null;
            }

            rectTransform.localScale = Vector3.one;
        }

        #endregion

        #region Slide Animations

        public static IEnumerator SlideIn(RectTransform rectTransform, SlideDirection direction, float distance = 500f, float duration = 0.3f)
        {
            Vector2 startPos = GetSlideStartPosition(rectTransform.anchoredPosition, direction, distance);
            Vector2 endPos = rectTransform.anchoredPosition;

            rectTransform.anchoredPosition = startPos;
            rectTransform.gameObject.SetActive(true);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = 1f - Mathf.Pow(1f - t, 3f); // Ease out cubic
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }

            rectTransform.anchoredPosition = endPos;
        }

        public static IEnumerator SlideOut(RectTransform rectTransform, SlideDirection direction, float distance = 500f, float duration = 0.3f, bool deactivate = true)
        {
            Vector2 startPos = rectTransform.anchoredPosition;
            Vector2 endPos = GetSlideEndPosition(startPos, direction, distance);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = t * t; // Ease in quad
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }

            rectTransform.anchoredPosition = endPos;

            if (deactivate)
            {
                rectTransform.gameObject.SetActive(false);
                rectTransform.anchoredPosition = startPos; // Reset for next show
            }
        }

        private static Vector2 GetSlideStartPosition(Vector2 endPos, SlideDirection direction, float distance)
        {
            return direction switch
            {
                SlideDirection.Left => endPos + Vector2.right * distance,
                SlideDirection.Right => endPos + Vector2.left * distance,
                SlideDirection.Up => endPos + Vector2.down * distance,
                SlideDirection.Down => endPos + Vector2.up * distance,
                _ => endPos
            };
        }

        private static Vector2 GetSlideEndPosition(Vector2 startPos, SlideDirection direction, float distance)
        {
            return direction switch
            {
                SlideDirection.Left => startPos + Vector2.left * distance,
                SlideDirection.Right => startPos + Vector2.right * distance,
                SlideDirection.Up => startPos + Vector2.up * distance,
                SlideDirection.Down => startPos + Vector2.down * distance,
                _ => startPos
            };
        }

        public enum SlideDirection
        {
            Left,
            Right,
            Up,
            Down
        }

        #endregion

        #region Shake Animation

        public static IEnumerator Shake(RectTransform rectTransform, float magnitude = 10f, float duration = 0.3f)
        {
            Vector2 originalPos = rectTransform.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude * (1f - elapsed / duration);
                float y = Random.Range(-1f, 1f) * magnitude * (1f - elapsed / duration) * 0.5f;

                rectTransform.anchoredPosition = originalPos + new Vector2(x, y);

                elapsed += Time.deltaTime;
                yield return null;
            }

            rectTransform.anchoredPosition = originalPos;
        }

        public static IEnumerator ShakeRotation(RectTransform rectTransform, float angle = 5f, float duration = 0.3f)
        {
            Quaternion originalRotation = rectTransform.localRotation;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float currentAngle = Random.Range(-angle, angle) * (1f - elapsed / duration);
                rectTransform.localRotation = originalRotation * Quaternion.Euler(0f, 0f, currentAngle);

                elapsed += Time.deltaTime;
                yield return null;
            }

            rectTransform.localRotation = originalRotation;
        }

        #endregion

        #region Pulse Animation

        public static IEnumerator Pulse(RectTransform rectTransform, float minScale = 0.95f, float maxScale = 1.05f, float speed = 2f, int pulseCount = 3)
        {
            Vector3 originalScale = rectTransform.localScale;
            int completed = 0;

            while (completed < pulseCount)
            {
                float elapsed = 0f;
                float pulseDuration = 1f / speed;

                // Expand
                while (elapsed < pulseDuration / 2f)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / (pulseDuration / 2f);
                    float scale = Mathf.Lerp(minScale, maxScale, t);
                    rectTransform.localScale = originalScale * scale;
                    yield return null;
                }

                // Contract
                elapsed = 0f;
                while (elapsed < pulseDuration / 2f)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / (pulseDuration / 2f);
                    float scale = Mathf.Lerp(maxScale, minScale, t);
                    rectTransform.localScale = originalScale * scale;
                    yield return null;
                }

                completed++;
            }

            rectTransform.localScale = originalScale;
        }

        public static IEnumerator ContinuousPulse(RectTransform rectTransform, float minScale = 0.98f, float maxScale = 1.02f, float speed = 1f)
        {
            Vector3 originalScale = rectTransform.localScale;

            while (true)
            {
                float scale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(Time.time * speed * Mathf.PI * 2f) + 1f) / 2f);
                rectTransform.localScale = originalScale * scale;
                yield return null;
            }
        }

        #endregion

        #region Color Animations

        public static IEnumerator ColorPulse(Graphic graphic, Color pulseColor, float duration = 0.5f)
        {
            Color originalColor = graphic.color;
            float elapsed = 0f;

            // To pulse color
            while (elapsed < duration / 2f)
            {
                elapsed += Time.deltaTime;
                graphic.color = Color.Lerp(originalColor, pulseColor, elapsed / (duration / 2f));
                yield return null;
            }

            // Back to original
            elapsed = 0f;
            while (elapsed < duration / 2f)
            {
                elapsed += Time.deltaTime;
                graphic.color = Color.Lerp(pulseColor, originalColor, elapsed / (duration / 2f));
                yield return null;
            }

            graphic.color = originalColor;
        }

        public static IEnumerator RainbowCycle(Graphic graphic, float speed = 1f, float duration = 3f)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float hue = (Time.time * speed) % 1f;
                graphic.color = Color.HSVToRGB(hue, 0.8f, 1f);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        #endregion

        #region Number Counter Animation

        public static IEnumerator CountUp(TextMeshProUGUI text, int startValue, int endValue, float duration = 1f, string format = "{0}")
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = 1f - Mathf.Pow(1f - t, 3f); // Ease out

                int currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, t));
                text.text = string.Format(format, currentValue);
                yield return null;
            }

            text.text = string.Format(format, endValue);
        }

        public static IEnumerator CountDown(TextMeshProUGUI text, int startValue, int endValue, float duration = 1f, string format = "{0}")
        {
            yield return CountUp(text, startValue, endValue, duration, format);
        }

        #endregion

        #region Typewriter Effect

        public static IEnumerator TypewriterEffect(TextMeshProUGUI text, string fullText, float charDelay = 0.05f)
        {
            text.text = "";

            foreach (char c in fullText)
            {
                text.text += c;
                yield return new WaitForSeconds(charDelay);
            }
        }

        #endregion
    }
}
