using UnityEngine;
using System;
using DigitPark.Managers;
using DigitPark.Themes;
using DigitPark.Games;
using DigitPark.Services.Firebase;
using DigitPark.Data;

namespace DigitPark.DevTools
{
    /// <summary>
    /// Manager de herramientas de debug para desarrollo y testing
    /// Solo funciona en Editor y Development Builds
    /// </summary>
    public class DebugManager : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static DebugManager _instance;
        public static DebugManager Instance => _instance;

        [Header("Debug Settings")]
        [SerializeField] private bool showDebugPanel = true;
        [SerializeField] private KeyCode togglePanelKey = KeyCode.F1;

        [Header("UI")]
        [SerializeField] private bool showOnScreenLog = false;
        [SerializeField] private int maxLogMessages = 10;

        // Estado interno
        private bool _panelVisible = false;
        private Rect _windowRect = new Rect(10, 10, 320, 500);
        private Vector2 _scrollPosition;
        private string[] _logMessages = new string[10];
        private int _logIndex = 0;
        private int _selectedTab = 0;
        private readonly string[] _tabNames = { "General", "Premium", "Games", "Firebase", "Themes" };

        // Cache de datos
        #pragma warning disable 0414
        private string _testUserId = "";
        private string _testUsername = "";
        #pragma warning restore 0414
        private float _testScore = 5.0f;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                Application.logMessageReceived += OnLogMessageReceived;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                Application.logMessageReceived -= OnLogMessageReceived;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(togglePanelKey))
            {
                _panelVisible = !_panelVisible;
            }
        }

        private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (!showOnScreenLog) return;

            string prefix = type switch
            {
                LogType.Error => "[ERROR] ",
                LogType.Warning => "[WARN] ",
                _ => ""
            };

            _logMessages[_logIndex] = $"{prefix}{condition}";
            _logIndex = (_logIndex + 1) % maxLogMessages;
        }

        private void OnGUI()
        {
            if (!showDebugPanel || !_panelVisible) return;

            _windowRect = GUILayout.Window(0, _windowRect, DrawDebugWindow, "Debug Panel (F1)");
        }

        private void DrawDebugWindow(int windowId)
        {
            // Tabs
            GUILayout.BeginHorizontal();
            for (int i = 0; i < _tabNames.Length; i++)
            {
                if (GUILayout.Toggle(_selectedTab == i, _tabNames[i], "Button"))
                {
                    _selectedTab = i;
                }
            }
            GUILayout.EndHorizontal();

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            switch (_selectedTab)
            {
                case 0: DrawGeneralTab(); break;
                case 1: DrawPremiumTab(); break;
                case 2: DrawGamesTab(); break;
                case 3: DrawFirebaseTab(); break;
                case 4: DrawThemesTab(); break;
            }

            GUILayout.EndScrollView();

            // Botón cerrar
            if (GUILayout.Button("Cerrar (F1)"))
            {
                _panelVisible = false;
            }

            GUI.DragWindow();
        }

        #region General Tab

        private void DrawGeneralTab()
        {
            GUILayout.Label("=== Estado General ===", GUI.skin.box);

            // Usuario actual
            var playerData = AuthenticationService.Instance?.GetCurrentPlayerData();
            if (playerData != null)
            {
                GUILayout.Label($"Usuario: {playerData.username}");
                GUILayout.Label($"ID: {playerData.userId}");
                GUILayout.Label($"Games Played: {playerData.totalGamesPlayed}");
                GUILayout.Label($"Games Won: {playerData.totalGamesWon}");
                GUILayout.Label($"Best Time: {playerData.bestTime:F2}s");
            }
            else
            {
                GUILayout.Label("No hay usuario logueado");
            }

            GUILayout.Space(10);

            // Servicios
            GUILayout.Label("=== Servicios ===", GUI.skin.box);
            DrawServiceStatus("AuthenticationService", AuthenticationService.Instance != null);
            DrawServiceStatus("DatabaseService", DatabaseService.Instance != null);
            DrawServiceStatus("AnalyticsService", AnalyticsService.Instance != null);
            DrawServiceStatus("NotificationService", NotificationService.Instance != null);
            DrawServiceStatus("PremiumManager", PremiumManager.Instance != null);
            DrawServiceStatus("ThemeManager", ThemeManager.Instance != null);
            DrawServiceStatus("GameSessionManager", GameSessionManager.Instance != null);

            GUILayout.Space(10);

            // Acciones generales
            GUILayout.Label("=== Acciones ===", GUI.skin.box);

            if (GUILayout.Button("Recargar Escena"))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }

            if (GUILayout.Button("Ir a MainMenu"))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }

            if (GUILayout.Button("Limpiar PlayerPrefs"))
            {
                if (Debug.isDebugBuild)
                {
                    PlayerPrefs.DeleteAll();
                    PlayerPrefs.Save();
                    UnityEngine.Debug.Log("[Debug] PlayerPrefs limpiados");
                }
            }
        }

        private void DrawServiceStatus(string name, bool active)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name, GUILayout.Width(180));
            GUILayout.Label(active ? "OK" : "NO", active ?
                new GUIStyle(GUI.skin.label) { normal = { textColor = Color.green } } :
                new GUIStyle(GUI.skin.label) { normal = { textColor = Color.red } });
            GUILayout.EndHorizontal();
        }

        #endregion

        #region Premium Tab

        private void DrawPremiumTab()
        {
            GUILayout.Label("=== Estado Premium ===", GUI.skin.box);

            if (PremiumManager.Instance != null)
            {
                GUILayout.Label($"Create Tournaments: {PremiumManager.Instance.CanCreateTournaments}");
                GUILayout.Label($"Cash Battle Create: {PremiumManager.Instance.CanCreateCashBattle}");
                GUILayout.Label($"Styles PRO: {PremiumManager.Instance.HasStylesPro}");
                GUILayout.Label($"Is Premium: {PremiumManager.Instance.IsPremium}");

                GUILayout.Space(10);
                GUILayout.Label("=== Desbloquear (Test) ===", GUI.skin.box);

                if (GUILayout.Button("Desbloquear: Create Tournaments"))
                {
                    PremiumManager.Instance.UnlockProduct(PremiumProduct.CreateTournaments);
                }

                if (GUILayout.Button("Desbloquear: Tournament Bundle"))
                {
                    PremiumManager.Instance.UnlockProduct(PremiumProduct.TournamentBundle);
                }

                if (GUILayout.Button("Desbloquear: Cash Battle Create"))
                {
                    PremiumManager.Instance.UnlockProduct(PremiumProduct.CashBattleCreate);
                }

                if (GUILayout.Button("Desbloquear: Styles PRO"))
                {
                    PremiumManager.Instance.UnlockProduct(PremiumProduct.StylesPro);
                }

                GUILayout.Space(10);

                if (GUILayout.Button("Resetear Todo Premium"))
                {
                    PlayerPrefs.DeleteKey("DP_Premium_CreateTournaments");
                    PlayerPrefs.DeleteKey("DP_Premium_CashBattleCreate");
                    PlayerPrefs.DeleteKey("DP_Premium_StylesPro");
                    PlayerPrefs.Save();
                    UnityEngine.Debug.Log("[Debug] Estado premium reseteado - reinicia la app");
                }
            }
            else
            {
                GUILayout.Label("PremiumManager no disponible");
            }
        }

        #endregion

        #region Games Tab

        private void DrawGamesTab()
        {
            GUILayout.Label("=== Sesion de Juego ===", GUI.skin.box);

            if (GameSessionManager.Instance != null)
            {
                var context = GameSessionManager.Instance.CurrentContext;
                if (context != null)
                {
                    GUILayout.Label($"Mode: {context.Mode}");
                    GUILayout.Label($"Current Game: {context.CurrentGame}");
                    GUILayout.Label($"Game Index: {context.CurrentGameIndex + 1}/{context.Games?.Count ?? 0}");
                    GUILayout.Label($"Entry Fee: ${context.EntryFee}");
                    GUILayout.Label($"Opponent: {context.OpponentName ?? "N/A"}");
                    GUILayout.Label($"Results: {context.Results?.Count ?? 0}");
                }
                else
                {
                    GUILayout.Label("No hay sesion activa");
                }

                GUILayout.Space(10);
                GUILayout.Label("=== Iniciar Juego (Test) ===", GUI.skin.box);

                if (GUILayout.Button("Practice: DigitRush"))
                {
                    GameSessionManager.Instance.StartPracticeSession(GameType.DigitRush);
                }

                if (GUILayout.Button("Practice: MemoryPairs"))
                {
                    GameSessionManager.Instance.StartPracticeSession(GameType.MemoryPairs);
                }

                if (GUILayout.Button("Practice: QuickMath"))
                {
                    GameSessionManager.Instance.StartPracticeSession(GameType.QuickMath);
                }

                if (GUILayout.Button("Practice: FlashTap"))
                {
                    GameSessionManager.Instance.StartPracticeSession(GameType.FlashTap);
                }

                if (GUILayout.Button("Practice: OddOneOut"))
                {
                    GameSessionManager.Instance.StartPracticeSession(GameType.OddOneOut);
                }

                GUILayout.Space(10);

                if (GUILayout.Button("Cancelar Sesion"))
                {
                    GameSessionManager.Instance.CancelSession();
                }
            }
            else
            {
                GUILayout.Label("GameSessionManager no disponible");
            }

            GUILayout.Space(10);
            GUILayout.Label("=== Simular Resultado ===", GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Score:", GUILayout.Width(60));
            if (float.TryParse(GUILayout.TextField(_testScore.ToString("F2"), GUILayout.Width(80)), out float newScore))
            {
                _testScore = newScore;
            }
            GUILayout.Label("s");
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Registrar Resultado (Win)"))
            {
                SimulateGameResult(true);
            }

            if (GUILayout.Button("Registrar Resultado (Lose)"))
            {
                SimulateGameResult(false);
            }
        }

        private void SimulateGameResult(bool win)
        {
            if (GameSessionManager.Instance == null) return;

            var result = new MinigameResult
            {
                TotalTime = _testScore,
                Errors = win ? 0 : 1,
                PenaltyTime = 0f,
                Completed = win
            };

            GameSessionManager.Instance.RegisterGameResult(result);
            UnityEngine.Debug.Log($"[Debug] Resultado simulado: {_testScore}s, Completado: {win}");
        }

        #endregion

        #region Firebase Tab

        private void DrawFirebaseTab()
        {
            GUILayout.Label("=== Firebase Status ===", GUI.skin.box);

            // Auth
            if (AuthenticationService.Instance != null)
            {
                GUILayout.Label($"Auth Initialized: {AuthenticationService.Instance.IsInitialized}");
                GUILayout.Label($"User Logged In: {AuthenticationService.Instance.IsUserAuthenticated()}");
            }

            // Database
            if (DatabaseService.Instance != null)
            {
                GUILayout.Label($"Database: {(DatabaseService.Instance != null ? "OK" : "NO")}");
            }

            // Notifications
            if (NotificationService.Instance != null)
            {
                GUILayout.Label($"FCM Initialized: {NotificationService.Instance.IsInitialized}");
                string token = NotificationService.Instance.GetToken();
                if (!string.IsNullOrEmpty(token))
                {
                    GUILayout.Label($"FCM Token: {token.Substring(0, Mathf.Min(20, token.Length))}...");
                }
            }

            GUILayout.Space(10);
            GUILayout.Label("=== Test Database ===", GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Score:", GUILayout.Width(60));
            if (float.TryParse(GUILayout.TextField(_testScore.ToString("F2"), GUILayout.Width(80)), out float score))
            {
                _testScore = score;
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Guardar Score en Firebase"))
            {
                SaveTestScore();
            }

            GUILayout.Space(10);
            GUILayout.Label("=== Analytics Test ===", GUI.skin.box);

            if (GUILayout.Button("Log: login (Anonymous)"))
            {
                AnalyticsService.Instance?.LogLogin("anonymous");
            }

            if (GUILayout.Button("Log: game_start (DigitRush)"))
            {
                AnalyticsService.Instance?.LogGameStart("DigitRush");
            }

            if (GUILayout.Button("Log: game_complete"))
            {
                AnalyticsService.Instance?.LogGameComplete("DigitRush", _testScore, (int)(_testScore * 100), true, false);
            }

            if (GUILayout.Button("Log: purchase_started"))
            {
                AnalyticsService.Instance?.LogPurchaseStarted("com.test.product", 9.99, "USD");
            }
        }

        private async void SaveTestScore()
        {
            var playerData = AuthenticationService.Instance?.GetCurrentPlayerData();
            if (playerData == null)
            {
                UnityEngine.Debug.LogWarning("[Debug] No hay usuario para guardar score");
                return;
            }

            try
            {
                if (DatabaseService.Instance == null) { UnityEngine.Debug.LogWarning("[Debug] DatabaseService not available"); return; }
                await DatabaseService.Instance.SaveScore(
                    playerData.userId,
                    playerData.username,
                    _testScore,
                    playerData.countryCode ?? "MX"
                );
                UnityEngine.Debug.Log($"[Debug] Score guardado: {_testScore}s");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[Debug] Error guardando score: {e.Message}");
            }
        }

        #endregion

        #region Themes Tab

        private void DrawThemesTab()
        {
            GUILayout.Label("=== Temas ===", GUI.skin.box);

            if (ThemeManager.Instance != null)
            {
                var current = ThemeManager.Instance.CurrentTheme;
                GUILayout.Label($"Tema actual: {current?.themeName ?? "N/A"}");
                GUILayout.Label($"Total temas: {ThemeManager.Instance.AvailableThemes.Count}");
                GUILayout.Label($"Temas desbloqueados: {ThemeManager.Instance.GetAvailableThemes().Count}");
                GUILayout.Label($"Temas bloqueados: {ThemeManager.Instance.GetLockedThemes().Count}");

                GUILayout.Space(10);
                GUILayout.Label("=== Cambiar Tema ===", GUI.skin.box);

                foreach (var theme in ThemeManager.Instance.AvailableThemes)
                {
                    bool isAvailable = ThemeManager.Instance.IsThemeAvailable(theme);
                    bool isCurrent = theme == current;

                    string label = theme.themeName;
                    if (theme.isPremium) label += " [PRO]";
                    if (!isAvailable) label += " (Bloqueado)";
                    if (isCurrent) label += " *";

                    if (GUILayout.Button(label))
                    {
                        if (isAvailable)
                        {
                            ThemeManager.Instance.SetTheme(theme);
                        }
                        else
                        {
                            UnityEngine.Debug.Log($"[Debug] Tema bloqueado: {theme.themeName}");
                        }
                    }
                }

                GUILayout.Space(10);

                if (GUILayout.Button("Forzar Refresh de Tema"))
                {
                    ThemeManager.Instance.RefreshTheme();
                }
            }
            else
            {
                GUILayout.Label("ThemeManager no disponible");
            }
        }

        #endregion

        #region Public Debug Methods

        /// <summary>
        /// Desbloquea todos los productos premium (solo debug)
        /// </summary>
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        [ContextMenu("Debug: Unlock All Premium")]
        private void UnlockAllPremium()
        {
            if (PremiumManager.Instance != null)
            {
                PremiumManager.Instance.UnlockProduct(PremiumProduct.TournamentBundle);
                PremiumManager.Instance.UnlockProduct(PremiumProduct.StylesPro);
                UnityEngine.Debug.Log("[Debug] Todos los productos premium desbloqueados");
            }
        }
#endif

        /// <summary>
        /// Simula recibir una notificacion push
        /// </summary>
        public void SimulateNotification(string type, string title, string body)
        {
            var notification = new NotificationData
            {
                Type = type,
                Title = title,
                Body = body,
                ReceivedAt = DateTime.Now
            };

            UnityEngine.Debug.Log($"[Debug] Notificacion simulada: {type} - {title}");
            // TODO: Invocar evento de NotificationService si es necesario
        }

        /// <summary>
        /// Simula ganar juegos de prueba
        /// </summary>
        public void AddTestWins(int amount)
        {
            var playerData = AuthenticationService.Instance?.GetCurrentPlayerData();
            if (playerData != null)
            {
                playerData.totalGamesWon += amount;
                playerData.totalGamesPlayed += amount;
                UnityEngine.Debug.Log($"[Debug] Agregadas {amount} victorias. Total: {playerData.totalGamesWon}");
            }
        }

        /// <summary>
        /// Muestra/oculta el panel de debug
        /// </summary>
        public void TogglePanel()
        {
            _panelVisible = !_panelVisible;
        }

        #endregion

#endif
    }
}
