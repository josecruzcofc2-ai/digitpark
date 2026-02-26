using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;

namespace DigitPark.Editor
{
    public static class BackButtonBuilder
    {
        private const string PREFAB_PATH = "Assets/_Project/Prefabs/Common/BackButton.prefab";
        private const string ICON_PATH = "Assets/_Project/Textures/UI/Icons/BackIcon.png";

        [MenuItem("DigitPark/UI/Prefabs/Create BackButton Prefab", false, 100)]
        public static void CreateBackButtonPrefab()
        {
            // Load the icon
            Sprite backIcon = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_PATH);
            if (backIcon == null)
            {
                Debug.LogError($"BackButton Icon not found at {ICON_PATH}");
                return;
            }

            // Create root GameObject
            GameObject backButton = new GameObject("BackButton");

            // Add RectTransform (required for UI)
            RectTransform rectTransform = backButton.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(80, 80); // Icon size
            rectTransform.anchorMin = new Vector2(0, 1); // Top-left anchor
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            // Add Button component
            Button button = backButton.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0f, 1f, 1f, 0.8f); // Cyan highlight
            colors.pressedColor = new Color(0f, 1f, 1f, 1f); // Full cyan when pressed
            colors.selectedColor = new Color(0f, 1f, 1f, 0.8f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            button.colors = colors;

            // Create Icon child
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(backButton.transform, false);

            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.sizeDelta = Vector2.zero; // Fill parent
            iconRect.anchoredPosition = Vector2.zero;

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.sprite = backIcon;
            iconImage.raycastTarget = false; // Don't block raycasts
            iconImage.preserveAspect = true;

            // Set button target graphic
            button.targetGraphic = iconImage;

            // Add BackButton component
            BackButton backButtonComponent = backButton.AddComponent<BackButton>();

            // Create the prefab
            string directory = System.IO.Path.GetDirectoryName(PREFAB_PATH);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            // Save as prefab
            PrefabUtility.SaveAsPrefabAsset(backButton, PREFAB_PATH);

            // Cleanup
            Object.DestroyImmediate(backButton);

            // Select the new prefab
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);

            Debug.Log($"✅ BackButton prefab created at {PREFAB_PATH}");
            AssetDatabase.Refresh();
        }

        // OBSOLETE: Removed "Update All BackButtons in Scenes"
        // Use: DigitPark > UI > Auto-Add > Add BackButtons to All Scenes instead
    }
}
