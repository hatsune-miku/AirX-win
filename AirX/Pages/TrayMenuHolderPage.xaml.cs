using AirX.Bridge;
using AirX.Extension;
using AirX.Model;
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
#pragma warning disable VSTHRD200
        [RelayCommand]
        public async Task SendFile()
        {
            var files = await OpenFileDialogAsync();
            if (files.Count == 0)
            {
                return;
            }

            if (AirXBridge.GetPeers().Count == 0)
            {
                UIUtil.MessageBoxAsync("Error", "No peers available", "OK", null)
                    .LogOnError();
                return;
            }

            var window = SelectPeerWindow.Instance;
            List<Peer> peers = await window.SelectPeersAsync();

            foreach (var peer in peers)
            {
                foreach (var file in files)
                {
                    AirXBridge.TrySendFile(file.Path, peer);
                }
            }
        }

        private async Task<IReadOnlyList<StorageFile>> OpenFileDialogAsync()
        {
            var filePicker = new FileOpenPicker();
            filePicker.SuggestedStartLocation = PickerLocationId.Desktop;
            filePicker.FileTypeFilter.Add("*");
            filePicker.CommitButtonText = "Send";

            var hwnd = WindowNative.GetWindowHandle(TrayIconHolderWindow.Instance);
            InitializeWithWindow.Initialize(filePicker, hwnd);

            return new List<StorageFile>() { await filePicker.PickSingleFileAsync() };
        }
    }
}
