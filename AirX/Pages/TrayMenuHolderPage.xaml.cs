using AirX.Bridge;
using AirX.Extension;
using AirX.Model;
using AirX.Util;
using AirX.View;
using AirX.ViewModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace AirX.Pages
{
    public sealed partial class TrayMenuHolderPage : Page
    {
        private GlobalViewModel ViewModel = GlobalViewModel.Instance;
        private SynchronizationContext context = SynchronizationContext.Current;

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

        [RelayCommand]
        public void SendFile()
        {
            OpenFileDialogAsync().ContinueWith(t =>
            {
                var files = t.Result;
                if (files.Count == 0)
                {
                    return;
                }

                if (AirXBridge.GetPeers().Count == 0)
                {
                    context.Post((_) =>
                    {
                        UIUtil.MessageBoxAsync("Error", "No peers available", "OK", null)
                            .LogOnError();
                    }, null);
                    return;
                }
                context.Post((_) =>
                {
                    var window = new SelectPeerWindow();
                    window.SelectPeers((peerItem) =>
                    {
                        foreach (var file in files)
                        {
                            AirXBridge.TrySendFile(file.Path, peerItem.Value);
                        }
                    });
                }, null);
            }, TaskScheduler.FromCurrentSynchronizationContext()).LogOnError();
        }

        private async Task<IReadOnlyList<StorageFile>> OpenFileDialogAsync()
        {
            var filePicker = new FileOpenPicker();
            filePicker.SuggestedStartLocation = PickerLocationId.Desktop;
            filePicker.FileTypeFilter.Add("*");
            filePicker.CommitButtonText = "Send";

            var hwnd = WindowNative.GetWindowHandle(TrayIconHolderWindow.Instance);
            InitializeWithWindow.Initialize(filePicker, hwnd);

            return new List<StorageFile>(await filePicker.PickMultipleFilesAsync());
        }
    }
}
