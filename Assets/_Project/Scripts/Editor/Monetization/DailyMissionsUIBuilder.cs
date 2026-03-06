using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.Managers;
using DigitPark.Data;
using DigitPark.UI;
using DigitPark.Monetization;

namespace DigitPark.Editor
{
    /// <summary>
    /// Daily Missions UI Builder - Neon Cyan theme
    /// Layout: ProgressBar → TopBar → TimerBar → OverallProgress → ScrollView (Daily + Weekly)
    /// Portrait 9:16 (1080x1920), matchWidthOrHeight=0
    /// NO SafeArea, NO tab bar, NO dialog confirmation
    ///
    /// Menu: DigitPark/UI Builders/Monetization/Daily Missions
    /// </summary>
    public class DailyMissionsUIBuilder : EditorWindow
    {
        #region Colors

        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.5f, 1f);
        private static readonly Color CYAN_GLOW = new Color(0f, 1f, 1f, 0.06f);

        private static readonly Color DARK_BG = new Color(0.02f, 0.04f, 0.08f, 1f);
        private static readonly Color CARD_BG = new Color(0.06f, 0.08f, 0.12f, 1f);

        private static readonly Color TEXT_WHITE = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.6f, 0.65f, 1f);
        private static readonly Color TEXT_DARK = new Color(0.05f, 0.05f, 0.08f, 1f);

        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color ORANGE_TIMER = new Color(1f, 0.5f, 0.12f, 1f);
        private static readonly Color PURPLE_WEEKLY = new Color(0.6f, 0.2f, 1f, 1f);
        private static readonly Color GREEN_SUCCESS = new Color(0.2f, 0.9f, 0.4f, 1f);

        private static readonly Color COIN_COLOR = new Color(1f, 0.85f, 0.3f, 1f);
        private static readonly Color GEM_COLOR = new Color(0.4f, 0.8f, 1f, 1f);
        private static readonly Color XP_COLOR = new Color(0.5f, 1f, 0.5f, 1f);

        #endregion

        #region Layout Anchors (Y: 0=bottom, 1=top)

        private const float PROGRESS_TOP = 0.993f;
        private const float PROGRESS_BOT = 0.990f;

        private const float TOPBAR_TOP = 0.988f;
        private const float TOPBAR_BOT = 0.955f;

        private const float TIMER_TOP = 0.952f;
        private const float TIMER_BOT = 0.925f;

        private const float TABS_TOP = 0.922f;
        private const float TABS_BOT = 0.892f;

        private const float OVERALL_TOP = 0.888f;
        private const float OVERALL_BOT = 0.815f;

        private const float SCROLL_TOP = 0.810f;
        private const float SCROLL_BOT = 0.015f;

        private const float SIDE_PAD = 30f;

        #endregion

        #region Paths

        private const string MISSIONS_ICONS_PATH = "Assets/_Project/Art/Icons/Missions/";
        private const string UI_ICONS_PATH = "Assets/_Project/Art/Icons/UI/";
        private const string CURRENCY_ICONS_PATH = "Assets/_Project/Art/Icons/Currency/";
        private const string BACK_BUTTON_PREFAB = "Assets/_Project/Prefabs/Common/BackButton.prefab";

        #endregion

        [MenuItem("DigitPark/UI Builders/Monetization/Daily Missions", false, 142)]
        public static void ShowWindow()
        {
            GetWindow<DailyMissionsUIBuilder>("Daily Missions Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Daily Missions UI Builder", EditorStyles.boldLabel);
            GUILayout.Label("Misiones Diarias + Semanales - Neon Cyan", EditorStyles.miniLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Layout (de arriba a abajo):\n\n" +
                "1. Progress Bar (linea delgada cyan)\n" +
                "2. Top Bar (Back + MISIONES + Info)\n" +
                "3. Timer Bar (countdown reinicio)\n" +
                "4. Overall Progress (barra con milestones)\n" +
                "5. ScrollView (misiones diarias + semanales)\n" +
                "6. Reward Claim Popup (oculto)\n\n" +
                "6 misiones diarias + 3 semanales\n" +
                "Sistema SO: 20 daily + 10 weekly definiciones, 2 pools, 1 prefab",
                MessageType.Info);

            GUILayout.Space(15);

            GUI.backgroundColor = CYAN_NEON;
            if (GUILayout.Button("RECONSTRUIR MISIONES COMPLETO", GUILayout.Height(50)))
                RebuildMissions();
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);

            GUI.backgroundColor = GOLD;
            if (GUILayout.Button("ASIGNAR REFERENCIAS AL MANAGER", GUILayout.Height(35)))
                SetupManagerReferences();
            GUI.backgroundColor = Color.white;

            GUILayout.Space(15);
            GUILayout.Label("Sistema de Misiones (ScriptableObjects):", EditorStyles.boldLabel);

            GUI.backgroundColor = PURPLE_WEEKLY;
            if (GUILayout.Button("Crear Definiciones SO (20 Daily + 10 Weekly)", GUILayout.Height(30)))
                CreateMissionDefinitions();
            if (GUILayout.Button("Crear Pools SO (Daily + Weekly)", GUILayout.Height(30)))
                CreateMissionPools();
            if (GUILayout.Button("Crear MissionCard Prefab", GUILayout.Height(30)))
                MissionCardPrefabBuilder.CreateMissionCardPrefabFromScript();
            GUI.backgroundColor = Color.white;

            GUILayout.Space(15);
            GUILayout.Label("Secciones individuales:", EditorStyles.boldLabel);

            if (GUILayout.Button("1. Background + Progress Bar", GUILayout.Height(25)))
            {
                Canvas c = UIBuilderCanvasHelper.FindMainCanvas();
                if (c != null) { CreateBackground(c.transform); CreateProgressBar(); }
            }
            if (GUILayout.Button("2. Top Bar", GUILayout.Height(25))) CreateTopBar();
            if (GUILayout.Button("3. Timer Bar", GUILayout.Height(25))) CreateTimerBar();
            if (GUILayout.Button("3b. Tab Bar (Daily/Weekly/Special)", GUILayout.Height(25))) CreateTabBar();
            if (GUILayout.Button("4. Overall Progress", GUILayout.Height(25))) CreateOverallProgress();
            if (GUILayout.Button("5. Missions ScrollView", GUILayout.Height(25))) CreateMissionsScrollView();
            if (GUILayout.Button("6. Reward Claim Popup", GUILayout.Height(25))) CreateRewardClaimPopup();
        }

        #region Main Rebuild

        private static void RebuildMissions()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("[DailyMissionsUI] No se encontro Canvas");
                return;
            }

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0f;
            }

            // Full clean of canvas children (keep TransitionCanvas and EventSystem)
            CleanupOldElements(canvas.transform);

            // Crear SO system (definiciones + pools + prefab)
            CreateMissionDefinitions();
            CreateMissionPools();
            EnsureMissionCardPrefab();

            // Crear UI
            CreateBackground(canvas.transform);
            CreateProgressBar();
            CreateTopBar();
            CreateTimerBar();
            CreateTabBar();
            CreateOverallProgress();
            CreateMissionsScrollView();
            CreateRewardClaimPopup();
            SetupManagerReferences();

            Debug.Log("[DailyMissionsUI] Misiones RECONSTRUIDAS exitosamente (UI + SO system)!");
            EditorUtility.SetDirty(canvas.gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        }

        #region Mission System SO Creation

        private static void CreateMissionDefinitions()
        {
            // Delegar al MissionSystemCreator que ya tiene todas las definiciones
            var method = typeof(MissionSystemCreator).GetMethod("ShowWindow", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            // Crear definiciones directamente sin abrir ventana
            EnsureMissionDirectories();
            CreateDailyMissionSOs();
            CreateWeeklyMissionSOs();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[DailyMissionsUI] 30 definiciones SO creadas (20 daily + 10 weekly)");
        }

        private static void CreateMissionPools()
        {
            EnsureMissionDirectories();

            // Cargar definiciones daily
            var dailyDefs = new System.Collections.Generic.List<MissionDefinitionSO>();
            string[] dailyGuids = AssetDatabase.FindAssets("t:MissionDefinitionSO", new[] { "Assets/_Project/Data/Missions/Daily" });
            foreach (string guid in dailyGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var def = AssetDatabase.LoadAssetAtPath<MissionDefinitionSO>(path);
                if (def != null) dailyDefs.Add(def);
            }

            // Cargar definiciones weekly
            var weeklyDefs = new System.Collections.Generic.List<MissionDefinitionSO>();
            string[] weeklyGuids = AssetDatabase.FindAssets("t:MissionDefinitionSO", new[] { "Assets/_Project/Data/Missions/Weekly" });
            foreach (string guid in weeklyGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var def = AssetDatabase.LoadAssetAtPath<MissionDefinitionSO>(path);
                if (def != null) weeklyDefs.Add(def);
            }

            // Daily Pool
            CreatePoolAsset("Assets/_Project/Data/Missions/DailyMissionPool.asset", "DailyPool",
                DigitPark.UI.Items.MissionCategory.Daily, 6, dailyDefs);

            // Weekly Pool
            CreatePoolAsset("Assets/_Project/Data/Missions/WeeklyMissionPool.asset", "WeeklyPool",
                DigitPark.UI.Items.MissionCategory.Weekly, 3, weeklyDefs);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[DailyMissionsUI] Pools creados: Daily ({dailyDefs.Count} misiones), Weekly ({weeklyDefs.Count} misiones)");
        }

        private static void CreatePoolAsset(string path, string poolName, DigitPark.UI.Items.MissionCategory category,
            int selectionCount, System.Collections.Generic.List<MissionDefinitionSO> missions)
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
            pool.missions = new System.Collections.Generic.List<MissionDefinitionSO>(missions);
            EditorUtility.SetDirty(pool);
        }

        private static void EnsureMissionCardPrefab()
        {
            string prefabPath = "Assets/_Project/Prefabs/Monetization/DailyMissions/MissionCard.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
            {
                MissionCardPrefabBuilder.CreateMissionCardPrefabFromScript();
            }
        }

        private static void EnsureMissionDirectories()
        {
            string[] dirs = {
                "Assets/_Project/Data/Missions/Daily",
                "Assets/_Project/Data/Missions/Weekly"
            };
            foreach (var dir in dirs)
            {
                if (!System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                    AssetDatabase.Refresh();
                }
            }
        }

        private static void CreateMissionSO(string folder, string id, string titleKey, string descKey,
            MissionActionType actionType, DigitPark.Games.GameType targetGame, bool isGameSpecific, int targetAmount,
            DigitPark.UI.Items.MissionDifficulty difficulty, DigitPark.UI.Items.MissionCategory category,
            MissionRewardType rewardType, int rewardAmount, int sortOrder)
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

        private static void CreateDailyMissionSOs()
        {
            const string P = "Assets/_Project/Data/Missions/Daily/";
            var GT = typeof(DigitPark.Games.GameType);
            var D = DigitPark.UI.Items.MissionDifficulty.Easy;
            var DM = DigitPark.UI.Items.MissionDifficulty.Medium;
            var DH = DigitPark.UI.Items.MissionDifficulty.Hard;
            var CD = DigitPark.UI.Items.MissionCategory.Daily;
            var C = MissionRewardType.DigitCoins;
            var G = MissionRewardType.DigitGems;

            CreateMissionSO(P, "daily_play_3", "ms_daily_play_3_title", "ms_daily_play_3_desc",
                MissionActionType.PlayGames, DigitPark.Games.GameType.DigitRush, false, 3, D, CD, C, 50, 0);
            CreateMissionSO(P, "daily_play_5", "ms_daily_play_5_title", "ms_daily_play_5_desc",
                MissionActionType.PlayGames, DigitPark.Games.GameType.DigitRush, false, 5, DM, CD, C, 80, 1);
            CreateMissionSO(P, "daily_win_1", "ms_daily_win_1_title", "ms_daily_win_1_desc",
                MissionActionType.WinGames, DigitPark.Games.GameType.DigitRush, false, 1, D, CD, C, 75, 2);
            CreateMissionSO(P, "daily_win_3", "ms_daily_win_3_title", "ms_daily_win_3_desc",
                MissionActionType.WinGames, DigitPark.Games.GameType.DigitRush, false, 3, DM, CD, C, 120, 3);
            CreateMissionSO(P, "daily_digitrush_2", "ms_daily_digitrush_2_title", "ms_daily_digitrush_2_desc",
                MissionActionType.PlaySpecificGame, DigitPark.Games.GameType.DigitRush, true, 2, D, CD, C, 60, 4);
            CreateMissionSO(P, "daily_memory_2", "ms_daily_memory_2_title", "ms_daily_memory_2_desc",
                MissionActionType.PlaySpecificGame, DigitPark.Games.GameType.MemoryPairs, true, 2, D, CD, C, 60, 5);
            CreateMissionSO(P, "daily_quickmath_2", "ms_daily_quickmath_2_title", "ms_daily_quickmath_2_desc",
                MissionActionType.PlaySpecificGame, DigitPark.Games.GameType.QuickMath, true, 2, D, CD, C, 60, 6);
            CreateMissionSO(P, "daily_flashtap_2", "ms_daily_flashtap_2_title", "ms_daily_flashtap_2_desc",
                MissionActionType.PlaySpecificGame, DigitPark.Games.GameType.FlashTap, true, 2, D, CD, C, 60, 7);
            CreateMissionSO(P, "daily_oddoneout_2", "ms_daily_oddoneout_2_title", "ms_daily_oddoneout_2_desc",
                MissionActionType.PlaySpecificGame, DigitPark.Games.GameType.OddOneOut, true, 2, D, CD, C, 60, 8);
            CreateMissionSO(P, "daily_score_500", "ms_daily_score_500_title", "ms_daily_score_500_desc",
                MissionActionType.ReachScore, DigitPark.Games.GameType.DigitRush, false, 500, D, CD, C, 40, 9);
            CreateMissionSO(P, "daily_score_2000", "ms_daily_score_2000_title", "ms_daily_score_2000_desc",
                MissionActionType.ReachScore, DigitPark.Games.GameType.DigitRush, false, 2000, DM, CD, C, 100, 10);
            CreateMissionSO(P, "daily_score_5000", "ms_daily_score_5000_title", "ms_daily_score_5000_desc",
                MissionActionType.ReachScore, DigitPark.Games.GameType.DigitRush, false, 5000, DH, CD, G, 25, 11);
            CreateMissionSO(P, "daily_total_1000", "ms_daily_total_1000_title", "ms_daily_total_1000_desc",
                MissionActionType.AccumulateScore, DigitPark.Games.GameType.DigitRush, false, 1000, D, CD, C, 50, 12);
            CreateMissionSO(P, "daily_total_3000", "ms_daily_total_3000_title", "ms_daily_total_3000_desc",
                MissionActionType.AccumulateScore, DigitPark.Games.GameType.DigitRush, false, 3000, DM, CD, C, 100, 13);
            CreateMissionSO(P, "daily_precision_80", "ms_daily_precision_80_title", "ms_daily_precision_80_desc",
                MissionActionType.PrecisionGame, DigitPark.Games.GameType.DigitRush, false, 1, DM, CD, C, 75, 14);
            CreateMissionSO(P, "daily_win_digitrush", "ms_daily_win_digitrush_title", "ms_daily_win_digitrush_desc",
                MissionActionType.WinSpecificGame, DigitPark.Games.GameType.DigitRush, true, 1, DM, CD, C, 80, 15);
            CreateMissionSO(P, "daily_win_memory", "ms_daily_win_memory_title", "ms_daily_win_memory_desc",
                MissionActionType.WinSpecificGame, DigitPark.Games.GameType.MemoryPairs, true, 1, DM, CD, C, 80, 16);
            CreateMissionSO(P, "daily_perfect", "ms_daily_perfect_title", "ms_daily_perfect_desc",
                MissionActionType.PrecisionGame, DigitPark.Games.GameType.DigitRush, false, 1, DH, CD, G, 30, 17);
            CreateMissionSO(P, "daily_sprint_1", "ms_daily_sprint_1_title", "ms_daily_sprint_1_desc",
                MissionActionType.CompleteCognitiveSprint, DigitPark.Games.GameType.DigitRush, false, 1, DM, CD, C, 100, 18);
            CreateMissionSO(P, "daily_all_games", "ms_daily_all_games_title", "ms_daily_all_games_desc",
                MissionActionType.PlayAllGameTypes, DigitPark.Games.GameType.DigitRush, false, 5, DH, CD, G, 40, 19);
        }

        private static void CreateWeeklyMissionSOs()
        {
            const string P = "Assets/_Project/Data/Missions/Weekly/";
            var DM = DigitPark.UI.Items.MissionDifficulty.Medium;
            var DH = DigitPark.UI.Items.MissionDifficulty.Hard;
            var CW = DigitPark.UI.Items.MissionCategory.Weekly;
            var C = MissionRewardType.DigitCoins;
            var G = MissionRewardType.DigitGems;

            CreateMissionSO(P, "weekly_play_20", "ms_weekly_play_20_title", "ms_weekly_play_20_desc",
                MissionActionType.PlayGames, DigitPark.Games.GameType.DigitRush, false, 20, DM, CW, C, 300, 0);
            CreateMissionSO(P, "weekly_play_30", "ms_weekly_play_30_title", "ms_weekly_play_30_desc",
                MissionActionType.PlayGames, DigitPark.Games.GameType.DigitRush, false, 30, DH, CW, C, 500, 1);
            CreateMissionSO(P, "weekly_win_10", "ms_weekly_win_10_title", "ms_weekly_win_10_desc",
                MissionActionType.WinGames, DigitPark.Games.GameType.DigitRush, false, 10, DM, CW, C, 250, 2);
            CreateMissionSO(P, "weekly_win_20", "ms_weekly_win_20_title", "ms_weekly_win_20_desc",
                MissionActionType.WinGames, DigitPark.Games.GameType.DigitRush, false, 20, DH, CW, G, 100, 3);
            CreateMissionSO(P, "weekly_all_games", "ms_weekly_all_games_title", "ms_weekly_all_games_desc",
                MissionActionType.PlayAllGameTypes, DigitPark.Games.GameType.DigitRush, false, 5, DM, CW, G, 75, 4);
            CreateMissionSO(P, "weekly_streak_5", "ms_weekly_streak_5_title", "ms_weekly_streak_5_desc",
                MissionActionType.WinStreak, DigitPark.Games.GameType.DigitRush, false, 5, DH, CW, G, 80, 5);
            CreateMissionSO(P, "weekly_score_25k", "ms_weekly_score_25k_title", "ms_weekly_score_25k_desc",
                MissionActionType.AccumulateScore, DigitPark.Games.GameType.DigitRush, false, 25000, DM, CW, C, 400, 6);
            CreateMissionSO(P, "weekly_score_50k", "ms_weekly_score_50k_title", "ms_weekly_score_50k_desc",
                MissionActionType.AccumulateScore, DigitPark.Games.GameType.DigitRush, false, 50000, DH, CW, G, 120, 7);
            CreateMissionSO(P, "weekly_precision_5", "ms_weekly_precision_5_title", "ms_weekly_precision_5_desc",
                MissionActionType.PrecisionGame, DigitPark.Games.GameType.DigitRush, false, 5, DH, CW, G, 60, 8);
            CreateMissionSO(P, "weekly_tournament", "ms_weekly_tournament_title", "ms_weekly_tournament_desc",
                MissionActionType.PlayTournament, DigitPark.Games.GameType.DigitRush, false, 1, DM, CW, G, 50, 9);
        }

        #endregion

        private static void CreateBackground(Transform parent)
        {
            var bg = FindOrCreate(parent, "Background");
            bg.transform.SetAsFirstSibling();
            var rt = GetOrAdd<RectTransform>(bg);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            GetOrAdd<Image>(bg).color = DARK_BG;
        }

        #endregion

        #region Progress Bar

        private static void CreateProgressBar()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var progressGO = FindOrCreate(canvas.transform, "ProgressBar");
            var pRT = GetOrAdd<RectTransform>(progressGO);
            SetAnchors(pRT, 0, PROGRESS_BOT, 1, PROGRESS_TOP);

            var slider = GetOrAdd<Slider>(progressGO);
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0;
            slider.maxValue = 6;
            slider.wholeNumbers = true;
            slider.value = 4;
            slider.interactable = false;

            // Slider Background
            var sliderBg = FindOrCreate(progressGO.transform, "Background");
            var sbRT = GetOrAdd<RectTransform>(sliderBg);
            sbRT.anchorMin = Vector2.zero;
            sbRT.anchorMax = Vector2.one;
            sbRT.offsetMin = Vector2.zero;
            sbRT.offsetMax = Vector2.zero;
            GetOrAdd<Image>(sliderBg).color = new Color(0.1f, 0.12f, 0.15f, 1f);

            // Fill Area
            var fillArea = FindOrCreate(progressGO.transform, "Fill Area");
            var faRT = GetOrAdd<RectTransform>(fillArea);
            faRT.anchorMin = Vector2.zero;
            faRT.anchorMax = Vector2.one;
            faRT.offsetMin = Vector2.zero;
            faRT.offsetMax = Vector2.zero;

            var fill = FindOrCreate(fillArea.transform, "Fill");
            var fRT = GetOrAdd<RectTransform>(fill);
            fRT.anchorMin = Vector2.zero;
            fRT.anchorMax = Vector2.one;
            fRT.offsetMin = Vector2.zero;
            fRT.offsetMax = Vector2.zero;
            GetOrAdd<Image>(fill).color = CYAN_NEON;

            slider.fillRect = fRT;
            slider.handleRect = null;
            slider.targetGraphic = GetOrAdd<Image>(progressGO);
            GetOrAdd<Image>(progressGO).color = Color.clear;

            Debug.Log("[DailyMissionsUI] ProgressBar creado");
        }

        #endregion

        #region Top Bar (Back + Title + Info)

        private static void CreateTopBar()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var topBar = FindOrCreate(canvas.transform, "TopBar");
            var tbRT = GetOrAdd<RectTransform>(topBar);
            SetAnchors(tbRT, 0, TOPBAR_BOT, 1, TOPBAR_TOP);

            // BackButton - Neon Cyan prefab
            var oldBackBtn = topBar.transform.Find("BackButton");
            if (oldBackBtn != null) Object.DestroyImmediate(oldBackBtn.gameObject);

            GameObject backBtnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BACK_BUTTON_PREFAB);
            GameObject backBtn;
            if (backBtnPrefab != null)
            {
                backBtn = (GameObject)PrefabUtility.InstantiatePrefab(backBtnPrefab, topBar.transform);
                backBtn.name = "BackButton";
            }
            else
            {
                backBtn = FindOrCreate(topBar.transform, "BackButton");
                GetOrAdd<Image>(backBtn).color = CARD_BG;
                GetOrAdd<Button>(backBtn);
                Debug.LogWarning("[DailyMissionsUI] BackButton prefab not found, using fallback");
            }
            var bRT = GetOrAdd<RectTransform>(backBtn);
            bRT.anchorMin = new Vector2(0, 0.5f);
            bRT.anchorMax = new Vector2(0, 0.5f);
            bRT.pivot = new Vector2(0, 0.5f);
            bRT.anchoredPosition = new Vector2(20, 0);
            bRT.sizeDelta = new Vector2(50, 50);

            // Title (center)
            var title = FindOrCreate(topBar.transform, "TitleText");
            var tRT = GetOrAdd<RectTransform>(title);
            tRT.anchorMin = new Vector2(0.07f, 0f);
            tRT.anchorMax = new Vector2(0.53f, 1f);
            tRT.pivot = new Vector2(0.5f, 0.5f);
            tRT.sizeDelta = Vector2.zero;
            tRT.anchoredPosition = Vector2.zero;
            var tTMP = GetOrAdd<TextMeshProUGUI>(title);
            tTMP.text = "MISSIONS";
            tTMP.fontSize = FontSizes.H4;
            tTMP.color = CYAN_NEON;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.MidlineLeft;
            tTMP.raycastTarget = false;
            tTMP.enableAutoSizing = true;
            tTMP.fontSizeMin = FontSizes.AutoMinTitle;
            tTMP.fontSizeMax = FontSizes.H4;
            tTMP.overflowMode = TextOverflowModes.Ellipsis;

            // Currency pills (between title and info button)
            var pills = CurrencyHeaderBarHelper.CreateCurrencyPills(topBar.transform);
            var pillsRT = pills.GetComponent<RectTransform>();
            pillsRT.anchorMin = new Vector2(0.42f, 0.05f);
            pillsRT.anchorMax = new Vector2(0.95f, 0.95f);
            pillsRT.offsetMin = Vector2.zero;
            pillsRT.offsetMax = Vector2.zero;

            // Info Button (right)
            var infoBtn = FindOrCreate(topBar.transform, "InfoButton");
            var iRT = GetOrAdd<RectTransform>(infoBtn);
            iRT.anchorMin = new Vector2(1, 0.5f);
            iRT.anchorMax = new Vector2(1, 0.5f);
            iRT.pivot = new Vector2(1, 0.5f);
            iRT.anchoredPosition = new Vector2(-SIDE_PAD, 0);
            iRT.sizeDelta = new Vector2(40, 40);
            var iBg = GetOrAdd<Image>(infoBtn);
            iBg.color = CARD_BG;
            GetOrAdd<Button>(infoBtn).targetGraphic = iBg;
            var iOutline = GetOrAdd<Outline>(infoBtn);
            iOutline.effectColor = CYAN_DARK;
            iOutline.effectDistance = new Vector2(1, 1);

            var infoText = FindOrCreate(infoBtn.transform, "Text");
            var itRT = GetOrAdd<RectTransform>(infoText);
            itRT.anchorMin = Vector2.zero;
            itRT.anchorMax = Vector2.one;
            itRT.offsetMin = Vector2.zero;
            itRT.offsetMax = Vector2.zero;
            var itTMP = GetOrAdd<TextMeshProUGUI>(infoText);
            itTMP.text = "?";
            itTMP.fontSize = FontSizes.Body;
            itTMP.fontStyle = FontStyles.Bold;
            itTMP.color = CYAN_NEON;
            itTMP.alignment = TextAlignmentOptions.Center;

            Debug.Log("[DailyMissionsUI] TopBar creado (Back + MISIONES + Info)");
        }

        #endregion

        #region Timer Bar

        private static void CreateTimerBar()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var timerBar = FindOrCreate(canvas.transform, "TimerBar");
            var tbRT = GetOrAdd<RectTransform>(timerBar);
            SetAnchors(tbRT, 0, TIMER_BOT, 1, TIMER_TOP);
            tbRT.offsetMin = new Vector2(SIDE_PAD, 0);
            tbRT.offsetMax = new Vector2(-SIDE_PAD, 0);

            var tbBg = GetOrAdd<Image>(timerBar);
            tbBg.color = CARD_BG;
            var tbOutline = GetOrAdd<Outline>(timerBar);
            tbOutline.effectColor = new Color(ORANGE_TIMER.r * 0.4f, ORANGE_TIMER.g * 0.4f, ORANGE_TIMER.b * 0.4f, 1f);
            tbOutline.effectDistance = new Vector2(1, 1);

            // 3D depth shadow
            var timerShadow = timerBar.AddComponent<Shadow>();
            timerShadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            timerShadow.effectDistance = new Vector2(3, -4);

            var hlg = GetOrAdd<HorizontalLayoutGroup>(timerBar);
            hlg.spacing = 10;
            hlg.padding = new RectOffset(15, 15, 0, 0);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // Timer Icon
            var timerIcon = FindOrCreate(timerBar.transform, "TimerIcon");
            var iconImg = GetOrAdd<Image>(timerIcon);
            iconImg.color = ORANGE_TIMER;
            Sprite timerSprite = LoadIcon(UI_ICONS_PATH + "TimerIcon.png");
            if (timerSprite != null) { iconImg.sprite = timerSprite; iconImg.preserveAspect = true; }
            var iconLE = GetOrAdd<LayoutElement>(timerIcon);
            iconLE.minWidth = 24;
            iconLE.minHeight = 24;
            iconLE.preferredWidth = 24;
            iconLE.preferredHeight = 24;

            // Label
            var label = FindOrCreate(timerBar.transform, "Label");
            var lTMP = GetOrAdd<TextMeshProUGUI>(label);
            lTMP.text = "Resets in:";
            lTMP.fontSize = FontSizes.Body;
            lTMP.fontStyle = FontStyles.Bold;
            lTMP.color = TEXT_SECONDARY;
            lTMP.alignment = TextAlignmentOptions.MidlineLeft;
            var lLE = GetOrAdd<LayoutElement>(label);
            lLE.flexibleWidth = 1;

            // Countdown
            var countdown = FindOrCreate(timerBar.transform, "CountdownText");
            var cdTMP = GetOrAdd<TextMeshProUGUI>(countdown);
            cdTMP.text = "12:34:56";
            cdTMP.fontSize = FontSizes.Body;
            cdTMP.fontStyle = FontStyles.Bold;
            cdTMP.color = ORANGE_TIMER;
            cdTMP.alignment = TextAlignmentOptions.MidlineRight;
            var cdLE = GetOrAdd<LayoutElement>(countdown);
            cdLE.minWidth = 120;

            Debug.Log("[DailyMissionsUI] TimerBar creado");
        }

        #endregion

        #region Tab Bar (Daily / Weekly / Special)

        private static void CreateTabBar()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var tabBar = FindOrCreate(canvas.transform, "TabBar");
            var tbRT = GetOrAdd<RectTransform>(tabBar);
            SetAnchors(tbRT, 0, TABS_BOT, 1, TABS_TOP);
            tbRT.offsetMin = new Vector2(SIDE_PAD, 0);
            tbRT.offsetMax = new Vector2(-SIDE_PAD, 0);

            var hlg = GetOrAdd<HorizontalLayoutGroup>(tabBar);
            hlg.spacing = 8;
            hlg.padding = new RectOffset(0, 0, 0, 0);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // Daily Tab (activo por defecto)
            CreateTabButton(tabBar, "DailyTab", "DAILY", CYAN_NEON, true);
            // Weekly Tab
            CreateTabButton(tabBar, "WeeklyTab", "WEEKLY", PURPLE_WEEKLY, false);
            // Special Tab
            CreateTabButton(tabBar, "SpecialTab", "SPECIAL", GOLD, false);

            Debug.Log("[DailyMissionsUI] TabBar creado (Daily/Weekly/Special)");
        }

        private static void CreateTabButton(GameObject parent, string name, string label, Color accentColor, bool isActive)
        {
            var tab = FindOrCreate(parent.transform, name);
            var tabBg = GetOrAdd<Image>(tab);
            tabBg.color = isActive ? new Color(0f, 1f, 1f, 0.12f) : new Color(0.15f, 0.17f, 0.22f, 0.5f);
            var tabBtn = GetOrAdd<Button>(tab);
            tabBtn.targetGraphic = tabBg;

            var tabOutline = GetOrAdd<Outline>(tab);
            tabOutline.effectColor = isActive ? CYAN_NEON : new Color(0.2f, 0.2f, 0.25f, 0.5f);
            tabOutline.effectDistance = new Vector2(1, 1);

            var tabText = FindOrCreate(tab.transform, "Text");
            var ttRT = GetOrAdd<RectTransform>(tabText);
            ttRT.anchorMin = Vector2.zero;
            ttRT.anchorMax = Vector2.one;
            ttRT.offsetMin = Vector2.zero;
            ttRT.offsetMax = Vector2.zero;
            var ttTMP = GetOrAdd<TextMeshProUGUI>(tabText);
            ttTMP.text = label;
            ttTMP.fontSize = FontSizes.BodyLarge;
            ttTMP.fontStyle = FontStyles.Bold;
            ttTMP.color = isActive ? Color.white : TEXT_SECONDARY;
            ttTMP.alignment = TextAlignmentOptions.Center;
            ttTMP.enableAutoSizing = true;
            ttTMP.fontSizeMin = FontSizes.AutoMinBody;
            ttTMP.fontSizeMax = FontSizes.BodyLarge;
            ttTMP.overflowMode = TextOverflowModes.Ellipsis;

            // Indicator bar (bottom line)
            var indicator = FindOrCreate(tab.transform, "Indicator");
            var iRT = GetOrAdd<RectTransform>(indicator);
            iRT.anchorMin = new Vector2(0.1f, 0);
            iRT.anchorMax = new Vector2(0.9f, 0);
            iRT.pivot = new Vector2(0.5f, 0);
            iRT.anchoredPosition = Vector2.zero;
            iRT.sizeDelta = new Vector2(0, 3);
            var iImg = GetOrAdd<Image>(indicator);
            iImg.color = isActive ? CYAN_NEON : Color.clear;
        }

        #endregion

        #region Overall Progress

        private static void CreateOverallProgress()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var panel = FindOrCreate(canvas.transform, "OverallProgress");
            var pRT = GetOrAdd<RectTransform>(panel);
            SetAnchors(pRT, 0, OVERALL_BOT, 1, OVERALL_TOP);
            pRT.offsetMin = new Vector2(SIDE_PAD, 0);
            pRT.offsetMax = new Vector2(-SIDE_PAD, 0);

            var pBg = GetOrAdd<Image>(panel);
            pBg.color = CARD_BG;
            var pOutline = GetOrAdd<Outline>(panel);
            pOutline.effectColor = CYAN_DARK;
            pOutline.effectDistance = new Vector2(1, 1);

            // 3D depth shadow
            var panelShadow = panel.AddComponent<Shadow>();
            panelShadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            panelShadow.effectDistance = new Vector2(3, -4);

            var vlg = GetOrAdd<VerticalLayoutGroup>(panel);
            vlg.spacing = 8;
            vlg.padding = new RectOffset(15, 15, 10, 10);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Row 1: Title row
            var titleRow = FindOrCreate(panel.transform, "TitleRow");
            var trHlg = GetOrAdd<HorizontalLayoutGroup>(titleRow);
            trHlg.spacing = 0;
            trHlg.childAlignment = TextAnchor.MiddleCenter;
            trHlg.childControlWidth = true;
            trHlg.childControlHeight = true;
            trHlg.childForceExpandWidth = true;
            var trLE = GetOrAdd<LayoutElement>(titleRow);
            trLE.preferredHeight = 22;

            var titleLeft = FindOrCreate(titleRow.transform, "TitleLeft");
            var tlTMP = GetOrAdd<TextMeshProUGUI>(titleLeft);
            tlTMP.text = "Daily Progress";
            tlTMP.fontSize = FontSizes.Body;
            tlTMP.fontStyle = FontStyles.Bold;
            tlTMP.color = TEXT_WHITE;
            tlTMP.alignment = TextAlignmentOptions.MidlineLeft;

            var titleRight = FindOrCreate(titleRow.transform, "TitleRight");
            var trTMP = GetOrAdd<TextMeshProUGUI>(titleRight);
            trTMP.text = "4/6 Missions";
            trTMP.fontSize = FontSizes.Body;
            trTMP.fontStyle = FontStyles.Bold;
            trTMP.color = TEXT_SECONDARY;
            trTMP.alignment = TextAlignmentOptions.MidlineRight;

            // Row 2: Progress slider
            var progressContainer = FindOrCreate(panel.transform, "ProgressContainer");
            var pcLE = GetOrAdd<LayoutElement>(progressContainer);
            pcLE.preferredHeight = 28;

            var slider = GetOrAdd<Slider>(progressContainer);
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0;
            slider.maxValue = 6;
            slider.wholeNumbers = true;
            slider.value = 4;
            slider.interactable = false;

            // Slider Background
            var sliderBg = FindOrCreate(progressContainer.transform, "Background");
            var sbRT = GetOrAdd<RectTransform>(sliderBg);
            sbRT.anchorMin = Vector2.zero;
            sbRT.anchorMax = Vector2.one;
            sbRT.offsetMin = Vector2.zero;
            sbRT.offsetMax = Vector2.zero;
            GetOrAdd<Image>(sliderBg).color = new Color(0.1f, 0.12f, 0.15f, 1f);

            // Fill Area
            var fillArea = FindOrCreate(progressContainer.transform, "Fill Area");
            var faRT = GetOrAdd<RectTransform>(fillArea);
            faRT.anchorMin = Vector2.zero;
            faRT.anchorMax = Vector2.one;
            faRT.offsetMin = Vector2.zero;
            faRT.offsetMax = Vector2.zero;

            var fill = FindOrCreate(fillArea.transform, "Fill");
            var fRT = GetOrAdd<RectTransform>(fill);
            fRT.anchorMin = Vector2.zero;
            fRT.anchorMax = Vector2.one;
            fRT.offsetMin = Vector2.zero;
            fRT.offsetMax = Vector2.zero;
            GetOrAdd<Image>(fill).color = CYAN_NEON;

            slider.fillRect = fRT;
            slider.handleRect = null;
            slider.targetGraphic = GetOrAdd<Image>(progressContainer);
            GetOrAdd<Image>(progressContainer).color = Color.clear;

            // Reward Markers overlaid on progress bar
            CreateRewardMarker(progressContainer, "Marker3", 0.5f, "50", false);
            CreateRewardMarker(progressContainer, "Marker5", 0.833f, "100", false);
            CreateRewardMarker(progressContainer, "MarkerAll", 1.0f, "200", true);

            // Row 3: Bonus reward row
            var bonusRow = FindOrCreate(panel.transform, "BonusRow");
            var brHlg = GetOrAdd<HorizontalLayoutGroup>(bonusRow);
            brHlg.spacing = 8;
            brHlg.childAlignment = TextAnchor.MiddleCenter;
            brHlg.childControlWidth = false;
            brHlg.childControlHeight = true;
            brHlg.childForceExpandWidth = false;
            brHlg.childForceExpandHeight = false;
            var brLE = GetOrAdd<LayoutElement>(bonusRow);
            brLE.preferredHeight = 30;

            // Bonus text
            var bonusText = FindOrCreate(bonusRow.transform, "BonusRewardText");
            var btTMP = GetOrAdd<TextMeshProUGUI>(bonusText);
            btTMP.text = "Bonus: +100 DigitCoins";
            btTMP.fontSize = FontSizes.Body;
            btTMP.fontStyle = FontStyles.Bold;
            btTMP.color = GOLD;
            btTMP.alignment = TextAlignmentOptions.MidlineLeft;
            var btLE = GetOrAdd<LayoutElement>(bonusText);
            btLE.flexibleWidth = 1;

            // Claim bonus button
            var claimBonusBtn = FindOrCreate(bonusRow.transform, "ClaimBonusButton");
            var cbBg = GetOrAdd<Image>(claimBonusBtn);
            cbBg.color = GOLD;
            var cbBtn = GetOrAdd<Button>(claimBonusBtn);
            cbBtn.targetGraphic = cbBg;
            claimBonusBtn.SetActive(false); // Oculto hasta que se cumplan las misiones
            var cbLE = GetOrAdd<LayoutElement>(claimBonusBtn);
            cbLE.preferredWidth = 100;
            cbLE.preferredHeight = 28;

            var cbTextObj = FindOrCreate(claimBonusBtn.transform, "Text");
            var cbTextRT = GetOrAdd<RectTransform>(cbTextObj);
            cbTextRT.anchorMin = Vector2.zero;
            cbTextRT.anchorMax = Vector2.one;
            cbTextRT.offsetMin = Vector2.zero;
            cbTextRT.offsetMax = Vector2.zero;
            var cbTextTMP = GetOrAdd<TextMeshProUGUI>(cbTextObj);
            cbTextTMP.text = "Claim";
            cbTextTMP.fontSize = FontSizes.Body;
            cbTextTMP.fontStyle = FontStyles.Bold;
            cbTextTMP.color = TEXT_DARK;
            cbTextTMP.alignment = TextAlignmentOptions.Center;

            Debug.Log("[DailyMissionsUI] OverallProgress creado (con bonus)");
        }

        private static void CreateRewardMarker(GameObject parent, string name, float xPos, string reward, bool isBonus)
        {
            var marker = FindOrCreate(parent.transform, name);
            var mRT = GetOrAdd<RectTransform>(marker);
            mRT.anchorMin = new Vector2(xPos, 0.5f);
            mRT.anchorMax = new Vector2(xPos, 0.5f);
            mRT.pivot = new Vector2(0.5f, 0.5f);
            mRT.sizeDelta = new Vector2(28, 28);
            mRT.anchoredPosition = Vector2.zero;

            var mImg = GetOrAdd<Image>(marker);
            mImg.color = isBonus ? GOLD : CYAN_DARK;
            var mOutline = GetOrAdd<Outline>(marker);
            mOutline.effectColor = isBonus ? GOLD : CYAN_NEON;
            mOutline.effectDistance = new Vector2(1, 1);

            var rewardText = FindOrCreate(marker.transform, "RewardText");
            var rwRT = GetOrAdd<RectTransform>(rewardText);
            rwRT.anchorMin = Vector2.zero;
            rwRT.anchorMax = Vector2.one;
            rwRT.offsetMin = Vector2.zero;
            rwRT.offsetMax = Vector2.zero;
            var rwTMP = GetOrAdd<TextMeshProUGUI>(rewardText);
            rwTMP.text = reward;
            rwTMP.fontSize = FontSizes.Body;
            rwTMP.fontStyle = FontStyles.Bold;
            rwTMP.color = TEXT_WHITE;
            rwTMP.alignment = TextAlignmentOptions.Center;
        }

        #endregion

        #region Missions ScrollView

        private static void CreateMissionsScrollView()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var scrollView = FindOrCreate(canvas.transform, "ScrollView");
            var svRT = GetOrAdd<RectTransform>(scrollView);
            SetAnchors(svRT, 0, SCROLL_BOT, 1, SCROLL_TOP);

            var scrollRect = GetOrAdd<ScrollRect>(scrollView);
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.scrollSensitivity = 50f;

            GetOrAdd<Image>(scrollView).color = Color.clear;

            // Viewport
            var viewport = FindOrCreate(scrollView.transform, "Viewport");
            var vpRT = GetOrAdd<RectTransform>(viewport);
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.offsetMin = Vector2.zero;
            vpRT.offsetMax = Vector2.zero;
            var vpImg = GetOrAdd<Image>(viewport);
            vpImg.color = Color.clear;
            vpImg.raycastTarget = true;
            GetOrAdd<RectMask2D>(viewport);
            scrollRect.viewport = vpRT;

            // Content
            var content = FindOrCreate(viewport.transform, "Content");
            var cRT = GetOrAdd<RectTransform>(content);
            cRT.anchorMin = new Vector2(0, 1);
            cRT.anchorMax = new Vector2(1, 1);
            cRT.pivot = new Vector2(0.5f, 1);
            cRT.sizeDelta = Vector2.zero;
            scrollRect.content = cRT;

            var csf = GetOrAdd<ContentSizeFitter>(content);
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var vlg = GetOrAdd<VerticalLayoutGroup>(content);
            vlg.spacing = 12;
            vlg.padding = new RectOffset(5, 5, 10, 25);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // --- Section Header: MISIONES DIARIAS ---
            CreateSectionHeader(content, "DailyHeader", "DAILY MISSIONS", CYAN_NEON);

            // --- 6 Daily Mission Cards ---
            CreateMissionCard(content, "Mission1",
                "Play 3 Matches", "Play in any mode",
                "play_matches", 2, 3, "50", "digitcoins", false, false);

            CreateMissionCard(content, "Mission2",
                "Win 1 Match", "Win in any mode",
                "win_matches", 1, 1, "100", "digitcoins", true, false);

            CreateMissionCard(content, "Mission3",
                "Earn 500 Points", "Accumulate points in matches",
                "earn_points", 350, 500, "25", "digitgems", false, false);

            CreateMissionCard(content, "Mission4",
                "Play with a Friend", "Invite a friend to play",
                "play_matches", 0, 1, "50", "digitcoins", false, false);

            CreateMissionCard(content, "Mission5",
                "Complete 3 Minigames", "Play 3 different types",
                "complete_minigames", 3, 3, "200", "digitcoins", true, false);

            CreateMissionCard(content, "Mission6",
                "80% Precision", "Complete with 80%+ precision",
                "earn_points", 1, 3, "30", "xp", false, false);

            // --- Section Header: MISIONES SEMANALES ---
            CreateSectionHeader(content, "WeeklyHeader", "WEEKLY MISSIONS", PURPLE_WEEKLY);

            // --- 3 Weekly Mission Cards ---
            CreateMissionCard(content, "Weekly1",
                "Win 10 Matches", "Accumulate wins this week",
                "win_matches", 7, 10, "150", "digitgems", false, true);

            CreateMissionCard(content, "Weekly2",
                "Play 20 Matches", "Play in any mode",
                "play_matches", 15, 20, "500", "digitcoins", false, true);

            CreateMissionCard(content, "Weekly3",
                "Reach Top 3", "Finish on podium 5 times",
                "earn_points", 3, 5, "Premium DigitGems", "digitgems", false, true);

            // Empty State Text (oculto, se muestra cuando no hay misiones)
            var emptyState = FindOrCreate(content.transform, "EmptyStateText");
            var esTMP = GetOrAdd<TextMeshProUGUI>(emptyState);
            esTMP.text = "No missions available";
            esTMP.fontSize = FontSizes.Body;
            esTMP.fontStyle = FontStyles.Bold;
            esTMP.color = TEXT_SECONDARY;
            esTMP.alignment = TextAlignmentOptions.Center;
            var esLE = GetOrAdd<LayoutElement>(emptyState);
            esLE.preferredHeight = 60;
            emptyState.SetActive(false);

            Debug.Log("[DailyMissionsUI] ScrollView creado (6 diarias + 3 semanales)");
        }

        private static void CreateSectionHeader(GameObject parent, string name, string title, Color color)
        {
            var header = FindOrCreate(parent.transform, name);

            var hlg = GetOrAdd<HorizontalLayoutGroup>(header);
            hlg.spacing = 10;
            hlg.padding = new RectOffset(5, 5, 8, 5);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            var hLE = GetOrAdd<LayoutElement>(header);
            hLE.preferredHeight = 50;

            // Icon
            var iconGO = FindOrCreate(header.transform, "Icon");
            var iconImg = GetOrAdd<Image>(iconGO);
            iconImg.color = color;
            iconImg.preserveAspect = true;
            Sprite iconSprite = LoadIcon(MISSIONS_ICONS_PATH + "MissionsIcon.png");
            if (iconSprite != null) iconImg.sprite = iconSprite;
            var iconLE = GetOrAdd<LayoutElement>(iconGO);
            iconLE.minWidth = 24;
            iconLE.minHeight = 24;
            iconLE.preferredWidth = 24;
            iconLE.preferredHeight = 24;

            // Title
            var titleGO = FindOrCreate(header.transform, "Title");
            var tTMP = GetOrAdd<TextMeshProUGUI>(titleGO);
            tTMP.text = title;
            tTMP.fontSize = FontSizes.Body;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.color = color;
            tTMP.alignment = TextAlignmentOptions.MidlineLeft;
            var tLE = GetOrAdd<LayoutElement>(titleGO);
            tLE.minWidth = 350;
        }

        private static void CreateMissionCard(GameObject parent, string name,
            string title, string description, string iconType,
            int current, int target, string rewardAmount, string rewardType,
            bool isCompleted, bool isWeekly)
        {
            Color accentColor = isWeekly ? PURPLE_WEEKLY : CYAN_NEON;
            float progressPercent = target > 0 ? (float)current / target : 0f;

            var card = FindOrCreate(parent.transform, name);
            var cardBg = GetOrAdd<Image>(card);
            cardBg.color = CARD_BG;
            var cardOutline = GetOrAdd<Outline>(card);
            cardOutline.effectColor = isCompleted ? GREEN_SUCCESS : (isWeekly ? new Color(PURPLE_WEEKLY.r * 0.5f, PURPLE_WEEKLY.g * 0.5f, PURPLE_WEEKLY.b * 0.5f, 1f) : CYAN_DARK);
            cardOutline.effectDistance = new Vector2(1, 1);

            // 3D depth shadow
            var cardShadow = card.AddComponent<Shadow>();
            cardShadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            cardShadow.effectDistance = new Vector2(3, -4);

            var cardLE = GetOrAdd<LayoutElement>(card);
            cardLE.preferredHeight = 130;

            var cardHlg = GetOrAdd<HorizontalLayoutGroup>(card);
            cardHlg.spacing = 12;
            cardHlg.padding = new RectOffset(12, 12, 10, 10);
            cardHlg.childAlignment = TextAnchor.MiddleCenter;
            cardHlg.childControlWidth = true;
            cardHlg.childControlHeight = true;
            cardHlg.childForceExpandWidth = false;
            cardHlg.childForceExpandHeight = false;

            // === Left Section: Mission Icon Area ===
            var iconContainer = FindOrCreate(card.transform, "IconContainer");
            var icLE = GetOrAdd<LayoutElement>(iconContainer);
            icLE.minWidth = 60;
            icLE.minHeight = 60;
            icLE.preferredWidth = 60;
            icLE.preferredHeight = 60;

            // Icon Glow (behind icon)
            var iconGlow = FindOrCreate(iconContainer.transform, "IconGlow");
            var igRT = GetOrAdd<RectTransform>(iconGlow);
            igRT.anchorMin = Vector2.zero;
            igRT.anchorMax = Vector2.one;
            igRT.offsetMin = Vector2.zero;
            igRT.offsetMax = Vector2.zero;
            var igImg = GetOrAdd<Image>(iconGlow);
            igImg.color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.15f);

            // Mission Icon
            var missionIcon = FindOrCreate(iconContainer.transform, "MissionIcon");
            var miRT = GetOrAdd<RectTransform>(missionIcon);
            miRT.anchorMin = new Vector2(0.5f, 0.5f);
            miRT.anchorMax = new Vector2(0.5f, 0.5f);
            miRT.sizeDelta = new Vector2(50, 50);
            miRT.anchoredPosition = Vector2.zero;
            var miImg = GetOrAdd<Image>(missionIcon);
            miImg.preserveAspect = true;
            miImg.color = Color.white;

            string iconPath = MISSIONS_ICONS_PATH + "icon_mission_" + iconType + ".png";
            Sprite missionSprite = LoadIcon(iconPath);
            if (missionSprite != null) miImg.sprite = missionSprite;

            // Completed check overlay
            if (isCompleted)
            {
                var checkOverlay = FindOrCreate(iconContainer.transform, "CheckOverlay");
                var coRT = GetOrAdd<RectTransform>(checkOverlay);
                coRT.anchorMin = new Vector2(0.6f, 0);
                coRT.anchorMax = new Vector2(1, 0.4f);
                coRT.offsetMin = Vector2.zero;
                coRT.offsetMax = Vector2.zero;
                var coTMP = GetOrAdd<TextMeshProUGUI>(checkOverlay);
                coTMP.text = "\u2713";
                coTMP.fontSize = FontSizes.Body;
                coTMP.fontStyle = FontStyles.Bold;
                coTMP.color = GREEN_SUCCESS;
                coTMP.alignment = TextAlignmentOptions.Center;
            }

            // === Center Section: Info ===
            var infoPanel = FindOrCreate(card.transform, "InfoPanel");
            var ipVlg = GetOrAdd<VerticalLayoutGroup>(infoPanel);
            ipVlg.spacing = 4;
            ipVlg.childAlignment = TextAnchor.MiddleLeft;
            ipVlg.childControlWidth = true;
            ipVlg.childControlHeight = false;
            ipVlg.childForceExpandWidth = true;
            ipVlg.childForceExpandHeight = false;
            var ipLE = GetOrAdd<LayoutElement>(infoPanel);
            ipLE.flexibleWidth = 1;

            // Title
            var titleGO = FindOrCreate(infoPanel.transform, "Title");
            var titleTMP = GetOrAdd<TextMeshProUGUI>(titleGO);
            titleTMP.text = title;
            titleTMP.fontSize = FontSizes.Body;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.color = isCompleted ? GREEN_SUCCESS : TEXT_WHITE;
            titleTMP.alignment = TextAlignmentOptions.MidlineLeft;
            titleTMP.overflowMode = TextOverflowModes.Ellipsis;
            var titleLE = GetOrAdd<LayoutElement>(titleGO);
            titleLE.preferredHeight = 32;

            // Description
            var descGO = FindOrCreate(infoPanel.transform, "Description");
            var descTMP = GetOrAdd<TextMeshProUGUI>(descGO);
            descTMP.text = description;
            descTMP.fontSize = FontSizes.Body;
            descTMP.fontStyle = FontStyles.Bold;
            descTMP.color = TEXT_SECONDARY;
            descTMP.alignment = TextAlignmentOptions.MidlineLeft;
            descTMP.overflowMode = TextOverflowModes.Ellipsis;
            var descLE = GetOrAdd<LayoutElement>(descGO);
            descLE.preferredHeight = 28;

            // Progress Row
            var progressRow = FindOrCreate(infoPanel.transform, "ProgressRow");
            var prHlg = GetOrAdd<HorizontalLayoutGroup>(progressRow);
            prHlg.spacing = 8;
            prHlg.childAlignment = TextAnchor.MiddleLeft;
            prHlg.childControlWidth = false;
            prHlg.childControlHeight = true;
            prHlg.childForceExpandWidth = false;
            prHlg.childForceExpandHeight = false;
            var prLE = GetOrAdd<LayoutElement>(progressRow);
            prLE.preferredHeight = 24;

            // Progress bar mini
            var progressBar = FindOrCreate(progressRow.transform, "ProgressBar");
            var pbBg = GetOrAdd<Image>(progressBar);
            pbBg.color = new Color(0.1f, 0.12f, 0.15f, 1f);
            var pbLE = GetOrAdd<LayoutElement>(progressBar);
            pbLE.minWidth = 120;
            pbLE.minHeight = 14;
            pbLE.preferredWidth = 120;
            pbLE.preferredHeight = 14;

            var progressFill = FindOrCreate(progressBar.transform, "Fill");
            var pfRT = GetOrAdd<RectTransform>(progressFill);
            pfRT.anchorMin = Vector2.zero;
            pfRT.anchorMax = new Vector2(progressPercent, 1);
            pfRT.offsetMin = Vector2.zero;
            pfRT.offsetMax = Vector2.zero;
            var pfImg = GetOrAdd<Image>(progressFill);
            pfImg.color = isCompleted ? GREEN_SUCCESS : accentColor;

            // Progress text
            var progressText = FindOrCreate(progressRow.transform, "ProgressText");
            var ptTMP = GetOrAdd<TextMeshProUGUI>(progressText);
            ptTMP.text = current + "/" + target;
            ptTMP.fontSize = FontSizes.Body;
            ptTMP.fontStyle = FontStyles.Bold;
            ptTMP.color = TEXT_SECONDARY;
            ptTMP.alignment = TextAlignmentOptions.MidlineLeft;
            var ptLE = GetOrAdd<LayoutElement>(progressText);
            ptLE.minWidth = 60;

            // === Right Section: Reward + Action ===
            var rewardPanel = FindOrCreate(card.transform, "RewardPanel");
            var rpVlg = GetOrAdd<VerticalLayoutGroup>(rewardPanel);
            rpVlg.spacing = 6;
            rpVlg.childAlignment = TextAnchor.MiddleCenter;
            rpVlg.childControlWidth = true;
            rpVlg.childControlHeight = false;
            rpVlg.childForceExpandWidth = true;
            rpVlg.childForceExpandHeight = false;
            var rpLE = GetOrAdd<LayoutElement>(rewardPanel);
            rpLE.minWidth = 80;
            rpLE.preferredWidth = 80;

            // Reward display (HLG)
            var rewardDisplay = FindOrCreate(rewardPanel.transform, "RewardDisplay");
            var rdHlg = GetOrAdd<HorizontalLayoutGroup>(rewardDisplay);
            rdHlg.spacing = 4;
            rdHlg.childAlignment = TextAnchor.MiddleCenter;
            rdHlg.childControlWidth = false;
            rdHlg.childControlHeight = true;
            rdHlg.childForceExpandWidth = false;
            rdHlg.childForceExpandHeight = false;
            var rdLE = GetOrAdd<LayoutElement>(rewardDisplay);
            rdLE.preferredHeight = 24;

            // Currency icon
            Color rewardColor;
            string currencyIconPath;
            switch (rewardType)
            {
                case "digitgems":
                    rewardColor = GEM_COLOR;
                    currencyIconPath = CURRENCY_ICONS_PATH + "icon_digitgem_single.png";
                    break;
                case "xp":
                    rewardColor = XP_COLOR;
                    currencyIconPath = UI_ICONS_PATH + "icon_xp.png";
                    break;
                default: // digitcoins
                    rewardColor = COIN_COLOR;
                    currencyIconPath = CURRENCY_ICONS_PATH + "icon_digitcoin_single.png";
                    break;
            }

            var currencyIcon = FindOrCreate(rewardDisplay.transform, "CurrencyIcon");
            var ciImg = GetOrAdd<Image>(currencyIcon);
            ciImg.color = rewardColor;
            ciImg.preserveAspect = true;
            Sprite currencySprite = LoadIcon(currencyIconPath);
            if (currencySprite != null) ciImg.sprite = currencySprite;
            var ciLE = GetOrAdd<LayoutElement>(currencyIcon);
            ciLE.minWidth = 20;
            ciLE.minHeight = 20;
            ciLE.preferredWidth = 20;
            ciLE.preferredHeight = 20;

            // Amount text
            var amountText = FindOrCreate(rewardDisplay.transform, "Amount");
            var atTMP = GetOrAdd<TextMeshProUGUI>(amountText);
            atTMP.text = rewardAmount;
            atTMP.fontSize = FontSizes.Body;
            atTMP.fontStyle = FontStyles.Bold;
            atTMP.color = rewardColor;
            atTMP.alignment = TextAlignmentOptions.MidlineLeft;
            var atLE = GetOrAdd<LayoutElement>(amountText);
            atLE.minWidth = 50;

            // Action button
            var actionBtn = FindOrCreate(rewardPanel.transform, "ActionButton");
            var abBg = GetOrAdd<Image>(actionBtn);
            var abBtn = GetOrAdd<Button>(actionBtn);
            abBtn.targetGraphic = abBg;
            var abLE = GetOrAdd<LayoutElement>(actionBtn);
            abLE.preferredHeight = 32;

            var actionText = FindOrCreate(actionBtn.transform, "Text");
            var actRT = GetOrAdd<RectTransform>(actionText);
            actRT.anchorMin = Vector2.zero;
            actRT.anchorMax = Vector2.one;
            actRT.offsetMin = Vector2.zero;
            actRT.offsetMax = Vector2.zero;
            var actTMP = GetOrAdd<TextMeshProUGUI>(actionText);

            if (isCompleted)
            {
                abBg.color = GREEN_SUCCESS;
                actTMP.text = "Claim";
                actTMP.fontSize = FontSizes.Body;
                actTMP.fontStyle = FontStyles.Bold;
                actTMP.color = TEXT_DARK;
                actTMP.alignment = TextAlignmentOptions.Center;
                var abOutline = GetOrAdd<Outline>(actionBtn);
                abOutline.effectColor = GREEN_SUCCESS;
                abOutline.effectDistance = new Vector2(1, 1);
            }
            else
            {
                abBg.color = CARD_BG;
                abBtn.interactable = false;
                actTMP.text = "In Progress";
                actTMP.fontSize = FontSizes.Body;
                actTMP.fontStyle = FontStyles.Bold;
                actTMP.color = TEXT_SECONDARY;
                actTMP.alignment = TextAlignmentOptions.Center;
            }
        }

        #endregion

        #region Reward Claim Popup

        private static void CreateRewardClaimPopup()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            // Blocker
            var blocker = FindOrCreate(canvas.transform, "RewardClaimBlocker");
            blocker.SetActive(false);
            blocker.transform.SetAsLastSibling();

            var bkRT = GetOrAdd<RectTransform>(blocker);
            bkRT.anchorMin = Vector2.zero;
            bkRT.anchorMax = Vector2.one;
            bkRT.offsetMin = Vector2.zero;
            bkRT.offsetMax = Vector2.zero;
            GetOrAdd<Image>(blocker).color = new Color(0f, 0f, 0f, 0.85f);
            var bkBtn = GetOrAdd<Button>(blocker);
            bkBtn.transition = Selectable.Transition.None;

            // Popup
            var popup = FindOrCreate(blocker.transform, "RewardPopup");
            var ppRT = GetOrAdd<RectTransform>(popup);
            ppRT.anchorMin = new Vector2(0.5f, 0.5f);
            ppRT.anchorMax = new Vector2(0.5f, 0.5f);
            ppRT.pivot = new Vector2(0.5f, 0.5f);
            ppRT.sizeDelta = new Vector2(480, 530);
            ppRT.anchoredPosition = Vector2.zero;

            var ppBg = GetOrAdd<Image>(popup);
            ppBg.color = CARD_BG;
            var ppOutline = GetOrAdd<Outline>(popup);
            ppOutline.effectColor = GOLD;
            ppOutline.effectDistance = new Vector2(2, 2);

            var vlg = GetOrAdd<VerticalLayoutGroup>(popup);
            vlg.spacing = 18;
            vlg.padding = new RectOffset(25, 25, 25, 25);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Celebration Icon
            var celebIcon = FindOrCreate(popup.transform, "CelebrationIcon");
            var ceImg = GetOrAdd<Image>(celebIcon);
            ceImg.color = GOLD;
            ceImg.preserveAspect = true;
            Sprite claimSprite = LoadIcon("Assets/_Project/Art/Icons/DailyRewards/icon_gift_open_basic.png");
            if (claimSprite != null) ceImg.sprite = claimSprite;
            var ceLE = GetOrAdd<LayoutElement>(celebIcon);
            ceLE.preferredWidth = 60;
            ceLE.preferredHeight = 60;

            // Title
            var popupTitle = FindOrCreate(popup.transform, "Title");
            var ptTMP = GetOrAdd<TextMeshProUGUI>(popupTitle);
            ptTMP.text = "Mission Completed!";
            ptTMP.fontSize = FontSizes.H4;
            ptTMP.fontStyle = FontStyles.Bold;
            ptTMP.color = GOLD;
            ptTMP.alignment = TextAlignmentOptions.Center;
            var ptLE = GetOrAdd<LayoutElement>(popupTitle);
            ptLE.preferredHeight = 48;

            // Mission Name (detailTitleText)
            var missionName = FindOrCreate(popup.transform, "MissionName");
            var mnTMP = GetOrAdd<TextMeshProUGUI>(missionName);
            mnTMP.text = "Mission name";
            mnTMP.fontSize = FontSizes.Body;
            mnTMP.fontStyle = FontStyles.Bold;
            mnTMP.color = TEXT_SECONDARY;
            mnTMP.alignment = TextAlignmentOptions.Center;
            var mnLE = GetOrAdd<LayoutElement>(missionName);
            mnLE.preferredHeight = 30;

            // Detail Description (detailDescriptionText)
            var detailDesc = FindOrCreate(popup.transform, "DetailDescription");
            var ddTMP = GetOrAdd<TextMeshProUGUI>(detailDesc);
            ddTMP.text = "Mission description";
            ddTMP.fontSize = FontSizes.Body;
            ddTMP.fontStyle = FontStyles.Bold;
            ddTMP.color = TEXT_SECONDARY;
            ddTMP.alignment = TextAlignmentOptions.Center;
            var ddLE = GetOrAdd<LayoutElement>(detailDesc);
            ddLE.preferredHeight = 28;

            // Detail Progress Bar (detailProgressBar)
            var detailProgressContainer = FindOrCreate(popup.transform, "DetailProgressBar");
            var dpcLE = GetOrAdd<LayoutElement>(detailProgressContainer);
            dpcLE.preferredHeight = 20;

            var detailSlider = GetOrAdd<Slider>(detailProgressContainer);
            detailSlider.direction = Slider.Direction.LeftToRight;
            detailSlider.minValue = 0;
            detailSlider.maxValue = 100;
            detailSlider.value = 60;
            detailSlider.interactable = false;

            var dsBg = FindOrCreate(detailProgressContainer.transform, "Background");
            var dsBgRT = GetOrAdd<RectTransform>(dsBg);
            dsBgRT.anchorMin = Vector2.zero;
            dsBgRT.anchorMax = Vector2.one;
            dsBgRT.offsetMin = Vector2.zero;
            dsBgRT.offsetMax = Vector2.zero;
            GetOrAdd<Image>(dsBg).color = new Color(0.1f, 0.12f, 0.15f, 1f);

            var dsFillArea = FindOrCreate(detailProgressContainer.transform, "Fill Area");
            var dsFaRT = GetOrAdd<RectTransform>(dsFillArea);
            dsFaRT.anchorMin = Vector2.zero;
            dsFaRT.anchorMax = Vector2.one;
            dsFaRT.offsetMin = Vector2.zero;
            dsFaRT.offsetMax = Vector2.zero;

            var dsFill = FindOrCreate(dsFillArea.transform, "Fill");
            var dsFRT = GetOrAdd<RectTransform>(dsFill);
            dsFRT.anchorMin = Vector2.zero;
            dsFRT.anchorMax = Vector2.one;
            dsFRT.offsetMin = Vector2.zero;
            dsFRT.offsetMax = Vector2.zero;
            GetOrAdd<Image>(dsFill).color = CYAN_NEON;

            detailSlider.fillRect = dsFRT;
            detailSlider.handleRect = null;
            detailSlider.targetGraphic = GetOrAdd<Image>(detailProgressContainer);
            GetOrAdd<Image>(detailProgressContainer).color = Color.clear;

            // Detail Progress Text (detailProgressText)
            var detailProgressText = FindOrCreate(popup.transform, "DetailProgressText");
            var dptTMP = GetOrAdd<TextMeshProUGUI>(detailProgressText);
            dptTMP.text = "3/5";
            dptTMP.fontSize = FontSizes.Body;
            dptTMP.fontStyle = FontStyles.Bold;
            dptTMP.color = CYAN_NEON;
            dptTMP.alignment = TextAlignmentOptions.Center;
            var dptLE = GetOrAdd<LayoutElement>(detailProgressText);
            dptLE.preferredHeight = 26;

            // Reward Display
            var rewardDisplay = FindOrCreate(popup.transform, "RewardDisplay");
            var rdHlg = GetOrAdd<HorizontalLayoutGroup>(rewardDisplay);
            rdHlg.spacing = 10;
            rdHlg.childAlignment = TextAnchor.MiddleCenter;
            rdHlg.childControlWidth = false;
            rdHlg.childControlHeight = true;
            rdHlg.childForceExpandWidth = false;
            rdHlg.childForceExpandHeight = false;
            var rdLE = GetOrAdd<LayoutElement>(rewardDisplay);
            rdLE.preferredHeight = 50;

            // Reward Icon
            var rewardIcon = FindOrCreate(rewardDisplay.transform, "Icon");
            var riImg = GetOrAdd<Image>(rewardIcon);
            riImg.color = COIN_COLOR;
            riImg.preserveAspect = true;
            Sprite coinSprite = LoadIcon(CURRENCY_ICONS_PATH + "icon_digitcoin_single.png");
            if (coinSprite != null) riImg.sprite = coinSprite;
            var riLE = GetOrAdd<LayoutElement>(rewardIcon);
            riLE.minWidth = 40;
            riLE.minHeight = 40;
            riLE.preferredWidth = 40;
            riLE.preferredHeight = 40;

            // Reward Amount
            var rewardAmount = FindOrCreate(rewardDisplay.transform, "Amount");
            var raTMP = GetOrAdd<TextMeshProUGUI>(rewardAmount);
            raTMP.text = "+100";
            raTMP.fontSize = FontSizes.Subtitle;
            raTMP.fontStyle = FontStyles.Bold;
            raTMP.color = COIN_COLOR;
            raTMP.alignment = TextAlignmentOptions.MidlineLeft;
            var raLE = GetOrAdd<LayoutElement>(rewardAmount);
            raLE.minWidth = 120;

            // Collect Button
            var collectBtn = FindOrCreate(popup.transform, "CollectButton");
            var cbBg = GetOrAdd<Image>(collectBtn);
            cbBg.color = GREEN_SUCCESS;
            GetOrAdd<Button>(collectBtn).targetGraphic = cbBg;
            var cbLE = GetOrAdd<LayoutElement>(collectBtn);
            cbLE.preferredHeight = 56;

            var collectText = FindOrCreate(collectBtn.transform, "Text");
            var ctRT = GetOrAdd<RectTransform>(collectText);
            ctRT.anchorMin = Vector2.zero;
            ctRT.anchorMax = Vector2.one;
            ctRT.offsetMin = Vector2.zero;
            ctRT.offsetMax = Vector2.zero;
            var ctTMP = GetOrAdd<TextMeshProUGUI>(collectText);
            ctTMP.text = "Collect";
            ctTMP.fontSize = FontSizes.Body;
            ctTMP.fontStyle = FontStyles.Bold;
            ctTMP.color = TEXT_DARK;
            ctTMP.alignment = TextAlignmentOptions.Center;

            Debug.Log("[DailyMissionsUI] RewardClaimPopup creado");
        }

        #endregion

        #region Manager References

        private static void SetupManagerReferences()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            Transform r = canvas.transform;

            // --- DailyMissionsManager (escena principal) ---
            var dailyManager = Object.FindFirstObjectByType<DailyMissionsManager>();
            if (dailyManager != null)
            {
                var so = new SerializedObject(dailyManager);
                int assigned = 0;

                // UI - Header
                assigned += SetRef(so, "backButton", FindInPath<Button>(r, "TopBar/BackButton"));
                assigned += SetRef(so, "titleText", FindInPath<TextMeshProUGUI>(r, "TopBar/TitleText"));
                assigned += SetRef(so, "refreshTimerText", FindInPath<TextMeshProUGUI>(r, "TimerBar/CountdownText"));
                assigned += SetRef(so, "totalPointsText", FindInPath<TextMeshProUGUI>(r, "OverallProgress/TitleRow/TitleRight"));

                // UI - Tabs
                assigned += SetRef(so, "dailyTab", FindInPath<Button>(r, "TabBar/DailyTab"));
                assigned += SetRef(so, "weeklyTab", FindInPath<Button>(r, "TabBar/WeeklyTab"));
                assigned += SetRef(so, "specialTab", FindInPath<Button>(r, "TabBar/SpecialTab"));

                // UI - Progress Summary
                assigned += SetRef(so, "dailyProgressBar", FindInPath<Slider>(r, "OverallProgress/ProgressContainer"));
                assigned += SetRef(so, "dailyProgressText", FindInPath<TextMeshProUGUI>(r, "OverallProgress/TitleRow/TitleRight"));
                assigned += SetRef(so, "bonusRewardText", FindInPath<TextMeshProUGUI>(r, "OverallProgress/BonusRow/BonusRewardText"));
                assigned += SetRef(so, "claimBonusButton", FindInPath<Button>(r, "OverallProgress/BonusRow/ClaimBonusButton"));

                // UI - Missions List
                assigned += SetRef(so, "missionsContainer", FindInPath<Transform>(r, "ScrollView/Viewport/Content"));
                assigned += SetRef(so, "scrollRect", FindInPath<ScrollRect>(r, "ScrollView"));
                assigned += SetRef(so, "emptyStateText", FindInPath<TextMeshProUGUI>(r, "ScrollView/Viewport/Content/EmptyStateText"));

                // UI - Reward Popup
                var blockerGO = r.Find("RewardClaimBlocker");
                if (blockerGO != null)
                {
                    var rewardProp = so.FindProperty("rewardPopup");
                    if (rewardProp != null) { rewardProp.objectReferenceValue = blockerGO.gameObject; assigned++; }
                }
                assigned += SetRef(so, "rewardPopupText", FindInPath<TextMeshProUGUI>(r, "RewardClaimBlocker/RewardPopup/RewardDisplay/Amount"));
                assigned += SetRef(so, "rewardPopupIcon", FindInPath<Image>(r, "RewardClaimBlocker/RewardPopup/RewardDisplay/Icon"));

                // UI - Detail Panel (reutiliza popup como detail)
                var popupGO = r.Find("RewardClaimBlocker");
                if (popupGO != null)
                {
                    var detailProp = so.FindProperty("missionDetailPanel");
                    if (detailProp != null) { detailProp.objectReferenceValue = popupGO.gameObject; assigned++; }
                }
                assigned += SetRef(so, "detailTitleText", FindInPath<TextMeshProUGUI>(r, "RewardClaimBlocker/RewardPopup/MissionName"));
                assigned += SetRef(so, "detailDescriptionText", FindInPath<TextMeshProUGUI>(r, "RewardClaimBlocker/RewardPopup/DetailDescription"));
                assigned += SetRef(so, "detailProgressBar", FindInPath<Slider>(r, "RewardClaimBlocker/RewardPopup/DetailProgressBar"));
                assigned += SetRef(so, "detailProgressText", FindInPath<TextMeshProUGUI>(r, "RewardClaimBlocker/RewardPopup/DetailProgressText"));
                assigned += SetRef(so, "detailRewardText", FindInPath<TextMeshProUGUI>(r, "RewardClaimBlocker/RewardPopup/RewardDisplay/Amount"));
                assigned += SetRef(so, "claimRewardButton", FindInPath<Button>(r, "RewardClaimBlocker/RewardPopup/CollectButton"));
                assigned += SetRef(so, "closeDetailButton", FindInPath<Button>(r, "RewardClaimBlocker"));

                // Reward Icons (cargar desde assets)
                var coinSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CURRENCY_ICONS_PATH + "icon_digitcoin_single.png");
                var gemSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CURRENCY_ICONS_PATH + "icon_digitgem_single.png");
                if (coinSprite != null) { var p = so.FindProperty("coinIcon"); if (p != null) { p.objectReferenceValue = coinSprite; assigned++; } }
                if (gemSprite != null) { var p = so.FindProperty("gemIcon"); if (p != null) { p.objectReferenceValue = gemSprite; assigned++; } }

                // Mission Pools (SO) - cargar desde assets conocidos
                var dailyPool = AssetDatabase.LoadAssetAtPath<MissionPoolSO>("Assets/_Project/Data/Missions/DailyMissionPool.asset");
                var weeklyPool = AssetDatabase.LoadAssetAtPath<MissionPoolSO>("Assets/_Project/Data/Missions/WeeklyMissionPool.asset");
                if (dailyPool != null) { var p = so.FindProperty("dailyPool"); if (p != null) { p.objectReferenceValue = dailyPool; assigned++; } }
                if (weeklyPool != null) { var p = so.FindProperty("weeklyPool"); if (p != null) { p.objectReferenceValue = weeklyPool; assigned++; } }

                // MissionCard Prefab
                var cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Monetization/DailyMissions/MissionCard.prefab");
                if (cardPrefab != null) { var p = so.FindProperty("missionCardPrefab"); if (p != null) { p.objectReferenceValue = cardPrefab; assigned++; } }

                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(dailyManager);
                Debug.Log($"[DailyMissionsUI] DailyMissionsManager: {assigned} referencias asignadas");
            }
            else
            {
                Debug.LogWarning("[DailyMissionsUI] DailyMissionsManager no encontrado en la escena. Agrega el componente primero.");
            }

            // --- MissionsManager legacy (Progression) ---
            var progressionManager = Object.FindFirstObjectByType<DigitPark.Progression.MissionsManager>();
            if (progressionManager != null)
            {
                var so = new SerializedObject(progressionManager);
                var dailyProp = so.FindProperty("dailyMissionsCount");
                if (dailyProp != null) dailyProp.intValue = 6;
                var weeklyProp = so.FindProperty("weeklyMissionsCount");
                if (weeklyProp != null) weeklyProp.intValue = 3;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(progressionManager);
                Debug.Log("[DailyMissionsUI] MissionsManager (Progression): configuracion sincronizada");
            }

            // Conectar BackButton a SceneNavigator
            var backBtn = FindInPath<Button>(r, "TopBar/BackButton");
            if (backBtn != null)
            {
                var navigator = Object.FindFirstObjectByType<DigitPark.Monetization.SceneNavigator>();
                if (navigator != null)
                {
                    backBtn.onClick.RemoveAllListeners();
                    UnityEditor.Events.UnityEventTools.AddStringPersistentListener(
                        backBtn.onClick,
                        navigator.NavigateTo,
                        DigitPark.Monetization.SceneNavigator.Scenes.MAIN_MENU
                    );
                    Debug.Log("[DailyMissionsUI] BackButton conectado a SceneNavigator -> MainMenu");
                }
            }
        }

        private static int SetRef<T>(SerializedObject so, string propName, T component) where T : Object
        {
            if (component == null) return 0;
            var prop = so.FindProperty(propName);
            if (prop == null) return 0;
            prop.objectReferenceValue = component;
            return 1;
        }

        private static T FindInPath<T>(Transform root, string path) where T : Component
        {
            Transform t = root;
            foreach (string part in path.Split('/'))
            {
                t = t.Find(part);
                if (t == null) return null;
            }
            return t.GetComponent<T>();
        }

        #endregion

        #region Helpers

        private static void CleanupOldUI()
        {
            string[] toClean = { "Background", "SafeArea" };
            foreach (var canvas in Object.FindObjectsOfType<Canvas>(true))
            {
                if (canvas.transform.parent != null) continue;
                // No tocar TransitionCanvas ni EffectsCanvas
                if (canvas.gameObject.name.Contains("Transition") ||
                    canvas.gameObject.name.Contains("Effects")) continue;
                foreach (string name in toClean)
                {
                    Transform t = canvas.transform.Find(name);
                    if (t != null) Object.DestroyImmediate(t.gameObject);
                }
            }
        }

        private static void CleanupOldElements(Transform parent)
        {
            var toDestroy = new System.Collections.Generic.List<GameObject>();
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                string name = child.gameObject.name;
                if (name == "TransitionCanvas" || name == "EventSystem")
                    continue;
                toDestroy.Add(child.gameObject);
            }
            foreach (var go in toDestroy)
                Object.DestroyImmediate(go);
        }

        private static GameObject FindOrCreate(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null) return existing.gameObject;
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static T GetOrAdd<T>(GameObject obj) where T : Component
        {
            T c = obj.GetComponent<T>();
            if (c == null) c = obj.AddComponent<T>();
            return c;
        }

        private static void SetAnchors(RectTransform rt, float xMin, float yMin, float xMax, float yMax)
        {
            rt.anchorMin = new Vector2(xMin, yMin);
            rt.anchorMax = new Vector2(xMax, yMax);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static Sprite LoadIcon(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        #endregion
    }
}
