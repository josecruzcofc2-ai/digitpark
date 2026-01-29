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
        [SerializeField] public Button cashBattleButton;
        [SerializeField] public Button settingsButton;

        [Header("UI - User Info")]
        [SerializeField] public Button userButton;
        [SerializeField] public TextMeshProUGUI userText;
        [SerializeField] public Button searchButton;

        [Header("UI - Notifications")]
        [SerializeField] public Button notificationsButton;
        [SerializeField] public Image notificationIconImage;
        [SerializeField] public Sprite notificationIconNormal;
        [SerializeField] public Sprite notificationIconActive;
        [SerializeField] public TextMeshProUGUI notificationBadgeText;

        [Header("UI - Premium")]
        [SerializeField] public Button premiumButton;
        [SerializeField] public GameObject premiumBadge;
        [SerializeField] public PremiumPanelUI premiumPanel;

        [Header("Animation")]
        [SerializeField] public Animator titleAnimator;

        private PlayerData currentPlayer;
        private int pendingNotificationsCount = 0;

        private void Start()
        {
            Debug.Log("[MainMenu] MainMenuManager iniciado");

            // Analytics - Screen tracking
            AnalyticsService.Instance?.LogScreenView("MainMenu", "MainMenuManager");

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
            cashBattleButton?.onClick.AddListener(OnCashBattleButtonClicked);
            settingsButton?.onClick.AddListener(OnSettingsButtonClicked);
            premiumButton?.onClick.AddListener(OnPremiumButtonClicked);

            // User info buttons
            userButton?.onClick.AddListener(OnUserButtonClicked);
            searchButton?.onClick.AddListener(OnSearchButtonClicked);
            notificationsButton?.onClick.AddListener(OnNotificationsButtonClicked);

            // Suscribirse a cambios de premium
            PremiumManager.OnPremiumStatusChanged += UpdatePremiumUI;

            // Suscribirse a eventos de notificaciones
            if (NotificationService.Instance != null)
            {
                NotificationService.Instance.OnNotificationReceived += OnNotificationReceived;
            }
        }

        private void OnDestroy()
        {
            PremiumManager.OnPremiumStatusChanged -= UpdatePremiumUI;

            // Desuscribirse de eventos de notificaciones
            if (NotificationService.Instance != null)
            {
                NotificationService.Instance.OnNotificationReceived -= OnNotificationReceived;
            }
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

        /// <summary>
        /// Callback cuando se recibe una notificación
        /// </summary>
        private void OnNotificationReceived(NotificationData notification)
        {
            pendingNotificationsCount++;
            UpdateNotificationIcon();
            Debug.Log($"[MainMenu] Notificación recibida: {notification.Title}");
        }

        /// <summary>
        /// Actualiza el icono de notificaciones según el contador
        /// </summary>
        public void UpdateNotificationIcon()
        {
            bool hasNotifications = pendingNotificationsCount > 0;

            // Cambiar sprite del icono
            if (notificationIconImage != null)
            {
                notificationIconImage.sprite = hasNotifications
                    ? notificationIconActive
                    : notificationIconNormal;
            }

            // Actualizar badge de texto (opcional)
            if (notificationBadgeText != null)
            {
                notificationBadgeText.gameObject.SetActive(hasNotifications);
                if (hasNotifications)
                {
                    notificationBadgeText.text = pendingNotificationsCount > 99
                        ? "99+"
                        : pendingNotificationsCount.ToString();
                }
            }
        }

        /// <summary>
        /// Establece el contador de notificaciones pendientes
        /// </summary>
        public void SetNotificationCount(int count)
        {
            pendingNotificationsCount = Mathf.Max(0, count);
            UpdateNotificationIcon();
        }

        /// <summary>
        /// Limpia todas las notificaciones pendientes
        /// </summary>
        public void ClearNotifications()
        {
            pendingNotificationsCount = 0;
            UpdateNotificationIcon();
        }

        #region Button Callbacks

        /// <summary>
        /// Abre la selección de modo de juego (Solo, 1v1, Torneos)
        /// </summary>
        private void OnPlayButtonClicked()
        {
            Debug.Log("[MainMenu] Abriendo selección de modo de juego");

            // Efecto de sonido
            // AudioManager.Instance?.PlaySFX("ButtonClick");

            // Cargar la escena de selección de modo
            SceneManager.LoadScene("PlayModeSelection");
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
        /// Navega a la escena de Cash Battle (dinero real - 18+)
        /// Verifica primero si el usuario tiene 18+ años
        /// </summary>
        private void OnCashBattleButtonClicked()
        {
            Debug.Log("[MainMenu] Intentando acceder a Cash Battle");

            // Verificar si el usuario esta verificado (18+)
            var kycService = DigitPark.Services.ServiceLocator.KYC;
            bool isVerified = kycService?.CanAccessCashBattle ?? false;

            if (!isVerified)
            {
                // Usuario NO verificado - ir a verificacion de edad primero
                Debug.Log("[MainMenu] Usuario no verificado - navegando a AgeVerification");
                SceneManager.LoadScene("AgeVerification");
            }
            else
            {
                // Usuario verificado - ir directo al Hub
                Debug.Log("[MainMenu] Usuario verificado - navegando a CashBattleHub");
                SceneManager.LoadScene("CashBattleHub");
            }
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
        /// Muestra el perfil del usuario actual
        /// </summary>
        private void OnUserButtonClicked()
        {
            Debug.Log("[MainMenu] Abriendo perfil de usuario");

            // AudioManager.Instance?.PlaySFX("ButtonClick");

            // TODO: Abrir panel de perfil o navegar a escena Profile
            SceneManager.LoadScene("Profile");
        }

        /// <summary>
        /// Abre el buscador de jugadores
        /// </summary>
        private void OnSearchButtonClicked()
        {
            Debug.Log("[MainMenu] Abriendo buscador de jugadores");

            // AudioManager.Instance?.PlaySFX("ButtonClick");

            // TODO: Abrir panel de búsqueda de jugadores
            SceneManager.LoadScene("SearchPlayers");
        }

        /// <summary>
        /// Abre el panel/escena de notificaciones
        /// </summary>
        private void OnNotificationsButtonClicked()
        {
            Debug.Log("[MainMenu] Abriendo notificaciones");

            // AudioManager.Instance?.PlaySFX("ButtonClick");

            // Limpiar contador al ver notificaciones
            ClearNotifications();

            // TODO: Abrir panel de notificaciones o navegar a escena
            // Por ahora, navegar a Profile con panel de notificaciones
            PlayerPrefs.SetString("OpenPanel", "Notifications");
            SceneManager.LoadScene("Profile");
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
