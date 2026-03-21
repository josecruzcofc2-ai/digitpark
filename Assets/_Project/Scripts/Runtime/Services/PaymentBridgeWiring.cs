using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using DigitPark.Payments;
using DigitPark.Payments.UI;
using DigitPark.Services.Firebase;
using DigitPark.Monetization;
using DigitPark.UI;

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

            // WARN-01: unregister para evitar acumulación de handlers entre compras
            PaymentBridge.UnregisterDeepLinkHandler = (path) =>
            {
                _deepLinkHandlers.Remove(path);
            };

            // BUG-06: Firebase ID token para Authorization en Cloud Functions (getEntitlements, syncEntitlements)
            PaymentBridge.GetFirebaseIdToken = async () =>
            {
                try
                {
#if FIREBASE_AUTH
                    var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
                    if (user != null)
                        return await user.TokenAsync(false);
#endif
                    return null;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[PaymentBridgeWiring] GetFirebaseIdToken failed: {e.Message}");
                    return null;
                }
            };

            PaymentErrorDialog.ShowPopupCallback = (title, message, buttonText, onButton) =>
            {
                if (onButton != null)
                    PopupManager.Instance?.ShowConfirmMessage(message, onButton);
                else
                    PopupManager.Instance?.ShowErrorMessage(message);
            };

            Debug.Log("[PaymentBridgeWiring] All delegates wired to game services");
        }

        private static void OnDeepLinkActivated(string url)
        {
            if (string.IsNullOrEmpty(url)) return;

            // S9-NEW-01: Strict URI parsing — prevent handler hijacking via crafted URLs
            System.Uri uri;
            try { uri = new System.Uri(url); }
            catch { return; } // URL malformada — ignorar

            // Verificar scheme exacto
            if (uri.Scheme != "digitpark") return;

            // Match host exacto, no substring Contains
            if (_deepLinkHandlers.TryGetValue(uri.Host, out var handler))
            {
                handler?.Invoke(url);
            }
        }
    }
}
