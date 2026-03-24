using UnityEngine;

namespace DigitPark.Payments
{
    /// <summary>
    /// Configuración de URLs para Firebase Cloud Functions.
    /// Cada campo apunta directamente a su Cloud Function específica.
    /// Formato: https://us-central1-TU-PROYECTO.cloudfunctions.net/nombreFuncion
    ///
    /// NOTA: appleSharedSecret NO va aquí — vive en Firebase Secret Manager (server-side).
    /// </summary>
    [System.Serializable]
    public class PaymentConfig
    {
        [Header("Firebase Cloud Functions URLs")]
        // Obtener estas URLs tras ejecutar: firebase deploy --only functions
        public string iapValidateReceiptUrl;        // .../iapValidateReceipt
        public string getEntitlementsUrl;           // .../getEntitlements
        public string syncEntitlementsUrl;          // .../syncEntitlements
        public string paymentsHealthUrl;            // .../paymentsHealth

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
