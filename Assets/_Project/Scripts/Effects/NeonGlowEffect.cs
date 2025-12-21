using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace DigitPark.Effects
{
    /// <summary>
    /// Efecto de brillo neon para elementos de UI
    /// Incluye: glow pulsante, glow estatico, rainbow glow
    /// </summary>
    [RequireComponent(typeof(Graphic))]
    public class NeonGlowEffect : MonoBehaviour
    {
        [Header("Glow Settings")]
        [SerializeField] private GlowMode glowMode = GlowMode.Pulse;
        [SerializeField] private Color glowColor = new Color(0f, 0.9608f, 1f, 1f);
        [SerializeField] private float glowIntensity = 1f;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float minGlow = 0.3f;
        [SerializeField] private float maxGlow = 1f;

        [Header("Outline")]
        [SerializeField] private bool useOutline = true;
        [SerializeField] private Vector2 outlineDistance = new Vector2(3, 3);

        [Header("Shadow")]
        [SerializeField] private bool useShadow = true;
        [SerializeField] private Vector2 shadowDistance = new Vector2(4, -4);

        [Header("Auto Start")]
        [SerializeField] private bool playOnStart = true;

        public enum GlowMode
        {
            Static,
            Pulse,
            Rainbow,
            Breathing,
            Flicker
        }

        private Graphic targetGraphic;
        private Outline outline;
        private Shadow shadow;
        private Color originalColor;
        private Coroutine glowCoroutine;
        private bool isPlaying;

        private void Awake()
        {
            targetGraphic = GetComponent<Graphic>();
            originalColor = targetGraphic.color;

            SetupEffects();
        }

        private void Start()
        {
            if (playOnStart)
            {
                StartGlow();
            }
        }

        private void OnDestroy()
        {
            StopGlow();
        }

        private void SetupEffects()
        {
            if (useOutline)
            {
                outline = GetComponent<Outline>();
                if (outline == null)
                {
                    outline = gameObject.AddComponent<Outline>();
                }
                outline.effectColor = glowColor;
                outline.effectDistance = outlineDistance;
            }

            if (useShadow)
            {
                shadow = GetComponent<Shadow>();
                if (shadow == null)
                {
                    shadow = gameObject.AddComponent<Shadow>();
                }
                shadow.effectColor = new Color(glowColor.r, glowColor.g, glowColor.b, 0.5f);
                shadow.effectDistance = shadowDistance;
            }
        }

        #region Public Methods

        public void StartGlow()
        {
            if (isPlaying) return;

            isPlaying = true;

            switch (glowMode)
            {
                case GlowMode.Static:
                    ApplyStaticGlow();
                    break;
                case GlowMode.Pulse:
                    glowCoroutine = StartCoroutine(PulseGlow());
                    break;
                case GlowMode.Rainbow:
                    glowCoroutine = StartCoroutine(RainbowGlow());
                    break;
                case GlowMode.Breathing:
                    glowCoroutine = StartCoroutine(BreathingGlow());
                    break;
                case GlowMode.Flicker:
                    glowCoroutine = StartCoroutine(FlickerGlow());
                    break;
            }
        }

        public void StopGlow()
        {
            if (!isPlaying) return;

            isPlaying = false;

            if (glowCoroutine != null)
            {
                StopCoroutine(glowCoroutine);
                glowCoroutine = null;
            }

            // Restaurar
            if (outline != null)
            {
                outline.effectColor = glowColor;
            }
        }

        public void SetGlowColor(Color color)
        {
            glowColor = color;

            if (outline != null)
            {
                outline.effectColor = color;
            }

            if (shadow != null)
            {
                shadow.effectColor = new Color(color.r, color.g, color.b, 0.5f);
            }
        }

        public void SetGlowMode(GlowMode mode)
        {
            bool wasPlaying = isPlaying;

            if (wasPlaying)
            {
                StopGlow();
            }

            glowMode = mode;

            if (wasPlaying)
            {
                StartGlow();
            }
        }

        #endregion

        #region Glow Modes

        private void ApplyStaticGlow()
        {
            if (outline != null)
            {
                Color c = glowColor;
                c.a = glowIntensity;
                outline.effectColor = c;
            }
        }

        private IEnumerator PulseGlow()
        {
            while (isPlaying)
            {
                float t = (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f) + 1f) / 2f;
                float intensity = Mathf.Lerp(minGlow, maxGlow, t) * glowIntensity;

                ApplyGlowIntensity(intensity);

                yield return null;
            }
        }

        private IEnumerator RainbowGlow()
        {
            while (isPlaying)
            {
                float hue = (Time.time * pulseSpeed * 0.2f) % 1f;
                Color rainbowColor = Color.HSVToRGB(hue, 0.8f, 1f);

                SetGlowColorInternal(rainbowColor);

                yield return null;
            }
        }

        private IEnumerator BreathingGlow()
        {
            while (isPlaying)
            {
                // Breathing pattern - mas suave que pulse
                float t = Time.time * pulseSpeed;
                float intensity = (Mathf.Sin(t) * 0.5f + 0.5f) * (Mathf.Sin(t * 0.7f) * 0.5f + 0.5f);
                intensity = Mathf.Lerp(minGlow, maxGlow, intensity) * glowIntensity;

                ApplyGlowIntensity(intensity);

                // Tambien variar ligeramente el tamaño del outline
                if (outline != null)
                {
                    float distMult = Mathf.Lerp(0.8f, 1.2f, intensity);
                    outline.effectDistance = outlineDistance * distMult;
                }

                yield return null;
            }
        }

        private IEnumerator FlickerGlow()
        {
            while (isPlaying)
            {
                // Random flicker
                float baseIntensity = Mathf.Lerp(minGlow, maxGlow, 0.7f);
                float flicker = Random.Range(-0.3f, 0.3f);
                float intensity = Mathf.Clamp01(baseIntensity + flicker) * glowIntensity;

                ApplyGlowIntensity(intensity);

                yield return new WaitForSeconds(Random.Range(0.02f, 0.1f));
            }
        }

        private void ApplyGlowIntensity(float intensity)
        {
            if (outline != null)
            {
                Color c = glowColor;
                c.a = intensity;
                outline.effectColor = c;
            }

            if (shadow != null)
            {
                Color c = glowColor;
                c.a = intensity * 0.5f;
                shadow.effectColor = c;
            }
        }

        private void SetGlowColorInternal(Color color)
        {
            if (outline != null)
            {
                outline.effectColor = color;
            }

            if (shadow != null)
            {
                Color shadowColor = color;
                shadowColor.a = 0.5f;
                shadow.effectColor = shadowColor;
            }
        }

        #endregion
    }
}
