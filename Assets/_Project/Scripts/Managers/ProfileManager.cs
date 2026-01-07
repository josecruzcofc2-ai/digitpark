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
    ///
    /// NUEVO DISEÑO:
    /// - AddFriendButton: Icono en esquina superior derecha (solo si NO es amigo)
    /// - FriendsButton + HistoryButton: Centrados en HorizontalLayoutGroup
    /// - ChallengeButton: CTA grande abajo (solo si ES amigo)
    /// </summary>
    public class ProfileManager : MonoBehaviour
    {
        [Header("UI - Header")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button addFriendIconButton;  // Icono esquina superior derecha

        [Header("UI - Profile Info")]
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private Image avatarImage;
        [SerializeField] private TextMeshProUGUI statusText;  // "Tu perfil", "Amigo", "No es amigo"

        [Header("UI - General Stats")]
        [SerializeField] private TextMeshProUGUI totalGamesText;
        [SerializeField] private TextMeshProUGUI winsText;
        [SerializeField] private TextMeshProUGUI winRateText;
        [SerializeField] private TextMeshProUGUI bestTimeText;
        [SerializeField] private TextMeshProUGUI averageTimeText;

        [Header("UI - Game Stats Values")]
        [SerializeField] private TextMeshProUGUI digitRushValueText;
        [SerializeField] private TextMeshProUGUI memoryPairsValueText;
        [SerializeField] private TextMeshProUGUI quickMathValueText;
        [SerializeField] private TextMeshProUGUI flashTapValueText;
        [SerializeField] private TextMeshProUGUI oddOneOutValueText;

        [Header("UI - Action Buttons (Centrados)")]
        [SerializeField] private Button friendsButton;    // Ver amigos (puede ocultarse por privacidad)
        [SerializeField] private Button historyButton;    // Historial (siempre visible)

        [Header("UI - CTA Button")]
        [SerializeField] private Button challengeButton;  // Retar (solo si es amigo)

        [Header("UI - Challenge Game Selection")]
        [SerializeField] private GameObject gameSelectionPanel;  // Panel para elegir juego
        [SerializeField] private Button darkOverlayButton;       // Para cerrar al tocar fuera
        [SerializeField] private Button cancelButton;            // Botón cancelar
        [SerializeField] private Button digitRushButton;
        [SerializeField] private Button memoryPairsButton;
        [SerializeField] private Button quickMathButton;
        [SerializeField] private Button flashTapButton;
        [SerializeField] private Button oddOneOutButton;

        // Estado
        private PlayerData currentPlayerData;
        private string viewingPlayerId;
        private bool isOwnProfile = true;
        private bool isFriend = false;

        #region Unity Lifecycle

        private void Start()
        {
            Debug.Log("[Profile] ProfileManager iniciado");

            SetupListeners();
            HideGameSelectionPanel();

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
            // Header
            backButton?.onClick.AddListener(OnBackClicked);
            addFriendIconButton?.onClick.AddListener(OnAddFriendClicked);

            // Action Buttons
            friendsButton?.onClick.AddListener(OnFriendsClicked);
            historyButton?.onClick.AddListener(OnHistoryClicked);

            // CTA
            challengeButton?.onClick.AddListener(OnChallengeClicked);

            // Game Selection Panel
            darkOverlayButton?.onClick.AddListener(OnGameSelectionCancelled);
            cancelButton?.onClick.AddListener(OnGameSelectionCancelled);
            digitRushButton?.onClick.AddListener(() => OnGameSelected("DigitRush"));
            memoryPairsButton?.onClick.AddListener(() => OnGameSelected("MemoryPairs"));
            quickMathButton?.onClick.AddListener(() => OnGameSelected("QuickMath"));
            flashTapButton?.onClick.AddListener(() => OnGameSelected("FlashTap"));
            oddOneOutButton?.onClick.AddListener(() => OnGameSelected("OddOneOut"));
        }

        #endregion

        #region Load Profile

        public void LoadProfileData(string playerId = null)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                isOwnProfile = true;
                LoadOwnProfile();
            }
            else
            {
                isOwnProfile = false;
                viewingPlayerId = playerId;
                LoadOtherProfile(playerId);
            }
        }

        private void LoadOwnProfile()
        {
            Debug.Log("[Profile] Cargando perfil propio");

            if (AuthenticationService.Instance != null)
            {
                currentPlayerData = AuthenticationService.Instance.GetCurrentPlayerData();
            }

            // UI para perfil propio
            SetStatusText("Tu perfil", new Color32(0, 255, 255, 255)); // Cyan

            // Ocultar botones de otros perfiles
            if (addFriendIconButton != null)
                addFriendIconButton.gameObject.SetActive(false);

            if (challengeButton != null)
                challengeButton.gameObject.SetActive(false);

            // Mostrar botones de accion
            if (friendsButton != null)
                friendsButton.gameObject.SetActive(true);

            if (historyButton != null)
                historyButton.gameObject.SetActive(true);

            UpdateUI();
        }

        private async void LoadOtherProfile(string playerId)
        {
            Debug.Log($"[Profile] Cargando perfil de: {playerId}");

            if (DatabaseService.Instance != null)
            {
                currentPlayerData = await DatabaseService.Instance.GetPlayerDataById(playerId);
            }

            // Verificar estado de amistad
            CheckFriendStatus(playerId);

            UpdateUI();
        }

        private void CheckFriendStatus(string playerId)
        {
            PlayerData myData = null;
            if (AuthenticationService.Instance != null)
            {
                myData = AuthenticationService.Instance.GetCurrentPlayerData();
            }

            isFriend = myData != null && myData.IsFriend(playerId);

            if (isFriend)
            {
                // ES AMIGO
                SetStatusText("Amigo", new Color32(0, 255, 136, 255)); // Verde

                // Ocultar agregar amigo, mostrar retar
                if (addFriendIconButton != null)
                    addFriendIconButton.gameObject.SetActive(false);

                if (challengeButton != null)
                    challengeButton.gameObject.SetActive(true);

                // Mostrar amigos (si la privacidad lo permite) e historial
                if (friendsButton != null)
                    friendsButton.gameObject.SetActive(true); // TODO: Verificar privacidad

                if (historyButton != null)
                    historyButton.gameObject.SetActive(true);
            }
            else
            {
                // NO ES AMIGO
                SetStatusText("No es amigo", new Color32(136, 136, 136, 255)); // Gris

                // Mostrar agregar amigo, ocultar retar
                if (addFriendIconButton != null)
                    addFriendIconButton.gameObject.SetActive(true);

                if (challengeButton != null)
                    challengeButton.gameObject.SetActive(false);

                // Ocultar amigos (no es amigo), mostrar historial
                if (friendsButton != null)
                    friendsButton.gameObject.SetActive(false);

                if (historyButton != null)
                    historyButton.gameObject.SetActive(true);
            }
        }

        private void SetStatusText(string text, Color32 color)
        {
            if (statusText != null)
            {
                statusText.text = text;
                statusText.color = color;
                statusText.gameObject.SetActive(true);
            }
        }

        #endregion

        #region Update UI

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

            UpdateGameStats();

            Debug.Log($"[Profile] UI actualizada para {currentPlayerData.username}");
        }

        private void UpdateGameStats()
        {
            if (digitRushValueText != null)
            {
                var stats = currentPlayerData.digitRushStats;
                digitRushValueText.text = stats != null
                    ? $"{stats.GetBestTimeFormatted()} | {stats.GetWinRate():F0}%"
                    : "-- | 0%";
            }

            if (memoryPairsValueText != null)
            {
                var stats = currentPlayerData.memoryPairsStats;
                memoryPairsValueText.text = stats != null
                    ? $"{stats.GetBestTimeFormatted()} | {stats.GetWinRate():F0}%"
                    : "-- | 0%";
            }

            if (quickMathValueText != null)
            {
                var stats = currentPlayerData.quickMathStats;
                quickMathValueText.text = stats != null
                    ? $"{stats.GetBestTimeFormatted()} | {stats.GetWinRate():F0}%"
                    : "-- | 0%";
            }

            if (flashTapValueText != null)
            {
                var stats = currentPlayerData.flashTapStats;
                flashTapValueText.text = stats != null
                    ? $"{stats.GetBestTimeFormatted()} | {stats.GetWinRate():F0}%"
                    : "-- | 0%";
            }

            if (oddOneOutValueText != null)
            {
                var stats = currentPlayerData.oddOneOutStats;
                oddOneOutValueText.text = stats != null
                    ? $"{stats.GetBestTimeFormatted()} | {stats.GetWinRate():F0}%"
                    : "-- | 0%";
            }
        }

        #endregion

        #region Button Callbacks

        private void OnBackClicked()
        {
            Debug.Log("[Profile] Volviendo atras");
            SceneManager.LoadScene("MainMenu");
        }

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

        private async void OnAddFriendClicked()
        {
            if (string.IsNullOrEmpty(viewingPlayerId))
            {
                Debug.LogWarning("[Profile] No hay jugador para agregar");
                return;
            }

            Debug.Log($"[Profile] Enviando solicitud de amistad a: {viewingPlayerId}");

            // TODO: Enviar solicitud de amistad via Firebase
            // await DatabaseService.Instance.SendFriendRequest(viewingPlayerId);

            // Feedback visual - cambiar icono o desactivar
            if (addFriendIconButton != null)
            {
                addFriendIconButton.interactable = false;

                // Cambiar color a gris para indicar que ya se envio
                var image = addFriendIconButton.GetComponent<Image>();
                if (image != null)
                    image.color = new Color32(136, 136, 136, 255);
            }

            // Actualizar status
            SetStatusText("Solicitud enviada", new Color32(255, 204, 0, 255)); // Amarillo
        }

        private void OnChallengeClicked()
        {
            if (string.IsNullOrEmpty(viewingPlayerId))
            {
                Debug.LogWarning("[Profile] No hay jugador para retar");
                return;
            }

            Debug.Log($"[Profile] Abriendo seleccion de juego para retar a: {viewingPlayerId}");

            // Mostrar panel de seleccion de juego
            ShowGameSelectionPanel();
        }

        #endregion

        #region Game Selection Panel

        private void ShowGameSelectionPanel()
        {
            if (gameSelectionPanel != null)
                gameSelectionPanel.SetActive(true);
        }

        private void HideGameSelectionPanel()
        {
            if (gameSelectionPanel != null)
                gameSelectionPanel.SetActive(false);
        }

        // Llamado desde los botones del panel de seleccion de juego
        public void OnGameSelected(string gameName)
        {
            Debug.Log($"[Profile] Juego seleccionado para reto: {gameName}");

            // Guardar datos del reto
            PlayerPrefs.SetString("ChallengePlayerId", viewingPlayerId);
            PlayerPrefs.SetString("ChallengeGameName", gameName);
            PlayerPrefs.Save();

            HideGameSelectionPanel();

            // TODO: Crear la partida de reto y navegar
            // SceneManager.LoadScene("Games/" + gameName);
        }

        public void OnGameSelectionCancelled()
        {
            Debug.Log("[Profile] Seleccion de juego cancelada");
            HideGameSelectionPanel();
        }

        #endregion
    }
}
