using UnityEngine;

namespace DigitPark.Payments
{
    /// <summary>
    /// Estado central de feature flags del sistema de pagos.
    /// Determina qué provider está activo y qué versión de la app corre.
    /// </summary>
    public static class PaymentFeatureFlag
    {
        private static PaymentProvider _activeCosmeticProvider = PaymentProvider.AppleIAP;
        private static AppVersion _currentVersion = AppVersion.Pro;
        private static bool _stripeEnabled = false;
        private static bool _appleIAPEnabled = true;
        private static bool _triumphEnabled = false;
        private static bool _maintenanceMode = false;
        private static bool _initialized = false;

        public static PaymentProvider ActiveCosmeticProvider => _activeCosmeticProvider;
        public static AppVersion CurrentVersion => _currentVersion;
        public static bool IsStripeEnabled => _stripeEnabled;
        public static bool IsAppleIAPEnabled => _appleIAPEnabled;
        public static bool IsTriumphEnabled => _triumphEnabled;
        public static bool IsMaintenanceMode => _maintenanceMode;
        public static bool IsProVersion => _currentVersion == AppVersion.Pro;
        public static bool IsInitialized => _initialized;

        public static void Initialize(RemoteConfigData data)
        {
            // Detectar versión automáticamente por scripting defines
#if DIGIT_PARK_PRO
            _currentVersion = AppVersion.Pro;
#elif DIGIT_PARK_GLOBAL
            _currentVersion = AppVersion.Global;
#else
            _currentVersion = AppVersion.Pro; // Default para desarrollo
#endif

            if (data != null)
            {
                ApplyConfig(data);
                Debug.Log($"[FeatureFlag] Inicializado desde Remote Config: provider={_activeCosmeticProvider}, stripe={_stripeEnabled}");
            }
            else
            {
                // Usar cache local o defaults
                var cached = LocalFlagCache.HasCachedFlags()
                    ? LocalFlagCache.Load()
                    : LocalFlagCache.GetDefaults();
                ApplyConfig(cached);
                Debug.Log("[FeatureFlag] Inicializado desde cache local (offline mode)");
            }

            // En versión Global, Stripe y Triumph siempre deshabilitados
            if (_currentVersion == AppVersion.Global)
            {
                _stripeEnabled = false;
                _triumphEnabled = false;
                if (_activeCosmeticProvider == PaymentProvider.Stripe)
                    _activeCosmeticProvider = PaymentProvider.AppleIAP;
            }

            _initialized = true;
        }

        private static void ApplyConfig(RemoteConfigData data)
        {
            _stripeEnabled = data.StripeEnabled;
            _appleIAPEnabled = data.AppleIAPEnabled;
            _triumphEnabled = data.TriumphEnabled;
            _maintenanceMode = data.MaintenanceMode;

            _activeCosmeticProvider = data.PaymentProvider == "stripe" && data.StripeEnabled
                ? PaymentProvider.Stripe
                : PaymentProvider.AppleIAP;
        }

        public static void ForceSwitch(PaymentProvider provider, string reason)
        {
            var previous = _activeCosmeticProvider;
            _activeCosmeticProvider = provider;

            // Actualizar flags según provider
            if (provider == PaymentProvider.AppleIAP)
                _stripeEnabled = false;
            else if (provider == PaymentProvider.Stripe)
                _stripeEnabled = true;

            // Persistir en cache local
            LocalFlagCache.SaveProviderOverride(provider == PaymentProvider.Stripe ? "stripe" : "apple_iap");

            Debug.Log($"[FeatureFlag] Switch forzado: {previous} -> {provider}. Razon: {reason}");
            PaymentEvents.EmitProviderSwitched(provider, reason);
        }

        public static void UpdateFromRemoteConfig(RemoteConfigData data)
        {
            if (data == null) return;
            ApplyConfig(data);
            LocalFlagCache.Save(data);
            Debug.Log("[FeatureFlag] Actualizado desde Remote Config");
        }

        public static void SetMaintenanceMode(bool enabled)
        {
            _maintenanceMode = enabled;
            Debug.Log($"[FeatureFlag] Modo mantenimiento: {enabled}");
        }
    }
}
