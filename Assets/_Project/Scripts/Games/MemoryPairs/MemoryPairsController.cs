using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.UI;
using DigitPark.Localization;

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
        [SerializeField] private Card3DEffect[] card3DEffects;

        [Header("Memory Pairs - UI")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI pairsFoundText;
        [SerializeField] private TextMeshProUGUI errorsText;
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private CanvasGroup winPanelCanvasGroup;
        [SerializeField] private TextMeshProUGUI statsText;

        [Header("Countdown")]
        [SerializeField] private CountdownUI countdownUI;
        [SerializeField] private bool useCountdown = true;

        [Header("Feedback")]
        [SerializeField] private GameObject feedbackPanel;
        [SerializeField] private TextMeshProUGUI feedbackText;

        [Header("Effects")]
        [SerializeField] private UISparkleEffect sparkleEffect;
        [SerializeField] private bool enableHapticFeedback = true;

        [Header("Digits para las cartas (se eligen 8 aleatorios del 1-9)")]
        [SerializeField] private string[] allDigits = { "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        private string[] selectedDigits; // 8 dígitos aleatorios seleccionados para esta partida

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
            StartGame();
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

        [Header("Preview Settings")]
        [SerializeField] private float previewDuration = 2f;

        private void OnCountdownComplete()
        {
            StartCoroutine(PreviewAndStartSequence());
        }

        private IEnumerator PreviewAndStartSequence()
        {
            // 1. Pop up animation for all cards
            for (int i = 0; i < card3DEffects.Length; i++)
            {
                if (card3DEffects[i] != null)
                    card3DEffects[i].AnimateGameStart();
            }
            yield return new WaitForSeconds(0.8f);

            // 2. Reveal all cards (show numbers) for preview
            for (int i = 0; i < card3DEffects.Length; i++)
            {
                if (card3DEffects[i] != null)
                    card3DEffects[i].FlipCard();
            }
            yield return new WaitForSeconds(previewDuration);

            // 3. Flip all cards back (hide numbers)
            for (int i = 0; i < card3DEffects.Length; i++)
            {
                if (card3DEffects[i] != null)
                    card3DEffects[i].FlipBack();
            }
            yield return new WaitForSeconds(0.5f);

            // 4. Start the game timer
            base.StartGame();
        }

        private void ShuffleCards()
        {
            // Seleccionar 8 dígitos aleatorios del 0-9
            SelectRandomDigits();

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
                    string digit = (pairId < selectedDigits.Length) ? selectedDigits[pairId] : pairId.ToString();
                    card3DEffects[i].SetupCard(pairId, digit);
                    card3DEffects[i].ResetCard();
                }
            }
        }

        /// <summary>
        /// Selecciona 8 dígitos aleatorios del array de 10 dígitos (0-9)
        /// </summary>
        private void SelectRandomDigits()
        {
            // Crear lista de todos los dígitos disponibles
            List<string> available = new List<string>(allDigits);

            // Mezclar la lista
            for (int i = available.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (available[i], available[j]) = (available[j], available[i]);
            }

            // Tomar los primeros 8
            selectedDigits = new string[totalPairs];
            for (int i = 0; i < totalPairs && i < available.Count; i++)
            {
                selectedDigits[i] = available[i];
            }

            Debug.Log($"[MemoryPairs] Dígitos seleccionados: {string.Join(", ", selectedDigits)}");
        }

        private void OnCardClicked(int cardIndex)
        {
            if (!isPlaying || isPaused || isProcessingPair) return;
            if (cardRevealed[cardIndex]) return;
            if (cardIndex == firstSelectedCard) return;

            if (card3DEffects != null && cardIndex < card3DEffects.Length && card3DEffects[cardIndex] != null)
            {
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

                // Toast feedback
                ShowFeedback(AutoLocalizer.Get("memorypairs_correct"), new Color(0.3f, 1f, 0.5f, 1f));

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

                // Toast feedback
                ShowFeedback(AutoLocalizer.Get("memorypairs_incorrect"), new Color(1f, 0.3f, 0.3f, 1f));

                if (card3DEffects != null)
                {
                    if (firstSelectedCard < card3DEffects.Length && card3DEffects[firstSelectedCard] != null)
                        card3DEffects[firstSelectedCard].ShowError();
                    if (secondSelectedCard < card3DEffects.Length && card3DEffects[secondSelectedCard] != null)
                        card3DEffects[secondSelectedCard].ShowError();
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
        }

        private void SetCardHidden(int cardIndex)
        {
            if (card3DEffects != null && cardIndex < card3DEffects.Length && card3DEffects[cardIndex] != null)
            {
                card3DEffects[cardIndex].FlipBack();
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

        private Coroutine feedbackCoroutine;

        private void ShowFeedback(string message, Color color)
        {
            if (feedbackText == null) return;

            if (feedbackCoroutine != null)
                StopCoroutine(feedbackCoroutine);

            feedbackCoroutine = StartCoroutine(AnimateFeedback(message, color));
        }

        private IEnumerator AnimateFeedback(string message, Color color)
        {
            feedbackText.text = message;
            feedbackText.color = color;

            // Update panel outline to match feedback color
            if (feedbackPanel != null)
            {
                feedbackPanel.SetActive(true);
                var outline = feedbackPanel.GetComponent<UnityEngine.UI.Outline>();
                if (outline != null)
                    outline.effectColor = new Color(color.r, color.g, color.b, 0.8f);
            }
            else
            {
                feedbackText.gameObject.SetActive(true);
            }

            // Scale pop-in on the panel (or text if no panel)
            Transform t = feedbackPanel != null ? feedbackPanel.transform : feedbackText.transform;
            t.localScale = Vector3.zero;

            float popDuration = 0.2f;
            float elapsed = 0f;
            while (elapsed < popDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / popDuration;
                float scale = 1f + 0.3f * Mathf.Sin(progress * Mathf.PI);
                t.localScale = Vector3.one * Mathf.Min(scale, 1.3f) * progress;
                yield return null;
            }
            t.localScale = Vector3.one;

            // Hold
            yield return new WaitForSeconds(0.8f);

            // Fade out
            float fadeDuration = 0.3f;
            elapsed = 0f;
            Color startColor = feedbackText.color;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f - (elapsed / fadeDuration);
                feedbackText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                t.localScale = Vector3.one * (0.8f + 0.2f * alpha);
                yield return null;
            }

            if (feedbackPanel != null)
                feedbackPanel.SetActive(false);
            else
                feedbackText.gameObject.SetActive(false);

            feedbackCoroutine = null;
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
            // Disable card interaction - base class ShowResultPanel handles panel display
            foreach (var btn in cardButtons)
            {
                if (btn != null) btn.interactable = false;
            }
        }

        private IEnumerator ShowWinPanel()
        {
            // Actualizar stats
            if (statsText != null)
            {
                statsText.text = $"{AutoLocalizer.Get("game_time")}: {GetFormattedTime()}\n{AutoLocalizer.Get("game_errors")}: {errorCount}\n{AutoLocalizer.Get("memorypairs_pairs_found")}: {pairsFound}/{totalPairs}\nMax Combo: x{maxCombo}";
            }

            // Fade in
            if (winPanelCanvasGroup == null) yield break;

            winPanelCanvasGroup.alpha = 0f;
            float duration = 0.4f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                winPanelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }

            winPanelCanvasGroup.alpha = 1f;
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
            }

            if (sparkleEffect != null)
            {
                sparkleEffect.ClearAllParticles();
            }

            if (winPanel != null) winPanel.SetActive(false);
            if (winPanelCanvasGroup != null) winPanelCanvasGroup.alpha = 0;
        }

        protected override void OnErrorOccurred()
        {
            UpdateUI();
        }

        #endregion
    }
}
