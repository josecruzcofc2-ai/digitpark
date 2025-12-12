using UnityEngine;

namespace DigitPark.UI.Theme
{
    /// <summary>
    /// ScriptableObject que contiene todos los colores del tema Neon Gamer
    /// </summary>
    [CreateAssetMenu(fileName = "NeonTheme", menuName = "DigitPark/Theme/Neon Theme Colors")]
    public class NeonThemeColors : ScriptableObject
    {
        [Header("=== FONDOS (Oscuros) ===")]
        [Tooltip("BG Deep Space - Fondo principal más oscuro")]
        public Color bgDeepSpace = new Color(0.0196f, 0.0314f, 0.0784f, 1f); // #050814

        [Tooltip("BG Secondary - Fondo secundario")]
        public Color bgSecondary = new Color(0.0314f, 0.0471f, 0.1098f, 1f); // #080C1C

        [Tooltip("BG Card - Fondo de tarjetas y paneles")]
        public Color bgCard = new Color(0.0627f, 0.0824f, 0.1569f, 1f); // #101528

        [Tooltip("BG Spotlight - Para degradados centrales")]
        public Color bgSpotlight = new Color(0.0784f, 0.0941f, 0.2f, 1f); // #141833

        [Header("=== COLORES NEÓN PRINCIPALES ===")]
        [Tooltip("Neon Cyan - Color principal de acción")]
        public Color neonCyan = new Color(0f, 0.9608f, 1f, 1f); // #00F5FF

        [Tooltip("Neon Purple - Color secundario")]
        public Color neonPurple = new Color(0.6157f, 0.2941f, 1f, 1f); // #9D4BFF

        [Tooltip("Neon Green - Éxito/Correcto")]
        public Color neonGreen = new Color(0.2353f, 1f, 0.4196f, 1f); // #3CFF6B

        [Tooltip("Neon Pink - Destacados/Torneos")]
        public Color neonPink = new Color(1f, 0.1804f, 0.6235f, 1f); // #FF2E9F

        [Tooltip("Neon Yellow - Advertencia/Atención/Oro")]
        public Color neonYellow = new Color(1f, 0.7882f, 0.2784f, 1f); // #FFC947

        [Header("=== TEXTO ===")]
        [Tooltip("Texto principal - Blanco")]
        public Color textPrimary = new Color(1f, 1f, 1f, 1f); // #FFFFFF

        [Tooltip("Texto secundario - Azul claro")]
        public Color textSecondary = new Color(0.7686f, 0.8f, 1f, 1f); // #C4CCFF

        [Tooltip("Texto desactivado/placeholder")]
        public Color textDisabled = new Color(0.4353f, 0.451f, 0.6f, 1f); // #6F7399

        [Header("=== ESTADOS ===")]
        [Tooltip("Error - Rojo neón")]
        public Color error = new Color(1f, 0.2f, 0.4f, 1f); // #FF3366

        [Tooltip("Borde normal de elementos")]
        public Color borderNormal = new Color(0.1255f, 0.1647f, 0.2902f, 1f); // #202A4A

        [Header("=== RANKINGS ===")]
        [Tooltip("Oro - 1er lugar")]
        public Color gold = new Color(1f, 0.7882f, 0.2784f, 1f); // #FFC947

        [Tooltip("Plata - 2do lugar")]
        public Color silver = new Color(0.7529f, 0.7529f, 0.7529f, 1f); // #C0C0C0

        [Tooltip("Bronce - 3er lugar")]
        public Color bronze = new Color(0.8039f, 0.498f, 0.1961f, 1f); // #CD7F32

        [Header("=== CONFIGURACIÓN DE GLOW ===")]
        [Range(0f, 1f)]
        public float glowAlpha = 0.35f;

        [Range(0f, 50f)]
        public float glowBlur = 20f;

        /// <summary>
        /// Obtiene el color con alpha modificado para glow
        /// </summary>
        public Color GetGlowColor(Color baseColor)
        {
            return new Color(baseColor.r, baseColor.g, baseColor.b, glowAlpha);
        }

        /// <summary>
        /// Crea un degradado entre dos colores
        /// </summary>
        public Gradient CreateGradient(Color color1, Color color2)
        {
            Gradient gradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[2];
            colorKeys[0].color = color1;
            colorKeys[0].time = 0f;
            colorKeys[1].color = color2;
            colorKeys[1].time = 1f;

            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0].alpha = 1f;
            alphaKeys[0].time = 0f;
            alphaKeys[1].alpha = 1f;
            alphaKeys[1].time = 1f;

            gradient.SetKeys(colorKeys, alphaKeys);
            return gradient;
        }
    }
}
