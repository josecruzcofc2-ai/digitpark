using UnityEngine;

namespace DigitPark.Themes
{
    /// <summary>
    /// ScriptableObject que define un tema visual completo para la app
    /// Cada tema contiene todos los colores, estilos y configuraciones visuales
    /// </summary>
    [CreateAssetMenu(fileName = "NewTheme", menuName = "DigitPark/Theme Data", order = 1)]
    public class ThemeData : ScriptableObject
    {
        [Header("Theme Info")]
        public string themeId = "default";
        public string themeName = "Default Theme";
        public string themeDescription = "Default theme description";
        public Sprite themeIcon;
        public bool isPremium = false;

        [Header("=== BACKGROUNDS ===")]
        [Tooltip("Color principal del fondo de la app")]
        public Color primaryBackground = new Color(0.05f, 0.05f, 0.1f, 1f);

        [Tooltip("Color secundario para paneles y cards")]
        public Color secondaryBackground = new Color(0.1f, 0.1f, 0.15f, 1f);

        [Tooltip("Color terciario para elementos elevados")]
        public Color tertiaryBackground = new Color(0.15f, 0.15f, 0.2f, 1f);

        [Tooltip("Color del overlay/blocker semi-transparente")]
        public Color overlayColor = new Color(0f, 0f, 0f, 0.85f);

        [Header("=== PRIMARY COLORS ===")]
        [Tooltip("Color de acento principal (botones principales, highlights)")]
        public Color primaryAccent = new Color(0f, 1f, 1f, 1f); // Cyan

        [Tooltip("Color de acento secundario (elementos secundarios)")]
        public Color secondaryAccent = new Color(1f, 0f, 0.5f, 1f); // Magenta

        [Tooltip("Color de acento terciario (detalles)")]
        public Color tertiaryAccent = new Color(1f, 0.84f, 0f, 1f); // Gold

        [Header("=== TEXT COLORS ===")]
        [Tooltip("Color del texto principal")]
        public Color textPrimary = Color.white;

        [Tooltip("Color del texto secundario (subtítulos, hints)")]
        public Color textSecondary = new Color(0.7f, 0.7f, 0.7f, 1f);

        [Tooltip("Color del texto deshabilitado")]
        public Color textDisabled = new Color(0.4f, 0.4f, 0.4f, 1f);

        [Tooltip("Color del texto en botones primarios")]
        public Color textOnPrimary = Color.black;

        [Tooltip("Color de títulos especiales")]
        public Color textTitle = new Color(0f, 1f, 1f, 1f);

        [Header("=== BUTTON COLORS ===")]
        [Tooltip("Color de botones primarios (CTA)")]
        public Color buttonPrimary = new Color(0f, 1f, 1f, 1f);

        [Tooltip("Color de botones primarios al pasar mouse")]
        public Color buttonPrimaryHover = new Color(0.3f, 1f, 1f, 1f);

        [Tooltip("Color de botones primarios presionados")]
        public Color buttonPrimaryPressed = new Color(0f, 0.7f, 0.7f, 1f);

        [Tooltip("Color de botones secundarios")]
        public Color buttonSecondary = new Color(0.2f, 0.2f, 0.25f, 1f);

        [Tooltip("Color de botones secundarios al pasar mouse")]
        public Color buttonSecondaryHover = new Color(0.3f, 0.3f, 0.35f, 1f);

        [Tooltip("Color de botones de peligro (eliminar, etc)")]
        public Color buttonDanger = new Color(1f, 0.3f, 0.3f, 1f);

        [Tooltip("Color del texto en botones de peligro")]
        public Color textOnDanger = Color.white;

        [Tooltip("Color de botones de éxito")]
        public Color buttonSuccess = new Color(0.3f, 1f, 0.3f, 1f);

        [Tooltip("Color del texto en botones de éxito")]
        public Color textOnSuccess = Color.black;

        [Header("=== INPUT FIELDS ===")]
        [Tooltip("Color de fondo de inputs")]
        public Color inputBackground = new Color(0.1f, 0.1f, 0.15f, 1f);

        [Tooltip("Color del borde de inputs")]
        public Color inputBorder = new Color(0.3f, 0.3f, 0.35f, 1f);

        [Tooltip("Color del borde de inputs con foco")]
        public Color inputBorderFocused = new Color(0f, 1f, 1f, 1f);

        [Tooltip("Color del placeholder")]
        public Color inputPlaceholder = new Color(0.5f, 0.5f, 0.5f, 1f);

        [Header("=== SPECIAL ELEMENTS ===")]
        [Tooltip("Color de bordes neón/glow")]
        public Color glowColor = new Color(0f, 1f, 1f, 0.5f);

        [Tooltip("Color de elementos premium/gold")]
        public Color premiumColor = new Color(1f, 0.84f, 0f, 1f);

        [Tooltip("Color de errores")]
        public Color errorColor = new Color(1f, 0.3f, 0.3f, 1f);

        [Tooltip("Color de advertencias")]
        public Color warningColor = new Color(1f, 0.7f, 0.2f, 1f);

        [Tooltip("Color de éxito")]
        public Color successColor = new Color(0.3f, 1f, 0.3f, 1f);

        [Tooltip("Color de información")]
        public Color infoColor = new Color(0.3f, 0.7f, 1f, 1f);

        [Header("=== CARDS & PANELS ===")]
        [Tooltip("Color de fondo de cards")]
        public Color cardBackground = new Color(0.12f, 0.12f, 0.18f, 1f);

        [Tooltip("Color del borde de cards")]
        public Color cardBorder = new Color(0f, 1f, 1f, 0.3f);

        [Tooltip("Radio de esquinas de cards")]
        public float cardCornerRadius = 10f;

        [Header("=== SCENE SPECIFIC COLORS ===")]
        [Tooltip("Header morado para escenas especiales (OddOneOut)")]
        public Color headerPurple = new Color(0.2f, 0.05f, 0.25f, 1f);

        [Tooltip("Header azul marino para escenas especiales (QuickMath)")]
        public Color headerNavy = new Color(0.02f, 0.05f, 0.15f, 1f);

        [Tooltip("Fondo azul marino neon (OddOneOut)")]
        public Color backgroundNavy = new Color(0.02f, 0.03f, 0.12f, 1f);

        [Tooltip("Fondo morado neon (QuickMath)")]
        public Color backgroundPurple = new Color(0.15f, 0.02f, 0.18f, 1f);

        [Header("=== TABS & NAVIGATION ===")]
        [Tooltip("Color de tab activo")]
        public Color tabActive = new Color(0f, 1f, 1f, 1f);

        [Tooltip("Color de tab inactivo")]
        public Color tabInactive = new Color(0.3f, 0.3f, 0.35f, 1f);

        [Tooltip("Color de texto en tab activo")]
        public Color tabTextActive = Color.black;

        [Tooltip("Color de texto en tab inactivo")]
        public Color tabTextInactive = new Color(0.6f, 0.6f, 0.6f, 1f);

        [Header("=== SCROLLBAR ===")]
        [Tooltip("Color del track del scrollbar")]
        public Color scrollbarTrack = new Color(0.1f, 0.1f, 0.15f, 1f);

        [Tooltip("Color del handle del scrollbar")]
        public Color scrollbarHandle = new Color(0f, 1f, 1f, 0.5f);

        [Header("=== TOGGLE & CHECKBOX ===")]
        [Tooltip("Color del toggle cuando está ON")]
        public Color toggleOn = new Color(0f, 1f, 1f, 1f);

        [Tooltip("Color del toggle cuando está OFF")]
        public Color toggleOff = new Color(0.3f, 0.3f, 0.35f, 1f);

        [Tooltip("Color del checkmark")]
        public Color toggleCheckmark = Color.black;

        [Header("=== SLIDER ===")]
        [Tooltip("Color del track del slider")]
        public Color sliderTrack = new Color(0.2f, 0.2f, 0.25f, 1f);

        [Tooltip("Color del fill del slider")]
        public Color sliderFill = new Color(0f, 1f, 1f, 1f);

        [Tooltip("Color del handle del slider")]
        public Color sliderHandle = Color.white;

        [Header("=== LEADERBOARD ===")]
        [Tooltip("Color del primer lugar")]
        public Color rank1Color = new Color(1f, 0.84f, 0f, 1f); // Gold

        [Tooltip("Color del segundo lugar")]
        public Color rank2Color = new Color(0.75f, 0.75f, 0.75f, 1f); // Silver

        [Tooltip("Color del tercer lugar")]
        public Color rank3Color = new Color(0.8f, 0.5f, 0.2f, 1f); // Bronze

        [Tooltip("Color de filas pares")]
        public Color rowEven = new Color(0.1f, 0.1f, 0.15f, 0.5f);

        [Tooltip("Color de filas impares")]
        public Color rowOdd = new Color(0.15f, 0.15f, 0.2f, 0.5f);

        [Header("=== EFFECTS ===")]
        [Tooltip("Intensidad del efecto glow (0-1)")]
        [Range(0f, 1f)]
        public float glowIntensity = 0.5f;

        [Tooltip("Usar sombras en elementos")]
        public bool useShadows = true;

        [Tooltip("Color de sombras")]
        public Color shadowColor = new Color(0f, 0f, 0f, 0.5f);

        [Tooltip("Distancia de sombras")]
        public Vector2 shadowDistance = new Vector2(2f, -2f);

        [Header("=== ANIMATIONS ===")]
        [Tooltip("Duración de transiciones de color")]
        public float colorTransitionDuration = 0.2f;

        [Tooltip("Usar animaciones de hover")]
        public bool useHoverAnimations = true;

        /// <summary>
        /// Obtiene el color de ranking según la posición
        /// </summary>
        public Color GetRankColor(int position)
        {
            switch (position)
            {
                case 1: return rank1Color;
                case 2: return rank2Color;
                case 3: return rank3Color;
                default: return textPrimary;
            }
        }

        /// <summary>
        /// Obtiene el color de fila alternado
        /// </summary>
        public Color GetRowColor(int index)
        {
            return index % 2 == 0 ? rowEven : rowOdd;
        }

        /// <summary>
        /// Crea una copia del tema
        /// </summary>
        public ThemeData Clone()
        {
            return Instantiate(this);
        }
    }
}
