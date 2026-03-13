using System.Collections.Generic;
using System.Threading.Tasks;

namespace DigitPark.Payments
{
    /// <summary>
    /// Interfaz implementada por StripePaymentProvider y AppleIAPProvider.
    /// NUNCA implementada por nada relacionado con Triumph.
    /// </summary>
    public interface IPaymentProvider
    {
        string ProviderName { get; }
        bool IsAvailable { get; }
        bool IsHealthy { get; }

        Task Initialize(PaymentConfig config);
        Task<PaymentResult> PurchaseProduct(CosmeticProduct product, string userId);
        Task<PaymentResult> RestorePurchases(string userId);
        Task<List<CosmeticProduct>> FetchProducts(List<string> productIds);
        Task<bool> HealthCheck();
        void Dispose();
    }
}
