using UnityEngine;
using System;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using DigitPark.Services.Firebase;

namespace DigitPark.Managers
{
    /// <summary>
    /// Tipos de productos premium disponibles
    /// </summary>
    public enum PremiumProduct
    {
        CreateTournaments,      // $3.99 USD - Crear torneos
        CashBattleCreate,       // $6.99 USD - Crear batallas con dinero real
        TournamentBundle,       // $8.99 USD - Ambos: Crear torneos + Cash Battle
        StylesPro               // Legacy - Desbloquea todos los temas premium (backwards compat)
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
    public class PremiumManager : MonoBehaviour, IDetailedStoreListener
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
        public const string PRODUCT_ID_CREATE_TOURNAMENTS = "com.digitpark.createtournaments";
        public const string PRODUCT_ID_CASH_BATTLE_CREATE = "com.digitpark.cashbattlecreate";
        public const string PRODUCT_ID_TOURNAMENT_BUNDLE = "com.digitpark.tournamentbundle";
        public const string PRODUCT_ID_STYLES_PRO = "com.digitpark.stylespro"; // Legacy

        [Header("=== PRECIOS (Solo para mostrar en UI) ===")]
        public const string PRICE_CREATE_TOURNAMENTS = "$3.99";
        public const string PRICE_CASH_BATTLE_CREATE = "$6.99";
        public const string PRICE_TOURNAMENT_BUNDLE = "$8.99";

        // Keys para PlayerPrefs (persistencia local)
        private const string CAN_CREATE_TOURNAMENTS_KEY = "Premium_CreateTournaments";
        private const string CAN_CREATE_CASH_BATTLE_KEY = "Premium_CashBattleCreate";
        private const string STYLES_PRO_KEY = "Premium_StylesPro";

        // Estado premium
        private bool _canCreateTournaments = false;
        private bool _canCreateCashBattle = false;
        private bool _hasStylesPro = false;

        // Unity IAP
        private static IStoreController _storeController;
        private static IExtensionProvider _extensionProvider;

        // Callback temporal para compras
        private Action<bool> _purchaseCallback;

        // Eventos públicos
        public static event Action OnPremiumStatusChanged;

        /// <summary>
        /// Indica si el usuario puede crear torneos
        /// </summary>
        public bool CanCreateTournaments => _canCreateTournaments;

        /// <summary>
        /// Indica si el usuario puede crear Cash Battles
        /// </summary>
        public bool CanCreateCashBattle => _canCreateCashBattle;

        /// <summary>
        /// Indica si el usuario tiene los estilos/temas premium desbloqueados (legacy)
        /// </summary>
        public bool HasStylesPro => _hasStylesPro;

        /// <summary>
        /// Indica si el usuario tiene algún tipo de premium
        /// </summary>
        public bool IsPremium => _canCreateTournaments || _canCreateCashBattle || _hasStylesPro;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                LoadPremiumStatus();
                InitializePurchasing();
                Debug.Log($"[Premium] Manager iniciado - Tournaments: {_canCreateTournaments}, CashBattle: {_canCreateCashBattle}");
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        #region Initialization

        private void InitializePurchasing()
        {
            if (_storeController != null) return;

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            // Agregar productos NonConsumable (se compra una vez y se mantiene para siempre)
            builder.AddProduct(PRODUCT_ID_CREATE_TOURNAMENTS, ProductType.NonConsumable);
            builder.AddProduct(PRODUCT_ID_CASH_BATTLE_CREATE, ProductType.NonConsumable);
            builder.AddProduct(PRODUCT_ID_TOURNAMENT_BUNDLE, ProductType.NonConsumable);

            Debug.Log("[Premium] Inicializando Unity IAP...");
            UnityPurchasing.Initialize(this, builder);
        }

        #endregion

        #region Status Management

        private void LoadPremiumStatus()
        {
            _canCreateTournaments = PlayerPrefs.GetInt(CAN_CREATE_TOURNAMENTS_KEY, 0) == 1;
            _canCreateCashBattle = PlayerPrefs.GetInt(CAN_CREATE_CASH_BATTLE_KEY, 0) == 1;
            _hasStylesPro = PlayerPrefs.GetInt(STYLES_PRO_KEY, 0) == 1;
        }

        private void SavePremiumStatus()
        {
            PlayerPrefs.SetInt(CAN_CREATE_TOURNAMENTS_KEY, _canCreateTournaments ? 1 : 0);
            PlayerPrefs.SetInt(CAN_CREATE_CASH_BATTLE_KEY, _canCreateCashBattle ? 1 : 0);
            PlayerPrefs.SetInt(STYLES_PRO_KEY, _hasStylesPro ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void UnlockProduct(PremiumProduct product)
        {
            switch (product)
            {
                case PremiumProduct.CreateTournaments:
                    _canCreateTournaments = true;
                    Debug.Log("[Premium] Desbloqueado: Crear Torneos");
                    break;

                case PremiumProduct.CashBattleCreate:
                    _canCreateCashBattle = true;
                    Debug.Log("[Premium] Desbloqueado: Crear Cash Battles");
                    break;

                case PremiumProduct.TournamentBundle:
                    _canCreateTournaments = true;
                    _canCreateCashBattle = true;
                    Debug.Log("[Premium] Desbloqueado: Tournament Bundle (Torneos + Cash Battle)");
                    break;

                case PremiumProduct.StylesPro:
                    _hasStylesPro = true;
                    Debug.Log("[Premium] Desbloqueado: Estilos PRO (legacy)");
                    break;
            }

            SavePremiumStatus();
            OnPremiumStatusChanged?.Invoke();
        }

        #endregion

        #region Purchase Methods

        /// <summary>
        /// Compra: Crear Torneos ($3.99 USD)
        /// </summary>
        public void PurchaseCreateTournaments(Action<bool> onComplete = null)
        {
            Debug.Log("[Premium] Iniciando compra: Crear Torneos");
            BuyProduct(PRODUCT_ID_CREATE_TOURNAMENTS, PremiumProduct.CreateTournaments, onComplete);
        }

        /// <summary>
        /// Compra: Crear Cash Battles ($6.99 USD)
        /// </summary>
        public void PurchaseCashBattleCreate(Action<bool> onComplete = null)
        {
            Debug.Log("[Premium] Iniciando compra: Cash Battle Create");
            BuyProduct(PRODUCT_ID_CASH_BATTLE_CREATE, PremiumProduct.CashBattleCreate, onComplete);
        }

        /// <summary>
        /// Compra: Tournament Bundle ($8.99 USD) - Ambos
        /// </summary>
        public void PurchaseTournamentBundle(Action<bool> onComplete = null)
        {
            Debug.Log("[Premium] Iniciando compra: Tournament Bundle");
            BuyProduct(PRODUCT_ID_TOURNAMENT_BUNDLE, PremiumProduct.TournamentBundle, onComplete);
        }

        private void BuyProduct(string productId, PremiumProduct product, Action<bool> onComplete)
        {
            _purchaseCallback = onComplete;

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

                // Analytics - Log purchase started
                AnalyticsService.Instance?.LogPurchaseStarted(
                    productId,
                    (double)p.metadata.localizedPrice,
                    p.metadata.isoCurrencyCode
                );

                _storeController.InitiatePurchase(p);
            }
            else
            {
                Debug.LogError($"[Premium] Producto no disponible: {productId}");
                onComplete?.Invoke(false);
            }
        }

        /// <summary>
        /// Restaurar compras (principalmente para iOS)
        /// </summary>
        public void RestorePurchases()
        {
            Debug.Log("[Premium] Restaurando compras...");

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
                    Debug.Log("[Premium] Compras restauradas");
                else
                    Debug.LogError($"[Premium] Error al restaurar: {error}");
            });
#elif UNITY_ANDROID
            // En Android, las compras se restauran automáticamente al inicializar
            Debug.Log("[Premium] En Android, las compras se restauran automáticamente");
#endif
        }

        #endregion

        #region Helper Methods

        public string GetProductPrice(PremiumProduct product)
        {
            // Obtener precio real de la tienda si está disponible
            if (_storeController != null)
            {
                string productId = GetProductId(product);
                Product p = _storeController.products.WithID(productId);
                if (p != null)
                    return p.metadata.localizedPriceString;
            }

            // Fallback a precios por defecto
            switch (product)
            {
                case PremiumProduct.CreateTournaments: return PRICE_CREATE_TOURNAMENTS;
                case PremiumProduct.CashBattleCreate: return PRICE_CASH_BATTLE_CREATE;
                case PremiumProduct.TournamentBundle: return PRICE_TOURNAMENT_BUNDLE;
                default: return "";
            }
        }

        public string GetProductId(PremiumProduct product)
        {
            switch (product)
            {
                case PremiumProduct.CreateTournaments: return PRODUCT_ID_CREATE_TOURNAMENTS;
                case PremiumProduct.CashBattleCreate: return PRODUCT_ID_CASH_BATTLE_CREATE;
                case PremiumProduct.TournamentBundle: return PRODUCT_ID_TOURNAMENT_BUNDLE;
                case PremiumProduct.StylesPro: return PRODUCT_ID_STYLES_PRO;
                default: return "";
            }
        }

        public string GetProductDescription(PremiumProduct product, string language = "es")
        {
            switch (product)
            {
                case PremiumProduct.CreateTournaments:
                    return language == "es"
                        ? "Crea torneos personalizados con tus amigos"
                        : "Create custom tournaments with your friends";

                case PremiumProduct.CashBattleCreate:
                    return language == "es"
                        ? "Crea batallas con dinero real y gana premios"
                        : "Create real money battles and win prizes";

                case PremiumProduct.TournamentBundle:
                    return language == "es"
                        ? "Crea torneos + Cash Battles - El paquete completo"
                        : "Create tournaments + Cash Battles - The complete package";

                case PremiumProduct.StylesPro:
                    return language == "es"
                        ? "Desbloquea todos los temas premium"
                        : "Unlock all premium themes";

                default:
                    return "";
            }
        }

        public bool HasProduct(PremiumProduct product)
        {
            switch (product)
            {
                case PremiumProduct.CreateTournaments:
                    return _canCreateTournaments;
                case PremiumProduct.CashBattleCreate:
                    return _canCreateCashBattle;
                case PremiumProduct.TournamentBundle:
                    return _canCreateTournaments && _canCreateCashBattle;
                case PremiumProduct.StylesPro:
                    return _hasStylesPro;
                default:
                    return false;
            }
        }

        #endregion

        #region Unity IAP Callbacks

        /// <summary>
        /// Callback: IAP inicializado correctamente
        /// </summary>
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.Log("[Premium] Unity IAP inicializado correctamente");
            _storeController = controller;
            _extensionProvider = extensions;

            // Verificar si el usuario ya tiene productos comprados
            CheckExistingPurchases();
        }

        private void CheckExistingPurchases()
        {
            // Verificar Create Tournaments
            Product createTournaments = _storeController.products.WithID(PRODUCT_ID_CREATE_TOURNAMENTS);
            if (createTournaments != null && createTournaments.hasReceipt)
            {
                Debug.Log("[Premium] Usuario ya tiene Create Tournaments");
                _canCreateTournaments = true;
            }

            // Verificar Cash Battle Create
            Product cashBattle = _storeController.products.WithID(PRODUCT_ID_CASH_BATTLE_CREATE);
            if (cashBattle != null && cashBattle.hasReceipt)
            {
                Debug.Log("[Premium] Usuario ya tiene Cash Battle Create");
                _canCreateCashBattle = true;
            }

            // Verificar Tournament Bundle
            Product bundle = _storeController.products.WithID(PRODUCT_ID_TOURNAMENT_BUNDLE);
            if (bundle != null && bundle.hasReceipt)
            {
                Debug.Log("[Premium] Usuario ya tiene Tournament Bundle");
                _canCreateTournaments = true;
                _canCreateCashBattle = true;
            }

            SavePremiumStatus();
            OnPremiumStatusChanged?.Invoke();
        }

        /// <summary>
        /// Callback: Error al inicializar IAP
        /// </summary>
        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError($"[Premium] Error al inicializar IAP: {error}");
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError($"[Premium] Error al inicializar IAP: {error} - {message}");
        }

        /// <summary>
        /// Callback: Compra procesada exitosamente
        /// </summary>
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            string productId = args.purchasedProduct.definition.id;
            Debug.Log($"[Premium] Procesando compra: {productId}");

            // Analytics - Log purchase completed
            AnalyticsService.Instance?.LogPurchaseCompleted(
                productId,
                (double)args.purchasedProduct.metadata.localizedPrice,
                args.purchasedProduct.metadata.isoCurrencyCode,
                args.purchasedProduct.transactionID
            );

            if (productId == PRODUCT_ID_CREATE_TOURNAMENTS)
            {
                UnlockProduct(PremiumProduct.CreateTournaments);
                _purchaseCallback?.Invoke(true);
            }
            else if (productId == PRODUCT_ID_CASH_BATTLE_CREATE)
            {
                UnlockProduct(PremiumProduct.CashBattleCreate);
                _purchaseCallback?.Invoke(true);
            }
            else if (productId == PRODUCT_ID_TOURNAMENT_BUNDLE)
            {
                UnlockProduct(PremiumProduct.TournamentBundle);
                _purchaseCallback?.Invoke(true);
            }
            else
            {
                Debug.LogWarning($"[Premium] Producto desconocido: {productId}");
            }

            _purchaseCallback = null;
            return PurchaseProcessingResult.Complete;
        }

        /// <summary>
        /// Callback: Compra fallida
        /// </summary>
        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            Debug.LogError($"[Premium] Compra fallida: {product.definition.id} - {reason}");

            // Analytics - Log purchase failed
            AnalyticsService.Instance?.LogPurchaseFailed(product.definition.id, reason.ToString());

            _purchaseCallback?.Invoke(false);
            _purchaseCallback = null;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            Debug.LogError($"[Premium] Compra fallida: {product.definition.id} - {failureDescription.message}");

            // Analytics - Log purchase failed
            AnalyticsService.Instance?.LogPurchaseFailed(product.definition.id, failureDescription.message);

            _purchaseCallback?.Invoke(false);
            _purchaseCallback = null;
        }

        #endregion

        #region Debug (Solo para desarrollo)

#if UNITY_EDITOR
        [ContextMenu("Debug: Unlock Create Tournaments")]
        private void DebugUnlockCreateTournaments()
        {
            UnlockProduct(PremiumProduct.CreateTournaments);
        }

        [ContextMenu("Debug: Unlock Cash Battle Create")]
        private void DebugUnlockCashBattle()
        {
            UnlockProduct(PremiumProduct.CashBattleCreate);
        }

        [ContextMenu("Debug: Unlock Tournament Bundle")]
        private void DebugUnlockBundle()
        {
            UnlockProduct(PremiumProduct.TournamentBundle);
        }

        [ContextMenu("Debug: Reset All Premium")]
        private void DebugResetPremium()
        {
            _canCreateTournaments = false;
            _canCreateCashBattle = false;
            _hasStylesPro = false;
            SavePremiumStatus();
            OnPremiumStatusChanged?.Invoke();
            Debug.Log("[Premium] Estado premium reseteado");
        }
#endif

        #endregion
    }
}
