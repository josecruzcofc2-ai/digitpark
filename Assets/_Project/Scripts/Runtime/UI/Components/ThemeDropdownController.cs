using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
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
        #pragma warning disable 0414
        private bool isInitialized = false;
        #pragma warning restore 0414
        private int lastValidIndex = 0;

        // Runtime lock icon references (one per dropdown item)
        private List<Image> lockIcons = new List<Image>();
        private bool wasDropdownOpen = false;

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

        private void Update()
        {
            if (dropdown == null) return;
            bool isOpen = FindDropdownList() != null;
            if (isOpen && !wasDropdownOpen)
            {
                StartCoroutine(UpdateLockIconsDelayed());
            }
            wasDropdownOpen = isOpen;
        }

        private IEnumerator UpdateLockIconsDelayed()
        {
            // Wait 2 frames: DropdownScrollFix reparents on frame 1, layout settles on frame 2
            yield return null;
            yield return null;
            UpdateLockIcons();
        }

        /// <summary>
        /// Finds the Dropdown List even after DropdownScrollFix reparents it to root Canvas.
        /// </summary>
        private Transform FindDropdownList()
        {
            // First check direct child (before DropdownScrollFix runs)
            Transform direct = dropdown.transform.Find("Dropdown List");
            if (direct != null) return direct;

            // After DropdownScrollFix, it's reparented to root Canvas
            Canvas rootCanvas = dropdown.GetComponentInParent<Canvas>()?.rootCanvas;
            if (rootCanvas != null)
            {
                return rootCanvas.transform.Find("Dropdown List");
            }
            return null;
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

                // No padding needed - lock icon is positioned absolutely via RectTransform

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

            // Find dropdown list (may be reparented to root Canvas by DropdownScrollFix)
            Transform dropdownList = FindDropdownList();
            if (dropdownList == null) return;

            // TMP_Dropdown structure: Dropdown List > Viewport > Content
            Transform content = dropdownList.Find("Viewport/Content");
            if (content == null)
                content = dropdownList.Find("Content");
            if (content == null) return;

            // TMP_Dropdown keeps the original Item template as first child (inactive).
            // Actual option items start at index 1.
            int themeIndex = 0;
            for (int i = 0; i < content.childCount && themeIndex < themes.Count; i++)
            {
                Transform item = content.GetChild(i);
                if (item == null) continue;

                // Skip the inactive template item
                if (!item.gameObject.activeSelf) continue;

                bool showLock = themes[themeIndex].isPremium && !IsThemeUnlocked(themes[themeIndex]);
                Sprite lockSprite = GetLockSpriteForTheme(themes[themeIndex]);

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

                // Adjust Item Label width: shrink when lock visible, full width when hidden
                Transform itemLabel = item.Find("Item Label");
                if (itemLabel != null)
                {
                    RectTransform labelRT = itemLabel.GetComponent<RectTransform>();
                    if (labelRT != null)
                        labelRT.offsetMax = new Vector2(showLock ? -40 : -10, labelRT.offsetMax.y);
                }

                themeIndex++;
            }
        }
    }
}
