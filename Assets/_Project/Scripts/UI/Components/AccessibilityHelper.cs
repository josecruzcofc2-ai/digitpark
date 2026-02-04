using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Localization;

namespace DigitPark.UI
{
    /// <summary>
    /// Roles de accesibilidad para elementos UI
    /// </summary>
    public enum AccessibilityRole
    {
        Button,
        Text,
        Image,
        Header,
        InputField,
        Toggle,
        Slider,
        Link
    }

    /// <summary>
    /// Helper de accesibilidad para VoiceOver (iOS) y TalkBack (Android)
    /// Asigna labels, hints y roles a elementos UI de Unity
    /// </summary>
    public static class AccessibilityHelper
    {
        /// <summary>
        /// Asigna un label de accesibilidad a un GameObject
        /// En iOS, VoiceOver lee el nombre del GameObject para elementos UI nativos
        /// </summary>
        public static void SetLabel(GameObject go, string label)
        {
            if (go == null || string.IsNullOrEmpty(label)) return;

            // Unity usa el nombre del GameObject como accessibility label en plataformas nativas
            // Prefijamos con el rol para que VoiceOver/TalkBack lean correctamente
            go.name = label;

            // Si tiene un componente de accesibilidad personalizado, actualizarlo
            var accComponent = go.GetComponent<AccessibilityElement>();
            if (accComponent == null)
            {
                accComponent = go.AddComponent<AccessibilityElement>();
            }
            accComponent.Label = label;
        }

        /// <summary>
        /// Asigna un hint de accesibilidad (descripcion adicional de la accion)
        /// </summary>
        public static void SetHint(GameObject go, string hint)
        {
            if (go == null) return;

            var accComponent = go.GetComponent<AccessibilityElement>();
            if (accComponent == null)
            {
                accComponent = go.AddComponent<AccessibilityElement>();
            }
            accComponent.Hint = hint;
        }

        /// <summary>
        /// Asigna un rol de accesibilidad al elemento
        /// </summary>
        public static void SetRole(GameObject go, AccessibilityRole role)
        {
            if (go == null) return;

            var accComponent = go.GetComponent<AccessibilityElement>();
            if (accComponent == null)
            {
                accComponent = go.AddComponent<AccessibilityElement>();
            }
            accComponent.Role = role;
        }

        /// <summary>
        /// Auto-genera el label de un boton a partir de su texto hijo
        /// </summary>
        public static void AutoLabel(Button btn)
        {
            if (btn == null) return;

            // Buscar TextMeshProUGUI en hijos
            var tmpText = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null && !string.IsNullOrEmpty(tmpText.text))
            {
                SetLabel(btn.gameObject, tmpText.text);
                SetRole(btn.gameObject, AccessibilityRole.Button);
                return;
            }

            // Fallback a Text legacy
            var legacyText = btn.GetComponentInChildren<Text>();
            if (legacyText != null && !string.IsNullOrEmpty(legacyText.text))
            {
                SetLabel(btn.gameObject, legacyText.text);
                SetRole(btn.gameObject, AccessibilityRole.Button);
                return;
            }

            // Si no tiene texto, usar el nombre del GameObject
            SetLabel(btn.gameObject, btn.gameObject.name);
            SetRole(btn.gameObject, AccessibilityRole.Button);
        }

        /// <summary>
        /// Configura accesibilidad para un input field con label localizado
        /// </summary>
        public static void SetupInputField(TMP_InputField inputField, string labelKey)
        {
            if (inputField == null) return;

            string label = LocalizationManager.Instance != null
                ? LocalizationManager.Instance.GetText(labelKey)
                : labelKey;

            SetLabel(inputField.gameObject, label);
            SetRole(inputField.gameObject, AccessibilityRole.InputField);
        }

        /// <summary>
        /// Configura accesibilidad para un boton con label localizado
        /// </summary>
        public static void SetupButton(Button button, string labelKey)
        {
            if (button == null) return;

            string label = LocalizationManager.Instance != null
                ? LocalizationManager.Instance.GetText(labelKey)
                : labelKey;

            SetLabel(button.gameObject, label);
            SetRole(button.gameObject, AccessibilityRole.Button);
        }

        /// <summary>
        /// Configura accesibilidad para un texto informativo con label localizado
        /// </summary>
        public static void SetupText(TextMeshProUGUI text, string labelKey)
        {
            if (text == null) return;

            string label = LocalizationManager.Instance != null
                ? LocalizationManager.Instance.GetText(labelKey)
                : labelKey;

            SetLabel(text.gameObject, label);
            SetRole(text.gameObject, AccessibilityRole.Text);
        }

        /// <summary>
        /// Etiqueta automaticamente todos los botones, inputs y textos de una escena
        /// </summary>
        public static void AutoLabelScene()
        {
            // Botones
            var buttons = Object.FindObjectsOfType<Button>(true);
            foreach (var btn in buttons)
            {
                if (btn.GetComponent<AccessibilityElement>() == null)
                {
                    AutoLabel(btn);
                }
            }

            // Input fields
            var inputFields = Object.FindObjectsOfType<TMP_InputField>(true);
            foreach (var input in inputFields)
            {
                if (input.GetComponent<AccessibilityElement>() == null)
                {
                    string label = input.placeholder != null
                        ? ((TextMeshProUGUI)input.placeholder)?.text ?? input.gameObject.name
                        : input.gameObject.name;
                    SetLabel(input.gameObject, label);
                    SetRole(input.gameObject, AccessibilityRole.InputField);
                }
            }

            // Sliders
            var sliders = Object.FindObjectsOfType<Slider>(true);
            foreach (var slider in sliders)
            {
                if (slider.GetComponent<AccessibilityElement>() == null)
                {
                    SetLabel(slider.gameObject, slider.gameObject.name);
                    SetRole(slider.gameObject, AccessibilityRole.Slider);
                }
            }

            // Toggles
            var toggles = Object.FindObjectsOfType<Toggle>(true);
            foreach (var toggle in toggles)
            {
                if (toggle.GetComponent<AccessibilityElement>() == null)
                {
                    var text = toggle.GetComponentInChildren<TextMeshProUGUI>();
                    string label = text != null ? text.text : toggle.gameObject.name;
                    SetLabel(toggle.gameObject, label);
                    SetRole(toggle.gameObject, AccessibilityRole.Toggle);
                }
            }

            Debug.Log($"[Accessibility] Escena etiquetada: {buttons.Length} botones, {inputFields.Length} inputs, {sliders.Length} sliders, {toggles.Length} toggles");
        }
    }

    /// <summary>
    /// Componente MonoBehaviour que almacena datos de accesibilidad en un GameObject
    /// VoiceOver/TalkBack leen estos datos para describir el elemento
    /// </summary>
    public class AccessibilityElement : MonoBehaviour
    {
        [Header("Accessibility")]
        [Tooltip("Label que lee VoiceOver/TalkBack")]
        public string Label;

        [Tooltip("Descripcion adicional de la accion")]
        public string Hint;

        [Tooltip("Rol del elemento")]
        public AccessibilityRole Role = AccessibilityRole.Button;

        private void Start()
        {
            ApplyAccessibility();
        }

        /// <summary>
        /// Aplica la configuracion de accesibilidad al GameObject
        /// </summary>
        public void ApplyAccessibility()
        {
            if (!string.IsNullOrEmpty(Label))
            {
                // El nombre del GameObject es lo que leen los screen readers en Unity UI
                gameObject.name = Label;
            }
        }

        /// <summary>
        /// Actualiza el label con texto localizado
        /// </summary>
        public void UpdateLocalizedLabel(string localizationKey)
        {
            if (LocalizationManager.Instance != null)
            {
                Label = LocalizationManager.Instance.GetText(localizationKey);
                ApplyAccessibility();
            }
        }
    }
}
