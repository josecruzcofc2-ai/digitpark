using UnityEngine;

namespace DigitPark.Payments
{
    /// <summary>
    /// Configuración de URLs para Firebase Cloud Functions.
    /// Cada campo apunta directamente a su Cloud Function específica.
    /// Formato: https://us-central1-TU-PROYECTO.cloudfunctions.net/nombreFuncion
    ///
    /// NOTA: appleSharedSecret NO va aquí — vive en Firebase Secret Manager (server-side).
    /// NOTA: stripeSecretKey NO va aquí — vive en Firebase Secret Manager (server-side).
    /// Solo el Stripe Publishable Key es seguro en el cliente.
    /// </summary>
    [System.Serializable]
    public class PaymentConfig
    {
        [Header("Stripe (cliente — seguro en Unity)")]
        public string stripePublishableKey;

        [Header("Firebase Cloud Functions URLs")]
        // Obtener estas URLs tras ejecutar: firebase deploy --only functions
        public string stripeCreateCheckoutUrl;      // .../stripeCreateCheckout
        public string stripeSessionStatusUrl;       // .../stripeSessionStatus
        public string stripeWebhookUrl;             // .../stripeWebhook (solo referencia)
        public string iapValidateReceiptUrl;        // .../iapValidateReceipt
        public string getEntitlementsUrl;           // .../getEntitlements
        public string syncEntitlementsUrl;          // .../syncEntitlements
        public string paymentsHealthUrl;            // .../paymentsHealth
        public string adminForceSwitchUrl;          // .../adminForceSwitch

        [Header("Timeouts y Reintentos")]
        public int stripeCheckoutTimeoutSeconds = 300;
        public int stripePollingIntervalMs = 2000;
        public int maxStripeRetries = 3;
        public float sessionPollingTimeoutMinutes = 5f;

        /// <summary>
        /// URL base derivada de stripeCreateCheckoutUrl (para health checks).
        /// Ej: https://us-central1-PROYECTO.cloudfunctions.net
        /// </summary>
        public string BackendBaseUrl
        {
            get
            {
                if (string.IsNullOrEmpty(paymentsHealthUrl)) return "";
                int idx = paymentsHealthUrl.LastIndexOf('/');
                return idx > 0 ? paymentsHealthUrl.Substring(0, idx) : paymentsHealthUrl;
            }
        }
    }
}
