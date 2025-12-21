using UnityEngine;
using System.Collections.Generic;

namespace DigitPark.Effects
{
    public enum ParticleType
    {
        NeonBurst,
        NeonBurstLarge,
        SuccessBurst,
        ErrorBurst,
        Ripple,
        Sparkle,
        ComboBurst
    }

    /// <summary>
    /// Gestor de sistemas de particulas - Crea efectos visuales satisfactorios
    /// </summary>
    public class ParticleSystemManager : MonoBehaviour
    {
        [Header("Neon Colors")]
        [SerializeField] private Color neonCyan = new Color(0f, 0.9608f, 1f, 1f);
        [SerializeField] private Color neonGold = new Color(1f, 0.84f, 0f, 1f);
        [SerializeField] private Color neonPurple = new Color(0.6157f, 0.2941f, 1f, 1f);
        [SerializeField] private Color neonGreen = new Color(0.2f, 1f, 0.4f, 1f);
        [SerializeField] private Color neonRed = new Color(1f, 0.3f, 0.3f, 1f);

        // Pool de particulas para optimizacion
        private Dictionary<ParticleType, Queue<ParticleSystem>> particlePools;
        private Transform particleContainer;

        private void Awake()
        {
            particlePools = new Dictionary<ParticleType, Queue<ParticleSystem>>();

            // Crear contenedor para particulas
            particleContainer = new GameObject("ParticleContainer").transform;
            particleContainer.SetParent(transform);
        }

        #region Public Methods

        /// <summary>
        /// Reproduce una explosion de particulas en la posicion indicada
        /// </summary>
        public void PlayButtonBurst(Vector3 worldPosition, ParticleType type)
        {
            ParticleSystem ps = GetOrCreateParticleSystem(type);
            ps.transform.position = worldPosition;
            ps.Play();
        }

        /// <summary>
        /// Efecto de onda/ripple
        /// </summary>
        public void PlayRipple(Vector3 worldPosition)
        {
            ParticleSystem ps = GetOrCreateParticleSystem(ParticleType.Ripple);
            ps.transform.position = worldPosition;
            ps.Play();
        }

        /// <summary>
        /// Efecto de combo con intensidad variable
        /// </summary>
        public void PlayComboBurst(Vector3 worldPosition, int comboCount)
        {
            ParticleSystem ps = GetOrCreateParticleSystem(ParticleType.ComboBurst);
            ps.transform.position = worldPosition;

            // Ajustar intensidad segun el combo
            var main = ps.main;
            main.startSize = Mathf.Min(0.5f + comboCount * 0.1f, 1.5f);

            var emission = ps.emission;
            emission.SetBurst(0, new ParticleSystem.Burst(0f, (short)(10 + comboCount * 5)));

            ps.Play();
        }

        /// <summary>
        /// Efecto de chispas continuas (para botones hover)
        /// </summary>
        public ParticleSystem StartSparkles(Vector3 worldPosition)
        {
            ParticleSystem ps = GetOrCreateParticleSystem(ParticleType.Sparkle);
            ps.transform.position = worldPosition;
            ps.Play();
            return ps;
        }

        public void StopSparkles(ParticleSystem ps)
        {
            if (ps != null)
            {
                ps.Stop();
            }
        }

        #endregion

        #region Particle System Creation

        private ParticleSystem GetOrCreateParticleSystem(ParticleType type)
        {
            // Verificar si hay uno disponible en el pool
            if (particlePools.TryGetValue(type, out Queue<ParticleSystem> pool) && pool.Count > 0)
            {
                ParticleSystem pooled = pool.Dequeue();
                if (pooled != null)
                {
                    pooled.gameObject.SetActive(true);
                    return pooled;
                }
            }

            // Crear nuevo
            return CreateParticleSystem(type);
        }

        private ParticleSystem CreateParticleSystem(ParticleType type)
        {
            GameObject psObj = new GameObject($"Particles_{type}");
            psObj.transform.SetParent(particleContainer);

            ParticleSystem ps = psObj.AddComponent<ParticleSystem>();
            ParticleSystemRenderer renderer = psObj.GetComponent<ParticleSystemRenderer>();

            // Configurar segun el tipo
            switch (type)
            {
                case ParticleType.NeonBurst:
                    ConfigureNeonBurst(ps, renderer, false);
                    break;
                case ParticleType.NeonBurstLarge:
                    ConfigureNeonBurst(ps, renderer, true);
                    break;
                case ParticleType.SuccessBurst:
                    ConfigureSuccessBurst(ps, renderer);
                    break;
                case ParticleType.ErrorBurst:
                    ConfigureErrorBurst(ps, renderer);
                    break;
                case ParticleType.Ripple:
                    ConfigureRipple(ps, renderer);
                    break;
                case ParticleType.Sparkle:
                    ConfigureSparkle(ps, renderer);
                    break;
                case ParticleType.ComboBurst:
                    ConfigureComboBurst(ps, renderer);
                    break;
            }

            // Auto-retorno al pool
            var poolReturn = psObj.AddComponent<ParticlePoolReturn>();
            poolReturn.Initialize(this, type);

            return ps;
        }

        private void ConfigureNeonBurst(ParticleSystem ps, ParticleSystemRenderer renderer, bool large)
        {
            var main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = large ? 0.6f : 0.4f;
            main.startSpeed = large ? 8f : 5f;
            main.startSize = large ? 0.15f : 0.1f;
            main.startColor = neonCyan;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;
            main.maxParticles = 50;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0;
            emission.SetBurst(0, new ParticleSystem.Burst(0f, (short)(large ? 30 : 20)));

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.1f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(neonCyan, 0f),
                    new GradientColorKey(neonPurple, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

            // Renderer
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = GetParticleMaterial();
        }

        private void ConfigureSuccessBurst(ParticleSystem ps, ParticleSystemRenderer renderer)
        {
            var main = ps.main;
            main.duration = 0.8f;
            main.loop = false;
            main.startLifetime = 0.8f;
            main.startSpeed = 6f;
            main.startSize = 0.12f;
            main.startColor = neonGreen;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;
            main.maxParticles = 40;
            main.gravityModifier = 0.5f;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0;
            emission.SetBurst(0, new ParticleSystem.Burst(0f, 25));

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.2f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(neonGreen, 0f),
                    new GradientColorKey(neonGold, 0.5f),
                    new GradientColorKey(neonCyan, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0.3f));

            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = GetParticleMaterial();
        }

        private void ConfigureErrorBurst(ParticleSystem ps, ParticleSystemRenderer renderer)
        {
            var main = ps.main;
            main.duration = 0.4f;
            main.loop = false;
            main.startLifetime = 0.3f;
            main.startSpeed = 4f;
            main.startSize = 0.1f;
            main.startColor = neonRed;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;
            main.maxParticles = 20;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0;
            emission.SetBurst(0, new ParticleSystem.Burst(0f, 15));

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.15f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(neonRed, 0f),
                    new GradientColorKey(Color.red, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = GetParticleMaterial();
        }

        private void ConfigureRipple(ParticleSystem ps, ParticleSystemRenderer renderer)
        {
            var main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = 0.5f;
            main.startSpeed = 0f;
            main.startSize = 0.1f;
            main.startColor = neonCyan;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;
            main.maxParticles = 1;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0;
            emission.SetBurst(0, new ParticleSystem.Burst(0f, 1));

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.2f, 1f, 2f));

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(neonCyan, 0f), new GradientColorKey(neonCyan, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = gradient;

            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = GetRippleMaterial();
        }

        private void ConfigureSparkle(ParticleSystem ps, ParticleSystemRenderer renderer)
        {
            var main = ps.main;
            main.duration = 2f;
            main.loop = true;
            main.startLifetime = 0.5f;
            main.startSpeed = 1f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.1f);
            main.startColor = neonGold;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;
            main.maxParticles = 20;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 10;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(1f, 0.5f, 0.1f);

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(neonGold, 0f), new GradientColorKey(neonCyan, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = gradient;

            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = GetParticleMaterial();
        }

        private void ConfigureComboBurst(ParticleSystem ps, ParticleSystemRenderer renderer)
        {
            var main = ps.main;
            main.duration = 1f;
            main.loop = false;
            main.startLifetime = 1f;
            main.startSpeed = 7f;
            main.startSize = 0.15f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;
            main.maxParticles = 60;
            main.gravityModifier = 0.3f;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0;
            emission.SetBurst(0, new ParticleSystem.Burst(0f, 20));

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 45f;
            shape.radius = 0.1f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(neonGold, 0f),
                    new GradientColorKey(neonCyan, 0.5f),
                    new GradientColorKey(neonPurple, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.5f, 1f, 1.5f));

            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = GetParticleMaterial();
        }

        #endregion

        #region Materials

        private Material particleMaterial;
        private Material rippleMaterial;

        private Material GetParticleMaterial()
        {
            if (particleMaterial == null)
            {
                // Crear material basico de particulas
                particleMaterial = new Material(Shader.Find("Particles/Standard Unlit"));
                particleMaterial.SetFloat("_Mode", 2); // Fade
                particleMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                particleMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                particleMaterial.EnableKeyword("_ALPHABLEND_ON");

                // Crear textura de circulo
                Texture2D tex = CreateCircleTexture(64);
                particleMaterial.mainTexture = tex;
            }
            return particleMaterial;
        }

        private Material GetRippleMaterial()
        {
            if (rippleMaterial == null)
            {
                rippleMaterial = new Material(Shader.Find("Particles/Standard Unlit"));
                rippleMaterial.SetFloat("_Mode", 2);
                rippleMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                rippleMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                rippleMaterial.EnableKeyword("_ALPHABLEND_ON");

                // Crear textura de anillo
                Texture2D tex = CreateRingTexture(64);
                rippleMaterial.mainTexture = tex;
            }
            return rippleMaterial;
        }

        private Texture2D CreateCircleTexture(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size / 2f;
            float radius = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float alpha = Mathf.Clamp01(1f - (dist / radius));
                    alpha = Mathf.Pow(alpha, 2f); // Suavizar bordes
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            tex.Apply();
            return tex;
        }

        private Texture2D CreateRingTexture(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size / 2f;
            float outerRadius = size / 2f;
            float innerRadius = size / 2f * 0.7f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float alpha = 0f;

                    if (dist < outerRadius && dist > innerRadius)
                    {
                        float ringCenter = (outerRadius + innerRadius) / 2f;
                        float ringWidth = outerRadius - innerRadius;
                        alpha = 1f - Mathf.Abs(dist - ringCenter) / (ringWidth / 2f);
                    }

                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            tex.Apply();
            return tex;
        }

        #endregion

        #region Pool Management

        public void ReturnToPool(ParticleSystem ps, ParticleType type)
        {
            if (!particlePools.ContainsKey(type))
            {
                particlePools[type] = new Queue<ParticleSystem>();
            }

            ps.gameObject.SetActive(false);
            particlePools[type].Enqueue(ps);
        }

        #endregion
    }

    /// <summary>
    /// Componente auxiliar para retornar particulas al pool
    /// </summary>
    public class ParticlePoolReturn : MonoBehaviour
    {
        private ParticleSystemManager manager;
        private ParticleType type;
        private ParticleSystem ps;

        public void Initialize(ParticleSystemManager manager, ParticleType type)
        {
            this.manager = manager;
            this.type = type;
            this.ps = GetComponent<ParticleSystem>();
        }

        private void OnParticleSystemStopped()
        {
            manager?.ReturnToPool(ps, type);
        }

        private void Update()
        {
            // Verificar si termino de reproducir
            if (ps != null && !ps.isPlaying && !ps.isEmitting && ps.particleCount == 0)
            {
                manager?.ReturnToPool(ps, type);
            }
        }
    }
}
