using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using DigitPark.Data;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

namespace DigitPark.Services.Firebase
{
    /// <summary>
    /// Servicio de base de datos Firebase Realtime Database
    /// Maneja guardado de jugadores, scores y leaderboards
    /// </summary>
    public class DatabaseService : MonoBehaviour
    {
        public static DatabaseService Instance { get; private set; }

        // Referencias a Firebase
        private DatabaseReference _databaseRef;
        private bool _isInitialized = false;

        // Datos en memoria (cache)
        private List<LeaderboardEntry> globalLeaderboard = new List<LeaderboardEntry>();
        private Dictionary<string, TournamentData> tournaments = new Dictionary<string, TournamentData>();

        // Constantes de rutas en Firebase
        private const string PLAYERS_PATH = "players";
        private const string LEADERBOARD_PATH = "leaderboards/global";
        private const string COUNTRY_LEADERBOARD_PATH = "leaderboards/country";
        private const string TOURNAMENTS_PATH = "tournaments";
        private const string SCORES_PATH = "scores";

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

        private async void Initialize()
        {
            Debug.Log("[Database] Inicializando Firebase Realtime Database...");

            try
            {
                // Firebase ya debería estar inicializado por FirebaseInitializer
                // pero verificamos dependencias
                var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

                if (dependencyStatus == DependencyStatus.Available)
                {
                    _databaseRef = FirebaseDatabase.DefaultInstance.RootReference;
                    _isInitialized = true;
                    Debug.Log("[Database] Firebase Realtime Database inicializado correctamente");

                    // Cargar leaderboard inicial
                    await LoadLeaderboardFromFirebase();
                }
                else
                {
                    Debug.LogError($"[Database] Firebase no disponible: {dependencyStatus}");
                    // Fallback a datos locales
                    LoadLocalData();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[Database] Error inicializando Firebase: {e.Message}");
                LoadLocalData();
            }
        }

        private void LoadLocalData()
        {
            Debug.Log("[Database] Cargando datos locales como fallback");

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

            SaveLeaderboardLocal();
        }

        private void SaveLeaderboardLocal()
        {
            var wrapper = new LeaderboardWrapper { entries = globalLeaderboard };
            PlayerPrefs.SetString("SimLeaderboard", JsonUtility.ToJson(wrapper));
            PlayerPrefs.Save();
        }

        private async Task LoadLeaderboardFromFirebase()
        {
            if (!_isInitialized) return;

            try
            {
                var snapshot = await _databaseRef.Child(LEADERBOARD_PATH)
                    .OrderByChild("time")
                    .LimitToFirst(200)
                    .GetValueAsync();

                globalLeaderboard.Clear();
                int position = 1;

                foreach (var child in snapshot.Children)
                {
                    var entry = new LeaderboardEntry
                    {
                        userId = child.Child("userId").Value?.ToString() ?? "",
                        username = child.Child("username").Value?.ToString() ?? "Player",
                        time = float.Parse(child.Child("time").Value?.ToString() ?? "999"),
                        countryCode = child.Child("countryCode").Value?.ToString() ?? "US",
                        avatarUrl = child.Child("avatarUrl").Value?.ToString() ?? "",
                        position = position++
                    };
                    globalLeaderboard.Add(entry);
                }

                Debug.Log($"[Database] Leaderboard cargado desde Firebase: {globalLeaderboard.Count} entradas");

                // Guardar cache local
                SaveLeaderboardLocal();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Database] Error cargando leaderboard de Firebase: {e.Message}");
                LoadLocalData();
            }
        }

        #region Player Data

        public async Task SavePlayerData(PlayerData playerData)
        {
            Debug.Log($"[Database] Guardando jugador: {playerData.userId}");

            if (_isInitialized && _databaseRef != null)
            {
                try
                {
                    // Guardar en Firebase
                    string json = JsonUtility.ToJson(playerData);
                    await _databaseRef.Child(PLAYERS_PATH).Child(playerData.userId).SetRawJsonValueAsync(json);
                    Debug.Log($"[Database] Jugador guardado en Firebase: {playerData.userId}");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Database] Error guardando en Firebase, usando local: {e.Message}");
                    SavePlayerDataLocal(playerData);
                }
            }
            else
            {
                SavePlayerDataLocal(playerData);
            }
        }

        private void SavePlayerDataLocal(PlayerData playerData)
        {
            string key = $"SimUser_{playerData.userId}";
            PlayerPrefs.SetString(key, JsonUtility.ToJson(playerData));
            PlayerPrefs.Save();
        }

        public async Task<PlayerData> LoadPlayerData(string userId)
        {
            Debug.Log($"[Database] Cargando jugador: {userId}");

            if (_isInitialized && _databaseRef != null)
            {
                try
                {
                    var snapshot = await _databaseRef.Child(PLAYERS_PATH).Child(userId).GetValueAsync();

                    if (snapshot.Exists)
                    {
                        string json = snapshot.GetRawJsonValue();
                        var playerData = JsonUtility.FromJson<PlayerData>(json);
                        Debug.Log($"[Database] Jugador cargado de Firebase: {userId}");
                        return playerData;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Database] Error cargando de Firebase, usando local: {e.Message}");
                }
            }

            // Fallback a datos locales
            return LoadPlayerDataLocal(userId);
        }

        private PlayerData LoadPlayerDataLocal(string userId)
        {
            string key = $"SimUser_{userId}";
            if (PlayerPrefs.HasKey(key))
            {
                string json = PlayerPrefs.GetString(key);
                return JsonUtility.FromJson<PlayerData>(json);
            }
            return null;
        }

        /// <summary>
        /// Obtiene los datos de un jugador por su ID (alias de LoadPlayerData)
        /// </summary>
        public async Task<PlayerData> GetPlayerDataById(string playerId)
        {
            Debug.Log($"[Database] Obteniendo datos del jugador: {playerId}");
            return await LoadPlayerData(playerId);
        }

        /// <summary>
        /// Busca jugadores por nombre de usuario
        /// </summary>
        public async Task<List<PlayerSearchResult>> SearchPlayers(string query, int maxResults = 20)
        {
            Debug.Log($"[Database] Buscando jugadores: {query}");
            await Task.Delay(100); // Simular latencia de red

            var results = new List<PlayerSearchResult>();
            string queryLower = query.ToLower();

            // Obtener datos del usuario actual para verificar amistades
            PlayerData currentUser = null;
            if (AuthenticationService.Instance != null)
            {
                currentUser = AuthenticationService.Instance.GetCurrentPlayerData();
            }

            // Buscar en el leaderboard (donde tenemos usuarios registrados)
            foreach (var entry in globalLeaderboard)
            {
                if (entry.username.ToLower().Contains(queryLower))
                {
                    // Intentar cargar datos completos del jugador
                    var playerData = await LoadPlayerData(entry.userId);

                    float winRate = 0f;
                    bool isFriend = false;

                    if (playerData != null)
                    {
                        winRate = playerData.GetWinRate();
                    }

                    // Verificar si es amigo
                    if (currentUser != null)
                    {
                        isFriend = currentUser.IsFriend(entry.userId);
                    }

                    results.Add(new PlayerSearchResult
                    {
                        playerId = entry.userId,
                        username = entry.username,
                        winRate = winRate,
                        isFriend = isFriend,
                        avatarUrl = entry.avatarUrl ?? ""
                    });

                    if (results.Count >= maxResults)
                        break;
                }
            }

            Debug.Log($"[Database] Encontrados {results.Count} jugadores");
            return results;
        }

        #endregion

        #region Leaderboards

        public async Task SaveScore(string userId, string username, float time, string countryCode)
        {
            Debug.Log($"[Database] Guardando score: {username} - {time}s");

            var entry = new LeaderboardEntry
            {
                userId = userId,
                username = username,
                time = time,
                countryCode = countryCode,
                timestamp = DateTime.UtcNow.ToString("o")
            };

            if (_isInitialized && _databaseRef != null)
            {
                try
                {
                    // Verificar si ya existe un score para este usuario
                    var existingSnapshot = await _databaseRef.Child(LEADERBOARD_PATH).Child(userId).GetValueAsync();

                    bool shouldUpdate = true;
                    if (existingSnapshot.Exists)
                    {
                        float existingTime = float.Parse(existingSnapshot.Child("time").Value?.ToString() ?? "999");
                        if (time >= existingTime)
                        {
                            Debug.Log($"[Database] Score no mejorado. Actual: {existingTime}s, Nuevo: {time}s");
                            shouldUpdate = false;
                        }
                    }

                    if (shouldUpdate)
                    {
                        // Guardar en leaderboard global
                        var entryData = new Dictionary<string, object>
                        {
                            { "userId", userId },
                            { "username", username },
                            { "time", time },
                            { "countryCode", countryCode },
                            { "timestamp", DateTime.UtcNow.ToString("o") }
                        };

                        await _databaseRef.Child(LEADERBOARD_PATH).Child(userId).SetValueAsync(entryData);

                        // Guardar en leaderboard por país
                        await _databaseRef.Child(COUNTRY_LEADERBOARD_PATH).Child(countryCode).Child(userId).SetValueAsync(entryData);

                        // Guardar en historial de scores
                        string scoreId = _databaseRef.Child(SCORES_PATH).Child(userId).Push().Key;
                        entryData["scoreId"] = scoreId;
                        await _databaseRef.Child(SCORES_PATH).Child(userId).Child(scoreId).SetValueAsync(entryData);

                        Debug.Log($"[Database] Score guardado en Firebase: {time}s");

                        // Recargar leaderboard
                        await LoadLeaderboardFromFirebase();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Database] Error guardando score en Firebase: {e.Message}");
                    SaveScoreLocal(entry);
                }
            }
            else
            {
                SaveScoreLocal(entry);
            }
        }

        private void SaveScoreLocal(LeaderboardEntry entry)
        {
            var existing = globalLeaderboard.Find(e => e.userId == entry.userId);

            if (existing != null)
            {
                if (entry.time < existing.time)
                {
                    existing.time = entry.time;
                    existing.username = entry.username;
                    Debug.Log($"[Database] Nuevo récord local: {entry.time}s");
                }
            }
            else
            {
                globalLeaderboard.Add(entry);
            }

            globalLeaderboard.Sort((a, b) => a.time.CompareTo(b.time));

            for (int i = 0; i < globalLeaderboard.Count; i++)
            {
                globalLeaderboard[i].position = i + 1;
            }

            SaveLeaderboardLocal();
        }

        public async Task<List<LeaderboardEntry>> GetGlobalLeaderboard(int topCount = 200)
        {
            Debug.Log($"[Database] Obteniendo top {topCount} global");

            if (_isInitialized && _databaseRef != null)
            {
                try
                {
                    var snapshot = await _databaseRef.Child(LEADERBOARD_PATH)
                        .OrderByChild("time")
                        .LimitToFirst(topCount)
                        .GetValueAsync();

                    var result = new List<LeaderboardEntry>();
                    int position = 1;

                    foreach (var child in snapshot.Children)
                    {
                        result.Add(new LeaderboardEntry
                        {
                            userId = child.Child("userId").Value?.ToString() ?? "",
                            username = child.Child("username").Value?.ToString() ?? "Player",
                            time = float.Parse(child.Child("time").Value?.ToString() ?? "999"),
                            countryCode = child.Child("countryCode").Value?.ToString() ?? "US",
                            position = position++
                        });
                    }

                    Debug.Log($"[Database] Leaderboard obtenido de Firebase: {result.Count} entradas");
                    return result;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Database] Error obteniendo leaderboard: {e.Message}");
                }
            }

            // Fallback a datos locales
            var localResult = new List<LeaderboardEntry>();
            int count = Math.Min(topCount, globalLeaderboard.Count);
            for (int i = 0; i < count; i++)
            {
                localResult.Add(globalLeaderboard[i]);
            }
            return localResult;
        }

        public async Task<List<LeaderboardEntry>> GetCountryLeaderboard(string countryCode, int topCount = 100)
        {
            Debug.Log($"[Database] Obteniendo leaderboard de {countryCode}");

            if (_isInitialized && _databaseRef != null)
            {
                try
                {
                    var snapshot = await _databaseRef.Child(COUNTRY_LEADERBOARD_PATH).Child(countryCode)
                        .OrderByChild("time")
                        .LimitToFirst(topCount)
                        .GetValueAsync();

                    var result = new List<LeaderboardEntry>();
                    int position = 1;

                    foreach (var child in snapshot.Children)
                    {
                        result.Add(new LeaderboardEntry
                        {
                            userId = child.Child("userId").Value?.ToString() ?? "",
                            username = child.Child("username").Value?.ToString() ?? "Player",
                            time = float.Parse(child.Child("time").Value?.ToString() ?? "999"),
                            countryCode = countryCode,
                            position = position++
                        });
                    }

                    return result;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Database] Error obteniendo leaderboard de país: {e.Message}");
                }
            }

            // Fallback a datos locales
            var filtered = globalLeaderboard.FindAll(e => e.countryCode == countryCode);
            int count = Math.Min(topCount, filtered.Count);
            return count > 0 ? filtered.GetRange(0, count) : new List<LeaderboardEntry>();
        }

        public async Task UpdateUsernameInLeaderboards(string userId, string newUsername, string countryCode)
        {
            Debug.Log($"[Database] Actualizando username en leaderboards: {newUsername}");

            if (_isInitialized && _databaseRef != null)
            {
                try
                {
                    // Actualizar en leaderboard global
                    await _databaseRef.Child(LEADERBOARD_PATH).Child(userId).Child("username").SetValueAsync(newUsername);

                    // Actualizar en leaderboard de país
                    await _databaseRef.Child(COUNTRY_LEADERBOARD_PATH).Child(countryCode).Child(userId).Child("username").SetValueAsync(newUsername);

                    Debug.Log($"[Database] Username actualizado en Firebase");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Database] Error actualizando username: {e.Message}");
                }
            }

            // Actualizar localmente también
            var entry = globalLeaderboard.Find(e => e.userId == userId);
            if (entry != null)
            {
                entry.username = newUsername;
                SaveLeaderboardLocal();
            }
        }

        /// <summary>
        /// Elimina al usuario de todos los leaderboards
        /// </summary>
        public async Task RemoveUserFromLeaderboards(string userId)
        {
            Debug.Log($"[Database] Eliminando usuario {userId} del leaderboard");

            if (_isInitialized && _databaseRef != null)
            {
                try
                {
                    // Obtener el país del usuario primero
                    var userSnapshot = await _databaseRef.Child(LEADERBOARD_PATH).Child(userId).GetValueAsync();
                    string countryCode = userSnapshot.Child("countryCode").Value?.ToString() ?? "";

                    // Eliminar de leaderboard global
                    await _databaseRef.Child(LEADERBOARD_PATH).Child(userId).RemoveValueAsync();

                    // Eliminar de leaderboard de país
                    if (!string.IsNullOrEmpty(countryCode))
                    {
                        await _databaseRef.Child(COUNTRY_LEADERBOARD_PATH).Child(countryCode).Child(userId).RemoveValueAsync();
                    }

                    Debug.Log($"[Database] Usuario eliminado de leaderboards en Firebase");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Database] Error eliminando de leaderboards: {e.Message}");
                }
            }

            // Eliminar localmente
            int removed = globalLeaderboard.RemoveAll(e => e.userId == userId);
            if (removed > 0)
            {
                for (int i = 0; i < globalLeaderboard.Count; i++)
                {
                    globalLeaderboard[i].position = i + 1;
                }
                SaveLeaderboardLocal();
            }
        }

        /// <summary>
        /// Obtiene el historial de scores de un usuario
        /// </summary>
        public async Task<List<ScoreEntry>> GetUserScoreHistory(string userId, int limit = 30)
        {
            Debug.Log($"[Database] Obteniendo historial de scores para: {userId}");

            var scores = new List<ScoreEntry>();

            if (_isInitialized && _databaseRef != null)
            {
                try
                {
                    var snapshot = await _databaseRef.Child(SCORES_PATH).Child(userId)
                        .OrderByChild("timestamp")
                        .LimitToLast(limit)
                        .GetValueAsync();

                    foreach (var child in snapshot.Children)
                    {
                        scores.Add(new ScoreEntry
                        {
                            time = float.Parse(child.Child("time").Value?.ToString() ?? "0"),
                            timestamp = child.Child("timestamp").Value?.ToString() ?? ""
                        });
                    }

                    // Ordenar de más reciente a más antiguo
                    scores.Reverse();
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Database] Error obteniendo historial: {e.Message}");
                }
            }

            return scores;
        }

        #endregion

        #region Tournaments

        public async Task<bool> CreateTournament(TournamentData tournament)
        {
            Debug.Log($"[Database] Creando torneo: {tournament.name}");

            if (_isInitialized && _databaseRef != null)
            {
                try
                {
                    string json = JsonUtility.ToJson(tournament);
                    await _databaseRef.Child(TOURNAMENTS_PATH).Child(tournament.tournamentId).SetRawJsonValueAsync(json);
                    Debug.Log($"[Database] Torneo creado en Firebase: {tournament.tournamentId}");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Database] Error creando torneo en Firebase: {e.Message}");
                }
            }

            // Guardar localmente también
            tournaments[tournament.tournamentId] = tournament;
            string key = $"SimTournament_{tournament.tournamentId}";
            PlayerPrefs.SetString(key, JsonUtility.ToJson(tournament));
            PlayerPrefs.Save();

            return true;
        }

        public async Task<TournamentData> GetTournament(string tournamentId)
        {
            // Primero buscar en cache
            if (tournaments.ContainsKey(tournamentId))
            {
                return tournaments[tournamentId];
            }

            if (_isInitialized && _databaseRef != null)
            {
                try
                {
                    var snapshot = await _databaseRef.Child(TOURNAMENTS_PATH).Child(tournamentId).GetValueAsync();
                    if (snapshot.Exists)
                    {
                        string json = snapshot.GetRawJsonValue();
                        var tournament = JsonUtility.FromJson<TournamentData>(json);
                        tournaments[tournamentId] = tournament;
                        return tournament;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Database] Error obteniendo torneo: {e.Message}");
                }
            }

            // Fallback local
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
            Debug.Log("[Database] Obteniendo torneos activos");

            if (_isInitialized && _databaseRef != null)
            {
                try
                {
                    var snapshot = await _databaseRef.Child(TOURNAMENTS_PATH)
                        .OrderByChild("status")
                        .GetValueAsync();

                    var active = new List<TournamentData>();

                    foreach (var child in snapshot.Children)
                    {
                        string json = child.GetRawJsonValue();
                        var tournament = JsonUtility.FromJson<TournamentData>(json);

                        if (tournament.status == Data.TournamentStatus.Scheduled ||
                            tournament.status == Data.TournamentStatus.Active)
                        {
                            active.Add(tournament);
                            tournaments[tournament.tournamentId] = tournament;
                        }
                    }

                    return active;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Database] Error obteniendo torneos: {e.Message}");
                }
            }

            // Fallback local
            var localActive = new List<TournamentData>();
            foreach (var t in tournaments.Values)
            {
                if (t.status == Data.TournamentStatus.Scheduled || t.status == Data.TournamentStatus.Active)
                {
                    localActive.Add(t);
                }
            }
            return localActive;
        }

        public async Task UpdateTournament(TournamentData tournament)
        {
            Debug.Log($"[Database] Actualizando torneo: {tournament.tournamentId}");

            if (_isInitialized && _databaseRef != null)
            {
                try
                {
                    string json = JsonUtility.ToJson(tournament);
                    await _databaseRef.Child(TOURNAMENTS_PATH).Child(tournament.tournamentId).SetRawJsonValueAsync(json);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Database] Error actualizando torneo: {e.Message}");
                }
            }

            // Guardar localmente
            tournaments[tournament.tournamentId] = tournament;
            string key = $"SimTournament_{tournament.tournamentId}";
            PlayerPrefs.SetString(key, JsonUtility.ToJson(tournament));
            PlayerPrefs.Save();
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

                // Entry fee se maneja via dinero real, no virtual coins

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

            // Refund de entry fee se maneja via dinero real, no virtual coins
            if (tournament.entryFee > 0)
            {
                tournament.totalPrizePool -= tournament.entryFee;
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
                if ((t.status == Data.TournamentStatus.Scheduled || t.status == Data.TournamentStatus.Active)
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

    /// <summary>
    /// Datos de resultado de busqueda de jugador
    /// </summary>
    [Serializable]
    public class PlayerSearchResult
    {
        public string playerId;
        public string username;
        public float winRate;
        public bool isFriend;
        public string avatarUrl;
        public string favoriteGame;
        public bool isOnline;
    }
}
