using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace DigitPark.Games
{
    /// <summary>
    /// Manager para la pantalla de seleccion de juegos
    /// Muestra los 5 juegos disponibles + opcion de Cognitive Sprint
    /// </summary>
    public class GameSelectorManager : MonoBehaviour
    {
        [Header("Game Buttons")]
        [SerializeField] private Button digitRushButton;
        [SerializeField] private Button memoryPairsButton;
        [SerializeField] private Button quickMathButton;
        [SerializeField] private Button flashTapButton;
        [SerializeField] private Button oddOneOutButton;

        [Header("Cognitive Sprint")]
        [SerializeField] private Button cognitiveSprintButton;
        [SerializeField] private GameObject cognitiveSprintPanel;
        [SerializeField] private Toggle[] gameToggles; // 5 toggles para seleccionar juegos
        [SerializeField] private Button startSprintButton;
        [SerializeField] private Button cancelSprintButton;
        [SerializeField] private TextMeshProUGUI selectedCountText;

        [Header("Navigation")]
        [SerializeField] private Button backButton;

        [Header("Mode Selection")]
        [SerializeField] private bool isPracticeMode = true; // Por defecto practica

        private void Start()
        {
            SetupButtons();
            SetupCognitiveSprintPanel();

            // Suscribirse a cambios de seleccion
            CognitiveSprintManager.Instance.OnSelectionChanged += UpdateSprintUI;
        }

        private void OnDestroy()
        {
            if (CognitiveSprintManager.Instance != null)
            {
                CognitiveSprintManager.Instance.OnSelectionChanged -= UpdateSprintUI;
            }
        }

        private void SetupButtons()
        {
            // Botones de juegos individuales
            if (digitRushButton != null)
                digitRushButton.onClick.AddListener(() => StartSingleGame(GameType.DigitRush));

            if (memoryPairsButton != null)
                memoryPairsButton.onClick.AddListener(() => StartSingleGame(GameType.MemoryPairs));

            if (quickMathButton != null)
                quickMathButton.onClick.AddListener(() => StartSingleGame(GameType.QuickMath));

            if (flashTapButton != null)
                flashTapButton.onClick.AddListener(() => StartSingleGame(GameType.FlashTap));

            if (oddOneOutButton != null)
                oddOneOutButton.onClick.AddListener(() => StartSingleGame(GameType.OddOneOut));

            // Cognitive Sprint
            if (cognitiveSprintButton != null)
                cognitiveSprintButton.onClick.AddListener(OpenCognitiveSprintPanel);

            // Back
            if (backButton != null)
                backButton.onClick.AddListener(GoBack);
        }

        private void SetupCognitiveSprintPanel()
        {
            if (cognitiveSprintPanel != null)
                cognitiveSprintPanel.SetActive(false);

            // Setup toggles
            if (gameToggles != null)
            {
                GameType[] types = { GameType.DigitRush, GameType.MemoryPairs, GameType.QuickMath,
                                    GameType.FlashTap, GameType.OddOneOut };

                for (int i = 0; i < gameToggles.Length && i < types.Length; i++)
                {
                    int index = i;
                    GameType gameType = types[i];

                    if (gameToggles[i] != null)
                    {
                        gameToggles[i].onValueChanged.AddListener((isOn) => OnToggleChanged(gameType, isOn));
                    }
                }
            }

            // Botones del panel
            if (startSprintButton != null)
                startSprintButton.onClick.AddListener(StartCognitiveSprint);

            if (cancelSprintButton != null)
                cancelSprintButton.onClick.AddListener(CloseCognitiveSprintPanel);
        }

        /// <summary>
        /// Inicia un juego individual
        /// </summary>
        private void StartSingleGame(GameType gameType)
        {
            if (isPracticeMode)
            {
                GameSessionManager.Instance.StartPracticeSession(gameType);
            }
            else
            {
                // Para modo competitivo, navegar a matchmaking primero
                // TODO: Implementar flujo de matchmaking
                Debug.Log($"Modo competitivo para {gameType} - ir a matchmaking");
            }
        }

        /// <summary>
        /// Abre el panel de Cognitive Sprint
        /// </summary>
        private void OpenCognitiveSprintPanel()
        {
            if (cognitiveSprintPanel != null)
            {
                cognitiveSprintPanel.SetActive(true);
                CognitiveSprintManager.Instance.ClearSelection();
                ResetToggles();
                UpdateSprintUI(new List<GameType>());
            }
        }

        /// <summary>
        /// Cierra el panel de Cognitive Sprint
        /// </summary>
        private void CloseCognitiveSprintPanel()
        {
            if (cognitiveSprintPanel != null)
            {
                cognitiveSprintPanel.SetActive(false);
                CognitiveSprintManager.Instance.ClearSelection();
            }
        }

        /// <summary>
        /// Cuando un toggle cambia
        /// </summary>
        private void OnToggleChanged(GameType gameType, bool isOn)
        {
            if (isOn)
            {
                CognitiveSprintManager.Instance.AddGame(gameType);
            }
            else
            {
                CognitiveSprintManager.Instance.RemoveGame(gameType);
            }
        }

        /// <summary>
        /// Actualiza la UI del sprint segun la seleccion
        /// </summary>
        private void UpdateSprintUI(List<GameType> selectedGames)
        {
            if (selectedCountText != null)
            {
                int count = selectedGames?.Count ?? 0;
                selectedCountText.text = $"{count}/{CognitiveSprintManager.MAX_GAMES} juegos seleccionados";

                // Cambiar color segun validez
                if (count >= CognitiveSprintManager.MIN_GAMES)
                {
                    selectedCountText.color = Color.green;
                }
                else
                {
                    selectedCountText.color = Color.white;
                }
            }

            // Habilitar/deshabilitar boton de inicio
            if (startSprintButton != null)
            {
                startSprintButton.interactable = CognitiveSprintManager.Instance.IsValidSelection();
            }
        }

        /// <summary>
        /// Resetea todos los toggles
        /// </summary>
        private void ResetToggles()
        {
            if (gameToggles != null)
            {
                foreach (var toggle in gameToggles)
                {
                    if (toggle != null)
                        toggle.isOn = false;
                }
            }
        }

        /// <summary>
        /// Inicia el Cognitive Sprint
        /// </summary>
        private void StartCognitiveSprint()
        {
            if (!CognitiveSprintManager.Instance.IsValidSelection())
            {
                Debug.LogWarning("Seleccion invalida para Cognitive Sprint");
                return;
            }

            if (isPracticeMode)
            {
                CognitiveSprintManager.Instance.StartPracticeSprint();
            }
            else
            {
                // TODO: Ir a matchmaking con los juegos seleccionados
                Debug.Log("Modo competitivo - ir a matchmaking con Cognitive Sprint");
            }
        }

        /// <summary>
        /// Vuelve al menu principal
        /// </summary>
        private void GoBack()
        {
            SceneManager.LoadScene("MainMenu");
        }

        /// <summary>
        /// Cambia entre modo practica y competitivo
        /// </summary>
        public void SetPracticeMode(bool practice)
        {
            isPracticeMode = practice;
        }
    }
}
