using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DigitPark.UI
{
    /// <summary>
    /// Countdown animado 3-2-1-GO! para inicio de juegos
    /// </summary>
    public class CountdownUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject countdownPanel;
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private Image backgroundOverlay;

        [Header("Animation Settings")]
        [SerializeField] private float numberDisplayDuration = 0.8f;
        [SerializeField] private float scaleUpSize = 1.5f;
        [SerializeField] private float scaleDownSize = 0.8f;

        [Header("Colors")]
        [SerializeField] private Color numberColor = new Color(0f, 1f, 1f, 1f); // Cyan
        [SerializeField] private Color goColor = new Color(0.3f, 1f, 0.5f, 1f); // Green
        [SerializeField] private Color overlayColor = new Color(0f, 0f, 0f, 0.5f);

        [Header("Audio (Optional)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip countSound;
        [SerializeField] private AudioClip goSound;

        // Events
        public event Action OnCountdownComplete;

        private RectTransform countdownTextRT;
        private Vector3 originalScale;
        private bool isCountingDown = false;

        private void Awake()
        {
            if (countdownText != null)
            {
                countdownTextRT = countdownText.GetComponent<RectTransform>();
                originalScale = countdownTextRT.localScale;
            }

            // Hide panel initially
            if (countdownPanel != null)
            {
                countdownPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Inicia el countdown 3-2-1-GO!
        /// </summary>
        public void StartCountdown()
        {
            if (isCountingDown) return;

            StartCoroutine(CountdownSequence());
        }

        /// <summary>
        /// Inicia countdown con callback personalizado
        /// </summary>
        public void StartCountdown(Action onComplete)
        {
            OnCountdownComplete = onComplete;
            StartCountdown();
        }

        private IEnumerator CountdownSequence()
        {
            isCountingDown = true;

            // Show panel
            if (countdownPanel != null)
            {
                countdownPanel.SetActive(true);
            }

            // Fade in overlay
            if (backgroundOverlay != null)
            {
                yield return StartCoroutine(FadeOverlay(0f, overlayColor.a, 0.2f));
            }

            // 3
            yield return StartCoroutine(ShowNumber("3", numberColor));

            // 2
            yield return StartCoroutine(ShowNumber("2", numberColor));

            // 1
            yield return StartCoroutine(ShowNumber("1", numberColor));

            // GO!
            yield return StartCoroutine(ShowGo());

            // Fade out overlay
            if (backgroundOverlay != null)
            {
                yield return StartCoroutine(FadeOverlay(overlayColor.a, 0f, 0.2f));
            }

            // Hide panel
            if (countdownPanel != null)
            {
                countdownPanel.SetActive(false);
            }

            isCountingDown = false;

            // Trigger event
            OnCountdownComplete?.Invoke();
        }

        private IEnumerator ShowNumber(string number, Color color)
        {
            if (countdownText == null) yield break;

            // Play sound
            PlaySound(countSound);

            // Set text
            countdownText.text = number;
            countdownText.color = color;

            // Animation: Scale up from small, then pulse
            float elapsed = 0f;
            float appearDuration = 0.15f;

            // Start small
            countdownTextRT.localScale = originalScale * scaleDownSize;

            // Scale up quickly (pop in)
            while (elapsed < appearDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / appearDuration;

                // Ease out back for bounce effect
                t = 1f + 2.7f * Mathf.Pow(t - 1f, 3f) + 1.7f * Mathf.Pow(t - 1f, 2f);

                countdownTextRT.localScale = Vector3.Lerp(
                    originalScale * scaleDownSize,
                    originalScale * scaleUpSize,
                    t
                );

                yield return null;
            }

            // Hold at large size briefly
            yield return new WaitForSeconds(0.1f);

            // Scale down to normal
            elapsed = 0f;
            float settleDuration = 0.2f;
            Vector3 startScale = countdownTextRT.localScale;

            while (elapsed < settleDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / settleDuration;

                // Ease out
                t = 1f - Mathf.Pow(1f - t, 2f);

                countdownTextRT.localScale = Vector3.Lerp(startScale, originalScale, t);

                yield return null;
            }

            countdownTextRT.localScale = originalScale;

            // Wait remaining time
            float remainingTime = numberDisplayDuration - appearDuration - settleDuration - 0.1f;
            if (remainingTime > 0)
            {
                // Pulse effect while waiting
                yield return StartCoroutine(PulseEffect(remainingTime));
            }
        }

        private IEnumerator ShowGo()
        {
            if (countdownText == null) yield break;

            // Play GO sound
            PlaySound(goSound);

            // Set text
            countdownText.text = "GO!";
            countdownText.color = goColor;

            // Big pop animation
            float elapsed = 0f;
            float popDuration = 0.2f;

            countdownTextRT.localScale = originalScale * 0.5f;

            // Pop up big
            while (elapsed < popDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / popDuration;

                // Strong ease out back
                t = 1f + 3f * Mathf.Pow(t - 1f, 3f) + 2f * Mathf.Pow(t - 1f, 2f);

                countdownTextRT.localScale = Vector3.Lerp(
                    originalScale * 0.5f,
                    originalScale * 1.8f,
                    t
                );

                yield return null;
            }

            // Hold
            yield return new WaitForSeconds(0.15f);

            // Fade out while staying big
            elapsed = 0f;
            float fadeOutDuration = 0.3f;
            Color startColor = countdownText.color;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeOutDuration;

                // Fade out
                countdownText.color = new Color(
                    startColor.r,
                    startColor.g,
                    startColor.b,
                    Mathf.Lerp(1f, 0f, t)
                );

                // Continue scaling up slightly
                countdownTextRT.localScale = Vector3.Lerp(
                    originalScale * 1.8f,
                    originalScale * 2.2f,
                    t
                );

                yield return null;
            }

            // Reset
            countdownText.color = startColor;
            countdownTextRT.localScale = originalScale;
        }

        private IEnumerator PulseEffect(float duration)
        {
            float elapsed = 0f;
            float pulseSpeed = 8f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                // Subtle pulse
                float pulse = 1f + Mathf.Sin(elapsed * pulseSpeed) * 0.05f;
                countdownTextRT.localScale = originalScale * pulse;

                yield return null;
            }

            countdownTextRT.localScale = originalScale;
        }

        private IEnumerator FadeOverlay(float fromAlpha, float toAlpha, float duration)
        {
            if (backgroundOverlay == null) yield break;

            float elapsed = 0f;
            Color color = backgroundOverlay.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                color.a = Mathf.Lerp(fromAlpha, toAlpha, t);
                backgroundOverlay.color = color;

                yield return null;
            }

            color.a = toAlpha;
            backgroundOverlay.color = color;
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        /// <summary>
        /// Cancela el countdown si está en progreso
        /// </summary>
        public void CancelCountdown()
        {
            if (isCountingDown)
            {
                StopAllCoroutines();
                isCountingDown = false;

                if (countdownPanel != null)
                {
                    countdownPanel.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Verifica si el countdown está en progreso
        /// </summary>
        public bool IsCountingDown => isCountingDown;
    }
}
