using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using DigitPark.Games;
using DigitPark.Services;
using DigitPark.Services.Firebase;
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
                // C-67: Use implicit bool to detect destroyed Unity objects (== null is overloaded)
                if (!_instance)
                {
                    _instance = FindObjectOfType<OnlineResultManager>();
                    if (!_instance)
                    {
                        var go = new GameObject("OnlineResultManager");
                        DontDestroyOnLoad(go);
                        // Set _instance BEFORE AddComponent to prevent Awake race condition
                        _instance = go.AddComponent<OnlineResultManager>();
                    }
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
        #pragma warning disable 0414
        private bool isWaitingForOpponent = false;
        #pragma warning restore 0414

        private const string PREFAB_BASE = "Prefabs/WinPanels/";

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPrefabsFromResources();
        }

        /// <summary>
        /// Auto-carga prefabs desde Resources si no están asignados por Inspector
        /// </summary>
        private void LoadPrefabsFromResources()
        {
            if (onlineWinPanelPrefab == null)
            {
                onlineWinPanelPrefab = Resources.Load<GameObject>(PREFAB_BASE + "OnlineWinPanel");
                if (onlineWinPanelPrefab == null)
                    Debug.LogError($"[OnlineResultManager] Failed to load prefab: {PREFAB_BASE}OnlineWinPanel");
            }
            if (onlineLosePanelPrefab == null)
            {
                onlineLosePanelPrefab = Resources.Load<GameObject>(PREFAB_BASE + "OnlineLosePanel");
                if (onlineLosePanelPrefab == null)
                    Debug.LogError($"[OnlineResultManager] Failed to load prefab: {PREFAB_BASE}OnlineLosePanel");
            }

            int loaded = 0;
            if (onlineWinPanelPrefab != null) loaded++;
            if (onlineLosePanelPrefab != null) loaded++;
            Debug.Log($"[OnlineResultManager] Prefabs loaded from Resources: {loaded}/2");
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
                if (playerErrors != opponentErrors)
                    playerWon = playerErrors < opponentErrors;
                else
                    playerWon = true; // Perfect tie: benefit of the doubt goes to player
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

        /// <summary>
        /// Envía todos los resultados del sprint y espera los del oponente.
        /// Cuando llegan, muestra SprintSummaryPanel en lugar de OnlineResultPanel.
        /// </summary>
        public void SubmitSprintAndWaitForResult(GameContext sprintContext)
        {
            if (sprintContext == null)
            {
                Debug.LogError("[OnlineResultManager] SubmitSprintAndWaitForResult: context is null");
                return;
            }

            string matchId = sprintContext.MatchId ?? GetCurrentMatchId();
            isWaitingForOpponent = true;

            Debug.Log($"[OnlineResultManager] Submitting sprint results for match {matchId}");

            if (MatchmakingService.Instance != null)
            {
                // Enviar el último resultado del sprint
                if (sprintContext.Results == null || sprintContext.Results.Count == 0)
                {
                    Debug.LogError("[OnlineResultManager] Sprint results list is empty — cannot submit!");
                    ResultPanelManager.Instance?.ShowSprintSummary(sprintContext);
                    return;
                }
                var lastResult = sprintContext.Results[sprintContext.Results.Count - 1];
                MatchmakingService.Instance.SubmitMatchResult(
                    matchId,
                    lastResult.FinalScore,
                    lastResult.TotalTime,
                    lastResult.Errors
                );

                // Escuchar resultado del oponente
                MatchmakingService.Instance.ListenForOpponentResult(matchId, (opponentScore, opponentTime) =>
                {
                    isWaitingForOpponent = false;

                    // Crear resultados del oponente para cada juego del sprint
                    // TODO: En el futuro, recibir resultados individuales del servidor
                    for (int i = sprintContext.OpponentResults.Count; i < sprintContext.Results.Count; i++)
                    {
                        var opponentResult = new MinigameResult
                        {
                            TotalTime = opponentTime / sprintContext.Results.Count,
                            PenaltyTime = 0,
                            Errors = 0,
                            Completed = true
                        };
                        sprintContext.OpponentResults.Add(opponentResult);
                    }

                    // Mostrar SprintSummary en lugar de OnlineResultPanel
                    ResultPanelManager.Instance.ShowSprintSummary(sprintContext);

                    // Limpiar datos de partida
                    PlayerPrefs.SetInt("IsOnlineMatch", 0);
                    PlayerPrefs.Save();
                });
            }
            else
            {
                Debug.LogError("[OnlineResultManager] MatchmakingService not available for sprint!");
                // Fallback: mostrar sprint summary con los datos que tenemos
                ResultPanelManager.Instance.ShowSprintSummary(sprintContext);
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
            Canvas canvas = UICanvasHelper.FindMainCanvas();
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

            // Economy Rebalance V55 — DC rewards per ranked match
            // Win=15 DC, Loss=5 DC, Perfect=+25 DC, FWOTD=+50 DC
            GrantRankedRewards(playerWon, playerErrors == 0);
        }

        /// <summary>
        /// Economy Rebalance V55: Grant DC rewards for ranked 1v1 matches.
        /// Win=15 DC, Loss=5 DC, Perfect Score=+25 DC bonus, First Win of the Day=+50 DC.
        /// Target: ~185 DC/day for active player (10 games, 6 wins).
        /// </summary>
        private void GrantRankedRewards(bool playerWon, bool isPerfect)
        {
            var currency = DigitPark.Monetization.CurrencyManager.Instance;
            if (currency == null) return;

            int dcReward = playerWon ? 15 : 5;
            string reason = playerWon ? "ranked_win" : "ranked_loss";

            // Perfect score bonus
            if (isPerfect && playerWon)
            {
                dcReward += 25;
                reason = "ranked_perfect_win";
            }

            // First Win of the Day (FWOTD)
            if (playerWon)
            {
                string todayKey = System.DateTime.UtcNow.ToString("yyyy-MM-dd");
                string lastFWOTD = PlayerPrefs.GetString("FWOTD_LastDate", "");
                if (lastFWOTD != todayKey)
                {
                    dcReward += 50;
                    PlayerPrefs.SetString("FWOTD_LastDate", todayKey);
                    PlayerPrefs.Save();
                    reason = isPerfect ? "ranked_fwotd_perfect" : "ranked_fwotd";
                    Debug.Log("[OnlineResultManager] First Win of the Day! +50 DC bonus");
                }
            }

            currency.AddCoins(dcReward);
            DigitPark.Services.Firebase.AnalyticsService.Instance?.LogVirtualCurrencyEarned("digitcoins", dcReward, reason);
            Debug.Log($"[OnlineResultManager] Ranked reward: +{dcReward} DC ({reason})");

            // Economy Rebalance V55 — Currency tutorial tooltip (shows once after first win)
            if (playerWon && PlayerPrefs.GetInt("tutorial_currency_shown", 0) == 0)
            {
                PlayerPrefs.SetInt("tutorial_currency_shown", 1);
                PlayerPrefs.Save();
                // The tooltip is shown by the UI layer (OnlineResultPanelController checks this flag)
                Debug.Log("[OnlineResultManager] Currency tutorial flag set — UI will show tooltip");
            }
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

        private void OnDestroy()
        {
            CancelInvoke();
            if (currentController != null)
            {
                currentController.OnContinueClicked -= OnContinueClicked;
                currentController.OnRematchClicked -= OnRematchClicked;
            }
            if (_instance == this) _instance = null;
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
