using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DigitPark.Monetization
{
    /// <summary>
    /// Componente para el display de monedas en el header.
    /// Muestra gemas y monedas, ambos son entry points a la tienda.
    /// </summary>
    public class HeaderCurrencyDisplay : MonoBehaviour
    {
        [Header("Gems Display")]
        [SerializeField] private Button _gemsButton;
        [SerializeField] private Image _gemsIcon;
        [SerializeField] private TextMeshProUGUI _gemsText;
        [SerializeField] private Button _gemsPlusButton;

        [Header("Coins Display")]
        [SerializeField] private Button _coinsButton;
        [SerializeField] private Image _coinsIcon;
        [SerializeField] private TextMeshProUGUI _coinsText;
        [SerializeField] private Button _coinsPlusButton;

        [Header("Colors")]
        [SerializeField] private Color _gemsColor = new Color(0.4f, 0.8f, 1f, 1f);
        [SerializeField] private Color _coinsColor = new Color(1f, 0.85f, 0.3f, 1f);

        [Header("Audio")]
        [SerializeField] private AudioClip _clickSound;

        private AudioSource _audioSource;
        private int _currentGems;
        private int _currentCoins;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
            }

            SetupColors();
        }

        private void OnEnable()
        {
            // Gems buttons
            if (_gemsButton != null)
                _gemsButton.onClick.AddListener(OnGemsClick);
            if (_gemsPlusButton != null)
                _gemsPlusButton.onClick.AddListener(OnGemsClick);

            // Coins buttons
            if (_coinsButton != null)
                _coinsButton.onClick.AddListener(OnCoinsClick);
            if (_coinsPlusButton != null)
                _coinsPlusButton.onClick.AddListener(OnCoinsClick);

            // Subscribe to currency updates
            var currency = CurrencyManager.Instance;
            if (currency != null)
            {
                currency.OnGemsChanged += OnGemsChanged;
                currency.OnCoinsChanged += OnCoinsChanged;
            }

            // Initial update
            RefreshDisplay();
        }

        private void OnDisable()
        {
            if (_gemsButton != null)
                _gemsButton.onClick.RemoveListener(OnGemsClick);
            if (_gemsPlusButton != null)
                _gemsPlusButton.onClick.RemoveListener(OnGemsClick);
            if (_coinsButton != null)
                _coinsButton.onClick.RemoveListener(OnCoinsClick);
            if (_coinsPlusButton != null)
                _coinsPlusButton.onClick.RemoveListener(OnCoinsClick);

            // Unsubscribe from currency updates
            var currency = CurrencyManager.Instance;
            if (currency != null)
            {
                currency.OnGemsChanged -= OnGemsChanged;
                currency.OnCoinsChanged -= OnCoinsChanged;
            }
        }

        private void OnGemsChanged(int newAmount, int delta)
        {
            SetGems(newAmount);
        }

        private void OnCoinsChanged(int newAmount, int delta)
        {
            SetCoins(newAmount);
        }

        private void SetupColors()
        {
            if (_gemsIcon != null)
                _gemsIcon.color = _gemsColor;
            if (_coinsIcon != null)
                _coinsIcon.color = _coinsColor;
        }

        private void OnGemsClick()
        {
            PlayClickSound();
            SceneNavigator.Instance.NavigateToShop(ShopTab.Gems);
            Debug.Log("[HeaderCurrencyDisplay] Gems clicked - navigating to Shop (Gems tab)");
        }

        private void OnCoinsClick()
        {
            PlayClickSound();
            SceneNavigator.Instance.NavigateToShop(ShopTab.Coins);
            Debug.Log("[HeaderCurrencyDisplay] Coins clicked - navigating to Shop (Coins tab)");
        }

        private void PlayClickSound()
        {
            if (_audioSource != null && _clickSound != null)
            {
                _audioSource.PlayOneShot(_clickSound);
            }
        }

        /// <summary>
        /// Actualiza el display con los valores actuales
        /// </summary>
        public void RefreshDisplay()
        {
            var currency = CurrencyManager.Instance;
            if (currency != null)
            {
                UpdateDisplay(currency.Gems, currency.Coins);
            }
            else
            {
                // Fallback placeholder values
                UpdateDisplay(0, 0);
            }
        }

        /// <summary>
        /// Actualiza el display con valores especificos
        /// </summary>
        public void UpdateDisplay(int gems, int coins)
        {
            _currentGems = gems;
            _currentCoins = coins;

            if (_gemsText != null)
                _gemsText.text = FormatAmount(gems);
            if (_coinsText != null)
                _coinsText.text = FormatAmount(coins);
        }

        /// <summary>
        /// Actualiza solo las gemas
        /// </summary>
        public void SetGems(int amount)
        {
            _currentGems = amount;
            if (_gemsText != null)
                _gemsText.text = FormatAmount(amount);
        }

        /// <summary>
        /// Actualiza solo las monedas
        /// </summary>
        public void SetCoins(int amount)
        {
            _currentCoins = amount;
            if (_coinsText != null)
                _coinsText.text = FormatAmount(amount);
        }

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

        /// <summary>
        /// Reproduce animacion de "Not Enough" en gemas
        /// </summary>
        public void ShakeGems()
        {
            if (_gemsButton != null)
            {
                StartCoroutine(ShakeCoroutine(_gemsButton.transform));
            }
        }

        /// <summary>
        /// Reproduce animacion de "Not Enough" en monedas
        /// </summary>
        public void ShakeCoins()
        {
            if (_coinsButton != null)
            {
                StartCoroutine(ShakeCoroutine(_coinsButton.transform));
            }
        }

        private System.Collections.IEnumerator ShakeCoroutine(Transform target)
        {
            Vector3 originalPosition = target.localPosition;
            float elapsed = 0f;
            float duration = 0.3f;
            float intensity = 8f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float x = Mathf.Sin(elapsed * 50f) * intensity * (1f - elapsed / duration);
                target.localPosition = originalPosition + new Vector3(x, 0, 0);
                yield return null;
            }

            target.localPosition = originalPosition;
        }
    }
}
