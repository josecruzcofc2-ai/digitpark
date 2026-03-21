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
#if FIREBASE_CRASHLYTICS
using Firebase.Crashlytics;
#endif

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
#if UNITY_EDITOR
        [Tooltip("Activar para usar Firebase real, desactivar para modo simulación (solo Editor)")]
        [SerializeField] private bool useFirebaseReal = true;
#else
        // PRODUCTION: Firebase siempre activo, no configurable desde Inspector
        private readonly bool useFirebaseReal = true;
#endif

        // Eventos
        public event Action<PlayerData> OnLoginSuccess;
        public event Action<string> OnLoginFailed;
        public event Action OnLogout;
        public event Action<string> OnInitFailed;

        // Firebase
        private FirebaseAuth firebaseAuth;
        private FirebaseUser currentUser;
        private bool isFirebaseInitialized = false;

        // Datos del jugador
        private PlayerData currentPlayerData;

        // Rate limiting
        private int _loginAttempts = 0;
        private float _loginCooldownUntil = 0f;

        // Guard against race condition between manual login and OnAuthStateChanged
        private bool _isManualLoginInProgress = false;

        // Propiedades públicas
        public bool IsFirebaseReal => useFirebaseReal;
#if UNITY_EDITOR
        public bool IsInitialized => isFirebaseInitialized || !useFirebaseReal;
#else
        // En producción, solo Firebase real cuenta como inicializado
        public bool IsInitialized => isFirebaseInitialized;
#endif

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
#if UNITY_EDITOR
                    // Fallback a simulación solo en Editor
                    useFirebaseReal = false;
                    InitializeSimulation();
#else
                    // En producción, NO caer a simulación — reportar error
                    Debug.LogError("[Auth] PRODUCCIÓN: Firebase no disponible. La autenticación no funcionará.");
                    OnInitFailed?.Invoke($"Firebase dependency error: {dependencyTask.Result}");
#endif
                }
            }
#if UNITY_EDITOR
            else
            {
                InitializeSimulation();
            }
#endif
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
            // Skip if a manual login/register is in progress to avoid race condition
            if (_isManualLoginInProgress) return;

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
                            else if (currentPlayerData != null)
                                OnLoginSuccess?.Invoke(currentPlayerData);
                            else
                                Debug.LogWarning("[Auth] StateChanged: LoadOrCreatePlayerData completed but currentPlayerData is null");
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
                    {
                        Debug.LogWarning($"[Auth] Token refresh falló: {t.Exception?.GetBaseException().Message}");
                        // Token revoked or user deleted server-side — force logout
                        var baseEx = t.Exception?.GetBaseException();
                        if (baseEx is FirebaseException fbEx &&
                            ((AuthError)fbEx.ErrorCode == AuthError.UserNotFound ||
                             (AuthError)fbEx.ErrorCode == AuthError.UserTokenExpired ||
                             (AuthError)fbEx.ErrorCode == AuthError.InvalidUserToken))
                        {
                            Debug.LogWarning("[Auth] Token revoked — forcing logout");
                            Logout();
                        }
                    }
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
            OnLoginSuccess = null;
            OnLoginFailed = null;
            OnLogout = null;
            OnInitFailed = null;
        }

        #region Login con Email

        public async Task<bool> LoginWithEmail(string email, string password, bool rememberMe)
        {
            // Rate limiting: block after 5 failed attempts for 30 seconds
            // Check and reset cooldown BEFORE incrementing to avoid off-by-one
            if (_loginAttempts >= 5 && Time.realtimeSinceStartup >= _loginCooldownUntil)
            {
                // Cooldown expired — reset attempts before incrementing
                _loginAttempts = 0;
            }

            if (_loginAttempts >= 5)
            {
                Debug.LogWarning("[Auth] Login rate limited");
                OnLoginFailed?.Invoke(AutoLocalizer.Get("auth_error_too_many_requests"));
                return false;
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

            if (firebaseAuth == null)
            {
                Debug.LogError("[Auth] Firebase Auth not initialized");
                OnLoginFailed?.Invoke(AutoLocalizer.Get("auth_error_not_initialized"));
                return false;
            }

            _isManualLoginInProgress = true;
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
                    PlayerPrefs.SetInt("DP_RememberMe", 1);
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
            finally
            {
                _isManualLoginInProgress = false;
            }
        }

        #endregion

        #region Registro con Email

        public async Task<bool> RegisterWithEmail(string email, string password, string username)
        {
            // C-P1-35: Minimum 8 chars — accounts may hold real money (CashBattle)
            if (password.Length < 8)
            {
                OnLoginFailed?.Invoke(AutoLocalizer.Get("error_password_too_short"));
                return false;
            }

            if (!useFirebaseReal)
            {
                return await RegisterWithEmailSimulation(email, password, username);
            }

            if (firebaseAuth == null)
            {
                Debug.LogError("[Auth] Firebase Auth not initialized");
                OnLoginFailed?.Invoke(AutoLocalizer.Get("auth_error_not_initialized"));
                return false;
            }

            _isManualLoginInProgress = true;
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
                try
                {
                    await SavePlayerDataToDatabase(currentPlayerData);
                }
                catch (Exception saveEx)
                {
                    // Cleanup orphaned Firebase Auth user if DB save fails
                    Debug.LogError($"[Auth] DB save failed after registration, cleaning up auth user: {saveEx.Message}");
                    try { await currentUser.DeleteAsync(); } catch { }
                    currentUser = null;
                    currentPlayerData = null;
                    OnLoginFailed?.Invoke(AutoLocalizer.Get("auth_error_connection"));
                    return false;
                }

                PlayerPrefs.SetInt("DP_RememberMe", 1);
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
            finally
            {
                _isManualLoginInProgress = false;
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

            if (firebaseAuth == null)
            {
                Debug.LogError("[Auth] Firebase Auth not initialized");
                OnLoginFailed?.Invoke(AutoLocalizer.Get("auth_error_not_initialized"));
                return false;
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

            if (firebaseAuth == null)
            {
                Debug.LogError("[Auth] Firebase Auth not initialized");
                OnLoginFailed?.Invoke(AutoLocalizer.Get("auth_error_not_initialized"));
                return false;
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
                try { firebaseAuth.SignOut(); }
                catch (Exception ex) { Debug.LogWarning($"[Auth] SignOut error (forcing local cleanup): {ex.Message}"); }
            }

            currentUser = null;
            currentPlayerData = null;

            PlayerPrefs.DeleteKey("DP_SavedUserId");
            PlayerPrefs.DeleteKey("DP_RememberMe");
            PlayerPrefs.Save();

            OnLogout?.Invoke();
            Debug.Log("[Auth] Sesión cerrada");
        }

        #endregion

        #region Delete Account

        public async Task<bool> DeleteAccount(string confirmPassword = "") // B2-A: acepta password para re-auth
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

                // B2-A: Re-autenticar antes de borrar si se provee password
                if (!string.IsNullOrEmpty(confirmPassword) && !string.IsNullOrEmpty(email))
                {
                    try
                    {
                        var credential = global::Firebase.Auth.EmailAuthProvider.GetCredential(email, confirmPassword);
                        await currentUser.ReauthenticateAsync(credential);
                        Debug.Log("[Auth] Re-autenticación exitosa");
                    }
                    catch (Exception reauthEx)
                    {
                        Debug.LogWarning($"[Auth] Re-autenticación fallida: {reauthEx.Message}");
                        return false;
                    }
                }

                // Step 1: Delete Firebase Auth FIRST (requires recent login)
                // If this fails with RequiresRecentLogin, no data is lost
                await currentUser.DeleteAsync();
                Debug.Log("[Auth] Firebase Auth account deleted");

                // Step 2: Delete DB data (best-effort — Cloud Function also handles GDPR delete)
                var dbService = DatabaseService.Instance;
                if (dbService != null)
                {
                    try
                    {
                        await dbService.RemoveUserFromLeaderboards(userId);
                        await dbService.DeleteMatchHistory(userId);
                        await dbService.DeleteAchievements(userId);
                        await dbService.DeleteFriendsList(userId);
                        await dbService.DeleteNotifications(userId);
                        await dbService.DeleteTournamentHistory(userId);
                    }
                    catch (Exception dbEx)
                    {
                        // Auth already deleted — log but don't fail. Server-side deleteUserData handles full cleanup.
                        Debug.LogWarning($"[Auth] DB cleanup partial (server will handle): {dbEx.Message}");
                    }
                }

                // Step 3: Clean local data (only DP_ prefixed keys, preserve system prefs)
                currentUser = null;
                currentPlayerData = null;
                DeleteDigitParkPrefs();

                OnLogout?.Invoke();
                return true;
            }
            catch (FirebaseException ex)
            {
                string errorMessage = GetFirebaseErrorMessage(ex);
                Debug.LogError($"[Auth] Error eliminando cuenta: {errorMessage}");

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
                // S15-NEW-03: read email from Firebase Auth, not from RTDB PlayerData
                string email = currentUser?.Email?.ToLower() ?? currentPlayerData.email?.ToLower() ?? "";

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
                PlayerPrefs.DeleteKey("DP_SavedUserId");
                PlayerPrefs.DeleteKey("DP_RememberMe");
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

            if (firebaseAuth == null)
            {
                Debug.LogError("[Auth] Firebase Auth not initialized");
                return false;
            }

            try
            {
                await firebaseAuth.SendPasswordResetEmailAsync(email);
                Debug.Log($"[Auth] Email de reseteo enviado a: {RedactEmail(email)}");
            }
            catch (Exception ex)
            {
                // B2-C: suprimir el error — no revelar si el email existe (account enumeration)
                Debug.Log($"[Auth] ResetPassword suppressed: {ex.GetType().Name}");
            }
            return true; // siempre true — la UI siempre muestra el mensaje genérico
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Deletes only DigitPark-related PlayerPrefs keys instead of DeleteAll()
        /// which would wipe system prefs (audio, accessibility, etc.)
        /// </summary>
        private static void DeleteDigitParkPrefs()
        {
            string[] prefixes = { "DP_", "dp_", "SimUser_", "SimPassword_", "Cash", "Equipped", "Owned" };
            // PlayerPrefs doesn't expose key enumeration, so delete known keys
            string[] knownKeys = {
                "DP_SavedUserId", "DP_RememberMe", "DP_TotalLoginDays", "DP_RankedWins_Total",
                "DP_PerfectScores_Total", "DP_ConsentGiven", "dp_cc_v2", "dp_cg_v2",
                "EquippedBackground", "OwnedBackgrounds", "EquippedFrame", "OwnedFrames",
                "EquippedTitle", "OwnedTitles", "EquippedEffect", "OwnedEffects",
                "EquippedEmote", "OwnedEmotes", "EquippedBattleCard", "OwnedBattleCards",
            };
            foreach (var key in knownKeys)
                PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }

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
            // S2-NEW-03: Validate format at service layer — UI validation is not sufficient
            if (string.IsNullOrWhiteSpace(newUsername) ||
                !System.Text.RegularExpressions.Regex.IsMatch(newUsername, @"^[a-zA-Z0-9_]+$"))
            {
                Debug.LogWarning("[Auth] UpdateUsername rechazado: formato inválido");
                return false;
            }

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
#if FIREBASE_CRASHLYTICS
                        Crashlytics.SetUserId(currentPlayerData.userId);
#endif
                        return;
                    }
                }

                // Crear nuevos datos
                // D-37: Fallback if AutoLocalizer not yet initialized (returns key as-is)
                string defaultUsername = AutoLocalizer.Get("auth_default_username");
                if (defaultUsername == "auth_default_username") defaultUsername = "Player";

                currentPlayerData = new PlayerData
                {
                    userId = user.UserId,
                    email = user.Email ?? "",
                    username = user.DisplayName ?? defaultUsername,
                    createdDate = DateTime.UtcNow,
                    lastLoginDate = DateTime.UtcNow
                };

                await SavePlayerDataToDatabase(currentPlayerData);
                Debug.Log($"[Auth] Nuevos datos creados: {currentPlayerData.username}");
#if FIREBASE_CRASHLYTICS
                Crashlytics.SetUserId(currentPlayerData.userId);
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Error cargando datos: {ex.Message}");

                // Fallback - crear datos locales
                // D-37: Fallback if AutoLocalizer not yet initialized (returns key as-is)
                string fallbackUsername = AutoLocalizer.Get("auth_default_username");
                if (fallbackUsername == "auth_default_username") fallbackUsername = "Player";

                currentPlayerData = new PlayerData
                {
                    userId = user.UserId,
                    email = user.Email ?? "",
                    username = user.DisplayName ?? fallbackUsername,
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
                // HIGH-02: Same message for both — prevents account enumeration
                AuthError.WrongPassword => AutoLocalizer.Get("auth_error_invalid_credentials"),
                AuthError.UserNotFound => AutoLocalizer.Get("auth_error_invalid_credentials"),
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
            if (PlayerPrefs.HasKey("DP_SavedUserId") && PlayerPrefs.GetInt("DP_RememberMe", 0) == 1)
            {
                string savedUserId = PlayerPrefs.GetString("DP_SavedUserId");
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
                    PlayerPrefs.SetString("DP_SavedUserId", currentPlayerData.userId);
                    PlayerPrefs.SetInt("DP_RememberMe", 1);
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
                PlayerPrefs.SetString("DP_SavedUserId", newUserId);
                PlayerPrefs.SetInt("DP_RememberMe", 1);
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
                PlayerPrefs.SetString("DP_SavedUserId", googleUserId);
                PlayerPrefs.SetInt("DP_RememberMe", 1);
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
                PlayerPrefs.SetString("DP_SavedUserId", appleUserId);
                PlayerPrefs.SetInt("DP_RememberMe", 1);
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
