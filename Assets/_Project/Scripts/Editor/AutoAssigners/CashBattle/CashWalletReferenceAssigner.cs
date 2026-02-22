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
    /// Reference Assigner for CashWallet scene.
    /// Automatically finds and assigns UI references to CashWalletSceneController.
    ///
    /// Menu: DigitPark/Auto Assigners/References/CashBattle/CashWallet References
    /// </summary>
    public class CashWalletReferenceAssigner : EditorWindow
    {
        private Vector2 scrollPosition;
        private static string log = "";
        private static int assignedCount = 0;
        private static int failedCount = 0;
        private static int alreadySetCount = 0;
        private static List<ReferenceResult> results = new List<ReferenceResult>();

        private static readonly string[] REQUIRED_REFS = {
            // Header
            "backButton", "balanceText", "bonusBalanceText",
            // Tab Buttons
            "depositTabButton", "withdrawTabButton", "historyTabButton",
            // Tab Panels
            "depositPanel", "withdrawPanel", "transactionHistoryPanel",
            // Deposit Section
            "depositOptionsContainer", "paymentMethodsContainer",
            // Withdraw Section
            "withdrawAmountInput", "withdrawButton", "withdrawableAmountText",
            "withdrawMinText", "withdrawFeeText", "kycRequiredPanel", "verifyKycButton",
            // Transaction History
            "transactionsContainer", "emptyHistoryText", "loadMoreButton",
            // Overlays
            "loadingOverlay", "successOverlay", "errorOverlay", "errorMessageText"
        };

        private struct ReferenceResult
        {
            public string fieldName;
            public string status;
            public bool success;
            public Object assignedObject;
        }

        #region Menu Items

        [MenuItem("DigitPark/Auto Assigners/References/CashBattle/CashWallet References", false, 243)]
        public static void ShowWindow()
        {
            var window = GetWindow<CashWalletReferenceAssigner>("CashWallet Reference Assigner");
            window.minSize = new Vector2(600, 500);
        }

        #endregion

        #region Window GUI

        private void OnGUI()
        {
            GUILayout.Label("CashWallet Scene Reference Assigner", EditorStyles.boldLabel);
            GUILayout.Space(10);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "CashWallet")
            {
                EditorGUILayout.HelpBox(
                    $"Current scene: {currentScene}\n" +
                    "Please open the CashWallet scene first!",
                    MessageType.Warning);
            }

            EditorGUILayout.HelpBox(
                "Assigns UI references to CashWalletSceneController:\n" +
                "- Header (back button, balance, bonus balance)\n" +
                "- Tab buttons (deposit, withdraw, history)\n" +
                "- Tab panels (deposit, withdraw, transaction history)\n" +
                "- Deposit section (options container, payment methods)\n" +
                "- Withdraw section (input, button, texts, KYC)\n" +
                "- Transaction history (container, empty text, load more)\n" +
                "- Overlays (loading, success, error)",
                MessageType.Info);

            GUILayout.Space(10);

            MonoBehaviour targetController = FindCashWalletSceneController();
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
            Log("=== ASSIGNING CASHWALLET REFERENCES ===");

            var controller = FindCashWalletSceneController();
            if (controller == null)
            {
                Log("ERROR: CashWalletSceneController not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            SerializedObject so = new SerializedObject(controller);
            so.Update();

            Canvas canvas = FindMainCanvas();
            Transform root = canvas != null ? canvas.transform : controller.transform.root;

            // ==================== HEADER ====================
            Transform backButtonT = FindDeep(root, "BackButton");
            AssignReference(so, "backButton", backButtonT != null ? backButtonT.GetComponent<Button>() : null);

            AssignReference(so, "balanceText", FindTextByDeep(root, "BalanceText"));
            AssignReference(so, "bonusBalanceText", FindTextByDeep(root, "BonusBalanceText"));

            // ==================== TAB BUTTONS ====================
            Transform depositTabT = FindDeep(root, "DepositTabButton");
            if (depositTabT == null) depositTabT = FindDeep(root, "TabDeposits");
            if (depositTabT == null) depositTabT = FindDeep(root, "DepositTab");
            AssignReference(so, "depositTabButton", depositTabT != null ? depositTabT.GetComponent<Button>() : null);

            Transform withdrawTabT = FindDeep(root, "WithdrawTabButton");
            if (withdrawTabT == null) withdrawTabT = FindDeep(root, "TabWithdrawals");
            if (withdrawTabT == null) withdrawTabT = FindDeep(root, "WithdrawTab");
            AssignReference(so, "withdrawTabButton", withdrawTabT != null ? withdrawTabT.GetComponent<Button>() : null);

            Transform historyTabT = FindDeep(root, "HistoryTabButton");
            if (historyTabT == null) historyTabT = FindDeep(root, "HistoryTab");
            if (historyTabT == null) historyTabT = FindDeep(root, "TabAll");
            AssignReference(so, "historyTabButton", historyTabT != null ? historyTabT.GetComponent<Button>() : null);

            // ==================== TAB PANELS ====================
            Transform depositPanelT = FindDeep(root, "DepositPanel");
            AssignReference(so, "depositPanel", depositPanelT != null ? depositPanelT.gameObject : null);

            Transform withdrawPanelT = FindDeep(root, "WithdrawPanel");
            AssignReference(so, "withdrawPanel", withdrawPanelT != null ? withdrawPanelT.gameObject : null);

            Transform txHistoryPanelT = FindDeep(root, "TransactionHistoryPanel");
            if (txHistoryPanelT == null) txHistoryPanelT = FindDeep(root, "HistoryPanel");
            AssignReference(so, "transactionHistoryPanel", txHistoryPanelT != null ? txHistoryPanelT.gameObject : null);

            // ==================== DEPOSIT SECTION ====================
            // Prefabs (desde disco)
            var depositPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/CashBattle/Wallet/DepositOptionUI.prefab");
            AssignReference(so, "depositOptionPrefab", depositPrefab);
            var txItemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/CashBattle/Wallet/TransactionItemUI.prefab");
            AssignReference(so, "transactionItemPrefab", txItemPrefab);
            // paymentMethodButtons -> skip (array, complex)
            Transform depositOptionsContainerT = FindDeep(root, "DepositOptionsContainer");
            AssignReference(so, "depositOptionsContainer", depositOptionsContainerT);

            Transform paymentMethodsContainerT = FindDeep(root, "PaymentMethodsContainer");
            AssignReference(so, "paymentMethodsContainer", paymentMethodsContainerT != null ? paymentMethodsContainerT.gameObject : null);

            // ==================== WITHDRAW SECTION ====================
            Transform withdrawAmountInputT = FindDeep(root, "WithdrawAmountInput");
            AssignReference(so, "withdrawAmountInput", withdrawAmountInputT != null ? withdrawAmountInputT.GetComponent<TMP_InputField>() : null);

            Transform withdrawButtonT = FindDeep(root, "WithdrawButton");
            AssignReference(so, "withdrawButton", withdrawButtonT != null ? withdrawButtonT.GetComponent<Button>() : null);

            AssignReference(so, "withdrawableAmountText", FindTextByDeep(root, "WithdrawableAmountText"));
            AssignReference(so, "withdrawMinText", FindTextByDeep(root, "WithdrawMinText"));
            AssignReference(so, "withdrawFeeText", FindTextByDeep(root, "WithdrawFeeText"));

            Transform kycRequiredPanelT = FindDeep(root, "KycRequiredPanel");
            AssignReference(so, "kycRequiredPanel", kycRequiredPanelT != null ? kycRequiredPanelT.gameObject : null);

            Transform verifyKycButtonT = FindDeep(root, "VerifyKycButton");
            AssignReference(so, "verifyKycButton", verifyKycButtonT != null ? verifyKycButtonT.GetComponent<Button>() : null);

            // ==================== TRANSACTION HISTORY ====================
            // transactionItemPrefab -> skip (prefab)
            Transform transactionsContainerT = FindDeep(root, "TransactionsContainer");
            if (transactionsContainerT == null)
            {
                // UIBuilder creates Content inside TransactionsList/ScrollView/Viewport
                Transform txList = FindDeep(root, "TransactionsList");
                if (txList != null)
                {
                    Transform viewport = FindDeep(txList, "Viewport");
                    if (viewport != null) transactionsContainerT = FindDeep(viewport, "Content");
                }
            }
            AssignReference(so, "transactionsContainer", transactionsContainerT);

            Transform emptyHistoryTextT = FindDeep(root, "EmptyHistoryText");
            if (emptyHistoryTextT == null) emptyHistoryTextT = FindDeep(root, "EmptyStateText");
            if (emptyHistoryTextT == null) emptyHistoryTextT = FindDeep(root, "EmptyText");
            AssignReference(so, "emptyHistoryText", emptyHistoryTextT != null ? emptyHistoryTextT.GetComponent<TextMeshProUGUI>() : null);

            Transform loadMoreButtonT = FindDeep(root, "LoadMoreButton");
            AssignReference(so, "loadMoreButton", loadMoreButtonT != null ? loadMoreButtonT.GetComponent<Button>() : null);

            // ==================== OVERLAYS ====================
            Transform loadingOverlayT = FindDeep(root, "LoadingOverlay");
            AssignReference(so, "loadingOverlay", loadingOverlayT != null ? loadingOverlayT.gameObject : null);

            Transform successOverlayT = FindDeep(root, "SuccessOverlay");
            AssignReference(so, "successOverlay", successOverlayT != null ? successOverlayT.gameObject : null);

            Transform errorOverlayT = FindDeep(root, "ErrorOverlay");
            AssignReference(so, "errorOverlay", errorOverlayT != null ? errorOverlayT.gameObject : null);

            AssignReference(so, "errorMessageText", FindTextByDeep(root, "ErrorMessageText"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(controller.gameObject);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            Log("=== ASSIGNMENT COMPLETE ===");
        }

        private static MonoBehaviour FindCashWalletSceneController()
        {
            MonoBehaviour fallback = null;
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
            {
                if (mb.GetType().Name == "CashWalletSceneController")
                {
                    // Prefer the instance that is NOT on a Canvas object
                    if (mb.GetComponent<Canvas>() == null)
                        return mb;
                    fallback = mb;
                }
            }
            return fallback;
        }

        private static Canvas FindMainCanvas()
        {
            return UIBuilderCanvasHelper.FindMainCanvas();
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
        private static void Log(string msg) { log += msg + "\n"; Debug.Log($"[CashWalletReferenceAssigner] {msg}"); }
        private static void AddResult(string f, string s, bool ok, Object o) { results.Add(new ReferenceResult { fieldName = f, status = s, success = ok, assignedObject = o }); }

        #endregion
    }
}
