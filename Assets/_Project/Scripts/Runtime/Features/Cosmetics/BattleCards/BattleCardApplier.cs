using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace DigitPark.Cosmetics
{
    /// <summary>
    /// Componente que aplica un BattleCard a un player/opponent card en matchmaking.
    /// Se añade via MatchmakingUIBuilder a cada card GameObject.
    /// Solo afecta el card visual — NO toca temas ni frames.
    /// </summary>
    public class BattleCardApplier : MonoBehaviour
    {
        [Header("Card Elements")]
        [SerializeField] private Image cardBackground;
        [SerializeField] private Outline outlineBorder;
        [SerializeField] private Image avatarGlow;
        [SerializeField] private Image avatarFrame;
        [SerializeField] private Image levelPillBg;
        [SerializeField] private TextMeshProUGUI playerNameText;

        private BattleCardData _currentCard;
        private string _tweenId;

        private void Awake()
        {
            _tweenId = $"battlecard_anim_{GetInstanceID()}";
        }

        /// <summary>
        /// Apply a BattleCard's visual parameters to this card.
        /// Call after FrameRenderer has applied the frame sprite.
        /// </summary>
        public void ApplyCard(BattleCardData card)
        {
            if (card == null) return;
            _currentCard = card;

            StopAnimation();

            // Card background
            if (cardBackground != null)
                cardBackground.color = card.cardBackgroundColor;

            // Border (Outline component on CardBackground)
            if (outlineBorder != null)
            {
                Color borderColor = card.accentPrimary;
                if (card.hasBorderGlow)
                    borderColor.a = card.borderGlowIntensity;
                else
                    borderColor.a = 0.7f;

                outlineBorder.effectColor = borderColor;
                outlineBorder.effectDistance = new Vector2(card.borderWidth, -card.borderWidth);
            }

            // Avatar glow
            if (avatarGlow != null)
            {
                Color glowCol = card.accentPrimary;
                glowCol.a = 0.4f;
                avatarGlow.color = glowCol;
            }

            // Avatar frame COLOR only (sprite is controlled by FrameRenderer)
            if (avatarFrame != null)
                avatarFrame.color = card.avatarFrameColor;

            // Level pill
            if (levelPillBg != null)
                levelPillBg.color = card.accentPrimary;

            // Name text
            if (playerNameText != null)
            {
                playerNameText.color = card.nameTextColor;
                if (card.nameHasGlow && card.nameGlowColor.a > 0.01f)
                {
                    playerNameText.outlineColor = card.nameGlowColor;
                    playerNameText.outlineWidth = card.nameGlowIntensity * 0.3f;
                }
                else
                {
                    playerNameText.outlineWidth = 0;
                }
            }

            // Start animations if needed
            if (card.isAnimatedBorder || card.isAnimatedAvatarFrame)
                StartAnimation();
        }

        /// <summary>
        /// Apply the default card (Neon Core).
        /// </summary>
        public void ApplyDefault()
        {
            var service = BattleCardService.Instance;
            if (service != null)
                ApplyCard(service.GetDefault() ?? service.GetEquippedCard());
        }

        /// <summary>
        /// Start border/avatar frame animations for Elite/Legend cards.
        /// All tweens use per-instance SetId for pause/play.
        /// </summary>
        public void StartAnimation()
        {
            if (_currentCard == null) return;

            // Border animation
            if (_currentCard.isAnimatedBorder && outlineBorder != null)
            {
                switch (_currentCard.borderAnimation)
                {
                    case BorderAnimationType.Pulse:
                        PlayPulse();
                        break;
                    case BorderAnimationType.Electric:
                        PlayElectric();
                        break;
                    case BorderAnimationType.DataStream:
                        PlayDataStream();
                        break;
                    case BorderAnimationType.Flame:
                        PlayFlame();
                        break;
                }
            }

            // Avatar frame animation
            if (_currentCard.isAnimatedAvatarFrame && avatarFrame != null)
            {
                avatarFrame.transform.DOScale(1.08f, 2f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
                    .SetId(_tweenId)
                    .SetLink(gameObject);
            }
        }

        public void StopAnimation()
        {
            DOTween.Kill(_tweenId);
        }

        public void PauseAnimation()
        {
            DOTween.Pause(_tweenId);
        }

        public void ResumeAnimation()
        {
            DOTween.Play(_tweenId);
        }

        private void OnDestroy()
        {
            DOTween.Kill(_tweenId);
        }

        #region Animation Types

        /// <summary>Helper: animate Outline.effectColor via DOTween.To</summary>
        private Tweener TweenOutlineColor(Color target, float duration)
        {
            return DOTween.To(
                () => outlineBorder.effectColor,
                c => outlineBorder.effectColor = c,
                target, duration);
        }

        private void PlayPulse()
        {
            Color from = _currentCard.accentPrimary;
            from.a = 0.4f;
            Color to = _currentCard.accentPrimary;
            to.a = 1f;

            outlineBorder.effectColor = from;
            TweenOutlineColor(to, 1.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetId(_tweenId)
                .SetLink(gameObject);
        }

        private void PlayElectric()
        {
            Color primary = _currentCard.accentPrimary;
            Color secondary = _currentCard.accentSecondary.a > 0.01f
                ? _currentCard.accentSecondary
                : Color.Lerp(primary, Color.yellow, 0.4f);

            TweenOutlineColor(secondary, 0.8f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutQuad)
                .SetId(_tweenId)
                .SetLink(gameObject);
        }

        private void PlayDataStream()
        {
            Color[] spectrum = new Color[]
            {
                _currentCard.accentPrimary,
                Color.Lerp(_currentCard.accentPrimary, _currentCard.accentSecondary, 0.33f),
                _currentCard.accentSecondary.a > 0.01f ? _currentCard.accentSecondary : Color.magenta,
                Color.Lerp(_currentCard.accentSecondary, _currentCard.accentPrimary, 0.33f),
            };

            var seq = DOTween.Sequence()
                .SetId(_tweenId)
                .SetLink(gameObject)
                .SetLoops(-1, LoopType.Restart);

            foreach (var col in spectrum)
                seq.Append(TweenOutlineColor(col, 1f).SetEase(Ease.Linear));
        }

        private void PlayFlame()
        {
            Color primary = _currentCard.accentPrimary;
            Color hot = _currentCard.accentSecondary.a > 0.01f
                ? _currentCard.accentSecondary
                : Color.Lerp(primary, Color.white, 0.3f);

            DOTween.Sequence()
                .SetId(_tweenId)
                .SetLink(gameObject)
                .SetLoops(-1, LoopType.Restart)
                .Append(TweenOutlineColor(hot, 0.15f).SetEase(Ease.InQuad))
                .Append(TweenOutlineColor(primary, 0.25f).SetEase(Ease.OutQuad))
                .Append(TweenOutlineColor(hot, 0.1f).SetEase(Ease.InQuad))
                .Append(TweenOutlineColor(primary, 0.3f).SetEase(Ease.OutQuad))
                .AppendInterval(0.2f);
        }

        #endregion
    }
}
