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
    /// Reference Assigner for CashBattleHub scene.
    /// Automatically finds and assigns UI references to CashBattleManager.
    ///
    /// Menu: DigitPark/Auto Assigners/References/CashBattle/CashBattleHub References
    /// </summary>
    public class CashBattleHubReferenceAssigner : EditorWindow
    {
        private Vector2 scrollPosition;
        private static string log = "";
        private static int assignedCount = 0;
        private static int failedCount = 0;
        private static int alreadySetCount = 0;
        private static List<ReferenceResult> results = new List<ReferenceResult>();

        private static readonly string[] REQUIRED_REFS = {
            // Header
            "titleText", "balanceText", "backButton",
            // Menu Cards
            "mainPanel", "battles1v1Card", "cashTournamentsCard", "walletCard", "cashProfileCard", "historyCard",
            // Sub-panels
            "gameSelectionPanel", "tournamentListPanel",
            // Confirm Bet
            "confirmBetPanel", "confirmBetText", "confirmBetButton", "cancelBetButton",
            // Matchmaking
            "matchmakingPanel", "matchmakingStatusText", "matchmakingTimerText", "opponentNameText", "cancelMatchmakingButton"
        };

        private struct ReferenceResult
        {
            public string fieldName;
            public string status;
            public bool success;
            public Object assignedObject;
        }

        #region Menu Items

        [MenuItem("DigitPark/Auto Assigners/References/CashBattle/CashBattleHub References", false, 240)]
        public static void ShowWindow()
        {
            var window = GetWindow<CashBattleHubReferenceAssigner>("CashBattleHub Reference Assigner");
            window.minSize = new Vector2(600, 500);
        }

        #endregion

        #region Window GUI

        private void OnGUI()
        {
            GUILayout.Label("CashBattleHub Scene Reference Assigner", EditorStyles.boldLabel);
            GUILayout.Space(10);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "CashBattleHub")
            {
                EditorGUILayout.HelpBox(
                    $"Escena actual: {currentScene}\nAbre la escena CashBattleHub primero!",
                    MessageType.Warning);
            }

            EditorGUILayout.HelpBox(
                "Asigna referencias UI a CashBattleManager:\n" +
                "- Header (titulo, balance, back button)\n" +
                "- Tarjetas del menu principal\n" +
                "- Paneles de apuesta y matchmaking",
                MessageType.Info);

            GUILayout.Space(10);

            MonoBehaviour target = FindController();
            if (target != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Target:", GUILayout.Width(50));
                EditorGUILayout.ObjectField(target, typeof(MonoBehaviour), true);
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
            Log("=== ASSIGNING CASHBATTLEHUB REFERENCES ===");

            var controller = FindController();
            if (controller == null)
            {
                Log("ERROR: CashBattleManager not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            SerializedObject so = new SerializedObject(controller);
            so.Update();

            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            Transform root = canvas != null ? canvas.transform : controller.transform.root;

            // === Header ===
            AssignReference(so, "titleText", FindTextByDeep(root, "TitleText"));
            AssignReference(so, "balanceText", FindTextByDeep(root, "BalanceText"));

            Transform backBtnT = FindDeep(root, "BackButton");
            AssignReference(so, "backButton", backBtnT != null ? backBtnT.GetComponent<Button>() : null);

            // === Menu Cards ===
            Transform mainPanelT = FindDeep(root, "CardsContainer") ?? FindDeep(root, "MainPanel");
            if (mainPanelT != null)
                AssignReference(so, "mainPanel", mainPanelT.gameObject);
            else
                AssignReference(so, "mainPanel", (Object)null);

            Transform battles1v1T = FindDeep(root, "Battles1v1Card");
            AssignReference(so, "battles1v1Card", battles1v1T != null ? battles1v1T.GetComponent<Button>() : null);

            Transform cashTournamentsT = FindDeep(root, "CashTournamentsCard");
            AssignReference(so, "cashTournamentsCard", cashTournamentsT != null ? cashTournamentsT.GetComponent<Button>() : null);

            Transform walletT = FindDeep(root, "WalletCard");
            AssignReference(so, "walletCard", walletT != null ? walletT.GetComponent<Button>() : null);

            Transform cashProfileT = FindDeep(root, "CashProfileCard");
            AssignReference(so, "cashProfileCard", cashProfileT != null ? cashProfileT.GetComponent<Button>() : null);

            Transform historyT = FindDeep(root, "HistoryCard");
            AssignReference(so, "historyCard", historyT != null ? historyT.GetComponent<Button>() : null);

            // === Sub-panels (find by type) ===
            // gameSelectionPanel - find by type CashBattle1v1Manager
            MonoBehaviour gameSelectionMB = null;
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "CashBattle1v1Manager") { gameSelectionMB = mb; break; }
            AssignReference(so, "gameSelectionPanel", gameSelectionMB);

            // tournamentListPanel - find by type TournamentListPanel
            MonoBehaviour tournamentListMB = null;
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "TournamentListPanel") { tournamentListMB = mb; break; }
            AssignReference(so, "tournamentListPanel", tournamentListMB);

            // === Confirm Bet ===
            Transform confirmBetPanelT = FindDeep(root, "ConfirmBetPanel");
            if (confirmBetPanelT != null)
                AssignReference(so, "confirmBetPanel", confirmBetPanelT.gameObject);
            else
                AssignReference(so, "confirmBetPanel", (Object)null);

            AssignReference(so, "confirmBetText", FindTextByDeep(root, "ConfirmBetText"));

            Transform confirmBetBtnT = FindDeep(root, "ConfirmBetButton");
            AssignReference(so, "confirmBetButton", confirmBetBtnT != null ? confirmBetBtnT.GetComponent<Button>() : null);

            Transform cancelBetBtnT = FindDeep(root, "CancelBetButton");
            AssignReference(so, "cancelBetButton", cancelBetBtnT != null ? cancelBetBtnT.GetComponent<Button>() : null);

            // === Matchmaking ===
            Transform matchmakingPanelT = FindDeep(root, "MatchmakingPanel");
            if (matchmakingPanelT != null)
                AssignReference(so, "matchmakingPanel", matchmakingPanelT.gameObject);
            else
                AssignReference(so, "matchmakingPanel", (Object)null);

            AssignReference(so, "matchmakingStatusText", FindTextByDeep(root, "MatchmakingStatusText"));
            AssignReference(so, "matchmakingTimerText", FindTextByDeep(root, "MatchmakingTimerText"));
            AssignReference(so, "opponentNameText", FindTextByDeep(root, "OpponentNameText"));

            Transform cancelMatchBtnT = FindDeep(root, "CancelMatchmakingButton");
            AssignReference(so, "cancelMatchmakingButton", cancelMatchBtnT != null ? cancelMatchBtnT.GetComponent<Button>() : null);

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(controller.gameObject);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            Log("=== ASSIGNMENT COMPLETE ===");
        }

        private static MonoBehaviour FindController()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "CashBattleManager") return mb;
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
        private static void Log(string msg) { log += msg + "\n"; Debug.Log($"[CashBattleHubReferenceAssigner] {msg}"); }
        private static void AddResult(string f, string s, bool ok, Object o) { results.Add(new ReferenceResult { fieldName = f, status = s, success = ok, assignedObject = o }); }

        #endregion
    }
}
