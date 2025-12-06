using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DigitPark.Services.Firebase;
using DigitPark.Data;
using DigitPark.Localization;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager de la escena de Torneos
    /// Gestiona creación, visualización y participación en torneos
    /// </summary>
    public class TournamentManager : MonoBehaviour
    {
        [Header("Tabs")]
        [SerializeField] public Button searchTab;
        [SerializeField] public Button myTournamentsTab;
        [SerializeField] public Button createTab;

        [Header("Search Options")]
        [SerializeField] public Button searchOptionsButton;
        [SerializeField] public GameObject searchOptionsPanel;
        [SerializeField] public Button closeSearchOptionsButton;
        [SerializeField] public TMP_InputField usernameSearchInput;
        [SerializeField] public TMP_InputField minPlayersInput;
        [SerializeField] public TMP_InputField maxPlayersInput;
        [SerializeField] public TMP_InputField minHoursInput;
        [SerializeField] public TMP_Dropdown minTimeUnitDropdown;
        [SerializeField] public TMP_InputField maxHoursInput;
        [SerializeField] public TMP_Dropdown maxTimeUnitDropdown;
        [SerializeField] public Button applyButton;
        [SerializeField] public Button clearButton;

        [Header("Tournaments List")]
        [SerializeField] public Transform tournamentsContainer;
        [SerializeField] public ScrollRect scrollRect;
        [SerializeField] public GameObject tournamentItemPrefab;

        [Header("Blocker Panel")]
        [SerializeField] public GameObject blockerPanel;

        [Header("Confirm Popup")]
        [SerializeField] public GameObject confirmPopup;
        [SerializeField] public TextMeshProUGUI messageText;
        [SerializeField] public TextMeshProUGUI tournamentInfoText;
        [SerializeField] public Button confirmButton;
        [SerializeField] public Button cancelButton;

        [Header("Create Tournament Block")]
        [SerializeField] public GameObject createTournamentBlock;
        [SerializeField] public GameObject createTournamentPanel;
        [SerializeField] public TextMeshProUGUI maxPlayersValue;
        [SerializeField] public Slider maxPlayersSlider;
        [SerializeField] public Toggle publicToggle;
        [SerializeField] public Toggle privateToggle;
        [SerializeField] public TextMeshProUGUI durationValue;
        [SerializeField] public Slider durationSlider;
        [SerializeField] public Button createButton;
        [SerializeField] public Button cancelCreateButton;

        [Header("Loading")]
        [SerializeField] public GameObject loadingPanel;

        [Header("Navigation")]
        [SerializeField] public Button backButton;

        [Header("Leaderboard View")]
        [SerializeField] public GameObject leaderboardBackButton;
        [SerializeField] public GameObject exitTournamentButton;
        [SerializeField] public GameObject searchTournamentButton;

        [Header("Exit Tournament Confirmation")]
        [SerializeField] public GameObject exitTournamentConfirmPanel;
        [SerializeField] public Button confirmExitButton;
        [SerializeField] public Button cancelExitButton;

        // Estado
        private TournamentView currentView = TournamentView.Search;
        private PlayerData currentPlayer;
        private List<TournamentData> activeTournaments = new List<TournamentData>();
        private List<TournamentData> filteredTournaments = new List<TournamentData>();
        private List<TournamentData> myTournaments = new List<TournamentData>();
        private TournamentData selectedTournament;

        // Estado del popup de confirmación
        private ConfirmPopupMode popupMode = ConfirmPopupMode.None;

        // Filtros de búsqueda
        private string searchUsername = "";
        private int searchMinPlayers = 0;
        private int searchMaxPlayers = 999;
        private float searchMinHours = 0;
        private float searchMaxHours = 999;
        private bool hasActiveFilters = false;

        private void Start()
        {
            Debug.Log("[Tournament] TournamentManager iniciado");

            // Verificar e inicializar servicios si no existen (para testing directo)
            EnsureServicesExist();

            // Cargar datos del jugador
            LoadPlayerData();

            // Configurar listeners
            SetupListeners();

            // Ocultar paneles inicialmente
            if (blockerPanel != null) blockerPanel.SetActive(false);
            if (searchOptionsPanel != null) searchOptionsPanel.SetActive(false);
            if (createTournamentBlock != null) createTournamentBlock.SetActive(false);
            if (confirmPopup != null) confirmPopup.SetActive(false);
            if (leaderboardBackButton != null) leaderboardBackButton.SetActive(false);
            if (exitTournamentButton != null) exitTournamentButton.SetActive(false);
            if (searchTournamentButton != null) searchTournamentButton.SetActive(true); // Siempre visible
            if (exitTournamentConfirmPanel != null) exitTournamentConfirmPanel.SetActive(false);

            // Mostrar vista inicial
            ShowView(TournamentView.Search);
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
            // Tabs
            searchTab?.onClick.AddListener(() => ShowView(TournamentView.Search));
            myTournamentsTab?.onClick.AddListener(() => ShowView(TournamentView.MyTournaments));
            createTab?.onClick.AddListener(() => ShowView(TournamentView.Create));

            // Search Options
            searchOptionsButton?.onClick.AddListener(ShowSearchOptions);
            closeSearchOptionsButton?.onClick.AddListener(HideSearchOptions);
            applyButton?.onClick.AddListener(ApplySearchFilters);
            clearButton?.onClick.AddListener(ClearSearchFilters);

            // Create Tournament
            maxPlayersSlider?.onValueChanged.AddListener(OnMaxPlayersChanged);
            durationSlider?.onValueChanged.AddListener(OnDurationChanged);
            publicToggle?.onValueChanged.AddListener(OnPublicToggled);
            privateToggle?.onValueChanged.AddListener(OnPrivateToggled);
            createButton?.onClick.AddListener(OnCreateTournamentClicked);
            cancelCreateButton?.onClick.AddListener(HideCreatePanel);

            // Confirm Popup
            confirmButton?.onClick.AddListener(OnConfirmClicked);
            cancelButton?.onClick.AddListener(HideConfirmPopup);

            // Navigation
            backButton?.onClick.AddListener(OnBackButtonClicked);

            // Leaderboard Navigation
            if (leaderboardBackButton != null)
            {
                Button backBtn = leaderboardBackButton.GetComponent<Button>();
                if (backBtn != null)
                {
                    backBtn.onClick.AddListener(OnLeaderboardBackClicked);
                }
            }

            // Exit Tournament
            if (exitTournamentButton != null)
            {
                Button exitBtn = exitTournamentButton.GetComponent<Button>();
                if (exitBtn != null)
                {
                    exitBtn.onClick.AddListener(OnExitTournamentButtonClicked);
                }
            }

            // Search Tournament (volver a la vista de búsqueda desde el leaderboard)
            if (searchTournamentButton != null)
            {
                Button searchBtn = searchTournamentButton.GetComponent<Button>();
                if (searchBtn != null)
                {
                    searchBtn.onClick.AddListener(OnSearchTournamentButtonClicked);
                }
            }

            // Exit Tournament Confirmation
            confirmExitButton?.onClick.AddListener(OnConfirmExitClicked);
            cancelExitButton?.onClick.AddListener(HideExitConfirmPanel);
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

            // Ocultar create tournament block
            if (createTournamentBlock != null)
                createTournamentBlock.SetActive(false);

            switch (view)
            {
                case TournamentView.Search:
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
            SetTabButtonState(searchTab, currentView == TournamentView.Search);
            SetTabButtonState(myTournamentsTab, currentView == TournamentView.MyTournaments);
            SetTabButtonState(createTab, currentView == TournamentView.Create);
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

        #region Search Options

        /// <summary>
        /// Muestra el panel de opciones de búsqueda
        /// </summary>
        private void ShowSearchOptions()
        {
            Debug.Log("[Tournament] Mostrando opciones de búsqueda");

            if (blockerPanel != null)
                blockerPanel.SetActive(true);

            if (searchOptionsPanel != null)
                searchOptionsPanel.SetActive(true);
        }

        /// <summary>
        /// Oculta el panel de opciones de búsqueda
        /// </summary>
        private void HideSearchOptions()
        {
            Debug.Log("[Tournament] Ocultando opciones de búsqueda");

            if (searchOptionsPanel != null)
                searchOptionsPanel.SetActive(false);

            if (blockerPanel != null)
                blockerPanel.SetActive(false);
        }

        /// <summary>
        /// Aplica los filtros de búsqueda
        /// </summary>
        private void ApplySearchFilters()
        {
            Debug.Log("[Tournament] Aplicando filtros de búsqueda");

            // Leer filtros
            searchUsername = usernameSearchInput?.text ?? "";

            // Usar valores por defecto si los campos están vacíos
            if (!int.TryParse(minPlayersInput?.text, out searchMinPlayers) || string.IsNullOrEmpty(minPlayersInput?.text))
                searchMinPlayers = 0;
            if (!int.TryParse(maxPlayersInput?.text, out searchMaxPlayers) || string.IsNullOrEmpty(maxPlayersInput?.text))
                searchMaxPlayers = 999;

            if (!float.TryParse(minHoursInput?.text, out searchMinHours) || string.IsNullOrEmpty(minHoursInput?.text))
                searchMinHours = 0;
            if (!float.TryParse(maxHoursInput?.text, out searchMaxHours) || string.IsNullOrEmpty(maxHoursInput?.text))
                searchMaxHours = 999;

            // Convertir a horas si la unidad seleccionada es "Días" (índice 1)
            if (minTimeUnitDropdown != null && minTimeUnitDropdown.value == 1 && !string.IsNullOrEmpty(minHoursInput?.text))
            {
                searchMinHours *= 24f;
            }
            if (maxTimeUnitDropdown != null && maxTimeUnitDropdown.value == 1 && !string.IsNullOrEmpty(maxHoursInput?.text))
            {
                searchMaxHours *= 24f;
            }

            // Verificar si hay filtros activos
            hasActiveFilters = !string.IsNullOrEmpty(searchUsername) ||
                              !string.IsNullOrEmpty(minPlayersInput?.text) ||
                              !string.IsNullOrEmpty(maxPlayersInput?.text) ||
                              !string.IsNullOrEmpty(minHoursInput?.text) ||
                              !string.IsNullOrEmpty(maxHoursInput?.text);

            // Aplicar filtros
            FilterTournaments();

            // Ocultar panel
            HideSearchOptions();
        }

        /// <summary>
        /// Limpia los filtros de búsqueda
        /// </summary>
        private void ClearSearchFilters()
        {
            Debug.Log("[Tournament] Limpiando filtros");

            // Resetear campos
            if (usernameSearchInput != null) usernameSearchInput.text = "";
            if (minPlayersInput != null) minPlayersInput.text = "";
            if (maxPlayersInput != null) maxPlayersInput.text = "";
            if (minHoursInput != null) minHoursInput.text = "";
            if (maxHoursInput != null) maxHoursInput.text = "";

            // Resetear dropdowns a "Horas" (índice 0)
            if (minTimeUnitDropdown != null) minTimeUnitDropdown.value = 0;
            if (maxTimeUnitDropdown != null) maxTimeUnitDropdown.value = 0;

            // Resetear valores
            searchUsername = "";
            searchMinPlayers = 0;
            searchMaxPlayers = 999;
            searchMinHours = 0;
            searchMaxHours = 999;
            hasActiveFilters = false;

            // Mostrar todos
            FilterTournaments();
        }

        /// <summary>
        /// Filtra los torneos según los criterios
        /// </summary>
        private void FilterTournaments()
        {
            filteredTournaments = new List<TournamentData>(activeTournaments);

            // Si NO hay filtros activos y estamos en la vista de búsqueda, ocultar torneos inscritos
            if (!hasActiveFilters && currentView == TournamentView.Search && currentPlayer != null)
            {
                filteredTournaments = filteredTournaments.FindAll(t => !t.IsParticipating(currentPlayer.userId));
                Debug.Log($"[Tournament] Ocultando torneos inscritos. Quedaron: {filteredTournaments.Count}");
            }

            // Filtrar por username del creador
            if (!string.IsNullOrEmpty(searchUsername))
            {
                filteredTournaments = filteredTournaments.FindAll(t =>
                    t.creatorName.ToLower().Contains(searchUsername.ToLower()));
            }

            // Filtrar por jugadores
            filteredTournaments = filteredTournaments.FindAll(t =>
                t.maxParticipants >= searchMinPlayers && t.maxParticipants <= searchMaxPlayers);

            // Filtrar por tiempo restante
            filteredTournaments = filteredTournaments.FindAll(t =>
            {
                float hoursRemaining = (float)t.GetTimeRemaining().TotalHours;
                return hoursRemaining >= searchMinHours && hoursRemaining <= searchMaxHours;
            });

            Debug.Log($"[Tournament] Filtrados: {filteredTournaments.Count} de {activeTournaments.Count}");

            // Actualizar vista
            DisplayTournaments(filteredTournaments);
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

            // Ocultar botones del leaderboard (excepto buscar torneo que siempre está visible)
            if (leaderboardBackButton != null)
                leaderboardBackButton.SetActive(false);
            if (exitTournamentButton != null)
                exitTournamentButton.SetActive(false);
            if (searchTournamentButton != null)
                searchTournamentButton.SetActive(true); // Siempre visible

            try
            {
                activeTournaments = await DatabaseService.Instance.GetActiveTournaments();

                Debug.Log($"[Tournament] {activeTournaments.Count} torneos activos cargados");

                // Usar FilterTournaments en lugar de DisplayTournaments para aplicar filtro de inscritos
                FilterTournaments();
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

            // Ocultar botones del leaderboard (excepto buscar torneo que siempre está visible)
            if (leaderboardBackButton != null)
                leaderboardBackButton.SetActive(false);
            if (exitTournamentButton != null)
                exitTournamentButton.SetActive(false);
            if (searchTournamentButton != null)
                searchTournamentButton.SetActive(true); // Siempre visible

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
        /// Crea una entrada de torneo usando el prefab TournamentItem
        /// </summary>
        private void CreateTournamentCard(TournamentData tournament)
        {
            if (tournamentsContainer == null || tournamentItemPrefab == null)
            {
                Debug.LogError($"[Tournament] Container o prefab nulo - Container: {(tournamentsContainer == null ? "NULL" : "OK")}, Prefab: {(tournamentItemPrefab == null ? "NULL" : "OK")}");
                return;
            }

            // Instanciar el prefab
            GameObject itemObj = Instantiate(tournamentItemPrefab, tournamentsContainer);
            itemObj.name = $"TournamentItem_{tournament.tournamentId}";

            // Obtener referencias a los componentes
            Image bg = itemObj.GetComponent<Image>();
            Button itemButton = itemObj.GetComponent<Button>();

            // Configurar color de fondo según participación
            bool isParticipating = tournament.IsParticipating(currentPlayer?.userId ?? "");
            if (bg != null)
            {
                bg.color = isParticipating ? new Color(0f, 0.83f, 1f, 0.3f) : new Color(0.15f, 0.15f, 0.2f, 0.95f);
            }

            // Configurar botón
            if (itemButton != null)
            {
                itemButton.onClick.RemoveAllListeners();
                TournamentData tournamentCopy = tournament;
                itemButton.onClick.AddListener(() => OnTournamentItemClicked(tournamentCopy));
            }

            // Buscar y configurar los textos hijos
            Transform participantsTransform = itemObj.transform.Find("ParticipantsText");
            if (participantsTransform != null)
            {
                TextMeshProUGUI participantsText = participantsTransform.GetComponent<TextMeshProUGUI>();
                if (participantsText != null)
                {
                    participantsText.text = $"{tournament.currentParticipants}/{tournament.maxParticipants}";
                }
            }

            Transform creatorTransform = itemObj.transform.Find("CreatorText");
            if (creatorTransform != null)
            {
                TextMeshProUGUI creatorText = creatorTransform.GetComponent<TextMeshProUGUI>();
                if (creatorText != null)
                {
                    creatorText.text = tournament.creatorName;
                }
            }

            Transform timeTransform = itemObj.transform.Find("TimeText");
            if (timeTransform != null)
            {
                TextMeshProUGUI timeText = timeTransform.GetComponent<TextMeshProUGUI>();
                if (timeText != null)
                {
                    string timeString = FormatTimeRemaining(tournament.GetTimeRemaining());
                    timeText.text = timeString;
                }
            }

            itemObj.SetActive(true);
        }

        /// <summary>
        /// Crea un divisor vertical (helper para leaderboard)
        /// </summary>
        private void CreateVerticalDivider(Transform parent, float xPosition)
        {
            GameObject divider = new GameObject("Divider");
            divider.transform.SetParent(parent, false);

            RectTransform divRT = divider.AddComponent<RectTransform>();
            divRT.anchorMin = new Vector2(0, 0);
            divRT.anchorMax = new Vector2(0, 1);
            divRT.pivot = new Vector2(0.5f, 0.5f);
            divRT.anchoredPosition = new Vector2(xPosition, 0);
            divRT.sizeDelta = new Vector2(1f, -20);

            Image divImage = divider.AddComponent<Image>();
            divImage.color = new Color(0.3f, 0.3f, 0.4f, 0.5f);
        }

        /// <summary>
        /// Crea un divisor horizontal (helper para leaderboard)
        /// </summary>
        private void CreateHorizontalDivider(Transform parent)
        {
            GameObject divider = new GameObject("HorizontalDivider");
            divider.transform.SetParent(parent, false);

            RectTransform divRT = divider.AddComponent<RectTransform>();
            divRT.anchorMin = new Vector2(0, 0);
            divRT.anchorMax = new Vector2(1, 0);
            divRT.pivot = new Vector2(0.5f, 0);
            divRT.anchoredPosition = Vector2.zero;
            divRT.sizeDelta = new Vector2(-40, 1f);

            Image divImage = divider.AddComponent<Image>();
            divImage.color = new Color(0.3f, 0.3f, 0.4f, 0.3f);
        }

        /// <summary>
        /// Crea un texto para el item (helper para leaderboard)
        /// </summary>
        private TextMeshProUGUI CreateItemText(Transform parent, string name, string text, int fontSize, Color color)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Normal;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Ellipsis;

            return tmp;
        }

        /// <summary>
        /// Maneja el clic en un item de torneo
        /// </summary>
        private void OnTournamentItemClicked(TournamentData tournament)
        {
            Debug.Log($"[Tournament] Clic en torneo: {tournament.tournamentId}");

            selectedTournament = tournament;
            bool isParticipating = tournament.IsParticipating(currentPlayer?.userId ?? "");

            if (isParticipating)
            {
                // Si ya participa, mostrar leaderboard directamente
                ShowTournamentLeaderboard(tournament);
            }
            else
            {
                // Si no participa, preguntar si quiere unirse
                popupMode = ConfirmPopupMode.JoinTournament;
                ShowConfirmPopupForJoin(tournament);
            }
        }

        #endregion

        #region Create Tournament

        /// <summary>
        /// Muestra el panel de crear torneo
        /// </summary>
        private void ShowCreateTournamentPanel()
        {
            Debug.Log("[Tournament] Mostrando panel de creación");

            ClearTournamentsList();

            // Mostrar blocker y create block
            if (blockerPanel != null) blockerPanel.SetActive(true);
            if (createTournamentBlock != null) createTournamentBlock.SetActive(true);
            if (createTournamentPanel != null) createTournamentPanel.SetActive(true);

            // Resetear sliders
            if (maxPlayersSlider != null)
            {
                maxPlayersSlider.value = 50;
                OnMaxPlayersChanged(50);
            }

            if (durationSlider != null)
            {
                durationSlider.value = 24; // 24 horas por defecto
                OnDurationChanged(24);
            }

            // Resetear toggles (Público por defecto)
            if (publicToggle != null) publicToggle.isOn = true;
            if (privateToggle != null) privateToggle.isOn = false;
        }

        /// <summary>
        /// Oculta el panel de crear torneo
        /// </summary>
        private void HideCreatePanel()
        {
            Debug.Log("[Tournament] Ocultando panel de creación");
            if (blockerPanel != null) blockerPanel.SetActive(false);
            if (createTournamentBlock != null) createTournamentBlock.SetActive(false);
            if (createTournamentPanel != null) createTournamentPanel.SetActive(false);

            // Volver a Search
            ShowView(TournamentView.Search);
        }

        /// <summary>
        /// Maneja cambio en slider de jugadores máximos
        /// </summary>
        private void OnMaxPlayersChanged(float value)
        {
            if (maxPlayersValue != null)
                maxPlayersValue.text = $"{(int)value}/55";
        }

        /// <summary>
        /// Maneja cambio en slider de duración
        /// </summary>
        private void OnDurationChanged(float value)
        {
            if (durationValue != null)
            {
                int hours = (int)value;
                if (hours < 24)
                    durationValue.text = $"{hours}h";
                else
                {
                    int days = hours / 24;
                    durationValue.text = $"{days}d";
                }
            }
        }

        /// <summary>
        /// Maneja toggle de público
        /// </summary>
        private void OnPublicToggled(bool isOn)
        {
            if (isOn && privateToggle != null)
            {
                privateToggle.isOn = false;
            }
        }

        /// <summary>
        /// Maneja toggle de privado
        /// </summary>
        private void OnPrivateToggled(bool isOn)
        {
            if (isOn && publicToggle != null)
            {
                publicToggle.isOn = false;
            }
        }

        /// <summary>
        /// Crea un nuevo torneo (directamente sin popup de confirmación)
        /// </summary>
        private async void OnCreateTournamentClicked()
        {
            if (currentPlayer == null) return;

            // Obtener valores de los sliders
            int maxParticipants = (int)(maxPlayersSlider?.value ?? 50);
            int durationHours = (int)(durationSlider?.value ?? 24);
            bool isPublic = publicToggle?.isOn ?? true;

            Debug.Log($"[Tournament] Creando torneo: {maxParticipants} jugadores, {durationHours}h");

            // Crear torneo directamente
            await CreateTournamentDirectly(maxParticipants, durationHours, isPublic);
        }

        /// <summary>
        /// Muestra popup de confirmación para unirse a torneo
        /// </summary>
        private void ShowConfirmPopupForJoin(TournamentData tournament)
        {
            if (confirmPopup == null || blockerPanel == null) return;

            // Configurar textos para UNIRSE a torneo
            if (messageText != null)
                messageText.text = AutoLocalizer.Get("join_confirm_message");

            if (tournamentInfoText != null)
            {
                string timeRemaining = FormatTimeRemaining(tournament.GetTimeRemaining());
                string creatorLabel = AutoLocalizer.Get("creator_label");
                string participantsLabel = AutoLocalizer.Get("participants");
                string timeRemainingLabel = AutoLocalizer.Get("time_remaining");
                tournamentInfoText.text = $"{creatorLabel} {tournament.creatorName}\n" +
                    $"{participantsLabel}: {tournament.currentParticipants}/{tournament.maxParticipants}\n" +
                    $"{timeRemainingLabel} {timeRemaining}";
            }

            // Mostrar blocker y popup
            if (blockerPanel != null)
                blockerPanel.SetActive(true);

            confirmPopup.SetActive(true);
        }

        /// <summary>
        /// Oculta popup de confirmación
        /// </summary>
        private void HideConfirmPopup()
        {
            if (confirmPopup != null)
                confirmPopup.SetActive(false);

            if (blockerPanel != null)
                blockerPanel.SetActive(false);
        }

        /// <summary>
        /// Confirma la acción del popup (solo para unirse a torneos)
        /// </summary>
        private async void OnConfirmClicked()
        {
            if (currentPlayer == null) return;

            // Ocultar popup
            HideConfirmPopup();

            // Ejecutar acción según el modo
            switch (popupMode)
            {
                case ConfirmPopupMode.JoinTournament:
                    await JoinTournamentConfirmed();
                    break;

                case ConfirmPopupMode.ViewLeaderboard:
                    ShowTournamentLeaderboard(selectedTournament);
                    break;
            }

            // Reset popup mode
            popupMode = ConfirmPopupMode.None;
        }

        /// <summary>
        /// Crea el torneo directamente y une al creador automáticamente
        /// </summary>
        private async System.Threading.Tasks.Task CreateTournamentDirectly(int maxParticipants, int durationHours, bool isPublic)
        {
            // Crear torneo
            TournamentData newTournament = new TournamentData
            {
                name = $"Torneo de {currentPlayer.username}",
                creatorId = currentPlayer.userId,
                creatorName = currentPlayer.username,
                entryFee = 0,
                maxParticipants = maxParticipants,
                startTime = System.DateTime.Now,
                endTime = System.DateTime.Now.AddHours(durationHours),
                region = TournamentRegion.Global,
                status = TournamentStatus.Scheduled
            };

            ShowLoading(true);

            try
            {
                // Guardar en Firebase
                bool success = await DatabaseService.Instance.CreateTournament(newTournament);

                if (success)
                {
                    Debug.Log($"[Tournament] Torneo creado exitosamente: {newTournament.tournamentId}");

                    // Unir automáticamente al creador
                    await DatabaseService.Instance.JoinTournament(newTournament.tournamentId, currentPlayer.userId);
                    Debug.Log($"[Tournament] Creador unido automáticamente al torneo");

                    // Analytics
                    AnalyticsService.Instance?.LogTournamentCreated(newTournament.tournamentId, 0);

                    // Mostrar mensaje de éxito
                    ShowSuccessMessage(AutoLocalizer.Get("create_success"));

                    // Ocultar panel de creación y volver a Search (después de 1.5 segundos)
                    await System.Threading.Tasks.Task.Delay(1500);
                    HideCreatePanel();

                    // Recargar torneos activos
                    LoadActiveTournaments();
                }
                else
                {
                    Debug.LogError("[Tournament] Error al crear torneo");
                    ShowErrorMessage(AutoLocalizer.Get("create_error"));
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Tournament] Error: {ex.Message}");
                ShowErrorMessage($"Error: {ex.Message}");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        /// <summary>
        /// Une al jugador al torneo después de confirmar
        /// </summary>
        private async System.Threading.Tasks.Task JoinTournamentConfirmed()
        {
            if (selectedTournament == null) return;

            ShowLoading(true);

            try
            {
                bool success = await DatabaseService.Instance.JoinTournament(selectedTournament.tournamentId, currentPlayer.userId);

                if (success)
                {
                    Debug.Log($"[Tournament] Unido exitosamente al torneo: {selectedTournament.tournamentId}");
                    ShowSuccessMessage(AutoLocalizer.Get("join_success"));

                    // Recargar torneos
                    await System.Threading.Tasks.Task.Delay(1500);
                    LoadActiveTournaments();
                }
                else
                {
                    ShowErrorMessage(AutoLocalizer.Get("join_error"));
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Tournament] Error: {ex.Message}");
                ShowErrorMessage($"Error: {ex.Message}");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Formatea el tiempo restante
        /// </summary>
        private string FormatTimeRemaining(System.TimeSpan time)
        {
            if (time.TotalSeconds <= 0)
                return AutoLocalizer.Get("finished");

            if (time.TotalDays >= 1)
                return AutoLocalizer.Get("time_days_hours", (int)time.TotalDays, time.Hours);

            if (time.TotalHours >= 1)
                return AutoLocalizer.Get("time_hours_minutes", (int)time.TotalHours, time.Minutes);

            return AutoLocalizer.Get("time_minutes_seconds", (int)time.TotalMinutes, time.Seconds);
        }

        /// <summary>
        /// Limpia la lista de torneos
        /// </summary>
        private void ClearTournamentsList()
        {
            if (tournamentsContainer == null) return;

            // Resetear la posición del ScrollRect antes de destruir para evitar MissingReferenceException
            if (scrollRect != null && scrollRect.content != null)
            {
                scrollRect.StopMovement();
                scrollRect.normalizedPosition = new Vector2(0, 1);
            }

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
            text.text = currentView == TournamentView.Search
                ? AutoLocalizer.Get("no_active_tournaments")
                : AutoLocalizer.Get("not_in_tournament");
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 24;
            text.color = Color.gray;
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

        /// <summary>
        /// Muestra mensaje de éxito
        /// </summary>
        private void ShowSuccessMessage(string message)
        {
            Debug.Log($"[Tournament] ✅ {message}");
            // TODO: Implementar popup de éxito visual
        }

        /// <summary>
        /// Muestra mensaje de error
        /// </summary>
        private void ShowErrorMessage(string message)
        {
            Debug.LogError($"[Tournament] ❌ {message}");
            // TODO: Implementar popup de error visual
        }

        /// <summary>
        /// Muestra el leaderboard del torneo
        /// </summary>
        private void ShowTournamentLeaderboard(TournamentData tournament)
        {
            Debug.Log($"[Tournament] Mostrando leaderboard del torneo: {tournament.tournamentId}");

            if (tournament == null || tournament.participants == null)
            {
                ShowErrorMessage("No se puede mostrar el leaderboard");
                return;
            }

            // Ordenar participantes por mejor tiempo
            tournament.SortParticipants();

            // Limpiar container
            ClearTournamentsList();

            // Mostrar botón de back
            if (leaderboardBackButton != null)
                leaderboardBackButton.SetActive(true);

            // Mostrar botón de buscar torneo
            if (searchTournamentButton != null)
                searchTournamentButton.SetActive(true);

            // Mostrar botón de salir del torneo si el usuario participa
            bool isParticipating = tournament.IsParticipating(currentPlayer?.userId ?? "");
            Debug.Log($"[Tournament] ¿Usuario participa? {isParticipating}, userId: {currentPlayer?.userId}, participantes: {tournament.participants?.Count ?? 0}");

            if (exitTournamentButton != null && isParticipating)
            {
                exitTournamentButton.SetActive(true);
                Debug.Log("[Tournament] Botón de salir del torneo ACTIVADO");
            }
            else
            {
                Debug.Log($"[Tournament] Botón de salir NO activado. exitButton null: {exitTournamentButton == null}, isParticipating: {isParticipating}");
            }

            // Título del leaderboard
            CreateLeaderboardTitle(tournament);

            // Crear items del leaderboard
            int position = 1;
            foreach (var participant in tournament.participants)
            {
                CreateLeaderboardItem(position, participant);
                position++;
            }

            Debug.Log($"[Tournament] Leaderboard mostrado con {tournament.participants.Count} participantes");
        }

        /// <summary>
        /// Crea el título del leaderboard
        /// </summary>
        private void CreateLeaderboardTitle(TournamentData tournament)
        {
            if (tournamentsContainer == null) return;

            GameObject titleObj = new GameObject("LeaderboardTitle");
            titleObj.transform.SetParent(tournamentsContainer, false);

            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 1);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0.5f, 1);
            titleRT.sizeDelta = new Vector2(0, 100);

            // Layout
            var layout = titleObj.AddComponent<LayoutElement>();
            layout.preferredHeight = 100;

            // Fondo
            Image bg = titleObj.AddComponent<Image>();
            bg.color = new Color(0f, 0.83f, 1f, 0.5f);

            // Texto del título
            TextMeshProUGUI titleText = CreateItemText(titleObj.transform, "TitleText",
                $"LEADERBOARD - {tournament.name}", 32, Color.white);
            RectTransform titleTextRT = titleText.GetComponent<RectTransform>();
            titleTextRT.anchorMin = new Vector2(0, 0.5f);
            titleTextRT.anchorMax = new Vector2(1, 1);
            titleTextRT.pivot = new Vector2(0.5f, 0.5f);
            titleTextRT.anchoredPosition = Vector2.zero;
            titleTextRT.sizeDelta = new Vector2(0, 0);
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;

            // Texto de info
            string timeRemaining = FormatTimeRemaining(tournament.GetTimeRemaining());
            TextMeshProUGUI infoText = CreateItemText(titleObj.transform, "InfoText",
                $"Participantes: {tournament.currentParticipants}/{tournament.maxParticipants} | Tiempo restante: {timeRemaining}",
                20, new Color(0.8f, 0.8f, 0.8f));
            RectTransform infoTextRT = infoText.GetComponent<RectTransform>();
            infoTextRT.anchorMin = new Vector2(0, 0);
            infoTextRT.anchorMax = new Vector2(1, 0.5f);
            infoTextRT.pivot = new Vector2(0.5f, 0.5f);
            infoTextRT.anchoredPosition = Vector2.zero;
            infoTextRT.sizeDelta = new Vector2(0, 0);
            infoText.alignment = TextAlignmentOptions.Center;
        }

        /// <summary>
        /// Crea un item del leaderboard - MISMO ESTILO QUE SCORES
        /// Estructura: Posición | Username | Tiempo
        /// </summary>
        private void CreateLeaderboardItem(int position, ParticipantScore participant)
        {
            if (tournamentsContainer == null) return;

            GameObject itemObj = new GameObject($"LeaderboardItem_{position}");
            itemObj.transform.SetParent(tournamentsContainer, false);

            RectTransform itemRT = itemObj.AddComponent<RectTransform>();
            itemRT.anchorMin = new Vector2(0, 1);
            itemRT.anchorMax = new Vector2(1, 1);
            itemRT.pivot = new Vector2(0.5f, 1);
            itemRT.anchoredPosition = Vector2.zero;
            itemRT.sizeDelta = new Vector2(0, 80);

            // Layout element para el container padre
            var layoutElement = itemObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 80;
            layoutElement.minHeight = 80;
            layoutElement.flexibleWidth = 1;

            // Verificar si es el jugador actual
            bool isCurrentPlayer = participant.userId == (currentPlayer?.userId ?? "");

            // Fondo - EXACTAMENTE como en Scores
            Image bg = itemObj.AddComponent<Image>();
            if (isCurrentPlayer)
            {
                bg.color = new Color(0f, 0.83f, 1f, 0.95f); // Azul eléctrico
            }
            else
            {
                bg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f); // Gris oscuro
            }

            // HorizontalLayoutGroup para controlar el orden de los elementos
            var hlg = itemObj.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.spacing = 10;
            hlg.padding = new RectOffset(20, 20, 5, 5);

            // 1. POSICIÓN (izquierda) - colores de medalla
            Color posColor;
            if (position == 1)
                posColor = new Color(1f, 0.84f, 0f); // Oro
            else if (position == 2)
                posColor = new Color(0.75f, 0.75f, 0.75f); // Plata
            else if (position == 3)
                posColor = new Color(0.8f, 0.5f, 0.2f); // Bronce
            else
                posColor = new Color(1f, 0.84f, 0f); // Amarillo

            GameObject posObj = new GameObject("Position");
            posObj.transform.SetParent(itemObj.transform, false);
            var posLayout = posObj.AddComponent<LayoutElement>();
            posLayout.preferredWidth = 80;
            posLayout.minWidth = 80;
            TextMeshProUGUI posText = posObj.AddComponent<TextMeshProUGUI>();
            posText.text = $"{position}";
            posText.fontSize = 32;
            posText.color = posColor;
            posText.alignment = TextAlignmentOptions.Center;
            posText.fontStyle = FontStyles.Bold;

            // 2. USERNAME (centro) - blanco
            GameObject nameObj = new GameObject("Username");
            nameObj.transform.SetParent(itemObj.transform, false);
            var nameLayout = nameObj.AddComponent<LayoutElement>();
            nameLayout.flexibleWidth = 1; // Se expande para llenar el espacio
            nameLayout.minWidth = 200;
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = participant.username;
            nameText.fontSize = 26;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Center;

            // 3. TIEMPO (derecha) - verde brillante
            string timeString = participant.bestTime == float.MaxValue ?
                AutoLocalizer.Get("no_time") : $"{participant.bestTime:F3}s";

            GameObject timeObj = new GameObject("Time");
            timeObj.transform.SetParent(itemObj.transform, false);
            var timeLayout = timeObj.AddComponent<LayoutElement>();
            timeLayout.preferredWidth = 150;
            timeLayout.minWidth = 150;
            TextMeshProUGUI timeText = timeObj.AddComponent<TextMeshProUGUI>();
            timeText.text = timeString;
            timeText.fontSize = 26;
            timeText.color = new Color(0f, 1f, 0.53f); // Verde brillante
            timeText.alignment = TextAlignmentOptions.Center;

            itemObj.SetActive(true);
        }

        /// <summary>
        /// Formatea un tiempo en segundos a formato legible (MM:SS.mmm)
        /// </summary>
        private string FormatTime(float timeInSeconds)
        {
            int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
            float seconds = timeInSeconds % 60f;
            return $"{minutes:00}:{seconds:00.000}";
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

        /// <summary>
        /// Vuelve de la vista de leaderboard a la vista de torneos
        /// </summary>
        private void OnLeaderboardBackClicked()
        {
            Debug.Log("[Tournament] Volviendo a la vista de torneos");

            // Ocultar botones de leaderboard
            if (leaderboardBackButton != null)
                leaderboardBackButton.SetActive(false);
            if (exitTournamentButton != null)
                exitTournamentButton.SetActive(false);
            if (searchTournamentButton != null)
                searchTournamentButton.SetActive(false);

            // Volver a mostrar la vista actual (Search o MyTournaments)
            ShowView(currentView);
        }

        /// <summary>
        /// Muestra el panel de opciones de búsqueda desde el leaderboard
        /// </summary>
        private void OnSearchTournamentButtonClicked()
        {
            Debug.Log("[Tournament] Mostrando opciones de búsqueda desde leaderboard");

            // Mostrar el panel de opciones de búsqueda
            ShowSearchOptions();
        }

        #endregion

        #region Exit Tournament

        /// <summary>
        /// Muestra el panel de confirmación para salir del torneo
        /// </summary>
        private void OnExitTournamentButtonClicked()
        {
            Debug.Log("[Tournament] Mostrando confirmación de salida del torneo");

            if (selectedTournament == null)
            {
                Debug.LogError("[Tournament] No hay torneo seleccionado");
                return;
            }

            // Mostrar blocker y panel de confirmación
            if (blockerPanel != null)
                blockerPanel.SetActive(true);

            if (exitTournamentConfirmPanel != null)
                exitTournamentConfirmPanel.SetActive(true);
        }

        /// <summary>
        /// Oculta el panel de confirmación de salida
        /// </summary>
        private void HideExitConfirmPanel()
        {
            Debug.Log("[Tournament] Ocultando confirmación de salida");

            if (exitTournamentConfirmPanel != null)
                exitTournamentConfirmPanel.SetActive(false);

            if (blockerPanel != null)
                blockerPanel.SetActive(false);
        }

        /// <summary>
        /// Confirma la salida del torneo
        /// </summary>
        private async void OnConfirmExitClicked()
        {
            if (selectedTournament == null || currentPlayer == null)
            {
                Debug.LogError("[Tournament] No hay torneo o jugador seleccionado");
                return;
            }

            Debug.Log($"[Tournament] Confirmando salida del torneo: {selectedTournament.tournamentId}");

            // Ocultar panel de confirmación
            HideExitConfirmPanel();

            ShowLoading(true);

            try
            {
                bool success = await DatabaseService.Instance.LeaveTournament(selectedTournament.tournamentId, currentPlayer.userId);

                if (success)
                {
                    Debug.Log($"[Tournament] Salida exitosa del torneo: {selectedTournament.tournamentId}");
                    ShowSuccessMessage(AutoLocalizer.Get("exit_success"));

                    // Ocultar botones de leaderboard
                    if (leaderboardBackButton != null)
                        leaderboardBackButton.SetActive(false);
                    if (exitTournamentButton != null)
                        exitTournamentButton.SetActive(false);
                    if (searchTournamentButton != null)
                        searchTournamentButton.SetActive(false);

                    // Esperar y recargar torneos
                    await System.Threading.Tasks.Task.Delay(1500);
                    LoadActiveTournaments();
                }
                else
                {
                    ShowErrorMessage(AutoLocalizer.Get("exit_error"));
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Tournament] Error al salir del torneo: {ex.Message}");
                ShowErrorMessage($"Error: {ex.Message}");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        #endregion
    }

    /// <summary>
    /// Vistas del manager de torneos
    /// </summary>
    public enum TournamentView
    {
        Search,
        MyTournaments,
        Create
    }

    /// <summary>
    /// Modo del popup de confirmación
    /// </summary>
    public enum ConfirmPopupMode
    {
        None,
        JoinTournament,
        ViewLeaderboard
    }
}
