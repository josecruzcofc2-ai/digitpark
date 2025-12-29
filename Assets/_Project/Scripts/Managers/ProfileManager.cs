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
    /// </summary>
    public class ProfileManager : MonoBehaviour
    {
        [Header("UI - Profile Info")]
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI totalGamesText;
        [SerializeField] private TextMeshProUGUI winsText;
        [SerializeField] private TextMeshProUGUI winRateText;
        [SerializeField] private TextMeshProUGUI coinsText;
        [SerializeField] private TextMeshProUGUI gemsText;
        [SerializeField] private Image avatarImage;

        [Header("UI - Stats Per Game")]
        [SerializeField] private TextMeshProUGUI digitRushBestText;
        [SerializeField] private TextMeshProUGUI memoryPairsBestText;
        [SerializeField] private TextMeshProUGUI quickMathBestText;
        [SerializeField] private TextMeshProUGUI flashTapBestText;
        [SerializeField] private TextMeshProUGUI oddOneOutBestText;
        [SerializeField] private TextMeshProUGUI bestTimeText;
        [SerializeField] private TextMeshProUGUI averageTimeText;

        [Header("UI - Navigation")]
        [SerializeField] private Button friendsButton;
        [SerializeField] private Button historyButton;
        [SerializeField] private Button achievementsButton;
        [SerializeField] private Button editProfileButton;
        [SerializeField] private Button backButton;

        [Header("UI - Other Player Profile")]
        [SerializeField] private GameObject addFriendButtonContainer;
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
            achievementsButton?.onClick.AddListener(OnAchievementsClicked);
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

            // Ocultar botones de agregar amigo en perfil propio
            if (addFriendButtonContainer != null)
                addFriendButtonContainer.SetActive(false);
            if (challengeButton != null)
                challengeButton.gameObject.SetActive(false);
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

            // Mostrar botones de agregar amigo en perfil de otros
            if (addFriendButtonContainer != null)
                addFriendButtonContainer.SetActive(true);
            if (challengeButton != null)
                challengeButton.gameObject.SetActive(true);
            if (editProfileButton != null)
                editProfileButton.gameObject.SetActive(false);

            // Verificar si ya es amigo
            CheckFriendStatus(playerId);

            UpdateUI();
        }

        private void CheckFriendStatus(string playerId)
        {
            // TODO: Verificar en Firebase si ya son amigos
            bool isFriend = false; // Placeholder

            if (friendStatusText != null)
            {
                friendStatusText.text = isFriend ? "Ya son amigos" : "";
                friendStatusText.gameObject.SetActive(isFriend);
            }

            if (addFriendButton != null)
                addFriendButton.gameObject.SetActive(!isFriend);
        }

        private void UpdateUI()
        {
            if (currentPlayerData == null)
            {
                Debug.LogWarning("[Profile] No hay datos del jugador");
                // Mostrar datos por defecto
                if (usernameText != null) usernameText.text = "Sin Usuario";
                if (levelText != null) levelText.text = "Nivel 1";
                return;
            }

            // Info basica
            if (usernameText != null)
                usernameText.text = currentPlayerData.username ?? "Sin Usuario";

            if (levelText != null)
                levelText.text = $"Nivel {currentPlayerData.level}";

            // Monedas
            if (coinsText != null)
                coinsText.text = $"{currentPlayerData.coins:N0}";

            if (gemsText != null)
                gemsText.text = $"{currentPlayerData.gems:N0}";

            // Estadisticas generales
            if (totalGamesText != null)
                totalGamesText.text = $"{currentPlayerData.totalGamesPlayed}";

            if (winsText != null)
                winsText.text = $"{currentPlayerData.totalGamesWon}";

            if (winRateText != null)
            {
                float winRate = currentPlayerData.totalGamesPlayed > 0
                    ? (float)currentPlayerData.totalGamesWon / currentPlayerData.totalGamesPlayed * 100f
                    : 0f;
                winRateText.text = $"{winRate:F1}%";
            }

            // Tiempos
            if (bestTimeText != null)
            {
                string bestTimeStr = currentPlayerData.bestTime < float.MaxValue
                    ? $"{currentPlayerData.bestTime:F2}s"
                    : "--";
                bestTimeText.text = $"Mejor: {bestTimeStr}";
            }

            if (averageTimeText != null)
            {
                string avgTimeStr = currentPlayerData.averageTime > 0
                    ? $"{currentPlayerData.averageTime:F2}s"
                    : "--";
                averageTimeText.text = $"Promedio: {avgTimeStr}";
            }

            // Best scores por juego
            UpdateGameStats();

            Debug.Log($"[Profile] UI actualizada para {currentPlayerData.username}");
        }

        private void UpdateGameStats()
        {
            // TODO: Cuando tengamos los 5 juegos, obtener mejores scores de cada uno
            // Por ahora mostramos placeholder
            if (digitRushBestText != null)
                digitRushBestText.text = "Best: --";

            if (memoryPairsBestText != null)
                memoryPairsBestText.text = "Best: --";

            if (quickMathBestText != null)
                quickMathBestText.text = "Best: --";

            if (flashTapBestText != null)
                flashTapBestText.text = "Best: --";

            if (oddOneOutBestText != null)
                oddOneOutBestText.text = "Best: --";
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

        private void OnAchievementsClicked()
        {
            Debug.Log("[Profile] Abriendo logros");
            // TODO: Mostrar panel de logros
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
