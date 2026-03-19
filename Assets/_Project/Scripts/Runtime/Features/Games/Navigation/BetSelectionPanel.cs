using System;
using System.Collections.Generic;
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
    /// Soporta apuestas preset (DigitCoins) y personalizada (multiplos de 5).
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

        [Header("=== DIGITCOIN BETS ===")]
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

        [Header("=== CUSTOM BET ===")]
        [SerializeField] private Image _customBetCardBg;
        [SerializeField] private Button _customCoinsToggle;
        [SerializeField] private TMP_InputField _customAmountInput;
        [SerializeField] private Button _customMinusButton;
        [SerializeField] private Button _customPlusButton;
        [SerializeField] private TextMeshProUGUI _customRewardText;

        [Header("=== ROUNDS SELECTION ===")]
        [SerializeField] private GameObject _roundsPanel;
        [SerializeField] private Button _rounds1Button;
        [SerializeField] private Button _rounds3Button;
        [SerializeField] private Button _rounds5Button;

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
        private int _customAmount = 10;
        private int _selectedRounds = 1;
        private const int STEP = 5;
        private const int MIN_CUSTOM = 5;
        private const int MAX_CUSTOM_BET = 5000; // Economy Rebalance: cap custom bets

        // Visual colors
        private static readonly Color CARD_BG = new Color(0.06f, 0.08f, 0.14f, 1f);
        private static readonly Color CARD_SEL = new Color(0.10f, 0.22f, 0.34f, 1f);
        private static readonly Color SEL_OUTLINE = new Color(0f, 1f, 1f, 0.7f);
        private static readonly Color CUSTOM_SEL = new Color(0.12f, 0.08f, 0.22f, 1f);
        private static readonly Color CUSTOM_UNSEL = new Color(0.05f, 0.05f, 0.1f, 1f);
        private static readonly Color TOGGLE_ON = new Color(0f, 0.8f, 1f, 0.3f);
        private static readonly Color TOGGLE_OFF = new Color(0.08f, 0.1f, 0.16f, 1f);

        // Store original outline state per card for restore on deselect
        private Dictionary<Button, (Color color, Vector2 distance)> _originalOutlines = new Dictionary<Button, (Color, Vector2)>();

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

            _coins50Button?.onClick.AddListener(() => SelectPresetBet(_coins50Button, 50, BetCurrencyType.DigitCoins));
            _coins100Button?.onClick.AddListener(() => SelectPresetBet(_coins100Button, 100, BetCurrencyType.DigitCoins));
            _coins250Button?.onClick.AddListener(() => SelectPresetBet(_coins250Button, 250, BetCurrencyType.DigitCoins));
            _coins500Button?.onClick.AddListener(() => SelectPresetBet(_coins500Button, 500, BetCurrencyType.DigitCoins));
            _coins1000Button?.onClick.AddListener(() => SelectPresetBet(_coins1000Button, 1000, BetCurrencyType.DigitCoins));

            _customCoinsToggle?.onClick.AddListener(() => ActivateCustomBet());
            _customMinusButton?.onClick.AddListener(OnCustomMinus);
            _customPlusButton?.onClick.AddListener(OnCustomPlus);
            if (_customAmountInput != null)
                _customAmountInput.onEndEdit.AddListener(OnCustomAmountEdited);

            // Rounds selection
            _rounds1Button?.onClick.AddListener(() => SelectRounds(1));
            _rounds3Button?.onClick.AddListener(() => SelectRounds(3));
            _rounds5Button?.onClick.AddListener(() => SelectRounds(5));
            SelectRounds(1); // Default

            _playButton?.onClick.AddListener(OnPlayClicked);
            _cancelButton?.onClick.AddListener(OnCancelClicked);

            if (CurrencyManager.Instance != null)
            {
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
            _customCoinsToggle?.onClick.RemoveAllListeners();
            _customMinusButton?.onClick.RemoveAllListeners();
            _customPlusButton?.onClick.RemoveAllListeners();
            if (_customAmountInput != null)
                _customAmountInput.onEndEdit.RemoveAllListeners();
            _playButton?.onClick.RemoveAllListeners();
            _cancelButton?.onClick.RemoveAllListeners();

            if (CurrencyManager.Instance != null)
            {
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

            // Economy Rebalance: 1.9x multiplier (5% rake)
            SetText(_coins50CostText, AutoLocalizer.Get("bet_coins_cost", "50"));
            SetText(_coins50RewardText, AutoLocalizer.Get("bet_coins_wager", "95"));
            SetText(_coins100CostText, AutoLocalizer.Get("bet_coins_cost", "100"));
            SetText(_coins100RewardText, AutoLocalizer.Get("bet_coins_wager", "190"));
            SetText(_coins250CostText, AutoLocalizer.Get("bet_coins_cost", "250"));
            SetText(_coins250RewardText, AutoLocalizer.Get("bet_coins_wager", "475"));
            SetText(_coins500CostText, AutoLocalizer.Get("bet_coins_cost", "500"));
            SetText(_coins500RewardText, AutoLocalizer.Get("bet_coins_wager", "950"));
            SetText(_coins1000CostText, AutoLocalizer.Get("bet_coins_cost", "1,000"));
            SetText(_coins1000RewardText, AutoLocalizer.Get("bet_coins_wager", "1,900"));

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
            _selectedCurrencyType = BetCurrencyType.DigitCoins;
            SetCustomHighlight(true);
        }

        private void HighlightCard(Button btn, bool on)
        {
            if (btn == null) return;

            // Background tint
            var img = btn.GetComponent<Image>();
            if (img != null)
                img.color = on ? CARD_SEL : CARD_BG;

            // Outline glow: bright cyan when selected, restore original when not
            var outline = btn.GetComponent<Outline>();
            if (outline != null)
            {
                if (on)
                {
                    if (!_originalOutlines.ContainsKey(btn))
                        _originalOutlines[btn] = (outline.effectColor, outline.effectDistance);
                    outline.effectColor = SEL_OUTLINE;
                    outline.effectDistance = new Vector2(3, 3);
                }
                else if (_originalOutlines.TryGetValue(btn, out var original))
                {
                    outline.effectColor = original.color;
                    outline.effectDistance = original.distance;
                }
            }
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
            int bal = c.Coins;
            return Mathf.Min((bal / STEP) * STEP, MAX_CUSTOM_BET);
        }

        private int Snap5(int v)
        {
            return Mathf.Max(MIN_CUSTOM, Mathf.RoundToInt(v / (float)STEP) * STEP);
        }

        private void UpdateCustomToggles()
        {
            SetToggleVisual(_customCoinsToggle, true);
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
            int reward = Mathf.RoundToInt(_customAmount * 1.9f); // Economy Rebalance: 5% rake
            string curr = AutoLocalizer.Get("currency_coins");
            _customRewardText.text = AutoLocalizer.Get("bet_custom_reward", reward.ToString("N0"), curr);
        }

        #endregion

        #region Actions

        private void SelectRounds(int rounds)
        {
            _selectedRounds = rounds;

            // Update button visuals
            Color active = new Color(1f, 0.84f, 0f, 1f); // Gold
            Color inactive = new Color(0.15f, 0.15f, 0.2f, 1f);

            _rounds1Button?.GetComponent<Image>()?.color = rounds == 1 ? active : inactive;
            _rounds3Button?.GetComponent<Image>()?.color = rounds == 3 ? active : inactive;
            _rounds5Button?.GetComponent<Image>()?.color = rounds == 5 ? active : inactive;
        }

        private void OnPlayClicked()
        {
            if (_isCustomBetSelected)
            {
                _selectedBetAmount = _customAmount;
                _selectedCurrencyType = BetCurrencyType.DigitCoins;
            }

            if (_selectedBetAmount > 0)
            {
                var currency = CurrencyManager.Instance;
                if (currency == null) return;

                bool ok = false;
                switch (_selectedCurrencyType)
                {
                    case BetCurrencyType.DigitCoins:
                        ok = currency.EscrowCoins(_selectedBetAmount);
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
            // Pass rounds to matchmaking
            PlayerPrefs.SetInt("DigitPark_MatchRounds", _selectedRounds);
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
