using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace DigitPark.Services.Mock
{
    /// <summary>
    /// Implementación Mock del servicio Matchmaking para desarrollo y testing
    /// Simula búsqueda de oponentes y partidas localmente
    /// </summary>
    public class MockMatchmakingService : IMatchmakingService
    {
        private MatchmakingStatus _currentStatus = MatchmakingStatus.Idle;
        private MatchInfo _currentMatch;
        private List<MatchInfo> _matchHistory = new List<MatchInfo>();
        private System.Threading.CancellationTokenSource _searchCancellation;

        public MatchmakingStatus CurrentStatus => _currentStatus;
        public MatchInfo CurrentMatch => _currentMatch;
        public bool IsSearching => _currentStatus == MatchmakingStatus.Searching;
        public bool HasActiveMatch => _currentMatch != null && _currentMatch.Status == MatchStatus.InProgress;

        public event Action<MatchmakingStatus> OnStatusChanged;
        public event Action<MatchInfo> OnMatchFound;
        public event Action<MatchInfo> OnMatchUpdated;
        public event Action<MatchResult> OnMatchCompleted;

        // Configuración de simulación
        public float MinSearchTimeSeconds { get; set; } = 2f;
        public float MaxSearchTimeSeconds { get; set; } = 8f;
        public float MatchFoundChance { get; set; } = 0.9f; // 90% de encontrar match
        public bool AlwaysFindMatch { get; set; } = true;  // Para desarrollo

        // Entry fees disponibles por juego
        private readonly Dictionary<CashGameType, decimal[]> _entryFees = new Dictionary<CashGameType, decimal[]>
        {
            { CashGameType.DigitRush, new[] { 0.50m, 1.00m, 2.00m, 5.00m, 10.00m } },
            { CashGameType.FlashTap, new[] { 0.50m, 1.00m, 2.00m, 5.00m, 10.00m } },
            { CashGameType.MemoryPairs, new[] { 0.50m, 1.00m, 2.00m, 5.00m } },
            { CashGameType.OddOneOut, new[] { 0.50m, 1.00m, 2.00m, 5.00m } },
            { CashGameType.QuickMath, new[] { 0.50m, 1.00m, 2.00m, 5.00m, 10.00m } }
        };

        // Oponentes simulados
        private readonly string[] _mockNames = { "ProGamer99", "SpeedDemon", "MathWiz", "QuickFingers", "BrainMaster", "ChampionX", "LuckyPlayer", "FastThinker" };
        private readonly string[] _mockAvatars = { "avatar_1", "avatar_2", "avatar_3", "avatar_4", "avatar_5" };

        public MockMatchmakingService()
        {
            Debug.Log("[MockMatchmaking] Servicio inicializado");
        }

        private void UpdateStatus(MatchmakingStatus newStatus)
        {
            if (_currentStatus != newStatus)
            {
                _currentStatus = newStatus;
                OnStatusChanged?.Invoke(newStatus);
                Debug.Log($"[MockMatchmaking] Estado: {newStatus}");
            }
        }

        private OpponentInfo GenerateMockOpponent()
        {
            return new OpponentInfo
            {
                UserId = "mock_opponent_" + Guid.NewGuid().ToString().Substring(0, 8),
                DisplayName = _mockNames[UnityEngine.Random.Range(0, _mockNames.Length)],
                AvatarUrl = _mockAvatars[UnityEngine.Random.Range(0, _mockAvatars.Length)],
                GamesPlayed = UnityEngine.Random.Range(10, 500),
                WinRate = UnityEngine.Random.Range(0.4f, 0.7f),
                SkillRating = UnityEngine.Random.Range(800, 1500)
            };
        }

        public decimal[] GetAvailableEntryFees(CashGameType gameType)
        {
            return _entryFees.ContainsKey(gameType) ? _entryFees[gameType] : new[] { 1.00m };
        }

        public async Task<MatchmakingResult> FindMatch(CashGameType gameType, decimal entryFee)
        {
            Debug.Log($"[MockMatchmaking] Buscando oponente para {gameType} - Entry: ${entryFee:F2}");

            if (IsSearching)
            {
                return MatchmakingResult.Failed("Ya hay una búsqueda en curso", "ALREADY_SEARCHING");
            }

            _searchCancellation = new System.Threading.CancellationTokenSource();
            UpdateStatus(MatchmakingStatus.Searching);

            try
            {
                // Simular tiempo de búsqueda
                float searchTime = UnityEngine.Random.Range(MinSearchTimeSeconds, MaxSearchTimeSeconds);
                Debug.Log($"[MockMatchmaking] Tiempo de búsqueda simulado: {searchTime:F1}s");

                await Task.Delay((int)(searchTime * 1000), _searchCancellation.Token);

                // Verificar si se encontró match
                bool found = AlwaysFindMatch || UnityEngine.Random.value < MatchFoundChance;

                if (!found)
                {
                    UpdateStatus(MatchmakingStatus.Timeout);
                    return MatchmakingResult.Failed("No se encontró oponente", "TIMEOUT");
                }

                // Crear match simulado
                decimal platformFee = entryFee * 0.1m; // 10% fee
                decimal prizePool = (entryFee * 2) - (platformFee * 2);

                _currentMatch = new MatchInfo
                {
                    MatchId = "match_" + Guid.NewGuid().ToString().Substring(0, 8),
                    GameType = gameType,
                    EntryFee = entryFee,
                    PrizePool = prizePool,
                    PlatformFee = platformFee,
                    Status = MatchStatus.WaitingForPlayers,
                    Opponent = GenerateMockOpponent(),
                    CreatedAt = DateTime.UtcNow
                };

                UpdateStatus(MatchmakingStatus.Found);
                OnMatchFound?.Invoke(_currentMatch);

                Debug.Log($"[MockMatchmaking] Oponente encontrado: {_currentMatch.Opponent.DisplayName}");

                return MatchmakingResult.Successful(MatchmakingStatus.Found, _currentMatch);
            }
            catch (TaskCanceledException)
            {
                UpdateStatus(MatchmakingStatus.Cancelled);
                return MatchmakingResult.Failed("Búsqueda cancelada", "CANCELLED");
            }
        }

        public async Task<MatchmakingResult> CancelSearch()
        {
            Debug.Log("[MockMatchmaking] Cancelando búsqueda...");

            if (!IsSearching)
            {
                return MatchmakingResult.Failed("No hay búsqueda activa", "NOT_SEARCHING");
            }

            _searchCancellation?.Cancel();
            UpdateStatus(MatchmakingStatus.Cancelled);

            await Task.Delay(100);
            return MatchmakingResult.Successful(MatchmakingStatus.Cancelled);
        }

        public async Task<MatchmakingResult> ConfirmMatch(string matchId)
        {
            Debug.Log($"[MockMatchmaking] Confirmando partida: {matchId}");

            await Task.Delay(500);

            if (_currentMatch == null || _currentMatch.MatchId != matchId)
            {
                return MatchmakingResult.Failed("Partida no encontrada", "MATCH_NOT_FOUND");
            }

            _currentMatch.Status = MatchStatus.InProgress;
            _currentMatch.StartedAt = DateTime.UtcNow;

            UpdateStatus(MatchmakingStatus.Ready);
            OnMatchUpdated?.Invoke(_currentMatch);

            Debug.Log("[MockMatchmaking] Partida confirmada - Lista para jugar");
            return MatchmakingResult.Successful(MatchmakingStatus.Ready, _currentMatch);
        }

        public async Task<MatchResult> SubmitScore(string matchId, int score)
        {
            Debug.Log($"[MockMatchmaking] Enviando score: {score} para partida {matchId}");

            await Task.Delay(1000);

            if (_currentMatch == null || _currentMatch.MatchId != matchId)
            {
                return new MatchResult { MatchId = matchId, IsWinner = false };
            }

            // Generar score del oponente (simulado)
            int opponentScore = UnityEngine.Random.Range((int)(score * 0.7f), (int)(score * 1.3f));

            _currentMatch.MyScore = score;
            _currentMatch.OpponentScore = opponentScore;
            _currentMatch.Status = MatchStatus.Completed;
            _currentMatch.EndedAt = DateTime.UtcNow;

            bool isWinner = score > opponentScore;
            decimal amountWon = isWinner ? _currentMatch.PrizePool : 0;
            decimal amountLost = isWinner ? 0 : _currentMatch.EntryFee;

            _currentMatch.IsWinner = isWinner;
            _currentMatch.WinAmount = amountWon;

            // Guardar en historial
            _matchHistory.Add(_currentMatch);

            var result = new MatchResult
            {
                MatchId = matchId,
                IsWinner = isWinner,
                MyScore = score,
                OpponentScore = opponentScore,
                AmountWon = amountWon,
                AmountLost = amountLost
            };

            OnMatchCompleted?.Invoke(result);
            UpdateStatus(MatchmakingStatus.Idle);

            Debug.Log($"[MockMatchmaking] Resultado: {(isWinner ? "VICTORIA" : "DERROTA")} - Score: {score} vs {opponentScore}");

            // Limpiar match actual
            _currentMatch = null;

            return result;
        }

        public async Task<MatchInfo> GetMatchStatus(string matchId)
        {
            await Task.Delay(200);

            if (_currentMatch?.MatchId == matchId)
                return _currentMatch;

            return _matchHistory.FirstOrDefault(m => m.MatchId == matchId);
        }

        public async Task<MatchInfo[]> GetMatchHistory(int limit = 20, int offset = 0)
        {
            await Task.Delay(300);

            return _matchHistory
                .OrderByDescending(m => m.CreatedAt)
                .Skip(offset)
                .Take(limit)
                .ToArray();
        }
    }
}
