using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using DigitPark.Localization;

namespace DigitPark.UI.Panels
{
    /// <summary>
    /// Panel de entrada de texto reutilizable con input y botones confirmar/cancelar.
    /// Agregar este script al prefab ChangeNamePanel o similar.
    /// </summary>
    public class InputPanelUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private GameObject blockerPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TextMeshProUGUI confirmButtonText;
        [SerializeField] private TextMeshProUGUI cancelButtonText;

        [Header("Validation")]
        [SerializeField] private int minLength = 3;
        [SerializeField] private int maxLength = 20;

        private Action<string> onConfirm;
        private Action onCancel;
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

            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClicked);

            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancelClicked);

            listenersConfigured = true;
        }

        private void HideVisuals()
        {
            if (panel != null)
                panel.SetActive(false);

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
        /// Muestra el panel de entrada
        /// </summary>
        /// <param name="title">Título del panel (opcional)</param>
        /// <param name="placeholder">Placeholder del input (opcional)</param>
        /// <param name="onConfirmCallback">Callback al confirmar (recibe el texto ingresado)</param>
        /// <param name="onCancelCallback">Callback al cancelar (opcional)</param>
        public void Show(string title, string placeholder, Action<string> onConfirmCallback, Action onCancelCallback = null)
        {
            // Asegurar que los listeners estén configurados
            ConfigureListeners();

            // Activar el GameObject del script primero si está desactivado
            if (!gameObject.activeInHierarchy)
                gameObject.SetActive(true);

            if (titleText != null && !string.IsNullOrEmpty(title))
                titleText.text = title;

            if (inputField != null)
            {
                inputField.text = "";
                if (!string.IsNullOrEmpty(placeholder))
                {
                    var placeholderText = inputField.placeholder?.GetComponent<TextMeshProUGUI>();
                    if (placeholderText != null)
                        placeholderText.text = placeholder;
                }
            }

            onConfirm = onConfirmCallback;
            onCancel = onCancelCallback;

            if (blockerPanel != null)
                blockerPanel.SetActive(true);

            if (panel != null)
            {
                panel.SetActive(true);
                panel.transform.SetAsLastSibling();
                AnimateIn(panel.transform);
            }

            SetButtonsInteractable(true);

            // Enfocar el input field
            if (inputField != null)
            {
                inputField.Select();
                inputField.ActivateInputField();
            }

            Debug.Log($"[InputPanelUI] Mostrando panel: {title}");
        }

        /// <summary>
        /// Muestra el panel con configuración simple
        /// </summary>
        public void Show(Action<string> onConfirmCallback, Action onCancelCallback = null)
        {
            Show(null, null, onConfirmCallback, onCancelCallback);
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
        /// Configura los límites de longitud del texto
        /// </summary>
        public void SetLengthLimits(int min, int max)
        {
            minLength = min;
            maxLength = max;

            if (inputField != null)
                inputField.characterLimit = max;
        }

        /// <summary>
        /// Oculta el panel
        /// </summary>
        public void Hide()
        {
            if (panel != null)
                AnimateOut(panel.transform, () =>
                {
                    panel.SetActive(false);
                    gameObject.SetActive(false);
                });

            if (blockerPanel != null)
            {
                var blockerCG = blockerPanel.GetComponent<CanvasGroup>();
                if (blockerCG == null) blockerCG = blockerPanel.AddComponent<CanvasGroup>();
                blockerCG.DOFade(0f, 0.2f).OnComplete(() => blockerPanel.SetActive(false)).SetUpdate(true);
            }

            if (inputField != null)
                inputField.text = "";

            onConfirm = null;
            onCancel = null;
        }

        private void AnimateIn(Transform t)
        {
            t.localScale = Vector3.one * 0.85f;
            var cg = t.GetComponent<CanvasGroup>();
            if (cg == null) cg = t.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            DOTween.Sequence()
                .Join(t.DOScale(1f, 0.3f).SetEase(Ease.OutBack))
                .Join(cg.DOFade(1f, 0.25f))
                .SetUpdate(true);
        }

        private void AnimateOut(Transform t, Action onComplete)
        {
            var cg = t.GetComponent<CanvasGroup>();
            if (cg == null) { t.gameObject.SetActive(false); onComplete?.Invoke(); return; }
            DOTween.Sequence()
                .Join(t.DOScale(0.9f, 0.2f).SetEase(Ease.InQuad))
                .Join(cg.DOFade(0f, 0.2f))
                .OnComplete(() => { t.localScale = Vector3.one; cg.alpha = 1f; onComplete?.Invoke(); })
                .SetUpdate(true);
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
        /// Obtiene el texto actual del input
        /// </summary>
        public string GetInputText()
        {
            return inputField != null ? inputField.text.Trim() : string.Empty;
        }

        /// <summary>
        /// Indica si el panel está visible
        /// </summary>
        public bool IsVisible => panel != null && panel.activeSelf;

        /// <summary>
        /// Valida el texto ingresado
        /// </summary>
        /// <returns>True si el texto es válido</returns>
        public bool ValidateInput(out string errorMessage)
        {
            string text = GetInputText();

            if (string.IsNullOrEmpty(text))
            {
                errorMessage = AutoLocalizer.Get("validation_field_empty");
                return false;
            }

            if (text.Length < minLength)
            {
                errorMessage = AutoLocalizer.Get("validation_min_chars", minLength);
                return false;
            }

            if (text.Length > maxLength)
            {
                errorMessage = AutoLocalizer.Get("validation_max_chars", maxLength);
                return false;
            }

            errorMessage = null;
            return true;
        }

        private void OnConfirmClicked()
        {
            if (!ValidateInput(out string errorMessage))
            {
                Debug.LogWarning($"[InputPanelUI] Validación fallida: {errorMessage}");
                return;
            }

            string text = GetInputText();
            Debug.Log($"[InputPanelUI] Confirmar clickeado con texto: {text}");

            SetButtonsInteractable(false);
            onConfirm?.Invoke(text);
            // No ocultar automáticamente - dejar que el caller decida después de procesar
        }

        private void OnCancelClicked()
        {
            Debug.Log("[InputPanelUI] Cancelar clickeado");
            onCancel?.Invoke();
            Hide();
        }
    }
}
