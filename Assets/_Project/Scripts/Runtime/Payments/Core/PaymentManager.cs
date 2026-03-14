using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using DigitPark.Payments.Entitlements;

namespace DigitPark.Payments
{
    /// <summary>
    /// Orquestador central del sistema de pagos cosméticos.
    /// Decide si usar Stripe o Apple IAP basado en PaymentFeatureFlag.
    ///
    /// REGLA CRÍTICA: Este manager NUNCA importa ni referencia:
    ///   - TriumphManager
    ///   - ServiceLocator (el de CashBattle)
    ///   - IWalletService, IKYCService, IMatchmakingService, ITournamentService
    ///   - Cualquier namespace DigitPark.Services.Triumph
    /// </summary>
    public class PaymentManager : MonoBehaviour
    {
        private static PaymentManager _instance;
        public static PaymentManager Instance
        {
            get
            {
                // D-49: Use implicit bool to detect destroyed Unity objects (== null is overloaded)
                if (!_instance)
                    _instance = FindObjectOfType<PaymentManager>();
                return _instance;
            }
        }

        [Header("Configuración")]
        [SerializeField] private PaymentConfig _config = new PaymentConfig();

        private IPaymentProvider _stripeProvider;
        private IPaymentProvider _iapProvider;
        private int _stripeFailureCount = 0;
        private const int MAX_STRIPE_FAILURES = 3;
        private bool _isPurchaseInProgress = false;
        private bool _isInitialized = false;

        public bool IsInitialized => _isInitialized;
        public IPaymentProvider GetIAPProvider() => _iapProvider;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private async void Start()
        {
            try { await InitializeProviders(); }
            catch (System.Exception e) { Debug.LogError($"[PaymentManager] Init failed: {e.Message}"); }
        }

        private async Task InitializeProviders()
        {
            Debug.Log("[PaymentManager] Inicializando providers...");

            // Validar que la config no sea la por defecto vacía
            if (_config == null || string.IsNullOrEmpty(_config.paymentsHealthUrl))
            {
                Debug.LogError("[PaymentManager] PaymentConfig no está asignada en Inspector o está vacía. " +
                               "Asigna el ScriptableObject en el Inspector antes de hacer builds.");
                _isInitialized = true; // Continuar para no bloquear, pero Stripe no estará disponible
                return;
            }

            // Apple IAP siempre se inicializa (es el failsafe)
            _iapProvider = new AppleIAP.AppleIAPProvider();
            await _iapProvider.Initialize(_config);
            Debug.Log($"[PaymentManager] AppleIAP disponible: {_iapProvider.IsAvailable}");

#if HAS_STRIPE || UNITY_EDITOR
            // Stripe solo en versión Pro
            if (PaymentFeatureFlag.IsStripeEnabled)
            {
                _stripeProvider = new Stripe.StripePaymentProvider();
                await _stripeProvider.Initialize(_config);
                Debug.Log($"[PaymentManager] Stripe disponible: {_stripeProvider?.IsAvailable}");
            }
#endif

            _isInitialized = true;
            Debug.Log("[PaymentManager] Sistema de pagos inicializado correctamente");
        }

        /// <summary>
        /// Compra un producto cosmético. Usa Stripe como primario, Apple IAP como fallback.
        /// </summary>
        public async Task<PaymentResult> PurchaseCosmetic(string productId)
        {
            if (_isPurchaseInProgress)
            {
                return PaymentResult.Failed(productId, "purchase_in_progress",
                    "Ya hay una compra en progreso", PaymentFeatureFlag.ActiveCosmeticProvider);
            }

            var product = ProductCatalog.FindProduct(productId);
            if (product == null)
            {
                // Intentar por Apple Product ID
                product = ProductCatalog.FindByAppleProductId(productId);
            }
            if (product == null)
            {
                return PaymentResult.Failed(productId, "product_not_found",
                    $"Producto no encontrado: {productId}", PaymentProvider.None);
            }

            string userId = GetCurrentUserId();
            if (userId == "anonymous")
            {
                Debug.LogError("[PaymentManager] Compra bloqueada: usuario no autenticado. Inicia sesión antes de comprar.");
                return PaymentResult.Failed(productId, "user_not_authenticated",
                    "Inicia sesión para realizar compras", PaymentProvider.None);
            }

            _isPurchaseInProgress = true;
            PaymentEvents.EmitPurchaseStarted(productId, PaymentFeatureFlag.ActiveCosmeticProvider);

            try
            {
                PaymentResult result = null;
                bool usedFallback = false;

                // Intentar con el provider activo
                if (PaymentFeatureFlag.ActiveCosmeticProvider == PaymentProvider.Stripe
                    && _stripeProvider != null && _stripeProvider.IsAvailable)
                {
                    result = await _stripeProvider.PurchaseProduct(product, userId);

                    if (!result.Success)
                    {
                        _stripeFailureCount++;
                        Debug.LogWarning($"[PaymentManager] Stripe falló ({_stripeFailureCount}/{MAX_STRIPE_FAILURES}): {result.ErrorMessage}");

                        if (_stripeFailureCount >= MAX_STRIPE_FAILURES)
                        {
                            PaymentFeatureFlag.ForceSwitch(PaymentProvider.AppleIAP,
                                $"Stripe falló {MAX_STRIPE_FAILURES} veces");
                        }

                        // Fallback a Apple IAP
                        if (_iapProvider != null && _iapProvider.IsAvailable)
                        {
                            Debug.Log("[PaymentManager] Haciendo fallback a Apple IAP...");
                            result = await _iapProvider.PurchaseProduct(product, userId);
                            usedFallback = true;
                        }
                    }
                    else
                    {
                        _stripeFailureCount = 0;
                    }
                }
                else if (_iapProvider != null && _iapProvider.IsAvailable)
                {
                    result = await _iapProvider.PurchaseProduct(product, userId);
                }
                else
                {
                    result = PaymentResult.Failed(productId, "no_provider_available",
                        "Ningún proveedor de pago disponible", PaymentProvider.None);
                }

                if (result != null && result.Success)
                {
                    result.WasProviderSwitched = usedFallback;
                    await OnPurchaseSuccess(result, product);
                    PaymentEvents.EmitPurchaseCompleted(result);
                }
                else if (result != null)
                {
                    PaymentEvents.EmitPurchaseFailed(result);
                }

                return result ?? PaymentResult.Failed(productId, "unknown_error",
                    "Error desconocido", PaymentProvider.None);
            }
            finally
            {
                _isPurchaseInProgress = false;
            }
        }

        private async Task OnPurchaseSuccess(PaymentResult result, CosmeticProduct product)
        {
            Debug.Log($"[PaymentManager] Compra exitosa: {result.ProductId} via {result.ProviderUsed}");

            // Guardar entitlement
            var entitlementService = EntitlementService.Instance;
            if (entitlementService != null)
            {
                await entitlementService.Grant(result.ProductId,
                    result.ProviderUsed.ToString().ToLowerInvariant(),
                    result.TransactionId);
            }

            // Procesar gems si aplica
            if (product.GemsAmount > 0)
            {
                int bonusGems = product.BonusPercent > 0
                    ? Mathf.RoundToInt(product.GemsAmount * (product.BonusPercent / 100f))
                    : 0;
                PaymentBridge.ProcessGemsPurchase?.Invoke(product.GemsAmount, bonusGems);
                Debug.Log($"[PaymentManager] Gems otorgados: {product.GemsAmount} + {bonusGems} bonus");
            }

            // Analytics
            PaymentBridge.LogPurchaseCompleted?.Invoke(result.ProductId, (double)product.PriceUSD, "USD", result.TransactionId);
        }

        public async Task<PaymentResult> RestorePurchases()
        {
            if (_iapProvider == null)
                return PaymentResult.Failed("restore", "no_iap", "Apple IAP no disponible", PaymentProvider.None);

            var result = await _iapProvider.RestorePurchases(GetCurrentUserId());
            return result;
        }

        public PaymentProvider GetActiveProvider() => PaymentFeatureFlag.ActiveCosmeticProvider;

        public void ResetStripeFailureCount()
        {
            _stripeFailureCount = 0;
            Debug.Log("[PaymentManager] Contador de fallos de Stripe reiniciado");
        }

        private string GetCurrentUserId()
        {
            return PaymentBridge.GetCurrentUserId();
        }
    }
}
