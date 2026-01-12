using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

namespace DigitPark.Monetization
{
    /// <summary>
    /// Manager principal de la escena Shop.
    /// Maneja tabs, items, compras y navegacion.
    /// </summary>
    public class ShopManager : MonoBehaviour
    {
        [Header("Tab References")]
        [SerializeField] private Button _gemsTabButton;
        [SerializeField] private Button _coinsTabButton;
        [SerializeField] private Button _themesTabButton;
        [SerializeField] private Button _offersTabButton;

        [Header("Content References")]
        [SerializeField] private GameObject _gemsContent;
        [SerializeField] private GameObject _coinsContent;
        [SerializeField] private GameObject _themesContent;
        [SerializeField] private GameObject _offersContent;

        [Header("Tab Visual Settings")]
        [SerializeField] private Color _activeTabColor = new Color(0f, 1f, 1f, 1f);
        [SerializeField] private Color _inactiveTabColor = new Color(0.12f, 0.16f, 0.2f, 1f);
        [SerializeField] private Color _activeTextColor = new Color(0.02f, 0.05f, 0.1f, 1f);
        [SerializeField] private Color _inactiveTextColor = new Color(0.95f, 0.95f, 0.95f, 1f);

        [Header("Popups")]
        [SerializeField] private GameObject _purchasePopup;
        [SerializeField] private GameObject _notEnoughGemsPopup;

        [Header("Navigation")]
        [SerializeField] private Button _backButton;

        [Header("Currency Display")]
        [SerializeField] private CurrencyDisplayUI _gemsDisplay;
        [SerializeField] private CurrencyDisplayUI _coinsDisplay;

        private ShopTab _currentTab = ShopTab.Gems;
        private Dictionary<ShopTab, Button> _tabButtons;
        private Dictionary<ShopTab, GameObject> _tabContents;
        private Dictionary<ShopTab, Image> _tabImages;
        private Dictionary<ShopTab, TextMeshProUGUI> _tabTexts;

        // Events
        public event Action<ShopTab> OnTabChanged;
        public event Action<string> OnItemPurchased;
        public event Action OnShopClosed;

        private void Awake()
        {
            InitializeTabDictionaries();
        }

        private void Start()
        {
            SetupButtons();
            HandleNavigationParams();
        }

        private void InitializeTabDictionaries()
        {
            _tabButtons = new Dictionary<ShopTab, Button>
            {
                { ShopTab.Gems, _gemsTabButton },
                { ShopTab.Coins, _coinsTabButton },
                { ShopTab.Themes, _themesTabButton },
                { ShopTab.Offers, _offersTabButton }
            };

            _tabContents = new Dictionary<ShopTab, GameObject>
            {
                { ShopTab.Gems, _gemsContent },
                { ShopTab.Coins, _coinsContent },
                { ShopTab.Themes, _themesContent },
                { ShopTab.Offers, _offersContent }
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
        }

        private void SetupButtons()
        {
            // Tab buttons
            if (_gemsTabButton != null)
                _gemsTabButton.onClick.AddListener(() => SwitchToTab(ShopTab.Gems));
            if (_coinsTabButton != null)
                _coinsTabButton.onClick.AddListener(() => SwitchToTab(ShopTab.Coins));
            if (_themesTabButton != null)
                _themesTabButton.onClick.AddListener(() => SwitchToTab(ShopTab.Themes));
            if (_offersTabButton != null)
                _offersTabButton.onClick.AddListener(() => SwitchToTab(ShopTab.Offers));

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
        /// Cambia a una tab especifica
        /// </summary>
        public void SwitchToTab(ShopTab tab)
        {
            _currentTab = tab;

            // Update tab visuals
            foreach (var kvp in _tabButtons)
            {
                bool isActive = kvp.Key == tab;

                // Update button image color
                if (_tabImages.TryGetValue(kvp.Key, out Image image) && image != null)
                {
                    image.color = isActive ? GetTabColor(kvp.Key) : _inactiveTabColor;
                }

                // Update text color
                if (_tabTexts.TryGetValue(kvp.Key, out TextMeshProUGUI text) && text != null)
                {
                    text.color = isActive ? _activeTextColor : _inactiveTextColor;
                }
            }

            // Show/hide content
            foreach (var kvp in _tabContents)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.SetActive(kvp.Key == tab);
                }
            }

            OnTabChanged?.Invoke(tab);
            Debug.Log($"[ShopManager] Switched to tab: {tab}");
        }

        private Color GetTabColor(ShopTab tab)
        {
            switch (tab)
            {
                case ShopTab.Gems:
                    return new Color(0.4f, 0.8f, 1f, 1f); // Gem blue
                case ShopTab.Coins:
                    return new Color(1f, 0.85f, 0.3f, 1f); // Gold
                case ShopTab.Themes:
                    return new Color(0.6f, 0.3f, 0.9f, 1f); // Purple
                case ShopTab.Offers:
                    return new Color(1f, 0.5f, 0.1f, 1f); // Orange
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

        // ==================== POPUP MANAGEMENT ====================

        public void ShowPurchasePopup(string itemId, string itemName, string price)
        {
            if (_purchasePopup != null)
            {
                _purchasePopup.SetActive(true);
                // TODO: Populate popup with item details
            }
        }

        public void HidePurchasePopup()
        {
            if (_purchasePopup != null)
            {
                _purchasePopup.SetActive(false);
            }
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

        // ==================== PURCHASE METHODS ====================

        public void PurchaseItem(string itemId)
        {
            // TODO: Implement actual purchase logic with payment provider
            Debug.Log($"[ShopManager] Purchasing item: {itemId}");
            OnItemPurchased?.Invoke(itemId);
        }

        public void ConfirmPurchase()
        {
            // Called when user confirms in popup
            HidePurchasePopup();
            // TODO: Process purchase
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
    }
}
