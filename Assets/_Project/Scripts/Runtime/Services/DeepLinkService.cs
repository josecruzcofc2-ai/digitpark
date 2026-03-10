using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DigitPark.Services
{
    /// <summary>
    /// Datos parseados de un deep link
    /// </summary>
    public class DeepLinkData
    {
        public DeepLinkType Type;
        public string Id;
        public Dictionary<string, string> Params = new Dictionary<string, string>();
    }

    /// <summary>
    /// Tipos de deep link soportados
    /// </summary>
    public enum DeepLinkType
    {
        Unknown,
        Profile,
        Tournament,
        Match,
        Invite
    }

    /// <summary>
    /// Servicio de Deep Linking
    /// Maneja URLs con esquema digitpark:// para navegacion directa
    /// Rutas:
    ///   digitpark://profile/{userId}
    ///   digitpark://tournament/{tournamentId}
    ///   digitpark://match/{matchId}
    ///   digitpark://invite/{referralCode}
    /// </summary>
    public class DeepLinkService : MonoBehaviour
    {
        public static DeepLinkService Instance { get; private set; }

        /// <summary>
        /// Deep link pendiente (recibido antes de que el usuario este autenticado)
        /// </summary>
        public DeepLinkData PendingDeepLink { get; private set; }

        /// <summary>
        /// Evento cuando se recibe un deep link
        /// </summary>
        public event Action<DeepLinkData> OnDeepLinkReceived;

        /// <summary>
        /// Si hay un deep link pendiente de procesar
        /// </summary>
        public bool HasPendingDeepLink => PendingDeepLink != null;

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

        private void Initialize()
        {
            // Suscribirse a deep links de Unity
            Application.deepLinkActivated += OnDeepLinkActivated;

            // Verificar si la app fue lanzada por un deep link
            if (!string.IsNullOrEmpty(Application.absoluteURL))
            {
                OnDeepLinkActivated(Application.absoluteURL);
            }

            Debug.Log("[DeepLink] Servicio inicializado");
        }

        private void OnDestroy()
        {
            Application.deepLinkActivated -= OnDeepLinkActivated;
        }

        /// <summary>
        /// Callback cuando se recibe un deep link
        /// </summary>
        private void OnDeepLinkActivated(string url)
        {
            Debug.Log($"[DeepLink] Recibido: {url}");

            DeepLinkData data = ParseUrl(url);
            if (data == null)
            {
                Debug.LogWarning($"[DeepLink] URL invalida: {url}");
                return;
            }

            OnDeepLinkReceived?.Invoke(data);

            // Si el usuario esta autenticado, navegar directamente
            if (IsUserAuthenticated())
            {
                NavigateToDeepLink(data);
            }
            else
            {
                // Guardar para procesar despues de auth
                PendingDeepLink = data;
                Debug.Log($"[DeepLink] Guardado como pendiente: {data.Type} - {data.Id}");
            }
        }

        /// <summary>
        /// Parsea una URL de deep link en DeepLinkData
        /// </summary>
        public DeepLinkData ParseUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;

            // Formato: digitpark://type/id?param1=value1&param2=value2
            try
            {
                Uri uri = new Uri(url);

                if (uri.Scheme != "digitpark") return null;

                string path = uri.Host;
                string[] segments = uri.AbsolutePath.TrimStart('/').Split('/');
                string id = segments.Length > 0 ? segments[0] : "";

                // Si el host es el tipo y el path es el id
                if (string.IsNullOrEmpty(id) && uri.AbsolutePath.Length > 1)
                {
                    id = uri.AbsolutePath.TrimStart('/');
                }

                var data = new DeepLinkData
                {
                    Id = id,
                    Type = ParseType(path)
                };

                // Parsear query params
                if (!string.IsNullOrEmpty(uri.Query))
                {
                    string query = uri.Query.TrimStart('?');
                    foreach (string param in query.Split('&'))
                    {
                        string[] kv = param.Split('=');
                        if (kv.Length == 2)
                        {
                            data.Params[Uri.UnescapeDataString(kv[0])] = Uri.UnescapeDataString(kv[1]);
                        }
                    }
                }

                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DeepLink] Error parseando URL: {e.Message}");
                return null;
            }
        }

        private DeepLinkType ParseType(string type)
        {
            return type.ToLower() switch
            {
                "profile" => DeepLinkType.Profile,
                "tournament" => DeepLinkType.Tournament,
                "match" => DeepLinkType.Match,
                "invite" => DeepLinkType.Invite,
                _ => DeepLinkType.Unknown
            };
        }

        /// <summary>
        /// Navega a la escena correspondiente al deep link
        /// </summary>
        public void NavigateToDeepLink(DeepLinkData data)
        {
            if (data == null) return;

            // Validate ID before using it for navigation or storage
            if (!IsValidId(data.Id))
            {
                Debug.LogWarning($"[DeepLink] ID invalido (caracteres no permitidos): {data.Id}");
                return;
            }

            Debug.Log($"[DeepLink] Navegando a: {data.Type} - {data.Id}");

            switch (data.Type)
            {
                case DeepLinkType.Profile:
                    // Navegar a perfil del usuario
                    // TODO: Pasar userId al ProfileManager
                    PlayerPrefs.SetString("DeepLink_ProfileId", data.Id);
                    SceneManager.LoadScene("Profile");
                    break;

                case DeepLinkType.Tournament:
                    // Navegar al lobby del torneo
                    PlayerPrefs.SetString("DeepLink_TournamentId", data.Id);
                    SceneManager.LoadScene("TournamentLobby");
                    break;

                case DeepLinkType.Match:
                    // Navegar al historial de partida
                    PlayerPrefs.SetString("DeepLink_MatchId", data.Id);
                    SceneManager.LoadScene("MatchHistory");
                    break;

                case DeepLinkType.Invite:
                    // Ir a registro con codigo de referido
                    PlayerPrefs.SetString("DeepLink_ReferralCode", data.Id);
                    SceneManager.LoadScene("Login");
                    break;

                default:
                    Debug.LogWarning($"[DeepLink] Tipo desconocido: {data.Type}");
                    break;
            }
        }

        /// <summary>
        /// Procesa el deep link pendiente (llamar despues de autenticacion exitosa)
        /// </summary>
        public void ProcessPendingDeepLink()
        {
            if (PendingDeepLink == null) return;

            var data = PendingDeepLink;
            PendingDeepLink = null;
            NavigateToDeepLink(data);
        }

        /// <summary>
        /// Limpia el deep link pendiente
        /// </summary>
        public void ClearPendingDeepLink()
        {
            PendingDeepLink = null;
        }

        /// <summary>
        /// Genera una URL de deep link para compartir
        /// </summary>
        public static string GenerateLink(DeepLinkType type, string id)
        {
            string typeName = type.ToString().ToLower();
            return $"digitpark://{typeName}/{id}";
        }

        /// <summary>
        /// Comparte un link usando la funcionalidad nativa del dispositivo
        /// </summary>
        public void ShareLink(string url, string message)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            ShareAndroid(message + "\n" + url);
#elif UNITY_IOS && !UNITY_EDITOR
            ShareIOS(message, url);
#else
            // En editor, copiar al clipboard
            GUIUtility.systemCopyBuffer = url;
            Debug.Log($"[DeepLink] Link copiado al clipboard: {url}");
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private void ShareAndroid(string text)
        {
            try
            {
                using (AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent"))
                using (AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent"))
                {
                    intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
                    intentObject.Call<AndroidJavaObject>("setType", "text/plain");
                    intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), text);

                    using (AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    using (AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, "Share");
                        currentActivity.Call("startActivity", chooser);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[DeepLink] Error compartiendo en Android: {e.Message}");
            }
        }
#endif

#if UNITY_IOS && !UNITY_EDITOR
        private void ShareIOS(string message, string url)
        {
            // Usar Social.framework via native plugin o UIActivityViewController
            // Por simplicidad, copiar al clipboard como fallback
            GUIUtility.systemCopyBuffer = url;
            Debug.Log($"[DeepLink] iOS: Link copiado al clipboard: {url}");
        }
#endif

        /// <summary>
        /// Validates that an ID from a deep link contains only safe characters
        /// (alphanumeric, hyphens, underscores). Prevents injection attacks.
        /// </summary>
        private bool IsValidId(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            return Regex.IsMatch(id, @"^[a-zA-Z0-9_-]+$");
        }

        private bool IsUserAuthenticated()
        {
            // Verificar si AuthenticationService existe y el usuario esta autenticado
            var authService = Firebase.AuthenticationService.Instance;
            return authService != null && authService.IsUserAuthenticated();
        }
    }
}
