using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using DigitPark.Games;
using DigitPark.UI;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager singleton para mostrar paneles de resultado post-minijuego
    /// Maneja Sprint Summary y Tournament Result
    /// </summary>
    public class ResultPanelManager : MonoBehaviour
    {
        private static ResultPanelManager _instance;
        public static ResultPanelManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("ResultPanelManager");
                    _instance = go.AddComponent<ResultPanelManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Sprint Summary Prefabs")]
        [SerializeField] private GameObject sprintSummaryPrefab;

        [Header("Tournament Result Prefabs")]
        [SerializeField] private GameObject tournamentResultWinPrefab;
        [SerializeField] private GameObject tournamentResultLosePrefab;

        // Panel activo actual
        private GameObject currentPanel;

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
            if (sprintSummaryPrefab == null)
                sprintSummaryPrefab = Resources.Load<GameObject>(PREFAB_BASE + "SprintSummaryPanel");
            if (tournamentResultWinPrefab == null)
                tournamentResultWinPrefab = Resources.Load<GameObject>(PREFAB_BASE + "TournamentResultWin");
            if (tournamentResultLosePrefab == null)
                tournamentResultLosePrefab = Resources.Load<GameObject>(PREFAB_BASE + "TournamentResultLose");
            // Log de verificación
            int loaded = 0;
            if (sprintSummaryPrefab != null) loaded++;
            if (tournamentResultWinPrefab != null) loaded++;
            if (tournamentResultLosePrefab != null) loaded++;
            Debug.Log($"[ResultPanelManager] Prefabs loaded from Resources: {loaded}/3");
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        // ====================================================================
        // SPRINT SUMMARY
        // ====================================================================

        /// <summary>
        /// Muestra el resumen de un sprint completo
        /// Detecta automáticamente si es practice/online/cash
        /// </summary>
        public void ShowSprintSummary(GameContext ctx)
        {
            if (ctx == null)
            {
                Debug.LogError("[ResultPanelManager] ShowSprintSummary: GameContext is null");
                return;
            }

            var controller = InstantiatePanel(sprintSummaryPrefab, "SprintSummary");
            if (controller == null) return;

            var sprintController = controller.GetComponent<SprintSummaryPanelController>();
            if (sprintController == null)
            {
                Debug.LogError("[ResultPanelManager] SprintSummaryPanelController not found on prefab");
                return;
            }

            // Detectar tipo de sprint y mostrar variante correcta
            if (ctx.OpponentResults != null && ctx.OpponentResults.Count > 0)
            {
                // Online sprint (has opponent data)
                sprintController.ShowOnlineSummary(ctx);
                sprintController.OnContinueClicked += () => NavigateTo("PlayModeSelection");
                sprintController.OnRematchClicked += () => NavigateTo("Matchmaking");
            }
            else
            {
                // Practice sprint
                sprintController.ShowPracticeSummary(ctx);
                sprintController.OnPlayAgainClicked += () => RestartSprint(ctx);
                sprintController.OnBackClicked += () => NavigateTo("GameSelector");
            }

            Debug.Log($"[ResultPanelManager] Sprint summary shown - {ctx.Results?.Count ?? 0} games");
        }

        // ====================================================================
        // TOURNAMENT RESULT
        // ====================================================================

        /// <summary>
        /// Muestra el resultado de un intento de torneo
        /// </summary>
        public void ShowTournamentResult(MinigameResult result, int position,
            int attemptsUsed, int maxAttempts, float bestTime, decimal prize)
        {
            bool isGoodResult = position <= 3;
            GameObject prefab = isGoodResult ? tournamentResultWinPrefab : tournamentResultLosePrefab;

            var panel = InstantiatePanel(prefab, "TournamentResult");
            if (panel == null) return;

            var controller = panel.GetComponent<TournamentResultPanelController>();
            if (controller == null)
            {
                Debug.LogError("[ResultPanelManager] TournamentResultPanelController not found on prefab");
                return;
            }

            controller.ShowTournamentResult(result, position, attemptsUsed, maxAttempts, bestTime, prize);

            controller.OnRetryClicked += () =>
            {
                CleanupCurrentPanel();
                // Reiniciar el juego del torneo
                var session = GameSessionManager.Instance;
                if (session?.HasActiveSession == true)
                {
                    string sceneName = session.CurrentContext.GetCurrentSceneName();
                    if (sceneName != null)
                        SceneManager.LoadScene(sceneName);
                }
            };
            controller.OnLeaderboardClicked += () =>
            {
                CleanupCurrentPanel();
                NavigateTo("TournamentLobby");
            };
            controller.OnExitClicked += () =>
            {
                CleanupCurrentPanel();
                NavigateTo("GameSelector");
            };

            Debug.Log($"[ResultPanelManager] Tournament result shown - Position: {position}, Attempts: {attemptsUsed}/{maxAttempts}");
        }

        // ====================================================================
        // HELPERS
        // ====================================================================

        private GameObject InstantiatePanel(GameObject prefab, string panelType)
        {
            CleanupCurrentPanel();

            if (prefab == null)
            {
                Debug.LogError($"[ResultPanelManager] {panelType} prefab not assigned!");
                return null;
            }

            Canvas canvas = UICanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("[ResultPanelManager] No Canvas found in scene!");
                return null;
            }

            currentPanel = Instantiate(prefab, canvas.transform);
            currentPanel.SetActive(true);
            return currentPanel;
        }

        private void CleanupCurrentPanel()
        {
            if (currentPanel != null)
            {
                Destroy(currentPanel);
                currentPanel = null;
            }
        }

        private void NavigateTo(string sceneName)
        {
            CleanupCurrentPanel();

            // Limpiar sesión si existe
            if (GameSessionManager.Instance != null)
            {
                GameSessionManager.Instance.EndSession();
            }

            SceneManager.LoadScene(sceneName);
        }

        private void RestartSprint(GameContext originalCtx)
        {
            CleanupCurrentPanel();

            // Iniciar nueva sesión de sprint con los mismos juegos
            if (GameSessionManager.Instance != null)
            {
                GameSessionManager.Instance.StartCognitiveSprintSession(
                    originalCtx.Games,
                    originalCtx.OpponentId,
                    originalCtx.OpponentName,
                    originalCtx.EntryFee,
                    originalCtx.MatchId
                );
            }
        }
    }
}
