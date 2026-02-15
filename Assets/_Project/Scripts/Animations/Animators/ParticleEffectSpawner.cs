using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace DigitPark.Animations
{
    /// <summary>
    /// Spawns and manages UI particle effects.
    /// Provides easy access to common particle effects like confetti, sparkles, etc.
    /// </summary>
    public class ParticleEffectSpawner : MonoBehaviour
    {
        public static ParticleEffectSpawner Instance { get; private set; }

        [Header("Particle Prefabs")]
        [SerializeField] private ParticleSystem confettiPrefab;
        [SerializeField] private ParticleSystem sparklesPrefab;
        [SerializeField] private ParticleSystem burstPrefab;
        [SerializeField] private ParticleSystem coinShowerPrefab;
        [SerializeField] private ParticleSystem starExplosionPrefab;
        [SerializeField] private ParticleSystem smokePuffPrefab;
        [SerializeField] private ParticleSystem glowPrefab;

        [Header("Settings")]
        [SerializeField] private Transform particlesParent;
        [SerializeField] private int maxParticleSystems = 20;

        private Queue<ParticleSystem> activeParticles = new Queue<ParticleSystem>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                if (transform.parent != null)
                    transform.SetParent(null);
                DontDestroyOnLoad(gameObject);

                if (particlesParent == null)
                    particlesParent = transform;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // ==================== SPAWN METHODS ====================

        /// <summary>
        /// Spawn confetti at position
        /// </summary>
        public ParticleSystem SpawnConfetti(Vector3 position)
        {
            return SpawnEffect(confettiPrefab, position);
        }

        /// <summary>
        /// Spawn sparkles at position
        /// </summary>
        public ParticleSystem SpawnSparkles(Vector3 position)
        {
            return SpawnEffect(sparklesPrefab, position);
        }

        /// <summary>
        /// Spawn burst effect at position
        /// </summary>
        public ParticleSystem SpawnBurst(Vector3 position, Color? color = null)
        {
            ParticleSystem ps = SpawnEffect(burstPrefab, position);

            if (ps != null && color.HasValue)
            {
                var main = ps.main;
                main.startColor = color.Value;
            }

            return ps;
        }

        /// <summary>
        /// Spawn coin shower
        /// </summary>
        public ParticleSystem SpawnCoinShower(Vector3 position)
        {
            return SpawnEffect(coinShowerPrefab, position);
        }

        /// <summary>
        /// Spawn star explosion
        /// </summary>
        public ParticleSystem SpawnStarExplosion(Vector3 position)
        {
            return SpawnEffect(starExplosionPrefab, position);
        }

        /// <summary>
        /// Spawn smoke puff
        /// </summary>
        public ParticleSystem SpawnSmokePuff(Vector3 position)
        {
            return SpawnEffect(smokePuffPrefab, position);
        }

        /// <summary>
        /// Spawn glow effect
        /// </summary>
        public ParticleSystem SpawnGlow(Vector3 position, Color? color = null)
        {
            ParticleSystem ps = SpawnEffect(glowPrefab, position);

            if (ps != null && color.HasValue)
            {
                var main = ps.main;
                main.startColor = color.Value;
            }

            return ps;
        }

        // ==================== SCREEN EFFECTS ====================

        /// <summary>
        /// Spawn confetti shower from top of screen
        /// </summary>
        public void SpawnScreenConfetti()
        {
            Vector3 topCenter = new Vector3(Screen.width / 2f, Screen.height + 50f, 0f);
            SpawnConfetti(topCenter);
        }

        /// <summary>
        /// Spawn burst at screen center
        /// </summary>
        public void SpawnCenterBurst(Color? color = null)
        {
            Vector3 center = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
            SpawnBurst(center, color);
        }

        /// <summary>
        /// Spawn multiple bursts in sequence
        /// </summary>
        public void SpawnFireworks(int count = 5, float delay = 0.3f)
        {
            StartCoroutine(FireworksCoroutine(count, delay));
        }

        private System.Collections.IEnumerator FireworksCoroutine(int count, float delay)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 randomPos = new Vector3(
                    Random.Range(Screen.width * 0.2f, Screen.width * 0.8f),
                    Random.Range(Screen.height * 0.3f, Screen.height * 0.7f),
                    0f
                );

                Color randomColor = Random.ColorHSV(0f, 1f, 0.8f, 1f, 0.8f, 1f);
                SpawnBurst(randomPos, randomColor);

                yield return new WaitForSeconds(delay);
            }
        }

        // ==================== UI ELEMENT EFFECTS ====================

        /// <summary>
        /// Spawn sparkles around UI element
        /// </summary>
        public void SpawnSparklesAroundElement(RectTransform element)
        {
            if (element == null) return;

            Vector3 pos = element.position;
            SpawnSparkles(pos);
        }

        /// <summary>
        /// Spawn burst on button press
        /// </summary>
        public void SpawnButtonPressBurst(RectTransform button, Color? color = null)
        {
            if (button == null) return;

            Vector3 pos = button.position;
            SpawnBurst(pos, color);
        }

        /// <summary>
        /// Spawn trail effect following element
        /// </summary>
        public ParticleSystem SpawnTrailEffect(Transform target)
        {
            ParticleSystem ps = SpawnEffect(sparklesPrefab, target.position);

            if (ps != null)
            {
                // Make it follow the target
                ps.transform.SetParent(target);
                ps.transform.localPosition = Vector3.zero;

                var emission = ps.emission;
                emission.rateOverTime = 20f;
            }

            return ps;
        }

        // ==================== REWARD EFFECTS ====================

        /// <summary>
        /// Play coin collect effect
        /// </summary>
        public void PlayCoinCollectEffect(Vector3 position)
        {
            SpawnBurst(position, new Color(1f, 0.85f, 0f)); // Gold color
            SpawnSparkles(position);
        }

        /// <summary>
        /// Play gem collect effect
        /// </summary>
        public void PlayGemCollectEffect(Vector3 position)
        {
            SpawnBurst(position, new Color(0.5f, 0f, 1f)); // Purple color
            SpawnSparkles(position);
        }

        /// <summary>
        /// Play victory celebration
        /// </summary>
        public void PlayVictoryCelebration()
        {
            SpawnScreenConfetti();
            SpawnFireworks(7, 0.2f);
        }

        /// <summary>
        /// Play level up effect
        /// </summary>
        public void PlayLevelUpEffect(Vector3 position)
        {
            SpawnStarExplosion(position);
            SpawnBurst(position, new Color(1f, 0.9f, 0.3f));
        }

        // ==================== CORE SPAWN METHOD ====================

        private ParticleSystem SpawnEffect(ParticleSystem prefab, Vector3 position)
        {
            if (prefab == null) return null;

            // Manage pool size
            while (activeParticles.Count >= maxParticleSystems)
            {
                ParticleSystem oldest = activeParticles.Dequeue();
                if (oldest != null)
                    Destroy(oldest.gameObject);
            }

            // Instantiate
            ParticleSystem ps = Instantiate(prefab, position, Quaternion.identity, particlesParent);
            ps.gameObject.SetActive(true);
            ps.Play();

            activeParticles.Enqueue(ps);

            // Auto destroy
            float lifetime = ps.main.duration + ps.main.startLifetime.constantMax;
            Destroy(ps.gameObject, lifetime + 0.5f);

            return ps;
        }

        // ==================== CUSTOM EFFECTS ====================

        /// <summary>
        /// Create custom burst with specific settings
        /// </summary>
        public ParticleSystem CreateCustomBurst(Vector3 position, Color color, int particleCount, float size, float speed)
        {
            if (burstPrefab == null) return null;

            ParticleSystem ps = Instantiate(burstPrefab, position, Quaternion.identity, particlesParent);

            var main = ps.main;
            main.startColor = color;
            main.startSize = size;
            main.startSpeed = speed;

            var emission = ps.emission;
            var burst = new ParticleSystem.Burst(0f, particleCount);
            emission.SetBurst(0, burst);

            ps.Play();

            float lifetime = main.duration + main.startLifetime.constantMax;
            Destroy(ps.gameObject, lifetime + 0.5f);

            return ps;
        }

        // ==================== UTILITY ====================

        /// <summary>
        /// Stop all active particle systems
        /// </summary>
        public void StopAllParticles()
        {
            while (activeParticles.Count > 0)
            {
                ParticleSystem ps = activeParticles.Dequeue();
                if (ps != null)
                {
                    ps.Stop();
                    Destroy(ps.gameObject, 1f);
                }
            }
        }

        /// <summary>
        /// Set particles parent canvas
        /// </summary>
        public void SetParticlesParent(Transform parent)
        {
            particlesParent = parent;
        }

        private void OnDestroy()
        {
            StopAllParticles();
        }
    }
}
