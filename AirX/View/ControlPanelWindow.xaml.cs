// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using WinRT.Interop;

namespace AirX.View
{
    public partial class ControlPanelWindow : BaseWindow
    {
        protected LoginWindowViewModel ViewModel;

        public static ControlPanelWindow Instance { get; private set; }

        public ControlPanelWindow()
        {
            this.InitializeComponent();
            this.ViewModel = new LoginWindowViewModel();
            Instance = this;

            PrepareWindow(
                new PrepareWindowParameters
                {
                    Title = "Control Panel",
                    Width = 1420,
                    Height = 930,
                    CenterScreen = true,
                    TopMost = false,
                    Resizable = true,
                    HaveMaximumButton = false,
                    HaveMinimumButton = true,
                    EnableMicaEffect = true,
                }
            );
            SetTitleBar(titleBar);
        }
    }
}
