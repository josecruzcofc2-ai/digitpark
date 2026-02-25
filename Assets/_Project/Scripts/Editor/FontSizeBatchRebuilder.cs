// ============================================================
// FontSizeBatchRebuilder.cs  –  ONE-TIME USE
// Abre cada escena y ejecuta su UIBuilder para propagar
// los cambios de FontSizes.cs a toda la UI.
// Reutilizable: ejecuta todos los UIBuilders de golpe.
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

    [MenuItem("DigitPark/Tools/Font Size Batch Rebuild ALL", false, 9999)]
    public static void RebuildAll()
    {
        if (!EditorUtility.DisplayDialog(
            "Font Size Batch Rebuild",
            "Esto abrirá TODAS las escenas una por una y ejecutará cada UIBuilder.\n\n" +
            "• 36 escenas + prefab builders\n" +
            "• Puede tardar varios minutos\n" +
            "• Guarda tu trabajo antes de continuar\n\n" +
            "¿Continuar?", "Sí, Reconstruir TODO", "Cancelar"))
            return;

        string originalScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;

        var entries = new Entry[]
        {
            // ── Core ──
            E("Assets/_Project/Scenes/Core/MainMenu.unity",                  "MainMenuUIBuilder",              "RebuildMainMenu"),
            E("Assets/_Project/Scenes/Core/Settings.unity",                  "SettingsUIBuilder",              "BuildSettingsUI"),

            // ── Auth ──
            E("Assets/_Project/Scenes/Auth/Login.unity",                     "LoginUIBuilder",                 "RebuildLoginScene"),
            E("Assets/_Project/Scenes/Auth/Register.unity",                  "RegisterUIBuilder",              "RebuildRegisterScene"),
            E("Assets/_Project/Scenes/Auth/AgeVerification.unity",           "AgeVerificationUIBuilder",       "RebuildAgeVerificationScene"),

            // ── Games ──
            E("Assets/_Project/Scenes/Games/DigitRush.unity",               "DigitRushUIBuilder",             "RebuildDigitRushUI"),
            E("Assets/_Project/Scenes/Games/FlashTap.unity",                "FlashTapUIBuilder",              "RebuildFlashTapUI"),
            E("Assets/_Project/Scenes/Games/MemoryPairs.unity",             "MemoryPairsUIBuilder",           "RebuildMemoryPairsUI"),
            E("Assets/_Project/Scenes/Games/OddOneOut.unity",               "OddOneOutUIBuilder",             "RebuildOddOneOutUI"),
            E("Assets/_Project/Scenes/Games/QuickMath.unity",               "QuickMathUIBuilder",             "RebuildQuickMathUI"),
            E("Assets/_Project/Scenes/Games/GameSelector.unity",            "GameSelectorUIBuilder",          "RebuildGameSelectorUI"),
            E("Assets/_Project/Scenes/Games/PlayModeSelection.unity",       "PlayModeSelectionUIBuilder",     "BuildPlayModeSelectionUI"),
            E("Assets/_Project/Scenes/Games/Matchmaking.unity",             "MatchmakingUIBuilder",           "BuildUI"),
            E("Assets/_Project/Scenes/Games/BetSelection.unity",            "BetSelectionPanelUIBuilder",     "BuildScene"),

            // ── Social ──
            E("Assets/_Project/Scenes/Social/Profile.unity",                "ProfileUIBuilder",               "RebuildProfile"),
            E("Assets/_Project/Scenes/Social/Friends.unity",                "FriendsUIBuilder",               "RebuildFriends"),
            E("Assets/_Project/Scenes/Social/FriendRequests.unity",         "FriendRequestsUIBuilder",        "RebuildFriendRequests"),
            E("Assets/_Project/Scenes/Social/MatchHistory.unity",           "MatchHistoryUIBuilder",          "RebuildMatchHistory"),
            E("Assets/_Project/Scenes/Social/Notifications.unity",          "NotificationsUIBuilder",         "RebuildNotifications"),
            E("Assets/_Project/Scenes/Social/Scores.unity",                 "ScoresUIBuilder",                "RebuildScoresUI"),
            E("Assets/_Project/Scenes/Social/SearchPlayers.unity",          "SearchPlayersUIBuilder",         "RebuildSearchPlayersUI"),

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
            E("Assets/_Project/Scenes/CashBattle/CashTournaments.unity",   "CashTournamentsUIBuilder",       "BuildCashTournamentsUI"),
            E("Assets/_Project/Scenes/CashBattle/CashHistory.unity",       "CashHistoryUIBuilder",           "BuildCashHistoryUI"),
            E("Assets/_Project/Scenes/CashBattle/CashProfile.unity",       "CashProfileUIBuilder",           "BuildAndAssign"),
            E("Assets/_Project/Scenes/CashBattle/CashWallet.unity",        "WalletUIBuilder",                "BuildWalletUI"),

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

        string summary = $"Font Size Batch Rebuild Completado!\n\n" +
                          $"OK: {ok}/{total}\n" +
                          $"FAIL: {fail}/{total}\n\n" +
                          "Revisa la Console para detalles.";

        EditorUtility.DisplayDialog("Batch Rebuild Completado", summary, "OK");
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
