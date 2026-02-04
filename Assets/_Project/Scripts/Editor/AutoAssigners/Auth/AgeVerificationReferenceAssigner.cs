using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;
using System.Reflection;

namespace DigitPark.Editor.AutoAssigners
{
    /// <summary>
    /// Reference Assigner for AgeVerification scene.
    /// Automatically finds and assigns UI references to AgeVerificationManager.
    ///
    /// Menu: DigitPark/Auto Assigners/References/Auth/AgeVerification References
    /// </summary>
    public class AgeVerificationReferenceAssigner : EditorWindow
    {
        private Vector2 scrollPosition;
        private static string log = "";
        private static int assignedCount = 0;
        private static int failedCount = 0;
        private static int alreadySetCount = 0;
        private static List<ReferenceResult> results = new List<ReferenceResult>();

        // Expected references for AgeVerificationManager (only critical/important ones)
        // Removed: successIcon, errorIcon (optional visual indicators)
        private static readonly string[] REQUIRED_REFS = {
            "titleText", "descriptionText", "statusText",
            "verifyButton", "backButton", "loadingIndicator"
        };

        private struct ReferenceResult
        {
            public string fieldName;
            public string status;
            public bool success;
            public Object assignedObject;
        }

        #region Menu Items

        [MenuItem("DigitPark/Auto Assigners/References/Auth/AgeVerification References", false, 205)]
        public static void ShowWindow()
        {
            var window = GetWindow<AgeVerificationReferenceAssigner>("AgeVerification Reference Assigner");
            window.minSize = new Vector2(600, 500);
        }

        #endregion

        #region Window GUI

        private void OnGUI()
        {
            GUILayout.Label("AgeVerification Scene Reference Assigner", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // Scene validation
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "AgeVerification")
            {
                EditorGUILayout.HelpBox(
                    $"Current scene: {currentScene}\n" +
                    "Please open the AgeVerification scene first!",
                    MessageType.Warning);
                GUILayout.Space(10);
            }

            EditorGUILayout.HelpBox(
                "Assigns UI references to AgeVerificationManager:\n" +
                "• Texts (title, description, status)\n" +
                "• Buttons (verify, back)\n" +
                "• Indicators (loading, success, error icons)",
                MessageType.Info);

            GUILayout.Space(10);

            // Check AgeVerificationManager exists
            MonoBehaviour targetManager = null;
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
            {
                if (mb.GetType().Name == "AgeVerificationManager")
                {
                    targetManager = mb;
                    break;
                }
            }

            if (targetManager == null)
            {
                EditorGUILayout.HelpBox(
                    "AgeVerificationManager not found in scene!\n" +
                    "Add an AgeVerificationManager component to assign references.",
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

            // Scan button
            GUI.backgroundColor = new Color(0.7f, 0.85f, 1f);
            if (GUILayout.Button("Scan Current References", GUILayout.Height(30)))
            {
                ResetLog();
                ScanCurrentReferences();
                Repaint();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(5);

            // Assign button
            GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
            if (GUILayout.Button("Auto-Assign All References", GUILayout.Height(40)))
            {
                ResetLog();
                AssignAllReferences();
                Repaint();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);

            // Results summary
            DrawResultsSummary();

            // Detailed log
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

            // Summary header
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

            // Individual results
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

                GUILayout.Label(result.fieldName, GUILayout.Width(150));
                GUILayout.Label(result.status, GUILayout.Width(120));

                if (result.assignedObject != null)
                {
                    EditorGUILayout.ObjectField(result.assignedObject, typeof(Object), true, GUILayout.Width(180));
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        #endregion

        #region Assignment Logic

        private static void ScanCurrentReferences()
        {
            Log("=== SCANNING AGE VERIFICATION REFERENCES ===");

            var manager = FindAgeVerificationManager();
            if (manager == null)
            {
                Log("ERROR: AgeVerificationManager not found in scene!");
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
            Log("=== ASSIGNING AGE VERIFICATION REFERENCES ===");

            var manager = FindAgeVerificationManager();
            if (manager == null)
            {
                Log("ERROR: AgeVerificationManager not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            SerializedObject so = new SerializedObject(manager);

            // Text elements
            AssignReference(so, "titleText", FindTextByName("title", "header", "agetitle"));
            AssignReference(so, "descriptionText", FindTextByName("description", "desc", "info", "message"));
            AssignReference(so, "statusText", FindTextByName("status", "state", "result"));

            // Buttons
            AssignReference(so, "verifyButton", FindButtonByName("verify", "confirm", "start", "check", "submit"));
            AssignReference(so, "backButton", FindButtonByName("back", "return", "cancel", "close"));

            // Indicators (only loadingIndicator - successIcon/errorIcon removed as optional)
            AssignReference(so, "loadingIndicator", FindByNameContains<Transform>("loading", "spinner", "wait", "progress"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);

            Log("=== ASSIGNMENT COMPLETE ===");
        }

        private static MonoBehaviour FindAgeVerificationManager()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
            {
                if (mb.GetType().Name == "AgeVerificationManager")
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
            Debug.Log($"[AgeVerificationReferenceAssigner] {message}");
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
