using UnityEngine;
using System;
// Descomentar esta línea después de instalar Unity Ads desde Package Manager:
// using UnityEngine.Advertisements;

#pragma warning disable CS0414 // El campo está asignado pero su valor nunca se usa (campos para Unity Ads pendientes de activar)

namespace DigitPark.Managers
{
    /// <summary>
    /// Tipos de anuncios disponibles
    /// </summary>
    public enum AdType
    {
        Banner,
        Interstitial,
        Rewarded
    }

    /// <summary>
    /// Manager de anuncios usando Unity Ads
    ///
    /// INSTRUCCIONES DE CONFIGURACIÓN:
    /// 1. Window → Package Manager → Unity Registry → "Advertisement Legacy" → Install
    /// 2. Descomentar "using UnityEngine.Advertisements;" arriba
    /// 3. Descomentar ": IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener" en la clase
    /// 4. Descomentar todo el código marcado con "// UNITY ADS:"
    /// 5. Configurar tus Game IDs en el Inspector o aquí en el código
    /// </summary>
    public class AdManager : MonoBehaviour
        // UNITY ADS: Descomentar la siguiente línea:
        // , IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
    {
        private static AdManager _instance;
        public static AdManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<AdManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("AdManager");
                        _instance = go.AddComponent<AdManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("=== CONFIGURACIÓN DE UNITY ADS ===")]
        [Tooltip("Tu Game ID de Android (ej: 5123456)")]
        [SerializeField] private string gameIdAndroid = "YOUR_ANDROID_GAME_ID";

        [Tooltip("Tu Game ID de iOS (ej: 5123457)")]
        [SerializeField] private string gameIdIOS = "YOUR_IOS_GAME_ID";

        [Tooltip("Activa para ver anuncios de prueba. Desactiva para producción.")]
        [SerializeField] private bool testMode = true;

        [Header("=== AD UNIT IDs ===")]
        [Tooltip("ID del banner (configurado en Unity Dashboard)")]
        [SerializeField] private string bannerAdUnitId = "Banner_Android";

        [Tooltip("ID del interstitial (configurado en Unity Dashboard)")]
        [SerializeField] private string interstitialAdUnitId = "Interstitial_Android";

        [Tooltip("ID del rewarded (configurado en Unity Dashboard)")]
        [SerializeField] private string rewardedAdUnitId = "Rewarded_Android";

        [Header("=== CONFIGURACIÓN DE FRECUENCIA ===")]
        [Tooltip("Mostrar interstitial cada X partidas")]
        [SerializeField] private int gamesBetweenAds = 3;

        // Estado interno
        private bool isInitialized = false;
        private bool isBannerShowing = false;
        private bool isInterstitialLoaded = false;
        private bool isRewardedLoaded = false;

        // Eventos públicos
        public static event Action OnInterstitialClosed;
        public static event Action<bool> OnRewardedCompleted; // true = usuario completó el video

        // Contador de partidas
        private int gamesPlayedSinceLastAd = 0;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAds();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            PremiumManager.OnPremiumStatusChanged -= OnPremiumStatusChanged;
        }

        #region Initialization

        private void InitializeAds()
        {
            // Si el usuario es premium, no inicializar ads
            if (PremiumManager.Instance != null && PremiumManager.Instance.HasNoAds)
            {
                Debug.Log("[Ads] Usuario premium - Anuncios desactivados");
                return;
            }

            // Determinar Game ID según plataforma
            string gameId = "";
#if UNITY_ANDROID
            gameId = gameIdAndroid;
#elif UNITY_IOS
            gameId = gameIdIOS;
#else
            gameId = gameIdAndroid; // Default para editor
#endif

            // Validar Game ID
            if (string.IsNullOrEmpty(gameId) || gameId.Contains("YOUR_"))
            {
                Debug.LogError("[Ads] ⚠️ Game ID no configurado. Ve al Inspector de AdManager y configura tus IDs.");
                Debug.LogError("[Ads] Obtén tus IDs en: https://dashboard.unity.com → Monetization → Ads");
                return;
            }

            Debug.Log($"[Ads] Inicializando Unity Ads - GameID: {gameId}, TestMode: {testMode}");

            // UNITY ADS: Descomentar la siguiente línea:
            // Advertisement.Initialize(gameId, testMode, this);

            // Suscribirse a cambios de premium
            PremiumManager.OnPremiumStatusChanged += OnPremiumStatusChanged;
        }

        private void OnPremiumStatusChanged()
        {
            if (PremiumManager.Instance != null && PremiumManager.Instance.HasNoAds)
            {
                Debug.Log("[Ads] Usuario ahora es premium - Desactivando anuncios");
                HideBanner();
                isInitialized = false;
            }
        }

        #endregion

        #region Banner Ads

        /// <summary>
        /// Muestra el banner en la parte inferior de la pantalla
        /// </summary>
        public void ShowBanner()
        {
            if (ShouldSkipAds())
            {
                Debug.Log("[Ads] Saltando banner - Usuario premium");
                return;
            }

            if (!isInitialized)
            {
                Debug.LogWarning("[Ads] Ads no inicializados. Esperando...");
                return;
            }

            if (isBannerShowing) return;

            Debug.Log("[Ads] Mostrando banner");

            // UNITY ADS: Descomentar las siguientes líneas:
            // Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
            // Advertisement.Banner.Show(bannerAdUnitId);

            isBannerShowing = true;
        }

        /// <summary>
        /// Oculta el banner
        /// </summary>
        public void HideBanner()
        {
            if (!isBannerShowing) return;

            Debug.Log("[Ads] Ocultando banner");

            // UNITY ADS: Descomentar la siguiente línea:
            // Advertisement.Banner.Hide();

            isBannerShowing = false;
        }

        #endregion

        #region Interstitial Ads

        /// <summary>
        /// Carga un anuncio intersticial
        /// </summary>
        public void LoadInterstitial()
        {
            if (ShouldSkipAds() || !isInitialized) return;

            Debug.Log("[Ads] Cargando intersticial...");

            // UNITY ADS: Descomentar la siguiente línea:
            // Advertisement.Load(interstitialAdUnitId, this);
        }

        /// <summary>
        /// Muestra un anuncio intersticial si está cargado
        /// </summary>
        public void ShowInterstitial(Action onClosed = null)
        {
            if (ShouldSkipAds())
            {
                Debug.Log("[Ads] Saltando intersticial - Usuario premium");
                onClosed?.Invoke();
                return;
            }

            if (!isInitialized || !isInterstitialLoaded)
            {
                Debug.LogWarning("[Ads] Intersticial no disponible");
                onClosed?.Invoke();
                return;
            }

            Debug.Log("[Ads] Mostrando intersticial");

            // UNITY ADS: Descomentar la siguiente línea:
            // Advertisement.Show(interstitialAdUnitId, this);

            isInterstitialLoaded = false;

            // Temporal: simular cierre inmediato mientras no está activado Unity Ads
            onClosed?.Invoke();
            OnInterstitialClosed?.Invoke();
        }

        /// <summary>
        /// Llamar después de cada partida completada.
        /// Muestra intersticial si corresponde según la frecuencia configurada.
        /// </summary>
        public void OnGameCompleted()
        {
            if (ShouldSkipAds()) return;

            gamesPlayedSinceLastAd++;
            Debug.Log($"[Ads] Partidas desde último anuncio: {gamesPlayedSinceLastAd}/{gamesBetweenAds}");

            if (gamesPlayedSinceLastAd >= gamesBetweenAds)
            {
                gamesPlayedSinceLastAd = 0;
                ShowInterstitial();
            }
        }

        #endregion

        #region Rewarded Ads

        /// <summary>
        /// Carga un anuncio con recompensa
        /// </summary>
        public void LoadRewarded()
        {
            if (!isInitialized) return;

            Debug.Log("[Ads] Cargando rewarded...");

            // UNITY ADS: Descomentar la siguiente línea:
            // Advertisement.Load(rewardedAdUnitId, this);
        }

        /// <summary>
        /// Muestra un anuncio con recompensa.
        /// El usuario recibe recompensa solo si completa el video.
        /// </summary>
        public void ShowRewarded(Action<bool> onCompleted = null)
        {
            if (!isInitialized || !isRewardedLoaded)
            {
                Debug.LogWarning("[Ads] Rewarded no disponible");
                onCompleted?.Invoke(false);
                return;
            }

            Debug.Log("[Ads] Mostrando rewarded");

            // UNITY ADS: Descomentar la siguiente línea:
            // Advertisement.Show(rewardedAdUnitId, this);

            isRewardedLoaded = false;

            // Temporal: simular recompensa mientras no está activado Unity Ads
            onCompleted?.Invoke(true);
            OnRewardedCompleted?.Invoke(true);
        }

        /// <summary>
        /// Verifica si hay un rewarded listo para mostrar
        /// </summary>
        public bool IsRewardedReady()
        {
            return isInitialized && isRewardedLoaded;
        }

        #endregion

        #region Helper Methods

        private bool ShouldSkipAds()
        {
            return PremiumManager.Instance != null && PremiumManager.Instance.HasNoAds;
        }

        public bool IsInitialized => isInitialized;

        #endregion

        #region Unity Ads Callbacks

        // ============================================================
        // UNITY ADS: Descomentar todo este bloque después de instalar
        // ============================================================

        /*
        // Callback: Inicialización completada
        public void OnInitializationComplete()
        {
            Debug.Log("[Ads] ✅ Unity Ads inicializado correctamente");
            isInitialized = true;

            // Pre-cargar anuncios
            LoadInterstitial();
            LoadRewarded();
        }

        // Callback: Error en inicialización
        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            Debug.LogError($"[Ads] ❌ Error al inicializar Unity Ads: {error} - {message}");
            isInitialized = false;
        }

        // Callback: Anuncio cargado
        public void OnUnityAdsAdLoaded(string placementId)
        {
            Debug.Log($"[Ads] ✅ Anuncio cargado: {placementId}");

            if (placementId == interstitialAdUnitId)
                isInterstitialLoaded = true;
            else if (placementId == rewardedAdUnitId)
                isRewardedLoaded = true;
        }

        // Callback: Error al cargar anuncio
        public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
        {
            Debug.LogError($"[Ads] ❌ Error al cargar {placementId}: {error} - {message}");

            // Reintentar después de 30 segundos
            Invoke(nameof(RetryLoadAds), 30f);
        }

        private void RetryLoadAds()
        {
            if (!isInterstitialLoaded) LoadInterstitial();
            if (!isRewardedLoaded) LoadRewarded();
        }

        // Callback: Anuncio mostrado completamente
        public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
        {
            Debug.Log($"[Ads] Anuncio completado: {placementId} - Estado: {showCompletionState}");

            if (placementId == rewardedAdUnitId)
            {
                // Rewarded: dar recompensa solo si completó el video
                bool rewarded = showCompletionState == UnityAdsShowCompletionState.COMPLETED;
                OnRewardedCompleted?.Invoke(rewarded);

                if (rewarded)
                    Debug.Log("[Ads] 🎁 Usuario completó el video - Recompensa otorgada");
                else
                    Debug.Log("[Ads] Usuario saltó el video - Sin recompensa");
            }
            else if (placementId == interstitialAdUnitId)
            {
                OnInterstitialClosed?.Invoke();
            }

            // Recargar el anuncio para la próxima vez
            Advertisement.Load(placementId, this);
        }

        // Callback: Error al mostrar anuncio
        public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
        {
            Debug.LogError($"[Ads] ❌ Error al mostrar {placementId}: {error} - {message}");

            // Recargar el anuncio
            Advertisement.Load(placementId, this);
        }

        // Callbacks adicionales (opcionales)
        public void OnUnityAdsShowStart(string placementId)
        {
            Debug.Log($"[Ads] Anuncio iniciado: {placementId}");
        }

        public void OnUnityAdsShowClick(string placementId)
        {
            Debug.Log($"[Ads] Usuario clickeó en anuncio: {placementId}");
        }
        */

        #endregion
    }
}
