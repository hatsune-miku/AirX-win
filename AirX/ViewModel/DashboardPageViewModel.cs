using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirX
{
    partial class DashboardPageViewModel : ObservableObject
    {
        [ObservableProperty]
        string airXVersion;

        [ObservableProperty]
        bool isConnected = false;

        [ObservableProperty]
        string message;

        [ObservableProperty]
        string connectMessage;
    }
}
