using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.UI;

namespace DigitPark.Games
{
    /// <summary>
    /// Controller para el juego Odd One Out
    /// El jugador debe encontrar la diferencia entre DOS cuadriculas 4x4
    /// Con sistema de combos, partículas y vibración háptica
    /// </summary>
    public class OddOneOutController : MinigameBase
    {
        public override GameType GameType => GameType.OddOneOut;

        [Header("Odd One Out - Grid Izquierda")]
        [SerializeField] private Button[] leftGridButtons;
        [SerializeField] private TextMeshProUGUI[] leftButtonTexts;
        [SerializeField] private Image[] leftButtonImages;

        [Header("Odd One Out - Grid Derecha")]
        [SerializeField] private Button[] rightGridButtons;
        [SerializeField] private TextMeshProUGUI[] rightButtonTexts;
        [SerializeField] private Image[] rightButtonImages;

        [Header("Odd One Out - UI")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI roundText;
        [SerializeField] private TextMeshProUGUI errorsText;
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private CanvasGroup winPanelCanvasGroup;
        // playAgainButton ya está definido en MinigameBase

        [Header("Effects")]
        [SerializeField] private UISparkleEffect sparkleEffect;
        [SerializeField] private bool enableHapticFeedback = true;

        [Header("Odd One Out - Settings")]
        [SerializeField] private int totalRounds = 5;

        // Estado del juego
        private int currentRound;
        private int oddButtonIndex;
        private bool isDifferenceOnRight;
        private int gridSize = 16;

        // Combo System
        private int currentCombo = 0;
        private int maxCombo = 0;

        // Pares confusos
        private readonly string[][] confusablePairs = new string[][]
        {
            new string[] { "6", "9" },
            new string[] { "O", "0" },
            new string[] { "I", "1" },
            new string[] { "S", "5" },
            new string[] { "B", "8" },
            new string[] { "Z", "2" },
            new string[] { "b", "d" },
            new string[] { "p", "q" },
            new string[] { "n", "u" },
            new string[] { "M", "W" },
            new string[] { "E", "F" },
            new string[] { "C", "G" },
            new string[] { "P", "R" },
            new string[] { "V", "U" },
            new string[] { "N", "Z" }
        };

        protected override void Awake()
        {
            base.Awake();
            totalRounds = config?.rounds ?? 5;
            gridSize = config?.TotalGridElements ?? 16;

            // Buscar UISparkleEffect si no está asignado
            if (sparkleEffect == null)
            {
                sparkleEffect = FindFirstObjectByType<UISparkleEffect>();
            }
        }

        protected override void Start()
        {
            base.Start();
            SetupButtons();
            // playAgainButton listener ya se configura en MinigameBase
        }

        private void SetupButtons()
        {
            if (leftGridButtons != null)
            {
                for (int i = 0; i < leftGridButtons.Length; i++)
                {
                    int index = i;
                    if (leftGridButtons[i] != null)
                        leftGridButtons[i].onClick.AddListener(() => OnButtonClicked(index, false));
                }
            }

            if (rightGridButtons != null)
            {
                for (int i = 0; i < rightGridButtons.Length; i++)
                {
                    int index = i;
                    if (rightGridButtons[i] != null)
                        rightGridButtons[i].onClick.AddListener(() => OnButtonClicked(index, true));
                }
            }
        }

        public override void StartGame()
        {
            base.StartGame();
            currentRound = 1;
            currentCombo = 0;
            maxCombo = 0;
            GeneratePuzzle();
            UpdateUI();
            AnimateRoundStart();
        }

        private void GeneratePuzzle()
        {
            int pairIndex = Random.Range(0, confusablePairs.Length);
            string baseChar = confusablePairs[pairIndex][0];
            string differentChar = confusablePairs[pairIndex][1];

            oddButtonIndex = Random.Range(0, gridSize);
            isDifferenceOnRight = Random.Range(0, 2) == 1;

            for (int i = 0; i < gridSize; i++)
            {
                // Grid izquierda
                if (leftButtonTexts != null && i < leftButtonTexts.Length && leftButtonTexts[i] != null)
                {
                    if (!isDifferenceOnRight && i == oddButtonIndex)
                        leftButtonTexts[i].text = differentChar;
                    else
                        leftButtonTexts[i].text = baseChar;
                }

                // Grid derecha
                if (rightButtonTexts != null && i < rightButtonTexts.Length && rightButtonTexts[i] != null)
                {
                    if (isDifferenceOnRight && i == oddButtonIndex)
                        rightButtonTexts[i].text = differentChar;
                    else
                        rightButtonTexts[i].text = baseChar;
                }

                // Resetear celdas 3D
                ResetCell(i, false);
                ResetCell(i, true);
            }

            EnableAllButtons(true);

            if (instructionText != null)
            {
                instructionText.text = "¡ENCUENTRA LA DIFERENCIA!";
                instructionText.color = new Color(1f, 0.84f, 0f, 1f);
            }
        }

        private void ResetCell(int index, bool isRight)
        {
            Button[] buttons = isRight ? rightGridButtons : leftGridButtons;
            if (buttons != null && index < buttons.Length && buttons[index] != null)
            {
                OddOneOutCell3D cell3D = buttons[index].GetComponent<OddOneOutCell3D>();
                if (cell3D != null)
                {
                    cell3D.ResetToNormal();
                }
            }
        }

        private void AnimateRoundStart()
        {
            // Animar celdas con pop-up escalonado
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    int index = row * 4 + col;
                    float delay = (row + col) * 0.03f;

                    AnimateCellStart(index, false, delay);
                    AnimateCellStart(index, true, delay + 0.02f);
                }
            }
        }

        private void AnimateCellStart(int index, bool isRight, float delay)
        {
            Button[] buttons = isRight ? rightGridButtons : leftGridButtons;
            if (buttons != null && index < buttons.Length && buttons[index] != null)
            {
                OddOneOutCell3D cell3D = buttons[index].GetComponent<OddOneOutCell3D>();
                if (cell3D != null)
                {
                    cell3D.AnimateRoundStart(delay);
                }
            }
        }

        private void EnableAllButtons(bool enable)
        {
            if (leftGridButtons != null)
            {
                foreach (var btn in leftGridButtons)
                {
                    if (btn != null) btn.interactable = enable;
                }
            }

            if (rightGridButtons != null)
            {
                foreach (var btn in rightGridButtons)
                {
                    if (btn != null) btn.interactable = enable;
                }
            }
        }

        private void OnButtonClicked(int buttonIndex, bool isRightGrid)
        {
            if (!isPlaying || isPaused) return;

            bool isCorrect = (buttonIndex == oddButtonIndex) && (isRightGrid == isDifferenceOnRight);

            if (isCorrect)
            {
                OnCorrectAnswer(buttonIndex, isRightGrid);
                currentRound++;
                UpdateUI();

                if (currentRound > totalRounds)
                {
                    StartCoroutine(EndGameAfterDelay());
                }
                else
                {
                    StartCoroutine(NextRoundAfterDelay());
                }
            }
            else
            {
                OnWrongAnswer(buttonIndex, isRightGrid);
            }
        }

        private void OnCorrectAnswer(int buttonIndex, bool isRightGrid)
        {
            Debug.Log($"[OddOneOut] ¡Correcto! Ronda {currentRound}");

            // Incrementar combo
            currentCombo++;
            if (currentCombo > maxCombo) maxCombo = currentCombo;

            // Vibración según combo
            if (currentCombo >= 4)
                TriggerHaptic(HapticType.Heavy);
            else if (currentCombo >= 2)
                TriggerHaptic(HapticType.Medium);
            else
                TriggerHaptic(HapticType.Light);

            // Partículas de acierto
            PlayCorrectParticles(buttonIndex, isRightGrid);

            // Animar celda correcta
            AnimateCellCorrect(buttonIndex, isRightGrid);

            // Actualizar combo display
            UpdateComboDisplay();

            // Mensaje de éxito
            if (instructionText != null)
            {
                if (currentCombo >= 4)
                    instructionText.text = "¡INCREÍBLE!";
                else if (currentCombo >= 2)
                    instructionText.text = "¡EXCELENTE!";
                else
                    instructionText.text = "¡CORRECTO!";

                instructionText.color = new Color(0.3f, 1f, 0.5f, 1f);
            }
        }

        private void OnWrongAnswer(int buttonIndex, bool isRightGrid)
        {
            Debug.Log($"[OddOneOut] Incorrecto en posición {buttonIndex}");

            RegisterError();

            // Resetear combo
            currentCombo = 0;
            UpdateComboDisplay();

            // Vibración de error
            TriggerHaptic(HapticType.Error);

            // Partículas de error
            PlayErrorParticles(buttonIndex, isRightGrid);

            // Animar celda error
            AnimateCellError(buttonIndex, isRightGrid);

            // Mensaje de error
            if (instructionText != null)
            {
                instructionText.text = "¡INTENTA DE NUEVO!";
                instructionText.color = new Color(1f, 0.3f, 0.3f, 1f);
            }
        }

        private void AnimateCellCorrect(int buttonIndex, bool isRightGrid)
        {
            Button[] buttons = isRightGrid ? rightGridButtons : leftGridButtons;
            if (buttons != null && buttonIndex < buttons.Length && buttons[buttonIndex] != null)
            {
                OddOneOutCell3D cell3D = buttons[buttonIndex].GetComponent<OddOneOutCell3D>();
                if (cell3D != null)
                {
                    cell3D.AnimateCorrect(currentCombo);
                }
            }
        }

        private void AnimateCellError(int buttonIndex, bool isRightGrid)
        {
            Button[] buttons = isRightGrid ? rightGridButtons : leftGridButtons;
            if (buttons != null && buttonIndex < buttons.Length && buttons[buttonIndex] != null)
            {
                OddOneOutCell3D cell3D = buttons[buttonIndex].GetComponent<OddOneOutCell3D>();
                if (cell3D != null)
                {
                    cell3D.AnimateError();
                }
            }
        }

        #region Particle Effects

        private void PlayCorrectParticles(int buttonIndex, bool isRightGrid)
        {
            if (sparkleEffect == null) return;

            Vector2 pos = GetCellPosition(buttonIndex, isRightGrid);
            sparkleEffect.PlayMatchSparkles(pos, currentCombo);

            if (currentCombo >= 3)
            {
                sparkleEffect.PlayStarBurst(pos, currentCombo - 1);
            }
        }

        private void PlayErrorParticles(int buttonIndex, bool isRightGrid)
        {
            if (sparkleEffect == null) return;

            Vector2 pos = GetCellPosition(buttonIndex, isRightGrid);
            sparkleEffect.PlayErrorSparkles(pos);
        }

        private Vector2 GetCellPosition(int buttonIndex, bool isRightGrid)
        {
            Button[] buttons = isRightGrid ? rightGridButtons : leftGridButtons;
            if (buttons != null && buttonIndex < buttons.Length && buttons[buttonIndex] != null)
            {
                RectTransform rt = buttons[buttonIndex].GetComponent<RectTransform>();
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
                    Vibrate(10);
                    break;
                case HapticType.Medium:
                    Vibrate(25);
                    break;
                case HapticType.Heavy:
                    Vibrate(50);
                    break;
                case HapticType.Error:
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
                Handheld.Vibrate();
            }
#elif UNITY_IOS && !UNITY_EDITOR
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

        #region Combo Display

        private void UpdateComboDisplay()
        {
            if (comboText == null) return;

            if (currentCombo >= 2)
            {
                comboText.gameObject.SetActive(true);
                comboText.text = $"x{currentCombo}";

                // Color basado en combo
                if (currentCombo >= 5)
                    comboText.color = new Color(1f, 0.85f, 0.2f); // Dorado
                else if (currentCombo >= 4)
                    comboText.color = new Color(1f, 0.5f, 0.2f); // Naranja
                else if (currentCombo >= 3)
                    comboText.color = new Color(0.3f, 1f, 0.5f); // Verde
                else
                    comboText.color = new Color(0f, 0.9f, 1f); // Cyan
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Victory

        private IEnumerator NextRoundAfterDelay()
        {
            EnableAllButtons(false);
            yield return new WaitForSeconds(0.6f);
            GeneratePuzzle();
            UpdateUI();
            AnimateRoundStart();
        }

        private IEnumerator EndGameAfterDelay()
        {
            EnableAllButtons(false);
            yield return new WaitForSeconds(0.3f);

            // Vibración de victoria
            TriggerHaptic(HapticType.Heavy);

            // Animación de victoria en ambos grids
            StartCoroutine(PlayVictorySequence());
        }

        private IEnumerator PlayVictorySequence()
        {
            // Animación de ola en ambos grids
            float waveDelay = 0.05f;

            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    int index = row * 4 + col;
                    float delay = (row + col) * waveDelay;

                    // Grid izquierda
                    if (leftGridButtons != null && index < leftGridButtons.Length && leftGridButtons[index] != null)
                    {
                        OddOneOutCell3D cell3D = leftGridButtons[index].GetComponent<OddOneOutCell3D>();
                        if (cell3D != null)
                        {
                            cell3D.PlayVictoryCelebration(delay);
                        }
                    }

                    // Grid derecha
                    if (rightGridButtons != null && index < rightGridButtons.Length && rightGridButtons[index] != null)
                    {
                        OddOneOutCell3D cell3D = rightGridButtons[index].GetComponent<OddOneOutCell3D>();
                        if (cell3D != null)
                        {
                            cell3D.PlayVictoryCelebration(delay + 0.02f);
                        }
                    }
                }
            }

            // Confeti
            if (sparkleEffect != null)
            {
                sparkleEffect.PlayVictoryConfetti();

                yield return new WaitForSeconds(0.4f);
                sparkleEffect.PlayConfettiExplosion(Vector2.zero);
            }

            yield return new WaitForSeconds(0.6f);

            EndGame();
        }

        #endregion

        private void UpdateUI()
        {
            if (roundText != null)
            {
                roundText.text = $"{Mathf.Min(currentRound, totalRounds)}/{totalRounds}";
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
            EnableAllButtons(true);
            UpdateComboDisplay();
        }

        protected override void OnGamePaused() { }

        protected override void OnGameResumed() { }

        protected override void OnGameEnded()
        {
            if (winPanel != null)
            {
                winPanel.SetActive(true);
                StartCoroutine(ShowWinPanel());
            }
            EnableAllButtons(false);
        }

        private IEnumerator ShowWinPanel()
        {
            if (winPanelCanvasGroup == null) yield break;

            // Actualizar stats
            if (statsText != null)
            {
                statsText.text = $"Tiempo: {GetFormattedTime()}\nErrores: {errorCount}\nMax Combo: x{maxCombo}";
            }

            // Fade in
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
            currentRound = 1;
            currentCombo = 0;
            maxCombo = 0;
            if (winPanel != null) winPanel.SetActive(false);
            if (winPanelCanvasGroup != null) winPanelCanvasGroup.alpha = 0;
            EnableAllButtons(true);
            UpdateComboDisplay();
        }

        protected override void OnErrorOccurred()
        {
            UpdateUI();
        }

        // RestartGame se maneja en MinigameBase.OnPlayAgainClicked()
    }
}
