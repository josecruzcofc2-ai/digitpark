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
    /// Reference Assigner for Shop scene.
    /// Automatically finds and assigns UI references to ShopManager.
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
            // Tabs (5 tabs V2)
            "_featuredTabButton", "_gemsTabButton", "_coinsTabButton", "_themesTabButton", "_cosmeticsTabButton",
            // Content panels (5 content areas V2)
            "_featuredContent", "_gemsContent", "_coinsContent", "_themesContent", "_cosmeticsContent",
            // Popups
            "_purchasePopup", "_notEnoughGemsPopup",
            // Popup UI
            "_popupItemName", "_popupItemPrice",
            "_popupConfirmButton", "_popupCancelButton",
            "_notEnoughCloseButton", "_notEnoughGetGemsButton",
            // Navigation
            "_backButton",
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

        [MenuItem("DigitPark/Auto Assigners/References/Monetization/Shop References", false, 277)]
        public static void ShowWindow()
        {
            var window = GetWindow<ShopReferenceAssigner>("Shop Reference Assigner");
            window.minSize = new Vector2(600, 550);
        }

        #endregion

        #region Window GUI

        private void OnGUI()
        {
            GUILayout.Label("Shop Scene Reference Assigner", EditorStyles.boldLabel);
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
                "Assigns UI references to ShopManager:\n" +
                "• Tabs (featured, gems, coins, themes, cosmetics)\n" +
                "• Content panels for each tab\n" +
                "• Purchase and Not Enough Gems popups\n" +
                "• Currency display texts",
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
            Log("=== ASSIGNING SHOP REFERENCES ===");

            var manager = FindShopManager();
            if (manager == null)
            {
                Log("ERROR: ShopManager not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            so.Update();

            // Use FindMainCanvas as root for deep search
            Canvas canvas = FindMainCanvas();
            Transform root = canvas != null ? canvas.transform : manager.transform.root;

            // Tab Buttons (V2 UIBuilder creates: FeaturedTab, GemsTab, CoinsTab, ThemesTab, CosmeticsTab)
            AssignReference(so, "_featuredTabButton", FindButtonByName("featuredtab", "featured", "destacado"));
            AssignReference(so, "_gemsTabButton", FindButtonByName("gemstab", "gems", "gemas"));
            AssignReference(so, "_coinsTabButton", FindButtonByName("coinstab", "coins", "monedas"));
            AssignReference(so, "_themesTabButton", FindButtonByName("themestab", "themes", "temas"));
            AssignReference(so, "_cosmeticsTabButton", FindButtonByName("cosmeticstab", "cosmetics", "cosmeticos"));

            // Content Panels - V2: each tab has its own ScrollView
            AssignReference(so, "_featuredContent", FindGameObjectByName("featuredscrollview", "featuredcontent", "featuredpanel"));
            AssignReference(so, "_gemsContent", FindGameObjectByName("gemsscrollview", "gemscontent", "gemssection", "gemspanel"));
            AssignReference(so, "_coinsContent", FindGameObjectByName("coinsscrollview", "coinscontent", "coinssection", "coinspanel"));
            AssignReference(so, "_themesContent", FindGameObjectByName("themesscrollview", "themescontent", "themespanel"));
            AssignReference(so, "_cosmeticsContent", FindGameObjectByName("cosmeticsscrollview", "cosmeticscontent", "cosmeticspanel"));

            // Popups - fields are GameObject, use FindGameObjectByName
            // UIBuilder creates: PurchasePopup (inside PurchaseBlocker), NotEnoughPopup (inside NotEnoughBlocker)
            AssignReference(so, "_purchasePopup", FindGameObjectByName("purchasepopup", "purchaseblocker", "confirmpopup", "buypopup"));
            AssignReference(so, "_notEnoughGemsPopup", FindGameObjectByName("notenoughpopup", "notenoughblocker", "notenough", "needgems"));

            // Popup UI - find within popup context
            // UIBuilder creates generic names: Title, Amount, Price, etc. inside popups
            // Search for specific popup children first, then fall back to generic
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

            // Navigation
            AssignReference(so, "_backButton", FindButtonByName("backbutton", "back", "return", "atras"));

            // Currency Display
            // UIBuilder creates Amount text inside GemsDisplay/CoinsDisplay containers
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
                    // Prefer the instance that is NOT on a Canvas object
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

        private static T FindByNameContains<T>(params string[] patterns) where T : Component
        {
            var all = Object.FindObjectsOfType<T>(true);
            foreach (var p in patterns) foreach (var o in all) if (o.gameObject.name.ToLower().Contains(p.ToLower())) return o;
            return null;
        }

        /// <summary>
        /// Finds a GameObject by name pattern (for GameObject-type serialized fields)
        /// </summary>
        private static GameObject FindGameObjectByName(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<Transform>(true);
            foreach (var p in patterns) foreach (var t in all) if (t.gameObject.name.ToLower().Contains(p.ToLower())) return t.gameObject;
            return null;
        }

        private static TextMeshProUGUI FindTextByName(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<TextMeshProUGUI>(true);
            foreach (var p in patterns) foreach (var t in all) if (t.gameObject.name.ToLower().Contains(p.ToLower())) return t;
            return null;
        }

        /// <summary>
        /// Finds a TextMeshProUGUI within a specific parent hierarchy
        /// </summary>
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

        /// <summary>
        /// Finds a Button within a specific parent hierarchy
        /// </summary>
        private static Button FindButtonInParent(Transform parent, params string[] patterns)
        {
            var buttons = parent.GetComponentsInChildren<Button>(true);
            foreach (var p in patterns) foreach (var b in buttons) if (b.gameObject.name.ToLower().Contains(p.ToLower())) return b;
            return null;
        }

        #endregion

        #region Helpers

        private static void ResetLog() { log = ""; assignedCount = 0; failedCount = 0; alreadySetCount = 0; results.Clear(); }
        private static void Log(string msg) { log += msg + "\n"; Debug.Log($"[ShopReferenceAssigner] {msg}"); }
        private static void AddResult(string f, string s, bool ok, Object o) { results.Add(new ReferenceResult { fieldName = f, status = s, success = ok, assignedObject = o }); }

        #endregion
    }
}
