using UnityEngine;

namespace DigitPark.Managers
{
    /// <summary>
    /// Auto-initializes the AchievementNotificationManager when the game starts.
    /// Uses RuntimeInitializeOnLoadMethod to create the manager before any scene loads.
    /// </summary>
    public static class AchievementNotificationInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            // Check if already exists
            if (AchievementNotificationManager.Instance != null)
            {
                Debug.Log("[AchievementNotification] Manager already exists");
                return;
            }

            // Create the manager
            GameObject managerObj = new GameObject("AchievementNotificationManager");
            managerObj.AddComponent<AchievementNotificationManager>();

            // DontDestroyOnLoad is handled in the manager's Awake

            Debug.Log("[AchievementNotification] Manager auto-initialized");
        }
    }
}
