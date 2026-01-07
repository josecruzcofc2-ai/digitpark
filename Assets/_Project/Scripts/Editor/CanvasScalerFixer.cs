#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace DigitPark.Editor
{
    /// <summary>
    /// Arregla Canvas Scaler en las 8 escenas nuevas
    /// Cambia a Scale With Screen Size, 1080x1920, Match 0.5
    /// </summary>
    public class CanvasScalerFixer : EditorWindow
    {
        private static readonly string[] SCENES_TO_FIX = new string[]
        {
            "Assets/_Project/Scenes/Profile.unity",
            "Assets/_Project/Scenes/SearchPlayers.unity",
            "Assets/_Project/Scenes/Games/GameSelector.unity",
            "Assets/_Project/Scenes/Games/CashBattle.unity",
            "Assets/_Project/Scenes/Games/MemoryPairs.unity",
            "Assets/_Project/Scenes/Games/QuickMath.unity",
            "Assets/_Project/Scenes/Games/FlashTap.unity",
            "Assets/_Project/Scenes/Games/OddOneOut.unity"
        };

        [MenuItem("DigitPark/Fix Canvas Scalers (8 New Scenes)")]
        public static void FixAllScenes()
        {
            if (!EditorUtility.DisplayDialog("Confirmar",
                "Arreglar Canvas Scaler en las 8 escenas nuevas?\n\n" +
                "Cambios:\n" +
                "- UI Scale Mode: Scale With Screen Size\n" +
                "- Reference Resolution: 1080 x 1920\n" +
                "- Match Width/Height: 0.5",
                "Si, Arreglar", "Cancelar"))
                return;

            int fixed_count = 0;
            string results = "";

            foreach (string scenePath in SCENES_TO_FIX)
            {
                string fullPath = Application.dataPath.Replace("Assets", "") + scenePath;
                if (!System.IO.File.Exists(fullPath))
                {
                    Debug.LogWarning($"[CanvasFixer] Escena no encontrada: {scenePath}");
                    continue;
                }

                try
                {
                    var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                    // Buscar Canvas Scaler
                    var scalers = Object.FindObjectsOfType<CanvasScaler>(true);

                    foreach (var scaler in scalers)
                    {
                        // Aplicar configuracion correcta
                        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                        scaler.referenceResolution = new Vector2(1080, 1920);
                        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                        scaler.matchWidthOrHeight = 0.5f;

                        EditorUtility.SetDirty(scaler);
                        Debug.Log($"[CanvasFixer] Arreglado Canvas en: {scene.name}");
                    }

                    if (scalers.Length > 0)
                    {
                        EditorSceneManager.SaveScene(scene);
                        fixed_count++;
                        results += $"{scene.name}: Arreglado\n";
                    }
                    else
                    {
                        results += $"{scene.name}: Sin CanvasScaler\n";
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[CanvasFixer] Error en {scenePath}: {e.Message}");
                    results += $"{scenePath}: ERROR\n";
                }
            }

            EditorUtility.DisplayDialog("Completado",
                $"Canvas Scalers arreglados.\n\n{results}\n" +
                $"Total: {fixed_count} escenas",
                "OK");
        }

        [MenuItem("DigitPark/Fix Canvas Scaler (Current Scene)")]
        public static void FixCurrentScene()
        {
            var scalers = Object.FindObjectsOfType<CanvasScaler>(true);

            if (scalers.Length == 0)
            {
                EditorUtility.DisplayDialog("Sin Canvas",
                    "No se encontro ningun CanvasScaler en la escena actual.", "OK");
                return;
            }

            foreach (var scaler in scalers)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;

                EditorUtility.SetDirty(scaler);
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            EditorUtility.DisplayDialog("Completado",
                $"Canvas Scaler arreglado en escena actual.\n\n" +
                "- Scale Mode: Scale With Screen Size\n" +
                "- Resolution: 1080 x 1920\n" +
                "- Match: 0.5\n\n" +
                "Guarda con Ctrl+S",
                "OK");
        }
    }
}
#endif
