using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DigitPark.Monetization;
using DigitPark.Localization;

namespace DigitPark.Monetization.Betting
{
    /// <summary>
    /// Manager de la escena BetSelection.
    /// Pantalla de seleccion de apuesta antes de Matchmaking.
    /// Flujo: GameSelector -> BetSelection -> Matchmaking -> Game
    /// Soporta apuestas preset (monedas/gemas) y personalizada (multiplos de 5).
    /// </summary>
    public class BetSelectionPanel : MonoBehaviour
    {
        [Header("=== NAVIGATION ===")]
        [SerializeField] private Button _backButton;

        [Header("=== HEADER ===")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _gameNameText;

        [Header("=== CURRENCY DISPLAY ===")]
        [SerializeField] private TextMeshProUGUI _gemsValueText;
        [SerializeField] private TextMeshProUGUI _coinsValueText;
        [SerializeField] private TextMeshProUGUI _gemsLabel;
        [SerializeField] private TextMeshProUGUI _coinsLabel;

        [Header("=== FREE BET ===")]
        [SerializeField] private Button _freeBetButton;
        [SerializeField] private TextMeshProUGUI _freeBetCostText;
        [SerializeField] private TextMeshProUGUI _freeBetRewardText;

        [Header("=== COIN BETS ===")]
        [SerializeField] private Button _coins50Button;
        [SerializeField] private TextMeshProUGUI _coins50CostText;
        [SerializeField] private TextMeshProUGUI _coins50RewardText;
        [SerializeField] private Button _coins100Button;
        [SerializeField] private TextMeshProUGUI _coins100CostText;
        [SerializeField] private TextMeshProUGUI _coins100RewardText;
        [SerializeField] private Button _coins250Button;
        [SerializeField] private TextMeshProUGUI _coins250CostText;
        [SerializeField] private TextMeshProUGUI _coins250RewardText;
        [SerializeField] private Button _coins500Button;
        [SerializeField] private TextMeshProUGUI _coins500CostText;
        [SerializeField] private TextMeshProUGUI _coins500RewardText;
        [SerializeField] private Button _coins1000Button;
        [SerializeField] private TextMeshProUGUI _coins1000CostText;
        [SerializeField] private TextMeshProUGUI _coins1000RewardText;

        [Header("=== GEM BETS ===")]
        [SerializeField] private Button _gems10Button;
        [SerializeField] private TextMeshProUGUI _gems10CostText;
        [SerializeField] private TextMeshProUGUI _gems10RewardText;
        [SerializeField] private Button _gems50Button;
        [SerializeField] private TextMeshProUGUI _gems50CostText;
        [SerializeField] private TextMeshProUGUI _gems50RewardText;
        [SerializeField] private Button _gems100Button;
        [SerializeField] private TextMeshProUGUI _gems100CostText;
        [SerializeField] private TextMeshProUGUI _gems100RewardText;
        [SerializeField] private Button _gems250Button;
        [SerializeField] private TextMeshProUGUI _gems250CostText;
        [SerializeField] private TextMeshProUGUI _gems250RewardText;
        [SerializeField] private Button _gems500Button;
        [SerializeField] private TextMeshProUGUI _gems500CostText;
        [SerializeField] private TextMeshProUGUI _gems500RewardText;

        [Header("=== CUSTOM BET ===")]
        [SerializeField] private Image _customBetCardBg;
        [SerializeField] private Button _customCoinsToggle;
        [SerializeField] private Button _customGemsToggle;
        [SerializeField] private TMP_InputField _customAmountInput;
        [SerializeField] private Button _customMinusButton;
        [SerializeField] private Button _customPlusButton;
        [SerializeField] private TextMeshProUGUI _customRewardText;

        [Header("=== ACTION BUTTONS ===")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _cancelButton;

        // PlayerPrefs keys
        private const string BET_AMOUNT_KEY = "DigitPark_BetAmount";
        private const string BET_CURRENCY_KEY = "DigitPark_BetCurrencyType";
        private const string GAME_TYPE_KEY = "MatchGameType";
        private const string IS_SPRINT_KEY = "MatchIsCognitiveSprint";

        // Preset selection state
        private int _selectedBetAmount = 0;
        private BetCurrencyType _selectedCurrencyType = BetCurrencyType.None;
        private Button _selectedButton;
        private bool _isCustomBetSelected = false;

        // Custom bet state
        private bool _customIsGems = false;
        private int _customAmount = 10;
        private const int STEP = 5;
        private const int MIN_CUSTOM = 5;

        // Visual colors
        private static readonly Color SEL_TINT = new Color(0f, 0.85f, 0.95f, 0.18f);
        private static readonly Color CARD_BG = new Color(0.06f, 0.08f, 0.14f, 1f);
        private static readonly Color CUSTOM_SEL = new Color(0.12f, 0.08f, 0.22f, 1f);
        private static readonly Color CUSTOM_UNSEL = new Color(0.05f, 0.05f, 0.1f, 1f);
        private static readonly Color TOGGLE_ON = new Color(0f, 0.8f, 1f, 0.3f);
        private static readonly Color TOGGLE_OFF = new Color(0.08f, 0.1f, 0.16f, 1f);

        private void Start()
        {
            SetupListeners();
            UpdateHeader();
            UpdateCurrencyDisplay();
            UpdateLabels();
            UpdateButtonStates();
            InitCustomBet();
            SelectPresetBet(_freeBetButton, 0, BetCurrencyType.None);
        }

        private void OnDestroy()
        {
            RemoveListeners();
        }

        #region Setup

        private void SetupListeners()
        {
            // Disable auto-navigation from BackButton prefab to prevent double listener
            var autoNav = _backButton?.GetComponent<DigitPark.UI.BackButton>();
            if (autoNav != null) autoNav.DisableAutoNavigation();
            _backButton?.onClick.AddListener(OnCancelClicked);

            _freeBetButton?.onClick.AddListener(() => SelectPresetBet(_freeBetButton, 0, BetCurrencyType.None));

            _coins50Button?.onClick.AddListener(() => SelectPresetBet(_coins50Button, 50, BetCurrencyType.Coins));
            _coins100Button?.onClick.AddListener(() => SelectPresetBet(_coins100Button, 100, BetCurrencyType.Coins));
            _coins250Button?.onClick.AddListener(() => SelectPresetBet(_coins250Button, 250, BetCurrencyType.Coins));
            _coins500Button?.onClick.AddListener(() => SelectPresetBet(_coins500Button, 500, BetCurrencyType.Coins));
            _coins1000Button?.onClick.AddListener(() => SelectPresetBet(_coins1000Button, 1000, BetCurrencyType.Coins));

            _gems10Button?.onClick.AddListener(() => SelectPresetBet(_gems10Button, 10, BetCurrencyType.Gems));
            _gems50Button?.onClick.AddListener(() => SelectPresetBet(_gems50Button, 50, BetCurrencyType.Gems));
            _gems100Button?.onClick.AddListener(() => SelectPresetBet(_gems100Button, 100, BetCurrencyType.Gems));
            _gems250Button?.onClick.AddListener(() => SelectPresetBet(_gems250Button, 250, BetCurrencyType.Gems));
            _gems500Button?.onClick.AddListener(() => SelectPresetBet(_gems500Button, 500, BetCurrencyType.Gems));

            _customCoinsToggle?.onClick.AddListener(() => SetCustomCurrency(false));
            _customGemsToggle?.onClick.AddListener(() => SetCustomCurrency(true));
            _customMinusButton?.onClick.AddListener(OnCustomMinus);
            _customPlusButton?.onClick.AddListener(OnCustomPlus);
            if (_customAmountInput != null)
                _customAmountInput.onEndEdit.AddListener(OnCustomAmountEdited);

            _playButton?.onClick.AddListener(OnPlayClicked);
            _cancelButton?.onClick.AddListener(OnCancelClicked);

            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnGemsChanged += OnCurrencyChanged;
                CurrencyManager.Instance.OnCoinsChanged += OnCurrencyChanged;
            }
        }

        private void RemoveListeners()
        {
            _backButton?.onClick.RemoveAllListeners();
            _freeBetButton?.onClick.RemoveAllListeners();
            _coins50Button?.onClick.RemoveAllListeners();
            _coins100Button?.onClick.RemoveAllListeners();
            _coins250Button?.onClick.RemoveAllListeners();
            _coins500Button?.onClick.RemoveAllListeners();
            _coins1000Button?.onClick.RemoveAllListeners();
            _gems10Button?.onClick.RemoveAllListeners();
            _gems50Button?.onClick.RemoveAllListeners();
            _gems100Button?.onClick.RemoveAllListeners();
            _gems250Button?.onClick.RemoveAllListeners();
            _gems500Button?.onClick.RemoveAllListeners();
            _customCoinsToggle?.onClick.RemoveAllListeners();
            _customGemsToggle?.onClick.RemoveAllListeners();
            _customMinusButton?.onClick.RemoveAllListeners();
            _customPlusButton?.onClick.RemoveAllListeners();
            if (_customAmountInput != null)
                _customAmountInput.onEndEdit.RemoveAllListeners();
            _playButton?.onClick.RemoveAllListeners();
            _cancelButton?.onClick.RemoveAllListeners();

            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnGemsChanged -= OnCurrencyChanged;
                CurrencyManager.Instance.OnCoinsChanged -= OnCurrencyChanged;
            }
        }

        #endregion

        #region UI Updates

        private void UpdateHeader()
        {
            if (_titleText != null)
                _titleText.text = AutoLocalizer.Get("bet_title");

            if (_gameNameText != null)
            {
                string gameTypeName = PlayerPrefs.GetString(GAME_TYPE_KEY, "");
                bool isSprint = PlayerPrefs.GetInt(IS_SPRINT_KEY, 0) == 1;

                if (isSprint)
                    _gameNameText.text = AutoLocalizer.Get("game_cognitive_sprint");
                else if (!string.IsNullOrEmpty(gameTypeName))
                    _gameNameText.text = gameTypeName;
                else
                    _gameNameText.text = "";
            }
        }

        private void UpdateLabels()
        {
            SetText(_freeBetCostText, AutoLocalizer.Get("bet_free"));
            SetText(_freeBetRewardText, AutoLocalizer.Get("bet_free_desc"));

            SetText(_coins50CostText, AutoLocalizer.Get("bet_coins_cost", "50"));
            SetText(_coins50RewardText, AutoLocalizer.Get("bet_coins_wager", "100"));
            SetText(_coins100CostText, AutoLocalizer.Get("bet_coins_cost", "100"));
            SetText(_coins100RewardText, AutoLocalizer.Get("bet_coins_wager", "200"));
            SetText(_coins250CostText, AutoLocalizer.Get("bet_coins_cost", "250"));
            SetText(_coins250RewardText, AutoLocalizer.Get("bet_coins_wager", "500"));
            SetText(_coins500CostText, AutoLocalizer.Get("bet_coins_cost", "500"));
            SetText(_coins500RewardText, AutoLocalizer.Get("bet_coins_wager", "1,000"));
            SetText(_coins1000CostText, AutoLocalizer.Get("bet_coins_cost", "1,000"));
            SetText(_coins1000RewardText, AutoLocalizer.Get("bet_coins_wager", "2,000"));

            SetText(_gems10CostText, AutoLocalizer.Get("bet_gems_cost", "10"));
            SetText(_gems10RewardText, AutoLocalizer.Get("bet_gems_wager", "20"));
            SetText(_gems50CostText, AutoLocalizer.Get("bet_gems_cost", "50"));
            SetText(_gems50RewardText, AutoLocalizer.Get("bet_gems_wager", "100"));
            SetText(_gems100CostText, AutoLocalizer.Get("bet_gems_cost", "100"));
            SetText(_gems100RewardText, AutoLocalizer.Get("bet_gems_wager", "200"));
            SetText(_gems250CostText, AutoLocalizer.Get("bet_gems_cost", "250"));
            SetText(_gems250RewardText, AutoLocalizer.Get("bet_gems_wager", "500"));
            SetText(_gems500CostText, AutoLocalizer.Get("bet_gems_cost", "500"));
            SetText(_gems500RewardText, AutoLocalizer.Get("bet_gems_wager", "1,000"));
        }

        private void UpdateCurrencyDisplay()
        {
            var currency = CurrencyManager.Instance;
            if (currency == null) return;
            SetText(_gemsValueText, currency.Gems.ToString("N0"));
            SetText(_coinsValueText, currency.Coins.ToString("N0"));
        }

        private void UpdateButtonStates()
        {
            var c = CurrencyManager.Instance;
            if (c == null) return;

            SetBtnState(_coins50Button, c.HasEnoughCoins(50));
            SetBtnState(_coins100Button, c.HasEnoughCoins(100));
            SetBtnState(_coins250Button, c.HasEnoughCoins(250));
            SetBtnState(_coins500Button, c.HasEnoughCoins(500));
            SetBtnState(_coins1000Button, c.HasEnoughCoins(1000));

            SetBtnState(_gems10Button, c.HasEnoughGems(10));
            SetBtnState(_gems50Button, c.HasEnoughGems(50));
            SetBtnState(_gems100Button, c.HasEnoughGems(100));
            SetBtnState(_gems250Button, c.HasEnoughGems(250));
            SetBtnState(_gems500Button, c.HasEnoughGems(500));
        }

        private void OnCurrencyChanged(int newAmount, int delta)
        {
            UpdateCurrencyDisplay();
            UpdateButtonStates();
            if (_isCustomBetSelected) UpdateCustomPreview();
        }

        #endregion

        #region Selection

        private void SelectPresetBet(Button button, int amount, BetCurrencyType type)
        {
            if (_selectedButton != null)
                HighlightCard(_selectedButton, false);

            _isCustomBetSelected = false;
            SetCustomHighlight(false);

            _selectedBetAmount = amount;
            _selectedCurrencyType = type;
            _selectedButton = button;

            if (button != null)
                HighlightCard(button, true);
        }

        private void ActivateCustomBet()
        {
            if (_selectedButton != null)
                HighlightCard(_selectedButton, false);
            _selectedButton = null;

            _isCustomBetSelected = true;
            _selectedBetAmount = _customAmount;
            _selectedCurrencyType = _customIsGems ? BetCurrencyType.Gems : BetCurrencyType.Coins;
            SetCustomHighlight(true);
        }

        private void HighlightCard(Button btn, bool on)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img == null) return;
            img.color = on
                ? new Color(CARD_BG.r + SEL_TINT.r, CARD_BG.g + SEL_TINT.g, CARD_BG.b + SEL_TINT.b, 1f)
                : CARD_BG;
        }

        private void SetCustomHighlight(bool on)
        {
            if (_customBetCardBg != null)
                _customBetCardBg.color = on ? CUSTOM_SEL : CUSTOM_UNSEL;
        }

        private void SetBtnState(Button btn, bool interactable)
        {
            if (btn == null) return;
            btn.interactable = interactable;
        }

        #endregion

        #region Custom Bet

        private void InitCustomBet()
        {
            _customIsGems = false;
            _customAmount = 10;
            UpdateCustomToggles();
            UpdateCustomInput();
            UpdateCustomPreview();
            if (_customAmountInput != null)
            {
                _customAmountInput.contentType = TMP_InputField.ContentType.IntegerNumber;
                _customAmountInput.characterLimit = 6;
            }
        }

        private void SetCustomCurrency(bool isGems)
        {
            _customIsGems = isGems;
            ClampCustom();
            UpdateCustomToggles();
            UpdateCustomInput();
            UpdateCustomPreview();
            ActivateCustomBet();
        }

        private void OnCustomMinus()
        {
            _customAmount = Mathf.Max(MIN_CUSTOM, _customAmount - STEP);
            UpdateCustomInput();
            UpdateCustomPreview();
            ActivateCustomBet();
        }

        private void OnCustomPlus()
        {
            _customAmount += STEP;
            ClampCustom();
            UpdateCustomInput();
            UpdateCustomPreview();
            ActivateCustomBet();
        }

        private void OnCustomAmountEdited(string value)
        {
            if (int.TryParse(value, out int v))
                _customAmount = Snap5(Mathf.Max(MIN_CUSTOM, v));
            else
                _customAmount = MIN_CUSTOM;
            ClampCustom();
            UpdateCustomInput();
            UpdateCustomPreview();
            ActivateCustomBet();
        }

        private void ClampCustom()
        {
            int max = MaxCustom();
            _customAmount = Mathf.Clamp(_customAmount, MIN_CUSTOM, Mathf.Max(MIN_CUSTOM, max));
        }

        private int MaxCustom()
        {
            var c = CurrencyManager.Instance;
            if (c == null) return MIN_CUSTOM;
            int bal = _customIsGems ? c.Gems : c.Coins;
            return (bal / STEP) * STEP;
        }

        private int Snap5(int v)
        {
            return Mathf.Max(MIN_CUSTOM, Mathf.RoundToInt(v / (float)STEP) * STEP);
        }

        private void UpdateCustomToggles()
        {
            SetToggleVisual(_customCoinsToggle, !_customIsGems);
            SetToggleVisual(_customGemsToggle, _customIsGems);
        }

        private void SetToggleVisual(Button btn, bool active)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null) img.color = active ? TOGGLE_ON : TOGGLE_OFF;
        }

        private void UpdateCustomInput()
        {
            if (_customAmountInput != null)
                _customAmountInput.text = _customAmount.ToString();
        }

        private void UpdateCustomPreview()
        {
            if (_customRewardText == null) return;
            int reward = _customAmount * 2;
            string curr = _customIsGems
                ? AutoLocalizer.Get("currency_gems")
                : AutoLocalizer.Get("currency_coins");
            _customRewardText.text = AutoLocalizer.Get("bet_custom_reward", reward.ToString("N0"), curr);
        }

        #endregion

        #region Actions

        private void OnPlayClicked()
        {
            if (_isCustomBetSelected)
            {
                _selectedBetAmount = _customAmount;
                _selectedCurrencyType = _customIsGems ? BetCurrencyType.Gems : BetCurrencyType.Coins;
            }

            if (_selectedBetAmount > 0)
            {
                var currency = CurrencyManager.Instance;
                if (currency == null) return;

                bool ok = false;
                switch (_selectedCurrencyType)
                {
                    case BetCurrencyType.Coins:
                        ok = currency.EscrowCoins(_selectedBetAmount);
                        break;
                    case BetCurrencyType.Gems:
                        ok = currency.EscrowGems(_selectedBetAmount);
                        break;
                    default:
                        ok = true;
                        break;
                }

                if (!ok)
                {
                    Debug.LogWarning("[BetSelection] Not enough currency for bet");
                    return;
                }
            }

            PlayerPrefs.SetInt(BET_AMOUNT_KEY, _selectedBetAmount);
            PlayerPrefs.SetInt(BET_CURRENCY_KEY, (int)_selectedCurrencyType);
            PlayerPrefs.Save();

            SceneManager.LoadScene("Matchmaking");
        }

        private void OnCancelClicked()
        {
            PlayerPrefs.SetInt(BET_AMOUNT_KEY, 0);
            PlayerPrefs.SetInt(BET_CURRENCY_KEY, 0);
            PlayerPrefs.Save();

            SceneManager.LoadScene("GameSelector");
        }

        #endregion

        #region Static API

        public static int GetStoredBetAmount()
        {
            return PlayerPrefs.GetInt(BET_AMOUNT_KEY, 0);
        }

        public static BetCurrencyType GetStoredBetCurrencyType()
        {
            return (BetCurrencyType)PlayerPrefs.GetInt(BET_CURRENCY_KEY, 0);
        }

        public static void ClearStoredBet()
        {
            PlayerPrefs.SetInt(BET_AMOUNT_KEY, 0);
            PlayerPrefs.SetInt(BET_CURRENCY_KEY, 0);
            PlayerPrefs.Save();
        }

        #endregion

        private static void SetText(TextMeshProUGUI tmp, string text)
        {
            if (tmp != null) tmp.text = text;
        }
    }
}
