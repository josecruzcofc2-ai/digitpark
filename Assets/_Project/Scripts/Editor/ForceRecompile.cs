using UnityEditor;
using UnityEngine;

namespace DigitPark.Editor
{
    /// <summary>
    /// Utilidad para forzar recompilación en Unity
    /// </summary>
    public static class ForceRecompile
    {
        [MenuItem("Tools/Force Recompile Scripts")]
        public static void RecompileScripts()
        {
            Debug.Log("[ForceRecompile] Forzando recompilación...");
            AssetDatabase.Refresh();
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
            Debug.Log("[ForceRecompile] Recompilación solicitada. Espera unos segundos...");
        }
    }
}
