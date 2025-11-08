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

        // Progresión
        public int level;
        public int experience;
        public int experienceToNextLevel;

        // Moneda del juego
        public int coins;
        public int gems;

        // Estadísticas de juego
        public float bestTime;
        public float averageTime;
        public int totalGamesPlayed;
        public int totalGamesWon;

        // Historial de mejores tiempos (últimos 50)
        public List<ScoreEntry> scoreHistory;

        // Torneos participados
        public Dictionary<string, TournamentResult> tournaments;

        // Configuraciones del jugador
        public PlayerSettings settings;

        // Logros desbloqueados
        public List<string> unlockedAchievements;

        // Estado premium
        public bool isPremium;
        public DateTime premiumExpiryDate;

        // Última sesión
        public DateTime lastLoginDate;
        public DateTime createdDate;

        // Daily rewards
        public int consecutiveLoginDays;
        public DateTime lastDailyRewardClaimed;

        public PlayerData()
        {
            userId = "";
            username = "Player";
            email = "";
            avatarUrl = "";
            countryCode = "US";

            level = 1;
            experience = 0;
            experienceToNextLevel = 100;

            coins = 0;
            gems = 0;

            bestTime = float.MaxValue;
            averageTime = 0f;
            totalGamesPlayed = 0;
            totalGamesWon = 0;

            scoreHistory = new List<ScoreEntry>();
            tournaments = new Dictionary<string, TournamentResult>();
            settings = new PlayerSettings();
            unlockedAchievements = new List<string>();

            isPremium = false;
            premiumExpiryDate = DateTime.MinValue;

            lastLoginDate = DateTime.Now;
            createdDate = DateTime.Now;

            consecutiveLoginDays = 0;
            lastDailyRewardClaimed = DateTime.MinValue;
        }

        /// <summary>
        /// Añade experiencia y verifica si sube de nivel
        /// </summary>
        public bool AddExperience(int amount)
        {
            experience += amount;

            if (experience >= experienceToNextLevel)
            {
                LevelUp();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sube el nivel del jugador
        /// </summary>
        private void LevelUp()
        {
            level++;
            experience -= experienceToNextLevel;
            experienceToNextLevel = CalculateNextLevelXP();

            // Recompensas por subir de nivel
            coins += level * 10;
            gems += level;
        }

        /// <summary>
        /// Calcula la XP necesaria para el siguiente nivel
        /// </summary>
        private int CalculateNextLevelXP()
        {
            return Mathf.RoundToInt(100 * Mathf.Pow(level, 1.5f));
        }

        /// <summary>
        /// Añade una nueva puntuación al historial
        /// </summary>
        public void AddScore(float time)
        {
            scoreHistory.Add(new ScoreEntry
            {
                time = time,
                timestamp = DateTime.Now
            });

            // Mantener solo las últimas 50 puntuaciones
            if (scoreHistory.Count > 50)
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
            return new Dictionary<string, object>
            {
                { "userId", userId },
                { "username", username },
                { "email", email },
                { "avatarUrl", avatarUrl },
                { "countryCode", countryCode },
                { "level", level },
                { "experience", experience },
                { "experienceToNextLevel", experienceToNextLevel },
                { "coins", coins },
                { "gems", gems },
                { "bestTime", bestTime },
                { "averageTime", averageTime },
                { "totalGamesPlayed", totalGamesPlayed },
                { "totalGamesWon", totalGamesWon },
                { "isPremium", isPremium },
                { "premiumExpiryDate", premiumExpiryDate.ToString() },
                { "lastLoginDate", lastLoginDate.ToString() },
                { "createdDate", createdDate.ToString() },
                { "consecutiveLoginDays", consecutiveLoginDays },
                { "lastDailyRewardClaimed", lastDailyRewardClaimed.ToString() }
            };
        }
    }

    /// <summary>
    /// Entrada individual de puntuación
    /// </summary>
    [System.Serializable]
    public class ScoreEntry
    {
        public float time;
        public DateTime timestamp;
        public string tournamentId; // null si es partida casual
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
        public DateTime completionDate;
    }
}
