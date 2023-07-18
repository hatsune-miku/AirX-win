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
    public sealed partial class TrayIconHolderWindow : Window
    {
        private static SynchronizationContext context;
        private static FilePartWorker filePartWorker = new();

        public static TrayIconHolderWindow Instance { get; private set; }


        public TrayIconHolderWindow()
        {
            this.InitializeComponent();

            context = SynchronizationContext.Current;
            Instance = this;

            TrySignInAsync().FireAndForget();
            AirXBridge.TryStartAirXService();
            AirXBridge.SetOnTextReceivedHandler(OnTextReceived);
            AirXBridge.SetOnFileComingHandler(OnFileComing);
            AirXBridge.SetOnFileSendingHandler(OnFileSending);
            AirXBridge.SetOnFilePartHandler(OnFilePart);

            // Hide the window visually.
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
                if (file == null || file.Status == AirXBridge.FileStatus.CancelledByReceiver || file.Status == AirXBridge.FileStatus.CancelledBySender)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return true;
            }

            filePartWorker.PostWorkload(new(fileId, offset, length, data));
            return false;
        }

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

        private static void OnFileComing(ulong fileSize, string fileName, string from)
        {
            context.Post((_) =>
            {
                UIUtil.MessageBoxAsync(
                    "Received File",
                    $"File {fileName} from {from} ({FileUtil.GetFileSizeDescription(fileSize)}) is coming! Receive?",
                    "Receive",
                    "Decline"
                ).ContinueWith(t =>
                {
                    bool accept = t.Result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary;
                    byte fileId = FileUtil.NextFileId();
                    if (accept)
                    {
                        PrepareForReceiveFile(fileId, fileSize, fileName, from);
                    }
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
                    var window = NewTextWindow.Create(text, source);
                    window.Activate();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }, null);
        }

        private static void PrepareForReceiveFile(byte fileId, ulong fileSize, string fileName, string from)
        {
            var savingFilename = FileUtil.GetFileName(fileName);
            var fullPath = Path.Join(SettingsUtil.String(Keys.SaveFilePath, ""), savingFilename);
            var directoryPath = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            var writingFileStream = File.Create(fullPath);

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

            // Preallocate file size
            writingFileStream.SetLength((long)fileSize);

            // Enqueue
            // Open window
            context.Post((_) =>
            {
                GlobalViewModel.Instance.ReceiveFiles.TryAdd(fileId, new(transferFile));

                var window = new NewFileWindow(fileId);
                window.Activate();
            }, null);
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            IntPtr hwnd = WindowNative.GetWindowHandle(this);
            PInvoke.User32.ShowWindow(hwnd, PInvoke.User32.WindowShowStyle.SW_HIDE);
        }
    }
}
