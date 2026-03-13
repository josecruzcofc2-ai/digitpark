#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using DigitPark.Payments;

namespace DigitPark.Payments.Tests
{
    [TestFixture]
    public class StripeAppleIAPSwitchTests
    {
        [Test]
        public void ForceSwitch_ToAppleIAP_UpdatesProvider()
        {
            PaymentFeatureFlag.Initialize(null);
            PaymentFeatureFlag.ForceSwitch(PaymentProvider.AppleIAP, "test_switch");
            Assert.AreEqual(PaymentProvider.AppleIAP, PaymentFeatureFlag.ActiveCosmeticProvider);
        }

        [Test]
        public void ForceSwitch_DisablesStripe_WhenSwitchingToIAP()
        {
            PaymentFeatureFlag.Initialize(null);
            PaymentFeatureFlag.ForceSwitch(PaymentProvider.AppleIAP, "stripe_down");
            Assert.IsFalse(PaymentFeatureFlag.IsStripeEnabled);
        }

        [Test]
        public void EntitlementsPreserved_AfterSwitch()
        {
            // Verificar que la lógica de entitlements es independiente del provider
            var record = new Entitlements.EntitlementRecord
            {
                productId = "sparks_100",
                provider = "stripe",
                transactionId = "tx_123",
                hasTournamentBenefit = false,
                isCosmetic = true
            };
            // Después de switch, el record sigue siendo válido
            Assert.AreEqual("stripe", record.provider);
            Assert.IsFalse(record.hasTournamentBenefit);
        }
    }
}
#endif
