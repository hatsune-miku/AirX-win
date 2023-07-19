using AirX.ViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using System;
using System.IO;
using System.Security.Policy;

namespace AirX.Model
{
    /// <summary>
    /// 正接收中的文件的数据模型
    /// ObservableObject`让使用这个类的SwiftUI的控件能够及时感知到值的变化、从而更新UI
    /// 参见 MVVM模式
    /// </summary>
    public partial class ReceiveFile : ObservableObject
    {
        public string RemoteFullPath { get; set; }          /** 这个文件在发送端的绝对路径 */
        public FileStream WritingStream { get; set; }       /** 本地存入文件的文件句柄，用于写入数据 */
        public String LocalSaveFullPath { get; set; }       /** 本地存入文件的绝对路径 */
        public ulong TotalSize { get; set; }                /** 文件总字节数 */
        public int FileId { get; set; }                     /** 前端分配的FileId，确保在前端这里是唯一的即可 */
        public Peer From { get; set; }                      /** 发送者 */
        public ulong Progress { get; set; }                 /** 已经传输了多少字节 */
        public AirXBridge.FileStatus Status { get; set; }   /** 传输状态 */

        // 用于显示的进度数据，每当progress更新数次后，让displayProgress更新一次
        // 这样做是为了减少UI更新的次数，提高性能
        [ObservableProperty]
        public ulong displayProgress;

        // 同上，用于显示的状态数据
        [ObservableProperty]
        public AirXBridge.FileStatus displayStatus;

        // 一个sample，用于测试
        public static readonly ReceiveFile Sample = new()
        {
            RemoteFullPath = "sample.pdf",
            WritingStream = null,
            LocalSaveFullPath = "",
            Progress = (ulong) (100 * 1024 * 0.65),
            TotalSize = 100 * 1024,
            FileId = -1,
            Status = AirXBridge.FileStatus.InProgress,
            From = Peer.Sample,
            DisplayProgress = (ulong)(100 * 1024 * 0.65),
            DisplayStatus = AirXBridge.FileStatus.InProgress,
        };

        // 当点击了取消按钮后，会触发这个函数
        public void OnCancelAndDelete(object sender, RoutedEventArgs e)
        {
            // 更改文件状态为“被接收端取消”
            Status = AirXBridge.FileStatus.CancelledByReceiver;

            // 触发UI更新
            GlobalViewModel.Instance.ReceiveFiles.TriggerNotifyChanged();
        }
    }
}
