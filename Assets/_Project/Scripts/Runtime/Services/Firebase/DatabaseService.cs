using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
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
                InitializeAsync().ContinueWithOnMainThread(t =>
                {
                    if (t.IsFaulted)
                        Debug.LogError($"[Database] InitializeAsync failed: {t.Exception?.InnerException?.Message}");
                });
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private async Task InitializeAsync()
        {
            Debug.Log("[Database] Inicializando Firebase Realtime Database...");

            try
            {
                // Firebase ya debería estar inicializado por FirebaseInitializer
                // pero verificamos dependencias
                var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

                if (dependencyStatus == DependencyStatus.Available)
                {
                    // DES-01: Enable offline persistence (auto-caches reads, queues writes)
                    try { FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(true); }
                    catch (DatabaseException) { /* Already set — safe to ignore */ }

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
                string json = PlayerPrefs.GetString("SimLeaderboard", "");
                if (string.IsNullOrEmpty(json)) return;
                try
                {
                    var wrapper = JsonUtility.FromJson<LeaderboardWrapper>(json);
                    if (wrapper?.entries != null)
                    {
                        globalLeaderboard = wrapper.entries;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Database] Corrupt local leaderboard JSON: {e.Message}");
                }
            }

            // No crear datos fake si el leaderboard está vacío — se mostrará empty state en UI
            if (globalLeaderboard.Count == 0)
            {
                Debug.LogWarning("[Database] Leaderboard vacío en modo offline. Se mostrará empty state.");
            }
        }

        private void SaveLeaderboardLocal()
        {
            var wrapper = new LeaderboardWrapper { entries = globalLeaderboard };
            PlayerPrefs.SetString("SimLeaderboard", JsonUtility.ToJson(wrapper));
            PlayerPrefs.Save();
        }

        private async Task LoadLeaderboardFromFirebase()
        {
            await LoadLeaderboardFromFirebase("");
        }

        private async Task LoadLeaderboardFromFirebase(string gameType)
        {
            if (!_isInitialized) return;

            string safeGameType = SanitizeFirebasePath(gameType);
            bool hasGameType = !string.IsNullOrEmpty(safeGameType);
            string path = hasGameType ? $"{LEADERBOARD_PATH}/{safeGameType}" : LEADERBOARD_PATH;

            try
            {
                var snapshot = await _databaseRef.Child(path)
                    .OrderByChild("time")
                    .LimitToFirst(200)
                    .GetValueAsync();

                var entries = new List<LeaderboardEntry>();
                int position = 1;

                foreach (var child in snapshot.Children)
                {
                    var entry = new LeaderboardEntry
                    {
                        userId = child.Child("userId").Value?.ToString() ?? "",
                        username = child.Child("username").Value?.ToString() ?? "Player",
                        time = SafeParseFloat(child.Child("time").Value?.ToString()),
                        countryCode = child.Child("countryCode").Value?.ToString() ?? "US",
                        avatarUrl = child.Child("avatarUrl").Value?.ToString() ?? "",
                        gameId = child.Child("gameId").Value?.ToString() ?? gameType,
                        position = position++
                    };
                    entries.Add(entry);
                }

                // Only update the global cache for the default (all games) query
                if (!hasGameType)
                {
                    globalLeaderboard = entries;
                }

                Debug.Log($"[Database] Leaderboard cargado desde Firebase: {entries.Count} entradas (gameType: {(hasGameType ? safeGameType : "all")})");

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
                        if (playerData == null)
                        {
                            Debug.LogWarning($"[Database] Malformed player JSON for userId={userId}");
                            return null;
                        }
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
                try { return JsonUtility.FromJson<PlayerData>(json); }
                catch (Exception e) { Debug.LogWarning($"[Database] Corrupt local player JSON: {e.Message}"); return null; }
            }
            return null;
        }

        /// <summary>
        /// Actualiza campos específicos del jugador en Firebase sin sobreescribir todo el objeto
        /// </summary>
        public async Task UpdatePlayerFields(string userId, Dictionary<string, object> updates)
        {
            if (string.IsNullOrEmpty(userId) || updates == null || updates.Count == 0) return;

            if (_isInitialized && _databaseRef != null)
            {
                try
                {
                    await _databaseRef.Child(PLAYERS_PATH).Child(userId).UpdateChildrenAsync(updates);
                    Debug.Log($"[Database] Campos actualizados en Firebase para: {userId} ({updates.Count} campos)");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Database] Error actualizando campos en Firebase: {e.Message}");
                    // Fallback: guardar localmente
                    SavePlayerFieldsLocal(userId, updates);
                }
            }
            else
            {
                SavePlayerFieldsLocal(userId, updates);
            }
        }

        private void SavePlayerFieldsLocal(string userId, Dictionary<string, object> updates)
        {
            string key = $"SimUser_{userId}_fields";
            // Merge con datos existentes
            var existing = new Dictionary<string, object>();
            if (PlayerPrefs.HasKey(key))
            {
                try
                {
                    string existingJson = PlayerPrefs.GetString(key);
                    var wrapper = JsonUtility.FromJson<FieldsWrapper>(existingJson);
                    if (wrapper?.keys != null)
                    {
                        for (int i = 0; i < wrapper.keys.Count; i++)
                            existing[wrapper.keys[i]] = wrapper.values[i];
                    }
                }
                catch (Exception ex) { Debug.LogWarning($"[Database] Error parsing local fields: {ex.Message}"); }
            }
            foreach (var kvp in updates)
                existing[kvp.Key] = kvp.Value;

            var newWrapper = new FieldsWrapper();
            foreach (var kvp in existing)
            {
                newWrapper.keys.Add(kvp.Key);
                newWrapper.values.Add(kvp.Value?.ToString() ?? "");
            }
            PlayerPrefs.SetString(key, JsonUtility.ToJson(newWrapper));
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Lee un campo específico del jugador en Firebase. Retorna null si no existe.
        /// </summary>
        public async Task<string> GetPlayerField(string userId, string fieldName)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(fieldName)) return null;

            if (_isInitialized && _databaseRef != null)
            {
                try
                {
                    var snapshot = await _databaseRef.Child(PLAYERS_PATH).Child(userId).Child(fieldName).GetValueAsync();
                    if (snapshot.Exists)
                        return snapshot.Value?.ToString();
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Database] Error leyendo campo {fieldName}: {e.Message}");
                }
            }

            // Fallback local
            string key = $"SimUser_{userId}_fields";
            if (PlayerPrefs.HasKey(key))
            {
                try
                {
                    var wrapper = JsonUtility.FromJson<FieldsWrapper>(PlayerPrefs.GetString(key));
                    if (wrapper?.keys != null)
                    {
                        int idx = wrapper.keys.IndexOf(fieldName);
                        if (idx >= 0) return wrapper.values[idx];
                    }
                }
                catch { /* ignore */ }
            }

            return null;
        }

        /// <summary>
        /// Checks if a username is already taken by any player in the database
        /// </summary>
        public async Task<bool> IsUsernameTaken(string username)
        {
            try
            {
                if (_databaseRef == null) return false;
                var snapshot = await _databaseRef.Child(PLAYERS_PATH)
                    .OrderByChild("username")
                    .EqualTo(username)
                    .GetValueAsync();
                return snapshot.Exists && snapshot.ChildrenCount > 0;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Database] Username check failed: {ex.Message}");
                return true; // Fail-closed: if we can't check, assume taken to prevent duplicates
            }
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
#if UNITY_EDITOR
            await Task.Delay(100); // Simular latencia de red
#endif

            var results = new List<PlayerSearchResult>();
            if (string.IsNullOrEmpty(query)) return results;
            string queryLower = query.ToLower();

            // Obtener datos del usuario actual para verificar amistades
            PlayerData currentUser = null;
            if (AuthenticationService.Instance != null)
            {
                currentUser = AuthenticationService.Instance.GetCurrentPlayerData();
            }

            // Collect matching entries first, then batch-load player data
            var matchingEntries = new List<LeaderboardEntry>();
            foreach (var entry in globalLeaderboard)
            {
                if (entry.username.ToLower().Contains(queryLower))
                {
                    matchingEntries.Add(entry);
                    if (matchingEntries.Count >= maxResults)
                        break;
                }
            }

            // Batch load player data in parallel to avoid N+1 query problem
            var loadTasks = new List<Task<PlayerData>>();
            foreach (var entry in matchingEntries)
            {
                loadTasks.Add(LoadPlayerData(entry.userId));
            }
            var playerDataResults = await Task.WhenAll(loadTasks);

            for (int i = 0; i < matchingEntries.Count; i++)
            {
                var entry = matchingEntries[i];
                var playerData = playerDataResults[i];

                float winRate = playerData != null ? playerData.GetWinRate() : 0f;
                bool isFriend = currentUser != null && currentUser.IsFriend(entry.userId);

                results.Add(new PlayerSearchResult
                {
                    playerId = entry.userId,
                    username = entry.username,
                    winRate = winRate,
                    isFriend = isFriend,
                    avatarUrl = entry.avatarUrl ?? ""
                });
            }

            Debug.Log($"[Database] Encontrados {results.Count} jugadores");
            return results;
        }

        #endregion

        #region Leaderboards

        public async Task SaveScore(string userId, string username, float time, string countryCode)
        {
            await SaveScore(userId, username, time, countryCode, "");
        }

        public async Task SaveScore(string userId, string username, float time, string countryCode, string gameId)
        {
            Debug.Log($"[Database] Guardando score: {username} - {time}s (gameId: {gameId})");

            var entry = new LeaderboardEntry
            {
                userId = userId,
                username = username,
                time = time,
                countryCode = countryCode,
                gameId = gameId,
                timestamp = DateTime.UtcNow.ToString("o")
            };

            // Sanitize user-provided path segments
            string safeGameId = SanitizeFirebasePath(gameId);
            string safeCountryCode = SanitizeFirebasePath(countryCode);

            if (string.IsNullOrEmpty(safeGameId)) safeGameId = "general";
            string globalPath = $"{LEADERBOARD_PATH}/{safeGameId}/{userId}";
            string countryPath = $"{COUNTRY_LEADERBOARD_PATH}/{safeCountryCode}/{userId}";

            if (_isInitialized && _databaseRef != null)
            {
                try
                {
                    // SEC-06 Option B: Get validation token from server before writing
                    string validationToken = await RequestValidationToken(userId, safeGameId, time);

                    // Verificar si ya existe un score para este usuario
                    var existingSnapshot = await _databaseRef.Child(globalPath).GetValueAsync();

                    bool shouldUpdate = true;
                    if (existingSnapshot.Exists)
                    {
                        float existingTime = SafeParseFloat(existingSnapshot.Child("time").Value?.ToString());
                        if (time >= existingTime)
                        {
                            Debug.Log($"[Database] Score no mejorado. Actual: {existingTime}s, Nuevo: {time}s");
                            shouldUpdate = false;
                        }
                    }

                    if (shouldUpdate)
                    {
                        var entryData = new Dictionary<string, object>
                        {
                            { "userId", userId },
                            { "username", username },
                            { "time", time },
                            { "countryCode", countryCode },
                            { "gameId", gameId },
                            { "timestamp", DateTime.UtcNow.ToString("o") },
                            { "validationToken", validationToken ?? "" }
                        };

                        // Atomic multi-path update: global + country + score history
                        string scoreId = _databaseRef.Child(SCORES_PATH).Child(userId).Push().Key;
                        var multiUpdate = new Dictionary<string, object>
                        {
                            { globalPath, entryData },
                            { countryPath, entryData },
                        };
                        // Score history entry includes scoreId
                        var historyData = new Dictionary<string, object>(entryData) { { "scoreId", scoreId } };
                        multiUpdate[$"{SCORES_PATH}/{userId}/{scoreId}"] = historyData;

                        await _databaseRef.UpdateChildrenAsync(multiUpdate);

                        Debug.Log($"[Database] Score guardado en Firebase (atomic): {time}s (gameId: {gameId})");

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

        // ==================== Score Validation (SEC-06) ====================

        /// <summary>
        /// SEC-06 Option B: Requests a validation token from the server for ranked scores.
        /// The token is included in the RTDB write so rules can verify it exists.
        /// Returns empty string if server is unreachable (graceful degradation).
        /// </summary>
        private async Task<string> RequestValidationToken(string userId, string gameId, float time)
        {
            string url = _validateScoreUrl;
            if (string.IsNullOrEmpty(url)) return "";

            try
            {
                string idToken = null;
                if (Payments.PaymentBridge.GetFirebaseIdToken != null)
                {
                    try { idToken = await Payments.PaymentBridge.GetFirebaseIdToken(); }
                    catch { }
                }

                var body = new Dictionary<string, object>
                {
                    { "gameType", gameId },
                    { "score", 0 },
                    { "timeSeconds", time }
                };
                string json = JsonUtility.ToJson(new ValidationRequestBody { gameType = gameId, score = 0, timeSeconds = time });
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);

                using (var request = new UnityEngine.Networking.UnityWebRequest(url, "POST"))
                {
                    request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bytes);
                    request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
                    request.SetRequestHeader("Content-Type", "application/json");
                    if (!string.IsNullOrEmpty(idToken))
                        request.SetRequestHeader("Authorization", $"Bearer {idToken}");
                    request.timeout = 5;

                    var op = request.SendWebRequest();
                    while (!op.isDone) await Task.Yield();

                    if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                    {
                        var response = JsonUtility.FromJson<ValidationTokenResponse>(request.downloadHandler.text);
                        return response?.validationToken ?? "";
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Database] Validation token request failed (score will still save): {e.Message}");
            }
            return "";
        }

        // URL config
        private string _validateScoreUrl;

        public void SetScoreEndpoints(string validateUrl)
        {
            _validateScoreUrl = validateUrl;
        }

        [Serializable]
        private class ValidationRequestBody
        {
            public string gameType;
            public int score;
            public float timeSeconds;
        }

        [Serializable]
        private class ValidationTokenResponse
        {
            public bool valid;
            public string validationToken;
        }

        // ==================== Leaderboard Queries ====================

        public async Task<List<LeaderboardEntry>> GetGlobalLeaderboard(int topCount = 200)
        {
            return await GetGlobalLeaderboard("", topCount);
        }

        public async Task<List<LeaderboardEntry>> GetGlobalLeaderboard(string gameId, int topCount = 200)
        {
            Debug.Log($"[Database] Obteniendo top {topCount} global (gameId: {gameId})");

            string safeGameId = SanitizeFirebasePath(gameId);
            bool hasGameId = !string.IsNullOrEmpty(safeGameId);
            string path = hasGameId ? $"{LEADERBOARD_PATH}/{safeGameId}" : LEADERBOARD_PATH;

            if (_isInitialized && _databaseRef != null)
            {
                // Retry up to 2 times on failure
                for (int attempt = 0; attempt < 2; attempt++)
                {
                    try
                    {
                        var snapshot = await _databaseRef.Child(path)
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
                                time = SafeParseFloat(child.Child("time").Value?.ToString()),
                                countryCode = child.Child("countryCode").Value?.ToString() ?? "US",
                                avatarUrl = child.Child("avatarUrl").Value?.ToString() ?? "",
                                gameId = child.Child("gameId").Value?.ToString() ?? gameId,
                                position = position++
                            });
                        }

                        Debug.Log($"[Database] Leaderboard obtenido de Firebase: {result.Count} entradas");

                        // Update local cache on success
                        if (!hasGameId && result.Count > 0)
                        {
                            globalLeaderboard = result;
                            SaveLeaderboardLocal();
                        }

                        return result;
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[Database] Error obteniendo leaderboard (intento {attempt + 1}/2): {e.Message}");
                        if (attempt < 1) await Task.Delay(500); // Brief delay before retry
                    }
                }
            }

            // Fallback to offline cache
            var localResult = new List<LeaderboardEntry>();
            var source = hasGameId
                ? globalLeaderboard.FindAll(e => e.gameId == gameId)
                : globalLeaderboard;
            int count = Math.Min(topCount, source.Count);
            for (int i = 0; i < count; i++)
            {
                localResult.Add(source[i]);
            }
            Debug.Log($"[Database] Leaderboard desde cache local: {localResult.Count} entradas");
            return localResult;
        }

        public async Task<List<LeaderboardEntry>> GetCountryLeaderboard(string countryCode, int topCount = 100)
        {
            return await GetCountryLeaderboard(countryCode, "", topCount);
        }

        public async Task<List<LeaderboardEntry>> GetCountryLeaderboard(string countryCode, string gameId, int topCount = 100)
        {
            Debug.Log($"[Database] Obteniendo leaderboard de {countryCode} (gameId: {gameId})");

            string safeCountryCode = SanitizeFirebasePath(countryCode);
            string safeGameId = SanitizeFirebasePath(gameId);
            bool hasGameId = !string.IsNullOrEmpty(safeGameId);
            string path = hasGameId
                ? $"{COUNTRY_LEADERBOARD_PATH}/{safeCountryCode}/{safeGameId}"
                : $"{COUNTRY_LEADERBOARD_PATH}/{safeCountryCode}";

            if (_isInitialized && _databaseRef != null)
            {
                try
                {
                    var snapshot = await _databaseRef.Child(path)
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
                            time = SafeParseFloat(child.Child("time").Value?.ToString()),
                            countryCode = countryCode,
                            avatarUrl = child.Child("avatarUrl").Value?.ToString() ?? "",
                            gameId = child.Child("gameId").Value?.ToString() ?? gameId,
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
            var filtered = globalLeaderboard.FindAll(e =>
                e.countryCode == countryCode && (!hasGameId || e.gameId == gameId));
            int count = Math.Min(topCount, filtered.Count);
            return count > 0 ? filtered.GetRange(0, count) : new List<LeaderboardEntry>();
        }

        // All known game types for per-game leaderboard operations
        private static readonly string[] ALL_GAME_TYPES = { "DigitRush", "MemoryPairs", "QuickMath", "FlashTap", "OddOneOut" };

        public async Task UpdateUsernameInLeaderboards(string userId, string newUsername, string countryCode)
        {
            Debug.Log($"[Database] Actualizando username en leaderboards: {newUsername}");

            string safeCountryCode = SanitizeFirebasePath(countryCode);

            if (_isInitialized && _databaseRef != null)
            {
                try
                {
                    // Actualizar en leaderboard global (legacy flat path)
                    await _databaseRef.Child(LEADERBOARD_PATH).Child(userId).Child("username").SetValueAsync(newUsername);

                    // Actualizar en leaderboard de país (legacy flat path)
                    await _databaseRef.Child(COUNTRY_LEADERBOARD_PATH).Child(safeCountryCode).Child(userId).Child("username").SetValueAsync(newUsername);

                    // Actualizar en per-game leaderboard paths
                    foreach (string gameType in ALL_GAME_TYPES)
                    {
                        var gameSnap = await _databaseRef.Child(LEADERBOARD_PATH).Child(gameType).Child(userId).GetValueAsync();
                        if (gameSnap.Exists)
                        {
                            await _databaseRef.Child(LEADERBOARD_PATH).Child(gameType).Child(userId).Child("username").SetValueAsync(newUsername);
                        }

                        var countryGameSnap = await _databaseRef.Child(COUNTRY_LEADERBOARD_PATH).Child(safeCountryCode).Child(gameType).Child(userId).GetValueAsync();
                        if (countryGameSnap.Exists)
                        {
                            await _databaseRef.Child(COUNTRY_LEADERBOARD_PATH).Child(safeCountryCode).Child(gameType).Child(userId).Child("username").SetValueAsync(newUsername);
                        }
                    }

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
                    string countryCode = (userSnapshot != null && userSnapshot.Exists)
                        ? userSnapshot.Child("countryCode").Value?.ToString() ?? ""
                        : "";

                    // Eliminar de leaderboard global (legacy flat path)
                    await _databaseRef.Child(LEADERBOARD_PATH).Child(userId).RemoveValueAsync();

                    // Eliminar de leaderboard de país (legacy flat path)
                    if (!string.IsNullOrEmpty(countryCode))
                    {
                        string safeCountryCode = SanitizeFirebasePath(countryCode);
                        await _databaseRef.Child(COUNTRY_LEADERBOARD_PATH).Child(safeCountryCode).Child(userId).RemoveValueAsync();

                        // Eliminar de per-game leaderboard paths
                        foreach (string gameType in ALL_GAME_TYPES)
                        {
                            await _databaseRef.Child(LEADERBOARD_PATH).Child(gameType).Child(userId).RemoveValueAsync();
                            await _databaseRef.Child(COUNTRY_LEADERBOARD_PATH).Child(safeCountryCode).Child(gameType).Child(userId).RemoveValueAsync();
                        }
                    }
                    else
                    {
                        // No country code — still remove per-game global entries
                        foreach (string gameType in ALL_GAME_TYPES)
                        {
                            await _databaseRef.Child(LEADERBOARD_PATH).Child(gameType).Child(userId).RemoveValueAsync();
                        }
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
                            time = SafeParseFloat(child.Child("time").Value?.ToString(), 0f),
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

        #region GDPR — Data Deletion

        public async Task DeleteMatchHistory(string userId)
        {
            if (!_isInitialized || _databaseRef == null) return;
            try { await _databaseRef.Child("matchHistory").Child(userId).RemoveValueAsync(); }
            catch (Exception ex) { Debug.LogWarning($"[Database] DeleteMatchHistory: {ex.Message}"); }
        }

        public async Task DeleteAchievements(string userId)
        {
            if (!_isInitialized || _databaseRef == null) return;
            try { await _databaseRef.Child("achievements").Child(userId).RemoveValueAsync(); }
            catch (Exception ex) { Debug.LogWarning($"[Database] DeleteAchievements: {ex.Message}"); }
        }

        /// <summary>
        /// B5-A: Writes /friends/{userId}/{friendId} = true.
        /// The bilateral rule allows the current user (friendId) to write to this path.
        /// Use instead of SavePlayerData(senderData) in AcceptFriendRequest.
        /// </summary>
        public async Task AddFriendEntry(string userId, string friendId)
        {
            if (!_isInitialized || _databaseRef == null) return;
            try
            {
                await _databaseRef.Child("friends").Child(userId).Child(friendId).SetValueAsync(true);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Database] AddFriendEntry({userId},{friendId}): {ex.Message}");
            }
        }

        /// <summary>
        /// B5-B: Removes /friends/{userId}/{friendId}.
        /// The bilateral rule allows the current user (friendId) to write to this path.
        /// Use instead of SavePlayerData(friendData) in RemoveFriend.
        /// </summary>
        public async Task RemoveFriendEntry(string userId, string friendId)
        {
            if (!_isInitialized || _databaseRef == null) return;
            try
            {
                await _databaseRef.Child("friends").Child(userId).Child(friendId).RemoveValueAsync();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Database] RemoveFriendEntry({userId},{friendId}): {ex.Message}");
            }
        }

        public async Task DeleteFriendsList(string userId)
        {
            if (!_isInitialized || _databaseRef == null) return;
            try { await _databaseRef.Child("friends").Child(userId).RemoveValueAsync(); }
            catch (Exception ex) { Debug.LogWarning($"[Database] DeleteFriendsList: {ex.Message}"); }
        }

        public async Task DeleteNotifications(string userId)
        {
            if (!_isInitialized || _databaseRef == null) return;
            try { await _databaseRef.Child("notifications").Child(userId).RemoveValueAsync(); }
            catch (Exception ex) { Debug.LogWarning($"[Database] DeleteNotifications: {ex.Message}"); }
        }

        public async Task DeleteTournamentHistory(string userId)
        {
            if (!_isInitialized || _databaseRef == null) return;
            try { await _databaseRef.Child("tournamentHistory").Child(userId).RemoveValueAsync(); }
            catch (Exception ex) { Debug.LogWarning($"[Database] DeleteTournamentHistory: {ex.Message}"); }
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
                    // AUDIT-FIXED [2026-03-10] M-02: Retornar false si Firebase falla para torneos reales
                    // Solo continuar con cache local en modo offline/simulación
                    tournaments[tournament.tournamentId] = tournament;
                    string keyFail = $"SimTournament_{tournament.tournamentId}";
                    PlayerPrefs.SetString(keyFail, JsonUtility.ToJson(tournament));
                    PlayerPrefs.Save();
                    return false;
                }
            }

            // Guardar localmente también como cache
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
                        if (tournament == null)
                        {
                            Debug.LogWarning($"[Database] GetTournament: malformed JSON for tournamentId={tournamentId}");
                            return null;
                        }
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
                try
                {
                    var tournament = JsonUtility.FromJson<TournamentData>(PlayerPrefs.GetString(key));
                    if (tournament != null) tournaments[tournamentId] = tournament;
                    return tournament;
                }
                catch (Exception e) { Debug.LogWarning($"[Database] Corrupt local tournament JSON: {e.Message}"); }
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
                    // AUDIT-FIXED [2026-03-10] H-04: Limitar la consulta para evitar descargar
                    // toda la colección de torneos. Firebase RTDB no filtra por valor, solo ordena.
                    // La reestructura ideal sería tournaments/active/{id} vs tournaments/completed/{id}.
                    // Como mitigation: limitar a 200 torneos más recientes por status.
                    var snapshot = await _databaseRef.Child(TOURNAMENTS_PATH)
                        .OrderByChild("status")
                        .LimitToLast(200)
                        .GetValueAsync();

                    var active = new List<TournamentData>();

                    foreach (var child in snapshot.Children)
                    {
                        string json = child.GetRawJsonValue();
                        var tournament = JsonUtility.FromJson<TournamentData>(json);

                        if (tournament == null) continue;
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

        public async Task<bool> UpdateTournament(TournamentData tournament) // B1-E: retorna bool para propagación de error
        {
            Debug.Log($"[Database] Actualizando torneo: {tournament.tournamentId}");

            bool firebaseSuccess = false;
            if (_isInitialized && _databaseRef != null)
            {
                try
                {
                    string json = JsonUtility.ToJson(tournament);
                    await _databaseRef.Child(TOURNAMENTS_PATH).Child(tournament.tournamentId).SetRawJsonValueAsync(json);
                    firebaseSuccess = true;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Database] Error actualizando torneo: {e.Message}");
                    return false;
                }
            }

            // Guardar localmente
            tournaments[tournament.tournamentId] = tournament;
            string key = $"SimTournament_{tournament.tournamentId}";
            PlayerPrefs.SetString(key, JsonUtility.ToJson(tournament));
            PlayerPrefs.Save();
            return firebaseSuccess;
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
        /// Obtiene los torneos activos en los que participa un jugador.
        /// AUDIT-FIXED [2026-03-10] H-05: Consulta Firebase si el cache local está vacío (ej. tras reinicio de app).
        /// </summary>
        public async Task<List<TournamentData>> GetPlayerActiveTournaments(string userId)
        {
            // Si el cache en memoria está vacío, recargar desde Firebase primero
            if (tournaments.Count == 0)
            {
                Debug.Log("[Database] Cache de torneos vacío — recargando desde Firebase");
                var allActive = await GetActiveTournaments();
                foreach (var t in allActive)
                    tournaments[t.tournamentId] = t;
            }

            var playerTournaments = new List<TournamentData>();

            foreach (var t in tournaments.Values)
            {
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

        #region Friend Requests

        public async Task SaveFriendRequest(FriendRequest request)
        {
            if (!_isInitialized || _databaseRef == null || request == null) return;

            try
            {
                string json = JsonUtility.ToJson(request);
                await _databaseRef.Child("friend_requests").Child(request.receiverId).Child(request.requestId).SetRawJsonValueAsync(json);
                await _databaseRef.Child("friend_requests").Child(request.senderId).Child(request.requestId).SetRawJsonValueAsync(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DatabaseService] SaveFriendRequest failed: {e.Message}");
            }
        }

        public async Task UpdateFriendRequestStatus(FriendRequest request)
        {
            if (!_isInitialized || _databaseRef == null || request == null) return;

            try
            {
                string json = JsonUtility.ToJson(request);
                // Update in both sender and receiver nodes
                await _databaseRef.Child("friend_requests").Child(request.receiverId).Child(request.requestId).SetRawJsonValueAsync(json);
                await _databaseRef.Child("friend_requests").Child(request.senderId).Child(request.requestId).SetRawJsonValueAsync(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DatabaseService] UpdateFriendRequestStatus failed: {e.Message}");
            }
        }

        public async Task DeleteFriendRequest(FriendRequest request)
        {
            if (!_isInitialized || _databaseRef == null || request == null) return;

            try
            {
                await _databaseRef.Child("friend_requests").Child(request.receiverId).Child(request.requestId).RemoveValueAsync();
                await _databaseRef.Child("friend_requests").Child(request.senderId).Child(request.requestId).RemoveValueAsync();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DatabaseService] DeleteFriendRequest failed: {e.Message}");
            }
        }

        public async Task<List<FriendRequest>> LoadFriendRequests(string userId)
        {
            var requests = new List<FriendRequest>();
            if (!_isInitialized || string.IsNullOrEmpty(userId)) return requests;

            try
            {
                var snapshot = await _databaseRef.Child("friend_requests").Child(userId).GetValueAsync();

                if (snapshot != null && snapshot.Exists)
                {
                    foreach (var child in snapshot.Children)
                    {
                        try
                        {
                            string json = child.GetRawJsonValue();
                            var request = JsonUtility.FromJson<FriendRequest>(json);
                            if (request != null)
                                requests.Add(request);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"[DatabaseService] Failed to parse friend request {child.Key}: {e.Message}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DatabaseService] LoadFriendRequests failed: {e.Message}");
            }

            return requests;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Removes characters not allowed in Firebase Realtime Database paths: / . $ [ ] #
        /// </summary>
        private static string SanitizeFirebasePath(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return Regex.Replace(input, @"[/.\$\[\]#]", "");
        }

        private static float SafeParseFloat(string value, float defaultValue = 999f)
        {
            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                return result;
            return defaultValue;
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
        public string gameId;
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

    [Serializable]
    public class FieldsWrapper
    {
        public List<string> keys = new List<string>();
        public List<string> values = new List<string>();
    }
}
