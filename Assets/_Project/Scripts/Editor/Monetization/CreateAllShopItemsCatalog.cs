#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using DigitPark.Monetization;
using DigitPark.Navigation;

namespace DigitPark.Editor
{
    /// <summary>
    /// Creates all ShopItemData ScriptableObjects for the shop catalog.
    /// Economy V56 prices — synced with PlayerFrameService, PlayerTitleService,
    /// VictoryEffectService, ThemeManager, and ProductCatalog.
    ///
    /// Total: ~89 items
    ///   Gem Packs       (6)  — $0.99–$99.99
    ///   Bundles          (2)  — $26.25–$30.45
    ///   Frames DC       (8)  — 2K–100K DC
    ///   Frames DG       (6)  — 100–1K DG
    ///   Frames USD     (12)  — $0.99–$14.99
    ///   Titles DC       (4)  — 8K–20K DC
    ///   Titles DG       (5)  — 150–500 DG
    ///   Titles USD      (4)  — $1.99–$9.99
    ///   Effects DC      (2)  — 12K–20K DC
    ///   Effects DG      (5)  — 200–750 DG
    ///   Effects USD     (5)  — $1.99–$6.99
    ///   Themes Tier A  (10)  — 150 DG
    ///   Themes Tier B  (10)  — 350 DG
    ///   Themes Tier C   (6)  — 600 DG
    ///   Themes Earnable (3)  — 350 DG (shortcut) — ElectricViolet pending ThemeData asset
    ///
    /// Idempotent: skips assets that already exist.
    /// Menu: DigitPark/Shop/Create All Shop Items Catalog
    /// </summary>
    public static class CreateAllShopItemsCatalog
    {
        private const string SHOP_DIR   = "Assets/_Project/Resources/Shop";
        private const string IAP_PREFIX = "com.matrixsoftware.digitpark.";
        private const int EXPECTED_TOTAL = 88;

        // ──────────────────────────────────────────────────────────
        //  ENTRY POINT
        // ──────────────────────────────────────────────────────────

        [MenuItem("DigitPark/Shop/Create All Shop Items Catalog")]
        public static void CreateAll()
        {
            EnsureDirectory();

            int created = 0, skipped = 0;

            CreateGemPacks(ref created, ref skipped);
            CreateBundles(ref created, ref skipped);
            CreateFramesDC(ref created, ref skipped);
            CreateFramesDG(ref created, ref skipped);
            CreateFramesUSD(ref created, ref skipped);
            CreateTitlesDC(ref created, ref skipped);
            CreateTitlesDG(ref created, ref skipped);
            CreateTitlesUSD(ref created, ref skipped);
            CreateEffectsDC(ref created, ref skipped);
            CreateEffectsDG(ref created, ref skipped);
            CreateEffectsUSD(ref created, ref skipped);
            CreateThemes(ref created, ref skipped);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[ShopCatalog] Done — Created: {created}, Skipped: {skipped}, Total: {created + skipped}");

            EditorUtility.DisplayDialog(
                "Shop Items Catalog",
                $"Completado.\n\nCreados:  {created}\nOmitidos: {skipped} (ya existian)\nTotal:    {created + skipped} / {EXPECTED_TOTAL}",
                "OK"
            );
        }

        // ──────────────────────────────────────────────────────────
        //  GEM PACKS (6) — sortOrder 10–15 — Tab: Currency
        // ──────────────────────────────────────────────────────────

        private static void CreateGemPacks(ref int created, ref int skipped)
        {
            var items = new (string prodId, string name, int gems, int bonus, float usd, int order)[]
            {
                ("sparks_100",   "150 Sparks",    150,   0, 0.99f,   10),
                ("sparks_500",   "500 Sparks",    500,  10, 4.99f,   11),
                ("sparks_1200",  "1,200 Sparks", 1200,  20, 9.99f,   12),
                ("sparks_2500",  "2,500 Sparks", 2500,  25, 19.99f,  13),
                ("sparks_6500",  "6,500 Sparks", 6500,  30, 49.99f,  14),
                ("sparks_14000", "14,000 Sparks",14000, 35, 99.99f,  15),
            };

            foreach (var (prodId, name, gems, bonus, usd, order) in items)
            {
                var so = ScriptableObject.CreateInstance<ShopItemData>();
                so.itemId        = prodId;
                so.displayName   = name;
                so.description   = $"Get {gems} DigitGems" + (bonus > 0 ? $" (+{bonus}% bonus)" : "") + ".";
                so.itemType      = ShopItemType.DigitGemsPack;
                so.shopTab       = ShopTab.Currency;
                so.priceType     = PriceType.RealMoney;
                so.realMoneyPrice = usd;
                so.iapProductId  = IAP_PREFIX + prodId.Replace("sparks_", "gems_");
                so.gemsAmount    = gems;
                so.bonusPercent  = bonus;
                so.sortOrder     = order;
                if (bonus >= 25) so.isBestValue = true;
                Save(so, $"GemPack_{SnakeToPascal(prodId)}", ref created, ref skipped);
            }
        }

        // ──────────────────────────────────────────────────────────
        //  BUNDLES (2) — sortOrder 20–21 — Tab: Featured
        // ──────────────────────────────────────────────────────────

        private static void CreateBundles(ref int created, ref int skipped)
        {
            // Premium Bundle
            var pb = ScriptableObject.CreateInstance<ShopItemData>();
            pb.itemId        = "premium_bundle";
            pb.displayName   = "Premium Theme Bundle";
            pb.description   = "Unlock all premium themes.";
            pb.itemType      = ShopItemType.PremiumBundle;
            pb.shopTab       = ShopTab.Featured;
            pb.priceType     = PriceType.RealMoney;
            pb.realMoneyPrice = 26.25f;
            pb.iapProductId  = IAP_PREFIX + "premium_bundle";
            pb.sortOrder     = 20;
            pb.isPopular     = true;
            Save(pb, "Bundle_Premium", ref created, ref skipped);

            // Complete Bundle
            var cb = ScriptableObject.CreateInstance<ShopItemData>();
            cb.itemId        = "complete_bundle";
            cb.displayName   = "Complete Theme Collection";
            cb.description   = "Unlock every theme in the game.";
            cb.itemType      = ShopItemType.PremiumBundle;
            cb.shopTab       = ShopTab.Featured;
            cb.priceType     = PriceType.RealMoney;
            cb.realMoneyPrice = 30.45f;
            cb.iapProductId  = IAP_PREFIX + "complete_bundle";
            cb.sortOrder     = 21;
            cb.isBestValue   = true;
            Save(cb, "Bundle_Complete", ref created, ref skipped);
        }

        // ──────────────────────────────────────────────────────────
        //  FRAMES DC (8) — sortOrder 100–107
        // ──────────────────────────────────────────────────────────

        private static void CreateFramesDC(ref int created, ref int skipped)
        {
            var items = new (string id, string name, int dc, int order)[]
            {
                ("basic",    "Basic Frame",    2_000,   100),
                ("bronze",   "Bronze Frame",   5_000,   101),
                ("silver",   "Silver Frame",   12_000,  102),
                ("gold",     "Gold Frame",     25_000,  103),
                ("neon",     "Neon Frame",     40_000,  104),
                ("diamond",  "Diamond Frame",  60_000,  105),
                ("crystal",  "Crystal Frame",  80_000,  106),
                ("platinum", "Platinum Frame", 100_000, 107),
            };

            foreach (var (id, name, dc, order) in items)
            {
                var so = MakeFrame(id, name, PriceType.DigitCoins, 0, dc, 0f, "", order);
                Save(so, $"Frame_DC_{Capitalize(id)}", ref created, ref skipped);
            }
        }

        // ──────────────────────────────────────────────────────────
        //  FRAMES DG (6) — sortOrder 200–205
        // ──────────────────────────────────────────────────────────

        private static void CreateFramesDG(ref int created, ref int skipped)
        {
            var items = new (string id, string name, int dg, int order)[]
            {
                ("sapphire", "Sapphire Frame",  100,   200),
                ("ruby",     "Ruby Frame",       200,   201),
                ("emerald",  "Emerald Frame",    350,   202),
                ("amethyst", "Amethyst Frame",   500,   203),
                ("topaz",    "Topaz Frame",      750,   204),
                ("obsidian", "Obsidian Frame",  1_000,  205),
            };

            foreach (var (id, name, dg, order) in items)
            {
                var so = MakeFrame(id, name, PriceType.DigitGems, dg, 0, 0f, "", order);
                Save(so, $"Frame_DG_{Capitalize(id)}", ref created, ref skipped);
            }
        }

        // ──────────────────────────────────────────────────────────
        //  FRAMES USD (12) — sortOrder 300–311
        // ──────────────────────────────────────────────────────────

        private static void CreateFramesUSD(ref int created, ref int skipped)
        {
            var items = new (string id, string name, float usd, int order)[]
            {
                ("plasma_spark",     "Plasma Spark Frame",    0.99f,  300),
                ("prism_shift",      "Prism Shift Frame",     0.99f,  301),
                ("holographic",      "Holographic Frame",     1.99f,  302),
                ("animated_fire",    "Quantum Fire Frame",    2.99f,  303),
                ("aurora_borealis",  "Aurora Borealis Frame", 3.99f,  304),
                ("legendary_crown",  "Legendary Crown Frame", 4.99f,  305),
                ("void_walker",      "Void Walker Frame",     5.99f,  306),
                ("storm_surge",      "Storm Surge Frame",     5.99f,  307),
                ("cosmic_rift",      "Cosmic Rift Frame",     9.99f,  308),
                ("infernal_god",     "Infernal God Frame",    9.99f,  309),
                ("divine_light",     "Divine Light Frame",    14.99f, 310),
                ("quantum_break",    "Quantum Break Frame",   14.99f, 311),
            };

            foreach (var (id, name, usd, order) in items)
            {
                var so = MakeFrame(id, name, PriceType.RealMoney, 0, 0, usd, IAP_PREFIX + "frame_" + id, order);
                Save(so, $"Frame_USD_{SnakeToPascal(id)}", ref created, ref skipped);
            }
        }

        // ──────────────────────────────────────────────────────────
        //  TITLES DC (4) — sortOrder 400–403
        // ──────────────────────────────────────────────────────────

        private static void CreateTitlesDC(ref int created, ref int skipped)
        {
            var items = new (string id, string name, int dc, int order)[]
            {
                ("strategist", "Strategist", 8_000,  400),
                ("analyst",    "Analyst",    8_000,  401),
                ("champion",   "Champion",   20_000, 402),
                ("gladiator",  "Gladiator",  20_000, 403),
            };

            foreach (var (id, name, dc, order) in items)
            {
                var so = MakeTitle(id, name, PriceType.DigitCoins, 0, dc, 0f, "", order);
                Save(so, $"Title_DC_{Capitalize(id)}", ref created, ref skipped);
            }
        }

        // ──────────────────────────────────────────────────────────
        //  TITLES DG (5) — sortOrder 500–504
        // ──────────────────────────────────────────────────────────

        private static void CreateTitlesDG(ref int created, ref int skipped)
        {
            var items = new (string id, string name, int dg, int order)[]
            {
                ("mastermind", "Mastermind", 150, 500),
                ("prodigy",    "Prodigy",    150, 501),
                ("titan",      "Titan",      300, 502),
                ("oracle",     "Oracle",     300, 503),
                ("phoenix",    "Phoenix",    500, 504),
            };

            foreach (var (id, name, dg, order) in items)
            {
                var so = MakeTitle(id, name, PriceType.DigitGems, dg, 0, 0f, "", order);
                Save(so, $"Title_DG_{Capitalize(id)}", ref created, ref skipped);
            }
        }

        // ──────────────────────────────────────────────────────────
        //  TITLES USD (4) — sortOrder 505–508
        // ──────────────────────────────────────────────────────────

        private static void CreateTitlesUSD(ref int created, ref int skipped)
        {
            var items = new (string id, string name, float usd, int order)[]
            {
                ("quantum",        "Quantum",       1.99f, 505),
                ("immortal_title", "Immortal",      2.99f, 506),
                ("transcendent",   "Transcendent",  4.99f, 507),
                ("apex_predator",  "Apex Predator", 9.99f, 508),
            };

            foreach (var (id, name, usd, order) in items)
            {
                var so = MakeTitle(id, name, PriceType.RealMoney, 0, 0, usd, IAP_PREFIX + "title_" + id, order);
                Save(so, $"Title_USD_{SnakeToPascal(id)}", ref created, ref skipped);
            }
        }

        // ──────────────────────────────────────────────────────────
        //  EFFECTS DC (2) — sortOrder 700–701
        // ──────────────────────────────────────────────────────────

        private static void CreateEffectsDC(ref int created, ref int skipped)
        {
            var items = new (string id, string name, int dc, int order)[]
            {
                ("confetti",  "Confetti Burst", 12_000, 700),
                ("fireworks", "Fireworks",      20_000, 701),
            };

            foreach (var (id, name, dc, order) in items)
            {
                var so = MakeEffect(id, name, PriceType.DigitCoins, 0, dc, 0f, "", order);
                Save(so, $"Effect_DC_{Capitalize(id)}", ref created, ref skipped);
            }
        }

        // ──────────────────────────────────────────────────────────
        //  EFFECTS DG (5) — sortOrder 710–714
        // ──────────────────────────────────────────────────────────

        private static void CreateEffectsDG(ref int created, ref int skipped)
        {
            var items = new (string id, string name, int dg, int order)[]
            {
                ("lightning",      "Lightning Strike", 200, 710),
                ("gold_rain",      "Gold Rain",        250, 711),
                ("neon_explosion", "Neon Explosion",    350, 712),
                ("pixel_rain",     "Pixel Rain",        500, 713),
                ("rainbow",        "Rainbow",           750, 714),
            };

            foreach (var (id, name, dg, order) in items)
            {
                var so = MakeEffect(id, name, PriceType.DigitGems, dg, 0, 0f, "", order);
                Save(so, $"Effect_DG_{SnakeToPascal(id)}", ref created, ref skipped);
            }
        }

        // ──────────────────────────────────────────────────────────
        //  EFFECTS USD (5) — sortOrder 720–724
        // ──────────────────────────────────────────────────────────

        private static void CreateEffectsUSD(ref int created, ref int skipped)
        {
            var items = new (string id, string name, float usd, int order)[]
            {
                ("crown_drop",      "Crown Drop",       1.99f, 720),
                ("cosmic_shatter",  "Cosmic Shatter",   1.99f, 721),
                ("fire_ring",       "Fire Ring",         2.99f, 722),
                ("quantum_rift",    "Quantum Rift",      3.99f, 723),
                ("divine_ascension","Divine Ascension",  6.99f, 724),
            };

            foreach (var (id, name, usd, order) in items)
            {
                var so = MakeEffect(id, name, PriceType.RealMoney, 0, 0, usd, IAP_PREFIX + "effect_" + id, order);
                Save(so, $"Effect_USD_{SnakeToPascal(id)}", ref created, ref skipped);
            }
        }

        // ──────────────────────────────────────────────────────────
        //  THEMES (30) — sortOrder 800–829 — Tab: Styles
        //  themeId must match ThemeData asset in Resources/Themes/
        // ──────────────────────────────────────────────────────────

        private static void CreateThemes(ref int created, ref int skipped)
        {
            // Tier A — 150 DG (sortOrder 800–809)
            // themeId must match ThemeData.themeId in Resources/Themes/ (PascalCase)
            var tierA = new (string themeId, string display)[]
            {
                ("Arctic",         "Arctic"),
                ("DeepOcean",      "Deep Ocean"),
                ("ToxicLime",      "Toxic Lime"),
                ("CoralSurge",     "Coral Surge"),
                ("Thunder",        "Thunder"),
                ("Titanium",       "Titanium"),
                ("ElectricOrange", "Electric Orange"),
                ("Phantom",        "Phantom"),
                ("Infrared",       "Infrared"),
                ("IceFire",        "Ice Fire"),
            };
            for (int i = 0; i < tierA.Length; i++)
            {
                var so = MakeTheme(tierA[i].themeId, tierA[i].display, 150, 800 + i);
                Save(so, $"Theme_A_{tierA[i].themeId}", ref created, ref skipped);
            }

            // Tier B — 350 DG (sortOrder 810–819)
            var tierB = new (string themeId, string display)[]
            {
                ("Sakura",           "Sakura"),
                ("Matrix",           "Matrix"),
                ("CyberFuchsia",     "Cyber Fuchsia"),
                ("PlasmaIndigo",     "Plasma Indigo"),
                ("Nebula",           "Nebula"),
                ("Aurora",           "Aurora"),
                ("Synthwave",        "Synthwave"),
                ("Vaporwave",        "Vaporwave"),
                ("Bioluminescence",  "Bioluminescence"),
                ("Ultraviolet",      "Ultraviolet"),
            };
            for (int i = 0; i < tierB.Length; i++)
            {
                var so = MakeTheme(tierB[i].themeId, tierB[i].display, 350, 810 + i);
                Save(so, $"Theme_B_{tierB[i].themeId}", ref created, ref skipped);
            }

            // Tier C — 600 DG (sortOrder 820–825)
            var tierC = new (string themeId, string display)[]
            {
                ("Volcanic",  "Volcanic"),
                ("Outrun",    "Outrun"),
                ("Glitch",    "Glitch"),
                ("Y2KChrome", "Y2K Chrome"),
                ("BloodMoon", "Blood Moon"),
                ("Void",      "Void"),
            };
            for (int i = 0; i < tierC.Length; i++)
            {
                var so = MakeTheme(tierC[i].themeId, tierC[i].display, 600, 820 + i);
                Save(so, $"Theme_C_{tierC[i].themeId}", ref created, ref skipped);
            }

            // Earnable — 350 DG shortcut (sortOrder 826–828)
            // NOTE: ElectricViolet ThemeData asset does not exist yet — add here when created
            var earnable = new (string themeId, string display)[]
            {
                ("Emerald",      "Emerald"),
                ("ElectricBlue", "Electric Blue"),
                ("Monochrome",   "Monochrome"),
            };
            for (int i = 0; i < earnable.Length; i++)
            {
                var so = MakeTheme(earnable[i].themeId, earnable[i].display, 350, 826 + i);
                Save(so, $"Theme_E_{earnable[i].themeId}", ref created, ref skipped);
            }
        }

        // ──────────────────────────────────────────────────────────
        //  FACTORIES
        // ──────────────────────────────────────────────────────────

        private static ShopItemData MakeFrame(string itemId, string displayName,
            PriceType priceType, int gems, int coins, float usd, string iapId, int sortOrder)
        {
            var so = ScriptableObject.CreateInstance<ShopItemData>();
            so.itemId         = itemId;
            so.displayName    = displayName;
            so.description    = $"Profile frame — {displayName}.";
            so.itemType       = ShopItemType.Frame;
            so.shopTab        = ShopTab.Styles;
            so.priceType      = priceType;
            so.gemsPrice      = gems;
            so.coinsPrice     = coins;
            so.realMoneyPrice = usd;
            so.iapProductId   = iapId;
            so.sortOrder      = sortOrder;
            return so;
        }

        private static ShopItemData MakeTitle(string itemId, string displayName,
            PriceType priceType, int gems, int coins, float usd, string iapId, int sortOrder)
        {
            var so = ScriptableObject.CreateInstance<ShopItemData>();
            so.itemId         = itemId;
            so.displayName    = displayName;
            so.description    = $"Player title: \"{displayName}\".";
            so.itemType       = ShopItemType.Title;
            so.shopTab        = ShopTab.Styles;
            so.priceType      = priceType;
            so.gemsPrice      = gems;
            so.coinsPrice     = coins;
            so.realMoneyPrice = usd;
            so.iapProductId   = iapId;
            so.sortOrder      = sortOrder;
            return so;
        }

        private static ShopItemData MakeEffect(string itemId, string displayName,
            PriceType priceType, int gems, int coins, float usd, string iapId, int sortOrder)
        {
            var so = ScriptableObject.CreateInstance<ShopItemData>();
            so.itemId         = itemId;
            so.displayName    = displayName;
            so.description    = $"Victory effect: {displayName}.";
            so.itemType       = ShopItemType.WinEffect;
            so.shopTab        = ShopTab.Effects;
            so.priceType      = priceType;
            so.gemsPrice      = gems;
            so.coinsPrice     = coins;
            so.realMoneyPrice = usd;
            so.iapProductId   = iapId;
            so.sortOrder      = sortOrder;
            return so;
        }

        private static ShopItemData MakeTheme(string themeId, string displayName,
            int dgPrice, int sortOrder)
        {
            var so = ScriptableObject.CreateInstance<ShopItemData>();
            so.itemId         = themeId;
            so.displayName    = displayName;
            so.description    = $"Visual theme: {displayName}.";
            so.itemType       = ShopItemType.Theme;
            so.shopTab        = ShopTab.Styles;
            so.priceType      = PriceType.DigitGems;
            so.gemsPrice      = dgPrice;
            so.themeId        = themeId;
            so.sortOrder      = sortOrder;
            return so;
        }

        // ──────────────────────────────────────────────────────────
        //  HELPERS
        // ──────────────────────────────────────────────────────────

        private static void EnsureDirectory()
        {
            if (!AssetDatabase.IsValidFolder(SHOP_DIR))
            {
                string parent = "Assets/_Project/Resources";
                if (!AssetDatabase.IsValidFolder(parent))
                    AssetDatabase.CreateFolder("Assets/_Project", "Resources");
                AssetDatabase.CreateFolder(parent, "Shop");
                Debug.Log($"[ShopCatalog] Created folder: {SHOP_DIR}");
            }
        }

        private static void Save(ShopItemData so, string assetName, ref int created, ref int skipped)
        {
            string path = $"{SHOP_DIR}/{assetName}.asset";
            if (AssetDatabase.LoadAssetAtPath<ShopItemData>(path) != null)
            {
                skipped++;
                return;
            }
            AssetDatabase.CreateAsset(so, path);
            EditorUtility.SetDirty(so);
            created++;
        }

        private static string Capitalize(string s)
            => s.Length == 0 ? s : char.ToUpper(s[0]) + s.Substring(1);

        private static string SnakeToPascal(string snake)
        {
            var parts = snake.Split('_');
            var sb = new System.Text.StringBuilder();
            foreach (var p in parts)
                sb.Append(Capitalize(p));
            return sb.ToString();
        }

        private static string ThemeDisplayName(string themeId)
        {
            // Convert snake_case themeId to display name
            var parts = themeId.Split('_');
            var sb = new System.Text.StringBuilder();
            foreach (var p in parts)
            {
                if (sb.Length > 0) sb.Append(' ');
                sb.Append(Capitalize(p));
            }
            return sb.ToString();
        }
    }
}
#endif
