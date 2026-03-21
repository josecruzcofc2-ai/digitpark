using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
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
        /// InvokePurchase callback: (bool success, string transactionId, string receiptData)
        /// </summary>
        public static System.Func<bool> GetIsPremiumAvailable = () => false;
        public static System.Action<string, System.Action<bool, string, string>> InvokePurchase = null;
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

            InvokePurchase(appleProductId, (success, transactionId, receiptData) =>
            {
                if (success)
                {
                    Debug.Log($"[AppleIAP] Compra exitosa: {appleProductId}, txId: {transactionId}");

                    // B2-J: await validación server antes de otorgar el ítem
                    // Usar SHA256 del receipt como fallback — reproducible y sin colisiones por GUID random
                    string txId = transactionId ?? ComputeSha256(receiptData ?? string.Empty).Substring(0, 32);
                    GrantAfterValidationAsync(product, appleProductId, userId, txId, receiptData, tcs);
                }
                else
                {
                    Debug.LogWarning($"[AppleIAP] Compra falló: {appleProductId}");
                    tcs.SetResult(PaymentResult.Failed(
                        product.ProductId, "iap_failed",
                        "Apple IAP transaction failed", PaymentProvider.AppleIAP));
                }
            });

            // P3-10: Timeout to prevent hanging Task if callback never fires
            var timeoutTask = System.Threading.Tasks.Task.Delay(120000); // 2 minutes
            var completedTask = await System.Threading.Tasks.Task.WhenAny(tcs.Task, timeoutTask);
            if (completedTask == timeoutTask)
            {
                Debug.LogWarning("[AppleIAP] Purchase timed out after 2 minutes");
                return PaymentResult.Failed(product.ProductId, "iap_timeout",
                    "Purchase timed out", PaymentProvider.AppleIAP);
            }
            return tcs.Task.Result;
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

        private static string ComputeSha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }

        // B2-J: grant solo después de validación server exitosa
        private async void GrantAfterValidationAsync(
            CosmeticProduct product, string appleProductId, string userId,
            string txId, string receiptData,
            TaskCompletionSource<PaymentResult> tcs)
        {
            bool valid = await ValidateReceiptAsync(appleProductId, userId, txId, receiptData);
            if (!valid)
            {
                Debug.LogWarning($"[AppleIAP] Validación server-side fallida — compra no otorgada para {appleProductId}");
                tcs.SetResult(PaymentResult.Failed(
                    product.ProductId, "receipt_invalid",
                    "Server-side receipt validation failed", PaymentProvider.AppleIAP));
                return;
            }
            tcs.SetResult(PaymentResult.Successful(product.ProductId, txId, PaymentProvider.AppleIAP));
        }

        private async Task<bool> ValidateReceiptAsync(string productId, string userId,
            string transactionId, string receiptData)
        {
            string backendUrl = _config?.iapValidateReceiptUrl;
            Debug.Log($"[AppleIAP] Iniciando validación server-side para: {productId}");

            try
            {
                var result = await _receiptValidator.ValidateReceipt(receiptData, productId, userId, backendUrl);

                if (result.IsValid)
                {
                    Debug.Log($"[AppleIAP] Server-side validation OK: {productId}, txId: {result.TransactionId}");
                    return true;
                }
                else
                {
                    Debug.LogWarning($"[AppleIAP] Server-side validation FAILED: {result.ErrorMessage}");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[AppleIAP] ValidateReceiptAsync exception: {e.Message}");
                return false;
            }
        }
    }
}
