using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT.Interop;
using AirX.Util;
using AirX.View;
using AirX.ViewModel;
using System.Windows.Xps.Serialization;

namespace AirX.Pages
{
    public sealed partial class ConfigurationPage : Page
    {
        ConfigurationViewModel ViewModel = new();

        public ConfigurationPage()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
        private void CopyText_Click(object sender, RoutedEventArgs args)
        {
            var package = new DataPackage();
            package.SetText("Copy this text");
            Clipboard.SetContent(package);
        }

        private async void PasteText_Click(object sender, RoutedEventArgs args)
        {
            var package = Clipboard.GetContent();
            if (package.Contains(StandardDataFormats.Text))
            {
                var text = await package.GetTextAsync();
            }
        }

        private async void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            var window = ControlPanelWindow.Instance;
            var hwnd = WindowNative.GetWindowHandle(window);
            InitializeWithWindow.Initialize(folderPicker, hwnd);

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                this.textBlock.Text = "Picked folder: " + folder.Name;
            }
            else
            {
                this.textBlock.Text = "Operation cancelled.";
            }
        }

        private bool IsPortValid(int port)
        {
            return port == 0 || (1024 < port && port < 65535);
        }

        private bool IsIpV4AddressValid(string address)
        {
            var parts = address.Split(':');
            return (parts.Length == 4 
                && parts.All(p => int.TryParse(p, out int res) && res >= 0 && res <= 255));
        }

        private bool IsGroupIdentityValid(int groupIdentity)
        {
            return 0 <= groupIdentity && groupIdentity <= 255;
        }

        // TODO: !
        private void OnLanDiscoveryServerPortSaved(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(ViewModel.LanDiscoveryServerPort, out int port) || !IsPortValid(port))
            {
                UIUtil.ShowContentDialog("Error", "Invalid port value.", Content.XamlRoot);
                return;
            }
            SettingsUtil.Write(DefaultKeys.DiscoveryServiceServerPort, port);
        }

        private void OnLanDiscoveryClientPortSaved(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(ViewModel.LanDiscoveryClientPort, out int port) || !IsPortValid(port))
            {
                UIUtil.ShowContentDialog("Error", "Invalid port value.", Content.XamlRoot);
                return;
            }
            SettingsUtil.Write(DefaultKeys.DiscoveryServiceClientPort, port);
        }

        private void OnDataServiceListenAddressSaved(object sender, RoutedEventArgs e)
        {
            if (!IsIpV4AddressValid(ViewModel.DataServiceAddressIpV4))
            {
                UIUtil.ShowContentDialog("Error", "Invalid IpV4 address.", Content.XamlRoot);
                return;
            }
            SettingsUtil.Write(DefaultKeys.DataServiceAddressIpV4, ViewModel.DataServiceAddressIpV4);
        }

        private void OnDataServiceListenPortSaved(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(ViewModel.DataServiceAddressIpV4, out int port) || !IsPortValid(port))
            {
                UIUtil.ShowContentDialog("Error", "Invalid port value.", Content.XamlRoot);
                return;
            }
            SettingsUtil.Write(DefaultKeys.DiscoveryServiceClientPort, port);
        }

        private void OnGroupIdentitySaved(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(ViewModel.GroupIdentity, out int gi) || !IsGroupIdentityValid(gi))
            {
                UIUtil.ShowContentDialog("Error", "Invalid group identity.", Content.XamlRoot);
                return;
            }
            SettingsUtil.Write(DefaultKeys.GroupIdentity, gi);
        }
    }
}
