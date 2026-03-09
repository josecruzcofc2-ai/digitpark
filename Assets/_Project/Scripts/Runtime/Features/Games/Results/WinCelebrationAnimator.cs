using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using DigitPark.Localization;
using DigitPark.UI;

namespace DigitPark.Animations
{
    /// <summary>
    /// Cinematic win celebration sequence for WinPanels.
    /// Creates its own overlay effects at runtime.
    ///
    /// Phases:
    ///   1. Screen flash white (0-0.3s)
    ///   2. Result icon pop-in with overshoot (0.3-0.8s)
    ///   3. Confetti rain from top (0.8s+, continuous)
    ///   4. NEW HIGH SCORE shockwave (optional)
    ///
    /// Usage:
    ///   WinCelebrationAnimator.PlayWin(canvas, resultIcon);
    ///   WinCelebrationAnimator.PlayLose(canvas);
    ///   WinCelebrationAnimator.PlayNewHighScore(canvas);
    /// </summary>
    public class WinCelebrationAnimator : MonoBehaviour
    {
        private static readonly Color FLASH_WIN = new Color(1f, 1f, 1f, 0.8f);
        private static readonly Color FLASH_LOSE = new Color(1f, 0.2f, 0.2f, 0.4f);
        private static readonly Color CONFETTI_GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color CONFETTI_CYAN = new Color(0f, 0.9f, 1f, 1f);
        private static readonly Color HIGH_SCORE_COLOR = new Color(1f, 0.84f, 0f, 1f);

        private Canvas parentCanvas;
        private RectTransform overlayRT;
        private Image flashImage;
        private readonly List<Tween> _activeTweens = new List<Tween>();
        private readonly List<GameObject> _confettiPieces = new List<GameObject>();
        private Coroutine _confettiCoroutine;

        /// <summary>
        /// Play win celebration on the given canvas.
        /// Optionally animate a result icon with pop-in.
        /// </summary>
        public static WinCelebrationAnimator PlayWin(Canvas canvas, Transform resultIconTransform = null)
        {
            if (canvas == null) return null;

            var go = new GameObject("WinCelebration");
            go.transform.SetParent(canvas.transform, false);
            go.transform.SetAsLastSibling();

            var animator = go.AddComponent<WinCelebrationAnimator>();
            animator.parentCanvas = canvas;
            animator.Setup();
            animator.StartCoroutine(animator.WinSequence(resultIconTransform));
            return animator;
        }

        /// <summary>
        /// Play lose effect (subtle red flash, no confetti).
        /// </summary>
        public static WinCelebrationAnimator PlayLose(Canvas canvas)
        {
            if (canvas == null) return null;

            var go = new GameObject("LoseCelebration");
            go.transform.SetParent(canvas.transform, false);
            go.transform.SetAsLastSibling();

            var animator = go.AddComponent<WinCelebrationAnimator>();
            animator.parentCanvas = canvas;
            animator.Setup();
            animator.StartCoroutine(animator.LoseSequence());
            return animator;
        }

        /// <summary>
        /// Play NEW HIGH SCORE effect (shockwave + golden text).
        /// Call after the win sequence.
        /// </summary>
        public void PlayNewHighScore()
        {
            StartCoroutine(NewHighScoreSequence());
        }

        private void Setup()
        {
            overlayRT = gameObject.AddComponent<RectTransform>();
            overlayRT.anchorMin = Vector2.zero;
            overlayRT.anchorMax = Vector2.one;
            overlayRT.offsetMin = Vector2.zero;
            overlayRT.offsetMax = Vector2.zero;

            var cg = gameObject.AddComponent<CanvasGroup>();
            cg.interactable = false;
            cg.blocksRaycasts = false;

            // Flash overlay
            var flashGO = new GameObject("Flash");
            flashGO.transform.SetParent(overlayRT, false);
            var flashRT = flashGO.AddComponent<RectTransform>();
            flashRT.anchorMin = Vector2.zero;
            flashRT.anchorMax = Vector2.one;
            flashRT.offsetMin = Vector2.zero;
            flashRT.offsetMax = Vector2.zero;

            flashImage = flashGO.AddComponent<Image>();
            flashImage.color = Color.clear;
            flashImage.raycastTarget = false;
        }

        // ==================== WIN SEQUENCE ====================

        private IEnumerator WinSequence(Transform resultIconTransform)
        {
            // Phase 1: White flash
            flashImage.color = FLASH_WIN;
            var fadeOut = flashImage.DOFade(0f, AccessibilityHelper.AnimDuration(0.4f)).SetEase(AccessibilityHelper.AnimEase(AnimConstants.SMOOTH));
            TrackTween(fadeOut);

            // Screen shake
            if (UIAnimationManager.Instance != null)
                UIAnimationManager.Instance.ScreenShake(15f, 0.25f);

            yield return new WaitForSeconds(0.15f);

            // Phase 2: Result icon pop-in
            if (resultIconTransform != null)
            {
                resultIconTransform.localScale = Vector3.zero;
                var popIn = DOTween.Sequence();
                popIn.Append(resultIconTransform.DOScale(AnimConstants.SCALE_GO_POP, AccessibilityHelper.AnimDuration(0.3f)).SetEase(AccessibilityHelper.AnimEase(AnimConstants.ENTER), 3f));
                popIn.Append(resultIconTransform.DOScale(1f, AccessibilityHelper.AnimDuration(AnimConstants.DURATION_QUICK)).SetEase(AccessibilityHelper.AnimEase(AnimConstants.SMOOTH)));
                TrackTween(popIn);
            }

            yield return new WaitForSeconds(0.5f);

            // Phase 3: Start confetti rain
            _confettiCoroutine = StartCoroutine(ConfettiRain());

            // Phase 4: Sparkle flash
            if (UIAnimationManager.Instance != null)
                UIAnimationManager.Instance.GoldFlash();
        }

        // ==================== LOSE SEQUENCE ====================

        private IEnumerator LoseSequence()
        {
            // Subtle red flash
            flashImage.color = FLASH_LOSE;
            var loseFade = flashImage.DOFade(0f, AccessibilityHelper.AnimDuration(0.5f)).SetEase(AccessibilityHelper.AnimEase(AnimConstants.SMOOTH));
            TrackTween(loseFade);

            yield return new WaitForSeconds(0.5f);

            // Auto-cleanup after effect
            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
        }

        // ==================== CONFETTI RAIN ====================

        private IEnumerator ConfettiRain()
        {
            float duration = 3f;
            float elapsed = 0f;
            float spawnInterval = 0.05f;
            float nextSpawn = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                if (elapsed >= nextSpawn)
                {
                    SpawnConfettiPiece();
                    nextSpawn += spawnInterval;

                    // Slow down spawning over time
                    spawnInterval = Mathf.Lerp(0.05f, 0.2f, elapsed / duration);
                }

                yield return null;
            }

            // Wait for last pieces to fall
            yield return new WaitForSeconds(2f);

            // Cleanup
            foreach (var piece in _confettiPieces)
            {
                if (piece != null)
                    Destroy(piece);
            }
            _confettiPieces.Clear();

            // Auto-destroy celebration overlay
            yield return new WaitForSeconds(0.5f);
            Destroy(gameObject);
        }

        private void SpawnConfettiPiece()
        {
            var piece = new GameObject("Confetti");
            piece.transform.SetParent(overlayRT, false);

            var rt = piece.AddComponent<RectTransform>();

            // Random position at top of screen
            float screenWidth = overlayRT.rect.width;
            float startX = UnityEngine.Random.Range(-screenWidth * 0.5f, screenWidth * 0.5f);
            float startY = overlayRT.rect.height * 0.5f + 20f;
            rt.anchoredPosition = new Vector2(startX, startY);

            // Random size
            float size = UnityEngine.Random.Range(8f, 18f);
            rt.sizeDelta = new Vector2(size, size * UnityEngine.Random.Range(0.5f, 1.5f));

            var img = piece.AddComponent<Image>();

            // Random color from palette
            Color[] palette = {
                CONFETTI_GOLD,
                CONFETTI_CYAN,
                new Color(1f, 0.5f, 0.2f, 1f),  // Orange
                new Color(0.6f, 0.3f, 1f, 1f),   // Purple
                new Color(0.2f, 1f, 0.4f, 1f),   // Green
                Color.white
            };
            img.color = palette[UnityEngine.Random.Range(0, palette.Length)];
            img.raycastTarget = false;

            // Random rotation
            rt.rotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(0f, 360f));

            // Animate: fall down with wobble
            float fallDuration = UnityEngine.Random.Range(1.5f, 3f);
            float endY = -overlayRT.rect.height * 0.5f - 50f;
            float wobbleAmplitude = UnityEngine.Random.Range(30f, 80f);

            var seq = DOTween.Sequence();

            // Fall
            seq.Append(rt.DOAnchorPosY(endY, fallDuration).SetEase(Ease.InQuad));

            // Horizontal wobble
            seq.Join(rt.DOAnchorPosX(startX + wobbleAmplitude, fallDuration)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo));

            // Rotate
            seq.Join(rt.DORotate(new Vector3(0, 0, UnityEngine.Random.Range(-720f, 720f)),
                fallDuration, RotateMode.FastBeyond360));

            // Fade out near bottom
            seq.Insert(fallDuration * 0.7f, img.DOFade(0f, fallDuration * 0.3f));

            seq.OnComplete(() =>
            {
                if (piece != null)
                    Destroy(piece);
            });

            TrackTween(seq);
            _confettiPieces.Add(piece);
        }

        // ==================== NEW HIGH SCORE ====================

        private IEnumerator NewHighScoreSequence()
        {
            // Create "NEW HIGH SCORE!" text
            var textGO = new GameObject("HighScoreText");
            textGO.transform.SetParent(overlayRT, false);

            var textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0.5f, 0.7f);
            textRT.anchorMax = new Vector2(0.5f, 0.7f);
            textRT.sizeDelta = new Vector2(500, 80);

            var text = textGO.AddComponent<TextMeshProUGUI>();
            text.text = AutoLocalizer.Get("new_high_score");
            text.fontSize = 52;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = HIGH_SCORE_COLOR;
            text.alpha = 0f;
            textRT.localScale = Vector3.one * 0.3f;

            // Pop in
            var seq = DOTween.Sequence();
            seq.Append(text.DOFade(1f, 0.1f));
            seq.Join(textRT.DOScale(AnimConstants.SCALE_GO_POP, AccessibilityHelper.AnimDuration(AnimConstants.DURATION_MEDIUM)).SetEase(AccessibilityHelper.AnimEase(AnimConstants.ENTER), 3f));
            seq.Append(textRT.DOScale(1f, 0.1f));

            // Screen shake
            if (UIAnimationManager.Instance != null)
                UIAnimationManager.Instance.ScreenShake(20f, 0.3f);

            // Gold flash
            if (UIAnimationManager.Instance != null)
                UIAnimationManager.Instance.GoldFlash();

            // Shockwave ring
            var ringGO = new GameObject("HighScoreRing");
            ringGO.transform.SetParent(overlayRT, false);
            var ringRT = ringGO.AddComponent<RectTransform>();
            ringRT.anchorMin = new Vector2(0.5f, 0.7f);
            ringRT.anchorMax = new Vector2(0.5f, 0.7f);
            ringRT.sizeDelta = new Vector2(80, 80);

            var ringImg = ringGO.AddComponent<Image>();
            ringImg.color = new Color(HIGH_SCORE_COLOR.r, HIGH_SCORE_COLOR.g, HIGH_SCORE_COLOR.b, 0.5f);
            ringImg.raycastTarget = false;

            var ringSeq = DOTween.Sequence();
            ringSeq.Append(ringRT.DOScale(12f, AccessibilityHelper.AnimDuration(0.5f)).SetEase(AccessibilityHelper.AnimEase(AnimConstants.SMOOTH)));
            ringSeq.Join(ringImg.DOFade(0f, 0.5f));
            ringSeq.OnComplete(() => Destroy(ringGO));
            TrackTween(ringSeq);

            // Pulse the text
            seq.AppendInterval(0.3f);
            seq.Append(textRT.DOPunchScale(Vector3.one * 0.15f, 0.4f, 4, 0.5f));

            // Hold then fade
            seq.AppendInterval(1.5f);
            seq.Append(text.DOFade(0f, 0.5f));
            seq.OnComplete(() => Destroy(textGO));

            TrackTween(seq);

            yield return new WaitForSeconds(3f);
        }

        // ==================== TWEEN MANAGEMENT ====================

        private void TrackTween(Tween tween)
        {
            if (tween == null) return;
            _activeTweens.Add(tween);
            tween.OnKill(() => _activeTweens.Remove(tween));
        }

        private void OnDestroy()
        {
            if (_confettiCoroutine != null)
                StopCoroutine(_confettiCoroutine);

            foreach (var t in _activeTweens)
            {
                t?.Kill();
            }
            _activeTweens.Clear();

            foreach (var piece in _confettiPieces)
            {
                if (piece != null)
                    Destroy(piece);
            }
            _confettiPieces.Clear();

            if (flashImage != null) flashImage.DOKill();
        }
    }
}
