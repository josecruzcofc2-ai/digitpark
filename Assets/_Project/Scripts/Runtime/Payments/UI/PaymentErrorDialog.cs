using UnityEngine;

namespace DigitPark.Payments.UI
{
    /// <summary>
    /// Diálogo de error para pagos fallidos.
    /// Casos:
    ///   - Stripe falló pero IAP funcionó → NO mostrar error (seamless fallback)
    ///   - Ambos fallaron → mostrar "payment_unavailable" con Retry
    ///   - Maintenance mode → mostrar "store_maintenance" sin botón
    /// </summary>
    public class PaymentErrorDialog : MonoBehaviour
    {
        /// <summary>
        /// Injected by Assembly-CSharp (e.g. PaymentBridgeWiring) to avoid direct
        /// dependency on PopupManager from this assembly.
        /// Signature: (title, message, buttonText, onButton)
        /// </summary>
        public static System.Action<string, string, string, System.Action> ShowPopupCallback;

        private void OnEnable()
        {
            PaymentEvents.OnPurchaseFailed += OnPurchaseFailed;
        }

        private void OnDisable()
        {
            PaymentEvents.OnPurchaseFailed -= OnPurchaseFailed;
        }

        private void OnPurchaseFailed(PaymentResult result)
        {
            if (this == null) return;
            if (result == null) return;

            // Si hubo fallback exitoso, no mostrar error
            if (result.WasProviderSwitched) return;

            // Maintenance mode
            if (PaymentFeatureFlag.IsMaintenanceMode)
            {
                ShowDialog("Store under maintenance", null, null);
                return;
            }

            // BUG-04: session_expired — el pago puede haberse procesado en Stripe.
            // NO ofrecer Retry (evita doble cargo). Informar al usuario que revise su correo.
            if (result.ErrorCode == "session_expired")
            {
                ShowDialog(
                    "Your payment may be processing. Check your Stripe confirmation email before trying again.",
                    "OK", null);
                return;
            }

            // Error de pago general
            ShowDialog("Payment unavailable", "Retry", OnRetryClicked);
        }

        private string _retryProductId;

        private void OnRetryClicked()
        {
            Debug.Log("[PaymentErrorDialog] Usuario quiere reintentar");
            // El ShopManager/caller debe manejar el retry
        }

        private void ShowDialog(string message, string buttonText, System.Action onButton)
        {
            Debug.Log($"[PaymentErrorDialog] Mostrando error: {message}");

            // B3-B: Route through PopupManager (scene-level UI) via injected delegate
            ShowPopupCallback?.Invoke("Payment Error", message, buttonText ?? "OK", onButton);
        }
    }
}
