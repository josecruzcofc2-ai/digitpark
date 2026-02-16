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
            "timerText", "comboText",
            // Stats Bar
            "roundText", "errorsText", "progressFill", "roundIndicatorText",
            // Settings Panel
            "settingsPanel", "toggleRounds1", "toggleRounds3", "toggleRounds5", "toggleRounds10",
            "startGameButton",
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
                "- Timer, round, errors texts\n" +
                "- Settings panel, progress bar\n" +
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

            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            Transform root = canvas != null ? canvas.transform : controller.transform.root;

            // UI Elements
            AssignReference(so, "timerText", FindTextByDeep(root, "TimerText"));
            AssignReference(so, "comboText", FindTextByDeep(root, "ComboText"));

            // Stats Bar
            AssignReference(so, "roundText", FindTextByDeep(root, "RoundText"));
            AssignReference(so, "errorsText", FindTextByDeep(root, "ErrorsText"));
            AssignReference(so, "roundIndicatorText", FindTextByDeep(root, "RoundIndicator"));

            Transform progressFillT = FindDeep(root, "ProgressFill");
            AssignReference(so, "progressFill", progressFillT != null ? progressFillT.GetComponent<RectTransform>() : null);

            // Settings Panel
            Transform settingsPanelT = FindDeep(root, "SettingsPanel");
            if (settingsPanelT != null)
            {
                AssignReference(so, "settingsPanel", settingsPanelT.gameObject);
                AssignToggleReference(so, "toggleRounds1", FindDeep(settingsPanelT, "ToggleRounds1"));
                AssignToggleReference(so, "toggleRounds3", FindDeep(settingsPanelT, "ToggleRounds3"));
                AssignToggleReference(so, "toggleRounds5", FindDeep(settingsPanelT, "ToggleRounds5"));
                AssignToggleReference(so, "toggleRounds10", FindDeep(settingsPanelT, "ToggleRounds10"));

                Transform startBtn = FindDeep(settingsPanelT, "StartGameButton");
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

            // Win Message (original panel)
            Transform winMsgObj = FindDeep(root, "WinMessagePanel");
            if (winMsgObj != null)
            {
                AssignReference(so, "winMessagePanel", winMsgObj.gameObject);
                var wmCgProp = so.FindProperty("winMessageCanvasGroup");
                if (wmCgProp != null && wmCgProp.objectReferenceValue == null)
                    wmCgProp.objectReferenceValue = winMsgObj.GetComponent<CanvasGroup>();
            }
            else
            {
                AssignReference(so, "winMessagePanel", (Object)null);
            }
            AssignReference(so, "successText", FindTextByDeep(root, "SuccessText"));

            // Result Panel (Practice - new)
            Transform resultPanelT = FindDeep(root, "ResultPanel");
            if (resultPanelT != null)
            {
                AssignReference(so, "resultPanel", resultPanelT.gameObject);
                var cgProp = so.FindProperty("resultPanelCanvasGroup");
                if (cgProp != null)
                    cgProp.objectReferenceValue = resultPanelT.GetComponent<CanvasGroup>();
            }
            else
            {
                AssignReference(so, "resultPanel", (Object)null);
            }
            AssignReference(so, "resultTitleText", FindTextByDeep(root, "ResultTitleText"));
            AssignReference(so, "resultTimeText", FindTextByDeep(root, "ResultTimeText"));
            AssignReference(so, "resultMessageText", FindTextByDeep(root, "ResultMessageText"));

            Transform resultPlayAgainT = FindDeep(root, "ResultPlayAgainButton");
            AssignReference(so, "resultPlayAgainButton", resultPlayAgainT != null ? resultPlayAgainT.GetComponent<Button>() : null);

            Transform resultExitT = FindDeep(root, "ResultExitButton");
            AssignReference(so, "resultExitButton", resultExitT != null ? resultExitT.GetComponent<Button>() : null);

            // Win/Lose Panels (Real Money - legacy, kept for compatibility)
            AssignReference(so, "winPanelRealMoney", FindWinPanelController("WinPanel_RealMoney"));
            AssignReference(so, "losePanelRealMoney", FindWinPanelController("LosePanel_RealMoney"));

            // Countdown UI
            Transform countdownPanelT = FindDeep(root, "CountdownPanel");
            AssignReference(so, "countdownUI", countdownPanelT != null ? countdownPanelT.GetComponent<CountdownUI>() : null);

            // Sparkle Effect
            Transform particleEffectsT = FindDeep(root, "ParticleEffects");
            AssignReference(so, "sparkleEffect", particleEffectsT != null ? particleEffectsT.GetComponent<UISparkleEffect>() : null);

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

        private static void AssignToggleReference(SerializedObject so, string propertyName, Transform toggleTransform)
        {
            if (toggleTransform != null)
                AssignReference(so, propertyName, toggleTransform.GetComponent<Toggle>());
            else
                AssignReference(so, propertyName, (Object)null);
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
        private static void Log(string msg) { log += msg + "\n"; Debug.Log($"[DigitRushReferenceAssigner] {msg}"); }
        private static void AddResult(string f, string s, bool ok, Object o) { results.Add(new ReferenceResult { fieldName = f, status = s, success = ok, assignedObject = o }); }

        #endregion
    }
}
