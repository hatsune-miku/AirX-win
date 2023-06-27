// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using AirX.Extension;
using AirX.Util;
using AirX.ViewModel;
using Microsoft.UI.Xaml;
using SRCounter;
using System;
using System.Threading;
using System.Threading.Tasks;
using WinRT.Interop;

namespace AirX.View
{
    public sealed partial class TrayIconHolderWindow : Window
    {
        private static AirXBridge.OnTextReceivedHandler TextHandler = OnTextReceived;
        private static AirXBridge.OnFileComingHandler FileHandler = OnFileComing;
        private static AirXBridge.OnFileSendingHandler FileSendingHandler = OnFileSending;
        private static AirXBridge.OnFilePartHandler FilePartHandler = OnFilePart;

        private static SynchronizationContext context;

        public TrayIconHolderWindow()
        {
            this.InitializeComponent();

            context = SynchronizationContext.Current;

            TrySignInAsync().LogOnError();
            AirXBridge.TryStartAirXService();
            AirXBridge.SetOnTextReceivedHandler(TextHandler);

            AppWindow.Resize(new(1, 1));
            AppWindow.Move(new(32768, 32768));
        }

        private async Task TrySignInAsync()
        {
            if (!await AccountUtil.TryAutomaticLogin())
            {
                var window = new LoginWindow();
                window.Activate();
                return;
            }
            NotificationUtil.ShowNotification(
                "Welcome back, " + GlobalViewModel.Instance.LoggingGreetingsName + "!");
        }

        private static void OnFilePart(byte fileId, uint offset, uint length, byte[] data)
        {
            
        }

        private static void OnFileSending(byte fileId, ulong progress, ulong total, AirXBridge.FileStatus status)
        {

        }

        private static void OnFileComing(ulong fileSize, string fileName, string from)
        {

        }

        private static void OnTextReceived(string text, string source)
        {
            if (AccountUtil.IsInBlockList(source))
            {
                return;
            }

            context.Post(_ =>
            {
                var window = NewTextWindow.InstanceOf(text, source);
                window.Activate();
            }, null);
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            IntPtr hwnd = WindowNative.GetWindowHandle(this);
            PInvoke.User32.ShowWindow(hwnd, PInvoke.User32.WindowShowStyle.SW_HIDE);
        }
    }
}
