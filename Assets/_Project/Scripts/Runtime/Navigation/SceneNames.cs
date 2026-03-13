namespace DigitPark.Navigation
{
    /// <summary>
    /// Centralized scene name constants.
    /// Use these instead of raw string literals in SceneManager.LoadScene calls
    /// to prevent typo-induced silent failures and simplify renaming.
    /// </summary>
    public static class SceneNames
    {
        // Core
        public const string Boot        = "Boot";
        public const string MainMenu    = "MainMenu";
        public const string Settings    = "Settings";

        // Auth
        public const string Login       = "Login";
        public const string Register    = "Register";
        public const string AgeVerification = "AgeVerification";

        // Games
        public const string GameSelector   = "GameSelector";
        public const string DigitRush      = "DigitRush";
        public const string FlashTap       = "FlashTap";
        public const string MemoryPairs    = "MemoryPairs";
        public const string OddOneOut      = "OddOneOut";
        public const string QuickMath      = "QuickMath";
        public const string GameResults    = "GameResults";

        // Matchmaking & Online
        public const string Matchmaking    = "Matchmaking";
        public const string BetSelection   = "BetSelection";
        public const string PlayModeSelection = "PlayModeSelection";

        // Tournaments
        public const string Tournaments     = "Tournaments";
        public const string TournamentLobby = "TournamentLobby";

        // Social
        public const string Profile         = "Profile";
        public const string Friends         = "Friends";
        public const string SearchPlayers   = "SearchPlayers";
        public const string Leaderboard     = "Leaderboard";
        public const string Notifications   = "Notifications";

        // Monetization
        public const string Shop            = "Shop";

        // CashBattle
        public const string CashBattleHub      = "CashBattleHub";
        public const string CashHistory        = "CashHistory";
        public const string CashMatchmaking    = "CashMatchmaking";
        public const string CashTournament     = "CashTournament";
        public const string CashTournamentLobby = "CashTournamentLobby";
    }
}
