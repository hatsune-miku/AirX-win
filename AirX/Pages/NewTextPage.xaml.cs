// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using AirX.Util;
using AirX.View;
using CommunityToolkit.Mvvm.Input;
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
    public partial class NewTextPage : Page
    {
        private NewTextViewModel ViewModel = new();
        private NewTextWindow _instance;

        public NewTextPage()
        {
            this.InitializeComponent();
        }

        public void SetWindowInstance(NewTextWindow instance)
        {
            this._instance = instance;
        }

        public void UpdateInformation(string title, string source)
        {
            ViewModel.Title = title;
            ViewModel.From = source;
        }

        [RelayCommand]
        private void Block()
        {
            AccountUtil.AddToBlockList(ViewModel.From);
            _instance.Close();
        }

        [RelayCommand]
        private void Cancel()
        {

        }

        private void OnBlockClicked(object sender, RoutedEventArgs e)
        {
            _ = new ContentDialog()
            {
                Title = "Blocking " + ViewModel.From,
                Content = "Are you sure to block " + ViewModel.From + "?",
                PrimaryButtonText = "Block",
                SecondaryButtonText = "Cancel",
                PrimaryButtonCommand = BlockCommand,
                SecondaryButtonCommand = CancelCommand,
                XamlRoot = Content.XamlRoot,
            }.ShowAsync();
        }
    }
}
