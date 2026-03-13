namespace DigitPark.Payments.Compliance
{
    /// <summary>
    /// Verifica en runtime que la versión correcta accede a los endpoints correctos.
    /// La versión Global NUNCA puede acceder a Stripe o Triumph.
    /// </summary>
    public static class VersionGuard
    {
        public static bool CanAccessStripe()
        {
#if DIGIT_PARK_PRO || UNITY_EDITOR
            return PaymentFeatureFlag.IsStripeEnabled;
#else
            return false; // Global NUNCA accede a Stripe
#endif
        }

        public static bool CanAccessTriumph()
        {
#if DIGIT_PARK_PRO || UNITY_EDITOR
            return PaymentFeatureFlag.IsTriumphEnabled;
#else
            return false; // Global NUNCA accede a Triumph
#endif
        }

        public static bool CanAccessAppleIAP()
            => PaymentFeatureFlag.IsAppleIAPEnabled;

        public static string GetRequiredAppVersionHeader()
        {
#if DIGIT_PARK_PRO
            return "pro";
#elif DIGIT_PARK_GLOBAL
            return "global";
#else
            return "development";
#endif
        }

        public static void ValidateEndpointAccess(string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint)) return;

            if (endpoint.Contains("/stripe/") && !CanAccessStripe())
                throw new System.InvalidOperationException(
                    $"[VersionGuard] Esta versión no puede acceder a endpoints de Stripe: {endpoint}");

            if (endpoint.Contains("/triumph/") && !CanAccessTriumph())
                throw new System.InvalidOperationException(
                    $"[VersionGuard] Esta versión no puede acceder a endpoints de Triumph: {endpoint}");
        }
    }
}
