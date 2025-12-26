using UnityEngine;
using System.Collections;

namespace DigitPark.Effects
{
    public enum CelebrationType
    {
        Small,
        Big,
        Epic
    }

    /// <summary>
    /// Gestor de celebraciones - Confetti, fuegos artificiales, efectos epicos
    /// Para momentos de maxima dopamina!
    /// </summary>
    public class CelebrationManager : MonoBehaviour
    {
        [Header("Colors")]
        [SerializeField] private Color[] confettiColors = new Color[]
        {
            new Color(0f, 0.9608f, 1f, 1f),    // Cyan
            new Color(1f, 0.84f, 0f, 1f),       // Gold
            new Color(0.6157f, 0.2941f, 1f, 1f), // Purple
            new Color(0.2f, 1f, 0.4f, 1f),      // Green
            new Color(1f, 0.4f, 0.7f, 1f),      // Pink
            new Color(1f, 0.5f, 0.2f, 1f)       // Orange
        };

        private Transform celebrationContainer;
        private Camera mainCamera;

        private void Awake()
        {
            celebrationContainer = new GameObject("CelebrationContainer").transform;
            celebrationContainer.SetParent(transform);
            mainCamera = Camera.main;
        }

        #region Confetti

        public void PlayConfetti(CelebrationType type)
        {
            int burstCount = type switch
            {
                CelebrationType.Small => 50,
                CelebrationType.Big => 150,
                CelebrationType.Epic => 300,
                _ => 50
            };

            float duration = type switch
            {
                CelebrationType.Small => 2f,
                CelebrationType.Big => 4f,
                CelebrationType.Epic => 6f,
                _ => 2f
            };

            StartCoroutine(SpawnConfetti(burstCount, duration, type == CelebrationType.Epic));
        }

        private IEnumerator SpawnConfetti(int count, float duration, bool continuous)
        {
            ParticleSystem confettiPS = CreateConfettiSystem(count, duration, continuous);
            confettiPS.Play();

            yield return new WaitForSeconds(duration + 2f);

            if (confettiPS != null)
            {
                Destroy(confettiPS.gameObject);
            }
        }

        private ParticleSystem CreateConfettiSystem(int burstCount, float duration, bool continuous)
        {
            GameObject confettiObj = new GameObject("Confetti");
            confettiObj.transform.SetParent(celebrationContainer);

            // Posicionar arriba de la pantalla
            if (mainCamera != null)
            {
                Vector3 topCenter = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 1.2f, 10f));
                confettiObj.transform.position = topCenter;
            }

            ParticleSystem ps = confettiObj.AddComponent<ParticleSystem>();
            ParticleSystemRenderer renderer = confettiObj.GetComponent<ParticleSystemRenderer>();

            // Main module
            var main = ps.main;
            main.duration = duration;
            main.loop = continuous;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2f, 4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;
            main.maxParticles = 500;
            main.gravityModifier = 0.5f;

            // Start color - random from confetti colors
            main.startColor = new ParticleSystem.MinMaxGradient(confettiColors[0], confettiColors[confettiColors.Length - 1]);

            // Emission
            var emission = ps.emission;
            emission.enabled = true;
            if (continuous)
            {
                emission.rateOverTime = burstCount / duration;
            }
            else
            {
                emission.rateOverTime = 0;
                emission.SetBurst(0, new ParticleSystem.Burst(0f, (short)burstCount));
            }

            // Shape - wide area at top
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(15f, 0.5f, 1f);

            // Velocity over lifetime - flutter effect
            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            // Todas las curvas deben estar en el mismo modo (TwoConstants)
            velocity.x = new ParticleSystem.MinMaxCurve(-1f, 1f);
            velocity.y = new ParticleSystem.MinMaxCurve(-2f, -0.5f);
            velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            // Rotation over lifetime - spinning
            var rotation = ps.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-180f * Mathf.Deg2Rad, 180f * Mathf.Deg2Rad);

            // Color over lifetime
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 0.8f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            // Renderer
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = CreateConfettiMaterial();

            return ps;
        }

        #endregion

        #region Fireworks

        public void PlayFireworks()
        {
            StartCoroutine(FireworksSequence());
        }

        private IEnumerator FireworksSequence()
        {
            int fireworkCount = 5;

            for (int i = 0; i < fireworkCount; i++)
            {
                // Posicion aleatoria en la pantalla
                float x = Random.Range(0.2f, 0.8f);
                float y = Random.Range(0.4f, 0.7f);

                if (mainCamera != null)
                {
                    Vector3 worldPos = mainCamera.ViewportToWorldPoint(new Vector3(x, y, 10f));
                    CreateFirework(worldPos, confettiColors[Random.Range(0, confettiColors.Length)]);
                }

                yield return new WaitForSeconds(Random.Range(0.2f, 0.5f));
            }
        }

        private void CreateFirework(Vector3 position, Color color)
        {
            GameObject fwObj = new GameObject("Firework");
            fwObj.transform.SetParent(celebrationContainer);
            fwObj.transform.position = position;

            ParticleSystem ps = fwObj.AddComponent<ParticleSystem>();
            ParticleSystemRenderer renderer = fwObj.GetComponent<ParticleSystemRenderer>();

            var main = ps.main;
            main.duration = 0.1f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 10f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;
            main.maxParticles = 100;
            main.gravityModifier = 0.5f;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0;
            emission.SetBurst(0, new ParticleSystem.Burst(0f, 60));

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(color, 0f),
                    new GradientColorKey(Color.white, 0.3f),
                    new GradientColorKey(color, 1f)
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
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0.2f));

            var trails = ps.trails;
            trails.enabled = true;
            trails.lifetime = 0.2f;
            trails.widthOverTrail = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));
            trails.colorOverLifetime = new ParticleSystem.MinMaxGradient(color);
            trails.inheritParticleColor = true;

            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = CreateParticleMaterial();
            renderer.trailMaterial = CreateTrailMaterial(color);

            ps.Play();

            // Auto destruir
            Destroy(fwObj, 3f);
        }

        #endregion

        #region Star Burst

        public void PlayStarBurst()
        {
            if (mainCamera == null) return;

            Vector3 center = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
            CreateStarBurst(center);
        }

        private void CreateStarBurst(Vector3 position)
        {
            GameObject starObj = new GameObject("StarBurst");
            starObj.transform.SetParent(celebrationContainer);
            starObj.transform.position = position;

            ParticleSystem ps = starObj.AddComponent<ParticleSystem>();
            ParticleSystemRenderer renderer = starObj.GetComponent<ParticleSystemRenderer>();

            var main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = 1f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(8f, 15f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
            main.startColor = new Color(1f, 0.84f, 0f, 1f); // Gold
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;
            main.maxParticles = 50;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0;
            emission.SetBurst(0, new ParticleSystem.Burst(0f, 30));

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(new Color(1f, 0.84f, 0f), 0.3f),
                    new GradientColorKey(new Color(1f, 0.5f, 0f), 1f)
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
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 0.5f);
            sizeCurve.AddKey(0.2f, 1.2f);
            sizeCurve.AddKey(1f, 0f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = CreateStarMaterial();

            ps.Play();

            Destroy(starObj, 3f);
        }

        #endregion

        #region Materials

        private Material CreateConfettiMaterial()
        {
            Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
            mat.SetFloat("_Mode", 2);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.EnableKeyword("_ALPHABLEND_ON");

            // Textura de rectangulo (confetti)
            Texture2D tex = CreateRectangleTexture(32, 48);
            mat.mainTexture = tex;

            return mat;
        }

        private Material CreateParticleMaterial()
        {
            Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
            mat.SetFloat("_Mode", 2);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.EnableKeyword("_ALPHABLEND_ON");

            Texture2D tex = CreateCircleTexture(64);
            mat.mainTexture = tex;

            return mat;
        }

        private Material CreateTrailMaterial(Color color)
        {
            Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
            mat.SetFloat("_Mode", 2);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.color = color;

            return mat;
        }

        private Material CreateStarMaterial()
        {
            Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
            mat.SetFloat("_Mode", 2);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.EnableKeyword("_ALPHABLEND_ON");

            Texture2D tex = CreateStarTexture(64);
            mat.mainTexture = tex;

            return mat;
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
                    alpha = Mathf.Pow(alpha, 2f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            tex.Apply();
            return tex;
        }

        private Texture2D CreateRectangleTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    tex.SetPixel(x, y, Color.white);
                }
            }

            tex.Apply();
            return tex;
        }

        private Texture2D CreateStarTexture(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size / 2f;

            // Limpiar
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, 0f));
                }
            }

            // Dibujar estrella de 4 puntas con glow
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x - center) / center;
                    float dy = Mathf.Abs(y - center) / center;

                    // Crear forma de estrella
                    float star = Mathf.Max(
                        1f - (dx + dy),
                        1f - Mathf.Sqrt(dx * dx + dy * dy) * 1.5f
                    );

                    star = Mathf.Clamp01(star);
                    star = Mathf.Pow(star, 0.5f);

                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, star));
                }
            }

            tex.Apply();
            return tex;
        }

        #endregion
    }
}
