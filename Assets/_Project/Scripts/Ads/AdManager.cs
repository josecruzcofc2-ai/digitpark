using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DigitPark.Managers;
using DigitPark.Localization;

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
        private int interstitialsShownCount = 0;

        [Header("Post-Ad Popup")]
        [SerializeField] private bool showRemoveAdsPopup = true;
        [SerializeField] private int showPopupAfterAds = 2; // Mostrar popup después de X interstitials
        private GameObject currentPopup;

        // Eventos
        public event Action OnRewardedAdCompleted;
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
                        TryShowRemoveAdsPopup(); // Mostrar popup de quitar anuncios
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
            TryShowRemoveAdsPopup(); // Mostrar popup de quitar anuncios
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

        #region Remove Ads Popup

        /// <summary>
        /// Muestra el popup de "quitar anuncios" después de un interstitial
        /// </summary>
        private void TryShowRemoveAdsPopup()
        {
            if (!showRemoveAdsPopup || hasNoAds) return;

            interstitialsShownCount++;

            // Mostrar popup cada X interstitials
            if (interstitialsShownCount >= showPopupAfterAds)
            {
                interstitialsShownCount = 0;
                ShowRemoveAdsPopup();
            }
        }

        /// <summary>
        /// Crea y muestra el popup de quitar anuncios con estilo neon
        /// </summary>
        public void ShowRemoveAdsPopup()
        {
            if (hasNoAds || currentPopup != null) return;

            // Buscar canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            // Crear popup
            currentPopup = CreateRemoveAdsPopup(canvas.transform);
            Debug.Log("[AdManager] Popup de quitar anuncios mostrado");
        }

        /// <summary>
        /// Crea el popup con estilo neon
        /// </summary>
        private GameObject CreateRemoveAdsPopup(Transform parent)
        {
            // Colores neon
            Color neonCyan = new Color(0f, 0.9608f, 1f, 1f);
            Color neonGold = new Color(1f, 0.84f, 0f, 1f);
            Color darkBg = new Color(0.02f, 0.05f, 0.1f, 0.95f);

            // === CONTENEDOR PRINCIPAL ===
            GameObject container = new GameObject("RemoveAdsPopup");
            container.transform.SetParent(parent, false);

            RectTransform containerRt = container.AddComponent<RectTransform>();
            containerRt.anchorMin = Vector2.zero;
            containerRt.anchorMax = Vector2.one;
            containerRt.offsetMin = Vector2.zero;
            containerRt.offsetMax = Vector2.zero;

            // === BLOCKER ===
            GameObject blocker = new GameObject("Blocker");
            blocker.transform.SetParent(container.transform, false);

            RectTransform blockerRt = blocker.AddComponent<RectTransform>();
            blockerRt.anchorMin = Vector2.zero;
            blockerRt.anchorMax = Vector2.one;
            blockerRt.offsetMin = Vector2.zero;
            blockerRt.offsetMax = Vector2.zero;

            Image blockerImg = blocker.AddComponent<Image>();
            blockerImg.color = new Color(0f, 0f, 0f, 0.8f);

            // === PANEL PRINCIPAL ===
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(container.transform, false);

            RectTransform panelRt = panel.AddComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.pivot = new Vector2(0.5f, 0.5f);
            panelRt.sizeDelta = new Vector2(600, 350);

            Image panelBg = panel.AddComponent<Image>();
            panelBg.color = darkBg;

            Outline panelOutline = panel.AddComponent<Outline>();
            panelOutline.effectColor = neonCyan;
            panelOutline.effectDistance = new Vector2(3, 3);

            // Layout
            VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(30, 30, 30, 30);
            vlg.spacing = 20;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // === TITULO ===
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panel.transform, false);

            LayoutElement titleLe = titleObj.AddComponent<LayoutElement>();
            titleLe.preferredHeight = 50;

            TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.text = AutoLocalizer.Get("tired_of_ads");
            titleTmp.fontSize = 32;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = neonCyan;
            titleTmp.alignment = TextAlignmentOptions.Center;

            // === DESCRIPCION ===
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(panel.transform, false);

            LayoutElement descLe = descObj.AddComponent<LayoutElement>();
            descLe.preferredHeight = 40;

            TextMeshProUGUI descTmp = descObj.AddComponent<TextMeshProUGUI>();
            descTmp.text = AutoLocalizer.Get("remove_ads_now");
            descTmp.fontSize = 20;
            descTmp.color = new Color(0.8f, 0.8f, 0.85f, 1f);
            descTmp.alignment = TextAlignmentOptions.Center;

            // === BOTONES CONTAINER ===
            GameObject buttonsContainer = new GameObject("Buttons");
            buttonsContainer.transform.SetParent(panel.transform, false);

            LayoutElement buttonsLe = buttonsContainer.AddComponent<LayoutElement>();
            buttonsLe.preferredHeight = 60;

            HorizontalLayoutGroup hlg = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // === BOTON QUITAR ANUNCIOS ===
            GameObject removeBtn = CreatePopupButton(
                buttonsContainer.transform,
                AutoLocalizer.Get("no_ads_title") + " - " + PremiumManager.PRICE_REMOVE_ADS,
                neonGold,
                () => {
                    CloseRemoveAdsPopup();
                    PremiumManager.Instance?.PurchaseRemoveAds();
                }
            );

            // === BOTON NO GRACIAS ===
            GameObject noThanksBtn = CreatePopupButton(
                buttonsContainer.transform,
                AutoLocalizer.Get("no_thanks"),
                new Color(0.3f, 0.3f, 0.35f, 1f),
                CloseRemoveAdsPopup
            );

            // === TEXTO PREMIUM ===
            GameObject premiumObj = new GameObject("PremiumHint");
            premiumObj.transform.SetParent(panel.transform, false);

            LayoutElement premiumLe = premiumObj.AddComponent<LayoutElement>();
            premiumLe.preferredHeight = 30;

            TextMeshProUGUI premiumTmp = premiumObj.AddComponent<TextMeshProUGUI>();
            premiumTmp.text = $"o {AutoLocalizer.Get("get_premium")} - {PremiumManager.PRICE_PREMIUM_FULL}";
            premiumTmp.fontSize = 16;
            premiumTmp.color = neonGold;
            premiumTmp.fontStyle = FontStyles.Underline;
            premiumTmp.alignment = TextAlignmentOptions.Center;

            // Hacer clickeable
            Button premiumBtn = premiumObj.AddComponent<Button>();
            premiumBtn.onClick.AddListener(() => {
                CloseRemoveAdsPopup();
                PremiumManager.Instance?.PurchasePremiumFull();
            });

            return container;
        }

        /// <summary>
        /// Crea un botón para el popup
        /// </summary>
        private GameObject CreatePopupButton(Transform parent, string text, Color bgColor, Action onClick)
        {
            GameObject btnObj = new GameObject("Button");
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(250, 55);

            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.preferredWidth = 250;
            le.preferredHeight = 55;

            Image bg = btnObj.AddComponent<Image>();
            bg.color = bgColor;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(() => onClick?.Invoke());

            // Texto
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(10, 5);
            textRt.offsetMax = new Vector2(-10, -5);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 18;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = bgColor.grayscale > 0.5f ? Color.black : Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            return btnObj;
        }

        /// <summary>
        /// Cierra el popup de quitar anuncios
        /// </summary>
        public void CloseRemoveAdsPopup()
        {
            if (currentPopup != null)
            {
                Destroy(currentPopup);
                currentPopup = null;
                Debug.Log("[AdManager] Popup cerrado");
            }
        }

        #endregion
    }
}
