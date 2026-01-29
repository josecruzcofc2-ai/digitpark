using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using DigitPark.Services;
using DigitPark.Services.Firebase;

namespace DigitPark.CashBattle
{
    /// <summary>
    /// Manager central del sistema de Wallet para Cash Battle.
    /// Singleton que actúa como FACADE/WRAPPER del ServiceLocator.Wallet.
    /// Mantiene compatibilidad con código existente mientras usa los nuevos servicios.
    /// </summary>
    public class WalletManager : MonoBehaviour
    {
        private static WalletManager _instance;
        public static WalletManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<WalletManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("WalletManager");
                        _instance = go.AddComponent<WalletManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        // ==================== SERVICIO INTERNO ====================

        private IWalletService _walletService;
        private IWalletService WalletService
        {
            get
            {
                if (_walletService == null)
                {
                    _walletService = ServiceLocator.Wallet;
                }
                return _walletService;
            }
        }

        // ==================== PROPIEDADES (Delegadas al servicio) ====================

        /// <summary>
        /// Balance actual del usuario
        /// </summary>
        public decimal Balance => WalletService?.CurrentBalance ?? 0m;

        /// <summary>
        /// Balance disponible (sin fondos reservados)
        /// </summary>
        public decimal AvailableBalance => WalletService?.AvailableBalance ?? 0m;

        /// <summary>
        /// Balance pendiente de verificación
        /// </summary>
        public decimal PendingBalance => WalletService?.PendingBalance ?? 0m;

        /// <summary>
        /// Si el usuario está verificado para retiros
        /// </summary>
        public bool IsVerified => ServiceLocator.KYC?.IsFullyVerified ?? false;

        // ==================== CONFIGURACIÓN ====================

        [Header("Deposit Options")]
        [SerializeField] private List<DepositOption> _depositOptions = new List<DepositOption>
        {
            new DepositOption { amount = 5m, bonus = 0m, isPopular = false },
            new DepositOption { amount = 10m, bonus = 1m, isPopular = false },
            new DepositOption { amount = 25m, bonus = 3m, isPopular = true },
            new DepositOption { amount = 50m, bonus = 7m, isPopular = false },
            new DepositOption { amount = 100m, bonus = 15m, isPopular = false },
        };

        public List<DepositOption> DepositOptions => _depositOptions;

        [Header("Withdrawal Settings")]
        [SerializeField] private decimal _minimumWithdrawal = 10m;
        [SerializeField] private decimal _maximumWithdrawal = 500m;

        public decimal MinimumWithdrawal => _minimumWithdrawal;
        public decimal MaximumWithdrawal => _maximumWithdrawal;

        // ==================== EVENTOS ====================

        /// <summary>
        /// Se dispara cuando el balance cambia. Params: (newBalance, delta)
        /// </summary>
        public event Action<decimal, decimal> OnBalanceChanged;

        /// <summary>
        /// Se dispara cuando una transacción se completa
        /// </summary>
        public event Action<WalletTransaction> OnTransactionCompleted;

        /// <summary>
        /// Se dispara cuando un depósito inicia
        /// </summary>
        public event Action OnDepositStarted;

        /// <summary>
        /// Se dispara cuando un depósito se completa
        /// </summary>
        public event Action<bool, string> OnDepositCompleted;

        /// <summary>
        /// Se dispara cuando un retiro se solicita
        /// </summary>
        public event Action<WalletTransaction> OnWithdrawalRequested;

        /// <summary>
        /// Se dispara cuando se necesita verificación KYC
        /// </summary>
        public event Action OnKYCRequired;

        private decimal _lastKnownBalance = 0m;

        // ==================== INICIALIZACIÓN ====================

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("[WalletManager] Wrapper iniciado");
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Suscribirse a eventos del servicio
            if (WalletService != null)
            {
                WalletService.OnBalanceChanged += HandleServiceBalanceChanged;
                WalletService.OnTransactionCompleted += HandleServiceTransactionCompleted;
                _lastKnownBalance = WalletService.CurrentBalance;
                Debug.Log($"[WalletManager] Conectado a servicio. Balance: ${Balance:F2}");
            }
            else
            {
                Debug.LogWarning("[WalletManager] ServiceLocator.Wallet no disponible");
            }
        }

        private void OnDestroy()
        {
            if (_walletService != null)
            {
                _walletService.OnBalanceChanged -= HandleServiceBalanceChanged;
                _walletService.OnTransactionCompleted -= HandleServiceTransactionCompleted;
            }

            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void HandleServiceBalanceChanged(decimal newBalance)
        {
            decimal delta = newBalance - _lastKnownBalance;
            _lastKnownBalance = newBalance;
            OnBalanceChanged?.Invoke(newBalance, delta);
        }

        private void HandleServiceTransactionCompleted(Services.WalletTransaction serviceTransaction)
        {
            // Convertir a formato local para compatibilidad
            var localTransaction = ConvertFromServiceTransaction(serviceTransaction);
            OnTransactionCompleted?.Invoke(localTransaction);
        }

        // ==================== BALANCE METHODS ====================

        /// <summary>
        /// Verifica si hay suficiente balance disponible
        /// </summary>
        public bool HasSufficientBalance(decimal amount)
        {
            return WalletService?.HasSufficientFunds(amount) ?? false;
        }

        /// <summary>
        /// Obtiene el balance formateado
        /// </summary>
        public string GetFormattedBalance()
        {
            return $"${Balance:F2}";
        }

        /// <summary>
        /// Obtiene el balance disponible formateado
        /// </summary>
        public string GetFormattedAvailableBalance()
        {
            return $"${AvailableBalance:F2}";
        }

        /// <summary>
        /// Obtiene estadísticas del wallet
        /// </summary>
        public (decimal deposits, decimal withdrawals, decimal net) GetStats()
        {
            // TODO: Obtener del servicio cuando esté disponible
            return (0m, 0m, 0m);
        }

        /// <summary>
        /// Obtiene datos del wallet (para compatibilidad)
        /// </summary>
        public WalletData GetWalletData()
        {
            return new WalletData
            {
                balance = Balance,
                pendingBalance = PendingBalance,
                isVerified = IsVerified
            };
        }

        /// <summary>
        /// Propiedad para acceder a datos del wallet
        /// </summary>
        public WalletData WalletData => GetWalletData();

        // ==================== DEPOSIT METHODS ====================

        /// <summary>
        /// Inicia un depósito con la opción seleccionada
        /// </summary>
        public async Task<bool> InitiateDeposit(DepositOption option, PaymentMethod method)
        {
            return await InitiateDeposit(option.amount, option.bonus, method);
        }

        /// <summary>
        /// Inicia un depósito con monto personalizado
        /// </summary>
        public async Task<bool> InitiateDeposit(decimal amount, decimal bonus, PaymentMethod method)
        {
            if (amount <= 0 || WalletService == null)
            {
                Debug.LogWarning("[WalletManager] Monto de depósito inválido o servicio no disponible");
                return false;
            }

            OnDepositStarted?.Invoke();
            Debug.Log($"[WalletManager] Iniciando depósito de ${amount:F2}");

            try
            {
                var request = new Services.DepositRequest
                {
                    Amount = amount + bonus, // El servicio maneja el total
                    PromoCode = bonus > 0 ? "DEPOSIT_BONUS" : null
                };

                var result = await WalletService.Deposit(request);

                if (result.Success)
                {
                    // Analytics
                    AnalyticsService.Instance?.LogDeposit(amount, method.ToString());

                    OnDepositCompleted?.Invoke(true, "Depósito exitoso");
                    Debug.Log($"[WalletManager] Depósito completado. Nuevo balance: ${Balance:F2}");
                    return true;
                }
                else
                {
                    OnDepositCompleted?.Invoke(false, result.Message ?? "Error al procesar el pago");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[WalletManager] Error en depósito: {e.Message}");
                OnDepositCompleted?.Invoke(false, e.Message);
                return false;
            }
        }

        // ==================== WITHDRAWAL METHODS ====================

        /// <summary>
        /// Solicita un retiro
        /// </summary>
        public async Task<bool> RequestWithdrawal(decimal amount, PaymentMethod method)
        {
            if (WalletService == null)
            {
                return false;
            }

            // Validaciones
            if (amount < _minimumWithdrawal)
            {
                Debug.LogWarning($"[WalletManager] Monto mínimo de retiro: ${_minimumWithdrawal:F2}");
                return false;
            }

            if (amount > _maximumWithdrawal)
            {
                Debug.LogWarning($"[WalletManager] Monto máximo de retiro: ${_maximumWithdrawal:F2}");
                return false;
            }

            if (!HasSufficientBalance(amount))
            {
                Debug.LogWarning("[WalletManager] Balance insuficiente para retiro");
                return false;
            }

            // Verificar KYC
            if (!IsVerified)
            {
                Debug.Log("[WalletManager] Se requiere verificación KYC para retiros");
                OnKYCRequired?.Invoke();
                return false;
            }

            Debug.Log($"[WalletManager] Solicitando retiro de ${amount:F2}");

            try
            {
                var request = new Services.WithdrawalRequest
                {
                    Amount = amount,
                    WithdrawalMethodId = method.ToString()
                };

                var result = await WalletService.Withdraw(request);

                if (result.Success)
                {
                    // Analytics
                    AnalyticsService.Instance?.LogWithdrawal(amount, method.ToString());

                    var transaction = new WalletTransaction
                    {
                        type = TransactionType.Withdrawal,
                        amount = -amount,
                        status = TransactionStatus.Pending,
                        description = $"Retiro solicitado"
                    };
                    OnWithdrawalRequested?.Invoke(transaction);
                    Debug.Log("[WalletManager] Retiro solicitado exitosamente");
                    return true;
                }
                else
                {
                    Debug.LogError($"[WalletManager] Error en retiro: {result.Message}");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[WalletManager] Error en retiro: {e.Message}");
                return false;
            }
        }

        // ==================== GAME TRANSACTION METHODS ====================

        /// <summary>
        /// Cobra entry fee para una partida
        /// </summary>
        public bool ChargeEntryFee(decimal amount, string matchId, string description)
        {
            if (!HasSufficientBalance(amount))
            {
                Debug.LogWarning("[WalletManager] Balance insuficiente para entry fee");
                return false;
            }

            // Reservar fondos usando el servicio
            _ = WalletService.ReserveFunds(amount, matchId);

            Debug.Log($"[WalletManager] Entry fee reservado: ${amount:F2} para {description}");
            return true;
        }

        /// <summary>
        /// Acredita ganancias de una partida
        /// </summary>
        public void CreditWinnings(decimal amount, string matchId, string description)
        {
            // En el servicio real, esto vendría del servidor
            // Por ahora, simulamos añadiendo fondos
            Debug.Log($"[WalletManager] Ganancias acreditadas: ${amount:F2} de {description}");

            // Crear transacción local para UI
            var transaction = new WalletTransaction
            {
                type = TransactionType.MatchWinnings,
                amount = amount,
                status = TransactionStatus.Completed,
                description = description,
                referenceId = matchId
            };
            OnTransactionCompleted?.Invoke(transaction);
        }

        /// <summary>
        /// Reembolsa una transacción
        /// </summary>
        public void ProcessRefund(decimal amount, string reason)
        {
            Debug.Log($"[WalletManager] Reembolso procesado: ${amount:F2}");

            var transaction = new WalletTransaction
            {
                type = TransactionType.Refund,
                amount = amount,
                status = TransactionStatus.Completed,
                description = $"Reembolso: {reason}"
            };
            OnTransactionCompleted?.Invoke(transaction);
        }

        // ==================== KYC / VERIFICATION ====================

        /// <summary>
        /// Inicia el proceso de verificación KYC
        /// </summary>
        public async Task<bool> StartKYCVerification()
        {
            Debug.Log("[WalletManager] Iniciando verificación KYC");

            if (ServiceLocator.KYC == null)
            {
                return false;
            }

            var result = await ServiceLocator.KYC.StartIdentityVerification();
            return result.Success;
        }

        // ==================== HISTORY ====================

        /// <summary>
        /// Obtiene el historial de transacciones
        /// </summary>
        public async Task<List<WalletTransaction>> GetTransactionHistoryAsync(int limit = 20)
        {
            if (WalletService == null)
            {
                return new List<WalletTransaction>();
            }

            var serviceTransactions = await WalletService.GetTransactionHistory(limit);
            return serviceTransactions.Select(ConvertFromServiceTransaction).ToList();
        }

        /// <summary>
        /// Versión síncrona para compatibilidad (retorna lista vacía, usar async)
        /// </summary>
        public List<WalletTransaction> GetTransactionHistory(int limit = 20)
        {
            Debug.LogWarning("[WalletManager] GetTransactionHistory síncrono deprecated. Usar GetTransactionHistoryAsync.");
            return new List<WalletTransaction>();
        }

        // ==================== SYNC WITH SERVER ====================

        /// <summary>
        /// Sincroniza el wallet con el servidor
        /// </summary>
        public async Task SyncWithServer()
        {
            Debug.Log("[WalletManager] Sincronizando con servidor...");

            if (WalletService != null)
            {
                await WalletService.RefreshBalance();
            }

            Debug.Log("[WalletManager] Sincronización completada");
        }

        // ==================== CONVERSIÓN DE TIPOS ====================

        private WalletTransaction ConvertFromServiceTransaction(Services.WalletTransaction serviceTransaction)
        {
            return new WalletTransaction
            {
                id = serviceTransaction.TransactionId,
                type = ConvertTransactionType(serviceTransaction.Type),
                status = ConvertTransactionStatus(serviceTransaction.Status),
                amount = serviceTransaction.Amount,
                description = serviceTransaction.Description,
                timestamp = serviceTransaction.Timestamp,
                referenceId = serviceTransaction.ReferenceId
            };
        }

        private TransactionType ConvertTransactionType(Services.TransactionType serviceType)
        {
            return serviceType switch
            {
                Services.TransactionType.Deposit => TransactionType.Deposit,
                Services.TransactionType.Withdrawal => TransactionType.Withdrawal,
                Services.TransactionType.MatchEntry => TransactionType.MatchEntry,
                Services.TransactionType.MatchWin => TransactionType.MatchWinnings,
                Services.TransactionType.TournamentEntry => TransactionType.TournamentEntry,
                Services.TransactionType.TournamentPrize => TransactionType.TournamentPrize,
                Services.TransactionType.Bonus => TransactionType.Bonus,
                Services.TransactionType.Refund => TransactionType.Refund,
                _ => TransactionType.Deposit
            };
        }

        private TransactionStatus ConvertTransactionStatus(Services.TransactionStatus serviceStatus)
        {
            return serviceStatus switch
            {
                Services.TransactionStatus.Pending => TransactionStatus.Pending,
                Services.TransactionStatus.Completed => TransactionStatus.Completed,
                Services.TransactionStatus.Failed => TransactionStatus.Failed,
                Services.TransactionStatus.Cancelled => TransactionStatus.Failed,
                Services.TransactionStatus.Refunded => TransactionStatus.Completed,
                _ => TransactionStatus.Pending
            };
        }

        // ==================== DEBUG ====================

#if UNITY_EDITOR
        [ContextMenu("Debug: Add $100")]
        private async void DebugAdd100()
        {
            if (WalletService != null)
            {
                await WalletService.QuickDeposit(100m);
                Debug.Log($"[WalletManager] Debug: Balance: ${Balance:F2}");
            }
        }

        [ContextMenu("Debug: Reset Services")]
        private void DebugResetServices()
        {
            ServiceLocator.Instance?.ResetMockServices();
            Debug.Log("[WalletManager] Servicios reseteados");
        }
#endif
    }
}
