using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace DigitPark.Payments.AppleIAP
{
    /// <summary>
    /// Provider de pagos via Apple IAP (Unity IAP existente).
    /// Wrapper sobre PremiumManager — no re-inicializa Unity IAP.
    /// Este es el FAILSAFE — siempre debe estar disponible.
    /// </summary>
    public class AppleIAPProvider : IPaymentProvider
    {
        /// <summary>
        /// Delegates injected by AppleIAPBridge (Assembly-CSharp) to avoid direct
        /// dependency on PremiumManager from this named assembly.
        /// Set these before Initialize() is called.
        /// </summary>
        public static System.Func<bool> GetIsPremiumAvailable = () => false;
        public static System.Action<string, System.Action<bool, string>> InvokePurchase = null;
        public static System.Action<System.Action> InvokeRestore = null;

        public string ProviderName => "AppleIAP";
        public bool IsAvailable => _isAvailable;
        public bool IsHealthy => _isHealthy;

        private bool _isAvailable = false;
        private bool _isHealthy = false;
        private PaymentConfig _config;
        private AppleReceiptValidator _receiptValidator;

        public async Task Initialize(PaymentConfig config)
        {
            _config = config;
            _receiptValidator = new AppleReceiptValidator();

            // Verificar que PremiumManager existe y está inicializado (via delegate inyectado)
            bool premiumReady = GetIsPremiumAvailable();
            _isAvailable = premiumReady;
            _isHealthy = premiumReady;

            Debug.Log($"[AppleIAP] Inicializado. PremiumManager disponible: {premiumReady}");
            await Task.Yield();
        }

        public async Task<PaymentResult> PurchaseProduct(CosmeticProduct product, string userId)
        {
            if (!_isAvailable || !GetIsPremiumAvailable())
            {
                return PaymentResult.Failed(product.ProductId, "iap_not_available",
                    "Apple IAP no disponible", PaymentProvider.AppleIAP);
            }

            if (InvokePurchase == null)
            {
                Debug.LogError("[AppleIAP] InvokePurchase delegate not set. AppleIAPBridge not initialized.");
                return PaymentResult.Failed(product.ProductId, "iap_bridge_missing",
                    "AppleIAPBridge not initialized", PaymentProvider.AppleIAP);
            }

            string appleProductId = product.AppleProductId;
            if (string.IsNullOrEmpty(appleProductId))
            {
                appleProductId = product.ProductId; // Fallback al ID directo
            }

            Debug.Log($"[AppleIAP] Iniciando compra: {appleProductId}");

            var tcs = new TaskCompletionSource<PaymentResult>();

            InvokePurchase(appleProductId, (success, transactionId) =>
            {
                if (success)
                {
                    Debug.Log($"[AppleIAP] Compra exitosa: {appleProductId}, txId: {transactionId}");

                    // Validación server-side adicional (no bloquea — Apple ya validó en su lado)
                    ValidateReceiptAsync(appleProductId, userId, transactionId).ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                            Debug.LogWarning($"[AppleIAP] Server-side validation error: {t.Exception?.GetBaseException().Message}");
                    }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);

                    tcs.SetResult(PaymentResult.Successful(
                        product.ProductId, transactionId ?? System.Guid.NewGuid().ToString("N"),
                        PaymentProvider.AppleIAP));
                }
                else
                {
                    Debug.LogWarning($"[AppleIAP] Compra falló: {appleProductId}");
                    tcs.SetResult(PaymentResult.Failed(
                        product.ProductId, "iap_failed",
                        "Apple IAP transaction failed", PaymentProvider.AppleIAP));
                }
            });

            return await tcs.Task;
        }

        public async Task<PaymentResult> RestorePurchases(string userId)
        {
            if (!_isAvailable || !GetIsPremiumAvailable() || InvokeRestore == null)
            {
                return PaymentResult.Failed("restore", "iap_not_available",
                    "Apple IAP no disponible", PaymentProvider.AppleIAP);
            }

            var tcs = new TaskCompletionSource<PaymentResult>();

            InvokeRestore(() =>
            {
                Debug.Log("[AppleIAP] Restore completado");
                tcs.SetResult(PaymentResult.Successful("restore", "restore_complete", PaymentProvider.AppleIAP));
            });

            return await tcs.Task;
        }

        public async Task<List<CosmeticProduct>> FetchProducts(List<string> productIds)
        {
            // Retornar catálogo estático (precios reales vienen de App Store)
            var result = new List<CosmeticProduct>();
            foreach (var id in productIds)
            {
                var product = ProductCatalog.FindProduct(id) ?? ProductCatalog.FindByAppleProductId(id);
                if (product != null) result.Add(product);
            }
            return result;
        }

        public async Task<bool> HealthCheck()
        {
            bool healthy = GetIsPremiumAvailable();
            _isHealthy = healthy;
            return healthy;
        }

        public void Dispose()
        {
            Debug.Log("[AppleIAP] Disposed");
        }

        private async Task ValidateReceiptAsync(string productId, string userId, string transactionId)
        {
            // La validación server-side es adicional — no bloquea la compra
            Debug.Log($"[AppleIAP] Validando receipt server-side para: {productId}");
            // El receiptData real viene de Unity IAP vía PremiumManager
            // Por ahora log de intención
            await Task.Yield();
        }
    }
}
