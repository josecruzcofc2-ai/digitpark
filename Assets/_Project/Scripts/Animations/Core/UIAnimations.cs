using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

namespace DigitPark.Animations
{
    /// <summary>
    /// Static utility class for common UI animations using DOTween.
    /// Use these methods throughout the app for consistent animations.
    /// </summary>
    public static class UIAnimations
    {
        // ==================== DURATIONS ====================
        public const float DURATION_INSTANT = 0.1f;
        public const float DURATION_FAST = 0.2f;
        public const float DURATION_NORMAL = 0.3f;
        public const float DURATION_SLOW = 0.5f;
        public const float DURATION_VERY_SLOW = 0.8f;

        // ==================== BUTTON ANIMATIONS ====================

        /// <summary>
        /// Standard button press animation (scale down then up)
        /// </summary>
        public static Tween ButtonPress(Transform target, float scale = 0.9f, float duration = DURATION_FAST)
        {
            return target.DOScale(scale, duration / 2)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => target.DOScale(1f, duration / 2).SetEase(Ease.OutBack));
        }

        /// <summary>
        /// Button bounce effect
        /// </summary>
        public static Tween ButtonBounce(Transform target, float scale = 1.1f, float duration = DURATION_NORMAL)
        {
            return DOTween.Sequence()
                .Append(target.DOScale(scale, duration * 0.4f).SetEase(Ease.OutQuad))
                .Append(target.DOScale(1f, duration * 0.6f).SetEase(Ease.OutBounce));
        }

        /// <summary>
        /// Continuous pulse animation for attention
        /// </summary>
        public static Tween ButtonPulse(Transform target, float minScale = 0.95f, float maxScale = 1.05f, float duration = 0.8f)
        {
            return target.DOScale(maxScale, duration / 2)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        /// <summary>
        /// Button 3D press effect (moves down)
        /// </summary>
        public static Tween Button3DPress(RectTransform target, float pressDepth = 5f, float duration = DURATION_INSTANT)
        {
            Vector2 originalPos = target.anchoredPosition;
            return DOTween.Sequence()
                .Append(target.DOAnchorPosY(originalPos.y - pressDepth, duration).SetEase(Ease.OutQuad))
                .Append(target.DOAnchorPosY(originalPos.y, duration).SetEase(Ease.OutBack));
        }

        // ==================== PANEL ANIMATIONS ====================

        /// <summary>
        /// Slide panel in from direction
        /// </summary>
        public static Tween SlideIn(RectTransform target, Direction direction, float duration = DURATION_NORMAL)
        {
            Vector2 startPos = GetOffscreenPosition(target, direction);
            Vector2 endPos = target.anchoredPosition;

            target.anchoredPosition = startPos;
            target.gameObject.SetActive(true);

            return target.DOAnchorPos(endPos, duration).SetEase(Ease.OutBack);
        }

        /// <summary>
        /// Slide panel out to direction
        /// </summary>
        public static Tween SlideOut(RectTransform target, Direction direction, float duration = DURATION_NORMAL, bool deactivate = true)
        {
            Vector2 endPos = GetOffscreenPosition(target, direction);

            return target.DOAnchorPos(endPos, duration)
                .SetEase(Ease.InBack)
                .OnComplete(() => { if (deactivate) target.gameObject.SetActive(false); });
        }

        /// <summary>
        /// Popup scale animation
        /// </summary>
        public static Tween PopupShow(Transform target, float duration = DURATION_NORMAL)
        {
            target.localScale = Vector3.zero;
            target.gameObject.SetActive(true);
            return target.DOScale(1f, duration).SetEase(Ease.OutBack);
        }

        /// <summary>
        /// Popup hide animation
        /// </summary>
        public static Tween PopupHide(Transform target, float duration = DURATION_FAST, bool deactivate = true)
        {
            return target.DOScale(0f, duration)
                .SetEase(Ease.InBack)
                .OnComplete(() => { if (deactivate) target.gameObject.SetActive(false); });
        }

        /// <summary>
        /// Fade panel in
        /// </summary>
        public static Tween FadeIn(CanvasGroup target, float duration = DURATION_NORMAL)
        {
            target.alpha = 0f;
            target.gameObject.SetActive(true);
            return target.DOFade(1f, duration).SetEase(Ease.OutQuad);
        }

        /// <summary>
        /// Fade panel out
        /// </summary>
        public static Tween FadeOut(CanvasGroup target, float duration = DURATION_NORMAL, bool deactivate = true)
        {
            return target.DOFade(0f, duration)
                .SetEase(Ease.InQuad)
                .OnComplete(() => { if (deactivate) target.gameObject.SetActive(false); });
        }

        // ==================== TEXT ANIMATIONS ====================

        /// <summary>
        /// Animate number counter
        /// </summary>
        public static Tween CounterAnimation(TextMeshProUGUI text, int startValue, int endValue, float duration = DURATION_SLOW, string format = "{0}")
        {
            return DOTween.To(() => startValue, x => {
                startValue = x;
                text.text = string.Format(format, x);
            }, endValue, duration).SetEase(Ease.OutQuad);
        }

        /// <summary>
        /// Typewriter effect for text
        /// </summary>
        public static Tween TypewriterEffect(TextMeshProUGUI text, string fullText, float duration = 1f)
        {
            text.text = "";
            return DOTween.To(() => 0, x => text.text = fullText.Substring(0, x), fullText.Length, duration)
                .SetEase(Ease.Linear);
        }

        /// <summary>
        /// Text punch scale
        /// </summary>
        public static Tween TextPunch(Transform target, float punch = 0.2f, float duration = DURATION_NORMAL)
        {
            return target.DOPunchScale(Vector3.one * punch, duration, 5, 0.5f);
        }

        // ==================== REWARD ANIMATIONS ====================

        /// <summary>
        /// Item flies to target position
        /// </summary>
        public static Tween FlyToTarget(RectTransform item, Vector3 targetPosition, float duration = DURATION_SLOW, Action onComplete = null)
        {
            return DOTween.Sequence()
                .Append(item.DOScale(1.2f, duration * 0.2f).SetEase(Ease.OutQuad))
                .Join(item.DOMove(targetPosition, duration * 0.8f).SetEase(Ease.InBack))
                .Join(item.DOScale(0.5f, duration * 0.8f).SetEase(Ease.InQuad))
                .OnComplete(() => onComplete?.Invoke());
        }

        /// <summary>
        /// Reward claim celebration
        /// </summary>
        public static Tween RewardCelebration(Transform target, float duration = DURATION_NORMAL)
        {
            return DOTween.Sequence()
                .Append(target.DOScale(1.3f, duration * 0.3f).SetEase(Ease.OutQuad))
                .Append(target.DOScale(1f, duration * 0.7f).SetEase(Ease.OutBounce))
                .Join(target.DOPunchRotation(new Vector3(0, 0, 15f), duration * 0.5f, 10, 0.5f));
        }

        // ==================== EFFECT ANIMATIONS ====================

        /// <summary>
        /// Shake effect for errors or impacts
        /// </summary>
        public static Tween Shake(Transform target, float strength = 10f, float duration = DURATION_NORMAL)
        {
            return target.DOShakePosition(duration, strength, 20, 90, false, true);
        }

        /// <summary>
        /// Shake rotation
        /// </summary>
        public static Tween ShakeRotation(Transform target, float strength = 15f, float duration = DURATION_NORMAL)
        {
            return target.DOShakeRotation(duration, new Vector3(0, 0, strength), 20, 90);
        }

        /// <summary>
        /// Glow pulse on image
        /// </summary>
        public static Tween GlowPulse(Image target, Color glowColor, float duration = 0.8f)
        {
            Color originalColor = target.color;
            return target.DOColor(glowColor, duration / 2)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        /// <summary>
        /// Flash effect
        /// </summary>
        public static Tween Flash(Image target, Color flashColor, float duration = DURATION_FAST)
        {
            Color originalColor = target.color;
            return DOTween.Sequence()
                .Append(target.DOColor(flashColor, duration / 2).SetEase(Ease.OutQuad))
                .Append(target.DOColor(originalColor, duration / 2).SetEase(Ease.InQuad));
        }

        // ==================== ENTRANCE ANIMATIONS ====================

        /// <summary>
        /// Staggered entrance for multiple items
        /// </summary>
        public static Sequence StaggeredEntrance(Transform[] items, float staggerDelay = 0.1f, float duration = DURATION_NORMAL)
        {
            Sequence seq = DOTween.Sequence();

            for (int i = 0; i < items.Length; i++)
            {
                items[i].localScale = Vector3.zero;
                seq.Insert(i * staggerDelay, items[i].DOScale(1f, duration).SetEase(Ease.OutBack));
            }

            return seq;
        }

        /// <summary>
        /// Cascade entrance from direction
        /// </summary>
        public static Sequence CascadeEntrance(RectTransform[] items, Direction direction, float staggerDelay = 0.08f, float duration = DURATION_NORMAL)
        {
            Sequence seq = DOTween.Sequence();

            for (int i = 0; i < items.Length; i++)
            {
                Vector2 originalPos = items[i].anchoredPosition;
                Vector2 startPos = GetOffscreenPosition(items[i], direction);
                items[i].anchoredPosition = startPos;

                seq.Insert(i * staggerDelay, items[i].DOAnchorPos(originalPos, duration).SetEase(Ease.OutBack));
            }

            return seq;
        }

        // ==================== SPECIAL ANIMATIONS ====================

        /// <summary>
        /// Spin animation
        /// </summary>
        public static Tween Spin(Transform target, float rotations = 1f, float duration = DURATION_SLOW)
        {
            return target.DORotate(new Vector3(0, 0, 360f * rotations), duration, RotateMode.FastBeyond360)
                .SetEase(Ease.InOutQuad);
        }

        /// <summary>
        /// Float up and down continuously
        /// </summary>
        public static Tween Float(Transform target, float distance = 10f, float duration = 1.5f)
        {
            return target.DOMoveY(target.position.y + distance, duration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        /// <summary>
        /// Breathing scale effect
        /// </summary>
        public static Tween Breathe(Transform target, float minScale = 0.98f, float maxScale = 1.02f, float duration = 2f)
        {
            return target.DOScale(maxScale, duration / 2)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        // ==================== COMBO ANIMATIONS ====================

        /// <summary>
        /// Combo text animation (scale up with punch)
        /// </summary>
        public static Tween ComboText(Transform target, int comboLevel)
        {
            float scale = 1f + (comboLevel * 0.1f);
            float shake = comboLevel * 2f;

            return DOTween.Sequence()
                .Append(target.DOScale(scale, DURATION_FAST).SetEase(Ease.OutQuad))
                .Join(target.DOShakeRotation(DURATION_FAST, new Vector3(0, 0, shake)))
                .Append(target.DOScale(1f, DURATION_NORMAL).SetEase(Ease.OutBounce));
        }

        /// <summary>
        /// Ultra combo effect (screen shake + zoom)
        /// </summary>
        public static Sequence UltraCombo(Transform screenTransform, Transform comboText)
        {
            return DOTween.Sequence()
                .Append(screenTransform.DOShakePosition(0.5f, 20f, 30))
                .Join(comboText.DOScale(2f, 0.2f).SetEase(Ease.OutQuad))
                .Append(comboText.DOScale(1.5f, 0.3f).SetEase(Ease.OutBounce));
        }

        // ==================== HELPERS ====================

        private static Vector2 GetOffscreenPosition(RectTransform target, Direction direction)
        {
            Vector2 pos = target.anchoredPosition;
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            switch (direction)
            {
                case Direction.Left:
                    return new Vector2(-screenWidth, pos.y);
                case Direction.Right:
                    return new Vector2(screenWidth, pos.y);
                case Direction.Up:
                    return new Vector2(pos.x, screenHeight);
                case Direction.Down:
                    return new Vector2(pos.x, -screenHeight);
                default:
                    return pos;
            }
        }

        /// <summary>
        /// Kill all tweens on target
        /// </summary>
        public static void KillTweens(Transform target)
        {
            target.DOKill();
        }

        /// <summary>
        /// Kill all tweens on target and reset scale
        /// </summary>
        public static void ResetAndKill(Transform target)
        {
            target.DOKill();
            target.localScale = Vector3.one;
            target.localRotation = Quaternion.identity;
        }
    }

    public enum Direction
    {
        Left,
        Right,
        Up,
        Down
    }
}
