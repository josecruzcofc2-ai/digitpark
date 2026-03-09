using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;
using System.Collections;
using DigitPark.Localization;
using DigitPark.UI;

namespace DigitPark.Animations
{
    /// <summary>
    /// Cinematic 3-2-1-GO! countdown for minigames and competitive matches.
    /// Creates its own fullscreen overlay UI at runtime.
    /// Usage: CountdownAnimator.Play(parentCanvas, onComplete)
    /// </summary>
    public class CountdownAnimator : MonoBehaviour
    {
        // Colors per number: 3=red, 2=yellow, 1=green, GO=white+glow
        private static readonly Color COLOR_3 = new Color(1f, 0.3f, 0.3f, 1f);
        private static readonly Color COLOR_2 = new Color(1f, 0.85f, 0.2f, 1f);
        private static readonly Color COLOR_1 = new Color(0.2f, 1f, 0.4f, 1f);
        private static readonly Color COLOR_GO = new Color(1f, 1f, 1f, 1f);
        private static readonly Color COLOR_GO_GLOW = new Color(0f, 1f, 1f, 0.6f);
        private static readonly Color OVERLAY_COLOR = new Color(0f, 0f, 0f, AnimConstants.COUNTDOWN_OVERLAY_ALPHA);

        private RectTransform overlayRT;
        private Image overlayImage;
        private TextMeshProUGUI countdownText;
        private Image shockwaveImage;
        private CanvasGroup canvasGroup;
        private Sequence _activeSequence;

        /// <summary>
        /// Creates and plays a cinematic countdown on the given canvas.
        /// Destroys itself when complete.
        /// </summary>
        public static void Play(Canvas parentCanvas, Action onComplete = null)
        {
            if (parentCanvas == null)
            {
                onComplete?.Invoke();
                return;
            }

            var go = new GameObject("CountdownOverlay");
            go.transform.SetParent(parentCanvas.transform, false);
            go.transform.SetAsLastSibling();

            var animator = go.AddComponent<CountdownAnimator>();
            animator.Setup();
            animator.StartCoroutine(animator.CountdownSequence(onComplete));
        }

        private void Setup()
        {
            // Fullscreen overlay
            overlayRT = gameObject.AddComponent<RectTransform>();
            overlayRT.anchorMin = Vector2.zero;
            overlayRT.anchorMax = Vector2.one;
            overlayRT.offsetMin = Vector2.zero;
            overlayRT.offsetMax = Vector2.zero;

            overlayImage = gameObject.AddComponent<Image>();
            overlayImage.color = OVERLAY_COLOR;
            overlayImage.raycastTarget = true;

            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;

            // Shockwave ring (for GO!)
            var shockwaveGO = new GameObject("Shockwave");
            shockwaveGO.transform.SetParent(transform, false);
            var swRT = shockwaveGO.AddComponent<RectTransform>();
            swRT.anchorMin = new Vector2(0.5f, 0.5f);
            swRT.anchorMax = new Vector2(0.5f, 0.5f);
            swRT.sizeDelta = new Vector2(100, 100);
            shockwaveImage = shockwaveGO.AddComponent<Image>();
            shockwaveImage.color = new Color(COLOR_GO_GLOW.r, COLOR_GO_GLOW.g, COLOR_GO_GLOW.b, 0f);
            shockwaveGO.SetActive(false);

            // Countdown text (centered)
            var textGO = new GameObject("CountdownText");
            textGO.transform.SetParent(transform, false);
            var textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0.5f, 0.5f);
            textRT.anchorMax = new Vector2(0.5f, 0.5f);
            textRT.sizeDelta = new Vector2(400, 400);

            countdownText = textGO.AddComponent<TextMeshProUGUI>();
            countdownText.text = "";
            countdownText.fontSize = 180;
            countdownText.fontStyle = FontStyles.Bold;
            countdownText.alignment = TextAlignmentOptions.Center;
            countdownText.enableAutoSizing = false;
            countdownText.color = Color.white;
        }

        private IEnumerator CountdownSequence(Action onComplete)
        {
            // Fade in overlay
            overlayImage.color = new Color(0, 0, 0, 0);
            overlayImage.DOFade(OVERLAY_COLOR.a, AccessibilityHelper.AnimDuration(AnimConstants.COUNTDOWN_FADE_IN));
            yield return new WaitForSeconds(0.25f);

            // 3
            yield return ShowNumber("3", COLOR_3);
            // 2
            yield return ShowNumber("2", COLOR_2);
            // 1
            yield return ShowNumber("1", COLOR_1);
            // GO!
            yield return ShowGO();

            // Fade out and destroy
            canvasGroup.DOFade(0f, 0.2f).OnComplete(() =>
            {
                onComplete?.Invoke();
                Destroy(gameObject);
            });
        }

        private IEnumerator ShowNumber(string number, Color color)
        {
            countdownText.text = number;
            countdownText.color = color;
            countdownText.transform.localScale = Vector3.one * AnimConstants.SCALE_COUNTDOWN_START;
            countdownText.alpha = 0f;

            // Zoom in + fade in
            _activeSequence?.Kill();
            _activeSequence = DOTween.Sequence();
            _activeSequence.Append(countdownText.transform.DOScale(1f, AccessibilityHelper.AnimDuration(AnimConstants.COUNTDOWN_NUMBER_IN)).SetEase(AccessibilityHelper.AnimEase(Ease.OutQuad)));
            _activeSequence.Join(countdownText.DOFade(1f, AccessibilityHelper.AnimDuration(AnimConstants.DURATION_QUICK)));

            // Screen shake on impact
            if (UIAnimationManager.Instance != null)
                UIAnimationManager.Instance.LightScreenShake();

            yield return new WaitForSeconds(AnimConstants.COUNTDOWN_NUMBER_HOLD);

            // Explode out
            _activeSequence?.Kill();
            _activeSequence = DOTween.Sequence();
            _activeSequence.Append(countdownText.transform.DOScale(0.5f, AccessibilityHelper.AnimDuration(AnimConstants.COUNTDOWN_NUMBER_OUT)).SetEase(AccessibilityHelper.AnimEase(AnimConstants.EXIT)));
            _activeSequence.Join(countdownText.DOFade(0f, AccessibilityHelper.AnimDuration(AnimConstants.COUNTDOWN_FADE_IN)));

            yield return new WaitForSeconds(0.3f);
        }

        private IEnumerator ShowGO()
        {
            countdownText.text = AutoLocalizer.Get("countdown_go");
            countdownText.color = COLOR_GO;
            countdownText.transform.localScale = Vector3.zero;
            countdownText.alpha = 1f;

            // Pop in with overshoot
            countdownText.transform.DOScale(AnimConstants.SCALE_GO_POP, AccessibilityHelper.AnimDuration(AnimConstants.COUNTDOWN_GO_POP)).SetEase(AccessibilityHelper.AnimEase(AnimConstants.ENTER));

            yield return new WaitForSeconds(0.2f);

            // Shockwave expand from center
            shockwaveImage.gameObject.SetActive(true);
            shockwaveImage.color = new Color(COLOR_GO_GLOW.r, COLOR_GO_GLOW.g, COLOR_GO_GLOW.b, 0.6f);
            shockwaveImage.rectTransform.localScale = Vector3.one;
            shockwaveImage.rectTransform.DOScale(15f, AccessibilityHelper.AnimDuration(AnimConstants.COUNTDOWN_SHOCKWAVE)).SetEase(AccessibilityHelper.AnimEase(AnimConstants.SMOOTH));
            shockwaveImage.DOFade(0f, AccessibilityHelper.AnimDuration(AnimConstants.COUNTDOWN_SHOCKWAVE));

            // Screen shake
            if (UIAnimationManager.Instance != null)
                UIAnimationManager.Instance.ScreenShake(20f, 0.3f);

            // Settle text
            countdownText.transform.DOScale(1f, AccessibilityHelper.AnimDuration(AnimConstants.DURATION_QUICK)).SetEase(AccessibilityHelper.AnimEase(AnimConstants.SMOOTH));

            yield return new WaitForSeconds(0.5f);

            // Fade text out
            countdownText.DOFade(0f, AccessibilityHelper.AnimDuration(AnimConstants.COUNTDOWN_FADE_IN));
            yield return new WaitForSeconds(0.25f);
        }

        private void OnDestroy()
        {
            _activeSequence?.Kill();
            DOTween.Kill(countdownText?.transform);
            DOTween.Kill(shockwaveImage?.rectTransform);
            DOTween.Kill(overlayImage);
            DOTween.Kill(canvasGroup);
        }
    }
}
