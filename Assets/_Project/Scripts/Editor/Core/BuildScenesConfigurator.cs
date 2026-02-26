using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DigitPark.Editor
{
    /// <summary>
    /// Configura automaticamente el orden de escenas en Build Settings (Player Settings).
    /// Orden logico basado en el flujo de navegacion de la app.
    ///
    /// Menu: DigitPark/Tools/Configure Build Scenes
    /// </summary>
    public class BuildScenesConfigurator : EditorWindow
    {
        private Vector2 scrollPosition;
        private static List<SceneEntry> sceneEntries = new List<SceneEntry>();
        private static bool configured = false;

        // ==================== ORDEN DE ESCENAS ====================
        // El indice 0 es la escena de entrada (Boot)
        // Agrupadas por flujo de navegacion

        private static readonly string[][] SCENE_ORDER = new string[][]
        {
            // === 0: ENTRY POINT ===
            new[] { "_Core/Boot" },

            // === 1-3: AUTH FLOW ===
            new[] { "Auth/Login" },
            new[] { "Auth/Register" },
            new[] { "Auth/AgeVerification" },

            // === 4: ONBOARDING ===
            new[] { "Onboarding/Onboarding" },

            // === 5-7: CORE (hub + settings) ===
            new[] { "_Core/MainMenu" },
            new[] { "_Core/Settings" },
            new[] { "Social/Profile/Profile" },

            // === 8-11: GAME NAVIGATION ===
            new[] { "Games/Navigation/PlayModeSelection" },
            new[] { "Games/Navigation/GameSelector" },
            new[] { "Games/Navigation/BetSelection" },
            new[] { "Games/Navigation/Matchmaking" },

            // === 12-16: GAMES ===
            new[] { "Games/Minigames/DigitRush" },
            new[] { "Games/Minigames/FlashTap" },
            new[] { "Games/Minigames/MemoryPairs" },
            new[] { "Games/Minigames/OddOneOut" },
            new[] { "Games/Minigames/QuickMath" },

            // === 17-22: SOCIAL ===
            new[] { "Social/Profile/Scores" },
            new[] { "Social/Friends/SearchPlayers" },
            new[] { "Social/Friends/Friends" },
            new[] { "Social/Friends/FriendRequests" },
            new[] { "Social/Profile/MatchHistory" },
            new[] { "Social/Notifications/Notifications" },

            // === 23-26: MONETIZATION ===
            new[] { "Monetization/Shop" },
            new[] { "Monetization/DailyMissions" },
            new[] { "Monetization/DailyRewards" },
            new[] { "Monetization/Achievements" },

            // === 27-29: TOURNAMENTS ===
            new[] { "Tournaments/TournamentsBrowser" },
            new[] { "Tournaments/TournamentCreate" },
            new[] { "Tournaments/TournamentLobby" },

            // === 30-36: CASH BATTLE ===
            new[] { "CashBattle/CashBattleHub" },
            new[] { "CashBattle/CashBattle1v1" },
            new[] { "CashBattle/CashTournaments" },
            new[] { "CashBattle/CashWallet" },
            new[] { "CashBattle/CashHistory" },
            new[] { "CashBattle/CashProfile" },
            new[] { "Onboarding/CashBattleOnboarding" },
        };

        private const string SCENES_ROOT = "Assets/_Project/Scenes/";

        private struct SceneEntry
        {
            public int index;
            public string name;
            public string path;
            public bool found;
            public bool enabled;
            public string group;
        }

        // ==================== MENU ====================

        [MenuItem("DigitPark/Tools/Configure Build Scenes", false, 10)]
        public static void ShowWindow()
        {
            var window = GetWindow<BuildScenesConfigurator>("Build Scenes Config");
            window.minSize = new Vector2(550, 600);
            RefreshEntries();
        }

        [MenuItem("DigitPark/Tools/Apply Build Scenes (Quick)", false, 11)]
        public static void QuickApply()
        {
            RefreshEntries();
            ApplyToBuildSettings();
        }

        // ==================== GUI ====================

        private void OnGUI()
        {
            GUILayout.Label("Build Scenes Configurator", EditorStyles.boldLabel);
            GUILayout.Space(5);

            EditorGUILayout.HelpBox(
                "Configura el orden de escenas en Build Settings.\n" +
                "Indice 0 = Boot (entry point). Orden por flujo de navegacion.\n" +
                "37 escenas totales: Auth > Core > Games > Social > Monetization > Tournaments > CashBattle",
                MessageType.Info);

            GUILayout.Space(10);

            // Action buttons
            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
            if (GUILayout.Button("Refresh", GUILayout.Height(35)))
            {
                RefreshEntries();
                Repaint();
            }

            GUI.backgroundColor = new Color(0.3f, 0.7f, 1f);
            if (GUILayout.Button("Apply to Build Settings", GUILayout.Height(35)))
            {
                ApplyToBuildSettings();
                configured = true;
                Repaint();
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            if (configured)
            {
                GUILayout.Space(5);
                EditorGUILayout.HelpBox("Build Settings actualizado correctamente.", MessageType.Info);
            }

            GUILayout.Space(10);

            // Summary
            int found = sceneEntries.Count(e => e.found);
            int total = sceneEntries.Count;
            EditorGUILayout.LabelField($"Escenas: {found}/{total} encontradas", EditorStyles.boldLabel);

            // Current build settings count
            int currentCount = EditorBuildSettings.scenes.Length;
            EditorGUILayout.LabelField($"Build Settings actual: {currentCount} escenas", EditorStyles.miniLabel);

            GUILayout.Space(5);

            // Scene list
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            string currentGroup = "";
            foreach (var entry in sceneEntries)
            {
                if (entry.group != currentGroup)
                {
                    currentGroup = entry.group;
                    GUILayout.Space(8);
                    GUI.color = new Color(0.5f, 0.9f, 1f);
                    GUILayout.Label($"--- {currentGroup} ---", EditorStyles.boldLabel);
                    GUI.color = Color.white;
                }

                EditorGUILayout.BeginHorizontal();

                // Index
                GUI.color = entry.found ? Color.white : Color.red;
                GUILayout.Label($"[{entry.index}]", GUILayout.Width(35));

                // Status
                GUI.color = entry.found ? Color.green : Color.red;
                GUILayout.Label(entry.found ? "OK" : "MISS", EditorStyles.boldLabel, GUILayout.Width(40));

                // Name
                GUI.color = Color.white;
                GUILayout.Label(entry.name, GUILayout.Width(200));

                // Path
                GUI.color = new Color(0.7f, 0.7f, 0.7f);
                GUILayout.Label(entry.path, EditorStyles.miniLabel);

                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        // ==================== LOGIC ====================

        private static void RefreshEntries()
        {
            sceneEntries.Clear();
            int index = 0;

            for (int g = 0; g < SCENE_ORDER.Length; g++)
            {
                string relativePath = SCENE_ORDER[g][0];
                string fullPath = SCENES_ROOT + relativePath + ".unity";
                string sceneName = Path.GetFileNameWithoutExtension(relativePath);
                string group = GetGroupName(relativePath);

                sceneEntries.Add(new SceneEntry
                {
                    index = index,
                    name = sceneName,
                    path = fullPath,
                    found = File.Exists(fullPath),
                    enabled = true,
                    group = group
                });

                index++;
            }

            Debug.Log($"[BuildScenesConfigurator] {sceneEntries.Count(e => e.found)}/{sceneEntries.Count} escenas encontradas");
        }

        private static string GetGroupName(string path)
        {
            string folder = path.Split('/')[0];
            switch (folder)
            {
                case "_Core": return "CORE";
                case "Auth": return "AUTH";
                case "Onboarding":
                    return path.Contains("CashBattle") ? "CASH BATTLE" : "ONBOARDING";
                case "Games":
                    if (path.Contains("Navigation/")) return "GAME NAVIGATION";
                    if (path.Contains("Minigames/")) return "MINIGAMES";
                    return "GAMES";
                case "Social":
                    if (path.Contains("Friends/")) return "SOCIAL - FRIENDS";
                    if (path.Contains("Profile/")) return "SOCIAL - PROFILE";
                    if (path.Contains("Notifications/")) return "SOCIAL - NOTIFICATIONS";
                    return "SOCIAL";
                case "Monetization": return "MONETIZATION";
                case "Tournaments": return "TOURNAMENTS";
                case "CashBattle": return "CASH BATTLE";
                default: return "OTHER";
            }
        }

        private static void ApplyToBuildSettings()
        {
            var buildScenes = new List<EditorBuildSettingsScene>();

            foreach (var entry in sceneEntries)
            {
                if (!entry.found)
                {
                    Debug.LogWarning($"[BuildScenesConfigurator] Escena no encontrada: {entry.path}");
                    continue;
                }

                buildScenes.Add(new EditorBuildSettingsScene(entry.path, entry.enabled));
            }

            EditorBuildSettings.scenes = buildScenes.ToArray();

            int count = buildScenes.Count;
            Debug.Log($"[BuildScenesConfigurator] Build Settings configurado: {count} escenas. Boot = indice 0.");
            EditorUtility.DisplayDialog("Build Scenes Configurado",
                $"{count} escenas asignadas al Build Settings.\n\n" +
                "Orden:\n" +
                "[0] Boot (entry point)\n" +
                "[1-3] Auth (Login, Register, AgeVerification)\n" +
                "[4] Onboarding\n" +
                "[5-7] Core (MainMenu, Settings, Profile)\n" +
                "[8-16] Games\n" +
                "[17-22] Social\n" +
                "[23-26] Monetization\n" +
                "[27-29] Tournaments\n" +
                "[30-36] Cash Battle",
                "OK");
        }
    }
}
