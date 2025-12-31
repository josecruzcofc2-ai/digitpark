using System;
using System.Collections.Generic;

namespace DigitPark.Games
{
    /// <summary>
    /// Contexto de una sesion de juego
    /// Contiene toda la informacion necesaria para una partida/torneo/sprint
    /// </summary>
    [Serializable]
    public class GameContext
    {
        /// <summary>
        /// Modo de juego actual
        /// </summary>
        public GameMode Mode { get; set; }

        /// <summary>
        /// Lista de juegos a jugar (para Cognitive Sprint o torneo multi-juego)
        /// </summary>
        public List<GameType> Games { get; set; }

        /// <summary>
        /// Indice del juego actual en la lista
        /// </summary>
        public int CurrentGameIndex { get; set; }

        /// <summary>
        /// ID del oponente (para 1v1)
        /// </summary>
        public string OpponentId { get; set; }

        /// <summary>
        /// Nombre del oponente para mostrar en UI
        /// </summary>
        public string OpponentName { get; set; }

        /// <summary>
        /// Cuota de entrada
        /// </summary>
        public decimal EntryFee { get; set; }

        /// <summary>
        /// ID del torneo (si aplica)
        /// </summary>
        public string TournamentId { get; set; }

        /// <summary>
        /// ID de la partida/match
        /// </summary>
        public string MatchId { get; set; }

        /// <summary>
        /// Resultados de cada juego jugado en esta sesion
        /// </summary>
        public List<MinigameResult> Results { get; set; }

        /// <summary>
        /// Resultados del oponente (para comparacion)
        /// </summary>
        public List<MinigameResult> OpponentResults { get; set; }

        /// <summary>
        /// Timestamp de inicio de la sesion
        /// </summary>
        public DateTime StartedAt { get; set; }

        public GameContext()
        {
            Games = new List<GameType>();
            Results = new List<MinigameResult>();
            OpponentResults = new List<MinigameResult>();
            CurrentGameIndex = 0;
            StartedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Obtiene el juego actual a jugar
        /// </summary>
        public GameType? CurrentGame
        {
            get
            {
                if (Games == null || CurrentGameIndex >= Games.Count)
                    return null;
                return Games[CurrentGameIndex];
            }
        }

        /// <summary>
        /// Si hay mas juegos por jugar
        /// </summary>
        public bool HasMoreGames => Games != null && CurrentGameIndex < Games.Count - 1;

        /// <summary>
        /// Avanza al siguiente juego
        /// </summary>
        public bool MoveToNextGame()
        {
            if (!HasMoreGames) return false;
            CurrentGameIndex++;
            return true;
        }

        /// <summary>
        /// Agrega un resultado a la sesion
        /// </summary>
        public void AddResult(MinigameResult result)
        {
            Results ??= new List<MinigameResult>();
            Results.Add(result);
        }

        /// <summary>
        /// Calcula el ganador basado en los resultados
        /// Retorna: 1 = jugador local gano, -1 = oponente gano, 0 = empate
        /// </summary>
        public int CalculateWinner()
        {
            if (Results == null || OpponentResults == null) return 0;

            int playerWins = 0;
            int opponentWins = 0;

            for (int i = 0; i < Results.Count && i < OpponentResults.Count; i++)
            {
                if (Results[i].IsBetterThan(OpponentResults[i]))
                    playerWins++;
                else if (OpponentResults[i].IsBetterThan(Results[i]))
                    opponentWins++;
            }

            if (playerWins > opponentWins) return 1;
            if (opponentWins > playerWins) return -1;
            return 0;
        }

        /// <summary>
        /// Obtiene el nombre de escena para el juego actual
        /// </summary>
        public string GetCurrentSceneName()
        {
            if (CurrentGame == null) return null;
            return $"Games/{CurrentGame}";
        }
    }
}
