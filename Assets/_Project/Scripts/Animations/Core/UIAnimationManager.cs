using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;

namespace DigitPark.Animations
{
    /// <summary>
    /// Central manager for UI animations throughout the app.
    /// Singleton that provides easy access to common animation patterns.
    /// </summary>
    public class UIAnimationManager : MonoBehaviour
    {
        public static UIAnimationManager Instance { get; private set; }

        [Header("Global Settings")]
        [SerializeField] private bool enableAnimations = true;
        [SerializeField] private float globalSpeedMultiplier = 1f;

        [Header("Screen Flash")]
        [SerializeField] private Image screenFlashImage;

        [Header("Screen Shake")]
        [SerializeField] private Transform mainCanvasTransform;

        // Cache for active animations
        private Dictionary<int, Tween> activeTweens = new Dictionary<int, Tween>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDOTween();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeDOTween()
        {
            DOTween.Init(true, true, LogBehaviour.ErrorsOnly);
            DOTween.defaultAutoPlay = AutoPlay.All;
            DOTween.defaultUpdateType = UpdateType.Normal;
            DOTween.defaultTimeScaleIndependent = false;
        }

        // ==================== GLOBAL CONTROLS ====================

        public void SetAnimationsEnabled(bool enabled)
        {
            enableAnimations = enabled;
        }

        public void SetGlobalSpeed(float speed)
        {
            globalSpeedMultiplier = Mathf.Clamp(speed, 0.1f, 3f);
            DOTween.timeScale = globalSpeedMultiplier;
        }

        public void PauseAllAnimations()
        {
            DOTween.PauseAll();
        }

        public void ResumeAllAnimations()
        {
            DOTween.PlayAll();
        }

        // ==================== SCREEN EFFECTS ====================

        /// <summary>
        /// Flash the screen with a color
        /// </summary>
        public void ScreenFlash(Color color, float duration = 0.3f)
        {
            if (!enableAnimations || screenFlashImage == null) return;

            screenFlashImage.gameObject.SetActive(true);
            screenFlashImage.color = new Color(color.r, color.g, color.b, 0f);

            DOTween.Sequence()
                .Append(screenFlashImage.DOFade(0.8f, duration * 0.3f))
                .Append(screenFlashImage.DOFade(0f, duration * 0.7f))
                .OnComplete(() => screenFlashImage.gameObject.SetActive(false));
        }

        /// <summary>
        /// White flash for impacts
        /// </summary>
        public void WhiteFlash(float duration = 0.2f)
        {
            ScreenFlash(Color.white, duration);
        }

        /// <summary>
        /// Red flash for errors/damage
        /// </summary>
        public void RedFlash(float duration = 0.2f)
        {
            ScreenFlash(new Color(1f, 0.2f, 0.2f), duration);
        }

        /// <summary>
        /// Gold flash for rewards
        /// </summary>
        public void GoldFlash(float duration = 0.3f)
        {
            ScreenFlash(new Color(1f, 0.85f, 0.3f), duration);
        }

        /// <summary>
        /// Screen shake effect
        /// </summary>
        public void ScreenShake(float intensity = 10f, float duration = 0.3f)
        {
            if (!enableAnimations || mainCanvasTransform == null) return;

            mainCanvasTransform.DOShakePosition(duration, intensity, 20, 90, false, true);
        }

        /// <summary>
        /// Heavy screen shake for big impacts
        /// </summary>
        public void HeavyScreenShake()
        {
            ScreenShake(25f, 0.5f);
        }

        /// <summary>
        /// Light screen shake for small impacts
        /// </summary>
        public void LightScreenShake()
        {
            ScreenShake(5f, 0.15f);
        }

        // ==================== CURRENCY ANIMATIONS ====================

        /// <summary>
        /// Animate currency gain with flying icon
        /// </summary>
        public void AnimateCurrencyGain(RectTransform icon, RectTransform targetPosition, int amount, Action onComplete = null)
        {
            if (!enableAnimations)
            {
                onComplete?.Invoke();
                return;
            }

            // Clone the icon
            GameObject clone = Instantiate(icon.gameObject, icon.parent);
            RectTransform cloneRT = clone.GetComponent<RectTransform>();
            cloneRT.position = icon.position;

            // Fly to target
            DOTween.Sequence()
                .Append(cloneRT.DOScale(1.5f, 0.2f).SetEase(Ease.OutQuad))
                .Append(cloneRT.DOMove(targetPosition.position, 0.5f).SetEase(Ease.InBack))
                .Join(cloneRT.DOScale(0.5f, 0.5f))
                .OnComplete(() =>
                {
                    Destroy(clone);
                    onComplete?.Invoke();
                });
        }

        /// <summary>
        /// Bump animation for currency display when value changes
        /// </summary>
        public void BumpCurrencyDisplay(Transform display)
        {
            if (!enableAnimations) return;

            display.DOKill();
            display.localScale = Vector3.one;
            display.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);
        }

        // ==================== TRANSITION HELPERS ====================

        /// <summary>
        /// Prepare elements for entrance animation
        /// </summary>
        public void PrepareForEntrance(Transform[] elements, EntranceType type)
        {
            foreach (var element in elements)
            {
                switch (type)
                {
                    case EntranceType.Scale:
                        element.localScale = Vector3.zero;
                        break;
                    case EntranceType.Fade:
                        var cg = element.GetComponent<CanvasGroup>();
                        if (cg) cg.alpha = 0f;
                        break;
                    case EntranceType.SlideLeft:
                        var rt = element.GetComponent<RectTransform>();
                        if (rt) rt.anchoredPosition += new Vector2(-500f, 0f);
                        break;
                    case EntranceType.SlideRight:
                        var rt2 = element.GetComponent<RectTransform>();
                        if (rt2) rt2.anchoredPosition += new Vector2(500f, 0f);
                        break;
                    case EntranceType.SlideUp:
                        var rt3 = element.GetComponent<RectTransform>();
                        if (rt3) rt3.anchoredPosition += new Vector2(0f, 500f);
                        break;
                    case EntranceType.SlideDown:
                        var rt4 = element.GetComponent<RectTransform>();
                        if (rt4) rt4.anchoredPosition += new Vector2(0f, -500f);
                        break;
                }
            }
        }

        // ==================== UTILITY ====================

        /// <summary>
        /// Create a screen flash image if it doesn't exist
        /// </summary>
        public void SetupScreenFlash(Canvas parentCanvas)
        {
            if (screenFlashImage != null) return;

            GameObject flashObj = new GameObject("ScreenFlash");
            flashObj.transform.SetParent(parentCanvas.transform, false);
            flashObj.transform.SetAsLastSibling();

            RectTransform rt = flashObj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;

            screenFlashImage = flashObj.AddComponent<Image>();
            screenFlashImage.color = new Color(1, 1, 1, 0);
            screenFlashImage.raycastTarget = false;
            flashObj.SetActive(false);
        }

        /// <summary>
        /// Set the main canvas for screen shake
        /// </summary>
        public void SetMainCanvas(Transform canvasTransform)
        {
            mainCanvasTransform = canvasTransform;
        }

        private void OnDestroy()
        {
            DOTween.KillAll();
        }
    }

    public enum EntranceType
    {
        Scale,
        Fade,
        SlideLeft,
        SlideRight,
        SlideUp,
        SlideDown
    }
}
