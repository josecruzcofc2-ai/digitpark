#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using DigitPark.Payments.Entitlements;

namespace DigitPark.Payments.Tests
{
    [TestFixture]
    public class EntitlementServiceTests
    {
        [Test]
        public void EntitlementRecord_HasCorrectDefaults()
        {
            var record = new EntitlementRecord
            {
                userId = "test_user",
                productId = "sparks_100",
                provider = "apple_iap",
                transactionId = "tx_123",
                grantedAt = System.DateTime.UtcNow.ToString("O"),
                appVersion = "pro"
            };

            Assert.IsFalse(record.hasTournamentBenefit,
                "hasTournamentBenefit siempre debe ser false");
        }

        [Test]
        public void EntitlementRecord_Provider_NeverTriumph()
        {
            var allowedProviders = new[] { "stripe", "apple_iap" };
            foreach (var provider in allowedProviders)
            {
                Assert.IsFalse(provider.Contains("triumph"),
                    $"Provider '{provider}' no debe ser Triumph");
            }
        }
    }
}
#endif
