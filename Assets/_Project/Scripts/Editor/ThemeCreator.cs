#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using DigitPark.Themes;
using System.IO;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para crear los temas predefinidos de la aplicación
    /// Ejecutar desde: DigitPark > Create All Themes
    /// </summary>
    public class ThemeCreator : EditorWindow
    {
        private const string THEMES_PATH = "Assets/_Project/Resources/Themes";

        [MenuItem("DigitPark/Create All Themes")]
        public static void CreateAllThemes()
        {
            // Asegurar que exista la carpeta
            if (!Directory.Exists(THEMES_PATH))
            {
                Directory.CreateDirectory(THEMES_PATH);
                AssetDatabase.Refresh();
            }

            // Crear todos los temas
            CreateNeonDarkTheme();
            CreateCleanLightTheme();
            CreateRetroArcadeTheme();
            CreateOceanTheme();
            CreateVolcanoTheme();
            CreateCyberpunkTheme();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[ThemeCreator] Todos los temas han sido creados en: " + THEMES_PATH);
            EditorUtility.DisplayDialog("Temas Creados",
                "Se han creado 6 temas en:\n" + THEMES_PATH, "OK");
        }

        [MenuItem("DigitPark/Themes/Create Neon Dark")]
        public static void CreateNeonDarkTheme()
        {
            ThemeData theme = ScriptableObject.CreateInstance<ThemeData>();

            // Info
            theme.themeId = "neon_dark";
            theme.themeName = "Neon Dark";
            theme.themeDescription = "Dark theme with neon accents - The original DigitPark style";
            theme.isPremium = false;

            // Backgrounds
            theme.primaryBackground = HexToColor("#0A0A14");
            theme.secondaryBackground = HexToColor("#12121C");
            theme.tertiaryBackground = HexToColor("#1A1A28");
            theme.overlayColor = new Color(0, 0, 0, 0.9f);

            // Primary Colors
            theme.primaryAccent = HexToColor("#00FFFF");    // Cyan
            theme.secondaryAccent = HexToColor("#FF0080");  // Magenta
            theme.tertiaryAccent = HexToColor("#FFD700");   // Gold

            // Text
            theme.textPrimary = Color.white;
            theme.textSecondary = HexToColor("#B0B0B0");
            theme.textDisabled = HexToColor("#606060");
            theme.textOnPrimary = HexToColor("#0A0A14");
            theme.textTitle = HexToColor("#00FFFF");

            // Buttons
            theme.buttonPrimary = HexToColor("#00FFFF");
            theme.buttonPrimaryHover = HexToColor("#40FFFF");
            theme.buttonPrimaryPressed = HexToColor("#00CCCC");
            theme.buttonSecondary = HexToColor("#2A2A3A");
            theme.buttonSecondaryHover = HexToColor("#3A3A4A");
            theme.buttonDanger = HexToColor("#FF4444");
            theme.buttonSuccess = HexToColor("#44FF44");

            // Input
            theme.inputBackground = HexToColor("#12121C");
            theme.inputBorder = HexToColor("#3A3A4A");
            theme.inputBorderFocused = HexToColor("#00FFFF");
            theme.inputPlaceholder = HexToColor("#606060");

            // Special
            theme.glowColor = new Color(0, 1, 1, 0.5f);
            theme.premiumColor = HexToColor("#FFD700");
            theme.errorColor = HexToColor("#FF4444");
            theme.warningColor = HexToColor("#FFAA00");
            theme.successColor = HexToColor("#44FF44");
            theme.infoColor = HexToColor("#4488FF");

            // Cards
            theme.cardBackground = HexToColor("#16162A");
            theme.cardBorder = new Color(0, 1, 1, 0.3f);

            // Tabs
            theme.tabActive = HexToColor("#00FFFF");
            theme.tabInactive = HexToColor("#2A2A3A");
            theme.tabTextActive = HexToColor("#0A0A14");
            theme.tabTextInactive = HexToColor("#808080");

            // Leaderboard
            theme.rank1Color = HexToColor("#FFD700");
            theme.rank2Color = HexToColor("#C0C0C0");
            theme.rank3Color = HexToColor("#CD7F32");
            theme.rowEven = new Color(0.1f, 0.1f, 0.15f, 0.5f);
            theme.rowOdd = new Color(0.15f, 0.15f, 0.2f, 0.5f);

            // Effects
            theme.glowIntensity = 0.5f;
            theme.useShadows = true;
            theme.shadowColor = new Color(0, 0, 0, 0.5f);

            SaveTheme(theme, "Theme_NeonDark");
        }

        [MenuItem("DigitPark/Themes/Create Clean Light")]
        public static void CreateCleanLightTheme()
        {
            ThemeData theme = ScriptableObject.CreateInstance<ThemeData>();

            // Info
            theme.themeId = "clean_light";
            theme.themeName = "Clean Light";
            theme.themeDescription = "Clean and professional light theme";
            theme.isPremium = false;

            // Backgrounds
            theme.primaryBackground = HexToColor("#F5F5F7");
            theme.secondaryBackground = HexToColor("#FFFFFF");
            theme.tertiaryBackground = HexToColor("#E8E8ED");
            theme.overlayColor = new Color(0, 0, 0, 0.5f);

            // Primary Colors
            theme.primaryAccent = HexToColor("#007AFF");    // Apple Blue
            theme.secondaryAccent = HexToColor("#5856D6");  // Purple
            theme.tertiaryAccent = HexToColor("#FF9500");   // Orange

            // Text
            theme.textPrimary = HexToColor("#1D1D1F");
            theme.textSecondary = HexToColor("#6E6E73");
            theme.textDisabled = HexToColor("#AEAEB2");
            theme.textOnPrimary = Color.white;
            theme.textTitle = HexToColor("#007AFF");

            // Buttons
            theme.buttonPrimary = HexToColor("#007AFF");
            theme.buttonPrimaryHover = HexToColor("#0A84FF");
            theme.buttonPrimaryPressed = HexToColor("#0066CC");
            theme.buttonSecondary = HexToColor("#E8E8ED");
            theme.buttonSecondaryHover = HexToColor("#D1D1D6");
            theme.buttonDanger = HexToColor("#FF3B30");
            theme.buttonSuccess = HexToColor("#34C759");

            // Input
            theme.inputBackground = Color.white;
            theme.inputBorder = HexToColor("#C6C6C8");
            theme.inputBorderFocused = HexToColor("#007AFF");
            theme.inputPlaceholder = HexToColor("#AEAEB2");

            // Special
            theme.glowColor = new Color(0, 0.48f, 1, 0.2f);
            theme.premiumColor = HexToColor("#FF9500");
            theme.errorColor = HexToColor("#FF3B30");
            theme.warningColor = HexToColor("#FF9500");
            theme.successColor = HexToColor("#34C759");
            theme.infoColor = HexToColor("#007AFF");

            // Cards
            theme.cardBackground = Color.white;
            theme.cardBorder = HexToColor("#E5E5EA");

            // Tabs
            theme.tabActive = HexToColor("#007AFF");
            theme.tabInactive = HexToColor("#E8E8ED");
            theme.tabTextActive = Color.white;
            theme.tabTextInactive = HexToColor("#6E6E73");

            // Leaderboard
            theme.rank1Color = HexToColor("#FFD700");
            theme.rank2Color = HexToColor("#A8A8A8");
            theme.rank3Color = HexToColor("#CD7F32");
            theme.rowEven = new Color(0.95f, 0.95f, 0.97f, 1f);
            theme.rowOdd = new Color(1f, 1f, 1f, 1f);

            // Effects
            theme.glowIntensity = 0.1f;
            theme.useShadows = true;
            theme.shadowColor = new Color(0, 0, 0, 0.1f);

            // Toggle
            theme.toggleOn = HexToColor("#34C759");
            theme.toggleOff = HexToColor("#E9E9EA");
            theme.toggleCheckmark = Color.white;

            // Slider
            theme.sliderTrack = HexToColor("#E9E9EA");
            theme.sliderFill = HexToColor("#007AFF");
            theme.sliderHandle = Color.white;

            SaveTheme(theme, "Theme_CleanLight");
        }

        [MenuItem("DigitPark/Themes/Create Retro Arcade")]
        public static void CreateRetroArcadeTheme()
        {
            ThemeData theme = ScriptableObject.CreateInstance<ThemeData>();

            // Info
            theme.themeId = "retro_arcade";
            theme.themeName = "Retro Arcade";
            theme.themeDescription = "Classic 80s arcade style with pixel aesthetics";
            theme.isPremium = false;

            // Backgrounds - Classic arcade cabinet colors
            theme.primaryBackground = HexToColor("#0D0208");  // Almost black with slight warmth
            theme.secondaryBackground = HexToColor("#1A0A14"); // Deep purple-black
            theme.tertiaryBackground = HexToColor("#2D1B2E"); // Purple tint
            theme.overlayColor = new Color(0.05f, 0.01f, 0.03f, 0.95f);

            // Primary Colors - Classic arcade palette
            theme.primaryAccent = HexToColor("#39FF14");    // Neon Green (classic arcade)
            theme.secondaryAccent = HexToColor("#FF073A");  // Neon Red
            theme.tertiaryAccent = HexToColor("#FFFF00");   // Pure Yellow

            // Text
            theme.textPrimary = HexToColor("#39FF14");      // Green text like old monitors
            theme.textSecondary = HexToColor("#00FF41");    // Lighter green
            theme.textDisabled = HexToColor("#0F5132");
            theme.textOnPrimary = HexToColor("#0D0208");
            theme.textTitle = HexToColor("#FFFF00");        // Yellow titles

            // Buttons
            theme.buttonPrimary = HexToColor("#FF073A");    // Red button
            theme.buttonPrimaryHover = HexToColor("#FF3366");
            theme.buttonPrimaryPressed = HexToColor("#CC0029");
            theme.buttonSecondary = HexToColor("#2D1B2E");
            theme.buttonSecondaryHover = HexToColor("#3D2B3E");
            theme.buttonDanger = HexToColor("#FF073A");
            theme.buttonSuccess = HexToColor("#39FF14");

            // Input
            theme.inputBackground = HexToColor("#0D0208");
            theme.inputBorder = HexToColor("#39FF14");
            theme.inputBorderFocused = HexToColor("#FFFF00");
            theme.inputPlaceholder = HexToColor("#0F5132");

            // Special
            theme.glowColor = new Color(0.22f, 1f, 0.08f, 0.6f);
            theme.premiumColor = HexToColor("#FFFF00");
            theme.errorColor = HexToColor("#FF073A");
            theme.warningColor = HexToColor("#FF6600");
            theme.successColor = HexToColor("#39FF14");
            theme.infoColor = HexToColor("#00BFFF");

            // Cards
            theme.cardBackground = HexToColor("#1A0A14");
            theme.cardBorder = new Color(0.22f, 1f, 0.08f, 0.5f);

            // Tabs
            theme.tabActive = HexToColor("#FF073A");
            theme.tabInactive = HexToColor("#2D1B2E");
            theme.tabTextActive = HexToColor("#FFFF00");
            theme.tabTextInactive = HexToColor("#39FF14");

            // Leaderboard - Arcade high scores
            theme.rank1Color = HexToColor("#FFFF00");       // Yellow for #1
            theme.rank2Color = HexToColor("#00FFFF");       // Cyan for #2
            theme.rank3Color = HexToColor("#FF6600");       // Orange for #3
            theme.rowEven = new Color(0.1f, 0.04f, 0.08f, 0.7f);
            theme.rowOdd = new Color(0.15f, 0.08f, 0.12f, 0.7f);

            // Effects
            theme.glowIntensity = 0.7f;
            theme.useShadows = true;
            theme.shadowColor = new Color(0.22f, 1f, 0.08f, 0.3f);
            theme.shadowDistance = new Vector2(3, -3);

            // Toggle
            theme.toggleOn = HexToColor("#39FF14");
            theme.toggleOff = HexToColor("#2D1B2E");
            theme.toggleCheckmark = HexToColor("#0D0208");

            // Slider
            theme.sliderTrack = HexToColor("#2D1B2E");
            theme.sliderFill = HexToColor("#39FF14");
            theme.sliderHandle = HexToColor("#FFFF00");

            // Scrollbar
            theme.scrollbarTrack = HexToColor("#1A0A14");
            theme.scrollbarHandle = HexToColor("#39FF14");

            SaveTheme(theme, "Theme_RetroArcade");
        }

        [MenuItem("DigitPark/Themes/Create Ocean")]
        public static void CreateOceanTheme()
        {
            ThemeData theme = ScriptableObject.CreateInstance<ThemeData>();

            // Info
            theme.themeId = "ocean";
            theme.themeName = "Ocean";
            theme.themeDescription = "Calm ocean blues and teals";
            theme.isPremium = false;

            // Backgrounds
            theme.primaryBackground = HexToColor("#0A1628");
            theme.secondaryBackground = HexToColor("#0F2137");
            theme.tertiaryBackground = HexToColor("#162D4A");
            theme.overlayColor = new Color(0.04f, 0.09f, 0.16f, 0.9f);

            // Primary Colors
            theme.primaryAccent = HexToColor("#00D9FF");    // Bright Cyan
            theme.secondaryAccent = HexToColor("#0099CC");  // Ocean Blue
            theme.tertiaryAccent = HexToColor("#00FFAA");   // Teal

            // Text
            theme.textPrimary = HexToColor("#E8F4F8");
            theme.textSecondary = HexToColor("#A0C4D0");
            theme.textDisabled = HexToColor("#4A6A78");
            theme.textOnPrimary = HexToColor("#0A1628");
            theme.textTitle = HexToColor("#00D9FF");

            // Buttons
            theme.buttonPrimary = HexToColor("#00D9FF");
            theme.buttonPrimaryHover = HexToColor("#40E8FF");
            theme.buttonPrimaryPressed = HexToColor("#00AACC");
            theme.buttonSecondary = HexToColor("#162D4A");
            theme.buttonSecondaryHover = HexToColor("#1E3D5A");
            theme.buttonDanger = HexToColor("#FF6B6B");
            theme.buttonSuccess = HexToColor("#00FFAA");

            // Input
            theme.inputBackground = HexToColor("#0F2137");
            theme.inputBorder = HexToColor("#2A4A5A");
            theme.inputBorderFocused = HexToColor("#00D9FF");
            theme.inputPlaceholder = HexToColor("#4A6A78");

            // Special
            theme.glowColor = new Color(0, 0.85f, 1, 0.4f);
            theme.premiumColor = HexToColor("#00FFAA");
            theme.errorColor = HexToColor("#FF6B6B");
            theme.warningColor = HexToColor("#FFB347");
            theme.successColor = HexToColor("#00FFAA");
            theme.infoColor = HexToColor("#00D9FF");

            // Cards
            theme.cardBackground = HexToColor("#0F2137");
            theme.cardBorder = new Color(0, 0.85f, 1, 0.3f);

            // Leaderboard
            theme.rank1Color = HexToColor("#FFD700");
            theme.rank2Color = HexToColor("#C0C0C0");
            theme.rank3Color = HexToColor("#CD7F32");

            SaveTheme(theme, "Theme_Ocean");
        }

        [MenuItem("DigitPark/Themes/Create Volcano")]
        public static void CreateVolcanoTheme()
        {
            ThemeData theme = ScriptableObject.CreateInstance<ThemeData>();

            // Info
            theme.themeId = "volcano";
            theme.themeName = "Volcano";
            theme.themeDescription = "Fiery reds and oranges";
            theme.isPremium = false;

            // Backgrounds
            theme.primaryBackground = HexToColor("#1A0A0A");
            theme.secondaryBackground = HexToColor("#2D1212");
            theme.tertiaryBackground = HexToColor("#3D1A1A");
            theme.overlayColor = new Color(0.1f, 0.04f, 0.04f, 0.9f);

            // Primary Colors
            theme.primaryAccent = HexToColor("#FF4500");    // Orange Red
            theme.secondaryAccent = HexToColor("#FF6B35");  // Coral
            theme.tertiaryAccent = HexToColor("#FFD700");   // Gold

            // Text
            theme.textPrimary = HexToColor("#FFF0E6");
            theme.textSecondary = HexToColor("#D0A090");
            theme.textDisabled = HexToColor("#6A4A4A");
            theme.textOnPrimary = HexToColor("#1A0A0A");
            theme.textTitle = HexToColor("#FF4500");

            // Buttons
            theme.buttonPrimary = HexToColor("#FF4500");
            theme.buttonPrimaryHover = HexToColor("#FF6633");
            theme.buttonPrimaryPressed = HexToColor("#CC3700");
            theme.buttonSecondary = HexToColor("#3D1A1A");
            theme.buttonSecondaryHover = HexToColor("#4D2A2A");
            theme.buttonDanger = HexToColor("#FF0000");
            theme.buttonSuccess = HexToColor("#FFD700");

            // Input
            theme.inputBackground = HexToColor("#2D1212");
            theme.inputBorder = HexToColor("#5A3030");
            theme.inputBorderFocused = HexToColor("#FF4500");
            theme.inputPlaceholder = HexToColor("#6A4A4A");

            // Special
            theme.glowColor = new Color(1, 0.27f, 0, 0.5f);
            theme.premiumColor = HexToColor("#FFD700");
            theme.errorColor = HexToColor("#FF0000");
            theme.warningColor = HexToColor("#FF6B35");
            theme.successColor = HexToColor("#FFD700");
            theme.infoColor = HexToColor("#FF8C00");

            // Cards
            theme.cardBackground = HexToColor("#2D1212");
            theme.cardBorder = new Color(1, 0.27f, 0, 0.3f);

            // Leaderboard
            theme.rank1Color = HexToColor("#FFD700");
            theme.rank2Color = HexToColor("#FF6B35");
            theme.rank3Color = HexToColor("#FF4500");

            SaveTheme(theme, "Theme_Volcano");
        }

        [MenuItem("DigitPark/Themes/Create Cyberpunk")]
        public static void CreateCyberpunkTheme()
        {
            ThemeData theme = ScriptableObject.CreateInstance<ThemeData>();

            // Info
            theme.themeId = "cyberpunk";
            theme.themeName = "Cyberpunk";
            theme.themeDescription = "Futuristic neon with pink and blue";
            theme.isPremium = false;

            // Backgrounds
            theme.primaryBackground = HexToColor("#0D0221");
            theme.secondaryBackground = HexToColor("#150734");
            theme.tertiaryBackground = HexToColor("#1E0C45");
            theme.overlayColor = new Color(0.05f, 0.01f, 0.13f, 0.9f);

            // Primary Colors
            theme.primaryAccent = HexToColor("#FF00FF");    // Magenta
            theme.secondaryAccent = HexToColor("#00FFFF");  // Cyan
            theme.tertiaryAccent = HexToColor("#FF6EC7");   // Pink

            // Text
            theme.textPrimary = HexToColor("#F0E6FF");
            theme.textSecondary = HexToColor("#B8A8D0");
            theme.textDisabled = HexToColor("#5A4A6A");
            theme.textOnPrimary = HexToColor("#0D0221");
            theme.textTitle = HexToColor("#FF00FF");

            // Buttons
            theme.buttonPrimary = HexToColor("#FF00FF");
            theme.buttonPrimaryHover = HexToColor("#FF40FF");
            theme.buttonPrimaryPressed = HexToColor("#CC00CC");
            theme.buttonSecondary = HexToColor("#1E0C45");
            theme.buttonSecondaryHover = HexToColor("#2E1C55");
            theme.buttonDanger = HexToColor("#FF0066");
            theme.buttonSuccess = HexToColor("#00FF88");

            // Input
            theme.inputBackground = HexToColor("#150734");
            theme.inputBorder = HexToColor("#4A2A6A");
            theme.inputBorderFocused = HexToColor("#FF00FF");
            theme.inputPlaceholder = HexToColor("#5A4A6A");

            // Special
            theme.glowColor = new Color(1, 0, 1, 0.5f);
            theme.premiumColor = HexToColor("#FFD700");
            theme.errorColor = HexToColor("#FF0066");
            theme.warningColor = HexToColor("#FF6EC7");
            theme.successColor = HexToColor("#00FF88");
            theme.infoColor = HexToColor("#00FFFF");

            // Cards
            theme.cardBackground = HexToColor("#150734");
            theme.cardBorder = new Color(1, 0, 1, 0.3f);

            // Leaderboard
            theme.rank1Color = HexToColor("#FFD700");
            theme.rank2Color = HexToColor("#FF00FF");
            theme.rank3Color = HexToColor("#00FFFF");

            // Toggle
            theme.toggleOn = HexToColor("#FF00FF");
            theme.toggleOff = HexToColor("#1E0C45");

            // Slider
            theme.sliderFill = HexToColor("#FF00FF");

            SaveTheme(theme, "Theme_Cyberpunk");
        }

        /// <summary>
        /// Guarda el tema como asset
        /// </summary>
        private static void SaveTheme(ThemeData theme, string fileName)
        {
            string path = $"{THEMES_PATH}/{fileName}.asset";

            // Verificar si ya existe
            ThemeData existing = AssetDatabase.LoadAssetAtPath<ThemeData>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(theme, existing);
                EditorUtility.SetDirty(existing);
                Debug.Log($"[ThemeCreator] Tema actualizado: {fileName}");
            }
            else
            {
                AssetDatabase.CreateAsset(theme, path);
                Debug.Log($"[ThemeCreator] Tema creado: {fileName}");
            }
        }

        /// <summary>
        /// Convierte color hexadecimal a Color
        /// </summary>
        private static Color HexToColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                return color;
            }
            return Color.white;
        }
    }
}
#endif
