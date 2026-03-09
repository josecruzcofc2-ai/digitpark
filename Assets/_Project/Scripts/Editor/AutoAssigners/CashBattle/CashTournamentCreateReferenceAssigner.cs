using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;

namespace DigitPark.Editor.AutoAssigners
{
    /// <summary>
    /// Reference Assigner for CashTournamentCreate scene.
    /// Automatically finds and assigns UI references to CashTournamentCreateManager.
    ///
    /// Menu: DigitPark/Auto Assigners/References/CashBattle/CashTournamentCreate References
    /// </summary>
    public class CashTournamentCreateReferenceAssigner : EditorWindow
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
            // Tournament Name
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
            "roundsDropdown", "timeLimitDropdown", "maxAttemptsDropdown",
            "allowSpectatorsToggle", "privateToggle", "privateCodeInput",
            // Preview
            "previewPanel", "previewNameText", "previewGameText",
            "previewEntryText", "previewPrizeText", "previewPlayersText",
            // Actions
            "createButton", "createButtonText", "creationFeeText",
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

        [MenuItem("DigitPark/Scenes/Assign References/CashBattle/Tournament Create", false, 184)]
        public static void ShowWindow()
        {
            var window = GetWindow<CashTournamentCreateReferenceAssigner>("CashTournamentCreate Reference Assigner");
            window.minSize = new Vector2(650, 600);
        }

        #endregion

        #region Window GUI

        private void OnGUI()
        {
            GUILayout.Label("CashTournamentCreate Reference Assigner", EditorStyles.boldLabel);
            GUILayout.Space(10);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "CashTournamentCreate")
            {
                EditorGUILayout.HelpBox(
                    $"Current scene: {currentScene}\n" +
                    "Please open the CashTournamentCreate scene first!",
                    MessageType.Warning);
            }

            EditorGUILayout.HelpBox(
                "Assigns UI references to CashTournamentCreateManager:\n" +
                "- Header (title, back button)\n" +
                "- Tournament name input + char count\n" +
                "- Game type dropdown + icon\n" +
                "- Entry fee dropdown, slider, custom input, display\n" +
                "- Players dropdown + estimated prize\n" +
                "- Schedule toggles + dropdowns\n" +
                "- Rules dropdowns + toggles + private code\n" +
                "- Preview panel fields\n" +
                "- Create button + creation fee\n" +
                "- Loading overlay + status text",
                MessageType.Info);

            GUILayout.Space(10);

            MonoBehaviour targetManager = FindCashTournamentCreateManager();
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
            Log("=== ASSIGNING CASHTOURNAMENTCREATE REFERENCES ===");

            var manager = FindCashTournamentCreateManager();
            if (manager == null)
            {
                Log("ERROR: CashTournamentCreateManager not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            so.Update();

            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            Transform root = canvas != null ? canvas.transform : manager.transform.root;

            // Header
            AssignReference(so, "backButton", FindButtonByName(root, "BackButton"));
            AssignReference(so, "titleText", FindTextByName(root, "TitleText"));

            // Tournament Name
            AssignReference(so, "tournamentNameInput", FindInputByName(root, "TournamentNameInput"));
            AssignReference(so, "nameCharCountText", FindTextByName(root, "CharCountText"));

            // Game Selection
            AssignReference(so, "gameTypeDropdown", FindDropdownByName(root, "GameTypeDropdown"));
            AssignReference(so, "selectedGameIcon", FindImageByName(root, "SelectedGameIcon"));

            // Entry Fee
            AssignReference(so, "entryFeeDropdown", FindDropdownByName(root, "EntryFeeDropdown"));
            AssignReference(so, "entryFeeSlider", FindSliderByName(root, "EntryFeeSlider"));
            AssignReference(so, "customEntryFeeInput", FindInputByName(root, "CustomEntryFeeInput"));
            AssignReference(so, "entryFeeDisplayText", FindTextByName(root, "EntryFeeDisplayText"));

            // Players
            AssignReference(so, "maxPlayersDropdown", FindDropdownByName(root, "MaxPlayersDropdown"));
            AssignReference(so, "estimatedPrizeText", FindTextByName(root, "EstimatedPrizeText"));

            // Schedule
            AssignReference(so, "startTimeDropdown", FindDropdownByName(root, "StartTimeDropdown"));
            AssignReference(so, "startImmediatelyToggle", FindToggleByName(root, "StartImmediatelyToggle"));
            AssignReference(so, "scheduledTimeText", FindTextByName(root, "ScheduledTimeText"));

            // Rules
            AssignReference(so, "roundsDropdown", FindDropdownByName(root, "RoundsDropdown"));
            AssignReference(so, "timeLimitDropdown", FindDropdownByName(root, "TimeLimitDropdown"));
            AssignReference(so, "maxAttemptsDropdown", FindDropdownByName(root, "MaxAttemptsDropdown"));
            AssignReference(so, "allowSpectatorsToggle", FindToggleByName(root, "AllowSpectatorsToggle"));
            AssignReference(so, "privateToggle", FindToggleByName(root, "PrivateToggle"));
            AssignReference(so, "privateCodeInput", FindInputByName(root, "PrivateCodeInput"));

            // Preview
            AssignGameObject(so, "previewPanel", root, "PreviewPanel");
            AssignReference(so, "previewNameText", FindTextByName(root, "PreviewNameText"));
            AssignReference(so, "previewGameText", FindTextByName(root, "PreviewGameText"));
            AssignReference(so, "previewEntryText", FindTextByName(root, "PreviewEntryText"));
            AssignReference(so, "previewPrizeText", FindTextByName(root, "PreviewPrizeText"));
            AssignReference(so, "previewPlayersText", FindTextByName(root, "PreviewPlayersText"));

            // Actions
            AssignReference(so, "createButton", FindButtonByName(root, "CreateButton"));
            AssignReference(so, "createButtonText", FindTextByName(root, "CashCreateButtonText"));
            AssignReference(so, "creationFeeText", FindTextByName(root, "CreationFeeText"));

            // Status
            AssignGameObject(so, "loadingOverlay", root, "LoadingOverlay");
            AssignReference(so, "statusText", FindTextByName(root, "CashCreateStatusText"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(manager.gameObject);
            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
            Log("=== ASSIGNMENT COMPLETE ===");
        }

        private static MonoBehaviour FindCashTournamentCreateManager()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "CashTournamentCreateManager") return mb;
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

        private static void AssignGameObject(SerializedObject so, string propertyName, Transform root, string goName)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { AddResult(propertyName, "Property not found", false, null); failedCount++; return; }
            if (prop.objectReferenceValue != null) { AddResult(propertyName, "Already Set", true, prop.objectReferenceValue); alreadySetCount++; return; }

            Transform t = FindDeep(root, goName);
            if (t != null)
            {
                prop.objectReferenceValue = t.gameObject;
                AddResult(propertyName, "Assigned", true, t.gameObject);
                assignedCount++;
            }
            else
            {
                AddResult(propertyName, "Not found", false, null);
                failedCount++;
            }
        }

        #endregion

        #region Finders

        private static Transform FindDeep(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                Transform result = FindDeep(child, name);
                if (result != null) return result;
            }
            return null;
        }

        private static TextMeshProUGUI FindTextByName(Transform root, string name)
        {
            Transform t = FindDeep(root, name);
            return t != null ? t.GetComponent<TextMeshProUGUI>() : null;
        }

        private static Button FindButtonByName(Transform root, string name)
        {
            Transform t = FindDeep(root, name);
            return t != null ? t.GetComponent<Button>() : null;
        }

        private static Image FindImageByName(Transform root, string name)
        {
            Transform t = FindDeep(root, name);
            return t != null ? t.GetComponent<Image>() : null;
        }

        private static TMP_Dropdown FindDropdownByName(Transform root, string name)
        {
            Transform t = FindDeep(root, name);
            return t != null ? t.GetComponent<TMP_Dropdown>() : null;
        }

        private static Slider FindSliderByName(Transform root, string name)
        {
            Transform t = FindDeep(root, name);
            return t != null ? t.GetComponent<Slider>() : null;
        }

        private static TMP_InputField FindInputByName(Transform root, string name)
        {
            Transform t = FindDeep(root, name);
            return t != null ? t.GetComponent<TMP_InputField>() : null;
        }

        private static Toggle FindToggleByName(Transform root, string name)
        {
            Transform t = FindDeep(root, name);
            return t != null ? t.GetComponent<Toggle>() : null;
        }

        #endregion

        #region Helpers

        private static void ResetLog()
        {
            log = "";
            assignedCount = 0;
            failedCount = 0;
            alreadySetCount = 0;
            results.Clear();
        }

        private static void Log(string msg)
        {
            log += msg + "\n";
            Debug.Log($"[CashTournamentCreateReferenceAssigner] {msg}");
        }

        private static void AddResult(string f, string s, bool ok, Object o)
        {
            results.Add(new ReferenceResult { fieldName = f, status = s, success = ok, assignedObject = o });
        }

        #endregion
    }
}
