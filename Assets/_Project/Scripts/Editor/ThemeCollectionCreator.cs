#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using DigitPark.Themes;

namespace DigitPark.Editor
{
    /// <summary>
    /// Creates all 9 premium theme assets for DigitPark.
    /// Run from: DigitPark > Themes > Create All Theme Assets
    /// </summary>
    public class ThemeCollectionCreator
    {
        [MenuItem("DigitPark/Themes/Create All Theme Assets (9 Premium)")]
        public static void CreateAllThemes()
        {
            if (!EditorUtility.DisplayDialog("Crear Temas",
                "Esto creara 9 temas premium en Resources/Themes/.\n\n" +
                "- Electric Violet\n- Crimson Blaze\n- Sakura\n- Emerald\n" +
                "- Sunset\n- Arctic\n- Midnight Gold\n- Monochrome\n- Deep Ocean\n\n" +
                "Los temas existentes con el mismo nombre seran reemplazados.",
                "Crear", "Cancelar"))
                return;

            // Ensure folder exists
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Resources/Themes"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Project/Resources"))
                    AssetDatabase.CreateFolder("Assets/_Project", "Resources");
                AssetDatabase.CreateFolder("Assets/_Project/Resources", "Themes");
            }

            int count = 0;
            count += CreateTheme(BuildElectricViolet()) ? 1 : 0;
            count += CreateTheme(BuildCrimsonBlaze()) ? 1 : 0;
            count += CreateTheme(BuildSakura()) ? 1 : 0;
            count += CreateTheme(BuildEmerald()) ? 1 : 0;
            count += CreateTheme(BuildSunset()) ? 1 : 0;
            count += CreateTheme(BuildArctic()) ? 1 : 0;
            count += CreateTheme(BuildMidnightGold()) ? 1 : 0;
            count += CreateTheme(BuildMonochrome()) ? 1 : 0;
            count += CreateTheme(BuildDeepOcean()) ? 1 : 0;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Completado",
                $"Se crearon {count} temas premium en:\nResources/Themes/",
                "OK");
        }

        private static bool CreateTheme(ThemeData theme)
        {
            string path = $"Assets/_Project/Resources/Themes/Theme_{theme.themeId}.asset";

            // Delete existing
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
            return Color.magenta; // Error visible
        }

        private static Color Hex(string hex, float alpha)
        {
            Color c = Hex(hex);
            c.a = alpha;
            return c;
        }

        // ============================================================
        // Helper: Apply common fixed values to a theme
        // ============================================================
        private static void ApplyFixedColors(ThemeData t)
        {
            // --- Status colors (NEVER change with theme) ---
            t.errorColor = Hex("#FF4D4D");
            t.warningColor = Hex("#FFB020");
            t.successColor = Hex("#4DFF7C");
            t.infoColor = Hex("#4DA6FF");

            // --- Premium color (always gold) ---
            t.premiumColor = Hex("#FFD700");

            // --- Rank colors (universal) ---
            t.rank1Color = Hex("#FFD700"); // Gold
            t.rank2Color = Hex("#C0C0C0"); // Silver
            t.rank3Color = Hex("#CD7F32"); // Bronze

            // --- Text on semantic buttons ---
            t.textOnDanger = Color.white;
            t.textOnSuccess = Color.black;

            // --- Button danger (always red) ---
            t.buttonDanger = Hex("#E53E3E");

            // --- Button success (always green) ---
            t.buttonSuccess = Hex("#38A169");

            // --- Card corner radius ---
            t.cardCornerRadius = 10f;

            // --- Shadows ---
            t.useShadows = true;
            t.shadowColor = new Color(0f, 0f, 0f, 0.5f);
            t.shadowDistance = new Vector2(2f, -2f);

            // --- Animations ---
            t.colorTransitionDuration = 0.25f;
            t.useHoverAnimations = true;

            // --- isPremium ---
            t.isPremium = true;
        }

        // ============================================================
        // 1. ELECTRIC VIOLET
        // ============================================================
        private static ThemeData BuildElectricViolet()
        {
            var t = ScriptableObject.CreateInstance<ThemeData>();
            t.themeId = "ElectricViolet";
            t.themeName = "Electric Violet";
            t.themeDescription = "Purple neon energy. Streamer aesthetic with electric violet glow.";
            ApplyFixedColors(t);

            // Backgrounds
            t.primaryBackground = Hex("#0F0A1A");
            t.secondaryBackground = Hex("#160F24");
            t.tertiaryBackground = Hex("#1E152E");
            t.overlayColor = Hex("#000000", 0.85f);

            // Accents
            t.primaryAccent = Hex("#A855F7");
            t.secondaryAccent = Hex("#C084FC");
            t.tertiaryAccent = Hex("#E9D5FF");

            // Text
            t.textPrimary = Color.white;
            t.textSecondary = Hex("#B8A8CC");
            t.textDisabled = Hex("#5A4D6B");
            t.textTitle = Hex("#C084FC");
            t.textOnPrimary = Color.white;

            // Buttons
            t.buttonPrimary = Hex("#A855F7");
            t.buttonPrimaryHover = Hex("#B975F9");
            t.buttonPrimaryPressed = Hex("#7E3BD0");
            t.buttonSecondary = Hex("#2A1F3D");
            t.buttonSecondaryHover = Hex("#362A4D");

            // Input
            t.inputBackground = Hex("#160F24");
            t.inputBorder = Hex("#3D2D5C");
            t.inputBorderFocused = Hex("#A855F7");
            t.inputPlaceholder = Hex("#5A4D6B");

            // Glow
            t.glowColor = Hex("#A855F7", 0.5f);
            t.glowIntensity = 0.5f;

            // Cards
            t.cardBackground = Hex("#1A1228");
            t.cardBorder = Hex("#A855F7", 0.3f);

            // Scene specific
            t.headerPurple = Hex("#2D1548");
            t.headerNavy = Hex("#140E22");
            t.backgroundNavy = Hex("#0F0A1A");
            t.backgroundPurple = Hex("#1A0E2E");

            // Tabs
            t.tabActive = Hex("#A855F7");
            t.tabInactive = Hex("#2A1F3D");
            t.tabTextActive = Color.white;
            t.tabTextInactive = Hex("#7A6B96");

            // Scrollbar
            t.scrollbarTrack = Hex("#160F24");
            t.scrollbarHandle = Hex("#A855F7", 0.5f);

            // Toggle
            t.toggleOn = Hex("#A855F7");
            t.toggleOff = Hex("#2A1F3D");
            t.toggleCheckmark = Color.white;

            // Slider
            t.sliderTrack = Hex("#2A1F3D");
            t.sliderFill = Hex("#A855F7");
            t.sliderHandle = Color.white;

            // Leaderboard rows
            t.rowEven = Hex("#160F24", 0.5f);
            t.rowOdd = Hex("#1E152E", 0.5f);

            return t;
        }

        // ============================================================
        // 2. CRIMSON BLAZE
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
        // 3. SAKURA
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
        // 4. EMERALD
        // ============================================================
        private static ThemeData BuildEmerald()
        {
            var t = ScriptableObject.CreateInstance<ThemeData>();
            t.themeId = "Emerald";
            t.themeName = "Emerald";
            t.themeDescription = "Forest green energy. Strategic calm with Matrix vibes.";
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
        // 5. SUNSET
        // ============================================================
        private static ThemeData BuildSunset()
        {
            var t = ScriptableObject.CreateInstance<ThemeData>();
            t.themeId = "Sunset";
            t.themeName = "Sunset";
            t.themeDescription = "Warm sunset energy. Orange and amber tones with a cozy glow.";
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
        // 6. ARCTIC
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
        // 7. MIDNIGHT GOLD
        // ============================================================
        private static ThemeData BuildMidnightGold()
        {
            var t = ScriptableObject.CreateInstance<ThemeData>();
            t.themeId = "MidnightGold";
            t.themeName = "Midnight Gold";
            t.themeDescription = "Luxury VIP aesthetic. Gold accents on deep navy for a premium feel.";
            ApplyFixedColors(t);

            t.primaryBackground = Hex("#0F0F1A");
            t.secondaryBackground = Hex("#161624");
            t.tertiaryBackground = Hex("#1E1E2E");
            t.overlayColor = Hex("#000000", 0.85f);

            t.primaryAccent = Hex("#EAB308");
            t.secondaryAccent = Hex("#FDE047");
            t.tertiaryAccent = Hex("#FEF9C3");

            t.textPrimary = Color.white;
            t.textSecondary = Hex("#CCC4A8");
            t.textDisabled = Hex("#6B644D");
            t.textTitle = Hex("#FDE047");
            t.textOnPrimary = Hex("#0F0F1A");

            t.buttonPrimary = Hex("#EAB308");
            t.buttonPrimaryHover = Hex("#F0C532");
            t.buttonPrimaryPressed = Hex("#C09006");
            t.buttonSecondary = Hex("#2E2E1F");
            t.buttonSecondaryHover = Hex("#3D3D2A");

            t.inputBackground = Hex("#161624");
            t.inputBorder = Hex("#4A4A2D");
            t.inputBorderFocused = Hex("#EAB308");
            t.inputPlaceholder = Hex("#6B644D");

            t.glowColor = Hex("#EAB308", 0.45f);
            t.glowIntensity = 0.45f;

            t.cardBackground = Hex("#1A1A28");
            t.cardBorder = Hex("#EAB308", 0.3f);

            t.headerPurple = Hex("#302810");
            t.headerNavy = Hex("#181416");
            t.backgroundNavy = Hex("#0F0F1A");
            t.backgroundPurple = Hex("#1E1A0E");

            t.tabActive = Hex("#EAB308");
            t.tabInactive = Hex("#2E2E1F");
            t.tabTextActive = Hex("#0F0F1A");
            t.tabTextInactive = Hex("#968E6B");

            t.scrollbarTrack = Hex("#161624");
            t.scrollbarHandle = Hex("#EAB308", 0.5f);

            t.toggleOn = Hex("#EAB308");
            t.toggleOff = Hex("#2E2E1F");
            t.toggleCheckmark = Hex("#0F0F1A");

            t.sliderTrack = Hex("#2E2E1F");
            t.sliderFill = Hex("#EAB308");
            t.sliderHandle = Color.white;

            t.rowEven = Hex("#161624", 0.5f);
            t.rowOdd = Hex("#1E1E2E", 0.5f);

            return t;
        }

        // ============================================================
        // 8. MONOCHROME
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
        // 9. DEEP OCEAN
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
    }
}
#endif
