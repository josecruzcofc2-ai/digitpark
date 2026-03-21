using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using DigitPark.Payments.Compliance;

namespace DigitPark.Payments.Stripe
{
#if HAS_STRIPE || UNITY_EDITOR
    /// <summary>
    /// Provider de pagos via Stripe Checkout.
    /// Flujo: PaymentManager → StripePaymentProvider → Backend → Stripe → Deep Link Return
    /// NUNCA contacta a Triumph. NUNCA procesa dinero real de torneos.
    /// </summary>
    public class StripePaymentProvider : IPaymentProvider
    {
        public string ProviderName => "Stripe";
        public bool IsAvailable => _isAvailable;
        public bool IsHealthy => _isHealthy;

        private bool _isAvailable = false;
        private bool _isHealthy = false;
        private PaymentConfig _config;
        private StripeCheckoutController _checkoutController;
        private StripeSessionPoller _poller;

        public async Task Initialize(PaymentConfig config)
        {
            _config = config;
            _checkoutController = new StripeCheckoutController();

            // Verificar que el backend responde
            bool healthy = await HealthCheck();
            _isAvailable = healthy;
            _isHealthy = healthy;

            Debug.Log($"[StripeProvider] Inicializado. Disponible: {_isAvailable}");
        }

        public async Task<PaymentResult> PurchaseProduct(CosmeticProduct product, string userId)
        {
            // Validación de compliance ANTES de cualquier request
            if (!StripeComplianceGuard.ValidateProduct(product))
            {
                return PaymentResult.Failed(product.ProductId, "compliance_violation",
                    "Producto no cumple con los términos de Stripe", PaymentProvider.Stripe);
            }

            if (!VersionGuard.CanAccessStripe())
            {
                return PaymentResult.Failed(product.ProductId, "version_not_allowed",
                    "Esta versión no tiene acceso a Stripe", PaymentProvider.Stripe);
            }

            // Paso 1: Crear checkout session en el backend
            var sessionData = await CreateCheckoutSession(product, userId);
            if (sessionData == null)
            {
                return PaymentResult.Failed(product.ProductId, "session_creation_failed",
                    "No se pudo crear la sesión de Stripe", PaymentProvider.Stripe);
            }

            Debug.Log($"[StripeProvider] Sesión creada: {sessionData.sessionId}");

            // Paso 2: Abrir checkout
            var pollerGO = StripeSessionPoller.GetOrCreate();

            // Abrir en Safari/browser. Callback null es intencional — el resultado
            // se obtiene via PollUntilComplete (deep link), no via callback síncrono.
            _checkoutController.OpenCheckout(sessionData.checkoutUrl, sessionData.sessionId, null);

            // Paso 3: Poll hasta completado
            var status = await pollerGO.PollUntilComplete(
                sessionData.sessionId,
                _config.stripeSessionStatusUrl,
                _config.sessionPollingTimeoutMinutes,
                _config.stripePollingIntervalMs / 1000f
            );

            // WARN-01: limpiar deep link handler cuando el poller termina (sea cual sea el resultado)
            _checkoutController.CleanupCheckout();

            switch (status)
            {
                case StripeSessionStatus.Completed:
                    Debug.Log($"[StripeProvider] Pago completado: {product.ProductId}");
                    return PaymentResult.Successful(product.ProductId, sessionData.sessionId, PaymentProvider.Stripe);

                case StripeSessionStatus.Canceled:
                    return PaymentResult.Failed(product.ProductId, "user_canceled",
                        "El usuario canceló el pago", PaymentProvider.Stripe);

                case StripeSessionStatus.Expired:
                    return PaymentResult.Failed(product.ProductId, "session_expired",
                        "La sesión de pago expiró", PaymentProvider.Stripe);

                default:
                    return PaymentResult.Failed(product.ProductId, "stripe_error",
                        "Error en el proceso de pago", PaymentProvider.Stripe);
            }
        }

        public async Task<PaymentResult> RestorePurchases(string userId)
        {
            // Stripe no necesita restore — las compras se verifican server-side
            Debug.Log("[StripeProvider] Restore no necesario para Stripe (server-side)");
            return PaymentResult.Successful("restore", "stripe_no_restore_needed", PaymentProvider.Stripe);
        }

        public async Task<List<CosmeticProduct>> FetchProducts(List<string> productIds)
        {
            // En Stripe, los productos y precios se definen en el Dashboard
            // Aquí retornamos el catálogo estático
            var result = new List<CosmeticProduct>();
            foreach (var id in productIds)
            {
                var product = ProductCatalog.FindProduct(id);
                if (product != null) result.Add(product);
            }
            return result;
        }

        public async Task<bool> HealthCheck()
        {
            if (string.IsNullOrEmpty(_config?.paymentsHealthUrl))
            {
                Debug.LogWarning("[StripeProvider] paymentsHealthUrl no configurada");
                return false;
            }

            string url = _config.paymentsHealthUrl;
            using (var request = UnityWebRequest.Get(url))
            {
                request.timeout = 5;
                var op = request.SendWebRequest();

                float elapsed = 0f;
                while (!op.isDone && elapsed < 5f)
                {
                    await Task.Yield();
                    elapsed += Time.deltaTime;
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var health = JsonUtility.FromJson<HealthResponse>(request.downloadHandler.text);
                    bool healthy = health?.stripe?.status == "healthy";
                    _isHealthy = healthy;
                    return healthy;
                }
            }

            _isHealthy = false;
            return false;
        }

        public void Dispose()
        {
            Debug.Log("[StripeProvider] Disposed");
        }

        private async Task<CheckoutSessionData> CreateCheckoutSession(
            CosmeticProduct product, string userId)
        {
            // BUG-02: Rechazar productos sin StripePriceId — enviar priceId="" causa 400 en el backend
            if (string.IsNullOrEmpty(product.StripePriceId))
            {
                Debug.LogError($"[StripeProvider] Producto '{product.ProductId}' no tiene StripePriceId. " +
                               "Crea el producto en Stripe Dashboard y asigna el price_xxx a ProductCatalog.");
                return null;
            }

            string url = _config.stripeCreateCheckoutUrl;

            var body = new CheckoutRequestBody
            {
                productId = product.ProductId,
                userId = userId,
                appVersion = PaymentFeatureFlag.IsProVersion ? "pro" : "global",
                priceId = product.StripePriceId,
                metadata_type = "cosmetic",
                metadata_has_tournament_benefit = "false"
            };

            string json = JsonUtility.ToJson(body);
            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);

            // Obtain Firebase ID token for Authorization header
            string idToken = null;
            if (PaymentBridge.GetFirebaseIdToken != null)
            {
                try { idToken = await PaymentBridge.GetFirebaseIdToken(); }
                catch (System.Exception e) { Debug.LogWarning($"[StripeProvider] Failed to get ID token: {e.Message}"); }
            }

            using (var request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("X-App-Version", VersionGuard.GetRequiredAppVersionHeader());
                if (!string.IsNullOrEmpty(idToken))
                    request.SetRequestHeader("Authorization", $"Bearer {idToken}");
                request.timeout = 15;

                var op = request.SendWebRequest();
                while (!op.isDone) await Task.Yield();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    return JsonUtility.FromJson<CheckoutSessionData>(request.downloadHandler.text);
                }

                Debug.LogError($"[StripeProvider] Error creando sesión: {request.error}");
                return null;
            }
        }

        [System.Serializable]
        private class CheckoutRequestBody
        {
            public string productId;
            public string userId;
            public string appVersion;
            public string priceId;
            public string metadata_type;
            public string metadata_has_tournament_benefit;
        }

        [System.Serializable]
        private class CheckoutSessionData
        {
            public string sessionId;
            public string checkoutUrl;
        }

        [System.Serializable]
        private class HealthResponse
        {
            public StripeHealth stripe;

            [System.Serializable]
            public class StripeHealth
            {
                public string status;
            }
        }
    }
#endif
}
