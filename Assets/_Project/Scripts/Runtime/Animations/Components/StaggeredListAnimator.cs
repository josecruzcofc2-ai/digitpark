using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

namespace DigitPark.Animations
{
    /// <summary>
    /// Animates list/grid items with a cascading entrance effect.
    /// Attach to any ScrollView content container or parent of list items.
    /// Call AnimateItems() after populating the list.
    /// </summary>
    public class StaggeredListAnimator : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float delayPerItem = 0.05f;
        [SerializeField] private float itemDuration = 0.3f;
        [SerializeField] private float slideOffset = 30f;
        [SerializeField] private Ease itemEase = Ease.OutQuad;

        [Header("Options")]
        [SerializeField] private bool animateOnEnable = false;
        [SerializeField] private bool includeScale = true;
        [SerializeField] private float startScale = 0.85f;

        private Sequence currentSequence;

        private void OnEnable()
        {
            if (animateOnEnable)
                AnimateItems();
        }

        private void OnDisable()
        {
            currentSequence?.Kill();
        }

        private void OnDestroy()
        {
            currentSequence?.Kill();
        }

        /// <summary>
        /// Animate all active child items with staggered entrance
        /// </summary>
        public void AnimateItems()
        {
            var items = GetActiveChildren();
            if (items.Count == 0) return;

            AnimateItems(items);
        }

        /// <summary>
        /// Animate specific transforms with staggered entrance
        /// </summary>
        public void AnimateItems(List<Transform> items)
        {
            if (items == null || items.Count == 0) return;

            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null) continue;

                var rt = item.GetComponent<RectTransform>();
                var cg = item.GetComponent<CanvasGroup>();
                if (cg == null) cg = item.gameObject.AddComponent<CanvasGroup>();

                // Store original position
                Vector2 originalPos = rt != null ? rt.anchoredPosition : Vector2.zero;

                // Prepare
                cg.alpha = 0f;
                if (rt != null)
                    rt.anchoredPosition = originalPos + new Vector2(0, -slideOffset);
                if (includeScale)
                    item.localScale = Vector3.one * startScale;

                float delay = i * delayPerItem;

                // Fade in
                currentSequence.Insert(delay, cg.DOFade(1f, itemDuration).SetEase(Ease.OutQuad));

                // Slide up
                if (rt != null)
                    currentSequence.Insert(delay, rt.DOAnchorPos(originalPos, itemDuration).SetEase(itemEase));

                // Scale
                if (includeScale)
                    currentSequence.Insert(delay, item.DOScale(1f, itemDuration).SetEase(itemEase));
            }
        }

        /// <summary>
        /// Animate items out (reverse stagger) then invoke callback
        /// </summary>
        public void AnimateOut(System.Action onComplete = null)
        {
            var items = GetActiveChildren();
            if (items.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null) continue;

                var cg = item.GetComponent<CanvasGroup>();
                if (cg == null) cg = item.gameObject.AddComponent<CanvasGroup>();

                float delay = i * (delayPerItem * 0.5f); // Faster exit
                currentSequence.Insert(delay, cg.DOFade(0f, itemDuration * 0.6f).SetEase(Ease.InQuad));

                if (includeScale)
                    currentSequence.Insert(delay, item.DOScale(startScale, itemDuration * 0.6f).SetEase(Ease.InQuad));
            }

            currentSequence.OnComplete(() => onComplete?.Invoke());
        }

        /// <summary>
        /// Reset all items to visible state (no animation)
        /// </summary>
        public void ResetItems()
        {
            currentSequence?.Kill();
            foreach (Transform child in transform)
            {
                if (!child.gameObject.activeSelf) continue;
                child.localScale = Vector3.one;
                var cg = child.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 1f;
            }
        }

        private List<Transform> GetActiveChildren()
        {
            var items = new List<Transform>();
            foreach (Transform child in transform)
            {
                if (child.gameObject.activeSelf)
                    items.Add(child);
            }
            return items;
        }
    }
}
