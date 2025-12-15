using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DigitPark.Services.Firebase;
using DigitPark.Data;
using DigitPark.UI.Common;
using DigitPark.UI.Panels;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager del menú principal
    /// Hub central de navegación simple
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        [Header("UI - Main Panel")]
        [SerializeField] public GameObject mainMenuPanel;
        [SerializeField] public TextMeshProUGUI titleText;
        [SerializeField] public Button playButton;
        [SerializeField] public Button scoresButton;
        [SerializeField] public Button tournamentsButton;
        [SerializeField] public Button settingsButton;

        [Header("UI - User Info")]
        [SerializeField] public TextMeshProUGUI userText;

        [Header("UI - Premium")]
        [SerializeField] public Button premiumButton;
        [SerializeField] public GameObject premiumBadge;
        [SerializeField] public PremiumPanelUI premiumPanel;

        [Header("Animation")]
        [SerializeField] public Animator titleAnimator;

        private PlayerData currentPlayer;

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

            // Mostrar panel principal
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);
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
            premiumButton?.onClick.AddListener(OnPremiumButtonClicked);

            // Suscribirse a cambios de premium
            PremiumManager.OnPremiumStatusChanged += UpdatePremiumUI;
        }

        private void OnDestroy()
        {
            PremiumManager.OnPremiumStatusChanged -= UpdatePremiumUI;
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
            // Mostrar nombre de usuario
            string displayUsername = string.IsNullOrEmpty(currentPlayer.username) ? "Sin Usuario" : currentPlayer.username;

            if (userText != null)
                userText.text = displayUsername;

            // Actualizar UI de premium
            UpdatePremiumUI();

            Debug.Log($"[MainMenu] UI actualizada para {displayUsername}");
        }

        /// <summary>
        /// Actualiza la UI relacionada con premium
        /// </summary>
        private void UpdatePremiumUI()
        {
            if (PremiumManager.Instance == null) return;

            bool isPremium = PremiumManager.Instance.IsPremium;

            // Mostrar/ocultar badge de premium
            if (premiumBadge != null)
                premiumBadge.SetActive(isPremium);

            // Cambiar apariencia del botón si ya es premium
            if (premiumButton != null)
            {
                var buttonImage = premiumButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    // Si es premium, cambiar a color dorado suave
                    buttonImage.color = isPremium
                        ? new Color(1f, 0.84f, 0f, 0.5f)  // Dorado semi-transparente
                        : new Color(1f, 0.84f, 0f, 1f);   // Dorado completo
                }
            }
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
        /// Muestra el panel de premium
        /// </summary>
        private void OnPremiumButtonClicked()
        {
            Debug.Log("[MainMenu] Mostrando panel Premium");

            // AudioManager.Instance?.PlaySFX("ButtonClick");

            if (premiumPanel != null)
            {
                premiumPanel.ShowWithDefaultHandlers();
            }
            else
            {
                // Crear panel por código si no está asignado
                var canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    PremiumPanelUI.CreateAndShow(canvas.transform);
                }
            }
        }

        #endregion
    }
}
