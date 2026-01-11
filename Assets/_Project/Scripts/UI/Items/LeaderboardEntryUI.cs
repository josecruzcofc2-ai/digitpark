using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Data;
using DigitPark.Services.Firebase;
using System;

namespace DigitPark.UI.Items
{
    /// <summary>
    /// Componente UI para una entrada de leaderboard/scores
    /// Se usa como prefab instanciado por LeaderboardManager
    /// Resolucion: Portrait 9:16 (1080x1920)
    /// </summary>
    public class LeaderboardEntryUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI positionText;
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Button itemButton;

        [Header("Dividers")]
        [SerializeField] private Image divider1;
        [SerializeField] private Image divider2;
        [SerializeField] private Image horizontalDivider;

        [Header("Colors")]
        [SerializeField] private Color currentPlayerColor = new Color(0f, 0.83f, 1f, 0.95f);
        [SerializeField] private Color normalColor = new Color(0.15f, 0.15f, 0.2f, 0.95f);
        [SerializeField] private Color goldColor = new Color(1f, 0.84f, 0f, 1f);
        [SerializeField] private Color silverColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        [SerializeField] private Color bronzeColor = new Color(0.8f, 0.5f, 0.2f, 1f);
        [SerializeField] private Color timeColor = new Color(0f, 1f, 0.53f, 1f);

        // Datos internos
        private LeaderboardEntry entryData;
        private Action<LeaderboardEntry> onClickCallback;

        /// <summary>
        /// Configura el item con los datos de la entrada
        /// </summary>
        public void Setup(LeaderboardEntry entry, bool isCurrentPlayer, Action<LeaderboardEntry> onClick = null)
        {
            entryData = entry;
            onClickCallback = onClick;

            // Posicion con color de medalla
            if (positionText != null)
            {
                positionText.text = $"{entry.position}";
                positionText.color = GetPositionColor(entry.position);
                positionText.fontStyle = FontStyles.Bold;
            }

            // Username
            if (usernameText != null)
            {
                usernameText.text = entry.username;
                usernameText.color = Color.white;
            }

            // Tiempo
            if (timeText != null)
            {
                timeText.text = $"{entry.time:F3}s";
                timeText.color = timeColor;
            }

            // Fondo segun si es el jugador actual
            if (backgroundImage != null)
            {
                backgroundImage.color = isCurrentPlayer ? currentPlayerColor : normalColor;
            }

            // Configurar boton si hay callback
            if (itemButton != null && onClick != null)
            {
                itemButton.onClick.RemoveAllListeners();
                itemButton.onClick.AddListener(OnItemClicked);
            }
        }

        /// <summary>
        /// Obtiene los datos de la entrada
        /// </summary>
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
                default: return goldColor; // Amarillo para resto
            }
        }

        #region Editor Setup Helper

        /// <summary>
        /// Configura las referencias automaticamente (llamar desde editor script)
        /// </summary>
        public void AutoSetupReferences()
        {
            // Buscar textos por nombre
            positionText = transform.Find("PositionText")?.GetComponent<TextMeshProUGUI>();
            usernameText = transform.Find("UsernameText")?.GetComponent<TextMeshProUGUI>();
            timeText = transform.Find("TimeText")?.GetComponent<TextMeshProUGUI>();

            // Background y Button
            backgroundImage = GetComponent<Image>();
            itemButton = GetComponent<Button>();

            // Dividers
            divider1 = transform.Find("VerticalDivider1")?.GetComponent<Image>();
            divider2 = transform.Find("VerticalDivider2")?.GetComponent<Image>();
            horizontalDivider = transform.Find("HorizontalDivider")?.GetComponent<Image>();
        }

        #endregion
    }
}
