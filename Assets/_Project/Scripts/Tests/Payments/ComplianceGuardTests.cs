#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using DigitPark.Payments;
using DigitPark.Payments.Compliance;
using System.Collections.Generic;

namespace DigitPark.Payments.Tests
{
    [TestFixture]
    public class ComplianceGuardTests
    {
        [Test]
        public void ValidateProduct_CleanProduct_ReturnsTrue()
        {
            var product = new CosmeticProduct
            {
                ProductId = "sparks_500",
                DisplayName = "500 Sparks",
                Metadata = new Dictionary<string, string>
                {
                    { "type", "cosmetic" },
                    { "has_tournament_benefit", "false" }
                }
            };
            Assert.IsTrue(StripeComplianceGuard.ValidateProduct(product));
        }

        [Test]
        public void ValidateProduct_WithTournamentInId_ReturnsFalse()
        {
            var product = new CosmeticProduct
            {
                ProductId = "tournament_skin",
                DisplayName = "Tournament Skin"
            };
            Assert.IsFalse(StripeComplianceGuard.ValidateProduct(product));
        }

        [Test]
        public void ValidateSessionMetadata_AlwaysHasCosmeticType()
        {
            var metadata = new Dictionary<string, string>
            {
                { "type", "cosmetic" },
                { "has_tournament_benefit", "false" },
                { "app", "digit_park_pro" }
            };
            Assert.IsTrue(StripeComplianceGuard.ValidateSessionMetadata(metadata));
        }

        [Test]
        public void ValidateSessionMetadata_WithGamblingTerm_ReturnsFalse()
        {
            var metadata = new Dictionary<string, string>
            {
                { "type", "cosmetic" },
                { "has_tournament_benefit", "false" },
                { "note", "gambling prize" }
            };
            Assert.IsFalse(StripeComplianceGuard.ValidateSessionMetadata(metadata));
        }
    }
}
#endif
