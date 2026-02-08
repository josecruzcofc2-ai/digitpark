#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using DigitPark.Themes;

namespace DigitPark.Editor
{
    /// <summary>
    /// Removes ThemeApplier and NeonButtonGlow from protected scenes (CashBattle, AgeVerification).
    /// These scenes have fixed gold styling that should never change with themes.
    /// Run from: DigitPark > Themes > Remove ThemeApplier from Protected Scenes
    /// </summary>
    public class ThemeApplierProtector : EditorWindow
    {
        /// <summary>
        /// Scenes that must NEVER have ThemeApplier or NeonButtonGlow.
        /// CashBattle = gold theme, AgeVerification = gold theme.
        /// </summary>
        private static readonly string[] ProtectedScenePaths = new string[]
        {
            "Assets/_Project/Scenes/CashBattle/CashBattleHub.unity",
            "Assets/_Project/Scenes/CashBattle/CashBattle1v1.unity",
            "Assets/_Project/Scenes/CashBattle/CashHistory.unity",
            "Assets/_Project/Scenes/CashBattle/CashTournaments.unity",
            "Assets/_Project/Scenes/CashBattle/CashWallet.unity",
            "Assets/_Project/Scenes/Auth/AgeVerification.unity",
        };

        [MenuItem("DigitPark/Themes/Remove ThemeApplier from Protected Scenes (CashBattle + AgeVerification)")]
        public static void RemoveFromProtectedScenes()
        {
            if (!EditorUtility.DisplayDialog("Confirmar",
                "Esto eliminara TODOS los componentes ThemeApplier y NeonButtonGlow de:\n\n" +
                "- CashBattleHub\n- CashBattle1v1\n- CashHistory\n- CashTournaments\n- CashWallet\n- AgeVerification\n\n" +
                "Estas escenas mantendran su estilo Gold fijo.",
                "Si, limpiar", "Cancelar"))
                return;

            string currentScenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            int totalRemoved = 0;

            foreach (string scenePath in ProtectedScenePaths)
            {
                if (!System.IO.File.Exists(scenePath))
                {
                    Debug.Log($"[ThemeProtector] Escena no encontrada (skip): {scenePath}");
                    continue;
                }

                try
                {
                    var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    int removed = RemoveThemeComponents();
                    totalRemoved += removed;

                    if (removed > 0)
                    {
                        EditorSceneManager.SaveScene(scene);
                        Debug.Log($"[ThemeProtector] {scenePath}: {removed} componentes eliminados");
                    }
                    else
                    {
                        Debug.Log($"[ThemeProtector] {scenePath}: ya estaba limpia");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[ThemeProtector] Error en {scenePath}: {e.Message}");
                }
            }

            // Reabrir la escena original
            if (!string.IsNullOrEmpty(currentScenePath) && System.IO.File.Exists(currentScenePath))
            {
                EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
            }

            EditorUtility.DisplayDialog("Completado",
                $"Se eliminaron {totalRemoved} componentes de tema de las escenas protegidas.\n\n" +
                "CashBattle y AgeVerification ahora estan limpias.",
                "OK");
        }

        private static int RemoveThemeComponents()
        {
            int count = 0;

            // Remove all ThemeApplier components
            var appliers = GameObject.FindObjectsOfType<ThemeApplier>(true);
            foreach (var applier in appliers)
            {
                Debug.Log($"[ThemeProtector]   Eliminando ThemeApplier de: {GetFullPath(applier.gameObject)}");
                DestroyImmediate(applier);
                count++;
            }

            // Remove all NeonButtonGlow components
            var glows = GameObject.FindObjectsOfType<DigitPark.UI.Components.NeonButtonGlow>(true);
            foreach (var glow in glows)
            {
                Debug.Log($"[ThemeProtector]   Eliminando NeonButtonGlow de: {GetFullPath(glow.gameObject)}");
                DestroyImmediate(glow);
                count++;
            }

            return count;
        }

        private static string GetFullPath(GameObject go)
        {
            string path = go.name;
            Transform parent = go.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }

        /// <summary>
        /// Checks if a scene path is protected (should not have ThemeApplier).
        /// Used by ThemeApplierSetup to skip these scenes.
        /// </summary>
        public static bool IsProtectedScene(string scenePath)
        {
            foreach (string protectedPath in ProtectedScenePaths)
            {
                if (scenePath.Contains("CashBattle") || scenePath.Contains("AgeVerification"))
                    return true;
            }
            return false;
        }
    }
}
#endif
