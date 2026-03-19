#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DigitPark.Editor
{
    /// <summary>
    /// E-01: Agrega el tag "FrameLayer" a TagManager.asset via SerializedObject.
    /// Idempotente: no hace nada si el tag ya existe.
    /// Menú: DigitPark/Setup/Add FrameLayer Tag
    /// </summary>
    public static class AddFrameLayerTag
    {
        private const string TAG_NAME = "FrameLayer";

        [MenuItem("DigitPark/Setup/Add FrameLayer Tag")]
        public static void AddTag()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0)
            {
                Debug.LogError("[AddFrameLayerTag] Could not load TagManager.asset.");
                return;
            }

            var tagManager = new SerializedObject(assets[0]);
            var tagsProp   = tagManager.FindProperty("tags");

            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue == TAG_NAME)
                {
                    Debug.Log($"[AddFrameLayerTag] Tag '{TAG_NAME}' already exists — nothing to do.");
                    return;
                }
            }

            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = TAG_NAME;
            tagManager.ApplyModifiedProperties();

            Debug.Log($"[AddFrameLayerTag] Tag '{TAG_NAME}' added successfully.");
        }
    }
}
#endif
