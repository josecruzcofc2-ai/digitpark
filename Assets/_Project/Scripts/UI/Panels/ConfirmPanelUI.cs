using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DigitPark.UI.Panels
{
    /// <summary>
    /// Panel de confirmación reutilizable con mensaje y botones confirmar/cancelar.
    /// Agregar este script al prefab ConfirmPanel.
    /// </summary>
    public class ConfirmPanelUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private GameObject blockerPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TextMeshProUGUI confirmButtonText;
        [SerializeField] private TextMeshProUGUI cancelButtonText;

        private Action onConfirm;
        private Action onCancel;
        private bool listenersConfigured = false;

        private void Awake()
        {
            ConfigureListeners();
            // Solo ocultar el panel visual, NO el GameObject del script
            HideVisuals();
        }

        private void OnEnable()
        {
            // Asegurar que los listeners estén configurados cuando se active
            ConfigureListeners();
        }

        private void ConfigureListeners()
        {
            if (listenersConfigured) return;

            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClicked);

            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancelClicked);

            listenersConfigured = true;
        }

        private void HideVisuals()
        {
            // Solo ocultar los paneles visuales, no el GameObject del script
            if (panel != null)
                panel.SetActive(false);

            // Solo ocultar blocker si es diferente al gameObject actual
            if (blockerPanel != null && blockerPanel != this.gameObject)
                blockerPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (confirmButton != null)
                confirmButton.onClick.RemoveListener(OnConfirmClicked);

            if (cancelButton != null)
                cancelButton.onClick.RemoveListener(OnCancelClicked);
        }

        /// <summary>
        /// Muestra el panel de confirmación
        /// </summary>
        /// <param name="title">Título del panel (opcional)</param>
        /// <param name="message">Mensaje a mostrar</param>
        /// <param name="onConfirmCallback">Callback al confirmar</param>
        /// <param name="onCancelCallback">Callback al cancelar (opcional)</param>
        public void Show(string title, string message, Action onConfirmCallback, Action onCancelCallback = null)
        {
            // Asegurar que los listeners estén configurados
            ConfigureListeners();

            if (titleText != null && !string.IsNullOrEmpty(title))
                titleText.text = title;

            if (messageText != null)
                messageText.text = message;

            onConfirm = onConfirmCallback;
            onCancel = onCancelCallback;

            // Activar el GameObject del script primero si está desactivado
            if (!gameObject.activeInHierarchy)
                gameObject.SetActive(true);

            if (blockerPanel != null)
                blockerPanel.SetActive(true);

            if (panel != null)
            {
                panel.SetActive(true);
                panel.transform.SetAsLastSibling();
            }

            SetButtonsInteractable(true);

            Debug.Log($"[ConfirmPanelUI] Mostrando: {title} - {message}");
        }

        /// <summary>
        /// Muestra el panel con solo mensaje (sin título)
        /// </summary>
        public void Show(string message, Action onConfirmCallback, Action onCancelCallback = null)
        {
            Show(null, message, onConfirmCallback, onCancelCallback);
        }

        /// <summary>
        /// Configura los textos de los botones
        /// </summary>
        public void SetButtonTexts(string confirmText, string cancelText)
        {
            if (confirmButtonText != null && !string.IsNullOrEmpty(confirmText))
                confirmButtonText.text = confirmText;

            if (cancelButtonText != null && !string.IsNullOrEmpty(cancelText))
                cancelButtonText.text = cancelText;
        }

        /// <summary>
        /// Oculta el panel
        /// </summary>
        public void Hide()
        {
            if (panel != null)
                panel.SetActive(false);

            if (blockerPanel != null)
                blockerPanel.SetActive(false);

            onConfirm = null;
            onCancel = null;
        }

        /// <summary>
        /// Habilita o deshabilita los botones
        /// </summary>
        public void SetButtonsInteractable(bool interactable)
        {
            if (confirmButton != null)
                confirmButton.interactable = interactable;

            if (cancelButton != null)
                cancelButton.interactable = interactable;
        }

        /// <summary>
        /// Indica si el panel está visible
        /// </summary>
        public bool IsVisible => panel != null && panel.activeSelf;

        private void OnConfirmClicked()
        {
            Debug.Log("[ConfirmPanelUI] Confirmar clickeado");
            SetButtonsInteractable(false);
            onConfirm?.Invoke();
            // No ocultar automáticamente - dejar que el caller decida
        }

        private void OnCancelClicked()
        {
            Debug.Log("[ConfirmPanelUI] Cancelar clickeado");
            onCancel?.Invoke();
            Hide();
        }
    }
}
