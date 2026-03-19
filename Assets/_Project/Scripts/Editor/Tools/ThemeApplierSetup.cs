#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;
using DigitPark.Themes;
using DigitPark.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// Agrega ThemeApplier a todos los elementos UI para que respondan a cambios de tema en runtime.
    /// Procesadores: Images, Texts, Buttons, Sliders, Outlines, Tabs, Dividers, ProgressBars, InputFields.
    /// </summary>
    public class ThemeApplierSetup : EditorWindow
    {
        [MenuItem("DigitPark/Polish/Themes/Add to Current Scene")]
        public static void AddToCurrentScene()
        {
            int count = AddThemeAppliers();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorUtility.DisplayDialog("ThemeApplier Setup",
                $"Se agregaron {count} componentes ThemeApplier.\n\nGuarda la escena (Ctrl+S)", "OK");
        }

        // "Apply Theme Colors" methods REMOVED.
        // These were destructive: they baked theme colors into scene files,
        // overwriting UIBuilder colors. Theme colors are applied at RUNTIME
        // by the ThemeApplier component, not in the Editor.

        // ==================== SCENE PATHS ====================

        private static string[] GetAllScenePaths()
        {
            return new string[]
            {
                // Auth
                "Assets/_Project/Scenes/Auth/Login.unity",
                "Assets/_Project/Scenes/Auth/Register.unity",
                // Core
                "Assets/_Project/Scenes/_Core/Boot.unity",
                "Assets/_Project/Scenes/_Core/MainMenu.unity",
                "Assets/_Project/Scenes/_Core/Settings.unity",
                // Games - Navigation
                "Assets/_Project/Scenes/Games/Navigation/GameSelector.unity",
                "Assets/_Project/Scenes/Games/Navigation/PlayModeSelection.unity",
                "Assets/_Project/Scenes/Games/Navigation/BetSelection.unity",
                "Assets/_Project/Scenes/Games/Navigation/Matchmaking.unity",
                // Games - Minigames
                "Assets/_Project/Scenes/Games/Minigames/DigitRush.unity",
                "Assets/_Project/Scenes/Games/Minigames/MemoryPairs.unity",
                "Assets/_Project/Scenes/Games/Minigames/QuickMath.unity",
                "Assets/_Project/Scenes/Games/Minigames/FlashTap.unity",
                "Assets/_Project/Scenes/Games/Minigames/OddOneOut.unity",
                // Social - Profile
                "Assets/_Project/Scenes/Social/Profile/Profile.unity",
                "Assets/_Project/Scenes/Social/Profile/Scores.unity",
                "Assets/_Project/Scenes/Social/Profile/MatchHistory.unity",
                // Social - Friends
                "Assets/_Project/Scenes/Social/Friends/Friends.unity",
                "Assets/_Project/Scenes/Social/Friends/FriendRequests.unity",
                "Assets/_Project/Scenes/Social/Friends/SearchPlayers.unity",
                // Social - Notifications
                "Assets/_Project/Scenes/Social/Notifications/Notifications.unity",
                // Monetization
                "Assets/_Project/Scenes/Monetization/DailyMissions.unity",
                "Assets/_Project/Scenes/Monetization/DailyRewards.unity",
                "Assets/_Project/Scenes/Monetization/Shop.unity",
                "Assets/_Project/Scenes/Monetization/Achievements.unity",
                // Tournaments
                "Assets/_Project/Scenes/Tournaments/TournamentsBrowser.unity",
                "Assets/_Project/Scenes/Tournaments/TournamentCreate.unity",
                "Assets/_Project/Scenes/Tournaments/TournamentLobby.unity",
                // Onboarding
                "Assets/_Project/Scenes/Onboarding/Onboarding.unity",
                // *** EXCLUIDAS - CashBattle (estilo gold fijo) ***
                // "Assets/_Project/Scenes/CashBattle/CashBattleHub.unity",
                // "Assets/_Project/Scenes/CashBattle/CashBattle1v1.unity",
                // "Assets/_Project/Scenes/CashBattle/CashHistory.unity",
                // "Assets/_Project/Scenes/CashBattle/CashTournaments.unity",
                // "Assets/_Project/Scenes/CashBattle/CashWallet.unity",
                // "Assets/_Project/Scenes/CashBattle/CashProfile.unity",
                // *** EXCLUIDAS - Protegidas ***
                // "Assets/_Project/Scenes/Auth/AgeVerification.unity",
                // "Assets/_Project/Scenes/Onboarding/CashBattleOnboarding.unity",
            };
        }

        // ==================== ADD TO PREFABS ====================

        [MenuItem("DigitPark/Polish/Themes/Add to Prefabs")]
        public static void AddToPrefabs()
        {
            int count = 0;

            // Search both Prefabs/ and Resources/Prefabs/
            var guidList = new List<string>();
            guidList.AddRange(AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Project/Prefabs" }));
            guidList.AddRange(AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Project/Resources/Prefabs" }));
            string[] prefabGuids = guidList.ToArray();

            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                // Skip CashBattle prefabs and gold-themed prefabs (estilo fijo, no ThemeApplier)
                if (path.Contains("CashBattle") || path.Contains("Gold")) continue;
                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
                bool modified = false;

                // Process buttons in prefab
                var buttons = prefabRoot.GetComponentsInChildren<Button>(true);
                foreach (var btn in buttons)
                {
                    if (btn.GetComponent<ThemeApplier>() == null)
                    {
                        ThemeApplier.ElementType elementType = ClassifyButton(btn.gameObject.name);
                        var applier = btn.gameObject.AddComponent<ThemeApplier>();
                        SetElementType(applier, elementType);
                        count++;
                        modified = true;
                    }
                }

                // Process images in prefab
                var images = prefabRoot.GetComponentsInChildren<Image>(true);
                foreach (var img in images)
                {
                    if (img.GetComponent<ThemeApplier>() != null) continue;
                    if (img.GetComponent<Button>() != null) continue;
                    if (img.GetComponent<GameCardEffect>() != null) continue;

                    ThemeApplier.ElementType et = ClassifyImage(img.gameObject.name);
                    if (et != ThemeApplier.ElementType.None)
                    {
                        var applier = img.gameObject.AddComponent<ThemeApplier>();
                        SetElementType(applier, et);
                        SetApplyToText(applier, false);
                        count++;
                        modified = true;
                    }
                }

                // Process texts in prefab
                var texts = prefabRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var text in texts)
                {
                    if (text.GetComponent<ThemeApplier>() != null) continue;
                    if (text.transform.parent?.GetComponent<Button>() != null) continue;

                    ThemeApplier.ElementType et = ClassifyText(text.gameObject.name);
                    if (et != ThemeApplier.ElementType.None)
                    {
                        var applier = text.gameObject.AddComponent<ThemeApplier>();
                        SetElementType(applier, et);
                        SetApplyToText(applier, true);
                        SetApplyToImage(applier, false);
                        count++;
                        modified = true;
                    }
                }

                // Process outlines in prefab (neon glows)
                var outlines = prefabRoot.GetComponentsInChildren<Outline>(true);
                foreach (var outline in outlines)
                {
                    if (outline.GetComponent<ThemeApplier>() != null) continue;
                    if (outline.GetComponent<Button>() != null) continue;

                    ThemeApplier.ElementType et = ClassifyOutline(outline.gameObject.name);
                    var applier = outline.gameObject.AddComponent<ThemeApplier>();
                    SetElementType(applier, et);
                    SetApplyToImage(applier, false);
                    SetApplyToText(applier, false);
                    SetApplyToOutline(applier, true);
                    count++;
                    modified = true;
                }

                if (modified)
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
                }
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Prefabs Actualizados",
                $"Se agregaron {count} componentes ThemeApplier a prefabs.\n" +
                "(Prefabs/ + Resources/Prefabs/, excluyendo CashBattle)", "OK");
        }

        // ==================== ADD TO ALL SCENES ====================

        [MenuItem("DigitPark/Polish/Themes/Add to ALL Scenes")]
        public static void AddToAllScenes()
        {
            if (!EditorUtility.DisplayDialog("Confirmar",
                "¿Agregar ThemeApplier a TODAS las escenas?\n\nEsto permitirá cambiar temas en runtime.",
                "Sí", "Cancelar"))
                return;

            string[] scenePaths = GetAllScenePaths();

            int totalCount = 0;
            foreach (string scenePath in scenePaths)
            {
                try
                {
                    var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    int count = AddThemeAppliers();
                    totalCount += count;
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"[ThemeApplierSetup] {scenePath}: {count} componentes agregados");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[ThemeApplierSetup] Error en {scenePath}: {e.Message}");
                }
            }

            EditorUtility.DisplayDialog("Completado",
                $"ThemeApplier agregado a todas las escenas.\n\nTotal: {totalCount} componentes",
                "OK");
        }

        // ==================== CORE: ADD THEME APPLIERS ====================

        private static int AddThemeAppliers()
        {
            int count = 0;

            count += ProcessImages();
            count += ProcessTexts();
            count += ProcessButtons();
            count += ProcessSliders();
            count += ProcessOutlines();
            count += ProcessDualImageOutlines(); // Image+Outline dual: outline skipped by ProcessOutlines
            count += ProcessTabs();
            count += ProcessDividers();
            count += ProcessProgressBars();
            count += ProcessInputFields();

            return count;
        }

        // ==================== PROCESSOR: IMAGES ====================

        private static int ProcessImages()
        {
            int count = 0;
            var images = GameObject.FindObjectsOfType<Image>(true);

            foreach (var img in images)
            {
                if (img.GetComponent<ThemeApplier>() != null) continue;
                if (img.GetComponent<Button>() != null) continue;
                if (img.GetComponent<GameCardEffect>() != null) continue;

                ThemeApplier.ElementType elementType = ClassifyImage(img.gameObject.name);

                if (elementType != ThemeApplier.ElementType.None)
                {
                    var applier = img.gameObject.AddComponent<ThemeApplier>();
                    SetElementType(applier, elementType);
                    SetApplyToText(applier, false);
                    EditorUtility.SetDirty(img.gameObject);
                    count++;
                }
            }
            return count;
        }

        private static ThemeApplier.ElementType ClassifyImage(string goName)
        {
            string name = goName.ToLower();

            // Backgrounds
            if (name.Contains("background") || name == "bg")
            {
                if (name == "background" || name == "bg" || name.Contains("main") || name.Contains("screen"))
                    return ThemeApplier.ElementType.PrimaryBackground;
                else if (name.Contains("card") || name.Contains("popup"))
                    return ThemeApplier.ElementType.CardBackground;
                else
                    return ThemeApplier.ElementType.SecondaryBackground;
            }

            // Overlay blockers (before generic panel check)
            if (name.Contains("blocker") || name.Contains("dimmer") ||
                (name.Contains("loading") && name.Contains("panel")))
                return ThemeApplier.ElementType.Overlay;

            // Panels/cards
            if (name.Contains("panel"))
                return ThemeApplier.ElementType.CardBackground;

            // Headers
            if (name.Contains("header") && !name.Contains("text"))
                return ThemeApplier.ElementType.SecondaryBackground;

            // Overlays
            if (name.Contains("overlay") && !name.Contains("glass"))
                return ThemeApplier.ElementType.Overlay;

            // Sections
            if (name.Contains("section") && !name.Contains("text"))
                return ThemeApplier.ElementType.TertiaryBackground;

            // Slider fills (not handled by ProcessSliders)
            if (name.Contains("fill") && name.Contains("slider"))
                return ThemeApplier.ElementType.SliderFill;

            // Handles
            if (name.Contains("handle"))
                return ThemeApplier.ElementType.SliderHandle;

            // Separators / Dividers
            if (name.Contains("separator") || name.Contains("divider") || name.Contains("line"))
            {
                // Skip layout lines that are just thin bars
                if (name.Contains("headerline") || name.Contains("topline") || name.Contains("bottomline"))
                    return ThemeApplier.ElementType.Accent;
                return ThemeApplier.ElementType.Accent;
            }

            // Tabs (images that are tab backgrounds without a ThemeApplier)
            if (name.Contains("tab") && !name.Contains("text") && !name.Contains("divider"))
            {
                if (name.Contains("active") || name.Contains("selected"))
                    return ThemeApplier.ElementType.TabActive;
                else if (name.Contains("inactive"))
                    return ThemeApplier.ElementType.TabInactive;
            }

            return ThemeApplier.ElementType.None;
        }

        // ==================== PROCESSOR: TEXTS ====================

        private static int ProcessTexts()
        {
            int count = 0;
            var texts = GameObject.FindObjectsOfType<TextMeshProUGUI>(true);

            foreach (var text in texts)
            {
                if (text.GetComponent<ThemeApplier>() != null) continue;
                if (text.transform.parent?.GetComponent<Button>() != null) continue;

                ThemeApplier.ElementType elementType = ClassifyText(text.gameObject.name);

                if (elementType != ThemeApplier.ElementType.None)
                {
                    var applier = text.gameObject.AddComponent<ThemeApplier>();
                    SetElementType(applier, elementType);
                    SetApplyToText(applier, true);
                    SetApplyToImage(applier, false);
                    EditorUtility.SetDirty(text.gameObject);
                    count++;
                }
            }
            return count;
        }

        private static ThemeApplier.ElementType ClassifyText(string goName)
        {
            string name = goName.ToLower();

            if (name.Contains("title") || name.Contains("header"))
                return ThemeApplier.ElementType.TextTitle;
            if (name.Contains("placeholder") || name == "versiontext" || name.Contains("version"))
                return ThemeApplier.ElementType.TextDisabled;
            if (name.Contains("label") || name.Contains("subtitle") || name == "loadingtext" ||
                name.Contains("count") || name.Contains("total") ||
                (name.Contains("stats") && !name.Contains("value")))
                return ThemeApplier.ElementType.TextSecondary;
            if (name.Contains("error"))
                return ThemeApplier.ElementType.Error;
            if (name.Contains("value") || name.Contains("score") || name.Contains("timer") ||
                name.Contains("round") || name.Contains("time") || name.Contains("reaction"))
                return ThemeApplier.ElementType.Accent;
            if (name.Contains("instruction") || name.Contains("problem"))
                return ThemeApplier.ElementType.TextPrimary;
            if (name.Contains("username") || name.Contains("user"))
                return ThemeApplier.ElementType.TextTitle;

            return ThemeApplier.ElementType.TextPrimary;
        }

        // ==================== PROCESSOR: BUTTONS ====================

        private static int ProcessButtons()
        {
            int count = 0;
            var buttons = GameObject.FindObjectsOfType<Button>(true);

            foreach (var btn in buttons)
            {
                if (btn.GetComponent<ThemeApplier>() != null) continue;
                if (btn.GetComponent<GameCardEffect>() != null) continue;

                ThemeApplier.ElementType elementType = ClassifyButton(btn.gameObject.name);

                var applier = btn.gameObject.AddComponent<ThemeApplier>();
                SetElementType(applier, elementType);
                EditorUtility.SetDirty(btn.gameObject);
                count++;

                // Also process button glow (Outline on the button itself)
                Outline btnOutline = btn.GetComponent<Outline>();
                if (btnOutline != null && btnOutline.GetComponent<ThemeApplier>() == null)
                {
                    // The button already got its ThemeApplier above.
                    // We need a SECOND ThemeApplier just for the outline glow.
                    // Since Unity allows multiple of the same component, we add one specifically for the outline.
                    // However, this gets complicated. Instead, enable applyToOutline on the button's applier
                    // only for button types that have glows.
                    ThemeApplier.ElementType glowType = GetButtonGlowType(elementType);
                    if (glowType != ThemeApplier.ElementType.None)
                    {
                        SetApplyToOutline(applier, true);
                    }
                }
            }
            return count;
        }

        private static ThemeApplier.ElementType ClassifyButton(string goName)
        {
            string name = goName.ToLower();

            // Back/Close buttons - accent color
            if (name.Contains("back") || name.Contains("return") || name.Contains("arrow") ||
                name.Contains("close") || name.Contains("exit"))
                return ThemeApplier.ElementType.Accent;

            // Primary action buttons
            if (name.Contains("login") || name.Contains("play") || name.Contains("confirm") ||
                name.Contains("create") || name.Contains("submit") || name.Contains("register") ||
                name.Contains("save") || name.Contains("accept") || name.Contains("join") ||
                name.Contains("start") || name.Contains("ok") || name.Contains("yes") ||
                name.Contains("claim") || name.Contains("collect") || name.Contains("send") ||
                name.Contains("search") || name.Contains("fab"))
                return ThemeApplier.ElementType.ButtonPrimary;

            // Danger buttons
            if (name.Contains("delete") || name.Contains("logout") || name.Contains("remove") ||
                name.Contains("reject") || name.Contains("decline") || name.Contains("challenge"))
                return ThemeApplier.ElementType.ButtonDanger;

            // Cancel/secondary
            if (name.Contains("cancel") || name.Contains("no") || name.Contains("clear") ||
                name.Contains("filter") || name.Contains("secondary"))
                return ThemeApplier.ElementType.ButtonSecondary;

            // Game buttons (grids, cards, answers)
            if (name.Contains("btn_") || name.Contains("card") || name.Contains("answer") ||
                name.Contains("grid") || name.Contains("left") || name.Contains("right") ||
                name.Contains("tap"))
                return ThemeApplier.ElementType.CardBackground;

            // Game selector buttons
            if (name.Contains("digit") || name.Contains("memory") || name.Contains("quick") ||
                name.Contains("flash") || name.Contains("odd") || name.Contains("cognitive") ||
                name.Contains("sprint"))
                return ThemeApplier.ElementType.ButtonPrimary;

            // Tab buttons
            if (name.Contains("tab"))
                return ThemeApplier.ElementType.TabInactive;

            return ThemeApplier.ElementType.ButtonSecondary;
        }

        private static ThemeApplier.ElementType GetButtonGlowType(ThemeApplier.ElementType buttonType)
        {
            switch (buttonType)
            {
                case ThemeApplier.ElementType.ButtonPrimary: return ThemeApplier.ElementType.ButtonGlowPrimary;
                case ThemeApplier.ElementType.ButtonSuccess: return ThemeApplier.ElementType.ButtonGlowSuccess;
                case ThemeApplier.ElementType.ButtonDanger: return ThemeApplier.ElementType.ButtonGlowDanger;
                default: return ThemeApplier.ElementType.None;
            }
        }

        // ==================== PROCESSOR: SLIDERS ====================

        private static int ProcessSliders()
        {
            int count = 0;
            var sliders = GameObject.FindObjectsOfType<Slider>(true);

            foreach (var slider in sliders)
            {
                // Fill
                if (slider.fillRect != null)
                {
                    var fill = slider.fillRect.GetComponent<Image>();
                    if (fill != null && fill.GetComponent<ThemeApplier>() == null)
                    {
                        var applier = fill.gameObject.AddComponent<ThemeApplier>();
                        SetElementType(applier, ThemeApplier.ElementType.SliderFill);
                        SetApplyToText(applier, false);
                        EditorUtility.SetDirty(fill.gameObject);
                        count++;
                    }
                }

                // Handle
                if (slider.handleRect != null)
                {
                    var handle = slider.handleRect.GetComponent<Image>();
                    if (handle != null && handle.GetComponent<ThemeApplier>() == null)
                    {
                        var applier = handle.gameObject.AddComponent<ThemeApplier>();
                        SetElementType(applier, ThemeApplier.ElementType.SliderHandle);
                        SetApplyToText(applier, false);
                        EditorUtility.SetDirty(handle.gameObject);
                        count++;
                    }
                }

                // Background/Track
                var bg = slider.transform.Find("Background")?.GetComponent<Image>();
                if (bg != null && bg.GetComponent<ThemeApplier>() == null)
                {
                    var applier = bg.gameObject.AddComponent<ThemeApplier>();
                    SetElementType(applier, ThemeApplier.ElementType.SliderTrack);
                    SetApplyToText(applier, false);
                    EditorUtility.SetDirty(bg.gameObject);
                    count++;
                }
            }
            return count;
        }

        // ==================== PROCESSOR: OUTLINES (Neon Glows) ====================

        private static int ProcessOutlines()
        {
            int count = 0;
            var outlines = GameObject.FindObjectsOfType<Outline>(true);

            foreach (var outline in outlines)
            {
                // Skip if already has ThemeApplier
                if (outline.GetComponent<ThemeApplier>() != null) continue;

                // Skip buttons (handled in ProcessButtons with applyToOutline)
                if (outline.GetComponent<Button>() != null) continue;

                // Skip GameCardEffect objects
                if (outline.GetComponent<GameCardEffect>() != null) continue;

                ThemeApplier.ElementType type = ClassifyOutline(outline.gameObject.name);

                var applier = outline.gameObject.AddComponent<ThemeApplier>();
                SetElementType(applier, type);
                SetApplyToImage(applier, false);
                SetApplyToText(applier, false);
                SetApplyToOutline(applier, true);
                EditorUtility.SetDirty(outline.gameObject);
                count++;
            }
            return count;
        }

        private static ThemeApplier.ElementType ClassifyOutline(string goName)
        {
            string name = goName.ToLower();

            if (name.Contains("button") || name.Contains("btn"))
                return ThemeApplier.ElementType.ButtonGlowPrimary;
            if (name.Contains("input") || name.Contains("field") || name.Contains("search"))
                return ThemeApplier.ElementType.Accent;
            if (name.Contains("card") || name.Contains("panel") || name.Contains("item"))
                return ThemeApplier.ElementType.Glow;
            if (name.Contains("tab"))
                return ThemeApplier.ElementType.Accent;

            return ThemeApplier.ElementType.Glow;
        }

        // ==================== PROCESSOR: DUAL IMAGE + OUTLINE ====================
        // GOs that have both Image AND Outline need a SECOND ThemeApplier for the outline.
        // ProcessOutlines() skips them because ProcessImages() already added ThemeApplier.
        // This processor detects that gap and adds the glow-only ThemeApplier.

        private static int ProcessDualImageOutlines()
        {
            int count = 0;
            var images = GameObject.FindObjectsOfType<Image>(true);

            foreach (var img in images)
            {
                // Must have an Outline to be a dual case
                Outline outline = img.GetComponent<Outline>();
                if (outline == null) continue;

                // Skip buttons (ProcessButtons handles their glow via applyToOutline)
                if (img.GetComponent<Button>() != null) continue;
                if (img.GetComponent<GameCardEffect>() != null) continue;

                // Check if any existing ThemeApplier already covers the outline
                bool outlineAlreadyCovered = false;
                var existingAppliers = img.GetComponents<ThemeApplier>();
                foreach (var ta in existingAppliers)
                {
                    var so = new SerializedObject(ta);
                    if (so.FindProperty("applyToOutline").boolValue)
                    {
                        outlineAlreadyCovered = true;
                        break;
                    }
                }
                if (outlineAlreadyCovered) continue;

                // If no ThemeApplier exists at all, ProcessOutlines() will handle it — skip
                if (existingAppliers.Length == 0) continue;

                // Dual case confirmed: has Image ThemeApplier but outline uncovered → add glow ThemeApplier
                var applier = img.gameObject.AddComponent<ThemeApplier>();
                SetElementType(applier, ThemeApplier.ElementType.Glow);
                SetApplyToImage(applier, false);
                SetApplyToText(applier, false);
                SetApplyToOutline(applier, true);
                EditorUtility.SetDirty(img.gameObject);
                count++;
            }
            return count;
        }

        // ==================== PROCESSOR: TABS ====================

        private static int ProcessTabs()
        {
            int count = 0;
            var images = GameObject.FindObjectsOfType<Image>(true);

            foreach (var img in images)
            {
                if (img.GetComponent<ThemeApplier>() != null) continue;

                string name = img.gameObject.name.ToLower();

                // Detect tab-like objects by name
                if (!name.Contains("tab")) continue;
                if (name.Contains("divider") || name.Contains("indicator") || name.Contains("text")) continue;

                // Must have a Button component to be a tab
                if (img.GetComponent<Button>() == null) continue;

                // Determine active vs inactive based on naming
                bool isActive = name.Contains("active") || name.Contains("selected") ||
                                name.Contains("nacional") || name.Contains("national");

                ThemeApplier.ElementType type = isActive
                    ? ThemeApplier.ElementType.TabActive
                    : ThemeApplier.ElementType.TabInactive;

                var applier = img.gameObject.AddComponent<ThemeApplier>();
                SetElementType(applier, type);
                SetApplyToText(applier, false);
                EditorUtility.SetDirty(img.gameObject);
                count++;
            }
            return count;
        }

        // ==================== PROCESSOR: DIVIDERS ====================

        private static int ProcessDividers()
        {
            int count = 0;
            var images = GameObject.FindObjectsOfType<Image>(true);

            foreach (var img in images)
            {
                if (img.GetComponent<ThemeApplier>() != null) continue;
                if (img.GetComponent<Button>() != null) continue;

                string name = img.gameObject.name.ToLower();

                if (name.Contains("divider") || name.Contains("separator") ||
                    (name.Contains("line") && !name.Contains("outline") && !name.Contains("underline")))
                {
                    // Only small images (dividers are typically thin)
                    RectTransform rt = img.GetComponent<RectTransform>();
                    if (rt == null) continue;

                    var applier = img.gameObject.AddComponent<ThemeApplier>();
                    SetElementType(applier, ThemeApplier.ElementType.Accent);
                    SetApplyToText(applier, false);
                    EditorUtility.SetDirty(img.gameObject);
                    count++;
                }
            }
            return count;
        }

        // ==================== PROCESSOR: PROGRESS BARS ====================

        private static int ProcessProgressBars()
        {
            int count = 0;
            var images = GameObject.FindObjectsOfType<Image>(true);

            foreach (var img in images)
            {
                if (img.GetComponent<ThemeApplier>() != null) continue;
                if (img.GetComponent<Button>() != null) continue;

                string name = img.gameObject.name.ToLower();

                // Detect fill/progress images that aren't part of a Slider
                bool isFill = name.Contains("fill") || name.Contains("progress");
                if (!isFill) continue;

                // Skip if parent is a Slider (already handled by ProcessSliders)
                bool isSliderChild = false;
                Transform parent = img.transform.parent;
                while (parent != null)
                {
                    if (parent.GetComponent<Slider>() != null)
                    {
                        isSliderChild = true;
                        break;
                    }
                    parent = parent.parent;
                }
                if (isSliderChild) continue;

                var applier = img.gameObject.AddComponent<ThemeApplier>();
                SetElementType(applier, ThemeApplier.ElementType.Accent);
                SetApplyToText(applier, false);
                EditorUtility.SetDirty(img.gameObject);
                count++;
            }
            return count;
        }

        // ==================== PROCESSOR: INPUT FIELDS ====================

        private static int ProcessInputFields()
        {
            int count = 0;
            var inputFields = GameObject.FindObjectsOfType<TMP_InputField>(true);

            foreach (var input in inputFields)
            {
                // Input background
                Image inputBg = input.GetComponent<Image>();
                if (inputBg != null && inputBg.GetComponent<ThemeApplier>() == null)
                {
                    var applier = inputBg.gameObject.AddComponent<ThemeApplier>();
                    SetElementType(applier, ThemeApplier.ElementType.InputBackground);
                    SetApplyToText(applier, false);
                    EditorUtility.SetDirty(inputBg.gameObject);
                    count++;
                }

                // Input border (Outline component on the input)
                Outline inputOutline = input.GetComponent<Outline>();
                if (inputOutline != null && inputOutline.GetComponent<ThemeApplier>() == null)
                {
                    // If there's already a ThemeApplier from above, enable outline on it
                    ThemeApplier existing = input.GetComponent<ThemeApplier>();
                    if (existing != null)
                    {
                        SetApplyToOutline(existing, true);
                    }
                }

                // Placeholder text
                if (input.placeholder != null)
                {
                    var placeholderGO = input.placeholder.gameObject;
                    if (placeholderGO.GetComponent<ThemeApplier>() == null)
                    {
                        var applier = placeholderGO.AddComponent<ThemeApplier>();
                        SetElementType(applier, ThemeApplier.ElementType.InputPlaceholder);
                        SetApplyToText(applier, true);
                        SetApplyToImage(applier, false);
                        EditorUtility.SetDirty(placeholderGO);
                        count++;
                    }
                }

                // Input text
                if (input.textComponent != null)
                {
                    var textGO = input.textComponent.gameObject;
                    if (textGO.GetComponent<ThemeApplier>() == null)
                    {
                        var applier = textGO.AddComponent<ThemeApplier>();
                        SetElementType(applier, ThemeApplier.ElementType.TextPrimary);
                        SetApplyToText(applier, true);
                        SetApplyToImage(applier, false);
                        EditorUtility.SetDirty(textGO);
                        count++;
                    }
                }
            }
            return count;
        }

        // ==================== HELPERS ====================

        private static void SetElementType(ThemeApplier applier, ThemeApplier.ElementType type)
        {
            var so = new SerializedObject(applier);
            so.FindProperty("elementType").enumValueIndex = (int)type;
            so.ApplyModifiedProperties();
        }

        private static void SetApplyToText(ThemeApplier applier, bool value)
        {
            var so = new SerializedObject(applier);
            so.FindProperty("applyToText").boolValue = value;
            so.ApplyModifiedProperties();
        }

        private static void SetApplyToImage(ThemeApplier applier, bool value)
        {
            var so = new SerializedObject(applier);
            so.FindProperty("applyToImage").boolValue = value;
            so.ApplyModifiedProperties();
        }

        private static void SetApplyToOutline(ThemeApplier applier, bool value)
        {
            var so = new SerializedObject(applier);
            so.FindProperty("applyToOutline").boolValue = value;
            so.ApplyModifiedProperties();
        }

        private static void SetApplyToShadow(ThemeApplier applier, bool value)
        {
            var so = new SerializedObject(applier);
            so.FindProperty("applyToShadow").boolValue = value;
            so.ApplyModifiedProperties();
        }
    }
}
#endif
