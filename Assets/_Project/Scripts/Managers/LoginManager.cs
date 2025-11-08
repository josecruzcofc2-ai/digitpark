using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DigitPark.Services.Firebase;
using DigitPark.Data;
using DigitPark.UI.Common;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager de la escena Login
    /// Maneja autenticación de usuarios (Email, Google, Apple) y registro
    /// </summary>
    public class LoginManager : MonoBehaviour
    {
        [Header("UI - Panels")]
        [SerializeField] public GameObject loginPanel;
        [SerializeField] public GameObject registerPanel;

        [Header("UI - Login Fields")]
        [SerializeField] public TMP_InputField loginEmailInput;
        [SerializeField] public TMP_InputField loginPasswordInput;
        [SerializeField] public Toggle rememberMeToggle;
        [SerializeField] public Button loginButton;
        [SerializeField] public Button showRegisterButton;

        [Header("UI - Register Fields")]
        [SerializeField] public TMP_InputField registerUsernameInput;
        [SerializeField] public TMP_InputField registerEmailInput;
        [SerializeField] public TMP_InputField registerPasswordInput;
        [SerializeField] public TMP_InputField registerConfirmPasswordInput;
        [SerializeField] public Button registerButton;
        [SerializeField] public Button backToLoginButton;

        [Header("UI - Social Login")]
        [SerializeField] public Button googleLoginButton;
        [SerializeField] public Button appleLoginButton;

        [Header("UI - Other")]
        [SerializeField] public Button forgotPasswordButton;
        [SerializeField] public TextMeshProUGUI errorMessageText;
        [SerializeField] public GameObject loadingPanel;

        [Header("UI - Title")]
        [SerializeField] public TextMeshProUGUI titleText;
        [SerializeField] public Animator titleAnimator;

        private bool isLoggingIn = false;
        private UsernamePopup usernamePopup;
        private PlayerData currentPlayerData;

        private void Start()
        {
            Debug.Log("[Login] LoginManager iniciado");

            // Verificar e inicializar servicios si no existen (para testing directo)
            EnsureServicesExist();

            // Configurar listeners
            SetupListeners();

            // Mostrar panel de login por defecto
            ShowLoginPanel();

            // Animar título
            if (titleAnimator != null)
            {
                titleAnimator.SetTrigger("Show");
            }

            // Suscribirse a eventos de autenticación
            if (AuthenticationService.Instance != null)
            {
                AuthenticationService.Instance.OnLoginSuccess += OnLoginSuccess;
                AuthenticationService.Instance.OnLoginFailed += OnLoginFailed;
            }

            // Cargar remember me si existe
            if (PlayerPrefs.HasKey("RememberMe"))
            {
                rememberMeToggle.isOn = PlayerPrefs.GetInt("RememberMe", 0) == 1;
            }

            // Crear popup de username (oculto inicialmente)
            CreateUsernamePopup();
        }

        /// <summary>
        /// Crea el popup de username para primera vez
        /// </summary>
        private void CreateUsernamePopup()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                usernamePopup = UsernamePopup.Create(canvas.transform);
                usernamePopup.Hide();
            }
        }

        /// <summary>
        /// Asegura que los servicios existan (para testing directo de escena)
        /// </summary>
        private void EnsureServicesExist()
        {
            if (AuthenticationService.Instance == null)
            {
                Debug.LogWarning("[Login] AuthenticationService no encontrado, creando instancia de respaldo...");
                GameObject authService = new GameObject("AuthenticationService");
                authService.AddComponent<AuthenticationService>();
            }

            if (DatabaseService.Instance == null)
            {
                Debug.LogWarning("[Login] DatabaseService no encontrado, creando instancia de respaldo...");
                GameObject dbService = new GameObject("DatabaseService");
                dbService.AddComponent<DatabaseService>();
            }
        }

        private void OnDestroy()
        {
            // Desuscribirse de eventos
            if (AuthenticationService.Instance != null)
            {
                AuthenticationService.Instance.OnLoginSuccess -= OnLoginSuccess;
                AuthenticationService.Instance.OnLoginFailed -= OnLoginFailed;
            }
        }

        /// <summary>
        /// Configura los listeners de los botones
        /// </summary>
        private void SetupListeners()
        {
            loginButton?.onClick.AddListener(OnLoginButtonClicked);
            showRegisterButton?.onClick.AddListener(GoToRegisterScene); // Cambiado a navegación de escena
            registerButton?.onClick.AddListener(OnRegisterButtonClicked);
            backToLoginButton?.onClick.AddListener(ShowLoginPanel);
            googleLoginButton?.onClick.AddListener(OnGoogleLoginClicked);
            appleLoginButton?.onClick.AddListener(OnAppleLoginClicked);
            forgotPasswordButton?.onClick.AddListener(OnForgotPasswordClicked);

            // Listeners para Enter key
            loginPasswordInput?.onEndEdit.AddListener(delegate { if (Input.GetKeyDown(KeyCode.Return)) OnLoginButtonClicked(); });
            registerConfirmPasswordInput?.onEndEdit.AddListener(delegate { if (Input.GetKeyDown(KeyCode.Return)) OnRegisterButtonClicked(); });
        }

        /// <summary>
        /// Navega a la escena Register
        /// </summary>
        private void GoToRegisterScene()
        {
            Debug.Log("[Login] Navegando a Register");
            SceneManager.LoadScene("Register");
        }

        #region UI Navigation

        /// <summary>
        /// Muestra el panel de login
        /// </summary>
        private void ShowLoginPanel()
        {
            loginPanel?.SetActive(true);
            registerPanel?.SetActive(false);
            ClearErrorMessage();

            Debug.Log("[Login] Mostrando panel de login");
        }

        /// <summary>
        /// Muestra el panel de registro
        /// </summary>
        private void ShowRegisterPanel()
        {
            loginPanel?.SetActive(false);
            registerPanel?.SetActive(true);
            ClearErrorMessage();

            Debug.Log("[Login] Mostrando panel de registro");
        }

        #endregion

        #region Login

        /// <summary>
        /// Maneja el click en el botón de login
        /// </summary>
        private async void OnLoginButtonClicked()
        {
            if (isLoggingIn) return;

            // Validar campos
            string email = loginEmailInput.text.Trim();
            string password = loginPasswordInput.text;

            if (!ValidateLoginInputs(email, password))
            {
                return;
            }

            isLoggingIn = true;
            ShowLoading(true);
            ClearErrorMessage();

            Debug.Log($"[Login] Intentando login para: {email}");

            // Intentar login
            bool success = await AuthenticationService.Instance.LoginWithEmail(
                email,
                password,
                rememberMeToggle.isOn
            );

            isLoggingIn = false;
            ShowLoading(false);

            if (!success)
            {
                // El error se maneja en OnLoginFailed
                Debug.LogWarning("[Login] Login fallido");
            }
        }

        /// <summary>
        /// Valida los inputs de login
        /// </summary>
        private bool ValidateLoginInputs(string email, string password)
        {
            if (string.IsNullOrEmpty(email))
            {
                ShowErrorMessage("Por favor ingresa tu email");
                return false;
            }

            if (!IsValidEmail(email))
            {
                ShowErrorMessage("Email inválido");
                return false;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowErrorMessage("Por favor ingresa tu contraseña");
                return false;
            }

            if (password.Length < 8)
            {
                ShowErrorMessage("La contraseña debe tener al menos 8 caracteres");
                return false;
            }

            return true;
        }

        #endregion

        #region Register

        /// <summary>
        /// Maneja el click en el botón de registro
        /// </summary>
        private async void OnRegisterButtonClicked()
        {
            if (isLoggingIn) return;

            // Validar campos
            string username = registerUsernameInput.text.Trim();
            string email = registerEmailInput.text.Trim();
            string password = registerPasswordInput.text;
            string confirmPassword = registerConfirmPasswordInput.text;

            if (!ValidateRegisterInputs(username, email, password, confirmPassword))
            {
                return;
            }

            isLoggingIn = true;
            ShowLoading(true);
            ClearErrorMessage();

            Debug.Log($"[Login] Intentando registro para: {email}");

            // Intentar registro
            bool success = await AuthenticationService.Instance.RegisterWithEmail(
                email,
                password,
                username
            );

            isLoggingIn = false;
            ShowLoading(false);

            if (!success)
            {
                Debug.LogWarning("[Login] Registro fallido");
            }
        }

        /// <summary>
        /// Valida los inputs de registro
        /// </summary>
        private bool ValidateRegisterInputs(string username, string email, string password, string confirmPassword)
        {
            if (string.IsNullOrEmpty(username))
            {
                ShowErrorMessage("Por favor ingresa un nombre de usuario");
                return false;
            }

            if (username.Length < 3)
            {
                ShowErrorMessage("El nombre de usuario debe tener al menos 3 caracteres");
                return false;
            }

            if (string.IsNullOrEmpty(email))
            {
                ShowErrorMessage("Por favor ingresa tu email");
                return false;
            }

            if (!IsValidEmail(email))
            {
                ShowErrorMessage("Email inválido");
                return false;
            }

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

            if (password != confirmPassword)
            {
                ShowErrorMessage("Las contraseñas no coinciden");
                return false;
            }

            return true;
        }

        #endregion

        #region Social Login

        /// <summary>
        /// Maneja el login con Google
        /// </summary>
        private async void OnGoogleLoginClicked()
        {
            if (isLoggingIn) return;

            isLoggingIn = true;
            ShowLoading(true);
            ClearErrorMessage();

            Debug.Log("[Login] Intentando login con Google");

            bool success = await AuthenticationService.Instance.LoginWithGoogle();

            isLoggingIn = false;
            ShowLoading(false);

            if (!success)
            {
                ShowErrorMessage("Error al iniciar sesión con Google");
            }
        }

        /// <summary>
        /// Maneja el login con Apple
        /// </summary>
        private async void OnAppleLoginClicked()
        {
            if (isLoggingIn) return;

            #if !UNITY_IOS
            ShowErrorMessage("Login con Apple solo disponible en iOS");
            return;
            #endif

            isLoggingIn = true;
            ShowLoading(true);
            ClearErrorMessage();

            Debug.Log("[Login] Intentando login con Apple");

            bool success = await AuthenticationService.Instance.LoginWithApple();

            isLoggingIn = false;
            ShowLoading(false);

            if (!success)
            {
                ShowErrorMessage("Error al iniciar sesión con Apple");
            }
        }

        #endregion

        #region Forgot Password

        /// <summary>
        /// Maneja el click en olvidé mi contraseña
        /// </summary>
        private async void OnForgotPasswordClicked()
        {
            string email = loginEmailInput.text.Trim();

            if (string.IsNullOrEmpty(email) || !IsValidEmail(email))
            {
                ShowErrorMessage("Por favor ingresa un email válido primero");
                return;
            }

            ShowLoading(true);
            ClearErrorMessage();

            bool success = await AuthenticationService.Instance.ResetPassword(email);

            ShowLoading(false);

            if (success)
            {
                ShowErrorMessage("Email de recuperación enviado. Revisa tu correo.", Color.green);
            }
            else
            {
                ShowErrorMessage("Error al enviar email de recuperación");
            }
        }

        #endregion

        #region Callbacks

        /// <summary>
        /// Callback cuando el login es exitoso
        /// </summary>
        private void OnLoginSuccess(PlayerData playerData)
        {
            Debug.Log($"[Login] Login exitoso para: {playerData.username}");

            // Guardar referencia al jugador
            currentPlayerData = playerData;

            // Registrar en analytics
            AnalyticsService.Instance?.SetUserId(playerData.userId);

            // Verificar si es primera vez (no tiene username o es "Sin usuario")
            if (string.IsNullOrEmpty(playerData.username) || playerData.username == "Sin usuario")
            {
                Debug.Log("[Login] Primera vez - mostrando popup de username");
                ShowUsernamePopup();
            }
            else
            {
                // Transición directa a MainMenu
                StartCoroutine(TransitionToMainMenu());
            }
        }

        /// <summary>
        /// Muestra el popup para elegir username (primera vez)
        /// </summary>
        private void ShowUsernamePopup()
        {
            usernamePopup?.ShowForFirstTime(
                onConfirm: async (username) =>
                {
                    Debug.Log($"[Login] Usuario eligió username: {username}");

                    // Actualizar username
                    bool success = await AuthenticationService.Instance.UpdateUsername(username);

                    if (success)
                    {
                        currentPlayerData.username = username;
                        Debug.Log("[Login] Username actualizado, yendo a MainMenu");
                        StartCoroutine(TransitionToMainMenu());
                    }
                    else
                    {
                        ShowErrorMessage("Error al guardar el nombre de usuario");
                    }
                },
                onLater: () =>
                {
                    Debug.Log("[Login] Usuario eligió 'Más tarde'");
                    // Ir a MainMenu sin username
                    StartCoroutine(TransitionToMainMenu());
                }
            );
        }

        /// <summary>
        /// Callback cuando el login falla
        /// </summary>
        private void OnLoginFailed(string errorMessage)
        {
            Debug.LogError($"[Login] Login fallido: {errorMessage}");

            // Mostrar mensaje de error amigable
            string friendlyMessage = GetFriendlyErrorMessage(errorMessage);
            ShowErrorMessage(friendlyMessage);
        }

        /// <summary>
        /// Transición animada al MainMenu
        /// </summary>
        private IEnumerator TransitionToMainMenu()
        {
            // Animación de salida (opcional)
            if (titleAnimator != null)
            {
                titleAnimator.SetTrigger("Hide");
            }

            yield return new WaitForSeconds(0.5f);

            // Cargar escena
            SceneManager.LoadScene("MainMenu");
        }

        #endregion

        #region UI Helpers

        /// <summary>
        /// Muestra un mensaje de error
        /// </summary>
        private void ShowErrorMessage(string message, Color? color = null)
        {
            if (errorMessageText != null)
            {
                errorMessageText.text = message;
                errorMessageText.color = color ?? Color.red;
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
            if (loginButton != null) loginButton.interactable = interactable;
            if (registerButton != null) registerButton.interactable = interactable;
            if (googleLoginButton != null) googleLoginButton.interactable = interactable;
            if (appleLoginButton != null) appleLoginButton.interactable = interactable;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Valida si un email es válido
        /// </summary>
        private bool IsValidEmail(string email)
        {
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
        /// Convierte mensajes de error técnicos en mensajes amigables
        /// </summary>
        private string GetFriendlyErrorMessage(string technicalError)
        {
            if (technicalError.Contains("auth/user-not-found"))
                return "Usuario no encontrado";

            if (technicalError.Contains("auth/wrong-password"))
                return "Contraseña incorrecta";

            if (technicalError.Contains("auth/email-already-in-use"))
                return "Este email ya está registrado";

            if (technicalError.Contains("auth/invalid-email"))
                return "Email inválido";

            if (technicalError.Contains("auth/weak-password"))
                return "La contraseña es muy débil";

            if (technicalError.Contains("auth/network-request-failed"))
                return "Error de conexión. Verifica tu internet";

            return "Error de autenticación. Intenta nuevamente";
        }

        #endregion
    }
}
