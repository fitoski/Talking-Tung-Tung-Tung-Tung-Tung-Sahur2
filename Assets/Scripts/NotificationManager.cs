using UnityEngine;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#elif UNITY_IOS
using Unity.Notifications.iOS;
#endif

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        RegisterChannels();
    }

    void RegisterChannels()
    {
#if UNITY_ANDROID
        var channel = new AndroidNotificationChannel
        {
            Id          = "hunger_channel",
            Name        = "Hunger Reminders",
            Importance  = Importance.Default,
            Description = "Reminds you to feed Tung Tung Sahur"
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
#elif UNITY_IOS
        new AuthorizationRequest(
            AuthorizationOption.Alert | AuthorizationOption.Sound, true);
#endif
    }

    public void ScheduleHungerNotification(int hoursDelay = 4)
    {
        string title = "Tung Tung Sahur is hungry!";
        string body = "My tummy’s growling—feed me!";

        if (Application.systemLanguage == SystemLanguage.Turkish)
        {
            title = "Tung Tung Sahur aç kaldı!";
            body = "Karnım acıktı, beni beslemeye ne dersin?";
        }

#if UNITY_ANDROID
        var notif = new AndroidNotification
        {
            Title    = title,
            Text     = body,
            FireTime = System.DateTime.Now.AddHours(hoursDelay)
        };
        AndroidNotificationCenter.SendNotification(notif, "hunger_channel");
#elif UNITY_IOS
        var trigger = new iOSNotificationTimeIntervalTrigger
        {
            TimeInterval = new System.TimeSpan(hours: hoursDelay, minutes: 0, seconds: 0),
            Repeats      = false
        };
        var iosNotif = new iOSNotification
        {
            Title            = title,
            Body             = body,
            ShowInForeground = true,
            Trigger          = trigger
        };
        iOSNotificationCenter.ScheduleNotification(iosNotif);
#endif
    }
}
