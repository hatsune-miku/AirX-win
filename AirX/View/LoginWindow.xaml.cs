// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using AirX.Services;
using AirX.Util;
using AirX.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Newtonsoft.Json.Serialization;
using System;
/**
* MVVM: Model View ViewModel 
*/

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AirX.View
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginWindow : BaseWindow
    {
        public static LoginWindow Instance;

        public string Token { get; set; }

        public LoginWindow()
        {
            this.InitializeComponent();

            Instance = this;
            PrepareWindow(
                new PrepareWindowParameters
                {
                    Title = "AirX Login",
                    Width = 827,
                    Height = 758,
                    CenterScreen = true,
                    TopMost = true,
                    Resizable = false,
                    HaveMaximumButton = false,
                    HaveMinimumButton = true,
                }
            );
        }
    }
}
