// ============================================================
// AllScenesBatchBuilder.cs
// Opens every scene in the app, runs its UIBuilder silently
// (no confirmation dialogs), saves the scene, and reports results.
//
// Groups mirror the Scenes/ folder structure:
//   Core > Auth > Onboarding > Games/Navigation > Games/Minigames >
//   Social/Profile > Social/Friends > Social/Notifications >
//   Monetization > Tournaments > Prefab Builders
//
// Menu: DigitPark/Tools/Batch Build All Scenes
// ============================================================
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DigitPark.Editor
{
    public class AllScenesBatchBuilder : EditorWindow
    {
        /// <summary>
        /// Set to true while batch building to suppress dialogs in UIBuilders.
        /// UIBuilders can check this flag to skip EditorUtility.DisplayDialog calls.
        /// </summary>
        public static bool SilentMode { get; set; } = false;

        private Vector2 scrollPosition;
        private static List<BuildResult> results = new List<BuildResult>();
        private static bool isRunning = false;

        private struct BuildResult
        {
            public string group;
            public string displayName;
            public string status;
            public bool success;
        }

        // ==================== ENTRY DEFINITION ====================

        private struct Entry
        {
            public string group;        // Display group (mirrors Scenes/ folder)
            public string scene;        // Scene path (null for prefab builders)
            public string displayName;  // Human-readable name
            public string className;    // UIBuilder class name
            public string method;       // Fallback method if BuildSilent() not found
        }

        private static Entry E(string group, string scene, string displayName, string className, string method)
        {
            return new Entry { group = group, scene = scene, displayName = displayName, className = className, method = method };
        }

        private const string S = "Assets/_Project/Scenes/";

        // ==================== ALL SCENES + PREFAB BUILDERS ====================
        // Order mirrors the Scenes/ folder structure exactly

        private static readonly Entry[] ALL_ENTRIES = new Entry[]
        {
            // ── CORE ──
            E("Core",       S + "_Core/Boot.unity",            "Boot",             "BootUIBuilder",                "RebuildBootScene"),
            E("Core",       S + "_Core/MainMenu.unity",        "MainMenu",         "MainMenuUIBuilder",            "RebuildMainMenu"),
            E("Core",       S + "_Core/Settings.unity",        "Settings",         "SettingsUIBuilder",            "BuildSettingsUI"),

            // ── AUTH ──
            E("Auth",       S + "Auth/Login.unity",            "Login",            "LoginUIBuilder",               "RebuildLoginScene"),
            E("Auth",       S + "Auth/Register.unity",         "Register",         "RegisterUIBuilder",            "RebuildRegisterScene"),
            // ── ONBOARDING ──
            E("Onboarding", S + "Onboarding/Onboarding.unity",             "Onboarding",             "OnboardingUIBuilder",             "RebuildOnboarding"),

            // ── GAMES - NAVIGATION ──
            E("Games / Navigation", S + "Games/Navigation/GameSelector.unity",      "GameSelector",       "GameSelectorUIBuilder",          "RebuildGameSelectorUI"),
            E("Games / Navigation", S + "Games/Navigation/PlayModeSelection.unity", "PlayModeSelection",  "PlayModeSelectionUIBuilder",     "BuildPlayModeSelectionUI"),
            E("Games / Navigation", S + "Games/Navigation/BetSelection.unity",      "BetSelection",       "BetSelectionUIBuilder",          "BuildScene"),
            E("Games / Navigation", S + "Games/Navigation/Matchmaking.unity",       "Matchmaking",        "MatchmakingUIBuilder",           "BuildUI"),

            // ── GAMES - MINIGAMES ──
            E("Games / Minigames", S + "Games/Minigames/DigitRush.unity",    "DigitRush",    "DigitRushUIBuilder",    "RebuildDigitRushUI"),
            E("Games / Minigames", S + "Games/Minigames/FlashTap.unity",     "FlashTap",     "FlashTapUIBuilder",     "RebuildFlashTapUI"),
            E("Games / Minigames", S + "Games/Minigames/MemoryPairs.unity",  "MemoryPairs",  "MemoryPairsUIBuilder",  "RebuildMemoryPairsUI"),
            E("Games / Minigames", S + "Games/Minigames/OddOneOut.unity",    "OddOneOut",    "OddOneOutUIBuilder",    "RebuildOddOneOutUI"),
            E("Games / Minigames", S + "Games/Minigames/QuickMath.unity",    "QuickMath",    "QuickMathUIBuilder",    "RebuildQuickMathUI"),

            // ── SOCIAL - PROFILE ──
            E("Social / Profile", S + "Social/Profile/Profile.unity",      "Profile",      "ProfileUIBuilder",       "RebuildProfile"),
            E("Social / Profile", S + "Social/Profile/Scores.unity",       "Scores",       "ScoresUIBuilder",        "RebuildScoresUI"),
            E("Social / Profile", S + "Social/Profile/MatchHistory.unity", "MatchHistory", "MatchHistoryUIBuilder",  "RebuildMatchHistory"),

            // ── SOCIAL - FRIENDS ──
            E("Social / Friends", S + "Social/Friends/Friends.unity",        "Friends",        "FriendsUIBuilder",          "RebuildFriends"),
            E("Social / Friends", S + "Social/Friends/FriendRequests.unity", "FriendRequests", "FriendRequestsUIBuilder",   "RebuildFriendRequests"),
            E("Social / Friends", S + "Social/Friends/SearchPlayers.unity",  "SearchPlayers",  "SearchPlayersUIBuilder",    "RebuildSearchPlayersUI"),

            // ── SOCIAL - NOTIFICATIONS ──
            E("Social / Notifications", S + "Social/Notifications/Notifications.unity", "Notifications", "NotificationsUIBuilder", "RebuildNotifications"),

            // ── MONETIZATION ──
            E("Monetization", S + "Monetization/Shop.unity",          "Shop",          "ShopPremiumUIBuilder",          "BuildCompleteUI"),
            E("Monetization", S + "Monetization/DailyMissions.unity", "DailyMissions", "DailyMissionsUIBuilder",        "RebuildMissions"),
            E("Monetization", S + "Monetization/DailyRewards.unity",  "DailyRewards",  "DailyRewardsPremiumUIBuilder",  "RebuildDailyRewards"),
            E("Monetization", S + "Monetization/Achievements.unity",  "Achievements",  "AchievementsUIBuilder",         "BuildTrophyShowcase"),

            // ── TOURNAMENTS ──
            E("Tournaments", S + "Tournaments/TournamentsBrowser.unity", "TournamentsBrowser", "TournamentsBrowserUIBuilder", "BuildCompleteUI"),
            E("Tournaments", S + "Tournaments/TournamentCreate.unity",   "TournamentCreate",   "TournamentCreateUIBuilder",   "BuildCompleteUI"),
            E("Tournaments", S + "Tournaments/TournamentLobby.unity",    "TournamentLobby",    "TournamentLobbyUIBuilder",    "BuildCompleteUI"),

            // ── PREFAB BUILDERS (no scene required) ──
            // Win Panels
            E("Prefabs / Win Panels", null, "WinPanel Normal (Win)",       "WinPanelUIBuilder",              "CreateNormalWinPanel"),
            E("Prefabs / Win Panels", null, "WinPanel Normal (Lose)",      "WinPanelUIBuilder",              "CreateNormalLosePanel"),
            E("Prefabs / Win Panels", null, "WinPanel RealMoney (Win)",    "WinPanelUIBuilder",              "CreateRealMoneyWinPanel"),
            E("Prefabs / Win Panels", null, "WinPanel RealMoney (Lose)",   "WinPanelUIBuilder",              "CreateLosePanel"),

            // Online Result Panels
            E("Prefabs / Result Panels", null, "Online Result (Win)",       "OnlineResultPanelUIBuilder",     "BuildWinPanel"),
            E("Prefabs / Result Panels", null, "Online Result (Lose)",      "OnlineResultPanelUIBuilder",     "BuildLosePanel"),
            E("Prefabs / Result Panels", null, "Tournament Result (Win)",   "TournamentResultPanelUIBuilder", "BuildWinPanel"),
            E("Prefabs / Result Panels", null, "Tournament Result (Lose)",  "TournamentResultPanelUIBuilder", "BuildLosePanel"),
            E("Prefabs / Result Panels", null, "Sprint Summary Panel",      "SprintSummaryPanelUIBuilder",    "BuildSprintSummaryPanel"),

            // Toast / Notification Prefabs
            E("Prefabs / Toasts", null, "Achievement Toast",          "AchievementToastUIBuilder",      "CreateAchievementToastPrefab"),
            E("Prefabs / Toasts", null, "In-App Toast",               "InAppToastUIBuilder",            "CreateInAppToastPrefab"),

            // Monetization & Item Prefabs
            E("Prefabs / Items", null, "Monetization Prefabs (All)",  "MonetizationPrefabBuilder", "CreateAllPrefabs"),
            E("Prefabs / Items", null, "Mission Card",                "MonetizationPrefabBuilder", "CreateMissionCardPrefab"),
            E("Prefabs / Items", null, "Tournament Item",             "MonetizationPrefabBuilder", "CreateTournamentItemPrefab"),
            E("Prefabs / Items", null, "Tournament Search Item",      "MonetizationPrefabBuilder", "CreateTournamentSearchItemPrefab"),
            E("Prefabs / Items", null, "Tournament My Item",          "MonetizationPrefabBuilder", "CreateTournamentMyItemPrefab"),
            E("Prefabs / Items", null, "Leaderboard Entry",           "MonetizationPrefabBuilder", "CreateLeaderboardEntryPrefab"),

            // Social Item Prefabs (builders live inside scene UIBuilders)
            E("Prefabs / Social", null, "Friend Card",         "FriendsUIBuilder",        "CreateFriendCardPrefab"),
            E("Prefabs / Social", null, "Request Item",        "FriendRequestsUIBuilder", "CreateRequestItemPrefab"),
            E("Prefabs / Social", null, "Notification Card",   "NotificationsUIBuilder",  "CreateNotificationCardPrefab"),
            E("Prefabs / Social", null, "Match History Entry", "MatchHistoryUIBuilder",   "CreateMatchEntryPrefab"),
            E("Prefabs / Social", null, "Player Card",         "SearchPlayersUIBuilder",  "CreatePlayerCardPrefab"),

            // Monetization extras (builders live inside scene UIBuilders)

            // Progression Prefabs
            E("Prefabs / Progression", null, "LevelUp Panel",          "LevelUpPanelBuilder",  "BuildPrefab"),

            // Animation Prefabs (Button3D, TransitionCanvas, UIAnimationManager)
            E("Prefabs / Animations",  null, "Animation System Prefabs", "AnimationSystemBuilder", "CreateAllPrefabs"),

            // Shop ScriptableObject Assets
            E("Assets / Shop", null, "Background Shop Items (13)",  "BackgroundShopItemBuilder",   "BuildAll"),
            E("Assets / Shop", null, "Shop Catalog — All Items (~89)", "CreateAllShopItemsCatalog", "CreateAll"),

            // Cosmetics ScriptableObject Assets
            E("Assets / Cosmetics", null, "BattleCard Catalog (19)", "BattleCardCatalogBuilder", "BuildAll"),
        };


        // ==================== MENU ====================

        [MenuItem("DigitPark/Scenes/Batch/Build ALL Scenes", false, 14)]
        public static void ShowWindow()
        {
            var window = GetWindow<AllScenesBatchBuilder>("Build All Scenes");
            window.minSize = new Vector2(600, 700);
        }

        // ==================== GUI ====================

        private void OnGUI()
        {
            GUILayout.Label("Batch Build All Scenes", EditorStyles.boldLabel);
            GUILayout.Space(5);

            // Count scenes vs prefabs
            int sceneCount = ALL_ENTRIES.Count(e => e.scene != null);
            int prefabCount = ALL_ENTRIES.Count(e => e.scene == null);

            EditorGUILayout.HelpBox(
                $"Builds every scene and prefab in the app without dialog interruptions.\n" +
                $"Opens each scene, runs its UIBuilder silently, and saves.\n\n" +
                $"{sceneCount} scenes + {prefabCount} prefab/asset builders = {ALL_ENTRIES.Length} total entries",
                MessageType.Info);

            GUILayout.Space(10);

            EditorGUI.BeginDisabledGroup(isRunning);

            // === BUILD ALL (Scenes + Prefabs) ===
            GUI.backgroundColor = new Color(0.3f, 0.85f, 0.4f);
            if (GUILayout.Button("★  BUILD ALL SCENES + PREFABS", GUILayout.Height(50)))
            {
                results.Clear();
                BuildAll();
                Repaint();
            }
            GUI.backgroundColor = Color.white;


            GUILayout.Space(5);

            // === BUILD SCENES ONLY / PREFABS ONLY ===
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.3f, 0.7f, 1f);
            if (GUILayout.Button("Scenes Only", GUILayout.Height(28)))
            {
                results.Clear();
                BuildFiltered(scenesOnly: true);
                Repaint();
            }

            GUI.backgroundColor = new Color(1f, 0.84f, 0f);
            if (GUILayout.Button("Prefabs Only", GUILayout.Height(28)))
            {
                results.Clear();
                BuildFiltered(scenesOnly: false);
                Repaint();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            // === PER-GROUP BUTTONS ===
            DrawGroupButtons();

            EditorGUI.EndDisabledGroup();

            GUILayout.Space(10);

            // === SCENE LIST ===
            DrawSceneList();

            // === RESULTS ===
            if (results.Count > 0)
            {
                GUILayout.Space(10);
                DrawResults();
            }
        }

        private void DrawGroupButtons()
        {
            var groups = ALL_ENTRIES.Select(e => e.group).Distinct().ToList();

            GUILayout.Space(5);
            GUILayout.Label("Build by Group:", EditorStyles.boldLabel);

            // Layout groups in rows of 3
            int col = 0;
            EditorGUILayout.BeginHorizontal();
            foreach (var group in groups)
            {
                if (col > 0 && col % 3 == 0)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }

                bool isPrefab = group.StartsWith("Prefabs");
                GUI.backgroundColor = isPrefab ? new Color(1f, 0.9f, 0.6f) : new Color(0.7f, 0.85f, 1f);
                if (GUILayout.Button(group, GUILayout.Height(22)))
                {
                    results.Clear();
                    BuildGroup(group);
                    Repaint();
                }
                col++;
            }
            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
        }

        private void DrawSceneList()
        {
            GUILayout.Label("All Entries (Scenes + Prefabs + Polish):", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));

            GUILayout.Space(4);

            string currentGroup = "";
            foreach (var entry in ALL_ENTRIES)
            {
                if (entry.group != currentGroup)
                {
                    currentGroup = entry.group;
                    GUILayout.Space(4);
                    GUI.color = new Color(0.5f, 0.9f, 1f);
                    GUILayout.Label($"--- {currentGroup} ---", EditorStyles.miniLabel);
                    GUI.color = Color.white;
                }

                bool isScene = entry.scene != null;
                bool sceneExists = isScene && System.IO.File.Exists(entry.scene);

                EditorGUILayout.BeginHorizontal();

                // Type indicator
                GUI.color = isScene ? new Color(0.3f, 0.7f, 1f) : new Color(1f, 0.84f, 0f);
                GUILayout.Label(isScene ? "SCN" : "PRE", EditorStyles.miniLabel, GUILayout.Width(30));

                // Status
                if (isScene)
                {
                    GUI.color = sceneExists ? Color.green : Color.red;
                    GUILayout.Label(sceneExists ? "OK" : "MISS", EditorStyles.miniLabel, GUILayout.Width(35));
                }
                else
                {
                    GUI.color = Color.yellow;
                    GUILayout.Label("---", EditorStyles.miniLabel, GUILayout.Width(35));
                }

                // Name
                GUI.color = Color.white;
                GUILayout.Label(entry.displayName, GUILayout.Width(200));

                // Class.Method
                GUI.color = new Color(0.6f, 0.6f, 0.6f);
                GUILayout.Label($"{entry.className}", EditorStyles.miniLabel);

                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        // ==================== BUILD LOGIC ====================

        private static void BuildAll()
        {
            BuildEntries(ALL_ENTRIES);
        }

        private static void BuildFiltered(bool scenesOnly)
        {
            var filtered = ALL_ENTRIES.Where(e => scenesOnly ? e.scene != null : e.scene == null).ToArray();
            BuildEntries(filtered);
        }

        private static void BuildGroup(string group)
        {
            var filtered = ALL_ENTRIES.Where(e => e.group == group).ToArray();
            BuildEntries(filtered);
        }

        private static void BuildEntries(Entry[] entries)
        {
            isRunning = true;
            SilentMode = true;
            string originalScene = EditorSceneManager.GetActiveScene().path;
            int successCount = 0;
            int failCount = 0;
            int skipCount = 0;
            int total = entries.Length;

            try
            {
                for (int i = 0; i < total; i++)
                {
                    var entry = entries[i];

                    EditorUtility.DisplayProgressBar(
                        "Building All Scenes",
                        $"[{i + 1}/{total}] {entry.group} / {entry.displayName}",
                        (float)i / total);

                    // Scene entries: open the scene first
                    if (entry.scene != null)
                    {
                        if (!System.IO.File.Exists(entry.scene))
                        {
                            results.Add(new BuildResult
                            {
                                group = entry.group,
                                displayName = entry.displayName,
                                status = "Scene not found",
                                success = false
                            });
                            skipCount++;
                            continue;
                        }

                        try
                        {
                            EditorSceneManager.OpenScene(entry.scene);
                        }
                        catch (Exception ex)
                        {
                            results.Add(new BuildResult
                            {
                                group = entry.group,
                                displayName = entry.displayName,
                                status = $"Failed to open: {ex.Message}",
                                success = false
                            });
                            failCount++;
                            continue;
                        }
                    }

                    // Try BuildSilent() first, fallback to named method
                    try
                    {
                        bool built = TryCallMethod(entry.className, "BuildSilent");
                        if (!built)
                        {
                            built = TryCallMethod(entry.className, entry.method);
                        }

                        if (built)
                        {
                            // Save scene if it's a scene entry
                            if (entry.scene != null)
                            {
                                EditorSceneManager.MarkSceneDirty(
                                    UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                                EditorSceneManager.SaveOpenScenes();
                            }

                            results.Add(new BuildResult
                            {
                                group = entry.group,
                                displayName = entry.displayName,
                                status = entry.scene != null ? "Built + Saved" : "Built",
                                success = true
                            });
                            successCount++;
                        }
                        else
                        {
                            results.Add(new BuildResult
                            {
                                group = entry.group,
                                displayName = entry.displayName,
                                status = $"Method not found ({entry.className})",
                                success = false
                            });
                            failCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        string innerMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                        results.Add(new BuildResult
                        {
                            group = entry.group,
                            displayName = entry.displayName,
                            status = $"Error: {innerMsg}",
                            success = false
                        });
                        failCount++;
                        Debug.LogError($"[AllScenesBatchBuilder] Error building {entry.displayName}: {ex}");
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                SilentMode = false;
                isRunning = false;

                // Return to original scene
                if (!string.IsNullOrEmpty(originalScene) && System.IO.File.Exists(originalScene))
                {
                    try { EditorSceneManager.OpenScene(originalScene); }
                    catch { /* best effort */ }
                }

                Debug.Log($"[AllScenesBatchBuilder] Done: {successCount} OK, {failCount} FAIL, {skipCount} SKIP — {total} total");
            }
        }

        // ==================== REFLECTION HELPERS ====================

        /// <summary>
        /// Finds a class by name and calls a static method via reflection.
        /// Returns true if the method was found and invoked.
        /// </summary>
        private static bool TryCallMethod(string className, string methodName)
        {
            Type type = FindType(className);
            if (type == null) return false;

            MethodInfo method = type.GetMethod(methodName,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (method == null) return false;

            method.Invoke(null, null);
            return true;
        }

        private static Type FindType(string name)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var t in asm.GetTypes())
                    {
                        if (t.Name == name)
                            return t;
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    // Some assemblies may fail to load types; skip
                }
            }
            return null;
        }

        // ==================== RESULTS DISPLAY ====================

        private void DrawResults()
        {
            GUILayout.Label("Results:", EditorStyles.boldLabel);

            int success = results.Count(r => r.success);
            int fail = results.Count - success;

            float rate = results.Count > 0 ? (float)success / results.Count : 0;
            GUI.color = rate == 1f ? new Color(0.2f, 0.8f, 0.2f) :
                        rate >= 0.7f ? new Color(1f, 0.8f, 0.2f) : new Color(1f, 0.4f, 0.4f);
            GUILayout.Label($"Success: {success} | Failed: {fail} | Total: {results.Count}");
            GUI.color = Color.white;

            var resultScroll = EditorGUILayout.BeginScrollView(
                Vector2.zero, GUILayout.Height(220));

            string currentGroup = "";
            foreach (var r in results)
            {
                if (r.group != currentGroup)
                {
                    currentGroup = r.group;
                    GUI.color = new Color(0.5f, 0.8f, 1f);
                    GUILayout.Label($"  {currentGroup}", EditorStyles.miniLabel);
                    GUI.color = Color.white;
                }

                EditorGUILayout.BeginHorizontal();
                GUI.color = r.success ? Color.green : Color.red;
                GUILayout.Label(r.success ? "OK" : "FAIL", EditorStyles.boldLabel, GUILayout.Width(40));
                GUI.color = Color.white;
                GUILayout.Label(r.displayName, GUILayout.Width(220));
                GUI.color = r.success ? Color.white : new Color(1f, 0.6f, 0.6f);
                GUILayout.Label(r.status, EditorStyles.miniLabel);
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
