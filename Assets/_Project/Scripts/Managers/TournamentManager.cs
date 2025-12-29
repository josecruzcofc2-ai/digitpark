using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DigitPark.Services.Firebase;
using DigitPark.Data;
using DigitPark.Localization;
using DigitPark.UI.Panels;

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

        [Header("UI - Panels (Prefabs)")]
        [SerializeField] private ErrorPanelUI errorPanel;
        [SerializeField] private ConfirmPanelUI confirmPanel;
        [SerializeField] private ConfirmPanelUI exitConfirmPanel;
        [SerializeField] private ConfirmPanelUI premiumRequiredPanel;

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

            // Remover comportamiento de botón del panel de búsqueda (no debería ser clickeable)
            RemovePanelButtonBehavior(searchOptionsPanel);

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
        /// NUEVO FLUJO: Solo mostrar torneos disponibles (no llenos) ya que los torneos
        /// no empiezan hasta que estén llenos. Si está lleno, ya empezó y no se puede unir.
        /// </summary>
        private void FilterTournaments()
        {
            filteredTournaments = new List<TournamentData>(activeTournaments);

            // NUEVO FLUJO: Filtrar torneos llenos (ya empezaron, no se puede unir)
            if (currentView == TournamentView.Search)
            {
                int beforeFilter = filteredTournaments.Count;
                filteredTournaments = filteredTournaments.FindAll(t => !t.IsFull());
                Debug.Log($"[Tournament] Filtrando torneos llenos. Antes: {beforeFilter}, Después: {filteredTournaments.Count}");
            }

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
            // IMPORTANTE: Limpiar el contenedor antes de crear nuevos items
            ClearTournamentsList();

            if (tournaments.Count == 0)
            {
                ShowEmptyMessage();
                return;
            }

            // Configurar el contenedor con VerticalLayoutGroup
            RectTransform containerRT = tournamentsContainer as RectTransform;
            if (containerRT != null)
            {
                containerRT.anchorMin = new Vector2(0, 1);
                containerRT.anchorMax = new Vector2(1, 1);
                containerRT.pivot = new Vector2(0.5f, 1);
                containerRT.anchoredPosition = Vector2.zero;
                containerRT.sizeDelta = new Vector2(0, 0);
            }

            var layoutGroup = tournamentsContainer.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = tournamentsContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            }
            layoutGroup.spacing = 5f;
            layoutGroup.padding = new RectOffset(5, 5, 5, 5);
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;

            var sizeFitter = tournamentsContainer.GetComponent<ContentSizeFitter>();
            if (sizeFitter == null)
            {
                sizeFitter = tournamentsContainer.gameObject.AddComponent<ContentSizeFitter>();
            }
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            foreach (var tournament in tournaments)
            {
                CreateTournamentCard(tournament);
            }

            // Forzar rebuild del layout
            StartCoroutine(ForceLayoutAndScrollToTop(containerRT));
        }

        // Lista para actualizar tiempo en tiempo real
        private List<(TextMeshProUGUI timeText, TournamentData tournament)> activeTournamentItems = new List<(TextMeshProUGUI, TournamentData)>();

        /// <summary>
        /// Crea una entrada de torneo por código - ESTILO SCORES NACIONAL
        /// Estructura: Participantes | Nombre Creador | Tiempo Restante
        /// </summary>
        private void CreateTournamentCard(TournamentData tournament)
        {
            if (tournamentsContainer == null)
            {
                Debug.LogError("[Tournament] Container nulo");
                return;
            }

            // Crear item por código
            GameObject itemObj = new GameObject($"TournamentItem_{tournament.tournamentId}");
            itemObj.transform.SetParent(tournamentsContainer, false);

            RectTransform itemRT = itemObj.AddComponent<RectTransform>();
            itemRT.sizeDelta = new Vector2(0, 60);

            // LayoutElement
            var layoutElement = itemObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 60;
            layoutElement.minHeight = 60;

            // Fondo según participación
            bool isParticipating = tournament.IsParticipating(currentPlayer?.userId ?? "");
            Image bg = itemObj.AddComponent<Image>();
            bg.color = isParticipating ? new Color(0f, 0.83f, 1f, 0.95f) : new Color(0.15f, 0.15f, 0.2f, 0.95f);

            // Botón para click
            Button itemButton = itemObj.AddComponent<Button>();
            itemButton.targetGraphic = bg;
            TournamentData tournamentCopy = tournament;
            itemButton.onClick.AddListener(() => OnTournamentItemClicked(tournamentCopy));

            // 1. PARTICIPANTES (izquierda - 0% a 15%)
            TextMeshProUGUI participantsText = CreateItemText(itemObj.transform, "ParticipantsText",
                $"{tournament.currentParticipants}/{tournament.maxParticipants}", 24, new Color(1f, 0.84f, 0f));
            RectTransform participantsRT = participantsText.GetComponent<RectTransform>();
            participantsRT.anchorMin = new Vector2(0, 0);
            participantsRT.anchorMax = new Vector2(0.15f, 1);
            participantsRT.offsetMin = Vector2.zero;
            participantsRT.offsetMax = Vector2.zero;
            participantsText.alignment = TextAlignmentOptions.Center;
            participantsText.fontStyle = FontStyles.Bold;

            // Divisor vertical 1 (15%)
            CreateTournamentCardDivider(itemObj.transform, 0.15f);

            // 2. NOMBRE DEL CREADOR (centro - 15% a 70%)
            TextMeshProUGUI creatorText = CreateItemText(itemObj.transform, "CreatorText",
                tournament.creatorName, 22, Color.white);
            RectTransform creatorRT = creatorText.GetComponent<RectTransform>();
            creatorRT.anchorMin = new Vector2(0.15f, 0);
            creatorRT.anchorMax = new Vector2(0.70f, 1);
            creatorRT.offsetMin = new Vector2(10, 0);
            creatorRT.offsetMax = new Vector2(-10, 0);
            creatorText.alignment = TextAlignmentOptions.Center;

            // Divisor vertical 2 (70%)
            CreateTournamentCardDivider(itemObj.transform, 0.70f);

            // 3. TIEMPO RESTANTE (derecha - 70% a 100%)
            string timeString = FormatTimeRemaining(tournament.GetTimeRemaining());
            TextMeshProUGUI timeText = CreateItemText(itemObj.transform, "TimeText",
                timeString, 22, new Color(0f, 1f, 0.53f));
            RectTransform timeRT = timeText.GetComponent<RectTransform>();
            timeRT.anchorMin = new Vector2(0.70f, 0);
            timeRT.anchorMax = new Vector2(1f, 1);
            timeRT.offsetMin = new Vector2(10, 0);
            timeRT.offsetMax = new Vector2(-10, 0);
            timeText.alignment = TextAlignmentOptions.Center;

            // Divisor horizontal
            CreateTournamentCardHorizontalDivider(itemObj.transform);

            // Guardar referencia para actualizar tiempo en tiempo real
            activeTournamentItems.Add((timeText, tournament));

            itemObj.SetActive(true);
        }

        /// <summary>
        /// Crea divisor vertical para tournament card
        /// </summary>
        private void CreateTournamentCardDivider(Transform parent, float anchorX)
        {
            GameObject divider = new GameObject("VerticalDivider");
            divider.transform.SetParent(parent, false);

            RectTransform divRT = divider.AddComponent<RectTransform>();
            divRT.anchorMin = new Vector2(anchorX, 0.1f);
            divRT.anchorMax = new Vector2(anchorX, 0.9f);
            divRT.pivot = new Vector2(0.5f, 0.5f);
            divRT.sizeDelta = new Vector2(2f, 0);

            Image divImage = divider.AddComponent<Image>();
            divImage.color = new Color(0.5f, 0.5f, 0.6f, 0.8f);
        }

        /// <summary>
        /// Crea divisor horizontal para tournament card
        /// </summary>
        private void CreateTournamentCardHorizontalDivider(Transform parent)
        {
            GameObject divider = new GameObject("HorizontalDivider");
            divider.transform.SetParent(parent, false);

            RectTransform divRT = divider.AddComponent<RectTransform>();
            divRT.anchorMin = new Vector2(0.02f, 0f);
            divRT.anchorMax = new Vector2(0.98f, 0f);
            divRT.pivot = new Vector2(0.5f, 0f);
            divRT.sizeDelta = new Vector2(0, 1f);

            Image divImage = divider.AddComponent<Image>();
            divImage.color = new Color(0.4f, 0.4f, 0.5f, 0.5f);
        }

        /// <summary>
        /// Actualiza el tiempo restante de los torneos en tiempo real
        /// </summary>
        private void Update()
        {
            // Actualizar tiempo de torneos activos cada frame
            for (int i = activeTournamentItems.Count - 1; i >= 0; i--)
            {
                var item = activeTournamentItems[i];
                if (item.timeText == null)
                {
                    activeTournamentItems.RemoveAt(i);
                    continue;
                }

                System.TimeSpan timeRemaining = item.tournament.GetTimeRemaining();
                if (timeRemaining.TotalSeconds <= 0)
                {
                    item.timeText.text = AutoLocalizer.Get("finished");
                    item.timeText.color = Color.red;
                }
                else
                {
                    item.timeText.text = FormatTimeRemaining(timeRemaining);
                }
            }
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

            // Si estamos en My Tournaments, el usuario ya participa - ir directo al leaderboard
            if (currentView == TournamentView.MyTournaments)
            {
                Debug.Log("[Tournament] Vista MyTournaments - mostrando leaderboard directamente");
                ShowTournamentLeaderboard(tournament);
                return;
            }

            // En otras vistas, verificar participación
            bool isParticipating = tournament.IsParticipating(currentPlayer?.userId ?? "");
            Debug.Log($"[Tournament] isParticipating: {isParticipating}, participantes: {tournament.participants?.Count ?? 0}");

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
        /// Muestra el panel de crear torneo (verifica premium primero)
        /// </summary>
        private void ShowCreateTournamentPanel()
        {
            Debug.Log("[Tournament] Intentando mostrar panel de creación");

            // Verificar si el usuario tiene premium para crear torneos
            if (PremiumManager.Instance != null && !PremiumManager.Instance.CanCreateTournaments)
            {
                Debug.Log("[Tournament] Usuario no tiene premium - Mostrando panel de premium requerido");
                ShowPremiumRequiredPanel();
                return;
            }

            ShowCreateTournamentPanelInternal();
        }

        /// <summary>
        /// Muestra el panel de premium requerido
        /// </summary>
        private void ShowPremiumRequiredPanel()
        {
            ClearTournamentsList();

            if (premiumRequiredPanel != null)
            {
                // Configurar textos de botones primero
                premiumRequiredPanel.SetButtonTexts(
                    AutoLocalizer.Get("get_premium"),
                    AutoLocalizer.Get("maybe_later")
                );

                // Mostrar el panel
                premiumRequiredPanel.Show(
                    AutoLocalizer.Get("premium_required_title"),
                    AutoLocalizer.Get("premium_required_message"),
                    OnGetPremiumClicked,
                    OnMaybeLaterClicked
                );
            }
            else
            {
                // Fallback si no hay panel de premium
                errorPanel?.Show(AutoLocalizer.Get("premium_required_message"));
            }
        }

        /// <summary>
        /// Acción cuando el usuario quiere obtener premium
        /// </summary>
        private void OnGetPremiumClicked()
        {
            Debug.Log("[Tournament] Usuario quiere obtener premium - Abriendo Settings");
            premiumRequiredPanel?.Hide();
            SceneManager.LoadScene("Settings");
        }

        /// <summary>
        /// Acción cuando el usuario elige "Quizás después"
        /// </summary>
        private void OnMaybeLaterClicked()
        {
            Debug.Log("[Tournament] Usuario eligió 'Quizás después'");
            premiumRequiredPanel?.Hide();
            ShowView(TournamentView.Search);
        }

        /// <summary>
        /// Muestra el panel de crear torneo (interno, ya verificado premium)
        /// </summary>
        private void ShowCreateTournamentPanelInternal()
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
                maxPlayersValue.text = $"{(int)value}/100";
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
                    durationValue.text = $"{hours}{AutoLocalizer.Get("hours_abbr")}";
                else
                {
                    int days = hours / 24;
                    durationValue.text = $"{days}{AutoLocalizer.Get("days_abbr")}";
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
                name = $"{AutoLocalizer.Get("tournament_of")} {currentPlayer.username}",
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
                ShowErrorMessage(AutoLocalizer.Get("error_creating_tournament"));
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
                ShowErrorMessage(AutoLocalizer.Get("error_joining_tournament"));
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

            // Limpiar lista de items activos (para actualización en tiempo real)
            activeTournamentItems.Clear();

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
            Debug.Log($"[Tournament] {message}");
            // Usar el mismo panel de error para mostrar éxito (con mensaje positivo)
            errorPanel?.Show(message);
        }

        /// <summary>
        /// Muestra mensaje de error
        /// </summary>
        private void ShowErrorMessage(string message)
        {
            Debug.LogError($"[Tournament] {message}");
            errorPanel?.Show(message);
        }

        /// <summary>
        /// Muestra el leaderboard del torneo
        /// </summary>
        private void ShowTournamentLeaderboard(TournamentData tournament)
        {
            Debug.Log($"[Tournament] Mostrando leaderboard del torneo: {tournament.tournamentId}");

            if (tournament == null || tournament.participants == null)
            {
                ShowErrorMessage(AutoLocalizer.Get("error_loading_tournaments"));
                return;
            }

            // Ordenar participantes por mejor tiempo
            tournament.SortParticipants();

            // Limpiar container
            ClearTournamentsList();

            // Asegurar que scrollRect.content apunte al tournamentsContainer
            RectTransform containerRT = tournamentsContainer as RectTransform;
            if (scrollRect != null && containerRT != null)
            {
                scrollRect.content = containerRT;
                // Desactivar scroll horizontal, solo permitir vertical
                scrollRect.horizontal = false;
                scrollRect.vertical = true;
            }

            // Configurar el RectTransform del container
            if (containerRT != null)
            {
                containerRT.anchorMin = new Vector2(0, 1);
                containerRT.anchorMax = new Vector2(1, 1);
                containerRT.pivot = new Vector2(0.5f, 1);
                containerRT.anchoredPosition = Vector2.zero;
                containerRT.sizeDelta = new Vector2(0, 0); // Ancho 0 = usar anclas, alto controlado por ContentSizeFitter
            }

            // Configurar el VerticalLayoutGroup del container
            var layoutGroup = tournamentsContainer.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = tournamentsContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            }
            layoutGroup.spacing = 5f;
            layoutGroup.padding = new RectOffset(5, 5, 5, 5);
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = true;  // FORZAR expansión horizontal
            layoutGroup.childForceExpandHeight = false;

            // ContentSizeFitter para que el contenedor se ajuste al contenido
            var sizeFitter = tournamentsContainer.GetComponent<ContentSizeFitter>();
            if (sizeFitter == null)
            {
                sizeFitter = tournamentsContainer.gameObject.AddComponent<ContentSizeFitter>();
            }
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

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

            // Forzar rebuild del layout y scroll al top
            StartCoroutine(ForceLayoutAndScrollToTop(containerRT));

            Debug.Log($"[Tournament] Leaderboard mostrado con {tournament.participants.Count} participantes");
        }

        /// <summary>
        /// Fuerza el rebuild del layout y hace scroll al top (en el siguiente frame)
        /// </summary>
        private System.Collections.IEnumerator ForceLayoutAndScrollToTop(RectTransform containerRT)
        {
            yield return null; // Esperar un frame

            if (containerRT != null)
            {
                // Forzar posición arriba
                containerRT.anchoredPosition = Vector2.zero;

                LayoutRebuilder.ForceRebuildLayoutImmediate(containerRT);
                Canvas.ForceUpdateCanvases();
            }

            yield return null; // Esperar otro frame

            // Forzar posición de nuevo
            if (containerRT != null)
            {
                containerRT.anchoredPosition = Vector2.zero;
            }

            if (scrollRect != null && scrollRect.content != null)
            {
                scrollRect.StopMovement();
                scrollRect.verticalNormalizedPosition = 1f; // Scroll al top

                // Forzar la posición del content
                if (scrollRect.content != null)
                {
                    scrollRect.content.anchoredPosition = Vector2.zero;
                }
            }

            yield return null; // Un frame más para asegurar

            if (containerRT != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(containerRT);
            }
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
                tournament.name, 32, Color.white);
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
            string participantsLabel = AutoLocalizer.Get("participants");
            string timeRemainingLabel = AutoLocalizer.Get("time_remaining");
            TextMeshProUGUI infoText = CreateItemText(titleObj.transform, "InfoText",
                $"{participantsLabel}: {tournament.currentParticipants}/{tournament.maxParticipants} | {timeRemainingLabel} {timeRemaining}",
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
        /// Estructura: Posición | Username | Tiempo (con divisores verticales)
        /// </summary>
        private void CreateLeaderboardItem(int position, ParticipantScore participant)
        {
            if (tournamentsContainer == null) return;

            GameObject itemObj = new GameObject($"LeaderboardItem_{position}");
            itemObj.transform.SetParent(tournamentsContainer, false);

            RectTransform itemRT = itemObj.AddComponent<RectTransform>();
            itemRT.sizeDelta = new Vector2(0, 60);

            // LayoutElement - NO usar flexibleWidth, dejar que el LayoutGroup controle
            var layoutElement = itemObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 60;
            layoutElement.minHeight = 60;

            // Verificar si es el jugador actual
            bool isCurrentPlayer = participant.userId == (currentPlayer?.userId ?? "");

            // Fondo - EXACTAMENTE como en LeaderboardManager
            Image bg = itemObj.AddComponent<Image>();
            if (isCurrentPlayer)
            {
                bg.color = new Color(0f, 0.83f, 1f, 0.95f); // Azul eléctrico
            }
            else
            {
                bg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f); // Gris oscuro
            }

            // Colores de posición (medallas)
            Color posColor;
            if (position == 1)
                posColor = new Color(1f, 0.84f, 0f); // Oro
            else if (position == 2)
                posColor = new Color(0.75f, 0.75f, 0.75f); // Plata
            else if (position == 3)
                posColor = new Color(0.8f, 0.5f, 0.2f); // Bronce
            else
                posColor = new Color(1f, 0.84f, 0f); // Amarillo

            // 1. POSICIÓN (izquierda) - 15% del ancho
            TextMeshProUGUI posText = CreateItemText(itemObj.transform, "PositionText", $"{position}", 28, posColor);
            RectTransform posRT = posText.GetComponent<RectTransform>();
            posRT.anchorMin = new Vector2(0, 0);
            posRT.anchorMax = new Vector2(0.15f, 1);
            posRT.offsetMin = Vector2.zero;
            posRT.offsetMax = Vector2.zero;
            posText.alignment = TextAlignmentOptions.Center;
            posText.fontStyle = FontStyles.Bold;

            // Divisor vertical 1 (15%)
            CreateLeaderboardVerticalDivider(itemObj.transform, 0.15f);

            // 2. USERNAME (centro) - del 15% al 70%
            TextMeshProUGUI nameText = CreateItemText(itemObj.transform, "NameText", participant.username, 22, Color.white);
            RectTransform nameRT = nameText.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0.15f, 0);
            nameRT.anchorMax = new Vector2(0.70f, 1);
            nameRT.offsetMin = new Vector2(5, 0);
            nameRT.offsetMax = new Vector2(-5, 0);
            nameText.alignment = TextAlignmentOptions.Center;

            // Divisor vertical 2 (70%)
            CreateLeaderboardVerticalDivider(itemObj.transform, 0.70f);

            // 3. TIEMPO (derecha) - del 70% al 100%
            string timeString = participant.bestTime == float.MaxValue ?
                AutoLocalizer.Get("no_time") : $"{participant.bestTime:F3}s";

            TextMeshProUGUI timeText = CreateItemText(itemObj.transform, "TimeText", timeString, 22, new Color(0f, 1f, 0.53f));
            RectTransform timeRT = timeText.GetComponent<RectTransform>();
            timeRT.anchorMin = new Vector2(0.70f, 0);
            timeRT.anchorMax = new Vector2(1f, 1);
            timeRT.offsetMin = new Vector2(5, 0);
            timeRT.offsetMax = new Vector2(-5, 0);
            timeText.alignment = TextAlignmentOptions.Center;

            // Línea divisoria horizontal (entre entradas)
            CreateLeaderboardHorizontalDivider(itemObj.transform);

            itemObj.SetActive(true);
        }

        /// <summary>
        /// Crea un divisor vertical usando anclas relativas (porcentaje)
        /// </summary>
        private void CreateLeaderboardVerticalDivider(Transform parent, float anchorX)
        {
            GameObject divider = new GameObject("VerticalDivider");
            divider.transform.SetParent(parent, false);

            RectTransform divRT = divider.AddComponent<RectTransform>();
            divRT.anchorMin = new Vector2(anchorX, 0.1f);
            divRT.anchorMax = new Vector2(anchorX, 0.9f);
            divRT.pivot = new Vector2(0.5f, 0.5f);
            divRT.sizeDelta = new Vector2(2f, 0);

            Image divImage = divider.AddComponent<Image>();
            divImage.color = new Color(0.5f, 0.5f, 0.6f, 0.8f);
        }

        /// <summary>
        /// Crea un divisor horizontal para el leaderboard
        /// </summary>
        private void CreateLeaderboardHorizontalDivider(Transform parent)
        {
            GameObject divider = new GameObject("HorizontalDivider");
            divider.transform.SetParent(parent, false);

            RectTransform divRT = divider.AddComponent<RectTransform>();
            divRT.anchorMin = new Vector2(0.02f, 0f);
            divRT.anchorMax = new Vector2(0.98f, 0f);
            divRT.pivot = new Vector2(0.5f, 0f);
            divRT.sizeDelta = new Vector2(0, 1f);

            Image divImage = divider.AddComponent<Image>();
            divImage.color = new Color(0.4f, 0.4f, 0.5f, 0.5f);
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

            if (exitConfirmPanel == null)
            {
                Debug.LogError("[Tournament] exitConfirmPanel no está asignado");
                return;
            }

            // Mostrar panel de confirmación usando el prefab
            string title = AutoLocalizer.Get("exit_confirm_title");
            string message = AutoLocalizer.Get("exit_confirm_message");

            exitConfirmPanel.SetButtonTexts(
                AutoLocalizer.Get("confirm"),
                AutoLocalizer.Get("cancel")
            );

            exitConfirmPanel.Show(title, message, OnConfirmExitClicked, null);
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
            exitConfirmPanel?.Hide();

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
                ShowErrorMessage(AutoLocalizer.Get("error_leaving_tournament"));
            }
            finally
            {
                ShowLoading(false);
            }
        }

        #endregion

        #region Utility

        /// <summary>
        /// Remueve el comportamiento de botón de un panel (evita animación de clic)
        /// </summary>
        private void RemovePanelButtonBehavior(GameObject panel)
        {
            if (panel == null) return;

            // Remover Button si existe (no debería estar en un panel)
            var button = panel.GetComponent<Button>();
            if (button != null)
            {
                Destroy(button);
                Debug.Log($"[Tournament] Removido Button de {panel.name}");
            }

            // Remover Selectable si existe y no es necesario
            var selectable = panel.GetComponent<Selectable>();
            if (selectable != null && !(selectable is Button) && !(selectable is Toggle) &&
                !(selectable is Slider) && !(selectable is InputField) && !(selectable is TMP_InputField) &&
                !(selectable is Dropdown) && !(selectable is TMP_Dropdown) && !(selectable is Scrollbar))
            {
                selectable.enabled = false;
                Debug.Log($"[Tournament] Deshabilitado Selectable de {panel.name}");
            }

            // Verificar si tiene Image con raycastTarget que puede interferir
            var image = panel.GetComponent<Image>();
            if (image != null)
            {
                // Asegurar que no tenga transición de color
                image.type = Image.Type.Sliced;
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
