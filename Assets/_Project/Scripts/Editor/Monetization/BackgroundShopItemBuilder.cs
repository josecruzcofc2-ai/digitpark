using UnityEngine;
using UnityEditor;
using DigitPark.Monetization;
using DigitPark.Navigation;

namespace DigitPark.Editor
{
    /// <summary>
    /// Genera los 13 ShopItemData ScriptableObjects para la sección Backgrounds del Shop.
    /// Crea o actualiza los assets en Assets/_Project/Resources/Configs/Shop/Backgrounds/.
    /// Menu: DigitPark/Backgrounds/Build All Background Shop Items
    /// </summary>
    public static class BackgroundShopItemBuilder
    {
        private const string OUTPUT_DIR = "Assets/_Project/Resources/Configs/Shop/Backgrounds";

        [MenuItem("DigitPark/Backgrounds/Build All Background Shop Items")]
        public static void BuildAll()
        {
            EnsureFolderExists();

            int count = 0;

            // ── TIER PREMIUM DG (190–140) ────────────────────────────────────────
            count += Create("bg_neural",        "Neural Network",
                "Red neuronal de nodos y sinapsis. La identidad cognitiva de DigitPark.",
                PriceType.DigitGems, gemsPrice: 190, sortOrder: 1);

            count += Create("bg_circuit",       "Circuit Board",
                "Trazas de PCB con nodos. El patrón más on-brand — tecnología pura.",
                PriceType.DigitGems, gemsPrice: 170, sortOrder: 2);

            count += Create("bg_dna",           "DNA Helix",
                "Doble hélice de ADN. Científico, premium, único en el catálogo.",
                PriceType.DigitGems, gemsPrice: 160, sortOrder: 3);

            count += Create("bg_constellation", "Constellation",
                "Mapa estelar con líneas. Elegante, espacioso, aspiracional.",
                PriceType.DigitGems, gemsPrice: 150, sortOrder: 4);

            count += Create("bg_fingerprint",   "Fingerprint",
                "Curvas biométricas concéntricas. Orgánico y exclusivo.",
                PriceType.DigitGems, gemsPrice: 140, sortOrder: 5);

            // ── TIER MID DG (120–110) ─────────────────────────────────────────────
            count += Create("bg_triangles",     "Triangular Mesh",
                "Malla low-poly geométrica. Sofisticado y versátil.",
                PriceType.DigitGems, gemsPrice: 120, sortOrder: 6);

            count += Create("bg_waveform",      "Waveform",
                "Ondas de audio entrelazadas. Dinámico y elegante.",
                PriceType.DigitGems, gemsPrice: 110, sortOrder: 7);

            // ── TIER ENTRY DG (100–70) ────────────────────────────────────────────
            count += Create("bg_digits",        "Digit Rain",
                "Lluvia de dígitos 0-9. El más on-brand para DigitPark.",
                PriceType.DigitGems, gemsPrice: 100, sortOrder: 8);

            count += Create("bg_binary",        "Binary Rain",
                "Columnas de 0s y 1s. Estética Matrix clásica.",
                PriceType.DigitGems, gemsPrice: 90, sortOrder: 9);

            count += Create("bg_hexgrid",       "Hex Grid",
                "Hexágonos outline dispersos. Geométrico y moderno.",
                PriceType.DigitGems, gemsPrice: 80, sortOrder: 10);

            count += Create("bg_crosshatch",    "Crosshatch",
                "Líneas diagonales cruzadas. Elegante y discreto.",
                PriceType.DigitGems, gemsPrice: 70, sortOrder: 11);

            // ── TIER DC EARNABLE ─────────────────────────────────────────────────
            count += Create("bg_grid",          "Graph Grid",
                "Cuadrícula milimetrada. Técnico y limpio. Ganable jugando.",
                PriceType.DigitCoins, coinsPrice: 12000, sortOrder: 12);

            count += Create("bg_dots",          "Dot Matrix",
                "Puntos equidistantes. Minimal y versátil. Primer objetivo free.",
                PriceType.DigitCoins, coinsPrice: 8000, sortOrder: 13);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Background Shop Items Created",
                $"{count} ShopItemData assets created/updated in:\n{OUTPUT_DIR}\n\n" +
                "Next step: run Shop UIBuilder to refresh the Shop scene.",
                "OK");

            Debug.Log($"[BackgroundShopItemBuilder] {count} items en {OUTPUT_DIR}");
        }

        // ==================== Core ====================

        private static int Create(
            string patternId,
            string displayName,
            string description,
            PriceType priceType,
            int gemsPrice  = 0,
            int coinsPrice = 0,
            int sortOrder  = 0)
        {
            string path = $"{OUTPUT_DIR}/{patternId}.asset";

            // Load or create
            var existing = AssetDatabase.LoadAssetAtPath<ShopItemData>(path);
            ShopItemData item;
            if (existing != null)
            {
                item = existing;
            }
            else
            {
                item = ScriptableObject.CreateInstance<ShopItemData>();
                AssetDatabase.CreateAsset(item, path);
            }

            item.itemId             = patternId;
            item.displayName        = displayName;
            item.description        = description;
            item.itemType           = ShopItemType.BackgroundPattern;
            item.shopTab            = ShopTab.Backgrounds;
            item.priceType          = priceType;
            item.gemsPrice          = gemsPrice;
            item.coinsPrice         = coinsPrice;
            item.backgroundPatternId = patternId;
            item.sortOrder          = sortOrder;

            // Load preview sprite from Resources/Backgrounds/
            string spritePath = $"Assets/_Project/Resources/Backgrounds/{patternId}.png";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite != null)
                item.icon = sprite;
            else
                Debug.LogWarning($"[BackgroundShopItemBuilder] Sprite not found: {spritePath}");

            EditorUtility.SetDirty(item);
            return 1;
        }

        // ==================== Folder setup ====================

        private static void EnsureFolderExists()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Resources"))
                AssetDatabase.CreateFolder("Assets/_Project", "Resources");

            if (!AssetDatabase.IsValidFolder("Assets/_Project/Resources/Configs"))
                AssetDatabase.CreateFolder("Assets/_Project/Resources", "Configs");

            if (!AssetDatabase.IsValidFolder("Assets/_Project/Resources/Configs/Shop"))
                AssetDatabase.CreateFolder("Assets/_Project/Resources/Configs", "Shop");

            if (!AssetDatabase.IsValidFolder(OUTPUT_DIR))
                AssetDatabase.CreateFolder("Assets/_Project/Resources/Configs/Shop", "Backgrounds");
        }
    }
}
