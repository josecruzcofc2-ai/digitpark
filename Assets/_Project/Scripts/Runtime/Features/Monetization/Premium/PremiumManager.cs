using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;
using System.Threading.Tasks;
using DigitPark.Services.Firebase;
using DigitPark.Monetization;

namespace DigitPark.Managers
{
    /// <summary>
    /// Tipos de productos premium disponibles
    /// </summary>
    public enum PremiumProduct
    {
        CreateTournaments,      // $3.99 USD - Crear torneos
        TournamentBundle,       // $7.99 USD - Crear torneos (bundle)
        StylesPro,              // Desbloquea pack de cosméticos premium: Frames + Titles (backwards compat)
        PremiumBundle,          // All Frames + All Titles bundle (30% off)
        CompleteBundle          // All Frames + All Titles + All Win Effects bundle (30% off)
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
                    _instance = FindFirstObjectByType<PremiumManager>();
                    if (_instance != null)
                    {
                        // Found existing instance — ensure it persists across scenes
                        DontDestroyOnLoad(_instance.gameObject);
                    }
                    else
                    {
                        GameObject go = new GameObject("PremiumManager");
                        _instance = go.AddComponent<PremiumManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        // ================================================================
        // CONFIGURACIÓN DE PRODUCTOS
        // Estos IDs deben coincidir con los configurados en las tiendas
        // ================================================================

        [Header("=== PRODUCT IDs - Non-Consumable ===")]
        public const string PRODUCT_ID_CREATE_TOURNAMENTS = "com.matrixsoftware.digitpark.createtournaments";
        public const string PRODUCT_ID_TOURNAMENT_BUNDLE = "com.matrixsoftware.digitpark.tournamentbundle";
        public const string PRODUCT_ID_STYLES_PRO = "com.matrixsoftware.digitpark.stylespro"; // Legacy
        public const string PRODUCT_ID_PREMIUM_BUNDLE = "com.matrixsoftware.digitpark.premium_bundle";
        public const string PRODUCT_ID_COMPLETE_BUNDLE = "com.matrixsoftware.digitpark.complete_bundle";

        [Header("=== PRODUCT IDs - Gem Packs (Consumable) ===")]
        public const string PRODUCT_ID_GEMS_100 = "com.matrixsoftware.digitpark.gems_100";
        public const string PRODUCT_ID_GEMS_500 = "com.matrixsoftware.digitpark.gems_500";
        public const string PRODUCT_ID_GEMS_1200 = "com.matrixsoftware.digitpark.gems_1200";
        public const string PRODUCT_ID_GEMS_2500 = "com.matrixsoftware.digitpark.gems_2500";
        public const string PRODUCT_ID_GEMS_6500 = "com.matrixsoftware.digitpark.gems_6500";
        public const string PRODUCT_ID_GEMS_14000 = "com.matrixsoftware.digitpark.gems_14000";

        [Header("=== PRECIOS (Solo para mostrar en UI) ===")]
        public const string PRICE_CREATE_TOURNAMENTS = ""; // C-P1-30: Use IAP localizedPriceString; empty = not yet loaded
        public const string PRICE_TOURNAMENT_BUNDLE = ""; // C-P1-30: Use IAP localizedPriceString
        public const string PRICE_PREMIUM_BUNDLE = ""; // C-P1-30: Use IAP localizedPriceString
        public const string PRICE_COMPLETE_BUNDLE = ""; // C-P1-30: Use IAP localizedPriceString

        // Gem pack definitions: productId → (baseGems, bonusPercent)
        private static readonly Dictionary<string, (int gems, int bonus)> GEM_PACK_MAP = new Dictionary<string, (int, int)>
        {
            { PRODUCT_ID_GEMS_100, (100, 0) },
            { PRODUCT_ID_GEMS_500, (500, 10) },
            { PRODUCT_ID_GEMS_1200, (1200, 20) },
            { PRODUCT_ID_GEMS_2500, (2500, 25) },
            { PRODUCT_ID_GEMS_6500, (6500, 30) },
            { PRODUCT_ID_GEMS_14000, (14000, 35) },
        };

        // Keys para PlayerPrefs (persistencia local)
        private const string CAN_CREATE_TOURNAMENTS_KEY = "Premium_CreateTournaments";
        private const string STYLES_PRO_KEY = "Premium_StylesPro";

        // Estado premium
        private bool _canCreateTournaments = false;
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
        /// Indica si el usuario tiene el pack de cosméticos premium desbloqueado (Frames + Titles + Win Effects)
        /// </summary>
        public bool HasStylesPro => _hasStylesPro;

        /// <summary>
        /// Indica si el usuario tiene algún tipo de premium
        /// </summary>
        public bool IsPremium => _canCreateTournaments || _hasStylesPro || IsPremiumPass;

        /// <summary>
        /// Indica si el usuario ha comprado Ad-Free permanente.
        /// </summary>
        public bool IsAdFree =>
            DigitPark.Payments.Entitlements.EntitlementService.Instance?.HasEntitlement("adfree_permanent") == true;

        /// <summary>
        /// Indica si el usuario tiene Premium Pass activo (suscripción mensual).
        /// </summary>
        public bool IsPremiumPass =>
            DigitPark.Payments.Entitlements.EntitlementService.Instance?.HasEntitlement("premium_pass_monthly") == true;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                LoadPremiumStatus();
                RestoreFromFirebaseAsync();
                InitializePurchasing();
                Debug.Log($"[Premium] Manager iniciado - Tournaments: {_canCreateTournaments}");
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

            // NonConsumable products (purchased once, kept forever)
            builder.AddProduct(PRODUCT_ID_CREATE_TOURNAMENTS, ProductType.NonConsumable);
            builder.AddProduct(PRODUCT_ID_TOURNAMENT_BUNDLE, ProductType.NonConsumable);
            builder.AddProduct(PRODUCT_ID_PREMIUM_BUNDLE, ProductType.NonConsumable);
            builder.AddProduct(PRODUCT_ID_COMPLETE_BUNDLE, ProductType.NonConsumable);

            // Consumable products - Gem Packs (can be purchased multiple times)
            builder.AddProduct(PRODUCT_ID_GEMS_100, ProductType.Consumable);
            builder.AddProduct(PRODUCT_ID_GEMS_500, ProductType.Consumable);
            builder.AddProduct(PRODUCT_ID_GEMS_1200, ProductType.Consumable);
            builder.AddProduct(PRODUCT_ID_GEMS_2500, ProductType.Consumable);
            builder.AddProduct(PRODUCT_ID_GEMS_6500, ProductType.Consumable);
            builder.AddProduct(PRODUCT_ID_GEMS_14000, ProductType.Consumable);

            Debug.Log("[Premium] Inicializando Unity IAP...");
            UnityPurchasing.Initialize(this, builder);
        }

        #endregion

        #region Status Management

        private void LoadPremiumStatus()
        {
            _canCreateTournaments = PlayerPrefs.GetInt(CAN_CREATE_TOURNAMENTS_KEY, 0) == 1;
            _hasStylesPro = PlayerPrefs.GetInt(STYLES_PRO_KEY, 0) == 1;
        }

        private void SavePremiumStatus()
        {
            PlayerPrefs.SetInt(CAN_CREATE_TOURNAMENTS_KEY, _canCreateTournaments ? 1 : 0);
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

                case PremiumProduct.TournamentBundle:
                    _canCreateTournaments = true;
                    Debug.Log("[Premium] Desbloqueado: Tournament Bundle (Torneos)");
                    break;

                case PremiumProduct.StylesPro:
                    _hasStylesPro = true;
                    Debug.Log("[Premium] Desbloqueado: Cosmetics PRO (Frames + Titles)");
                    break;

                case PremiumProduct.PremiumBundle:
                    _hasStylesPro = true;
                    Debug.Log("[Premium] Desbloqueado: Premium Bundle (All Frames + All Titles)");
                    break;

                case PremiumProduct.CompleteBundle:
                    _hasStylesPro = true;
                    Debug.Log("[Premium] Desbloqueado: Complete Bundle (All Frames + Titles + Win Effects)");
                    break;
            }

            SavePremiumStatus();
            SyncPremiumToFirebase();
            OnPremiumStatusChanged?.Invoke();
        }

        /// <summary>
        /// Sincroniza estado premium a Firebase para que no se pierda al reinstalar
        /// </summary>
        private bool _pendingSync = false;

        private async void SyncPremiumToFirebase()
        {
            try
            {
                var auth = AuthenticationService.Instance;
                var db = DatabaseService.Instance;
                if (auth == null || db == null) { _pendingSync = true; return; }

                string userId = auth.GetCurrentPlayerData()?.userId;
                if (string.IsNullOrEmpty(userId)) { _pendingSync = true; return; }

                var updates = new Dictionary<string, object>
                {
                    { "premium_createTournaments", _canCreateTournaments },
                    { "premium_stylesPro", _hasStylesPro }
                };

                await db.UpdatePlayerFields(userId, updates);
                _pendingSync = false;
                Debug.Log("[Premium] Estado premium sincronizado a Firebase");
            }
            catch (Exception e)
            {
                _pendingSync = true;
                Debug.LogWarning($"[Premium] Error sincronizando premium a Firebase (will retry): {e.Message}");
            }
        }

        /// <summary>
        /// Retries pending sync. Call from OnApplicationFocus(true) or after auth login.
        /// </summary>
        public void RetrySyncIfPending()
        {
            if (_pendingSync) SyncPremiumToFirebase();
        }

        /// <summary>
        /// Restaura estado premium desde Firebase al iniciar.
        /// Toma OR de local vs Firebase (si alguno dice true, es true) para no perder compras.
        /// </summary>
        private async void RestoreFromFirebaseAsync()
        {
            try
            {
                var auth = AuthenticationService.Instance;
                var db = DatabaseService.Instance;
                if (auth == null || db == null) return;

                // Esperar a que auth esté listo
                int retries = 0;
                while (!auth.IsInitialized && retries < 50)
                {
                    await Task.Delay(100);
                    retries++;
                }

                string userId = auth.GetCurrentPlayerData()?.userId;
                if (string.IsNullOrEmpty(userId)) return;

                // Leer cada campo premium directamente de Firebase
                bool changed = false;

                string fbTournaments = await db.GetPlayerField(userId, "premium_createTournaments");
                if (string.Equals(fbTournaments, "True", StringComparison.OrdinalIgnoreCase) && !_canCreateTournaments)
                {
                    _canCreateTournaments = true;
                    changed = true;
                }

                string fbStylesPro = await db.GetPlayerField(userId, "premium_stylesPro");
                if (string.Equals(fbStylesPro, "True", StringComparison.OrdinalIgnoreCase) && !_hasStylesPro)
                {
                    _hasStylesPro = true;
                    changed = true;
                }

                if (changed)
                {
                    SavePremiumStatus();
                    OnPremiumStatusChanged?.Invoke();
                    Debug.Log($"[Premium] Restaurado desde Firebase - Tournaments: {_canCreateTournaments}, StylesPro: {_hasStylesPro}");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Premium] Error restaurando premium desde Firebase: {e.Message}");
            }
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
        /// Compra: Tournament Bundle ($8.99 USD) - Ambos
        /// </summary>
        public void PurchaseTournamentBundle(Action<bool> onComplete = null)
        {
            Debug.Log("[Premium] Iniciando compra: Tournament Bundle");
            BuyProduct(PRODUCT_ID_TOURNAMENT_BUNDLE, PremiumProduct.TournamentBundle, onComplete);
        }

        /// <summary>
        /// Compra: Premium Bundle — All Frames + All Titles ($26.25 USD, 30% off)
        /// </summary>
        public void PurchasePremiumBundle(Action<bool> onComplete = null)
        {
            Debug.Log("[Premium] Iniciando compra: Premium Bundle (All Frames + Titles)");
            BuyProduct(PRODUCT_ID_PREMIUM_BUNDLE, PremiumProduct.PremiumBundle, onComplete);
        }

        /// <summary>
        /// Compra: Complete Bundle — All Frames + Titles + Win Effects ($30.45 USD, 30% off)
        /// </summary>
        public void PurchaseCompleteBundle(Action<bool> onComplete = null)
        {
            Debug.Log("[Premium] Iniciando compra: Complete Bundle (All Frames + Titles + Win Effects)");
            BuyProduct(PRODUCT_ID_COMPLETE_BUNDLE, PremiumProduct.CompleteBundle, onComplete);
        }

        /// <summary>
        /// Compra un paquete de gemas (consumable IAP)
        /// </summary>
        public void PurchaseGemPack(string iapProductId, Action<bool> onComplete = null)
        {
            if (!IsGemPackProduct(iapProductId))
            {
                Debug.LogError($"[Premium] Not a gem pack product: {iapProductId}");
                onComplete?.Invoke(false);
                return;
            }

            _purchaseCallback = onComplete;

            if (_storeController == null)
            {
                Debug.LogError("[Premium] Store no inicializado");
                _purchaseCallback = null;
                onComplete?.Invoke(false);
                return;
            }

            Product p = _storeController.products.WithID(iapProductId);
            if (p != null && p.availableToPurchase)
            {
                Debug.Log($"[Premium] Comprando gem pack: {p.definition.id} - {p.metadata.localizedPriceString}");

                AnalyticsService.Instance?.LogPurchaseStarted(
                    iapProductId,
                    (double)p.metadata.localizedPrice,
                    p.metadata.isoCurrencyCode
                );

                _storeController.InitiatePurchase(p);
            }
            else
            {
                Debug.LogError($"[Premium] Gem pack no disponible: {iapProductId}");
                _purchaseCallback = null;
                onComplete?.Invoke(false);
            }
        }

        /// <summary>
        /// Checks if a product ID belongs to a gem pack
        /// </summary>
        public static bool IsGemPackProduct(string productId)
        {
            return GEM_PACK_MAP.ContainsKey(productId);
        }

        /// <summary>
        /// Processes a gem pack purchase after successful IAP transaction
        /// </summary>
        private void ProcessGemPackPurchase(string productId)
        {
            if (!GEM_PACK_MAP.TryGetValue(productId, out var packInfo))
            {
                Debug.LogError($"[Premium] Unknown gem pack: {productId}");
                return;
            }

            int bonusGems = packInfo.bonus > 0
                ? Mathf.RoundToInt(packInfo.gems * (packInfo.bonus / 100f))
                : 0;

            CurrencyManager.Instance?.ProcessGemsPurchase(packInfo.gems, bonusGems);

            Debug.Log($"[Premium] Gem pack granted: {packInfo.gems} + {bonusGems} bonus = {packInfo.gems + bonusGems} total");
        }

        /// <summary>
        /// Gets the localized price for a gem pack from the store
        /// </summary>
        public string GetGemPackPrice(string iapProductId)
        {
            if (_storeController != null)
            {
                Product p = _storeController.products.WithID(iapProductId);
                if (p != null)
                    return p.metadata.localizedPriceString;
            }
            return null;
        }

        /// <summary>
        /// Generic purchase by product ID (for shop items like themes, bundles)
        /// </summary>
        public void PurchaseByProductId(string productId, Action<bool> onComplete = null)
        {
            _purchaseCallback = onComplete;

            if (_storeController == null)
            {
                Debug.LogError("[Premium] Store no inicializado");
                _purchaseCallback = null;
                onComplete?.Invoke(false);
                return;
            }

            Product p = _storeController.products.WithID(productId);
            if (p != null && p.availableToPurchase)
            {
                Debug.Log($"[Premium] Comprando: {p.definition.id} - {p.metadata.localizedPriceString}");

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
                _purchaseCallback = null;
                onComplete?.Invoke(false);
            }
        }

        private void BuyProduct(string productId, PremiumProduct product, Action<bool> onComplete)
        {
            _purchaseCallback = onComplete;

            if (_storeController == null)
            {
                Debug.LogError("[Premium] Store no inicializado");
                _purchaseCallback = null; // Clear before direct invoke to prevent double-call from IAP restore
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
                _purchaseCallback = null; // Clear before direct invoke to prevent double-call from IAP restore
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
                case PremiumProduct.TournamentBundle: return PRICE_TOURNAMENT_BUNDLE;
                case PremiumProduct.PremiumBundle: return PRICE_PREMIUM_BUNDLE;
                case PremiumProduct.CompleteBundle: return PRICE_COMPLETE_BUNDLE;
                default: return "";
            }
        }

        public string GetProductId(PremiumProduct product)
        {
            switch (product)
            {
                case PremiumProduct.CreateTournaments: return PRODUCT_ID_CREATE_TOURNAMENTS;
                case PremiumProduct.TournamentBundle: return PRODUCT_ID_TOURNAMENT_BUNDLE;
                case PremiumProduct.StylesPro: return PRODUCT_ID_STYLES_PRO;
                case PremiumProduct.PremiumBundle: return PRODUCT_ID_PREMIUM_BUNDLE;
                case PremiumProduct.CompleteBundle: return PRODUCT_ID_COMPLETE_BUNDLE;
                default: return "";
            }
        }

        public string GetProductDescription(PremiumProduct product, string language = "es")
        {
            switch (product)
            {
                case PremiumProduct.CreateTournaments:
                    return string.Equals(language, "es", System.StringComparison.OrdinalIgnoreCase)
                        ? "Crea torneos personalizados con tus amigos"
                        : "Create custom tournaments with your friends";

                case PremiumProduct.TournamentBundle:
                    return string.Equals(language, "es", System.StringComparison.OrdinalIgnoreCase)
                        ? "Crea torneos personalizados - El paquete completo"
                        : "Create custom tournaments - The complete package";

                case PremiumProduct.StylesPro:
                    return string.Equals(language, "es", System.StringComparison.OrdinalIgnoreCase)
                        ? "Desbloquea todos los frames y títulos premium"
                        : "Unlock all premium frames and titles";

                case PremiumProduct.PremiumBundle:
                    return string.Equals(language, "es", System.StringComparison.OrdinalIgnoreCase)
                        ? "Todos los frames y títulos con 30% de descuento"
                        : "All frames and titles with 30% discount";

                case PremiumProduct.CompleteBundle:
                    return string.Equals(language, "es", System.StringComparison.OrdinalIgnoreCase)
                        ? "Todos los frames, títulos y efectos de victoria con 30% de descuento"
                        : "All frames, titles and win effects with 30% discount";

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
                case PremiumProduct.TournamentBundle:
                    return _canCreateTournaments;
                case PremiumProduct.StylesPro:
                case PremiumProduct.PremiumBundle:
                case PremiumProduct.CompleteBundle:
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

            // Verificar Tournament Bundle
            Product bundle = _storeController.products.WithID(PRODUCT_ID_TOURNAMENT_BUNDLE);
            if (bundle != null && bundle.hasReceipt)
            {
                Debug.Log("[Premium] Usuario ya tiene Tournament Bundle");
                _canCreateTournaments = true;
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

            // Receipt validation (local)
            if (!ValidateReceipt(args.purchasedProduct))
            {
                Debug.LogError($"[Premium] Receipt validation failed for: {productId}");
                _purchaseCallback?.Invoke(false);
                _purchaseCallback = null;
                return PurchaseProcessingResult.Complete;
            }

            // Analytics - Log purchase completed
            AnalyticsService.Instance?.LogPurchaseCompleted(
                productId,
                (double)args.purchasedProduct.metadata.localizedPrice,
                args.purchasedProduct.metadata.isoCurrencyCode,
                args.purchasedProduct.transactionID
            );

            // Handle gem pack consumables
            if (IsGemPackProduct(productId))
            {
                ProcessGemPackPurchase(productId);
                _purchaseCallback?.Invoke(true);
                _purchaseCallback = null;
                _pendingPurchaseExtCallback?.Invoke(true,
                    args.purchasedProduct.transactionID ?? "",
                    args.purchasedProduct.receipt ?? "");
                _pendingPurchaseExtCallback = null;
                return PurchaseProcessingResult.Complete;
            }

            // Handle non-consumable premium products
            if (productId == PRODUCT_ID_CREATE_TOURNAMENTS)
            {
                UnlockProduct(PremiumProduct.CreateTournaments);
                _purchaseCallback?.Invoke(true);
            }
            else if (productId == PRODUCT_ID_TOURNAMENT_BUNDLE)
            {
                UnlockProduct(PremiumProduct.TournamentBundle);
                _purchaseCallback?.Invoke(true);
            }
            else if (productId == PRODUCT_ID_PREMIUM_BUNDLE)
            {
                UnlockProduct(PremiumProduct.PremiumBundle);
                _purchaseCallback?.Invoke(true);
            }
            else if (productId == PRODUCT_ID_COMPLETE_BUNDLE)
            {
                UnlockProduct(PremiumProduct.CompleteBundle);
                _purchaseCallback?.Invoke(true);
            }
            else
            {
                Debug.LogWarning($"[Premium] Producto desconocido: {productId}");
            }

            _purchaseCallback = null;
            _pendingPurchaseExtCallback?.Invoke(true,
                args.purchasedProduct.transactionID ?? "",
                args.purchasedProduct.receipt ?? "");
            _pendingPurchaseExtCallback = null;
            return PurchaseProcessingResult.Complete;
        }

        /// <summary>
        /// Local receipt validation using CrossPlatformValidator.
        /// Requires Tangle files generated via Window > Unity IAP > Receipt Validation Obfuscator.
        /// </summary>
        private bool ValidateReceipt(Product product)
        {
#if !UNITY_EDITOR
            try
            {
                var validator = new CrossPlatformValidator(
                    GooglePlayTangle.Data(),
                    AppleTangle.Data(),
                    Application.identifier
                );

                var result = validator.Validate(product.receipt);
                Debug.Log($"[Premium] Receipt validated: {result.Length} receipts");
                return true;
            }
            catch (IAPSecurityException ex)
            {
                Debug.LogError($"[Premium] Receipt validation failed: {ex.Message}");
                return false;
            }
#else
            // Skip validation in Editor
            return true;
#endif
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
            _pendingPurchaseExtCallback?.Invoke(false, "", "");
            _pendingPurchaseExtCallback = null;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            Debug.LogError($"[Premium] Compra fallida: {product.definition.id} - {failureDescription.message}");

            // Analytics - Log purchase failed
            AnalyticsService.Instance?.LogPurchaseFailed(product.definition.id, failureDescription.message);

            _purchaseCallback?.Invoke(false);
            _purchaseCallback = null;
            _pendingPurchaseExtCallback?.Invoke(false, "", "");
            _pendingPurchaseExtCallback = null;
        }

        #endregion

        #region PaymentManager Bridge

        /// <summary>
        /// Bridge para que AppleIAPProvider llame compras IAP programáticamente.
        /// Retorna via callback porque Unity IAP es asíncrono basado en callbacks.
        /// </summary>
        /// <summary>
        /// Callback: (bool success, string transactionId, string receiptData)
        /// receiptData es el JSON receipt de Unity IAP para validación server-side en AppleIAPProvider.
        /// </summary>
        public void PurchaseProductWithCallback(string appleProductId,
            System.Action<bool, string, string> onComplete)
        {
            _pendingPurchaseExtCallback = onComplete;
            PurchaseByProductId(appleProductId);
        }

        private System.Action<bool, string, string> _pendingPurchaseExtCallback;

        /// <summary>
        /// Bridge para restaurar compras con callback de finalización.
        /// </summary>
        public void RestorePurchases(System.Action onComplete)
        {
            _pendingRestoreCallback = onComplete;
            RestorePurchases();
        }

        private System.Action _pendingRestoreCallback;

        #endregion

        #region Debug (Solo para desarrollo)

#if UNITY_EDITOR
        [ContextMenu("Debug: Unlock Create Tournaments")]
        private void DebugUnlockCreateTournaments()
        {
            UnlockProduct(PremiumProduct.CreateTournaments);
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
            _hasStylesPro = false;
            SavePremiumStatus();
            OnPremiumStatusChanged?.Invoke();
            Debug.Log("[Premium] Estado premium reseteado");
        }
#endif

        #endregion
    }
}
