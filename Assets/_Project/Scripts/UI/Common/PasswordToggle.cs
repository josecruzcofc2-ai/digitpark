using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DigitPark.UI.Common
{
    /// <summary>
    /// Controla la visibilidad de un campo de contraseña
    /// Alterna entre mostrar/ocultar el texto usando iconos
    /// </summary>
    public class PasswordToggle : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private TMP_InputField passwordInput;

        [Header("Iconos")]
        [SerializeField] private Sprite eyeOpenIcon;   // Ojo abierto (password oculto)
        [SerializeField] private Sprite eyeClosedIcon; // Ojo cerrado (password visible)

        private Image buttonImage;
        private bool isPasswordVisible = false;

        private void Start()
        {
            // Obtener el componente Image del botón
            buttonImage = GetComponent<Image>();

            if (passwordInput != null)
            {
                // Asegurar que inicie como password oculto
                passwordInput.contentType = TMP_InputField.ContentType.Password;
                UpdateIcon();
            }
            else
            {
                Debug.LogError("[PasswordToggle] passwordInput no está asignado!");
            }

            // Validar que los iconos estén asignados
            if (eyeOpenIcon == null || eyeClosedIcon == null)
            {
                Debug.LogWarning("[PasswordToggle] Los iconos no están asignados. Asigna eyeOpenIcon y eyeClosedIcon en el Inspector.");
            }
        }

        /// <summary>
        /// Alterna la visibilidad de la contraseña
        /// Llamar este método desde el onClick del botón
        /// </summary>
        public void TogglePasswordVisibility()
        {
            if (passwordInput == null)
            {
                Debug.LogError("[PasswordToggle] passwordInput es null!");
                return;
            }

            isPasswordVisible = !isPasswordVisible;

            if (isPasswordVisible)
            {
                // Mostrar contraseña
                passwordInput.contentType = TMP_InputField.ContentType.Standard;
            }
            else
            {
                // Ocultar contraseña
                passwordInput.contentType = TMP_InputField.ContentType.Password;
            }

            // Forzar actualización del campo para que el cambio sea visible
            passwordInput.ForceLabelUpdate();

            // Actualizar el icono del botón
            UpdateIcon();
        }

        /// <summary>
        /// Actualiza el icono del botón según el estado
        /// </summary>
        private void UpdateIcon()
        {
            if (buttonImage != null && eyeOpenIcon != null && eyeClosedIcon != null)
            {
                // Si la contraseña es visible, mostrar ojo cerrado (para indicar "ocultar")
                // Si la contraseña está oculta, mostrar ojo abierto (para indicar "mostrar")
                buttonImage.sprite = isPasswordVisible ? eyeClosedIcon : eyeOpenIcon;
            }
        }
    }
}
