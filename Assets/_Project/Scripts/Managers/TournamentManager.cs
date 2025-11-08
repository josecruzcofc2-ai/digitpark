using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DigitPark.Services.Firebase;
using DigitPark.Data;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager de la escena de Torneos
    /// Gestiona creación, visualización y participación en torneos
    /// </summary>
    public class TournamentManager : MonoBehaviour
    {
        [Header("Tabs")]
        [SerializeField] public Button activeTournamentsTab;
        [SerializeField] public Button myTournamentsTab;
        [SerializeField] public Button createTournamentTab;

        [Header("Tournaments List")]
        [SerializeField] public Transform tournamentsContainer;
        [SerializeField] public GameObject tournamentCardPrefab;
        [SerializeField] public ScrollRect scrollRect;

        [Header("Create Tournament Panel")]
        [SerializeField] public GameObject createTournamentPanel;
        [SerializeField] public TMP_InputField tournamentNameInput;
        [SerializeField] public TMP_Dropdown durationDropdown;
        [SerializeField] public TMP_InputField entryFeeInput;
        [SerializeField] public TMP_InputField maxParticipantsInput;
        [SerializeField] public TMP_Dropdown regionDropdown;
        [SerializeField] public Button createButton;
        [SerializeField] public Button cancelCreateButton;

        [Header("Tournament Detail Panel")]
        [SerializeField] public GameObject tournamentDetailPanel;
        [SerializeField] public TextMeshProUGUI detailNameText;
        [SerializeField] public TextMeshProUGUI detailTimeRemainingText;
        [SerializeField] public TextMeshProUGUI detailPrizePoolText;
        [SerializeField] public TextMeshProUGUI detailParticipantsText;
        [SerializeField] public TextMeshProUGUI detailEntryFeeText;
        [SerializeField] public Transform detailLeaderboardContainer;
        [SerializeField] public GameObject detailLeaderboardEntryPrefab;
        [SerializeField] public Button joinTournamentButton;
        [SerializeField] public Button playTournamentButton;
        [SerializeField] public Button closeTournamentDetailButton;

        [Header("Loading")]
        [SerializeField] public GameObject loadingPanel;

        [Header("Navigation")]
        [SerializeField] public Button backButton;

        // Estado
        private TournamentView currentView = TournamentView.Active;
        private PlayerData currentPlayer;
        private List<TournamentData> activeTournaments = new List<TournamentData>();
        private List<TournamentData> myTournaments = new List<TournamentData>();
        private TournamentData selectedTournament;

        private void Start()
        {
            Debug.Log("[Tournament] TournamentManager iniciado");

            // Verificar e inicializar servicios si no existen (para testing directo)
            EnsureServicesExist();

            // Cargar datos del jugador
            LoadPlayerData();

            // Configurar listeners
            SetupListeners();

            // Mostrar vista inicial
            ShowView(TournamentView.Active);
        }

        /// <summary>
        /// Asegura que los servicios existan (para testing directo de escena)
        /// </summary>
        private void EnsureServicesExist()
        {
            if (AuthenticationService.Instance == null)
            {
                Debug.LogWarning("[Tournament] AuthenticationService no encontrado, creando instancia de respaldo...");
                GameObject authService = new GameObject("AuthenticationService");
                authService.AddComponent<AuthenticationService>();
            }

            if (DatabaseService.Instance == null)
            {
                Debug.LogWarning("[Tournament] DatabaseService no encontrado, creando instancia de respaldo...");
                GameObject dbService = new GameObject("DatabaseService");
                dbService.AddComponent<DatabaseService>();
            }
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
                    Debug.LogError("[Tournament] No hay datos del jugador");
                    SceneManager.LoadScene("Login");
                }
            }
        }

        /// <summary>
        /// Configura los listeners
        /// </summary>
        private void SetupListeners()
        {
            activeTournamentsTab?.onClick.AddListener(() => ShowView(TournamentView.Active));
            myTournamentsTab?.onClick.AddListener(() => ShowView(TournamentView.MyTournaments));
            createTournamentTab?.onClick.AddListener(() => ShowView(TournamentView.Create));

            createButton?.onClick.AddListener(OnCreateTournamentClicked);
            cancelCreateButton?.onClick.AddListener(() => ShowView(TournamentView.Active));

            joinTournamentButton?.onClick.AddListener(OnJoinTournamentClicked);
            playTournamentButton?.onClick.AddListener(OnPlayTournamentClicked);
            closeTournamentDetailButton?.onClick.AddListener(() => tournamentDetailPanel?.SetActive(false));

            backButton?.onClick.AddListener(OnBackButtonClicked);
        }

        #endregion

        #region Views

        /// <summary>
        /// Muestra la vista seleccionada
        /// </summary>
        private void ShowView(TournamentView view)
        {
            Debug.Log($"[Tournament] Mostrando vista: {view}");

            currentView = view;
            UpdateTabVisuals();

            // Ocultar todos los paneles
            createTournamentPanel?.SetActive(false);

            switch (view)
            {
                case TournamentView.Active:
                    LoadActiveTournaments();
                    break;

                case TournamentView.MyTournaments:
                    LoadMyTournaments();
                    break;

                case TournamentView.Create:
                    ShowCreateTournamentPanel();
                    break;
            }
        }

        /// <summary>
        /// Actualiza los visuales de las tabs
        /// </summary>
        private void UpdateTabVisuals()
        {
            SetTabButtonState(activeTournamentsTab, currentView == TournamentView.Active);
            SetTabButtonState(myTournamentsTab, currentView == TournamentView.MyTournaments);
            SetTabButtonState(createTournamentTab, currentView == TournamentView.Create);
        }

        /// <summary>
        /// Establece el estado visual de un botón de tab
        /// </summary>
        private void SetTabButtonState(Button button, bool isSelected)
        {
            if (button == null) return;

            ColorBlock colors = button.colors;
            colors.normalColor = isSelected ? new Color(0f, 0.83f, 1f) : Color.gray;
            button.colors = colors;
        }

        #endregion

        #region Load Tournaments

        /// <summary>
        /// Carga los torneos activos
        /// </summary>
        private async void LoadActiveTournaments()
        {
            ShowLoading(true);
            ClearTournamentsList();

            try
            {
                activeTournaments = await DatabaseService.Instance.GetActiveTournaments();

                Debug.Log($"[Tournament] {activeTournaments.Count} torneos activos cargados");

                DisplayTournaments(activeTournaments);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Tournament] Error al cargar torneos: {ex.Message}");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        /// <summary>
        /// Carga los torneos del jugador
        /// </summary>
        private void LoadMyTournaments()
        {
            ShowLoading(true);
            ClearTournamentsList();

            try
            {
                // Filtrar torneos en los que participa el jugador
                myTournaments = activeTournaments.FindAll(t => t.IsParticipating(currentPlayer.userId));

                Debug.Log($"[Tournament] {myTournaments.Count} torneos propios encontrados");

                DisplayTournaments(myTournaments);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Tournament] Error al cargar mis torneos: {ex.Message}");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        /// <summary>
        /// Muestra la lista de torneos
        /// </summary>
        private void DisplayTournaments(List<TournamentData> tournaments)
        {
            if (tournaments.Count == 0)
            {
                ShowEmptyMessage();
                return;
            }

            foreach (var tournament in tournaments)
            {
                CreateTournamentCard(tournament);
            }
        }

        /// <summary>
        /// Crea una card de torneo
        /// </summary>
        private void CreateTournamentCard(TournamentData tournament)
        {
            if (tournamentCardPrefab == null || tournamentsContainer == null) return;

            GameObject cardObj = Instantiate(tournamentCardPrefab, tournamentsContainer);

            // Configurar componentes de la card
            Text[] texts = cardObj.GetComponentsInChildren<Text>();
            if (texts.Length >= 5)
            {
                texts[0].text = tournament.name; // Nombre
                texts[1].text = FormatTimeRemaining(tournament.GetTimeRemaining()); // Tiempo restante
                texts[2].text = $"{tournament.totalPrizePool} coins"; // Premio
                texts[3].text = $"{tournament.currentParticipants}/{tournament.maxParticipants}"; // Participantes
                texts[4].text = $"Entrada: {tournament.entryFee} coins"; // Entrada
            }

            // Botón para ver detalles
            Button button = cardObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => ShowTournamentDetail(tournament));
            }

            // Color según estado
            Image bg = cardObj.GetComponent<Image>();
            if (bg != null)
            {
                if (tournament.IsParticipating(currentPlayer.userId))
                {
                    bg.color = new Color(0f, 0.83f, 1f, 0.3f); // Participando
                }
                else if (tournament.IsFull())
                {
                    bg.color = new Color(0.5f, 0.5f, 0.5f, 0.3f); // Lleno
                }
            }
        }

        #endregion

        #region Create Tournament

        /// <summary>
        /// Muestra el panel de crear torneo
        /// </summary>
        private void ShowCreateTournamentPanel()
        {
            // Verificar si el jugador es premium
            if (currentPlayer != null && !currentPlayer.IsPremiumActive())
            {
                Debug.LogWarning("[Tournament] Solo usuarios premium pueden crear torneos");
                ShowPremiumRequiredMessage();
                return;
            }

            ClearTournamentsList();
            createTournamentPanel?.SetActive(true);

            // Resetear campos
            if (tournamentNameInput != null) tournamentNameInput.text = "";
            if (entryFeeInput != null) entryFeeInput.text = "100";
            if (maxParticipantsInput != null) maxParticipantsInput.text = "50";
        }

        /// <summary>
        /// Crea un nuevo torneo
        /// </summary>
        private async void OnCreateTournamentClicked()
        {
            if (currentPlayer == null) return;

            // Validar campos
            string name = tournamentNameInput?.text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogWarning("[Tournament] Nombre de torneo vacío");
                return;
            }

            int entryFee = 100;
            int.TryParse(entryFeeInput?.text, out entryFee);

            int maxParticipants = 50;
            int.TryParse(maxParticipantsInput?.text, out maxParticipants);

            // Crear torneo
            TournamentData newTournament = new TournamentData
            {
                name = name,
                creatorId = currentPlayer.userId,
                creatorName = currentPlayer.username,
                entryFee = entryFee,
                maxParticipants = maxParticipants,
                startTime = System.DateTime.Now,
                endTime = System.DateTime.Now.AddHours(24),
                region = TournamentRegion.Global,
                status = TournamentStatus.Scheduled
            };

            // Configurar duración según dropdown
            if (durationDropdown != null)
            {
                switch (durationDropdown.value)
                {
                    case 0: newTournament.endTime = newTournament.startTime.AddHours(1); break;
                    case 1: newTournament.endTime = newTournament.startTime.AddHours(6); break;
                    case 2: newTournament.endTime = newTournament.startTime.AddHours(12); break;
                    case 3: newTournament.endTime = newTournament.startTime.AddHours(24); break;
                    case 4: newTournament.endTime = newTournament.startTime.AddDays(3); break;
                    case 5: newTournament.endTime = newTournament.startTime.AddDays(7); break;
                }
            }

            // Configurar región según dropdown
            if (regionDropdown != null)
            {
                newTournament.region = (TournamentRegion)regionDropdown.value;
                if (newTournament.region == TournamentRegion.Country)
                {
                    newTournament.countryCode = currentPlayer.countryCode;
                }
            }

            ShowLoading(true);

            try
            {
                // Guardar en Firebase
                bool success = await DatabaseService.Instance.CreateTournament(newTournament);

                if (success)
                {
                    Debug.Log($"[Tournament] Torneo creado: {newTournament.name}");

                    // Analytics
                    AnalyticsService.Instance?.LogTournamentCreated(newTournament.tournamentId, entryFee);

                    // Volver a vista de torneos activos
                    ShowView(TournamentView.Active);
                }
                else
                {
                    Debug.LogError("[Tournament] Error al crear torneo");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Tournament] Error: {ex.Message}");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        #endregion

        #region Tournament Detail

        /// <summary>
        /// Muestra los detalles de un torneo
        /// </summary>
        private void ShowTournamentDetail(TournamentData tournament)
        {
            Debug.Log($"[Tournament] Mostrando detalles de: {tournament.name}");

            selectedTournament = tournament;

            if (tournamentDetailPanel == null) return;

            tournamentDetailPanel.SetActive(true);

            // Actualizar información
            if (detailNameText != null)
                detailNameText.text = tournament.name;

            if (detailTimeRemainingText != null)
                detailTimeRemainingText.text = FormatTimeRemaining(tournament.GetTimeRemaining());

            if (detailPrizePoolText != null)
                detailPrizePoolText.text = $"{tournament.totalPrizePool} coins";

            if (detailParticipantsText != null)
                detailParticipantsText.text = $"{tournament.currentParticipants}/{tournament.maxParticipants}";

            if (detailEntryFeeText != null)
                detailEntryFeeText.text = $"{tournament.entryFee} coins";

            // Mostrar leaderboard del torneo
            DisplayTournamentLeaderboard(tournament);

            // Configurar botones
            bool isParticipating = tournament.IsParticipating(currentPlayer.userId);

            if (joinTournamentButton != null)
            {
                joinTournamentButton.gameObject.SetActive(!isParticipating);
                joinTournamentButton.interactable = tournament.CanJoin(currentPlayer);
            }

            if (playTournamentButton != null)
            {
                playTournamentButton.gameObject.SetActive(isParticipating && tournament.IsActive());
            }
        }

        /// <summary>
        /// Muestra el leaderboard del torneo
        /// </summary>
        private void DisplayTournamentLeaderboard(TournamentData tournament)
        {
            if (detailLeaderboardContainer == null) return;

            // Limpiar
            foreach (Transform child in detailLeaderboardContainer)
            {
                Destroy(child.gameObject);
            }

            // Mostrar participantes
            foreach (var participant in tournament.participants)
            {
                CreateLeaderboardEntry(participant);
            }
        }

        /// <summary>
        /// Crea una entrada en el leaderboard del torneo
        /// </summary>
        private void CreateLeaderboardEntry(ParticipantScore participant)
        {
            if (detailLeaderboardEntryPrefab == null || detailLeaderboardContainer == null) return;

            GameObject entryObj = Instantiate(detailLeaderboardEntryPrefab, detailLeaderboardContainer);

            Text[] texts = entryObj.GetComponentsInChildren<Text>();
            if (texts.Length >= 3)
            {
                int position = selectedTournament.participants.IndexOf(participant) + 1;
                texts[0].text = $"#{position}";
                texts[1].text = participant.username;
                texts[2].text = participant.bestTime < float.MaxValue ? $"{participant.bestTime:F3}s" : "--";
            }
        }

        /// <summary>
        /// Unirse a un torneo
        /// </summary>
        private async void OnJoinTournamentClicked()
        {
            if (selectedTournament == null || currentPlayer == null) return;

            Debug.Log($"[Tournament] Uniéndose a: {selectedTournament.name}");

            // Verificar que puede unirse
            if (!selectedTournament.CanJoin(currentPlayer))
            {
                Debug.LogWarning("[Tournament] No puede unirse al torneo");
                return;
            }

            ShowLoading(true);

            try
            {
                bool success = await DatabaseService.Instance.JoinTournament(
                    selectedTournament.tournamentId,
                    currentPlayer.userId
                );

                if (success)
                {
                    Debug.Log("[Tournament] Unión exitosa");

                    // Analytics
                    AnalyticsService.Instance?.LogTournamentJoined(selectedTournament.tournamentId);

                    // Actualizar vista
                    LoadActiveTournaments();
                    tournamentDetailPanel?.SetActive(false);
                }
                else
                {
                    Debug.LogError("[Tournament] Error al unirse");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Tournament] Error: {ex.Message}");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        /// <summary>
        /// Jugar en el torneo
        /// </summary>
        private void OnPlayTournamentClicked()
        {
            if (selectedTournament == null) return;

            Debug.Log($"[Tournament] Jugando torneo: {selectedTournament.tournamentId}");

            // Pasar el ID del torneo al GameManager
            PlayerPrefs.SetString("CurrentTournamentId", selectedTournament.tournamentId);
            PlayerPrefs.Save();

            // Cargar escena de juego
            SceneManager.LoadScene("Game");
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Formatea el tiempo restante
        /// </summary>
        private string FormatTimeRemaining(System.TimeSpan time)
        {
            if (time.TotalSeconds <= 0)
                return "Finalizado";

            if (time.TotalDays >= 1)
                return $"{(int)time.TotalDays}d {time.Hours}h";

            if (time.TotalHours >= 1)
                return $"{(int)time.TotalHours}h {time.Minutes}m";

            return $"{(int)time.TotalMinutes}m {time.Seconds}s";
        }

        /// <summary>
        /// Limpia la lista de torneos
        /// </summary>
        private void ClearTournamentsList()
        {
            if (tournamentsContainer == null) return;

            foreach (Transform child in tournamentsContainer)
            {
                Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// Muestra mensaje cuando no hay torneos
        /// </summary>
        private void ShowEmptyMessage()
        {
            GameObject emptyMsg = new GameObject("EmptyMessage");
            emptyMsg.transform.SetParent(tournamentsContainer);

            Text text = emptyMsg.AddComponent<Text>();
            text.text = currentView == TournamentView.Active ? "No hay torneos activos" : "No participas en ningún torneo";
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 24;
            text.color = Color.gray;
        }

        /// <summary>
        /// Muestra mensaje de premium requerido
        /// </summary>
        private void ShowPremiumRequiredMessage()
        {
            Debug.LogWarning("[Tournament] Se requiere premium");
            // Aquí se mostraría un diálogo para comprar premium
        }

        /// <summary>
        /// Muestra/oculta panel de carga
        /// </summary>
        private void ShowLoading(bool show)
        {
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(show);
            }
        }

        #endregion

        #region Navigation

        /// <summary>
        /// Vuelve al menú principal
        /// </summary>
        private void OnBackButtonClicked()
        {
            Debug.Log("[Tournament] Volviendo al menú principal");
            SceneManager.LoadScene("MainMenu");
        }

        #endregion
    }

    /// <summary>
    /// Vistas del manager de torneos
    /// </summary>
    public enum TournamentView
    {
        Active,
        MyTournaments,
        Create
    }
}
