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
    /// Escalating combo visual feedback system for minigames.
    /// Creates its own UI overlay at runtime. Attach to any game scene.
    ///
    /// Tiers:
    ///   x2-x3:  Green text "COMBO x3" with punch scale
    ///   x4-x6:  Larger text, particles per hit, green screen border glow
    ///   x7-x9:  "AMAZING!" milestone text, intense particles, yellow glow
    ///   x10-x14: "INCREDIBLE!", fire-like border, strong haptic
    ///   x15+:   "GODLIKE!", golden full-screen glow, maximum effects
    ///
    /// Usage:
    ///   comboController.OnComboChanged(currentCombo);
    ///   comboController.OnComboReset();
    /// </summary>
    public class ComboVisualController : MonoBehaviour
    {
        // ==================== TIER COLORS ====================
        private static readonly Color COLOR_TIER_1 = new Color(0.2f, 1f, 0.4f, 1f);      // Green (x2-x3)
        private static readonly Color COLOR_TIER_2 = new Color(0f, 0.9f, 1f, 1f);         // Cyan (x4-x6)
        private static readonly Color COLOR_TIER_3 = new Color(1f, 0.85f, 0.2f, 1f);      // Yellow (x7-x9)
        private static readonly Color COLOR_TIER_4 = new Color(1f, 0.5f, 0.2f, 1f);       // Orange (x10-x14)
        private static readonly Color COLOR_TIER_5 = new Color(1f, 0.84f, 0f, 1f);        // Gold (x15+)

        // ==================== GLOW COLORS ====================
        private static readonly Color GLOW_GREEN = new Color(0.2f, 1f, 0.4f, 0.3f);
        private static readonly Color GLOW_YELLOW = new Color(1f, 0.85f, 0.2f, 0.4f);
        private static readonly Color GLOW_ORANGE = new Color(1f, 0.5f, 0.2f, 0.5f);
        private static readonly Color GLOW_GOLD = new Color(1f, 0.84f, 0f, 0.6f);

        // ==================== UI REFERENCES ====================
        private Canvas parentCanvas;
        private RectTransform overlayRT;
        private CanvasGroup canvasGroup;

        // Combo counter text
        private TextMeshProUGUI comboText;
        private RectTransform comboTextRT;

        // Milestone text ("AMAZING!", "INCREDIBLE!", etc.)
        private TextMeshProUGUI milestoneText;
        private RectTransform milestoneTextRT;

        // Screen border glow (4 edge images)
        private Image glowTop;
        private Image glowBottom;
        private Image glowLeft;
        private Image glowRight;

        // State
        private int currentCombo;
        private int previousTier;
        private bool isSetup;

        // Tween tracking
        private readonly List<Tween> _activeTweens = new List<Tween>();
        private Sequence _comboSequence;
        private Sequence _milestoneSequence;
        private Sequence _glowSequence;

        /// <summary>
        /// Initialize the combo visual system on the given canvas.
        /// Call once during game setup.
        /// </summary>
        public void Initialize(Canvas canvas)
        {
            if (canvas == null || isSetup) return;
            parentCanvas = canvas;
            Setup();
            isSetup = true;
        }

        /// <summary>
        /// Call when combo count changes (on correct answer/hit).
        /// </summary>
        public void OnComboChanged(int combo)
        {
            if (!isSetup) return;

            int prevCombo = currentCombo;
            currentCombo = combo;

            if (currentCombo < 2)
            {
                HideComboUI();
                return;
            }

            int tier = GetTier(currentCombo);
            Color tierColor = GetTierColor(tier);

            // Update combo text
            UpdateComboText(tierColor);

            // Punch animation on every hit
            AnimateComboPunch(tier);

            // Check for tier upgrade → milestone text
            if (tier > previousTier && tier >= 2)
            {
                ShowMilestoneText(tier, tierColor);
            }

            // Update screen border glow
            UpdateBorderGlow(tier);

            // Screen shake at high combos
            if (currentCombo >= 7 && UIAnimationManager.Instance != null)
            {
                float intensity = Mathf.Min(currentCombo * 1.5f, 25f);
                UIAnimationManager.Instance.LightScreenShake();
            }

            previousTier = tier;
        }

        /// <summary>
        /// Call when combo resets (on error/wrong answer).
        /// </summary>
        public void OnComboReset()
        {
            if (!isSetup) return;

            if (currentCombo >= 3)
            {
                // Dramatic combo break effect
                AnimateComboBreak();
            }

            currentCombo = 0;
            previousTier = 0;
            HideComboUI();
        }

        // ==================== SETUP ====================

        private void Setup()
        {
            // Create overlay container (non-blocking)
            var overlayGO = new GameObject("ComboVisualOverlay");
            overlayGO.transform.SetParent(parentCanvas.transform, false);
            overlayGO.transform.SetAsLastSibling();

            overlayRT = overlayGO.AddComponent<RectTransform>();
            overlayRT.anchorMin = Vector2.zero;
            overlayRT.anchorMax = Vector2.one;
            overlayRT.offsetMin = Vector2.zero;
            overlayRT.offsetMax = Vector2.zero;

            canvasGroup = overlayGO.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            // Create combo text (top-center area)
            CreateComboText();

            // Create milestone text (center of screen)
            CreateMilestoneText();

            // Create border glow images
            CreateBorderGlow();
        }

        private void CreateComboText()
        {
            var go = new GameObject("ComboText");
            go.transform.SetParent(overlayRT, false);

            comboTextRT = go.AddComponent<RectTransform>();
            comboTextRT.anchorMin = new Vector2(0.5f, 0.85f);
            comboTextRT.anchorMax = new Vector2(0.5f, 0.85f);
            comboTextRT.sizeDelta = new Vector2(300, 80);

            comboText = go.AddComponent<TextMeshProUGUI>();
            comboText.text = "";
            comboText.fontSize = 48;
            comboText.fontStyle = FontStyles.Bold;
            comboText.alignment = TextAlignmentOptions.Center;
            comboText.enableAutoSizing = false;
            comboText.alpha = 0f;
        }

        private void CreateMilestoneText()
        {
            var go = new GameObject("MilestoneText");
            go.transform.SetParent(overlayRT, false);

            milestoneTextRT = go.AddComponent<RectTransform>();
            milestoneTextRT.anchorMin = new Vector2(0.5f, 0.6f);
            milestoneTextRT.anchorMax = new Vector2(0.5f, 0.6f);
            milestoneTextRT.sizeDelta = new Vector2(500, 100);

            milestoneText = go.AddComponent<TextMeshProUGUI>();
            milestoneText.text = "";
            milestoneText.fontSize = 64;
            milestoneText.fontStyle = FontStyles.Bold;
            milestoneText.alignment = TextAlignmentOptions.Center;
            milestoneText.enableAutoSizing = false;
            milestoneText.alpha = 0f;
        }

        private void CreateBorderGlow()
        {
            float thickness = 30f;

            // Top
            glowTop = CreateGlowEdge("GlowTop",
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0, -thickness));

            // Bottom
            glowBottom = CreateGlowEdge("GlowBottom",
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(0, thickness));

            // Left
            glowLeft = CreateGlowEdge("GlowLeft",
                new Vector2(0f, 0f), new Vector2(0f, 1f),
                new Vector2(thickness, 0));

            // Right
            glowRight = CreateGlowEdge("GlowRight",
                new Vector2(1f, 0f), new Vector2(1f, 1f),
                new Vector2(-thickness, 0));
        }

        private Image CreateGlowEdge(string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta)
        {
            var go = new GameObject(name);
            go.transform.SetParent(overlayRT, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // For horizontal edges, set height; for vertical, set width
            if (Mathf.Abs(anchorMin.x - anchorMax.x) > 0.5f)
            {
                // Horizontal edge (top/bottom)
                rt.sizeDelta = new Vector2(0, Mathf.Abs(sizeDelta.y));
                if (sizeDelta.y < 0)
                {
                    // Top edge: anchor at top, grow downward
                    rt.pivot = new Vector2(0.5f, 1f);
                }
                else
                {
                    // Bottom edge: anchor at bottom, grow upward
                    rt.pivot = new Vector2(0.5f, 0f);
                }
            }
            else
            {
                // Vertical edge (left/right)
                rt.sizeDelta = new Vector2(Mathf.Abs(sizeDelta.x), 0);
                if (sizeDelta.x > 0)
                {
                    // Left edge
                    rt.pivot = new Vector2(0f, 0.5f);
                }
                else
                {
                    // Right edge
                    rt.pivot = new Vector2(1f, 0.5f);
                }
            }

            var img = go.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0);
            img.raycastTarget = false;

            return img;
        }

        // ==================== TIER LOGIC ====================

        private int GetTier(int combo)
        {
            if (combo >= 15) return 5;
            if (combo >= 10) return 4;
            if (combo >= 7) return 3;
            if (combo >= 4) return 2;
            if (combo >= 2) return 1;
            return 0;
        }

        private Color GetTierColor(int tier)
        {
            switch (tier)
            {
                case 1: return COLOR_TIER_1;
                case 2: return COLOR_TIER_2;
                case 3: return COLOR_TIER_3;
                case 4: return COLOR_TIER_4;
                case 5: return COLOR_TIER_5;
                default: return Color.white;
            }
        }

        private string GetMilestoneText(int tier)
        {
            switch (tier)
            {
                case 2: return "GREAT!";
                case 3: return "AMAZING!";
                case 4: return "INCREDIBLE!";
                case 5: return "GODLIKE!";
                default: return "";
            }
        }

        private Color GetGlowColor(int tier)
        {
            switch (tier)
            {
                case 2: return GLOW_GREEN;
                case 3: return GLOW_YELLOW;
                case 4: return GLOW_ORANGE;
                case 5: return GLOW_GOLD;
                default: return Color.clear;
            }
        }

        // ==================== ANIMATIONS ====================

        private void UpdateComboText(Color color)
        {
            comboText.text = $"COMBO x{currentCombo}";
            comboText.color = color;

            // Fade in if hidden
            if (comboText.alpha < 0.5f)
            {
                var t = comboText.DOFade(1f, 0.15f);
                TrackTween(t);
            }

            // Scale text based on tier
            int tier = GetTier(currentCombo);
            float targetSize = 48 + (tier - 1) * 8;
            comboText.fontSize = targetSize;
        }

        private void AnimateComboPunch(int tier)
        {
            _comboSequence?.Kill();

            float punchScale = 0.2f + tier * 0.05f;
            _comboSequence = DOTween.Sequence();
            _comboSequence.Append(comboTextRT.DOPunchScale(Vector3.one * punchScale, 0.25f, 1, 0.5f));
            TrackTween(_comboSequence);
        }

        private void ShowMilestoneText(int tier, Color color)
        {
            string text = GetMilestoneText(tier);
            if (string.IsNullOrEmpty(text)) return;

            _milestoneSequence?.Kill();

            milestoneText.text = text;
            milestoneText.color = color;
            milestoneText.alpha = 0f;
            milestoneTextRT.localScale = Vector3.one * 0.3f;

            _milestoneSequence = DOTween.Sequence();

            // Pop in with overshoot
            _milestoneSequence.Append(milestoneText.DOFade(1f, 0.1f));
            _milestoneSequence.Join(milestoneTextRT.DOScale(1.2f, 0.25f).SetEase(Ease.OutBack));

            // Settle
            _milestoneSequence.Append(milestoneTextRT.DOScale(1f, 0.1f).SetEase(Ease.InOutQuad));

            // Hold
            _milestoneSequence.AppendInterval(0.6f);

            // Fade out upward
            _milestoneSequence.Append(milestoneText.DOFade(0f, 0.3f));
            _milestoneSequence.Join(milestoneTextRT.DOAnchorPosY(
                milestoneTextRT.anchoredPosition.y + 40f, 0.3f).SetEase(Ease.InQuad));

            // Reset position after fade
            _milestoneSequence.OnComplete(() =>
            {
                milestoneTextRT.anchoredPosition = new Vector2(0, 0);
            });

            TrackTween(_milestoneSequence);

            // Screen flash for high tiers
            if (tier >= 3 && UIAnimationManager.Instance != null)
            {
                if (tier >= 5)
                    UIAnimationManager.Instance.GoldFlash();
                else
                    UIAnimationManager.Instance.WhiteFlash();
            }
        }

        private void UpdateBorderGlow(int tier)
        {
            if (tier < 2)
            {
                FadeGlowOut();
                return;
            }

            _glowSequence?.Kill();
            Color glowColor = GetGlowColor(tier);

            _glowSequence = DOTween.Sequence();

            // Fade in glow edges
            _glowSequence.Append(glowTop.DOColor(glowColor, 0.3f));
            _glowSequence.Join(glowBottom.DOColor(glowColor, 0.3f));
            _glowSequence.Join(glowLeft.DOColor(glowColor, 0.3f));
            _glowSequence.Join(glowRight.DOColor(glowColor, 0.3f));

            // Pulse effect for tier 4+
            if (tier >= 4)
            {
                Color pulseColor = new Color(glowColor.r, glowColor.g, glowColor.b, glowColor.a * 1.5f);
                _glowSequence.Append(glowTop.DOColor(pulseColor, 0.4f).SetLoops(-1, LoopType.Yoyo));
                _glowSequence.Join(glowBottom.DOColor(pulseColor, 0.4f).SetLoops(-1, LoopType.Yoyo));
                _glowSequence.Join(glowLeft.DOColor(pulseColor, 0.4f).SetLoops(-1, LoopType.Yoyo));
                _glowSequence.Join(glowRight.DOColor(pulseColor, 0.4f).SetLoops(-1, LoopType.Yoyo));
            }

            TrackTween(_glowSequence);
        }

        private void FadeGlowOut()
        {
            _glowSequence?.Kill();

            Color clear = Color.clear;
            var t1 = glowTop.DOColor(clear, 0.3f);
            var t2 = glowBottom.DOColor(clear, 0.3f);
            var t3 = glowLeft.DOColor(clear, 0.3f);
            var t4 = glowRight.DOColor(clear, 0.3f);

            TrackTween(t1);
            TrackTween(t2);
            TrackTween(t3);
            TrackTween(t4);
        }

        private void AnimateComboBreak()
        {
            // Flash combo text red briefly, then fade out
            var seq = DOTween.Sequence();

            comboText.color = Color.red;
            seq.Append(comboTextRT.DOShakePosition(0.3f, 10f, 20));
            seq.Join(comboText.DOFade(0f, 0.4f));

            // Flash border red then fade
            Color redGlow = new Color(1f, 0.2f, 0.2f, 0.4f);
            seq.Join(glowTop.DOColor(redGlow, 0.1f));
            seq.Join(glowBottom.DOColor(redGlow, 0.1f));
            seq.Join(glowLeft.DOColor(redGlow, 0.1f));
            seq.Join(glowRight.DOColor(redGlow, 0.1f));

            seq.AppendInterval(0.1f);

            seq.Append(glowTop.DOColor(Color.clear, 0.3f));
            seq.Join(glowBottom.DOColor(Color.clear, 0.3f));
            seq.Join(glowLeft.DOColor(Color.clear, 0.3f));
            seq.Join(glowRight.DOColor(Color.clear, 0.3f));

            TrackTween(seq);
        }

        private void HideComboUI()
        {
            _comboSequence?.Kill();

            if (comboText != null && comboText.alpha > 0f)
            {
                var t = comboText.DOFade(0f, 0.2f);
                TrackTween(t);
            }

            FadeGlowOut();
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
            _comboSequence?.Kill();
            _milestoneSequence?.Kill();
            _glowSequence?.Kill();

            foreach (var t in _activeTweens)
            {
                t?.Kill();
            }
            _activeTweens.Clear();

            if (comboText != null) comboText.DOKill();
            if (comboTextRT != null) comboTextRT.DOKill();
            if (milestoneText != null) milestoneText.DOKill();
            if (milestoneTextRT != null) milestoneTextRT.DOKill();
            if (glowTop != null) glowTop.DOKill();
            if (glowBottom != null) glowBottom.DOKill();
            if (glowLeft != null) glowLeft.DOKill();
            if (glowRight != null) glowRight.DOKill();
        }
    }
}
