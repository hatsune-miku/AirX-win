using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT.Interop;
using AirX.Util;
using AirX.View;
using AirX.ViewModel;
using AirX.Model;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;

namespace AirX.Pages
{
    public sealed partial class ConfigurationPage : Page
    {
        ConfigurationViewModel ViewModel = new();
        public ConfigurationPage()
        {
            this.InitializeComponent();

            ViewModel.SettingsItems.Add(new Model.SettingsItem
            {
                Title = "LAN Discovery Server Port",
                Description = "1024 ~ 65535",
                SettingsKey = Keys.DiscoveryServiceServerPort,
                Validator = IsPortValid,
                XamlRoot = Content.XamlRoot,
                ViewModel = ViewModel,
            });

            ViewModel.SettingsItems.Add(new Model.SettingsItem
            {
                Title = "LAN Discovery Client Port",
                Description = "1024 ~ 65535",
                SettingsKey = Keys.DiscoveryServiceClientPort,
                Validator = IsPortValid,
                XamlRoot = Content.XamlRoot,
                ViewModel = ViewModel,
            });

            ViewModel.SettingsItems.Add(new Model.SettingsItem
            {
                Title = "Data Service Listen Address (IpV4)",
                SettingsKey = Keys.DataServiceAddressIpV4,
                Validator = IsIpV4AddressValid,
                XamlRoot = Content.XamlRoot,
                ViewModel = ViewModel,
            });

            ViewModel.SettingsItems.Add(new Model.SettingsItem
            {
                Title = "Data Service Listen Port",
                Description = "1024 ~ 65535",
                SettingsKey = Keys.DataServiceListenPort,
                Validator = IsPortValid,
                XamlRoot = Content.XamlRoot,
                ViewModel = ViewModel,
            });

            ViewModel.SettingsItems.Add(new Model.SettingsItem
            {
                Title = "Group Identity",
                Description = "0 ~ 255. Only devices with the same group identity can discover each other.",
                SettingsKey = Keys.GroupIdentity,
                Validator = IsGroupIdentityValid,
                XamlRoot = Content.XamlRoot,
                ViewModel = ViewModel,
            });
        }

        public string GetTitle()
        {
            return ViewModel.IsUnsaved
                ? "Preferences - Edited"
                : "Perferences";
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

        private bool IsPortValid(string portRepr)
        {
            return int.TryParse(portRepr, out int port)
                && port == 0 || (1024 < port && port < 65535);
        }

        private bool IsIpV4AddressValid(string address)
        {
            var parts = address.Split(':');
            return (parts.Length == 4 
                && parts.All(p => int.TryParse(p, out int res) && res >= 0 && res <= 255));
        }

        private bool IsGroupIdentityValid(string groupIdentityRepr)
        {
            return int.TryParse(groupIdentityRepr, out int groupIdentity)
                && 0 <= groupIdentity && groupIdentity <= 255;
        }
    }
}
