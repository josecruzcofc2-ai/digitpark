using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DigitPark.Data;
using DigitPark.Localization;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;

namespace DigitPark.Services.Firebase
{
    public enum AuthProvider
    {
        Email,
        Google,
        Apple
    }

    public class AuthenticationService : MonoBehaviour
    {
        public static AuthenticationService Instance { get; private set; }

        [Header("Configuración")]
        [Tooltip("Activar para usar Firebase real, desactivar para modo simulación")]
        [SerializeField] private bool useFirebaseReal = true;

        // Eventos
        public event Action<PlayerData> OnLoginSuccess;
        public event Action<string> OnLoginFailed;
        public event Action OnLogout;

        // Firebase
        private FirebaseAuth firebaseAuth;
        private FirebaseUser currentUser;
        private bool isFirebaseInitialized = false;

        // Datos del jugador
        private PlayerData currentPlayerData;

        // Rate limiting
        private int _loginAttempts = 0;
        private float _loginCooldownUntil = 0f;

        // Propiedades públicas
        public bool IsFirebaseReal => useFirebaseReal;
        public bool IsInitialized => isFirebaseInitialized || !useFirebaseReal;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                StartCoroutine(InitializeAsync());
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private System.Collections.IEnumerator InitializeAsync()
        {
            if (useFirebaseReal)
            {
                Debug.Log("[Auth] Inicializando Firebase Auth...");

                var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();
                yield return new WaitUntil(() => dependencyTask.IsCompleted);

                if (dependencyTask.Result == DependencyStatus.Available)
                {
                    firebaseAuth = FirebaseAuth.DefaultInstance;
                    firebaseAuth.StateChanged += OnAuthStateChanged;

                    // Verificar si hay usuario logueado
                    if (firebaseAuth.CurrentUser != null)
                    {
                        currentUser = firebaseAuth.CurrentUser;
                        Debug.Log($"[Auth] Usuario ya logueado: {RedactEmail(currentUser.Email)}");
                        yield return LoadOrCreatePlayerData(currentUser);
                    }

                    isFirebaseInitialized = true;
                    Debug.Log("[Auth] Firebase Auth inicializado correctamente");
                }
                else
                {
                    Debug.LogError($"[Auth] Error al inicializar Firebase: {dependencyTask.Result}");
                    // Fallback a simulación
                    useFirebaseReal = false;
                    InitializeSimulation();
                }
            }
            else
            {
                InitializeSimulation();
            }
        }

        private void InitializeSimulation()
        {
            Debug.Log("[Auth] Modo Simulación activado");
            CheckForSavedUserSimulation();
        }

        /// <summary>
        /// Hashes password with SHA256 so it's never stored in plain text in PlayerPrefs.
        /// Simulation-only; production uses Firebase Auth which handles hashing server-side.
        /// </summary>
        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var sb = new StringBuilder(64);
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        private void OnAuthStateChanged(object sender, EventArgs e)
        {
            if (firebaseAuth.CurrentUser != currentUser)
            {
                bool signedIn = firebaseAuth.CurrentUser != null;

                if (!signedIn && currentUser != null)
                {
                    Debug.Log("[Auth] Usuario deslogueado");
                    currentUser = null;
                    currentPlayerData = null;
                    OnLogout?.Invoke();
                }

                currentUser = firebaseAuth.CurrentUser;

                // AUDIT-FIXED [2026-03-10] H-02: Si Firebase restaura sesión automáticamente
                // y currentPlayerData es null, cargar datos del jugador desde la DB
                if (signedIn)
                {
                    Debug.Log($"[Auth] Estado cambiado - Usuario: {RedactEmail(currentUser.Email)}");
                    if (currentPlayerData == null)
                    {
                        _ = LoadOrCreatePlayerData(currentUser).ContinueWith(t =>
                        {
                            if (this == null) return;
                            if (t.IsFaulted)
                                Debug.LogError($"[Auth] Error cargando datos en StateChanged: {t.Exception?.GetBaseException().Message}");
                            else
                                OnLoginSuccess?.Invoke(currentPlayerData);
                        }, System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());
                    }
                }
            }
        }

        // AUDIT-FIXED [2026-03-10] H-01: Refrescar token al volver al foreground
        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && useFirebaseReal && currentUser != null)
            {
                _ = currentUser.ReloadAsync().ContinueWithOnMainThread(t =>
                {
                    if (this == null) return;
                    if (t.IsFaulted)
                        Debug.LogWarning($"[Auth] Token refresh falló: {t.Exception?.GetBaseException().Message}");
                    else
                        Debug.Log("[Auth] Token refrescado al volver al foreground");
                });
            }
        }

        private void OnDestroy()
        {
            if (firebaseAuth != null)
            {
                firebaseAuth.StateChanged -= OnAuthStateChanged;
            }
        }

        #region Login con Email

        public async Task<bool> LoginWithEmail(string email, string password, bool rememberMe)
        {
            // Rate limiting: block after 5 failed attempts for 30 seconds
            if (_loginAttempts >= 5)
            {
                if (Time.realtimeSinceStartup < _loginCooldownUntil)
                {
                    Debug.LogWarning("[Auth] Login rate limited");
                    OnLoginFailed?.Invoke(AutoLocalizer.Get("auth_error_too_many_requests"));
                    return false;
                }
                // Cooldown expired — reset attempts
                _loginAttempts = 0;
            }

            _loginAttempts++;
            if (_loginAttempts >= 5)
            {
                _loginCooldownUntil = Time.realtimeSinceStartup + 30f;
            }

            if (!useFirebaseReal)
            {
                return await LoginWithEmailSimulation(email, password, rememberMe);
            }

            try
            {
                Debug.Log($"[Auth] Login con email: {RedactEmail(email)}");

                var authResult = await firebaseAuth.SignInWithEmailAndPasswordAsync(email, password);
                currentUser = authResult.User;

                Debug.Log($"[Auth] Login exitoso: {RedactEmail(currentUser.Email)}");

                // Cargar o crear datos del jugador
                await LoadOrCreatePlayerData(currentUser);

                // Guardar preferencia de recordar
                if (rememberMe)
                {
                    PlayerPrefs.SetInt("RememberMe", 1);
                    PlayerPrefs.Save();
                }

                _loginAttempts = 0; // Reset on success
                AnalyticsService.Instance?.LogLogin("email");
                OnLoginSuccess?.Invoke(currentPlayerData);
                return true;
            }
            catch (FirebaseException ex)
            {
                string errorMessage = GetFirebaseErrorMessage(ex);
                Debug.LogError($"[Auth] Error login: {errorMessage}");
                OnLoginFailed?.Invoke(errorMessage);
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Unexpected error ({ex.GetType().Name}): {ex.Message}");
                OnLoginFailed?.Invoke(AutoLocalizer.Get("auth_error_connection"));
                return false;
            }
        }

        #endregion

        #region Registro con Email

        public async Task<bool> RegisterWithEmail(string email, string password, string username)
        {
            if (!useFirebaseReal)
            {
                return await RegisterWithEmailSimulation(email, password, username);
            }

            try
            {
                Debug.Log($"[Auth] Registro: {RedactEmail(email)}");

                // Check username uniqueness
                if (!string.IsNullOrEmpty(username) && DatabaseService.Instance != null)
                {
                    bool taken = await DatabaseService.Instance.IsUsernameTaken(username);
                    if (taken)
                    {
                        Debug.LogWarning("[Auth] Username already taken");
                        // AUDIT-FIXED [2026-03-10] M-06: usar AutoLocalizer en lugar de string hardcodeada
                        OnLoginFailed?.Invoke(AutoLocalizer.Get("auth_error_username_taken"));
                        return false;
                    }
                }

                var authResult = await firebaseAuth.CreateUserWithEmailAndPasswordAsync(email, password);
                currentUser = authResult.User;

                // Actualizar perfil con username
                var profile = new UserProfile { DisplayName = username };
                await currentUser.UpdateUserProfileAsync(profile);

                Debug.Log($"[Auth] Registro exitoso: {username}");

                // Crear datos del jugador
                currentPlayerData = new PlayerData
                {
                    userId = currentUser.UserId,
                    email = email,
                    username = username,
                    createdDate = DateTime.UtcNow,
                    lastLoginDate = DateTime.UtcNow
                };

                // Guardar en base de datos
                await SavePlayerDataToDatabase(currentPlayerData);

                PlayerPrefs.SetInt("RememberMe", 1);
                PlayerPrefs.Save();

                AnalyticsService.Instance?.LogSignUp("email");
                OnLoginSuccess?.Invoke(currentPlayerData);
                return true;
            }
            catch (FirebaseException ex)
            {
                string errorMessage = GetFirebaseErrorMessage(ex);
                Debug.LogError($"[Auth] Error registro: {errorMessage}");
                OnLoginFailed?.Invoke(errorMessage);
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Unexpected error ({ex.GetType().Name}): {ex.Message}");
                OnLoginFailed?.Invoke(AutoLocalizer.Get("auth_error_connection"));
                return false;
            }
        }

        #endregion

        #region Login con Google

        public async Task<bool> LoginWithGoogle()
        {
            if (!useFirebaseReal)
            {
                return await LoginWithGoogleSimulation();
            }

            try
            {
                Debug.Log("[Auth] Iniciando login con Google...");

                // Crear el provider de Google
                var provider = new FederatedOAuthProviderData();
                provider.ProviderId = "google.com";

                // Scopes opcionales (email y profile vienen por defecto)
                provider.Scopes = new List<string>
                {
                    "email",
                    "profile"
                };

                var federatedProvider = new FederatedOAuthProvider();
                federatedProvider.SetProviderData(provider);

                // Iniciar Sign-In con el provider (abre WebView)
                var authResult = await firebaseAuth.SignInWithProviderAsync(federatedProvider);
                currentUser = authResult.User;

                Debug.Log($"[Auth] Login Google exitoso: {RedactEmail(currentUser.Email)}");

                // Cargar o crear datos del jugador
                await LoadOrCreatePlayerData(currentUser);

                AnalyticsService.Instance?.LogLogin("google");
                OnLoginSuccess?.Invoke(currentPlayerData);
                return true;
            }
            catch (FirebaseException ex)
            {
                string errorMessage = GetFirebaseErrorMessage(ex);
                Debug.LogError($"[Auth] Error Google: {errorMessage}");

                // El usuario canceló el login
                if (ex.Message.Contains("cancelled") || ex.Message.Contains("canceled"))
                {
                    OnLoginFailed?.Invoke(AutoLocalizer.Get("auth_login_cancelled"));
                }
                else
                {
                    OnLoginFailed?.Invoke(AutoLocalizer.Get("auth_error_google", errorMessage));
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Error Google: {ex.Message}");

                if (ex.Message.Contains("cancelled") || ex.Message.Contains("canceled"))
                {
                    OnLoginFailed?.Invoke(AutoLocalizer.Get("auth_login_cancelled"));
                }
                else
                {
                    OnLoginFailed?.Invoke(AutoLocalizer.Get("auth_error_google_generic"));
                }
                return false;
            }
        }

        #endregion

        #region Login con Apple

        /// <summary>
        /// Inicia sesión con Apple (iOS obligatorio si hay login social)
        /// Requiere: Sign in with Apple Unity Plugin
        /// </summary>
        public async Task<bool> LoginWithApple()
        {
            if (!useFirebaseReal)
            {
                return await LoginWithAppleSimulation();
            }

            try
            {
                Debug.Log("[Auth] Iniciando login con Apple...");

                // Crear el provider de Apple
                var provider = new FederatedOAuthProviderData();
                provider.ProviderId = "apple.com";

                // Scopes para Apple
                provider.Scopes = new List<string>
                {
                    "email",
                    "name"
                };

                var federatedProvider = new FederatedOAuthProvider();
                federatedProvider.SetProviderData(provider);

                // Iniciar Sign-In con Apple
                var authResult = await firebaseAuth.SignInWithProviderAsync(federatedProvider);
                currentUser = authResult.User;

                Debug.Log($"[Auth] Login Apple exitoso: {RedactEmail(currentUser.Email ?? currentUser.UserId)}");

                // Cargar o crear datos del jugador
                await LoadOrCreatePlayerData(currentUser);

                AnalyticsService.Instance?.LogLogin("apple");
                OnLoginSuccess?.Invoke(currentPlayerData);
                return true;
            }
            catch (FirebaseException ex)
            {
                string errorMessage = GetFirebaseErrorMessage(ex);
                Debug.LogError($"[Auth] Error Apple: {errorMessage}");

                if (ex.Message.Contains("cancelled") || ex.Message.Contains("canceled"))
                {
                    OnLoginFailed?.Invoke(AutoLocalizer.Get("auth_login_cancelled"));
                }
                else
                {
                    OnLoginFailed?.Invoke(AutoLocalizer.Get("auth_error_apple", errorMessage));
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Error Apple: {ex.Message}");

                if (ex.Message.Contains("cancelled") || ex.Message.Contains("canceled"))
                {
                    OnLoginFailed?.Invoke(AutoLocalizer.Get("auth_login_cancelled"));
                }
                else
                {
                    OnLoginFailed?.Invoke(AutoLocalizer.Get("auth_error_apple_generic"));
                }
                return false;
            }
        }

        #endregion

        #region Logout

        public void Logout()
        {
            Debug.Log("[Auth] Cerrando sesión...");

            if (useFirebaseReal && firebaseAuth != null)
            {
                firebaseAuth.SignOut();
            }

            currentUser = null;
            currentPlayerData = null;

            PlayerPrefs.DeleteKey("SavedUserId");
            PlayerPrefs.DeleteKey("RememberMe");
            PlayerPrefs.Save();

            OnLogout?.Invoke();
            Debug.Log("[Auth] Sesión cerrada");
        }

        #endregion

        #region Delete Account

        public async Task<bool> DeleteAccount()
        {
            if (!useFirebaseReal)
            {
                return await DeleteAccountSimulation();
            }

            try
            {
                if (currentUser == null)
                {
                    Debug.LogError("[Auth] No hay usuario para eliminar");
                    return false;
                }

                string userId = currentUser.UserId;
                string email = currentUser.Email ?? "";

                Debug.Log($"[Auth] Eliminando cuenta de Firebase: {RedactEmail(email)}");

                // Eliminar datos de la base de datos primero
                var dbService = DatabaseService.Instance;
                if (dbService != null)
                {
                    await dbService.RemoveUserFromLeaderboards(userId);
                }

                // Eliminar el usuario de Firebase Auth
                await currentUser.DeleteAsync();

                Debug.Log("[Auth] Cuenta eliminada de Firebase exitosamente");

                // Limpiar datos locales
                currentUser = null;
                currentPlayerData = null;

                PlayerPrefs.DeleteKey("SavedUserId");
                PlayerPrefs.DeleteKey("RememberMe");
                PlayerPrefs.DeleteKey($"SimUser_{userId}");
                PlayerPrefs.DeleteKey($"SimUserByEmail_{email.ToLower()}");
                PlayerPrefs.Save();

                OnLogout?.Invoke();
                return true;
            }
            catch (FirebaseException ex)
            {
                string errorMessage = GetFirebaseErrorMessage(ex);
                Debug.LogError($"[Auth] Error eliminando cuenta: {errorMessage}");

                // Si requiere re-autenticación reciente
                if ((AuthError)ex.ErrorCode == AuthError.RequiresRecentLogin)
                {
                    OnLoginFailed?.Invoke(AutoLocalizer.Get("auth_error_requires_relogin"));
                }
                else
                {
                    OnLoginFailed?.Invoke(AutoLocalizer.Get("auth_error_delete_account", errorMessage));
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Unexpected error ({ex.GetType().Name}): {ex.Message}");
                OnLoginFailed?.Invoke(AutoLocalizer.Get("auth_error_delete_generic"));
                return false;
            }
        }

        private async Task<bool> DeleteAccountSimulation()
        {
            try
            {
                if (currentPlayerData == null)
                {
                    return false;
                }

                string userId = currentPlayerData.userId;
                string email = currentPlayerData.email?.ToLower() ?? "";

                Debug.Log($"[Auth] (Simulación) Eliminando cuenta: {RedactEmail(email)}");

                // Eliminar de leaderboards
                var dbService = DatabaseService.Instance;
                if (dbService != null)
                {
                    await dbService.RemoveUserFromLeaderboards(userId);
                }

                // Eliminar datos de PlayerPrefs
                PlayerPrefs.DeleteKey($"SimUser_{userId}");
                PlayerPrefs.DeleteKey($"SimUserByEmail_{email}");
                PlayerPrefs.DeleteKey($"SimPassword_{userId}");
                PlayerPrefs.DeleteKey("SavedUserId");
                PlayerPrefs.DeleteKey("RememberMe");
                PlayerPrefs.Save();

                currentPlayerData = null;

                OnLogout?.Invoke();
                Debug.Log("[Auth] (Simulación) Cuenta eliminada exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Unexpected error ({ex.GetType().Name}): {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Reset Password

        public async Task<bool> ResetPassword(string email)
        {
            if (!useFirebaseReal)
            {
                Debug.Log($"[Auth] (Simulación) Email de reseteo enviado a: {RedactEmail(email)}");
                await Task.Delay(500);
                return true;
            }

            try
            {
                await firebaseAuth.SendPasswordResetEmailAsync(email);
                Debug.Log($"[Auth] Email de reseteo enviado a: {RedactEmail(email)}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Error reset: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Helpers

        public bool IsUserAuthenticated()
        {
            if (useFirebaseReal)
            {
                return currentUser != null;
            }
            return currentPlayerData != null;
        }

        public PlayerData GetCurrentPlayerData()
        {
            return currentPlayerData;
        }

        public void UpdateCurrentPlayerData(PlayerData playerData)
        {
            currentPlayerData = playerData;
            Debug.Log($"[Auth] Datos actualizados: {playerData.username}");
        }

        public async Task<bool> UpdateUsername(string newUsername)
        {
            try
            {
                if (currentPlayerData == null) return false;

                string oldUsername = currentPlayerData.username;
                currentPlayerData.username = newUsername;

                if (useFirebaseReal && currentUser != null)
                {
                    var profile = new UserProfile { DisplayName = newUsername };
                    await currentUser.UpdateUserProfileAsync(profile);
                    await SavePlayerDataToDatabase(currentPlayerData);
                }
                else
                {
                    // Simulación
                    string userDataKey = $"SimUser_{currentPlayerData.userId}";
                    PlayerPrefs.SetString(userDataKey, JsonUtility.ToJson(currentPlayerData));
                    PlayerPrefs.Save();
                }

                Debug.Log($"[Auth] Username actualizado: {oldUsername} -> {newUsername}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Unexpected error ({ex.GetType().Name}): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Redacts an email for safe logging (PII protection)
        /// </summary>
        private static string RedactEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return "***";
            return email.Length > 2 ? email.Substring(0, 2) + "***" : "***";
        }

        public string GetCurrentUserId()
        {
            if (useFirebaseReal && currentUser != null)
            {
                return currentUser.UserId;
            }
            return currentPlayerData?.userId;
        }

        #endregion

        #region Firebase Helpers

        private async Task LoadOrCreatePlayerData(FirebaseUser user)
        {
            try
            {
                // Intentar cargar datos existentes de la base de datos
                var dbService = DatabaseService.Instance;
                if (dbService != null)
                {
                    var existingData = await dbService.LoadPlayerData(user.UserId);
                    if (existingData != null)
                    {
                        currentPlayerData = existingData;
                        currentPlayerData.lastLoginDate = DateTime.UtcNow;
                        await SavePlayerDataToDatabase(currentPlayerData);
                        Debug.Log($"[Auth] Datos cargados: {currentPlayerData.username}");
                        return;
                    }
                }

                // Crear nuevos datos
                currentPlayerData = new PlayerData
                {
                    userId = user.UserId,
                    email = user.Email ?? "",
                    username = user.DisplayName ?? AutoLocalizer.Get("auth_default_username"),
                    createdDate = DateTime.UtcNow,
                    lastLoginDate = DateTime.UtcNow
                };

                await SavePlayerDataToDatabase(currentPlayerData);
                Debug.Log($"[Auth] Nuevos datos creados: {currentPlayerData.username}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Error cargando datos: {ex.Message}");

                // Fallback - crear datos locales
                currentPlayerData = new PlayerData
                {
                    userId = user.UserId,
                    email = user.Email ?? "",
                    username = user.DisplayName ?? AutoLocalizer.Get("auth_default_username"),
                    createdDate = DateTime.UtcNow,
                    lastLoginDate = DateTime.UtcNow
                };
            }
        }

        private async Task SavePlayerDataToDatabase(PlayerData data)
        {
            var dbService = DatabaseService.Instance;
            if (dbService != null)
            {
                await dbService.SavePlayerData(data);
            }
        }

        private string GetFirebaseErrorMessage(FirebaseException ex)
        {
            // Mapear códigos de error de Firebase a mensajes amigables
            var errorCode = (AuthError)ex.ErrorCode;

            return errorCode switch
            {
                AuthError.InvalidEmail => AutoLocalizer.Get("auth_error_invalid_email"),
                AuthError.WrongPassword => AutoLocalizer.Get("auth_error_wrong_password"),
                AuthError.UserNotFound => AutoLocalizer.Get("auth_error_user_not_found"),
                AuthError.EmailAlreadyInUse => AutoLocalizer.Get("auth_error_email_in_use"),
                AuthError.WeakPassword => AutoLocalizer.Get("auth_error_weak_password"),
                AuthError.NetworkRequestFailed => AutoLocalizer.Get("auth_error_network"),
                AuthError.TooManyRequests => AutoLocalizer.Get("auth_error_too_many_requests"),
                AuthError.UserDisabled => AutoLocalizer.Get("auth_error_user_disabled"),
                // AUDIT-FIXED [2026-03-10] H-03: usar clave localizada en lugar de mensaje raw de Firebase
                _ => AutoLocalizer.Get("auth_error_generic")
            };
        }

        #endregion

        #region Simulación (Fallback)

        private void CheckForSavedUserSimulation()
        {
            if (PlayerPrefs.HasKey("SavedUserId") && PlayerPrefs.GetInt("RememberMe", 0) == 1)
            {
                string savedUserId = PlayerPrefs.GetString("SavedUserId");
                string userDataKey = $"SimUser_{savedUserId}";

                if (PlayerPrefs.HasKey(userDataKey))
                {
                    string jsonData = PlayerPrefs.GetString(userDataKey);
                    currentPlayerData = JsonUtility.FromJson<PlayerData>(jsonData);
                    if (currentPlayerData == null) { Debug.LogWarning("[Auth] (Sim) Auto-login: malformed PlayerData JSON, skipping."); return; }
                    Debug.Log($"[Auth] (Sim) Auto-login: {currentPlayerData.username}");
                }
            }
        }

        private async Task<bool> LoginWithEmailSimulation(string email, string password, bool rememberMe)
        {
            try
            {
                await Task.Delay(500);

                string userKey = $"SimUserByEmail_{email.ToLower()}";

                if (!PlayerPrefs.HasKey(userKey))
                {
                    OnLoginFailed?.Invoke(AutoLocalizer.Get("auth_error_user_not_found_register"));
                    return false;
                }

                string userId = PlayerPrefs.GetString(userKey);
                string savedHash = PlayerPrefs.GetString($"SimPassword_{userId}", "");

                if (HashPassword(password) != savedHash)
                {
                    OnLoginFailed?.Invoke(AutoLocalizer.Get("auth_error_wrong_password"));
                    return false;
                }

                string jsonData = PlayerPrefs.GetString($"SimUser_{userId}");
                currentPlayerData = JsonUtility.FromJson<PlayerData>(jsonData);
                if (currentPlayerData == null)
                {
                    Debug.LogError("[Auth] (Sim) Login: malformed PlayerData JSON.");
                    OnLoginFailed?.Invoke(AutoLocalizer.Get("auth_error_generic"));
                    return false;
                }
                currentPlayerData.lastLoginDate = DateTime.UtcNow;

                if (rememberMe)
                {
                    PlayerPrefs.SetString("SavedUserId", currentPlayerData.userId);
                    PlayerPrefs.SetInt("RememberMe", 1);
                }
                PlayerPrefs.Save();

                _loginAttempts = 0; // Reset on success
                AnalyticsService.Instance?.LogLogin("email");
                OnLoginSuccess?.Invoke(currentPlayerData);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Unexpected error ({ex.GetType().Name}): {ex.Message}");
                OnLoginFailed?.Invoke(AutoLocalizer.Get("auth_error_generic"));
                return false;
            }
        }

        private async Task<bool> RegisterWithEmailSimulation(string email, string password, string username)
        {
            try
            {
                await Task.Delay(800);

                string emailKey = $"SimUserByEmail_{email.ToLower()}";
                if (PlayerPrefs.HasKey(emailKey))
                {
                    OnLoginFailed?.Invoke(AutoLocalizer.Get("auth_error_email_in_use"));
                    return false;
                }

                string newUserId = Guid.NewGuid().ToString();

                currentPlayerData = new PlayerData
                {
                    userId = newUserId,
                    email = email,
                    username = username,
                    createdDate = DateTime.UtcNow,
                    lastLoginDate = DateTime.UtcNow
                };

                PlayerPrefs.SetString($"SimUser_{newUserId}", JsonUtility.ToJson(currentPlayerData));
                PlayerPrefs.SetString(emailKey, newUserId);
                PlayerPrefs.SetString($"SimPassword_{newUserId}", HashPassword(password));
                PlayerPrefs.SetString("SavedUserId", newUserId);
                PlayerPrefs.SetInt("RememberMe", 1);
                PlayerPrefs.Save();

                AnalyticsService.Instance?.LogSignUp("email");
                OnLoginSuccess?.Invoke(currentPlayerData);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Unexpected error ({ex.GetType().Name}): {ex.Message}");
                OnLoginFailed?.Invoke(AutoLocalizer.Get("auth_error_generic"));
                return false;
            }
        }

        private async Task<bool> LoginWithGoogleSimulation()
        {
            try
            {
                await Task.Delay(1000);

                string googleUserId = "google_" + Guid.NewGuid().ToString().Substring(0, 8);
                string googleEmail = $"usuario.google.{UnityEngine.Random.Range(1000, 9999)}@gmail.com";

                currentPlayerData = new PlayerData
                {
                    userId = googleUserId,
                    email = googleEmail,
                    username = AutoLocalizer.Get("auth_default_username"),
                    createdDate = DateTime.UtcNow,
                    lastLoginDate = DateTime.UtcNow
                };

                PlayerPrefs.SetString($"SimUser_{googleUserId}", JsonUtility.ToJson(currentPlayerData));
                PlayerPrefs.SetString("SavedUserId", googleUserId);
                PlayerPrefs.SetInt("RememberMe", 1);
                PlayerPrefs.Save();

                AnalyticsService.Instance?.LogLogin("google");
                OnLoginSuccess?.Invoke(currentPlayerData);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Unexpected error ({ex.GetType().Name}): {ex.Message}");
                OnLoginFailed?.Invoke(AutoLocalizer.Get("auth_error_generic"));
                return false;
            }
        }

        private async Task<bool> LoginWithAppleSimulation()
        {
            try
            {
                await Task.Delay(1000);

                string appleUserId = "apple_" + Guid.NewGuid().ToString().Substring(0, 8);
                // Apple puede ocultar el email real del usuario
                string appleEmail = $"privaterelay.{UnityEngine.Random.Range(1000, 9999)}@privaterelay.appleid.com";

                currentPlayerData = new PlayerData
                {
                    userId = appleUserId,
                    email = appleEmail,
                    username = Localization.AutoLocalizer.Get("auth_default_apple_username"),
                    createdDate = DateTime.UtcNow,
                    lastLoginDate = DateTime.UtcNow
                };

                PlayerPrefs.SetString($"SimUser_{appleUserId}", JsonUtility.ToJson(currentPlayerData));
                PlayerPrefs.SetString("SavedUserId", appleUserId);
                PlayerPrefs.SetInt("RememberMe", 1);
                PlayerPrefs.Save();

                Debug.Log($"[Auth] (Simulación) Login Apple exitoso: {RedactEmail(appleEmail)}");
                AnalyticsService.Instance?.LogLogin("apple");
                OnLoginSuccess?.Invoke(currentPlayerData);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Unexpected error ({ex.GetType().Name}): {ex.Message}");
                OnLoginFailed?.Invoke(AutoLocalizer.Get("auth_error_generic"));
                return false;
            }
        }

        #endregion
    }
}
