// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using AirX.Extension;
using AirX.Util;
using AirX.View;
using AirX.ViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SRCounter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AirX.View
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TrayIconHolderWindow : Window
    {
        private static AirXBridge.OnTextReceivedHandler Handler = OnTextReceived;
        private static SynchronizationContext context;

        public TrayIconHolderWindow()
        {
            this.InitializeComponent();

            context = SynchronizationContext.Current;

            TrySignInAsync().LogOnError();
            AirXBridge.TryStartAirXService();
            AirXBridge.SetOnTextReceivedHandler(Handler);
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
