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
    /// Reference Assigner for Scores/Leaderboard scene.
    /// Automatically finds and assigns UI references to LeaderboardManager.
    /// Includes Game Selector buttons (5 juegos, sin CognitiveSprint).
    ///
    /// Menu: DigitPark/Auto Assigners/References/Social/Leaderboard References
    /// </summary>
    public class LeaderboardReferenceAssigner : EditorWindow
    {
        private Vector2 scrollPosition;
        private static string log = "";
        private static int assignedCount = 0;
        private static int failedCount = 0;
        private static int alreadySetCount = 0;
        private static List<ReferenceResult> results = new List<ReferenceResult>();

        // 5 juegos con ranking (sin CognitiveSprint)
        private static readonly string[] GAME_IDS = {
            "DigitRush", "FlashTap", "MemoryPairs", "OddOneOut", "QuickMath"
        };

        private static readonly string[] REQUIRED_REFS = {
            // Game Selector
            "gameButtons", "gameButtonBackgrounds", "gameButtonIcons",
            // Tabs
            "nacionalTab", "mundialTab",
            // Leaderboard
            "leaderboardContainer", "scrollRect",
            // Loading
            "loadingPanel", "loadingText",
            // Navigation
            "backButton",
            // Empty State
            "emptyState", "playButton",
            // Player Position Panel
            "playerPositionPanel", "positionNumberText", "positionTimeText"
            // Note: leaderboardEntryPrefab requires manual prefab assignment
        };

        private struct ReferenceResult
        {
            public string fieldName;
            public string status;
            public bool success;
            public Object assignedObject;
        }

        #region Menu Items

        [MenuItem("DigitPark/Scenes/Assign References/Social/Leaderboard", false, 152)]
        public static void ShowWindow()
        {
            var window = GetWindow<LeaderboardReferenceAssigner>("Leaderboard Reference Assigner");
            window.minSize = new Vector2(600, 500);
        }

        #endregion

        #region Window GUI

        private void OnGUI()
        {
            GUILayout.Label("Leaderboard (Scores) Scene Reference Assigner", EditorStyles.boldLabel);
            GUILayout.Space(10);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "Scores")
            {
                EditorGUILayout.HelpBox(
                    $"Current scene: {currentScene}\n" +
                    "Please open the Scores scene first!",
                    MessageType.Warning);
                GUILayout.Space(10);
            }

            EditorGUILayout.HelpBox(
                "Assigns UI references to LeaderboardManager:\n" +
                "• Game Selector (5 game buttons with icons)\n" +
                "• Tabs (nacional, mundial)\n" +
                "• Leaderboard container and scroll\n" +
                "• Loading panel\n" +
                "• Player Position panel\n" +
                "• Back button",
                MessageType.Info);

            GUILayout.Space(10);

            MonoBehaviour targetManager = FindLeaderboardManager();

            if (targetManager == null)
            {
                EditorGUILayout.HelpBox(
                    "LeaderboardManager not found in scene!\n" +
                    "Add a LeaderboardManager component to assign references.",
                    MessageType.Error);
            }
            else
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
        }

        #endregion

        /// <summary>
        /// Método estático para ejecutar desde otros scripts (ej. ScoresUIBuilder)
        /// </summary>
        public static void RunAutoAssign()
        {
            ResetLog();
            AssignAllReferences();
        }

        #region Assignment Logic

        private static void AssignAllReferences()
        {
            Log("=== ASSIGNING LEADERBOARD REFERENCES ===");

            var manager = FindLeaderboardManager();
            if (manager == null)
            {
                Log("ERROR: LeaderboardManager not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            so.Update();

            // ====== GAME SELECTOR (arrays de 5 elementos) ======
            AssignGameSelectorArrays(so);

            // ====== TABS ======
            AssignReference(so, "nacionalTab", FindButtonByName("nationaltab", "nacionaltab", "nacional", "national", "local", "country"));
            AssignReference(so, "mundialTab", FindButtonByName("globaltab", "mundialtab", "mundial", "global", "world"));

            // ====== LEADERBOARD ======
            AssignReference(so, "leaderboardContainer", FindByNameContains<Transform>("leaderboardcontainer", "leaderboard", "entries", "container"));
            AssignReference(so, "scrollRect", FindByNameContains<ScrollRect>("scroll", "leaderboardscroll"));

            // ====== LOADING ======
            AssignReference(so, "loadingPanel", FindByNameContains<Transform>("loadingpanel", "loading"));
            AssignReference(so, "loadingText", FindTextByName("loadingtext", "loading", "cargando"));

            // ====== NAVIGATION ======
            AssignReference(so, "backButton", FindButtonByName("back", "return", "backbutton"));

            // ====== EMPTY STATE ======
            AssignReference(so, "emptyState", FindByNameContains<Transform>("emptystate", "empty", "nodata"));
            AssignReference(so, "playButton", FindButtonByName("playbutton", "play", "jugar"));

            // ====== PLAYER POSITION PANEL ======
            AssignGameObject(so, "playerPositionPanel", "playerposition", "positionpanel");
            AssignReference(so, "positionNumberText", FindTextByName("positionnumber", "positionnum", "ranknum"));
            AssignReference(so, "positionTimeText", FindTextByName("positiontime", "ranktime", "postime"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(manager.gameObject);
            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
            Log("=== ASSIGNMENT COMPLETE ===");
        }

        /// <summary>
        /// Busca GameButton_DigitRush, GameButton_FlashTap, etc. y asigna los arrays
        /// gameButtons (Button[]), gameButtonBackgrounds (Image[]), gameButtonIcons (Image[])
        /// </summary>
        private static void AssignGameSelectorArrays(SerializedObject so)
        {
            var buttonsProp = so.FindProperty("gameButtons");
            var bgsProp = so.FindProperty("gameButtonBackgrounds");
            var iconsProp = so.FindProperty("gameButtonIcons");

            if (buttonsProp == null) { AddResult("gameButtons", "Property not found", false, null); failedCount++; return; }
            if (bgsProp == null) { AddResult("gameButtonBackgrounds", "Property not found", false, null); failedCount++; return; }
            if (iconsProp == null) { AddResult("gameButtonIcons", "Property not found", false, null); failedCount++; return; }

            // Resize arrays to 5
            buttonsProp.arraySize = GAME_IDS.Length;
            bgsProp.arraySize = GAME_IDS.Length;
            iconsProp.arraySize = GAME_IDS.Length;

            int foundButtons = 0;
            int foundBgs = 0;
            int foundIcons = 0;

            for (int i = 0; i < GAME_IDS.Length; i++)
            {
                string searchName = $"GameButton_{GAME_IDS[i]}";

                // Buscar el GameObject del botón
                Transform btnTransform = FindTransformByExactName(searchName);
                if (btnTransform != null)
                {
                    // Button component
                    Button btn = btnTransform.GetComponent<Button>();
                    if (btn != null)
                    {
                        buttonsProp.GetArrayElementAtIndex(i).objectReferenceValue = btn;
                        foundButtons++;
                    }

                    // Background Image (es el Image del propio botón)
                    Image bg = btnTransform.GetComponent<Image>();
                    if (bg != null)
                    {
                        bgsProp.GetArrayElementAtIndex(i).objectReferenceValue = bg;
                        foundBgs++;
                    }

                    // Icon Image (hijo llamado "Icon")
                    Transform iconT = btnTransform.Find("Icon");
                    if (iconT != null)
                    {
                        Image iconImg = iconT.GetComponent<Image>();
                        if (iconImg != null)
                        {
                            iconsProp.GetArrayElementAtIndex(i).objectReferenceValue = iconImg;
                            foundIcons++;
                        }
                    }
                }
                else
                {
                    Log($"WARNING: {searchName} not found in scene");
                }
            }

            AddResult("gameButtons", foundButtons == GAME_IDS.Length ? $"Assigned ({foundButtons})" : $"Partial ({foundButtons}/{GAME_IDS.Length})", foundButtons > 0, null);
            if (foundButtons > 0) assignedCount++; else failedCount++;

            AddResult("gameButtonBackgrounds", foundBgs == GAME_IDS.Length ? $"Assigned ({foundBgs})" : $"Partial ({foundBgs}/{GAME_IDS.Length})", foundBgs > 0, null);
            if (foundBgs > 0) assignedCount++; else failedCount++;

            AddResult("gameButtonIcons", foundIcons == GAME_IDS.Length ? $"Assigned ({foundIcons})" : $"Partial ({foundIcons}/{GAME_IDS.Length})", foundIcons > 0, null);
            if (foundIcons > 0) assignedCount++; else failedCount++;
        }

        private static MonoBehaviour FindLeaderboardManager()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "LeaderboardManager") return mb;
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

        private static Transform FindTransformByExactName(string name)
        {
            var all = Object.FindObjectsOfType<Transform>(true);
            foreach (var t in all)
                if (t.gameObject.name == name) return t;
            return null;
        }

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

        private static void AssignGameObject(SerializedObject so, string propertyName, params string[] patterns)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { AddResult(propertyName, "Property not found", false, null); failedCount++; return; }
            if (prop.objectReferenceValue != null) { AddResult(propertyName, "Already Set", true, prop.objectReferenceValue); alreadySetCount++; return; }
            var all = Object.FindObjectsOfType<Transform>(true);
            foreach (var p in patterns)
                foreach (var o in all)
                    if (o.gameObject.name.ToLower().Contains(p.ToLower()))
                    {
                        prop.objectReferenceValue = o.gameObject;
                        AddResult(propertyName, "Assigned", true, o.gameObject);
                        assignedCount++;
                        return;
                    }
            AddResult(propertyName, "Not found", false, null); failedCount++;
        }

        #endregion

        #region Helpers

        private static void ResetLog() { log = ""; assignedCount = 0; failedCount = 0; alreadySetCount = 0; results.Clear(); }
        private static void Log(string msg) { log += msg + "\n"; Debug.Log($"[LeaderboardReferenceAssigner] {msg}"); }
        private static void AddResult(string f, string s, bool ok, Object o) { results.Add(new ReferenceResult { fieldName = f, status = s, success = ok, assignedObject = o }); }

        #endregion
    }
}
