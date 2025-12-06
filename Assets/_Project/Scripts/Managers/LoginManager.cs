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
    /// Maneja autenticación de usuarios (Email, Google, Apple)
    /// </summary>
    public class LoginManager : MonoBehaviour
    {
        [Header("UI - Login Panel")]
        [SerializeField] public GameObject loginPanel;
        [SerializeField] public TextMeshProUGUI titleText;
        [SerializeField] public TMP_InputField emailInput;
        [SerializeField] public TMP_InputField passwordInput;
        [SerializeField] public Toggle rememberToggle;
        [SerializeField] public Button loginButton;
        [SerializeField] public Button googleButton;
        [SerializeField] public Button appleButton;
        [SerializeField] public Button registerButton;

        [Header("UI - Other")]
        [SerializeField] public GameObject loadingPanel;
        [SerializeField] public Button backButton;

        [Header("UI - Error Popup")]
        [SerializeField] public GameObject errorBlocker;
        [SerializeField] public GameObject errorPopup;
        [SerializeField] public TextMeshProUGUI errorMessage;
        [SerializeField] public Button okButton;

        [Header("Animation")]
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

            // Ocultar error popup inicialmente
            if (errorBlocker != null) errorBlocker.SetActive(false);

            // Mostrar panel de login
            if (loginPanel != null)
                loginPanel.SetActive(true);

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
            if (PlayerPrefs.HasKey("RememberMe") && rememberToggle != null)
            {
                rememberToggle.isOn = PlayerPrefs.GetInt("RememberMe", 0) == 1;
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
            // Debug para verificar referencias
            Debug.Log($"[Login] loginButton: {(loginButton != null ? "OK" : "NULL")}");
            Debug.Log($"[Login] emailInput: {(emailInput != null ? "OK" : "NULL")}");
            Debug.Log($"[Login] passwordInput: {(passwordInput != null ? "OK" : "NULL")}");
            Debug.Log($"[Login] registerButton: {(registerButton != null ? "OK" : "NULL")}");

            if (loginButton != null)
            {
                loginButton.onClick.AddListener(OnLoginButtonClicked);
                Debug.Log("[Login] Listener de loginButton configurado");
            }
            else
            {
                Debug.LogError("[Login] loginButton es NULL! Asigna la referencia en el Inspector");
            }

            registerButton?.onClick.AddListener(GoToRegisterScene);
            googleButton?.onClick.AddListener(OnGoogleLoginClicked);
            appleButton?.onClick.AddListener(OnAppleLoginClicked);
            okButton?.onClick.AddListener(HideError);

            // Listener para Enter key
            passwordInput?.onEndEdit.AddListener(delegate { if (Input.GetKeyDown(KeyCode.Return)) OnLoginButtonClicked(); });
        }

        /// <summary>
        /// Navega a la escena Register
        /// </summary>
        private void GoToRegisterScene()
        {
            Debug.Log("[Login] Navegando a Register");
            SceneManager.LoadScene("Register");
        }

        #region Login

        /// <summary>
        /// Maneja el click en el botón de login
        /// </summary>
        private async void OnLoginButtonClicked()
        {
            Debug.Log("[Login] ===== BOTÓN LOGIN PRESIONADO =====");

            if (isLoggingIn)
            {
                Debug.Log("[Login] Ya está intentando login, ignorando...");
                return;
            }

            // Validar campos
            if (emailInput == null || passwordInput == null)
            {
                Debug.LogError("[Login] emailInput o passwordInput son NULL!");
                return;
            }

            string email = emailInput.text.Trim();
            string password = passwordInput.text;

            Debug.Log($"[Login] Email: {email}, Password length: {password.Length}");

            if (!ValidateLoginInputs(email, password))
            {
                return;
            }

            isLoggingIn = true;
            ShowLoading(true);

            Debug.Log($"[Login] Intentando login para: {email}");

            // Intentar login
            bool success = await AuthenticationService.Instance.LoginWithEmail(
                email,
                password,
                rememberToggle != null && rememberToggle.isOn
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

        #region Social Login

        /// <summary>
        /// Maneja el login con Google
        /// </summary>
        private async void OnGoogleLoginClicked()
        {
            if (isLoggingIn) return;

            isLoggingIn = true;
            ShowLoading(true);

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
            await System.Threading.Tasks.Task.CompletedTask; // Evitar warning CS1998
            return;
#else
            isLoggingIn = true;
            ShowLoading(true);

            Debug.Log("[Login] Intentando login con Apple");

            bool success = await AuthenticationService.Instance.LoginWithApple();

            isLoggingIn = false;
            ShowLoading(false);

            if (!success)
            {
                ShowErrorMessage("Error al iniciar sesión con Apple");
            }
#endif
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
        /// Muestra un mensaje de error usando el popup
        /// </summary>
        private void ShowErrorMessage(string message, Color? color = null)
        {
            Debug.Log($"[Login] 🚨 Mostrando error: {message}");

            if (errorMessage != null)
            {
                errorMessage.text = message;
            }

            if (errorBlocker != null)
            {
                errorBlocker.SetActive(true);
            }

            if (errorPopup != null)
            {
                errorPopup.SetActive(true);
            }
        }

        /// <summary>
        /// Oculta el popup de error
        /// </summary>
        public void HideError()
        {
            Debug.Log("[Login] Ocultando popup de error");

            if (errorBlocker != null)
            {
                errorBlocker.SetActive(false);
            }

            if (errorPopup != null)
            {
                errorPopup.SetActive(false);
            }
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
            if (googleButton != null) googleButton.interactable = interactable;
            if (appleButton != null) appleButton.interactable = interactable;
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
