using System;

namespace DigitPark.Games
{
    /// <summary>
    /// Interfaz que todos los minijuegos deben implementar
    /// Garantiza un contrato comun para el GameSessionManager
    /// </summary>
    public interface IMinigame
    {
        /// <summary>
        /// Tipo de este minijuego
        /// </summary>
        GameType GameType { get; }

        /// <summary>
        /// Si el juego esta actualmente en progreso
        /// </summary>
        bool IsPlaying { get; }

        /// <summary>
        /// Inicializa el juego con la configuracion dada
        /// </summary>
        void Initialize(MinigameConfig config);

        /// <summary>
        /// Inicia una nueva partida
        /// </summary>
        void StartGame();

        /// <summary>
        /// Pausa el juego (si aplica)
        /// </summary>
        void PauseGame();

        /// <summary>
        /// Reanuda el juego pausado
        /// </summary>
        void ResumeGame();

        /// <summary>
        /// Termina el juego forzosamente
        /// </summary>
        void EndGame();

        /// <summary>
        /// Reinicia el juego para una nueva partida
        /// </summary>
        void ResetGame();

        /// <summary>
        /// Obtiene el resultado actual/final del juego
        /// </summary>
        MinigameResult GetResult();

        /// <summary>
        /// Evento disparado cuando el juego termina
        /// </summary>
        event Action<MinigameResult> OnGameCompleted;

        /// <summary>
        /// Evento disparado cuando hay un error
        /// </summary>
        event Action<int> OnError; // int = numero total de errores
    }
}
