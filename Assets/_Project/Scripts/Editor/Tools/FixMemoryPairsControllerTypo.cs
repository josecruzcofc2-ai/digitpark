#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DigitPark.Editor
{
    /// <summary>
    /// E-04: Renombra el GO mal escrito "MemoryParisController" → "MemoryPairsController"
    /// en MemoryPairs.unity usando la Unity API (sin editar YAML directamente).
    /// Menú: DigitPark/Fix/Rename MemoryPairsController Typo
    /// </summary>
    public static class FixMemoryPairsControllerTypo
    {
        private const string TYPO_NAME   = "MemoryParisController";
        private const string FIXED_NAME  = "MemoryPairsController";
        private const string SCENE_PATH  = "Assets/_Project/Scenes/Games/Minigames/MemoryPairs.unity";

        [MenuItem("DigitPark/Fix/Rename MemoryPairsController Typo")]
        public static void RenameTypo()
        {
            var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Additive);

            var typo = GameObject.Find(TYPO_NAME);
            if (typo == null)
            {
                Debug.Log($"[FixMemoryPairs] '{TYPO_NAME}' not found — already fixed or GO renamed.");
                EditorSceneManager.CloseScene(scene, true);
                return;
            }

            Undo.RecordObject(typo, "Rename MemoryPairsController typo");
            typo.name = FIXED_NAME;
            EditorUtility.SetDirty(typo);
            EditorSceneManager.SaveScene(scene);
            EditorSceneManager.CloseScene(scene, true);

            Debug.Log($"[FixMemoryPairs] Done — '{TYPO_NAME}' renamed to '{FIXED_NAME}'.");
        }
    }
}
#endif
