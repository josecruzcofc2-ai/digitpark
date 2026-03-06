using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using DigitPark.Monetization;
using DG.Tweening;
using DigitPark.Animations;
using DigitPark.Localization;

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

        // ==================== ACTION BUTTONS ====================
        [Header("Action Buttons")]
        [SerializeField] private Button depositTabButton;
        [SerializeField] private Button withdrawTabButton;
        [SerializeField] private Button historyTabButton;
        [SerializeField] private Button closeDepositButton;
        [SerializeField] private Button closeWithdrawButton;
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
        private WalletTab previousTab = WalletTab.Deposit;
        private bool isTabTransitioning;
        private Sequence tabTransitionSequence;
        private Dictionary<WalletTab, Vector2> panelOriginalPositions = new Dictionary<WalletTab, Vector2>();
        private PaymentMethod selectedPaymentMethod = PaymentMethod.CreditCard;
        private int currentTransactionPage = 0;
        private List<GameObject> spawnedDepositOptions = new List<GameObject>();
        private List<GameObject> spawnedTransactions = new List<GameObject>();
        private decimal previousBalance = 0m;

        private enum WalletTab
        {
            Deposit,
            Withdraw,
            History
        }

        // ==================== DEPOSIT OPTIONS ====================
        private readonly DepositOption[] depositOptions = new DepositOption[]
        {
            new DepositOption { amount = 5m },
            new DepositOption { amount = 10m },
            new DepositOption { amount = 25m },
            new DepositOption { amount = 50m },
            new DepositOption { amount = 100m },
            new DepositOption { amount = 200m }
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
            tabTransitionSequence?.Kill();
            UnsubscribeFromEvents();
        }

        private void InitializeUI()
        {
            // Cache original positions of content panels
            CacheOriginalPosition(WalletTab.Deposit, depositPanel);
            CacheOriginalPosition(WalletTab.Withdraw, withdrawPanel);
            CacheOriginalPosition(WalletTab.History, transactionHistoryPanel);

            // Setup deposit options
            PopulateDepositOptions();

            // Start with all overlay panels hidden - main view shows transaction list
            if (depositPanel) depositPanel.SetActive(false);
            if (withdrawPanel) withdrawPanel.SetActive(false);
            if (transactionHistoryPanel) transactionHistoryPanel.SetActive(false);

            // Hide overlays
            if (loadingOverlay) loadingOverlay.SetActive(false);
            if (successOverlay) successOverlay.SetActive(false);
            if (errorOverlay) errorOverlay.SetActive(false);
        }

        private void CacheOriginalPosition(WalletTab tab, GameObject panel)
        {
            if (panel != null)
            {
                var rt = panel.GetComponent<RectTransform>();
                if (rt != null)
                    panelOriginalPositions[tab] = rt.anchoredPosition;
            }
        }

        private void SetupButtonListeners()
        {
            // Back button - disable auto-navigation from BackButtonGold prefab to prevent double listener
            var autoNav = backButton?.GetComponent<DigitPark.UI.BackButtonGold>();
            if (autoNav != null) autoNav.DisableAutoNavigation();
            if (backButton)
                backButton.onClick.AddListener(OnBackClicked);

            // Action buttons - open modal panels
            if (depositTabButton)
                depositTabButton.onClick.AddListener(() => ShowOverlayPanel(depositPanel));
            if (withdrawTabButton)
                withdrawTabButton.onClick.AddListener(() => ShowOverlayPanel(withdrawPanel));
            if (historyTabButton)
                historyTabButton.onClick.AddListener(() => ShowTab(WalletTab.History));

            // Close buttons on modal panels
            if (closeDepositButton)
                closeDepositButton.onClick.AddListener(() => HideOverlayPanel(depositPanel));
            if (closeWithdrawButton)
                closeWithdrawButton.onClick.AddListener(() => HideOverlayPanel(withdrawPanel));

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
            if (isTabTransitioning) return;

            previousTab = currentTab;
            currentTab = tab;

            // Update tab colors with animation
            UpdateTabColors();

            // Determine slide direction
            bool goingRight = (int)tab > (int)previousTab;

            // Get panel references
            GameObject oldPanel = GetPanelForTab(previousTab);
            GameObject newPanel = GetPanelForTab(tab);

            // Animate transition
            tabTransitionSequence?.Kill();
            tabTransitionSequence = DOTween.Sequence();
            isTabTransitioning = true;

            // Fade out + slide old content
            if (oldPanel != null && oldPanel.activeSelf && previousTab != tab)
            {
                var oldCG = oldPanel.GetComponent<CanvasGroup>();
                if (oldCG == null) oldCG = oldPanel.AddComponent<CanvasGroup>();
                var oldRT = oldPanel.GetComponent<RectTransform>();
                Vector2 oldOrigPos = panelOriginalPositions.ContainsKey(previousTab) ? panelOriginalPositions[previousTab] : Vector2.zero;
                float exitDir = goingRight ? -30f : 30f;
                var capturedOldPanel = oldPanel;
                var capturedOldRT = oldRT;
                var capturedOldCG = oldCG;
                var capturedOldOrigPos = oldOrigPos;

                tabTransitionSequence
                    .Join(capturedOldCG.DOFade(0f, 0.15f))
                    .Join(capturedOldRT.DOAnchorPosX(capturedOldOrigPos.x + exitDir, 0.15f));

                tabTransitionSequence.InsertCallback(0.15f, () =>
                {
                    capturedOldPanel.SetActive(false);
                    capturedOldRT.anchoredPosition = capturedOldOrigPos;
                    capturedOldCG.alpha = 1f;
                });
            }

            // Fade in + slide new content
            if (newPanel != null)
            {
                var newCG = newPanel.GetComponent<CanvasGroup>();
                if (newCG == null) newCG = newPanel.AddComponent<CanvasGroup>();
                var newRT = newPanel.GetComponent<RectTransform>();
                Vector2 newOrigPos = panelOriginalPositions.ContainsKey(tab) ? panelOriginalPositions[tab] : Vector2.zero;
                float enterDir = goingRight ? 30f : -30f;

                newCG.alpha = 0f;
                newPanel.SetActive(true);
                newRT.anchoredPosition = newOrigPos + new Vector2(enterDir, 0);

                float enterDelay = (previousTab != tab && oldPanel != null && oldPanel != newPanel) ? 0.1f : 0f;

                tabTransitionSequence.Insert(enterDelay, newCG.DOFade(1f, 0.2f));
                tabTransitionSequence.Insert(enterDelay, newRT.DOAnchorPos(newOrigPos, 0.2f).SetEase(Ease.OutQuad));
            }

            // Hide other panels
            HideOtherPanels(tab, previousTab);

            tabTransitionSequence.OnComplete(() =>
            {
                isTabTransitioning = false;

                // Load content for tab after animation
                switch (tab)
                {
                    case WalletTab.Withdraw:
                        RefreshWithdrawPanel();
                        break;
                    case WalletTab.History:
                        LoadTransactionHistory(reset: true);
                        break;
                }
            });
        }

        private GameObject GetPanelForTab(WalletTab tab)
        {
            switch (tab)
            {
                case WalletTab.Deposit: return depositPanel;
                case WalletTab.Withdraw: return withdrawPanel;
                case WalletTab.History: return transactionHistoryPanel;
                default: return null;
            }
        }

        private void HideOtherPanels(WalletTab activeTab, WalletTab animatingTab)
        {
            WalletTab[] allTabs = { WalletTab.Deposit, WalletTab.Withdraw, WalletTab.History };
            foreach (var t in allTabs)
            {
                if (t != activeTab && t != animatingTab)
                {
                    var panel = GetPanelForTab(t);
                    if (panel != null) panel.SetActive(false);
                }
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
            if (image) image.DOColor(isActive ? activeTabColor : inactiveTabColor, 0.2f);

            var text = button.GetComponentInChildren<TextMeshProUGUI>();
            if (text) text.DOColor(isActive ? Color.white : new Color(0.7f, 0.7f, 0.7f), 0.2f);

            // Scale animation for active tab
            button.transform.DOScale(isActive ? 1.05f : 1f, 0.2f).SetEase(Ease.OutCubic);
        }

        // ==================== OVERLAY PANELS ====================

        private void ShowOverlayPanel(GameObject panel)
        {
            if (panel == null) return;

            // Hide any other open panel first
            if (depositPanel && depositPanel != panel && depositPanel.activeSelf)
                depositPanel.SetActive(false);
            if (withdrawPanel && withdrawPanel != panel && withdrawPanel.activeSelf)
                withdrawPanel.SetActive(false);

            panel.SetActive(true);
            var cg = panel.GetComponent<CanvasGroup>();
            if (cg == null) cg = panel.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.DOFade(1f, 0.25f).SetEase(Ease.OutQuad);

            // Scale punch on inner panel for premium feel
            var inner = panel.transform.Find("InnerPanel");
            if (inner != null)
            {
                inner.localScale = Vector3.one * 0.92f;
                inner.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
            }

            // Refresh content
            if (panel == withdrawPanel)
                RefreshWithdrawPanel();
        }

        private void HideOverlayPanel(GameObject panel)
        {
            if (panel == null || !panel.activeSelf) return;

            var cg = panel.GetComponent<CanvasGroup>();
            if (cg == null) cg = panel.AddComponent<CanvasGroup>();
            cg.DOFade(0f, 0.2f).OnComplete(() =>
            {
                panel.SetActive(false);
                cg.alpha = 1f;
            });
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
            Debug.Log($"[CashWalletScene] Deposit selected: ${option.amount}");

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
                withdrawMinText.text = AutoLocalizer.Get("wallet_minimum", $"${minimumWithdraw:F2}");
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
                    withdrawFeeText.text = fee > 0 ? AutoLocalizer.Get("wallet_fee", $"${fee:F2}") : AutoLocalizer.Get("wallet_no_fee");
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

            previousBalance = walletData.balance;

            if (bonusBalanceText)
            {
                // Mostrar balance pendiente como "bonus" o lifetime deposits
                bonusBalanceText.text = AutoLocalizer.Get("wallet_pending", $"${walletData.pendingBalance:F2}");
            }
        }

        // ==================== EVENT HANDLERS ====================

        private void OnBalanceChanged(decimal newBalance, decimal delta)
        {
            // Animar balance text cuando hay cambio
            if (balanceText != null && delta != 0)
            {
                balanceText.transform.DOKill();
                int startVal = (int)(previousBalance * 100);
                int endVal = (int)(newBalance * 100);
                DOTween.To(() => startVal, x => {
                    startVal = x;
                    balanceText.text = $"${x / 100f:F2}";
                }, endVal, 0.8f).SetEase(Ease.OutQuad).OnComplete(() => {
                    balanceText.text = $"${newBalance:F2}";
                });

                // Punch scale para feedback visual
                UIAnimations.TextPunch(balanceText.transform, 0.15f, 0.3f);

                // Actualizar bonus balance sin animación
                if (bonusBalanceText && WalletManager.Instance != null)
                {
                    bonusBalanceText.text = AutoLocalizer.Get("wallet_pending", $"${WalletManager.Instance.WalletData.pendingBalance:F2}");
                }
            }
            else
            {
                RefreshUI();
            }

            previousBalance = newBalance;
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
                ShowError(message ?? AutoLocalizer.Get("wallet_deposit_error"));
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
                if (show)
                {
                    loadingOverlay.SetActive(true);
                    var cg = loadingOverlay.GetComponent<CanvasGroup>();
                    if (cg == null) cg = loadingOverlay.AddComponent<CanvasGroup>();
                    cg.alpha = 0f;
                    cg.DOFade(1f, 0.2f).SetUpdate(true);
                }
                else
                {
                    var cg = loadingOverlay.GetComponent<CanvasGroup>();
                    if (cg != null)
                        cg.DOFade(0f, 0.2f).SetUpdate(true).OnComplete(() => loadingOverlay.SetActive(false));
                    else
                        loadingOverlay.SetActive(false);
                }
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
