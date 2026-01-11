using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DigitPark.Services.Firebase;
using DigitPark.Data;
using DigitPark.UI;
using DigitPark.Localization;
using DigitPark.UI.Items;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager de la escena de Scores/Rankings
    /// Muestra tablas de clasificación: Nacional (país) y Mundial (global)
    /// </summary>
    public class LeaderboardManager : MonoBehaviour
    {
        [Header("Tabs")]
        [SerializeField] public Button nacionalTab;
        [SerializeField] public Button mundialTab;

        [Header("Leaderboard UI")]
        [SerializeField] public Transform leaderboardContainer;
        [SerializeField] public GameObject leaderboardEntryPrefab;
        [SerializeField] public ScrollRect scrollRect;

        [Header("Loading")]
        [SerializeField] public GameObject loadingPanel;
        [SerializeField] public TextMeshProUGUI loadingText;

        [Header("Navigation")]
        [SerializeField] public Button backButton;

        [Header("Empty State")]
        [SerializeField] private GameObject emptyState;
        [SerializeField] private Button playButton;

        [Header("Player Position Panel")]
        [SerializeField] private GameObject playerPositionPanel;
        [SerializeField] private TextMeshProUGUI positionNumberText;
        [SerializeField] private TextMeshProUGUI positionTimeText;

        // Estado
        private LeaderboardTab currentTab = LeaderboardTab.Local;
        private PlayerData currentPlayer;

        // Datos
        private List<LeaderboardEntry> localScores;
        private List<LeaderboardEntry> globalScores;

        private async void Start()
        {
            Debug.Log("[Leaderboard] LeaderboardManager iniciado");

            // Verificar e inicializar servicios si no existen (para testing directo)
            EnsureServicesExist();

            // Configurar listeners primero
            SetupListeners();

            // Obtener datos del jugador (async)
            await LoadPlayerDataAsync();

            // Cargar leaderboard inicial (Nacional por defecto)
            LoadLeaderboard(LeaderboardTab.Local);
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
        /// Carga los datos del jugador de forma asíncrona
        /// </summary>
        private async System.Threading.Tasks.Task LoadPlayerDataAsync()
        {
            if (AuthenticationService.Instance != null)
            {
                currentPlayer = AuthenticationService.Instance.GetCurrentPlayerData();

                if (currentPlayer == null)
                {
                    Debug.LogError("[Leaderboard] No hay datos del jugador");
                    SceneManager.LoadScene("Login");
                    return;
                }

                Debug.Log($"[Leaderboard] Datos del jugador: {currentPlayer.username}");

                // Recargar datos desde Firebase para tener el historial más actualizado
                if (DatabaseService.Instance != null)
                {
                    var freshData = await DatabaseService.Instance.LoadPlayerData(currentPlayer.userId);
                    if (freshData != null)
                    {
                        currentPlayer = freshData;
                        AuthenticationService.Instance.UpdateCurrentPlayerData(currentPlayer);
                    }
                }
            }
        }

        /// <summary>
        /// Configura los listeners de los botones
        /// </summary>
        private void SetupListeners()
        {
            if (nacionalTab != null)
            {
                nacionalTab.onClick.RemoveAllListeners();
                nacionalTab.onClick.AddListener(() => OnTabSelected(LeaderboardTab.Local));
                nacionalTab.interactable = true;
                Debug.Log($"[Leaderboard] Listener de NACIONAL configurado");
            }
            else
            {
                Debug.LogError("[Leaderboard] nacionalTab es NULL!");
            }

            if (mundialTab != null)
            {
                mundialTab.onClick.RemoveAllListeners();
                mundialTab.onClick.AddListener(() => OnTabSelected(LeaderboardTab.Global));
                mundialTab.interactable = true;
                Debug.Log($"[Leaderboard] Listener de MUNDIAL configurado");
            }
            else
            {
                Debug.LogError("[Leaderboard] mundialTab es NULL!");
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(OnBackButtonClicked);
                backButton.interactable = true;
                Debug.Log($"[Leaderboard] Listener de VOLVER configurado");
            }
            else
            {
                Debug.LogError("[Leaderboard] backButton es NULL!");
            }

            // Play Button (en EmptyState)
            if (playButton != null)
            {
                playButton.onClick.RemoveAllListeners();
                playButton.onClick.AddListener(OnPlayButtonClicked);
            }
        }

        /// <summary>
        /// Navega a GameSelector para jugar
        /// </summary>
        private void OnPlayButtonClicked()
        {
            Debug.Log("[Leaderboard] Navegando a GameSelector");
            SceneManager.LoadScene("GameSelector");
        }

        #endregion

        #region Tab Navigation

        /// <summary>
        /// Cuando se selecciona una tab
        /// </summary>
        private void OnTabSelected(LeaderboardTab tab)
        {
            Debug.Log($"[Leaderboard] OnTabSelected - Tab: {tab}");

            if (currentTab == tab)
            {
                return;
            }

            currentTab = tab;
            UpdateTabVisuals();
            LoadLeaderboard(tab);
        }

        /// <summary>
        /// Actualiza los visuales de las tabs
        /// </summary>
        private void UpdateTabVisuals()
        {
            SetTabButtonState(nacionalTab, currentTab == LeaderboardTab.Local);
            SetTabButtonState(mundialTab, currentTab == LeaderboardTab.Global);
        }

        /// <summary>
        /// Establece el estado visual de un botón de tab
        /// </summary>
        private void SetTabButtonState(Button button, bool isSelected)
        {
            if (button == null) return;

            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                if (isSelected)
                {
                    buttonImage.color = new Color(0f, 0.83f, 1f); // Azul eléctrico
                }
                else
                {
                    buttonImage.color = new Color(0.3f, 0.3f, 0.4f); // Gris oscuro
                }
            }
        }

        #endregion

        #region Load Leaderboards

        /// <summary>
        /// Carga el leaderboard según la tab seleccionada
        /// </summary>
        private async void LoadLeaderboard(LeaderboardTab tab)
        {
            ShowLoading(true, AutoLocalizer.Get("loading_rankings"));

            // Limpiar leaderboard actual
            ClearLeaderboard();

            List<LeaderboardEntry> entries = null;

            try
            {
                switch (tab)
                {
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
                ShowErrorMessage(AutoLocalizer.Get("error_loading_rankings"));
            }
            finally
            {
                ShowLoading(false);
            }
        }

        /// <summary>
        /// Carga las puntuaciones locales (país)
        /// </summary>
        private async System.Threading.Tasks.Task<List<LeaderboardEntry>> LoadLocalScores()
        {
            Debug.Log("[Leaderboard] Cargando puntuaciones nacionales...");

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
            Debug.Log("[Leaderboard] Cargando puntuaciones mundiales...");

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

            // Ocultar estado vacío
            HideEmptyState();

            // Configurar el RectTransform del container
            RectTransform containerRT = leaderboardContainer as RectTransform;
            if (containerRT != null)
            {
                containerRT.anchorMin = new Vector2(0, 1);
                containerRT.anchorMax = new Vector2(1, 1);
                containerRT.pivot = new Vector2(0.5f, 1);
                containerRT.anchoredPosition = Vector2.zero;
                containerRT.gameObject.SetActive(true);

                if (containerRT.sizeDelta.x <= 0 && scrollRect != null && scrollRect.viewport != null)
                {
                    float viewportWidth = scrollRect.viewport.rect.width;
                    containerRT.sizeDelta = new Vector2(viewportWidth, containerRT.sizeDelta.y);
                }
            }

            // Obtener o crear VerticalLayoutGroup
            var layoutGroup = leaderboardContainer.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = leaderboardContainer.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            }

            layoutGroup.spacing = 5f;
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;

            foreach (var entry in entries)
            {
                CreateLeaderboardEntry(entry);
            }

            // Resaltar posición del jugador si está en la lista
            HighlightPlayerPosition(entries);

            // Forzar reconstrucción del layout
            StartCoroutine(ForceLayoutRebuild(containerRT));
        }

        /// <summary>
        /// Fuerza la reconstrucción del layout
        /// </summary>
        private System.Collections.IEnumerator ForceLayoutRebuild(RectTransform containerRT)
        {
            if (containerRT == null) yield break;

            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(containerRT);
            UnityEngine.Canvas.ForceUpdateCanvases();

            yield return null;

            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(containerRT);

            yield return null;

            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        /// <summary>
        /// Crea una entrada visual en el leaderboard usando prefab
        /// </summary>
        private void CreateLeaderboardEntry(LeaderboardEntry entry)
        {
            if (leaderboardContainer == null) return;

            bool isCurrentPlayer = currentPlayer != null && entry.userId == currentPlayer.userId;

            // Verificar si hay prefab asignado
            if (leaderboardEntryPrefab != null)
            {
                // USAR PREFAB
                GameObject entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
                entryObj.name = $"Entry_{entry.position}";

                LeaderboardEntryUI entryUI = entryObj.GetComponent<LeaderboardEntryUI>();
                if (entryUI != null)
                {
                    entryUI.Setup(entry, isCurrentPlayer);
                }
                else
                {
                    Debug.LogWarning("[Leaderboard] Prefab no tiene LeaderboardEntryUI, usando fallback");
                    Destroy(entryObj);
                    CreateLeaderboardEntryFallback(entry, isCurrentPlayer);
                }

                entryObj.SetActive(true);
            }
            else
            {
                // FALLBACK: Crear por codigo si no hay prefab
                CreateLeaderboardEntryFallback(entry, isCurrentPlayer);
            }
        }

        /// <summary>
        /// Fallback: Crea entrada por codigo si no hay prefab
        /// </summary>
        private void CreateLeaderboardEntryFallback(LeaderboardEntry entry, bool isCurrentPlayer)
        {
            GameObject entryObj = new GameObject($"Entry_{entry.position}");
            entryObj.transform.SetParent(leaderboardContainer, false);

            RectTransform entryRT = entryObj.AddComponent<RectTransform>();
            entryRT.anchorMin = new Vector2(0, 1);
            entryRT.anchorMax = new Vector2(1, 1);
            entryRT.pivot = new Vector2(0.5f, 1);
            entryRT.anchoredPosition = Vector2.zero;
            entryRT.sizeDelta = new Vector2(0, 70);

            var layoutElement = entryObj.AddComponent<UnityEngine.UI.LayoutElement>();
            layoutElement.preferredHeight = 70;
            layoutElement.minHeight = 70;

            Image bg = entryObj.AddComponent<Image>();
            bg.color = isCurrentPlayer ? new Color(0f, 0.83f, 1f, 0.95f) : new Color(0.15f, 0.15f, 0.2f, 0.95f);

            // Posicion (0% - 15%)
            Color positionColor = entry.position <= 3 ? GetMedalColor(entry.position) : new Color(1f, 0.84f, 0f);
            TextMeshProUGUI posText = CreateEntryText(entryObj.transform, "PositionText", $"{entry.position}", 28, positionColor);
            RectTransform posRT = posText.GetComponent<RectTransform>();
            posRT.anchorMin = new Vector2(0, 0);
            posRT.anchorMax = new Vector2(0.15f, 1);
            posRT.offsetMin = Vector2.zero;
            posRT.offsetMax = Vector2.zero;
            posText.alignment = TMPro.TextAlignmentOptions.Center;
            posText.fontStyle = TMPro.FontStyles.Bold;

            // Divisor 1
            CreateVerticalDividerAnchored(entryObj.transform, 0.15f);

            // Username (15% - 70%)
            TextMeshProUGUI nameText = CreateEntryText(entryObj.transform, "UsernameText", entry.username, 22, Color.white);
            RectTransform nameRT = nameText.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0.15f, 0);
            nameRT.anchorMax = new Vector2(0.70f, 1);
            nameRT.offsetMin = new Vector2(10, 0);
            nameRT.offsetMax = new Vector2(-10, 0);
            nameText.alignment = TMPro.TextAlignmentOptions.Center;

            // Divisor 2
            CreateVerticalDividerAnchored(entryObj.transform, 0.70f);

            // Tiempo (70% - 100%)
            TextMeshProUGUI timeText = CreateEntryText(entryObj.transform, "TimeText", $"{entry.time:F3}s", 22, new Color(0f, 1f, 0.53f));
            RectTransform timeRT = timeText.GetComponent<RectTransform>();
            timeRT.anchorMin = new Vector2(0.70f, 0);
            timeRT.anchorMax = new Vector2(1f, 1);
            timeRT.offsetMin = new Vector2(10, 0);
            timeRT.offsetMax = new Vector2(-10, 0);
            timeText.alignment = TMPro.TextAlignmentOptions.Center;

            CreateHorizontalDivider(entryObj.transform);

            // Agregar componente UI
            LeaderboardEntryUI entryUI = entryObj.AddComponent<LeaderboardEntryUI>();
            entryUI.AutoSetupReferences();

            entryObj.SetActive(true);
        }

        /// <summary>
        /// Crea divisor vertical usando anclas (para fallback)
        /// </summary>
        private void CreateVerticalDividerAnchored(Transform parent, float anchorX)
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
            divImage.raycastTarget = false;
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
            divRT.sizeDelta = new Vector2(1f, -20);

            Image divImage = divider.AddComponent<Image>();
            divImage.color = new Color(0.3f, 0.3f, 0.4f, 0.5f);
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
            divRT.sizeDelta = new Vector2(-40, 1f);

            Image divImage = divider.AddComponent<Image>();
            divImage.color = new Color(0.3f, 0.3f, 0.4f, 0.3f);
        }

        /// <summary>
        /// Crea un texto para una entrada del leaderboard
        /// </summary>
        private TextMeshProUGUI CreateEntryText(Transform parent, string name, string text, int fontSize, Color color)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();

            if (TMPro.TMP_Settings.defaultFontAsset != null)
            {
                tmp.font = TMPro.TMP_Settings.defaultFontAsset;
            }

            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.fontStyle = TMPro.FontStyles.Normal;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TMPro.TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;

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
        /// Resalta la posición del jugador actual y actualiza el panel inferior
        /// </summary>
        private void HighlightPlayerPosition(List<LeaderboardEntry> entries)
        {
            if (currentPlayer == null) return;

            var playerEntry = entries.Find(e => e.userId == currentPlayer.userId);
            if (playerEntry != null)
            {
                Debug.Log($"[Leaderboard] Jugador encontrado en posición #{playerEntry.position}");
                UpdatePlayerPositionPanel(playerEntry.position, playerEntry.time);
            }
            else
            {
                // El jugador no está en el ranking
                UpdatePlayerPositionPanel(-1, 0);
            }
        }

        /// <summary>
        /// Actualiza el panel inferior con la posición del jugador
        /// </summary>
        private void UpdatePlayerPositionPanel(int position, float time)
        {
            if (playerPositionPanel == null) return;

            playerPositionPanel.SetActive(true);

            if (positionNumberText != null)
            {
                if (position > 0)
                {
                    positionNumberText.text = $"#{position}";
                    positionNumberText.color = new Color(1f, 0.84f, 0f); // Gold
                }
                else
                {
                    positionNumberText.text = AutoLocalizer.Get("not_ranked");
                    positionNumberText.color = new Color(0.5f, 0.5f, 0.5f); // Gray
                }
            }

            if (positionTimeText != null)
            {
                if (time > 0)
                {
                    positionTimeText.text = $"{time:F3}s";
                }
                else
                {
                    positionTimeText.text = "--";
                }
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
        /// Muestra el estado vacío cuando no hay datos
        /// </summary>
        private void ShowEmptyMessage()
        {
            Debug.Log("[Leaderboard] No hay datos para mostrar");

            // Mostrar EmptyState si existe
            if (emptyState != null)
            {
                emptyState.SetActive(true);
            }

            // Ocultar panel de posición del jugador
            if (playerPositionPanel != null)
            {
                playerPositionPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Oculta el estado vacío
        /// </summary>
        private void HideEmptyState()
        {
            if (emptyState != null)
            {
                emptyState.SetActive(false);
            }

            // Mostrar panel de posición
            if (playerPositionPanel != null)
            {
                playerPositionPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Muestra mensaje de error
        /// </summary>
        private void ShowErrorMessage(string message)
        {
            Debug.LogError($"[Leaderboard] {message}");

            GameObject errorMsg = new GameObject("ErrorMessage");
            errorMsg.transform.SetParent(leaderboardContainer, false);

            RectTransform rt = errorMsg.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(800, 100);

            TextMeshProUGUI text = errorMsg.AddComponent<TextMeshProUGUI>();
            if (TMPro.TMP_Settings.defaultFontAsset != null)
                text.font = TMPro.TMP_Settings.defaultFontAsset;
            text.text = message;
            text.alignment = TMPro.TextAlignmentOptions.Center;
            text.fontSize = 24;
            text.color = Color.red;
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
        Local,
        Global
    }
}
