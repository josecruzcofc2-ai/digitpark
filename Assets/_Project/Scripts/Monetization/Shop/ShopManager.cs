using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using DG.Tweening;

namespace DigitPark.Monetization
{
    /// <summary>
    /// Manager principal de la escena Shop.
    /// Maneja tabs, items, compras y navegacion.
    /// </summary>
    public class ShopManager : MonoBehaviour
    {
        [Header("Tab References")]
        [SerializeField] private Button _featuredTabButton;
        [SerializeField] private Button _gemsTabButton;
        [SerializeField] private Button _coinsTabButton;
        [SerializeField] private Button _themesTabButton;
        [SerializeField] private Button _cosmeticsTabButton;

        [Header("Content References")]
        [SerializeField] private GameObject _featuredContent;
        [SerializeField] private GameObject _gemsContent;
        [SerializeField] private GameObject _coinsContent;
        [SerializeField] private GameObject _themesContent;
        [SerializeField] private GameObject _cosmeticsContent;

        [Header("Tab Visual Settings")]
        [SerializeField] private Color _activeTabColor = new Color(0f, 1f, 1f, 1f);
        [SerializeField] private Color _inactiveTabColor = new Color(0.12f, 0.16f, 0.2f, 1f);
        [SerializeField] private Color _activeTextColor = new Color(0.02f, 0.05f, 0.1f, 1f);
        [SerializeField] private Color _inactiveTextColor = new Color(0.95f, 0.95f, 0.95f, 1f);

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

        [Header("Currency Display")]
        [SerializeField] private CurrencyDisplayUI _gemsDisplay;
        [SerializeField] private CurrencyDisplayUI _coinsDisplay;
        [SerializeField] private TextMeshProUGUI _headerGemsText;
        [SerializeField] private TextMeshProUGUI _headerCoinsText;

        [Header("Shop Items")]
        [SerializeField] private List<ShopItemUI> _shopItems = new List<ShopItemUI>();

        private ShopTab _currentTab = ShopTab.Featured;
        private ShopTab _previousTab = ShopTab.Featured;
        private bool _isTabTransitioning;
        private Sequence _tabTransitionSequence;
        private Dictionary<ShopTab, Button> _tabButtons;
        private Dictionary<ShopTab, GameObject> _tabContents;
        private Dictionary<ShopTab, Image> _tabImages;
        private Dictionary<ShopTab, TextMeshProUGUI> _tabTexts;
        private Dictionary<ShopTab, Vector2> _tabContentOriginalPositions = new Dictionary<ShopTab, Vector2>();

        // Current item being purchased
        private ShopItemUI _currentPurchaseItem;

        // Events
        public event Action<ShopTab> OnTabChanged;
        public event Action<string> OnItemPurchased;
        public event Action OnShopClosed;

        private void Awake()
        {
            InitializeTabDictionaries();
            FindShopItems();
        }

        private void Start()
        {
            SetupButtons();
            SetupPopups();
            SetupCurrencyListeners();
            HandleNavigationParams();
            RefreshCurrencyDisplay();
        }

        private void OnDestroy()
        {
            _tabTransitionSequence?.Kill();
            RemoveCurrencyListeners();
        }

        private void FindShopItems()
        {
            // Find all ShopItemUI components in the scene
            _shopItems.Clear();
            _shopItems.AddRange(FindObjectsOfType<ShopItemUI>());

            // Subscribe to item events
            foreach (var item in _shopItems)
            {
                item.OnPurchaseRequested += OnItemPurchaseRequested;
            }

            Debug.Log($"[ShopManager] Found {_shopItems.Count} shop items");
        }

        private void SetupPopups()
        {
            // Purchase popup buttons
            if (_popupConfirmButton != null)
                _popupConfirmButton.onClick.AddListener(ConfirmPurchase);
            if (_popupCancelButton != null)
                _popupCancelButton.onClick.AddListener(CancelPurchase);

            // Not enough gems popup buttons
            if (_notEnoughCloseButton != null)
                _notEnoughCloseButton.onClick.AddListener(HideNotEnoughGemsPopup);
            if (_notEnoughGetGemsButton != null)
                _notEnoughGetGemsButton.onClick.AddListener(OnGetGemsClicked);

            // Make sure popups are hidden initially
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

            // Unsubscribe from shop items
            foreach (var item in _shopItems)
            {
                if (item != null)
                    item.OnPurchaseRequested -= OnItemPurchaseRequested;
            }
        }

        private void OnGemsChanged(int newAmount, int delta)
        {
            RefreshCurrencyDisplay();
        }

        private void OnCoinsChanged(int newAmount, int delta)
        {
            RefreshCurrencyDisplay();
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

            // Also update header text if available
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

        private void InitializeTabDictionaries()
        {
            _tabButtons = new Dictionary<ShopTab, Button>
            {
                { ShopTab.Featured, _featuredTabButton },
                { ShopTab.Gems, _gemsTabButton },
                { ShopTab.Coins, _coinsTabButton },
                { ShopTab.Themes, _themesTabButton },
                { ShopTab.Cosmetics, _cosmeticsTabButton }
            };

            _tabContents = new Dictionary<ShopTab, GameObject>
            {
                { ShopTab.Featured, _featuredContent },
                { ShopTab.Gems, _gemsContent },
                { ShopTab.Coins, _coinsContent },
                { ShopTab.Themes, _themesContent },
                { ShopTab.Cosmetics, _cosmeticsContent }
            };

            // Cache tab images and texts for color changes
            _tabImages = new Dictionary<ShopTab, Image>();
            _tabTexts = new Dictionary<ShopTab, TextMeshProUGUI>();

            foreach (var kvp in _tabButtons)
            {
                if (kvp.Value != null)
                {
                    _tabImages[kvp.Key] = kvp.Value.GetComponent<Image>();
                    _tabTexts[kvp.Key] = kvp.Value.GetComponentInChildren<TextMeshProUGUI>();
                }
            }

            // Cache original positions for tab content panels
            foreach (var kvp in _tabContents)
            {
                if (kvp.Value != null)
                {
                    var rt = kvp.Value.GetComponent<RectTransform>();
                    if (rt != null)
                        _tabContentOriginalPositions[kvp.Key] = rt.anchoredPosition;
                }
            }
        }

        private void SetupButtons()
        {
            // Tab buttons
            if (_featuredTabButton != null)
                _featuredTabButton.onClick.AddListener(() => SwitchToTab(ShopTab.Featured));
            if (_gemsTabButton != null)
                _gemsTabButton.onClick.AddListener(() => SwitchToTab(ShopTab.Gems));
            if (_coinsTabButton != null)
                _coinsTabButton.onClick.AddListener(() => SwitchToTab(ShopTab.Coins));
            if (_themesTabButton != null)
                _themesTabButton.onClick.AddListener(() => SwitchToTab(ShopTab.Themes));
            if (_cosmeticsTabButton != null)
                _cosmeticsTabButton.onClick.AddListener(() => SwitchToTab(ShopTab.Cosmetics));

            // Back button
            if (_backButton != null)
                _backButton.onClick.AddListener(OnBackButtonClick);

            // Initial tab
            SwitchToTab(_currentTab);
        }

        private void HandleNavigationParams()
        {
            var navigator = SceneNavigator.Instance;
            var navParams = navigator.ConsumeParams();

            if (navParams != null)
            {
                // Switch to target tab
                if (!string.IsNullOrEmpty(navParams.TargetTab))
                {
                    if (Enum.TryParse<ShopTab>(navParams.TargetTab, out ShopTab targetTab))
                    {
                        SwitchToTab(targetTab);
                    }
                }

                // Show popup if requested
                if (navParams.ShowPopup)
                {
                    ShowNotEnoughGemsPopup();
                }

                // Scroll to specific offer
                if (!string.IsNullOrEmpty(navParams.ItemId))
                {
                    ScrollToItem(navParams.ItemId);
                }

                Debug.Log($"[ShopManager] Handled navigation params - Tab: {navParams.TargetTab}, ShowPopup: {navParams.ShowPopup}");
            }
        }

        /// <summary>
        /// Cambia a una tab especifica con animacion DOTween
        /// </summary>
        public void SwitchToTab(ShopTab tab)
        {
            if (_isTabTransitioning) return;
            if (_currentTab == tab && _tabContents.TryGetValue(tab, out var currentPanel) && currentPanel != null && currentPanel.activeSelf) return;

            _previousTab = _currentTab;
            _currentTab = tab;

            // Update tab button visuals with DOTween
            foreach (var kvp in _tabButtons)
            {
                bool isActive = kvp.Key == tab;

                if (_tabImages.TryGetValue(kvp.Key, out Image image) && image != null)
                {
                    image.DOColor(isActive ? GetTabColor(kvp.Key) : _inactiveTabColor, 0.2f);
                }

                if (_tabTexts.TryGetValue(kvp.Key, out TextMeshProUGUI text) && text != null)
                {
                    text.DOColor(isActive ? _activeTextColor : _inactiveTextColor, 0.2f);
                }

                // Scale animation for active tab
                if (kvp.Value != null)
                {
                    kvp.Value.transform.DOScale(isActive ? 1.05f : 1f, 0.2f).SetEase(Ease.OutCubic);
                }
            }

            // Determine slide direction
            bool goingRight = (int)tab > (int)_previousTab;

            // Animate content panels
            _tabTransitionSequence?.Kill();
            _tabTransitionSequence = DOTween.Sequence();
            _isTabTransitioning = true;

            // Fade out + slide old content
            if (_tabContents.TryGetValue(_previousTab, out var oldContent) && oldContent != null && oldContent.activeSelf && _previousTab != tab)
            {
                var oldCG = oldContent.GetComponent<CanvasGroup>();
                if (oldCG == null) oldCG = oldContent.AddComponent<CanvasGroup>();
                var oldRT = oldContent.GetComponent<RectTransform>();
                Vector2 oldOrigPos = _tabContentOriginalPositions.ContainsKey(_previousTab) ? _tabContentOriginalPositions[_previousTab] : Vector2.zero;
                float exitDir = goingRight ? -30f : 30f;
                var capturedOldContent = oldContent;
                var capturedOldRT = oldRT;
                var capturedOldCG = oldCG;
                var capturedOldOrigPos = oldOrigPos;

                _tabTransitionSequence
                    .Join(capturedOldCG.DOFade(0f, 0.15f))
                    .Join(capturedOldRT.DOAnchorPosX(capturedOldOrigPos.x + exitDir, 0.15f));

                _tabTransitionSequence.InsertCallback(0.15f, () =>
                {
                    capturedOldContent.SetActive(false);
                    capturedOldRT.anchoredPosition = capturedOldOrigPos;
                    capturedOldCG.alpha = 1f;
                });
            }

            // Fade in + slide new content
            if (_tabContents.TryGetValue(tab, out var newContent) && newContent != null)
            {
                var newCG = newContent.GetComponent<CanvasGroup>();
                if (newCG == null) newCG = newContent.AddComponent<CanvasGroup>();
                var newRT = newContent.GetComponent<RectTransform>();
                Vector2 newOrigPos = _tabContentOriginalPositions.ContainsKey(tab) ? _tabContentOriginalPositions[tab] : Vector2.zero;
                float enterDir = goingRight ? 30f : -30f;

                newCG.alpha = 0f;
                newContent.SetActive(true);
                newRT.anchoredPosition = newOrigPos + new Vector2(enterDir, 0);

                float enterDelay = (_previousTab != tab && oldContent != null && oldContent != newContent) ? 0.1f : 0f;

                _tabTransitionSequence.Insert(enterDelay, newCG.DOFade(1f, 0.2f));
                _tabTransitionSequence.Insert(enterDelay, newRT.DOAnchorPos(newOrigPos, 0.2f).SetEase(Ease.OutQuad));
            }

            // Hide all other content panels immediately (safety)
            foreach (var kvp in _tabContents)
            {
                if (kvp.Key != tab && kvp.Key != _previousTab && kvp.Value != null)
                {
                    kvp.Value.SetActive(false);
                }
            }

            _tabTransitionSequence.OnComplete(() => _isTabTransitioning = false);

            OnTabChanged?.Invoke(tab);
            Debug.Log($"[ShopManager] Switched to tab: {tab}");
        }

        private Color GetTabColor(ShopTab tab)
        {
            switch (tab)
            {
                case ShopTab.Featured:
                    return new Color(1f, 0.84f, 0f, 1f); // Gold
                case ShopTab.Gems:
                    return new Color(0.4f, 0.8f, 1f, 1f); // Gem blue
                case ShopTab.Coins:
                    return new Color(1f, 0.85f, 0.3f, 1f); // Gold
                case ShopTab.Themes:
                    return new Color(0.6f, 0.3f, 0.9f, 1f); // Purple
                case ShopTab.Cosmetics:
                    return new Color(1f, 0.3f, 0.6f, 1f); // Pink/Magenta
                default:
                    return _activeTabColor;
            }
        }

        private void ScrollToItem(string itemId)
        {
            // TODO: Implement scroll to specific item
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

            // For real money items, show confirmation popup
            if (itemData.priceType == PriceType.RealMoney)
            {
                ShowPurchasePopup(item);
                return;
            }

            // For in-game currency, check if can afford
            if (!itemData.CanAfford())
            {
                if (itemData.priceType == PriceType.Gems)
                {
                    ShowNotEnoughGemsPopup();
                }
                return;
            }

            // Show confirmation popup for all purchases
            ShowPurchasePopup(item);
        }

        // ==================== POPUP MANAGEMENT ====================

        public void ShowPurchasePopup(ShopItemUI item)
        {
            if (_purchasePopup == null || item == null) return;

            _currentPurchaseItem = item;
            var itemData = item.ItemData;

            // Populate popup
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
                    case ShopItemType.GemsPack:
                        amountText = $"{itemData.GetTotalGems():N0} Gemas";
                        break;
                    case ShopItemType.CoinsPack:
                        amountText = $"{itemData.GetTotalCoins():N0} Monedas";
                        break;
                    default:
                        amountText = itemData.displayName;
                        break;
                }
                _popupItemName.text = amountText;
            }

            if (_popupItemPrice != null)
            {
                string pricePrefix = itemData.priceType == PriceType.RealMoney ? "Precio: " : "";
                _popupItemPrice.text = pricePrefix + itemData.GetFormattedPrice();
            }

            _purchasePopup.SetActive(true);
            Debug.Log($"[ShopManager] Showing purchase popup for: {itemData.displayName}");
        }

        public void ShowPurchasePopup(string itemId, string itemName, string price)
        {
            if (_purchasePopup != null)
            {
                if (_popupItemName != null) _popupItemName.text = itemName;
                if (_popupItemPrice != null) _popupItemPrice.text = price;
                _purchasePopup.SetActive(true);
            }
        }

        public void HidePurchasePopup()
        {
            if (_purchasePopup != null)
            {
                _purchasePopup.SetActive(false);
            }
            _currentPurchaseItem = null;
        }

        public void ShowNotEnoughGemsPopup()
        {
            if (_notEnoughGemsPopup != null)
            {
                _notEnoughGemsPopup.SetActive(true);
            }
        }

        public void HideNotEnoughGemsPopup()
        {
            if (_notEnoughGemsPopup != null)
            {
                _notEnoughGemsPopup.SetActive(false);
            }
        }

        private void OnGetGemsClicked()
        {
            HideNotEnoughGemsPopup();
            SwitchToTab(ShopTab.Gems);
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

            // Handle real money purchases (IAP)
            if (itemData.priceType == PriceType.RealMoney)
            {
                ProcessIAPPurchase(itemData);
            }
            else
            {
                // In-game currency purchase
                bool success = _currentPurchaseItem.TryPurchase();
                if (success)
                {
                    OnItemPurchased?.Invoke(itemData.itemId);
                    Debug.Log($"[ShopManager] Purchase successful: {itemData.displayName}");
                }
            }

            HidePurchasePopup();
        }

        private void ProcessIAPPurchase(ShopItemData itemData)
        {
            // For now, simulate IAP purchase
            // In production, this would call PremiumManager or Unity IAP
            Debug.Log($"[ShopManager] Processing IAP: {itemData.iapProductId}");

            // Simulate successful purchase
            StartCoroutine(SimulateIAPPurchase(itemData));
        }

        private System.Collections.IEnumerator SimulateIAPPurchase(ShopItemData itemData)
        {
            yield return new WaitForSeconds(0.5f);

            // Grant the rewards
            itemData.GrantRewards();

            OnItemPurchased?.Invoke(itemData.itemId);
            Debug.Log($"[ShopManager] IAP purchase completed: {itemData.displayName}");
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

        /// <summary>
        /// Registra un ShopItemUI manualmente
        /// </summary>
        public void RegisterShopItem(ShopItemUI item)
        {
            if (item != null && !_shopItems.Contains(item))
            {
                _shopItems.Add(item);
                item.OnPurchaseRequested += OnItemPurchaseRequested;
            }
        }

        /// <summary>
        /// Obtiene todos los items de una tab especifica
        /// </summary>
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
    }
}
