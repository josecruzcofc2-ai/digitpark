using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DigitPark.Services.Firebase;
using DigitPark.Data;
using DigitPark.Localization;
using DigitPark.UI.Panels;

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

        [Header("UI - Buttons")]
        [SerializeField] public Button createAccountButton;
        [SerializeField] public Button backButton;

        [Header("UI - Loading")]
        [SerializeField] public GameObject loadingPanel;

        [Header("UI - Panels (Prefabs)")]
        [SerializeField] private ErrorPanelUI errorPanel;

        private bool isRegistering = false;

        private void Start()
        {
            Debug.Log("[Register] RegisterManager iniciado");

            // Verificar e inicializar servicios si no existen (para testing directo)
            EnsureServicesExist();

            // Configurar listeners
            SetupListeners();

            // Ocultar error panel inicialmente
            HideError();

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
            createAccountButton?.onClick.AddListener(OnCreateAccountClicked);
            backButton?.onClick.AddListener(OnBackButtonClicked);

            // El ErrorPanelUI maneja su propio botón internamente

            // Listeners para Enter key
            confirmPasswordInput?.onEndEdit.AddListener(delegate { if (Input.GetKeyDown(KeyCode.Return)) OnCreateAccountClicked(); });
        }

        /// <summary>
        /// Maneja el click en el botón CREAR CUENTA
        /// </summary>
        private async void OnCreateAccountClicked()
        {
            if (isRegistering) return;

            // Obtener valores
            string username = usernameInput?.text.Trim() ?? "";
            string email = emailInput?.text.Trim() ?? "";
            string password = passwordInput?.text ?? "";
            string confirmPassword = confirmPasswordInput?.text ?? "";

            // Validar todos los campos
            if (!ValidateAllFields(username, email, password, confirmPassword))
            {
                return;
            }

            isRegistering = true;
            ShowLoading(true);
            HideError();

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
        }

        /// <summary>
        /// Vuelve a la escena de Login
        /// </summary>
        private void OnBackButtonClicked()
        {
            Debug.Log("[Register] Volviendo a Login");
            SceneManager.LoadScene("Login");
        }

        /// <summary>
        /// Valida todos los campos del formulario
        /// </summary>
        private bool ValidateAllFields(string username, string email, string password, string confirmPassword)
        {
            // Validar username primero (es requerido ahora)
            if (string.IsNullOrEmpty(username))
            {
                ShowError(GetLocalizedText("error_username_empty"));
                return false;
            }

            if (username.Length < 3)
            {
                ShowError(GetLocalizedText("error_username_too_short"));
                return false;
            }

            if (username.Length > 20)
            {
                ShowError(GetLocalizedText("error_username_too_long"));
                return false;
            }

            // Validar caracteres permitidos (letras, números, guión bajo)
            if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
            {
                ShowError(GetLocalizedText("error_username_invalid_chars"));
                return false;
            }

            // Validar email
            if (string.IsNullOrEmpty(email))
            {
                ShowError(GetLocalizedText("error_email_empty"));
                return false;
            }

            if (!IsValidEmail(email))
            {
                ShowError(GetLocalizedText("error_email_invalid"));
                return false;
            }

            // Validar contraseña
            if (string.IsNullOrEmpty(password))
            {
                ShowError(GetLocalizedText("error_password_empty"));
                return false;
            }

            if (password.Length < 6)
            {
                ShowError(GetLocalizedText("error_password_too_short"));
                return false;
            }

            // Validar confirmación de contraseña
            if (string.IsNullOrEmpty(confirmPassword))
            {
                ShowError(GetLocalizedText("error_confirm_password_empty"));
                return false;
            }

            if (password != confirmPassword)
            {
                ShowError(GetLocalizedText("error_passwords_not_match"));
                return false;
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
            ShowError(friendlyMessage);
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
        /// Muestra el panel de error con un mensaje
        /// </summary>
        private void ShowError(string message)
        {
            Debug.Log($"[Register] Mostrando error: {message}");
            errorPanel?.Show(message);
        }

        /// <summary>
        /// Oculta el panel de error
        /// </summary>
        public void HideError()
        {
            errorPanel?.Hide();
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
            if (createAccountButton != null) createAccountButton.interactable = interactable;
            if (backButton != null) backButton.interactable = interactable;
        }

        /// <summary>
        /// Convierte mensajes de error técnicos en mensajes amigables localizados
        /// </summary>
        private string GetFriendlyErrorMessage(string technicalError)
        {
            if (technicalError.Contains("auth/email-already-in-use"))
                return GetLocalizedText("error_email_already_registered");

            if (technicalError.Contains("auth/invalid-email"))
                return GetLocalizedText("error_email_invalid");

            if (technicalError.Contains("auth/weak-password"))
                return GetLocalizedText("error_password_weak");

            if (technicalError.Contains("auth/network-request-failed"))
                return GetLocalizedText("error_no_connection");

            if (technicalError.Contains("timeout"))
                return GetLocalizedText("error_timeout");

            return GetLocalizedText("error_create_account");
        }

        /// <summary>
        /// Obtiene texto localizado usando LocalizationManager
        /// </summary>
        private string GetLocalizedText(string key)
        {
            if (LocalizationManager.Instance != null)
            {
                return LocalizationManager.Instance.GetText(key);
            }
            return key;
        }

        #endregion
    }
}
