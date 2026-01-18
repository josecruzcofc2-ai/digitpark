using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DigitPark.Animations
{
    /// <summary>
    /// Professional Battle Pass controller with Clash Royale-style animations.
    /// Features: Snap scrolling, tier scaling, parallax, unlock animations.
    /// Attach to the TiersScrollView GameObject.
    /// </summary>
    public class BattlePassController : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        [Header("References")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform content;
        [SerializeField] private RectTransform viewport;

        [Header("Tier Settings")]
        [SerializeField] private float tierWidth = 150f;
        [SerializeField] private float tierSpacing = 0f;
        [SerializeField] private int totalTiers = 30;
        [SerializeField] private int currentTier = 12;

        [Header("Snap Settings")]
        [SerializeField] private bool enableSnap = true;
        [SerializeField] private float snapSpeed = 10f;
        [SerializeField] private float snapThreshold = 0.1f;

        [Header("Scale Effect")]
        [SerializeField] private float selectedScale = 1.15f;
        [SerializeField] private float nearbyScale = 1.0f;
        [SerializeField] private float farScale = 0.85f;
        [SerializeField] private float scaleTransitionSpeed = 8f;

        [Header("Visual Effects")]
        [SerializeField] private bool enableGlow = true;
        [SerializeField] private Color glowColor = new Color(0f, 1f, 1f, 0.5f);
        [SerializeField] private float glowPulseSpeed = 1.5f;

        [Header("Parallax Background")]
        [SerializeField] private RectTransform parallaxLayer1;
        [SerializeField] private RectTransform parallaxLayer2;
        [SerializeField] private float parallaxFactor1 = 0.3f;
        [SerializeField] private float parallaxFactor2 = 0.5f;

        [Header("Audio")]
        [SerializeField] private AudioClip scrollTickSound;
        [SerializeField] private AudioClip selectSound;
        [SerializeField] private AudioClip claimSound;
        [SerializeField] private AudioClip unlockSound;

        // Events
        public event Action<int> OnTierSelected;
        public event Action<int> OnTierClaimed;
        public event Action<int> OnTierUnlocked;

        // Private
        private List<RectTransform> tierItems = new List<RectTransform>();
        private List<CanvasGroup> tierCanvasGroups = new List<CanvasGroup>();
        private int selectedIndex = 0;
        private int lastSelectedIndex = -1;
        private bool isDragging = false;
        private bool isSnapping = false;
        private float targetScrollPosition;
        private AudioSource audioSource;
        private Image currentGlow;
        private Tween glowTween;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        private void Start()
        {
            InitializeReferences();
            CollectTierItems();
            SetupInitialState();
            StartCoroutine(EntranceAnimation());
        }

        private void Update()
        {
            if (!isDragging && enableSnap)
            {
                HandleSnapScrolling();
            }

            UpdateTierScales();
            UpdateParallax();
            CheckSelectedTierChanged();
        }

        // ==================== INITIALIZATION ====================

        private void InitializeReferences()
        {
            if (scrollRect == null)
                scrollRect = GetComponent<ScrollRect>();

            if (scrollRect != null)
            {
                if (content == null)
                    content = scrollRect.content;
                if (viewport == null)
                    viewport = scrollRect.viewport;
            }
        }

        private void CollectTierItems()
        {
            tierItems.Clear();
            tierCanvasGroups.Clear();

            if (content == null) return;

            for (int i = 0; i < content.childCount; i++)
            {
                Transform child = content.GetChild(i);
                if (child.name.StartsWith("Tier_"))
                {
                    RectTransform rt = child.GetComponent<RectTransform>();
                    tierItems.Add(rt);

                    // Add CanvasGroup for alpha control
                    CanvasGroup cg = child.GetComponent<CanvasGroup>();
                    if (cg == null)
                        cg = child.gameObject.AddComponent<CanvasGroup>();
                    tierCanvasGroups.Add(cg);
                }
            }

            totalTiers = tierItems.Count;
            Debug.Log($"[BattlePassController] Found {totalTiers} tiers");
        }

        private void SetupInitialState()
        {
            // Scroll to current tier
            selectedIndex = Mathf.Clamp(currentTier - 1, 0, totalTiers - 1);
            ScrollToTier(selectedIndex, false);

            // Setup glow on current tier
            if (enableGlow && selectedIndex < tierItems.Count)
            {
                CreateGlowEffect(tierItems[selectedIndex]);
            }
        }

        // ==================== ENTRANCE ANIMATION ====================

        private IEnumerator EntranceAnimation()
        {
            // Hide all tiers initially
            foreach (var cg in tierCanvasGroups)
            {
                cg.alpha = 0f;
            }

            foreach (var tier in tierItems)
            {
                tier.localScale = Vector3.zero;
            }

            yield return new WaitForSeconds(0.3f);

            // Staggered entrance from current tier outward
            int centerIndex = selectedIndex;
            float staggerDelay = 0.05f;
            int maxDistance = Mathf.Max(centerIndex, totalTiers - centerIndex - 1);

            for (int d = 0; d <= maxDistance; d++)
            {
                // Left side
                int leftIndex = centerIndex - d;
                if (leftIndex >= 0 && leftIndex < tierItems.Count)
                {
                    AnimateTierEntrance(leftIndex, d * staggerDelay);
                }

                // Right side (skip center on first iteration)
                if (d > 0)
                {
                    int rightIndex = centerIndex + d;
                    if (rightIndex < tierItems.Count)
                    {
                        AnimateTierEntrance(rightIndex, d * staggerDelay);
                    }
                }

                yield return new WaitForSeconds(staggerDelay * 0.5f);
            }
        }

        private void AnimateTierEntrance(int index, float delay)
        {
            if (index < 0 || index >= tierItems.Count) return;

            RectTransform tier = tierItems[index];
            CanvasGroup cg = tierCanvasGroups[index];

            float targetScale = GetTargetScale(index);

            DOTween.Sequence()
                .SetDelay(delay)
                .Append(tier.DOScale(targetScale * 1.2f, 0.2f).SetEase(Ease.OutQuad))
                .Join(cg.DOFade(1f, 0.2f))
                .Append(tier.DOScale(targetScale, 0.15f).SetEase(Ease.OutBack));
        }

        // ==================== SNAP SCROLLING ====================

        private void HandleSnapScrolling()
        {
            if (scrollRect == null || content == null) return;

            // Calculate current scroll position
            float scrollPos = content.anchoredPosition.x;
            float viewportCenter = viewport.rect.width / 2f;

            // Find closest tier to center
            int closestIndex = 0;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < tierItems.Count; i++)
            {
                float tierCenter = GetTierCenterPosition(i);
                float distance = Mathf.Abs(-scrollPos + viewportCenter - tierCenter);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex = i;
                }
            }

            selectedIndex = closestIndex;

            // Snap to closest tier
            if (!isSnapping && Mathf.Abs(scrollRect.velocity.x) < 100f)
            {
                float targetX = -(GetTierCenterPosition(closestIndex) - viewportCenter);

                if (Mathf.Abs(scrollPos - targetX) > snapThreshold)
                {
                    content.DOAnchorPosX(targetX, 0.3f)
                        .SetEase(Ease.OutQuad)
                        .OnStart(() => isSnapping = true)
                        .OnComplete(() => isSnapping = false);
                }
            }
        }

        private float GetTierCenterPosition(int index)
        {
            if (index < 0 || index >= tierItems.Count) return 0;

            // Calculate position based on tier width and spacing
            float padding = 20f; // Match the padding in BattlePassUIBuilder
            return padding + (tierWidth * index) + (tierWidth / 2f);
        }

        // ==================== SCALE EFFECTS ====================

        private void UpdateTierScales()
        {
            if (content == null) return;

            float scrollPos = -content.anchoredPosition.x;
            float viewportCenter = viewport.rect.width / 2f;

            for (int i = 0; i < tierItems.Count; i++)
            {
                float tierCenter = GetTierCenterPosition(i);
                float distanceFromCenter = Mathf.Abs(scrollPos + viewportCenter - tierCenter);

                float targetScale = GetTargetScale(i, distanceFromCenter);

                // Smooth scale transition
                tierItems[i].localScale = Vector3.Lerp(
                    tierItems[i].localScale,
                    Vector3.one * targetScale,
                    Time.deltaTime * scaleTransitionSpeed
                );

                // Alpha based on distance
                if (i < tierCanvasGroups.Count)
                {
                    float targetAlpha = distanceFromCenter < tierWidth * 2 ? 1f : 0.7f;
                    tierCanvasGroups[i].alpha = Mathf.Lerp(
                        tierCanvasGroups[i].alpha,
                        targetAlpha,
                        Time.deltaTime * scaleTransitionSpeed
                    );
                }
            }
        }

        private float GetTargetScale(int index, float distanceFromCenter = -1)
        {
            if (distanceFromCenter < 0)
            {
                // Simple calculation based on index vs selected
                int distanceFromSelected = Mathf.Abs(index - selectedIndex);
                if (distanceFromSelected == 0) return selectedScale;
                if (distanceFromSelected == 1) return nearbyScale;
                return farScale;
            }

            // Distance-based calculation
            if (distanceFromCenter < tierWidth * 0.5f)
                return selectedScale;
            else if (distanceFromCenter < tierWidth * 1.5f)
                return Mathf.Lerp(selectedScale, nearbyScale, (distanceFromCenter - tierWidth * 0.5f) / tierWidth);
            else if (distanceFromCenter < tierWidth * 2.5f)
                return Mathf.Lerp(nearbyScale, farScale, (distanceFromCenter - tierWidth * 1.5f) / tierWidth);
            else
                return farScale;
        }

        // ==================== PARALLAX ====================

        private void UpdateParallax()
        {
            if (content == null) return;

            float scrollPos = content.anchoredPosition.x;

            if (parallaxLayer1 != null)
            {
                parallaxLayer1.anchoredPosition = new Vector2(scrollPos * parallaxFactor1, parallaxLayer1.anchoredPosition.y);
            }

            if (parallaxLayer2 != null)
            {
                parallaxLayer2.anchoredPosition = new Vector2(scrollPos * parallaxFactor2, parallaxLayer2.anchoredPosition.y);
            }
        }

        // ==================== SELECTION CHANGE ====================

        private void CheckSelectedTierChanged()
        {
            if (selectedIndex != lastSelectedIndex)
            {
                OnTierSelectionChanged(lastSelectedIndex, selectedIndex);
                lastSelectedIndex = selectedIndex;
            }
        }

        private void OnTierSelectionChanged(int oldIndex, int newIndex)
        {
            // Play tick sound
            if (scrollTickSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(scrollTickSound, 0.3f);
            }

            // Move glow to new tier
            if (enableGlow && newIndex < tierItems.Count)
            {
                MoveGlowToTier(tierItems[newIndex]);
            }

            OnTierSelected?.Invoke(newIndex + 1);
        }

        // ==================== GLOW EFFECT ====================

        private void CreateGlowEffect(RectTransform tier)
        {
            if (currentGlow != null)
                Destroy(currentGlow.gameObject);

            GameObject glowObj = new GameObject("SelectionGlow");
            glowObj.transform.SetParent(tier, false);
            glowObj.transform.SetAsFirstSibling();

            RectTransform glowRT = glowObj.AddComponent<RectTransform>();
            glowRT.anchorMin = Vector2.zero;
            glowRT.anchorMax = Vector2.one;
            glowRT.sizeDelta = new Vector2(20, 20);

            currentGlow = glowObj.AddComponent<Image>();
            currentGlow.color = glowColor;
            currentGlow.raycastTarget = false;

            // Start pulse animation
            StartGlowPulse();
        }

        private void MoveGlowToTier(RectTransform tier)
        {
            if (currentGlow == null)
            {
                CreateGlowEffect(tier);
                return;
            }

            glowTween?.Kill();

            // Animate glow to new position
            currentGlow.transform.SetParent(tier, true);
            currentGlow.transform.SetAsFirstSibling();

            RectTransform glowRT = currentGlow.rectTransform;
            glowRT.DOAnchorPos(Vector2.zero, 0.2f).SetEase(Ease.OutQuad);

            StartGlowPulse();
        }

        private void StartGlowPulse()
        {
            if (currentGlow == null) return;

            glowTween?.Kill();
            glowTween = currentGlow.DOFade(0.8f, glowPulseSpeed / 2f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        // ==================== DRAG HANDLERS ====================

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = true;
            isSnapping = false;
            content.DOKill();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
        }

        // ==================== PUBLIC API ====================

        /// <summary>
        /// Scroll to specific tier with animation
        /// </summary>
        public void ScrollToTier(int tierIndex, bool animate = true)
        {
            tierIndex = Mathf.Clamp(tierIndex, 0, totalTiers - 1);

            float viewportCenter = viewport.rect.width / 2f;
            float targetX = -(GetTierCenterPosition(tierIndex) - viewportCenter);

            if (animate)
            {
                content.DOAnchorPosX(targetX, 0.5f).SetEase(Ease.OutQuad);
            }
            else
            {
                content.anchoredPosition = new Vector2(targetX, content.anchoredPosition.y);
            }

            selectedIndex = tierIndex;
        }

        /// <summary>
        /// Play tier unlock animation
        /// </summary>
        public void PlayTierUnlock(int tierIndex)
        {
            if (tierIndex < 0 || tierIndex >= tierItems.Count) return;

            if (unlockSound != null && audioSource != null)
                audioSource.PlayOneShot(unlockSound);

            RectTransform tier = tierItems[tierIndex];

            // Flash effect
            if (UIAnimationManager.Instance != null)
                UIAnimationManager.Instance.GoldFlash(0.3f);

            // Tier animation
            DOTween.Sequence()
                .Append(tier.DOScale(selectedScale * 1.3f, 0.2f).SetEase(Ease.OutQuad))
                .Append(tier.DOScale(selectedScale, 0.3f).SetEase(Ease.OutBounce))
                .Join(tier.DOShakeRotation(0.3f, new Vector3(0, 0, 10f), 10));

            OnTierUnlocked?.Invoke(tierIndex + 1);
        }

        /// <summary>
        /// Play reward claim animation
        /// </summary>
        public void PlayRewardClaim(int tierIndex, RectTransform targetPosition, Action onComplete = null)
        {
            if (tierIndex < 0 || tierIndex >= tierItems.Count) return;

            if (claimSound != null && audioSource != null)
                audioSource.PlayOneShot(claimSound);

            RectTransform tier = tierItems[tierIndex];

            // Find reward icon in tier
            Transform rewardSlot = tier.Find("FreeReward") ?? tier.Find("PremiumReward");
            if (rewardSlot != null)
            {
                // Create flying reward clone
                GameObject clone = Instantiate(rewardSlot.gameObject, transform.parent);
                RectTransform cloneRT = clone.GetComponent<RectTransform>();
                cloneRT.position = rewardSlot.position;

                DOTween.Sequence()
                    .Append(cloneRT.DOScale(1.5f, 0.2f).SetEase(Ease.OutQuad))
                    .Append(cloneRT.DOMove(targetPosition.position, 0.5f).SetEase(Ease.InBack))
                    .Join(cloneRT.DOScale(0.3f, 0.5f))
                    .OnComplete(() =>
                    {
                        Destroy(clone);
                        onComplete?.Invoke();
                    });
            }

            OnTierClaimed?.Invoke(tierIndex + 1);
        }

        /// <summary>
        /// Set current progress tier
        /// </summary>
        public void SetCurrentTier(int tier)
        {
            currentTier = tier;
            selectedIndex = Mathf.Clamp(tier - 1, 0, totalTiers - 1);
        }

        /// <summary>
        /// Get currently selected tier (1-indexed)
        /// </summary>
        public int GetSelectedTier() => selectedIndex + 1;

        private void OnDestroy()
        {
            glowTween?.Kill();
            content?.DOKill();
        }
    }
}
