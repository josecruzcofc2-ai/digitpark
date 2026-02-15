using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

namespace DigitPark.CashBattle
{
    /// <summary>
    /// Componente UI para una opción de depósito en el panel de wallet.
    /// Se usa como prefab instanciado por WalletPanelUI.
    /// </summary>
    public class DepositOptionUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button _button;
        [SerializeField] private Image _background;
        [SerializeField] private TextMeshProUGUI _amountText;
        [SerializeField] private TextMeshProUGUI _bonusText;
        [SerializeField] private GameObject _popularBadge;
        [SerializeField] private Image _outline;

        [Header("Colors")]
        [SerializeField] private Color _normalBgColor = new Color(0.12f, 0.15f, 0.2f, 0.95f);
        [SerializeField] private Color _selectedBgColor = new Color(0f, 0.3f, 0.4f, 0.95f);
        [SerializeField] private Color _popularBgColor = new Color(0.25f, 0.2f, 0.05f, 0.95f);
        [SerializeField] private Color _bonusColor = new Color(0f, 1f, 0.5f, 1f);
        [SerializeField] private Color _popularOutlineColor = new Color(1f, 0.84f, 0f, 1f);

        private DepositOption _option;
        private Action<DepositOption> _onClickCallback;
        private bool _isSelected;

        private void Awake()
        {
            if (_button == null)
                _button = GetComponent<Button>();

            if (_background == null)
                _background = GetComponent<Image>();
        }

        /// <summary>
        /// Configura el item con los datos de la opción
        /// </summary>
        public void Setup(DepositOption option, Action<DepositOption> onClick)
        {
            _option = option;
            _onClickCallback = onClick;

            // Monto principal
            if (_amountText != null)
            {
                _amountText.text = $"${option.amount:F2}";
            }

            // Bonus
            if (_bonusText != null)
            {
                if (option.bonus > 0)
                {
                    _bonusText.gameObject.SetActive(true);
                    _bonusText.text = $"+${option.bonus:F2} BONUS";
                    _bonusText.color = _bonusColor;
                }
                else
                {
                    _bonusText.gameObject.SetActive(false);
                }
            }

            // Popular badge
            if (_popularBadge != null)
            {
                if (option.isPopular)
                {
                    _popularBadge.SetActive(true);
                    _popularBadge.transform.localScale = Vector3.zero;
                    _popularBadge.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
                }
                else
                {
                    _popularBadge.SetActive(false);
                }
            }

            // Background color
            UpdateBackground();

            // Outline para popular
            if (_outline != null)
            {
                _outline.gameObject.SetActive(option.isPopular);
                _outline.color = _popularOutlineColor;
            }

            // Configurar botón
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(OnClicked);
            }
        }

        /// <summary>
        /// Marca este item como seleccionado
        /// </summary>
        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            UpdateBackground();
        }

        private void UpdateBackground()
        {
            if (_background == null) return;

            if (_isSelected)
            {
                _background.color = _selectedBgColor;
            }
            else if (_option?.isPopular == true)
            {
                _background.color = _popularBgColor;
            }
            else
            {
                _background.color = _normalBgColor;
            }
        }

        private void OnClicked()
        {
            _onClickCallback?.Invoke(_option);
        }

        /// <summary>
        /// Obtiene los datos de la opción
        /// </summary>
        public DepositOption GetOption()
        {
            return _option;
        }

        // ==================== EDITOR HELPERS ====================

#if UNITY_EDITOR
        public void AutoSetupReferences()
        {
            _button = GetComponent<Button>();
            _background = GetComponent<Image>();
            _amountText = transform.Find("AmountText")?.GetComponent<TextMeshProUGUI>();
            _bonusText = transform.Find("BonusText")?.GetComponent<TextMeshProUGUI>();
            _popularBadge = transform.Find("PopularBadge")?.gameObject;
            _outline = transform.Find("Outline")?.GetComponent<Image>();
        }
#endif
    }
}
