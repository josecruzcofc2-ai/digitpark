using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using DigitPark.Monetization;
using DigitPark.Navigation;
using DigitPark.Localization;
using DigitPark.UI;
using DG.Tweening;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager para la escena de onboarding/tutorial.
    /// Arquitectura slide-based: cada step es un slide independiente dentro de SlidesContainer.
    /// </summary>
    public class OnboardingManager : MonoBehaviour
    {
        [Header("UI - Main")]
        [SerializeField] private Button skipButton;
        [SerializeField] private TextMeshProUGUI skipButtonText;

        [Header("Slides Container")]
        [SerializeField] private Transform slidesContainer;
        private GameObject[] slideObjects;

        [Header("UI - Navigation")]
        [SerializeField] private Button nextButton;
        [SerializeField] private Button prevButton;
        [SerializeField] private TextMeshProUGUI nextButtonText;
        [SerializeField] private TextMeshProUGUI prevButtonText;
        [SerializeField] private Transform dotsContainer;
        [SerializeField] private GameObject dotPrefab;

        [Header("UI - Progress")]
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI stepCounterText;

        [Header("UI - Name Input (Slide 2)")]
        [SerializeField] private GameObject nameInputPanel;
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private Button confirmNameButton;
        [SerializeField] private TextMeshProUGUI nameErrorText;

        [Header("UI - Avatar Selection (Slide 3)")]
        [SerializeField] private GameObject avatarSelectionPanel;
        [SerializeField] private Transform avatarContainer;
        [SerializeField] private GameObject avatarOptionPrefab;

        [Header("UI - Tutorial Completion (Slide 8)")]
        [SerializeField] private GameObject completionPanel;
        [SerializeField] private TextMeshProUGUI completionTitleText;
        [SerializeField] private TextMeshProUGUI completionMessageText;
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private Button startPlayingButton;

        [Header("Onboarding Steps")]
        [SerializeField] private List<OnboardingStep> steps = new List<OnboardingStep>();

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
        [SerializeField] private float transitionDuration = 0.35f;

        // State
        private int currentStepIndex = 0;
        private List<GameObject> spawnedDots = new List<GameObject>();
        private List<GameObject> spawnedAvatars = new List<GameObject>();
        private string selectedAvatarId = "";
        private string playerName = "";
        private bool isTransitioning = false;

        private void Start()
        {
            InitializeSlideObjects();
            InitializeSteps();
            LocalizeSlideTexts();
            SetupUI();
            SetupListeners();
            ShowStep(0);
            AnimateEntrance();
        }

        private void InitializeSlideObjects()
        {
            if (slidesContainer == null) return;

            int slideCount = steps.Count > 0 ? steps.Count : 8;
            slideObjects = new GameObject[slideCount];
            for (int i = 0; i < slideCount; i++)
            {
                Transform slide = slidesContainer.Find($"Slide{i + 1}");
                if (slide != null)
                    slideObjects[i] = slide.gameObject;
            }
        }

        private void InitializeSteps()
        {
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

        private void LocalizeSlideTexts()
        {
            if (slideObjects == null) return;

            // Slide titles
            LocalizeSlideTitle(0, "onboarding_welcome_title");
            LocalizeSlideTitle(1, "onboarding_name_title");
            LocalizeSlideTitle(2, "onboarding_avatar_title");
            LocalizeSlideTitle(3, "onboarding_games_title");
            LocalizeSlideTitle(4, "onboarding_cashbattle_title");
            LocalizeSlideTitle(5, "tournament_button");
            LocalizeSlideTitle(6, "dr_title");

            // Slide 1 - Welcome content
            LocalizeContentTexts(0, new[]
            {
                ("onboarding_welcome_desc", false),
                ("onboarding_welcome_highlight", true),
                ("onboarding_welcome_b1", false),
                ("onboarding_welcome_b2", false),
                ("onboarding_welcome_b3", false)
            });

            // Slide 4 - Games content
            LocalizeContentTexts(3, new[]
            {
                ("onboarding_games_desc", false),
                ("onboarding_games_highlight", true),
                ("onboarding_games_b1", false),
                ("onboarding_games_b2", false),
                ("onboarding_games_b3", false),
                ("onboarding_games_b4", false),
                ("onboarding_games_b5", false)
            });

            // Slide 5 - CashBattle content
            LocalizeContentTexts(4, new[]
            {
                ("onboarding_cashbattle_desc", false),
                ("onboarding_cashbattle_highlight", true),
                ("onboarding_cashbattle_b1", false),
                ("onboarding_cashbattle_b2", false),
                ("onboarding_cashbattle_b3", false)
            });

            // Slide 6 - Tournaments content
            LocalizeContentTexts(5, new[]
            {
                ("onboarding_tournaments_desc", false),
                ("onboarding_tournaments_highlight", true),
                ("onboarding_tournaments_b1", false),
                ("onboarding_tournaments_b2", false),
                ("onboarding_tournaments_b3", false)
            });

            // Slide 7 - Rewards content
            LocalizeContentTexts(6, new[]
            {
                ("onboarding_rewards_desc", false),
                ("onboarding_rewards_highlight", true),
                ("onboarding_rewards_b1", false),
                ("onboarding_rewards_b2", false),
                ("onboarding_rewards_b3", false)
            });

            // Slide 2 - Name Input placeholder
            if (slideObjects[1] != null)
            {
                var placeholder = slideObjects[1].transform.Find("NameInputPanel/InputContainer/NameInput/Text Area/Placeholder");
                if (placeholder != null)
                {
                    var tmp = placeholder.GetComponent<TextMeshProUGUI>();
                    if (tmp != null) tmp.text = AutoLocalizer.Get("onboarding_name_placeholder");
                }
            }

            // Navigation buttons
            if (prevButtonText != null) prevButtonText.text = AutoLocalizer.Get("onboarding_back");
            if (skipButtonText != null) skipButtonText.text = AutoLocalizer.Get("onboarding_skip");
        }

        private void LocalizeSlideTitle(int slideIndex, string key)
        {
            if (slideIndex < 0 || slideIndex >= slideObjects.Length || slideObjects[slideIndex] == null) return;
            var titleT = slideObjects[slideIndex].transform.Find("SlideTitle");
            if (titleT != null)
            {
                var tmp = titleT.GetComponent<TextMeshProUGUI>();
                if (tmp != null) tmp.text = AutoLocalizer.Get(key);
            }
        }

        private void LocalizeContentTexts(int slideIndex, (string key, bool isBulletSkip)[] keys)
        {
            if (slideIndex < 0 || slideIndex >= slideObjects.Length || slideObjects[slideIndex] == null) return;

            var contentT = slideObjects[slideIndex].transform.Find("ContentCard/Content");
            if (contentT == null) return;

            // Gather all Text children (skip Spacer objects)
            var textComponents = new List<TextMeshProUGUI>();
            for (int i = 0; i < contentT.childCount; i++)
            {
                var child = contentT.GetChild(i);
                var tmp = child.GetComponent<TextMeshProUGUI>();
                if (tmp != null) textComponents.Add(tmp);
            }

            int textIdx = 0;
            for (int i = 0; i < keys.Length && textIdx < textComponents.Count; i++)
            {
                string localized = AutoLocalizer.Get(keys[i].key);
                // Preserve bullet prefix if text already has one
                if (textComponents[textIdx].text.StartsWith("\u2022 "))
                    textComponents[textIdx].text = "\u2022 " + localized;
                else
                    textComponents[textIdx].text = localized;
                textIdx++;
            }
        }

        private void SetupUI()
        {
            // Deactivate all slides except first
            if (slideObjects != null)
            {
                for (int i = 0; i < slideObjects.Length; i++)
                    if (slideObjects[i] != null)
                        slideObjects[i].SetActive(i == 0);
            }

            if (skipButton) skipButton.gameObject.SetActive(allowSkip);
            if (prevButton) prevButton.gameObject.SetActive(false);

            CreateNavigationDots();
            UpdateProgressBar();
        }

        private void SetupListeners()
        {
            if (skipButton) skipButton.onClick.AddListener(OnSkipClicked);
            if (nextButton) nextButton.onClick.AddListener(OnNextClicked);
            if (prevButton) prevButton.onClick.AddListener(OnPrevClicked);

            if (confirmNameButton) confirmNameButton.onClick.AddListener(OnConfirmName);
            if (nameInput) nameInput.onEndEdit.AddListener(_ => OnConfirmName());

            if (startPlayingButton) startPlayingButton.onClick.AddListener(OnStartPlaying);
        }

        private void CreateNavigationDots()
        {
            if (dotsContainer == null) return;

            foreach (var dot in spawnedDots)
            {
                if (dot) Destroy(dot);
            }
            spawnedDots.Clear();

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

            // Activate only the current slide
            if (slideObjects != null)
            {
                for (int i = 0; i < slideObjects.Length; i++)
                    if (slideObjects[i] != null)
                        slideObjects[i].SetActive(i == index);
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
            }

            // Auto advance for info steps
            if (!step.requiresInteraction && autoAdvanceDelay > 0)
            {
                CancelInvoke(nameof(AutoAdvance));
                Invoke(nameof(AutoAdvance), autoAdvanceDelay);
            }

            Debug.Log($"[Onboarding] Showing step {index + 1}: {step.id}");
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
                nameInputPanel.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack).SetLink(nameInputPanel);

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

            if (string.IsNullOrWhiteSpace(name))
            {
                ShowNameError(AutoLocalizer.Get("onboarding_name_error_empty"));
                return;
            }

            if (name.Length < 3)
            {
                ShowNameError(AutoLocalizer.Get("onboarding_name_error_short"));
                return;
            }

            if (name.Length > 20)
            {
                ShowNameError(AutoLocalizer.Get("onboarding_name_error_long"));
                return;
            }

            playerName = name;
            PlayerPrefs.SetString("PlayerName", playerName);
            PlayerPrefs.Save();

            if (nextButton) nextButton.interactable = true;
            // Show hint: name can be changed later in Settings
            if (nameErrorText)
            {
                nameErrorText.text = AutoLocalizer.Get("onboarding_name_change_hint");
                nameErrorText.color = new Color(0f, 0.9f, 0.8f, 0.9f); // cyan hint
                nameErrorText.gameObject.SetActive(true);
            }

            Debug.Log($"[Onboarding] Name set: {playerName}");
        }

        private void ShowNameError(string message)
        {
            if (nameErrorText)
            {
                nameErrorText.text = message;
                nameErrorText.color = new Color(1f, 0.3f, 0.3f, 1f); // red error
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

            foreach (var avatar in spawnedAvatars)
            {
                if (avatar) Destroy(avatar);
            }
            spawnedAvatars.Clear();

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

            var nameObj = new GameObject("Name");
            nameObj.transform.SetParent(item.transform, false);
            var nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0);
            nameRT.anchorMax = new Vector2(1, 0.3f);
            nameRT.offsetMin = new Vector2(5, 5);
            nameRT.offsetMax = new Vector2(-5, 0);

            var nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = option.name;
            nameText.fontSize = FontSizes.Body;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = Color.white;

            return item;
        }

        private void OnAvatarSelected(string avatarId, GameObject selectedItem)
        {
            selectedAvatarId = avatarId;

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

            if (currentStep.stepType == OnboardingStepType.NameInput)
            {
                if (string.IsNullOrWhiteSpace(playerName))
                {
                    ShowNameError(AutoLocalizer.Get("onboarding_name_error_empty"));
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
            if (slideObjects == null) return;
            isTransitioning = true;

            GameObject oldSlide = (currentStepIndex >= 0 && currentStepIndex < slideObjects.Length)
                ? slideObjects[currentStepIndex] : null;
            GameObject newSlide = (newIndex >= 0 && newIndex < slideObjects.Length)
                ? slideObjects[newIndex] : null;

            // Activate new slide (invisible initially)
            if (newSlide != null)
            {
                newSlide.SetActive(true);
                var newCG = newSlide.GetComponent<CanvasGroup>() ?? newSlide.AddComponent<CanvasGroup>();
                newCG.alpha = 0f;
                newSlide.transform.localScale = Vector3.one * 0.95f;
            }

            var seq = DOTween.Sequence().SetLink(gameObject);
            float fadeDuration = transitionDuration * 0.6f;

            // Fade out old slide
            if (oldSlide != null)
            {
                var oldCG = oldSlide.GetComponent<CanvasGroup>() ?? oldSlide.AddComponent<CanvasGroup>();
                seq.Append(oldCG.DOFade(0f, fadeDuration));
            }

            // Fade in new slide
            if (newSlide != null)
            {
                var newCG = newSlide.GetComponent<CanvasGroup>();
                if (newCG != null)
                {
                    seq.Join(newCG.DOFade(1f, fadeDuration));
                    seq.Join(newSlide.transform.DOScale(1f, fadeDuration).SetEase(Ease.OutCubic));
                }
            }

            seq.OnComplete(() =>
            {
                // Deactivate old slide
                if (oldSlide != null)
                {
                    oldSlide.SetActive(false);
                    var oldCG = oldSlide.GetComponent<CanvasGroup>();
                    if (oldCG != null) oldCG.alpha = 1f;
                }

                ShowStep(newIndex);
                isTransitioning = false;
            });
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

            // First slide scale from 0
            if (slideObjects != null && slideObjects[0] != null)
            {
                slideObjects[0].transform.localScale = Vector3.zero;
                slideObjects[0].transform.DOScale(1f, 0.4f).SetDelay(0.15f).SetEase(Ease.OutBack);
            }

            // Dots fade in
            if (dotsTransform != null)
            {
                var cg = dotsTransform.gameObject.GetComponent<CanvasGroup>() ?? dotsTransform.gameObject.AddComponent<CanvasGroup>();
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
            CompleteOnboarding(giveRewards: false);
        }

        private void OnStartPlaying()
        {
            CompleteOnboarding();
        }

        private void CompleteOnboarding(bool giveRewards = true)
        {
            PlayerPrefs.SetInt("OnboardingComplete", 1);

            if (giveRewards)
            {
                int currentCoins = PlayerPrefs.GetInt("PlayerCoins", 0);
                PlayerPrefs.SetInt("PlayerCoins", currentCoins + completionRewardCoins);

                int currentGems = PlayerPrefs.GetInt("PlayerGems", 0);
                PlayerPrefs.SetInt("PlayerGems", currentGems + completionRewardGems);
            }

            PlayerPrefs.Save();

            Debug.Log($"[Onboarding] Completed. Rewards given: {giveRewards}");

            SceneNavigator.Instance?.NavigateTo(SceneNavigator.Scenes.MAIN_MENU);
        }

        public static bool IsOnboardingComplete()
        {
            return PlayerPrefs.GetInt("OnboardingComplete", 0) == 1;
        }

        public static void ResetOnboarding()
        {
            PlayerPrefs.DeleteKey("OnboardingComplete");
            PlayerPrefs.DeleteKey("PlayerName");
            PlayerPrefs.DeleteKey("PlayerAvatar");
            PlayerPrefs.Save();
        }

        private string L(string key, params object[] args)
        {
            return args.Length > 0 ? AutoLocalizer.Get(key, args) : AutoLocalizer.Get(key);
        }

        private void OnDestroy()
        {
            transform.DOKill();

            // Remove all button listeners
            skipButton?.onClick.RemoveAllListeners();
            nextButton?.onClick.RemoveAllListeners();
            prevButton?.onClick.RemoveAllListeners();
            confirmNameButton?.onClick.RemoveAllListeners();
            startPlayingButton?.onClick.RemoveAllListeners();
            nameInput?.onEndEdit.RemoveAllListeners();
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
