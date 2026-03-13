#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using DigitPark.Payments;

namespace DigitPark.Editor.Payments
{
    /// <summary>
    /// Ventana de debugging para el sistema de pagos.
    /// Menú: DigitPark/Payment Debug
    /// </summary>
    public class PaymentDebugWindow : EditorWindow
    {
        [MenuItem("DigitPark/Payment Debug Window")]
        public static void ShowWindow()
        {
            GetWindow<PaymentDebugWindow>("Payment Debug");
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Payment System Debugger", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Entra en Play Mode para usar el debugger.", MessageType.Info);
                if (GUILayout.Button("Validar Catálogo de Productos"))
                {
                    bool valid = ProductCatalog.ValidateCatalogCompliance();
                    EditorUtility.DisplayDialog("Validación",
                        valid ? "Catálogo OK — sin términos prohibidos" : "ERROR — Catálogo tiene violaciones",
                        "OK");
                }
                return;
            }

            // Estado en Play Mode
            bool flagsInit = PaymentFeatureFlag.IsInitialized;
            EditorGUILayout.LabelField($"Feature Flags: {(flagsInit ? "Inicializado" : "Sin inicializar")}");

            if (flagsInit)
            {
                EditorGUILayout.LabelField($"Provider Activo: {PaymentFeatureFlag.ActiveCosmeticProvider}");
                EditorGUILayout.LabelField($"Stripe Habilitado: {PaymentFeatureFlag.IsStripeEnabled}");
                EditorGUILayout.LabelField($"AppleIAP Habilitado: {PaymentFeatureFlag.IsAppleIAPEnabled}");
                EditorGUILayout.LabelField($"Triumph Habilitado: {PaymentFeatureFlag.IsTriumphEnabled}");
                EditorGUILayout.LabelField($"Maintenance Mode: {PaymentFeatureFlag.IsMaintenanceMode}");
                EditorGUILayout.LabelField($"Versión App: {PaymentFeatureFlag.CurrentVersion}");
            }

            EditorGUILayout.Space(10);

            // PaymentManager
            var pm = PaymentManager.Instance;
            EditorGUILayout.LabelField($"PaymentManager: {(pm != null ? "Activo" : "Nulo")}");
            if (pm != null)
            {
                EditorGUILayout.LabelField($"  Inicializado: {pm.IsInitialized}");
                EditorGUILayout.LabelField($"  Provider Activo: {pm.GetActiveProvider()}");
            }

            EditorGUILayout.Space(10);

            // Acciones de debug
            EditorGUILayout.LabelField("Acciones de Debug:", EditorStyles.boldLabel);

            if (GUILayout.Button("Forzar Switch → Apple IAP"))
            {
                PaymentFeatureFlag.ForceSwitch(PaymentProvider.AppleIAP, "debug_manual");
            }

            if (GUILayout.Button("Forzar Switch → Stripe"))
            {
                PaymentFeatureFlag.ForceSwitch(PaymentProvider.Stripe, "debug_manual");
            }

            if (GUILayout.Button("Simular Abort Protocol"))
            {
                StripeAbortProtocol.ExecuteAbort(AbortReason.ManualDeveloperTrigger);
            }

            if (GUILayout.Button("Reset Abort Protocol"))
            {
                StripeAbortProtocol.Reset();
            }

            if (GUILayout.Button("Validar Catálogo"))
            {
                bool valid = ProductCatalog.ValidateCatalogCompliance();
                Debug.Log($"[Debug] Catálogo válido: {valid}");
            }

            EditorGUILayout.Space(5);

            // Audit log
            string auditLog = PlayerPrefs.GetString("isolation_audit_latest", "Sin datos");
            EditorGUILayout.LabelField("Último Audit Log:");
            EditorGUILayout.HelpBox(auditLog, MessageType.None);
        }
    }
}
#endif
