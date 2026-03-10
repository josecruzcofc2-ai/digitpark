using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using DigitPark.Services;
using DigitPark.Services.Firebase;
using DigitPark.Localization;

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
        private static readonly object _lock = new object();
        public static WalletManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
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
        public decimal Balance => WalletService?.CurrentBalance ?? 0;

        /// <summary>
        /// Available balance (excluding reserved funds)
        /// </summary>
        public decimal AvailableBalance => WalletService?.AvailableBalance ?? 0;

        /// <summary>
        /// Pending verification balance
        /// </summary>
        public decimal PendingBalance => WalletService?.PendingBalance ?? 0;

        // ==================== CONFIGURATION ====================

        [Header("Deposit Options")]
        [SerializeField] private List<DepositOption> _depositOptions = new List<DepositOption>
        {
            new DepositOption { amount = 5f },
            new DepositOption { amount = 10f },
            new DepositOption { amount = 25f },
            new DepositOption { amount = 50f },
            new DepositOption { amount = 100f },
        };

        public List<DepositOption> DepositOptions => _depositOptions;

        [Header("Withdrawal Settings")]
        [SerializeField] private float _minimumWithdrawal = 10f;
        [SerializeField] private float _maximumWithdrawal = 500f;

        public float MinimumWithdrawal => _minimumWithdrawal;
        public float MaximumWithdrawal => _maximumWithdrawal;

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

        private decimal _lastKnownBalance = 0;

        /// <summary>
        /// Prevents double-click double-charge on financial operations
        /// </summary>
        private bool _isProcessing = false;

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
                Debug.Log("[WalletManager] Connected to service. Balance: [REDACTED]");
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
        public (float deposits, float withdrawals, float net) GetStats()
        {
            // TODO: Get from service when available
            return (0f, 0f, 0f);
        }

        /// <summary>
        /// Gets wallet data (for compatibility)
        /// </summary>
        public WalletData GetWalletData()
        {
            return new WalletData
            {
                balance = (float)Balance,
                pendingBalance = (float)PendingBalance
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
            return await InitiateDeposit((decimal)option.amount, method);
        }

        /// <summary>
        /// Initiates a deposit with a custom amount
        /// </summary>
        public async Task<bool> InitiateDeposit(decimal amount, PaymentMethod method)
        {
            if (_isProcessing) return false;
            _isProcessing = true;

            try
            {
                if (amount <= 0 || WalletService == null)
                {
                    Debug.LogWarning("[WalletManager] Invalid deposit amount or service not available");
                    return false;
                }

                OnDepositStarted?.Invoke();
                Debug.Log("[WalletManager] Deposit initiated");

                var request = new Services.DepositRequest
                {
                    Amount = amount,
                    PromoCode = null
                };

                var result = await WalletService.Deposit(request);

                if (result.Success)
                {
                    // Analytics
                    AnalyticsService.Instance?.LogDeposit(amount, method.ToString());

                    OnDepositCompleted?.Invoke(true, AutoLocalizer.Get("wallet_deposit_success"));
                    Debug.Log("[WalletManager] Deposit completed successfully");
                    return true;
                }
                else
                {
                    OnDepositCompleted?.Invoke(false, result.Message ?? AutoLocalizer.Get("wallet_deposit_error"));
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[WalletManager] Deposit error: {e.Message}");
                OnDepositCompleted?.Invoke(false, e.Message);
                return false;
            }
            finally
            {
                _isProcessing = false;
            }
        }

        // ==================== WITHDRAWAL METHODS ====================

        /// <summary>
        /// Requests a withdrawal
        /// </summary>
        public async Task<bool> RequestWithdrawal(decimal amount, PaymentMethod method)
        {
            if (_isProcessing) return false;
            _isProcessing = true;

            try
            {
                if (WalletService == null)
                {
                    return false;
                }

                // Validations
                if (amount < (decimal)_minimumWithdrawal)
                {
                    Debug.LogWarning("[WalletManager] Withdrawal below minimum amount");
                    return false;
                }

                if (amount > (decimal)_maximumWithdrawal)
                {
                    Debug.LogWarning("[WalletManager] Withdrawal exceeds maximum amount");
                    return false;
                }

                if (!HasSufficientBalance(amount))
                {
                    Debug.LogWarning("[WalletManager] Insufficient balance for withdrawal");
                    return false;
                }

                Debug.Log("[WalletManager] Withdrawal requested");

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
                        amount = (float)(-amount),
                        status = TransactionStatus.Pending,
                        description = AutoLocalizer.Get("wallet_withdrawal_requested")
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
            finally
            {
                _isProcessing = false;
            }
        }

        // ==================== GAME TRANSACTION METHODS ====================

        /// <summary>
        /// Charges entry fee for a match
        /// </summary>
        public async Task<bool> ChargeEntryFee(decimal amount, string matchId, string description)
        {
            if (_isProcessing) return false;
            _isProcessing = true;

            try
            {
                if (!HasSufficientBalance(amount))
                {
                    Debug.LogWarning("[WalletManager] Insufficient balance for entry fee");
                    return false;
                }

                if (WalletService == null)
                {
                    Debug.LogError("[WalletManager] WalletService not available");
                    return false;
                }

                // Reserve funds using the service and await confirmation
                var result = await WalletService.ReserveFunds(amount, matchId);
                if (result == null || !result.Success)
                {
                    Debug.LogError($"[WalletManager] Failed to reserve funds: {result?.Message ?? "unknown error"}");
                    return false;
                }

                Debug.Log($"[WalletManager] Entry fee reserved for {description}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[WalletManager] Error reserving entry fee: {e.Message}");
                return false;
            }
            finally
            {
                _isProcessing = false;
            }
        }

        /// <summary>
        /// Credits winnings from a match
        /// </summary>
        public void CreditWinnings(decimal amount, string matchId, string description)
        {
            // In the real service, this would come from the server
            // For now, we simulate by adding funds
            Debug.Log($"[WalletManager] Winnings credited from {description}");

            // Create local transaction for UI
            var transaction = new WalletTransaction
            {
                type = TransactionType.MatchWinnings,
                amount = (float)amount,
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
            Debug.Log("[WalletManager] Refund processed");

            var transaction = new WalletTransaction
            {
                type = TransactionType.Refund,
                amount = (float)amount,
                status = TransactionStatus.Completed,
                description = AutoLocalizer.Get("wallet_refund_description", reason)
            };
            OnTransactionCompleted?.Invoke(transaction);
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
                amount = (float)serviceTransaction.Amount,
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
                await WalletService.QuickDeposit(100);
                Debug.Log("[WalletManager] Debug: Balance updated");
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
