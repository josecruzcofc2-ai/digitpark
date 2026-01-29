using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;

namespace DigitPark.Editor
{
    /// <summary>
    /// Asigna automáticamente los prefabs a los managers que los necesitan.
    /// Usa las rutas correctas según la estructura de prefabs organizada.
    /// </summary>
    public class PrefabReferenceAssigner : EditorWindow
    {
        private Vector2 scrollPos;
        private List<string> logs = new List<string>();
        private static List<string> batchLogs = new List<string>();

        // Rutas base de prefabs
        private const string PREFAB_BASE = "Assets/_Project/Prefabs";
        private const string SCENES_PATH = "Assets/_Project/Scenes";

        // Mapeo de prefabs
        private static readonly Dictionary<string, string> PrefabPaths = new Dictionary<string, string>
        {
            // Social
            { "LeaderboardEntry", "Social/LeaderboardEntry.prefab" },
            { "PlayerSearchItem", "Social/PlayerSearchItem.prefab" },

            // Onboarding
            { "StepDotItem", "Onboarding/StepDotItem.prefab" },
            { "AvatarOptionItem", "Onboarding/AvatarOptionItem.prefab" },

            // Monetization
            { "TrophyCard", "Monetization/TrophyCard.prefab" },
            { "AchievementItem", "Monetization/Achievements/AchievementItem.prefab" },
            { "CategoryHeader", "Monetization/Achievements/CategoryHeader.prefab" },
            { "RewardTierItem", "Monetization/BattlePass/RewardTierItem.prefab" },
            { "MissionItem", "Monetization/DailyMissions/MissionItem.prefab" },
            { "RewardDayItem", "Monetization/DailyRewards/RewardDayItem.prefab" },
            { "ChestRewardItem", "Monetization/ChestOpening/ChestRewardItem.prefab" },

            // Games
            { "OnlineWinPanel", "Games/WinPanels/OnlineWinPanel.prefab" },
            { "OnlineLosePanel", "Games/WinPanels/OnlineLosePanel.prefab" },
            { "WinPanel_Normal", "Games/WinPanels/WinPanel_Normal.prefab" },
            { "LosePanel_Normal", "Games/WinPanels/LosePanel_Normal.prefab" },
            { "WinPanel_RealMoney", "Games/WinPanels/WinPanel_RealMoney.prefab" },
            { "LosePanel_RealMoney", "Games/WinPanels/LosePanel_RealMoney.prefab" },

            // Tournaments
            { "TournamentItem", "Tournaments/Browser/TournamentItem.prefab" },
            { "TournamentSearchItem", "Tournaments/Browser/TournamentSearchItem.prefab" },
            { "TournamentMyItem", "Tournaments/Browser/TournamentMyItem.prefab" },
            { "PrizeRowItem", "Tournaments/Lobby/PrizeRowItem.prefab" },
            { "ParticipantItem", "Tournaments/Lobby/ParticipantItem.prefab" },

            // CashBattle
            { "TransactionItemUI", "CashBattle/Wallet/TransactionItemUI.prefab" },
            { "DepositOptionUI", "CashBattle/Wallet/DepositOptionUI.prefab" },
            { "MatchHistoryItem", "CashBattle/History/MatchHistoryItem.prefab" },
            { "TournamentCardUI", "CashBattle/Tournaments/TournamentCardUI.prefab" },

            // Common
            { "ConfirmPanel", "Common/ConfirmPanel.prefab" },
            { "ErrorPanel", "Common/ErrorPanel.prefab" },
            { "BackButton", "Common/BackButton.prefab" },
            { "PlayerCard", "Common/PlayerCard.prefab" }
        };

        [MenuItem("DigitPark/Tools/Prefab Reference Assigner", false, 51)]
        public static void ShowWindow()
        {
            GetWindow<PrefabReferenceAssigner>("Prefab Assigner");
        }

        private void OnGUI()
        {
            GUILayout.Label("Prefab Reference Assigner", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Asigna automáticamente los prefabs a los managers.\n" +
                "Abre la escena correspondiente antes de asignar.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            // === BOTONES POR MANAGER ===
            GUILayout.Label("Asignar por Manager:", EditorStyles.boldLabel);

            if (GUILayout.Button("Leaderboard Manager", GUILayout.Height(25)))
                AssignLeaderboardPrefabs();

            if (GUILayout.Button("Onboarding Manager", GUILayout.Height(25)))
                AssignOnboardingPrefabs();

            if (GUILayout.Button("Achievements Manager", GUILayout.Height(25)))
                AssignAchievementsPrefabs();

            if (GUILayout.Button("BattlePass Manager", GUILayout.Height(25)))
                AssignBattlePassPrefabs();

            if (GUILayout.Button("Daily Rewards Manager", GUILayout.Height(25)))
                AssignDailyRewardsPrefabs();

            if (GUILayout.Button("Daily Missions Manager", GUILayout.Height(25)))
                AssignDailyMissionsPrefabs();

            if (GUILayout.Button("Chest Opening Manager", GUILayout.Height(25)))
                AssignChestOpeningPrefabs();

            if (GUILayout.Button("Search Players Manager", GUILayout.Height(25)))
                AssignSearchPlayersPrefabs();

            if (GUILayout.Button("Tournament Manager", GUILayout.Height(25)))
                AssignTournamentPrefabs();

            if (GUILayout.Button("Tournament Lobby Manager", GUILayout.Height(25)))
                AssignTournamentLobbyPrefabs();

            if (GUILayout.Button("Online Result Manager", GUILayout.Height(25)))
                AssignOnlineResultPrefabs();

            EditorGUILayout.Space(15);

            // === ASIGNAR TODO ===
            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
            if (GUILayout.Button("ASIGNAR TODOS EN ESCENA ACTUAL", GUILayout.Height(35)))
            {
                AssignAllInCurrentScene();
            }

            EditorGUILayout.Space(10);

            GUI.backgroundColor = new Color(1f, 0.6f, 0.2f);
            if (GUILayout.Button("⚡ ASIGNAR A TODAS LAS ESCENAS ⚡", GUILayout.Height(45)))
            {
                if (EditorUtility.DisplayDialog("Asignar a Todas las Escenas",
                    "Esto abrirá cada escena del proyecto, asignará los prefabs y guardará.\n\n" +
                    "¿Deseas continuar?",
                    "Sí, Asignar Todo", "Cancelar"))
                {
                    AssignToAllScenes();
                }
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);

            // === LOGS ===
            if (logs.Count > 0)
            {
                GUILayout.Label("Resultados:", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                foreach (var log in logs)
                {
                    EditorGUILayout.LabelField(log);
                }
                EditorGUILayout.EndVertical();

                if (GUILayout.Button("Limpiar Log"))
                {
                    logs.Clear();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private static GameObject LoadPrefab(string prefabKey)
        {
            if (!PrefabPaths.TryGetValue(prefabKey, out string relativePath))
            {
                Debug.LogWarning($"[PrefabAssigner] No se encontró la ruta para: {prefabKey}");
                return null;
            }

            string fullPath = $"{PREFAB_BASE}/{relativePath}";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);

            if (prefab == null)
            {
                Debug.LogWarning($"[PrefabAssigner] Prefab no encontrado: {fullPath}");
            }

            return prefab;
        }

        private void AssignLeaderboardPrefabs()
        {
            logs.Clear();
            var manager = Object.FindFirstObjectByType<Managers.LeaderboardManager>();
            if (manager == null)
            {
                logs.Add("No se encontró LeaderboardManager");
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            int count = TryAssignPrefab(so, "leaderboardEntryPrefab", "LeaderboardEntry");
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);

            logs.Add($"LeaderboardManager: {count} prefabs asignados");
            MarkSceneDirty();
        }

        private void AssignOnboardingPrefabs()
        {
            logs.Clear();
            var manager = Object.FindFirstObjectByType<Managers.OnboardingManager>();
            if (manager == null)
            {
                logs.Add("No se encontró OnboardingManager");
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            int count = 0;
            count += TryAssignPrefab(so, "dotPrefab", "StepDotItem");
            count += TryAssignPrefab(so, "avatarOptionPrefab", "AvatarOptionItem");
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);

            logs.Add($"OnboardingManager: {count} prefabs asignados");
            MarkSceneDirty();
        }

        private void AssignAchievementsPrefabs()
        {
            logs.Clear();
            var manager = FindManagerByName("AchievementsManager");
            if (manager == null)
            {
                logs.Add("No se encontró AchievementsManager");
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            int count = TryAssignPrefab(so, "trophyCardPrefab", "TrophyCard");
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);

            logs.Add($"AchievementsManager: {count} prefabs asignados");
            MarkSceneDirty();
        }

        private void AssignBattlePassPrefabs()
        {
            logs.Clear();
            var manager = FindManagerByName("BattlePassManager");
            if (manager == null)
            {
                logs.Add("No se encontró BattlePassManager");
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            int count = TryAssignPrefab(so, "rewardTierPrefab", "RewardTierItem");
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);

            logs.Add($"BattlePassManager: {count} prefabs asignados");
            MarkSceneDirty();
        }

        private void AssignDailyRewardsPrefabs()
        {
            logs.Clear();
            var manager = Object.FindFirstObjectByType<Managers.DailyRewardsManager>();
            if (manager == null)
            {
                logs.Add("No se encontró DailyRewardsManager");
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            int count = TryAssignPrefab(so, "rewardDayPrefab", "RewardDayItem");
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);

            logs.Add($"DailyRewardsManager: {count} prefabs asignados");
            MarkSceneDirty();
        }

        private void AssignDailyMissionsPrefabs()
        {
            logs.Clear();
            var manager = FindManagerByName("DailyMissionsManager");
            if (manager == null)
            {
                logs.Add("No se encontró DailyMissionsManager");
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            int count = TryAssignPrefab(so, "missionItemPrefab", "MissionItem");
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);

            logs.Add($"DailyMissionsManager: {count} prefabs asignados");
            MarkSceneDirty();
        }

        private void AssignChestOpeningPrefabs()
        {
            logs.Clear();
            var manager = FindManagerByName("ChestOpeningManager");
            if (manager == null)
            {
                logs.Add("No se encontró ChestOpeningManager");
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            int count = TryAssignPrefab(so, "rewardItemPrefab", "ChestRewardItem");
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);

            logs.Add($"ChestOpeningManager: {count} prefabs asignados");
            MarkSceneDirty();
        }

        private void AssignSearchPlayersPrefabs()
        {
            logs.Clear();
            var manager = Object.FindFirstObjectByType<Managers.SearchPlayersManager>();
            if (manager == null)
            {
                logs.Add("No se encontró SearchPlayersManager");
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            int count = TryAssignPrefab(so, "playerItemPrefab", "PlayerSearchItem");
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);

            logs.Add($"SearchPlayersManager: {count} prefabs asignados");
            MarkSceneDirty();
        }

        private void AssignTournamentPrefabs()
        {
            logs.Clear();
            var manager = Object.FindFirstObjectByType<Managers.TournamentManager>();
            if (manager == null)
            {
                logs.Add("No se encontró TournamentManager");
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            int count = 0;
            count += TryAssignPrefab(so, "tournamentItemPrefab", "TournamentItem");
            count += TryAssignPrefab(so, "tournamentSearchItemPrefab", "TournamentSearchItem");
            count += TryAssignPrefab(so, "tournamentMyItemPrefab", "TournamentMyItem");
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);

            logs.Add($"TournamentManager: {count} prefabs asignados");
            MarkSceneDirty();
        }

        private void AssignTournamentLobbyPrefabs()
        {
            logs.Clear();
            var manager = FindManagerByName("TournamentLobbyManager");
            if (manager == null)
            {
                logs.Add("No se encontró TournamentLobbyManager");
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            int count = 0;
            count += TryAssignPrefab(so, "prizeRowPrefab", "PrizeRowItem");
            count += TryAssignPrefab(so, "participantItemPrefab", "ParticipantItem");
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);

            logs.Add($"TournamentLobbyManager: {count} prefabs asignados");
            MarkSceneDirty();
        }

        private void AssignOnlineResultPrefabs()
        {
            logs.Clear();
            var manager = Object.FindFirstObjectByType<Managers.OnlineResultManager>();
            if (manager == null)
            {
                logs.Add("No se encontró OnlineResultManager");
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            int count = 0;
            count += TryAssignPrefab(so, "onlineWinPanelPrefab", "OnlineWinPanel");
            count += TryAssignPrefab(so, "onlineLosePanelPrefab", "OnlineLosePanel");
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);

            logs.Add($"OnlineResultManager: {count} prefabs asignados");
            MarkSceneDirty();
        }

        private void AssignAllInCurrentScene()
        {
            logs.Clear();
            logs.Add("=== Asignando prefabs en escena actual ===");

            AssignLeaderboardPrefabs();
            AssignOnboardingPrefabs();
            AssignAchievementsPrefabs();
            AssignBattlePassPrefabs();
            AssignDailyRewardsPrefabs();
            AssignDailyMissionsPrefabs();
            AssignChestOpeningPrefabs();
            AssignSearchPlayersPrefabs();
            AssignTournamentPrefabs();
            AssignTournamentLobbyPrefabs();
            AssignOnlineResultPrefabs();

            logs.Add("=== Completado ===");
        }

        private static int TryAssignPrefab(SerializedObject so, string propertyName, string prefabKey)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop == null)
            {
                Debug.Log($"[PrefabAssigner] Propiedad no encontrada: {propertyName}");
                return 0;
            }

            // Solo asignar si está vacío
            if (prop.objectReferenceValue != null)
            {
                Debug.Log($"[PrefabAssigner] {propertyName} ya tiene valor asignado");
                return 0;
            }

            GameObject prefab = LoadPrefab(prefabKey);
            if (prefab != null)
            {
                prop.objectReferenceValue = prefab;
                Debug.Log($"[PrefabAssigner] Asignado: {propertyName} = {prefabKey}");
                return 1;
            }

            return 0;
        }

        private static MonoBehaviour FindManagerByName(string typeName)
        {
            var allMonos = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var mono in allMonos)
            {
                if (mono.GetType().Name == typeName)
                    return mono;
            }
            return null;
        }

        private static void MarkSceneDirty()
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        // ==================== BATCH ASSIGNMENT TO ALL SCENES ====================

        [MenuItem("DigitPark/Tools/Assign Prefabs to ALL Scenes", false, 52)]
        public static void AssignToAllScenesMenu()
        {
            if (EditorUtility.DisplayDialog("Asignar a Todas las Escenas",
                "Esto abrirá cada escena del proyecto, asignará los prefabs y guardará.\n\n" +
                "¿Deseas continuar?",
                "Sí, Asignar Todo", "Cancelar"))
            {
                AssignToAllScenes();
            }
        }

        private static void AssignToAllScenes()
        {
            batchLogs.Clear();

            // Guardar escena actual si tiene cambios
            string currentScenePath = EditorSceneManager.GetActiveScene().path;
            if (EditorSceneManager.GetActiveScene().isDirty)
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            }

            // Buscar todas las escenas
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { SCENES_PATH });
            List<string> scenePaths = new List<string>();

            foreach (string guid in sceneGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                scenePaths.Add(path);
            }

            if (scenePaths.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", $"No se encontraron escenas en {SCENES_PATH}", "OK");
                return;
            }

            int totalAssigned = 0;
            int scenesProcessed = 0;

            try
            {
                for (int i = 0; i < scenePaths.Count; i++)
                {
                    string scenePath = scenePaths[i];
                    string sceneName = Path.GetFileNameWithoutExtension(scenePath);

                    // Mostrar progreso
                    bool cancel = EditorUtility.DisplayCancelableProgressBar(
                        "Asignando Prefabs",
                        $"Procesando: {sceneName} ({i + 1}/{scenePaths.Count})",
                        (float)i / scenePaths.Count);

                    if (cancel)
                    {
                        batchLogs.Add("=== CANCELADO POR USUARIO ===");
                        break;
                    }

                    // Abrir escena
                    EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                    // Asignar prefabs
                    int assigned = AssignAllPrefabsInScene();
                    totalAssigned += assigned;
                    scenesProcessed++;

                    if (assigned > 0)
                    {
                        batchLogs.Add($"✓ {sceneName}: {assigned} prefabs asignados");

                        // Guardar escena
                        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                    }
                    else
                    {
                        batchLogs.Add($"· {sceneName}: sin cambios");
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            // Volver a la escena original si existe
            if (!string.IsNullOrEmpty(currentScenePath) && File.Exists(currentScenePath))
            {
                EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
            }

            // Mostrar resultados
            string summary = $"=== RESUMEN ===\n" +
                           $"Escenas procesadas: {scenesProcessed}\n" +
                           $"Total prefabs asignados: {totalAssigned}\n\n" +
                           "Detalles:\n" +
                           string.Join("\n", batchLogs);

            Debug.Log($"[PrefabAssigner] {summary}");

            EditorUtility.DisplayDialog("Asignación Completada",
                $"Escenas procesadas: {scenesProcessed}\n" +
                $"Prefabs asignados: {totalAssigned}\n\n" +
                "Ver Console para detalles.",
                "OK");
        }

        /// <summary>
        /// Asigna todos los prefabs en la escena actual sin limpiar logs
        /// Retorna el número de prefabs asignados
        /// </summary>
        private static int AssignAllPrefabsInScene()
        {
            int total = 0;

            // Leaderboard
            var leaderboardMgr = Object.FindFirstObjectByType<Managers.LeaderboardManager>();
            if (leaderboardMgr != null)
            {
                SerializedObject so = new SerializedObject(leaderboardMgr);
                total += TryAssignPrefab(so, "leaderboardEntryPrefab", "LeaderboardEntry");
                so.ApplyModifiedProperties();
                if (total > 0) EditorUtility.SetDirty(leaderboardMgr);
            }

            // Onboarding
            var onboardingMgr = Object.FindFirstObjectByType<Managers.OnboardingManager>();
            if (onboardingMgr != null)
            {
                SerializedObject so = new SerializedObject(onboardingMgr);
                total += TryAssignPrefab(so, "dotPrefab", "StepDotItem");
                total += TryAssignPrefab(so, "avatarOptionPrefab", "AvatarOptionItem");
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(onboardingMgr);
            }

            // Achievements
            var achievementsMgr = FindManagerByName("AchievementsManager");
            if (achievementsMgr != null)
            {
                SerializedObject so = new SerializedObject(achievementsMgr);
                total += TryAssignPrefab(so, "trophyCardPrefab", "TrophyCard");
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(achievementsMgr);
            }

            // BattlePass
            var battlePassMgr = FindManagerByName("BattlePassManager");
            if (battlePassMgr != null)
            {
                SerializedObject so = new SerializedObject(battlePassMgr);
                total += TryAssignPrefab(so, "rewardTierPrefab", "RewardTierItem");
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(battlePassMgr);
            }

            // DailyRewards
            var dailyRewardsMgr = Object.FindFirstObjectByType<Managers.DailyRewardsManager>();
            if (dailyRewardsMgr != null)
            {
                SerializedObject so = new SerializedObject(dailyRewardsMgr);
                total += TryAssignPrefab(so, "rewardDayPrefab", "RewardDayItem");
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(dailyRewardsMgr);
            }

            // DailyMissions
            var dailyMissionsMgr = FindManagerByName("DailyMissionsManager");
            if (dailyMissionsMgr != null)
            {
                SerializedObject so = new SerializedObject(dailyMissionsMgr);
                total += TryAssignPrefab(so, "missionItemPrefab", "MissionItem");
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(dailyMissionsMgr);
            }

            // ChestOpening
            var chestMgr = FindManagerByName("ChestOpeningManager");
            if (chestMgr != null)
            {
                SerializedObject so = new SerializedObject(chestMgr);
                total += TryAssignPrefab(so, "rewardItemPrefab", "ChestRewardItem");
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(chestMgr);
            }

            // SearchPlayers
            var searchPlayersMgr = Object.FindFirstObjectByType<Managers.SearchPlayersManager>();
            if (searchPlayersMgr != null)
            {
                SerializedObject so = new SerializedObject(searchPlayersMgr);
                total += TryAssignPrefab(so, "playerItemPrefab", "PlayerSearchItem");
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(searchPlayersMgr);
            }

            // Tournament
            var tournamentMgr = Object.FindFirstObjectByType<Managers.TournamentManager>();
            if (tournamentMgr != null)
            {
                SerializedObject so = new SerializedObject(tournamentMgr);
                total += TryAssignPrefab(so, "tournamentItemPrefab", "TournamentItem");
                total += TryAssignPrefab(so, "tournamentSearchItemPrefab", "TournamentSearchItem");
                total += TryAssignPrefab(so, "tournamentMyItemPrefab", "TournamentMyItem");
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(tournamentMgr);
            }

            // TournamentLobby
            var tournamentLobbyMgr = FindManagerByName("TournamentLobbyManager");
            if (tournamentLobbyMgr != null)
            {
                SerializedObject so = new SerializedObject(tournamentLobbyMgr);
                total += TryAssignPrefab(so, "prizeRowPrefab", "PrizeRowItem");
                total += TryAssignPrefab(so, "participantItemPrefab", "ParticipantItem");
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(tournamentLobbyMgr);
            }

            // OnlineResult
            var onlineResultMgr = Object.FindFirstObjectByType<Managers.OnlineResultManager>();
            if (onlineResultMgr != null)
            {
                SerializedObject so = new SerializedObject(onlineResultMgr);
                total += TryAssignPrefab(so, "onlineWinPanelPrefab", "OnlineWinPanel");
                total += TryAssignPrefab(so, "onlineLosePanelPrefab", "OnlineLosePanel");
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(onlineResultMgr);
            }

            // TournamentsBrowser (separado de TournamentManager)
            var tournamentsBrowserMgr = FindManagerByName("TournamentsBrowserManager");
            if (tournamentsBrowserMgr != null)
            {
                SerializedObject so = new SerializedObject(tournamentsBrowserMgr);
                total += TryAssignPrefab(so, "tournamentItemPrefab", "TournamentItem");
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(tournamentsBrowserMgr);
            }

            if (total > 0)
            {
                MarkSceneDirty();
            }

            return total;
        }
    }
}
