using System;
using UnityEngine;

#if UNITY_EDITOR || HAS_TRIUMPH
namespace DigitPark.Services
{
    /// <summary>
    /// Manager centralizado para Triumph SDK.
    /// Triumph maneja: KYC, Wallet, Matchmaking, Tournaments, Geo-fencing, Pagos.
    /// Este manager es el único punto de contacto con Triumph.
    /// </summary>
    public class TriumphManager : MonoBehaviour
    {
        private static TriumphManager _instance;
        public static TriumphManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<TriumphManager>();
                }
                return _instance;
            }
        }

        [Header("Triumph Configuration")]
        [SerializeField] private string _apiKey = "";
        [SerializeField] private bool _useSandbox = true;
        [SerializeField] private bool _isEnabled = false; // false = Mock mode

        [Header("Mock Mode Settings")]
        [SerializeField] private float _mockBalance = 100f;
        [SerializeField] private bool _mockIsVerified = false;

        // Estado
        private bool _isInitialized = false;
        private decimal _balance;
        private bool _isVerified;

        // Eventos
        public static event Action OnTriumphReady;
        public static event Action<decimal> OnBalanceChanged;
        public static event Action<bool> OnVerificationChanged;
        public static event Action<string> OnGameStartRequested; // Triumph dice "juega ahora"
        public static event Action<int, decimal> OnMatchCompleted; // score, prize

        // Propiedades
        public static bool IsEnabled => Instance?._isEnabled ?? false;
        public static bool IsInitialized => Instance?._isInitialized ?? false;
        public static bool IsSandbox => Instance?._useSandbox ?? true;
        public static decimal Balance => Instance?._balance ?? 0m;
        public static bool IsVerified => Instance?._isVerified ?? false;
        public static bool CanPlayCash => IsVerified && Balance > 0;

        /// <summary>
        /// Prefijo para identificar transacciones de Triumph (nunca deben aparecer en el sistema cosmético)
        /// </summary>
        public const string TRANSACTION_PREFIX = "triumph_";

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        private void Initialize()
        {
            if (_isEnabled && !string.IsNullOrEmpty(_apiKey))
            {
                InitializeTriumph();
            }
            else
            {
                InitializeMockMode();
            }
        }

        #region Triumph Integration (TODO: Implementar cuando tengas SDK)

        private void InitializeTriumph()
        {
            Debug.Log($"[TriumphManager] Inicializando Triumph SDK (Sandbox: {_useSandbox})");

            // TODO: Descomentar cuando tengas el SDK
            // TriumphSDK.Initialize(_apiKey, _useSandbox);
            // TriumphSDK.OnReady += HandleTriumphReady;
            // TriumphSDK.OnBalanceChanged += HandleBalanceChanged;
            // TriumphSDK.OnGameStart += HandleGameStart;
            // TriumphSDK.OnMatchComplete += HandleMatchComplete;

            // Por ahora usar mock
            Debug.LogWarning("[TriumphManager] Triumph SDK no implementado. Usando Mock.");
            InitializeMockMode();
        }

        /// <summary>
        /// Abre la UI de Triumph para Cash Battle.
        /// Triumph maneja: verificación, depósitos, matchmaking, todo.
        /// </summary>
        public static void OpenTriumph()
        {
            if (!IsEnabled)
            {
                Debug.LogWarning("[TriumphManager] Triumph no habilitado. Usando flujo mock.");
                Instance?.SimulateMockFlow();
                return;
            }

            // TODO: TriumphSDK.Present();
            Debug.Log("[TriumphManager] TODO: TriumphSDK.Present()");
        }

        /// <summary>
        /// Envía el score al final de una partida Cash.
        /// </summary>
        public static void SubmitScore(int score)
        {
            if (!IsEnabled)
            {
                Instance?.SimulateMockScoreSubmit(score);
                return;
            }

            // TODO: TriumphSDK.SubmitScore(score);
            Debug.Log($"[TriumphManager] TODO: TriumphSDK.SubmitScore({score})");
        }

        /// <summary>
        /// Verifica si el usuario puede jugar Cash Battle en su ubicación.
        /// Triumph maneja geo-fencing automáticamente.
        /// </summary>
        public static bool CanAccessCashBattle()
        {
            if (!IsEnabled)
            {
                return true; // Mock siempre permite
            }

            // TODO: return TriumphSDK.IsAvailableInCurrentLocation();
            return true;
        }

        #endregion

        #region Mock Mode (Para desarrollo)

        private void InitializeMockMode()
        {
            Debug.Log("[TriumphManager] Modo MOCK activado");
            _balance = (decimal)_mockBalance;
            _isVerified = _mockIsVerified;
            _isInitialized = true;
            OnTriumphReady?.Invoke();
        }

        private void SimulateMockFlow()
        {
            Debug.Log("[TriumphManager] Mock: Simulando flujo de Triumph...");

            // Simular que el usuario ya está verificado y tiene balance
            if (!_isVerified)
            {
                _isVerified = true;
                OnVerificationChanged?.Invoke(true);
            }

            // Simular que encontró partida después de 2 segundos
            Invoke(nameof(SimulateMockMatchFound), 2f);
        }

        private void SimulateMockMatchFound()
        {
            Debug.Log("[TriumphManager] Mock: ¡Partida encontrada! Iniciando juego...");
            OnGameStartRequested?.Invoke("DigitRush"); // Triumph dice qué juego
        }

        private void SimulateMockScoreSubmit(int score)
        {
            Debug.Log($"[TriumphManager] Mock: Score enviado: {score}");

            // Simular resultado (50% win)
            bool isWinner = UnityEngine.Random.value > 0.5f;
            decimal prize = isWinner ? 9.50m : 0m;

            if (isWinner)
            {
                _balance += prize;
                OnBalanceChanged?.Invoke(_balance);
            }

            OnMatchCompleted?.Invoke(score, prize);
            Debug.Log($"[TriumphManager] Mock: Resultado - {(isWinner ? "Victoria" : "Derrota")}, Premio: ${prize}");
        }

        #endregion

#if UNITY_EDITOR
        #region Editor-Only Debug Methods

        [ContextMenu("Mock: Add $50")]
        private void MockAddBalance()
        {
            _balance += 50m;
            OnBalanceChanged?.Invoke(_balance);
            Debug.Log($"[TriumphManager] Mock: Balance = ${_balance}");
        }

        [ContextMenu("Mock: Toggle Verified")]
        private void MockToggleVerified()
        {
            _isVerified = !_isVerified;
            OnVerificationChanged?.Invoke(_isVerified);
            Debug.Log($"[TriumphManager] Mock: Verified = {_isVerified}");
        }

        [ContextMenu("Mock: Simulate Match")]
        private void MockSimulateMatch()
        {
            SimulateMockFlow();
        }

        #endregion
#endif

        #region Event Handlers (Para Triumph SDK real)

        private void HandleTriumphReady()
        {
            _isInitialized = true;
            OnTriumphReady?.Invoke();
        }

        private void HandleBalanceChanged(decimal newBalance)
        {
            _balance = newBalance;
            OnBalanceChanged?.Invoke(newBalance);
        }

        private void HandleGameStart(string gameType)
        {
            OnGameStartRequested?.Invoke(gameType);
        }

        private void HandleMatchComplete(int score, decimal prize)
        {
            OnMatchCompleted?.Invoke(score, prize);
        }

        #endregion

        private void OnDestroy()
        {
            CancelInvoke();
        }
    }
}
#endif
