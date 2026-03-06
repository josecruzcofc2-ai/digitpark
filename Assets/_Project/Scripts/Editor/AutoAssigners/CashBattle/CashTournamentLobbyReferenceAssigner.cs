using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;
using DigitPark.Editor;

namespace DigitPark.Editor.AutoAssigners
{
    /// <summary>
    /// Reference Assigner for CashTournamentLobby scene.
    /// Automatically finds and assigns UI references to CashTournamentLobbyManager.
    ///
    /// Menu: DigitPark/Auto Assigners/References/CashBattle/CashTournamentLobby References
    /// </summary>
    public class CashTournamentLobbyReferenceAssigner : EditorWindow
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
            // Tournament Info
            "gameTypeText", "entryFeeText", "prizePoolText",
            "playersProgressBar", "playersProgressText", "countdownText",
            // Rules
            "attemptsRuleText", "timeLimitRuleText",
            // Prize Distribution
            "prizeDistributionContainer",
            // Tabs
            "participantsTabButton", "chatTabButton",
            "participantsContent", "chatContent",
            "participantsTabIndicator", "chatTabIndicator", "chatBadgeText",
            // Participants
            "participantsContainer",
            // Chat
            "chatScrollRect", "chatMessagesContainer", "chatInput", "sendChatButton",
            // Actions
            "shareButton", "playButton", "playButtonText",
            // Status
            "loadingOverlay", "statusText", "startingOverlay", "startingCountdownText"
        };

        private struct ReferenceResult
        {
            public string fieldName;
            public string status;
            public bool success;
            public Object assignedObject;
        }

        #region Menu Items

        [MenuItem("DigitPark/Auto Assigners/References/CashBattle/CashTournamentLobby References", false, 185)]
        public static void ShowWindow()
        {
            var window = GetWindow<CashTournamentLobbyReferenceAssigner>("CashTournamentLobby Reference Assigner");
            window.minSize = new Vector2(600, 500);
        }

        #endregion

        #region Window GUI

        private void OnGUI()
        {
            GUILayout.Label("CashTournamentLobby Scene Reference Assigner", EditorStyles.boldLabel);
            GUILayout.Space(10);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "CashTournamentLobby")
            {
                EditorGUILayout.HelpBox(
                    $"Current scene: {currentScene}\n" +
                    "Please open the CashTournamentLobby scene first!",
                    MessageType.Warning);
            }

            EditorGUILayout.HelpBox(
                "Assigns UI references to CashTournamentLobbyManager:\n" +
                "- Header (back button, tournament name, status badge)\n" +
                "- Tournament info (game type, entry fee, prize pool, players, countdown, rules)\n" +
                "- Prize distribution container\n" +
                "- Tabs (participants, chat) and indicators\n" +
                "- Participants container\n" +
                "- Chat (scroll rect, messages, input, send button)\n" +
                "- Actions (join, leave, share buttons)\n" +
                "- Status overlays (loading, starting)",
                MessageType.Info);

            GUILayout.Space(10);

            MonoBehaviour targetManager = FindLobbyManager();
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
            Log("=== ASSIGNING CASHTOURNAMENTLOBBY REFERENCES ===");

            var manager = FindLobbyManager();
            if (manager == null)
            {
                Log("ERROR: CashTournamentLobbyManager not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            so.Update();

            Canvas canvas = FindMainCanvas();
            Transform root = canvas != null ? canvas.transform : manager.transform.root;

            // Header
            Transform backBtnT = FindDeep(root, "BackButton");
            AssignReference(so, "backButton", backBtnT != null ? backBtnT.GetComponent<Button>() : null);
            AssignReference(so, "tournamentNameText", FindTextByDeep(root, "TournamentNameText"));
            AssignReference(so, "statusBadgeText", FindTextByDeep(root, "StatusBadgeText"));

            Transform statusBadgeT = FindDeep(root, "StatusBadge");
            AssignReference(so, "statusBadgeImage", statusBadgeT != null ? statusBadgeT.GetComponent<Image>() : null);

            // Tournament Info
            AssignReference(so, "gameTypeText", FindTextByDeep(root, "GameTypeText"));
            AssignReference(so, "entryFeeText", FindTextByDeep(root, "EntryFeeText"));
            AssignReference(so, "prizePoolText", FindTextByDeep(root, "PrizePoolText"));

            Transform progressFillT = FindDeep(root, "ProgressBarFill");
            AssignReference(so, "playersProgressBar", progressFillT != null ? progressFillT.GetComponent<Image>() : null);
            AssignReference(so, "playersProgressText", FindTextByDeep(root, "PlayersProgressText"));
            AssignReference(so, "countdownText", FindTextByDeep(root, "CountdownText"));

            // Rules
            AssignReference(so, "attemptsRuleText", FindTextByDeep(root, "AttemptsRuleText"));
            AssignReference(so, "timeLimitRuleText", FindTextByDeep(root, "TimeLimitRuleText"));

            // Prize Distribution
            Transform prizeContainerT = FindDeep(root, "PrizeRowsContainer");
            AssignReference(so, "prizeDistributionContainer", prizeContainerT);

            // Tabs
            Transform participantsTabT = FindDeep(root, "ParticipantsTab");
            AssignReference(so, "participantsTabButton", participantsTabT != null ? participantsTabT.GetComponent<Button>() : null);

            Transform chatTabT = FindDeep(root, "ChatTab");
            AssignReference(so, "chatTabButton", chatTabT != null ? chatTabT.GetComponent<Button>() : null);

            Transform participantsContentT = FindDeep(root, "ParticipantsContent");
            AssignReference(so, "participantsContent", participantsContentT != null ? participantsContentT.gameObject : null);

            Transform chatContentT = FindDeep(root, "ChatContent");
            AssignReference(so, "chatContent", chatContentT != null ? chatContentT.gameObject : null);

            Transform participantsIndT = FindDeep(root, "ParticipantsTabIndicator");
            AssignReference(so, "participantsTabIndicator", participantsIndT != null ? participantsIndT.GetComponent<Image>() : null);

            Transform chatIndT = FindDeep(root, "ChatTabIndicator");
            AssignReference(so, "chatTabIndicator", chatIndT != null ? chatIndT.GetComponent<Image>() : null);

            AssignReference(so, "chatBadgeText", FindTextByDeep(root, "ChatBadgeText"));

            // Participants
            Transform participantsContainerT = FindDeep(root, "ParticipantsContainer");
            AssignReference(so, "participantsContainer", participantsContainerT);

            // Participant prefab
            var participantPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Tournaments/Lobby/ParticipantItem.prefab");
            AssignReference(so, "participantItemPrefab", participantPrefab);

            // Chat
            Transform chatScrollT = FindDeep(root, "ChatScrollRect");
            AssignReference(so, "chatScrollRect", chatScrollT != null ? chatScrollT.GetComponent<ScrollRect>() : null);

            Transform chatMsgContainerT = FindDeep(root, "ChatMessagesContainer");
            AssignReference(so, "chatMessagesContainer", chatMsgContainerT);

            Transform chatInputT = FindDeep(root, "ChatInput");
            AssignReference(so, "chatInput", chatInputT != null ? chatInputT.GetComponent<TMP_InputField>() : null);

            Transform sendBtnT = FindDeep(root, "SendChatButton");
            AssignReference(so, "sendChatButton", sendBtnT != null ? sendBtnT.GetComponent<Button>() : null);

            // Actions
            Transform shareBtnT = FindDeep(root, "ShareButton");
            AssignReference(so, "shareButton", shareBtnT != null ? shareBtnT.GetComponent<Button>() : null);

            Transform playBtnT = FindDeep(root, "PlayButton");
            AssignReference(so, "playButton", playBtnT != null ? playBtnT.GetComponent<Button>() : null);
            AssignReference(so, "playButtonText", FindTextByDeep(root, "CashPlayButtonText"));

            // Status
            Transform loadingT = FindDeep(root, "LoadingOverlay");
            AssignReference(so, "loadingOverlay", loadingT != null ? loadingT.gameObject : null);
            AssignReference(so, "statusText", FindTextByDeep(root, "CashLoadingStatusText"));

            Transform startingT = FindDeep(root, "StartingOverlay");
            AssignReference(so, "startingOverlay", startingT != null ? startingT.gameObject : null);
            AssignReference(so, "startingCountdownText", FindTextByDeep(root, "StartingCountdownText"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(manager.gameObject);
            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
            Log("=== ASSIGNMENT COMPLETE ===");
        }

        private static MonoBehaviour FindLobbyManager()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "CashTournamentLobbyManager") return mb;
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

        private static Canvas FindMainCanvas()
        {
            return UIBuilderCanvasHelper.FindMainCanvas();
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

        #endregion

        #region Helpers

        private static void ResetLog() { log = ""; assignedCount = 0; failedCount = 0; alreadySetCount = 0; results.Clear(); }
        private static void Log(string msg) { log += msg + "\n"; Debug.Log($"[CashTournamentLobbyReferenceAssigner] {msg}"); }
        private static void AddResult(string f, string s, bool ok, Object o) { results.Add(new ReferenceResult { fieldName = f, status = s, success = ok, assignedObject = o }); }

        #endregion
    }
}
