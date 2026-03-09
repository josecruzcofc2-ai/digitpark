using UnityEngine;
using TMPro;
using System.Collections;

namespace DigitPark.Localization
{
    /// <summary>
    /// Componente para textos localizados
    /// Agrega este componente a cualquier TextMeshProUGUI que necesite traducción
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizedTextComponent : MonoBehaviour
    {
        [Tooltip("Clave de traducción (ej: login_title, play_button)")]
        public string textKey;

        private TextMeshProUGUI textComponent;
        private bool isSubscribed = false;

        private void Awake()
        {
            textComponent = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            StartCoroutine(InitializeWithDelay());
        }

        private IEnumerator InitializeWithDelay()
        {
            // Esperar un frame para asegurar que LocalizationManager esté listo
            yield return null;

            UpdateText();
        }

        private void OnEnable()
        {
            // Suscribirse al evento de cambio de idioma
            if (!isSubscribed)
            {
                LocalizationManager.OnLanguageChanged += UpdateText;
                isSubscribed = true;
            }

            // Actualizar después de un pequeño delay
            StartCoroutine(UpdateAfterFrame());
        }

        private IEnumerator UpdateAfterFrame()
        {
            yield return null;
            UpdateText();
        }

        private void OnDisable()
        {
            // Desuscribirse para evitar memory leaks
            if (isSubscribed)
            {
                LocalizationManager.OnLanguageChanged -= UpdateText;
                isSubscribed = false;
            }
        }

        /// <summary>
        /// Actualiza el texto con la traducción actual
        /// </summary>
        public void UpdateText()
        {
            if (textComponent == null)
                textComponent = GetComponent<TextMeshProUGUI>();

            if (textComponent == null || string.IsNullOrEmpty(textKey))
                return;

            if (LocalizationManager.Instance != null)
            {
                textComponent.text = LocalizationManager.Instance.GetText(textKey);
            }
        }

        /// <summary>
        /// Cambia la clave de traducción y actualiza el texto
        /// </summary>
        public void SetKey(string newKey)
        {
            textKey = newKey;
            UpdateText();
        }
    }
}
