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
    /// Manager de la escena de Settings
    /// Gestiona todas las configuraciones del juego y perfil del usuario
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] public GameObject accountPanel;
        [SerializeField] public GameObject gamePanel;
        [SerializeField] public GameObject visualPanel;
        [SerializeField] public GameObject languagePanel;
        [SerializeField] public GameObject logoutPanel;

        [Header("Navigation Buttons")]
        [SerializeField] public Button accountButton;
        [SerializeField] public Button gameButton;
        [SerializeField] public Button visualButton;
        [SerializeField] public Button languageButton;
        [SerializeField] public Button logoutTabButton;
        [SerializeField] public Button backButton;

        [Header("Account Settings")]
        [SerializeField] public TMP_InputField usernameInput;
        [SerializeField] public Button changeUsernameButton;
        [SerializeField] public TextMeshProUGUI userIdText;
        [SerializeField] public TextMeshProUGUI emailText;
        [SerializeField] public Button logoutButton;

        [Header("Game Settings")]
        [SerializeField] public Slider musicVolumeSlider;
        [SerializeField] public Slider sfxVolumeSlider;
        [SerializeField] public TextMeshProUGUI musicVolumeText;
        [SerializeField] public TextMeshProUGUI sfxVolumeText;
        [SerializeField] public Toggle vibrationToggle;
        [SerializeField] public Toggle notificationsToggle;

        [Header("Visual Settings")]
        [SerializeField] public TMP_Dropdown themeDropdown;
        [SerializeField] public TMP_Dropdown qualityDropdown;
        [SerializeField] public TMP_Dropdown fpsDropdown;

        [Header("Language Settings")]
        [SerializeField] public TMP_Dropdown languageDropdown;

        [Header("Premium")]
        [SerializeField] public GameObject premiumPanel;
        [SerializeField] public Button buyPremiumButton;
        [SerializeField] public TextMeshProUGUI premiumStatusText;

        // Estado
        private SettingsPanel currentPanel = SettingsPanel.Account;
        private PlayerData currentPlayer;
        private PlayerSettings settings;
        private ConfirmationPopup confirmationPopup;

        private void Start()
        {
            Debug.Log("[Settings] SettingsManager iniciado");

            // Crear popup de confirmación
            CreateConfirmationPopup();

            // Cargar datos del jugador
            LoadPlayerData();

            // Configurar listeners
            SetupListeners();

            // Cargar configuraciones
            LoadSettings();

            // Mostrar panel inicial
            ShowPanel(SettingsPanel.Account);
        }

        /// <summary>
        /// Crea el popup de confirmación
        /// </summary>
        private void CreateConfirmationPopup()
        {
            // Buscar el canvas principal
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                confirmationPopup = ConfirmationPopup.Create(canvas.transform);
                confirmationPopup.Hide(); // Ocultar inicialmente
            }
        }

        #region Initialization

        /// <summary>
        /// Carga los datos del jugador
        /// </summary>
        private void LoadPlayerData()
        {
            if (AuthenticationService.Instance != null)
            {
                currentPlayer = AuthenticationService.Instance.GetCurrentPlayerData();

                if (currentPlayer != null)
                {
                    settings = currentPlayer.settings;
                    UpdateAccountInfo();
                }
            }
        }

        /// <summary>
        /// Configura los listeners
        /// </summary>
        private void SetupListeners()
        {
            // Navegación
            accountButton?.onClick.AddListener(() => ShowPanel(SettingsPanel.Account));
            gameButton?.onClick.AddListener(() => ShowPanel(SettingsPanel.Game));
            visualButton?.onClick.AddListener(() => ShowPanel(SettingsPanel.Visual));
            languageButton?.onClick.AddListener(() => ShowPanel(SettingsPanel.Language));
            logoutTabButton?.onClick.AddListener(() => ShowPanel(SettingsPanel.Logout));
            backButton?.onClick.AddListener(OnBackButtonClicked);

            // Account
            changeUsernameButton?.onClick.AddListener(OnChangeUsernameClicked);
            logoutButton?.onClick.AddListener(OnLogoutClicked);

            // Game
            musicVolumeSlider?.onValueChanged.AddListener(OnMusicVolumeChanged);
            sfxVolumeSlider?.onValueChanged.AddListener(OnSFXVolumeChanged);
            vibrationToggle?.onValueChanged.AddListener(OnVibrationToggled);
            notificationsToggle?.onValueChanged.AddListener(OnNotificationsToggled);

            // Visual
            themeDropdown?.onValueChanged.AddListener(OnThemeChanged);
            qualityDropdown?.onValueChanged.AddListener(OnQualityChanged);
            fpsDropdown?.onValueChanged.AddListener(OnFPSChanged);

            // Language
            languageDropdown?.onValueChanged.AddListener(OnLanguageChanged);

            // Premium
            buyPremiumButton?.onClick.AddListener(OnBuyPremiumClicked);
        }

        #endregion

        #region Panel Navigation

        /// <summary>
        /// Muestra el panel seleccionado
        /// </summary>
        private void ShowPanel(SettingsPanel panel)
        {
            Debug.Log($"[Settings] Mostrando panel: {panel}");

            currentPanel = panel;

            // Ocultar todos
            accountPanel?.SetActive(false);
            gamePanel?.SetActive(false);
            visualPanel?.SetActive(false);
            languagePanel?.SetActive(false);
            logoutPanel?.SetActive(false);

            // Mostrar el seleccionado
            switch (panel)
            {
                case SettingsPanel.Account:
                    accountPanel?.SetActive(true);
                    break;
                case SettingsPanel.Game:
                    gamePanel?.SetActive(true);
                    break;
                case SettingsPanel.Visual:
                    visualPanel?.SetActive(true);
                    break;
                case SettingsPanel.Language:
                    languagePanel?.SetActive(true);
                    break;
                case SettingsPanel.Logout:
                    logoutPanel?.SetActive(true);
                    break;
            }

            UpdateNavigationVisuals();
        }

        /// <summary>
        /// Actualiza los visuales de navegación
        /// </summary>
        private void UpdateNavigationVisuals()
        {
            SetButtonState(accountButton, currentPanel == SettingsPanel.Account);
            SetButtonState(gameButton, currentPanel == SettingsPanel.Game);
            SetButtonState(visualButton, currentPanel == SettingsPanel.Visual);
            SetButtonState(languageButton, currentPanel == SettingsPanel.Language);
            SetButtonState(logoutTabButton, currentPanel == SettingsPanel.Logout);
        }

        /// <summary>
        /// Establece el estado visual de un botón
        /// </summary>
        private void SetButtonState(Button button, bool isSelected)
        {
            if (button == null) return;

            ColorBlock colors = button.colors;
            colors.normalColor = isSelected ? new Color(0f, 0.83f, 1f) : Color.gray;
            button.colors = colors;
        }

        #endregion

        #region Load/Save Settings

        /// <summary>
        /// Carga todas las configuraciones
        /// </summary>
        private void LoadSettings()
        {
            if (settings == null) return;

            Debug.Log("[Settings] Cargando configuraciones...");

            // Audio
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = settings.musicVolume;
                OnMusicVolumeChanged(settings.musicVolume);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = settings.sfxVolume;
                OnSFXVolumeChanged(settings.sfxVolume);
            }

            // Otros toggles
            if (vibrationToggle != null)
                vibrationToggle.isOn = settings.vibrationEnabled;

            if (notificationsToggle != null)
                notificationsToggle.isOn = settings.pushNotificationsEnabled;

            // Visual
            if (themeDropdown != null)
                themeDropdown.value = (int)settings.theme;

            if (qualityDropdown != null)
                qualityDropdown.value = (int)settings.graphicsQuality;

            if (fpsDropdown != null)
            {
                fpsDropdown.value = settings.targetFPS == 30 ? 0 : 1;
            }

            // Idioma
            if (languageDropdown != null)
            {
                languageDropdown.value = GetLanguageIndex(settings.language);
            }

            Debug.Log("[Settings] Configuraciones cargadas");
        }

        /// <summary>
        /// Guarda las configuraciones
        /// </summary>
        private async void SaveSettings()
        {
            if (currentPlayer == null) return;

            Debug.Log("[Settings] Guardando configuraciones...");

            // Aplicar configuraciones
            settings.Apply();

            // Guardar en Firebase
            await DatabaseService.Instance.SavePlayerData(currentPlayer);

            Debug.Log("[Settings] Configuraciones guardadas");
        }

        #endregion

        #region Account Settings

        /// <summary>
        /// Actualiza la información de cuenta en la UI
        /// </summary>
        private void UpdateAccountInfo()
        {
            if (currentPlayer == null) return;

            if (userIdText != null)
                userIdText.text = $"ID: {currentPlayer.userId}";

            if (emailText != null)
                emailText.text = currentPlayer.email;

            if (usernameInput != null)
                usernameInput.text = currentPlayer.username;

            // Premium status
            UpdatePremiumStatus();
        }

        /// <summary>
        /// Cambia el nombre de usuario
        /// </summary>
        private void OnChangeUsernameClicked()
        {
            string newUsername = usernameInput?.text.Trim();

            if (string.IsNullOrEmpty(newUsername))
            {
                Debug.LogWarning("[Settings] Nombre de usuario vacío");
                return;
            }

            if (newUsername.Length < 3)
            {
                Debug.LogWarning("[Settings] Nombre muy corto");
                return;
            }

            // Si el nombre no ha cambiado, no hacer nada
            if (currentPlayer != null && newUsername == currentPlayer.username)
            {
                Debug.Log("[Settings] El nombre no ha cambiado");
                return;
            }

            // Mostrar popup de confirmación
            string currentUsername = currentPlayer?.username ?? "Sin nombre";
            confirmationPopup?.ShowWithValues(
                "CAMBIAR NOMBRE",
                "¿Confirmas el cambio de nombre?",
                currentUsername,
                newUsername,
                () => ConfirmUsernameChange(newUsername)
            );
        }

        /// <summary>
        /// Confirma y ejecuta el cambio de nombre
        /// </summary>
        private async void ConfirmUsernameChange(string newUsername)
        {
            Debug.Log($"[Settings] Cambiando nombre a: {newUsername}");

            bool success = await AuthenticationService.Instance.UpdateUsername(newUsername);

            if (success)
            {
                currentPlayer.username = newUsername;
                Debug.Log("[Settings] Nombre actualizado");
            }
            else
            {
                Debug.LogError("[Settings] Error al actualizar nombre");
            }
        }

        /// <summary>
        /// Cierra sesión
        /// </summary>
        private void OnLogoutClicked()
        {
            Debug.Log("[Settings] Mostrando confirmación de logout...");

            // Crear popup temporal de logout
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                LogoutConfirmationPopup logoutPopup = LogoutConfirmationPopup.Create(canvas.transform);
                logoutPopup.Show(() => ConfirmLogout());
            }
            else
            {
                // Si no hay canvas, cerrar sesión directamente
                ConfirmLogout();
            }
        }

        /// <summary>
        /// Confirma y ejecuta el cierre de sesión
        /// </summary>
        private void ConfirmLogout()
        {
            Debug.Log("[Settings] Cerrando sesión...");

            AuthenticationService.Instance?.Logout();

            SceneManager.LoadScene("Login");
        }

        #endregion

        #region Game Settings

        /// <summary>
        /// Cambia el volumen de música
        /// </summary>
        private void OnMusicVolumeChanged(float value)
        {
            settings.musicVolume = value;

            if (musicVolumeText != null)
                musicVolumeText.text = $"{(int)(value * 100)}%";

            AudioManager.Instance?.SetMusicVolume(value);
        }

        /// <summary>
        /// Cambia el volumen de SFX
        /// </summary>
        private void OnSFXVolumeChanged(float value)
        {
            settings.sfxVolume = value;

            if (sfxVolumeText != null)
                sfxVolumeText.text = $"{(int)(value * 100)}%";

            AudioManager.Instance?.SetSFXVolume(value);

            // Reproducir SFX de prueba
            AudioManager.Instance?.PlaySFX("ButtonClick");
        }

        /// <summary>
        /// Activa/desactiva vibración
        /// </summary>
        private void OnVibrationToggled(bool enabled)
        {
            settings.vibrationEnabled = enabled;
            SaveSettings();

            Debug.Log($"[Settings] Vibración: {enabled}");
        }

        /// <summary>
        /// Activa/desactiva notificaciones
        /// </summary>
        private void OnNotificationsToggled(bool enabled)
        {
            settings.pushNotificationsEnabled = enabled;
            SaveSettings();

            Debug.Log($"[Settings] Notificaciones: {enabled}");
        }

        #endregion

        #region Visual Settings

        /// <summary>
        /// Cambia el tema
        /// </summary>
        private void OnThemeChanged(int index)
        {
            settings.theme = (ThemeType)index;
            SaveSettings();

            Debug.Log($"[Settings] Tema: {settings.theme}");

            // Aquí se aplicaría el tema visualmente
        }

        /// <summary>
        /// Cambia la calidad gráfica
        /// </summary>
        private void OnQualityChanged(int index)
        {
            settings.graphicsQuality = (GraphicsQuality)index;
            SaveSettings();

            Debug.Log($"[Settings] Calidad: {settings.graphicsQuality}");
        }

        /// <summary>
        /// Cambia el límite de FPS
        /// </summary>
        private void OnFPSChanged(int index)
        {
            settings.targetFPS = index == 0 ? 30 : 60;
            SaveSettings();

            Debug.Log($"[Settings] FPS: {settings.targetFPS}");
        }

        #endregion

        #region Language Settings

        /// <summary>
        /// Cambia el idioma
        /// </summary>
        private void OnLanguageChanged(int index)
        {
            SystemLanguage newLanguage = GetLanguageFromIndex(index);
            settings.language = newLanguage;
            SaveSettings();

            Debug.Log($"[Settings] Idioma: {newLanguage}");

            // Aquí se aplicaría el cambio de idioma
            // LocalizationManager.Instance?.SetLanguage(newLanguage);
        }

        /// <summary>
        /// Obtiene el índice del dropdown según el idioma
        /// </summary>
        private int GetLanguageIndex(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.Spanish: return 0;
                case SystemLanguage.English: return 1;
                case SystemLanguage.Portuguese: return 2;
                case SystemLanguage.French: return 3;
                case SystemLanguage.German: return 4;
                case SystemLanguage.Japanese: return 5;
                case SystemLanguage.Korean: return 6;
                case SystemLanguage.Chinese: return 7;
                default: return 1; // English por defecto
            }
        }

        /// <summary>
        /// Obtiene el idioma según el índice del dropdown
        /// </summary>
        private SystemLanguage GetLanguageFromIndex(int index)
        {
            switch (index)
            {
                case 0: return SystemLanguage.Spanish;
                case 1: return SystemLanguage.English;
                case 2: return SystemLanguage.Portuguese;
                case 3: return SystemLanguage.French;
                case 4: return SystemLanguage.German;
                case 5: return SystemLanguage.Japanese;
                case 6: return SystemLanguage.Korean;
                case 7: return SystemLanguage.Chinese;
                default: return SystemLanguage.English;
            }
        }

        #endregion

        #region Premium

        /// <summary>
        /// Actualiza el estado premium en la UI
        /// </summary>
        private void UpdatePremiumStatus()
        {
            if (currentPlayer == null || premiumStatusText == null) return;

            if (currentPlayer.IsPremiumActive())
            {
                premiumStatusText.text = $"Premium activo hasta {currentPlayer.premiumExpiryDate:dd/MM/yyyy}";
                premiumStatusText.color = new Color(1f, 0.84f, 0f); // Dorado
            }
            else
            {
                premiumStatusText.text = "No eres Premium";
                premiumStatusText.color = Color.gray;
            }
        }

        /// <summary>
        /// Comprar premium
        /// </summary>
        private void OnBuyPremiumClicked()
        {
            Debug.Log("[Settings] Comprando premium...");

            // Aquí se abriría la tienda IAP
            // IAPManager.Instance?.PurchasePremium();
        }

        #endregion

        #region Navigation

        /// <summary>
        /// Vuelve al menú principal
        /// </summary>
        private void OnBackButtonClicked()
        {
            Debug.Log("[Settings] Volviendo al menú principal");

            // Guardar antes de salir
            SaveSettings();

            SceneManager.LoadScene("MainMenu");
        }

        #endregion
    }

    /// <summary>
    /// Paneles de configuración
    /// </summary>
    public enum SettingsPanel
    {
        Account,
        Game,
        Visual,
        Language,
        Logout
    }
}
