using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DigitPark.Animations
{
    /// <summary>
    /// Handles main menu entrance animations and continuous effects.
    /// Creates an engaging first impression with staggered element reveals.
    /// </summary>
    public class MainMenuAnimator : MonoBehaviour
    {
        [Header("Logo")]
        [SerializeField] private RectTransform logo;
        [SerializeField] private Image logoGlow;
        [SerializeField] private ParticleSystem logoParticles;

        [Header("Menu Buttons")]
        [SerializeField] private RectTransform[] menuButtons;
        [SerializeField] private float buttonStaggerDelay = 0.1f;

        [Header("Header Elements")]
        [SerializeField] private RectTransform headerBar;
        [SerializeField] private RectTransform currencyDisplay;
        [SerializeField] private RectTransform profileButton;

        [Header("Bottom Navigation")]
        [SerializeField] private RectTransform bottomNavBar;
        [SerializeField] private RectTransform[] navItems;

        [Header("Featured Content")]
        [SerializeField] private RectTransform featuredBanner;
        [SerializeField] private RectTransform[] featuredCards;

        [Header("Background")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private RectTransform[] floatingElements;

        [Header("Animation Settings")]
        [SerializeField] private float logoDropDuration = 0.6f;
        [SerializeField] private float buttonEntranceDuration = 0.3f;
        [SerializeField] private float headerSlideDuration = 0.4f;
        [SerializeField] private float floatSpeed = 2f;
        [SerializeField] private float floatDistance = 20f;

        [Header("Audio")]
        [SerializeField] private AudioClip entranceSound;
        [SerializeField] private AudioClip logoSound;
        [SerializeField] private AudioClip buttonAppearSound;

        // Events
        public event Action OnEntranceComplete;

        private AudioSource audioSource;
        private List<Tween> continuousTweens = new List<Tween>();

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        // ==================== ENTRANCE SEQUENCE ====================

        /// <summary>
        /// Play the full entrance animation sequence
        /// </summary>
        public void PlayEntranceSequence()
        {
            StartCoroutine(EntranceSequenceCoroutine());
        }

        private IEnumerator EntranceSequenceCoroutine()
        {
            // Hide all elements initially
            PrepareElementsForEntrance();

            yield return new WaitForSeconds(0.2f);

            // Play entrance sound
            if (entranceSound != null && audioSource != null)
                audioSource.PlayOneShot(entranceSound);

            // Phase 1: Background fade in
            if (backgroundImage != null)
            {
                backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, 0f);
                backgroundImage.DOFade(1f, 0.5f).SetLink(backgroundImage.gameObject);
            }

            yield return new WaitForSeconds(0.3f);

            // Phase 2: Logo drop
            yield return StartCoroutine(AnimateLogoEntrance());

            // Phase 3: Header slide down
            yield return StartCoroutine(AnimateHeaderEntrance());

            // Phase 4: Menu buttons staggered entrance
            yield return StartCoroutine(AnimateButtonsEntrance());

            // Phase 5: Featured content
            yield return StartCoroutine(AnimateFeaturedEntrance());

            // Phase 6: Bottom nav slide up
            yield return StartCoroutine(AnimateBottomNavEntrance());

            // Start continuous animations
            StartContinuousAnimations();

            OnEntranceComplete?.Invoke();
        }

        /// <summary>
        /// Quick entrance (skip some animations)
        /// </summary>
        public void PlayQuickEntrance()
        {
            // Show all elements instantly
            ShowAllElements();

            // Just play button pulse and start continuous
            if (menuButtons != null)
            {
                foreach (var button in menuButtons)
                {
                    if (button != null)
                        button.DOPunchScale(Vector3.one * 0.1f, 0.3f, 3).SetLink(button.gameObject);
                }
            }

            StartContinuousAnimations();
        }

        // ==================== ENTRANCE PHASES ====================

        private void PrepareElementsForEntrance()
        {
            // Logo - above screen
            if (logo != null)
            {
                logo.anchoredPosition = new Vector2(logo.anchoredPosition.x, Screen.height);
                logo.localScale = Vector3.one * 1.5f;
            }

            // Header - above screen
            if (headerBar != null)
                headerBar.anchoredPosition = new Vector2(headerBar.anchoredPosition.x, 200f);

            // Menu buttons - scaled to zero
            if (menuButtons != null)
            {
                foreach (var button in menuButtons)
                {
                    if (button != null)
                        button.localScale = Vector3.zero;
                }
            }

            // Featured - off to the right
            if (featuredBanner != null)
                featuredBanner.anchoredPosition = new Vector2(Screen.width, featuredBanner.anchoredPosition.y);

            if (featuredCards != null)
            {
                foreach (var card in featuredCards)
                {
                    if (card != null)
                        card.localScale = Vector3.zero;
                }
            }

            // Bottom nav - below screen
            if (bottomNavBar != null)
                bottomNavBar.anchoredPosition = new Vector2(bottomNavBar.anchoredPosition.x, -200f);

            // Nav items - scaled to zero
            if (navItems != null)
            {
                foreach (var item in navItems)
                {
                    if (item != null)
                        item.localScale = Vector3.zero;
                }
            }
        }

        private IEnumerator AnimateLogoEntrance()
        {
            if (logo == null) yield break;

            if (logoSound != null && audioSource != null)
                audioSource.PlayOneShot(logoSound);

            // Drop with bounce
            Sequence logoSeq = DOTween.Sequence().SetLink(logo.gameObject);
            logoSeq.Append(logo.DOAnchorPosY(0, logoDropDuration).SetEase(Ease.OutBounce));
            logoSeq.Join(logo.DOScale(1f, logoDropDuration).SetEase(Ease.OutBack));

            yield return logoSeq.WaitForCompletion();

            // Logo glow
            if (logoGlow != null)
            {
                logoGlow.gameObject.SetActive(true);
                logoGlow.color = new Color(logoGlow.color.r, logoGlow.color.g, logoGlow.color.b, 0f);
                logoGlow.DOFade(0.8f, 0.3f).SetLink(logoGlow.gameObject);
            }

            // Logo particles
            if (logoParticles != null)
                logoParticles.Play();
        }

        private IEnumerator AnimateHeaderEntrance()
        {
            Sequence headerSeq = DOTween.Sequence().SetLink(gameObject);

            if (headerBar != null)
                headerSeq.Append(headerBar.DOAnchorPosY(0, headerSlideDuration).SetEase(Ease.OutQuad));

            if (currencyDisplay != null)
            {
                currencyDisplay.localScale = Vector3.zero;
                headerSeq.Append(currencyDisplay.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
            }

            if (profileButton != null)
            {
                profileButton.localScale = Vector3.zero;
                headerSeq.Join(profileButton.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
            }

            yield return headerSeq.WaitForCompletion();
        }

        private IEnumerator AnimateButtonsEntrance()
        {
            if (menuButtons == null) yield break;

            for (int i = 0; i < menuButtons.Length; i++)
            {
                if (menuButtons[i] == null) continue;

                if (buttonAppearSound != null && audioSource != null)
                    audioSource.PlayOneShot(buttonAppearSound, 0.5f);

                menuButtons[i].DOScale(1f, buttonEntranceDuration).SetEase(Ease.OutBack).SetLink(menuButtons[i].gameObject);
                yield return new WaitForSeconds(buttonStaggerDelay);
            }
        }

        private IEnumerator AnimateFeaturedEntrance()
        {
            if (featuredBanner != null)
            {
                featuredBanner.DOAnchorPosX(0, 0.4f).SetEase(Ease.OutQuad).SetLink(featuredBanner.gameObject);
                yield return new WaitForSeconds(0.2f);
            }

            if (featuredCards == null) yield break;

            for (int i = 0; i < featuredCards.Length; i++)
            {
                if (featuredCards[i] == null) continue;

                featuredCards[i].DOScale(1f, 0.25f).SetEase(Ease.OutBack).SetLink(featuredCards[i].gameObject);
                yield return new WaitForSeconds(0.08f);
            }
        }

        private IEnumerator AnimateBottomNavEntrance()
        {
            if (bottomNavBar != null)
            {
                bottomNavBar.DOAnchorPosY(0, 0.3f).SetEase(Ease.OutQuad).SetLink(bottomNavBar.gameObject);
                yield return new WaitForSeconds(0.1f);
            }

            if (navItems == null) yield break;

            for (int i = 0; i < navItems.Length; i++)
            {
                if (navItems[i] == null) continue;

                navItems[i].DOScale(1f, 0.2f).SetEase(Ease.OutBack).SetLink(navItems[i].gameObject);
                yield return new WaitForSeconds(0.05f);
            }
        }

        // ==================== CONTINUOUS ANIMATIONS ====================

        private void StartContinuousAnimations()
        {
            // Logo breathing
            if (logo != null)
            {
                continuousTweens.Add(
                    logo.DOScale(1.02f, 2f)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetLink(logo.gameObject)
                );
            }

            // Logo glow pulse
            if (logoGlow != null)
            {
                continuousTweens.Add(
                    logoGlow.DOFade(0.5f, 1.5f)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetLink(logoGlow.gameObject)
                );
            }

            // Floating elements
            if (floatingElements != null)
            {
                foreach (var element in floatingElements)
                {
                    if (element == null) continue;

                    float randomOffset = UnityEngine.Random.Range(0f, 2f);
                    float randomSpeed = floatSpeed + UnityEngine.Random.Range(-0.5f, 0.5f);

                    continuousTweens.Add(
                        element.DOAnchorPosY(element.anchoredPosition.y + floatDistance, randomSpeed)
                            .SetEase(Ease.InOutSine)
                            .SetLoops(-1, LoopType.Yoyo)
                            .SetDelay(randomOffset)
                            .SetLink(element.gameObject)
                    );
                }
            }
        }

        public void StopContinuousAnimations()
        {
            foreach (var tween in continuousTweens)
            {
                tween?.Kill();
            }
            continuousTweens.Clear();
        }

        // ==================== BUTTON INTERACTIONS ====================

        /// <summary>
        /// Play attention animation on a specific button
        /// </summary>
        public void PlayButtonAttention(int buttonIndex)
        {
            if (buttonIndex < 0 || buttonIndex >= menuButtons.Length) return;

            var button = menuButtons[buttonIndex];
            if (button == null) return;

            DOTween.Sequence()
                .Append(button.DOScale(1.1f, 0.2f).SetEase(Ease.OutQuad))
                .Append(button.DOScale(1f, 0.3f).SetEase(Ease.OutBounce))
                .SetLoops(2)
                .SetLink(button.gameObject);
        }

        /// <summary>
        /// Highlight a menu button
        /// </summary>
        public void HighlightButton(int buttonIndex)
        {
            if (buttonIndex < 0 || buttonIndex >= menuButtons.Length) return;

            var button = menuButtons[buttonIndex];
            if (button == null) return;

            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                // Track in continuousTweens so StopContinuousAnimations() can kill them
                continuousTweens.Add(
                    buttonImage.DOColor(Color.white, 0.3f)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetLink(button.gameObject)
                );
            }

            continuousTweens.Add(
                button.DOScale(1.05f, 0.5f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetLink(button.gameObject)
            );
        }

        // ==================== EXIT ANIMATIONS ====================

        /// <summary>
        /// Play exit animation when navigating away
        /// </summary>
        public void PlayExitAnimation(Action onComplete = null)
        {
            StopContinuousAnimations();

            Sequence exitSeq = DOTween.Sequence().SetLink(gameObject);

            // Buttons scale out
            if (menuButtons != null)
            {
                for (int i = menuButtons.Length - 1; i >= 0; i--)
                {
                    if (menuButtons[i] != null)
                        exitSeq.Insert((menuButtons.Length - 1 - i) * 0.05f,
                            menuButtons[i].DOScale(0f, 0.2f).SetEase(Ease.InBack));
                }
            }

            // Logo fade up
            if (logo != null)
                exitSeq.Insert(0.1f, logo.DOAnchorPosY(300f, 0.3f).SetEase(Ease.InBack));

            // Header slide up
            if (headerBar != null)
                exitSeq.Insert(0.1f, headerBar.DOAnchorPosY(200f, 0.3f).SetEase(Ease.InQuad));

            // Bottom nav slide down
            if (bottomNavBar != null)
                exitSeq.Insert(0.1f, bottomNavBar.DOAnchorPosY(-200f, 0.3f).SetEase(Ease.InQuad));

            exitSeq.OnComplete(() => onComplete?.Invoke());
        }

        // ==================== UTILITY ====================

        private void ShowAllElements()
        {
            if (logo != null)
            {
                logo.anchoredPosition = new Vector2(logo.anchoredPosition.x, 0);
                logo.localScale = Vector3.one;
            }

            if (headerBar != null)
                headerBar.anchoredPosition = new Vector2(headerBar.anchoredPosition.x, 0);

            if (menuButtons != null)
            {
                foreach (var button in menuButtons)
                {
                    if (button != null)
                        button.localScale = Vector3.one;
                }
            }

            if (bottomNavBar != null)
                bottomNavBar.anchoredPosition = new Vector2(bottomNavBar.anchoredPosition.x, 0);

            if (navItems != null)
            {
                foreach (var item in navItems)
                {
                    if (item != null)
                        item.localScale = Vector3.one;
                }
            }
        }

        /// <summary>
        /// Reset all elements to entrance-ready state
        /// </summary>
        public void Reset()
        {
            StopContinuousAnimations();
            PrepareElementsForEntrance();
        }

        private void OnDestroy()
        {
            StopContinuousAnimations();
            DOTween.Kill(logo);
            DOTween.Kill(headerBar);
            DOTween.Kill(bottomNavBar);

            if (menuButtons != null)
            {
                foreach (var button in menuButtons)
                {
                    if (button != null)
                        DOTween.Kill(button);
                }
            }
        }
    }
}
