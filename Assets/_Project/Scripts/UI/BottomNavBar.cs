using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace DigitPark.UI
{
    /// <summary>
    /// Barra de navegacion inferior con acceso rapido a secciones principales.
    /// Incluye: Home, Play, Shop, Missions, Profile
    /// </summary>
    public class BottomNavBar : MonoBehaviour
    {
        [Header("Nav Buttons")]
        [SerializeField] private NavButton _homeButton;
        [SerializeField] private NavButton _playButton;
        [SerializeField] private NavButton _shopButton;
        [SerializeField] private NavButton _missionsButton;
        [SerializeField] private NavButton _profileButton;

        [Header("Visual Settings")]
        [SerializeField] private Color _activeColor = new Color(0f, 1f, 1f, 1f);
        [SerializeField] private Color _inactiveColor = new Color(0.5f, 0.55f, 0.6f, 1f);
        [SerializeField] private Color _shopColor = new Color(1f, 0.84f, 0f, 1f); // Gold for shop

        [Header("Audio")]
        [SerializeField] private AudioClip _clickSound;

        [Header("Notification Badges")]
        [SerializeField] private GameObject _shopBadge;
        [SerializeField] private GameObject _missionsBadge;
        [SerializeField] private TextMeshProUGUI _missionsBadgeText;

        private NavItem _currentItem = NavItem.Home;
        private AudioSource _audioSource;

        public NavItem CurrentItem => _currentItem;

        public event Action<NavItem> OnNavItemSelected;

        [Serializable]
        public class NavButton
        {
            public Button button;
            public Image icon;
            public TextMeshProUGUI label;
            public Image indicator;
        }

        public enum NavItem
        {
            Home,
            Play,
            Shop,
            Missions,
            Profile
        }

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
            }
        }

        private void OnEnable()
        {
            SetupButtons();
        }

        private void OnDisable()
        {
            RemoveButtonListeners();
        }

        private void SetupButtons()
        {
            if (_homeButton?.button != null)
                _homeButton.button.onClick.AddListener(() => OnNavButtonClick(NavItem.Home));
            if (_playButton?.button != null)
                _playButton.button.onClick.AddListener(() => OnNavButtonClick(NavItem.Play));
            if (_shopButton?.button != null)
                _shopButton.button.onClick.AddListener(() => OnNavButtonClick(NavItem.Shop));
            if (_missionsButton?.button != null)
                _missionsButton.button.onClick.AddListener(() => OnNavButtonClick(NavItem.Missions));
            if (_profileButton?.button != null)
                _profileButton.button.onClick.AddListener(() => OnNavButtonClick(NavItem.Profile));
        }

        private void RemoveButtonListeners()
        {
            if (_homeButton?.button != null)
                _homeButton.button.onClick.RemoveAllListeners();
            if (_playButton?.button != null)
                _playButton.button.onClick.RemoveAllListeners();
            if (_shopButton?.button != null)
                _shopButton.button.onClick.RemoveAllListeners();
            if (_missionsButton?.button != null)
                _missionsButton.button.onClick.RemoveAllListeners();
            if (_profileButton?.button != null)
                _profileButton.button.onClick.RemoveAllListeners();
        }

        private void OnNavButtonClick(NavItem item)
        {
            PlayClickSound();
            SelectItem(item);
            NavigateToItem(item);
        }

        /// <summary>
        /// Selecciona un item sin navegar (solo visual)
        /// </summary>
        public void SelectItem(NavItem item)
        {
            _currentItem = item;
            UpdateVisuals();
            OnNavItemSelected?.Invoke(item);
        }

        private void UpdateVisuals()
        {
            UpdateButton(_homeButton, _currentItem == NavItem.Home, _activeColor);
            UpdateButton(_playButton, _currentItem == NavItem.Play, _activeColor);
            UpdateButton(_shopButton, _currentItem == NavItem.Shop, _shopColor);
            UpdateButton(_missionsButton, _currentItem == NavItem.Missions, _activeColor);
            UpdateButton(_profileButton, _currentItem == NavItem.Profile, _activeColor);
        }

        private void UpdateButton(NavButton navButton, bool isActive, Color activeColor)
        {
            if (navButton == null) return;

            Color color = isActive ? activeColor : _inactiveColor;

            if (navButton.icon != null)
                navButton.icon.color = color;

            if (navButton.label != null)
                navButton.label.color = color;

            if (navButton.indicator != null)
                navButton.indicator.gameObject.SetActive(isActive);
        }

        private void NavigateToItem(NavItem item)
        {
            var navigator = Monetization.SceneNavigator.Instance;

            switch (item)
            {
                case NavItem.Home:
                    navigator.NavigateTo(Monetization.SceneNavigator.Scenes.MAIN_MENU);
                    break;
                case NavItem.Play:
                    navigator.NavigateTo(Monetization.SceneNavigator.Scenes.CASH_BATTLE_HUB);
                    break;
                case NavItem.Shop:
                    navigator.NavigateToShop();
                    break;
                case NavItem.Missions:
                    navigator.NavigateTo(Monetization.SceneNavigator.Scenes.DAILY_MISSIONS);
                    break;
                case NavItem.Profile:
                    navigator.NavigateTo(Monetization.SceneNavigator.Scenes.PROFILE);
                    break;
            }

            Debug.Log($"[BottomNavBar] Navigating to: {item}");
        }

        private void PlayClickSound()
        {
            if (_audioSource != null && _clickSound != null)
            {
                _audioSource.PlayOneShot(_clickSound);
            }
        }

        // ==================== BADGE MANAGEMENT ====================

        /// <summary>
        /// Muestra/oculta el badge de ofertas en Shop
        /// </summary>
        public void SetShopBadge(bool show)
        {
            if (_shopBadge != null)
                _shopBadge.SetActive(show);
        }

        /// <summary>
        /// Actualiza el badge de misiones con el numero de misiones completadas sin reclamar
        /// </summary>
        public void SetMissionsBadge(int count)
        {
            if (_missionsBadge != null)
            {
                _missionsBadge.SetActive(count > 0);

                if (_missionsBadgeText != null)
                {
                    _missionsBadgeText.text = count > 9 ? "9+" : count.ToString();
                }
            }
        }
    }
}
