using AirX.Bridge;
using AirX.Extension;
using AirX.Model;
using AirX.Util;
using AirX.ViewModel;
using AirX.Worker;
using Microsoft.UI.Xaml;
using SRCounter;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WinRT.Interop;

namespace AirX.View
{
    /// <summary>
    /// 处理所有后台任务的类
    /// </summary>
    public sealed partial class TrayIconHolderWindow : Window
    {
        /// <summary>
        /// 用于UI线程运行代码
        /// </summary>
        private static SynchronizationContext context;

        /// <summary>
        /// 用于写入文件数据的worker
        /// </summary>
        private static FilePartWorker filePartWorker = new();

        /// <summary>
        /// 本Page所属窗口的实例
        /// </summary>
        public static TrayIconHolderWindow Instance { get; private set; }

        /// <summary>
        /// 程序运行的时候
        /// </summary>
        public TrayIconHolderWindow()
        {
            this.InitializeComponent();

            context = SynchronizationContext.Current;
            Instance = this;

            /// 尝试登陆
            TrySignInAsync().FireAndForget();

            /// 启动AirX服务
            AirXBridge.TryStartAirXService();

            /// 设置回调函数
            AirXBridge.SetOnTextReceivedHandler(OnTextReceived);
            AirXBridge.SetOnFileComingHandler(OnFileComing);
            AirXBridge.SetOnFileSendingHandler(OnFileSending);
            AirXBridge.SetOnFilePartHandler(OnFilePart);

            // Hide the window visually.
            /// 把窗口远远移动到窗口之外，并设置成1x1像素大小，是隐藏窗口而不将其关闭的一种常见办法
            AppWindow.Resize(new(1, 1));
            AppWindow.Move(new(32768, 32768));

            // Start workers.
            filePartWorker.Start();
        }

        private async Task TrySignInAsync()
        {
            if (!await AccountUtil.TryAutomaticLoginAsync())
            {
                // Prompt to login if token expired.
                var window = new LoginWindow();
                window.Activate();
                return;
            }

            var displayName = GlobalViewModel.Instance.LoggingGreetingsName;
            NotificationUtil.ShowNotification(
                $"Welcome back, {displayName}!");
        }

        // Called when a FilePartPacket is received.
        // In most cases, multiple calls will be made for a single file.
        // Return true to stop receiving the file.
        private static bool OnFilePart(byte fileId, UInt64 offset, UInt64 length, byte[] data)
        {
            try
            {
                NewFileViewModel remoteViewModel = GlobalViewModel.Instance.ReceiveFiles[fileId];
                var file = remoteViewModel.ReceivingFile;

                /// 判断是否来了个不存在的fileId？
                /// 或者用户是不是已经取消掉这个文件了
                if (file == null || file.Status == AirXBridge.FileStatus.CancelledByReceiver || file.Status == AirXBridge.FileStatus.CancelledBySender)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return true;
            }

            /// 提交workload给worker，让他在后台把文件写入
            filePartWorker.PostWorkload(new(fileId, offset, length, data));

            /// 不要断开连接
            return false;
        }

        /// 每当发出去的文件又传了一点点的时候，调用一次，报告最新progress
        private static void OnFileSending(byte fileId, ulong progress, ulong total, AirXBridge.FileStatus status)
        {
            switch (status)
            {
                case AirXBridge.FileStatus.Rejected:
                    {
                        Debug.WriteLine("File rejected!");
                        break;
                    }
                case AirXBridge.FileStatus.Accepted:
                    {
                        Debug.WriteLine("File accepted!");
                        break;
                    }
                case AirXBridge.FileStatus.Completed:
                    {
                        Debug.WriteLine("File completed!");
                        break;
                    }
                case AirXBridge.FileStatus.Error:
                    {
                        Debug.WriteLine("File error!");
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        /// 每当别人试图发给我一个文件（还没发呢），调用一次
        /// 这时候，由用户决定要不要收。
        private static void OnFileComing(ulong fileSize, string fileName, string from)
        {
            context.Post((_) =>
            {
                /// UI线程弹窗询问用户：要接受吗？
                UIUtil.MessageBoxAsync(
                    "Received File",
                    $"File {fileName} from {from} ({FileUtil.GetFileSizeDescription(fileSize)}) is coming! Receive?",
                    "Receive",
                    "Decline"
                ).ContinueWith(t =>
                {
                    bool accept = t.Result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary;
                    byte fileId = FileUtil.NextFileId();

                    /// 如果accept，准备接收这个文件，进行准备工作
                    if (accept)
                    {
                        PrepareForReceiveFile(fileId, fileSize, fileName, from);
                    }

                    /// 把答复告知发送方
                    AirXBridge.RespondToFile(
                        Peer.Parse(from),
                        fileId,
                        fileSize,
                        fileName,
                        accept
                    );
                }, TaskScheduler.Default).FireAndForget();
            }, null);
        }

        /// 每当新文本来了的时候，调用一次
        private static void OnTextReceived(string text, string source)
        {
            if (AccountUtil.IsInBlockList(source))
            {
                return;
            }

            context.Post(_ =>
            {
                try
                {
                    /// 弹出新文本窗口
                    var window = NewTextWindow.Create(text, source);
                    window.Activate();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }, null);
        }

        /// 准备接受一个文件的准备工作
        private static void PrepareForReceiveFile(byte fileId, ulong fileSize, string fileName, string from)
        {
            /// 确定要存在哪个文件夹：默认是 `下载/AirxFiles`
            /// 这是通过先获得`下载`这一目录的URL（URL不一定非得是网址，本地文件也可以是URL）
            /// 然后拼接上AirXFiles而成
            var savingFilename = FileUtil.GetFileName(fileName);
            var fullPath = Path.Join(SettingsUtil.String(Keys.SaveFilePath, ""), savingFilename);
            var directoryPath = Path.GetDirectoryName(fullPath);

            /// 目标目录（`下载/AirXFiles`）是否还不存在？
            if (!Directory.Exists(directoryPath))
            {
                /// 若不存在，就创建它
                Directory.CreateDirectory(directoryPath);
            }

            /// 目标文件是否还不存在？不存在就创建它，然后打开；存在的话直接打开。
            var writingFileStream = File.Create(fullPath);

            /// 构造ReceiveFile对象，现在我们有一个待收文件了
            var transferFile = new ReceiveFile
            {
                RemoteFullPath = fileName,
                WritingStream = writingFileStream,
                LocalSaveFullPath = fullPath,
                Progress = 0,
                TotalSize = fileSize,
                FileId = 1,
                Status = AirXBridge.FileStatus.Accepted,
                From = Peer.Parse(from),
            };

            /// 提前把硬盘空间给占上，steam同款做法
            writingFileStream.SetLength((long)fileSize);

            // Enqueue
            // Open window
            context.Post((_) =>
            {
                /// 把待收文件放进全局状态里
                GlobalViewModel.Instance.ReceiveFiles.TryAdd(fileId, new(transferFile));

                /// 同时弹出新文件窗口
                var window = new NewFileWindow(fileId);
                window.Activate();
            }, null);

            /// 还记得之前respondToFile吗？现在文件传输已经开始了，onFilePart函数将会开始被频繁调用。
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            IntPtr hwnd = WindowNative.GetWindowHandle(this);
            PInvoke.User32.ShowWindow(hwnd, PInvoke.User32.WindowShowStyle.SW_HIDE);
        }
    }
}
