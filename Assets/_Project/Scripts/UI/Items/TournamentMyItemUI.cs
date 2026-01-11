using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Data;
using DigitPark.Localization;
using DigitPark.Services.Firebase;
using System;
using System.Collections.Generic;

namespace DigitPark.UI.Items
{
    /// <summary>
    /// Componente UI para item de torneo en "MIS TORNEOS"
    /// Card detallada con información del progreso del jugador
    /// Expandible para mostrar leaderboard completo
    /// Resolución: Portrait 9:16 (1080x1920)
    /// </summary>
    public class TournamentMyItemUI : MonoBehaviour
    {
        [Header("Header Section")]
        [SerializeField] private TextMeshProUGUI tournamentNameText;
        [SerializeField] private TextMeshProUGUI timeRemainingText;
        [SerializeField] private Image statusIndicator;

        [Header("Player Stats Section")]
        [SerializeField] private TextMeshProUGUI myPositionText;
        [SerializeField] private TextMeshProUGUI myBestTimeText;
        [SerializeField] private TextMeshProUGUI myAttemptsText;

        [Header("Creator Section")]
        [SerializeField] private TextMeshProUGUI creatorNameText;
        [SerializeField] private TextMeshProUGUI creatorTimeText;

        [Header("Tournament Info")]
        [SerializeField] private TextMeshProUGUI participantsText;
        [SerializeField] private TextMeshProUGUI maxAttemptsText;

        [Header("Expandable Section")]
        [SerializeField] private GameObject expandedSection;
        [SerializeField] private Transform leaderboardContainer;
        [SerializeField] private GameObject leaderboardEntryPrefab;
        [SerializeField] private Button expandButton;
        [SerializeField] private TextMeshProUGUI expandButtonText;
        [SerializeField] private Image expandArrow;

        [Header("Action Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button viewLeaderboardButton;
        [SerializeField] private Button exitTournamentButton;

        [Header("Background")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image headerBackground;

        [Header("Colors")]
        [SerializeField] private Color normalBgColor = new Color(0.08f, 0.12f, 0.18f, 0.98f);
        [SerializeField] private Color headerColor = new Color(0.05f, 0.15f, 0.2f, 1f);
        [SerializeField] private Color goldColor = new Color(1f, 0.84f, 0f, 1f);
        [SerializeField] private Color silverColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        [SerializeField] private Color bronzeColor = new Color(0.8f, 0.5f, 0.2f, 1f);
        [SerializeField] private Color cyanColor = new Color(0f, 1f, 1f, 1f);
        [SerializeField] private Color greenColor = new Color(0f, 1f, 0.53f, 1f);
        [SerializeField] private Color urgentColor = new Color(1f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color myPositionHighlight = new Color(0f, 0.83f, 1f, 0.3f);

        // Estado
        private TournamentData tournamentData;
        private string currentUserId;
        private bool isExpanded = false;
        private Action<TournamentData> onPlayCallback;
        private Action<TournamentData> onViewLeaderboardCallback;
        private Action<TournamentData> onExitCallback;

        /// <summary>
        /// Configura el item con los datos del torneo
        /// </summary>
        public void Setup(TournamentData tournament, string userId,
            Action<TournamentData> onPlay = null,
            Action<TournamentData> onViewLeaderboard = null,
            Action<TournamentData> onExit = null)
        {
            tournamentData = tournament;
            currentUserId = userId;
            onPlayCallback = onPlay;
            onViewLeaderboardCallback = onViewLeaderboard;
            onExitCallback = onExit;

            // Header
            SetupHeader();

            // Stats del jugador
            SetupPlayerStats();

            // Info del creador
            SetupCreatorInfo();

            // Info general
            SetupTournamentInfo();

            // Botones
            SetupButtons();

            // Estado inicial: colapsado
            if (expandedSection != null)
            {
                expandedSection.SetActive(false);
                isExpanded = false;
            }

            // Background
            if (backgroundImage != null)
                backgroundImage.color = normalBgColor;

            if (headerBackground != null)
                headerBackground.color = headerColor;
        }

        #region Setup Methods

        private void SetupHeader()
        {
            // Nombre del torneo
            if (tournamentNameText != null)
            {
                tournamentNameText.text = tournamentData.name;
                tournamentNameText.color = cyanColor;
            }

            // Tiempo restante
            UpdateTimeRemaining();

            // Indicador de estado
            if (statusIndicator != null)
            {
                switch (tournamentData.status)
                {
                    case TournamentStatus.Active:
                        statusIndicator.color = greenColor;
                        break;
                    case TournamentStatus.Scheduled:
                        statusIndicator.color = goldColor;
                        break;
                    case TournamentStatus.Completed:
                        statusIndicator.color = Color.gray;
                        break;
                    default:
                        statusIndicator.color = urgentColor;
                        break;
                }
            }
        }

        private void SetupPlayerStats()
        {
            var myParticipant = tournamentData.participants?.Find(p => p.userId == currentUserId);

            if (myParticipant != null)
            {
                // Mi posición
                int myPosition = GetPlayerPosition(currentUserId);
                if (myPositionText != null)
                {
                    myPositionText.text = $"#{myPosition}";
                    myPositionText.color = GetPositionColor(myPosition);
                }

                // Mi mejor tiempo
                if (myBestTimeText != null)
                {
                    if (myParticipant.bestTime < float.MaxValue)
                    {
                        myBestTimeText.text = $"{myParticipant.bestTime:F3}s";
                        myBestTimeText.color = greenColor;
                    }
                    else
                    {
                        myBestTimeText.text = AutoLocalizer.Get("no_attempts");
                        myBestTimeText.color = Color.gray;
                    }
                }

                // Mis intentos
                if (myAttemptsText != null)
                {
                    if (tournamentData.rules.maxAttempts > 0)
                    {
                        myAttemptsText.text = $"{myParticipant.attempts}/{tournamentData.rules.maxAttempts}";
                        // Color urgente si quedan pocos intentos
                        bool lowAttempts = (tournamentData.rules.maxAttempts - myParticipant.attempts) <= 2;
                        myAttemptsText.color = lowAttempts ? urgentColor : Color.white;
                    }
                    else
                    {
                        myAttemptsText.text = $"{myParticipant.attempts}";
                        myAttemptsText.color = Color.white;
                    }
                }
            }
            else
            {
                // No encontrado (error)
                if (myPositionText != null) myPositionText.text = "--";
                if (myBestTimeText != null) myBestTimeText.text = "--";
                if (myAttemptsText != null) myAttemptsText.text = "--";
            }
        }

        private void SetupCreatorInfo()
        {
            // Mostrar información del creador del torneo
            if (creatorNameText != null)
            {
                creatorNameText.text = tournamentData.creatorName;
                creatorNameText.color = goldColor;
            }

            if (creatorTimeText != null)
            {
                // Buscar el tiempo del creador si está participando
                var creatorParticipant = tournamentData.participants?.Find(p => p.userId == tournamentData.creatorId);
                if (creatorParticipant != null && creatorParticipant.bestTime < float.MaxValue)
                {
                    creatorTimeText.text = $"{creatorParticipant.bestTime:F3}s";
                    creatorTimeText.color = greenColor;
                }
                else
                {
                    creatorTimeText.text = AutoLocalizer.Get("no_time");
                    creatorTimeText.color = Color.gray;
                }
            }
        }

        private void SetupTournamentInfo()
        {
            // Participantes
            if (participantsText != null)
            {
                participantsText.text = $"{tournamentData.currentParticipants}/{tournamentData.maxParticipants}";
            }

            // Intentos máximos
            if (maxAttemptsText != null)
            {
                if (tournamentData.rules.maxAttempts > 0)
                {
                    maxAttemptsText.text = $"{tournamentData.rules.maxAttempts} {AutoLocalizer.Get("max_attempts")}";
                    maxAttemptsText.gameObject.SetActive(true);
                }
                else
                {
                    maxAttemptsText.gameObject.SetActive(false);
                }
            }
        }

        private void SetupButtons()
        {
            // Expand button
            if (expandButton != null)
            {
                expandButton.onClick.RemoveAllListeners();
                expandButton.onClick.AddListener(ToggleExpand);
            }

            // Texto inicial del botón expandir
            if (expandButtonText != null)
            {
                expandButtonText.text = AutoLocalizer.Get("view_tournament_data");
            }

            // Play button
            if (playButton != null)
            {
                playButton.onClick.RemoveAllListeners();
                playButton.onClick.AddListener(() => onPlayCallback?.Invoke(tournamentData));

                // Deshabilitar si el torneo terminó o no quedan intentos
                bool canPlay = tournamentData.status == TournamentStatus.Active;
                if (tournamentData.rules.maxAttempts > 0)
                {
                    var myParticipant = tournamentData.participants?.Find(p => p.userId == currentUserId);
                    if (myParticipant != null && myParticipant.attempts >= tournamentData.rules.maxAttempts)
                    {
                        canPlay = false;
                    }
                }
                playButton.interactable = canPlay;
            }

            // View leaderboard button
            if (viewLeaderboardButton != null)
            {
                viewLeaderboardButton.onClick.RemoveAllListeners();
                viewLeaderboardButton.onClick.AddListener(() => onViewLeaderboardCallback?.Invoke(tournamentData));
            }

            // Exit tournament button
            if (exitTournamentButton != null)
            {
                exitTournamentButton.onClick.RemoveAllListeners();
                exitTournamentButton.onClick.AddListener(() => onExitCallback?.Invoke(tournamentData));
            }
        }

        #endregion

        #region Expand/Collapse

        private void ToggleExpand()
        {
            isExpanded = !isExpanded;

            if (expandedSection != null)
            {
                expandedSection.SetActive(isExpanded);
            }

            // Actualizar texto del botón
            if (expandButtonText != null)
            {
                expandButtonText.text = isExpanded
                    ? AutoLocalizer.Get("hide_tournament_data")
                    : AutoLocalizer.Get("view_tournament_data");
            }

            // Rotar flecha
            if (expandArrow != null)
            {
                expandArrow.transform.rotation = Quaternion.Euler(0, 0, isExpanded ? 180 : 0);
            }

            if (isExpanded)
            {
                PopulateLeaderboard();
            }
        }

        private void PopulateLeaderboard()
        {
            if (leaderboardContainer == null) return;

            // Limpiar entradas anteriores
            foreach (Transform child in leaderboardContainer)
            {
                Destroy(child.gameObject);
            }

            if (tournamentData.participants == null || tournamentData.participants.Count == 0)
                return;

            // Ordenar participantes
            tournamentData.SortParticipants();

            // Crear entradas
            for (int i = 0; i < tournamentData.participants.Count; i++)
            {
                var participant = tournamentData.participants[i];
                CreateLeaderboardEntry(i + 1, participant);
            }
        }

        private void CreateLeaderboardEntry(int position, ParticipantScore participant)
        {
            GameObject entryObj;

            if (leaderboardEntryPrefab != null)
            {
                entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
            }
            else
            {
                // Fallback: crear por código
                entryObj = CreateLeaderboardEntryFallback(position, participant);
            }

            bool isMe = participant.userId == currentUserId;

            // Configurar si hay componente
            var entryUI = entryObj.GetComponent<LeaderboardEntryUI>();
            if (entryUI != null)
            {
                // Crear LeaderboardEntry temporal
                var entry = new LeaderboardEntry
                {
                    position = position,
                    userId = participant.userId,
                    username = participant.username,
                    time = participant.bestTime < float.MaxValue ? participant.bestTime : 0
                };
                entryUI.Setup(entry, isMe);
            }

            // Resaltar si soy yo
            if (isMe)
            {
                var bg = entryObj.GetComponent<Image>();
                if (bg != null)
                {
                    bg.color = myPositionHighlight;
                }
            }
        }

        private GameObject CreateLeaderboardEntryFallback(int position, ParticipantScore participant)
        {
            GameObject entryObj = new GameObject($"Entry_{position}");
            entryObj.transform.SetParent(leaderboardContainer, false);

            RectTransform rt = entryObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 50);

            LayoutElement le = entryObj.AddComponent<LayoutElement>();
            le.preferredHeight = 50;
            le.flexibleWidth = 1;

            Image bg = entryObj.AddComponent<Image>();
            bool isMe = participant.userId == currentUserId;
            bg.color = isMe ? myPositionHighlight : new Color(0.1f, 0.1f, 0.15f, 0.8f);

            // Texto simple
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(entryObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            string timeStr = participant.bestTime < float.MaxValue ? $"{participant.bestTime:F3}s" : "--";
            tmp.text = $"  #{position}  {participant.username}  {timeStr}  ({participant.attempts})";
            tmp.fontSize = 18;
            tmp.color = isMe ? cyanColor : Color.white;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.verticalAlignment = VerticalAlignmentOptions.Middle;

            return entryObj;
        }

        #endregion

        #region Update Methods

        /// <summary>
        /// Actualiza el tiempo restante (llamar desde Update del manager)
        /// </summary>
        public void UpdateTimeRemaining()
        {
            if (tournamentData == null || timeRemainingText == null) return;

            TimeSpan timeRemaining = tournamentData.GetTimeRemaining();

            if (timeRemaining.TotalSeconds <= 0)
            {
                timeRemainingText.text = AutoLocalizer.Get("finished");
                timeRemainingText.color = urgentColor;
            }
            else
            {
                timeRemainingText.text = FormatTimeRemaining(timeRemaining);
                timeRemainingText.color = timeRemaining.TotalHours < 1 ? urgentColor : greenColor;
            }
        }

        /// <summary>
        /// Refresca toda la información del item
        /// </summary>
        public void Refresh()
        {
            if (tournamentData == null) return;

            SetupPlayerStats();
            SetupCreatorInfo();
            UpdateTimeRemaining();

            if (isExpanded)
            {
                PopulateLeaderboard();
            }
        }

        #endregion

        #region Helpers

        private int GetPlayerPosition(string userId)
        {
            if (tournamentData.participants == null) return -1;

            tournamentData.SortParticipants();

            for (int i = 0; i < tournamentData.participants.Count; i++)
            {
                if (tournamentData.participants[i].userId == userId)
                    return i + 1;
            }
            return -1;
        }

        private Color GetPositionColor(int position)
        {
            switch (position)
            {
                case 1: return goldColor;
                case 2: return silverColor;
                case 3: return bronzeColor;
                default: return Color.white;
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

        public TournamentData GetTournamentData()
        {
            return tournamentData;
        }

        #endregion

        #region Editor Setup Helper

        public void AutoSetupReferences()
        {
            // Header
            tournamentNameText = transform.Find("Header/TournamentName")?.GetComponent<TextMeshProUGUI>();
            timeRemainingText = transform.Find("Header/TimeRemaining")?.GetComponent<TextMeshProUGUI>();
            statusIndicator = transform.Find("Header/StatusIndicator")?.GetComponent<Image>();

            // Player Stats
            myPositionText = transform.Find("PlayerStats/MyPosition")?.GetComponent<TextMeshProUGUI>();
            myBestTimeText = transform.Find("PlayerStats/MyBestTime")?.GetComponent<TextMeshProUGUI>();
            myAttemptsText = transform.Find("PlayerStats/MyAttempts")?.GetComponent<TextMeshProUGUI>();

            // Creator
            creatorNameText = transform.Find("CreatorInfo/CreatorName")?.GetComponent<TextMeshProUGUI>();
            creatorTimeText = transform.Find("CreatorInfo/CreatorTime")?.GetComponent<TextMeshProUGUI>();

            // Info
            participantsText = transform.Find("TournamentInfo/Participants")?.GetComponent<TextMeshProUGUI>();
            maxAttemptsText = transform.Find("TournamentInfo/MaxAttempts")?.GetComponent<TextMeshProUGUI>();

            // Expanded
            expandedSection = transform.Find("ExpandedSection")?.gameObject;
            leaderboardContainer = transform.Find("ExpandedSection/LeaderboardContainer");
            expandButton = transform.Find("ExpandButton")?.GetComponent<Button>();
            expandButtonText = transform.Find("ExpandButton/Text")?.GetComponent<TextMeshProUGUI>();
            expandArrow = transform.Find("ExpandButton/Arrow")?.GetComponent<Image>();

            // Actions
            playButton = transform.Find("Actions/PlayButton")?.GetComponent<Button>();
            viewLeaderboardButton = transform.Find("Actions/ViewLeaderboardButton")?.GetComponent<Button>();
            exitTournamentButton = transform.Find("Actions/ExitButton")?.GetComponent<Button>();

            // Background
            backgroundImage = GetComponent<Image>();
            headerBackground = transform.Find("Header")?.GetComponent<Image>();
        }

        #endregion
    }
}
