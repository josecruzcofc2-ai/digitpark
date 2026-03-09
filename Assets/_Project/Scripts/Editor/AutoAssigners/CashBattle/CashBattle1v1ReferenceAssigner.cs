using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;

namespace DigitPark.Editor.AutoAssigners
{
    /// <summary>
    /// Reference Assigner for CashBattle1v1 scene.
    /// Automatically finds and assigns UI references to CashBattle1v1Manager.
    ///
    /// Menu: DigitPark/Auto Assigners/References/CashBattle/CashBattle 1v1 References
    /// </summary>
    public class CashBattle1v1ReferenceAssigner : EditorWindow
    {
        private Vector2 scrollPosition;
        private static string log = "";
        private static int assignedCount = 0;
        private static int failedCount = 0;
        private static int alreadySetCount = 0;
        private static List<ReferenceResult> results = new List<ReferenceResult>();

        private static readonly string[] REQUIRED_REFS = {
            // Header
            "titleText", "backButton",
            // Game Selection (dropdown + details)
            "gameDropdown", "viewDetailsButton", "selectedGameIcon", "selectedGameDescription",
            // Game Selection Modal
            "gameSelectionModal", "gameCardsContainer", "confirmGameButton", "closeModalButton",
            // Entry Fee
            "selectedFeeText",
            // Note: entryFeeContainer, entryFeeButtons[] excluded - container/array manual assignment
            // Custom Entry
            "customAmountInput", "earningsText", "minMaxText",
            // Online Players
            "onlinePlayersText", "onlineIndicator",
            // Action
            "findOpponentButton", "findOpponentText",
            // Cognitive Sprint
            "cognitiveSprintButton", "cognitiveSprintPanel", "sprintSelectionText"
        };

        private struct ReferenceResult
        {
            public string fieldName;
            public string status;
            public bool success;
            public Object assignedObject;
        }

        #region Menu Items

        [MenuItem("DigitPark/Scenes/Assign References/CashBattle/1v1", false, 181)]
        public static void ShowWindow()
        {
            var window = GetWindow<CashBattle1v1ReferenceAssigner>("CashBattle 1v1 Reference Assigner");
            window.minSize = new Vector2(600, 550);
        }

        #endregion

        #region Window GUI

        private void OnGUI()
        {
            GUILayout.Label("CashBattle 1v1 Scene Reference Assigner", EditorStyles.boldLabel);
            GUILayout.Space(10);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "CashBattle1v1")
            {
                EditorGUILayout.HelpBox(
                    $"Current scene: {currentScene}\n" +
                    "Please open the CashBattle1v1 scene first!",
                    MessageType.Warning);
            }

            EditorGUILayout.HelpBox(
                "Assigns UI references to CashBattle1v1Manager:\n" +
                "- Header (title, back button)\n" +
                "- Game selection dropdown + details\n" +
                "- Game selection modal (overlay with cards)\n" +
                "- Entry fee selection and custom input\n" +
                "- Online players indicator\n" +
                "- Find opponent button",
                MessageType.Info);

            GUILayout.Space(10);

            MonoBehaviour targetPanel = FindCashBattle1v1Manager();
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
            Log("=== ASSIGNING CASHBATTLE 1V1 REFERENCES ===");

            var panel = FindCashBattle1v1Manager();
            if (panel == null)
            {
                Log("ERROR: CashBattle1v1Manager not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            SerializedObject so = new SerializedObject(panel);
            so.Update();

            // Header (UIBuilder: "TitleText", "BackButton")
            AssignReference(so, "titleText", FindTextByName("titletext"));
            AssignReference(so, "backButton", FindButtonByName("backbutton"));

            // Game Selection - Dropdown + Details
            AssignReference(so, "gameDropdown", FindByNameContains<TMP_Dropdown>("gamedropdown"));
            AssignReference(so, "viewDetailsButton", FindButtonByName("viewdetails"));
            AssignReference(so, "selectedGameIcon", FindImageByName("selectedgameicon"));
            AssignReference(so, "selectedGameDescription", FindTextByName("selectedgamedesc"));

            // Game Selection Modal
            AssignGameObject(so, "gameSelectionModal", "GameSelectionModal");
            AssignReference(so, "gameCardsContainer", FindByNameContains<Transform>("gamecardscontainer"));
            AssignReference(so, "confirmGameButton", FindButtonByName("confirmgame"));
            AssignReference(so, "closeModalButton", FindButtonByName("closemodal"));

            // Entry Fee (UIBuilder: "SelectedFeeText")
            AssignReference(so, "selectedFeeText", FindTextByName("selectedfeetext"));

            // Custom Entry (UIBuilder: "CustomInputField", "PotentialEarningsText", "MaxLabel")
            AssignReference(so, "customAmountInput", FindByNameContains<TMP_InputField>("custominputfield"));
            AssignReference(so, "earningsText", FindTextByName("potentialearningstext"));
            AssignReference(so, "minMaxText", FindTextByName("maxlabel"));

            // Online Players (UIBuilder: "OnlinePlayersText", "GreenDot")
            AssignReference(so, "onlinePlayersText", FindTextByName("onlineplayerstext"));
            AssignReference(so, "onlineIndicator", FindImageByName("greendot"));

            // Action Button (UIBuilder: "FindOpponentButton", "FindOpponentText")
            AssignReference(so, "findOpponentButton", FindButtonByName("findopponentbutton"));
            AssignReference(so, "findOpponentText", FindTextByName("findopponenttext"));

            // Cognitive Sprint (la card GameCard_CognitiveSprint ES el boton)
            AssignReference(so, "cognitiveSprintButton", FindButtonByName("gamecard_cognitivesprint"));
            AssignGameObject(so, "cognitiveSprintPanel", "CognitiveSprintPanel");
            AssignReference(so, "sprintSelectionText", FindTextByName("sprintselectiontext"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(panel);
            EditorUtility.SetDirty(panel.gameObject);
            EditorSceneManager.MarkSceneDirty(panel.gameObject.scene);
            Log("=== ASSIGNMENT COMPLETE ===");
        }

        private static MonoBehaviour FindCashBattle1v1Manager()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "CashBattle1v1Manager") return mb;
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
        private static void Log(string msg) { log += msg + "\n"; Debug.Log($"[CashBattle1v1ReferenceAssigner] {msg}"); }
        private static void AddResult(string f, string s, bool ok, Object o) { results.Add(new ReferenceResult { fieldName = f, status = s, success = ok, assignedObject = o }); }

        #endregion
    }
}
