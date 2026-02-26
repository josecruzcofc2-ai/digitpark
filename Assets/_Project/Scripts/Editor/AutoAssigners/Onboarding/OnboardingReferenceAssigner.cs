using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;

namespace DigitPark.Editor.AutoAssigners
{
    /// <summary>
    /// Reference Assigner for Onboarding scene (slide-based architecture).
    /// Automatically finds and assigns UI references to OnboardingManager.
    ///
    /// Menu: DigitPark/Auto Assigners/References/Onboarding/Onboarding References
    /// </summary>
    public class OnboardingReferenceAssigner : EditorWindow
    {
        private Vector2 scrollPosition;
        private static string log = "";
        private static int assignedCount = 0;
        private static int failedCount = 0;
        private static int alreadySetCount = 0;
        private static List<ReferenceResult> results = new List<ReferenceResult>();

        private static readonly string[] REQUIRED_REFS = {
            // Main UI
            "skipButton", "skipButtonText",
            // Slides Container
            "slidesContainer",
            // Navigation
            "nextButton", "prevButton", "nextButtonText", "prevButtonText", "dotsContainer",
            // Progress
            "progressBar", "stepCounterText",
            // Name Input (Slide 2)
            "nameInputPanel", "nameInput", "confirmNameButton", "nameErrorText",
            // Avatar Selection (Slide 3)
            "avatarSelectionPanel", "avatarContainer",
            // Tutorial Completion (Slide 8)
            "completionPanel", "completionTitleText", "completionMessageText",
            "rewardText", "startPlayingButton",
            // Sections (for animations)
            "progressBarTransform", "topBarTransform", "dotsTransform", "navigationTransform"
        };

        private struct ReferenceResult
        {
            public string fieldName;
            public string status;
            public bool success;
            public Object assignedObject;
        }

        #region Menu Items

        [MenuItem("DigitPark/Auto Assigners/References/Onboarding/Onboarding References", false, 170)]
        public static void ShowWindow()
        {
            var window = GetWindow<OnboardingReferenceAssigner>("Onboarding Reference Assigner");
            window.minSize = new Vector2(600, 550);
        }

        #endregion

        #region Window GUI

        private void OnGUI()
        {
            GUILayout.Label("Onboarding Scene Reference Assigner", EditorStyles.boldLabel);
            GUILayout.Space(10);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "Onboarding")
            {
                EditorGUILayout.HelpBox(
                    $"Current scene: {currentScene}\n" +
                    "Please open the Onboarding scene first!",
                    MessageType.Warning);
            }

            EditorGUILayout.HelpBox(
                "Assigns UI references to OnboardingManager (slide-based):\n" +
                "• Main (skip button)\n" +
                "• SlidesContainer (Slide1-Slide8)\n" +
                "• Navigation (next, prev, dots, progress)\n" +
                "• Name input (Slide2) and Avatar selection (Slide3)\n" +
                "• Completion panel (Slide8)",
                MessageType.Info);

            GUILayout.Space(10);

            MonoBehaviour targetManager = FindOnboardingManager();
            if (targetManager != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Target:", GUILayout.Width(50));
                EditorGUILayout.ObjectField(targetManager, typeof(MonoBehaviour), true);
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

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(350));
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
                GUILayout.Label(result.success ? (result.status == "Already Set" ? "=" : "+") : "X", GUILayout.Width(20));
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

        #region Assignment Logic

        private static void AssignAllReferences()
        {
            Log("=== ASSIGNING ONBOARDING REFERENCES (SLIDE-BASED) ===");

            var manager = FindOnboardingManager();
            if (manager == null)
            {
                Log("ERROR: OnboardingManager not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            so.Update();

            // Main UI
            AssignReference(so, "skipButton", FindButtonByName("skip", "saltar", "omitir"));
            AssignReference(so, "skipButtonText", FindTextInChild("skipbutton", "text"));

            // Slides Container
            AssignReference(so, "slidesContainer", FindByNameContains<Transform>("slidescontainer"));

            // Navigation
            AssignReference(so, "nextButton", FindButtonByName("next", "siguiente", "continue"));
            AssignReference(so, "prevButton", FindButtonByName("prev", "anterior", "back"));
            AssignReference(so, "nextButtonText", FindTextInChild("nextbutton", "text"));
            AssignReference(so, "prevButtonText", FindTextInChild("prevbutton", "text"));
            AssignReference(so, "dotsContainer", FindByNameContains<Transform>("dots", "indicators", "pagination", "progressdots"));

            // Progress
            AssignReference(so, "progressBar", FindByNameContains<Slider>("progressbar", "progress"));
            AssignReference(so, "stepCounterText", FindTextByName("stepcounter", "counter", "step"));

            // Name Input (Slide 2)
            AssignGameObject(so, "nameInputPanel", "nameinputpanel", "nameinput");
            AssignInputField(so, "nameInput", "nameinput", "name");
            AssignReference(so, "confirmNameButton", FindButtonByName("confirmname", "confirm", "confirmar"));
            AssignReference(so, "nameErrorText", FindTextByName("nameerror", "error"));

            // Avatar Selection (Slide 3)
            AssignGameObject(so, "avatarSelectionPanel", "avatarselection", "avatarpanel");
            AssignReference(so, "avatarContainer", FindByNameContains<Transform>("avatarcontainer", "avatargrid", "avatars"));

            // Tutorial Completion (Slide 8)
            AssignGameObject(so, "completionPanel", "completionpanel", "completion", "finish");
            AssignReference(so, "completionTitleText", FindTextByName("completiontitle", "congratulations"));
            AssignReference(so, "completionMessageText", FindTextByName("completionmessage", "completiondesc"));
            AssignReference(so, "rewardText", FindTextByName("rewardtext", "reward", "premio"));
            AssignReference(so, "startPlayingButton", FindButtonByName("startplaying", "start", "comenzar"));

            // Sections (for animations)
            AssignRectTransform(so, "progressBarTransform", "progressbar", "progress");
            AssignRectTransform(so, "topBarTransform", "topbar", "header");
            AssignRectTransform(so, "dotsTransform", "dotscontainer", "dots");
            AssignRectTransform(so, "navigationTransform", "navigationpanel", "navigation");

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(manager.gameObject);
            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
            Log("=== ASSIGNMENT COMPLETE ===");
        }

        private static MonoBehaviour FindOnboardingManager()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "OnboardingManager") return mb;
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

        private static TextMeshProUGUI FindTextInChild(string parentPattern, string childPattern)
        {
            var all = Object.FindObjectsOfType<Transform>(true);
            foreach (var t in all)
            {
                if (t.gameObject.name.ToLower().Contains(parentPattern.ToLower()))
                {
                    var texts = t.GetComponentsInChildren<TextMeshProUGUI>(true);
                    foreach (var txt in texts)
                        if (txt.gameObject.name.ToLower().Contains(childPattern.ToLower()))
                            return txt;
                }
            }
            return null;
        }

        private static void AssignGameObject(SerializedObject so, string propertyName, params string[] patterns)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { AddResult(propertyName, "Property not found", false, null); failedCount++; return; }
            if (prop.objectReferenceValue != null) { AddResult(propertyName, "Already Set", true, prop.objectReferenceValue); alreadySetCount++; return; }
            var all = Object.FindObjectsOfType<Transform>(true);
            foreach (var p in patterns)
                foreach (var o in all)
                    if (o.gameObject.name.ToLower().Contains(p.ToLower()))
                    {
                        prop.objectReferenceValue = o.gameObject;
                        AddResult(propertyName, "Assigned", true, o.gameObject);
                        assignedCount++;
                        return;
                    }
            AddResult(propertyName, "Not found", false, null); failedCount++;
        }

        private static void AssignInputField(SerializedObject so, string propertyName, params string[] patterns)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { AddResult(propertyName, "Property not found", false, null); failedCount++; return; }
            if (prop.objectReferenceValue != null) { AddResult(propertyName, "Already Set", true, prop.objectReferenceValue); alreadySetCount++; return; }
            var all = Object.FindObjectsOfType<TMP_InputField>(true);
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

        private static void AssignRectTransform(SerializedObject so, string propertyName, params string[] patterns)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { AddResult(propertyName, "Property not found", false, null); failedCount++; return; }
            if (prop.objectReferenceValue != null) { AddResult(propertyName, "Already Set", true, prop.objectReferenceValue); alreadySetCount++; return; }
            var all = Object.FindObjectsOfType<RectTransform>(true);
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

        #endregion

        #region Helpers

        private static void ResetLog() { log = ""; assignedCount = 0; failedCount = 0; alreadySetCount = 0; results.Clear(); }
        private static void Log(string msg) { log += msg + "\n"; Debug.Log($"[OnboardingReferenceAssigner] {msg}"); }
        private static void AddResult(string f, string s, bool ok, Object o) { results.Add(new ReferenceResult { fieldName = f, status = s, success = ok, assignedObject = o }); }

        #endregion
    }
}
