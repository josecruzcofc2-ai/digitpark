using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DigitPark.Localization;

namespace DigitPark.UI.Common
{
    /// <summary>
    /// Popup para solicitar nombre de usuario
    /// Usado en Login (primera vez) y MainMenu (si no tiene username)
    /// </summary>
    public class UsernamePopup : MonoBehaviour
    {
        private GameObject popupPanel;
        private TMP_InputField usernameInput;
        private Button confirmButton;
        private Button laterButton; // Para Login primera vez
        private Button cancelButton; // Para MainMenu

        private Action<string> onConfirmCallback;
        private Action onCancelCallback;

#pragma warning disable 0414
        private bool isFirstTime; // Para saber qué botones mostrar
#pragma warning restore 0414

        /// <summary>
        /// Crea un popup de username
        /// </summary>
        public static UsernamePopup Create(Transform parent)
        {
            GameObject popupObj = new GameObject("UsernamePopup");
            popupObj.transform.SetParent(parent, false);

            UsernamePopup popup = popupObj.AddComponent<UsernamePopup>();
            popup.BuildPopup();

            return popup;
        }

        private void BuildPopup()
        {
            // Panel principal semi-transparente de fondo
            GameObject overlay = new GameObject("Overlay");
            overlay.transform.SetParent(transform, false);

            RectTransform overlayRT = overlay.AddComponent<RectTransform>();
            overlayRT.anchorMin = Vector2.zero;
            overlayRT.anchorMax = Vector2.one;
            overlayRT.sizeDelta = Vector2.zero;

            Image overlayImg = overlay.AddComponent<Image>();
            overlayImg.color = new Color(0, 0, 0, 0.7f);

            // Panel del popup
            popupPanel = new GameObject("Panel");
            popupPanel.transform.SetParent(overlay.transform, false);

            RectTransform panelRT = popupPanel.AddComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.5f, 0.5f);
            panelRT.anchorMax = new Vector2(0.5f, 0.5f);
            panelRT.pivot = new Vector2(0.5f, 0.5f);
            panelRT.sizeDelta = new Vector2(600, 350);
            panelRT.anchoredPosition = Vector2.zero;

            Image panelImg = popupPanel.AddComponent<Image>();
            panelImg.color = new Color(0.1f, 0.1f, 0.2f, 0.98f);

            // Outline para el panel
            Outline outline = popupPanel.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.3f);
            outline.effectDistance = new Vector2(2, -2);

            CreateContent();
        }

        private void CreateContent()
        {
            // Título
            TextMeshProUGUI titleText = UIFactory.CreateText(
                popupPanel.transform,
                "Title",
                AutoLocalizer.Get("username_popup_title"),
                40,
                UIFactory.ElectricBlue,
                TMPro.TextAlignmentOptions.Center
            );
            RectTransform titleRT = titleText.GetComponent<RectTransform>();
            titleRT.anchoredPosition = new Vector2(0, 100);
            titleRT.sizeDelta = new Vector2(500, 60);

            // Input Field
            GameObject inputObj = new GameObject("UsernameInputField");
            inputObj.transform.SetParent(popupPanel.transform, false);

            RectTransform inputRT = inputObj.AddComponent<RectTransform>();
            inputRT.anchoredPosition = new Vector2(0, 20);
            inputRT.sizeDelta = new Vector2(450, 55);

            Image inputBg = inputObj.AddComponent<Image>();
            inputBg.color = new Color(0.2f, 0.2f, 0.3f);

            usernameInput = inputObj.AddComponent<TMP_InputField>();
            usernameInput.textViewport = inputRT;

            // Texto placeholder
            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(inputObj.transform, false);

            RectTransform placeholderRT = placeholderObj.AddComponent<RectTransform>();
            placeholderRT.anchorMin = Vector2.zero;
            placeholderRT.anchorMax = Vector2.one;
            placeholderRT.sizeDelta = Vector2.zero;
            placeholderRT.offsetMin = new Vector2(10, 0);
            placeholderRT.offsetMax = new Vector2(-10, 0);

            TextMeshProUGUI placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholderText.text = AutoLocalizer.Get("username_placeholder");
            placeholderText.fontSize = 24;
            placeholderText.color = new Color(0.5f, 0.5f, 0.5f);
            placeholderText.alignment = TMPro.TextAlignmentOptions.Left;

            // Texto del input
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(inputObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;
            textRT.offsetMin = new Vector2(10, 0);
            textRT.offsetMax = new Vector2(-10, 0);

            TextMeshProUGUI inputText = textObj.AddComponent<TextMeshProUGUI>();
            inputText.fontSize = 24;
            inputText.color = Color.white;
            inputText.alignment = TMPro.TextAlignmentOptions.Left;

            usernameInput.textComponent = inputText;
            usernameInput.placeholder = placeholderText;
            usernameInput.characterLimit = 20;

            CreateButtons();
        }

        private void CreateButtons()
        {
            // Botón Confirmar (azul)
            confirmButton = UIFactory.CreateButton(
                popupPanel.transform,
                "ConfirmButton",
                AutoLocalizer.Get("confirm_button"),
                new Vector2(200, 60),
                UIFactory.ElectricBlue
            );
            RectTransform confirmRT = confirmButton.GetComponent<RectTransform>();
            confirmRT.anchoredPosition = new Vector2(-110, -90);
            confirmButton.onClick.AddListener(OnConfirmClicked);

            AddRoundedCorners(confirmButton.gameObject, 12f);

            // Botón "Más tarde" (para Login primera vez) - Gris
            laterButton = UIFactory.CreateButton(
                popupPanel.transform,
                "LaterButton",
                AutoLocalizer.Get("later_button"),
                new Vector2(200, 60),
                new Color(0.4f, 0.4f, 0.45f)
            );
            RectTransform laterRT = laterButton.GetComponent<RectTransform>();
            laterRT.anchoredPosition = new Vector2(110, -90);
            laterButton.onClick.AddListener(OnLaterClicked);
            laterButton.gameObject.SetActive(false); // Oculto por defecto

            AddRoundedCorners(laterButton.gameObject, 12f);

            // Botón Cancelar (para MainMenu) - Rojo
            cancelButton = UIFactory.CreateButton(
                popupPanel.transform,
                "CancelButton",
                AutoLocalizer.Get("cancel_button"),
                new Vector2(200, 60),
                new Color(0.7f, 0.2f, 0.2f)
            );
            RectTransform cancelRT = cancelButton.GetComponent<RectTransform>();
            cancelRT.anchoredPosition = new Vector2(110, -90);
            cancelButton.onClick.AddListener(OnCancelClicked);
            cancelButton.gameObject.SetActive(false); // Oculto por defecto

            AddRoundedCorners(cancelButton.gameObject, 12f);
        }

        /// <summary>
        /// Muestra el popup para primera vez (Login)
        /// </summary>
        public void ShowForFirstTime(Action<string> onConfirm, Action onLater)
        {
            isFirstTime = true;
            onConfirmCallback = onConfirm;
            onCancelCallback = onLater;

            // Mostrar botón "Más tarde", ocultar "Cancelar"
            laterButton.gameObject.SetActive(true);
            cancelButton.gameObject.SetActive(false);

            // Cambiar título
            TextMeshProUGUI titleText = popupPanel.GetComponentInChildren<TextMeshProUGUI>();
            if (titleText != null)
            {
                titleText.text = AutoLocalizer.Get("username_popup_title");
            }

            gameObject.SetActive(true);
            usernameInput.text = "";
            usernameInput.Select();
        }

        /// <summary>
        /// Muestra el popup desde MainMenu (cuando no tiene username)
        /// </summary>
        public void ShowForUpdate(Action<string> onConfirm, Action onCancel = null)
        {
            isFirstTime = false;
            onConfirmCallback = onConfirm;
            onCancelCallback = onCancel;

            // Mostrar botón "Cancelar", ocultar "Más tarde"
            laterButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(true);

            // Cambiar título
            TextMeshProUGUI titleText = popupPanel.GetComponentInChildren<TextMeshProUGUI>();
            if (titleText != null)
            {
                titleText.text = AutoLocalizer.Get("username_popup_title");
            }

            gameObject.SetActive(true);
            usernameInput.text = "";
            usernameInput.Select();
        }

        /// <summary>
        /// Oculta el popup
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnConfirmClicked()
        {
            string username = usernameInput.text.Trim();

            if (string.IsNullOrEmpty(username))
            {
                Debug.LogWarning("[UsernamePopup] Nombre vacío");
                return;
            }

            if (username.Length < 3)
            {
                Debug.LogWarning("[UsernamePopup] Nombre muy corto (mínimo 3 caracteres)");
                return;
            }

            Debug.Log($"[UsernamePopup] Usuario confirmado: {username}");
            onConfirmCallback?.Invoke(username);
            Hide();
        }

        private void OnLaterClicked()
        {
            Debug.Log("[UsernamePopup] Usuario presionó 'Más tarde'");
            onCancelCallback?.Invoke();
            Hide();
        }

        private void OnCancelClicked()
        {
            Debug.Log("[UsernamePopup] Usuario canceló");
            onCancelCallback?.Invoke();
            Hide();
        }

        private void AddRoundedCorners(GameObject target, float radius)
        {
            Image image = target.GetComponent<Image>();
            if (image != null)
            {
                Outline outline = target.GetComponent<Outline>();
                if (outline == null)
                {
                    outline = target.AddComponent<Outline>();
                }
                outline.effectColor = new Color(0, 0, 0, 0.2f);
                outline.effectDistance = new Vector2(1, -1);
            }
        }
    }
}
