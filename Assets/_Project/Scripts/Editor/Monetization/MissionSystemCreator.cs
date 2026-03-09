using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using DigitPark.Data;
using DigitPark.Games;
using DigitPark.UI.Items;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor window para crear todos los ScriptableObject assets del sistema de misiones.
    /// Crea 20 definiciones diarias, 10 semanales, y 2 pools.
    /// </summary>
    public class MissionSystemCreator : EditorWindow
    {
        private const string DAILY_PATH = "Assets/_Project/Data/Missions/Daily/";
        private const string WEEKLY_PATH = "Assets/_Project/Data/Missions/Weekly/";
        private const string POOLS_PATH = "Assets/_Project/Data/Missions/";

        [MenuItem("DigitPark/Setup/Missions/Create Mission System")]
        public static void ShowWindow()
        {
            var window = GetWindow<MissionSystemCreator>("Mission System Creator");
            window.minSize = new Vector2(350, 300);
        }

        private void OnGUI()
        {
            GUILayout.Label("Mission System Creator", EditorStyles.boldLabel);
            GUILayout.Label("Crea ScriptableObjects para el sistema de misiones", EditorStyles.miniLabel);
            GUILayout.Space(15);

            if (GUILayout.Button("Crear Definiciones (20 Daily + 10 Weekly)", GUILayout.Height(35)))
            {
                CreateAllDefinitions();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Crear Pools (Daily + Weekly)", GUILayout.Height(35)))
            {
                CreatePools();
            }

            GUILayout.Space(15);

            if (GUILayout.Button("CREAR TODO (Definiciones + Pools + Prefab)", GUILayout.Height(45)))
            {
                CreateAllDefinitions();
                CreatePools();
                MissionCardPrefabBuilder.CreateMissionCardPrefabFromScript();
                EditorUtility.DisplayDialog("Completado",
                    "Sistema de misiones creado:\n- 20 definiciones diarias\n- 10 definiciones semanales\n- 2 pools\n- 1 prefab MissionCard",
                    "OK");
            }
        }

        private static void CreateAllDefinitions()
        {
            EnsureDirectories();
            CreateDailyDefinitions();
            CreateWeeklyDefinitions();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[MissionSystemCreator] 30 definiciones creadas (20 daily + 10 weekly)");
        }

        private static void EnsureDirectories()
        {
            if (!System.IO.Directory.Exists(DAILY_PATH))
                System.IO.Directory.CreateDirectory(DAILY_PATH);
            if (!System.IO.Directory.Exists(WEEKLY_PATH))
                System.IO.Directory.CreateDirectory(WEEKLY_PATH);
            if (!System.IO.Directory.Exists(POOLS_PATH))
                System.IO.Directory.CreateDirectory(POOLS_PATH);
        }

        #region Daily Definitions

        private static void CreateDailyDefinitions()
        {
            CreateMission(DAILY_PATH, "daily_play_3", "ms_daily_play_3_title", "ms_daily_play_3_desc",
                MissionActionType.PlayGames, GameType.DigitRush, false, 3,
                MissionDifficulty.Easy, MissionCategory.Daily, MissionRewardType.DigitCoins, 50, 0);

            CreateMission(DAILY_PATH, "daily_play_5", "ms_daily_play_5_title", "ms_daily_play_5_desc",
                MissionActionType.PlayGames, GameType.DigitRush, false, 5,
                MissionDifficulty.Medium, MissionCategory.Daily, MissionRewardType.DigitCoins, 80, 1);

            CreateMission(DAILY_PATH, "daily_win_1", "ms_daily_win_1_title", "ms_daily_win_1_desc",
                MissionActionType.WinGames, GameType.DigitRush, false, 1,
                MissionDifficulty.Easy, MissionCategory.Daily, MissionRewardType.DigitCoins, 75, 2);

            CreateMission(DAILY_PATH, "daily_win_3", "ms_daily_win_3_title", "ms_daily_win_3_desc",
                MissionActionType.WinGames, GameType.DigitRush, false, 3,
                MissionDifficulty.Medium, MissionCategory.Daily, MissionRewardType.DigitCoins, 120, 3);

            CreateMission(DAILY_PATH, "daily_digitrush_2", "ms_daily_digitrush_2_title", "ms_daily_digitrush_2_desc",
                MissionActionType.PlaySpecificGame, GameType.DigitRush, true, 2,
                MissionDifficulty.Easy, MissionCategory.Daily, MissionRewardType.DigitCoins, 60, 4);

            CreateMission(DAILY_PATH, "daily_memory_2", "ms_daily_memory_2_title", "ms_daily_memory_2_desc",
                MissionActionType.PlaySpecificGame, GameType.MemoryPairs, true, 2,
                MissionDifficulty.Easy, MissionCategory.Daily, MissionRewardType.DigitCoins, 60, 5);

            CreateMission(DAILY_PATH, "daily_quickmath_2", "ms_daily_quickmath_2_title", "ms_daily_quickmath_2_desc",
                MissionActionType.PlaySpecificGame, GameType.QuickMath, true, 2,
                MissionDifficulty.Easy, MissionCategory.Daily, MissionRewardType.DigitCoins, 60, 6);

            CreateMission(DAILY_PATH, "daily_flashtap_2", "ms_daily_flashtap_2_title", "ms_daily_flashtap_2_desc",
                MissionActionType.PlaySpecificGame, GameType.FlashTap, true, 2,
                MissionDifficulty.Easy, MissionCategory.Daily, MissionRewardType.DigitCoins, 60, 7);

            CreateMission(DAILY_PATH, "daily_oddoneout_2", "ms_daily_oddoneout_2_title", "ms_daily_oddoneout_2_desc",
                MissionActionType.PlaySpecificGame, GameType.OddOneOut, true, 2,
                MissionDifficulty.Easy, MissionCategory.Daily, MissionRewardType.DigitCoins, 60, 8);

            CreateMission(DAILY_PATH, "daily_score_500", "ms_daily_score_500_title", "ms_daily_score_500_desc",
                MissionActionType.ReachScore, GameType.DigitRush, false, 500,
                MissionDifficulty.Easy, MissionCategory.Daily, MissionRewardType.DigitCoins, 40, 9);

            CreateMission(DAILY_PATH, "daily_score_2000", "ms_daily_score_2000_title", "ms_daily_score_2000_desc",
                MissionActionType.ReachScore, GameType.DigitRush, false, 2000,
                MissionDifficulty.Medium, MissionCategory.Daily, MissionRewardType.DigitCoins, 100, 10);

            CreateMission(DAILY_PATH, "daily_score_5000", "ms_daily_score_5000_title", "ms_daily_score_5000_desc",
                MissionActionType.ReachScore, GameType.DigitRush, false, 5000,
                MissionDifficulty.Hard, MissionCategory.Daily, MissionRewardType.DigitGems, 25, 11);

            CreateMission(DAILY_PATH, "daily_total_1000", "ms_daily_total_1000_title", "ms_daily_total_1000_desc",
                MissionActionType.AccumulateScore, GameType.DigitRush, false, 1000,
                MissionDifficulty.Easy, MissionCategory.Daily, MissionRewardType.DigitCoins, 50, 12);

            CreateMission(DAILY_PATH, "daily_total_3000", "ms_daily_total_3000_title", "ms_daily_total_3000_desc",
                MissionActionType.AccumulateScore, GameType.DigitRush, false, 3000,
                MissionDifficulty.Medium, MissionCategory.Daily, MissionRewardType.DigitCoins, 100, 13);

            CreateMission(DAILY_PATH, "daily_precision_80", "ms_daily_precision_80_title", "ms_daily_precision_80_desc",
                MissionActionType.PrecisionGame, GameType.DigitRush, false, 1,
                MissionDifficulty.Medium, MissionCategory.Daily, MissionRewardType.DigitCoins, 75, 14);

            CreateMission(DAILY_PATH, "daily_win_digitrush", "ms_daily_win_digitrush_title", "ms_daily_win_digitrush_desc",
                MissionActionType.WinSpecificGame, GameType.DigitRush, true, 1,
                MissionDifficulty.Medium, MissionCategory.Daily, MissionRewardType.DigitCoins, 80, 15);

            CreateMission(DAILY_PATH, "daily_win_memory", "ms_daily_win_memory_title", "ms_daily_win_memory_desc",
                MissionActionType.WinSpecificGame, GameType.MemoryPairs, true, 1,
                MissionDifficulty.Medium, MissionCategory.Daily, MissionRewardType.DigitCoins, 80, 16);

            CreateMission(DAILY_PATH, "daily_perfect", "ms_daily_perfect_title", "ms_daily_perfect_desc",
                MissionActionType.PrecisionGame, GameType.DigitRush, false, 1,
                MissionDifficulty.Hard, MissionCategory.Daily, MissionRewardType.DigitGems, 30, 17);

            CreateMission(DAILY_PATH, "daily_sprint_1", "ms_daily_sprint_1_title", "ms_daily_sprint_1_desc",
                MissionActionType.CompleteCognitiveSprint, GameType.DigitRush, false, 1,
                MissionDifficulty.Medium, MissionCategory.Daily, MissionRewardType.DigitCoins, 100, 18);

            CreateMission(DAILY_PATH, "daily_all_games", "ms_daily_all_games_title", "ms_daily_all_games_desc",
                MissionActionType.PlayAllGameTypes, GameType.DigitRush, false, 5,
                MissionDifficulty.Hard, MissionCategory.Daily, MissionRewardType.DigitGems, 40, 19);
        }

        #endregion

        #region Weekly Definitions

        private static void CreateWeeklyDefinitions()
        {
            CreateMission(WEEKLY_PATH, "weekly_play_20", "ms_weekly_play_20_title", "ms_weekly_play_20_desc",
                MissionActionType.PlayGames, GameType.DigitRush, false, 20,
                MissionDifficulty.Medium, MissionCategory.Weekly, MissionRewardType.DigitCoins, 300, 0);

            CreateMission(WEEKLY_PATH, "weekly_play_30", "ms_weekly_play_30_title", "ms_weekly_play_30_desc",
                MissionActionType.PlayGames, GameType.DigitRush, false, 30,
                MissionDifficulty.Hard, MissionCategory.Weekly, MissionRewardType.DigitCoins, 500, 1);

            CreateMission(WEEKLY_PATH, "weekly_win_10", "ms_weekly_win_10_title", "ms_weekly_win_10_desc",
                MissionActionType.WinGames, GameType.DigitRush, false, 10,
                MissionDifficulty.Medium, MissionCategory.Weekly, MissionRewardType.DigitCoins, 250, 2);

            CreateMission(WEEKLY_PATH, "weekly_win_20", "ms_weekly_win_20_title", "ms_weekly_win_20_desc",
                MissionActionType.WinGames, GameType.DigitRush, false, 20,
                MissionDifficulty.Hard, MissionCategory.Weekly, MissionRewardType.DigitGems, 100, 3);

            CreateMission(WEEKLY_PATH, "weekly_all_games", "ms_weekly_all_games_title", "ms_weekly_all_games_desc",
                MissionActionType.PlayAllGameTypes, GameType.DigitRush, false, 5,
                MissionDifficulty.Medium, MissionCategory.Weekly, MissionRewardType.DigitGems, 75, 4);

            CreateMission(WEEKLY_PATH, "weekly_streak_5", "ms_weekly_streak_5_title", "ms_weekly_streak_5_desc",
                MissionActionType.WinStreak, GameType.DigitRush, false, 5,
                MissionDifficulty.Hard, MissionCategory.Weekly, MissionRewardType.DigitGems, 80, 5);

            CreateMission(WEEKLY_PATH, "weekly_score_25k", "ms_weekly_score_25k_title", "ms_weekly_score_25k_desc",
                MissionActionType.AccumulateScore, GameType.DigitRush, false, 25000,
                MissionDifficulty.Medium, MissionCategory.Weekly, MissionRewardType.DigitCoins, 400, 6);

            CreateMission(WEEKLY_PATH, "weekly_score_50k", "ms_weekly_score_50k_title", "ms_weekly_score_50k_desc",
                MissionActionType.AccumulateScore, GameType.DigitRush, false, 50000,
                MissionDifficulty.Hard, MissionCategory.Weekly, MissionRewardType.DigitGems, 120, 7);

            CreateMission(WEEKLY_PATH, "weekly_precision_5", "ms_weekly_precision_5_title", "ms_weekly_precision_5_desc",
                MissionActionType.PrecisionGame, GameType.DigitRush, false, 5,
                MissionDifficulty.Hard, MissionCategory.Weekly, MissionRewardType.DigitGems, 60, 8);

            CreateMission(WEEKLY_PATH, "weekly_tournament", "ms_weekly_tournament_title", "ms_weekly_tournament_desc",
                MissionActionType.PlayTournament, GameType.DigitRush, false, 1,
                MissionDifficulty.Medium, MissionCategory.Weekly, MissionRewardType.DigitGems, 50, 9);
        }

        #endregion

        #region Pools

        private static void CreatePools()
        {
            EnsureDirectories();

            // Cargar todas las definiciones diarias
            var dailyDefs = new List<MissionDefinitionSO>();
            string[] dailyGuids = AssetDatabase.FindAssets("t:MissionDefinitionSO", new[] { "Assets/_Project/Data/Missions/Daily" });
            foreach (string guid in dailyGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var def = AssetDatabase.LoadAssetAtPath<MissionDefinitionSO>(path);
                if (def != null) dailyDefs.Add(def);
            }

            // Cargar todas las definiciones semanales
            var weeklyDefs = new List<MissionDefinitionSO>();
            string[] weeklyGuids = AssetDatabase.FindAssets("t:MissionDefinitionSO", new[] { "Assets/_Project/Data/Missions/Weekly" });
            foreach (string guid in weeklyGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var def = AssetDatabase.LoadAssetAtPath<MissionDefinitionSO>(path);
                if (def != null) weeklyDefs.Add(def);
            }

            // Daily Pool
            CreatePool(POOLS_PATH + "DailyMissionPool.asset", "DailyPool", MissionCategory.Daily, 6, dailyDefs);

            // Weekly Pool
            CreatePool(POOLS_PATH + "WeeklyMissionPool.asset", "WeeklyPool", MissionCategory.Weekly, 3, weeklyDefs);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[MissionSystemCreator] Pools creados: Daily ({dailyDefs.Count} misiones), Weekly ({weeklyDefs.Count} misiones)");
        }

        private static void CreatePool(string path, string poolName, MissionCategory category, int selectionCount, List<MissionDefinitionSO> missions)
        {
            var pool = AssetDatabase.LoadAssetAtPath<MissionPoolSO>(path);
            if (pool == null)
            {
                pool = ScriptableObject.CreateInstance<MissionPoolSO>();
                AssetDatabase.CreateAsset(pool, path);
            }

            pool.poolName = poolName;
            pool.category = category;
            pool.selectionCount = selectionCount;
            pool.missions = new List<MissionDefinitionSO>(missions);
            EditorUtility.SetDirty(pool);
        }

        #endregion

        #region Helpers

        private static void CreateMission(string folder, string id, string titleKey, string descKey,
            MissionActionType actionType, GameType targetGame, bool isGameSpecific, int targetAmount,
            MissionDifficulty difficulty, MissionCategory category, MissionRewardType rewardType, int rewardAmount, int sortOrder)
        {
            string path = folder + id + ".asset";

            var mission = AssetDatabase.LoadAssetAtPath<MissionDefinitionSO>(path);
            if (mission == null)
            {
                mission = ScriptableObject.CreateInstance<MissionDefinitionSO>();
                AssetDatabase.CreateAsset(mission, path);
            }

            mission.missionId = id;
            mission.titleLocKey = titleKey;
            mission.descriptionLocKey = descKey;
            mission.actionType = actionType;
            mission.targetGame = targetGame;
            mission.isGameSpecific = isGameSpecific;
            mission.targetAmount = targetAmount;
            mission.difficulty = difficulty;
            mission.category = category;
            mission.rewardType = rewardType;
            mission.rewardAmount = rewardAmount;
            mission.sortOrder = sortOrder;

            EditorUtility.SetDirty(mission);
        }

        #endregion
    }
}
