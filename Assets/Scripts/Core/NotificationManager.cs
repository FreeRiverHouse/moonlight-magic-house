using System;
using UnityEngine;

namespace MoonlightMagicHouse
{
    // Schedules local push notifications for pet needs reminders.
    public class NotificationManager : MonoBehaviour
    {
        const string PREF_NOTIF = "notif_enabled";

        public static bool Enabled
        {
            get  => PlayerPrefs.GetInt(PREF_NOTIF, 1) == 1;
            set  => PlayerPrefs.SetInt(PREF_NOTIF, value ? 1 : 0);
        }

        void OnApplicationPause(bool paused)
        {
            if (paused && Enabled) ScheduleReminders();
            else                   CancelAll();
        }

        void ScheduleReminders()
        {
            CancelAll();
#if UNITY_IOS
            ScheduleIOS("Your pet is hungry! 🍽️",         "Come back and feed them!", hours: 3);
            ScheduleIOS("Your pet misses you! 💫",          "Visit the Moonlight House!", hours: 8);
            ScheduleIOS("Don't break your streak! 🌙",     "Log in for your daily reward.", hours: 23);
#elif UNITY_ANDROID
            ScheduleAndroid("Your pet is hungry! 🍽️",      "Come back and feed them!", hours: 3);
            ScheduleAndroid("Your pet misses you! 💫",       "Visit the Moonlight House!", hours: 8);
            ScheduleAndroid("Don't break your streak! 🌙",  "Log in for your daily reward.", hours: 23);
#endif
        }

        void CancelAll()
        {
#if UNITY_IOS
            Unity.Notifications.iOS.iOSNotificationCenter.RemoveAllScheduledNotifications();
#elif UNITY_ANDROID
            Unity.Notifications.Android.AndroidNotificationCenter.CancelAllScheduledNotifications();
#endif
        }

#if UNITY_IOS
        void ScheduleIOS(string title, string body, int hours)
        {
            var trigger = new Unity.Notifications.iOS.iOSNotificationTimeIntervalTrigger
            {
                TimeInterval = TimeSpan.FromHours(hours),
                Repeats = false
            };
            var notification = new Unity.Notifications.iOS.iOSNotification
            {
                Title    = title,
                Body     = body,
                Trigger  = trigger,
                ShowInForeground = false
            };
            Unity.Notifications.iOS.iOSNotificationCenter.ScheduleNotification(notification);
        }
#endif

#if UNITY_ANDROID
        void ScheduleAndroid(string title, string body, int hours)
        {
            var notification = new Unity.Notifications.Android.AndroidNotification
            {
                Title       = title,
                Text        = body,
                FireTime    = DateTime.Now.AddHours(hours),
                SmallIcon   = "icon_0",
                LargeIcon   = "icon_1"
            };
            Unity.Notifications.Android.AndroidNotificationCenter
                .SendNotification(notification, "moonlight_channel");
        }
#endif
    }
}
