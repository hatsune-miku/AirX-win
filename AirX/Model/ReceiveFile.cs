using AirX.Bridge;
using AirX.ViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Lombok.NET;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirX.Model
{
    public partial class ReceiveFile : ObservableObject
    {
        public string Filename { get; set; }
        public FileStream WritingStream { get; set; }
        public String FullPath { get; set; }
        public ulong TotalSize { get; set; }
        public int FileId { get; set; }
        public Peer From { get; set; }
        public ulong Progress { get; set; }
        public AirXBridge.FileStatus Status { get; set; }

        [ObservableProperty]
        public ulong displayProgress;

        [ObservableProperty]
        public AirXBridge.FileStatus displayStatus;

        public static readonly ReceiveFile Sample = new()
        {
            Filename = "sample.pdf",
            WritingStream = null,
            FullPath = "",
            Progress = (ulong) (100 * 1024 * 0.65),
            TotalSize = 100 * 1024,
            FileId = -1,
            Status = AirXBridge.FileStatus.InProgress,
            From = Peer.Sample,
            DisplayProgress = (ulong)(100 * 1024 * 0.65),
            DisplayStatus = AirXBridge.FileStatus.InProgress,
        };

        public void OnCancelAndDelete(object sender, RoutedEventArgs e)
        {
            Status = AirXBridge.FileStatus.CancelledByReceiver;
            GlobalViewModel.Instance.ReceiveFiles.TriggerNotifyChanged();
        }
    }
}
