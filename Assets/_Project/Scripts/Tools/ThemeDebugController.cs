using UnityEngine;

namespace DigitPark.Tools
{
    /// <summary>
    /// Debug controller to unlock/lock premium themes during runtime testing.
    /// Place on a persistent GameObject in Boot scene (DontDestroyOnLoad).
    /// When unlockAllThemes is ON, all premium themes are accessible without purchase.
    /// </summary>
    public class ThemeDebugController : MonoBehaviour
    {
        private static ThemeDebugController _instance;
        public static ThemeDebugController Instance => _instance;

        [Header("Theme Debug Settings")]
        [Tooltip("When ON, all premium themes are unlocked for testing")]
        [SerializeField] private bool unlockAllThemes = false;

        /// <summary>
        /// Returns true if all themes should be unlocked (debug mode)
        /// </summary>
        public bool UnlockAllThemes => unlockAllThemes;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Toggle unlock at runtime (can be called from UI or debug menu)
        /// </summary>
        public void SetUnlockAllThemes(bool unlock)
        {
            unlockAllThemes = unlock;
            Debug.Log($"[ThemeDebug] Themes unlock: {unlock}");

            // Refresh theme dropdown if ThemeManager exists
            if (Themes.ThemeManager.Instance != null)
            {
                Themes.ThemeManager.Instance.RefreshTheme();
            }
        }
    }
}
