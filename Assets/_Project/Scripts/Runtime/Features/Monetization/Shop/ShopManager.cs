using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using DigitPark.Animations;
using System;
using System.Collections.Generic;
using DigitPark.Managers;
using DigitPark.Localization;
using DigitPark.Navigation;
using DigitPark.Payments;

namespace DigitPark.Monetization
{
    /// <summary>
    /// Manager principal de la escena Shop V4.
    /// Scroll continuo estilo Clash Royale — todas las secciones visibles.
    /// </summary>
    public class ShopManager : MonoBehaviour
    {
        [Header("Scroll View")]
        [SerializeField] private GameObject _shopScrollView;

        [Header("Popups")]
        [SerializeField] private GameObject _purchasePopup;
        [SerializeField] private GameObject _notEnoughGemsPopup;

        [Header("Popup UI References")]
        [SerializeField] private Image _popupItemIcon;
        [SerializeField] private TextMeshProUGUI _popupItemName;
        [SerializeField] private TextMeshProUGUI _popupItemPrice;
        [SerializeField] private Button _popupConfirmButton;
        [SerializeField] private Button _popupCancelButton;
        [SerializeField] private Button _notEnoughCloseButton;
        [SerializeField] private Button _notEnoughGetGemsButton;

        [Header("Navigation")]
        [SerializeField] private Button _backButton;

        [Header("Entrance Animation")]
        [SerializeField] private RectTransform _headerTransform;
        [SerializeField] private RectTransform _scrollViewTransform;

        [Header("Currency Display")]
        [SerializeField] private CurrencyDisplayUI _gemsDisplay;
        [SerializeField] private CurrencyDisplayUI _coinsDisplay;
        [SerializeField] private TextMeshProUGUI _headerGemsText;
        [SerializeField] private TextMeshProUGUI _headerCoinsText;

        [Header("Shop Items")]
        [SerializeField] private List<ShopItemUI> _shopItems = new List<ShopItemUI>();

        private ShopItemUI _currentPurchaseItem;
        private ScrollRect _scrollRect;

        // Events
        public event Action<ShopTab> OnTabChanged;
        public event Action<string> OnItemPurchased;
        public event Action OnShopClosed;

        private void Awake()
        {
            FindShopItems();
            CacheScrollRect();
        }

        private void Start()
        {
            SetupButtons();
            SetupPopups();
            SetupCurrencyListeners();
            RefreshCurrencyDisplay();
            ForceLayoutRebuild();
            HandleNavigationParams();
            AnimateEntrance();
        }

        private void OnDestroy()
        {
            DOTween.Kill(gameObject);
            if (_headerGemsText != null) DOTween.Kill(_headerGemsText.gameObject);
            if (_headerCoinsText != null) DOTween.Kill(_headerCoinsText.gameObject);
            RemoveCurrencyListeners();
        }

        private void CacheScrollRect()
        {
            if (_shopScrollView != null)
                _scrollRect = _shopScrollView.GetComponent<ScrollRect>();
        }

        private void FindShopItems()
        {
            _shopItems.Clear();
            _shopItems.AddRange(FindObjectsOfType<ShopItemUI>());

            foreach (var item in _shopItems)
            {
                item.OnPurchaseRequested += OnItemPurchaseRequested;
            }

            Debug.Log($"[ShopManager] Found {_shopItems.Count} shop items");
        }

        // ==================== SETUP ====================

        private void SetupPopups()
        {
            if (_popupConfirmButton != null)
                _popupConfirmButton.onClick.AddListener(ConfirmPurchase);
            if (_popupCancelButton != null)
                _popupCancelButton.onClick.AddListener(CancelPurchase);

            if (_notEnoughCloseButton != null)
                _notEnoughCloseButton.onClick.AddListener(HideNotEnoughGemsPopup);
            if (_notEnoughGetGemsButton != null)
                _notEnoughGetGemsButton.onClick.AddListener(OnGetGemsClicked);

            HidePurchasePopup();
            HideNotEnoughGemsPopup();
        }

        private void SetupCurrencyListeners()
        {
            var currency = CurrencyManager.Instance;
            if (currency != null)
            {
                currency.OnGemsChanged += OnGemsChanged;
                currency.OnCoinsChanged += OnCoinsChanged;
                currency.OnNotEnoughGems += OnNotEnoughGems;
            }
        }

        private void RemoveCurrencyListeners()
        {
            var currency = CurrencyManager.Instance;
            if (currency != null)
            {
                currency.OnGemsChanged -= OnGemsChanged;
                currency.OnCoinsChanged -= OnCoinsChanged;
                currency.OnNotEnoughGems -= OnNotEnoughGems;
            }

            foreach (var item in _shopItems)
            {
                if (item != null)
                    item.OnPurchaseRequested -= OnItemPurchaseRequested;
            }
        }

        private void OnGemsChanged(int newAmount, int delta)
        {
            RefreshCurrencyDisplay();
            if (delta != 0)
                AnimateCurrencyChange(_gemsDisplay, _headerGemsText, delta > 0);
        }

        private void OnCoinsChanged(int newAmount, int delta)
        {
            RefreshCurrencyDisplay();
            if (delta != 0)
                AnimateCurrencyChange(_coinsDisplay, _headerCoinsText, delta > 0);
        }

        private void OnNotEnoughGems(int amountNeeded)
        {
            ShowNotEnoughGemsPopup();
        }

        private void RefreshCurrencyDisplay()
        {
            var currency = CurrencyManager.Instance;
            if (currency == null) return;

            if (_gemsDisplay != null)
                _gemsDisplay.SetAmount(currency.Gems);
            if (_coinsDisplay != null)
                _coinsDisplay.SetAmount(currency.Coins);

            if (_headerGemsText != null)
                _headerGemsText.text = FormatCurrency(currency.Gems);
            if (_headerCoinsText != null)
                _headerCoinsText.text = FormatCurrency(currency.Coins);
        }

        private string FormatCurrency(int amount)
        {
            if (amount >= 1000000)
                return $"{amount / 1000000f:0.#}M";
            else if (amount >= 10000)
                return $"{amount / 1000f:0.#}K";
            else
                return amount.ToString("N0");
        }

        private void SetupButtons()
        {
            var autoNav = _backButton?.GetComponent<DigitPark.UI.BackButton>();
            if (autoNav != null) autoNav.DisableAutoNavigation();
            if (_backButton != null)
                _backButton.onClick.AddListener(OnBackButtonClick);
        }

        private void HandleNavigationParams()
        {
            var navigator = SceneNavigator.Instance;
            var navParams = navigator.ConsumeParams();

            if (navParams != null)
            {
                if (!string.IsNullOrEmpty(navParams.TargetTab))
                {
                    if (Enum.TryParse<ShopTab>(navParams.TargetTab, out ShopTab targetTab))
                    {
                        ScrollToSection(targetTab);
                    }
                }

                if (navParams.ShowPopup)
                {
                    ShowNotEnoughGemsPopup();
                }

                if (!string.IsNullOrEmpty(navParams.ItemId))
                {
                    ScrollToItem(navParams.ItemId);
                }

                Debug.Log($"[ShopManager] Handled navigation params - Tab: {navParams.TargetTab}, ShowPopup: {navParams.ShowPopup}");
            }
        }

        /// <summary>
        /// Scroll programatico a una seccion.
        /// Continuous scroll — maps ShopTab to approximate positions.
        /// </summary>
        public void ScrollToSection(ShopTab tab)
        {
            if (_scrollRect == null) return;

            // Normalized position (1 = top, 0 = bottom)
            float pos = 1f;
            switch (tab)
            {
                case ShopTab.Featured: pos = 1f; break;
                case ShopTab.Currency: pos = 0.75f; break;
                case ShopTab.Styles: pos = 0.45f; break;
                case ShopTab.Effects: pos = 0.15f; break;
                case ShopTab.BattleCards: pos = 0f; break;
            }

            _scrollRect.verticalNormalizedPosition = pos;
            OnTabChanged?.Invoke(tab);
            Debug.Log($"[ShopManager] Scrolled to section: {tab}");
        }

        private void ScrollToItem(string itemId)
        {
            Debug.Log($"[ShopManager] Scroll to item: {itemId}");
        }

        private void OnBackButtonClick()
        {
            OnShopClosed?.Invoke();
            SceneNavigator.Instance.GoBack();
        }

        // ==================== ITEM PURCHASE REQUEST ====================

        private void OnItemPurchaseRequested(ShopItemUI item)
        {
            if (item == null || item.ItemData == null) return;

            var itemData = item.ItemData;

            if (itemData.priceType == PriceType.RealMoney)
            {
                ShowPurchasePopup(item);
                return;
            }

            if (!itemData.CanAfford())
            {
                if (itemData.priceType == PriceType.DigitGems)
                {
                    ShowNotEnoughGemsPopup();
                }
                return;
            }

            ShowPurchasePopup(item);
        }

        // ==================== POPUP MANAGEMENT ====================

        public void ShowPurchasePopup(ShopItemUI item)
        {
            if (_purchasePopup == null || item == null) return;

            _currentPurchaseItem = item;
            var itemData = item.ItemData;

            if (_popupItemIcon != null && itemData.icon != null)
            {
                _popupItemIcon.sprite = itemData.icon;
                _popupItemIcon.color = itemData.accentColor;
            }

            if (_popupItemName != null)
            {
                string amountText = "";
                switch (itemData.itemType)
                {
                    case ShopItemType.DigitGemsPack:
                        amountText = $"{itemData.GetTotalGems():N0} {AutoLocalizer.Get("currency_digitgems")}";
                        break;
                    case ShopItemType.DigitCoinsPack:
                        amountText = $"{itemData.GetTotalCoins():N0} {AutoLocalizer.Get("currency_digitcoins")}";
                        break;
                    default:
                        amountText = itemData.displayName;
                        break;
                }
                _popupItemName.text = amountText;
            }

            if (_popupItemPrice != null)
            {
                string pricePrefix = itemData.priceType == PriceType.RealMoney ? AutoLocalizer.Get("shop_price_prefix") : "";
                _popupItemPrice.text = pricePrefix + itemData.GetFormattedPrice();
            }

            _purchasePopup.SetActive(true);
            AnimatePanelIn(_purchasePopup.transform);
            Debug.Log($"[ShopManager] Showing purchase popup for: {itemData.displayName}");
        }

        public void ShowPurchasePopup(string itemId, string itemName, string price)
        {
            if (_purchasePopup != null)
            {
                if (_popupItemName != null) _popupItemName.text = itemName;
                if (_popupItemPrice != null) _popupItemPrice.text = price;
                _purchasePopup.SetActive(true);
                AnimatePanelIn(_purchasePopup.transform);
            }
        }

        public void HidePurchasePopup()
        {
            if (_purchasePopup != null)
            {
                AnimatePanelOut(_purchasePopup.transform, () =>
                {
                    _purchasePopup.SetActive(false);
                });
            }
            _currentPurchaseItem = null;
        }

        public void ShowNotEnoughGemsPopup()
        {
            if (_notEnoughGemsPopup != null)
            {
                _notEnoughGemsPopup.SetActive(true);
                AnimatePanelIn(_notEnoughGemsPopup.transform);
            }
        }

        public void HideNotEnoughGemsPopup()
        {
            if (_notEnoughGemsPopup != null)
            {
                AnimatePanelOut(_notEnoughGemsPopup.transform, () =>
                {
                    _notEnoughGemsPopup.SetActive(false);
                });
            }
        }

        private void OnGetGemsClicked()
        {
            HideNotEnoughGemsPopup();
            ScrollToSection(ShopTab.Currency);
        }

        // ==================== PURCHASE METHODS ====================

        public void PurchaseItem(string itemId)
        {
            Debug.Log($"[ShopManager] Purchasing item: {itemId}");
            OnItemPurchased?.Invoke(itemId);
        }

        public void ConfirmPurchase()
        {
            if (_currentPurchaseItem == null)
            {
                HidePurchasePopup();
                return;
            }

            var itemData = _currentPurchaseItem.ItemData;

            if (itemData.priceType == PriceType.RealMoney)
            {
                HidePurchasePopup();
                ProcessIAPPurchase(itemData);
            }
            else
            {
                bool success = _currentPurchaseItem.TryPurchase();
                if (success)
                {
                    OnItemPurchased?.Invoke(itemData.itemId);
                    PlayPurchaseCelebration(_currentPurchaseItem);
                    Debug.Log($"[ShopManager] Purchase successful: {itemData.displayName}");
                }
                HidePurchasePopup();
            }
        }

        private async void ProcessIAPPurchase(ShopItemData itemData)
        {
            try
            {
            string productId = itemData.iapProductId;
            Debug.Log($"[ShopManager] Processing payment: {productId}");

            // Usar PaymentManager si está disponible (Stripe + AppleIAP con fallback)
            if (PaymentManager.Instance != null && PaymentManager.Instance.IsInitialized)
            {
                var result = await PaymentManager.Instance.PurchaseCosmetic(productId);

                if (result.Success)
                {
                    itemData.GrantRewards();
                    OnItemPurchased?.Invoke(itemData.itemId);
                    PlayPurchaseCelebration(_currentPurchaseItem);

                    if (result.WasProviderSwitched)
                        Debug.Log($"[ShopManager] Compra exitosa via fallback ({result.ProviderUsed}): {itemData.displayName}");
                    else
                        Debug.Log($"[ShopManager] Compra exitosa via {result.ProviderUsed}: {itemData.displayName}");
                }
                else
                {
                    Debug.LogWarning($"[ShopManager] Compra fallida: {itemData.displayName} — {result.ErrorMessage}");
                }
                return;
            }

            // Fallback legacy: usar PremiumManager directamente si PaymentManager no está listo
            Debug.LogWarning("[ShopManager] PaymentManager no disponible, usando PremiumManager legacy");

            if (PremiumManager.IsGemPackProduct(productId))
            {
                PremiumManager.Instance.PurchaseGemPack(productId, (success) =>
                {
                    if (success)
                    {
                        OnItemPurchased?.Invoke(itemData.itemId);
                        PlayPurchaseCelebration(_currentPurchaseItem);
                        Debug.Log($"[ShopManager] Gem pack IAP completado: {itemData.displayName}");
                    }
                    else
                    {
                        Debug.LogWarning($"[ShopManager] Gem pack IAP fallido: {itemData.displayName}");
                    }
                });
            }
            else
            {
                PremiumManager.Instance.PurchaseByProductId(productId, (success) =>
                {
                    if (success)
                    {
                        itemData.GrantRewards();
                        OnItemPurchased?.Invoke(itemData.itemId);
                        PlayPurchaseCelebration(_currentPurchaseItem);
                        Debug.Log($"[ShopManager] IAP completado: {itemData.displayName}");
                    }
                    else
                    {
                        Debug.LogWarning($"[ShopManager] IAP fallido: {itemData.displayName}");
                    }
                });
            }
            } // end try
            catch (System.Exception ex)
            {
                Debug.LogError($"[ShopManager] ProcessIAPPurchase exception: {ex.Message}");
            }
        }

        public void CancelPurchase()
        {
            HidePurchasePopup();
        }

        // ==================== CURRENCY UPDATE ====================

        public void UpdateCurrencyDisplay(int gems, int coins)
        {
            if (_gemsDisplay != null)
                _gemsDisplay.SetAmount(gems);
            if (_coinsDisplay != null)
                _coinsDisplay.SetAmount(coins);
        }

        public void RegisterShopItem(ShopItemUI item)
        {
            if (item != null && !_shopItems.Contains(item))
            {
                _shopItems.Add(item);
                item.OnPurchaseRequested += OnItemPurchaseRequested;
            }
        }

        public List<ShopItemUI> GetItemsForTab(ShopTab tab)
        {
            var result = new List<ShopItemUI>();
            foreach (var item in _shopItems)
            {
                if (item != null && item.ItemData != null && item.ItemData.shopTab == tab)
                {
                    result.Add(item);
                }
            }
            return result;
        }

        // ==================== ANIMATIONS ====================

        private void AnimateEntrance()
        {
            // Header: slide from top
            if (_headerTransform != null)
            {
                Vector2 pos = _headerTransform.anchoredPosition;
                _headerTransform.anchoredPosition = new Vector2(pos.x, pos.y + 200);
                _headerTransform.DOAnchorPos(pos, 0.4f).SetEase(AnimConstants.ENTER);
            }

            // ScrollView: fade in + staggered items
            if (_scrollViewTransform != null)
            {
                var cg = _scrollViewTransform.GetComponent<CanvasGroup>();
                if (cg == null) cg = _scrollViewTransform.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                DOTween.Sequence()
                    .AppendInterval(0.2f)
                    .Append(cg.DOFade(1f, 0.4f))
                    .OnComplete(() => AnimateScrollItemsEntrance())
                    .SetLink(gameObject);
            }
        }

        private void AnimateScrollItemsEntrance()
        {
            if (_scrollRect == null || _scrollRect.content == null) return;

            var seq = DOTween.Sequence().SetLink(gameObject);
            int i = 0;
            foreach (Transform child in _scrollRect.content)
            {
                if (!child.gameObject.activeSelf) continue;
                var itemCG = child.GetComponent<CanvasGroup>();
                if (itemCG == null) itemCG = child.gameObject.AddComponent<CanvasGroup>();
                itemCG.alpha = 0f;
                child.localScale = Vector3.one * 0.85f;
                float delay = i * 0.04f;
                seq.Insert(delay, itemCG.DOFade(1f, AnimConstants.DURATION_MEDIUM).SetEase(AnimConstants.SMOOTH));
                seq.Insert(delay, child.DOScale(1f, AnimConstants.DURATION_MEDIUM).SetEase(AnimConstants.ENTER));
                i++;
            }
        }

        private void AnimatePanelIn(Transform panel)
        {
            Transform popupInner = panel.childCount > 0 ? panel.GetChild(0) : panel;

            popupInner.localScale = Vector3.one * 0.85f;
            var cg = popupInner.GetComponent<CanvasGroup>();
            if (cg == null) cg = popupInner.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;

            DOTween.Sequence()
                .Join(popupInner.DOScale(1f, UIAnimations.DURATION_NORMAL).SetEase(AnimConstants.ENTER))
                .Join(cg.DOFade(1f, AnimConstants.DURATION_MEDIUM).SetEase(AnimConstants.SMOOTH))
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        private void AnimatePanelOut(Transform panel, Action onComplete)
        {
            Transform popupInner = panel.childCount > 0 ? panel.GetChild(0) : panel;

            var cg = popupInner.GetComponent<CanvasGroup>();
            if (cg == null) cg = popupInner.gameObject.AddComponent<CanvasGroup>();

            DOTween.Sequence()
                .Join(popupInner.DOScale(0.9f, UIAnimations.DURATION_FAST).SetEase(AnimConstants.EXIT))
                .Join(cg.DOFade(0f, UIAnimations.DURATION_FAST).SetEase(AnimConstants.EXIT))
                .OnComplete(() =>
                {
                    if (popupInner != null) popupInner.localScale = Vector3.one;
                    if (cg != null) cg.alpha = 1f;
                    onComplete?.Invoke();
                })
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        private void AnimateCurrencyChange(CurrencyDisplayUI display, TextMeshProUGUI headerText, bool isGain)
        {
            if (display != null)
            {
                display.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 5, 0.5f).SetUpdate(true);
            }

            if (headerText != null)
            {
                Color flashColor = isGain ? new Color(0.3f, 1f, 0.5f, 1f) : new Color(1f, 0.3f, 0.3f, 1f);
                Color originalColor = headerText.color;
                headerText.DOColor(flashColor, 0.15f).SetUpdate(true)
                    .SetLink(headerText.gameObject)
                    .OnComplete(() => headerText.DOColor(originalColor, 0.3f).SetUpdate(true)
                        .SetLink(headerText.gameObject));
            }
        }

        private void PlayPurchaseCelebration(ShopItemUI item)
        {
            if (item == null) return;

            item.transform.DOPunchScale(Vector3.one * 0.2f, 0.4f, 5, 0.5f);

            var particleSpawner = DigitPark.Animations.ParticleEffectSpawner.Instance;
            if (particleSpawner != null)
            {
                particleSpawner.SpawnCenterBurst();
            }

            var uiAnimManager = DigitPark.Animations.UIAnimationManager.Instance;
            if (uiAnimManager != null)
            {
                uiAnimManager.GoldFlash();
            }
        }

        // ==================== LAYOUT ====================

        private void ForceLayoutRebuild()
        {
            if (_shopScrollView != null)
            {
                var scrollContent = _shopScrollView.GetComponent<ScrollRect>();
                if (scrollContent != null && scrollContent.content != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(scrollContent.content);
                }
            }

            foreach (var layout in GetComponentsInChildren<LayoutGroup>(true))
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
            }
        }
    }
}
