﻿using AirX.Extension;
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
            textBoxUid.Focus(FocusState.Programmatic);
        }

        private async Task HandleLoginAsync()
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
            SettingsUtil.Write(Keys.SavedUid, ViewModel.Uid);
            SettingsUtil.Write(Keys.LoggedInUid, ViewModel.Uid);
            SettingsUtil.Write(Keys.SavedCredential, response.token);
            SettingsUtil.Write(Keys.SavedCredentialType, CredentialType.AirXToken);
            GlobalViewModel.Instance.LoggingInUid = ViewModel.Uid;
            GlobalViewModel.Instance.IsSignedIn = true;

            SettingsUtil.Write(
                Keys.ShouldAutoSignIn,
                ViewModel.ShouldRememberPassword
            );

            await AccountUtil.SendGreetingsAsync();

            LoginWindow.Instance?.Close();
        }

        private void OnLoginButtonClicked(object sender, RoutedEventArgs e)
        {
            HandleLoginAsync().FireAndForget();
        }

        private void onTextBoxesKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != Windows.System.VirtualKey.Enter)
            {
                return;
            }
            OnLoginButtonClicked(sender, null);
        }

        private void GoogleLoginButton_Click(object sender, RoutedEventArgs e)
        {
            var _ = googleSignInHelper.TrySignInAsync();
        }
    }
}
