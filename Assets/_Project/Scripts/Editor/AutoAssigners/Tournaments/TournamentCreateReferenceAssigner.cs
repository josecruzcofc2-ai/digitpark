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
    /// Reference Assigner for TournamentCreate scene.
    /// Automatically finds and assigns UI references to TournamentCreateManager.
    ///
    /// Menu: DigitPark/Auto Assigners/References/Tournaments/TournamentCreate References
    /// </summary>
    public class TournamentCreateReferenceAssigner : EditorWindow
    {
        private Vector2 scrollPosition;
        private static string log = "";
        private static int assignedCount = 0;
        private static int failedCount = 0;
        private static int alreadySetCount = 0;
        private static List<ReferenceResult> results = new List<ReferenceResult>();

        private static readonly string[] REQUIRED_REFS = {
            // Header
            "backButton", "titleText",
            // Name Input
            "tournamentNameInput", "nameCharCountText",
            // Game Selection
            "gameTypeDropdown", "selectedGameIcon",
            // Entry Fee
            "entryFeeDropdown", "entryFeeSlider", "customEntryFeeInput", "entryFeeDisplayText",
            // Players
            "maxPlayersDropdown", "estimatedPrizeText",
            // Schedule
            "startTimeDropdown", "startImmediatelyToggle", "scheduledTimeText",
            // Rules
            "roundsDropdown", "timeLimitDropdown", "maxAttemptsDropdown", "allowSpectatorsToggle",
            // Privacy
            "privateToggle", "privateCodeInput",
            // Preview
            "previewPanel", "previewNameText", "previewGameText",
            "previewEntryText", "previewPrizeText", "previewPlayersText",
            // Actions
            "createButton", "previewButton", "createButtonText", "creationFeeText",
            // Status
            "loadingOverlay", "statusText"
        };

        private struct ReferenceResult
        {
            public string fieldName;
            public string status;
            public bool success;
            public Object assignedObject;
        }

        #region Menu Items

        [MenuItem("DigitPark/Auto Assigners/References/Tournaments/TournamentCreate References", false, 161)]
        public static void ShowWindow()
        {
            var window = GetWindow<TournamentCreateReferenceAssigner>("TournamentCreate Reference Assigner");
            window.minSize = new Vector2(600, 500);
        }

        public static void RunAutoAssign()
        {
            ResetLog();
            AssignAllReferences();
        }

        #endregion

        #region Window GUI

        private void OnGUI()
        {
            GUILayout.Label("TournamentCreate Scene Reference Assigner", EditorStyles.boldLabel);
            GUILayout.Space(10);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "TournamentCreate")
            {
                EditorGUILayout.HelpBox(
                    $"Current scene: {currentScene}\n" +
                    "Please open the TournamentCreate scene first!",
                    MessageType.Warning);
            }

            EditorGUILayout.HelpBox(
                "Assigns UI references to TournamentCreateManager:\n" +
                "• Header (back, title)\n" +
                "• Form fields (name, game type, entry fee, players)\n" +
                "• Action buttons (create, preview)\n" +
                "• Status/Loading",
                MessageType.Info);

            GUILayout.Space(10);

            MonoBehaviour targetManager = FindTournamentCreateManager();
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
            Log("=== ASSIGNING TOURNAMENTCREATE REFERENCES ===");

            var manager = FindTournamentCreateManager();
            if (manager == null)
            {
                Log("ERROR: TournamentCreateManager not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            so.Update();

            // Header
            AssignReference(so, "backButton", FindButtonByName("back", "return"));
            AssignReference(so, "titleText", FindTextByName("title", "header"));

            // Name Input
            AssignReference(so, "tournamentNameInput", FindByNameContains<TMP_InputField>("tournamentname", "nameinput", "name"));
            AssignReference(so, "nameCharCountText", FindTextByName("charcount", "count", "character"));

            // Game Selection
            AssignReference(so, "gameTypeDropdown", FindByNameContains<TMP_Dropdown>("gametype", "game", "juego"));
            AssignReference(so, "selectedGameIcon", FindImageByName("selectedgame", "gameicon"));

            // Entry Fee
            AssignReference(so, "entryFeeDropdown", FindByNameContains<TMP_Dropdown>("entryfee", "fee", "entrada"));
            AssignReference(so, "entryFeeSlider", FindByNameContains<Slider>("entryfee", "fee"));
            AssignReference(so, "customEntryFeeInput", FindByNameContains<TMP_InputField>("custom", "customentryfee"));
            AssignReference(so, "entryFeeDisplayText", FindTextByName("entryfeedisplay", "feedisplay", "display"));

            // Players
            AssignReference(so, "maxPlayersDropdown", FindByNameContains<TMP_Dropdown>("maxplayers", "players", "jugadores"));
            AssignReference(so, "estimatedPrizeText", FindTextByName("estimatedprize", "prize", "premio"));

            // Schedule
            AssignReference(so, "startTimeDropdown", FindByNameContains<TMP_Dropdown>("starttime", "schedule", "horario"));
            AssignToggle(so, "startImmediatelyToggle", "immediate", "start", "ahora");
            AssignReference(so, "scheduledTimeText", FindTextByName("scheduledtime", "scheduled", "horario"));

            // Rules
            AssignReference(so, "roundsDropdown", FindByNameContains<TMP_Dropdown>("rounds", "rondas"));
            AssignReference(so, "timeLimitDropdown", FindByNameContains<TMP_Dropdown>("timelimit", "tiempo", "limit"));
            AssignReference(so, "maxAttemptsDropdown", FindByNameContains<TMP_Dropdown>("maxattempts", "intentos"));
            AssignToggle(so, "allowSpectatorsToggle", "spectator", "espectador", "watch");

            // Privacy
            AssignToggle(so, "privateToggle", "private", "privado");
            AssignReference(so, "privateCodeInput", FindByNameContains<TMP_InputField>("privatecode", "code", "codigo"));

            // Preview
            AssignGameObject(so, "previewPanel", "previewpanel", "preview");
            AssignReference(so, "previewNameText", FindTextByName("previewname"));
            AssignReference(so, "previewGameText", FindTextByName("previewgame"));
            AssignReference(so, "previewEntryText", FindTextByName("previewentry"));
            AssignReference(so, "previewPrizeText", FindTextByName("previewprize"));
            AssignReference(so, "previewPlayersText", FindTextByName("previewplayers"));

            // Actions
            AssignReference(so, "createButton", FindButtonByName("create", "crear", "submit"));
            AssignReference(so, "previewButton", FindButtonByName("preview", "vista"));
            AssignReference(so, "createButtonText", FindTextByName("createbuttontext", "createtext"));
            AssignReference(so, "creationFeeText", FindTextByName("creationfee", "feetext", "costo"));

            // Status
            AssignGameObject(so, "loadingOverlay", "loadingoverlay", "loading");
            AssignReference(so, "statusText", FindTextByName("status", "message"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(manager.gameObject);
            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
            Log("=== ASSIGNMENT COMPLETE ===");
        }

        private static MonoBehaviour FindTournamentCreateManager()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "TournamentCreateManager") return mb;
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

        private static void AssignToggle(SerializedObject so, string propertyName, params string[] patterns)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { AddResult(propertyName, "Property not found", false, null); failedCount++; return; }
            if (prop.objectReferenceValue != null) { AddResult(propertyName, "Already Set", true, prop.objectReferenceValue); alreadySetCount++; return; }
            var all = Object.FindObjectsOfType<Toggle>(true);
            foreach (var p in patterns)
                foreach (var o in all)
                    if (o.gameObject.name.ToLower().Contains(p.ToLower()))
                    {
                        prop.objectReferenceValue = o;
                        AddResult(propertyName, "Assigned", true, o);
                        assignedCount++;
                        return;
                    }
            AddResult(propertyName, "Not found", false, null); failedCount++;
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
        private static void Log(string msg) { log += msg + "\n"; Debug.Log($"[TournamentCreateReferenceAssigner] {msg}"); }
        private static void AddResult(string f, string s, bool ok, Object o) { results.Add(new ReferenceResult { fieldName = f, status = s, success = ok, assignedObject = o }); }

        #endregion
    }
}
