using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
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

        private CanvasGroup panelCG;

        private void OnEnable()
        {
            RefreshAll(animated: false);
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(Hide);
            }
        }

        public void Show()
        {
            GameObject target = panel != null ? panel : gameObject;
            target.SetActive(true);

            panelCG = target.GetComponent<CanvasGroup>();
            if (panelCG == null) panelCG = target.AddComponent<CanvasGroup>();

            RefreshAll(animated: true);
            AnimateIn(target.transform);
        }

        public void Hide()
        {
            GameObject target = panel != null ? panel : gameObject;

            if (panelCG != null)
            {
                AnimateOut(target.transform, () => target.SetActive(false));
            }
            else
            {
                target.SetActive(false);
            }
        }

        public void RefreshAll(bool animated = false)
        {
            // Emerald — 365 days login (non-consecutive)
            int totalLoginDays = PlayerPrefs.GetInt("DP_TotalLoginDays", 0);
            UpdateBar(emeraldBar, "Emerald", "emerald_trophy",
                totalLoginDays, 365,
                AutoLocalizer.Get("trophy_emerald_desc"),
                new Color(0.063f, 0.725f, 0.506f), animated, 0);

            // Electric Blue — 1,000 ranked wins
            int rankedWins = PlayerPrefs.GetInt("DP_RankedWins_Total", 0);
            UpdateBar(electricBlueBar, "Electric Blue", "electric_blue_trophy",
                rankedWins, 1000,
                AutoLocalizer.Get("trophy_electric_blue_desc"),
                new Color(0.231f, 0.51f, 0.965f), animated, 1);

            // Electric Violet — 100 perfect scores
            int perfectScores = PlayerPrefs.GetInt("DP_PerfectScores_Total", 0);
            UpdateBar(electricVioletBar, "Electric Violet", "electric_violet_trophy",
                perfectScores, 100,
                AutoLocalizer.Get("trophy_electric_violet_desc"),
                new Color(0.659f, 0.333f, 0.969f), animated, 2);

            // Monochrome — Level 50
            int level = PlayerProgressionSystem.Instance != null
                ? PlayerProgressionSystem.Instance.CurrentLevel : 1;
            UpdateBar(monochromeBar, "Monochrome", "monochrome_trophy",
                level, 50,
                AutoLocalizer.Get("trophy_monochrome_desc"),
                new Color(0.82f, 0.835f, 0.859f), animated, 3);
        }

        private void UpdateBar(TrophyBar bar, string themeName, string achievementId,
            int current, int target, string description, Color themeColor,
            bool animated = false, int index = 0)
        {
            if (bar == null) return;

            bool unlocked = current >= target;
            float progress = Mathf.Clamp01((float)current / target);

            if (bar.nameText != null)
                bar.nameText.text = themeName;

            if (bar.descText != null)
                bar.descText.text = description;

            if (bar.progressFill != null)
                bar.progressFill.color = unlocked ? themeColor : themeColor * 0.6f;

            if (bar.themePreview != null)
                bar.themePreview.color = unlocked ? themeColor : new Color(0.2f, 0.2f, 0.2f, 0.5f);

            if (bar.lockIcon != null)
                bar.lockIcon.SetActive(!unlocked);

            if (animated && bar.progressFill != null)
            {
                float delay = index * 0.12f;
                bar.progressFill.fillAmount = 0f;
                if (bar.percentText != null) bar.percentText.text = "0%";
                if (bar.progressText != null) bar.progressText.text = $"0 / {target:N0}";

                int targetPercent = Mathf.RoundToInt(progress * 100);
                DOTween.Sequence()
                    .AppendInterval(delay)
                    .Append(bar.progressFill.DOFillAmount(progress, 0.6f).SetEase(Ease.OutQuad))
                    .Join(DOVirtual.Float(0, current, 0.6f, v =>
                    {
                        if (bar.progressText != null)
                            bar.progressText.text = $"{Mathf.RoundToInt(v):N0} / {target:N0}";
                        if (bar.percentText != null)
                            bar.percentText.text = $"{Mathf.RoundToInt(v / (float)target * 100)}%";
                    }).SetEase(Ease.OutQuad))
                    .OnComplete(() =>
                    {
                        if (unlocked && bar.unlockedBadge != null)
                        {
                            bar.unlockedBadge.SetActive(true);
                            bar.unlockedBadge.transform.localScale = Vector3.zero;
                            bar.unlockedBadge.transform.DOScale(1f, 0.3f).SetEase(Ease.OutElastic).SetUpdate(true).SetLink(bar.unlockedBadge);
                        }
                    })
                    .SetUpdate(true)
                    .SetLink(gameObject);

                if (!unlocked && bar.unlockedBadge != null)
                    bar.unlockedBadge.SetActive(false);
            }
            else
            {
                if (bar.progressText != null)
                    bar.progressText.text = $"{current:N0} / {target:N0}";

                if (bar.percentText != null)
                    bar.percentText.text = $"{Mathf.RoundToInt(progress * 100)}%";

                if (bar.progressFill != null)
                    bar.progressFill.fillAmount = progress;

                if (bar.unlockedBadge != null)
                    bar.unlockedBadge.SetActive(unlocked);
            }
        }

        private void AnimateIn(Transform t)
        {
            if (panelCG == null) return;
            panelCG.alpha = 0f;
            t.localScale = Vector3.one * 0.9f;
            DOTween.Sequence()
                .Join(t.DOScale(1f, 0.25f).SetEase(Ease.OutBack))
                .Join(panelCG.DOFade(1f, 0.2f))
                .SetUpdate(true)
                .SetLink(t.gameObject);
        }

        private void AnimateOut(Transform t, System.Action onComplete)
        {
            if (panelCG == null) { onComplete?.Invoke(); return; }
            DOTween.Sequence()
                .Join(t.DOScale(0.95f, 0.15f).SetEase(Ease.InQuad))
                .Join(panelCG.DOFade(0f, 0.15f))
                .OnComplete(() =>
                {
                    t.localScale = Vector3.one;
                    panelCG.alpha = 1f;
                    onComplete?.Invoke();
                })
                .SetUpdate(true)
                .SetLink(t.gameObject);
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
