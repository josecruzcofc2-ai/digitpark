using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

namespace DigitPark.Editor
{
    /// <summary>
    /// Asigna automaticamente los iconos a los componentes UI en las escenas.
    /// </summary>
    public class IconAssigner : EditorWindow
    {
        private static readonly string ICONS_PATH = "Assets/_Project/Art/Icons";

        [MenuItem("DigitPark/Icons/Assign All Icons in Scene", false, 300)]
        public static void AssignAllIcons()
        {
            int assigned = 0;

            // Assign BottomNavBar icons
            assigned += AssignBottomNavBarIcons();

            // Assign Currency Display icons
            assigned += AssignCurrencyDisplayIcons();

            // Assign Compact Currency Button icons
            assigned += AssignCompactCurrencyIcons();

            if (assigned > 0)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                    UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }

            Debug.Log($"[IconAssigner] Total icons assigned: {assigned}");
            EditorUtility.DisplayDialog("Icon Assigner", $"Se asignaron {assigned} iconos correctamente.", "OK");
        }

        [MenuItem("DigitPark/Icons/Configure Icons as Sprites", false, 301)]
        public static void ConfigureIconsAsSprites()
        {
            string[] iconFiles = {
                "icon_home.png",
                "icon_play.png",
                "icon_shop.png",
                "icon_missions.png",
                "icon_profile.png",
                "icon_gem.png",
                "icon_coin.png",
                "icon_cash.png"
            };

            int configured = 0;

            foreach (var iconFile in iconFiles)
            {
                string path = $"{ICONS_PATH}/{iconFile}";
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer != null)
                {
                    if (importer.textureType != TextureImporterType.Sprite)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        importer.spriteImportMode = SpriteImportMode.Single;
                        importer.alphaIsTransparency = true;
                        importer.mipmapEnabled = false;
                        importer.filterMode = FilterMode.Bilinear;
                        importer.textureCompression = TextureImporterCompression.Compressed;

                        // Set max size for mobile
                        importer.maxTextureSize = 256;

                        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                        configured++;
                        Debug.Log($"[IconAssigner] Configured as Sprite: {iconFile}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[IconAssigner] Icon not found: {path}");
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"[IconAssigner] Configured {configured} icons as Sprites");
            EditorUtility.DisplayDialog("Icon Assigner", $"Se configuraron {configured} iconos como Sprites.", "OK");
        }

        private static int AssignBottomNavBarIcons()
        {
            int count = 0;
            var bottomNav = Object.FindObjectOfType<UI.BottomNavBar>();

            if (bottomNav == null)
            {
                Debug.Log("[IconAssigner] No BottomNavBar found in scene");
                return 0;
            }

            SerializedObject so = new SerializedObject(bottomNav);

            // Home button
            count += AssignIconToNavButton(so, "_homeButton", "icon_home.png");
            // Play button
            count += AssignIconToNavButton(so, "_playButton", "icon_play.png");
            // Shop button
            count += AssignIconToNavButton(so, "_shopButton", "icon_shop.png");
            // Missions button
            count += AssignIconToNavButton(so, "_missionsButton", "icon_missions.png");
            // Profile button
            count += AssignIconToNavButton(so, "_profileButton", "icon_profile.png");

            so.ApplyModifiedProperties();

            Debug.Log($"[IconAssigner] BottomNavBar: {count} icons assigned");
            return count;
        }

        private static int AssignIconToNavButton(SerializedObject so, string buttonPropertyName, string iconFileName)
        {
            SerializedProperty navBtnProp = so.FindProperty(buttonPropertyName);
            if (navBtnProp == null) return 0;

            SerializedProperty iconProp = navBtnProp.FindPropertyRelative("icon");
            if (iconProp == null) return 0;

            Sprite sprite = LoadSprite(iconFileName);
            if (sprite != null)
            {
                // Get the Image component and assign sprite directly
                Image iconImage = iconProp.objectReferenceValue as Image;
                if (iconImage != null)
                {
                    iconImage.sprite = sprite;
                    EditorUtility.SetDirty(iconImage);
                    return 1;
                }
            }

            return 0;
        }

        private static int AssignCurrencyDisplayIcons()
        {
            int count = 0;
            var currencyDisplay = Object.FindObjectOfType<Monetization.HeaderCurrencyDisplay>();

            if (currencyDisplay == null)
            {
                Debug.Log("[IconAssigner] No HeaderCurrencyDisplay found in scene");
                return 0;
            }

            SerializedObject so = new SerializedObject(currencyDisplay);

            // Gems icon
            SerializedProperty gemsIconProp = so.FindProperty("_gemsIcon");
            if (gemsIconProp != null && gemsIconProp.objectReferenceValue != null)
            {
                Image gemsIcon = gemsIconProp.objectReferenceValue as Image;
                if (gemsIcon != null)
                {
                    Sprite gemSprite = LoadSprite("icon_gem.png");
                    if (gemSprite != null)
                    {
                        gemsIcon.sprite = gemSprite;
                        EditorUtility.SetDirty(gemsIcon);
                        count++;
                    }
                }
            }

            // Coins icon
            SerializedProperty coinsIconProp = so.FindProperty("_coinsIcon");
            if (coinsIconProp != null && coinsIconProp.objectReferenceValue != null)
            {
                Image coinsIcon = coinsIconProp.objectReferenceValue as Image;
                if (coinsIcon != null)
                {
                    Sprite coinSprite = LoadSprite("icon_coin.png");
                    if (coinSprite != null)
                    {
                        coinsIcon.sprite = coinSprite;
                        EditorUtility.SetDirty(coinsIcon);
                        count++;
                    }
                }
            }

            so.ApplyModifiedProperties();

            Debug.Log($"[IconAssigner] HeaderCurrencyDisplay: {count} icons assigned");
            return count;
        }

        private static int AssignCompactCurrencyIcons()
        {
            int count = 0;

            // Find CompactCurrencyButton in scene
            GameObject compactBtn = GameObject.Find("CompactCurrencyButton");
            if (compactBtn == null)
            {
                Debug.Log("[IconAssigner] No CompactCurrencyButton found in scene");
                return 0;
            }

            // Find GemIcon child
            Transform gemIconTransform = compactBtn.transform.Find("GemIcon");
            if (gemIconTransform != null)
            {
                Image gemImage = gemIconTransform.GetComponent<Image>();
                if (gemImage != null)
                {
                    Sprite gemSprite = LoadSprite("icon_gem.png");
                    if (gemSprite != null)
                    {
                        gemImage.sprite = gemSprite;
                        EditorUtility.SetDirty(gemImage);
                        count++;
                    }
                }
            }

            Debug.Log($"[IconAssigner] CompactCurrencyButton: {count} icons assigned");
            return count;
        }

        private static Sprite LoadSprite(string fileName)
        {
            string path = $"{ICONS_PATH}/{fileName}";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if (sprite == null)
            {
                Debug.LogWarning($"[IconAssigner] Could not load sprite: {path}");
            }

            return sprite;
        }
    }
}
