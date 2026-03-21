using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitPark.Data
{
    /// <summary>
    /// Datos completos del jugador, sincronizados con Firebase
    /// </summary>
    [System.Serializable]
    public class PlayerData
    {
        public string userId;
        public string username;
        public string email;
        public string avatarUrl;
        public string countryCode;

        // Estadisticas generales
        public float bestTime;
        public float averageTime;
        public int totalGamesPlayed;
        public int totalGamesWon;

        // Estadisticas por juego
        public GameStats digitRushStats;
        public GameStats memoryPairsStats;
        public GameStats quickMathStats;
        public GameStats flashTapStats;
        public GameStats oddOneOutStats;

        // Historial de tiempos (ultimos 30)
        public List<ScoreEntry> scoreHistory;

        // Torneos participados (List for JsonUtility compatibility — Dictionary not serializable)
        public List<TournamentResultEntry> tournamentResults;

        // Configuraciones del jugador
        public PlayerSettings settings;

        // Lista de amigos
        public List<string> friends;

        // Estado premium
        public bool isPremium;
        // ISO 8601 strings for JsonUtility compatibility (DateTime serializes as {})
        public string premiumExpiryDateISO;

        // Ultima sesion
        public string lastLoginDateISO;
        public string createdDateISO;

        // Helper properties for DateTime access
        public DateTime premiumExpiryDate
        {
            get => DateTime.TryParse(premiumExpiryDateISO, null, System.Globalization.DateTimeStyles.RoundtripKind, out var d) ? d : DateTime.MinValue;
            set => premiumExpiryDateISO = value.ToString("o");
        }
        public DateTime lastLoginDate
        {
            get => DateTime.TryParse(lastLoginDateISO, null, System.Globalization.DateTimeStyles.RoundtripKind, out var d) ? d : DateTime.Now;
            set => lastLoginDateISO = value.ToString("o");
        }
        public DateTime createdDate
        {
            get => DateTime.TryParse(createdDateISO, null, System.Globalization.DateTimeStyles.RoundtripKind, out var d) ? d : DateTime.Now;
            set => createdDateISO = value.ToString("o");
        }

        public PlayerData()
        {
            userId = "";
            username = "Player";
            email = "";
            avatarUrl = "";
            countryCode = "US";

            bestTime = -1f; // Sentinel: no score yet (float.MaxValue corrupts Firebase queries)
            averageTime = 0f;
            totalGamesPlayed = 0;
            totalGamesWon = 0;

            // Inicializar stats por juego
            digitRushStats = new GameStats();
            memoryPairsStats = new GameStats();
            quickMathStats = new GameStats();
            flashTapStats = new GameStats();
            oddOneOutStats = new GameStats();

            scoreHistory = new List<ScoreEntry>();
            tournamentResults = new List<TournamentResultEntry>();
            settings = new PlayerSettings();
            friends = new List<string>();

            isPremium = false;
            premiumExpiryDate = DateTime.MinValue;

            lastLoginDate = DateTime.Now;
            createdDate = DateTime.Now;
        }

        /// <summary>
        /// Obtiene las estadisticas de un juego especifico
        /// </summary>
        public GameStats GetGameStats(string gameType)
        {
            return gameType switch
            {
                "DigitRush" => digitRushStats,
                "MemoryPairs" => memoryPairsStats,
                "QuickMath" => quickMathStats,
                "FlashTap" => flashTapStats,
                "OddOneOut" => oddOneOutStats,
                _ => null
            };
        }

        /// <summary>
        /// Verifica si un jugador es amigo
        /// </summary>
        public bool IsFriend(string playerId)
        {
            return friends != null && friends.Contains(playerId);
        }

        /// <summary>
        /// Añade una nueva puntuación al historial
        /// </summary>
        public void AddScore(float time)
        {
            var entry = new ScoreEntry
            {
                time = time
            };
            entry.SetTimestamp(DateTime.Now);

            scoreHistory.Add(entry);

            // Mantener solo las últimas 30 puntuaciones
            if (scoreHistory.Count > 30)
            {
                scoreHistory.RemoveAt(0);
            }

            // Actualizar mejor tiempo
            if (time < bestTime)
            {
                bestTime = time;
            }

            // Actualizar promedio
            UpdateAverageTime();
        }

        /// <summary>
        /// Actualiza el tiempo promedio del jugador
        /// </summary>
        private void UpdateAverageTime()
        {
            if (scoreHistory.Count == 0) return;

            float total = 0f;
            foreach (var score in scoreHistory)
            {
                total += score.time;
            }

            averageTime = total / scoreHistory.Count;
        }

        /// <summary>
        /// Verifica si el jugador es premium activo
        /// </summary>
        public bool IsPremiumActive()
        {
            return isPremium && DateTime.Now < premiumExpiryDate;
        }

        /// <summary>
        /// Convierte a Dictionary para Firebase
        /// </summary>
        public Dictionary<string, object> ToDictionary()
        {
            // S15-NEW-03: email omitido — Firebase Auth ya lo tiene; no guardarlo en RTDB
            return new Dictionary<string, object>
            {
                { "userId", userId },
                { "username", username },
                { "avatarUrl", avatarUrl },
                { "countryCode", countryCode },
                { "bestTime", bestTime },
                { "averageTime", averageTime },
                { "totalGamesPlayed", totalGamesPlayed },
                { "totalGamesWon", totalGamesWon },
                { "isPremium", isPremium },
                { "premiumExpiryDateISO", premiumExpiryDateISO ?? "" },
                { "lastLoginDateISO", lastLoginDateISO ?? "" },
                { "createdDateISO", createdDateISO ?? "" },
                // Per-game stats (P2-DATA-03: were missing from ToDictionary)
                { "digitRushStats", JsonUtility.ToJson(digitRushStats ?? new GameStats()) },
                { "memoryPairsStats", JsonUtility.ToJson(memoryPairsStats ?? new GameStats()) },
                { "quickMathStats", JsonUtility.ToJson(quickMathStats ?? new GameStats()) },
                { "flashTapStats", JsonUtility.ToJson(flashTapStats ?? new GameStats()) },
                { "oddOneOutStats", JsonUtility.ToJson(oddOneOutStats ?? new GameStats()) },
            };
        }

        /// <summary>
        /// Calcula el win rate general
        /// </summary>
        public float GetWinRate()
        {
            if (totalGamesPlayed == 0) return 0f;
            return (float)totalGamesWon / totalGamesPlayed * 100f;
        }
    }

    /// <summary>
    /// Estadisticas de un juego especifico
    /// </summary>
    [System.Serializable]
    public class GameStats
    {
        public int gamesPlayed;
        public int gamesWon;
        public float bestTime;
        public float averageTime;

        public GameStats()
        {
            gamesPlayed = 0;
            gamesWon = 0;
            bestTime = -1f;
            averageTime = 0f;
        }

        /// <summary>
        /// Calcula el porcentaje de victorias
        /// </summary>
        public float GetWinRate()
        {
            if (gamesPlayed == 0) return 0f;
            return (float)gamesWon / gamesPlayed * 100f;
        }

        /// <summary>
        /// Obtiene el mejor tiempo formateado
        /// </summary>
        public string GetBestTimeFormatted()
        {
            if (bestTime < 0) return "--";
            return $"{bestTime:F2}s";
        }
    }

    /// <summary>
    /// Entrada individual de puntuación
    /// </summary>
    [System.Serializable]
    public class ScoreEntry
    {
        public float time;
        public string timestamp; // Almacenado como string para serialización JSON
        public string tournamentId; // null si es partida casual

        // Propiedad helper para obtener DateTime
        public DateTime GetTimestamp()
        {
            if (DateTime.TryParse(timestamp, out DateTime result))
                return result;
            return DateTime.Now;
        }

        public void SetTimestamp(DateTime value)
        {
            timestamp = value.ToString("o"); // ISO 8601 format
        }
    }

    /// <summary>
    /// Resultado de un torneo específico
    /// </summary>
    [System.Serializable]
    public class TournamentResult
    {
        public string tournamentId;
        public string tournamentName;
        public float bestTime;
        public int finalPosition;
        public int totalParticipants;
        public int prizeWon;
        public string completionDate;
    }

    /// <summary>
    /// JsonUtility-compatible wrapper for tournament results (replaces Dictionary&lt;string, TournamentResult&gt;)
    /// </summary>
    [System.Serializable]
    public class TournamentResultEntry
    {
        public string key;
        public TournamentResult value;
    }
}
