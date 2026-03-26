using System.Collections.Generic;
using UnityEngine;

namespace DigitPark.Payments
{
    public enum CosmeticProductType
    {
        Consumable,
        NonConsumable,
        Subscription
    }

    /// <summary>
    /// Definición de producto cosmético.
    /// NUNCA incluye entry fees, depósitos o cualquier cosa de Triumph.
    /// </summary>
    public class CosmeticProduct
    {
        public string ProductId { get; set; }
        public string DisplayName { get; set; }
        public CosmeticProductType Type { get; set; }
        public decimal PriceUSD { get; set; }
        public string AppleProductId { get; set; }
        public int GemsAmount { get; set; }
        public int BonusPercent { get; set; }
        public Dictionary<string, string> Metadata { get; set; }

        public CosmeticProduct()
        {
            Metadata = new Dictionary<string, string>
            {
                { "type", "cosmetic" },
                { "app", "digit_park_pro" },
                { "has_tournament_benefit", "false" }
            };
        }
    }

    /// <summary>
    /// Catálogo de productos cosméticos por versión.
    /// REGLA: Ningún producto puede tener "tournament", "prize", "cash", "entry_fee",
    /// "real_money", "wager", "bet" o "gambling" en ningún campo.
    /// </summary>
    public static class ProductCatalog
    {
        public static readonly CosmeticProduct[] ProProducts = new CosmeticProduct[]
        {
            // ── Gem Packs (C-02: amounts alineados al diseño) ──
            new CosmeticProduct
            {
                ProductId = "sparks_100",
                DisplayName = "100 Sparks",
                Type = CosmeticProductType.Consumable,
                PriceUSD = 0.99m,
                AppleProductId = "com.matrixsoftware.digitpark.gems_100",
                GemsAmount = 100,
                BonusPercent = 0
            },
            new CosmeticProduct
            {
                ProductId = "sparks_300",
                DisplayName = "300 Sparks",
                Type = CosmeticProductType.Consumable,
                PriceUSD = 2.99m,
                AppleProductId = "com.matrixsoftware.digitpark.gems_300",
                GemsAmount = 300,
                BonusPercent = 0
            },
            new CosmeticProduct
            {
                ProductId = "sparks_500",
                DisplayName = "650 Sparks",
                Type = CosmeticProductType.Consumable,
                PriceUSD = 4.99m,
                AppleProductId = "com.matrixsoftware.digitpark.gems_500",
                GemsAmount = 650,
                BonusPercent = 10
            },
            new CosmeticProduct
            {
                ProductId = "sparks_1200",
                DisplayName = "1,200 Sparks",
                Type = CosmeticProductType.Consumable,
                PriceUSD = 9.99m,
                AppleProductId = "com.matrixsoftware.digitpark.gems_1200",
                GemsAmount = 1200,
                BonusPercent = 20
            },
            new CosmeticProduct
            {
                ProductId = "sparks_2500",
                DisplayName = "2,500 Sparks",
                Type = CosmeticProductType.Consumable,
                PriceUSD = 19.99m,
                AppleProductId = "com.matrixsoftware.digitpark.gems_2500",
                GemsAmount = 2500,
                BonusPercent = 25
            },
            new CosmeticProduct
            {
                ProductId = "sparks_6500",
                DisplayName = "6,500 Sparks",
                Type = CosmeticProductType.Consumable,
                PriceUSD = 49.99m,
                AppleProductId = "com.matrixsoftware.digitpark.gems_6500",
                GemsAmount = 6500,
                BonusPercent = 30
            },
            new CosmeticProduct
            {
                ProductId = "sparks_14000",
                DisplayName = "18,900 Sparks",
                Type = CosmeticProductType.Consumable,
                PriceUSD = 99.99m,
                AppleProductId = "com.matrixsoftware.digitpark.gems_14000",
                GemsAmount = 18900,
                BonusPercent = 35
            },

            // ── Theme Bundles ──
            new CosmeticProduct
            {
                ProductId = "premium_bundle",
                DisplayName = "Premium Theme Bundle",
                Type = CosmeticProductType.NonConsumable,
                PriceUSD = 26.25m,
                AppleProductId = "com.matrixsoftware.digitpark.premium_bundle"
            },
            new CosmeticProduct
            {
                ProductId = "complete_bundle",
                DisplayName = "Complete Theme Collection",
                Type = CosmeticProductType.NonConsumable,
                PriceUSD = 30.45m,
                AppleProductId = "com.matrixsoftware.digitpark.complete_bundle"
            },

            // ── Welcome Packs (C-01) ──
            new CosmeticProduct
            {
                ProductId = "welcome_pack_basic",
                DisplayName = "Welcome Pack",
                Type = CosmeticProductType.NonConsumable,
                PriceUSD = 1.99m,
                AppleProductId = "com.matrixsoftware.digitpark.welcome_pack_basic"
            },
            new CosmeticProduct
            {
                ProductId = "welcome_pack_vip",
                DisplayName = "VIP Welcome Pack",
                Type = CosmeticProductType.NonConsumable,
                PriceUSD = 4.99m,
                AppleProductId = "com.matrixsoftware.digitpark.welcome_pack_vip"
            },

            // ── Starter Pack (C-01) ──
            new CosmeticProduct
            {
                ProductId = "starter_pack",
                DisplayName = "Starter Pack",
                Type = CosmeticProductType.NonConsumable,
                PriceUSD = 2.99m,
                AppleProductId = "com.matrixsoftware.digitpark.starter_pack"
            },

            // ── Ad-Free (C-01) ──
            new CosmeticProduct
            {
                ProductId = "adfree_permanent",
                DisplayName = "Ad-Free",
                Type = CosmeticProductType.NonConsumable,
                PriceUSD = 4.99m,
                AppleProductId = "com.matrixsoftware.digitpark.adfree_permanent"
            },

            // ── Premium Pass (C-01) ──
            new CosmeticProduct
            {
                ProductId = "premium_pass_monthly",
                DisplayName = "Premium Pass",
                Type = CosmeticProductType.Subscription,
                PriceUSD = 9.99m,
                AppleProductId = "com.matrixsoftware.digitpark.premium_pass_monthly"
            },

            // ── Titles USD (C-01) ──
            new CosmeticProduct
            {
                ProductId = "title_quantum",
                DisplayName = "Quantum",
                Type = CosmeticProductType.NonConsumable,
                PriceUSD = 1.99m,
                AppleProductId = "com.matrixsoftware.digitpark.title_quantum"
            },
            new CosmeticProduct
            {
                ProductId = "title_immortal",
                DisplayName = "Immortal",
                Type = CosmeticProductType.NonConsumable,
                PriceUSD = 2.99m,
                AppleProductId = "com.matrixsoftware.digitpark.title_immortal"
            },
            new CosmeticProduct
            {
                ProductId = "title_transcendent",
                DisplayName = "Transcendent",
                Type = CosmeticProductType.NonConsumable,
                PriceUSD = 4.99m,
                AppleProductId = "com.matrixsoftware.digitpark.title_transcendent"
            },
            new CosmeticProduct
            {
                ProductId = "title_apex_predator",
                DisplayName = "Apex Predator",
                Type = CosmeticProductType.NonConsumable,
                PriceUSD = 9.99m,
                AppleProductId = "com.matrixsoftware.digitpark.title_apex_predator"
            },

            // ── Win Effects USD (C-01) ──
            new CosmeticProduct
            {
                ProductId = "effect_cosmic_shatter",
                DisplayName = "Cosmic Shatter",
                Type = CosmeticProductType.NonConsumable,
                PriceUSD = 1.99m,
                AppleProductId = "com.matrixsoftware.digitpark.effect_cosmic_shatter"
            },
            new CosmeticProduct
            {
                ProductId = "effect_quantum_rift",
                DisplayName = "Quantum Rift",
                Type = CosmeticProductType.NonConsumable,
                PriceUSD = 3.99m,
                AppleProductId = "com.matrixsoftware.digitpark.effect_quantum_rift"
            },
            new CosmeticProduct
            {
                ProductId = "effect_divine_ascension",
                DisplayName = "Divine Ascension",
                Type = CosmeticProductType.NonConsumable,
                PriceUSD = 6.99m,
                AppleProductId = "com.matrixsoftware.digitpark.effect_divine_ascension"
            },

            // ── Profile Frames USD ──
            new CosmeticProduct
            {
                ProductId = "frame_plasma_spark",
                DisplayName = "Plasma Spark Frame",
                Type = CosmeticProductType.NonConsumable,
                PriceUSD = 0.99m,
                AppleProductId = "com.matrixsoftware.digitpark.frame_plasma_spark"
            },
            new CosmeticProduct
            {
                ProductId = "frame_prism_shift",
                DisplayName = "Prism Shift Frame",
                Type = CosmeticProductType.NonConsumable,
                PriceUSD = 0.99m,
                AppleProductId = "com.matrixsoftware.digitpark.frame_prism_shift"
            },
            new CosmeticProduct
            {
                ProductId = "frame_aurora_borealis",
                DisplayName = "Aurora Borealis Frame",
                Type = CosmeticProductType.NonConsumable,
                PriceUSD = 3.99m,
                AppleProductId = "com.matrixsoftware.digitpark.frame_aurora_borealis"
            },
            new CosmeticProduct
            {
                ProductId = "frame_void_walker",
                DisplayName = "Void Walker Frame",
                Type = CosmeticProductType.NonConsumable,
                PriceUSD = 5.99m,
                AppleProductId = "com.matrixsoftware.digitpark.frame_void_walker"
            },
            new CosmeticProduct
            {
                ProductId = "frame_storm_surge",
                DisplayName = "Storm Surge Frame",
                Type = CosmeticProductType.NonConsumable,
                PriceUSD = 5.99m,
                AppleProductId = "com.matrixsoftware.digitpark.frame_storm_surge"
            },
            new CosmeticProduct
            {
                ProductId = "frame_cosmic_rift",
                DisplayName = "Cosmic Rift Frame",
                Type = CosmeticProductType.NonConsumable,
                PriceUSD = 9.99m,
                AppleProductId = "com.matrixsoftware.digitpark.frame_cosmic_rift"
            },
            new CosmeticProduct
            {
                ProductId = "frame_infernal_god",
                DisplayName = "Infernal God Frame",
                Type = CosmeticProductType.NonConsumable,
                PriceUSD = 9.99m,
                AppleProductId = "com.matrixsoftware.digitpark.frame_infernal_god"
            },
            new CosmeticProduct
            {
                ProductId = "frame_divine_light",
                DisplayName = "Divine Light Frame",
                Type = CosmeticProductType.NonConsumable,
                PriceUSD = 14.99m,
                AppleProductId = "com.matrixsoftware.digitpark.frame_divine_light"
            },
            new CosmeticProduct
            {
                ProductId = "frame_quantum_break",
                DisplayName = "Quantum Break Frame",
                Type = CosmeticProductType.NonConsumable,
                PriceUSD = 14.99m,
                AppleProductId = "com.matrixsoftware.digitpark.frame_quantum_break"
            },
            // C-01: Frames USD adicionales
            new CosmeticProduct
            {
                ProductId = "frame_holographic",
                DisplayName = "Holographic Frame",
                Type = CosmeticProductType.NonConsumable,
                PriceUSD = 1.99m,
                AppleProductId = "com.matrixsoftware.digitpark.frame_holographic"
            },
            new CosmeticProduct
            {
                ProductId = "frame_quantum_fire",
                DisplayName = "Quantum Fire Frame",
                Type = CosmeticProductType.NonConsumable,
                PriceUSD = 2.99m,
                AppleProductId = "com.matrixsoftware.digitpark.frame_quantum_fire"
            },
            new CosmeticProduct
            {
                ProductId = "frame_legendary_crown",
                DisplayName = "Legendary Crown Frame",
                Type = CosmeticProductType.NonConsumable,
                PriceUSD = 4.99m,
                AppleProductId = "com.matrixsoftware.digitpark.frame_legendary_crown"
            }
        };

        public static readonly CosmeticProduct[] GlobalProducts = ProProducts;

        public static CosmeticProduct[] GetCatalog(AppVersion version)
            => version == AppVersion.Pro ? ProProducts : GlobalProducts;

        public static CosmeticProduct FindProduct(string productId)
        {
            foreach (var p in ProProducts)
                if (p.ProductId == productId) return p;
            return null;
        }

        /// <summary>
        /// Busca producto por Apple Product ID (para mapping IAP -> CosmeticProduct)
        /// </summary>
        public static CosmeticProduct FindByAppleProductId(string appleProductId)
        {
            foreach (var p in ProProducts)
                if (p.AppleProductId == appleProductId) return p;
            return null;
        }

        public static bool ValidateCatalogCompliance()
        {
            // DISC-02: Validacion de compliance (incluye "triumph")
            string[] prohibitedTerms = {
                "tournament", "prize", "cash_game", "skill_game",
                "real_money", "entry_fee", "wager", "bet", "gambling", "triumph"
            };

            foreach (var product in ProProducts)
            {
                string id = product.ProductId.ToLowerInvariant();
                string name = product.DisplayName.ToLowerInvariant();
                foreach (var term in prohibitedTerms)
                {
                    if (id.Contains(term) || name.Contains(term))
                    {
                        Debug.LogError(
                            $"[ProductCatalog] COMPLIANCE VIOLATION: Product '{product.ProductId}' " +
                            $"contiene término prohibido '{term}'.");
                        return false;
                    }
                }
            }
            return true;
        }
    }

    public enum AppVersion
    {
        Pro,
        Global
    }
}
