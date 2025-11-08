using System;
using System.Threading.Tasks;
using UnityEngine;
using DigitPark.Data;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;

namespace DigitPark.Services.Firebase
{
    /// <summary>
    /// Servicio de autenticación con Firebase
    /// Maneja login, registro, OAuth (Google, Apple) y gestión de sesión
    /// </summary>
    public class AuthenticationService : MonoBehaviour
    {
        public static AuthenticationService Instance { get; private set; }

        // Referencias de Firebase
        private FirebaseAuth auth;
        private FirebaseUser currentUser;

        // Eventos
        public event Action<PlayerData> OnLoginSuccess;
        public event Action<string> OnLoginFailed;
        public event Action OnLogout;

        private PlayerData currentPlayerData;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeFirebase();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Inicializa Firebase Authentication
        /// </summary>
        private void InitializeFirebase()
        {
            Debug.Log("[Auth] Inicializando Firebase Authentication...");

            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                if (task.Result == DependencyStatus.Available)
                {
                    auth = FirebaseAuth.DefaultInstance;
                    Debug.Log("[Auth] Firebase Authentication inicializado correctamente");

                    // Verificar si hay usuario guardado
                    CheckForSavedUser();
                }
                else
                {
                    Debug.LogError($"[Auth] Error al inicializar Firebase: {task.Result}");
                }
            });
        }

        /// <summary>
        /// Verifica si hay un usuario guardado (Remember Me)
        /// </summary>
        private void CheckForSavedUser()
        {
            if (PlayerPrefs.HasKey("SavedUserId") && PlayerPrefs.GetInt("RememberMe", 0) == 1)
            {
                string savedUserId = PlayerPrefs.GetString("SavedUserId");
                Debug.Log($"[Auth] Usuario guardado encontrado: {savedUserId}");
                // Cargar datos del usuario desde Firebase
                LoadUserData(savedUserId);
            }
        }

        /// <summary>
        /// Login con email y contraseña
        /// </summary>
        public async Task<bool> LoginWithEmail(string email, string password, bool rememberMe)
        {
            try
            {
                Debug.Log($"[Auth] Intentando login con email: {email}");

               
                var authResult = await auth.SignInWithEmailAndPasswordAsync(email, password);
                currentUser = authResult.User;

                Debug.Log($"[Auth] Login exitoso: {currentUser.UserId}");

                // Guardar preferencia de recordar
                if (rememberMe)
                {
                    PlayerPrefs.SetString("SavedUserId", currentUser.UserId);
                    PlayerPrefs.SetInt("RememberMe", 1);
                }

                // Cargar datos del usuario
                await LoadUserData(currentUser.UserId);

                OnLoginSuccess?.Invoke(currentPlayerData);
                return true;
                

                // Simulación para desarrollo - verificar si el usuario ya existe
                await Task.Delay(1000);

                string savedDataKey = $"PlayerData_{email}";

                if (PlayerPrefs.HasKey(savedDataKey))
                {
                    // Cargar datos existentes del usuario
                    string jsonData = PlayerPrefs.GetString(savedDataKey);
                    currentPlayerData = JsonUtility.FromJson<PlayerData>(jsonData);
                    Debug.Log($"[Auth] Usuario existente cargado: {currentPlayerData.username}");
                }
                else
                {
                    // Usuario no encontrado (debería fallar en producción)
                    Debug.LogWarning("[Auth] Usuario no encontrado en simulación");
                    currentPlayerData = new PlayerData
                    {
                        userId = Guid.NewGuid().ToString(),
                        email = email,
                        username = "" // SIN username por defecto
                    };

                    // Guardar nuevo usuario para simulación
                    string jsonData = JsonUtility.ToJson(currentPlayerData);
                    PlayerPrefs.SetString(savedDataKey, jsonData);
                }

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
                Debug.LogError($"[Auth] Error en login: {ex.Message}");
                OnLoginFailed?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Registro con email y contraseña
        /// </summary>
        public async Task<bool> RegisterWithEmail(string email, string password, string username)
        {
            try
            {
                Debug.Log($"[Auth] Intentando registro con email: {email}");

                
                var authResult = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
                currentUser = authResult.User;

                // Actualizar perfil con nombre de usuario
                UserProfile profile = new UserProfile { DisplayName = username };
                await currentUser.UpdateUserProfileAsync(profile);

                Debug.Log($"[Auth] Registro exitoso: {currentUser.UserId}");

                // Crear datos del nuevo jugador
                currentPlayerData = new PlayerData
                {
                    userId = currentUser.UserId,
                    email = email,
                    username = username,
                    createdDate = DateTime.Now
                };

                // Guardar en base de datos
                await DatabaseService.Instance.SavePlayerData(currentPlayerData);

                OnLoginSuccess?.Invoke(currentPlayerData);
                return true;
                

                // Simulación para desarrollo
                await Task.Delay(1000);
                currentPlayerData = new PlayerData
                {
                    userId = Guid.NewGuid().ToString(),
                    email = email,
                    username = string.IsNullOrEmpty(username) ? "" : username, // Usar el username proporcionado
                    createdDate = DateTime.Now
                };

                // Guardar datos del usuario en PlayerPrefs para persistencia
                string savedDataKey = $"PlayerData_{email}";
                string jsonData = JsonUtility.ToJson(currentPlayerData);
                PlayerPrefs.SetString(savedDataKey, jsonData);
                PlayerPrefs.Save();

                Debug.Log($"[Auth] Usuario registrado y guardado: {currentPlayerData.username}");

                OnLoginSuccess?.Invoke(currentPlayerData);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Error en registro: {ex.Message}");
                OnLoginFailed?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Login con Google
        /// </summary>
        public async Task<bool> LoginWithGoogle()
        {
            try
            {
                Debug.Log("[Auth] Iniciando login con Google...");

                // NOTA: Requiere Google Sign-In plugin para Unity
                // Para activar: Importar "Google Sign-In Unity Plugin"
                // Descomentar el código abajo cuando esté instalado

                /*
                GoogleSignIn.Configuration = new GoogleSignInConfiguration
                {
                    RequestIdToken = true,
                    WebClientId = "TU_WEB_CLIENT_ID.apps.googleusercontent.com"
                };

                var googleUser = await GoogleSignIn.DefaultInstance.SignIn();
                var credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);
                var authResult = await auth.SignInWithCredentialAsync(credential);

                currentUser = authResult.User;
                Debug.Log($"[Auth] Login con Google exitoso: {currentUser.UserId}");

                await LoadUserData(currentUser.UserId);

                OnLoginSuccess?.Invoke(currentPlayerData);
                return true;
                */

                // Simulación (hasta instalar plugin de Google Sign-In)
                await Task.Delay(1500);
                Debug.LogWarning("[Auth] Login con Google no disponible - Requiere Google Sign-In plugin");
                OnLoginFailed?.Invoke("Google Sign-In no configurado");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Error en login con Google: {ex.Message}");
                OnLoginFailed?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Login con Apple (iOS)
        /// </summary>
        public async Task<bool> LoginWithApple()
        {
            try
            {
                Debug.Log("[Auth] Iniciando login con Apple...");

                #if UNITY_IOS
                // NOTA: Requiere Apple Sign-In configurado
                // Para activar: Descomentar cuando tengas Apple Developer configurado

                /*
                var credential = OAuthProvider.GetCredential("apple.com", appleIdToken, null, null);
                var authResult = await auth.SignInWithCredentialAsync(credential);

                currentUser = authResult.User;
                Debug.Log($"[Auth] Login con Apple exitoso: {currentUser.UserId}");

                await LoadUserData(currentUser.UserId);

                OnLoginSuccess?.Invoke(currentPlayerData);
                return true;
                */
                #endif

                // Simulación
                await Task.Delay(1500);
                Debug.Log("[Auth] Login con Apple simulado (solo disponible en iOS)");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Error en login con Apple: {ex.Message}");
                OnLoginFailed?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Logout del usuario actual
        /// </summary>
        public void Logout()
        {
            Debug.Log("[Auth] Cerrando sesión...");

            auth.SignOut();
            currentUser = null;

            currentPlayerData = null;

            // Limpiar datos guardados
            PlayerPrefs.DeleteKey("SavedUserId");
            PlayerPrefs.DeleteKey("RememberMe");
            PlayerPrefs.Save();

            OnLogout?.Invoke();

            Debug.Log("[Auth] Sesión cerrada");
        }

        /// <summary>
        /// Carga los datos del usuario desde Firebase Database
        /// </summary>
        private async Task LoadUserData(string userId)
        {
            try
            {
                currentPlayerData = await DatabaseService.Instance.LoadPlayerData(userId);

                if (currentPlayerData == null)
                {
                    // Si no existe, crear nuevo perfil
                    currentPlayerData = new PlayerData
                    {
                        userId = userId,
                        createdDate = DateTime.Now
                    };

                    await DatabaseService.Instance.SavePlayerData(currentPlayerData);
                }

                // Actualizar última conexión
                currentPlayerData.lastLoginDate = DateTime.Now;
                await DatabaseService.Instance.SavePlayerData(currentPlayerData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Error cargando datos del usuario: {ex.Message}");
            }
        }

        /// <summary>
        /// Resetear contraseña
        /// </summary>
        public async Task<bool> ResetPassword(string email)
        {
            try
            {
                Debug.Log($"[Auth] Enviando email de reseteo a: {email}");

                await auth.SendPasswordResetEmailAsync(email);

                await Task.Delay(500);
                Debug.Log("[Auth] Email de reseteo enviado");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Error al resetear contraseña: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verifica si hay un usuario autenticado
        /// </summary>
        public bool IsUserAuthenticated()
        {
            // return currentUser != null;
            return currentPlayerData != null;
        }

        /// <summary>
        /// Obtiene los datos del jugador actual
        /// </summary>
        public PlayerData GetCurrentPlayerData()
        {
            return currentPlayerData;
        }

        /// <summary>
        /// Actualiza el nombre de usuario
        /// </summary>
        public async Task<bool> UpdateUsername(string newUsername)
        {
            try
            {
                if (currentPlayerData == null) return false;

                currentPlayerData.username = newUsername;

                
                UserProfile profile = new UserProfile { DisplayName = newUsername };
                await currentUser.UpdateUserProfileAsync(profile);
                

                await DatabaseService.Instance.SavePlayerData(currentPlayerData);

                // Actualizar PlayerPrefs para persistencia en simulación
                string savedDataKey = $"PlayerData_{currentPlayerData.email}";
                string jsonData = JsonUtility.ToJson(currentPlayerData);
                PlayerPrefs.SetString(savedDataKey, jsonData);
                PlayerPrefs.Save();

                Debug.Log($"[Auth] Nombre de usuario actualizado a: {newUsername}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Error al actualizar username: {ex.Message}");
                return false;
            }
        }
    }
}
