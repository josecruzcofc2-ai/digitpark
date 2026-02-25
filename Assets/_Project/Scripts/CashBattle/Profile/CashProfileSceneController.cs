using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DigitPark.Localization;
using DigitPark.Monetization;
using DigitPark.Animations;
using DigitPark.Effects;
using DigitPark.UI.Panels;
using DigitPark.Services.Firebase;

namespace DigitPark.CashBattle
{
    /// <summary>
    /// Controller para la escena CashProfile.
    /// Muestra estadisticas privadas del usuario en Cash Battle.
    /// Solo el usuario puede ver sus propios stats de dinero real (privacidad).
    ///
    /// Animation logic is delegated to CashProfileAnimator (same pattern as
    /// MainMenuManager → MainMenuAnimator).
    /// </summary>
    public class CashProfileSceneController : MonoBehaviour
    {
        // ==================== HEADER ====================
        [Header("Header")]
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI titleText;

        // ==================== AVATAR SECTION ====================
        [Header("Avatar")]
        [SerializeField] private Image avatarImage;
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private Button editNameButton;
        [SerializeField] private TextMeshProUGUI memberSinceText;

        // ==================== CHANGE NAME ====================
        [Header("Change Name")]
        [SerializeField] private InputPanelUI changeNamePanel;
        [SerializeField] private ErrorPanelUI errorPanel;

        // ==================== SUMMARY STATS (3 boxes) ====================
        [Header("Summary Stats")]
        [SerializeField] private TextMeshProUGUI summaryTotalMatchesText;
        [SerializeField] private TextMeshProUGUI summaryWinRateText;
        [SerializeField] private TextMeshProUGUI summaryNetProfitText;

        // ==================== STATS GRID (10 stats) ====================
        [Header("Stats Grid")]
        [SerializeField] private TextMeshProUGUI winsText;
        [SerializeField] private TextMeshProUGUI lossesText;
        [SerializeField] private TextMeshProUGUI drawsText;
        [SerializeField] private TextMeshProUGUI currentStreakText;
        [SerializeField] private TextMeshProUGUI bestStreakText;
        [SerializeField] private TextMeshProUGUI tournamentsPlayedText;
        [SerializeField] private TextMeshProUGUI tournamentWinsText;
        [SerializeField] private TextMeshProUGUI avgEarningsText;
        [SerializeField] private TextMeshProUGUI totalEarningsText;
        [SerializeField] private TextMeshProUGUI totalSpentText;

        // ==================== RECORD CARD ====================
        [Header("Record Card")]
        [SerializeField] private TextMeshProUGUI recordText;
        [SerializeField] private Image winRateBarFill;
        [SerializeField] private TextMeshProUGUI winRateText;

        // ==================== RANK ====================
        [Header("Rank")]
        [SerializeField] private TextMeshProUGUI rankBadgeText;

        // ==================== PER-GAME STATS ====================
        [Header("Per-Game Stats")]
        [SerializeField] private Image digitRushBarFill;
        [SerializeField] private TextMeshProUGUI digitRushValueText;
        [SerializeField] private Image memoryPairsBarFill;
        [SerializeField] private TextMeshProUGUI memoryPairsValueText;
        [SerializeField] private Image quickMathBarFill;
        [SerializeField] private TextMeshProUGUI quickMathValueText;
        [SerializeField] private Image flashTapBarFill;
        [SerializeField] private TextMeshProUGUI flashTapValueText;
        [SerializeField] private Image oddOneOutBarFill;
        [SerializeField] private TextMeshProUGUI oddOneOutValueText;

        // ==================== NAME CHANGE ====================
        private const string NAME_CHANGE_COUNT_KEY = "NameChangeCount";
        private const int NAME_CHANGE_GEM_COST = 100;

        // ==================== ANIMATOR ====================
        private CashProfileAnimator _animator;

        // ==================== LIFECYCLE ====================

        private void Start()
        {
            _animator = GetComponent<CashProfileAnimator>()
                ?? FindObjectOfType<CashProfileAnimator>();
            SetupListeners();
            SetupButtonEffects();
            RefreshProfile();
            _animator?.PlayEntranceSequence();
        }

        private void OnDestroy()
        {
            CancelInvoke();
        }

        private void SetupListeners()
        {
            // Disable auto-navigation from BackButtonGold prefab to prevent double listener
            var autoNav = backButton?.GetComponent<DigitPark.UI.BackButtonGold>();
            if (autoNav != null) autoNav.DisableAutoNavigation();
            if (backButton)
                backButton.onClick.AddListener(OnBackClicked);

            editNameButton?.onClick.AddListener(OnEditNameClicked);
        }

        // ==================== BUTTON EFFECTS ====================

        private void SetupButtonEffects()
        {
            if (backButton == null) return;

            var fx = backButton.GetComponent<ButtonEffects>();
            if (fx == null)
                backButton.gameObject.AddComponent<ButtonEffects>();

            ColorBlock cb = backButton.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1f, 0.95f, 0.7f, 1f);
            cb.pressedColor = new Color(0.8f, 0.67f, 0f, 1f);
            cb.selectedColor = Color.white;
            backButton.colors = cb;
        }

        // ==================== PROFILE DATA ====================

        private void RefreshProfile()
        {
            RefreshUserInfo();
            RefreshStats();
        }

        private void RefreshUserInfo()
        {
            if (usernameText)
            {
                // Sync from AuthenticationService (single source of truth)
                string displayName = null;
                if (AuthenticationService.Instance != null)
                {
                    var playerData = AuthenticationService.Instance.GetCurrentPlayerData();
                    displayName = playerData?.username;
                }
                if (string.IsNullOrEmpty(displayName))
                    displayName = PlayerPrefs.GetString("DisplayName", "Player");

                usernameText.text = displayName;
            }

            if (memberSinceText)
            {
                string memberDate = PlayerPrefs.GetString("MemberSince", "2024");
                memberSinceText.text = AutoLocalizer.Get("cashprofile_member_since", memberDate);
            }
        }

        private void RefreshStats()
        {
            if (HistoryManager.Instance == null) return;

            var stats = HistoryManager.Instance.GetStats();

            // Summary stats
            if (summaryTotalMatchesText)
                summaryTotalMatchesText.text = stats.totalMatches.ToString();

            if (summaryWinRateText)
                summaryWinRateText.text = $"{stats.winRate:F1}%";

            if (summaryNetProfitText)
            {
                summaryNetProfitText.text = stats.netProfit >= 0
                    ? $"+${stats.netProfit:F2}"
                    : $"-${Math.Abs(stats.netProfit):F2}";
                summaryNetProfitText.color = stats.netProfit >= 0
                    ? new Color(0f, 1f, 0.5f)
                    : new Color(1f, 0.4f, 0.4f);
            }

            // W/L/D
            if (winsText) winsText.text = stats.wins.ToString();
            if (lossesText) lossesText.text = stats.losses.ToString();
            if (drawsText) drawsText.text = stats.draws.ToString();

            // Streaks
            var (currentStreak, isWinStreak) = HistoryManager.Instance.GetCurrentStreak();
            if (currentStreakText)
            {
                currentStreakText.text = $"{currentStreak} {(isWinStreak ? "W" : "L")}";
                currentStreakText.color = isWinStreak
                    ? new Color(0f, 1f, 0.5f)
                    : new Color(1f, 0.4f, 0.4f);
            }

            if (bestStreakText)
            {
                int bestStreak = HistoryManager.Instance.GetBestWinStreak();
                bestStreakText.text = $"{bestStreak} W";
            }

            // Tournaments
            if (tournamentsPlayedText)
                tournamentsPlayedText.text = stats.tournamentsPlayed.ToString();
            if (tournamentWinsText)
                tournamentWinsText.text = stats.tournamentWins.ToString();

            // Earnings
            if (avgEarningsText)
                avgEarningsText.text = $"${stats.avgEarningsPerMatch:F2}";
            if (totalEarningsText)
                totalEarningsText.text = $"${stats.totalEarnings:F2}";
            if (totalSpentText)
                totalSpentText.text = $"${stats.totalSpent:F2}";

            // Record Card
            if (recordText)
                recordText.text = $"{stats.wins}W  \u00b7  {stats.losses}L  \u00b7  {stats.draws}D";
            if (winRateText)
                winRateText.text = $"{stats.winRate:F0}% Win Rate";
            if (winRateBarFill)
                winRateBarFill.fillAmount = stats.winRate / 100f;

            // Rank
            if (rankBadgeText)
                rankBadgeText.text = GetRank(stats.totalMatches, stats.winRate);
        }

        // ==================== RANK CALCULATION ====================

        private string GetRank(int totalMatches, float winRate)
        {
            if (totalMatches >= 100 && winRate >= 70) return "DIAMOND";
            if (totalMatches >= 50 && winRate >= 60) return "PLATINUM";
            if (totalMatches >= 25 && winRate >= 50) return "GOLD";
            if (totalMatches >= 10) return "SILVER";
            return "BRONZE";
        }

        // ==================== CHANGE NAME ====================

        private void OnEditNameClicked()
        {
            int changeCount = PlayerPrefs.GetInt(NAME_CHANGE_COUNT_KEY, 0);

            if (changeCount > 0)
            {
                int currentGems = CurrencyManager.Instance?.Gems ?? 0;
                if (currentGems < NAME_CHANGE_GEM_COST)
                {
                    errorPanel?.Show(AutoLocalizer.Get("not_enough_gems_name_change", NAME_CHANGE_GEM_COST));
                    return;
                }
            }

            if (changeNamePanel != null)
            {
                changeNamePanel.SetLengthLimits(3, 20);
                changeNamePanel.Show(
                    changeCount == 0
                        ? AutoLocalizer.Get("change_name_title")
                        : AutoLocalizer.Get("change_name_title_cost", NAME_CHANGE_GEM_COST),
                    AutoLocalizer.Get("new_name_placeholder"),
                    OnConfirmNameChange,
                    null
                );
            }
        }

        private async void OnConfirmNameChange(string newUsername)
        {
            if (AuthenticationService.Instance == null) return;

            var playerData = AuthenticationService.Instance.GetCurrentPlayerData();
            if (playerData != null && newUsername == playerData.username)
            {
                changeNamePanel?.Hide();
                return;
            }

            int changeCount = PlayerPrefs.GetInt(NAME_CHANGE_COUNT_KEY, 0);

            if (changeCount > 0)
            {
                bool spent = CurrencyManager.Instance?.SpendGems(NAME_CHANGE_GEM_COST) ?? false;
                if (!spent)
                {
                    errorPanel?.Show(AutoLocalizer.Get("not_enough_gems_name_change", NAME_CHANGE_GEM_COST));
                    changeNamePanel?.SetButtonsInteractable(true);
                    return;
                }
            }

            Debug.Log($"[CashProfile] Cambiando nombre a: {newUsername}");

            bool success = await AuthenticationService.Instance.UpdateUsername(newUsername);

            if (success)
            {
                Debug.Log("[CashProfile] Nombre actualizado exitosamente");
                if (playerData != null) playerData.username = newUsername;
                PlayerPrefs.SetInt(NAME_CHANGE_COUNT_KEY, changeCount + 1);
                PlayerPrefs.SetString("DisplayName", newUsername);
                PlayerPrefs.Save();
                changeNamePanel?.Hide();

                if (usernameText != null)
                    usernameText.text = newUsername;
            }
            else
            {
                Debug.LogError("[CashProfile] Error al actualizar nombre");
                changeNamePanel?.SetButtonsInteractable(true);
                errorPanel?.Show(AutoLocalizer.Get("error_changing_name"));
            }
        }

        // ==================== NAVIGATION ====================

        private void OnBackClicked()
        {
            UIAnimations.ButtonPress(backButton.transform, 0.9f, 0.15f);
            SceneNavigator.Instance?.GoBack();
        }
    }
}
