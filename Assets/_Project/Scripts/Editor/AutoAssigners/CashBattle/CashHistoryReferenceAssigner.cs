using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;
using System.Reflection;

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
            "winsText", "lossesText",
            // Tabs
            "allTabButton", "matchesTabButton", "tournamentsTabButton",
            // List
            "entriesContainer", "emptyStateText", "loadMoreButton",
            // Detail Panel
            "detailPanel", "closeDetailButton",
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

        [MenuItem("DigitPark/Auto Assigners/References/CashBattle/CashHistory References", false, 215)]
        public static void ShowWindow()
        {
            var window = GetWindow<CashHistoryReferenceAssigner>("CashHistory Reference Assigner");
            window.minSize = new Vector2(600, 550);
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
                GUILayout.Space(10);
            }

            EditorGUILayout.HelpBox(
                "Assigns UI references to CashHistorySceneController:\n" +
                "• Header (back, title)\n" +
                "• Stats panel (matches, wins, profit)\n" +
                "• Tabs (all, matches, tournaments)\n" +
                "• Entry list and detail panel",
                MessageType.Info);

            GUILayout.Space(10);

            MonoBehaviour targetManager = FindCashHistorySceneController();

            if (targetManager == null)
            {
                EditorGUILayout.HelpBox(
                    "CashHistorySceneController not found in scene!\n" +
                    "Add a CashHistorySceneController component to assign references.",
                    MessageType.Error);
                GUILayout.Space(10);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Target:", GUILayout.Width(50));
                EditorGUILayout.ObjectField(targetManager, typeof(MonoBehaviour), true);
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(10);

            GUI.backgroundColor = new Color(0.7f, 0.85f, 1f);
            if (GUILayout.Button("Scan Current References", GUILayout.Height(30)))
            {
                ResetLog();
                ScanCurrentReferences();
                Repaint();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(5);

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

            GUILayout.Label("Detailed Log:", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
            EditorGUILayout.TextArea(log, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        private void DrawResultsSummary()
        {
            if (results.Count == 0) return;

            int total = results.Count;
            int successTotal = assignedCount + alreadySetCount;

            EditorGUILayout.BeginVertical("box");

            float successRate = (float)successTotal / total;
            Color summaryColor;
            string summaryText;

            if (successRate == 1f)
            {
                summaryColor = new Color(0.2f, 0.8f, 0.2f);
                summaryText = "✓ ALL REFERENCES SET";
            }
            else if (successRate >= 0.7f)
            {
                summaryColor = new Color(1f, 0.8f, 0.2f);
                summaryText = "⚠ PARTIAL - Some references missing";
            }
            else
            {
                summaryColor = new Color(1f, 0.4f, 0.4f);
                summaryText = "✗ INCOMPLETE - Many references missing";
            }

            GUI.color = summaryColor;
            GUILayout.Label(summaryText, EditorStyles.boldLabel);
            GUI.color = Color.white;

            GUILayout.Label($"Assigned: {assignedCount} | Already Set: {alreadySetCount} | Failed: {failedCount}");

            GUILayout.Space(5);

            foreach (var result in results)
            {
                EditorGUILayout.BeginHorizontal();

                if (result.success)
                {
                    if (result.status == "Already Set")
                    {
                        GUI.color = new Color(0.5f, 0.8f, 1f);
                        GUILayout.Label("●", GUILayout.Width(20));
                    }
                    else
                    {
                        GUI.color = Color.green;
                        GUILayout.Label("✓", GUILayout.Width(20));
                    }
                }
                else
                {
                    GUI.color = Color.red;
                    GUILayout.Label("✗", GUILayout.Width(20));
                }
                GUI.color = Color.white;

                GUILayout.Label(result.fieldName, GUILayout.Width(180));
                GUILayout.Label(result.status, GUILayout.Width(120));

                if (result.assignedObject != null)
                {
                    EditorGUILayout.ObjectField(result.assignedObject, typeof(Object), true, GUILayout.Width(150));
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        #endregion

        #region Assignment Logic

        private static void ScanCurrentReferences()
        {
            Log("=== SCANNING CASHHISTORY REFERENCES ===");

            var manager = FindCashHistorySceneController();
            if (manager == null)
            {
                Log("ERROR: CashHistorySceneController not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            foreach (var fieldName in REQUIRED_REFS)
            {
                var field = GetField(manager, fieldName);
                if (field != null)
                {
                    var value = field.GetValue(manager);
                    if (value != null && !(value is Object obj && obj == null))
                    {
                        AddResult(fieldName, "Already Set", true, value as Object);
                        alreadySetCount++;
                    }
                    else
                    {
                        AddResult(fieldName, "Not Set", false, null);
                        failedCount++;
                    }
                }
                else
                {
                    AddResult(fieldName, "Field not found", false, null);
                    failedCount++;
                }
            }

            Log("=== SCAN COMPLETE ===");
        }

        private static void AssignAllReferences()
        {
            Log("=== ASSIGNING CASHHISTORY REFERENCES ===");

            var manager = FindCashHistorySceneController();
            if (manager == null)
            {
                Log("ERROR: CashHistorySceneController not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            SerializedObject so = new SerializedObject(manager);

            // Header
            AssignReference(so, "backButton", FindButtonByName("back", "return", "close"));
            AssignReference(so, "titleText", FindTextByName("title", "header", "history"));

            // Stats Panel
            AssignReference(so, "statsPanel", FindByNameContains<Transform>("statspanel", "stats"));
            AssignReference(so, "totalMatchesText", FindTextByName("totalmatches", "matches", "partidas"));
            AssignReference(so, "winRateText", FindTextByName("winrate", "rate", "porcentaje"));
            AssignReference(so, "netProfitText", FindTextByName("netprofit", "profit", "ganancia"));
            AssignReference(so, "winsText", FindTextByName("wins", "victorias"));
            AssignReference(so, "lossesText", FindTextByName("losses", "derrotas"));

            // Tabs
            AssignReference(so, "allTabButton", FindButtonByName("alltab", "all", "todos"));
            AssignReference(so, "matchesTabButton", FindButtonByName("matchestab", "matches", "partidas"));
            AssignReference(so, "tournamentsTabButton", FindButtonByName("tournamentstab", "tournaments", "torneos"));

            // List
            AssignReference(so, "entriesContainer", FindByNameContains<Transform>("entriescontainer", "entries", "list"));
            AssignReference(so, "emptyStateText", FindTextByName("empty", "noentries", "vacio"));
            AssignReference(so, "loadMoreButton", FindButtonByName("loadmore", "more", "cargar"));

            // Detail Panel
            AssignReference(so, "detailPanel", FindByNameContains<Transform>("detailpanel", "detail"));
            AssignReference(so, "closeDetailButton", FindButtonByName("closedetail", "close", "cerrar"));

            // Loading
            AssignReference(so, "loadingIndicator", FindByNameContains<Transform>("loading", "indicator", "spinner"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);

            Log("=== ASSIGNMENT COMPLETE ===");
        }

        private static MonoBehaviour FindCashHistorySceneController()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
            {
                if (mb.GetType().Name == "CashHistorySceneController")
                    return mb;
            }
            return null;
        }

        private static void AssignReference(SerializedObject so, string propertyName, Object value)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null)
            {
                AddResult(propertyName, "Property not found", false, null);
                Log($"? Property not found: {propertyName}");
                failedCount++;
                return;
            }

            if (prop.objectReferenceValue != null)
            {
                AddResult(propertyName, "Already Set", true, prop.objectReferenceValue);
                Log($"● {propertyName}: Already set to {prop.objectReferenceValue.name}");
                alreadySetCount++;
                return;
            }

            if (value != null)
            {
                prop.objectReferenceValue = value;
                AddResult(propertyName, "Assigned", true, value);
                Log($"+ {propertyName}: Assigned {value.name}");
                assignedCount++;
            }
            else
            {
                AddResult(propertyName, "Not found in scene", false, null);
                Log($"✗ {propertyName}: Not found in scene");
                failedCount++;
            }
        }

        #endregion

        #region Finders

        private static T FindByNameContains<T>(params string[] patterns) where T : Component
        {
            var all = Object.FindObjectsOfType<T>(true);
            foreach (var pattern in patterns)
            {
                foreach (var obj in all)
                {
                    if (obj.gameObject.name.ToLower().Contains(pattern.ToLower()))
                        return obj;
                }
            }
            return null;
        }

        private static TextMeshProUGUI FindTextByName(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<TextMeshProUGUI>(true);
            foreach (var pattern in patterns)
            {
                foreach (var text in all)
                {
                    if (text.gameObject.name.ToLower().Contains(pattern.ToLower()))
                        return text;
                }
            }
            return null;
        }

        private static Button FindButtonByName(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<Button>(true);
            foreach (var pattern in patterns)
            {
                foreach (var btn in all)
                {
                    if (btn.gameObject.name.ToLower().Contains(pattern.ToLower()))
                        return btn;
                }
            }
            return null;
        }

        private static FieldInfo GetField(object obj, string fieldName)
        {
            var type = obj.GetType();
            return type.GetField(fieldName,
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.FlattenHierarchy);
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

        private static void Log(string message)
        {
            log += message + "\n";
            Debug.Log($"[CashHistoryReferenceAssigner] {message}");
        }

        private static void AddResult(string field, string status, bool success, Object obj)
        {
            results.Add(new ReferenceResult
            {
                fieldName = field,
                status = status,
                success = success,
                assignedObject = obj
            });
        }

        #endregion
    }
}
