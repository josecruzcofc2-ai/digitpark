using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DigitPark.UI;
using DigitPark.Managers;

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

        [Header("Navigation (Base)")]
        [SerializeField] protected Button backButton;
        [SerializeField] protected Button playAgainButton;

        [Header("Win/Lose Panels")]
        [SerializeField] protected WinPanelController winPanelNormal;
        [SerializeField] protected WinPanelController losePanelNormal;
        [SerializeField] protected WinPanelController winPanelRealMoney;
        [SerializeField] protected WinPanelController losePanelRealMoney;

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
            // Setup navigation buttons
            SetupNavigationButtons();

            // Verificar si hay un contexto de sesion activo
            if (GameSessionManager.Instance != null && GameSessionManager.Instance.HasActiveSession)
            {
                var context = GameSessionManager.Instance.CurrentContext;
                Debug.Log($"Iniciando {GameType} en modo {context.Mode}");
            }

            Initialize(config);
        }

        /// <summary>
        /// Configura los botones de navegacion
        /// </summary>
        protected virtual void SetupNavigationButtons()
        {
            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);

            if (playAgainButton != null)
                playAgainButton.onClick.AddListener(OnPlayAgainClicked);
        }

        /// <summary>
        /// Vuelve al selector de juegos
        /// </summary>
        protected virtual void OnBackClicked()
        {
            Debug.Log($"[{GameType}] Volviendo a GameSelector");
            SceneManager.LoadScene("GameSelector");
        }

        /// <summary>
        /// Jugar de nuevo
        /// </summary>
        protected virtual void OnPlayAgainClicked()
        {
            Debug.Log($"[{GameType}] Jugando de nuevo");
            ResetGame();
            StartGame();
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

            // Show appropriate win/lose panel
            ShowResultPanel(currentResult);

            Debug.Log($"{GameType} terminado: {currentResult}");
        }

        /// <summary>
        /// Muestra el panel de resultado apropiado según el modo de juego
        /// </summary>
        protected virtual void ShowResultPanel(MinigameResult result)
        {
            // Verificar si es una partida online 1v1
            if (OnlineResultManager.IsOnlineMatch())
            {
                HandleOnlineResult(result);
                return;
            }

            // Check if we're in a real money session
            bool isRealMoneyMode = GameSessionManager.Instance?.HasActiveSession == true &&
                                   GameSessionManager.Instance.CurrentContext?.Mode != GameMode.Practice;

            if (isRealMoneyMode)
            {
                // Real money mode - need to wait for opponent result
                // For now, just show the panel (in future, compare with opponent)
                var context = GameSessionManager.Instance.CurrentContext;

                // TODO: Get opponent result from server
                // For now, simulate - player always wins if completed
                bool playerWon = result.Completed;
                decimal entryFee = context?.EntryFee ?? 0;

                if (playerWon)
                {
                    if (winPanelRealMoney != null)
                    {
                        winPanelRealMoney.ShowRealMoneyResult(result, null, entryFee, true, "Opponent");
                        SetupPanelCallbacks(winPanelRealMoney);
                    }
                }
                else
                {
                    if (losePanelRealMoney != null)
                    {
                        losePanelRealMoney.ShowRealMoneyResult(result, null, entryFee, false, "Opponent");
                        SetupPanelCallbacks(losePanelRealMoney);
                    }
                }
            }
            else
            {
                // Practice mode - show normal result panel
                if (result.Completed)
                {
                    if (winPanelNormal != null)
                    {
                        winPanelNormal.ShowNormalResult(result);
                        SetupPanelCallbacks(winPanelNormal);
                    }
                }
                else
                {
                    // Game ended without completion (timeout, quit, etc)
                    if (losePanelNormal != null)
                    {
                        losePanelNormal.ShowNormalResult(result);
                        SetupPanelCallbacks(losePanelNormal);
                    }
                }
            }
        }

        /// <summary>
        /// Maneja el resultado de una partida online 1v1
        /// Envía el resultado a Firebase y espera al oponente
        /// </summary>
        private void HandleOnlineResult(MinigameResult result)
        {
            string matchId = OnlineResultManager.GetCurrentMatchId();
            string playerName = PlayerPrefs.GetString("PlayerName", "Jugador");

            Debug.Log($"[{GameType}] Partida online terminada. MatchId: {matchId}, Tiempo: {result.FinalScore:F2}s");

            // Enviar resultado y esperar al oponente
            OnlineResultManager.Instance.SubmitAndWaitForResult(
                matchId,
                result,
                playerName,
                (playerWon) =>
                {
                    Debug.Log($"[{GameType}] Resultado online: {(playerWon ? "VICTORIA" : "DERROTA")}");
                }
            );
        }

        /// <summary>
        /// Configura los callbacks de los botones del panel de resultado
        /// </summary>
        private void SetupPanelCallbacks(WinPanelController panel)
        {
            if (panel == null) return;

            // Clear previous listeners
            panel.OnAcceptClicked -= OnPanelAcceptClicked;
            panel.OnPlayAgainClicked -= OnPanelPlayAgainClicked;

            // Add new listeners
            panel.OnAcceptClicked += OnPanelAcceptClicked;
            panel.OnPlayAgainClicked += OnPanelPlayAgainClicked;
        }

        /// <summary>
        /// Callback cuando se acepta el resultado (vuelve al selector)
        /// </summary>
        private void OnPanelAcceptClicked()
        {
            Debug.Log($"[{GameType}] Accept clicked - returning to selector");
            SceneManager.LoadScene("GameSelector");
        }

        /// <summary>
        /// Callback cuando se quiere jugar de nuevo
        /// </summary>
        private void OnPanelPlayAgainClicked()
        {
            Debug.Log($"[{GameType}] Play Again clicked");

            // Hide panels
            winPanelNormal?.Hide();
            losePanelNormal?.Hide();
            winPanelRealMoney?.Hide();
            losePanelRealMoney?.Hide();

            // Reset and start
            ResetGame();
            StartGame();
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
