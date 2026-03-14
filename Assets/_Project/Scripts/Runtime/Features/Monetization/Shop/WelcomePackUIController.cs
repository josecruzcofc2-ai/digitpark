using System.Collections;
using UnityEngine;
using TMPro;

namespace DigitPark.Monetization
{
    /// <summary>
    /// Economy Rebalance V55 — Welcome Pack UI Controller
    ///
    /// Manages visibility of the 2 welcome pack banners based on WelcomePackService state.
    /// - StarterPackBanner: visible D1-D3, reappears D7 with "LAST CHANCE" badge
    /// - PremiumWelcomeBanner: visible D1-D5, no reappearance
    /// - Timer countdown updates every second
    /// - Hides entire container if no packs are visible (returning user)
    /// </summary>
    public class WelcomePackUIController : MonoBehaviour
    {
        private GameObject _starterBanner;
        private GameObject _premiumBanner;
        private TextMeshProUGUI _starterTimer;
        private TextMeshProUGUI _premiumTimer;
        private Coroutine _timerCoroutine;

        private void OnEnable()
        {
            FindBanners();
            RefreshVisibility();
            StartTimer();

            if (WelcomePackService.Instance != null)
                WelcomePackService.Instance.OnVisibilityChanged += RefreshVisibility;
        }

        private void OnDisable()
        {
            StopTimer();
            if (WelcomePackService.Instance != null)
                WelcomePackService.Instance.OnVisibilityChanged -= RefreshVisibility;
        }

        private void FindBanners()
        {
            _starterBanner = FindChild("StarterPackBanner");
            _premiumBanner = FindChild("PremiumWelcomeBanner");

            if (_starterBanner != null)
                _starterTimer = FindTimerText(_starterBanner, "StarterPackBannerTimer");
            if (_premiumBanner != null)
                _premiumTimer = FindTimerText(_premiumBanner, "PremiumWelcomeBannerTimer");
        }

        public void RefreshVisibility()
        {
            var service = WelcomePackService.Instance;
            if (service == null || !service.IsInitialized)
            {
                // Service not ready — hide all
                SetActive(_starterBanner, false);
                SetActive(_premiumBanner, false);
                gameObject.SetActive(false);
                return;
            }

            bool starterVis = service.IsStarterVisible;
            bool premiumVis = service.IsPremiumVisible;

            SetActive(_starterBanner, starterVis);
            SetActive(_premiumBanner, premiumVis);

            // Hide entire container if nothing to show
            gameObject.SetActive(starterVis || premiumVis);
        }

        private void UpdateTimers()
        {
            var service = WelcomePackService.Instance;
            if (service == null) return;

            if (_starterTimer != null && service.IsStarterVisible)
            {
                var remaining = service.StarterTimeRemaining;
                string prefix = service.IsStarterLastChance ? "LAST CHANCE: " : "Expires in: ";
                _starterTimer.text = $"{prefix}{remaining.Days}d {remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
            }

            if (_premiumTimer != null && service.IsPremiumVisible)
            {
                var remaining = service.PremiumTimeRemaining;
                _premiumTimer.text = $"Expires in: {remaining.Days}d {remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
            }
        }

        private void StartTimer()
        {
            StopTimer();
            _timerCoroutine = StartCoroutine(TimerCoroutine());
        }

        private void StopTimer()
        {
            if (_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine);
                _timerCoroutine = null;
            }
        }

        private IEnumerator TimerCoroutine()
        {
            while (true)
            {
                UpdateTimers();
                yield return new WaitForSeconds(1f);
            }
        }

        private GameObject FindChild(string name)
        {
            Transform t = FindDeep(transform, name);
            return t != null ? t.gameObject : null;
        }

        private TextMeshProUGUI FindTimerText(GameObject banner, string timerName)
        {
            Transform t = FindDeep(banner.transform, timerName);
            return t != null ? t.GetComponent<TextMeshProUGUI>() : null;
        }

        private static Transform FindDeep(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            foreach (Transform child in parent)
            {
                Transform found = FindDeep(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private static void SetActive(GameObject go, bool active)
        {
            if (go != null) go.SetActive(active);
        }
    }
}
