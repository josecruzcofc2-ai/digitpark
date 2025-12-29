using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager de Cash Battle - Competencias con dinero real (18+)
    /// Integra con Triumph para pagos
    /// </summary>
    public class CashBattleManager : MonoBehaviour
    {
        [Header("UI - Main Panel")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI balanceText;

        [Header("UI - Navigation Buttons")]
        [SerializeField] private Button battles1v1Button;
        [SerializeField] private Button cashTournamentsButton;
        [SerializeField] private Button walletButton;
        [SerializeField] private Button backButton;

        [Header("UI - Age Verification")]
        [SerializeField] private GameObject ageVerificationPanel;
        [SerializeField] private Button verifyAgeButton;
        [SerializeField] private TextMeshProUGUI verificationStatusText;

        [Header("Settings")]
        [SerializeField] private bool requireAgeVerification = true;

        private bool isAgeVerified = false;
        private float currentBalance = 0f;

        private void Start()
        {
            Debug.Log("[CashBattle] CashBattleManager iniciado");

            SetupListeners();
            CheckAgeVerification();
            UpdateUI();
        }

        private void SetupListeners()
        {
            battles1v1Button?.onClick.AddListener(OnBattles1v1Clicked);
            cashTournamentsButton?.onClick.AddListener(OnCashTournamentsClicked);
            walletButton?.onClick.AddListener(OnWalletClicked);
            backButton?.onClick.AddListener(OnBackClicked);
            verifyAgeButton?.onClick.AddListener(OnVerifyAgeClicked);
        }

        private void CheckAgeVerification()
        {
            // TODO: Verificar con Triumph si el usuario ya está verificado
            isAgeVerified = PlayerPrefs.GetInt("AgeVerified", 0) == 1;

            if (requireAgeVerification && !isAgeVerified)
            {
                ShowAgeVerificationPanel();
            }
            else
            {
                ShowMainPanel();
            }
        }

        private void ShowAgeVerificationPanel()
        {
            if (ageVerificationPanel != null)
                ageVerificationPanel.SetActive(true);
            if (mainPanel != null)
                mainPanel.SetActive(false);
        }

        private void ShowMainPanel()
        {
            if (ageVerificationPanel != null)
                ageVerificationPanel.SetActive(false);
            if (mainPanel != null)
                mainPanel.SetActive(true);
        }

        private void UpdateUI()
        {
            // TODO: Obtener balance real de Triumph
            currentBalance = PlayerPrefs.GetFloat("CashBalance", 0f);

            if (balanceText != null)
                balanceText.text = $"${currentBalance:F2}";
        }

        #region Button Callbacks

        private void OnBattles1v1Clicked()
        {
            Debug.Log("[CashBattle] Navegando a Battles 1v1");

            if (!isAgeVerified && requireAgeVerification)
            {
                ShowAgeVerificationPanel();
                return;
            }

            SceneManager.LoadScene("CashBattle1v1");
        }

        private void OnCashTournamentsClicked()
        {
            Debug.Log("[CashBattle] Navegando a Cash Tournaments");

            if (!isAgeVerified && requireAgeVerification)
            {
                ShowAgeVerificationPanel();
                return;
            }

            SceneManager.LoadScene("CashTournaments");
        }

        private void OnWalletClicked()
        {
            Debug.Log("[CashBattle] Abriendo Wallet");

            if (!isAgeVerified && requireAgeVerification)
            {
                ShowAgeVerificationPanel();
                return;
            }

            // TODO: Abrir panel de wallet o integrar con Triumph
            SceneManager.LoadScene("Wallet");
        }

        private void OnBackClicked()
        {
            Debug.Log("[CashBattle] Volviendo al Main Menu");
            SceneManager.LoadScene("MainMenu");
        }

        private void OnVerifyAgeClicked()
        {
            Debug.Log("[CashBattle] Iniciando verificacion de edad");

            // TODO: Integrar con Triumph para verificacion real
            // Por ahora simulamos la verificacion
            StartAgeVerification();
        }

        #endregion

        #region Age Verification

        private void StartAgeVerification()
        {
            // TODO: Llamar a Triumph SDK para verificacion de ID
            // Triumph.VerifyAge(OnAgeVerificationComplete);

            // Simulacion temporal
            if (verificationStatusText != null)
                verificationStatusText.text = "Verificando...";

            // Simular delay de verificacion
            Invoke(nameof(SimulateVerificationComplete), 2f);
        }

        private void SimulateVerificationComplete()
        {
            // Simulacion - en produccion esto viene de Triumph
            OnAgeVerificationComplete(true);
        }

        private void OnAgeVerificationComplete(bool verified)
        {
            isAgeVerified = verified;
            PlayerPrefs.SetInt("AgeVerified", verified ? 1 : 0);
            PlayerPrefs.Save();

            if (verified)
            {
                Debug.Log("[CashBattle] Edad verificada exitosamente");
                if (verificationStatusText != null)
                    verificationStatusText.text = "Verificado!";

                // Mostrar panel principal despues de un momento
                Invoke(nameof(ShowMainPanel), 1f);
            }
            else
            {
                Debug.Log("[CashBattle] Verificacion de edad fallida");
                if (verificationStatusText != null)
                    verificationStatusText.text = "Verificacion fallida. Debes ser mayor de 18.";
            }
        }

        #endregion
    }
}
