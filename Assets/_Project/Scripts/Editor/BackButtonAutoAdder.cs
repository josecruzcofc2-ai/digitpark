using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;

namespace DigitPark.Editor
{
    /// <summary>
    /// Intelligently adds BackButton prefabs to all scenes
    /// - Gold theme for Cash Battle scenes
    /// - Cyan theme for regular scenes
    /// </summary>
    public static class BackButtonAutoAdder
    {
        private const string BACK_BUTTON_CYAN_PATH = "Assets/_Project/Prefabs/Common/BackButton.prefab";
        private const string BACK_BUTTON_GOLD_PATH = "Assets/_Project/Prefabs/Common/BackButtonGold.prefab";
        private const string SCENES_PATH = "Assets/_Project/Scenes";

        // Scenes that use GOLD back button (Cash Battle theme)
        private static readonly HashSet<string> GOLD_SCENES = new HashSet<string>
        {
            "CashBattle1v1",
            "CashBattleHub",
            "CashHistory",
            "CashTournaments",
            "CashWallet",
            "AgeVerification"
        };

        // Scenes that should NOT have ANY back button
        private static readonly HashSet<string> EXCLUDED_SCENES = new HashSet<string>
        {
            // Authentication flow (can't go back)
            "Login",
            "Register",

            // Main menu (root of navigation)
            "MainMenu",

            // Game scenes (handled by game manager)
            "DigitRush",
            "FlashTap",
            "MemoryPairs",
            "QuickMath",
            "OddOneOut",
            "CognitiveSprint",

            // Matchmaking (has cancel button instead)
            "Matchmaking",

            // Onboarding scenes (have own navigation)
            "Onboarding",
            "CashBattleOnboarding",

            // Boot scene
            "Boot"
        };

        [MenuItem("DigitPark/UI/Auto-Add/Add BackButtons to All Scenes", false, 200)]
        public static void AddBackButtonsToAllScenes()
        {
            // Load both prefabs
            GameObject cyanPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BACK_BUTTON_CYAN_PATH);
            GameObject goldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BACK_BUTTON_GOLD_PATH);

            if (cyanPrefab == null)
            {
                Debug.LogError($"❌ Cyan BackButton prefab not found at {BACK_BUTTON_CYAN_PATH}");
                Debug.Log("Create it first using: DigitPark > UI > Prefabs > Create BackButton Prefab");
                return;
            }

            if (goldPrefab == null)
            {
                Debug.LogWarning($"⚠️ Gold BackButton prefab not found at {BACK_BUTTON_GOLD_PATH}");
                Debug.Log("Create it using: DigitPark > UI > Prefabs > Create BackButtonGold Prefab");
                Debug.Log("Continuing with Cyan buttons only...");
            }

            // Find all scene files
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { SCENES_PATH });
            List<string> scenePaths = sceneGuids.Select(AssetDatabase.GUIDToAssetPath).ToList();

            Debug.Log($"🔍 Found {scenePaths.Count} scenes");
            Debug.Log($"💎 Gold scenes: {string.Join(", ", GOLD_SCENES)}");
            Debug.Log($"🚫 Excluded scenes: {string.Join(", ", EXCLUDED_SCENES)}");

            int cyanAdded = 0;
            int goldAdded = 0;
            int skippedCount = 0;
            int errorCount = 0;

            foreach (string scenePath in scenePaths)
            {
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

                // Skip excluded scenes
                if (EXCLUDED_SCENES.Contains(sceneName))
                {
                    Debug.Log($"⏭️ Skipped {sceneName} (excluded)");
                    skippedCount++;
                    continue;
                }

                // Determine which prefab to use
                GameObject prefabToUse;
                string buttonType;

                if (GOLD_SCENES.Contains(sceneName))
                {
                    if (goldPrefab == null)
                    {
                        Debug.LogWarning($"⚠️ {sceneName}: Needs gold button but prefab not found, skipping");
                        skippedCount++;
                        continue;
                    }
                    prefabToUse = goldPrefab;
                    buttonType = "GOLD";
                }
                else
                {
                    prefabToUse = cyanPrefab;
                    buttonType = "CYAN";
                }

                // Add BackButton to this scene
                if (AddBackButtonToScene(scenePath, sceneName, prefabToUse, buttonType))
                {
                    if (buttonType == "GOLD")
                        goldAdded++;
                    else
                        cyanAdded++;
                }
                else
                {
                    errorCount++;
                }
            }

            Debug.Log($"✅ BackButton Auto-Add Complete!");
            Debug.Log($"  💎 Gold added: {goldAdded}");
            Debug.Log($"  🔷 Cyan added: {cyanAdded}");
            Debug.Log($"  ⏭️ Skipped: {skippedCount}");
            Debug.Log($"  ❌ Errors: {errorCount}");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static bool AddBackButtonToScene(string scenePath, string sceneName, GameObject backButtonPrefab, string buttonType)
        {
            try
            {
                // Open scene
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                // Find Canvas
                Canvas canvas = Object.FindFirstObjectByType<Canvas>();
                if (canvas == null)
                {
                    Debug.LogWarning($"⚠️ {sceneName}: No Canvas found, skipping");
                    return false;
                }

                // Remove existing BackButton if any (clean install)
                var existingBackButton = FindExistingBackButton(canvas.transform);
                if (existingBackButton != null)
                {
                    Object.DestroyImmediate(existingBackButton.gameObject);
                    Debug.Log($"  🗑️ Removed old BackButton from {sceneName}");
                }

                // Find SafeArea or use Canvas directly (NO Header creation)
                Transform parent = canvas.transform.Find("SafeArea") ?? canvas.transform;

                // Instantiate BackButton prefab directly on parent
                GameObject backButtonInstance = (GameObject)PrefabUtility.InstantiatePrefab(backButtonPrefab, parent);
                backButtonInstance.name = buttonType == "GOLD" ? "BackButtonGold" : "BackButton";

                // Position it at top-left corner
                RectTransform backButtonRect = backButtonInstance.GetComponent<RectTransform>();
                if (backButtonRect != null)
                {
                    backButtonRect.anchorMin = new Vector2(0, 1);
                    backButtonRect.anchorMax = new Vector2(0, 1);
                    backButtonRect.pivot = new Vector2(0, 1);
                    backButtonRect.anchoredPosition = new Vector2(15, -15);
                    backButtonRect.sizeDelta = new Vector2(50, 50);
                }

                // Make sure it's on top (last sibling = rendered last = on top)
                backButtonInstance.transform.SetAsLastSibling();

                // Mark scene as dirty and save
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);

                string icon = buttonType == "GOLD" ? "💎" : "🔷";
                Debug.Log($"{icon} {sceneName}: {buttonType} BackButton added successfully");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ {sceneName}: Error adding BackButton - {e.Message}");
                return false;
            }
        }

        private static Transform FindExistingBackButton(Transform canvas)
        {
            // Try all common locations
            string[] searchPaths = new string[]
            {
                "BackButton",
                "BackButtonGold",
                "SafeArea/BackButton",
                "SafeArea/BackButtonGold",
                "Header/BackButton",
                "Header/BackButtonGold"
            };

            foreach (string path in searchPaths)
            {
                Transform backButton = canvas.Find(path);
                if (backButton != null) return backButton;
            }

            // Also search recursively for any BackButton component
            var allButtons = canvas.GetComponentsInChildren<Transform>(true);
            foreach (var t in allButtons)
            {
                if (t.name == "BackButton" || t.name == "BackButtonGold")
                    return t;
            }

            return null;
        }

        [MenuItem("DigitPark/UI/Auto-Add/Remove All BackButtons from Scenes", false, 201)]
        public static void RemoveBackButtonFromAllScenes()
        {
            if (!EditorUtility.DisplayDialog(
                "Remove All BackButtons",
                "This will remove ALL BackButtons (cyan and gold) from ALL scenes. Are you sure?",
                "Yes, Remove All",
                "Cancel"))
            {
                return;
            }

            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { SCENES_PATH });
            List<string> scenePaths = sceneGuids.Select(AssetDatabase.GUIDToAssetPath).ToList();

            int removedCount = 0;

            foreach (string scenePath in scenePaths)
            {
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                Canvas canvas = Object.FindFirstObjectByType<Canvas>();

                if (canvas != null)
                {
                    Transform backButton = FindExistingBackButton(canvas.transform);

                    if (backButton != null)
                    {
                        Object.DestroyImmediate(backButton.gameObject);
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                        removedCount++;
                        Debug.Log($"🗑️ Removed BackButton from {sceneName}");
                    }
                }
            }

            Debug.Log($"✅ Removed BackButton from {removedCount} scenes");
            AssetDatabase.SaveAssets();
        }

        [MenuItem("DigitPark/UI/Auto-Add/Update BackButton in Current Scene", false, 202)]
        public static void UpdateBackButtonInCurrentScene()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("❌ No Canvas found in current scene");
                return;
            }

            // Get current scene name
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(
                EditorSceneManager.GetActiveScene().path);

            // Determine which prefab to use
            GameObject prefabToUse;
            string buttonType;

            if (GOLD_SCENES.Contains(sceneName))
            {
                prefabToUse = AssetDatabase.LoadAssetAtPath<GameObject>(BACK_BUTTON_GOLD_PATH);
                buttonType = "GOLD";
            }
            else
            {
                prefabToUse = AssetDatabase.LoadAssetAtPath<GameObject>(BACK_BUTTON_CYAN_PATH);
                buttonType = "CYAN";
            }

            if (prefabToUse == null)
            {
                Debug.LogError($"❌ {buttonType} BackButton prefab not found");
                return;
            }

            // Find existing BackButton
            Transform existingBackButton = FindExistingBackButton(canvas.transform);
            Transform header = canvas.transform.Find("Header");

            if (existingBackButton != null)
            {
                // Store position
                RectTransform oldRect = existingBackButton.GetComponent<RectTransform>();
                Vector2 oldPosition = oldRect.anchoredPosition;
                Vector2 oldSize = oldRect.sizeDelta;

                // Remove old
                Object.DestroyImmediate(existingBackButton.gameObject);

                // Add new
                GameObject newBackButton = (GameObject)PrefabUtility.InstantiatePrefab(
                    prefabToUse, header ?? canvas.transform);
                RectTransform newRect = newBackButton.GetComponent<RectTransform>();
                newRect.anchoredPosition = oldPosition;
                newRect.sizeDelta = oldSize;

                string icon = buttonType == "GOLD" ? "💎" : "🔷";
                Debug.Log($"{icon} {buttonType} BackButton updated in current scene");
            }
            else
            {
                Debug.LogWarning("⚠️ No existing BackButton found to update");
            }
        }
    }
}
