using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor window for testing achievements.
    /// Allows unlocking any achievement and triggers the toast notification.
    /// Access via: DigitPark > Tools > Achievement Debug Tester
    /// </summary>
    public class AchievementDebugTester : EditorWindow
    {
        private Vector2 scrollPosition;
        private Dictionary<string, bool> achievementStates = new Dictionary<string, bool>();
        private string searchFilter = "";
        private bool showOnlyLocked = false;
        private bool showOnlyUnlocked = false;

        // Achievement data structure for the tester
        private List<AchievementTestData> allAchievements;

        [MenuItem("DigitPark/Tools/Achievement Debug Tester")]
        public static void ShowWindow()
        {
            var window = GetWindow<AchievementDebugTester>("Achievement Tester");
            window.minSize = new Vector2(450, 600);
            window.Initialize();
        }

        private void Initialize()
        {
            LoadAchievementDefinitions();
            LoadSavedStates();
        }

        private void LoadAchievementDefinitions()
        {
            allAchievements = new List<AchievementTestData>
            {
                // ==================== BEGINNER (4) ====================
                new AchievementTestData("first_game", "Primer Paso", "Completa tu primera partida", "Beginner", 10, "Logro_Primeros_Pasos"),
                new AchievementTestData("tutorial_complete", "Aprendiz", "Completa el tutorial", "Beginner", 10, "Logro_Graduado"),
                new AchievementTestData("first_win", "Primera Victoria", "Gana tu primera partida", "Beginner", 15, "Logro_Primera_Victoria"),
                new AchievementTestData("profile_complete", "Identidad", "Completa tu perfil", "Beginner", 10, "Logro_Perfil_Completo"),

                // ==================== MASTERY (5) ====================
                new AchievementTestData("digitrush_master", "Maestro de Dígitos", "Alcanza 10,000 puntos en DigitRush", "Mastery", 50, "Logro_Maestro_Numeros"),
                new AchievementTestData("flashtap_master", "Reflejos de Luz", "Alcanza 100 taps perfectos en FlashTap", "Mastery", 50, "Logro_Reflejos_Rayo"),
                new AchievementTestData("memorypairs_master", "Memoria Fotográfica", "Completa MemoryPairs sin errores", "Mastery", 50, "Logro_Genio"),
                new AchievementTestData("quickmath_master", "Calculadora Humana", "Resuelve 50 problemas seguidos", "Mastery", 50, "Logro_Maestro_Matematicas"),
                new AchievementTestData("oddoneout_master", "Ojo de Águila", "Encuentra 100 diferencias", "Mastery", 50, "Logro_Ojo_Aguila"),

                // ==================== VICTORIES (5) ====================
                new AchievementTestData("wins_10", "Competidor", "Gana 10 partidas", "Victories", 20, "Logro_10_Victorias"),
                new AchievementTestData("wins_50", "Veterano", "Gana 50 partidas", "Victories", 40, "Logro_50_Victorias"),
                new AchievementTestData("wins_100", "Centurión", "Gana 100 partidas", "Victories", 60, "Logro_Centurion"),
                new AchievementTestData("wins_500", "Leyenda", "Gana 500 partidas", "Victories", 100, "Logro_500_Victorias"),
                new AchievementTestData("wins_1000", "Inmortal", "Gana 1,000 partidas", "Victories", 200, "Logro_1000_Victorias"),

                // ==================== STREAKS (4) ====================
                new AchievementTestData("streak_3", "En Racha", "Gana 3 partidas seguidas", "Streaks", 25, "Logro_Racha_Fuego"),
                new AchievementTestData("streak_5", "Imparable", "Gana 5 partidas seguidas", "Streaks", 40, "Logro_Victoria_Racha_7"),
                new AchievementTestData("streak_10", "Dominación", "Gana 10 partidas seguidas", "Streaks", 75, "Logro_Demoledor"),
                new AchievementTestData("streak_20", "Invencible", "Gana 20 partidas seguidas", "Streaks", 150, "Logro_Victoria_Racha_30", true),

                // ==================== CASH BATTLE (7) ====================
                new AchievementTestData("cash_first", "Apostador", "Completa tu primera Cash Battle", "CashBattle", 25, "Logro_Ficha_Cash"),
                new AchievementTestData("cash_first_win", "Ganador Real", "Gana tu primera Cash Battle", "CashBattle", 35, "Logro_Rey_Monedas"),
                new AchievementTestData("cash_10_wins", "Jugador Serio", "Gana 10 Cash Battles", "CashBattle", 50, "Logro_VIP_1000"),
                new AchievementTestData("cash_50_wins", "High Roller", "Gana 50 Cash Battles", "CashBattle", 100, "Logro_VIP_Dados"),
                new AchievementTestData("cash_100_wins", "Tiburón", "Gana 100 Cash Battles", "CashBattle", 200, "Logro_Tiburon_Cash"),
                new AchievementTestData("cash_earnings_100", "Primeros $100", "Acumula $100 en ganancias", "CashBattle", 75, "Logro_Bolsa_100"),
                new AchievementTestData("cash_earnings_1000", "Club de los Mil", "Acumula $1,000 en ganancias", "CashBattle", 250, "Logro_Millonario", true),

                // ==================== TOURNAMENTS (5) ====================
                new AchievementTestData("tournament_first", "Participante", "Participa en tu primer torneo", "Tournaments", 20, "Logro_Torneo_Bracket"),
                new AchievementTestData("tournament_top3", "Podio", "Termina en Top 3", "Tournaments", 50, "Logro_Coleccion_Trofeos"),
                new AchievementTestData("tournament_win", "Campeón", "Gana un torneo", "Tournaments", 100, "Logro_Campeon_1"),
                new AchievementTestData("tournament_5_wins", "Multicampeón", "Gana 5 torneos", "Tournaments", 200, "Logro_4_Estrellas"),
                new AchievementTestData("tournament_create", "Organizador", "Crea tu primer torneo", "Tournaments", 30, "Logro_Organizador_Torneo"),

                // ==================== SOCIAL (5) ====================
                new AchievementTestData("friend_first", "Primer Amigo", "Añade tu primer amigo", "Social", 15, "Logro_Primer_Rival"),
                new AchievementTestData("friends_10", "Popular", "Tiene 10 amigos", "Social", 30, "Logro_Social_10_Amigos"),
                new AchievementTestData("friends_50", "Influencer", "Tiene 50 amigos", "Social", 75, "Logro_Influencer"),
                new AchievementTestData("challenge_friend", "Retador", "Reta a un amigo", "Social", 20, "Logro_Versus"),
                new AchievementTestData("beat_friend", "Rival", "Vence a un amigo", "Social", 25, "Logro_Amigo_Rival"),

                // ==================== PROGRESSION (8) ====================
                new AchievementTestData("level_10", "Nivel 10", "Alcanza el nivel 10", "Progression", 25, "Logro_Nivel_10"),
                new AchievementTestData("level_25", "Nivel 25", "Alcanza el nivel 25", "Progression", 50, "Logro_Nivel_25"),
                new AchievementTestData("level_50", "Nivel 50", "Alcanza el nivel 50", "Progression", 75, "Logro_Nivel50"),
                new AchievementTestData("level_100", "Nivel 100", "Alcanza el nivel 100", "Progression", 150, "Logro_Avance_Epico"),
                new AchievementTestData("rank_up", "Ascenso", "Sube de rango por primera vez", "Progression", 30, "Logro_Ascenso"),
                new AchievementTestData("rank_elite", "Élite", "Alcanza rango Diamante", "Progression", 100, "Logro_Elite"),
                new AchievementTestData("rank_legend", "Leyenda", "Alcanza rango Leyenda", "Progression", 200, "Logro_Leyenda"),
                new AchievementTestData("rank_immortal", "Inmortal", "Alcanza rango Inmortal", "Progression", 500, "Logro_Inmortal", true),

                // ==================== COLLECTOR ====================
                // Reservado para V2 (sin cofres/battlepass)

                // ==================== TIME (6) ====================
                new AchievementTestData("days_7", "Una Semana", "Juega 7 días", "Time", 25, "Logro_Racha_7_Dias"),
                new AchievementTestData("days_30", "Un Mes", "Juega 30 días", "Time", 50, "Logro_Racha_30_Dias"),
                new AchievementTestData("days_100", "100 Días", "Juega 100 días", "Time", 100, "Logro_Racha_100_Dias"),
                new AchievementTestData("days_365", "Un Año", "Juega 365 días", "Time", 300, "Logro_Racha_365_Dias", true),
                new AchievementTestData("daily_streak_7", "Racha Semanal", "Login 7 días seguidos", "Time", 30, "Logro_Login_Semanal"),
                new AchievementTestData("daily_streak_30", "Racha Mensual", "Login 30 días seguidos", "Time", 75, "Logro_Login_Mensual"),

                // ==================== SECRET (4) ====================
                new AchievementTestData("night_owl", "Búho Nocturno", "Juega a las 3:00 AM", "Secret", 50, "Logro_Buho_Nocturno", true),
                new AchievementTestData("perfect_game", "Perfección", "100% precisión en cualquier juego", "Secret", 100, "Logro_Perfeccionista", true),
                new AchievementTestData("comeback_king", "Rey del Comeback", "Gana perdiendo por 50%+", "Secret", 75, "Logro_Ave_Fenix", true),
                new AchievementTestData("speed_demon", "Demonio de Velocidad", "Completa un juego en <10 segundos", "Secret", 100, "Logro_Demonio_Velocidad", true),
            };
        }

        private void LoadSavedStates()
        {
            achievementStates.Clear();
            foreach (var achievement in allAchievements)
            {
                bool isCompleted = PlayerPrefs.GetInt($"Achievement_{achievement.id}_completed", 0) == 1;
                achievementStates[achievement.id] = isCompleted;
            }
        }

        private void OnGUI()
        {
            if (allAchievements == null)
            {
                Initialize();
            }

            // Header
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("🏆 Achievement Debug Tester", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Marca un logro para desbloquearlo y ver la notificación", EditorStyles.miniLabel);
            EditorGUILayout.Space(5);

            // Runtime warning
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("⚠️ Debes estar en PLAY MODE para ver las notificaciones toast.", MessageType.Warning);
            }

            EditorGUILayout.Space(5);

            // Controls
            EditorGUILayout.BeginHorizontal();
            searchFilter = EditorGUILayout.TextField("Buscar:", searchFilter, GUILayout.Width(300));
            if (GUILayout.Button("Limpiar", GUILayout.Width(70)))
            {
                searchFilter = "";
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            showOnlyLocked = GUILayout.Toggle(showOnlyLocked, "Solo Bloqueados", GUILayout.Width(120));
            showOnlyUnlocked = GUILayout.Toggle(showOnlyUnlocked, "Solo Desbloqueados", GUILayout.Width(140));

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Reset ALL", GUILayout.Width(80)))
            {
                if (EditorUtility.DisplayDialog("Reset Achievements",
                    "¿Resetear TODOS los logros?\nEsto borrará todo el progreso guardado.", "Sí", "No"))
                {
                    ResetAllAchievements();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Stats
            int unlocked = achievementStates.Count(x => x.Value);
            int total = allAchievements.Count;
            EditorGUILayout.LabelField($"Progreso: {unlocked}/{total} ({(unlocked * 100 / total)}%)", EditorStyles.boldLabel);

            EditorGUILayout.Space(5);

            // Scrollable list
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            string currentCategory = "";
            foreach (var achievement in allAchievements)
            {
                // Filter
                if (!string.IsNullOrEmpty(searchFilter))
                {
                    if (!achievement.title.ToLower().Contains(searchFilter.ToLower()) &&
                        !achievement.id.ToLower().Contains(searchFilter.ToLower()))
                        continue;
                }

                bool isUnlocked = achievementStates.ContainsKey(achievement.id) && achievementStates[achievement.id];

                if (showOnlyLocked && isUnlocked) continue;
                if (showOnlyUnlocked && !isUnlocked) continue;

                // Category header
                if (achievement.category != currentCategory)
                {
                    currentCategory = achievement.category;
                    EditorGUILayout.Space(8);

                    Color catColor = GetCategoryColor(currentCategory);
                    GUI.backgroundColor = catColor;
                    EditorGUILayout.BeginVertical("box");
                    GUI.backgroundColor = Color.white;

                    EditorGUILayout.LabelField($"━━━ {currentCategory.ToUpper()} ━━━", EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();
                }

                // Achievement row
                DrawAchievementRow(achievement, isUnlocked);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);

            // Quick actions
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🎯 Unlock Random", GUILayout.Height(30)))
            {
                UnlockRandomAchievement();
            }
            if (GUILayout.Button("📦 Unlock All Beginner", GUILayout.Height(30)))
            {
                UnlockCategory("Beginner");
            }
            if (GUILayout.Button("🔄 Refresh", GUILayout.Height(30)))
            {
                LoadSavedStates();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawAchievementRow(AchievementTestData achievement, bool isUnlocked)
        {
            Color bgColor = isUnlocked ? new Color(0.2f, 0.5f, 0.3f, 0.3f) : new Color(0.3f, 0.3f, 0.3f, 0.2f);
            GUI.backgroundColor = bgColor;

            EditorGUILayout.BeginHorizontal("box");
            GUI.backgroundColor = Color.white;

            // Checkbox
            bool newState = EditorGUILayout.Toggle(isUnlocked, GUILayout.Width(20));

            if (newState != isUnlocked)
            {
                if (newState)
                {
                    UnlockAchievement(achievement);
                }
                else
                {
                    LockAchievement(achievement);
                }
            }

            // Icon preview
            Sprite icon = LoadIconSprite(achievement.iconName);
            if (icon != null)
            {
                GUILayout.Box(icon.texture, GUILayout.Width(32), GUILayout.Height(32));
            }
            else
            {
                GUILayout.Box("?", GUILayout.Width(32), GUILayout.Height(32));
            }

            // Info
            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            string secretTag = achievement.isSecret ? " [SECRET]" : "";
            EditorGUILayout.LabelField($"{achievement.title}{secretTag}", EditorStyles.boldLabel, GUILayout.Width(200));
            EditorGUILayout.LabelField($"{achievement.points} pts", GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(achievement.description, EditorStyles.miniLabel);

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void UnlockAchievement(AchievementTestData achievement)
        {
            // Save to PlayerPrefs
            PlayerPrefs.SetInt($"Achievement_{achievement.id}_completed", 1);
            PlayerPrefs.SetInt($"Achievement_{achievement.id}_progress", 100);
            PlayerPrefs.Save();

            achievementStates[achievement.id] = true;

            Debug.Log($"[AchievementTester] Unlocked: {achievement.title}");

            // Trigger notification if in play mode
            if (Application.isPlaying)
            {
                TriggerNotification(achievement);
            }
        }

        private void LockAchievement(AchievementTestData achievement)
        {
            PlayerPrefs.SetInt($"Achievement_{achievement.id}_completed", 0);
            PlayerPrefs.SetInt($"Achievement_{achievement.id}_progress", 0);
            PlayerPrefs.SetInt($"Achievement_{achievement.id}_claimed", 0);
            PlayerPrefs.Save();

            achievementStates[achievement.id] = false;

            Debug.Log($"[AchievementTester] Locked: {achievement.title}");
        }

        private void TriggerNotification(AchievementTestData achievement)
        {
            // Try to find the notification manager
            var notificationManager = Object.FindObjectOfType<Managers.AchievementNotificationManager>();

            if (notificationManager != null)
            {
                Sprite icon = LoadIconSprite(achievement.iconName);
                notificationManager.ShowNotification(
                    achievement.title,
                    achievement.description,
                    achievement.points,
                    icon,
                    achievement.isSecret
                );
                Debug.Log($"[AchievementTester] Notification triggered for: {achievement.title}");
            }
            else
            {
                Debug.LogWarning("[AchievementTester] AchievementNotificationManager not found! Make sure it's initialized.");
            }
        }

        private Sprite LoadIconSprite(string iconName)
        {
            if (string.IsNullOrEmpty(iconName)) return null;

            // Try Resources path
            Sprite sprite = Resources.Load<Sprite>($"Icons/Achievements/{iconName}");
            if (sprite != null) return sprite;

            // Try AssetDatabase path
            string assetPath = $"Assets/_Project/Resources/Icons/Achievements/{iconName}.png";
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite != null) return sprite;

            // Try Art folder
            assetPath = $"Assets/_Project/Art/Icons/Achievements/{iconName}.png";
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);

            return sprite;
        }

        private void ResetAllAchievements()
        {
            foreach (var achievement in allAchievements)
            {
                PlayerPrefs.DeleteKey($"Achievement_{achievement.id}_completed");
                PlayerPrefs.DeleteKey($"Achievement_{achievement.id}_progress");
                PlayerPrefs.DeleteKey($"Achievement_{achievement.id}_claimed");
            }
            PlayerPrefs.Save();
            LoadSavedStates();
            Debug.Log("[AchievementTester] All achievements reset!");
        }

        private void UnlockRandomAchievement()
        {
            var locked = allAchievements.Where(a => !achievementStates.ContainsKey(a.id) || !achievementStates[a.id]).ToList();
            if (locked.Count > 0)
            {
                var random = locked[Random.Range(0, locked.Count)];
                UnlockAchievement(random);
            }
            else
            {
                Debug.Log("[AchievementTester] All achievements already unlocked!");
            }
        }

        private void UnlockCategory(string category)
        {
            var categoryAchievements = allAchievements.Where(a => a.category == category).ToList();
            foreach (var achievement in categoryAchievements)
            {
                if (!achievementStates.ContainsKey(achievement.id) || !achievementStates[achievement.id])
                {
                    UnlockAchievement(achievement);
                }
            }
        }

        private Color GetCategoryColor(string category)
        {
            return category switch
            {
                "Beginner" => new Color(0.3f, 0.7f, 1f, 0.5f),
                "Mastery" => new Color(0.5f, 1f, 0.5f, 0.5f),
                "Victories" => new Color(0.2f, 0.8f, 0.4f, 0.5f),
                "Streaks" => new Color(1f, 0.5f, 0.2f, 0.5f),
                "CashBattle" => new Color(0.4f, 0.8f, 0.2f, 0.5f),
                "Tournaments" => new Color(1f, 0.6f, 0.2f, 0.5f),
                "Social" => new Color(0.4f, 0.6f, 1f, 0.5f),
                "Progression" => new Color(0.8f, 0.6f, 1f, 0.5f),
                "Collector" => new Color(1f, 0.7f, 0.3f, 0.5f),
                "Time" => new Color(0.5f, 0.8f, 0.9f, 0.5f),
                "Secret" => new Color(0.8f, 0.2f, 1f, 0.5f),
                _ => Color.gray
            };
        }

        // Data structure for test achievements
        private class AchievementTestData
        {
            public string id;
            public string title;
            public string description;
            public string category;
            public int points;
            public string iconName;
            public bool isSecret;

            public AchievementTestData(string id, string title, string description, string category, int points, string iconName, bool isSecret = false)
            {
                this.id = id;
                this.title = title;
                this.description = description;
                this.category = category;
                this.points = points;
                this.iconName = iconName;
                this.isSecret = isSecret;
            }
        }
    }
}
