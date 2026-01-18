using UnityEditor;
using UnityEngine;

namespace DigitPark.Editor
{
    /// <summary>
    /// Utilidad para forzar recompilación en Unity
    /// </summary>
    public static class ForceRecompile
    {
        [MenuItem("DigitPark/Tools/Force Recompile Scripts", false, 10)]
        public static void RecompileScripts()
        {
            Debug.Log("[ForceRecompile] Forzando recompilación...");
            AssetDatabase.Refresh();
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
            Debug.Log("[ForceRecompile] Recompilación solicitada. Espera unos segundos...");
        }
    }
}
