// ============================================================
// FontSizeBatchRebuilder.cs  –  LEGACY (use AllScenesBatchBuilder instead)
// Quick static version that opens every scene and runs its UIBuilder.
// For the EditorWindow version with progress UI, use:
//   DigitPark > Tools > Batch Build All Scenes
// ============================================================
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System;
using System.Reflection;

public static class FontSizeBatchRebuilder
{
    struct Entry
    {
        public string scene;   // null = prefab builder (no scene needed)
        public string type;    // class name
        public string method;  // static method to invoke
    }

    [MenuItem("DigitPark/Tools/Batch Rebuild ALL (Legacy Quick)", false, 9999)]
    public static void RebuildAll()
    {
        if (!EditorUtility.DisplayDialog(
            "Batch Rebuild ALL Scenes",
            "Opens ALL scenes and runs every UIBuilder.\n\n" +
            "• 39 scenes + prefab builders\n" +
            "• May take several minutes\n" +
            "• Save your work before continuing\n\n" +
            "Continue?", "Yes, Rebuild ALL", "Cancel"))
            return;

        string originalScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;

        var entries = new Entry[]
        {
            // ── Core ──
            E("Assets/_Project/Scenes/_Core/MainMenu.unity",                 "MainMenuUIBuilder",              "RebuildMainMenu"),
            E("Assets/_Project/Scenes/_Core/Settings.unity",                 "SettingsUIBuilder",              "BuildSettingsUI"),

            // ── Auth ──
            E("Assets/_Project/Scenes/Auth/Login.unity",                     "LoginUIBuilder",                 "RebuildLoginScene"),
            E("Assets/_Project/Scenes/Auth/Register.unity",                  "RegisterUIBuilder",              "RebuildRegisterScene"),
            E("Assets/_Project/Scenes/Auth/AgeVerification.unity",           "AgeVerificationUIBuilder",       "RebuildAgeVerificationScene"),

            // ── Games - Navigation ──
            E("Assets/_Project/Scenes/Games/Navigation/GameSelector.unity",        "GameSelectorUIBuilder",          "RebuildGameSelectorUI"),
            E("Assets/_Project/Scenes/Games/Navigation/PlayModeSelection.unity",   "PlayModeSelectionUIBuilder",     "BuildPlayModeSelectionUI"),
            E("Assets/_Project/Scenes/Games/Navigation/Matchmaking.unity",         "MatchmakingUIBuilder",           "BuildUI"),
            E("Assets/_Project/Scenes/Games/Navigation/BetSelection.unity",        "BetSelectionUIBuilder",          "BuildScene"),

            // ── Games - Minigames ──
            E("Assets/_Project/Scenes/Games/Minigames/DigitRush.unity",      "DigitRushUIBuilder",             "RebuildDigitRushUI"),
            E("Assets/_Project/Scenes/Games/Minigames/FlashTap.unity",       "FlashTapUIBuilder",              "RebuildFlashTapUI"),
            E("Assets/_Project/Scenes/Games/Minigames/MemoryPairs.unity",    "MemoryPairsUIBuilder",           "RebuildMemoryPairsUI"),
            E("Assets/_Project/Scenes/Games/Minigames/OddOneOut.unity",      "OddOneOutUIBuilder",             "RebuildOddOneOutUI"),
            E("Assets/_Project/Scenes/Games/Minigames/QuickMath.unity",      "QuickMathUIBuilder",             "RebuildQuickMathUI"),

            // ── Social - Profile ──
            E("Assets/_Project/Scenes/Social/Profile/Profile.unity",          "ProfileUIBuilder",               "RebuildProfile"),
            E("Assets/_Project/Scenes/Social/Profile/MatchHistory.unity",     "MatchHistoryUIBuilder",          "RebuildMatchHistory"),
            E("Assets/_Project/Scenes/Social/Profile/Scores.unity",           "ScoresUIBuilder",                "RebuildScoresUI"),

            // ── Social - Friends ──
            E("Assets/_Project/Scenes/Social/Friends/Friends.unity",          "FriendsUIBuilder",               "RebuildFriends"),
            E("Assets/_Project/Scenes/Social/Friends/FriendRequests.unity",   "FriendRequestsUIBuilder",        "RebuildFriendRequests"),
            E("Assets/_Project/Scenes/Social/Friends/SearchPlayers.unity",    "SearchPlayersUIBuilder",         "RebuildSearchPlayersUI"),

            // ── Social - Notifications ──
            E("Assets/_Project/Scenes/Social/Notifications/Notifications.unity", "NotificationsUIBuilder",      "RebuildNotifications"),

            // ── Monetization ──
            E("Assets/_Project/Scenes/Monetization/Achievements.unity",     "AchievementsUIBuilder",          "BuildTrophyShowcase"),
            E("Assets/_Project/Scenes/Monetization/DailyRewards.unity",     "DailyRewardsPremiumUIBuilder",   "RebuildDailyRewards"),
            E("Assets/_Project/Scenes/Monetization/DailyMissions.unity",    "DailyMissionsUIBuilder",         "RebuildMissions"),
            E("Assets/_Project/Scenes/Monetization/Shop.unity",             "ShopPremiumUIBuilder",           "BuildCompleteUI"),

            // ── Tournaments ──
            E("Assets/_Project/Scenes/Tournaments/TournamentCreate.unity",  "TournamentCreateUIBuilder",      "BuildCompleteUI"),
            E("Assets/_Project/Scenes/Tournaments/TournamentLobby.unity",   "TournamentLobbyUIBuilder",       "BuildCompleteUI"),
            E("Assets/_Project/Scenes/Tournaments/TournamentsBrowser.unity","TournamentsBrowserUIBuilder",    "BuildCompleteUI"),

            // ── CashBattle ──
            E("Assets/_Project/Scenes/CashBattle/CashBattleHub.unity",     "CashBattleUIBuilder",            "BuildPremiumUI"),
            E("Assets/_Project/Scenes/CashBattle/CashBattle1v1.unity",     "CashBattle1v1UIBuilder",         "BuildCashBattle1v1UI"),
            E("Assets/_Project/Scenes/CashBattle/CashTournaments/CashTournaments.unity",       "CashTournamentsUIBuilder",       "BuildCashTournamentsUI"),
            E("Assets/_Project/Scenes/CashBattle/CashHistory.unity",                         "CashHistoryUIBuilder",           "BuildCashHistoryUI"),
            E("Assets/_Project/Scenes/CashBattle/CashProfile.unity",                         "CashProfileUIBuilder",           "BuildAndAssign"),
            E("Assets/_Project/Scenes/CashBattle/CashWallet.unity",                          "WalletUIBuilder",                "BuildWalletUI"),
            E("Assets/_Project/Scenes/CashBattle/CashMatchmaking.unity",                     "CashMatchmakingUIBuilder",       "BuildUI"),
            E("Assets/_Project/Scenes/CashBattle/CashTournaments/CashTournamentCreate.unity", "CashTournamentCreateUIBuilder", "BuildGoldUI"),
            E("Assets/_Project/Scenes/CashBattle/CashTournaments/CashTournamentLobby.unity",  "CashTournamentLobbyUIBuilder",  "BuildGoldUI"),

            // ── Onboarding ──
            E("Assets/_Project/Scenes/Onboarding/Onboarding.unity",              "OnboardingUIBuilder",              "RebuildOnboarding"),
            E("Assets/_Project/Scenes/Onboarding/CashBattleOnboarding.unity",    "CashBattleOnboardingUIBuilder",    "RebuildCashBattleOnboarding"),

            // ── Prefab Builders (no scene) ──
            E(null, "WinPanelUIBuilder",                "CreateNormalWinPanel"),
            E(null, "WinPanelUIBuilder",                "CreateNormalLosePanel"),
            E(null, "WinPanelUIBuilder",                "CreateRealMoneyWinPanel"),
            E(null, "OnlineResultPanelUIBuilder",       "BuildWinPanel"),
            E(null, "OnlineResultPanelUIBuilder",       "BuildLosePanel"),
            E(null, "CashBattleResultPanelUIBuilder",   "BuildWinPanel"),
            E(null, "CashBattleResultPanelUIBuilder",   "BuildLosePanel"),
            E(null, "TournamentResultPanelUIBuilder",   "BuildWinPanel"),
            E(null, "TournamentResultPanelUIBuilder",   "BuildLosePanel"),
            E(null, "SprintSummaryPanelUIBuilder",      "BuildSprintSummaryPanel"),
            E(null, "CashTournamentResultsUIBuilder",   "BuildSilent"),
            E(null, "AchievementToastUIBuilder",        "CreateAchievementToastPrefab"),
            E(null, "InAppToastUIBuilder",              "CreateInAppToastPrefab"),
            E(null, "MonetizationPrefabBuilder",        "CreateAllPrefabs"),
            E(null, "MissionCardPrefabBuilder",         "CreateMissionCardPrefab"),
            E(null, "ItemPrefabBuilder",                "CreateTournamentSearchItemPrefab"),
            E(null, "ItemPrefabBuilder",                "CreateTournamentMyItemPrefab"),
            E(null, "ItemPrefabBuilder",                "CreateLeaderboardEntryPrefab"),
        };

        int ok = 0, fail = 0, total = entries.Length;

        for (int i = 0; i < total; i++)
        {
            var e = entries[i];
            string label = $"{e.type}.{e.method}";
            EditorUtility.DisplayProgressBar(
                "Font Size Batch Rebuild",
                $"[{i + 1}/{total}] {label}",
                (float)i / total);

            try
            {
                // Open scene if needed
                if (!string.IsNullOrEmpty(e.scene))
                {
                    if (!System.IO.File.Exists(e.scene))
                    {
                        Debug.LogWarning($"[BatchRebuild] SKIP (scene not found): {e.scene}");
                        fail++;
                        continue;
                    }
                    EditorSceneManager.OpenScene(e.scene);
                }

                // Find type
                Type type = FindType(e.type);
                if (type == null)
                {
                    Debug.LogError($"[BatchRebuild] FAIL (type not found): {e.type}");
                    fail++;
                    continue;
                }

                // Find method (public or private, static)
                MethodInfo method = type.GetMethod(e.method,
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (method == null)
                {
                    Debug.LogError($"[BatchRebuild] FAIL (method not found): {label}");
                    fail++;
                    continue;
                }

                // Invoke
                method.Invoke(null, null);

                // Save scene
                if (!string.IsNullOrEmpty(e.scene))
                {
                    EditorSceneManager.MarkSceneDirty(
                        UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                    EditorSceneManager.SaveOpenScenes();
                }

                Debug.Log($"[BatchRebuild] OK [{i + 1}/{total}]: {label}");
                ok++;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BatchRebuild] FAIL: {label}\n{ex}");
                fail++;
            }
        }

        EditorUtility.ClearProgressBar();

        // Restore original scene
        if (!string.IsNullOrEmpty(originalScene))
        {
            try { EditorSceneManager.OpenScene(originalScene); }
            catch { /* best effort */ }
        }

        string summary = $"Batch Rebuild ALL Completado!\n\n" +
                          $"OK: {ok}/{total}\n" +
                          $"FAIL: {fail}/{total}\n\n" +
                          "Revisa la Console para detalles.";

        EditorUtility.DisplayDialog("Batch Rebuild Done", summary, "OK");
        Debug.Log($"[BatchRebuild] ========== RESUMEN: {ok} OK, {fail} FAIL de {total} total ==========");
    }

    static Entry E(string scene, string type, string method)
    {
        return new Entry { scene = scene, type = type, method = method };
    }

    static Type FindType(string name)
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
                // Some assemblies may fail to load types; skip them
            }
        }
        return null;
    }
}
