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
        private bool isFirebaseInitialized = false;

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
        private async void InitializeFirebase()
        {
            Debug.Log("[Auth] 🔄 Inicializando Firebase Authentication...");

            try
            {
                var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

                if (dependencyStatus == DependencyStatus.Available)
                {
                    // Importante: Esperar un frame para asegurar que Unity está listo
                    await Task.Delay(100);

                    auth = FirebaseAuth.DefaultInstance;

                    if (auth != null)
                    {
                        isFirebaseInitialized = true;
                        Debug.Log("[Auth] ✅ Firebase Authentication inicializado correctamente");
                        Debug.Log($"[Auth] ✅ Firebase App Name: {FirebaseApp.DefaultInstance.Name}");
                        Debug.Log($"[Auth] ✅ Auth Instance: {auth.App.Name}");

                        // Verificar si hay usuario guardado (esperar a que termine)
                        await CheckForSavedUser();
                    }
                    else
                    {
                        Debug.LogError("[Auth] ❌ FirebaseAuth.DefaultInstance es NULL");
                        isFirebaseInitialized = false;
                    }
                }
                else
                {
                    Debug.LogError($"[Auth] ❌ Error al inicializar Firebase: {dependencyStatus}");
                    Debug.LogError("[Auth] Verifica que google-services.json (Android) o GoogleService-Info.plist (iOS) estén configurados");
                    isFirebaseInitialized = false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] ❌ Excepción al inicializar Firebase: {ex.Message}");
                Debug.LogError($"[Auth] StackTrace: {ex.StackTrace}");
                isFirebaseInitialized = false;
            }
        }

        /// <summary>
        /// Verifica si hay un usuario guardado (Remember Me)
        /// </summary>
        private async Task CheckForSavedUser()
        {
            // Verificar si el usuario marcó "Recordarme"
            if (PlayerPrefs.HasKey("SavedUserId") && PlayerPrefs.GetInt("RememberMe", 0) == 1)
            {
                string savedUserId = PlayerPrefs.GetString("SavedUserId");
                Debug.Log($"[Auth] 🔐 Preferencia 'Recordar' encontrada para userId: {savedUserId}");

                // Verificar si Firebase Auth mantiene la sesión activa
                if (auth.CurrentUser != null)
                {
                    Debug.Log($"[Auth] ✅ Sesión de Firebase activa!");
                    Debug.Log($"[Auth] ✅ Firebase CurrentUser: {auth.CurrentUser.UserId}");

                    // Verificar que el userId coincida
                    if (auth.CurrentUser.UserId == savedUserId)
                    {
                        currentUser = auth.CurrentUser;
                        Debug.Log($"[Auth] ✅ Auto-login exitoso para: {currentUser.Email}");

                        // Cargar datos del usuario desde Firebase
                        await LoadUserData(savedUserId);

                        // Verificar que se hayan cargado los datos
                        if (currentPlayerData != null)
                        {
                            Debug.Log($"[Auth] ✅ Auto-login completado. Usuario: {currentPlayerData.username}");
                            Debug.Log($"[Auth] ✅ BootManager redirigirá al MainMenu automáticamente");
                        }
                        else
                        {
                            Debug.LogWarning("[Auth] ⚠️ No se pudieron cargar los datos del usuario");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[Auth] ⚠️ El userId guardado ({savedUserId}) no coincide con CurrentUser ({auth.CurrentUser.UserId})");
                        // Borrar PlayerPrefs desactualizados
                        PlayerPrefs.DeleteKey("SavedUserId");
                        PlayerPrefs.DeleteKey("RememberMe");
                        PlayerPrefs.Save();
                    }
                }
                else
                {
                    Debug.LogWarning("[Auth] ⚠️ No hay sesión activa en Firebase. El usuario debe hacer login de nuevo.");
                    // Borrar PlayerPrefs ya que no hay sesión válida
                    PlayerPrefs.DeleteKey("SavedUserId");
                    PlayerPrefs.DeleteKey("RememberMe");
                    PlayerPrefs.Save();
                }
            }
            else
            {
                Debug.Log("[Auth] ℹ️ No hay preferencia 'Recordar' guardada");
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

                // Verificar si Firebase está inicializado
                if (!isFirebaseInitialized || auth == null)
                {
                    Debug.LogWarning("[Auth] ⏳ Firebase Auth no está inicializado! Esperando...");

                    // Esperar hasta 15 segundos para que Firebase se inicialice
                    int attempts = 0;
                    while ((!isFirebaseInitialized || auth == null) && attempts < 30)
                    {
                        await Task.Delay(500);
                        attempts++;

                        if (attempts % 5 == 0) // Log cada 2.5 segundos
                        {
                            Debug.Log($"[Auth] ⏳ Esperando inicialización... {attempts * 0.5f}s (isInit: {isFirebaseInitialized}, auth: {auth != null})");
                        }
                    }

                    if (!isFirebaseInitialized || auth == null)
                    {
                        Debug.LogError("[Auth] ❌ Firebase no se inicializó después de 15 segundos");
                        Debug.LogError($"[Auth] ❌ Estado final: isFirebaseInitialized={isFirebaseInitialized}, auth={auth != null}");
                        Debug.LogError("[Auth] ❌ SOLUCIÓN: Verifica que AuthenticationService esté en la escena Boot y que google-services.json esté configurado");
                        OnLoginFailed?.Invoke("Error: Firebase no está inicializado. Reinicia el juego");
                        return false;
                    }

                    Debug.Log("[Auth] ✅ Firebase inicializado después de esperar");
                }

                Debug.Log("[Auth] ✅ Firebase Auth disponible, intentando SignIn...");

                // Usar ContinueWithOnMainThread para mejor manejo de errores
                var signInTask = auth.SignInWithEmailAndPasswordAsync(email, password);
                bool loginSuccess = false;

                await signInTask.ContinueWithOnMainThread(task =>
                {
                    if (task.IsCanceled)
                    {
                        Debug.LogError("[Auth] ❌ Login cancelado");
                        OnLoginFailed?.Invoke("Login cancelado");
                        loginSuccess = false;
                        return;
                    }

                    if (task.IsFaulted)
                    {
                        Debug.LogError("[Auth] ❌ Login fallido - procesando errores...");

                        foreach (var ex in task.Exception.Flatten().InnerExceptions)
                        {
                            if (ex is FirebaseException firebaseEx)
                            {
                                var authError = (AuthError)firebaseEx.ErrorCode;
                                Debug.LogError($"[Auth] 🔥 Firebase AuthError: {authError}");
                                Debug.LogError($"[Auth] 🔥 Firebase Message: {firebaseEx.Message}");
                                Debug.LogError($"[Auth] 🔥 Firebase ErrorCode: {firebaseEx.ErrorCode}");

                                string errorMessage = GetFirebaseErrorMessage(firebaseEx);
                                OnLoginFailed?.Invoke(errorMessage);
                            }
                            else
                            {
                                Debug.LogError($"[Auth] ❌ Exception: {ex.GetType().Name}");
                                Debug.LogError($"[Auth] ❌ Message: {ex.Message}");
                                OnLoginFailed?.Invoke($"Error: {ex.Message}");
                            }
                        }
                        loginSuccess = false;
                        return;
                    }

                    // Login exitoso
                    currentUser = task.Result.User;
                    Debug.Log($"[Auth] ✅ Login exitoso!");
                    Debug.Log($"[Auth] ✅ User ID: {currentUser.UserId}");
                    Debug.Log($"[Auth] ✅ Email: {currentUser.Email}");
                    loginSuccess = true;
                });

                // Si el task falló, retornar false inmediatamente
                if (!loginSuccess)
                {
                    Debug.LogError("[Auth] ❌ Retornando false - login falló");
                    return false;
                }

                // Guardar o borrar preferencia de recordar
                if (rememberMe)
                {
                    PlayerPrefs.SetString("SavedUserId", currentUser.UserId);
                    PlayerPrefs.SetInt("RememberMe", 1);
                    Debug.Log("[Auth] Preferencia 'Recordar' guardada");
                }
                else
                {
                    PlayerPrefs.DeleteKey("SavedUserId");
                    PlayerPrefs.DeleteKey("RememberMe");
                    Debug.Log("[Auth] Preferencia 'Recordar' borrada");
                }
                PlayerPrefs.Save();

                // Cargar datos del usuario
                Debug.Log("[Auth] Cargando datos del usuario desde Firebase...");
                await LoadUserData(currentUser.UserId);

                Debug.Log("[Auth] Invocando OnLoginSuccess...");
                OnLoginSuccess?.Invoke(currentPlayerData);
                return true;

                // MODO SIMULACIÓN (Firebase no configurado)
                Debug.LogWarning("[Auth] Firebase no configurado, usando modo simulación");
                await Task.Delay(500);

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
                    // Usuario no encontrado en simulación - debe registrarse primero
                    Debug.LogWarning("[Auth] Usuario no encontrado. Créalo primero con Register.");
                    OnLoginFailed?.Invoke("Usuario no encontrado. Debes registrarte primero.");
                    return false;
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
        /// Actualiza los datos del jugador actual (usado al recargar desde Firebase)
        /// </summary>
        public void UpdateCurrentPlayerData(PlayerData playerData)
        {
            currentPlayerData = playerData;
            Debug.Log($"[Auth] Datos del jugador actualizados: {playerData.username}");
        }

        /// <summary>
        /// Actualiza el nombre de usuario
        /// </summary>
        public async Task<bool> UpdateUsername(string newUsername)
        {
            try
            {
                if (currentPlayerData == null) return false;

                string oldUsername = currentPlayerData.username;
                currentPlayerData.username = newUsername;


                UserProfile profile = new UserProfile { DisplayName = newUsername };
                await currentUser.UpdateUserProfileAsync(profile);


                // Guardar datos del jugador
                await DatabaseService.Instance.SavePlayerData(currentPlayerData);

                // Actualizar username en leaderboards (global y país)
                Debug.Log($"[Auth] Actualizando username en leaderboards: {oldUsername} → {newUsername}");
                await DatabaseService.Instance.UpdateUsernameInLeaderboards(
                    currentPlayerData.userId,
                    newUsername,
                    currentPlayerData.countryCode
                );

                // Actualizar PlayerPrefs para persistencia en simulación
                string savedDataKey = $"PlayerData_{currentPlayerData.email}";
                string jsonData = JsonUtility.ToJson(currentPlayerData);
                PlayerPrefs.SetString(savedDataKey, jsonData);
                PlayerPrefs.Save();

                Debug.Log($"[Auth] ✅ Nombre de usuario actualizado completamente: {oldUsername} → {newUsername}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Error al actualizar username: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Convierte errores de Firebase en mensajes amigables
        /// </summary>
        private string GetFirebaseErrorMessage(FirebaseException ex)
        {
            switch (ex.ErrorCode)
            {
                case (int)AuthError.InvalidEmail:
                    return "Email inválido";
                case (int)AuthError.WrongPassword:
                    return "Contraseña incorrecta";
                case (int)AuthError.UserNotFound:
                    return "Usuario no encontrado";
                case (int)AuthError.EmailAlreadyInUse:
                    return "Este email ya está registrado";
                case (int)AuthError.WeakPassword:
                    return "La contraseña es muy débil";
                case (int)AuthError.NetworkRequestFailed:
                    return "Error de conexión. Verifica tu internet";
                case (int)AuthError.TooManyRequests:
                    return "Demasiados intentos. Espera un momento";
                default:
                    return $"Error de autenticación: {ex.Message}";
            }
        }
    }
}
