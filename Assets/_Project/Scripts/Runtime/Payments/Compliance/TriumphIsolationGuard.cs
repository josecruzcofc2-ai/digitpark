using UnityEngine;
using DigitPark.Payments;

namespace DigitPark.Payments.Compliance
{
    /// <summary>
    /// Monitor runtime que verifica que no hay contaminación cruzada entre Triumph y Stripe/AppleIAP.
    /// Solo activo en versión Pro.
    /// </summary>
    public class TriumphIsolationGuard : MonoBehaviour
    {
        private static TriumphIsolationGuard _instance;
        public static TriumphIsolationGuard Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<TriumphIsolationGuard>();
                return _instance;
            }
        }

        private const float AUDIT_INTERVAL = 60f;
        private bool _triumphActive = false;
        private bool _isolationBroken = false;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
#if DIGIT_PARK_PRO || UNITY_EDITOR
            Debug.Log("[IsolationGuard] Monitor de aislamiento Triumph↔Stripe iniciado");
            VerifyIsolation();
            InvokeRepeating(nameof(PeriodicAudit), AUDIT_INTERVAL, AUDIT_INTERVAL);
#else
            Debug.Log("[IsolationGuard] Versión Global — sin riesgo de contaminación Triumph");
#endif
        }

        private void OnDestroy()
        {
            CancelInvoke();
        }

        public void RegisterTriumphActive(bool active)
        {
            _triumphActive = active;
            Debug.Log($"[IsolationGuard] Triumph activo: {active}");
        }

        public bool VerifyIsolation()
        {
            bool isolated = true;

            // Verificar que PaymentManager no tiene referencias a Triumph
            var paymentManager = PaymentManager.Instance;
            if (paymentManager != null)
            {
                // Check: el provider activo no es de tipo "triumph"
                var provider = paymentManager.GetActiveProvider();
                if (provider != PaymentProvider.Stripe && provider != PaymentProvider.AppleIAP && provider != PaymentProvider.None)
                {
                    Debug.LogError("[IsolationGuard] CRÍTICO: Provider activo no es Stripe ni AppleIAP");
                    isolated = false;
                }
            }

            // Verificar compliance de catálogo
            if (!ProductCatalog.ValidateCatalogCompliance())
            {
                Debug.LogError("[IsolationGuard] CRÍTICO: Catálogo de productos contiene términos prohibidos");
                isolated = false;
            }

            if (!isolated && !_isolationBroken)
            {
                _isolationBroken = true;
                Debug.LogError("[IsolationGuard] AISLAMIENTO COMPROMETIDO — Ejecutando abort protocol");
                StripeAbortProtocol.ExecuteAbort(AbortReason.CrossContamination);
            }

            WriteAuditLog(isolated);
            return isolated;
        }

        public bool ValidateTransactionIsolation(PaymentResult result)
        {
            if (result == null) return false;

            // El transactionId nunca debe empezar con prefijos de Triumph
            if (!string.IsNullOrEmpty(result.TransactionId))
            {
                if (result.TransactionId.StartsWith("triumph_") ||
                    result.TransactionId.StartsWith("tmatch_"))
                {
                    Debug.LogError($"[IsolationGuard] CRÍTICO: TransactionId parece ser de Triumph: {result.TransactionId}");
                    return false;
                }
            }

            // El provider debe ser Stripe o AppleIAP
            if (result.ProviderUsed != PaymentProvider.Stripe &&
                result.ProviderUsed != PaymentProvider.AppleIAP &&
                result.ProviderUsed != PaymentProvider.None)
            {
                Debug.LogError($"[IsolationGuard] Provider no reconocido: {result.ProviderUsed}");
                return false;
            }

            // El producto debe estar en el catálogo
            var product = ProductCatalog.FindProduct(result.ProductId);
            if (product == null && result.ProductId != "restore" && result.ProductId != "restore_complete")
            {
                Debug.LogWarning($"[IsolationGuard] ProductId no encontrado en catálogo: {result.ProductId}");
            }

            return true;
        }

        private void PeriodicAudit()
        {
            VerifyIsolation();
        }

        private void WriteAuditLog(bool isolated)
        {
            string triumphState = _triumphActive ? "ON" : "OFF";
            string stripeState = PaymentFeatureFlag.IsStripeEnabled ? "ON" : "OFF";
            string contamination = isolated ? "NONE" : "DETECTED";
            string log = $"Triumph: {triumphState} | Stripe: {stripeState} | Cross-contamination: {contamination} | {System.DateTime.UtcNow:O}";
            PlayerPrefs.SetString("isolation_audit_latest", log);
            PlayerPrefs.Save();
            Debug.Log($"[IsolationGuard] Audit: {log}");
        }
    }
}
