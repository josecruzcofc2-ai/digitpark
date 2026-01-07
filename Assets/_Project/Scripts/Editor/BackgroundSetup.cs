#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using DigitPark.Themes;

namespace DigitPark.Editor
{
    /// <summary>
    /// Agrega Background consistente a todas las escenas
    /// Color: #0A1428 (azul marino neon de Profile)
    /// </summary>
    public class BackgroundSetup : EditorWindow
    {
        // Color del Background - mismo que Profile
        private static readonly Color32 BACKGROUND_COLOR = new Color32(10, 20, 40, 255); // #0A1428

        private static readonly string[] SCENE_PATHS = new string[]
        {
            "Assets/_Project/Scenes/Boot.unity",
            "Assets/_Project/Scenes/Login.unity",
            "Assets/_Project/Scenes/Register.unity",
            "Assets/_Project/Scenes/MainMenu.unity",
            "Assets/_Project/Scenes/Profile.unity",
            "Assets/_Project/Scenes/Settings.unity",
            "Assets/_Project/Scenes/Scores.unity",
            "Assets/_Project/Scenes/Tournaments.unity",
            "Assets/_Project/Scenes/SearchPlayers.unity",
            "Assets/_Project/Scenes/Games/GameSelector.unity",
            "Assets/_Project/Scenes/Games/DigitRush.unity",
            "Assets/_Project/Scenes/Games/MemoryPairs.unity",
            "Assets/_Project/Scenes/Games/QuickMath.unity",
            "Assets/_Project/Scenes/Games/FlashTap.unity",
            "Assets/_Project/Scenes/Games/OddOneOut.unity",
            "Assets/_Project/Scenes/Games/CashBattle.unity"
        };

        [MenuItem("DigitPark/UI/Add Background to All Scenes")]
        public static void AddBackgroundToAllScenes()
        {
            int added = 0;
            int skipped = 0;
            int errors = 0;

            foreach (string scenePath in SCENE_PATHS)
            {
                // Verificar que la escena existe
                if (!System.IO.File.Exists(scenePath))
                {
                    Debug.LogWarning($"[Background] Escena no encontrada: {scenePath}");
                    errors++;
                    continue;
                }

                try
                {
                    var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

                    bool result = AddBackgroundToCurrentScene(sceneName);

                    if (result)
                    {
                        EditorSceneManager.SaveScene(scene);
                        added++;
                        Debug.Log($"[Background] ✓ Agregado a: {sceneName}");
                    }
                    else
                    {
                        skipped++;
                        Debug.Log($"[Background] - Ya existe en: {sceneName}");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[Background] Error en {scenePath}: {ex.Message}");
                    errors++;
                }
            }

            EditorUtility.DisplayDialog("Background Setup Completado",
                $"Resultados:\n\n" +
                $"✓ Backgrounds agregados: {added}\n" +
                $"- Ya existían: {skipped}\n" +
                $"✗ Errores: {errors}\n\n" +
                $"Color aplicado: #0A1428\n" +
                $"ThemeApplier: PrimaryBackground",
                "OK");
        }

        [MenuItem("DigitPark/UI/Add Background to Current Scene")]
        public static void AddBackgroundToCurrentSceneMenu()
        {
            string sceneName = EditorSceneManager.GetActiveScene().name;
            bool result = AddBackgroundToCurrentScene(sceneName);

            if (result)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorUtility.DisplayDialog("Background Agregado",
                    $"Background agregado a: {sceneName}\n\n" +
                    "Color: #0A1428\n" +
                    "ThemeApplier: PrimaryBackground\n\n" +
                    "No olvides guardar la escena.",
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Ya Existe",
                    $"La escena {sceneName} ya tiene un Background.\n\n" +
                    "Si quieres recrearlo, elimina el actual primero.",
                    "OK");
            }
        }

        private static bool AddBackgroundToCurrentScene(string sceneName)
        {
            // Buscar Canvas
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning($"[Background] No se encontró Canvas en: {sceneName}");
                return false;
            }

            // Verificar si ya existe Background
            Transform existingBG = canvas.transform.Find("Background");
            if (existingBG != null)
            {
                // Ya existe, verificar si tiene ThemeApplier
                ThemeApplier applier = existingBG.GetComponent<ThemeApplier>();
                if (applier == null)
                {
                    // Agregar ThemeApplier al existente
                    applier = existingBG.gameObject.AddComponent<ThemeApplier>();
                    SetupThemeApplier(applier);

                    // Actualizar color
                    Image img = existingBG.GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = BACKGROUND_COLOR;
                        EditorUtility.SetDirty(img);
                    }

                    EditorUtility.SetDirty(applier);
                    Debug.Log($"[Background] ThemeApplier agregado al Background existente en: {sceneName}");
                }
                return false;
            }

            // Crear Background
            GameObject background = new GameObject("Background");
            background.transform.SetParent(canvas.transform, false);

            // IMPORTANTE: Mover al inicio de la jerarquía (detrás de todo)
            background.transform.SetAsFirstSibling();

            // Agregar Image
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = BACKGROUND_COLOR;
            bgImage.raycastTarget = false; // No bloquea clicks

            // Configurar RectTransform - Stretch completo
            RectTransform rt = background.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);

            // Agregar ThemeApplier
            ThemeApplier themeApplier = background.AddComponent<ThemeApplier>();
            SetupThemeApplier(themeApplier);

            EditorUtility.SetDirty(background);
            EditorUtility.SetDirty(canvas.gameObject);

            return true;
        }

        private static void SetupThemeApplier(ThemeApplier applier)
        {
            // Usar SerializedObject para configurar campos privados
            SerializedObject so = new SerializedObject(applier);

            // Element Type = PrimaryBackground (index 1 en el enum)
            SerializedProperty elementTypeProp = so.FindProperty("elementType");
            if (elementTypeProp != null)
            {
                elementTypeProp.enumValueIndex = 1; // PrimaryBackground
            }

            // Apply to Image = true
            SerializedProperty applyToImageProp = so.FindProperty("applyToImage");
            if (applyToImageProp != null)
            {
                applyToImageProp.boolValue = true;
            }

            // Apply to Text = false
            SerializedProperty applyToTextProp = so.FindProperty("applyToText");
            if (applyToTextProp != null)
            {
                applyToTextProp.boolValue = false;
            }

            // Animate Transition = true
            SerializedProperty animateProp = so.FindProperty("animateTransition");
            if (animateProp != null)
            {
                animateProp.boolValue = true;
            }

            so.ApplyModifiedProperties();
        }

        [MenuItem("DigitPark/UI/Check Backgrounds Status")]
        public static void CheckBackgroundsStatus()
        {
            string report = "=== Estado de Backgrounds ===\n\n";
            int withBG = 0;
            int withoutBG = 0;

            foreach (string scenePath in SCENE_PATHS)
            {
                if (!System.IO.File.Exists(scenePath))
                {
                    continue;
                }

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

                Canvas canvas = Object.FindObjectOfType<Canvas>();
                if (canvas == null)
                {
                    report += $"⚠ {sceneName}: Sin Canvas\n";
                    continue;
                }

                Transform bg = canvas.transform.Find("Background");
                if (bg != null)
                {
                    ThemeApplier applier = bg.GetComponent<ThemeApplier>();
                    Image img = bg.GetComponent<Image>();
                    string colorHex = img != null ? ColorUtility.ToHtmlStringRGB(img.color) : "N/A";
                    string hasApplier = applier != null ? "✓" : "✗";

                    report += $"✓ {sceneName}: Background #{colorHex} (ThemeApplier: {hasApplier})\n";
                    withBG++;
                }
                else
                {
                    report += $"✗ {sceneName}: SIN BACKGROUND\n";
                    withoutBG++;
                }
            }

            report += $"\n=== Resumen ===\n";
            report += $"Con Background: {withBG}\n";
            report += $"Sin Background: {withoutBG}\n";

            Debug.Log(report);

            EditorUtility.DisplayDialog("Estado de Backgrounds",
                $"Con Background: {withBG}\n" +
                $"Sin Background: {withoutBG}\n\n" +
                "Ver Console para detalles completos.",
                "OK");
        }
    }
}
#endif
