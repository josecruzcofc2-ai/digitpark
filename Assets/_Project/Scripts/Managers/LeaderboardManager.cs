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
        [SerializeField] public Button personalTab;
        [SerializeField] public Button nacionalTab;
        [SerializeField] public Button mundialTab;

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

        private async void Start()
        {
            Debug.Log("[Leaderboard] LeaderboardManager iniciado");

            // Verificar e inicializar servicios si no existen (para testing directo)
            EnsureServicesExist();

            // Configurar listeners primero
            SetupListeners();

            // Obtener datos del jugador (async)
            await LoadPlayerDataAsync();

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

                Debug.Log($"[Leaderboard] Datos iniciales del jugador: {currentPlayer.username}");

                // Recargar datos desde Firebase para tener el historial más actualizado
                if (DatabaseService.Instance != null)
                {
                    Debug.Log("[Leaderboard] Recargando datos del jugador desde Firebase...");
                    var freshData = await DatabaseService.Instance.LoadPlayerData(currentPlayer.userId);
                    if (freshData != null)
                    {
                        currentPlayer = freshData;
                        AuthenticationService.Instance.UpdateCurrentPlayerData(currentPlayer);
                        Debug.Log($"[Leaderboard] Datos actualizados. Scores en historial: {currentPlayer.scoreHistory.Count}");
                    }
                    else
                    {
                        Debug.LogWarning("[Leaderboard] No se pudieron recargar datos desde Firebase, usando datos en memoria");
                    }
                }
            }
        }

        /// <summary>
        /// Configura los listeners de los botones
        /// </summary>
        private void SetupListeners()
        {
            if (personalTab != null)
            {
                personalTab.onClick.RemoveAllListeners();
                personalTab.onClick.AddListener(() => OnTabSelected(LeaderboardTab.Personal));
                personalTab.interactable = true;
                Debug.Log($"[Leaderboard] Listener de PERSONALES configurado");
            }
            else
            {
                Debug.LogError("[Leaderboard] personalTab es NULL!");
            }

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
        }

        #endregion

        #region Tab Navigation

        /// <summary>
        /// Cuando se selecciona una tab
        /// </summary>
        private void OnTabSelected(LeaderboardTab tab)
        {
            Debug.Log($"[Leaderboard] OnTabSelected llamado - Tab: {tab}, CurrentTab: {currentTab}");

            if (currentTab == tab)
            {
                Debug.Log($"[Leaderboard] Tab ya seleccionada, ignorando");
                return;
            }

            Debug.Log($"[Leaderboard] Cambiando a tab: {tab}");

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
            SetTabButtonState(personalTab, currentTab == LeaderboardTab.Personal);
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
                ShowErrorMessage(AutoLocalizer.Get("error_loading_rankings"));
            }
            finally
            {
                ShowLoading(false);

                // Actualizar panel de mejor tiempo (mostrar solo en Personal)
                UpdatePersonalBestPanel();
            }
        }

        /// <summary>
        /// Carga las puntuaciones personales del jugador (orden cronológico inverso - más reciente primero)
        /// </summary>
        private async System.Threading.Tasks.Task<List<LeaderboardEntry>> LoadPersonalScores()
        {
            Debug.Log("========== [Leaderboard] CARGANDO PUNTUACIONES PERSONALES ==========");

            if (currentPlayer == null)
            {
                Debug.LogError("[Leaderboard] ❌ currentPlayer es NULL!");
                return new List<LeaderboardEntry>();
            }

            Debug.Log($"[Leaderboard] ✓ Jugador encontrado: {currentPlayer.username} (ID: {currentPlayer.userId})");
            Debug.Log($"[Leaderboard] ✓ Scores en scoreHistory: {currentPlayer.scoreHistory?.Count ?? 0}");

            // DEBUG: Mostrar los scores si existen
            if (currentPlayer.scoreHistory != null && currentPlayer.scoreHistory.Count > 0)
            {
                Debug.Log($"[Leaderboard] 📊 Mostrando primeros 5 scores del historial:");
                for (int i = 0; i < Mathf.Min(5, currentPlayer.scoreHistory.Count); i++)
                {
                    var score = currentPlayer.scoreHistory[i];
                    Debug.Log($"  [{i}] Tiempo: {score.time}s, Timestamp: {score.timestamp}");
                }
            }

            // Convertir el historial de scores a LeaderboardEntry
            List<LeaderboardEntry> entries = new List<LeaderboardEntry>();

            if (currentPlayer.scoreHistory == null || currentPlayer.scoreHistory.Count == 0)
            {
                Debug.LogWarning("[Leaderboard] ⚠️ El jugador no tiene scores personales registrados");
                Debug.LogWarning("[Leaderboard] ⚠️ Juega una partida primero para ver tus scores personales");
                return entries;
            }

            // Tomar solo los últimos 30 scores
            var recentScores = currentPlayer.scoreHistory.TakeLast(30).ToList();

            // IMPORTANTE: Mantener orden cronológico INVERSO (más reciente primero)
            recentScores.Reverse();

            int position = 1;
            foreach (var score in recentScores)
            {
                entries.Add(new LeaderboardEntry
                {
                    userId = currentPlayer.userId,
                    username = currentPlayer.username,
                    time = score.time,
                    countryCode = currentPlayer.countryCode,
                    avatarUrl = currentPlayer.avatarUrl,
                    position = position,  // Posición cronológica (1 = más reciente)
                    timestamp = score.timestamp  // Guardamos el timestamp para mostrarlo
                });
                position++;
            }

            Debug.Log($"[Leaderboard] ✓ Total de {entries.Count} scores personales procesados");
            Debug.Log($"[Leaderboard] 📋 Primeras 3 entradas a mostrar:");
            for (int i = 0; i < Mathf.Min(3, entries.Count); i++)
            {
                var entry = entries[i];
                Debug.Log($"  Entry {i}: Posición #{entry.position}, Tiempo: {entry.time}s");
            }
            Debug.Log("========== [Leaderboard] FIN CARGA PERSONAL ==========");

            personalScores = entries;

            // Delay para simular carga async
            await System.Threading.Tasks.Task.Delay(100);

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

            // Configurar el RectTransform del container PRIMERO
            RectTransform containerRT = leaderboardContainer as RectTransform;
            if (containerRT != null)
            {
                // Configurar anclas para que se estire horizontalmente
                containerRT.anchorMin = new Vector2(0, 1);
                containerRT.anchorMax = new Vector2(1, 1);
                containerRT.pivot = new Vector2(0.5f, 1);

                // Establecer posición inicial
                containerRT.anchoredPosition = Vector2.zero;

                // CRÍTICO: Asegurar que el container esté visible y activo
                containerRT.gameObject.SetActive(true);

                // Si tiene ancho 0, establecer un ancho mínimo basado en el viewport
                if (containerRT.sizeDelta.x <= 0 && scrollRect != null && scrollRect.viewport != null)
                {
                    float viewportWidth = scrollRect.viewport.rect.width;
                    containerRT.sizeDelta = new Vector2(viewportWidth, containerRT.sizeDelta.y);
                    Debug.Log($"[Leaderboard] Ancho del content corregido a: {viewportWidth}");
                }
                else
                {
                    Debug.Log($"[Leaderboard] Content ancho actual: {containerRT.sizeDelta.x}");
                }
            }

            // Obtener o crear VerticalLayoutGroup
            var layoutGroup = leaderboardContainer.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
            if (layoutGroup == null)
            {
                Debug.Log("[Leaderboard] Creando nuevo VerticalLayoutGroup");
                layoutGroup = leaderboardContainer.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            }
            else
            {
                Debug.Log("[Leaderboard] VerticalLayoutGroup ya existe, configurándolo");
            }

            // Configurar el LayoutGroup correctamente
            layoutGroup.spacing = 5f;
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = false; // NO forzar expansión horizontal
            layoutGroup.childForceExpandHeight = false;

            Debug.Log($"[Leaderboard] LayoutGroup configurado - childForceExpandWidth: {layoutGroup.childForceExpandWidth}");

            int createdCount = 0;
            foreach (var entry in entries)
            {
                CreateLeaderboardEntry(entry);
                createdCount++;
            }

            Debug.Log($"[Leaderboard] {createdCount} entradas creadas en el container");
            // Resaltar posición del jugador si está en la lista
            HighlightPlayerPosition(entries);

            // Forzar reconstrucción del layout después de varios frames
            StartCoroutine(ForceLayoutRebuildMultipleTimes(containerRT, entries.Count));
        }

        /// <summary>
        /// Fuerza la reconstrucción del layout múltiples veces para asegurar visibilidad
        /// </summary>
        private System.Collections.IEnumerator ForceLayoutRebuildMultipleTimes(RectTransform containerRT, int expectedCount)
        {
            if (containerRT == null)
            {
                Debug.LogError("[Leaderboard] containerRT es NULL en ForceLayoutRebuildMultipleTimes!");
                yield break;
            }

            // Frame 0: Rebuild inicial
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(containerRT);
            UnityEngine.Canvas.ForceUpdateCanvases();
            Debug.Log($"[Leaderboard] Frame 0 - Layout rebuilt. Content size: {containerRT.sizeDelta}");

            yield return null; // Esperar 1 frame

            // Frame 1: Segundo rebuild
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(containerRT);
            UnityEngine.Canvas.ForceUpdateCanvases();
            Debug.Log($"[Leaderboard] Frame 1 - Layout rebuilt. Content size: {containerRT.sizeDelta}");

            // Log detallado de las primeras entradas
            for (int i = 0; i < containerRT.childCount && i < 3; i++)
            {
                Transform child = containerRT.GetChild(i);
                RectTransform childRT = child as RectTransform;
                if (childRT != null)
                {
                    var image = child.GetComponent<Image>();
                    var layoutEl = child.GetComponent<UnityEngine.UI.LayoutElement>();
                    Debug.Log($"[Leaderboard] Hijo {i}: {child.name}");
                    Debug.Log($"  - Size: {childRT.sizeDelta}, Pos: {childRT.anchoredPosition}");
                    Debug.Log($"  - Activo: {child.gameObject.activeSelf}, Image: {image != null && image.enabled}");
                    Debug.Log($"  - LayoutElement minWidth: {layoutEl?.minWidth}, preferredHeight: {layoutEl?.preferredHeight}");
                    Debug.Log($"  - Rect (world): {childRT.rect}");
                }
            }

            yield return null; // Esperar otro frame

            // Frame 2: Scroll al top
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 1f;
                Debug.Log($"[Leaderboard] Scroll resetted to top");
            }

            // Frame 2: Rebuild final
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(containerRT);
            Debug.Log($"[Leaderboard] Frame 2 - Final rebuild. Content size: {containerRT.sizeDelta}");
            Debug.Log($"[Leaderboard] ✅ DisplayLeaderboard completado. {expectedCount} entradas deberían ser visibles.");
        }

        private System.Collections.IEnumerator ScrollToTopNextFrame()
        {
            yield return null; // Esperar un frame
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
            if (leaderboardContainer == null)
            {
                Debug.LogError("[Leaderboard] leaderboardContainer es NULL!");
                return;
            }

            // Crear entrada programáticamente (sin prefab)
            GameObject entryObj = new GameObject($"Entry_{entry.position}");
            entryObj.transform.SetParent(leaderboardContainer, false);

            RectTransform entryRT = entryObj.AddComponent<RectTransform>();

            // CRÍTICO - Configuración correcta del RectTransform
            entryRT.anchorMin = new Vector2(0, 1);     // Anclar desde la izquierda-arriba
            entryRT.anchorMax = new Vector2(1, 1);     // Stretch horizontal completo
            entryRT.pivot = new Vector2(0.5f, 1);      // Pivot en el centro superior
            entryRT.anchoredPosition = Vector2.zero;   // IMPORTANTE: Posición (0,0)
            entryRT.sizeDelta = new Vector2(0, 80);    // Ancho 0 = stretch, altura 80

            Debug.Log($"[Leaderboard] Entry {entry.position} ANTES de LayoutElement - Position: {entryRT.anchoredPosition}, Size: {entryRT.sizeDelta}");

            // LayoutElement para controlar altura
            var layoutElement = entryObj.AddComponent<UnityEngine.UI.LayoutElement>();
            layoutElement.preferredHeight = 80;
            layoutElement.minHeight = 80;
            layoutElement.flexibleWidth = 1; // Permitir que se expanda horizontalmente

            // FORZAR anclas de nuevo después del LayoutElement (el LayoutGroup puede cambiarlas)
            entryRT.anchorMin = new Vector2(0, 1);
            entryRT.anchorMax = new Vector2(1, 1);
            entryRT.pivot = new Vector2(0.5f, 1);
            entryRT.anchoredPosition = Vector2.zero;
            entryRT.sizeDelta = new Vector2(0, 80);

            Debug.Log($"[Leaderboard] Entrada creada: {entry.username} - {entry.time}s (Posición: {entry.position})");
            Debug.Log($"[Leaderboard] Entry RectTransform FINAL - Anchors: {entryRT.anchorMin} to {entryRT.anchorMax}, Size: {entryRT.sizeDelta}, Pos: {entryRT.anchoredPosition}");

            // CRÍTICO: Asegurar que la entrada esté activa
            entryObj.SetActive(true);

            // Fondo de la entrada
            Image bg = entryObj.AddComponent<Image>();
            bool isCurrentPlayer = currentPlayer != null && entry.userId == currentPlayer.userId;

            if (isCurrentPlayer)
            {
                bg.color = new Color(0f, 0.83f, 1f, 0.95f); // Azul eléctrico MÁS VISIBLE
            }
            else
            {
                bg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f); // Gris oscuro MÁS VISIBLE
            }

            Debug.Log($"[Leaderboard] Fondo de entrada configurado con color: {bg.color}, Enabled: {bg.enabled}");

            // Si es modo Personal, NO mostrar el TOP#
            bool isPersonalTab = currentTab == LeaderboardTab.Personal;

            float leftPadding = 20f;

            if (isPersonalTab)
            {
                // MODO PERSONAL: Número (izquierda) | Tiempo (centro-derecha) - IGUAL QUE TORNEOS

                // Número del historial (izquierda - 0% a 15%)
                Color posColor = entry.position <= 3 ? GetMedalColor(entry.position) : new Color(1f, 0.84f, 0f);
                TextMeshProUGUI numberText = CreateEntryText(entryObj.transform, "NumberText", $"#{entry.position}", 28, posColor);
                RectTransform numberRT = numberText.GetComponent<RectTransform>();
                numberRT.anchorMin = new Vector2(0, 0);
                numberRT.anchorMax = new Vector2(0.15f, 1);
                numberRT.offsetMin = Vector2.zero;
                numberRT.offsetMax = Vector2.zero;
                numberText.alignment = TMPro.TextAlignmentOptions.Center;
                numberText.fontStyle = TMPro.FontStyles.Bold;

                // Divisor vertical (15%)
                CreatePersonalVerticalDivider(entryObj.transform, 0.15f);

                // Tiempo (centro-derecha - 15% a 100%)
                TextMeshProUGUI timeText = CreateEntryText(entryObj.transform, "TimeText", $"{entry.time:F3}s", 28, new Color(0f, 1f, 0.53f));
                RectTransform timeRT = timeText.GetComponent<RectTransform>();
                timeRT.anchorMin = new Vector2(0.15f, 0);
                timeRT.anchorMax = new Vector2(1f, 1);
                timeRT.offsetMin = new Vector2(10, 0);
                timeRT.offsetMax = new Vector2(-10, 0);
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
        /// Crea un divisor vertical usando anclas relativas (porcentaje) - para modo Personal
        /// </summary>
        private void CreatePersonalVerticalDivider(Transform parent, float anchorX)
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
                    playerPositionText.text = $"{AutoLocalizer.Get("your_position")} #{playerEntry.position}";
                }

                if (playerTimeText != null)
                {
                    string label = AutoLocalizer.Get("personal_best_time");
                    playerTimeText.text = $"{label}: {playerEntry.time:F3}s";
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
                Debug.Log($"[Leaderboard] LoadingPanel {(show ? "MOSTRADO" : "OCULTADO")}");
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
            emptyMsg.transform.SetParent(leaderboardContainer, false);

            TextMeshProUGUI text = emptyMsg.AddComponent<TextMeshProUGUI>();
            text.text = AutoLocalizer.Get("no_scores_yet");
            text.alignment = TMPro.TextAlignmentOptions.Center;
            text.fontSize = 24;
            text.color = Color.gray;

            RectTransform rt = emptyMsg.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(800, 200);
        }

        /// <summary>
        /// Muestra mensaje de error
        /// </summary>
        private void ShowErrorMessage(string message)
        {
            Debug.LogError($"[Leaderboard] {message}");

            GameObject errorMsg = new GameObject("ErrorMessage");
            errorMsg.transform.SetParent(leaderboardContainer, false);

            TextMeshProUGUI text = errorMsg.AddComponent<TextMeshProUGUI>();
            text.text = message;
            text.alignment = TMPro.TextAlignmentOptions.Center;
            text.fontSize = 24;
            text.color = Color.red;

            RectTransform rt = errorMsg.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(800, 100);
        }

        /// <summary>
        /// Formatea un timestamp a formato legible (DD/MM/YYYY HH:MM)
        /// </summary>
        private string FormatTimestamp(string timestamp)
        {
            if (string.IsNullOrEmpty(timestamp))
            {
                return AutoLocalizer.Get("no_date");
            }

            try
            {
                System.DateTime dateTime = System.DateTime.Parse(timestamp);
                return dateTime.ToString("dd/MM/yyyy HH:mm");
            }
            catch
            {
                return AutoLocalizer.Get("invalid_date");
            }
        }

        /// <summary>
        /// Actualiza el panel de mejor tiempo (solo para tab Personal)
        /// </summary>
        private void UpdatePersonalBestPanel()
        {
            if (playerHighlightPanel == null) return;

            if (currentTab == LeaderboardTab.Personal && currentPlayer != null)
            {
                // Mostrar panel con el mejor tiempo
                playerHighlightPanel.SetActive(true);

                if (playerTimeText != null)
                {
                    float bestTime = currentPlayer.bestTime;
                    string label = AutoLocalizer.Get("personal_best_time");
                    if (bestTime == float.MaxValue || bestTime <= 0)
                    {
                        playerTimeText.text = $"{label}: {AutoLocalizer.Get("no_best_time_yet")}";
                    }
                    else
                    {
                        playerTimeText.text = $"{label}: {bestTime:F3}s";
                    }
                    Debug.Log($"[Leaderboard] Mostrando mejor tiempo personal: {bestTime} (MaxValue={float.MaxValue})");
                }

                if (playerPositionText != null)
                {
                    playerPositionText.text = AutoLocalizer.Get("history_games", currentPlayer.scoreHistory?.Count ?? 0);
                }
            }
            else
            {
                // Ocultar panel en otras tabs
                playerHighlightPanel.SetActive(false);
            }
        }

        #endregion

        #region Navigation

        /// <summary>
        /// Vuelve al menú principal
        /// </summary>
        private void OnBackButtonClicked()
        {
            Debug.Log("[Leaderboard] ✅ BOTÓN VOLVER PRESIONADO - Volviendo al menú principal");
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
