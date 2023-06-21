using AirX.Util;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirX.ViewModel
{
    public partial class ConfigurationViewModel : ObservableObject
    {
        [ObservableProperty]
        string lanDiscoveryServerPort = SettingsUtil.Int(DefaultKeys.DiscoveryServiceServerPort, 9818).ToString();

        [ObservableProperty]
        string lanDiscoveryClientPort = SettingsUtil.Int(DefaultKeys.DiscoveryServiceClientPort, 0).ToString();

        [ObservableProperty]
        string dataServiceAddressIpV4 = SettingsUtil.String(DefaultKeys.DataServiceAddressIpV4, "0.0.0.0");

        [ObservableProperty]
        string dataServicePort = SettingsUtil.Int(DefaultKeys.DataServiceListenPort, 9819).ToString();

        [ObservableProperty]
        string groupIdentity = SettingsUtil.Int(DefaultKeys.GroupIdentity, 0).ToString();
    }
}
