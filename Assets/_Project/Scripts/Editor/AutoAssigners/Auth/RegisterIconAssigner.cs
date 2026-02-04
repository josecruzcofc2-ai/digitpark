using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

namespace DigitPark.Editor.AutoAssigners
{
    /// <summary>
    /// Icon Assigner for Register scene.
    /// Assigns password toggle icons for password and confirm password fields.
    ///
    /// Menu: DigitPark/Auto Assigners/Icons/Auth/Register Icons
    /// </summary>
    public class RegisterIconAssigner : EditorWindow
    {
        private Vector2 scrollPosition;
        private static string log = "";
        private static int assignedCount = 0;
        private static int failedCount = 0;
        private static int skippedCount = 0;
        private static List<AssignmentResult> results = new List<AssignmentResult>();

        // Icon paths
        private const string ICONS_BASE = "Assets/_Project/Art/Icons/Auth";
        private const string EYE_OPEN = "Common/icons8-eye-96.png";
        private const string EYE_CLOSED = "Common/icons8-closed-eye-96.png";

        private struct AssignmentResult
        {
            public string targetName;
            public string iconName;
            public bool success;
            public string message;
        }

        #region Menu Items

        [MenuItem("DigitPark/Auto Assigners/Icons/Auth/Register Icons", false, 103)]
        public static void ShowWindow()
        {
            var window = GetWindow<RegisterIconAssigner>("Register Icon Assigner");
            window.minSize = new Vector2(550, 450);
        }

        #endregion

        #region Window GUI

        private void OnGUI()
        {
            GUILayout.Label("Register Scene Icon Assigner", EditorStyles.boldLabel);
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
                "Assigns icons to Register scene UI elements:\n" +
                "• Password visibility toggle icons (eye open/closed)\n" +
                "• Confirm password toggle icons",
                MessageType.Info);

            GUILayout.Space(10);

            // Icons preview
            GUILayout.Label("Available Icons:", EditorStyles.boldLabel);
            DrawIconPreview();

            GUILayout.Space(10);

            // Assign button
            GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
            if (GUILayout.Button("Assign Register Icons", GUILayout.Height(40)))
            {
                ResetLog();
                AssignAllIcons();
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

        private void DrawIconPreview()
        {
            EditorGUILayout.BeginHorizontal();
            DrawIconBox("Eye Open", $"{ICONS_BASE}/{EYE_OPEN}");
            DrawIconBox("Eye Closed", $"{ICONS_BASE}/{EYE_CLOSED}");
            EditorGUILayout.EndHorizontal();
        }

        private void DrawIconBox(string label, string path)
        {
            EditorGUILayout.BeginVertical("box", GUILayout.Width(80));
            GUILayout.Label(label, EditorStyles.miniLabel);

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
            {
                GUILayout.Box(sprite.texture, GUILayout.Width(48), GUILayout.Height(48));
            }
            else
            {
                GUI.color = Color.red;
                GUILayout.Box("?", GUILayout.Width(48), GUILayout.Height(48));
                GUI.color = Color.white;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawResultsSummary()
        {
            if (results.Count == 0) return;

            EditorGUILayout.BeginVertical("box");

            // Summary header with color
            Color summaryColor = failedCount == 0 ? new Color(0.2f, 0.8f, 0.2f) : new Color(1f, 0.6f, 0.2f);
            GUI.color = summaryColor;
            GUILayout.Label($"Results: {assignedCount} assigned | {failedCount} failed | {skippedCount} skipped", EditorStyles.boldLabel);
            GUI.color = Color.white;

            // Individual results
            foreach (var result in results)
            {
                EditorGUILayout.BeginHorizontal();

                if (result.success)
                {
                    GUI.color = Color.green;
                    GUILayout.Label("✓", GUILayout.Width(20));
                }
                else
                {
                    GUI.color = Color.red;
                    GUILayout.Label("✗", GUILayout.Width(20));
                }
                GUI.color = Color.white;

                GUILayout.Label($"{result.targetName}: {result.message}");
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        #endregion

        #region Assignment Logic

        private static void AssignAllIcons()
        {
            Log("=== REGISTER ICON ASSIGNMENT ===");

            // Password toggle icons (both password and confirm password)
            AssignPasswordToggleIcons();

            Log("=== ASSIGNMENT COMPLETE ===");
        }

        private static void AssignPasswordToggleIcons()
        {
            Log("Searching for password toggle icons...");

            string[] togglePatterns = {
                "passwordtoggle", "showpassword", "eyebutton", "togglepassword",
                "passwordeye", "eyeicon", "visibilitytoggle", "eye",
                "confirmpasswordtoggle", "confirmtoggle"
            };

            var allImages = Object.FindObjectsOfType<Image>(true);
            int found = 0;

            foreach (var image in allImages)
            {
                string objName = image.gameObject.name.ToLower();

                bool isToggle = false;
                foreach (var pattern in togglePatterns)
                {
                    if (objName.Contains(pattern))
                    {
                        isToggle = true;
                        break;
                    }
                }

                if (isToggle)
                {
                    Sprite eyeSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ICONS_BASE}/{EYE_OPEN}");
                    if (eyeSprite != null)
                    {
                        image.sprite = eyeSprite;
                        EditorUtility.SetDirty(image);
                        AddResult(image.gameObject.name, "Eye icon", true, "Assigned eye open icon");
                        Log($"+ Assigned eye icon to: {image.gameObject.name}");
                        assignedCount++;
                        found++;
                    }
                }
            }

            // Try PasswordToggle components
            var toggles = Object.FindObjectsOfType<MonoBehaviour>(true);
            foreach (var toggle in toggles)
            {
                if (toggle.GetType().Name == "PasswordToggle")
                {
                    SerializedObject so = new SerializedObject(toggle);

                    var eyeOpenProp = so.FindProperty("eyeOpenSprite");
                    var eyeClosedProp = so.FindProperty("eyeClosedSprite");

                    if (eyeOpenProp != null)
                    {
                        Sprite openSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ICONS_BASE}/{EYE_OPEN}");
                        if (openSprite != null)
                        {
                            eyeOpenProp.objectReferenceValue = openSprite;
                            AddResult($"PasswordToggle ({toggle.gameObject.name})", "eyeOpenSprite", true, "Assigned");
                            Log($"+ Assigned eyeOpenSprite to PasswordToggle on {toggle.gameObject.name}");
                            assignedCount++;
                        }
                    }

                    if (eyeClosedProp != null)
                    {
                        Sprite closedSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ICONS_BASE}/{EYE_CLOSED}");
                        if (closedSprite != null)
                        {
                            eyeClosedProp.objectReferenceValue = closedSprite;
                            AddResult($"PasswordToggle ({toggle.gameObject.name})", "eyeClosedSprite", true, "Assigned");
                            Log($"+ Assigned eyeClosedSprite to PasswordToggle on {toggle.gameObject.name}");
                            assignedCount++;
                        }
                    }

                    so.ApplyModifiedProperties();
                }
            }

            if (found == 0)
            {
                Log("? No password toggle images found in scene");
                AddResult("Password Toggles", "Eye icons", false, "No toggle images found");
                skippedCount++;
            }
        }

        #endregion

        #region Helpers

        private static void ResetLog()
        {
            log = "";
            assignedCount = 0;
            failedCount = 0;
            skippedCount = 0;
            results.Clear();
        }

        private static void Log(string message)
        {
            log += message + "\n";
            Debug.Log($"[RegisterIconAssigner] {message}");
        }

        private static void AddResult(string target, string icon, bool success, string message)
        {
            results.Add(new AssignmentResult
            {
                targetName = target,
                iconName = icon,
                success = success,
                message = message
            });
        }

        #endregion
    }
}
