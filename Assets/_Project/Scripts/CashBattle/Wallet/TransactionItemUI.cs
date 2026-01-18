using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DigitPark.CashBattle
{
    /// <summary>
    /// Componente UI para un item de transacción en el historial del wallet.
    /// Se usa como prefab instanciado por WalletPanelUI.
    /// </summary>
    public class TransactionItemUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image _background;
        [SerializeField] private Image _typeIcon;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _dateText;
        [SerializeField] private TextMeshProUGUI _amountText;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private Image _statusIndicator;

        [Header("Type Icons")]
        [SerializeField] private Sprite _depositIcon;
        [SerializeField] private Sprite _withdrawalIcon;
        [SerializeField] private Sprite _entryFeeIcon;
        [SerializeField] private Sprite _winningsIcon;
        [SerializeField] private Sprite _bonusIcon;
        [SerializeField] private Sprite _refundIcon;

        [Header("Colors")]
        [SerializeField] private Color _normalBgColor = new Color(0.1f, 0.12f, 0.15f, 0.95f);
        [SerializeField] private Color _pendingColor = new Color(1f, 0.84f, 0f, 1f);
        [SerializeField] private Color _completedColor = new Color(0f, 1f, 0.5f, 1f);
        [SerializeField] private Color _failedColor = new Color(1f, 0.3f, 0.3f, 1f);

        private WalletTransaction _transaction;

        /// <summary>
        /// Configura el item con los datos de la transacción
        /// </summary>
        public void Setup(WalletTransaction transaction)
        {
            _transaction = transaction;

            // Descripción
            if (_descriptionText != null)
            {
                _descriptionText.text = transaction.description;
            }

            // Fecha
            if (_dateText != null)
            {
                _dateText.text = transaction.GetFormattedDate();
            }

            // Monto
            if (_amountText != null)
            {
                _amountText.text = transaction.GetFormattedAmount();
                _amountText.color = transaction.GetTypeColor();
            }

            // Icono de tipo
            SetTypeIcon(transaction.type);

            // Status
            SetStatus(transaction.status);

            // Background
            if (_background != null)
            {
                _background.color = _normalBgColor;
            }
        }

        private void SetTypeIcon(TransactionType type)
        {
            if (_typeIcon == null) return;

            Sprite icon = null;
            Color iconColor = Color.white;

            switch (type)
            {
                case TransactionType.Deposit:
                    icon = _depositIcon;
                    iconColor = new Color(0f, 1f, 0.5f, 1f); // Verde
                    break;

                case TransactionType.Withdrawal:
                    icon = _withdrawalIcon;
                    iconColor = new Color(1f, 0.6f, 0.2f, 1f); // Naranja
                    break;

                case TransactionType.EntryFee:
                    icon = _entryFeeIcon;
                    iconColor = new Color(1f, 0.4f, 0.4f, 1f); // Rojo
                    break;

                case TransactionType.Winnings:
                    icon = _winningsIcon;
                    iconColor = new Color(1f, 0.84f, 0f, 1f); // Dorado
                    break;

                case TransactionType.Bonus:
                    icon = _bonusIcon;
                    iconColor = new Color(0f, 0.83f, 1f, 1f); // Cyan
                    break;

                case TransactionType.Refund:
                    icon = _refundIcon;
                    iconColor = new Color(0.7f, 0.7f, 1f, 1f); // Azul claro
                    break;
            }

            if (icon != null)
            {
                _typeIcon.sprite = icon;
                _typeIcon.gameObject.SetActive(true);
            }
            else
            {
                _typeIcon.gameObject.SetActive(false);
            }

            _typeIcon.color = iconColor;
        }

        private void SetStatus(TransactionStatus status)
        {
            if (_statusText != null)
            {
                switch (status)
                {
                    case TransactionStatus.Pending:
                        _statusText.text = "Pendiente";
                        _statusText.color = _pendingColor;
                        break;

                    case TransactionStatus.Completed:
                        _statusText.text = "";
                        _statusText.gameObject.SetActive(false);
                        break;

                    case TransactionStatus.Failed:
                        _statusText.text = "Fallido";
                        _statusText.color = _failedColor;
                        break;

                    case TransactionStatus.Cancelled:
                        _statusText.text = "Cancelado";
                        _statusText.color = _failedColor;
                        break;
                }
            }

            if (_statusIndicator != null)
            {
                switch (status)
                {
                    case TransactionStatus.Pending:
                        _statusIndicator.color = _pendingColor;
                        _statusIndicator.gameObject.SetActive(true);
                        break;

                    case TransactionStatus.Completed:
                        _statusIndicator.color = _completedColor;
                        _statusIndicator.gameObject.SetActive(false);
                        break;

                    case TransactionStatus.Failed:
                    case TransactionStatus.Cancelled:
                        _statusIndicator.color = _failedColor;
                        _statusIndicator.gameObject.SetActive(true);
                        break;
                }
            }
        }

        /// <summary>
        /// Obtiene los datos de la transacción
        /// </summary>
        public WalletTransaction GetTransaction()
        {
            return _transaction;
        }

        // ==================== EDITOR HELPERS ====================

#if UNITY_EDITOR
        public void AutoSetupReferences()
        {
            _background = GetComponent<Image>();
            _typeIcon = transform.Find("TypeIcon")?.GetComponent<Image>();
            _descriptionText = transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
            _dateText = transform.Find("Date")?.GetComponent<TextMeshProUGUI>();
            _amountText = transform.Find("Amount")?.GetComponent<TextMeshProUGUI>();
            _statusText = transform.Find("Status")?.GetComponent<TextMeshProUGUI>();
            _statusIndicator = transform.Find("StatusIndicator")?.GetComponent<Image>();
        }
#endif
    }
}
