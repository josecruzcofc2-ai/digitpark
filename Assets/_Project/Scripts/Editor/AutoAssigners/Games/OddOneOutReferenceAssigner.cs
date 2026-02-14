using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;
using DigitPark.Editor;
using DigitPark.UI;

namespace DigitPark.Editor.AutoAssigners
{
    /// <summary>
    /// Reference Assigner for OddOneOut scene.
    /// Automatically finds and assigns UI references to OddOneOutController.
    ///
    /// Menu: DigitPark/Auto Assigners/References/Games/OddOneOut References
    /// </summary>
    public class OddOneOutReferenceAssigner : EditorWindow
    {
        private Vector2 scrollPosition;
        private static string log = "";
        private static int assignedCount = 0;
        private static int failedCount = 0;
        private static int alreadySetCount = 0;
        private static List<ReferenceResult> results = new List<ReferenceResult>();

        private static readonly string[] REQUIRED_REFS = {
            // UI
            "timerText", "roundText", "errorsText",
            "comboText", "roundIndicatorText", "progressFill",
            // Countdown
            "countdownUI",
            // Settings Panel
            "settingsPanel", "toggleRounds1", "toggleRounds3", "toggleRounds5", "toggleRounds10",
            "startGameButton",
            // Feedback
            "feedbackPanel", "feedbackText",
            // Win/Lose Panels (Normal)
            "winPanelNormal", "losePanelNormal",
            // Win/Lose Panels (Cash Battle)
            "winPanelRealMoney", "losePanelRealMoney"
            // MANUAL ASSIGNMENT REQUIRED:
            // - leftGridButtons[] (16 buttons)
            // - leftButtonTexts[] (16 TMP texts)
            // - rightGridButtons[] (16 buttons)
            // - rightButtonTexts[] (16 TMP texts)
            // Note: sparkleEffect excluded - auto-found in code
        };

        private struct ReferenceResult
        {
            public string fieldName;
            public string status;
            public bool success;
            public Object assignedObject;
        }

        #region Menu Items

        [MenuItem("DigitPark/Auto Assigners/References/Games/OddOneOut References", false, 234)]
        public static void ShowWindow()
        {
            var window = GetWindow<OddOneOutReferenceAssigner>("OddOneOut Reference Assigner");
            window.minSize = new Vector2(600, 500);
        }

        #endregion

        #region Window GUI

        private void OnGUI()
        {
            GUILayout.Label("OddOneOut Scene Reference Assigner", EditorStyles.boldLabel);
            GUILayout.Space(10);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "OddOneOut")
            {
                EditorGUILayout.HelpBox(
                    $"Current scene: {currentScene}\n" +
                    "Please open the OddOneOut scene first!",
                    MessageType.Warning);
            }

            EditorGUILayout.HelpBox(
                "Assigns UI references to OddOneOutController:\n" +
                "- Timer, round, errors, combo texts\n" +
                "- Countdown, settings panel, feedback\n" +
                "- Win/Lose panels (Normal + RealMoney)\n\n" +
                "MANUAL ASSIGNMENT:\n" +
                "- leftGridButtons[16], leftButtonTexts[16]\n" +
                "- rightGridButtons[16], rightButtonTexts[16]",
                MessageType.Info);

            GUILayout.Space(10);

            MonoBehaviour targetController = FindOddOneOutController();
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
        /// Ejecuta la asignación de referencias. Llamable desde otros Editor scripts.
        /// </summary>
        public static void RunAutoAssign()
        {
            ResetLog();
            AssignAllReferences();
        }

        #region Assignment Logic

        private static void AssignAllReferences()
        {
            Log("=== ASSIGNING ODDONEOUT REFERENCES ===");

            var controller = FindOddOneOutController();
            if (controller == null)
            {
                Log("ERROR: OddOneOutController not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            SerializedObject so = new SerializedObject(controller);
            so.Update();

            // Find root for deep search
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            Transform root = canvas != null ? canvas.transform : controller.transform.root;

            // UI Elements
            AssignReference(so, "timerText", FindTextByDeep(root, "TimerText"));
            AssignReference(so, "roundText", FindTextByDeep(root, "RoundText"));
            AssignReference(so, "errorsText", FindTextByDeep(root, "ErrorsText"));
            AssignReference(so, "comboText", FindTextByDeep(root, "ComboText"));
            AssignReference(so, "roundIndicatorText", FindTextByDeep(root, "RoundIndicator"));

            // Progress Fill
            Transform progressFillT = FindDeep(root, "ProgressFill");
            AssignReference(so, "progressFill", progressFillT != null ? progressFillT.GetComponent<RectTransform>() : null);

            // Countdown
            Transform countdownPanel = FindDeep(root, "CountdownPanel");
            if (countdownPanel != null)
                AssignReference(so, "countdownUI", countdownPanel.GetComponent<CountdownUI>());
            else
                AssignReference(so, "countdownUI", (Object)null);

            // Settings Panel
            Transform settingsPanel = FindDeep(root, "SettingsPanel");
            if (settingsPanel != null)
            {
                AssignReference(so, "settingsPanel", settingsPanel.gameObject);
                AssignToggleReference(so, "toggleRounds1", FindDeep(settingsPanel, "ToggleRounds1"));
                AssignToggleReference(so, "toggleRounds3", FindDeep(settingsPanel, "ToggleRounds3"));
                AssignToggleReference(so, "toggleRounds5", FindDeep(settingsPanel, "ToggleRounds5"));
                AssignToggleReference(so, "toggleRounds10", FindDeep(settingsPanel, "ToggleRounds10"));

                Transform startBtn = FindDeep(settingsPanel, "StartGameButton");
                AssignReference(so, "startGameButton", startBtn != null ? startBtn.GetComponent<Button>() : null);
            }
            else
            {
                AssignReference(so, "settingsPanel", (Object)null);
                AssignReference(so, "toggleRounds1", (Object)null);
                AssignReference(so, "toggleRounds3", (Object)null);
                AssignReference(so, "toggleRounds5", (Object)null);
                AssignReference(so, "toggleRounds10", (Object)null);
                AssignReference(so, "startGameButton", (Object)null);
            }

            // Feedback
            Transform feedbackPanel = FindDeep(root, "FeedbackPanel");
            if (feedbackPanel != null)
            {
                AssignReference(so, "feedbackPanel", feedbackPanel.gameObject);
                Transform feedbackText = FindDeep(feedbackPanel, "FeedbackText");
                AssignReference(so, "feedbackText", feedbackText != null ? feedbackText.GetComponent<TextMeshProUGUI>() : null);
            }
            else
            {
                AssignReference(so, "feedbackPanel", (Object)null);
                AssignReference(so, "feedbackText", (Object)null);
            }

            // Win/Lose Panels (Normal)
            Transform winNormal = FindDeep(root, "WinPanel_Normal");
            AssignReference(so, "winPanelNormal", winNormal != null ? winNormal.GetComponent<WinPanelController>() : null);

            Transform loseNormal = FindDeep(root, "LosePanel_Normal");
            AssignReference(so, "losePanelNormal", loseNormal != null ? loseNormal.GetComponent<WinPanelController>() : null);

            // Win/Lose Panels (Cash Battle)
            AssignReference(so, "winPanelRealMoney", FindWinPanelController("WinPanel_RealMoney"));
            AssignReference(so, "losePanelRealMoney", FindWinPanelController("LosePanel_RealMoney"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(controller.gameObject);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            Log("=== ASSIGNMENT COMPLETE ===");
        }

        private static MonoBehaviour FindOddOneOutController()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "OddOneOutController") return mb;
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

        private static void AssignToggleReference(SerializedObject so, string propertyName, Transform toggleTransform)
        {
            if (toggleTransform != null)
                AssignReference(so, propertyName, toggleTransform.GetComponent<Toggle>());
            else
                AssignReference(so, propertyName, (Object)null);
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

        private static WinPanelController FindWinPanelController(string name)
        {
            var all = Object.FindObjectsOfType<WinPanelController>(true);
            foreach (var w in all) if (w.gameObject.name == name) return w;
            return null;
        }

        #endregion

        #region Helpers

        private static void ResetLog() { log = ""; assignedCount = 0; failedCount = 0; alreadySetCount = 0; results.Clear(); }
        private static void Log(string msg) { log += msg + "\n"; Debug.Log($"[OddOneOutReferenceAssigner] {msg}"); }
        private static void AddResult(string f, string s, bool ok, Object o) { results.Add(new ReferenceResult { fieldName = f, status = s, success = ok, assignedObject = o }); }

        #endregion
    }
}
