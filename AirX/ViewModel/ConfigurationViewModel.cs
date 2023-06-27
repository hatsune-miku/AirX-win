using AirX.Model;
using AirX.Util;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AirX.ViewModel
{
    public partial class ConfigurationViewModel : ObservableObject
    {
        [ObservableProperty]
        List<SettingsItem> settingsItems = new();

        [ObservableProperty]
        bool isUnsaved = false;

        [ObservableProperty]
        bool shouldShowAdvancedSettings = SettingsUtil.Bool(Keys.ShouldShowAdvancedSettings, false);
    }
}
