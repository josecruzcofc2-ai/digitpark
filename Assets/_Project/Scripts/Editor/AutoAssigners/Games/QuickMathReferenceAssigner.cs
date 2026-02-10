using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;
using DigitPark.UI;

namespace DigitPark.Editor.AutoAssigners
{
    /// <summary>
    /// Reference Assigner for QuickMath scene.
    /// Automatically finds and assigns UI references to QuickMathController.
    ///
    /// Menu: DigitPark/Auto Assigners/References/Games/QuickMath References
    /// </summary>
    public class QuickMathReferenceAssigner : EditorWindow
    {
        private Vector2 scrollPosition;
        private static string log = "";
        private static int assignedCount = 0;
        private static int failedCount = 0;
        private static int alreadySetCount = 0;
        private static List<ReferenceResult> results = new List<ReferenceResult>();

        private static readonly string[] REQUIRED_REFS = {
            // Equation Display
            "problemText", "numberAText", "numberBText", "operatorText", "questionMarkText", "equationPanel",
            // UI
            "timerText", "roundText", "errorsText", "comboText", "roundIndicatorText",
            // Settings Panel
            "settingsPanel", "toggleAddition", "toggleSubtraction", "toggleMultiplication", "toggleDivision",
            "toggleEasy", "toggleNormal", "toggleHard", "toggleRounds3", "toggleRounds5", "toggleRounds10",
            "startGameButton", "difficultyDescText",
            // Feedback
            "feedbackPanel", "feedbackText",
            // Panels & Effects
            "comboCanvasGroup", "progressFill",
            // Win/Lose Panels (Normal)
            "winPanelNormal", "losePanelNormal",
            // Win/Lose Panels (Cash Battle)
            "winPanelRealMoney", "losePanelRealMoney"
            // Note: answerButtons[], answerTexts[] - arrays require manual assignment
            // Note: sparkleEffect - auto-found in code
        };

        private struct ReferenceResult
        {
            public string fieldName;
            public string status;
            public bool success;
            public Object assignedObject;
        }

        #region Menu Items

        [MenuItem("DigitPark/Auto Assigners/References/Games/QuickMath References", false, 232)]
        public static void ShowWindow()
        {
            var window = GetWindow<QuickMathReferenceAssigner>("QuickMath Reference Assigner");
            window.minSize = new Vector2(600, 500);
        }

        #endregion

        #region Window GUI

        private void OnGUI()
        {
            GUILayout.Label("QuickMath Scene Reference Assigner", EditorStyles.boldLabel);
            GUILayout.Space(10);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "QuickMath")
            {
                EditorGUILayout.HelpBox(
                    $"Current scene: {currentScene}\n" +
                    "Please open the QuickMath scene first!",
                    MessageType.Warning);
            }

            EditorGUILayout.HelpBox(
                "Assigns UI references to QuickMathController:\n" +
                "- Equation display (problem text, panel)\n" +
                "- Timer, round, errors, combo texts\n" +
                "- Settings panel (toggles, start button)\n" +
                "- Feedback panel\n" +
                "- Win/Lose panels (Normal + Real Money)\n\n" +
                "Note: Answer buttons array requires manual assignment",
                MessageType.Info);

            GUILayout.Space(10);

            MonoBehaviour targetController = FindQuickMathController();
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
                GUILayout.Label(result.fieldName, GUILayout.Width(200));
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
            Log("=== ASSIGNING QUICKMATH REFERENCES ===");

            var controller = FindQuickMathController();
            if (controller == null)
            {
                Log("ERROR: QuickMathController not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            SerializedObject so = new SerializedObject(controller);
            so.Update();

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            Transform root = canvas != null ? canvas.transform : controller.transform.root;

            // Equation Display
            AssignReference(so, "problemText", FindDeepComponent<TextMeshProUGUI>(root, "ProblemText"));
            AssignReference(so, "numberAText", FindDeepComponent<TextMeshProUGUI>(root, "NumberA"));
            AssignReference(so, "numberBText", FindDeepComponent<TextMeshProUGUI>(root, "NumberB"));
            AssignReference(so, "operatorText", FindDeepComponent<TextMeshProUGUI>(root, "OperatorText"));
            AssignReference(so, "questionMarkText", FindTextByName("questionmark", "question"));
            AssignReference(so, "equationPanel", FindDeepComponent<RectTransform>(root, "EquationPanel"));

            // UI Elements
            AssignReference(so, "timerText", FindDeepComponent<TextMeshProUGUI>(root, "TimerText"));
            AssignReference(so, "roundText", FindDeepComponent<TextMeshProUGUI>(root, "RoundText"));
            AssignReference(so, "errorsText", FindDeepComponent<TextMeshProUGUI>(root, "ErrorsText"));
            AssignReference(so, "comboText", FindDeepComponent<TextMeshProUGUI>(root, "ComboText"));
            AssignReference(so, "roundIndicatorText", FindDeepComponent<TextMeshProUGUI>(root, "RoundIndicator"));

            // Settings Panel
            Transform settingsPanel = FindDeep(root, "SettingsPanel");
            AssignReference(so, "settingsPanel", settingsPanel != null ? settingsPanel.gameObject : null);
            AssignToggleReference(so, "toggleAddition", root, "ToggleAddition");
            AssignToggleReference(so, "toggleSubtraction", root, "ToggleSubtraction");
            AssignToggleReference(so, "toggleMultiplication", root, "ToggleMultiplication");
            AssignToggleReference(so, "toggleDivision", root, "ToggleDivision");
            AssignToggleReference(so, "toggleEasy", root, "ToggleEasy");
            AssignToggleReference(so, "toggleNormal", root, "ToggleNormal");
            AssignToggleReference(so, "toggleHard", root, "ToggleHard");
            AssignToggleReference(so, "toggleRounds3", root, "ToggleRounds3");
            AssignToggleReference(so, "toggleRounds5", root, "ToggleRounds5");
            AssignToggleReference(so, "toggleRounds10", root, "ToggleRounds10");
            AssignReference(so, "startGameButton", FindDeepComponent<Button>(root, "StartGameButton"));
            AssignReference(so, "difficultyDescText", FindDeepComponent<TextMeshProUGUI>(root, "DifficultyDescText"));

            // Feedback
            Transform feedbackPanel = FindDeep(root, "FeedbackPanel");
            AssignReference(so, "feedbackPanel", feedbackPanel != null ? feedbackPanel.gameObject : null);
            AssignReference(so, "feedbackText", FindDeepComponent<TextMeshProUGUI>(root, "FeedbackText"));

            // Panels & Effects
            AssignCanvasGroup(so, "comboCanvasGroup", "ComboContainer");
            AssignReference(so, "progressFill", FindDeepComponent<RectTransform>(root, "ProgressFill"));

            // Win/Lose Panels (Normal - base class fields)
            AssignReference(so, "winPanelNormal", FindWinPanelController("WinPanel_Normal"));
            AssignReference(so, "losePanelNormal", FindWinPanelController("LosePanel_Normal"));

            // Win/Lose Panels (Cash Battle - base class fields)
            AssignReference(so, "winPanelRealMoney", FindWinPanelController("WinPanel_RealMoney"));
            AssignReference(so, "losePanelRealMoney", FindWinPanelController("LosePanel_RealMoney"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(controller.gameObject);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            Log("=== ASSIGNMENT COMPLETE ===");
        }

        private static MonoBehaviour FindQuickMathController()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "QuickMathController") return mb;
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

        private static void AssignToggleReference(SerializedObject so, string propertyName, Transform root, string objectName)
        {
            Transform t = FindDeep(root, objectName);
            Toggle toggle = t != null ? t.GetComponent<Toggle>() : null;
            AssignReference(so, propertyName, toggle);
        }

        #endregion

        #region Finders

        private static Transform FindDeep(Transform root, string name)
        {
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                Transform result = FindDeep(child, name);
                if (result != null) return result;
            }
            return null;
        }

        private static T FindDeepComponent<T>(Transform root, string name) where T : Component
        {
            Transform t = FindDeep(root, name);
            return t != null ? t.GetComponent<T>() : null;
        }

        private static T FindByNameContains<T>(params string[] patterns) where T : Component
        {
            var all = Object.FindObjectsOfType<T>(true);
            foreach (var p in patterns) foreach (var o in all) if (o.gameObject.name.ToLower().Contains(p.ToLower())) return o;
            return null;
        }

        private static TextMeshProUGUI FindTextByName(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<TextMeshProUGUI>(true);
            foreach (var p in patterns) foreach (var t in all) if (t.gameObject.name.ToLower().Contains(p.ToLower())) return t;
            return null;
        }

        private static Button FindButtonByName(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<Button>(true);
            foreach (var p in patterns) foreach (var b in all) if (b.gameObject.name.ToLower().Contains(p.ToLower())) return b;
            return null;
        }

        private static void AssignCanvasGroup(SerializedObject so, string propertyName, params string[] patterns)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { AddResult(propertyName, "Property not found", false, null); failedCount++; return; }
            if (prop.objectReferenceValue != null) { AddResult(propertyName, "Already Set", true, prop.objectReferenceValue); alreadySetCount++; return; }
            var all = Object.FindObjectsOfType<CanvasGroup>(true);
            foreach (var p in patterns)
                foreach (var o in all)
                    if (o.gameObject.name.ToLower().Contains(p.ToLower()))
                    {
                        prop.objectReferenceValue = o;
                        AddResult(propertyName, "Assigned", true, o);
                        assignedCount++;
                        return;
                    }
            AddResult(propertyName, "Not found", false, null); failedCount++;
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
        private static void Log(string msg) { log += msg + "\n"; Debug.Log($"[QuickMathReferenceAssigner] {msg}"); }
        private static void AddResult(string f, string s, bool ok, Object o) { results.Add(new ReferenceResult { fieldName = f, status = s, success = ok, assignedObject = o }); }

        #endregion
    }
}
