using UnityEngine;
using UnityEditor;
using DigitPark.Themes;

namespace DigitPark.Editor
{
    /// <summary>
    /// Configura automáticamente la propiedad isChromatic y patternTintColor
    /// en los ThemeData ScriptableObjects existentes.
    ///
    /// Chromatic = el patrón de background usa el color de acento del tema en lugar de blanco.
    /// Impacto visual: ★★★★★ — trazas neón de color sobre fondo oscuro.
    ///
    /// Menu: DigitPark/Backgrounds/Configure Chromatic Themes
    /// </summary>
    public static class ChromaticThemeConfigurator
    {
        private const string THEMES_DIR = "Assets/_Project/Resources/Themes";

        // Temas Chromatic: themeId → patternTintColor (#RRGGBB)
        // Seleccionados por máximo impacto visual de su acento con el patrón
        private static readonly (string fileName, string label, string hexTint)[] CHROMATIC_THEMES =
        {
            ("Theme_NeonDark",      "Neon Dark",      "#00FFFF"), // Cyan — el más icónico
            ("Theme_Synthwave",     "Synthwave",      "#FF1493"), // Magenta — definición del synthwave
            ("Theme_Infrared",      "Infrared",       "#FF3333"), // Rojo neón — heat signature
            ("Theme_ToxicLime",     "Toxic Lime",     "#AAFF00"), // Lima — radioactivo
            ("Theme_PlasmaIndigo",  "Plasma Indigo",  "#8A2BE2"), // Índigo — plasma visible
            ("Theme_ElectricBlue",  "Electric Blue",  "#007FFF"), // Azul eléctrico — energía
            ("Theme_Aurora",        "Aurora Borealis","#00FF7F"), // Verde aurora — el más natural
            ("Theme_Nebula",        "Nebula",         "#9D4BFF"), // Púrpura espacial — nebulosa
        };

        [MenuItem("DigitPark/Backgrounds/Configure Chromatic Themes")]
        public static void ConfigureAll()
        {
            int chromatic = 0;
            int standard  = 0;
            int missing   = 0;

            // Load all ThemeData assets in the themes folder
            string[] guids = AssetDatabase.FindAssets("t:ThemeData", new[] { THEMES_DIR });

            foreach (var guid in guids)
            {
                string path  = AssetDatabase.GUIDToAssetPath(guid);
                var    theme = AssetDatabase.LoadAssetAtPath<ThemeData>(path);
                if (theme == null) { missing++; continue; }

                string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                bool   found    = false;

                foreach (var (name, label, hex) in CHROMATIC_THEMES)
                {
                    if (fileName != name) continue;

                    ColorUtility.TryParseHtmlString(hex, out Color tint);
                    theme.isChromatic      = true;
                    theme.patternTintColor = tint;
                    EditorUtility.SetDirty(theme);
                    Debug.Log($"[ChromaticThemeConfigurator] CHROMATIC: {label} → {hex}");
                    chromatic++;
                    found = true;
                    break;
                }

                if (!found)
                {
                    // Standard theme — ensure white tint (reset any leftover value)
                    theme.isChromatic      = false;
                    theme.patternTintColor = Color.white;
                    EditorUtility.SetDirty(theme);
                    standard++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Chromatic Themes Configured",
                $"Done!\n\n" +
                $"  Chromatic (colored tint): {chromatic}\n" +
                $"  Standard  (white tint):   {standard}\n" +
                $"  Missing / skipped:        {missing}\n\n" +
                "Chromatic themes configured:\n" +
                "  Neon Dark (#00FFFF) · Synthwave (#FF1493)\n" +
                "  Infrared (#FF3333) · Toxic Lime (#AAFF00)\n" +
                "  Plasma Indigo (#8A2BE2) · Electric Blue (#007FFF)\n" +
                "  Aurora (#00FF7F) · Nebula (#9D4BFF)",
                "OK");
        }
    }
}
