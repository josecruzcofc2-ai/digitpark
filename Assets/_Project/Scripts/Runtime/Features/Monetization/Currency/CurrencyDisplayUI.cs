using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DigitPark.Navigation;

namespace DigitPark.Monetization
{
    /// <summary>
    /// Componente para mostrar la moneda (gemas o monedas) con actualizacion en tiempo real.
    /// Incluye boton para ir a la tienda al tocar.
    /// </summary>
    public class CurrencyDisplayUI : MonoBehaviour
    {
        [Header("Currency Type")]
        [SerializeField] private CurrencyType _currencyType = CurrencyType.DigitGems;

        [Header("UI References")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _amountText;
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _plusButton;

        [Header("Visual Settings")]
        [SerializeField] private Sprite _gemsIcon;
        [SerializeField] private Sprite _coinsIcon;
        [SerializeField] private Color _gemsColor = new Color(0.4f, 0.8f, 1f, 1f);
        [SerializeField] private Color _coinsColor = new Color(1f, 0.85f, 0.3f, 1f);

        [Header("Animation")]
        [SerializeField] private bool _animateOnChange = true;
        [SerializeField] private float _punchScale = 1.2f;
        [SerializeField] private float _animationDuration = 0.3f;

        [Header("Audio")]
        [SerializeField] private AudioClip _clickSound;
        [SerializeField] private AudioClip _updateSound;

        private int _currentAmount;
        private AudioSource _audioSource;
        private Vector3 _originalScale;

        public CurrencyType CurrencyType
        {
            get => _currencyType;
            set
            {
                _currencyType = value;
                UpdateVisuals();
            }
        }

        public int CurrentAmount => _currentAmount;

        // Events
        public event Action<int> OnAmountChanged;
        public event Action OnClicked;

        private void Awake()
        {
            _originalScale = transform.localScale;

            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
            }

            // Setup button if not assigned
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }

            UpdateVisuals();
        }

        private void OnEnable()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(OnButtonClick);
            }

            // Subscribe to currency changes
            SubscribeToCurrencyEvents();
        }

        private void OnDisable()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnButtonClick);
            }

            UnsubscribeFromCurrencyEvents();
        }

        private void SubscribeToCurrencyEvents()
        {
            var currency = CurrencyManager.Instance;
            if (currency != null)
            {
                if (_currencyType == CurrencyType.DigitGems)
                {
                    currency.OnGemsChanged += OnCurrencyChangedGems;
                    SetAmount(currency.Gems, false);
                }
                else
                {
                    currency.OnCoinsChanged += OnCurrencyChangedCoins;
                    SetAmount(currency.Coins, false);
                }
            }
        }

        private void UnsubscribeFromCurrencyEvents()
        {
            var currency = CurrencyManager.Instance;
            if (currency != null)
            {
                currency.OnGemsChanged -= OnCurrencyChangedGems;
                currency.OnCoinsChanged -= OnCurrencyChangedCoins;
            }
        }

        private void OnCurrencyChangedGems(int newAmount, int delta)
        {
            if (_currencyType == CurrencyType.DigitGems)
            {
                SetAmount(newAmount);
            }
        }

        private void OnCurrencyChangedCoins(int newAmount, int delta)
        {
            if (_currencyType == CurrencyType.DigitCoins)
            {
                SetAmount(newAmount);
            }
        }

        private void UpdateVisuals()
        {
            if (_iconImage != null)
            {
                switch (_currencyType)
                {
                    case CurrencyType.DigitGems:
                        if (_gemsIcon != null) _iconImage.sprite = _gemsIcon;
                        _iconImage.color = _gemsColor;
                        break;
                    case CurrencyType.DigitCoins:
                        if (_coinsIcon != null) _iconImage.sprite = _coinsIcon;
                        _iconImage.color = _coinsColor;
                        break;
                }
            }
        }

        /// <summary>
        /// Actualiza el monto mostrado
        /// </summary>
        public void SetAmount(int amount, bool animate = true)
        {
            int previousAmount = _currentAmount;
            _currentAmount = amount;

            if (_amountText != null)
            {
                _amountText.text = FormatAmount(amount);
            }

            if (animate && _animateOnChange && previousAmount != amount)
            {
                PlayUpdateAnimation();
                PlaySound(_updateSound);
            }

            if (previousAmount != amount)
            {
                OnAmountChanged?.Invoke(amount);
            }
        }

        /// <summary>
        /// Agrega una cantidad al monto actual (display/animation only).
        /// This only updates the local display value and does NOT modify
        /// the authoritative balance in CurrencyManager. Use
        /// CurrencyManager.AddGems/AddCoins to change the real balance;
        /// the display will auto-update via the OnGemsChanged/OnCoinsChanged events.
        /// </summary>
        public void AddAmount(int delta)
        {
            SetAmount(_currentAmount + delta);
        }

        /// <summary>
        /// Formatea el numero para mostrar (ej: 1,250 o 1.2K)
        /// </summary>
        private string FormatAmount(int amount)
        {
            if (amount >= 1000000)
            {
                return $"{amount / 1000000f:0.#}M";
            }
            else if (amount >= 10000)
            {
                return $"{amount / 1000f:0.#}K";
            }
            else
            {
                return amount.ToString("N0");
            }
        }

        private void OnButtonClick()
        {
            PlaySound(_clickSound);
            OnClicked?.Invoke();

            // Navigate to shop
            ShopTab targetTab = ShopTab.Currency;
            SceneNavigator.Instance.NavigateToShop(targetTab);

            Debug.Log($"[CurrencyDisplayUI] Clicked, navigating to Shop ({targetTab})");
        }

        private void PlayUpdateAnimation()
        {
            StartCoroutine(PunchScaleCoroutine());
        }

        private System.Collections.IEnumerator PunchScaleCoroutine()
        {
            float elapsed = 0f;
            float halfDuration = _animationDuration / 2f;

            // Scale up
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                float scale = Mathf.Lerp(1f, _punchScale, t);
                transform.localScale = _originalScale * scale;
                yield return null;
            }

            // Scale down
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                float scale = Mathf.Lerp(_punchScale, 1f, t);
                transform.localScale = _originalScale * scale;
                yield return null;
            }

            transform.localScale = _originalScale;
        }

        private void PlaySound(AudioClip clip)
        {
            if (_audioSource != null && clip != null)
            {
                _audioSource.PlayOneShot(clip);
            }
        }

        /// <summary>
        /// Reproduce animacion de "Not Enough" (shake)
        /// </summary>
        public void PlayNotEnoughAnimation()
        {
            StartCoroutine(ShakeCoroutine());
        }

        private System.Collections.IEnumerator ShakeCoroutine()
        {
            float elapsed = 0f;
            float duration = 0.3f;
            float intensity = 10f;
            Vector3 originalPosition = transform.localPosition;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float x = Mathf.Sin(elapsed * 50f) * intensity * (1f - elapsed / duration);
                transform.localPosition = originalPosition + new Vector3(x, 0, 0);
                yield return null;
            }

            transform.localPosition = originalPosition;
        }
    }

    public enum CurrencyType
    {
        DigitGems,
        DigitCoins
    }
}
