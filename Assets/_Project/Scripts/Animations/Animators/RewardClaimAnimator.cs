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

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            // Initialize current values
            currentValues[RewardType.Coins] = 0;
            currentValues[RewardType.Gems] = 0;
            currentValues[RewardType.Tickets] = 0;
        }

        // ==================== CURRENCY CLAIM ====================

        /// <summary>
        /// Claim coins with flying animation
        /// </summary>
        public void ClaimCoins(Vector3 sourcePosition, int amount, Action onComplete = null)
        {
            ClaimCurrency(RewardType.Coins, sourcePosition, amount, onComplete);
        }

        /// <summary>
        /// Claim gems with flying animation
        /// </summary>
        public void ClaimGems(Vector3 sourcePosition, int amount, Action onComplete = null)
        {
            ClaimCurrency(RewardType.Gems, sourcePosition, amount, onComplete);
        }

        /// <summary>
        /// Claim tickets with flying animation
        /// </summary>
        public void ClaimTickets(Vector3 sourcePosition, int amount, Action onComplete = null)
        {
            ClaimCurrency(RewardType.Tickets, sourcePosition, amount, onComplete);
        }

        /// <summary>
        /// Generic currency claim
        /// </summary>
        public void ClaimCurrency(RewardType type, Vector3 sourcePosition, int amount, Action onComplete = null)
        {
            StartCoroutine(ClaimCurrencyCoroutine(type, sourcePosition, amount, onComplete));
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
                Sequence flySeq = DOTween.Sequence();

                // Pop out
                flySeq.Append(iconRT.DOScale(1.3f, flyDuration * 0.2f).SetEase(Ease.OutQuad));

                // Fly to target with arc
                Vector3 midPoint = Vector3.Lerp(iconRT.position, target.position, 0.5f);
                midPoint.y += 100f;

                flySeq.Append(iconRT.DOPath(
                    new Vector3[] { iconRT.position, midPoint, target.position },
                    flyDuration * 0.8f,
                    PathType.CatmullRom
                ).SetEase(Ease.InQuad));

                flySeq.Join(iconRT.DOScale(0.5f, flyDuration * 0.8f));
                flySeq.Join(iconRT.DORotate(new Vector3(0, 0, 360f), flyDuration, RotateMode.FastBeyond360));

                flySeq.OnComplete(() =>
                {
                    // Play sound
                    if (sound != null && audioSource != null)
                        audioSource.PlayOneShot(sound, 0.5f);

                    // Bump target
                    target.DOPunchScale(Vector3.one * 0.2f, 0.15f, 5);

                    // Update counter
                    completedIcons++;
                    int newValue = startValue + (amountPerIcon * completedIcons);
                    if (completedIcons == iconsToSpawn)
                        newValue = endValue;

                    if (displayText != null)
                        displayText.text = newValue.ToString();

                    Destroy(icon);
                });

                yield return new WaitForSeconds(iconSpawnDelay);
            }

            // Wait for all icons
            yield return new WaitUntil(() => completedIcons >= iconsToSpawn);

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
                screenFlash.DOFade(0.6f, 0.15f).OnComplete(() =>
                    screenFlash.DOFade(0f, 0.3f).OnComplete(() =>
                        screenFlash.gameObject.SetActive(false)));
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

            Sequence popupSeq = DOTween.Sequence();
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

            rewardPopup.DOScale(0f, 0.2f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    rewardPopup.gameObject.SetActive(false);
                    onComplete?.Invoke();
                });
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

            DOTween.To(() => startValue, x =>
            {
                startValue = x;
                text.text = x.ToString();
            }, targetValue, counterAnimDuration).SetEase(Ease.OutQuad);

            // Punch scale
            text.transform.DOPunchScale(Vector3.one * 0.2f, counterAnimDuration, 5);

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
                RewardType.Coins => coinIconPrefab,
                RewardType.Gems => gemIconPrefab,
                RewardType.Tickets => ticketIconPrefab,
                _ => coinIconPrefab
            };
        }

        private RectTransform GetTarget(RewardType type)
        {
            return type switch
            {
                RewardType.Coins => coinsTarget,
                RewardType.Gems => gemsTarget,
                RewardType.Tickets => ticketsTarget,
                _ => coinsTarget
            };
        }

        private TextMeshProUGUI GetDisplayText(RewardType type)
        {
            return type switch
            {
                RewardType.Coins => coinsText,
                RewardType.Gems => gemsText,
                RewardType.Tickets => ticketsText,
                _ => coinsText
            };
        }

        private AudioClip GetSound(RewardType type)
        {
            return type switch
            {
                RewardType.Coins => coinSound,
                RewardType.Gems => gemSound,
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
            DOTween.Kill(rewardPopup);
        }
    }

    public enum RewardType
    {
        Coins,
        Gems,
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
