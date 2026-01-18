using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DigitPark.Services
{
    /// <summary>
    /// Tipo de transacción en el wallet
    /// </summary>
    public enum TransactionType
    {
        Deposit,            // Depósito de dinero
        Withdrawal,         // Retiro de dinero
        MatchEntry,         // Entrada a partida 1v1
        MatchWin,           // Ganancia de partida
        MatchLoss,          // Pérdida (entry fee)
        TournamentEntry,    // Entrada a torneo
        TournamentPrize,    // Premio de torneo
        Bonus,              // Bonus promocional
        Refund              // Reembolso
    }

    /// <summary>
    /// Estado de una transacción
    /// </summary>
    public enum TransactionStatus
    {
        Pending,            // En proceso
        Completed,          // Completada
        Failed,             // Fallida
        Cancelled,          // Cancelada
        Refunded            // Reembolsada
    }

    /// <summary>
    /// Registro de una transacción
    /// </summary>
    public class WalletTransaction
    {
        public string TransactionId { get; set; }
        public TransactionType Type { get; set; }
        public TransactionStatus Status { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public string ReferenceId { get; set; } // Match ID, Tournament ID, etc.
    }

    /// <summary>
    /// Resultado de una operación de wallet
    /// </summary>
    public class WalletResult
    {
        public bool Success { get; set; }
        public decimal NewBalance { get; set; }
        public string TransactionId { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }

        public static WalletResult Successful(decimal balance, string transactionId = null, string message = null)
        {
            return new WalletResult
            {
                Success = true,
                NewBalance = balance,
                TransactionId = transactionId,
                Message = message
            };
        }

        public static WalletResult Failed(string message, string errorCode = null)
        {
            return new WalletResult { Success = false, Message = message, ErrorCode = errorCode };
        }
    }

    /// <summary>
    /// Información de depósito requerida
    /// </summary>
    public class DepositRequest
    {
        public decimal Amount { get; set; }
        public string PaymentMethodId { get; set; } // ID del método de pago guardado
        public string PromoCode { get; set; }       // Código promocional opcional
    }

    /// <summary>
    /// Información de retiro requerida
    /// </summary>
    public class WithdrawalRequest
    {
        public decimal Amount { get; set; }
        public string WithdrawalMethodId { get; set; } // ID del método de retiro
    }

    /// <summary>
    /// Servicio de Wallet para Cash Battle
    /// Maneja balance, depósitos, retiros y transacciones
    /// </summary>
    public interface IWalletService
    {
        /// <summary>
        /// Balance actual del usuario
        /// </summary>
        decimal CurrentBalance { get; }

        /// <summary>
        /// Balance pendiente (en proceso de verificación)
        /// </summary>
        decimal PendingBalance { get; }

        /// <summary>
        /// Balance disponible para jugar (CurrentBalance - fondos bloqueados)
        /// </summary>
        decimal AvailableBalance { get; }

        /// <summary>
        /// Si el usuario tiene suficiente balance para una cantidad
        /// </summary>
        bool HasSufficientFunds(decimal amount);

        /// <summary>
        /// Evento cuando cambia el balance
        /// </summary>
        event Action<decimal> OnBalanceChanged;

        /// <summary>
        /// Evento cuando se completa una transacción
        /// </summary>
        event Action<WalletTransaction> OnTransactionCompleted;

        /// <summary>
        /// Obtiene el balance actualizado del servidor
        /// </summary>
        Task<WalletResult> RefreshBalance();

        /// <summary>
        /// Inicia un depósito de dinero
        /// Abre el flujo de pago de Triumph/Stripe
        /// </summary>
        Task<WalletResult> Deposit(DepositRequest request);

        /// <summary>
        /// Depósito rápido con cantidad predefinida
        /// </summary>
        Task<WalletResult> QuickDeposit(decimal amount);

        /// <summary>
        /// Solicita un retiro de fondos
        /// Requiere KYC completo
        /// </summary>
        Task<WalletResult> Withdraw(WithdrawalRequest request);

        /// <summary>
        /// Retiro rápido del balance completo
        /// </summary>
        Task<WalletResult> WithdrawAll();

        /// <summary>
        /// Reserva fondos para una partida (bloquea temporalmente)
        /// </summary>
        Task<WalletResult> ReserveFunds(decimal amount, string matchId);

        /// <summary>
        /// Libera fondos reservados (si la partida se cancela)
        /// </summary>
        Task<WalletResult> ReleaseFunds(string matchId);

        /// <summary>
        /// Obtiene el historial de transacciones
        /// </summary>
        Task<List<WalletTransaction>> GetTransactionHistory(int limit = 50, int offset = 0);

        /// <summary>
        /// Obtiene los detalles de una transacción específica
        /// </summary>
        Task<WalletTransaction> GetTransaction(string transactionId);
    }
}
