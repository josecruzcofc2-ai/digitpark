using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Reflection;
using DigitPark.Editor;

namespace DigitPark.Editor.AutoAssigners
{
    /// <summary>
    /// Reference Assigner for Notifications scene.
    /// Automatically finds and assigns UI references to NotificationsManager.
    /// Covers all 20 serialized fields: header, tabs, content, footer, animations.
    ///
    /// Menu: DigitPark/Auto Assigners/References/Social/Notifications References
    /// </summary>
    public class NotificationsReferenceAssigner : EditorWindow
    {
        private Vector2 scrollPosition;
        private static string log = "";
        private static int assignedCount = 0;
        private static int failedCount = 0;
        private static int alreadySetCount = 0;
        private static List<ReferenceResult> results = new List<ReferenceResult>();

        private static readonly string[] REQUIRED_REFS = {
            // Header
            "backButton", "titleText", "countText",
            // Tabs
            "tabAll", "tabSocial", "tabGames", "tabRewards",
            "tabAllIndicator", "tabSocialIndicator", "tabGamesIndicator", "tabRewardsIndicator",
            // Content
            "scrollContent", "notificationCardPrefab", "emptyText", "loadingIndicator",
            // Footer
            "markAllReadButton", "markAllReadText",
            // Animations
            "headerTransform", "tabsTransform", "scrollViewTransform", "footerTransform"
        };

        private struct ReferenceResult
        {
            public string fieldName;
            public string status;
            public bool success;
            public Object assignedObject;
        }

        #region Menu Items

        [MenuItem("DigitPark/Auto Assigners/References/Social/Notifications References", false, 232)]
        public static void ShowWindow()
        {
            var window = GetWindow<NotificationsReferenceAssigner>("Notifications Reference Assigner");
            window.minSize = new Vector2(600, 550);
        }

        #endregion

        #region Window GUI

        private void OnGUI()
        {
            GUILayout.Label("Notifications Scene Reference Assigner", EditorStyles.boldLabel);
            GUILayout.Space(10);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "Notifications")
            {
                EditorGUILayout.HelpBox(
                    $"Current scene: {currentScene}\n" +
                    "Please open the Notifications scene first!",
                    MessageType.Warning);
                GUILayout.Space(10);
            }

            EditorGUILayout.HelpBox(
                "Assigns UI references to NotificationsManager:\n" +
                "• Header (back, title, count)\n" +
                "• Tabs (all, social, games, rewards + indicators)\n" +
                "• Content (scroll, prefab, empty, loading)\n" +
                "• Footer (mark all read)\n" +
                "• Animation sections (header, tabs, scroll, footer)",
                MessageType.Info);

            GUILayout.Space(10);

            MonoBehaviour targetManager = FindNotificationsManager();

            if (targetManager == null)
            {
                EditorGUILayout.HelpBox(
                    "NotificationsManager not found in scene!\n" +
                    "Add a NotificationsManager component to assign references.",
                    MessageType.Error);
                GUILayout.Space(10);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Target:", GUILayout.Width(50));
                EditorGUILayout.ObjectField(targetManager, typeof(MonoBehaviour), true);
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(10);

            GUI.backgroundColor = new Color(0.7f, 0.85f, 1f);
            if (GUILayout.Button("Scan Current References", GUILayout.Height(30)))
            {
                ResetLog();
                ScanCurrentReferences();
                Repaint();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(5);

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
            GUILayout.Label(successRate == 1f ? "\u2713 ALL REFERENCES SET" : "\u26A0 Some references missing", EditorStyles.boldLabel);
            GUI.color = Color.white;

            GUILayout.Label($"Assigned: {assignedCount} | Already Set: {alreadySetCount} | Failed: {failedCount}");

            foreach (var result in results)
            {
                EditorGUILayout.BeginHorizontal();
                GUI.color = result.success ? (result.status == "Already Set" ? new Color(0.5f, 0.8f, 1f) : Color.green) : Color.red;
                GUILayout.Label(result.success ? (result.status == "Already Set" ? "\u25CF" : "\u2713") : "\u2717", GUILayout.Width(20));
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

        private static void ScanCurrentReferences()
        {
            Log("=== SCANNING NOTIFICATIONS REFERENCES ===");

            var manager = FindNotificationsManager();
            if (manager == null)
            {
                Log("ERROR: NotificationsManager not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            foreach (var fieldName in REQUIRED_REFS)
            {
                var field = GetField(manager, fieldName);
                if (field != null)
                {
                    var value = field.GetValue(manager);
                    if (value != null && !(value is Object obj && obj == null))
                    {
                        AddResult(fieldName, "Already Set", true, value as Object);
                        alreadySetCount++;
                    }
                    else
                    {
                        AddResult(fieldName, "Not Set", false, null);
                        failedCount++;
                    }
                }
                else
                {
                    AddResult(fieldName, "Field not found", false, null);
                    failedCount++;
                }
            }

            Log("=== SCAN COMPLETE ===");
        }

        private static void AssignAllReferences()
        {
            Log("=== ASSIGNING NOTIFICATIONS REFERENCES ===");

            var manager = FindNotificationsManager();
            if (manager == null)
            {
                Log("ERROR: NotificationsManager not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            so.Update();

            // Header
            AssignReference(so, "backButton", FindButtonByName("backbutton"));
            AssignReference(so, "titleText", FindTextByName("titletext"));
            AssignReference(so, "countText", FindTextByName("counttext"));

            // Tabs
            AssignReference(so, "tabAll", FindButtonByName("taball"));
            AssignReference(so, "tabSocial", FindButtonByName("tabsocial"));
            AssignReference(so, "tabGames", FindButtonByName("tabgames"));
            AssignReference(so, "tabRewards", FindButtonByName("tabrewards"));
            AssignReference(so, "tabAllIndicator", FindImageByPath("Tabs/TabAll/Indicator"));
            AssignReference(so, "tabSocialIndicator", FindImageByPath("Tabs/TabSocial/Indicator"));
            AssignReference(so, "tabGamesIndicator", FindImageByPath("Tabs/TabGames/Indicator"));
            AssignReference(so, "tabRewardsIndicator", FindImageByPath("Tabs/TabRewards/Indicator"));

            // Content
            AssignTransformReference(so, "scrollContent", "ScrollView/Viewport/Content");
            AssignPrefabReference(so, "notificationCardPrefab", "Assets/_Project/Prefabs/Social/NotificationCard.prefab");
            AssignReference(so, "emptyText", FindTextByName("emptytext"));
            AssignGameObjectReference(so, "loadingIndicator", "loadingindicator");

            // Footer
            AssignReference(so, "markAllReadButton", FindButtonByName("markallreadbutton"));
            AssignReference(so, "markAllReadText", FindTextByPath("Footer/MarkAllReadButton/Text"));

            // Animations
            AssignReference(so, "headerTransform", FindRectByName("header"));
            AssignReference(so, "tabsTransform", FindRectByName("tabs"));
            AssignReference(so, "scrollViewTransform", FindRectByName("scrollview"));
            AssignReference(so, "footerTransform", FindRectByName("footer"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(manager.gameObject);
            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);

            Log("=== ASSIGNMENT COMPLETE ===");
        }

        private static MonoBehaviour FindNotificationsManager()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "NotificationsManager") return mb;
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

        private static void AssignTransformReference(SerializedObject so, string propertyName, string path)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { AddResult(propertyName, "Property not found", false, null); failedCount++; return; }
            if (prop.objectReferenceValue != null) { AddResult(propertyName, "Already Set", true, prop.objectReferenceValue); alreadySetCount++; return; }

            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) { AddResult(propertyName, "Canvas not found", false, null); failedCount++; return; }

            Transform t = canvas.transform.Find(path);
            if (t != null) { prop.objectReferenceValue = t; AddResult(propertyName, "Assigned", true, t); assignedCount++; }
            else { AddResult(propertyName, "Not found", false, null); failedCount++; }
        }

        private static void AssignPrefabReference(SerializedObject so, string propertyName, string prefabPath)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { AddResult(propertyName, "Property not found", false, null); failedCount++; return; }
            if (prop.objectReferenceValue != null) { AddResult(propertyName, "Already Set", true, prop.objectReferenceValue); alreadySetCount++; return; }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null) { prop.objectReferenceValue = prefab; AddResult(propertyName, "Assigned", true, prefab); assignedCount++; }
            else { AddResult(propertyName, "Prefab not found", false, null); failedCount++; }
        }

        private static void AssignGameObjectReference(SerializedObject so, string propertyName, string namePattern)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { AddResult(propertyName, "Property not found", false, null); failedCount++; return; }
            if (prop.objectReferenceValue != null) { AddResult(propertyName, "Already Set", true, prop.objectReferenceValue); alreadySetCount++; return; }

            var all = Object.FindObjectsOfType<Transform>(true);
            foreach (var t in all)
                if (t.gameObject.name.ToLower().Contains(namePattern.ToLower()))
                {
                    prop.objectReferenceValue = t.gameObject;
                    AddResult(propertyName, "Assigned", true, t.gameObject);
                    assignedCount++;
                    return;
                }
            AddResult(propertyName, "Not found", false, null); failedCount++;
        }

        #endregion

        #region Finders

        private static TextMeshProUGUI FindTextByName(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<TextMeshProUGUI>(true);
            foreach (var p in patterns) foreach (var t in all) if (t.gameObject.name.ToLower().Contains(p.ToLower())) return t;
            return null;
        }

        private static TextMeshProUGUI FindTextByPath(string path)
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return null;
            Transform t = canvas.transform;
            foreach (string part in path.Split('/'))
            {
                t = t.Find(part);
                if (t == null) return null;
            }
            return t.GetComponent<TextMeshProUGUI>();
        }

        private static Button FindButtonByName(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<Button>(true);
            foreach (var p in patterns) foreach (var b in all) if (b.gameObject.name.ToLower().Contains(p.ToLower())) return b;
            return null;
        }

        private static Image FindImageByPath(string path)
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return null;
            Transform t = canvas.transform;
            foreach (string part in path.Split('/'))
            {
                t = t.Find(part);
                if (t == null) return null;
            }
            return t.GetComponent<Image>();
        }

        private static RectTransform FindRectByName(params string[] patterns)
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return null;
            foreach (var p in patterns)
            {
                Transform t = canvas.transform.Find(p.Substring(0, 1).ToUpper() + p.Substring(1));
                if (t != null) return t.GetComponent<RectTransform>();
                // Also try exact match
                for (int i = 0; i < canvas.transform.childCount; i++)
                {
                    var child = canvas.transform.GetChild(i);
                    if (child.name.ToLower() == p.ToLower())
                        return child.GetComponent<RectTransform>();
                }
            }
            return null;
        }

        private static FieldInfo GetField(object obj, string fieldName)
        {
            return obj.GetType().GetField(fieldName,
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        }

        #endregion

        #region Helpers

        private static void ResetLog() { log = ""; assignedCount = 0; failedCount = 0; alreadySetCount = 0; results.Clear(); }
        private static void Log(string msg) { log += msg + "\n"; Debug.Log($"[NotificationsReferenceAssigner] {msg}"); }
        private static void AddResult(string f, string s, bool ok, Object o) { results.Add(new ReferenceResult { fieldName = f, status = s, success = ok, assignedObject = o }); }

        #endregion
    }
}
