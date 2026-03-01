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
    /// Central manager for the Cash Battle Wallet system.
    /// Singleton that acts as a FACADE/WRAPPER for ServiceLocator.Wallet.
    /// Maintains compatibility with existing code while using the new services.
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

        // ==================== INTERNAL SERVICE ====================

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

        // ==================== PROPERTIES (Delegated to service) ====================

        /// <summary>
        /// Current user balance
        /// </summary>
        public decimal Balance => WalletService?.CurrentBalance ?? 0m;

        /// <summary>
        /// Available balance (excluding reserved funds)
        /// </summary>
        public decimal AvailableBalance => WalletService?.AvailableBalance ?? 0m;

        /// <summary>
        /// Pending verification balance
        /// </summary>
        public decimal PendingBalance => WalletService?.PendingBalance ?? 0m;

        /// <summary>
        /// Whether the user is verified for withdrawals
        /// </summary>
        public bool IsVerified => ServiceLocator.KYC?.IsFullyVerified ?? false;

        // ==================== CONFIGURATION ====================

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

        // ==================== EVENTS ====================

        /// <summary>
        /// Fired when the balance changes. Params: (newBalance, delta)
        /// </summary>
        public event Action<decimal, decimal> OnBalanceChanged;

        /// <summary>
        /// Fired when a transaction is completed
        /// </summary>
        public event Action<WalletTransaction> OnTransactionCompleted;

        /// <summary>
        /// Fired when a deposit starts
        /// </summary>
        public event Action OnDepositStarted;

        /// <summary>
        /// Fired when a deposit is completed
        /// </summary>
        public event Action<bool, string> OnDepositCompleted;

        /// <summary>
        /// Fired when a withdrawal is requested
        /// </summary>
        public event Action<WalletTransaction> OnWithdrawalRequested;

        /// <summary>
        /// Fired when KYC verification is required
        /// </summary>
        public event Action OnKYCRequired;

        private decimal _lastKnownBalance = 0m;

        // ==================== INITIALIZATION ====================

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("[WalletManager] Wrapper initialized");
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Subscribe to service events
            if (WalletService != null)
            {
                WalletService.OnBalanceChanged += HandleServiceBalanceChanged;
                WalletService.OnTransactionCompleted += HandleServiceTransactionCompleted;
                _lastKnownBalance = WalletService.CurrentBalance;
                Debug.Log($"[WalletManager] Connected to service. Balance: ${Balance:F2}");
            }
            else
            {
                Debug.LogWarning("[WalletManager] ServiceLocator.Wallet not available");
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
            // Convert to local format for compatibility
            var localTransaction = ConvertFromServiceTransaction(serviceTransaction);
            OnTransactionCompleted?.Invoke(localTransaction);
        }

        // ==================== BALANCE METHODS ====================

        /// <summary>
        /// Checks if there is sufficient available balance
        /// </summary>
        public bool HasSufficientBalance(decimal amount)
        {
            return WalletService?.HasSufficientFunds(amount) ?? false;
        }

        /// <summary>
        /// Gets the formatted balance
        /// </summary>
        public string GetFormattedBalance()
        {
            return $"${Balance:F2}";
        }

        /// <summary>
        /// Gets the formatted available balance
        /// </summary>
        public string GetFormattedAvailableBalance()
        {
            return $"${AvailableBalance:F2}";
        }

        /// <summary>
        /// Gets wallet statistics
        /// </summary>
        public (decimal deposits, decimal withdrawals, decimal net) GetStats()
        {
            // TODO: Get from service when available
            return (0m, 0m, 0m);
        }

        /// <summary>
        /// Gets wallet data (for compatibility)
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
        /// Property to access wallet data
        /// </summary>
        public WalletData WalletData => GetWalletData();

        // ==================== DEPOSIT METHODS ====================

        /// <summary>
        /// Initiates a deposit with the selected option
        /// </summary>
        public async Task<bool> InitiateDeposit(DepositOption option, PaymentMethod method)
        {
            return await InitiateDeposit(option.amount, option.bonus, method);
        }

        /// <summary>
        /// Initiates a deposit with a custom amount
        /// </summary>
        public async Task<bool> InitiateDeposit(decimal amount, decimal bonus, PaymentMethod method)
        {
            if (amount <= 0 || WalletService == null)
            {
                Debug.LogWarning("[WalletManager] Invalid deposit amount or service not available");
                return false;
            }

            OnDepositStarted?.Invoke();
            Debug.Log($"[WalletManager] Initiating deposit of ${amount:F2}");

            try
            {
                var request = new Services.DepositRequest
                {
                    Amount = amount + bonus, // The service handles the total
                    PromoCode = bonus > 0 ? "DEPOSIT_BONUS" : null
                };

                var result = await WalletService.Deposit(request);

                if (result.Success)
                {
                    // Analytics
                    AnalyticsService.Instance?.LogDeposit(amount, method.ToString());

                    OnDepositCompleted?.Invoke(true, "Deposit successful");
                    Debug.Log($"[WalletManager] Deposit completed. New balance: ${Balance:F2}");
                    return true;
                }
                else
                {
                    OnDepositCompleted?.Invoke(false, result.Message ?? "Error processing payment");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[WalletManager] Deposit error: {e.Message}");
                OnDepositCompleted?.Invoke(false, e.Message);
                return false;
            }
        }

        // ==================== WITHDRAWAL METHODS ====================

        /// <summary>
        /// Requests a withdrawal
        /// </summary>
        public async Task<bool> RequestWithdrawal(decimal amount, PaymentMethod method)
        {
            if (WalletService == null)
            {
                return false;
            }

            // Validations
            if (amount < _minimumWithdrawal)
            {
                Debug.LogWarning($"[WalletManager] Minimum withdrawal amount: ${_minimumWithdrawal:F2}");
                return false;
            }

            if (amount > _maximumWithdrawal)
            {
                Debug.LogWarning($"[WalletManager] Maximum withdrawal amount: ${_maximumWithdrawal:F2}");
                return false;
            }

            if (!HasSufficientBalance(amount))
            {
                Debug.LogWarning("[WalletManager] Insufficient balance for withdrawal");
                return false;
            }

            // Verify KYC
            if (!IsVerified)
            {
                Debug.Log("[WalletManager] KYC verification required for withdrawals");
                OnKYCRequired?.Invoke();
                return false;
            }

            Debug.Log($"[WalletManager] Requesting withdrawal of ${amount:F2}");

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
                        description = $"Withdrawal requested"
                    };
                    OnWithdrawalRequested?.Invoke(transaction);
                    Debug.Log("[WalletManager] Withdrawal requested successfully");
                    return true;
                }
                else
                {
                    Debug.LogError($"[WalletManager] Withdrawal error: {result.Message}");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[WalletManager] Withdrawal error: {e.Message}");
                return false;
            }
        }

        // ==================== GAME TRANSACTION METHODS ====================

        /// <summary>
        /// Charges entry fee for a match
        /// </summary>
        public bool ChargeEntryFee(decimal amount, string matchId, string description)
        {
            if (!HasSufficientBalance(amount))
            {
                Debug.LogWarning("[WalletManager] Insufficient balance for entry fee");
                return false;
            }

            // Reserve funds using the service
            _ = WalletService.ReserveFunds(amount, matchId);

            Debug.Log($"[WalletManager] Entry fee reserved: ${amount:F2} for {description}");
            return true;
        }

        /// <summary>
        /// Credits winnings from a match
        /// </summary>
        public void CreditWinnings(decimal amount, string matchId, string description)
        {
            // In the real service, this would come from the server
            // For now, we simulate by adding funds
            Debug.Log($"[WalletManager] Winnings credited: ${amount:F2} from {description}");

            // Create local transaction for UI
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
        /// Refunds a transaction
        /// </summary>
        public void ProcessRefund(decimal amount, string reason)
        {
            Debug.Log($"[WalletManager] Refund processed: ${amount:F2}");

            var transaction = new WalletTransaction
            {
                type = TransactionType.Refund,
                amount = amount,
                status = TransactionStatus.Completed,
                description = $"Refund: {reason}"
            };
            OnTransactionCompleted?.Invoke(transaction);
        }

        // ==================== KYC / VERIFICATION ====================

        /// <summary>
        /// Starts the KYC verification process
        /// </summary>
        public async Task<bool> StartKYCVerification()
        {
            Debug.Log("[WalletManager] Starting KYC verification");

            if (ServiceLocator.KYC == null)
            {
                return false;
            }

            var result = await ServiceLocator.KYC.StartIdentityVerification();
            return result.Success;
        }

        // ==================== HISTORY ====================

        /// <summary>
        /// Gets the transaction history
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
        /// Synchronous version for compatibility (returns empty list, use async)
        /// </summary>
        public List<WalletTransaction> GetTransactionHistory(int limit = 20)
        {
            Debug.LogWarning("[WalletManager] Synchronous GetTransactionHistory deprecated. Use GetTransactionHistoryAsync.");
            return new List<WalletTransaction>();
        }

        // ==================== SYNC WITH SERVER ====================

        /// <summary>
        /// Syncs the wallet with the server
        /// </summary>
        public async Task SyncWithServer()
        {
            Debug.Log("[WalletManager] Syncing with server...");

            if (WalletService != null)
            {
                await WalletService.RefreshBalance();
            }

            Debug.Log("[WalletManager] Sync completed");
        }

        // ==================== TYPE CONVERSION ====================

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
            Debug.Log("[WalletManager] Services reset");
        }
#endif
    }
}
