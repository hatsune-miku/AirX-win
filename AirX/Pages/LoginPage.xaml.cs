using AirX.Extension;
using AirX.Helper;
using AirX.Services;
using AirX.Util;
using AirX.View;
using AirX.ViewModel;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;

namespace AirX.Pages
{
    public sealed partial class LoginPage : Page
    {
        public LoginWindowViewModel ViewModel { get; set; }
        private GoogleSignInHelper googleSignInHelper = new();

        public LoginPage()
        {
            InitializeComponent();

            ViewModel = new LoginWindowViewModel();

            // 自动选中用户名的输入框方便用户输入
            textBoxUid.Focus(FocusState.Programmatic);
        }

        // 进行登陆操作
        private async Task HandleLoginAsync()
        {
            // 已经在登陆中了，防止重复点击
            if (ViewModel.IsLoggingIn)
            {
                return;
            }

            // 检查用户名和密码是否为空
            if (ViewModel.Uid.Trim() == "" || ViewModel.Password.Trim() == "")
            {
                return;
            }

            ViewModel.IsLoggingIn = true;
            AirXCloud.LoginResponse response;
            try
            {
                // 正式发送登录请求
                response = await AirXCloud.LoginAsync(ViewModel.Uid, ViewModel.Password);
                if (!response.success)
                {
                    // 失败的话，弹出错误提示
                    UIUtil.ShowContentDialog(
                        "Error", "Login failed: " + response.message,
                        Content.XamlRoot
                    );
                    return;
                }
            }
            catch (AirXCloud.UnauthorizedException)
            {
                // 这个分支不可能出现，因为登录本身就不会验证token，因此401 Unauthorized无从谈起
                UIUtil.ShowContentDialog(
                    "Error", "Login failed: unknown reason.",
                    Content.XamlRoot
                );
                return;
            }
            finally
            {
                ViewModel.IsLoggingIn = false;
            }

            // Success
            /// 登录成功，保存用户名和token到本地
            SettingsUtil.Write(Keys.SavedUid, ViewModel.Uid);
            SettingsUtil.Write(Keys.LoggedInUid, ViewModel.Uid);
            SettingsUtil.Write(Keys.SavedCredential, response.token);
            SettingsUtil.Write(Keys.SavedCredentialType, CredentialType.AirXToken);
            GlobalViewModel.Instance.LoggingInUid = ViewModel.Uid;
            GlobalViewModel.Instance.IsSignedIn = true;

            /// 保存是否自动登录的设置
            SettingsUtil.Write(
                Keys.ShouldAutoSignIn,
                ViewModel.ShouldRememberPassword
            );

            /// 发送Greetings请求验证token有效性的同时获取用户信息
            await AccountUtil.SendGreetingsAsync();

            /// 完事儿后登陆窗口关闭
            LoginWindow.Instance?.Close();
        }

        private void OnLoginButtonClicked(object sender, RoutedEventArgs e)
        {
            HandleLoginAsync().FireAndForget();
        }

        /// <summary>
        /// 回车键按下时，自动点击登录按钮
        /// </summary>
        private void onTextBoxesKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != Windows.System.VirtualKey.Enter)
            {
                return;
            }
            OnLoginButtonClicked(sender, null);
        }

        /// <summary>
        /// 谷歌要求的登录按钮的相关操作
        /// </summary>
        private void GoogleLoginButton_Click(object sender, RoutedEventArgs e)
        {
            var _ = googleSignInHelper.TrySignInAsync();
        }
    }
}
