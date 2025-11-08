using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DigitPark.Services.Firebase;
using DigitPark.Data;
using DigitPark.UI;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager de la escena de Scores/Rankings
    /// Muestra tablas de clasificación: Personal, Local (país) y Global
    /// Usa UIFactory para colores (BrightGreen, NeonYellow, etc.)
    /// </summary>
    public class LeaderboardManager : MonoBehaviour
    {
        [Header("Tabs")]
        [SerializeField] public Button personalTabButton;
        [SerializeField] public Button localTabButton;
        [SerializeField] public Button globalTabButton;

        [Header("Leaderboard UI")]
        [SerializeField] public Transform leaderboardContainer;
        [SerializeField] public GameObject leaderboardEntryPrefab;
        [SerializeField] public ScrollRect scrollRect;

        [Header("Loading")]
        [SerializeField] public GameObject loadingPanel;
        [SerializeField] public TextMeshProUGUI loadingText;

        [Header("Player Highlight")]
        [SerializeField] public GameObject playerHighlightPanel;
        [SerializeField] public TextMeshProUGUI playerPositionText;
        [SerializeField] public TextMeshProUGUI playerTimeText;

        [Header("Navigation")]
        [SerializeField] public Button backButton;

        // Estado
        private LeaderboardTab currentTab = LeaderboardTab.Personal;
        private PlayerData currentPlayer;

        // Datos
        private List<LeaderboardEntry> personalScores;
        private List<LeaderboardEntry> localScores;
        private List<LeaderboardEntry> globalScores;

        private void Start()
        {
            Debug.Log("[Leaderboard] LeaderboardManager iniciado");

            // Verificar e inicializar servicios si no existen (para testing directo)
            EnsureServicesExist();

            // Obtener datos del jugador
            LoadPlayerData();

            // Configurar listeners
            SetupListeners();

            // Cargar leaderboard inicial
            LoadLeaderboard(LeaderboardTab.Personal);
        }

        /// <summary>
        /// Asegura que los servicios existan (para testing directo de escena)
        /// </summary>
        private void EnsureServicesExist()
        {
            if (AuthenticationService.Instance == null)
            {
                Debug.LogWarning("[Leaderboard] AuthenticationService no encontrado, creando instancia de respaldo...");
                GameObject authService = new GameObject("AuthenticationService");
                authService.AddComponent<AuthenticationService>();
            }

            if (DatabaseService.Instance == null)
            {
                Debug.LogWarning("[Leaderboard] DatabaseService no encontrado, creando instancia de respaldo...");
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
                    Debug.LogError("[Leaderboard] No hay datos del jugador");
                    SceneManager.LoadScene("Login");
                }
            }
        }

        /// <summary>
        /// Configura los listeners de los botones
        /// </summary>
        private void SetupListeners()
        {
            personalTabButton?.onClick.AddListener(() => OnTabSelected(LeaderboardTab.Personal));
            localTabButton?.onClick.AddListener(() => OnTabSelected(LeaderboardTab.Local));
            globalTabButton?.onClick.AddListener(() => OnTabSelected(LeaderboardTab.Global));
            backButton?.onClick.AddListener(OnBackButtonClicked);
        }

        #endregion

        #region Tab Navigation

        /// <summary>
        /// Cuando se selecciona una tab
        /// </summary>
        private void OnTabSelected(LeaderboardTab tab)
        {
            if (currentTab == tab) return;

            Debug.Log($"[Leaderboard] Tab seleccionada: {tab}");

            currentTab = tab;
            UpdateTabVisuals();
            LoadLeaderboard(tab);
        }

        /// <summary>
        /// Actualiza los visuales de las tabs
        /// </summary>
        private void UpdateTabVisuals()
        {
            // Resetear todos los botones
            SetTabButtonState(personalTabButton, currentTab == LeaderboardTab.Personal);
            SetTabButtonState(localTabButton, currentTab == LeaderboardTab.Local);
            SetTabButtonState(globalTabButton, currentTab == LeaderboardTab.Global);
        }

        /// <summary>
        /// Establece el estado visual de un botón de tab
        /// </summary>
        private void SetTabButtonState(Button button, bool isSelected)
        {
            if (button == null) return;

            ColorBlock colors = button.colors;
            if (isSelected)
            {
                colors.normalColor = new Color(0f, 0.83f, 1f); // Azul eléctrico
            }
            else
            {
                colors.normalColor = Color.gray;
            }
            button.colors = colors;
        }

        #endregion

        #region Load Leaderboards

        /// <summary>
        /// Carga el leaderboard según la tab seleccionada
        /// </summary>
        private async void LoadLeaderboard(LeaderboardTab tab)
        {
            ShowLoading(true, "Cargando rankings...");

            // Limpiar leaderboard actual
            ClearLeaderboard();

            List<LeaderboardEntry> entries = null;

            try
            {
                switch (tab)
                {
                    case LeaderboardTab.Personal:
                        entries = await LoadPersonalScores();
                        break;

                    case LeaderboardTab.Local:
                        entries = await LoadLocalScores();
                        break;

                    case LeaderboardTab.Global:
                        entries = await LoadGlobalScores();
                        break;
                }

                if (entries != null && entries.Count > 0)
                {
                    DisplayLeaderboard(entries);
                }
                else
                {
                    ShowEmptyMessage();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Leaderboard] Error al cargar: {ex.Message}");
                ShowErrorMessage("Error al cargar rankings");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        /// <summary>
        /// Carga las puntuaciones personales del jugador
        /// </summary>
        private async System.Threading.Tasks.Task<List<LeaderboardEntry>> LoadPersonalScores()
        {
            Debug.Log("[Leaderboard] Cargando puntuaciones personales...");

            if (currentPlayer == null) return new List<LeaderboardEntry>();

            // Convertir el historial de scores a LeaderboardEntry
            List<LeaderboardEntry> entries = new List<LeaderboardEntry>();

            foreach (var score in currentPlayer.scoreHistory)
            {
                entries.Add(new LeaderboardEntry
                {
                    userId = currentPlayer.userId,
                    username = currentPlayer.username,
                    time = score.time,
                    countryCode = currentPlayer.countryCode,
                    avatarUrl = currentPlayer.avatarUrl
                });
            }

            // Ordenar por mejor tiempo
            entries.Sort((a, b) => a.time.CompareTo(b.time));

            // Asignar posiciones
            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].position = i + 1;
            }

            personalScores = entries;
            return entries;
        }

        /// <summary>
        /// Carga las puntuaciones locales (país)
        /// </summary>
        private async System.Threading.Tasks.Task<List<LeaderboardEntry>> LoadLocalScores()
        {
            Debug.Log("[Leaderboard] Cargando puntuaciones locales...");

            if (currentPlayer == null) return new List<LeaderboardEntry>();

            // Obtener leaderboard del país desde Firebase
            localScores = await DatabaseService.Instance.GetCountryLeaderboard(currentPlayer.countryCode, 100);

            // Asignar posiciones
            for (int i = 0; i < localScores.Count; i++)
            {
                localScores[i].position = i + 1;
            }

            return localScores;
        }

        /// <summary>
        /// Carga las puntuaciones globales
        /// </summary>
        private async System.Threading.Tasks.Task<List<LeaderboardEntry>> LoadGlobalScores()
        {
            Debug.Log("[Leaderboard] Cargando puntuaciones globales...");

            // Obtener leaderboard global desde Firebase
            globalScores = await DatabaseService.Instance.GetGlobalLeaderboard(200);

            // Asignar posiciones
            for (int i = 0; i < globalScores.Count; i++)
            {
                globalScores[i].position = i + 1;
            }

            return globalScores;
        }

        #endregion

        #region Display Leaderboard

        /// <summary>
        /// Muestra el leaderboard en la UI
        /// </summary>
        private void DisplayLeaderboard(List<LeaderboardEntry> entries)
        {
            Debug.Log($"[Leaderboard] Mostrando {entries.Count} entradas");

            foreach (var entry in entries)
            {
                CreateLeaderboardEntry(entry);
            }

            // Resaltar posición del jugador si está en la lista
            HighlightPlayerPosition(entries);

            // Scroll al top
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        /// <summary>
        /// Crea una entrada visual en el leaderboard
        /// </summary>
        private void CreateLeaderboardEntry(LeaderboardEntry entry)
        {
            if (leaderboardContainer == null) return;

            // Crear entrada programáticamente (sin prefab)
            GameObject entryObj = new GameObject($"Entry_{entry.position}");
            entryObj.transform.SetParent(leaderboardContainer, false);

            RectTransform entryRT = entryObj.AddComponent<RectTransform>();
            entryRT.sizeDelta = new Vector2(1040, 80); // Ancho completo, altura fija
            entryRT.anchorMin = new Vector2(0, 1);
            entryRT.anchorMax = new Vector2(0, 1);
            entryRT.pivot = new Vector2(0, 1);

            // Fondo de la entrada
            Image bg = entryObj.AddComponent<Image>();
            bool isCurrentPlayer = currentPlayer != null && entry.userId == currentPlayer.userId;

            if (isCurrentPlayer)
            {
                bg.color = new Color(0f, 0.83f, 1f, 0.2f); // Azul eléctrico translúcido
            }
            else
            {
                bg.color = new Color(0.15f, 0.15f, 0.2f, 0.5f); // Gris oscuro
            }

            // Si es modo Personal, NO mostrar el TOP#
            bool isPersonalTab = currentTab == LeaderboardTab.Personal;

            float leftPadding = 20f;
            float dividerWidth = 1f;

            if (isPersonalTab)
            {
                // MODO PERSONAL: Solo Nombre y Tiempo
                // Crear divisor vertical en el centro
                CreateVerticalDivider(entryObj.transform, 520f); // Centro exacto

                // Nombre (izquierda)
                TextMeshProUGUI nameText = CreateEntryText(entryObj.transform, "NameText", entry.username, 28, Color.white);
                RectTransform nameRT = nameText.GetComponent<RectTransform>();
                nameRT.anchorMin = new Vector2(0, 0);
                nameRT.anchorMax = new Vector2(0.5f, 1);
                nameRT.pivot = new Vector2(0.5f, 0.5f);
                nameRT.anchoredPosition = Vector2.zero;
                nameRT.sizeDelta = Vector2.zero;
                nameText.alignment = TMPro.TextAlignmentOptions.Center;

                // Tiempo (derecha)
                TextMeshProUGUI timeText = CreateEntryText(entryObj.transform, "TimeText", $"{entry.time:F3}s", 28, new Color(0f, 1f, 0.53f)); // BrightGreen
                RectTransform timeRT = timeText.GetComponent<RectTransform>();
                timeRT.anchorMin = new Vector2(0.5f, 0);
                timeRT.anchorMax = new Vector2(1, 1);
                timeRT.pivot = new Vector2(0.5f, 0.5f);
                timeRT.anchoredPosition = Vector2.zero;
                timeRT.sizeDelta = Vector2.zero;
                timeText.alignment = TMPro.TextAlignmentOptions.Center;
            }
            else
            {
                // MODO LOCAL/GLOBAL: TOP#, Nombre y Tiempo (3 columnas)
                // Divisores verticales
                CreateVerticalDivider(entryObj.transform, 150f); // Después del TOP#
                CreateVerticalDivider(entryObj.transform, 670f); // Después del Nombre

                // TOP# (izquierda)
                Color positionColor = entry.position <= 3 ? GetMedalColor(entry.position) : new Color(1f, 0.84f, 0f); // NeonYellow
                TextMeshProUGUI posText = CreateEntryText(entryObj.transform, "PositionText", $"{entry.position}", 32, positionColor);
                RectTransform posRT = posText.GetComponent<RectTransform>();
                posRT.anchorMin = new Vector2(0, 0);
                posRT.anchorMax = new Vector2(0, 1);
                posRT.pivot = new Vector2(0, 0.5f);
                posRT.anchoredPosition = new Vector2(leftPadding, 0);
                posRT.sizeDelta = new Vector2(130, 0);
                posText.alignment = TMPro.TextAlignmentOptions.Center;
                posText.fontStyle = TMPro.FontStyles.Bold;

                // Nombre (centro)
                TextMeshProUGUI nameText = CreateEntryText(entryObj.transform, "NameText", entry.username, 26, Color.white);
                RectTransform nameRT = nameText.GetComponent<RectTransform>();
                nameRT.anchorMin = new Vector2(0, 0);
                nameRT.anchorMax = new Vector2(0, 1);
                nameRT.pivot = new Vector2(0, 0.5f);
                nameRT.anchoredPosition = new Vector2(160f, 0);
                nameRT.sizeDelta = new Vector2(500, 0);
                nameText.alignment = TMPro.TextAlignmentOptions.Center;

                // Tiempo (derecha)
                TextMeshProUGUI timeText = CreateEntryText(entryObj.transform, "TimeText", $"{entry.time:F3}s", 26, new Color(0f, 1f, 0.53f)); // BrightGreen
                RectTransform timeRT = timeText.GetComponent<RectTransform>();
                timeRT.anchorMin = new Vector2(1, 0);
                timeRT.anchorMax = new Vector2(1, 1);
                timeRT.pivot = new Vector2(1, 0.5f);
                timeRT.anchoredPosition = new Vector2(-20, 0);
                timeRT.sizeDelta = new Vector2(350, 0);
                timeText.alignment = TMPro.TextAlignmentOptions.Center;
            }

            // Línea divisoria horizontal sutil (entre entradas)
            CreateHorizontalDivider(entryObj.transform);
        }

        /// <summary>
        /// Crea un divisor vertical sutil
        /// </summary>
        private void CreateVerticalDivider(Transform parent, float xPosition)
        {
            GameObject divider = new GameObject("VerticalDivider");
            divider.transform.SetParent(parent, false);

            RectTransform divRT = divider.AddComponent<RectTransform>();
            divRT.anchorMin = new Vector2(0, 0);
            divRT.anchorMax = new Vector2(0, 1);
            divRT.pivot = new Vector2(0.5f, 0.5f);
            divRT.anchoredPosition = new Vector2(xPosition, 0);
            divRT.sizeDelta = new Vector2(1f, -20); // 1px de ancho, con padding vertical

            Image divImage = divider.AddComponent<Image>();
            divImage.color = new Color(0.3f, 0.3f, 0.4f, 0.5f); // Gris sutil
        }

        /// <summary>
        /// Crea un divisor horizontal sutil
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
            divRT.sizeDelta = new Vector2(-40, 1f); // Ancho casi completo, 1px de alto

            Image divImage = divider.AddComponent<Image>();
            divImage.color = new Color(0.3f, 0.3f, 0.4f, 0.3f); // Gris muy sutil
        }

        /// <summary>
        /// Crea un texto para una entrada del leaderboard
        /// </summary>
        private TextMeshProUGUI CreateEntryText(Transform parent, string name, string text, int fontSize, Color color)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.fontStyle = TMPro.FontStyles.Normal;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TMPro.TextOverflowModes.Ellipsis;

            return tmp;
        }

        /// <summary>
        /// Obtiene el color de medalla según la posición
        /// </summary>
        private Color GetMedalColor(int position)
        {
            switch (position)
            {
                case 1: return new Color(1f, 0.84f, 0f); // Oro
                case 2: return new Color(0.75f, 0.75f, 0.75f); // Plata
                case 3: return new Color(0.8f, 0.5f, 0.2f); // Bronce
                default: return Color.white;
            }
        }

        /// <summary>
        /// Resalta la posición del jugador actual
        /// </summary>
        private void HighlightPlayerPosition(List<LeaderboardEntry> entries)
        {
            if (currentPlayer == null || playerHighlightPanel == null) return;

            var playerEntry = entries.Find(e => e.userId == currentPlayer.userId);

            if (playerEntry != null)
            {
                playerHighlightPanel.SetActive(true);

                if (playerPositionText != null)
                {
                    playerPositionText.text = $"Tu posición: #{playerEntry.position}";
                }

                if (playerTimeText != null)
                {
                    playerTimeText.text = $"Mejor tiempo: {playerEntry.time:F3}s";
                }
            }
            else
            {
                playerHighlightPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Limpia el leaderboard actual
        /// </summary>
        private void ClearLeaderboard()
        {
            if (leaderboardContainer == null) return;

            foreach (Transform child in leaderboardContainer)
            {
                Destroy(child.gameObject);
            }
        }

        #endregion

        #region UI Helpers

        /// <summary>
        /// Muestra u oculta el panel de carga
        /// </summary>
        private void ShowLoading(bool show, string message = "Cargando...")
        {
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(show);
            }

            if (loadingText != null && show)
            {
                loadingText.text = message;
            }
        }

        /// <summary>
        /// Muestra mensaje cuando no hay datos
        /// </summary>
        private void ShowEmptyMessage()
        {
            Debug.Log("[Leaderboard] No hay datos para mostrar");

            GameObject emptyMsg = new GameObject("EmptyMessage");
            emptyMsg.transform.SetParent(leaderboardContainer);

            Text text = emptyMsg.AddComponent<Text>();
            text.text = "No hay puntuaciones aún";
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 24;
            text.color = Color.gray;

            RectTransform rt = emptyMsg.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(400, 100);
        }

        /// <summary>
        /// Muestra mensaje de error
        /// </summary>
        private void ShowErrorMessage(string message)
        {
            Debug.LogError($"[Leaderboard] {message}");

            GameObject errorMsg = new GameObject("ErrorMessage");
            errorMsg.transform.SetParent(leaderboardContainer);

            Text text = errorMsg.AddComponent<Text>();
            text.text = message;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 24;
            text.color = Color.red;

            RectTransform rt = errorMsg.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(400, 100);
        }

        #endregion

        #region Navigation

        /// <summary>
        /// Vuelve al menú principal
        /// </summary>
        private void OnBackButtonClicked()
        {
            Debug.Log("[Leaderboard] Volviendo al menú principal");
            SceneManager.LoadScene("MainMenu");
        }

        #endregion
    }

    /// <summary>
    /// Tipos de tab del leaderboard
    /// </summary>
    public enum LeaderboardTab
    {
        Personal,
        Local,
        Global
    }
}
