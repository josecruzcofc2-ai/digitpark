using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using DigitPark.Animations;
using DigitPark.Services;
using DigitPark.Services.Firebase;
using DigitPark.Data;
using DigitPark.UI;
using DigitPark.UI.Common;
using DigitPark.UI.Panels;
using DigitPark.Localization;
using DigitPark.Monetization;
using DigitPark.Navigation;

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
        [SerializeField] public GameObject notificationBadge;
        [SerializeField] public TextMeshProUGUI notificationBadgeText;

        [Header("UI - Monetization")]
        [SerializeField] public Button shopButton;
        [SerializeField] public Button achievementsButton;
        [SerializeField] public Button dailyMissionsButton;
        [SerializeField] public Button dailyRewardsButton;
        [SerializeField] public Button missionsCardButton;

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

            // Mostrar panel principal con fade in
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(true);
                var cg = mainMenuPanel.GetComponent<CanvasGroup>();
                if (cg == null) cg = mainMenuPanel.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                cg.DOFade(1f, 0.4f).SetEase(Ease.OutQuad).SetLink(gameObject);
            }
        }

        /// <summary>
        /// Configura los listeners de los botones
        /// </summary>
        private void SetupListeners()
        {
            playButton?.onClick.AddListener(OnPlayButtonClicked);
            scoresButton?.onClick.AddListener(OnScoresButtonClicked);
            settingsButton?.onClick.AddListener(OnSettingsButtonClicked);
            premiumButton?.onClick.AddListener(OnPremiumButtonClicked);
            shopButton?.onClick.AddListener(OnShopButtonClicked);
            achievementsButton?.onClick.AddListener(OnAchievementsButtonClicked);
            dailyMissionsButton?.onClick.AddListener(OnDailyMissionsButtonClicked);
            dailyRewardsButton?.onClick.AddListener(OnDailyRewardsButtonClicked);
            missionsCardButton?.onClick.AddListener(OnDailyMissionsButtonClicked);

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

            // Suscribirse a cambios en el almacenamiento de notificaciones
            if (NotificationStorageService.Instance != null)
            {
                NotificationStorageService.Instance.OnUnreadCountChanged += OnUnreadCountChanged;
                // Sincronizar badge con el conteo real persistido
                SetNotificationCount(NotificationStorageService.Instance.GetUnreadCount());
            }
        }

        private void OnDestroy()
        {
            DOTween.Kill(transform);
            PremiumManager.OnPremiumStatusChanged -= UpdatePremiumUI;

            // Desuscribirse de eventos de notificaciones
            if (NotificationService.Instance != null)
            {
                NotificationService.Instance.OnNotificationReceived -= OnNotificationReceived;
            }

            if (NotificationStorageService.Instance != null)
            {
                NotificationStorageService.Instance.OnUnreadCountChanged -= OnUnreadCountChanged;
            }
        }

        private void OnUnreadCountChanged(int count)
        {
            SetNotificationCount(count);
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
                SceneNavigator.Instance?.NavigateTo("Login");
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
            string displayUsername = string.IsNullOrEmpty(currentPlayer.username) ? AutoLocalizer.Get("no_username") : currentPlayer.username;

            if (userText != null)
                userText.text = displayUsername;

            // Actualizar UI de premium
            UpdatePremiumUI();

            Debug.Log($"[MainMenu] UI actualizada para {displayUsername}");
        }

        /// <summary>
        /// Actualiza la UI relacionada con premium.
        /// El botón Premium siempre es visible (para que no-premium pueda comprarlo).
        /// Solo cambia la apariencia según el estado.
        /// </summary>
        private void UpdatePremiumUI()
        {
            if (PremiumManager.Instance == null) return;

            bool isPremium = PremiumManager.Instance.IsPremium;

            // Cambiar apariencia del botón según estado premium
            if (premiumButton != null)
            {
                var buttonImage = premiumButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    // Si es premium, cambiar a color dorado suave (indica activo)
                    buttonImage.color = isPremium
                        ? new Color(1f, 0.84f, 0f, 0.5f)  // Dorado semi-transparente
                        : new Color(1f, 0.84f, 0f, 1f);   // Dorado completo
                }
            }

            // El premiumBadge es un indicador visual (ej: estrella/checkmark)
            // Solo mostrarlo si ES premium, pero nunca ocultar el botón padre
            if (premiumBadge != null && premiumBadge != premiumButton?.gameObject)
            {
                if (isPremium && !premiumBadge.activeSelf)
                {
                    premiumBadge.SetActive(true);
                    premiumBadge.transform.localScale = Vector3.zero;
                    premiumBadge.transform.DOScale(1f, AnimConstants.DURATION_ENTER).SetEase(AnimConstants.ENTER).SetLink(premiumBadge);
                }
                else if (!isPremium && premiumBadge.activeSelf)
                {
                    premiumBadge.transform.DOScale(0f, AnimConstants.DURATION_QUICK).SetEase(AnimConstants.EXIT)
                        .SetLink(premiumBadge).OnComplete(() => premiumBadge.SetActive(false));
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

            // Mostrar/ocultar badge rojo con BadgeAnimator
            if (notificationBadge != null)
            {
                var badgeAnim = notificationBadge.GetComponent<DigitPark.Animations.BadgeAnimator>();
                if (badgeAnim != null)
                {
                    if (hasNotifications && !notificationBadge.activeSelf)
                        badgeAnim.Show();
                    else if (!hasNotifications && notificationBadge.activeSelf)
                        badgeAnim.Hide();
                    else if (hasNotifications && notificationBadge.activeSelf)
                        badgeAnim.PlayUpdate();
                }
                else
                {
                    notificationBadge.SetActive(hasNotifications);
                }
            }

            // Actualizar texto del badge
            if (notificationBadgeText != null && hasNotifications)
            {
                notificationBadgeText.text = pendingNotificationsCount > 99
                    ? "99+"
                    : pendingNotificationsCount.ToString();
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
            SceneNavigator.Instance?.NavigateTo("PlayModeSelection");
        }

        /// <summary>
        /// Navega a la escena de scores/rankings
        /// </summary>
        private void OnScoresButtonClicked()
        {
            Debug.Log("[MainMenu] Navegando a Scores");

            // AudioManager.Instance?.PlaySFX("ButtonClick");

            SceneNavigator.Instance?.NavigateTo("Scores");
        }

        /// <summary>
        /// Navega a la escena de configuración
        /// </summary>
        private void OnSettingsButtonClicked()
        {
            Debug.Log("[MainMenu] Navegando a Settings");

            // AudioManager.Instance?.PlaySFX("ButtonClick");

            SceneNavigator.Instance?.NavigateTo("Settings");
        }

        /// <summary>
        /// Muestra el perfil del usuario actual
        /// </summary>
        private void OnUserButtonClicked()
        {
            Debug.Log("[MainMenu] Abriendo perfil de usuario");

            // AudioManager.Instance?.PlaySFX("ButtonClick");

            SceneNavigator.Instance?.NavigateTo("Profile");
        }

        /// <summary>
        /// Abre el buscador de jugadores
        /// </summary>
        private void OnSearchButtonClicked()
        {
            Debug.Log("[MainMenu] Abriendo buscador de jugadores");

            // AudioManager.Instance?.PlaySFX("ButtonClick");

            SceneNavigator.Instance?.NavigateTo("SearchPlayers");
        }

        /// <summary>
        /// Abre el panel/escena de notificaciones
        /// </summary>
        private void OnNotificationsButtonClicked()
        {
            Debug.Log("[MainMenu] Abriendo notificaciones");

            // Limpiar contador visual al ver notificaciones
            ClearNotifications();

            // Navegar a la escena de Notificaciones
            PlayerPrefs.SetString("DP_NotificationsReturnScene", "MainMenu");
            SceneNavigator.Instance?.NavigateTo("Notifications");
        }

        /// <summary>
        /// Muestra el panel de premium
        /// </summary>
        private void OnShopButtonClicked()
        {
            Debug.Log("[MainMenu] Navegando a Shop");
            SceneNavigator.Instance?.NavigateTo("Shop");
        }

        private void OnAchievementsButtonClicked()
        {
            Debug.Log("[MainMenu] Navegando a Achievements");
            SceneNavigator.Instance?.NavigateTo("Achievements");
        }

        private void OnDailyMissionsButtonClicked()
        {
            Debug.Log("[MainMenu] Navegando a DailyMissions");
            SceneNavigator.Instance?.NavigateTo("DailyMissions");
        }

        private void OnDailyRewardsButtonClicked()
        {
            Debug.Log("[MainMenu] Navegando a DailyRewards");
            SceneNavigator.Instance?.NavigateTo("DailyRewards");
        }

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
                Canvas canvas = UICanvasHelper.FindMainCanvas();
                if (canvas != null)
                {
                    PremiumPanelUI.CreateAndShow(canvas.transform);
                }
            }
        }

        #endregion
    }
}
