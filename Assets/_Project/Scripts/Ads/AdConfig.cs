using UnityEngine;

namespace DigitPark.Ads
{
    /// <summary>
    /// Configuración de IDs de AdMob
    /// Crear asset: Right Click > Create > DigitPark > Ads > Ad Config
    /// </summary>
    [CreateAssetMenu(fileName = "AdConfig", menuName = "DigitPark/Ads/Ad Config")]
    public class AdConfig : ScriptableObject
    {
        [Header("=== APP IDs ===")]
        [Tooltip("App ID de AdMob para Android (formato: ca-app-pub-XXXXXXXX~XXXXXXXX)")]
        public string androidAppId = "ca-app-pub-3940256099942544~3347511713"; // Test ID

        [Tooltip("App ID de AdMob para iOS (formato: ca-app-pub-XXXXXXXX~XXXXXXXX)")]
        public string iosAppId = "ca-app-pub-3940256099942544~1458002511"; // Test ID

        [Header("=== BANNER ADS ===")]
        [Tooltip("Banner Ad Unit ID para Android")]
        public string androidBannerId = "ca-app-pub-3940256099942544/6300978111"; // Test ID

        [Tooltip("Banner Ad Unit ID para iOS")]
        public string iosBannerId = "ca-app-pub-3940256099942544/2934735716"; // Test ID

        [Header("=== INTERSTITIAL ADS ===")]
        [Tooltip("Interstitial Ad Unit ID para Android")]
        public string androidInterstitialId = "ca-app-pub-3940256099942544/1033173712"; // Test ID

        [Tooltip("Interstitial Ad Unit ID para iOS")]
        public string iosInterstitialId = "ca-app-pub-3940256099942544/4411468910"; // Test ID

        [Header("=== REWARDED ADS ===")]
        [Tooltip("Rewarded Ad Unit ID para Android")]
        public string androidRewardedId = "ca-app-pub-3940256099942544/5224354917"; // Test ID

        [Tooltip("Rewarded Ad Unit ID para iOS")]
        public string iosRewardedId = "ca-app-pub-3940256099942544/1712485313"; // Test ID

        [Header("=== CONFIGURACIÓN ===")]
        [Tooltip("Usar IDs de prueba (activar en desarrollo)")]
        public bool useTestIds = true;

        [Tooltip("Mostrar banner en la parte inferior")]
        public bool showBannerOnStart = false;

        [Tooltip("Frecuencia de interstitials (cada X partidas)")]
        [Range(1, 10)]
        public int interstitialFrequency = 3;

        /// <summary>
        /// Obtiene el App ID según la plataforma
        /// </summary>
        public string AppId
        {
            get
            {
#if UNITY_ANDROID
                return androidAppId;
#elif UNITY_IOS
                return iosAppId;
#else
                return androidAppId;
#endif
            }
        }

        /// <summary>
        /// Obtiene el Banner ID según la plataforma
        /// </summary>
        public string BannerId
        {
            get
            {
#if UNITY_ANDROID
                return androidBannerId;
#elif UNITY_IOS
                return iosBannerId;
#else
                return androidBannerId;
#endif
            }
        }

        /// <summary>
        /// Obtiene el Interstitial ID según la plataforma
        /// </summary>
        public string InterstitialId
        {
            get
            {
#if UNITY_ANDROID
                return androidInterstitialId;
#elif UNITY_IOS
                return iosInterstitialId;
#else
                return androidInterstitialId;
#endif
            }
        }

        /// <summary>
        /// Obtiene el Rewarded ID según la plataforma
        /// </summary>
        public string RewardedId
        {
            get
            {
#if UNITY_ANDROID
                return androidRewardedId;
#elif UNITY_IOS
                return iosRewardedId;
#else
                return androidRewardedId;
#endif
            }
        }
    }
}
