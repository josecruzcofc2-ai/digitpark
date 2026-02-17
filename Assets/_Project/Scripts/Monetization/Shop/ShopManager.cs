using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

namespace DigitPark.Monetization
{
    /// <summary>
    /// Manager principal de la escena Shop V3.
    /// Scroll continuo sin tabs - todas las secciones visibles.
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

        [Header("Currency Display")]
        [SerializeField] private CurrencyDisplayUI _gemsDisplay;
        [SerializeField] private CurrencyDisplayUI _coinsDisplay;
        [SerializeField] private TextMeshProUGUI _headerGemsText;
        [SerializeField] private TextMeshProUGUI _headerCoinsText;

        [Header("Shop Items")]
        [SerializeField] private List<ShopItemUI> _shopItems = new List<ShopItemUI>();

        // Current item being purchased
        private ShopItemUI _currentPurchaseItem;

        // ScrollRect reference for programmatic scrolling
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
            HandleNavigationParams();
            RefreshCurrencyDisplay();
            ForceLayoutRebuild();
        }

        private void OnDestroy()
        {
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
            if (_backButton != null)
                _backButton.onClick.AddListener(OnBackButtonClick);
        }

        private void HandleNavigationParams()
        {
            var navigator = SceneNavigator.Instance;
            var navParams = navigator.ConsumeParams();

            if (navParams != null)
            {
                // Scroll to section based on tab param
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
        /// Scroll programatico a una seccion por ShopTab.
        /// Mapea tabs a posiciones aproximadas del scroll.
        /// </summary>
        public void ScrollToSection(ShopTab tab)
        {
            if (_scrollRect == null) return;

            // Normalized position (1 = top, 0 = bottom)
            float pos = 1f;
            switch (tab)
            {
                case ShopTab.Featured: pos = 1f; break;
                case ShopTab.Gems: pos = 0.7f; break;
                case ShopTab.Coins: pos = 0.55f; break;
                case ShopTab.Themes: pos = 0.4f; break;
                case ShopTab.Cosmetics: pos = 0.2f; break;
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
                if (itemData.priceType == PriceType.Gems)
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
            ScrollToSection(ShopTab.Gems);
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
                ProcessIAPPurchase(itemData);
            }
            else
            {
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
            Debug.Log($"[ShopManager] Processing IAP: {itemData.iapProductId}");
            StartCoroutine(SimulateIAPPurchase(itemData));
        }

        private System.Collections.IEnumerator SimulateIAPPurchase(ShopItemData itemData)
        {
            yield return new WaitForSeconds(0.5f);

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

            // Also rebuild all nested layout groups
            foreach (var layout in GetComponentsInChildren<LayoutGroup>(true))
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
            }
        }
    }
}
