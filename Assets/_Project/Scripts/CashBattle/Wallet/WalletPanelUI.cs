using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using DigitPark.Localization;

namespace DigitPark.CashBattle
{
    /// <summary>
    /// Panel visual del Wallet para Cash Battle.
    /// Maneja toda la UI de depósitos, retiros e historial.
    /// </summary>
    public class WalletPanelUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Header")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _balanceText;
        [SerializeField] private TextMeshProUGUI _balanceLabelText;
        [SerializeField] private Button _closeButton;

        [Header("Tab Navigation")]
        [SerializeField] private Button _depositTabButton;
        [SerializeField] private Button _withdrawTabButton;
        [SerializeField] private Button _historyTabButton;
        [SerializeField] private Image _depositTabIndicator;
        [SerializeField] private Image _withdrawTabIndicator;
        [SerializeField] private Image _historyTabIndicator;

        [Header("Deposit Panel")]
        [SerializeField] private GameObject _depositPanel;
        [SerializeField] private Transform _depositOptionsContainer;
        [SerializeField] private GameObject _depositOptionPrefab;
        [SerializeField] private TMP_InputField _customAmountInput;
        [SerializeField] private Button _customAmountButton;

        [Header("Payment Methods")]
        [SerializeField] private Button _creditCardButton;
        [SerializeField] private Button _paypalButton;
        [SerializeField] private Button _applePayButton;
        [SerializeField] private Button _googlePayButton;
        [SerializeField] private GameObject _paymentMethodsPanel;

        [Header("Withdraw Panel")]
        [SerializeField] private GameObject _withdrawPanel;
        [SerializeField] private TextMeshProUGUI _availableBalanceText;
        [SerializeField] private TMP_InputField _withdrawAmountInput;
        [SerializeField] private Button _withdrawButton;
        [SerializeField] private TextMeshProUGUI _withdrawInfoText;
        [SerializeField] private GameObject _kycRequiredPanel;
        [SerializeField] private Button _verifyButton;

        [Header("History Panel")]
        [SerializeField] private GameObject _historyPanel;
        [SerializeField] private Transform _historyContainer;
        [SerializeField] private GameObject _transactionItemPrefab;
        [SerializeField] private TextMeshProUGUI _noTransactionsText;
        [SerializeField] private ScrollRect _historyScrollRect;

        [Header("Loading & Status")]
        [SerializeField] private GameObject _loadingOverlay;
        [SerializeField] private TextMeshProUGUI _loadingText;
        [SerializeField] private GameObject _successOverlay;
        [SerializeField] private TextMeshProUGUI _successText;
        [SerializeField] private GameObject _errorOverlay;
        [SerializeField] private TextMeshProUGUI _errorText;

        [Header("Stats Section")]
        [SerializeField] private TextMeshProUGUI _lifetimeDepositsText;
        [SerializeField] private TextMeshProUGUI _lifetimeWithdrawalsText;
        [SerializeField] private TextMeshProUGUI _netWinningsText;

        [Header("Colors")]
        [SerializeField] private Color _activeTabColor = new Color(0f, 0.83f, 1f, 1f);
        [SerializeField] private Color _inactiveTabColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        [SerializeField] private Color _positiveColor = new Color(0f, 1f, 0.5f, 1f);
        [SerializeField] private Color _negativeColor = new Color(1f, 0.4f, 0.4f, 1f);

        [Header("Animation")]
        [SerializeField] private float _fadeInDuration = 0.3f;
        [SerializeField] private float _statusDisplayDuration = 2f;

        // Estado
        private WalletTab _currentTab = WalletTab.Deposit;
        private decimal _selectedDepositAmount;
        private PaymentMethod _selectedPaymentMethod = PaymentMethod.CreditCard;
        private List<GameObject> _depositOptionInstances = new List<GameObject>();
        private List<GameObject> _transactionInstances = new List<GameObject>();

        // Eventos
        public event Action OnCloseRequested;

        private void Awake()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            SetupListeners();
            SubscribeToWalletEvents();
            RefreshUI();
            SwitchToTab(WalletTab.Deposit);
        }

        private void OnDestroy()
        {
            UnsubscribeFromWalletEvents();
        }

        private void OnEnable()
        {
            RefreshUI();
        }

        // ==================== SETUP ====================

        private void SetupListeners()
        {
            // Close
            _closeButton?.onClick.AddListener(OnCloseClicked);

            // Tabs
            _depositTabButton?.onClick.AddListener(() => SwitchToTab(WalletTab.Deposit));
            _withdrawTabButton?.onClick.AddListener(() => SwitchToTab(WalletTab.Withdraw));
            _historyTabButton?.onClick.AddListener(() => SwitchToTab(WalletTab.History));

            // Payment methods
            _creditCardButton?.onClick.AddListener(() => SelectPaymentMethod(PaymentMethod.CreditCard));
            _paypalButton?.onClick.AddListener(() => SelectPaymentMethod(PaymentMethod.PayPal));
            _applePayButton?.onClick.AddListener(() => SelectPaymentMethod(PaymentMethod.ApplePay));
            _googlePayButton?.onClick.AddListener(() => SelectPaymentMethod(PaymentMethod.GooglePay));

            // Custom amount
            _customAmountButton?.onClick.AddListener(OnCustomAmountClicked);

            // Withdraw
            _withdrawButton?.onClick.AddListener(OnWithdrawClicked);
            _verifyButton?.onClick.AddListener(OnVerifyClicked);

            // Input validation
            _withdrawAmountInput?.onValueChanged.AddListener(OnWithdrawAmountChanged);
            _customAmountInput?.onValueChanged.AddListener(OnCustomAmountChanged);
        }

        private void SubscribeToWalletEvents()
        {
            if (WalletManager.Instance != null)
            {
                WalletManager.Instance.OnBalanceChanged += OnBalanceChanged;
                WalletManager.Instance.OnDepositStarted += OnDepositStarted;
                WalletManager.Instance.OnDepositCompleted += OnDepositCompleted;
                WalletManager.Instance.OnTransactionCompleted += OnTransactionCompleted;
                WalletManager.Instance.OnKYCRequired += OnKYCRequired;
            }
        }

        private void UnsubscribeFromWalletEvents()
        {
            if (WalletManager.Instance != null)
            {
                WalletManager.Instance.OnBalanceChanged -= OnBalanceChanged;
                WalletManager.Instance.OnDepositStarted -= OnDepositStarted;
                WalletManager.Instance.OnDepositCompleted -= OnDepositCompleted;
                WalletManager.Instance.OnTransactionCompleted -= OnTransactionCompleted;
                WalletManager.Instance.OnKYCRequired -= OnKYCRequired;
            }
        }

        // ==================== PUBLIC METHODS ====================

        /// <summary>
        /// Muestra el panel con animación
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            RefreshUI();
            StartCoroutine(FadeIn());
            Debug.Log("[WalletPanelUI] Panel mostrado");
        }

        /// <summary>
        /// Oculta el panel con animación
        /// </summary>
        public void Hide()
        {
            StartCoroutine(FadeOutAndHide());
        }

        /// <summary>
        /// Refresca toda la UI
        /// </summary>
        public void RefreshUI()
        {
            UpdateBalanceDisplay();
            UpdateStatsDisplay();
            PopulateDepositOptions();
            UpdateWithdrawPanel();
            PopulateHistory();
        }

        // ==================== TAB NAVIGATION ====================

        private void SwitchToTab(WalletTab tab)
        {
            _currentTab = tab;

            // Ocultar todos los paneles
            _depositPanel?.SetActive(false);
            _withdrawPanel?.SetActive(false);
            _historyPanel?.SetActive(false);

            // Resetear indicadores
            SetTabIndicator(_depositTabIndicator, false);
            SetTabIndicator(_withdrawTabIndicator, false);
            SetTabIndicator(_historyTabIndicator, false);

            // Mostrar panel seleccionado
            switch (tab)
            {
                case WalletTab.Deposit:
                    _depositPanel?.SetActive(true);
                    SetTabIndicator(_depositTabIndicator, true);
                    PopulateDepositOptions();
                    break;

                case WalletTab.Withdraw:
                    _withdrawPanel?.SetActive(true);
                    SetTabIndicator(_withdrawTabIndicator, true);
                    UpdateWithdrawPanel();
                    break;

                case WalletTab.History:
                    _historyPanel?.SetActive(true);
                    SetTabIndicator(_historyTabIndicator, true);
                    PopulateHistory();
                    break;
            }

            Debug.Log($"[WalletPanelUI] Tab cambiado a: {tab}");
        }

        private void SetTabIndicator(Image indicator, bool active)
        {
            if (indicator != null)
            {
                indicator.color = active ? _activeTabColor : _inactiveTabColor;
                indicator.gameObject.SetActive(active);
            }
        }

        // ==================== BALANCE DISPLAY ====================

        private void UpdateBalanceDisplay()
        {
            if (WalletManager.Instance == null) return;

            if (_balanceText != null)
            {
                _balanceText.text = WalletManager.Instance.GetFormattedBalance();
            }

            if (_availableBalanceText != null)
            {
                _availableBalanceText.text = WalletManager.Instance.GetFormattedAvailableBalance();
            }
        }

        private void UpdateStatsDisplay()
        {
            if (WalletManager.Instance == null) return;

            var stats = WalletManager.Instance.GetStats();

            if (_lifetimeDepositsText != null)
                _lifetimeDepositsText.text = $"${stats.deposits:F2}";

            if (_lifetimeWithdrawalsText != null)
                _lifetimeWithdrawalsText.text = $"${stats.withdrawals:F2}";

            if (_netWinningsText != null)
            {
                _netWinningsText.text = $"${stats.net:F2}";
                _netWinningsText.color = stats.net >= 0 ? _positiveColor : _negativeColor;
            }
        }

        // ==================== DEPOSIT ====================

        private void PopulateDepositOptions()
        {
            if (_depositOptionsContainer == null || WalletManager.Instance == null) return;

            // Limpiar opciones existentes
            foreach (var instance in _depositOptionInstances)
            {
                if (instance != null) Destroy(instance);
            }
            _depositOptionInstances.Clear();

            // Crear nuevas opciones
            foreach (var option in WalletManager.Instance.DepositOptions)
            {
                CreateDepositOptionUI(option);
            }
        }

        private void CreateDepositOptionUI(DepositOption option)
        {
            GameObject optionObj;

            if (_depositOptionPrefab != null)
            {
                optionObj = Instantiate(_depositOptionPrefab, _depositOptionsContainer);
            }
            else
            {
                // Crear por código si no hay prefab
                optionObj = CreateDepositOptionFallback(option);
            }

            _depositOptionInstances.Add(optionObj);

            // Configurar componente
            var optionUI = optionObj.GetComponent<DepositOptionUI>();
            if (optionUI != null)
            {
                optionUI.Setup(option, OnDepositOptionSelected);
            }
            else
            {
                // Fallback: configurar botón directamente
                var button = optionObj.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() => OnDepositOptionSelected(option));
                }
            }
        }

        private GameObject CreateDepositOptionFallback(DepositOption option)
        {
            GameObject obj = new GameObject($"DepositOption_{option.amount}");
            obj.transform.SetParent(_depositOptionsContainer, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200, 80);

            Image bg = obj.AddComponent<Image>();
            bg.color = option.isPopular ? new Color(1f, 0.84f, 0f, 0.2f) : new Color(0.15f, 0.2f, 0.25f, 0.95f);

            Button btn = obj.AddComponent<Button>();
            btn.targetGraphic = bg;

            // Texto
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);

            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = option.GetDisplayText();
            tmp.fontSize = 18;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            return obj;
        }

        private void OnDepositOptionSelected(DepositOption option)
        {
            _selectedDepositAmount = option.amount;
            Debug.Log($"[WalletPanelUI] Opción de depósito seleccionada: ${option.amount:F2}");

            // Mostrar panel de métodos de pago
            ShowPaymentMethods(option);
        }

        private void ShowPaymentMethods(DepositOption option)
        {
            if (_paymentMethodsPanel != null)
            {
                _paymentMethodsPanel.SetActive(true);
            }

            // Configurar los botones de pago para procesar este monto
            ConfigurePaymentButtons(option);
        }

        private void ConfigurePaymentButtons(DepositOption option)
        {
            // Los botones ya tienen listeners, solo necesitamos guardar la opción seleccionada
            _selectedDepositAmount = option.amount;
        }

        private void SelectPaymentMethod(PaymentMethod method)
        {
            _selectedPaymentMethod = method;
            ProcessDeposit();
        }

        private async void ProcessDeposit()
        {
            if (_selectedDepositAmount <= 0)
            {
                ShowError(AutoLocalizer.Get("wallet_select_amount"));
                return;
            }

            Debug.Log($"[WalletPanelUI] Procesando depósito: ${_selectedDepositAmount:F2} vía {_selectedPaymentMethod}");

            var option = WalletManager.Instance.DepositOptions.Find(o => o.amount == _selectedDepositAmount);
            decimal bonus = option?.bonus ?? 0;

            bool success = await WalletManager.Instance.InitiateDeposit(_selectedDepositAmount, bonus, _selectedPaymentMethod);

            if (!success)
            {
                ShowError(AutoLocalizer.Get("wallet_deposit_error"));
            }
        }

        private void OnCustomAmountClicked()
        {
            if (_customAmountInput == null) return;

            string amountStr = _customAmountInput.text;
            if (decimal.TryParse(amountStr, out decimal amount) && amount > 0)
            {
                _selectedDepositAmount = amount;
                ShowPaymentMethods(new DepositOption { amount = amount, bonus = 0 });
            }
            else
            {
                ShowError(AutoLocalizer.Get("wallet_invalid_amount"));
            }
        }

        private void OnCustomAmountChanged(string value)
        {
            // Validación en tiempo real si se desea
        }

        // ==================== WITHDRAW ====================

        private void UpdateWithdrawPanel()
        {
            if (WalletManager.Instance == null) return;

            // Actualizar balance disponible
            if (_availableBalanceText != null)
            {
                _availableBalanceText.text = WalletManager.Instance.GetFormattedAvailableBalance();
            }

            // Mostrar info de retiro
            if (_withdrawInfoText != null)
            {
                _withdrawInfoText.text = $"Mínimo: ${WalletManager.Instance.MinimumWithdrawal:F2}\nMáximo: ${WalletManager.Instance.MaximumWithdrawal:F2}\nProcesamiento: 3-5 días hábiles";
            }

            // Mostrar/ocultar panel de KYC
            bool needsKYC = !WalletManager.Instance.IsVerified;
            if (_kycRequiredPanel != null)
            {
                _kycRequiredPanel.SetActive(needsKYC);
            }

            if (_withdrawButton != null)
            {
                _withdrawButton.interactable = !needsKYC;
            }
        }

        private void OnWithdrawAmountChanged(string value)
        {
            // Validación en tiempo real
            if (decimal.TryParse(value, out decimal amount))
            {
                bool isValid = amount >= WalletManager.Instance.MinimumWithdrawal &&
                              amount <= WalletManager.Instance.MaximumWithdrawal &&
                              amount <= WalletManager.Instance.AvailableBalance;

                if (_withdrawButton != null)
                {
                    _withdrawButton.interactable = isValid && WalletManager.Instance.IsVerified;
                }
            }
        }

        private async void OnWithdrawClicked()
        {
            if (_withdrawAmountInput == null) return;

            if (!decimal.TryParse(_withdrawAmountInput.text, out decimal amount))
            {
                ShowError(AutoLocalizer.Get("wallet_invalid_amount"));
                return;
            }

            ShowLoading("Procesando retiro...");

            bool success = await WalletManager.Instance.RequestWithdrawal(amount, _selectedPaymentMethod);

            HideLoading();

            if (success)
            {
                ShowSuccess($"Retiro de ${amount:F2} solicitado");
                _withdrawAmountInput.text = "";
                UpdateWithdrawPanel();
            }
            else
            {
                ShowError(AutoLocalizer.Get("wallet_withdrawal_error"));
            }
        }

        private async void OnVerifyClicked()
        {
            ShowLoading("Iniciando verificación...");

            bool success = await WalletManager.Instance.StartKYCVerification();

            HideLoading();

            if (success)
            {
                ShowSuccess("Verificación completada");
                UpdateWithdrawPanel();
            }
            else
            {
                ShowError(AutoLocalizer.Get("wallet_verification_error"));
            }
        }

        // ==================== HISTORY ====================

        private void PopulateHistory()
        {
            if (_historyContainer == null || WalletManager.Instance == null) return;

            // Limpiar historial existente
            foreach (var instance in _transactionInstances)
            {
                if (instance != null) Destroy(instance);
            }
            _transactionInstances.Clear();

            var transactions = WalletManager.Instance.GetTransactionHistory(50);

            if (transactions.Count == 0)
            {
                if (_noTransactionsText != null)
                {
                    _noTransactionsText.gameObject.SetActive(true);
                    _noTransactionsText.text = AutoLocalizer.Get("wallet_no_transactions");
                }
                return;
            }

            if (_noTransactionsText != null)
            {
                _noTransactionsText.gameObject.SetActive(false);
            }

            foreach (var transaction in transactions)
            {
                CreateTransactionItemUI(transaction);
            }
        }

        private void CreateTransactionItemUI(WalletTransaction transaction)
        {
            GameObject itemObj;

            if (_transactionItemPrefab != null)
            {
                itemObj = Instantiate(_transactionItemPrefab, _historyContainer);
            }
            else
            {
                itemObj = CreateTransactionItemFallback(transaction);
            }

            _transactionInstances.Add(itemObj);

            var itemUI = itemObj.GetComponent<TransactionItemUI>();
            if (itemUI != null)
            {
                itemUI.Setup(transaction);
            }
        }

        private GameObject CreateTransactionItemFallback(WalletTransaction transaction)
        {
            GameObject obj = new GameObject($"Transaction_{transaction.transactionId.Substring(0, 8)}");
            obj.transform.SetParent(_historyContainer, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 60);

            LayoutElement le = obj.AddComponent<LayoutElement>();
            le.preferredHeight = 60;
            le.flexibleWidth = 1;

            HorizontalLayoutGroup hlg = obj.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(15, 15, 10, 10);
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            Image bg = obj.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.12f, 0.15f, 0.95f);

            // Description
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(obj.transform, false);
            TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = $"{transaction.description}\n<size=12><color=#888>{transaction.GetFormattedDate()}</color></size>";
            descText.fontSize = 14;
            descText.color = Color.white;
            var descLE = descObj.AddComponent<LayoutElement>();
            descLE.flexibleWidth = 1;

            // Amount
            GameObject amtObj = new GameObject("Amount");
            amtObj.transform.SetParent(obj.transform, false);
            TextMeshProUGUI amtText = amtObj.AddComponent<TextMeshProUGUI>();
            amtText.text = transaction.GetFormattedAmount();
            amtText.fontSize = 16;
            amtText.fontStyle = FontStyles.Bold;
            amtText.color = transaction.GetTypeColor();
            amtText.alignment = TextAlignmentOptions.Right;
            var amtLE = amtObj.AddComponent<LayoutElement>();
            amtLE.preferredWidth = 80;

            return obj;
        }

        // ==================== WALLET EVENTS ====================

        private void OnBalanceChanged(decimal newBalance, decimal delta)
        {
            UpdateBalanceDisplay();
            UpdateStatsDisplay();
        }

        private void OnDepositStarted()
        {
            ShowLoading("Procesando depósito...");
        }

        private void OnDepositCompleted(bool success, string message)
        {
            HideLoading();

            if (success)
            {
                ShowSuccess(message);
                RefreshUI();
            }
            else
            {
                ShowError(message);
            }
        }

        private void OnTransactionCompleted(WalletTransaction transaction)
        {
            // Refrescar historial si estamos en esa tab
            if (_currentTab == WalletTab.History)
            {
                PopulateHistory();
            }
        }

        private void OnKYCRequired()
        {
            SwitchToTab(WalletTab.Withdraw);
            if (_kycRequiredPanel != null)
            {
                _kycRequiredPanel.SetActive(true);
            }
        }

        // ==================== STATUS OVERLAYS ====================

        private void ShowLoading(string message)
        {
            if (_loadingOverlay != null)
            {
                _loadingOverlay.SetActive(true);
                if (_loadingText != null)
                    _loadingText.text = message;
            }
        }

        private void HideLoading()
        {
            if (_loadingOverlay != null)
            {
                _loadingOverlay.SetActive(false);
            }
        }

        private void ShowSuccess(string message)
        {
            if (_successOverlay != null)
            {
                _successOverlay.SetActive(true);
                if (_successText != null)
                    _successText.text = message;

                StartCoroutine(HideAfterDelay(_successOverlay, _statusDisplayDuration));
            }
        }

        private void ShowError(string message)
        {
            if (_errorOverlay != null)
            {
                _errorOverlay.SetActive(true);
                if (_errorText != null)
                    _errorText.text = message;

                StartCoroutine(HideAfterDelay(_errorOverlay, _statusDisplayDuration));
            }
        }

        private IEnumerator HideAfterDelay(GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (obj != null)
                obj.SetActive(false);
        }

        // ==================== ANIMATION ====================

        private IEnumerator FadeIn()
        {
            if (_canvasGroup == null) yield break;

            _canvasGroup.alpha = 0f;
            float elapsed = 0f;

            while (elapsed < _fadeInDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / _fadeInDuration);
                yield return null;
            }

            _canvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOutAndHide()
        {
            if (_canvasGroup == null)
            {
                gameObject.SetActive(false);
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < _fadeInDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / _fadeInDuration);
                yield return null;
            }

            _canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        // ==================== CLOSE ====================

        private void OnCloseClicked()
        {
            OnCloseRequested?.Invoke();
            Hide();
        }

        // ==================== EDITOR HELPERS ====================

#if UNITY_EDITOR
        public void AutoSetupReferences()
        {
            // Header
            _titleText = transform.Find("Header/Title")?.GetComponent<TextMeshProUGUI>();
            _balanceText = transform.Find("Header/Balance/Amount")?.GetComponent<TextMeshProUGUI>();
            _closeButton = transform.Find("Header/CloseButton")?.GetComponent<Button>();

            // Tabs
            Transform tabsPanel = transform.Find("TabsPanel");
            if (tabsPanel != null)
            {
                _depositTabButton = tabsPanel.Find("DepositTab")?.GetComponent<Button>();
                _withdrawTabButton = tabsPanel.Find("WithdrawTab")?.GetComponent<Button>();
                _historyTabButton = tabsPanel.Find("HistoryTab")?.GetComponent<Button>();
            }

            // Panels
            _depositPanel = transform.Find("Content/DepositPanel")?.gameObject;
            _withdrawPanel = transform.Find("Content/WithdrawPanel")?.gameObject;
            _historyPanel = transform.Find("Content/HistoryPanel")?.gameObject;

            // Deposit
            _depositOptionsContainer = transform.Find("Content/DepositPanel/OptionsContainer");

            // Withdraw
            _withdrawAmountInput = transform.Find("Content/WithdrawPanel/AmountInput")?.GetComponent<TMP_InputField>();
            _withdrawButton = transform.Find("Content/WithdrawPanel/WithdrawButton")?.GetComponent<Button>();

            // History
            _historyContainer = transform.Find("Content/HistoryPanel/ScrollView/Viewport/Content");

            // Overlays
            _loadingOverlay = transform.Find("LoadingOverlay")?.gameObject;
            _successOverlay = transform.Find("SuccessOverlay")?.gameObject;
            _errorOverlay = transform.Find("ErrorOverlay")?.gameObject;

            Debug.Log("[WalletPanelUI] Referencias configuradas automáticamente");
        }
#endif
    }

    /// <summary>
    /// Tabs del wallet
    /// </summary>
    public enum WalletTab
    {
        Deposit,
        Withdraw,
        History
    }
}
