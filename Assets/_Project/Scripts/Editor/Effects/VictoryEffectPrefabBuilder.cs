using UnityEngine;
using UnityEditor;
using DigitPark.Effects;

namespace DigitPark.Editor
{
    /// <summary>
    /// Genera los 8 prefabs de efectos de victoria en Resources/Effects/.
    /// Cada prefab tiene un ParticleSystem configurado + VictoryEffectPlayer.
    /// Menu: DigitPark/Effects/Build All Victory Effect Prefabs
    /// </summary>
    public static class VictoryEffectPrefabBuilder
    {
        private const string OUTPUT_PATH = "Assets/_Project/Resources/Effects";

        private static readonly string[] EFFECT_NAMES = {
            "Confetti", "Fireworks", "Lightning", "GoldRain",
            "NeonExplosion", "Rainbow", "CrownDrop", "FireRing"
        };

        [MenuItem("DigitPark/Effects/Build All Victory Effect Prefabs")]
        public static void BuildAll()
        {
            EnsureDirectory();

            foreach (var name in EFFECT_NAMES)
            {
                BuildEffect(name);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[VictoryEffectPrefabBuilder] {EFFECT_NAMES.Length} prefabs generados en {OUTPUT_PATH}");
        }

        private static void EnsureDirectory()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Resources"))
                AssetDatabase.CreateFolder("Assets/_Project", "Resources");
            if (!AssetDatabase.IsValidFolder(OUTPUT_PATH))
                AssetDatabase.CreateFolder("Assets/_Project/Resources", "Effects");
        }

        private static void BuildEffect(string effectName)
        {
            string prefabPath = $"{OUTPUT_PATH}/{effectName}.prefab";

            // Cargar prefab existente o crear nuevo
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            GameObject root;

            if (existing != null)
            {
                root = (GameObject)PrefabUtility.InstantiatePrefab(existing);
            }
            else
            {
                root = new GameObject(effectName);
            }

            // Asegurar VictoryEffectPlayer
            if (root.GetComponent<VictoryEffectPlayer>() == null)
                root.AddComponent<VictoryEffectPlayer>();

            // Configurar ParticleSystem segun el tipo
            switch (effectName)
            {
                case "Confetti": SetupConfetti(root); break;
                case "Fireworks": SetupFireworks(root); break;
                case "Lightning": SetupLightning(root); break;
                case "GoldRain": SetupGoldRain(root); break;
                case "NeonExplosion": SetupNeonExplosion(root); break;
                case "Rainbow": SetupRainbow(root); break;
                case "CrownDrop": SetupCrownDrop(root); break;
                case "FireRing": SetupFireRing(root); break;
            }

            // Guardar como prefab
            if (existing != null)
            {
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Object.DestroyImmediate(root);
            }
            else
            {
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Object.DestroyImmediate(root);
            }

            Debug.Log($"[VictoryEffectPrefabBuilder] Prefab generado: {effectName}");
        }

        // ==================== SHADER HELPER ====================

        private static Shader GetParticleShader()
        {
            return Shader.Find("Particles/Standard Unlit")
                ?? Shader.Find("Particles/Alpha Blended")
                ?? Shader.Find("Sprites/Default");
        }

        private static Material CreateParticleMaterial(bool additive = false)
        {
            var shader = GetParticleShader();
            if (shader == null) return new Material(Shader.Find("Hidden/InternalErrorShader"));
            Material mat = new Material(shader);
            mat.SetFloat("_Mode", 2);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", additive
                ? (int)UnityEngine.Rendering.BlendMode.One
                : (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.EnableKeyword("_ALPHABLEND_ON");
            return mat;
        }

        private static Texture2D CreateCircleTexture(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size / 2f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float alpha = Mathf.Clamp01(1f - (dist / center));
                    alpha = Mathf.Pow(alpha, 2f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateRectTexture(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    tex.SetPixel(x, y, Color.white);
            tex.Apply();
            return tex;
        }

        private static void SetFadeOverLifetime(ParticleSystem ps)
        {
            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.7f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = g;
        }

        private static ParticleSystem GetOrAddPS(GameObject parent, string childName = null)
        {
            GameObject target;
            if (childName != null)
            {
                Transform existing = parent.transform.Find(childName);
                if (existing != null)
                {
                    target = existing.gameObject;
                }
                else
                {
                    target = new GameObject(childName);
                    target.transform.SetParent(parent.transform, false);
                }
            }
            else
            {
                target = parent;
            }

            ParticleSystem ps = target.GetComponent<ParticleSystem>();
            if (ps == null) ps = target.AddComponent<ParticleSystem>();

            var renderer = target.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
            }

            return ps;
        }

        // ==================== EFFECT SETUPS ====================

        private static void SetupConfetti(GameObject root)
        {
            ParticleSystem ps = GetOrAddPS(root, "ConfettiPS");
            var renderer = ps.GetComponent<ParticleSystemRenderer>();

            var main = ps.main;
            main.duration = 3f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2f, 4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = true;
            main.maxParticles = 200;
            main.gravityModifier = 0.5f;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0;
            emission.SetBurst(0, new ParticleSystem.Burst(0f, 120));

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(15f, 0.5f, 1f);

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.x = new ParticleSystem.MinMaxCurve(-1f, 1f);
            velocity.y = new ParticleSystem.MinMaxCurve(-2f, -0.5f);

            var rotation = ps.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-180f * Mathf.Deg2Rad, 180f * Mathf.Deg2Rad);

            SetFadeOverLifetime(ps);

            Material mat = CreateParticleMaterial();
            mat.mainTexture = CreateRectTexture(32, 48);
            renderer.material = mat;
        }

        private static void SetupFireworks(GameObject root)
        {
            // 3 explosiones secuenciales
            for (int i = 0; i < 3; i++)
            {
                ParticleSystem ps = GetOrAddPS(root, $"Burst{i}");
                var renderer = ps.GetComponent<ParticleSystemRenderer>();

                var main = ps.main;
                main.duration = 0.1f;
                main.loop = false;
                main.startDelay = i * 0.5f;
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(8f, 15f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.playOnAwake = true;
                main.maxParticles = 80;
                main.gravityModifier = 0.5f;

                // Offset position
                ps.transform.localPosition = new Vector3(
                    (i - 1) * 3f, Random.Range(-1f, 2f), 0f);

                var emission = ps.emission;
                emission.enabled = true;
                emission.rateOverTime = 0;
                emission.SetBurst(0, new ParticleSystem.Burst(0f, 60));

                var shape = ps.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.3f;

                var trails = ps.trails;
                trails.enabled = true;
                trails.lifetime = 0.2f;
                trails.inheritParticleColor = true;

                SetFadeOverLifetime(ps);

                Material mat = CreateParticleMaterial(true);
                mat.mainTexture = CreateCircleTexture(64);
                renderer.material = mat;
                renderer.trailMaterial = CreateParticleMaterial(true);
            }
        }

        private static void SetupLightning(GameObject root)
        {
            ParticleSystem ps = GetOrAddPS(root, "LightningPS");
            var renderer = ps.GetComponent<ParticleSystemRenderer>();

            var main = ps.main;
            main.duration = 1.8f;
            main.loop = false;
            main.startLifetime = 0.4f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 12f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.1f);
            main.startRotation = new ParticleSystem.MinMaxCurve(-15f * Mathf.Deg2Rad, 15f * Mathf.Deg2Rad);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = true;
            main.maxParticles = 60;
            main.gravityModifier = 2f;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0;
            emission.SetBurst(0, new ParticleSystem.Burst(0f, 40));

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(10f, 0.1f, 0.1f);
            shape.position = new Vector3(0, 5f, 0);

            SetFadeOverLifetime(ps);

            // Flash sub-system
            ParticleSystem flash = GetOrAddPS(root, "FlashPS");
            var flashMain = flash.main;
            flashMain.duration = 0.1f;
            flashMain.loop = false;
            flashMain.startLifetime = 0.1f;
            flashMain.startSpeed = 0f;
            flashMain.startSize = 20f;
            flashMain.startColor = new Color(1f, 1f, 1f, 0.6f);
            flashMain.playOnAwake = true;

            var flashEmission = flash.emission;
            flashEmission.enabled = true;
            flashEmission.rateOverTime = 0;
            flashEmission.SetBurst(0, new ParticleSystem.Burst(0f, 1));

            Material mat = CreateParticleMaterial(true);
            mat.mainTexture = CreateCircleTexture(64);
            renderer.material = mat;

            var flashRenderer = flash.GetComponent<ParticleSystemRenderer>();
            flashRenderer.material = CreateParticleMaterial(true);
            flashRenderer.material.mainTexture = CreateCircleTexture(64);
        }

        private static void SetupGoldRain(GameObject root)
        {
            ParticleSystem ps = GetOrAddPS(root, "GoldRainPS");
            var renderer = ps.GetComponent<ParticleSystemRenderer>();

            var main = ps.main;
            main.duration = 3f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 3f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.3f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.startColor = new Color(1f, 0.84f, 0f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = true;
            main.maxParticles = 100;
            main.gravityModifier = 1.5f;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 30;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(20f, 0.1f, 1f);
            shape.position = new Vector3(0, 5f, 0);

            var rotation = ps.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-360f * Mathf.Deg2Rad, 360f * Mathf.Deg2Rad);

            SetFadeOverLifetime(ps);

            Material mat = CreateParticleMaterial();
            mat.mainTexture = CreateRectTexture(24, 24);
            renderer.material = mat;
        }

        private static void SetupNeonExplosion(GameObject root)
        {
            ParticleSystem ps = GetOrAddPS(root, "ExplosionPS");
            var renderer = ps.GetComponent<ParticleSystemRenderer>();

            var main = ps.main;
            main.duration = 0.1f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(10f, 20f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = true;
            main.maxParticles = 120;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0;
            emission.SetBurst(0, new ParticleSystem.Burst(0f, 100));

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f,
                AnimationCurve.EaseInOut(0f, 1f, 1f, 0.1f));

            SetFadeOverLifetime(ps);

            // Shockwave ring
            ParticleSystem ring = GetOrAddPS(root, "RingPS");
            var ringMain = ring.main;
            ringMain.duration = 0.8f;
            ringMain.loop = false;
            ringMain.startLifetime = 0.8f;
            ringMain.startSpeed = 0f;
            ringMain.startSize = 0.5f;
            ringMain.playOnAwake = true;

            var ringEmission = ring.emission;
            ringEmission.enabled = true;
            ringEmission.rateOverTime = 0;
            ringEmission.SetBurst(0, new ParticleSystem.Burst(0f, 1));

            var ringSol = ring.sizeOverLifetime;
            ringSol.enabled = true;
            ringSol.size = new ParticleSystem.MinMaxCurve(1f,
                AnimationCurve.Linear(0f, 0f, 1f, 15f));

            Material mat = CreateParticleMaterial(true);
            mat.mainTexture = CreateCircleTexture(64);
            renderer.material = mat;

            var ringRenderer = ring.GetComponent<ParticleSystemRenderer>();
            ringRenderer.material = CreateParticleMaterial(true);
            ringRenderer.material.mainTexture = CreateCircleTexture(64);
        }

        private static void SetupRainbow(GameObject root)
        {
            Color[] rainbow = {
                Color.red, new Color(1f, 0.5f, 0f), Color.yellow,
                Color.green, Color.cyan, Color.blue, new Color(0.5f, 0f, 1f)
            };

            for (int i = 0; i < 7; i++)
            {
                ParticleSystem ps = GetOrAddPS(root, $"Layer{i}");
                var renderer = ps.GetComponent<ParticleSystemRenderer>();

                var main = ps.main;
                main.duration = 1.5f;
                main.loop = false;
                main.startDelay = i * 0.1f;
                main.startLifetime = new ParticleSystem.MinMaxCurve(1f, 2f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 6f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.15f);
                main.startColor = rainbow[i];
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.playOnAwake = true;
                main.maxParticles = 20;

                var emission = ps.emission;
                emission.enabled = true;
                emission.rateOverTime = 0;
                emission.SetBurst(0, new ParticleSystem.Burst(0f, 15));

                var shape = ps.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Box;
                shape.scale = new Vector3(15f, 0.1f, 0.1f);
                shape.position = new Vector3(-8f, i * 0.3f - 1f, 0);

                var velocity = ps.velocityOverLifetime;
                velocity.enabled = true;
                velocity.x = new ParticleSystem.MinMaxCurve(2f, 4f);

                SetFadeOverLifetime(ps);

                Material mat = CreateParticleMaterial();
                mat.mainTexture = CreateCircleTexture(32);
                renderer.material = mat;
            }
        }

        private static void SetupCrownDrop(GameObject root)
        {
            // Crown uses DOTween animation at runtime, here we just set up the burst particles
            ParticleSystem ps = GetOrAddPS(root, "ImpactBurstPS");
            var renderer = ps.GetComponent<ParticleSystemRenderer>();

            var main = ps.main;
            main.duration = 0.1f;
            main.loop = false;
            main.startDelay = 0.8f; // After crown lands
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 10f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.2f);
            main.startColor = new Color(1f, 0.84f, 0f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = true;
            main.maxParticles = 40;
            main.gravityModifier = 0.3f;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0;
            emission.SetBurst(0, new ParticleSystem.Burst(0f, 30));

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f;

            SetFadeOverLifetime(ps);

            // Shimmer particles (continuous glow after landing)
            ParticleSystem shimmer = GetOrAddPS(root, "ShimmerPS");
            var shimmerMain = shimmer.main;
            shimmerMain.duration = 1f;
            shimmerMain.loop = false;
            shimmerMain.startDelay = 1.0f;
            shimmerMain.startLifetime = 0.5f;
            shimmerMain.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            shimmerMain.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.1f);
            shimmerMain.startColor = new Color(1f, 0.9f, 0.5f);
            shimmerMain.playOnAwake = true;
            shimmerMain.maxParticles = 20;

            var shimmerEmission = shimmer.emission;
            shimmerEmission.enabled = true;
            shimmerEmission.rateOverTime = 15;

            var shimmerShape = shimmer.shape;
            shimmerShape.enabled = true;
            shimmerShape.shapeType = ParticleSystemShapeType.Sphere;
            shimmerShape.radius = 0.5f;

            Material mat = CreateParticleMaterial(true);
            mat.mainTexture = CreateCircleTexture(64);
            renderer.material = mat;

            var shimmerRenderer = shimmer.GetComponent<ParticleSystemRenderer>();
            shimmerRenderer.material = CreateParticleMaterial(true);
            shimmerRenderer.material.mainTexture = CreateCircleTexture(32);
        }

        private static void SetupFireRing(GameObject root)
        {
            // Primary fire ring
            ParticleSystem ps = GetOrAddPS(root, "FireRingPS");
            var renderer = ps.GetComponent<ParticleSystemRenderer>();

            var main = ps.main;
            main.duration = 1.5f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 6f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = true;
            main.maxParticles = 80;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0;
            emission.SetBurst(0, new ParticleSystem.Burst(0f, 60));

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.5f;
            shape.radiusThickness = 0f; // Edge only

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient g = new Gradient();
            g.SetKeys(
                new[] {
                    new GradientColorKey(new Color(1f, 0.6f, 0f), 0f),
                    new GradientColorKey(new Color(1f, 0.2f, 0f), 0.5f),
                    new GradientColorKey(new Color(0.2f, 0f, 0f), 1f)
                },
                new[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.8f, 0.6f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            col.color = g;

            // Second ring (delayed, larger)
            ParticleSystem ps2 = GetOrAddPS(root, "FireRing2PS");
            var main2 = ps2.main;
            main2.duration = 1.2f;
            main2.loop = false;
            main2.startDelay = 0.3f;
            main2.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.8f);
            main2.startSpeed = new ParticleSystem.MinMaxCurve(5f, 8f);
            main2.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
            main2.simulationSpace = ParticleSystemSimulationSpace.World;
            main2.playOnAwake = true;
            main2.maxParticles = 60;

            var emission2 = ps2.emission;
            emission2.enabled = true;
            emission2.rateOverTime = 0;
            emission2.SetBurst(0, new ParticleSystem.Burst(0f, 50));

            var shape2 = ps2.shape;
            shape2.enabled = true;
            shape2.shapeType = ParticleSystemShapeType.Circle;
            shape2.radius = 1f;
            shape2.radiusThickness = 0f;

            var col2 = ps2.colorOverLifetime;
            col2.enabled = true;
            col2.color = g;

            Material mat = CreateParticleMaterial(true);
            mat.mainTexture = CreateCircleTexture(64);
            renderer.material = mat;

            var renderer2 = ps2.GetComponent<ParticleSystemRenderer>();
            renderer2.material = CreateParticleMaterial(true);
            renderer2.material.mainTexture = CreateCircleTexture(64);
        }
    }
}
