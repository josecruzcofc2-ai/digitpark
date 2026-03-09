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
    /// Handles game selector card animations including
    /// carousel effects, card selection, and hover states.
    /// </summary>
    public class GameSelectorAnimator : MonoBehaviour
    {
        [Header("Card Container")]
        [SerializeField] private RectTransform cardsContainer;
        [SerializeField] private List<RectTransform> gameCards = new List<RectTransform>();

        [Header("Selection Indicator")]
        [SerializeField] private RectTransform selectionFrame;
        [SerializeField] private Image selectionGlow;
        [SerializeField] private ParticleSystem selectionParticles;

        [Header("Card Settings")]
        [SerializeField] private float selectedScale = 1.15f;
        [SerializeField] private float unselectedScale = 0.85f;
        [SerializeField] private float selectedAlpha = 1f;
        [SerializeField] private float unselectedAlpha = 1f;
        [SerializeField] private float cardSpacing = 250f;

        [Header("Animation Settings")]
        [SerializeField] private float scrollDuration = 0.3f;
        [SerializeField] private float selectDuration = 0.25f;
        [SerializeField] private float entranceStagger = 0.1f;

        [Header("Parallax")]
        [SerializeField] private RectTransform[] parallaxLayers;
        [SerializeField] private float[] parallaxFactors;

        [Header("Audio")]
        [SerializeField] private AudioClip scrollSound;
        [SerializeField] private AudioClip selectSound;
        [SerializeField] private AudioClip confirmSound;

        // Events
        public event Action<int> OnCardSelected;
        public event Action<int> OnCardConfirmed;

        private AudioSource audioSource;
        private int currentIndex = 0;
        private bool isAnimating;
        private List<CanvasGroup> cardCanvasGroups = new List<CanvasGroup>();

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            // Cache canvas groups
            foreach (var card in gameCards)
            {
                if (card != null)
                {
                    CanvasGroup cg = card.GetComponent<CanvasGroup>();
                    if (cg == null)
                        cg = card.gameObject.AddComponent<CanvasGroup>();
                    cardCanvasGroups.Add(cg);
                }
            }
        }

        // ==================== ENTRANCE ====================

        /// <summary>
        /// Play cards entrance animation
        /// </summary>
        public void PlayEntranceAnimation(Action onComplete = null)
        {
            StartCoroutine(EntranceCoroutine(onComplete));
        }

        private IEnumerator EntranceCoroutine(Action onComplete)
        {
            // Hide all cards initially
            foreach (var card in gameCards)
            {
                if (card != null)
                {
                    card.localScale = Vector3.zero;
                    card.anchoredPosition = new Vector2(card.anchoredPosition.x, -200f);
                }
            }

            yield return new WaitForSeconds(0.2f);

            // Staggered entrance
            for (int i = 0; i < gameCards.Count; i++)
            {
                if (gameCards[i] == null) continue;

                Sequence cardSeq = DOTween.Sequence();

                // Scale up
                cardSeq.Append(gameCards[i].DOScale(i == currentIndex ? selectedScale : unselectedScale, 0.3f)
                    .SetEase(Ease.OutBack));

                // Slide up
                cardSeq.Join(gameCards[i].DOAnchorPosY(0, 0.3f).SetEase(Ease.OutQuad));

                // Set alpha
                if (i < cardCanvasGroups.Count)
                {
                    cardCanvasGroups[i].alpha = 0f;
                    cardSeq.Join(cardCanvasGroups[i].DOFade(i == currentIndex ? selectedAlpha : unselectedAlpha, 0.3f));
                }

                yield return new WaitForSeconds(entranceStagger);
            }

            yield return new WaitForSeconds(0.3f);

            // Show selection frame
            if (selectionFrame != null)
            {
                selectionFrame.gameObject.SetActive(true);
                selectionFrame.localScale = Vector3.zero;
                selectionFrame.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
            }

            // Start selection glow
            StartSelectionGlow();

            onComplete?.Invoke();
        }

        // ==================== NAVIGATION ====================

        /// <summary>
        /// Navigate to next card
        /// </summary>
        public void NavigateNext()
        {
            if (isAnimating || currentIndex >= gameCards.Count - 1) return;
            NavigateToIndex(currentIndex + 1);
        }

        /// <summary>
        /// Navigate to previous card
        /// </summary>
        public void NavigatePrevious()
        {
            if (isAnimating || currentIndex <= 0) return;
            NavigateToIndex(currentIndex - 1);
        }

        /// <summary>
        /// Navigate to specific card index
        /// </summary>
        public void NavigateToIndex(int index)
        {
            if (isAnimating || index < 0 || index >= gameCards.Count || index == currentIndex) return;

            StartCoroutine(NavigateCoroutine(index));
        }

        private IEnumerator NavigateCoroutine(int targetIndex)
        {
            isAnimating = true;

            // Play scroll sound
            if (scrollSound != null && audioSource != null)
                audioSource.PlayOneShot(scrollSound);

            int previousIndex = currentIndex;
            currentIndex = targetIndex;

            // Animate all cards
            Sequence scrollSeq = DOTween.Sequence();

            for (int i = 0; i < gameCards.Count; i++)
            {
                if (gameCards[i] == null) continue;

                bool isSelected = (i == currentIndex);
                float targetScale = isSelected ? selectedScale : unselectedScale;
                float targetAlpha = isSelected ? selectedAlpha : unselectedAlpha;

                // Scale
                scrollSeq.Join(gameCards[i].DOScale(targetScale, scrollDuration).SetEase(Ease.OutQuad));

                // Alpha
                if (i < cardCanvasGroups.Count)
                    scrollSeq.Join(cardCanvasGroups[i].DOFade(targetAlpha, scrollDuration));

                // Depth effect (Y position based on selection)
                float yOffset = isSelected ? 20f : 0f;
                scrollSeq.Join(gameCards[i].DOAnchorPosY(yOffset, scrollDuration).SetEase(Ease.OutQuad));
            }

            // Move container to center selected card
            float targetX = -currentIndex * cardSpacing;
            scrollSeq.Join(cardsContainer.DOAnchorPosX(targetX, scrollDuration).SetEase(Ease.OutQuad));

            // Move selection frame
            if (selectionFrame != null && currentIndex < gameCards.Count)
            {
                scrollSeq.Join(selectionFrame.DOMove(gameCards[currentIndex].position, scrollDuration).SetEase(Ease.OutQuad));
            }

            // Parallax
            UpdateParallax(targetIndex);

            yield return scrollSeq.WaitForCompletion();

            // Pulse selected card
            if (gameCards[currentIndex] != null)
            {
                gameCards[currentIndex].DOPunchScale(Vector3.one * 0.05f, 0.2f, 3);
            }

            isAnimating = false;
            OnCardSelected?.Invoke(currentIndex);
        }

        private void UpdateParallax(int index)
        {
            if (parallaxLayers == null || parallaxFactors == null) return;

            float baseOffset = -index * cardSpacing;

            for (int i = 0; i < parallaxLayers.Length && i < parallaxFactors.Length; i++)
            {
                if (parallaxLayers[i] == null) continue;

                float parallaxX = baseOffset * parallaxFactors[i];
                parallaxLayers[i].DOAnchorPosX(parallaxX, scrollDuration).SetEase(Ease.OutQuad);
            }
        }

        // ==================== SELECTION ====================

        /// <summary>
        /// Confirm current card selection
        /// </summary>
        public void ConfirmSelection(Action onComplete = null)
        {
            if (isAnimating) return;
            StartCoroutine(ConfirmCoroutine(onComplete));
        }

        private IEnumerator ConfirmCoroutine(Action onComplete)
        {
            isAnimating = true;

            // Play confirm sound
            if (confirmSound != null && audioSource != null)
                audioSource.PlayOneShot(confirmSound);

            // Selection particles
            if (selectionParticles != null)
                selectionParticles.Play();

            // Flash effect
            if (UIAnimationManager.Instance != null)
                UIAnimationManager.Instance.WhiteFlash(0.2f);

            // Zoom selected card
            if (currentIndex < gameCards.Count && gameCards[currentIndex] != null)
            {
                Sequence confirmSeq = DOTween.Sequence();
                confirmSeq.Append(gameCards[currentIndex].DOScale(selectedScale * 1.3f, selectDuration).SetEase(Ease.OutQuad));
                confirmSeq.Join(gameCards[currentIndex].DOPunchRotation(new Vector3(0, 0, 5f), selectDuration * 2, 10));

                // Fade out other cards
                for (int i = 0; i < gameCards.Count; i++)
                {
                    if (i != currentIndex && i < cardCanvasGroups.Count)
                    {
                        confirmSeq.Join(cardCanvasGroups[i].DOFade(0f, selectDuration));
                    }
                }

                yield return confirmSeq.WaitForCompletion();
            }

            isAnimating = false;
            OnCardConfirmed?.Invoke(currentIndex);
            onComplete?.Invoke();
        }

        // ==================== HOVER EFFECTS ====================

        /// <summary>
        /// Play hover effect on card
        /// </summary>
        public void PlayCardHover(int index)
        {
            if (index < 0 || index >= gameCards.Count || gameCards[index] == null) return;

            gameCards[index].DOScale(gameCards[index].localScale.x * 1.05f, 0.15f).SetEase(Ease.OutQuad);

            Image cardImage = gameCards[index].GetComponent<Image>();
            if (cardImage != null)
            {
                Color original = cardImage.color;
                cardImage.DOColor(Color.white, 0.15f);
            }
        }

        /// <summary>
        /// Remove hover effect from card
        /// </summary>
        public void RemoveCardHover(int index)
        {
            if (index < 0 || index >= gameCards.Count || gameCards[index] == null) return;

            float targetScale = (index == currentIndex) ? selectedScale : unselectedScale;
            gameCards[index].DOScale(targetScale, 0.15f).SetEase(Ease.OutQuad);
        }

        // ==================== CONTINUOUS EFFECTS ====================

        private void StartSelectionGlow()
        {
            if (selectionGlow == null) return;

            selectionGlow.DOFade(0.8f, 0.8f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        /// <summary>
        /// Play attention animation on specific card
        /// </summary>
        public void PlayCardAttention(int index)
        {
            if (index < 0 || index >= gameCards.Count || gameCards[index] == null) return;

            DOTween.Sequence()
                .Append(gameCards[index].DOScale(gameCards[index].localScale.x * 1.1f, 0.2f).SetEase(Ease.OutQuad))
                .Append(gameCards[index].DOScale(gameCards[index].localScale.x, 0.3f).SetEase(Ease.OutBounce))
                .SetLoops(2);
        }

        /// <summary>
        /// Play "new" badge animation
        /// </summary>
        public void PlayNewBadgeAnimation(RectTransform badge)
        {
            if (badge == null) return;

            badge.localScale = Vector3.zero;
            badge.gameObject.SetActive(true);

            DOTween.Sequence()
                .Append(badge.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack))
                .Append(badge.DOScale(1f, 0.1f))
                .Append(badge.DOScale(1.05f, 0.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo));
        }

        // ==================== UTILITY ====================

        /// <summary>
        /// Register cards at runtime
        /// </summary>
        public void RegisterCards(List<RectTransform> cards)
        {
            gameCards = cards;
            cardCanvasGroups.Clear();

            foreach (var card in gameCards)
            {
                if (card != null)
                {
                    CanvasGroup cg = card.GetComponent<CanvasGroup>();
                    if (cg == null)
                        cg = card.gameObject.AddComponent<CanvasGroup>();
                    cardCanvasGroups.Add(cg);
                }
            }
        }

        /// <summary>
        /// Get current selected index
        /// </summary>
        public int GetCurrentIndex() => currentIndex;

        /// <summary>
        /// Reset to initial state
        /// </summary>
        public void Reset()
        {
            currentIndex = 0;
            isAnimating = false;

            if (cardsContainer != null)
                cardsContainer.anchoredPosition = Vector2.zero;

            for (int i = 0; i < gameCards.Count; i++)
            {
                if (gameCards[i] == null) continue;

                gameCards[i].localScale = Vector3.one * (i == 0 ? selectedScale : unselectedScale);

                if (i < cardCanvasGroups.Count)
                    cardCanvasGroups[i].alpha = i == 0 ? selectedAlpha : unselectedAlpha;
            }
        }

        private void OnDestroy()
        {
            DOTween.Kill(cardsContainer);
            DOTween.Kill(selectionFrame);
            DOTween.Kill(selectionGlow);

            foreach (var card in gameCards)
            {
                if (card != null)
                    DOTween.Kill(card);
            }
        }
    }
}
