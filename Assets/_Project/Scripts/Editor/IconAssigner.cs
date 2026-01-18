using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace DigitPark.Editor
{
    /// <summary>
    /// Herramienta para gestionar y asignar iconos automáticamente.
    /// Incluye inventario de iconos, detección de iconos faltantes,
    /// y asignación automática a componentes UI.
    /// </summary>
    public class IconAssigner : EditorWindow
    {
        private static readonly string ICONS_PATH = "Assets/_Project/Art/Icons";

        private Vector2 scrollPos;
        private Dictionary<string, Sprite> allIcons = new Dictionary<string, Sprite>();
        private List<string> missingIcons = new List<string>();
        private int selectedTab = 0;
        private string[] tabs = { "Inventory", "Missing", "Assign" };

        [MenuItem("DigitPark/Tools/Icon Manager", false, 3)]
        public static void ShowWindow()
        {
            var window = GetWindow<IconAssigner>("Icon Manager");
            window.minSize = new Vector2(450, 500);
        }

        private void OnEnable()
        {
            LoadAllIcons();
            DetectMissingIcons();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("DigitPark Icon Manager", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // Tabs
            selectedTab = GUILayout.Toolbar(selectedTab, tabs);
            EditorGUILayout.Space(10);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            switch (selectedTab)
            {
                case 0:
                    DrawInventoryTab();
                    break;
                case 1:
                    DrawMissingTab();
                    break;
                case 2:
                    DrawAssignTab();
                    break;
            }

            EditorGUILayout.EndScrollView();
        }

        #region Inventory Tab

        private void DrawInventoryTab()
        {
            if (GUILayout.Button("Refresh Icons", GUILayout.Height(30)))
            {
                LoadAllIcons();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField($"Total Icons: {allIcons.Count}", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            var categories = new Dictionary<string, List<string>>
            {
                { "Achievements", allIcons.Keys.Where(k => k.Contains("achievement")).OrderBy(k => k).ToList() },
                { "BattlePass", allIcons.Keys.Where(k => k.Contains("battlepass")).OrderBy(k => k).ToList() },
                { "Chests", allIcons.Keys.Where(k => k.Contains("chest")).OrderBy(k => k).ToList() },
                { "Currency (Coins/Gems/XP)", allIcons.Keys.Where(k => k.Contains("coin") || k.Contains("gem") || k == "icon_xp").OrderBy(k => k).ToList() },
                { "Daily Rewards", allIcons.Keys.Where(k => k.Contains("daily")).OrderBy(k => k).ToList() },
                { "Missions", allIcons.Keys.Where(k => k.Contains("mission")).OrderBy(k => k).ToList() },
                { "UI Elements", allIcons.Keys.Where(k => k.StartsWith("icon_ui")).OrderBy(k => k).ToList() },
                { "Games", allIcons.Keys.Where(k => k.Contains("icon") && (k.Contains("rush") || k.Contains("tap") || k.Contains("math") || k.Contains("memory") || k.Contains("odd"))).OrderBy(k => k).ToList() },
                { "Other", allIcons.Keys.Where(k => !k.Contains("achievement") && !k.Contains("battlepass") && !k.Contains("chest") && !k.Contains("coin") && !k.Contains("gem") && !k.Contains("daily") && !k.Contains("mission") && !k.StartsWith("icon_ui") && k != "icon_xp").OrderBy(k => k).ToList() },
            };

            foreach (var category in categories)
            {
                if (category.Value.Count == 0) continue;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"{category.Key} ({category.Value.Count})", EditorStyles.boldLabel);

                foreach (var iconName in category.Value)
                {
                    EditorGUILayout.BeginHorizontal();

                    if (allIcons.TryGetValue(iconName, out Sprite sprite) && sprite != null)
                    {
                        GUILayout.Label(AssetPreview.GetAssetPreview(sprite.texture), GUILayout.Width(32), GUILayout.Height(32));
                    }
                    else
                    {
                        GUILayout.Label("?", GUILayout.Width(32), GUILayout.Height(32));
                    }

                    EditorGUILayout.LabelField(iconName);
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
        }

        #endregion

        #region Missing Tab

        private void DrawMissingTab()
        {
            if (GUILayout.Button("Detect Missing Icons", GUILayout.Height(30)))
            {
                DetectMissingIcons();
            }

            EditorGUILayout.Space(10);

            if (missingIcons.Count == 0)
            {
                EditorGUILayout.HelpBox("¡No faltan iconos críticos!", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox($"Faltan {missingIcons.Count} iconos que deberías generar:", MessageType.Warning);
                EditorGUILayout.Space(5);

                foreach (var icon in missingIcons)
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    EditorGUILayout.LabelField("❌", GUILayout.Width(20));
                    EditorGUILayout.LabelField(icon);
                    if (GUILayout.Button("Copy Name", GUILayout.Width(80)))
                    {
                        EditorGUIUtility.systemCopyBuffer = icon;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Prompt para generar iconos faltantes:", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(GenerateMissingIconsPrompt(), MessageType.None);

                if (GUILayout.Button("Copy Prompt to Clipboard"))
                {
                    EditorGUIUtility.systemCopyBuffer = GenerateMissingIconsPrompt();
                    Debug.Log("Prompt copiado al clipboard");
                }
            }
        }

        private void DetectMissingIcons()
        {
            missingIcons.Clear();

            // Iconos que deberían existir
            var expectedIcons = new List<string>
            {
                // Daily Rewards (días 1-7)
                "icon_daily_reward_day1",
                "icon_daily_reward_day2",
                "icon_daily_reward_day3",
                "icon_daily_reward_day4",
                "icon_daily_reward_day5",
                "icon_daily_reward_day6",
                "icon_daily_reward_day7",

                // Chests (todos deberían tener open y closed)
                "icon_chest_common_closed", "icon_chest_common_open",
                "icon_chest_rare_closed", "icon_chest_rare_open",
                "icon_chest_epic_closed", "icon_chest_epic_open",
                "icon_chest_legendary_closed", "icon_chest_legendary_open",
                "icon_chest_mythic_closed", "icon_chest_mythic_open",

                // Currency singles
                "icon_coin_single",
                "icon_gem_single",
            };

            foreach (var expected in expectedIcons)
            {
                if (!allIcons.ContainsKey(expected.ToLower()))
                {
                    missingIcons.Add(expected);
                }
            }
        }

        private string GenerateMissingIconsPrompt()
        {
            if (missingIcons.Count == 0) return "";

            string prompt = "Generate the following game UI icons in the same style as the existing icons:\n\n";
            foreach (var icon in missingIcons)
            {
                prompt += $"- {icon.Replace("icon_", "").Replace("_", " ")}\n";
            }
            prompt += "\nStyle: Modern, colorful, suitable for mobile gaming. PNG with transparent background, 512x512.";
            return prompt;
        }

        #endregion

        #region Assign Tab

        private void DrawAssignTab()
        {
            EditorGUILayout.LabelField("Quick Assign Actions", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Scene Components", EditorStyles.miniBoldLabel);

            if (GUILayout.Button("Assign Bottom NavBar Icons"))
            {
                AssignBottomNavBarIcons();
            }

            if (GUILayout.Button("Assign Currency Display Icons"))
            {
                AssignCurrencyDisplayIcons();
            }

            if (GUILayout.Button("Assign All Scene Icons"))
            {
                AssignAllSceneIcons();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Texture Settings", EditorStyles.miniBoldLabel);

            if (GUILayout.Button("Configure All Icons as Sprites"))
            {
                ConfigureAllIconsAsSprites();
            }
            EditorGUILayout.EndVertical();
        }

        #endregion

        #region Icon Loading

        private void LoadAllIcons()
        {
            allIcons.Clear();

            // Buscar todos los sprites en la carpeta de iconos
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { ICONS_PATH });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                if (sprite != null)
                {
                    string key = Path.GetFileNameWithoutExtension(path).ToLower();
                    allIcons[key] = sprite;
                }
            }

            // También cargar texturas que aún no son sprites
            string[] texGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { ICONS_PATH });
            foreach (string guid in texGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string key = Path.GetFileNameWithoutExtension(path).ToLower();

                if (!allIcons.ContainsKey(key))
                {
                    // Es una textura sin sprite, agregarla como null para tracking
                    allIcons[key] = null;
                }
            }

            Debug.Log($"[IconManager] Loaded {allIcons.Count} icons");
        }

        #endregion

        #region Assignment Methods

        [MenuItem("DigitPark/Tools/Icons/Assign All Icons in Scene", false, 4)]
        public static void AssignAllSceneIcons()
        {
            int total = 0;
            total += AssignBottomNavBarIcons();
            total += AssignCurrencyDisplayIcons();
            total += AssignCompactCurrencyIcons();
            total += AssignDailyRewardsIcons();

            if (total > 0)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                    UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }

            Debug.Log($"[IconManager] Assigned {total} icons");
            EditorUtility.DisplayDialog("Icon Manager", $"Se asignaron {total} iconos.", "OK");
        }

        [MenuItem("DigitPark/Tools/Icons/Assign Daily Rewards Icons", false, 6)]
        public static int AssignDailyRewardsIcons()
        {
            int count = 0;

            Debug.Log("[IconManager] ========== ASSIGNING DAILY REWARDS ICONS ==========");

            // First, ensure all icons are configured as sprites
            ConfigureAllIconsAsSprites();

            // Find all Image components in the scene that need icons
            var allImages = Object.FindObjectsOfType<Image>(true);
            Debug.Log($"[IconManager] Found {allImages.Length} Image components in scene");

            foreach (var img in allImages)
            {
                string objName = img.gameObject.name;
                string parentName = img.transform.parent?.name ?? "";
                string grandparentName = img.transform.parent?.parent?.name ?? "";
                string fullPath = GetFullPath(img.transform);

                // ===== CHESTS =====

                // Day 7 chest (in Day7Special) - Check this FIRST to avoid conflict
                if (objName == "Chest" && fullPath.Contains("Day7"))
                {
                    Debug.Log($"[IconManager] Found Day7 chest at: {fullPath}");
                    count += TryAssignSprite(img, "icon_chest_legendary_closed");
                    continue;
                }

                // Today's reward chest (TodayRewardPanel/Chest)
                if (objName == "Chest" && (parentName == "TodayRewardPanel" || fullPath.Contains("TodayReward")))
                {
                    Debug.Log($"[IconManager] Found TodayReward chest at: {fullPath}");
                    count += TryAssignSprite(img, "icon_chest_rare_closed");
                    continue;
                }

                // ClaimCelebration chest
                if (objName == "Chest" && fullPath.Contains("ClaimCelebration"))
                {
                    Debug.Log($"[IconManager] Found ClaimCelebration chest at: {fullPath}");
                    count += TryAssignSprite(img, "icon_chest_rare_open");
                    continue;
                }

                // Day 1-6 chests (by parent name pattern: Day1/ChestContainer/Chest)
                if (objName == "Chest" && parentName == "ChestContainer")
                {
                    string dayName = grandparentName;
                    string iconName = GetChestIconForDay(dayName);
                    if (!string.IsNullOrEmpty(iconName))
                    {
                        Debug.Log($"[IconManager] Found {dayName} chest at: {fullPath}, assigning: {iconName}");
                        count += TryAssignSprite(img, iconName);
                    }
                    continue;
                }

                // ===== ICONS =====

                // Fire icon (streak)
                if (objName == "FireIcon" || (objName == "Fire" && fullPath.Contains("Streak")))
                {
                    Debug.Log($"[IconManager] Found Fire/Streak icon at: {fullPath}");
                    count += TryAssignSprite(img, "icon_daily_streak");
                    continue;
                }

                // Currency icons in header
                if (objName == "Icon" && parentName == "Coins")
                {
                    count += TryAssignSprite(img, "icon_coin");
                    continue;
                }
                if (objName == "Icon" && parentName == "Gems")
                {
                    count += TryAssignSprite(img, "icon_gem");
                    continue;
                }

                // Gem icon (in streak lost popup offer)
                if (objName == "Gem" && fullPath.Contains("Offer"))
                {
                    count += TryAssignSprite(img, "icon_gem");
                    continue;
                }

                // Clock/Timer icon
                if (objName == "ClockIcon")
                {
                    count += TryAssignSprite(img, "icon_ui_timer");
                    continue;
                }

                // Coin/XP icons in celebration
                if (parentName == "Coins" && objName == "Icon" && fullPath.Contains("Celebration"))
                {
                    count += TryAssignSprite(img, "icon_coin");
                    continue;
                }
            }

            if (count > 0)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                    UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }

            Debug.Log($"[IconManager] ========== ASSIGNED {count} DAILY REWARDS ICONS ==========");
            return count;
        }

        private static string GetChestIconForDay(string dayName)
        {
            switch (dayName)
            {
                case "Day1":
                case "Day2":
                case "Day4":
                    return "icon_chest_common_closed";
                case "Day3":
                case "Day6":
                    return "icon_chest_rare_closed";
                case "Day5":
                    return "icon_chest_epic_closed";
                default:
                    return null;
            }
        }

        [MenuItem("DigitPark/Tools/Icons/Diagnose Daily Rewards Scene", false, 7)]
        public static void DiagnoseDailyRewardsScene()
        {
            Debug.Log("[IconManager] ========== DIAGNOSING DAILY REWARDS SCENE ==========");

            var allImages = Object.FindObjectsOfType<Image>(true);
            Debug.Log($"[IconManager] Total Image components in scene: {allImages.Length}");

            var candidates = new List<string>();

            foreach (var img in allImages)
            {
                string objName = img.gameObject.name;
                string fullPath = GetFullPath(img.transform);

                // Check for potential Daily Rewards targets
                if (objName == "Chest" ||
                    objName == "FireIcon" || objName == "Fire" ||
                    objName == "ClockIcon" ||
                    (objName == "Icon" && (fullPath.Contains("Coin") || fullPath.Contains("Gem"))) ||
                    objName == "Gem")
                {
                    string hasSprite = img.sprite != null ? $"[HAS: {img.sprite.name}]" : "[NO SPRITE]";
                    candidates.Add($"  {fullPath} {hasSprite}");
                }
            }

            if (candidates.Count == 0)
            {
                Debug.LogWarning("[IconManager] No Daily Rewards icon candidates found! Make sure the scene is built.");
            }
            else
            {
                Debug.Log($"[IconManager] Found {candidates.Count} icon candidates:");
                foreach (var c in candidates)
                {
                    Debug.Log(c);
                }
            }

            // Also check available icons
            Debug.Log("\n[IconManager] ========== AVAILABLE CHEST ICONS ==========");
            string[] chestIcons = new[] {
                "icon_chest_common_closed", "icon_chest_common_open",
                "icon_chest_rare_closed", "icon_chest_rare_open",
                "icon_chest_epic_closed", "icon_chest_epic_open",
                "icon_chest_legendary_closed", "icon_chest_legendary_open"
            };

            foreach (var iconName in chestIcons)
            {
                Sprite sprite = LoadIconSprite(iconName);
                string status = sprite != null ? "✓ Found" : "✗ Missing";
                Debug.Log($"  {iconName}: {status}");
            }

            Debug.Log("\n[IconManager] ========== AVAILABLE DAILY ICONS ==========");
            string[] dailyIcons = new[] {
                "icon_daily_streak", "icon_coin", "icon_gem", "icon_ui_timer"
            };

            foreach (var iconName in dailyIcons)
            {
                Sprite sprite = LoadIconSprite(iconName);
                string status = sprite != null ? "✓ Found" : "✗ Missing";
                Debug.Log($"  {iconName}: {status}");
            }

            Debug.Log("[IconManager] ========== DIAGNOSIS COMPLETE ==========");
        }

        private static int TryAssignSprite(Image img, string iconName)
        {
            // Check if we already have this sprite assigned
            if (img.sprite != null && img.sprite.name.ToLower() == iconName.ToLower())
            {
                Debug.Log($"[IconManager] Sprite {iconName} already assigned to {img.gameObject.name}");
                return 0; // Already assigned
            }

            Sprite sprite = LoadIconSprite(iconName);
            if (sprite != null)
            {
                img.sprite = sprite;
                img.color = Color.white;
                img.preserveAspect = true;
                EditorUtility.SetDirty(img);
                Debug.Log($"[IconManager] ✓ Assigned {iconName} to {GetFullPath(img.transform)}");
                return 1;
            }
            else
            {
                Debug.LogWarning($"[IconManager] ✗ Could not find sprite: {iconName}");
                return 0;
            }
        }

        private static string GetFullPath(Transform t)
        {
            string path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }

        private static string GetGameObjectPath(GameObject obj)
        {
            return GetFullPath(obj.transform);
        }

        private static int AssignBottomNavBarIcons()
        {
            int count = 0;
            var bottomNav = Object.FindObjectOfType<UI.BottomNavBar>();

            if (bottomNav == null)
            {
                Debug.Log("[IconManager] No BottomNavBar found");
                return 0;
            }

            // Mapping of nav buttons to icons
            var mapping = new Dictionary<string, string>
            {
                { "_homeButton", "icon_home" },
                { "_playButton", "icon_play" },
                { "_shopButton", "icon_shop" },
                { "_missionsButton", "icon_missions" },
                { "_profileButton", "icon_profile" }
            };

            SerializedObject so = new SerializedObject(bottomNav);

            foreach (var kvp in mapping)
            {
                count += TryAssignNavButtonIcon(so, kvp.Key, kvp.Value);
            }

            so.ApplyModifiedProperties();
            return count;
        }

        private static int TryAssignNavButtonIcon(SerializedObject so, string propName, string iconName)
        {
            SerializedProperty prop = so.FindProperty(propName);
            if (prop == null) return 0;

            SerializedProperty iconProp = prop.FindPropertyRelative("icon");
            if (iconProp?.objectReferenceValue == null) return 0;

            Image img = iconProp.objectReferenceValue as Image;
            if (img == null) return 0;

            Sprite sprite = LoadIconSprite(iconName);
            if (sprite == null) return 0;

            img.sprite = sprite;
            EditorUtility.SetDirty(img);
            return 1;
        }

        private static int AssignCurrencyDisplayIcons()
        {
            int count = 0;
            var display = Object.FindObjectOfType<Monetization.HeaderCurrencyDisplay>();

            if (display == null) return 0;

            SerializedObject so = new SerializedObject(display);

            count += TryAssignImageProp(so, "_gemsIcon", "icon_gem_single");
            count += TryAssignImageProp(so, "_coinsIcon", "icon_coin_single");

            so.ApplyModifiedProperties();
            return count;
        }

        private static int AssignCompactCurrencyIcons()
        {
            int count = 0;
            GameObject btn = GameObject.Find("CompactCurrencyButton");
            if (btn == null) return 0;

            Transform gemIcon = btn.transform.Find("GemIcon");
            if (gemIcon != null)
            {
                Image img = gemIcon.GetComponent<Image>();
                if (img != null)
                {
                    Sprite sprite = LoadIconSprite("icon_gem_single");
                    if (sprite != null)
                    {
                        img.sprite = sprite;
                        EditorUtility.SetDirty(img);
                        count++;
                    }
                }
            }

            return count;
        }

        private static int TryAssignImageProp(SerializedObject so, string propName, string iconName)
        {
            SerializedProperty prop = so.FindProperty(propName);
            if (prop?.objectReferenceValue == null) return 0;

            Image img = prop.objectReferenceValue as Image;
            if (img == null) return 0;

            Sprite sprite = LoadIconSprite(iconName);
            if (sprite == null) return 0;

            img.sprite = sprite;
            EditorUtility.SetDirty(img);
            return 1;
        }

        private static Sprite LoadIconSprite(string iconName)
        {
            // Normalizar el nombre (sin extensión, lowercase)
            string searchName = iconName.ToLower().Replace(".png", "");

            // Buscar en todas las subcarpetas con nombre exacto
            string[] guids = AssetDatabase.FindAssets($"{searchName} t:Sprite", new[] { ICONS_PATH });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(path).ToLower();

                // Verificar que sea el archivo exacto
                if (fileName == searchName)
                {
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (sprite != null)
                    {
                        Debug.Log($"[IconManager] Found sprite at: {path}");
                        return sprite;
                    }
                }
            }

            // Fallback: buscar por path directo en subcarpetas conocidas
            string[] subfolders = new[]
            {
                "",           // Root
                "Chests",
                "DailyRewards",
                "Currency",
                "UI",
                "Achievements",
                "Rangos",
                "Games",
            };

            foreach (var subfolder in subfolders)
            {
                string fullPath = string.IsNullOrEmpty(subfolder)
                    ? $"{ICONS_PATH}/{iconName}.png"
                    : $"{ICONS_PATH}/{subfolder}/{iconName}.png";

                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(fullPath);
                if (sprite != null)
                {
                    Debug.Log($"[IconManager] Found sprite at path: {fullPath}");
                    return sprite;
                }
            }

            Debug.LogWarning($"[IconManager] Could not find sprite: {iconName}");
            return null;
        }

        #endregion

        #region Scene-Specific Icon Assignment

        // ==================== ACHIEVEMENTS ====================
        [MenuItem("DigitPark/Tools/Icons/Scenes/Assign Achievements Icons", false, 20)]
        public static int AssignAchievementsIcons()
        {
            int count = 0;
            Debug.Log("[IconManager] ========== ASSIGNING ACHIEVEMENTS ICONS ==========");

            ConfigureAllIconsAsSprites();
            var allImages = Object.FindObjectsOfType<Image>(true);

            // Achievement icon mapping (TrophyCard structure: Icon object inside card)
            var achievementIcons = new Dictionary<string, string>
            {
                // Original achievement icons
                { "FirstVictory", "icon_achievement_first_victory" },
                { "WinningStreak", "icon_achievement_winning_streak" },
                { "Collector", "icon_achievement_collector" },
                { "DedicatedPlayer", "icon_achievement_dedicated_player" },
                { "Master", "icon_achievement_master" },
                { "Legend", "icon_achievement_legend" },
                { "Tournament", "icon_achievement_tournament" },
                // New Logro_ icons
                { "Nivel50", "Logro_Nivel50" },
                { "BattlePassCompleto", "Logro_BattlePass_Completo" },
                { "AvanceEpico", "Logro_Avance_Epico" },
                { "Racha7Dias", "Logro_Racha_7_Dias" },
                { "Racha30Dias", "Logro_Racha_30_Dias" },
                { "Racha100Dias", "Logro_Racha_100_Dias" },
                { "Racha365Dias", "Logro_Racha_365_Dias" },
                { "VictoriaRacha7", "Logro_Victoria_Racha_7" },
                { "VictoriaRacha30", "Logro_Victoria_Racha_30" },
                { "PrimerTesoro", "Logro_Primer_Tesoro" },
                { "CofreEpico", "Logro_Cofre_Epico" },
                { "ColeccionistaCofres", "Logro_Coleccionista_Cofres" },
                { "Perfeccionista", "Logro_Perfeccionista" },
                { "DemonioVelocidad", "Logro_Demonio_Velocidad" },
                { "BuhoNocturno", "Logro_Buho_Nocturno" },
                { "AveFenix", "Logro_Ave_Fenix" },
            };

            foreach (var img in allImages)
            {
                string objName = img.gameObject.name;
                string parentName = img.transform.parent?.name ?? "";
                string fullPath = GetFullPath(img.transform);

                // Trophy/Achievement icons
                if (objName == "Icon" || objName == "TrophyIcon" || objName == "AchievementIcon")
                {
                    foreach (var kvp in achievementIcons)
                    {
                        if (parentName.Contains(kvp.Key) || fullPath.Contains(kvp.Key))
                        {
                            count += TryAssignSprite(img, kvp.Value);
                            break;
                        }
                    }
                }

                // Currency icons in header
                if (objName == "Icon" && parentName == "Coins")
                    count += TryAssignSprite(img, "icon_coin");
                if (objName == "Icon" && parentName == "Gems")
                    count += TryAssignSprite(img, "icon_gem");

                // XP icon
                if (objName == "XPIcon" || (objName == "Icon" && parentName == "XP"))
                    count += TryAssignSprite(img, "icon_xp");
            }

            MarkSceneDirtyIfChanged(count);
            Debug.Log($"[IconManager] ========== ASSIGNED {count} ACHIEVEMENTS ICONS ==========");
            return count;
        }

        // ==================== BATTLE PASS ====================
        [MenuItem("DigitPark/Tools/Icons/Scenes/Assign BattlePass Icons", false, 21)]
        public static int AssignBattlePassIcons()
        {
            int count = 0;
            Debug.Log("[IconManager] ========== ASSIGNING BATTLEPASS ICONS ==========");

            ConfigureAllIconsAsSprites();
            var allImages = Object.FindObjectsOfType<Image>(true);

            foreach (var img in allImages)
            {
                string objName = img.gameObject.name;
                string parentName = img.transform.parent?.name ?? "";
                string fullPath = GetFullPath(img.transform);

                // BattlePass specific icons
                if (objName == "SeasonIcon" || objName == "BPIcon")
                    count += TryAssignSprite(img, "icon_battlepass_season");
                if (objName == "LevelIcon")
                    count += TryAssignSprite(img, "icon_battlepass_level");
                if (objName == "PremiumBadge" || objName == "PremiumIcon")
                    count += TryAssignSprite(img, "icon_battlepass_premium_badge");

                // Reward icons based on track
                if (objName == "RewardIcon" && fullPath.Contains("Free"))
                    count += TryAssignSprite(img, "icon_battlepass_reward_free");
                if (objName == "RewardIcon" && fullPath.Contains("Premium"))
                    count += TryAssignSprite(img, "icon_battlepass_reward_premium");

                // Currency in rewards
                if (objName == "CoinIcon" || (objName == "Icon" && parentName.Contains("Coin")))
                    count += TryAssignSprite(img, "icon_coin_single");
                if (objName == "GemIcon" || (objName == "Icon" && parentName.Contains("Gem")))
                    count += TryAssignSprite(img, "icon_gem_single");
                if (objName == "XPIcon" || (objName == "Icon" && parentName.Contains("XP")))
                    count += TryAssignSprite(img, "icon_xp");

                // Chest rewards
                if (objName == "ChestIcon" || objName == "Chest")
                {
                    if (fullPath.Contains("Epic"))
                        count += TryAssignSprite(img, "icon_chest_epic_closed");
                    else if (fullPath.Contains("Legendary"))
                        count += TryAssignSprite(img, "icon_chest_legendary_closed");
                    else if (fullPath.Contains("Rare"))
                        count += TryAssignSprite(img, "icon_chest_rare_closed");
                    else
                        count += TryAssignSprite(img, "icon_chest_common_closed");
                }
            }

            MarkSceneDirtyIfChanged(count);
            Debug.Log($"[IconManager] ========== ASSIGNED {count} BATTLEPASS ICONS ==========");
            return count;
        }

        // ==================== CHEST OPENING ====================
        [MenuItem("DigitPark/Tools/Icons/Scenes/Assign ChestOpening Icons", false, 22)]
        public static int AssignChestOpeningIcons()
        {
            int count = 0;
            Debug.Log("[IconManager] ========== ASSIGNING CHEST OPENING ICONS ==========");

            ConfigureAllIconsAsSprites();
            var allImages = Object.FindObjectsOfType<Image>(true);

            foreach (var img in allImages)
            {
                string objName = img.gameObject.name;
                string parentName = img.transform.parent?.name ?? "";
                string fullPath = GetFullPath(img.transform);

                // Main chest display
                if (objName == "ChestImage" || objName == "Chest" || objName == "MainChest")
                {
                    // Determine rarity by parent or path
                    if (fullPath.Contains("Legendary") || fullPath.Contains("legendary"))
                        count += TryAssignSprite(img, "icon_chest_legendary_closed");
                    else if (fullPath.Contains("Epic") || fullPath.Contains("epic"))
                        count += TryAssignSprite(img, "icon_chest_epic_closed");
                    else if (fullPath.Contains("Rare") || fullPath.Contains("rare"))
                        count += TryAssignSprite(img, "icon_chest_rare_closed");
                    else if (fullPath.Contains("Mythic") || fullPath.Contains("mythic"))
                        count += TryAssignSprite(img, "icon_chest_mythic_closed");
                    else
                        count += TryAssignSprite(img, "icon_chest_common_closed");
                }

                // Opened chest variants
                if (objName == "OpenedChest" || objName == "ChestOpen")
                {
                    if (fullPath.Contains("Legendary"))
                        count += TryAssignSprite(img, "icon_chest_legendary_open");
                    else if (fullPath.Contains("Epic"))
                        count += TryAssignSprite(img, "icon_chest_epic_open");
                    else if (fullPath.Contains("Rare"))
                        count += TryAssignSprite(img, "icon_chest_rare_open");
                    else
                        count += TryAssignSprite(img, "icon_chest_common_open");
                }

                // Reward icons
                if (objName == "CoinIcon" || (objName == "Icon" && parentName.Contains("Coin")))
                    count += TryAssignSprite(img, "icon_coin_single");
                if (objName == "GemIcon" || (objName == "Icon" && parentName.Contains("Gem")))
                    count += TryAssignSprite(img, "icon_gem_single");
            }

            MarkSceneDirtyIfChanged(count);
            Debug.Log($"[IconManager] ========== ASSIGNED {count} CHEST OPENING ICONS ==========");
            return count;
        }

        // ==================== DAILY MISSIONS ====================
        [MenuItem("DigitPark/Tools/Icons/Scenes/Assign Missions Icons", false, 23)]
        public static int AssignMissionsIcons()
        {
            int count = 0;
            Debug.Log("[IconManager] ========== ASSIGNING MISSIONS ICONS ==========");

            ConfigureAllIconsAsSprites();
            var allImages = Object.FindObjectsOfType<Image>(true);

            foreach (var img in allImages)
            {
                string objName = img.gameObject.name;
                string parentName = img.transform.parent?.name ?? "";
                string fullPath = GetFullPath(img.transform);

                // Mission type icons
                if (objName == "MissionIcon" || objName == "TypeIcon")
                {
                    if (fullPath.Contains("Play") || fullPath.Contains("play"))
                        count += TryAssignSprite(img, "icon_mission_play_matches");
                    else if (fullPath.Contains("Win") || fullPath.Contains("win"))
                        count += TryAssignSprite(img, "icon_mission_win_matches");
                    else if (fullPath.Contains("Score") || fullPath.Contains("Points"))
                        count += TryAssignSprite(img, "icon_mission_earn_points");
                    else if (fullPath.Contains("Complete") || fullPath.Contains("complete"))
                        count += TryAssignSprite(img, "icon_mission_complete");
                    else if (fullPath.Contains("Minigame") || fullPath.Contains("AllGames"))
                        count += TryAssignSprite(img, "icon_mission_complete_minigames");
                    else if (fullPath.Contains("Powerup") || fullPath.Contains("Power"))
                        count += TryAssignSprite(img, "icon_mission_use_powerups");
                    else if (fullPath.Contains("Locked") || fullPath.Contains("locked"))
                        count += TryAssignSprite(img, "icon_mission_locked");
                }

                // Completed checkmark
                if (objName == "CompletedIcon" || objName == "CheckIcon")
                    count += TryAssignSprite(img, "icon_mission_complete");

                // Currency rewards
                if (objName == "CoinIcon" || (objName == "Icon" && parentName.Contains("Coin")))
                    count += TryAssignSprite(img, "icon_coin");
                if (objName == "GemIcon" || (objName == "Icon" && parentName.Contains("Gem")))
                    count += TryAssignSprite(img, "icon_gem");
                if (objName == "XPIcon" || (objName == "Icon" && parentName.Contains("XP")))
                    count += TryAssignSprite(img, "icon_xp");

                // Timer
                if (objName == "TimerIcon" || objName == "ClockIcon")
                    count += TryAssignSprite(img, "icon_ui_timer");
            }

            MarkSceneDirtyIfChanged(count);
            Debug.Log($"[IconManager] ========== ASSIGNED {count} MISSIONS ICONS ==========");
            return count;
        }

        // ==================== SHOP ====================
        [MenuItem("DigitPark/Tools/Icons/Scenes/Assign Shop Icons", false, 24)]
        public static int AssignShopIcons()
        {
            int count = 0;
            Debug.Log("[IconManager] ========== ASSIGNING SHOP ICONS ==========");

            ConfigureAllIconsAsSprites();
            var allImages = Object.FindObjectsOfType<Image>(true);

            // Mapping for coin/gem packs
            var coinPacks = new Dictionary<string, string>
            {
                { "1000", "icon_coin_pack_1000" },
                { "6000", "icon_coin_pack_6000" },
                { "15000", "icon_coin_pack_15000" },
            };
            var gemPacks = new Dictionary<string, string>
            {
                { "100", "icon_gem_pack_100" },
                { "500", "icon_gem_pack_500" },
                { "1200", "icon_gem_pack_1200" },
                { "2500", "icon_gem_pack_2500" },
                { "16000", "icon_gem_pack_16000" },
            };

            foreach (var img in allImages)
            {
                string objName = img.gameObject.name;
                string parentName = img.transform.parent?.name ?? "";
                string fullPath = GetFullPath(img.transform);

                // Coin pack icons
                if (fullPath.Contains("Coin") && (objName == "PackIcon" || objName == "Icon"))
                {
                    foreach (var kvp in coinPacks)
                    {
                        if (fullPath.Contains(kvp.Key) || parentName.Contains(kvp.Key))
                        {
                            count += TryAssignSprite(img, kvp.Value);
                            break;
                        }
                    }
                    // Default coin icon
                    if (img.sprite == null)
                        count += TryAssignSprite(img, "icon_coin_single");
                }

                // Gem pack icons
                if (fullPath.Contains("Gem") && (objName == "PackIcon" || objName == "Icon"))
                {
                    foreach (var kvp in gemPacks)
                    {
                        if (fullPath.Contains(kvp.Key) || parentName.Contains(kvp.Key))
                        {
                            count += TryAssignSprite(img, kvp.Value);
                            break;
                        }
                    }
                    // Default gem icon
                    if (img.sprite == null)
                        count += TryAssignSprite(img, "icon_gem_single");
                }

                // Chest icons in shop
                if (objName == "ChestIcon" || (objName == "Icon" && fullPath.Contains("Chest")))
                {
                    if (fullPath.Contains("Legendary"))
                        count += TryAssignSprite(img, "icon_chest_legendary_closed");
                    else if (fullPath.Contains("Epic"))
                        count += TryAssignSprite(img, "icon_chest_epic_closed");
                    else if (fullPath.Contains("Rare"))
                        count += TryAssignSprite(img, "icon_chest_rare_closed");
                    else if (fullPath.Contains("Mythic"))
                        count += TryAssignSprite(img, "icon_chest_mythic_closed");
                    else
                        count += TryAssignSprite(img, "icon_chest_common_closed");
                }

                // Header currency
                if (objName == "Icon" && parentName == "Coins")
                    count += TryAssignSprite(img, "icon_coin");
                if (objName == "Icon" && parentName == "Gems")
                    count += TryAssignSprite(img, "icon_gem");
            }

            MarkSceneDirtyIfChanged(count);
            Debug.Log($"[IconManager] ========== ASSIGNED {count} SHOP ICONS ==========");
            return count;
        }

        // ==================== TOURNAMENTS ====================
        [MenuItem("DigitPark/Tools/Icons/Scenes/Assign Tournaments Icons", false, 25)]
        public static int AssignTournamentsIcons()
        {
            int count = 0;
            Debug.Log("[IconManager] ========== ASSIGNING TOURNAMENTS ICONS ==========");

            ConfigureAllIconsAsSprites();
            var allImages = Object.FindObjectsOfType<Image>(true);

            foreach (var img in allImages)
            {
                string objName = img.gameObject.name;
                string parentName = img.transform.parent?.name ?? "";
                string fullPath = GetFullPath(img.transform);

                // Trophy icon
                if (objName == "TrophyIcon" || objName == "Trophy")
                    count += TryAssignSprite(img, "Trofeo");

                // Tournament icon
                if (objName == "TournamentIcon")
                    count += TryAssignSprite(img, "PlayModeSelectionTorunamentIcon");

                // Create tournament icon
                if (objName == "CreateIcon" || fullPath.Contains("Create"))
                    count += TryAssignSprite(img, "CreateTournamentIcon");

                // Game icons
                if (objName == "GameIcon")
                {
                    if (fullPath.Contains("DigitRush"))
                        count += TryAssignSprite(img, "DigitRushIcon");
                    else if (fullPath.Contains("FlashTap"))
                        count += TryAssignSprite(img, "FlashTapIcon");
                    else if (fullPath.Contains("MemoryPairs"))
                        count += TryAssignSprite(img, "MemoryPairsIcon");
                    else if (fullPath.Contains("QuickMath"))
                        count += TryAssignSprite(img, "QuickMathIcon");
                    else if (fullPath.Contains("OddOneOut"))
                        count += TryAssignSprite(img, "OddOneOutIcon");
                }

                // Reward icons
                if (objName == "CoinIcon" || (objName == "Icon" && parentName.Contains("Coin")))
                    count += TryAssignSprite(img, "icon_coin");
                if (objName == "GemIcon" || (objName == "Icon" && parentName.Contains("Gem")))
                    count += TryAssignSprite(img, "icon_gem");

                // Timer
                if (objName == "TimerIcon" || objName == "ClockIcon")
                    count += TryAssignSprite(img, "icon_ui_timer");

                // Player icon
                if (objName == "PlayerIcon" || objName == "ParticipantIcon")
                    count += TryAssignSprite(img, "PersonWhite");
            }

            MarkSceneDirtyIfChanged(count);
            Debug.Log($"[IconManager] ========== ASSIGNED {count} TOURNAMENTS ICONS ==========");
            return count;
        }

        // ==================== CASH BATTLE ====================
        [MenuItem("DigitPark/Tools/Icons/Scenes/Assign CashBattle Icons", false, 26)]
        public static int AssignCashBattleIcons()
        {
            int count = 0;
            Debug.Log("[IconManager] ========== ASSIGNING CASH BATTLE ICONS ==========");

            ConfigureAllIconsAsSprites();
            var allImages = Object.FindObjectsOfType<Image>(true);

            // Rank icons mapping
            var rankIcons = new Dictionary<string, string>
            {
                { "Novato", "Rango_Novato" },
                { "Bronce", "Rango_Bronce" },
                { "Plata", "Rango_Plata" },
                { "Oro", "Rango_Oro" },
                { "Platino", "Rango_Platino" },
                { "Diamante", "Rango_Diamante" },
                { "Estratega", "Rango_Estratega" },
                { "Campeon", "Rango_Campeon" },
                { "Fenix", "Rango_Fenix" },
                { "Celestial", "Rango_Celestial" },
            };

            foreach (var img in allImages)
            {
                string objName = img.gameObject.name;
                string parentName = img.transform.parent?.name ?? "";
                string fullPath = GetFullPath(img.transform);

                // Rank icons
                if (objName == "RankIcon" || objName == "RangoIcon" || objName == "Icon")
                {
                    foreach (var kvp in rankIcons)
                    {
                        if (fullPath.Contains(kvp.Key) || parentName.Contains(kvp.Key))
                        {
                            count += TryAssignSprite(img, kvp.Value);
                            break;
                        }
                    }
                }

                // Cash/Money icon
                if (objName == "CashIcon" || objName == "MoneyIcon")
                    count += TryAssignSprite(img, "icon_cash");

                // Swords/Battle icon
                if (objName == "BattleIcon" || objName == "SwordsIcon")
                    count += TryAssignSprite(img, "Swords");

                // Currency
                if (objName == "CoinIcon" || (objName == "Icon" && parentName.Contains("Coin")))
                    count += TryAssignSprite(img, "icon_coin");

                // Timer
                if (objName == "TimerIcon" || objName == "ClockIcon")
                    count += TryAssignSprite(img, "icon_ui_timer");

                // Player icon
                if (objName == "PlayerIcon" || objName == "OpponentIcon")
                    count += TryAssignSprite(img, "PersonWhite");
            }

            MarkSceneDirtyIfChanged(count);
            Debug.Log($"[IconManager] ========== ASSIGNED {count} CASH BATTLE ICONS ==========");
            return count;
        }

        // ==================== PROFILE / SCORES ====================
        [MenuItem("DigitPark/Tools/Icons/Scenes/Assign Profile Icons", false, 27)]
        public static int AssignProfileIcons()
        {
            int count = 0;
            Debug.Log("[IconManager] ========== ASSIGNING PROFILE ICONS ==========");

            ConfigureAllIconsAsSprites();
            var allImages = Object.FindObjectsOfType<Image>(true);

            // Rank icons mapping
            var rankIcons = new Dictionary<string, string>
            {
                { "Novato", "Rango_Novato" },
                { "Bronce", "Rango_Bronce" },
                { "Plata", "Rango_Plata" },
                { "Oro", "Rango_Oro" },
                { "Platino", "Rango_Platino" },
                { "Diamante", "Rango_Diamante" },
                { "Estratega", "Rango_Estratega" },
                { "Campeon", "Rango_Campeon" },
                { "Fenix", "Rango_Fenix" },
                { "Celestial", "Rango_Celestial" },
            };

            foreach (var img in allImages)
            {
                string objName = img.gameObject.name;
                string parentName = img.transform.parent?.name ?? "";
                string fullPath = GetFullPath(img.transform);

                // Player rank icon
                if (objName == "RankIcon" || objName == "RangoIcon")
                {
                    foreach (var kvp in rankIcons)
                    {
                        if (fullPath.Contains(kvp.Key) || parentName.Contains(kvp.Key))
                        {
                            count += TryAssignSprite(img, kvp.Value);
                            break;
                        }
                    }
                }

                // Trophy/Achievement showcase
                if (objName == "TrophyIcon" || objName == "AchievementIcon")
                    count += TryAssignSprite(img, "Trofeo");

                // Stats icons
                if (objName == "WinsIcon")
                    count += TryAssignSprite(img, "icon_achievement_winning_streak");
                if (objName == "GamesIcon")
                    count += TryAssignSprite(img, "icon_mission_play_matches");
                if (objName == "StreakIcon")
                    count += TryAssignSprite(img, "icon_daily_streak");

                // Currency display
                if (objName == "CoinIcon" || (objName == "Icon" && parentName.Contains("Coin")))
                    count += TryAssignSprite(img, "icon_coin");
                if (objName == "GemIcon" || (objName == "Icon" && parentName.Contains("Gem")))
                    count += TryAssignSprite(img, "icon_gem");

                // Settings icon
                if (objName == "SettingsIcon")
                    count += TryAssignSprite(img, "SettingsIcon");

                // Edit/Profile icon
                if (objName == "EditIcon" || objName == "ProfileIcon")
                    count += TryAssignSprite(img, "icon_profile");
            }

            MarkSceneDirtyIfChanged(count);
            Debug.Log($"[IconManager] ========== ASSIGNED {count} PROFILE ICONS ==========");
            return count;
        }

        // ==================== AUTO-DETECT AND ASSIGN ====================
        [MenuItem("DigitPark/Tools/Icons/Auto-Assign Icons (Current Scene)", false, 10)]
        public static void AutoAssignCurrentSceneIcons()
        {
            string sceneName = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name;
            int total = 0;

            Debug.Log($"[IconManager] Auto-detecting icons for scene: {sceneName}");

            // Always run common assignments
            total += AssignBottomNavBarIcons();
            total += AssignCurrencyDisplayIcons();
            total += AssignCompactCurrencyIcons();

            // Scene-specific assignments
            switch (sceneName)
            {
                case "DailyRewards":
                    total += AssignDailyRewardsIcons();
                    break;
                case "Achievements":
                    total += AssignAchievementsIcons();
                    break;
                case "BattlePass":
                    total += AssignBattlePassIcons();
                    break;
                case "ChestOpening":
                    total += AssignChestOpeningIcons();
                    break;
                case "DailyMissions":
                    total += AssignMissionsIcons();
                    break;
                case "Shop":
                    total += AssignShopIcons();
                    break;
                case "TournamentsBrowser":
                case "TournamentCreate":
                case "TournamentLobby":
                    total += AssignTournamentsIcons();
                    break;
                case "CashBattleHub":
                case "CashBattle1v1":
                case "CashWallet":
                case "CashHistory":
                case "CashTournaments":
                    total += AssignCashBattleIcons();
                    break;
                case "Profile":
                case "Scores":
                    total += AssignProfileIcons();
                    break;
                default:
                    Debug.Log($"[IconManager] No specific icon assignment for scene: {sceneName}");
                    break;
            }

            EditorUtility.DisplayDialog("Icon Manager",
                $"Se asignaron {total} iconos en la escena {sceneName}.",
                "OK");
        }

        private static void MarkSceneDirtyIfChanged(int count)
        {
            if (count > 0)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                    UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }
        }

        #endregion

        #region Texture Configuration

        [MenuItem("DigitPark/Tools/Icons/Configure All as Sprites", false, 5)]
        public static void ConfigureAllIconsAsSprites()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { ICONS_PATH });
            int configured = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                // Skip non-PNG files
                if (!path.EndsWith(".png")) continue;

                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.alphaIsTransparency = true;
                    importer.mipmapEnabled = false;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.textureCompression = TextureImporterCompression.Compressed;
                    importer.maxTextureSize = 512;

                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    configured++;
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"[IconManager] Configured {configured} textures as Sprites");
            EditorUtility.DisplayDialog("Icon Manager", $"Se configuraron {configured} texturas como Sprites.", "OK");
        }

        #endregion

        #region Public API

        /// <summary>
        /// Obtiene un icono por nombre (para uso en otros scripts)
        /// </summary>
        public static Sprite GetIcon(string iconName)
        {
            return LoadIconSprite(iconName);
        }

        #endregion
    }
}
