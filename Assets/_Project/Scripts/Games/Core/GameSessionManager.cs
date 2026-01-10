using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        public void RegisterGameResult(MinigameResult result)
        {
            if (CurrentContext == null)
            {
                Debug.LogError("No hay sesion activa para registrar resultado");
                return;
            }

            CurrentContext.AddResult(result);
            OnGameCompleted?.Invoke(result);

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
        /// Guarda los resultados de la sesion
        /// </summary>
        private void SaveSessionResults()
        {
            // TODO: Implementar guardado en Firebase/servidor
            Debug.Log($"Sesion terminada. Resultados: {CurrentContext.Results.Count} juegos completados");
        }
    }
}
