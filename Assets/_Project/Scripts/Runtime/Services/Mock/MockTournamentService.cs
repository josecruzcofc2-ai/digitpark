using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using DigitPark.Localization;

namespace DigitPark.Services.Mock
{
    /// <summary>
    /// Implementación Mock del servicio de Torneos para desarrollo y testing
    /// Simula torneos con dinero real localmente
    /// </summary>
    public class MockTournamentService : ITournamentService
    {
        private TournamentInfo _activeTournament;
        private List<TournamentInfo> _availableTournaments = new List<TournamentInfo>();
        private List<TournamentInfo> _tournamentHistory = new List<TournamentInfo>();

        public TournamentInfo ActiveTournament => _activeTournament;
        public bool IsInTournament => _activeTournament != null;

        public event Action<TournamentInfo> OnTournamentUpdated;
        public event Action<TournamentPlayerStatus> OnPlayerStatusChanged;
        public event Action<TournamentInfo, int, decimal> OnTournamentCompleted;

        // Configuración de simulación
        public float SimulatedDelaySeconds { get; set; } = 0.8f;

        public MockTournamentService()
        {
            GenerateMockTournaments();
            Debug.Log("[MockTournament] Servicio inicializado con torneos de prueba");
        }

        private void GenerateMockTournaments()
        {
            var now = DateTime.UtcNow;

            _availableTournaments = new List<TournamentInfo>
            {
                // Torneo próximo
                new TournamentInfo
                {
                    TournamentId = "tournament_001",
                    Name = "DigitRush Championship",
                    Description = "Torneo diario de DigitRush. ¡Demuestra tu velocidad!",
                    GameType = CashGameType.DigitRush,
                    Type = TournamentType.Leaderboard,
                    Status = TournamentStatus.Registration,
                    EntryFee = 5.00m,
                    PrizePool = 450.00m,
                    GuaranteedPrize = 400.00m,
                    MaxPlayers = 100,
                    CurrentPlayers = 67,
                    MinPlayers = 10,
                    RegistrationStart = now.AddHours(-2),
                    RegistrationEnd = now.AddMinutes(30),
                    StartTime = now.AddMinutes(35),
                    Prizes = GeneratePrizes(450.00m, 10),
                    MyStatus = TournamentPlayerStatus.NotJoined
                },
                // Torneo en progreso
                new TournamentInfo
                {
                    TournamentId = "tournament_002",
                    Name = "QuickMath Masters",
                    Description = "Torneo semanal de matemáticas rápidas",
                    GameType = CashGameType.QuickMath,
                    Type = TournamentType.Leaderboard,
                    Status = TournamentStatus.InProgress,
                    EntryFee = 10.00m,
                    PrizePool = 1800.00m,
                    GuaranteedPrize = 1500.00m,
                    MaxPlayers = 200,
                    CurrentPlayers = 189,
                    MinPlayers = 50,
                    RegistrationStart = now.AddHours(-4),
                    RegistrationEnd = now.AddHours(-1),
                    StartTime = now.AddHours(-1),
                    Prizes = GeneratePrizes(1800.00m, 20),
                    MyStatus = TournamentPlayerStatus.NotJoined
                },
                // Torneo pequeño
                new TournamentInfo
                {
                    TournamentId = "tournament_003",
                    Name = "FlashTap Blitz",
                    Description = "Torneo rápido de reflejos. 30 minutos de acción.",
                    GameType = CashGameType.FlashTap,
                    Type = TournamentType.Leaderboard,
                    Status = TournamentStatus.Registration,
                    EntryFee = 2.00m,
                    PrizePool = 90.00m,
                    GuaranteedPrize = 80.00m,
                    MaxPlayers = 50,
                    CurrentPlayers = 23,
                    MinPlayers = 10,
                    RegistrationStart = now.AddMinutes(-30),
                    RegistrationEnd = now.AddMinutes(60),
                    StartTime = now.AddMinutes(65),
                    Prizes = GeneratePrizes(90.00m, 5),
                    MyStatus = TournamentPlayerStatus.NotJoined
                },
                // Torneo premium
                new TournamentInfo
                {
                    TournamentId = "tournament_004",
                    Name = "Memory Masters Elite",
                    Description = "Torneo de alto nivel. Solo para los mejores.",
                    GameType = CashGameType.MemoryPairs,
                    Type = TournamentType.Leaderboard,
                    Status = TournamentStatus.Registration,
                    EntryFee = 25.00m,
                    PrizePool = 2250.00m,
                    GuaranteedPrize = 2000.00m,
                    MaxPlayers = 100,
                    CurrentPlayers = 45,
                    MinPlayers = 20,
                    RegistrationStart = now.AddHours(-1),
                    RegistrationEnd = now.AddHours(2),
                    StartTime = now.AddHours(2.5),
                    Prizes = GeneratePrizes(2250.00m, 15),
                    MyStatus = TournamentPlayerStatus.NotJoined
                }
            };
        }

        private List<TournamentPrize> GeneratePrizes(decimal totalPool, int paidPositions)
        {
            var prizes = new List<TournamentPrize>();

            // Distribución típica: 1st 40%, 2nd 20%, 3rd 12%, etc.
            float[] distribution = { 0.40f, 0.20f, 0.12f, 0.08f, 0.05f, 0.04f, 0.03f, 0.02f, 0.02f, 0.02f };

            for (int i = 0; i < Math.Min(paidPositions, distribution.Length); i++)
            {
                prizes.Add(new TournamentPrize
                {
                    Position = i + 1,
                    PositionLabel = GetPositionLabel(i + 1),
                    Amount = totalPool * (decimal)distribution[i],
                    PercentageOfPool = distribution[i] * 100
                });
            }

            return prizes;
        }

        private string GetPositionLabel(int position)
        {
            return position switch
            {
                1 => "1st",
                2 => "2nd",
                3 => "3rd",
                _ => $"{position}th"
            };
        }

        public async Task<List<TournamentInfo>> GetAvailableTournaments(CashGameType? gameType = null)
        {
            await Task.Delay((int)(SimulatedDelaySeconds * 500));

            var tournaments = _availableTournaments
                .Where(t => t.Status == TournamentStatus.Registration || t.Status == TournamentStatus.Upcoming);

            if (gameType.HasValue)
                tournaments = tournaments.Where(t => t.GameType == gameType.Value);

            return tournaments.OrderBy(t => t.StartTime).ToList();
        }

        public async Task<List<TournamentInfo>> GetUpcomingTournaments(int limit = 10)
        {
            await Task.Delay((int)(SimulatedDelaySeconds * 300));

            return _availableTournaments
                .Where(t => t.Status == TournamentStatus.Registration || t.Status == TournamentStatus.Upcoming)
                .OrderBy(t => t.StartTime)
                .Take(limit)
                .ToList();
        }

        public async Task<TournamentInfo> GetTournamentDetails(string tournamentId)
        {
            await Task.Delay((int)(SimulatedDelaySeconds * 200));

            return _availableTournaments.FirstOrDefault(t => t.TournamentId == tournamentId)
                ?? _tournamentHistory.FirstOrDefault(t => t.TournamentId == tournamentId);
        }

        public async Task<TournamentResult> JoinTournament(string tournamentId)
        {
            Debug.Log($"[MockTournament] Inscribiendo en torneo: {tournamentId}");

            await Task.Delay((int)(SimulatedDelaySeconds * 1000));

            var tournament = _availableTournaments.FirstOrDefault(t => t.TournamentId == tournamentId);

            if (tournament == null)
            {
                return TournamentResult.Failed("Torneo no encontrado", "NOT_FOUND");
            }

            if (tournament.Status != TournamentStatus.Registration)
            {
                return TournamentResult.Failed("El registro está cerrado", "REGISTRATION_CLOSED");
            }

            if (tournament.CurrentPlayers >= tournament.MaxPlayers)
            {
                return TournamentResult.Failed("Torneo lleno", "TOURNAMENT_FULL");
            }

            if (_activeTournament != null)
            {
                return TournamentResult.Failed("Ya estás en un torneo", "ALREADY_IN_TOURNAMENT");
            }

            // Inscribir
            tournament.CurrentPlayers++;
            tournament.MyStatus = TournamentPlayerStatus.Registered;
            _activeTournament = tournament;

            OnPlayerStatusChanged?.Invoke(TournamentPlayerStatus.Registered);
            OnTournamentUpdated?.Invoke(tournament);

            Debug.Log($"[MockTournament] Inscrito exitosamente en: {tournament.Name}");
            return TournamentResult.Successful(tournament, "Inscripción exitosa");
        }

        public async Task<TournamentResult> LeaveTournament(string tournamentId)
        {
            Debug.Log($"[MockTournament] Saliendo de torneo: {tournamentId}");

            await Task.Delay((int)(SimulatedDelaySeconds * 500));

            if (_activeTournament == null || _activeTournament.TournamentId != tournamentId)
            {
                return TournamentResult.Failed(AutoLocalizer.Get("tournament_not_member"), "NOT_IN_TOURNAMENT");
            }

            if (_activeTournament.Status == TournamentStatus.InProgress)
            {
                return TournamentResult.Failed("No puedes salir de un torneo en progreso", "TOURNAMENT_IN_PROGRESS");
            }

            _activeTournament.CurrentPlayers--;
            _activeTournament.MyStatus = TournamentPlayerStatus.NotJoined;

            var tournament = _activeTournament;
            _activeTournament = null;

            OnPlayerStatusChanged?.Invoke(TournamentPlayerStatus.NotJoined);

            Debug.Log($"[MockTournament] Salida exitosa de: {tournament.Name}");
            return TournamentResult.Successful(tournament, "Has abandonado el torneo. Entry fee reembolsado.");
        }

        public async Task<List<TournamentLeaderboardEntry>> GetLeaderboard(string tournamentId, int limit = 100)
        {
            await Task.Delay((int)(SimulatedDelaySeconds * 400));

            // Generar leaderboard simulado
            var leaderboard = new List<TournamentLeaderboardEntry>();
            string[] names = { "ProGamer", "SpeedKing", "MathGenius", "QuickDraw", "BrainStorm", "Champion", "Winner", "Master" };

            for (int i = 0; i < Math.Min(limit, 50); i++)
            {
                leaderboard.Add(new TournamentLeaderboardEntry
                {
                    Position = i + 1,
                    UserId = $"user_{i}",
                    DisplayName = $"{names[i % names.Length]}{UnityEngine.Random.Range(1, 999)}",
                    Score = 10000 - (i * UnityEngine.Random.Range(50, 150)),
                    GamesPlayed = UnityEngine.Random.Range(3, 10),
                    IsMe = i == 12 // Simular que estamos en posición 13
                });
            }

            return leaderboard;
        }

        public async Task<TournamentResult> SubmitTournamentScore(string tournamentId, int score)
        {
            Debug.Log($"[MockTournament] Enviando score: {score} al torneo {tournamentId}");

            await Task.Delay((int)(SimulatedDelaySeconds * 500));

            if (_activeTournament == null || _activeTournament.TournamentId != tournamentId)
            {
                return TournamentResult.Failed(AutoLocalizer.Get("tournament_not_member"), "NOT_IN_TOURNAMENT");
            }

            _activeTournament.MyScore = (_activeTournament.MyScore ?? 0) + score;
            OnTournamentUpdated?.Invoke(_activeTournament);

            Debug.Log($"[MockTournament] Score actualizado. Total: {_activeTournament.MyScore}");
            return TournamentResult.Successful(_activeTournament, $"Score registrado: {score}");
        }

        public async Task<List<TournamentInfo>> GetTournamentHistory(int limit = 20, int offset = 0)
        {
            await Task.Delay((int)(SimulatedDelaySeconds * 300));

            return _tournamentHistory
                .OrderByDescending(t => t.EndTime)
                .Skip(offset)
                .Take(limit)
                .ToList();
        }

        // Métodos de testing
        public void SimulateTournamentEnd(int finalPosition, decimal prize)
        {
            if (_activeTournament == null) return;

            _activeTournament.Status = TournamentStatus.Completed;
            _activeTournament.EndTime = DateTime.UtcNow;
            _activeTournament.MyPosition = finalPosition;
            _activeTournament.MyStatus = finalPosition <= 3 ? TournamentPlayerStatus.Winner : TournamentPlayerStatus.Eliminated;

            _tournamentHistory.Add(_activeTournament);

            OnTournamentCompleted?.Invoke(_activeTournament, finalPosition, prize);
            OnPlayerStatusChanged?.Invoke(_activeTournament.MyStatus);

            Debug.Log($"[MockTournament] Torneo terminado. Posición: {finalPosition}, Premio: ${prize:F2}");

            _activeTournament = null;
        }
    }
}
