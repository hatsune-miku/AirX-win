using AirX.Data;
using AirX.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirX.ViewModel
{
    public partial class ReceiveFilesPageViewModel : ObservableObject
    {
        [ObservableProperty]
        public List<ReceiveFile> receiveFiles;

        [ObservableProperty]
        public bool noReceiveFiles = true;
    }
}
