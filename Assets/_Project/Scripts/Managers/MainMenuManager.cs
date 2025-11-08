using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DigitPark.Services.Firebase;
using DigitPark.Data;
using DigitPark.UI.Common;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager del menú principal
    /// Hub central de navegación y display de información del jugador
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        [Header("UI - Title")]
        [SerializeField] public TextMeshProUGUI titleText;
        [SerializeField] public Animator titleAnimator;

        [Header("UI - Player Info")]
        [SerializeField] public TextMeshProUGUI usernameText;
        [SerializeField] public Button usernameButton; // Botón clickeable para username
        [SerializeField] public TextMeshProUGUI levelText;
        [SerializeField] public TextMeshProUGUI coinsText;
        [SerializeField] public TextMeshProUGUI gemsText;
        [SerializeField] public Image experienceBar;
        [SerializeField] public Image avatarImage;

        [Header("UI - Main Buttons")]
        [SerializeField] public Button playButton;
        [SerializeField] public Button scoresButton;
        [SerializeField] public Button tournamentsButton;
        [SerializeField] public Button settingsButton;

        [Header("UI - Additional")]
        [SerializeField] public Button profileButton;
        [SerializeField] public Button dailyRewardButton;
        [SerializeField] public Button achievementsButton;
        [SerializeField] public GameObject dailyRewardNotification;

        [Header("UI - Statistics")]
        [SerializeField] public TextMeshProUGUI bestTimeText;
        [SerializeField] public TextMeshProUGUI gamesPlayedText;
        [SerializeField] public TextMeshProUGUI worldRankText;

        private PlayerData currentPlayer;
        private UsernamePopup usernamePopup;

        private void Start()
        {
            Debug.Log("[MainMenu] MainMenuManager iniciado");

            // Verificar e inicializar servicios si no existen (para testing directo)
            EnsureServicesExist();

            // Configurar listeners
            SetupListeners();

            // Cargar datos del jugador
            LoadPlayerData();

            // Animar entrada del título
            if (titleAnimator != null)
            {
                titleAnimator.SetTrigger("Show");
            }

            // Crear popup de username (oculto inicialmente)
            CreateUsernamePopup();

            // Verificar daily reward
            CheckDailyReward();
        }

        /// <summary>
        /// Crea el popup de username
        /// </summary>
        private void CreateUsernamePopup()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                usernamePopup = UsernamePopup.Create(canvas.transform);
                usernamePopup.Hide();
            }
        }

        /// <summary>
        /// Configura los listeners de los botones
        /// </summary>
        private void SetupListeners()
        {
            playButton?.onClick.AddListener(OnPlayButtonClicked);
            scoresButton?.onClick.AddListener(OnScoresButtonClicked);
            tournamentsButton?.onClick.AddListener(OnTournamentsButtonClicked);
            settingsButton?.onClick.AddListener(OnSettingsButtonClicked);
            profileButton?.onClick.AddListener(OnProfileButtonClicked);
            dailyRewardButton?.onClick.AddListener(OnDailyRewardButtonClicked);
            achievementsButton?.onClick.AddListener(OnAchievementsButtonClicked);
            usernameButton?.onClick.AddListener(OnUsernameButtonClicked);
        }

        /// <summary>
        /// Asegura que los servicios existan (para testing directo de escena)
        /// </summary>
        private void EnsureServicesExist()
        {
            if (AuthenticationService.Instance == null)
            {
                Debug.LogWarning("[MainMenu] AuthenticationService no encontrado, creando instancia de respaldo...");
                GameObject authService = new GameObject("AuthenticationService");
                authService.AddComponent<AuthenticationService>();
            }

            if (DatabaseService.Instance == null)
            {
                Debug.LogWarning("[MainMenu] DatabaseService no encontrado, creando instancia de respaldo...");
                GameObject dbService = new GameObject("DatabaseService");
                dbService.AddComponent<DatabaseService>();
            }
        }

        /// <summary>
        /// Carga los datos del jugador actual
        /// </summary>
        private void LoadPlayerData()
        {
            if (AuthenticationService.Instance == null)
            {
                Debug.LogError("[MainMenu] AuthenticationService no disponible después de inicialización");
                return;
            }

            currentPlayer = AuthenticationService.Instance.GetCurrentPlayerData();

            if (currentPlayer == null)
            {
                Debug.LogError("[MainMenu] No hay datos del jugador");
                // Volver al login
                SceneManager.LoadScene("Login");
                return;
            }

            UpdateUI();
        }

        /// <summary>
        /// Actualiza toda la UI con los datos del jugador
        /// </summary>
        private void UpdateUI()
        {
            // Información del jugador
            string displayUsername = string.IsNullOrEmpty(currentPlayer.username) ? "Sin usuario" : currentPlayer.username;

            if (usernameText != null)
                usernameText.text = displayUsername;

            if (levelText != null)
                levelText.text = $"Nivel {currentPlayer.level}";

            if (coinsText != null)
                coinsText.text = currentPlayer.coins.ToString();

            if (gemsText != null)
                gemsText.text = currentPlayer.gems.ToString();

            // Barra de experiencia
            if (experienceBar != null)
            {
                float expPercentage = (float)currentPlayer.experience / currentPlayer.experienceToNextLevel;
                experienceBar.fillAmount = expPercentage;
            }

            // Estadísticas
            if (bestTimeText != null)
            {
                if (currentPlayer.bestTime < float.MaxValue)
                    bestTimeText.text = $"Mejor: {currentPlayer.bestTime:F3}s";
                else
                    bestTimeText.text = "Mejor: --";
            }

            if (gamesPlayedText != null)
                gamesPlayedText.text = $"Partidas: {currentPlayer.totalGamesPlayed}";

            // Avatar (cargar desde URL o usar default)
            if (avatarImage != null && !string.IsNullOrEmpty(currentPlayer.avatarUrl))
            {
                // Aquí iría la lógica para cargar la imagen desde URL
                // StartCoroutine(LoadAvatarFromURL(currentPlayer.avatarUrl));
            }

            Debug.Log($"[MainMenu] UI actualizada para {currentPlayer.username}");
        }

        #region Button Callbacks

        /// <summary>
        /// Inicia una partida
        /// </summary>
        private void OnPlayButtonClicked()
        {
            Debug.Log("[MainMenu] Iniciando partida");

            // Efecto de sonido
            // AudioManager.Instance?.PlaySFX("ButtonClick");

            // Registrar en analytics
            AnalyticsService.Instance?.LogGameStart();

            // Cargar escena de juego
            SceneManager.LoadScene("Game");
        }

        /// <summary>
        /// Navega a la escena de scores/rankings
        /// </summary>
        private void OnScoresButtonClicked()
        {
            Debug.Log("[MainMenu] Navegando a Scores");

            // AudioManager.Instance?.PlaySFX("ButtonClick");

            SceneManager.LoadScene("Scores");
        }

        /// <summary>
        /// Navega a la escena de torneos
        /// </summary>
        private void OnTournamentsButtonClicked()
        {
            Debug.Log("[MainMenu] Navegando a Tournaments");

            // AudioManager.Instance?.PlaySFX("ButtonClick");

            SceneManager.LoadScene("Tournaments");
        }

        /// <summary>
        /// Navega a la escena de configuración
        /// </summary>
        private void OnSettingsButtonClicked()
        {
            Debug.Log("[MainMenu] Navegando a Settings");

            // AudioManager.Instance?.PlaySFX("ButtonClick");

            SceneManager.LoadScene("Settings");
        }

        /// <summary>
        /// Abre el perfil del jugador
        /// </summary>
        private void OnProfileButtonClicked()
        {
            Debug.Log("[MainMenu] Abriendo perfil");

            // Aquí se abriría un panel de perfil
            // ProfilePanel.Instance?.Show(currentPlayer);
        }

        /// <summary>
        /// Maneja el click en el botón de username
        /// </summary>
        private void OnUsernameButtonClicked()
        {
            Debug.Log("[MainMenu] Click en username");

            // Solo mostrar popup si el usuario NO tiene username o es "Sin usuario"
            if (string.IsNullOrEmpty(currentPlayer.username) || currentPlayer.username == "Sin usuario")
            {
                Debug.Log("[MainMenu] Mostrando popup para elegir username");

                usernamePopup?.ShowForUpdate(
                    onConfirm: async (username) =>
                    {
                        Debug.Log($"[MainMenu] Usuario eligió username: {username}");

                        // Actualizar username
                        bool success = await AuthenticationService.Instance.UpdateUsername(username);

                        if (success)
                        {
                            currentPlayer.username = username;
                            UpdateUI(); // Actualizar la UI con el nuevo nombre
                            Debug.Log("[MainMenu] Username actualizado correctamente");
                        }
                        else
                        {
                            Debug.LogError("[MainMenu] Error al actualizar username");
                        }
                    },
                    onCancel: () =>
                    {
                        Debug.Log("[MainMenu] Usuario canceló cambio de username");
                    }
                );
            }
            else
            {
                // Si ya tiene username, abrir perfil
                OnProfileButtonClicked();
            }
        }

        /// <summary>
        /// Reclama la recompensa diaria
        /// </summary>
        private void OnDailyRewardButtonClicked()
        {
            Debug.Log("[MainMenu] Reclamando recompensa diaria");

            if (!CanClaimDailyReward())
            {
                Debug.LogWarning("[MainMenu] Recompensa diaria ya reclamada hoy");
                return;
            }

            ClaimDailyReward();
        }

        /// <summary>
        /// Abre el panel de logros
        /// </summary>
        private void OnAchievementsButtonClicked()
        {
            Debug.Log("[MainMenu] Abriendo logros");

            // Aquí se abriría un panel de logros
            // AchievementsPanel.Instance?.Show(currentPlayer);
        }

        #endregion

        #region Daily Rewards

        /// <summary>
        /// Verifica si hay recompensa diaria disponible
        /// </summary>
        private void CheckDailyReward()
        {
            if (currentPlayer == null) return;

            bool canClaim = CanClaimDailyReward();

            if (dailyRewardNotification != null)
            {
                dailyRewardNotification.SetActive(canClaim);
            }

            if (dailyRewardButton != null)
            {
                dailyRewardButton.interactable = canClaim;
            }

            Debug.Log($"[MainMenu] Recompensa diaria disponible: {canClaim}");
        }

        /// <summary>
        /// Verifica si se puede reclamar la recompensa diaria
        /// </summary>
        private bool CanClaimDailyReward()
        {
            if (currentPlayer == null) return false;

            // Verificar si ya se reclamó hoy
            System.DateTime lastClaim = currentPlayer.lastDailyRewardClaimed;
            System.DateTime today = System.DateTime.Now.Date;

            return lastClaim.Date < today;
        }

        /// <summary>
        /// Reclama la recompensa diaria
        /// </summary>
        private async void ClaimDailyReward()
        {
            if (currentPlayer == null) return;

            // Verificar streak de días consecutivos
            System.DateTime yesterday = System.DateTime.Now.AddDays(-1).Date;
            if (currentPlayer.lastDailyRewardClaimed.Date == yesterday)
            {
                currentPlayer.consecutiveLoginDays++;
            }
            else
            {
                currentPlayer.consecutiveLoginDays = 1;
            }

            // Calcular recompensa (aumenta con el streak)
            int coinsReward = 50 + (currentPlayer.consecutiveLoginDays * 10);
            int gemsReward = currentPlayer.consecutiveLoginDays >= 7 ? 5 : 0;

            // Dar recompensas
            currentPlayer.coins += coinsReward;
            currentPlayer.gems += gemsReward;
            currentPlayer.lastDailyRewardClaimed = System.DateTime.Now;

            // Guardar en Firebase
            await DatabaseService.Instance.SavePlayerData(currentPlayer);

            // Registrar en analytics
            AnalyticsService.Instance?.LogCoinsEarned(coinsReward, "daily_reward");

            // Actualizar UI
            UpdateUI();

            // Mostrar recompensa al usuario
            Debug.Log($"[MainMenu] Recompensa diaria reclamada: {coinsReward} coins, {gemsReward} gems");

            // Aquí se mostraría un popup con la recompensa
            // RewardPopup.Instance?.Show(coinsReward, gemsReward, currentPlayer.consecutiveLoginDays);

            // Ocultar notificación
            if (dailyRewardNotification != null)
            {
                dailyRewardNotification.SetActive(false);
            }
        }

        #endregion

        #region World Rank

        /// <summary>
        /// Actualiza el ranking mundial del jugador
        /// </summary>
        private async void UpdateWorldRank()
        {
            if (currentPlayer == null || worldRankText == null) return;

            // Obtener leaderboard global
            var leaderboard = await DatabaseService.Instance.GetGlobalLeaderboard(200);

            // Buscar posición del jugador
            int position = leaderboard.FindIndex(entry => entry.userId == currentPlayer.userId);

            if (position >= 0)
            {
                worldRankText.text = $"Rank #{position + 1}";
            }
            else
            {
                worldRankText.text = "Rank: --";
            }
        }

        #endregion

        #region Notifications

        /// <summary>
        /// Verifica y muestra notificaciones pendientes
        /// </summary>
        private void CheckNotifications()
        {
            // Verificar torneos completados
            // Verificar nuevos logros
            // Verificar mensajes del sistema
        }

        #endregion
    }
}
