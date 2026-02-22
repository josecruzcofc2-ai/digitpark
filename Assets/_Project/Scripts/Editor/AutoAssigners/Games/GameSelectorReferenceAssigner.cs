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
    /// Reference Assigner for GameSelector scene.
    /// Automatically finds and assigns UI references to GameSelectorManager.
    ///
    /// Menu: DigitPark/Auto Assigners/References/Games/GameSelector References
    /// </summary>
    public class GameSelectorReferenceAssigner : EditorWindow
    {
        private Vector2 scrollPosition;
        private static string log = "";
        private static int assignedCount = 0;
        private static int failedCount = 0;
        private static int alreadySetCount = 0;
        private static List<ReferenceResult> results = new List<ReferenceResult>();

        private static readonly string[] REQUIRED_REFS = {
            "digitRushButton", "memoryPairsButton", "quickMathButton",
            "flashTapButton", "oddOneOutButton",
            "cognitiveSprintButton", "cognitiveSprintPanel",
            "backButton",
            "rulesPanel", "rulesPlayButton", "rulesCancelButton"
        };

        private struct ReferenceResult
        {
            public string fieldName;
            public string status;
            public bool success;
            public Object assignedObject;
        }

        #region Menu Items

        [MenuItem("DigitPark/Auto Assigners/References/Games/GameSelector References", false, 237)]
        public static void ShowWindow()
        {
            var window = GetWindow<GameSelectorReferenceAssigner>("GameSelector Reference Assigner");
            window.minSize = new Vector2(600, 550);
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
            GUILayout.Label("GameSelector Scene Reference Assigner", EditorStyles.boldLabel);
            GUILayout.Space(10);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "GameSelector")
            {
                EditorGUILayout.HelpBox(
                    $"Current scene: {currentScene}\n" +
                    "Please open the GameSelector scene first!",
                    MessageType.Warning);
                GUILayout.Space(10);
            }

            EditorGUILayout.HelpBox(
                "Assigns UI references to GameSelectorManager:\n" +
                "• Game buttons (5 individual games)\n" +
                "• Cognitive Sprint button and panel\n" +
                "• Rules panel\n" +
                "• Back button",
                MessageType.Info);

            GUILayout.Space(10);

            MonoBehaviour targetManager = FindGameSelectorManager();

            if (targetManager == null)
            {
                EditorGUILayout.HelpBox(
                    "GameSelectorManager not found in scene!\n" +
                    "Add a GameSelectorManager component to assign references.",
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
                    GUI.color = result.status == "Already Set" ? new Color(0.5f, 0.8f, 1f) : Color.green;
                    GUILayout.Label(result.status == "Already Set" ? "●" : "✓", GUILayout.Width(20));
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
                    EditorGUILayout.ObjectField(result.assignedObject, typeof(Object), true, GUILayout.Width(150));

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        #endregion

        #region Assignment Logic

        private static void ScanCurrentReferences()
        {
            Log("=== SCANNING GAMESELECTOR REFERENCES ===");

            var manager = FindGameSelectorManager();
            if (manager == null)
            {
                Log("ERROR: GameSelectorManager not found in scene!");
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
            Log("=== ASSIGNING GAMESELECTOR REFERENCES ===");

            var manager = FindGameSelectorManager();
            if (manager == null)
            {
                Log("ERROR: GameSelectorManager not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            so.Update();

            // Game Buttons
            AssignReference(so, "digitRushButton", FindButtonByName("digitrush", "rush"));
            AssignReference(so, "memoryPairsButton", FindButtonByName("memory", "pairs"));
            AssignReference(so, "quickMathButton", FindButtonByName("quickmath", "math"));
            AssignReference(so, "flashTapButton", FindButtonByName("flashtap", "flash", "tap"));
            AssignReference(so, "oddOneOutButton", FindButtonByName("oddoneout", "odd", "one"));

            // Cognitive Sprint
            AssignReference(so, "cognitiveSprintButton", FindButtonByName("cognitivesprint", "sprint", "cognitive"));
            AssignReference(so, "cognitiveSprintPanel", FindByNameContains<Transform>("cognitivesprintpanel", "sprintpanel"));

            // Navigation
            AssignReference(so, "backButton", FindButtonByName("back", "return", "close"));

            // Rules Panel
            AssignReference(so, "rulesPanel", FindByNameContains<Transform>("rulespanel", "rules"));
            AssignReference(so, "rulesPlayButton", FindButtonByName("rulesplay", "play", "start"));
            AssignReference(so, "rulesCancelButton", FindButtonByName("rulescancel", "cancel", "close"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(manager.gameObject);
            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);

            Log("=== ASSIGNMENT COMPLETE ===");
        }

        private static MonoBehaviour FindGameSelectorManager()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
            {
                if (mb.GetType().Name == "GameSelectorManager")
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
                failedCount++;
                return;
            }

            if (prop.objectReferenceValue != null)
            {
                AddResult(propertyName, "Already Set", true, prop.objectReferenceValue);
                alreadySetCount++;
                return;
            }

            if (value != null)
            {
                prop.objectReferenceValue = value;
                AddResult(propertyName, "Assigned", true, value);
                assignedCount++;
            }
            else
            {
                AddResult(propertyName, "Not found in scene", false, null);
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
            Debug.Log($"[GameSelectorReferenceAssigner] {message}");
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
