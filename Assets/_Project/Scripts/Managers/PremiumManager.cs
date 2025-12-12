using UnityEngine;
using System;
// Descomentar después de instalar Unity IAP desde Package Manager:
// using UnityEngine.Purchasing;
// using UnityEngine.Purchasing.Extension;

namespace DigitPark.Managers
{
    /// <summary>
    /// Tipos de productos premium disponibles
    /// </summary>
    public enum PremiumProduct
    {
        RemoveAds,          // $10 MXN - Solo quita anuncios
        PremiumFull         // $20 MXN - Quita anuncios + Crear torneos
    }

    /// <summary>
    /// Manager de funciones premium y compras in-app
    ///
    /// INSTRUCCIONES DE CONFIGURACIÓN:
    /// 1. Window → Package Manager → Unity Registry → "In App Purchasing" → Install
    /// 2. Descomentar los "using" de arriba
    /// 3. Descomentar ": IDetailedStoreListener" en la clase
    /// 4. Descomentar todo el código marcado con "// UNITY IAP:"
    /// 5. Configurar productos en Google Play Console / App Store Connect
    /// </summary>
    public class PremiumManager : MonoBehaviour
        // UNITY IAP: Descomentar la siguiente línea:
        // , IDetailedStoreListener
    {
        private static PremiumManager _instance;
        public static PremiumManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<PremiumManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("PremiumManager");
                        _instance = go.AddComponent<PremiumManager>();
                    }
                }
                return _instance;
            }
        }

        // ================================================================
        // CONFIGURACIÓN DE PRODUCTOS
        // Estos IDs deben coincidir con los configurados en las tiendas
        // ================================================================

        [Header("=== PRODUCT IDs ===")]
        [Tooltip("ID del producto en Google Play / App Store")]
        public const string PRODUCT_ID_REMOVE_ADS = "com.digitpark.removeads";

        [Tooltip("ID del producto en Google Play / App Store")]
        public const string PRODUCT_ID_PREMIUM_FULL = "com.digitpark.premiumfull";

        [Header("=== PRECIOS (Solo para mostrar en UI) ===")]
        public const string PRICE_REMOVE_ADS = "$10 MXN";
        public const string PRICE_PREMIUM_FULL = "$20 MXN";

        // Keys para PlayerPrefs (persistencia local)
        private const string NO_ADS_KEY = "Premium_NoAds";
        private const string CAN_CREATE_TOURNAMENTS_KEY = "Premium_CreateTournaments";

        // Estado premium
        private bool _hasNoAds = false;
        private bool _canCreateTournaments = false;

        // UNITY IAP: Descomentar estas líneas:
        // private static IStoreController _storeController;
        // private static IExtensionProvider _extensionProvider;

        // Callback temporal para compras
        private Action<bool> _purchaseCallback;

        // Eventos públicos
        public static event Action OnPremiumStatusChanged;

        /// <summary>
        /// Indica si el usuario tiene la versión sin anuncios
        /// </summary>
        public bool HasNoAds => _hasNoAds;

        /// <summary>
        /// Indica si el usuario puede crear torneos (premium completo)
        /// </summary>
        public bool CanCreateTournaments => _canCreateTournaments;

        /// <summary>
        /// Indica si el usuario tiene algún tipo de premium
        /// </summary>
        public bool IsPremium => _hasNoAds || _canCreateTournaments;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                LoadPremiumStatus();
                InitializePurchasing();
                Debug.Log($"[Premium] Manager iniciado - NoAds: {_hasNoAds}, CreateTournaments: {_canCreateTournaments}");
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        #region Initialization

        private void InitializePurchasing()
        {
            // UNITY IAP: Descomentar este bloque:
            /*
            if (_storeController != null) return;

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            // Agregar productos
            // Consumable = se puede comprar múltiples veces
            // NonConsumable = se compra una vez y se mantiene para siempre
            builder.AddProduct(PRODUCT_ID_REMOVE_ADS, ProductType.NonConsumable);
            builder.AddProduct(PRODUCT_ID_PREMIUM_FULL, ProductType.NonConsumable);

            Debug.Log("[Premium] Inicializando Unity IAP...");
            UnityPurchasing.Initialize(this, builder);
            */

            Debug.Log("[Premium] Unity IAP no configurado. Las compras están simuladas.");
        }

        #endregion

        #region Status Management

        private void LoadPremiumStatus()
        {
            _hasNoAds = PlayerPrefs.GetInt(NO_ADS_KEY, 0) == 1;
            _canCreateTournaments = PlayerPrefs.GetInt(CAN_CREATE_TOURNAMENTS_KEY, 0) == 1;
        }

        private void SavePremiumStatus()
        {
            PlayerPrefs.SetInt(NO_ADS_KEY, _hasNoAds ? 1 : 0);
            PlayerPrefs.SetInt(CAN_CREATE_TOURNAMENTS_KEY, _canCreateTournaments ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void UnlockProduct(PremiumProduct product)
        {
            switch (product)
            {
                case PremiumProduct.RemoveAds:
                    _hasNoAds = true;
                    Debug.Log("[Premium] ✅ Desbloqueado: Sin Anuncios");
                    break;

                case PremiumProduct.PremiumFull:
                    _hasNoAds = true;
                    _canCreateTournaments = true;
                    Debug.Log("[Premium] ✅ Desbloqueado: Premium Completo");
                    break;
            }

            SavePremiumStatus();
            OnPremiumStatusChanged?.Invoke();
        }

        #endregion

        #region Purchase Methods

        /// <summary>
        /// Compra: Quitar anuncios ($10 MXN)
        /// </summary>
        public void PurchaseRemoveAds(Action<bool> onComplete = null)
        {
            Debug.Log("[Premium] Iniciando compra: Quitar Anuncios");
            BuyProduct(PRODUCT_ID_REMOVE_ADS, PremiumProduct.RemoveAds, onComplete);
        }

        /// <summary>
        /// Compra: Premium completo ($20 MXN)
        /// </summary>
        public void PurchasePremiumFull(Action<bool> onComplete = null)
        {
            Debug.Log("[Premium] Iniciando compra: Premium Completo");
            BuyProduct(PRODUCT_ID_PREMIUM_FULL, PremiumProduct.PremiumFull, onComplete);
        }

        private void BuyProduct(string productId, PremiumProduct product, Action<bool> onComplete)
        {
            _purchaseCallback = onComplete;

            // UNITY IAP: Descomentar este bloque y comentar la simulación:
            /*
            if (_storeController == null)
            {
                Debug.LogError("[Premium] Store no inicializado");
                onComplete?.Invoke(false);
                return;
            }

            Product p = _storeController.products.WithID(productId);
            if (p != null && p.availableToPurchase)
            {
                Debug.Log($"[Premium] Comprando: {p.definition.id} - {p.metadata.localizedPriceString}");
                _storeController.InitiatePurchase(p);
            }
            else
            {
                Debug.LogError($"[Premium] Producto no disponible: {productId}");
                onComplete?.Invoke(false);
            }
            */

            // SIMULACIÓN (Eliminar cuando actives Unity IAP):
            Debug.Log($"[Premium] 🔄 Simulando compra de {productId}...");
            StartCoroutine(SimulatePurchase(product, onComplete));
        }

        private System.Collections.IEnumerator SimulatePurchase(PremiumProduct product, Action<bool> onComplete)
        {
            // Simular delay de procesamiento
            yield return new WaitForSeconds(1.5f);

            // Simular compra exitosa
            UnlockProduct(product);
            onComplete?.Invoke(true);
            Debug.Log("[Premium] ✅ Compra simulada exitosa");
        }

        /// <summary>
        /// Restaurar compras (principalmente para iOS)
        /// </summary>
        public void RestorePurchases()
        {
            Debug.Log("[Premium] Restaurando compras...");

            // UNITY IAP: Descomentar este bloque:
            /*
            if (_storeController == null)
            {
                Debug.LogError("[Premium] Store no inicializado");
                return;
            }

            #if UNITY_IOS
            var apple = _extensionProvider.GetExtension<IAppleExtensions>();
            apple.RestoreTransactions((result, error) =>
            {
                if (result)
                    Debug.Log("[Premium] ✅ Compras restauradas");
                else
                    Debug.LogError($"[Premium] Error al restaurar: {error}");
            });
            #elif UNITY_ANDROID
            // En Android, las compras se restauran automáticamente al inicializar
            Debug.Log("[Premium] En Android, las compras se restauran automáticamente");
            #endif
            */

            Debug.Log("[Premium] Restauración simulada - Unity IAP no configurado");
        }

        #endregion

        #region Helper Methods

        public string GetProductPrice(PremiumProduct product)
        {
            // UNITY IAP: Reemplazar con precio real de la tienda:
            /*
            if (_storeController != null)
            {
                string productId = product == PremiumProduct.RemoveAds
                    ? PRODUCT_ID_REMOVE_ADS
                    : PRODUCT_ID_PREMIUM_FULL;

                Product p = _storeController.products.WithID(productId);
                if (p != null)
                    return p.metadata.localizedPriceString;
            }
            */

            return product == PremiumProduct.RemoveAds ? PRICE_REMOVE_ADS : PRICE_PREMIUM_FULL;
        }

        public string GetProductDescription(PremiumProduct product, string language = "es")
        {
            switch (product)
            {
                case PremiumProduct.RemoveAds:
                    return language == "es"
                        ? "Elimina todos los anuncios de la aplicación"
                        : "Remove all ads from the app";

                case PremiumProduct.PremiumFull:
                    return language == "es"
                        ? "Sin anuncios + Crear torneos ilimitados"
                        : "No ads + Create unlimited tournaments";

                default:
                    return "";
            }
        }

        public bool HasProduct(PremiumProduct product)
        {
            switch (product)
            {
                case PremiumProduct.RemoveAds:
                    return _hasNoAds;
                case PremiumProduct.PremiumFull:
                    return _canCreateTournaments;
                default:
                    return false;
            }
        }

        #endregion

        #region Unity IAP Callbacks

        // ============================================================
        // UNITY IAP: Descomentar todo este bloque después de instalar
        // ============================================================

        /*
        // Callback: IAP inicializado correctamente
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.Log("[Premium] ✅ Unity IAP inicializado correctamente");
            _storeController = controller;
            _extensionProvider = extensions;

            // Verificar si el usuario ya tiene productos comprados
            CheckExistingPurchases();
        }

        private void CheckExistingPurchases()
        {
            // Verificar Remove Ads
            Product removeAds = _storeController.products.WithID(PRODUCT_ID_REMOVE_ADS);
            if (removeAds != null && removeAds.hasReceipt)
            {
                Debug.Log("[Premium] Usuario ya tiene Remove Ads");
                _hasNoAds = true;
            }

            // Verificar Premium Full
            Product premiumFull = _storeController.products.WithID(PRODUCT_ID_PREMIUM_FULL);
            if (premiumFull != null && premiumFull.hasReceipt)
            {
                Debug.Log("[Premium] Usuario ya tiene Premium Full");
                _hasNoAds = true;
                _canCreateTournaments = true;
            }

            SavePremiumStatus();
            OnPremiumStatusChanged?.Invoke();
        }

        // Callback: Error al inicializar IAP
        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError($"[Premium] ❌ Error al inicializar IAP: {error}");
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError($"[Premium] ❌ Error al inicializar IAP: {error} - {message}");
        }

        // Callback: Compra procesada
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            string productId = args.purchasedProduct.definition.id;
            Debug.Log($"[Premium] Procesando compra: {productId}");

            if (productId == PRODUCT_ID_REMOVE_ADS)
            {
                UnlockProduct(PremiumProduct.RemoveAds);
                _purchaseCallback?.Invoke(true);
            }
            else if (productId == PRODUCT_ID_PREMIUM_FULL)
            {
                UnlockProduct(PremiumProduct.PremiumFull);
                _purchaseCallback?.Invoke(true);
            }
            else
            {
                Debug.LogWarning($"[Premium] Producto desconocido: {productId}");
            }

            _purchaseCallback = null;
            return PurchaseProcessingResult.Complete;
        }

        // Callback: Compra fallida
        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            Debug.LogError($"[Premium] ❌ Compra fallida: {product.definition.id} - {reason}");
            _purchaseCallback?.Invoke(false);
            _purchaseCallback = null;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            Debug.LogError($"[Premium] ❌ Compra fallida: {product.definition.id} - {failureDescription.message}");
            _purchaseCallback?.Invoke(false);
            _purchaseCallback = null;
        }
        */

        #endregion

        #region Debug (Solo para desarrollo)

#if UNITY_EDITOR
        [ContextMenu("Debug: Unlock No Ads")]
        private void DebugUnlockNoAds()
        {
            UnlockProduct(PremiumProduct.RemoveAds);
        }

        [ContextMenu("Debug: Unlock Premium Full")]
        private void DebugUnlockPremiumFull()
        {
            UnlockProduct(PremiumProduct.PremiumFull);
        }

        [ContextMenu("Debug: Reset All Premium")]
        private void DebugResetPremium()
        {
            _hasNoAds = false;
            _canCreateTournaments = false;
            SavePremiumStatus();
            OnPremiumStatusChanged?.Invoke();
            Debug.Log("[Premium] Estado premium reseteado");
        }
#endif

        #endregion
    }
}
