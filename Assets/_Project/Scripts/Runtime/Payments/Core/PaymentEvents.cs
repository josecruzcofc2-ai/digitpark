namespace DigitPark.Payments
{
    /// <summary>
    /// Eventos estáticos del sistema de pagos cosméticos (Apple IAP).
    /// </summary>
    public static class PaymentEvents
    {
        public static event System.Action<string, PaymentProvider> OnPurchaseStarted;
        public static event System.Action<PaymentResult> OnPurchaseCompleted;
        public static event System.Action<PaymentResult> OnPurchaseFailed;

        internal static void EmitPurchaseStarted(string productId, PaymentProvider provider)
            => OnPurchaseStarted?.Invoke(productId, provider);
        internal static void EmitPurchaseCompleted(PaymentResult result)
            => OnPurchaseCompleted?.Invoke(result);
        internal static void EmitPurchaseFailed(PaymentResult result)
            => OnPurchaseFailed?.Invoke(result);
    }

    public enum PaymentProvider
    {
        AppleIAP,
        None
    }
}
