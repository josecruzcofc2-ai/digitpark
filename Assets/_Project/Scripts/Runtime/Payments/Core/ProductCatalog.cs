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
        public string StripePriceId { get; set; }
        public string AppleProductId { get; set; }
        public int GemsAmount { get; set; }
        public int BonusPercent { get; set; }
        public string ThemeId { get; set; }
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
                ProductId = "sparks_500",
                DisplayName = "500 Sparks",
                Type = CosmeticProductType.Consumable,
                PriceUSD = 4.99m,
                AppleProductId = "com.matrixsoftware.digitpark.gems_500",
                GemsAmount = 500,
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
                DisplayName = "14,000 Sparks",
                Type = CosmeticProductType.Consumable,
                PriceUSD = 99.99m,
                AppleProductId = "com.matrixsoftware.digitpark.gems_14000",
                GemsAmount = 14000,
                BonusPercent = 35
            },
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
            string[] prohibitedTerms = {
                "tournament", "prize", "cash_game", "skill_game",
                "real_money", "entry_fee", "wager", "bet", "gambling"
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
