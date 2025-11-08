using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using DigitPark.Data;

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
        private void InitializeDatabase()
        {
            Debug.Log("[Database] Inicializando Firebase Database...");

            try
            {
                // Obtener la app de Firebase y configurar la Database URL
                // Usar global:: para evitar conflicto con namespace local
                global::Firebase.FirebaseApp app = global::Firebase.FirebaseApp.DefaultInstance;

                if (app.Options.DatabaseUrl != null)
                {
                    app.Options.DatabaseUrl = new System.Uri(DATABASE_URL);
                }

                // Obtener la instancia con la URL específica
                databaseRef = FirebaseDatabase.GetInstance(app, DATABASE_URL).RootReference;
                firestore = FirebaseFirestore.DefaultInstance;

                Debug.Log($"[Database] Firebase Database inicializado con URL: {DATABASE_URL}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Database] Error al inicializar: {ex.Message}");
                Debug.LogWarning("[Database] Usando modo simulado sin Firebase");
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


                string json = JsonUtility.ToJson(playerData);
                await databaseRef.Child("players").Child(playerData.userId).SetRawJsonValueAsync(json);
                

                await Task.Delay(100); // Simulación
                Debug.Log("[Database] Datos del jugador guardados");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Database] Error al guardar datos del jugador: {ex.Message}");
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

             
                var snapshot = await databaseRef.Child("players").Child(userId).GetValueAsync();

                if (snapshot.Exists)
                {
                    string json = snapshot.GetRawJsonValue();
                    PlayerData data = JsonUtility.FromJson<PlayerData>(json);
                    Debug.Log("[Database] Datos del jugador cargados");
                    return data;
                }
                

                await Task.Delay(100); // Simulación
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Database] Error al cargar datos del jugador: {ex.Message}");
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
                Debug.Log($"[Database] Verificando score: {username} - {time}s");

                
                
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

                    // Guardar en leaderboard global
                    await databaseRef.Child("leaderboards").Child("global").Child(userId).SetValueAsync(scoreData);

                    // Guardar en leaderboard por país
                    await databaseRef.Child("leaderboards").Child($"country_{countryCode}").Child(userId).SetValueAsync(scoreData);

                    Debug.Log("[Database] Score actualizado en leaderboards (global y local)");
                }
                

                await Task.Delay(100);
                Debug.Log("[Database] Score procesado");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Database] Error al guardar score: {ex.Message}");
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

               
                var snapshot = await databaseRef.Child("leaderboards").Child("global")
                    .OrderByChild("time")
                    .LimitToFirst(topCount)
                    .GetValueAsync();

                List<LeaderboardEntry> entries = new List<LeaderboardEntry>();

                if (snapshot.Exists)
                {
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
                }

                return entries;
                

                await Task.Delay(200);
                return new List<LeaderboardEntry>(); // Simulación
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Database] Error al obtener leaderboard global: {ex.Message}");
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

                
                var snapshot = await databaseRef.Child("leaderboards").Child($"country_{countryCode}")
                    .OrderByChild("time")
                    .LimitToFirst(topCount)
                    .GetValueAsync();

                List<LeaderboardEntry> entries = new List<LeaderboardEntry>();

                if (snapshot.Exists)
                {
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
                }

                return entries;
                

                await Task.Delay(200);
                return new List<LeaderboardEntry>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Database] Error al obtener leaderboard de país: {ex.Message}");
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
