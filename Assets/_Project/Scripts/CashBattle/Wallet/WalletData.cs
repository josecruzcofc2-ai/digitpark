using UnityEngine;
using System;
using System.Collections.Generic;

namespace DigitPark.CashBattle
{
    /// <summary>
    /// Wallet transaction types
    /// </summary>
    public enum TransactionType
    {
        Deposit,            // Money deposit
        Withdrawal,         // Money withdrawal
        EntryFee,           // Entry fee payment (legacy)
        MatchEntry,         // 1v1 match entry
        Winnings,           // Winnings (legacy)
        MatchWinnings,      // 1v1 match winnings
        TournamentEntry,    // Tournament entry
        TournamentPrize,    // Tournament prize
        Bonus,              // Promotional bonus
        Refund              // Refund
    }

    /// <summary>
    /// Transaction status
    /// </summary>
    public enum TransactionStatus
    {
        Pending,        // In progress
        Completed,      // Completed
        Failed,         // Failed
        Cancelled       // Cancelled
    }

    /// <summary>
    /// Supported payment methods
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
    /// Represents an individual wallet transaction
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
        public string referenceId;      // Match/tournament ID if applicable
        public PaymentMethod? paymentMethod;

        public WalletTransaction()
        {
            transactionId = Guid.NewGuid().ToString();
            timestamp = DateTime.UtcNow;
            status = TransactionStatus.Pending;
        }

        /// <summary>
        /// Creates a deposit transaction
        /// </summary>
        public static WalletTransaction CreateDeposit(decimal amount, PaymentMethod method)
        {
            return new WalletTransaction
            {
                type = TransactionType.Deposit,
                amount = amount,
                paymentMethod = method,
                description = $"Deposit via {method}"
            };
        }

        /// <summary>
        /// Creates a withdrawal transaction
        /// </summary>
        public static WalletTransaction CreateWithdrawal(decimal amount, PaymentMethod method)
        {
            return new WalletTransaction
            {
                type = TransactionType.Withdrawal,
                amount = -amount, // Negative for withdrawals
                paymentMethod = method,
                description = $"Withdrawal via {method}"
            };
        }

        /// <summary>
        /// Creates an entry fee transaction
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
        /// Creates a winnings transaction
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
        /// Gets the color associated with the transaction type
        /// </summary>
        public Color GetTypeColor()
        {
            switch (type)
            {
                case TransactionType.Deposit:
                case TransactionType.Winnings:
                case TransactionType.Bonus:
                    return new Color(0f, 1f, 0.5f, 1f); // Green
                case TransactionType.Withdrawal:
                case TransactionType.EntryFee:
                    return new Color(1f, 0.4f, 0.4f, 1f); // Red
                case TransactionType.Refund:
                    return new Color(1f, 0.84f, 0f, 1f); // Gold
                default:
                    return Color.white;
            }
        }

        /// <summary>
        /// Gets the icon/symbol for the type
        /// </summary>
        public string GetTypeSymbol()
        {
            return amount >= 0 ? "+" : "";
        }

        /// <summary>
        /// Formats the amount for display
        /// </summary>
        public string GetFormattedAmount()
        {
            return $"{GetTypeSymbol()}${Math.Abs(amount):F2}";
        }

        /// <summary>
        /// Formats the date for display
        /// </summary>
        public string GetFormattedDate()
        {
            TimeSpan timeSince = DateTime.UtcNow - timestamp;

            if (timeSince.TotalMinutes < 1)
                return "Now";
            if (timeSince.TotalMinutes < 60)
                return $"{(int)timeSince.TotalMinutes}m ago";
            if (timeSince.TotalHours < 24)
                return $"{(int)timeSince.TotalHours}h ago";
            if (timeSince.TotalDays < 7)
                return $"{(int)timeSince.TotalDays}d ago";

            return timestamp.ToString("dd/MM/yyyy");
        }
    }

    /// <summary>
    /// User wallet data
    /// </summary>
    [Serializable]
    public class WalletData
    {
        public string userId;
        public decimal balance;
        public decimal pendingBalance;      // Pending balance (pending withdrawals)
        public decimal lifetimeDeposits;
        public decimal lifetimeWithdrawals;
        public decimal lifetimeWinnings;
        public decimal lifetimeLosses;
        public List<WalletTransaction> transactions;
        public bool isVerified;             // KYC verified
        public DateTime lastUpdated;

        public WalletData()
        {
            transactions = new List<WalletTransaction>();
            lastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Available balance (total - pending)
        /// </summary>
        public decimal AvailableBalance => balance - pendingBalance;

        /// <summary>
        /// Total net winnings
        /// </summary>
        public decimal NetWinnings => lifetimeWinnings - lifetimeLosses;

        /// <summary>
        /// Adds a transaction and updates statistics
        /// </summary>
        public void AddTransaction(WalletTransaction transaction)
        {
            transaction.balanceAfter = balance + transaction.amount;
            balance = transaction.balanceAfter;
            transactions.Insert(0, transaction); // Most recent first
            lastUpdated = DateTime.UtcNow;

            // Update statistics
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

            // Limit history to last 100 transactions in memory
            if (transactions.Count > 100)
            {
                transactions.RemoveRange(100, transactions.Count - 100);
            }
        }

        /// <summary>
        /// Gets transactions filtered by type
        /// </summary>
        public List<WalletTransaction> GetTransactionsByType(TransactionType type)
        {
            return transactions.FindAll(t => t.type == type);
        }

        /// <summary>
        /// Gets transactions from a time period
        /// </summary>
        public List<WalletTransaction> GetTransactionsSince(DateTime since)
        {
            return transactions.FindAll(t => t.timestamp >= since);
        }
    }

    /// <summary>
    /// Predefined deposit options
    /// </summary>
    [Serializable]
    public class DepositOption
    {
        public decimal amount;
        public string promoCode;

        public decimal TotalAmount => amount;

        public string GetDisplayText()
        {
            return $"${amount:F2}";
        }
    }
}
