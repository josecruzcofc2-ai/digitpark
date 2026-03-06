using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;
using DigitPark.Localization;

namespace DigitPark.UI.Items
{
    /// <summary>
    /// UI component for tournament participant item prefab.
    /// Displays player avatar, name, rank and ready status.
    /// </summary>
    public class ParticipantItemUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image avatarImage;
        [SerializeField] private Image avatarBorder;
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private TextMeshProUGUI rankText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image countryFlag;

        [Header("UI - Status")]
        [SerializeField] private GameObject readyIndicator;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Image onlineIndicator;

        [Header("UI - Attempts & Time")]
        [SerializeField] private TextMeshProUGUI attemptsText;
        [SerializeField] private TextMeshProUGUI bestTimeText;

        [Header("UI - Actions")]
        [SerializeField] private Button viewProfileButton;
        [SerializeField] private Button challengeButton;

        [Header("Visual Settings")]
        [SerializeField] private Color onlineColor = new Color(0.3f, 0.9f, 0.4f);
        [SerializeField] private Color offlineColor = new Color(0.5f, 0.5f, 0.5f);
        [SerializeField] private Color readyColor = new Color(0.3f, 0.9f, 0.4f);
        [SerializeField] private Color notReadyColor = new Color(0.9f, 0.6f, 0.2f);
        [SerializeField] private Color selfHighlightColor = new Color(0f, 0.5f, 0.8f, 0.3f);

        private string userId;
        private Action<string> onViewProfile;
        private Action<string> onChallenge;

        public void Setup(ParticipantDisplayData data, Action<string> profileCallback = null, Action<string> challengeCallback = null)
        {
            userId = data.userId;
            onViewProfile = profileCallback;
            onChallenge = challengeCallback;

            // Avatar
            if (avatarImage && data.avatar != null)
                avatarImage.sprite = data.avatar;

            // Username
            if (usernameText)
                usernameText.text = data.username;

            // Rank
            if (rankText)
            {
                rankText.text = data.rank > 0 ? $"#{data.rank}" : "-";
                rankText.gameObject.SetActive(data.showRank);
            }

            // Country flag
            if (countryFlag)
            {
                countryFlag.gameObject.SetActive(data.countryFlag != null);
                if (data.countryFlag != null)
                    countryFlag.sprite = data.countryFlag;
            }

            // Online status
            if (onlineIndicator)
                onlineIndicator.color = data.isOnline ? onlineColor : offlineColor;

            // Ready status
            UpdateReadyStatus(data.isReady);

            // Attempts & Time
            if (attemptsText)
            {
                attemptsText.text = $"{data.attemptsUsed}/{data.maxAttempts}";
                attemptsText.gameObject.SetActive(data.showAttempts);
            }

            if (bestTimeText)
            {
                bestTimeText.text = data.bestTime > 0 ? FormatTime(data.bestTime) : "-";
                bestTimeText.gameObject.SetActive(data.showBestTime);
            }

            // Highlight if self
            if (backgroundImage && data.isSelf)
                backgroundImage.color = selfHighlightColor;

            // Buttons
            SetupButtons(data.isSelf);
        }

        public void UpdateReadyStatus(bool isReady)
        {
            if (readyIndicator)
            {
                if (isReady)
                {
                    readyIndicator.SetActive(true);
                    readyIndicator.transform.localScale = Vector3.zero;
                    readyIndicator.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
                }
                else
                {
                    readyIndicator.transform.DOScale(0f, 0.15f).SetEase(Ease.InBack)
                        .OnComplete(() => readyIndicator.SetActive(false));
                }
            }

            if (statusText)
            {
                statusText.text = isReady ? AutoLocalizer.Get("tournament_ready") : AutoLocalizer.Get("tournament_waiting");
                statusText.color = isReady ? readyColor : notReadyColor;
            }
        }

        private void SetupButtons(bool isSelf)
        {
            if (viewProfileButton)
            {
                viewProfileButton.onClick.RemoveAllListeners();
                viewProfileButton.onClick.AddListener(() => onViewProfile?.Invoke(userId));
            }

            if (challengeButton)
            {
                challengeButton.gameObject.SetActive(!isSelf);
                challengeButton.onClick.RemoveAllListeners();
                challengeButton.onClick.AddListener(() => onChallenge?.Invoke(userId));
            }
        }

        private string FormatTime(float seconds)
        {
            if (seconds >= 60)
            {
                int mins = (int)(seconds / 60);
                float secs = seconds % 60;
                return $"{mins}:{secs:00.00}";
            }
            return $"{seconds:0.00}s";
        }

        public void PlayReadyAnimation()
        {
            if (readyIndicator)
            {
                readyIndicator.transform.localScale = Vector3.one;
            }
        }
    }

    [System.Serializable]
    public class ParticipantDisplayData
    {
        public string userId;
        public string username;
        public Sprite avatar;
        public Sprite countryFlag;
        public int rank;
        public int attemptsUsed;
        public int maxAttempts;
        public float bestTime;
        public bool isOnline;
        public bool isReady;
        public bool isSelf;
        public bool showRank;
        public bool showAttempts;
        public bool showBestTime;
    }
}
