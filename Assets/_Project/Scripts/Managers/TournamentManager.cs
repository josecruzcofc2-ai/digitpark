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
        [SerializeField] public TMP_InputField maxHoursInput;
        [SerializeField] public Button applyButton;
        [SerializeField] public Button clearButton;

        [Header("Tournaments List")]
        [SerializeField] public Transform tournamentsContainer;
        [SerializeField] public ScrollRect scrollRect;

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

        // Estado
        private TournamentView currentView = TournamentView.Search;
        private PlayerData currentPlayer;
        private List<TournamentData> activeTournaments = new List<TournamentData>();
        private List<TournamentData> filteredTournaments = new List<TournamentData>();
        private List<TournamentData> myTournaments = new List<TournamentData>();
        private TournamentData selectedTournament;

        // Filtros de búsqueda
        private string searchUsername = "";
        private int searchMinPlayers = 0;
        private int searchMaxPlayers = 999;
        private float searchMinHours = 0;
        private float searchMaxHours = 999;

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
        }

        /// <summary>
        /// Aplica los filtros de búsqueda
        /// </summary>
        private void ApplySearchFilters()
        {
            Debug.Log("[Tournament] Aplicando filtros de búsqueda");

            // Leer filtros
            searchUsername = usernameSearchInput?.text ?? "";

            int.TryParse(minPlayersInput?.text, out searchMinPlayers);
            int.TryParse(maxPlayersInput?.text, out searchMaxPlayers);

            float.TryParse(minHoursInput?.text, out searchMinHours);
            float.TryParse(maxHoursInput?.text, out searchMaxHours);

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

            // Resetear valores
            searchUsername = "";
            searchMinPlayers = 0;
            searchMaxPlayers = 999;
            searchMinHours = 0;
            searchMaxHours = 999;

            // Mostrar todos
            FilterTournaments();
        }

        /// <summary>
        /// Filtra los torneos según los criterios
        /// </summary>
        private void FilterTournaments()
        {
            filteredTournaments = new List<TournamentData>(activeTournaments);

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
        /// Crea una entrada de torneo (TournamentItem)
        /// Estructura: Participants | Divider1 | CreatorText | Divider2 | TimeText
        /// </summary>
        private void CreateTournamentCard(TournamentData tournament)
        {
            if (tournamentsContainer == null) return;

            // Crear TournamentItem programáticamente
            GameObject itemObj = new GameObject($"TournamentItem_{tournament.tournamentId}");
            itemObj.transform.SetParent(tournamentsContainer, false);

            RectTransform itemRT = itemObj.AddComponent<RectTransform>();
            itemRT.anchorMin = new Vector2(0, 1);
            itemRT.anchorMax = new Vector2(1, 1);
            itemRT.pivot = new Vector2(0.5f, 1);
            itemRT.anchoredPosition = Vector2.zero;
            itemRT.sizeDelta = new Vector2(0, 80); // Altura 80

            // LayoutElement
            var layoutElement = itemObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 80;
            layoutElement.minHeight = 80;

            // Fondo
            Image bg = itemObj.AddComponent<Image>();
            bool isParticipating = tournament.IsParticipating(currentPlayer?.userId ?? "");
            bg.color = isParticipating ? new Color(0f, 0.83f, 1f, 0.3f) : new Color(0.15f, 0.15f, 0.2f, 0.95f);

            // Crear divisores verticales
            CreateVerticalDivider(itemObj.transform, 150f);  // Después de Participants
            CreateVerticalDivider(itemObj.transform, 670f);  // Después de CreatorText

            // Participants (izquierda) - "20/55"
            TextMeshProUGUI participantsText = CreateItemText(itemObj.transform, "ParticipantsText",
                $"{tournament.currentParticipants}/{tournament.maxParticipants}", 28, Color.white);
            RectTransform participantsRT = participantsText.GetComponent<RectTransform>();
            participantsRT.anchorMin = new Vector2(0, 0);
            participantsRT.anchorMax = new Vector2(0, 1);
            participantsRT.pivot = new Vector2(0, 0.5f);
            participantsRT.anchoredPosition = new Vector2(20, 0);
            participantsRT.sizeDelta = new Vector2(130, 0);
            participantsText.alignment = TextAlignmentOptions.Center;

            // CreatorText (centro) - "NombreUsuario"
            TextMeshProUGUI creatorText = CreateItemText(itemObj.transform, "CreatorText",
                tournament.creatorName, 26, Color.white);
            RectTransform creatorRT = creatorText.GetComponent<RectTransform>();
            creatorRT.anchorMin = new Vector2(0, 0);
            creatorRT.anchorMax = new Vector2(0, 1);
            creatorRT.pivot = new Vector2(0, 0.5f);
            creatorRT.anchoredPosition = new Vector2(160f, 0);
            creatorRT.sizeDelta = new Vector2(500, 0);
            creatorText.alignment = TextAlignmentOptions.Center;

            // TimeText (derecha) - "23:45:00"
            string timeString = FormatTimeRemaining(tournament.GetTimeRemaining());
            TextMeshProUGUI timeText = CreateItemText(itemObj.transform, "TimeText",
                timeString, 26, new Color(0f, 1f, 0.53f)); // Verde brillante
            RectTransform timeRT = timeText.GetComponent<RectTransform>();
            timeRT.anchorMin = new Vector2(1, 0);
            timeRT.anchorMax = new Vector2(1, 1);
            timeRT.pivot = new Vector2(1, 0.5f);
            timeRT.anchoredPosition = new Vector2(-20, 0);
            timeRT.sizeDelta = new Vector2(350, 0);
            timeText.alignment = TextAlignmentOptions.Center;

            // Línea divisoria horizontal
            CreateHorizontalDivider(itemObj.transform);

            itemObj.SetActive(true);
        }

        /// <summary>
        /// Crea un divisor vertical
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
        /// Crea un divisor horizontal
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
        /// Crea un texto para el item
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
        /// Crea un nuevo torneo
        /// </summary>
        private void OnCreateTournamentClicked()
        {
            if (currentPlayer == null) return;

            // Obtener valores de los sliders
            int maxParticipants = (int)(maxPlayersSlider?.value ?? 50);
            int durationHours = (int)(durationSlider?.value ?? 24);
            bool isPublic = publicToggle?.isOn ?? true;

            // Mostrar popup de confirmación
            ShowConfirmPopup(maxParticipants, durationHours, isPublic);
        }

        /// <summary>
        /// Muestra popup de confirmación para crear torneo
        /// </summary>
        private void ShowConfirmPopup(int maxPlayers, int durationHours, bool isPublic)
        {
            if (confirmPopup == null || blockerPanel == null) return;

            // Calcular info
            string durationText = durationHours < 24 ? $"{durationHours}h" : $"{durationHours / 24}d";
            string typeText = isPublic ? "Público" : "Privado";

            // Configurar textos
            if (messageText != null)
                messageText.text = "¿Confirmas la creación del torneo?";

            if (tournamentInfoText != null)
                tournamentInfoText.text = $"Jugadores: {maxPlayers}/55\nDuración: {durationText}\nTipo: {typeText}";

            // Mostrar popup
            confirmPopup.SetActive(true);
        }

        /// <summary>
        /// Oculta popup de confirmación
        /// </summary>
        private void HideConfirmPopup()
        {
            if (confirmPopup != null)
                confirmPopup.SetActive(false);
        }

        /// <summary>
        /// Confirma la creación del torneo
        /// </summary>
        private async void OnConfirmClicked()
        {
            if (currentPlayer == null) return;

            // Obtener valores
            int maxParticipants = (int)(maxPlayersSlider?.value ?? 50);
            int durationHours = (int)(durationSlider?.value ?? 24);
            bool isPublic = publicToggle?.isOn ?? true;

            // Ocultar popup
            HideConfirmPopup();

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
                    Debug.Log($"[Tournament] Torneo creado exitosamente");

                    // Analytics
                    AnalyticsService.Instance?.LogTournamentCreated(newTournament.tournamentId, 0);

                    // Mostrar mensaje de éxito
                    ShowSuccessMessage("¡Torneo creado exitosamente!");

                    // Ocultar panel de creación y volver a Search (después de 1.5 segundos)
                    await System.Threading.Tasks.Task.Delay(1500);
                    HideCreatePanel();
                }
                else
                {
                    Debug.LogError("[Tournament] Error al crear torneo");
                    ShowErrorMessage("No se pudo crear el torneo. Intenta nuevamente.");
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
            text.text = currentView == TournamentView.Search ? "No hay torneos activos" : "No participas en ningún torneo";
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
        Search,
        MyTournaments,
        Create
    }
}
