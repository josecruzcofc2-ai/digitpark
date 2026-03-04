using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// Builder for premium golden back button prefab (Cash Battle theme).
    /// If BackIconGold.png exists, uses it as icon sprite.
    /// Otherwise falls back to TMP gold arrow character.
    /// </summary>
    public static class BackButtonGoldBuilder
    {
        private const string PREFAB_PATH = "Assets/_Project/Prefabs/Common/BackButtonGold.prefab";
        private const string WHITE_SPRITE_PATH = "Assets/_Project/Art/Icons/UI/WhiteSquare.png";
        private const string GOLD_ICON_PATH = "Assets/_Project/Art/Icons/Navigation/BackIconGold.png";
        private const string FONT_ASSET_PATH = "Assets/_Project/Art/Fonts/Rajdhani/Rajdhani-Medium SDF.asset";

        private static readonly Color GOLD = new Color(1f, 0.843f, 0f, 1f);
        private static readonly Color GOLD_HIGHLIGHT = new Color(1f, 0.647f, 0f, 0.9f);
        private static readonly Color GOLD_PRESSED = new Color(1f, 0.549f, 0f, 1f);
        private static readonly Color DARK_BG = new Color(0.08f, 0.06f, 0.12f, 0.95f);

        [MenuItem("DigitPark/UI/Prefabs/Create BackButtonGold Prefab", false, 101)]
        public static void CreateBackButtonGoldPrefab()
        {
            // Create root GameObject
            GameObject backButton = new GameObject("BackButtonGold");

            RectTransform rectTransform = backButton.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(80, 80);
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            // Dark background
            Image bg = backButton.AddComponent<Image>();
            Sprite whiteSprite = AssetDatabase.LoadAssetAtPath<Sprite>(WHITE_SPRITE_PATH);
            if (whiteSprite != null) bg.sprite = whiteSprite;
            bg.color = DARK_BG;
            bg.raycastTarget = true;

            // Gold border
            Outline outline = backButton.AddComponent<Outline>();
            outline.effectColor = GOLD;
            outline.effectDistance = new Vector2(2, 2);

            // Button component
            Button button = backButton.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            button.targetGraphic = bg;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = GOLD_HIGHLIGHT;
            colors.pressedColor = GOLD_PRESSED;
            colors.selectedColor = GOLD_HIGHLIGHT;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            button.colors = colors;

            // Icon child
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(backButton.transform, false);

            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.sizeDelta = Vector2.zero;
            iconRect.anchoredPosition = Vector2.zero;

            // Try gold icon sprite first, fall back to TMP arrow
            Sprite goldIcon = AssetDatabase.LoadAssetAtPath<Sprite>(GOLD_ICON_PATH);
            Image iconImage = null;

            if (goldIcon != null)
            {
                iconImage = iconObj.AddComponent<Image>();
                iconImage.sprite = goldIcon;
                iconImage.preserveAspect = true;
                iconImage.raycastTarget = false;
                Debug.Log("✅ Using BackIconGold.png sprite");
            }
            else
            {
                // Fallback: TMP gold arrow on dark background
                TextMeshProUGUI arrowTMP = iconObj.AddComponent<TextMeshProUGUI>();
                TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_ASSET_PATH);
                if (font != null) arrowTMP.font = font;
                arrowTMP.text = "\u2039";
                arrowTMP.fontSize = FontSizes.H1;
                arrowTMP.color = GOLD;
                arrowTMP.alignment = TextAlignmentOptions.Center;
                arrowTMP.fontStyle = FontStyles.Bold;
                arrowTMP.raycastTarget = false;
                Debug.LogWarning($"⚠️ BackIconGold.png not found at {GOLD_ICON_PATH}. Using TMP arrow fallback.");
                Debug.Log("Place a gold back icon at: " + GOLD_ICON_PATH);
            }

            // Add BackButtonGold component and wire serialized references
            var bbg = backButton.AddComponent<BackButtonGold>();
            var so = new SerializedObject(bbg);
            so.FindProperty("button").objectReferenceValue = button;
            if (iconImage != null)
                so.FindProperty("iconImage").objectReferenceValue = iconImage;
            so.ApplyModifiedProperties();

            // Save prefab
            string directory = System.IO.Path.GetDirectoryName(PREFAB_PATH);
            if (!System.IO.Directory.Exists(directory))
                System.IO.Directory.CreateDirectory(directory);

            PrefabUtility.SaveAsPrefabAsset(backButton, PREFAB_PATH);
            Object.DestroyImmediate(backButton);

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
            Debug.Log($"✅ BackButtonGold prefab created at {PREFAB_PATH}");
            AssetDatabase.Refresh();
        }
    }
}
