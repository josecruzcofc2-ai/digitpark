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

        [Header("UI - Claim Animation")]
        [SerializeField] private GameObject claimAnimationPanel;
        [SerializeField] private TextMeshProUGUI claimRewardText;
        [SerializeField] private Image claimRewardIcon;
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
        private PulseAnimation _claimPulse;
        private RectTransform _coinPillTarget;
        private RectTransform _gemPillTarget;

        private void Start()
        {
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
        /// Carga iconos neon desde Resources
        /// </summary>
        private void LoadNeonIcons()
        {
            coinIconNeon = Resources.Load<Sprite>("Icons/CoinIconNeon");
            gemIconNeon = Resources.Load<Sprite>("Icons/GemIconNeon");
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
            if (backButton) backButton.onClick.AddListener(OnBackClicked);
            if (claimButton) claimButton.onClick.AddListener(OnClaimClicked);
            if (continueButton) continueButton.onClick.AddListener(OnContinueClicked);
        }

        private void SetupClaimPulse()
        {
            if (claimButton == null) return;

            // Rounded corners en el boton de claim
            var claimImage = claimButton.GetComponent<Image>();
            if (claimImage != null) UIPolish.ApplyRoundedCorners(claimImage);

            // Pulse animation
            _claimPulse = claimButton.gameObject.AddComponent<PulseAnimation>();
            if (claimGlow != null)
            {
                // Rounded corners en el glow
                var glowImage = claimGlow.GetComponent<Image>();
                if (glowImage != null) UIPolish.ApplyRoundedCorners(glowImage);
                _claimPulse.GlowTarget = claimGlow;
            }
            _claimPulse.enabled = canClaimToday;
        }

        private void FindCurrencyTargets()
        {
            // Buscar currency pills del header (creados por UIBuilder)
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindObjectOfType<Canvas>();
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
                streakProgressBar.value = currentStreak % nextMilestone;

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
                    UIPolish.ApplyRoundedCorners(mImg, 4);

                    var labelObj = new GameObject("Label");
                    labelObj.transform.SetParent(markerObj.transform, false);
                    var labelRT = labelObj.AddComponent<RectTransform>();
                    labelRT.anchorMin = Vector2.zero;
                    labelRT.anchorMax = Vector2.one;
                    labelRT.offsetMin = Vector2.zero;
                    labelRT.offsetMax = Vector2.zero;

                    var labelText = labelObj.AddComponent<TMPro.TextMeshProUGUI>();
                    labelText.text = milestone.ToString();
                    labelText.fontSize = 9;
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
                        rlText.fontSize = 8;
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
            // Clear existing
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

            // Animate rewards grid entrance
            AnimateRewardsGridEntrance();
        }

        /// <summary>
        /// Anima la entrada de los items del grid de recompensas con efecto staggered
        /// </summary>
        private void AnimateRewardsGridEntrance()
        {
            if (rewardsContainer == null || rewardsContainer.childCount == 0) return;

            var seq = DOTween.Sequence();
            for (int i = 0; i < rewardsContainer.childCount; i++)
            {
                var child = rewardsContainer.GetChild(i);
                if (!child.gameObject.activeSelf) continue;
                var cg = child.GetComponent<CanvasGroup>();
                if (cg == null) cg = child.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                child.localScale = Vector3.one * 0.85f;
                float delay = i * 0.05f;
                seq.Insert(delay, cg.DOFade(1f, 0.3f).SetEase(Ease.OutQuad));
                seq.Insert(delay, child.DOScale(1f, 0.3f).SetEase(Ease.OutQuad));
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
            var item = new GameObject($"Day_{dayIndex + 1}");
            item.transform.SetParent(rewardsContainer, false);

            var rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(100, 120);

            var image = item.AddComponent<Image>();
            UIPolish.ApplyRoundedCorners(image);

            bool isClaimed = dayIndex < currentDayInCycle;
            bool isToday = dayIndex == currentDayInCycle;

            // Colors con estilo neon
            if (isClaimed)
                image.color = GREEN_CLAIMED;
            else if (isToday)
                image.color = CARD_BG;
            else
                image.color = CARD_BG_LOCKED;

            // Borde glow cyan redondeado para el dia actual
            if (isToday && canClaimToday)
            {
                UIPolish.CreateRoundedGlowBorder(item.transform, CYAN_NEON, 14, 3f);
            }

            // Day label
            var dayLabelObj = new GameObject("DayLabel");
            dayLabelObj.transform.SetParent(item.transform, false);
            var dayLabelRT = dayLabelObj.AddComponent<RectTransform>();
            dayLabelRT.anchorMin = new Vector2(0, 0.78f);
            dayLabelRT.anchorMax = new Vector2(1, 1);
            dayLabelRT.offsetMin = new Vector2(5, 0);
            dayLabelRT.offsetMax = new Vector2(-5, -3);

            var dayLabelText = dayLabelObj.AddComponent<TextMeshProUGUI>();
            dayLabelText.text = L("dr_day", dayIndex + 1);
            dayLabelText.fontSize = 11;
            dayLabelText.fontStyle = FontStyles.Bold;
            dayLabelText.alignment = TextAlignmentOptions.Center;
            dayLabelText.color = isToday ? CYAN_NEON : new Color(0.6f, 0.6f, 0.65f);

            // Icono de reward (sprite real en vez de emoji)
            var iconObj = new GameObject("RewardIcon");
            iconObj.transform.SetParent(item.transform, false);
            var iconRT = iconObj.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.2f, 0.35f);
            iconRT.anchorMax = new Vector2(0.8f, 0.78f);
            iconRT.offsetMin = Vector2.zero;
            iconRT.offsetMax = Vector2.zero;

            var iconImage = iconObj.AddComponent<Image>();
            iconImage.sprite = GetRewardIcon(reward.type);
            iconImage.preserveAspect = true;
            if (isClaimed) iconImage.color = new Color(1f, 1f, 1f, 0.4f);

            // Cantidad con color de tipo
            var amountObj = new GameObject("Amount");
            amountObj.transform.SetParent(item.transform, false);
            var amountRT = amountObj.AddComponent<RectTransform>();
            amountRT.anchorMin = new Vector2(0, 0.15f);
            amountRT.anchorMax = new Vector2(1, 0.38f);
            amountRT.offsetMin = new Vector2(3, 0);
            amountRT.offsetMax = new Vector2(-3, 0);

            var amountText = amountObj.AddComponent<TextMeshProUGUI>();
            amountText.text = $"+{reward.amount}";
            amountText.fontSize = 13;
            amountText.fontStyle = FontStyles.Bold;
            amountText.alignment = TextAlignmentOptions.Center;
            amountText.color = isClaimed ? new Color(0.5f, 0.5f, 0.5f) : GetRewardTypeColor(reward.type);

            // Status indicator
            var statusObj = new GameObject("Status");
            statusObj.transform.SetParent(item.transform, false);
            var statusRT = statusObj.AddComponent<RectTransform>();
            statusRT.anchorMin = new Vector2(0, 0);
            statusRT.anchorMax = new Vector2(1, 0.18f);
            statusRT.offsetMin = new Vector2(3, 2);
            statusRT.offsetMax = new Vector2(-3, 0);

            var statusText = statusObj.AddComponent<TextMeshProUGUI>();
            if (isClaimed)
            {
                statusText.text = "OK";
                statusText.color = new Color(0.2f, 0.9f, 0.4f);
            }
            else if (isToday && canClaimToday)
            {
                statusText.text = L("dr_today");
                statusText.color = CYAN_NEON;
            }
            else
            {
                statusText.text = "";
            }
            statusText.fontSize = 11;
            statusText.fontStyle = FontStyles.Bold;
            statusText.alignment = TextAlignmentOptions.Center;

            return item;
        }

        /// <summary>
        /// Crea el card especial de Day 7 con mayor impacto visual
        /// </summary>
        private void CreateDay7Card(int dayIndex, DailyRewardConfig reward)
        {
            bool isClaimed = dayIndex < currentDayInCycle;
            bool isToday = dayIndex == currentDayInCycle;
            int daysUntil = dayIndex - currentDayInCycle;

            var item = new GameObject("Day7_GrandPrize");
            item.transform.SetParent(rewardsContainer, false);

            var rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(320, 140);

            var image = item.AddComponent<Image>();
            UIPolish.ApplyRoundedCorners(image);
            image.color = isClaimed ? GREEN_CLAIMED : CARD_BG;

            // Borde dorado glow redondeado
            if (!isClaimed)
            {
                Color borderColor = isToday ? GOLD : new Color(GOLD.r, GOLD.g, GOLD.b, 0.4f);
                UIPolish.CreateRoundedGlowBorder(item.transform, borderColor, 14, 3f);
            }

            // Titulo "DIA 7 - GRAN PREMIO"
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(item.transform, false);
            var titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.3f, 0.7f);
            titleRT.anchorMax = new Vector2(1f, 1f);
            titleRT.offsetMin = new Vector2(5, 0);
            titleRT.offsetMax = new Vector2(-10, -5);

            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = L("dr_grand_prize");
            titleText.fontSize = 16;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.color = isClaimed ? new Color(0.5f, 0.5f, 0.5f) : GOLD;

            // Icono grande
            var iconObj = new GameObject("RewardIcon");
            iconObj.transform.SetParent(item.transform, false);
            var iconRT = iconObj.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.02f, 0.1f);
            iconRT.anchorMax = new Vector2(0.28f, 0.9f);
            iconRT.offsetMin = Vector2.zero;
            iconRT.offsetMax = Vector2.zero;

            var iconImage = iconObj.AddComponent<Image>();
            iconImage.sprite = GetRewardIcon(reward.type);
            iconImage.preserveAspect = true;
            if (isClaimed) iconImage.color = new Color(1f, 1f, 1f, 0.4f);

            // Reward details
            var detailObj = new GameObject("Details");
            detailObj.transform.SetParent(item.transform, false);
            var detailRT = detailObj.AddComponent<RectTransform>();
            detailRT.anchorMin = new Vector2(0.3f, 0.25f);
            detailRT.anchorMax = new Vector2(1f, 0.7f);
            detailRT.offsetMin = new Vector2(5, 0);
            detailRT.offsetMax = new Vector2(-10, 0);

            var detailText = detailObj.AddComponent<TextMeshProUGUI>();
            detailText.text = $"+{reward.amount} {GetRewardTypeName(reward.type)}";
            detailText.fontSize = 18;
            detailText.fontStyle = FontStyles.Bold;
            detailText.alignment = TextAlignmentOptions.Left;
            detailText.color = isClaimed ? new Color(0.5f, 0.5f, 0.5f) : GetRewardTypeColor(reward.type);

            // Status / countdown
            var statusObj = new GameObject("Status");
            statusObj.transform.SetParent(item.transform, false);
            var statusRT = statusObj.AddComponent<RectTransform>();
            statusRT.anchorMin = new Vector2(0.3f, 0f);
            statusRT.anchorMax = new Vector2(1f, 0.28f);
            statusRT.offsetMin = new Vector2(5, 3);
            statusRT.offsetMax = new Vector2(-10, 0);

            var statusText = statusObj.AddComponent<TextMeshProUGUI>();
            if (isClaimed)
            {
                statusText.text = L("ms_completed");
                statusText.color = new Color(0.2f, 0.9f, 0.4f);
            }
            else if (isToday)
            {
                statusText.text = L("dr_today");
                statusText.color = GOLD;
            }
            else
            {
                statusText.text = L("dr_unlocks_in", daysUntil);
                statusText.color = new Color(0.6f, 0.6f, 0.65f);
            }
            statusText.fontSize = 12;
            statusText.alignment = TextAlignmentOptions.Left;

            // Shimmer effect premium (solo si no reclamado)
            if (!isClaimed)
            {
                item.AddComponent<ShimmerEffect>();
            }

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

            // Show claim animation
            ShowClaimAnimation(todayReward);

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
            PopulateRewardsGrid();

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

        private void ShowClaimAnimation(DailyRewardConfig reward)
        {
            if (claimAnimationPanel)
            {
                claimAnimationPanel.SetActive(true);

                if (claimRewardText)
                {
                    claimRewardText.text = $"+{reward.amount} {GetRewardTypeName(reward.type)}";
                }

                if (claimRewardIcon)
                {
                    claimRewardIcon.sprite = GetRewardIcon(reward.type);
                }

                if (claimParticles)
                {
                    claimParticles.Play();
                }
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
                milestonePanel.SetActive(true);

                if (milestoneText)
                {
                    milestoneText.text = L("dr_milestone_days", days);
                }

                if (milestoneBonusText)
                {
                    milestoneBonusText.text = L("dr_milestone_bonus_gems", bonus);
                }
            }
        }

        private void OnContinueClicked()
        {
            if (claimAnimationPanel) claimAnimationPanel.SetActive(false);
            if (milestonePanel) milestonePanel.SetActive(false);
        }

        private void OnEnable()
        {
            InvokeRepeating(nameof(UpdateNextResetTimer), 0f, 1f);
        }

        private void OnDisable()
        {
            CancelInvoke(nameof(UpdateNextResetTimer));
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
