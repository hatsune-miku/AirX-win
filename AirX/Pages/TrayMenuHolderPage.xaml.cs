using AirX.Util;
using AirX.View;
using AirX.ViewModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System;

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
    }
}
