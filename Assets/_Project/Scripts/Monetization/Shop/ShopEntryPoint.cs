using UnityEngine;
using UnityEngine.UI;

namespace DigitPark.Monetization
{
    /// <summary>
    /// Componente que convierte cualquier Button en un entry point a la tienda.
    /// Adjuntar a botones como: CurrencyDisplay, Premium Card, Shop NavBar button, etc.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ShopEntryPoint : MonoBehaviour
    {
        [Header("Shop Navigation Settings")]
        [Tooltip("Tab que se mostrara al abrir la tienda")]
        [SerializeField] private ShopTab _targetTab = ShopTab.Gems;

        [Tooltip("ID de oferta especifica (opcional)")]
        [SerializeField] private string _offerId;

        [Tooltip("Mostrar popup al abrir (ej: Not Enough Gems)")]
        [SerializeField] private bool _showPopup = false;

        [Header("Audio")]
        [SerializeField] private AudioClip _clickSound;

        private Button _button;
        private AudioSource _audioSource;

        public ShopTab TargetTab
        {
            get => _targetTab;
            set => _targetTab = value;
        }

        public string OfferId
        {
            get => _offerId;
            set => _offerId = value;
        }

        private void Awake()
        {
            _button = GetComponent<Button>();
            _audioSource = GetComponent<AudioSource>();

            if (_audioSource == null && _clickSound != null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
            }
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            PlayClickSound();
            NavigateToShop();
        }

        private void PlayClickSound()
        {
            if (_audioSource != null && _clickSound != null)
            {
                _audioSource.PlayOneShot(_clickSound);
            }
        }

        private void NavigateToShop()
        {
            var navigator = SceneNavigator.Instance;

            if (!string.IsNullOrEmpty(_offerId))
            {
                // Navigate to specific offer
                navigator.NavigateToShopOffer(_offerId);
            }
            else if (_showPopup)
            {
                // Navigate showing gems popup (Not Enough Gems flow)
                navigator.NavigateToShopForGems();
            }
            else
            {
                // Navigate to specific tab
                navigator.NavigateToShop(_targetTab);
            }

            Debug.Log($"[ShopEntryPoint] Navigating to Shop, Tab: {_targetTab}, Offer: {_offerId}");
        }

        /// <summary>
        /// Metodo publico para navegar programaticamente
        /// </summary>
        public void Navigate()
        {
            NavigateToShop();
        }

        /// <summary>
        /// Navegar a una tab especifica programaticamente
        /// </summary>
        public void NavigateToTab(ShopTab tab)
        {
            _targetTab = tab;
            NavigateToShop();
        }
    }
}
