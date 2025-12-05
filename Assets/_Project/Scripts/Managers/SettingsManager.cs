using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DigitPark.Services.Firebase;
using DigitPark.Data;
using DigitPark.Localization;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager de la escena de Settings
    /// Gestiona volumen, cambio de nombre, eliminar cuenta y cierre de sesión
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        [Header("UI - Settings Panel")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private TextMeshProUGUI titleText;

        [Header("UI - Volume Sliders")]
        [SerializeField] private Slider soundVolumeSlider;
        [SerializeField] private TextMeshProUGUI soundValueText;
        [SerializeField] private Slider effectsVolumeSlider;
        [SerializeField] private TextMeshProUGUI effectsValueText;

        [Header("UI - Language")]
        [SerializeField] private TMP_Dropdown languageDropdown;

        [Header("UI - Buttons")]
        [SerializeField] private Button changeNameButton;
        [SerializeField] private Button logoutButton;
        [SerializeField] private Button deleteAccountButton;
        [SerializeField] private Button backButton;

        [Header("UI - Change Name Panel")]
        [SerializeField] private GameObject changeNamePanel;
        [SerializeField] private TextMeshProUGUI changeNameTitleText;
        [SerializeField] private TMP_InputField newNameInput;
        [SerializeField] private Button confirmNameButton;
        [SerializeField] private Button cancelNameButton;

        [Header("UI - Delete Confirm Panel")]
        [SerializeField] private GameObject deleteConfirmPanel;
        [SerializeField] private TextMeshProUGUI deleteTitleText;
        [SerializeField] private Button confirmDeleteButton;
        [SerializeField] private Button cancelDeleteButton;

        [Header("UI - Logout Confirm Panel")]
        [SerializeField] private GameObject logoutBlockerPanel;
        [SerializeField] private GameObject logoutConfirmPanel;
        [SerializeField] private TextMeshProUGUI logoutTitleMessage;
        [SerializeField] private Button confirmLogoutButton;
        [SerializeField] private TextMeshProUGUI confirmLogoutButtonText;
        [SerializeField] private Button cancelLogoutButton;
        [SerializeField] private TextMeshProUGUI cancelLogoutButtonText;

        // Keys para PlayerPrefs
        private const string SOUND_VOLUME_KEY = "SoundVolume";
        private const string EFFECTS_VOLUME_KEY = "EffectsVolume";

        private PlayerData currentPlayer;

        private void Start()
        {
            Debug.Log("[Settings] SettingsManager iniciado");

            LoadPlayerData();
            LoadVolumeSettings();
            SetupLanguageDropdown();
            SetupListeners();
            HidePanels();

            if (settingsPanel != null)
                settingsPanel.SetActive(true);
        }

        #region Initialization

        private void LoadPlayerData()
        {
            if (AuthenticationService.Instance != null)
            {
                currentPlayer = AuthenticationService.Instance.GetCurrentPlayerData();

                if (currentPlayer == null)
                {
                    Debug.LogError("[Settings] No hay datos del jugador");
                    SceneManager.LoadScene("Login");
                }
            }
        }

        private void LoadVolumeSettings()
        {
            // Cargar volumen guardado o usar 50% por defecto
            float soundVolume = PlayerPrefs.GetFloat(SOUND_VOLUME_KEY, 0.5f);
            float effectsVolume = PlayerPrefs.GetFloat(EFFECTS_VOLUME_KEY, 0.5f);

            if (soundVolumeSlider != null)
            {
                soundVolumeSlider.value = soundVolume;
                UpdateSoundValueText(soundVolume);
            }

            if (effectsVolumeSlider != null)
            {
                effectsVolumeSlider.value = effectsVolume;
                UpdateEffectsValueText(effectsVolume);
            }

            Debug.Log($"[Settings] Volumen cargado - Sonido: {soundVolume * 100}%, Efectos: {effectsVolume * 100}%");
        }

        private void SetupLanguageDropdown()
        {
            if (languageDropdown == null) return;

            // Las opciones ya están configuradas en el Inspector:
            // 0: English, 1: Español, 2: Français, 3: Português, 4: Deutsch

            // Establecer el valor actual
            int currentIndex = PlayerPrefs.GetInt("Language", 0);

            if (LocalizationManager.Instance != null)
            {
                currentIndex = LocalizationManager.Instance.GetCurrentLanguageIndex();
            }

            // Asegurar que el índice esté dentro del rango
            currentIndex = Mathf.Clamp(currentIndex, 0, languageDropdown.options.Count - 1);

            // Remover listener temporalmente para evitar que se dispare al establecer el valor
            languageDropdown.onValueChanged.RemoveListener(OnLanguageDropdownChanged);
            languageDropdown.value = currentIndex;
            languageDropdown.RefreshShownValue();
            languageDropdown.onValueChanged.AddListener(OnLanguageDropdownChanged);

            string[] languageNames = { "English", "Español", "Français", "Português", "Deutsch" };
            string currentLanguage = currentIndex < languageNames.Length ? languageNames[currentIndex] : "Unknown";
            Debug.Log($"[Settings] Idioma actual: {currentLanguage}");
        }

        private void SetupListeners()
        {
            // Sliders
            soundVolumeSlider?.onValueChanged.AddListener(OnSoundVolumeChanged);
            effectsVolumeSlider?.onValueChanged.AddListener(OnEffectsVolumeChanged);

            // Language Dropdown - ya se configura en SetupLanguageDropdown()

            // Botones principales
            changeNameButton?.onClick.AddListener(OnChangeNameButtonClicked);
            logoutButton?.onClick.AddListener(OnLogoutClicked);
            deleteAccountButton?.onClick.AddListener(OnDeleteAccountButtonClicked);
            backButton?.onClick.AddListener(OnBackButtonClicked);

            // Panel cambiar nombre
            confirmNameButton?.onClick.AddListener(OnConfirmNameClicked);
            cancelNameButton?.onClick.AddListener(OnCancelNameClicked);

            // Panel eliminar cuenta
            confirmDeleteButton?.onClick.AddListener(OnConfirmDeleteClicked);
            cancelDeleteButton?.onClick.AddListener(OnCancelDeleteClicked);

            // Panel confirmar logout
            confirmLogoutButton?.onClick.AddListener(OnConfirmLogoutClicked);
            cancelLogoutButton?.onClick.AddListener(OnCancelLogoutClicked);
        }

        private void HidePanels()
        {
            if (changeNamePanel != null)
                changeNamePanel.SetActive(false);

            if (deleteConfirmPanel != null)
                deleteConfirmPanel.SetActive(false);

            if (logoutBlockerPanel != null)
                logoutBlockerPanel.SetActive(false);

            if (logoutConfirmPanel != null)
                logoutConfirmPanel.SetActive(false);
        }

        #endregion

        #region Language

        private void OnLanguageDropdownChanged(int index)
        {
            string[] languageNames = { "English", "Español", "Français", "Português", "Deutsch" };
            string language = index < languageNames.Length ? languageNames[index] : "Unknown";
            Debug.Log($"[Settings] Cambiando idioma a: {language}");

            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.SetLanguage(index);
            }
            else
            {
                // Guardar en PlayerPrefs si LocalizationManager no existe
                PlayerPrefs.SetInt("Language", index);
                PlayerPrefs.Save();
            }
        }

        #endregion

        #region Volume Control

        private void OnSoundVolumeChanged(float value)
        {
            UpdateSoundValueText(value);
            PlayerPrefs.SetFloat(SOUND_VOLUME_KEY, value);
            PlayerPrefs.Save();

            // Aplicar volumen al AudioManager si existe
            ApplySoundVolume(value);

            Debug.Log($"[Settings] Volumen de sonido: {Mathf.RoundToInt(value * 100)}%");
        }

        private void OnEffectsVolumeChanged(float value)
        {
            UpdateEffectsValueText(value);
            PlayerPrefs.SetFloat(EFFECTS_VOLUME_KEY, value);
            PlayerPrefs.Save();

            // Aplicar volumen al AudioManager si existe
            ApplyEffectsVolume(value);

            Debug.Log($"[Settings] Volumen de efectos: {Mathf.RoundToInt(value * 100)}%");
        }

        private void UpdateSoundValueText(float value)
        {
            if (soundValueText != null)
            {
                soundValueText.text = $"{Mathf.RoundToInt(value * 100)}%";
            }
        }

        private void UpdateEffectsValueText(float value)
        {
            if (effectsValueText != null)
            {
                effectsValueText.text = $"{Mathf.RoundToInt(value * 100)}%";
            }
        }

        private void ApplySoundVolume(float volume)
        {
            // Aquí puedes conectar con tu AudioManager
            // AudioManager.Instance?.SetMusicVolume(volume);
            AudioListener.volume = volume; // Volumen global como fallback
        }

        private void ApplyEffectsVolume(float volume)
        {
            // Aquí puedes conectar con tu AudioManager
            // AudioManager.Instance?.SetSFXVolume(volume);
        }

        /// <summary>
        /// Obtiene el volumen de sonido guardado (para usar desde otros scripts)
        /// </summary>
        public static float GetSoundVolume()
        {
            return PlayerPrefs.GetFloat(SOUND_VOLUME_KEY, 0.5f);
        }

        /// <summary>
        /// Obtiene el volumen de efectos guardado (para usar desde otros scripts)
        /// </summary>
        public static float GetEffectsVolume()
        {
            return PlayerPrefs.GetFloat(EFFECTS_VOLUME_KEY, 0.5f);
        }

        #endregion

        #region Change Name

        private void OnChangeNameButtonClicked()
        {
            Debug.Log("[Settings] Mostrando panel de cambio de nombre");

            if (changeNamePanel != null)
            {
                changeNamePanel.SetActive(true);

                if (newNameInput != null)
                {
                    newNameInput.text = "";
                    newNameInput.Select();
                    newNameInput.ActivateInputField();
                }
            }
        }

        private async void OnConfirmNameClicked()
        {
            if (newNameInput == null || currentPlayer == null) return;

            string newUsername = newNameInput.text.Trim();

            // Validaciones
            if (string.IsNullOrEmpty(newUsername))
            {
                Debug.LogWarning("[Settings] El nombre no puede estar vacío");
                return;
            }

            if (newUsername.Length < 3)
            {
                Debug.LogWarning("[Settings] El nombre debe tener al menos 3 caracteres");
                return;
            }

            if (newUsername.Length > 20)
            {
                Debug.LogWarning("[Settings] El nombre debe tener máximo 20 caracteres");
                return;
            }

            if (newUsername == currentPlayer.username)
            {
                OnCancelNameClicked();
                return;
            }

            Debug.Log($"[Settings] Cambiando nombre a: {newUsername}");

            // Deshabilitar botones
            SetNameButtonsInteractable(false);

            bool success = await AuthenticationService.Instance.UpdateUsername(newUsername);

            if (success)
            {
                Debug.Log("[Settings] Nombre actualizado exitosamente");
                currentPlayer.username = newUsername;
                OnCancelNameClicked();
            }
            else
            {
                Debug.LogError("[Settings] Error al actualizar nombre");
                SetNameButtonsInteractable(true);
            }
        }

        private void OnCancelNameClicked()
        {
            Debug.Log("[Settings] Cancelando cambio de nombre");

            if (changeNamePanel != null)
                changeNamePanel.SetActive(false);

            if (newNameInput != null)
                newNameInput.text = "";

            SetNameButtonsInteractable(true);
        }

        private void SetNameButtonsInteractable(bool interactable)
        {
            if (confirmNameButton != null) confirmNameButton.interactable = interactable;
            if (cancelNameButton != null) cancelNameButton.interactable = interactable;
        }

        #endregion

        #region Delete Account

        private void OnDeleteAccountButtonClicked()
        {
            Debug.Log("[Settings] Mostrando confirmación de eliminar cuenta");

            if (deleteConfirmPanel != null)
            {
                deleteConfirmPanel.SetActive(true);
            }
        }

        private async void OnConfirmDeleteClicked()
        {
            Debug.Log("[Settings] Eliminando cuenta...");

            if (currentPlayer == null) return;

            // Deshabilitar botones
            SetDeleteButtonsInteractable(false);

            string userId = currentPlayer.userId;
            string email = currentPlayer.email?.ToLower() ?? "";

            // Eliminar usuario de todos los leaderboards
            if (DatabaseService.Instance != null)
            {
                await DatabaseService.Instance.RemoveUserFromLeaderboards(userId);
            }

            // Eliminar datos del usuario de PlayerPrefs
            PlayerPrefs.DeleteKey($"SimUser_{userId}");
            PlayerPrefs.DeleteKey($"SimUserByEmail_{email}");
            PlayerPrefs.DeleteKey($"SimPassword_{userId}");
            PlayerPrefs.DeleteKey("SavedUserId");
            PlayerPrefs.DeleteKey("RememberMe");
            PlayerPrefs.Save();

            Debug.Log("[Settings] Cuenta eliminada exitosamente");

            // Logout y volver a Login
            AuthenticationService.Instance?.Logout();
            SceneManager.LoadScene("Login");
        }

        private void OnCancelDeleteClicked()
        {
            Debug.Log("[Settings] Cancelando eliminación de cuenta");

            if (deleteConfirmPanel != null)
                deleteConfirmPanel.SetActive(false);

            SetDeleteButtonsInteractable(true);
        }

        private void SetDeleteButtonsInteractable(bool interactable)
        {
            if (confirmDeleteButton != null) confirmDeleteButton.interactable = interactable;
            if (cancelDeleteButton != null) cancelDeleteButton.interactable = interactable;
        }

        #endregion

        #region Logout

        private void OnLogoutClicked()
        {
            Debug.Log("[Settings] Mostrando confirmación de logout");

            if (logoutBlockerPanel != null)
                logoutBlockerPanel.SetActive(true);

            if (logoutConfirmPanel != null)
                logoutConfirmPanel.SetActive(true);
        }

        private void OnConfirmLogoutClicked()
        {
            Debug.Log("[Settings] Cerrando sesión...");

            AuthenticationService.Instance?.Logout();
            SceneManager.LoadScene("Login");
        }

        private void OnCancelLogoutClicked()
        {
            Debug.Log("[Settings] Cancelando logout");

            if (logoutBlockerPanel != null)
                logoutBlockerPanel.SetActive(false);

            if (logoutConfirmPanel != null)
                logoutConfirmPanel.SetActive(false);
        }

        #endregion

        #region Navigation

        private void OnBackButtonClicked()
        {
            Debug.Log("[Settings] Volviendo al menú principal");
            SceneManager.LoadScene("MainMenu");
        }

        #endregion
    }
}
