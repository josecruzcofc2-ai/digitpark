using System;
using UnityEngine;
using DigitPark.Services.Mock;

namespace DigitPark.Services
{
    /// <summary>
    /// Modo de operación de los servicios
    /// </summary>
    public enum ServiceMode
    {
        [Tooltip("Usa servicios simulados (Mock) - Para desarrollo y testing")]
        Mock,

        [Tooltip("Usa servicios reales de Triumph - Para producción")]
        Production
    }

    /// <summary>
    /// Punto central para acceder a todos los servicios de Cash Battle
    /// Agrega este componente a un GameObject en tu escena Boot
    /// </summary>
    public class ServiceLocator : MonoBehaviour
    {
        #region Singleton
        private static ServiceLocator _instance;

        /// <summary>
        /// True si existe una instancia de ServiceLocator (no lanza error)
        /// </summary>
        public static bool Exists
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<ServiceLocator>();
                return _instance != null;
            }
        }

        public static ServiceLocator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ServiceLocator>();
                    if (_instance == null)
                    {
                        Debug.LogWarning("[ServiceLocator] No se encontró instancia. ServiceLocator debe estar en la escena Boot.");
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Inspector Settings
        [Header("=== CONFIGURACIÓN DE SERVICIOS ===")]
        [Space(10)]

        [Tooltip("Modo de operación de los servicios")]
        [SerializeField] private ServiceMode _serviceMode = ServiceMode.Mock;

        [Space(20)]
        [Header("=== CONFIGURACIÓN MOCK ===")]
        [Tooltip("Balance inicial para testing (solo en modo Mock)")]
        [SerializeField] private float _mockInitialBalance = 100f;

        [Tooltip("Delay simulado para operaciones de red (segundos)")]
        [SerializeField] [Range(0.1f, 5f)] private float _mockDelaySeconds = 1f;

        [Tooltip("Simular fallos de pago (para testing de errores)")]
        [SerializeField] private bool _simulatePaymentFailures = false;

        [Tooltip("Siempre encontrar oponente en matchmaking")]
        [SerializeField] private bool _alwaysFindMatch = true;

        [Space(20)]
        [Header("=== CONFIGURACIÓN TRIUMPH (Producción) ===")]
        [Tooltip("API Key de Triumph (solo producción)")]
        [SerializeField] private string _triumphApiKey = "";

        [Tooltip("Entorno de Triumph")]
        #pragma warning disable 0414
        [SerializeField] private bool _triumphSandbox = true;
        #pragma warning restore 0414

        [Space(20)]
        [Header("=== DEBUG ===")]
        [SerializeField] private bool _showDebugLogs = true;
        #endregion

        #region Services
        private IKYCService _kycService;
        private IWalletService _walletService;
        private IMatchmakingService _matchmakingService;
        private ITournamentService _tournamentService;

        // Acceso estático a los servicios
        public static IKYCService KYC => Instance?._kycService;
        public static IWalletService Wallet => Instance?._walletService;
        public static IMatchmakingService Matchmaking => Instance?._matchmakingService;
        public static ITournamentService Tournament => Instance?._tournamentService;

        // Propiedades de estado
        public static bool IsInitialized => Instance != null && Instance._isInitialized;
        public static ServiceMode CurrentMode => Instance?._serviceMode ?? ServiceMode.Mock;
        public static bool IsMockMode => CurrentMode == ServiceMode.Mock;
        public static bool IsProductionMode => CurrentMode == ServiceMode.Production;
        #endregion

        #region Events
        /// <summary>
        /// Evento cuando los servicios están listos
        /// </summary>
        public static event Action OnServicesReady;

        /// <summary>
        /// Evento cuando cambia el modo de servicio
        /// </summary>
        public static event Action<ServiceMode> OnServiceModeChanged;
        #endregion

        private bool _isInitialized = false;

        #region Unity Lifecycle
        private void Awake()
        {
            // Singleton setup
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("[ServiceLocator] Ya existe una instancia. Destruyendo duplicado.");
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeServices();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnValidate()
        {
            // Re-inicializar si cambia el modo en el Inspector durante Play Mode
            if (Application.isPlaying && _isInitialized)
            {
                Debug.Log($"[ServiceLocator] Modo cambiado a: {_serviceMode}");
                InitializeServices();
                OnServiceModeChanged?.Invoke(_serviceMode);
            }
        }
        #endregion

        #region Initialization
        private void InitializeServices()
        {
            Log($"Inicializando servicios en modo: {_serviceMode}");

            switch (_serviceMode)
            {
                case ServiceMode.Mock:
                    InitializeMockServices();
                    break;

                case ServiceMode.Production:
                    InitializeTriumphServices();
                    break;
            }

            _isInitialized = true;
            OnServicesReady?.Invoke();

            Log("Servicios inicializados correctamente");
        }

        private void InitializeMockServices()
        {
            Log("Configurando servicios MOCK...");

            // KYC Service
            var mockKYC = new MockKYCService
            {
                SimulatedDelaySeconds = _mockDelaySeconds,
                SimulateVerificationFailure = false
            };
            _kycService = mockKYC;

            // Wallet Service
            var mockWallet = new MockWalletService
            {
                SimulatedDelaySeconds = _mockDelaySeconds,
                InitialMockBalance = (decimal)_mockInitialBalance,
                SimulatePaymentFailure = _simulatePaymentFailures
            };
            _walletService = mockWallet;

            // Matchmaking Service
            var mockMatchmaking = new MockMatchmakingService
            {
                MinSearchTimeSeconds = _mockDelaySeconds,
                MaxSearchTimeSeconds = _mockDelaySeconds * 3,
                AlwaysFindMatch = _alwaysFindMatch
            };
            _matchmakingService = mockMatchmaking;

            // Tournament Service
            var mockTournament = new MockTournamentService
            {
                SimulatedDelaySeconds = _mockDelaySeconds
            };
            _tournamentService = mockTournament;

            Log("Servicios MOCK configurados");
        }

        private void InitializeTriumphServices()
        {
            Log("Configurando servicios de PRODUCCIÓN (Triumph)...");

            if (string.IsNullOrEmpty(_triumphApiKey))
            {
                Debug.LogError("[ServiceLocator] ¡API Key de Triumph no configurada! Usando Mock como fallback.");
                InitializeMockServices();
                return;
            }

            // TODO: Implementar servicios reales de Triumph
            // Por ahora, mostrar advertencia y usar Mock
            Debug.LogWarning("[ServiceLocator] Servicios de Triumph no implementados aún. Usando Mock.");

            // Cuando implementes Triumph:
            // _kycService = new TriumphKYCService(_triumphApiKey, _triumphSandbox);
            // _walletService = new TriumphWalletService(_triumphApiKey, _triumphSandbox);
            // _matchmakingService = new TriumphMatchmakingService(_triumphApiKey, _triumphSandbox);
            // _tournamentService = new TriumphTournamentService(_triumphApiKey, _triumphSandbox);

            InitializeMockServices(); // Fallback temporal
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Cambia el modo de servicio en runtime.
        /// Restricted to editor/tests to prevent runtime mode switching in production.
        /// </summary>
#if UNITY_EDITOR
        public void SetServiceMode(ServiceMode mode)
#else
        internal void SetServiceMode(ServiceMode mode)
#endif
        {
            if (_serviceMode != mode)
            {
                _serviceMode = mode;
                InitializeServices();
                OnServiceModeChanged?.Invoke(mode);
            }
        }

        /// <summary>
        /// Obtiene información de estado de todos los servicios.
        /// Restricted to prevent exposing internal state in production builds.
        /// </summary>
#if UNITY_EDITOR
        public ServiceStatusInfo GetServiceStatus()
#else
        internal ServiceStatusInfo GetServiceStatus()
#endif
        {
            return new ServiceStatusInfo
            {
                Mode = _serviceMode,
                IsInitialized = _isInitialized,
                KYCStatus = _kycService?.CurrentStatus ?? KYCStatus.NotStarted,
                WalletBalance = _walletService?.CurrentBalance ?? 0,
                IsSearchingMatch = _matchmakingService?.IsSearching ?? false,
                IsInTournament = _tournamentService?.IsInTournament ?? false
            };
        }

        /// <summary>
        /// Reset completo de todos los servicios Mock (solo para testing).
        /// Restricted to editor/tests to prevent runtime reset in production.
        /// </summary>
#if UNITY_EDITOR
        public void ResetMockServices()
#else
        internal void ResetMockServices()
#endif
        {
            if (_serviceMode != ServiceMode.Mock)
            {
                Debug.LogWarning("[ServiceLocator] Reset solo disponible en modo Mock");
                return;
            }

            Log("Reseteando todos los servicios Mock...");

            if (_kycService is MockKYCService mockKYC)
                _ = mockKYC.ResetVerification();

#if UNITY_EDITOR
            if (_walletService is MockWalletService mockWallet)
                mockWallet.ResetWallet();
#endif

            Log("Servicios Mock reseteados");
        }
        #endregion

        #region Debug Helpers
        private void Log(string message)
        {
            if (_showDebugLogs)
                Debug.Log($"[ServiceLocator] {message}");
        }
        #endregion
    }

    /// <summary>
    /// Información de estado de los servicios
    /// </summary>
    public class ServiceStatusInfo
    {
        public ServiceMode Mode { get; set; }
        public bool IsInitialized { get; set; }
        public KYCStatus KYCStatus { get; set; }
        public decimal WalletBalance { get; set; }
        public bool IsSearchingMatch { get; set; }
        public bool IsInTournament { get; set; }
    }
}
