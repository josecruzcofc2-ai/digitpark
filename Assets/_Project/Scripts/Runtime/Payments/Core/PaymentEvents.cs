namespace DigitPark.Payments
{
    /// <summary>
    /// Eventos estáticos del sistema de pagos cosméticos.
    /// Cualquier parte del juego puede suscribirse sin dependencia directa.
    /// NUNCA emite eventos de Triumph — solo Stripe y AppleIAP.
    /// </summary>
    public static class PaymentEvents
    {
        public static event System.Action<string, PaymentProvider> OnPurchaseStarted;
        public static event System.Action<PaymentResult> OnPurchaseCompleted;
        public static event System.Action<PaymentResult> OnPurchaseFailed;
        public static event System.Action<PaymentProvider, string> OnProviderSwitched;
        public static event System.Action<AbortReason> OnAbortProtocolExecuted;

        internal static void EmitPurchaseStarted(string productId, PaymentProvider provider)
            => OnPurchaseStarted?.Invoke(productId, provider);
        internal static void EmitPurchaseCompleted(PaymentResult result)
            => OnPurchaseCompleted?.Invoke(result);
        internal static void EmitPurchaseFailed(PaymentResult result)
            => OnPurchaseFailed?.Invoke(result);
        internal static void EmitProviderSwitched(PaymentProvider p, string reason)
            => OnProviderSwitched?.Invoke(p, reason);
        internal static void EmitAbortExecuted(AbortReason reason)
            => OnAbortProtocolExecuted?.Invoke(reason);
    }

    public enum PaymentProvider
    {
        Stripe,
        AppleIAP,
        None
    }
}
