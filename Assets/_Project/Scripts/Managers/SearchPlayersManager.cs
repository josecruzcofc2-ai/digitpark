using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using DigitPark.Services.Firebase;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager de busqueda de jugadores
    /// Permite buscar, agregar amigos y retar jugadores
    /// </summary>
    public class SearchPlayersManager : MonoBehaviour
    {
        [Header("UI - Search")]
        [SerializeField] private TMP_InputField searchInputField;
        [SerializeField] private Button searchButton;
        [SerializeField] private Button clearButton;

        [Header("UI - Results")]
        [SerializeField] private Transform resultsContainer;
        [SerializeField] private GameObject playerItemPrefab;
        [SerializeField] private TextMeshProUGUI noResultsText;
        [SerializeField] private GameObject loadingIndicator;

        [Header("UI - Navigation")]
        [SerializeField] private Button backButton;

        [Header("Settings")]
        [SerializeField] private int maxResults = 20;
        [SerializeField] private float searchDelay = 0.5f;

        private List<GameObject> currentResults = new List<GameObject>();
        private string lastSearchQuery;
        private bool isSearching = false;

        private void Start()
        {
            Debug.Log("[SearchPlayers] SearchPlayersManager iniciado");

            SetupListeners();
            ClearResults();
        }

        private void SetupListeners()
        {
            searchButton?.onClick.AddListener(OnSearchClicked);
            clearButton?.onClick.AddListener(OnClearClicked);
            backButton?.onClick.AddListener(OnBackClicked);

            // Busqueda en tiempo real mientras escribe
            if (searchInputField != null)
            {
                searchInputField.onValueChanged.AddListener(OnSearchInputChanged);
                searchInputField.onSubmit.AddListener(OnSearchSubmit);
            }
        }

        private void OnSearchInputChanged(string value)
        {
            // Busqueda con delay para no sobrecargar
            if (value.Length >= 3)
            {
                CancelInvoke(nameof(DelayedSearch));
                Invoke(nameof(DelayedSearch), searchDelay);
            }
            else if (value.Length == 0)
            {
                ClearResults();
            }
        }

        private void OnSearchSubmit(string value)
        {
            if (value.Length >= 2)
            {
                PerformSearch(value);
            }
        }

        private void DelayedSearch()
        {
            if (searchInputField != null && searchInputField.text.Length >= 3)
            {
                PerformSearch(searchInputField.text);
            }
        }

        private void OnSearchClicked()
        {
            if (searchInputField != null && searchInputField.text.Length >= 2)
            {
                PerformSearch(searchInputField.text);
            }
        }

        private void OnClearClicked()
        {
            if (searchInputField != null)
                searchInputField.text = "";

            ClearResults();
        }

        private void OnBackClicked()
        {
            Debug.Log("[SearchPlayers] Volviendo al Main Menu");
            SceneManager.LoadScene("MainMenu");
        }

        private async void PerformSearch(string query)
        {
            if (isSearching || query == lastSearchQuery)
                return;

            lastSearchQuery = query;
            isSearching = true;

            Debug.Log($"[SearchPlayers] Buscando: {query}");

            // Mostrar indicador de carga
            if (loadingIndicator != null)
                loadingIndicator.SetActive(true);
            if (noResultsText != null)
                noResultsText.gameObject.SetActive(false);

            // Buscar en Firebase
            List<PlayerSearchResult> results = null;

            if (DatabaseService.Instance != null)
            {
                results = await DatabaseService.Instance.SearchPlayers(query, maxResults);
            }
            else
            {
                // Fallback con datos de prueba si no hay Firebase
                Debug.LogWarning("[SearchPlayers] DatabaseService no disponible, usando datos de prueba");
                results = GetTestResults();
            }

            OnSearchComplete(results);
        }

        private List<PlayerSearchResult> GetTestResults()
        {
            return new List<PlayerSearchResult>
            {
                new PlayerSearchResult { playerId = "player1", username = "ProGamer123", winRate = 67.5f, isFriend = false },
                new PlayerSearchResult { playerId = "player2", username = "MathWizard", winRate = 54.2f, isFriend = true },
                new PlayerSearchResult { playerId = "player3", username = "SpeedKing", winRate = 71.3f, isFriend = false },
            };
        }

        private void OnSearchComplete(List<PlayerSearchResult> results)
        {
            isSearching = false;

            // Ocultar indicador de carga
            if (loadingIndicator != null)
                loadingIndicator.SetActive(false);

            // Limpiar resultados anteriores
            ClearResults();

            if (results == null || results.Count == 0)
            {
                // Mostrar mensaje de no resultados
                if (noResultsText != null)
                {
                    noResultsText.text = "No se encontraron jugadores";
                    noResultsText.gameObject.SetActive(true);
                }
                return;
            }

            // Crear items de resultado
            foreach (var result in results)
            {
                CreatePlayerItem(result);
            }

            Debug.Log($"[SearchPlayers] Encontrados {results.Count} jugadores");
        }

        private void CreatePlayerItem(PlayerSearchResult result)
        {
            if (playerItemPrefab == null || resultsContainer == null)
            {
                Debug.LogWarning("[SearchPlayers] Falta prefab o container");
                return;
            }

            GameObject item = Instantiate(playerItemPrefab, resultsContainer);
            currentResults.Add(item);

            // Configuracion basica del item
            SetupBasicPlayerItem(item, result);
        }

        private void SetupBasicPlayerItem(GameObject item, PlayerSearchResult result)
        {
            // Buscar componentes de texto
            var texts = item.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var text in texts)
            {
                if (text.name.Contains("Username") || text.name.Contains("Name"))
                    text.text = result.username;
                else if (text.name.Contains("WinRate") || text.name.Contains("Rate"))
                    text.text = $"{result.winRate:F1}%";
                else if (text.name.Contains("Friend") && text.name.Contains("Status"))
                    text.text = result.isFriend ? "Amigo" : "";
            }

            // Configurar boton principal para ver perfil
            var mainButton = item.GetComponent<Button>();
            if (mainButton != null)
            {
                string playerId = result.playerId;
                mainButton.onClick.AddListener(() => OnPlayerItemClicked(playerId));
            }

            // Buscar y configurar botones de accion
            var buttons = item.GetComponentsInChildren<Button>();
            foreach (var btn in buttons)
            {
                string playerId = result.playerId;
                if (btn.name.Contains("AddFriend") || btn.name.Contains("Friend"))
                {
                    // Ocultar si ya es amigo
                    btn.gameObject.SetActive(!result.isFriend);
                    btn.onClick.AddListener(() => OnAddFriendClicked(playerId));
                }
                else if (btn.name.Contains("Challenge") || btn.name.Contains("Retar"))
                {
                    // Solo mostrar boton de reto si es amigo
                    btn.gameObject.SetActive(result.isFriend);
                    btn.onClick.AddListener(() => OnChallengeClicked(playerId));
                }
            }
        }

        private void ClearResults()
        {
            foreach (var item in currentResults)
            {
                if (item != null)
                    Destroy(item);
            }
            currentResults.Clear();

            if (noResultsText != null)
                noResultsText.gameObject.SetActive(false);
        }

        #region Item Callbacks

        private void OnPlayerItemClicked(string playerId)
        {
            Debug.Log($"[SearchPlayers] Ver perfil de: {playerId}");

            // Guardar ID y navegar a perfil
            PlayerPrefs.SetString("ViewProfileId", playerId);
            PlayerPrefs.Save();

            SceneManager.LoadScene("Profile");
        }

        private void OnAddFriendClicked(string playerId)
        {
            Debug.Log($"[SearchPlayers] Agregar amigo: {playerId}");

            // TODO: Enviar solicitud de amistad via Firebase
        }

        private void OnChallengeClicked(string playerId)
        {
            Debug.Log($"[SearchPlayers] Retar a: {playerId}");

            // Guardar ID y navegar a setup de reto
            PlayerPrefs.SetString("ChallengePlayerId", playerId);
            PlayerPrefs.Save();

            // TODO: Navegar a seleccion de juego
            // SceneManager.LoadScene("ChallengeSetup");
        }

        #endregion
    }
}
