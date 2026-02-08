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
    /// Shows lock icon for premium themes that haven't been purchased.
    /// Lock disappears when purchased OR when ThemeDebugController.UnlockAllThemes is ON.
    /// </summary>
    [RequireComponent(typeof(TMP_Dropdown))]
    public class ThemeDropdownController : MonoBehaviour
    {
        [Header("Lock Icon")]
        [SerializeField] private Sprite lockIconSprite;

        private TMP_Dropdown dropdown;
        private bool isInitialized = false;
        private int lastValidIndex = 0;

        // Runtime lock icon references (one per dropdown item)
        private List<Image> lockIcons = new List<Image>();

        private void Awake()
        {
            dropdown = GetComponent<TMP_Dropdown>();

            // Auto-load lock icon if not assigned
            if (lockIconSprite == null)
            {
                lockIconSprite = Resources.Load<Sprite>("UI/Icons/icon_lock_gold");
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

                // Add lock suffix for premium locked themes
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

            // Debug controller overrides
            if (ThemeDebugController.Instance != null && ThemeDebugController.Instance.UnlockAllThemes)
                return true;

            // PremiumDebugController (existing debug tool)
            if (PremiumDebugController.Instance != null && PremiumDebugController.Instance.AllowThemeChange)
                return true;

            // Check real purchase status
            if (PremiumManager.Instance != null)
                return PremiumManager.Instance.HasStylesPro;

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
        /// Called by the dropdown template when it creates items.
        /// Adds lock icon Image to premium items.
        /// Hook this after dropdown.Show() if needed.
        /// </summary>
        public void UpdateLockIcons()
        {
            if (lockIconSprite == null || ThemeManager.Instance == null) return;

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
                    lockImg.sprite = lockIconSprite;
                    lockImg.preserveAspect = true;
                    lockImg.raycastTarget = false;
                }
                else
                {
                    lockImg = lockTransform.GetComponent<Image>();
                }

                // Show lock only for premium themes that are locked
                bool showLock = themes[i].isPremium && !IsThemeUnlocked(themes[i]);
                if (lockImg != null)
                {
                    lockImg.gameObject.SetActive(showLock);
                }
            }
        }
    }
}
