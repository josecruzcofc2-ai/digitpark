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
            "instructionText", "comboText",
            // Navigation (MinigameBase) - buttons inside winPanel
            "playAgainButton", "backButton",
            // Panels
            "winPanel", "winPanelCanvasGroup", "statsText",
            // Win/Lose Panels (Cash Battle)
            "winPanelRealMoney", "losePanelRealMoney"
            // MANUAL ASSIGNMENT REQUIRED:
            // - leftGridButtons[] (16 buttons)
            // - leftButtonTexts[] (16 TMP texts - for displaying digits/chars)
            // - rightGridButtons[] (16 buttons)
            // - rightButtonTexts[] (16 TMP texts - for displaying digits/chars)
            // Note: leftButtonImages[], rightButtonImages[] excluded - NOT USED (game uses text only)
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
                "- Timer, round, errors, instruction, combo texts\n" +
                "- Win panel\n\n" +
                "MANUAL ASSIGNMENT:\n" +
                "- leftGridButtons[16], leftButtonTexts[16]\n" +
                "- rightGridButtons[16], rightButtonTexts[16]\n" +
                "Game uses TEXT/DIGITS only, NOT images",
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

            // UI Elements
            AssignReference(so, "timerText", FindTextByName("timer", "time", "tiempo"));
            AssignReference(so, "roundText", FindTextByName("round", "ronda", "level"));
            AssignReference(so, "errorsText", FindTextByName("error", "mistakes", "wrong"));
            AssignReference(so, "instructionText", FindTextByName("instruction", "find", "difference", "encuentra"));
            AssignReference(so, "comboText", FindTextByName("combo", "streak"));

            // Navigation (MinigameBase) - buttons inside winPanel
            AssignReference(so, "playAgainButton", FindButtonByName("playagain"));
            AssignReference(so, "backButton", FindButtonByName("exit", "back", "salir"));

            // Panels
            AssignReference(so, "winPanel", FindByNameContains<Transform>("win", "result", "complete"));
            AssignCanvasGroup(so, "winPanelCanvasGroup", "winpanel", "win");
            AssignReference(so, "statsText", FindTextByName("stats", "estadisticas"));

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

        #endregion

        #region Finders

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
        private static void Log(string msg) { log += msg + "\n"; Debug.Log($"[OddOneOutReferenceAssigner] {msg}"); }
        private static void AddResult(string f, string s, bool ok, Object o) { results.Add(new ReferenceResult { fieldName = f, status = s, success = ok, assignedObject = o }); }

        #endregion
    }
}
