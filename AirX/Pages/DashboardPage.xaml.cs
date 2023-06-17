// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using AirX.Services;
using AirX.Util;
using AirX.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace AirX.Pages
{
    public sealed partial class DashboardPage : Page
    {
        private DashboardPageViewModel ViewModel = new DashboardPageViewModel();
        private GlobalViewModel GlobalViewModel = GlobalViewModel.Instance;

        public DashboardPage()
        {
            ViewModel.AirXVersion = AirXBridge.airx_version().ToString();
            this.InitializeComponent();
        }


        private void OnConnectClicked(object sender, RoutedEventArgs e)
        {
        }
    }
}
