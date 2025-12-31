using System;
using UnityEngine;

namespace DigitPark.Games
{
    /// <summary>
    /// Clase base abstracta para todos los minijuegos
    /// Implementa funcionalidad comun y obliga a implementar la logica especifica
    /// </summary>
    public abstract class MinigameBase : MonoBehaviour, IMinigame
    {
        [Header("Configuracion")]
        [SerializeField] protected MinigameConfig config;

        // Estado del juego
        protected bool isPlaying;
        protected bool isPaused;
        protected float currentTime;
        protected int errorCount;

        // Resultado
        protected MinigameResult currentResult;

        // Propiedades de IMinigame
        public abstract GameType GameType { get; }
        public bool IsPlaying => isPlaying;

        // Eventos
        public event Action<MinigameResult> OnGameCompleted;
        public event Action<int> OnError;

        protected virtual void Awake()
        {
            currentResult = new MinigameResult
            {
                GameType = GameType
            };
        }

        protected virtual void Start()
        {
            // Verificar si hay un contexto de sesion activo
            if (GameSessionManager.Instance.HasActiveSession)
            {
                var context = GameSessionManager.Instance.CurrentContext;
                Debug.Log($"Iniciando {GameType} en modo {context.Mode}");
            }

            Initialize(config);
        }

        protected virtual void Update()
        {
            if (isPlaying && !isPaused)
            {
                currentTime += Time.deltaTime;
                UpdateTimer();
            }
        }

        /// <summary>
        /// Inicializa el juego con la configuracion
        /// </summary>
        public virtual void Initialize(MinigameConfig config)
        {
            this.config = config;
            ResetGame();
        }

        /// <summary>
        /// Inicia el juego
        /// </summary>
        public virtual void StartGame()
        {
            isPlaying = true;
            isPaused = false;
            currentTime = 0f;
            errorCount = 0;

            OnGameStarted();
            Debug.Log($"{GameType} iniciado");
        }

        /// <summary>
        /// Pausa el juego
        /// </summary>
        public virtual void PauseGame()
        {
            if (!isPlaying) return;
            isPaused = true;
            OnGamePaused();
        }

        /// <summary>
        /// Reanuda el juego
        /// </summary>
        public virtual void ResumeGame()
        {
            if (!isPlaying) return;
            isPaused = false;
            OnGameResumed();
        }

        /// <summary>
        /// Termina el juego
        /// </summary>
        public virtual void EndGame()
        {
            isPlaying = false;
            isPaused = false;

            // Construir resultado
            currentResult.TotalTime = currentTime;
            currentResult.Errors = errorCount;
            currentResult.PenaltyTime = errorCount * (config?.errorPenalty ?? 2f);
            currentResult.Completed = true;
            currentResult.CompletedAt = DateTime.UtcNow;

            OnGameEnded();

            // Notificar
            OnGameCompleted?.Invoke(currentResult);

            // Registrar en sesion si hay una activa
            if (GameSessionManager.Instance.HasActiveSession)
            {
                GameSessionManager.Instance.RegisterGameResult(currentResult);
            }

            Debug.Log($"{GameType} terminado: {currentResult}");
        }

        /// <summary>
        /// Reinicia el juego
        /// </summary>
        public virtual void ResetGame()
        {
            isPlaying = false;
            isPaused = false;
            currentTime = 0f;
            errorCount = 0;

            currentResult = new MinigameResult
            {
                GameType = GameType
            };

            OnGameReset();
        }

        /// <summary>
        /// Obtiene el resultado actual
        /// </summary>
        public MinigameResult GetResult()
        {
            currentResult.TotalTime = currentTime;
            currentResult.Errors = errorCount;
            currentResult.PenaltyTime = errorCount * (config?.errorPenalty ?? 2f);
            return currentResult;
        }

        /// <summary>
        /// Registra un error
        /// </summary>
        protected virtual void RegisterError()
        {
            errorCount++;
            OnError?.Invoke(errorCount);
            OnErrorOccurred();
        }

        /// <summary>
        /// Obtiene el tiempo formateado para UI
        /// </summary>
        protected string GetFormattedTime()
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            int milliseconds = Mathf.FloorToInt((currentTime * 100f) % 100f);

            if (minutes > 0)
                return $"{minutes}:{seconds:00}.{milliseconds:00}";
            return $"{seconds}.{milliseconds:00}";
        }

        // Metodos abstractos que cada juego debe implementar
        protected abstract void OnGameStarted();
        protected abstract void OnGamePaused();
        protected abstract void OnGameResumed();
        protected abstract void OnGameEnded();
        protected abstract void OnGameReset();
        protected abstract void OnErrorOccurred();
        protected abstract void UpdateTimer();
    }
}
