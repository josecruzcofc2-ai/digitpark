using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DigitPark.Services;
using DigitPark.Themes;
using DigitPark.Localization;
using DigitPark.UI;

namespace DigitPark.UI.Builders
{
    /// <summary>
    /// Builder para el panel de recompensas diarias
    /// Muestra los 7 días del ciclo y permite reclamar
    /// </summary>
    public class DailyRewardPanelBuilder : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button claimButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI streakText;
        [SerializeField] private TextMeshProUGUI timerText;

        [Header("Day Items")]
        [SerializeField] private Transform daysContainer;
        [SerializeField] private GameObject dayItemPrefab;

        [Header("Claim Animation")]
        [SerializeField] private GameObject rewardPopup;
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private Image rewardIcon;

        [Header("Visual Settings")]
        [SerializeField] private Color claimedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color currentColor = new Color(0f, 1f, 1f, 1f);
        [SerializeField] private Color futureColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        [SerializeField] private Color specialColor = new Color(1f, 0.8f, 0f, 1f);

        private DailyRewardDayItem[] _dayItems;
        private Coroutine _timerCoroutine;

        private void OnEnable()
        {
            if (DailyRewardService.Instance != null)
            {
                DailyRewardService.Instance.OnRewardClaimed += OnRewardClaimed;
            }
            ThemeManager.OnThemeChanged += ApplyTheme;
            LocalizationManager.OnLanguageChanged += UpdateTexts;

            RefreshUI();
            StartTimer();
        }

        private void OnDisable()
        {
            if (DailyRewardService.Instance != null)
            {
                DailyRewardService.Instance.OnRewardClaimed -= OnRewardClaimed;
            }
            ThemeManager.OnThemeChanged -= ApplyTheme;
            LocalizationManager.OnLanguageChanged -= UpdateTexts;

            StopTimer();
        }

        private void Start()
        {
            SetupButtons();
            GenerateDayItems();
            RefreshUI();
            ApplyTheme(ThemeManager.Instance?.CurrentTheme);
        }

        #region Setup

        private void SetupButtons()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(Hide);
            }

            if (claimButton != null)
            {
                claimButton.onClick.RemoveAllListeners();
                claimButton.onClick.AddListener(ClaimReward);
            }
        }

        private void GenerateDayItems()
        {
            if (daysContainer == null) return;

            // Limpiar existentes
            foreach (Transform child in daysContainer)
            {
                Destroy(child.gameObject);
            }

            var rewards = DailyRewardService.Instance?.GetAllRewards();
            if (rewards == null || rewards.Count == 0) return;

            _dayItems = new DailyRewardDayItem[rewards.Count];

            for (int i = 0; i < rewards.Count; i++)
            {
                GameObject item;
                if (dayItemPrefab != null)
                {
                    item = Instantiate(dayItemPrefab, daysContainer);
                }
                else
                {
                    item = CreateDefaultDayItem();
                    item.transform.SetParent(daysContainer, false);
                }

                var dayItem = item.GetComponent<DailyRewardDayItem>();
                if (dayItem == null)
                {
                    dayItem = item.AddComponent<DailyRewardDayItem>();
                }

                dayItem.Setup(rewards[i], i + 1);
                _dayItems[i] = dayItem;
            }
        }

        private GameObject CreateDefaultDayItem()
        {
            GameObject item = new GameObject("DayItem");

            // RectTransform
            RectTransform rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(80, 100);

            // Layout Element
            LayoutElement le = item.AddComponent<LayoutElement>();
            le.preferredWidth = 80;
            le.preferredHeight = 100;

            // Background
            Image bg = item.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.2f, 1f);

            // Day number
            GameObject dayObj = new GameObject("DayNumber");
            dayObj.transform.SetParent(item.transform, false);

            RectTransform dayRt = dayObj.AddComponent<RectTransform>();
            dayRt.anchorMin = new Vector2(0, 0.7f);
            dayRt.anchorMax = new Vector2(1, 1f);
            dayRt.offsetMin = Vector2.zero;
            dayRt.offsetMax = Vector2.zero;

            TextMeshProUGUI dayText = dayObj.AddComponent<TextMeshProUGUI>();
            dayText.text = AutoLocalizer.Get("dr_day", 1);
            dayText.fontSize = FontSizes.Body;
            dayText.alignment = TextAlignmentOptions.Center;
            dayText.color = Color.white;

            // Reward amount
            GameObject amountObj = new GameObject("Amount");
            amountObj.transform.SetParent(item.transform, false);

            RectTransform amountRt = amountObj.AddComponent<RectTransform>();
            amountRt.anchorMin = new Vector2(0, 0.3f);
            amountRt.anchorMax = new Vector2(1, 0.7f);
            amountRt.offsetMin = Vector2.zero;
            amountRt.offsetMax = Vector2.zero;

            TextMeshProUGUI amountText = amountObj.AddComponent<TextMeshProUGUI>();
            amountText.text = "100";
            amountText.fontSize = FontSizes.Body;
            amountText.fontStyle = FontStyles.Bold;
            amountText.alignment = TextAlignmentOptions.Center;
            amountText.color = Color.cyan;

            // Status indicator
            GameObject statusObj = new GameObject("Status");
            statusObj.transform.SetParent(item.transform, false);

            RectTransform statusRt = statusObj.AddComponent<RectTransform>();
            statusRt.anchorMin = new Vector2(0.5f, 0);
            statusRt.anchorMax = new Vector2(0.5f, 0.3f);
            statusRt.sizeDelta = new Vector2(20, 20);

            Image statusImg = statusObj.AddComponent<Image>();
            statusImg.color = Color.gray;

            return item;
        }

        #endregion

        #region UI Updates

        public void RefreshUI()
        {
            if (DailyRewardService.Instance == null) return;

            UpdateTexts();
            UpdateDayItems();
            UpdateClaimButton();
        }

        private void UpdateTexts()
        {
            if (titleText != null)
            {
                titleText.text = AutoLocalizer.Get("daily_rewards");
            }

            if (streakText != null)
            {
                int streak = DailyRewardService.Instance?.ConsecutiveDays ?? 0;
                streakText.text = string.Format(AutoLocalizer.Get("streak_days"), streak);
            }
        }

        private void UpdateDayItems()
        {
            if (_dayItems == null || DailyRewardService.Instance == null) return;

            int currentDay = DailyRewardService.Instance.CurrentDay;
            bool canClaim = DailyRewardService.Instance.HasRewardAvailable;

            for (int i = 0; i < _dayItems.Length; i++)
            {
                int day = i + 1;
                DayItemState state;

                if (day < currentDay)
                {
                    state = DayItemState.Claimed;
                }
                else if (day == currentDay)
                {
                    state = canClaim ? DayItemState.Available : DayItemState.Claimed;
                }
                else
                {
                    state = DayItemState.Locked;
                }

                _dayItems[i].SetState(state, GetColorForState(state, _dayItems[i].IsSpecial));
            }
        }

        private Color GetColorForState(DayItemState state, bool isSpecial)
        {
            if (isSpecial && state == DayItemState.Available)
            {
                return specialColor;
            }

            return state switch
            {
                DayItemState.Claimed => claimedColor,
                DayItemState.Available => currentColor,
                DayItemState.Locked => futureColor,
                _ => futureColor
            };
        }

        private void UpdateClaimButton()
        {
            if (claimButton == null) return;

            bool canClaim = DailyRewardService.Instance?.HasRewardAvailable ?? false;
            claimButton.interactable = canClaim;

            var btnText = claimButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = canClaim
                    ? AutoLocalizer.Get("claim")
                    : AutoLocalizer.Get("claimed");
            }
        }

        #endregion

        #region Timer

        private void StartTimer()
        {
            StopTimer();
            _timerCoroutine = StartCoroutine(UpdateTimerCoroutine());
        }

        private void StopTimer()
        {
            if (_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine);
                _timerCoroutine = null;
            }
        }

        private IEnumerator UpdateTimerCoroutine()
        {
            while (true)
            {
                UpdateTimer();
                yield return new WaitForSeconds(1f);
            }
        }

        private void UpdateTimer()
        {
            if (timerText == null || DailyRewardService.Instance == null) return;

            if (DailyRewardService.Instance.HasRewardAvailable)
            {
                timerText.text = AutoLocalizer.Get("available_now");
            }
            else
            {
                var timeLeft = DailyRewardService.Instance.GetTimeUntilNextReward();
                timerText.text = $"{timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}";
            }
        }

        #endregion

        #region Actions

        private void ClaimReward()
        {
            if (DailyRewardService.Instance == null) return;

            bool success = DailyRewardService.Instance.ClaimReward();
            if (success)
            {
                // La animación se maneja en OnRewardClaimed
            }
        }

        private void OnRewardClaimed(DailyReward reward, int day)
        {
            // Mostrar popup de recompensa
            ShowRewardPopup(reward);

            // Actualizar UI
            RefreshUI();
        }

        private void ShowRewardPopup(DailyReward reward)
        {
            if (rewardPopup != null)
            {
                rewardPopup.SetActive(true);

                if (rewardText != null)
                {
                    rewardText.text = $"+{reward.amount}\n{reward.description}";
                }

                if (rewardIcon != null && reward.icon != null)
                {
                    rewardIcon.sprite = reward.icon;
                    rewardIcon.gameObject.SetActive(true);
                }

                // Auto-ocultar después de 2 segundos
                StartCoroutine(HideRewardPopupDelayed(2f));
            }
        }

        private IEnumerator HideRewardPopupDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (rewardPopup != null)
            {
                rewardPopup.SetActive(false);
            }
        }

        #endregion

        #region Show/Hide

        public void Show()
        {
            if (panel != null)
            {
                panel.SetActive(true);
                RefreshUI();
            }
            else
            {
                gameObject.SetActive(true);
            }
        }

        public void Hide()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        #endregion

        #region Theme

        private void ApplyTheme(ThemeData theme)
        {
            if (theme == null) return;

            if (titleText != null)
                titleText.color = theme.textTitle;

            if (streakText != null)
                streakText.color = theme.primaryAccent;

            if (timerText != null)
                timerText.color = theme.textSecondary;

            currentColor = theme.primaryAccent;
        }

        #endregion
    }

    #region Day Item Component

    public enum DayItemState
    {
        Locked,
        Available,
        Claimed
    }

    /// <summary>
    /// Componente para cada día en el panel de recompensas
    /// </summary>
    public class DailyRewardDayItem : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private TextMeshProUGUI dayText;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private Image statusIcon;
        [SerializeField] private GameObject checkmark;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private GameObject glowEffect;

        private DailyReward _reward;
        private int _day;

        public bool IsSpecial => _reward?.isSpecial ?? false;

        public void Setup(DailyReward reward, int day)
        {
            _reward = reward;
            _day = day;

            // Auto-detectar componentes
            if (background == null)
                background = GetComponent<Image>();

            if (dayText == null)
                dayText = transform.Find("DayNumber")?.GetComponent<TextMeshProUGUI>();

            if (amountText == null)
                amountText = transform.Find("Amount")?.GetComponent<TextMeshProUGUI>();

            // Configurar textos
            if (dayText != null)
                dayText.text = AutoLocalizer.Get("dr_day", day);

            if (amountText != null)
                amountText.text = reward.amount.ToString();
        }

        public void SetState(DayItemState state, Color color)
        {
            if (background != null)
            {
                background.color = state == DayItemState.Available
                    ? new Color(color.r, color.g, color.b, 0.3f)
                    : new Color(0.1f, 0.1f, 0.15f, 1f);
            }

            if (amountText != null)
            {
                amountText.color = color;
            }

            // Checkmark para reclamados
            if (checkmark != null)
            {
                checkmark.SetActive(state == DayItemState.Claimed);
            }

            // Lock para futuros
            if (lockIcon != null)
            {
                lockIcon.SetActive(state == DayItemState.Locked);
            }

            // Glow para disponible
            if (glowEffect != null)
            {
                glowEffect.SetActive(state == DayItemState.Available);
            }

            // Outline para día actual
            var outline = GetComponent<Outline>();
            if (state == DayItemState.Available)
            {
                if (outline == null)
                    outline = gameObject.AddComponent<Outline>();

                outline.effectColor = color;
                outline.effectDistance = new Vector2(2, 2);
                outline.enabled = true;
            }
            else if (outline != null)
            {
                outline.enabled = false;
            }
        }
    }

    #endregion
}
