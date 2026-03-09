using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using DigitPark.Localization;

namespace DigitPark.UI.Panels
{
    /// <summary>
    /// Panel de error reutilizable con mensaje y botón de aceptar.
    /// Agregar este script al prefab ErrorPanel.
    /// </summary>
    public class ErrorPanelUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI errorText;
        [SerializeField] private Button acceptButton;
        [SerializeField] private TextMeshProUGUI acceptButtonText;

        private Action onAccept;
        private bool listenersConfigured = false;
        private Sequence _currentSequence;

        private void Awake()
        {
            ConfigureListeners();
            HideVisuals();
        }

        private void OnEnable()
        {
            ConfigureListeners();
        }

        private void ConfigureListeners()
        {
            if (listenersConfigured) return;

            if (acceptButton != null)
                acceptButton.onClick.AddListener(OnAcceptClicked);

            listenersConfigured = true;
        }

        private void HideVisuals()
        {
            if (panel != null)
                panel.SetActive(false);
        }

        private void OnDestroy()
        {
            _currentSequence?.Kill();
            if (acceptButton != null)
                acceptButton.onClick.RemoveListener(OnAcceptClicked);
        }

        /// <summary>
        /// Muestra el panel con un mensaje de error
        /// </summary>
        /// <param name="message">Mensaje de error a mostrar</param>
        /// <param name="onAcceptCallback">Callback al presionar aceptar (opcional)</param>
        /// <param name="buttonText">Texto del botón (opcional, default: "Aceptar")</param>
        public void Show(string message, Action onAcceptCallback = null, string buttonText = null)
        {
            // Asegurar que los listeners estén configurados
            ConfigureListeners();

            // Activar el GameObject del script primero si está desactivado
            if (!gameObject.activeInHierarchy)
                gameObject.SetActive(true);

            if (errorText != null)
                errorText.text = message;

            if (acceptButtonText != null)
                acceptButtonText.text = string.IsNullOrEmpty(buttonText) ? AutoLocalizer.Get("ErrorButtonText") : buttonText;

            onAccept = onAcceptCallback;

            if (panel != null)
            {
                panel.SetActive(true);
                panel.transform.SetAsLastSibling();
                AnimateIn(panel.transform);
            }

            Debug.Log($"[ErrorPanelUI] Mostrando error: {message}");
        }

        /// <summary>
        /// Oculta el panel
        /// </summary>
        public void Hide()
        {
            if (panel != null)
                AnimateOut(panel.transform, () => panel.SetActive(false));

            onAccept = null;
        }

        private void AnimateIn(Transform t)
        {
            _currentSequence?.Kill();
            t.localScale = Vector3.one * 0.85f;
            var cg = t.GetComponent<CanvasGroup>();
            if (cg == null) cg = t.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            _currentSequence = DOTween.Sequence()
                .Join(t.DOScale(1f, 0.3f).SetEase(Ease.OutBack))
                .Join(cg.DOFade(1f, 0.25f))
                .SetUpdate(true);
        }

        private void AnimateOut(Transform t, Action onComplete)
        {
            _currentSequence?.Kill();
            var cg = t.GetComponent<CanvasGroup>();
            if (cg == null) { t.gameObject.SetActive(false); onComplete?.Invoke(); return; }
            _currentSequence = DOTween.Sequence()
                .Join(t.DOScale(0.9f, 0.2f).SetEase(Ease.InQuad))
                .Join(cg.DOFade(0f, 0.2f))
                .OnComplete(() => { t.localScale = Vector3.one; cg.alpha = 1f; onComplete?.Invoke(); })
                .SetUpdate(true);
        }

        /// <summary>
        /// Indica si el panel está visible
        /// </summary>
        public bool IsVisible => panel != null && panel.activeSelf;

        private void OnAcceptClicked()
        {
            Debug.Log("[ErrorPanelUI] Aceptar clickeado");
            onAccept?.Invoke();
            Hide();
        }
    }
}
