using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;
using WinRT.Interop;
using Windows.Foundation;
using Windows.Graphics;
using AirX.View;
using Microsoft.UI.Xaml.Controls;
using AirX.Extension;

namespace AirX.Util
{
    /// <summary>
    /// /// UI相关工具函数
    /// </summary>
    public static class UIUtil
    {
        /// <summary>
        /// 从WinUI3 Window对象得到对应的AppWindow对象
        /// 两种Window功能不同
        /// </summary>
        public static AppWindow GetAppWindow(Window window)
        {
            var hWnd = WindowNative.GetWindowHandle(window);
            var wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(wndId);
        }

        /// <summary>
        /// 把AppWindow移动到屏幕正中央
        /// </summary>
        public static void MoveWindowToCenterScreen(AppWindow window)
        {
            var point = CalculateCenterScreenPoint(
                window.Size.Width, window.Size.Height);
            window.Move(new PointInt32((int) point.X, (int) point.Y));
        }

        /// <summary>
        /// 获得屏幕宽度和高度，单位：像素
        /// </summary>
        public static Size GetPrimaryScreenSize()
        {
            int screenWidth = PInvoke.User32.GetSystemMetrics(PInvoke.User32.SystemMetric.SM_CXSCREEN);
            int screenHeight = PInvoke.User32.GetSystemMetrics(PInvoke.User32.SystemMetric.SM_CYSCREEN);
            return new Size(screenWidth, screenHeight);
        }

        /// <summary>
        /// 显示一个消息框，但是只能依托窗口作为宿主才能显示
        /// </summary>
        public static void ShowContentDialog(string title, string content, XamlRoot xamlRoot)
        {
            new ContentDialog()
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = xamlRoot
            }.ShowAsync()
            .AsTask()
            .FireAndForget();
        }

        /// <summary>
        /// 显示一个消息框，但是只能依托窗口作为宿主才能显示。返回用户的选择。
        /// </summary>
        public static async Task<ContentDialogResult> ShowContentDialogYesNoAsync(string title, string content, string primaryButtonText, string secondaryButtonText, XamlRoot xamlRoot)
        {
            return await new ContentDialog()
            {
                Title = title,
                Content = content,
                PrimaryButtonText = primaryButtonText,
                SecondaryButtonText = secondaryButtonText,
                XamlRoot = xamlRoot
            }.ShowAsync().AsTask();
        }

        /// <summary>
        /// 设置一个窗口的可见性
        /// </summary>
        public static void SetWindowVisibility(Window window, bool visible)
        {
            PInvoke.User32.ShowWindow(
                WindowNative.GetWindowHandle(window),
                visible
                    ? PInvoke.User32.WindowShowStyle.SW_SHOW
                    : PInvoke.User32.WindowShowStyle.SW_HIDE
            );
        }

        /// <summary>
        /// 获取电脑屏幕的正中心点的X和Y坐标
        /// </summary>
        public static Point CalculateCenterScreenPoint(int width, int height)
        {
            var size = GetPrimaryScreenSize();
            return new Point
            {
                X = size.Width / 2 - width / 2,
                Y = size.Height / 2 - height / 2
            };
        }

        /// <summary>
        /// 显示一个消息框，其不以窗口作为宿主，能够在无窗口的情况下显示，是真正的消息框。
        /// 原理为其自己自带一个窗口，只不过这个窗口时刻调整自己的大小，变成和消息框一样大。
        /// </summary>
        public static async Task<ContentDialogResult> MessageBoxAsync(
            string title, string content, string primaryButtonText, string secondaryButtonText)
        {
            var window = new MessageBoxWindow(
                title, content, primaryButtonText, secondaryButtonText);
            ContentDialogResult result = await window.ShowAsync();
            return result;
        }
    }
}
