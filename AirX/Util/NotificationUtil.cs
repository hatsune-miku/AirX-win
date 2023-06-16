using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using System.Diagnostics;
using Windows.UI.Notifications;

namespace SRCounter;

public class NotificationUtil
{
    public static void ShowNotification(string content)
    {
        AppNotificationManager.Default.NotificationInvoked += (AppNotificationManager sender, AppNotificationActivatedEventArgs args) =>
        {
        };
        var manager = AppNotificationManager.Default;
        manager.Register();

        Debug.WriteLine($"Notification: {content}");

        manager.Show(
            new AppNotificationBuilder()
                .AddText(content)
                .SetDuration(AppNotificationDuration.Long)
                .AddButton(new AppNotificationButton("OK").AddArgument("action", "OK"))
            .BuildNotification()
        );
    }
}
