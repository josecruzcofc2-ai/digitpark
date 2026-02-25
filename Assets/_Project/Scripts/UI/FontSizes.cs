namespace DigitPark.UI
{
    public static class FontSizes
    {
        // ── Standard Tiers (largest to smallest) ──
        public const float Logo          = 288f;
        public const float AppBranding   = 122f;
        public const float CardSymbol    = 90f;
        public const float Equation      = 80f;
        public const float SceneTitle    = 78f;
        public const float DisplayLarge  = 72f;
        public const float AuthTitle     = 64f;
        public const float DisplayMedium = 56f;
        public const float CardTitle     = 52f;
        public const float SectionHeader = 48f;
        public const float ValueLarge    = 44f;
        public const float ValueMedium   = 42f;
        public const float Penalty       = 42f;
        public const float LabelLarge    = 40f;
        public const float Button        = 36f;
        public const float BodyLarge     = 32f;
        public const float Body          = 30f;

        // ── Auto-Size Minimums (only for fontSizeMin in auto-sizing, never as fixed size) ──
        public const float AutoMinTitle   = 36f;
        public const float AutoMinValue   = 30f;
        public const float AutoMinBody    = 24f;
        public const float AutoMinCompact = 20f;
        public const float AutoMinTiny    = 20f;

        // ── Debug (not shipped to production) ──
        public const float DebugText = 14f;
    }
}
