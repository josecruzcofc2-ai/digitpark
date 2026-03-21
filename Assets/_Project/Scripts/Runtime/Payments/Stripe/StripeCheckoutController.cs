using UnityEngine;

namespace DigitPark.Payments.Stripe
{
#if HAS_STRIPE || UNITY_EDITOR
    /// <summary>
    /// Controla la apertura del checkout en Safari View Controller (iOS)
    /// o en el browser del sistema (Editor/Android).
    /// Maneja deep links de retorno digitpark://stripe-return?session_id=xxx
    /// </summary>
    public class StripeCheckoutController
    {
        private string _pendingSessionId;
        private System.Action<bool, string> _onCheckoutComplete;

        public void OpenCheckout(string checkoutUrl, string sessionId, System.Action<bool, string> onComplete)
        {
            _pendingSessionId = sessionId;
            _onCheckoutComplete = onComplete;

            // Registrar handler de deep link para retorno de Stripe
            PaymentBridge.RegisterDeepLinkHandler?.Invoke("stripe-return", OnStripeReturn);

            Debug.Log($"[StripeCheckout] Abriendo checkout: {checkoutUrl}");

#if UNITY_IOS && !UNITY_EDITOR
            // En iOS: usar SFSafariViewController via plugin nativo si está disponible
            // Por ahora fallback a Application.OpenURL
            OpenIOSCheckout(checkoutUrl);
#else
            // Editor / Android: abrir en browser del sistema
            Application.OpenURL(checkoutUrl);
#endif
        }

        /// <summary>
        /// Llama este método cuando la sesión Stripe termina (éxito, cancelación o timeout)
        /// para desregistrar el deep link handler y evitar acumulación entre compras (WARN-01).
        /// </summary>
        public void CleanupCheckout()
        {
            PaymentBridge.UnregisterDeepLinkHandler?.Invoke("stripe-return");
            _onCheckoutComplete = null;
            _pendingSessionId = null;
        }

        private void OnStripeReturn(string deepLink)
        {
            Debug.Log($"[StripeCheckout] Deep link recibido: {deepLink}");

            // Extraer session_id del query string
            string sessionId = ExtractQueryParam(deepLink, "session_id");

            if (sessionId == _pendingSessionId)
            {
                Debug.Log("[StripeCheckout] Checkout completado via deep link");
                _onCheckoutComplete?.Invoke(true, sessionId);
                CleanupCheckout();
            }
            else
            {
                Debug.LogWarning($"[StripeCheckout] Session ID no coincide: {sessionId} != {_pendingSessionId}");
            }
        }

        private string ExtractQueryParam(string url, string paramName)
        {
            if (string.IsNullOrEmpty(url)) return null;
            int idx = url.IndexOf($"{paramName}=");
            if (idx < 0) return null;
            int start = idx + paramName.Length + 1;
            int end = url.IndexOf('&', start);
            return end < 0 ? url.Substring(start) : url.Substring(start, end - start);
        }

#if UNITY_IOS && !UNITY_EDITOR
        private void OpenIOSCheckout(string url)
        {
            // S17-NEW-01: SFSafariViewControllerBridge.mm no implementado — usar Application.OpenURL
            // La URL proviene de nuestro backend Stripe (fuente de confianza)
            Application.OpenURL(url);
        }
#endif
    }
#endif
}
