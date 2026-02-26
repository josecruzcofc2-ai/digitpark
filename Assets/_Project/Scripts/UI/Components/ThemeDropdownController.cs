using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DigitPark.Themes;
using DigitPark.Managers;
using DigitPark.Tools;

namespace DigitPark.UI.Components
{
    /// <summary>
    /// Runtime controller for the theme dropdown in Settings.
    /// Populates dynamically from ThemeManager.AvailableThemes.
    /// Shows gold lock for paid premium themes, silver lock for earnable themes.
    /// Locks disappear when themes are acquired.
    /// </summary>
    [RequireComponent(typeof(TMP_Dropdown))]
    public class ThemeDropdownController : MonoBehaviour
    {
        [Header("Lock Icons")]
        [SerializeField] private Sprite lockGoldSprite;
        [SerializeField] private Sprite lockSilverSprite;

        private TMP_Dropdown dropdown;
        private bool isInitialized = false;
        private int lastValidIndex = 0;

        // Runtime lock icon references (one per dropdown item)
        private List<Image> lockIcons = new List<Image>();

        private void Awake()
        {
            dropdown = GetComponent<TMP_Dropdown>();

            // Auto-load lock icons if not assigned
            if (lockGoldSprite == null)
            {
                lockGoldSprite = Resources.Load<Sprite>("UI/Icons/icon_lock_gold");
            }
            if (lockSilverSprite == null)
            {
                lockSilverSprite = Resources.Load<Sprite>("UI/Icons/icon_lock_silver");
            }
        }

        private void Start()
        {
            Initialize();
        }

        private void OnEnable()
        {
            ThemeManager.OnThemeChanged += OnThemeChangedExternally;
            PremiumManager.OnPremiumStatusChanged += OnPremiumStatusChanged;
        }

        private void OnDisable()
        {
            ThemeManager.OnThemeChanged -= OnThemeChangedExternally;
            PremiumManager.OnPremiumStatusChanged -= OnPremiumStatusChanged;
        }

        /// <summary>
        /// Populates the dropdown with all themes from ThemeManager
        /// </summary>
        public void Initialize()
        {
            if (dropdown == null) return;

            if (ThemeManager.Instance == null)
            {
                Debug.LogWarning("[ThemeDropdown] ThemeManager not available yet");
                return;
            }

            PopulateDropdown();

            dropdown.onValueChanged.RemoveListener(OnThemeSelected);
            dropdown.onValueChanged.AddListener(OnThemeSelected);

            SyncWithCurrentTheme();
            isInitialized = true;
        }

        /// <summary>
        /// Fills the dropdown options from ThemeManager.AvailableThemes
        /// </summary>
        private void PopulateDropdown()
        {
            var themes = ThemeManager.Instance.AvailableThemes;
            dropdown.ClearOptions();

            var options = new List<TMP_Dropdown.OptionData>();
            foreach (var theme in themes)
            {
                string displayName = theme.themeName;

                // Add lock suffix for locked themes (spacing for icon)
                if (theme.isPremium && !IsThemeUnlocked(theme))
                {
                    displayName += "  \u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0";
                }

                options.Add(new TMP_Dropdown.OptionData(displayName));
            }

            dropdown.AddOptions(options);
            Debug.Log($"[ThemeDropdown] Populated with {themes.Count} themes");
        }

        /// <summary>
        /// Checks if a theme is unlocked (free, purchased, or debug unlocked)
        /// </summary>
        private bool IsThemeUnlocked(ThemeData theme)
        {
            if (theme == null) return false;
            if (!theme.isPremium) return true;

            // Debug controller override
            if (PremiumDebugController.Instance != null && PremiumDebugController.Instance.AllowThemeChange)
                return true;

            // Check individual theme ownership
            if (ThemeManager.Instance != null && ThemeManager.Instance.IsThemeOwned(theme.themeId))
                return true;

            // Check real purchase status (legacy StylesPro unlocks all paid themes)
            if (!theme.isEarnable && PremiumManager.Instance != null && PremiumManager.Instance.HasStylesPro)
                return true;

            return false;
        }

        /// <summary>
        /// Checks if theme at index is unlocked
        /// </summary>
        private bool IsThemeUnlockedAtIndex(int index)
        {
            var themes = ThemeManager.Instance.AvailableThemes;
            if (index < 0 || index >= themes.Count) return false;
            return IsThemeUnlocked(themes[index]);
        }

        /// <summary>
        /// Syncs the dropdown value with ThemeManager's current theme
        /// </summary>
        private void SyncWithCurrentTheme()
        {
            if (ThemeManager.Instance == null || dropdown == null) return;

            int currentIndex = ThemeManager.Instance.CurrentThemeIndex;
            if (currentIndex >= 0 && currentIndex < dropdown.options.Count)
            {
                dropdown.SetValueWithoutNotify(currentIndex);
                lastValidIndex = currentIndex;
            }
        }

        /// <summary>
        /// Called when user selects a theme from the dropdown
        /// </summary>
        private void OnThemeSelected(int index)
        {
            if (ThemeManager.Instance == null) return;

            if (!IsThemeUnlockedAtIndex(index))
            {
                Debug.Log($"[ThemeDropdown] Theme locked, reverting to last valid");
                dropdown.SetValueWithoutNotify(lastValidIndex);
                ShowPurchasePrompt();
                return;
            }

            lastValidIndex = index;
            ThemeManager.Instance.SetTheme(index);
        }

        /// <summary>
        /// Shows the StylesPro purchase panel
        /// </summary>
        private void ShowPurchasePrompt()
        {
            DigitPark.UI.Panels.StylesProPromptPanel.CreateAndShow();
        }

        /// <summary>
        /// Callback when theme changes externally
        /// </summary>
        private void OnThemeChangedExternally(ThemeData theme)
        {
            SyncWithCurrentTheme();
        }

        /// <summary>
        /// Callback when premium status changes (purchase completed or debug toggle)
        /// </summary>
        private void OnPremiumStatusChanged()
        {
            Refresh();
        }

        /// <summary>
        /// Refreshes the dropdown (re-populates with updated lock status)
        /// </summary>
        public void Refresh()
        {
            isInitialized = false;
            Initialize();
        }

        /// <summary>
        /// Returns the appropriate lock sprite for a theme.
        /// Gold lock = paid (real money), Silver lock = earnable (gameplay).
        /// </summary>
        private Sprite GetLockSpriteForTheme(ThemeData theme)
        {
            if (theme == null) return lockGoldSprite;
            return theme.isEarnable ? lockSilverSprite : lockGoldSprite;
        }

        /// <summary>
        /// Called by the dropdown template when it creates items.
        /// Adds lock icon Image to premium items.
        /// Gold lock for paid themes, silver lock for earnable themes.
        /// Lock disappears when theme is acquired.
        /// </summary>
        public void UpdateLockIcons()
        {
            if (ThemeManager.Instance == null) return;

            var themes = ThemeManager.Instance.AvailableThemes;

            // Find all items in the dropdown list
            Transform dropdownList = dropdown.transform.Find("Dropdown List");
            if (dropdownList == null) return;

            Transform content = dropdownList.Find("Content");
            if (content == null) return;

            for (int i = 0; i < content.childCount && i < themes.Count; i++)
            {
                Transform item = content.GetChild(i);
                if (item == null) continue;

                bool showLock = themes[i].isPremium && !IsThemeUnlocked(themes[i]);
                Sprite lockSprite = GetLockSpriteForTheme(themes[i]);

                // Find or create lock icon
                Transform lockTransform = item.Find("LockIcon");
                Image lockImg;

                if (lockTransform == null)
                {
                    GameObject lockObj = new GameObject("LockIcon");
                    lockObj.transform.SetParent(item, false);

                    RectTransform lockRT = lockObj.AddComponent<RectTransform>();
                    lockRT.anchorMin = new Vector2(1, 0.5f);
                    lockRT.anchorMax = new Vector2(1, 0.5f);
                    lockRT.pivot = new Vector2(1, 0.5f);
                    lockRT.sizeDelta = new Vector2(28, 28);
                    lockRT.anchoredPosition = new Vector2(-8, 0);

                    lockImg = lockObj.AddComponent<Image>();
                    lockImg.preserveAspect = true;
                    lockImg.raycastTarget = false;
                }
                else
                {
                    lockImg = lockTransform.GetComponent<Image>();
                }

                // Set correct sprite and visibility
                if (lockImg != null)
                {
                    lockImg.sprite = lockSprite;
                    lockImg.gameObject.SetActive(showLock);
                }
            }
        }
    }
}
