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

namespace AirX.Util
{
    public static class UIUtil
    {
        public static AppWindow GetAppWindow(Window window)
        {
            var hWnd = WindowNative.GetWindowHandle(window);
            var wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(wndId);
        }

        public static void MoveWindowToCenterScreen(AppWindow window)
        {
            var point = CalculateCenterScreenPoint(
                window.Size.Width, window.Size.Height);
            window.Move(new PointInt32((int) point.X, (int) point.Y));
        }

        public static Size GetPrimaryScreenSize()
        {
            int screenWidth = PInvoke.User32.GetSystemMetrics(PInvoke.User32.SystemMetric.SM_CXSCREEN);
            int screenHeight = PInvoke.User32.GetSystemMetrics(PInvoke.User32.SystemMetric.SM_CYSCREEN);
            return new Size(screenWidth, screenHeight);
        }

        public static async void ShowContentDialog(string title, string content, XamlRoot xamlRoot)
        {
            _ = await new ContentDialog()
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = xamlRoot
            }.ShowAsync();
        }

        public static void SetWindowVisibility(Window window, bool visible)
        {
            PInvoke.User32.ShowWindow(
                WindowNative.GetWindowHandle(window),
                visible
                    ? PInvoke.User32.WindowShowStyle.SW_SHOW
                    : PInvoke.User32.WindowShowStyle.SW_HIDE
            );
        }

        public static Point CalculateCenterScreenPoint(int width, int height)
        {
            var size = GetPrimaryScreenSize();
            return new Point
            {
                X = size.Width / 2 - width / 2,
                Y = size.Height / 2 - height / 2
            };
        }

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
