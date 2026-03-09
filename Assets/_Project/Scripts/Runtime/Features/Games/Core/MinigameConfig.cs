using UnityEngine;

namespace DigitPark.Games
{
    /// <summary>
    /// ScriptableObject con configuracion de cada minijuego
    /// Permite ajustar dificultad y parametros sin tocar codigo
    /// </summary>
    [CreateAssetMenu(fileName = "MinigameConfig", menuName = "DigitPark/Minigame Config")]
    public class MinigameConfig : ScriptableObject
    {
        [Header("Identificacion")]
        public GameType gameType;
        public string displayName;
        [TextArea(2, 4)]
        public string description;
        public Sprite icon;

        [Header("Tiempo")]
        [Tooltip("Tiempo limite en segundos (0 = sin limite)")]
        public float timeLimit = 0f;

        [Tooltip("Penalizacion por error en segundos")]
        public float errorPenalty = 2f;

        [Header("Dificultad")]
        [Tooltip("Nivel de dificultad base (1-5)")]
        [Range(1, 5)]
        public int difficultyLevel = 1;

        [Header("Configuracion Especifica")]
        [Tooltip("Tamano del grid (para juegos con cuadricula)")]
        public Vector2Int gridSize = new Vector2Int(3, 3);

        [Tooltip("Numero de rondas/intentos")]
        public int rounds = 1;

        [Tooltip("Tiempo de espera antes de iniciar (countdown)")]
        public float countdownTime = 3f;

        [Header("Audio")]
        public AudioClip successSound;
        public AudioClip errorSound;
        public AudioClip completeSound;

        /// <summary>
        /// Obtiene el numero total de elementos segun el tamano del grid
        /// </summary>
        public int TotalGridElements => gridSize.x * gridSize.y;
    }
}
