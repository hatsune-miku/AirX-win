using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirX.ViewModel
{
    public partial class AboutViewModel : ObservableObject
    {
        [ObservableProperty]
        string airXVersion;

        [ObservableProperty]
        string buildValue;

        [ObservableProperty]
        string versionValue;

        [ObservableProperty]
        string copyright;
    }
}
