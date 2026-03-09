using UnityEngine;
using System;
using System.Collections.Generic;
using DigitPark.Localization;

namespace DigitPark.CashBattle
{
    /// <summary>
    /// Tipo de entrada en el historial
    /// </summary>
    public enum HistoryEntryType
    {
        Match1v1,           // Partida 1v1
        CognitiveSprint,    // Cognitive Sprint (múltiples juegos)
        Tournament,         // Torneo
        TournamentPrize     // Premio de torneo
    }

    /// <summary>
    /// Resultado de una partida/torneo
    /// </summary>
    public enum MatchResult
    {
        Win,
        Loss,
        Draw,
        Pending,        // En progreso
        Cancelled,      // Cancelado/Reembolsado
        Disqualified    // Descalificado
    }

    /// <summary>
    /// Representa una entrada en el historial de Cash Battle
    /// </summary>
    [Serializable]
    public class HistoryEntry
    {
        public string entryId;
        public HistoryEntryType type;
        public MatchResult result;
        public DateTime timestamp;

        [Header("Match Info")]
        public string gameType;             // Tipo de juego (Memory, Reaction, etc.)
        public string[] gamesPlayed;        // Para Cognitive Sprint
        public string opponentName;
        public string opponentId;
        public string tournamentName;
        public string tournamentId;

        [Header("Scores")]
        public float myScore;
        public float opponentScore;
        public int myPosition;              // Posición en torneo
        public int totalParticipants;

        [Header("Money")]
        public decimal entryFee;
        public decimal prize;
        public decimal netResult;           // prize - entryFee

        [Header("Details")]
        public float matchDuration;         // Duración en segundos
        public string notes;

        public HistoryEntry()
        {
            entryId = Guid.NewGuid().ToString();
            timestamp = DateTime.UtcNow;
            result = MatchResult.Pending;
        }

        /// <summary>
        /// Crea una entrada para partida 1v1
        /// </summary>
        public static HistoryEntry Create1v1Match(string gameType, string opponentName, decimal entryFee)
        {
            return new HistoryEntry
            {
                type = HistoryEntryType.Match1v1,
                gameType = gameType,
                opponentName = opponentName,
                entryFee = entryFee,
                result = MatchResult.Pending
            };
        }

        /// <summary>
        /// Crea una entrada para Cognitive Sprint
        /// </summary>
        public static HistoryEntry CreateCognitiveSprint(string[] games, string opponentName, decimal entryFee)
        {
            return new HistoryEntry
            {
                type = HistoryEntryType.CognitiveSprint,
                gameType = "Cognitive Sprint",
                gamesPlayed = games,
                opponentName = opponentName,
                entryFee = entryFee,
                result = MatchResult.Pending
            };
        }

        /// <summary>
        /// Crea una entrada para torneo
        /// </summary>
        public static HistoryEntry CreateTournament(string tournamentName, string tournamentId, decimal entryFee)
        {
            return new HistoryEntry
            {
                type = HistoryEntryType.Tournament,
                tournamentName = tournamentName,
                tournamentId = tournamentId,
                entryFee = entryFee,
                result = MatchResult.Pending
            };
        }

        /// <summary>
        /// Completa la entrada con el resultado
        /// </summary>
        public void CompleteWithResult(MatchResult matchResult, float myScoreValue, float opponentScoreValue, decimal prizeAmount)
        {
            result = matchResult;
            myScore = myScoreValue;
            opponentScore = opponentScoreValue;
            prize = prizeAmount;
            netResult = prize - entryFee;
        }

        /// <summary>
        /// Completa entrada de torneo
        /// </summary>
        public void CompleteTournament(int position, int participants, decimal prizeAmount)
        {
            myPosition = position;
            totalParticipants = participants;
            prize = prizeAmount;
            netResult = prize - entryFee;

            // Determinar resultado basado en posición
            if (prizeAmount > 0)
                result = MatchResult.Win;
            else if (position <= participants / 2)
                result = MatchResult.Draw; // Top 50% pero sin premio
            else
                result = MatchResult.Loss;
        }

        /// <summary>
        /// Obtiene el color del resultado
        /// </summary>
        public Color GetResultColor()
        {
            switch (result)
            {
                case MatchResult.Win:
                    return new Color(0f, 1f, 0.5f, 1f); // Verde
                case MatchResult.Loss:
                    return new Color(1f, 0.4f, 0.4f, 1f); // Rojo
                case MatchResult.Draw:
                    return new Color(1f, 0.84f, 0f, 1f); // Dorado
                case MatchResult.Pending:
                    return new Color(0f, 0.83f, 1f, 1f); // Cyan
                case MatchResult.Cancelled:
                    return new Color(0.6f, 0.6f, 0.6f, 1f); // Gris
                case MatchResult.Disqualified:
                    return new Color(0.8f, 0.2f, 0.8f, 1f); // Morado
                default:
                    return Color.white;
            }
        }

        /// <summary>
        /// Obtiene texto del resultado
        /// </summary>
        public string GetResultText()
        {
            switch (result)
            {
                case MatchResult.Win: return AutoLocalizer.Get("history_result_victory");
                case MatchResult.Loss: return AutoLocalizer.Get("history_result_defeat");
                case MatchResult.Draw: return AutoLocalizer.Get("history_result_draw");
                case MatchResult.Pending: return AutoLocalizer.Get("history_in_progress").ToUpper();
                case MatchResult.Cancelled: return AutoLocalizer.Get("history_result_cancelled");
                case MatchResult.Disqualified: return AutoLocalizer.Get("history_result_disqualified");
                default: return "";
            }
        }

        /// <summary>
        /// Obtiene el título para mostrar
        /// </summary>
        public string GetDisplayTitle()
        {
            switch (type)
            {
                case HistoryEntryType.Match1v1:
                    return $"1v1 - {gameType}";
                case HistoryEntryType.CognitiveSprint:
                    return AutoLocalizer.Get("history_cognitive_sprint_games", gamesPlayed?.Length ?? 0);
                case HistoryEntryType.Tournament:
                    return tournamentName ?? AutoLocalizer.Get("history_tournament_default");
                case HistoryEntryType.TournamentPrize:
                    return AutoLocalizer.Get("history_tournament_prize", tournamentName);
                default:
                    return AutoLocalizer.Get("history_match_default");
            }
        }

        /// <summary>
        /// Obtiene subtítulo (oponente o posición)
        /// </summary>
        public string GetDisplaySubtitle()
        {
            switch (type)
            {
                case HistoryEntryType.Match1v1:
                case HistoryEntryType.CognitiveSprint:
                    return $"vs {opponentName ?? AutoLocalizer.Get("history_opponent_default")}";
                case HistoryEntryType.Tournament:
                    if (result == MatchResult.Pending)
                        return AutoLocalizer.Get("history_in_progress_ellipsis");
                    return AutoLocalizer.Get("history_position", myPosition, totalParticipants);
                default:
                    return "";
            }
        }

        /// <summary>
        /// Obtiene el monto neto formateado
        /// </summary>
        public string GetFormattedNetResult()
        {
            if (netResult > 0)
                return $"+${netResult:F2}";
            else if (netResult < 0)
                return $"-${Math.Abs(netResult):F2}";
            else
                return "$0.00";
        }

        /// <summary>
        /// Formatea la fecha para display
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

            return timestamp.ToString("dd/MM/yyyy HH:mm");
        }

        /// <summary>
        /// Formatea la duración de la partida
        /// </summary>
        public string GetFormattedDuration()
        {
            if (matchDuration <= 0) return "";

            TimeSpan duration = TimeSpan.FromSeconds(matchDuration);
            if (duration.TotalMinutes < 1)
                return $"{(int)duration.TotalSeconds}s";
            return $"{(int)duration.TotalMinutes}m {duration.Seconds}s";
        }
    }

    /// <summary>
    /// Estadísticas del historial
    /// </summary>
    [Serializable]
    public class HistoryStats
    {
        public int totalMatches;
        public int wins;
        public int losses;
        public int draws;

        public decimal totalEarnings;
        public decimal totalSpent;
        public decimal netProfit;

        public int tournamentsPlayed;
        public int tournamentWins;
        public int bestTournamentPosition;

        public float winRate => totalMatches > 0 ? (float)wins / totalMatches * 100f : 0f;
        public float avgEarningsPerMatch => totalMatches > 0 ? (float)(totalEarnings / totalMatches) : 0f;

        public void RecalculateFromHistory(List<HistoryEntry> entries)
        {
            totalMatches = 0;
            wins = 0;
            losses = 0;
            draws = 0;
            totalEarnings = 0;
            totalSpent = 0;
            tournamentsPlayed = 0;
            tournamentWins = 0;
            bestTournamentPosition = int.MaxValue;

            foreach (var entry in entries)
            {
                if (entry.result == MatchResult.Pending) continue;

                totalMatches++;
                totalSpent += entry.entryFee;

                switch (entry.result)
                {
                    case MatchResult.Win:
                        wins++;
                        totalEarnings += entry.prize;
                        break;
                    case MatchResult.Loss:
                        losses++;
                        break;
                    case MatchResult.Draw:
                        draws++;
                        totalEarnings += entry.prize;
                        break;
                }

                if (entry.type == HistoryEntryType.Tournament)
                {
                    tournamentsPlayed++;
                    if (entry.myPosition == 1)
                        tournamentWins++;
                    if (entry.myPosition > 0 && entry.myPosition < bestTournamentPosition)
                        bestTournamentPosition = entry.myPosition;
                }
            }

            netProfit = totalEarnings - totalSpent;

            if (bestTournamentPosition == int.MaxValue)
                bestTournamentPosition = 0;
        }
    }

    /// <summary>
    /// Filtros para el historial
    /// </summary>
    [Serializable]
    public class HistoryFilter
    {
        public HistoryEntryType? typeFilter;
        public MatchResult? resultFilter;
        public DateTime? fromDate;
        public DateTime? toDate;
        public string gameTypeFilter;

        public bool Matches(HistoryEntry entry)
        {
            if (typeFilter.HasValue && entry.type != typeFilter.Value)
                return false;

            if (resultFilter.HasValue && entry.result != resultFilter.Value)
                return false;

            if (fromDate.HasValue && entry.timestamp < fromDate.Value)
                return false;

            if (toDate.HasValue && entry.timestamp > toDate.Value)
                return false;

            if (!string.IsNullOrEmpty(gameTypeFilter) && entry.gameType != gameTypeFilter)
                return false;

            return true;
        }
    }
}
