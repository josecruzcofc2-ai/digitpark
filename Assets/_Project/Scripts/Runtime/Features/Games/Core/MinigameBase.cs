using System;
using UnityEngine;
using UnityEngine.UI;
using DigitPark.UI;
using DigitPark.Managers;
using DigitPark.Services;
using DigitPark.Services.Firebase;
using DigitPark.Localization;
using DigitPark.Navigation;

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
            // Disable auto-navigation from BackButton prefab to prevent double listener
            var autoNav = backButton?.GetComponent<DigitPark.UI.BackButton>();
            if (autoNav != null) autoNav.DisableAutoNavigation();
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
            SceneNavigator.Instance?.NavigateTo("GameSelector");
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

            Screen.sleepTimeout = SleepTimeout.NeverSleep;
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
            Screen.sleepTimeout = SleepTimeout.SystemSetting;

            // Construir resultado
            currentResult.TotalTime = currentTime;
            currentResult.Errors = errorCount;
            currentResult.PenaltyTime = errorCount * (config?.errorPenalty ?? 2f);
            currentResult.Completed = true;
            currentResult.CompletedAt = DateTime.UtcNow;

            OnGameEnded();

            // Registrar en sesion si hay una activa (before notifying listeners)
            if (GameSessionManager.Instance != null && GameSessionManager.Instance.HasActiveSession)
            {
                GameSessionManager.Instance.RegisterGameResult(currentResult);
            }

            // Notificar
            OnGameCompleted?.Invoke(currentResult);

            // Show appropriate win/lose panel
            ShowResultPanel(currentResult);

            Debug.Log($"{GameType} terminado: {currentResult}");
        }

        /// <summary>
        /// Muestra el panel de resultado apropiado según el modo de juego
        /// Routing:
        ///   E1 (Practice single) → Normal panel
        ///   E2 mid-sprint → Normal panel (transición), E2 final → SprintSummary practice
        ///   E3 (Online 1v1) → OnlineResult (existente)
        ///   E4 mid-sprint → Normal panel, E4 final → SprintSummary online
        ///   E5/E6 (Tournament) → TournamentResult
        ///   E7 (Cash 1v1 single) → CashBattleResult
        ///   E8 mid-sprint → Normal panel, E8 final → SprintSummary cash
        /// </summary>
        protected virtual void ShowResultPanel(MinigameResult result)
        {
            var ctx = GameSessionManager.Instance?.CurrentContext;

            // 1. Online free 1v1 (no sprint)
            if (OnlineResultManager.IsOnlineMatch() && ctx?.Mode != GameMode.CognitiveSprint)
            {
                HandleOnlineResult(result);
                return;
            }

            // 2. Online 1v1 in sprint - submit and let OnlineResultManager handle
            if (OnlineResultManager.IsOnlineMatch() && ctx?.Mode == GameMode.CognitiveSprint)
            {
                if (!ctx.HasMoreGames)
                {
                    // Último juego del sprint online → esperar oponente, luego SprintSummary
                    OnlineResultManager.Instance.SubmitSprintAndWaitForResult(ctx);
                    return;
                }
                // Mid-sprint online: enviar resultado pero mostrar panel normal de transición
                HandleOnlineResult(result);
                return;
            }

            // 3. Tournament (free)
            if (ctx?.Mode == GameMode.Tournament)
            {
                HandleTournamentResult(result, ctx);
                return;
            }

            // 4. CognitiveSprint - último juego (practice o cash)
            if (ctx?.Mode == GameMode.CognitiveSprint && !ctx.HasMoreGames)
            {
                if (ResultPanelManager.Instance == null)
                {
                    Debug.LogWarning("[MinigameBase] ResultPanelManager not found for sprint summary");
                    return;
                }
                ResultPanelManager.Instance.ShowSprintSummary(ctx);
                return;
            }

            // 6. CognitiveSprint - más juegos (cualquier modo) → panel normal de transición
            // 7. Practice single → panel normal
            ShowNormalResultPanel(result);
        }

        /// <summary>
        /// Muestra panel normal (práctica o transición mid-sprint)
        /// </summary>
        private void ShowNormalResultPanel(MinigameResult result)
        {
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
                if (losePanelNormal != null)
                {
                    losePanelNormal.ShowNormalResult(result);
                    SetupPanelCallbacks(losePanelNormal);
                }
            }
        }

        /// <summary>
        /// Maneja resultado de torneo usando datos del contexto local
        /// </summary>
        private void HandleTournamentResult(MinigameResult result, GameContext ctx)
        {
            int attemptsUsed = PlayerPrefs.GetInt($"tournament_{ctx.TournamentId}_attempts", 1);
            float bestTime = PlayerPrefs.GetFloat($"tournament_{ctx.TournamentId}_best", result.FinalScore);

            if (result.FinalScore < bestTime)
            {
                bestTime = result.FinalScore;
                PlayerPrefs.SetFloat($"tournament_{ctx.TournamentId}_best", bestTime);
            }
            PlayerPrefs.SetInt($"tournament_{ctx.TournamentId}_attempts", attemptsUsed + 1);
            PlayerPrefs.Save();

            if (ResultPanelManager.Instance == null) { Debug.LogWarning("[MinigameBase] ResultPanelManager not found"); return; }
            ResultPanelManager.Instance.ShowTournamentResult(result, 1, attemptsUsed, 3, bestTime, 0m);
        }

        /// <summary>
        /// Maneja el resultado de una partida online 1v1
        /// Envía el resultado a Firebase y espera al oponente
        /// </summary>
        private void HandleOnlineResult(MinigameResult result)
        {
            string matchId = OnlineResultManager.GetCurrentMatchId();
            string playerName = AuthenticationService.Instance?.GetCurrentPlayerData()?.username
                ?? PlayerPrefs.GetString("DP_PlayerName", AutoLocalizer.Get("default_player_name"));

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
        /// Callback cuando se acepta el resultado (vuelve al selector o avanza en CognitiveSprint)
        /// </summary>
        private void OnPanelAcceptClicked()
        {
            var session = GameSessionManager.Instance;
            if (session?.HasActiveSession == true &&
                session.CurrentContext?.Mode == GameMode.CognitiveSprint &&
                session.CurrentContext.HasMoreGames)
            {
                Debug.Log($"[{GameType}] Accept clicked - advancing to next game in Cognitive Sprint");
                session.ProceedToNextGame();
                return;
            }

            Debug.Log($"[{GameType}] Accept clicked - returning to selector");
            SceneNavigator.Instance?.NavigateTo("GameSelector");
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

        /// <summary>
        /// Indica si el juego está en modo práctica (sin sesión activa o modo Practice)
        /// </summary>
        protected bool IsPracticeMode()
        {
            if (GameSessionManager.Instance == null || !GameSessionManager.Instance.HasActiveSession)
                return true;
            return GameSessionManager.Instance.CurrentContext?.Mode == GameMode.Practice;
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
