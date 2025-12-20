using UnityEngine;

namespace DigitPark.Ads
{
    /// <summary>
    /// Configuración de IDs de Unity Ads
    /// Crear asset: Right Click > Create > DigitPark > Ads > Ad Config
    /// </summary>
    [CreateAssetMenu(fileName = "AdConfig", menuName = "DigitPark/Ads/Ad Config")]
    public class AdConfig : ScriptableObject
    {
        [Header("=== UNITY ADS GAME IDs ===")]
        [Tooltip("Game ID de Unity Ads para Android")]
        public string androidGameId = "6008736";

        [Tooltip("Game ID de Unity Ads para iOS")]
        public string iosGameId = "6008737";

        [Header("=== INTERSTITIAL ADS ===")]
        [Tooltip("Interstitial Placement ID para Android")]
        public string androidInterstitialId = "Interstitial_Android";

        [Tooltip("Interstitial Placement ID para iOS")]
        public string iosInterstitialId = "Interstitial_iOS";

        [Header("=== REWARDED ADS ===")]
        [Tooltip("Rewarded Placement ID para Android")]
        public string androidRewardedId = "Rewarded_Android";

        [Tooltip("Rewarded Placement ID para iOS")]
        public string iosRewardedId = "Rewarded_iOS";

        [Header("=== BANNER ADS ===")]
        [Tooltip("Banner Placement ID para Android")]
        public string androidBannerId = "Banner_Android";

        [Tooltip("Banner Placement ID para iOS")]
        public string iosBannerId = "Banner_iOS";

        [Header("=== CONFIGURACIÓN ===")]
        [Tooltip("Usar modo de prueba (activar en desarrollo)")]
        public bool testMode = true;

        [Tooltip("Mostrar banner al iniciar")]
        public bool showBannerOnStart = false;

        [Tooltip("Frecuencia de interstitials (cada X partidas)")]
        [Range(1, 10)]
        public int interstitialFrequency = 3;

        /// <summary>
        /// Obtiene el Game ID según la plataforma
        /// </summary>
        public string GameId
        {
            get
            {
#if UNITY_ANDROID
                return androidGameId;
#elif UNITY_IOS
                return iosGameId;
#else
                return androidGameId;
#endif
            }
        }

        /// <summary>
        /// Obtiene el Banner Placement ID según la plataforma
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
        /// Obtiene el Interstitial Placement ID según la plataforma
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
        /// Obtiene el Rewarded Placement ID según la plataforma
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
