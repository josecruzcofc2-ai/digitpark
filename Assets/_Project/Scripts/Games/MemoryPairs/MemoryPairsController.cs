using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.UI;

namespace DigitPark.Games
{
    /// <summary>
    /// Controller para el juego Memory Pairs
    /// El jugador debe encontrar todos los pares de cartas iguales
    /// Con efectos 3D, combos, partículas y vibración háptica
    /// </summary>
    public class MemoryPairsController : MinigameBase
    {
        public override GameType GameType => GameType.MemoryPairs;

        [Header("Memory Pairs - Grid")]
        [SerializeField] private Button[] cardButtons;
        [SerializeField] private Image[] cardImages;
        [SerializeField] private Card3DEffect[] card3DEffects;

        [Header("Memory Pairs - UI")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI pairsFoundText;
        [SerializeField] private TextMeshProUGUI errorsText;
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private GameObject winPanel;

        [Header("Countdown")]
        [SerializeField] private CountdownUI countdownUI;
        [SerializeField] private bool useCountdown = true;

        [Header("Effects")]
        [SerializeField] private UISparkleEffect sparkleEffect;
        [SerializeField] private bool enableHapticFeedback = true;

        [Header("Memory Pairs - Sprites")]
        [SerializeField] private Sprite cardBackSprite;
        [SerializeField] private Sprite[] cardFrontSprites;

        [Header("Digits para las cartas")]
        [SerializeField] private string[] cardDigits = { "0", "1", "2", "3", "4", "5", "6", "7" };

        // Estado del juego
        private int[] cardValues;
        private bool[] cardRevealed;
        private int firstSelectedCard = -1;
        private int secondSelectedCard = -1;
        private bool isProcessingPair;
        private int pairsFound;
        private int totalPairs = 8;

        // Sistema de combo
        private int currentCombo = 0;
        private int maxCombo = 0;
        private float lastMatchTime = 0f;
        private const float COMBO_TIMEOUT = 5f;

        protected override void Awake()
        {
            base.Awake();
            int gridSize = config?.TotalGridElements ?? 16;
            totalPairs = gridSize / 2;
            cardValues = new int[gridSize];
            cardRevealed = new bool[gridSize];

            // Buscar UISparkleEffect si no está asignado
            if (sparkleEffect == null)
            {
                sparkleEffect = FindFirstObjectByType<UISparkleEffect>();
            }
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

            if (card3DEffects == null || card3DEffects.Length == 0)
            {
                card3DEffects = new Card3DEffect[cardButtons.Length];
                for (int i = 0; i < cardButtons.Length; i++)
                {
                    card3DEffects[i] = cardButtons[i].GetComponent<Card3DEffect>();
                }
            }

            for (int i = 0; i < cardButtons.Length; i++)
            {
                int index = i;
                cardButtons[i].onClick.AddListener(() => OnCardClicked(index));

                if (card3DEffects[i] != null)
                {
                    card3DEffects[i].OnCardFlipped += OnCard3DFlipped;
                }
            }
        }

        private void OnCard3DFlipped(Card3DEffect card)
        {
            // Evento cuando termina la animación de flip
        }

        public override void StartGame()
        {
            pairsFound = 0;
            isProcessingPair = false;
            firstSelectedCard = -1;
            secondSelectedCard = -1;
            currentCombo = 0;
            maxCombo = 0;

            ShuffleCards();
            SetAllCardsWaiting();
            UpdateUI();

            if (useCountdown && countdownUI != null)
            {
                countdownUI.StartCountdown(OnCountdownComplete);
            }
            else
            {
                OnCountdownComplete();
            }
        }

        private void SetAllCardsWaiting()
        {
            for (int i = 0; i < card3DEffects.Length; i++)
            {
                if (card3DEffects[i] != null)
                {
                    card3DEffects[i].SetWaitingState();
                }
            }
        }

        private void OnCountdownComplete()
        {
            AnimateCardsPopUp();
            StartCoroutine(ActivateGameAfterDelay(0.5f));
        }

        private void AnimateCardsPopUp()
        {
            for (int i = 0; i < card3DEffects.Length; i++)
            {
                if (card3DEffects[i] != null)
                {
                    card3DEffects[i].AnimateGameStart();
                }
            }
        }

        private IEnumerator ActivateGameAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            base.StartGame();
        }

        private void ShuffleCards()
        {
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

            for (int i = 0; i < cardValues.Length && i < cards.Count; i++)
            {
                cardValues[i] = cards[i];
                cardRevealed[i] = false;

                if (card3DEffects != null && i < card3DEffects.Length && card3DEffects[i] != null)
                {
                    int pairId = cards[i];
                    string digit = (pairId < cardDigits.Length) ? cardDigits[pairId] : pairId.ToString();
                    card3DEffects[i].SetupCard(pairId, digit);

                    if (cardFrontSprites != null && pairId < cardFrontSprites.Length)
                    {
                        card3DEffects[i].SetCardSprite(cardFrontSprites[pairId]);
                    }

                    card3DEffects[i].ResetCard();
                }
                else
                {
                    SetCardHidden(i);
                }
            }
        }

        private void OnCardClicked(int cardIndex)
        {
            if (!isPlaying || isPaused || isProcessingPair) return;
            if (cardRevealed[cardIndex]) return;
            if (cardIndex == firstSelectedCard) return;

            if (card3DEffects != null && cardIndex < card3DEffects.Length && card3DEffects[cardIndex] != null)
            {
                if (card3DEffects[cardIndex].IsAnimating) return;
                if (card3DEffects[cardIndex].IsFaceUp) return;
            }

            // Vibración leve al tocar carta
            TriggerHaptic(HapticType.Light);

            SetCardRevealed(cardIndex);

            if (firstSelectedCard == -1)
            {
                firstSelectedCard = cardIndex;
            }
            else
            {
                secondSelectedCard = cardIndex;
                StartCoroutine(ProcessPair());
            }
        }

        private IEnumerator ProcessPair()
        {
            isProcessingPair = true;

            yield return new WaitForSeconds(0.5f);

            if (cardValues[firstSelectedCard] == cardValues[secondSelectedCard])
            {
                // ===== PAR ENCONTRADO =====
                cardRevealed[firstSelectedCard] = true;
                cardRevealed[secondSelectedCard] = true;
                pairsFound++;

                // Actualizar combo
                if (Time.time - lastMatchTime < COMBO_TIMEOUT || currentCombo == 0)
                {
                    currentCombo++;
                    if (currentCombo > maxCombo) maxCombo = currentCombo;
                }
                else
                {
                    currentCombo = 1;
                }
                lastMatchTime = Time.time;

                // Vibración de éxito (más fuerte con combo)
                TriggerHaptic(currentCombo >= 3 ? HapticType.Heavy : HapticType.Medium);

                // Animar como matched con nivel de combo
                if (card3DEffects != null)
                {
                    if (firstSelectedCard < card3DEffects.Length && card3DEffects[firstSelectedCard] != null)
                        card3DEffects[firstSelectedCard].MarkAsMatched(currentCombo);
                    if (secondSelectedCard < card3DEffects.Length && card3DEffects[secondSelectedCard] != null)
                        card3DEffects[secondSelectedCard].MarkAsMatched(currentCombo);
                }

                // Partículas de match
                PlayMatchParticles(firstSelectedCard, secondSelectedCard);

                yield return new WaitForSeconds(0.3f);

                if (pairsFound >= totalPairs)
                {
                    // ===== VICTORIA =====
                    yield return StartCoroutine(PlayVictorySequence());
                    EndGame();
                }
            }
            else
            {
                // ===== ERROR =====
                currentCombo = 0; // Resetear combo
                RegisterError();

                // Vibración de error
                TriggerHaptic(HapticType.Error);

                // Partículas de error
                PlayErrorParticles(firstSelectedCard, secondSelectedCard);

                if (card3DEffects != null)
                {
                    if (firstSelectedCard < card3DEffects.Length && card3DEffects[firstSelectedCard] != null)
                        card3DEffects[firstSelectedCard].ShowError();
                    if (secondSelectedCard < card3DEffects.Length && card3DEffects[secondSelectedCard] != null)
                        card3DEffects[secondSelectedCard].ShowError();
                }
                else
                {
                    yield return new WaitForSeconds(0.5f);
                    SetCardHidden(firstSelectedCard);
                    SetCardHidden(secondSelectedCard);
                }
            }

            firstSelectedCard = -1;
            secondSelectedCard = -1;
            isProcessingPair = false;
            UpdateUI();
        }

        #region Particle Effects

        private void PlayMatchParticles(int card1, int card2)
        {
            if (sparkleEffect == null) return;

            // Obtener posiciones de las cartas
            Vector2 pos1 = GetCardPosition(card1);
            Vector2 pos2 = GetCardPosition(card2);

            // Chispas en ambas cartas
            sparkleEffect.PlayMatchSparkles(pos1, currentCombo);
            sparkleEffect.PlayMatchSparkles(pos2, currentCombo);

            // Estrellas extra para combo alto
            if (currentCombo >= 3)
            {
                sparkleEffect.PlayStarBurst(pos1, currentCombo);
                sparkleEffect.PlayStarBurst(pos2, currentCombo);
            }
        }

        private void PlayErrorParticles(int card1, int card2)
        {
            if (sparkleEffect == null) return;

            Vector2 pos1 = GetCardPosition(card1);
            Vector2 pos2 = GetCardPosition(card2);

            sparkleEffect.PlayErrorSparkles(pos1);
            sparkleEffect.PlayErrorSparkles(pos2);
        }

        private Vector2 GetCardPosition(int cardIndex)
        {
            if (card3DEffects != null && cardIndex < card3DEffects.Length && card3DEffects[cardIndex] != null)
            {
                RectTransform rt = card3DEffects[cardIndex].GetComponent<RectTransform>();
                if (rt != null)
                {
                    return rt.anchoredPosition;
                }
            }
            return Vector2.zero;
        }

        #endregion

        #region Haptic Feedback

        private enum HapticType
        {
            Light,
            Medium,
            Heavy,
            Error
        }

        private void TriggerHaptic(HapticType type)
        {
            if (!enableHapticFeedback) return;

#if UNITY_ANDROID || UNITY_IOS
            switch (type)
            {
                case HapticType.Light:
                    // Vibración corta y suave
                    Vibrate(10);
                    break;
                case HapticType.Medium:
                    // Vibración media
                    Vibrate(25);
                    break;
                case HapticType.Heavy:
                    // Vibración fuerte
                    Vibrate(50);
                    break;
                case HapticType.Error:
                    // Patrón de error: dos vibraciones cortas
                    StartCoroutine(ErrorVibratePattern());
                    break;
            }
#endif
        }

        private void Vibrate(long milliseconds)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    AndroidJavaObject vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");

                    if (vibrator != null)
                    {
                        // API 26+
                        using (AndroidJavaClass vibrationEffect = new AndroidJavaClass("android.os.VibrationEffect"))
                        {
                            AndroidJavaObject effect = vibrationEffect.CallStatic<AndroidJavaObject>(
                                "createOneShot", milliseconds, -1);
                            vibrator.Call("vibrate", effect);
                        }
                    }
                }
            }
            catch (System.Exception)
            {
                // Fallback simple
                Handheld.Vibrate();
            }
#elif UNITY_IOS && !UNITY_EDITOR
            // iOS usa el sistema de haptic feedback del dispositivo
            Handheld.Vibrate();
#endif
        }

        private IEnumerator ErrorVibratePattern()
        {
            Vibrate(30);
            yield return new WaitForSeconds(0.1f);
            Vibrate(30);
        }

        #endregion

        #region Victory Sequence

        private IEnumerator PlayVictorySequence()
        {
            // Vibración de victoria
            TriggerHaptic(HapticType.Heavy);
            yield return new WaitForSeconds(0.1f);
            TriggerHaptic(HapticType.Heavy);

            // Animación de "ola" de celebración en las cartas
            float waveDelay = 0.05f;
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    int index = row * 4 + col;
                    if (card3DEffects != null && index < card3DEffects.Length && card3DEffects[index] != null)
                    {
                        float delay = (row + col) * waveDelay;
                        card3DEffects[index].PlayVictoryCelebration(delay);
                    }
                }
            }

            // Confeti
            if (sparkleEffect != null)
            {
                sparkleEffect.PlayVictoryConfetti();

                // Explosión de confeti desde el centro
                yield return new WaitForSeconds(0.3f);
                sparkleEffect.PlayConfettiExplosion(Vector2.zero);
            }

            yield return new WaitForSeconds(1f);
        }

        #endregion

        #region Card State Methods

        private void SetCardRevealed(int cardIndex)
        {
            if (card3DEffects != null && cardIndex < card3DEffects.Length && card3DEffects[cardIndex] != null)
            {
                card3DEffects[cardIndex].FlipCard();
            }
            else if (cardImages != null && cardIndex < cardImages.Length)
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
            if (card3DEffects != null && cardIndex < card3DEffects.Length && card3DEffects[cardIndex] != null)
            {
                card3DEffects[cardIndex].FlipBack();
            }
            else if (cardImages != null && cardIndex < cardImages.Length && cardBackSprite != null)
            {
                cardImages[cardIndex].sprite = cardBackSprite;
            }
        }

        #endregion

        #region UI Updates

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

            if (comboText != null)
            {
                if (currentCombo >= 2)
                {
                    comboText.gameObject.SetActive(true);
                    comboText.text = $"x{currentCombo}";

                    // Color basado en combo
                    if (currentCombo >= 5)
                        comboText.color = new Color(1f, 0.85f, 0.2f); // Dorado
                    else if (currentCombo >= 3)
                        comboText.color = new Color(1f, 0.5f, 0.2f); // Naranja
                    else
                        comboText.color = new Color(0.3f, 1f, 0.5f); // Verde
                }
                else
                {
                    comboText.gameObject.SetActive(false);
                }
            }
        }

        protected override void UpdateTimer()
        {
            if (timerText != null)
            {
                timerText.text = GetFormattedTime();
            }

            // Verificar timeout de combo
            if (currentCombo > 0 && Time.time - lastMatchTime > COMBO_TIMEOUT)
            {
                currentCombo = 0;
                UpdateUI();
            }
        }

        #endregion

        #region Game State Callbacks

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
            currentCombo = 0;
            maxCombo = 0;

            for (int i = 0; i < cardRevealed.Length; i++)
            {
                cardRevealed[i] = false;

                if (card3DEffects != null && i < card3DEffects.Length && card3DEffects[i] != null)
                {
                    card3DEffects[i].ResetCard();
                }
                else
                {
                    SetCardHidden(i);
                }
            }

            if (sparkleEffect != null)
            {
                sparkleEffect.ClearAllParticles();
            }

            if (winPanel != null) winPanel.SetActive(false);
        }

        protected override void OnErrorOccurred()
        {
            UpdateUI();
        }

        #endregion
    }
}
