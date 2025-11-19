using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DigitPark.Services.Firebase;
using DigitPark.Data;
using DigitPark.UI.Common;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager de la escena de Settings
    /// Gestiona el cambio de nombre de usuario y cierre de sesión
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        [Header("UI - Settings Panel")]
        [SerializeField] public GameObject settingsPanel;
        [SerializeField] public TextMeshProUGUI titleText;
        [SerializeField] public Button changeNameButton;
        [SerializeField] public Button logoutButton;
        [SerializeField] public Button backButton;

        [Header("UI - Change Name Panel")]
        [SerializeField] public GameObject changeNamePanel;
        [SerializeField] public TextMeshProUGUI changeNameTitleText;
        [SerializeField] public TMP_InputField newNameInput;
        [SerializeField] public Button confirmNameButton;
        [SerializeField] public Button cancelNameButton;

        private PlayerData currentPlayer;

        private void Start()
        {
            Debug.Log("[Settings] SettingsManager iniciado");

            // Cargar datos del jugador
            LoadPlayerData();

            // Configurar listeners
            SetupListeners();

            // Ocultar panel de cambio de nombre inicialmente
            if (changeNamePanel != null)
                changeNamePanel.SetActive(false);

            // Mostrar panel principal
            if (settingsPanel != null)
                settingsPanel.SetActive(true);
        }

        #region Initialization

        /// <summary>
        /// Carga los datos del jugador
        /// </summary>
        private void LoadPlayerData()
        {
            if (AuthenticationService.Instance != null)
            {
                currentPlayer = AuthenticationService.Instance.GetCurrentPlayerData();

                if (currentPlayer == null)
                {
                    Debug.LogError("[Settings] No hay datos del jugador");
                    SceneManager.LoadScene("Login");
                }
            }
        }

        /// <summary>
        /// Configura los listeners
        /// </summary>
        private void SetupListeners()
        {
            changeNameButton?.onClick.AddListener(OnChangeNameButtonClicked);
            logoutButton?.onClick.AddListener(OnLogoutClicked);
            backButton?.onClick.AddListener(OnBackButtonClicked);

            // Listeners del panel de cambio de nombre
            confirmNameButton?.onClick.AddListener(OnConfirmNameClicked);
            cancelNameButton?.onClick.AddListener(OnCancelNameClicked);
        }

        #endregion

        #region Change Name

        /// <summary>
        /// Muestra el panel para cambiar nombre
        /// </summary>
        private void OnChangeNameButtonClicked()
        {
            Debug.Log("[Settings] Mostrando panel de cambio de nombre");

            if (changeNamePanel != null)
            {
                changeNamePanel.SetActive(true);

                // Limpiar el input field
                if (newNameInput != null)
                {
                    newNameInput.text = "";
                    newNameInput.Select();
                    newNameInput.ActivateInputField();
                }
            }
        }

        /// <summary>
        /// Confirma el cambio de nombre
        /// </summary>
        private async void OnConfirmNameClicked()
        {
            if (newNameInput == null || currentPlayer == null) return;

            string newUsername = newNameInput.text.Trim();

            // Validación
            if (string.IsNullOrEmpty(newUsername))
            {
                Debug.LogWarning("[Settings] El nombre no puede estar vacío");
                return;
            }

            if (newUsername.Length < 3)
            {
                Debug.LogWarning("[Settings] El nombre debe tener al menos 3 caracteres");
                return;
            }

            if (newUsername.Length > 20)
            {
                Debug.LogWarning("[Settings] El nombre debe tener máximo 20 caracteres");
                return;
            }

            // Si el nombre no cambió, solo cerrar el panel
            if (newUsername == currentPlayer.username)
            {
                Debug.Log("[Settings] El nombre es el mismo, cerrando panel");
                OnCancelNameClicked();
                return;
            }

            Debug.Log($"[Settings] Cambiando nombre a: {newUsername}");

            // Deshabilitar botones mientras se procesa
            if (confirmNameButton != null) confirmNameButton.interactable = false;
            if (cancelNameButton != null) cancelNameButton.interactable = false;

            // Actualizar nombre en Firebase
            bool success = await AuthenticationService.Instance.UpdateUsername(newUsername);

            if (success)
            {
                Debug.Log("[Settings] ✅ Nombre actualizado exitosamente");
                currentPlayer.username = newUsername;

                // Cerrar panel
                OnCancelNameClicked();
            }
            else
            {
                Debug.LogError("[Settings] ❌ Error al actualizar nombre");

                // Rehabilitar botones
                if (confirmNameButton != null) confirmNameButton.interactable = true;
                if (cancelNameButton != null) cancelNameButton.interactable = true;
            }
        }

        /// <summary>
        /// Cancela el cambio de nombre
        /// </summary>
        private void OnCancelNameClicked()
        {
            Debug.Log("[Settings] Cancelando cambio de nombre");

            if (changeNamePanel != null)
            {
                changeNamePanel.SetActive(false);
            }

            // Limpiar input
            if (newNameInput != null)
            {
                newNameInput.text = "";
            }

            // Rehabilitar botones
            if (confirmNameButton != null) confirmNameButton.interactable = true;
            if (cancelNameButton != null) cancelNameButton.interactable = true;
        }

        #endregion

        #region Logout

        /// <summary>
        /// Cierra sesión
        /// </summary>
        private void OnLogoutClicked()
        {
            Debug.Log("[Settings] Cerrando sesión...");

            // Logout
            AuthenticationService.Instance?.Logout();

            // Volver a Login
            SceneManager.LoadScene("Login");
        }

        #endregion

        #region Navigation

        /// <summary>
        /// Vuelve al menú principal
        /// </summary>
        private void OnBackButtonClicked()
        {
            Debug.Log("[Settings] Volviendo al menú principal");
            SceneManager.LoadScene("MainMenu");
        }

        #endregion
    }
}
