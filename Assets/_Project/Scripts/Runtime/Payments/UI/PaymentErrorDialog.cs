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

            // Intentar usar PopupManager si existe
            // PopupManager.Instance?.ShowPopup("Error", message, buttonText, onButton);

            // Fallback: Dialog nativo de Unity (solo editor)
#if UNITY_EDITOR
            UnityEditor.EditorUtility.DisplayDialog("Payment Error", message, buttonText ?? "OK");
#endif
        }
    }
}
