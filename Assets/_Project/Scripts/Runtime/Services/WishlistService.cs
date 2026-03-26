using System;
using System.Collections.Generic;
using UnityEngine;
using DigitPark.Localization;
using DigitPark.Managers;
using DigitPark.UI;

namespace DigitPark.Services
{
    /// <summary>
    /// Wishlist system for Daily Offers FOMO loop.
    ///
    /// Design:
    /// - Player hearts any shop item → saved to PlayerPrefs
    /// - Each day after offers generate: checks if any wishlisted item is in today's offers
    /// - If match: in-app notification + local notification stub at 10:00 AM local
    /// - Max 1 notification per day (anti-spam)
    /// - Local notification requires Unity Mobile Notifications package (TODO when package installed)
    ///   Install: Window > Package Manager > com.unity.mobile.notifications
    /// </summary>
    public class WishlistService : MonoBehaviour
    {
        public static WishlistService Instance { get; private set; }

        private const string PREFS_KEY         = "DP_Wishlist";
        private const string NOTIF_DATE_KEY    = "DP_WishlistNotifDate";

        private readonly HashSet<string> _wishlist = new HashSet<string>();

        // ==================== LIFECYCLE ====================

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadWishlist();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // ==================== PUBLIC API ====================

        public bool IsWishlisted(string itemId) => _wishlist.Contains(itemId);

        public void Add(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return;
            if (_wishlist.Add(itemId))
            {
                SaveWishlist();
                Debug.Log($"[WishlistService] Added: {itemId}");
            }
        }

        public void Remove(string itemId)
        {
            if (_wishlist.Remove(itemId))
            {
                SaveWishlist();
                Debug.Log($"[WishlistService] Removed: {itemId}");
            }
        }

        public void Toggle(string itemId)
        {
            if (_wishlist.Contains(itemId))
                Remove(itemId);
            else
                Add(itemId);
        }

        /// <summary>
        /// Returns how many of today's offers are in the wishlist (unpurchased, non-free).
        /// Used to show a badge count on the shop icon.
        /// </summary>
        public int GetMatchCount(DailyOffer[] todayOffers)
        {
            if (todayOffers == null || _wishlist.Count == 0) return 0;

            int count = 0;
            foreach (var offer in todayOffers)
            {
                if (offer != null && !offer.purchased && !offer.isFree && _wishlist.Contains(offer.itemId))
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Call this after DailyOfferService generates today's offers.
        /// Fires in-app notification + schedules local notification if any wishlisted item is available.
        /// Runs at most once per day (anti-spam).
        /// </summary>
        public void CheckAndNotifyWishlist(DailyOffer[] todayOffers)
        {
            if (todayOffers == null || _wishlist.Count == 0) return;

            // Only notify once per day
            string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            if (PlayerPrefs.GetString(NOTIF_DATE_KEY, "") == today) return;

            foreach (var offer in todayOffers)
            {
                if (offer == null || offer.isFree || offer.purchased) continue;
                if (!_wishlist.Contains(offer.itemId)) continue;

                // Match found — stamp the day first to prevent duplicate calls
                PlayerPrefs.SetString(NOTIF_DATE_KEY, today);
                PlayerPrefs.Save();

                // Local notification for players who don't have the app open
                ScheduleLocalNotification(offer.displayName);

                Debug.Log($"[WishlistService] Wishlist match in today's offers: {offer.itemId}");
                break; // Max 1 notification per day even if multiple matches
            }
        }

        // ==================== LOCAL NOTIFICATION STUB ====================

        /// <summary>
        /// Schedules a local notification for 10:00 AM local time.
        /// Requires: Window > Package Manager > com.unity.mobile.notifications
        /// </summary>
        private void ScheduleLocalNotification(string itemName)
        {
            // TODO: implement when Unity Mobile Notifications package is installed
            // Example (iOS):
            //   var notification = new iOSNotification();
            //   notification.Title = AutoLocalizer.Get("wishlist_notif_title");
            //   notification.Body  = AutoLocalizer.Get("wishlist_notif_body", itemName);
            //   notification.Trigger = new iOSNotificationCalendarTrigger { Hour = 10, Minute = 0, Repeats = false };
            //   iOSNotificationCenter.ScheduleNotification(notification);
            //
            // Example (Android):
            //   var notification = new AndroidNotification();
            //   notification.Title    = AutoLocalizer.Get("wishlist_notif_title");
            //   notification.Text     = AutoLocalizer.Get("wishlist_notif_body", itemName);
            //   notification.FireTime = GetNext10AM();
            //   AndroidNotificationCenter.SendNotification(notification, "wishlist_channel");

            Debug.Log($"[WishlistService] Local notification stub — would notify for: {itemName} at 10:00 AM local");
        }

        // ==================== PERSISTENCE ====================

        private void SaveWishlist()
        {
            PlayerPrefs.SetString(PREFS_KEY, string.Join(",", _wishlist));
            PlayerPrefs.Save();
        }

        private void LoadWishlist()
        {
            _wishlist.Clear();
            string saved = PlayerPrefs.GetString(PREFS_KEY, "");
            if (string.IsNullOrEmpty(saved)) return;

            foreach (var id in saved.Split(','))
            {
                if (!string.IsNullOrEmpty(id))
                    _wishlist.Add(id);
            }

            Debug.Log($"[WishlistService] Loaded {_wishlist.Count} wishlisted items");
        }
    }
}
