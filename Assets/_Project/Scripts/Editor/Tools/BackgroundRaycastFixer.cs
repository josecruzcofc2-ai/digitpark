using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace DigitPark.Editor
{
    /// <summary>
    /// Fixes all fullscreen Background Images that have raycastTarget=true.
    /// Background images should never block raycasts since they're decorative.
    /// Menu: DigitPark > Fix > Disable Background RaycastTarget (All Scenes)
    /// </summary>
    public class BackgroundRaycastFixer
    {
        [MenuItem("DigitPark/Fix/Disable Background RaycastTarget (Current Scene)", false, 500)]
        public static void FixCurrentScene()
        {
            int fixed_count = FixBackgroundsInCurrentScene();

            if (fixed_count > 0)
            {
                EditorSceneManager.MarkAllScenesDirty();
            }

            EditorUtility.DisplayDialog("Background Raycast Fix",
                $"Fixed {fixed_count} Background Image(s) in current scene.\n\n" +
                "Remember to save the scene (Ctrl+S).",
                "OK");
        }

        [MenuItem("DigitPark/Fix/Disable Background RaycastTarget (All Scenes)", false, 501)]
        public static void FixAllScenes()
        {
            string[] scenes = GetAllScenePaths();
            int totalFixed = 0;
            int scenesProcessed = 0;

            foreach (string scenePath in scenes)
            {
                if (!System.IO.File.Exists(scenePath.Replace("/", "\\")))
                {
                    Debug.LogWarning($"[BgFix] Scene not found: {scenePath}");
                    continue;
                }

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                int fixed_count = FixBackgroundsInCurrentScene();

                if (fixed_count > 0)
                {
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"[BgFix] {scenePath}: Fixed {fixed_count} Background(s)");
                }

                totalFixed += fixed_count;
                scenesProcessed++;
            }

            EditorUtility.DisplayDialog("Background Raycast Fix - All Scenes",
                $"Processed {scenesProcessed} scenes.\n" +
                $"Fixed {totalFixed} Background Image(s) total.\n\n" +
                "All scenes have been saved.",
                "OK");
        }

        private static int FixBackgroundsInCurrentScene()
        {
            int fixed_count = 0;
            Image[] allImages = Object.FindObjectsOfType<Image>(true);

            foreach (Image img in allImages)
            {
                if (!img.raycastTarget) continue;

                string name = img.gameObject.name.ToLower();
                if (name != "background") continue;

                RectTransform rt = img.rectTransform;
                bool isFullscreen = rt.anchorMin == Vector2.zero && rt.anchorMax == Vector2.one;

                // Also check if it has a Button component (buttons need raycast)
                if (img.GetComponent<Button>() != null) continue;

                if (isFullscreen || IsLikelyBackground(img))
                {
                    img.raycastTarget = false;
                    EditorUtility.SetDirty(img.gameObject);
                    fixed_count++;
                    Debug.Log($"[BgFix] Fixed: {GetFullPath(img.transform)} (raycastTarget -> false)");
                }
            }

            return fixed_count;
        }

        private static bool IsLikelyBackground(Image img)
        {
            // Background images typically have no sprite and a solid color
            // Or they are the first child of a canvas
            Transform parent = img.transform.parent;
            if (parent != null && parent.GetComponent<Canvas>() != null)
            {
                // Direct child of a Canvas named Background = definitely a bg
                return true;
            }
            return false;
        }

        private static string GetFullPath(Transform t)
        {
            string path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }

        private static string[] GetAllScenePaths()
        {
            return new string[]
            {
                // Core
                "Assets/_Project/Scenes/_Core/MainMenu.unity",
                "Assets/_Project/Scenes/_Core/Settings.unity",
                // Games - Navigation
                "Assets/_Project/Scenes/Games/Navigation/GameSelector.unity",
                "Assets/_Project/Scenes/Games/Navigation/PlayModeSelection.unity",
                "Assets/_Project/Scenes/Games/Navigation/BetSelection.unity",
                "Assets/_Project/Scenes/Games/Navigation/Matchmaking.unity",
                // Games - Minigames
                "Assets/_Project/Scenes/Games/Minigames/DigitRush.unity",
                "Assets/_Project/Scenes/Games/Minigames/FlashTap.unity",
                "Assets/_Project/Scenes/Games/Minigames/MemoryPairs.unity",
                "Assets/_Project/Scenes/Games/Minigames/OddOneOut.unity",
                "Assets/_Project/Scenes/Games/Minigames/QuickMath.unity",
                // Social - Profile
                "Assets/_Project/Scenes/Social/Profile/Profile.unity",
                "Assets/_Project/Scenes/Social/Profile/Scores.unity",
                "Assets/_Project/Scenes/Social/Profile/MatchHistory.unity",
                // Social - Friends
                "Assets/_Project/Scenes/Social/Friends/Friends.unity",
                // Monetization
                "Assets/_Project/Scenes/Monetization/Achievements.unity",
                "Assets/_Project/Scenes/Monetization/Shop.unity",
                "Assets/_Project/Scenes/Monetization/DailyMissions.unity",
                "Assets/_Project/Scenes/Monetization/DailyRewards.unity",
                // Tournaments
                "Assets/_Project/Scenes/Tournaments/TournamentsBrowser.unity",
                "Assets/_Project/Scenes/Tournaments/TournamentCreate.unity",
                "Assets/_Project/Scenes/Tournaments/TournamentLobby.unity",
            };
        }
    }
}
