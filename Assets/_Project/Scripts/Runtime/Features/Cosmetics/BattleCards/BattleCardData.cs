using UnityEngine;

namespace DigitPark.Cosmetics
{
    /// <summary>
    /// Categoría de adquisición del BattleCard.
    /// </summary>
    public enum BattleCardCategory
    {
        Free,        // Tier 0 — gratis al crear cuenta
        DC,          // Tier 1 — DigitCoins (farmeable)
        DGStandard,  // Tier 2 — DigitGems (sin animaciones)
        DGPremium,   // Tier 3 — DigitGems alto (con DOTween)
        Earn         // Tier 4 — Solo via achievement legendario
    }

    /// <summary>
    /// Tipo de animación del borde del BattleCard.
    /// </summary>
    public enum BorderAnimationType
    {
        None,
        Pulse,       // Alpha oscila 0.4→1.0 en loop
        Electric,    // Color rápido rojo↔naranja
        DataStream,  // Color HSV cycle lento
        Flame        // Color rápido con pulso agresivo
    }

    /// <summary>
    /// Datos de un BattleCard — cosmético de identidad visual en matchmaking.
    /// Solo afecta el card de matchmaking, NO cambia temas ni frames.
    /// </summary>
    [CreateAssetMenu(fileName = "NewBattleCard", menuName = "DigitPark/BattleCard", order = 10)]
    public class BattleCardData : ScriptableObject
    {
        // ── IDENTIDAD ──
        public string cardId;
        public string displayNameKey;      // clave de localización
        public BattleCardCategory category;

        // ── PRECIO ──
        public int dcPrice;
        public int dgPrice;
        public string earnAchievementId;   // null si no es Earn

        // ── FONDO DEL CARD ──
        public Color cardBackgroundColor = new Color(0.063f, 0.078f, 0.157f); // #101428
        public float patternOpacity;       // 0–1

        // ── ACCENT COLOR ──
        public Color accentPrimary = new Color(0f, 0.898f, 1f);  // #00E5FF
        public Color accentSecondary;
        public bool useGradientAccent;

        // ── BORDE ──
        [Range(1f, 6f)]
        public float borderWidth = 2f;
        public bool hasBorderGlow;
        [Range(0f, 1f)]
        public float borderGlowIntensity = 0.4f;
        public bool isAnimatedBorder;
        public BorderAnimationType borderAnimation = BorderAnimationType.None;

        // ── AVATAR FRAME COLOR ──
        public Color avatarFrameColor = new Color(0f, 0.898f, 1f);
        public bool isAnimatedAvatarFrame;

        // ── TIPOGRAFÍA ──
        public Color nameTextColor = Color.white;
        public bool nameHasGlow;
        [Range(0f, 1f)]
        public float nameGlowIntensity;
        public Color nameGlowColor = Color.clear;

        // ── SHOP ──
        public string descriptionLocKey;
    }
}
