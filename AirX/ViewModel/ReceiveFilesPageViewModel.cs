using AirX.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

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
