using AirX.Bridge;
using AirX.Extension;
using AirX.Util;
using AirX.View;
using AirX.ViewModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;

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

        [RelayCommand]
        public void SendFile()
        {
            var path = "D:\\test.txt";
            var peers = AirXBridge.GetPeers();
            if (peers.Count == 0)
            {
                UIUtil.MessageBoxAsync(path, "No peers available", "OK", null)
                    .LogOnError();
                return;
            }

            var peer = peers.First();
            UIUtil.MessageBoxAsync(path, "Peer: " + peer.Hostname + ", Send?", "Send", "Cancel")
                .ContinueWith(t =>
                {
                    if (t.Result == ContentDialogResult.Primary)
                    {
                        AirXBridge.TrySendFile(path, peer);
                    }
                }, TaskScheduler.Default)
                .LogOnError();
        }
    }
}
