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

        // No gold-button scenes remain after CashBattle removal
        private static readonly HashSet<string> GOLD_SCENES = new HashSet<string>();

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

            // Matchmaking scenes (have cancel button instead)
            "Matchmaking",

            // Onboarding scenes (have own navigation)
            "Onboarding",

            // Boot scene
            "Boot"
        };

        [MenuItem("DigitPark/Polish/UI/BackButtons to All Scenes", false, 200)]
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
                Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
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

                // Determine parent: gold buttons go inside the Header GO for perfect alignment
                // with TitleText and currency pill. Header may live at different depths:
                //   SafeArea > Header
                //   WalletUI > Header  (CashWallet — no SafeArea)
                Transform parent;

                if (buttonType == "GOLD")
                {
                    Transform header = FindHeaderInCanvas(canvas.transform);
                    if (header != null)
                        parent = header;
                    else
                    {
                        Transform safeAreaFallback = canvas.transform.Find("SafeArea");
                        parent = safeAreaFallback ?? canvas.transform;
                    }
                }
                else
                {
                    Transform safeArea = canvas.transform.Find("SafeArea");
                    parent = safeArea ?? canvas.transform;
                }

                // Instantiate BackButton prefab
                GameObject backButtonInstance = (GameObject)PrefabUtility.InstantiatePrefab(backButtonPrefab, parent);
                backButtonInstance.name = buttonType == "GOLD" ? "BackButtonGold" : "BackButton";

                RectTransform backButtonRect = backButtonInstance.GetComponent<RectTransform>();
                if (backButtonRect != null)
                {
                    if (buttonType == "GOLD" && parent.name == "Header")
                    {
                        // Inside Header: anchor left, vertically centred.
                        // pivot (0, 0.5) → anchoredPosition.x = left edge offset, y = 0 = header centre.
                        backButtonRect.anchorMin  = new Vector2(0, 0.5f);
                        backButtonRect.anchorMax  = new Vector2(0, 0.5f);
                        backButtonRect.pivot      = new Vector2(0, 0.5f);
                        backButtonRect.anchoredPosition = new Vector2(15, 0);
                        backButtonRect.sizeDelta  = new Vector2(50, 50);
                    }
                    else
                    {
                        // Fallback (cyan or no Header found): top-left of SafeArea / Canvas
                        backButtonRect.anchorMin  = new Vector2(0, 1);
                        backButtonRect.anchorMax  = new Vector2(0, 1);
                        backButtonRect.pivot      = new Vector2(0, 1);
                        backButtonRect.anchoredPosition = new Vector2(15, -15);
                        backButtonRect.sizeDelta  = new Vector2(50, 50);
                    }
                }

                // Render on top of other Header children
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

        /// <summary>
        /// Finds the Header GO in the canvas regardless of depth.
        /// Checks: direct child, SafeArea>Header, any first-level child>Header.
        /// </summary>
        private static Transform FindHeaderInCanvas(Transform canvas)
        {
            // 1. Direct child of Canvas named "Header"
            Transform direct = canvas.Find("Header");
            if (direct != null) return direct;

            // 2. SafeArea > Header  (CashBattleHub, CashHistory, CashTournaments…)
            Transform safeArea = canvas.Find("SafeArea");
            if (safeArea != null)
            {
                Transform h = safeArea.Find("Header");
                if (h != null) return h;
            }

            // 3. Any immediate child > Header  (WalletUI > Header, etc.)
            foreach (Transform child in canvas)
            {
                Transform h = child.Find("Header");
                if (h != null) return h;
            }

            return null;
        }

        private static Transform FindExistingBackButton(Transform canvas)
        {
            // Try all known locations (including nested under SafeArea > Header)
            string[] searchPaths = new string[]
            {
                "BackButton",
                "BackButtonGold",
                "SafeArea/BackButton",
                "SafeArea/BackButtonGold",
                "SafeArea/Header/BackButton",
                "SafeArea/Header/BackButtonGold",
                "Header/BackButton",
                "Header/BackButtonGold"
            };

            foreach (string path in searchPaths)
            {
                Transform found = canvas.Find(path);
                if (found != null) return found;
            }

            // Recursive fallback
            foreach (var t in canvas.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "BackButton" || t.name == "BackButtonGold")
                    return t;
            }

            return null;
        }

        /// <summary>
        /// Re-places the BackButtonGold in every gold scene with the corrected 88×88 / (15,-45) position.
        /// Use this to fix misaligned gold buttons without touching cyan scenes.
        /// </summary>
        [MenuItem("DigitPark/Polish/UI/Fix Gold BackButton Positions", false, 201)]
        public static void FixGoldBackButtonPositions()
        {
            GameObject goldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BACK_BUTTON_GOLD_PATH);
            if (goldPrefab == null)
            {
                Debug.LogError($"[BackButton] Gold prefab not found: {BACK_BUTTON_GOLD_PATH}");
                return;
            }

            int fixed_ = 0;
            int errors = 0;

            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { SCENES_PATH });
            foreach (string guid in sceneGuids)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(guid);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                if (!GOLD_SCENES.Contains(sceneName)) continue;

                if (AddBackButtonToScene(scenePath, sceneName, goldPrefab, "GOLD"))
                    fixed_++;
                else
                    errors++;
            }

            Debug.Log($"[BackButton] Fix Gold complete — {fixed_} fixed, {errors} errors");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Also remove any leftover BackButtonGold from now-excluded scenes (e.g. CashMatchmaking)
            RemoveBackButtonFromExcludedGoldScenes();

            if (!AllScenesBatchBuilder.SilentMode)
                EditorUtility.DisplayDialog("Fix Gold BackButtons",
                    $"BackButtonGold reposicionado en {fixed_} escenas.\n\n" +
                    "Ubicación: Header (SafeArea>Header o WalletUI>Header)\n" +
                    "Posición: anchor=(0,0.5), anchoredPosition=(15,0), size=(50,50)\n" +
                    "Centrado verticalmente con TitleText y currency pill.",
                    "OK");
        }

        // Scenes that were previously gold but now excluded — remove any leftover BackButtonGold
        private static readonly HashSet<string> FORMERLY_GOLD_EXCLUDED = new HashSet<string>
        {
            "CashMatchmaking"
        };

        private static void RemoveBackButtonFromExcludedGoldScenes()
        {
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { SCENES_PATH });
            foreach (string guid in sceneGuids)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(guid);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                if (!FORMERLY_GOLD_EXCLUDED.Contains(sceneName)) continue;

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
                if (canvas == null) continue;

                Transform existing = FindExistingBackButton(canvas.transform);
                if (existing != null)
                {
                    Object.DestroyImmediate(existing.gameObject);
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"[BackButton] Removed BackButtonGold from {sceneName} (excluded scene)");
                }
            }
        }

        [MenuItem("DigitPark/Polish/UI/Remove All BackButtons", false, 202)]
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
                Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();

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

        [MenuItem("DigitPark/Polish/UI/BackButton to Current Scene", false, 203)]
        public static void AddBackButtonToCurrentScene()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("No Canvas found in current scene");
                return;
            }

            var scene = EditorSceneManager.GetActiveScene();
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scene.path);

            if (EXCLUDED_SCENES.Contains(sceneName))
            {
                Debug.LogWarning($"{sceneName} is in the excluded list (no back button needed)");
                return;
            }

            // Auto-detect gold or cyan
            bool isGold = GOLD_SCENES.Contains(sceneName);
            string buttonType = isGold ? "GOLD" : "CYAN";
            string prefabPath = isGold ? BACK_BUTTON_GOLD_PATH : BACK_BUTTON_CYAN_PATH;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"{buttonType} BackButton prefab not found at {prefabPath}");
                return;
            }

            // Remove existing if any
            Transform existing = FindExistingBackButton(canvas.transform);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
                Debug.Log($"Removed old BackButton from {sceneName}");
            }

            // Determine parent (same logic as batch method)
            Transform parent;

            if (isGold)
            {
                Transform header = FindHeaderInCanvas(canvas.transform);
                if (header != null)
                    parent = header;
                else
                {
                    Transform safeAreaFallback = canvas.transform.Find("SafeArea");
                    parent = safeAreaFallback ?? canvas.transform;
                }
            }
            else
            {
                Transform safeArea2 = canvas.transform.Find("SafeArea");
                parent = safeArea2 ?? canvas.transform;
            }

            // Instantiate
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = isGold ? "BackButtonGold" : "BackButton";

            RectTransform rt = instance.GetComponent<RectTransform>();
            if (rt != null)
            {
                if (isGold && parent.name == "Header")
                {
                    rt.anchorMin  = new Vector2(0, 0.5f);
                    rt.anchorMax  = new Vector2(0, 0.5f);
                    rt.pivot      = new Vector2(0, 0.5f);
                    rt.anchoredPosition = new Vector2(15, 0);
                    rt.sizeDelta  = new Vector2(50, 50);
                }
                else
                {
                    rt.anchorMin  = new Vector2(0, 1);
                    rt.anchorMax  = new Vector2(0, 1);
                    rt.pivot      = new Vector2(0, 1);
                    rt.anchoredPosition = new Vector2(15, -15);
                    rt.sizeDelta  = new Vector2(50, 50);
                }
            }

            instance.transform.SetAsLastSibling();

            EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log($"[BackButton] {buttonType} BackButton added to {sceneName}");
        }
    }
}
