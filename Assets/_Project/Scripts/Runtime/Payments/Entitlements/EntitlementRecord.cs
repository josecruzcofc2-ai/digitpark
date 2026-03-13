namespace DigitPark.Payments.Entitlements
{
    /// <summary>
    /// Registro de un entitlement (compra cosmética verificada).
    /// provider es siempre "stripe" o "apple_iap" — NUNCA "triumph".
    /// hasTournamentBenefit es siempre false.
    /// isCosmetic es siempre true.
    /// </summary>
    [System.Serializable]
    public class EntitlementRecord
    {
        public string userId;
        public string productId;
        public string provider;           // "stripe" | "apple_iap"
        public string transactionId;
        public string grantedAt;           // ISO 8601
        public string appVersion;          // "pro" | "global"
        public bool hasTournamentBenefit;  // SIEMPRE false
        public bool isCosmetic;            // SIEMPRE true
    }
}
