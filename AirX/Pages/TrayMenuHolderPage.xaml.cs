using AirX.Bridge;
using AirX.Extension;
using AirX.Util;
using AirX.View;
using AirX.ViewModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace AirX.Pages
{
    public sealed partial class TrayMenuHolderPage : Page
    {
        private GlobalViewModel ViewModel = GlobalViewModel.Instance;

        public TrayMenuHolderPage()
        {
            this.InitializeComponent();
        }

        [RelayCommand]
        public void ExitApplication()
        {
            Environment.Exit(0);
        }

        [RelayCommand]
        public void ShowAboutAirX()
        {
            var window = new AboutWindow();
            window.Activate();
        }

        [RelayCommand]
        public void ToggleService()
        {
            if (ViewModel.IsServiceOnline)
            {
                AirXBridge.TryStopAirXService();
            }
            else
            {
                AirXBridge.TryStartAirXService();
            }
        }

        [RelayCommand]
        public void OpenControlPanel()
        {
            var window = new ControlPanelWindow();
            window.Activate();
        }

        [RelayCommand]
        public void ToggleSignInOut()
        {
            if (ViewModel.IsSignedIn)
            {
                ViewModel.IsSignedIn = false;
                AccountUtil.ClearSavedUserInfoAndSignOut();
                return;
            }

            var window = new LoginWindow();
            window.Activate();
        }

// RelayCommand加上Async后缀的话会很奇怪
#pragma warning disable VSTHRD100
        [RelayCommand]
        public async void SendFile()
        {
            var files = await OpenFileDialogAsync();
            if (files.Count == 0)
            {
                return;
            }

            var peers = AirXBridge.GetPeers();
            if (peers.Count == 0)
            {
                UIUtil.MessageBoxAsync("Error", "No peers available", "OK", null)
                    .LogOnError();
                return;
            }

            var result = await UIUtil.MessageBoxAsync("Send files", "Send to all " + peers.Count + " peer(s)?", "Send!", "Cancel");
            if (result == ContentDialogResult.Primary)
            {
                foreach (var peer in peers)
                {
                    foreach (var file in files)
                    {
                        AirXBridge.TrySendFile(file.Path, peer);
                    }
                }
            }
        }

        private async Task<IReadOnlyList<StorageFile>> OpenFileDialogAsync()
        {
            var filePicker = new FileOpenPicker();
            filePicker.SuggestedStartLocation = PickerLocationId.Desktop;
            filePicker.FileTypeFilter.Add("*.*");
            filePicker.CommitButtonText = "Send";

            var hwnd = WindowNative.GetWindowHandle(TrayIconHolderWindow.Instance);
            InitializeWithWindow.Initialize(filePicker, hwnd);

            return await filePicker.PickMultipleFilesAsync();
        }
    }
}
