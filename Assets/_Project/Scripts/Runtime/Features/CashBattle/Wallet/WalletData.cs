using UnityEngine;
using System;
using System.Collections.Generic;
using DigitPark.Localization;

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
        public float amount;
        public float balanceAfter;
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
        public static WalletTransaction CreateDeposit(float amount, PaymentMethod method)
        {
            return new WalletTransaction
            {
                type = TransactionType.Deposit,
                amount = amount,
                paymentMethod = method,
                description = AutoLocalizer.Get("wallet_deposit_via", method)
            };
        }

        /// <summary>
        /// Creates a withdrawal transaction
        /// </summary>
        public static WalletTransaction CreateWithdrawal(float amount, PaymentMethod method)
        {
            return new WalletTransaction
            {
                type = TransactionType.Withdrawal,
                amount = -amount, // Negative for withdrawals
                paymentMethod = method,
                description = AutoLocalizer.Get("wallet_withdrawal_via", method)
            };
        }

        /// <summary>
        /// Creates an entry fee transaction
        /// </summary>
        public static WalletTransaction CreateEntryFee(float amount, string matchId, string matchDescription)
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
        public static WalletTransaction CreateWinnings(float amount, string matchId, string matchDescription)
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
            return amount >= 0 ? "+" : "-";
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
                return AutoLocalizer.Get("time_now");
            if (timeSince.TotalMinutes < 60)
                return AutoLocalizer.Get("time_ago_minutes", (int)timeSince.TotalMinutes);
            if (timeSince.TotalHours < 24)
                return AutoLocalizer.Get("time_ago_hours", (int)timeSince.TotalHours);
            if (timeSince.TotalDays < 7)
                return AutoLocalizer.Get("time_ago_days", (int)timeSince.TotalDays);

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
        public float balance;
        public float pendingBalance;      // Pending balance (pending withdrawals)
        public float lifetimeDeposits;
        public float lifetimeWithdrawals;
        public float lifetimeWinnings;
        public float lifetimeLosses;
        public List<WalletTransaction> transactions;
        public DateTime lastUpdated;

        public WalletData()
        {
            transactions = new List<WalletTransaction>();
            lastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Available balance (total - pending)
        /// </summary>
        public float AvailableBalance => balance - pendingBalance;

        /// <summary>
        /// Total net winnings
        /// </summary>
        public float NetWinnings => lifetimeWinnings - lifetimeLosses;

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
        public float amount;
        public string promoCode;

        public float TotalAmount => amount;

        public string GetDisplayText()
        {
            return $"${amount:F2}";
        }
    }
}
