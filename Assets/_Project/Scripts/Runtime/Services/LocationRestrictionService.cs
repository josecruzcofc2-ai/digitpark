using UnityEngine;
using System;
using System.Collections.Generic;

namespace DigitPark.Services
{
    /// <summary>
    /// Servicio para verificar restricciones de ubicacion geografica.
    /// Triumph SDK no permite cash gaming en ciertos estados de USA.
    /// </summary>
    public class LocationRestrictionService : MonoBehaviour
    {
        public static LocationRestrictionService Instance { get; private set; }

        // Estados PROHIBIDOS para cash gaming (Triumph)
        private static readonly HashSet<string> RESTRICTED_STATES = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Arizona", "AZ",
            "Arkansas", "AR",
            "Colorado", "CO",
            "Connecticut", "CT",
            "Delaware", "DE",
            "Florida", "FL",
            "Kentucky", "KY",
            "Louisiana", "LA",
            "Maine", "ME",
            "Maryland", "MD",
            "Mississippi", "MS",
            "Montana", "MT",
            "Nebraska", "NE",
            "New Hampshire", "NH",
            "New Mexico", "NM",
            "South Carolina", "SC",
            "Tennessee", "TN",
            "Puerto Rico", "PR",
            "Washington", "WA"
        };

        // Estados PERMITIDOS (para mostrar al usuario)
        public static readonly string[] ALLOWED_STATES = new string[]
        {
            "Alabama", "Alaska", "California", "Georgia", "Hawaii", "Idaho",
            "Illinois", "Indiana", "Iowa", "Kansas", "Massachusetts", "Minnesota",
            "Missouri", "Nevada", "New Jersey", "New York", "North Carolina",
            "North Dakota", "Ohio", "Oklahoma", "Oregon", "Pennsylvania",
            "Rhode Island", "Texas", "Utah", "Vermont", "Virginia",
            "West Virginia", "Wisconsin", "Wyoming", "Washington D.C."
        };

        // Estado actual del usuario
        public string CurrentState { get; private set; } = "";
        public string CurrentCountry { get; private set; } = "";
        public bool IsLocationKnown { get; private set; } = false;
        public bool IsRestricted { get; private set; } = false;
        public bool IsCheckingLocation { get; private set; } = false;

        // Eventos
        public event Action<bool, string> OnLocationChecked; // (isRestricted, stateName)
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
        /// Verifica la ubicacion del usuario.
        /// En produccion, esto usara Triumph SDK para geolocalizacion.
        /// </summary>
        public void CheckLocation()
        {
            if (IsCheckingLocation) return;

            IsCheckingLocation = true;
            Debug.Log("[LocationRestriction] Verificando ubicacion...");

            // TODO: En produccion, usar Triumph SDK para verificar ubicacion real
            // Por ahora, simulamos que estamos en un estado permitido
            // Triumph SDK maneja la geolocalizacion con GPS

            // Simular delay de verificacion
            Invoke(nameof(SimulateLocationCheck), 0.5f);
        }

        private void SimulateLocationCheck()
        {
            // En desarrollo: Asumir estado permitido
            // En produccion: Triumph SDK determinara esto
            CurrentCountry = "United States";
            CurrentState = "California"; // Estado permitido por defecto
            IsLocationKnown = true;
            IsRestricted = IsStateRestricted(CurrentState);
            IsCheckingLocation = false;

            Debug.Log($"[LocationRestriction] Ubicacion: {CurrentState}, {CurrentCountry}. Restringido: {IsRestricted}");
            OnLocationChecked?.Invoke(IsRestricted, CurrentState);
        }

        /// <summary>
        /// Verifica si un estado esta restringido
        /// </summary>
        public bool IsStateRestricted(string state)
        {
            if (string.IsNullOrEmpty(state)) return false;
            return RESTRICTED_STATES.Contains(state);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Forzar estado restringido (para testing)
        /// </summary>
        public void SetRestrictedState(string state)
        {
            CurrentState = state;
            CurrentCountry = "United States";
            IsLocationKnown = true;
            IsRestricted = IsStateRestricted(state);
            OnLocationChecked?.Invoke(IsRestricted, CurrentState);
        }

        /// <summary>
        /// Forzar estado permitido (para testing)
        /// </summary>
        public void SetAllowedState(string state = "California")
        {
            CurrentState = state;
            CurrentCountry = "United States";
            IsLocationKnown = true;
            IsRestricted = false;
            OnLocationChecked?.Invoke(IsRestricted, CurrentState);
        }
#endif

        /// <summary>
        /// Verifica si el usuario puede acceder a Cash Battle
        /// </summary>
        public bool CanAccessCashBattle()
        {
            // Si no conocemos la ubicacion, asumir que no puede (seguridad)
            if (!IsLocationKnown) return false;

            // Si esta en estado restringido, no puede
            if (IsRestricted) return false;

            return true;
        }

        private void OnDestroy()
        {
            CancelInvoke();
        }
    }
}
