using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using DigitPark.Data;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;

namespace DigitPark.Services.Firebase
{
    public enum AuthProvider
    {
        Email,
        Google
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
                        Debug.Log($"[Auth] Usuario ya logueado: {currentUser.Email}");
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

                if (signedIn)
                {
                    Debug.Log($"[Auth] Estado cambiado - Usuario: {currentUser.Email}");
                }
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
            if (!useFirebaseReal)
            {
                return await LoginWithEmailSimulation(email, password, rememberMe);
            }

            try
            {
                Debug.Log($"[Auth] Login con email: {email}");

                var authResult = await firebaseAuth.SignInWithEmailAndPasswordAsync(email, password);
                currentUser = authResult.User;

                Debug.Log($"[Auth] Login exitoso: {currentUser.Email}");

                // Cargar o crear datos del jugador
                await LoadOrCreatePlayerData(currentUser);

                // Guardar preferencia de recordar
                if (rememberMe)
                {
                    PlayerPrefs.SetInt("RememberMe", 1);
                    PlayerPrefs.Save();
                }

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
                Debug.LogError($"[Auth] Error: {ex.Message}");
                OnLoginFailed?.Invoke("Error de conexión. Intenta de nuevo.");
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
                Debug.Log($"[Auth] Registro: {email}");

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
                    createdDate = DateTime.Now,
                    lastLoginDate = DateTime.Now,
                    coins = 1000,
                    gems = 50
                };

                // Guardar en base de datos
                await SavePlayerDataToDatabase(currentPlayerData);

                PlayerPrefs.SetInt("RememberMe", 1);
                PlayerPrefs.Save();

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
                Debug.LogError($"[Auth] Error: {ex.Message}");
                OnLoginFailed?.Invoke("Error de conexión. Intenta de nuevo.");
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

                Debug.Log($"[Auth] Login Google exitoso: {currentUser.Email}");

                // Cargar o crear datos del jugador
                await LoadOrCreatePlayerData(currentUser);

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
                    OnLoginFailed?.Invoke("Inicio de sesión cancelado");
                }
                else
                {
                    OnLoginFailed?.Invoke($"Error con Google: {errorMessage}");
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Error Google: {ex.Message}");

                if (ex.Message.Contains("cancelled") || ex.Message.Contains("canceled"))
                {
                    OnLoginFailed?.Invoke("Inicio de sesión cancelado");
                }
                else
                {
                    OnLoginFailed?.Invoke("Error al iniciar sesión con Google");
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

                Debug.Log($"[Auth] Eliminando cuenta de Firebase: {email}");

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
                    OnLoginFailed?.Invoke("Por seguridad, cierra sesión y vuelve a iniciar antes de eliminar la cuenta");
                }
                else
                {
                    OnLoginFailed?.Invoke($"Error al eliminar cuenta: {errorMessage}");
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Error: {ex.Message}");
                OnLoginFailed?.Invoke("Error al eliminar la cuenta");
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

                Debug.Log($"[Auth] (Simulación) Eliminando cuenta: {email}");

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
                Debug.LogError($"[Auth] Error: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Reset Password

        public async Task<bool> ResetPassword(string email)
        {
            if (!useFirebaseReal)
            {
                Debug.Log($"[Auth] (Simulación) Email de reseteo enviado a: {email}");
                await Task.Delay(500);
                return true;
            }

            try
            {
                await firebaseAuth.SendPasswordResetEmailAsync(email);
                Debug.Log($"[Auth] Email de reseteo enviado a: {email}");
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
                Debug.LogError($"[Auth] Error: {ex.Message}");
                return false;
            }
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
                        currentPlayerData.lastLoginDate = DateTime.Now;
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
                    username = user.DisplayName ?? "Sin nombre",
                    createdDate = DateTime.Now,
                    lastLoginDate = DateTime.Now,
                    coins = 1000,
                    gems = 50
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
                    username = user.DisplayName ?? "Sin nombre",
                    createdDate = DateTime.Now,
                    lastLoginDate = DateTime.Now,
                    coins = 1000,
                    gems = 50
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
                AuthError.InvalidEmail => "Email inválido",
                AuthError.WrongPassword => "Contraseña incorrecta",
                AuthError.UserNotFound => "Usuario no encontrado",
                AuthError.EmailAlreadyInUse => "Este email ya está registrado",
                AuthError.WeakPassword => "La contraseña es muy débil (mínimo 6 caracteres)",
                AuthError.NetworkRequestFailed => "Error de conexión. Verifica tu internet",
                AuthError.TooManyRequests => "Demasiados intentos. Espera un momento",
                AuthError.UserDisabled => "Esta cuenta ha sido deshabilitada",
                _ => $"Error: {ex.Message}"
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
                    OnLoginFailed?.Invoke("Usuario no encontrado. Regístrate primero.");
                    return false;
                }

                string userId = PlayerPrefs.GetString(userKey);
                string savedPassword = PlayerPrefs.GetString($"SimPassword_{userId}", "");

                if (password != savedPassword)
                {
                    OnLoginFailed?.Invoke("Contraseña incorrecta");
                    return false;
                }

                string jsonData = PlayerPrefs.GetString($"SimUser_{userId}");
                currentPlayerData = JsonUtility.FromJson<PlayerData>(jsonData);
                currentPlayerData.lastLoginDate = DateTime.Now;

                if (rememberMe)
                {
                    PlayerPrefs.SetString("SavedUserId", currentPlayerData.userId);
                    PlayerPrefs.SetInt("RememberMe", 1);
                }
                PlayerPrefs.Save();

                OnLoginSuccess?.Invoke(currentPlayerData);
                return true;
            }
            catch (Exception ex)
            {
                OnLoginFailed?.Invoke(ex.Message);
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
                    OnLoginFailed?.Invoke("Este email ya está registrado");
                    return false;
                }

                string newUserId = Guid.NewGuid().ToString();

                currentPlayerData = new PlayerData
                {
                    userId = newUserId,
                    email = email,
                    username = username,
                    createdDate = DateTime.Now,
                    lastLoginDate = DateTime.Now,
                    coins = 1000,
                    gems = 50
                };

                PlayerPrefs.SetString($"SimUser_{newUserId}", JsonUtility.ToJson(currentPlayerData));
                PlayerPrefs.SetString(emailKey, newUserId);
                PlayerPrefs.SetString($"SimPassword_{newUserId}", password);
                PlayerPrefs.SetString("SavedUserId", newUserId);
                PlayerPrefs.SetInt("RememberMe", 1);
                PlayerPrefs.Save();

                OnLoginSuccess?.Invoke(currentPlayerData);
                return true;
            }
            catch (Exception ex)
            {
                OnLoginFailed?.Invoke(ex.Message);
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
                    username = "Sin nombre",
                    createdDate = DateTime.Now,
                    lastLoginDate = DateTime.Now,
                    coins = 1000,
                    gems = 50
                };

                PlayerPrefs.SetString($"SimUser_{googleUserId}", JsonUtility.ToJson(currentPlayerData));
                PlayerPrefs.SetString("SavedUserId", googleUserId);
                PlayerPrefs.SetInt("RememberMe", 1);
                PlayerPrefs.Save();

                OnLoginSuccess?.Invoke(currentPlayerData);
                return true;
            }
            catch (Exception ex)
            {
                OnLoginFailed?.Invoke(ex.Message);
                return false;
            }
        }

        #endregion
    }
}
