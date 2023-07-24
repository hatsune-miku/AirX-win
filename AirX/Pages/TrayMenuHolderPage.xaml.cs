using AirX.Extension;
using AirX.Util;
using AirX.View;
using AirX.ViewModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AirX.Pages
{
    public sealed partial class TrayMenuHolderPage : Page
    {
        private AboutWindow aboutWindow = null;
        private GlobalViewModel ViewModel = GlobalViewModel.Instance;

        public TrayMenuHolderPage()
        {
            this.InitializeComponent();
        }

        [RelayCommand]
        public void ExitApplication()
        {
            Process.GetCurrentProcess().Kill();
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
