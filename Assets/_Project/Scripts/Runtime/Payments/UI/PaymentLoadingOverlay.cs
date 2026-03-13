using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DigitPark.Payments.UI
{
    /// <summary>
    /// Overlay semitransparente que bloquea input mientras se procesa un pago.
    /// Se suscribe a PaymentEvents para mostrarse/ocultarse automáticamente.
    /// </summary>
    public class PaymentLoadingOverlay : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Image _spinnerImage;

        [Header("Animación")]
        [SerializeField] private float _spinSpeed = 120f;

        private bool _isVisible = false;

        private void Awake()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
            Hide();
        }

        private void OnEnable()
        {
            PaymentEvents.OnPurchaseStarted += OnPurchaseStarted;
            PaymentEvents.OnPurchaseCompleted += OnPurchaseEnded;
            PaymentEvents.OnPurchaseFailed += OnPurchaseEnded;
        }

        private void OnDisable()
        {
            PaymentEvents.OnPurchaseStarted -= OnPurchaseStarted;
            PaymentEvents.OnPurchaseCompleted -= OnPurchaseEnded;
            PaymentEvents.OnPurchaseFailed -= OnPurchaseEnded;
        }

        private void Update()
        {
            if (_isVisible && _spinnerImage != null)
            {
                _spinnerImage.transform.Rotate(0, 0, -_spinSpeed * Time.deltaTime);
            }
        }

        private void OnPurchaseStarted(string productId, PaymentProvider provider)
        {
            if (_messageText != null)
                _messageText.text = "Processing payment...";
            Show();
        }

        private void OnPurchaseEnded(PaymentResult result)
        {
            Hide();
        }

        public void Show()
        {
            _isVisible = true;
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = true;
            }
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            _isVisible = false;
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
        }
    }
}
