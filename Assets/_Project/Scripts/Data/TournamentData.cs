using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitPark.Data
{
    /// <summary>
    /// Datos completos de un torneo
    /// </summary>
    [System.Serializable]
    public class TournamentData
    {
        public string tournamentId;
        public string name;
        public string creatorId;
        public string creatorName;

        // Tiempo
        public DateTime startTime;
        public DateTime endTime;
        public TournamentDuration duration;

        // Entrada y premios
        public int entryFee;
        public int totalPrizePool;
        public Dictionary<int, int> prizeDistribution; // Posición -> Cantidad

        // Participantes
        public List<ParticipantScore> participants;
        public int maxParticipants;
        public int currentParticipants;

        // Región
        public TournamentRegion region;
        public string countryCode; // Si es regional
        public string inviteCode; // Si es privado

        // Reglas
        public TournamentRules rules;

        // Estado
        public TournamentStatus status;

        // Categoría
        public string category;
        public string description;

        public TournamentData()
        {
            tournamentId = Guid.NewGuid().ToString();
            name = "New Tournament";
            creatorId = "";
            creatorName = "";

            startTime = DateTime.Now;
            endTime = DateTime.Now.AddHours(24);
            duration = TournamentDuration.OneDay;

            entryFee = 100;
            totalPrizePool = 0;
            prizeDistribution = new Dictionary<int, int>();

            participants = new List<ParticipantScore>();
            maxParticipants = 100;
            currentParticipants = 0;

            region = TournamentRegion.Global;
            countryCode = "";
            inviteCode = "";

            rules = new TournamentRules();
            status = TournamentStatus.Scheduled;

            category = "Standard";
            description = "";
        }

        /// <summary>
        /// Verifica si el torneo está activo
        /// </summary>
        public bool IsActive()
        {
            DateTime now = DateTime.Now;
            return status == TournamentStatus.Active && now >= startTime && now < endTime;
        }

        /// <summary>
        /// Verifica si el torneo está completo
        /// </summary>
        public bool IsFull()
        {
            return currentParticipants >= maxParticipants;
        }

        /// <summary>
        /// Verifica si un jugador puede unirse
        /// </summary>
        public bool CanJoin(PlayerData player)
        {
            if (IsFull()) return false;
            if (player.coins < entryFee) return false;
            if (status != TournamentStatus.Scheduled && status != TournamentStatus.Active) return false;
            if (IsParticipating(player.userId)) return false;

            return true;
        }

        /// <summary>
        /// Verifica si un jugador ya está participando
        /// </summary>
        public bool IsParticipating(string userId)
        {
            return participants.Exists(p => p.userId == userId);
        }

        /// <summary>
        /// Añade un participante al torneo
        /// </summary>
        public bool AddParticipant(PlayerData player)
        {
            if (!CanJoin(player)) return false;

            participants.Add(new ParticipantScore
            {
                userId = player.userId,
                username = player.username,
                avatarUrl = player.avatarUrl,
                countryCode = player.countryCode,
                bestTime = float.MaxValue,
                attempts = 0,
                joinedAt = DateTime.Now
            });

            currentParticipants++;
            totalPrizePool += entryFee;

            return true;
        }

        /// <summary>
        /// Actualiza la puntuación de un participante
        /// </summary>
        public void UpdateParticipantScore(string userId, float time)
        {
            var participant = participants.Find(p => p.userId == userId);
            if (participant != null)
            {
                participant.attempts++;

                if (time < participant.bestTime)
                {
                    participant.bestTime = time;
                    participant.lastAttemptTime = DateTime.Now;
                }

                // Reordenar leaderboard
                SortParticipants();
            }
        }

        /// <summary>
        /// Ordena los participantes por mejor tiempo
        /// </summary>
        public void SortParticipants()
        {
            participants.Sort((a, b) =>
            {
                if (a.bestTime == float.MaxValue && b.bestTime == float.MaxValue)
                    return a.joinedAt.CompareTo(b.joinedAt);
                if (a.bestTime == float.MaxValue) return 1;
                if (b.bestTime == float.MaxValue) return -1;
                return a.bestTime.CompareTo(b.bestTime);
            });
        }

        /// <summary>
        /// Calcula la distribución de premios
        /// </summary>
        public void CalculatePrizeDistribution()
        {
            prizeDistribution.Clear();

            int availablePrize = Mathf.RoundToInt(totalPrizePool * 0.5f); // Máx 50% del pool

            // Distribución por defecto Top 10
            if (currentParticipants >= 10)
            {
                prizeDistribution[1] = Mathf.RoundToInt(availablePrize * 0.25f); // 25%
                prizeDistribution[2] = Mathf.RoundToInt(availablePrize * 0.15f); // 15%
                prizeDistribution[3] = Mathf.RoundToInt(availablePrize * 0.10f); // 10%
                prizeDistribution[4] = Mathf.RoundToInt(availablePrize * 0.05f); // 5%
                prizeDistribution[5] = Mathf.RoundToInt(availablePrize * 0.05f); // 5%
            }
            else if (currentParticipants >= 3)
            {
                prizeDistribution[1] = Mathf.RoundToInt(availablePrize * 0.30f); // 30%
                prizeDistribution[2] = Mathf.RoundToInt(availablePrize * 0.15f); // 15%
                prizeDistribution[3] = Mathf.RoundToInt(availablePrize * 0.05f); // 5%
            }
        }

        /// <summary>
        /// Obtiene el tiempo restante del torneo
        /// </summary>
        public TimeSpan GetTimeRemaining()
        {
            if (status == TournamentStatus.Completed)
                return TimeSpan.Zero;

            return endTime - DateTime.Now;
        }

        /// <summary>
        /// Convierte a Dictionary para Firebase
        /// </summary>
        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                { "tournamentId", tournamentId },
                { "name", name },
                { "creatorId", creatorId },
                { "creatorName", creatorName },
                { "startTime", startTime.ToString() },
                { "endTime", endTime.ToString() },
                { "entryFee", entryFee },
                { "totalPrizePool", totalPrizePool },
                { "maxParticipants", maxParticipants },
                { "currentParticipants", currentParticipants },
                { "region", region.ToString() },
                { "countryCode", countryCode },
                { "inviteCode", inviteCode },
                { "status", status.ToString() },
                { "category", category },
                { "description", description }
            };
        }
    }

    /// <summary>
    /// Puntuación de un participante en un torneo
    /// </summary>
    [System.Serializable]
    public class ParticipantScore
    {
        public string userId;
        public string username;
        public string avatarUrl;
        public string countryCode;
        public float bestTime;
        public int attempts;
        public DateTime joinedAt;
        public DateTime lastAttemptTime;
    }

    /// <summary>
    /// Reglas especiales del torneo
    /// </summary>
    [System.Serializable]
    public class TournamentRules
    {
        public int maxAttempts; // -1 para ilimitado
        public bool allowPowerUps;
        public int gridSize; // 3 para 3x3, podría expandirse

        public TournamentRules()
        {
            maxAttempts = -1;
            allowPowerUps = false;
            gridSize = 3;
        }
    }

    public enum TournamentDuration
    {
        OneHour,
        SixHours,
        TwelveHours,
        OneDay,
        ThreeDays,
        OneWeek
    }

    public enum TournamentRegion
    {
        Global,
        Country,
        Private
    }

    public enum TournamentStatus
    {
        Scheduled,
        Active,
        Completed,
        Cancelled
    }
}
