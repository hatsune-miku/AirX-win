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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AirX.View
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ControlPanelWindow : BaseWindow
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
