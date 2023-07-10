﻿using AirX.Bridge;
using AirX.ViewModel;

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
                AirXVersion = AirXNative.airx_version().ToString(),
                AirXVersionString = AirXBridge.GetVersionString(),
                BuildValue = "2",
                VersionValue = "1.1",
                Copyright = "© 2023 Chang Guan",
            };

            PrepareWindow(
                new WindowParameters
                {
                    Title = "About AirX",
                    WidthPortion = 890 / 3840.0 * 1.25,
                    HeightPortion = 525 / 2160.0 * 1.25,
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
