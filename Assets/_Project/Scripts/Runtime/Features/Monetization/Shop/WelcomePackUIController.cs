using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;

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
        private Tween _lastChancePulseTween;
        private Tween _starterFomoTween;
        private Tween _premiumFomoTween;

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
            _lastChancePulseTween?.Kill();
            _starterFomoTween?.Kill();
            _premiumFomoTween?.Kill();
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
                SetActive(_starterBanner, false);
                SetActive(_premiumBanner, false);
                gameObject.SetActive(false);
                return;
            }

            bool starterVis = service.IsStarterVisible;
            bool premiumVis = service.IsPremiumVisible;

            gameObject.SetActive(starterVis || premiumVis);
            AnimateBannerIn(_starterBanner, starterVis);
            AnimateBannerIn(_premiumBanner, premiumVis);

            // "LAST CHANCE" pulse on D7 starter banner
            _lastChancePulseTween?.Kill();
            if (starterVis && service.IsStarterLastChance && _starterBanner != null)
            {
                _lastChancePulseTween = _starterBanner.transform
                    .DOScale(1.03f, 1f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetUpdate(true)
                    .SetLink(_starterBanner);
            }
        }

        private void AnimateBannerIn(GameObject banner, bool visible)
        {
            if (banner == null) return;

            if (!visible)
            {
                banner.SetActive(false);
                return;
            }

            bool wasActive = banner.activeSelf;
            banner.SetActive(true);

            if (!wasActive)
            {
                var cg = banner.GetComponent<CanvasGroup>();
                if (cg == null) cg = banner.AddComponent<CanvasGroup>();

                cg.alpha = 0f;
                banner.transform.localScale = Vector3.one * 0.9f;
                cg.DOFade(1f, 0.25f).SetUpdate(true).SetLink(banner);
                banner.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack)
                    .SetUpdate(true).SetLink(banner);
            }
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

                // FOMO pulse when < 1 hour
                if (remaining.TotalHours < 1 && _starterFomoTween == null)
                {
                    _starterFomoTween = _starterTimer.DOColor(new Color(1f, 0.27f, 0.27f), 0.5f)
                        .SetLoops(-1, LoopType.Yoyo).SetUpdate(true).SetLink(_starterTimer.gameObject);
                }
            }

            if (_premiumTimer != null && service.IsPremiumVisible)
            {
                var remaining = service.PremiumTimeRemaining;
                _premiumTimer.text = $"Expires in: {remaining.Days}d {remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";

                if (remaining.TotalHours < 1 && _premiumFomoTween == null)
                {
                    _premiumFomoTween = _premiumTimer.DOColor(new Color(1f, 0.27f, 0.27f), 0.5f)
                        .SetLoops(-1, LoopType.Yoyo).SetUpdate(true).SetLink(_premiumTimer.gameObject);
                }
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
