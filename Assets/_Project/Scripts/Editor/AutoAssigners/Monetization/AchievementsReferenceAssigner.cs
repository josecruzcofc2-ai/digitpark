using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;
using System.Reflection;

namespace DigitPark.Editor.AutoAssigners
{
    /// <summary>
    /// Reference Assigner for Achievements scene.
    /// Automatically finds and assigns UI references to AchievementsManager.
    ///
    /// Menu: DigitPark/Auto Assigners/References/Monetization/Achievements References
    /// </summary>
    public class AchievementsReferenceAssigner : EditorWindow
    {
        private Vector2 scrollPosition;
        private static string log = "";
        private static int assignedCount = 0;
        private static int failedCount = 0;
        private static int alreadySetCount = 0;
        private static List<ReferenceResult> results = new List<ReferenceResult>();

        private static readonly string[] REQUIRED_REFS = {
            // Header
            "backButton", "titleText", "totalPointsText", "completionText", "overallProgressBar",
            // Tabs Container
            "tabsContainer", "tabsScrollRect",
            // Category Tabs (11)
            "allTab", "beginnerTab", "masteryTab", "victoriesTab", "streaksTab",
            "cashBattleTab", "tournamentsTab", "socialTab", "progressionTab", "collectorTab", "timeTab", "secretTab",
            // Showcase
            "showcaseContainer", "scrollRect",
            // Empty State
            "emptyStateContainer", "emptyStateText",
            // Detail Panel
            "detailPanel", "detailTitleText", "detailDescriptionText", "detailProgressBar",
            "detailProgressText", "claimRewardButton", "closeDetailButton",
            // Reward Celebration
            "rewardCelebration", "rewardAmountText"
        };

        private struct ReferenceResult
        {
            public string fieldName;
            public string status;
            public bool success;
            public Object assignedObject;
        }

        #region Menu Items

        [MenuItem("DigitPark/Auto Assigners/References/Monetization/Achievements References", false, 275)]
        public static void ShowWindow()
        {
            var window = GetWindow<AchievementsReferenceAssigner>("Achievements Reference Assigner");
            window.minSize = new Vector2(650, 600);
        }

        #endregion

        #region Window GUI

        private void OnGUI()
        {
            GUILayout.Label("Achievements Scene Reference Assigner", EditorStyles.boldLabel);
            GUILayout.Space(10);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "Achievements")
            {
                EditorGUILayout.HelpBox(
                    $"Current scene: {currentScene}\n" +
                    "Please open the Achievements scene first!",
                    MessageType.Warning);
            }

            EditorGUILayout.HelpBox(
                "Assigns UI references to AchievementsManager:\n" +
                "• Header (back, title, points, completion, progress)\n" +
                "• 11 Category Tabs (scrollable)\n" +
                "• Trophy showcase container\n" +
                "• Detail panel and reward celebration",
                MessageType.Info);

            GUILayout.Space(10);

            MonoBehaviour targetManager = FindAchievementsManager();
            if (targetManager != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Target:", GUILayout.Width(50));
                EditorGUILayout.ObjectField(targetManager, typeof(MonoBehaviour), true);
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(10);

            GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
            if (GUILayout.Button("Auto-Assign All References", GUILayout.Height(40)))
            {
                ResetLog();
                AssignAllReferences();
                Repaint();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);
            DrawResultsSummary();
        }

        private void DrawResultsSummary()
        {
            if (results.Count == 0) return;

            int total = results.Count;
            int successTotal = assignedCount + alreadySetCount;

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(400));
            EditorGUILayout.BeginVertical("box");

            float successRate = (float)successTotal / total;
            GUI.color = successRate == 1f ? new Color(0.2f, 0.8f, 0.2f) :
                        successRate >= 0.7f ? new Color(1f, 0.8f, 0.2f) : new Color(1f, 0.4f, 0.4f);
            GUILayout.Label(successRate == 1f ? "✓ ALL REFERENCES SET" : "⚠ Some references missing", EditorStyles.boldLabel);
            GUI.color = Color.white;

            GUILayout.Label($"Assigned: {assignedCount} | Already Set: {alreadySetCount} | Failed: {failedCount}");

            foreach (var result in results)
            {
                EditorGUILayout.BeginHorizontal();
                GUI.color = result.success ? (result.status == "Already Set" ? new Color(0.5f, 0.8f, 1f) : Color.green) : Color.red;
                GUILayout.Label(result.success ? (result.status == "Already Set" ? "●" : "✓") : "✗", GUILayout.Width(20));
                GUI.color = Color.white;
                GUILayout.Label(result.fieldName, GUILayout.Width(180));
                GUILayout.Label(result.status, GUILayout.Width(120));
                if (result.assignedObject != null)
                    EditorGUILayout.ObjectField(result.assignedObject, typeof(Object), true, GUILayout.Width(150));
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        #endregion

        #region Assignment Logic

        private static void AssignAllReferences()
        {
            Log("=== ASSIGNING ACHIEVEMENTS REFERENCES ===");

            var manager = FindAchievementsManager();
            if (manager == null)
            {
                Log("ERROR: AchievementsManager not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            SerializedObject so = new SerializedObject(manager);

            // Header
            AssignReference(so, "backButton", FindButtonByName("back", "return"));
            AssignReference(so, "titleText", FindTextByName("title", "header", "logros"));
            AssignReference(so, "totalPointsText", FindTextByName("totalpoints", "points", "puntos"));
            AssignReference(so, "completionText", FindTextByName("completion", "completados", "progress"));
            AssignReference(so, "overallProgressBar", FindByNameContains<Slider>("overall", "progress", "total"));

            // Tabs Container
            AssignReference(so, "tabsContainer", FindByNameContains<Transform>("tabscontainer", "tabs"));
            AssignReference(so, "tabsScrollRect", FindByNameContains<ScrollRect>("tabsscroll", "tabs"));

            // Category Tabs (11)
            AssignReference(so, "allTab", FindButtonByName("alltab", "all", "todos"));
            AssignReference(so, "beginnerTab", FindButtonByName("beginnertab", "beginner", "principiante"));
            AssignReference(so, "masteryTab", FindButtonByName("masterytab", "mastery", "maestria"));
            AssignReference(so, "victoriesTab", FindButtonByName("victoriestab", "victories", "victorias"));
            AssignReference(so, "streaksTab", FindButtonByName("streakstab", "streaks", "rachas"));
            AssignReference(so, "cashBattleTab", FindButtonByName("cashbattletab", "cashbattle", "cash"));
            AssignReference(so, "tournamentsTab", FindButtonByName("tournamentstab", "tournaments", "torneos"));
            AssignReference(so, "socialTab", FindButtonByName("socialtab", "social", "amigos"));
            AssignReference(so, "progressionTab", FindButtonByName("progressiontab", "progression", "progreso"));
            AssignReference(so, "collectorTab", FindButtonByName("collectortab", "collector", "coleccion"));
            AssignReference(so, "timeTab", FindButtonByName("timetab", "time", "tiempo"));
            AssignReference(so, "secretTab", FindButtonByName("secrettab", "secret", "secretos"));

            // Showcase
            AssignReference(so, "showcaseContainer", FindByNameContains<Transform>("showcase", "trophies", "container"));
            AssignReference(so, "scrollRect", FindByNameContains<ScrollRect>("scroll", "showcase"));

            // Empty State
            AssignReference(so, "emptyStateContainer", FindByNameContains<Transform>("emptystate", "empty", "nodata"));
            AssignReference(so, "emptyStateText", FindTextByName("emptystate", "empty", "no"));

            // Detail Panel
            AssignReference(so, "detailPanel", FindByNameContains<Transform>("detailpanel", "detail", "detalle"));
            AssignReference(so, "detailTitleText", FindTextByName("detailtitle", "achievementtitle"));
            AssignReference(so, "detailDescriptionText", FindTextByName("detaildescription", "description", "descripcion"));
            AssignReference(so, "detailProgressBar", FindByNameContains<Slider>("detailprogress", "achievementprogress"));
            AssignReference(so, "detailProgressText", FindTextByName("detailprogresstext", "progressvalue"));
            AssignReference(so, "claimRewardButton", FindButtonByName("claimreward", "claim", "reclamar"));
            AssignReference(so, "closeDetailButton", FindButtonByName("closedetail", "close", "cerrar"));

            // Reward Celebration
            AssignReference(so, "rewardCelebration", FindByNameContains<Transform>("celebration", "reward", "celebracion"));
            AssignReference(so, "rewardAmountText", FindTextByName("rewardamount", "amount", "cantidad"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            Log("=== ASSIGNMENT COMPLETE ===");
        }

        private static MonoBehaviour FindAchievementsManager()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "AchievementsManager") return mb;
            return null;
        }

        private static void AssignReference(SerializedObject so, string propertyName, Object value)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { AddResult(propertyName, "Property not found", false, null); failedCount++; return; }
            if (prop.objectReferenceValue != null) { AddResult(propertyName, "Already Set", true, prop.objectReferenceValue); alreadySetCount++; return; }
            if (value != null) { prop.objectReferenceValue = value; AddResult(propertyName, "Assigned", true, value); assignedCount++; }
            else { AddResult(propertyName, "Not found", false, null); failedCount++; }
        }

        #endregion

        #region Finders

        private static T FindByNameContains<T>(params string[] patterns) where T : Component
        {
            var all = Object.FindObjectsOfType<T>(true);
            foreach (var p in patterns) foreach (var o in all) if (o.gameObject.name.ToLower().Contains(p.ToLower())) return o;
            return null;
        }

        private static TextMeshProUGUI FindTextByName(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<TextMeshProUGUI>(true);
            foreach (var p in patterns) foreach (var t in all) if (t.gameObject.name.ToLower().Contains(p.ToLower())) return t;
            return null;
        }

        private static Button FindButtonByName(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<Button>(true);
            foreach (var p in patterns) foreach (var b in all) if (b.gameObject.name.ToLower().Contains(p.ToLower())) return b;
            return null;
        }

        #endregion

        #region Helpers

        private static void ResetLog() { log = ""; assignedCount = 0; failedCount = 0; alreadySetCount = 0; results.Clear(); }
        private static void Log(string msg) { log += msg + "\n"; Debug.Log($"[AchievementsReferenceAssigner] {msg}"); }
        private static void AddResult(string f, string s, bool ok, Object o) { results.Add(new ReferenceResult { fieldName = f, status = s, success = ok, assignedObject = o }); }

        #endregion
    }
}
