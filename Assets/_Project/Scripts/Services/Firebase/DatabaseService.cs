using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using DigitPark.Data;

using Firebase;
using Firebase.Database;
using Firebase.Firestore;
using Firebase.Extensions;

namespace DigitPark.Services.Firebase
{
    /// <summary>
    /// Servicio para gestionar la base de datos de Firebase
    /// Maneja players, scores, tournaments y leaderboards
    /// </summary>
    public class DatabaseService : MonoBehaviour
    {
        public static DatabaseService Instance { get; private set; }
        private DatabaseReference databaseRef;
        private FirebaseFirestore firestore;

  
        private const string DATABASE_URL = "https://digitpark-7d772-default-rtdb.firebaseio.com/";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDatabase();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Inicializa Firebase Database
        /// </summary>
        private async void InitializeDatabase()
        {
            Debug.Log("[Database] Inicializando Firebase Database...");

            try
            {
                // Verificar dependencias de Firebase primero
                var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

                if (dependencyStatus != DependencyStatus.Available)
                {
                    Debug.LogError($"[Database] No se pudieron resolver las dependencias de Firebase: {dependencyStatus}");
                    return;
                }

                // Obtener la app de Firebase
                FirebaseApp app = FirebaseApp.DefaultInstance;

                Debug.Log($"[Database] Firebase App obtenida, configurando Database...");

                // Obtener la instancia con la URL específica
                databaseRef = FirebaseDatabase.GetInstance(app, DATABASE_URL).RootReference;
                firestore = FirebaseFirestore.DefaultInstance;

                Debug.Log($"[Database] Firebase Database inicializado correctamente con URL: {DATABASE_URL}");
                Debug.Log($"[Database] DatabaseRef es null: {databaseRef == null}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Database] Error al inicializar: {ex.Message}");
                Debug.LogError($"[Database] Stack trace: {ex.StackTrace}");
            }
        }

        #region Player Data

        /// <summary>
        /// Guarda o actualiza los datos del jugador
        /// </summary>
        public async Task SavePlayerData(PlayerData playerData)
        {
            try
            {
                Debug.Log($"[Database] Guardando datos del jugador: {playerData.userId}");

                if (databaseRef == null)
                {
                    Debug.LogError("[Database] DatabaseRef es null! Firebase no está inicializado correctamente");
                    return;
                }

                string json = JsonUtility.ToJson(playerData);
                Debug.Log($"[Database] JSON a guardar: {json.Substring(0, Mathf.Min(200, json.Length))}...");

                await databaseRef.Child("players").Child(playerData.userId).SetRawJsonValueAsync(json);

                Debug.Log("[Database] Datos del jugador guardados exitosamente en Firebase");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Database] Error al guardar datos del jugador: {ex.Message}");
                Debug.LogError($"[Database] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Carga los datos del jugador
        /// </summary>
        public async Task<PlayerData> LoadPlayerData(string userId)
        {
            try
            {
                Debug.Log($"[Database] Cargando datos del jugador: {userId}");

                if (databaseRef == null)
                {
                    Debug.LogError("[Database] DatabaseRef es null! Firebase no está inicializado correctamente");
                    return null;
                }

                var snapshot = await databaseRef.Child("players").Child(userId).GetValueAsync();

                if (snapshot.Exists)
                {
                    string json = snapshot.GetRawJsonValue();
                    Debug.Log($"[Database] JSON cargado: {json.Substring(0, Mathf.Min(200, json.Length))}...");
                    PlayerData data = JsonUtility.FromJson<PlayerData>(json);
                    Debug.Log($"[Database] Datos del jugador cargados exitosamente. Scores: {data.scoreHistory.Count}");
                    return data;
                }
                else
                {
                    Debug.LogWarning($"[Database] No se encontraron datos para el usuario: {userId}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Database] Error al cargar datos del jugador: {ex.Message}");
                Debug.LogError($"[Database] Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// Actualiza el balance de monedas del jugador
        /// </summary>
        public async Task UpdatePlayerCoins(string userId, int coins, int gems)
        {
            try
            {
                
                Dictionary<string, object> updates = new Dictionary<string, object>
                {
                    { "coins", coins },
                    { "gems", gems }
                };

                await databaseRef.Child("players").Child(userId).UpdateChildrenAsync(updates);
                

                await Task.Delay(50);
                Debug.Log($"[Database] Monedas actualizadas: {coins} coins, {gems} gems");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Database] Error al actualizar monedas: {ex.Message}");
            }
        }

        #endregion

        #region Leaderboards

        /// <summary>
        /// Guarda una nueva puntuación en el leaderboard global
        /// SOLO actualiza si es el mejor tiempo del jugador
        /// </summary>
        public async Task SaveScore(string userId, string username, float time, string countryCode)
        {
            try
            {
                Debug.Log($"[Database] Verificando score: {username} - {time}s en país {countryCode}");

                if (databaseRef == null)
                {
                    Debug.LogError("[Database] DatabaseRef es null! No se puede guardar el score");
                    return;
                }

                var globalSnapshot = await databaseRef.Child("leaderboards").Child("global").Child(userId).GetValueAsync();

                bool shouldUpdate = false;

                if (!globalSnapshot.Exists)
                {
                    // No existe, guardar directamente
                    shouldUpdate = true;
                    Debug.Log("[Database] Primer score del usuario, guardando...");
                }
                else
                {
                    // Existe, verificar si el nuevo tiempo es mejor (menor)
                    float existingTime = float.Parse(globalSnapshot.Child("time").Value.ToString());
                    if (time < existingTime)
                    {
                        shouldUpdate = true;
                        Debug.Log($"[Database] Nuevo récord! {time}s < {existingTime}s, actualizando...");
                    }
                    else
                    {
                        Debug.Log($"[Database] Tiempo no mejoró ({time}s >= {existingTime}s), no se actualiza leaderboard");
                    }
                }

                if (shouldUpdate)
                {
                    var scoreData = new Dictionary<string, object>
                    {
                        { "userId", userId },
                        { "username", username },
                        { "time", time },
                        { "countryCode", countryCode },
                        { "timestamp", ServerValue.Timestamp }
                    };

                    Debug.Log($"[Database] Guardando score en Firebase...");

                    // Guardar en leaderboard global
                    await databaseRef.Child("leaderboards").Child("global").Child(userId).SetValueAsync(scoreData);
                    Debug.Log($"[Database] Score guardado en leaderboard global");

                    // Guardar en leaderboard por país
                    await databaseRef.Child("leaderboards").Child($"country_{countryCode}").Child(userId).SetValueAsync(scoreData);
                    Debug.Log($"[Database] Score guardado en leaderboard de país: {countryCode}");

                    Debug.Log("[Database] Score actualizado en leaderboards (global y local)");
                }

                Debug.Log("[Database] Score procesado completamente");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Database] Error al guardar score: {ex.Message}");
                Debug.LogError($"[Database] Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Obtiene el leaderboard global (top N jugadores)
        /// </summary>
        public async Task<List<LeaderboardEntry>> GetGlobalLeaderboard(int topCount = 200)
        {
            try
            {
                Debug.Log($"[Database] Obteniendo top {topCount} global...");

                if (databaseRef == null)
                {
                    Debug.LogError("[Database] DatabaseRef es null! No se puede obtener leaderboard global");
                    return new List<LeaderboardEntry>();
                }

                var snapshot = await databaseRef.Child("leaderboards").Child("global")
                    .OrderByChild("time")
                    .LimitToFirst(topCount)
                    .GetValueAsync();

                List<LeaderboardEntry> entries = new List<LeaderboardEntry>();

                if (snapshot.Exists)
                {
                    Debug.Log($"[Database] Snapshot existe, procesando {snapshot.ChildrenCount} entradas");

                    foreach (var child in snapshot.Children)
                    {
                        var entry = new LeaderboardEntry
                        {
                            userId = child.Child("userId").Value.ToString(),
                            username = child.Child("username").Value.ToString(),
                            time = float.Parse(child.Child("time").Value.ToString()),
                            countryCode = child.Child("countryCode").Value.ToString()
                        };
                        entries.Add(entry);
                    }

                    Debug.Log($"[Database] Leaderboard global cargado: {entries.Count} entradas");
                }
                else
                {
                    Debug.LogWarning("[Database] No hay datos en el leaderboard global");
                }

                return entries;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Database] Error al obtener leaderboard global: {ex.Message}");
                Debug.LogError($"[Database] Stack trace: {ex.StackTrace}");
                return new List<LeaderboardEntry>();
            }
        }

        /// <summary>
        /// Obtiene el leaderboard de un país específico
        /// </summary>
        public async Task<List<LeaderboardEntry>> GetCountryLeaderboard(string countryCode, int topCount = 100)
        {
            try
            {
                Debug.Log($"[Database] Obteniendo top {topCount} de {countryCode}...");

                if (databaseRef == null)
                {
                    Debug.LogError("[Database] DatabaseRef es null! No se puede obtener leaderboard de país");
                    return new List<LeaderboardEntry>();
                }

                var snapshot = await databaseRef.Child("leaderboards").Child($"country_{countryCode}")
                    .OrderByChild("time")
                    .LimitToFirst(topCount)
                    .GetValueAsync();

                List<LeaderboardEntry> entries = new List<LeaderboardEntry>();

                if (snapshot.Exists)
                {
                    Debug.Log($"[Database] Snapshot existe, procesando {snapshot.ChildrenCount} entradas de {countryCode}");

                    foreach (var child in snapshot.Children)
                    {
                        var entry = new LeaderboardEntry
                        {
                            userId = child.Child("userId").Value.ToString(),
                            username = child.Child("username").Value.ToString(),
                            time = float.Parse(child.Child("time").Value.ToString()),
                            countryCode = child.Child("countryCode").Value.ToString()
                        };
                        entries.Add(entry);
                    }

                    Debug.Log($"[Database] Leaderboard de {countryCode} cargado: {entries.Count} entradas");
                }
                else
                {
                    Debug.LogWarning($"[Database] No hay datos en el leaderboard de {countryCode}");
                }

                return entries;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Database] Error al obtener leaderboard de país: {ex.Message}");
                Debug.LogError($"[Database] Stack trace: {ex.StackTrace}");
                return new List<LeaderboardEntry>();
            }
        }

        #endregion

        #region Tournaments

        /// <summary>
        /// Crea un nuevo torneo
        /// </summary>
        public async Task<bool> CreateTournament(TournamentData tournament)
        {
            try
            {
                Debug.Log($"[Database] Creando torneo: {tournament.name}");

             
                string json = JsonUtility.ToJson(tournament);
                await databaseRef.Child("tournaments").Child(tournament.tournamentId).SetRawJsonValueAsync(json);
                

                await Task.Delay(100);
                Debug.Log("[Database] Torneo creado exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Database] Error al crear torneo: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Obtiene un torneo por ID
        /// </summary>
        public async Task<TournamentData> GetTournament(string tournamentId)
        {
            try
            {
                Debug.Log($"[Database] Obteniendo torneo: {tournamentId}");

               
                var snapshot = await databaseRef.Child("tournaments").Child(tournamentId).GetValueAsync();

                if (snapshot.Exists)
                {
                    string json = snapshot.GetRawJsonValue();
                    return JsonUtility.FromJson<TournamentData>(json);
                }
                

                await Task.Delay(100);
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Database] Error al obtener torneo: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Obtiene todos los torneos activos
        /// </summary>
        public async Task<List<TournamentData>> GetActiveTournaments()
        {
            try
            {
                Debug.Log("[Database] Obteniendo torneos activos...");

                
                var snapshot = await databaseRef.Child("tournaments")
                    .OrderByChild("status")
                    .EqualTo("Active")
                    .GetValueAsync();

                List<TournamentData> tournaments = new List<TournamentData>();

                if (snapshot.Exists)
                {
                    foreach (var child in snapshot.Children)
                    {
                        string json = child.GetRawJsonValue();
                        tournaments.Add(JsonUtility.FromJson<TournamentData>(json));
                    }
                }

                return tournaments;
                

                await Task.Delay(200);
                return new List<TournamentData>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Database] Error al obtener torneos activos: {ex.Message}");
                return new List<TournamentData>();
            }
        }

        /// <summary>
        /// Actualiza el torneo en la base de datos
        /// </summary>
        public async Task UpdateTournament(TournamentData tournament)
        {
            try
            {
                
                string json = JsonUtility.ToJson(tournament);
                await databaseRef.Child("tournaments").Child(tournament.tournamentId).SetRawJsonValueAsync(json);
                

                await Task.Delay(100);
                Debug.Log($"[Database] Torneo actualizado: {tournament.tournamentId}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Database] Error al actualizar torneo: {ex.Message}");
            }
        }

        /// <summary>
        /// Añade un participante a un torneo
        /// </summary>
        public async Task<bool> JoinTournament(string tournamentId, string userId)
        {
            try
            {
                Debug.Log($"[Database] Usuario {userId} uniéndose al torneo {tournamentId}");


                var tournamentData = await GetTournament(tournamentId);
                if (tournamentData != null)
                {
                    var playerData = await LoadPlayerData(userId);
                    if (tournamentData.AddParticipant(playerData))
                    {
                        await UpdateTournament(tournamentData);

                        // Deducir entrada del balance del jugador
                        playerData.coins -= tournamentData.entryFee;
                        await SavePlayerData(playerData);

                        return true;
                    }
                }
                

                await Task.Delay(100);
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Database] Error al unirse al torneo: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Actualiza el username en todos los leaderboards donde el usuario tenga registros
        /// Llamar cuando el usuario cambia su nombre
        /// </summary>
        public async Task UpdateUsernameInLeaderboards(string userId, string newUsername, string countryCode)
        {
            try
            {
                Debug.Log($"[Database] Actualizando username en leaderboards para userId: {userId} → {newUsername}");

                if (databaseRef == null)
                {
                    Debug.LogError("[Database] DatabaseRef es null! No se puede actualizar username en leaderboards");
                    return;
                }

                // Actualizar en leaderboard global
                var globalRef = databaseRef.Child("leaderboards").Child("global").Child(userId);
                var globalSnapshot = await globalRef.GetValueAsync();

                if (globalSnapshot.Exists)
                {
                    await globalRef.Child("username").SetValueAsync(newUsername);
                    Debug.Log($"[Database] ✅ Username actualizado en leaderboard global");
                }
                else
                {
                    Debug.Log($"[Database] ℹ️ Usuario no tiene registro en leaderboard global");
                }

                // Actualizar en leaderboard de país
                var countryRef = databaseRef.Child("leaderboards").Child($"country_{countryCode}").Child(userId);
                var countrySnapshot = await countryRef.GetValueAsync();

                if (countrySnapshot.Exists)
                {
                    await countryRef.Child("username").SetValueAsync(newUsername);
                    Debug.Log($"[Database] ✅ Username actualizado en leaderboard de país: {countryCode}");
                }
                else
                {
                    Debug.Log($"[Database] ℹ️ Usuario no tiene registro en leaderboard de país: {countryCode}");
                }

                Debug.Log($"[Database] ✅ Username actualizado exitosamente en todos los leaderboards");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Database] Error al actualizar username en leaderboards: {ex.Message}");
                Debug.LogError($"[Database] Stack trace: {ex.StackTrace}");
            }
        }

        #endregion

        #region Analytics

        /// <summary>
        /// Registra un evento de juego
        /// </summary>
        public void LogGameEvent(string eventName, Dictionary<string, object> parameters)
        {
            try
            {
                Debug.Log($"[Database] Evento: {eventName}");

                // NOTA: Requiere Firebase Analytics
                // Descomentar cuando Firebase Analytics esté configurado
                /*
                var paramArray = new Firebase.Analytics.Parameter[parameters.Count];
                int i = 0;
                foreach (var kvp in parameters)
                {
                    paramArray[i] = new Firebase.Analytics.Parameter(kvp.Key, kvp.Value.ToString());
                    i++;
                }
                Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, paramArray);
                */
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Database] Error al registrar evento: {ex.Message}");
            }
        }

        #endregion
    }

    /// <summary>
    /// Entrada del leaderboard
    /// </summary>
    [System.Serializable]
    public class LeaderboardEntry
    {
        public string userId;
        public string username;
        public float time;
        public string countryCode;
        public string avatarUrl;
        public int position;
    }
}
