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
            { "GoogleSignInText", "sign_in_google" },
            { "AppleSignInText", "sign_in_apple" },

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

            // Theme/Style
            { "ChangeStyleText", "change_style" },
            { "ChangeStyleLabel", "change_style" },
            { "ChangeStyle", "change_style" },
            { "ChangeThemeText", "change_style" },
            { "ChangeThemeLabel", "change_style" },
            { "ThemeLabel", "change_style" },

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
            { "SearchOptionsButtonText", "search_tournament" },
            { "SearchTournamentButton", "search_tournament" },
            { "SearchTournamentButtonText", "search_tournament" },
            { "SearchOptionsTitle", "search_options" },
            { "SearchOptionsTitleText", "search_options" },
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

            // ==================== BOOT ====================
            { "Subtitle", "boot_subtitle" },
            { "Subtitle2", "boot_subtitle2" },
            { "LoadingText", "boot_loading" },

            // ==================== GENERAL ====================
            { "GeneralLoadingText", "loading" },
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

            // ==================== CREATE TOURNAMENT ====================
            { "CreateTournamentTitle", "create_tournament_title" },
            { "CreateTournamentTitleText", "create_tournament_title" },

            // ==================== TOURNAMENTS - Additional Labels ====================
            { "UsernameSearchPlaceholder", "username_search_placeholder" },
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
            { "EmailInputPlaceholder", "email_placeholder" },
            { "PasswordInputPlaceholder", "password_placeholder" },
            { "ConfirmPasswordInputPlaceholder", "confirm_password_placeholder" },
            { "UsernameInputPlaceholder", "username_placeholder" },
            { "AlreadyHaveAccountText", "already_have_account" },
            { "BackToLoginText", "back_to_login" },
            { "BackToLoginButtonText", "back_to_login" },

            // ==================== PREMIUM ====================
            { "PremiumButtonText", "premium_button" },
            { "PremiumBannerText", "premium_banner" },
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
            { "NoAdsTitle", "no_ads_title" },
            { "NoAdsTitleText", "no_ads_title" },
            { "NoAdsDescription", "no_ads_description" },
            { "NoAdsDescriptionText", "no_ads_description" },
            { "NoAdsPrice", "no_ads_price" },
            { "NoAdsPriceText", "no_ads_price" },
            { "PremiumFullPrice", "premium_full_price" },
            { "PremiumFullPriceText", "premium_full_price" },
            { "BuyButton", "buy_button" },
            { "BuyButtonText", "buy_button" },
            { "RecommendedBadge", "premium_recommended" },
            { "RecommendedBadgeText", "premium_recommended" },
            { "TiredOfAds", "tired_of_ads" },
            { "TiredOfAdsText", "tired_of_ads" },
            { "NoThanks", "no_thanks" },
            { "NoThanksText", "no_thanks" },
            { "NoThanksButton", "no_thanks" },
            { "Feature1Text", "premium_feature_no_ads" },
            { "Feature2Text", "premium_feature_tournaments" },
            { "Feature3Text", "premium_feature_badge" },
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
        /// Localiza todos los textos en la escena actual y los centra automáticamente
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
                    // Centrar el texto automáticamente al localizar
                    CenterTextIfNeeded(tmp);
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
                    // Centrar el texto automáticamente al localizar
                    CenterLegacyTextIfNeeded(text);
                    localizedCount++;
                }
            }

            Debug.Log($"[AutoLocalizer] {localizedCount} textos localizados en escena");
        }

        /// <summary>
        /// Centra un TextMeshProUGUI y configura Auto-Size para que el texto siempre quepa
        /// </summary>
        private void CenterTextIfNeeded(TextMeshProUGUI tmp)
        {
            if (tmp == null) return;

            string name = tmp.gameObject.name.ToLower();

            // Aplicar a títulos, botones, labels y placeholders
            bool shouldProcess = name.Contains("title") ||
                name.Contains("button") ||
                name.Contains("label") ||
                name.Contains("placeholder") ||
                name.Contains("tab") ||
                name.Contains("header") ||
                name.Contains("text");

            if (shouldProcess)
            {
                // FORZAR CENTRADO para textos específicos de Settings (Change Language, Change Style)
                // Estos textos tienen iconos al lado, solo forzar Left alignment para consistencia
                if (name.Contains("changelanguage") || name.Contains("changestyle") ||
                    name.Contains("changetheme"))
                {
                    tmp.alignment = TextAlignmentOptions.Left;
                    return; // No aplicar más transformaciones a estos textos
                }

                // 1. CENTRAR EL TEXTO - Forzar centrado horizontal
                TextAlignmentOptions currentAlign = tmp.alignment;

                // Convertir cualquier alineación a centrado horizontal manteniendo vertical
                if (currentAlign == TextAlignmentOptions.Left || currentAlign == TextAlignmentOptions.TopLeft)
                    tmp.alignment = TextAlignmentOptions.Top;
                else if (currentAlign == TextAlignmentOptions.MidlineLeft || currentAlign == TextAlignmentOptions.Midline)
                    tmp.alignment = TextAlignmentOptions.Center;
                else if (currentAlign == TextAlignmentOptions.BottomLeft)
                    tmp.alignment = TextAlignmentOptions.Bottom;
                else if (currentAlign == TextAlignmentOptions.BaselineLeft)
                    tmp.alignment = TextAlignmentOptions.Baseline;
                else if (currentAlign == TextAlignmentOptions.CaplineLeft)
                    tmp.alignment = TextAlignmentOptions.Capline;
                else if (currentAlign == TextAlignmentOptions.TopRight)
                    tmp.alignment = TextAlignmentOptions.Top;
                else if (currentAlign == TextAlignmentOptions.Right || currentAlign == TextAlignmentOptions.MidlineRight)
                    tmp.alignment = TextAlignmentOptions.Center;
                else if (currentAlign == TextAlignmentOptions.BottomRight)
                    tmp.alignment = TextAlignmentOptions.Bottom;
                // Si ya está centrado, dejarlo como está

                // 2. HABILITAR AUTO-SIZE para que el texto se ajuste al contenedor
                if (!tmp.enableAutoSizing)
                {
                    tmp.enableAutoSizing = true;
                    // Configurar tamaños mínimo y máximo razonables
                    float currentSize = tmp.fontSize;
                    tmp.fontSizeMin = Mathf.Max(8f, currentSize * 0.4f); // Mínimo 40% del tamaño original o 8
                    tmp.fontSizeMax = currentSize > 0 ? currentSize : 36f; // Máximo el tamaño original
                }

                // 3. CONFIGURAR OVERFLOW para evitar que el texto se salga
                tmp.overflowMode = TextOverflowModes.Ellipsis; // Mostrar ... si aún no cabe
            }
        }

        /// <summary>
        /// Centra un Text legacy y configura para que el texto quepa
        /// </summary>
        private void CenterLegacyTextIfNeeded(Text text)
        {
            if (text == null) return;

            string name = text.gameObject.name.ToLower();

            // Aplicar a títulos, botones, labels y placeholders
            bool shouldProcess = name.Contains("title") ||
                name.Contains("button") ||
                name.Contains("label") ||
                name.Contains("placeholder") ||
                name.Contains("tab") ||
                name.Contains("header") ||
                name.Contains("text");

            if (shouldProcess)
            {
                // 1. CENTRAR EL TEXTO - Forzar centrado horizontal
                if (text.alignment == TextAnchor.UpperLeft || text.alignment == TextAnchor.UpperRight)
                    text.alignment = TextAnchor.UpperCenter;
                else if (text.alignment == TextAnchor.MiddleLeft || text.alignment == TextAnchor.MiddleRight)
                    text.alignment = TextAnchor.MiddleCenter;
                else if (text.alignment == TextAnchor.LowerLeft || text.alignment == TextAnchor.LowerRight)
                    text.alignment = TextAnchor.LowerCenter;

                // 2. HABILITAR BEST FIT para que el texto se ajuste al contenedor
                if (!text.resizeTextForBestFit)
                {
                    text.resizeTextForBestFit = true;
                    text.resizeTextMinSize = 8;
                    text.resizeTextMaxSize = text.fontSize > 0 ? text.fontSize : 36;
                }

                // 3. CONFIGURAR OVERFLOW
                text.horizontalOverflow = HorizontalWrapMode.Wrap;
                text.verticalOverflow = VerticalWrapMode.Truncate;
            }
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
