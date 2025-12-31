using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DigitPark.Games
{
    /// <summary>
    /// Controller para el juego Memory Pairs
    /// El jugador debe encontrar todos los pares de cartas iguales
    /// </summary>
    public class MemoryPairsController : MinigameBase
    {
        public override GameType GameType => GameType.MemoryPairs;

        [Header("Memory Pairs - Grid")]
        [SerializeField] private Button[] cardButtons; // 16 botones (4x4)
        [SerializeField] private Image[] cardImages;

        [Header("Memory Pairs - UI")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI pairsFoundText;
        [SerializeField] private TextMeshProUGUI errorsText;
        [SerializeField] private GameObject winPanel;

        [Header("Memory Pairs - Sprites")]
        [SerializeField] private Sprite cardBackSprite;
        [SerializeField] private Sprite[] cardFrontSprites; // 8 sprites diferentes para 8 pares

        // Estado del juego
        private int[] cardValues; // Valor de cada carta (0-7 para 8 pares)
        private bool[] cardRevealed; // Si la carta esta revelada permanentemente
        private int firstSelectedCard = -1;
        private int secondSelectedCard = -1;
        private bool isProcessingPair;
        private int pairsFound;
        private int totalPairs = 8;

        protected override void Awake()
        {
            base.Awake();
            int gridSize = config?.TotalGridElements ?? 16;
            totalPairs = gridSize / 2;
            cardValues = new int[gridSize];
            cardRevealed = new bool[gridSize];
        }

        protected override void Start()
        {
            base.Start();
            SetupCards();
        }

        private void SetupCards()
        {
            if (cardButtons == null || cardButtons.Length < 16)
            {
                Debug.LogError("MemoryPairs requiere al menos 16 botones");
                return;
            }

            for (int i = 0; i < cardButtons.Length; i++)
            {
                int index = i;
                cardButtons[i].onClick.AddListener(() => OnCardClicked(index));
            }
        }

        public override void StartGame()
        {
            base.StartGame();
            ShuffleCards();
            pairsFound = 0;
            isProcessingPair = false;
            firstSelectedCard = -1;
            secondSelectedCard = -1;
            UpdateUI();
        }

        private void ShuffleCards()
        {
            // Crear pares (0,0,1,1,2,2,...,7,7)
            List<int> cards = new List<int>();
            for (int i = 0; i < totalPairs; i++)
            {
                cards.Add(i);
                cards.Add(i);
            }

            // Fisher-Yates shuffle
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (cards[i], cards[j]) = (cards[j], cards[i]);
            }

            // Asignar valores y ocultar cartas
            for (int i = 0; i < cardValues.Length && i < cards.Count; i++)
            {
                cardValues[i] = cards[i];
                cardRevealed[i] = false;
                SetCardHidden(i);
            }
        }

        private void OnCardClicked(int cardIndex)
        {
            if (!isPlaying || isPaused || isProcessingPair) return;
            if (cardRevealed[cardIndex]) return; // Ya encontrada
            if (cardIndex == firstSelectedCard) return; // Ya seleccionada

            // Revelar carta
            SetCardRevealed(cardIndex);

            if (firstSelectedCard == -1)
            {
                // Primera carta
                firstSelectedCard = cardIndex;
            }
            else
            {
                // Segunda carta
                secondSelectedCard = cardIndex;
                StartCoroutine(ProcessPair());
            }
        }

        private IEnumerator ProcessPair()
        {
            isProcessingPair = true;

            yield return new WaitForSeconds(0.8f);

            if (cardValues[firstSelectedCard] == cardValues[secondSelectedCard])
            {
                // Par encontrado
                cardRevealed[firstSelectedCard] = true;
                cardRevealed[secondSelectedCard] = true;
                pairsFound++;

                if (pairsFound >= totalPairs)
                {
                    EndGame();
                }
            }
            else
            {
                // No es par - ocultar cartas
                RegisterError();
                SetCardHidden(firstSelectedCard);
                SetCardHidden(secondSelectedCard);
            }

            firstSelectedCard = -1;
            secondSelectedCard = -1;
            isProcessingPair = false;
            UpdateUI();
        }

        private void SetCardRevealed(int cardIndex)
        {
            if (cardImages != null && cardIndex < cardImages.Length)
            {
                int spriteIndex = cardValues[cardIndex];
                if (cardFrontSprites != null && spriteIndex < cardFrontSprites.Length)
                {
                    cardImages[cardIndex].sprite = cardFrontSprites[spriteIndex];
                }
            }
        }

        private void SetCardHidden(int cardIndex)
        {
            if (cardImages != null && cardIndex < cardImages.Length && cardBackSprite != null)
            {
                cardImages[cardIndex].sprite = cardBackSprite;
            }
        }

        private void UpdateUI()
        {
            if (pairsFoundText != null)
            {
                pairsFoundText.text = $"{pairsFound}/{totalPairs}";
            }

            if (errorsText != null)
            {
                errorsText.text = errorCount.ToString();
            }
        }

        protected override void UpdateTimer()
        {
            if (timerText != null)
            {
                timerText.text = GetFormattedTime();
            }
        }

        protected override void OnGameStarted()
        {
            if (winPanel != null) winPanel.SetActive(false);

            foreach (var btn in cardButtons)
            {
                if (btn != null) btn.interactable = true;
            }
        }

        protected override void OnGamePaused() { }

        protected override void OnGameResumed() { }

        protected override void OnGameEnded()
        {
            if (winPanel != null) winPanel.SetActive(true);
        }

        protected override void OnGameReset()
        {
            pairsFound = 0;
            firstSelectedCard = -1;
            secondSelectedCard = -1;
            isProcessingPair = false;

            for (int i = 0; i < cardRevealed.Length; i++)
            {
                cardRevealed[i] = false;
                SetCardHidden(i);
            }

            if (winPanel != null) winPanel.SetActive(false);
        }

        protected override void OnErrorOccurred()
        {
            UpdateUI();
        }
    }
}
