using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using System.Diagnostics;
using Windows.UI.Notifications;

namespace SRCounter;

/// <summary>
/// 通知工具类
/// </summary>
public class NotificationUtil
{
    private static bool _didManagerInitialized = false;

    /// <summary>
    /// 在屏幕右下角弹出通知消息
    /// </summary>
    public static void ShowNotification(string content)
    {
        var manager = AppNotificationManager.Default;

        if (!_didManagerInitialized)
        {
            AppNotificationManager.Default.NotificationInvoked += (AppNotificationManager sender, AppNotificationActivatedEventArgs args) =>
            {
            };
            manager.Register();
            _didManagerInitialized = true;
        }

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
