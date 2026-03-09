using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

namespace DigitPark.Editor.AutoAssigners
{
    /// <summary>
    /// Icon Assigner for Login scene.
    /// Assigns Google, Apple, and password toggle icons.
    ///
    /// Menu: DigitPark/Auto Assigners/Icons/Auth/Login Icons
    /// </summary>
    public class LoginIconAssigner : EditorWindow
    {
        private Vector2 scrollPosition;
        private static string log = "";
        private static int assignedCount = 0;
        private static int failedCount = 0;
        private static int skippedCount = 0;
        private static List<AssignmentResult> results = new List<AssignmentResult>();

        // Icon paths
        private const string ICONS_BASE = "Assets/_Project/Art/Icons/Auth";
        private const string GOOGLE_ICON = "Google/google_logo_official.png";
        private const string GOOGLE_DARK = "Google/google_icon_dark.png";
        private const string APPLE_ICON = "Apple/apple_logo_black.png";
        private const string EYE_OPEN_PATH = "Assets/_Project/Art/Icons/Navigation/EyeOpen.png";
        private const string EYE_CLOSED_PATH = "Assets/_Project/Art/Icons/Navigation/EyeClosed.png";

        private struct AssignmentResult
        {
            public string targetName;
            public string iconName;
            public bool success;
            public string message;
        }

        #region Menu Items

        [MenuItem("DigitPark/Scenes/Assign Icons/Auth/Login", false, 100)]
        public static void ShowWindow()
        {
            var window = GetWindow<LoginIconAssigner>("Login Icon Assigner");
            window.minSize = new Vector2(550, 500);
        }

        #endregion

        #region Window GUI

        private void OnGUI()
        {
            GUILayout.Label("Login Scene Icon Assigner", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // Scene validation
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "Login")
            {
                EditorGUILayout.HelpBox(
                    $"Current scene: {currentScene}\n" +
                    "Please open the Login scene first!",
                    MessageType.Warning);
                GUILayout.Space(10);
            }

            EditorGUILayout.HelpBox(
                "Assigns icons to Login scene UI elements:\n" +
                "• Google Sign-In button icon\n" +
                "• Apple Sign-In button icon\n" +
                "• Password visibility toggle icons (eye open/closed)",
                MessageType.Info);

            GUILayout.Space(10);

            // Icons preview
            GUILayout.Label("Available Icons:", EditorStyles.boldLabel);
            DrawIconPreview();

            GUILayout.Space(10);

            // Assign button
            GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
            if (GUILayout.Button("Assign Login Icons", GUILayout.Height(40)))
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
            DrawIconBox("Google", $"{ICONS_BASE}/{GOOGLE_ICON}");
            DrawIconBox("Apple", $"{ICONS_BASE}/{APPLE_ICON}");
            DrawIconBox("Eye Open", EYE_OPEN_PATH);
            DrawIconBox("Eye Closed", EYE_CLOSED_PATH);
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

                // Status icon
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
            Log("=== LOGIN ICON ASSIGNMENT ===");

            // Google button
            AssignIconToButton(
                new[] { "GoogleButton", "Google", "SignInWithGoogle", "GoogleSignIn" },
                $"{ICONS_BASE}/{GOOGLE_ICON}",
                "Google logo"
            );

            // Apple button
            AssignIconToButton(
                new[] { "AppleButton", "Apple", "SignInWithApple", "AppleSignIn" },
                $"{ICONS_BASE}/{APPLE_ICON}",
                "Apple logo"
            );

            // Password toggle icons
            AssignPasswordToggleIcons();

            Log("=== ASSIGNMENT COMPLETE ===");
        }

        private static void AssignIconToButton(string[] buttonPatterns, string iconPath, string description)
        {
            var buttons = Object.FindObjectsOfType<Button>(true);
            Button targetButton = null;

            // Find button by patterns
            foreach (var pattern in buttonPatterns)
            {
                foreach (var btn in buttons)
                {
                    if (btn.gameObject.name.ToLower().Contains(pattern.ToLower()))
                    {
                        targetButton = btn;
                        break;
                    }
                }
                if (targetButton != null) break;
            }

            if (targetButton == null)
            {
                // Also search in transforms
                var allTransforms = Object.FindObjectsOfType<Transform>(true);
                foreach (var pattern in buttonPatterns)
                {
                    foreach (var t in allTransforms)
                    {
                        if (t.name.ToLower().Contains(pattern.ToLower()))
                        {
                            targetButton = t.GetComponent<Button>() ?? t.GetComponentInChildren<Button>(true);
                            if (targetButton != null) break;
                        }
                    }
                    if (targetButton != null) break;
                }
            }

            if (targetButton == null)
            {
                AddResult(buttonPatterns[0], description, false, "Button not found");
                Log($"? Button not found: {string.Join(", ", buttonPatterns)}");
                failedCount++;
                return;
            }

            // Find icon image
            Image iconImage = FindIconImage(targetButton);

            if (iconImage == null)
            {
                AddResult(targetButton.name, description, false, "Icon Image child not found");
                Log($"? Icon Image not found in button: {targetButton.name}");
                failedCount++;
                return;
            }

            // Load and assign sprite
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            if (sprite != null)
            {
                iconImage.sprite = sprite;
                EditorUtility.SetDirty(iconImage);
                AddResult(targetButton.name, description, true, $"Assigned to {iconImage.name}");
                Log($"+ Assigned {description} to: {targetButton.name}/{iconImage.name}");
                assignedCount++;
            }
            else
            {
                AddResult(targetButton.name, description, false, $"Sprite not found: {iconPath}");
                Log($"! Sprite not found: {iconPath}");
                failedCount++;
            }
        }

        private static Image FindIconImage(Button button)
        {
            // Try finding child named Icon
            var iconTransform = button.transform.Find("Icon");
            if (iconTransform != null)
            {
                var img = iconTransform.GetComponent<Image>();
                if (img != null) return img;
            }

            // Try any Image child with icon/logo in name
            var images = button.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img.gameObject != button.gameObject)
                {
                    string name = img.gameObject.name.ToLower();
                    if (name.Contains("icon") || name.Contains("logo"))
                        return img;
                }
            }

            // Return first non-button Image
            foreach (var img in images)
            {
                if (img.gameObject != button.gameObject)
                    return img;
            }

            return null;
        }

        private static void AssignPasswordToggleIcons()
        {
            Log("Searching for password toggle icons...");

            string[] togglePatterns = {
                "passwordtoggle", "showpassword", "eyebutton", "togglepassword",
                "passwordeye", "eyeicon", "visibilitytoggle", "eye"
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
                    Sprite eyeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(EYE_OPEN_PATH);
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
                        Sprite openSprite = AssetDatabase.LoadAssetAtPath<Sprite>(EYE_OPEN_PATH);
                        if (openSprite != null)
                        {
                            eyeOpenProp.objectReferenceValue = openSprite;
                            AddResult("PasswordToggle", "eyeOpenSprite", true, "Assigned");
                            Log($"+ Assigned eyeOpenSprite to PasswordToggle");
                            assignedCount++;
                        }
                    }

                    if (eyeClosedProp != null)
                    {
                        Sprite closedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(EYE_CLOSED_PATH);
                        if (closedSprite != null)
                        {
                            eyeClosedProp.objectReferenceValue = closedSprite;
                            AddResult("PasswordToggle", "eyeClosedSprite", true, "Assigned");
                            Log($"+ Assigned eyeClosedSprite to PasswordToggle");
                            assignedCount++;
                        }
                    }

                    so.ApplyModifiedProperties();
                }
            }

            if (found == 0)
            {
                Log("? No password toggle images found in scene");
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
            Debug.Log($"[LoginIconAssigner] {message}");
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
