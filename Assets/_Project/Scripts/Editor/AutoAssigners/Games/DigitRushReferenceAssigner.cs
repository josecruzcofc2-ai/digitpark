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
    /// Reference Assigner for DigitRush scene.
    /// Automatically finds and assigns UI references to DigitRushController.
    ///
    /// Menu: DigitPark/Auto Assigners/References/Games/DigitRush References
    /// </summary>
    public class DigitRushReferenceAssigner : EditorWindow
    {
        private Vector2 scrollPosition;
        private static string log = "";
        private static int assignedCount = 0;
        private static int failedCount = 0;
        private static int alreadySetCount = 0;
        private static List<ReferenceResult> results = new List<ReferenceResult>();

        private static readonly string[] REQUIRED_REFS = {
            // Grid
            "gridButtons",
            // UI
            "timerText", "bestTimeText", "comboText",
            // Countdown & Effects
            "countdownUI", "sparkleEffect",
            // Win Message (original)
            "winMessagePanel",
            // Result Panel (Practice - new)
            "resultPanel", "resultTitleText", "resultTimeText", "resultMessageText",
            "resultPlayAgainButton", "resultExitButton",
            // Win/Lose Panels (Cash Battle)
            "winPanelRealMoney", "losePanelRealMoney"
        };

        private struct ReferenceResult
        {
            public string fieldName;
            public string status;
            public bool success;
            public Object assignedObject;
        }

        #region Menu Items

        [MenuItem("DigitPark/Auto Assigners/References/Games/DigitRush References", false, 230)]
        public static void ShowWindow()
        {
            var window = GetWindow<DigitRushReferenceAssigner>("DigitRush Reference Assigner");
            window.minSize = new Vector2(600, 500);
        }

        #endregion

        #region Window GUI

        private void OnGUI()
        {
            GUILayout.Label("DigitRush Scene Reference Assigner", EditorStyles.boldLabel);
            GUILayout.Space(10);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "DigitRush")
            {
                EditorGUILayout.HelpBox(
                    $"Current scene: {currentScene}\n" +
                    "Please open the DigitRush scene first!",
                    MessageType.Warning);
            }

            EditorGUILayout.HelpBox(
                "Assigns UI references to DigitRushController:\n" +
                "- Grid buttons (9 cells)\n" +
                "- Timer and best time texts\n" +
                "- Combo display\n" +
                "- Countdown UI and sparkle effect\n" +
                "- Result panel and win/lose panels",
                MessageType.Info);

            GUILayout.Space(10);

            MonoBehaviour targetController = FindDigitRushController();
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
            Log("=== ASSIGNING DIGITRUSH REFERENCES ===");

            var controller = FindDigitRushController();
            if (controller == null)
            {
                Log("ERROR: DigitRushController not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            SerializedObject so = new SerializedObject(controller);
            so.Update();

            // Grid Buttons (9 cells inside GridContainer)
            AssignGridButtons(so);

            // UI Elements
            AssignReference(so, "timerText", FindTextByName("timer", "time", "tiempo"));
            AssignReference(so, "bestTimeText", FindTextByName("best", "mejor", "record"));
            AssignReference(so, "comboText", FindTextByName("combo", "streak"));

            // Win Message (original panel)
            AssignReference(so, "winMessagePanel", FindByNameContains<Transform>("winmessage", "winpanel"));
            var winMsgObj = FindByNameContains<Transform>("winmessage", "winpanel");
            if (winMsgObj != null)
            {
                var wmCgProp = so.FindProperty("winMessageCanvasGroup");
                if (wmCgProp != null && wmCgProp.objectReferenceValue == null)
                    wmCgProp.objectReferenceValue = winMsgObj.GetComponent<CanvasGroup>();
            }
            AssignReference(so, "successText", FindTextByName("success", "wintext", "mensaje"));

            // Result Panel (Practice - new)
            AssignReference(so, "resultPanel", FindByExactName<Transform>("ResultPanel"));
            AssignReference(so, "resultTitleText", FindTextByExactName("ResultTitleText"));
            AssignReference(so, "resultTimeText", FindTextByExactName("ResultTimeText"));
            AssignReference(so, "resultMessageText", FindTextByExactName("ResultMessageText"));
            AssignReference(so, "resultPlayAgainButton", FindButtonByName("resultplayagain"));
            AssignReference(so, "resultExitButton", FindButtonByName("resultexit"));

            // Result Panel CanvasGroup
            var resultPanelObj = FindByExactName<Transform>("ResultPanel");
            if (resultPanelObj != null)
            {
                var cgProp = so.FindProperty("resultPanelCanvasGroup");
                if (cgProp != null)
                    cgProp.objectReferenceValue = resultPanelObj.GetComponent<CanvasGroup>();
            }

            // Win/Lose Panels (Cash Battle)
            AssignReference(so, "winPanelRealMoney", FindWinPanelController("WinPanel_RealMoney"));
            AssignReference(so, "losePanelRealMoney", FindWinPanelController("LosePanel_RealMoney"));

            // Countdown UI
            var countdownUI = Object.FindFirstObjectByType<DigitPark.UI.CountdownUI>(FindObjectsInactive.Include);
            AssignReference(so, "countdownUI", countdownUI);

            // Sparkle Effect
            var sparkleEffect = Object.FindFirstObjectByType<DigitPark.UI.UISparkleEffect>(FindObjectsInactive.Include);
            AssignReference(so, "sparkleEffect", sparkleEffect);

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(controller.gameObject);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            Log("=== ASSIGNMENT COMPLETE ===");
        }

        private static MonoBehaviour FindDigitRushController()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "DigitRushController") return mb;
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

        private static void AssignGridButtons(SerializedObject so)
        {
            var prop = so.FindProperty("gridButtons");
            if (prop == null) { AddResult("gridButtons", "Property not found", false, null); failedCount++; return; }

            // Find GridContainer by searching all transforms
            Transform gridContainer = null;
            foreach (var t in Object.FindObjectsOfType<UnityEngine.UI.GridLayoutGroup>(true))
            {
                if (t.gameObject.name == "GridContainer")
                {
                    gridContainer = t.transform;
                    break;
                }
            }

            if (gridContainer == null)
            {
                AddResult("gridButtons", "GridContainer not found", false, null);
                failedCount++;
                return;
            }

            prop.arraySize = 9;
            int found = 0;
            for (int i = 0; i < 9; i++)
            {
                Transform cell = gridContainer.Find($"Cell_{i + 1}");
                if (cell != null)
                {
                    Button btn = cell.GetComponent<Button>();
                    if (btn != null)
                    {
                        prop.GetArrayElementAtIndex(i).objectReferenceValue = btn;
                        found++;
                    }
                }
            }

            if (found == 9)
            {
                AddResult("gridButtons", $"Assigned ({found}/9)", true, gridContainer);
                assignedCount++;
            }
            else
            {
                AddResult("gridButtons", $"Partial ({found}/9)", found > 0, gridContainer);
                if (found > 0) assignedCount++; else failedCount++;
            }
        }

        private static T FindByNameContains<T>(params string[] patterns) where T : Component
        {
            var all = Object.FindObjectsOfType<T>(true);
            foreach (var p in patterns) foreach (var o in all) if (o.gameObject.name.ToLower().Contains(p.ToLower())) return o;
            return null;
        }

        private static T FindByExactName<T>(string name) where T : Component
        {
            var all = Object.FindObjectsOfType<T>(true);
            foreach (var o in all) if (o.gameObject.name == name) return o;
            return null;
        }

        private static TextMeshProUGUI FindTextByName(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<TextMeshProUGUI>(true);
            foreach (var p in patterns) foreach (var t in all) if (t.gameObject.name.ToLower().Contains(p.ToLower())) return t;
            return null;
        }

        private static TextMeshProUGUI FindTextByExactName(string name)
        {
            var all = Object.FindObjectsOfType<TextMeshProUGUI>(true);
            foreach (var t in all) if (t.gameObject.name == name) return t;
            return null;
        }

        private static Button FindButtonByName(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<Button>(true);
            foreach (var p in patterns) foreach (var b in all) if (b.gameObject.name.ToLower().Contains(p.ToLower())) return b;
            return null;
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
        private static void Log(string msg) { log += msg + "\n"; Debug.Log($"[DigitRushReferenceAssigner] {msg}"); }
        private static void AddResult(string f, string s, bool ok, Object o) { results.Add(new ReferenceResult { fieldName = f, status = s, success = ok, assignedObject = o }); }

        #endregion
    }
}
