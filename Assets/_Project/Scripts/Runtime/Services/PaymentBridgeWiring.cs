using System.Collections.Generic;
using UnityEngine;
using DigitPark.Payments;
using DigitPark.Services.Firebase;
using DigitPark.Monetization;

namespace DigitPark.Services
{
    /// <summary>
    /// Wires Assembly-CSharp services (AnalyticsService, AuthenticationService,
    /// DatabaseService, CurrencyManager, DeepLinkService) into PaymentBridge delegates.
    /// This file is in Assembly-CSharp so it can reference both sides freely.
    /// BootManager calls Wire() before InitializePaymentSystem().
    /// </summary>
    public static class PaymentBridgeWiring
    {
        // Registered deep link handlers: path → callback
        private static readonly Dictionary<string, System.Action<string>> _deepLinkHandlers
            = new Dictionary<string, System.Action<string>>();

        public static void Wire()
        {
            PaymentBridge.GetCurrentUserId = () =>
            {
                var auth = AuthenticationService.Instance;
                return auth?.GetCurrentPlayerData()?.userId ?? "anonymous";
            };

            PaymentBridge.LogCustomEvent = (eventName, parameters) =>
            {
                AnalyticsService.Instance?.LogCustomEvent(eventName, parameters);
            };

            PaymentBridge.LogPurchaseCompleted = (productId, price, currency, transactionId) =>
            {
                AnalyticsService.Instance?.LogPurchaseCompleted(productId, price, currency, transactionId);
            };

            PaymentBridge.ProcessGemsPurchase = (gems, bonus) =>
            {
                CurrencyManager.Instance?.ProcessGemsPurchase(gems, bonus);
            };

            PaymentBridge.UpdatePlayerFields = (fields) =>
            {
                var db = DatabaseService.Instance;
                if (db != null)
                {
                    string uid = PaymentBridge.GetCurrentUserId();
                    return db.UpdatePlayerFields(uid, fields);
                }
                return System.Threading.Tasks.Task.CompletedTask;
            };

            // Deep link handler registration: stores callback and dispatches via Application.deepLinkActivated
            PaymentBridge.RegisterDeepLinkHandler = (path, callback) =>
            {
                _deepLinkHandlers[path] = callback;
                // Ensure Application.deepLinkActivated is hooked (idempotent)
                Application.deepLinkActivated -= OnDeepLinkActivated;
                Application.deepLinkActivated += OnDeepLinkActivated;
            };

            Debug.Log("[PaymentBridgeWiring] All delegates wired to game services");
        }

        private static void OnDeepLinkActivated(string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            foreach (var kvp in _deepLinkHandlers)
            {
                if (url.Contains(kvp.Key))
                {
                    kvp.Value?.Invoke(url);
                }
            }
        }
    }
}
