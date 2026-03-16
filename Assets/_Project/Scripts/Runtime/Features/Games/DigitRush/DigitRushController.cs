using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DigitPark.Services.Firebase;
using DigitPark.Data;
using DigitPark.Localization;
using DigitPark.UI;
using DigitPark.Games;
using DigitPark.Services;
using DigitPark.Animations;

namespace DigitPark.Managers
{
    /// <summary>
    /// Main manager for the DigitRush game
    /// Controls the core 3x3 grid mechanic and the 1-9 number sequence
    /// With combo system, particles, and haptic vibration
    /// </summary>
    public class DigitRushController : MonoBehaviour
    {
        public static DigitRushController Instance { get; private set; }

        [Header("Grid")]
        [SerializeField] private Button[] gridButtons; // 9 buttons of the 3x3 grid

        [Header("Game UI")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button backButton;

        [Header("Stats Bar")]
        [SerializeField] private TextMeshProUGUI roundText;
        [SerializeField] private TextMeshProUGUI errorsText;
        [SerializeField] private RectTransform progressFill;
        [SerializeField] private TextMeshProUGUI roundIndicatorText;

        [Header("Settings Panel")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Toggle toggleRounds1;
        [SerializeField] private Toggle toggleRounds3;
        [SerializeField] private Toggle toggleRounds5;
        [SerializeField] private Button startGameButton;

        [Header("Countdown")]
        [SerializeField] private CountdownUI countdownUI;
        [SerializeField] private bool useCountdown = true;

        [Header("Effects")]
        [SerializeField] private UISparkleEffect sparkleEffect;
        [SerializeField] private bool enableHapticFeedback = true;

        [Header("Win Message")]
        [SerializeField] private GameObject winMessagePanel;
        [SerializeField] private CanvasGroup winMessageCanvasGroup;
        [SerializeField] private TextMeshProUGUI successText;

        [Header("Result Panel (Practice - new)")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private CanvasGroup resultPanelCanvasGroup;
        [SerializeField] private TextMeshProUGUI resultTitleText;
        [SerializeField] private TextMeshProUGUI resultTimeText;
        [SerializeField] private TextMeshProUGUI resultMessageText;
        [SerializeField] private Button resultPlayAgainButton;
        [SerializeField] private Button resultExitButton;

        [Header("Win/Lose Panels (Cash Battle)")]
        [SerializeField] private WinPanelController winPanelRealMoney;
        [SerializeField] private WinPanelController losePanelRealMoney;

        [Header("Premium Banner (Result Screen)")]
        [SerializeField] private GameObject premiumBannerContainer;
        [SerializeField] private Button premiumBannerButton;
        [SerializeField] private TextMeshProUGUI premiumBannerText;

        // Game State
        private int currentTargetNumber = 1;
        private bool isGameActive = false;
        private bool isTimerStarted = false;

        // Timer
        private float currentTime = 0f;
        private float bestTime = float.MaxValue;

        // Round System
        private int currentRound = 1;
        private int totalRounds = 1;
        private int totalErrors = 0;
        private float cumulativeTime = 0f;

        // Combo System
        private int currentCombo = 0;
        private int maxCombo = 0;
        private ComboVisualController comboVisualController;

        // Player Data
        private PlayerData currentPlayer;

        // Numbers assigned to each button
        private int[] gridNumbers = new int[9];

        // Shake coroutines
        private Coroutine[] shakeCoroutines = new Coroutine[9];
        private Vector2[] shakeOriginalPositions = new Vector2[9];

        // Success messages
        private readonly string[] level1Keys = {
            "msg_godlike_focus", "msg_mind_on_fire", "msg_exceptional_reflexes",
            "msg_neural_perfection", "msg_time_master", "msg_superhuman",
            "msg_unstoppable_force", "msg_legendary_speed", "msg_pure_genius", "msg_absolute_legend"
        };
        private readonly string[] level2Keys = {
            "msg_incredible_focus", "msg_blazing_fast", "msg_sharp_mind",
            "msg_impressive_reflexes", "msg_excellent_timing", "msg_on_fire",
            "msg_amazing_speed", "msg_brilliant_play", "msg_stellar_performance", "msg_remarkable"
        };
        private readonly string[] level3Keys = {
            "msg_great_job", "msg_well_played", "msg_nice_speed", "msg_good_reflexes", "msg_solid_time"
        };
        private readonly string[] level4Keys = {
            "msg_good_effort", "msg_not_bad", "msg_keep_going", "msg_nice_try", "msg_getting_better"
        };
        private readonly string[] level5Keys = {
            "msg_completed", "msg_done", "msg_finished", "msg_keep_practicing", "msg_you_can_improve"
        };
        private readonly string[] level6Keys = {
            "msg_almost_there", "msg_breathe_continue", "msg_next_will_be_better",
            "msg_dont_give_up", "msg_patience_wins", "msg_every_try_counts",
            "msg_progress_not_perfection", "msg_keep_calm", "msg_believe_yourself", "msg_stay_focused"
        };

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            // Find UISparkleEffect if not assigned
            if (sparkleEffect == null)
            {
                sparkleEffect = FindFirstObjectByType<UISparkleEffect>();
            }
        }

        private void Start()
        {
            Debug.Log("[Game] DigitRushController started");
            LoadPlayerData();
            SetupListeners();
            SetupSettingsPanel();

            // Initialize combo visual controller
            comboVisualController = gameObject.AddComponent<ComboVisualController>();
            Canvas canvas = gridButtons[0].GetComponentInParent<Canvas>();
            comboVisualController.Initialize(canvas);

            if (IsPracticeMode() && settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }
            else
            {
                // Read rounds from GameContext for competitive modes
                var ctx = GameSessionManager.Instance?.CurrentContext;
                if (ctx != null && ctx.Rounds > 0)
                    totalRounds = ctx.Rounds;

                StartNewGame();
            }
        }

        private void SetupSettingsPanel()
        {
            if (toggleRounds1 != null)
                toggleRounds1.onValueChanged.AddListener(v => { UpdateToggleVisual(toggleRounds1, v); if (v) SetRadio(toggleRounds1); });
            if (toggleRounds3 != null)
                toggleRounds3.onValueChanged.AddListener(v => { UpdateToggleVisual(toggleRounds3, v); if (v) SetRadio(toggleRounds3); });
            if (toggleRounds5 != null)
                toggleRounds5.onValueChanged.AddListener(v => { UpdateToggleVisual(toggleRounds5, v); if (v) SetRadio(toggleRounds5); });
            if (startGameButton != null)
                startGameButton.onClick.AddListener(OnStartGameClicked);

            SetToggleDefault(toggleRounds1, true);
            SetToggleDefault(toggleRounds3, false);
            SetToggleDefault(toggleRounds5, false);
        }

        private void SetRadio(Toggle active)
        {
            Toggle[] all = { toggleRounds1, toggleRounds3, toggleRounds5 };
            foreach (var t in all)
            {
                if (t != null && t != active) { t.isOn = false; UpdateToggleVisual(t, false); }
            }
            UpdateToggleVisual(active, true);
        }

        private void UpdateToggleVisual(Toggle toggle, bool isOn)
        {
            if (toggle == null) return;
            var bg = toggle.GetComponent<Image>();
            if (bg != null)
                bg.color = isOn ? new Color(0f, 1f, 1f, 1f) : new Color(0.08f, 0.12f, 0.18f, 1f);

            var label = toggle.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.color = isOn ? new Color(0.02f, 0.05f, 0.1f, 1f) : Color.white;
        }

        private void SetToggleDefault(Toggle toggle, bool value)
        {
            if (toggle == null) return;
            toggle.isOn = value;
            UpdateToggleVisual(toggle, value);
        }

        private void OnStartGameClicked()
        {
            if (toggleRounds1 != null && toggleRounds1.isOn) totalRounds = 1;
            else if (toggleRounds3 != null && toggleRounds3.isOn) totalRounds = 3;
            else if (toggleRounds5 != null && toggleRounds5.isOn) totalRounds = 5;
            // 10 rounds removed — max is 5
            else totalRounds = 1;

            currentRound = 1;
            totalErrors = 0;
            cumulativeTime = 0f;
            if (settingsPanel != null) settingsPanel.SetActive(false);
            StartNewGame();
        }

        private void Update()
        {
            if (isGameActive && isTimerStarted)
            {
                currentTime += Time.deltaTime;
                UpdateTimerDisplay();
            }
        }

        private void LoadPlayerData()
        {
            if (AuthenticationService.Instance != null)
            {
                currentPlayer = AuthenticationService.Instance.GetCurrentPlayerData();

                if (currentPlayer != null)
                {
                    bestTime = currentPlayer.bestTime;
                }
            }
        }

        private bool IsPracticeMode()
        {
            if (GameSessionManager.Instance == null || !GameSessionManager.Instance.HasActiveSession)
                return true;
            return GameSessionManager.Instance.CurrentContext?.Mode == GameMode.Practice;
        }

        private bool IsOnlineMode()
        {
            return OnlineResultManager.IsOnlineMatch();
        }

        private void SetupListeners()
        {
            for (int i = 0; i < gridButtons.Length; i++)
            {
                int index = i;
                gridButtons[i].onClick.AddListener(() => OnGridButtonClicked(index));
                shakeCoroutines[i] = null;
            }

            playAgainButton?.onClick.AddListener(OnPlayAgainClicked);
            // Disable auto-navigation from BackButton prefab to prevent double listener
            var autoNav = backButton?.GetComponent<DigitPark.UI.BackButton>();
            if (autoNav != null) autoNav.DisableAutoNavigation();
            backButton?.onClick.AddListener(OnBackButtonClicked);
            premiumBannerButton?.onClick.AddListener(OnPremiumBannerClicked);
            resultPlayAgainButton?.onClick.AddListener(OnPlayAgainClicked);
            resultExitButton?.onClick.AddListener(BackToMenu);

            PremiumManager.OnPremiumStatusChanged += UpdatePremiumBanner;
            UpdatePremiumBanner();
        }

        private void OnDestroy()
        {
            PremiumManager.OnPremiumStatusChanged -= UpdatePremiumBanner;
            StopAllCoroutines();

            // Clean up active shake coroutines
            for (int i = 0; i < shakeCoroutines.Length; i++)
            {
                if (shakeCoroutines[i] != null)
                {
                    StopCoroutine(shakeCoroutines[i]);
                    shakeCoroutines[i] = null;
                }
            }

            if (Instance == this) Instance = null;
        }

        private void UpdatePremiumBanner()
        {
            if (premiumBannerContainer == null) return;

            bool isPremium = PremiumManager.Instance != null && PremiumManager.Instance.IsPremium;
            premiumBannerContainer.SetActive(!isPremium);

            if (premiumBannerText != null && !isPremium)
            {
                premiumBannerText.text = $"{AutoLocalizer.Get("premium_feature_no_ads")} + {AutoLocalizer.Get("premium_feature_tournaments")}";
            }
        }

        private void OnPremiumBannerClicked()
        {
            Debug.Log("[Game] Premium banner clicked");
            Canvas canvas = UICanvasHelper.FindMainCanvas();
            if (canvas != null)
            {
                DigitPark.UI.Panels.PremiumPanelUI.CreateAndShow(canvas.transform);
            }
        }

        public void StartNewGame()
        {
            Debug.Log("[Game] Starting new game...");

            currentTargetNumber = 1;
            currentTime = 0f;
            isGameActive = false;
            isTimerStarted = false;
            currentCombo = 0;
            maxCombo = 0;

            GenerateRandomNumbers();
            ResetButtonVisuals();
            SetGridButtonsInteractable(false);
            HideNumbers();
            UpdateComboDisplay();

            if (winMessagePanel != null)
            {
                winMessagePanel.SetActive(false);
                if (winMessageCanvasGroup != null) winMessageCanvasGroup.alpha = 0;
            }

            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
                if (resultPanelCanvasGroup != null) resultPanelCanvasGroup.alpha = 0;
            }

            UpdateTimerDisplay();
            UpdateRoundUI();

            if (useCountdown)
            {
                Canvas canvas = gridButtons[0].GetComponentInParent<Canvas>();
                CountdownAnimator.Play(canvas, OnCountdownComplete);
            }
            else
            {
                OnCountdownComplete();
            }

            Debug.Log("[Game] New game started - Numbers hidden until GO!");
        }

        private void OnCountdownComplete()
        {
            Debug.Log("[Game] Countdown completed - Revealing numbers!");

            AssignNumbersToButtons();
            AnimateButtonsPopUp();
            StartCoroutine(ActivateGameAfterDelay(0.3f));
        }

        private IEnumerator ActivateGameAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            isGameActive = true;
            isTimerStarted = true;
            SetGridButtonsInteractable(true);

            Debug.Log("[Game] Game active! Timer running...");
        }

        private void SetGridButtonsInteractable(bool interactable)
        {
            foreach (var button in gridButtons)
            {
                if (button != null)
                {
                    button.interactable = interactable;
                }
            }
        }

        private void HideNumbers()
        {
            for (int i = 0; i < gridButtons.Length; i++)
            {
                if (gridButtons[i] != null)
                {
                    TextMeshProUGUI numberText = gridButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (numberText != null)
                    {
                        numberText.text = "";
                    }
                }
            }
        }

        private void AnimateButtonsPopUp()
        {
            foreach (var button in gridButtons)
            {
                if (button != null)
                {
                    var cell3D = button.GetComponent<Cell3DButton>();
                    if (cell3D != null)
                    {
                        cell3D.AnimateGameStart();
                    }
                }
            }
        }

        private void GenerateRandomNumbers()
        {
            List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            for (int i = numbers.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                int temp = numbers[i];
                numbers[i] = numbers[randomIndex];
                numbers[randomIndex] = temp;
            }

            for (int i = 0; i < 9; i++)
            {
                gridNumbers[i] = numbers[i];
            }

            Debug.Log($"[Game] Numbers generated: {string.Join(", ", gridNumbers)}");
        }

        private void AssignNumbersToButtons()
        {
            for (int i = 0; i < gridButtons.Length; i++)
            {
                TextMeshProUGUI numberText = gridButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (numberText != null)
                {
                    numberText.text = gridNumbers[i].ToString();
                }
            }
        }

        private void ResetButtonVisuals()
        {
            for (int i = 0; i < gridButtons.Length; i++)
            {
                if (shakeCoroutines[i] != null)
                {
                    StopCoroutine(shakeCoroutines[i]);
                    shakeCoroutines[i] = null;

                    RectTransform rt = gridButtons[i].GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.anchoredPosition = shakeOriginalPositions[i];
                    }
                }

                gridButtons[i].interactable = true;

                Image buttonImage = gridButtons[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = new Color(0.15f, 0.15f, 0.25f);
                }

                TextMeshProUGUI numberText = gridButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (numberText != null)
                {
                    numberText.color = Color.white;
                }

                // Reset Cell3DButton state
                var cell3D = gridButtons[i].GetComponent<Cell3DButton>();
                if (cell3D != null)
                {
                    cell3D.ResetToNormal();
                }
            }
        }

        private void OnGridButtonClicked(int buttonIndex)
        {
            if (!isGameActive) return;

            if (!isTimerStarted)
            {
                isTimerStarted = true;
                Debug.Log("[Game] Timer started");
            }

            int clickedNumber = gridNumbers[buttonIndex];
            Debug.Log($"[Game] Button {buttonIndex} clicked - Number: {clickedNumber} | Expected: {currentTargetNumber}");

            if (clickedNumber == currentTargetNumber)
            {
                OnCorrectNumberClicked(buttonIndex);
                currentTargetNumber++;

                if (currentTargetNumber > 9)
                {
                    OnGameComplete();
                }
            }
            else
            {
                OnWrongNumberClicked(buttonIndex);
            }
        }

        private void OnCorrectNumberClicked(int buttonIndex)
        {
            Debug.Log($"[Game] Correct! Number {gridNumbers[buttonIndex]}");

            // Increment combo
            currentCombo++;
            if (currentCombo > maxCombo) maxCombo = currentCombo;

            // Success vibration (stronger with combo)
            if (currentCombo >= 7)
                TriggerHaptic(HapticType.Heavy);
            else if (currentCombo >= 4)
                TriggerHaptic(HapticType.Medium);
            else
                TriggerHaptic(HapticType.Light);

            // Success particles
            PlayCorrectParticles(buttonIndex);

            // Animate button as completed
            var cell3D = gridButtons[buttonIndex].GetComponent<Cell3DButton>();
            if (cell3D != null)
            {
                cell3D.AnimateToCompleted(currentCombo);
            }
            else
            {
                // Fallback visual
                Image buttonImage = gridButtons[buttonIndex].GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = new Color(0.2f, 0.7f, 0.3f, 0.5f);
                }
            }

            gridButtons[buttonIndex].interactable = false;

            TextMeshProUGUI numberText = gridButtons[buttonIndex].GetComponentInChildren<TextMeshProUGUI>();
            if (numberText != null)
            {
                numberText.color = new Color(0.5f, 0.5f, 0.5f);
            }

            UpdateComboDisplay();
            comboVisualController?.OnComboChanged(currentCombo);
        }

        private void OnWrongNumberClicked(int buttonIndex)
        {
            Debug.Log($"[Game] Incorrect! Clicked {gridNumbers[buttonIndex]}, expected {currentTargetNumber}");

            // Reset combo
            comboVisualController?.OnComboReset();
            currentCombo = 0;
            totalErrors++;

            // +1 second penalty
            currentTime += 1f;
            UpdateTimerDisplay();

            UpdateComboDisplay();
            UpdateRoundUI();

            // Error vibration
            TriggerHaptic(HapticType.Error);

            // Error particles
            PlayErrorParticles(buttonIndex);

            // Error animation in Cell3DButton (red flash + shake + release)
            var cell3D = gridButtons[buttonIndex].GetComponent<Cell3DButton>();
            if (cell3D != null)
            {
                cell3D.ShakeError();
            }
            else
            {
                // Fallback: shake the RectTransform directly
                RectTransform rt = gridButtons[buttonIndex].GetComponent<RectTransform>();

                if (shakeCoroutines[buttonIndex] != null)
                {
                    StopCoroutine(shakeCoroutines[buttonIndex]);
                    rt.anchoredPosition = shakeOriginalPositions[buttonIndex];
                }

                shakeOriginalPositions[buttonIndex] = rt.anchoredPosition;
                shakeCoroutines[buttonIndex] = StartCoroutine(ShakeButton(buttonIndex));
            }

            // Show floating red "+1" above the button
            ShowPenaltyText(buttonIndex);
        }

        private IEnumerator ShakeButton(int buttonIndex)
        {
            RectTransform rt = gridButtons[buttonIndex].GetComponent<RectTransform>();
            Vector2 originalPos = shakeOriginalPositions[buttonIndex];

            float elapsed = 0f;
            float duration = 0.3f;
            float magnitude = 10f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                rt.anchoredPosition = originalPos + new Vector2(x, y);

                elapsed += Time.deltaTime;
                yield return null;
            }

            rt.anchoredPosition = originalPos;
            shakeCoroutines[buttonIndex] = null;
        }

        #region Particle Effects

        private void PlayCorrectParticles(int buttonIndex)
        {
            if (sparkleEffect == null) return;

            Vector2 pos = GetButtonPosition(buttonIndex);
            sparkleEffect.PlayMatchSparkles(pos, currentCombo);

            // Extra stars for high combo
            if (currentCombo >= 5)
            {
                sparkleEffect.PlayStarBurst(pos, currentCombo - 3);
            }
        }

        private void PlayErrorParticles(int buttonIndex)
        {
            if (sparkleEffect == null) return;

            Vector2 pos = GetButtonPosition(buttonIndex);
            sparkleEffect.PlayErrorSparkles(pos);
        }

        private Vector2 GetButtonPosition(int buttonIndex)
        {
            if (buttonIndex < gridButtons.Length && gridButtons[buttonIndex] != null)
            {
                RectTransform rt = gridButtons[buttonIndex].GetComponent<RectTransform>();
                if (rt != null)
                {
                    return rt.anchoredPosition;
                }
            }
            return Vector2.zero;
        }

        private void ShowPenaltyText(int buttonIndex)
        {
            if (buttonIndex >= gridButtons.Length || gridButtons[buttonIndex] == null) return;

            RectTransform buttonRT = gridButtons[buttonIndex].GetComponent<RectTransform>();
            if (buttonRT == null) return;

            // Create floating "+1" text directly on the canvas
            Canvas canvas = gridButtons[buttonIndex].GetComponentInParent<Canvas>();
            if (canvas == null) return;

            StartCoroutine(AnimatePenaltyText(buttonRT, canvas));
        }

        private IEnumerator AnimatePenaltyText(RectTransform buttonRT, Canvas canvas)
        {
            // Create text object
            GameObject penaltyObj = new GameObject("PenaltyText");
            penaltyObj.transform.SetParent(canvas.transform, false);

            RectTransform rt = penaltyObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(120, 60);

            TextMeshProUGUI tmp = penaltyObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "+1";
            tmp.fontSize = FontSizes.Subtitle;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinBody;
            tmp.fontSizeMax = tmp.fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color(1f, 0.3f, 0.3f, 1f); // Red
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            // Black outline for readability
            Outline outline = penaltyObj.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
            outline.effectDistance = new Vector2(2, -2);

            // Position above the button
            Vector3 buttonWorldPos = buttonRT.position;
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, buttonWorldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform, screenPos, canvas.worldCamera, out Vector2 localPos);
            rt.anchoredPosition = localPos + new Vector2(0, 30f);

            // Animation: punch scale + float up + fade out
            Vector2 startPos = rt.anchoredPosition;
            float duration = 1.2f;
            float elapsed = 0f;

            // Initial punch scale
            float punchDur = 0.12f;
            float punchElapsed = 0f;
            while (punchElapsed < punchDur)
            {
                punchElapsed += Time.deltaTime;
                float t = punchElapsed / punchDur;
                float scale = t < 0.5f
                    ? Mathf.Lerp(0.3f, 1.4f, t * 2f)
                    : Mathf.Lerp(1.4f, 1f, (t - 0.5f) * 2f);
                rt.localScale = Vector3.one * scale;
                yield return null;
            }
            rt.localScale = Vector3.one;

            // Float up and fade out
            Color startColor = tmp.color;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Upward movement with deceleration
                float yOffset = 80f * t * (1f - t * 0.3f);
                rt.anchoredPosition = startPos + new Vector2(0, yOffset);

                // Fade out in the second half
                if (t > 0.5f)
                {
                    float fadeT = (t - 0.5f) / 0.5f;
                    tmp.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(1f, 0f, fadeT));
                }

                yield return null;
            }

            Destroy(penaltyObj);
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

            if (currentCombo >= 3)
            {
                comboText.gameObject.SetActive(true);
                comboText.text = $"x{currentCombo}";

                // Color based on combo
                if (currentCombo >= 8)
                    comboText.color = new Color(1f, 0.85f, 0.2f); // Gold
                else if (currentCombo >= 6)
                    comboText.color = new Color(1f, 0.5f, 0.2f); // Orange
                else if (currentCombo >= 4)
                    comboText.color = new Color(0.3f, 1f, 0.5f); // Green
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

        private void OnGameComplete()
        {
            Debug.Log($"[Game] ROUND COMPLETED! Time: {currentTime:F3}s | Max Combo: {maxCombo}");

            isGameActive = false;
            isTimerStarted = false;
            cumulativeTime += currentTime;

            currentRound++;
            UpdateRoundUI();

            if (currentRound > totalRounds)
            {
                // Final victory
                FinalEndGame();
            }
            else
            {
                // Next round
                StartCoroutine(NextRoundSequence());
            }
        }

        private IEnumerator NextRoundSequence()
        {
            yield return new WaitForSeconds(0.8f);

            currentTargetNumber = 1;
            currentTime = 0f;
            currentCombo = 0;

            GenerateRandomNumbers();
            ResetButtonVisuals();
            SetGridButtonsInteractable(false);
            HideNumbers();
            UpdateComboDisplay();
            UpdateTimerDisplay();
            UpdateRoundUI();

            // Start round directly without countdown
            AssignNumbersToButtons();
            AnimateButtonsPopUp();
            StartCoroutine(ActivateGameAfterDelay(0.3f));
        }

        private void FinalEndGame()
        {
            bool isNewRecord = cumulativeTime < bestTime;

            if (isNewRecord)
            {
                bestTime = cumulativeTime;
                Debug.Log($"[Game] NEW RECORD! {bestTime:F3}s");
                SaveBestTime();
            }

            // Use cumulative time for final score
            float finalTime = cumulativeTime;

            SaveScoreToDatabase();

            // Register result in GameSessionManager
            var result = new MinigameResult
            {
                GameType = GameType.DigitRush,
                TotalTime = finalTime,
                Errors = totalErrors,
                PenaltyTime = totalErrors * 1f, // Each error adds 1 second penalty
                Completed = true,
                CompletedAt = System.DateTime.UtcNow
            };

            if (GameSessionManager.Instance != null && GameSessionManager.Instance.HasActiveSession)
            {
                GameSessionManager.Instance.RegisterGameResult(result);
            }

            // Victory vibration
            TriggerHaptic(HapticType.Heavy);

            // Victory sequence with panel based on mode
            StartCoroutine(PlayVictorySequence(result, isNewRecord));
        }

        private IEnumerator PlayVictorySequence(MinigameResult result, bool isNewRecord)
        {
            yield return new WaitForSeconds(0.1f);
            TriggerHaptic(HapticType.Heavy);

            // Celebration "wave" animation on the buttons
            float waveDelay = 0.08f;
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    int index = row * 3 + col;
                    if (index < gridButtons.Length && gridButtons[index] != null)
                    {
                        var cell3D = gridButtons[index].GetComponent<Cell3DButton>();
                        if (cell3D != null)
                        {
                            float delay = (row + col) * waveDelay;
                            cell3D.PlayVictoryCelebration(delay);
                        }
                    }
                }
            }

            // Confetti
            if (sparkleEffect != null)
            {
                sparkleEffect.PlayVictoryConfetti();

                yield return new WaitForSeconds(0.3f);
                sparkleEffect.PlayConfettiExplosion(Vector2.zero);
            }

            yield return new WaitForSeconds(0.5f);

            // Show panel based on game mode
            var ctx = GameSessionManager.Instance?.CurrentContext;

            // 1. Online free 1v1 (no sprint)
            if (IsOnlineMode() && ctx?.Mode != GameMode.CognitiveSprint)
            {
                HandleOnlineResult(result);
            }
            // 2. Online sprint final -> wait for opponent, then SprintSummary
            else if (IsOnlineMode() && ctx?.Mode == GameMode.CognitiveSprint)
            {
                if (!ctx.HasMoreGames)
                    OnlineResultManager.Instance.SubmitSprintAndWaitForResult(ctx);
                else
                    HandleOnlineResult(result);
            }
            // 3. Tournament
            else if (ctx?.Mode == GameMode.Tournament)
            {
                HandleTournamentResult(result, ctx);
            }
            // 4. CashBattle single game
            else if (ctx != null && ctx.EntryFee > 0 && ctx.Mode == GameMode.SingleGame)
            {
                HandleCashBattleResult(result, ctx);
            }
            // 5. CognitiveSprint final (practice or cash)
            else if (ctx?.Mode == GameMode.CognitiveSprint && !ctx.HasMoreGames)
            {
                ResultPanelManager.Instance.ShowSprintSummary(ctx);
            }
            // 6. CognitiveSprint mid-game -> normal transition panel
            else if (ctx?.Mode == GameMode.CognitiveSprint && ctx.HasMoreGames)
            {
                ShowPracticeResult(isNewRecord);
            }
            // 7. Practice single
            else
            {
                ShowPracticeResult(isNewRecord);
            }
        }

        private string GetSuccessMessage(float time)
        {
            string[] keys;

            if (time < 1f)
                keys = level1Keys;
            else if (time < 2f)
                keys = level2Keys;
            else if (time < 3f)
                keys = level3Keys;
            else if (time < 4f)
                keys = level4Keys;
            else if (time < 5f)
                keys = level5Keys;
            else
                keys = level6Keys;

            string selectedKey = keys[Random.Range(0, keys.Length)];
            Debug.Log($"[Game] GetSuccessMessage - Time: {time:F3}s, Selected key: {selectedKey}");

            string translatedText = AutoLocalizer.Get(selectedKey);
            Debug.Log($"[Game] Translated text: '{translatedText}'");
            return translatedText;
        }

        private void ShowPracticeResult(bool isNewRecord)
        {
            // Use resultPanel (new) if available, otherwise fallback to winMessagePanel (original)
            if (resultPanel != null)
            {
                if (resultTitleText != null)
                    resultTitleText.text = AutoLocalizer.Get("digitrush_result_title");

                if (resultTimeText != null)
                    resultTimeText.text = AutoLocalizer.Get("result_panel_time", $"{cumulativeTime:F3}");

                if (resultMessageText != null)
                {
                    string message = GetSuccessMessage(cumulativeTime);
                    if (isNewRecord)
                        message += $"\n{AutoLocalizer.Get("result_panel_new_record")}";
                    resultMessageText.text = message;
                }

                // Localize button texts
                if (resultPlayAgainButton != null)
                {
                    var btnText = resultPlayAgainButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (btnText != null) btnText.text = AutoLocalizer.Get("digitrush_play_again");
                }
                if (resultExitButton != null)
                {
                    var btnText = resultExitButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (btnText != null) btnText.text = AutoLocalizer.Get("digitrush_exit");
                }

                resultPanel.SetActive(true);
                StartCoroutine(FadeInResultPanel());
            }
            else if (winMessagePanel != null)
            {
                // Fallback: usar winMessagePanel original (stays visible with buttons)
                if (successText != null)
                {
                    string timeFormatted = $"{cumulativeTime:F3}{AutoLocalizer.Get("seconds_abbr")}";
                    string message = GetSuccessMessage(cumulativeTime);
                    if (isNewRecord)
                        message += $"\n{AutoLocalizer.Get("result_panel_new_record")}";
                    successText.text = $"{timeFormatted}\n{message}";
                }

                winMessagePanel.SetActive(true);
                if (winMessageCanvasGroup != null)
                    StartCoroutine(FadeInWinMessage());
            }
        }

        private IEnumerator FadeInWinMessage()
        {
            winMessageCanvasGroup.alpha = 0f;
            float duration = 0.5f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                winMessageCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }
            winMessageCanvasGroup.alpha = 1f;
        }

        private IEnumerator FadeInResultPanel()
        {
            if (resultPanelCanvasGroup == null) yield break;

            resultPanelCanvasGroup.alpha = 0f;
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                resultPanelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }

            resultPanelCanvasGroup.alpha = 1f;
        }

        private void ShowRealMoneyResult(MinigameResult result)
        {
            var context = GameSessionManager.Instance?.CurrentContext;
            decimal entryFee = context?.EntryFee ?? 0;

            // TODO: Get real opponent result from server
            bool playerWon = result.Completed;

            if (playerWon && winPanelRealMoney != null)
            {
                winPanelRealMoney.ShowRealMoneyResult(result, null, entryFee, true, context?.OpponentName ?? AutoLocalizer.Get("default_opponent"));
            }
            else if (!playerWon && losePanelRealMoney != null)
            {
                losePanelRealMoney.ShowRealMoneyResult(result, null, entryFee, false, context?.OpponentName ?? AutoLocalizer.Get("default_opponent"));
            }
        }

        private void HandleOnlineResult(MinigameResult result)
        {
            string matchId = OnlineResultManager.GetCurrentMatchId();
            string playerName = PlayerPrefs.GetString("PlayerName", "Player");

            Debug.Log($"[DigitRush] Online match finished. MatchId: {matchId}, Time: {result.TotalTime:F3}s");

            OnlineResultManager.Instance.SubmitAndWaitForResult(
                matchId,
                result,
                playerName,
                (playerWon) =>
                {
                    Debug.Log($"[DigitRush] Online result: {(playerWon ? "VICTORY" : "DEFEAT")}");
                }
            );
        }

        private void HandleTournamentResult(MinigameResult result, GameContext ctx)
        {
            var tournamentService = ServiceLocator.Tournament;

            int position = 1;
            decimal prize = ctx.EntryFee > 0 ? ctx.EntryFee * 5m : 0;
            int attemptsUsed = PlayerPrefs.GetInt($"tournament_{ctx.TournamentId}_attempts", 1);
            int maxAttempts = 3;
            float bestTime = PlayerPrefs.GetFloat($"tournament_{ctx.TournamentId}_best", result.FinalScore);

            if (tournamentService?.ActiveTournament != null)
            {
                var tournament = tournamentService.ActiveTournament;
                position = tournament.MyPosition ?? 1;
                prize = tournament.PrizePool;

                tournamentService.SubmitTournamentScore(ctx.TournamentId, (int)(result.FinalScore * 100f));
            }

            if (result.FinalScore < bestTime)
            {
                bestTime = result.FinalScore;
                PlayerPrefs.SetFloat($"tournament_{ctx.TournamentId}_best", bestTime);
            }
            PlayerPrefs.SetInt($"tournament_{ctx.TournamentId}_attempts", attemptsUsed + 1);
            PlayerPrefs.Save();

            ResultPanelManager.Instance.ShowTournamentResult(
                result, position, attemptsUsed, maxAttempts, bestTime, prize);
        }

        private void HandleCashBattleResult(MinigameResult result, GameContext ctx)
        {
            string matchId = ctx.MatchId ?? OnlineResultManager.GetCurrentMatchId();
            string opponentName = ctx.OpponentName ?? OnlineResultManager.GetCurrentOpponentName();

            if (MatchmakingService.Instance != null && !string.IsNullOrEmpty(matchId))
            {
                MatchmakingService.Instance.SubmitMatchResult(
                    matchId, result.FinalScore, result.TotalTime, result.Errors);

                MatchmakingService.Instance.ListenForOpponentResult(matchId,
                    (opponentScore, opponentTime) =>
                    {
                        var opponentResult = new MinigameResult
                        {
                            TotalTime = opponentTime,
                            PenaltyTime = opponentScore - opponentTime,
                            Errors = 0,
                            Completed = true
                        };

                        bool playerWon = result.FinalScore < opponentScore;
                        ResultPanelManager.Instance.ShowCashBattleResult(
                            result, opponentResult, ctx.EntryFee, playerWon, opponentName);
                    });
            }
            else
            {
                bool playerWon = result.Completed;
                ResultPanelManager.Instance.ShowCashBattleResult(
                    result, null, ctx.EntryFee, playerWon, opponentName);
            }
        }

        #endregion

        #region Save Data

        private async void SaveBestTime()
        {
            try
            {
                if (currentPlayer == null || DatabaseService.Instance == null) return;

                currentPlayer.bestTime = bestTime;
                await DatabaseService.Instance.SavePlayerData(currentPlayer);

                Debug.Log($"[Game] Best time saved: {bestTime:F3}s");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DigitRushController] {ex.Message}");
            }
        }

        private void SaveScoreToDatabase()
        {
            // Score saving is now handled by GameSessionManager.RegisterGameResult()
            // which correctly passes gameId. Only log here for debugging.
            Debug.Log($"[Game] Score {cumulativeTime:F3}s will be saved by GameSessionManager");
        }

        #endregion

        #region UI Updates

        private void UpdateTimerDisplay()
        {
            if (timerText != null)
            {
                timerText.text = $"{currentTime:F3}{AutoLocalizer.Get("seconds_abbr")}";
            }
        }

        private void UpdateRoundUI()
        {
            if (roundText != null)
                roundText.text = $"{Mathf.Min(currentRound, totalRounds)}/{totalRounds}";

            if (errorsText != null)
                errorsText.text = totalErrors.ToString();

            if (roundIndicatorText != null)
                roundIndicatorText.text = $"{Mathf.Min(currentRound, totalRounds)}/{totalRounds}";

            if (progressFill != null)
            {
                progressFill.transform.parent.parent.gameObject.SetActive(totalRounds > 1);
                float progress = (float)(currentRound - 1) / totalRounds;
                progressFill.anchorMax = new Vector2(progress, 1f);
            }
        }

        private void OnPlayAgainClicked()
        {
            if (IsPracticeMode() && settingsPanel != null)
            {
                if (resultPanel != null) resultPanel.SetActive(false);
                if (winMessagePanel != null) winMessagePanel.SetActive(false);
                settingsPanel.SetActive(true);
            }
            else
            {
                StartNewGame();
            }
        }

        private void OnBackButtonClicked()
        {
            BackToMenu();
        }

        public void BackToMenu()
        {
            if (GameSessionManager.Instance != null && GameSessionManager.Instance.HasActiveSession)
            {
                Debug.Log("[Game] Returning to game selector");
                SceneManager.LoadScene("GameSelector");
            }
            else
            {
                Debug.Log("[Game] Returning to main menu");
                SceneManager.LoadScene("MainMenu");
            }
        }

        #endregion
    }
}
