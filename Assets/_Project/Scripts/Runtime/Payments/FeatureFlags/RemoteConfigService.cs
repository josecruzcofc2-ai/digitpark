using System.Collections;
using UnityEngine;

namespace DigitPark.Payments
{
    /// <summary>
    /// Servicio de Firebase Remote Config para feature flags de pagos.
    /// Funciona en modo degradado (solo LocalFlagCache) si Firebase Remote Config
    /// no está instalado. Usar #if FIREBASE_REMOTE_CONFIG para el código de Firebase.
    /// </summary>
    public class RemoteConfigService : MonoBehaviour
    {
        private static RemoteConfigService _instance;
        public static RemoteConfigService Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindFirstObjectByType<RemoteConfigService>();
                return _instance;
            }
        }

        private const float POLL_INTERVAL_SECONDS = 900f; // 15 minutos
        private const float FETCH_TIMEOUT = 10f;

        private RemoteConfigData _currentConfig;
        private bool _isReady = false;

        public bool IsReady => _isReady;
        public RemoteConfigData GetCurrentConfig() => _currentConfig;

        public static event System.Action<RemoteConfigData> OnConfigUpdated;

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

        private void Start()
        {
            StartCoroutine(InitializeAndFetch());
            InvokeRepeating(nameof(PollRemoteConfig), POLL_INTERVAL_SECONDS, POLL_INTERVAL_SECONDS);
        }

        private void OnDestroy()
        {
            CancelInvoke();
        }

        private IEnumerator InitializeAndFetch()
        {
            // Cargar desde cache local inmediatamente para no bloquear el boot
            _currentConfig = LocalFlagCache.HasCachedFlags()
                ? LocalFlagCache.Load()
                : LocalFlagCache.GetDefaults();
            _isReady = true;
            Debug.Log("[RemoteConfig] Config inicial cargada desde cache/defaults");

#if FIREBASE_REMOTE_CONFIG
            // Firebase Remote Config fetch
            yield return StartCoroutine(FetchFromFirebase());
#else
            Debug.Log("[RemoteConfig] Firebase Remote Config no instalado - usando cache local");
            yield return null;
#endif
        }

#if FIREBASE_REMOTE_CONFIG
        private IEnumerator FetchFromFirebase()
        {
            Firebase.RemoteConfig.FirebaseRemoteConfig rcInstance;
            try { rcInstance = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance; }
            catch (System.Exception e) { Debug.LogWarning($"[RemoteConfig] DefaultInstance failed: {e.Message}"); yield break; }

            var fetchTask = rcInstance.FetchAsync(System.TimeSpan.Zero);

            float elapsed = 0f;
            while (!fetchTask.IsCompleted && elapsed < FETCH_TIMEOUT)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (fetchTask.IsCompletedSuccessfully)
            {
                // WARN-03: .AsIEnumerator() no existe en todas las versiones del Firebase SDK
                var activateTask = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
                while (!activateTask.IsCompleted) yield return null;

                var rc = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance;
                _currentConfig = new RemoteConfigData
                {
                    PaymentProvider = rc.GetValue("payment_provider").StringValue,
                    StripeEnabled = rc.GetValue("stripe_enabled").BooleanValue,
                    AppleIAPEnabled = rc.GetValue("apple_iap_enabled").BooleanValue,
                    TriumphEnabled = rc.GetValue("triumph_enabled").BooleanValue,
                    MaintenanceMode = rc.GetValue("maintenance_mode").BooleanValue,
                    AppVersion = rc.GetValue("app_version").StringValue
                };

                LocalFlagCache.Save(_currentConfig);
                PaymentFeatureFlag.UpdateFromRemoteConfig(_currentConfig);
                OnConfigUpdated?.Invoke(_currentConfig);
                Debug.Log("[RemoteConfig] Config actualizada desde Firebase");
            }
            else
            {
                Debug.LogWarning("[RemoteConfig] Fetch de Firebase fallo - usando cache local");
            }
        }
#endif

        private void PollRemoteConfig()
        {
#if FIREBASE_REMOTE_CONFIG
            StartCoroutine(FetchFromFirebase());
#endif
        }

        public void ForceRemoteSwitch(string provider)
        {
            if (_currentConfig != null)
            {
                _currentConfig.PaymentProvider = provider;
                _currentConfig.StripeEnabled = (provider == "stripe");
                LocalFlagCache.Save(_currentConfig);
                PaymentFeatureFlag.UpdateFromRemoteConfig(_currentConfig);
                Debug.Log($"[RemoteConfig] Switch forzado a: {provider}");
            }
        }
    }
}
