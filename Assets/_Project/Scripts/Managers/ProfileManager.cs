using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DigitPark.Services.Firebase;
using DigitPark.Data;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager del perfil de usuario
    /// Muestra estadisticas, historial y permite gestionar amigos
    /// Version actualizada - Sin coins, gems, levels
    /// </summary>
    public class ProfileManager : MonoBehaviour
    {
        [Header("UI - Profile Info")]
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private Image avatarImage;

        [Header("UI - General Stats")]
        [SerializeField] private TextMeshProUGUI totalGamesText;
        [SerializeField] private TextMeshProUGUI winsText;
        [SerializeField] private TextMeshProUGUI winRateText;
        [SerializeField] private TextMeshProUGUI bestTimeText;
        [SerializeField] private TextMeshProUGUI averageTimeText;

        [Header("UI - Game Stats Values (Solo valores, labels son fijos)")]
        [SerializeField] private TextMeshProUGUI digitRushValueText;
        [SerializeField] private TextMeshProUGUI memoryPairsValueText;
        [SerializeField] private TextMeshProUGUI quickMathValueText;
        [SerializeField] private TextMeshProUGUI flashTapValueText;
        [SerializeField] private TextMeshProUGUI oddOneOutValueText;

        [Header("UI - Navigation")]
        [SerializeField] private Button friendsButton;
        [SerializeField] private Button historyButton;
        [SerializeField] private Button editProfileButton;
        [SerializeField] private Button backButton;

        [Header("UI - Other Player Profile")]
        [SerializeField] private GameObject otherProfileButtonsContainer;
        [SerializeField] private Button addFriendButton;
        [SerializeField] private Button challengeButton;
        [SerializeField] private TextMeshProUGUI friendStatusText;

        private PlayerData currentPlayerData;
        private string viewingPlayerId;
        private bool isOwnProfile = true;

        private void Start()
        {
            Debug.Log("[Profile] ProfileManager iniciado");

            SetupListeners();

            // Verificar si venimos a ver el perfil de otro jugador
            string viewProfileId = PlayerPrefs.GetString("ViewProfileId", "");
            if (!string.IsNullOrEmpty(viewProfileId))
            {
                PlayerPrefs.DeleteKey("ViewProfileId");
                LoadProfileData(viewProfileId);
            }
            else
            {
                LoadProfileData();
            }
        }

        private void SetupListeners()
        {
            friendsButton?.onClick.AddListener(OnFriendsClicked);
            historyButton?.onClick.AddListener(OnHistoryClicked);
            editProfileButton?.onClick.AddListener(OnEditProfileClicked);
            backButton?.onClick.AddListener(OnBackClicked);

            addFriendButton?.onClick.AddListener(OnAddFriendClicked);
            challengeButton?.onClick.AddListener(OnChallengeClicked);
        }

        public void LoadProfileData(string playerId = null)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                // Cargar perfil propio
                isOwnProfile = true;
                LoadOwnProfile();
            }
            else
            {
                // Cargar perfil de otro jugador
                isOwnProfile = false;
                viewingPlayerId = playerId;
                LoadOtherProfile(playerId);
            }
        }

        private void LoadOwnProfile()
        {
            // Obtener datos del jugador actual desde AuthenticationService
            if (AuthenticationService.Instance != null)
            {
                currentPlayerData = AuthenticationService.Instance.GetCurrentPlayerData();
            }

            // Ocultar botones de otro perfil en perfil propio
            if (otherProfileButtonsContainer != null)
                otherProfileButtonsContainer.SetActive(false);
            if (editProfileButton != null)
                editProfileButton.gameObject.SetActive(true);

            UpdateUI();
        }

        private async void LoadOtherProfile(string playerId)
        {
            // Cargar datos del jugador desde Firebase
            if (DatabaseService.Instance != null)
            {
                currentPlayerData = await DatabaseService.Instance.GetPlayerDataById(playerId);
            }

            // Mostrar botones de otro perfil
            if (otherProfileButtonsContainer != null)
                otherProfileButtonsContainer.SetActive(true);
            if (editProfileButton != null)
                editProfileButton.gameObject.SetActive(false);

            // Verificar si ya es amigo
            CheckFriendStatus(playerId);

            UpdateUI();
        }

        private void CheckFriendStatus(string playerId)
        {
            // Verificar si ya son amigos usando PlayerData del usuario actual
            PlayerData myData = null;
            if (AuthenticationService.Instance != null)
            {
                myData = AuthenticationService.Instance.GetCurrentPlayerData();
            }

            bool isFriend = myData != null && myData.IsFriend(playerId);

            if (friendStatusText != null)
            {
                friendStatusText.text = isFriend ? "Amigos" : "";
                friendStatusText.gameObject.SetActive(isFriend);
            }

            // Mostrar/ocultar boton de agregar amigo
            if (addFriendButton != null)
                addFriendButton.gameObject.SetActive(!isFriend);

            // Solo puede retar si son amigos
            if (challengeButton != null)
                challengeButton.gameObject.SetActive(isFriend);
        }

        private void UpdateUI()
        {
            if (currentPlayerData == null)
            {
                Debug.LogWarning("[Profile] No hay datos del jugador");
                if (usernameText != null) usernameText.text = "Sin Usuario";
                return;
            }

            // Info basica
            if (usernameText != null)
                usernameText.text = currentPlayerData.username ?? "Sin Usuario";

            // Estadisticas generales
            if (totalGamesText != null)
                totalGamesText.text = $"{currentPlayerData.totalGamesPlayed}";

            if (winsText != null)
                winsText.text = $"{currentPlayerData.totalGamesWon}";

            if (winRateText != null)
                winRateText.text = $"{currentPlayerData.GetWinRate():F1}%";

            // Tiempos generales
            if (bestTimeText != null)
            {
                string bestTimeStr = currentPlayerData.bestTime < float.MaxValue
                    ? $"{currentPlayerData.bestTime:F2}s"
                    : "--";
                bestTimeText.text = bestTimeStr;
            }

            if (averageTimeText != null)
            {
                string avgTimeStr = currentPlayerData.averageTime > 0
                    ? $"{currentPlayerData.averageTime:F2}s"
                    : "--";
                averageTimeText.text = avgTimeStr;
            }

            // Stats por juego
            UpdateGameStats();

            Debug.Log($"[Profile] UI actualizada para {currentPlayerData.username}");
        }

        private void UpdateGameStats()
        {
            // Digit Rush
            if (digitRushValueText != null)
            {
                var stats = currentPlayerData.digitRushStats;
                digitRushValueText.text = stats != null
                    ? $"{stats.GetBestTimeFormatted()} | {stats.GetWinRate():F0}%"
                    : "-- | 0%";
            }

            // Memory Pairs
            if (memoryPairsValueText != null)
            {
                var stats = currentPlayerData.memoryPairsStats;
                memoryPairsValueText.text = stats != null
                    ? $"{stats.GetBestTimeFormatted()} | {stats.GetWinRate():F0}%"
                    : "-- | 0%";
            }

            // Quick Math
            if (quickMathValueText != null)
            {
                var stats = currentPlayerData.quickMathStats;
                quickMathValueText.text = stats != null
                    ? $"{stats.GetBestTimeFormatted()} | {stats.GetWinRate():F0}%"
                    : "-- | 0%";
            }

            // Flash Tap
            if (flashTapValueText != null)
            {
                var stats = currentPlayerData.flashTapStats;
                flashTapValueText.text = stats != null
                    ? $"{stats.GetBestTimeFormatted()} | {stats.GetWinRate():F0}%"
                    : "-- | 0%";
            }

            // Odd One Out
            if (oddOneOutValueText != null)
            {
                var stats = currentPlayerData.oddOneOutStats;
                oddOneOutValueText.text = stats != null
                    ? $"{stats.GetBestTimeFormatted()} | {stats.GetWinRate():F0}%"
                    : "-- | 0%";
            }
        }

        #region Button Callbacks

        private void OnFriendsClicked()
        {
            Debug.Log("[Profile] Abriendo lista de amigos");
            // TODO: Mostrar panel de amigos o navegar a escena Friends
        }

        private void OnHistoryClicked()
        {
            Debug.Log("[Profile] Abriendo historial de partidas");
            // TODO: Mostrar panel de historial
        }

        private void OnEditProfileClicked()
        {
            Debug.Log("[Profile] Editando perfil");
            // TODO: Mostrar panel de edicion de perfil
        }

        private void OnBackClicked()
        {
            Debug.Log("[Profile] Volviendo al Main Menu");
            SceneManager.LoadScene("MainMenu");
        }

        private async void OnAddFriendClicked()
        {
            if (string.IsNullOrEmpty(viewingPlayerId))
            {
                Debug.LogWarning("[Profile] No hay jugador para agregar");
                return;
            }

            Debug.Log($"[Profile] Agregando amigo: {viewingPlayerId}");

            // TODO: Enviar solicitud de amistad via Firebase
            // await DatabaseService.Instance.SendFriendRequest(viewingPlayerId);

            if (addFriendButton != null)
            {
                addFriendButton.interactable = false;
                var buttonText = addFriendButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                    buttonText.text = "Solicitud enviada";
            }
        }

        private void OnChallengeClicked()
        {
            if (string.IsNullOrEmpty(viewingPlayerId))
            {
                Debug.LogWarning("[Profile] No hay jugador para retar");
                return;
            }

            Debug.Log($"[Profile] Retando a: {viewingPlayerId}");

            // Guardar el ID del jugador a retar
            PlayerPrefs.SetString("ChallengePlayerId", viewingPlayerId);
            PlayerPrefs.Save();

            // TODO: Navegar a seleccion de juego para reto
            // SceneManager.LoadScene("ChallengeSetup");
        }

        #endregion
    }
}
