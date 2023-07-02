using AirX.Services;
using AirX.Util;
using AirX.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Newtonsoft.Json.Serialization;
using System;
using System.Drawing;

namespace AirX.View
{
    public sealed partial class LoginWindow : BaseWindow
    {
        public static LoginWindow Instance;

        public string Token { get; set; }

        public LoginWindow()
        {
            this.InitializeComponent();

            Instance = this;
            PrepareWindow(
                new WindowParameters
                {
                    Title = "AirX Login",
                    WidthPortion = 827 / 3840.0 * 1.25,
                    HeightPortion = 758 / 2160.0 * 1.25,
                    CenterScreen = true,
                    TopMost = false,
                    Resizable = false,
                    HaveMaximumButton = false,
                    HaveMinimumButton = true,
                    EnableMicaEffect = true,
                }
            );
            SetTitleBar(titleBar);
        }
    }
}
