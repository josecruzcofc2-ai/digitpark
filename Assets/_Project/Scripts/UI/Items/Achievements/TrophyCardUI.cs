using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using DG.Tweening;

namespace DigitPark.UI.Items
{
    /// <summary>
    /// Trophy Card UI component for the Achievement Trophy Showcase.
    /// Features: 3D-like glass case effect, glow, particles, animations.
    /// </summary>
    public class TrophyCardUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Card Structure")]
        [SerializeField] private RectTransform cardContainer;
        [SerializeField] private Image cardBackground;
        [SerializeField] private Image glassOverlay;
        [SerializeField] private Image borderGlow;

        [Header("Trophy Display")]
        [SerializeField] private Image trophyIcon;
        [SerializeField] private Image trophyShadow;
        [SerializeField] private Image lockedOverlay;
        [SerializeField] private Image questionMark;

        [Header("Progress")]
        [SerializeField] private GameObject progressContainer;
        [SerializeField] private Image progressFill;
        [SerializeField] private Image progressBackground;
        [SerializeField] private TextMeshProUGUI progressText;

        [Header("Info")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Image completedBadge;

        [Header("Effects")]
        [SerializeField] private ParticleSystem glowParticles;
        [SerializeField] private Image shineEffect;

        [Header("Colors")]
        [SerializeField] private Color unlockedGlowColor = new Color(0f, 1f, 1f, 0.6f);
        [SerializeField] private Color lockedGlowColor = new Color(0.3f, 0.3f, 0.4f, 0.3f);
        [SerializeField] private Color progressGlowColor = new Color(1f, 0.84f, 0f, 0.5f);
        [SerializeField] private Color secretGlowColor = new Color(0.8f, 0.2f, 1f, 0.5f);

        // State
        private AchievementData _data;
        private bool _isUnlocked;
        private bool _isInProgress;
        private bool _isSecret;
        private bool _isHovering;

        // Events
        public event Action<TrophyCardUI, AchievementData> OnCardClicked;

        // Animation tweens
        private Tween _floatTween;
        private Tween _glowTween;
        private Tween _shineTween;

        private void OnEnable()
        {
            if (_isUnlocked)
            {
                StartIdleAnimations();
            }
        }

        private void OnDisable()
        {
            StopAllAnimations();
        }

        private void OnDestroy()
        {
            StopAllAnimations();
        }

        /// <summary>
        /// Initialize the trophy card with achievement data
        /// </summary>
        public void Setup(AchievementData data)
        {
            _data = data;
            _isUnlocked = data.isCompleted;
            _isInProgress = !data.isCompleted && data.currentProgress > 0;
            _isSecret = data.isSecret && !data.isCompleted;

            // Auto-find references if not assigned in Inspector
            FindReferencesIfNeeded();

            UpdateVisuals();

            if (_isUnlocked)
            {
                StartIdleAnimations();
            }
        }

        /// <summary>
        /// Automatically find child references if not assigned in Inspector
        /// </summary>
        private void FindReferencesIfNeeded()
        {
            if (cardContainer == null)
                cardContainer = transform.Find("CardContainer")?.GetComponent<RectTransform>();

            if (cardBackground == null && cardContainer != null)
                cardBackground = cardContainer.GetComponent<Image>();

            if (trophyIcon == null)
            {
                var trophyIconTransform = transform.Find("CardContainer/TrophyIcon");
                if (trophyIconTransform != null)
                    trophyIcon = trophyIconTransform.GetComponent<Image>();
            }

            if (glassOverlay == null)
            {
                var glassTransform = transform.Find("CardContainer/GlassOverlay");
                if (glassTransform != null)
                    glassOverlay = glassTransform.GetComponent<Image>();
            }

            if (borderGlow == null)
            {
                var borderTransform = transform.Find("CardContainer/BorderGlow");
                if (borderTransform != null)
                    borderGlow = borderTransform.GetComponent<Image>();
            }

            if (lockedOverlay == null)
            {
                var lockedTransform = transform.Find("CardContainer/TrophyIcon/LockedOverlay");
                if (lockedTransform != null)
                    lockedOverlay = lockedTransform.GetComponent<Image>();
            }

            if (questionMark == null)
            {
                var questionTransform = transform.Find("CardContainer/TrophyIcon/QuestionMark");
                if (questionTransform != null)
                    questionMark = questionTransform.GetComponent<Image>();
            }

            if (progressContainer == null)
            {
                var progressTransform = transform.Find("CardContainer/ProgressContainer");
                if (progressTransform != null)
                    progressContainer = progressTransform.gameObject;
            }

            if (progressFill == null && progressContainer != null)
            {
                var fillTransform = progressContainer.transform.Find("ProgressBackground/ProgressFill");
                if (fillTransform != null)
                    progressFill = fillTransform.GetComponent<Image>();
            }

            if (progressText == null && progressContainer != null)
            {
                var textTransform = progressContainer.transform.Find("ProgressText");
                if (textTransform != null)
                    progressText = textTransform.GetComponent<TextMeshProUGUI>();
            }

            if (titleText == null)
            {
                var titleTransform = transform.Find("CardContainer/TitleText");
                if (titleTransform != null)
                    titleText = titleTransform.GetComponent<TextMeshProUGUI>();
            }

            if (completedBadge == null)
            {
                var badgeTransform = transform.Find("CardContainer/CompletedBadge");
                if (badgeTransform != null)
                    completedBadge = badgeTransform.GetComponent<Image>();
            }

            if (shineEffect == null)
            {
                var shineTransform = transform.Find("CardContainer/ShineEffect");
                if (shineTransform != null)
                    shineEffect = shineTransform.GetComponent<Image>();
            }
        }

        private void UpdateVisuals()
        {
            // Title
            if (titleText)
            {
                titleText.text = _isSecret ? "???" : _data.title;
                titleText.color = _isUnlocked ? Color.white : new Color(0.6f, 0.6f, 0.6f);
            }

            // Trophy Icon
            if (trophyIcon)
            {
                if (_isSecret)
                {
                    trophyIcon.gameObject.SetActive(false);
                    if (questionMark) questionMark.gameObject.SetActive(true);
                }
                else
                {
                    trophyIcon.gameObject.SetActive(true);
                    if (questionMark) questionMark.gameObject.SetActive(false);

                    // Load icon if available
                    if (_data.icon != null)
                    {
                        trophyIcon.sprite = _data.icon;
                        Debug.Log($"[TrophyCard] Icon assigned for: {_data.title}");
                    }
                    else
                    {
                        Debug.LogWarning($"[TrophyCard] No icon for: {_data.title} (id: {_data.id})");
                    }

                    // Grayscale if locked
                    trophyIcon.color = _isUnlocked ? Color.white : new Color(0.4f, 0.4f, 0.4f);
                }
            }
            else
            {
                Debug.LogError($"[TrophyCard] trophyIcon reference is NULL for: {_data?.title}");
            }

            // Shadow
            if (trophyShadow)
            {
                trophyShadow.gameObject.SetActive(_isUnlocked && !_isSecret);
            }

            // Locked overlay
            if (lockedOverlay)
            {
                lockedOverlay.gameObject.SetActive(!_isUnlocked && !_isSecret);
            }

            // Progress
            if (progressContainer)
            {
                progressContainer.SetActive(_isInProgress);

                if (_isInProgress && progressFill)
                {
                    float progress = (float)_data.currentProgress / _data.targetProgress;
                    progressFill.fillAmount = progress;

                    if (progressText)
                    {
                        progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
                    }
                }
            }

            // Completed badge
            if (completedBadge)
            {
                if (_isUnlocked)
                {
                    completedBadge.gameObject.SetActive(true);
                    completedBadge.transform.localScale = Vector3.zero;
                    completedBadge.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
                }
                else
                {
                    completedBadge.gameObject.SetActive(false);
                }
            }

            // Glow color
            UpdateGlowColor();

            // Particles
            if (glowParticles)
            {
                if (_isUnlocked)
                {
                    glowParticles.Play();
                }
                else
                {
                    glowParticles.Stop();
                }
            }

            // Glass overlay alpha
            if (glassOverlay)
            {
                glassOverlay.color = new Color(1f, 1f, 1f, _isUnlocked ? 0.1f : 0.05f);
            }
        }

        private void UpdateGlowColor()
        {
            Color targetColor;

            if (_isSecret)
            {
                targetColor = secretGlowColor;
            }
            else if (_isUnlocked)
            {
                targetColor = unlockedGlowColor;
            }
            else if (_isInProgress)
            {
                targetColor = progressGlowColor;
            }
            else
            {
                targetColor = lockedGlowColor;
            }

            if (borderGlow)
            {
                borderGlow.color = targetColor;
            }

            if (cardBackground)
            {
                // Tint background slightly with glow color
                Color bgColor = new Color(
                    0.05f + targetColor.r * 0.05f,
                    0.08f + targetColor.g * 0.05f,
                    0.12f + targetColor.b * 0.05f,
                    0.95f
                );
                cardBackground.color = bgColor;
            }
        }

        #region Animations

        private void StartIdleAnimations()
        {
            StopAllAnimations();

            // Float animation for trophy
            if (trophyIcon && _isUnlocked)
            {
                Vector3 startPos = trophyIcon.rectTransform.anchoredPosition;
                _floatTween = trophyIcon.rectTransform
                    .DOAnchorPosY(startPos.y + 5f, 2f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }

            // Glow pulse animation
            if (borderGlow && _isUnlocked)
            {
                Color baseColor = borderGlow.color;
                _glowTween = DOTween.To(
                    () => borderGlow.color.a,
                    x => borderGlow.color = new Color(baseColor.r, baseColor.g, baseColor.b, x),
                    0.3f,
                    1.5f
                ).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
            }

            // Shine effect
            if (shineEffect && _isUnlocked)
            {
                StartShineAnimation();
            }
        }

        private void StartShineAnimation()
        {
            if (shineEffect == null) return;

            shineEffect.gameObject.SetActive(true);
            RectTransform shineRT = shineEffect.rectTransform;

            // Reset position
            shineRT.anchoredPosition = new Vector2(-200f, 0f);

            // Animate across the card
            _shineTween = shineRT
                .DOAnchorPosX(200f, 2f)
                .SetEase(Ease.Linear)
                .SetDelay(3f)
                .SetLoops(-1, LoopType.Restart);
        }

        private void StopAllAnimations()
        {
            _floatTween?.Kill();
            _glowTween?.Kill();
            _shineTween?.Kill();
        }

        /// <summary>
        /// Play unlock celebration animation
        /// </summary>
        public void PlayUnlockAnimation(Action onComplete = null)
        {
            StopAllAnimations();

            Sequence seq = DOTween.Sequence();

            // Scale up
            if (cardContainer)
            {
                seq.Append(cardContainer.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack));
            }

            // Flash glow
            if (borderGlow)
            {
                seq.Join(borderGlow.DOColor(Color.white, 0.2f));
                seq.Append(borderGlow.DOColor(unlockedGlowColor, 0.3f));
            }

            // Particles burst
            if (glowParticles)
            {
                seq.AppendCallback(() => glowParticles.Emit(30));
            }

            // Scale back
            if (cardContainer)
            {
                seq.Append(cardContainer.DOScale(1f, 0.2f).SetEase(Ease.OutBounce));
            }

            seq.OnComplete(() =>
            {
                _isUnlocked = true;
                UpdateVisuals();
                StartIdleAnimations();
                onComplete?.Invoke();
            });
        }

        #endregion

        #region Interaction

        public void OnPointerClick(PointerEventData eventData)
        {
            // Click animation
            if (cardContainer)
            {
                cardContainer.DOPunchScale(Vector3.one * 0.05f, 0.2f, 5, 0.5f);
            }

            OnCardClicked?.Invoke(this, _data);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovering = true;

            // Scale up slightly
            if (cardContainer)
            {
                cardContainer.DOScale(1.05f, 0.2f).SetEase(Ease.OutCubic);
            }

            // Brighten glow
            if (borderGlow)
            {
                Color current = borderGlow.color;
                borderGlow.DOColor(new Color(current.r, current.g, current.b, current.a + 0.2f), 0.2f);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovering = false;

            // Scale back
            if (cardContainer)
            {
                cardContainer.DOScale(1f, 0.2f).SetEase(Ease.OutCubic);
            }

            // Reset glow
            UpdateGlowColor();
        }

        #endregion

        #region Public Methods

        public AchievementData GetData() => _data;

        public void SetSelected(bool selected)
        {
            if (selected)
            {
                if (cardContainer)
                {
                    cardContainer.DOScale(1.1f, 0.2f).SetEase(Ease.OutBack);
                }
            }
            else
            {
                if (cardContainer)
                {
                    cardContainer.DOScale(1f, 0.2f).SetEase(Ease.OutCubic);
                }
            }
        }

        public void RefreshProgress(int currentProgress)
        {
            if (_data == null) return;

            _data.currentProgress = currentProgress;
            _isInProgress = !_data.isCompleted && currentProgress > 0;

            if (progressContainer)
            {
                progressContainer.SetActive(_isInProgress);

                if (_isInProgress && progressFill)
                {
                    float progress = (float)currentProgress / _data.targetProgress;
                    progressFill.DOFillAmount(progress, 0.3f);

                    if (progressText)
                    {
                        progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
                    }
                }
            }

            UpdateGlowColor();
        }

        #endregion
    }

    /// <summary>
    /// Data structure for achievements
    /// </summary>
    [Serializable]
    public class AchievementData
    {
        public string id;
        public string title;
        public string description;
        public string category;
        public int points;
        public int targetProgress;
        public int currentProgress;
        public bool isCompleted;
        public bool isClaimed;
        public bool isSecret;
        public Sprite icon;

        public float Progress => targetProgress > 0 ? (float)currentProgress / targetProgress : 0f;
    }
}
