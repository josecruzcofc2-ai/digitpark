using System;
using System.Threading.Tasks;
using UnityEngine;
using FirebaseDB = Firebase.Database;

namespace DigitPark.Services
{
    /// <summary>
    /// Centralized server time helper using Firebase .info/serverTimeOffset.
    /// Prevents client clock manipulation for daily rewards, missions, and offers.
    /// </summary>
    public static class ServerTimeHelper
    {
        private static long _cachedOffsetMs = 0;
        private static bool _offsetLoaded = false;

        /// <summary>
        /// Returns server-adjusted UTC time. Falls back to local UTC if Firebase unavailable.
        /// </summary>
        public static DateTime UtcNow
        {
            get
            {
                if (_offsetLoaded)
                    return DateTime.UtcNow.AddMilliseconds(_cachedOffsetMs);
                return DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Fetches the server time offset from Firebase and caches it.
        /// Call once at boot (e.g., from BootManager) and periodically refresh.
        /// </summary>
        public static async Task RefreshOffsetAsync()
        {
            try
            {
                var db = FirebaseDB.FirebaseDatabase.DefaultInstance;
                var serverTimeRef = db.GetReference(".info/serverTimeOffset");
                var snapshot = await serverTimeRef.GetValueAsync();
                if (snapshot?.Value != null)
                {
                    _cachedOffsetMs = Convert.ToInt64(snapshot.Value);
                    _offsetLoaded = true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ServerTimeHelper] Offset unavailable, using local time: {ex.Message}");
            }
        }
    }
}
