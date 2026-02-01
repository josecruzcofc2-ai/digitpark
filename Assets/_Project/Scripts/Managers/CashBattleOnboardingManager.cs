using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DigitPark.Monetization;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager for Cash Battle Onboarding scene
    /// Guides users through the real-money gameplay flow:
    /// Welcome → Verification (18+) → Deposit → Play → Win
    /// </summary>
    public class CashBattleOnboardingManager : MonoBehaviour
    {
        [Header("Slides Container")]
        [SerializeField] private Transform slidesContainer;

        [Header("Navigation Buttons")]
        [SerializeField] private Button nextButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private TextMeshProUGUI nextButtonText;

        [Header("Navigation Dots (Optional)")]
        [SerializeField] private Transform dotsContainer;
        [SerializeField] private GameObject dotPrefab;

        [Header("Progress Indicator (Optional)")]
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI progressText;

        [Header("Configuration")]
        [SerializeField] private int totalSlides = 5;
        [SerializeField] private bool allowSkip = true;
        [SerializeField] private float transitionDuration = 0.3f;

        // State
        private int currentSlideIndex = 0;
        private GameObject[] slides;
        private GameObject[] spawnedDots;
        private bool isTransitioning = false;

        // Constants
        private const string PREFS_KEY_COMPLETED = "CashBattleOnboardingComplete";

        private void Start()
        {
            InitializeSlides();
            SetupUI();
            SetupListeners();
            ShowSlide(0);
        }

        private void InitializeSlides()
        {
            if (slidesContainer == null)
            {
                Debug.LogError("[CashBattleOnboarding] SlidesContainer not assigned!");
                return;
            }

            // Find all slides (Slide1, Slide2, Slide3, Slide4, Slide5)
            slides = new GameObject[totalSlides];
            for (int i = 0; i < totalSlides; i++)
            {
                Transform slideTransform = slidesContainer.Find($"Slide{i + 1}");
                if (slideTransform != null)
                {
                    slides[i] = slideTransform.gameObject;
                }
                else
                {
                    Debug.LogWarning($"[CashBattleOnboarding] Slide{i + 1} not found!");
                }
            }

            Debug.Log($"[CashBattleOnboarding] Initialized {slides.Length} slides");
        }

        private void SetupUI()
        {
            // Hide all slides initially
            foreach (var slide in slides)
            {
                if (slide != null) slide.SetActive(false);
            }

            // Setup buttons
            if (skipButton) skipButton.gameObject.SetActive(allowSkip);
            if (backButton) backButton.gameObject.SetActive(false);

            // Create navigation dots
            CreateNavigationDots();
            UpdateProgressBar();
        }

        private void SetupListeners()
        {
            if (nextButton) nextButton.onClick.AddListener(OnNextClicked);
            if (backButton) backButton.onClick.AddListener(OnBackClicked);
            if (skipButton) skipButton.onClick.AddListener(OnSkipClicked);
        }

        private void CreateNavigationDots()
        {
            if (dotsContainer == null) return;

            // Clear existing
            foreach (Transform child in dotsContainer)
            {
                Destroy(child.gameObject);
            }

            spawnedDots = new GameObject[totalSlides];

            for (int i = 0; i < totalSlides; i++)
            {
                GameObject dot;

                if (dotPrefab != null)
                {
                    dot = Instantiate(dotPrefab, dotsContainer);
                }
                else
                {
                    // Create fallback dot
                    dot = new GameObject($"Dot_{i}");
                    dot.transform.SetParent(dotsContainer, false);

                    RectTransform rt = dot.AddComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(12, 12);

                    Image img = dot.AddComponent<Image>();
                    img.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);

                    // Make it circular
                    var mask = dot.AddComponent<Mask>();
                    mask.showMaskGraphic = true;
                }

                spawnedDots[i] = dot;
            }

            Debug.Log($"[CashBattleOnboarding] Created {spawnedDots.Length} navigation dots");
        }

        private void UpdateNavigationDots()
        {
            if (spawnedDots == null || spawnedDots.Length == 0) return;

            for (int i = 0; i < spawnedDots.Length; i++)
            {
                if (spawnedDots[i] == null) continue;

                Image dotImage = spawnedDots[i].GetComponent<Image>();
                if (dotImage == null) continue;

                bool isActive = i == currentSlideIndex;
                bool isPast = i < currentSlideIndex;

                // GOLD color scheme
                if (isActive)
                {
                    dotImage.color = new Color(1f, 0.843f, 0f, 1f); // Gold
                    spawnedDots[i].transform.localScale = Vector3.one * 1.4f;
                }
                else if (isPast)
                {
                    dotImage.color = new Color(1f, 0.647f, 0f, 0.8f); // Orange gold
                    spawnedDots[i].transform.localScale = Vector3.one;
                }
                else
                {
                    dotImage.color = new Color(0.3f, 0.3f, 0.3f, 0.5f); // Dark gray
                    spawnedDots[i].transform.localScale = Vector3.one;
                }
            }
        }

        private void UpdateProgressBar()
        {
            if (progressBar != null)
            {
                progressBar.maxValue = totalSlides - 1;
                progressBar.value = currentSlideIndex;
            }

            if (progressText != null)
            {
                progressText.text = $"{currentSlideIndex + 1} / {totalSlides}";
            }
        }

        private void ShowSlide(int index)
        {
            if (index < 0 || index >= slides.Length)
            {
                Debug.LogWarning($"[CashBattleOnboarding] Invalid slide index: {index}");
                return;
            }

            currentSlideIndex = index;

            // Deactivate all slides
            foreach (var slide in slides)
            {
                if (slide != null) slide.SetActive(false);
            }

            // Activate current slide
            if (slides[index] != null)
            {
                slides[index].SetActive(true);
            }

            // Update navigation
            UpdateNavigationButtons();
            UpdateNavigationDots();
            UpdateProgressBar();

            Debug.Log($"[CashBattleOnboarding] Showing slide {index + 1}/{totalSlides}");
        }

        private void UpdateNavigationButtons()
        {
            // Back button only visible after first slide
            if (backButton)
            {
                backButton.gameObject.SetActive(currentSlideIndex > 0);
            }

            // Update Next button text
            if (nextButtonText)
            {
                if (currentSlideIndex == totalSlides - 1)
                {
                    nextButtonText.text = "EMPEZAR AHORA";
                }
                else
                {
                    nextButtonText.text = "SIGUIENTE";
                }
            }
        }

        private void OnNextClicked()
        {
            if (isTransitioning) return;

            if (currentSlideIndex < totalSlides - 1)
            {
                // Go to next slide
                StartCoroutine(TransitionToSlide(currentSlideIndex + 1));
            }
            else
            {
                // Last slide - complete onboarding
                CompleteOnboarding();
            }
        }

        private void OnBackClicked()
        {
            if (isTransitioning) return;

            if (currentSlideIndex > 0)
            {
                StartCoroutine(TransitionToSlide(currentSlideIndex - 1));
            }
        }

        private void OnSkipClicked()
        {
            // Skip directly to completion (no rewards for skipping)
            CompleteOnboarding(markAsComplete: true);
        }

        private IEnumerator TransitionToSlide(int targetIndex)
        {
            isTransitioning = true;

            // Optional: Add fade/slide animation here
            yield return new WaitForSeconds(transitionDuration * 0.5f);

            ShowSlide(targetIndex);

            yield return new WaitForSeconds(transitionDuration * 0.5f);

            isTransitioning = false;
        }

        private void CompleteOnboarding(bool markAsComplete = true)
        {
            if (markAsComplete)
            {
                // Mark as completed
                PlayerPrefs.SetInt(PREFS_KEY_COMPLETED, 1);
                PlayerPrefs.Save();

                Debug.Log("[CashBattleOnboarding] Onboarding completed!");
            }

            // Navigate to Cash Battle Hub
            NavigateToCashBattleHub();
        }

        private void NavigateToCashBattleHub()
        {
            // Check if user is verified first
            if (!AgeVerificationManager.IsVerified())
            {
                Debug.LogWarning("[CashBattleOnboarding] User not verified! Redirecting to AgeVerification...");
                SceneNavigator.Instance?.NavigateTo(SceneNavigator.Scenes.AGE_VERIFICATION);
                return;
            }

            // Navigate to Cash Battle Hub
            if (SceneNavigator.Instance != null)
            {
                Debug.Log("[CashBattleOnboarding] Navigating to Cash Battle Hub");
                SceneNavigator.Instance.NavigateTo(SceneNavigator.Scenes.CASH_BATTLE_HUB);
            }
            else
            {
                Debug.LogError("[CashBattleOnboarding] SceneNavigator not available!");
            }
        }

        /// <summary>
        /// Check if Cash Battle onboarding has been completed
        /// </summary>
        public static bool IsOnboardingComplete()
        {
            return PlayerPrefs.GetInt(PREFS_KEY_COMPLETED, 0) == 1;
        }

        /// <summary>
        /// Reset Cash Battle onboarding (for testing)
        /// </summary>
        public static void ResetOnboarding()
        {
            PlayerPrefs.DeleteKey(PREFS_KEY_COMPLETED);
            PlayerPrefs.Save();
            Debug.Log("[CashBattleOnboarding] Onboarding reset!");
        }

        /// <summary>
        /// Manual slide navigation (for testing or external control)
        /// </summary>
        public void GoToSlide(int slideIndex)
        {
            if (!isTransitioning)
            {
                StartCoroutine(TransitionToSlide(slideIndex));
            }
        }

        // Public getters
        public int CurrentSlideIndex => currentSlideIndex;
        public int TotalSlides => totalSlides;
        public bool IsTransitioning => isTransitioning;

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-find references if not set
            if (slidesContainer == null)
            {
                Transform safeArea = transform.Find("SafeArea");
                if (safeArea != null)
                {
                    slidesContainer = safeArea.Find("SlidesContainer");
                }
            }

            if (nextButton == null)
            {
                Transform navPanel = transform.Find("SafeArea/NavigationPanel");
                if (navPanel != null)
                {
                    Transform buttonsContainer = navPanel.Find("Buttons");
                    if (buttonsContainer != null)
                    {
                        Transform startBtn = buttonsContainer.Find("StartButton");
                        if (startBtn != null)
                        {
                            nextButton = startBtn.GetComponent<Button>();
                            nextButtonText = startBtn.GetComponentInChildren<TextMeshProUGUI>();
                        }
                    }
                }
            }
        }
#endif
    }
}
