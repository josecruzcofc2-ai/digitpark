using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using DigitPark.Monetization;
using DigitPark.Localization;
using DigitPark.Services;
using DigitPark.Services.Firebase;
using DigitPark.UI;
using DG.Tweening;
using DigitPark.Animations;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager para la escena de recompensas diarias.
    /// Sistema de login rewards con racha de dias consecutivos.
    /// </summary>
    public class DailyRewardsManager : MonoBehaviour
    {
        [Header("UI - Header")]
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI streakText;
        [SerializeField] private TextMeshProUGUI nextResetText;

        [Header("UI - Current Day")]
        [SerializeField] private GameObject currentDayHighlight;
        [SerializeField] private TextMeshProUGUI currentDayText;
        [SerializeField] private Image currentDayRewardIcon;
        [SerializeField] private TextMeshProUGUI currentDayRewardText;

        [Header("UI - Rewards Grid")]
        [SerializeField] private Transform rewardsContainer;
        [SerializeField] private GameObject rewardDayPrefab;
        [SerializeField] private int daysInCycle = 7;

        [Header("UI - Claim Button")]
        [SerializeField] private Button claimButton;
        [SerializeField] private TextMeshProUGUI claimButtonText;
        [SerializeField] private GameObject claimGlow;

        [Header("UI - Bonus Info")]
        [SerializeField] private TextMeshProUGUI bonusInfoText;
        [SerializeField] private Slider streakProgressBar;
        [SerializeField] private TextMeshProUGUI streakBonusText;

        [Header("UI - Claim Animation (Clash Royale style)")]
        [SerializeField] private GameObject claimAnimationPanel;
        [SerializeField] private Image darkOverlayImage;
        [SerializeField] private Image giftBoxImage;
        [SerializeField] private Image giftGlowImage;
        [SerializeField] private Image lightBurstImage;
        [SerializeField] private TextMeshProUGUI celebTitleText;
        [SerializeField] private Image claimRewardIcon;
        [SerializeField] private TextMeshProUGUI claimRewardText;
        [SerializeField] private TextMeshProUGUI streakInfoText;
        [SerializeField] private ParticleSystem claimParticles;
        [SerializeField] private Button continueButton;

        [Header("UI - Streak Milestone")]
        [SerializeField] private GameObject milestonePanel;
        [SerializeField] private TextMeshProUGUI milestoneText;
        [SerializeField] private TextMeshProUGUI milestoneBonusText;

        [Header("Reward Icons")]
        [SerializeField] private Sprite coinIcon;
        [SerializeField] private Sprite gemIcon;
        [SerializeField] private Sprite xpIcon;
        [SerializeField] private Sprite mysteryIcon;

        [Header("Gift Box Icons (per-day)")]
        [SerializeField] private Sprite[] giftDayIcons = new Sprite[7]; // day1-7
        [SerializeField] private Sprite giftOpenBasicIcon;   // opened: days 1-4
        [SerializeField] private Sprite giftOpenPremiumIcon;  // opened: day 5
        [SerializeField] private Sprite giftOpenEpicIcon;     // opened: day 6

        [Header("Configuration")]
        [SerializeField] private List<DailyRewardConfig> rewards = new List<DailyRewardConfig>();
        [SerializeField] private int[] streakMilestones = { 7, 14, 30 };
        [SerializeField] private int[] milestoneBonuses = { 100, 250, 500 };

        // Neon theme colors
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_GLOW = new Color(0f, 1f, 1f, 0.15f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GOLD_GLOW = new Color(1f, 0.84f, 0f, 0.25f);
        private static readonly Color GREEN_CLAIMED = new Color(0.15f, 0.5f, 0.25f, 0.9f);
        private static readonly Color CARD_BG = new Color(0.06f, 0.08f, 0.12f, 1f);
        private static readonly Color CARD_BG_LOCKED = new Color(0.1f, 0.1f, 0.13f, 0.7f);
        private static readonly Color COIN_COLOR = new Color(1f, 0.85f, 0.3f, 1f);
        private static readonly Color GEM_COLOR = new Color(0.4f, 0.8f, 1f, 1f);
        private static readonly Color XP_COLOR = new Color(0.5f, 1f, 0.5f, 1f);
        private static readonly Color GREEN_SUCCESS = new Color(0.2f, 0.9f, 0.4f, 1f);
        private static readonly Color TEXT_WHITE = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.6f, 0.65f, 1f);
        private static readonly Color TEXT_DARK = new Color(0.05f, 0.05f, 0.08f, 1f);
        private static readonly Color LOCKED_OVERLAY = new Color(0.03f, 0.04f, 0.07f, 0.85f);

        // State
        private List<GameObject> spawnedDayItems = new List<GameObject>();
        private int currentStreak = 0;
        private int currentDayInCycle = 0;
        private bool canClaimToday = false;
        private DateTime lastClaimDate;
        private DailyRewardConfig todayReward;

        // Neon icon sprites (loaded from Resources)
        private Sprite coinIconNeon;
        private Sprite gemIconNeon;

        /// <summary>Returns the gift box sprite for a given day (1-7)</summary>
        private Sprite GetGiftIconForDay(int day)
        {
            int index = Mathf.Clamp(day - 1, 0, 6);
            return giftDayIcons != null && index < giftDayIcons.Length ? giftDayIcons[index] : null;
        }

        /// <summary>Returns the opened gift sprite for claim animation</summary>
        private Sprite GetGiftOpenIcon(int day) => day switch
        {
            1 or 2 or 3 or 4 => giftOpenBasicIcon,
            5 => giftOpenPremiumIcon,
            6 => giftOpenEpicIcon,
            7 => GetGiftIconForDay(7), // day7 chest is already open
            _ => giftOpenBasicIcon
        };

        /// <summary>Returns the gift state sprite (claimed days use same icon with tint, locked use same icon greyed)</summary>
        private Sprite GetGiftStateIcon(int day, bool claimed, bool locked)
        {
            return GetGiftIconForDay(day);
        }
        private PulseAnimation _claimPulse;
        private RectTransform _coinPillTarget;
        private RectTransform _gemPillTarget;
        private GameObject _spawnedDay7Card;

        private void Start()
        {
            EnsureServicesExist();
            LoadNeonIcons();
            InitializeRewards();
            LoadProgress();
            SetupUI();
            SetupListeners();
            CheckClaimStatus();
            PopulateRewardsGrid();
            SetupClaimPulse();
            FindCurrencyTargets();

            // Analytics: screen view
            AnalyticsService.Instance?.LogScreenView("DailyRewards");
        }

        /// <summary>
        /// Asegura que los servicios necesarios existan (para ejecucion directa sin Boot)
        /// </summary>
        private void EnsureServicesExist()
        {
            if (LocalizationManager.Instance == null)
            {
                GameObject locObj = new GameObject("LocalizationManager");
                locObj.AddComponent<LocalizationManager>();
                Debug.Log("[DailyRewards] LocalizationManager creado (ejecucion directa)");
            }

            // Auto-detect refs si no estan asignadas (ejecucion directa sin AutoAssigner)
            var canvas = UICanvasHelper.FindMainCanvas();
            if (canvas != null)
            {
                Transform r = canvas.transform;

                if (rewardsContainer == null)
                {
                    Transform grid = r.Find("DaysGrid");
                    if (grid != null) rewardsContainer = grid;
                }

                if (claimButton == null)
                {
                    Transform btn = r.Find("ClaimButton");
                    if (btn != null) claimButton = btn.GetComponent<Button>();
                }

                if (claimButtonText == null && claimButton != null)
                {
                    claimButtonText = claimButton.GetComponentInChildren<TextMeshProUGUI>();
                }

                if (nextResetText == null)
                {
                    Transform timer = r.Find("TimerBar/TimeText");
                    if (timer != null) nextResetText = timer.GetComponent<TextMeshProUGUI>();
                }

                if (streakText == null)
                {
                    Transform st = r.Find("StreakPanel/TopRow/StreakCount");
                    if (st != null) streakText = st.GetComponent<TextMeshProUGUI>();
                }

                if (backButton == null)
                {
                    Transform bb = r.Find("TopBar/BackButton");
                    if (bb != null) backButton = bb.GetComponent<Button>();
                }

                Debug.Log("[DailyRewards] Refs auto-detectadas para ejecucion directa");
            }
        }

        /// <summary>
        /// Carga iconos neon desde Resources
        /// </summary>
        private void LoadNeonIcons()
        {
            coinIconNeon = Resources.Load<Sprite>("Icons/DigitCoinIcon");
            gemIconNeon = Resources.Load<Sprite>("Icons/DigitGemIcon");
        }

        /// <summary>
        /// Helper de localizacion
        /// </summary>
        private string L(string key, params object[] args)
        {
            if (LocalizationManager.Instance == null) return key;
            return args.Length > 0
                ? LocalizationManager.Instance.GetText(key, args)
                : LocalizationManager.Instance.GetText(key);
        }

        /// <summary>
        /// Obtiene nombre localizado del tipo de recompensa
        /// </summary>
        private string GetRewardTypeName(string type)
        {
            return type switch
            {
                "coins" => L("reward_coins"),
                "gems" => L("reward_gems"),
                "xp" => L("reward_xp"),
                _ => type
            };
        }

        /// <summary>
        /// Obtiene el color del tipo de recompensa
        /// </summary>
        private Color GetRewardTypeColor(string type)
        {
            return type switch
            {
                "coins" => COIN_COLOR,
                "gems" => GEM_COLOR,
                "xp" => XP_COLOR,
                _ => Color.white
            };
        }

        private void InitializeRewards()
        {
            // Default rewards if not set in inspector
            if (rewards.Count == 0)
            {
                rewards = new List<DailyRewardConfig>
                {
                    new DailyRewardConfig { day = 1, type = "coins", amount = 50, name = "reward_coins" },
                    new DailyRewardConfig { day = 2, type = "coins", amount = 75, name = "reward_coins" },
                    new DailyRewardConfig { day = 3, type = "gems", amount = 5, name = "reward_gems" },
                    new DailyRewardConfig { day = 4, type = "coins", amount = 100, name = "reward_coins" },
                    new DailyRewardConfig { day = 5, type = "xp", amount = 200, name = "reward_xp" },
                    new DailyRewardConfig { day = 6, type = "coins", amount = 150, name = "reward_coins" },
                    new DailyRewardConfig { day = 7, type = "gems", amount = 25, name = "reward_gems", isSpecial = true },
                };

                daysInCycle = rewards.Count;
            }
        }

        private void LoadProgress()
        {
            currentStreak = PlayerPrefs.GetInt("DailyRewards_Streak", 0);
            currentDayInCycle = PlayerPrefs.GetInt("DailyRewards_CurrentDay", 0);

            string lastClaimStr = PlayerPrefs.GetString("DailyRewards_LastClaim", "");
            if (!string.IsNullOrEmpty(lastClaimStr))
            {
                lastClaimDate = DateTime.Parse(lastClaimStr);
            }
            else
            {
                lastClaimDate = DateTime.MinValue;
            }
        }

        private void SaveProgress()
        {
            PlayerPrefs.SetInt("DailyRewards_Streak", currentStreak);
            PlayerPrefs.SetInt("DailyRewards_CurrentDay", currentDayInCycle);
            PlayerPrefs.SetString("DailyRewards_LastClaim", lastClaimDate.ToString("yyyy-MM-dd"));
            PlayerPrefs.Save();
        }

        private void SetupUI()
        {
            if (claimAnimationPanel) claimAnimationPanel.SetActive(false);
            if (milestonePanel) milestonePanel.SetActive(false);

            UpdateStreakDisplay();
            UpdateNextResetTimer();
        }

        private void SetupListeners()
        {
            // Disable auto-navigation from BackButton prefab to prevent double listener
            var autoNav = backButton?.GetComponent<DigitPark.UI.BackButton>();
            if (autoNav != null) autoNav.DisableAutoNavigation();
            if (backButton) backButton.onClick.AddListener(OnBackClicked);
            if (claimButton) claimButton.onClick.AddListener(OnClaimClicked);
            if (continueButton) continueButton.onClick.AddListener(OnContinueClicked);
        }

        private void SetupClaimPulse()
        {
            if (claimButton == null) return;

            // Pulse animation
            _claimPulse = claimButton.gameObject.AddComponent<PulseAnimation>();
            if (claimGlow != null)
            {
                _claimPulse.GlowTarget = claimGlow;
            }
            _claimPulse.enabled = canClaimToday;
        }

        private void FindCurrencyTargets()
        {
            // Buscar currency pills del header (creados por UIBuilder)
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = UICanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var coinPill = FindDeepChild(canvas.transform, "CoinPill");
            if (coinPill != null) _coinPillTarget = coinPill.GetComponent<RectTransform>();

            var gemPill = FindDeepChild(canvas.transform, "GemPill");
            if (gemPill != null) _gemPillTarget = gemPill.GetComponent<RectTransform>();
        }

        private Transform FindDeepChild(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
                var result = FindDeepChild(child, name);
                if (result != null) return result;
            }
            return null;
        }

        private void CheckClaimStatus()
        {
            DateTime today = DateTime.Now.Date;
            DateTime lastClaim = lastClaimDate.Date;

            // Check if already claimed today
            if (lastClaim == today)
            {
                canClaimToday = false;
            }
            // Check if missed a day (streak broken)
            else if (lastClaim < today.AddDays(-1) && currentStreak > 0)
            {
                // Streak broken, reset
                currentStreak = 0;
                currentDayInCycle = 0;
                canClaimToday = true;
                Debug.Log("[DailyRewards] Streak broken, resetting");
            }
            else
            {
                canClaimToday = true;
            }

            // Get today's reward
            todayReward = rewards[currentDayInCycle % rewards.Count];

            UpdateClaimButton();
            UpdateCurrentDayDisplay();
        }

        private void UpdateStreakDisplay()
        {
            if (streakText)
            {
                streakText.text = L("dr_streak", currentStreak);
            }

            if (streakProgressBar)
            {
                int nextMilestone = GetNextMilestone();
                streakProgressBar.maxValue = nextMilestone;
                float targetValue = currentStreak % nextMilestone;
                DOTween.Kill(streakProgressBar);
                streakProgressBar.DOValue(targetValue, 0.6f).SetEase(Ease.OutQuad);

                // Crear/actualizar milestone markers
                UpdateStreakMilestoneMarkers(nextMilestone);
            }

            if (streakBonusText)
            {
                int nextMilestone = GetNextMilestone();
                int bonusIndex = GetMilestoneIndex(nextMilestone);
                if (bonusIndex >= 0 && bonusIndex < milestoneBonuses.Length)
                {
                    streakBonusText.text = L("dr_bonus_info", nextMilestone, milestoneBonuses[bonusIndex]);
                }
            }
        }

        private void UpdateStreakMilestoneMarkers(int maxMilestone)
        {
            if (streakProgressBar == null) return;
            var barRT = streakProgressBar.GetComponent<RectTransform>();
            if (barRT == null) return;

            foreach (int milestone in streakMilestones)
            {
                if (milestone > maxMilestone) break;

                string markerName = $"StreakMarker_{milestone}";
                var existing = streakProgressBar.transform.Find(markerName);

                GameObject markerObj;
                if (existing != null)
                {
                    markerObj = existing.gameObject;
                }
                else
                {
                    markerObj = new GameObject(markerName);
                    markerObj.transform.SetParent(streakProgressBar.transform, false);

                    var mRT = markerObj.AddComponent<RectTransform>();
                    float normalizedPos = (float)milestone / maxMilestone;
                    mRT.anchorMin = new Vector2(normalizedPos, 0.5f);
                    mRT.anchorMax = new Vector2(normalizedPos, 0.5f);
                    mRT.sizeDelta = new Vector2(22, 22);
                    mRT.anchoredPosition = Vector2.zero;

                    var mImg = markerObj.AddComponent<Image>();

                    var labelObj = new GameObject("Label");
                    labelObj.transform.SetParent(markerObj.transform, false);
                    var labelRT = labelObj.AddComponent<RectTransform>();
                    labelRT.anchorMin = Vector2.zero;
                    labelRT.anchorMax = Vector2.one;
                    labelRT.offsetMin = Vector2.zero;
                    labelRT.offsetMax = Vector2.zero;

                    var labelText = labelObj.AddComponent<TMPro.TextMeshProUGUI>();
                    labelText.text = milestone.ToString();
                    labelText.fontSize = FontSizes.Body;
                    labelText.fontStyle = TMPro.FontStyles.Bold;
                    labelText.alignment = TMPro.TextAlignmentOptions.Center;

                    // Label de recompensa debajo del marker
                    int mIdx = GetMilestoneIndex(milestone);
                    if (mIdx >= 0 && mIdx < milestoneBonuses.Length)
                    {
                        var rewardLabelObj = new GameObject("RewardLabel");
                        rewardLabelObj.transform.SetParent(markerObj.transform, false);
                        var rlRT = rewardLabelObj.AddComponent<RectTransform>();
                        rlRT.anchorMin = new Vector2(0.5f, 0f);
                        rlRT.anchorMax = new Vector2(0.5f, 0f);
                        rlRT.pivot = new Vector2(0.5f, 1f);
                        rlRT.sizeDelta = new Vector2(40, 12);
                        rlRT.anchoredPosition = new Vector2(0, -3);

                        var rlText = rewardLabelObj.AddComponent<TMPro.TextMeshProUGUI>();
                        rlText.text = $"+{milestoneBonuses[mIdx]}";
                        rlText.fontSize = FontSizes.Body;
                        rlText.color = new Color(0.4f, 0.8f, 1f); // GEM_COLOR
                        rlText.alignment = TMPro.TextAlignmentOptions.Center;
                        rlText.overflowMode = TMPro.TextOverflowModes.Overflow;
                    }
                }

                // Actualizar color basado en si se alcanzo
                bool reached = currentStreak >= milestone;
                var img = markerObj.GetComponent<Image>();
                if (img != null)
                    img.color = reached ? GOLD : new Color(0.15f, 0.15f, 0.2f);

                var txt = markerObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (txt != null)
                    txt.color = reached ? new Color(0.05f, 0.05f, 0.08f) : new Color(0.5f, 0.5f, 0.5f);
            }
        }

        private int GetNextMilestone()
        {
            foreach (int milestone in streakMilestones)
            {
                if (currentStreak < milestone) return milestone;
            }
            return streakMilestones[streakMilestones.Length - 1];
        }

        private int GetMilestoneIndex(int milestone)
        {
            for (int i = 0; i < streakMilestones.Length; i++)
            {
                if (streakMilestones[i] == milestone) return i;
            }
            return -1;
        }

        private void UpdateNextResetTimer()
        {
            DateTime tomorrow = DateTime.Now.Date.AddDays(1);
            TimeSpan timeUntilReset = tomorrow - DateTime.Now;

            if (nextResetText)
            {
                nextResetText.text = canClaimToday
                    ? L("dr_available_now")
                    : L("dr_next_in", UIPolish.FormatTimerHHMM(timeUntilReset.Hours, timeUntilReset.Minutes));
            }
        }

        private void UpdateClaimButton()
        {
            if (claimButton)
            {
                claimButton.interactable = canClaimToday;
            }

            if (claimButtonText)
            {
                claimButtonText.text = canClaimToday ? L("dr_claim_reward") : L("dr_claimed");
            }

            if (claimGlow)
            {
                claimGlow.SetActive(canClaimToday);
            }

            if (_claimPulse != null)
            {
                _claimPulse.enabled = canClaimToday;
                if (!canClaimToday) _claimPulse.StopPulse();
            }
        }

        private void UpdateCurrentDayDisplay()
        {
            int displayDay = currentDayInCycle + 1;

            if (currentDayText)
            {
                currentDayText.text = L("dr_day", displayDay);
            }

            if (todayReward != null)
            {
                if (currentDayRewardIcon)
                {
                    currentDayRewardIcon.sprite = GetRewardIcon(todayReward.type);
                }

                if (currentDayRewardText)
                {
                    currentDayRewardText.text = $"+{todayReward.amount} {GetRewardTypeName(todayReward.type)}";
                }
            }
        }

        private void PopulateRewardsGrid()
        {
            // Clean up UIBuilder static elements that cause overlap
            CleanupUIBuilderStaticElements();

            // Clear ALL children of the grid (including editor-created placeholders)
            if (rewardsContainer != null)
            {
                for (int i = rewardsContainer.childCount - 1; i >= 0; i--)
                {
                    Destroy(rewardsContainer.GetChild(i).gameObject);
                }
            }

            // Destroy previously spawned Day7 card (canvas sibling)
            if (_spawnedDay7Card != null)
            {
                Destroy(_spawnedDay7Card);
                _spawnedDay7Card = null;
            }

            foreach (var item in spawnedDayItems)
            {
                if (item) Destroy(item);
            }
            spawnedDayItems.Clear();

            // Create day items (days 1-6)
            for (int i = 0; i < rewards.Count; i++)
            {
                if (rewards[i].isSpecial)
                {
                    // Day 7 se crea con layout especial
                    CreateDay7Card(i, rewards[i]);
                }
                else
                {
                    CreateDayItem(i, rewards[i]);
                }
            }

            // Add PulseAnimation to current day card (subtle breathing effect)
            AddPulseToCurrentDayCard();

            // Animate rewards grid entrance
            AnimateRewardsGridEntrance();
        }

        /// <summary>
        /// Agrega PulseAnimation al card del dia actual para atraer atencion.
        /// </summary>
        private void AddPulseToCurrentDayCard()
        {
            if (rewardsContainer == null) return;

            // Find the grid index for the current day
            int gridIndex = currentDayInCycle;
            for (int i = 0; i <= currentDayInCycle && i < rewards.Count; i++)
            {
                if (rewards[i].isSpecial)
                    gridIndex--;
            }

            if (gridIndex >= 0 && gridIndex < rewardsContainer.childCount)
            {
                var currentCard = rewardsContainer.GetChild(gridIndex).gameObject;
                var pulse = currentCard.AddComponent<PulseAnimation>();
                // Subtle settings: min=0.98, max=1.02, speed=2
                pulse.enabled = canClaimToday;
            }
        }

        /// <summary>
        /// Elimina elementos estaticos del UIBuilder que causan overlap con el contenido dinamico.
        /// Incluye Day7Card, Day7Glow, y WeekLabel.
        /// </summary>
        private void CleanupUIBuilderStaticElements()
        {
            var canvas = UICanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            string[] elementsToRemove = { "Day7Card", "Day7Glow", "WeekLabel" };
            foreach (string name in elementsToRemove)
            {
                Transform element = canvas.transform.Find(name);
                if (element != null)
                {
                    Destroy(element.gameObject);
                }
            }
        }

        /// <summary>
        /// Anima la entrada de los items del grid de recompensas con efecto staggered.
        /// Day7 card aparece con delay adicional despues del grid.
        /// </summary>
        private void AnimateRewardsGridEntrance()
        {
            if (rewardsContainer == null || rewardsContainer.childCount == 0) return;

            var seq = DOTween.Sequence();
            float lastDelay = 0f;

            for (int i = 0; i < rewardsContainer.childCount; i++)
            {
                var child = rewardsContainer.GetChild(i);
                if (!child.gameObject.activeSelf) continue;
                var cg = child.GetComponent<CanvasGroup>();
                if (cg == null) cg = child.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                child.localScale = Vector3.one * 0.85f;
                float delay = i * 0.1f;
                seq.Insert(delay, cg.DOFade(1f, 0.3f).SetEase(Ease.OutQuad));
                seq.Insert(delay, child.DOScale(1f, 0.3f).SetEase(Ease.OutQuad));
                lastDelay = delay;
            }

            // Day7 card entrance with delay after grid completes
            if (_spawnedDay7Card != null)
            {
                var d7CG = _spawnedDay7Card.GetComponent<CanvasGroup>();
                if (d7CG == null) d7CG = _spawnedDay7Card.AddComponent<CanvasGroup>();
                d7CG.alpha = 0f;
                _spawnedDay7Card.transform.localScale = Vector3.one * 0.85f;

                float day7Delay = lastDelay + 0.5f;
                seq.Insert(day7Delay, d7CG.DOFade(1f, 0.35f).SetEase(Ease.OutQuad));
                seq.Insert(day7Delay, _spawnedDay7Card.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack));
                seq.InsertCallback(day7Delay + 0.35f, () =>
                {
                    if (_spawnedDay7Card != null)
                        ScalePunch.Play(_spawnedDay7Card, 1.05f, 0.25f);
                });
            }
        }

        /// <summary>
        /// Anima la transicion de claim sin reconstruir todo el grid.
        /// El card reclamado transiciona a "claimed", el siguiente a "today".
        /// </summary>
        private void AnimateClaimTransition(int claimedDayIndex)
        {
            if (rewardsContainer == null) return;

            // Find the claimed card in the grid (non-special days only, indexed 0-5)
            int gridIndex = claimedDayIndex;
            // Account for the fact special day (7) is not in the grid
            for (int i = 0; i <= claimedDayIndex; i++)
            {
                if (i < rewards.Count && rewards[i].isSpecial)
                    gridIndex--;
            }

            // --- Animate claimed card to "claimed" state ---
            if (gridIndex >= 0 && gridIndex < rewardsContainer.childCount)
            {
                var claimedCard = rewardsContainer.GetChild(gridIndex).gameObject;
                var cardImage = claimedCard.GetComponent<Image>();
                var cardOutline = claimedCard.GetComponent<Outline>();

                // Background → GREEN_CLAIMED
                if (cardImage != null)
                    cardImage.DOColor(GREEN_CLAIMED, 0.4f);

                // Outline → GREEN_SUCCESS
                if (cardOutline != null)
                {
                    DOTween.To(() => cardOutline.effectColor, c => cardOutline.effectColor = c,
                        GREEN_SUCCESS, 0.4f);
                    cardOutline.effectDistance = new Vector2(1, 1);
                }

                // DayLabel color → GREEN_SUCCESS
                var dayLabelTMP = claimedCard.transform.Find("DayLabel")?.GetComponent<TextMeshProUGUI>();
                if (dayLabelTMP != null)
                    dayLabelTMP.DOColor(GREEN_SUCCESS, 0.4f);

                // Fade icon to 40% opacity
                var iconImg = claimedCard.transform.Find("RewardIcon")?.GetComponent<Image>();
                if (iconImg != null)
                    iconImg.DOFade(0.4f, 0.3f);

                // Fade amount text to 50% opacity
                var amountTMP = claimedCard.transform.Find("AmountText")?.GetComponent<TextMeshProUGUI>();
                if (amountTMP != null)
                    amountTMP.DOFade(0.5f, 0.3f);

                // Remove IconGlow if present
                var iconGlow = claimedCard.transform.Find("RewardIcon/IconGlow");
                if (iconGlow != null)
                {
                    var igImg = iconGlow.GetComponent<Image>();
                    if (igImg != null)
                        igImg.DOFade(0f, 0.2f).OnComplete(() => Destroy(iconGlow.gameObject));
                }

                // Remove TodayBadge with scale-out
                var todayBadge = claimedCard.transform.Find("TodayBadge");
                if (todayBadge != null)
                {
                    todayBadge.DOScale(0f, 0.2f).SetEase(Ease.InBack)
                        .OnComplete(() => Destroy(todayBadge.gameObject));
                }

                // Remove PulseAnimation
                var pulse = claimedCard.GetComponent<PulseAnimation>();
                if (pulse != null)
                {
                    pulse.StopPulse();
                    Destroy(pulse);
                }

                // Create CheckOverlay with bounce-in
                var check = new GameObject("CheckOverlay");
                check.transform.SetParent(claimedCard.transform, false);
                var chRT = check.AddComponent<RectTransform>();
                chRT.anchorMin = new Vector2(1, 1);
                chRT.anchorMax = new Vector2(1, 1);
                chRT.pivot = new Vector2(1, 1);
                chRT.anchoredPosition = new Vector2(-4, -4);
                chRT.sizeDelta = new Vector2(26, 26);
                check.AddComponent<Image>().color = GREEN_SUCCESS;

                var checkText = new GameObject("Text");
                checkText.transform.SetParent(check.transform, false);
                var ctRT = checkText.AddComponent<RectTransform>();
                ctRT.anchorMin = Vector2.zero;
                ctRT.anchorMax = Vector2.one;
                ctRT.offsetMin = Vector2.zero;
                ctRT.offsetMax = Vector2.zero;
                var ctTMP = checkText.AddComponent<TextMeshProUGUI>();
                ctTMP.text = "\u2713";
                ctTMP.fontSize = FontSizes.Body;
                ctTMP.fontStyle = FontStyles.Bold;
                ctTMP.color = TEXT_DARK;
                ctTMP.alignment = TextAlignmentOptions.Center;

                check.transform.localScale = Vector3.zero;
                check.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetDelay(0.2f);
            }

            // --- Animate next day card to "today" state ---
            int nextDayIndex = claimedDayIndex + 1;
            if (nextDayIndex < rewards.Count && !rewards[nextDayIndex].isSpecial)
            {
                int nextGridIndex = nextDayIndex;
                for (int i = 0; i <= nextDayIndex; i++)
                {
                    if (i < rewards.Count && rewards[i].isSpecial)
                        nextGridIndex--;
                }

                if (nextGridIndex >= 0 && nextGridIndex < rewardsContainer.childCount)
                {
                    var nextCard = rewardsContainer.GetChild(nextGridIndex).gameObject;
                    var nextOutline = nextCard.GetComponent<Outline>();

                    // Outline → GOLD
                    if (nextOutline != null)
                    {
                        DOTween.To(() => nextOutline.effectColor, c => nextOutline.effectColor = c,
                            GOLD, 0.4f);
                        nextOutline.effectDistance = new Vector2(2, 2);
                    }

                    // DayLabel color → GOLD
                    var nextDayLabel = nextCard.transform.Find("DayLabel")?.GetComponent<TextMeshProUGUI>();
                    if (nextDayLabel != null)
                        nextDayLabel.DOColor(GOLD, 0.4f);

                    // Remove LockOverlay if present
                    var lockOverlay = nextCard.transform.Find("LockOverlay");
                    if (lockOverlay != null)
                    {
                        var loCG = lockOverlay.gameObject.AddComponent<CanvasGroup>();
                        loCG.DOFade(0f, 0.3f).OnComplete(() => Destroy(lockOverlay.gameObject));
                    }

                    // Create TodayBadge with bounce-in
                    var badge = new GameObject("TodayBadge");
                    badge.transform.SetParent(nextCard.transform, false);
                    var bdRT = badge.AddComponent<RectTransform>();
                    bdRT.anchorMin = new Vector2(0.5f, 1);
                    bdRT.anchorMax = new Vector2(0.5f, 1);
                    bdRT.pivot = new Vector2(0.5f, 1);
                    bdRT.anchoredPosition = new Vector2(0, 2);
                    bdRT.sizeDelta = new Vector2(80, 22);
                    badge.AddComponent<Image>().color = GOLD;

                    var badgeText = new GameObject("Text");
                    badgeText.transform.SetParent(badge.transform, false);
                    var bttRT = badgeText.AddComponent<RectTransform>();
                    bttRT.anchorMin = Vector2.zero;
                    bttRT.anchorMax = Vector2.one;
                    bttRT.offsetMin = Vector2.zero;
                    bttRT.offsetMax = Vector2.zero;
                    var bttTMP = badgeText.AddComponent<TextMeshProUGUI>();
                    bttTMP.text = L("dr_today");
                    bttTMP.fontSize = FontSizes.Body;
                    bttTMP.fontStyle = FontStyles.Bold;
                    bttTMP.color = TEXT_DARK;
                    bttTMP.alignment = TextAlignmentOptions.Center;

                    badge.transform.localScale = Vector3.zero;
                    badge.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetDelay(0.3f);

                    // Add PulseAnimation to new "today" card
                    var nextPulse = nextCard.AddComponent<PulseAnimation>();
                    nextPulse.enabled = true;

                    // Create IconGlow with fade-in
                    var iconContainer = nextCard.transform.Find("RewardIcon");
                    if (iconContainer != null)
                    {
                        var iconGlow = new GameObject("IconGlow");
                        iconGlow.transform.SetParent(iconContainer, false);
                        iconGlow.transform.SetAsFirstSibling();
                        var igRT = iconGlow.AddComponent<RectTransform>();
                        igRT.anchorMin = Vector2.zero;
                        igRT.anchorMax = Vector2.one;
                        igRT.offsetMin = new Vector2(-10, -10);
                        igRT.offsetMax = new Vector2(10, 10);
                        var igImg = iconGlow.AddComponent<Image>();
                        igImg.color = new Color(GOLD.r, GOLD.g, GOLD.b, 0f);
                        igImg.raycastTarget = false;
                        igImg.DOFade(0.15f, 0.3f).SetDelay(0.3f);
                    }
                }
            }
        }

        private void CreateDayItem(int dayIndex, DailyRewardConfig reward)
        {
            GameObject item;

            if (rewardDayPrefab != null)
            {
                item = Instantiate(rewardDayPrefab, rewardsContainer);
            }
            else
            {
                item = CreateDayItemFallback(dayIndex, reward);
            }

            spawnedDayItems.Add(item);
        }

        private GameObject CreateDayItemFallback(int dayIndex, DailyRewardConfig reward)
        {
            bool isClaimed = dayIndex < currentDayInCycle;
            bool isToday = dayIndex == currentDayInCycle;
            bool isLocked = dayIndex > currentDayInCycle;

            // Card container - let GridLayoutGroup handle size
            var card = new GameObject($"Day_{dayIndex + 1}");
            card.transform.SetParent(rewardsContainer, false);
            card.AddComponent<RectTransform>();

            var image = card.AddComponent<Image>();

            if (isClaimed)
                image.color = GREEN_CLAIMED;
            else if (isToday)
                image.color = CARD_BG;
            else
                image.color = CARD_BG_LOCKED;

            // Outline (state-colored, matching UIBuilder)
            var outline = card.AddComponent<Outline>();
            if (isClaimed)
            {
                outline.effectColor = GREEN_SUCCESS;
                outline.effectDistance = new Vector2(1, 1);
            }
            else if (isToday)
            {
                outline.effectColor = GOLD;
                outline.effectDistance = new Vector2(2, 2);
            }
            else
            {
                outline.effectColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
                outline.effectDistance = new Vector2(1, 1);
            }

            // Shadow for 3D depth
            var shadow = card.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            shadow.effectDistance = new Vector2(3, -4);

            // VLG for card content (matching UIBuilder pattern)
            var vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 4;
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Day Label
            var dayLabel = new GameObject("DayLabel");
            dayLabel.transform.SetParent(card.transform, false);
            dayLabel.AddComponent<RectTransform>();
            dayLabel.AddComponent<LayoutElement>().preferredHeight = 42;
            var dlTMP = dayLabel.AddComponent<TextMeshProUGUI>();
            dlTMP.text = L("dr_day", dayIndex + 1);
            dlTMP.fontSize = FontSizes.Body;
            dlTMP.fontStyle = FontStyles.Bold;
            dlTMP.alignment = TextAlignmentOptions.Center;
            dlTMP.color = isClaimed ? GREEN_SUCCESS : (isToday ? GOLD : TEXT_WHITE);
            dlTMP.enableAutoSizing = true;
            dlTMP.fontSizeMin = FontSizes.AutoMinBody;
            dlTMP.fontSizeMax = FontSizes.Body;
            dlTMP.overflowMode = TextOverflowModes.Ellipsis;

            // Reward Icon
            var iconContainer = new GameObject("RewardIcon");
            iconContainer.transform.SetParent(card.transform, false);
            iconContainer.AddComponent<RectTransform>();
            var iconLE = iconContainer.AddComponent<LayoutElement>();
            iconLE.preferredHeight = 60;
            iconLE.preferredWidth = 60;
            var iconImg = iconContainer.AddComponent<Image>();
            iconImg.sprite = GetRewardIcon(reward.type);
            iconImg.preserveAspect = true;
            if (isClaimed) iconImg.color = new Color(1f, 1f, 1f, 0.4f);

            // Gold glow behind icon for current day
            if (isToday)
            {
                var iconGlow = new GameObject("IconGlow");
                iconGlow.transform.SetParent(iconContainer.transform, false);
                iconGlow.transform.SetAsFirstSibling();
                var igRT = iconGlow.AddComponent<RectTransform>();
                igRT.anchorMin = Vector2.zero;
                igRT.anchorMax = Vector2.one;
                igRT.offsetMin = new Vector2(-10, -10);
                igRT.offsetMax = new Vector2(10, 10);
                var igImg = iconGlow.AddComponent<Image>();
                igImg.color = new Color(GOLD.r, GOLD.g, GOLD.b, 0.15f);
                igImg.raycastTarget = false;
            }

            // Amount Text
            var amountObj = new GameObject("AmountText");
            amountObj.transform.SetParent(card.transform, false);
            amountObj.AddComponent<RectTransform>();
            amountObj.AddComponent<LayoutElement>().preferredHeight = 42;
            var amTMP = amountObj.AddComponent<TextMeshProUGUI>();
            amTMP.text = $"+{reward.amount}";
            amTMP.fontSize = FontSizes.Body;
            amTMP.fontStyle = FontStyles.Bold;
            amTMP.alignment = TextAlignmentOptions.Center;
            amTMP.overflowMode = TextOverflowModes.Ellipsis;
            amTMP.enableAutoSizing = true;
            amTMP.fontSizeMin = FontSizes.AutoMinBody;
            amTMP.fontSizeMax = FontSizes.Body;
            amTMP.color = isClaimed ? new Color(1f, 1f, 1f, 0.5f) : GetRewardTypeColor(reward.type);

            // --- Status Overlays ---

            if (isClaimed)
            {
                // Green check overlay (top-right)
                var check = new GameObject("CheckOverlay");
                check.transform.SetParent(card.transform, false);
                var chRT = check.AddComponent<RectTransform>();
                chRT.anchorMin = new Vector2(1, 1);
                chRT.anchorMax = new Vector2(1, 1);
                chRT.pivot = new Vector2(1, 1);
                chRT.anchoredPosition = new Vector2(-4, -4);
                chRT.sizeDelta = new Vector2(26, 26);
                check.AddComponent<Image>().color = GREEN_SUCCESS;

                var checkText = new GameObject("Text");
                checkText.transform.SetParent(check.transform, false);
                var ctRT = checkText.AddComponent<RectTransform>();
                ctRT.anchorMin = Vector2.zero;
                ctRT.anchorMax = Vector2.one;
                ctRT.offsetMin = Vector2.zero;
                ctRT.offsetMax = Vector2.zero;
                var ctTMP = checkText.AddComponent<TextMeshProUGUI>();
                ctTMP.text = "\u2713";
                ctTMP.fontSize = FontSizes.Body;
                ctTMP.fontStyle = FontStyles.Bold;
                ctTMP.color = TEXT_DARK;
                ctTMP.alignment = TextAlignmentOptions.Center;
            }
            else if (isToday)
            {
                // TODAY badge (top-center)
                var badge = new GameObject("TodayBadge");
                badge.transform.SetParent(card.transform, false);
                var bdRT = badge.AddComponent<RectTransform>();
                bdRT.anchorMin = new Vector2(0.5f, 1);
                bdRT.anchorMax = new Vector2(0.5f, 1);
                bdRT.pivot = new Vector2(0.5f, 1);
                bdRT.anchoredPosition = new Vector2(0, 2);
                bdRT.sizeDelta = new Vector2(80, 22);
                badge.AddComponent<Image>().color = GOLD;

                var badgeText = new GameObject("Text");
                badgeText.transform.SetParent(badge.transform, false);
                var bttRT = badgeText.AddComponent<RectTransform>();
                bttRT.anchorMin = Vector2.zero;
                bttRT.anchorMax = Vector2.one;
                bttRT.offsetMin = Vector2.zero;
                bttRT.offsetMax = Vector2.zero;
                var bttTMP = badgeText.AddComponent<TextMeshProUGUI>();
                bttTMP.text = L("dr_today");
                bttTMP.fontSize = FontSizes.Body;
                bttTMP.fontStyle = FontStyles.Bold;
                bttTMP.color = TEXT_DARK;
                bttTMP.alignment = TextAlignmentOptions.Center;
            }
            else if (isLocked)
            {
                // Lock overlay (full dark overlay + lock icon)
                var lockOverlay = new GameObject("LockOverlay");
                lockOverlay.transform.SetParent(card.transform, false);
                var loRT = lockOverlay.AddComponent<RectTransform>();
                loRT.anchorMin = Vector2.zero;
                loRT.anchorMax = Vector2.one;
                loRT.offsetMin = Vector2.zero;
                loRT.offsetMax = Vector2.zero;
                lockOverlay.AddComponent<Image>().color = LOCKED_OVERLAY;

                // Lock icon
                var lockIcon = new GameObject("LockIcon");
                lockIcon.transform.SetParent(lockOverlay.transform, false);
                var liRT = lockIcon.AddComponent<RectTransform>();
                liRT.anchorMin = new Vector2(0.5f, 0.5f);
                liRT.anchorMax = new Vector2(0.5f, 0.5f);
                liRT.sizeDelta = new Vector2(30, 30);
                var liImg = lockIcon.AddComponent<Image>();
                liImg.preserveAspect = true;
                Sprite lockSprite = Resources.Load<Sprite>("Icons/LockIcon");
                if (lockSprite != null) { liImg.sprite = lockSprite; liImg.color = Color.white; }
                else liImg.color = TEXT_SECONDARY;
            }

            return card;
        }

        /// <summary>
        /// Crea el card especial de Day 7 como hermano de DaysGrid (no hijo),
        /// posicionado con anchors que coinciden con el UIBuilder.
        /// </summary>
        private void CreateDay7Card(int dayIndex, DailyRewardConfig reward)
        {
            bool isClaimed = dayIndex < currentDayInCycle;
            bool isToday = dayIndex == currentDayInCycle;
            int daysUntil = dayIndex - currentDayInCycle;

            // Create as sibling of DaysGrid (canvas child), not inside the grid
            var canvas = UICanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            var item = new GameObject("Day7_GrandPrize");
            item.transform.SetParent(canvas.transform, false);

            // Anchors matching UIBuilder DAY7 region
            var rt = item.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.023f, 0.455f);
            rt.anchorMax = new Vector2(0.977f, 0.558f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var image = item.AddComponent<Image>();
            image.color = isClaimed ? GREEN_CLAIMED : CARD_BG;

            // Outline (gold, matching UIBuilder)
            var outline = item.AddComponent<Outline>();
            if (isClaimed)
            {
                outline.effectColor = GREEN_SUCCESS;
                outline.effectDistance = new Vector2(1, 1);
            }
            else
            {
                outline.effectColor = isToday ? GOLD : new Color(GOLD.r, GOLD.g, GOLD.b, 0.4f);
                outline.effectDistance = new Vector2(2, 2);
            }

            // Shadow for 3D depth
            var shadow = item.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            shadow.effectDistance = new Vector2(3, -4);

            // HLG layout: IconArea left + Info VLG right (matching UIBuilder)
            var hlg = item.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.padding = new RectOffset(12, 12, 8, 8);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            // Icon area (left side)
            var iconObj = new GameObject("RewardIcon");
            iconObj.transform.SetParent(item.transform, false);
            iconObj.AddComponent<RectTransform>();
            var iconLE = iconObj.AddComponent<LayoutElement>();
            iconLE.preferredWidth = 70;
            iconLE.preferredHeight = 70;
            var iconImage = iconObj.AddComponent<Image>();
            iconImage.sprite = GetRewardIcon(reward.type);
            iconImage.preserveAspect = true;
            if (isClaimed) iconImage.color = new Color(1f, 1f, 1f, 0.4f);

            // Info VLG (right side)
            var infoObj = new GameObject("Info");
            infoObj.transform.SetParent(item.transform, false);
            infoObj.AddComponent<RectTransform>();
            var infoLE = infoObj.AddComponent<LayoutElement>();
            infoLE.flexibleWidth = 1;
            var infoVLG = infoObj.AddComponent<VerticalLayoutGroup>();
            infoVLG.spacing = 2;
            infoVLG.childAlignment = TextAnchor.MiddleLeft;
            infoVLG.childControlWidth = true;
            infoVLG.childControlHeight = false;
            infoVLG.childForceExpandWidth = true;
            infoVLG.childForceExpandHeight = false;

            // Title "DAY 7 - GRAND PRIZE"
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(infoObj.transform, false);
            titleObj.AddComponent<RectTransform>();
            titleObj.AddComponent<LayoutElement>().preferredHeight = 38;
            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = L("dr_grand_prize");
            titleText.fontSize = FontSizes.Body;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.color = isClaimed ? new Color(0.5f, 0.5f, 0.5f) : GOLD;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = FontSizes.AutoMinBody;
            titleText.fontSizeMax = FontSizes.Body;
            titleText.overflowMode = TextOverflowModes.Ellipsis;

            // Reward details
            var detailObj = new GameObject("Details");
            detailObj.transform.SetParent(infoObj.transform, false);
            detailObj.AddComponent<RectTransform>();
            detailObj.AddComponent<LayoutElement>().preferredHeight = 38;
            var detailText = detailObj.AddComponent<TextMeshProUGUI>();
            detailText.text = $"+{reward.amount} {GetRewardTypeName(reward.type)}";
            detailText.fontSize = FontSizes.Body;
            detailText.fontStyle = FontStyles.Bold;
            detailText.alignment = TextAlignmentOptions.Left;
            detailText.color = isClaimed ? new Color(0.5f, 0.5f, 0.5f) : GetRewardTypeColor(reward.type);
            detailText.enableAutoSizing = true;
            detailText.fontSizeMin = FontSizes.AutoMinBody;
            detailText.fontSizeMax = FontSizes.Body;
            detailText.overflowMode = TextOverflowModes.Ellipsis;

            // Status / countdown
            var statusObj = new GameObject("Status");
            statusObj.transform.SetParent(infoObj.transform, false);
            statusObj.AddComponent<RectTransform>();
            statusObj.AddComponent<LayoutElement>().preferredHeight = 32;
            var statusText = statusObj.AddComponent<TextMeshProUGUI>();
            if (isClaimed)
            {
                statusText.text = L("ms_completed");
                statusText.color = GREEN_SUCCESS;
            }
            else if (isToday)
            {
                statusText.text = L("dr_today");
                statusText.color = GOLD;
            }
            else
            {
                statusText.text = L("dr_unlocks_in", daysUntil);
                statusText.color = TEXT_SECONDARY;
            }
            statusText.fontSize = FontSizes.Body;
            statusText.alignment = TextAlignmentOptions.Left;
            statusText.enableAutoSizing = true;
            statusText.fontSizeMin = FontSizes.AutoMinBody;
            statusText.fontSizeMax = FontSizes.Body;
            statusText.overflowMode = TextOverflowModes.Ellipsis;

            // Shimmer effect premium (solo si no reclamado)
            if (!isClaimed)
            {
                item.AddComponent<ShimmerEffect>();
            }

            _spawnedDay7Card = item;
            spawnedDayItems.Add(item);
        }

        private Sprite GetRewardIcon(string type)
        {
            // Preferir iconos neon si estan disponibles
            return type switch
            {
                "coins" => coinIconNeon != null ? coinIconNeon : coinIcon,
                "gems" => gemIconNeon != null ? gemIconNeon : gemIcon,
                "xp" => xpIcon,
                _ => mysteryIcon
            };
        }

        private void OnClaimClicked()
        {
            if (!canClaimToday) return;

            // Claim the reward
            ClaimTodayReward();
        }

        private void ClaimTodayReward()
        {
            if (todayReward == null) return;

            // Apply reward
            ApplyReward(todayReward);

            // Capture before state update for animation
            int claimedDayIndex = currentDayInCycle;

            // Update state
            currentStreak++;
            currentDayInCycle = (currentDayInCycle + 1) % rewards.Count;
            lastClaimDate = DateTime.Now;
            canClaimToday = false;

            SaveProgress();

            // Scale punch feedback
            if (claimButton != null)
                ScalePunch.Play(claimButton.gameObject, 1.15f, 0.3f);

            // Coin/gem fly animation hacia el currency pill
            LaunchCoinFly(todayReward.type);

            // Show claim animation (pass original day index before increment)
            ShowClaimAnimation(todayReward, claimedDayIndex);

            // Check for milestone
            CheckMilestone();

            // Analytics: track daily reward claimed
            AnalyticsService.Instance?.LogDailyRewardClaimed(
                currentDayInCycle,
                todayReward.type,
                todayReward.amount
            );

            // Haptic feedback
#if UNITY_IOS
            Handheld.Vibrate();
#elif UNITY_ANDROID
            Handheld.Vibrate();
#endif

            // Update UI
            UpdateStreakDisplay();
            UpdateClaimButton();
            UpdateCurrentDayDisplay();
            AnimateClaimTransition(claimedDayIndex);

            // Achievement tracking
            AchievementService.Instance?.OnDailyRewardClaimed(currentStreak);

            Debug.Log($"[DailyRewards] Claimed day {currentDayInCycle}, streak: {currentStreak}");
        }

        private void LaunchCoinFly(string rewardType)
        {
            if (claimButton == null) return;
            var originRT = claimButton.GetComponent<RectTransform>();

            RectTransform targetRT = null;
            if (rewardType == "gems" && _gemPillTarget != null)
                targetRT = _gemPillTarget;
            else if (_coinPillTarget != null)
                targetRT = _coinPillTarget;

            if (targetRT != null)
            {
                Sprite flyIcon = GetRewardIcon(rewardType);
                CoinFlyAnimation.Play(originRT, targetRT, rewardType, 8, 0.7f, flyIcon);
            }
        }

        private void ApplyReward(DailyRewardConfig reward)
        {
            switch (reward.type)
            {
                case "coins":
                    int currentCoins = PlayerPrefs.GetInt("PlayerCoins", 0);
                    PlayerPrefs.SetInt("PlayerCoins", currentCoins + reward.amount);
                    break;

                case "gems":
                    int currentGems = PlayerPrefs.GetInt("PlayerGems", 0);
                    PlayerPrefs.SetInt("PlayerGems", currentGems + reward.amount);
                    break;

                case "xp":
                    int currentXP = PlayerPrefs.GetInt("PlayerXP", 0);
                    PlayerPrefs.SetInt("PlayerXP", currentXP + reward.amount);
                    break;

            }

            PlayerPrefs.Save();

            // Analytics: track virtual currency earned
            AnalyticsService.Instance?.LogVirtualCurrencyEarned(
                reward.type,
                reward.amount,
                "daily_reward"
            );
        }

        /// <summary>
        /// Clash Royale-style gift opening animation sequence:
        /// 1. Dark overlay fades in
        /// 2. Gift box bounces into center (closed)
        /// 3. Gift glow pulses behind it
        /// 4. Gift shakes with anticipation
        /// 5. Light burst flash + sprite swap to opened gift
        /// 6. Rewards float in from above
        /// 7. Particles burst
        /// 8. "TAP TO CONTINUE" fades in
        /// </summary>
        private void ShowClaimAnimation(DailyRewardConfig reward, int claimedDayIndex)
        {
            if (claimAnimationPanel == null) return;

            // Set reward data
            if (claimRewardText)
                claimRewardText.text = $"+{reward.amount} {GetRewardTypeName(reward.type)}";
            if (claimRewardIcon)
                claimRewardIcon.sprite = GetRewardIcon(reward.type);
            if (streakInfoText)
                streakInfoText.text = L("dr_streak", currentStreak);
            if (celebTitleText)
                celebTitleText.text = L("dr_reward_obtained");

            // Set closed gift icon for the claimed day (1-indexed for icon methods)
            int claimedDay = claimedDayIndex + 1;
            Sprite closedGift = GetGiftIconForDay(claimedDay);
            Sprite openedGift = GetGiftOpenIcon(claimedDay);
            if (giftBoxImage && closedGift) giftBoxImage.sprite = closedGift;

            // Prepare initial states
            claimAnimationPanel.SetActive(true);

            // Dark overlay: start invisible
            if (darkOverlayImage)
            {
                var doCG = darkOverlayImage.GetComponent<CanvasGroup>();
                if (doCG == null) doCG = darkOverlayImage.gameObject.AddComponent<CanvasGroup>();
                doCG.alpha = 0f;
            }

            // Gift box: start invisible and small
            if (giftBoxImage)
            {
                giftBoxImage.transform.localScale = Vector3.zero;
                giftBoxImage.color = Color.white;
            }

            // Gift glow: start invisible
            if (giftGlowImage)
            {
                giftGlowImage.transform.localScale = Vector3.zero;
                giftGlowImage.color = new Color(GOLD.r, GOLD.g, GOLD.b, 0f);
            }

            // Light burst: start invisible
            if (lightBurstImage)
                lightBurstImage.color = new Color(1f, 0.95f, 0.8f, 0f);

            // Reward container: start invisible
            Transform rewardContainer = claimAnimationPanel.transform.Find("RewardContainer");
            CanvasGroup rcCG = null;
            if (rewardContainer)
            {
                rcCG = rewardContainer.GetComponent<CanvasGroup>();
                if (rcCG == null) rcCG = rewardContainer.gameObject.AddComponent<CanvasGroup>();
                rcCG.alpha = 0f;
                rewardContainer.localScale = Vector3.one * 0.5f;
            }

            // Continue button: start invisible
            if (continueButton)
            {
                var cbCG = continueButton.GetComponent<CanvasGroup>();
                if (cbCG == null) cbCG = continueButton.gameObject.AddComponent<CanvasGroup>();
                cbCG.alpha = 0f;
                continueButton.interactable = false;
            }

            // === BUILD THE SEQUENCE ===
            var seq = DOTween.Sequence();

            // Phase 1 (0.0s): Dark overlay fades in
            if (darkOverlayImage)
            {
                var doCG = darkOverlayImage.GetComponent<CanvasGroup>();
                seq.Insert(0f, doCG.DOFade(1f, 0.35f).SetEase(Ease.OutQuad));
            }

            // Phase 2 (0.2s): Gift box bounces in (0 → 1.25 → 1.0)
            if (giftBoxImage)
            {
                seq.Insert(0.2f, giftBoxImage.transform.DOScale(1.25f, 0.45f).SetEase(Ease.OutBack));
                seq.Insert(0.65f, giftBoxImage.transform.DOScale(1f, 0.15f).SetEase(Ease.InOutQuad));
            }

            // Phase 2b (0.3s): Gift glow fades in and starts pulsing
            if (giftGlowImage)
            {
                seq.Insert(0.3f, giftGlowImage.transform.DOScale(1f, 0.4f).SetEase(Ease.OutQuad));
                seq.Insert(0.3f, giftGlowImage.DOFade(0.25f, 0.4f));
                // Continuous rotation for magical feel
                seq.InsertCallback(0.7f, () =>
                {
                    if (giftGlowImage)
                        giftGlowImage.transform.DORotate(new Vector3(0, 0, 360), 8f, RotateMode.FastBeyond360)
                            .SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
                });
            }

            // Phase 3 (1.0s): Gift shakes with anticipation
            if (giftBoxImage)
            {
                seq.Insert(1.0f, giftBoxImage.transform.DOShakeRotation(0.6f, new Vector3(0, 0, 18f), 14, 90f, true));
                // Subtle scale pumps during shake
                seq.Insert(1.0f, giftBoxImage.transform.DOPunchScale(new Vector3(0.08f, 0.08f, 0), 0.6f, 8, 0.5f));
            }

            // Phase 4 (1.7s): Light burst flash
            if (lightBurstImage)
            {
                seq.Insert(1.7f, lightBurstImage.DOFade(0.85f, 0.12f).SetEase(Ease.OutQuad));
                seq.Insert(1.82f, lightBurstImage.DOFade(0f, 0.4f).SetEase(Ease.InQuad));
            }

            // Phase 4b (1.75s): Gift sprite swap to OPENED + scale punch
            if (giftBoxImage)
            {
                seq.InsertCallback(1.75f, () =>
                {
                    if (giftBoxImage && openedGift)
                        giftBoxImage.sprite = openedGift;
                });
                seq.Insert(1.75f, giftBoxImage.transform.DOPunchScale(new Vector3(0.3f, 0.3f, 0), 0.4f, 6, 0.5f));
            }

            // Phase 5 (2.0s): Rewards container slides in from above
            if (rewardContainer && rcCG)
            {
                seq.Insert(2.0f, rcCG.DOFade(1f, 0.35f).SetEase(Ease.OutQuad));
                seq.Insert(2.0f, rewardContainer.DOScale(1f, 0.4f).SetEase(Ease.OutBack));
            }

            // Phase 5b (2.1s): Particles burst
            seq.InsertCallback(2.1f, () =>
            {
                if (claimParticles) claimParticles.Play();
            });

            // Phase 6 (2.5s): Gift glow intensifies
            if (giftGlowImage)
            {
                seq.Insert(2.0f, giftGlowImage.DOFade(0.4f, 0.3f));
                seq.Insert(2.0f, giftGlowImage.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutQuad));
            }

            // Phase 7 (2.8s): "TAP TO CONTINUE" fades in + enable interaction
            if (continueButton)
            {
                var cbCG = continueButton.GetComponent<CanvasGroup>();
                seq.Insert(2.8f, cbCG.DOFade(1f, 0.4f));
                // Pulse the continue text
                seq.InsertCallback(3.2f, () =>
                {
                    if (continueButton)
                    {
                        continueButton.interactable = true;
                        // Gentle breathing pulse on continue text
                        continueButton.transform.DOScale(1.05f, 0.8f)
                            .SetLoops(-1, LoopType.Yoyo)
                            .SetEase(Ease.InOutSine);
                    }
                });
            }
        }

        private void CheckMilestone()
        {
            foreach (int milestone in streakMilestones)
            {
                if (currentStreak == milestone)
                {
                    int bonusIndex = GetMilestoneIndex(milestone);
                    if (bonusIndex >= 0 && bonusIndex < milestoneBonuses.Length)
                    {
                        int bonus = milestoneBonuses[bonusIndex];
                        ApplyMilestoneBonus(bonus);
                        ShowMilestonePopup(milestone, bonus);
                    }
                    break;
                }
            }
        }

        private void ApplyMilestoneBonus(int gemBonus)
        {
            int currentGems = PlayerPrefs.GetInt("PlayerGems", 0);
            PlayerPrefs.SetInt("PlayerGems", currentGems + gemBonus);
            PlayerPrefs.Save();

            // Analytics
            AnalyticsService.Instance?.LogVirtualCurrencyEarned("gems", gemBonus, "daily_milestone");

            Debug.Log($"[DailyRewards] Milestone bonus applied: +{gemBonus} gems");
        }

        private void ShowMilestonePopup(int days, int bonus)
        {
            if (milestonePanel)
            {
                if (milestoneText)
                {
                    milestoneText.text = L("dr_milestone_days", days);
                }

                if (milestoneBonusText)
                {
                    milestoneBonusText.text = L("dr_milestone_bonus_gems", bonus);
                }

                AnimatePanelIn(milestonePanel);
            }
        }

        private void OnContinueClicked()
        {
            if (claimAnimationPanel && claimAnimationPanel.activeSelf)
                DismissClaimAnimation();
            if (milestonePanel && milestonePanel.activeSelf)
                AnimatePanelOut(milestonePanel);
        }

        /// <summary>
        /// Clash Royale dismiss: gift shrinks, rewards fade, overlay dims out.
        /// </summary>
        private void DismissClaimAnimation()
        {
            if (claimAnimationPanel == null) return;

            // Kill all ongoing tweens on claim animation children
            DOTween.Kill(claimAnimationPanel.transform, true);
            if (giftBoxImage) DOTween.Kill(giftBoxImage.transform);
            if (giftGlowImage) DOTween.Kill(giftGlowImage.transform);
            if (continueButton) DOTween.Kill(continueButton.transform);

            var dismissSeq = DOTween.Sequence();

            // Gift box shrinks and fades
            if (giftBoxImage)
            {
                dismissSeq.Insert(0f, giftBoxImage.transform.DOScale(0.3f, 0.3f).SetEase(Ease.InBack));
                dismissSeq.Insert(0f, giftBoxImage.DOFade(0f, 0.25f));
            }

            // Gift glow fades
            if (giftGlowImage)
            {
                dismissSeq.Insert(0f, giftGlowImage.DOFade(0f, 0.25f));
                dismissSeq.Insert(0f, giftGlowImage.transform.DOScale(0.5f, 0.3f));
            }

            // Reward container scales down and fades
            Transform rewardContainer = claimAnimationPanel.transform.Find("RewardContainer");
            if (rewardContainer)
            {
                var rcCG = rewardContainer.GetComponent<CanvasGroup>();
                if (rcCG == null) rcCG = rewardContainer.gameObject.AddComponent<CanvasGroup>();
                dismissSeq.Insert(0.05f, rcCG.DOFade(0f, 0.2f));
                dismissSeq.Insert(0.05f, rewardContainer.DOScale(0.8f, 0.25f).SetEase(Ease.InQuad));
            }

            // Continue button fades
            if (continueButton)
            {
                var cbCG = continueButton.GetComponent<CanvasGroup>();
                if (cbCG) dismissSeq.Insert(0f, cbCG.DOFade(0f, 0.15f));
                continueButton.interactable = false;
            }

            // Dark overlay fades out last
            if (darkOverlayImage)
            {
                var doCG = darkOverlayImage.GetComponent<CanvasGroup>();
                if (doCG) dismissSeq.Insert(0.15f, doCG.DOFade(0f, 0.25f));
            }

            // Stop particles
            if (claimParticles) claimParticles.Stop();

            // Hide panel when done
            dismissSeq.OnComplete(() =>
            {
                claimAnimationPanel.SetActive(false);
            });
        }

        private void OnEnable()
        {
            InvokeRepeating(nameof(UpdateNextResetTimer), 0f, 1f);
        }

        private void OnDisable()
        {
            CancelInvoke(nameof(UpdateNextResetTimer));
        }

        private void AnimatePanelIn(GameObject panel)
        {
            if (panel == null) return;
            panel.SetActive(true);
            CanvasGroup cg = panel.GetComponent<CanvasGroup>();
            if (cg == null) cg = panel.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            panel.transform.localScale = Vector3.one * 0.85f;
            DOTween.Kill(panel.transform);
            panel.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
            cg.DOFade(1f, 0.25f);
        }

        private void AnimatePanelOut(GameObject panel, Action onComplete = null)
        {
            if (panel == null) { onComplete?.Invoke(); return; }
            CanvasGroup cg = panel.GetComponent<CanvasGroup>();
            if (cg == null) cg = panel.AddComponent<CanvasGroup>();
            DOTween.Kill(panel.transform);
            panel.transform.DOScale(0.9f, 0.2f).SetEase(Ease.InQuad);
            cg.DOFade(0f, 0.2f).OnComplete(() =>
            {
                panel.SetActive(false);
                cg.alpha = 1f;
                panel.transform.localScale = Vector3.one;
                onComplete?.Invoke();
            });
        }

        private void OnBackClicked()
        {
            SceneNavigator.Instance?.GoBack();
        }
    }

    [Serializable]
    public class DailyRewardConfig
    {
        public int day;
        public string type;
        public int amount;
        public string name;
        public bool isSpecial;
    }
}
