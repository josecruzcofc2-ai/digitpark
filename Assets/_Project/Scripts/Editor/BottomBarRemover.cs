using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor tool to remove ALL BottomBar GameObjects from all scenes in the project
    /// This is a one-time cleanup tool to replace BottomBars with NeonButtonGlow
    /// </summary>
    public class BottomBarRemover : EditorWindow
    {
        private Vector2 scrollPosition;
        private Dictionary<string, int> sceneResults = new Dictionary<string, int>();
        private bool isProcessing = false;

        // All scenes in the project
        private static readonly string[] ALL_SCENES = new string[]
        {
            // Main scenes
            "Assets/_Project/Scenes/Boot.unity",
            "Assets/_Project/Scenes/Login.unity",
            "Assets/_Project/Scenes/Register.unity",
            "Assets/_Project/Scenes/MainMenu.unity",
            "Assets/_Project/Scenes/Scores.unity",
            "Assets/_Project/Scenes/Settings.unity",
            "Assets/_Project/Scenes/Tournaments.unity",
            "Assets/_Project/Scenes/HowToPlay.unity",
            "Assets/_Project/Scenes/CountrySelector.unity",
            "Assets/_Project/Scenes/Profile.unity",
            "Assets/_Project/Scenes/SearchPlayers.unity",
            "Assets/_Project/Scenes/CashBattle.unity",
            // Game scenes
            "Assets/_Project/Scenes/Games/GameSelector.unity",
            "Assets/_Project/Scenes/Games/DigitRush.unity",
            "Assets/_Project/Scenes/Games/MemoryPairs.unity",
            "Assets/_Project/Scenes/Games/QuickMath.unity",
            "Assets/_Project/Scenes/Games/FlashTap.unity",
            "Assets/_Project/Scenes/Games/OddOneOut.unity"
        };

        [MenuItem("DigitPark/Cleanup/Remove All BottomBars")]
        public static void ShowWindow()
        {
            var window = GetWindow<BottomBarRemover>("BottomBar Remover");
            window.minSize = new Vector2(400, 500);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("BottomBar Removal Tool", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox(
                "This tool will remove ALL GameObjects named 'BottomBar' from all scenes.\n\n" +
                "BottomBars are being replaced with NeonButtonGlow component for a cleaner neon aesthetic.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Current scene section
            EditorGUILayout.LabelField("Current Scene", EditorStyles.boldLabel);

            int currentCount = CountBottomBarsInCurrentScene();
            EditorGUILayout.LabelField($"BottomBars found: {currentCount}");

            using (new EditorGUI.DisabledGroupScope(currentCount == 0 || isProcessing))
            {
                if (GUILayout.Button("Remove from Current Scene", GUILayout.Height(30)))
                {
                    int removed = RemoveFromCurrentScene();
                    EditorUtility.DisplayDialog("Complete",
                        $"Removed {removed} BottomBar(s) from current scene.\n\nRemember to save the scene!",
                        "OK");
                }
            }

            EditorGUILayout.Space(20);

            // All scenes section
            EditorGUILayout.LabelField("All Scenes", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledGroupScope(isProcessing))
            {
                if (GUILayout.Button("Scan All Scenes", GUILayout.Height(25)))
                {
                    ScanAllScenes();
                }

                EditorGUILayout.Space(5);

                GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
                if (GUILayout.Button("REMOVE FROM ALL SCENES (Auto-save)", GUILayout.Height(35)))
                {
                    if (EditorUtility.DisplayDialog("Confirm Removal",
                        "This will:\n\n" +
                        "1. Open each scene\n" +
                        "2. Remove all BottomBar GameObjects\n" +
                        "3. Save and close the scene\n\n" +
                        "This action cannot be undone easily.\n\nContinue?",
                        "Yes, Remove All", "Cancel"))
                    {
                        RemoveFromAllScenes();
                    }
                }
                GUI.backgroundColor = Color.white;
            }

            // Results section
            if (sceneResults.Count > 0)
            {
                EditorGUILayout.Space(15);
                EditorGUILayout.LabelField("Results", EditorStyles.boldLabel);

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));

                int totalRemoved = 0;
                foreach (var result in sceneResults)
                {
                    string sceneName = System.IO.Path.GetFileNameWithoutExtension(result.Key);
                    string status = result.Value > 0 ? $"{result.Value} removed" : "Clean";
                    Color color = result.Value > 0 ? Color.yellow : Color.green;

                    EditorGUILayout.BeginHorizontal();
                    GUI.color = color;
                    EditorGUILayout.LabelField($"  {sceneName}: {status}");
                    GUI.color = Color.white;
                    EditorGUILayout.EndHorizontal();

                    totalRemoved += result.Value;
                }

                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField($"Total BottomBars removed: {totalRemoved}", EditorStyles.boldLabel);
            }
        }

        private int CountBottomBarsInCurrentScene()
        {
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            return allObjects.Count(go =>
                go.name == "BottomBar" &&
                go.scene.isLoaded &&
                !EditorUtility.IsPersistent(go));
        }

        private int RemoveFromCurrentScene()
        {
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            var bottomBars = allObjects.Where(go =>
                go.name == "BottomBar" &&
                go.scene.isLoaded &&
                !EditorUtility.IsPersistent(go)).ToList();

            int removed = 0;
            foreach (var bar in bottomBars)
            {
                if (bar != null)
                {
                    Undo.DestroyObjectImmediate(bar);
                    removed++;
                }
            }

            if (removed > 0)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }

            return removed;
        }

        private void ScanAllScenes()
        {
            sceneResults.Clear();
            string currentScenePath = EditorSceneManager.GetActiveScene().path;

            foreach (string scenePath in ALL_SCENES)
            {
                if (!System.IO.File.Exists(scenePath))
                {
                    sceneResults[scenePath] = -1; // Not found
                    continue;
                }

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                int count = CountBottomBarsInCurrentScene();
                sceneResults[scenePath] = count;
            }

            // Return to original scene
            if (!string.IsNullOrEmpty(currentScenePath) && System.IO.File.Exists(currentScenePath))
            {
                EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
            }

            Repaint();
        }

        private void RemoveFromAllScenes()
        {
            isProcessing = true;
            sceneResults.Clear();

            string currentScenePath = EditorSceneManager.GetActiveScene().path;
            int totalRemoved = 0;

            try
            {
                for (int i = 0; i < ALL_SCENES.Length; i++)
                {
                    string scenePath = ALL_SCENES[i];

                    EditorUtility.DisplayProgressBar("Removing BottomBars",
                        $"Processing: {System.IO.Path.GetFileName(scenePath)}",
                        (float)i / ALL_SCENES.Length);

                    if (!System.IO.File.Exists(scenePath))
                    {
                        sceneResults[scenePath] = -1;
                        continue;
                    }

                    var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    int removed = RemoveFromCurrentSceneNoUndo();
                    sceneResults[scenePath] = removed;
                    totalRemoved += removed;

                    if (removed > 0)
                    {
                        EditorSceneManager.SaveScene(scene);
                        Debug.Log($"[BottomBarRemover] Removed {removed} BottomBars from {scenePath}");
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                isProcessing = false;

                // Return to original scene
                if (!string.IsNullOrEmpty(currentScenePath) && System.IO.File.Exists(currentScenePath))
                {
                    EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
                }
            }

            EditorUtility.DisplayDialog("Complete",
                $"Removed {totalRemoved} BottomBar(s) from {sceneResults.Count(r => r.Value > 0)} scenes.",
                "OK");

            Repaint();
        }

        private int RemoveFromCurrentSceneNoUndo()
        {
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            var bottomBars = allObjects.Where(go =>
                go.name == "BottomBar" &&
                go.scene.isLoaded &&
                !EditorUtility.IsPersistent(go)).ToList();

            int removed = 0;
            foreach (var bar in bottomBars)
            {
                if (bar != null)
                {
                    DestroyImmediate(bar);
                    removed++;
                }
            }

            return removed;
        }
    }
}
