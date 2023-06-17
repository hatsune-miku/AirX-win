using AirX.Services;
using AirX.Util;
using AirX.View;
using AirX.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace AirX.Pages
{
    public sealed partial class LoginPage : Page
    {
        public LoginWindowViewModel ViewModel { get; set; }

        public LoginPage()
        {
            InitializeComponent();

            ViewModel = new LoginWindowViewModel();
            textBoxUid.Focus(FocusState.Programmatic);
        }

        private async void onLoginButtonClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsLoggingIn)
            {
                return;
            }

            if (ViewModel.Uid.Trim() == "" || ViewModel.Password.Trim() == "")
            {
                return;
            }

            ViewModel.IsLoggingIn = true;
            AirXCloud.LoginResponse response;
            try
            {
                response = await AirXCloud.LoginAsync(ViewModel.Uid, ViewModel.Password);
                if (!response.success)
                {
                    UIUtil.ShowContentDialog(
                        "Error", "Login failed: " + response.message,
                        Content.XamlRoot
                    );
                    return;
                }
            }
            catch (AirXCloud.UnauthorizedException)
            {
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
            SettingsUtil.Write(DefaultKeys.SavedUid, ViewModel.Uid);
            SettingsUtil.Write(DefaultKeys.LoggedInUid, ViewModel.Uid);
            SettingsUtil.Write(DefaultKeys.SavedCredential, response.token);
            SettingsUtil.Write(DefaultKeys.SavedCredentialType, CredentialType.AirXToken);
            GlobalViewModel.Instance.LoggingInUid = ViewModel.Uid;
            GlobalViewModel.Instance.IsSignedIn = true;

            SettingsUtil.Write(
                DefaultKeys.ShouldAutoSignIn,
                ViewModel.ShouldRememberPassword
            );

            await AccountUtil.TryGreetings();

            LoginWindow.Instance?.Close();
        }

        private void onTextBoxesKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != Windows.System.VirtualKey.Enter)
            {
                return;
            }
            onLoginButtonClicked(sender, null);
        }
    }
}
