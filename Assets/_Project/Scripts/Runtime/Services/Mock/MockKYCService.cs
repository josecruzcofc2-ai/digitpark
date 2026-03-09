using System;
using System.Threading.Tasks;
using UnityEngine;

namespace DigitPark.Services.Mock
{
    /// <summary>
    /// Mock implementation of the KYC service for development and testing.
    /// Simulates all operations without connecting to real APIs.
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

        // Simulation configuration
        public float SimulatedDelaySeconds { get; set; } = 1.5f;
        public bool SimulateVerificationFailure { get; set; } = false;
        public float FailureChance { get; set; } = 0f; // 0-1

        public MockKYCService()
        {
            LoadState();
        }

        private void LoadState()
        {
            // Load saved state
            _currentStatus = (KYCStatus)PlayerPrefs.GetInt(PREFS_KYC_STATUS, 0);

            _userInfo = new UserVerificationInfo
            {
                UserId = "mock_user_" + SystemInfo.deviceUniqueIdentifier.Substring(0, 8),
                Status = _currentStatus
            };

            // Load birth date if it exists
            string birthDateStr = PlayerPrefs.GetString(PREFS_BIRTH_DATE, "");
            if (!string.IsNullOrEmpty(birthDateStr) && DateTime.TryParse(birthDateStr, out DateTime birthDate))
            {
                _userInfo.BirthDate = birthDate;
                _userInfo.Age = CalculateAge(birthDate);
            }

            // Load verification date if it exists
            string verificationDateStr = PlayerPrefs.GetString(PREFS_VERIFICATION_DATE, "");
            if (!string.IsNullOrEmpty(verificationDateStr) && DateTime.TryParse(verificationDateStr, out DateTime verificationDate))
            {
                _userInfo.VerificationDate = verificationDate;
                _userInfo.ExpirationDate = verificationDate.AddYears(1); // Expires in 1 year
            }

            Debug.Log($"[MockKYC] State loaded: {_currentStatus}");
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
                Debug.Log($"[MockKYC] Status updated: {newStatus}");
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
            Debug.Log($"[MockKYC] Verifying age: {birthDate:yyyy-MM-dd}");

            // Simulate network delay
            await Task.Delay((int)(SimulatedDelaySeconds * 1000));

            // Simulate failure if configured
            if (ShouldSimulateFailure())
            {
                Debug.Log("[MockKYC] Simulating age verification failure");
                return KYCResult.Failed("Simulated verification error", "MOCK_FAILURE");
            }

            int age = CalculateAge(birthDate);
            _userInfo.BirthDate = birthDate;
            _userInfo.Age = age;

            if (age < 18)
            {
                Debug.Log($"[MockKYC] Underage user: {age} years old");
                UpdateStatus(KYCStatus.Rejected);
                return KYCResult.Failed("You must be 18 or older to use Cash Battle", "AGE_REQUIREMENT");
            }

            Debug.Log($"[MockKYC] Age verified: {age} years old");
            UpdateStatus(KYCStatus.AgeVerified);
            return KYCResult.Successful(KYCStatus.AgeVerified, "Age verified successfully");
        }

        public async Task<KYCResult> StartIdentityVerification()
        {
            Debug.Log("[MockKYC] Starting identity verification (SIMULATED)");

            // Verify that age has been verified first
            if (_currentStatus == KYCStatus.NotStarted)
            {
                return KYCResult.Failed("You must verify your age first", "AGE_NOT_VERIFIED");
            }

            UpdateStatus(KYCStatus.DocumentPending);

            // Simulate verification process (in production, Triumph opens its UI)
            Debug.Log("[MockKYC] Simulating document + selfie process...");
            await Task.Delay((int)(SimulatedDelaySeconds * 2 * 1000));

            // Simulate failure if configured
            if (ShouldSimulateFailure())
            {
                Debug.Log("[MockKYC] Simulating identity verification failure");
                UpdateStatus(KYCStatus.Rejected);
                return KYCResult.Failed("Identity verification rejected", "IDENTITY_REJECTED");
            }

            // Success
            _userInfo.VerificationDate = DateTime.UtcNow;
            _userInfo.ExpirationDate = DateTime.UtcNow.AddYears(1);

            UpdateStatus(KYCStatus.FullyVerified);

            Debug.Log("[MockKYC] Verification complete - User can use Cash Battle");
            return KYCResult.Successful(KYCStatus.FullyVerified, "Identity verified successfully");
        }

        public async Task<KYCResult> RefreshVerificationStatus()
        {
            Debug.Log("[MockKYC] Refreshing verification status...");

            await Task.Delay((int)(SimulatedDelaySeconds * 0.5f * 1000));

            // In mock, we simply return the current status
            return KYCResult.Successful(_currentStatus);
        }

        public async Task<KYCResult> ResetVerification()
        {
            Debug.Log("[MockKYC] Resetting verification (testing only)");

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

            return KYCResult.Successful(KYCStatus.NotStarted, "Verification reset");
        }
    }
}
