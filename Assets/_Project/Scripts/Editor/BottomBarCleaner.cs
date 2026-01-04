using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor tool to clean up duplicate/orphaned BottomBar GameObjects
    /// </summary>
    public class BottomBarCleaner : EditorWindow
    {
        [MenuItem("DigitPark/Clean Up BottomBars")]
        public static void ShowWindow()
        {
            GetWindow<BottomBarCleaner>("BottomBar Cleaner");
        }

        [MenuItem("DigitPark/Clean Up BottomBars (All Scenes)")]
        public static void CleanAllScenes()
        {
            if (!EditorUtility.DisplayDialog("Clean All Scenes",
                "This will remove all BottomBar GameObjects from all scenes in Assets/_Project/Scenes/. Continue?",
                "Yes, Clean All", "Cancel"))
            {
                return;
            }

            string[] scenePaths = new string[]
            {
                // Escenas principales
                "Assets/_Project/Scenes/Boot.unity",
                "Assets/_Project/Scenes/Login.unity",
                "Assets/_Project/Scenes/Register.unity",
                "Assets/_Project/Scenes/MainMenu.unity",
                "Assets/_Project/Scenes/Scores.unity",
                "Assets/_Project/Scenes/Settings.unity",
                "Assets/_Project/Scenes/Tournaments.unity",
                "Assets/_Project/Scenes/HowToPlay.unity",
                "Assets/_Project/Scenes/CountrySelector.unity",
                // Escenas nuevas
                "Assets/_Project/Scenes/Profile.unity",
                "Assets/_Project/Scenes/SearchPlayers.unity",
                "Assets/_Project/Scenes/CashBattle.unity",
                // Escenas de juegos
                "Assets/_Project/Scenes/Games/GameSelector.unity",
                "Assets/_Project/Scenes/Games/DigitRush.unity",
                "Assets/_Project/Scenes/Games/MemoryPairs.unity",
                "Assets/_Project/Scenes/Games/QuickMath.unity",
                "Assets/_Project/Scenes/Games/FlashTap.unity",
                "Assets/_Project/Scenes/Games/OddOneOut.unity"
            };

            int totalRemoved = 0;

            foreach (string scenePath in scenePaths)
            {
                if (!System.IO.File.Exists(scenePath)) continue;

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                int removed = CleanCurrentScene();
                totalRemoved += removed;

                if (removed > 0)
                {
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"[BottomBarCleaner] Removed {removed} BottomBars from {scenePath}");
                }
            }

            EditorUtility.DisplayDialog("Cleanup Complete",
                $"Removed {totalRemoved} BottomBar GameObjects from all scenes.",
                "OK");
        }

        private void OnGUI()
        {
            GUILayout.Label("BottomBar Cleanup Tool", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.Label("This tool removes all BottomBar GameObjects from scenes.");
            GUILayout.Label("BottomBars are decorative elements that can accumulate as duplicates.");
            GUILayout.Space(10);

            if (GUILayout.Button("Clean Current Scene"))
            {
                int removed = CleanCurrentScene();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorUtility.DisplayDialog("Cleanup Complete",
                    $"Removed {removed} BottomBar GameObjects from current scene.\n\nDon't forget to save the scene!",
                    "OK");
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Clean ALL Scenes (Auto-save)"))
            {
                CleanAllScenes();
            }

            GUILayout.Space(20);
            GUILayout.Label("Current Scene Stats:", EditorStyles.boldLabel);

            int count = CountBottomBars();
            GUILayout.Label($"BottomBars found: {count}");
        }

        private static int CountBottomBars()
        {
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            return allObjects.Count(go => go.name == "BottomBar" && go.scene.isLoaded);
        }

        private static int CleanCurrentScene()
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

            return removed;
        }
    }
}
