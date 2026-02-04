using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DigitPark.Services;
using DigitPark.Services.Firebase;
using DigitPark.Data;
using DigitPark.UI.Components;
using DG.Tweening;
using DigitPark.Animations;
using System.Collections.Generic;

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
        [SerializeField] private AvatarUI avatarUI;
        [SerializeField] private Button editAvatarButton;
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
        private string returnScene = "MainMenu"; // Escena a la que volver

        #region Unity Lifecycle

        private void Start()
        {
            Debug.Log("[Profile] ProfileManager iniciado");

            SetupListeners();
            HideGameSelectionPanel();

            // Verificar de qué escena venimos para el botón Back
            returnScene = PlayerPrefs.GetString("ProfileReturnScene", "MainMenu");
            PlayerPrefs.DeleteKey("ProfileReturnScene");

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

            // Avatar edit
            editAvatarButton?.onClick.AddListener(OnEditAvatarClicked);

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

            // Avatar change events
            if (AvatarService.Instance != null)
            {
                AvatarService.Instance.OnAvatarChanged += OnAvatarChanged;
            }
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

            // Mostrar boton editar avatar (solo en perfil propio)
            if (editAvatarButton != null)
                editAvatarButton.gameObject.SetActive(true);

            // Cargar avatar del usuario actual
            LoadAvatar();

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

            // Ocultar boton editar avatar (no es nuestro perfil)
            if (editAvatarButton != null)
                editAvatarButton.gameObject.SetActive(false);

            // Cargar avatar del otro jugador
            LoadAvatar();

            // Verificar estado de amistad
            CheckFriendStatus(playerId);

            UpdateUI();
        }

        private void CheckFriendStatus(string playerId)
        {
            // Usar FriendService para verificar estado
            isFriend = FriendService.Instance.IsFriend(playerId);
            bool hasPendingRequest = FriendService.Instance.HasPendingRequestWith(playerId);
            bool sentRequest = FriendService.Instance.HasSentRequestTo(playerId);

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
                if (hasPendingRequest)
                {
                    // Hay solicitud pendiente
                    if (sentRequest)
                    {
                        SetStatusText("Solicitud enviada", new Color32(255, 204, 0, 255)); // Amarillo
                        if (addFriendIconButton != null)
                        {
                            addFriendIconButton.gameObject.SetActive(true);
                            addFriendIconButton.interactable = false;
                            var image = addFriendIconButton.GetComponent<Image>();
                            if (image != null) image.color = new Color32(136, 136, 136, 255);
                        }
                    }
                    else
                    {
                        // Recibimos solicitud de este jugador
                        SetStatusText("Te envio solicitud", new Color32(0, 255, 136, 255)); // Verde
                        if (addFriendIconButton != null)
                        {
                            addFriendIconButton.gameObject.SetActive(true);
                            addFriendIconButton.interactable = true;
                            // Cambiar texto del boton a "Aceptar"
                        }
                    }
                }
                else
                {
                    SetStatusText("No es amigo", new Color32(136, 136, 136, 255)); // Gris
                    if (addFriendIconButton != null)
                    {
                        addFriendIconButton.gameObject.SetActive(true);
                        addFriendIconButton.interactable = true;
                    }
                }

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

            // Estadisticas generales con animación
            AnimateGeneralStats();

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

        private void AnimateGeneralStats()
        {
            if (currentPlayerData == null) return;

            int totalGames = currentPlayerData.totalGamesPlayed;
            int wins = currentPlayerData.totalGamesWon;
            float winRate = currentPlayerData.GetWinRate();

            // Counter animation para total de juegos
            if (totalGamesText != null)
            {
                totalGamesText.text = "0";
                UIAnimations.CounterAnimation(totalGamesText, 0, totalGames, 0.8f);
            }

            // Counter animation para victorias
            if (winsText != null)
            {
                winsText.text = "0";
                UIAnimations.CounterAnimation(winsText, 0, wins, 0.8f);
            }

            // Animación para win rate (float con decimal)
            if (winRateText != null)
            {
                winRateText.text = "0.0%";
                float val = 0f;
                DOTween.To(() => val, x => {
                    val = x;
                    winRateText.text = $"{x:F1}%";
                }, winRate, 0.8f).SetEase(Ease.OutQuad);
            }

            // Best time y average time (valores directos, sin counter)
            if (bestTimeText != null)
            {
                bestTimeText.text = currentPlayerData.bestTime < float.MaxValue
                    ? $"{currentPlayerData.bestTime:F2}s"
                    : "--";
            }

            if (averageTimeText != null)
            {
                averageTimeText.text = currentPlayerData.averageTime > 0
                    ? $"{currentPlayerData.averageTime:F2}s"
                    : "--";
            }
        }

        #endregion

        #region Avatar

        private async void LoadAvatar()
        {
            if (currentPlayerData == null) return;

            // Si hay AvatarUI component, usarlo (ya maneja todo el flujo)
            if (avatarUI != null)
            {
                if (isOwnProfile)
                {
                    await avatarUI.LoadCurrentUserAvatar();
                }
                else
                {
                    await avatarUI.LoadUserAvatar(
                        currentPlayerData.userId,
                        currentPlayerData.avatarUrl,
                        currentPlayerData.username
                    );
                }
                return;
            }

            // Fallback: cargar directamente en el Image
            if (avatarImage != null)
            {
                if (AvatarService.Instance != null)
                {
                    try
                    {
                        Sprite avatar;
                        if (isOwnProfile)
                        {
                            avatar = await AvatarService.Instance.LoadCurrentUserAvatar();
                        }
                        else
                        {
                            avatar = await AvatarService.Instance.LoadAvatar(
                                currentPlayerData.userId,
                                currentPlayerData.avatarUrl,
                                currentPlayerData.username
                            );
                        }
                        if (avatar != null)
                        {
                            avatarImage.sprite = avatar;
                            avatarImage.color = Color.white;
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"[Profile] Error cargando avatar: {e.Message}");
                        // Generar avatar con inicial como fallback
                        Sprite initialAvatar = AvatarInitialGenerator.GenerateAvatar(
                            currentPlayerData.username, currentPlayerData.userId);
                        avatarImage.sprite = initialAvatar;
                        avatarImage.color = Color.white;
                    }
                }
                else
                {
                    // Sin AvatarService, generar avatar con inicial
                    Sprite initialAvatar = AvatarInitialGenerator.GenerateAvatar(
                        currentPlayerData.username, currentPlayerData.userId);
                    avatarImage.sprite = initialAvatar;
                    avatarImage.color = Color.white;
                }
            }
        }

        private void OnEditAvatarClicked()
        {
            if (!isOwnProfile) return;

            // Si hay AvatarUI con edición, ya lo maneja
            if (avatarUI != null && avatarUI.isActiveAndEnabled)
            {
                // AvatarUI ya tiene su propio editButton, pero permitimos trigger externo
            }

            // Abrir selector de galería directamente
            if (AvatarService.Instance != null)
            {
                AvatarService.Instance.PickAvatarFromGallery();
            }
            else
            {
                Debug.LogWarning("[Profile] AvatarService no disponible para editar avatar");
            }
        }

        private void OnAvatarChanged(Sprite newAvatar)
        {
            // Actualizar avatar si estamos viendo nuestro propio perfil
            if (isOwnProfile && avatarImage != null && newAvatar != null)
            {
                avatarImage.sprite = newAvatar;
                avatarImage.color = Color.white;
            }
        }

        private void OnDestroy()
        {
            if (AvatarService.Instance != null)
            {
                AvatarService.Instance.OnAvatarChanged -= OnAvatarChanged;
            }
        }

        #endregion

        #region Button Callbacks

        private void OnBackClicked()
        {
            Debug.Log($"[Profile] Volviendo a: {returnScene}");
            SceneManager.LoadScene(returnScene);
        }

        private void OnFriendsClicked()
        {
            Debug.Log("[Profile] Navegando a escena de amigos");

            PlayerPrefs.SetString("FriendsReturnScene", "Profile");
            PlayerPrefs.Save();
            SceneManager.LoadScene("Friends");
        }

        private void OnHistoryClicked()
        {
            Debug.Log("[Profile] Abriendo historial de partidas");

            PlayerPrefs.SetString("MatchHistoryReturnScene", "Profile");
            PlayerPrefs.Save();
            SceneManager.LoadScene("MatchHistory");
        }

        private async void OnAddFriendClicked()
        {
            if (string.IsNullOrEmpty(viewingPlayerId))
            {
                Debug.LogWarning("[Profile] No hay jugador para agregar");
                return;
            }

            // Verificar si ya hay solicitud pendiente
            if (FriendService.Instance.HasPendingRequestWith(viewingPlayerId))
            {
                Debug.Log("[Profile] Ya existe solicitud pendiente");
                SetStatusText("Solicitud pendiente", new Color32(255, 204, 0, 255));
                return;
            }

            Debug.Log($"[Profile] Enviando solicitud de amistad a: {viewingPlayerId}");

            // Enviar solicitud usando FriendService
            var result = await FriendService.Instance.SendFriendRequest(viewingPlayerId);

            if (result.Success)
            {
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
            else
            {
                Debug.LogWarning($"[Profile] Error al enviar solicitud: {result.Message}");
                SetStatusText(result.Message, new Color32(255, 100, 100, 255)); // Rojo
            }
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
