using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DigitPark.Payments
{
    /// <summary>
    /// Static delegates injected from Assembly-CSharp (PaymentBridgeWiring)
    /// to avoid direct dependency on AnalyticsService, AuthenticationService,
    /// DatabaseService, CurrencyManager, and DeepLinkService from this named assembly.
    /// All delegates default to safe no-ops until wired.
    /// </summary>
    public static class PaymentBridge
    {
        // Auth
        public static Func<string> GetCurrentUserId = () => "anonymous";

        // Analytics
        public static Action<string, Dictionary<string, object>> LogCustomEvent = null;
        public static Action<string, double, string, string> LogPurchaseCompleted = null;

        // Currency
        public static Action<int, int> ProcessGemsPurchase = null;

        // Firebase Database
        public static Func<Dictionary<string, object>, Task> UpdatePlayerFields = null;

        // DeepLink
        public static Action<string, Action<string>> RegisterDeepLinkHandler = null;
    }
}
