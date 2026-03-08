using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;
using DigitPark.Services.Firebase;
using DigitPark.Data;
using DigitPark.Localization;
using DigitPark.UI;
using DigitPark.UI.Panels;
using DigitPark.UI.Components;
using DigitPark.Themes;
using DigitPark.Monetization;

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
        [SerializeField] private TextMeshProUGUI changeLangLabel;
        [SerializeField] private LanguageDropdownStyler languageStyler;

        [Header("UI - Theme")]
        [SerializeField] private TMP_Dropdown themeDropdown;
        [SerializeField] private TextMeshProUGUI changeThemeLabel;

        [Header("UI - Buttons")]
        [SerializeField] private Button changeNameButton;
        [SerializeField] private TextMeshProUGUI changeNameCostText;
        [SerializeField] private Button copyIDButton;
        [SerializeField] private TextMeshProUGUI playerIdText;
        [SerializeField] private Button logoutButton;
        [SerializeField] private Button deleteAccountButton;
        [SerializeField] private Button backButton;

        [Header("UI - Premium Section")]
        [SerializeField] private GameObject premiumSection;
        [SerializeField] private Button createTournamentsButton;
        [SerializeField] private TextMeshProUGUI createTournamentsButtonText;
        [SerializeField] private Button tournamentBundleButton;
        [SerializeField] private TextMeshProUGUI tournamentBundleButtonText;
        [SerializeField] private Button restorePurchasesButton;

        [Header("UI - Premium Button (Pro)")]
        [SerializeField] private Button premiumButton;
        [SerializeField] private GameObject premiumBadge;
        [SerializeField] private PremiumPanelUI premiumPanel;

        [Header("UI - Panels (Prefabs)")]
        [SerializeField] private InputPanelUI changeNamePanel;
        [SerializeField] private ConfirmPanelUI deleteConfirmPanel;
        [SerializeField] private ConfirmPanelUI logoutConfirmPanel;
        [SerializeField] private ErrorPanelUI errorPanel;

        [Header("UI - Vibration Toggle")]
        [SerializeField] private Toggle vibrationToggle;
        [SerializeField] private TextMeshProUGUI vibrationToggleText;
        [SerializeField] private Image vibrationToggleBg;

        [Header("UI - Shop")]
        [SerializeField] private Button shopButton;

        [Header("UI - Legal Section")]
        [SerializeField] private Button termsButton;
        [SerializeField] private Button privacyButton;
        [SerializeField] private Button responsibleGamingButton;
        [SerializeField] private Button triumphTermsButton;
        [SerializeField] private Button selfExclusionButton;
        [SerializeField] private ConfirmPanelUI selfExclusionConfirmPanel;

        // URLs Legales
        private const string URL_TERMS = "https://docs.triumpharcade.com/terms-of-use";
        private const string URL_PRIVACY = "https://docs.triumpharcade.com/privacy-policy";
        private const string URL_RESPONSIBLE_GAMING = "https://docs.triumpharcade.com/responsible-gaming-policy";
        private const string URL_TRIUMPH_TERMS = "https://docs.triumpharcade.com/terms-of-use";

        // Keys para PlayerPrefs
        private const string SOUND_VOLUME_KEY = "SoundVolume";
        private const string EFFECTS_VOLUME_KEY = "EffectsVolume";
        private const string VIBRATION_KEY = "VibrationEnabled";
        private const string NOTIFICATIONS_KEY = "NotificationsEnabled";
        // CashBattleBypassAuth solo se puede activar desde EditorBootConfig (Editor-only)
        private const string CASHBATTLE_BYPASS_AUTH_KEY = "CashBattleBypassAuth";
        private const string NAME_CHANGE_COUNT_KEY = "NameChangeCount";
        private const int NAME_CHANGE_GEM_COST = 100;

        private PlayerData currentPlayer;

        private void Start()
        {
            Debug.Log("[Settings] SettingsManager iniciado");

            if (titleText != null)
                titleText.text = AutoLocalizer.Get("settings_title");

            LoadPlayerData();
            LoadVolumeSettings();
            InitVibrationToggle();
            SetupLanguageDropdown();
            SetupLanguageStyler();
            SetupThemeDropdown();
            SetupListeners();
            HidePanels();
            UpdatePremiumUI();
            UpdateNameChangeCost();

            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
                var cg = settingsPanel.GetComponent<CanvasGroup>();
                if (cg == null) cg = settingsPanel.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                cg.DOFade(1f, 0.35f).SetEase(Ease.OutQuad);
            }
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
                else if (playerIdText != null)
                {
                    playerIdText.text = $"#{currentPlayer.userId}";
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

        private void SetupLanguageStyler()
        {
            // Si no hay styler asignado, intentar agregarlo al dropdown
            if (languageStyler == null && languageDropdown != null)
            {
                languageStyler = languageDropdown.GetComponent<LanguageDropdownStyler>();
                if (languageStyler == null)
                {
                    languageStyler = languageDropdown.gameObject.AddComponent<LanguageDropdownStyler>();
                }
            }

            // Aplicar estilos si el styler existe
            if (languageStyler != null)
            {
                languageStyler.ApplyStyles();
                Debug.Log("[Settings] Estilos del dropdown de idioma aplicados");
            }
        }

        private void SetupThemeDropdown()
        {
            if (themeDropdown == null) return;

            // Limpiar opciones existentes
            themeDropdown.ClearOptions();

            // Verificar que ThemeManager existe
            if (ThemeManager.Instance == null)
            {
                Debug.LogWarning("[Settings] ThemeManager no disponible");
                return;
            }

            // Agregar todas las opciones de temas disponibles
            var themes = ThemeManager.Instance.AvailableThemes;
            var options = new System.Collections.Generic.List<TMP_Dropdown.OptionData>();

            foreach (var theme in themes)
            {
                string themeName = AutoLocalizer.Get($"theme_{theme.themeId}");
                if (string.IsNullOrEmpty(themeName) || themeName.StartsWith("theme_"))
                {
                    themeName = theme.themeName; // Fallback al nombre original
                }
                options.Add(new TMP_Dropdown.OptionData(themeName));
            }

            themeDropdown.AddOptions(options);

            // Establecer el valor actual
            int currentIndex = ThemeManager.Instance.CurrentThemeIndex;
            currentIndex = Mathf.Clamp(currentIndex, 0, themeDropdown.options.Count - 1);

            // Remover listener temporalmente para evitar que se dispare al establecer el valor
            themeDropdown.onValueChanged.RemoveListener(OnThemeDropdownChanged);
            themeDropdown.value = currentIndex;
            themeDropdown.RefreshShownValue();
            themeDropdown.onValueChanged.AddListener(OnThemeDropdownChanged);

            // Actualizar label si existe
            if (changeThemeLabel != null)
            {
                changeThemeLabel.text = AutoLocalizer.Get("change_theme");
            }

            Debug.Log($"[Settings] Tema actual: {ThemeManager.Instance.CurrentTheme?.themeName ?? "Ninguno"}");
        }

        private void SetupListeners()
        {
            // Sliders
            soundVolumeSlider?.onValueChanged.AddListener(OnSoundVolumeChanged);
            effectsVolumeSlider?.onValueChanged.AddListener(OnEffectsVolumeChanged);
            vibrationToggle?.onValueChanged.AddListener(OnVibrationToggled);

            // Language Dropdown - ya se configura en SetupLanguageDropdown()

            // Botones principales
            changeNameButton?.onClick.AddListener(OnChangeNameButtonClicked);
            copyIDButton?.onClick.AddListener(OnCopyIDClicked);
            logoutButton?.onClick.AddListener(OnLogoutClicked);
            deleteAccountButton?.onClick.AddListener(OnDeleteAccountButtonClicked);
            // Disable auto-navigation from BackButton prefab to prevent double listener
            var autoNav = backButton?.GetComponent<DigitPark.UI.BackButton>();
            if (autoNav != null) autoNav.DisableAutoNavigation();
            backButton?.onClick.AddListener(OnBackButtonClicked);

            // Premium buttons
            createTournamentsButton?.onClick.AddListener(OnCreateTournamentsClicked);
            tournamentBundleButton?.onClick.AddListener(OnTournamentBundleClicked);
            restorePurchasesButton?.onClick.AddListener(OnRestorePurchasesClicked);
            premiumButton?.onClick.AddListener(OnPremiumButtonClicked);

            // Shop (use SceneNavigator so GoBack returns to Settings)
            shopButton?.onClick.AddListener(() => SceneNavigator.Instance?.NavigateTo("Shop"));

            // Legal buttons
            termsButton?.onClick.AddListener(() => OpenURL(URL_TERMS));
            privacyButton?.onClick.AddListener(() => OpenURL(URL_PRIVACY));
            responsibleGamingButton?.onClick.AddListener(() => OpenURL(URL_RESPONSIBLE_GAMING));
            triumphTermsButton?.onClick.AddListener(() => OpenURL(URL_TRIUMPH_TERMS));
            selfExclusionButton?.onClick.AddListener(OnSelfExclusionClicked);

            // Suscribirse a cambios de estado premium
            PremiumManager.OnPremiumStatusChanged += UpdatePremiumUI;

            // Los paneles ahora manejan sus propios listeners internamente
        }

        private void OnEnable()
        {
            LocalizationManager.OnLanguageChanged += OnLanguageChanged;
            ThemeManager.OnThemeChanged += OnThemeChanged;
        }

        private void OnDisable()
        {
            LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
            ThemeManager.OnThemeChanged -= OnThemeChanged;
        }

        private void OnThemeChanged(ThemeData newTheme)
        {
            // Actualizar estilos de los dropdowns cuando cambia el tema
            ApplyThemeToDropdowns(newTheme);
        }

        private void ApplyThemeToDropdowns(ThemeData theme)
        {
            if (theme == null) return;

            Color accentColor = theme.primaryAccent;
            Color bgColor = theme.cardBackground;

            // Aplicar al theme dropdown
            if (themeDropdown != null)
            {
                // Label del dropdown de tema
                var themeLabelTransform = themeDropdown.transform.Find("Label");
                if (themeLabelTransform != null)
                {
                    var themeLabel = themeLabelTransform.GetComponent<TextMeshProUGUI>();
                    if (themeLabel != null)
                    {
                        themeLabel.color = accentColor;
                    }
                }

                // Flecha del dropdown de tema
                var themeArrow = themeDropdown.transform.Find("Arrow");
                if (themeArrow != null)
                {
                    var arrowImg = themeArrow.GetComponent<Image>();
                    if (arrowImg != null)
                    {
                        arrowImg.color = accentColor;
                    }
                }
            }

            // Actualizar label de "Change Theme"
            if (changeThemeLabel != null)
            {
                changeThemeLabel.color = accentColor;
            }

            // Actualizar label de "Change Language"
            if (changeLangLabel != null)
            {
                changeLangLabel.color = accentColor;
            }

            // Forzar actualización del language styler
            if (languageStyler != null)
            {
                languageStyler.SetThemeColor(accentColor);
            }
        }

        private void OnDestroy()
        {
            PremiumManager.OnPremiumStatusChanged -= UpdatePremiumUI;
        }

        private void OnLanguageChanged()
        {
            RefreshThemeDropdownLabels();

            // Refresh all texts in Settings when language changes
            if (AutoLocalizer.Instance != null)
            {
                AutoLocalizer.Instance.LocalizeAllTexts();
            }
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

        #region CashBattle Bypass

        /// <summary>
        /// Verifica si el bypass está habilitado.
        /// Solo puede activarse desde EditorBootConfig (Editor-only).
        /// En builds de producción siempre retorna false.
        /// </summary>
        public static bool IsCashBattleAuthBypassed()
        {
#if UNITY_EDITOR
            return PlayerPrefs.GetInt(CASHBATTLE_BYPASS_AUTH_KEY, 0) == 1;
#else
            return false;
#endif
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

        #region Theme

        private void OnThemeDropdownChanged(int index)
        {
            // Theme change is handled by ThemeDropdownController (which checks locks).
            // This listener is intentionally empty to avoid bypassing lock checks.
            // ThemeDropdownController.OnThemeSelected() handles lock verification,
            // revert to last valid index, and showing the purchase panel.
        }

        /// <summary>
        /// Actualiza el dropdown de temas cuando cambia el idioma
        /// </summary>
        private void RefreshThemeDropdownLabels()
        {
            if (themeDropdown == null || ThemeManager.Instance == null) return;

            int currentIndex = themeDropdown.value;
            var themes = ThemeManager.Instance.AvailableThemes;

            for (int i = 0; i < themeDropdown.options.Count && i < themes.Count; i++)
            {
                string themeName = AutoLocalizer.Get($"theme_{themes[i].themeId}");
                if (string.IsNullOrEmpty(themeName) || themeName.StartsWith("theme_"))
                {
                    themeName = themes[i].themeName;
                }
                themeDropdown.options[i].text = themeName;
            }

            themeDropdown.RefreshShownValue();

            if (changeThemeLabel != null)
            {
                changeThemeLabel.text = AutoLocalizer.Get("change_theme");
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

        /// <summary>
        /// Initializes vibration toggle UI state from saved PlayerPrefs.
        /// Must be called before SetupListeners to avoid double-fire.
        /// </summary>
        private void InitVibrationToggle()
        {
            if (vibrationToggle == null) return;

            // Fallback: find toggle text dynamically if serialized ref is missing
            if (vibrationToggleText == null)
            {
                var candidate = vibrationToggle.GetComponentInChildren<TextMeshProUGUI>();
                if (candidate != null)
                {
                    vibrationToggleText = candidate;
                    Debug.Log("[Settings] vibrationToggleText asignado dinámicamente via fallback");
                }
            }

            bool isOn = IsVibrationEnabled();
            vibrationToggle.SetIsOnWithoutNotify(isOn);
            UpdateVibrationUI(isOn);
        }

        private void OnVibrationToggled(bool isOn)
        {
            SetVibrationEnabled(isOn);
            UpdateVibrationUI(isOn);
        }

        private void UpdateVibrationUI(bool isOn)
        {
            if (vibrationToggleText != null)
            {
                vibrationToggleText.text = isOn
                    ? AutoLocalizer.Get("toggle_on")
                    : AutoLocalizer.Get("toggle_off");

                vibrationToggleText.color = isOn
                    ? new Color(0.02f, 0.04f, 0.08f, 1f)   // DARK_NAVY (legible on cyan)
                    : new Color(0.6f, 0.6f, 0.65f, 1f);    // TEXT_GRAY
            }
            else
            {
                Debug.LogWarning("[Settings] vibrationToggleText es null — ejecuta SettingsReferenceAssigner");
            }

            if (vibrationToggleBg != null)
                vibrationToggleBg.color = isOn
                    ? new Color(0f, 1f, 1f, 1f)            // CYAN_NEON
                    : new Color(0.25f, 0.25f, 0.3f, 1f);   // TOGGLE_OFF_BG
        }

        /// <summary>
        /// Verifica si la vibración está habilitada
        /// </summary>
        public static bool IsVibrationEnabled()
        {
            return PlayerPrefs.GetInt(VIBRATION_KEY, 1) == 1;
        }

        /// <summary>
        /// Establece el estado de vibración
        /// </summary>
        public static void SetVibrationEnabled(bool enabled)
        {
            PlayerPrefs.SetInt(VIBRATION_KEY, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Vibra el dispositivo si está habilitado
        /// </summary>
        public static void Vibrate()
        {
            if (IsVibrationEnabled())
            {
#if UNITY_IOS || UNITY_ANDROID
                Handheld.Vibrate();
#endif
            }
        }

        /// <summary>
        /// Vibra ligeramente (para feedback de UI)
        /// </summary>
        public static void VibrateTap()
        {
            if (!IsVibrationEnabled()) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (AndroidJavaObject vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
                {
                    vibrator.Call("vibrate", 10L); // 10ms
                }
            }
            catch { }
#elif UNITY_IOS && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }

        /// <summary>
        /// Verifica si las notificaciones están habilitadas
        /// </summary>
        public static bool AreNotificationsEnabled()
        {
            return PlayerPrefs.GetInt(NOTIFICATIONS_KEY, 1) == 1;
        }

        /// <summary>
        /// Establece el estado de notificaciones
        /// </summary>
        public static void SetNotificationsEnabled(bool enabled)
        {
            PlayerPrefs.SetInt(NOTIFICATIONS_KEY, enabled ? 1 : 0);
            PlayerPrefs.Save();

            // Sincronizar con NotificationService si existe
            if (Services.Firebase.NotificationService.Instance != null)
            {
                Services.Firebase.NotificationService.Instance.SetNotificationsEnabled(enabled);
            }
        }

        #endregion

        #region Copy ID

        private void OnCopyIDClicked()
        {
            string playerID = currentPlayer?.userId ?? "Unknown";
            GUIUtility.systemCopyBuffer = playerID;
            Debug.Log($"[Settings] ID copiada al portapapeles: {playerID}");

            // Show toast notification
            if (InAppNotificationManager.Instance != null)
            {
                InAppNotificationManager.Instance.Show(
                    AutoLocalizer.Get("id_copied"), "", "info");
            }
        }

        #endregion

        #region Change Name

        private void UpdateNameChangeCost()
        {
            if (changeNameCostText == null) return;

            if (IsFirstNameChange())
            {
                changeNameCostText.text = AutoLocalizer.Get("free_label");
                changeNameCostText.color = new Color(0.2f, 0.8f, 0.4f); // green
            }
            else
            {
                changeNameCostText.text = $"{NAME_CHANGE_GEM_COST}";
                changeNameCostText.color = new Color(0f, 1f, 1f); // cyan
            }
        }

        private int GetNameChangeCount()
        {
            return PlayerPrefs.GetInt(NAME_CHANGE_COUNT_KEY, 0);
        }

        private bool IsFirstNameChange()
        {
            return GetNameChangeCount() == 0;
        }

        private void OnChangeNameButtonClicked()
        {
            Debug.Log("[Settings] Mostrando panel de cambio de nombre");

            int changeCount = GetNameChangeCount();

            // After first free change, check if user has enough gems
            if (changeCount > 0)
            {
                int currentGems = CurrencyManager.Instance?.Gems ?? 0;
                if (currentGems < NAME_CHANGE_GEM_COST)
                {
                    errorPanel?.Show(AutoLocalizer.Get("not_enough_gems_name_change", NAME_CHANGE_GEM_COST));
                    return;
                }
            }

            if (changeNamePanel != null)
            {
                changeNamePanel.SetLengthLimits(3, 20);
                changeNamePanel.Show(
                    changeCount == 0
                        ? AutoLocalizer.Get("change_name_title")
                        : AutoLocalizer.Get("change_name_title_cost", NAME_CHANGE_GEM_COST),
                    AutoLocalizer.Get("new_name_placeholder"),
                    OnConfirmNameClicked,
                    null
                );
            }
        }

        private async void OnConfirmNameClicked(string newUsername)
        {
            if (currentPlayer == null) return;

            if (newUsername == currentPlayer.username)
            {
                changeNamePanel?.Hide();
                return;
            }

            int changeCount = GetNameChangeCount();

            // Deduct gems if not first change
            if (changeCount > 0)
            {
                bool spent = CurrencyManager.Instance?.SpendGems(NAME_CHANGE_GEM_COST) ?? false;
                if (!spent)
                {
                    errorPanel?.Show(AutoLocalizer.Get("not_enough_gems_name_change", NAME_CHANGE_GEM_COST));
                    changeNamePanel?.SetButtonsInteractable(true);
                    return;
                }
            }

            Debug.Log($"[Settings] Cambiando nombre a: {newUsername}");

            bool success = await AuthenticationService.Instance.UpdateUsername(newUsername);

            if (success)
            {
                Debug.Log("[Settings] Nombre actualizado exitosamente");
                currentPlayer.username = newUsername;
                PlayerPrefs.SetInt(NAME_CHANGE_COUNT_KEY, changeCount + 1);
                PlayerPrefs.Save();
                UpdateNameChangeCost();
                changeNamePanel?.Hide();
            }
            else
            {
                Debug.LogError("[Settings] Error al actualizar nombre");
                changeNamePanel?.SetButtonsInteractable(true);
                errorPanel?.Show(AutoLocalizer.Get("error_changing_name"));
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
                    AutoLocalizer.Get("delete_account_title"),
                    AutoLocalizer.Get("delete_account_confirm_message"),
                    OnConfirmDeleteClicked,
                    null
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
                    errorPanel?.Show(AutoLocalizer.Get("error_deleting_account"));
                }
            }
            else
            {
                Debug.LogError("[Settings] AuthenticationService no disponible");
                deleteConfirmPanel?.Hide();
                errorPanel?.Show(AutoLocalizer.Get("error_server"));
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
                    AutoLocalizer.Get("logout_title"),
                    AutoLocalizer.Get("logout_confirm_message"),
                    OnConfirmLogoutClicked,
                    null
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
            Debug.Log("[Settings] Volviendo atrás");
            SceneNavigator.Instance?.GoBack();
        }

        #endregion

        #region Premium

        /// <summary>
        /// Actualiza la UI de premium según el estado actual
        /// </summary>
        private void UpdatePremiumUI()
        {
            if (PremiumManager.Instance == null) return;

            bool canCreateTournaments = PremiumManager.Instance.CanCreateTournaments;
            bool canCreateCashBattle = PremiumManager.Instance.CanCreateCashBattle;
            bool hasBundle = canCreateTournaments && canCreateCashBattle;

            // Actualizar boton de crear torneos
            if (createTournamentsButton != null)
            {
                createTournamentsButton.interactable = !canCreateTournaments;
                if (createTournamentsButtonText != null)
                {
                    if (canCreateTournaments)
                    {
                        createTournamentsButtonText.text = AutoLocalizer.Get("already_purchased");
                    }
                    else
                    {
                        createTournamentsButtonText.text = $"{AutoLocalizer.Get("create_tournaments_title")} - {PremiumManager.PRICE_CREATE_TOURNAMENTS}";
                    }
                }
            }

            // Actualizar boton de tournament bundle
            if (tournamentBundleButton != null)
            {
                tournamentBundleButton.interactable = !hasBundle;
                if (tournamentBundleButtonText != null)
                {
                    if (hasBundle)
                    {
                        tournamentBundleButtonText.text = AutoLocalizer.Get("already_purchased");
                    }
                    else
                    {
                        tournamentBundleButtonText.text = $"{AutoLocalizer.Get("tournament_bundle_title")} - {PremiumManager.PRICE_TOURNAMENT_BUNDLE}";
                    }
                }
            }

            // Ocultar seccion de premium si ya tiene todo
            if (premiumSection != null && hasBundle)
            {
                // Opcionalmente ocultar la seccion si tiene todo
                // premiumSection.SetActive(false);
            }

            Debug.Log($"[Settings] Premium UI actualizada - CreateTournaments: {canCreateTournaments}, CashBattle: {canCreateCashBattle}");

            // Actualizar el badge del boton Pro
            UpdatePremiumBadge();
        }

        /// <summary>
        /// Compra: Crear Torneos ($3.99 USD)
        /// </summary>
        private void OnCreateTournamentsClicked()
        {
            Debug.Log("[Settings] Iniciando compra: Crear Torneos");

            if (PremiumManager.Instance.CanCreateTournaments)
            {
                errorPanel?.Show(AutoLocalizer.Get("already_purchased"));
                return;
            }

            // Deshabilitar boton mientras se procesa
            if (createTournamentsButton != null) createTournamentsButton.interactable = false;

            PremiumManager.Instance.PurchaseCreateTournaments(success =>
            {
                if (success)
                {
                    errorPanel?.Show(AutoLocalizer.Get("purchase_success"));
                }
                else
                {
                    errorPanel?.Show(AutoLocalizer.Get("purchase_failed"));
                    if (createTournamentsButton != null) createTournamentsButton.interactable = true;
                }
            });
        }

        /// <summary>
        /// Compra: Tournament Bundle ($8.99 USD)
        /// </summary>
        private void OnTournamentBundleClicked()
        {
            Debug.Log("[Settings] Iniciando compra: Tournament Bundle");

            bool hasBundle = PremiumManager.Instance.CanCreateTournaments && PremiumManager.Instance.CanCreateCashBattle;
            if (hasBundle)
            {
                errorPanel?.Show(AutoLocalizer.Get("already_purchased"));
                return;
            }

            // Deshabilitar boton mientras se procesa
            if (tournamentBundleButton != null) tournamentBundleButton.interactable = false;

            PremiumManager.Instance.PurchaseTournamentBundle(success =>
            {
                if (success)
                {
                    errorPanel?.Show(AutoLocalizer.Get("purchase_success"));
                }
                else
                {
                    errorPanel?.Show(AutoLocalizer.Get("purchase_failed"));
                    if (tournamentBundleButton != null) tournamentBundleButton.interactable = true;
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

        /// <summary>
        /// Muestra el panel de compras premium
        /// </summary>
        private void OnPremiumButtonClicked()
        {
            Debug.Log("[Settings] Mostrando panel premium");

            if (premiumPanel != null)
            {
                premiumPanel.Show();
            }
            else
            {
                // Si no hay panel asignado, crear uno dinámicamente
                var canvas = UICanvasHelper.FindMainCanvas();
                if (canvas != null)
                {
                    PremiumPanelUI.CreateAndShow(canvas.transform);
                }
            }
        }

        /// <summary>
        /// Actualiza el badge del botón Pro
        /// </summary>
        private void UpdatePremiumBadge()
        {
            if (premiumBadge == null) return;

            bool isPremium = PremiumManager.Instance != null &&
                            PremiumManager.Instance.CanCreateTournaments &&
                            PremiumManager.Instance.CanCreateCashBattle;

            if (isPremium && !premiumBadge.activeSelf)
            {
                premiumBadge.SetActive(true);
                premiumBadge.transform.localScale = Vector3.zero;
                premiumBadge.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
            }
            else if (!isPremium)
            {
                premiumBadge.SetActive(false);
            }

            // Cambiar apariencia del botón si ya es premium
            if (premiumButton != null)
            {
                var buttonImage = premiumButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    // Si es premium, cambiar a color dorado suave
                    buttonImage.color = isPremium
                        ? new Color(0.15f, 0.2f, 0.3f, 1f) // Más sutil cuando ya es premium
                        : new Color(0.1f, 0.15f, 0.25f, 1f); // Color normal
                }
            }
        }

        #endregion

        #region Legal

        /// <summary>
        /// Abre una URL en el navegador del sistema
        /// </summary>
        private void OpenURL(string url)
        {
            Debug.Log($"[Settings] Abriendo URL: {url}");
            Application.OpenURL(url);
        }

        /// <summary>
        /// Muestra confirmacion de auto-exclusion de Cash Battle
        /// </summary>
        private void OnSelfExclusionClicked()
        {
            Debug.Log("[Settings] Mostrando panel de auto-exclusion");

            if (selfExclusionConfirmPanel != null)
            {
                selfExclusionConfirmPanel.Show(
                    AutoLocalizer.Get("self_exclusion_title"),
                    AutoLocalizer.Get("self_exclusion_message"),
                    OnConfirmSelfExclusion,
                    null
                );
            }
        }

        private void OnConfirmSelfExclusion()
        {
            Debug.Log("[Settings] Usuario solicito auto-exclusion");

            // TODO: Implementar llamada a Triumph SDK para auto-exclusion
            // Por ahora, mostrar mensaje de contacto
            selfExclusionConfirmPanel?.Hide();
            errorPanel?.Show(AutoLocalizer.Get("self_exclusion_contact_message"));
        }

        #endregion
    }
}
