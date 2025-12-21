using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace DigitPark.Editor
{
    public class BackButtonPrefabCreator
    {
        [MenuItem("DigitPark/Create BackButton Prefab")]
        public static void CreateBackButtonPrefab()
        {
            // Crear el GameObject
            GameObject backButton = new GameObject("BackButton");

            // RectTransform
            RectTransform rt = backButton.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(60, -30);
            rt.sizeDelta = new Vector2(60, 60);

            // Image
            Image image = backButton.AddComponent<Image>();
            image.color = new Color(0f, 0.9608f, 1f, 1f); // Cyan
            image.raycastTarget = true;
            image.preserveAspect = true;

            // Intentar cargar el sprite arrowWhite
            string[] guids = AssetDatabase.FindAssets("arrowWhite t:Sprite");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null)
                {
                    image.sprite = sprite;
                }
            }

            // Button
            Button button = backButton.AddComponent<Button>();
            button.targetGraphic = image;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            button.colors = colors;

            // Crear carpeta de prefabs si no existe
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
            }
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/UI"))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "UI");
            }

            // Guardar como prefab
            string prefabPath = "Assets/_Project/Prefabs/UI/BackButton.prefab";

            // Eliminar prefab existente si existe
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                AssetDatabase.DeleteAsset(prefabPath);
            }

            // Crear el prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(backButton, prefabPath);

            // Destruir el objeto temporal
            Object.DestroyImmediate(backButton);

            // Seleccionar el prefab creado
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);

            Debug.Log($"BackButton prefab creado en: {prefabPath}");
            EditorUtility.DisplayDialog("Prefab Creado",
                "BackButton prefab creado exitosamente.\n\n" +
                "Ubicación: Assets/_Project/Prefabs/UI/BackButton.prefab\n\n" +
                "Ahora puedes arrastrarlo a cualquier Canvas en tus escenas.",
                "OK");
        }
    }
}
