using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using DigitPark.Localization;

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
            // Update texts via localization (supports runtime language changes)
            if (titleText != null) titleText.text = AutoLocalizer.Get("logout_title");
            if (messageText != null) messageText.text = AutoLocalizer.Get("logout_confirm_message");

            // Update button texts
            var confirmTmp = confirmButton?.GetComponentInChildren<TextMeshProUGUI>();
            if (confirmTmp != null) confirmTmp.text = AutoLocalizer.Get("popup_confirm_button");
            var cancelTmp = cancelButton?.GetComponentInChildren<TextMeshProUGUI>();
            if (cancelTmp != null) cancelTmp.text = AutoLocalizer.Get("popup_cancel_button");

            if (panel != null)
            {
                panel.SetActive(true);
                panel.transform.SetAsLastSibling();

                var cg = panel.GetComponent<CanvasGroup>();
                if (cg == null) cg = panel.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                panel.transform.localScale = Vector3.one * 0.85f;
                DOTween.Sequence()
                    .Join(panel.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack))
                    .Join(cg.DOFade(1f, 0.25f))
                    .SetUpdate(true)
                    .SetLink(panel);
            }

            onConfirm = onConfirmCallback;
        }

        /// <summary>
        /// Oculta el popup
        /// </summary>
        public void Hide()
        {
            if (panel != null)
            {
                var cg = panel.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    DOTween.Sequence()
                        .Join(panel.transform.DOScale(0.9f, 0.2f).SetEase(Ease.InQuad))
                        .Join(cg.DOFade(0f, 0.2f))
                        .OnComplete(() => { panel.SetActive(false); Destroy(gameObject); })
                        .SetUpdate(true)
                        .SetLink(panel);
                }
                else
                {
                    panel.SetActive(false);
                    Destroy(gameObject);
                }
            }
            else
            {
                Destroy(gameObject);
            }

            onConfirm = null;
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
                "LOG OUT",
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
                "You are about to sign out of your account",
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
                "CONFIRM",
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
                "CANCEL",
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

        private void OnDestroy()
        {
            transform.DOKill(true);
        }
    }
}
