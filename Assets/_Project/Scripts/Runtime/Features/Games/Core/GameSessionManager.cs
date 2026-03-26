using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using DigitPark.Services;
using DigitPark.Services.Firebase;
using DigitPark.Data;
using DigitPark.Navigation;
using DigitPark.Economy;
#if UNITY_ANDROID
using Google.Play.Review;
#endif

namespace DigitPark.Games
{
    /// <summary>
    /// Manager singleton que orquesta las sesiones de juego
    /// Persiste entre escenas y maneja el contexto de la partida actual
    /// </summary>
    public class GameSessionManager : MonoBehaviour
    {
        private static GameSessionManager _instance;
        public static GameSessionManager Instance => _instance;

        /// <summary>
        /// Contexto de la sesion actual
        /// </summary>
        public GameContext CurrentContext { get; private set; }

        /// <summary>
        /// Si hay una sesion activa
        /// </summary>
        public bool HasActiveSession => CurrentContext != null;

        /// <summary>
        /// Evento cuando una sesion inicia
        /// </summary>
        public event Action<GameContext> OnSessionStarted;

        /// <summary>
        /// Evento cuando un juego dentro de la sesion termina
        /// </summary>
        public event Action<MinigameResult> OnGameCompleted;

        /// <summary>
        /// Evento cuando toda la sesion termina
        /// </summary>
        public event Action<GameContext> OnSessionEnded;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Inicia una sesion de practica con un solo juego
        /// </summary>
        public void StartPracticeSession(GameType gameType)
        {
            CurrentContext = new GameContext
            {
                Mode = GameMode.Practice,
                Games = new List<GameType> { gameType },
                EntryFee = 0
            };

            // Analytics
            AnalyticsService.Instance?.LogGameStart(gameType.ToString());

            OnSessionStarted?.Invoke(CurrentContext);
            LoadCurrentGame();
        }

        /// <summary>
        /// Inicia una sesion 1v1 de un solo juego
        /// </summary>
        public void StartSingleGameSession(GameType gameType, string opponentId, string opponentName, decimal entryFee, string matchId, int rounds = 1)
        {
            CurrentContext = new GameContext
            {
                Mode = GameMode.SingleGame,
                Games = new List<GameType> { gameType },
                OpponentId = opponentId,
                OpponentName = opponentName,
                EntryFee = entryFee,
                MatchId = matchId,
                Rounds = rounds
            };

            // Analytics
            AnalyticsService.Instance?.LogGameStart(gameType.ToString());
            AnalyticsService.Instance?.LogMatchmakingStart(gameType.ToString(), entryFee);

            OnSessionStarted?.Invoke(CurrentContext);
            LoadCurrentGame();
        }

        /// <summary>
        /// Inicia una sesion de partida online 1v1
        /// </summary>
        public void StartOnlineMatch(string matchId, string opponentName)
        {
            // El contexto actual ya deberia estar configurado por SetContext
            // Solo actualizamos los datos de la partida
            if (CurrentContext != null)
            {
                CurrentContext.MatchId = matchId;
                CurrentContext.OpponentName = opponentName;
                CurrentContext.Mode = GameMode.Online;
            }

            Debug.Log($"[GameSessionManager] Online match started: {matchId} vs {opponentName}");
        }

        /// <summary>
        /// Configura el contexto directamente (para casos especiales)
        /// </summary>
        public void SetContext(GameContext context)
        {
            CurrentContext = context;
        }

        /// <summary>
        /// Dispara OnSessionStarted manualmente (usar después de SetContext)
        /// </summary>
        public void NotifySessionStarted()
        {
            if (CurrentContext != null)
            {
                OnSessionStarted?.Invoke(CurrentContext);
            }
        }

        /// <summary>
        /// Registra el resultado de un juego completado
        /// </summary>
        public async void RegisterGameResult(MinigameResult result)
        {
            try
            {
                if (CurrentContext == null)
                {
                    Debug.LogError("No hay sesion activa para registrar resultado");
                    return;
                }

                // Obtener datos del jugador actual
                var playerData = AuthenticationService.Instance?.GetCurrentPlayerData();
                bool isNewRecord = false;

                if (playerData != null && CurrentContext.CurrentGame.HasValue)
                {
                    string gameType = CurrentContext.CurrentGame.Value.ToString();
                    var gameStats = playerData.GetGameStats(gameType);

                    // Verificar si es nuevo record
                    if (gameStats != null && result.TotalTime < gameStats.bestTime)
                    {
                        isNewRecord = true;
                        gameStats.bestTime = result.TotalTime;
                    }

                    // Actualizar estadísticas del juego
                    if (gameStats != null)
                    {
                        gameStats.gamesPlayed++;
                        // Solo contar como victoria en Practice (completar = ganar)
                        // En modos competitivos, la victoria se determina al comparar con oponente
                        bool isPractice = CurrentContext.Mode == GameMode.Practice;
                        if (result.Completed && isPractice) gameStats.gamesWon++;

                        // Recalcular promedio usando double para evitar precision loss
                        double totalTime = (double)gameStats.averageTime * (gameStats.gamesPlayed - 1) + result.TotalTime;
                        gameStats.averageTime = gameStats.gamesPlayed > 0
                            ? (float)(totalTime / gameStats.gamesPlayed)
                            : result.TotalTime;
                    }

                    // Actualizar estadísticas generales
                    playerData.totalGamesPlayed++;
                    bool isPracticeMode = CurrentContext.Mode == GameMode.Practice;
                    if (result.Completed && isPracticeMode) playerData.totalGamesWon++;
                    if (result.TotalTime < playerData.bestTime)
                    {
                        playerData.bestTime = result.TotalTime;
                        isNewRecord = true;
                    }
                    playerData.AddScore(result.TotalTime);

                    // Guardar en Firebase
                    try
                    {
                        if (DatabaseService.Instance != null)
                        {
                            await DatabaseService.Instance.SavePlayerData(playerData);
                            string gameId = result.GameType.ToString();
                            await DatabaseService.Instance.SaveScore(
                                playerData.userId,
                                playerData.username,
                                result.TotalTime,
                                playerData.countryCode,
                                gameId
                            );

                        }
                        else
                        {
                            Debug.LogWarning("[GameSession] DatabaseService not available — score not saved!");
                        }

                        Debug.Log($"[GameSession] Score guardado en Firebase: {result.TotalTime}s");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"[GameSession] Error guardando score: {e.Message}");
                    }
                }

                // Analytics - Registrar game_complete
                if (CurrentContext.CurrentGame.HasValue)
                {
                    AnalyticsService.Instance?.LogGameComplete(
                        CurrentContext.CurrentGame.Value.ToString(),
                        result.TotalTime,
                        (int)result.FinalScore,
                        result.Completed,
                        isNewRecord
                    );
                }

                CurrentContext.AddResult(result);

                // === Post-game coin rewards ===
                int coinsEarned = CalculatePostGameReward(result, CurrentContext.Mode);
                if (coinsEarned > 0)
                {
                    DigitPark.Monetization.CurrencyManager.Instance?.AddCoins(coinsEarned);
                    Debug.Log($"[GameSession] Post-game reward: +{coinsEarned} coins (mode: {CurrentContext.Mode})");
                }

                // === Settle bet if active ===
                if (CurrentContext.BetAmount > 0 && CurrentContext.BetCurrencyType != DigitPark.Monetization.BetCurrencyType.None)
                {
                    // In competitive modes, determine win by comparing with opponent results
                    // In practice mode, completion = win
                    bool playerWon;
                    if (CurrentContext.Mode == GameMode.Practice)
                    {
                        playerWon = result.Completed;
                    }
                    else if (CurrentContext.OpponentResults != null && CurrentContext.OpponentResults.Count > 0)
                    {
                        // Compare total time with opponent's — lower time wins.
                        // Use TotalTime (not FinalScore) for consistent comparison — FinalScore includes penalties
                        // which are already factored into TotalTime for the local player's result.TotalTime.
                        float opponentTime = CurrentContext.OpponentResults[CurrentContext.OpponentResults.Count - 1].TotalTime;
                        playerWon = result.Completed && result.TotalTime < opponentTime;
                    }
                    else
                    {
                        // No opponent results yet — fallback to completion
                        playerWon = result.Completed;
                    }
                    DigitPark.Monetization.CurrencyManager.Instance?.SettleBet(playerWon);
                    Debug.Log($"[GameSession] Bet settled: {(playerWon ? "WON" : "LOST")} {CurrentContext.BetAmount} {CurrentContext.BetCurrencyType}");
                }

                OnGameCompleted?.Invoke(result);

                // Win streak tracking — only count actual victories in competitive modes
                {
                    bool isWin;
                    if (CurrentContext.Mode == GameMode.Practice)
                    {
                        isWin = result.Completed;
                    }
                    else if (CurrentContext.OpponentResults != null && CurrentContext.OpponentResults.Count > 0)
                    {
                        // Use TotalTime consistently (not FinalScore) for win determination
                        float opTime = CurrentContext.OpponentResults[CurrentContext.OpponentResults.Count - 1].TotalTime;
                        isWin = result.Completed && result.TotalTime < opTime;
                    }
                    else
                    {
                        isWin = result.Completed;
                    }

                    if (isWin)
                    {
                        int currentStreak = PlayerPrefs.GetInt("DP_CurrentWinStreak", 0) + 1;
                        PlayerPrefs.SetInt("DP_CurrentWinStreak", currentStreak);
                        PlayerPrefs.Save();

                        // Sync win streak to Firebase
                        SyncWinStreakToFirebase(currentStreak);
                    }
                    else
                    {
                        PlayerPrefs.SetInt("DP_CurrentWinStreak", 0);
                        PlayerPrefs.Save();

                        // Sync reset streak to Firebase
                        SyncWinStreakToFirebase(0);
                    }

                    // scorePercentile requiere datos comparativos reales (ranking backend).
                    // Se mantiene en 0 hasta tener esos datos — sin bonus especulativo.
                    const float scorePercentile = 0f;

                }

                if (result.Completed)
                {
                    int gamesPlayed = PlayerPrefs.GetInt("DP_TotalGamesPlayed", 0) + 1;
                    PlayerPrefs.SetInt("DP_TotalGamesPlayed", gamesPlayed);
                    PlayerPrefs.Save();
                    if (gamesPlayed == 10)
                    {
#if UNITY_IOS && !UNITY_EDITOR
                        UnityEngine.iOS.Device.RequestStoreReview();
#elif UNITY_ANDROID && !UNITY_EDITOR
                        RequestAndroidReviewFlow();
#endif
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameSessionManager] {ex.Message}");
            }
        }

        /// <summary>
        /// Avanza al siguiente juego en Cognitive Sprint
        /// </summary>
        public void ProceedToNextGame()
        {
            if (CurrentContext == null || !CurrentContext.HasMoreGames)
            {
                Debug.LogWarning("No hay mas juegos en la sesion");
                EndSession();
                return;
            }

            CurrentContext.MoveToNextGame();
            LoadCurrentGame();
        }

        /// <summary>
        /// Termina la sesion actual
        /// </summary>
        public void EndSession()
        {
            if (CurrentContext == null) return;

            OnSessionEnded?.Invoke(CurrentContext);

            // Guardar resultados con referencia local antes de nulificar
            var contextToSave = CurrentContext;
            CurrentContext = null;
            _ = SaveSessionResults(contextToSave).ContinueWith(t =>
            {
                if (t.IsFaulted) Debug.LogError($"[GameSessionManager] SaveSessionResults failed: {t.Exception?.GetBaseException().Message}");
            });
        }

        /// <summary>
        /// Cancela la sesion actual sin guardar
        /// </summary>
        public void CancelSession()
        {
            CurrentContext = null;
            SceneNavigator.Instance?.NavigateTo("MainMenu");
        }

        /// <summary>
        /// Carga la escena del juego actual
        /// </summary>
        private void LoadCurrentGame()
        {
            if (CurrentContext?.CurrentGame == null)
            {
                Debug.LogError("No hay juego actual para cargar");
                return;
            }

            string sceneName = GetSceneNameForGame(CurrentContext.CurrentGame.Value);
            Debug.Log($"Cargando escena: {sceneName}");
            SceneNavigator.Instance?.NavigateTo(sceneName);
        }

        /// <summary>
        /// Obtiene el nombre de escena para un tipo de juego
        /// </summary>
        public string GetSceneNameForGame(GameType gameType)
        {
            return gameType switch
            {
                GameType.DigitRush => "DigitRush",
                GameType.MemoryPairs => "MemoryPairs",
                GameType.QuickMath => "QuickMath",
                GameType.FlashTap => "FlashTap",
                GameType.OddOneOut => "OddOneOut",
                _ => "MainMenu"
            };
        }

        /// <summary>
        /// Syncs the current win streak value to Firebase
        /// </summary>
        private void SyncWinStreakToFirebase(int currentStreak)
        {
            if (DatabaseService.Instance != null)
            {
                var playerData = AuthenticationService.Instance?.GetCurrentPlayerData();
                if (playerData != null)
                {
                    var updates = new Dictionary<string, object> { { "currentWinStreak", currentStreak } };
                    DatabaseService.Instance.UpdatePlayerFields(playerData.userId, updates).ContinueWith(t =>
                    {
                        if (t.IsFaulted) Debug.LogWarning($"[GameSessionManager] Win streak sync failed: {t.Exception?.GetBaseException().Message}");
                    }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }
            }
        }

        /// <summary>
        /// Calcula la recompensa en monedas post-juego segun modo y resultado
        /// </summary>
        private int CalculatePostGameReward(MinigameResult result, GameMode mode)
        {
            switch (mode)
            {
                case GameMode.Practice:
                    if (!result.Completed) return 0; // No reward if abandoned
                    int reward = EconomyConstants.COINS_PRACTICE_BASE;
                    // Bonus for beating personal best
                    if (result.Completed)
                    {
                        var playerData = AuthenticationService.Instance?.GetCurrentPlayerData();
                        if (playerData != null && CurrentContext.CurrentGame.HasValue)
                        {
                            var stats = playerData.GetGameStats(CurrentContext.CurrentGame.Value.ToString());
                            if (stats != null && result.TotalTime < stats.bestTime)
                                reward += EconomyConstants.COINS_PRACTICE_PB_BONUS;
                        }
                    }
                    return reward;

                case GameMode.SingleGame:
                    return result.Completed ? EconomyConstants.COINS_SINGLEGAME_WIN : EconomyConstants.COINS_SINGLEGAME_LOSS;

                case GameMode.Online:
                    return 0; // Handled by OnlineResultManager.GrantRankedRewards (FWOTD + perfect bonus)

                default:
                    return 0;
            }
        }

        /// <summary>
        /// Guarda los resultados de la sesion
        /// </summary>
        private async Task SaveSessionResults(GameContext ctx)
        {
            try
            {
                if (ctx == null || ctx.Results.Count == 0)
                {
                    Debug.Log("[GameSession] Sesión terminada sin resultados que guardar");
                    return;
                }

                Debug.Log($"[GameSession] Sesion terminada. Guardando {ctx.Results.Count} resultados...");

                var playerData = AuthenticationService.Instance?.GetCurrentPlayerData();
                if (playerData == null) return;

                try
                {
                    // Guardar datos actualizados del jugador
                    if (DatabaseService.Instance != null)
                    {
                        await DatabaseService.Instance.SavePlayerData(playerData);
                    }

                    Debug.Log("[GameSession] Resultados guardados exitosamente");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[GameSession] Error guardando resultados: {e.Message}");
                }

                // Guardar en historial de partidas generales
                RecordToMatchHistory(ctx);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameSessionManager] {ex.Message}");
            }
        }

        /// <summary>
        /// Registra la sesion en el historial de partidas generales
        /// </summary>
        private void RecordToMatchHistory(GameContext ctx)
        {
            if (ctx == null || ctx.Results.Count == 0) return;

            // Solo Practice y Online van al historial general
            if (ctx.Mode != GameMode.Practice && ctx.Mode != GameMode.Online)
                return;

            bool isPractice = ctx.Mode == GameMode.Practice;

            {
                // Juego individual: una entrada por resultado
                foreach (var r in ctx.Results)
                {
                    string gameType = r.GameType.ToString();
                    MatchHistoryEntry entry;

                    if (isPractice)
                    {
                        entry = MatchHistoryEntry.CreatePractice(
                            gameType, r.TotalTime, r.Errors, r.PenaltyTime);
                    }
                    else
                    {
                        // Buscar score del oponente para este juego
                        float opScore = 0f;
                        int idx = ctx.Results.IndexOf(r);
                        if (ctx.OpponentResults != null && idx < ctx.OpponentResults.Count)
                            opScore = ctx.OpponentResults[idx].FinalScore;

                        entry = MatchHistoryEntry.CreateOnlineMatch(
                            gameType, r.TotalTime, r.Errors, r.PenaltyTime,
                            ctx.OpponentName, ctx.OpponentId, opScore);
                    }

                    MatchHistoryStorage.Instance.AddEntry(entry);
                }
            }

            Debug.Log($"[GameSession] Partida registrada en historial general");
        }

#if UNITY_ANDROID
        private async void RequestAndroidReviewFlow()
        {
            try
            {
                var reviewManager = new ReviewManager();
                var requestTask = reviewManager.RequestReviewFlow();
                await requestTask;
                if (requestTask.Error != ReviewErrorCode.NoError) return;
                var launchTask = reviewManager.LaunchReviewFlow(requestTask.Result);
                await launchTask;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GameSession] Android review flow failed: {e.Message}");
            }
        }
#endif
    }
}
