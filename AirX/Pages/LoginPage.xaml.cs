// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using AirX.Services;
using AirX.Util;
using AirX.View;
using AirX.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AirX.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
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
