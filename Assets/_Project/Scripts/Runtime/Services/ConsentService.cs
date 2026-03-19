using UnityEngine;

namespace DigitPark.Services
{
    /// <summary>
    /// GDPR consent management — checks and records user consent before analytics/tracking.
    /// Consent is stored in PlayerPrefs with DP_ prefix.
    /// </summary>
    public static class ConsentService
    {
        private const string CONSENT_KEY = "DP_ConsentGiven";

        public static bool HasConsent() => SecurePrefs.GetInt(CONSENT_KEY, 0) == 1;

        public static void Accept()  { SecurePrefs.SetInt(CONSENT_KEY, 1); }

        public static void Decline() { SecurePrefs.SetInt(CONSENT_KEY, 0); }
    }
}
