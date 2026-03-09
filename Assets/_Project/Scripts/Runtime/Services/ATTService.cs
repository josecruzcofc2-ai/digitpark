using System;
using UnityEngine;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace DigitPark.Services
{
    /// <summary>
    /// App Tracking Transparency (ATT) para iOS 14.5+
    /// Muestra el dialog nativo de ATT antes de inicializar Firebase Analytics
    /// </summary>
    public class ATTService : MonoBehaviour
    {
        public static ATTService Instance { get; private set; }

        public enum ATTStatus
        {
            NotDetermined = 0,
            Restricted = 1,
            Denied = 2,
            Authorized = 3
        }

        /// <summary>
        /// Estado actual del tracking
        /// </summary>
        public ATTStatus TrackingStatus { get; private set; } = ATTStatus.NotDetermined;

        /// <summary>
        /// Si el usuario autorizo el tracking
        /// </summary>
        public bool IsTrackingAuthorized => TrackingStatus == ATTStatus.Authorized;

        /// <summary>
        /// Si ya se solicito el permiso (no importa la respuesta)
        /// </summary>
        public bool HasRequestedPermission => TrackingStatus != ATTStatus.NotDetermined;

        /// <summary>
        /// Evento cuando cambia el estado de ATT
        /// </summary>
        public event Action<ATTStatus> OnTrackingStatusChanged;

        private bool _requestCompleted = false;

        /// <summary>
        /// Si la solicitud de ATT ya termino
        /// </summary>
        public bool RequestCompleted => _requestCompleted;

#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern int ATTService_GetTrackingStatus();

        [DllImport("__Internal")]
        private static extern void ATTService_RequestTracking();
#endif

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Solicita permiso de tracking. En plataformas no-iOS, completa inmediatamente.
        /// </summary>
        public void RequestTrackingAuthorization()
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (IsIOSVersionSupported())
            {
                // Verificar estado actual
                int currentStatus = ATTService_GetTrackingStatus();
                TrackingStatus = (ATTStatus)currentStatus;

                if (TrackingStatus == ATTStatus.NotDetermined)
                {
                    Debug.Log("[ATT] Solicitando permiso de tracking...");
                    ATTService_RequestTracking();
                    // El callback viene desde el plugin nativo via SendMessage
                }
                else
                {
                    Debug.Log($"[ATT] Tracking ya determinado: {TrackingStatus}");
                    _requestCompleted = true;
                    OnTrackingStatusChanged?.Invoke(TrackingStatus);
                }
            }
            else
            {
                // iOS < 14.5, tracking siempre autorizado
                Debug.Log("[ATT] iOS < 14.5, tracking autorizado por defecto");
                TrackingStatus = ATTStatus.Authorized;
                _requestCompleted = true;
                OnTrackingStatusChanged?.Invoke(TrackingStatus);
            }
#else
            // En Editor o Android, autorizar directamente
            Debug.Log("[ATT] Plataforma no-iOS, tracking autorizado por defecto");
            TrackingStatus = ATTStatus.Authorized;
            _requestCompleted = true;
            OnTrackingStatusChanged?.Invoke(TrackingStatus);
#endif
        }

        /// <summary>
        /// Callback desde el plugin nativo iOS cuando el usuario responde al dialog
        /// </summary>
        public void OnTrackingRequestComplete(string statusString)
        {
            if (int.TryParse(statusString, out int status))
            {
                TrackingStatus = (ATTStatus)status;
            }
            else
            {
                TrackingStatus = ATTStatus.Denied;
            }

            _requestCompleted = true;
            Debug.Log($"[ATT] Tracking status: {TrackingStatus}");
            OnTrackingStatusChanged?.Invoke(TrackingStatus);
        }

#if UNITY_IOS
        private bool IsIOSVersionSupported()
        {
            // ATT requiere iOS 14.5+
            string osVersion = UnityEngine.iOS.Device.systemVersion;
            if (Version.TryParse(osVersion, out Version version))
            {
                return version >= new Version(14, 5);
            }
            return false;
        }
#endif
    }
}
