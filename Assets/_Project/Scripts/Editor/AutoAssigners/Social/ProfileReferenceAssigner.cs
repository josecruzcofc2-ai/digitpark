using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

namespace DigitPark.Editor.AutoAssigners
{
    /// <summary>
    /// Reference Assigner for Profile scene.
    /// Automatically finds and assigns UI references to ProfileManager.
    ///
    /// Menu: DigitPark/Auto Assigners/References/Social/Profile References
    /// </summary>
    public class ProfileReferenceAssigner : EditorWindow
    {
        private Vector2 scrollPosition;
        private static string log = "";
        private static int assignedCount = 0;
        private static int failedCount = 0;
        private static int alreadySetCount = 0;
        private static List<ReferenceResult> results = new List<ReferenceResult>();

        private static readonly string[] REQUIRED_REFS = {
            // Header
            "backButton", "addFriendIconButton",
            // Profile Info
            "usernameText", "avatarImage", "avatarUI", "editAvatarButton", "statusText",
            // General Stats
            "totalGamesText", "winsText", "winRateText",
            // Game Stats
            "digitRushValueText", "memoryPairsValueText", "quickMathValueText",
            "flashTapValueText", "oddOneOutValueText",
            // Action Buttons
            "friendsButton", "historyButton", "challengeButton",
            // Game Selection Panel
            "gameSelectionPanel", "cancelButton"
        };

        private struct ReferenceResult
        {
            public string fieldName;
            public string status;
            public bool success;
            public Object assignedObject;
        }

        #region Menu Items

        [MenuItem("DigitPark/Auto Assigners/References/Social/Profile References", false, 255)]
        public static void ShowWindow()
        {
            var window = GetWindow<ProfileReferenceAssigner>("Profile Reference Assigner");
            window.minSize = new Vector2(600, 550);
        }

        #endregion

        #region Window GUI

        private void OnGUI()
        {
            GUILayout.Label("Profile Scene Reference Assigner", EditorStyles.boldLabel);
            GUILayout.Space(10);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "Profile")
            {
                EditorGUILayout.HelpBox(
                    $"Current scene: {currentScene}\n" +
                    "Please open the Profile scene first!",
                    MessageType.Warning);
            }

            EditorGUILayout.HelpBox(
                "Assigns UI references to ProfileManager:\n" +
                "• Header (back, add friend)\n" +
                "• Profile info (username, avatar, status)\n" +
                "• Stats (general and per-game)\n" +
                "• Action buttons and game selection panel",
                MessageType.Info);

            GUILayout.Space(10);

            MonoBehaviour targetManager = FindProfileManager();
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
            GUILayout.Label(successRate == 1f ? "✓ ALL REFERENCES SET" : "⚠ Some references missing", EditorStyles.boldLabel);
            GUI.color = Color.white;

            GUILayout.Label($"Assigned: {assignedCount} | Already Set: {alreadySetCount} | Failed: {failedCount}");

            foreach (var result in results)
            {
                EditorGUILayout.BeginHorizontal();
                GUI.color = result.success ? (result.status == "Already Set" ? new Color(0.5f, 0.8f, 1f) : Color.green) : Color.red;
                GUILayout.Label(result.success ? (result.status == "Already Set" ? "●" : "✓") : "✗", GUILayout.Width(20));
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

        #region Assignment Logic

        private static void AssignAllReferences()
        {
            Log("=== ASSIGNING PROFILE REFERENCES ===");

            var manager = FindProfileManager();
            if (manager == null)
            {
                Log("ERROR: ProfileManager not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            SerializedObject so = new SerializedObject(manager);

            // Header
            AssignReference(so, "backButton", FindButtonByName("back", "return", "atras"));
            AssignReference(so, "addFriendIconButton", FindButtonByName("addfriend", "add", "agregar"));

            // Profile Info
            AssignReference(so, "usernameText", FindTextByName("username", "nombre", "playername"));
            AssignReference(so, "avatarImage", FindImageByName("avatar", "profile", "foto"));
            AssignReference(so, "avatarUI", FindComponentByName<DigitPark.UI.Components.AvatarUI>("avatar"));
            AssignReference(so, "editAvatarButton", FindButtonByName("editavatar", "changeavatar", "editphoto", "camera"));
            AssignReference(so, "statusText", FindTextByName("status", "estado", "relation"));

            // General Stats
            AssignReference(so, "totalGamesText", FindTextByName("totalgames", "played", "partidas"));
            AssignReference(so, "winsText", FindTextByName("wins", "victorias", "won"));
            AssignReference(so, "winRateText", FindTextByName("winrate", "rate", "porcentaje"));

            // Game Stats
            AssignReference(so, "digitRushValueText", FindTextByName("digitrush", "digit"));
            AssignReference(so, "memoryPairsValueText", FindTextByName("memorypairs", "memory"));
            AssignReference(so, "quickMathValueText", FindTextByName("quickmath", "math"));
            AssignReference(so, "flashTapValueText", FindTextByName("flashtap", "flash"));
            AssignReference(so, "oddOneOutValueText", FindTextByName("oddoneout", "odd"));

            // Action Buttons
            AssignReference(so, "friendsButton", FindButtonByName("friends", "amigos"));
            AssignReference(so, "historyButton", FindButtonByName("history", "historial"));
            AssignReference(so, "challengeButton", FindButtonByName("challenge", "retar", "vs"));

            // Game Selection Panel
            AssignReference(so, "gameSelectionPanel", FindByNameContains<Transform>("gameselection", "selectgame", "gamepanel"));
            AssignReference(so, "cancelButton", FindButtonByName("cancel", "cancelar", "close"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            Log("=== ASSIGNMENT COMPLETE ===");
        }

        private static MonoBehaviour FindProfileManager()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "ProfileManager") return mb;
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

        private static T FindComponentByName<T>(params string[] patterns) where T : Component
        {
            var all = Object.FindObjectsOfType<T>(true);
            foreach (var p in patterns) foreach (var c in all) if (c.gameObject.name.ToLower().Contains(p.ToLower())) return c;
            return null;
        }

        #endregion

        #region Helpers

        private static void ResetLog() { log = ""; assignedCount = 0; failedCount = 0; alreadySetCount = 0; results.Clear(); }
        private static void Log(string msg) { log += msg + "\n"; Debug.Log($"[ProfileReferenceAssigner] {msg}"); }
        private static void AddResult(string f, string s, bool ok, Object o) { results.Add(new ReferenceResult { fieldName = f, status = s, success = ok, assignedObject = o }); }

        #endregion
    }
}
