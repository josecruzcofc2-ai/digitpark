using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace DigitPark.Localization
{
    /// <summary>
    /// Sistema de auto-localización híbrido
    /// Busca textos por nombre de GameObject y aplica traducciones automáticamente
    /// Se ejecuta al cargar cada escena y cuando cambia el idioma
    /// </summary>
    public class AutoLocalizer : MonoBehaviour
    {
        private static AutoLocalizer _instance;
        public static AutoLocalizer Instance => _instance;

        // Mapeo de nombres de GameObject a keys de localización
        private static readonly Dictionary<string, string> TextNameToKeyMap = new Dictionary<string, string>
        {
            // ==================== LOGIN ====================
            { "LoginTitle", "login_title" },
            { "LoginTitleText", "login_title" },
            { "EmailPlaceholder", "email_placeholder" },
            { "PasswordPlaceholder", "password_placeholder" },
            { "LoginButton", "login_button" },
            { "LoginButtonText", "login_button" },
            { "SignInButton", "login_button" },
            { "SignInButtonText", "login_button" },
            { "RegisterButton", "register_button" },
            { "RegisterButtonText", "register_button" },
            { "CreateAccountButton", "register_button" },
            { "CreateAccountButtonText", "register_button" },
            { "RememberMeText", "remember_me" },
            { "ForgotPasswordText", "forgot_password" },
            { "OrContinueWithText", "or_continue_with" },

            // ==================== MAIN MENU ====================
            { "PlayButton", "play_button" },
            { "PlayButtonText", "play_button" },
            { "PlayText", "play_button" },
            { "ScoresButton", "scores_button" },
            { "ScoresButtonText", "scores_button" },
            { "ScoresText", "scores_button" },
            { "TournamentButton", "tournament_button" },
            { "TournamentButtonText", "tournament_button" },
            { "TournamentsButton", "tournament_button" },
            { "TournamentsButtonText", "tournament_button" },
            { "TournamentsText", "tournament_button" },
            { "SettingsButton", "settings_button" },
            { "SettingsButtonText", "settings_button" },
            { "SettingsText", "settings_button" },

            // ==================== SETTINGS ====================
            { "SettingsTitle", "settings_title" },
            { "SettingsTitleText", "settings_title" },
            { "SoundVolumeText", "volume_sound" },
            { "SoundVolumeLabel", "volume_sound" },
            { "SoundVolumeSliderLabel", "volume_sound" },
            { "EffectsVolumeText", "volume_effects" },
            { "EffectsVolumeLabel", "volume_effects" },
            { "EffectsVolumeSliderLabel", "volume_effects" },
            { "ChangeNameButton", "change_name" },
            { "ChangeNameButtonText", "change_name" },
            { "ChangeUsernameButton", "change_name" },
            { "ChangeUsernameButtonText", "change_name" },
            { "LogoutButton", "logout_button" },
            { "LogoutButtonText", "logout_button" },
            { "DeleteAccountButton", "delete_account" },
            { "DeleteAccountButtonText", "delete_account" },
            { "LanguageText", "language" },
            { "LanguageLabel", "language" },
            { "ChangeLanguageText", "change_language" },
            { "ChangeLanguageLabel", "change_language" },
            { "ChangeLanguage", "change_language" },
            { "ChangueLanguageText", "change_language" }, // Typo in scene
            { "BackButton", "back_button" },
            { "BackButtonText", "back_button" },
            { "BackText", "back_button" },

            // ==================== CHANGE NAME PANEL ====================
            { "ChangeNameTitle", "change_name_title" },
            { "ChangeNameTitleText", "change_name_title" },
            { "NewNamePlaceholder", "new_name_placeholder" },
            { "ConfirmButton", "confirm_button" },
            { "ConfirmButtonText", "confirm_button" },
            { "CancelButton", "cancel_button" },
            { "CancelButtonText", "cancel_button" },

            // ==================== DELETE ACCOUNT ====================
            { "DeleteConfirmTitle", "delete_confirm_title" },
            { "DeleteConfirmTitleText", "delete_confirm_title" },
            { "DeleteConfirmMessage", "delete_confirm_message" },
            { "DeleteConfirmMessageText", "delete_confirm_message" },
            { "DeleteButton", "delete_button" },
            { "DeleteButtonText", "delete_button" },

            // ==================== GAME ====================
            { "TimerLabel", "timer_label" },
            { "TimerLabelText", "timer_label" },
            { "TimerText", "timer_label" },
            { "TimeLabel", "timer_label" },
            { "BestTimeLabel", "best_time" },
            { "BestTimeLabelText", "best_time" },
            { "BestTimeText", "best_time" },
            { "PlayAgainButton", "play_again" },
            { "PlayAgainButtonText", "play_again" },
            { "PlayAgainText", "play_again" },
            { "NewRecordText", "new_record" },
            { "NewRecordLabel", "new_record" },

            // ==================== LEADERBOARD / SCORES ====================
            { "LeaderboardTitle", "leaderboard_title" },
            { "LeaderboardTitleText", "leaderboard_title" },
            { "ScoresTitle", "leaderboard_title" },
            { "ScoresTitleText", "leaderboard_title" },
            { "GlobalTab", "global_tab" },
            { "GlobalTabText", "global_tab" },
            { "GlobalButton", "global_tab" },
            { "GlobalButtonText", "global_tab" },
            { "GlobalText", "global_tab" },
            { "CountryTab", "country_tab" },
            { "CountryTabText", "country_tab" },
            { "CountryButton", "country_tab" },
            { "CountryButtonText", "country_tab" },
            { "CountryText", "country_tab" },
            { "PositionLabel", "position" },
            { "PositionText", "position" },
            { "PlayerLabel", "player" },
            { "PlayerText", "player" },
            { "TimeHeaderLabel", "time" },
            { "TimeHeaderText", "time" },
            { "TimeText", "time" },

            // ==================== TOURNAMENTS ====================
            { "TournamentsTitle", "tournaments_title" },
            { "TournamentsTitleText", "tournaments_title" },
            { "SearchTab", "search_tab" },
            { "SearchTabText", "search_tab" },
            { "SearchButton", "search" },
            { "SearchButtonText", "search" },
            { "MyTournamentsTab", "my_tournaments_tab" },
            { "MyTournamentsTabText", "my_tournaments_tab" },
            { "MyTournamentsButton", "my_tournaments_tab" },
            { "MyTournamentsButtonText", "my_tournaments_tab" },
            { "CreateTab", "create_tab" },
            { "CreateTabText", "create_tab" },
            { "CreateButton", "create_tournament" },
            { "CreateButtonText", "create_tournament" },
            { "CreateTournamentButton", "create_tournament" },
            { "CreateTournamentButtonText", "create_tournament" },
            { "JoinButton", "join_tournament" },
            { "JoinButtonText", "join_tournament" },
            { "JoinTournamentButton", "join_tournament" },
            { "ExitTournamentButton", "exit_tournament" },
            { "ExitTournamentButtonText", "exit_tournament" },
            { "EntryFeeLabel", "entry_fee" },
            { "PrizePoolLabel", "prize_pool" },
            { "ParticipantsLabel", "participants" },
            { "MaxPlayersLabel", "max_players" },
            { "MaxPlayersText", "max_players" },
            { "DurationLabel", "duration" },
            { "DurationText", "duration" },
            { "PublicToggleText", "public" },
            { "PublicLabel", "public" },
            { "PrivateToggleText", "private" },
            { "PrivateLabel", "private" },
            { "ExitConfirmTitle", "exit_confirm_title" },
            { "ExitConfirmTitleText", "exit_confirm_title" },
            { "ExitConfirmMessage", "exit_confirm_message" },
            { "ExitConfirmMessageText", "exit_confirm_message" },
            { "ConfirmExitButton", "confirm_button" },
            { "ConfirmExitButtonText", "confirm_button" },
            { "CancelExitButton", "cancel_button" },
            { "CancelExitButtonText", "cancel_button" },

            // ==================== GENERAL ====================
            { "LoadingText", "loading" },
            { "ErrorText", "error" },
            { "SuccessText", "success" },
            { "YesButton", "yes" },
            { "YesButtonText", "yes" },
            { "NoButton", "no" },
            { "NoButtonText", "no" },
            { "OkButton", "ok" },
            { "OkButtonText", "ok" },
            { "CloseButton", "close" },
            { "CloseButtonText", "close" },
            { "SaveButton", "save" },
            { "SaveButtonText", "save" },
            { "ApplyButton", "apply" },
            { "ApplyButtonText", "apply" },
            { "ClearButton", "clear" },
            { "ClearButtonText", "clear" },
            { "FilterButton", "filter" },
            { "FilterButtonText", "filter" },
            { "OptionsButton", "options" },
            { "OptionsButtonText", "options" },

            // ==================== USERNAME POPUP ====================
            { "UsernamePopupTitle", "username_popup_title" },
            { "UsernamePopupTitleText", "username_popup_title" },
            { "UsernamePlaceholder", "username_placeholder" },

            // ==================== LOGOUT CONFIRM ====================
            { "LogoutConfirmTitle", "logout_confirm_title" },
            { "LogoutConfirmTitleText", "logout_confirm_title" },
            { "LogoutConfirmMessage", "logout_confirm_message" },
            { "LogoutConfirmMessageText", "logout_confirm_message" },

            // ==================== SCORES TABS ====================
            { "PersonalTab", "personal_tab" },
            { "PersonalTabText", "personal_tab" },

            // ==================== SEARCH OPTIONS ====================
            { "SearchOptionsTitle", "search_options_title" },
            { "SearchOptionsTitleText", "search_options_title" },

            // ==================== CREATE TOURNAMENT ====================
            { "CreateTournamentTitle", "create_tournament_title" },
            { "CreateTournamentTitleText", "create_tournament_title" },

            // ==================== TOURNAMENTS - Additional Labels ====================
            { "MinTimeLabel", "min_time" },
            { "MaxTimeLabel", "max_time" },
            { "MinPlayersLabel", "min_players" },
            { "PlayersLabel", "participants" },
            { "TypeLabel", "type" },
            { "SearchLabel", "search" },
            { "LeaderboardBackButton", "back_button" },
            { "LeaderboardBackButtonText", "back_button" },

            // ==================== REGISTER ====================
            { "RegisterTitleText", "register_title" },

            // ==================== PREMIUM ====================
            { "PremiumSectionTitle", "premium_section_title" },
            { "PremiumSectionTitleText", "premium_section_title" },
            { "PremiumTitle", "premium_section_title" },
            { "RemoveAdsButton", "remove_ads_title" },
            { "RemoveAdsButtonText", "remove_ads_title" },
            { "RemoveAdsTitle", "remove_ads_title" },
            { "RemoveAdsDescription", "remove_ads_description" },
            { "PremiumFullButton", "premium_full_title" },
            { "PremiumFullButtonText", "premium_full_title" },
            { "PremiumFullTitle", "premium_full_title" },
            { "PremiumFullDescription", "premium_full_description" },
            { "RestorePurchasesButton", "restore_purchases" },
            { "RestorePurchasesButtonText", "restore_purchases" },
            { "RestorePurchasesText", "restore_purchases" },
            { "PremiumRequiredTitle", "premium_required_title" },
            { "PremiumRequiredTitleText", "premium_required_title" },
            { "PremiumRequiredMessage", "premium_required_message" },
            { "PremiumRequiredMessageText", "premium_required_message" },
            { "GetPremiumButton", "get_premium" },
            { "GetPremiumButtonText", "get_premium" },
            { "MaybeLaterButton", "maybe_later" },
            { "MaybeLaterButtonText", "maybe_later" },
        };

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);

                // Suscribirse a eventos
                SceneManager.sceneLoaded += OnSceneLoaded;
                LocalizationManager.OnLanguageChanged += OnLanguageChanged;

                Debug.Log("[AutoLocalizer] Inicializado");
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"[AutoLocalizer] Escena cargada: {scene.name}");
            // Pequeño delay para asegurar que todos los objetos están inicializados
            Invoke(nameof(LocalizeAllTexts), 0.1f);
        }

        private void OnLanguageChanged()
        {
            Debug.Log("[AutoLocalizer] Idioma cambiado, re-localizando...");
            LocalizeAllTexts();
        }

        /// <summary>
        /// Localiza todos los textos en la escena actual
        /// </summary>
        public void LocalizeAllTexts()
        {
            if (LocalizationManager.Instance == null)
            {
                Debug.LogWarning("[AutoLocalizer] LocalizationManager no disponible");
                return;
            }

            int localizedCount = 0;

            // Buscar todos los TextMeshProUGUI
            TextMeshProUGUI[] tmpTexts = FindObjectsOfType<TextMeshProUGUI>(true);
            foreach (var tmp in tmpTexts)
            {
                if (TryLocalizeByName(tmp.gameObject.name, out string key))
                {
                    tmp.text = LocalizationManager.Instance.GetText(key);
                    localizedCount++;
                }
            }

            // Buscar todos los Text legacy
            Text[] legacyTexts = FindObjectsOfType<Text>(true);
            foreach (var text in legacyTexts)
            {
                if (TryLocalizeByName(text.gameObject.name, out string key))
                {
                    text.text = LocalizationManager.Instance.GetText(key);
                    localizedCount++;
                }
            }

            Debug.Log($"[AutoLocalizer] {localizedCount} textos localizados en escena");
        }

        /// <summary>
        /// Intenta encontrar una key de localización basada en el nombre del GameObject
        /// </summary>
        private bool TryLocalizeByName(string gameObjectName, out string key)
        {
            // Buscar coincidencia exacta
            if (TextNameToKeyMap.TryGetValue(gameObjectName, out key))
            {
                return true;
            }

            // Buscar sin sufijos comunes (Text, Label, Button, etc.)
            string cleanName = gameObjectName
                .Replace("Text", "")
                .Replace("Label", "")
                .Replace("Button", "")
                .Replace(" ", "")
                .Trim();

            if (!string.IsNullOrEmpty(cleanName) && TextNameToKeyMap.TryGetValue(cleanName, out key))
            {
                return true;
            }

            // Buscar con sufijo Text
            if (TextNameToKeyMap.TryGetValue(gameObjectName + "Text", out key))
            {
                return true;
            }

            key = null;
            return false;
        }

        /// <summary>
        /// Registra un nuevo mapeo de nombre a key (para extensibilidad)
        /// </summary>
        public static void RegisterTextMapping(string gameObjectName, string localizationKey)
        {
            TextNameToKeyMap[gameObjectName] = localizationKey;
        }

        /// <summary>
        /// Localiza un texto específico por su key
        /// Útil para textos dinámicos que necesitan actualización manual
        /// </summary>
        public static void LocalizeText(TextMeshProUGUI textComponent, string key)
        {
            if (textComponent != null && LocalizationManager.Instance != null)
            {
                textComponent.text = LocalizationManager.Instance.GetText(key);
            }
        }

        /// <summary>
        /// Localiza un texto específico por su key con parámetros
        /// Útil para textos dinámicos como "Tiempo: {0}s"
        /// </summary>
        public static void LocalizeText(TextMeshProUGUI textComponent, string key, params object[] args)
        {
            if (textComponent != null && LocalizationManager.Instance != null)
            {
                textComponent.text = LocalizationManager.Instance.GetText(key, args);
            }
        }

        /// <summary>
        /// Localiza un texto legacy por su key
        /// </summary>
        public static void LocalizeText(Text textComponent, string key)
        {
            if (textComponent != null && LocalizationManager.Instance != null)
            {
                textComponent.text = LocalizationManager.Instance.GetText(key);
            }
        }

        /// <summary>
        /// Obtiene el texto localizado directamente
        /// Shortcut para LocalizationManager.Instance.GetText()
        /// </summary>
        public static string Get(string key)
        {
            return LocalizationManager.Instance?.GetText(key) ?? key;
        }

        /// <summary>
        /// Obtiene el texto localizado con parámetros
        /// </summary>
        public static string Get(string key, params object[] args)
        {
            return LocalizationManager.Instance?.GetText(key, args) ?? key;
        }
    }
}
