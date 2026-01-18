using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace DigitPark.CashBattle
{
    /// <summary>
    /// Componente UI para mostrar un item del historial.
    /// Se usa como prefab en la lista de historial.
    /// </summary>
    public class HistoryEntryItemUI : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image resultIndicator;

        [Header("Main Info")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private TextMeshProUGUI dateText;

        [Header("Result")]
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Image resultBadge;

        [Header("Score")]
        [SerializeField] private TextMeshProUGUI scoreText;

        [Header("Money")]
        [SerializeField] private TextMeshProUGUI netResultText;

        [Header("Icon")]
        [SerializeField] private Image gameTypeIcon;
        [SerializeField] private Sprite match1v1Icon;
        [SerializeField] private Sprite cognitiveSprintIcon;
        [SerializeField] private Sprite tournamentIcon;

        [Header("Interaction")]
        [SerializeField] private Button itemButton;

        [Header("Colors")]
        [SerializeField] private Color winColor = new Color(0f, 1f, 0.5f, 1f);
        [SerializeField] private Color lossColor = new Color(1f, 0.4f, 0.4f, 1f);
        [SerializeField] private Color drawColor = new Color(1f, 0.84f, 0f, 1f);
        [SerializeField] private Color pendingColor = new Color(0f, 0.83f, 1f, 1f);
        [SerializeField] private Color cancelledColor = new Color(0.6f, 0.6f, 0.6f, 1f);

        private HistoryEntry _entry;
        private Action<HistoryEntry> _onClick;

        private void Awake()
        {
            if (itemButton)
            {
                itemButton.onClick.AddListener(OnItemClicked);
            }
        }

        /// <summary>
        /// Configura el item con los datos de la entrada
        /// </summary>
        public void Setup(HistoryEntry entry, Action<HistoryEntry> onClick = null)
        {
            _entry = entry;
            _onClick = onClick;

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (_entry == null) return;

            // Title and subtitle
            if (titleText)
                titleText.text = _entry.GetDisplayTitle();

            if (subtitleText)
                subtitleText.text = _entry.GetDisplaySubtitle();

            // Date
            if (dateText)
                dateText.text = _entry.GetFormattedDate();

            // Result
            if (resultText)
            {
                resultText.text = _entry.GetResultText();
                resultText.color = _entry.GetResultColor();
            }

            // Result indicator (colored bar/dot)
            if (resultIndicator)
            {
                resultIndicator.color = _entry.GetResultColor();
            }

            // Result badge
            if (resultBadge)
            {
                resultBadge.color = _entry.GetResultColor();
            }

            // Score
            if (scoreText)
            {
                if (_entry.type == HistoryEntryType.Tournament)
                {
                    if (_entry.result == MatchResult.Pending)
                    {
                        scoreText.text = "...";
                    }
                    else
                    {
                        scoreText.text = $"#{_entry.myPosition}";
                    }
                }
                else
                {
                    if (_entry.result == MatchResult.Pending)
                    {
                        scoreText.text = "En curso";
                    }
                    else
                    {
                        scoreText.text = $"{_entry.myScore:F0} - {_entry.opponentScore:F0}";
                    }
                }
            }

            // Net result (money)
            if (netResultText)
            {
                if (_entry.result == MatchResult.Pending)
                {
                    netResultText.text = $"-${_entry.entryFee:F2}";
                    netResultText.color = pendingColor;
                }
                else
                {
                    netResultText.text = _entry.GetFormattedNetResult();
                    netResultText.color = _entry.netResult >= 0 ? winColor : lossColor;
                }
            }

            // Game type icon
            UpdateGameTypeIcon();

            // Background tint based on result
            UpdateBackgroundTint();
        }

        private void UpdateGameTypeIcon()
        {
            if (gameTypeIcon == null) return;

            Sprite icon = null;

            switch (_entry.type)
            {
                case HistoryEntryType.Match1v1:
                    icon = match1v1Icon;
                    break;
                case HistoryEntryType.CognitiveSprint:
                    icon = cognitiveSprintIcon;
                    break;
                case HistoryEntryType.Tournament:
                case HistoryEntryType.TournamentPrize:
                    icon = tournamentIcon;
                    break;
            }

            if (icon != null)
            {
                gameTypeIcon.sprite = icon;
                gameTypeIcon.gameObject.SetActive(true);
            }
            else
            {
                gameTypeIcon.gameObject.SetActive(false);
            }
        }

        private void UpdateBackgroundTint()
        {
            if (backgroundImage == null) return;

            Color bgColor = Color.white;
            float alpha = 0.1f;

            switch (_entry.result)
            {
                case MatchResult.Win:
                    bgColor = winColor;
                    break;
                case MatchResult.Loss:
                    bgColor = lossColor;
                    break;
                case MatchResult.Draw:
                    bgColor = drawColor;
                    break;
                case MatchResult.Pending:
                    bgColor = pendingColor;
                    break;
                case MatchResult.Cancelled:
                case MatchResult.Disqualified:
                    bgColor = cancelledColor;
                    break;
            }

            bgColor.a = alpha;
            backgroundImage.color = bgColor;
        }

        private void OnItemClicked()
        {
            _onClick?.Invoke(_entry);
        }

        /// <summary>
        /// Actualiza el display si la entrada cambió
        /// </summary>
        public void Refresh()
        {
            UpdateDisplay();
        }

        /// <summary>
        /// Obtiene la entrada asociada
        /// </summary>
        public HistoryEntry GetEntry()
        {
            return _entry;
        }
    }
}
