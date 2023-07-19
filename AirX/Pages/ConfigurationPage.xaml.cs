using System.Collections.Generic;
using System.Linq;
using AirX.Util;
using AirX.ViewModel;
using AirX.Model;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using AirX.Extension;

namespace AirX.Pages
{
    /// <summary>
    /// 定义一个设置模板选择器
    /// 根据设置项的类型，选择不同的模板
    /// String类型：显示一个TextBox，用户可以输入字符串
    /// Bool类型：显示一个ToggleSwitch，用户可以切换开关
    /// </summary>
    public class SettingsDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate BooleanSettingsTemplate { get; set; }
        public DataTemplate StringSettingsTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is SettingsItem settingsItem)
            {
                switch (settingsItem.ItemType)
                {
                    case SettingsItemType.String:
                        /// String类型：显示一个TextBox，用户可以输入字符串
                        return StringSettingsTemplate;

                    case SettingsItemType.Boolean:
                        /// Bool类型：显示一个ToggleSwitch，用户可以切换开关
                        return BooleanSettingsTemplate;

                    default:
                        return StringSettingsTemplate;
                }
            }
            return base.SelectTemplateCore(item, container);
        }
    }

    public sealed partial class ConfigurationPage : Page
    {
        List<SettingsItem> SettingsItems = new();
        ConfigurationViewModel ViewModel = new();

        public ConfigurationPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 当页面加载完成后，加载设置项
        /// </summary>
        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            SettingsItems.Add(new Model.SettingsItem
            {
                Title = "LAN Group Identifier",
                Description = "0 ~ 255. Only devices with the same group identity can discover each other.",
                SettingsKey = Keys.GroupIdentifier,
                Validator = IsGroupIdentityValid,
                ItemType = SettingsItemType.String,
                IsAdvanced = false,
            });

            SettingsItems.Add(new Model.SettingsItem
            {
                Title = "Text Popup Display Time (ms)",
                Description = "1000 ~ 10000.",
                SettingsKey = Keys.NewTextPopupDisplayTimeMillis,
                ItemType = SettingsItemType.String,
                Validator = IsMillisTimeValid,
                IsAdvanced = false,
            });

            SettingsItems.Add(new Model.SettingsItem
            {
                Title = "Show Developer Console",
                Description = "Enables debug output from libairx.",
                SettingsKey = Keys.ShouldShowConsole,
                ItemType = SettingsItemType.Boolean,
                IsAdvanced = true,
            });

            SettingsItems.Add(new Model.SettingsItem
            {
                Title = "Kafka Producer",
                Description = "Enable to share clipboard data over the internet.. Does not affect LAN settings.",
                SettingsKey = Keys.IsKafkaProducer,
                ItemType = SettingsItemType.Boolean,
                IsAdvanced = true,
            });

            SettingsItems.Add(new Model.SettingsItem
            {
                Title = "Kafka Consumer",
                Description = "Enable to accept clipboard data from the internet. Does not affect LAN settings.",
                SettingsKey = Keys.IsKafkaConsumer,
                ItemType = SettingsItemType.Boolean,
                IsAdvanced = true,
            });

            SettingsItems.Add(new Model.SettingsItem
            {
                Title = "LAN Discovery Server Port",
                Description = "1024 ~ 65535",
                SettingsKey = Keys.DiscoveryServiceServerPort,
                Validator = IsPortValid,
                ItemType = SettingsItemType.String,
                IsAdvanced = true,
            });

            SettingsItems.Add(new Model.SettingsItem
            {
                Title = "LAN Discovery Client Port",
                Description = "0 or 1024 ~ 65535",
                SettingsKey = Keys.DiscoveryServiceClientPort,
                Validator = IsPortValid,
                ItemType = SettingsItemType.String,
                IsAdvanced = true,
            });

            SettingsItems.Add(new Model.SettingsItem
            {
                Title = "LAN Data Service Listen Address (IpV4)",
                SettingsKey = Keys.DataServiceAddressIpV4,
                Validator = IsIpV4AddressValid,
                ItemType = SettingsItemType.String,
                IsAdvanced = true,
            });

            SettingsItems.Add(new Model.SettingsItem
            {
                Title = "LAN Data Service Listen Port",
                Description = "1024 ~ 65535",
                SettingsKey = Keys.DataServiceListenPort,
                Validator = IsPortValid,
                ItemType = SettingsItemType.String,
                IsAdvanced = true,
            });

            SettingsItems.Add(new Model.SettingsItem
            {
                Title = "AirX Cloud Server",
                Description = "Only connect to private servers you trust.",
                SettingsKey = Keys.AirXCloudAddress,
                ItemType = SettingsItemType.String,
                IsAdvanced = true,
            });

            // 最后给每个设置项设置XamlRoot和ViewModel
            foreach (var item in SettingsItems)
            {
                item.XamlRoot = Content.XamlRoot;
                item.ViewModel = ViewModel;
            }

            // 根据是否显示高级设置，过滤设置项
            ApplyLatestItemFilters();
        }

        public void ApplyLatestItemFilters()
        {
            // 要么开启了高级设置，要么设置项不是高级设置，才显示
            ViewModel.SettingsItems = SettingsItems.Where(
                item => ViewModel.ShouldShowAdvancedSettings || !item.IsAdvanced
            ).ToList();
        }

        // 设置并记忆是否显示高级设置
        public void SetShouldShowAdvancedSettings(bool value)
        {
            ViewModel.ShouldShowAdvancedSettings = value;
            SettingsUtil.Write(Keys.ShouldShowAdvancedSettings, value.ToString().ToLower());
            ApplyLatestItemFilters();
        }

        // 获取是否显示高级设置
        public bool GetShouldShowAdvancedSettings()
        {
            return ViewModel.ShouldShowAdvancedSettings;
        }

        // 根据是否有未保存的设置项，设置标题
        public string GetTitle()
        {
            return ViewModel.IsUnsaved
                ? "Preferences - Edited"
                : "Perferences";
        }

        // 判断端口是否合法
        private bool IsPortValid(string portRepr)
        {
            return int.TryParse(portRepr, out int port)
                && port == 0 || (1024 < port && port < 65535);
        }

        // 判断IpV4地址是否合法
        private bool IsIpV4AddressValid(string address)
        {
            var parts = address.Split('.');
            return (parts.Length == 4 
                && parts.All(p => int.TryParse(p, out int res) && res >= 0 && res <= 255));
        }

        // 判断AirX组标识是否合法
        private bool IsGroupIdentityValid(string groupIdentityRepr)
        {
            return int.TryParse(groupIdentityRepr, out int groupIdentity)
                && 0 <= groupIdentity && groupIdentity <= 255;
        }

        // 判断毫秒时间是否在合法范围内
        private bool IsMillisTimeValid(string millisRepr)
        {
            return int.TryParse(millisRepr, out int res)
                && 1000 <= res && res <= 10000;
        }

        // 清理缓存按钮，目前没有实现
        private void OnCleanCacheClicked(object sender, RoutedEventArgs args)
        {
            // TODO: Clean cache
            UIUtil.ShowContentDialogYesNoAsync("Clean Cache", "This will delete all cached data.",
                "Clean", "Cancel", Content.XamlRoot)
                .FireAndForget();
        }
    }
}
