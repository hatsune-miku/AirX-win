using AirX.Model;
using AirX.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace AirX.Pages
{
    public sealed partial class ReceivedFilesPage : Page
    {
        private List<ReceiveFile> ReceiveFiles;

        public ReceivedFilesPage()
        {
            this.InitializeComponent();
        }

        private void OnPageLoading(FrameworkElement sender, object args)
        {
            ReceiveFiles = GlobalViewModel.Instance.ReceiveFiles.Values
                .Select(item => item.ReceivingFile)
                .ToList();
        }
    }
}
