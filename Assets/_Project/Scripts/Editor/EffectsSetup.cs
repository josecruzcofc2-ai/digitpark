using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using DigitPark.Effects;

namespace DigitPark.Editor
{
    /// <summary>
    /// Herramientas de editor para configurar el sistema de efectos
    /// </summary>
    public class EffectsSetup
    {
        [MenuItem("DigitPark/Effects/Setup FeedbackManager")]
        public static void SetupFeedbackManager()
        {
            // Verificar si ya existe
            FeedbackManager existing = Object.FindObjectOfType<FeedbackManager>();
            if (existing != null)
            {
                EditorUtility.DisplayDialog("Info",
                    "FeedbackManager ya existe en la escena.", "OK");
                Selection.activeGameObject = existing.gameObject;
                return;
            }

            // Crear FeedbackManager
            GameObject fmObj = new GameObject("FeedbackManager");
            fmObj.AddComponent<FeedbackManager>();

            // Agregar FloatingText
            GameObject ftObj = new GameObject("FloatingText");
            ftObj.transform.SetParent(fmObj.transform);
            ftObj.AddComponent<FloatingText>();

            Undo.RegisterCreatedObjectUndo(fmObj, "Create FeedbackManager");
            Selection.activeGameObject = fmObj;

            EditorUtility.DisplayDialog("Exito",
                "FeedbackManager creado.\n\n" +
                "Este objeto persiste entre escenas (DontDestroyOnLoad).\n" +
                "Colócalo en tu primera escena (Boot o Login).",
                "OK");
        }

        [MenuItem("DigitPark/Effects/Add ButtonEffects to All Buttons")]
        public static void AddButtonEffectsToAllButtons()
        {
            Button[] buttons = Object.FindObjectsOfType<Button>(true);
            int added = 0;

            foreach (Button btn in buttons)
            {
                if (btn.GetComponent<ButtonEffects>() == null)
                {
                    ButtonEffects effects = btn.gameObject.AddComponent<ButtonEffects>();

                    // Determinar tipo segun el nombre
                    string name = btn.gameObject.name.ToLower();

                    if (name.Contains("play") || name.Contains("start") || name.Contains("buy"))
                    {
                        // Boton importante
                        SetButtonEffectType(effects, ButtonEffects.ButtonEffectType.Important);
                    }
                    else if (name.Contains("confirm") || name.Contains("ok") || name.Contains("accept"))
                    {
                        SetButtonEffectType(effects, ButtonEffects.ButtonEffectType.Success);
                    }
                    else if (name.Contains("delete") || name.Contains("cancel") || name.Contains("close"))
                    {
                        SetButtonEffectType(effects, ButtonEffects.ButtonEffectType.Danger);
                    }
                    else if (name.Contains("premium") || name.Contains("pro"))
                    {
                        SetButtonEffectType(effects, ButtonEffects.ButtonEffectType.Premium);
                    }

                    EditorUtility.SetDirty(btn.gameObject);
                    added++;
                }
            }

            EditorUtility.DisplayDialog("ButtonEffects Agregados",
                $"Se agregó ButtonEffects a {added} botones.\n\n" +
                "Recuerda guardar la escena (Ctrl+S).",
                "OK");
        }

        private static void SetButtonEffectType(ButtonEffects effects, ButtonEffects.ButtonEffectType type)
        {
            // Usar SerializedObject para modificar campos privados
            SerializedObject so = new SerializedObject(effects);
            SerializedProperty typeProp = so.FindProperty("effectType");
            if (typeProp != null)
            {
                typeProp.enumValueIndex = (int)type;
                so.ApplyModifiedProperties();
            }
        }

        [MenuItem("DigitPark/Effects/Add ButtonEffects to Selected")]
        public static void AddButtonEffectsToSelected()
        {
            GameObject[] selected = Selection.gameObjects;
            int added = 0;

            foreach (GameObject obj in selected)
            {
                Button btn = obj.GetComponent<Button>();
                if (btn != null && btn.GetComponent<ButtonEffects>() == null)
                {
                    btn.gameObject.AddComponent<ButtonEffects>();
                    EditorUtility.SetDirty(obj);
                    added++;
                }
            }

            EditorUtility.DisplayDialog("ButtonEffects",
                $"Se agregó ButtonEffects a {added} botones seleccionados.",
                "OK");
        }

        [MenuItem("DigitPark/Effects/Add NeonGlow to Selected")]
        public static void AddNeonGlowToSelected()
        {
            GameObject[] selected = Selection.gameObjects;
            int added = 0;

            foreach (GameObject obj in selected)
            {
                Graphic graphic = obj.GetComponent<Graphic>();
                if (graphic != null && obj.GetComponent<NeonGlowEffect>() == null)
                {
                    obj.AddComponent<NeonGlowEffect>();
                    EditorUtility.SetDirty(obj);
                    added++;
                }
            }

            EditorUtility.DisplayDialog("NeonGlow",
                $"Se agregó NeonGlowEffect a {added} elementos.",
                "OK");
        }

        [MenuItem("DigitPark/Effects/Setup All Scenes")]
        public static void SetupAllScenes()
        {
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

            foreach (string scenePath in scenes)
            {
                if (!System.IO.File.Exists(scenePath.Replace("/", "\\")))
                {
                    Debug.LogWarning($"Escena no encontrada: {scenePath}");
                    continue;
                }

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                Button[] buttons = Object.FindObjectsOfType<Button>(true);
                int sceneButtons = 0;

                foreach (Button btn in buttons)
                {
                    if (btn.GetComponent<ButtonEffects>() == null)
                    {
                        ButtonEffects effects = btn.gameObject.AddComponent<ButtonEffects>();

                        // Auto-detectar tipo
                        string name = btn.gameObject.name.ToLower();
                        if (name.Contains("play") || name.Contains("start") || name.Contains("buy"))
                        {
                            SetButtonEffectType(effects, ButtonEffects.ButtonEffectType.Important);
                        }
                        else if (name.Contains("premium") || name.Contains("pro"))
                        {
                            SetButtonEffectType(effects, ButtonEffects.ButtonEffectType.Premium);
                        }

                        EditorUtility.SetDirty(btn.gameObject);
                        sceneButtons++;
                    }
                }

                if (sceneButtons > 0)
                {
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"[EffectsSetup] {scenePath}: {sceneButtons} botones actualizados");
                }

                totalButtons += sceneButtons;
            }

            EditorUtility.DisplayDialog("Setup Completado",
                $"Se agregó ButtonEffects a {totalButtons} botones en todas las escenas.\n\n" +
                "Todas las escenas han sido guardadas.",
                "OK");
        }

        [MenuItem("DigitPark/Effects/Create Background Particles")]
        public static void CreateBackgroundParticles()
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontró Canvas.", "OK");
                return;
            }

            // Crear contenedor
            GameObject bgParticles = new GameObject("BackgroundParticles");
            bgParticles.transform.SetParent(canvas.transform, false);
            bgParticles.transform.SetAsFirstSibling(); // Ponerlo atras

            RectTransform rt = bgParticles.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Crear sistema de particulas
            ParticleSystem ps = bgParticles.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 10f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(5f, 10f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 15f);
            main.startSize = new ParticleSystem.MinMaxCurve(2f, 8f);
            main.startColor = new Color(0f, 0.9608f, 1f, 0.3f);
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = 50;

            var emission = ps.emission;
            emission.rateOverTime = 3;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Rectangle;
            shape.scale = new Vector3(20f, 12f, 1f);

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
                    new GradientAlphaKey(0.2f, 0.2f),
                    new GradientAlphaKey(0.2f, 0.8f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            var renderer = bgParticles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = -100;

            // Crear material
            Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
            mat.SetFloat("_Mode", 2);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            renderer.material = mat;

            Undo.RegisterCreatedObjectUndo(bgParticles, "Create Background Particles");
            Selection.activeGameObject = bgParticles;

            EditorUtility.DisplayDialog("Creado",
                "BackgroundParticles creado.\n" +
                "Aparecerá como particulas flotantes sutiles en el fondo.",
                "OK");
        }
    }
}
