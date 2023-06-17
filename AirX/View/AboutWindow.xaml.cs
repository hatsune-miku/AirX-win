using AirX.ViewModel;
using Microsoft.UI;
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
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;

namespace AirX.View
{
    public sealed partial class AboutWindow : BaseWindow
    {
        private readonly AboutViewModel ViewModel;

        public AboutWindow()
        {
            this.InitializeComponent();

            ViewModel = new()
            {
                AirXVersion = AirXBridge.airx_version().ToString(),
                BuildValue = "1",
                VersionValue = "1.0.0",
                Copyright = "© 2023 Chang Guan",
            };

            PrepareWindow(
                new PrepareWindowParameters
                {
                    Title = "About AirX",
                    Width = 890,
                    Height = 525,
                    CenterScreen = true,
                    TopMost = true,
                    Resizable = false,
                    HaveMaximumButton = false,
                    HaveMinimumButton = false,
                    EnableMicaEffect = true,
                }
            );

            SetTitleBar(titleBar);
        }
    }
}
