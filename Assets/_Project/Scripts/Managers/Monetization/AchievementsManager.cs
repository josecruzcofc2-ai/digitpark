using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using DigitPark.UI.Items;
using DigitPark.Monetization;
using DigitPark.Localization;
using DG.Tweening;

namespace DigitPark.Managers
{
    /// <summary>
    /// Trophy Showcase Achievements Manager.
    /// Displays achievements as collectible trophies in glass display cases.
    /// </summary>
    public class AchievementsManager : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI totalPointsText;
        [SerializeField] private TextMeshProUGUI completionText;
        [SerializeField] private Slider overallProgressBar;

        [Header("Category Tabs (11 categories)")]
        [SerializeField] private Transform tabsContainer;
        [SerializeField] private ScrollRect tabsScrollRect;
        [SerializeField] private Button allTab;
        [SerializeField] private Button beginnerTab;
        [SerializeField] private Button masteryTab;
        [SerializeField] private Button victoriesTab;
        [SerializeField] private Button streaksTab;
        [SerializeField] private Button cashBattleTab;
        [SerializeField] private Button tournamentsTab;
        [SerializeField] private Button socialTab;
        [SerializeField] private Button progressionTab;
        [SerializeField] private Button collectorTab;
        [SerializeField] private Button timeTab;
        [SerializeField] private Button secretTab;

        [Header("Trophy Showcase")]
        [SerializeField] private Transform showcaseContainer;
        [SerializeField] private GameObject trophyCardPrefab;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private GridLayoutGroup gridLayout;

        [Header("Empty State")]
        [SerializeField] private GameObject emptyStateContainer;
        [SerializeField] private TextMeshProUGUI emptyStateText;
        [SerializeField] private Image emptyStateIcon;

        [Header("Detail Panel")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private CanvasGroup detailPanelCanvasGroup;
        [SerializeField] private Image detailBlocker;
        [SerializeField] private RectTransform detailCard;

        [Header("Detail Panel - Content")]
        [SerializeField] private Image detailTrophyIcon;
        [SerializeField] private TextMeshProUGUI detailTitleText;
        [SerializeField] private TextMeshProUGUI detailDescriptionText;
        [SerializeField] private TextMeshProUGUI detailCategoryText;
        [SerializeField] private Slider detailProgressBar;
        [SerializeField] private TextMeshProUGUI detailProgressText;
        [SerializeField] private TextMeshProUGUI detailPointsText;
        [SerializeField] private Button claimRewardButton;
        [SerializeField] private TextMeshProUGUI claimButtonText;
        [SerializeField] private Button closeDetailButton;
        [SerializeField] private ParticleSystem detailParticles;

        [Header("Reward Celebration")]
        [SerializeField] private GameObject rewardCelebration;
        [SerializeField] private TextMeshProUGUI rewardAmountText;
        [SerializeField] private ParticleSystem celebrationParticles;
        [SerializeField] private Image celebrationGlow;

        [Header("Icons")]
        [SerializeField] private Sprite defaultTrophyIcon;
        [SerializeField] private Sprite lockedTrophyIcon;
        [SerializeField] private Sprite secretTrophyIcon;

        [Header("Tab Colors")]
        [SerializeField] private Color tabActiveColor = new Color(0f, 1f, 1f, 1f);
        [SerializeField] private Color tabInactiveColor = new Color(0.2f, 0.25f, 0.3f, 1f);
        [SerializeField] private Color tabActiveTextColor = new Color(0.02f, 0.05f, 0.1f, 1f);
        [SerializeField] private Color tabInactiveTextColor = new Color(0.6f, 0.6f, 0.6f, 1f);

        // State
        private AchievementCategory currentCategory = AchievementCategory.All;
        private List<AchievementDefinition> allAchievements = new List<AchievementDefinition>();
        private List<TrophyCardUI> spawnedCards = new List<TrophyCardUI>();
        private AchievementData selectedAchievement;
        private TrophyCardUI selectedCard;

        public enum AchievementCategory
        {
            All,
            Beginner,       // Onboarding achievements
            Mastery,        // Game mastery achievements
            Victories,      // Win-based achievements
            Streaks,        // Win streak achievements
            CashBattle,     // Cash battle achievements
            Tournaments,    // Tournament achievements
            Social,         // Friend-based achievements
            Progression,    // Level/rank achievements
            Collector,      // Collection achievements (reserved for V2)
            Time,           // Login/dedication achievements
            Secret          // Hidden achievements
        }

        #region Initialization

        private void Start()
        {
            InitializeAchievements();
            SetupUI();
            SetupListeners();
            LoadShowcase();
        }

        private void InitializeAchievements()
        {
            // Define all achievements with their data
            // Total: 53 achievements across 11 categories
            allAchievements = new List<AchievementDefinition>
            {
                // ==================== BEGINNER (Onboarding) ====================
                new AchievementDefinition("first_game", "Primer Paso", "Completa tu primera partida", AchievementCategory.Beginner, 10, 1, "Logro_Primeros_Pasos"),
                new AchievementDefinition("tutorial_complete", "Aprendiz", "Completa el tutorial", AchievementCategory.Beginner, 10, 1, "Logro_Graduado"),
                new AchievementDefinition("first_win", "Primera Victoria", "Gana tu primera partida", AchievementCategory.Beginner, 15, 1, "Logro_Primera_Victoria"),
                new AchievementDefinition("profile_complete", "Identidad", "Completa tu perfil (avatar, nombre)", AchievementCategory.Beginner, 10, 1, "Logro_Perfil_Completo"),

                // ==================== MASTERY (Per Game) ====================
                new AchievementDefinition("digitrush_master", "Maestro de Dígitos", "Alcanza 10,000 puntos en DigitRush", AchievementCategory.Mastery, 50, 10000, "Logro_Maestro_Numeros"),
                new AchievementDefinition("flashtap_master", "Reflejos de Luz", "Alcanza 100 taps perfectos en FlashTap", AchievementCategory.Mastery, 50, 100, "Logro_Reflejos_Rayo"),
                new AchievementDefinition("memorypairs_master", "Memoria Fotográfica", "Completa MemoryPairs sin errores", AchievementCategory.Mastery, 50, 1, "Logro_Genio"),
                new AchievementDefinition("quickmath_master", "Calculadora Humana", "Resuelve 50 problemas seguidos en QuickMath", AchievementCategory.Mastery, 50, 50, "Logro_Maestro_Matematicas"),
                new AchievementDefinition("oddoneout_master", "Ojo de Águila", "Encuentra 100 diferencias en OddOneOut", AchievementCategory.Mastery, 50, 100, "Logro_Ojo_Aguila"),

                // ==================== VICTORIES ====================
                new AchievementDefinition("wins_10", "Competidor", "Gana 10 partidas", AchievementCategory.Victories, 20, 10, "Logro_10_Victorias"),
                new AchievementDefinition("wins_50", "Veterano", "Gana 50 partidas", AchievementCategory.Victories, 40, 50, "Logro_50_Victorias"),
                new AchievementDefinition("wins_100", "Centurión", "Gana 100 partidas", AchievementCategory.Victories, 60, 100, "Logro_Centurion"),
                new AchievementDefinition("wins_500", "Leyenda", "Gana 500 partidas", AchievementCategory.Victories, 100, 500, "Logro_500_Victorias"),
                new AchievementDefinition("wins_1000", "Inmortal", "Gana 1,000 partidas", AchievementCategory.Victories, 200, 1000, "Logro_1000_Victorias"),

                // ==================== STREAKS ====================
                new AchievementDefinition("streak_3", "En Racha", "Gana 3 partidas seguidas", AchievementCategory.Streaks, 25, 3, "Logro_Racha_Fuego"),
                new AchievementDefinition("streak_5", "Imparable", "Gana 5 partidas seguidas", AchievementCategory.Streaks, 40, 5, "Logro_Victoria_Racha_7"),
                new AchievementDefinition("streak_10", "Dominación", "Gana 10 partidas seguidas", AchievementCategory.Streaks, 75, 10, "Logro_Demoledor"),
                new AchievementDefinition("streak_20", "Invencible", "Gana 20 partidas seguidas", AchievementCategory.Streaks, 150, 20, "Logro_Victoria_Racha_30", true), // SECRET

                // ==================== CASH BATTLE ====================
                new AchievementDefinition("cash_first", "Apostador", "Completa tu primera Cash Battle", AchievementCategory.CashBattle, 25, 1, "Logro_Ficha_Cash"),
                new AchievementDefinition("cash_first_win", "Ganador Real", "Gana tu primera Cash Battle", AchievementCategory.CashBattle, 35, 1, "Logro_Rey_Monedas"),
                new AchievementDefinition("cash_10_wins", "Jugador Serio", "Gana 10 Cash Battles", AchievementCategory.CashBattle, 50, 10, "Logro_VIP_1000"),
                new AchievementDefinition("cash_50_wins", "High Roller", "Gana 50 Cash Battles", AchievementCategory.CashBattle, 100, 50, "Logro_VIP_Dados"),
                new AchievementDefinition("cash_100_wins", "Tiburón", "Gana 100 Cash Battles", AchievementCategory.CashBattle, 200, 100, "Logro_Tiburon_Cash"),
                new AchievementDefinition("cash_earnings_100", "Primeros $100", "Acumula $100 en ganancias", AchievementCategory.CashBattle, 75, 100, "Logro_Bolsa_100"),
                new AchievementDefinition("cash_earnings_1000", "Club de los Mil", "Acumula $1,000 en ganancias", AchievementCategory.CashBattle, 250, 1000, "Logro_Millonario", true), // SECRET

                // ==================== TOURNAMENTS ====================
                new AchievementDefinition("tournament_first", "Participante", "Participa en tu primer torneo", AchievementCategory.Tournaments, 20, 1, "Logro_Torneo_Bracket"),
                new AchievementDefinition("tournament_top3", "Podio", "Termina en Top 3 de un torneo", AchievementCategory.Tournaments, 50, 1, "Logro_Coleccion_Trofeos"),
                new AchievementDefinition("tournament_win", "Campeón", "Gana un torneo", AchievementCategory.Tournaments, 100, 1, "Logro_Campeon_1"),
                new AchievementDefinition("tournament_5_wins", "Multicampeón", "Gana 5 torneos", AchievementCategory.Tournaments, 200, 5, "Logro_4_Estrellas"),
                new AchievementDefinition("tournament_create", "Organizador", "Crea tu primer torneo", AchievementCategory.Tournaments, 30, 1, "Logro_Organizador_Torneo"),

                // ==================== SOCIAL ====================
                new AchievementDefinition("friend_first", "Primer Amigo", "Añade tu primer amigo", AchievementCategory.Social, 15, 1, "Logro_Primer_Rival"),
                new AchievementDefinition("friends_10", "Popular", "Tiene 10 amigos", AchievementCategory.Social, 30, 10, "Logro_Social_10_Amigos"),
                new AchievementDefinition("friends_50", "Influencer", "Tiene 50 amigos", AchievementCategory.Social, 75, 50, "Logro_Influencer"),
                new AchievementDefinition("challenge_friend", "Retador", "Reta a un amigo a una partida", AchievementCategory.Social, 20, 1, "Logro_Versus"),
                new AchievementDefinition("beat_friend", "Rival", "Vence a un amigo", AchievementCategory.Social, 25, 1, "Logro_Amigo_Rival"),

                // ==================== PROGRESSION ====================
                new AchievementDefinition("level_10", "Nivel 10", "Alcanza el nivel 10", AchievementCategory.Progression, 25, 10, "Logro_Nivel_10"),
                new AchievementDefinition("level_25", "Nivel 25", "Alcanza el nivel 25", AchievementCategory.Progression, 50, 25, "Logro_Nivel_25"),
                new AchievementDefinition("level_50", "Nivel 50", "Alcanza el nivel 50", AchievementCategory.Progression, 75, 50, "Logro_Nivel50"),
                new AchievementDefinition("level_100", "Nivel 100", "Alcanza el nivel 100", AchievementCategory.Progression, 150, 100, "Logro_Avance_Epico"),

                // ==================== COLLECTOR ==================== (Reservado para V2)

                // ==================== TIME ====================
                new AchievementDefinition("days_7", "Una Semana", "Juega 7 días", AchievementCategory.Time, 25, 7, "Logro_Racha_7_Dias"),
                new AchievementDefinition("days_30", "Un Mes", "Juega 30 días", AchievementCategory.Time, 50, 30, "Logro_Racha_30_Dias"),
                new AchievementDefinition("days_100", "100 Días", "Juega 100 días", AchievementCategory.Time, 100, 100, "Logro_Racha_100_Dias"),
                new AchievementDefinition("days_365", "Un Año", "Juega 365 días", AchievementCategory.Time, 300, 365, "Logro_Racha_365_Dias", true), // SECRET
                new AchievementDefinition("daily_streak_7", "Racha Semanal", "Login 7 días seguidos", AchievementCategory.Time, 30, 7, "Logro_Login_Semanal"),
                new AchievementDefinition("daily_streak_30", "Racha Mensual", "Login 30 días seguidos", AchievementCategory.Time, 75, 30, "Logro_Login_Mensual"),

                // ==================== SECRET ====================
                new AchievementDefinition("night_owl", "Búho Nocturno", "Juega a las 3:00 AM", AchievementCategory.Secret, 50, 1, "Logro_Buho_Nocturno", true),
                new AchievementDefinition("perfect_game", "Perfección", "Completa cualquier juego con 100% precisión", AchievementCategory.Secret, 100, 1, "Logro_Perfeccionista", true),
                new AchievementDefinition("comeback_king", "Rey del Comeback", "Gana perdiendo por 50%+", AchievementCategory.Secret, 75, 1, "Logro_Ave_Fenix", true),
                new AchievementDefinition("speed_demon", "Demonio de Velocidad", "Completa un juego en menos de 10 segundos", AchievementCategory.Secret, 100, 1, "Logro_Demonio_Velocidad", true),
            };

            // Load saved progress
            LoadAllProgress();
        }

        private void LoadAllProgress()
        {
            foreach (var achievement in allAchievements)
            {
                achievement.currentProgress = PlayerPrefs.GetInt($"Achievement_{achievement.id}_progress", 0);
                achievement.isCompleted = PlayerPrefs.GetInt($"Achievement_{achievement.id}_completed", 0) == 1;
                achievement.isClaimed = PlayerPrefs.GetInt($"Achievement_{achievement.id}_claimed", 0) == 1;
            }
        }

        private void SaveProgress(AchievementDefinition achievement)
        {
            PlayerPrefs.SetInt($"Achievement_{achievement.id}_progress", achievement.currentProgress);
            PlayerPrefs.SetInt($"Achievement_{achievement.id}_completed", achievement.isCompleted ? 1 : 0);
            PlayerPrefs.SetInt($"Achievement_{achievement.id}_claimed", achievement.isClaimed ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void SetupUI()
        {
            if (detailPanel) detailPanel.SetActive(false);
            if (rewardCelebration) rewardCelebration.SetActive(false);
            if (emptyStateContainer) emptyStateContainer.SetActive(false);

            UpdateHeaderStats();
            UpdateTabVisuals();
        }

        private void SetupListeners()
        {
            // Back button
            if (backButton) backButton.onClick.AddListener(OnBackClicked);

            // Detail panel
            if (closeDetailButton) closeDetailButton.onClick.AddListener(CloseDetailPanel);
            if (detailBlocker) detailBlocker.GetComponent<Button>()?.onClick.AddListener(CloseDetailPanel);
            if (claimRewardButton) claimRewardButton.onClick.AddListener(ClaimReward);

            // Tabs - 11 categories with scrollable tabs
            if (allTab) allTab.onClick.AddListener(() => SwitchCategory(AchievementCategory.All));
            if (beginnerTab) beginnerTab.onClick.AddListener(() => SwitchCategory(AchievementCategory.Beginner));
            if (masteryTab) masteryTab.onClick.AddListener(() => SwitchCategory(AchievementCategory.Mastery));
            if (victoriesTab) victoriesTab.onClick.AddListener(() => SwitchCategory(AchievementCategory.Victories));
            if (streaksTab) streaksTab.onClick.AddListener(() => SwitchCategory(AchievementCategory.Streaks));
            if (cashBattleTab) cashBattleTab.onClick.AddListener(() => SwitchCategory(AchievementCategory.CashBattle));
            if (tournamentsTab) tournamentsTab.onClick.AddListener(() => SwitchCategory(AchievementCategory.Tournaments));
            if (socialTab) socialTab.onClick.AddListener(() => SwitchCategory(AchievementCategory.Social));
            if (progressionTab) progressionTab.onClick.AddListener(() => SwitchCategory(AchievementCategory.Progression));
            if (collectorTab) collectorTab.onClick.AddListener(() => SwitchCategory(AchievementCategory.Collector));
            if (timeTab) timeTab.onClick.AddListener(() => SwitchCategory(AchievementCategory.Time));
            if (secretTab) secretTab.onClick.AddListener(() => SwitchCategory(AchievementCategory.Secret));
        }

        #endregion

        #region Header Stats

        private void UpdateHeaderStats()
        {
            int totalPoints = 0;
            int earnedPoints = 0;
            int completed = 0;
            int total = allAchievements.Count;

            foreach (var achievement in allAchievements)
            {
                totalPoints += achievement.points;
                if (achievement.isCompleted)
                {
                    earnedPoints += achievement.points;
                    completed++;
                }
            }

            if (totalPointsText)
            {
                totalPointsText.text = $"{earnedPoints:N0}";
            }

            if (completionText)
            {
                int percentage = total > 0 ? (completed * 100 / total) : 0;
                completionText.text = $"{completed}/{total} ({percentage}%)";
            }

            if (overallProgressBar)
            {
                float progress = total > 0 ? (float)completed / total : 0f;
                overallProgressBar.DOValue(progress, 0.5f).SetEase(Ease.OutCubic);
            }
        }

        #endregion

        #region Category Tabs

        private void SwitchCategory(AchievementCategory category)
        {
            if (currentCategory == category) return;

            currentCategory = category;
            UpdateTabVisuals();
            LoadShowcase();

            // Scroll to top
            if (scrollRect)
            {
                scrollRect.DOVerticalNormalizedPos(1f, 0.3f);
            }
        }

        private void UpdateTabVisuals()
        {
            UpdateTabButton(allTab, currentCategory == AchievementCategory.All);
            UpdateTabButton(beginnerTab, currentCategory == AchievementCategory.Beginner);
            UpdateTabButton(masteryTab, currentCategory == AchievementCategory.Mastery);
            UpdateTabButton(victoriesTab, currentCategory == AchievementCategory.Victories);
            UpdateTabButton(streaksTab, currentCategory == AchievementCategory.Streaks);
            UpdateTabButton(cashBattleTab, currentCategory == AchievementCategory.CashBattle);
            UpdateTabButton(tournamentsTab, currentCategory == AchievementCategory.Tournaments);
            UpdateTabButton(socialTab, currentCategory == AchievementCategory.Social);
            UpdateTabButton(progressionTab, currentCategory == AchievementCategory.Progression);
            UpdateTabButton(collectorTab, currentCategory == AchievementCategory.Collector);
            UpdateTabButton(timeTab, currentCategory == AchievementCategory.Time);
            UpdateTabButton(secretTab, currentCategory == AchievementCategory.Secret);
        }

        private void UpdateTabButton(Button button, bool isActive)
        {
            if (button == null) return;

            var image = button.GetComponent<Image>();
            var text = button.GetComponentInChildren<TextMeshProUGUI>();

            if (image)
            {
                image.DOColor(isActive ? tabActiveColor : tabInactiveColor, 0.2f);
            }

            if (text)
            {
                text.DOColor(isActive ? tabActiveTextColor : tabInactiveTextColor, 0.2f);
            }

            // Scale animation
            button.transform.DOScale(isActive ? 1.05f : 1f, 0.2f).SetEase(Ease.OutCubic);
        }

        #endregion

        #region Trophy Showcase

        private void LoadShowcase()
        {
            ClearShowcase();

            var filtered = FilterAchievements();

            if (filtered.Count == 0)
            {
                ShowEmptyState();
                return;
            }

            HideEmptyState();

            // Create trophy cards with staggered animation
            for (int i = 0; i < filtered.Count; i++)
            {
                CreateTrophyCard(filtered[i], i * 0.05f);
            }
        }

        private List<AchievementDefinition> FilterAchievements()
        {
            return currentCategory switch
            {
                AchievementCategory.Beginner => allAchievements.FindAll(a => a.category == AchievementCategory.Beginner),
                AchievementCategory.Mastery => allAchievements.FindAll(a => a.category == AchievementCategory.Mastery),
                AchievementCategory.Victories => allAchievements.FindAll(a => a.category == AchievementCategory.Victories),
                AchievementCategory.Streaks => allAchievements.FindAll(a => a.category == AchievementCategory.Streaks),
                AchievementCategory.CashBattle => allAchievements.FindAll(a => a.category == AchievementCategory.CashBattle),
                AchievementCategory.Tournaments => allAchievements.FindAll(a => a.category == AchievementCategory.Tournaments),
                AchievementCategory.Social => allAchievements.FindAll(a => a.category == AchievementCategory.Social),
                AchievementCategory.Progression => allAchievements.FindAll(a => a.category == AchievementCategory.Progression),
                AchievementCategory.Collector => allAchievements.FindAll(a => a.category == AchievementCategory.Collector),
                AchievementCategory.Time => allAchievements.FindAll(a => a.category == AchievementCategory.Time),
                AchievementCategory.Secret => allAchievements.FindAll(a => a.isSecret), // Show all secrets
                _ => allAchievements.FindAll(a => !a.isSecret) // All shows non-secret achievements
            };
        }

        private void CreateTrophyCard(AchievementDefinition definition, float delay)
        {
            GameObject cardObj;

            if (trophyCardPrefab != null)
            {
                cardObj = Instantiate(trophyCardPrefab, showcaseContainer);
            }
            else
            {
                // Create fallback card
                cardObj = CreateFallbackTrophyCard();
                cardObj.transform.SetParent(showcaseContainer, false);
            }

            var card = cardObj.GetComponent<TrophyCardUI>();
            if (card == null)
            {
                card = cardObj.AddComponent<TrophyCardUI>();
            }

            // Convert to AchievementData
            var data = new AchievementData
            {
                id = definition.id,
                title = definition.title,
                description = definition.description,
                category = definition.category.ToString(),
                points = definition.points,
                targetProgress = definition.targetProgress,
                currentProgress = definition.currentProgress,
                isCompleted = definition.isCompleted,
                isClaimed = definition.isClaimed,
                isSecret = definition.isSecret,
                icon = LoadAchievementIcon(definition.iconName)
            };

            card.Setup(data);
            card.OnCardClicked += OnTrophyCardClicked;

            spawnedCards.Add(card);

            // Entrance animation
            cardObj.transform.localScale = Vector3.zero;
            cardObj.transform.DOScale(1f, 0.4f)
                .SetDelay(delay)
                .SetEase(Ease.OutBack);
        }

        private Sprite LoadAchievementIcon(string iconName)
        {
            if (string.IsNullOrEmpty(iconName))
            {
                Debug.LogWarning($"[Achievements] Icon name is empty, using default");
                return defaultTrophyIcon;
            }

            // Try to load from Resources
            var sprite = Resources.Load<Sprite>($"Icons/Achievements/{iconName}");
            if (sprite != null)
            {
                Debug.Log($"[Achievements] Loaded icon: {iconName}");
                return sprite;
            }

            // Try loading from alternate path
            sprite = Resources.Load<Sprite>($"Achievements/{iconName}");
            if (sprite != null)
            {
                Debug.Log($"[Achievements] Loaded icon from alternate path: {iconName}");
                return sprite;
            }

            Debug.LogWarning($"[Achievements] Could not load icon: {iconName} - using default");
            return defaultTrophyIcon;
        }

        private GameObject CreateFallbackTrophyCard()
        {
            // Create a basic card structure when no prefab is available
            var card = new GameObject("TrophyCard");
            var rt = card.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300f, 350f);

            // Background
            var bg = card.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.08f, 0.12f, 0.95f);

            // This is a simplified fallback - the prefab will have all the proper structure
            return card;
        }

        private void ClearShowcase()
        {
            foreach (var card in spawnedCards)
            {
                if (card != null)
                {
                    card.OnCardClicked -= OnTrophyCardClicked;
                    Destroy(card.gameObject);
                }
            }
            spawnedCards.Clear();
        }

        private void ShowEmptyState()
        {
            if (emptyStateContainer)
            {
                emptyStateContainer.SetActive(true);

                if (emptyStateText)
                {
                    emptyStateText.text = currentCategory switch
                    {
                        AchievementCategory.Secret => "Los logros secretos se revelan al completarlos...",
                        _ => "No hay logros en esta categoría"
                    };
                }
            }
        }

        private void HideEmptyState()
        {
            if (emptyStateContainer)
            {
                emptyStateContainer.SetActive(false);
            }
        }

        #endregion

        #region Detail Panel

        private void OnTrophyCardClicked(TrophyCardUI card, AchievementData data)
        {
            selectedCard = card;
            selectedAchievement = data;
            ShowDetailPanel(data);
        }

        private void ShowDetailPanel(AchievementData data)
        {
            if (detailPanel == null) return;

            detailPanel.SetActive(true);

            // Update content
            if (detailTrophyIcon)
            {
                detailTrophyIcon.sprite = data.isSecret && !data.isCompleted ? secretTrophyIcon : data.icon ?? defaultTrophyIcon;
                detailTrophyIcon.color = data.isCompleted ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            }

            if (detailTitleText)
            {
                detailTitleText.text = data.isSecret && !data.isCompleted ? "???" : data.title;
            }

            if (detailDescriptionText)
            {
                detailDescriptionText.text = data.isSecret && !data.isCompleted
                    ? "Completa este logro secreto para descubrir su descripción..."
                    : data.description;
            }

            if (detailCategoryText)
            {
                detailCategoryText.text = data.category.ToUpper();
            }

            if (detailProgressBar)
            {
                detailProgressBar.maxValue = data.targetProgress;
                detailProgressBar.value = data.currentProgress;
            }

            if (detailProgressText)
            {
                if (data.isCompleted)
                {
                    detailProgressText.text = L("completed");
                    detailProgressText.color = new Color(0f, 1f, 0.5f);
                }
                else
                {
                    detailProgressText.text = $"{data.currentProgress} / {data.targetProgress}";
                    detailProgressText.color = new Color(1f, 0.84f, 0f);
                }
            }

            if (detailPointsText)
            {
                detailPointsText.text = $"+{data.points} pts";
            }

            // Claim button
            if (claimRewardButton)
            {
                bool canClaim = data.isCompleted && !data.isClaimed;
                claimRewardButton.gameObject.SetActive(canClaim);
                claimRewardButton.interactable = canClaim;
            }

            // Particles
            if (detailParticles && data.isCompleted)
            {
                detailParticles.Play();
            }

            // Animate in
            AnimateDetailPanelIn();
        }

        private void AnimateDetailPanelIn()
        {
            if (detailBlocker)
            {
                detailBlocker.color = new Color(0, 0, 0, 0);
                detailBlocker.DOColor(new Color(0, 0, 0, 0.85f), 0.3f);
            }

            if (detailCard)
            {
                detailCard.localScale = Vector3.one * 0.8f;
                detailCard.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
            }

            if (detailPanelCanvasGroup)
            {
                detailPanelCanvasGroup.alpha = 0f;
                detailPanelCanvasGroup.DOFade(1f, 0.3f);
            }
        }

        private void CloseDetailPanel()
        {
            if (detailPanel == null) return;

            // Deselect card
            if (selectedCard != null)
            {
                selectedCard.SetSelected(false);
            }

            // Animate out
            Sequence seq = DOTween.Sequence();

            if (detailCard)
            {
                seq.Append(detailCard.DOScale(0.8f, 0.2f).SetEase(Ease.InCubic));
            }

            if (detailBlocker)
            {
                seq.Join(detailBlocker.DOColor(new Color(0, 0, 0, 0), 0.2f));
            }

            if (detailPanelCanvasGroup)
            {
                seq.Join(detailPanelCanvasGroup.DOFade(0f, 0.2f));
            }

            seq.OnComplete(() =>
            {
                detailPanel.SetActive(false);
                selectedAchievement = null;
                selectedCard = null;
            });
        }

        #endregion

        #region Rewards

        private void ClaimReward()
        {
            if (selectedAchievement == null || selectedAchievement.isClaimed) return;

            // Find and update the definition
            var definition = allAchievements.Find(a => a.id == selectedAchievement.id);
            if (definition != null)
            {
                definition.isClaimed = true;
                SaveProgress(definition);
            }

            selectedAchievement.isClaimed = true;

            // Hide claim button
            if (claimRewardButton)
            {
                claimRewardButton.interactable = false;
                claimRewardButton.transform.DOScale(0f, 0.2f);
            }

            // Show celebration
            ShowRewardCelebration(selectedAchievement.points);

            // Update header
            UpdateHeaderStats();

            Debug.Log($"[Achievements] Claimed reward: {selectedAchievement.title} (+{selectedAchievement.points} pts)");
        }

        private void ShowRewardCelebration(int points)
        {
            if (rewardCelebration == null) return;

            rewardCelebration.SetActive(true);

            if (rewardAmountText)
            {
                rewardAmountText.text = $"+{points}";
                rewardAmountText.transform.localScale = Vector3.zero;
                rewardAmountText.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
            }

            if (celebrationParticles)
            {
                celebrationParticles.Play();
            }

            if (celebrationGlow)
            {
                celebrationGlow.color = new Color(1f, 0.84f, 0f, 0f);
                celebrationGlow.DOColor(new Color(1f, 0.84f, 0f, 0.5f), 0.3f)
                    .SetLoops(4, LoopType.Yoyo);
            }

            // Hide after delay
            DOVirtual.DelayedCall(2.5f, () =>
            {
                if (rewardCelebration)
                {
                    rewardCelebration.SetActive(false);
                }
            });
        }

        #endregion

        #region Public API

        /// <summary>
        /// Update progress for an achievement (call from game logic)
        /// </summary>
        public void UpdateProgress(string achievementId, int progress)
        {
            var achievement = allAchievements.Find(a => a.id == achievementId);
            if (achievement == null || achievement.isCompleted) return;

            achievement.currentProgress = Mathf.Min(progress, achievement.targetProgress);

            if (achievement.currentProgress >= achievement.targetProgress)
            {
                achievement.isCompleted = true;
                Debug.Log($"[Achievements] Completed: {achievement.title}");

                // Show toast notification (works in any scene)
                if (AchievementNotificationManager.Instance != null)
                {
                    AchievementNotificationManager.Instance.ShowNotification(achievement);
                }

                // Find and animate the card (only if in Achievements scene)
                var card = spawnedCards.Find(c => c.GetData()?.id == achievementId);
                if (card != null)
                {
                    card.PlayUnlockAnimation();
                }
            }

            SaveProgress(achievement);
            UpdateHeaderStats();

            // Refresh card if visible
            var visibleCard = spawnedCards.Find(c => c.GetData()?.id == achievementId);
            if (visibleCard != null)
            {
                visibleCard.RefreshProgress(achievement.currentProgress);
            }
        }

        /// <summary>
        /// Increment progress for an achievement
        /// </summary>
        public void IncrementProgress(string achievementId, int amount = 1)
        {
            var achievement = allAchievements.Find(a => a.id == achievementId);
            if (achievement == null) return;

            UpdateProgress(achievementId, achievement.currentProgress + amount);
        }

        /// <summary>
        /// Check if an achievement is completed
        /// </summary>
        public bool IsCompleted(string achievementId)
        {
            var achievement = allAchievements.Find(a => a.id == achievementId);
            return achievement?.isCompleted ?? false;
        }

        /// <summary>
        /// Get total earned points
        /// </summary>
        public int GetTotalPoints()
        {
            return allAchievements.Where(a => a.isCompleted).Sum(a => a.points);
        }

        #endregion

        #region Navigation

        private void OnBackClicked()
        {
            SceneNavigator.Instance?.GoBack();
        }

        #endregion

        private string L(string key, params object[] args)
        {
            if (LocalizationManager.Instance == null) return key;
            string text = LocalizationManager.Instance.GetText(key);
            return args.Length > 0 ? string.Format(text, args) : text;
        }
    }

    /// <summary>
    /// Internal achievement definition
    /// </summary>
    [Serializable]
    public class AchievementDefinition
    {
        public string id;
        public string title;
        public string description;
        public AchievementsManager.AchievementCategory category;
        public int points;
        public int targetProgress;
        public int currentProgress;
        public bool isCompleted;
        public bool isClaimed;
        public bool isSecret;
        public string iconName;

        public AchievementDefinition(string id, string title, string description,
            AchievementsManager.AchievementCategory category, int points, int target,
            string iconName = null, bool isSecret = false)
        {
            this.id = id;
            this.title = title;
            this.description = description;
            this.category = category;
            this.points = points;
            this.targetProgress = target;
            this.iconName = iconName;
            this.isSecret = isSecret;
        }
    }
}
