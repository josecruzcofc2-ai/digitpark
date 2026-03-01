using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;

namespace DigitPark.Editor.AutoAssigners
{
    /// <summary>
    /// Reference Assigner for CashMatchmaking scene.
    /// Automatically finds and assigns UI references to CashMatchmakingManager.
    ///
    /// Menu: DigitPark/Auto Assigners/References/CashBattle/CashMatchmaking References
    /// </summary>
    public class CashMatchmakingReferenceAssigner : EditorWindow
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
            // Entry Fee
            "entryFeeText",
            // Player Card
            "playerAvatar", "playerNameText", "playerLevelText", "playerCard",
            // Opponent Card
            "opponentAvatar", "opponentNameText", "opponentLevelText", "opponentCard",
            "opponentSearchingIndicator", "opponentSearchRing",
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

        [MenuItem("DigitPark/Auto Assigners/References/CashBattle/CashMatchmaking References", false, 182)]
        public static void ShowWindow()
        {
            var window = GetWindow<CashMatchmakingReferenceAssigner>("CashMatchmaking Reference Assigner");
            window.minSize = new Vector2(600, 550);
        }

        #endregion

        #region Window GUI

        private void OnGUI()
        {
            GUILayout.Label("CashMatchmaking Scene Reference Assigner", EditorStyles.boldLabel);
            GUILayout.Space(10);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "CashMatchmaking")
            {
                EditorGUILayout.HelpBox(
                    $"Current scene: {currentScene}\n" +
                    "Please open the CashMatchmaking scene first!",
                    MessageType.Warning);
            }

            EditorGUILayout.HelpBox(
                "Assigns UI references to CashMatchmakingManager:\n" +
                "- Header (title, game icon, game name)\n" +
                "- Entry fee display\n" +
                "- Player card (avatar, name, level)\n" +
                "- Opponent card (avatar, name, level, searching indicator)\n" +
                "- VS section\n" +
                "- Search status (spinner, status text, timer)\n" +
                "- Countdown panel\n" +
                "- Buttons (cancel, back)\n" +
                "- Effects (screen flash)",
                MessageType.Info);

            GUILayout.Space(10);

            MonoBehaviour targetPanel = FindCashMatchmakingManager();
            if (targetPanel != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Target:", GUILayout.Width(50));
                EditorGUILayout.ObjectField(targetPanel, typeof(MonoBehaviour), true);
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
            GUILayout.Label(successRate == 1f ? "ALL REFERENCES SET" : "Some references missing", EditorStyles.boldLabel);
            GUI.color = Color.white;

            GUILayout.Label($"Assigned: {assignedCount} | Already Set: {alreadySetCount} | Failed: {failedCount}");

            foreach (var result in results)
            {
                EditorGUILayout.BeginHorizontal();
                GUI.color = result.success ? (result.status == "Already Set" ? new Color(0.5f, 0.8f, 1f) : Color.green) : Color.red;
                GUILayout.Label(result.success ? (result.status == "Already Set" ? "o" : "+") : "x", GUILayout.Width(20));
                GUI.color = Color.white;
                GUILayout.Label(result.fieldName, GUILayout.Width(200));
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
            Log("=== ASSIGNING CASH MATCHMAKING REFERENCES ===");

            var panel = FindCashMatchmakingManager();
            if (panel == null)
            {
                Log("ERROR: CashMatchmakingManager not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            SerializedObject so = new SerializedObject(panel);
            so.Update();

            // Header (UIBuilder: "TitleText", "GameIcon", "GameNameText")
            AssignReference(so, "titleText", FindTextByName("titletext"));
            AssignReference(so, "gameIconImage", FindImageByName("gameicon"));
            AssignReference(so, "gameTypeText", FindTextByName("gamenametext"));

            // Entry Fee (UIBuilder: "EntryFeeText")
            AssignReference(so, "entryFeeText", FindTextByName("entryfeetext"));

            // Player Card (UIBuilder: "PlayerCard", "PlayerAvatar", "PlayerName", "PlayerLevel/LevelText")
            AssignGameObject(so, "playerCard", "PlayerCard");
            AssignReference(so, "playerAvatar", FindImageByName("playeravatar"));
            AssignReference(so, "playerNameText", FindTextByName("playername"));
            AssignReference(so, "playerLevelText", FindTextByName("leveltext", "playerlevel"));

            // Opponent Card (UIBuilder: "OpponentCard", "OpponentAvatar", "OpponentName", "OpponentLevel/LevelText")
            AssignGameObject(so, "opponentCard", "OpponentCard");
            AssignReference(so, "opponentAvatar", FindImageByName("opponentavatar"));
            AssignReference(so, "opponentNameText", FindTextByName("opponentname"));
            AssignReference(so, "opponentLevelText", FindTextByName("opponentlevel"));
            AssignGameObject(so, "opponentSearchingIndicator", "SearchingIndicator");
            AssignReference(so, "opponentSearchRing", FindImageByName("searchring"));

            // VS Section (UIBuilder: "VSContainer", "VSText")
            AssignGameObject(so, "vsContainer", "VSContainer");
            AssignReference(so, "vsText", FindTextByName("vstext"));

            // Search Status (UIBuilder: "SearchSpinner", "InnerRing", "StatusText", "TimerText")
            AssignGameObject(so, "searchingSpinner", "SearchSpinner");
            AssignReference(so, "searchingRing", FindImageByName("innerring"));
            AssignReference(so, "statusText", FindTextByName("statustext"));
            AssignReference(so, "timerText", FindTextByName("timertext"));

            // Countdown (UIBuilder: "CountdownPanel", "CountdownText", "GetReadyText")
            AssignGameObject(so, "countdownPanel", "CountdownPanel");
            AssignReference(so, "countdownText", FindTextByName("countdowntext"));
            AssignReference(so, "getReadyText", FindTextByName("getreadytext"));

            // Buttons (UIBuilder: "CancelButton", "BackButtonGold")
            AssignReference(so, "cancelButton", FindButtonByName("cancelbutton"));
            AssignReference(so, "backButton", FindButtonByName("backbuttongold"));

            // Effects (UIBuilder: "ScreenFlash")
            AssignReference(so, "screenFlash", FindImageByName("screenflash"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(panel);
            EditorUtility.SetDirty(panel.gameObject);
            EditorSceneManager.MarkSceneDirty(panel.gameObject.scene);
            Log("=== ASSIGNMENT COMPLETE ===");
        }

        private static MonoBehaviour FindCashMatchmakingManager()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "CashMatchmakingManager") return mb;
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
                foreach (var t in all)
                    if (t.gameObject.name.ToLower().Contains(p.ToLower()))
                    {
                        prop.objectReferenceValue = t.gameObject;
                        AddResult(propertyName, "Assigned", true, t.gameObject);
                        assignedCount++;
                        return;
                    }
            AddResult(propertyName, "Not found", false, null); failedCount++;
        }

        #endregion

        #region Helpers

        private static void ResetLog() { log = ""; assignedCount = 0; failedCount = 0; alreadySetCount = 0; results.Clear(); }
        private static void Log(string msg) { log += msg + "\n"; Debug.Log($"[CashMatchmakingReferenceAssigner] {msg}"); }
        private static void AddResult(string f, string s, bool ok, Object o) { results.Add(new ReferenceResult { fieldName = f, status = s, success = ok, assignedObject = o }); }

        #endregion
    }
}
