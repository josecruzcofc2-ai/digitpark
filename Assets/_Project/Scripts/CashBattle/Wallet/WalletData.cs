using UnityEngine;
using System;
using System.Collections.Generic;

namespace DigitPark.CashBattle
{
    /// <summary>
    /// Tipos de transacción de wallet
    /// </summary>
    public enum TransactionType
    {
        Deposit,            // Depósito de dinero
        Withdrawal,         // Retiro de dinero
        EntryFee,           // Pago de entrada (legacy)
        MatchEntry,         // Entrada a partida 1v1
        Winnings,           // Ganancias (legacy)
        MatchWinnings,      // Ganancias de partida 1v1
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
        Pending,        // En proceso
        Completed,      // Completada
        Failed,         // Fallida
        Cancelled       // Cancelada
    }

    /// <summary>
    /// Métodos de pago soportados
    /// </summary>
    public enum PaymentMethod
    {
        CreditCard,
        DebitCard,
        PayPal,
        ApplePay,
        GooglePay,
        BankTransfer
    }

    /// <summary>
    /// Representa una transacción individual del wallet
    /// </summary>
    [Serializable]
    public class WalletTransaction
    {
        public string transactionId;

        // Alias for compatibility with ServiceLocator
        public string id { get => transactionId; set => transactionId = value; }
        public TransactionType type;
        public TransactionStatus status;
        public decimal amount;
        public decimal balanceAfter;
        public DateTime timestamp;
        public string description;
        public string referenceId;      // ID de partida/torneo si aplica
        public PaymentMethod? paymentMethod;

        public WalletTransaction()
        {
            transactionId = Guid.NewGuid().ToString();
            timestamp = DateTime.UtcNow;
            status = TransactionStatus.Pending;
        }

        /// <summary>
        /// Crea una transacción de depósito
        /// </summary>
        public static WalletTransaction CreateDeposit(decimal amount, PaymentMethod method)
        {
            return new WalletTransaction
            {
                type = TransactionType.Deposit,
                amount = amount,
                paymentMethod = method,
                description = $"Depósito vía {method}"
            };
        }

        /// <summary>
        /// Crea una transacción de retiro
        /// </summary>
        public static WalletTransaction CreateWithdrawal(decimal amount, PaymentMethod method)
        {
            return new WalletTransaction
            {
                type = TransactionType.Withdrawal,
                amount = -amount, // Negativo para retiros
                paymentMethod = method,
                description = $"Retiro vía {method}"
            };
        }

        /// <summary>
        /// Crea una transacción de entry fee
        /// </summary>
        public static WalletTransaction CreateEntryFee(decimal amount, string matchId, string matchDescription)
        {
            return new WalletTransaction
            {
                type = TransactionType.EntryFee,
                amount = -amount,
                referenceId = matchId,
                description = matchDescription
            };
        }

        /// <summary>
        /// Crea una transacción de ganancias
        /// </summary>
        public static WalletTransaction CreateWinnings(decimal amount, string matchId, string matchDescription)
        {
            return new WalletTransaction
            {
                type = TransactionType.Winnings,
                amount = amount,
                referenceId = matchId,
                description = matchDescription
            };
        }

        /// <summary>
        /// Obtiene el color asociado al tipo de transacción
        /// </summary>
        public Color GetTypeColor()
        {
            switch (type)
            {
                case TransactionType.Deposit:
                case TransactionType.Winnings:
                case TransactionType.Bonus:
                    return new Color(0f, 1f, 0.5f, 1f); // Verde
                case TransactionType.Withdrawal:
                case TransactionType.EntryFee:
                    return new Color(1f, 0.4f, 0.4f, 1f); // Rojo
                case TransactionType.Refund:
                    return new Color(1f, 0.84f, 0f, 1f); // Dorado
                default:
                    return Color.white;
            }
        }

        /// <summary>
        /// Obtiene el icono/símbolo para el tipo
        /// </summary>
        public string GetTypeSymbol()
        {
            return amount >= 0 ? "+" : "";
        }

        /// <summary>
        /// Formatea el monto para display
        /// </summary>
        public string GetFormattedAmount()
        {
            return $"{GetTypeSymbol()}${Math.Abs(amount):F2}";
        }

        /// <summary>
        /// Formatea la fecha para display
        /// </summary>
        public string GetFormattedDate()
        {
            TimeSpan timeSince = DateTime.UtcNow - timestamp;

            if (timeSince.TotalMinutes < 1)
                return "Ahora";
            if (timeSince.TotalMinutes < 60)
                return $"Hace {(int)timeSince.TotalMinutes}m";
            if (timeSince.TotalHours < 24)
                return $"Hace {(int)timeSince.TotalHours}h";
            if (timeSince.TotalDays < 7)
                return $"Hace {(int)timeSince.TotalDays}d";

            return timestamp.ToString("dd/MM/yyyy");
        }
    }

    /// <summary>
    /// Datos del wallet del usuario
    /// </summary>
    [Serializable]
    public class WalletData
    {
        public string userId;
        public decimal balance;
        public decimal pendingBalance;      // Balance en proceso (retiros pendientes)
        public decimal lifetimeDeposits;
        public decimal lifetimeWithdrawals;
        public decimal lifetimeWinnings;
        public decimal lifetimeLosses;
        public List<WalletTransaction> transactions;
        public bool isVerified;             // KYC verificado
        public DateTime lastUpdated;

        public WalletData()
        {
            transactions = new List<WalletTransaction>();
            lastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Balance disponible (total - pendiente)
        /// </summary>
        public decimal AvailableBalance => balance - pendingBalance;

        /// <summary>
        /// Ganancias netas totales
        /// </summary>
        public decimal NetWinnings => lifetimeWinnings - lifetimeLosses;

        /// <summary>
        /// Agrega una transacción y actualiza estadísticas
        /// </summary>
        public void AddTransaction(WalletTransaction transaction)
        {
            transaction.balanceAfter = balance + transaction.amount;
            balance = transaction.balanceAfter;
            transactions.Insert(0, transaction); // Más reciente primero
            lastUpdated = DateTime.UtcNow;

            // Actualizar estadísticas
            switch (transaction.type)
            {
                case TransactionType.Deposit:
                case TransactionType.Bonus:
                    lifetimeDeposits += transaction.amount;
                    break;
                case TransactionType.Withdrawal:
                    lifetimeWithdrawals += Math.Abs(transaction.amount);
                    break;
                case TransactionType.Winnings:
                    lifetimeWinnings += transaction.amount;
                    break;
                case TransactionType.EntryFee:
                    lifetimeLosses += Math.Abs(transaction.amount);
                    break;
            }

            // Limitar historial a últimas 100 transacciones en memoria
            if (transactions.Count > 100)
            {
                transactions.RemoveRange(100, transactions.Count - 100);
            }
        }

        /// <summary>
        /// Obtiene transacciones filtradas por tipo
        /// </summary>
        public List<WalletTransaction> GetTransactionsByType(TransactionType type)
        {
            return transactions.FindAll(t => t.type == type);
        }

        /// <summary>
        /// Obtiene transacciones de un período
        /// </summary>
        public List<WalletTransaction> GetTransactionsSince(DateTime since)
        {
            return transactions.FindAll(t => t.timestamp >= since);
        }
    }

    /// <summary>
    /// Opciones de depósito predefinidas
    /// </summary>
    [Serializable]
    public class DepositOption
    {
        public decimal amount;
        public decimal bonus;           // Bonus promocional
        public bool isPopular;
        public string promoCode;

        public decimal TotalAmount => amount + bonus;

        public string GetDisplayText()
        {
            if (bonus > 0)
                return $"${amount:F2} + ${bonus:F2} BONUS";
            return $"${amount:F2}";
        }
    }
}
