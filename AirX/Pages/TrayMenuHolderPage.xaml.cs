using AirX.Extension;
using AirX.Util;
using AirX.View;
using AirX.ViewModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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

        /// <summary>
        /// 退出菜单项
        /// </summary>
        [RelayCommand]
        public void ExitApplication()
        {
            Application.Current.Exit();
        }

        /// <summary>
        /// 关于菜单项
        /// </summary>
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

        /// <summary>
        /// 开关服务菜单项
        /// </summary>
        [RelayCommand]
        public void ToggleService()
        {
            AirXUtil.UserToggleService();
        }

        /// <summary>
        /// 打开控制面板菜单项
        /// </summary>
        [RelayCommand]
        public void OpenControlPanel()
        {
            var window = new ControlPanelWindow();
            window.Activate();
        }

        /// <summary>
        /// 登录/登出菜单项
        /// </summary>
        [RelayCommand]
        public void ToggleSignInOut()
        {
            AccountUtil.UserToggleSignInOut();
        }

        /// <summary>
        /// 发送文件菜单项
        /// </summary>
        [RelayCommand]
        public void SendFile()
        {
            AirXUtil.UserSendFileAsync()
                .FireAndForget();
        }
    }
}
