using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitPark.Controllers
{
    /// <summary>
    /// Controlador de efectos visuales y partículas
    /// Maneja explosiones, trails, confetti y otros efectos del juego
    /// </summary>
    public class EffectsController : MonoBehaviour
    {
        [Header("Particle Prefabs")]
        [SerializeField] private ParticleSystem correctTouchParticles;
        [SerializeField] private ParticleSystem wrongTouchParticles;
        [SerializeField] private ParticleSystem completionParticles;
        [SerializeField] private ParticleSystem newRecordParticles;
        [SerializeField] private ParticleSystem confettiParticles;

        [Header("Trail Effect")]
        [SerializeField] private TrailRenderer fingerTrail;
        [SerializeField] private float trailTime = 0.3f;

        [Header("Glow Effects")]
        [SerializeField] private GameObject glowPrefab;
        [SerializeField] private float glowDuration = 0.5f;

        [Header("Object Pool")]
        [SerializeField] private int poolSize = 20;

        // Object pools
        private Queue<ParticleSystem> correctParticlePool;
        private Queue<ParticleSystem> wrongParticlePool;
        private Queue<GameObject> glowPool;

        // Trail
        private TrailRenderer activeTrail;
        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;

            // Inicializar object pools
            InitializeObjectPools();

            // Configurar trail
            if (fingerTrail != null)
            {
                fingerTrail.time = trailTime;
                fingerTrail.emitting = false;
            }
        }

        private void Update()
        {
            // Actualizar trail si está activo
            UpdateFingerTrail();
        }

        #region Object Pooling

        /// <summary>
        /// Inicializa los object pools para mejor rendimiento
        /// </summary>
        private void InitializeObjectPools()
        {
            correctParticlePool = new Queue<ParticleSystem>();
            wrongParticlePool = new Queue<ParticleSystem>();
            glowPool = new Queue<GameObject>();

            // Pre-instanciar partículas correctas
            if (correctTouchParticles != null)
            {
                for (int i = 0; i < poolSize / 2; i++)
                {
                    ParticleSystem ps = Instantiate(correctTouchParticles, transform);
                    ps.gameObject.SetActive(false);
                    correctParticlePool.Enqueue(ps);
                }
            }

            // Pre-instanciar partículas incorrectas
            if (wrongTouchParticles != null)
            {
                for (int i = 0; i < poolSize / 2; i++)
                {
                    ParticleSystem ps = Instantiate(wrongTouchParticles, transform);
                    ps.gameObject.SetActive(false);
                    wrongParticlePool.Enqueue(ps);
                }
            }

            // Pre-instanciar glows
            if (glowPrefab != null)
            {
                for (int i = 0; i < poolSize; i++)
                {
                    GameObject glow = Instantiate(glowPrefab, transform);
                    glow.SetActive(false);
                    glowPool.Enqueue(glow);
                }
            }

            Debug.Log("[Effects] Object pools inicializados");
        }

        /// <summary>
        /// Obtiene una partícula del pool
        /// </summary>
        private ParticleSystem GetParticleFromPool(Queue<ParticleSystem> pool, ParticleSystem prefab)
        {
            if (pool.Count > 0)
            {
                ParticleSystem ps = pool.Dequeue();
                ps.gameObject.SetActive(true);
                return ps;
            }
            else
            {
                // Si el pool está vacío, crear uno nuevo
                ParticleSystem ps = Instantiate(prefab, transform);
                return ps;
            }
        }

        /// <summary>
        /// Devuelve una partícula al pool
        /// </summary>
        private void ReturnParticleToPool(ParticleSystem ps, Queue<ParticleSystem> pool)
        {
            ps.Stop();
            ps.Clear();
            ps.gameObject.SetActive(false);
            pool.Enqueue(ps);
        }

        /// <summary>
        /// Obtiene un glow del pool
        /// </summary>
        private GameObject GetGlowFromPool()
        {
            if (glowPool.Count > 0)
            {
                GameObject glow = glowPool.Dequeue();
                glow.SetActive(true);
                return glow;
            }
            else if (glowPrefab != null)
            {
                return Instantiate(glowPrefab, transform);
            }

            return null;
        }

        /// <summary>
        /// Devuelve un glow al pool
        /// </summary>
        private void ReturnGlowToPool(GameObject glow)
        {
            glow.SetActive(false);
            glowPool.Enqueue(glow);
        }

        #endregion

        #region Particle Effects

        /// <summary>
        /// Reproduce efecto de toque correcto
        /// </summary>
        public void PlayCorrectEffect(Vector3 position)
        {
            Debug.Log($"[Effects] Efecto correcto en {position}");

            // Partículas verdes brillantes
            if (correctTouchParticles != null)
            {
                ParticleSystem ps = GetParticleFromPool(correctParticlePool, correctTouchParticles);
                ps.transform.position = position;
                ps.Play();

                StartCoroutine(ReturnParticleAfterPlay(ps, correctParticlePool));
            }

            // Glow effect
            PlayGlowEffect(position, Color.green);
        }

        /// <summary>
        /// Reproduce efecto de toque incorrecto
        /// </summary>
        public void PlayWrongEffect(Vector3 position)
        {
            Debug.Log($"[Effects] Efecto incorrecto en {position}");

            // Partículas rojas
            if (wrongTouchParticles != null)
            {
                ParticleSystem ps = GetParticleFromPool(wrongParticlePool, wrongTouchParticles);
                ps.transform.position = position;
                ps.Play();

                StartCoroutine(ReturnParticleAfterPlay(ps, wrongParticlePool));
            }

            // Glow effect rojo
            PlayGlowEffect(position, Color.red);
        }

        /// <summary>
        /// Reproduce efecto de completar el juego
        /// </summary>
        public void PlayCompletionEffect()
        {
            Debug.Log("[Effects] Efecto de completación");

            // Confetti desde arriba
            if (confettiParticles != null)
            {
                confettiParticles.transform.position = new Vector3(0, 5, 0);
                confettiParticles.Play();
            }

            // Explosión de partículas
            if (completionParticles != null)
            {
                completionParticles.Play();
            }

            // Screen flash
            StartCoroutine(ScreenFlash(Color.white, 0.3f));
        }

        /// <summary>
        /// Reproduce efecto de nuevo récord
        /// </summary>
        public void PlayNewRecordEffect()
        {
            Debug.Log("[Effects] Efecto de nuevo récord");

            // Partículas doradas especiales
            if (newRecordParticles != null)
            {
                newRecordParticles.Play();
            }

            // Flash dorado
            StartCoroutine(ScreenFlash(new Color(1f, 0.84f, 0f), 0.5f));

            // Confetti dorado
            if (confettiParticles != null)
            {
                var main = confettiParticles.main;
                main.startColor = new Color(1f, 0.84f, 0f);
                confettiParticles.Play();
            }
        }

        /// <summary>
        /// Devuelve una partícula al pool después de que termine de reproducirse
        /// </summary>
        private IEnumerator ReturnParticleAfterPlay(ParticleSystem ps, Queue<ParticleSystem> pool)
        {
            yield return new WaitForSeconds(ps.main.duration + ps.main.startLifetime.constantMax);
            ReturnParticleToPool(ps, pool);
        }

        #endregion

        #region Glow Effects

        /// <summary>
        /// Reproduce un efecto de brillo/glow
        /// </summary>
        private void PlayGlowEffect(Vector3 position, Color color)
        {
            GameObject glow = GetGlowFromPool();
            if (glow == null) return;

            glow.transform.position = position;

            // Configurar color
            SpriteRenderer sr = glow.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = color;
            }

            // Animar
            StartCoroutine(AnimateGlow(glow));
        }

        /// <summary>
        /// Anima el efecto de glow (fade out y scale)
        /// </summary>
        private IEnumerator AnimateGlow(GameObject glow)
        {
            float elapsed = 0f;
            Vector3 startScale = Vector3.one * 0.5f;
            Vector3 endScale = Vector3.one * 2f;

            SpriteRenderer sr = glow.GetComponent<SpriteRenderer>();

            while (elapsed < glowDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / glowDuration;

                // Scale up
                glow.transform.localScale = Vector3.Lerp(startScale, endScale, t);

                // Fade out
                if (sr != null)
                {
                    Color color = sr.color;
                    color.a = 1f - t;
                    sr.color = color;
                }

                yield return null;
            }

            // Devolver al pool
            ReturnGlowToPool(glow);
        }

        #endregion

        #region Trail Effect

        /// <summary>
        /// Actualiza el trail del dedo
        /// </summary>
        private void UpdateFingerTrail()
        {
            if (fingerTrail == null) return;

            #if UNITY_EDITOR || UNITY_STANDALONE
            // En editor, usar mouse
            if (Input.GetMouseButtonDown(0))
            {
                StartTrail(Input.mousePosition);
            }
            else if (Input.GetMouseButton(0))
            {
                UpdateTrailPosition(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                StopTrail();
            }
            #else
            // En móvil, usar touch
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    StartTrail(touch.position);
                }
                else if (touch.phase == TouchPhase.Moved)
                {
                    UpdateTrailPosition(touch.position);
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    StopTrail();
                }
            }
            #endif
        }

        /// <summary>
        /// Inicia el trail
        /// </summary>
        private void StartTrail(Vector2 screenPosition)
        {
            if (fingerTrail == null) return;

            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));
            fingerTrail.transform.position = worldPos;
            fingerTrail.emitting = true;
            fingerTrail.Clear();
        }

        /// <summary>
        /// Actualiza la posición del trail
        /// </summary>
        private void UpdateTrailPosition(Vector2 screenPosition)
        {
            if (fingerTrail == null || !fingerTrail.emitting) return;

            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));
            fingerTrail.transform.position = worldPos;
        }

        /// <summary>
        /// Detiene el trail
        /// </summary>
        private void StopTrail()
        {
            if (fingerTrail != null)
            {
                fingerTrail.emitting = false;
            }
        }

        #endregion

        #region Screen Effects

        /// <summary>
        /// Flash de pantalla completa
        /// </summary>
        private IEnumerator ScreenFlash(Color flashColor, float duration)
        {
            // Crear un panel de UI temporal que cubra toda la pantalla
            GameObject flashPanel = new GameObject("FlashPanel");
            flashPanel.transform.SetParent(transform);

            Canvas canvas = flashPanel.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            UnityEngine.UI.Image image = flashPanel.AddComponent<UnityEngine.UI.Image>();
            image.color = flashColor;

            RectTransform rt = flashPanel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            // Fade out
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f - (elapsed / duration);
                Color color = image.color;
                color.a = alpha;
                image.color = color;

                yield return null;
            }

            // Destruir
            Destroy(flashPanel);
        }

        /// <summary>
        /// Efecto de vignette pulsante (opcional)
        /// </summary>
        public void PulseVignette(float intensity, float duration)
        {
            StartCoroutine(PulseVignetteCoroutine(intensity, duration));
        }

        private IEnumerator PulseVignetteCoroutine(float intensity, float duration)
        {
            // Aquí iría la lógica para animar el post-processing vignette
            // Requiere el Post Processing Stack de Unity

            yield return new WaitForSeconds(duration);
        }

        #endregion
    }
}
