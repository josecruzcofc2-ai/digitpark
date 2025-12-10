using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DigitPark.Localization;

namespace DigitPark.UI
{
    /// <summary>
    /// Manager global para mostrar popups de error y confirmación
    /// Utiliza los prefabs ErrorPanel y ConfirmPanel
    /// </summary>
    public class PopupManager : MonoBehaviour
    {
        private static PopupManager _instance;
        public static PopupManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<PopupManager>();
                    if (_instance == null)
                    {
                        Debug.LogWarning("[PopupManager] No instance found in scene");
                    }
                }
                return _instance;
            }
        }

        [Header("Error Panel")]
        [SerializeField] private GameObject errorPanel;
        [SerializeField] private TextMeshProUGUI errorText;
        [SerializeField] private Button acceptButton;
        [SerializeField] private TextMeshProUGUI acceptButtonText;

        [Header("Confirm Panel")]
        [SerializeField] private GameObject confirmPanel;
        [SerializeField] private TextMeshProUGUI confirmText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private TextMeshProUGUI confirmButtonText;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TextMeshProUGUI cancelButtonText;

        // Callbacks para el panel de confirmación
        private Action onConfirmCallback;
        private Action onCancelCallback;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Debug.Log("[PopupManager] Duplicate instance destroyed");
                Destroy(gameObject);
                return;
            }

            SetupListeners();
            HideAllPanels();
        }

        private void SetupListeners()
        {
            // Error Panel
            acceptButton?.onClick.AddListener(HideError);

            // Confirm Panel
            confirmButton?.onClick.AddListener(OnConfirmClicked);
            cancelButton?.onClick.AddListener(OnCancelClicked);
        }

        private void HideAllPanels()
        {
            if (errorPanel != null) errorPanel.SetActive(false);
            if (confirmPanel != null) confirmPanel.SetActive(false);
        }

        #region Error Panel Methods

        /// <summary>
        /// Muestra un error usando una key de localización
        /// </summary>
        public void ShowError(string localizationKey)
        {
            string message = GetLocalizedText(localizationKey);
            ShowErrorMessage(message);
        }

        /// <summary>
        /// Muestra un mensaje de error directamente
        /// </summary>
        public void ShowErrorMessage(string message)
        {
            Debug.Log($"[PopupManager] Showing error: {message}");

            if (errorText != null)
            {
                errorText.text = message;
            }

            // Traducir el botón de aceptar
            if (acceptButtonText != null)
            {
                acceptButtonText.text = GetLocalizedText("popup_accept_button");
            }

            if (errorPanel != null)
            {
                errorPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Oculta el panel de error
        /// </summary>
        public void HideError()
        {
            Debug.Log("[PopupManager] Hiding error panel");

            if (errorPanel != null)
            {
                errorPanel.SetActive(false);
            }
        }

        #endregion

        #region Confirm Panel Methods

        /// <summary>
        /// Muestra un diálogo de confirmación usando una key de localización
        /// </summary>
        public void ShowConfirm(string messageKey, Action onConfirm, Action onCancel = null)
        {
            string message = GetLocalizedText(messageKey);
            ShowConfirmMessage(message, onConfirm, onCancel);
        }

        /// <summary>
        /// Muestra un diálogo de confirmación con mensaje directo
        /// </summary>
        public void ShowConfirmMessage(string message, Action onConfirm, Action onCancel = null)
        {
            Debug.Log($"[PopupManager] Showing confirm: {message}");

            onConfirmCallback = onConfirm;
            onCancelCallback = onCancel;

            if (confirmText != null)
            {
                confirmText.text = message;
            }

            // Traducir los botones
            if (confirmButtonText != null)
            {
                confirmButtonText.text = GetLocalizedText("popup_confirm_button");
            }

            if (cancelButtonText != null)
            {
                cancelButtonText.text = GetLocalizedText("popup_cancel_button");
            }

            if (confirmPanel != null)
            {
                confirmPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Muestra confirmación de logout
        /// </summary>
        public void ShowLogoutConfirm(Action onConfirm)
        {
            ShowConfirm("confirm_logout_message", onConfirm);
        }

        /// <summary>
        /// Muestra confirmación de eliminar cuenta
        /// </summary>
        public void ShowDeleteAccountConfirm(Action onConfirm)
        {
            ShowConfirm("confirm_delete_message", onConfirm);
        }

        /// <summary>
        /// Muestra confirmación de salir del torneo
        /// </summary>
        public void ShowExitTournamentConfirm(Action onConfirm)
        {
            ShowConfirm("confirm_exit_tournament_message", onConfirm);
        }

        private void OnConfirmClicked()
        {
            Debug.Log("[PopupManager] Confirm clicked");
            HideConfirm();
            onConfirmCallback?.Invoke();
        }

        private void OnCancelClicked()
        {
            Debug.Log("[PopupManager] Cancel clicked");
            HideConfirm();
            onCancelCallback?.Invoke();
        }

        /// <summary>
        /// Oculta el panel de confirmación
        /// </summary>
        public void HideConfirm()
        {
            Debug.Log("[PopupManager] Hiding confirm panel");

            if (confirmPanel != null)
            {
                confirmPanel.SetActive(false);
            }

            onConfirmCallback = null;
            onCancelCallback = null;
        }

        #endregion

        #region Helper Methods

        private string GetLocalizedText(string key)
        {
            if (LocalizationManager.Instance != null)
            {
                return LocalizationManager.Instance.GetText(key);
            }
            return key;
        }

        /// <summary>
        /// Verifica si algún popup está visible
        /// </summary>
        public bool IsAnyPopupVisible()
        {
            return (errorPanel != null && errorPanel.activeSelf) ||
                   (confirmPanel != null && confirmPanel.activeSelf);
        }

        #endregion
    }
}
