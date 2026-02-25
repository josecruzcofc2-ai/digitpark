using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace DigitPark.Editor
{
    /// <summary>
    /// Assigns the Neon Cyan BackButton prefab to specific scenes
    /// that were missing it or had incorrect styles (gold/provisional).
    /// Target scenes: Settings, Matchmaking, DailyRewards, TournamentCreate
    /// </summary>
    public static class BackButtonNeonFixer
    {
        private const string BACK_BUTTON_PREFAB_PATH = "Assets/_Project/Prefabs/Common/BackButton.prefab";

        // Scenes and their specific parent paths for the BackButton
        private static readonly Dictionary<string, SceneBackButtonConfig> TARGET_SCENES = new Dictionary<string, SceneBackButtonConfig>
        {
            {
                "Assets/_Project/Scenes/_Core/Settings.unity",
                new SceneBackButtonConfig("Settings", "Header", new Vector2(30, 0), anchorY: 0.5f)
            },
            {
                "Assets/_Project/Scenes/Games/Flow/Matchmaking.unity",
                new SceneBackButtonConfig("Matchmaking", "SafeArea", new Vector2(30, -40), anchorY: 1f)
            },
            {
                "Assets/_Project/Scenes/Monetization/DailyRewards.unity",
                new SceneBackButtonConfig("DailyRewards", "TopBar", new Vector2(25, 0), anchorY: 0.5f)
            },
            {
                "Assets/_Project/Scenes/Tournaments/TournamentCreate.unity",
                new SceneBackButtonConfig("TournamentCreate", "Header", new Vector2(20, 0), anchorY: 0.5f)
            }
        };

        private struct SceneBackButtonConfig
        {
            public string displayName;
            public string parentPath;
            public Vector2 position;
            public float anchorY;

            public SceneBackButtonConfig(string displayName, string parentPath, Vector2 position, float anchorY)
            {
                this.displayName = displayName;
                this.parentPath = parentPath;
                this.position = position;
                this.anchorY = anchorY;
            }
        }

        [MenuItem("DigitPark/UI/Fix BackButton Neon in 4 Scenes", false, 203)]
        public static void FixBackButtonsInTargetScenes()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BACK_BUTTON_PREFAB_PATH);
            if (prefab == null)
            {
                Debug.LogError($"[BackButtonNeonFixer] Prefab not found at {BACK_BUTTON_PREFAB_PATH}");
                Debug.Log("Create it first: DigitPark > UI > Prefabs > Create BackButton Prefab");
                return;
            }

            // Save current scene so we can return to it
            string currentScenePath = EditorSceneManager.GetActiveScene().path;

            int fixed_count = 0;
            int error_count = 0;

            Debug.Log("[BackButtonNeonFixer] === Fixing Neon BackButton in 4 scenes ===");

            foreach (var kvp in TARGET_SCENES)
            {
                string scenePath = kvp.Key;
                SceneBackButtonConfig config = kvp.Value;

                if (FixSceneBackButton(scenePath, config, prefab))
                    fixed_count++;
                else
                    error_count++;
            }

            Debug.Log($"[BackButtonNeonFixer] === Done! Fixed: {fixed_count}, Errors: {error_count} ===");

            // Return to original scene
            if (!string.IsNullOrEmpty(currentScenePath))
                EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);

            AssetDatabase.SaveAssets();
        }

        private static bool FixSceneBackButton(string scenePath, SceneBackButtonConfig config, GameObject prefab)
        {
            try
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
                if (canvas == null)
                {
                    Debug.LogError($"[BackButtonNeonFixer] {config.displayName}: No Canvas found");
                    return false;
                }

                // Remove ALL existing back buttons (any name, any location)
                RemoveAllBackButtons(canvas.transform);

                // Find the target parent
                Transform parent;
                if (string.IsNullOrEmpty(config.parentPath))
                {
                    parent = canvas.transform;
                }
                else
                {
                    parent = FindRecursive(canvas.transform, config.parentPath);
                    if (parent == null)
                    {
                        Debug.LogWarning($"[BackButtonNeonFixer] {config.displayName}: Parent '{config.parentPath}' not found, using Canvas");
                        parent = canvas.transform;
                    }
                }

                // Instantiate the neon cyan prefab
                GameObject backBtn = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                backBtn.name = "BackButton";

                // Position it
                RectTransform rt = backBtn.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, config.anchorY);
                rt.anchorMax = new Vector2(0, config.anchorY);
                rt.pivot = new Vector2(0, config.anchorY);
                rt.anchoredPosition = config.position;
                rt.sizeDelta = new Vector2(50, 50);

                // Render on top
                backBtn.transform.SetAsLastSibling();

                // Save
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);

                Debug.Log($"[BackButtonNeonFixer] {config.displayName}: Neon Cyan BackButton assigned");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BackButtonNeonFixer] {config.displayName}: Error - {e.Message}");
                return false;
            }
        }

        private static void RemoveAllBackButtons(Transform root)
        {
            // Search common paths first
            string[] searchPaths = new string[]
            {
                "BackButton",
                "BackButtonGold",
                "SafeArea/BackButton",
                "SafeArea/BackButtonGold",
                "Header/BackButton",
                "Header/BackButtonGold",
                "TopBar/BackButton",
                "TopBar/BackButtonGold"
            };

            foreach (string path in searchPaths)
            {
                Transform found = root.Find(path);
                if (found != null)
                {
                    Debug.Log($"  Removed old: {path}");
                    Object.DestroyImmediate(found.gameObject);
                }
            }

            // Also search recursively for anything named BackButton/BackButtonGold
            var allTransforms = root.GetComponentsInChildren<Transform>(true);
            foreach (var t in allTransforms)
            {
                if (t == null) continue;
                if (t.name == "BackButton" || t.name == "BackButtonGold")
                {
                    Debug.Log($"  Removed old (recursive): {t.name}");
                    Object.DestroyImmediate(t.gameObject);
                }
            }
        }

        private static Transform FindRecursive(Transform root, string name)
        {
            // Try direct child first
            Transform direct = root.Find(name);
            if (direct != null) return direct;

            // Try SafeArea/name
            Transform safeArea = root.Find("SafeArea");
            if (safeArea != null)
            {
                Transform inSafe = safeArea.Find(name);
                if (inSafe != null) return inSafe;
            }

            // Recursive search
            foreach (Transform child in root)
            {
                if (child.name == name) return child;
                Transform found = FindRecursive(child, name);
                if (found != null) return found;
            }

            return null;
        }
    }
}
