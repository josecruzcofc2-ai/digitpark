using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using DigitPark.Games;
using DigitPark.Services;
using DigitPark.UI;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager singleton para mostrar resultados de partidas online 1v1
    /// Instancia los prefabs de victoria/derrota según el resultado
    /// </summary>
    public class OnlineResultManager : MonoBehaviour
    {
        private static OnlineResultManager _instance;
        public static OnlineResultManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("OnlineResultManager");
                    _instance = go.AddComponent<OnlineResultManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Prefabs")]
        [SerializeField] private GameObject onlineWinPanelPrefab;
        [SerializeField] private GameObject onlineLosePanelPrefab;

        [Header("Settings")]
        [SerializeField] private string returnSceneName = "GameSelector"; // Mantiene modo 1v1 activo

        // Panel activo actual
        private GameObject currentPanel;
        private OnlineResultPanelController currentController;

        // Datos del resultado actual
        private string currentMatchId;
        private bool isWaitingForOpponent = false;

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
        /// Muestra el resultado de una partida online
        /// Compara los tiempos y muestra victoria o derrota
        /// </summary>
        public void ShowResult(
            string playerName,
            float playerTime,
            int playerErrors,
            string opponentName,
            float opponentTime,
            int opponentErrors)
        {
            // Determinar ganador (menor tiempo gana)
            bool playerWon = playerTime < opponentTime;

            // Si hay empate en tiempo, el que tenga menos errores gana
            if (Mathf.Approximately(playerTime, opponentTime))
            {
                playerWon = playerErrors < opponentErrors;
            }

            ShowResultPanel(playerWon, playerName, playerTime, playerErrors,
                           opponentName, opponentTime, opponentErrors);
        }

        /// <summary>
        /// Muestra el resultado usando MinigameResult
        /// </summary>
        public void ShowResult(MinigameResult playerResult, MinigameResult opponentResult,
                              string playerName, string opponentName)
        {
            ShowResult(
                playerName,
                playerResult.TotalTime,
                playerResult.Errors,
                opponentName,
                opponentResult.TotalTime,
                opponentResult.Errors
            );
        }

        /// <summary>
        /// Envía el resultado al servidor y espera el resultado del oponente
        /// </summary>
        public void SubmitAndWaitForResult(string matchId, MinigameResult playerResult,
                                           string playerName, Action<bool> onResultReady)
        {
            currentMatchId = matchId;
            isWaitingForOpponent = true;

            // Enviar resultado a Firebase
            if (MatchmakingService.Instance != null)
            {
                MatchmakingService.Instance.SubmitMatchResult(
                    matchId,
                    playerResult.FinalScore,
                    playerResult.TotalTime,
                    playerResult.Errors
                );

                // Escuchar resultado del oponente
                MatchmakingService.Instance.ListenForOpponentResult(matchId, (opponentScore, opponentTime) =>
                {
                    isWaitingForOpponent = false;

                    // Crear resultado del oponente
                    // FinalScore es calculado (TotalTime + PenaltyTime), así que configuramos TotalTime
                    var opponentResult = new MinigameResult
                    {
                        TotalTime = opponentTime,
                        PenaltyTime = opponentScore - opponentTime, // Calcular penalización si la hay
                        Errors = 0 // No tenemos esta info del oponente por ahora
                    };

                    // Obtener nombre del oponente
                    string opponentName = PlayerPrefs.GetString("CurrentOpponentId", "Oponente");

                    // Mostrar resultado
                    ShowResult(playerResult, opponentResult, playerName, opponentName);

                    onResultReady?.Invoke(playerResult.TotalTime < opponentTime);
                });
            }
            else
            {
                Debug.LogError("[OnlineResultManager] MatchmakingService not available!");
                onResultReady?.Invoke(false);
            }
        }

        private void ShowResultPanel(bool playerWon, string playerName, float playerTime, int playerErrors,
                                     string opponentName, float opponentTime, int opponentErrors)
        {
            // Destruir panel anterior si existe
            if (currentPanel != null)
            {
                Destroy(currentPanel);
            }

            // Seleccionar prefab correcto
            GameObject prefab = playerWon ? onlineWinPanelPrefab : onlineLosePanelPrefab;

            if (prefab == null)
            {
                Debug.LogError($"[OnlineResultManager] {(playerWon ? "Win" : "Lose")} panel prefab not assigned!");

                // Fallback: crear panel básico en código
                CreateFallbackPanel(playerWon, playerName, playerTime, opponentName, opponentTime);
                return;
            }

            // Encontrar Canvas en la escena actual
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[OnlineResultManager] No Canvas found in scene!");
                return;
            }

            // Instanciar panel
            currentPanel = Instantiate(prefab, canvas.transform);
            currentPanel.SetActive(true);

            // Configurar controlador
            currentController = currentPanel.GetComponent<OnlineResultPanelController>();
            if (currentController != null)
            {
                currentController.ShowOnlineResult(
                    playerName, playerTime, playerErrors,
                    opponentName, opponentTime, opponentErrors,
                    playerWon
                );

                // Suscribirse a eventos
                currentController.OnContinueClicked += OnContinueClicked;
                currentController.OnRematchClicked += OnRematchClicked;
            }

            Debug.Log($"[OnlineResultManager] Showing {(playerWon ? "WIN" : "LOSE")} panel");
        }

        private void CreateFallbackPanel(bool playerWon, string playerName, float playerTime,
                                        string opponentName, float opponentTime)
        {
            // Crear un panel básico si no hay prefab asignado
            Debug.LogWarning("[OnlineResultManager] Using fallback panel - assign prefabs for better UI!");

            // Por ahora solo loguear el resultado
            string result = playerWon ? "VICTORIA" : "DERROTA";
            float diff = Mathf.Abs(playerTime - opponentTime);
            Debug.Log($"=== {result} ===");
            Debug.Log($"{playerName}: {playerTime:F2}s");
            Debug.Log($"{opponentName}: {opponentTime:F2}s");
            Debug.Log($"Diferencia: {diff:F2}s");

            // Después de un delay, volver al selector
            Invoke(nameof(ReturnToSelector), 3f);
        }

        private void OnContinueClicked()
        {
            CleanupAndReturn();
        }

        private void OnRematchClicked()
        {
            // Por ahora, volver a matchmaking
            // En el futuro: implementar sistema de revancha
            Debug.Log("[OnlineResultManager] Rematch requested - returning to matchmaking");

            if (currentPanel != null)
            {
                Destroy(currentPanel);
                currentPanel = null;
            }

            // Volver a matchmaking con el mismo juego
            SceneManager.LoadScene("Matchmaking");
        }

        private void CleanupAndReturn()
        {
            // Limpiar
            if (currentController != null)
            {
                currentController.OnContinueClicked -= OnContinueClicked;
                currentController.OnRematchClicked -= OnRematchClicked;
            }

            if (currentPanel != null)
            {
                Destroy(currentPanel);
                currentPanel = null;
            }

            currentController = null;
            currentMatchId = null;

            // Limpiar datos de partida
            PlayerPrefs.DeleteKey("CurrentMatchId");
            PlayerPrefs.DeleteKey("CurrentOpponentId");
            PlayerPrefs.SetInt("IsOnlineMatch", 0);
            PlayerPrefs.Save();

            ReturnToSelector();
        }

        private void ReturnToSelector()
        {
            SceneManager.LoadScene(returnSceneName);
        }

        /// <summary>
        /// Verifica si estamos en una partida online
        /// </summary>
        public static bool IsOnlineMatch()
        {
            return PlayerPrefs.GetInt("IsOnlineMatch", 0) == 1;
        }

        /// <summary>
        /// Obtiene el ID de la partida actual
        /// </summary>
        public static string GetCurrentMatchId()
        {
            return PlayerPrefs.GetString("CurrentMatchId", "");
        }

        /// <summary>
        /// Obtiene el nombre del oponente actual
        /// </summary>
        public static string GetCurrentOpponentName()
        {
            return PlayerPrefs.GetString("CurrentOpponentId", "Oponente");
        }
    }
}
