using UnityEngine;
using System;

// INSTRUCCIONES DE INSTALACIÓN:
// 1. Descarga Google Mobile Ads Unity Plugin desde:
//    https://github.com/googleads/googleads-mobile-unity/releases
// 2. Importa el .unitypackage en tu proyecto
// 3. Ve a Assets > External Dependency Manager > Android Resolver > Resolve
// 4. Descomenta la línea siguiente:
// #define ADMOB_INSTALLED

namespace DigitPark.Ads
{
    /// <summary>
    /// Manager principal para anuncios de AdMob
    /// Maneja Banner, Interstitial y Rewarded ads
    /// </summary>
    public class AdManager : MonoBehaviour
    {
        private static AdManager _instance;
        public static AdManager Instance => _instance;

        [Header("Configuración")]
        [SerializeField] private AdConfig adConfig;

        // Estado
        private bool isInitialized = false;
        private int gamesPlayedSinceLastAd = 0;
        private bool hasNoAds = false;

        // Eventos
        public event Action OnRewardedAdCompleted;
        public event Action OnRewardedAdFailed;
        public event Action OnInterstitialClosed;

#if ADMOB_INSTALLED
        private GoogleMobileAds.Api.BannerView bannerView;
        private GoogleMobileAds.Api.InterstitialAd interstitialAd;
        private GoogleMobileAds.Api.RewardedAd rewardedAd;
#endif

        public bool IsInitialized => isInitialized;
        public bool HasNoAds => hasNoAds;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            // Verificar si el usuario tiene premium sin anuncios
            CheckPremiumStatus();

            if (!hasNoAds)
            {
                InitializeAds();
            }
        }

        /// <summary>
        /// Verifica si el usuario compró quitar anuncios
        /// </summary>
        private void CheckPremiumStatus()
        {
            if (PremiumManager.Instance != null)
            {
                hasNoAds = PremiumManager.Instance.HasNoAds;
            }

            // Suscribirse a cambios de premium
            PremiumManager.OnPremiumStatusChanged += OnPremiumStatusChanged;
        }

        private void OnPremiumStatusChanged()
        {
            if (PremiumManager.Instance != null)
            {
                hasNoAds = PremiumManager.Instance.HasNoAds;

                if (hasNoAds)
                {
                    HideBanner();
                    Debug.Log("[AdManager] Usuario premium - Anuncios desactivados");
                }
            }
        }

        private void OnDestroy()
        {
            PremiumManager.OnPremiumStatusChanged -= OnPremiumStatusChanged;
            DestroyAds();
        }

        #region Initialization

        /// <summary>
        /// Inicializa el SDK de AdMob
        /// </summary>
        public void InitializeAds()
        {
            if (isInitialized)
            {
                Debug.LogWarning("[AdManager] Ya está inicializado");
                return;
            }

            if (adConfig == null)
            {
                Debug.LogError("[AdManager] AdConfig no está asignado");
                return;
            }

            Debug.Log("[AdManager] Inicializando AdMob...");

#if ADMOB_INSTALLED
            // Configurar antes de inicializar
            GoogleMobileAds.Api.MobileAds.RaiseAdEventsOnUnityMainThread = true;

            GoogleMobileAds.Api.MobileAds.Initialize(initStatus =>
            {
                Debug.Log("[AdManager] AdMob inicializado correctamente");
                isInitialized = true;

                // Precargar anuncios
                LoadInterstitial();
                LoadRewarded();

                // Mostrar banner si está configurado
                if (adConfig.showBannerOnStart && !hasNoAds)
                {
                    ShowBanner();
                }
            });
#else
            // Modo simulación sin SDK
            Debug.Log("[AdManager] Modo simulación - SDK no instalado");
            isInitialized = true;
#endif
        }

        #endregion

        #region Banner Ads

        /// <summary>
        /// Muestra el banner en la parte inferior de la pantalla
        /// </summary>
        public void ShowBanner()
        {
            if (hasNoAds)
            {
                Debug.Log("[AdManager] Usuario premium - No se muestra banner");
                return;
            }

            if (!isInitialized)
            {
                Debug.LogWarning("[AdManager] No inicializado");
                return;
            }

#if ADMOB_INSTALLED
            Debug.Log("[AdManager] Mostrando banner...");

            // Destruir banner anterior si existe
            if (bannerView != null)
            {
                bannerView.Destroy();
            }

            // Crear nuevo banner
            bannerView = new GoogleMobileAds.Api.BannerView(
                adConfig.BannerId,
                GoogleMobileAds.Api.AdSize.Banner,
                GoogleMobileAds.Api.AdPosition.Bottom
            );

            // Cargar anuncio
            var adRequest = new GoogleMobileAds.Api.AdRequest();
            bannerView.LoadAd(adRequest);
#else
            Debug.Log("[AdManager] Banner mostrado (simulación)");
#endif
        }

        /// <summary>
        /// Oculta el banner
        /// </summary>
        public void HideBanner()
        {
#if ADMOB_INSTALLED
            if (bannerView != null)
            {
                bannerView.Hide();
                Debug.Log("[AdManager] Banner ocultado");
            }
#else
            Debug.Log("[AdManager] Banner ocultado (simulación)");
#endif
        }

        /// <summary>
        /// Destruye el banner completamente
        /// </summary>
        public void DestroyBanner()
        {
#if ADMOB_INSTALLED
            if (bannerView != null)
            {
                bannerView.Destroy();
                bannerView = null;
                Debug.Log("[AdManager] Banner destruido");
            }
#endif
        }

        #endregion

        #region Interstitial Ads

        /// <summary>
        /// Carga un anuncio interstitial
        /// </summary>
        public void LoadInterstitial()
        {
            if (!isInitialized || hasNoAds) return;

#if ADMOB_INSTALLED
            Debug.Log("[AdManager] Cargando interstitial...");

            // Destruir anterior si existe
            if (interstitialAd != null)
            {
                interstitialAd.Destroy();
                interstitialAd = null;
            }

            var adRequest = new GoogleMobileAds.Api.AdRequest();

            GoogleMobileAds.Api.InterstitialAd.Load(
                adConfig.InterstitialId,
                adRequest,
                (GoogleMobileAds.Api.InterstitialAd ad, GoogleMobileAds.Api.LoadAdError error) =>
                {
                    if (error != null || ad == null)
                    {
                        Debug.LogError($"[AdManager] Error cargando interstitial: {error}");
                        return;
                    }

                    Debug.Log("[AdManager] Interstitial cargado");
                    interstitialAd = ad;

                    // Configurar eventos
                    interstitialAd.OnAdFullScreenContentClosed += () =>
                    {
                        Debug.Log("[AdManager] Interstitial cerrado");
                        OnInterstitialClosed?.Invoke();
                        LoadInterstitial(); // Precargar siguiente
                    };
                }
            );
#else
            Debug.Log("[AdManager] Interstitial cargado (simulación)");
#endif
        }

        /// <summary>
        /// Muestra el interstitial si está listo
        /// </summary>
        /// <returns>true si se mostró, false si no estaba listo</returns>
        public bool ShowInterstitial()
        {
            if (hasNoAds)
            {
                Debug.Log("[AdManager] Usuario premium - No se muestra interstitial");
                return false;
            }

#if ADMOB_INSTALLED
            if (interstitialAd != null && interstitialAd.CanShowAd())
            {
                Debug.Log("[AdManager] Mostrando interstitial");
                interstitialAd.Show();
                return true;
            }
            else
            {
                Debug.Log("[AdManager] Interstitial no está listo");
                LoadInterstitial();
                return false;
            }
#else
            Debug.Log("[AdManager] Interstitial mostrado (simulación)");
            OnInterstitialClosed?.Invoke();
            return true;
#endif
        }

        /// <summary>
        /// Muestra interstitial basado en frecuencia configurada
        /// Llamar después de cada partida
        /// </summary>
        public void TryShowInterstitialAfterGame()
        {
            if (hasNoAds) return;

            gamesPlayedSinceLastAd++;

            if (gamesPlayedSinceLastAd >= adConfig.interstitialFrequency)
            {
                if (ShowInterstitial())
                {
                    gamesPlayedSinceLastAd = 0;
                }
            }
        }

        #endregion

        #region Rewarded Ads

        /// <summary>
        /// Carga un anuncio rewarded
        /// </summary>
        public void LoadRewarded()
        {
            if (!isInitialized) return;

#if ADMOB_INSTALLED
            Debug.Log("[AdManager] Cargando rewarded...");

            // Destruir anterior si existe
            if (rewardedAd != null)
            {
                rewardedAd.Destroy();
                rewardedAd = null;
            }

            var adRequest = new GoogleMobileAds.Api.AdRequest();

            GoogleMobileAds.Api.RewardedAd.Load(
                adConfig.RewardedId,
                adRequest,
                (GoogleMobileAds.Api.RewardedAd ad, GoogleMobileAds.Api.LoadAdError error) =>
                {
                    if (error != null || ad == null)
                    {
                        Debug.LogError($"[AdManager] Error cargando rewarded: {error}");
                        return;
                    }

                    Debug.Log("[AdManager] Rewarded cargado");
                    rewardedAd = ad;

                    // Configurar evento de cierre
                    rewardedAd.OnAdFullScreenContentClosed += () =>
                    {
                        LoadRewarded(); // Precargar siguiente
                    };
                }
            );
#else
            Debug.Log("[AdManager] Rewarded cargado (simulación)");
#endif
        }

        /// <summary>
        /// Verifica si hay un rewarded listo para mostrar
        /// </summary>
        public bool IsRewardedReady()
        {
#if ADMOB_INSTALLED
            return rewardedAd != null && rewardedAd.CanShowAd();
#else
            return true; // Simulación siempre listo
#endif
        }

        /// <summary>
        /// Muestra el rewarded ad
        /// </summary>
        /// <param name="onRewardEarned">Callback cuando el usuario completa el video</param>
        public void ShowRewarded(Action onRewardEarned = null)
        {
#if ADMOB_INSTALLED
            if (rewardedAd != null && rewardedAd.CanShowAd())
            {
                Debug.Log("[AdManager] Mostrando rewarded");

                rewardedAd.Show((GoogleMobileAds.Api.Reward reward) =>
                {
                    Debug.Log($"[AdManager] Reward ganado: {reward.Amount} {reward.Type}");
                    onRewardEarned?.Invoke();
                    OnRewardedAdCompleted?.Invoke();
                });
            }
            else
            {
                Debug.LogWarning("[AdManager] Rewarded no está listo");
                OnRewardedAdFailed?.Invoke();
                LoadRewarded();
            }
#else
            Debug.Log("[AdManager] Rewarded mostrado (simulación)");
            onRewardEarned?.Invoke();
            OnRewardedAdCompleted?.Invoke();
#endif
        }

        #endregion

        #region Cleanup

        private void DestroyAds()
        {
#if ADMOB_INSTALLED
            if (bannerView != null)
            {
                bannerView.Destroy();
                bannerView = null;
            }

            if (interstitialAd != null)
            {
                interstitialAd.Destroy();
                interstitialAd = null;
            }

            if (rewardedAd != null)
            {
                rewardedAd.Destroy();
                rewardedAd = null;
            }
#endif
        }

        #endregion

        #region Public Helpers

        /// <summary>
        /// Desactiva todos los anuncios (cuando el usuario compra premium)
        /// </summary>
        public void DisableAds()
        {
            hasNoAds = true;
            HideBanner();
            DestroyAds();
            Debug.Log("[AdManager] Anuncios desactivados permanentemente");
        }

        #endregion
    }
}
