using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

namespace DigitPark.Editor.AutoAssigners
{
    /// <summary>
    /// Icon Assigner for AgeVerification scene.
    /// Assigns 18+ verification icon and related UI elements.
    ///
    /// Menu: DigitPark/Auto Assigners/Icons/Auth/AgeVerification Icons
    /// </summary>
    public class AgeVerificationIconAssigner : EditorWindow
    {
        private Vector2 scrollPosition;
        private static string log = "";
        private static int assignedCount = 0;
        private static int failedCount = 0;
        private static int skippedCount = 0;
        private static List<AssignmentResult> results = new List<AssignmentResult>();

        // Icon paths
        private const string ICONS_BASE = "Assets/_Project/Art/Icons/Auth";
        private const string AGE_18_ICON = "Common/18plus_icon.png";
        private const string AGE_VERIFICATION_ICON = "Common/age_verification_icon.png";
        private const string WARNING_ICON = "Assets/_Project/Art/Icons/UI/warning.png";
        private const string CHECKMARK_ICON = "Assets/_Project/Art/Icons/UI/checkmark.png";

        private struct AssignmentResult
        {
            public string targetName;
            public string iconName;
            public bool success;
            public string message;
        }

        #region Menu Items

        [MenuItem("DigitPark/Scenes/Assign Icons/Auth/AgeVerification", false, 102)]
        public static void ShowWindow()
        {
            var window = GetWindow<AgeVerificationIconAssigner>("AgeVerification Icon Assigner");
            window.minSize = new Vector2(550, 450);
        }

        #endregion

        #region Window GUI

        private void OnGUI()
        {
            GUILayout.Label("AgeVerification Scene Icon Assigner", EditorStyles.boldLabel);
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
                "Assigns icons to AgeVerification scene UI elements:\n" +
                "• 18+ Age verification icon\n" +
                "• Warning/confirmation icons\n" +
                "• Checkbox/checkmark icons",
                MessageType.Info);

            GUILayout.Space(10);

            // Icons preview
            GUILayout.Label("Available Icons:", EditorStyles.boldLabel);
            DrawIconPreview();

            GUILayout.Space(10);

            // Assign button
            GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
            if (GUILayout.Button("Assign AgeVerification Icons", GUILayout.Height(40)))
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
            DrawIconBox("18+ Icon", $"{ICONS_BASE}/{AGE_18_ICON}");
            DrawIconBox("Warning", WARNING_ICON);
            DrawIconBox("Checkmark", CHECKMARK_ICON);
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
                GUI.color = Color.yellow;
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
            Log("=== AGE VERIFICATION ICON ASSIGNMENT ===");

            // Main 18+ icon
            AssignAgeIcon();

            // Warning icons
            AssignWarningIcons();

            // Checkmark icons
            AssignCheckmarkIcons();

            Log("=== ASSIGNMENT COMPLETE ===");
        }

        private static void AssignAgeIcon()
        {
            Log("Searching for age verification icon...");

            string[] ageIconPatterns = {
                "ageicon", "icon18", "18plus", "verificationicon",
                "ageverifyicon", "ageimage", "mainicon"
            };

            var allImages = Object.FindObjectsOfType<Image>(true);
            bool found = false;

            foreach (var image in allImages)
            {
                string objName = image.gameObject.name.ToLower();

                bool isAgeIcon = false;
                foreach (var pattern in ageIconPatterns)
                {
                    if (objName.Contains(pattern))
                    {
                        isAgeIcon = true;
                        break;
                    }
                }

                if (isAgeIcon)
                {
                    // Try multiple icon paths
                    string[] possiblePaths = {
                        $"{ICONS_BASE}/{AGE_18_ICON}",
                        $"{ICONS_BASE}/{AGE_VERIFICATION_ICON}",
                        WARNING_ICON
                    };

                    foreach (var path in possiblePaths)
                    {
                        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                        if (sprite != null)
                        {
                            image.sprite = sprite;
                            EditorUtility.SetDirty(image);
                            AddResult(image.gameObject.name, "18+ icon", true, $"Assigned from {System.IO.Path.GetFileName(path)}");
                            Log($"+ Assigned 18+ icon to: {image.gameObject.name}");
                            assignedCount++;
                            found = true;
                            break;
                        }
                    }

                    if (found) break;
                }
            }

            if (!found)
            {
                Log("? No age verification icon found in scene");
                AddResult("Age Icon", "18+ icon", false, "No matching Image found");
                skippedCount++;
            }
        }

        private static void AssignWarningIcons()
        {
            Log("Searching for warning icons...");

            string[] warningPatterns = { "warning", "alert", "caution" };

            var allImages = Object.FindObjectsOfType<Image>(true);

            foreach (var image in allImages)
            {
                string objName = image.gameObject.name.ToLower();

                foreach (var pattern in warningPatterns)
                {
                    if (objName.Contains(pattern))
                    {
                        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(WARNING_ICON);
                        if (sprite != null)
                        {
                            image.sprite = sprite;
                            EditorUtility.SetDirty(image);
                            AddResult(image.gameObject.name, "Warning icon", true, "Assigned");
                            Log($"+ Assigned warning icon to: {image.gameObject.name}");
                            assignedCount++;
                        }
                        break;
                    }
                }
            }
        }

        private static void AssignCheckmarkIcons()
        {
            Log("Searching for checkmark icons...");

            string[] checkPatterns = { "checkmark", "check", "tick", "confirm" };

            var allImages = Object.FindObjectsOfType<Image>(true);

            foreach (var image in allImages)
            {
                string objName = image.gameObject.name.ToLower();

                // Skip buttons (they use confirm differently)
                if (objName.Contains("button")) continue;

                foreach (var pattern in checkPatterns)
                {
                    if (objName.Contains(pattern))
                    {
                        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CHECKMARK_ICON);
                        if (sprite != null)
                        {
                            image.sprite = sprite;
                            EditorUtility.SetDirty(image);
                            AddResult(image.gameObject.name, "Checkmark icon", true, "Assigned");
                            Log($"+ Assigned checkmark icon to: {image.gameObject.name}");
                            assignedCount++;
                        }
                        break;
                    }
                }
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
            Debug.Log($"[AgeVerificationIconAssigner] {message}");
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
