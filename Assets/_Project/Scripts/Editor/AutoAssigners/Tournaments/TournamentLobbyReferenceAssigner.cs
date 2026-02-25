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
    /// Reference Assigner for TournamentLobby scene.
    /// Automatically finds and assigns UI references to TournamentLobbyManager.
    ///
    /// Menu: DigitPark/Auto Assigners/References/Tournaments/TournamentLobby References
    /// </summary>
    public class TournamentLobbyReferenceAssigner : EditorWindow
    {
        private Vector2 scrollPosition;
        private static string log = "";
        private static int assignedCount = 0;
        private static int failedCount = 0;
        private static int alreadySetCount = 0;
        private static List<ReferenceResult> results = new List<ReferenceResult>();

        private static readonly string[] REQUIRED_REFS = {
            // Header
            "backButton", "tournamentNameText", "statusBadgeText", "statusBadgeImage",
            // Info
            "gameTypeText", "entryFeeText", "prizePoolText",
            "playersProgressBar", "playersProgressText", "countdownText",
            // Rules
            "attemptsRuleText", "timeLimitRuleText",
            // Tabs
            "participantsTabButton", "chatTabButton",
            "participantsContent", "chatContent",
            "participantsTabIndicator", "chatTabIndicator", "chatBadgeText",
            // Participants
            "participantsContainer", "participantsHeaderText",
            // Chat
            "chatScrollRect", "chatMessagesContainer", "chatInput", "sendChatButton",
            // Actions
            "joinButton", "leaveButton", "shareButton", "joinButtonText",
            // Status
            "loadingOverlay", "statusText", "startingOverlay", "startingCountdownText",
            // Configuration
            "defaultAvatarSprite"
        };

        private struct ReferenceResult
        {
            public string fieldName;
            public string status;
            public bool success;
            public Object assignedObject;
        }

        #region Menu Items

        [MenuItem("DigitPark/Auto Assigners/References/Tournaments/TournamentLobby References", false, 265)]
        public static void ShowWindow()
        {
            var window = GetWindow<TournamentLobbyReferenceAssigner>("TournamentLobby Reference Assigner");
            window.minSize = new Vector2(600, 500);
        }

        public static void RunAutoAssign()
        {
            ResetLog();
            AssignAllReferences();
        }

        #endregion

        #region Window GUI

        private void OnGUI()
        {
            GUILayout.Label("TournamentLobby Scene Reference Assigner", EditorStyles.boldLabel);
            GUILayout.Space(10);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "TournamentLobby")
            {
                EditorGUILayout.HelpBox(
                    $"Current scene: {currentScene}\n" +
                    "Please open the TournamentLobby scene first!",
                    MessageType.Warning);
            }

            EditorGUILayout.HelpBox(
                "Assigns UI references to TournamentLobbyManager:\n" +
                "• Header (back, name, status badge)\n" +
                "• Info card (game, fee, prize, players progress, countdown, rules)\n" +
                "• Tabs (participants, chat)\n" +
                "• Participants list + Chat system\n" +
                "• Action buttons (join, leave, share)\n" +
                "• Overlays (loading, starting)",
                MessageType.Info);

            GUILayout.Space(10);

            MonoBehaviour targetManager = FindTournamentLobbyManager();
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

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            DrawResultsSummary();
            EditorGUILayout.EndScrollView();
        }

        private void DrawResultsSummary()
        {
            if (results.Count == 0) return;

            int total = results.Count;
            int successTotal = assignedCount + alreadySetCount;

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
                GUILayout.Label(result.fieldName, GUILayout.Width(220));
                GUILayout.Label(result.status, GUILayout.Width(120));
                if (result.assignedObject != null)
                    EditorGUILayout.ObjectField(result.assignedObject, typeof(Object), true, GUILayout.Width(150));
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        #endregion

        #region Assignment Logic

        private static void AssignAllReferences()
        {
            Log("=== ASSIGNING TOURNAMENTLOBBY REFERENCES ===");

            var manager = FindTournamentLobbyManager();
            if (manager == null)
            {
                Log("ERROR: TournamentLobbyManager not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            so.Update();

            // Header
            AssignReference(so, "backButton", FindButtonByName("backbutton", "back"));
            AssignReference(so, "tournamentNameText", FindTextByExactName("TournamentNameText"));
            AssignReference(so, "statusBadgeText", FindTextByExactName("StatusBadgeText"));
            AssignReference(so, "statusBadgeImage", FindImageByExactName("StatusBadge"));

            // Info Card
            AssignReference(so, "gameTypeText", FindTextByExactName("GameTypeText"));
            AssignReference(so, "entryFeeText", FindTextByExactName("EntryFeeText"));
            AssignReference(so, "prizePoolText", FindTextByExactName("PrizePoolText"));
            AssignReference(so, "playersProgressBar", FindImageByExactName("PlayersProgressBar"));
            AssignReference(so, "playersProgressText", FindTextByExactName("PlayersProgressText"));
            AssignReference(so, "countdownText", FindTextByExactName("CountdownText"));

            // Rules
            AssignReference(so, "attemptsRuleText", FindTextByExactName("AttemptsRuleText"));
            AssignReference(so, "timeLimitRuleText", FindTextByExactName("TimeLimitRuleText"));

            // Tabs
            AssignReference(so, "participantsTabButton", FindButtonByExactName("ParticipantsTab"));
            AssignReference(so, "chatTabButton", FindButtonByExactName("ChatTab"));
            AssignGameObject(so, "participantsContent", "ParticipantsContent");
            AssignGameObject(so, "chatContent", "ChatContent");
            AssignReference(so, "participantsTabIndicator", FindImageByExactName("ParticipantsTabIndicator"));
            AssignReference(so, "chatTabIndicator", FindImageByExactName("ChatTabIndicator"));
            AssignReference(so, "chatBadgeText", FindTextByExactName("ChatBadgeText"));

            // Participants
            AssignTransform(so, "participantsContainer", "Content", "ParticipantsScrollView");
            AssignReference(so, "participantsHeaderText", FindTextByName("participantsheader", "leaderboardheader"));

            // Chat
            AssignScrollRect(so, "chatScrollRect", "ChatScrollView");
            AssignTransform(so, "chatMessagesContainer", "ChatMessagesContainer");
            AssignInputField(so, "chatInput", "ChatInput");
            AssignReference(so, "sendChatButton", FindButtonByExactName("SendChatButton"));

            // Actions
            AssignReference(so, "joinButton", FindButtonByExactName("JoinButton"));
            AssignReference(so, "leaveButton", FindButtonByExactName("LeaveButton"));
            AssignReference(so, "shareButton", FindButtonByExactName("ShareButton"));
            AssignReference(so, "joinButtonText", FindTextByExactName("JoinButtonText"));

            // Status
            AssignGameObject(so, "loadingOverlay", "LoadingOverlay");
            AssignReference(so, "statusText", FindTextByExactName("StatusText"));
            AssignGameObject(so, "startingOverlay", "StartingOverlay");
            AssignReference(so, "startingCountdownText", FindTextByExactName("StartingCountdownText"));

            // Configuration - Default avatar sprite
            AssignSpriteAsset(so, "defaultAvatarSprite", "Assets/_Project/Art/Icons/Social/Profile/AvatarDefaultNeon.png");

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(manager.gameObject);
            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
            Log("=== ASSIGNMENT COMPLETE ===");
        }

        private static MonoBehaviour FindTournamentLobbyManager()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "TournamentLobbyManager") return mb;
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

        private static TextMeshProUGUI FindTextByExactName(string name)
        {
            foreach (var t in Object.FindObjectsOfType<TextMeshProUGUI>(true))
                if (t.gameObject.name == name) return t;
            return null;
        }

        private static TextMeshProUGUI FindTextByName(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<TextMeshProUGUI>(true);
            foreach (var p in patterns) foreach (var t in all) if (t.gameObject.name.ToLower().Contains(p.ToLower())) return t;
            return null;
        }

        private static Button FindButtonByExactName(string name)
        {
            foreach (var b in Object.FindObjectsOfType<Button>(true))
                if (b.gameObject.name == name) return b;
            return null;
        }

        private static Button FindButtonByName(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<Button>(true);
            foreach (var p in patterns) foreach (var b in all) if (b.gameObject.name.ToLower().Contains(p.ToLower())) return b;
            return null;
        }

        private static Image FindImageByExactName(string name)
        {
            foreach (var i in Object.FindObjectsOfType<Image>(true))
                if (i.gameObject.name == name) return i;
            return null;
        }

        private static T FindByNameContains<T>(params string[] patterns) where T : Component
        {
            var all = Object.FindObjectsOfType<T>(true);
            foreach (var p in patterns) foreach (var o in all) if (o.gameObject.name.ToLower().Contains(p.ToLower())) return o;
            return null;
        }

        /// <summary>
        /// Assigns a Transform field by finding a GameObject with exact name,
        /// optionally scoped under a parent name.
        /// </summary>
        private static void AssignTransform(SerializedObject so, string propertyName, string targetName, string parentName = null)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { AddResult(propertyName, "Property not found", false, null); failedCount++; return; }
            if (prop.objectReferenceValue != null) { AddResult(propertyName, "Already Set", true, prop.objectReferenceValue); alreadySetCount++; return; }

            foreach (var t in Object.FindObjectsOfType<Transform>(true))
            {
                if (t.gameObject.name != targetName) continue;
                if (parentName != null && (t.parent == null || !t.parent.gameObject.name.Contains(parentName))) continue;
                prop.objectReferenceValue = t;
                AddResult(propertyName, "Assigned", true, t);
                assignedCount++;
                return;
            }
            AddResult(propertyName, "Not found", false, null); failedCount++;
        }

        private static void AssignGameObject(SerializedObject so, string propertyName, params string[] patterns)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { AddResult(propertyName, "Property not found", false, null); failedCount++; return; }
            if (prop.objectReferenceValue != null) { AddResult(propertyName, "Already Set", true, prop.objectReferenceValue); alreadySetCount++; return; }
            var all = Object.FindObjectsOfType<Transform>(true);
            foreach (var p in patterns)
                foreach (var o in all)
                    if (o.gameObject.name == p)
                    {
                        prop.objectReferenceValue = o.gameObject;
                        AddResult(propertyName, "Assigned", true, o.gameObject);
                        assignedCount++;
                        return;
                    }
            AddResult(propertyName, "Not found", false, null); failedCount++;
        }

        private static void AssignScrollRect(SerializedObject so, string propertyName, params string[] patterns)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { AddResult(propertyName, "Property not found", false, null); failedCount++; return; }
            if (prop.objectReferenceValue != null) { AddResult(propertyName, "Already Set", true, prop.objectReferenceValue); alreadySetCount++; return; }
            var all = Object.FindObjectsOfType<ScrollRect>(true);
            foreach (var p in patterns)
                foreach (var o in all)
                    if (o.gameObject.name == p)
                    {
                        prop.objectReferenceValue = o;
                        AddResult(propertyName, "Assigned", true, o);
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
                    if (o.gameObject.name == p)
                    {
                        prop.objectReferenceValue = o;
                        AddResult(propertyName, "Assigned", true, o);
                        assignedCount++;
                        return;
                    }
            AddResult(propertyName, "Not found", false, null); failedCount++;
        }

        private static void AssignSpriteAsset(SerializedObject so, string propertyName, string assetPath)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { AddResult(propertyName, "Property not found", false, null); failedCount++; return; }
            if (prop.objectReferenceValue != null) { AddResult(propertyName, "Already Set", true, prop.objectReferenceValue); alreadySetCount++; return; }
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite != null) { prop.objectReferenceValue = sprite; AddResult(propertyName, "Assigned", true, sprite); assignedCount++; }
            else { AddResult(propertyName, $"Asset not found: {assetPath}", false, null); failedCount++; }
        }

        #endregion

        #region Helpers

        private static void ResetLog() { log = ""; assignedCount = 0; failedCount = 0; alreadySetCount = 0; results.Clear(); }
        private static void Log(string msg) { log += msg + "\n"; Debug.Log($"[TournamentLobbyReferenceAssigner] {msg}"); }
        private static void AddResult(string f, string s, bool ok, Object o) { results.Add(new ReferenceResult { fieldName = f, status = s, success = ok, assignedObject = o }); }

        #endregion
    }
}
