using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace DigitPark.Services.Mock
{
    /// <summary>
    /// Implementación Mock del servicio Wallet para desarrollo y testing
    /// Simula todas las operaciones de dinero localmente
    /// </summary>
    public class MockWalletService : IWalletService
    {
        private const string PREFS_BALANCE = "Mock_Wallet_Balance";
        private const string PREFS_TRANSACTIONS = "Mock_Wallet_Transactions";

        private decimal _currentBalance;
        private decimal _pendingBalance;
        private Dictionary<string, decimal> _reservedFunds = new Dictionary<string, decimal>();
        private List<WalletTransaction> _transactions = new List<WalletTransaction>();

        public decimal CurrentBalance => _currentBalance;
        public decimal PendingBalance => _pendingBalance;
        public decimal AvailableBalance => _currentBalance - _reservedFunds.Values.Sum();

        public event Action<decimal> OnBalanceChanged;
        public event Action<WalletTransaction> OnTransactionCompleted;

        // Configuración de simulación
        public float SimulatedDelaySeconds { get; set; } = 1f;
        public decimal InitialMockBalance { get; set; } = 100m; // Balance inicial para testing
        public bool SimulatePaymentFailure { get; set; } = false;

        public MockWalletService()
        {
            LoadState();
        }

        private void LoadState()
        {
            // Cargar balance guardado o usar inicial
            string balanceStr = PlayerPrefs.GetString(PREFS_BALANCE, "");
            if (!string.IsNullOrEmpty(balanceStr) && decimal.TryParse(balanceStr, out decimal savedBalance))
            {
                _currentBalance = savedBalance;
            }
            else
            {
                _currentBalance = InitialMockBalance;
                SaveState();
            }

            // Cargar transacciones (simplificado - en producción usarías JSON)
            LoadTransactions();

            Debug.Log($"[MockWallet] Balance cargado: ${_currentBalance:F2}");
        }

        private void SaveState()
        {
            PlayerPrefs.SetString(PREFS_BALANCE, _currentBalance.ToString());
            SaveTransactions();
            PlayerPrefs.Save();
        }

        private void LoadTransactions()
        {
            string json = PlayerPrefs.GetString(PREFS_TRANSACTIONS, "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var wrapper = JsonUtility.FromJson<TransactionListWrapper>(json);
                    if (wrapper?.transactions != null)
                    {
                        _transactions = wrapper.transactions.Select(t => t.ToTransaction()).ToList();
                    }
                }
                catch
                {
                    _transactions = new List<WalletTransaction>();
                }
            }
        }

        private void SaveTransactions()
        {
            var wrapper = new TransactionListWrapper
            {
                transactions = _transactions.TakeLast(100).Select(t => new SerializableTransaction(t)).ToList()
            };
            string json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(PREFS_TRANSACTIONS, json);
        }

        public bool HasSufficientFunds(decimal amount) => AvailableBalance >= amount;

        private void UpdateBalance(decimal newBalance)
        {
            _currentBalance = newBalance;
            SaveState();
            OnBalanceChanged?.Invoke(newBalance);
            Debug.Log($"[MockWallet] Balance actualizado: ${newBalance:F2}");
        }

        private WalletTransaction CreateTransaction(TransactionType type, decimal amount, string description, string referenceId = null)
        {
            var transaction = new WalletTransaction
            {
                TransactionId = Guid.NewGuid().ToString(),
                Type = type,
                Status = TransactionStatus.Completed,
                Amount = amount,
                BalanceAfter = _currentBalance,
                Description = description,
                Timestamp = DateTime.UtcNow,
                ReferenceId = referenceId
            };

            _transactions.Add(transaction);
            SaveTransactions();
            OnTransactionCompleted?.Invoke(transaction);

            return transaction;
        }

        public async Task<WalletResult> RefreshBalance()
        {
            Debug.Log("[MockWallet] Refrescando balance...");
            await Task.Delay((int)(SimulatedDelaySeconds * 500));
            return WalletResult.Successful(_currentBalance);
        }

        public async Task<WalletResult> Deposit(DepositRequest request)
        {
            Debug.Log($"[MockWallet] Procesando depósito: ${request.Amount:F2}");

            await Task.Delay((int)(SimulatedDelaySeconds * 1000));

            if (SimulatePaymentFailure)
            {
                Debug.Log("[MockWallet] Simulando fallo de pago");
                return WalletResult.Failed("Error de pago simulado", "PAYMENT_FAILED");
            }

            if (request.Amount <= 0)
            {
                return WalletResult.Failed("Cantidad inválida", "INVALID_AMOUNT");
            }

            decimal bonusAmount = 0;
            // Simular bonus por código promocional
            if (!string.IsNullOrEmpty(request.PromoCode))
            {
                if (request.PromoCode.ToUpper() == "WELCOME50")
                {
                    bonusAmount = request.Amount * 0.5m; // 50% bonus
                    Debug.Log($"[MockWallet] Código promocional aplicado: +${bonusAmount:F2} bonus");
                }
            }

            decimal totalDeposit = request.Amount + bonusAmount;
            UpdateBalance(_currentBalance + totalDeposit);

            var transaction = CreateTransaction(
                TransactionType.Deposit,
                totalDeposit,
                bonusAmount > 0 ? $"Depósito ${request.Amount:F2} + Bonus ${bonusAmount:F2}" : $"Depósito ${request.Amount:F2}"
            );

            Debug.Log($"[MockWallet] Depósito exitoso. Nuevo balance: ${_currentBalance:F2}");
            return WalletResult.Successful(_currentBalance, transaction.TransactionId, "Depósito exitoso");
        }

        public async Task<WalletResult> QuickDeposit(decimal amount)
        {
            return await Deposit(new DepositRequest { Amount = amount });
        }

        public async Task<WalletResult> Withdraw(WithdrawalRequest request)
        {
            Debug.Log($"[MockWallet] Procesando retiro: ${request.Amount:F2}");

            await Task.Delay((int)(SimulatedDelaySeconds * 1500));

            if (SimulatePaymentFailure)
            {
                Debug.Log("[MockWallet] Simulando fallo de retiro");
                return WalletResult.Failed("Error de retiro simulado", "WITHDRAWAL_FAILED");
            }

            if (request.Amount <= 0)
            {
                return WalletResult.Failed("Cantidad inválida", "INVALID_AMOUNT");
            }

            if (!HasSufficientFunds(request.Amount))
            {
                return WalletResult.Failed("Fondos insuficientes", "INSUFFICIENT_FUNDS");
            }

            UpdateBalance(_currentBalance - request.Amount);

            var transaction = CreateTransaction(
                TransactionType.Withdrawal,
                -request.Amount,
                $"Retiro ${request.Amount:F2}"
            );

            Debug.Log($"[MockWallet] Retiro exitoso. Nuevo balance: ${_currentBalance:F2}");
            return WalletResult.Successful(_currentBalance, transaction.TransactionId, "Retiro procesado. Llegará en 1-3 días hábiles.");
        }

        public async Task<WalletResult> WithdrawAll()
        {
            return await Withdraw(new WithdrawalRequest { Amount = AvailableBalance });
        }

        public async Task<WalletResult> ReserveFunds(decimal amount, string matchId)
        {
            Debug.Log($"[MockWallet] Reservando ${amount:F2} para partida {matchId}");

            await Task.Delay(100);

            if (!HasSufficientFunds(amount))
            {
                return WalletResult.Failed("Fondos insuficientes", "INSUFFICIENT_FUNDS");
            }

            _reservedFunds[matchId] = amount;
            Debug.Log($"[MockWallet] Fondos reservados. Disponible: ${AvailableBalance:F2}");

            return WalletResult.Successful(AvailableBalance);
        }

        public async Task<WalletResult> ReleaseFunds(string matchId)
        {
            Debug.Log($"[MockWallet] Liberando fondos de partida {matchId}");

            await Task.Delay(100);

            if (_reservedFunds.ContainsKey(matchId))
            {
                _reservedFunds.Remove(matchId);
                Debug.Log($"[MockWallet] Fondos liberados. Disponible: ${AvailableBalance:F2}");
            }

            return WalletResult.Successful(AvailableBalance);
        }

        public async Task<List<WalletTransaction>> GetTransactionHistory(int limit = 50, int offset = 0)
        {
            await Task.Delay((int)(SimulatedDelaySeconds * 300));

            return _transactions
                .OrderByDescending(t => t.Timestamp)
                .Skip(offset)
                .Take(limit)
                .ToList();
        }

        public async Task<WalletTransaction> GetTransaction(string transactionId)
        {
            await Task.Delay((int)(SimulatedDelaySeconds * 200));
            return _transactions.FirstOrDefault(t => t.TransactionId == transactionId);
        }

        // Métodos adicionales para Mock/Testing
        public void SetBalance(decimal amount)
        {
            Debug.Log($"[MockWallet] Balance establecido manualmente: ${amount:F2}");
            UpdateBalance(amount);
        }

        public void AddFunds(decimal amount, string description = "Fondos añadidos (testing)")
        {
            UpdateBalance(_currentBalance + amount);
            CreateTransaction(TransactionType.Bonus, amount, description);
        }

        public void SimulateMatchResult(string matchId, bool won, decimal entryFee)
        {
            ReleaseFunds(matchId);

            if (won)
            {
                decimal winnings = entryFee * 1.8m; // 90% del pool (2 jugadores, 10% fee)
                UpdateBalance(_currentBalance + winnings - entryFee);
                CreateTransaction(TransactionType.MatchWin, winnings - entryFee, $"Victoria en partida", matchId);
            }
            else
            {
                UpdateBalance(_currentBalance - entryFee);
                CreateTransaction(TransactionType.MatchLoss, -entryFee, $"Derrota en partida", matchId);
            }
        }

        public void ResetWallet()
        {
            _currentBalance = InitialMockBalance;
            _pendingBalance = 0;
            _reservedFunds.Clear();
            _transactions.Clear();
            SaveState();
            OnBalanceChanged?.Invoke(_currentBalance);
            Debug.Log($"[MockWallet] Wallet reseteado. Balance: ${_currentBalance:F2}");
        }
    }

    // Clases auxiliares para serialización con JsonUtility
    [Serializable]
    public class TransactionListWrapper
    {
        public List<SerializableTransaction> transactions;
    }

    [Serializable]
    public class SerializableTransaction
    {
        public string transactionId;
        public int type;
        public int status;
        public float amount;
        public float balanceAfter;
        public string description;
        public string timestamp;
        public string referenceId;

        public SerializableTransaction() { }

        public SerializableTransaction(WalletTransaction t)
        {
            transactionId = t.TransactionId;
            type = (int)t.Type;
            status = (int)t.Status;
            amount = (float)t.Amount;
            balanceAfter = (float)t.BalanceAfter;
            description = t.Description;
            timestamp = t.Timestamp.ToString("O");
            referenceId = t.ReferenceId;
        }

        public WalletTransaction ToTransaction()
        {
            return new WalletTransaction
            {
                TransactionId = transactionId,
                Type = (TransactionType)type,
                Status = (TransactionStatus)status,
                Amount = (decimal)amount,
                BalanceAfter = (decimal)balanceAfter,
                Description = description,
                Timestamp = DateTime.Parse(timestamp),
                ReferenceId = referenceId
            };
        }
    }
}
