using System;
using System.Threading.Tasks;
using UnityEngine;
using DigitPark.Data;

namespace DigitPark.Services.Firebase
{
    /// <summary>
    /// Servicio de autenticación (Modo Simulación)
    /// Usa PlayerPrefs para almacenar datos localmente
    /// </summary>
    public class AuthenticationService : MonoBehaviour
    {
        public static AuthenticationService Instance { get; private set; }

        // Eventos
        public event Action<PlayerData> OnLoginSuccess;
        public event Action<string> OnLoginFailed;
        public event Action OnLogout;

        private PlayerData currentPlayerData;
        private bool isInitialized = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private async void Initialize()
        {
            Debug.Log("[Auth] Inicializando servicio de autenticación (Simulación)...");
            isInitialized = true;

            await Task.Delay(100);
            await CheckForSavedUser();

            Debug.Log("[Auth] Servicio listo");
        }

        private async Task CheckForSavedUser()
        {
            if (PlayerPrefs.HasKey("SavedUserId") && PlayerPrefs.GetInt("RememberMe", 0) == 1)
            {
                string savedUserId = PlayerPrefs.GetString("SavedUserId");
                Debug.Log($"[Auth] Usuario guardado encontrado: {savedUserId}");

                string userDataKey = $"SimUser_{savedUserId}";
                if (PlayerPrefs.HasKey(userDataKey))
                {
                    string jsonData = PlayerPrefs.GetString(userDataKey);
                    currentPlayerData = JsonUtility.FromJson<PlayerData>(jsonData);
                    Debug.Log($"[Auth] Auto-login exitoso: {currentPlayerData.username}");
                }
            }
            else
            {
                Debug.Log("[Auth] No hay sesión guardada");
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Login con email y contraseña
        /// </summary>
        public async Task<bool> LoginWithEmail(string email, string password, bool rememberMe)
        {
            try
            {
                Debug.Log($"[Auth] Login con email: {email}");
                await Task.Delay(500);

                string userKey = $"SimUserByEmail_{email.ToLower()}";

                if (!PlayerPrefs.HasKey(userKey))
                {
                    Debug.LogWarning("[Auth] Usuario no encontrado");
                    OnLoginFailed?.Invoke("Usuario no encontrado. Regístrate primero.");
                    return false;
                }

                string userId = PlayerPrefs.GetString(userKey);
                string userDataKey = $"SimUser_{userId}";
                string savedPassword = PlayerPrefs.GetString($"SimPassword_{userId}", "");

                if (password != savedPassword)
                {
                    Debug.LogWarning("[Auth] Contraseña incorrecta");
                    OnLoginFailed?.Invoke("Contraseña incorrecta");
                    return false;
                }

                string jsonData = PlayerPrefs.GetString(userDataKey);
                currentPlayerData = JsonUtility.FromJson<PlayerData>(jsonData);

                currentPlayerData.lastLoginDate = DateTime.Now;
                PlayerPrefs.SetString(userDataKey, JsonUtility.ToJson(currentPlayerData));

                if (rememberMe)
                {
                    PlayerPrefs.SetString("SavedUserId", currentPlayerData.userId);
                    PlayerPrefs.SetInt("RememberMe", 1);
                }
                PlayerPrefs.Save();

                Debug.Log($"[Auth] Login exitoso: {currentPlayerData.username}");
                OnLoginSuccess?.Invoke(currentPlayerData);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Error: {ex.Message}");
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
                Debug.Log($"[Auth] Registro: {email}");
                await Task.Delay(800);

                string emailKey = $"SimUserByEmail_{email.ToLower()}";
                if (PlayerPrefs.HasKey(emailKey))
                {
                    Debug.LogWarning("[Auth] Email ya registrado");
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

                string userDataKey = $"SimUser_{newUserId}";
                PlayerPrefs.SetString(userDataKey, JsonUtility.ToJson(currentPlayerData));
                PlayerPrefs.SetString(emailKey, newUserId);
                PlayerPrefs.SetString($"SimPassword_{newUserId}", password);

                PlayerPrefs.SetString("SavedUserId", newUserId);
                PlayerPrefs.SetInt("RememberMe", 1);
                PlayerPrefs.Save();

                Debug.Log($"[Auth] Registro exitoso: {username}");
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
        /// Login con Google (Simulado)
        /// </summary>
        public async Task<bool> LoginWithGoogle()
        {
            try
            {
                Debug.Log("[Auth] Login con Google (simulado)...");
                await Task.Delay(1000);

                string googleUserId = "google_" + Guid.NewGuid().ToString().Substring(0, 8);
                string googleEmail = $"usuario.google.{UnityEngine.Random.Range(1000, 9999)}@gmail.com";

                currentPlayerData = new PlayerData
                {
                    userId = googleUserId,
                    email = googleEmail,
                    username = "Usuario Google",
                    createdDate = DateTime.Now,
                    lastLoginDate = DateTime.Now,
                    coins = 1000,
                    gems = 50
                };

                string userDataKey = $"SimUser_{googleUserId}";
                PlayerPrefs.SetString(userDataKey, JsonUtility.ToJson(currentPlayerData));
                PlayerPrefs.SetString("SavedUserId", googleUserId);
                PlayerPrefs.SetInt("RememberMe", 1);
                PlayerPrefs.Save();

                Debug.Log($"[Auth] Login Google exitoso: {googleEmail}");
                OnLoginSuccess?.Invoke(currentPlayerData);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Error: {ex.Message}");
                OnLoginFailed?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Login con Apple (Simulado)
        /// </summary>
        public async Task<bool> LoginWithApple()
        {
            try
            {
                Debug.Log("[Auth] Login con Apple (simulado)...");
                await Task.Delay(1000);

                string appleUserId = "apple_" + Guid.NewGuid().ToString().Substring(0, 8);

                currentPlayerData = new PlayerData
                {
                    userId = appleUserId,
                    email = $"usuario.{UnityEngine.Random.Range(1000, 9999)}@privaterelay.appleid.com",
                    username = "Usuario Apple",
                    createdDate = DateTime.Now,
                    lastLoginDate = DateTime.Now,
                    coins = 1000,
                    gems = 50
                };

                string userDataKey = $"SimUser_{appleUserId}";
                PlayerPrefs.SetString(userDataKey, JsonUtility.ToJson(currentPlayerData));
                PlayerPrefs.SetString("SavedUserId", appleUserId);
                PlayerPrefs.SetInt("RememberMe", 1);
                PlayerPrefs.Save();

                Debug.Log("[Auth] Login Apple exitoso");
                OnLoginSuccess?.Invoke(currentPlayerData);
                return true;
            }
            catch (Exception ex)
            {
                OnLoginFailed?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Cerrar sesión
        /// </summary>
        public void Logout()
        {
            Debug.Log("[Auth] Cerrando sesión...");

            currentPlayerData = null;

            PlayerPrefs.DeleteKey("SavedUserId");
            PlayerPrefs.DeleteKey("RememberMe");
            PlayerPrefs.Save();

            OnLogout?.Invoke();
            Debug.Log("[Auth] Sesión cerrada");
        }

        /// <summary>
        /// Resetear contraseña
        /// </summary>
        public async Task<bool> ResetPassword(string email)
        {
            Debug.Log($"[Auth] Email de reseteo enviado a: {email}");
            await Task.Delay(500);
            return true;
        }

        /// <summary>
        /// Verifica si hay un usuario autenticado
        /// </summary>
        public bool IsUserAuthenticated()
        {
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
        /// Actualiza los datos del jugador actual
        /// </summary>
        public void UpdateCurrentPlayerData(PlayerData playerData)
        {
            currentPlayerData = playerData;
            Debug.Log($"[Auth] Datos actualizados: {playerData.username}");
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

                string userDataKey = $"SimUser_{currentPlayerData.userId}";
                PlayerPrefs.SetString(userDataKey, JsonUtility.ToJson(currentPlayerData));
                PlayerPrefs.Save();

                Debug.Log($"[Auth] Username actualizado: {oldUsername} -> {newUsername}");
                await Task.CompletedTask;
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Error: {ex.Message}");
                return false;
            }
        }
    }
}
