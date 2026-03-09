using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace DigitPark.Editor
{
    /// <summary>
    /// Automatically assigns app icons for iOS from the AppIcon folder.
    /// Run from: DigitPark > Auto Assigners > Core > Assign App Icons (iOS)
    /// </summary>
    public class AppIconAutoAssigner
    {
        private const string ICONS_PATH = "Assets/_Project/Art/Icons/AppIcon";

        [MenuItem("DigitPark/Scenes/Assign Icons/Core/App Icons (iOS)")]
        public static void AssignIOSIcons()
        {
            Debug.Log("=== DigitPark App Icon Auto Assigner ===");

            // Define iOS icon requirements: (width, kind, subkind)
            // Kind: 0=Application, 1=Settings, 2=Notification, 3=Spotlight, 4=Marketing
            var iconMappings = new Dictionary<int, (int kind, string subkind, string description)>
            {
                // Application Icons
                { 180, (0, "iPhone", "Application @3x") },
                { 120, (0, "iPhone", "Application @2x") },
                { 152, (0, "iPad", "Application iPad @2x") },
                { 167, (0, "iPad", "Application iPad Pro") },
                { 76, (0, "iPad", "Application iPad @1x") },

                // Spotlight Icons
                { 80, (3, "iPhone", "Spotlight @2x") },
                // 120 is shared with Application

                // Settings Icons
                { 87, (1, "iPhone", "Settings @3x") },
                { 58, (1, "iPhone", "Settings @2x") },
                { 29, (1, "iPhone", "Settings @1x") },

                // Notification Icons
                { 60, (2, "iPhone", "Notification @3x") },
                { 40, (2, "iPhone", "Notification @2x") },
                { 20, (2, "iPhone", "Notification @1x") },

                // Marketing
                { 1024, (4, "", "Marketing / App Store") },
            };

            int assigned = 0;
            int failed = 0;

            foreach (var mapping in iconMappings)
            {
                int size = mapping.Key;
                string description = mapping.Value.description;

                string iconPath = $"{ICONS_PATH}/AppIcon_{size}x{size}.png";

                Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);

                if (icon != null)
                {
                    Debug.Log($"<color=green>✓</color> Found: {size}x{size} - {description}");
                    assigned++;
                }
                else
                {
                    Debug.LogWarning($"<color=red>✗</color> Missing: {size}x{size} - {description} (expected at {iconPath})");
                    failed++;
                }
            }

            Debug.Log($"\n=== Summary ===");
            Debug.Log($"Found: {assigned} icons");
            Debug.Log($"Missing: {failed} icons");

            if (failed == 0)
            {
                Debug.Log("<color=cyan>All icons are ready! Now assign them manually in:</color>");
                Debug.Log("<color=yellow>Edit > Project Settings > Player > iOS > Icon</color>");
                Debug.Log("\nIcon locations:");
                Debug.Log($"<color=white>{ICONS_PATH}/</color>");

                EditorUtility.DisplayDialog(
                    "App Icons Ready",
                    $"All {assigned} iOS icons found!\n\n" +
                    "To assign them:\n" +
                    "1. Go to Edit > Project Settings > Player\n" +
                    "2. Select iOS tab\n" +
                    "3. Expand 'Icon' section\n" +
                    "4. Drag icons from:\n" +
                    $"   {ICONS_PATH}/\n\n" +
                    "Icons are named by size (e.g., AppIcon_180x180.png)",
                    "OK"
                );
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Missing Icons",
                    $"Found {assigned} icons, but {failed} are missing.\n\n" +
                    "Check the Console for details.",
                    "OK"
                );
            }

            // Select the icons folder in Project window
            var folder = AssetDatabase.LoadAssetAtPath<Object>(ICONS_PATH);
            if (folder != null)
            {
                Selection.activeObject = folder;
                EditorGUIUtility.PingObject(folder);
            }
        }

        [MenuItem("DigitPark/Scenes/Assign Icons/Core/Open App Icons Folder")]
        public static void OpenIconsFolder()
        {
            var folder = AssetDatabase.LoadAssetAtPath<Object>(ICONS_PATH);
            if (folder != null)
            {
                Selection.activeObject = folder;
                EditorGUIUtility.PingObject(folder);
            }
            else
            {
                Debug.LogError($"Icons folder not found at: {ICONS_PATH}");
            }
        }
    }
}
