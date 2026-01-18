using System;
using System.Threading.Tasks;
using UnityEngine;

namespace DigitPark.Services.Mock
{
    /// <summary>
    /// Implementación Mock del servicio KYC para desarrollo y testing
    /// Simula todas las operaciones sin conectarse a APIs reales
    /// </summary>
    public class MockKYCService : IKYCService
    {
        private const string PREFS_KYC_STATUS = "Mock_KYC_Status";
        private const string PREFS_BIRTH_DATE = "Mock_KYC_BirthDate";
        private const string PREFS_VERIFICATION_DATE = "Mock_KYC_VerificationDate";

        private KYCStatus _currentStatus;
        private UserVerificationInfo _userInfo;

        public KYCStatus CurrentStatus => _currentStatus;
        public bool IsFullyVerified => _currentStatus == KYCStatus.FullyVerified;
        public bool CanAccessCashBattle => IsFullyVerified;
        public UserVerificationInfo UserInfo => _userInfo;

        public event Action<KYCStatus> OnStatusChanged;

        // Configuración de simulación
        public float SimulatedDelaySeconds { get; set; } = 1.5f;
        public bool SimulateVerificationFailure { get; set; } = false;
        public float FailureChance { get; set; } = 0f; // 0-1

        public MockKYCService()
        {
            LoadState();
        }

        private void LoadState()
        {
            // Cargar estado guardado
            _currentStatus = (KYCStatus)PlayerPrefs.GetInt(PREFS_KYC_STATUS, 0);

            _userInfo = new UserVerificationInfo
            {
                UserId = "mock_user_" + SystemInfo.deviceUniqueIdentifier.Substring(0, 8),
                Status = _currentStatus
            };

            // Cargar fecha de nacimiento si existe
            string birthDateStr = PlayerPrefs.GetString(PREFS_BIRTH_DATE, "");
            if (!string.IsNullOrEmpty(birthDateStr) && DateTime.TryParse(birthDateStr, out DateTime birthDate))
            {
                _userInfo.BirthDate = birthDate;
                _userInfo.Age = CalculateAge(birthDate);
            }

            // Cargar fecha de verificación si existe
            string verificationDateStr = PlayerPrefs.GetString(PREFS_VERIFICATION_DATE, "");
            if (!string.IsNullOrEmpty(verificationDateStr) && DateTime.TryParse(verificationDateStr, out DateTime verificationDate))
            {
                _userInfo.VerificationDate = verificationDate;
                _userInfo.ExpirationDate = verificationDate.AddYears(1); // Expira en 1 año
            }

            Debug.Log($"[MockKYC] Estado cargado: {_currentStatus}");
        }

        private void SaveState()
        {
            PlayerPrefs.SetInt(PREFS_KYC_STATUS, (int)_currentStatus);

            if (_userInfo.BirthDate.HasValue)
                PlayerPrefs.SetString(PREFS_BIRTH_DATE, _userInfo.BirthDate.Value.ToString("O"));

            if (_userInfo.VerificationDate.HasValue)
                PlayerPrefs.SetString(PREFS_VERIFICATION_DATE, _userInfo.VerificationDate.Value.ToString("O"));

            PlayerPrefs.Save();
        }

        private int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;
            return age;
        }

        private void UpdateStatus(KYCStatus newStatus)
        {
            if (_currentStatus != newStatus)
            {
                _currentStatus = newStatus;
                _userInfo.Status = newStatus;
                SaveState();
                OnStatusChanged?.Invoke(newStatus);
                Debug.Log($"[MockKYC] Estado actualizado: {newStatus}");
            }
        }

        private bool ShouldSimulateFailure()
        {
            if (SimulateVerificationFailure) return true;
            if (FailureChance > 0) return UnityEngine.Random.value < FailureChance;
            return false;
        }

        public async Task<KYCResult> VerifyAge(DateTime birthDate)
        {
            Debug.Log($"[MockKYC] Verificando edad: {birthDate:yyyy-MM-dd}");

            // Simular delay de red
            await Task.Delay((int)(SimulatedDelaySeconds * 1000));

            // Simular fallo si está configurado
            if (ShouldSimulateFailure())
            {
                Debug.Log("[MockKYC] Simulando fallo de verificación de edad");
                return KYCResult.Failed("Error de verificación simulado", "MOCK_FAILURE");
            }

            int age = CalculateAge(birthDate);
            _userInfo.BirthDate = birthDate;
            _userInfo.Age = age;

            if (age < 18)
            {
                Debug.Log($"[MockKYC] Usuario menor de edad: {age} años");
                UpdateStatus(KYCStatus.Rejected);
                return KYCResult.Failed("Debes ser mayor de 18 años para usar Cash Battle", "AGE_REQUIREMENT");
            }

            Debug.Log($"[MockKYC] Edad verificada: {age} años");
            UpdateStatus(KYCStatus.AgeVerified);
            return KYCResult.Successful(KYCStatus.AgeVerified, "Edad verificada correctamente");
        }

        public async Task<KYCResult> StartIdentityVerification()
        {
            Debug.Log("[MockKYC] Iniciando verificación de identidad (SIMULADA)");

            // Verificar que primero se haya verificado edad
            if (_currentStatus == KYCStatus.NotStarted)
            {
                return KYCResult.Failed("Primero debes verificar tu edad", "AGE_NOT_VERIFIED");
            }

            UpdateStatus(KYCStatus.DocumentPending);

            // Simular proceso de verificación (en real, Triumph abre su UI)
            Debug.Log("[MockKYC] Simulando proceso de documento + selfie...");
            await Task.Delay((int)(SimulatedDelaySeconds * 2 * 1000));

            // Simular fallo si está configurado
            if (ShouldSimulateFailure())
            {
                Debug.Log("[MockKYC] Simulando fallo de verificación de identidad");
                UpdateStatus(KYCStatus.Rejected);
                return KYCResult.Failed("Verificación de identidad rechazada", "IDENTITY_REJECTED");
            }

            // Éxito
            _userInfo.VerificationDate = DateTime.UtcNow;
            _userInfo.ExpirationDate = DateTime.UtcNow.AddYears(1);

            UpdateStatus(KYCStatus.FullyVerified);

            Debug.Log("[MockKYC] Verificación completa - Usuario puede usar Cash Battle");
            return KYCResult.Successful(KYCStatus.FullyVerified, "Identidad verificada correctamente");
        }

        public async Task<KYCResult> RefreshVerificationStatus()
        {
            Debug.Log("[MockKYC] Refrescando estado de verificación...");

            await Task.Delay((int)(SimulatedDelaySeconds * 0.5f * 1000));

            // En mock, simplemente retornamos el estado actual
            return KYCResult.Successful(_currentStatus);
        }

        public async Task<KYCResult> ResetVerification()
        {
            Debug.Log("[MockKYC] Reseteando verificación (solo para testing)");

            await Task.Delay(100);

            _currentStatus = KYCStatus.NotStarted;
            _userInfo = new UserVerificationInfo
            {
                UserId = _userInfo.UserId,
                Status = KYCStatus.NotStarted
            };

            PlayerPrefs.DeleteKey(PREFS_KYC_STATUS);
            PlayerPrefs.DeleteKey(PREFS_BIRTH_DATE);
            PlayerPrefs.DeleteKey(PREFS_VERIFICATION_DATE);
            PlayerPrefs.Save();

            OnStatusChanged?.Invoke(KYCStatus.NotStarted);

            return KYCResult.Successful(KYCStatus.NotStarted, "Verificación reseteada");
        }
    }
}
