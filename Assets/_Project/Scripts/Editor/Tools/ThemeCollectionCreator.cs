#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using DigitPark.Themes;

namespace DigitPark.Editor
{
    /// <summary>
    /// Creates all 19 premium theme assets for DigitPark.
    /// Run from: DigitPark > Themes > Create All Theme Assets
    ///
    /// Theme catalog (sorted by exoticism, most exotic first):
    ///  1. Phantom         - Ultra-dark stealth, barely visible accents
    ///  2. Volcanic         - Lava bicolor red-orange on dark earth
    ///  3. Cyber Fuchsia    - Hot magenta cyberpunk nightclub
    ///  4. Toxic Lime       - Radioactive yellow-green
    ///  5. Infrared         - Neon rose-red night vision
    ///  6. Plasma Indigo    - Deep space blue-violet
    ///  7. Coral Surge      - Warm coral tropical energy
    ///  8. Nebula           - Cosmic violet starfield
    ///  9. Sakura           - Cherry blossom pink elegance
    /// 10. Electric Violet  - Purple neon streamer
    /// 11. Sunset           - Warm orange cozy glow
    /// 12. Matrix           - Hacker green terminal
    /// 13. Deep Ocean       - Teal underwater mystery
    /// 14. Emerald          - Forest teal-green calm
    /// 15. Crimson Blaze    - Aggressive red esports
    /// 16. Titanium         - Cool slate metallic industrial
    /// 17. Electric Blue    - Royal blue racing/sports
    /// 18. Arctic           - Ice cold sky blue
    /// 19. Monochrome       - Pure black and white minimal
    /// (Base: Neon Dark - cyan, not premium)
    ///
    /// PROHIBITED ZONE: No gold/amber/yellow themes (45-65 hue).
    /// Gold is reserved for CashBattle (real money) to avoid confusion.
    /// </summary>
    public class ThemeCollectionCreator
    {
        [MenuItem("DigitPark/Themes/Create All Theme Assets (19 Premium)")]
        public static void CreateAllThemes()
        {
            if (!EditorUtility.DisplayDialog("Crear Temas",
                "Esto creara 19 temas premium en Resources/Themes/.\n\n" +
                "Originales:\n" +
                "- Electric Violet, Crimson Blaze, Sakura, Emerald\n" +
                "- Sunset, Arctic, Monochrome, Deep Ocean\n\n" +
                "Nuevos:\n" +
                "- Toxic Lime, Matrix, Cyber Fuchsia, Plasma Indigo\n" +
                "- Electric Blue, Coral Surge, Infrared, Phantom\n" +
                "- Titanium, Nebula, Volcanic\n\n" +
                "Los temas existentes con el mismo nombre seran reemplazados.\n" +
                "Midnight Gold sera eliminado (conflicto con CashBattle gold).",
                "Crear", "Cancelar"))
                return;

            // Ensure folder exists
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Resources/Themes"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Project/Resources"))
                    AssetDatabase.CreateFolder("Assets/_Project", "Resources");
                AssetDatabase.CreateFolder("Assets/_Project/Resources", "Themes");
            }

            // Delete Midnight Gold if it exists
            string goldPath = "Assets/_Project/Resources/Themes/Theme_MidnightGold.asset";
            if (AssetDatabase.LoadAssetAtPath<ThemeData>(goldPath) != null)
            {
                AssetDatabase.DeleteAsset(goldPath);
                Debug.Log("[ThemeCreator] Eliminado: Midnight Gold (conflicto con CashBattle gold)");
            }

            int count = 0;
            // === Original 8 (sin Midnight Gold) ===
            count += CreateTheme(BuildElectricViolet()) ? 1 : 0;
            count += CreateTheme(BuildCrimsonBlaze()) ? 1 : 0;
            count += CreateTheme(BuildSakura()) ? 1 : 0;
            count += CreateTheme(BuildEmerald()) ? 1 : 0;
            count += CreateTheme(BuildSunset()) ? 1 : 0;
            count += CreateTheme(BuildArctic()) ? 1 : 0;
            count += CreateTheme(BuildMonochrome()) ? 1 : 0;
            count += CreateTheme(BuildDeepOcean()) ? 1 : 0;
            // === 11 Nuevos ===
            count += CreateTheme(BuildToxicLime()) ? 1 : 0;
            count += CreateTheme(BuildMatrix()) ? 1 : 0;
            count += CreateTheme(BuildCyberFuchsia()) ? 1 : 0;
            count += CreateTheme(BuildPlasmaIndigo()) ? 1 : 0;
            count += CreateTheme(BuildElectricBlue()) ? 1 : 0;
            count += CreateTheme(BuildCoralSurge()) ? 1 : 0;
            count += CreateTheme(BuildInfrared()) ? 1 : 0;
            count += CreateTheme(BuildPhantom()) ? 1 : 0;
            count += CreateTheme(BuildTitanium()) ? 1 : 0;
            count += CreateTheme(BuildNebula()) ? 1 : 0;
            count += CreateTheme(BuildVolcanic()) ? 1 : 0;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Completado",
                $"Se crearon {count} temas premium en:\nResources/Themes/\n\n(Midnight Gold eliminado)",
                "OK");
        }

        private static bool CreateTheme(ThemeData theme)
        {
            string path = $"Assets/_Project/Resources/Themes/Theme_{theme.themeId}.asset";

            if (AssetDatabase.LoadAssetAtPath<ThemeData>(path) != null)
                AssetDatabase.DeleteAsset(path);

            AssetDatabase.CreateAsset(theme, path);
            Debug.Log($"[ThemeCreator] Creado: {theme.themeName} -> {path}");
            return true;
        }

        // ============================================================
        // Helper: Hex to Color
        // ============================================================
        private static Color Hex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color c))
                return c;
            return Color.magenta;
        }

        private static Color Hex(string hex, float alpha)
        {
            Color c = Hex(hex);
            c.a = alpha;
            return c;
        }

        // ============================================================
        // Helper: Apply common fixed values (semantic colors)
        // ============================================================
        private static void ApplyFixedColors(ThemeData t)
        {
            // Status colors (NEVER change with theme)
            t.errorColor = Hex("#FF4D4D");
            t.warningColor = Hex("#FFB020");
            t.successColor = Hex("#4DFF7C");
            t.infoColor = Hex("#4DA6FF");

            // Premium color (always gold)
            t.premiumColor = Hex("#FFD700");

            // Rank colors (universal)
            t.rank1Color = Hex("#FFD700"); // Gold
            t.rank2Color = Hex("#C0C0C0"); // Silver
            t.rank3Color = Hex("#CD7F32"); // Bronze

            // Text on semantic buttons
            t.textOnDanger = Color.white;
            t.textOnSuccess = Color.black;

            // Button danger (always red)
            t.buttonDanger = Hex("#E53E3E");

            // Button success (always green)
            t.buttonSuccess = Hex("#38A169");

            // Card corner radius
            t.cardCornerRadius = 10f;

            // Shadows
            t.useShadows = true;
            t.shadowColor = new Color(0f, 0f, 0f, 0.5f);
            t.shadowDistance = new Vector2(2f, -2f);

            // Animations
            t.colorTransitionDuration = 0.25f;
            t.useHoverAnimations = true;

            // isPremium
            t.isPremium = true;
        }

        // ============================================================
        // Helper: Build a complete theme from accent color + background hue
        // Reduces boilerplate for the 11 new themes
        // ============================================================
        private static void ApplyAccentDerived(ThemeData t, string accent, string bgBase,
            string bgSecondary, string bgTertiary, string cardBg,
            string textSecondaryHex, string textDisabledHex,
            string textTitleHex, string textOnPrimaryHex,
            string btnSecondary, string btnSecondaryHover,
            string inputBorderHex, string inputPlaceholderHex,
            float glowAlpha, float glowIntensityVal)
        {
            // Backgrounds
            t.primaryBackground = Hex(bgBase);
            t.secondaryBackground = Hex(bgSecondary);
            t.tertiaryBackground = Hex(bgTertiary);
            t.overlayColor = Hex("#000000", 0.85f);

            // Text
            t.textPrimary = Color.white;
            t.textSecondary = Hex(textSecondaryHex);
            t.textDisabled = Hex(textDisabledHex);
            t.textTitle = Hex(textTitleHex);
            t.textOnPrimary = Hex(textOnPrimaryHex);

            // Input
            t.inputBackground = Hex(bgSecondary);
            t.inputBorder = Hex(inputBorderHex);
            t.inputBorderFocused = Hex(accent);
            t.inputPlaceholder = Hex(inputPlaceholderHex);

            // Glow
            t.glowColor = Hex(accent, glowAlpha);
            t.glowIntensity = glowIntensityVal;

            // Cards
            t.cardBackground = Hex(cardBg);
            t.cardBorder = Hex(accent, 0.3f);

            // Buttons secondary
            t.buttonSecondary = Hex(btnSecondary);
            t.buttonSecondaryHover = Hex(btnSecondaryHover);

            // Tabs
            t.tabActive = Hex(accent);
            t.tabInactive = Hex(btnSecondary);
            t.tabTextActive = Hex(textOnPrimaryHex);
            t.tabTextInactive = Hex(textDisabledHex);

            // Scrollbar
            t.scrollbarTrack = Hex(bgSecondary);
            t.scrollbarHandle = Hex(accent, 0.5f);

            // Toggle
            t.toggleOn = Hex(accent);
            t.toggleOff = Hex(btnSecondary);
            t.toggleCheckmark = Hex(textOnPrimaryHex);

            // Slider
            t.sliderTrack = Hex(btnSecondary);
            t.sliderFill = Hex(accent);
            t.sliderHandle = Color.white;

            // Leaderboard rows
            t.rowEven = Hex(bgSecondary, 0.5f);
            t.rowOdd = Hex(bgTertiary, 0.5f);
        }

        // ============================================================
        //  1. ELECTRIC VIOLET (#A855F7) - Purple neon streamer
        // ============================================================
        private static ThemeData BuildElectricViolet()
        {
            var t = ScriptableObject.CreateInstance<ThemeData>();
            t.themeId = "ElectricViolet";
            t.themeName = "Electric Violet";
            t.themeDescription = "Purple neon energy. Streamer aesthetic with electric violet glow.";
            ApplyFixedColors(t);

            t.primaryBackground = Hex("#0F0A1A");
            t.secondaryBackground = Hex("#160F24");
            t.tertiaryBackground = Hex("#1E152E");
            t.overlayColor = Hex("#000000", 0.85f);

            t.primaryAccent = Hex("#A855F7");
            t.secondaryAccent = Hex("#C084FC");
            t.tertiaryAccent = Hex("#E9D5FF");

            t.textPrimary = Color.white;
            t.textSecondary = Hex("#B8A8CC");
            t.textDisabled = Hex("#5A4D6B");
            t.textTitle = Hex("#C084FC");
            t.textOnPrimary = Color.white;

            t.buttonPrimary = Hex("#A855F7");
            t.buttonPrimaryHover = Hex("#B975F9");
            t.buttonPrimaryPressed = Hex("#7E3BD0");
            t.buttonSecondary = Hex("#2A1F3D");
            t.buttonSecondaryHover = Hex("#362A4D");

            t.inputBackground = Hex("#160F24");
            t.inputBorder = Hex("#3D2D5C");
            t.inputBorderFocused = Hex("#A855F7");
            t.inputPlaceholder = Hex("#5A4D6B");

            t.glowColor = Hex("#A855F7", 0.5f);
            t.glowIntensity = 0.5f;

            t.cardBackground = Hex("#1A1228");
            t.cardBorder = Hex("#A855F7", 0.3f);

            t.headerPurple = Hex("#2D1548");
            t.headerNavy = Hex("#140E22");
            t.backgroundNavy = Hex("#0F0A1A");
            t.backgroundPurple = Hex("#1A0E2E");

            t.tabActive = Hex("#A855F7");
            t.tabInactive = Hex("#2A1F3D");
            t.tabTextActive = Color.white;
            t.tabTextInactive = Hex("#7A6B96");

            t.scrollbarTrack = Hex("#160F24");
            t.scrollbarHandle = Hex("#A855F7", 0.5f);

            t.toggleOn = Hex("#A855F7");
            t.toggleOff = Hex("#2A1F3D");
            t.toggleCheckmark = Color.white;

            t.sliderTrack = Hex("#2A1F3D");
            t.sliderFill = Hex("#A855F7");
            t.sliderHandle = Color.white;

            t.rowEven = Hex("#160F24", 0.5f);
            t.rowOdd = Hex("#1E152E", 0.5f);

            return t;
        }

        // ============================================================
        //  2. CRIMSON BLAZE (#EF4444) - Aggressive red esports
        // ============================================================
        private static ThemeData BuildCrimsonBlaze()
        {
            var t = ScriptableObject.CreateInstance<ThemeData>();
            t.themeId = "CrimsonBlaze";
            t.themeName = "Crimson Blaze";
            t.themeDescription = "Aggressive red energy. Competitive esports intensity.";
            ApplyFixedColors(t);

            t.primaryBackground = Hex("#1A0A0A");
            t.secondaryBackground = Hex("#241010");
            t.tertiaryBackground = Hex("#2E1616");
            t.overlayColor = Hex("#000000", 0.85f);

            t.primaryAccent = Hex("#EF4444");
            t.secondaryAccent = Hex("#F97316");
            t.tertiaryAccent = Hex("#FCA5A5");

            t.textPrimary = Color.white;
            t.textSecondary = Hex("#CCA8A8");
            t.textDisabled = Hex("#6B4D4D");
            t.textTitle = Hex("#F87171");
            t.textOnPrimary = Color.white;

            t.buttonPrimary = Hex("#EF4444");
            t.buttonPrimaryHover = Hex("#F26666");
            t.buttonPrimaryPressed = Hex("#C03030");
            t.buttonSecondary = Hex("#3D1F1F");
            t.buttonSecondaryHover = Hex("#4D2A2A");

            t.inputBackground = Hex("#241010");
            t.inputBorder = Hex("#5C2D2D");
            t.inputBorderFocused = Hex("#EF4444");
            t.inputPlaceholder = Hex("#6B4D4D");

            t.glowColor = Hex("#EF4444", 0.5f);
            t.glowIntensity = 0.55f;

            t.cardBackground = Hex("#281212");
            t.cardBorder = Hex("#EF4444", 0.3f);

            t.headerPurple = Hex("#481520");
            t.headerNavy = Hex("#220E12");
            t.backgroundNavy = Hex("#1A0A0A");
            t.backgroundPurple = Hex("#2E0E16");

            t.tabActive = Hex("#EF4444");
            t.tabInactive = Hex("#3D1F1F");
            t.tabTextActive = Color.white;
            t.tabTextInactive = Hex("#966B6B");

            t.scrollbarTrack = Hex("#241010");
            t.scrollbarHandle = Hex("#EF4444", 0.5f);

            t.toggleOn = Hex("#EF4444");
            t.toggleOff = Hex("#3D1F1F");
            t.toggleCheckmark = Color.white;

            t.sliderTrack = Hex("#3D1F1F");
            t.sliderFill = Hex("#EF4444");
            t.sliderHandle = Color.white;

            t.rowEven = Hex("#241010", 0.5f);
            t.rowOdd = Hex("#2E1616", 0.5f);

            return t;
        }

        // ============================================================
        //  3. SAKURA (#F472B6) - Cherry blossom pink elegance
        // ============================================================
        private static ThemeData BuildSakura()
        {
            var t = ScriptableObject.CreateInstance<ThemeData>();
            t.themeId = "Sakura";
            t.themeName = "Sakura";
            t.themeDescription = "Cherry blossom elegance. Soft pink glow with Japanese aesthetic.";
            ApplyFixedColors(t);

            t.primaryBackground = Hex("#1A0F14");
            t.secondaryBackground = Hex("#24141C");
            t.tertiaryBackground = Hex("#2E1A24");
            t.overlayColor = Hex("#000000", 0.85f);

            t.primaryAccent = Hex("#F472B6");
            t.secondaryAccent = Hex("#EC4899");
            t.tertiaryAccent = Hex("#FBCFE8");

            t.textPrimary = Color.white;
            t.textSecondary = Hex("#CCA8B8");
            t.textDisabled = Hex("#6B4D5A");
            t.textTitle = Hex("#F9A8D4");
            t.textOnPrimary = Hex("#1A0F14");

            t.buttonPrimary = Hex("#F472B6");
            t.buttonPrimaryHover = Hex("#F78EC5");
            t.buttonPrimaryPressed = Hex("#D05A98");
            t.buttonSecondary = Hex("#3D1F2E");
            t.buttonSecondaryHover = Hex("#4D2A3A");

            t.inputBackground = Hex("#24141C");
            t.inputBorder = Hex("#5C2D44");
            t.inputBorderFocused = Hex("#F472B6");
            t.inputPlaceholder = Hex("#6B4D5A");

            t.glowColor = Hex("#F472B6", 0.45f);
            t.glowIntensity = 0.45f;

            t.cardBackground = Hex("#281620");
            t.cardBorder = Hex("#F472B6", 0.3f);

            t.headerPurple = Hex("#3D1530");
            t.headerNavy = Hex("#220E18");
            t.backgroundNavy = Hex("#1A0F14");
            t.backgroundPurple = Hex("#2E0E22");

            t.tabActive = Hex("#F472B6");
            t.tabInactive = Hex("#3D1F2E");
            t.tabTextActive = Hex("#1A0F14");
            t.tabTextInactive = Hex("#966B80");

            t.scrollbarTrack = Hex("#24141C");
            t.scrollbarHandle = Hex("#F472B6", 0.5f);

            t.toggleOn = Hex("#F472B6");
            t.toggleOff = Hex("#3D1F2E");
            t.toggleCheckmark = Hex("#1A0F14");

            t.sliderTrack = Hex("#3D1F2E");
            t.sliderFill = Hex("#F472B6");
            t.sliderHandle = Color.white;

            t.rowEven = Hex("#24141C", 0.5f);
            t.rowOdd = Hex("#2E1A24", 0.5f);

            return t;
        }

        // ============================================================
        //  4. EMERALD (#10B981) - Forest teal-green calm
        // ============================================================
        private static ThemeData BuildEmerald()
        {
            var t = ScriptableObject.CreateInstance<ThemeData>();
            t.themeId = "Emerald";
            t.themeName = "Emerald";
            t.themeDescription = "Forest green energy. Strategic calm with nature vibes.";
            ApplyFixedColors(t);

            t.primaryBackground = Hex("#0A1A14");
            t.secondaryBackground = Hex("#10241A");
            t.tertiaryBackground = Hex("#162E22");
            t.overlayColor = Hex("#000000", 0.85f);

            t.primaryAccent = Hex("#10B981");
            t.secondaryAccent = Hex("#34D399");
            t.tertiaryAccent = Hex("#A7F3D0");

            t.textPrimary = Color.white;
            t.textSecondary = Hex("#A8CCB8");
            t.textDisabled = Hex("#4D6B5A");
            t.textTitle = Hex("#34D399");
            t.textOnPrimary = Hex("#0A1A14");

            t.buttonPrimary = Hex("#10B981");
            t.buttonPrimaryHover = Hex("#30D098");
            t.buttonPrimaryPressed = Hex("#0A9466");
            t.buttonSecondary = Hex("#1F3D2E");
            t.buttonSecondaryHover = Hex("#2A4D3A");

            t.inputBackground = Hex("#10241A");
            t.inputBorder = Hex("#2D5C44");
            t.inputBorderFocused = Hex("#10B981");
            t.inputPlaceholder = Hex("#4D6B5A");

            t.glowColor = Hex("#10B981", 0.5f);
            t.glowIntensity = 0.5f;

            t.cardBackground = Hex("#122820");
            t.cardBorder = Hex("#10B981", 0.3f);

            t.headerPurple = Hex("#15482D");
            t.headerNavy = Hex("#0E221A");
            t.backgroundNavy = Hex("#0A1A14");
            t.backgroundPurple = Hex("#0E2E1E");

            t.tabActive = Hex("#10B981");
            t.tabInactive = Hex("#1F3D2E");
            t.tabTextActive = Hex("#0A1A14");
            t.tabTextInactive = Hex("#6B9680");

            t.scrollbarTrack = Hex("#10241A");
            t.scrollbarHandle = Hex("#10B981", 0.5f);

            t.toggleOn = Hex("#10B981");
            t.toggleOff = Hex("#1F3D2E");
            t.toggleCheckmark = Hex("#0A1A14");

            t.sliderTrack = Hex("#1F3D2E");
            t.sliderFill = Hex("#10B981");
            t.sliderHandle = Color.white;

            t.rowEven = Hex("#10241A", 0.5f);
            t.rowOdd = Hex("#162E22", 0.5f);

            return t;
        }

        // ============================================================
        //  5. SUNSET (#FB923C) - Warm orange cozy glow
        // ============================================================
        private static ThemeData BuildSunset()
        {
            var t = ScriptableObject.CreateInstance<ThemeData>();
            t.themeId = "Sunset";
            t.themeName = "Sunset";
            t.themeDescription = "Warm sunset energy. Orange tones with a cozy glow.";
            ApplyFixedColors(t);

            t.primaryBackground = Hex("#1A1008");
            t.secondaryBackground = Hex("#241810");
            t.tertiaryBackground = Hex("#2E2016");
            t.overlayColor = Hex("#000000", 0.85f);

            t.primaryAccent = Hex("#FB923C");
            t.secondaryAccent = Hex("#FBBF24");
            t.tertiaryAccent = Hex("#FDE68A");

            t.textPrimary = Color.white;
            t.textSecondary = Hex("#CCB8A8");
            t.textDisabled = Hex("#6B5A4D");
            t.textTitle = Hex("#FDBA74");
            t.textOnPrimary = Hex("#1A1008");

            t.buttonPrimary = Hex("#FB923C");
            t.buttonPrimaryHover = Hex("#FCA85C");
            t.buttonPrimaryPressed = Hex("#D07830");
            t.buttonSecondary = Hex("#3D2E1F");
            t.buttonSecondaryHover = Hex("#4D3A2A");

            t.inputBackground = Hex("#241810");
            t.inputBorder = Hex("#5C442D");
            t.inputBorderFocused = Hex("#FB923C");
            t.inputPlaceholder = Hex("#6B5A4D");

            t.glowColor = Hex("#FB923C", 0.45f);
            t.glowIntensity = 0.45f;

            t.cardBackground = Hex("#281C12");
            t.cardBorder = Hex("#FB923C", 0.3f);

            t.headerPurple = Hex("#48350F");
            t.headerNavy = Hex("#22180E");
            t.backgroundNavy = Hex("#1A1008");
            t.backgroundPurple = Hex("#2E1E0E");

            t.tabActive = Hex("#FB923C");
            t.tabInactive = Hex("#3D2E1F");
            t.tabTextActive = Hex("#1A1008");
            t.tabTextInactive = Hex("#96806B");

            t.scrollbarTrack = Hex("#241810");
            t.scrollbarHandle = Hex("#FB923C", 0.5f);

            t.toggleOn = Hex("#FB923C");
            t.toggleOff = Hex("#3D2E1F");
            t.toggleCheckmark = Hex("#1A1008");

            t.sliderTrack = Hex("#3D2E1F");
            t.sliderFill = Hex("#FB923C");
            t.sliderHandle = Color.white;

            t.rowEven = Hex("#241810", 0.5f);
            t.rowOdd = Hex("#2E2016", 0.5f);

            return t;
        }

        // ============================================================
        //  6. ARCTIC (#38BDF8) - Ice cold sky blue
        // ============================================================
        private static ThemeData BuildArctic()
        {
            var t = ScriptableObject.CreateInstance<ThemeData>();
            t.themeId = "Arctic";
            t.themeName = "Arctic";
            t.themeDescription = "Ice cold minimalism. Clean blue tones with crystalline clarity.";
            ApplyFixedColors(t);

            t.primaryBackground = Hex("#0C1929");
            t.secondaryBackground = Hex("#112236");
            t.tertiaryBackground = Hex("#162B42");
            t.overlayColor = Hex("#000000", 0.85f);

            t.primaryAccent = Hex("#38BDF8");
            t.secondaryAccent = Hex("#7DD3FC");
            t.tertiaryAccent = Hex("#E0F2FE");

            t.textPrimary = Color.white;
            t.textSecondary = Hex("#A8C4D6");
            t.textDisabled = Hex("#4D6478");
            t.textTitle = Hex("#7DD3FC");
            t.textOnPrimary = Hex("#0C1929");

            t.buttonPrimary = Hex("#38BDF8");
            t.buttonPrimaryHover = Hex("#5CCBFA");
            t.buttonPrimaryPressed = Hex("#2098CC");
            t.buttonSecondary = Hex("#1F3044");
            t.buttonSecondaryHover = Hex("#2A3D54");

            t.inputBackground = Hex("#112236");
            t.inputBorder = Hex("#2D4A66");
            t.inputBorderFocused = Hex("#38BDF8");
            t.inputPlaceholder = Hex("#4D6478");

            t.glowColor = Hex("#38BDF8", 0.45f);
            t.glowIntensity = 0.45f;

            t.cardBackground = Hex("#142638");
            t.cardBorder = Hex("#38BDF8", 0.3f);

            t.headerPurple = Hex("#153656");
            t.headerNavy = Hex("#0E1E32");
            t.backgroundNavy = Hex("#0C1929");
            t.backgroundPurple = Hex("#0E2440");

            t.tabActive = Hex("#38BDF8");
            t.tabInactive = Hex("#1F3044");
            t.tabTextActive = Hex("#0C1929");
            t.tabTextInactive = Hex("#6B8BA0");

            t.scrollbarTrack = Hex("#112236");
            t.scrollbarHandle = Hex("#38BDF8", 0.5f);

            t.toggleOn = Hex("#38BDF8");
            t.toggleOff = Hex("#1F3044");
            t.toggleCheckmark = Hex("#0C1929");

            t.sliderTrack = Hex("#1F3044");
            t.sliderFill = Hex("#38BDF8");
            t.sliderHandle = Color.white;

            t.rowEven = Hex("#112236", 0.5f);
            t.rowOdd = Hex("#162B42", 0.5f);

            return t;
        }

        // ============================================================
        //  7. MONOCHROME (#D1D5DB) - Pure black and white minimal
        // ============================================================
        private static ThemeData BuildMonochrome()
        {
            var t = ScriptableObject.CreateInstance<ThemeData>();
            t.themeId = "Monochrome";
            t.themeName = "Monochrome";
            t.themeDescription = "Pure black and white. Ultra-minimal stealth mode with zero color.";
            ApplyFixedColors(t);

            t.primaryBackground = Hex("#18181B");
            t.secondaryBackground = Hex("#1F1F23");
            t.tertiaryBackground = Hex("#27272A");
            t.overlayColor = Hex("#000000", 0.85f);

            t.primaryAccent = Hex("#D1D5DB");
            t.secondaryAccent = Hex("#F9FAFB");
            t.tertiaryAccent = Hex("#FFFFFF");

            t.textPrimary = Color.white;
            t.textSecondary = Hex("#A1A1AA");
            t.textDisabled = Hex("#52525B");
            t.textTitle = Hex("#F4F4F5");
            t.textOnPrimary = Hex("#18181B");

            t.buttonPrimary = Hex("#D1D5DB");
            t.buttonPrimaryHover = Hex("#E5E7EB");
            t.buttonPrimaryPressed = Hex("#9CA3AF");
            t.buttonSecondary = Hex("#2D2D32");
            t.buttonSecondaryHover = Hex("#3A3A40");

            t.inputBackground = Hex("#1F1F23");
            t.inputBorder = Hex("#3F3F46");
            t.inputBorderFocused = Hex("#D1D5DB");
            t.inputPlaceholder = Hex("#52525B");

            t.glowColor = Hex("#D1D5DB", 0.3f);
            t.glowIntensity = 0.3f;

            t.cardBackground = Hex("#232328");
            t.cardBorder = Hex("#D1D5DB", 0.2f);

            t.headerPurple = Hex("#2A2A30");
            t.headerNavy = Hex("#1C1C20");
            t.backgroundNavy = Hex("#18181B");
            t.backgroundPurple = Hex("#222228");

            t.tabActive = Hex("#D1D5DB");
            t.tabInactive = Hex("#2D2D32");
            t.tabTextActive = Hex("#18181B");
            t.tabTextInactive = Hex("#71717A");

            t.scrollbarTrack = Hex("#1F1F23");
            t.scrollbarHandle = Hex("#D1D5DB", 0.4f);

            t.toggleOn = Hex("#D1D5DB");
            t.toggleOff = Hex("#2D2D32");
            t.toggleCheckmark = Hex("#18181B");

            t.sliderTrack = Hex("#2D2D32");
            t.sliderFill = Hex("#D1D5DB");
            t.sliderHandle = Color.white;

            t.rowEven = Hex("#1F1F23", 0.5f);
            t.rowOdd = Hex("#27272A", 0.5f);

            return t;
        }

        // ============================================================
        //  8. DEEP OCEAN (#14B8A6) - Teal underwater mystery
        // ============================================================
        private static ThemeData BuildDeepOcean()
        {
            var t = ScriptableObject.CreateInstance<ThemeData>();
            t.themeId = "DeepOcean";
            t.themeName = "Deep Ocean";
            t.themeDescription = "Underwater mystery. Teal and turquoise tones from the abyss.";
            ApplyFixedColors(t);

            t.primaryBackground = Hex("#0A1520");
            t.secondaryBackground = Hex("#0F1E2C");
            t.tertiaryBackground = Hex("#142738");
            t.overlayColor = Hex("#000000", 0.85f);

            t.primaryAccent = Hex("#14B8A6");
            t.secondaryAccent = Hex("#2DD4BF");
            t.tertiaryAccent = Hex("#99F6E4");

            t.textPrimary = Color.white;
            t.textSecondary = Hex("#A8C8C2");
            t.textDisabled = Hex("#4D6B66");
            t.textTitle = Hex("#2DD4BF");
            t.textOnPrimary = Hex("#0A1520");

            t.buttonPrimary = Hex("#14B8A6");
            t.buttonPrimaryHover = Hex("#30D0BE");
            t.buttonPrimaryPressed = Hex("#0E9484");
            t.buttonSecondary = Hex("#1A3040");
            t.buttonSecondaryHover = Hex("#243D50");

            t.inputBackground = Hex("#0F1E2C");
            t.inputBorder = Hex("#2D5C54");
            t.inputBorderFocused = Hex("#14B8A6");
            t.inputPlaceholder = Hex("#4D6B66");

            t.glowColor = Hex("#14B8A6", 0.5f);
            t.glowIntensity = 0.5f;

            t.cardBackground = Hex("#122230");
            t.cardBorder = Hex("#14B8A6", 0.3f);

            t.headerPurple = Hex("#154840");
            t.headerNavy = Hex("#0E2220");
            t.backgroundNavy = Hex("#0A1520");
            t.backgroundPurple = Hex("#0E2E28");

            t.tabActive = Hex("#14B8A6");
            t.tabInactive = Hex("#1A3040");
            t.tabTextActive = Hex("#0A1520");
            t.tabTextInactive = Hex("#6B9690");

            t.scrollbarTrack = Hex("#0F1E2C");
            t.scrollbarHandle = Hex("#14B8A6", 0.5f);

            t.toggleOn = Hex("#14B8A6");
            t.toggleOff = Hex("#1A3040");
            t.toggleCheckmark = Hex("#0A1520");

            t.sliderTrack = Hex("#1A3040");
            t.sliderFill = Hex("#14B8A6");
            t.sliderHandle = Color.white;

            t.rowEven = Hex("#0F1E2C", 0.5f);
            t.rowOdd = Hex("#142738", 0.5f);

            return t;
        }

        // ============================================================
        //  9. TOXIC LIME (#84CC16) - Radioactive yellow-green
        // ============================================================
        private static ThemeData BuildToxicLime()
        {
            var t = ScriptableObject.CreateInstance<ThemeData>();
            t.themeId = "ToxicLime";
            t.themeName = "Toxic Lime";
            t.themeDescription = "Radioactive energy. Toxic yellow-green glow from the wasteland.";
            ApplyFixedColors(t);

            t.primaryAccent = Hex("#84CC16");
            t.secondaryAccent = Hex("#A3E635");
            t.tertiaryAccent = Hex("#D9F99D");

            t.buttonPrimary = Hex("#84CC16");
            t.buttonPrimaryHover = Hex("#9BD830");
            t.buttonPrimaryPressed = Hex("#65A30D");

            ApplyAccentDerived(t, "#84CC16",
                "#0C1A08", "#141F0E", "#1C2814",   // bg: dark green-black
                "#182210",                            // card
                "#A8CC96", "#4D6B3D",                 // text secondary/disabled
                "#A3E635", "#0C1A08",                 // title/onPrimary
                "#2A3D1A", "#364D24",                 // btn secondary
                "#3D5C1F", "#4D6B3D",                 // input border/placeholder
                0.55f, 0.55f);                        // glow alpha/intensity

            t.headerPurple = Hex("#2D4810");
            t.headerNavy = Hex("#14220A");
            t.backgroundNavy = Hex("#0C1A08");
            t.backgroundPurple = Hex("#1E2E0A");

            return t;
        }

        // ============================================================
        // 10. MATRIX (#22C55E) - Hacker green terminal
        // ============================================================
        private static ThemeData BuildMatrix()
        {
            var t = ScriptableObject.CreateInstance<ThemeData>();
            t.themeId = "Matrix";
            t.themeName = "Matrix";
            t.themeDescription = "Hacker terminal. Pure green on black like falling code.";
            ApplyFixedColors(t);

            t.primaryAccent = Hex("#22C55E");
            t.secondaryAccent = Hex("#4ADE80");
            t.tertiaryAccent = Hex("#BBF7D0");

            t.buttonPrimary = Hex("#22C55E");
            t.buttonPrimaryHover = Hex("#40D474");
            t.buttonPrimaryPressed = Hex("#16A34A");

            ApplyAccentDerived(t, "#22C55E",
                "#080F08", "#0E170E", "#141F14",   // bg: pure black-green
                "#121C12",                           // card
                "#8FCC8F", "#3D6B3D",               // text secondary/disabled
                "#4ADE80", "#080F08",               // title/onPrimary
                "#1A3D1A", "#244D24",               // btn secondary
                "#2D5C2D", "#3D6B3D",               // input border/placeholder
                0.45f, 0.45f);                       // glow: low CRT effect

            t.headerPurple = Hex("#154822");
            t.headerNavy = Hex("#0A1A0A");
            t.backgroundNavy = Hex("#080F08");
            t.backgroundPurple = Hex("#0E2E12");

            return t;
        }

        // ============================================================
        // 11. CYBER FUCHSIA (#D946EF) - Hot magenta cyberpunk
        // ============================================================
        private static ThemeData BuildCyberFuchsia()
        {
            var t = ScriptableObject.CreateInstance<ThemeData>();
            t.themeId = "CyberFuchsia";
            t.themeName = "Cyber Fuchsia";
            t.themeDescription = "Hot magenta cyberpunk. Tokyo neon nightclub energy.";
            ApplyFixedColors(t);

            t.primaryAccent = Hex("#D946EF");
            t.secondaryAccent = Hex("#E879F9");
            t.tertiaryAccent = Hex("#F5D0FE");

            t.buttonPrimary = Hex("#D946EF");
            t.buttonPrimaryHover = Hex("#E066F4");
            t.buttonPrimaryPressed = Hex("#B030CC");

            ApplyAccentDerived(t, "#D946EF",
                "#1A0A1A", "#240F24", "#2E162E",   // bg: dark magenta-black
                "#281228",                           // card
                "#CCA8CC", "#6B4D6B",               // text secondary/disabled
                "#E879F9", "#1A0A1A",               // title/onPrimary
                "#3D1F3D", "#4D2A4D",               // btn secondary
                "#5C2D5C", "#6B4D6B",               // input border/placeholder
                0.55f, 0.55f);                       // glow: high intensity

            t.headerPurple = Hex("#481548");
            t.headerNavy = Hex("#220E22");
            t.backgroundNavy = Hex("#1A0A1A");
            t.backgroundPurple = Hex("#2E0E2E");

            return t;
        }

        // ============================================================
        // 12. PLASMA INDIGO (#6366F1) - Deep space blue-violet
        // ============================================================
        private static ThemeData BuildPlasmaIndigo()
        {
            var t = ScriptableObject.CreateInstance<ThemeData>();
            t.themeId = "PlasmaIndigo";
            t.themeName = "Plasma Indigo";
            t.themeDescription = "Deep space energy. Electric indigo from a dimensional portal.";
            ApplyFixedColors(t);

            t.primaryAccent = Hex("#6366F1");
            t.secondaryAccent = Hex("#818CF8");
            t.tertiaryAccent = Hex("#C7D2FE");

            t.buttonPrimary = Hex("#6366F1");
            t.buttonPrimaryHover = Hex("#7C7FF5");
            t.buttonPrimaryPressed = Hex("#4F46E5");

            ApplyAccentDerived(t, "#6366F1",
                "#0A0A1E", "#0F0F28", "#161632",   // bg: very dark navy-indigo
                "#12122A",                           // card
                "#A8A8D6", "#4D4D78",               // text secondary/disabled
                "#818CF8", "#0A0A1E",               // title/onPrimary
                "#1F1F44", "#2A2A54",               // btn secondary
                "#2D2D66", "#4D4D78",               // input border/placeholder
                0.5f, 0.5f);

            t.headerPurple = Hex("#1A1A56");
            t.headerNavy = Hex("#0E0E28");
            t.backgroundNavy = Hex("#0A0A1E");
            t.backgroundPurple = Hex("#16163A");

            return t;
        }

        // ============================================================
        // 13. ELECTRIC BLUE (#3B82F6) - Royal blue racing
        // ============================================================
        private static ThemeData BuildElectricBlue()
        {
            var t = ScriptableObject.CreateInstance<ThemeData>();
            t.themeId = "ElectricBlue";
            t.themeName = "Electric Blue";
            t.themeDescription = "Royal blue intensity. Racing speed and competitive esports.";
            ApplyFixedColors(t);

            t.primaryAccent = Hex("#3B82F6");
            t.secondaryAccent = Hex("#60A5FA");
            t.tertiaryAccent = Hex("#BFDBFE");

            t.buttonPrimary = Hex("#3B82F6");
            t.buttonPrimaryHover = Hex("#5A96F8");
            t.buttonPrimaryPressed = Hex("#2563EB");

            ApplyAccentDerived(t, "#3B82F6",
                "#08101E", "#0E1828", "#142032",   // bg: deep navy
                "#101C2A",                          // card
                "#A8BCD6", "#4D6478",              // text secondary/disabled
                "#60A5FA", "#08101E",              // title/onPrimary
                "#1A2E48", "#243A58",              // btn secondary
                "#2D4A6E", "#4D6478",              // input border/placeholder
                0.5f, 0.5f);

            t.headerPurple = Hex("#0E2A56");
            t.headerNavy = Hex("#081832");
            t.backgroundNavy = Hex("#08101E");
            t.backgroundPurple = Hex("#0E2040");

            return t;
        }

        // ============================================================
        // 14. CORAL SURGE (#FB7185) - Warm coral tropical
        // ============================================================
        private static ThemeData BuildCoralSurge()
        {
            var t = ScriptableObject.CreateInstance<ThemeData>();
            t.themeId = "CoralSurge";
            t.themeName = "Coral Surge";
            t.themeDescription = "Warm coral energy. Tropical reef vibes with living warmth.";
            ApplyFixedColors(t);

            t.primaryAccent = Hex("#FB7185");
            t.secondaryAccent = Hex("#FDA4AF");
            t.tertiaryAccent = Hex("#FFE4E6");

            t.buttonPrimary = Hex("#FB7185");
            t.buttonPrimaryHover = Hex("#FC8E9E");
            t.buttonPrimaryPressed = Hex("#E05568");

            ApplyAccentDerived(t, "#FB7185",
                "#1A0C10", "#24121A", "#2E1A22",   // bg: dark warm rose
                "#281418",                           // card
                "#CCA8B0", "#6B4D54",               // text secondary/disabled
                "#FDA4AF", "#1A0C10",               // title/onPrimary
                "#3D1F28", "#4D2A34",               // btn secondary
                "#5C2D3A", "#6B4D54",               // input border/placeholder
                0.45f, 0.45f);

            t.headerPurple = Hex("#481828");
            t.headerNavy = Hex("#220E14");
            t.backgroundNavy = Hex("#1A0C10");
            t.backgroundPurple = Hex("#2E1018");

            return t;
        }

        // ============================================================
        // 15. INFRARED (#E11D48) - Neon rose-red night vision
        // ============================================================
        private static ThemeData BuildInfrared()
        {
            var t = ScriptableObject.CreateInstance<ThemeData>();
            t.themeId = "Infrared";
            t.themeName = "Infrared";
            t.themeDescription = "Night vision rose-red. Military-grade neon from thermal sensors.";
            ApplyFixedColors(t);

            t.primaryAccent = Hex("#E11D48");
            t.secondaryAccent = Hex("#FB7185");
            t.tertiaryAccent = Hex("#FFE4E6");

            t.buttonPrimary = Hex("#E11D48");
            t.buttonPrimaryHover = Hex("#F03060");
            t.buttonPrimaryPressed = Hex("#BE123C");

            ApplyAccentDerived(t, "#E11D48",
                "#1A0808", "#240E0E", "#2E1414",   // bg: very dark blood
                "#280E10",                           // card
                "#CC9898", "#6B4040",               // text secondary/disabled
                "#FB7185", "#1A0808",               // title/onPrimary
                "#3D1818", "#4D2222",               // btn secondary
                "#5C2020", "#6B4040",               // input border/placeholder
                0.5f, 0.5f);

            t.headerPurple = Hex("#481018");
            t.headerNavy = Hex("#220808");
            t.backgroundNavy = Hex("#1A0808");
            t.backgroundPurple = Hex("#2E080E");

            return t;
        }

        // ============================================================
        // 16. PHANTOM (#7C3AED @ dim) - Ultra-dark stealth
        // ============================================================
        private static ThemeData BuildPhantom()
        {
            var t = ScriptableObject.CreateInstance<ThemeData>();
            t.themeId = "Phantom";
            t.themeName = "Phantom";
            t.themeDescription = "Ghost mode. Ultra-dark stealth with barely visible violet accents.";
            ApplyFixedColors(t);

            // Accent is dim violet - ghost-like
            t.primaryAccent = Hex("#7C3AED");
            t.secondaryAccent = Hex("#6D28D9");
            t.tertiaryAccent = Hex("#A78BFA");

            t.buttonPrimary = Hex("#7C3AED");
            t.buttonPrimaryHover = Hex("#8B50F0");
            t.buttonPrimaryPressed = Hex("#5B21B6");

            // Ultra-dark backgrounds, almost pure black
            t.primaryBackground = Hex("#08060E");
            t.secondaryBackground = Hex("#0C0A14");
            t.tertiaryBackground = Hex("#100E1A");
            t.overlayColor = Hex("#000000", 0.92f);

            t.textPrimary = Hex("#D4D0E0");  // Slightly muted white
            t.textSecondary = Hex("#6B6480");
            t.textDisabled = Hex("#3A3448");
            t.textTitle = Hex("#A78BFA");     // Dim violet title
            t.textOnPrimary = Hex("#08060E");

            t.buttonSecondary = Hex("#1A162A");
            t.buttonSecondaryHover = Hex("#221E34");

            t.inputBackground = Hex("#0C0A14");
            t.inputBorder = Hex("#2A2440");
            t.inputBorderFocused = Hex("#7C3AED");
            t.inputPlaceholder = Hex("#3A3448");

            // Very dim glow - ghost mode
            t.glowColor = Hex("#7C3AED", 0.2f);
            t.glowIntensity = 0.2f;

            t.cardBackground = Hex("#0E0C18");
            t.cardBorder = Hex("#7C3AED", 0.15f);

            t.headerPurple = Hex("#141024");
            t.headerNavy = Hex("#0A0810");
            t.backgroundNavy = Hex("#08060E");
            t.backgroundPurple = Hex("#0E0A18");

            t.tabActive = Hex("#7C3AED");
            t.tabInactive = Hex("#1A162A");
            t.tabTextActive = Color.white;
            t.tabTextInactive = Hex("#4A4460");

            t.scrollbarTrack = Hex("#0C0A14");
            t.scrollbarHandle = Hex("#7C3AED", 0.3f);

            t.toggleOn = Hex("#7C3AED");
            t.toggleOff = Hex("#1A162A");
            t.toggleCheckmark = Color.white;

            t.sliderTrack = Hex("#1A162A");
            t.sliderFill = Hex("#7C3AED");
            t.sliderHandle = Hex("#D4D0E0");

            t.rowEven = Hex("#0C0A14", 0.4f);
            t.rowOdd = Hex("#100E1A", 0.4f);

            return t;
        }

        // ============================================================
        // 17. TITANIUM (#94A3B8) - Cool slate metallic
        // ============================================================
        private static ThemeData BuildTitanium()
        {
            var t = ScriptableObject.CreateInstance<ThemeData>();
            t.themeId = "Titanium";
            t.themeName = "Titanium";
            t.themeDescription = "Industrial metal. Cool slate-blue alloy with mechanical precision.";
            ApplyFixedColors(t);

            t.primaryAccent = Hex("#94A3B8");
            t.secondaryAccent = Hex("#CBD5E1");
            t.tertiaryAccent = Hex("#E2E8F0");

            t.buttonPrimary = Hex("#94A3B8");
            t.buttonPrimaryHover = Hex("#A8B5C6");
            t.buttonPrimaryPressed = Hex("#64748B");

            ApplyAccentDerived(t, "#94A3B8",
                "#10131A", "#161A22", "#1C212A",   // bg: dark steel-blue
                "#181D26",                          // card
                "#8896A8", "#4D5A6B",              // text secondary/disabled
                "#CBD5E1", "#10131A",              // title/onPrimary
                "#252D38", "#303A48",              // btn secondary
                "#3D4A5C", "#4D5A6B",             // input border/placeholder
                0.35f, 0.35f);                     // glow: subtle metallic

            t.headerPurple = Hex("#1E2838");
            t.headerNavy = Hex("#121820");
            t.backgroundNavy = Hex("#10131A");
            t.backgroundPurple = Hex("#181E28");

            return t;
        }

        // ============================================================
        // 18. NEBULA (#8B5CF6) - Cosmic violet starfield
        // ============================================================
        private static ThemeData BuildNebula()
        {
            var t = ScriptableObject.CreateInstance<ThemeData>();
            t.themeId = "Nebula";
            t.themeName = "Nebula";
            t.themeDescription = "Cosmic stardust. Ethereal violet-blue from deep space nebulae.";
            ApplyFixedColors(t);

            t.primaryAccent = Hex("#8B5CF6");
            t.secondaryAccent = Hex("#EC4899");  // Pink contrast for nebula effect
            t.tertiaryAccent = Hex("#C4B5FD");

            t.buttonPrimary = Hex("#8B5CF6");
            t.buttonPrimaryHover = Hex("#9D74F8");
            t.buttonPrimaryPressed = Hex("#7C3AED");

            // Backgrounds with blue-purple cosmic tint
            t.primaryBackground = Hex("#0C0816");
            t.secondaryBackground = Hex("#120E20");
            t.tertiaryBackground = Hex("#18142A");
            t.overlayColor = Hex("#000000", 0.88f);

            t.textPrimary = Color.white;
            t.textSecondary = Hex("#B0A8CC");
            t.textDisabled = Hex("#584D7A");
            t.textTitle = Hex("#C4B5FD");
            t.textOnPrimary = Color.white;

            t.buttonSecondary = Hex("#241C3E");
            t.buttonSecondaryHover = Hex("#302650");

            t.inputBackground = Hex("#120E20");
            t.inputBorder = Hex("#3D2D66");
            t.inputBorderFocused = Hex("#8B5CF6");
            t.inputPlaceholder = Hex("#584D7A");

            t.glowColor = Hex("#8B5CF6", 0.5f);
            t.glowIntensity = 0.5f;

            t.cardBackground = Hex("#161028");
            t.cardBorder = Hex("#8B5CF6", 0.3f);

            t.headerPurple = Hex("#2A1856");
            t.headerNavy = Hex("#100A22");
            t.backgroundNavy = Hex("#0C0816");
            t.backgroundPurple = Hex("#180E30");

            t.tabActive = Hex("#8B5CF6");
            t.tabInactive = Hex("#241C3E");
            t.tabTextActive = Color.white;
            t.tabTextInactive = Hex("#6B5AA0");

            t.scrollbarTrack = Hex("#120E20");
            t.scrollbarHandle = Hex("#8B5CF6", 0.5f);

            t.toggleOn = Hex("#8B5CF6");
            t.toggleOff = Hex("#241C3E");
            t.toggleCheckmark = Color.white;

            t.sliderTrack = Hex("#241C3E");
            t.sliderFill = Hex("#8B5CF6");
            t.sliderHandle = Color.white;

            t.rowEven = Hex("#120E20", 0.5f);
            t.rowOdd = Hex("#18142A", 0.5f);

            return t;
        }

        // ============================================================
        // 19. VOLCANIC (#F43F5E) - Lava bicolor red-orange
        // ============================================================
        private static ThemeData BuildVolcanic()
        {
            var t = ScriptableObject.CreateInstance<ThemeData>();
            t.themeId = "Volcanic";
            t.themeName = "Volcanic";
            t.themeDescription = "Living lava. Bicolor red-orange magma on dark volcanic earth.";
            ApplyFixedColors(t);

            // Bicolor: primary red-rose + secondary orange for magma effect
            t.primaryAccent = Hex("#F43F5E");
            t.secondaryAccent = Hex("#FB923C");
            t.tertiaryAccent = Hex("#FCA5A5");

            t.buttonPrimary = Hex("#F43F5E");
            t.buttonPrimaryHover = Hex("#F85A74");
            t.buttonPrimaryPressed = Hex("#D42848");

            // Backgrounds: very dark brown-red like volcanic rock
            t.primaryBackground = Hex("#1A0C08");
            t.secondaryBackground = Hex("#24120E");
            t.tertiaryBackground = Hex("#2E1A14");
            t.overlayColor = Hex("#000000", 0.85f);

            t.textPrimary = Color.white;
            t.textSecondary = Hex("#CCA898");
            t.textDisabled = Hex("#6B4D42");
            t.textTitle = Hex("#FB923C");      // Orange title for lava glow
            t.textOnPrimary = Color.white;

            t.buttonSecondary = Hex("#3D1F18");
            t.buttonSecondaryHover = Hex("#4D2A22");

            t.inputBackground = Hex("#24120E");
            t.inputBorder = Hex("#5C2D20");
            t.inputBorderFocused = Hex("#F43F5E");
            t.inputPlaceholder = Hex("#6B4D42");

            // Glow: warm red-orange lava
            t.glowColor = Hex("#F43F5E", 0.5f);
            t.glowIntensity = 0.55f;

            t.cardBackground = Hex("#28140E");
            t.cardBorder = Hex("#F43F5E", 0.3f);

            t.headerPurple = Hex("#481510");
            t.headerNavy = Hex("#220E08");
            t.backgroundNavy = Hex("#1A0C08");
            t.backgroundPurple = Hex("#2E100A");

            t.tabActive = Hex("#F43F5E");
            t.tabInactive = Hex("#3D1F18");
            t.tabTextActive = Color.white;
            t.tabTextInactive = Hex("#966B60");

            t.scrollbarTrack = Hex("#24120E");
            t.scrollbarHandle = Hex("#F43F5E", 0.5f);

            t.toggleOn = Hex("#F43F5E");
            t.toggleOff = Hex("#3D1F18");
            t.toggleCheckmark = Color.white;

            t.sliderTrack = Hex("#3D1F18");
            t.sliderFill = Hex("#F43F5E");
            t.sliderHandle = Color.white;

            t.rowEven = Hex("#24120E", 0.5f);
            t.rowOdd = Hex("#2E1A14", 0.5f);

            return t;
        }
    }
}
#endif
