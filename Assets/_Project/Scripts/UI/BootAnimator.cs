using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Themes;

namespace DigitPark.UI
{
    /// <summary>
    /// Controlador de animaciones para la escena Boot
    /// Maneja efectos visuales: logo animado, partículas, glow, typewriter
    /// </summary>
    public class BootAnimator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private Image loadingBarFill;
        [SerializeField] private Image loadingBarGlow;
        [SerializeField] private CanvasGroup logoCanvasGroup;
        [SerializeField] private RectTransform logoTransform;
        [SerializeField] private ParticleSystem neonParticles;

        [Header("Animation Settings")]
        [SerializeField] private float logoFadeInDuration = 0.8f;
        [SerializeField] private float logoScaleDuration = 0.5f;
        [SerializeField] private float glowPulseDuration = 1.2f;
        [SerializeField] private float typewriterSpeed = 0.03f;

        private ThemeData currentTheme;
        private Coroutine typewriterCoroutine;
        private Coroutine glowCoroutine;
        private string targetLoadingText = "";

        // Colores del tema para efectos
        private Color accentColor;
        private Color glowColor;
        private Color secondaryAccent;

        private void Awake()
        {
            // Obtener tema actual
            if (ThemeManager.Instance != null)
            {
                currentTheme = ThemeManager.Instance.CurrentTheme;
                ApplyThemeColors();
            }
            else
            {
                // Colores por defecto si no hay ThemeManager
                accentColor = new Color(0f, 1f, 1f, 1f); // Cyan
                glowColor = new Color(0f, 1f, 1f, 0.5f);
                secondaryAccent = new Color(1f, 0f, 0.5f, 1f); // Magenta
            }
        }

        /// <summary>
        /// Aplica los colores del tema actual a las variables locales
        /// </summary>
        private void ApplyThemeColors()
        {
            if (currentTheme != null)
            {
                accentColor = currentTheme.primaryAccent;
                glowColor = currentTheme.glowColor;
                secondaryAccent = currentTheme.secondaryAccent;
            }
        }

        /// <summary>
        /// Inicia la secuencia completa de animación del Boot
        /// </summary>
        public void StartBootAnimation()
        {
            StartCoroutine(BootAnimationSequence());
        }

        /// <summary>
        /// Secuencia principal de animación
        /// </summary>
        private IEnumerator BootAnimationSequence()
        {
            // 1. Logo aparece con efecto "power on"
            yield return StartCoroutine(AnimateLogoPowerOn());

            // 2. Iniciar partículas de neón
            if (neonParticles != null)
            {
                neonParticles.Play();
            }

            // 3. Iniciar efecto glow en la barra de progreso
            if (loadingBarGlow != null)
            {
                glowCoroutine = StartCoroutine(AnimateGlowPulse());
            }
        }

        #region Logo Animation

        /// <summary>
        /// Animación de "encendido" del logo estilo arcade
        /// </summary>
        private IEnumerator AnimateLogoPowerOn()
        {
            if (logoCanvasGroup == null || logoTransform == null) yield break;

            // Estado inicial: invisible y pequeño
            logoCanvasGroup.alpha = 0f;
            logoTransform.localScale = Vector3.one * 0.5f;

            // Efecto de parpadeo inicial (como tubo de neón encendiéndose)
            for (int i = 0; i < 3; i++)
            {
                logoCanvasGroup.alpha = Random.Range(0.3f, 0.7f);
                yield return new WaitForSeconds(0.05f);
                logoCanvasGroup.alpha = 0f;
                yield return new WaitForSeconds(0.08f);
            }

            // Fade in suave con scale
            float elapsed = 0f;
            while (elapsed < logoFadeInDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / logoFadeInDuration;
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                logoCanvasGroup.alpha = smoothT;
                logoTransform.localScale = Vector3.Lerp(Vector3.one * 0.7f, Vector3.one, smoothT);

                yield return null;
            }

            // Asegurar estado final
            logoCanvasGroup.alpha = 1f;
            logoTransform.localScale = Vector3.one;

            // Pequeño "bounce" al final
            yield return StartCoroutine(LogoBounce());
        }

        /// <summary>
        /// Efecto bounce sutil del logo
        /// </summary>
        private IEnumerator LogoBounce()
        {
            if (logoTransform == null) yield break;

            float elapsed = 0f;
            while (elapsed < logoScaleDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / logoScaleDuration;

                // Curva de bounce
                float bounce = 1f + Mathf.Sin(t * Mathf.PI) * 0.1f;
                logoTransform.localScale = Vector3.one * bounce;

                yield return null;
            }

            logoTransform.localScale = Vector3.one;
        }

        #endregion

        #region Loading Bar Glow

        /// <summary>
        /// Efecto de pulso/glow en la barra de progreso
        /// </summary>
        private IEnumerator AnimateGlowPulse()
        {
            if (loadingBarGlow == null) yield break;

            while (true)
            {
                float elapsed = 0f;
                while (elapsed < glowPulseDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / glowPulseDuration;

                    // Onda sinusoidal para el alpha del glow
                    float alpha = 0.3f + Mathf.Sin(t * Mathf.PI * 2f) * 0.4f;
                    Color glowColorWithAlpha = glowColor;
                    glowColorWithAlpha.a = alpha;
                    loadingBarGlow.color = glowColorWithAlpha;

                    yield return null;
                }
            }
        }

        /// <summary>
        /// Actualiza el color de la barra según el progreso
        /// </summary>
        public void UpdateLoadingBarColor(float progress)
        {
            if (loadingBarFill == null) return;

            // Gradiente de color según progreso (accent -> secondary accent)
            Color barColor = Color.Lerp(accentColor, secondaryAccent, progress * 0.3f);
            loadingBarFill.color = barColor;
        }

        #endregion

        #region Typewriter Effect

        /// <summary>
        /// Muestra texto con efecto typewriter
        /// </summary>
        public void SetLoadingText(string text)
        {
            if (loadingText == null) return;

            // Si ya hay una animación, cancelarla
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
            }

            targetLoadingText = text;
            typewriterCoroutine = StartCoroutine(TypewriterEffect(text));
        }

        /// <summary>
        /// Efecto typewriter para el texto
        /// </summary>
        private IEnumerator TypewriterEffect(string text)
        {
            loadingText.text = "";

            foreach (char c in text)
            {
                loadingText.text += c;
                yield return new WaitForSeconds(typewriterSpeed);
            }

            typewriterCoroutine = null;
        }

        /// <summary>
        /// Establece el texto inmediatamente sin animación
        /// </summary>
        public void SetLoadingTextImmediate(string text)
        {
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
            }

            if (loadingText != null)
            {
                loadingText.text = text;
            }
        }

        #endregion

        #region Particle Control

        /// <summary>
        /// Configura las partículas con los colores del tema
        /// </summary>
        public void ConfigureParticles(ParticleSystem particles)
        {
            if (particles == null) return;

            neonParticles = particles;

            var main = particles.main;
            main.startColor = new ParticleSystem.MinMaxGradient(accentColor, secondaryAccent);

            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(accentColor, 0f),
                    new GradientColorKey(secondaryAccent, 0.5f),
                    new GradientColorKey(accentColor, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.8f, 0.3f),
                    new GradientAlphaKey(0.8f, 0.7f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;
        }

        #endregion

        #region Public Setters

        public void SetTitleText(TextMeshProUGUI title)
        {
            titleText = title;
        }

        public void SetLoadingTextReference(TextMeshProUGUI text)
        {
            loadingText = text;
        }

        public void SetLoadingBarFill(Image fill)
        {
            loadingBarFill = fill;
        }

        public void SetLoadingBarGlow(Image glow)
        {
            loadingBarGlow = glow;
        }

        public void SetLogoCanvasGroup(CanvasGroup cg)
        {
            logoCanvasGroup = cg;
        }

        public void SetLogoTransform(RectTransform rt)
        {
            logoTransform = rt;
        }

        #endregion

        private void OnDestroy()
        {
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
            }
            if (glowCoroutine != null)
            {
                StopCoroutine(glowCoroutine);
            }
        }
    }
}
