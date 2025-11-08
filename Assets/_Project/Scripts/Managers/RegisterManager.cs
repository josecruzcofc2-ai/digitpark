using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DigitPark.Services.Firebase;
using DigitPark.Data;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager de la escena Register
    /// Maneja la creación de nuevas cuentas con validaciones completas
    /// </summary>
    public class RegisterManager : MonoBehaviour
    {
        [Header("UI - Title")]
        [SerializeField] public TextMeshProUGUI titleText;

        [Header("UI - Input Fields")]
        [SerializeField] public TMP_InputField usernameInput;
        [SerializeField] public TMP_InputField emailInput;
        [SerializeField] public TMP_InputField passwordInput;
        [SerializeField] public TMP_InputField confirmPasswordInput;
        [SerializeField] public Toggle rememberMeToggle;

        [Header("UI - Buttons")]
        [SerializeField] public Button confirmButton;

        [Header("UI - Feedback")]
        [SerializeField] public TextMeshProUGUI errorMessageText;
        [SerializeField] public GameObject loadingPanel;

        private bool isRegistering = false;

        private void Start()
        {
            Debug.Log("[Register] RegisterManager iniciado");

            // Verificar e inicializar servicios si no existen (para testing directo)
            EnsureServicesExist();

            // Configurar listeners
            SetupListeners();

            // Suscribirse a eventos de autenticación
            if (AuthenticationService.Instance != null)
            {
                AuthenticationService.Instance.OnLoginSuccess += OnRegisterSuccess;
                AuthenticationService.Instance.OnLoginFailed += OnRegisterFailed;
            }
        }

        private void OnDestroy()
        {
            // Desuscribirse de eventos
            if (AuthenticationService.Instance != null)
            {
                AuthenticationService.Instance.OnLoginSuccess -= OnRegisterSuccess;
                AuthenticationService.Instance.OnLoginFailed -= OnRegisterFailed;
            }
        }

        /// <summary>
        /// Asegura que los servicios existan (para testing directo de escena)
        /// </summary>
        private void EnsureServicesExist()
        {
            if (AuthenticationService.Instance == null)
            {
                Debug.LogWarning("[Register] AuthenticationService no encontrado, creando instancia de respaldo...");
                GameObject authService = new GameObject("AuthenticationService");
                authService.AddComponent<AuthenticationService>();
            }

            if (DatabaseService.Instance == null)
            {
                Debug.LogWarning("[Register] DatabaseService no encontrado, creando instancia de respaldo...");
                GameObject dbService = new GameObject("DatabaseService");
                dbService.AddComponent<DatabaseService>();
            }
        }

        /// <summary>
        /// Configura los listeners de los botones
        /// </summary>
        private void SetupListeners()
        {
            confirmButton?.onClick.AddListener(OnConfirmButtonClicked);

            // Listeners para Enter key
            confirmPasswordInput?.onEndEdit.AddListener(delegate { if (Input.GetKeyDown(KeyCode.Return)) OnConfirmButtonClicked(); });
        }

        /// <summary>
        /// Maneja el click en el botón CONFIRMAR
        /// </summary>
        private async void OnConfirmButtonClicked()
        {
            if (isRegistering) return;

            // Obtener valores
            string username = usernameInput?.text.Trim() ?? "";
            string email = emailInput?.text.Trim() ?? "";
            string password = passwordInput?.text ?? "";
            string confirmPassword = confirmPasswordInput?.text ?? "";
            bool rememberMe = rememberMeToggle?.isOn ?? false;

            // Validar todos los campos
            if (!ValidateAllFields(username, email, password, confirmPassword))
            {
                return;
            }

            isRegistering = true;
            ShowLoading(true);
            ClearErrorMessage();

            Debug.Log($"[Register] Intentando registrar cuenta para: {email}");

            // Intentar registro
            bool success = await AuthenticationService.Instance.RegisterWithEmail(
                email,
                password,
                username // Puede ser vacío si es opcional
            );

            isRegistering = false;
            ShowLoading(false);

            if (!success)
            {
                // El error se maneja en OnRegisterFailed
                Debug.LogWarning("[Register] Registro fallido");
            }
            else
            {
                // Guardar preferencia de recordar
                if (rememberMe)
                {
                    PlayerPrefs.SetInt("RememberMe", 1);
                }
                else
                {
                    PlayerPrefs.SetInt("RememberMe", 0);
                }
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// Valida todos los campos del formulario
        /// </summary>
        private bool ValidateAllFields(string username, string email, string password, string confirmPassword)
        {
            // Validar email
            if (string.IsNullOrEmpty(email))
            {
                ShowErrorMessage("Por favor ingresa tu correo electrónico");
                return false;
            }

            if (!IsValidEmail(email))
            {
                ShowErrorMessage("El correo debe contener una @");
                return false;
            }

            // Validar contraseña
            if (string.IsNullOrEmpty(password))
            {
                ShowErrorMessage("Por favor ingresa una contraseña");
                return false;
            }

            if (password.Length < 8)
            {
                ShowErrorMessage("La contraseña debe tener al menos 8 caracteres");
                return false;
            }

            // Validar confirmación de contraseña
            if (password != confirmPassword)
            {
                ShowErrorMessage("Las contraseñas no coinciden");
                return false;
            }

            // Validar username (opcional, pero si se proporciona debe cumplir requisitos)
            if (!string.IsNullOrEmpty(username))
            {
                if (username.Length < 6)
                {
                    ShowErrorMessage("El nombre debe tener al menos 6 caracteres");
                    return false;
                }

                if (username.Length > 20)
                {
                    ShowErrorMessage("El nombre no puede tener más de 20 caracteres");
                    return false;
                }

                // Validar caracteres permitidos (letras, números, guión bajo)
                if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
                {
                    ShowErrorMessage("El nombre solo puede contener letras, números y guión bajo");
                    return false;
                }

                // TODO: Validar que el nombre de usuario no esté en uso
                // Esto requiere una consulta a la base de datos
            }

            return true;
        }

        /// <summary>
        /// Valida si un email es válido y contiene @
        /// </summary>
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            // Verificar que contenga @
            if (!email.Contains("@"))
                return false;

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


        /// <summary>
        /// Callback cuando el registro es exitoso
        /// </summary>
        private void OnRegisterSuccess(PlayerData playerData)
        {
            Debug.Log($"[Register] Registro exitoso para: {playerData.email}");

            // Registrar en analytics
            AnalyticsService.Instance?.SetUserId(playerData.userId);

            // Transición a MainMenu
            StartCoroutine(TransitionToMainMenu());
        }

        /// <summary>
        /// Callback cuando el registro falla
        /// </summary>
        private void OnRegisterFailed(string errorMessage)
        {
            Debug.LogError($"[Register] Registro fallido: {errorMessage}");

            // Mostrar mensaje de error amigable
            string friendlyMessage = GetFriendlyErrorMessage(errorMessage);
            ShowErrorMessage(friendlyMessage);
        }

        /// <summary>
        /// Transición animada al MainMenu
        /// </summary>
        private IEnumerator TransitionToMainMenu()
        {
            yield return new WaitForSeconds(0.5f);

            // Cargar escena MainMenu
            SceneManager.LoadScene("MainMenu");
        }

        #region UI Helpers

        /// <summary>
        /// Muestra un mensaje de error
        /// </summary>
        private void ShowErrorMessage(string message)
        {
            if (errorMessageText != null)
            {
                errorMessageText.text = message;
                errorMessageText.gameObject.SetActive(true);

                // Auto-ocultar después de 5 segundos
                StartCoroutine(HideErrorMessageAfterDelay(5f));
            }
        }

        /// <summary>
        /// Limpia el mensaje de error
        /// </summary>
        private void ClearErrorMessage()
        {
            if (errorMessageText != null)
            {
                errorMessageText.text = "";
                errorMessageText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Oculta el mensaje de error después de un delay
        /// </summary>
        private IEnumerator HideErrorMessageAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            ClearErrorMessage();
        }

        /// <summary>
        /// Muestra u oculta el panel de carga
        /// </summary>
        private void ShowLoading(bool show)
        {
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(show);
            }

            // Deshabilitar botones mientras carga
            SetButtonsInteractable(!show);
        }

        /// <summary>
        /// Habilita o deshabilita todos los botones
        /// </summary>
        private void SetButtonsInteractable(bool interactable)
        {
            if (confirmButton != null) confirmButton.interactable = interactable;
        }

        /// <summary>
        /// Convierte mensajes de error técnicos en mensajes amigables
        /// </summary>
        private string GetFriendlyErrorMessage(string technicalError)
        {
            if (technicalError.Contains("auth/email-already-in-use"))
                return "Este correo ya está registrado";

            if (technicalError.Contains("auth/invalid-email"))
                return "Correo inválido";

            if (technicalError.Contains("auth/weak-password"))
                return "La contraseña es muy débil";

            if (technicalError.Contains("auth/network-request-failed"))
                return "Error de conexión. Verifica tu internet";

            return "Error al crear la cuenta. Intenta nuevamente";
        }

        #endregion
    }
}
