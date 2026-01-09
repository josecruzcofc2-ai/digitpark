using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DigitPark.Localization;

namespace DigitPark.UI.Common
{
    /// <summary>
    /// Popup para recuperar contraseña olvidada
    /// Solicita el email y envía el link de recuperación via Firebase
    /// </summary>
    public class ForgotPasswordPopup : MonoBehaviour
    {
        private GameObject popupPanel;
        private TMP_InputField emailInput;
        private TextMeshProUGUI messageText;
        private Button sendButton;
        private Button cancelButton;

        private Action<string> onSendCallback;
        private Action onCancelCallback;

        /// <summary>
        /// Crea un popup de recuperación de contraseña
        /// </summary>
        public static ForgotPasswordPopup Create(Transform parent)
        {
            GameObject popupObj = new GameObject("ForgotPasswordPopup");
            popupObj.transform.SetParent(parent, false);

            ForgotPasswordPopup popup = popupObj.AddComponent<ForgotPasswordPopup>();
            popup.BuildPopup();

            return popup;
        }

        private void BuildPopup()
        {
            // Panel principal semi-transparente de fondo (overlay)
            GameObject overlay = new GameObject("Overlay");
            overlay.transform.SetParent(transform, false);

            RectTransform overlayRT = overlay.AddComponent<RectTransform>();
            overlayRT.anchorMin = Vector2.zero;
            overlayRT.anchorMax = Vector2.one;
            overlayRT.sizeDelta = Vector2.zero;

            Image overlayImg = overlay.AddComponent<Image>();
            overlayImg.color = new Color(0, 0, 0, 0.8f);

            // Botón invisible para cerrar al tocar fuera (opcional)
            Button overlayButton = overlay.AddComponent<Button>();
            overlayButton.onClick.AddListener(OnCancelClicked);
            ColorBlock colors = overlayButton.colors;
            colors.normalColor = Color.clear;
            colors.highlightedColor = Color.clear;
            colors.pressedColor = Color.clear;
            overlayButton.colors = colors;

            // Panel del popup
            popupPanel = new GameObject("Panel");
            popupPanel.transform.SetParent(overlay.transform, false);

            RectTransform panelRT = popupPanel.AddComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.5f, 0.5f);
            panelRT.anchorMax = new Vector2(0.5f, 0.5f);
            panelRT.pivot = new Vector2(0.5f, 0.5f);
            panelRT.sizeDelta = new Vector2(620, 420);
            panelRT.anchoredPosition = Vector2.zero;

            Image panelImg = popupPanel.AddComponent<Image>();
            panelImg.color = new Color(0.08f, 0.08f, 0.16f, 0.98f);

            // Outline neon para el panel
            Outline outline = popupPanel.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0.8f, 1f, 0.4f);
            outline.effectDistance = new Vector2(2, -2);

            CreateContent();
        }

        private void CreateContent()
        {
            // Título
            TextMeshProUGUI titleText = UIFactory.CreateText(
                popupPanel.transform,
                "Title",
                AutoLocalizer.Get("forgot_password_title"),
                36,
                UIFactory.ElectricBlue,
                TextAlignmentOptions.Center
            );
            RectTransform titleRT = titleText.GetComponent<RectTransform>();
            titleRT.anchoredPosition = new Vector2(0, 140);
            titleRT.sizeDelta = new Vector2(550, 50);

            // Descripción
            TextMeshProUGUI descText = UIFactory.CreateText(
                popupPanel.transform,
                "Description",
                AutoLocalizer.Get("forgot_password_description"),
                22,
                new Color(0.7f, 0.7f, 0.7f),
                TextAlignmentOptions.Center
            );
            RectTransform descRT = descText.GetComponent<RectTransform>();
            descRT.anchoredPosition = new Vector2(0, 80);
            descRT.sizeDelta = new Vector2(550, 60);

            // Input Field para email
            CreateEmailInput();

            // Mensaje de estado (éxito/error)
            messageText = UIFactory.CreateText(
                popupPanel.transform,
                "MessageText",
                "",
                20,
                Color.white,
                TextAlignmentOptions.Center
            );
            RectTransform msgRT = messageText.GetComponent<RectTransform>();
            msgRT.anchoredPosition = new Vector2(0, -40);
            msgRT.sizeDelta = new Vector2(550, 40);
            messageText.gameObject.SetActive(false);

            // Botones
            CreateButtons();
        }

        private void CreateEmailInput()
        {
            GameObject inputObj = new GameObject("EmailInputField");
            inputObj.transform.SetParent(popupPanel.transform, false);

            RectTransform inputRT = inputObj.AddComponent<RectTransform>();
            inputRT.anchoredPosition = new Vector2(0, 10);
            inputRT.sizeDelta = new Vector2(500, 60);

            Image inputBg = inputObj.AddComponent<Image>();
            inputBg.color = new Color(0.15f, 0.15f, 0.25f);

            // Outline sutil
            Outline inputOutline = inputObj.AddComponent<Outline>();
            inputOutline.effectColor = new Color(0f, 0.8f, 1f, 0.2f);
            inputOutline.effectDistance = new Vector2(1, -1);

            emailInput = inputObj.AddComponent<TMP_InputField>();
            emailInput.contentType = TMP_InputField.ContentType.EmailAddress;
            emailInput.characterLimit = 100;

            // Text Area
            GameObject textAreaObj = new GameObject("TextArea");
            textAreaObj.transform.SetParent(inputObj.transform, false);

            RectTransform textAreaRT = textAreaObj.AddComponent<RectTransform>();
            textAreaRT.anchorMin = Vector2.zero;
            textAreaRT.anchorMax = Vector2.one;
            textAreaRT.sizeDelta = Vector2.zero;
            textAreaRT.offsetMin = new Vector2(15, 0);
            textAreaRT.offsetMax = new Vector2(-15, 0);

            // Placeholder
            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(textAreaObj.transform, false);

            RectTransform placeholderRT = placeholderObj.AddComponent<RectTransform>();
            placeholderRT.anchorMin = Vector2.zero;
            placeholderRT.anchorMax = Vector2.one;
            placeholderRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholderText.text = AutoLocalizer.Get("placeholder_email");
            placeholderText.fontSize = 24;
            placeholderText.color = new Color(0.5f, 0.5f, 0.5f);
            placeholderText.alignment = TextAlignmentOptions.Left;

            // Texto del input
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(textAreaObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI inputText = textObj.AddComponent<TextMeshProUGUI>();
            inputText.fontSize = 24;
            inputText.color = Color.white;
            inputText.alignment = TextAlignmentOptions.Left;

            emailInput.textViewport = textAreaRT;
            emailInput.textComponent = inputText;
            emailInput.placeholder = placeholderText;
        }

        private void CreateButtons()
        {
            // Botón Enviar (cyan)
            sendButton = UIFactory.CreateButton(
                popupPanel.transform,
                "SendButton",
                AutoLocalizer.Get("send_button"),
                new Vector2(220, 55),
                UIFactory.ElectricBlue
            );
            RectTransform sendRT = sendButton.GetComponent<RectTransform>();
            sendRT.anchoredPosition = new Vector2(-120, -110);
            sendButton.onClick.AddListener(OnSendClicked);

            // Texto del botón en negro para contraste
            TextMeshProUGUI sendText = sendButton.GetComponentInChildren<TextMeshProUGUI>();
            if (sendText != null)
            {
                sendText.color = Color.black;
                sendText.fontSize = 24;
            }

            // Botón Cancelar (gris)
            cancelButton = UIFactory.CreateButton(
                popupPanel.transform,
                "CancelButton",
                AutoLocalizer.Get("cancel_button"),
                new Vector2(220, 55),
                new Color(0.3f, 0.3f, 0.4f)
            );
            RectTransform cancelRT = cancelButton.GetComponent<RectTransform>();
            cancelRT.anchoredPosition = new Vector2(120, -110);
            cancelButton.onClick.AddListener(OnCancelClicked);

            TextMeshProUGUI cancelText = cancelButton.GetComponentInChildren<TextMeshProUGUI>();
            if (cancelText != null)
            {
                cancelText.fontSize = 24;
            }
        }

        /// <summary>
        /// Muestra el popup
        /// </summary>
        /// <param name="onSend">Callback cuando el usuario presiona enviar (recibe el email)</param>
        /// <param name="onCancel">Callback cuando el usuario cancela</param>
        /// <param name="prefillEmail">Email para prellenar (opcional)</param>
        public void Show(Action<string> onSend, Action onCancel = null, string prefillEmail = "")
        {
            onSendCallback = onSend;
            onCancelCallback = onCancel;

            emailInput.text = prefillEmail;
            HideMessage();

            gameObject.SetActive(true);
            emailInput.Select();
        }

        /// <summary>
        /// Oculta el popup
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Muestra un mensaje de éxito
        /// </summary>
        public void ShowSuccess(string message)
        {
            messageText.text = message;
            messageText.color = new Color(0.3f, 1f, 0.5f); // Verde
            messageText.gameObject.SetActive(true);

            // Deshabilitar el botón de enviar después del éxito
            sendButton.interactable = false;
        }

        /// <summary>
        /// Muestra un mensaje de error
        /// </summary>
        public void ShowError(string message)
        {
            messageText.text = message;
            messageText.color = new Color(1f, 0.4f, 0.4f); // Rojo
            messageText.gameObject.SetActive(true);
        }

        /// <summary>
        /// Oculta el mensaje
        /// </summary>
        public void HideMessage()
        {
            messageText.gameObject.SetActive(false);
            sendButton.interactable = true;
        }

        /// <summary>
        /// Habilita/deshabilita el botón de enviar (para mostrar loading)
        /// </summary>
        public void SetSendButtonInteractable(bool interactable)
        {
            sendButton.interactable = interactable;
        }

        private void OnSendClicked()
        {
            string email = emailInput.text.Trim();

            if (string.IsNullOrEmpty(email))
            {
                ShowError(AutoLocalizer.Get("error_email_empty"));
                return;
            }

            if (!IsValidEmail(email))
            {
                ShowError(AutoLocalizer.Get("error_email_invalid"));
                return;
            }

            Debug.Log($"[ForgotPasswordPopup] Enviando recuperación a: {email}");
            onSendCallback?.Invoke(email);
        }

        private void OnCancelClicked()
        {
            Debug.Log("[ForgotPasswordPopup] Usuario canceló");
            onCancelCallback?.Invoke();
            Hide();
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return false;
            if (!email.Contains("@")) return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
