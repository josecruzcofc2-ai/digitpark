using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;

namespace DigitPark.Editor.AutoAssigners
{
    /// <summary>
    /// Reference Assigner for Settings scene.
    /// Automatically finds and assigns UI references to SettingsManager.
    ///
    /// Menu: DigitPark/Auto Assigners/References/Core/Settings References
    /// </summary>
    public class SettingsReferenceAssigner : EditorWindow
    {
        private Vector2 scrollPosition;
        private static string log = "";
        private static int assignedCount = 0;
        private static int failedCount = 0;
        private static int alreadySetCount = 0;
        private static List<ReferenceResult> results = new List<ReferenceResult>();

        private static readonly string[] REQUIRED_REFS = {
            // Panel
            "settingsPanel", "titleText",
            // Volume
            "soundVolumeSlider", "soundValueText", "effectsVolumeSlider", "effectsValueText",
            // Language
            "languageDropdown", "changeLangLabel",
            // Theme
            "themeDropdown", "changeThemeLabel",
            // Buttons
            "changeNameButton", "changeNameCostText", "copyIDButton",
            "logoutButton", "deleteAccountButton", "backButton",
            // Shop
            "shopButton",
            // Premium Section
            "premiumSection", "removeAdsButton", "removeAdsButtonText",
            "premiumFullButton", "premiumFullButtonText", "restorePurchasesButton",
            // Premium Quick Access
            "premiumButton", "premiumBadge", "premiumPanel",
            // Panels (prefab-based)
            "changeNamePanel", "deleteConfirmPanel", "logoutConfirmPanel",
            "errorPanel", "selfExclusionConfirmPanel",
            // Legal
            "termsButton", "privacyButton", "responsibleGamingButton",
            "triumphTermsButton", "selfExclusionButton"
            // Note: languageStyler - assigned at runtime
        };

        private struct ReferenceResult
        {
            public string fieldName;
            public string status;
            public bool success;
            public Object assignedObject;
        }

        #region Menu Items

        [MenuItem("DigitPark/Auto Assigners/References/Core/Settings References", false, 111)]
        public static void ShowWindow()
        {
            var window = GetWindow<SettingsReferenceAssigner>("Settings Reference Assigner");
            window.minSize = new Vector2(600, 500);
        }

        /// <summary>
        /// Public entry point for auto-assigning references (called from SettingsUIBuilder).
        /// </summary>
        public static void RunAutoAssign()
        {
            ResetLog();
            AssignAllReferences();
        }

        #endregion

        #region Window GUI

        private void OnGUI()
        {
            GUILayout.Label("Settings Scene Reference Assigner", EditorStyles.boldLabel);
            GUILayout.Space(10);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "Settings")
            {
                EditorGUILayout.HelpBox(
                    $"Current scene: {currentScene}\n" +
                    "Please open the Settings scene first!",
                    MessageType.Warning);
            }

            EditorGUILayout.HelpBox(
                "Assigns UI references to SettingsManager:\n" +
                "• Volume sliders (sound, effects)\n" +
                "• Language and Theme dropdowns\n" +
                "• Action buttons (name, logout, delete)\n" +
                "• Legal buttons",
                MessageType.Info);

            GUILayout.Space(10);

            MonoBehaviour targetManager = FindSettingsManager();
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
            Log("=== ASSIGNING SETTINGS REFERENCES ===");

            var manager = FindSettingsManager();
            if (manager == null)
            {
                Log("ERROR: SettingsManager not found in scene!");
                failedCount = REQUIRED_REFS.Length;
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            so.Update();

            // Panel
            AssignReference(so, "settingsPanel", FindByNameContains<Transform>("settingspanel", "settings", "panel"));
            AssignReference(so, "titleText", FindTextByName("title", "header", "settings"));

            // Volume
            AssignReference(so, "soundVolumeSlider", FindByNameContains<Slider>("sound", "music", "volumen"));
            AssignReference(so, "soundValueText", FindTextByName("soundvalue", "musicvalue"));
            AssignReference(so, "effectsVolumeSlider", FindByNameContains<Slider>("effects", "sfx", "efectos"));
            AssignReference(so, "effectsValueText", FindTextByName("effectsvalue", "sfxvalue"));

            // Language
            AssignReference(so, "languageDropdown", FindByNameContains<TMP_Dropdown>("language", "idioma", "lang"));
            AssignReference(so, "changeLangLabel", FindTextByName("changelang", "language", "idioma"));

            // Theme
            AssignReference(so, "themeDropdown", FindByNameContains<TMP_Dropdown>("theme", "tema"));
            AssignReference(so, "changeThemeLabel", FindTextByName("changetheme", "theme", "tema"));

            // Buttons
            AssignReference(so, "changeNameButton", FindButtonByName("changename", "name", "nombre"));
            AssignReference(so, "changeNameCostText", FindTextByName("changenamecosttext"));
            AssignReference(so, "copyIDButton", FindButtonByName("copybutton", "copy"));
            AssignReference(so, "logoutButton", FindButtonByName("logout", "signout", "salir"));
            AssignReference(so, "deleteAccountButton", FindButtonByName("delete", "eliminar", "borrar"));
            AssignReference(so, "backButton", FindButtonByName("back", "return", "atras"));

            // Shop
            AssignReference(so, "shopButton", FindButtonByName("shop", "tienda", "store"));

            // Premium Section
            AssignReference(so, "premiumSection", FindByNameContains<Transform>("premiumsection", "premiumcard"));
            AssignReference(so, "removeAdsButton", FindButtonByName("removeads", "noads", "quitaranuncios"));
            AssignReference(so, "removeAdsButtonText", FindTextByName("removeadsbuttontext", "removeadstext"));
            AssignReference(so, "premiumFullButton", FindButtonByName("premiumfull"));
            AssignReference(so, "premiumFullButtonText", FindTextByName("premiumfullbuttontext", "premiumfull"));
            AssignReference(so, "restorePurchasesButton", FindButtonByName("restore", "restaurar"));

            // Premium Quick Access
            AssignReference(so, "premiumButton", FindButtonByName("premiumbutton", "pro"));
            AssignReference(so, "premiumBadge", FindByNameContains<Transform>("premiumbadge", "badge"));
            AssignMonoBehaviour(so, "premiumPanel", "PremiumPanelUI");

            // Panels (prefab-based components in hierarchy)
            AssignMonoBehaviour(so, "changeNamePanel", "InputPanelUI");
            AssignMonoBehaviour(so, "deleteConfirmPanel", "ConfirmPanelUI", "deleteconfirm", "confirmdelete");
            AssignMonoBehaviour(so, "logoutConfirmPanel", "ConfirmPanelUI", "logoutconfirm", "confirmlogout");
            AssignMonoBehaviour(so, "errorPanel", "ErrorPanelUI");
            AssignMonoBehaviour(so, "selfExclusionConfirmPanel", "ConfirmPanelUI", "selfexclusion", "exclusion");

            // Legal
            AssignReference(so, "termsButton", FindButtonByName("terms", "terminos", "tos"));
            AssignReference(so, "privacyButton", FindButtonByName("privacy", "privacidad"));
            AssignReference(so, "responsibleGamingButton", FindButtonByName("responsible", "responsable"));
            AssignReference(so, "triumphTermsButton", FindButtonByName("triumph"));
            AssignReference(so, "selfExclusionButton", FindButtonByName("selfexclusion", "exclusion", "autoexclusion"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(manager.gameObject);
            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
            Log("=== ASSIGNMENT COMPLETE ===");
        }

        private static MonoBehaviour FindSettingsManager()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "SettingsManager") return mb;
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

        private static void AssignMonoBehaviour(SerializedObject so, string propertyName, string typeName, params string[] namePatterns)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) { AddResult(propertyName, "Property not found", false, null); failedCount++; return; }
            if (prop.objectReferenceValue != null) { AddResult(propertyName, "Already Set", true, prop.objectReferenceValue); alreadySetCount++; return; }
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
            {
                if (mb.GetType().Name == typeName)
                {
                    if (namePatterns.Length == 0)
                    {
                        prop.objectReferenceValue = mb;
                        AddResult(propertyName, "Assigned", true, mb);
                        assignedCount++;
                        return;
                    }
                    string goName = mb.gameObject.name.ToLower();
                    foreach (var p in namePatterns)
                        if (goName.Contains(p.ToLower()))
                        {
                            prop.objectReferenceValue = mb;
                            AddResult(propertyName, "Assigned", true, mb);
                            assignedCount++;
                            return;
                        }
                }
            }
            AddResult(propertyName, "Not found", false, null); failedCount++;
        }

        #endregion

        #region Helpers

        private static void ResetLog() { log = ""; assignedCount = 0; failedCount = 0; alreadySetCount = 0; results.Clear(); }
        private static void Log(string msg) { log += msg + "\n"; Debug.Log($"[SettingsReferenceAssigner] {msg}"); }
        private static void AddResult(string f, string s, bool ok, Object o) { results.Add(new ReferenceResult { fieldName = f, status = s, success = ok, assignedObject = o }); }

        #endregion
    }
}
