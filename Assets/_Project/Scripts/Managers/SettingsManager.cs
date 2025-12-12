using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DigitPark.Services.Firebase;
using DigitPark.Data;
using DigitPark.Localization;
using DigitPark.UI.Panels;

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

        [Header("UI - Premium Section")]
        [SerializeField] private GameObject premiumSection;
        [SerializeField] private Button removeAdsButton;
        [SerializeField] private TextMeshProUGUI removeAdsButtonText;
        [SerializeField] private Button premiumFullButton;
        [SerializeField] private TextMeshProUGUI premiumFullButtonText;
        [SerializeField] private Button restorePurchasesButton;

        [Header("UI - Panels (Prefabs)")]
        [SerializeField] private InputPanelUI changeNamePanel;
        [SerializeField] private ConfirmPanelUI deleteConfirmPanel;
        [SerializeField] private ConfirmPanelUI logoutConfirmPanel;
        [SerializeField] private ErrorPanelUI errorPanel;

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
            UpdatePremiumUI();

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

            // Premium buttons
            removeAdsButton?.onClick.AddListener(OnRemoveAdsClicked);
            premiumFullButton?.onClick.AddListener(OnPremiumFullClicked);
            restorePurchasesButton?.onClick.AddListener(OnRestorePurchasesClicked);

            // Suscribirse a cambios de estado premium
            PremiumManager.OnPremiumStatusChanged += UpdatePremiumUI;

            // Los paneles ahora manejan sus propios listeners internamente
        }

        private void OnDestroy()
        {
            PremiumManager.OnPremiumStatusChanged -= UpdatePremiumUI;
        }

        private void HidePanels()
        {
            // Los paneles se ocultan automáticamente en su Awake()
            // Este método ya no es necesario pero se mantiene por compatibilidad
            changeNamePanel?.Hide();
            deleteConfirmPanel?.Hide();
            logoutConfirmPanel?.Hide();
            errorPanel?.Hide();
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
                changeNamePanel.SetLengthLimits(3, 20);
                changeNamePanel.Show(
                    "Cambiar Nombre",
                    "Nuevo nombre de usuario",
                    OnConfirmNameClicked,
                    null // OnCancel se maneja internamente
                );
            }
        }

        private async void OnConfirmNameClicked(string newUsername)
        {
            if (currentPlayer == null) return;

            // La validación ya se hace en InputPanelUI
            if (newUsername == currentPlayer.username)
            {
                changeNamePanel?.Hide();
                return;
            }

            Debug.Log($"[Settings] Cambiando nombre a: {newUsername}");

            bool success = await AuthenticationService.Instance.UpdateUsername(newUsername);

            if (success)
            {
                Debug.Log("[Settings] Nombre actualizado exitosamente");
                currentPlayer.username = newUsername;
                changeNamePanel?.Hide();
            }
            else
            {
                Debug.LogError("[Settings] Error al actualizar nombre");
                changeNamePanel?.SetButtonsInteractable(true);
                errorPanel?.Show("Error al actualizar el nombre. Intenta de nuevo.");
            }
        }

        #endregion

        #region Delete Account

        private void OnDeleteAccountButtonClicked()
        {
            Debug.Log("[Settings] Mostrando confirmación de eliminar cuenta");

            if (deleteConfirmPanel != null)
            {
                deleteConfirmPanel.Show(
                    "Eliminar Cuenta",
                    "¿Estás seguro de que deseas eliminar tu cuenta? Esta acción no se puede deshacer.",
                    OnConfirmDeleteClicked,
                    null // OnCancel se maneja internamente
                );
            }
        }

        private async void OnConfirmDeleteClicked()
        {
            Debug.Log("[Settings] Eliminando cuenta...");

            if (currentPlayer == null) return;

            if (AuthenticationService.Instance != null)
            {
                bool success = await AuthenticationService.Instance.DeleteAccount();

                if (success)
                {
                    Debug.Log("[Settings] Cuenta eliminada exitosamente");
                    SceneManager.LoadScene("Login");
                }
                else
                {
                    Debug.LogError("[Settings] Error al eliminar la cuenta");
                    deleteConfirmPanel?.Hide();
                    errorPanel?.Show("Error al eliminar la cuenta. Intenta de nuevo.");
                }
            }
            else
            {
                Debug.LogError("[Settings] AuthenticationService no disponible");
                deleteConfirmPanel?.Hide();
                errorPanel?.Show("Servicio no disponible. Intenta más tarde.");
            }
        }

        #endregion

        #region Logout

        private void OnLogoutClicked()
        {
            Debug.Log("[Settings] Mostrando confirmación de logout");

            if (logoutConfirmPanel != null)
            {
                logoutConfirmPanel.Show(
                    "Cerrar Sesión",
                    "¿Estás seguro de que deseas cerrar sesión?",
                    OnConfirmLogoutClicked,
                    null // OnCancel se maneja internamente
                );
            }
        }

        private void OnConfirmLogoutClicked()
        {
            Debug.Log("[Settings] Cerrando sesión...");

            logoutConfirmPanel?.Hide();
            AuthenticationService.Instance?.Logout();
            SceneManager.LoadScene("Login");
        }

        #endregion

        #region Navigation

        private void OnBackButtonClicked()
        {
            Debug.Log("[Settings] Volviendo al menú principal");
            SceneManager.LoadScene("MainMenu");
        }

        #endregion

        #region Premium

        /// <summary>
        /// Actualiza la UI de premium según el estado actual
        /// </summary>
        private void UpdatePremiumUI()
        {
            if (PremiumManager.Instance == null) return;

            bool hasNoAds = PremiumManager.Instance.HasNoAds;
            bool canCreateTournaments = PremiumManager.Instance.CanCreateTournaments;

            // Actualizar botón de quitar anuncios
            if (removeAdsButton != null)
            {
                removeAdsButton.interactable = !hasNoAds;
                if (removeAdsButtonText != null)
                {
                    if (hasNoAds)
                    {
                        removeAdsButtonText.text = AutoLocalizer.Get("already_purchased");
                    }
                    else
                    {
                        removeAdsButtonText.text = $"{AutoLocalizer.Get("remove_ads_title")} - {PremiumManager.PRICE_REMOVE_ADS}";
                    }
                }
            }

            // Actualizar botón de premium completo
            if (premiumFullButton != null)
            {
                premiumFullButton.interactable = !canCreateTournaments;
                if (premiumFullButtonText != null)
                {
                    if (canCreateTournaments)
                    {
                        premiumFullButtonText.text = AutoLocalizer.Get("already_purchased");
                    }
                    else
                    {
                        premiumFullButtonText.text = $"{AutoLocalizer.Get("premium_full_title")} - {PremiumManager.PRICE_PREMIUM_FULL}";
                    }
                }
            }

            // Ocultar sección de premium si ya tiene todo
            if (premiumSection != null && hasNoAds && canCreateTournaments)
            {
                // Opcionalmente ocultar la sección si tiene todo
                // premiumSection.SetActive(false);
            }

            Debug.Log($"[Settings] Premium UI actualizada - NoAds: {hasNoAds}, CreateTournaments: {canCreateTournaments}");
        }

        /// <summary>
        /// Compra: Quitar anuncios ($10 MXN)
        /// </summary>
        private void OnRemoveAdsClicked()
        {
            Debug.Log("[Settings] Iniciando compra: Quitar Anuncios");

            if (PremiumManager.Instance.HasNoAds)
            {
                errorPanel?.Show(AutoLocalizer.Get("already_purchased"));
                return;
            }

            // Deshabilitar botón mientras se procesa
            if (removeAdsButton != null) removeAdsButton.interactable = false;

            PremiumManager.Instance.PurchaseRemoveAds(success =>
            {
                if (success)
                {
                    errorPanel?.Show(AutoLocalizer.Get("purchase_success"));
                }
                else
                {
                    errorPanel?.Show(AutoLocalizer.Get("purchase_failed"));
                    if (removeAdsButton != null) removeAdsButton.interactable = true;
                }
            });
        }

        /// <summary>
        /// Compra: Premium completo ($20 MXN)
        /// </summary>
        private void OnPremiumFullClicked()
        {
            Debug.Log("[Settings] Iniciando compra: Premium Completo");

            if (PremiumManager.Instance.CanCreateTournaments)
            {
                errorPanel?.Show(AutoLocalizer.Get("already_purchased"));
                return;
            }

            // Deshabilitar botón mientras se procesa
            if (premiumFullButton != null) premiumFullButton.interactable = false;

            PremiumManager.Instance.PurchasePremiumFull(success =>
            {
                if (success)
                {
                    errorPanel?.Show(AutoLocalizer.Get("purchase_success"));
                }
                else
                {
                    errorPanel?.Show(AutoLocalizer.Get("purchase_failed"));
                    if (premiumFullButton != null) premiumFullButton.interactable = true;
                }
            });
        }

        /// <summary>
        /// Restaurar compras (principalmente para iOS)
        /// </summary>
        private void OnRestorePurchasesClicked()
        {
            Debug.Log("[Settings] Restaurando compras...");
            PremiumManager.Instance.RestorePurchases();
        }

        #endregion
    }
}
