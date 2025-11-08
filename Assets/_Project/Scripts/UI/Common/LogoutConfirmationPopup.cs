using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DigitPark.UI.Common
{
    /// <summary>
    /// Popup de confirmación específico para cerrar sesión
    /// </summary>
    public class LogoutConfirmationPopup : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        private Action onConfirm;

        /// <summary>
        /// Muestra el popup de confirmación de logout
        /// </summary>
        public void Show(Action onConfirmCallback)
        {
            if (panel != null)
            {
                panel.SetActive(true);
                // Mover al final de la jerarquía para estar encima de todo
                panel.transform.SetAsLastSibling();
            }

            onConfirm = onConfirmCallback;
        }

        /// <summary>
        /// Oculta el popup
        /// </summary>
        public void Hide()
        {
            if (panel != null)
                panel.SetActive(false);

            onConfirm = null;

            // Destruir el popup después de ocultarlo
            Destroy(gameObject);
        }

        private void OnConfirmClicked()
        {
            onConfirm?.Invoke();
            Hide();
        }

        private void OnCancelClicked()
        {
            Hide();
        }

        /// <summary>
        /// Crea un LogoutConfirmationPopup programáticamente
        /// </summary>
        public static LogoutConfirmationPopup Create(Transform parent)
        {
            // Crear panel de fondo oscuro
            GameObject popupObj = new GameObject("LogoutConfirmationPopup");
            popupObj.transform.SetParent(parent, false);

            RectTransform popupRT = popupObj.AddComponent<RectTransform>();
            popupRT.anchorMin = Vector2.zero;
            popupRT.anchorMax = Vector2.one;
            popupRT.sizeDelta = Vector2.zero;

            Image bgImage = popupObj.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.8f);
            bgImage.raycastTarget = true; // Bloquea clicks detrás

            // Agregar CanvasGroup para control de interacción
            CanvasGroup canvasGroup = popupObj.AddComponent<CanvasGroup>();
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            LogoutConfirmationPopup popup = popupObj.AddComponent<LogoutConfirmationPopup>();

            // Crear panel central
            GameObject panelObj = UIFactory.CreatePanelWithSize(
                popupObj.transform,
                "Panel",
                new Vector2(700, 400),
                new Color(0.1f, 0.1f, 0.2f)
            );

            RectTransform panelRT = panelObj.GetComponent<RectTransform>();
            panelRT.anchoredPosition = Vector2.zero;
            popup.panel = popupObj;

            // Agregar Outline al panel para mejor visual
            Outline outline = panelObj.AddComponent<Outline>();
            outline.effectColor = UIFactory.CoralRed;
            outline.effectDistance = new Vector2(3, -3);

            // Título
            TextMeshProUGUI title = UIFactory.CreateText(
                panelObj.transform,
                "Title",
                "CERRAR SESIÓN",
                36,
                UIFactory.CoralRed,
                TMPro.TextAlignmentOptions.Center
            );
            RectTransform titleRT = title.GetComponent<RectTransform>();
            titleRT.anchoredPosition = new Vector2(0, 120);
            titleRT.sizeDelta = new Vector2(600, 50);
            title.outlineWidth = 0.3f;
            title.outlineColor = new Color(0, 0, 0, 0.5f);
            popup.titleText = title;

            // Mensaje
            TextMeshProUGUI message = UIFactory.CreateText(
                panelObj.transform,
                "Message",
                "Estás a punto de cerrar sesión\nen tu cuenta",
                28,
                Color.white,
                TMPro.TextAlignmentOptions.Center
            );
            RectTransform messageRT = message.GetComponent<RectTransform>();
            messageRT.anchoredPosition = new Vector2(0, 20);
            messageRT.sizeDelta = new Vector2(600, 120);
            popup.messageText = message;

            // Botón Confirmar
            Button confirmBtn = UIFactory.CreateButton(
                panelObj.transform,
                "ConfirmButton",
                "CONFIRMAR",
                new Vector2(300, 70),
                UIFactory.BrightGreen
            );
            RectTransform confirmRT = confirmBtn.GetComponent<RectTransform>();
            confirmRT.anchoredPosition = new Vector2(-160, -110);
            AddRoundedCorners(confirmBtn.gameObject, 15f);
            popup.confirmButton = confirmBtn;

            // Botón Cancelar
            Button cancelBtn = UIFactory.CreateButton(
                panelObj.transform,
                "CancelButton",
                "CANCELAR",
                new Vector2(300, 70),
                new Color(0.5f, 0.2f, 0.2f)
            );
            RectTransform cancelRT = cancelBtn.GetComponent<RectTransform>();
            cancelRT.anchoredPosition = new Vector2(160, -110);
            AddRoundedCorners(cancelBtn.gameObject, 15f);
            popup.cancelButton = cancelBtn;

            // Configurar listeners
            confirmBtn.onClick.AddListener(popup.OnConfirmClicked);
            cancelBtn.onClick.AddListener(popup.OnCancelClicked);

            return popup;
        }

        private static void AddRoundedCorners(GameObject target, float radius)
        {
            Image image = target.GetComponent<Image>();
            if (image != null)
            {
                Outline outline = target.GetComponent<Outline>();
                if (outline == null)
                {
                    outline = target.AddComponent<Outline>();
                }
                outline.effectColor = new Color(0, 0, 0, 0.3f);
                outline.effectDistance = new Vector2(2, -2);
            }
        }
    }
}
