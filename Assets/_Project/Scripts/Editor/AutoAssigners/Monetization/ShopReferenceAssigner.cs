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
    /// Reference Assigner for Shop scene V4.
    /// Automatically finds and assigns UI references to ShopManager.
    /// Continuous scroll layout — all sections in one view.
    ///
    /// Menu: DigitPark/Auto Assigners/References/Monetization/Shop References
    /// </summary>
    public class ShopReferenceAssigner : EditorWindow
    {
        private Vector2 scrollPosition;
        private static string log = "";
        private static int assignedCount = 0;
        private static int failedCount = 0;
        private static int alreadySetCount = 0;
        private static List<ReferenceResult> results = new List<ReferenceResult>();

        private static readonly string[] REQUIRED_REFS = {
            // Scroll View
            "_shopScrollView",
            // Popups
            "_purchasePopup", "_notEnoughGemsPopup",
            // Popup UI
            "_popupItemName", "_popupItemPrice",
            "_popupConfirmButton", "_popupCancelButton",
            "_notEnoughCloseButton", "_notEnoughGetGemsButton",
            // Navigation
            "_backButton",
            // Entrance Animation
            "_headerTransform", "_scrollViewTransform",
            // Currency
            "_headerGemsText", "_headerCoinsText"
        };

        private struct ReferenceResult
        {
            public string fieldName;
            public string status;
            public bool success;
            public Object assignedObject;
        }

        #region Menu Items

        [MenuItem("DigitPark/Scenes/Assign References/Monetization/Shop", false, 143)]
        public static void ShowWindow()
        {
            var window = GetWindow<ShopReferenceAssigner>("Shop Reference Assigner");
            window.minSize = new Vector2(600, 550);
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
            GUILayout.Label("Shop Scene Reference Assigner V4 (Continuous Scroll)", EditorStyles.boldLabel);
            GUILayout.Space(10);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "Shop")
            {
                EditorGUILayout.HelpBox(
                    $"Current scene: {currentScene}\n" +
                    "Please open the Shop scene first!",
                    MessageType.Warning);
            }

            EditorGUILayout.HelpBox(
                "Assigns UI references to ShopManager V4:\n" +
                "• ShopScrollView (continuous scroll)\n" +
                "• Purchase and Not Enough DigitGems popups\n" +
                "• Currency display texts\n" +
                "• Back button + entrance animation transforms",
                MessageType.Info);

            GUILayout.Space(10);

            MonoBehaviour targetManager = FindShopManager();
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

        /// <summary>
        /// Ejecuta la asignacion de todas las referencias.
        /// Internal para que ShopPremiumUIBuilder pueda invocarlo automaticamente.
        /// </summary>
        internal static void AssignAllReferences()
        {
            Log("=== ASSIGNING SHOP V4 REFERENCES (CONTINUOUS SCROLL) ===");

            var manager = FindShopManager();
            if (manager == null)
            {
                Log("ERROR: ShopManager not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            so.Update();

            Canvas canvas = FindMainCanvas();
            Transform root = canvas != null ? canvas.transform : manager.transform.root;

            // ========== SCROLL VIEW ==========
            Transform scrollViewT = FindDeep(root, "ShopScrollView");
            AssignReference(so, "_shopScrollView", scrollViewT != null ? scrollViewT.gameObject : FindGameObjectByName("shopscrollview"));

            // ========== POPUPS ==========
            AssignReference(so, "_purchasePopup", FindGameObjectByName("purchasepopup", "purchaseblocker", "confirmpopup", "buypopup"));
            AssignReference(so, "_notEnoughGemsPopup", FindGameObjectByName("notenoughpopup", "notenoughblocker", "notenough", "needgems"));

            Transform purchasePopupT = FindDeep(root, "PurchasePopup");
            if (purchasePopupT != null)
            {
                AssignReference(so, "_popupItemName", FindTextInParent(purchasePopupT, "amount", "itemname", "popupitemname"));
                AssignReference(so, "_popupItemPrice", FindTextInParent(purchasePopupT, "price", "itemprice", "popupitemprice"));
                AssignReference(so, "_popupConfirmButton", FindButtonInParent(purchasePopupT, "confirmbutton", "confirm", "comprar", "buy"));
                AssignReference(so, "_popupCancelButton", FindButtonInParent(purchasePopupT, "cancelbutton", "cancel", "cancelar"));
            }
            else
            {
                AssignReference(so, "_popupItemName", FindTextByName("popupitemname", "itemname", "popuptitle"));
                AssignReference(so, "_popupItemPrice", FindTextByName("popupitemprice", "itemprice"));
                AssignReference(so, "_popupConfirmButton", FindButtonByName("popupconfirm", "confirmbutton", "confirm"));
                AssignReference(so, "_popupCancelButton", FindButtonByName("popupcancel", "cancelbutton"));
            }

            Transform notEnoughPopupT = FindDeep(root, "NotEnoughPopup");
            if (notEnoughPopupT != null)
            {
                AssignReference(so, "_notEnoughCloseButton", FindButtonInParent(notEnoughPopupT, "closebutton", "close", "cerrar"));
                AssignReference(so, "_notEnoughGetGemsButton", FindButtonInParent(notEnoughPopupT, "getgemsbutton", "getgems", "obtener"));
            }
            else
            {
                AssignReference(so, "_notEnoughCloseButton", FindButtonByName("notenoughclose", "closebutton", "cerrar"));
                AssignReference(so, "_notEnoughGetGemsButton", FindButtonByName("getgemsbutton", "getgems", "obtener"));
            }

            // ========== NAVIGATION ==========
            AssignReference(so, "_backButton", FindButtonByName("backbutton", "back", "return", "atras"));

            // ========== ENTRANCE ANIMATION ==========
            Transform headerT = FindDeep(root, "Header");
            AssignReference(so, "_headerTransform", headerT != null ? (Object)headerT.GetComponent<RectTransform>() : null);
            // ScrollView transform for entrance animation
            if (scrollViewT == null) scrollViewT = FindDeep(root, "ShopScrollView");
            AssignReference(so, "_scrollViewTransform", scrollViewT != null ? (Object)scrollViewT.GetComponent<RectTransform>() : null);

            // ========== CURRENCY DISPLAY ==========
            Transform gemsDisplayT = FindDeep(root, "GemsDisplay");
            Transform coinsDisplayT = FindDeep(root, "CoinsDisplay");
            TextMeshProUGUI gemsText = gemsDisplayT != null ? FindTextInParent(gemsDisplayT, "amount", "gemstext", "gemsvalue") : null;
            TextMeshProUGUI coinsText = coinsDisplayT != null ? FindTextInParent(coinsDisplayT, "amount", "coinstext", "coinsvalue") : null;
            if (gemsText == null) gemsText = FindTextByName("headergems", "gemstext", "gemsvalue");
            if (coinsText == null) coinsText = FindTextByName("headercoins", "coinstext", "coinsvalue");
            AssignReference(so, "_headerGemsText", gemsText);
            AssignReference(so, "_headerCoinsText", coinsText);

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(manager.gameObject);
            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
            Log("=== ASSIGNMENT COMPLETE ===");
        }

        private static MonoBehaviour FindShopManager()
        {
            MonoBehaviour fallback = null;
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
            {
                if (mb.GetType().Name == "ShopManager")
                {
                    if (mb.GetComponent<Canvas>() == null)
                        return mb;
                    fallback = mb;
                }
            }
            return fallback;
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

        private static GameObject FindGameObjectByName(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<Transform>(true);
            foreach (var p in patterns) foreach (var t in all) if (t.gameObject.name.ToLower().Contains(p.ToLower())) return t.gameObject;
            return null;
        }

        private static GameObject FindGameObjectInChildren(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name) return child.gameObject;
            }
            return null;
        }

        private static TextMeshProUGUI FindTextByName(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<TextMeshProUGUI>(true);
            foreach (var p in patterns) foreach (var t in all) if (t.gameObject.name.ToLower().Contains(p.ToLower())) return t;
            return null;
        }

        private static TextMeshProUGUI FindTextInParent(Transform parent, params string[] patterns)
        {
            var texts = parent.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var p in patterns) foreach (var t in texts) if (t.gameObject.name.ToLower().Contains(p.ToLower())) return t;
            return null;
        }

        private static Button FindButtonByName(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<Button>(true);
            foreach (var p in patterns) foreach (var b in all) if (b.gameObject.name.ToLower().Contains(p.ToLower())) return b;
            return null;
        }

        private static Button FindButtonInParent(Transform parent, params string[] patterns)
        {
            var buttons = parent.GetComponentsInChildren<Button>(true);
            foreach (var p in patterns) foreach (var b in buttons) if (b.gameObject.name.ToLower().Contains(p.ToLower())) return b;
            return null;
        }

        #endregion

        #region Helpers

        internal static void ResetLog() { log = ""; assignedCount = 0; failedCount = 0; alreadySetCount = 0; results.Clear(); }
        private static void Log(string msg) { log += msg + "\n"; Debug.Log($"[ShopReferenceAssigner] {msg}"); }
        private static void AddResult(string f, string s, bool ok, Object o) { results.Add(new ReferenceResult { fieldName = f, status = s, success = ok, assignedObject = o }); }

        #endregion
    }
}
