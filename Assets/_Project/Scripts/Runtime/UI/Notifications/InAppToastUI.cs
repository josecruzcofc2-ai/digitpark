using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Collections.Generic;
using DG.Tweening;
using DigitPark.Animations;
using DigitPark.Managers;
using DigitPark.Services;
using DigitPark.Localization;

namespace DigitPark.UI
{
    /// <summary>
    /// Componente visual del toast de notificación in-app.
    /// Slide desde arriba, auto-dismiss, swipe-to-dismiss, botones de acción.
    /// Colores por categoría: Social (azul), Games (naranja), Rewards (dorado), System (cyan).
    /// </summary>
    public class InAppToastUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Container")]
        [SerializeField] private RectTransform toastContainer;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Background")]
        [SerializeField] private Image background;
        [SerializeField] private Outline borderOutline;

        [Header("Icon")]
        [SerializeField] private Image typeIconImage;

        [Header("Text")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI bodyText;

        [Header("Buttons")]
        [SerializeField] private Button cardButton;
        [SerializeField] private Button dismissButton;
        [SerializeField] private Button primaryButton;
        [SerializeField] private TextMeshProUGUI primaryButtonText;
        [SerializeField] private Image primaryButtonBg;
        [SerializeField] private Button secondaryButton;
        [SerializeField] private TextMeshProUGUI secondaryButtonText;

        [Header("Timing")]
        [SerializeField] private float slideInDuration = AnimConstants.TOAST_SLIDE_IN;
        [SerializeField] private float displayDuration = AnimConstants.TOAST_DISPLAY;
        [SerializeField] private float slideOutDuration = AnimConstants.TOAST_SLIDE_OUT;

        // Category colors
        private static readonly Color SOCIAL_COLOR = new Color(0.4f, 0.6f, 1f, 1f);
        private static readonly Color GAMES_COLOR = new Color(1f, 0.5f, 0f, 1f);
        private static readonly Color REWARDS_COLOR = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color SYSTEM_COLOR = new Color(0f, 1f, 1f, 1f);
        private static readonly Color BG_COLOR = new Color(0.04f, 0.06f, 0.12f, 0.96f);

        // Events
        public event Action OnToastDismissed;
        public event Action<InAppToastData, string> OnToastAction;

        // State
        private bool _isShowing;
        public bool IsShowing => _isShowing;
        private InAppToastData _currentData;
        private Sequence _displaySequence;
        private Tween _currentTween;
        private Vector2 _hiddenPosition;
        private Vector2 _shownPosition;

        // Queue
        private Queue<InAppToastData> _pendingToasts = new Queue<InAppToastData>();

        // Swipe
        private Vector2 _swipeStartPos;
        private bool _isSwiping;

        #region Unity Lifecycle

        private void Awake()
        {
            if (toastContainer != null)
            {
                _shownPosition = toastContainer.anchoredPosition;
                _hiddenPosition = new Vector2(_shownPosition.x, _shownPosition.y + 250f);
                toastContainer.anchoredPosition = _hiddenPosition;
            }

            if (canvasGroup != null)
                canvasGroup.alpha = 0f;

            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _currentTween?.Kill();
            _displaySequence?.Kill();
            // Kill any orphaned snap-back or border tweens
            if (toastContainer != null) toastContainer.DOKill();
            if (canvasGroup != null) canvasGroup.DOKill();
            if (borderOutline != null) DOTween.Kill(borderOutline);
        }

        #endregion

        #region Show / Dismiss

        public void Show(InAppToastData data)
        {
            if (_isShowing)
            {
                _pendingToasts.Enqueue(data);
                return;
            }

            _isShowing = true;
            _currentData = data;

            UpdateDisplay(data);
            SetupButtons(data);
            gameObject.SetActive(true);
            PlayShowAnimation();
        }

        public void Dismiss()
        {
            if (!_isShowing) return;
            PlayHideAnimation();
        }

        public void DismissImmediate()
        {
            _currentTween?.Kill();
            _displaySequence?.Kill();

            if (toastContainer != null)
                toastContainer.anchoredPosition = _hiddenPosition;
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;

            gameObject.SetActive(false);
            _isShowing = false;
            ShowNextQueued();
        }

        private void ShowNextQueued()
        {
            if (_pendingToasts.Count > 0)
            {
                var next = _pendingToasts.Dequeue();
                _isShowing = false;
                Show(next);
            }
        }

        #endregion

        #region Display

        private void UpdateDisplay(InAppToastData data)
        {
            Color accentColor = GetCategoryColor(data.category);

            // Type icon
            if (typeIconImage != null)
            {
                typeIconImage.sprite = GetTypeIconSprite(data.type);
                typeIconImage.color = accentColor;
            }

            // Title
            if (titleText != null)
            {
                titleText.text = data.title;
                titleText.color = new Color(0.95f, 0.95f, 0.95f, 1f);
            }

            // Body
            if (bodyText != null)
            {
                bodyText.text = data.body;
            }

            // Border color por categoría
            if (borderOutline != null)
            {
                borderOutline.effectColor = new Color(accentColor.r, accentColor.g, accentColor.b, 0.6f);
            }

            // Background
            if (background != null)
            {
                background.color = BG_COLOR;
            }
        }

        private void SetupButtons(InAppToastData data)
        {
            // Clear previous listeners
            primaryButton?.onClick.RemoveAllListeners();
            secondaryButton?.onClick.RemoveAllListeners();
            dismissButton?.onClick.RemoveAllListeners();
            cardButton?.onClick.RemoveAllListeners();

            // Dismiss button
            dismissButton?.onClick.AddListener(() =>
            {
                Dismiss();
            });

            // Actions based on type
            bool showPrimary = false;
            bool showSecondary = false;
            string primaryLabel = "";
            string secondaryLabel = "";

            switch (data.type)
            {
                case "friend_request":
                    primaryLabel = AutoLocalizer.Get("toast_accept");
                    secondaryLabel = "✕";
                    showPrimary = true;
                    showSecondary = true;
                    break;
                case "challenge":
                    primaryLabel = AutoLocalizer.Get("toast_accept");
                    secondaryLabel = "✕";
                    showPrimary = true;
                    showSecondary = true;
                    break;
                case "tournament_start":
                    primaryLabel = AutoLocalizer.Get("toast_join");
                    showPrimary = true;
                    break;
                case "daily_reward":
                    primaryLabel = AutoLocalizer.Get("toast_claim");
                    showPrimary = true;
                    break;
                case "promo":
                    primaryLabel = AutoLocalizer.Get("toast_view");
                    showPrimary = true;
                    break;
            }

            // Show/hide buttons
            if (primaryButton != null)
            {
                primaryButton.gameObject.SetActive(showPrimary);
                if (showPrimary)
                {
                    if (primaryButtonText != null) primaryButtonText.text = primaryLabel;
                    if (primaryButtonBg != null) primaryButtonBg.color = GetCategoryColor(data.category);
                    primaryButton.onClick.AddListener(() =>
                    {
                        OnToastAction?.Invoke(data, "primary");
                        DismissImmediate();
                    });
                }
            }

            if (secondaryButton != null)
            {
                secondaryButton.gameObject.SetActive(showSecondary);
                if (showSecondary)
                {
                    if (secondaryButtonText != null) secondaryButtonText.text = secondaryLabel;
                    secondaryButton.onClick.AddListener(() =>
                    {
                        OnToastAction?.Invoke(data, "secondary");
                        Dismiss();
                    });
                }
            }
        }

        #endregion

        #region Animations

        private void PlayShowAnimation()
        {
            _displaySequence?.Kill();
            _displaySequence = DOTween.Sequence();

            // Reset
            if (toastContainer != null)
            {
                toastContainer.anchoredPosition = _hiddenPosition;
                toastContainer.localScale = new Vector3(0.95f, 0.95f, 1f);
            }
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;

            // Slide in
            if (toastContainer != null)
            {
                _displaySequence.Append(
                    toastContainer.DOAnchorPos(_shownPosition, AccessibilityHelper.AnimDuration(slideInDuration))
                        .SetEase(AccessibilityHelper.AnimEase(AnimConstants.ENTER), 1.1f));

                _displaySequence.Join(
                    toastContainer.DOScale(1f, AccessibilityHelper.AnimDuration(slideInDuration * 0.7f))
                        .SetEase(AccessibilityHelper.AnimEase(AnimConstants.SMOOTH)));
            }

            // Fade in
            if (canvasGroup != null)
            {
                _displaySequence.Join(
                    canvasGroup.DOFade(1f, slideInDuration * 0.5f));
            }

            // Border glow pulse
            if (borderOutline != null)
            {
                Color original = borderOutline.effectColor;
                Color bright = new Color(original.r, original.g, original.b, 1f);
                _displaySequence.Append(DOTween.To(
                    () => borderOutline.effectColor,
                    x => borderOutline.effectColor = x,
                    bright, 0.15f));
                _displaySequence.Append(DOTween.To(
                    () => borderOutline.effectColor,
                    x => borderOutline.effectColor = x,
                    original, 0.2f));
            }

            // Wait, then auto-dismiss
            _displaySequence.AppendInterval(displayDuration);
            _displaySequence.AppendCallback(() => PlayHideAnimation());
        }

        private void PlayHideAnimation()
        {
            _displaySequence?.Kill();
            _currentTween?.Kill();

            Sequence hideSeq = DOTween.Sequence();

            if (toastContainer != null)
            {
                hideSeq.Append(
                    toastContainer.DOAnchorPos(_hiddenPosition, AccessibilityHelper.AnimDuration(slideOutDuration))
                        .SetEase(AccessibilityHelper.AnimEase(AnimConstants.EXIT)));
            }

            if (canvasGroup != null)
            {
                hideSeq.Join(
                    canvasGroup.DOFade(0f, slideOutDuration));
            }

            if (toastContainer != null)
            {
                hideSeq.Join(
                    toastContainer.DOScale(0.95f, slideOutDuration));
            }

            hideSeq.OnComplete(() =>
            {
                gameObject.SetActive(false);
                _isShowing = false;
                OnToastDismissed?.Invoke();
                ShowNextQueued();
            });

            _currentTween = hideSeq;
        }

        #endregion

        #region Interaction

        public void OnPointerClick(PointerEventData eventData)
        {
            // Only handle tap if not swiping and not clicking a button
            if (_isSwiping) return;

            OnToastAction?.Invoke(_currentData, "tap");
            Dismiss();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _swipeStartPos = eventData.position;
            _isSwiping = true;
            _displaySequence?.Kill(); // Stop auto-dismiss while swiping
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

                if (canvasGroup != null)
                    canvasGroup.alpha = Mathf.Clamp01(1f - (deltaY / 150f));
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isSwiping) return;
            _isSwiping = false;

            float deltaY = eventData.position.y - _swipeStartPos.y;

            if (deltaY > 80f)
            {
                // Swiped enough — dismiss
                PlayHideAnimation();
            }
            else
            {
                // Snap back and restart auto-dismiss
                if (toastContainer != null)
                {
                    toastContainer.DOKill();
                    toastContainer.DOAnchorPos(_shownPosition, 0.2f).SetEase(Ease.OutCubic).SetLink(gameObject);
                }
                if (canvasGroup != null)
                {
                    canvasGroup.DOKill();
                    canvasGroup.DOFade(1f, 0.2f).SetLink(gameObject);
                }

                // Restart auto-dismiss timer
                _displaySequence?.Kill();
                _displaySequence = DOTween.Sequence();
                _displaySequence.AppendInterval(displayDuration);
                _displaySequence.AppendCallback(() => PlayHideAnimation());
            }
        }

        #endregion

        #region Helpers

        private Color GetCategoryColor(NotificationCategory category)
        {
            switch (category)
            {
                case NotificationCategory.Social: return SOCIAL_COLOR;
                case NotificationCategory.Games: return GAMES_COLOR;
                case NotificationCategory.Rewards: return REWARDS_COLOR;
                default: return SYSTEM_COLOR;
            }
        }

        private Sprite GetTypeIconSprite(string type)
        {
            string iconName = type switch
            {
                "friend_request" => "Icons/ProfileIcon",
                "friend_accepted" => "Icons/icon_handshake",
                "challenge" => "Icons/icon_trophy",
                "tournament_start" => "Icons/icon_trophy",
                "tournament_result" => "Icons/icon_medal",
                "daily_reward" => "Icons/icon_gift",
                "promo" => "Icons/stat_earnings",
                _ => "Icons/NotificationsIcon"
            };
            return Resources.Load<Sprite>(iconName);
        }

        #endregion
    }
}
