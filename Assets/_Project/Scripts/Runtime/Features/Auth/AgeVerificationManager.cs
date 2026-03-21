using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DigitPark.Monetization;
using DigitPark.Navigation;
using DigitPark.Services;
using DigitPark.Localization;
using DG.Tweening;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager para la escena de verificación de edad (18+).
    /// SIMPLIFICADO: Triumph SDK maneja todo el flujo de KYC con su propia UI.
    /// Este manager solo lanza el flujo de Triumph y muestra el estado.
    /// </summary>
    public class AgeVerificationManager : MonoBehaviour
    {
        [Header("UI - Textos")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("UI - Botones")]
        [SerializeField] private Button verifyButton;      // Botón principal para iniciar verificación Triumph
        [SerializeField] private Button backButton;        // Botón de regreso

        [Header("UI - Indicadores (Opcionales)")]
        [SerializeField] private GameObject loadingIndicator;
        [SerializeField] private GameObject successIcon;
        [SerializeField] private GameObject errorIcon;

        [Header("URLs Legales (fallback — Remote Config overrides)")]
        [SerializeField] private string termsUrl = "https://digitpark.com/terms";
        [SerializeField] private string privacyUrl = "https://digitpark.com/privacy";

        private string TermsUrl =>
            DigitPark.Payments.RemoteConfigService.Instance?.GetCurrentConfig()?.TermsUrl ?? termsUrl;
        private string PrivacyUrl =>
            DigitPark.Payments.RemoteConfigService.Instance?.GetCurrentConfig()?.PrivacyUrl ?? privacyUrl;

        // Events
        public static event Action<bool> OnVerificationComplete;

        private bool isVerifying = false;
        private IKYCService _kycService;

        private void Start()
        {
            if (ServiceLocator.Exists)
            {
                _kycService = ServiceLocator.KYC;
            }

            if (_kycService == null)
            {
                Debug.Log("[AgeVerification] ServiceLocator.KYC no disponible - verificación manual requerida.");
            }

            SetupUI();
            SetupListeners();
            LoadCurrentState();
        }

        private void OnEnable()
        {
            if (_kycService != null)
            {
                _kycService.OnStatusChanged += OnKYCStatusChanged;
            }
        }

        private void OnDisable()
        {
            CancelInvoke();
            if (_kycService != null)
            {
                _kycService.OnStatusChanged -= OnKYCStatusChanged;
            }
        }

        private void SetupUI()
        {
            ShowLoadingIndicator(false);
            if (successIcon) successIcon.SetActive(false);
            if (errorIcon) errorIcon.SetActive(false);
            if (statusText) statusText.text = "";
        }

        private void SetupListeners()
        {
            if (verifyButton) verifyButton.onClick.AddListener(OnVerifyClicked);
            // Disable auto-navigation from BackButton prefab to prevent double listener
            var autoNav = backButton?.GetComponent<DigitPark.UI.BackButton>();
            if (autoNav != null) autoNav.DisableAutoNavigation();
            if (backButton) backButton.onClick.AddListener(OnBackClicked);
        }

        private void LoadCurrentState()
        {
            if (_kycService == null) return;

            var status = _kycService.CurrentStatus;
            Debug.Log($"[AgeVerification] Estado KYC actual: {status}");
            UpdateUIForStatus(status);
        }

        private void OnKYCStatusChanged(KYCStatus newStatus)
        {
            Debug.Log($"[AgeVerification] Estado KYC cambió a: {newStatus}");
            UpdateUIForStatus(newStatus);
        }

        private void UpdateUIForStatus(KYCStatus status)
        {
            switch (status)
            {
                case KYCStatus.NotStarted:
                case KYCStatus.AgeVerified:
                    ShowVerificationNeeded();
                    break;

                case KYCStatus.DocumentPending:
                    ShowPendingVerification();
                    break;

                case KYCStatus.FullyVerified:
                    ShowFullyVerified();
                    break;

                case KYCStatus.Rejected:
                    ShowRejected();
                    break;
            }
        }

        private void ShowVerificationNeeded()
        {
            if (statusText)
            {
                statusText.text = L("age_verification_status_needed");
                statusText.color = Color.white;
            }
            if (successIcon) successIcon.SetActive(false);
            if (errorIcon) errorIcon.SetActive(false);
            if (verifyButton) verifyButton.interactable = true;
        }

        private void ShowPendingVerification()
        {
            if (statusText)
            {
                statusText.text = L("age_verification_status_pending");
                statusText.color = new Color(1f, 0.84f, 0f);
            }
            ShowLoadingIndicator(true);
            if (verifyButton) verifyButton.interactable = false;
        }

        private void ShowFullyVerified()
        {
            if (statusText)
            {
                statusText.text = L("age_verification_status_complete");
                statusText.color = new Color(0.3f, 1f, 0.5f);
            }
            if (successIcon) successIcon.SetActive(true);
            if (errorIcon) errorIcon.SetActive(false);
            ShowLoadingIndicator(false);
            if (verifyButton) verifyButton.gameObject.SetActive(false);

            OnVerificationComplete?.Invoke(true);
            Invoke(nameof(NavigateBack), 1.5f);
        }

        private void ShowRejected()
        {
            if (statusText)
            {
                statusText.text = L("age_verification_status_failed");
                statusText.color = new Color(1f, 0.4f, 0.4f);
            }
            if (successIcon) successIcon.SetActive(false);
            if (errorIcon) errorIcon.SetActive(true);
            ShowLoadingIndicator(false);
            if (verifyButton) verifyButton.interactable = true;

            OnVerificationComplete?.Invoke(false);
        }

        /// <summary>
        /// Llamado cuando el usuario toca "Verificar mi Edad"
        /// Lanza el flujo de verificación de Triumph SDK
        /// </summary>
        private async void OnVerifyClicked()
        {
            try
            {
                if (isVerifying || _kycService == null) return;

                isVerifying = true;
                ShowLoadingIndicator(true);
                if (statusText) statusText.text = L("age_verification_status_pending");
                if (verifyButton) verifyButton.interactable = false;

                Debug.Log("[AgeVerification] Iniciando verificación via Triumph SDK");

                // Triumph SDK maneja TODO el flujo de verificación con su propia UI
                var result = await _kycService.StartIdentityVerification();

                // Verificar que no se destruyó el objeto durante el await
                if (this == null) return;

                isVerifying = false;
                ShowLoadingIndicator(false);

                if (!result.Success)
                {
                    ShowError(result.Message ?? L("age_verification_error_generic"));
                }
                // Si es exitoso, OnKYCStatusChanged actualizará la UI
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                if (this != null)
                {
                    isVerifying = false;
                    ShowLoadingIndicator(false);
                    if (verifyButton) verifyButton.interactable = true;
                    if (statusText)
                    {
                        statusText.text = L("age_verification_error_generic");
                        statusText.color = new Color(1f, 0.4f, 0.4f);
                    }
                }
            }
        }

        private void ShowError(string message)
        {
            if (statusText)
            {
                statusText.text = message;
                statusText.color = new Color(1f, 0.4f, 0.4f);
            }
            if (successIcon) successIcon.SetActive(false);
            if (errorIcon) errorIcon.SetActive(true);
            if (verifyButton) verifyButton.interactable = true;
        }

        private void OnBackClicked()
        {
            NavigateBack();
        }

        private void NavigateBack()
        {
            if (SceneNavigator.Instance != null)
            {
                SceneNavigator.Instance.GoBack();
            }
        }

        /// <summary>
        /// Check if user is age verified (legacy static method)
        /// Ahora usa ServiceLocator
        /// </summary>
        public static bool IsVerified()
        {
            if (ServiceLocator.KYC != null)
            {
                return ServiceLocator.KYC.CanAccessCashBattle;
            }
            // Fallback a PlayerPrefs para compatibilidad
            return PlayerPrefs.GetInt("DP_AgeVerified", 0) == 1;
        }

        /// <summary>
        /// Check if user is fully KYC verified
        /// </summary>
        public static bool IsFullyVerified()
        {
            return ServiceLocator.KYC?.IsFullyVerified ?? false;
        }

        /// <summary>
        /// Get current KYC status
        /// </summary>
        public static KYCStatus GetKYCStatus()
        {
            return ServiceLocator.KYC?.CurrentStatus ?? KYCStatus.NotStarted;
        }

        /// <summary>
        /// Reset verification status (for testing)
        /// </summary>
        public static async void ResetVerification()
        {
            try
            {
                if (ServiceLocator.KYC != null)
                {
                    await ServiceLocator.KYC.ResetVerification();
                }
                // También limpiar PlayerPrefs por compatibilidad
                PlayerPrefs.DeleteKey("DP_AgeVerified");
                PlayerPrefs.DeleteKey("DP_BirthDate");
                PlayerPrefs.Save();
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void ShowLoadingIndicator(bool show)
        {
            if (loadingIndicator == null) return;

            if (show)
            {
                loadingIndicator.SetActive(true);
                var cg = loadingIndicator.GetComponent<CanvasGroup>();
                if (cg == null) cg = loadingIndicator.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                cg.DOFade(1f, 0.2f).SetUpdate(true);
            }
            else
            {
                var cg = loadingIndicator.GetComponent<CanvasGroup>();
                if (cg != null)
                    cg.DOFade(0f, 0.2f).SetUpdate(true).OnComplete(() => loadingIndicator.SetActive(false));
                else
                    loadingIndicator.SetActive(false);
            }
        }

        private string L(string key, params object[] args)
        {
            return args.Length > 0 ? AutoLocalizer.Get(key, args) : AutoLocalizer.Get(key);
        }

        private void OnDestroy()
        {
            CancelInvoke();
            transform.DOKill();
        }
    }
}
