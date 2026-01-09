using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace DigitPark.UI
{
    /// <summary>
    /// Sistema de partículas basado en UI para efectos de chispas y confeti
    /// No requiere sistema de partículas de Unity, usa Image components
    /// </summary>
    public class UISparkleEffect : MonoBehaviour
    {
        [Header("Sparkle Settings")]
        [SerializeField] private int sparkleCount = 12;
        [SerializeField] private float sparkleSize = 15f;
        [SerializeField] private float sparkleLifetime = 0.6f;
        [SerializeField] private float sparkleSpeed = 300f;
        [SerializeField] private float sparkleSpread = 360f;

        [Header("Colors")]
        [SerializeField] private Color[] sparkleColors = new Color[]
        {
            new Color(0.3f, 1f, 0.5f, 1f),    // Verde
            new Color(0.5f, 1f, 0.7f, 1f),    // Verde claro
            new Color(1f, 1f, 1f, 1f),        // Blanco
            new Color(0.8f, 1f, 0.4f, 1f),    // Lima
        };

        [Header("Confetti Settings")]
        [SerializeField] private int confettiCount = 30;
        [SerializeField] private float confettiSize = 20f;
        [SerializeField] private float confettiLifetime = 2f;
        [SerializeField] private float confettiSpeed = 400f;
        [SerializeField] private float confettiGravity = 300f;

        [Header("Confetti Colors")]
        [SerializeField] private Color[] confettiColors = new Color[]
        {
            new Color(1f, 0.3f, 0.5f, 1f),    // Rosa
            new Color(0.3f, 1f, 0.5f, 1f),    // Verde
            new Color(0.3f, 0.5f, 1f, 1f),    // Azul
            new Color(1f, 1f, 0.3f, 1f),      // Amarillo
            new Color(1f, 0.5f, 0.3f, 1f),    // Naranja
            new Color(0.8f, 0.3f, 1f, 1f),    // Morado
        };

        private RectTransform rectTransform;
        private Canvas parentCanvas;
        private List<GameObject> activeParticles = new List<GameObject>();

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            parentCanvas = GetComponentInParent<Canvas>();
        }

        /// <summary>
        /// Dispara chispas verdes para match exitoso
        /// </summary>
        public void PlayMatchSparkles(Vector2 position, int comboLevel = 1)
        {
            StartCoroutine(SpawnSparkles(position, comboLevel));
        }

        /// <summary>
        /// Dispara chispas rojas para error
        /// </summary>
        public void PlayErrorSparkles(Vector2 position)
        {
            Color[] errorColors = new Color[]
            {
                new Color(1f, 0.3f, 0.3f, 1f),
                new Color(1f, 0.5f, 0.4f, 1f),
                new Color(0.8f, 0.2f, 0.2f, 1f),
            };

            StartCoroutine(SpawnSparklesWithColors(position, errorColors, 8, 0.4f));
        }

        /// <summary>
        /// Lluvia de confeti para victoria
        /// </summary>
        public void PlayVictoryConfetti()
        {
            StartCoroutine(SpawnConfetti());
        }

        /// <summary>
        /// Explosión de confeti desde el centro
        /// </summary>
        public void PlayConfettiExplosion(Vector2 position)
        {
            StartCoroutine(SpawnConfettiExplosion(position));
        }

        private IEnumerator SpawnSparkles(Vector2 position, int comboLevel)
        {
            int count = sparkleCount + (comboLevel - 1) * 4; // Más chispas con combo
            float size = sparkleSize + (comboLevel - 1) * 3f; // Más grandes con combo

            for (int i = 0; i < count; i++)
            {
                CreateSparkle(position, size, comboLevel);

                if (i % 3 == 0)
                    yield return null; // Distribuir creación
            }
        }

        private IEnumerator SpawnSparklesWithColors(Vector2 position, Color[] colors, int count, float lifetime)
        {
            for (int i = 0; i < count; i++)
            {
                CreateSparkleWithColor(position, sparkleSize, colors[Random.Range(0, colors.Length)], lifetime);

                if (i % 3 == 0)
                    yield return null;
            }
        }

        private void CreateSparkle(Vector2 position, float size, int comboLevel)
        {
            GameObject sparkle = new GameObject("Sparkle");
            sparkle.transform.SetParent(transform, false);

            RectTransform rt = sparkle.AddComponent<RectTransform>();
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(size, size);

            Image img = sparkle.AddComponent<Image>();

            // Color basado en combo
            Color baseColor = sparkleColors[Random.Range(0, sparkleColors.Length)];
            if (comboLevel > 1)
            {
                // Más brillante y dorado con combo alto
                baseColor = Color.Lerp(baseColor, new Color(1f, 0.9f, 0.3f, 1f), (comboLevel - 1) * 0.2f);
            }
            img.color = baseColor;

            // Dirección aleatoria
            float angle = Random.Range(0f, sparkleSpread) * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            float speed = sparkleSpeed * Random.Range(0.5f, 1.5f);

            StartCoroutine(AnimateSparkle(sparkle, rt, img, direction, speed, sparkleLifetime));
        }

        private void CreateSparkleWithColor(Vector2 position, float size, Color color, float lifetime)
        {
            GameObject sparkle = new GameObject("Sparkle");
            sparkle.transform.SetParent(transform, false);

            RectTransform rt = sparkle.AddComponent<RectTransform>();
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(size, size);

            Image img = sparkle.AddComponent<Image>();
            img.color = color;

            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            float speed = sparkleSpeed * Random.Range(0.5f, 1.2f);

            StartCoroutine(AnimateSparkle(sparkle, rt, img, direction, speed, lifetime));
        }

        private IEnumerator AnimateSparkle(GameObject sparkle, RectTransform rt, Image img, Vector2 direction, float speed, float lifetime)
        {
            float elapsed = 0f;
            Vector2 startPos = rt.anchoredPosition;
            Color startColor = img.color;
            Vector2 startSize = rt.sizeDelta;

            while (elapsed < lifetime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / lifetime;

                // Movimiento con desaceleración
                float moveT = 1f - Mathf.Pow(1f - t, 2f);
                rt.anchoredPosition = startPos + direction * speed * moveT * lifetime;

                // Fade out y shrink
                float alpha = 1f - Mathf.Pow(t, 2f);
                img.color = new Color(startColor.r, startColor.g, startColor.b, startColor.a * alpha);

                float scale = 1f - t * 0.5f;
                rt.sizeDelta = startSize * scale;

                // Rotación
                rt.Rotate(0, 0, Time.deltaTime * 360f);

                yield return null;
            }

            Destroy(sparkle);
        }

        private IEnumerator SpawnConfetti()
        {
            // Confeti cayendo desde arriba
            float screenWidth = parentCanvas != null ? ((RectTransform)parentCanvas.transform).rect.width : 1080f;
            float screenHeight = parentCanvas != null ? ((RectTransform)parentCanvas.transform).rect.height : 1920f;

            for (int i = 0; i < confettiCount; i++)
            {
                float x = Random.Range(-screenWidth * 0.5f, screenWidth * 0.5f);
                float y = screenHeight * 0.6f;

                CreateConfetti(new Vector2(x, y), false);

                yield return new WaitForSeconds(0.03f);
            }
        }

        private IEnumerator SpawnConfettiExplosion(Vector2 position)
        {
            for (int i = 0; i < confettiCount; i++)
            {
                CreateConfetti(position, true);

                if (i % 5 == 0)
                    yield return null;
            }
        }

        private void CreateConfetti(Vector2 position, bool isExplosion)
        {
            GameObject confetti = new GameObject("Confetti");
            confetti.transform.SetParent(transform, false);

            RectTransform rt = confetti.AddComponent<RectTransform>();
            rt.anchoredPosition = position;

            // Tamaño rectangular aleatorio
            float w = confettiSize * Random.Range(0.5f, 1f);
            float h = confettiSize * Random.Range(1f, 2f);
            rt.sizeDelta = new Vector2(w, h);
            rt.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

            Image img = confetti.AddComponent<Image>();
            img.color = confettiColors[Random.Range(0, confettiColors.Length)];

            Vector2 direction;
            float speed;

            if (isExplosion)
            {
                // Explosión radial
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                speed = confettiSpeed * Random.Range(0.5f, 1.5f);
            }
            else
            {
                // Caída con movimiento lateral
                direction = new Vector2(Random.Range(-0.3f, 0.3f), -1f).normalized;
                speed = confettiSpeed * Random.Range(0.3f, 0.8f);
            }

            StartCoroutine(AnimateConfetti(confetti, rt, img, direction, speed));
        }

        private IEnumerator AnimateConfetti(GameObject confetti, RectTransform rt, Image img, Vector2 initialDirection, float speed)
        {
            float elapsed = 0f;
            Vector2 position = rt.anchoredPosition;
            Vector2 velocity = initialDirection * speed;
            Color startColor = img.color;
            float rotationSpeed = Random.Range(-720f, 720f);
            float wobblePhase = Random.Range(0f, Mathf.PI * 2f);
            float wobbleSpeed = Random.Range(3f, 6f);

            while (elapsed < confettiLifetime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / confettiLifetime;

                // Aplicar gravedad
                velocity.y -= confettiGravity * Time.deltaTime;

                // Wobble horizontal
                float wobble = Mathf.Sin(elapsed * wobbleSpeed + wobblePhase) * 50f * Time.deltaTime;
                velocity.x += wobble;

                // Actualizar posición
                position += velocity * Time.deltaTime;
                rt.anchoredPosition = position;

                // Rotación
                rt.Rotate(0, 0, rotationSpeed * Time.deltaTime);

                // Fade out al final
                if (t > 0.7f)
                {
                    float fadeT = (t - 0.7f) / 0.3f;
                    img.color = new Color(startColor.r, startColor.g, startColor.b, startColor.a * (1f - fadeT));
                }

                yield return null;
            }

            Destroy(confetti);
        }

        /// <summary>
        /// Efecto de estrellas girando (para combo alto)
        /// </summary>
        public void PlayStarBurst(Vector2 position, int starCount = 5)
        {
            StartCoroutine(SpawnStars(position, starCount));
        }

        private IEnumerator SpawnStars(Vector2 position, int count)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = (360f / count) * i;
                CreateStar(position, angle);
            }
            yield return null;
        }

        private void CreateStar(Vector2 position, float startAngle)
        {
            GameObject star = new GameObject("Star");
            star.transform.SetParent(transform, false);

            RectTransform rt = star.AddComponent<RectTransform>();
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(25f, 25f);

            Image img = star.AddComponent<Image>();
            img.color = new Color(1f, 0.95f, 0.3f, 1f); // Dorado

            StartCoroutine(AnimateStar(star, rt, img, startAngle));
        }

        private IEnumerator AnimateStar(GameObject star, RectTransform rt, Image img, float startAngle)
        {
            float duration = 0.5f;
            float elapsed = 0f;
            Vector2 center = rt.anchoredPosition;
            float radius = 0f;
            float maxRadius = 80f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Expandir en espiral
                radius = maxRadius * EaseOutQuad(t);
                float currentAngle = (startAngle + t * 180f) * Mathf.Deg2Rad;

                rt.anchoredPosition = center + new Vector2(
                    Mathf.Cos(currentAngle) * radius,
                    Mathf.Sin(currentAngle) * radius
                );

                // Scale y fade
                float scale = 1f + t * 0.5f;
                rt.sizeDelta = new Vector2(25f * scale, 25f * scale);

                float alpha = 1f - Mathf.Pow(t, 2f);
                img.color = new Color(1f, 0.95f, 0.3f, alpha);

                // Rotación
                rt.Rotate(0, 0, Time.deltaTime * 540f);

                yield return null;
            }

            Destroy(star);
        }

        private float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);

        /// <summary>
        /// Limpia todas las partículas activas
        /// </summary>
        public void ClearAllParticles()
        {
            foreach (Transform child in transform)
            {
                if (child.name.Contains("Sparkle") || child.name.Contains("Confetti") || child.name.Contains("Star"))
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }
}
