using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DigitPark.Payments
{
    /// <summary>
    /// Static delegates injected from Assembly-CSharp (PaymentBridgeWiring)
    /// to avoid direct dependency on AnalyticsService, AuthenticationService,
    /// DatabaseService, and CurrencyManager from this named assembly.
    /// All delegates default to safe no-ops until wired.
    /// </summary>
    public static class PaymentBridge
    {
        // Auth
        public static Func<string> GetCurrentUserId = () => "anonymous";

        // Analytics — safe no-ops to prevent NRE if not wired
        public static Action<string, Dictionary<string, object>> LogCustomEvent = (_, __) => { };
        public static Action<string, double, string, string> LogPurchaseCompleted = (_, __, ___, ____) => { };

        // Currency
        public static Action<int, int> ProcessGemsPurchase = (_, __) =>
            UnityEngine.Debug.LogWarning("[PaymentBridge] ProcessGemsPurchase not wired");

        // Firebase Database
        public static Func<Dictionary<string, object>, Task> UpdatePlayerFields = _ =>
            Task.CompletedTask;

        // Auth — Firebase ID token para Authorization header en Cloud Functions
        // BUG-06: necesario para que SyncWithServer() pueda llamar a getEntitlements autenticado
        public static Func<Task<string>> GetFirebaseIdToken = null;
    }
}
