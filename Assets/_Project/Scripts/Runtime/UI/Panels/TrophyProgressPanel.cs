using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Services;
using DigitPark.Progression;
using DigitPark.Localization;

namespace DigitPark.UI
{
    /// <summary>
    /// Economy Rebalance V55 — "My Trophies" Panel
    ///
    /// Shows progress toward 4 earnable themes (trophy-only, not purchasable):
    /// - Emerald:         365 days login (dedication)
    /// - Electric Blue:   1,000 ranked wins (volume + skill)
    /// - Electric Violet: 100 perfect scores (pure skill)
    /// - Monochrome:      Level 50 (progression)
    ///
    /// Each bar shows: theme preview (locked/unlocked) + progress bar + percentage + current/target.
    /// Accessible from Profile panel.
    /// </summary>
    public class TrophyProgressPanel : MonoBehaviour
    {
        [Header("Trophy Bars")]
        [SerializeField] private TrophyBar emeraldBar;
        [SerializeField] private TrophyBar electricBlueBar;
        [SerializeField] private TrophyBar electricVioletBar;
        [SerializeField] private TrophyBar monochromeBar;

        [Header("Panel")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Button closeButton;

        private void OnEnable()
        {
            RefreshAll();
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(Hide);
            }
        }

        public void Show()
        {
            if (panel != null) panel.SetActive(true);
            else gameObject.SetActive(true);
            RefreshAll();
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
            else gameObject.SetActive(false);
        }

        public void RefreshAll()
        {
            // Emerald — 365 days login (non-consecutive)
            int totalLoginDays = PlayerPrefs.GetInt("DP_TotalLoginDays", 0);
            UpdateBar(emeraldBar, "Emerald", "emerald_trophy",
                totalLoginDays, 365,
                AutoLocalizer.Get("trophy_emerald_desc"),
                new Color(0.063f, 0.725f, 0.506f));

            // Electric Blue — 1,000 ranked wins
            int rankedWins = PlayerPrefs.GetInt("DP_RankedWins_Total", 0);
            UpdateBar(electricBlueBar, "Electric Blue", "electric_blue_trophy",
                rankedWins, 1000,
                AutoLocalizer.Get("trophy_electric_blue_desc"),
                new Color(0.231f, 0.51f, 0.965f));

            // Electric Violet — 100 perfect scores
            int perfectScores = PlayerPrefs.GetInt("DP_PerfectScores_Total", 0);
            UpdateBar(electricVioletBar, "Electric Violet", "electric_violet_trophy",
                perfectScores, 100,
                AutoLocalizer.Get("trophy_electric_violet_desc"),
                new Color(0.659f, 0.333f, 0.969f));

            // Monochrome — Level 50
            int level = PlayerProgressionSystem.Instance != null
                ? PlayerProgressionSystem.Instance.CurrentLevel : 1;
            UpdateBar(monochromeBar, "Monochrome", "monochrome_trophy",
                level, 50,
                AutoLocalizer.Get("trophy_monochrome_desc"),
                new Color(0.82f, 0.835f, 0.859f));
        }

        private void UpdateBar(TrophyBar bar, string themeName, string achievementId,
            int current, int target, string description, Color themeColor)
        {
            if (bar == null) return;

            bool unlocked = current >= target;
            float progress = Mathf.Clamp01((float)current / target);

            if (bar.nameText != null)
                bar.nameText.text = themeName;

            if (bar.descText != null)
                bar.descText.text = description;

            if (bar.progressText != null)
                bar.progressText.text = $"{current:N0} / {target:N0}";

            if (bar.percentText != null)
                bar.percentText.text = $"{Mathf.RoundToInt(progress * 100)}%";

            if (bar.progressFill != null)
            {
                bar.progressFill.fillAmount = progress;
                bar.progressFill.color = unlocked ? themeColor : themeColor * 0.6f;
            }

            if (bar.themePreview != null)
            {
                bar.themePreview.color = unlocked ? themeColor : new Color(0.2f, 0.2f, 0.2f, 0.5f);
            }

            if (bar.lockIcon != null)
                bar.lockIcon.SetActive(!unlocked);

            if (bar.unlockedBadge != null)
                bar.unlockedBadge.SetActive(unlocked);
        }
    }

    [System.Serializable]
    public class TrophyBar
    {
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descText;
        public TextMeshProUGUI progressText;
        public TextMeshProUGUI percentText;
        public Image progressFill;
        public Image themePreview;
        public GameObject lockIcon;
        public GameObject unlockedBadge;
    }
}
