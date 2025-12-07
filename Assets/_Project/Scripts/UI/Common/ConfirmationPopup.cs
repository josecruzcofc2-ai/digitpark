using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Localization;

namespace DigitPark.UI.Common
{
    /// <summary>
    /// Popup genérico de confirmación con título, mensaje y botones de confirmar/cancelar
    /// </summary>
    public class ConfirmationPopup : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private TextMeshProUGUI currentValueText;
        [SerializeField] private TextMeshProUGUI newValueText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TextMeshProUGUI confirmButtonText;

        private Action onConfirm;
        private Action onCancel;

        // Los listeners se configuran en Create() ahora

        /// <summary>
        /// Muestra el popup de confirmación
        /// </summary>
        public void Show(string title, string message, Action onConfirmCallback, Action onCancelCallback = null, string confirmText = "CONFIRMAR")
        {
            if (panel != null)
            {
                panel.SetActive(true);
                // Mover al final de la jerarquía para estar encima de todo
                panel.transform.SetAsLastSibling();
            }

            if (titleText != null)
                titleText.text = title;

            if (messageText != null)
                messageText.text = message;

            if (confirmButtonText != null)
                confirmButtonText.text = confirmText;

            // Ocultar textos de valores (solo para cambio de nombre)
            if (currentValueText != null)
                currentValueText.gameObject.SetActive(false);

            if (newValueText != null)
                newValueText.gameObject.SetActive(false);

            onConfirm = onConfirmCallback;
            onCancel = onCancelCallback;
        }

        /// <summary>
        /// Muestra el popup con valores actuales y nuevos (para cambio de nombre)
        /// </summary>
        public void ShowWithValues(string title, string message, string currentValue, string newValue, Action onConfirmCallback, Action onCancelCallback = null)
        {
            Show(title, message, onConfirmCallback, onCancelCallback);

            // Mostrar valores
            if (currentValueText != null)
            {
                currentValueText.gameObject.SetActive(true);
                currentValueText.text = $"{AutoLocalizer.Get("current_value")} {currentValue}";
            }

            if (newValueText != null)
            {
                newValueText.gameObject.SetActive(true);
                newValueText.text = $"{AutoLocalizer.Get("new_value")} {newValue}";
            }
        }

        /// <summary>
        /// Oculta el popup
        /// </summary>
        public void Hide()
        {
            if (panel != null)
                panel.SetActive(false);

            onConfirm = null;
            onCancel = null;
        }

        private void OnConfirmClicked()
        {
            onConfirm?.Invoke();
            Hide();
        }

        private void OnCancelClicked()
        {
            onCancel?.Invoke();
            Hide();
        }

        /// <summary>
        /// Crea un ConfirmationPopup programáticamente
        /// </summary>
        public static ConfirmationPopup Create(Transform parent)
        {
            // Crear panel de fondo oscuro
            GameObject popupObj = new GameObject("ConfirmationPopup");
            popupObj.transform.SetParent(parent, false);

            RectTransform popupRT = popupObj.AddComponent<RectTransform>();
            popupRT.anchorMin = Vector2.zero;
            popupRT.anchorMax = Vector2.one;
            popupRT.sizeDelta = Vector2.zero;

            Image bgImage = popupObj.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.8f);
            bgImage.raycastTarget = true; // Asegurar que bloquea clicks detrás

            // Agregar CanvasGroup para control de interacción
            CanvasGroup canvasGroup = popupObj.AddComponent<CanvasGroup>();
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            ConfirmationPopup popup = popupObj.AddComponent<ConfirmationPopup>();

            // Crear panel central
            GameObject panelObj = UIFactory.CreatePanelWithSize(
                popupObj.transform,
                "Panel",
                new Vector2(700, 500),
                new Color(0.1f, 0.1f, 0.2f)
            );

            RectTransform panelRT = panelObj.GetComponent<RectTransform>();
            panelRT.anchoredPosition = Vector2.zero;
            popup.panel = popupObj;

            // Título
            TextMeshProUGUI title = UIFactory.CreateText(
                panelObj.transform,
                "Title",
                "CONFIRMACIÓN",
                36,
                UIFactory.ElectricBlue,
                TMPro.TextAlignmentOptions.Center
            );
            RectTransform titleRT = title.GetComponent<RectTransform>();
            titleRT.anchoredPosition = new Vector2(0, 180);
            titleRT.sizeDelta = new Vector2(600, 50);
            popup.titleText = title;

            // Mensaje
            TextMeshProUGUI message = UIFactory.CreateText(
                panelObj.transform,
                "Message",
                "¿Estás seguro?",
                28,
                Color.white,
                TMPro.TextAlignmentOptions.Center
            );
            RectTransform messageRT = message.GetComponent<RectTransform>();
            messageRT.anchoredPosition = new Vector2(0, 80);
            messageRT.sizeDelta = new Vector2(600, 100);
            popup.messageText = message;

            // Valor actual
            TextMeshProUGUI currentValue = UIFactory.CreateText(
                panelObj.transform,
                "CurrentValue",
                "Actual: ",
                24,
                new Color(0.7f, 0.7f, 0.7f),
                TMPro.TextAlignmentOptions.Center
            );
            RectTransform currentRT = currentValue.GetComponent<RectTransform>();
            currentRT.anchoredPosition = new Vector2(0, 0);
            currentRT.sizeDelta = new Vector2(600, 40);
            popup.currentValueText = currentValue;

            // Valor nuevo
            TextMeshProUGUI newValue = UIFactory.CreateText(
                panelObj.transform,
                "NewValue",
                "Nuevo: ",
                24,
                UIFactory.NeonYellow,
                TMPro.TextAlignmentOptions.Center
            );
            RectTransform newRT = newValue.GetComponent<RectTransform>();
            newRT.anchoredPosition = new Vector2(0, -40);
            newRT.sizeDelta = new Vector2(600, 40);
            popup.newValueText = newValue;

            // Botón Confirmar
            Button confirmBtn = UIFactory.CreateButton(
                panelObj.transform,
                "ConfirmButton",
                "CONFIRMAR",
                new Vector2(300, 70),
                UIFactory.BrightGreen
            );
            RectTransform confirmRT = confirmBtn.GetComponent<RectTransform>();
            confirmRT.anchoredPosition = new Vector2(-160, -150);
            popup.confirmButton = confirmBtn;
            popup.confirmButtonText = confirmBtn.GetComponentInChildren<TextMeshProUGUI>();

            // Botón Cancelar
            Button cancelBtn = UIFactory.CreateButton(
                panelObj.transform,
                "CancelButton",
                "CANCELAR",
                new Vector2(300, 70),
                new Color(0.5f, 0.2f, 0.2f)
            );
            RectTransform cancelRT = cancelBtn.GetComponent<RectTransform>();
            cancelRT.anchoredPosition = new Vector2(160, -150);
            popup.cancelButton = cancelBtn;

            // Configurar listeners AQUÍ en lugar de Start()
            confirmBtn.onClick.AddListener(popup.OnConfirmClicked);
            cancelBtn.onClick.AddListener(popup.OnCancelClicked);

            return popup;
        }
    }
}
