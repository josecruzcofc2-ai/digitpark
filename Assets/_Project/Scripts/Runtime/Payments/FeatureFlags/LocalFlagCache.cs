using UnityEngine;

namespace DigitPark.Payments
{
    /// <summary>
    /// Cache local de feature flags usando PlayerPrefs.
    /// Persiste entre sesiones y funciona offline.
    /// Prefijo "dp_" para evitar colisiones con otras claves.
    /// </summary>
    public static class LocalFlagCache
    {
        private const string KEY_PAYMENT_PROVIDER = "dp_payment_provider";
        private const string KEY_STRIPE_ENABLED = "dp_stripe_enabled";
        private const string KEY_IAP_ENABLED = "dp_iap_enabled";
        private const string KEY_TRIUMPH_ENABLED = "dp_triumph_enabled";
        private const string KEY_MAINTENANCE_MODE = "dp_maintenance_mode";
        private const string KEY_FLAGS_TIMESTAMP = "dp_flags_timestamp";
        private const string KEY_FLAGS_SOURCE = "dp_flags_source";
        private const string KEY_APP_VERSION = "dp_app_version";

        public static void Save(RemoteConfigData data)
        {
            if (data == null) return;
            PlayerPrefs.SetString(KEY_PAYMENT_PROVIDER, data.PaymentProvider);
            PlayerPrefs.SetString(KEY_STRIPE_ENABLED, data.StripeEnabled ? "true" : "false");
            PlayerPrefs.SetString(KEY_IAP_ENABLED, data.AppleIAPEnabled ? "true" : "false");
            PlayerPrefs.SetString(KEY_TRIUMPH_ENABLED, data.TriumphEnabled ? "true" : "false");
            PlayerPrefs.SetString(KEY_MAINTENANCE_MODE, data.MaintenanceMode ? "true" : "false");
            PlayerPrefs.SetString(KEY_APP_VERSION, data.AppVersion);
            PlayerPrefs.SetString(KEY_FLAGS_TIMESTAMP, System.DateTime.UtcNow.ToString("O"));
            PlayerPrefs.SetString(KEY_FLAGS_SOURCE, "remote");
            PlayerPrefs.Save();
            Debug.Log("[LocalFlagCache] Flags guardados");
        }

        public static RemoteConfigData Load()
        {
            if (!HasCachedFlags()) return GetDefaults();
            return new RemoteConfigData
            {
                PaymentProvider = PlayerPrefs.GetString(KEY_PAYMENT_PROVIDER, "apple_iap"),
                StripeEnabled = PlayerPrefs.GetString(KEY_STRIPE_ENABLED, "false") == "true",
                AppleIAPEnabled = PlayerPrefs.GetString(KEY_IAP_ENABLED, "true") == "true",
                TriumphEnabled = PlayerPrefs.GetString(KEY_TRIUMPH_ENABLED, "false") == "true",
                MaintenanceMode = PlayerPrefs.GetString(KEY_MAINTENANCE_MODE, "false") == "true",
                AppVersion = PlayerPrefs.GetString(KEY_APP_VERSION, "pro")
            };
        }

        public static bool HasCachedFlags()
            => PlayerPrefs.HasKey(KEY_FLAGS_TIMESTAMP);

        public static System.DateTime GetLastUpdateTime()
        {
            string ts = PlayerPrefs.GetString(KEY_FLAGS_TIMESTAMP, "");
            if (System.DateTime.TryParse(ts, out System.DateTime result)) return result;
            return System.DateTime.MinValue;
        }

        public static RemoteConfigData GetDefaults()
        {
            return new RemoteConfigData
            {
                PaymentProvider = "apple_iap",
                StripeEnabled = false,
                AppleIAPEnabled = true,
                TriumphEnabled = false,
                MaintenanceMode = false,
                AppVersion = "pro"
            };
        }

        public static void SaveProviderOverride(string provider)
        {
            PlayerPrefs.SetString(KEY_PAYMENT_PROVIDER, provider);
            PlayerPrefs.SetString(KEY_FLAGS_SOURCE, "local_override");
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Datos de configuración remota
    /// </summary>
    [System.Serializable]
    public class RemoteConfigData
    {
        public string PaymentProvider = "apple_iap";
        public bool StripeEnabled = false;
        public bool AppleIAPEnabled = true;
        public bool TriumphEnabled = false;
        public bool MaintenanceMode = false;
        public string AppVersion = "pro";
    }
}
