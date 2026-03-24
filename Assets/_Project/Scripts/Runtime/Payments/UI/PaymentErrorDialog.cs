using UnityEngine;

namespace DigitPark.Payments.UI
{
    /// <summary>
    /// Diálogo de error para pagos fallidos.
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
