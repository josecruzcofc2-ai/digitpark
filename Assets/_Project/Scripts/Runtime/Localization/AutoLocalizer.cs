using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using DigitPark.UI;

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
            { "OrText", "login_or_divider" },
            { "RegisterButton", "register_button" },
            { "RegisterButtonText", "register_button" },
            { "CreateAccountButton", "register_button" },
            { "CreateAccountButtonText", "register_button" },
            { "RememberMeText", "remember_me" },
            { "ForgotPasswordText", "forgot_password" },
            { "ForgotPasswordButton", "forgot_password" },
            { "NoAccountText", "no_account" },
            { "OrContinueWithText", "or_continue_with" },
            { "GoogleSignInText", "sign_in_google" },
            { "AppleSignInText", "sign_in_apple" },
            { "GeneralLoadingText", "loading" },
            { "CreatingAccountText", "creating_account" },

            // ==================== MAIN MENU ====================
            { "PlayButton", "play_button" },
            { "PlayButtonText", "play_button" },
            { "PlayText", "play_button" },
            { "ScoresButton", "scores_button" },
            { "ScoresButtonText", "scores_button" },
            { "ScoresText", "scores_button" },
            { "SettingsButton", "settings_button" },
            { "SettingsButtonText", "settings_button" },
            { "SettingsText", "settings_button" },

            // Main Menu Cards
            { "PlayCardTitle", "play_title" },
            { "PlayCardSubtitle", "play_subtitle" },
            { "ClaimButtonText", "claim" },
            { "RankingsLabel", "rankings" },
            { "SearchLabel", "search" },
            { "ShopLabel", "shop_title" },
            { "PremiumLabel", "premium_section_title" },

            // Player Card
            { "OnlineLabel", "online" },

            // ==================== SETTINGS ====================
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

            // Toggle
            { "VibrationToggleLabel", "vibration" },

            // Theme/Style
            { "ChangeStyleText", "change_style" },
            { "ChangeStyleLabel", "change_style" },
            { "ChangeStyle", "change_style" },
            { "ChangeThemeText", "change_style" },
            { "ChangeThemeLabel", "change_style" },
            { "ThemeLabel", "change_style" },

            // Settings Section Titles
            { "AccountCardTitle", "settings_account" },
            { "AudioCardTitle", "settings_audio" },
            { "AppearanceCardTitle", "settings_appearance" },
            // PremiumSectionTitle already mapped in PREMIUM section below
            { "LegalCardTitle", "settings_legal" },
            { "DangerCardTitle", "settings_danger_zone" },

            // Settings Button Labels
            { "ShopButtonText", "shop_nav" },
            // RestorePurchasesButtonText already mapped in PREMIUM section below
            // RemoveAdsButtonText already mapped in PREMIUM section below
            // PremiumFullButtonText already mapped in PREMIUM section below
            { "TermsButtonText", "terms_conditions" },
            { "PrivacyButtonText", "privacy_policy" },
            { "ResponsibleGamingButtonText", "responsible_gaming" },
            { "SelfExclusionButtonText", "self_exclusion" },

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
            { "NationalTab", "national_tab" },
            { "NationalTabText", "national_tab" },
            { "ScoresPositionLabel", "your_position" },
            { "PositionText", "position" },
            { "PlayerLabel", "player" },
            { "PlayerText", "player" },
            { "TimeHeaderLabel", "time" },
            { "TimeHeaderText", "time" },
            { "TimeText", "time" },

            // ==================== TOURNAMENTS ====================
            { "SearchTab", "search_tab" },
            { "SearchTabText", "search_tab" },
            { "SearchButton", "search" },
            { "SearchButtonText", "search" },
            { "SearchOptionsTitle", "search_options" },
            { "SearchOptionsTitleText", "search_options" },
            { "CreateTab", "create_tab" },
            { "CreateTabText", "create_tab" },
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
            { "ErrorTitleText", "error_title" },
            { "Subtitle2", "boot_subtitle2" },
            { "LoadingText", "boot_loading" },

            // ==================== GENERAL ====================
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

            // ==================== TOURNAMENTS - Additional Labels ====================
            { "UsernameSearchPlaceholder", "username_search_placeholder" },
            { "MinTimeLabel", "min_time" },
            { "MaxTimeLabel", "max_time" },
            { "MinPlayersLabel", "min_players" },
            { "PlayersLabel", "participants" },
            { "TypeLabel", "type" },
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
            { "Feature3Text", "premium_feature_badge" },

            // ==================== PLAY MODE SELECTION ====================
            { "PlayModeTitleText", "play_mode_title" },
            { "PlayModeTitle", "play_mode_title" },
            { "SoloTitleText", "solo_title" },
            { "SoloTitle", "solo_title" },
            { "SoloDescText", "solo_description" },
            { "SoloDescription", "solo_description" },
            { "OneVsOneTitleText", "1v1_title" },
            { "OneVsOneTitle", "1v1_title" },
            { "OneVsOneDescText", "1v1_description" },
            { "OneVsOneDescription", "1v1_description" },

            // ==================== WIN/LOSE PANELS ====================
            { "CompletedText", "completed" },
            { "WinText", "you_won" },
            { "LoseText", "you_lost" },
            { "TimeoutText", "time_expired" },
            { "AcceptButton", "accept_button" },
            { "AcceptButtonText", "accept_button" },
            { "RetryButton", "retry_button" },
            { "RetryButtonText", "retry_button" },
            { "ExitButton", "exit_button" },
            { "ExitButtonText", "exit_button" },
            { "RematchButton", "rematch_button" },
            { "RematchButtonText", "rematch_button" },
            { "ErrorsLabel", "errors_label" },
            { "ErrorsLabelText", "errors_label" },
            { "YourTimeLabel", "your_time" },

            // ==================== MATCH HISTORY ====================
            { "MatchHistoryTitle", "match_history_title" },
            { "MatchHistoryTitleText", "match_history_title" },
            { "TotalCountText", "history_match_count" },
            { "LoadMoreButton", "load_more" },
            { "LoadMoreButtonText", "load_more" },

            // ==================== DAILY MISSIONS ====================
            { "DailyProgressTitle", "daily_progress" },
            { "DailyProgressText", "daily_progress" },
            { "ResetsIn", "resets_in" },
            { "ResetsInText", "resets_in" },
            { "CollectButton", "collect_button" },
            { "CollectButtonText", "collect_button" },
            { "BonusText", "bonus_text" },

            // ==================== DAILY REWARDS ====================
            { "StreakLabel", "streak_label" },
            { "StreakLabelText", "streak_label" },
            { "TodayBadge", "today_badge" },
            { "TodayBadgeText", "today_badge" },
            { "TodaysReward", "todays_reward" },
            { "TodaysRewardText", "todays_reward" },
            { "ClaimRewardButton", "claim_reward_button" },
            { "ClaimRewardButtonText", "claim_reward_button" },
            { "NextRewardIn", "next_reward_in" },
            { "NextRewardInText", "next_reward_in" },
            { "ContinueButton", "continue_button" },
            { "ContinueButtonText", "continue_button" },

            // ==================== SHOP ====================
            { "ShopTitle", "shop_title" },
            { "ShopTitleText", "shop_title" },
            { "SpecialOffers", "special_offers" },
            { "SpecialOffersText", "special_offers" },
            { "DailyOffers", "daily_offers" },
            { "DailyOffersText", "daily_offers" },
            { "CoinsSection", "coins_section" },
            { "ThemesSection", "themes_section" },
            { "PremiumBundleBannerTitle", "shop_premium_bundle" },
            { "PremiumBundleBannerDesc", "shop_premium_bundle_desc" },
            { "CompleteBundleBannerTitle", "shop_complete_bundle" },
            { "CompleteBundleBannerDesc", "shop_complete_bundle_desc" },
            { "PremiumBundleTitle", "styles_premium_bundle_btn" },
            { "CompleteBundleTitle", "styles_complete_bundle_btn" },
            { "FramesSection", "frames_section" },
            { "TitlesSection", "titles_section" },
            { "EquippedStatus", "equipped_status" },
            { "EquippedStatusText", "equipped_status" },
            { "InUseStatus", "in_use_status" },
            { "InUseStatusText", "in_use_status" },

            // ==================== TOURNAMENTS EXTRA ====================
            { "FeaturedTab", "featured_tab" },
            { "FeaturedTabText", "featured_tab" },
            { "FiltersButton", "filters_button" },
            { "FiltersButtonText", "filters_button" },
            { "ParticipantsTab", "participants_tab" },
            { "ParticipantsTabText", "participants_tab" },
            { "PrizesLabel", "prizes_label" },
            { "PrizesLabelText", "prizes_label" },
            { "ShareButton", "share_button" },
            { "ShareButtonText", "share_button" },

            // ==================== ONBOARDING ====================
            { "WelcomeTitle", "welcome_title" },
            { "WelcomeTitleText", "welcome_title" },
            { "WelcomeSubtitle", "welcome_subtitle" },
            { "WelcomeSubtitleText", "welcome_subtitle" },
            { "NamePlaceholder", "name_placeholder" },
            { "SkipButton", "skip_button" },
            { "SkipButtonText", "skip_button" },
            { "NextButton", "next_button" },
            { "NextButtonText", "next_button" },
            { "StartButton", "start_button" },
            { "StartButtonText", "start_button" },

            // ==================== GAME SELECTOR ====================
            { "RulesDescText", "game_rules_label" },

            // ==================== PLAY MODE SELECTION (extra) ====================
            { "SubtitleText", "play_mode_subtitle" },

            // ==================== BET SELECTION ====================
            { "FreeBetCostText", "bet_free" },
            { "FreeBetRewardText", "bet_free_reward" },
            { "CustomRewardText", "bet_custom_reward" },
            { "CustomSectionText", "bet_custom_section" },
            { "CoinsSectionText", "bet_coins_section" },
            { "GemsSectionText", "bet_gems_section" },
            { "CoinsToggleText", "bet_coins_section" },
            { "GemsToggleText", "bet_gems_section" },
            { "AmountPlaceholder", "amount_placeholder" },

            // ==================== GAME SELECTOR ====================
            { "GameSelectorTitleText", "select_game" },

            // ==================== MATCHMAKING ====================

            // ==================== MINIGAME SETTINGS ====================
            { "DigitRushTitle", "game_digitrush" },
            { "DigitRushSubtitle", "game_digitrush_desc" },
            { "FlashTapTitle", "game_flashtap" },
            { "FlashTapSubtitle", "game_flashtap_desc" },
            { "MemoryPairsTitle", "game_memorypairs" },
            { "MemoryPairsSubtitle", "game_memorypairs_desc" },
            { "OddOneOutTitle", "game_oddoneout" },
            { "OddOneOutSubtitle", "game_oddoneout_desc" },
            { "QuickMathTitle", "game_quickmath" },
            { "QuickMathSubtitle", "game_quickmath_desc" },
            { "RoundsHeaderText", "rounds_header" },
            { "StartText", "start_game" },
            { "InstructionText", "flashtap_wait" },
            { "OperationsHeaderText", "operations_header" },
            { "DifficultyHeaderText", "difficulty_header" },
            { "DifficultyDescText", "difficulty_desc" },

            // ==================== MINIGAME WIN/LOSE PANELS ====================
            { "ResultTitleText", "completed" },
            { "ResultTimeText", "result_time" },
            { "ResultMessageText", "result_message" },
            { "AvgTimeValue_Label", "avg_time_label" },
            { "BestTimeValue_Label", "best_time_label" },
            { "ErrorsValue_Label", "errors_label" },
            { "PenaltyValue_Label", "penalty_label" },
            { "RatingValue_Label", "rating_label" },
            { "TimeText_Label", "time_label" },
            { "PanelErrorsText_Label", "errors_label" },
            { "MaxComboValue_Label", "max_combo_label" },
            { "MaxStreakValue_Label", "max_streak_label" },
            { "RoundsValue_Label", "rounds_label" },
            { "LoseTimeText_Label", "time_label" },
            { "LosePanelErrorsText_Label", "errors_label" },
            { "LoseMaxComboValue_Label", "max_combo_label" },
            { "LoseMaxStreakValue_Label", "max_streak_label" },
            { "LosePenaltyValue_Label", "penalty_label" },
            { "LoseRoundsValue_Label", "rounds_label" },
            { "ComboText", "combo_display" },

            // Scores EmptyState
            { "EmptyTitle", "empty_leaderboard_title" },
            { "EmptySubtitle", "empty_leaderboard_subtitle" },

            // ==================== AGE VERIFICATION FIX ====================
            { "VerifyingText", "verifying" },

            // ==================== SETTINGS (V41) ====================
            { "CopyButtonText", "settings_copy" },
            { "PremiumPanelTitle", "premium_panel_title" },
            { "ChangeNamePlaceholder", "change_name_placeholder" },

            // ==================== SOCIAL - SCORES (V41) ====================
            { "RankingsTitleText", "rankings_title" },
            { "NoScoresText", "no_scores_yet" },
            { "PositionHeader", "position_header" },
            { "PlayerHeader", "player_header" },
            { "TimeHeader", "time_header" },
            { "CountryHeader", "country_header" },
            { "YourPositionText", "your_position" },
            { "WeeklyTabText", "weekly_tab" },
            { "AllTimeTabText", "all_time_tab" },
            { "ByGameTabText", "by_game_tab" },

            // ==================== TOURNAMENT LOBBY (V41) ====================
            { "GameTypeText", "game_type" },
            { "RankCol", "rank_column" },
            { "PlayerCol", "player_column" },
            { "LeaveButtonText", "leave" },

            // ==================== WIN PANELS (V42) ====================
            { "WinTitle", "completed" },
            { "LoseTitle", "lose_times_up" },
            { "LoseSubtitleText", "lose_subtitle" },
            { "MoneyWinTitle", "you_won" },
            { "MoneyLoseTitle", "defeat_title" },
            { "LoseRealMoneySubtitleText", "lose_money_subtitle" },
            { "PlayerScoreLabel", "player_you" },
            { "OpponentScoreLabel", "opponent_label" },
            { "OnlineWinTitle", "you_won" },
            { "OnlineLoseTitle", "defeat_title" },

            { "GameHeader", "header_game" },
            { "ErrorsHeader", "header_errors" },
            { "TotalTimeLabel", "total_time" },
            { "TotalErrorsLabel", "total_errors" },

            // ==================== TOURNAMENT RESULT (V42) ====================
            { "ScoreLabel", "score_label" },
            { "PositionLabel", "position_label" },
            { "PrizeLabel", "prize_label" },
            { "AttemptsText", "attempts_text" },

            // ==================== ITEM PREFABS (V42) ====================
            { "LabelMyPosition", "label_my_position" },
            { "LabelCreator", "label_creator" },
            { "LabelCreatorTime", "label_time" },
            { "LabelMyTime", "label_my_best" },

            // ==================== TOURNAMENT CREATE SECTIONS (V42) ====================
            { "GameTypeLabel", "game_type" },
            { "PlayersAndPrizeLabel", "players_and_prize" },
            { "StartScheduleLabel", "start_schedule" },
            { "PrivacyLabel", "privacy_label" },
            { "PreviewLabel", "preview_label" },

            // ==================== TOURNAMENT LOBBY POPUPS (V42) ====================
            { "PrizesPopupTitle", "prizes_title" },
            { "PrizesCloseText", "close" },
            { "FirstPlaceLabel", "first_place" },
            { "SecondPlaceLabel", "second_place" },
            { "ThirdPlaceLabel", "third_place" },
            { "StayButtonText", "stay_button" },
            { "ClearFiltersText", "clear_filters" },

            // ==================== SHOP POPUPS (V43 AUDIT) ====================
            { "StarterPackTitle", "starter_pack_title" },
            { "StarterPackContents", "starter_pack_contents" },
            { "BundlePremiumTitle", "bundle_premium_title" },
            { "BundlePremiumDesc", "bundle_premium_desc" },
            { "ShopConfirmTitle", "shop_confirm_purchase_title" },
            { "ShopInsufficientTitle", "shop_insufficient_title" },
            { "ShopInsufficientMessage", "shop_insufficient_message" },
            { "StatusBadge", "status_in_use" },
            { "GetGemsButtonText", "get_digitgems" },

            // ==================== PROFILE (V43 AUDIT) ====================
            { "ChallengeText", "challenge_button" },
            { "GameSelectionTitle", "choose_game_title" },

            // ==================== DAILY MISSIONS (V43 AUDIT) ====================
            { "ResetsInLabel", "resets_in" },
            { "TitleLeft", "daily_progress" },
            { "ActionButtonText", "claim" },

            // ==================== DAILY REWARDS (V43 AUDIT) ====================
            { "GrandPrizeLabel", "day7_grand_prize" },
            { "Reward2", "exclusive_item_bonus" },
            { "ClaimRewardText", "claim_reward_button" },
            { "NextRewardLabel", "next_reward_in" },
            { "TapToContinueText", "tap_to_continue" },

            // ==================== ACHIEVEMENTS (V43 AUDIT) ====================
            { "CategoryDropdownLabel", "all_category" },

            // ==================== NOTIFICATIONS (V43 AUDIT) ====================

            // ==================== TOURNAMENT LOBBY (V43 AUDIT) ====================

            // ==================== TOURNAMENTS BROWSER (V43 AUDIT) ====================
            { "LoadMoreText", "load_more" },

            { "ReducedMotionLabel", "setting_reduced_motion" },

            // ==================== LEVEL UP PANEL ====================
            // "ContinueButtonText" already mapped at line 516 → "continue_button"
            { "RewardUnlockedLabel", "reward_unlocked_title" },
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
        /// Configura auto-sizing como red de seguridad para traducciones largas.
        /// NUNCA cambia la alineación - respeta lo que el UIBuilder configuró.
        /// </summary>
        private void CenterTextIfNeeded(TextMeshProUGUI tmp)
        {
            if (tmp == null) return;

            // Solo habilitar auto-sizing si no estaba ya configurado por el UIBuilder
            if (!tmp.enableAutoSizing)
            {
                float currentSize = tmp.fontSize;
                if (currentSize > 0)
                {
                    tmp.enableAutoSizing = true;
                    tmp.fontSizeMin = Mathf.Max(FontSizes.AutoMinBody, currentSize * 0.6f);
                    tmp.fontSizeMax = currentSize;
                }
            }

            // Overflow como protección - solo si está en modo predeterminado
            if (tmp.overflowMode == TextOverflowModes.Overflow)
            {
                tmp.overflowMode = TextOverflowModes.Ellipsis;
            }
        }

        /// <summary>
        /// Configura best-fit como red de seguridad para traducciones largas en Text legacy.
        /// NUNCA cambia la alineación - respeta lo que el UIBuilder configuró.
        /// </summary>
        private void CenterLegacyTextIfNeeded(Text text)
        {
            if (text == null) return;

            // Solo habilitar best-fit si no estaba ya configurado
            if (!text.resizeTextForBestFit)
            {
                text.resizeTextForBestFit = true;
                text.resizeTextMinSize = 8;
                text.resizeTextMaxSize = text.fontSize > 0 ? text.fontSize : 36;
            }

            // Overflow como protección
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
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
            // Only allow suffix stripping if cleaned name retains at least 80% of original length
            string cleanName = gameObjectName
                .Replace("Text", "")
                .Replace("Label", "")
                .Replace("Button", "")
                .Replace(" ", "")
                .Trim();

            if (!string.IsNullOrEmpty(cleanName)
                && cleanName.Length >= gameObjectName.Length * 0.8f
                && TextNameToKeyMap.TryGetValue(cleanName, out key))
            {
                Debug.LogWarning($"[AutoLocalizer] Fuzzy match: '{gameObjectName}' -> '{cleanName}' -> key '{key}'. Consider adding an exact mapping.");
                return true;
            }

            // Buscar con sufijo Text
            if (TextNameToKeyMap.TryGetValue(gameObjectName + "Text", out key))
            {
                Debug.LogWarning($"[AutoLocalizer] Fuzzy match: '{gameObjectName}' -> '{gameObjectName}Text' -> key '{key}'. Consider adding an exact mapping.");
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
