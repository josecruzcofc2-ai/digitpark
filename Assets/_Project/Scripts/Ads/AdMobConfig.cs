using UnityEngine;

namespace DigitPark.Ads
{
    /// <summary>
    /// Configuracion de Google AdMob
    /// Crear asset: Right Click > Create > DigitPark > Ads > AdMob Config
    /// </summary>
    [CreateAssetMenu(fileName = "AdMobConfig", menuName = "DigitPark/Ads/AdMob Config")]
    public class AdMobConfig : ScriptableObject
    {
        [Header("=== APP IDs (desde AdMob Console) ===")]
        [Tooltip("App ID de AdMob para Android (ca-app-pub-XXXXX~YYYYY)")]
        public string androidAppId = "ca-app-pub-8836537554948913~1688351060";

        [Tooltip("App ID de AdMob para iOS (ca-app-pub-XXXXX~YYYYY)")]
        public string iosAppId = "ca-app-pub-8836537554948913~7292031535";

        [Header("=== BANNER AD UNIT IDs ===")]
        [Tooltip("Banner Ad Unit ID para Android")]
        public string androidBannerId = "ca-app-pub-8836537554948913/2277160967";

        [Tooltip("Banner Ad Unit ID para iOS")]
        public string iosBannerId = "ca-app-pub-8836537554948913/7321604088";

        [Header("=== INTERSTITIAL AD UNIT IDs ===")]
        [Tooltip("Interstitial Ad Unit ID para Android")]
        public string androidInterstitialId = "ca-app-pub-8836537554948913/9828347929";

        [Tooltip("Interstitial Ad Unit ID para iOS")]
        public string iosInterstitialId = "ca-app-pub-8836537554948913/8741022184";

        [Header("=== REWARDED AD UNIT IDs ===")]
        [Tooltip("Rewarded Ad Unit ID para Android")]
        public string androidRewardedId = "ca-app-pub-8836537554948913/5803867653";

        [Tooltip("Rewarded Ad Unit ID para iOS")]
        public string iosRewardedId = "ca-app-pub-8836537554948913/3374620217";

        [Header("=== CONFIGURACION ===")]
        [Tooltip("Usar anuncios de prueba (SIEMPRE activar en desarrollo)")]
        public bool useTestAds = true;

        [Tooltip("Mostrar banner al iniciar")]
        public bool showBannerOnStart = false;

        [Tooltip("Frecuencia de interstitials (cada X partidas)")]
        [Range(1, 10)]
        public int interstitialFrequency = 3;

        [Tooltip("Posicion del banner")]
        public BannerPosition bannerPosition = BannerPosition.Bottom;

        /// <summary>
        /// Obtiene el App ID segun la plataforma
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
        /// Obtiene el Banner Ad Unit ID segun la plataforma
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
        /// Obtiene el Interstitial Ad Unit ID segun la plataforma
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
        /// Obtiene el Rewarded Ad Unit ID segun la plataforma
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

    public enum BannerPosition
    {
        Top,
        Bottom,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
}
