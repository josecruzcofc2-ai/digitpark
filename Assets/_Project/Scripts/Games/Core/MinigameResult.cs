using System;

namespace DigitPark.Games
{
    /// <summary>
    /// Resultado estandarizado de cualquier minijuego
    /// Todos los juegos deben retornar este resultado al finalizar
    /// </summary>
    [Serializable]
    public class MinigameResult
    {
        /// <summary>
        /// Tipo de juego que genero este resultado
        /// </summary>
        public GameType GameType { get; set; }

        /// <summary>
        /// Tiempo total en segundos (menor = mejor)
        /// </summary>
        public float TotalTime { get; set; }

        /// <summary>
        /// Numero de errores cometidos
        /// </summary>
        public int Errors { get; set; }

        /// <summary>
        /// Tiempo de penalizacion por errores (en segundos)
        /// </summary>
        public float PenaltyTime { get; set; }

        /// <summary>
        /// Puntuacion final (TotalTime + PenaltyTime)
        /// En juegos donde menor tiempo = mejor, este es el score a comparar
        /// </summary>
        public float FinalScore => TotalTime + PenaltyTime;

        /// <summary>
        /// Si el jugador completo el juego exitosamente
        /// </summary>
        public bool Completed { get; set; }

        /// <summary>
        /// Timestamp de cuando se completo el juego
        /// </summary>
        public DateTime CompletedAt { get; set; }

        /// <summary>
        /// ID del jugador que obtuvo este resultado
        /// </summary>
        public string PlayerId { get; set; }

        /// <summary>
        /// Datos adicionales especificos del juego (JSON)
        /// Ejemplo: para FlashTap, tiempos de reaccion individuales
        /// </summary>
        public string ExtraData { get; set; }

        public MinigameResult()
        {
            CompletedAt = DateTime.UtcNow;
            Completed = false;
        }

        /// <summary>
        /// Compara dos resultados. Retorna true si este resultado es mejor que el otro
        /// </summary>
        public bool IsBetterThan(MinigameResult other)
        {
            if (other == null) return true;
            if (!Completed && other.Completed) return false;
            if (Completed && !other.Completed) return true;

            // Menor score = mejor
            return FinalScore < other.FinalScore;
        }

        public override string ToString()
        {
            return $"[{GameType}] Time: {TotalTime:F2}s, Errors: {Errors}, Final: {FinalScore:F2}s";
        }
    }
}
