using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DigitPark.Services
{
    /// <summary>
    /// Tipo de torneo
    /// </summary>
    public enum TournamentType
    {
        SingleElimination,  // Eliminación directa
        DoubleElimination,  // Doble eliminación
        RoundRobin,         // Todos contra todos
        Swiss,              // Sistema suizo
        Leaderboard         // Por puntuación (más común en mobile)
    }

    /// <summary>
    /// Estado del torneo
    /// </summary>
    public enum TournamentStatus
    {
        Upcoming,           // Próximo a iniciar
        Scheduled,          // Programado (alias de Upcoming)
        Registration,       // Registro abierto
        Active,             // Activo (alias de InProgress)
        InProgress,         // En curso
        Completed,          // Finalizado
        Cancelled           // Cancelado
    }

    /// <summary>
    /// Estado del jugador en un torneo
    /// </summary>
    public enum TournamentPlayerStatus
    {
        NotJoined,          // No inscrito
        Registered,         // Inscrito
        Playing,            // Jugando
        Eliminated,         // Eliminado
        Winner,             // Ganador
        Refunded            // Reembolsado (torneo cancelado)
    }

    /// <summary>
    /// Información de premio por posición
    /// </summary>
    public class TournamentPrize
    {
        public int Position { get; set; }           // 1, 2, 3, etc.
        public string PositionLabel { get; set; }   // "1st", "2nd-3rd", etc.
        public decimal Amount { get; set; }
        public float PercentageOfPool { get; set; }
    }

    /// <summary>
    /// Información de un torneo
    /// </summary>
    public class TournamentInfo
    {
        public string TournamentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public CashGameType GameType { get; set; }
        public TournamentType Type { get; set; }
        public TournamentStatus Status { get; set; }
        public decimal EntryFee { get; set; }
        public decimal PrizePool { get; set; }
        public decimal GuaranteedPrize { get; set; }    // Premio mínimo garantizado
        public int MaxPlayers { get; set; }
        public int CurrentPlayers { get; set; }
        public int MinPlayers { get; set; }             // Mínimo para iniciar
        public DateTime RegistrationStart { get; set; }
        public DateTime RegistrationEnd { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<TournamentPrize> Prizes { get; set; }
        public TournamentPlayerStatus MyStatus { get; set; }
        public int? MyPosition { get; set; }
        public int? MyScore { get; set; }
    }

    /// <summary>
    /// Entrada en el leaderboard del torneo
    /// </summary>
    public class TournamentLeaderboardEntry
    {
        public int Position { get; set; }
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public string AvatarUrl { get; set; }
        public int Score { get; set; }
        public int GamesPlayed { get; set; }
        public bool IsMe { get; set; }
    }

    /// <summary>
    /// Resultado de operación de torneo
    /// </summary>
    public class TournamentResult
    {
        public bool Success { get; set; }
        public TournamentInfo Tournament { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }

        public static TournamentResult Successful(TournamentInfo tournament, string message = null)
        {
            return new TournamentResult { Success = true, Tournament = tournament, Message = message };
        }

        public static TournamentResult Failed(string message, string errorCode = null)
        {
            return new TournamentResult { Success = false, Message = message, ErrorCode = errorCode };
        }
    }

    /// <summary>
    /// Servicio de Torneos para Cash Battle
    /// Maneja inscripción, participación y premios de torneos
    /// </summary>
    public interface ITournamentService
    {
        /// <summary>
        /// Torneo activo actual del usuario (si está en uno)
        /// </summary>
        TournamentInfo ActiveTournament { get; }

        /// <summary>
        /// Si el usuario está actualmente en un torneo
        /// </summary>
        bool IsInTournament { get; }

        /// <summary>
        /// Evento cuando se actualiza un torneo
        /// </summary>
        event Action<TournamentInfo> OnTournamentUpdated;

        /// <summary>
        /// Evento cuando el jugador avanza/es eliminado
        /// </summary>
        event Action<TournamentPlayerStatus> OnPlayerStatusChanged;

        /// <summary>
        /// Evento cuando termina un torneo
        /// </summary>
        event Action<TournamentInfo, int, decimal> OnTournamentCompleted; // tournament, position, prize

        /// <summary>
        /// Obtiene lista de torneos disponibles
        /// </summary>
        Task<List<TournamentInfo>> GetAvailableTournaments(CashGameType? gameType = null);

        /// <summary>
        /// Obtiene torneos próximos
        /// </summary>
        Task<List<TournamentInfo>> GetUpcomingTournaments(int limit = 10);

        /// <summary>
        /// Obtiene detalles de un torneo específico
        /// </summary>
        Task<TournamentInfo> GetTournamentDetails(string tournamentId);

        /// <summary>
        /// Inscribe al usuario en un torneo
        /// </summary>
        Task<TournamentResult> JoinTournament(string tournamentId);

        /// <summary>
        /// Retira al usuario de un torneo (antes de que inicie)
        /// </summary>
        Task<TournamentResult> LeaveTournament(string tournamentId);

        /// <summary>
        /// Obtiene el leaderboard de un torneo
        /// </summary>
        Task<List<TournamentLeaderboardEntry>> GetLeaderboard(string tournamentId, int limit = 100);

        /// <summary>
        /// Envía score de una partida del torneo
        /// </summary>
        Task<TournamentResult> SubmitTournamentScore(string tournamentId, int score);

        /// <summary>
        /// Obtiene historial de torneos del usuario
        /// </summary>
        Task<List<TournamentInfo>> GetTournamentHistory(int limit = 20, int offset = 0);
    }
}
