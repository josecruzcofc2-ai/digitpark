using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DigitPark.Services.Firebase;
using DigitPark.Data;
using DigitPark.UI.Common;
using DigitPark.UI.Panels;
using DigitPark.Localization;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager de la escena Login
    /// Maneja autenticación de usuarios (Email)
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

        [Header("UI - Panels (Prefabs)")]
        [SerializeField] private ErrorPanelUI errorPanel;

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

            // Configurar inputs (límites, placeholders, hints)
            ConfigureInputFields();

            // Configurar listeners
            SetupListeners();

            // El ErrorPanelUI se oculta automáticamente en su Awake()

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

            // Suscribirse a cambios de idioma
            LocalizationManager.OnLanguageChanged += UpdateLocalizedTexts;

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
            LocalizationManager.OnLanguageChanged -= UpdateLocalizedTexts;
        }

        /// <summary>
        /// Configura los input fields con límites y placeholders descriptivos
        /// </summary>
        private void ConfigureInputFields()
        {
            // Configurar límites de caracteres
            if (emailInput != null)
            {
                emailInput.characterLimit = 100;
            }

            if (passwordInput != null)
            {
                passwordInput.characterLimit = 50;
            }

            // Actualizar textos localizados
            UpdateLocalizedTexts();
        }

        /// <summary>
        /// Actualiza todos los textos localizados (placeholders y títulos centrados)
        /// </summary>
        private void UpdateLocalizedTexts()
        {
            // Actualizar placeholders con información de límites
            if (emailInput != null && emailInput.placeholder is TextMeshProUGUI emailPlaceholder)
            {
                emailPlaceholder.text = GetLocalizedText("placeholder_email");
                emailPlaceholder.alignment = TextAlignmentOptions.Center;
            }

            if (passwordInput != null && passwordInput.placeholder is TextMeshProUGUI passPlaceholder)
            {
                passPlaceholder.text = GetLocalizedText("placeholder_password");
                passPlaceholder.alignment = TextAlignmentOptions.Center;
            }

            // Actualizar título
            if (titleText != null)
            {
                titleText.text = GetLocalizedText("login_title");
                titleText.alignment = TextAlignmentOptions.Center;
            }
            // El layout de los toggles es manejado por LocalizedTextLayoutFixer global
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

            // Ambos botones (Google y Apple) siempre visibles

            // El ErrorPanelUI maneja su propio botón internamente

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
                ShowErrorMessage(GetLocalizedText("error_email_empty"));
                return false;
            }

            if (!IsValidEmail(email))
            {
                ShowErrorMessage(GetLocalizedText("error_email_invalid"));
                return false;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowErrorMessage(GetLocalizedText("error_password_empty"));
                return false;
            }

            if (password.Length < 6)
            {
                ShowErrorMessage(GetLocalizedText("error_password_too_short"));
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
                ShowErrorMessage(GetLocalizedText("error_auth_generic"));
            }
        }

        /// <summary>
        /// Maneja el login con Apple (solo iOS)
        /// </summary>
        private async void OnAppleLoginClicked()
        {
            if (isLoggingIn) return;

            isLoggingIn = true;
            ShowLoading(true);

            Debug.Log("[Login] Intentando login con Apple");

            bool success = await AuthenticationService.Instance.LoginWithApple();

            isLoggingIn = false;
            ShowLoading(false);

            if (!success)
            {
                ShowErrorMessage(GetLocalizedText("error_auth_generic"));
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
                        ShowErrorMessage(GetLocalizedText("error_save_username"));
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
        /// Muestra un mensaje de error usando el panel
        /// </summary>
        private void ShowErrorMessage(string message, Color? color = null)
        {
            Debug.Log($"[Login] Mostrando error: {message}");
            errorPanel?.Show(message);
        }

        /// <summary>
        /// Oculta el panel de error
        /// </summary>
        public void HideError()
        {
            Debug.Log("[Login] Ocultando panel de error");
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
                return GetLocalizedText("error_user_not_found");

            if (technicalError.Contains("auth/wrong-password"))
                return GetLocalizedText("error_wrong_password");

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

            return GetLocalizedText("error_auth_generic");
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
