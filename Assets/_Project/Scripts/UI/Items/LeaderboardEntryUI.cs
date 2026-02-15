using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Data;
using DigitPark.Services.Firebase;
using System;
using DG.Tweening;

namespace DigitPark.UI.Items
{
    /// <summary>
    /// Componente UI para una entrada de leaderboard/scores
    /// Diseño: Posición + Avatar + Username + Tiempo
    /// Se usa como prefab instanciado por LeaderboardManager
    /// </summary>
    public class LeaderboardEntryUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI positionText;
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Button itemButton;

        [Header("Avatar")]
        [SerializeField] private Image avatarImage;

        [Header("Medal")]
        [SerializeField] private Image medalIndicator;

        [Header("Colors")]
        [SerializeField] private Color currentPlayerHighlight = new Color(0f, 0.83f, 1f, 0.25f);
        [SerializeField] private Color evenRowColor = new Color(0.04f, 0.08f, 0.13f, 0.95f);
        [SerializeField] private Color oddRowColor = new Color(0.06f, 0.1f, 0.16f, 0.95f);
        [SerializeField] private Color goldColor = new Color(1f, 0.84f, 0f, 1f);
        [SerializeField] private Color silverColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        [SerializeField] private Color bronzeColor = new Color(0.8f, 0.5f, 0.2f, 1f);
        [SerializeField] private Color normalPositionColor = new Color(0.5f, 0.55f, 0.65f, 1f);
        [SerializeField] private Color timeColor = new Color(0f, 1f, 0.53f, 1f);

        private LeaderboardEntry entryData;
        private Action<LeaderboardEntry> onClickCallback;

        /// <summary>
        /// Configura el item con los datos de la entrada
        /// </summary>
        public void Setup(LeaderboardEntry entry, bool isCurrentPlayer, Action<LeaderboardEntry> onClick = null)
        {
            entryData = entry;
            onClickCallback = onClick;

            // Posición: sin # para top 3, con # para #4+
            if (positionText != null)
            {
                positionText.text = entry.position <= 3 ? $"{entry.position}" : $"#{entry.position}";
                positionText.fontSize = entry.position <= 3 ? 28 : 22;
                positionText.color = GetPositionColor(entry.position);
                positionText.fontStyle = FontStyles.Bold;
            }

            // Avatar placeholder (futuro: cargar desde avatarUrl)
            if (avatarImage != null)
            {
                avatarImage.color = new Color(0.2f, 0.25f, 0.35f);
                // TODO: Si entry.avatarUrl no es vacío, cargar imagen async
            }

            // Medal indicator para top 3
            if (medalIndicator != null)
            {
                if (entry.position <= 3)
                {
                    medalIndicator.gameObject.SetActive(true);
                    medalIndicator.transform.localScale = Vector3.zero;
                    medalIndicator.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
                    Color medalColor = GetPositionColor(entry.position);
                    medalIndicator.color = new Color(medalColor.r, medalColor.g, medalColor.b, 0.4f);
                }
                else
                {
                    medalIndicator.gameObject.SetActive(false);
                }
            }

            // Username
            if (usernameText != null)
            {
                usernameText.text = entry.username;
                usernameText.color = isCurrentPlayer ? new Color(0f, 1f, 1f) : Color.white;
                usernameText.fontStyle = FontStyles.Bold;
            }

            // Tiempo
            if (timeText != null)
            {
                timeText.text = $"{entry.time:F3}s";
                timeText.color = timeColor;
                timeText.fontStyle = FontStyles.Bold;
            }

            // Fondo: cyan highlight para jugador actual, alternante para el resto
            if (backgroundImage != null)
            {
                if (isCurrentPlayer)
                {
                    backgroundImage.color = currentPlayerHighlight;
                }
                else
                {
                    backgroundImage.color = entry.position % 2 == 0 ? evenRowColor : oddRowColor;
                }
            }

            // Configurar botón si hay callback
            if (itemButton != null && onClick != null)
            {
                itemButton.onClick.RemoveAllListeners();
                itemButton.onClick.AddListener(OnItemClicked);
            }
        }

        public LeaderboardEntry GetEntryData()
        {
            return entryData;
        }

        private void OnItemClicked()
        {
            onClickCallback?.Invoke(entryData);
        }

        private Color GetPositionColor(int position)
        {
            switch (position)
            {
                case 1: return goldColor;
                case 2: return silverColor;
                case 3: return bronzeColor;
                default: return normalPositionColor;
            }
        }

        #region Editor Setup Helper

        /// <summary>
        /// Configura las referencias automáticamente buscando por nombre de hijo
        /// </summary>
        public void AutoSetupReferences()
        {
            positionText = transform.Find("PositionText")?.GetComponent<TextMeshProUGUI>();
            usernameText = transform.Find("UsernameText")?.GetComponent<TextMeshProUGUI>();
            timeText = transform.Find("TimeText")?.GetComponent<TextMeshProUGUI>();
            avatarImage = transform.Find("AvatarImage")?.GetComponent<Image>();
            medalIndicator = transform.Find("MedalIndicator")?.GetComponent<Image>();
            backgroundImage = GetComponent<Image>();
            itemButton = GetComponent<Button>();
        }

        #endregion
    }
}
