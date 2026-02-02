using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Linq;

namespace DigitPark.Editor.AutoAssigners
{
    /// <summary>
    /// Auto-assigns all references for AchievementsManager in the scene.
    /// Menu: DigitPark/Auto Assigners/Achievements Manager
    /// </summary>
    public class AchievementsReferenceAssigner : EditorWindow
    {
        private Vector2 scrollPosition;
        private static int assignedCount = 0;
        private static int failedCount = 0;
        private static string log = "";

        [MenuItem("DigitPark/Auto Assigners/References/Achievements Manager (Window)", false, 100)]
        public static void ShowWindow()
        {
            var window = GetWindow<AchievementsReferenceAssigner>("Achievements Assigner");
            window.minSize = new Vector2(400, 500);
        }

        [MenuItem("DigitPark/Auto Assigners/References/Achievements Manager (Quick)", false, 101)]
        public static void QuickAssign()
        {
            AssignAllReferences();
            EditorUtility.DisplayDialog("Achievements Manager",
                $"Referencias asignadas: {assignedCount}\nFallidas: {failedCount}", "OK");
        }

        private void OnGUI()
        {
            GUILayout.Label("Achievements Manager - Reference Assigner", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Este script auto-asigna todas las referencias del AchievementsManager " +
                "basándose en los nombres de los GameObjects en la escena.\n\n" +
                "Asegúrate de tener la escena Achievements abierta.",
                MessageType.Info);

            GUILayout.Space(10);

            if (GUILayout.Button("Asignar TODAS las Referencias", GUILayout.Height(40)))
            {
                AssignAllReferences();
            }

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Resultado:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Asignadas: {assignedCount}  |  Fallidas: {failedCount}");

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Log:", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
            EditorGUILayout.TextArea(log, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            GUILayout.Space(10);

            if (GUILayout.Button("Limpiar Log"))
            {
                log = "";
                assignedCount = 0;
                failedCount = 0;
            }
        }

        public static void AssignAllReferences()
        {
            log = "";
            assignedCount = 0;
            failedCount = 0;

            // Find AchievementsManager
            var managers = Object.FindObjectsOfType<MonoBehaviour>(true);
            MonoBehaviour manager = null;

            foreach (var m in managers)
            {
                if (m.GetType().Name == "AchievementsManager")
                {
                    manager = m;
                    break;
                }
            }

            if (manager == null)
            {
                log = "ERROR: No se encontró AchievementsManager en la escena.\n";
                log += "Asegúrate de tener la escena Achievements abierta.";
                failedCount = 1;
                return;
            }

            Log($"AchievementsManager encontrado en: {manager.gameObject.name}");
            Log("-------------------------------------------");

            SerializedObject so = new SerializedObject(manager);

            // ==================== HEADER ====================
            Log("\n[HEADER]");
            TryAssignButton(so, "backButton", "BackButton", "BackButtonGold", "BtnBack");
            TryAssignTMP(so, "titleText", "TitleText", "Title", "HeaderTitle");
            TryAssignTMP(so, "totalPointsText", "TotalPointsText", "PointsText", "TotalPoints");
            TryAssignTMP(so, "completionText", "CompletionText", "ProgressText", "CompletionProgress");
            TryAssignSlider(so, "overallProgressBar", "OverallProgressBar", "ProgressBar", "MainProgressBar", "progressBar");

            // ==================== CATEGORY TABS ====================
            Log("\n[CATEGORY TABS]");
            TryAssignTransform(so, "tabsContainer", "TabsContainer", "CategoryTabs", "TabsContent");
            TryAssignScrollRect(so, "tabsScrollRect", "TabsScrollRect", "CategoryTabsScroll", "TabsScroll");

            // Individual tabs
            TryAssignButton(so, "allTab", "AllTab", "TabAll", "BtnAll");
            TryAssignButton(so, "beginnerTab", "BeginnerTab", "TabBeginner", "BtnBeginner");
            TryAssignButton(so, "masteryTab", "MasteryTab", "TabMastery", "BtnMastery");
            TryAssignButton(so, "victoriesTab", "VictoriesTab", "TabVictories", "BtnVictories");
            TryAssignButton(so, "streaksTab", "StreaksTab", "TabStreaks", "BtnStreaks");
            TryAssignButton(so, "cashBattleTab", "CashBattleTab", "TabCashBattle", "BtnCashBattle");
            TryAssignButton(so, "tournamentsTab", "TournamentsTab", "TabTournaments", "BtnTournaments");
            TryAssignButton(so, "socialTab", "SocialTab", "TabSocial", "BtnSocial");
            TryAssignButton(so, "progressionTab", "ProgressionTab", "TabProgression", "BtnProgression");
            // collectorTab removido para V1 (sin BattlePass/Cofres)
            TryAssignButton(so, "timeTab", "TimeTab", "TabTime", "BtnTime");
            TryAssignButton(so, "secretTab", "SecretTab", "TabSecret", "BtnSecret");

            // ==================== TROPHY SHOWCASE ====================
            Log("\n[TROPHY SHOWCASE]");
            TryAssignTransform(so, "showcaseContainer", "ShowcaseContainer", "TrophyGrid", "Content", "GridContent");
            TryAssignGameObject(so, "trophyCardPrefab", "TrophyCard");
            TryAssignScrollRect(so, "scrollRect", "AchievementsScrollView", "TrophyScrollView", "MainScrollView");
            TryAssignGridLayout(so, "gridLayout", "TrophyGrid", "GridContent", "Content");

            // ==================== EMPTY STATE ====================
            Log("\n[EMPTY STATE]");
            TryAssignGameObject(so, "emptyStateContainer", "EmptyStateContainer", "EmptyState", "NoAchievements");
            TryAssignTMP(so, "emptyStateText", "EmptyStateText", "EmptyText", "NoResultsText");
            TryAssignImage(so, "emptyStateIcon", "EmptyStateIcon", "EmptyIcon", "Icon");

            // ==================== DETAIL PANEL ====================
            Log("\n[DETAIL PANEL]");
            TryAssignGameObject(so, "detailPanel", "DetailPanel", "AchievementDetailPanel", "TrophyDetail");
            TryAssignCanvasGroup(so, "detailPanelCanvasGroup", "DetailPanel", "DetailPanelBlocker");
            TryAssignImage(so, "detailBlocker", "DetailPanelBlocker", "DetailBlocker", "Blocker", "ModalBlocker");
            TryAssignRectTransform(so, "detailCard", "DetailCard", "DetailContent", "CardContainer");

            // Detail content
            Log("\n[DETAIL CONTENT]");
            TryAssignImage(so, "detailTrophyIcon", "DetailTrophyIcon", "TrophyIcon", "DetailIcon");
            TryAssignTMP(so, "detailTitleText", "DetailTitleText", "DetailTitle", "AchievementTitle");
            TryAssignTMP(so, "detailDescriptionText", "DetailDescriptionText", "DetailDescription", "Description");
            TryAssignTMP(so, "detailCategoryText", "DetailCategoryText", "CategoryText", "CategoryLabel");
            TryAssignSlider(so, "detailProgressBar", "DetailProgressBar", "DetailProgress", "progressBar");
            TryAssignTMP(so, "detailProgressText", "DetailProgressText", "ProgressLabel");
            TryAssignTMP(so, "detailPointsText", "DetailPointsText", "PointsLabel", "RewardPoints");
            TryAssignButton(so, "claimRewardButton", "ClaimRewardButton", "ClaimButton", "BtnClaim");
            TryAssignTMP(so, "claimButtonText", "ClaimButtonText", "ClaimText");
            TryAssignButton(so, "closeDetailButton", "CloseDetailButton", "CloseButton", "BtnClose");
            TryAssignParticleSystem(so, "detailParticles", "DetailParticles", "TrophyParticles");

            // ==================== REWARD CELEBRATION ====================
            Log("\n[REWARD CELEBRATION]");
            TryAssignGameObject(so, "rewardCelebration", "RewardCelebration", "CelebrationPanel", "RewardPopup");
            TryAssignTMP(so, "rewardAmountText", "RewardAmountText", "RewardAmount", "PointsEarned");
            TryAssignParticleSystem(so, "celebrationParticles", "CelebrationParticles", "ConfettiParticles");
            TryAssignImage(so, "celebrationGlow", "CelebrationGlow", "GlowEffect");

            // Apply changes
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Log("\n-------------------------------------------");
            Log($"COMPLETADO: {assignedCount} asignadas, {failedCount} no encontradas");
        }

        #region Helper Methods

        private static void Log(string message)
        {
            log += message + "\n";
        }

        private static bool TryAssignButton(SerializedObject so, string propertyName, params string[] searchNames)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { failedCount++; Log($"  x {propertyName}: propiedad no encontrada"); return false; }
            if (prop.objectReferenceValue != null) { Log($"  - {propertyName}: ya asignado"); return true; }

            foreach (var name in searchNames)
            {
                var obj = FindInScene<Button>(name);
                if (obj != null)
                {
                    prop.objectReferenceValue = obj;
                    assignedCount++;
                    Log($"  + {propertyName} <- {obj.name}");
                    return true;
                }
            }
            failedCount++;
            Log($"  x {propertyName}: no encontrado ({string.Join(", ", searchNames)})");
            return false;
        }

        private static bool TryAssignTMP(SerializedObject so, string propertyName, params string[] searchNames)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { failedCount++; Log($"  x {propertyName}: propiedad no encontrada"); return false; }
            if (prop.objectReferenceValue != null) { Log($"  - {propertyName}: ya asignado"); return true; }

            foreach (var name in searchNames)
            {
                var obj = FindInScene<TextMeshProUGUI>(name);
                if (obj != null)
                {
                    prop.objectReferenceValue = obj;
                    assignedCount++;
                    Log($"  + {propertyName} <- {obj.name}");
                    return true;
                }
            }
            failedCount++;
            Log($"  x {propertyName}: no encontrado");
            return false;
        }

        private static bool TryAssignImage(SerializedObject so, string propertyName, params string[] searchNames)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { failedCount++; return false; }
            if (prop.objectReferenceValue != null) { Log($"  - {propertyName}: ya asignado"); return true; }

            foreach (var name in searchNames)
            {
                var obj = FindInScene<Image>(name);
                if (obj != null)
                {
                    prop.objectReferenceValue = obj;
                    assignedCount++;
                    Log($"  + {propertyName} <- {obj.name}");
                    return true;
                }
            }
            failedCount++;
            Log($"  x {propertyName}: no encontrado");
            return false;
        }

        private static bool TryAssignSlider(SerializedObject so, string propertyName, params string[] searchNames)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { failedCount++; return false; }
            if (prop.objectReferenceValue != null) { Log($"  - {propertyName}: ya asignado"); return true; }

            foreach (var name in searchNames)
            {
                var obj = FindInScene<Slider>(name);
                if (obj != null)
                {
                    prop.objectReferenceValue = obj;
                    assignedCount++;
                    Log($"  + {propertyName} <- {obj.name}");
                    return true;
                }
            }
            failedCount++;
            Log($"  x {propertyName}: no encontrado");
            return false;
        }

        private static bool TryAssignScrollRect(SerializedObject so, string propertyName, params string[] searchNames)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { failedCount++; return false; }
            if (prop.objectReferenceValue != null) { Log($"  - {propertyName}: ya asignado"); return true; }

            foreach (var name in searchNames)
            {
                var obj = FindInScene<ScrollRect>(name);
                if (obj != null)
                {
                    prop.objectReferenceValue = obj;
                    assignedCount++;
                    Log($"  + {propertyName} <- {obj.name}");
                    return true;
                }
            }
            failedCount++;
            Log($"  x {propertyName}: no encontrado");
            return false;
        }

        private static bool TryAssignGridLayout(SerializedObject so, string propertyName, params string[] searchNames)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { failedCount++; return false; }
            if (prop.objectReferenceValue != null) { Log($"  - {propertyName}: ya asignado"); return true; }

            foreach (var name in searchNames)
            {
                var obj = FindInScene<GridLayoutGroup>(name);
                if (obj != null)
                {
                    prop.objectReferenceValue = obj;
                    assignedCount++;
                    Log($"  + {propertyName} <- {obj.name}");
                    return true;
                }
            }
            failedCount++;
            Log($"  x {propertyName}: no encontrado");
            return false;
        }

        private static bool TryAssignTransform(SerializedObject so, string propertyName, params string[] searchNames)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { failedCount++; return false; }
            if (prop.objectReferenceValue != null) { Log($"  - {propertyName}: ya asignado"); return true; }

            foreach (var name in searchNames)
            {
                var obj = FindGameObjectInScene(name);
                if (obj != null)
                {
                    prop.objectReferenceValue = obj.transform;
                    assignedCount++;
                    Log($"  + {propertyName} <- {obj.name}");
                    return true;
                }
            }
            failedCount++;
            Log($"  x {propertyName}: no encontrado");
            return false;
        }

        private static bool TryAssignRectTransform(SerializedObject so, string propertyName, params string[] searchNames)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { failedCount++; return false; }
            if (prop.objectReferenceValue != null) { Log($"  - {propertyName}: ya asignado"); return true; }

            foreach (var name in searchNames)
            {
                var obj = FindGameObjectInScene(name);
                if (obj != null && obj.GetComponent<RectTransform>() != null)
                {
                    prop.objectReferenceValue = obj.GetComponent<RectTransform>();
                    assignedCount++;
                    Log($"  + {propertyName} <- {obj.name}");
                    return true;
                }
            }
            failedCount++;
            Log($"  x {propertyName}: no encontrado");
            return false;
        }

        private static bool TryAssignGameObject(SerializedObject so, string propertyName, params string[] searchNames)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { failedCount++; return false; }
            if (prop.objectReferenceValue != null) { Log($"  - {propertyName}: ya asignado"); return true; }

            // For prefabs, check Resources first
            if (propertyName.Contains("Prefab") || propertyName.Contains("prefab"))
            {
                foreach (var name in searchNames)
                {
                    var prefab = Resources.Load<GameObject>($"Prefabs/Monetization/{name}");
                    if (prefab == null)
                        prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_Project/Prefabs/Monetization/{name}.prefab");

                    if (prefab != null)
                    {
                        prop.objectReferenceValue = prefab;
                        assignedCount++;
                        Log($"  + {propertyName} <- {prefab.name} (prefab)");
                        return true;
                    }
                }
            }

            foreach (var name in searchNames)
            {
                var obj = FindGameObjectInScene(name);
                if (obj != null)
                {
                    prop.objectReferenceValue = obj;
                    assignedCount++;
                    Log($"  + {propertyName} <- {obj.name}");
                    return true;
                }
            }
            failedCount++;
            Log($"  x {propertyName}: no encontrado");
            return false;
        }

        private static bool TryAssignCanvasGroup(SerializedObject so, string propertyName, params string[] searchNames)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { failedCount++; return false; }
            if (prop.objectReferenceValue != null) { Log($"  - {propertyName}: ya asignado"); return true; }

            foreach (var name in searchNames)
            {
                var obj = FindInScene<CanvasGroup>(name);
                if (obj != null)
                {
                    prop.objectReferenceValue = obj;
                    assignedCount++;
                    Log($"  + {propertyName} <- {obj.name}");
                    return true;
                }
            }
            failedCount++;
            Log($"  x {propertyName}: no encontrado");
            return false;
        }

        private static bool TryAssignParticleSystem(SerializedObject so, string propertyName, params string[] searchNames)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { failedCount++; return false; }
            if (prop.objectReferenceValue != null) { Log($"  - {propertyName}: ya asignado"); return true; }

            foreach (var name in searchNames)
            {
                var obj = FindInScene<ParticleSystem>(name);
                if (obj != null)
                {
                    prop.objectReferenceValue = obj;
                    assignedCount++;
                    Log($"  + {propertyName} <- {obj.name}");
                    return true;
                }
            }
            // Particle systems are optional, don't count as failure
            Log($"  ? {propertyName}: no encontrado (opcional)");
            return false;
        }

        private static T FindInScene<T>(string name) where T : Component
        {
            var allObjects = Object.FindObjectsOfType<T>(true);

            // Exact match first
            foreach (var obj in allObjects)
            {
                if (obj.name == name)
                    return obj;
            }

            // Contains match
            foreach (var obj in allObjects)
            {
                if (obj.name.Contains(name) || name.Contains(obj.name))
                    return obj;
            }

            return null;
        }

        private static GameObject FindGameObjectInScene(string name)
        {
            var allObjects = Object.FindObjectsOfType<Transform>(true);

            // Exact match first
            foreach (var obj in allObjects)
            {
                if (obj.name == name)
                    return obj.gameObject;
            }

            // Contains match
            foreach (var obj in allObjects)
            {
                if (obj.name.Contains(name))
                    return obj.gameObject;
            }

            return null;
        }

        #endregion
    }
}
