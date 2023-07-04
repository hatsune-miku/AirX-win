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
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace AirX.Pages
{
    public sealed partial class TrayMenuHolderPage : Page
    {
        private AboutWindow aboutWindow = null;

        public TrayMenuHolderPage()
        {
            this.InitializeComponent();
        }

        [RelayCommand]
        public void ExitApplication()
        {
            UIUtil.MessageBoxAsync("Exit", "Confirm to exit AirX?", "Exit", "Cancel")
                .ContinueWith(t =>
                {
                    if (t.Result == ContentDialogResult.Primary)
                    {
                        Application.Current.Exit();
                    }
                }, TaskScheduler.Default)
                .FireAndForget();
        }

        [RelayCommand]
        public void ShowAboutAirX()
        {
            if (aboutWindow != null)
            {
                aboutWindow.Close();
            }
            aboutWindow = new AboutWindow();
            aboutWindow.Activate();
        }

        [RelayCommand]
        public void ToggleService()
        {
            AirXUtil.UserToggleService();
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
            AccountUtil.UserToggleSignInOut();
        }

        [RelayCommand]
        public void SendFile()
        {
            AirXUtil.UserSendFileAsync()
                .FireAndForget();
        }
    }
}
