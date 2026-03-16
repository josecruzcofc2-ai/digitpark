using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;

namespace DigitPark.Editor.AutoAssigners
{
    /// <summary>
    /// Reference Assigner for BetSelection scene.
    /// Automatically finds and assigns UI references to BetSelectionPanel.
    /// Supports: Free + 5 DigitCoin tiers + Custom bet section + Action buttons.
    ///
    /// Menu: DigitPark/Auto Assigners/References/Games/BetSelection References
    /// </summary>
    public class BetSelectionReferenceAssigner : EditorWindow
    {
        private Vector2 scrollPosition;
        private static string log = "";
        private static int assignedCount = 0;
        private static int failedCount = 0;
        private static int alreadySetCount = 0;
        private static List<ReferenceResult> results = new List<ReferenceResult>();

        private static readonly string[] REQUIRED_REFS = {
            // Navigation
            "_backButton",
            // Header
            "_titleText", "_gameNameText",
            // Currency Display
            "_gemsValueText", "_coinsValueText", "_gemsLabel", "_coinsLabel",
            // Free Bet
            "_freeBetButton", "_freeBetCostText", "_freeBetRewardText",
            // Coin Bets
            "_coins50Button", "_coins50CostText", "_coins50RewardText",
            "_coins100Button", "_coins100CostText", "_coins100RewardText",
            "_coins250Button", "_coins250CostText", "_coins250RewardText",
            "_coins500Button", "_coins500CostText", "_coins500RewardText",
            "_coins1000Button", "_coins1000CostText", "_coins1000RewardText",
            // Custom Bet
            "_customBetCardBg", "_customCoinsToggle",
            "_customAmountInput", "_customMinusButton", "_customPlusButton",
            "_customRewardText",
            // Rounds Selection
            "_roundsPanel", "_rounds1Button", "_rounds3Button", "_rounds5Button",
            // Action Buttons
            "_playButton", "_cancelButton"
        };

        private struct ReferenceResult
        {
            public string fieldName;
            public string status;
            public bool success;
            public Object assignedObject;
        }

        #region Menu Items

        [MenuItem("DigitPark/Scenes/Assign References/Games/BetSelection", false, 128)]
        public static void ShowWindow()
        {
            var window = GetWindow<BetSelectionReferenceAssigner>("BetSelection Reference Assigner");
            window.minSize = new Vector2(600, 600);
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
            GUILayout.Label("BetSelection Scene Reference Assigner", EditorStyles.boldLabel);
            GUILayout.Space(10);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "BetSelection")
            {
                EditorGUILayout.HelpBox(
                    $"Current scene: {currentScene}\n" +
                    "Please open the BetSelection scene first!",
                    MessageType.Warning);
            }

            EditorGUILayout.HelpBox(
                "Assigns UI references to BetSelectionPanel:\n" +
                "- Header (title, game name)\n" +
                "- Currency display (DigitGems, DigitCoins labels + values)\n" +
                "- Free bet (button, cost, reward)\n" +
                "- DigitCoin bets: 50, 100, 250, 500, 1000\n" +
                "- Custom bet (card bg, input, stepper, preview)\n" +
                "- Action buttons (play, cancel)",
                MessageType.Info);

            GUILayout.Space(10);

            MonoBehaviour targetManager = FindBetSelectionPanel();
            if (targetManager != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Target:", GUILayout.Width(50));
                EditorGUILayout.ObjectField(targetManager, typeof(MonoBehaviour), true);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "BetSelectionPanel not found in scene!\n" +
                    "Add the BetSelectionPanel component to a GameObject first.",
                    MessageType.Error);
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

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(400));
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
                GUILayout.Label(result.success ? (result.status == "Already Set" ? "=" : "+") : "X", GUILayout.Width(20));
                GUI.color = Color.white;
                GUILayout.Label(result.fieldName, GUILayout.Width(220));
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

        public static void AssignAllReferences()
        {
            Log("=== ASSIGNING BETSELECTION REFERENCES ===");

            var manager = FindBetSelectionPanel();
            if (manager == null)
            {
                Log("ERROR: BetSelectionPanel not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            so.Update();

            // Navigation
            AssignReference(so, "_backButton", FindButtonByName("backbutton", "back", "return", "atras"));

            // Header
            AssignReference(so, "_titleText", FindTextByName("titletext", "title"));
            AssignReference(so, "_gameNameText", FindTextByName("gamenametext", "gamename"));

            // Currency Display
            AssignReference(so, "_gemsValueText", FindTextByName("gemsvaluetext", "gemsvalue"));
            AssignReference(so, "_coinsValueText", FindTextByName("coinsvaluetext", "coinsvalue"));
            AssignReference(so, "_gemsLabel", FindTextByName("gemslabel"));
            AssignReference(so, "_coinsLabel", FindTextByName("coinslabel"));

            // Free Bet
            AssignReference(so, "_freeBetButton", FindButtonByParentName("freebetopt", "freebet"));
            AssignReference(so, "_freeBetCostText", FindTextByName("freebetcosttext", "freebetcost"));
            AssignReference(so, "_freeBetRewardText", FindTextByName("freebetrewardtext", "freebetreward"));

            // Coin 50
            AssignReference(so, "_coins50Button", FindButtonByParentName("coins50bet", "coins50"));
            AssignReference(so, "_coins50CostText", FindTextByName("coins50costtext", "coins50cost"));
            AssignReference(so, "_coins50RewardText", FindTextByName("coins50rewardtext", "coins50reward"));

            // Coin 100
            AssignReference(so, "_coins100Button", FindButtonByParentName("coins100bet", "coins100"));
            AssignReference(so, "_coins100CostText", FindTextByName("coins100costtext", "coins100cost"));
            AssignReference(so, "_coins100RewardText", FindTextByName("coins100rewardtext", "coins100reward"));

            // Coin 250
            AssignReference(so, "_coins250Button", FindButtonByParentName("coins250bet", "coins250"));
            AssignReference(so, "_coins250CostText", FindTextByName("coins250costtext", "coins250cost"));
            AssignReference(so, "_coins250RewardText", FindTextByName("coins250rewardtext", "coins250reward"));

            // Coin 500
            AssignReference(so, "_coins500Button", FindButtonByParentName("coins500bet", "coins500"));
            AssignReference(so, "_coins500CostText", FindTextByName("coins500costtext", "coins500cost"));
            AssignReference(so, "_coins500RewardText", FindTextByName("coins500rewardtext", "coins500reward"));

            // Coin 1000
            AssignReference(so, "_coins1000Button", FindButtonByParentName("coins1000bet", "coins1000"));
            AssignReference(so, "_coins1000CostText", FindTextByName("coins1000costtext", "coins1000cost"));
            AssignReference(so, "_coins1000RewardText", FindTextByName("coins1000rewardtext", "coins1000reward"));

            // Custom Bet
            AssignReference(so, "_customBetCardBg", FindImageByName("custombetcard"));
            AssignReference(so, "_customCoinsToggle", FindButtonByName("customcoinstoggle"));
            AssignReference(so, "_customAmountInput", FindInputFieldByName("customamountinput"));
            AssignReference(so, "_customMinusButton", FindButtonByName("customminusbutton", "customminus"));
            AssignReference(so, "_customPlusButton", FindButtonByName("customplusbutton", "customplus"));
            AssignReference(so, "_customRewardText", FindTextByName("customrewardtext"));

            // Rounds Selection
            AssignReference(so, "_roundsPanel", FindImageByName("roundspanel"));
            AssignReference(so, "_rounds1Button", FindButtonByName("rounds1button"));
            AssignReference(so, "_rounds3Button", FindButtonByName("rounds3button"));
            AssignReference(so, "_rounds5Button", FindButtonByName("rounds5button"));

            // Action Buttons
            AssignReference(so, "_playButton", FindButtonByName("playbutton", "play"));
            AssignReference(so, "_cancelButton", FindButtonByName("cancelbutton", "cancel"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(manager.gameObject);
            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
            Log("=== ASSIGNMENT COMPLETE ===");
        }

        public static MonoBehaviour FindBetSelectionPanel()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "BetSelectionPanel") return mb;
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

        private static TextMeshProUGUI FindTextByName(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<TextMeshProUGUI>(true);
            foreach (var p in patterns)
                foreach (var t in all)
                    if (t.gameObject.name.ToLower().Replace(" ", "").Contains(p.ToLower()))
                        return t;
            return null;
        }

        private static Button FindButtonByName(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<Button>(true);
            foreach (var p in patterns)
                foreach (var b in all)
                    if (b.gameObject.name.ToLower().Replace(" ", "").Contains(p.ToLower()))
                        return b;
            return null;
        }

        private static Button FindButtonByParentName(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<Button>(true);
            foreach (var p in patterns)
                foreach (var b in all)
                    if (b.gameObject.name.ToLower().Replace(" ", "").Contains(p.ToLower()))
                        return b;
            return null;
        }

        private static Image FindImageByName(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<Image>(true);
            foreach (var p in patterns)
                foreach (var i in all)
                    if (i.gameObject.name.ToLower().Replace(" ", "").Contains(p.ToLower()))
                        return i;
            return null;
        }

        private static TMP_InputField FindInputFieldByName(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<TMP_InputField>(true);
            foreach (var p in patterns)
                foreach (var inp in all)
                    if (inp.gameObject.name.ToLower().Replace(" ", "").Contains(p.ToLower()))
                        return inp;
            return null;
        }

        #endregion

        #region Helpers

        private static void ResetLog() { log = ""; assignedCount = 0; failedCount = 0; alreadySetCount = 0; results.Clear(); }
        private static void Log(string msg) { log += msg + "\n"; Debug.Log($"[BetSelectionReferenceAssigner] {msg}"); }
        private static void AddResult(string f, string s, bool ok, Object o) { results.Add(new ReferenceResult { fieldName = f, status = s, success = ok, assignedObject = o }); }

        #endregion
    }
}
