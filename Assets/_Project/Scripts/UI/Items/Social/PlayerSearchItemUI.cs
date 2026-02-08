using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DigitPark.Localization;

namespace DigitPark.UI.Items
{
    /// <summary>
    /// UI component for player search result item prefab.
    /// Displays player info with action buttons.
    /// </summary>
    public class PlayerSearchItemUI : MonoBehaviour
    {
        [Header("UI - Player Info")]
        [SerializeField] private Image avatarImage;
        [SerializeField] private Image avatarBorder;
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Image countryFlag;
        [SerializeField] private Image onlineIndicator;

        [Header("UI - Stats")]
        [SerializeField] private TextMeshProUGUI winsText;
        [SerializeField] private TextMeshProUGUI winRateText;
        [SerializeField] private GameObject statsContainer;

        [Header("UI - Actions")]
        [SerializeField] private Button viewProfileButton;
        [SerializeField] private Button addFriendButton;
        [SerializeField] private Button challengeButton;
        [SerializeField] private Image addFriendIcon;
        [SerializeField] private TextMeshProUGUI addFriendText;

        [Header("UI - Friend Status")]
        [SerializeField] private GameObject friendBadge;
        [SerializeField] private GameObject pendingBadge;

        [Header("Visual Settings")]
        [SerializeField] private Color onlineColor = new Color(0.3f, 0.9f, 0.4f);
        [SerializeField] private Color offlineColor = new Color(0.5f, 0.5f, 0.5f);
        [SerializeField] private Color premiumBorderColor = new Color(1f, 0.7f, 0.2f);

        private string userId;
        private Action<string> onViewProfile;
        private Action<string> onAddFriend;
        private Action<string> onChallenge;

        public void Setup(PlayerSearchDisplayData data,
            Action<string> profileCallback = null,
            Action<string> friendCallback = null,
            Action<string> challengeCallback = null)
        {
            userId = data.userId;
            onViewProfile = profileCallback;
            onAddFriend = friendCallback;
            onChallenge = challengeCallback;

            // Player info
            if (avatarImage && data.avatar != null)
                avatarImage.sprite = data.avatar;

            if (avatarBorder && data.isPremium)
                avatarBorder.color = premiumBorderColor;

            if (usernameText)
                usernameText.text = data.username;

            if (levelText)
                levelText.text = AutoLocalizer.Get("player_level", data.level);

            // Country
            if (countryFlag)
            {
                countryFlag.gameObject.SetActive(data.countryFlag != null);
                if (data.countryFlag != null)
                    countryFlag.sprite = data.countryFlag;
            }

            // Online status
            if (onlineIndicator)
                onlineIndicator.color = data.isOnline ? onlineColor : offlineColor;

            // Stats
            if (statsContainer)
                statsContainer.SetActive(data.showStats);

            if (winsText)
                winsText.text = AutoLocalizer.Get("player_wins", data.totalWins);

            if (winRateText)
                winRateText.text = $"{data.winRate:0}%";

            // Friend status
            UpdateFriendStatus(data.friendStatus);

            // Setup buttons
            SetupButtons();
        }

        private void UpdateFriendStatus(FriendStatus status)
        {
            if (friendBadge)
                friendBadge.SetActive(status == FriendStatus.Friend);

            if (pendingBadge)
                pendingBadge.SetActive(status == FriendStatus.Pending);

            if (addFriendButton)
            {
                bool canAdd = status == FriendStatus.None;
                addFriendButton.interactable = canAdd;

                if (addFriendText)
                {
                    switch (status)
                    {
                        case FriendStatus.None:
                            addFriendText.text = AutoLocalizer.Get("player_add");
                            break;
                        case FriendStatus.Pending:
                            addFriendText.text = AutoLocalizer.Get("player_pending");
                            break;
                        case FriendStatus.Friend:
                            addFriendText.text = AutoLocalizer.Get("player_friend");
                            break;
                    }
                }
            }
        }

        private void SetupButtons()
        {
            if (viewProfileButton)
            {
                viewProfileButton.onClick.RemoveAllListeners();
                viewProfileButton.onClick.AddListener(() => onViewProfile?.Invoke(userId));
            }

            if (addFriendButton)
            {
                addFriendButton.onClick.RemoveAllListeners();
                addFriendButton.onClick.AddListener(OnAddFriendClicked);
            }

            if (challengeButton)
            {
                challengeButton.onClick.RemoveAllListeners();
                challengeButton.onClick.AddListener(() => onChallenge?.Invoke(userId));
            }
        }

        private void OnAddFriendClicked()
        {
            onAddFriend?.Invoke(userId);

            // Visual feedback
            if (addFriendButton)
            {
                addFriendButton.interactable = false;
                if (addFriendText)
                    addFriendText.text = AutoLocalizer.Get("player_sent");
            }

            if (pendingBadge)
            {
                pendingBadge.SetActive(true);
                pendingBadge.transform.localScale = Vector3.one;
            }
        }

        public void PlayHighlightAnimation()
        {
            // Simple highlight - no animation without tweening library
        }
    }

    [System.Serializable]
    public class PlayerSearchDisplayData
    {
        public string userId;
        public string username;
        public Sprite avatar;
        public Sprite countryFlag;
        public int level;
        public int totalWins;
        public float winRate;
        public bool isOnline;
        public bool isPremium;
        public bool showStats;
        public FriendStatus friendStatus;
    }

    public enum FriendStatus
    {
        None,
        Pending,
        Friend
    }
}
