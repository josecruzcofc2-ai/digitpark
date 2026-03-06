using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using DigitPark.Services;
using DigitPark.Services.Firebase;
using DigitPark.Data;

namespace DigitPark.Games
{
    /// <summary>
    /// Manager singleton que orquesta las sesiones de juego
    /// Persiste entre escenas y maneja el contexto de la partida actual
    /// </summary>
    public class GameSessionManager : MonoBehaviour
    {
        private static GameSessionManager _instance;
        public static GameSessionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("GameSessionManager");
                    _instance = go.AddComponent<GameSessionManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

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
        public void StartSingleGameSession(GameType gameType, string opponentId, string opponentName, decimal entryFee, string matchId)
        {
            CurrentContext = new GameContext
            {
                Mode = GameMode.SingleGame,
                Games = new List<GameType> { gameType },
                OpponentId = opponentId,
                OpponentName = opponentName,
                EntryFee = entryFee,
                MatchId = matchId
            };

            // Analytics
            AnalyticsService.Instance?.LogGameStart(gameType.ToString());
            AnalyticsService.Instance?.LogMatchmakingStart(gameType.ToString(), entryFee);

            OnSessionStarted?.Invoke(CurrentContext);
            LoadCurrentGame();
        }

        /// <summary>
        /// Inicia un Cognitive Sprint con multiples juegos
        /// </summary>
        public void StartCognitiveSprintSession(List<GameType> games, string opponentId, string opponentName, decimal entryFee, string matchId)
        {
            if (games == null || games.Count < 2 || games.Count > 5)
            {
                Debug.LogError("Cognitive Sprint requiere entre 2 y 5 juegos");
                return;
            }

            CurrentContext = new GameContext
            {
                Mode = GameMode.CognitiveSprint,
                Games = games,
                OpponentId = opponentId,
                OpponentName = opponentName,
                EntryFee = entryFee,
                MatchId = matchId
            };

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
        /// Inicia una sesion de torneo
        /// </summary>
        public void StartTournamentSession(GameType gameType, string tournamentId, string matchId, string opponentId, string opponentName)
        {
            CurrentContext = new GameContext
            {
                Mode = GameMode.Tournament,
                Games = new List<GameType> { gameType },
                TournamentId = tournamentId,
                MatchId = matchId,
                OpponentId = opponentId,
                OpponentName = opponentName
            };

            // Analytics
            AnalyticsService.Instance?.LogGameStart(gameType.ToString());
            AnalyticsService.Instance?.LogTournamentJoined(tournamentId, gameType.ToString(), 0);

            OnSessionStarted?.Invoke(CurrentContext);
            LoadCurrentGame();
        }

        /// <summary>
        /// Configura el contexto directamente (para casos especiales)
        /// </summary>
        public void SetContext(GameContext context)
        {
            CurrentContext = context;
        }

        /// <summary>
        /// Registra el resultado de un juego completado
        /// </summary>
        public async void RegisterGameResult(MinigameResult result)
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
                    if (result.Completed) gameStats.gamesWon++;

                    // Recalcular promedio
                    float totalTime = gameStats.averageTime * (gameStats.gamesPlayed - 1) + result.TotalTime;
                    gameStats.averageTime = totalTime / gameStats.gamesPlayed;
                }

                // Actualizar estadísticas generales
                playerData.totalGamesPlayed++;
                if (result.Completed) playerData.totalGamesWon++;
                if (result.TotalTime < playerData.bestTime)
                {
                    playerData.bestTime = result.TotalTime;
                    isNewRecord = true;
                }
                playerData.AddScore(result.TotalTime);

                // Guardar en Firebase
                try
                {
                    await DatabaseService.Instance?.SavePlayerData(playerData);
                    string gameId = result.GameType.ToString();
                    await DatabaseService.Instance?.SaveScore(
                        playerData.userId,
                        playerData.username,
                        result.TotalTime,
                        playerData.countryCode,
                        gameId
                    );

                    // Si es torneo, actualizar score en el torneo
                    if (CurrentContext.Mode == GameMode.Tournament && !string.IsNullOrEmpty(CurrentContext.TournamentId))
                    {
                        await DatabaseService.Instance?.UpdateTournamentScore(
                            CurrentContext.TournamentId,
                            playerData.userId,
                            result.TotalTime
                        );
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
                bool playerWon = result.Completed;
                DigitPark.Monetization.CurrencyManager.Instance?.SettleBet(playerWon);
                Debug.Log($"[GameSession] Bet settled: {(playerWon ? "WON" : "LOST")} {CurrentContext.BetAmount} {CurrentContext.BetCurrencyType}");
            }

            OnGameCompleted?.Invoke(result);

            // === Achievement tracking ===
            {
                string gameType = CurrentContext.CurrentGame?.ToString();
                int score = (int)result.FinalScore;
                int accuracy = result.Errors == 0 ? 100 : Mathf.Max(0, 100 - (result.Errors * 10));

                AchievementService.Instance?.OnGameCompleted(
                    result.Completed,
                    result.TotalTime,
                    gameType,
                    score,
                    accuracy,
                    wasBehind: false
                );

                // Win streak tracking
                if (result.Completed)
                {
                    int currentStreak = PlayerPrefs.GetInt("CurrentWinStreak", 0) + 1;
                    PlayerPrefs.SetInt("CurrentWinStreak", currentStreak);
                    PlayerPrefs.Save();
                    AchievementService.Instance?.OnWinStreakChanged(currentStreak);
                }
                else
                {
                    PlayerPrefs.SetInt("CurrentWinStreak", 0);
                    PlayerPrefs.Save();
                }
            }

            // Incrementar contador de partidas en ReviewService
            ReviewService.Instance?.IncrementGamesPlayed();

            // Intentar mostrar review prompt si gano (no despues de perder)
            if (result.Completed)
            {
                ReviewService.Instance?.TryRequestReview(justLost: false);
            }

            // Si hay mas juegos en Cognitive Sprint, avanzar
            if (CurrentContext.Mode == GameMode.CognitiveSprint && CurrentContext.HasMoreGames)
            {
                // Mostrar pantalla de transicion o cargar siguiente juego
                Debug.Log($"Juego {CurrentContext.CurrentGameIndex + 1} completado. Siguiente juego...");
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

            // Guardar resultados si es necesario
            SaveSessionResults();

            CurrentContext = null;
        }

        /// <summary>
        /// Cancela la sesion actual sin guardar
        /// </summary>
        public void CancelSession()
        {
            CurrentContext = null;
            SceneManager.LoadScene("MainMenu");
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
            SceneManager.LoadScene(sceneName);
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
        /// Calcula la recompensa en monedas post-juego segun modo y resultado
        /// </summary>
        private int CalculatePostGameReward(MinigameResult result, GameMode mode)
        {
            switch (mode)
            {
                case GameMode.Practice:
                    int reward = 15; // Base: complete practice
                    // Bonus for beating personal best
                    if (result.Completed)
                    {
                        var playerData = AuthenticationService.Instance?.GetCurrentPlayerData();
                        if (playerData != null && CurrentContext.CurrentGame.HasValue)
                        {
                            var stats = playerData.GetGameStats(CurrentContext.CurrentGame.Value.ToString());
                            if (stats != null && result.TotalTime < stats.bestTime)
                                reward += 10; // Beat personal best bonus
                        }
                    }
                    return reward;

                case GameMode.SingleGame:
                case GameMode.Online:
                    return result.Completed ? 25 : 10; // Win: +25, Loss: +10

                case GameMode.Tournament:
                    return result.Completed ? 50 : 15; // Win: +50, Loss: +15

                case GameMode.CognitiveSprint:
                    return result.Completed ? 30 : 10; // Win: +30, Loss: +10

                case GameMode.CashTournament:
                    return 0; // No coin rewards for cash tournaments (real money only)

                default:
                    return 0;
            }
        }

        /// <summary>
        /// Guarda los resultados de la sesion
        /// </summary>
        private async void SaveSessionResults()
        {
            if (CurrentContext == null || CurrentContext.Results.Count == 0)
            {
                Debug.Log("[GameSession] Sesión terminada sin resultados que guardar");
                return;
            }

            Debug.Log($"[GameSession] Sesion terminada. Guardando {CurrentContext.Results.Count} resultados...");

            var playerData = AuthenticationService.Instance?.GetCurrentPlayerData();
            if (playerData == null) return;

            try
            {
                // Guardar datos actualizados del jugador
                await DatabaseService.Instance?.SavePlayerData(playerData);

                // Si es torneo, reportar que terminó
                if (CurrentContext.Mode == GameMode.Tournament && !string.IsNullOrEmpty(CurrentContext.TournamentId))
                {
                    // Calcular mejor tiempo de la sesión
                    float bestSessionTime = float.MaxValue;
                    foreach (var result in CurrentContext.Results)
                    {
                        if (result.TotalTime < bestSessionTime)
                            bestSessionTime = result.TotalTime;
                    }

                    await DatabaseService.Instance?.UpdateTournamentScore(
                        CurrentContext.TournamentId,
                        playerData.userId,
                        bestSessionTime
                    );
                }

                Debug.Log("[GameSession] Resultados guardados exitosamente");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GameSession] Error guardando resultados: {e.Message}");
            }

            // Guardar en historial de partidas generales (Practice y Online, no CashBattle)
            RecordToMatchHistory();
        }

        /// <summary>
        /// Registra la sesion en el historial de partidas generales
        /// </summary>
        private void RecordToMatchHistory()
        {
            if (CurrentContext == null || CurrentContext.Results.Count == 0) return;

            // Solo Practice y Online van al historial general
            // SingleGame/Tournament/CashBattle van a CashHistory
            if (CurrentContext.Mode != GameMode.Practice && CurrentContext.Mode != GameMode.Online)
                return;

            bool isPractice = CurrentContext.Mode == GameMode.Practice;
            bool isSprint = CurrentContext.Games != null && CurrentContext.Games.Count > 1;

            if (isSprint)
            {
                // Cognitive Sprint: una sola entrada con todos los juegos
                float totalTime = 0f;
                int totalErrors = 0;
                float totalPenalty = 0f;

                foreach (var r in CurrentContext.Results)
                {
                    totalTime += r.TotalTime;
                    totalErrors += r.Errors;
                    totalPenalty += r.PenaltyTime;
                }

                string[] gameNames = new string[CurrentContext.Games.Count];
                for (int i = 0; i < CurrentContext.Games.Count; i++)
                    gameNames[i] = CurrentContext.Games[i].ToString();

                MatchHistoryEntry entry;
                if (isPractice)
                {
                    entry = MatchHistoryEntry.CreateCognitiveSprintPractice(
                        gameNames, totalTime, totalErrors, totalPenalty);
                }
                else
                {
                    float opponentTotal = 0f;
                    foreach (var r in CurrentContext.OpponentResults)
                        opponentTotal += r.FinalScore;

                    entry = MatchHistoryEntry.CreateCognitiveSprintOnline(
                        gameNames, totalTime, totalErrors, totalPenalty,
                        CurrentContext.OpponentName, CurrentContext.OpponentId, opponentTotal);
                }

                MatchHistoryStorage.Instance.AddEntry(entry);
            }
            else
            {
                // Juego individual: una entrada por resultado
                foreach (var r in CurrentContext.Results)
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
                        int idx = CurrentContext.Results.IndexOf(r);
                        if (CurrentContext.OpponentResults != null && idx < CurrentContext.OpponentResults.Count)
                            opScore = CurrentContext.OpponentResults[idx].FinalScore;

                        entry = MatchHistoryEntry.CreateOnlineMatch(
                            gameType, r.TotalTime, r.Errors, r.PenaltyTime,
                            CurrentContext.OpponentName, CurrentContext.OpponentId, opScore);
                    }

                    MatchHistoryStorage.Instance.AddEntry(entry);
                }
            }

            Debug.Log($"[GameSession] Partida registrada en historial general");
        }
    }
}
