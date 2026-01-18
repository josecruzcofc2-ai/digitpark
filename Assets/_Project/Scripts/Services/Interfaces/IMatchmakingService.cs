using System;
using System.Threading.Tasks;

namespace DigitPark.Services
{
    /// <summary>
    /// Tipos de juego disponibles para Cash Battle
    /// </summary>
    public enum CashGameType
    {
        DigitRush,
        FlashTap,
        MemoryPairs,
        OddOneOut,
        QuickMath
    }

    /// <summary>
    /// Estado del matchmaking
    /// </summary>
    public enum MatchmakingStatus
    {
        Idle,               // No buscando
        Searching,          // Buscando oponente
        Found,              // Oponente encontrado
        Confirming,         // Esperando confirmación
        Ready,              // Listo para jugar
        Cancelled,          // Búsqueda cancelada
        Timeout,            // Tiempo de búsqueda agotado
        Error               // Error en matchmaking
    }

    /// <summary>
    /// Estado de una partida activa
    /// </summary>
    public enum MatchStatus
    {
        WaitingForPlayers,  // Esperando jugadores
        InProgress,         // Partida en curso
        Completed,          // Partida terminada
        Disputed,           // En disputa (raro)
        Cancelled           // Partida cancelada
    }

    /// <summary>
    /// Información del oponente encontrado
    /// </summary>
    public class OpponentInfo
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public string AvatarUrl { get; set; }
        public int GamesPlayed { get; set; }
        public float WinRate { get; set; }
        public int SkillRating { get; set; }
    }

    /// <summary>
    /// Información de una partida
    /// </summary>
    public class MatchInfo
    {
        public string MatchId { get; set; }
        public CashGameType GameType { get; set; }
        public decimal EntryFee { get; set; }
        public decimal PrizePool { get; set; }
        public decimal PlatformFee { get; set; }
        public MatchStatus Status { get; set; }
        public OpponentInfo Opponent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public int? MyScore { get; set; }
        public int? OpponentScore { get; set; }
        public bool? IsWinner { get; set; }
        public decimal? WinAmount { get; set; }
    }

    /// <summary>
    /// Resultado de operación de matchmaking
    /// </summary>
    public class MatchmakingResult
    {
        public bool Success { get; set; }
        public MatchmakingStatus Status { get; set; }
        public MatchInfo Match { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        public int? EstimatedWaitSeconds { get; set; }

        public static MatchmakingResult Successful(MatchmakingStatus status, MatchInfo match = null)
        {
            return new MatchmakingResult { Success = true, Status = status, Match = match };
        }

        public static MatchmakingResult Failed(string message, string errorCode = null)
        {
            return new MatchmakingResult
            {
                Success = false,
                Status = MatchmakingStatus.Error,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }

    /// <summary>
    /// Resultado de una partida completada
    /// </summary>
    public class MatchResult
    {
        public string MatchId { get; set; }
        public bool IsWinner { get; set; }
        public int MyScore { get; set; }
        public int OpponentScore { get; set; }
        public decimal AmountWon { get; set; }      // Ganancia (0 si perdió)
        public decimal AmountLost { get; set; }     // Pérdida (entry fee si perdió)
        public decimal NewBalance { get; set; }
    }

    /// <summary>
    /// Servicio de Matchmaking para Cash Battle 1v1
    /// Maneja búsqueda de oponentes y estado de partidas
    /// </summary>
    public interface IMatchmakingService
    {
        /// <summary>
        /// Estado actual del matchmaking
        /// </summary>
        MatchmakingStatus CurrentStatus { get; }

        /// <summary>
        /// Partida actual (si existe)
        /// </summary>
        MatchInfo CurrentMatch { get; }

        /// <summary>
        /// Si está actualmente buscando oponente
        /// </summary>
        bool IsSearching { get; }

        /// <summary>
        /// Si hay una partida activa
        /// </summary>
        bool HasActiveMatch { get; }

        /// <summary>
        /// Evento cuando cambia el estado de matchmaking
        /// </summary>
        event Action<MatchmakingStatus> OnStatusChanged;

        /// <summary>
        /// Evento cuando se encuentra un oponente
        /// </summary>
        event Action<MatchInfo> OnMatchFound;

        /// <summary>
        /// Evento cuando se actualiza la partida
        /// </summary>
        event Action<MatchInfo> OnMatchUpdated;

        /// <summary>
        /// Evento cuando termina una partida
        /// </summary>
        event Action<MatchResult> OnMatchCompleted;

        /// <summary>
        /// Inicia búsqueda de oponente para un juego específico
        /// </summary>
        Task<MatchmakingResult> FindMatch(CashGameType gameType, decimal entryFee);

        /// <summary>
        /// Cancela la búsqueda actual
        /// </summary>
        Task<MatchmakingResult> CancelSearch();

        /// <summary>
        /// Confirma participación cuando se encuentra oponente
        /// </summary>
        Task<MatchmakingResult> ConfirmMatch(string matchId);

        /// <summary>
        /// Envía el score de la partida
        /// </summary>
        Task<MatchResult> SubmitScore(string matchId, int score);

        /// <summary>
        /// Obtiene el estado actual de una partida
        /// </summary>
        Task<MatchInfo> GetMatchStatus(string matchId);

        /// <summary>
        /// Obtiene el historial de partidas del usuario
        /// </summary>
        Task<MatchInfo[]> GetMatchHistory(int limit = 20, int offset = 0);

        /// <summary>
        /// Obtiene entry fees disponibles para un juego
        /// </summary>
        decimal[] GetAvailableEntryFees(CashGameType gameType);
    }
}
