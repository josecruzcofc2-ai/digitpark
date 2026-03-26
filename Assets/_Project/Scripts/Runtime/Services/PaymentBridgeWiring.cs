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
    /// DatabaseService, CurrencyManager) into PaymentBridge delegates.
    /// This file is in Assembly-CSharp so it can reference both sides freely.
    /// BootManager calls Wire() before InitializePaymentSystem().
    /// </summary>
    public static class PaymentBridgeWiring
    {
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
    }
}
