using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
                acceptButtonText.text = string.IsNullOrEmpty(buttonText) ? "Aceptar" : buttonText;

            onAccept = onAcceptCallback;

            if (panel != null)
            {
                panel.SetActive(true);
                panel.transform.SetAsLastSibling();
            }

            Debug.Log($"[ErrorPanelUI] Mostrando error: {message}");
        }

        /// <summary>
        /// Oculta el panel
        /// </summary>
        public void Hide()
        {
            if (panel != null)
                panel.SetActive(false);

            onAccept = null;
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
