// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using AirX.Bridge;
using AirX.Extension;
using AirX.Model;
using AirX.Util;
using AirX.ViewModel;
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
        private static AirXBridge.OnTextReceivedHandler TextHandler = OnTextReceived;
        private static AirXBridge.OnFileSendingHandler FileSendingHandler = OnFileSending;
        private static AirXBridge.OnFilePartHandler FilePartHandler = OnFilePart;
        private static AirXBridge.OnFileComingHandler FileComingHandler = OnFileComing;

        private static SynchronizationContext context;

        public static TrayIconHolderWindow Instance { get; private set; }


        public TrayIconHolderWindow()
        {
            this.InitializeComponent();

            context = SynchronizationContext.Current;
            Instance = this;

            TrySignInAsync().LogOnError();
            AirXBridge.TryStartAirXService();
            AirXBridge.SetOnTextReceivedHandler(TextHandler);
            AirXBridge.SetOnFileComingHandler(FileComingHandler);
            AirXBridge.SetOnFileSendingHandler(FileSendingHandler);
            AirXBridge.SetOnFilePartHandler(FilePartHandler);

            AppWindow.Resize(new(1, 1));
            AppWindow.Move(new(32768, 32768));
        }

        private async Task TrySignInAsync()
        {
            if (!await AccountUtil.TryAutomaticLogin())
            {
                var window = new LoginWindow();
                window.Activate();
                return;
            }
            NotificationUtil.ShowNotification(
                "Welcome back, " + GlobalViewModel.Instance.LoggingGreetingsName + "!");
        }

        private static void OnFilePart(byte fileId, UInt64 offset, UInt64 length, byte[] data)
        {
            ReceiveFile receiveFile;
            try
            {
                receiveFile = GlobalViewModel.Instance.ReceiveFiles[fileId];
            }
            catch (Exception)
            {
                Debug.WriteLine("Unexpected file incoming: fileid=" + fileId + ", length=" + length);
                return;
            }

            // User cancelled the file?
            if (receiveFile.Status == AirXBridge.FileStatus.CancelledByReceiver)
            {
                Debug.WriteLine("File cancelled!");
                GlobalViewModel.Instance.ReceiveFiles.Remove(fileId);
                // TODO: Send cancel packet to sender
                return;
            }

            receiveFile.Status = AirXBridge.FileStatus.InProgress;
            Debug.WriteLine($"File part received: offset={offset}, length={length}");
            if (receiveFile.WritingStream == null)
            {
                receiveFile.Status = AirXBridge.FileStatus.Error;
                Debug.WriteLine("File not accepted!");
                return;
            }

            receiveFile.WritingStream.Seek((long) offset, SeekOrigin.Begin);
            receiveFile.WritingStream.Write(data, 0, (int)length);
            receiveFile.Progress += length;
            
            Debug.WriteLine($"File progress: {receiveFile.Progress}/{receiveFile.TotalSize}");
            if (receiveFile.Progress == receiveFile.TotalSize)
            {
                receiveFile.WritingStream.Close();
                receiveFile.Status = AirXBridge.FileStatus.Completed;
                Debug.WriteLine("File recv completed!");
            }
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
                    $"File {fileName} from {from} is coming! Receive?",
                    "Receive!",
                    "Decline!"
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
                }, TaskScheduler.Default).LogOnError();
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
                Filename = fileName,
                WritingStream = writingFileStream,
                FullPath = fullPath,
                Progress = 0,
                TotalSize = fileSize,
                FileId = 1,
                Status = AirXBridge.FileStatus.Accepted,
                From = Peer.Parse(from),
            };

            // Preallocate file size
            writingFileStream.SetLength((long)fileSize);

            // Enqueue
            GlobalViewModel.Instance.ReceiveFiles.TryAdd(fileId, transferFile);

            // Open window
            context.Post((_) =>
            {
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
