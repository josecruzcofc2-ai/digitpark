using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using DigitPark.Cosmetics;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor tool: adds a BackgroundPattern GO (RawImage + BackgroundPatternReceiver)
    /// to the main Canvas of each scene in the background pattern system.
    ///
    /// 39 scenes total:
    ///   - 28 thematic scenes  → white or chromatic tint (decided at runtime by receiver)
    ///   - 11 real money scenes → gold tint (decided at runtime by receiver)
    ///
    /// Run from: DigitPark → Backgrounds → Add Pattern Layer to Current Scene
    ///           DigitPark → Backgrounds → Add Pattern Layer to All Scenes
    /// </summary>
    public static class BackgroundPatternSetup
    {
        // ==================== Scene paths ====================

        private static readonly string[] ALL_SCENE_PATHS =
        {
            // ── 28 Thematic scenes ──────────────────────────────────────────────
            "Assets/_Project/Scenes/_Core/Boot.unity",
            "Assets/_Project/Scenes/_Core/MainMenu.unity",
            "Assets/_Project/Scenes/_Core/Settings.unity",
            "Assets/_Project/Scenes/Auth/Login.unity",
            "Assets/_Project/Scenes/Auth/Register.unity",
            "Assets/_Project/Scenes/Games/Navigation/GameSelector.unity",
            "Assets/_Project/Scenes/Games/Navigation/PlayModeSelection.unity",
            "Assets/_Project/Scenes/Games/Navigation/BetSelection.unity",
            "Assets/_Project/Scenes/Games/Navigation/Matchmaking.unity",
            "Assets/_Project/Scenes/Tournaments/TournamentsBrowser.unity",
            "Assets/_Project/Scenes/Tournaments/TournamentCreate.unity",
            "Assets/_Project/Scenes/Tournaments/TournamentLobby.unity",
            "Assets/_Project/Scenes/Games/Minigames/DigitRush.unity",
            "Assets/_Project/Scenes/Games/Minigames/FlashTap.unity",
            "Assets/_Project/Scenes/Games/Minigames/MemoryPairs.unity",
            "Assets/_Project/Scenes/Games/Minigames/OddOneOut.unity",
            "Assets/_Project/Scenes/Games/Minigames/QuickMath.unity",
            "Assets/_Project/Scenes/Social/Profile/Profile.unity",
            "Assets/_Project/Scenes/Social/Profile/Scores.unity",
            "Assets/_Project/Scenes/Social/Profile/MatchHistory.unity",
            "Assets/_Project/Scenes/Social/Friends/Friends.unity",
            "Assets/_Project/Scenes/Social/Friends/FriendRequests.unity",
            "Assets/_Project/Scenes/Social/Friends/SearchPlayers.unity",
            "Assets/_Project/Scenes/Social/Notifications/Notifications.unity",
            "Assets/_Project/Scenes/Monetization/Shop.unity",
            "Assets/_Project/Scenes/Monetization/DailyMissions.unity",
            "Assets/_Project/Scenes/Monetization/DailyRewards.unity",
            "Assets/_Project/Scenes/Monetization/Achievements.unity",

            // ── 11 Real money scenes (gold tint applied at runtime) ─────────────
        };

        // ==================== Menu items ====================

        [MenuItem("DigitPark/Backgrounds/Add Pattern Layer to Current Scene")]
        public static void AddPatternLayerToCurrentScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            bool added = AddPatternLayerToLoadedScene(scene.name);

            if (added)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                Debug.Log($"[BGPattern] Done — BackgroundPattern added to {scene.name}. Save scene to persist.");
            }
        }

        [MenuItem("DigitPark/Backgrounds/Add Pattern Layer to All Scenes")]
        public static void AddPatternLayerToAllScenes()
        {
            // Save current scene if dirty before iterating
            if (EditorSceneManager.GetActiveScene().isDirty)
            {
                if (AllScenesBatchBuilder.SilentMode)
                {
                    EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                }
                else
                {
                    bool save = EditorUtility.DisplayDialog(
                        "Unsaved Changes",
                        "The active scene has unsaved changes. Save before proceeding?",
                        "Save & Continue", "Cancel");
                    if (!save) return;
                    EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                }
            }

            int added    = 0;
            int skipped  = 0;
            int notFound = 0;

            foreach (var path in ALL_SCENE_PATHS)
            {
                if (!File.Exists(path))
                {
                    Debug.LogWarning($"[BGPattern] Scene not found: {path}");
                    notFound++;
                    continue;
                }

                var scene   = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                bool wasAdded = AddPatternLayerToLoadedScene(scene.name);

                if (wasAdded)
                {
                    EditorSceneManager.SaveScene(scene);
                    added++;
                }
                else
                {
                    skipped++;
                }
            }

            if (!AllScenesBatchBuilder.SilentMode)
                EditorUtility.DisplayDialog(
                    "BackgroundPattern Setup Complete",
                    $"Finished!\n\n" +
                    $"  Added:    {added} scenes\n" +
                    $"  Skipped:  {skipped} (already existed)\n" +
                    $"  Missing:  {notFound} (path not found — check log)",
                    "OK");
        }

        // ==================== Core logic ====================

        /// <summary>
        /// Adds BackgroundPattern GO to the main Canvas of the currently loaded scene.
        /// Returns true if a new GO was added, false if it already existed.
        /// </summary>
        private static bool AddPatternLayerToLoadedScene(string sceneName)
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogWarning($"[BGPattern] No Canvas found in scene: {sceneName}");
                return false;
            }

            // Avoid duplicates
            if (canvas.transform.Find("BackgroundPattern") != null)
            {
                Debug.Log($"[BGPattern] Already exists in: {sceneName}");
                return false;
            }

            // ── Create GO ──────────────────────────────────────────────────────
            var go = new GameObject("BackgroundPattern");
            go.transform.SetParent(canvas.transform, false);

            // Sibling order: right after Background (index 0) = index 1
            Transform bg = canvas.transform.Find("Background");
            int siblingIndex = bg != null ? bg.GetSiblingIndex() + 1 : 1;
            go.transform.SetSiblingIndex(siblingIndex);

            // RawImage — full stretch, transparent by default (bg_solid state)
            var rawImg = go.AddComponent<RawImage>();
            rawImg.color         = new Color(1f, 1f, 1f, 0f);
            rawImg.raycastTarget = false;

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin  = Vector2.zero;
            rt.anchorMax  = Vector2.one;
            rt.offsetMin  = Vector2.zero;
            rt.offsetMax  = Vector2.zero;

            // Receiver — handles tint logic and registers with the Manager at runtime
            go.AddComponent<BackgroundPatternReceiver>();

            EditorUtility.SetDirty(go);
            Debug.Log($"[BGPattern] Added BackgroundPattern to: {sceneName}");
            return true;
        }
    }
}
