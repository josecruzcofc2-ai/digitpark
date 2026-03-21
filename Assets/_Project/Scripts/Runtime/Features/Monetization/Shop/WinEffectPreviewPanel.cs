using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using DigitPark.Services;
using DigitPark.Localization;

namespace DigitPark.Monetization
{
    /// <summary>
    /// Panel overlay para previsualizar efectos de victoria en el Shop.
    /// Se muestra encima del Shop con fondo oscuro.
    /// </summary>
    public class WinEffectPreviewPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI effectNameText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Button previewButton;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Button equipButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Image backgroundGradient;
        [SerializeField] private Transform effectPreviewArea;

        private VictoryEffectData _currentEffect;
        private GameObject _previewInstance;

        private void Awake()
        {
            if (previewButton != null) previewButton.onClick.AddListener(OnPreviewClick);
            if (purchaseButton != null) purchaseButton.onClick.AddListener(OnPurchaseClick);
            if (equipButton != null) equipButton.onClick.AddListener(OnEquipClick);
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            CleanupPreview();
            if (previewButton != null) previewButton.onClick.RemoveAllListeners();
            if (purchaseButton != null) purchaseButton.onClick.RemoveAllListeners();
            if (equipButton != null) equipButton.onClick.RemoveAllListeners();
            if (closeButton != null) closeButton.onClick.RemoveAllListeners();
        }

        public void ShowEffect(VictoryEffectData effect)
        {
            if (effect == null) return;
            _currentEffect = effect;

            gameObject.SetActive(true);

            // Name
            if (effectNameText != null)
                effectNameText.text = AutoLocalizer.Get(effect.nameKey);

            // Background gradient
            if (backgroundGradient != null)
                backgroundGradient.color = Color.Lerp(effect.primaryColor, effect.secondaryColor, 0.5f) * 0.3f;

            UpdateButtons();
            AnimateIn();
        }

        public void Hide()
        {
            CleanupPreview();
            AnimateOut(() => gameObject.SetActive(false));
        }

        private void AnimateIn()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                transform.localScale = Vector3.one * 0.9f;
                DOTween.Sequence()
                    .Join(transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack))
                    .Join(canvasGroup.DOFade(1f, 0.25f))
                    .SetUpdate(true)
                    .SetLink(gameObject);
            }
        }

        private void AnimateOut(System.Action onComplete)
        {
            if (canvasGroup == null) { onComplete?.Invoke(); return; }

            DOTween.Sequence()
                .Join(transform.DOScale(0.95f, 0.15f).SetEase(Ease.InQuad))
                .Join(canvasGroup.DOFade(0f, 0.15f))
                .OnComplete(() =>
                {
                    transform.localScale = Vector3.one;
                    canvasGroup.alpha = 1f;
                    onComplete?.Invoke();
                })
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        private void UpdateButtons()
        {
            var service = VictoryEffectService.Instance;
            if (service == null || _currentEffect == null) return;

            bool owned = service.IsOwned(_currentEffect.effectId);
            bool equipped = service.EquippedEffectId == _currentEffect.effectId;

            // Price
            if (priceText != null)
            {
                if (owned)
                    priceText.text = AutoLocalizer.Get("win_effect_already_owned");
                else
                    priceText.text = GetFormattedPrice(_currentEffect);
            }

            // Status
            if (statusText != null)
            {
                statusText.text = equipped ? AutoLocalizer.Get("win_effect_equipped_label") : "";
            }

            // Button states
            if (purchaseButton != null) purchaseButton.gameObject.SetActive(!owned);
            if (equipButton != null) equipButton.gameObject.SetActive(owned && !equipped);
            if (previewButton != null) previewButton.gameObject.SetActive(true);
        }

        private string GetFormattedPrice(VictoryEffectData effect)
        {
            switch (effect.priceType)
            {
                case EffectPriceType.Free:
                    return AutoLocalizer.Get("win_effect_free_label");
                case EffectPriceType.DigitCoins:
                    return $"{effect.coinPrice:N0} DC";
                case EffectPriceType.DigitGems:
                    return $"{effect.gemPrice:N0} DG";
                case EffectPriceType.RealMoney:
                    return $"${effect.realMoneyPrice:F2}";
                default:
                    return "";
            }
        }

        private void OnPreviewClick()
        {
            if (_currentEffect == null) return;
            CleanupPreview();

            Vector3 previewPos = effectPreviewArea != null
                ? effectPreviewArea.position
                : transform.position;

            var prefab = Resources.Load<GameObject>(_currentEffect.particlePrefabName);
            if (prefab != null)
            {
                _previewInstance = Instantiate(prefab, previewPos, Quaternion.identity, transform);
                var player = _previewInstance.GetComponent<Effects.VictoryEffectPlayer>();
                if (player != null)
                    player.Initialize(_currentEffect);
                else
                    Destroy(_previewInstance, 5f);
            }
            else
            {
                Debug.LogWarning($"[WinEffectPreviewPanel] Prefab not found: {_currentEffect.particlePrefabName}");
            }
        }

        private void OnPurchaseClick()
        {
            if (_currentEffect == null) return;
            var service = VictoryEffectService.Instance;
            if (service == null) return;

            if (_currentEffect.priceType == EffectPriceType.RealMoney)
            {
                Debug.LogWarning("[WinEffectPreviewPanel] IAP purchase needed — route through PaymentManager");
                return;
            }

            if (service.TryPurchaseEffect(_currentEffect.effectId))
            {
                Debug.Log($"[WinEffectPreviewPanel] Effect purchased: {_currentEffect.effectId}");
                UpdateButtons();
            }
        }

        private void OnEquipClick()
        {
            if (_currentEffect == null) return;
            var service = VictoryEffectService.Instance;
            if (service == null) return;

            service.EquipEffect(_currentEffect.effectId);
            UpdateButtons();
        }

        private void CleanupPreview()
        {
            if (_previewInstance != null)
            {
                Destroy(_previewInstance);
                _previewInstance = null;
            }
        }
    }
}
