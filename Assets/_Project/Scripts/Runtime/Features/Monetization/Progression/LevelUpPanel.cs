using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;
using DigitPark.Localization;

namespace DigitPark.Progression
{
    /// <summary>
    /// Panel modal de level-up con animación DOTween de 6 fases.
    /// Se instancia on-demand desde LevelUpNotifier via Resources.Load("UI/LevelUpPanel").
    /// </summary>
    public class LevelUpPanel : MonoBehaviour
    {
        [Header("Overlay")]
        [SerializeField] private Image overlay;
        [SerializeField] private CanvasGroup panelCanvasGroup;
        [SerializeField] private RectTransform panelRoot;

        [Header("Level Badge")]
        [SerializeField] private TextMeshProUGUI levelUpLabel;
        [SerializeField] private TextMeshProUGUI oldLevelText;
        [SerializeField] private TextMeshProUGUI newLevelText;
        [SerializeField] private Image badgeRing;
        [SerializeField] private TextMeshProUGUI milestoneLabel;

        [Header("XP Bar")]
        [SerializeField] private CanvasGroup xpBarGroup;
        [SerializeField] private Image xpBarFill;
        [SerializeField] private Image xpBarFlash;
        [SerializeField] private TextMeshProUGUI xpLabel;
        [SerializeField] private TextMeshProUGUI experienceTitle;

        [Header("Reward")]
        [SerializeField] private RectTransform rewardCard;
        [SerializeField] private CanvasGroup rewardCardGroup;
        [SerializeField] private TextMeshProUGUI rewardUnlockedLabel;
        [SerializeField] private TextMeshProUGUI rewardNameText;
        [SerializeField] private TextMeshProUGUI rewardHintText;
        [SerializeField] private Image rewardIcon;
        [SerializeField] private TextMeshProUGUI nextRewardHint;

        [Header("CTA")]
        [SerializeField] private Button continueButton;
        [SerializeField] private CanvasGroup continueButtonGroup;
        [SerializeField] private RectTransform continueButtonRT;

        // Tracked sequences for safe cleanup
        private Sequence _mainSequence;
        private Sequence _ringLoop;
        private Sequence _xpBarLoop;
        private Sequence _buttonLoop;

        // Queue for multi-level-up
        private static readonly Queue<LevelUpData> _queue = new Queue<LevelUpData>();
        private static bool _isShowing;

        // Milestone levels
        private static readonly HashSet<int> MilestoneLevels = new HashSet<int>
            { 10, 25, 50, 100, 200, 300, 500 };

        // Colors
        private static readonly Color CYAN_NEON = new Color(0f, 0.898f, 1f);       // #00E5FF
        private static readonly Color MAGENTA_NEON = new Color(1f, 0f, 0.647f);     // #FF00A5
        private static readonly Color GOLD = new Color(1f, 0.843f, 0f);             // #FFD700
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.6f, 0.65f);
        private static readonly Color PANEL_BG = new Color(0.05f, 0.07f, 0.11f, 1f);

        private struct LevelUpData
        {
            public int oldLevel;
            public int newLevel;
            public float xpProgress;
            public LevelReward reward;
        }

        private void Awake()
        {
            if (continueButton != null)
                continueButton.onClick.AddListener(Close);

            // Overlay tap also closes
            if (overlay != null)
            {
                var overlayBtn = overlay.gameObject.GetComponent<Button>();
                if (overlayBtn == null) overlayBtn = overlay.gameObject.AddComponent<Button>();
                overlayBtn.onClick.AddListener(Close);
            }
        }

        private void OnDestroy()
        {
            KillAllTweens();
            if (continueButton != null) continueButton.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// Enqueue and show a level-up. If already showing, queues for later.
        /// </summary>
        public void Show(int oldLevel, int newLevel, float newLevelXPProgress,
                         LevelReward reward = null)
        {
            var data = new LevelUpData
            {
                oldLevel = oldLevel,
                newLevel = newLevel,
                xpProgress = newLevelXPProgress,
                reward = reward
            };

            _queue.Enqueue(data);

            if (!_isShowing)
                ShowNext();
        }

        private void ShowNext()
        {
            if (_queue.Count == 0)
            {
                _isShowing = false;
                return;
            }

            _isShowing = true;
            var data = _queue.Dequeue();
            gameObject.SetActive(true);

            // Reset state
            ResetUI();

            bool isMilestone = MilestoneLevels.Contains(data.newLevel);

            // Build main sequence
            _mainSequence = DOTween.Sequence().SetLink(gameObject);

            // FASE 0 — Entry
            PlayEntryPhase(isMilestone);

            // FASE 1 — Level Badge
            PlayLevelBadgePhase(data.oldLevel, data.newLevel, isMilestone);

            // FASE 2 — XP Bar
            PlayXPBarPhase(data.xpProgress);

            // FASE 3 — Reward Reveal
            PlayRewardPhase(data.reward, data.newLevel);

            // FASE 4 — Milestone extras (handled inside Fase 1)

            // FASE 5 — CTA Button
            PlayButtonPhase();
        }

        private void ResetUI()
        {
            KillAllTweens();

            if (overlay != null) overlay.color = new Color(0, 0, 0, 0);
            if (panelCanvasGroup != null) { panelCanvasGroup.alpha = 0; panelCanvasGroup.blocksRaycasts = true; }
            if (panelRoot != null) panelRoot.localScale = new Vector3(0.5f, 0.5f, 1f);
            if (levelUpLabel != null) levelUpLabel.transform.localScale = Vector3.zero;
            if (badgeRing != null) badgeRing.transform.localScale = Vector3.zero;
            if (oldLevelText != null) { oldLevelText.alpha = 1; oldLevelText.transform.localScale = Vector3.one; }
            if (newLevelText != null) { newLevelText.alpha = 0; newLevelText.transform.localScale = Vector3.zero; }
            if (milestoneLabel != null) { milestoneLabel.alpha = 0; milestoneLabel.transform.localScale = Vector3.zero; }
            if (xpBarGroup != null) xpBarGroup.alpha = 0;
            if (xpBarFill != null) xpBarFill.fillAmount = 0;
            if (xpBarFlash != null) xpBarFlash.color = new Color(1, 1, 1, 0);
            if (rewardCardGroup != null) rewardCardGroup.alpha = 0;
            if (rewardCard != null) rewardCard.anchoredPosition = new Vector2(0, -120);
            if (nextRewardHint != null) nextRewardHint.alpha = 0;
            if (continueButtonGroup != null) { continueButtonGroup.alpha = 0; continueButtonGroup.blocksRaycasts = false; }
            if (continueButtonRT != null) continueButtonRT.localScale = new Vector3(0.8f, 0.8f, 1f);
        }

        #region Animation Phases

        private void PlayEntryPhase(bool isMilestone)
        {
            // Overlay fade in
            if (overlay != null)
                _mainSequence.Append(overlay.DOFade(0.85f, 0.35f).SetEase(Ease.Linear));

            // Panel scale + alpha
            if (panelRoot != null)
            {
                Ease entryEase = isMilestone ? Ease.OutElastic : Ease.OutBack;
                _mainSequence.Join(panelRoot.DOScale(Vector3.one, 0.45f).SetEase(entryEase));
            }
            if (panelCanvasGroup != null)
                _mainSequence.Join(panelCanvasGroup.DOFade(1f, 0.30f).SetEase(Ease.OutQuad));

            // Haptic at t=0.15
            _mainSequence.InsertCallback(0.15f, () =>
            {
#if UNITY_IOS || UNITY_ANDROID
                Handheld.Vibrate();
#endif
            });

            // Milestone: full-screen flash
            if (isMilestone && overlay != null)
            {
                _mainSequence.Insert(0f,
                    overlay.DOColor(new Color(1, 1, 1, 0.6f), 0.25f)
                        .SetEase(Ease.OutQuad)
                        .SetLoops(2, LoopType.Yoyo));
            }
        }

        private void PlayLevelBadgePhase(int oldLevel, int newLevel, bool isMilestone)
        {
            // "LEVEL UP!" label
            if (levelUpLabel != null)
            {
                levelUpLabel.text = AutoLocalizer.Get("levelup_title");
                levelUpLabel.color = CYAN_NEON;
                _mainSequence.Insert(0.40f,
                    levelUpLabel.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack).From(0f));
            }

            // Badge ring
            if (badgeRing != null)
            {
                _mainSequence.Insert(0.60f,
                    badgeRing.transform.DOScale(1f, 0.5f).SetEase(Ease.OutElastic).From(0f));

                // Ring color loop
                _ringLoop = DOTween.Sequence().SetLink(gameObject);
                _ringLoop.Append(badgeRing.DOColor(CYAN_NEON, 1f));
                _ringLoop.Append(badgeRing.DOColor(MAGENTA_NEON, 1f));
                _ringLoop.SetLoops(-1, LoopType.Yoyo);
                _ringLoop.Pause();
                _mainSequence.InsertCallback(1.1f, () => _ringLoop.Play());
            }

            // Old level number
            if (oldLevelText != null)
            {
                oldLevelText.text = oldLevel.ToString();
                _mainSequence.Insert(0.70f,
                    oldLevelText.transform.DOScale(0.3f, 0.25f).SetEase(Ease.InBack));
                _mainSequence.Insert(0.70f,
                    oldLevelText.DOFade(0f, 0.25f));
                _mainSequence.Insert(0.70f,
                    oldLevelText.rectTransform.DOAnchorPosY(20f, 0.25f).SetRelative(true));
            }

            // New level number - slam in
            if (newLevelText != null)
            {
                newLevelText.text = newLevel.ToString();
                _mainSequence.Insert(0.90f,
                    newLevelText.DOFade(1f, 0.20f));
                _mainSequence.Insert(0.90f,
                    newLevelText.transform.DOScale(1f, 0.40f).SetEase(Ease.OutBack).From(0f));
                _mainSequence.Insert(1.00f,
                    newLevelText.transform.DOPunchScale(new Vector3(0.25f, 0.25f, 0), 0.3f, 2));

                // Haptic on slam
                _mainSequence.InsertCallback(1.00f, () =>
                {
#if UNITY_IOS || UNITY_ANDROID
                    Handheld.Vibrate();
#endif
                });
            }

            // Milestone label
            if (isMilestone && milestoneLabel != null)
            {
                milestoneLabel.text = AutoLocalizer.Get("levelup_milestone");
                milestoneLabel.color = GOLD;
                _mainSequence.Insert(0.50f,
                    milestoneLabel.DOFade(1f, 0.25f));
                _mainSequence.Insert(0.50f,
                    milestoneLabel.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack).From(0f));
            }
        }

        private void PlayXPBarPhase(float newProgress)
        {
            if (xpBarGroup == null || xpBarFill == null) return;

            // Show XP bar
            _mainSequence.Insert(1.00f,
                xpBarGroup.DOFade(1f, 0.20f));

            // Experience title
            if (experienceTitle != null)
                experienceTitle.text = AutoLocalizer.Get("experience_label");

            // Part A: Fill to 100%
            _mainSequence.Insert(1.10f,
                xpBarFill.DOFillAmount(1f, 0.40f).SetEase(Ease.OutCubic));

            // Flash at completion
            if (xpBarFlash != null)
            {
                _mainSequence.Insert(1.50f,
                    xpBarFlash.DOFade(0.6f, 0.075f).SetLoops(2, LoopType.Yoyo));
            }

            // Part B: Reset and fill to new progress
            _mainSequence.InsertCallback(1.65f, () =>
            {
                if (xpBarFill != null) xpBarFill.fillAmount = 0;
            });
            _mainSequence.Insert(1.65f,
                xpBarFill.DOFillAmount(Mathf.Clamp01(newProgress), 0.50f).SetEase(Ease.OutQuad).SetDelay(0.01f));

            // Update XP label
            _mainSequence.InsertCallback(1.65f, () =>
            {
                if (xpLabel != null)
                {
                    var prog = PlayerProgressionSystem.Instance;
                    if (prog != null)
                        xpLabel.text = prog.GetProgressString();
                }
            });

            // XP bar subtle glow loop
            _xpBarLoop = DOTween.Sequence().SetLink(gameObject);
            if (xpBarFill != null)
            {
                _xpBarLoop.Append(xpBarFill.DOColor(
                    Color.Lerp(CYAN_NEON, MAGENTA_NEON, 0.3f), 1f));
                _xpBarLoop.Append(xpBarFill.DOColor(CYAN_NEON, 1f));
                _xpBarLoop.SetLoops(-1, LoopType.Yoyo);
                _xpBarLoop.Pause();
            }
            _mainSequence.InsertCallback(2.15f, () => _xpBarLoop?.Play());
        }

        private void PlayRewardPhase(LevelReward reward, int newLevel)
        {
            if (reward != null)
            {
                // Case A: Has reward
                if (rewardCard != null && rewardCardGroup != null)
                {
                    // Slide in from below
                    _mainSequence.Insert(1.80f,
                        rewardCard.DOAnchorPosY(0, 0.45f).SetEase(Ease.OutBack));
                    _mainSequence.Insert(1.80f,
                        rewardCardGroup.DOFade(1f, 0.30f));
                }

                // "REWARD UNLOCKED" typewriter via DOTween.To + substring
                if (rewardUnlockedLabel != null)
                {
                    string fullText = AutoLocalizer.Get("reward_unlocked_title");
                    rewardUnlockedLabel.text = "";
                    rewardUnlockedLabel.color = GOLD;
                    float typewriterDuration = fullText.Length * 0.03f;
                    _mainSequence.Insert(2.00f,
                        DOTween.To(() => 0, charCount =>
                        {
                            if (rewardUnlockedLabel != null)
                                rewardUnlockedLabel.text = fullText.Substring(0, Mathf.Min(charCount, fullText.Length));
                        }, fullText.Length, typewriterDuration)
                            .SetEase(Ease.Linear)
                            .SetLink(gameObject));
                }

                // Reward icon
                if (rewardIcon != null)
                {
                    rewardIcon.transform.localScale = Vector3.zero;
                    _mainSequence.Insert(2.20f,
                        rewardIcon.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack).From(0f));

                    // Try to load icon
                    string iconPath = GetRewardIconPath(reward);
                    var sprite = Resources.Load<Sprite>(iconPath);
                    if (sprite != null) rewardIcon.sprite = sprite;
                }

                // Reward name
                if (rewardNameText != null)
                {
                    rewardNameText.text = reward.name;
                    rewardNameText.alpha = 0;
                    rewardNameText.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
                    _mainSequence.Insert(2.40f,
                        rewardNameText.DOFade(1f, 0.25f));
                    _mainSequence.Insert(2.40f,
                        rewardNameText.transform.DOScale(1f, 0.25f));
                }

                // Equip hint
                if (rewardHintText != null)
                {
                    rewardHintText.text = AutoLocalizer.Get("levelup_equip_hint");
                    rewardHintText.alpha = 0;
                    _mainSequence.Insert(2.50f,
                        rewardHintText.DOFade(0.7f, 0.25f));
                }

                // Hide next reward hint
                if (nextRewardHint != null) nextRewardHint.gameObject.SetActive(false);
            }
            else
            {
                // Case B: No reward - show "Next reward at level X"
                if (rewardCard != null) rewardCard.gameObject.SetActive(false);

                if (nextRewardHint != null)
                {
                    nextRewardHint.gameObject.SetActive(true);
                    int nextRewardLevel = FindNextRewardLevel(newLevel);
                    if (nextRewardLevel > 0)
                        nextRewardHint.text = AutoLocalizer.Get("levelup_next_reward", nextRewardLevel);
                    else
                        nextRewardHint.text = "";

                    nextRewardHint.color = TEXT_SECONDARY;
                    _mainSequence.Insert(1.80f,
                        nextRewardHint.DOFade(1f, 0.30f));
                }
            }
        }

        private void PlayButtonPhase()
        {
            if (continueButtonGroup == null) return;

            // Fade in
            _mainSequence.Insert(2.50f,
                continueButtonGroup.DOFade(1f, 0.30f).SetEase(Ease.OutQuad));
            if (continueButtonRT != null)
            {
                _mainSequence.Insert(2.50f,
                    continueButtonRT.DOScale(1f, 0.30f).SetEase(Ease.OutBack));
            }

            // Enable interaction
            _mainSequence.InsertCallback(2.80f, () =>
            {
                if (continueButtonGroup != null) continueButtonGroup.blocksRaycasts = true;
            });

            // Pulse loop
            _buttonLoop = DOTween.Sequence().SetLink(gameObject);
            if (continueButtonRT != null)
            {
                _buttonLoop.Append(continueButtonRT.DOScale(1.04f, 0.75f).SetEase(Ease.InOutSine));
                _buttonLoop.Append(continueButtonRT.DOScale(1f, 0.75f).SetEase(Ease.InOutSine));
                _buttonLoop.SetLoops(-1, LoopType.Restart);
                _buttonLoop.Pause();
            }
            _mainSequence.InsertCallback(2.80f, () => _buttonLoop?.Play());
        }

        #endregion

        #region Close

        public void Close()
        {
            KillAllTweens();

            var closeSeq = DOTween.Sequence().SetLink(gameObject);

            if (panelRoot != null)
                closeSeq.Append(panelRoot.DOScale(0.9f, 0.25f).SetEase(Ease.InBack));
            if (panelCanvasGroup != null)
                closeSeq.Join(panelCanvasGroup.DOFade(0f, 0.25f));
            if (overlay != null)
                closeSeq.Join(overlay.DOFade(0f, 0.25f));

            closeSeq.OnComplete(() =>
            {
                if (_queue.Count > 0)
                    ShowNext();
                else
                {
                    _isShowing = false;
                    Destroy(gameObject);
                }
            });
        }

        private void KillAllTweens()
        {
            _ringLoop?.Kill();
            _xpBarLoop?.Kill();
            _buttonLoop?.Kill();
            _mainSequence?.Kill();
            _ringLoop = null;
            _xpBarLoop = null;
            _buttonLoop = null;
            _mainSequence = null;
        }

        #endregion

        #region Helpers

        private string GetRewardIconPath(LevelReward reward)
        {
            switch (reward.type)
            {
                case RewardType.Frame:
                    return $"Icons/Frames/{reward.rewardId}";
                case RewardType.Avatar:
                    return $"Icons/Avatars/{reward.rewardId}";
                case RewardType.Title:
                    return $"Icons/Titles/{reward.rewardId}";
                case RewardType.DigitCoins:
                    return "Icons/Currency/digitcoin";
                case RewardType.DigitGems:
                    return "Icons/Currency/digitgem";
                default:
                    return $"Icons/Rewards/{reward.rewardId}";
            }
        }

        private int FindNextRewardLevel(int currentLevel)
        {
            var prog = PlayerProgressionSystem.Instance;
            if (prog == null) return 0;

            // Search forward for next reward
            for (int lvl = currentLevel + 1; lvl <= 500; lvl++)
            {
                if (prog.GetRewardForLevel(lvl) != null)
                    return lvl;
            }
            return 0;
        }

        #if UNITY_EDITOR
        [ContextMenu("Test Level Up (No Reward)")]
        private void TestLevelUpNoReward()
        {
            Show(14, 15, 0.35f);
        }

        [ContextMenu("Test Level Up (With Reward)")]
        private void TestLevelUpWithReward()
        {
            Show(49, 50, 0.1f, new LevelReward("Title: Veteran", RewardType.Title, "title_veteran"));
        }

        [ContextMenu("Test Milestone")]
        private void TestMilestone()
        {
            Show(99, 100, 0.05f, new LevelReward("Title: Centurion", RewardType.Title, "title_centurion"));
        }
        #endif

        #endregion
    }
}
