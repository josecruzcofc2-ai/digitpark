using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitPark.CashBattle
{
    /// <summary>
    /// Manager central del historial de Cash Battle.
    /// Singleton que persiste entre escenas.
    /// </summary>
    public class HistoryManager : MonoBehaviour
    {
        private static HistoryManager _instance;
        public static HistoryManager Instance => _instance;

        // ==================== ESTADO ====================

        private List<HistoryEntry> _history = new List<HistoryEntry>();
        private HistoryStats _stats = new HistoryStats();

        public List<HistoryEntry> History => _history;
        public HistoryStats Stats => _stats;

        // ==================== EVENTOS ====================

        /// <summary>
        /// Se dispara cuando se agrega una nueva entrada
        /// </summary>
        public event Action<HistoryEntry> OnEntryAdded;

        /// <summary>
        /// Se dispara cuando una entrada se actualiza (ej: resultado de partida)
        /// </summary>
        public event Action<HistoryEntry> OnEntryUpdated;

        /// <summary>
        /// Se dispara cuando las estadísticas cambian
        /// </summary>
        public event Action<HistoryStats> OnStatsUpdated;

        // ==================== CONFIGURACIÓN ====================

        private const int MAX_HISTORY_ENTRIES = 500;
        private const string HISTORY_KEY = "CashBattle_History";
        private const string STATS_KEY = "CashBattle_Stats";

        // ==================== INICIALIZACIÓN ====================

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                LoadHistory();
                Debug.Log($"[HistoryManager] Iniciado - {_history.Count} entradas cargadas");
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void LoadHistory()
        {
            // Cargar historial
            string historyJson = PlayerPrefs.GetString(HISTORY_KEY, "");
            if (!string.IsNullOrEmpty(historyJson))
            {
                try
                {
                    var wrapper = JsonUtility.FromJson<HistoryListWrapper>(historyJson);
                    if (wrapper != null && wrapper.entries != null)
                    {
                        _history = wrapper.entries;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[HistoryManager] Error al cargar historial: {e.Message}");
                    _history = new List<HistoryEntry>();
                }
            }

            // Cargar stats
            string statsJson = PlayerPrefs.GetString(STATS_KEY, "");
            if (!string.IsNullOrEmpty(statsJson))
            {
                try
                {
                    _stats = JsonUtility.FromJson<HistoryStats>(statsJson);
                }
                catch
                {
                    _stats = new HistoryStats();
                    _stats.RecalculateFromHistory(_history);
                }
            }
            else
            {
                _stats = new HistoryStats();
                _stats.RecalculateFromHistory(_history);
            }
        }

        private void SaveHistory()
        {
            try
            {
                var wrapper = new HistoryListWrapper { entries = _history };
                string historyJson = JsonUtility.ToJson(wrapper);
                PlayerPrefs.SetString(HISTORY_KEY, historyJson);

                string statsJson = JsonUtility.ToJson(_stats);
                PlayerPrefs.SetString(STATS_KEY, statsJson);

                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogError($"[HistoryManager] Error al guardar: {e.Message}");
            }
        }

        // ==================== ADD ENTRIES ====================

        /// <summary>
        /// Registra una nueva partida 1v1
        /// </summary>
        public HistoryEntry StartMatch1v1(string gameType, string opponentName, string opponentId, float entryFee)
        {
            var entry = HistoryEntry.Create1v1Match(gameType, opponentName, entryFee);
            entry.opponentId = opponentId;

            AddEntry(entry);

            Debug.Log($"[HistoryManager] Partida 1v1 iniciada: {gameType} vs {opponentName}");
            return entry;
        }

        /// <summary>
        /// Registra un nuevo Cognitive Sprint
        /// </summary>
        public HistoryEntry StartCognitiveSprint(string[] games, string opponentName, string opponentId, float entryFee)
        {
            var entry = HistoryEntry.CreateCognitiveSprint(games, opponentName, entryFee);
            entry.opponentId = opponentId;

            AddEntry(entry);

            Debug.Log($"[HistoryManager] Cognitive Sprint iniciado: {games.Length} juegos vs {opponentName}");
            return entry;
        }

        /// <summary>
        /// Registra participación en torneo
        /// </summary>
        public HistoryEntry JoinTournament(string tournamentName, string tournamentId, float entryFee)
        {
            var entry = HistoryEntry.CreateTournament(tournamentName, tournamentId, entryFee);

            AddEntry(entry);

            Debug.Log($"[HistoryManager] Torneo registrado: {tournamentName}");
            return entry;
        }

        private void AddEntry(HistoryEntry entry)
        {
            _history.Insert(0, entry); // Más reciente primero

            // Limitar tamaño del historial
            if (_history.Count > MAX_HISTORY_ENTRIES)
            {
                _history.RemoveRange(MAX_HISTORY_ENTRIES, _history.Count - MAX_HISTORY_ENTRIES);
            }

            SaveHistory();
            OnEntryAdded?.Invoke(entry);
        }

        // ==================== COMPLETE ENTRIES ====================

        /// <summary>
        /// Completa una partida 1v1 o Cognitive Sprint
        /// </summary>
        public void CompleteMatch(string entryId, MatchResult result, float myScore, float opponentScore, float prize)
        {
            var entry = _history.Find(e => e.entryId == entryId);
            if (entry == null)
            {
                Debug.LogWarning($"[HistoryManager] Entrada no encontrada: {entryId}");
                return;
            }

            entry.CompleteWithResult(result, myScore, opponentScore, prize);

            // Actualizar estadísticas
            _stats.RecalculateFromHistory(_history);

            SaveHistory();
            OnEntryUpdated?.Invoke(entry);
            OnStatsUpdated?.Invoke(_stats);

            Debug.Log($"[HistoryManager] Partida completada: {result} - Neto: {entry.GetFormattedNetResult()}");
        }

        /// <summary>
        /// Completa un torneo
        /// </summary>
        public void CompleteTournament(string entryId, int position, int totalParticipants, float prize)
        {
            var entry = _history.Find(e => e.entryId == entryId);
            if (entry == null)
            {
                Debug.LogWarning($"[HistoryManager] Entrada de torneo no encontrada: {entryId}");
                return;
            }

            entry.CompleteTournament(position, totalParticipants, prize);

            // Actualizar estadísticas
            _stats.RecalculateFromHistory(_history);

            SaveHistory();
            OnEntryUpdated?.Invoke(entry);
            OnStatsUpdated?.Invoke(_stats);

            Debug.Log($"[HistoryManager] Torneo completado: #{position}/{totalParticipants} - Premio: ${prize:F2}");
        }

        /// <summary>
        /// Cancela/reembolsa una entrada
        /// </summary>
        public void CancelEntry(string entryId, string reason = "")
        {
            var entry = _history.Find(e => e.entryId == entryId);
            if (entry == null) return;

            entry.result = MatchResult.Cancelled;
            entry.notes = reason;
            entry.prize = entry.entryFee; // Reembolso completo
            entry.netResult = 0;

            _stats.RecalculateFromHistory(_history);

            SaveHistory();
            OnEntryUpdated?.Invoke(entry);
            OnStatsUpdated?.Invoke(_stats);

            Debug.Log($"[HistoryManager] Entrada cancelada: {entryId}");
        }

        // ==================== QUERIES ====================

        /// <summary>
        /// Obtiene entradas con filtro opcional
        /// </summary>
        public List<HistoryEntry> GetEntries(HistoryFilter filter = null, int limit = 50, int offset = 0)
        {
            IEnumerable<HistoryEntry> result = _history;

            if (filter != null)
            {
                result = result.Where(e => filter.Matches(e));
            }

            return result.Skip(offset).Take(limit).ToList();
        }

        /// <summary>
        /// Obtiene entradas de los últimos N días
        /// </summary>
        public List<HistoryEntry> GetRecentEntries(int days, int limit = 50)
        {
            DateTime since = DateTime.UtcNow.AddDays(-days);
            return _history.Where(e => e.timestamp >= since).Take(limit).ToList();
        }

        /// <summary>
        /// Obtiene solo victorias
        /// </summary>
        public List<HistoryEntry> GetWins(int limit = 20)
        {
            return _history.Where(e => e.result == MatchResult.Win).Take(limit).ToList();
        }

        /// <summary>
        /// Obtiene solo torneos
        /// </summary>
        public List<HistoryEntry> GetTournaments(int limit = 20)
        {
            return _history.Where(e => e.type == HistoryEntryType.Tournament).Take(limit).ToList();
        }

        /// <summary>
        /// Obtiene partidas pendientes (en curso)
        /// </summary>
        public List<HistoryEntry> GetPendingMatches()
        {
            return _history.Where(e => e.result == MatchResult.Pending).ToList();
        }

        /// <summary>
        /// Busca una entrada por ID
        /// </summary>
        public HistoryEntry GetEntry(string entryId)
        {
            return _history.Find(e => e.entryId == entryId);
        }

        /// <summary>
        /// Busca entradas por torneo
        /// </summary>
        public HistoryEntry GetTournamentEntry(string tournamentId)
        {
            return _history.Find(e => e.tournamentId == tournamentId);
        }

        // ==================== STATISTICS ====================

        /// <summary>
        /// Obtiene estadísticas generales
        /// </summary>
        public HistoryStats GetStats()
        {
            return _stats;
        }

        /// <summary>
        /// Recalcula estadísticas
        /// </summary>
        public void RecalculateStats()
        {
            _stats.RecalculateFromHistory(_history);
            SaveHistory();
            OnStatsUpdated?.Invoke(_stats);
        }

        /// <summary>
        /// Obtiene racha actual de victorias/derrotas
        /// </summary>
        public (int streak, bool isWinStreak) GetCurrentStreak()
        {
            if (_history.Count == 0) return (0, true);

            var completedMatches = _history.Where(e => e.result == MatchResult.Win || e.result == MatchResult.Loss).ToList();
            if (completedMatches.Count == 0) return (0, true);

            bool isWin = completedMatches[0].result == MatchResult.Win;
            int streak = 0;

            foreach (var entry in completedMatches)
            {
                bool entryIsWin = entry.result == MatchResult.Win;
                if (entryIsWin == isWin)
                    streak++;
                else
                    break;
            }

            return (streak, isWin);
        }

        /// <summary>
        /// Obtiene la mejor racha de victorias
        /// </summary>
        public int GetBestWinStreak()
        {
            int bestStreak = 0;
            int currentStreak = 0;

            foreach (var entry in _history.OrderBy(e => e.timestamp))
            {
                if (entry.result == MatchResult.Win)
                {
                    currentStreak++;
                    if (currentStreak > bestStreak)
                        bestStreak = currentStreak;
                }
                else if (entry.result == MatchResult.Loss)
                {
                    currentStreak = 0;
                }
            }

            return bestStreak;
        }

        /// <summary>
        /// Obtiene ganancias de los últimos N días
        /// </summary>
        public float GetEarningsLastDays(int days)
        {
            DateTime since = DateTime.UtcNow.AddDays(-days);
            return _history
                .Where(e => e.timestamp >= since && e.result != MatchResult.Pending)
                .Sum(e => e.netResult);
        }

        // ==================== DEBUG ====================

#if UNITY_EDITOR
        [ContextMenu("Debug: Add Sample Win")]
        private void DebugAddSampleWin()
        {
            var entry = StartMatch1v1("Memory", "TestOpponent", "test123", 5f);
            CompleteMatch(entry.entryId, MatchResult.Win, 2500, 1800, 9.5f);
        }

        [ContextMenu("Debug: Add Sample Loss")]
        private void DebugAddSampleLoss()
        {
            var entry = StartMatch1v1("Reaction", "ProPlayer", "pro123", 10f);
            CompleteMatch(entry.entryId, MatchResult.Loss, 1200, 1800, 0f);
        }

        [ContextMenu("Debug: Add Sample Tournament")]
        private void DebugAddSampleTournament()
        {
            var entry = JoinTournament("Weekend Challenge", "tour123", 25f);
            CompleteTournament(entry.entryId, 3, 64, 50f);
        }

        [ContextMenu("Debug: Clear History")]
        private void DebugClearHistory()
        {
            _history.Clear();
            _stats = new HistoryStats();
            SaveHistory();
            Debug.Log("[HistoryManager] Historial limpiado");
        }

        [ContextMenu("Debug: Print Stats")]
        private void DebugPrintStats()
        {
            Debug.Log($"[HistoryManager] Stats:");
            Debug.Log($"  Total: {_stats.totalMatches} | W:{_stats.wins} L:{_stats.losses} D:{_stats.draws}");
            Debug.Log($"  Win Rate: {_stats.winRate:F1}%");
            Debug.Log($"  Net Profit: ${_stats.netProfit:F2}");
            Debug.Log($"  Tournaments: {_stats.tournamentsPlayed} (Wins: {_stats.tournamentWins})");
        }
#endif
    }

    /// <summary>
    /// Wrapper para serializar lista de historial
    /// </summary>
    [Serializable]
    internal class HistoryListWrapper
    {
        public List<HistoryEntry> entries;
    }
}
