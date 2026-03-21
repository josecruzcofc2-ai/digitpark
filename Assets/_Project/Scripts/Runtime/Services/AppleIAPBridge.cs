using UnityEngine;
using DigitPark.Managers;
using DigitPark.Payments.AppleIAP;

namespace DigitPark.Services
{
    /// <summary>
    /// Wires PremiumManager (Assembly-CSharp) into AppleIAPProvider (DigitPark.Payments.Runtime assembly).
    /// This file is in Assembly-CSharp (outside the Payments .asmdef) so it can reference both sides.
    /// BootManager calls WireDelegate() before PaymentManager initializes.
    /// </summary>
    public static class AppleIAPBridge
    {
        public static void WireDelegate()
        {
            AppleIAPProvider.GetIsPremiumAvailable = () => PremiumManager.Instance != null;
            AppleIAPProvider.InvokePurchase = (productId, callback) =>
            {
                var pm = PremiumManager.Instance;
                if (pm != null)
                    pm.PurchaseProductWithCallback(productId, callback);
                else
                    callback?.Invoke(false, null, null);
            };
            AppleIAPProvider.InvokeRestore = (callback) =>
            {
                var pm = PremiumManager.Instance;
                if (pm != null)
                    pm.RestorePurchases(callback);
                else
                    callback?.Invoke();
            };
            Debug.Log("[AppleIAPBridge] Delegates wired to PremiumManager");
        }
    }
}
