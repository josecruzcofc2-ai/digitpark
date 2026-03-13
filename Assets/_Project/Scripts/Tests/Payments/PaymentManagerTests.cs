#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEngine;
using DigitPark.Payments;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DigitPark.Payments.Tests
{
    [TestFixture]
    public class PaymentManagerTests
    {
        [Test]
        public void ProductCatalog_FindProduct_ReturnsCorrectProduct()
        {
            var product = ProductCatalog.FindProduct("sparks_100");
            Assert.IsNotNull(product);
            Assert.AreEqual("sparks_100", product.ProductId);
            Assert.AreEqual(100, product.GemsAmount);
        }

        [Test]
        public void ProductCatalog_FindProduct_ReturnsNullForUnknown()
        {
            var product = ProductCatalog.FindProduct("unknown_product");
            Assert.IsNull(product);
        }

        [Test]
        public void ProductCatalog_ValidateCatalogCompliance_ReturnsTrue()
        {
            bool valid = ProductCatalog.ValidateCatalogCompliance();
            Assert.IsTrue(valid, "El catálogo no debe contener términos prohibidos");
        }

        [Test]
        public void PaymentResult_Successful_HasCorrectFields()
        {
            var result = PaymentResult.Successful("sparks_100", "tx_123", PaymentProvider.AppleIAP);
            Assert.IsTrue(result.Success);
            Assert.AreEqual("sparks_100", result.ProductId);
            Assert.AreEqual("tx_123", result.TransactionId);
            Assert.AreEqual(PaymentProvider.AppleIAP, result.ProviderUsed);
            Assert.IsFalse(result.WasProviderSwitched);
        }

        [Test]
        public void PaymentResult_Failed_HasCorrectFields()
        {
            var result = PaymentResult.Failed("sparks_100", "stripe_error", "Stripe falló", PaymentProvider.Stripe);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("sparks_100", result.ProductId);
            Assert.AreEqual("stripe_error", result.ErrorCode);
            Assert.AreEqual(PaymentProvider.Stripe, result.ProviderUsed);
        }

        [Test]
        public void PaymentResult_NeverHasTriumphTransactionId()
        {
            var result = PaymentResult.Successful("sparks_100", "tx_normal_123", PaymentProvider.Stripe);
            Assert.IsFalse(result.TransactionId.StartsWith("triumph_"),
                "TransactionId de Stripe no debe empezar con 'triumph_'");
        }

        [Test]
        public void ProductCatalog_ContainsNoProhibitedTerms()
        {
            string[] prohibited = { "tournament", "prize", "cash_game", "entry_fee", "gambling" };
            foreach (var product in ProductCatalog.ProProducts)
            {
                foreach (var term in prohibited)
                {
                    Assert.IsFalse(product.ProductId.Contains(term),
                        $"ProductId '{product.ProductId}' contiene término prohibido '{term}'");
                    Assert.IsFalse(product.DisplayName.ToLower().Contains(term),
                        $"DisplayName '{product.DisplayName}' contiene término prohibido '{term}'");
                }
            }
        }
    }
}
#endif
