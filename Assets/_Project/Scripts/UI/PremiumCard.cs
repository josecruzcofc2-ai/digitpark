using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DigitPark.UI
{
    /// <summary>
    /// Tarjeta Premium en el MainMenu que lleva a ofertas especiales en la tienda.
    /// Muestra ofertas destacadas, pase de batalla, starter packs, etc.
    /// </summary>
    public class PremiumCard : MonoBehaviour
    {
        [Header("Card Type")]
        [SerializeField] private PremiumCardType _cardType = PremiumCardType.SpecialOffer;

        [Header("UI References")]
        [SerializeField] private Button _cardButton;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _subtitleText;
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private GameObject _discountBadge;
        [SerializeField] private TextMeshProUGUI _discountText;
        [SerializeField] private GameObject _timerContainer;
        [SerializeField] private TextMeshProUGUI _timerText;

        [Header("Visual Settings")]
        [SerializeField] private Color _offerColor = new Color(0.6f, 0.3f, 0.9f, 1f);
        [SerializeField] private Color _battlePassColor = new Color(1f, 0.5f, 0.1f, 1f);
        [SerializeField] private Color _starterPackColor = new Color(0.2f, 0.8f, 0.4f, 1f);

        [Header("Offer Data")]
        [SerializeField] private string _offerId;
        [SerializeField] private Monetization.ShopTab _targetTab = Monetization.ShopTab.Offers;

        [Header("Audio")]
        [SerializeField] private AudioClip _clickSound;

        private AudioSource _audioSource;

        public enum PremiumCardType
        {
            SpecialOffer,
            BattlePass,
            StarterPack,
            DailyDeal,
            WeeklyBundle
        }

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null && _clickSound != null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
            }

            SetupVisuals();
        }

        private void OnEnable()
        {
            if (_cardButton != null)
            {
                _cardButton.onClick.AddListener(OnCardClick);
            }
            else
            {
                // Try to get button from this object
                Button btn = GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(OnCardClick);
                }
            }
        }

        private void OnDisable()
        {
            if (_cardButton != null)
            {
                _cardButton.onClick.RemoveListener(OnCardClick);
            }
            else
            {
                Button btn = GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveListener(OnCardClick);
                }
            }
        }

        private void SetupVisuals()
        {
            Color accentColor = GetCardColor();

            if (_backgroundImage != null)
            {
                // Apply gradient or tint based on card type
                _backgroundImage.color = new Color(accentColor.r * 0.3f, accentColor.g * 0.3f, accentColor.b * 0.3f, 0.95f);
            }

            if (_iconImage != null)
            {
                _iconImage.color = accentColor;
            }

            // Setup default content based on type
            SetupDefaultContent();
        }

        private Color GetCardColor()
        {
            switch (_cardType)
            {
                case PremiumCardType.BattlePass:
                    return _battlePassColor;
                case PremiumCardType.StarterPack:
                    return _starterPackColor;
                case PremiumCardType.SpecialOffer:
                case PremiumCardType.DailyDeal:
                case PremiumCardType.WeeklyBundle:
                default:
                    return _offerColor;
            }
        }

        private void SetupDefaultContent()
        {
            switch (_cardType)
            {
                case PremiumCardType.BattlePass:
                    SetContent("PASE DE BATALLA", "Temporada 1 - Neon Dreams", "$9.99", "", false);
                    _targetTab = Monetization.ShopTab.Offers;
                    break;

                case PremiumCardType.StarterPack:
                    SetContent("STARTER PACK", "500 Gemas + Tema Exclusivo", "$2.99", "70%", true);
                    _targetTab = Monetization.ShopTab.Offers;
                    break;

                case PremiumCardType.DailyDeal:
                    SetContent("OFERTA DEL DIA", "1000 Monedas + 50 Gemas", "$1.99", "50%", true);
                    _targetTab = Monetization.ShopTab.Offers;
                    break;

                case PremiumCardType.WeeklyBundle:
                    SetContent("BUNDLE SEMANAL", "Todo lo que necesitas", "$4.99", "40%", true);
                    _targetTab = Monetization.ShopTab.Offers;
                    break;

                case PremiumCardType.SpecialOffer:
                default:
                    SetContent("OFERTA ESPECIAL", "Tiempo limitado!", "$4.99", "60%", true);
                    _targetTab = Monetization.ShopTab.Offers;
                    break;
            }
        }

        /// <summary>
        /// Configura el contenido de la tarjeta
        /// </summary>
        public void SetContent(string title, string subtitle, string price, string discount, bool showDiscount)
        {
            if (_titleText != null)
                _titleText.text = title;

            if (_subtitleText != null)
                _subtitleText.text = subtitle;

            if (_priceText != null)
                _priceText.text = price;

            if (_discountBadge != null)
                _discountBadge.SetActive(showDiscount && !string.IsNullOrEmpty(discount));

            if (_discountText != null)
                _discountText.text = $"{discount} OFF";
        }

        /// <summary>
        /// Configura el timer de la oferta
        /// </summary>
        public void SetTimer(float remainingSeconds)
        {
            if (_timerContainer != null)
            {
                _timerContainer.SetActive(remainingSeconds > 0);
            }

            if (_timerText != null && remainingSeconds > 0)
            {
                System.TimeSpan time = System.TimeSpan.FromSeconds(remainingSeconds);

                if (time.TotalDays >= 1)
                {
                    _timerText.text = $"{(int)time.TotalDays}d {time.Hours}h";
                }
                else
                {
                    _timerText.text = $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}";
                }
            }
        }

        private void OnCardClick()
        {
            PlayClickSound();
            NavigateToShop();
        }

        private void NavigateToShop()
        {
            var navigator = Monetization.SceneNavigator.Instance;

            if (!string.IsNullOrEmpty(_offerId))
            {
                // Navigate to specific offer
                navigator.NavigateToShopOffer(_offerId);
            }
            else if (_cardType == PremiumCardType.BattlePass)
            {
                // Navigate to Battle Pass scene
                navigator.NavigateTo(Monetization.SceneNavigator.Scenes.BATTLE_PASS);
            }
            else
            {
                // Navigate to shop offers tab
                navigator.NavigateToShop(_targetTab);
            }

            Debug.Log($"[PremiumCard] Clicked - Type: {_cardType}, Navigating to shop");
        }

        private void PlayClickSound()
        {
            if (_audioSource != null && _clickSound != null)
            {
                _audioSource.PlayOneShot(_clickSound);
            }
        }

        /// <summary>
        /// Configura el ID de oferta para deep linking
        /// </summary>
        public void SetOfferId(string id)
        {
            _offerId = id;
        }
    }
}
