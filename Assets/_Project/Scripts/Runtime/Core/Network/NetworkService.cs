using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace DigitPark.Services
{
    /// <summary>
    /// Servicio de monitoreo de conectividad de red
    /// Monitorea la conexion a internet y notifica cambios de estado
    /// </summary>
    public class NetworkService : MonoBehaviour
    {
        public static NetworkService Instance { get; private set; }

        public enum ConnectionType
        {
            None,
            Mobile,
            WiFi
        }

        [Header("Settings")]
        [SerializeField] private float checkInterval = 3f;
        [SerializeField] private string pingUrl = "https://www.google.com/generate_204";
        [SerializeField] private float pingTimeout = 5f;

        /// <summary>
        /// Si hay conexion a internet confirmada
        /// </summary>
        public bool IsOnline { get; private set; } = true;

        /// <summary>
        /// Tipo de conexion actual
        /// </summary>
        public ConnectionType CurrentConnectionType { get; private set; } = ConnectionType.WiFi;

        /// <summary>
        /// Se dispara cuando se pierde la conexion
        /// </summary>
        public event Action OnConnectionLost;

        /// <summary>
        /// Se dispara cuando se restaura la conexion
        /// </summary>
        public event Action OnConnectionRestored;

        /// <summary>
        /// Se dispara cuando cambia el estado de conexion (bool isOnline)
        /// </summary>
        public event Action<bool> OnConnectionStatusChanged;

        private bool _previousOnlineState = true;
        private Coroutine _monitorCoroutine;

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
            // Verificar estado inicial
            UpdateConnectionType();
            IsOnline = Application.internetReachability != NetworkReachability.NotReachable;
            _previousOnlineState = IsOnline;

            // Iniciar monitoreo periodico
            _monitorCoroutine = StartCoroutine(MonitorConnection());
            Debug.Log($"[Network] Inicializado - Online: {IsOnline}, Tipo: {CurrentConnectionType}");
        }

        private void OnDestroy()
        {
            StopAllCoroutines();

            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Coroutine que monitorea la conexion periodicamente
        /// </summary>
        private IEnumerator MonitorConnection()
        {
            while (true)
            {
                yield return new WaitForSeconds(checkInterval);

                UpdateConnectionType();

                // Verificacion rapida via Application.internetReachability
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    SetOnlineState(false);
                }
                else
                {
                    // Ping real para confirmar conectividad
                    yield return StartCoroutine(PingCheck());
                }
            }
        }

        /// <summary>
        /// Hace un ping real para verificar conectividad
        /// </summary>
        private IEnumerator PingCheck()
        {
            using (UnityWebRequest request = UnityWebRequest.Head(pingUrl))
            {
                request.timeout = (int)pingTimeout;
                yield return request.SendWebRequest();

                bool reachable = request.result == UnityWebRequest.Result.Success;
                SetOnlineState(reachable);
            }
        }

        /// <summary>
        /// Actualiza el tipo de conexion
        /// </summary>
        private void UpdateConnectionType()
        {
            switch (Application.internetReachability)
            {
                case NetworkReachability.NotReachable:
                    CurrentConnectionType = ConnectionType.None;
                    break;
                case NetworkReachability.ReachableViaCarrierDataNetwork:
                    CurrentConnectionType = ConnectionType.Mobile;
                    break;
                case NetworkReachability.ReachableViaLocalAreaNetwork:
                    CurrentConnectionType = ConnectionType.WiFi;
                    break;
            }
        }

        /// <summary>
        /// Establece el estado online y dispara eventos si cambio
        /// </summary>
        private void SetOnlineState(bool online)
        {
            IsOnline = online;

            if (online != _previousOnlineState)
            {
                _previousOnlineState = online;
                OnConnectionStatusChanged?.Invoke(online);

                if (online)
                {
                    Debug.Log("[Network] Conexion restaurada");
                    OnConnectionRestored?.Invoke();
                }
                else
                {
                    Debug.Log("[Network] Conexion perdida");
                    OnConnectionLost?.Invoke();
                }
            }
        }

        /// <summary>
        /// Fuerza una verificacion inmediata de conectividad
        /// </summary>
        public void CheckNow()
        {
            StartCoroutine(ForceCheck());
        }

        private IEnumerator ForceCheck()
        {
            UpdateConnectionType();

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                SetOnlineState(false);
            }
            else
            {
                yield return StartCoroutine(PingCheck());
            }
        }
    }
}
