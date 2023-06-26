using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirX.ViewModel
{
    partial class MessageBoxViewModel : ObservableObject
    {
        [ObservableProperty]
        public string title = "";

        [ObservableProperty]
        public string message = "";

        [ObservableProperty]
        public string buttonTitle = "OK";
    }
}
