#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DigitPark.Editor.Payments
{
    /// <summary>
    /// Tool para cambiar entre builds Pro (US) y Global (ROW).
    /// Menú: DigitPark/Build Profile Switcher
    /// </summary>
    public class BuildProfileSwitcherWindow : EditorWindow
    {
        private const string PRO_SYMBOLS = "DIGIT_PARK_PRO;HAS_TRIUMPH;HAS_STRIPE;HAS_APPLE_IAP";
        private const string GLOBAL_SYMBOLS = "DIGIT_PARK_GLOBAL;HAS_APPLE_IAP";
        private const string DEV_SYMBOLS = "DIGIT_PARK_PRO;HAS_TRIUMPH;HAS_STRIPE;HAS_APPLE_IAP;UNITY_INCLUDE_TESTS";
        private const string PRO_BUNDLE_ID = "com.matrixsoftware.digitpark.pro";
        private const string GLOBAL_BUNDLE_ID = "com.matrixsoftware.digitpark";

        [MenuItem("DigitPark/Build Profile Switcher")]
        public static void ShowWindow()
        {
            GetWindow<BuildProfileSwitcherWindow>("Build Profile Switcher");
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Digit Park — Build Profile Switcher", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // Estado actual
            var currentGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string currentSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(currentGroup);
            string currentBundle = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS);

            EditorGUILayout.HelpBox(
                $"Versión activa: {DetectCurrentProfile(currentSymbols)}\n" +
                $"Bundle ID: {currentBundle}\n" +
                $"Defines: {currentSymbols}",
                MessageType.Info
            );

            EditorGUILayout.Space(10);

            // Features del perfil
            bool isPro = currentSymbols.Contains("DIGIT_PARK_PRO");
            EditorGUILayout.LabelField("Features activas:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"  Triumph SDK: {(currentSymbols.Contains("HAS_TRIUMPH") ? "✓" : "✗")}");
            EditorGUILayout.LabelField($"  Stripe: {(currentSymbols.Contains("HAS_STRIPE") ? "✓" : "✗")}");
            EditorGUILayout.LabelField($"  Apple IAP: {(currentSymbols.Contains("HAS_APPLE_IAP") ? "✓" : "✗")}");

            EditorGUILayout.Space(15);

            // Botones
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Switch to PRO (US)", GUILayout.Height(40)))
            {
                SwitchToPro(currentGroup);
            }

            if (GUILayout.Button("Switch to GLOBAL (ROW)", GUILayout.Height(40)))
            {
                SwitchToGlobal(currentGroup);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Switch to DEVELOPMENT (All Features + Tests)"))
            {
                SwitchToDev(currentGroup);
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "ADVERTENCIA: Cambiar define symbols causa recompilación completa de Unity.",
                MessageType.Warning
            );
        }

        private string DetectCurrentProfile(string symbols)
        {
            if (symbols.Contains("DIGIT_PARK_PRO")) return "PRO (US)";
            if (symbols.Contains("DIGIT_PARK_GLOBAL")) return "GLOBAL (ROW)";
            return "Sin perfil definido";
        }

        private void SwitchToPro(BuildTargetGroup group)
        {
            if (!EditorUtility.DisplayDialog("Confirmar",
                "¿Cambiar a perfil PRO (US)?\n\nEsto causará recompilación completa.",
                "Sí, cambiar", "Cancelar")) return;

            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, PRO_SYMBOLS);
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, PRO_BUNDLE_ID);
            Debug.Log($"[BuildProfile] Cambiado a PRO. Bundle: {PRO_BUNDLE_ID}");
            EditorUtility.DisplayDialog("Listo", "Perfil PRO activado. Unity recompilará.", "OK");
        }

        private void SwitchToGlobal(BuildTargetGroup group)
        {
            if (!EditorUtility.DisplayDialog("Confirmar",
                "¿Cambiar a perfil GLOBAL (ROW)?\n\nEsto deshabilitará Stripe y Triumph.",
                "Sí, cambiar", "Cancelar")) return;

            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, GLOBAL_SYMBOLS);
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, GLOBAL_BUNDLE_ID);
            Debug.Log($"[BuildProfile] Cambiado a GLOBAL. Bundle: {GLOBAL_BUNDLE_ID}");
            EditorUtility.DisplayDialog("Listo", "Perfil GLOBAL activado. Unity recompilará.", "OK");
        }

        private void SwitchToDev(BuildTargetGroup group)
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, DEV_SYMBOLS);
            Debug.Log("[BuildProfile] Cambiado a DEVELOPMENT (todas las features + tests)");
        }
    }
}
#endif
