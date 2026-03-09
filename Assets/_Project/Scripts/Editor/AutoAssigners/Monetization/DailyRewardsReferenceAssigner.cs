using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Reflection;

namespace DigitPark.Editor.AutoAssigners
{
    /// <summary>
    /// Reference Assigner for DailyRewards scene.
    /// Automatically finds and assigns UI references to DailyRewardsManager.
    ///
    /// Menu: DigitPark/Auto Assigners/References/Monetization/DailyRewards References
    /// </summary>
    public class DailyRewardsReferenceAssigner : EditorWindow
    {
        private Vector2 scrollPosition;
        private static string log = "";
        private static int assignedCount = 0;
        private static int failedCount = 0;
        private static int alreadySetCount = 0;
        private static List<ReferenceResult> results = new List<ReferenceResult>();

        private static readonly string[] REQUIRED_REFS = {
            // Header
            "backButton", "titleText", "streakText", "nextResetText",
            // Current Day
            "currentDayHighlight", "currentDayText", "currentDayRewardIcon", "currentDayRewardText",
            // Rewards Grid
            "rewardsContainer",
            // Claim Button
            "claimButton", "claimButtonText", "claimGlow",
            // Bonus Info
            "bonusInfoText", "streakProgressBar", "streakBonusText",
            // Claim Animation (Clash Royale style)
            "claimAnimationPanel", "darkOverlayImage", "giftBoxImage", "giftGlowImage", "lightBurstImage",
            "celebTitleText", "claimRewardIcon", "claimRewardText", "streakInfoText",
            "claimParticles", "continueButton",
            // Milestone
            "milestonePanel", "milestoneText", "milestoneBonusText",
            // Gift Box Icons (assigned via UIBuilder sprite refs, not scene objects)
            "giftDayIcons", "giftOpenBasicIcon", "giftOpenPremiumIcon", "giftOpenEpicIcon"
        };

        private struct ReferenceResult
        {
            public string fieldName;
            public string status;
            public bool success;
            public Object assignedObject;
        }

        #region Menu Items

        [MenuItem("DigitPark/Scenes/Assign References/Monetization/DailyRewards", false, 140)]
        public static void ShowWindow()
        {
            var window = GetWindow<DailyRewardsReferenceAssigner>("DailyRewards Reference Assigner");
            window.minSize = new Vector2(600, 500);
        }

        #endregion

        #region Window GUI

        private void OnGUI()
        {
            GUILayout.Label("DailyRewards Scene Reference Assigner", EditorStyles.boldLabel);
            GUILayout.Space(10);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "DailyRewards")
            {
                EditorGUILayout.HelpBox(
                    $"Current scene: {currentScene}\n" +
                    "Please open the DailyRewards scene first!",
                    MessageType.Warning);
            }

            EditorGUILayout.HelpBox(
                "Assigns UI references to DailyRewardsManager:\n" +
                "• Header (back, title, streak, reset timer)\n" +
                "• Rewards grid container\n" +
                "• Claim button and animation\n" +
                "• Progress bar and milestone panel",
                MessageType.Info);

            GUILayout.Space(10);

            MonoBehaviour targetManager = FindDailyRewardsManager();
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
        }

        #endregion

        #region Assignment Logic

        private static void AssignAllReferences()
        {
            Log("=== ASSIGNING DAILYREWARDS REFERENCES ===");

            var manager = FindDailyRewardsManager();
            if (manager == null)
            {
                Log("ERROR: DailyRewardsManager not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            so.Update();

            // Header
            AssignReference(so, "backButton", FindButtonByName("back", "return"));
            AssignReference(so, "titleText", FindTextByName("title", "header", "titulo"));
            AssignReference(so, "streakText", FindTextByName("streak", "racha"));
            AssignReference(so, "nextResetText", FindTextByName("reset", "timer", "next"));

            // Current Day
            AssignGameObject(so, "currentDayHighlight", "currentday", "highlight", "today");
            AssignReference(so, "currentDayText", FindTextByName("currentdaytext", "currentday", "todayday"));
            AssignReference(so, "currentDayRewardIcon", FindImageByName("currentrewardicon", "currentdayreward", "todayreward"));
            AssignReference(so, "currentDayRewardText", FindTextByName("currentdayrewardtext", "currentrewardtext", "todayrewardtext"));

            // Rewards Grid
            AssignReference(so, "rewardsContainer", FindByNameContains<Transform>("rewards", "grid", "container"));

            // Claim Button
            AssignReference(so, "claimButton", FindButtonByName("claim", "reclamar", "collect"));
            AssignReference(so, "claimButtonText", FindTextByName("claimbutton", "reclamar"));
            AssignGameObject(so, "claimGlow", "claimglow", "glow", "claimeffect");

            // Bonus Info
            AssignReference(so, "bonusInfoText", FindTextByName("bonusinfo", "bonustext"));
            AssignReference(so, "streakProgressBar", FindByNameContains<Slider>("streak", "progress"));
            AssignReference(so, "streakBonusText", FindTextByName("bonus", "streakbonus"));

            // Claim Animation
            AssignReference(so, "claimAnimationPanel", FindByNameContains<Transform>("claimanimation", "animation", "reward"));
            AssignReference(so, "claimRewardText", FindTextByName("claimrewardtext", "claimreward", "animationreward"));
            AssignReference(so, "claimRewardIcon", FindImageByName("claimrewardicon", "claimreward", "animationicon"));
            AssignReference(so, "claimParticles", FindByNameContains<ParticleSystem>("particles", "claimparticles", "confetti"));
            AssignReference(so, "continueButton", FindButtonByName("continue", "continuar", "ok"));

            // Milestone
            AssignReference(so, "milestonePanel", FindByNameContains<Transform>("milestone", "hito"));
            AssignReference(so, "milestoneText", FindTextByName("milestonetext", "milestone", "milestonetitle"));
            AssignReference(so, "milestoneBonusText", FindTextByName("milestonebonus", "bonusmilestone"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(manager.gameObject);
            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
            Log("=== ASSIGNMENT COMPLETE ===");
        }

        private static MonoBehaviour FindDailyRewardsManager()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "DailyRewardsManager") return mb;
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

        private static Image FindImageByName(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<Image>(true);
            foreach (var p in patterns) foreach (var i in all) if (i.gameObject.name.ToLower().Contains(p.ToLower())) return i;
            return null;
        }

        private static void AssignGameObject(SerializedObject so, string propertyName, params string[] patterns)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { AddResult(propertyName, "Property not found", false, null); failedCount++; return; }
            if (prop.objectReferenceValue != null) { AddResult(propertyName, "Already Set", true, prop.objectReferenceValue); alreadySetCount++; return; }
            var all = Object.FindObjectsOfType<Transform>(true);
            foreach (var p in patterns)
                foreach (var o in all)
                    if (o.gameObject.name.ToLower().Contains(p.ToLower()))
                    {
                        prop.objectReferenceValue = o.gameObject;
                        AddResult(propertyName, "Assigned", true, o.gameObject);
                        assignedCount++;
                        return;
                    }
            AddResult(propertyName, "Not found", false, null); failedCount++;
        }

        #endregion

        #region Helpers

        private static void ResetLog() { log = ""; assignedCount = 0; failedCount = 0; alreadySetCount = 0; results.Clear(); }
        private static void Log(string msg) { log += msg + "\n"; Debug.Log($"[DailyRewardsReferenceAssigner] {msg}"); }
        private static void AddResult(string f, string s, bool ok, Object o) { results.Add(new ReferenceResult { fieldName = f, status = s, success = ok, assignedObject = o }); }

        #endregion
    }
}
