using System;
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
using System.Threading.Tasks;
using DigitPark.Localization;
using DigitPark.UI.Panels;
using DigitPark.Monetization;

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
        [SerializeField] private Button editNameButton;
        [SerializeField] private TextMeshProUGUI statusText;  // "Tu perfil", "Amigo", "No es amigo"

        [Header("UI - Change Name")]
        [SerializeField] private InputPanelUI changeNamePanel;
        [SerializeField] private ErrorPanelUI errorPanel;

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

        [Header("UI - Game Stats Bars")]
        [SerializeField] private Image digitRushBarFill;
        [SerializeField] private Image memoryPairsBarFill;
        [SerializeField] private Image quickMathBarFill;
        [SerializeField] private Image flashTapBarFill;
        [SerializeField] private Image oddOneOutBarFill;

        [Header("UI - Action Buttons (Centrados)")]
        [SerializeField] private Button friendsButton;    // Ver amigos (puede ocultarse por privacidad)
        [SerializeField] private Button historyButton;    // Historial (siempre visible)

        [Header("UI - CTA Button")]
        [SerializeField] private Button challengeButton;  // Retar (solo si es amigo)

        [Header("UI - Loading")]
        [SerializeField] private GameObject loadingIndicator;  // PR-06: shown while loading another player's profile

        [Header("UI - Challenge Game Selection")]
        [SerializeField] private GameObject gameSelectionPanel;  // Panel para elegir juego
        [SerializeField] private Button darkOverlayButton;       // Para cerrar al tocar fuera
        [SerializeField] private Button cancelButton;            // Botón cancelar
        [SerializeField] private Button digitRushButton;
        [SerializeField] private Button memoryPairsButton;
        [SerializeField] private Button quickMathButton;
        [SerializeField] private Button flashTapButton;
        [SerializeField] private Button oddOneOutButton;

        // Name change (same logic as Settings)
        private const string NAME_CHANGE_COUNT_KEY = "NameChangeCount";
        private const int NAME_CHANGE_GEM_COST = 100;

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
            SetLoadingVisible(false);  // PR-06: ensure hidden on start

            // Verificar de qué escena venimos para el botón Back
            returnScene = PlayerPrefs.GetString("DP_ProfileReturnScene", "MainMenu");
            PlayerPrefs.DeleteKey("DP_ProfileReturnScene");

            // Verificar si venimos a ver el perfil de otro jugador
            string viewProfileId = PlayerPrefs.GetString("DP_ViewProfileId", "");
            if (!string.IsNullOrEmpty(viewProfileId))
            {
                PlayerPrefs.DeleteKey("DP_ViewProfileId");
                LoadProfileData(viewProfileId);
            }
            else
            {
                LoadProfileData();
            }
        }

        private void SetupListeners()
        {
            // Header - disable auto-navigation from BackButton prefab to prevent double listener
            var autoNav = backButton?.GetComponent<DigitPark.UI.BackButton>();
            if (autoNav != null) autoNav.DisableAutoNavigation();
            backButton?.onClick.AddListener(OnBackClicked);
            addFriendIconButton?.onClick.AddListener(OnAddFriendClicked);

            // Edit buttons (solo perfil propio)
            editNameButton?.onClick.AddListener(OnEditNameClicked);

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

            // Avatar system removed
        }

        private void Update()
        {
            // PR-05: Escape key / Android back button closes the scene
            if (Input.GetKeyDown(KeyCode.Escape))
                OnBackClicked();
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
                _ = LoadOtherProfileAsync(playerId).ContinueWith(t =>
                {
                    if (t.IsFaulted) Debug.LogError($"[ProfileManager] LoadOtherProfileAsync failed: {t.Exception?.GetBaseException().Message}");
                }, System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());
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
            SetStatusText(AutoLocalizer.Get("profile_own_profile"), new Color32(0, 255, 255, 255)); // Cyan

            // Ocultar botones de otros perfiles
            if (addFriendIconButton != null)
                addFriendIconButton.gameObject.SetActive(false);

            if (challengeButton != null)
                challengeButton.gameObject.SetActive(false);

            // Mostrar botones de edicion (solo en perfil propio)
            if (editNameButton != null)
                editNameButton.gameObject.SetActive(true);

            // Mostrar botones de accion
            if (friendsButton != null)
                friendsButton.gameObject.SetActive(true);

            if (historyButton != null)
                historyButton.gameObject.SetActive(true);

            UpdateUI();
        }

        private async Task LoadOtherProfileAsync(string playerId)
        {
            Debug.Log($"[Profile] Cargando perfil de: {playerId}");

            SetLoadingVisible(true);  // PR-06

            if (DatabaseService.Instance != null)
            {
                currentPlayerData = await DatabaseService.Instance.GetPlayerDataById(playerId);
            }

            if (this == null) return;

            SetLoadingVisible(false);  // PR-06

            // Ocultar botones de edicion (no es nuestro perfil)
            if (editNameButton != null)
                editNameButton.gameObject.SetActive(false);

            // Verificar estado de amistad
            CheckFriendStatus(playerId);

            UpdateUI();
        }

        private void CheckFriendStatus(string playerId)
        {
            var friendService = FriendService.Instance;
            if (friendService == null)
            {
                Debug.LogWarning("[ProfileManager] FriendService not available — skipping friend status check");
                return;
            }

            // Usar FriendService para verificar estado
            isFriend = friendService.IsFriend(playerId);
            bool hasPendingRequest = friendService.HasPendingRequestWith(playerId);
            bool sentRequest = friendService.HasSentRequestTo(playerId);

            if (isFriend)
            {
                // ES AMIGO
                SetStatusText(AutoLocalizer.Get("profile_friend_status"), new Color32(0, 255, 136, 255)); // Verde

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
                        SetStatusText(AutoLocalizer.Get("profile_request_sent"), new Color32(255, 204, 0, 255)); // Amarillo
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
                        SetStatusText(AutoLocalizer.Get("profile_received_request"), new Color32(0, 255, 136, 255)); // Verde
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
                    SetStatusText(AutoLocalizer.Get("profile_not_friend"), new Color32(136, 136, 136, 255)); // Gris
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
                if (usernameText != null) usernameText.text = AutoLocalizer.Get("profile_no_user");
                return;
            }

            // Info basica
            if (usernameText != null)
                usernameText.text = DigitPark.UI.UICanvasHelper.TmpSafe(currentPlayerData.username) ?? AutoLocalizer.Get("profile_no_user");

            // Estadisticas generales con animación
            AnimateGeneralStats();

            UpdateGameStats();

            Debug.Log($"[Profile] UI actualizada para {currentPlayerData.username}");
        }

        private void UpdateGameStats()
        {
            UpdateSingleGameStat(currentPlayerData.digitRushStats, digitRushValueText, digitRushBarFill);
            UpdateSingleGameStat(currentPlayerData.memoryPairsStats, memoryPairsValueText, memoryPairsBarFill);
            UpdateSingleGameStat(currentPlayerData.quickMathStats, quickMathValueText, quickMathBarFill);
            UpdateSingleGameStat(currentPlayerData.flashTapStats, flashTapValueText, flashTapBarFill);
            UpdateSingleGameStat(currentPlayerData.oddOneOutStats, oddOneOutValueText, oddOneOutBarFill);
        }

        private void UpdateSingleGameStat(GameStats stats, TextMeshProUGUI valueText, Image barFill)
        {
            float winRate = 0f;
            if (valueText != null)
            {
                if (stats != null)
                {
                    winRate = stats.GetWinRate();
                    valueText.text = $"{stats.GetBestTimeFormatted()} | {winRate:F0}%";
                }
                else
                {
                    valueText.text = AutoLocalizer.Get("stats_no_data");
                }
            }
            if (barFill != null)
                barFill.fillAmount = winRate / 100f;
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
                    if (winRateText != null) winRateText.text = $"{x:F1}%";
                }, winRate, 0.8f).SetEase(Ease.OutQuad).SetLink(gameObject);
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

        #region Cleanup

        private void OnDestroy()
        {
            DOTween.Kill(transform);

            // Remove button listeners to prevent leaks
            backButton?.onClick.RemoveAllListeners();
            addFriendIconButton?.onClick.RemoveAllListeners();
            editNameButton?.onClick.RemoveAllListeners();
            friendsButton?.onClick.RemoveAllListeners();
            historyButton?.onClick.RemoveAllListeners();
            challengeButton?.onClick.RemoveAllListeners();
            darkOverlayButton?.onClick.RemoveAllListeners();
            cancelButton?.onClick.RemoveAllListeners();
            digitRushButton?.onClick.RemoveAllListeners();
            memoryPairsButton?.onClick.RemoveAllListeners();
            quickMathButton?.onClick.RemoveAllListeners();
            flashTapButton?.onClick.RemoveAllListeners();
            oddOneOutButton?.onClick.RemoveAllListeners();
        }

        #endregion

        #region Button Callbacks

        private void SetLoadingVisible(bool visible)
        {
            if (loadingIndicator != null)
                loadingIndicator.SetActive(visible);
        }

        private void OnBackClicked()
        {
            Debug.Log($"[Profile] Volviendo a: {returnScene}");
            SceneManager.LoadScene(returnScene);
        }

        private void OnFriendsClicked()
        {
            Debug.Log("[Profile] Navegando a escena de amigos");

            PlayerPrefs.SetString("DP_FriendsReturnScene", "Profile");
            PlayerPrefs.Save();
            SceneManager.LoadScene("Friends");
        }

        private void OnHistoryClicked()
        {
            Debug.Log("[Profile] Abriendo historial de partidas");

            PlayerPrefs.SetString("DP_MatchHistoryReturnScene", "Profile");
            PlayerPrefs.Save();
            SceneManager.LoadScene("MatchHistory");
        }

        private async void OnAddFriendClicked()
        {
            try
            {
                if (string.IsNullOrEmpty(viewingPlayerId))
                {
                    Debug.LogWarning("[Profile] No hay jugador para agregar");
                    return;
                }

                // Verificar si ya hay solicitud pendiente
                if (FriendService.Instance != null && FriendService.Instance.HasPendingRequestWith(viewingPlayerId))
                {
                    Debug.Log("[Profile] Ya existe solicitud pendiente");
                    SetStatusText(AutoLocalizer.Get("profile_request_pending"), new Color32(255, 204, 0, 255));
                    return;
                }

                Debug.Log($"[Profile] Enviando solicitud de amistad a: {viewingPlayerId}");

                // Enviar solicitud usando FriendService
                if (FriendService.Instance == null) { Debug.LogWarning("[Profile] FriendService not available"); return; }
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
                    SetStatusText(AutoLocalizer.Get("profile_request_sent"), new Color32(255, 204, 0, 255)); // Amarillo
                }
                else
                {
                    Debug.LogWarning($"[Profile] Error al enviar solicitud: {result.Message}");
                    SetStatusText(result.Message, new Color32(255, 100, 100, 255)); // Rojo
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ProfileManager] {ex.Message}");
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
            PlayerPrefs.SetString("DP_ChallengePlayerId", viewingPlayerId);
            PlayerPrefs.SetString("DP_ChallengeGameName", gameName);
            PlayerPrefs.Save();

            HideGameSelectionPanel();

            InAppNotificationManager.Instance?.Show(AutoLocalizer.Get("feature_coming_soon"), "", "info");
        }

        public void OnGameSelectionCancelled()
        {
            Debug.Log("[Profile] Seleccion de juego cancelada");
            HideGameSelectionPanel();
        }

        #endregion

        #region Change Name

        private void OnEditNameClicked()
        {
            if (!isOwnProfile) return;

            int changeCount = PlayerPrefs.GetInt(NAME_CHANGE_COUNT_KEY, 0);

            // After first free change, check gems
            if (changeCount > 0)
            {
                int currentGems = CurrencyManager.Instance?.Gems ?? 0;
                if (currentGems < NAME_CHANGE_GEM_COST)
                {
                    errorPanel?.Show(AutoLocalizer.Get("not_enough_gems_name_change", NAME_CHANGE_GEM_COST));
                    return;
                }
            }

            if (changeNamePanel != null)
            {
                changeNamePanel.SetLengthLimits(3, 20);
                changeNamePanel.Show(
                    changeCount == 0
                        ? AutoLocalizer.Get("change_name_title")
                        : AutoLocalizer.Get("change_name_title_cost", NAME_CHANGE_GEM_COST),
                    AutoLocalizer.Get("new_name_placeholder"),
                    OnConfirmNameChange,
                    null
                );
            }
        }

        private async void OnConfirmNameChange(string newUsername)
        {
            try
            {
                if (currentPlayerData == null) return;
                if (newUsername == currentPlayerData.username)
                {
                    changeNamePanel?.Hide();
                    return;
                }

                int changeCount = PlayerPrefs.GetInt(NAME_CHANGE_COUNT_KEY, 0);

                // Deduct gems if not first change
                if (changeCount > 0)
                {
                    bool spent = CurrencyManager.Instance?.SpendGems(NAME_CHANGE_GEM_COST) ?? false;
                    if (!spent)
                    {
                        errorPanel?.Show(AutoLocalizer.Get("not_enough_gems_name_change", NAME_CHANGE_GEM_COST));
                        changeNamePanel?.SetButtonsInteractable(true);
                        return;
                    }
                }

                Debug.Log($"[Profile] Cambiando nombre a: {newUsername}");

                if (AuthenticationService.Instance == null) { Debug.LogError("[Profile] AuthService not available"); return; }
                bool success = await AuthenticationService.Instance.UpdateUsername(newUsername);

                if (success)
                {
                    Debug.Log("[Profile] Nombre actualizado exitosamente");
                    currentPlayerData.username = newUsername;
                    PlayerPrefs.SetInt(NAME_CHANGE_COUNT_KEY, changeCount + 1);
                    PlayerPrefs.SetString("DP_DisplayName", newUsername);
                    PlayerPrefs.Save();
                    changeNamePanel?.Hide();

                    // Actualizar UI inmediatamente
                    if (usernameText != null)
                        usernameText.text = newUsername;

                    // Sync nombre a Firebase (leaderboards + perfil)
                    try
                    {
                        var profilePlayerData = AuthenticationService.Instance?.GetCurrentPlayerData();
                        if (DatabaseService.Instance != null && profilePlayerData != null)
                            await DatabaseService.Instance.UpdatePlayerFields(profilePlayerData.userId, new System.Collections.Generic.Dictionary<string, object>
                            {
                                { "username", newUsername }
                            });
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"[Profile] Firebase name sync failed: {e.Message}");
                    }
                }
                else
                {
                    Debug.LogError("[Profile] Error al actualizar nombre");
                    changeNamePanel?.SetButtonsInteractable(true);
                    errorPanel?.Show(AutoLocalizer.Get("error_changing_name"));
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ProfileManager] {ex.Message}");
            }
        }

        #endregion
    }
}
