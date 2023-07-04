using AirX.Extension;
using AirX.Util;
using AirX.View;
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
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace AirX.Pages
{
    public sealed partial class NewFilePage : Page
    {
        private NewFileViewModel ViewModel;
        private GlobalViewModel GlobalViewModel = GlobalViewModel.Instance;

        private NewFileWindow _instance;

        public int FileId { get; set; } = 0;

        public NewFilePage()
        {
            this.InitializeComponent();
            ViewModel = GlobalViewModel.Instance.ReceiveFiles[FileId];
            DataContext = ViewModel;
        }

        public void SetWindowInstance(NewFileWindow instance)
        {
            this._instance = instance;
        }

        public string GetFileSizeDescription()
        {
            ulong sizeInBytes;
            try
            {
                sizeInBytes = ViewModel.ReceivingFile.TotalSize;
                return FileUtil.GetFileSizeDescription(sizeInBytes);
            }
            catch
            {
                return "<error>";
            }
        }

        public string GetFrom()
        {
            try
            {
                return "File from " + ViewModel.ReceivingFile.From.ToString();
            }
            catch
            {
                return "<error>";
            }
        }

        private void OnStopClicked(object sender, RoutedEventArgs e)
        {
            ViewModel.ReceivingFile.Status = AirXBridge.FileStatus.CancelledByReceiver;
            _instance?.Close();
        }


        private void OnBlockClicked(object sender, RoutedEventArgs e)
        {
            var target = ViewModel.ReceivingFile.From.ToString();
            new ContentDialog()
            {
                Title = "Blocking " + target,
                Content = "Are you sure to block " + target + "?",
                PrimaryButtonText = "Block",
                SecondaryButtonText = "Cancel",
                XamlRoot = Content.XamlRoot,
            }.ShowAsync().AsTask().ContinueWith(t =>
            {
                try
                {
                    if (t.Result == ContentDialogResult.Primary)
                    {
                        AccountUtil.AddToBlockList(target);
                    }
                }
                catch { }
            }, TaskScheduler.Default).FireAndForget();
        }
    }
}
