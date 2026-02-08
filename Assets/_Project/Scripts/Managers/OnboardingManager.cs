using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using DigitPark.Monetization;
using DigitPark.Localization;
using DG.Tweening;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager para la escena de onboarding/tutorial.
    /// Guía a nuevos usuarios a través de la introducción de la aplicación.
    /// </summary>
    public class OnboardingManager : MonoBehaviour
    {
        [Header("UI - Main")]
        [SerializeField] private Button skipButton;
        [SerializeField] private TextMeshProUGUI skipButtonText;
        [SerializeField] private Button backButton;

        [Header("UI - Step Display")]
        [SerializeField] private Image stepImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private GameObject characterContainer;
        [SerializeField] private Animator characterAnimator;

        [Header("UI - Navigation")]
        [SerializeField] private Button nextButton;
        [SerializeField] private Button prevButton;
        [SerializeField] private TextMeshProUGUI nextButtonText;
        [SerializeField] private Transform dotsContainer;
        [SerializeField] private GameObject dotPrefab;

        [Header("UI - Progress")]
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI stepCounterText;

        [Header("UI - Interactive Elements")]
        [SerializeField] private GameObject highlightOverlay;
        [SerializeField] private RectTransform highlightTarget;
        [SerializeField] private TextMeshProUGUI highlightTooltipText;
        [SerializeField] private GameObject tapToContinuePrompt;

        [Header("UI - Name Input (Welcome Step)")]
        [SerializeField] private GameObject nameInputPanel;
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private Button confirmNameButton;
        [SerializeField] private TextMeshProUGUI nameErrorText;

        [Header("UI - Avatar Selection")]
        [SerializeField] private GameObject avatarSelectionPanel;
        [SerializeField] private Transform avatarContainer;
        [SerializeField] private GameObject avatarOptionPrefab;

        [Header("UI - Tutorial Completion")]
        [SerializeField] private GameObject completionPanel;
        [SerializeField] private TextMeshProUGUI completionTitleText;
        [SerializeField] private TextMeshProUGUI completionMessageText;
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private Button startPlayingButton;

        [Header("Onboarding Steps")]
        [SerializeField] private List<OnboardingStep> steps = new List<OnboardingStep>();

        [Header("Step Images")]
        [SerializeField] private Sprite welcomeImage;
        [SerializeField] private Sprite gamesImage;
        [SerializeField] private Sprite cashBattleImage;
        [SerializeField] private Sprite tournamentsImage;
        [SerializeField] private Sprite rewardsImage;
        [SerializeField] private Sprite socialImage;

        [Header("UI - Sections (for animations)")]
        [SerializeField] private RectTransform progressBarTransform;
        [SerializeField] private RectTransform topBarTransform;
        [SerializeField] private RectTransform dotsTransform;
        [SerializeField] private RectTransform navigationTransform;

        [Header("Avatar Options")]
        [SerializeField] private List<AvatarOption> avatarOptions = new List<AvatarOption>();

        [Header("Configuration")]
        [SerializeField] private int completionRewardCoins = 500;
        [SerializeField] private int completionRewardGems = 50;
        [SerializeField] private float autoAdvanceDelay = 0f;
        [SerializeField] private bool allowSkip = true;

        // State
        private int currentStepIndex = 0;
        private List<GameObject> spawnedDots = new List<GameObject>();
        private List<GameObject> spawnedAvatars = new List<GameObject>();
        private string selectedAvatarId = "";
        private string playerName = "";
        private bool isTransitioning = false;

        private void Start()
        {
            InitializeSteps();
            SetupUI();
            SetupListeners();
            ShowStep(0);
            AnimateEntrance();
        }

        private void InitializeSteps()
        {
            // Default steps if not set in inspector
            if (steps.Count == 0)
            {
                steps = new List<OnboardingStep>
                {
                    new OnboardingStep
                    {
                        id = "welcome",
                        title = L("onboarding_welcome_title"),
                        description = L("onboarding_welcome_desc"),
                        stepType = OnboardingStepType.Info,
                        requiresInteraction = false
                    },
                    new OnboardingStep
                    {
                        id = "name",
                        title = L("onboarding_name_title"),
                        description = L("onboarding_name_desc"),
                        stepType = OnboardingStepType.NameInput,
                        requiresInteraction = true
                    },
                    new OnboardingStep
                    {
                        id = "avatar",
                        title = L("onboarding_avatar_title"),
                        description = L("onboarding_avatar_desc"),
                        stepType = OnboardingStepType.AvatarSelection,
                        requiresInteraction = true
                    },
                    new OnboardingStep
                    {
                        id = "games",
                        title = L("onboarding_games_title"),
                        description = L("onboarding_games_desc"),
                        stepType = OnboardingStepType.Info,
                        requiresInteraction = false
                    },
                    new OnboardingStep
                    {
                        id = "cashbattle",
                        title = L("onboarding_cashbattle_title"),
                        description = L("onboarding_cashbattle_desc"),
                        stepType = OnboardingStepType.Info,
                        requiresInteraction = false
                    },
                    new OnboardingStep
                    {
                        id = "tournaments",
                        title = L("tournament_button"),
                        description = L("tournaments_description"),
                        stepType = OnboardingStepType.Info,
                        requiresInteraction = false
                    },
                    new OnboardingStep
                    {
                        id = "rewards",
                        title = L("dr_title"),
                        description = L("onboarding_rewards_desc"),
                        stepType = OnboardingStepType.Info,
                        requiresInteraction = false
                    },
                    new OnboardingStep
                    {
                        id = "complete",
                        title = L("onboarding_ready_title"),
                        description = L("onboarding_ready_desc"),
                        stepType = OnboardingStepType.Completion,
                        requiresInteraction = true
                    }
                };
            }

            // Initialize avatar options if not set
            if (avatarOptions.Count == 0)
            {
                avatarOptions = new List<AvatarOption>
                {
                    new AvatarOption { id = "avatar_01", name = AutoLocalizer.Get("avatar_explorer") },
                    new AvatarOption { id = "avatar_02", name = AutoLocalizer.Get("avatar_warrior") },
                    new AvatarOption { id = "avatar_03", name = AutoLocalizer.Get("avatar_mage") },
                    new AvatarOption { id = "avatar_04", name = AutoLocalizer.Get("avatar_ninja") },
                    new AvatarOption { id = "avatar_05", name = AutoLocalizer.Get("avatar_robot") },
                    new AvatarOption { id = "avatar_06", name = AutoLocalizer.Get("avatar_alien") },
                };
            }
        }

        private void SetupUI()
        {
            // Hide all special panels
            if (nameInputPanel) nameInputPanel.SetActive(false);
            if (avatarSelectionPanel) avatarSelectionPanel.SetActive(false);
            if (completionPanel) completionPanel.SetActive(false);
            if (highlightOverlay) highlightOverlay.SetActive(false);
            if (tapToContinuePrompt) tapToContinuePrompt.SetActive(false);

            if (skipButton) skipButton.gameObject.SetActive(allowSkip);
            if (backButton) backButton.gameObject.SetActive(false);
            if (prevButton) prevButton.gameObject.SetActive(false);

            // Create navigation dots
            CreateNavigationDots();
            UpdateProgressBar();
        }

        private void SetupListeners()
        {
            if (skipButton) skipButton.onClick.AddListener(OnSkipClicked);
            if (backButton) backButton.onClick.AddListener(OnBackClicked);
            if (nextButton) nextButton.onClick.AddListener(OnNextClicked);
            if (prevButton) prevButton.onClick.AddListener(OnPrevClicked);

            if (confirmNameButton) confirmNameButton.onClick.AddListener(OnConfirmName);
            if (nameInput) nameInput.onEndEdit.AddListener(_ => OnConfirmName());

            if (startPlayingButton) startPlayingButton.onClick.AddListener(OnStartPlaying);
        }

        private void CreateNavigationDots()
        {
            if (dotsContainer == null) return;

            // Clear existing
            foreach (var dot in spawnedDots)
            {
                if (dot) Destroy(dot);
            }
            spawnedDots.Clear();

            // Create new dots
            for (int i = 0; i < steps.Count; i++)
            {
                GameObject dot;
                if (dotPrefab != null)
                {
                    dot = Instantiate(dotPrefab, dotsContainer);
                }
                else
                {
                    dot = new GameObject($"Dot_{i}");
                    dot.transform.SetParent(dotsContainer, false);
                    var rt = dot.AddComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(12, 12);
                    var img = dot.AddComponent<Image>();
                    img.color = new Color(0.3f, 0.3f, 0.3f);
                }

                spawnedDots.Add(dot);
            }
        }

        private void UpdateNavigationDots()
        {
            for (int i = 0; i < spawnedDots.Count; i++)
            {
                var dot = spawnedDots[i];
                if (dot == null) continue;

                var image = dot.GetComponent<Image>();
                if (image)
                {
                    bool isActive = i == currentStepIndex;
                    bool isPast = i < currentStepIndex;

                    if (isActive)
                        image.color = new Color(0f, 0.83f, 1f);
                    else if (isPast)
                        image.color = new Color(0f, 0.5f, 0.65f);
                    else
                        image.color = new Color(0.3f, 0.3f, 0.3f);

                    // Scale active dot
                    dot.transform.localScale = isActive ? Vector3.one * 1.3f : Vector3.one;
                }
            }
        }

        private void UpdateProgressBar()
        {
            if (progressBar)
            {
                progressBar.maxValue = steps.Count - 1;
                progressBar.value = currentStepIndex;
            }

            if (stepCounterText)
            {
                stepCounterText.text = $"{currentStepIndex + 1}/{steps.Count}";
            }
        }

        private void ShowStep(int index)
        {
            if (index < 0 || index >= steps.Count) return;

            currentStepIndex = index;
            var step = steps[index];

            // Hide all special panels first
            if (nameInputPanel) nameInputPanel.SetActive(false);
            if (avatarSelectionPanel) avatarSelectionPanel.SetActive(false);
            if (completionPanel) completionPanel.SetActive(false);

            // Update text
            if (titleText) titleText.text = step.title;
            if (descriptionText) descriptionText.text = step.description;

            // Update image
            if (stepImage)
            {
                stepImage.sprite = GetStepImage(step.id);
                stepImage.gameObject.SetActive(stepImage.sprite != null);
            }

            // Update navigation
            if (prevButton) prevButton.gameObject.SetActive(index > 0);

            UpdateNextButton(step);
            UpdateNavigationDots();
            UpdateProgressBar();

            // Show step-specific UI
            switch (step.stepType)
            {
                case OnboardingStepType.NameInput:
                    ShowNameInput();
                    break;

                case OnboardingStepType.AvatarSelection:
                    ShowAvatarSelection();
                    break;

                case OnboardingStepType.Completion:
                    ShowCompletion();
                    break;

                case OnboardingStepType.Interactive:
                    ShowInteractiveHighlight(step);
                    break;
            }

            // Character animation
            if (characterAnimator)
            {
                characterAnimator.SetTrigger("Talk");
            }

            // Auto advance for info steps
            if (!step.requiresInteraction && autoAdvanceDelay > 0)
            {
                CancelInvoke(nameof(AutoAdvance));
                Invoke(nameof(AutoAdvance), autoAdvanceDelay);
            }

            Debug.Log($"[Onboarding] Showing step {index + 1}: {step.id}");
        }

        private Sprite GetStepImage(string stepId)
        {
            return stepId switch
            {
                "welcome" => welcomeImage,
                "games" => gamesImage,
                "cashbattle" => cashBattleImage,
                "tournaments" => tournamentsImage,
                "rewards" => rewardsImage,
                "social" => socialImage,
                _ => null
            };
        }

        private void UpdateNextButton(OnboardingStep step)
        {
            if (nextButton == null) return;

            bool canAdvance = !step.requiresInteraction ||
                             step.stepType == OnboardingStepType.Info;

            nextButton.interactable = canAdvance;

            if (nextButtonText)
            {
                if (currentStepIndex == steps.Count - 1)
                {
                    nextButtonText.text = AutoLocalizer.Get("onboarding_begin");
                }
                else if (step.stepType == OnboardingStepType.NameInput)
                {
                    nextButtonText.text = AutoLocalizer.Get("onboarding_confirm");
                }
                else if (step.stepType == OnboardingStepType.AvatarSelection)
                {
                    nextButtonText.text = AutoLocalizer.Get("onboarding_select");
                }
                else
                {
                    nextButtonText.text = AutoLocalizer.Get("onboarding_next");
                }
            }
        }

        private void ShowNameInput()
        {
            if (nameInputPanel)
            {
                nameInputPanel.SetActive(true);
                nameInputPanel.transform.localScale = Vector3.one * 0.85f;
                nameInputPanel.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);

                if (nameInput)
                {
                    nameInput.text = playerName;
                    nameInput.Select();
                }

                if (nameErrorText) nameErrorText.gameObject.SetActive(false);
            }

            if (nextButton) nextButton.interactable = !string.IsNullOrWhiteSpace(playerName);
        }

        private void OnConfirmName()
        {
            if (nameInput == null) return;

            string name = nameInput.text.Trim();

            // Validate
            if (string.IsNullOrWhiteSpace(name))
            {
                ShowNameError("Por favor ingresa un nombre");
                return;
            }

            if (name.Length < 3)
            {
                ShowNameError("El nombre debe tener al menos 3 caracteres");
                return;
            }

            if (name.Length > 20)
            {
                ShowNameError("El nombre no puede tener más de 20 caracteres");
                return;
            }

            playerName = name;
            PlayerPrefs.SetString("PlayerName", playerName);
            PlayerPrefs.Save();

            if (nextButton) nextButton.interactable = true;
            if (nameErrorText) nameErrorText.gameObject.SetActive(false);

            Debug.Log($"[Onboarding] Name set: {playerName}");
        }

        private void ShowNameError(string message)
        {
            if (nameErrorText)
            {
                nameErrorText.text = message;
                nameErrorText.gameObject.SetActive(true);
            }
        }

        private void ShowAvatarSelection()
        {
            if (avatarSelectionPanel)
            {
                avatarSelectionPanel.SetActive(true);
                avatarSelectionPanel.transform.localScale = Vector3.one * 0.85f;
                avatarSelectionPanel.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
                PopulateAvatars();
            }

            if (nextButton) nextButton.interactable = !string.IsNullOrEmpty(selectedAvatarId);
        }

        private void PopulateAvatars()
        {
            if (avatarContainer == null) return;

            // Clear existing
            foreach (var avatar in spawnedAvatars)
            {
                if (avatar) Destroy(avatar);
            }
            spawnedAvatars.Clear();

            // Create avatar options
            foreach (var option in avatarOptions)
            {
                CreateAvatarOption(option);
            }
        }

        private void CreateAvatarOption(AvatarOption option)
        {
            GameObject item;

            if (avatarOptionPrefab != null)
            {
                item = Instantiate(avatarOptionPrefab, avatarContainer);
            }
            else
            {
                item = CreateAvatarOptionFallback(option);
            }

            spawnedAvatars.Add(item);

            // Setup click
            var button = item.GetComponent<Button>() ?? item.AddComponent<Button>();
            var opt = option;
            button.onClick.AddListener(() => OnAvatarSelected(opt.id, item));
        }

        private GameObject CreateAvatarOptionFallback(AvatarOption option)
        {
            var item = new GameObject($"Avatar_{option.id}");
            item.transform.SetParent(avatarContainer, false);

            var rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(100, 130);

            var image = item.AddComponent<Image>();
            image.color = selectedAvatarId == option.id
                ? new Color(0f, 0.5f, 0.8f, 0.9f)
                : new Color(0.2f, 0.2f, 0.25f, 0.9f);

            // Avatar placeholder
            var avatarObj = new GameObject("AvatarImage");
            avatarObj.transform.SetParent(item.transform, false);
            var avatarRT = avatarObj.AddComponent<RectTransform>();
            avatarRT.anchorMin = new Vector2(0.1f, 0.3f);
            avatarRT.anchorMax = new Vector2(0.9f, 0.95f);
            avatarRT.offsetMin = Vector2.zero;
            avatarRT.offsetMax = Vector2.zero;

            var avatarImg = avatarObj.AddComponent<Image>();
            avatarImg.color = new Color(0.4f, 0.4f, 0.4f);
            if (option.sprite != null) avatarImg.sprite = option.sprite;

            // Name
            var nameObj = new GameObject("Name");
            nameObj.transform.SetParent(item.transform, false);
            var nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0);
            nameRT.anchorMax = new Vector2(1, 0.3f);
            nameRT.offsetMin = new Vector2(5, 5);
            nameRT.offsetMax = new Vector2(-5, 0);

            var nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = option.name;
            nameText.fontSize = 12;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = Color.white;

            return item;
        }

        private void OnAvatarSelected(string avatarId, GameObject selectedItem)
        {
            selectedAvatarId = avatarId;

            // Update visual selection
            foreach (var avatar in spawnedAvatars)
            {
                if (avatar == null) continue;
                var image = avatar.GetComponent<Image>();
                if (image)
                {
                    image.color = avatar == selectedItem
                        ? new Color(0f, 0.5f, 0.8f, 0.9f)
                        : new Color(0.2f, 0.2f, 0.25f, 0.9f);
                }
            }

            PlayerPrefs.SetString("PlayerAvatar", selectedAvatarId);
            PlayerPrefs.Save();

            if (nextButton) nextButton.interactable = true;

            Debug.Log($"[Onboarding] Avatar selected: {avatarId}");
        }

        private void ShowInteractiveHighlight(OnboardingStep step)
        {
            if (highlightOverlay)
            {
                highlightOverlay.SetActive(true);

                if (highlightTooltipText)
                {
                    highlightTooltipText.text = step.highlightTooltip;
                }
            }

            if (tapToContinuePrompt)
            {
                tapToContinuePrompt.SetActive(true);
            }
        }

        private void ShowCompletion()
        {
            if (completionPanel)
            {
                completionPanel.SetActive(true);
                completionPanel.transform.localScale = Vector3.one * 0.85f;
                completionPanel.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);

                if (completionTitleText)
                {
                    completionTitleText.text = L("onboarding_well_done", playerName);
                }

                if (completionMessageText)
                {
                    completionMessageText.text = L("onboarding_completion_msg");
                }

                if (rewardText)
                {
                    rewardText.text = $"+{completionRewardCoins} {L("reward_coins")} | +{completionRewardGems} {L("reward_gems")}";
                }
            }

            if (nextButton) nextButton.gameObject.SetActive(false);
            if (skipButton) skipButton.gameObject.SetActive(false);
        }

        private void AutoAdvance()
        {
            if (currentStepIndex < steps.Count - 1)
            {
                OnNextClicked();
            }
        }

        private void OnNextClicked()
        {
            if (isTransitioning) return;

            var currentStep = steps[currentStepIndex];

            // Validate required input
            if (currentStep.stepType == OnboardingStepType.NameInput)
            {
                if (string.IsNullOrWhiteSpace(playerName))
                {
                    ShowNameError("Por favor ingresa un nombre");
                    return;
                }
            }
            else if (currentStep.stepType == OnboardingStepType.AvatarSelection)
            {
                if (string.IsNullOrEmpty(selectedAvatarId))
                {
                    return;
                }
            }

            if (currentStepIndex < steps.Count - 1)
            {
                TransitionToStep(currentStepIndex + 1);
            }
            else
            {
                CompleteOnboarding();
            }
        }

        private void OnPrevClicked()
        {
            if (isTransitioning) return;

            if (currentStepIndex > 0)
            {
                TransitionToStep(currentStepIndex - 1);
            }
        }

        private void TransitionToStep(int newIndex)
        {
            isTransitioning = true;

            const float fadeOut = 0.12f;
            const float fadeIn = 0.2f;

            CanvasGroup imgCG = null, titleCG = null, descCG = null;
            if (stepImage != null)
                imgCG = stepImage.GetComponent<CanvasGroup>() ?? stepImage.gameObject.AddComponent<CanvasGroup>();
            if (titleText != null)
                titleCG = titleText.GetComponent<CanvasGroup>() ?? titleText.gameObject.AddComponent<CanvasGroup>();
            if (descriptionText != null)
                descCG = descriptionText.GetComponent<CanvasGroup>() ?? descriptionText.gameObject.AddComponent<CanvasGroup>();

            var seq = DOTween.Sequence();

            // Fade out current content
            if (imgCG != null)
            {
                seq.Append(imgCG.DOFade(0f, fadeOut));
                seq.Join(stepImage.transform.DOScale(0.85f, fadeOut));
            }
            if (titleCG != null) seq.Join(titleCG.DOFade(0f, fadeOut));
            if (descCG != null) seq.Join(descCG.DOFade(0f, fadeOut));

            // Switch to new step
            seq.AppendCallback(() =>
            {
                ShowStep(newIndex);
                if (imgCG != null) { imgCG.alpha = 0f; stepImage.transform.localScale = Vector3.one * 0.85f; }
                if (titleCG != null) titleCG.alpha = 0f;
                if (descCG != null) descCG.alpha = 0f;
            });

            // Fade in new content
            if (imgCG != null)
            {
                seq.Append(imgCG.DOFade(1f, fadeIn));
                seq.Join(stepImage.transform.DOScale(1f, fadeIn).SetEase(Ease.OutBack));
            }
            if (titleCG != null) seq.Join(titleCG.DOFade(1f, fadeIn));
            if (descCG != null) seq.Join(descCG.DOFade(1f, fadeIn));

            seq.OnComplete(() => isTransitioning = false);
        }

        #region Animations

        private void AnimateEntrance()
        {
            // Progress bar slide from top
            if (progressBarTransform != null)
            {
                Vector2 pos = progressBarTransform.anchoredPosition;
                progressBarTransform.anchoredPosition = new Vector2(pos.x, pos.y + 100);
                progressBarTransform.DOAnchorPos(pos, 0.3f).SetEase(Ease.OutCubic);
            }

            // Top bar slide from top
            if (topBarTransform != null)
            {
                Vector2 pos = topBarTransform.anchoredPosition;
                topBarTransform.anchoredPosition = new Vector2(pos.x, pos.y + 150);
                topBarTransform.DOAnchorPos(pos, 0.35f).SetDelay(0.1f).SetEase(Ease.OutCubic);
            }

            // Step image scale from 0
            if (stepImage != null)
            {
                stepImage.transform.localScale = Vector3.zero;
                stepImage.transform.DOScale(1f, 0.4f).SetDelay(0.15f).SetEase(Ease.OutBack);
            }

            // Title fade in
            if (titleText != null)
            {
                var cg = titleText.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                cg.DOFade(1f, 0.35f).SetDelay(0.25f);
            }

            // Description fade in
            if (descriptionText != null)
            {
                var cg = descriptionText.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                cg.DOFade(1f, 0.35f).SetDelay(0.3f);
            }

            // Dots fade in
            if (dotsTransform != null)
            {
                var cg = dotsTransform.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                cg.DOFade(1f, 0.3f).SetDelay(0.35f);
            }

            // Navigation slide from bottom
            if (navigationTransform != null)
            {
                Vector2 pos = navigationTransform.anchoredPosition;
                navigationTransform.anchoredPosition = new Vector2(pos.x, pos.y - 100);
                navigationTransform.DOAnchorPos(pos, 0.35f).SetDelay(0.2f).SetEase(Ease.OutBack);
            }
        }

        #endregion

        private void OnSkipClicked()
        {
            // Mark as completed without rewards
            CompleteOnboarding(giveRewards: false);
        }

        private void OnBackClicked()
        {
            OnPrevClicked();
        }

        private void OnStartPlaying()
        {
            CompleteOnboarding();
        }

        private void CompleteOnboarding(bool giveRewards = true)
        {
            // Mark onboarding as complete
            PlayerPrefs.SetInt("OnboardingComplete", 1);

            if (giveRewards)
            {
                // Give completion rewards
                int currentCoins = PlayerPrefs.GetInt("PlayerCoins", 0);
                PlayerPrefs.SetInt("PlayerCoins", currentCoins + completionRewardCoins);

                int currentGems = PlayerPrefs.GetInt("PlayerGems", 0);
                PlayerPrefs.SetInt("PlayerGems", currentGems + completionRewardGems);
            }

            PlayerPrefs.Save();

            Debug.Log($"[Onboarding] Completed. Rewards given: {giveRewards}");

            // Navigate to main menu
            SceneNavigator.Instance?.NavigateTo(SceneNavigator.Scenes.MAIN_MENU);
        }

        /// <summary>
        /// Check if onboarding has been completed
        /// </summary>
        public static bool IsOnboardingComplete()
        {
            return PlayerPrefs.GetInt("OnboardingComplete", 0) == 1;
        }

        /// <summary>
        /// Reset onboarding (for testing)
        /// </summary>
        public static void ResetOnboarding()
        {
            PlayerPrefs.DeleteKey("OnboardingComplete");
            PlayerPrefs.DeleteKey("PlayerName");
            PlayerPrefs.DeleteKey("PlayerAvatar");
            PlayerPrefs.Save();
        }

        private string L(string key, params object[] args)
        {
            if (LocalizationManager.Instance == null) return key;
            string text = LocalizationManager.Instance.GetText(key);
            return args.Length > 0 ? string.Format(text, args) : text;
        }
    }

    [Serializable]
    public class OnboardingStep
    {
        public string id;
        public string title;
        [TextArea] public string description;
        public OnboardingStepType stepType = OnboardingStepType.Info;
        public bool requiresInteraction = false;
        public string highlightTooltip;
        public RectTransform highlightTarget;
    }

    public enum OnboardingStepType
    {
        Info,
        NameInput,
        AvatarSelection,
        Interactive,
        Completion
    }

    [Serializable]
    public class AvatarOption
    {
        public string id;
        public string name;
        public Sprite sprite;
    }
}
