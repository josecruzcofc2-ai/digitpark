using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Services.Firebase;
using DigitPark.Data;
using DigitPark.UI;
using DigitPark.UI.Common;
using DigitPark.UI.Panels;
using DigitPark.Localization;
using DigitPark.Navigation;
using DG.Tweening;

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
        [SerializeField] public Button forgotPasswordButton;

        [Header("UI - Other")]
        [SerializeField] public GameObject loadingPanel;
        [SerializeField] public Button backButton;

        [Header("UI - Panels (Prefabs)")]
        [SerializeField] private ErrorPanelUI errorPanel;

        [Header("Animation")]
        [SerializeField] public Animator titleAnimator;

        private bool isLoggingIn = false;
        private bool _isTransitioning = false;
        private UsernamePopup usernamePopup;
        private ForgotPasswordPopup forgotPasswordPopup;
        private PlayerData currentPlayerData;

        private void Start()
        {
            Debug.Log("[Login] LoginManager iniciado");

            // Verificar e inicializar servicios si no existen (para testing directo)
            EnsureServicesExist();

            // Configurar inputs (límites, placeholders, hints)
            ConfigureInputFields();

            // Auto-configurar PasswordToggle si no están asignados
            ConfigurePasswordToggles();

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

            // Suscribirse a eventos de autenticación (unsubscribe first to prevent duplicates)
            if (AuthenticationService.Instance != null)
            {
                AuthenticationService.Instance.OnLoginSuccess -= OnLoginSuccess;
                AuthenticationService.Instance.OnLoginFailed -= OnLoginFailed;
                AuthenticationService.Instance.OnLoginSuccess += OnLoginSuccess;
                AuthenticationService.Instance.OnLoginFailed += OnLoginFailed;
            }

            // Suscribirse a cambios de idioma
            LocalizationManager.OnLanguageChanged += UpdateLocalizedTexts;

            // Cargar remember me si existe
            if (PlayerPrefs.HasKey("DP_RememberMe") && rememberToggle != null)
            {
                rememberToggle.isOn = PlayerPrefs.GetInt("DP_RememberMe", 0) == 1;
            }

            // Crear popup de username (oculto inicialmente)
            CreateUsernamePopup();

            // Crear popup de forgot password (oculto inicialmente)
            CreateForgotPasswordPopup();
        }

        /// <summary>
        /// Crea el popup de username para primera vez
        /// </summary>
        private void CreateUsernamePopup()
        {
            Canvas canvas = UICanvasHelper.FindMainCanvas();
            if (canvas != null)
            {
                usernamePopup = UsernamePopup.Create(canvas.transform);
                usernamePopup.Hide();
            }
        }

        /// <summary>
        /// Crea el popup de forgot password
        /// </summary>
        private void CreateForgotPasswordPopup()
        {
            Canvas canvas = UICanvasHelper.FindMainCanvas();
            if (canvas != null)
            {
                forgotPasswordPopup = ForgotPasswordPopup.Create(canvas.transform);
                forgotPasswordPopup.Hide();
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
            StopAllCoroutines();
            DOTween.Kill(transform);

            // Remove button listeners
            loginButton?.onClick.RemoveAllListeners();
            registerButton?.onClick.RemoveAllListeners();
            googleButton?.onClick.RemoveAllListeners();
            appleButton?.onClick.RemoveAllListeners();
            forgotPasswordButton?.onClick.RemoveAllListeners();
            passwordInput?.onEndEdit.RemoveAllListeners();

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
                passwordInput.characterLimit = 128;
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
                emailPlaceholder.text = AutoLocalizer.Get("placeholder_email");
                emailPlaceholder.alignment = TextAlignmentOptions.Center;
            }

            if (passwordInput != null && passwordInput.placeholder is TextMeshProUGUI passPlaceholder)
            {
                passPlaceholder.text = AutoLocalizer.Get("placeholder_password");
                passPlaceholder.alignment = TextAlignmentOptions.Center;
            }

            // Actualizar título
            if (titleText != null)
            {
                titleText.text = AutoLocalizer.Get("login_title");
                titleText.alignment = TextAlignmentOptions.Center;
            }
            // El layout de los toggles es manejado por LocalizedTextLayoutFixer global
        }

        /// <summary>
        /// Busca y configura automáticamente los PasswordToggle en la escena
        /// </summary>
        private void ConfigurePasswordToggles()
        {
            var toggles = FindObjectsOfType<PasswordToggle>(true);
            foreach (var toggle in toggles)
            {
                Transform parent = toggle.transform.parent;
                while (parent != null)
                {
                    TMP_InputField input = parent.GetComponentInChildren<TMP_InputField>();
                    if (input != null)
                    {
                        string parentName = parent.name.ToLower();
                        if (parentName.Contains("password") && passwordInput != null)
                        {
                            SetPasswordToggleInput(toggle, passwordInput);
                        }
                        break;
                    }
                    parent = parent.parent;
                }
            }
        }

        private void SetPasswordToggleInput(PasswordToggle toggle, TMP_InputField input)
        {
            var field = typeof(PasswordToggle).GetField("passwordInput",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(toggle, input);
                Debug.Log($"[Login] PasswordToggle configurado para: {input.name}");
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
            forgotPasswordButton?.onClick.AddListener(OnForgotPasswordClicked);

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
            SceneNavigator.Instance?.NavigateTo("Register");
        }

        #region Login

        /// <summary>
        /// Maneja el click en el botón de login
        /// </summary>
        private async void OnLoginButtonClicked()
        {
            try
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

                Debug.Log("[Login] Credenciales recibidas");

                if (!ValidateLoginInputs(email, password))
                {
                    return;
                }

                isLoggingIn = true;
                ShowLoading(true);

                Debug.Log("[Login] Intentando login");

                // Intentar login con timeout de 30 segundos
                var loginTask = AuthenticationService.Instance.LoginWithEmail(
                    email,
                    password,
                    rememberToggle != null && rememberToggle.isOn
                );
                if (await Task.WhenAny(loginTask, Task.Delay(TimeSpan.FromSeconds(30))) != loginTask)
                {
                    if (this == null) return;
                    isLoggingIn = false;
                    ShowLoading(false);
                    ShowErrorMessage(AutoLocalizer.Get("auth_error_timeout"));
                    return;
                }
                bool success = await loginTask;

                // MonoBehaviour may have been destroyed during await
                if (this == null) return;

                isLoggingIn = false;
                ShowLoading(false);

                if (!success)
                {
                    // El error se maneja en OnLoginFailed
                    Debug.LogWarning("[Login] Login fallido");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                if (this != null)
                {
                    isLoggingIn = false;
                    ShowLoading(false);
                }
            }
        }

        /// <summary>
        /// Valida los inputs de login
        /// </summary>
        private bool ValidateLoginInputs(string email, string password)
        {
            if (string.IsNullOrEmpty(email))
            {
                ShowErrorMessage(AutoLocalizer.Get("error_email_empty"));
                return false;
            }

            if (!IsValidEmail(email))
            {
                ShowErrorMessage(AutoLocalizer.Get("error_email_invalid"));
                return false;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowErrorMessage(AutoLocalizer.Get("error_password_empty"));
                return false;
            }

            if (password.Length < 8)
            {
                ShowErrorMessage(AutoLocalizer.Get("error_password_too_short"));
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
            try
            {
                if (isLoggingIn) return;

                isLoggingIn = true;
                ShowLoading(true);

                Debug.Log("[Login] Intentando login con Google");

                bool success = await AuthenticationService.Instance.LoginWithGoogle();

                // MonoBehaviour may have been destroyed during await
                if (this == null) return;

                isLoggingIn = false;
                ShowLoading(false);

                if (success)
                {
                    AnalyticsService.Instance?.LogLogin("google");
                }
                else
                {
                    ShowErrorMessage(AutoLocalizer.Get("error_auth_generic"));
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                if (this != null)
                {
                    isLoggingIn = false;
                    ShowLoading(false);
                }
            }
        }

        /// <summary>
        /// Maneja el login con Apple (solo iOS)
        /// </summary>
        private async void OnAppleLoginClicked()
        {
            try
            {
                if (isLoggingIn) return;

                isLoggingIn = true;
                ShowLoading(true);

                Debug.Log("[Login] Intentando login con Apple");

                bool success = await AuthenticationService.Instance.LoginWithApple();

                // MonoBehaviour may have been destroyed during await
                if (this == null) return;

                isLoggingIn = false;
                ShowLoading(false);

                if (success)
                {
                    AnalyticsService.Instance?.LogLogin("apple");
                }
                else
                {
                    ShowErrorMessage(AutoLocalizer.Get("error_auth_generic"));
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                if (this != null)
                {
                    isLoggingIn = false;
                    ShowLoading(false);
                }
            }
        }

        #endregion

        #region Forgot Password

        /// <summary>
        /// Muestra el popup de recuperación de contraseña
        /// </summary>
        private void OnForgotPasswordClicked()
        {
            Debug.Log("[Login] Abriendo popup de recuperación de contraseña");

            // Prellenar con el email si ya tiene uno escrito
            string prefillEmail = emailInput?.text.Trim() ?? "";

            forgotPasswordPopup?.Show(
                onSend: async (email) =>
                {
                    await SendPasswordResetEmail(email);
                },
                onCancel: () =>
                {
                    Debug.Log("[Login] Usuario canceló recuperación de contraseña");
                },
                prefillEmail: prefillEmail
            );
        }

        /// <summary>
        /// Envía el email de recuperación de contraseña via Firebase
        /// </summary>
        private async System.Threading.Tasks.Task SendPasswordResetEmail(string email)
        {
            Debug.Log("[Login] Enviando email de recuperación");

            forgotPasswordPopup?.SetSendButtonInteractable(false);

            try
            {
                bool success = await AuthenticationService.Instance.ResetPassword(email);

                if (success)
                {
                    Debug.Log("[Login] Email de recuperación enviado exitosamente");
                    forgotPasswordPopup?.ShowSuccess(AutoLocalizer.Get("forgot_password_success"));

                    // Cerrar popup después de 3 segundos
                    StartCoroutine(CloseForgotPasswordPopupDelayed(3f));
                }
                else
                {
                    Debug.LogWarning("[Login] Error al enviar email de recuperación");
                    forgotPasswordPopup?.ShowError(AutoLocalizer.Get("forgot_password_error"));
                    forgotPasswordPopup?.SetSendButtonInteractable(true);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Login] Excepción al enviar email de recuperación: {e.Message}");
                forgotPasswordPopup?.ShowError(AutoLocalizer.Get("forgot_password_error"));
                forgotPasswordPopup?.SetSendButtonInteractable(true);
            }
        }

        private IEnumerator CloseForgotPasswordPopupDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            forgotPasswordPopup?.Hide();
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
            AnalyticsService.Instance?.LogLogin("email");

            // Verificar si es primera vez (no tiene username o es "Sin usuario")
            if (string.IsNullOrEmpty(playerData.username) || playerData.username.Equals("Player", StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log("[Login] Primera vez - mostrando popup de username");
                ShowUsernamePopup();
            }
            else
            {
                // Transición directa a MainMenu
                if (!_isTransitioning)
                {
                    _isTransitioning = true;
                    StartCoroutine(TransitionToMainMenu());
                }
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
                        if (!_isTransitioning)
                        {
                            _isTransitioning = true;
                            StartCoroutine(TransitionToMainMenu());
                        }
                    }
                    else
                    {
                        ShowErrorMessage(AutoLocalizer.Get("error_save_username"));
                    }
                },
                onLater: () =>
                {
                    Debug.Log("[Login] Usuario eligió 'Más tarde'");
                    // Ir a MainMenu sin username
                    if (!_isTransitioning)
                    {
                        _isTransitioning = true;
                        StartCoroutine(TransitionToMainMenu());
                    }
                }
            );
        }

        /// <summary>
        /// Callback cuando el login falla
        /// </summary>
        private void OnLoginFailed(string errorMessage)
        {
            Debug.LogError($"[Login] Login fallido: {errorMessage}");

            isLoggingIn = false;
            ShowLoading(false);

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
            SceneNavigator.Instance?.NavigateTo("MainMenu");
        }

        #endregion

        #region UI Helpers

        /// <summary>
        /// Muestra un mensaje de error usando el panel
        /// </summary>
        private void ShowErrorMessage(string message)
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
                if (show)
                {
                    loadingPanel.SetActive(true);
                    var cg = loadingPanel.GetComponent<CanvasGroup>();
                    if (cg == null) cg = loadingPanel.AddComponent<CanvasGroup>();
                    cg.alpha = 0f;
                    cg.DOFade(1f, 0.2f).SetUpdate(true);
                }
                else
                {
                    var cg = loadingPanel.GetComponent<CanvasGroup>();
                    if (cg != null)
                        cg.DOFade(0f, 0.2f).SetUpdate(true).SetLink(loadingPanel).OnComplete(() => { if (loadingPanel != null) loadingPanel.SetActive(false); });
                    else
                        loadingPanel.SetActive(false);
                }
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
                return AutoLocalizer.Get("error_user_not_found");

            if (technicalError.Contains("auth/wrong-password"))
                return AutoLocalizer.Get("error_wrong_password");

            if (technicalError.Contains("auth/email-already-in-use"))
                return AutoLocalizer.Get("error_email_already_registered");

            if (technicalError.Contains("auth/invalid-email"))
                return AutoLocalizer.Get("error_email_invalid");

            if (technicalError.Contains("auth/weak-password"))
                return AutoLocalizer.Get("error_password_weak");

            if (technicalError.Contains("auth/network-request-failed"))
                return AutoLocalizer.Get("error_no_connection");

            if (technicalError.Contains("timeout"))
                return AutoLocalizer.Get("error_timeout");

            return AutoLocalizer.Get("error_auth_generic");
        }

        #endregion
    }
}
