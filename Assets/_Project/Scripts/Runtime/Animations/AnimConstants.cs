using DG.Tweening;

namespace DigitPark.Animations
{
    /// <summary>
    /// Constantes centralizadas para animaciones DOTween.
    /// Duraciones base están en UIAnimations (INSTANT/FAST/NORMAL/SLOW/VERY_SLOW).
    /// </summary>
    public static class AnimConstants
    {
        // ==================== DURACIONES ADICIONALES ====================
        public const float DURATION_QUICK = 0.15f;
        public const float DURATION_MEDIUM = 0.25f;
        public const float DURATION_ENTER = 0.35f;

        // ==================== EASING ESTÁNDAR ====================
        public const Ease ENTER = Ease.OutBack;
        public const Ease EXIT = Ease.InCubic;
        public const Ease EMPHASIS = Ease.OutElastic;
        public const Ease FADE = Ease.Linear;
        public const Ease SMOOTH = Ease.OutCubic;
        public const Ease BOUNCE = Ease.OutBounce;

        // ==================== DURACIONES COUNTDOWN ====================
        public const float COUNTDOWN_FADE_IN = 0.2f;
        public const float COUNTDOWN_NUMBER_IN = 0.35f;
        public const float COUNTDOWN_NUMBER_HOLD = 0.4f;
        public const float COUNTDOWN_NUMBER_OUT = 0.25f;
        public const float COUNTDOWN_GO_POP = 0.2f;
        public const float COUNTDOWN_SHOCKWAVE = 0.5f;
        public const float COUNTDOWN_OVERLAY_ALPHA = 1.0f;

        // ==================== DURACIONES TOAST ====================
        public const float TOAST_SLIDE_IN = 0.4f;
        public const float TOAST_SLIDE_OUT = 0.25f;
        public const float TOAST_DISPLAY = 5f;

        // ==================== ESCALAS ====================
        public const float SCALE_START = 0.85f;
        public const float SCALE_PRESS = 0.92f;
        public const float SCALE_PULSE = 1.08f;
        public const float SCALE_COUNTDOWN_START = 2.5f;
        public const float SCALE_GO_POP = 1.3f;

        // ==================== EASING ADICIONAL ====================
        public const Ease QUICK = Ease.OutQuad;
        public const Ease EXIT_QUICK = Ease.InQuad;
        public const Ease BREATHE = Ease.InOutSine;

        // ==================== DURACIONES UI ====================
        public const float DURATION_BUTTON = 0.08f;
        public const float DURATION_HOVER = 0.12f;
        public const float DURATION_COLOR_FLASH = 0.1f;
        public const float DURATION_FLY = 0.4f;
        public const float DURATION_BG_FADE = 0.5f;
        public const float DURATION_BREATHE = 2f;
        public const float DURATION_GLOW_PULSE = 1.5f;

        // ==================== OFFSETS ====================
        public const float SLIDE_OFFSET = 80f;
        public const float TOAST_HIDDEN_OFFSET = 250f;
    }
}
