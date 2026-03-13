#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using DigitPark.Payments;

namespace DigitPark.Payments.Tests
{
    [TestFixture]
    public class FeatureFlagTests
    {
        [Test]
        public void Initialize_WithNullConfig_DefaultsToAppleIAP()
        {
            PaymentFeatureFlag.Initialize(null);
            // Con config null, debe usar defaults (AppleIAP)
            Assert.AreEqual(PaymentProvider.AppleIAP, PaymentFeatureFlag.ActiveCosmeticProvider);
        }

        [Test]
        public void Initialize_WithStripeConfig_SetsStripeProvider()
        {
            var config = new RemoteConfigData
            {
                PaymentProvider = "stripe",
                StripeEnabled = true,
                AppleIAPEnabled = true,
                AppVersion = "pro"
            };
            PaymentFeatureFlag.Initialize(config);
            // Solo si estamos en versión Pro
#if DIGIT_PARK_PRO || UNITY_EDITOR
            Assert.AreEqual(PaymentProvider.Stripe, PaymentFeatureFlag.ActiveCosmeticProvider);
#endif
        }

        [Test]
        public void ForceSwitch_UpdatesActiveProvider()
        {
            PaymentFeatureFlag.Initialize(null);
            PaymentFeatureFlag.ForceSwitch(PaymentProvider.AppleIAP, "test");
            Assert.AreEqual(PaymentProvider.AppleIAP, PaymentFeatureFlag.ActiveCosmeticProvider);
        }

        [Test]
        public void LocalFlagCache_GetDefaults_ReturnsAppleIAP()
        {
            var defaults = LocalFlagCache.GetDefaults();
            Assert.AreEqual("apple_iap", defaults.PaymentProvider);
            Assert.IsFalse(defaults.StripeEnabled);
            Assert.IsTrue(defaults.AppleIAPEnabled);
            Assert.IsFalse(defaults.TriumphEnabled);
        }
    }
}
#endif
