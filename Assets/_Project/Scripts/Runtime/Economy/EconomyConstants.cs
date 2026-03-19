namespace DigitPark.Economy
{
    /// <summary>
    /// Centralised economy constants — DigitCoins (DC) rewards per game mode.
    /// Edit here to rebalance; all callers update automatically.
    ///
    /// Target (active player, 10 games, 6 wins):
    ///   Practice    ~270 DC/day  (6 × 45 + 4 × 30)
    ///   Ranked 1v1  ~185 DC/day  (6 × 15 + 4 × 5 + 1 FWOTD 50)
    ///   Tournaments  ~700 DC/day  (6 × 100 + 4 × 25)
    /// </summary>
    public static class EconomyConstants
    {
        // ── Practice ──────────────────────────────────────────────────
        public const int COINS_PRACTICE_BASE        = 30;  // Complete any practice game
        public const int COINS_PRACTICE_PB_BONUS    = 15;  // Beat personal best

        // ── SingleGame (free 1v1) ─────────────────────────────────────
        public const int COINS_SINGLEGAME_WIN        = 50;
        public const int COINS_SINGLEGAME_LOSS       = 15;

        // ── Ranked 1v1 (Online mode) ──────────────────────────────────
        public const int COINS_RANKED_WIN            = 15;
        public const int COINS_RANKED_LOSS           = 5;
        public const int COINS_RANKED_PERFECT_BONUS  = 25;  // Win + zero errors
        public const int COINS_RANKED_FWOTD_BONUS    = 50;  // First Win of the Day

        // ── Tournament ────────────────────────────────────────────────
        public const int COINS_TOURNAMENT_WIN        = 100;
        public const int COINS_TOURNAMENT_LOSS       = 25;

        // ── Cognitive Sprint ──────────────────────────────────────────
        public const int COINS_SPRINT_WIN            = 60;
        public const int COINS_SPRINT_LOSS           = 15;
    }
}
