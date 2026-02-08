using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DigitPark.Editor.AutoAssigners
{
    /// <summary>
    /// Editor tool to assign achievement icons to achievements.
    /// Maps 53 achievement IDs to their corresponding icon files.
    /// </summary>
    public class AchievementIconAssigner : EditorWindow
    {
        private Vector2 scrollPosition;
        private Dictionary<string, string> iconMapping;
        private Dictionary<string, Sprite> loadedSprites = new Dictionary<string, Sprite>();
        private List<string> missingIcons = new List<string>();
        private List<string> unusedIcons = new List<string>();

        private const string ICONS_PATH = "Assets/_Project/Art/Icons/Achievements";

        [MenuItem("DigitPark/Auto Assigners/Icons/Monetization/Achievement Icons", false, 143)]
        public static void ShowWindow()
        {
            var window = GetWindow<AchievementIconAssigner>("Achievement Icon Assigner");
            window.minSize = new Vector2(600, 700);
            window.Initialize();
        }

        private void Initialize()
        {
            BuildIconMapping();
            AnalyzeMapping();
            LoadSprites();
        }

        private void BuildIconMapping()
        {
            // Complete mapping of 53 achievement IDs to icon file names
            iconMapping = new Dictionary<string, string>
            {
                // ==================== BEGINNER (4) ====================
                { "first_game", "Logro_Primeros_Pasos" },
                { "tutorial_complete", "Logro_Graduado" },
                { "first_win", "Logro_Primera_Victoria" },
                { "profile_complete", "Logro_Perfil_Completo" },

                // ==================== MASTERY (5) ====================
                { "digitrush_master", "Logro_Maestro_Numeros" },
                { "flashtap_master", "Logro_Reflejos_Rayo" },
                { "memorypairs_master", "Logro_Genio" },
                { "quickmath_master", "Logro_Maestro_Matematicas" },
                { "oddoneout_master", "Logro_Ojo_Aguila" },

                // ==================== VICTORIES (5) ====================
                { "wins_10", "Logro_10_Victorias" },
                { "wins_50", "Logro_50_Victorias" },
                { "wins_100", "Logro_Centurion" },
                { "wins_500", "Logro_500_Victorias" },
                { "wins_1000", "Logro_1000_Victorias" },

                // ==================== STREAKS (4) ====================
                { "streak_3", "Logro_Racha_Fuego" },
                { "streak_5", "Logro_Victoria_Racha_7" },      // Closest match
                { "streak_10", "Logro_Demoledor" },
                { "streak_20", "Logro_Victoria_Racha_30" },    // Closest match (SECRET)

                // ==================== CASH BATTLE (7) ====================
                { "cash_first", "Logro_Ficha_Cash" },
                { "cash_first_win", "Logro_Rey_Monedas" },
                { "cash_10_wins", "Logro_VIP_1000" },          // Reused - needs specific icon
                { "cash_50_wins", "Logro_VIP_Dados" },
                { "cash_100_wins", "Logro_Tiburon_Cash" },
                { "cash_earnings_100", "Logro_Bolsa_100" },
                { "cash_earnings_1000", "Logro_Millonario" },  // SECRET

                // ==================== TOURNAMENTS (5) ====================
                { "tournament_first", "Logro_Torneo_Bracket" },
                { "tournament_top3", "Logro_Coleccion_Trofeos" },
                { "tournament_win", "Logro_Campeon_1" },
                { "tournament_5_wins", "Logro_4_Estrellas" },  // Multi-champion
                { "tournament_create", "Logro_Organizador_Torneo" },

                // ==================== SOCIAL (5) ====================
                { "friend_first", "Logro_Primer_Rival" },
                { "friends_10", "Logro_Social_10_Amigos" },
                { "friends_50", "Logro_Influencer" },
                { "challenge_friend", "Logro_Versus" },
                { "beat_friend", "Logro_Amigo_Rival" },

                // ==================== PROGRESSION (8) ====================
                { "level_10", "Logro_Nivel_10" },
                { "level_25", "Logro_Nivel_25" },
                { "level_50", "Logro_Nivel50" },
                { "level_100", "Logro_Avance_Epico" },

                // ==================== COLLECTOR ====================
                // Reservado para V2

                // ==================== TIME (6) ====================
                { "days_7", "Logro_Racha_7_Dias" },
                { "days_30", "Logro_Racha_30_Dias" },
                { "days_100", "Logro_Racha_100_Dias" },
                { "days_365", "Logro_Racha_365_Dias" },        // SECRET
                { "daily_streak_7", "Logro_Login_Semanal" },   // Consecutive login
                { "daily_streak_30", "Logro_Login_Mensual" },  // Consecutive login

                // ==================== SECRET (4) ====================
                { "night_owl", "Logro_Buho_Nocturno" },
                { "perfect_game", "Logro_Perfeccionista" },
                { "comeback_king", "Logro_Ave_Fenix" },
                { "speed_demon", "Logro_Demonio_Velocidad" },
            };
        }

        private void AnalyzeMapping()
        {
            missingIcons.Clear();
            unusedIcons.Clear();

            // Find achievements without icons
            foreach (var kvp in iconMapping)
            {
                if (string.IsNullOrEmpty(kvp.Value))
                {
                    missingIcons.Add(kvp.Key);
                }
            }

            // Find icons not used by any achievement
            if (Directory.Exists(ICONS_PATH))
            {
                var iconFiles = Directory.GetFiles(ICONS_PATH, "*.png")
                    .Select(f => Path.GetFileNameWithoutExtension(f))
                    .ToList();

                var usedIcons = iconMapping.Values.Where(v => !string.IsNullOrEmpty(v)).ToHashSet();

                foreach (var iconFile in iconFiles)
                {
                    if (!usedIcons.Contains(iconFile))
                    {
                        unusedIcons.Add(iconFile);
                    }
                }
            }
        }

        private void LoadSprites()
        {
            loadedSprites.Clear();

            foreach (var kvp in iconMapping)
            {
                if (!string.IsNullOrEmpty(kvp.Value))
                {
                    string path = $"{ICONS_PATH}/{kvp.Value}.png";
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (sprite != null)
                    {
                        loadedSprites[kvp.Key] = sprite;
                    }
                }
            }
        }

        private void OnGUI()
        {
            if (iconMapping == null)
            {
                Initialize();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Achievement Icon Assigner", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"53 Achievements (V1) | {loadedSprites.Count} Icons | {missingIcons.Count} Missing", EditorStyles.miniLabel);
            EditorGUILayout.Space(10);

            // Summary boxes
            EditorGUILayout.BeginHorizontal();

            // Missing icons box
            GUI.backgroundColor = missingIcons.Count > 0 ? new Color(1f, 0.5f, 0.5f) : Color.green;
            EditorGUILayout.BeginVertical("box", GUILayout.Width(280));
            EditorGUILayout.LabelField($"MISSING ICONS ({missingIcons.Count})", EditorStyles.boldLabel);
            foreach (var missing in missingIcons)
            {
                EditorGUILayout.LabelField($"  • {missing}", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndVertical();

            // Unused icons box
            GUI.backgroundColor = unusedIcons.Count > 0 ? new Color(1f, 0.9f, 0.5f) : Color.green;
            EditorGUILayout.BeginVertical("box", GUILayout.Width(280));
            EditorGUILayout.LabelField($"UNUSED ICONS ({unusedIcons.Count})", EditorStyles.boldLabel);
            foreach (var unused in unusedIcons)
            {
                EditorGUILayout.LabelField($"  • {unused}", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndVertical();

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh", GUILayout.Height(30)))
            {
                Initialize();
            }
            if (GUILayout.Button("Apply to AchievementsManager", GUILayout.Height(30)))
            {
                ApplyMappingToManager();
            }
            if (GUILayout.Button("Export Missing Icons List", GUILayout.Height(30)))
            {
                ExportMissingIconsList();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Full Mapping:", EditorStyles.boldLabel);

            // Scrollable mapping list
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            string currentCategory = "";
            foreach (var kvp in iconMapping)
            {
                string category = GetCategoryFromId(kvp.Key);
                if (category != currentCategory)
                {
                    currentCategory = category;
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField($"━━━ {category.ToUpper()} ━━━", EditorStyles.boldLabel);
                }

                EditorGUILayout.BeginHorizontal();

                // Achievement ID
                EditorGUILayout.LabelField(kvp.Key, GUILayout.Width(180));

                // Arrow
                EditorGUILayout.LabelField("→", GUILayout.Width(20));

                // Icon name or MISSING
                if (string.IsNullOrEmpty(kvp.Value))
                {
                    GUI.color = Color.red;
                    EditorGUILayout.LabelField("MISSING", GUILayout.Width(200));
                    GUI.color = Color.white;
                }
                else
                {
                    EditorGUILayout.LabelField(kvp.Value, GUILayout.Width(200));
                }

                // Preview sprite
                if (loadedSprites.TryGetValue(kvp.Key, out var sprite))
                {
                    GUILayout.Box(sprite.texture, GUILayout.Width(32), GUILayout.Height(32));
                }
                else
                {
                    GUILayout.Box("?", GUILayout.Width(32), GUILayout.Height(32));
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private string GetCategoryFromId(string id)
        {
            if (id.StartsWith("first_") || id == "tutorial_complete" || id == "profile_complete") return "Beginner";
            if (id.EndsWith("_master")) return "Mastery";
            if (id.StartsWith("wins_")) return "Victories";
            if (id.StartsWith("streak_")) return "Streaks";
            if (id.StartsWith("cash_")) return "Cash Battle";
            if (id.StartsWith("tournament_")) return "Tournaments";
            if (id.StartsWith("friend") || id == "challenge_friend" || id == "beat_friend") return "Social";
            if (id.StartsWith("level_") || id.StartsWith("rank_")) return "Progression";
            // Collector category reserved for V2
            if (id.StartsWith("days_") || id.StartsWith("daily_")) return "Time";
            if (id == "night_owl" || id == "perfect_game" || id == "comeback_king" || id == "speed_demon") return "Secret";
            return "Other";
        }

        private void ApplyMappingToManager()
        {
            // Find AchievementsManager script
            string managerPath = "Assets/_Project/Scripts/Managers/Monetization/AchievementsManager.cs";

            if (!File.Exists(managerPath))
            {
                EditorUtility.DisplayDialog("Error", "AchievementsManager.cs not found!", "OK");
                return;
            }

            string content = File.ReadAllText(managerPath);
            int changesCount = 0;

            // Replace iconName values in AchievementDefinition constructors
            foreach (var kvp in iconMapping)
            {
                if (string.IsNullOrEmpty(kvp.Value)) continue;

                // Pattern: new AchievementDefinition("achievement_id", ..., "old_icon_name")
                // or: new AchievementDefinition("achievement_id", ..., "old_icon_name", true)

                string oldPattern1 = $"\"{kvp.Key}\", true)";
                string oldPattern2 = $"\"{kvp.Key}\")";

                if (content.Contains(oldPattern1))
                {
                    content = content.Replace(oldPattern1, $"\"{kvp.Value}\", true)");
                    changesCount++;
                }
                else if (content.Contains(oldPattern2))
                {
                    content = content.Replace(oldPattern2, $"\"{kvp.Value}\")");
                    changesCount++;
                }
            }

            if (changesCount > 0)
            {
                File.WriteAllText(managerPath, content);
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Success", $"Updated {changesCount} icon references in AchievementsManager.cs", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Info", "No changes needed - icon names already match or need manual update.", "OK");
            }
        }

        private void ExportMissingIconsList()
        {
            string report = "=== MISSING ACHIEVEMENT ICONS ===\n\n";
            report += "The following achievements need icons to be created:\n\n";

            var achievementDetails = new Dictionary<string, (string title, string description)>
            {
                { "daily_streak_7", ("Racha Semanal", "Login 7 días seguidos") },
                { "daily_streak_30", ("Racha Mensual", "Login 30 días seguidos") },
            };

            int i = 1;
            foreach (var missing in missingIcons)
            {
                if (achievementDetails.TryGetValue(missing, out var details))
                {
                    report += $"{i}. {missing}\n";
                    report += $"   Título: {details.title}\n";
                    report += $"   Descripción: {details.description}\n";
                    report += $"   Archivo sugerido: Logro_{details.title.Replace(" ", "_")}.png\n\n";
                    i++;
                }
            }

            report += "\n=== SUGGESTED ICON CONCEPTS ===\n\n";
            report += "1. daily_streak_7 (Racha Semanal): Calendar with 7 checkmarks and flame\n";
            report += "2. daily_streak_30 (Racha Mensual): Calendar with flame and 30 badge\n";

            string path = "Assets/_Project/Docs/MissingAchievementIcons.txt";
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, report);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Exported", $"Missing icons list saved to:\n{path}", "OK");
            EditorUtility.RevealInFinder(path);
        }

        /// <summary>
        /// Get the icon sprite for an achievement ID at runtime
        /// </summary>
        public static Sprite GetIconForAchievement(string achievementId)
        {
            var mapping = GetStaticMapping();

            if (mapping.TryGetValue(achievementId, out string iconName) && !string.IsNullOrEmpty(iconName))
            {
                return Resources.Load<Sprite>($"Icons/Achievements/{iconName}");
            }

            return null;
        }

        /// <summary>
        /// Static mapping for runtime use (53 achievements V1)
        /// </summary>
        public static Dictionary<string, string> GetStaticMapping()
        {
            return new Dictionary<string, string>
            {
                // BEGINNER
                { "first_game", "Logro_Primeros_Pasos" },
                { "tutorial_complete", "Logro_Graduado" },
                { "first_win", "Logro_Primera_Victoria" },
                { "profile_complete", "Logro_Perfil_Completo" },
                // MASTERY
                { "digitrush_master", "Logro_Maestro_Numeros" },
                { "flashtap_master", "Logro_Reflejos_Rayo" },
                { "memorypairs_master", "Logro_Genio" },
                { "quickmath_master", "Logro_Maestro_Matematicas" },
                { "oddoneout_master", "Logro_Ojo_Aguila" },
                // VICTORIES
                { "wins_10", "Logro_10_Victorias" },
                { "wins_50", "Logro_50_Victorias" },
                { "wins_100", "Logro_Centurion" },
                { "wins_500", "Logro_500_Victorias" },
                { "wins_1000", "Logro_1000_Victorias" },
                // STREAKS
                { "streak_3", "Logro_Racha_Fuego" },
                { "streak_5", "Logro_Victoria_Racha_7" },
                { "streak_10", "Logro_Demoledor" },
                { "streak_20", "Logro_Victoria_Racha_30" },
                // CASH BATTLE
                { "cash_first", "Logro_Ficha_Cash" },
                { "cash_first_win", "Logro_Rey_Monedas" },
                { "cash_10_wins", "Logro_VIP_1000" },
                { "cash_50_wins", "Logro_VIP_Dados" },
                { "cash_100_wins", "Logro_Tiburon_Cash" },
                { "cash_earnings_100", "Logro_Bolsa_100" },
                { "cash_earnings_1000", "Logro_Millonario" },
                // TOURNAMENTS
                { "tournament_first", "Logro_Torneo_Bracket" },
                { "tournament_top3", "Logro_Coleccion_Trofeos" },
                { "tournament_win", "Logro_Campeon_1" },
                { "tournament_5_wins", "Logro_4_Estrellas" },
                { "tournament_create", "Logro_Organizador_Torneo" },
                // SOCIAL
                { "friend_first", "Logro_Primer_Rival" },
                { "friends_10", "Logro_Social_10_Amigos" },
                { "friends_50", "Logro_Influencer" },
                { "challenge_friend", "Logro_Versus" },
                { "beat_friend", "Logro_Amigo_Rival" },
                // PROGRESSION
                { "level_10", "Logro_Nivel_10" },
                { "level_25", "Logro_Nivel_25" },
                { "level_50", "Logro_Nivel50" },
                { "level_100", "Logro_Avance_Epico" },
                // COLLECTOR - Reservado para V2
                // TIME
                { "days_7", "Logro_Racha_7_Dias" },
                { "days_30", "Logro_Racha_30_Dias" },
                { "days_100", "Logro_Racha_100_Dias" },
                { "days_365", "Logro_Racha_365_Dias" },
                { "daily_streak_7", "Logro_Login_Semanal" },
                { "daily_streak_30", "Logro_Login_Mensual" },
                // SECRET
                { "night_owl", "Logro_Buho_Nocturno" },
                { "perfect_game", "Logro_Perfeccionista" },
                { "comeback_king", "Logro_Ave_Fenix" },
                { "speed_demon", "Logro_Demonio_Velocidad" },
            };
        }
    }
}
