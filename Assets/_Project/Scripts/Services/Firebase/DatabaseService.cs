using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using DigitPark.Data;

namespace DigitPark.Services.Firebase
{
    /// <summary>
    /// Servicio de base de datos (Modo Simulación)
    /// Usa PlayerPrefs para almacenar datos localmente
    /// </summary>
    public class DatabaseService : MonoBehaviour
    {
        public static DatabaseService Instance { get; private set; }

        // Datos en memoria
        private List<LeaderboardEntry> globalLeaderboard = new List<LeaderboardEntry>();
        private Dictionary<string, TournamentData> tournaments = new Dictionary<string, TournamentData>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            Debug.Log("[Database] Inicializando servicio de datos (Simulación)...");
            LoadData();
            Debug.Log("[Database] Servicio listo");
        }

        private void LoadData()
        {
            // Cargar leaderboard
            if (PlayerPrefs.HasKey("SimLeaderboard"))
            {
                string json = PlayerPrefs.GetString("SimLeaderboard");
                var wrapper = JsonUtility.FromJson<LeaderboardWrapper>(json);
                if (wrapper?.entries != null)
                {
                    globalLeaderboard = wrapper.entries;
                }
            }

            if (globalLeaderboard.Count == 0)
            {
                CreateSampleData();
            }

            Debug.Log($"[Database] Leaderboard cargado: {globalLeaderboard.Count} entradas");
        }

        private void CreateSampleData()
        {
            string[] names = { "ProGamer99", "SpeedRunner", "ChampionX", "FastFingers", "GoldMaster" };
            string[] countries = { "US", "MX", "ES", "AR", "CO" };

            for (int i = 0; i < 5; i++)
            {
                globalLeaderboard.Add(new LeaderboardEntry
                {
                    userId = $"sample_{i}",
                    username = names[i],
                    time = 10f + (i * 2.5f) + UnityEngine.Random.Range(0f, 1f),
                    countryCode = countries[i],
                    position = i + 1
                });
            }

            SaveLeaderboard();
        }

        private void SaveLeaderboard()
        {
            var wrapper = new LeaderboardWrapper { entries = globalLeaderboard };
            PlayerPrefs.SetString("SimLeaderboard", JsonUtility.ToJson(wrapper));
            PlayerPrefs.Save();
        }

        #region Player Data

        public async Task SavePlayerData(PlayerData playerData)
        {
            Debug.Log($"[Database] Guardando jugador: {playerData.userId}");

            string key = $"SimUser_{playerData.userId}";
            PlayerPrefs.SetString(key, JsonUtility.ToJson(playerData));
            PlayerPrefs.Save();

            await Task.Delay(50);
        }

        public async Task<PlayerData> LoadPlayerData(string userId)
        {
            Debug.Log($"[Database] Cargando jugador: {userId}");

            string key = $"SimUser_{userId}";
            if (PlayerPrefs.HasKey(key))
            {
                string json = PlayerPrefs.GetString(key);
                await Task.Delay(50);
                return JsonUtility.FromJson<PlayerData>(json);
            }

            return null;
        }

        public async Task UpdatePlayerCoins(string userId, int coins, int gems)
        {
            var player = await LoadPlayerData(userId);
            if (player != null)
            {
                player.coins = coins;
                player.gems = gems;
                await SavePlayerData(player);
            }
        }

        #endregion

        #region Leaderboards

        public async Task SaveScore(string userId, string username, float time, string countryCode)
        {
            Debug.Log($"[Database] Guardando score: {username} - {time}s");

            var existing = globalLeaderboard.Find(e => e.userId == userId);

            if (existing != null)
            {
                if (time < existing.time)
                {
                    existing.time = time;
                    existing.username = username;
                    Debug.Log($"[Database] Nuevo récord: {time}s");
                }
            }
            else
            {
                globalLeaderboard.Add(new LeaderboardEntry
                {
                    userId = userId,
                    username = username,
                    time = time,
                    countryCode = countryCode
                });
            }

            globalLeaderboard.Sort((a, b) => a.time.CompareTo(b.time));

            for (int i = 0; i < globalLeaderboard.Count; i++)
            {
                globalLeaderboard[i].position = i + 1;
            }

            SaveLeaderboard();
            await Task.Delay(50);
        }

        public async Task<List<LeaderboardEntry>> GetGlobalLeaderboard(int topCount = 200)
        {
            Debug.Log($"[Database] Obteniendo top {topCount} global");
            await Task.Delay(100);

            var result = new List<LeaderboardEntry>();
            int count = Math.Min(topCount, globalLeaderboard.Count);

            for (int i = 0; i < count; i++)
            {
                result.Add(globalLeaderboard[i]);
            }

            return result;
        }

        public async Task<List<LeaderboardEntry>> GetCountryLeaderboard(string countryCode, int topCount = 100)
        {
            await Task.Delay(50);
            var filtered = globalLeaderboard.FindAll(e => e.countryCode == countryCode);
            int count = Math.Min(topCount, filtered.Count);
            return filtered.GetRange(0, count);
        }

        public async Task UpdateUsernameInLeaderboards(string userId, string newUsername, string countryCode)
        {
            var entry = globalLeaderboard.Find(e => e.userId == userId);
            if (entry != null)
            {
                entry.username = newUsername;
                SaveLeaderboard();
            }
            await Task.Delay(50);
        }

        /// <summary>
        /// Elimina al usuario de todos los leaderboards
        /// </summary>
        public async Task RemoveUserFromLeaderboards(string userId)
        {
            Debug.Log($"[Database] Eliminando usuario {userId} del leaderboard");

            int removed = globalLeaderboard.RemoveAll(e => e.userId == userId);

            if (removed > 0)
            {
                // Recalcular posiciones
                for (int i = 0; i < globalLeaderboard.Count; i++)
                {
                    globalLeaderboard[i].position = i + 1;
                }

                SaveLeaderboard();
                Debug.Log($"[Database] Usuario eliminado del leaderboard ({removed} entradas)");
            }

            await Task.Delay(50);
        }

        #endregion

        #region Tournaments

        public async Task<bool> CreateTournament(TournamentData tournament)
        {
            Debug.Log($"[Database] Creando torneo: {tournament.name}");
            tournaments[tournament.tournamentId] = tournament;

            string key = $"SimTournament_{tournament.tournamentId}";
            PlayerPrefs.SetString(key, JsonUtility.ToJson(tournament));
            PlayerPrefs.Save();

            await Task.Delay(100);
            return true;
        }

        public async Task<TournamentData> GetTournament(string tournamentId)
        {
            if (tournaments.ContainsKey(tournamentId))
            {
                await Task.Delay(50);
                return tournaments[tournamentId];
            }

            string key = $"SimTournament_{tournamentId}";
            if (PlayerPrefs.HasKey(key))
            {
                var tournament = JsonUtility.FromJson<TournamentData>(PlayerPrefs.GetString(key));
                tournaments[tournamentId] = tournament;
                return tournament;
            }

            return null;
        }

        public async Task<List<TournamentData>> GetActiveTournaments()
        {
            await Task.Delay(100);
            var active = new List<TournamentData>();

            foreach (var t in tournaments.Values)
            {
                if (t.status == TournamentStatus.Scheduled || t.status == TournamentStatus.Active)
                {
                    active.Add(t);
                }
            }

            return active;
        }

        public async Task UpdateTournament(TournamentData tournament)
        {
            tournaments[tournament.tournamentId] = tournament;
            string key = $"SimTournament_{tournament.tournamentId}";
            PlayerPrefs.SetString(key, JsonUtility.ToJson(tournament));
            PlayerPrefs.Save();
            await Task.Delay(50);
        }

        public async Task<bool> JoinTournament(string tournamentId, string userId)
        {
            var tournament = await GetTournament(tournamentId);
            if (tournament == null) return false;

            var player = await LoadPlayerData(userId);
            if (player == null) return false;

            if (tournament.AddParticipant(player))
            {
                await UpdateTournament(tournament);

                if (tournament.entryFee > 0)
                {
                    player.coins -= tournament.entryFee;
                    await SavePlayerData(player);
                }

                Debug.Log($"[Database] {player.username} se unió al torneo");
                return true;
            }

            return false;
        }

        public async Task<bool> LeaveTournament(string tournamentId, string userId)
        {
            var tournament = await GetTournament(tournamentId);
            if (tournament == null) return false;

            if (!tournament.IsParticipating(userId)) return false;

            var player = await LoadPlayerData(userId);
            if (player == null) return false;

            tournament.participants.RemoveAll(p => p.userId == userId);
            tournament.currentParticipants--;

            if (tournament.entryFee > 0)
            {
                player.coins += tournament.entryFee;
                tournament.totalPrizePool -= tournament.entryFee;
                await SavePlayerData(player);
            }

            await UpdateTournament(tournament);
            return true;
        }

        /// <summary>
        /// Actualiza el score de un participante en un torneo
        /// </summary>
        public async Task<bool> UpdateTournamentScore(string tournamentId, string userId, float time)
        {
            var tournament = await GetTournament(tournamentId);
            if (tournament == null)
            {
                Debug.LogWarning($"[Database] Torneo no encontrado: {tournamentId}");
                return false;
            }

            if (!tournament.IsParticipating(userId))
            {
                Debug.LogWarning($"[Database] Usuario {userId} no participa en torneo {tournamentId}");
                return false;
            }

            // Actualizar score del participante
            tournament.UpdateParticipantScore(userId, time);

            // Guardar cambios
            await UpdateTournament(tournament);

            Debug.Log($"[Database] Score actualizado en torneo {tournamentId}: {time}s");
            return true;
        }

        /// <summary>
        /// Obtiene los torneos activos en los que participa un jugador
        /// </summary>
        public async Task<List<TournamentData>> GetPlayerActiveTournaments(string userId)
        {
            await Task.Delay(50);
            var playerTournaments = new List<TournamentData>();

            foreach (var t in tournaments.Values)
            {
                // Solo torneos activos o programados donde el jugador participa
                if ((t.status == TournamentStatus.Scheduled || t.status == TournamentStatus.Active)
                    && t.IsParticipating(userId))
                {
                    playerTournaments.Add(t);
                }
            }

            Debug.Log($"[Database] Torneos activos del jugador {userId}: {playerTournaments.Count}");
            return playerTournaments;
        }

        /// <summary>
        /// Actualiza el score en TODOS los torneos activos donde participa el jugador
        /// </summary>
        public async Task UpdateScoreInAllActiveTournaments(string userId, float time)
        {
            var playerTournaments = await GetPlayerActiveTournaments(userId);

            foreach (var tournament in playerTournaments)
            {
                await UpdateTournamentScore(tournament.tournamentId, userId, time);
            }

            if (playerTournaments.Count > 0)
            {
                Debug.Log($"[Database] Score {time}s actualizado en {playerTournaments.Count} torneos");
            }
        }

        #endregion

        #region Analytics

        public void LogGameEvent(string eventName, Dictionary<string, object> parameters)
        {
            Debug.Log($"[Database] Evento: {eventName}");
        }

        #endregion
    }

    [Serializable]
    public class LeaderboardWrapper
    {
        public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
    }

    [Serializable]
    public class LeaderboardEntry
    {
        public string userId;
        public string username;
        public float time;
        public string countryCode;
        public string avatarUrl;
        public int position;
        public string timestamp;
    }
}
