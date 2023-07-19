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
        private ReceiveFilesPageViewModel ViewModel = new();

        public ReceivedFilesPage()
        {
            this.InitializeComponent();
        }

        private void OnPageLoading(FrameworkElement sender, object args)
        {
            // 页面加载时，从GlobalViewModel中获取对应的ReceivingFile
            // 然后监听ReceiveFiles的MapChanged事件，当有新的ReceivingFile时，刷新页面
            GlobalViewModel.Instance.ReceiveFiles.MapChanged += OnMapChanged;
            RefreshView();
        }

        private void OnMapChanged(IObservableMap<int, NewFileViewModel> sender, IMapChangedEventArgs<int> @event)
        {
            // 当有新的ReceivingFile时，刷新页面
            RefreshView();
        }

        private void RefreshView()
        {
            ViewModel.ReceiveFiles = GlobalViewModel.Instance.ReceiveFiles.Values
                .Select(item => item.ReceivingFile)
                .ToList();
        }
    }
}
