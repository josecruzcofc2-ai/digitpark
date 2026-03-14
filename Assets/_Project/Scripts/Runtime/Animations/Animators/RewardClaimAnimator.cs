using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DigitPark.Animations
{
    /// <summary>
    /// Handles reward claim animations including fly-to-target effects,
    /// currency counters, and celebration effects.
    /// </summary>
    public class RewardClaimAnimator : MonoBehaviour
    {
        [Header("Currency Targets")]
        [SerializeField] private RectTransform coinsTarget;
        [SerializeField] private RectTransform gemsTarget;
        [SerializeField] private RectTransform ticketsTarget;

        [Header("Currency Displays")]
        [SerializeField] private TextMeshProUGUI coinsText;
        [SerializeField] private TextMeshProUGUI gemsText;
        [SerializeField] private TextMeshProUGUI ticketsText;

        [Header("Flying Icons")]
        [SerializeField] private GameObject coinIconPrefab;
        [SerializeField] private GameObject gemIconPrefab;
        [SerializeField] private GameObject ticketIconPrefab;
        [SerializeField] private Transform flyingIconsParent;

        [Header("Celebration Effects")]
        [SerializeField] private ParticleSystem celebrationParticles;
        [SerializeField] private ParticleSystem confettiParticles;
        [SerializeField] private Image screenFlash;

        [Header("Reward Popup")]
        [SerializeField] private RectTransform rewardPopup;
        [SerializeField] private Image rewardIcon;
        [SerializeField] private TextMeshProUGUI rewardNameText;
        [SerializeField] private TextMeshProUGUI rewardAmountText;

        [Header("Animation Settings")]
        [SerializeField] private int maxFlyingIcons = 10;
        [SerializeField] private float flyDuration = 0.6f;
        [SerializeField] private float iconSpawnDelay = 0.05f;
        [SerializeField] private float counterAnimDuration = 0.5f;

        [Header("Audio")]
        [SerializeField] private AudioClip coinSound;
        [SerializeField] private AudioClip gemSound;
        [SerializeField] private AudioClip ticketSound;
        [SerializeField] private AudioClip bigRewardSound;
        [SerializeField] private AudioClip celebrationSound;

        // Events
        public event Action<RewardType, int> OnRewardClaimed;
        public event Action OnAllRewardsClaimed;

        private AudioSource audioSource;
        private Dictionary<RewardType, int> currentValues = new Dictionary<RewardType, int>();
        private readonly List<Tween> _activeTweens = new List<Tween>();

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            // Initialize current values
            currentValues[RewardType.DigitCoins] = 0;
            currentValues[RewardType.DigitGems] = 0;
            currentValues[RewardType.Tickets] = 0;
        }

        // ==================== CURRENCY CLAIM ====================

        /// <summary>
        /// Claim coins with flying animation
        /// </summary>
        public void ClaimCoins(Vector3 sourcePosition, int amount, Action onComplete = null)
        {
            ClaimCurrency(RewardType.DigitCoins, sourcePosition, amount, onComplete);
        }

        /// <summary>
        /// Claim gems with flying animation
        /// </summary>
        public void ClaimGems(Vector3 sourcePosition, int amount, Action onComplete = null)
        {
            ClaimCurrency(RewardType.DigitGems, sourcePosition, amount, onComplete);
        }

        /// <summary>
        /// Claim tickets with flying animation
        /// </summary>
        public void ClaimTickets(Vector3 sourcePosition, int amount, Action onComplete = null)
        {
            ClaimCurrency(RewardType.Tickets, sourcePosition, amount, onComplete);
        }

        /// <summary>
        /// Generic currency claim. Automatically uses shower mode for large amounts (100+).
        /// </summary>
        public void ClaimCurrency(RewardType type, Vector3 sourcePosition, int amount, Action onComplete = null)
        {
            if (amount >= 100)
            {
                ClaimCurrencyShower(type, sourcePosition, amount, onComplete);
            }
            else
            {
                StartCoroutine(ClaimCurrencyCoroutine(type, sourcePosition, amount, onComplete));
            }
        }

        private IEnumerator ClaimCurrencyCoroutine(RewardType type, Vector3 sourcePos, int amount, Action onComplete)
        {
            GameObject prefab = GetIconPrefab(type);
            RectTransform target = GetTarget(type);
            TextMeshProUGUI displayText = GetDisplayText(type);
            AudioClip sound = GetSound(type);

            if (prefab == null || target == null) yield break;

            int iconsToSpawn = Mathf.Min(amount, maxFlyingIcons);
            int amountPerIcon = amount / iconsToSpawn;
            int completedIcons = 0;

            int startValue = currentValues[type];
            int endValue = startValue + amount;

            for (int i = 0; i < iconsToSpawn; i++)
            {
                // Spawn icon
                GameObject icon = Instantiate(prefab, flyingIconsParent);
                RectTransform iconRT = icon.GetComponent<RectTransform>();
                iconRT.position = sourcePos;
                icon.SetActive(true);

                // Add random offset to source
                Vector3 randomOffset = new Vector3(
                    UnityEngine.Random.Range(-50f, 50f),
                    UnityEngine.Random.Range(-50f, 50f),
                    0f
                );
                iconRT.position += randomOffset;

                // Animate
                int iconIndex = i;
                Vector3 startPos = iconRT.position;
                Vector3 midPoint = Vector3.Lerp(startPos, target.position, 0.5f);
                midPoint.y += 100f;

                Sequence flySeq = DOTween.Sequence().SetLink(icon);

                // Pop out
                flySeq.Append(iconRT.DOScale(1.3f, flyDuration * 0.2f).SetEase(Ease.OutQuad));

                // Fly to target with arc (use cached startPos)
                flySeq.Append(iconRT.DOPath(
                    new Vector3[] { startPos, midPoint, target.position },
                    flyDuration * 0.8f,
                    PathType.CatmullRom
                ).SetEase(Ease.InQuad));

                flySeq.Join(iconRT.DOScale(0.5f, flyDuration * 0.8f));
                flySeq.Join(iconRT.DORotate(new Vector3(0, 0, 360f), flyDuration, RotateMode.FastBeyond360));

                flySeq.OnComplete(() =>
                {
                    _activeTweens.Remove(flySeq);

                    // Play sound
                    if (sound != null && audioSource != null)
                        audioSource.PlayOneShot(sound, 0.5f);

                    // Bump target
                    if (target != null)
                    {
                        var bump = target.DOPunchScale(Vector3.one * 0.2f, 0.15f, 5);
                        _activeTweens.Add(bump);
                        bump.OnComplete(() => _activeTweens.Remove(bump));
                    }

                    // Update counter
                    completedIcons++;
                    int newValue = startValue + (amountPerIcon * completedIcons);
                    if (completedIcons == iconsToSpawn)
                        newValue = endValue;

                    if (displayText != null)
                        displayText.text = newValue.ToString();

                    Destroy(icon);
                });

                _activeTweens.Add(flySeq);

                yield return new WaitForSeconds(iconSpawnDelay);
            }

            // Wait for all icons
            yield return new WaitUntil(() => completedIcons >= iconsToSpawn);

            currentValues[type] = endValue;
            OnRewardClaimed?.Invoke(type, amount);
            onComplete?.Invoke();
        }

        // ==================== SHOWER MODE (LARGE AMOUNTS) ====================

        /// <summary>
        /// Shower mode for large currency amounts (100+).
        /// Icons burst out in a radial pattern, then fly to target.
        /// </summary>
        public void ClaimCurrencyShower(RewardType type, Vector3 sourcePos, int amount, Action onComplete = null)
        {
            StartCoroutine(ShowerCoroutine(type, sourcePos, amount, onComplete));
        }

        private IEnumerator ShowerCoroutine(RewardType type, Vector3 sourcePos, int amount, Action onComplete)
        {
            GameObject prefab = GetIconPrefab(type);
            RectTransform target = GetTarget(type);
            TextMeshProUGUI displayText = GetDisplayText(type);
            AudioClip sound = GetSound(type);

            if (prefab == null || target == null) yield break;

            // Screen flash for shower
            if (UIAnimationManager.Instance != null)
                UIAnimationManager.Instance.GoldFlash();

            int iconsToSpawn = Mathf.Min(amount / 5, 20); // More icons for shower
            if (iconsToSpawn < 5) iconsToSpawn = 5;
            int completedIcons = 0;
            int startValue = currentValues[type];
            int endValue = startValue + amount;

            // Phase 1: Burst out radially
            List<RectTransform> spawnedIcons = new List<RectTransform>();

            for (int i = 0; i < iconsToSpawn; i++)
            {
                GameObject icon = Instantiate(prefab, flyingIconsParent);
                RectTransform iconRT = icon.GetComponent<RectTransform>();
                iconRT.position = sourcePos;
                iconRT.localScale = Vector3.zero;
                icon.SetActive(true);
                spawnedIcons.Add(iconRT);

                // Burst outward in random direction
                float angle = (360f / iconsToSpawn) * i + UnityEngine.Random.Range(-15f, 15f);
                float distance = UnityEngine.Random.Range(80f, 200f);
                Vector3 burstTarget = sourcePos + new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * distance,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * distance,
                    0f
                );

                var burstSeq = DOTween.Sequence().SetLink(icon);
                burstSeq.Append(iconRT.DOScale(1.5f, 0.15f).SetEase(Ease.OutBack));
                burstSeq.Join(iconRT.DOMove(burstTarget, 0.2f).SetEase(Ease.OutQuad));
                burstSeq.Append(iconRT.DOScale(1f, 0.1f));
                _activeTweens.Add(burstSeq);
                burstSeq.OnKill(() => _activeTweens.Remove(burstSeq));
            }

            yield return new WaitForSeconds(0.35f);

            // Phase 2: All fly to target with stagger
            for (int i = 0; i < spawnedIcons.Count; i++)
            {
                var iconRT = spawnedIcons[i];
                if (iconRT == null) continue;

                int index = i;
                Vector3 startPos = iconRT.position;
                Sequence flySeq = DOTween.Sequence().SetLink(iconRT.gameObject);

                flySeq.Append(iconRT.DOMove(target.position, flyDuration * 0.6f).SetEase(Ease.InQuad));
                flySeq.Join(iconRT.DOScale(0.4f, flyDuration * 0.6f));
                flySeq.Join(iconRT.DORotate(new Vector3(0, 0, 720f), flyDuration * 0.6f, RotateMode.FastBeyond360));

                flySeq.OnComplete(() =>
                {
                    _activeTweens.Remove(flySeq);

                    if (sound != null && audioSource != null)
                        audioSource.PlayOneShot(sound, 0.3f);

                    // Rapid counter tick
                    completedIcons++;
                    int newValue = startValue + Mathf.RoundToInt((float)completedIcons / iconsToSpawn * amount);
                    if (completedIcons >= iconsToSpawn) newValue = endValue;

                    if (displayText != null)
                        displayText.text = newValue.ToString();

                    // Bump on each arrival
                    if (target != null)
                    {
                        var bump = target.DOPunchScale(Vector3.one * 0.15f, 0.1f, 3);
                        _activeTweens.Add(bump);
                        bump.OnComplete(() => _activeTweens.Remove(bump));
                    }

                    Destroy(iconRT.gameObject);
                });

                _activeTweens.Add(flySeq);

                yield return new WaitForSeconds(0.03f); // Rapid stagger
            }

            yield return new WaitUntil(() => completedIcons >= iconsToSpawn);

            // Final big bump on counter
            if (target != null)
            {
                var finalBump = target.DOPunchScale(Vector3.one * 0.3f, 0.3f, 6, 0.5f);
                _activeTweens.Add(finalBump);
                finalBump.OnKill(() => _activeTweens.Remove(finalBump));
            }

            currentValues[type] = endValue;
            OnRewardClaimed?.Invoke(type, amount);
            onComplete?.Invoke();
        }

        // ==================== REWARD POPUP ====================

        /// <summary>
        /// Show reward popup with celebration
        /// </summary>
        public void ShowRewardPopup(Sprite icon, string name, int amount, bool isBigReward = false)
        {
            StartCoroutine(RewardPopupCoroutine(icon, name, amount, isBigReward));
        }

        private IEnumerator RewardPopupCoroutine(Sprite icon, string name, int amount, bool isBig)
        {
            if (rewardPopup == null) yield break;

            // Setup
            if (rewardIcon != null)
                rewardIcon.sprite = icon;

            if (rewardNameText != null)
                rewardNameText.text = name;

            if (rewardAmountText != null)
                rewardAmountText.text = $"x{amount}";

            // Flash for big rewards
            if (isBig && screenFlash != null)
            {
                screenFlash.gameObject.SetActive(true);
                screenFlash.color = new Color(1f, 0.9f, 0.3f, 0f);
                var flashSeq = DOTween.Sequence().SetLink(screenFlash.gameObject);
                flashSeq.Append(screenFlash.DOFade(0.6f, 0.15f));
                flashSeq.Append(screenFlash.DOFade(0f, 0.3f));
                flashSeq.OnComplete(() => screenFlash.gameObject.SetActive(false));
                _activeTweens.Add(flashSeq);
                flashSeq.OnKill(() => _activeTweens.Remove(flashSeq));
            }

            // Play sound
            AudioClip soundToPlay = isBig ? bigRewardSound : celebrationSound;
            if (soundToPlay != null && audioSource != null)
                audioSource.PlayOneShot(soundToPlay);

            // Particles
            if (celebrationParticles != null)
                celebrationParticles.Play();

            if (isBig && confettiParticles != null)
                confettiParticles.Play();

            // Show popup
            rewardPopup.gameObject.SetActive(true);
            rewardPopup.localScale = Vector3.zero;

            Sequence popupSeq = DOTween.Sequence().SetLink(rewardPopup.gameObject);
            _activeTweens.Add(popupSeq);
            popupSeq.Append(rewardPopup.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack));
            popupSeq.Append(rewardPopup.DOScale(1f, 0.2f).SetEase(Ease.InOutQuad));

            if (isBig)
            {
                popupSeq.Join(rewardPopup.DOPunchRotation(new Vector3(0, 0, 10f), 0.5f, 10));
            }

            // Icon bounce
            if (rewardIcon != null)
            {
                popupSeq.Insert(0.3f, rewardIcon.transform.DOPunchScale(Vector3.one * 0.3f, 0.4f, 5));
            }

            popupSeq.OnKill(() => _activeTweens.Remove(popupSeq));
            yield return popupSeq.WaitForCompletion();
        }

        /// <summary>
        /// Hide reward popup
        /// </summary>
        public void HideRewardPopup(Action onComplete = null)
        {
            if (rewardPopup == null)
            {
                onComplete?.Invoke();
                return;
            }

            var hideTween = rewardPopup.DOScale(0f, 0.2f)
                .SetEase(Ease.InBack)
                .SetLink(rewardPopup.gameObject)
                .OnComplete(() =>
                {
                    rewardPopup.gameObject.SetActive(false);
                    onComplete?.Invoke();
                });
            _activeTweens.Add(hideTween);
            hideTween.OnKill(() => _activeTweens.Remove(hideTween));
        }

        // ==================== MULTIPLE REWARDS ====================

        /// <summary>
        /// Claim multiple rewards in sequence
        /// </summary>
        public void ClaimMultipleRewards(List<RewardData> rewards, Vector3 sourcePosition, Action onAllComplete = null)
        {
            StartCoroutine(ClaimMultipleCoroutine(rewards, sourcePosition, onAllComplete));
        }

        private IEnumerator ClaimMultipleCoroutine(List<RewardData> rewards, Vector3 sourcePos, Action onComplete)
        {
            foreach (var reward in rewards)
            {
                bool completed = false;
                ClaimCurrency(reward.type, sourcePos, reward.amount, () => completed = true);
                yield return new WaitUntil(() => completed);
                yield return new WaitForSeconds(0.2f);
            }

            OnAllRewardsClaimed?.Invoke();
            onComplete?.Invoke();
        }

        // ==================== COUNTER ANIMATIONS ====================

        /// <summary>
        /// Animate counter from current value
        /// </summary>
        public void AnimateCounter(RewardType type, int targetValue)
        {
            TextMeshProUGUI text = GetDisplayText(type);
            if (text == null) return;

            int startValue = currentValues[type];

            var counterTween = DOTween.To(() => startValue, x =>
            {
                startValue = x;
                if (text != null) text.text = x.ToString();
            }, targetValue, counterAnimDuration).SetEase(Ease.OutQuad);
            _activeTweens.Add(counterTween);
            counterTween.OnKill(() => _activeTweens.Remove(counterTween));

            // Punch scale
            var punchTween = text.transform.DOPunchScale(Vector3.one * 0.2f, counterAnimDuration, 5);
            _activeTweens.Add(punchTween);
            punchTween.OnKill(() => _activeTweens.Remove(punchTween));

            currentValues[type] = targetValue;
        }

        /// <summary>
        /// Set counter value instantly
        /// </summary>
        public void SetCounterValue(RewardType type, int value)
        {
            currentValues[type] = value;

            TextMeshProUGUI text = GetDisplayText(type);
            if (text != null)
                text.text = value.ToString();
        }

        // ==================== CELEBRATION EFFECTS ====================

        /// <summary>
        /// Play celebration without rewards
        /// </summary>
        public void PlayCelebration(bool big = false)
        {
            if (celebrationParticles != null)
                celebrationParticles.Play();

            if (big && confettiParticles != null)
                confettiParticles.Play();

            if (celebrationSound != null && audioSource != null)
                audioSource.PlayOneShot(celebrationSound);

            if (UIAnimationManager.Instance != null)
            {
                if (big)
                    UIAnimationManager.Instance.GoldFlash();
                else
                    UIAnimationManager.Instance.WhiteFlash(0.15f);
            }
        }

        // ==================== HELPERS ====================

        private GameObject GetIconPrefab(RewardType type)
        {
            return type switch
            {
                RewardType.DigitCoins => coinIconPrefab,
                RewardType.DigitGems => gemIconPrefab,
                RewardType.Tickets => ticketIconPrefab,
                _ => coinIconPrefab
            };
        }

        private RectTransform GetTarget(RewardType type)
        {
            return type switch
            {
                RewardType.DigitCoins => coinsTarget,
                RewardType.DigitGems => gemsTarget,
                RewardType.Tickets => ticketsTarget,
                _ => coinsTarget
            };
        }

        private TextMeshProUGUI GetDisplayText(RewardType type)
        {
            return type switch
            {
                RewardType.DigitCoins => coinsText,
                RewardType.DigitGems => gemsText,
                RewardType.Tickets => ticketsText,
                _ => coinsText
            };
        }

        private AudioClip GetSound(RewardType type)
        {
            return type switch
            {
                RewardType.DigitCoins => coinSound,
                RewardType.DigitGems => gemSound,
                RewardType.Tickets => ticketSound,
                _ => coinSound
            };
        }

        /// <summary>
        /// Set currency targets at runtime
        /// </summary>
        public void SetTargets(RectTransform coins, RectTransform gems, RectTransform tickets)
        {
            coinsTarget = coins;
            gemsTarget = gems;
            ticketsTarget = tickets;
        }

        private void OnDestroy()
        {
            // Kill all tracked tweens
            for (int i = _activeTweens.Count - 1; i >= 0; i--)
                _activeTweens[i]?.Kill();
            _activeTweens.Clear();

            DOTween.Kill(rewardPopup);
            if (screenFlash != null) screenFlash.DOKill();
        }
    }

    public enum RewardType
    {
        DigitCoins,
        DigitGems,
        Tickets
    }

    [System.Serializable]
    public class RewardData
    {
        public RewardType type;
        public int amount;
        public Sprite icon;
        public string name;
    }
}
