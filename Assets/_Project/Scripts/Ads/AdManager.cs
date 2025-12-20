using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DigitPark.Managers;
using DigitPark.Localization;
using UnityEngine.Advertisements;

namespace DigitPark.Ads
{
    /// <summary>
    /// Manager principal para Unity Ads
    /// Maneja Interstitial, Rewarded y Banner ads
    /// </summary>
    public class AdManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
    {
        private static AdManager _instance;
        public static AdManager Instance => _instance;

        [Header("Configuracion")]
        [SerializeField] private AdConfig adConfig;

        // Estado
        private bool isInitialized = false;
        private int gamesPlayedSinceLastAd = 0;
        private bool hasNoAds = false;
        private int interstitialsShownCount = 0;

        // Estado de carga de anuncios
        private bool isInterstitialLoaded = false;
        private bool isRewardedLoaded = false;

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
                    Debug.Log("[AdManager] Usuario premium - Anuncios desactivados");
                }
            }
        }

        private void OnDestroy()
        {
            PremiumManager.OnPremiumStatusChanged -= OnPremiumStatusChanged;
        }

        #region Initialization

        public void InitializeAds()
        {
            if (isInitialized)
            {
                Debug.LogWarning("[AdManager] Ya esta inicializado");
                return;
            }

            if (adConfig == null)
            {
                Debug.LogError("[AdManager] AdConfig no esta asignado");
                return;
            }

            Debug.Log($"[AdManager] Inicializando Unity Ads con Game ID: {adConfig.GameId}");
            Advertisement.Initialize(adConfig.GameId, adConfig.testMode, this);
        }

        public void OnInitializationComplete()
        {
            Debug.Log("[AdManager] Unity Ads inicializado correctamente");
            isInitialized = true;

            // Precargar anuncios
            LoadInterstitial();
            LoadRewarded();

            if (adConfig.showBannerOnStart && !hasNoAds)
            {
                ShowBanner();
            }
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            Debug.LogError($"[AdManager] Error inicializando Unity Ads: {error} - {message}");
        }

        #endregion

        #region Banner Ads

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

            Debug.Log("[AdManager] Mostrando banner...");
            Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
            Advertisement.Banner.Show(adConfig.BannerId);
        }

        public void HideBanner()
        {
            Advertisement.Banner.Hide();
            Debug.Log("[AdManager] Banner ocultado");
        }

        public void DestroyBanner()
        {
            Advertisement.Banner.Hide();
            Debug.Log("[AdManager] Banner destruido");
        }

        #endregion

        #region Interstitial Ads

        public void LoadInterstitial()
        {
            if (!isInitialized || hasNoAds) return;

            Debug.Log("[AdManager] Cargando interstitial...");
            Advertisement.Load(adConfig.InterstitialId, this);
        }

        public bool ShowInterstitial()
        {
            if (hasNoAds)
            {
                Debug.Log("[AdManager] Usuario premium - No se muestra interstitial");
                return false;
            }

            if (!isInitialized)
            {
                Debug.LogWarning("[AdManager] No inicializado");
                return false;
            }

            if (isInterstitialLoaded)
            {
                Debug.Log("[AdManager] Mostrando interstitial");
                Advertisement.Show(adConfig.InterstitialId, this);
                isInterstitialLoaded = false;
                return true;
            }
            else
            {
                Debug.Log("[AdManager] Interstitial no esta listo, cargando...");
                LoadInterstitial();
                return false;
            }
        }

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

        public void LoadRewarded()
        {
            if (!isInitialized) return;

            Debug.Log("[AdManager] Cargando rewarded...");
            Advertisement.Load(adConfig.RewardedId, this);
        }

        public bool IsRewardedReady()
        {
            return isRewardedLoaded;
        }

        public void ShowRewarded(Action onRewardEarned = null)
        {
            if (!isInitialized)
            {
                Debug.LogWarning("[AdManager] No inicializado");
                return;
            }

            if (isRewardedLoaded)
            {
                Debug.Log("[AdManager] Mostrando rewarded");
                pendingRewardCallback = onRewardEarned;
                Advertisement.Show(adConfig.RewardedId, this);
                isRewardedLoaded = false;
            }
            else
            {
                Debug.LogWarning("[AdManager] Rewarded no esta listo");
                LoadRewarded();
            }
        }

        #endregion

        #region Unity Ads Callbacks

        public void OnUnityAdsAdLoaded(string placementId)
        {
            Debug.Log($"[AdManager] Anuncio cargado: {placementId}");

            if (placementId == adConfig.InterstitialId)
            {
                isInterstitialLoaded = true;
            }
            else if (placementId == adConfig.RewardedId)
            {
                isRewardedLoaded = true;
            }
        }

        public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
        {
            Debug.LogError($"[AdManager] Error cargando {placementId}: {error} - {message}");

            // Reintentar despues de un delay
            if (placementId == adConfig.InterstitialId)
            {
                Invoke(nameof(LoadInterstitial), 5f);
            }
            else if (placementId == adConfig.RewardedId)
            {
                Invoke(nameof(LoadRewarded), 5f);
            }
        }

        public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
        {
            Debug.Log($"[AdManager] Anuncio completado: {placementId} - Estado: {showCompletionState}");

            if (placementId == adConfig.InterstitialId)
            {
                OnInterstitialClosed?.Invoke();
                TryShowRemoveAdsPopup();
                LoadInterstitial();
            }
            else if (placementId == adConfig.RewardedId)
            {
                if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
                {
                    Debug.Log("[AdManager] Reward ganado!");
                    pendingRewardCallback?.Invoke();
                    OnRewardedAdCompleted?.Invoke();
                }
                pendingRewardCallback = null;
                LoadRewarded();
            }
        }

        public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
        {
            Debug.LogError($"[AdManager] Error mostrando {placementId}: {error} - {message}");

            if (placementId == adConfig.InterstitialId)
            {
                LoadInterstitial();
            }
            else if (placementId == adConfig.RewardedId)
            {
                pendingRewardCallback = null;
                LoadRewarded();
            }
        }

        public void OnUnityAdsShowStart(string placementId)
        {
            Debug.Log($"[AdManager] Anuncio iniciado: {placementId}");
        }

        public void OnUnityAdsShowClick(string placementId)
        {
            Debug.Log($"[AdManager] Anuncio clickeado: {placementId}");
        }

        #endregion

        #region Public Helpers

        public void DisableAds()
        {
            hasNoAds = true;
            HideBanner();
            Debug.Log("[AdManager] Anuncios desactivados permanentemente");
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
            Debug.Log("[AdManager] Popup de quitar anuncios mostrado");
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

            Image blockerImg = blocker.AddComponent<Image>();
            blockerImg.color = new Color(0f, 0f, 0f, 0.8f);

            // Panel
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

            VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
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
            LayoutElement titleLe = titleObj.AddComponent<LayoutElement>();
            titleLe.preferredHeight = 50;
            TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.text = AutoLocalizer.Get("tired_of_ads");
            titleTmp.fontSize = 32;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = neonCyan;
            titleTmp.alignment = TextAlignmentOptions.Center;

            // Descripcion
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(panel.transform, false);
            LayoutElement descLe = descObj.AddComponent<LayoutElement>();
            descLe.preferredHeight = 40;
            TextMeshProUGUI descTmp = descObj.AddComponent<TextMeshProUGUI>();
            descTmp.text = AutoLocalizer.Get("remove_ads_now");
            descTmp.fontSize = 20;
            descTmp.color = new Color(0.8f, 0.8f, 0.85f, 1f);
            descTmp.alignment = TextAlignmentOptions.Center;

            // Botones
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
            LayoutElement premiumLe = premiumObj.AddComponent<LayoutElement>();
            premiumLe.preferredHeight = 30;
            TextMeshProUGUI premiumTmp = premiumObj.AddComponent<TextMeshProUGUI>();
            premiumTmp.text = $"o {AutoLocalizer.Get("get_premium")} - {PremiumManager.PRICE_PREMIUM_FULL}";
            premiumTmp.fontSize = 16;
            premiumTmp.color = neonGold;
            premiumTmp.fontStyle = FontStyles.Underline;
            premiumTmp.alignment = TextAlignmentOptions.Center;

            Button premiumBtn = premiumObj.AddComponent<Button>();
            premiumBtn.onClick.AddListener(() => { CloseRemoveAdsPopup(); PremiumManager.Instance?.PurchasePremiumFull(); });

            return container;
        }

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
