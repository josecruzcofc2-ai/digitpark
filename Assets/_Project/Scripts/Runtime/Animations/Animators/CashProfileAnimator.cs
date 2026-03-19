using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;
using System.Collections.Generic;
using DigitPark.CashBattle;

namespace DigitPark.Animations
{
    /// <summary>
    /// Handles CashProfile scene entrance animations and continuous effects.
    /// Follows the same pattern as MainMenuAnimator:
    ///   - Self-contained: finds its own targets via FindDeep at runtime
    ///   - Public API: PlayEntranceSequence, PlayQuickEntrance, PlayExitAnimation, Reset
    ///   - Events: OnEntranceComplete
    ///   - Cleanup: OnDestroy kills all tweens
    ///
    /// Sequence:
    ///   0.00s Header slides down
    ///   0.15s Gold separator fades in
    ///   0.25s Avatar card pops in (scale 0 → 1)
    ///   0.45s Hero stats stagger in (3 boxes, 0.1s apart)
    ///   0.70s Counter animations on hero stats
    ///   0.80s Section header fades in
    ///   0.90s Stats grid cells cascade in (10 items, 0.06s apart)
    ///   → Continuous: avatar ring breathing, gold separator pulse, accent line glow
    /// </summary>
    public class CashProfileAnimator : MonoBehaviour
    {
        [Header("Entrance Timings")]
        [SerializeField] private float headerSlideDuration = 0.35f;
        [SerializeField] private float goldSepFadeDuration = 0.25f;
        [SerializeField] private float avatarPopDuration = 0.4f;
        [SerializeField] private float heroStatDuration = 0.35f;
        [SerializeField] private float heroStatStagger = 0.1f;
        [SerializeField] private float counterDuration = 0.8f;
        [SerializeField] private float gridCellDuration = 0.3f;
        [SerializeField] private float gridCellStagger = 0.06f;

        [Header("Continuous Effects")]
        [SerializeField] private float avatarBreathScale = 1.04f;
        [SerializeField] private float avatarBreathDuration = 2f;
        [SerializeField] private float goldSepPulseExtra = 0.2f;
        [SerializeField] private float goldSepPulseDuration = 1.5f;
        [SerializeField] private float accentPulseDuration = 1.2f;

        // Events
        public event Action OnEntranceComplete;

        // Runtime-found animation targets
        private RectTransform _headerRect;
        private Image _goldSepImage;
        private Transform _avatarCard;
        private Transform _avatarRing;
        private Transform _heroStats;
        private Transform _sectionHeader;
        private Transform _statsGrid;
        private float _goldSepTargetAlpha;

        // Counter animation targets (found via hierarchy)
        private TextMeshProUGUI _summaryTotalMatchesText;
        private TextMeshProUGUI _summaryWinRateText;
        private TextMeshProUGUI _summaryNetProfitText;

        // Cached scene references (C-P2-04: avoid repeated FindObjectOfType during animations)
        private Canvas _cachedCanvas;
        private HistoryManager _historyManager;

        // Active tweens
        private Sequence _entranceSeq;
        private List<Tween> _continuousTweens = new List<Tween>();

        // ==================== LIFECYCLE ====================

        private void Awake()
        {
            FindAnimationTargets();
        }

        private void OnDestroy()
        {
            StopAllAnimations();
        }

        // ==================== PUBLIC API ====================

        /// <summary>
        /// Play the full entrance animation sequence.
        /// Call after the Controller has set all text values via RefreshProfile().
        /// </summary>
        public void PlayEntranceSequence()
        {
            SetupEntranceState();
            BuildEntranceSequence();
        }

        /// <summary>
        /// Quick entrance: show all elements instantly with a subtle punch.
        /// Use when returning to the scene (skip full animation).
        /// </summary>
        public void PlayQuickEntrance()
        {
            ShowAllElements();

            // Subtle punch on hero stats
            if (_heroStats != null)
            {
                for (int i = 0; i < _heroStats.childCount; i++)
                {
                    Transform child = _heroStats.GetChild(i);
                    child.DOPunchScale(Vector3.one * 0.08f, 0.3f, 3).SetLink(child.gameObject);
                }
            }

            StartContinuousEffects();
        }

        /// <summary>
        /// Play exit animation when navigating away.
        /// </summary>
        public void PlayExitAnimation(Action onComplete = null)
        {
            StopContinuousEffects();

            Sequence exitSeq = DOTween.Sequence().SetLink(gameObject);

            // Stats grid scales out (reverse cascade)
            if (_statsGrid != null)
            {
                for (int i = _statsGrid.childCount - 1; i >= 0; i--)
                {
                    Transform child = _statsGrid.GetChild(i);
                    float delay = (_statsGrid.childCount - 1 - i) * 0.03f;
                    exitSeq.Insert(delay,
                        child.DOScale(0f, 0.15f).SetEase(Ease.InBack));
                }
            }

            // Hero stats scale out
            if (_heroStats != null)
            {
                for (int i = 0; i < _heroStats.childCount; i++)
                {
                    exitSeq.Insert(0.05f,
                        _heroStats.GetChild(i).DOScale(0f, 0.2f).SetEase(Ease.InBack));
                }
            }

            // Avatar card shrinks
            if (_avatarCard != null)
                exitSeq.Insert(0.1f, _avatarCard.DOScale(0f, 0.2f).SetEase(Ease.InBack));

            // Header slides up
            if (_headerRect != null)
                exitSeq.Insert(0.1f,
                    _headerRect.DOAnchorPosY(_headerRect.anchoredPosition.y + 120f, 0.25f)
                        .SetEase(Ease.InQuad));

            exitSeq.OnComplete(() => onComplete?.Invoke());
        }

        /// <summary>
        /// Stop all animations and reset to entrance-ready state.
        /// </summary>
        public void Reset()
        {
            StopAllAnimations();
            SetupEntranceState();
        }

        /// <summary>
        /// Stop all active tweens (entrance + continuous).
        /// </summary>
        public void StopAllAnimations()
        {
            _entranceSeq?.Kill();
            _entranceSeq = null;
            StopContinuousEffects();
        }

        // ==================== FIND ANIMATION TARGETS ====================

        private void FindAnimationTargets()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                if (_cachedCanvas == null) _cachedCanvas = FindObjectOfType<Canvas>();
                canvas = _cachedCanvas;
            }
            if (canvas == null) return;

            Transform root = canvas.transform;

            _headerRect = FindDeep(root, "Header")?.GetComponent<RectTransform>();
            _avatarCard = FindDeep(root, "AvatarCard");
            _avatarRing = FindDeep(root, "AvatarRing");
            _heroStats = FindDeep(root, "HeroStats");
            _statsGrid = FindDeep(root, "StatsGrid");

            // Section header name matches UIBuilder: "Section_" + title.Replace(" ", "")
            _sectionHeader = FindDeep(root, "Section_ESTADÍSTICASDETALLADAS");

            // Gold separator
            Transform sepT = FindDeep(root, "GoldSeparator");
            if (sepT) _goldSepImage = sepT.GetComponent<Image>();
            if (_goldSepImage) _goldSepTargetAlpha = _goldSepImage.color.a;

            // Counter text targets (inside hero stat boxes)
            Transform summaryTotalT = FindDeep(root, "SummaryTotalMatches");
            if (summaryTotalT != null)
            {
                Transform v = summaryTotalT.Find("Value");
                if (v) _summaryTotalMatchesText = v.GetComponent<TextMeshProUGUI>();
            }

            Transform summaryWinRateT = FindDeep(root, "SummaryWinRate");
            if (summaryWinRateT != null)
            {
                Transform v = summaryWinRateT.Find("Value");
                if (v) _summaryWinRateText = v.GetComponent<TextMeshProUGUI>();
            }

            Transform summaryNetProfitT = FindDeep(root, "SummaryNetProfit");
            if (summaryNetProfitT != null)
            {
                Transform v = summaryNetProfitT.Find("Value");
                if (v) _summaryNetProfitText = v.GetComponent<TextMeshProUGUI>();
            }
        }

        private Transform FindDeep(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                Transform r = FindDeep(child, name);
                if (r != null) return r;
            }
            return null;
        }

        // ==================== ENTRANCE SEQUENCE ====================

        private void BuildEntranceSequence()
        {
            _entranceSeq = DOTween.Sequence().SetLink(gameObject);

            // 0.0s — Header slides down from above
            if (_headerRect)
            {
                Vector2 target = _headerRect.anchoredPosition;
                _headerRect.anchoredPosition = target + new Vector2(0, 120);
                _entranceSeq.Append(
                    _headerRect.DOAnchorPos(target, headerSlideDuration).SetEase(Ease.OutQuad)
                );
            }

            // 0.15s — Gold separator fades in
            if (_goldSepImage)
            {
                _entranceSeq.Insert(0.15f,
                    _goldSepImage.DOFade(_goldSepTargetAlpha, goldSepFadeDuration)
                        .SetEase(Ease.OutQuad)
                );
            }

            // 0.25s — Avatar card pops in (scale 0 → 1)
            if (_avatarCard)
            {
                _entranceSeq.Insert(0.25f,
                    _avatarCard.DOScale(1f, avatarPopDuration).SetEase(Ease.OutBack)
                );
            }

            // 0.45s — Hero stats stagger in (3 boxes)
            if (_heroStats)
            {
                for (int i = 0; i < _heroStats.childCount; i++)
                {
                    Transform child = _heroStats.GetChild(i);
                    float delay = 0.45f + i * heroStatStagger;

                    _entranceSeq.Insert(delay,
                        child.DOScale(1f, heroStatDuration).SetEase(Ease.OutBack)
                    );

                    CanvasGroup cg = EnsureCanvasGroup(child);
                    _entranceSeq.Insert(delay,
                        cg.DOFade(1f, 0.25f).SetEase(Ease.OutQuad)
                    );
                }
            }

            // 0.70s — Counter animations on hero stats
            _entranceSeq.InsertCallback(0.70f, AnimateCounterTexts);

            // 0.80s — Section header fades in
            if (_sectionHeader)
            {
                CanvasGroup shCG = EnsureCanvasGroup(_sectionHeader);
                _entranceSeq.Insert(0.80f,
                    shCG.DOFade(1f, 0.3f).SetEase(Ease.OutQuad)
                );
            }

            // 0.90s — Stats grid cells cascade in (10 items)
            if (_statsGrid)
            {
                for (int i = 0; i < _statsGrid.childCount; i++)
                {
                    Transform child = _statsGrid.GetChild(i);
                    float delay = 0.90f + i * gridCellStagger;

                    _entranceSeq.Insert(delay,
                        child.DOScale(1f, gridCellDuration).SetEase(Ease.OutBack)
                    );

                    CanvasGroup cg = EnsureCanvasGroup(child);
                    _entranceSeq.Insert(delay,
                        cg.DOFade(1f, 0.25f).SetEase(Ease.OutQuad)
                    );
                }
            }

            // After entrance completes → start continuous effects + fire event
            _entranceSeq.OnComplete(() =>
            {
                StartContinuousEffects();
                OnEntranceComplete?.Invoke();
            });
        }

        /// <summary>
        /// Sets initial hidden state for all animated elements before entrance.
        /// </summary>
        private void SetupEntranceState()
        {
            // Gold separator: invisible
            if (_goldSepImage)
            {
                Color c = _goldSepImage.color;
                _goldSepImage.color = new Color(c.r, c.g, c.b, 0f);
            }

            // Avatar card: scale 0
            if (_avatarCard)
                _avatarCard.localScale = Vector3.zero;

            // Hero stat boxes: scale 0, alpha 0
            if (_heroStats)
            {
                for (int i = 0; i < _heroStats.childCount; i++)
                {
                    Transform child = _heroStats.GetChild(i);
                    child.localScale = Vector3.zero;
                    EnsureCanvasGroup(child).alpha = 0f;
                }
            }

            // Section header: alpha 0
            if (_sectionHeader)
                EnsureCanvasGroup(_sectionHeader).alpha = 0f;

            // Stats grid cells: scale 0.8, alpha 0
            if (_statsGrid)
            {
                for (int i = 0; i < _statsGrid.childCount; i++)
                {
                    Transform child = _statsGrid.GetChild(i);
                    child.localScale = Vector3.one * 0.8f;
                    EnsureCanvasGroup(child).alpha = 0f;
                }
            }
        }

        /// <summary>
        /// Show all elements instantly (for quick entrance).
        /// </summary>
        private void ShowAllElements()
        {
            if (_goldSepImage)
            {
                Color c = _goldSepImage.color;
                _goldSepImage.color = new Color(c.r, c.g, c.b, _goldSepTargetAlpha);
            }

            if (_avatarCard)
                _avatarCard.localScale = Vector3.one;

            if (_heroStats)
            {
                for (int i = 0; i < _heroStats.childCount; i++)
                {
                    Transform child = _heroStats.GetChild(i);
                    child.localScale = Vector3.one;
                    EnsureCanvasGroup(child).alpha = 1f;
                }
            }

            if (_sectionHeader)
                EnsureCanvasGroup(_sectionHeader).alpha = 1f;

            if (_statsGrid)
            {
                for (int i = 0; i < _statsGrid.childCount; i++)
                {
                    Transform child = _statsGrid.GetChild(i);
                    child.localScale = Vector3.one;
                    EnsureCanvasGroup(child).alpha = 1f;
                }
            }
        }

        // ==================== CONTINUOUS EFFECTS ====================

        private void StartContinuousEffects()
        {
            // Avatar ring breathing (subtle scale pulse)
            if (_avatarRing)
            {
                _continuousTweens.Add(
                    _avatarRing.DOScale(avatarBreathScale, avatarBreathDuration)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetLink(_avatarRing.gameObject)
                );
            }

            // Gold separator glow pulse
            if (_goldSepImage)
            {
                float baseAlpha = _goldSepTargetAlpha;
                _continuousTweens.Add(
                    _goldSepImage.DOFade(baseAlpha + goldSepPulseExtra, goldSepPulseDuration)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetLink(_goldSepImage.gameObject)
                );
            }

            // Hero stat accent lines: subtle glow pulse
            if (_heroStats)
            {
                for (int i = 0; i < _heroStats.childCount; i++)
                {
                    Transform child = _heroStats.GetChild(i);
                    Transform accentLine = child.Find("AccentLine");
                    if (accentLine)
                    {
                        Image accent = accentLine.GetComponent<Image>();
                        if (accent)
                        {
                            Color ac = accent.color;
                            float startDelay = i * 0.5f;
                            _continuousTweens.Add(
                                accent.DOFade(ac.a * 0.5f, accentPulseDuration)
                                    .SetEase(Ease.InOutSine)
                                    .SetLoops(-1, LoopType.Yoyo)
                                    .SetDelay(startDelay)
                                    .SetLink(accent.gameObject)
                            );
                        }
                    }
                }
            }
        }

        private void StopContinuousEffects()
        {
            foreach (var tween in _continuousTweens)
                tween?.Kill();
            _continuousTweens.Clear();
        }

        // ==================== COUNTER ANIMATIONS ====================

        private void AnimateCounterTexts()
        {
            // Read stats from HistoryManager singleton directly (no reflection)
            var historyManager = FindHistoryManager();
            if (historyManager == null) return;

            var stats = historyManager.GetStats();
            if (stats == null) return;

            int totalMatches = stats.totalMatches;
            float winRate = stats.winRate;
            float netProfit = stats.netProfit;

            // Animate total matches counter
            AnimateCounter(_summaryTotalMatchesText, 0, totalMatches, counterDuration);

            // Animate win rate counter
            AnimateCounter(_summaryWinRateText, 0f, winRate, counterDuration, "{0:F1}%");

            // Animate net profit counter
            if (_summaryNetProfitText != null)
            {
                string prefix = netProfit >= 0 ? "+$" : "-$";
                float absTarget = Mathf.Abs(netProfit);
                DOTween.To(() => 0f, x =>
                {
                    _summaryNetProfitText.text = $"{prefix}{x:F2}";
                }, absTarget, counterDuration).SetEase(Ease.OutQuad).SetLink(gameObject);
            }
        }

        private HistoryManager FindHistoryManager()
        {
            // Use singleton Instance directly instead of iterating all MonoBehaviours
            if (HistoryManager.Instance != null)
                return HistoryManager.Instance;

            // Fallback: find in scene (cached to avoid repeated search)
            if (_historyManager == null) _historyManager = FindObjectOfType<HistoryManager>();
            return _historyManager;
        }

        private void AnimateCounter(TextMeshProUGUI text, int from, int to, float duration)
        {
            if (text == null) return;
            DOTween.To(() => from, x => text.text = x.ToString(), to, duration)
                .SetEase(Ease.OutQuad).SetLink(gameObject);
        }

        private void AnimateCounter(TextMeshProUGUI text, float from, float to, float duration, string format)
        {
            if (text == null) return;
            DOTween.To(() => from, x => text.text = string.Format(format, x), to, duration)
                .SetEase(Ease.OutQuad).SetLink(gameObject);
        }

        // ==================== HELPERS ====================

        private CanvasGroup EnsureCanvasGroup(Transform t)
        {
            var cg = t.GetComponent<CanvasGroup>();
            if (cg == null) cg = t.gameObject.AddComponent<CanvasGroup>();
            return cg;
        }
    }
}
