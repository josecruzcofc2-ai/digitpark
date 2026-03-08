using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;
using DigitPark.Editor;

namespace DigitPark.Editor.AutoAssigners
{
    /// <summary>
    /// Reference Assigner for CashTournaments scene.
    /// Automatically finds and assigns UI references to CashTournamentsManager.
    ///
    /// Menu: DigitPark/Auto Assigners/References/CashBattle/CashTournaments References
    /// </summary>
    public class CashTournamentsReferenceAssigner : EditorWindow
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
            // Tournament List
            "tournamentsContainer", "noTournamentsText",
            // Note: tournamentCardPrefab excluded - prefab requires manual assignment
            // Filters
            "gameFilterDropdown", "feeFilterDropdown",
            // Actions
            "refreshButton", "loadingIndicator",
            // Create Tournament
            "createTournamentButton", "createTournamentPanel",
            "maxPlayersSlider", "maxPlayersText",
            "entryFeeSlider", "entryFeeText",
            "durationSlider", "durationText",
            "gameTypeDropdown", "confirmCreateButton", "cancelCreateButton",
            // Premium
            "premiumRequiredPanel"
        };

        private struct ReferenceResult
        {
            public string fieldName;
            public string status;
            public bool success;
            public Object assignedObject;
        }

        #region Menu Items

        [MenuItem("DigitPark/Auto Assigners/References/CashBattle/CashTournaments References", false, 183)]
        public static void ShowWindow()
        {
            var window = GetWindow<CashTournamentsReferenceAssigner>("CashTournaments Reference Assigner");
            window.minSize = new Vector2(600, 500);
        }

        #endregion

        #region Window GUI

        private void OnGUI()
        {
            GUILayout.Label("CashTournaments Scene Reference Assigner", EditorStyles.boldLabel);
            GUILayout.Space(10);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "CashTournaments")
            {
                EditorGUILayout.HelpBox(
                    $"Current scene: {currentScene}\n" +
                    "Please open the CashTournaments scene first!",
                    MessageType.Warning);
            }

            EditorGUILayout.HelpBox(
                "Assigns UI references to CashTournamentsManager:\n" +
                "- Header (title, back button)\n" +
                "- Tournaments container and empty text\n" +
                "- Filter dropdowns (game, fee)\n" +
                "- Refresh button and loading indicator\n" +
                "- Create tournament panel (sliders, dropdowns, buttons)\n" +
                "- Premium required panel (ConfirmPanelUI)",
                MessageType.Info);

            GUILayout.Space(10);

            MonoBehaviour targetPanel = FindCashTournamentsManager();
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

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
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

        /// <summary>
        /// Ejecuta la asignacion de referencias. Llamable desde otros Editor scripts.
        /// </summary>
        public static void RunAutoAssign()
        {
            ResetLog();
            AssignAllReferences();
        }

        #region Assignment Logic

        private static void AssignAllReferences()
        {
            Log("=== ASSIGNING CASHTOURNAMENTS REFERENCES ===");

            var panel = FindCashTournamentsManager();
            if (panel == null)
            {
                Log("ERROR: CashTournamentsManager not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            SerializedObject so = new SerializedObject(panel);
            so.Update();

            Canvas canvas = FindMainCanvas();
            Transform root = canvas != null ? canvas.transform : panel.transform.root;

            // Header
            AssignReference(so, "titleText", FindTextByDeep(root, "TitleText"));

            Transform backBtnT = FindDeep(root, "BackButton");
            AssignReference(so, "backButton", backBtnT != null ? backBtnT.GetComponent<Button>() : null);

            // Tournament List
            Transform containerT = FindDeep(root, "TournamentsContainer");
            if (containerT == null) containerT = FindDeep(root, "Content");
            AssignReference(so, "tournamentsContainer", containerT);

            var tournamentPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/CashBattle/Tournaments/TournamentCardUI.prefab");
            AssignReference(so, "tournamentCardPrefab", tournamentPrefab);

            TextMeshProUGUI noTournamentsT = FindTextByDeep(root, "NoTournamentsText");
            if (noTournamentsT == null) noTournamentsT = FindTextByDeep(root, "EmptyStateText");
            AssignReference(so, "noTournamentsText", noTournamentsT);

            Transform emptyStateT = FindDeep(root, "EmptyState");
            AssignReference(so, "emptyState", emptyStateT != null ? emptyStateT.gameObject : null);

            // Filters
            Transform gameFilterT = FindDeep(root, "GameFilterDropdown");
            AssignReference(so, "gameFilterDropdown", gameFilterT != null ? gameFilterT.GetComponent<TMP_Dropdown>() : null);

            Transform feeFilterT = FindDeep(root, "FeeFilterDropdown");
            AssignReference(so, "feeFilterDropdown", feeFilterT != null ? feeFilterT.GetComponent<TMP_Dropdown>() : null);

            // Actions
            Transform refreshBtnT = FindDeep(root, "RefreshButton");
            AssignReference(so, "refreshButton", refreshBtnT != null ? refreshBtnT.GetComponent<Button>() : null);

            Transform loadingT = FindDeep(root, "LoadingIndicator");
            AssignReference(so, "loadingIndicator", loadingT != null ? loadingT.gameObject : null);

            // Create Tournament
            Transform createBtnT = FindDeep(root, "CreateTournamentBtn");
            if (createBtnT == null) createBtnT = FindDeep(root, "CreateTournamentButton");
            AssignReference(so, "createTournamentButton", createBtnT != null ? createBtnT.GetComponent<Button>() : null);

            Transform createPanelT = FindDeep(root, "CreateTournamentPanel");
            AssignReference(so, "createTournamentPanel", createPanelT != null ? createPanelT.gameObject : null);

            // Sliders
            Transform maxPlayersSliderT = FindDeep(root, "MaxPlayersSlider");
            AssignReference(so, "maxPlayersSlider", maxPlayersSliderT != null ? maxPlayersSliderT.GetComponent<Slider>() : null);

            AssignReference(so, "maxPlayersText", FindTextByDeep(root, "MaxPlayersText"));

            Transform entryFeeSliderT = FindDeep(root, "EntryFeeSlider");
            AssignReference(so, "entryFeeSlider", entryFeeSliderT != null ? entryFeeSliderT.GetComponent<Slider>() : null);

            AssignReference(so, "entryFeeText", FindTextByDeep(root, "EntryFeeText"));

            Transform durationSliderT = FindDeep(root, "DurationSlider");
            AssignReference(so, "durationSlider", durationSliderT != null ? durationSliderT.GetComponent<Slider>() : null);

            AssignReference(so, "durationText", FindTextByDeep(root, "DurationText"));

            // Game Type Dropdown
            Transform gameTypeT = FindDeep(root, "GameTypeDropdown");
            AssignReference(so, "gameTypeDropdown", gameTypeT != null ? gameTypeT.GetComponent<TMP_Dropdown>() : null);

            // Confirm / Cancel Create
            Transform confirmCreateT = FindDeep(root, "ConfirmCreateButton");
            AssignReference(so, "confirmCreateButton", confirmCreateT != null ? confirmCreateT.GetComponent<Button>() : null);

            Transform cancelCreateT = FindDeep(root, "CancelCreateButton");
            AssignReference(so, "cancelCreateButton", cancelCreateT != null ? cancelCreateT.GetComponent<Button>() : null);

            // Premium Required Panel (find by type ConfirmPanelUI)
            AssignReference(so, "premiumRequiredPanel", FindMonoBehaviourByType("ConfirmPanelUI"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(panel);
            EditorUtility.SetDirty(panel.gameObject);
            EditorSceneManager.MarkSceneDirty(panel.gameObject.scene);
            Log("=== ASSIGNMENT COMPLETE ===");
        }

        private static MonoBehaviour FindCashTournamentsManager()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "CashTournamentsManager") return mb;
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

        private static Canvas FindMainCanvas()
        {
            return UIBuilderCanvasHelper.FindMainCanvas();
        }

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

        private static TextMeshProUGUI FindTextByDeep(Transform root, string name)
        {
            Transform t = FindDeep(root, name);
            return t != null ? t.GetComponent<TextMeshProUGUI>() : null;
        }

        private static MonoBehaviour FindMonoBehaviourByType(string typeName)
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
            {
                if (mb.GetType().Name == typeName)
                    return mb;
            }
            return null;
        }

        #endregion

        #region Helpers

        private static void ResetLog() { log = ""; assignedCount = 0; failedCount = 0; alreadySetCount = 0; results.Clear(); }
        private static void Log(string msg) { log += msg + "\n"; Debug.Log($"[CashTournamentsReferenceAssigner] {msg}"); }
        private static void AddResult(string f, string s, bool ok, Object o) { results.Add(new ReferenceResult { fieldName = f, status = s, success = ok, assignedObject = o }); }

        #endregion
    }
}
