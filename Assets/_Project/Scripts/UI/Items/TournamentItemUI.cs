using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Data;
using DigitPark.Localization;
using System;

namespace DigitPark.UI.Items
{
    /// <summary>
    /// Componente UI para un item de torneo en la lista
    /// Se usa como prefab instanciado por TournamentManager
    /// </summary>
    public class TournamentItemUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI participantsText;
        [SerializeField] private TextMeshProUGUI creatorText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Button itemButton;

        [Header("Dividers")]
        [SerializeField] private Image divider1;
        [SerializeField] private Image divider2;
        [SerializeField] private Image horizontalDivider;

        [Header("Colors")]
        [SerializeField] private Color participatingColor = new Color(0f, 0.83f, 1f, 0.95f);
        [SerializeField] private Color normalColor = new Color(0.15f, 0.15f, 0.2f, 0.95f);
        [SerializeField] private Color participantsTextColor = new Color(1f, 0.84f, 0f, 1f);
        [SerializeField] private Color timeTextColor = new Color(0f, 1f, 0.53f, 1f);

        // Datos internos
        private TournamentData tournamentData;
        private Action<TournamentData> onClickCallback;

        /// <summary>
        /// Configura el item con los datos del torneo
        /// </summary>
        public void Setup(TournamentData tournament, bool isParticipating, Action<TournamentData> onClick)
        {
            tournamentData = tournament;
            onClickCallback = onClick;

            // Participantes
            if (participantsText != null)
            {
                participantsText.text = $"{tournament.currentParticipants}/{tournament.maxParticipants}";
                participantsText.color = participantsTextColor;
            }

            // Creador
            if (creatorText != null)
            {
                creatorText.text = tournament.creatorName;
                creatorText.color = Color.white;
            }

            // Tiempo restante
            UpdateTimeRemaining();

            // Fondo segun participacion
            if (backgroundImage != null)
            {
                backgroundImage.color = isParticipating ? participatingColor : normalColor;
            }

            // Configurar boton
            if (itemButton != null)
            {
                itemButton.onClick.RemoveAllListeners();
                itemButton.onClick.AddListener(OnItemClicked);
            }
        }

        /// <summary>
        /// Actualiza el tiempo restante (llamar desde Update del manager)
        /// </summary>
        public void UpdateTimeRemaining()
        {
            if (tournamentData == null || timeText == null) return;

            TimeSpan timeRemaining = tournamentData.GetTimeRemaining();

            if (timeRemaining.TotalSeconds <= 0)
            {
                timeText.text = AutoLocalizer.Get("finished");
                timeText.color = Color.red;
            }
            else
            {
                timeText.text = FormatTimeRemaining(timeRemaining);
                timeText.color = timeTextColor;
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
            onClickCallback?.Invoke(tournamentData);
        }

        private string FormatTimeRemaining(TimeSpan time)
        {
            if (time.TotalDays >= 1)
                return AutoLocalizer.Get("time_days_hours", (int)time.TotalDays, time.Hours);

            if (time.TotalHours >= 1)
                return AutoLocalizer.Get("time_hours_minutes", (int)time.TotalHours, time.Minutes);

            return AutoLocalizer.Get("time_minutes_seconds", (int)time.TotalMinutes, time.Seconds);
        }

        #region Editor Setup Helper

        /// <summary>
        /// Configura las referencias automaticamente (llamar desde editor script)
        /// </summary>
        public void AutoSetupReferences()
        {
            // Buscar textos por nombre
            participantsText = transform.Find("ParticipantsText")?.GetComponent<TextMeshProUGUI>();
            creatorText = transform.Find("CreatorText")?.GetComponent<TextMeshProUGUI>();
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
