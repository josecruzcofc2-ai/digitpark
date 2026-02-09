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
            "titleText", "balanceText", "backButton",
            "mainPanel", "battles1v1Card", "cashTournamentsCard", "walletCard", "historyCard",
            "ageVerificationPanel", "verifyAgeButton", "verificationStatusText",
            "confirmBetPanel", "confirmBetText", "confirmBetButton", "cancelBetButton",
            "matchmakingPanel", "matchmakingStatusText", "cancelMatchmakingButton"
        };

        private struct ReferenceResult
        {
            public string fieldName;
            public string status;
            public bool success;
            public Object assignedObject;
        }

        #region Menu Items

        [MenuItem("DigitPark/Auto Assigners/References/CashBattle/CashBattleHub References", false, 211)]
        public static void ShowWindow()
        {
            var window = GetWindow<CashBattleHubReferenceAssigner>("CashBattleHub Reference Assigner");
            window.minSize = new Vector2(600, 550);
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
                    $"Current scene: {currentScene}\n" +
                    "Please open the CashBattleHub scene first!",
                    MessageType.Warning);
                GUILayout.Space(10);
            }

            EditorGUILayout.HelpBox(
                "Assigns UI references to CashBattleManager:\n" +
                "• Header (title, balance, back)\n" +
                "• Main cards (1v1, tournaments, wallet, history)\n" +
                "• Age verification panel\n" +
                "• Matchmaking panel",
                MessageType.Info);

            GUILayout.Space(10);

            MonoBehaviour targetManager = FindCashBattleManager();

            if (targetManager == null)
            {
                EditorGUILayout.HelpBox(
                    "CashBattleManager not found in scene!\n" +
                    "Add a CashBattleManager component to assign references.",
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

            GUILayout.Label("Detailed Log:", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
            EditorGUILayout.TextArea(log, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        private void DrawResultsSummary()
        {
            if (results.Count == 0) return;

            int total = results.Count;
            int successTotal = assignedCount + alreadySetCount;

            EditorGUILayout.BeginVertical("box");

            float successRate = (float)successTotal / total;
            Color summaryColor;
            string summaryText;

            if (successRate == 1f)
            {
                summaryColor = new Color(0.2f, 0.8f, 0.2f);
                summaryText = "✓ ALL REFERENCES SET";
            }
            else if (successRate >= 0.7f)
            {
                summaryColor = new Color(1f, 0.8f, 0.2f);
                summaryText = "⚠ PARTIAL - Some references missing";
            }
            else
            {
                summaryColor = new Color(1f, 0.4f, 0.4f);
                summaryText = "✗ INCOMPLETE - Many references missing";
            }

            GUI.color = summaryColor;
            GUILayout.Label(summaryText, EditorStyles.boldLabel);
            GUI.color = Color.white;

            GUILayout.Label($"Assigned: {assignedCount} | Already Set: {alreadySetCount} | Failed: {failedCount}");

            GUILayout.Space(5);

            foreach (var result in results)
            {
                EditorGUILayout.BeginHorizontal();

                if (result.success)
                {
                    if (result.status == "Already Set")
                    {
                        GUI.color = new Color(0.5f, 0.8f, 1f);
                        GUILayout.Label("●", GUILayout.Width(20));
                    }
                    else
                    {
                        GUI.color = Color.green;
                        GUILayout.Label("✓", GUILayout.Width(20));
                    }
                }
                else
                {
                    GUI.color = Color.red;
                    GUILayout.Label("✗", GUILayout.Width(20));
                }
                GUI.color = Color.white;

                GUILayout.Label(result.fieldName, GUILayout.Width(180));
                GUILayout.Label(result.status, GUILayout.Width(120));

                if (result.assignedObject != null)
                {
                    EditorGUILayout.ObjectField(result.assignedObject, typeof(Object), true, GUILayout.Width(150));
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        #endregion

        #region Assignment Logic

        private static void ScanCurrentReferences()
        {
            Log("=== SCANNING CASHBATTLEHUB REFERENCES ===");

            var manager = FindCashBattleManager();
            if (manager == null)
            {
                Log("ERROR: CashBattleManager not found in scene!");
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
            Log("=== ASSIGNING CASHBATTLEHUB REFERENCES ===");

            var manager = FindCashBattleManager();
            if (manager == null)
            {
                Log("ERROR: CashBattleManager not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            so.Update();

            // Header
            AssignReference(so, "titleText", FindTextByName("title", "header", "cashbattle"));
            AssignReference(so, "balanceText", FindTextByName("balance", "wallet", "money"));
            AssignReference(so, "backButton", FindButtonByName("back", "return", "close"));

            // Main Panel Cards
            AssignReference(so, "mainPanel", FindByNameContains<Transform>("mainpanel", "main", "content"));
            AssignReference(so, "battles1v1Card", FindButtonByName("1v1", "battles", "versus"));
            AssignReference(so, "cashTournamentsCard", FindButtonByName("tournament", "torneos"));
            AssignReference(so, "walletCard", FindButtonByName("wallet", "billetera", "deposit"));
            AssignReference(so, "historyCard", FindButtonByName("history", "historial", "record"));

            // Age Verification
            AssignReference(so, "ageVerificationPanel", FindByNameContains<Transform>("ageverification", "verification", "age"));
            AssignReference(so, "verifyAgeButton", FindButtonByName("verify", "verificar", "age"));
            AssignReference(so, "verificationStatusText", FindTextByName("verificationstatus", "status"));

            // Bet Confirmation
            AssignReference(so, "confirmBetPanel", FindByNameContains<Transform>("confirmbet", "betconfirm"));
            AssignReference(so, "confirmBetText", FindTextByName("confirmbet"));
            AssignReference(so, "confirmBetButton", FindButtonByName("confirmbet"));
            AssignReference(so, "cancelBetButton", FindButtonByName("cancelbet"));

            // Matchmaking
            AssignReference(so, "matchmakingPanel", FindByNameContains<Transform>("matchmaking", "searching", "finding"));
            AssignReference(so, "matchmakingStatusText", FindTextByName("matchmakingstatus", "searchstatus"));
            AssignReference(so, "cancelMatchmakingButton", FindButtonByName("cancel", "cancelar", "stop"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(manager.gameObject);
            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);

            Log("=== ASSIGNMENT COMPLETE ===");
        }

        private static MonoBehaviour FindCashBattleManager()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
            {
                if (mb.GetType().Name == "CashBattleManager")
                    return mb;
            }
            return null;
        }

        private static void AssignReference(SerializedObject so, string propertyName, Object value)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null)
            {
                AddResult(propertyName, "Property not found", false, null);
                Log($"? Property not found: {propertyName}");
                failedCount++;
                return;
            }

            if (prop.objectReferenceValue != null)
            {
                AddResult(propertyName, "Already Set", true, prop.objectReferenceValue);
                Log($"● {propertyName}: Already set to {prop.objectReferenceValue.name}");
                alreadySetCount++;
                return;
            }

            if (value != null)
            {
                prop.objectReferenceValue = value;
                AddResult(propertyName, "Assigned", true, value);
                Log($"+ {propertyName}: Assigned {value.name}");
                assignedCount++;
            }
            else
            {
                AddResult(propertyName, "Not found in scene", false, null);
                Log($"✗ {propertyName}: Not found in scene");
                failedCount++;
            }
        }

        #endregion

        #region Finders

        private static T FindByNameContains<T>(params string[] patterns) where T : Component
        {
            var all = Object.FindObjectsOfType<T>(true);
            foreach (var pattern in patterns)
            {
                foreach (var obj in all)
                {
                    if (obj.gameObject.name.ToLower().Contains(pattern.ToLower()))
                        return obj;
                }
            }
            return null;
        }

        private static TextMeshProUGUI FindTextByName(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<TextMeshProUGUI>(true);
            foreach (var pattern in patterns)
            {
                foreach (var text in all)
                {
                    if (text.gameObject.name.ToLower().Contains(pattern.ToLower()))
                        return text;
                }
            }
            return null;
        }

        private static Button FindButtonByName(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<Button>(true);
            foreach (var pattern in patterns)
            {
                foreach (var btn in all)
                {
                    if (btn.gameObject.name.ToLower().Contains(pattern.ToLower()))
                        return btn;
                }
            }
            return null;
        }

        private static FieldInfo GetField(object obj, string fieldName)
        {
            var type = obj.GetType();
            return type.GetField(fieldName,
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        }

        #endregion

        #region Helpers

        private static void ResetLog()
        {
            log = "";
            assignedCount = 0;
            failedCount = 0;
            alreadySetCount = 0;
            results.Clear();
        }

        private static void Log(string message)
        {
            log += message + "\n";
            Debug.Log($"[CashBattleHubReferenceAssigner] {message}");
        }

        private static void AddResult(string field, string status, bool success, Object obj)
        {
            results.Add(new ReferenceResult
            {
                fieldName = field,
                status = status,
                success = success,
                assignedObject = obj
            });
        }

        #endregion
    }
}
