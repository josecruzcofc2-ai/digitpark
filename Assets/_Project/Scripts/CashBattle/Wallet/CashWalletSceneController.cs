using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using DigitPark.Monetization;

namespace DigitPark.CashBattle
{
    /// <summary>
    /// Controller principal para la escena CashWallet.
    /// Maneja toda la UI y lógica de la pantalla de Wallet.
    /// </summary>
    public class CashWalletSceneController : MonoBehaviour
    {
        // ==================== HEADER ====================
        [Header("Header")]
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI balanceText;
        [SerializeField] private TextMeshProUGUI bonusBalanceText;

        // ==================== TABS ====================
        [Header("Tabs")]
        [SerializeField] private Button depositTabButton;
        [SerializeField] private Button withdrawTabButton;
        [SerializeField] private Button historyTabButton;
        [SerializeField] private Color activeTabColor = new Color(0f, 0.83f, 1f, 1f);
        [SerializeField] private Color inactiveTabColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        // ==================== PANELS ====================
        [Header("Panels")]
        [SerializeField] private GameObject depositPanel;
        [SerializeField] private GameObject withdrawPanel;
        [SerializeField] private GameObject transactionHistoryPanel;

        // ==================== DEPOSIT PANEL ====================
        [Header("Deposit Panel")]
        [SerializeField] private Transform depositOptionsContainer;
        [SerializeField] private GameObject depositOptionPrefab;
        [SerializeField] private Button[] paymentMethodButtons;
        [SerializeField] private GameObject paymentMethodsContainer;

        // ==================== WITHDRAW PANEL ====================
        [Header("Withdraw Panel")]
        [SerializeField] private TMP_InputField withdrawAmountInput;
        [SerializeField] private Button withdrawButton;
        [SerializeField] private TextMeshProUGUI withdrawableAmountText;
        [SerializeField] private TextMeshProUGUI withdrawMinText;
        [SerializeField] private TextMeshProUGUI withdrawFeeText;
        [SerializeField] private GameObject kycRequiredPanel;
        [SerializeField] private Button verifyKycButton;

        // ==================== TRANSACTION HISTORY ====================
        [Header("Transaction History")]
        [SerializeField] private Transform transactionsContainer;
        [SerializeField] private GameObject transactionItemPrefab;
        [SerializeField] private TextMeshProUGUI emptyHistoryText;
        [SerializeField] private Button loadMoreButton;

        // ==================== OVERLAYS ====================
        [Header("Overlays")]
        [SerializeField] private GameObject loadingOverlay;
        [SerializeField] private GameObject successOverlay;
        [SerializeField] private GameObject errorOverlay;
        [SerializeField] private TextMeshProUGUI errorMessageText;

        // ==================== CONFIGURATION ====================
        [Header("Configuration")]
        [SerializeField] private float minimumWithdrawFloat = 10f;
        [SerializeField] private float withdrawFeePercentFloat = 0f;
        [SerializeField] private int transactionsPerPage = 20;

        private decimal minimumWithdraw => (decimal)minimumWithdrawFloat;
        private decimal withdrawFeePercent => (decimal)withdrawFeePercentFloat;

        // ==================== STATE ====================
        private WalletTab currentTab = WalletTab.Deposit;
        private PaymentMethod selectedPaymentMethod = PaymentMethod.CreditCard;
        private int currentTransactionPage = 0;
        private List<GameObject> spawnedDepositOptions = new List<GameObject>();
        private List<GameObject> spawnedTransactions = new List<GameObject>();

        private enum WalletTab
        {
            Deposit,
            Withdraw,
            History
        }

        // ==================== DEPOSIT OPTIONS ====================
        private readonly DepositOption[] depositOptions = new DepositOption[]
        {
            new DepositOption { amount = 5m, bonus = 0m },
            new DepositOption { amount = 10m, bonus = 0.50m, isPopular = true },
            new DepositOption { amount = 25m, bonus = 2.50m },
            new DepositOption { amount = 50m, bonus = 7.50m },
            new DepositOption { amount = 100m, bonus = 20m, isPopular = true },
            new DepositOption { amount = 200m, bonus = 50m }
        };

        // ==================== LIFECYCLE ====================

        private void Start()
        {
            InitializeUI();
            SetupButtonListeners();
            SubscribeToEvents();
            RefreshUI();

            // Check for navigation params
            CheckNavigationParams();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void InitializeUI()
        {
            // Setup deposit options
            PopulateDepositOptions();

            // Setup initial tab
            ShowTab(WalletTab.Deposit);

            // Hide overlays
            if (loadingOverlay) loadingOverlay.SetActive(false);
            if (successOverlay) successOverlay.SetActive(false);
            if (errorOverlay) errorOverlay.SetActive(false);
        }

        private void SetupButtonListeners()
        {
            // Back button
            if (backButton)
                backButton.onClick.AddListener(OnBackClicked);

            // Tab buttons
            if (depositTabButton)
                depositTabButton.onClick.AddListener(() => ShowTab(WalletTab.Deposit));
            if (withdrawTabButton)
                withdrawTabButton.onClick.AddListener(() => ShowTab(WalletTab.Withdraw));
            if (historyTabButton)
                historyTabButton.onClick.AddListener(() => ShowTab(WalletTab.History));

            // Withdraw
            if (withdrawButton)
                withdrawButton.onClick.AddListener(OnWithdrawClicked);
            if (verifyKycButton)
                verifyKycButton.onClick.AddListener(OnVerifyKycClicked);
            if (withdrawAmountInput)
                withdrawAmountInput.onValueChanged.AddListener(OnWithdrawAmountChanged);

            // Load more
            if (loadMoreButton)
                loadMoreButton.onClick.AddListener(OnLoadMoreClicked);

            // Payment method buttons
            SetupPaymentMethodButtons();
        }

        private void SetupPaymentMethodButtons()
        {
            if (paymentMethodButtons == null) return;

            for (int i = 0; i < paymentMethodButtons.Length; i++)
            {
                int index = i;
                PaymentMethod method = (PaymentMethod)i;
                paymentMethodButtons[i]?.onClick.AddListener(() => SelectPaymentMethod(method));
            }
        }

        private void SubscribeToEvents()
        {
            if (WalletManager.Instance != null)
            {
                WalletManager.Instance.OnBalanceChanged += OnBalanceChanged;
                WalletManager.Instance.OnDepositCompleted += OnDepositCompleted;
                WalletManager.Instance.OnTransactionCompleted += OnTransactionCompleted;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (WalletManager.Instance != null)
            {
                WalletManager.Instance.OnBalanceChanged -= OnBalanceChanged;
                WalletManager.Instance.OnDepositCompleted -= OnDepositCompleted;
                WalletManager.Instance.OnTransactionCompleted -= OnTransactionCompleted;
            }
        }

        private void CheckNavigationParams()
        {
            var navParams = SceneNavigator.Instance?.ConsumeParams();
            if (navParams != null)
            {
                // Check if we should show a specific tab
                if (!string.IsNullOrEmpty(navParams.TargetTab))
                {
                    if (Enum.TryParse<WalletTab>(navParams.TargetTab, out WalletTab tab))
                    {
                        ShowTab(tab);
                    }
                }
            }
        }

        // ==================== TAB MANAGEMENT ====================

        private void ShowTab(WalletTab tab)
        {
            currentTab = tab;

            // Update panels
            if (depositPanel) depositPanel.SetActive(tab == WalletTab.Deposit);
            if (withdrawPanel) withdrawPanel.SetActive(tab == WalletTab.Withdraw);
            if (transactionHistoryPanel) transactionHistoryPanel.SetActive(tab == WalletTab.History);

            // Update tab colors
            UpdateTabColors();

            // Load content for tab
            switch (tab)
            {
                case WalletTab.Withdraw:
                    RefreshWithdrawPanel();
                    break;
                case WalletTab.History:
                    LoadTransactionHistory(reset: true);
                    break;
            }
        }

        private void UpdateTabColors()
        {
            UpdateTabButton(depositTabButton, currentTab == WalletTab.Deposit);
            UpdateTabButton(withdrawTabButton, currentTab == WalletTab.Withdraw);
            UpdateTabButton(historyTabButton, currentTab == WalletTab.History);
        }

        private void UpdateTabButton(Button button, bool isActive)
        {
            if (button == null) return;

            var image = button.GetComponent<Image>();
            if (image) image.color = isActive ? activeTabColor : inactiveTabColor;

            var text = button.GetComponentInChildren<TextMeshProUGUI>();
            if (text) text.color = isActive ? Color.white : new Color(0.7f, 0.7f, 0.7f);
        }

        // ==================== DEPOSIT ====================

        private void PopulateDepositOptions()
        {
            if (depositOptionsContainer == null || depositOptionPrefab == null) return;

            // Clear existing
            foreach (var obj in spawnedDepositOptions)
            {
                if (obj) Destroy(obj);
            }
            spawnedDepositOptions.Clear();

            // Create options
            foreach (var option in depositOptions)
            {
                var optionObj = Instantiate(depositOptionPrefab, depositOptionsContainer);
                spawnedDepositOptions.Add(optionObj);

                var optionUI = optionObj.GetComponent<DepositOptionUI>();
                if (optionUI)
                {
                    optionUI.Setup(option, OnDepositOptionSelected);
                }
            }
        }

        private void SelectPaymentMethod(PaymentMethod method)
        {
            selectedPaymentMethod = method;

            // Update button visuals
            if (paymentMethodButtons != null)
            {
                for (int i = 0; i < paymentMethodButtons.Length; i++)
                {
                    var btn = paymentMethodButtons[i];
                    if (btn == null) continue;

                    bool isSelected = i == (int)method;
                    var image = btn.GetComponent<Image>();
                    if (image)
                    {
                        image.color = isSelected ? activeTabColor : Color.white;
                    }
                }
            }

            Debug.Log($"[CashWalletScene] Payment method selected: {method}");
        }

        private async void OnDepositOptionSelected(DepositOption option)
        {
            Debug.Log($"[CashWalletScene] Deposit selected: ${option.amount} with ${option.bonus} bonus");

            // Show loading
            ShowLoading(true);

            // Start deposit through WalletManager (async)
            if (WalletManager.Instance != null)
            {
                await WalletManager.Instance.InitiateDeposit(option, selectedPaymentMethod);
            }
        }

        // ==================== WITHDRAW ====================

        private void RefreshWithdrawPanel()
        {
            if (WalletManager.Instance == null) return;

            var walletData = WalletManager.Instance.WalletData;

            // Update withdrawable amount
            if (withdrawableAmountText)
            {
                withdrawableAmountText.text = $"${walletData.AvailableBalance:F2}";
            }

            // Update minimum
            if (withdrawMinText)
            {
                withdrawMinText.text = $"Mínimo: ${minimumWithdraw:F2}";
            }

            // Show/hide KYC panel
            bool needsKyc = !walletData.isVerified && walletData.AvailableBalance >= minimumWithdraw;
            if (kycRequiredPanel)
            {
                kycRequiredPanel.SetActive(needsKyc);
            }

            // Update withdraw button state
            UpdateWithdrawButton();
        }

        private void OnWithdrawAmountChanged(string value)
        {
            UpdateWithdrawButton();

            // Calculate fee
            if (decimal.TryParse(value, out decimal amount))
            {
                decimal fee = amount * (withdrawFeePercent / 100m);
                if (withdrawFeeText)
                {
                    withdrawFeeText.text = fee > 0 ? $"Comisión: ${fee:F2}" : "Sin comisión";
                }
            }
        }

        private void UpdateWithdrawButton()
        {
            if (withdrawButton == null || WalletManager.Instance == null) return;

            bool canWithdraw = false;

            if (decimal.TryParse(withdrawAmountInput?.text, out decimal amount))
            {
                var walletData = WalletManager.Instance.WalletData;
                canWithdraw = amount >= minimumWithdraw &&
                              amount <= walletData.AvailableBalance &&
                              walletData.isVerified;
            }

            withdrawButton.interactable = canWithdraw;
        }

        private void OnWithdrawClicked()
        {
            if (!decimal.TryParse(withdrawAmountInput?.text, out decimal amount)) return;

            Debug.Log($"[CashWalletScene] Withdraw requested: ${amount}");

            ShowLoading(true);
            WalletManager.Instance?.RequestWithdrawal(amount, selectedPaymentMethod);
        }

        private void OnVerifyKycClicked()
        {
            Debug.Log("[CashWalletScene] KYC verification requested");
            // Navigate to KYC verification scene or show popup
            // SceneNavigator.Instance?.NavigateTo(SceneNavigator.Scenes.AGE_VERIFICATION);
        }

        // ==================== TRANSACTION HISTORY ====================

        private void LoadTransactionHistory(bool reset = false)
        {
            if (reset)
            {
                currentTransactionPage = 0;
                ClearTransactionItems();
            }

            if (WalletManager.Instance == null) return;

            // GetTransactionHistory solo acepta limit, no offset
            var transactions = WalletManager.Instance.GetTransactionHistory(transactionsPerPage);

            // Show empty state if needed
            if (emptyHistoryText)
            {
                emptyHistoryText.gameObject.SetActive(transactions.Count == 0 && currentTransactionPage == 0);
            }

            // Populate transactions
            PopulateTransactions(transactions);

            // Show/hide load more
            if (loadMoreButton)
            {
                loadMoreButton.gameObject.SetActive(transactions.Count >= transactionsPerPage);
            }
        }

        private void PopulateTransactions(List<WalletTransaction> transactions)
        {
            if (transactionsContainer == null || transactionItemPrefab == null) return;

            foreach (var transaction in transactions)
            {
                var itemObj = Instantiate(transactionItemPrefab, transactionsContainer);
                spawnedTransactions.Add(itemObj);

                var itemUI = itemObj.GetComponent<TransactionItemUI>();
                if (itemUI)
                {
                    itemUI.Setup(transaction);
                }
            }
        }

        private void ClearTransactionItems()
        {
            foreach (var obj in spawnedTransactions)
            {
                if (obj) Destroy(obj);
            }
            spawnedTransactions.Clear();
        }

        private void OnLoadMoreClicked()
        {
            currentTransactionPage++;
            LoadTransactionHistory(reset: false);
        }

        // ==================== UI REFRESH ====================

        private void RefreshUI()
        {
            if (WalletManager.Instance == null) return;

            var walletData = WalletManager.Instance.WalletData;

            // Update balance displays
            if (balanceText)
            {
                balanceText.text = $"${walletData.balance:F2}";
            }

            if (bonusBalanceText)
            {
                // Mostrar balance pendiente como "bonus" o lifetime deposits
                bonusBalanceText.text = $"Pendiente: ${walletData.pendingBalance:F2}";
            }
        }

        // ==================== EVENT HANDLERS ====================

        private void OnBalanceChanged(decimal newBalance, decimal delta)
        {
            RefreshUI();
            RefreshWithdrawPanel();
        }

        private void OnDepositCompleted(bool success, string message)
        {
            ShowLoading(false);

            if (success)
            {
                ShowSuccess(message);
                RefreshUI();
            }
            else
            {
                ShowError(message ?? "Error al procesar el depósito. Intenta de nuevo.");
            }
        }

        private void OnTransactionCompleted(WalletTransaction transaction)
        {
            // Refresh history if on that tab
            if (currentTab == WalletTab.History)
            {
                LoadTransactionHistory(reset: true);
            }
        }

        // ==================== OVERLAYS ====================

        private void ShowLoading(bool show)
        {
            if (loadingOverlay)
            {
                loadingOverlay.SetActive(show);
            }
        }

        private void ShowSuccess(string message)
        {
            if (successOverlay)
            {
                successOverlay.SetActive(true);

                var text = successOverlay.GetComponentInChildren<TextMeshProUGUI>();
                if (text) text.text = message;

                // Auto-hide after delay
                Invoke(nameof(HideSuccess), 2f);
            }
        }

        private void HideSuccess()
        {
            if (successOverlay)
            {
                successOverlay.SetActive(false);
            }
        }

        private void ShowError(string message)
        {
            if (errorOverlay)
            {
                errorOverlay.SetActive(true);
                if (errorMessageText) errorMessageText.text = message;
            }
        }

        public void HideError()
        {
            if (errorOverlay)
            {
                errorOverlay.SetActive(false);
            }
        }

        // ==================== NAVIGATION ====================

        private void OnBackClicked()
        {
            SceneNavigator.Instance?.GoBack();
        }

        // ==================== PUBLIC API ====================

        /// <summary>
        /// Opens wallet to a specific tab
        /// </summary>
        public void OpenTab(string tabName)
        {
            if (Enum.TryParse<WalletTab>(tabName, out WalletTab tab))
            {
                ShowTab(tab);
            }
        }

        /// <summary>
        /// Refreshes all wallet data
        /// </summary>
        public void RefreshAll()
        {
            RefreshUI();
            if (currentTab == WalletTab.Withdraw)
            {
                RefreshWithdrawPanel();
            }
            else if (currentTab == WalletTab.History)
            {
                LoadTransactionHistory(reset: true);
            }
        }
    }
}
