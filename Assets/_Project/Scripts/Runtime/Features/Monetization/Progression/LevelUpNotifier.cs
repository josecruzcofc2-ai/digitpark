using UnityEngine;
using DigitPark.Localization;
using DigitPark.Managers;

namespace DigitPark.Progression
{
    /// <summary>
    /// Escucha los eventos OnLevelUp y OnRewardUnlocked de PlayerProgressionSystem.
    /// Muestra el panel rico LevelUpPanel si está disponible, fallback a toast.
    /// Añadir este componente al mismo GameObject que PlayerProgressionSystem en Boot.
    /// </summary>
    [RequireComponent(typeof(PlayerProgressionSystem))]
    public class LevelUpNotifier : MonoBehaviour
    {
        private const string PREFAB_PATH = "UI/LevelUpPanel";
        private LevelUpPanel _activePanel;

        private void OnEnable()
        {
            var progression = GetComponent<PlayerProgressionSystem>();
            if (progression != null)
            {
                progression.OnLevelUp       += HandleLevelUp;
                progression.OnRewardUnlocked += HandleRewardUnlocked;
            }
        }

        private void OnDisable()
        {
            var progression = GetComponent<PlayerProgressionSystem>();
            if (progression != null)
            {
                progression.OnLevelUp       -= HandleLevelUp;
                progression.OnRewardUnlocked -= HandleRewardUnlocked;
            }
        }

        private void HandleLevelUp(int newLevel)
        {
            int oldLevel = newLevel - 1;
            float progress = 0f;
            LevelReward reward = null;

            var prog = PlayerProgressionSystem.Instance;
            if (prog != null)
            {
                progress = prog.GetLevelProgress();
                reward = prog.GetRewardForLevel(newLevel);
            }

            // Try rich panel first
            var panel = GetOrCreatePanel();
            if (panel != null)
            {
                panel.Show(oldLevel, newLevel, progress, reward);
                Debug.Log($"[LevelUpNotifier] Level up → {newLevel} (panel rico)");
            }
            else
            {
                // Fallback: toast
                string title = AutoLocalizer.Get("levelup_title");
                string body  = AutoLocalizer.Get("levelup_body", newLevel);
                InAppNotificationManager.Instance?.Show(title, body, "level_up");
                Debug.Log($"[LevelUpNotifier] Level up → {newLevel} (toast fallback)");
            }
        }

        private void HandleRewardUnlocked(LevelReward reward)
        {
            // The reward is shown inside the LevelUpPanel as part of Fase 3.
            // Only show toast if the panel is not active (edge case).
            if (_activePanel != null && _activePanel.gameObject.activeInHierarchy)
                return;

            string title = AutoLocalizer.Get("reward_unlocked_title");
            string body  = reward.name;
            InAppNotificationManager.Instance?.Show(title, body, "level_reward");
            Debug.Log($"[LevelUpNotifier] Reward unlocked: {reward.name}");
        }

        private LevelUpPanel GetOrCreatePanel()
        {
            // Reuse active panel if it exists (for queue support)
            if (_activePanel != null && _activePanel.gameObject != null)
                return _activePanel;

            var prefab = Resources.Load<GameObject>(PREFAB_PATH);
            if (prefab == null)
            {
                Debug.LogWarning($"[LevelUpNotifier] Prefab not found at Resources/{PREFAB_PATH}. Run DigitPark/Progression/Build LevelUp Panel Prefab first.");
                return null;
            }

            // Find root canvas to parent under
            Canvas rootCanvas = FindRootCanvas();
            GameObject instance = rootCanvas != null
                ? Object.Instantiate(prefab, rootCanvas.transform)
                : Object.Instantiate(prefab);

            _activePanel = instance.GetComponent<LevelUpPanel>();
            return _activePanel;
        }

        private Canvas FindRootCanvas()
        {
            // Prefer a canvas tagged MainCanvas, else find any
            var canvases = FindObjectsOfType<Canvas>();
            foreach (var c in canvases)
            {
                if (c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay)
                    return c;
            }
            return canvases.Length > 0 ? canvases[0] : null;
        }
    }
}
