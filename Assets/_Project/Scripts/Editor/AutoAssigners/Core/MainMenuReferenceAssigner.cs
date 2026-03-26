using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Reflection;

namespace DigitPark.Editor.AutoAssigners
{
    /// <summary>
    /// Reference Assigner for MainMenu scene.
    /// Automatically finds and assigns UI references to MainMenuManager.
    /// Covers all 18 serialized fields including notifications, premium, and animator.
    ///
    /// Menu: DigitPark/Auto Assigners/References/Core/MainMenu References
    /// </summary>
    public class MainMenuReferenceAssigner : EditorWindow
    {
        private Vector2 scrollPosition;
        private static string log = "";
        private static int assignedCount = 0;
        private static int failedCount = 0;
        private static int alreadySetCount = 0;
        private static List<ReferenceResult> results = new List<ReferenceResult>();

        private static readonly string[] REQUIRED_REFS = {
            // Main Panel
            "mainMenuPanel", "titleText",
            // Navigation
            "playButton", "scoresButton", "cashBattleButton", "settingsButton",
            // User Info
            "userButton", "userText", "searchButton",
            // Notifications
            "notificationsButton", "notificationIconImage",
            "notificationBadge", "notificationBadgeText",
            // Premium
            "premiumButton", "premiumBadge", "premiumPanel",
            // Monetization
            "shopButton",
            // Animation
            "titleAnimator"
            // Note: notificationIconNormal, notificationIconActive (Sprites) assigned via UIBuilder icon system
        };

        private struct ReferenceResult
        {
            public string fieldName;
            public string status;
            public bool success;
            public Object assignedObject;
        }

        #region Menu Items

        [MenuItem("DigitPark/Scenes/Assign References/Core/MainMenu", false, 110)]
        public static void ShowWindow()
        {
            var window = GetWindow<MainMenuReferenceAssigner>("MainMenu Reference Assigner");
            window.minSize = new Vector2(600, 550);
        }

        #endregion

        #region Window GUI

        private void OnGUI()
        {
            GUILayout.Label("MainMenu Scene Reference Assigner", EditorStyles.boldLabel);
            GUILayout.Space(10);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "MainMenu")
            {
                EditorGUILayout.HelpBox(
                    $"Current scene: {currentScene}\n" +
                    "Please open the MainMenu scene first!",
                    MessageType.Warning);
                GUILayout.Space(10);
            }

            EditorGUILayout.HelpBox(
                "Assigns UI references to MainMenuManager:\n" +
                "• Main Panel (title/logo)\n" +
                "• Navigation (play, scores, cash battle, settings)\n" +
                "• User info (profile card, search, username)\n" +
                "• Notifications (button, icon, badge)\n" +
                "• Premium (button, badge)\n\n" +
                "Note: Sprite fields (notificationIconNormal/Active) and\n" +
                "titleAnimator are assigned via the UIBuilder icon system.",
                MessageType.Info);

            GUILayout.Space(10);

            MonoBehaviour targetManager = FindMainMenuManager();

            if (targetManager == null)
            {
                EditorGUILayout.HelpBox(
                    "MainMenuManager not found in scene!\n" +
                    "Add a MainMenuManager component to assign references.",
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
            Log("=== SCANNING MAINMENU REFERENCES ===");

            var manager = FindMainMenuManager();
            if (manager == null)
            {
                Log("ERROR: MainMenuManager not found in scene!");
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
            Log("=== ASSIGNING MAINMENU REFERENCES ===");

            var manager = FindMainMenuManager();
            if (manager == null)
            {
                Log("ERROR: MainMenuManager not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            so.Update();

            // Main Panel - canvas root or panel container
            AssignReference(so, "mainMenuPanel", FindByNameContains<Transform>("mainmenupanel", "mainpanel"));
            // titleText - LogoText in Header
            AssignReference(so, "titleText", FindTextByName("logotext", "logo", "digitpark", "title"));

            // Navigation buttons
            AssignReference(so, "playButton", FindButtonByName("playcard", "play", "jugar"));
            AssignReference(so, "scoresButton", FindButtonByName("rankingscard", "rankings", "scores"));
            AssignReference(so, "cashBattleButton", FindButtonByName("cashbattlecard", "cashbattle", "cash"));
            AssignReference(so, "settingsButton", FindButtonByName("settingsbutton", "settings"));

            // User info
            AssignReference(so, "userButton", FindButtonByName("profilecard", "playercard"));
            AssignReference(so, "userText", FindTextByName("username", "usernametext"));
            AssignReference(so, "searchButton", FindButtonByName("searchcard", "search", "buscar"));

            // Notifications
            AssignReference(so, "notificationsButton", FindButtonByName("notificationsbutton", "notification"));
            AssignReference(so, "notificationIconImage", FindImageByName("notificationsbutton/icon", "notificon"));
            AssignNotificationBadge(so);
            AssignReference(so, "notificationBadgeText", FindTextByName("badgetext", "notifbadge"));

            // Premium
            AssignReference(so, "premiumButton", FindButtonByName("premiumcard", "premium"));
            AssignReference(so, "premiumBadge", FindByNameContains<Transform>("premiumcard", "premiumbadge"));
            AssignPremiumPanel(so);

            // Monetization
            AssignReference(so, "shopButton", FindButtonByName("shopcard", "shop", "tienda"));

            // Animation
            AssignAnimator(so, "titleAnimator", "logotext", "title", "digitpark");

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(manager.gameObject);
            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);

            Log("=== ASSIGNMENT COMPLETE ===");
        }

        private static MonoBehaviour FindMainMenuManager()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "MainMenuManager") return mb;
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

        private static Image FindImageByName(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<Image>(true);
            foreach (var p in patterns) foreach (var i in all) if (i.gameObject.name.ToLower().Contains(p.ToLower())) return i;
            return null;
        }

        private static void AssignNotificationBadge(SerializedObject so)
        {
            var prop = so.FindProperty("notificationBadge");
            if (prop == null) { AddResult("notificationBadge", "Property not found", false, null); failedCount++; return; }
            if (prop.objectReferenceValue != null) { AddResult("notificationBadge", "Already Set", true, prop.objectReferenceValue); alreadySetCount++; return; }
            var all = Object.FindObjectsOfType<Transform>(true);
            foreach (var t in all)
                if (t.gameObject.name == "Badge" && t.parent != null && t.parent.gameObject.name.ToLower().Contains("notification"))
                {
                    prop.objectReferenceValue = t.gameObject;
                    AddResult("notificationBadge", "Assigned", true, t.gameObject);
                    assignedCount++;
                    return;
                }
            AddResult("notificationBadge", "Not found", false, null); failedCount++;
        }

        private static void AssignPremiumPanel(SerializedObject so)
        {
            var prop = so.FindProperty("premiumPanel");
            if (prop == null) { AddResult("premiumPanel", "Property not found", false, null); failedCount++; return; }
            if (prop.objectReferenceValue != null) { AddResult("premiumPanel", "Already Set", true, prop.objectReferenceValue); alreadySetCount++; return; }
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "PremiumPanelUI")
                {
                    prop.objectReferenceValue = mb;
                    AddResult("premiumPanel", "Assigned", true, mb);
                    assignedCount++;
                    return;
                }
            AddResult("premiumPanel", "Not found", false, null); failedCount++;
        }

        private static void AssignAnimator(SerializedObject so, string propertyName, params string[] patterns)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { AddResult(propertyName, "Property not found", false, null); failedCount++; return; }
            if (prop.objectReferenceValue != null) { AddResult(propertyName, "Already Set", true, prop.objectReferenceValue); alreadySetCount++; return; }
            var all = Object.FindObjectsOfType<Animator>(true);
            foreach (var p in patterns)
                foreach (var a in all)
                    if (a.gameObject.name.ToLower().Contains(p.ToLower()))
                    {
                        prop.objectReferenceValue = a;
                        AddResult(propertyName, "Assigned", true, a);
                        assignedCount++;
                        return;
                    }
            AddResult(propertyName, "Not found", false, null); failedCount++;
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
        private static void Log(string msg) { log += msg + "\n"; Debug.Log($"[MainMenuReferenceAssigner] {msg}"); }
        private static void AddResult(string f, string s, bool ok, Object o) { results.Add(new ReferenceResult { fieldName = f, status = s, success = ok, assignedObject = o }); }

        #endregion
    }
}
