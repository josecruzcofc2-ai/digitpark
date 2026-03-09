using UnityEngine;
using UnityEditor;
using System.IO;

namespace DigitPark.Editor
{
    /// <summary>
    /// Automatically sets all achievement icons to Sprite import type.
    /// Run from: DigitPark > Tools > Setup Achievement Icons Import Settings
    /// </summary>
    public class AchievementIconImporter : AssetPostprocessor
    {
        private const string ICONS_PATH = "Assets/_Project/Resources/Icons/Achievements";

        [MenuItem("DigitPark/Setup/Achievement Icons Import Settings")]
        public static void SetupAllIconsImportSettings()
        {
            // Find all PNG files directly
            string[] pngFiles = System.IO.Directory.GetFiles(
                System.IO.Path.GetFullPath(ICONS_PATH),
                "*.png",
                System.IO.SearchOption.TopDirectoryOnly
            );

            int count = 0;
            int total = pngFiles.Length;

            foreach (string fullPath in pngFiles)
            {
                // Convert to Unity path
                string assetPath = "Assets" + fullPath.Replace(Application.dataPath, "").Replace("\\", "/");

                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

                if (importer != null)
                {
                    bool needsUpdate = importer.textureType != TextureImporterType.Sprite;

                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.mipmapEnabled = false;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.textureCompression = TextureImporterCompression.Compressed;
                    importer.maxTextureSize = 512;

                    // Set sprite settings
                    TextureImporterSettings settings = new TextureImporterSettings();
                    importer.ReadTextureSettings(settings);
                    settings.spriteMeshType = SpriteMeshType.FullRect;
                    settings.spritePixelsPerUnit = 100;
                    importer.SetTextureSettings(settings);

                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();

                    if (needsUpdate) count++;
                }
                else
                {
                    Debug.LogWarning($"[AchievementIconImporter] Could not get importer for: {assetPath}");
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Import Settings Updated",
                $"Updated {count} icons to Sprite type.\nTotal PNG files found: {total}", "OK");

            Debug.Log($"[AchievementIconImporter] Updated {count} icons to Sprite type. Total: {total}");
        }

        // Auto-import new icons as sprites
        void OnPreprocessTexture()
        {
            if (assetPath.StartsWith(ICONS_PATH))
            {
                TextureImporter importer = (TextureImporter)assetImporter;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.maxTextureSize = 512;
            }
        }
    }
}
