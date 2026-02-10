using UnityEngine;

namespace DigitPark.Managers
{
    /// <summary>
    /// Auto-initializes the InAppNotificationManager when the game starts.
    /// Also initializes NotificationStorageService for persistence.
    /// Uses RuntimeInitializeOnLoadMethod to create before any scene loads.
    /// </summary>
    public static class InAppNotificationInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            // NotificationStorageService
            if (DigitPark.Services.NotificationStorageService.Instance == null)
            {
                GameObject storageObj = new GameObject("NotificationStorageService");
                storageObj.AddComponent<DigitPark.Services.NotificationStorageService>();
                Debug.Log("[InAppNotification] NotificationStorageService auto-initialized");
            }

            // InAppNotificationManager
            if (InAppNotificationManager.Instance == null)
            {
                GameObject managerObj = new GameObject("InAppNotificationManager");
                managerObj.AddComponent<InAppNotificationManager>();
                Debug.Log("[InAppNotification] InAppNotificationManager auto-initialized");
            }
        }
    }
}
