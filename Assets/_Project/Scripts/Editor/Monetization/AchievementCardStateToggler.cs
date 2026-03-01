using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using DigitPark.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// EditorWindow to toggle all Trophy Cards between visual states
    /// (Locked, Unlocked, In-Progress, Secret, Mix) for preview in the editor.
    /// </summary>
    public class AchievementCardStateToggler : EditorWindow
    {
        // Alias colors from AchievementsUIBuilder
        private static Color CARD_BG => AchievementsUIBuilder.CARD_BG;
        private static Color TEXT_PRIMARY => AchievementsUIBuilder.TEXT_PRIMARY;
        private static Color TEXT_SECONDARY => AchievementsUIBuilder.TEXT_SECONDARY;
        private static Color TEXT_DARK => AchievementsUIBuilder.TEXT_DARK;
        private static Color BUTTON_SUCCESS => AchievementsUIBuilder.BUTTON_SUCCESS;
        private static Color CAT_SECRET => AchievementsUIBuilder.CAT_SECRET;
        private static Color TROPHY_UNLOCKED_GLOW => AchievementsUIBuilder.TROPHY_UNLOCKED_GLOW;
        private static Color TROPHY_PROGRESS_GLOW => AchievementsUIBuilder.TROPHY_PROGRESS_GLOW;
        private static Color TROPHY_SECRET_GLOW => AchievementsUIBuilder.TROPHY_SECRET_GLOW;
        private static Color GLASS_OVERLAY => AchievementsUIBuilder.GLASS_OVERLAY;

        private static readonly Color LOCKED_BORDER = new Color(0.3f, 0.3f, 0.4f, 0.3f);

        [MenuItem("DigitPark/UI Builders/Monetization/Achievements Card State Toggler", false, 141)]
        public static void ShowWindow()
        {
            var window = GetWindow<AchievementCardStateToggler>("Card State Toggler");
            window.minSize = new Vector2(320, 340);
        }

        private void OnGUI()
        {
            GUILayout.Label("Achievement Card State Toggler", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            // Scene check
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName != "Achievements")
            {
                EditorGUILayout.HelpBox(
                    $"Escena actual: {sceneName}\nAbre la escena Achievements para usar esta herramienta.",
                    MessageType.Warning);
                return;
            }

            // Find grid
            GridLayoutGroup grid = FindTrophyGrid();
            if (grid == null)
            {
                EditorGUILayout.HelpBox(
                    "No se encontro el GridLayoutGroup dentro de TrophyShowcaseScrollView.",
                    MessageType.Error);
                return;
            }

            int cardCount = CountTrophyCards(grid.transform);
            EditorGUILayout.HelpBox($"Cards encontradas: {cardCount}", MessageType.Info);
            EditorGUILayout.Space(8);

            // Buttons
            if (GUILayout.Button("All Locked", GUILayout.Height(32)))
            {
                int count = ApplyState(grid.transform, CardState.Locked);
                MarkDirty();
                Debug.Log($"[CardStateToggler] {count} cards -> Locked");
            }

            if (GUILayout.Button("All Unlocked", GUILayout.Height(32)))
            {
                int count = ApplyState(grid.transform, CardState.Unlocked);
                MarkDirty();
                Debug.Log($"[CardStateToggler] {count} cards -> Unlocked");
            }

            if (GUILayout.Button("Mix (Default)", GUILayout.Height(32)))
            {
                int count = ApplyMix(grid.transform);
                MarkDirty();
                Debug.Log($"[CardStateToggler] {count} cards -> Mix (restaurado)");
            }

            if (GUILayout.Button("All In-Progress", GUILayout.Height(32)))
            {
                int count = ApplyState(grid.transform, CardState.InProgress);
                MarkDirty();
                Debug.Log($"[CardStateToggler] {count} cards -> In-Progress");
            }

            if (GUILayout.Button("All Secret", GUILayout.Height(32)))
            {
                int count = ApplyState(grid.transform, CardState.Secret);
                MarkDirty();
                Debug.Log($"[CardStateToggler] {count} cards -> Secret");
            }
        }

        // ==================== STATE ENUM ====================

        private enum CardState
        {
            Locked,
            Unlocked,
            InProgress,
            Secret
        }

        // ==================== GRID FINDING ====================

        private static GridLayoutGroup FindTrophyGrid()
        {
            // Search for TrophyShowcaseScrollView > Viewport > Content (GridLayoutGroup)
            var scrollViews = Object.FindObjectsOfType<ScrollRect>();
            foreach (var sr in scrollViews)
            {
                if (sr.gameObject.name == "TrophyShowcaseScrollView")
                {
                    if (sr.content != null)
                    {
                        var grid = sr.content.GetComponent<GridLayoutGroup>();
                        if (grid != null) return grid;
                    }
                }
            }
            return null;
        }

        private static int CountTrophyCards(Transform gridTransform)
        {
            int count = 0;
            foreach (Transform child in gridTransform)
            {
                if (child.name.StartsWith("Trophy_"))
                    count++;
            }
            return count;
        }

        // ==================== STATE APPLICATION ====================

        private static int ApplyState(Transform gridTransform, CardState state)
        {
            int count = 0;
            foreach (Transform child in gridTransform)
            {
                if (!child.name.StartsWith("Trophy_")) continue;

                Transform container = child.Find("CardContainer");
                if (container == null) continue;

                float randomProgress = Random.Range(0.2f, 0.8f);
                ApplyCardState(container, state, randomProgress);
                EditorUtility.SetDirty(child.gameObject);
                count++;
            }
            return count;
        }

        private static void ApplyCardState(Transform container, CardState state, float progressPercent = 0.5f)
        {
            // Get references
            Image cardBg = container.GetComponent<Image>();
            Outline outline = container.GetComponent<Outline>();
            Transform borderGlowT = container.Find("BorderGlow");
            Transform glassOverlayT = container.Find("GlassOverlay");
            Transform trophyIconT = container.Find("TrophyIcon");
            Transform progressContainerT = container.Find("ProgressContainer");
            Transform titleTextT = container.Find("TitleText");
            Transform completedBadgeT = container.Find("CompletedBadge");
            Transform shineEffectT = container.Find("ShineEffect");

            Image borderGlow = borderGlowT != null ? borderGlowT.GetComponent<Image>() : null;
            Image glassOverlay = glassOverlayT != null ? glassOverlayT.GetComponent<Image>() : null;
            Image trophyIcon = trophyIconT != null ? trophyIconT.GetComponent<Image>() : null;
            TextMeshProUGUI titleText = titleTextT != null ? titleTextT.GetComponent<TextMeshProUGUI>() : null;

            // TrophyIcon children
            Transform trophyShadowT = trophyIconT != null ? trophyIconT.Find("TrophyShadow") : null;
            Transform lockedOverlayT = trophyIconT != null ? trophyIconT.Find("LockedOverlay") : null;
            Transform questionMarkT = trophyIconT != null ? trophyIconT.Find("QuestionMark") : null;

            // Progress children
            Transform progressBgT = progressContainerT != null ? progressContainerT.Find("ProgressBackground") : null;
            Transform progressFillT = progressBgT != null ? progressBgT.Find("ProgressFill") : null;
            Transform progressTextT = progressContainerT != null ? progressContainerT.Find("ProgressText") : null;

            Color glowColor;

            switch (state)
            {
                case CardState.Unlocked:
                    glowColor = TROPHY_UNLOCKED_GLOW;

                    // CardBG tinted
                    if (cardBg) cardBg.color = new Color(
                        0.05f + glowColor.r * 0.05f,
                        0.08f + glowColor.g * 0.05f,
                        0.12f + glowColor.b * 0.05f,
                        0.95f);

                    // Border
                    if (borderGlow) borderGlow.color = glowColor;
                    if (outline) { outline.effectColor = glowColor; outline.effectDistance = new Vector2(2, 2); }

                    // TrophyIcon
                    if (trophyIconT) trophyIconT.gameObject.SetActive(true);
                    if (trophyIcon) trophyIcon.color = Color.white;
                    if (trophyShadowT) trophyShadowT.gameObject.SetActive(true);
                    if (lockedOverlayT) lockedOverlayT.gameObject.SetActive(false);
                    if (questionMarkT) questionMarkT.gameObject.SetActive(false);

                    // Progress
                    if (progressContainerT) progressContainerT.gameObject.SetActive(false);

                    // Title
                    if (titleText) titleText.color = Color.white;

                    // CompletedBadge
                    if (completedBadgeT) completedBadgeT.gameObject.SetActive(true);

                    // Glass
                    if (glassOverlay) glassOverlay.color = new Color(1f, 1f, 1f, 0.1f);
                    break;

                case CardState.Locked:
                    glowColor = LOCKED_BORDER;

                    if (cardBg) cardBg.color = CARD_BG;

                    if (borderGlow) borderGlow.color = glowColor;
                    if (outline) { outline.effectColor = glowColor; outline.effectDistance = new Vector2(1, 1); }

                    if (trophyIconT) trophyIconT.gameObject.SetActive(true);
                    if (trophyIcon) trophyIcon.color = new Color(0.4f, 0.4f, 0.4f, 1f);
                    if (trophyShadowT) trophyShadowT.gameObject.SetActive(false);
                    if (lockedOverlayT) lockedOverlayT.gameObject.SetActive(true);
                    if (questionMarkT) questionMarkT.gameObject.SetActive(false);

                    if (progressContainerT) progressContainerT.gameObject.SetActive(false);

                    if (titleText) titleText.color = TEXT_SECONDARY;

                    if (completedBadgeT) completedBadgeT.gameObject.SetActive(false);

                    if (glassOverlay) glassOverlay.color = new Color(1f, 1f, 1f, 0.05f);
                    break;

                case CardState.InProgress:
                    glowColor = TROPHY_PROGRESS_GLOW;

                    if (cardBg) cardBg.color = CARD_BG;

                    if (borderGlow) borderGlow.color = glowColor;
                    if (outline) { outline.effectColor = glowColor; outline.effectDistance = new Vector2(1, 1); }

                    if (trophyIconT) trophyIconT.gameObject.SetActive(true);
                    if (trophyIcon) trophyIcon.color = new Color(0.4f, 0.4f, 0.4f, 1f);
                    if (trophyShadowT) trophyShadowT.gameObject.SetActive(false);
                    if (lockedOverlayT) lockedOverlayT.gameObject.SetActive(true);
                    if (questionMarkT) questionMarkT.gameObject.SetActive(false);

                    // Progress active with random fill
                    if (progressContainerT) progressContainerT.gameObject.SetActive(true);
                    if (progressFillT != null)
                    {
                        RectTransform fillRT = progressFillT.GetComponent<RectTransform>();
                        if (fillRT != null)
                        {
                            fillRT.anchorMax = new Vector2(progressPercent, 1);
                        }
                    }
                    if (progressTextT != null)
                    {
                        var tmp = progressTextT.GetComponent<TextMeshProUGUI>();
                        if (tmp != null)
                            tmp.text = $"{Mathf.RoundToInt(progressPercent * 100)}%";
                    }

                    if (titleText) titleText.color = TEXT_SECONDARY;

                    if (completedBadgeT) completedBadgeT.gameObject.SetActive(false);

                    if (glassOverlay) glassOverlay.color = new Color(1f, 1f, 1f, 0.05f);
                    break;

                case CardState.Secret:
                    glowColor = TROPHY_SECRET_GLOW;

                    if (cardBg) cardBg.color = CARD_BG;

                    if (borderGlow) borderGlow.color = glowColor;
                    if (outline) { outline.effectColor = glowColor; outline.effectDistance = new Vector2(1, 1); }

                    // TrophyIcon hidden, QuestionMark shown
                    if (trophyIconT) trophyIconT.gameObject.SetActive(false);
                    if (questionMarkT) questionMarkT.gameObject.SetActive(true);

                    if (progressContainerT) progressContainerT.gameObject.SetActive(false);

                    if (titleText)
                    {
                        titleText.text = "???";
                        titleText.color = CAT_SECRET;
                    }

                    if (completedBadgeT) completedBadgeT.gameObject.SetActive(false);

                    if (glassOverlay) glassOverlay.color = new Color(1f, 1f, 1f, 0.05f);
                    break;
            }
        }

        // ==================== MIX (RESTORE ORIGINAL) ====================

        /// <summary>
        /// Restores each card to its original state as defined in CreateTrophyShowcaseGrid.
        /// Data is embedded here to match the builder exactly.
        /// </summary>
        private static int ApplyMix(Transform gridTransform)
        {
            // Original card data: (name, title, progressPercent, isUnlocked, hasProgress, isSecret)
            var cardData = new (string name, string title, int progress, bool unlocked, bool inProgress, bool secret)[]
            {
                // BEGINNER
                ("Trophy_first_game", "First Steps", 0, false, false, false),
                ("Trophy_tutorial_complete", "Apprentice", 0, false, false, false),
                ("Trophy_first_win", "First Victory", 100, true, false, false),
                ("Trophy_profile_complete", "Identity", 0, false, false, false),
                // MASTERY
                ("Trophy_digitrush_master", "Digit Master", 45, false, true, false),
                ("Trophy_flashtap_master", "Lightning Reflexes", 30, false, true, false),
                ("Trophy_memorypairs_master", "Photographic Memory", 0, false, false, false),
                ("Trophy_quickmath_master", "Human Calculator", 20, false, true, false),
                ("Trophy_oddoneout_master", "Eagle Eye", 65, false, true, false),
                // VICTORIES
                ("Trophy_wins_10", "Competitor", 80, false, true, false),
                ("Trophy_wins_50", "Veteran", 40, false, true, false),
                ("Trophy_wins_100", "Centurion", 15, false, true, false),
                ("Trophy_wins_500", "Legend", 5, false, true, false),
                ("Trophy_wins_1000", "Immortal", 0, false, false, false),
                // STREAKS
                ("Trophy_streak_3", "On a Streak", 100, true, false, false),
                ("Trophy_streak_5", "Unstoppable", 60, false, true, false),
                ("Trophy_streak_10", "Domination", 0, false, false, false),
                ("Trophy_streak_20", "Invincible", 0, false, false, true),
                // CASH BATTLE
                ("Trophy_cash_first", "Bettor", 100, true, false, false),
                ("Trophy_cash_first_win", "Real Winner", 100, true, false, false),
                ("Trophy_cash_10_wins", "Serious Player", 50, false, true, false),
                ("Trophy_cash_50_wins", "High Roller", 20, false, true, false),
                ("Trophy_cash_100_wins", "Shark", 5, false, true, false),
                ("Trophy_cash_earnings_100", "First $100", 35, false, true, false),
                ("Trophy_cash_earnings_1000", "Thousand Club", 0, false, false, true),
                // TOURNAMENTS
                ("Trophy_tournament_first", "Participant", 100, true, false, false),
                ("Trophy_tournament_top3", "Podium", 0, false, false, false),
                ("Trophy_tournament_win", "Champion", 0, false, false, false),
                ("Trophy_tournament_5_wins", "Multi-Champion", 0, false, false, false),
                ("Trophy_tournament_create", "Organizer", 0, false, false, false),
                // SOCIAL
                ("Trophy_friend_first", "First Friend", 100, true, false, false),
                ("Trophy_friends_10", "Popular", 70, false, true, false),
                ("Trophy_friends_50", "Influencer", 10, false, true, false),
                ("Trophy_challenge_friend", "Challenger", 0, false, false, false),
                ("Trophy_beat_friend", "Rival", 0, false, false, false),
                // PROGRESSION
                ("Trophy_level_10", "Level 10", 100, true, false, false),
                ("Trophy_level_25", "Level 25", 60, false, true, false),
                ("Trophy_level_50", "Level 50", 30, false, true, false),
                ("Trophy_level_100", "Level 100", 0, false, false, false),
                // TIME
                ("Trophy_days_7", "One Week", 100, true, false, false),
                ("Trophy_days_30", "One Month", 50, false, true, false),
                ("Trophy_days_100", "100 Days", 15, false, true, false),
                ("Trophy_days_365", "One Year", 0, false, false, true),
                ("Trophy_daily_streak_7", "Weekly Streak", 100, true, false, false),
                ("Trophy_daily_streak_30", "Monthly Streak", 25, false, true, false),
                // SECRET
                ("Trophy_night_owl", "Night Owl", 0, false, false, true),
                ("Trophy_perfect_game", "Perfection", 0, false, false, true),
                ("Trophy_comeback_king", "Comeback King", 0, false, false, true),
                ("Trophy_speed_demon", "Speed Demon", 0, false, false, true),
            };

            // Build lookup
            var lookup = new System.Collections.Generic.Dictionary<string,
                (string title, int progress, bool unlocked, bool inProgress, bool secret)>();
            foreach (var d in cardData)
                lookup[d.name] = (d.title, d.progress, d.unlocked, d.inProgress, d.secret);

            int count = 0;
            foreach (Transform child in gridTransform)
            {
                if (!child.name.StartsWith("Trophy_")) continue;

                Transform container = child.Find("CardContainer");
                if (container == null) continue;

                if (lookup.TryGetValue(child.name, out var data))
                {
                    CardState state;
                    if (data.secret)
                        state = CardState.Secret;
                    else if (data.unlocked)
                        state = CardState.Unlocked;
                    else if (data.inProgress)
                        state = CardState.InProgress;
                    else
                        state = CardState.Locked;

                    ApplyCardState(container, state, data.progress / 100f);

                    // Restore original title for non-secret
                    if (state != CardState.Secret)
                    {
                        Transform titleT = container.Find("TitleText");
                        if (titleT != null)
                        {
                            var tmp = titleT.GetComponent<TextMeshProUGUI>();
                            if (tmp != null) tmp.text = data.title;
                        }
                    }
                }
                else
                {
                    // Unknown card, default to locked
                    ApplyCardState(container, CardState.Locked);
                }

                EditorUtility.SetDirty(child.gameObject);
                count++;
            }
            return count;
        }

        // ==================== UTILITY ====================

        private static void MarkDirty()
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}
