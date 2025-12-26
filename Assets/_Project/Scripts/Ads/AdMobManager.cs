using UnityEngine;
using System;
using DigitPark.Managers;
using DigitPark.Localization;
using GoogleMobileAds.Api;

namespace DigitPark.Ads
{
    /// <summary>
    /// Manager principal para Google AdMob
    /// Maneja Banner, Interstitial y Rewarded ads
    /// </summary>
    public class AdMobManager : MonoBehaviour
    {
        private static AdMobManager _instance;
        public static AdMobManager Instance => _instance;

        [Header("Configuracion")]
        [SerializeField] private AdMobConfig config;

        // Estado
        private bool isInitialized = false;
        private int gamesPlayedSinceLastAd = 0;
        private bool hasNoAds = false;
        private int interstitialsShownCount = 0;

        // AdMob Ads
        private BannerView bannerView;
        private InterstitialAd interstitialAd;
        private RewardedAd rewardedAd;

        private bool isInterstitialLoaded = false;
        private bool isRewardedLoaded = false;
        private bool isBannerShowing = false;

        [Header("Post-Ad Popup")]
        [SerializeField] private bool showRemoveAdsPopup = true;
        [SerializeField] private int showPopupAfterAds = 2;
        private GameObject currentPopup;

        // Eventos
        public event Action OnRewardedAdCompleted;
        public event Action OnInterstitialClosed;

        // Callback temporal para rewarded
        private Action pendingRewardCallback;

        public bool IsInitialized => isInitialized;
        public bool HasNoAds => hasNoAds;
        // isInterstitialLoaded y isRewardedLoaded se usan en IsRewardedReady() y para tracking interno
        public bool InterstitialLoadedFlag => isInterstitialLoaded;
        public bool RewardedLoadedFlag => isRewardedLoaded;

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
            CheckPremiumStatus();

            if (!hasNoAds)
            {
                InitializeAds();
            }
        }

        private void CheckPremiumStatus()
        {
            if (PremiumManager.Instance != null)
            {
                hasNoAds = PremiumManager.Instance.HasNoAds;
            }

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
                    Debug.Log("[AdMob] Usuario premium - Anuncios desactivados");
                }
            }
        }

        private void OnDestroy()
        {
            PremiumManager.OnPremiumStatusChanged -= OnPremiumStatusChanged;
            DestroyAds();
        }

        #region Initialization

        public void InitializeAds()
        {
            if (isInitialized)
            {
                Debug.LogWarning("[AdMob] Ya esta inicializado");
                return;
            }

            if (config == null)
            {
                Debug.LogError("[AdMob] AdMobConfig no esta asignado. Crea uno en: Create > DigitPark > Ads > AdMob Config");
                return;
            }

            Debug.Log($"[AdMob] Inicializando con App ID: {config.AppId}");

            MobileAds.Initialize(initStatus =>
            {
                Debug.Log("[AdMob] SDK inicializado correctamente");
                isInitialized = true;

                // Precargar anuncios
                LoadInterstitial();
                LoadRewarded();

                if (config.showBannerOnStart && !hasNoAds)
                {
                    ShowBanner();
                }
            });
        }

        private void DestroyAds()
        {
            bannerView?.Destroy();
            interstitialAd?.Destroy();
            rewardedAd?.Destroy();
        }

        #endregion

        #region Banner Ads

        public void ShowBanner()
        {
            if (hasNoAds)
            {
                Debug.Log("[AdMob] Usuario premium - No se muestra banner");
                return;
            }

            if (!isInitialized)
            {
                Debug.LogWarning("[AdMob] No inicializado");
                return;
            }

            if (isBannerShowing) return;

            Debug.Log("[AdMob] Mostrando banner...");

            // Crear banner
            AdSize adSize = AdSize.Banner;
            bannerView = new BannerView(config.BannerId, adSize, GetBannerPosition());

            // Escuchar eventos
            bannerView.OnBannerAdLoaded += () => {
                Debug.Log("[AdMob] Banner cargado");
                isBannerShowing = true;
            };
            bannerView.OnBannerAdLoadFailed += (LoadAdError error) => {
                Debug.LogError($"[AdMob] Error cargando banner: {error.GetMessage()}");
            };

            // Cargar anuncio
            AdRequest request = new AdRequest();
            bannerView.LoadAd(request);
        }

        public void HideBanner()
        {
            if (!isBannerShowing) return;

            Debug.Log("[AdMob] Ocultando banner");
            bannerView?.Hide();
            isBannerShowing = false;
        }

        public void DestroyBanner()
        {
            bannerView?.Destroy();
            bannerView = null;
            isBannerShowing = false;
            Debug.Log("[AdMob] Banner destruido");
        }

        private AdPosition GetBannerPosition()
        {
            return config.bannerPosition switch
            {
                BannerPosition.Top => AdPosition.Top,
                BannerPosition.Bottom => AdPosition.Bottom,
                BannerPosition.TopLeft => AdPosition.TopLeft,
                BannerPosition.TopRight => AdPosition.TopRight,
                BannerPosition.BottomLeft => AdPosition.BottomLeft,
                BannerPosition.BottomRight => AdPosition.BottomRight,
                _ => AdPosition.Bottom
            };
        }

        #endregion

        #region Interstitial Ads

        public void LoadInterstitial()
        {
            if (!isInitialized || hasNoAds) return;

            Debug.Log("[AdMob] Cargando interstitial...");

            // Limpiar anterior si existe
            interstitialAd?.Destroy();

            AdRequest request = new AdRequest();
            InterstitialAd.Load(config.InterstitialId, request, (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null)
                {
                    Debug.LogError($"[AdMob] Error cargando interstitial: {error.GetMessage()}");
                    isInterstitialLoaded = false;
                    // Reintentar en 30 segundos
                    Invoke(nameof(LoadInterstitial), 30f);
                    return;
                }

                Debug.Log("[AdMob] Interstitial cargado");
                interstitialAd = ad;
                isInterstitialLoaded = true;

                // Configurar eventos
                interstitialAd.OnAdFullScreenContentClosed += () =>
                {
                    Debug.Log("[AdMob] Interstitial cerrado");
                    OnInterstitialClosed?.Invoke();
                    TryShowRemoveAdsPopup();
                    LoadInterstitial(); // Recargar para la proxima vez
                };

                interstitialAd.OnAdFullScreenContentFailed += (AdError adError) =>
                {
                    Debug.LogError($"[AdMob] Error mostrando interstitial: {adError.GetMessage()}");
                    LoadInterstitial();
                };
            });
        }

        public bool ShowInterstitial()
        {
            if (hasNoAds)
            {
                Debug.Log("[AdMob] Usuario premium - No se muestra interstitial");
                return false;
            }

            if (!isInitialized)
            {
                Debug.LogWarning("[AdMob] No inicializado");
                return false;
            }

            if (interstitialAd != null && interstitialAd.CanShowAd())
            {
                Debug.Log("[AdMob] Mostrando interstitial");
                interstitialAd.Show();
                isInterstitialLoaded = false;
                return true;
            }

            Debug.Log("[AdMob] Interstitial no esta listo, cargando...");
            LoadInterstitial();
            return false;
        }

        public void TryShowInterstitialAfterGame()
        {
            if (hasNoAds) return;

            gamesPlayedSinceLastAd++;

            if (gamesPlayedSinceLastAd >= config.interstitialFrequency)
            {
                if (ShowInterstitial())
                {
                    gamesPlayedSinceLastAd = 0;
                }
            }
        }

        #endregion

        #region Rewarded Ads

        public void LoadRewarded()
        {
            if (!isInitialized) return;

            Debug.Log("[AdMob] Cargando rewarded...");

            // Limpiar anterior si existe
            rewardedAd?.Destroy();

            AdRequest request = new AdRequest();
            RewardedAd.Load(config.RewardedId, request, (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null)
                {
                    Debug.LogError($"[AdMob] Error cargando rewarded: {error.GetMessage()}");
                    isRewardedLoaded = false;
                    // Reintentar en 30 segundos
                    Invoke(nameof(LoadRewarded), 30f);
                    return;
                }

                Debug.Log("[AdMob] Rewarded cargado");
                rewardedAd = ad;
                isRewardedLoaded = true;

                // Configurar eventos
                rewardedAd.OnAdFullScreenContentClosed += () =>
                {
                    Debug.Log("[AdMob] Rewarded cerrado");
                    LoadRewarded(); // Recargar para la proxima vez
                };

                rewardedAd.OnAdFullScreenContentFailed += (AdError adError) =>
                {
                    Debug.LogError($"[AdMob] Error mostrando rewarded: {adError.GetMessage()}");
                    pendingRewardCallback = null;
                    LoadRewarded();
                };
            });
        }

        public bool IsRewardedReady()
        {
            return rewardedAd != null && rewardedAd.CanShowAd();
        }

        public void ShowRewarded(Action onRewardEarned = null)
        {
            if (!isInitialized)
            {
                Debug.LogWarning("[AdMob] No inicializado");
                return;
            }

            if (rewardedAd != null && rewardedAd.CanShowAd())
            {
                Debug.Log("[AdMob] Mostrando rewarded");
                pendingRewardCallback = onRewardEarned;

                rewardedAd.Show((Reward reward) =>
                {
                    Debug.Log($"[AdMob] Reward ganado! Tipo: {reward.Type}, Cantidad: {reward.Amount}");
                    pendingRewardCallback?.Invoke();
                    OnRewardedAdCompleted?.Invoke();
                    pendingRewardCallback = null;
                });

                isRewardedLoaded = false;
                return;
            }

            Debug.LogWarning("[AdMob] Rewarded no esta listo");
            LoadRewarded();
        }

        #endregion

        #region Public Helpers

        public void DisableAds()
        {
            hasNoAds = true;
            HideBanner();
            Debug.Log("[AdMob] Anuncios desactivados permanentemente");
        }

        #endregion

        #region Remove Ads Popup

        private void TryShowRemoveAdsPopup()
        {
            if (!showRemoveAdsPopup || hasNoAds) return;

            interstitialsShownCount++;

            if (interstitialsShownCount >= showPopupAfterAds)
            {
                interstitialsShownCount = 0;
                ShowRemoveAdsPopup();
            }
        }

        public void ShowRemoveAdsPopup()
        {
            if (hasNoAds || currentPopup != null) return;

            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            currentPopup = CreateRemoveAdsPopup(canvas.transform);
            Debug.Log("[AdMob] Popup de quitar anuncios mostrado");
        }

        private GameObject CreateRemoveAdsPopup(Transform parent)
        {
            Color neonCyan = new Color(0f, 0.9608f, 1f, 1f);
            Color neonGold = new Color(1f, 0.84f, 0f, 1f);
            Color darkBg = new Color(0.02f, 0.05f, 0.1f, 0.95f);

            GameObject container = new GameObject("RemoveAdsPopup");
            container.transform.SetParent(parent, false);

            RectTransform containerRt = container.AddComponent<RectTransform>();
            containerRt.anchorMin = Vector2.zero;
            containerRt.anchorMax = Vector2.one;
            containerRt.offsetMin = Vector2.zero;
            containerRt.offsetMax = Vector2.zero;

            // Blocker
            GameObject blocker = new GameObject("Blocker");
            blocker.transform.SetParent(container.transform, false);

            RectTransform blockerRt = blocker.AddComponent<RectTransform>();
            blockerRt.anchorMin = Vector2.zero;
            blockerRt.anchorMax = Vector2.one;
            blockerRt.offsetMin = Vector2.zero;
            blockerRt.offsetMax = Vector2.zero;

            UnityEngine.UI.Image blockerImg = blocker.AddComponent<UnityEngine.UI.Image>();
            blockerImg.color = new Color(0f, 0f, 0f, 0.8f);

            // Panel
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(container.transform, false);

            RectTransform panelRt = panel.AddComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.pivot = new Vector2(0.5f, 0.5f);
            panelRt.sizeDelta = new Vector2(600, 350);

            UnityEngine.UI.Image panelBg = panel.AddComponent<UnityEngine.UI.Image>();
            panelBg.color = darkBg;

            UnityEngine.UI.Outline panelOutline = panel.AddComponent<UnityEngine.UI.Outline>();
            panelOutline.effectColor = neonCyan;
            panelOutline.effectDistance = new Vector2(3, 3);

            UnityEngine.UI.VerticalLayoutGroup vlg = panel.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            vlg.padding = new RectOffset(30, 30, 30, 30);
            vlg.spacing = 20;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Titulo
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panel.transform, false);
            UnityEngine.UI.LayoutElement titleLe = titleObj.AddComponent<UnityEngine.UI.LayoutElement>();
            titleLe.preferredHeight = 50;
            TMPro.TextMeshProUGUI titleTmp = titleObj.AddComponent<TMPro.TextMeshProUGUI>();
            titleTmp.text = AutoLocalizer.Get("tired_of_ads");
            titleTmp.fontSize = 32;
            titleTmp.fontStyle = TMPro.FontStyles.Bold;
            titleTmp.color = neonCyan;
            titleTmp.alignment = TMPro.TextAlignmentOptions.Center;

            // Descripcion
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(panel.transform, false);
            UnityEngine.UI.LayoutElement descLe = descObj.AddComponent<UnityEngine.UI.LayoutElement>();
            descLe.preferredHeight = 40;
            TMPro.TextMeshProUGUI descTmp = descObj.AddComponent<TMPro.TextMeshProUGUI>();
            descTmp.text = AutoLocalizer.Get("remove_ads_now");
            descTmp.fontSize = 20;
            descTmp.color = new Color(0.8f, 0.8f, 0.85f, 1f);
            descTmp.alignment = TMPro.TextAlignmentOptions.Center;

            // Botones
            GameObject buttonsContainer = new GameObject("Buttons");
            buttonsContainer.transform.SetParent(panel.transform, false);
            UnityEngine.UI.LayoutElement buttonsLe = buttonsContainer.AddComponent<UnityEngine.UI.LayoutElement>();
            buttonsLe.preferredHeight = 60;
            UnityEngine.UI.HorizontalLayoutGroup hlg = buttonsContainer.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            CreatePopupButton(buttonsContainer.transform,
                AutoLocalizer.Get("no_ads_title") + " - " + PremiumManager.PRICE_REMOVE_ADS,
                neonGold,
                () => { CloseRemoveAdsPopup(); PremiumManager.Instance?.PurchaseRemoveAds(); });

            CreatePopupButton(buttonsContainer.transform,
                AutoLocalizer.Get("no_thanks"),
                new Color(0.3f, 0.3f, 0.35f, 1f),
                CloseRemoveAdsPopup);

            // Premium hint
            GameObject premiumObj = new GameObject("PremiumHint");
            premiumObj.transform.SetParent(panel.transform, false);
            UnityEngine.UI.LayoutElement premiumLe = premiumObj.AddComponent<UnityEngine.UI.LayoutElement>();
            premiumLe.preferredHeight = 30;
            TMPro.TextMeshProUGUI premiumTmp = premiumObj.AddComponent<TMPro.TextMeshProUGUI>();
            premiumTmp.text = $"o {AutoLocalizer.Get("get_premium")} - {PremiumManager.PRICE_PREMIUM_FULL}";
            premiumTmp.fontSize = 16;
            premiumTmp.color = neonGold;
            premiumTmp.fontStyle = TMPro.FontStyles.Underline;
            premiumTmp.alignment = TMPro.TextAlignmentOptions.Center;

            UnityEngine.UI.Button premiumBtn = premiumObj.AddComponent<UnityEngine.UI.Button>();
            premiumBtn.onClick.AddListener(() => { CloseRemoveAdsPopup(); PremiumManager.Instance?.PurchasePremiumFull(); });

            return container;
        }

        private GameObject CreatePopupButton(Transform parent, string text, Color bgColor, Action onClick)
        {
            GameObject btnObj = new GameObject("Button");
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(250, 55);

            UnityEngine.UI.LayoutElement le = btnObj.AddComponent<UnityEngine.UI.LayoutElement>();
            le.preferredWidth = 250;
            le.preferredHeight = 55;

            UnityEngine.UI.Image bg = btnObj.AddComponent<UnityEngine.UI.Image>();
            bg.color = bgColor;

            UnityEngine.UI.Button btn = btnObj.AddComponent<UnityEngine.UI.Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(() => onClick?.Invoke());

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(10, 5);
            textRt.offsetMax = new Vector2(-10, -5);

            TMPro.TextMeshProUGUI tmp = textObj.AddComponent<TMPro.TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 18;
            tmp.fontStyle = TMPro.FontStyles.Bold;
            tmp.color = bgColor.grayscale > 0.5f ? Color.black : Color.white;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;

            return btnObj;
        }

        public void CloseRemoveAdsPopup()
        {
            if (currentPopup != null)
            {
                Destroy(currentPopup);
                currentPopup = null;
                Debug.Log("[AdMob] Popup cerrado");
            }
        }

        #endregion
    }
}
