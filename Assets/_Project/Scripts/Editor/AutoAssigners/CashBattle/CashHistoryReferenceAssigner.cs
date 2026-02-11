using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;

namespace DigitPark.Editor.AutoAssigners
{
    /// <summary>
    /// Reference Assigner for CashHistory scene.
    /// Automatically finds and assigns UI references to CashHistorySceneController.
    ///
    /// Menu: DigitPark/Auto Assigners/References/CashBattle/CashHistory References
    /// </summary>
    public class CashHistoryReferenceAssigner : EditorWindow
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
            // Stats Panel
            "statsPanel", "totalMatchesText", "winRateText", "netProfitText",
            "winsText", "lossesText", "drawsText",
            "currentStreakText", "bestStreakText",
            "tournamentsPlayedText", "tournamentWinsText",
            // Filter Tabs
            "allTabButton", "matchesTabButton", "tournamentsTabButton",
            // Advanced Filters
            "resultFilterDropdown", "dateFilterDropdown", "clearFiltersButton",
            // Match History List
            "entriesContainer", "scrollRect", "emptyStateText", "loadMoreButton",
            // Detail Panel
            "detailPanel", "detailTitleText", "detailSubtitleText",
            "detailResultText", "detailScoreText",
            "detailEntryFeeText", "detailPrizeText", "detailNetText",
            "detailDateText", "detailDurationText",
            "closeDetailButton",
            // Loading
            "loadingIndicator"
        };

        private struct ReferenceResult
        {
            public string fieldName;
            public string status;
            public bool success;
            public Object assignedObject;
        }

        #region Menu Items

        [MenuItem("DigitPark/Auto Assigners/References/CashBattle/CashHistory References", false, 241)]
        public static void ShowWindow()
        {
            var window = GetWindow<CashHistoryReferenceAssigner>("CashHistory Reference Assigner");
            window.minSize = new Vector2(600, 500);
        }

        #endregion

        #region Window GUI

        private void OnGUI()
        {
            GUILayout.Label("CashHistory Scene Reference Assigner", EditorStyles.boldLabel);
            GUILayout.Space(10);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "CashHistory")
            {
                EditorGUILayout.HelpBox(
                    $"Current scene: {currentScene}\n" +
                    "Please open the CashHistory scene first!",
                    MessageType.Warning);
            }

            EditorGUILayout.HelpBox(
                "Assigns UI references to CashHistorySceneController:\n" +
                "- Header (back button, title)\n" +
                "- Stats panel (matches, win rate, profit, streaks)\n" +
                "- Filter tabs (all, matches, tournaments)\n" +
                "- Advanced filters (dropdowns)\n" +
                "- Match history list and scroll view\n" +
                "- Detail panel with all fields\n" +
                "- Loading indicator",
                MessageType.Info);

            GUILayout.Space(10);

            MonoBehaviour targetController = FindController();
            if (targetController != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Target:", GUILayout.Width(50));
                EditorGUILayout.ObjectField(targetController, typeof(MonoBehaviour), true);
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
            Log("=== ASSIGNING CASHHISTORY REFERENCES ===");

            var controller = FindController();
            if (controller == null)
            {
                Log("ERROR: CashHistorySceneController not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            SerializedObject so = new SerializedObject(controller);
            so.Update();

            Canvas canvas = FindMainCanvas();
            Transform root = canvas != null ? canvas.transform : controller.transform.root;

            // ==================== HEADER ====================
            Transform backBtnT = FindDeep(root, "BackButton");
            AssignReference(so, "backButton", backBtnT != null ? backBtnT.GetComponent<Button>() : null);

            AssignReference(so, "titleText", FindTextByDeep(root, "TitleText"));

            // ==================== STATS PANEL ====================
            Transform statsPanelT = FindDeep(root, "StatsPanel");
            AssignReference(so, "statsPanel", statsPanelT != null ? statsPanelT.gameObject : null);

            // Try primary name first, then fallback name
            TextMeshProUGUI totalMatchesTMP = FindTextByDeep(root, "TotalMatchesValue");
            if (totalMatchesTMP == null) totalMatchesTMP = FindTextByDeep(root, "TotalMatchesText");
            AssignReference(so, "totalMatchesText", totalMatchesTMP);

            TextMeshProUGUI winRateTMP = FindTextByDeep(root, "WinRateValue");
            if (winRateTMP == null) winRateTMP = FindTextByDeep(root, "WinRateText");
            AssignReference(so, "winRateText", winRateTMP);

            TextMeshProUGUI netProfitTMP = FindTextByDeep(root, "NetProfitValue");
            if (netProfitTMP == null) netProfitTMP = FindTextByDeep(root, "NetProfitText");
            AssignReference(so, "netProfitText", netProfitTMP);

            TextMeshProUGUI winsTMP = FindTextByDeep(root, "WinsValue");
            if (winsTMP == null) winsTMP = FindTextByDeep(root, "WinsText");
            AssignReference(so, "winsText", winsTMP);

            TextMeshProUGUI lossesTMP = FindTextByDeep(root, "LossesValue");
            if (lossesTMP == null) lossesTMP = FindTextByDeep(root, "LossesText");
            AssignReference(so, "lossesText", lossesTMP);

            TextMeshProUGUI drawsTMP = FindTextByDeep(root, "DrawsValue");
            if (drawsTMP == null) drawsTMP = FindTextByDeep(root, "DrawsText");
            AssignReference(so, "drawsText", drawsTMP);

            TextMeshProUGUI currentStreakTMP = FindTextByDeep(root, "CurrentStreakValue");
            if (currentStreakTMP == null) currentStreakTMP = FindTextByDeep(root, "CurrentStreakText");
            AssignReference(so, "currentStreakText", currentStreakTMP);

            TextMeshProUGUI bestStreakTMP = FindTextByDeep(root, "BestStreakValue");
            if (bestStreakTMP == null) bestStreakTMP = FindTextByDeep(root, "BestStreakText");
            AssignReference(so, "bestStreakText", bestStreakTMP);

            AssignReference(so, "tournamentsPlayedText", FindTextByDeep(root, "TournamentsPlayedValue"));
            AssignReference(so, "tournamentWinsText", FindTextByDeep(root, "TournamentWinsValue"));

            // ==================== FILTER TABS ====================
            Transform allTabT = FindDeep(root, "AllTab");
            if (allTabT == null) allTabT = FindDeep(root, "AllTabButton");
            AssignReference(so, "allTabButton", allTabT != null ? allTabT.GetComponent<Button>() : null);

            Transform matchesTabT = FindDeep(root, "MatchesTab");
            if (matchesTabT == null) matchesTabT = FindDeep(root, "MatchesTabButton");
            AssignReference(so, "matchesTabButton", matchesTabT != null ? matchesTabT.GetComponent<Button>() : null);

            Transform tournamentsTabT = FindDeep(root, "TournamentsTab");
            if (tournamentsTabT == null) tournamentsTabT = FindDeep(root, "TournamentsTabButton");
            AssignReference(so, "tournamentsTabButton", tournamentsTabT != null ? tournamentsTabT.GetComponent<Button>() : null);

            // ==================== ADVANCED FILTERS ====================
            Transform resultFilterT = FindDeep(root, "ResultFilterDropdown");
            AssignReference(so, "resultFilterDropdown", resultFilterT != null ? resultFilterT.GetComponent<TMP_Dropdown>() : null);

            Transform dateFilterT = FindDeep(root, "DateFilterDropdown");
            AssignReference(so, "dateFilterDropdown", dateFilterT != null ? dateFilterT.GetComponent<TMP_Dropdown>() : null);

            Transform clearFiltersT = FindDeep(root, "ClearFiltersButton");
            AssignReference(so, "clearFiltersButton", clearFiltersT != null ? clearFiltersT.GetComponent<Button>() : null);

            // ==================== MATCH HISTORY LIST ====================
            // entriesContainer: look for EntriesContainer first, then Content inside a ScrollView
            Transform entriesContainerT = FindDeep(root, "EntriesContainer");
            if (entriesContainerT == null)
            {
                // Try to find Content inside MatchHistoryList ScrollView
                Transform scrollViewT = FindDeep(root, "MatchHistoryList");
                if (scrollViewT != null)
                {
                    entriesContainerT = FindDeep(scrollViewT, "Content");
                }
            }
            AssignReference(so, "entriesContainer", entriesContainerT);

            // historyEntryPrefab - skip (prefab, can't auto-assign from scene)
            Log("~ historyEntryPrefab: Skipped (prefab reference, assign manually)");

            // scrollRect
            Transform scrollRectT = FindDeep(root, "MatchHistoryList");
            if (scrollRectT == null)
            {
                // Find any ScrollRect in the scene
                ScrollRect[] allScrollRects = Object.FindObjectsOfType<ScrollRect>(true);
                foreach (var sr in allScrollRects)
                {
                    if (sr.gameObject.name != "TransitionCanvas")
                    {
                        scrollRectT = sr.transform;
                        break;
                    }
                }
            }
            AssignReference(so, "scrollRect", scrollRectT != null ? scrollRectT.GetComponent<ScrollRect>() : null);

            AssignReference(so, "emptyStateText", FindTextByDeep(root, "EmptyStateText"));

            Transform loadMoreT = FindDeep(root, "LoadMoreButton");
            AssignReference(so, "loadMoreButton", loadMoreT != null ? loadMoreT.GetComponent<Button>() : null);

            // ==================== DETAIL PANEL ====================
            Transform detailPanelT = FindDeep(root, "DetailPanel");
            AssignReference(so, "detailPanel", detailPanelT != null ? detailPanelT.gameObject : null);

            AssignReference(so, "detailTitleText", FindTextByDeep(root, "DetailTitleText"));
            AssignReference(so, "detailSubtitleText", FindTextByDeep(root, "DetailSubtitleText"));
            AssignReference(so, "detailResultText", FindTextByDeep(root, "DetailResultText"));
            AssignReference(so, "detailScoreText", FindTextByDeep(root, "DetailScoreText"));
            AssignReference(so, "detailEntryFeeText", FindTextByDeep(root, "DetailEntryFeeText"));
            AssignReference(so, "detailPrizeText", FindTextByDeep(root, "DetailPrizeText"));
            AssignReference(so, "detailNetText", FindTextByDeep(root, "DetailNetText"));
            AssignReference(so, "detailDateText", FindTextByDeep(root, "DetailDateText"));
            AssignReference(so, "detailDurationText", FindTextByDeep(root, "DetailDurationText"));

            Transform closeDetailT = FindDeep(root, "CloseDetailButton");
            AssignReference(so, "closeDetailButton", closeDetailT != null ? closeDetailT.GetComponent<Button>() : null);

            // ==================== LOADING ====================
            Transform loadingT = FindDeep(root, "LoadingIndicator");
            AssignReference(so, "loadingIndicator", loadingT != null ? loadingT.gameObject : null);

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(controller.gameObject);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            Log("=== ASSIGNMENT COMPLETE ===");
        }

        private static Canvas FindMainCanvas()
        {
            foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (c.gameObject.name == "Canvas")
                    return c;
            }
            foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (c.gameObject.name != "TransitionCanvas")
                    return c;
            }
            return Object.FindFirstObjectByType<Canvas>();
        }

        private static MonoBehaviour FindController()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "CashHistorySceneController") return mb;
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

        #endregion

        #region Helpers

        private static void ResetLog() { log = ""; assignedCount = 0; failedCount = 0; alreadySetCount = 0; results.Clear(); }
        private static void Log(string msg) { log += msg + "\n"; Debug.Log($"[CashHistoryReferenceAssigner] {msg}"); }
        private static void AddResult(string f, string s, bool ok, Object o) { results.Add(new ReferenceResult { fieldName = f, status = s, success = ok, assignedObject = o }); }

        #endregion
    }
}
