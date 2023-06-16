// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AirX.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NewTextPage : Page
    {
        private NewTextViewModel ViewModel = new NewTextViewModel();

        public NewTextPage(string title, string from)
        {
            ViewModel.Title = title;
            ViewModel.From = "Text from " + from;

            this.InitializeComponent();
        }
        public static void Popup(string title, string from)
        {
            NewTextPage page = new NewTextPage(title, from);
            Window window = new Window();

            Frame root = new Frame();
            root.Content = page;
            window.Content = root;

            window.Activate();
            page.AdjustWindow(window);
        }

        private void HandleMessageReceived(string message)
        {
        }

        private void AdjustWindow(Window window)
        {
            IntPtr hwnd = WindowNative.GetWindowHandle(window);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

            int width = 640;
            int height = 480;

            appWindow.Resize(new Windows.Graphics.SizeInt32(width, height));
            appWindow.Move(
                new Windows.Graphics.PointInt32(
                    (int)(GetActualScreenWidth() - width),
                    (int)(GetActualScreenHeight() - height - 48)
                    )
                );
        }

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern bool ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        private int GetActualScreenWidth()
        {
            return (int)(GetSystemMetrics(0));
        }
        private int GetActualScreenHeight()
        {
            return (int)(GetSystemMetrics(1));
        }

        private double GetDpiX()
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            int ret = GetDeviceCaps(hdc, 88);
            ReleaseDC(IntPtr.Zero, hdc);
            return ret / 100.0;
        }

        private double GetDpiY()
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            int ret = GetDeviceCaps(hdc, 90);
            ReleaseDC(IntPtr.Zero, hdc);
            return ret / 100.0;
        }


    }
}
