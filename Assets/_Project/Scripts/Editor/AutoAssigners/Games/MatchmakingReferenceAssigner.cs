using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;

namespace DigitPark.Editor.AutoAssigners
{
    /// <summary>
    /// Reference Assigner for Matchmaking scene.
    /// Automatically finds and assigns UI references to MatchmakingManager.
    ///
    /// Menu: DigitPark/Auto Assigners/References/Games/Matchmaking References
    /// </summary>
    public class MatchmakingReferenceAssigner : EditorWindow
    {
        private Vector2 scrollPosition;
        private static string log = "";
        private static int assignedCount = 0;
        private static int failedCount = 0;
        private static int alreadySetCount = 0;
        private static List<ReferenceResult> results = new List<ReferenceResult>();

        private static readonly string[] REQUIRED_REFS = {
            // Header
            "titleText", "gameIconImage", "gameTypeText",
            // Player Card
            "playerAvatar", "playerNameText", "playerLevelText", "playerCard",
            // Opponent Card
            "opponentAvatar", "opponentNameText", "opponentLevelText",
            "opponentCard", "opponentSearchingIndicator", "opponentSearchRing",
            // VS Section
            "vsContainer", "vsText",
            // Search Status
            "statusText", "timerText", "searchingSpinner", "searchingRing",
            // Countdown
            "countdownPanel", "countdownText", "getReadyText",
            // Buttons
            "cancelButton", "backButton",
            // Effects
            "screenFlash"
        };

        private struct ReferenceResult
        {
            public string fieldName;
            public string status;
            public bool success;
            public Object assignedObject;
        }

        #region Menu Items

        [MenuItem("DigitPark/Auto Assigners/References/Games/Matchmaking References", false, 235)]
        public static void ShowWindow()
        {
            var window = GetWindow<MatchmakingReferenceAssigner>("Matchmaking Reference Assigner");
            window.minSize = new Vector2(600, 550);
        }

        #endregion

        #region Window GUI

        private void OnGUI()
        {
            GUILayout.Label("Matchmaking Scene Reference Assigner", EditorStyles.boldLabel);
            GUILayout.Space(10);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "Matchmaking")
            {
                EditorGUILayout.HelpBox(
                    $"Current scene: {currentScene}\n" +
                    "Please open the Matchmaking scene first!",
                    MessageType.Warning);
            }

            EditorGUILayout.HelpBox(
                "Assigns UI references to MatchmakingManager:\n" +
                "• Header (title, game icon, game type)\n" +
                "• Player card (avatar, name, level)\n" +
                "• Opponent card (avatar, name, level, search ring)\n" +
                "• VS section and search status\n" +
                "• Countdown panel and effects\n" +
                "• Buttons (cancel, back)",
                MessageType.Info);

            GUILayout.Space(10);

            MonoBehaviour targetManager = FindMatchmakingManager();
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

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(350));
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
                GUILayout.Label(result.fieldName, GUILayout.Width(220));
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
            Log("=== ASSIGNING MATCHMAKING REFERENCES ===");

            var manager = FindMatchmakingManager();
            if (manager == null)
            {
                Log("ERROR: MatchmakingManager not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            so.Update();

            // Header
            AssignReference(so, "titleText", FindTextByName("titletext", "title", "searching"));
            AssignReference(so, "gameIconImage", FindImageByName("gameicon", "icon"));
            AssignReference(so, "gameTypeText", FindTextByName("gamenametext", "gametype", "gamename"));

            // Player Card
            AssignReference(so, "playerAvatar", FindImageByName("playeravatar"));
            AssignReference(so, "playerNameText", FindTextByName("playername"));
            AssignReference(so, "playerLevelText", FindTextByName("playerlevel", "leveltext"));
            AssignReference(so, "playerCard", FindByNameContains<Transform>("playercard"));

            // Opponent Card
            AssignReference(so, "opponentAvatar", FindImageByName("opponentavatar"));
            AssignReference(so, "opponentNameText", FindTextByName("opponentname"));
            AssignReference(so, "opponentLevelText", FindTextByName("opponentlevel"));
            AssignReference(so, "opponentCard", FindByNameContains<Transform>("opponentcard"));
            AssignReference(so, "opponentSearchingIndicator", FindByNameContains<Transform>("searchingindicator"));
            AssignReference(so, "opponentSearchRing", FindImageByName("searchring"));

            // VS Section
            AssignReference(so, "vsContainer", FindByNameContains<Transform>("vscontainer"));
            AssignReference(so, "vsText", FindTextByName("vstext"));

            // Search Status
            AssignReference(so, "statusText", FindTextByName("statustext"));
            AssignReference(so, "timerText", FindTextByName("timertext"));
            AssignReference(so, "searchingSpinner", FindByNameContains<Transform>("searchspinner"));
            AssignReference(so, "searchingRing", FindImageByName("innerring"));

            // Countdown
            AssignReference(so, "countdownPanel", FindByNameContains<Transform>("countdownpanel"));
            AssignReference(so, "countdownText", FindTextByName("countdowntext"));
            AssignReference(so, "getReadyText", FindTextByName("getreadytext", "getready"));

            // Buttons
            AssignReference(so, "cancelButton", FindButtonByName("cancelbutton", "cancel"));
            AssignReference(so, "backButton", FindButtonByName("backbutton", "back"));

            // Effects
            AssignReference(so, "screenFlash", FindImageByName("screenflash", "flash"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(manager.gameObject);
            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
            Log("=== ASSIGNMENT COMPLETE ===");
        }

        private static MonoBehaviour FindMatchmakingManager()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "MatchmakingManager") return mb;
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

        #endregion

        #region Helpers

        private static void ResetLog() { log = ""; assignedCount = 0; failedCount = 0; alreadySetCount = 0; results.Clear(); }
        private static void Log(string msg) { log += msg + "\n"; Debug.Log($"[MatchmakingReferenceAssigner] {msg}"); }
        private static void AddResult(string f, string s, bool ok, Object o) { results.Add(new ReferenceResult { fieldName = f, status = s, success = ok, assignedObject = o }); }

        #endregion
    }
}
