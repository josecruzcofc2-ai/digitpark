#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using DigitPark.Payments;
using DigitPark.Payments.Compliance;

namespace DigitPark.Payments.Tests
{
    [TestFixture]
    public class TriumphIsolationTests
    {
        [Test]
        public void ProductCatalog_ContainsNoProhibitedTerms()
        {
            bool valid = ProductCatalog.ValidateCatalogCompliance();
            Assert.IsTrue(valid, "El catálogo cosmético no debe tener términos de Triumph");
        }

        [Test]
        public void PaymentResult_NeverHasTriumphTransactionId()
        {
            var result = PaymentResult.Successful("sparks_100", "stripe_abc123", PaymentProvider.Stripe);
            Assert.IsFalse(result.TransactionId.StartsWith("triumph_"));
            Assert.IsFalse(result.TransactionId.StartsWith("tmatch_"));
        }

        [Test]
        public void StripeComplianceGuard_RejectsProductWithTriumphTerm()
        {
            var badProduct = new CosmeticProduct
            {
                ProductId = "triumph_tournament_entry",
                DisplayName = "Entry Fee"
            };
            bool valid = StripeComplianceGuard.ValidateProduct(badProduct);
            Assert.IsFalse(valid, "Producto con 'triumph' en ID debe ser rechazado");
        }

        [Test]
        public void StripeComplianceGuard_AcceptsCleanProduct()
        {
            var goodProduct = new CosmeticProduct
            {
                ProductId = "sparks_100",
                DisplayName = "100 Sparks",
                Metadata = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "type", "cosmetic" },
                    { "has_tournament_benefit", "false" }
                }
            };
            bool valid = StripeComplianceGuard.ValidateProduct(goodProduct);
            Assert.IsTrue(valid, "Producto cosmético limpio debe ser aceptado");
        }
    }
}
#endif
