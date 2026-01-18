using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace DigitPark.Monetization
{
    /// <summary>
    /// Componente UI para un item de la tienda.
    /// Se adjunta a cada card/item en el grid de la tienda.
    /// </summary>
    public class ShopItemUI : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private ShopItemData _itemData;

        [Header("UI References")]
        [SerializeField] private Button _button;
        [SerializeField] private Image _background;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _amountText;
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private Image _priceIcon;

        [Header("Badge References")]
        [SerializeField] private GameObject _popularBadge;
        [SerializeField] private GameObject _bestValueBadge;
        [SerializeField] private GameObject _bonusBadge;
        [SerializeField] private TextMeshProUGUI _bonusText;

        [Header("Outline")]
        [SerializeField] private Outline _outline;

        [Header("Audio")]
        [SerializeField] private AudioClip _clickSound;

        [Header("Colors")]
        [SerializeField] private Color _gemsColor = new Color(0.4f, 0.8f, 1f, 1f);
        [SerializeField] private Color _coinsColor = new Color(1f, 0.85f, 0.3f, 1f);
        [SerializeField] private Color _popularColor = new Color(1f, 0.84f, 0f, 1f);
        [SerializeField] private Color _disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        private AudioSource _audioSource;

        public ShopItemData ItemData => _itemData;

        // Events
        public event Action<ShopItemUI> OnItemClicked;
        public event Action<ShopItemUI> OnPurchaseRequested;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null && _clickSound != null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
            }

            // Get button if not assigned
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }
        }

        private void OnEnable()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(OnButtonClick);
            }

            // Subscribe to currency changes to update affordability
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnGemsChanged += OnCurrencyChanged;
                CurrencyManager.Instance.OnCoinsChanged += OnCurrencyChanged;
            }

            RefreshUI();
        }

        private void OnDisable()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnButtonClick);
            }

            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnGemsChanged -= OnCurrencyChanged;
                CurrencyManager.Instance.OnCoinsChanged -= OnCurrencyChanged;
            }
        }

        /// <summary>
        /// Configura el item con datos especificos
        /// </summary>
        public void Setup(ShopItemData data)
        {
            _itemData = data;
            RefreshUI();
        }

        /// <summary>
        /// Actualiza la UI con los datos actuales
        /// </summary>
        public void RefreshUI()
        {
            if (_itemData == null) return;

            // Icon
            if (_iconImage != null && _itemData.icon != null)
            {
                _iconImage.sprite = _itemData.icon;
                _iconImage.color = _itemData.accentColor;
            }
            else if (_iconImage != null)
            {
                // Use accent color as placeholder
                _iconImage.color = _itemData.accentColor;
            }

            // Amount text
            if (_amountText != null)
            {
                switch (_itemData.itemType)
                {
                    case ShopItemType.GemsPack:
                        _amountText.text = _itemData.gemsAmount.ToString("N0");
                        _amountText.color = _gemsColor;
                        break;

                    case ShopItemType.CoinsPack:
                        _amountText.text = _itemData.coinsAmount.ToString("N0");
                        _amountText.color = _coinsColor;
                        break;

                    default:
                        _amountText.text = _itemData.displayName;
                        _amountText.color = _itemData.accentColor;
                        break;
                }
            }

            // Price
            if (_priceText != null)
            {
                _priceText.text = _itemData.GetFormattedPrice();
            }

            // Price icon (show gem icon if price is in gems)
            if (_priceIcon != null)
            {
                _priceIcon.gameObject.SetActive(_itemData.priceType == PriceType.Gems);
            }

            // Badges
            if (_popularBadge != null)
            {
                _popularBadge.SetActive(_itemData.isPopular);
            }

            if (_bestValueBadge != null)
            {
                _bestValueBadge.SetActive(_itemData.isBestValue);
            }

            if (_bonusBadge != null && _bonusText != null)
            {
                bool showBonus = _itemData.showBonusBadge && _itemData.bonusPercent > 0;
                _bonusBadge.SetActive(showBonus);
                if (showBonus)
                {
                    _bonusText.text = $"{_itemData.GetBonusText()} BONUS";
                }
            }

            // Outline color based on popularity
            if (_outline != null)
            {
                if (_itemData.isPopular)
                {
                    _outline.effectColor = _popularColor;
                    _outline.effectDistance = new Vector2(2, 2);
                }
                else
                {
                    _outline.effectColor = _itemData.accentColor * 0.6f;
                    _outline.effectDistance = new Vector2(1, 1);
                }
            }

            // Update affordability visual
            UpdateAffordabilityVisual();
        }

        private void UpdateAffordabilityVisual()
        {
            if (_itemData == null || _button == null) return;

            // Real money items are always "affordable" (IAP handles payment)
            if (_itemData.priceType == PriceType.RealMoney || _itemData.priceType == PriceType.Free)
            {
                _button.interactable = true;
                if (_background != null) _background.color = Color.white;
                return;
            }

            bool canAfford = _itemData.CanAfford();
            _button.interactable = true; // Keep interactable to show "not enough" popup

            // Visual feedback for affordability
            if (_background != null)
            {
                _background.color = canAfford ? Color.white : new Color(0.8f, 0.8f, 0.8f, 1f);
            }
        }

        private void OnCurrencyChanged(int newAmount, int delta)
        {
            UpdateAffordabilityVisual();
        }

        private void OnButtonClick()
        {
            PlayClickSound();
            OnItemClicked?.Invoke(this);
            OnPurchaseRequested?.Invoke(this);

            Debug.Log($"[ShopItemUI] Item clicked: {_itemData?.displayName}");
        }

        /// <summary>
        /// Intenta comprar el item directamente
        /// </summary>
        public bool TryPurchase()
        {
            if (_itemData == null) return false;

            // For real money items, trigger IAP flow
            if (_itemData.priceType == PriceType.RealMoney)
            {
                // This should be handled by ShopManager with IAP
                Debug.Log($"[ShopItemUI] IAP purchase requested: {_itemData.iapProductId}");
                return false;
            }

            // For in-game currency purchases
            bool success = _itemData.TryPurchase();

            if (success)
            {
                PlayPurchaseAnimation();
                Debug.Log($"[ShopItemUI] Purchase successful: {_itemData.displayName}");
            }
            else
            {
                PlayNotEnoughAnimation();
                Debug.Log($"[ShopItemUI] Purchase failed: not enough currency");
            }

            return success;
        }

        private void PlayClickSound()
        {
            if (_audioSource != null && _clickSound != null)
            {
                _audioSource.PlayOneShot(_clickSound);
            }
        }

        private void PlayPurchaseAnimation()
        {
            // Scale punch animation
            StartCoroutine(PunchScaleCoroutine(1.15f, 0.2f));
        }

        private void PlayNotEnoughAnimation()
        {
            // Shake animation
            StartCoroutine(ShakeCoroutine());
        }

        private System.Collections.IEnumerator PunchScaleCoroutine(float targetScale, float duration)
        {
            Vector3 originalScale = transform.localScale;
            float elapsed = 0f;
            float halfDuration = duration / 2f;

            // Scale up
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                transform.localScale = Vector3.Lerp(originalScale, originalScale * targetScale, t);
                yield return null;
            }

            // Scale down
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                transform.localScale = Vector3.Lerp(originalScale * targetScale, originalScale, t);
                yield return null;
            }

            transform.localScale = originalScale;
        }

        private System.Collections.IEnumerator ShakeCoroutine()
        {
            Vector3 originalPosition = transform.localPosition;
            float elapsed = 0f;
            float duration = 0.3f;
            float intensity = 8f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float x = Mathf.Sin(elapsed * 50f) * intensity * (1f - elapsed / duration);
                transform.localPosition = originalPosition + new Vector3(x, 0, 0);
                yield return null;
            }

            transform.localPosition = originalPosition;
        }

        // ==================== EDITOR ====================

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_itemData != null)
            {
                RefreshUI();
            }
        }
#endif
    }
}
