using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
using System.Collections.Generic;

namespace DigitPark.Animations
{
    /// <summary>
    /// Animated score counter and result reveal for post-game panels.
    /// Handles: score count-up, sequential stat reveals, win/loss effects.
    /// </summary>
    public class ScoreRevealAnimator : MonoBehaviour
    {
        [Header("Timing")]
        [SerializeField] private float countUpDuration = 1.2f;
        [SerializeField] private float statRevealDelay = 0.25f;
        [SerializeField] private float initialDelay = 0.3f;

        [Header("Score Animation")]
        [SerializeField] private Ease countUpEase = Ease.OutQuad;
        [SerializeField] private float scorePunchScale = 1.2f;

        [Header("Colors")]
        [SerializeField] private Color winColor = new Color(0.3f, 1f, 0.5f);
        [SerializeField] private Color loseColor = new Color(1f, 0.3f, 0.3f);
        [SerializeField] private Color drawColor = new Color(1f, 0.85f, 0.3f);

        private Sequence revealSequence;

        private void OnDestroy()
        {
            revealSequence?.Kill();
        }

        /// <summary>
        /// Animate a score counter from 0 to target value
        /// </summary>
        public Tween AnimateScore(TextMeshProUGUI scoreText, int targetScore, string format = "{0}")
        {
            if (scoreText == null) return null;
            int current = 0;
            return DOTween.To(() => current, x =>
            {
                current = x;
                if (scoreText != null) scoreText.text = string.Format(format, x);
            }, targetScore, countUpDuration)
            .SetEase(countUpEase)
            .SetLink(scoreText.gameObject)
            .OnComplete(() =>
            {
                // Punch on complete
                if (scoreText != null)
                    scoreText.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 6, 0.5f).SetLink(scoreText.gameObject);
            });
        }

        /// <summary>
        /// Reveal a full result panel with sequential animations
        /// </summary>
        /// <param name="elements">UI elements to reveal in order</param>
        /// <param name="scoreText">Main score text for count-up</param>
        /// <param name="targetScore">Final score value</param>
        /// <param name="isWin">Whether the player won</param>
        /// <param name="onComplete">Called when all reveals are done</param>
        public void RevealResults(List<GameObject> elements, TextMeshProUGUI scoreText = null,
            int targetScore = 0, ResultType resultType = ResultType.Win, Action onComplete = null)
        {
            revealSequence?.Kill();
            revealSequence = DOTween.Sequence();

            // Prepare all elements
            foreach (var element in elements)
            {
                if (element == null) continue;
                var cg = GetOrAddCanvasGroup(element);
                cg.alpha = 0f;
                element.transform.localScale = Vector3.one * 0.8f;
            }

            // Sequentially reveal each element
            float currentDelay = initialDelay;

            foreach (var element in elements)
            {
                if (element == null) continue;

                var cg = GetOrAddCanvasGroup(element);

                revealSequence.Insert(currentDelay,
                    cg.DOFade(1f, 0.25f).SetEase(Ease.OutQuad));
                revealSequence.Insert(currentDelay,
                    element.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));

                currentDelay += statRevealDelay;
            }

            // Score counter animation
            if (scoreText != null && targetScore > 0)
            {
                revealSequence.Insert(currentDelay, AnimateScore(scoreText, targetScore));
                currentDelay += countUpDuration;
            }

            // Result-specific effects
            revealSequence.InsertCallback(currentDelay, () =>
            {
                switch (resultType)
                {
                    case ResultType.Win:
                        if (scoreText != null)
                        {
                            scoreText.color = winColor;
                            scoreText.transform.DOPunchScale(Vector3.one * (scorePunchScale - 1f), 0.4f, 8, 0.5f).SetLink(scoreText.gameObject);
                        }
                        break;
                    case ResultType.Loss:
                        if (scoreText != null)
                        {
                            scoreText.color = loseColor;
                            scoreText.transform.DOShakePosition(0.3f, 5f, 15, 90, false, true).SetLink(scoreText.gameObject);
                        }
                        break;
                    case ResultType.Draw:
                        if (scoreText != null)
                            scoreText.color = drawColor;
                        break;
                }
            });

            revealSequence.OnComplete(() => onComplete?.Invoke());
        }

        /// <summary>
        /// Animate a single stat row (label + value) with entrance
        /// </summary>
        public Tween RevealStat(GameObject statRow, float delay = 0f)
        {
            if (statRow == null) return null;

            var cg = GetOrAddCanvasGroup(statRow);
            cg.alpha = 0f;
            statRow.transform.localScale = Vector3.one * 0.9f;

            return DOTween.Sequence()
                .AppendInterval(delay)
                .Append(cg.DOFade(1f, 0.25f).SetEase(Ease.OutQuad))
                .Join(statRow.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack))
                .SetLink(statRow);
        }

        /// <summary>
        /// Animate a VS section with dramatic entrance
        /// </summary>
        public Sequence AnimateVSSection(Transform playerCard, Transform vsText, Transform opponentCard)
        {
            var seq = DOTween.Sequence();

            // Player card slides in from left
            if (playerCard != null)
            {
                var rt = playerCard.GetComponent<RectTransform>();
                Vector2 original = rt != null ? rt.anchoredPosition : Vector2.zero;
                if (rt != null) rt.anchoredPosition = original + new Vector2(-200f, 0);
                playerCard.localScale = Vector3.one * 0.8f;

                seq.Append(rt.DOAnchorPos(original, 0.4f).SetEase(Ease.OutBack));
                seq.Join(playerCard.DOScale(1f, 0.4f).SetEase(Ease.OutBack));
            }

            // VS pops in
            if (vsText != null)
            {
                vsText.localScale = Vector3.zero;
                seq.Append(vsText.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
                seq.Join(vsText.DOPunchRotation(new Vector3(0, 0, 10f), 0.3f, 8, 0.5f));
            }

            // Opponent card slides in from right
            if (opponentCard != null)
            {
                var rt = opponentCard.GetComponent<RectTransform>();
                Vector2 original = rt != null ? rt.anchoredPosition : Vector2.zero;
                if (rt != null) rt.anchoredPosition = original + new Vector2(200f, 0);
                opponentCard.localScale = Vector3.one * 0.8f;

                seq.Append(rt.DOAnchorPos(original, 0.4f).SetEase(Ease.OutBack));
                seq.Join(opponentCard.DOScale(1f, 0.4f).SetEase(Ease.OutBack));
            }

            return seq;
        }

        private CanvasGroup GetOrAddCanvasGroup(GameObject go)
        {
            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null) cg = go.AddComponent<CanvasGroup>();
            return cg;
        }
    }

    public enum ResultType
    {
        Win,
        Loss,
        Draw
    }
}
