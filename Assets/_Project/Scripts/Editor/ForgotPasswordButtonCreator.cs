using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI.Common;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor tool to create the Forgot Password button in the Login scene
    /// </summary>
    public class ForgotPasswordButtonCreator : EditorWindow
    {
        [MenuItem("DigitPark/UI/Create Forgot Password Button")]
        public static void CreateButton()
        {
            // Find the Canvas
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Error", "No Canvas found in the scene. Open the Login scene first.", "OK");
                return;
            }

            // Find LoginPanel
            GameObject loginPanel = GameObject.Find("LoginPanel");
            Transform parent = loginPanel != null ? loginPanel.transform : canvas.transform;

            // Create the button GameObject
            GameObject buttonObj = new GameObject("ForgotPasswordButton");
            buttonObj.transform.SetParent(parent, false);

            // Add RectTransform - centered
            RectTransform rt = buttonObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(400, 50);
            rt.anchoredPosition = Vector2.zero; // Center - user will move it

            // Add transparent Image (required for Button raycast)
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0, 0, 0, 0); // Fully transparent

            // Add Button component
            Button button = buttonObj.AddComponent<Button>();

            // Configure button colors for link-style (no visual change on hover except text)
            ColorBlock colors = button.colors;
            colors.normalColor = Color.clear;
            colors.highlightedColor = Color.clear;
            colors.pressedColor = Color.clear;
            colors.selectedColor = Color.clear;
            colors.disabledColor = Color.clear;
            button.colors = colors;

            // Create TextMeshPro child
            GameObject textObj = new GameObject("ForgotPasswordText");
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "Olvidaste tu contrasena?";
            tmp.fontSize = 24;
            tmp.color = new Color(0f, 0.9f, 1f, 1f); // Cyan neon
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Underline; // Link style underline

            // Add hover effect script
            ForgotPasswordHoverEffect hoverEffect = buttonObj.AddComponent<ForgotPasswordHoverEffect>();

            // Try to assign to LoginManager
            var loginManager = FindFirstObjectByType<DigitPark.Managers.LoginManager>();
            if (loginManager != null)
            {
                // Use SerializedObject to properly set the reference
                SerializedObject serializedManager = new SerializedObject(loginManager);
                SerializedProperty forgotPasswordProp = serializedManager.FindProperty("forgotPasswordButton");
                if (forgotPasswordProp != null)
                {
                    forgotPasswordProp.objectReferenceValue = button;
                    serializedManager.ApplyModifiedProperties();
                    Debug.Log("[ForgotPasswordButtonCreator] Button assigned to LoginManager.forgotPasswordButton");
                }
            }

            // Select the new button
            Selection.activeGameObject = buttonObj;

            // Mark scene as dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log("[ForgotPasswordButtonCreator] Forgot Password button created successfully!");
            EditorUtility.DisplayDialog("Success",
                "Forgot Password button created!\n\n" +
                "The button is centered in the scene.\n" +
                "Move it to the desired position (below Password field).\n\n" +
                "It has been automatically assigned to LoginManager.",
                "OK");
        }
    }
}
