using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using DigitPark.Effects;

namespace DigitPark.Editor
{
    /// <summary>
    /// Script para ejecutar setup de efectos en batch mode
    /// </summary>
    public class BatchEffectsSetup
    {
        [MenuItem("DigitPark/Effects/[AUTO] Setup Complete System")]
        public static void SetupCompleteSystem()
        {
            Debug.Log("=== INICIANDO SETUP COMPLETO DE EFECTOS ===");

            string[] scenes = new string[]
            {
                "Assets/_Project/Scenes/Boot.unity",
                "Assets/_Project/Scenes/Login.unity",
                "Assets/_Project/Scenes/Register.unity",
                "Assets/_Project/Scenes/MainMenu.unity",
                "Assets/_Project/Scenes/Game.unity",
                "Assets/_Project/Scenes/Settings.unity",
                "Assets/_Project/Scenes/Scores.unity",
                "Assets/_Project/Scenes/Tournaments.unity"
            };

            int totalButtons = 0;
            int totalGlows = 0;
            bool feedbackManagerCreated = false;

            foreach (string scenePath in scenes)
            {
                if (!System.IO.File.Exists(scenePath.Replace("/", "\\")))
                {
                    Debug.LogWarning($"[Setup] Escena no encontrada: {scenePath}");
                    continue;
                }

                Debug.Log($"[Setup] Procesando: {scenePath}");
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                // === FEEDBACK MANAGER (solo en Boot) ===
                if (scenePath.Contains("Boot") && !feedbackManagerCreated)
                {
                    if (Object.FindObjectOfType<FeedbackManager>() == null)
                    {
                        GameObject fmObj = new GameObject("FeedbackManager");
                        fmObj.AddComponent<FeedbackManager>();

                        GameObject ftObj = new GameObject("FloatingText");
                        ftObj.transform.SetParent(fmObj.transform);
                        ftObj.AddComponent<FloatingText>();

                        Debug.Log("[Setup] FeedbackManager creado en Boot");
                        feedbackManagerCreated = true;
                    }
                }

                // === BUTTON EFFECTS ===
                Button[] buttons = Object.FindObjectsOfType<Button>(true);
                foreach (Button btn in buttons)
                {
                    if (btn.GetComponent<ButtonEffects>() == null)
                    {
                        ButtonEffects effects = btn.gameObject.AddComponent<ButtonEffects>();
                        ConfigureButtonType(effects, btn.gameObject.name);
                        EditorUtility.SetDirty(btn.gameObject);
                        totalButtons++;
                    }
                }

                // === NEON GLOW en titulos y elementos importantes ===
                TMPro.TextMeshProUGUI[] texts = Object.FindObjectsOfType<TMPro.TextMeshProUGUI>(true);
                foreach (var text in texts)
                {
                    string name = text.gameObject.name.ToLower();
                    if ((name.Contains("title") || name.Contains("titulo") || name.Contains("header"))
                        && text.GetComponent<NeonGlowEffect>() == null)
                    {
                        var glow = text.gameObject.AddComponent<NeonGlowEffect>();
                        // Configurar modo breathing para titulos
                        var so = new SerializedObject(glow);
                        var modeProp = so.FindProperty("glowMode");
                        if (modeProp != null)
                        {
                            modeProp.enumValueIndex = (int)NeonGlowEffect.GlowMode.Breathing;
                            so.ApplyModifiedProperties();
                        }
                        EditorUtility.SetDirty(text.gameObject);
                        totalGlows++;
                    }
                }

                // === BACKGROUND PARTICLES (en MainMenu) ===
                if (scenePath.Contains("MainMenu"))
                {
                    CreateBackgroundParticlesIfNeeded();
                }

                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[Setup] {scenePath} guardado");
            }

            Debug.Log("=== SETUP COMPLETADO ===");
            Debug.Log($"Total botones con efectos: {totalButtons}");
            Debug.Log($"Total elementos con glow: {totalGlows}");
            Debug.Log($"FeedbackManager creado: {feedbackManagerCreated}");

            EditorUtility.DisplayDialog("Setup Completado",
                $"Sistema de efectos configurado:\n\n" +
                $"• {totalButtons} botones con efectos\n" +
                $"• {totalGlows} elementos con glow\n" +
                $"• FeedbackManager: {(feedbackManagerCreated ? "Creado" : "Ya existía")}\n\n" +
                "Todas las escenas han sido guardadas.",
                "OK");
        }

        private static void ConfigureButtonType(ButtonEffects effects, string buttonName)
        {
            string name = buttonName.ToLower();

            ButtonEffects.ButtonEffectType type = ButtonEffects.ButtonEffectType.Normal;

            if (name.Contains("play") || name.Contains("start") || name.Contains("jugar"))
            {
                type = ButtonEffects.ButtonEffectType.Important;
            }
            else if (name.Contains("buy") || name.Contains("comprar") || name.Contains("purchase"))
            {
                type = ButtonEffects.ButtonEffectType.Important;
            }
            else if (name.Contains("confirm") || name.Contains("ok") || name.Contains("accept") || name.Contains("aceptar"))
            {
                type = ButtonEffects.ButtonEffectType.Success;
            }
            else if (name.Contains("delete") || name.Contains("eliminar") || name.Contains("cancel") || name.Contains("cancelar"))
            {
                type = ButtonEffects.ButtonEffectType.Danger;
            }
            else if (name.Contains("close") || name.Contains("cerrar") || name.Contains("back"))
            {
                type = ButtonEffects.ButtonEffectType.Normal;
            }
            else if (name.Contains("premium") || name.Contains("pro"))
            {
                type = ButtonEffects.ButtonEffectType.Premium;
            }
            else if (name.Contains("login") || name.Contains("register") || name.Contains("signup"))
            {
                type = ButtonEffects.ButtonEffectType.Important;
            }

            // Aplicar tipo usando SerializedObject
            var so = new SerializedObject(effects);
            var typeProp = so.FindProperty("effectType");
            if (typeProp != null)
            {
                typeProp.enumValueIndex = (int)type;
                so.ApplyModifiedProperties();
            }
        }

        private static void CreateBackgroundParticlesIfNeeded()
        {
            if (GameObject.Find("BackgroundParticles") != null) return;

            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null) return;

            GameObject bgParticles = new GameObject("BackgroundParticles");
            bgParticles.transform.SetParent(canvas.transform, false);
            bgParticles.transform.SetAsFirstSibling();

            RectTransform rt = bgParticles.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            ParticleSystem ps = bgParticles.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 10f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(5f, 10f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 15f);
            main.startSize = new ParticleSystem.MinMaxCurve(2f, 8f);
            main.startColor = new Color(0f, 0.9608f, 1f, 0.15f);
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = 30;
            main.playOnAwake = true;

            var emission = ps.emission;
            emission.rateOverTime = 2;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Rectangle;
            shape.scale = new Vector3(15f, 10f, 1f);

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(0f, 0.9608f, 1f), 0f),
                    new GradientColorKey(new Color(0.6157f, 0.2941f, 1f), 0.5f),
                    new GradientColorKey(new Color(1f, 0.84f, 0f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.15f, 0.3f),
                    new GradientAlphaKey(0.15f, 0.7f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            var renderer = bgParticles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = -100;

            Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
            mat.SetFloat("_Mode", 2);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            renderer.material = mat;

            Debug.Log("[Setup] BackgroundParticles creado en MainMenu");
        }

        // Metodo para llamar desde batch mode
        public static void RunBatchSetup()
        {
            SetupCompleteSystem();
        }
    }
}
