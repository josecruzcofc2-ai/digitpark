using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Data;
using DigitPark.Localization;
using System;

namespace DigitPark.UI.Items
{
    /// <summary>
    /// Componente UI para item de torneo en la vista de BÚSQUEDA
    /// Item compacto tipo lista para decidir si unirse
    /// Resolución: Portrait 9:16 (1080x1920)
    /// </summary>
    public class TournamentSearchItemUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI participantsText;
        [SerializeField] private TextMeshProUGUI creatorText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI gameTypeText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Button itemButton;
        [SerializeField] private Image arrowIcon;

        [Header("Status Indicators")]
        [SerializeField] private GameObject fullIndicator;
        [SerializeField] private GameObject privateIndicator;

        [Header("Colors")]
        [SerializeField] private Color normalBgColor = new Color(0.15f, 0.15f, 0.2f, 0.95f);
        [SerializeField] private Color fullBgColor = new Color(0.3f, 0.15f, 0.15f, 0.95f);
        [SerializeField] private Color participantsColor = new Color(1f, 0.84f, 0f, 1f);
        [SerializeField] private Color timeColor = new Color(0f, 1f, 0.53f, 1f);
        [SerializeField] private Color timeUrgentColor = new Color(1f, 0.3f, 0.3f, 1f);

        // Datos internos
        private TournamentData tournamentData;
        private Action<TournamentData> onClickCallback;

        /// <summary>
        /// Configura el item con los datos del torneo
        /// </summary>
        public void Setup(TournamentData tournament, Action<TournamentData> onClick)
        {
            tournamentData = tournament;
            onClickCallback = onClick;

            bool isFull = tournament.IsFull();

            // Participantes
            if (participantsText != null)
            {
                participantsText.text = $"{tournament.currentParticipants}/{tournament.maxParticipants}";
                participantsText.color = isFull ? timeUrgentColor : participantsColor;
            }

            // Creador
            if (creatorText != null)
            {
                creatorText.text = tournament.creatorName;
                creatorText.color = Color.white;
            }

            // Tipo de juego (si está disponible)
            if (gameTypeText != null)
            {
                gameTypeText.text = !string.IsNullOrEmpty(tournament.category) ? tournament.category : "";
                gameTypeText.gameObject.SetActive(!string.IsNullOrEmpty(tournament.category));
            }

            // Tiempo restante
            UpdateTimeRemaining();

            // Fondo según estado
            if (backgroundImage != null)
            {
                backgroundImage.color = isFull ? fullBgColor : normalBgColor;
            }

            // Indicadores
            if (fullIndicator != null)
            {
                fullIndicator.SetActive(isFull);
            }

            if (privateIndicator != null)
            {
                privateIndicator.SetActive(tournament.region == TournamentRegion.Private);
            }

            // Flecha (ocultar si está lleno)
            if (arrowIcon != null)
            {
                arrowIcon.gameObject.SetActive(!isFull);
            }

            // Configurar botón
            if (itemButton != null)
            {
                itemButton.onClick.RemoveAllListeners();
                itemButton.onClick.AddListener(OnItemClicked);
                itemButton.interactable = !isFull;
            }
        }

        /// <summary>
        /// Actualiza el tiempo restante
        /// </summary>
        public void UpdateTimeRemaining()
        {
            if (tournamentData == null || timeText == null) return;

            TimeSpan timeRemaining = tournamentData.GetTimeRemaining();

            if (timeRemaining.TotalSeconds <= 0)
            {
                timeText.text = AutoLocalizer.Get("finished");
                timeText.color = timeUrgentColor;
            }
            else
            {
                timeText.text = FormatTimeRemaining(timeRemaining);
                // Color urgente si queda menos de 1 hora
                timeText.color = timeRemaining.TotalHours < 1 ? timeUrgentColor : timeColor;
            }
        }

        /// <summary>
        /// Obtiene los datos del torneo
        /// </summary>
        public TournamentData GetTournamentData()
        {
            return tournamentData;
        }

        private void OnItemClicked()
        {
            if (tournamentData != null && !tournamentData.IsFull())
            {
                onClickCallback?.Invoke(tournamentData);
            }
        }

        private string FormatTimeRemaining(TimeSpan time)
        {
            if (time.TotalDays >= 1)
                return $"{(int)time.TotalDays}d {time.Hours}h";

            if (time.TotalHours >= 1)
                return $"{(int)time.TotalHours}h {time.Minutes}m";

            return $"{(int)time.TotalMinutes}m {time.Seconds}s";
        }

        #region Editor Setup Helper

        public void AutoSetupReferences()
        {
            participantsText = transform.Find("ParticipantsText")?.GetComponent<TextMeshProUGUI>();
            creatorText = transform.Find("CreatorText")?.GetComponent<TextMeshProUGUI>();
            timeText = transform.Find("TimeText")?.GetComponent<TextMeshProUGUI>();
            gameTypeText = transform.Find("GameTypeText")?.GetComponent<TextMeshProUGUI>();
            backgroundImage = GetComponent<Image>();
            itemButton = GetComponent<Button>();
            arrowIcon = transform.Find("ArrowIcon")?.GetComponent<Image>();
            fullIndicator = transform.Find("FullIndicator")?.gameObject;
            privateIndicator = transform.Find("PrivateIndicator")?.gameObject;
        }

        #endregion
    }
}
