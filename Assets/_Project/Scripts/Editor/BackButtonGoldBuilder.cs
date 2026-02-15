using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using DigitPark.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// Builder for premium golden back button prefab (Cash Battle theme)
    /// </summary>
    public static class BackButtonGoldBuilder
    {
        private const string PREFAB_PATH = "Assets/_Project/Prefabs/Common/BackButtonGold.prefab";
        private const string ICON_PATH = "Assets/_Project/Textures/UI/Icons/BackIconGold.png";

        [MenuItem("DigitPark/UI/Prefabs/Create BackButtonGold Prefab", false, 101)]
        public static void CreateBackButtonGoldPrefab()
        {
            // Load the gold icon
            Sprite goldIcon = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_PATH);
            if (goldIcon == null)
            {
                Debug.LogError($"❌ BackButtonGold Icon not found at {ICON_PATH}");
                return;
            }

            // Create root GameObject
            GameObject backButton = new GameObject("BackButtonGold");

            // Add RectTransform (required for UI)
            RectTransform rectTransform = backButton.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(80, 80); // Icon size
            rectTransform.anchorMin = new Vector2(0, 1); // Top-left anchor
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            // Add Button component
            Button button = backButton.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;

            // Gold color scheme
            Color goldNormal = new Color(1f, 0.843f, 0f, 1f); // #FFD700
            Color goldHighlight = new Color(1f, 0.647f, 0f, 0.9f); // #FFA500 slightly transparent
            Color goldPressed = new Color(1f, 0.549f, 0f, 1f); // #FF8C00

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = goldHighlight;
            colors.pressedColor = goldPressed;
            colors.selectedColor = goldHighlight;
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
            iconImage.sprite = goldIcon;
            iconImage.raycastTarget = true; // Required for button hit area
            iconImage.preserveAspect = true;

            // Set button target graphic
            button.targetGraphic = iconImage;

            // Add BackButtonGold component
            BackButtonGold backButtonComponent = backButton.AddComponent<BackButtonGold>();

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

            Debug.Log($"✅ BackButtonGold prefab created at {PREFAB_PATH}");
            AssetDatabase.Refresh();
        }
    }
}
