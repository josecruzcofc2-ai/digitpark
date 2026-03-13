using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using DG.Tweening;
using DigitPark.Localization;

namespace DigitPark.UI
{
    /// <summary>
    /// UI Component for the Achievement Toast notification.
    /// Handles animations and display of achievement unlock notifications.
    /// Appears from top of screen with Xbox-style animation.
    /// </summary>
    public class AchievementToastUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("Container")]
        [SerializeField] private RectTransform toastContainer;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Background")]
        [SerializeField] private Image outerGlow;
        [SerializeField] private Image background;
        [SerializeField] private Image topAccent;
        [SerializeField] private Image bottomAccent;

        [Header("Icon")]
        [SerializeField] private Image iconGlow;
        [SerializeField] private Image iconFrame;
        [SerializeField] private Image achievementIcon;

        [Header("Text")]
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI completionText;

        [Header("Progress")]
        [SerializeField] private Image progressBarFill;
        [SerializeField] private GameObject progressSection;

        [Header("Effects")]
        [SerializeField] private RectTransform shineEffect;
        [SerializeField] private ParticleSystem sparkleParticles;

        [Header("Audio")]
        [SerializeField] private AudioClip unlockSound;
        [SerializeField] private AudioClip epicUnlockSound;

        [Header("Colors")]
        [SerializeField] private Color normalBorderColor = new Color(0f, 1f, 1f, 1f);
        [SerializeField] private Color secretBorderColor = new Color(0.7f, 0.3f, 1f, 1f);
        [SerializeField] private Color epicBorderColor = new Color(1f, 0.84f, 0f, 1f);

        [Header("Timing")]
        [SerializeField] private float slideInDuration = 0.5f;
        [SerializeField] private float displayDuration = 4f;
        [SerializeField] private float slideOutDuration = 0.3f;

        // Events
        public event Action OnToastClicked;
        public event Action OnToastDismissed;

        // State
        private bool _isShowing;
        private Tween _currentTween;
        private Sequence _displaySequence;
        private Vector2 _hiddenPosition;
        private Vector2 _shownPosition;
        private bool _positionsInitialized;

        // Achievement data for click handling
        private string _currentAchievementId;

        private void Awake()
        {
            InitializePositions();

            // Start hidden
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            gameObject.SetActive(false);
        }

        /// <summary>
        /// Calculate show/hide positions. Safe to call multiple times.
        /// </summary>
        private void InitializePositions()
        {
            if (toastContainer != null && !_positionsInitialized)
            {
                _shownPosition = toastContainer.anchoredPosition;
                _hiddenPosition = new Vector2(_shownPosition.x, _shownPosition.y + 200f);
                toastContainer.anchoredPosition = _hiddenPosition;
                _positionsInitialized = true;
            }
        }

        private void OnDestroy()
        {
            _currentTween?.Kill();
            _displaySequence?.Kill();
            // Kill any orphaned shine/snap-back tweens
            if (shineEffect != null) shineEffect.DOKill();
            if (toastContainer != null) toastContainer.DOKill();
            if (canvasGroup != null) canvasGroup.DOKill();
            CancelInvoke();
        }

        /// <summary>
        /// Show the achievement toast with data
        /// </summary>
        public void Show(AchievementToastData data)
        {
            if (_isShowing)
            {
                // Queue or replace current toast
                DismissImmediate();
            }

            // Ensure positions are calculated (may not have happened in Awake
            // if references were assigned via reflection after AddComponent)
            InitializePositions();

            _isShowing = true;
            _currentAchievementId = data.achievementId;

            // Update UI
            UpdateDisplay(data);

            // Show and animate
            gameObject.SetActive(true);
            PlayShowAnimation(data.isEpic);

            // Safety fallback: force dismiss after total expected duration + buffer
            CancelInvoke(nameof(ForceDismiss));
            float totalDuration = slideInDuration + displayDuration + slideOutDuration + 2f;
            Invoke(nameof(ForceDismiss), totalDuration);
        }

        /// <summary>
        /// Safety fallback in case DOTween animation fails to dismiss
        /// </summary>
        private void ForceDismiss()
        {
            if (_isShowing)
            {
                Debug.Log("[AchievementToast] Force dismiss (safety fallback)");
                DismissImmediate();
            }
        }

        /// <summary>
        /// Dismiss the toast immediately
        /// </summary>
        public void DismissImmediate()
        {
            CancelInvoke(nameof(ForceDismiss));
            _currentTween?.Kill();
            _displaySequence?.Kill();

            if (toastContainer != null)
            {
                toastContainer.anchoredPosition = _hiddenPosition;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            _isShowing = false;
            gameObject.SetActive(false);
            OnToastDismissed?.Invoke();
        }

        /// <summary>
        /// Dismiss with animation
        /// </summary>
        public void Dismiss()
        {
            if (!_isShowing) return;

            PlayHideAnimation();
        }

        private void UpdateDisplay(AchievementToastData data)
        {
            // Icon
            if (achievementIcon != null && data.icon != null)
            {
                achievementIcon.sprite = data.icon;
                achievementIcon.color = Color.white;
            }

            // Texts
            if (headerText != null)
            {
                headerText.text = AutoLocalizer.Get("ach_achievement_unlocked");
            }

            if (titleText != null)
            {
                titleText.text = data.title;
            }

            if (descriptionText != null)
            {
                descriptionText.text = data.description;
            }

            if (completionText != null)
            {
                completionText.text = AutoLocalizer.Get("completed");
            }

            // Progress bar (always full for completed achievements)
            if (progressBarFill != null)
            {
                progressBarFill.fillAmount = 1f;
            }

            // Update colors based on achievement type
            UpdateColors(data.isSecret, data.isEpic);
        }

        private void UpdateColors(bool isSecret, bool isEpic)
        {
            Color borderColor = isEpic ? epicBorderColor : (isSecret ? secretBorderColor : normalBorderColor);

            if (background != null)
            {
                var outline = background.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.effectColor = borderColor;
                }
            }

            if (bottomAccent != null)
            {
                bottomAccent.color = borderColor;
            }

            // Glow intensity
            if (outerGlow != null)
            {
                Color glowColor = borderColor;
                glowColor.a = isEpic ? 0.5f : 0.3f;
                outerGlow.color = glowColor;
            }

            if (iconGlow != null)
            {
                Color iconGlowColor = isEpic ? epicBorderColor : new Color(1f, 0.9f, 0.4f, 0.3f);
                iconGlowColor.a = isEpic ? 0.6f : 0.3f;
                iconGlow.color = iconGlowColor;
            }
        }

        private void PlayShowAnimation(bool isEpic)
        {
            _displaySequence?.Kill();
            _displaySequence = DOTween.Sequence();

            // Reset position
            if (toastContainer != null)
            {
                toastContainer.anchoredPosition = _hiddenPosition;
                toastContainer.localScale = new Vector3(0.8f, 0.8f, 1f);
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            // Play sound
            PlayUnlockSound(isEpic);

            // Slide in from top with bounce
            if (toastContainer != null)
            {
                _displaySequence.Append(
                    toastContainer.DOAnchorPos(_shownPosition, slideInDuration)
                        .SetEase(Ease.OutBack, 1.2f)
                );

                _displaySequence.Join(
                    toastContainer.DOScale(1f, slideInDuration * 0.8f)
                        .SetEase(Ease.OutBack)
                );
            }

            // Fade in
            if (canvasGroup != null)
            {
                _displaySequence.Join(
                    canvasGroup.DOFade(1f, slideInDuration * 0.5f)
                );
            }

            // Glow pulse on arrival
            if (outerGlow != null)
            {
                Color originalColor = outerGlow.color;
                Color brightColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0.8f);

                _displaySequence.Append(
                    outerGlow.DOColor(brightColor, 0.2f)
                );
                _displaySequence.Append(
                    outerGlow.DOColor(originalColor, 0.3f)
                );
            }

            // Icon frame pulse
            if (iconFrame != null)
            {
                _displaySequence.Join(
                    iconFrame.transform.DOPunchScale(Vector3.one * 0.2f, 0.4f, 2)
                );
            }

            // Shine effect
            if (shineEffect != null)
            {
                _displaySequence.AppendCallback(() => PlayShineEffect());
            }

            // Emit particles
            if (sparkleParticles != null)
            {
                _displaySequence.AppendCallback(() => sparkleParticles.Emit(isEpic ? 30 : 15));
            }

            // Wait for display duration
            _displaySequence.AppendInterval(displayDuration);

            // Auto-hide
            _displaySequence.AppendCallback(() => PlayHideAnimation());
        }

        private void PlayHideAnimation()
        {
            CancelInvoke(nameof(ForceDismiss));
            _currentTween?.Kill();

            Sequence hideSequence = DOTween.Sequence();

            // Slide out to top
            if (toastContainer != null)
            {
                hideSequence.Append(
                    toastContainer.DOAnchorPos(_hiddenPosition, slideOutDuration)
                        .SetEase(Ease.InCubic)
                );
            }

            // Fade out
            if (canvasGroup != null)
            {
                hideSequence.Join(
                    canvasGroup.DOFade(0f, slideOutDuration)
                );
            }

            // Scale down slightly
            if (toastContainer != null)
            {
                hideSequence.Join(
                    toastContainer.DOScale(0.9f, slideOutDuration)
                );
            }

            hideSequence.OnComplete(() =>
            {
                gameObject.SetActive(false);
                _isShowing = false;
                OnToastDismissed?.Invoke();
            });

            _currentTween = hideSequence;
        }

        private void PlayShineEffect()
        {
            if (shineEffect == null) return;

            // Kill any previous shine tween
            shineEffect.DOKill();

            // Reset position
            shineEffect.anchoredPosition = new Vector2(-100f, 0f);

            // Animate across (tracked via shineEffect target for DOKill)
            shineEffect.DOAnchorPosX(500f, 0.8f)
                .SetEase(Ease.Linear);
        }

        private void PlayUnlockSound(bool isEpic)
        {
            AudioClip clip = isEpic ? epicUnlockSound : unlockSound;

            if (clip != null)
            {
                // Try to find an AudioSource or create one
                AudioSource audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                }

                audioSource.PlayOneShot(clip);
            }
        }

        // ==================== INTERACTION ====================

        public void OnPointerClick(PointerEventData eventData)
        {
            // Dismiss and notify
            OnToastClicked?.Invoke();

            // Navigate to achievements scene
            if (!string.IsNullOrEmpty(_currentAchievementId))
            {
                NavigateToAchievements();
            }

            Dismiss();
        }

        private void NavigateToAchievements()
        {
            // Use SceneNavigator if available
            var navigator = DigitPark.Navigation.SceneNavigator.Instance;
            if (navigator != null)
            {
                navigator.NavigateTo("Achievements");
            }
            else
            {
                // Fallback
                UnityEngine.SceneManagement.SceneManager.LoadScene("Achievements");
            }
        }

        // ==================== SWIPE TO DISMISS ====================

        private Vector2 _swipeStartPos;
        private bool _isSwiping;

        public void OnBeginDrag(PointerEventData eventData)
        {
            _swipeStartPos = eventData.position;
            _isSwiping = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isSwiping) return;

            float deltaY = eventData.position.y - _swipeStartPos.y;

            // Only allow swipe up
            if (deltaY > 0 && toastContainer != null)
            {
                Vector2 newPos = _shownPosition + new Vector2(0f, deltaY * 0.5f);
                toastContainer.anchoredPosition = newPos;

                // Fade based on distance
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = Mathf.Clamp01(1f - (deltaY / 150f));
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isSwiping) return;
            _isSwiping = false;

            float deltaY = eventData.position.y - _swipeStartPos.y;

            // If swiped up enough, dismiss
            if (deltaY > 80f)
            {
                Dismiss();
            }
            else
            {
                // Snap back
                if (toastContainer != null)
                {
                    toastContainer.DOKill();
                    toastContainer.DOAnchorPos(_shownPosition, 0.2f).SetEase(Ease.OutCubic).SetLink(gameObject);
                }
                if (canvasGroup != null)
                {
                    canvasGroup.DOKill();
                    canvasGroup.DOFade(1f, 0.2f);
                }
            }
        }
    }

    /// <summary>
    /// Data structure for achievement toast notifications
    /// </summary>
    [Serializable]
    public class AchievementToastData
    {
        public string achievementId;
        public string title;
        public string description;
        public int points;
        public Sprite icon;
        public bool isSecret;
        public bool isEpic; // For achievements worth 100+ points
    }
}
