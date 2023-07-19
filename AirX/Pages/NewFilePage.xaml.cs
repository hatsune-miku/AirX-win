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

        // NewFilePage持有一个对自身窗口的引用，用于关闭窗口
        private NewFileWindow _instance;

        // 每个NewFilePage都有一个FileId，用于在GlobalViewModel中找到对应的ReceivingFile
        public int FileId { get; set; } = 0;

        public NewFilePage()
        {
            this.InitializeComponent();
        }

        private void OnPageLoading(FrameworkElement sender, object args)
        {
            // 页面加载时，从GlobalViewModel中获取对应的ReceivingFile
            ViewModel = GlobalViewModel.Instance.ReceiveFiles[FileId];
            DataContext = ViewModel;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
        }

        // 开放给外部使用，用于设置NewFilePage持有的窗口实例
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
            catch (Exception)
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
            catch (Exception)
            {
                return "<error>";
            }
        }

        private async Task HandleStopAsync()
        {
            var result = await UIUtil.ShowContentDialogYesNoAsync(
                "Stop", "Are you sure to stop receiving this file?", "Stop", "Don't Stop", Content.XamlRoot);

            if (result == ContentDialogResult.Primary)
            {
                ViewModel.ReceivingFile.Status = AirXBridge.FileStatus.CancelledByReceiver;
                _instance?.Close();
            }
        }

        private void OnStopOrOpenFolderClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel.ReceivingFile.Status == AirXBridge.FileStatus.Completed)
            {
                FileUtil.OpenFolderInExplorer(ViewModel.ReceivingFile.LocalSaveFullPath);
                _instance?.Close();
            }
            else
            {
                HandleStopAsync().FireAndForget();
            }
        }


        private void OnBlockClicked(object sender, RoutedEventArgs e)
        {
            var target = ViewModel.ReceivingFile.From.ToString();
            new ContentDialog()
            {
                Title = "Blocking " + target,
                Content = "Are you sure you want to block (" + target + ") and stop receiving everything from them?",
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
                catch (Exception) { }
            }, TaskScheduler.Default).FireAndForget();
        }
    }
}
