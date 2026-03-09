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
    /// Reference Assigner for Register scene.
    /// Automatically finds and assigns UI references to RegisterManager.
    ///
    /// Menu: DigitPark/Auto Assigners/References/Auth/Register References
    /// </summary>
    public class RegisterReferenceAssigner : EditorWindow
    {
        private Vector2 scrollPosition;
        private static string log = "";
        private static int assignedCount = 0;
        private static int failedCount = 0;
        private static int alreadySetCount = 0;
        private static List<ReferenceResult> results = new List<ReferenceResult>();

        // Expected references for RegisterManager
        private static readonly string[] REQUIRED_REFS = {
            "titleText", "usernameInput", "emailInput", "passwordInput",
            "confirmPasswordInput", "createAccountButton", "backButton",
            "loadingPanel", "errorPanel"
        };

        private struct ReferenceResult
        {
            public string fieldName;
            public string status;
            public bool success;
            public Object assignedObject;
        }

        #region Menu Items

        [MenuItem("DigitPark/Scenes/Assign References/Auth/Register", false, 101)]
        public static void ShowWindow()
        {
            var window = GetWindow<RegisterReferenceAssigner>("Register Reference Assigner");
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
            GUILayout.Label("Register Scene Reference Assigner", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // Scene validation
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "Register")
            {
                EditorGUILayout.HelpBox(
                    $"Current scene: {currentScene}\n" +
                    "Please open the Register scene first!",
                    MessageType.Warning);
                GUILayout.Space(10);
            }

            EditorGUILayout.HelpBox(
                "Assigns UI references to RegisterManager:\n" +
                "• Input fields (username, email, password, confirmPassword)\n" +
                "• Buttons (createAccount, back)\n" +
                "• Panels (loading, error)\n" +
                "• Other (title text)",
                MessageType.Info);

            GUILayout.Space(10);

            // Check RegisterManager exists
            MonoBehaviour targetManager = null;
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
            {
                if (mb.GetType().Name == "RegisterManager")
                {
                    targetManager = mb;
                    break;
                }
            }

            if (targetManager == null)
            {
                EditorGUILayout.HelpBox(
                    "RegisterManager not found in scene!\n" +
                    "Add a RegisterManager component to assign references.",
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
            Log("=== SCANNING REGISTER REFERENCES ===");

            var manager = FindRegisterManager();
            if (manager == null)
            {
                Log("ERROR: RegisterManager not found in scene!");
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
            Log("=== ASSIGNING REGISTER REFERENCES ===");

            var manager = FindRegisterManager();
            if (manager == null)
            {
                Log("ERROR: RegisterManager not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            so.Update();

            // Title Text
            AssignReference(so, "titleText", FindByNameContains<TextMeshProUGUI>("title", "header"));

            // Input Fields
            AssignReference(so, "usernameInput", FindInputByName("username", "user", "name"));
            AssignReference(so, "emailInput", FindInputByName("email"));
            AssignReference(so, "passwordInput", FindPasswordInput(false));
            AssignReference(so, "confirmPasswordInput", FindPasswordInput(true));

            // Buttons
            AssignReference(so, "createAccountButton", FindButtonByName("create", "register", "signup", "submit"));
            AssignReference(so, "backButton", FindButtonByName("back", "return", "cancel", "close"));

            // Panels
            AssignReference(so, "loadingPanel", FindByNameContains<Transform>("loading", "spinner", "wait"));
            AssignReference(so, "errorPanel", FindByType("ErrorPanelUI"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(manager.gameObject);
            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);

            Log("=== ASSIGNMENT COMPLETE ===");
        }

        private static MonoBehaviour FindRegisterManager()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
            {
                if (mb.GetType().Name == "RegisterManager")
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

        private static TMP_InputField FindInputByName(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<TMP_InputField>(true);
            foreach (var pattern in patterns)
            {
                foreach (var input in all)
                {
                    if (input.gameObject.name.ToLower().Contains(pattern.ToLower()))
                        return input;
                }
            }
            return null;
        }

        private static TMP_InputField FindPasswordInput(bool isConfirm)
        {
            var all = Object.FindObjectsOfType<TMP_InputField>(true);
            foreach (var input in all)
            {
                string name = input.gameObject.name.ToLower();
                bool hasPassword = name.Contains("password") || name.Contains("pass");
                bool hasConfirm = name.Contains("confirm") || name.Contains("repeat") || name.Contains("verify");

                if (isConfirm)
                {
                    if (hasPassword && hasConfirm) return input;
                }
                else
                {
                    if (hasPassword && !hasConfirm) return input;
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

        private static Object FindByType(string typeName)
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
            {
                if (mb.GetType().Name == typeName)
                    return mb;
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
            Debug.Log($"[RegisterReferenceAssigner] {message}");
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
