using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace DigitPark.Editor
{
    /// <summary>
    /// Batch builder that opens each CashBattle scene, runs the UIBuilder silently
    /// (no confirmation dialogs), and saves the scene.
    ///
    /// Menu: DigitPark/Tools/Batch Build All CashBattle Scenes
    /// </summary>
    public class CashBattleScenesBatchBuilder : EditorWindow
    {
        private Vector2 scrollPosition;
        private static List<BuildResult> results = new List<BuildResult>();
        private static bool isRunning = false;

        private struct BuildResult
        {
            public string sceneName;
            public string status;
            public bool success;
        }

        private const string SCENES_ROOT = "Assets/_Project/Scenes/CashBattle/";

        // Scene name → UIBuilder class that has BuildSilent()
        private static readonly string[][] CASH_SCENES = new string[][]
        {
            // { sceneName, builderClassName }
            new[] { "CashBattleHub", "CashBattleUIBuilder" },
            new[] { "CashBattle1v1", "CashBattle1v1UIBuilder" },
            new[] { "CashTournaments/CashTournaments", "CashTournamentsUIBuilder" },
            new[] { "CashWallet", "WalletUIBuilder" },
            new[] { "CashHistory", "CashHistoryUIBuilder" },
            new[] { "CashProfile", "CashProfileUIBuilder" },
            new[] { "CashMatchmaking", "CashMatchmakingUIBuilder" },
            new[] { "CashTournaments/CashTournamentCreate", "CashTournamentCreateUIBuilder" },
            new[] { "CashTournaments/CashTournamentLobby", "CashTournamentLobbyUIBuilder" },
        };

        [MenuItem("DigitPark/Scenes/Batch/Build CashBattle Scenes", false, 15)]
        public static void ShowWindow()
        {
            var window = GetWindow<CashBattleScenesBatchBuilder>("CashBattle Batch Builder");
            window.minSize = new Vector2(550, 500);
        }

        private void OnGUI()
        {
            GUILayout.Label("CashBattle Scenes Batch Builder", EditorStyles.boldLabel);
            GUILayout.Space(5);

            EditorGUILayout.HelpBox(
                "Ejecuta el UIBuilder de cada escena CashBattle sin interrupciones.\n" +
                "Abre cada escena, construye la UI, y guarda automaticamente.\n\n" +
                $"{CASH_SCENES.Length} escenas CashBattle + 1 panel overlay (Results)",
                MessageType.Info);

            GUILayout.Space(10);

            // List of scenes
            GUILayout.Label("Escenas a construir:", EditorStyles.boldLabel);
            foreach (var scene in CASH_SCENES)
            {
                EditorGUILayout.LabelField($"  - {scene[0]} ({scene[1]})");
            }
            EditorGUILayout.LabelField("  - CashTournamentResults (panel overlay)");

            GUILayout.Space(10);

            EditorGUI.BeginDisabledGroup(isRunning);

            GUI.backgroundColor = new Color(1f, 0.84f, 0f);
            if (GUILayout.Button("CONSTRUIR TODAS LAS ESCENAS", GUILayout.Height(45)))
            {
                results.Clear();
                BuildAllScenes();
                Repaint();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(5);

            GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
            if (GUILayout.Button("Solo asignar referencias (sin reconstruir UI)", GUILayout.Height(30)))
            {
                results.Clear();
                AssignAllReferences();
                Repaint();
            }
            GUI.backgroundColor = Color.white;

            EditorGUI.EndDisabledGroup();

            GUILayout.Space(10);

            // Results
            if (results.Count > 0)
            {
                DrawResults();
            }
        }

        private static void BuildAllScenes()
        {
            isRunning = true;
            string originalScene = EditorSceneManager.GetActiveScene().path;
            int successCount = 0;
            int failCount = 0;

            try
            {
                for (int i = 0; i < CASH_SCENES.Length; i++)
                {
                    string sceneName = CASH_SCENES[i][0];
                    string builderName = CASH_SCENES[i][1];
                    string scenePath = SCENES_ROOT + sceneName + ".unity";

                    EditorUtility.DisplayProgressBar(
                        "Building CashBattle Scenes",
                        $"[{i + 1}/{CASH_SCENES.Length}] {sceneName}...",
                        (float)i / CASH_SCENES.Length);

                    if (!System.IO.File.Exists(scenePath))
                    {
                        results.Add(new BuildResult
                        {
                            sceneName = sceneName,
                            status = "Scene file not found",
                            success = false
                        });
                        failCount++;
                        continue;
                    }

                    try
                    {
                        // Open scene
                        EditorSceneManager.OpenScene(scenePath);

                        // Find and call BuildSilent() via reflection
                        bool built = CallBuildSilent(builderName);

                        if (built)
                        {
                            // Save the scene
                            EditorSceneManager.SaveOpenScenes();
                            results.Add(new BuildResult
                            {
                                sceneName = sceneName,
                                status = "Built + Saved",
                                success = true
                            });
                            successCount++;
                        }
                        else
                        {
                            results.Add(new BuildResult
                            {
                                sceneName = sceneName,
                                status = "BuildSilent() not found",
                                success = false
                            });
                            failCount++;
                        }
                    }
                    catch (System.Exception e)
                    {
                        results.Add(new BuildResult
                        {
                            sceneName = sceneName,
                            status = $"Error: {e.Message}",
                            success = false
                        });
                        failCount++;
                        Debug.LogError($"[BatchBuilder] Error building {sceneName}: {e}");
                    }
                }

                // Build Results Panel (overlay - doesn't need a scene)
                try
                {
                    bool builtResults = CallBuildSilent("CashTournamentResultsUIBuilder");
                    results.Add(new BuildResult
                    {
                        sceneName = "CashTournamentResults (panel)",
                        status = builtResults ? "Built" : "BuildSilent() not found",
                        success = builtResults
                    });
                    if (builtResults) successCount++;
                    else failCount++;
                }
                catch (System.Exception e)
                {
                    results.Add(new BuildResult
                    {
                        sceneName = "CashTournamentResults",
                        status = $"Error: {e.Message}",
                        success = false
                    });
                    failCount++;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                isRunning = false;

                // Return to original scene
                if (!string.IsNullOrEmpty(originalScene) && System.IO.File.Exists(originalScene))
                {
                    EditorSceneManager.OpenScene(originalScene);
                }

                Debug.Log($"[BatchBuilder] Completado: {successCount} exitosos, {failCount} fallidos de {CASH_SCENES.Length + 1} total");
            }
        }

        private static void AssignAllReferences()
        {
            isRunning = true;
            string originalScene = EditorSceneManager.GetActiveScene().path;

            // Reference assigner class names follow pattern: {SceneName}ReferenceAssigner
            string[][] assigners = new string[][]
            {
                new[] { "CashBattle1v1", "CashBattle1v1ReferenceAssigner" },
                new[] { "CashMatchmaking", "CashMatchmakingReferenceAssigner" },
                new[] { "CashTournaments/CashTournamentCreate", "CashTournamentCreateReferenceAssigner" },
                new[] { "CashTournaments/CashTournamentLobby", "CashTournamentLobbyReferenceAssigner" },
            };

            try
            {
                for (int i = 0; i < assigners.Length; i++)
                {
                    string sceneName = assigners[i][0];
                    string assignerName = assigners[i][1];
                    string scenePath = SCENES_ROOT + sceneName + ".unity";

                    EditorUtility.DisplayProgressBar(
                        "Assigning References",
                        $"[{i + 1}/{assigners.Length}] {sceneName}...",
                        (float)i / assigners.Length);

                    if (!System.IO.File.Exists(scenePath))
                    {
                        results.Add(new BuildResult
                        {
                            sceneName = sceneName,
                            status = "Scene not found",
                            success = false
                        });
                        continue;
                    }

                    try
                    {
                        EditorSceneManager.OpenScene(scenePath);
                        bool assigned = CallAssignReferences(assignerName);
                        EditorSceneManager.SaveOpenScenes();

                        results.Add(new BuildResult
                        {
                            sceneName = sceneName,
                            status = assigned ? "References assigned" : "Assigner not found",
                            success = assigned
                        });
                    }
                    catch (System.Exception e)
                    {
                        results.Add(new BuildResult
                        {
                            sceneName = sceneName,
                            status = $"Error: {e.Message}",
                            success = false
                        });
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                isRunning = false;

                if (!string.IsNullOrEmpty(originalScene) && System.IO.File.Exists(originalScene))
                {
                    EditorSceneManager.OpenScene(originalScene);
                }
            }
        }

        /// <summary>
        /// Finds a UIBuilder class by name and calls its BuildSilent() static method via reflection
        /// </summary>
        private static bool CallBuildSilent(string className)
        {
            // Search all assemblies for the class
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.Name == className)
                    {
                        var method = type.GetMethod("BuildSilent",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                        if (method != null)
                        {
                            method.Invoke(null, null);
                            return true;
                        }
                        else
                        {
                            Debug.LogWarning($"[BatchBuilder] {className} does not have BuildSilent() method");
                            return false;
                        }
                    }
                }
            }

            Debug.LogWarning($"[BatchBuilder] Class {className} not found in any assembly");
            return false;
        }

        /// <summary>
        /// Finds a reference assigner class and calls its RunAutoAssign() or equivalent
        /// </summary>
        private static bool CallAssignReferences(string className)
        {
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.Name == className)
                    {
                        // Try common method names for auto-assign
                        string[] methodNames = { "RunAutoAssign", "AssignAllReferences", "AutoAssign" };
                        foreach (var methodName in methodNames)
                        {
                            var method = type.GetMethod(methodName,
                                System.Reflection.BindingFlags.Public |
                                System.Reflection.BindingFlags.NonPublic |
                                System.Reflection.BindingFlags.Static);

                            if (method != null)
                            {
                                method.Invoke(null, null);
                                return true;
                            }
                        }

                        Debug.LogWarning($"[BatchBuilder] {className} does not have a recognized assign method");
                        return false;
                    }
                }
            }

            Debug.LogWarning($"[BatchBuilder] Class {className} not found");
            return false;
        }

        private void DrawResults()
        {
            GUILayout.Label("Resultados:", EditorStyles.boldLabel);

            int success = 0;
            int fail = 0;
            foreach (var r in results)
            {
                if (r.success) success++;
                else fail++;
            }

            float rate = results.Count > 0 ? (float)success / results.Count : 0;
            GUI.color = rate == 1f ? new Color(0.2f, 0.8f, 0.2f) :
                        rate >= 0.7f ? new Color(1f, 0.8f, 0.2f) : new Color(1f, 0.4f, 0.4f);
            GUILayout.Label($"Exitosos: {success} | Fallidos: {fail} | Total: {results.Count}");
            GUI.color = Color.white;

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(250));

            foreach (var r in results)
            {
                EditorGUILayout.BeginHorizontal();
                GUI.color = r.success ? Color.green : Color.red;
                GUILayout.Label(r.success ? "OK" : "FAIL", EditorStyles.boldLabel, GUILayout.Width(40));
                GUI.color = Color.white;
                GUILayout.Label(r.sceneName, GUILayout.Width(200));
                GUILayout.Label(r.status);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
